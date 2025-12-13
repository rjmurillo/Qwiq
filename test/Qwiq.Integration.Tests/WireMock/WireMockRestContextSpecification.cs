using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qwiq.Tests.Common;
using System;
using WireMock.Server;

namespace Qwiq.WireMock
{
    /// <summary>
    /// Base class for REST client tests that use WireMock for HTTP mocking.
    /// </summary>
    /// <remarks>
    /// This base class provides a WireMock server and helper methods for configuring
    /// mock responses. Tests derived from this class exercise the full REST client
    /// implementation without requiring live Azure DevOps connectivity.
    ///
    /// Use this for:
    /// - Unit testing REST client behavior
    /// - Testing error handling scenarios
    /// - Testing edge cases that are hard to reproduce with real data
    /// - Fast, isolated tests that don't require network access
    ///
    /// For integration tests against real Azure DevOps, use <see cref="TimedContextSpecification"/>
    /// with <see cref="IntegrationSettings.CreateRestStore"/>.
    ///
    /// Note: WireMock uses HTTP (not HTTPS) because VssConnection ignores all SSL certificate
    /// validation bypass attempts. Using HTTP with VssCredentials() allows the mock server
    /// to work without requiring SSL certificate configuration.
    /// </remarks>
    /// <seealso href="https://github.com/WireMock-Net/WireMock.Net/issues/1387">GitHub Issue #1387 - TaskCanceledException on Windows</seealso>
    [TestCategory("WireMock")]
    [Ignore("WireMock.Net OWIN hosting deadlocks in test host processes on .NET Framework 4.7.2. Server binds port but never processes requests. See issues #393, #470, #577, #1387.")]
#pragma warning disable CA1001 // Disposable field '_context' is disposed in Cleanup() method
    public abstract class WireMockRestContextSpecification : TimedContextSpecification
#pragma warning restore CA1001
    {
        private WireMockRestStoreContext? _context;

        /// <summary>
        /// Gets the WireMock server for configuring mock responses.
        /// </summary>
        protected WireMockServer Server => _context?.Server ?? throw new InvalidOperationException("Context not initialized. Call base.Given() first.");

        /// <summary>
        /// Gets the work item store backed by WireMock.
        /// </summary>
        protected IWorkItemStore? Store { get; private set; }

        /// <summary>
        /// Gets the base URL of the WireMock server.
        /// </summary>
        protected string BaseUrl => _context?.BaseUrl ?? throw new InvalidOperationException("Context not initialized.");

        /// <summary>
        /// Initializes the WireMock context using real captured Azure DevOps API responses.
        /// </summary>
        public override void Given()
        {
            _context = new WireMockRestStoreContext();

            // Log the WireMock server URL for debugging
            System.Diagnostics.Trace.WriteLine($"WireMock server started at: {Server.Url}");

            // Load real Azure DevOps API responses from captured stubs FIRST
            // This includes projects, WIQL queries, work items, and work item types
            var stubsPath = AzureDevOpsWireMockExtensions.GetDefaultStubsFilePath();
            Server.LoadStubsFromFile(stubsPath);

            // Setup VssConnection handshake endpoints LAST to override any stubs
            // WireMock uses "last registered wins" for matching mappings
            // The JSON stubs have /_apis/connectionData.* regex which would override handshake
            // By registering handshake AFTER, we ensure correct connectionData and resourceAreas responses
            Server.SetupVssConnectionHandshake();

            // Log all registered mappings for debugging
            var mappings = Server.Mappings;
            System.Diagnostics.Trace.WriteLine($"WireMock has {mappings.Count()} registered mappings:");
            foreach (var mapping in mappings)
            {
                System.Diagnostics.Trace.WriteLine($"  - {mapping.Title ?? mapping.Guid.ToString()}");
            }

            // Create the store after stubs are loaded
            Store = TimedAction(() => _context.CreateWorkItemStore(), "WireMock", "Create WorkItemStore");
        }

        public override void Cleanup()
        {
            (Store as IDisposable)?.Dispose();
            _context?.Dispose();
            base.Cleanup();
        }
    }
}
