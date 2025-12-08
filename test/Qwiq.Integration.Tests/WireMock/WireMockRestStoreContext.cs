using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Qwiq.Client.Rest;
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using WireMock.Server;
using WireMock.Settings;

namespace Qwiq.WireMock
{
    /// <summary>
    /// Provides a WireMock-based REST store context for testing the REST client implementation
    /// without requiring live Azure DevOps connectivity.
    /// </summary>
    /// <remarks>
    /// This context creates a WireMock server with HTTPS and configures a VssConnection to use it.
    /// Tests can configure mock responses using the <see cref="Server"/> property.
    ///
    /// The WireMock server intercepts HTTP traffic, allowing full coverage of the REST client
    /// implementation including serialization, HTTP handling, and error scenarios.
    ///
    /// Note: This context temporarily bypasses SSL certificate validation for the WireMock
    /// self-signed certificate. This is acceptable for testing purposes only.
    /// </remarks>
    public class WireMockRestStoreContext : IDisposable
    {
        private bool _disposed;
        private readonly RemoteCertificateValidationCallback? _originalCallback;

        /// <summary>
        /// Gets the WireMock server instance for configuring mock responses.
        /// </summary>
        public WireMockServer Server { get; }

        /// <summary>
        /// Gets the base URL of the WireMock server (HTTPS).
        /// </summary>
        public string BaseUrl => Server.Url!;

        /// <summary>
        /// Gets the VssConnection configured to use the WireMock server.
        /// </summary>
        public VssConnection Connection { get; }

        /// <summary>
        /// Gets the WorkItemTrackingHttpClient configured to use the WireMock server.
        /// This is lazily initialized when first accessed to allow mock responses to be configured first.
        /// </summary>
        public WorkItemTrackingHttpClient WorkItemTrackingClient => _workItemTrackingClient ??= Connection.GetClient<WorkItemTrackingHttpClient>();
        private WorkItemTrackingHttpClient? _workItemTrackingClient;

        /// <summary>
        /// Creates a new WireMock-based REST store context with HTTPS.
        /// </summary>
        /// <remarks>
        /// WireMock uses a self-signed certificate for HTTPS. This context temporarily
        /// bypasses certificate validation during testing.
        ///
        /// IMPORTANT: Configure mock responses using the Server property BEFORE
        /// accessing WorkItemTrackingClient or calling CreateWorkItemStore().
        /// The VssConnection.GetClient&lt;T&gt;() call requires the connection data
        /// endpoint to be mocked first.
        /// </remarks>
        public WireMockRestStoreContext()
        {
            // Save original certificate validation callback
            _originalCallback = ServicePointManager.ServerCertificateValidationCallback;

            // Bypass SSL certificate validation for WireMock's self-signed cert
            // This is safe for testing purposes only
#pragma warning disable CA5359 // Intentionally accepting all certificates for WireMock testing
            ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertificates;
#pragma warning restore CA5359

            // Start WireMock server with HTTPS (required for VssBasicCredential)
            var settings = new WireMockServerSettings
            {
                UseSSL = true,
                Port = null // Use random available port
            };
            Server = WireMockServer.Start(settings);

            // Create VssConnection pointing to WireMock with HTTPS
            // Use basic credentials (empty) - WireMock doesn't validate auth
            var credentials = new VssBasicCredential(string.Empty, string.Empty);

            // Create connection with settings that work for local testing
            Connection = new VssConnection(new Uri(BaseUrl), credentials);
            Connection.Settings.BypassProxyOnLocal = true;
            Connection.Settings.CompressionEnabled = false; // Easier debugging

            // NOTE: Do NOT call Connection.GetClient<T>() here!
            // The VssConnection requires the /_apis/connectionData endpoint to be mocked first.
            // The WorkItemTrackingClient property is lazily initialized when first accessed.
        }

        /// <summary>
        /// Creates an IWorkItemStore using the WireMock-backed connection.
        /// </summary>
        /// <returns>A work item store that sends requests to WireMock.</returns>
        public IWorkItemStore CreateWorkItemStore()
        {
            // Use the internal constructor that accepts a custom client factory
            var tpcProxy = Connection.AsProxy();
            if (tpcProxy == null)
            {
                throw new InvalidOperationException("Failed to create TPC proxy from VssConnection.");
            }

            // Cast to IInternalTeamProjectCollection which provides GetClient<T>
            var internalTpc = (IInternalTeamProjectCollection)tpcProxy;

            return new Client.Rest.WorkItemStore(
                () => internalTpc,
                () => WorkItemTrackingClient,
                QueryFactory.GetInstance);
        }

        private static bool AcceptAllCertificates(
            object sender,
            X509Certificate? certificate,
            X509Chain? chain,
            SslPolicyErrors sslPolicyErrors)
        {
            // Accept all certificates for WireMock testing
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Restore original certificate validation callback
                ServicePointManager.ServerCertificateValidationCallback = _originalCallback;

                _workItemTrackingClient?.Dispose();
                Connection?.Dispose();
                Server?.Stop();
                Server?.Dispose();
            }

            _disposed = true;
        }
    }
}
