using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using WireMock.Server;
using WireMock.Settings;
using WireMockLogging = global::WireMock.Logging;

namespace Qwiq.WireMock
{
    /// <summary>
    /// Diagnostic tests to verify WireMock.Net is working correctly on this machine.
    /// Tests different configurations to identify what works.
    /// </summary>
    /// <remarks>
    /// These tests are for diagnosing WireMock.Net platform issues.
    /// Run manually with: dotnet test --filter "FullyQualifiedName~WireMockDiagnosticTest"
    /// </remarks>
    [TestClass]
    [TestCategory("WireMock")]
    [TestCategory("localOnly")]
    [Ignore("Diagnostic tests - run manually to troubleshoot WireMock.Net issues. See https://github.com/WireMock-Net/WireMock.Net/issues/1387")]
    public class WireMockDiagnosticTest
    {
        /// <summary>
        /// Test basic .NET TCP listener to verify port binding works at all.
        /// </summary>
        [TestMethod]
        public void NetFramework_TcpListener_Should_Work()
        {
            // Find a free port
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();

            Console.WriteLine($"Found free port: {port}");

            // Now use that port for a simple TCP listener test
            var testListener = new TcpListener(IPAddress.Loopback, port);
            testListener.Start();
            Console.WriteLine($"TCP listener started on port {port}");

            // Check it's listening
            Assert.IsTrue(testListener.Server.IsBound, "Socket should be bound");

            testListener.Stop();
            Console.WriteLine("TCP listener test passed - basic networking works");
        }

        /// <summary>
        /// Test basic HttpListener (no Kestrel/OWIN) to verify HTTP works.
        /// </summary>
        [TestMethod]
        public void HttpListener_Should_Work()
        {
            // HttpListener requires URL reservation or admin rights
            // Use localhost instead of 127.0.0.1 since localhost is usually reserved for users
            int port = GetFreePort();
            var prefix = $"http://localhost:{port}/";

            using var listener = new HttpListener();
            listener.Prefixes.Add(prefix);

            try
            {
                listener.Start();
                Console.WriteLine($"HttpListener started on {prefix}");

                // Start a background task to handle one request
                var handled = new ManualResetEventSlim(false);
                var listenerTask = Task.Run(() =>
                {
                    try
                    {
                        var context = listener.GetContext();
                        context.Response.StatusCode = 200;
                        context.Response.Close();
                        handled.Set();
                        Console.WriteLine("HttpListener handled a request");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Listener error: {ex.Message}");
                    }
                });

                // Make a request
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var response = client.GetAsync(prefix).GetAwaiter().GetResult();

                Console.WriteLine($"Response: {response.StatusCode}");
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                // Wait for handler
                handled.Wait(TimeSpan.FromSeconds(2));
            }
            catch (HttpListenerException ex)
            {
                Console.WriteLine($"HttpListener failed: {ex.ErrorCode} - {ex.Message}");
                Console.WriteLine("This may indicate a URL reservation issue. Try running as admin or use netsh to add URL reservation.");
                throw;
            }
            finally
            {
                listener.Stop();
            }
        }

        /// <summary>
        /// Test WireMock with explicit settings and wait for ready.
        /// </summary>
        [TestMethod]
        public void WireMock_With_Settings_Should_Respond()
        {
            int port = GetFreePort();
            Console.WriteLine($"Using port: {port}");

            var settings = new WireMockServerSettings
            {
                Port = port,
                StartAdminInterface = true,
                ReadStaticMappings = false,
                WatchStaticMappings = false
            };

            using var server = WireMockServer.Start(settings);

            Console.WriteLine($"WireMock IsStarted: {server.IsStarted}");
            Console.WriteLine($"WireMock Url: {server.Url}");
            Console.WriteLine($"WireMock Port: {server.Port}");
            Console.WriteLine($"WireMock Urls: {string.Join(", ", server.Urls)}");

            // Wait a bit for the server to truly be ready
            Thread.Sleep(1000);
            Console.WriteLine("Waited 1 second for server to be ready");

            // Try multiple approaches
            TryRequest($"http://127.0.0.1:{port}/__admin/mappings", "127.0.0.1");
            TryRequest($"http://localhost:{port}/__admin/mappings", "localhost");
            TryRequest($"{server.Url}/__admin/mappings", "server.Url");
        }

        /// <summary>
        /// Test WireMock with longer wait and retry logic.
        /// </summary>
        [TestMethod]
        public void WireMock_With_Retry_Should_Eventually_Respond()
        {
            int port = GetFreePort();
            Console.WriteLine($"Using port: {port}");

            // Try with explicit host settings and logger to see what's happening
            var settings = new WireMockServerSettings
            {
                Port = port,
                UseSSL = false,
                StartAdminInterface = true,
                AllowPartialMapping = true,
                Logger = new WireMockLogging.WireMockConsoleLogger()
            };

            using var server = WireMockServer.Start(settings);

            Console.WriteLine($"WireMock IsStarted: {server.IsStarted}");
            Console.WriteLine($"WireMock Url: {server.Url}");
            Console.WriteLine($"WireMock Urls: [{string.Join(", ", server.Urls)}]");

            // Try with exponential backoff
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var url = $"http://127.0.0.1:{port}/__admin/mappings";

            for (int attempt = 1; attempt <= 5; attempt++)
            {
                int delay = (int)Math.Pow(2, attempt) * 100; // 200, 400, 800, 1600, 3200 ms
                Console.WriteLine($"Attempt {attempt}: waiting {delay}ms then requesting {url}");

                Thread.Sleep(delay);

                try
                {
                    var response = client.GetAsync(url).GetAwaiter().GetResult();
                    Console.WriteLine($"Attempt {attempt}: SUCCESS - Status {response.StatusCode}");
                    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                    return; // Success!
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine($"Attempt {attempt}: Timeout");
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Attempt {attempt}: {ex.Message}");
                }
            }

            Assert.Fail("WireMock never responded after 5 attempts");
        }

        /// <summary>
        /// Test WireMock using HostUrl setting to bind to specific interface.
        /// </summary>
        [TestMethod]
        public void WireMock_With_HostUrl_Should_Respond()
        {
            int port = GetFreePort();
            Console.WriteLine($"Using port: {port}");

            // Try binding to specific URL
            var settings = new WireMockServerSettings
            {
                Urls = new[] { $"http://127.0.0.1:{port}" },
                StartAdminInterface = true,
                Logger = new WireMockLogging.WireMockConsoleLogger()
            };

            using var server = WireMockServer.Start(settings);

            Console.WriteLine($"WireMock IsStarted: {server.IsStarted}");
            Console.WriteLine($"WireMock Urls: [{string.Join(", ", server.Urls)}]");

            Thread.Sleep(500);

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var url = $"http://127.0.0.1:{port}/__admin/mappings";
            Console.WriteLine($"Requesting: {url}");

            try
            {
                var response = client.GetAsync(url).GetAwaiter().GetResult();
                Console.WriteLine($"SUCCESS: {response.StatusCode}");
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAILED: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Test if using localhost vs 127.0.0.1 makes a difference.
        /// </summary>
        [TestMethod]
        public void WireMock_Localhost_String_Should_Respond()
        {
            int port = GetFreePort();
            Console.WriteLine($"Using port: {port}");

            // Use localhost string explicitly
            var settings = new WireMockServerSettings
            {
                Urls = new[] { $"http://localhost:{port}" },
                StartAdminInterface = true,
                Logger = new WireMockLogging.WireMockConsoleLogger()
            };

            using var server = WireMockServer.Start(settings);

            Console.WriteLine($"WireMock IsStarted: {server.IsStarted}");
            Console.WriteLine($"WireMock Urls: [{string.Join(", ", server.Urls)}]");

            Thread.Sleep(500);

            // Request using same localhost string
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var url = $"http://localhost:{port}/__admin/mappings";
            Console.WriteLine($"Requesting: {url}");

            try
            {
                var response = client.GetAsync(url).GetAwaiter().GetResult();
                Console.WriteLine($"SUCCESS: {response.StatusCode}");
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAILED: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        private static void TryRequest(string url, string label)
        {
            Console.WriteLine($"Trying {label}: {url}");
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };

            try
            {
                var response = client.GetAsync(url).GetAwaiter().GetResult();
                Console.WriteLine($"  {label}: SUCCESS - {response.StatusCode}");
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"  {label}: TIMEOUT");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"  {label}: FAILED - {ex.Message}");
            }
        }

        private static int GetFreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
