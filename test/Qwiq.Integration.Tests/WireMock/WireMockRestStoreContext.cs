using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Qwiq.Client.Rest;
using System;
using WireMock.Logging;
using WireMock.Server;
using WireMock.Settings;

namespace Qwiq.WireMock
{
    /// <summary>
    /// Provides a WireMock-based REST store context for testing the REST client implementation
    /// without requiring live Azure DevOps connectivity.
    /// </summary>
    /// <remarks>
    /// This context creates a WireMock HTTP server and configures a VssConnection to use it.
    /// Tests can configure mock responses using the <see cref="Server"/> property.
    ///
    /// The WireMock server intercepts HTTP traffic, allowing full coverage of the REST client
    /// implementation including serialization, HTTP handling, and error scenarios.
    ///
    /// HTTP (not HTTPS) is used because:
    /// 1. VssBasicCredential requires HTTPS and VssConnection's internal HttpClient does NOT honor
    ///    ServicePointManager.ServerCertificateValidationCallback - SSL bypass cannot be reliably achieved
    /// 2. VssCredentials() with no authentication allows HTTP connections for on-premises scenarios
    /// 3. WireMock HTTPS requires SSL certificate bindings that may fail on CI runners without elevated privileges
    /// </remarks>
    public class WireMockRestStoreContext : IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Gets the WireMock server instance for configuring mock responses.
        /// </summary>
        public WireMockServer Server { get; }

        /// <summary>
        /// Gets the base URL of the WireMock server (HTTP).
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
        /// Creates a new WireMock-based REST store context with HTTP.
        /// </summary>
        /// <remarks>
        /// IMPORTANT: Configure mock responses using the Server property BEFORE
        /// accessing WorkItemTrackingClient or calling CreateWorkItemStore().
        /// The VssConnection.GetClient&lt;T&gt;() call requires the /_apis/connectionData
        /// endpoint to be mocked first.
        /// </remarks>
        public WireMockRestStoreContext()
        {
            // Start WireMock server with HTTP (not HTTPS)
            // We use HTTP because:
            // 1. VssBasicCredential requires HTTPS and we cannot reliably bypass SSL validation
            //    (VssConnection's HttpClient ignores ServicePointManager callbacks)
            // 2. VssCredentials() allows HTTP connections for on-premises/anonymous scenarios
            // 3. WireMock HTTPS requires SSL certificate bindings that fail on CI runners
            // Start WireMock with explicit settings for reliability
            // Using 127.0.0.1 binding and explicit configuration helps avoid platform issues
            var settings = new WireMockServerSettings
            {
                // Let WireMock pick a free port but bind to specific interface
                Urls = new[] { "http://127.0.0.1:0" },
                StartAdminInterface = true,
                AllowPartialMapping = true
            };
            Server = WireMockServer.Start(settings);

            System.Diagnostics.Trace.WriteLine($"[WireMock] Server.IsStarted: {Server.IsStarted}");

            // Use VssCredentials with no authentication - simulates on-premises anonymous access
            // This allows HTTP connections (unlike VssBasicCredential which requires HTTPS)
            var credentials = new VssCredentials();

            // Log the URL we're connecting to
            System.Diagnostics.Trace.WriteLine($"[WireMock] Server URL: {BaseUrl}");
            System.Diagnostics.Trace.WriteLine($"[WireMock] Server.Url: {Server.Url}");
            System.Diagnostics.Trace.WriteLine($"[WireMock] Server.Urls: {string.Join(", ", Server.Urls ?? Array.Empty<string>())}");
            System.Diagnostics.Trace.WriteLine($"[WireMock] Server port: {Server.Port}");

            // Verify WireMock server is actually responding using polling with retries
            const int maxRetries = 10;
            const int delayMs = 100;
            bool serverReady = false;

            using (var testClient = new System.Net.Http.HttpClient())
            {
                testClient.Timeout = TimeSpan.FromSeconds(2);

                for (int retry = 0; retry < maxRetries && !serverReady; retry++)
                {
                    try
                    {
                        var testResponse = testClient.GetAsync($"{BaseUrl}/__admin/mappings").Result;
                        if (testResponse.IsSuccessStatusCode)
                        {
                            serverReady = true;
                            System.Diagnostics.Trace.WriteLine($"[WireMock] Server ready after {retry + 1} attempt(s)");
                        }
                    }
                    catch
                    {
                        // Server not ready yet, wait and retry
                        System.Threading.Thread.Sleep(delayMs);
                    }
                }
            }

            if (!serverReady)
            {
                Server?.Stop();
                Server?.Dispose();
                throw new InvalidOperationException($"WireMock server at {BaseUrl} failed to respond after {maxRetries} attempts");
            }

            // Create connection with settings that work for local testing
            Connection = new VssConnection(new Uri(BaseUrl), credentials);
            Connection.Settings.BypassProxyOnLocal = true;
            Connection.Settings.CompressionEnabled = false; // Easier debugging
            Connection.Settings.SendTimeout = TimeSpan.FromSeconds(5); // Reasonable timeout for WireMock tests

            System.Diagnostics.Trace.WriteLine($"[WireMock] VssConnection created for URI: {Connection.Uri}");

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
                // Log all requests for debugging
                if (Server != null)
                {
                    System.Diagnostics.Trace.WriteLine($"[WireMock] Total mappings: {Server.Mappings.Count()}");
                    System.Diagnostics.Trace.WriteLine("[WireMock] ===== ALL RECEIVED REQUESTS =====");
                    foreach (var entry in Server.LogEntries)
                    {
                        // Check if request was matched by looking at MappingGuid
                        var matched = entry.MappingGuid != null && entry.MappingGuid != Guid.Empty;
                        var status = matched ? "MATCHED" : "UNMATCHED";
                        System.Diagnostics.Trace.WriteLine($"[WireMock] [{status}] {entry.RequestMessage.Method} {entry.RequestMessage.Url}");

                        if (!matched && entry.RequestMessage.Headers != null)
                        {
                            System.Diagnostics.Trace.WriteLine($"[WireMock]   Headers:");
                            foreach (var header in entry.RequestMessage.Headers)
                            {
                                System.Diagnostics.Trace.WriteLine($"[WireMock]     {header.Key}: {string.Join(", ", header.Value)}");
                            }
                        }
                    }
                    System.Diagnostics.Trace.WriteLine("[WireMock] ===== END REQUESTS =====");
                }

                _workItemTrackingClient?.Dispose();
                Connection?.Dispose();
                Server?.Stop();
                Server?.Dispose();
            }

            _disposed = true;
        }
    }
}
