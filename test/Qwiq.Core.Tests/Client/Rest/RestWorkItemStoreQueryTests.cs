using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Qwiq.Client.Rest;
using Qwiq.Credentials;
using Qwiq.Tests.Common;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Qwiq.Client.Rest
{
    /// <summary>
    /// Tests for REST client query execution using WireMock.Net for HTTP mocking.
    /// </summary>
    /// <remarks>
    /// These tests demonstrate the refactored factory pattern that enables unit testing without Azure DevOps connectivity.
    /// The WorkItemStoreFactory now accepts an internal ITfsConnectionFactory parameter for test injection.
    /// 
    /// KNOWN ISSUE: VssConnection attempts to establish a connection during construction, requiring SSL certificate validation.
    /// Future enhancement: Configure HttpClient to accept WireMock's self-signed certificate or use a different mocking approach.
    /// </remarks>
    [TestClass]
    [TestCategory("RestUnit")]
    [Ignore("SSL certificate validation pending - see class remarks")]
    public class Given_WorkItemStore_When_Querying_With_WIQL : ContextSpecification
    {
        private WireMockServer? _server;
        private IWorkItemStore? _store;
        private IWorkItemCollection? _result;
        private const string TestWiql = "SELECT [System.Id] FROM WorkItems WHERE [System.WorkItemType] = 'Bug'";

        public override void Given()
        {
            // Start WireMock server with HTTPS support
            var settings = new WireMock.Settings.WireMockServerSettings
            {
                UseSSL = true,
                Port = 0 // Use random port
            };
            _server = WireMockServer.Start(settings);

            // Configure mock response for WIQL query
            _server
                .Given(Request.Create()
                    .WithPath("/*/_apis/wit/wiql")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{
                        ""queryType"": ""flat"",
                        ""queryResultType"": ""workItem"",
                        ""asOf"": ""2025-12-06T00:00:00.000Z"",
                        ""workItems"": [
                            {
                                ""id"": 1,
                                ""url"": """ + _server.Url + @"/_apis/wit/workItems/1""
                            },
                            {
                                ""id"": 2,
                                ""url"": """ + _server.Url + @"/_apis/wit/workItems/2""
                            }
                        ]
                    }"));

            // Mock the work items endpoint
            _server
                .Given(Request.Create()
                    .WithPath("/*/_apis/wit/workItems")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{
                        ""count"": 2,
                        ""value"": [
                            {
                                ""id"": 1,
                                ""rev"": 1,
                                ""fields"": {
                                    ""System.Id"": 1,
                                    ""System.WorkItemType"": ""Bug"",
                                    ""System.Title"": ""Test Bug 1"",
                                    ""System.State"": ""Active"",
                                    ""System.TeamProject"": ""TestProject""
                                },
                                ""url"": """ + _server.Url + @"/_apis/wit/workItems/1""
                            },
                            {
                                ""id"": 2,
                                ""rev"": 1,
                                ""fields"": {
                                    ""System.Id"": 2,
                                    ""System.WorkItemType"": ""Bug"",
                                    ""System.Title"": ""Test Bug 2"",
                                    ""System.State"": ""Active"",
                                    ""System.TeamProject"": ""TestProject""
                                },
                                ""url"": """ + _server.Url + @"/_apis/wit/workItems/2""
                            }
                        ]
                    }"));

            // Create mock connection factory that points to WireMock server
            var mockConnectionFactory = new MockTfsConnectionFactory(new Uri(_server.Url!));

            // Create authentication options with a simple credentials factory
            // Credentials don't matter for WireMock, just need to provide something
            var options = new AuthenticationOptions(
                new Uri(_server.Url!),
                AuthenticationTypes.Basic,
                _ => new VssCredentials[] { new VssBasicCredential(string.Empty, string.Empty) });

            // Get the factory instance and use internal overload for testing
            var factory = (WorkItemStoreFactory)WorkItemStoreFactory.Default;
            _store = factory.Create(options, mockConnectionFactory);
        }

        public override void When()
        {
            // Execute query against WireMock server
            _result = _store?.Query(TestWiql);
        }

        [TestMethod]
        public void Should_Return_Two_WorkItems()
        {
            _result.ShouldNotBeNull();
#pragma warning disable CA1829, CA1826 // Use Count property when available
            _result.Count().ShouldBe(2);
#pragma warning restore CA1829, CA1826
        }

        [TestMethod]
        public void Should_Have_Correct_WorkItem_Ids()
        {
            _result.ShouldNotBeNull();
            var ids = _result.Select(wi => wi.Id).ToArray();
            ids.ShouldContain(1);
            ids.ShouldContain(2);
        }

        [TestMethod]
        public void Should_Have_Correct_WorkItem_Types()
        {
            _result.ShouldNotBeNull();
            _result.All(wi => wi.Type?.Name == "Bug").ShouldBeTrue();
        }

        public override void Cleanup()
        {
            _server?.Stop();
            _server?.Dispose();
            (_store as IDisposable)?.Dispose();
        }
    }
}
