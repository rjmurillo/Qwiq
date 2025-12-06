using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Qwiq.Client.Rest;
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
    [TestClass]
    [TestCategory("RestUnit")]
    public class Given_WorkItemStore_When_Querying_With_WIQL : ContextSpecification
    {
#pragma warning disable CS0649 // Field is never assigned to - pending implementation
        private WireMockServer? _server;
        private IWorkItemStore? _store;
        private IWorkItemCollection? _result;
#pragma warning restore CS0649
        private const string TestWiql = "SELECT [System.Id] FROM WorkItems WHERE [System.WorkItemType] = 'Bug'";

        public override void Given()
        {
            // Start WireMock server on random port
            _server = WireMockServer.Start();

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

            // TODO: Create store instance pointing to WireMock server
            // This requires creating a test factory or helper that can inject the base URL
            // For now, this is a placeholder showing the test structure
        }

        public override void When()
        {
            // TODO: Execute query once store creation is implemented
            // _result = _store?.Query(TestWiql);
        }

        [TestMethod]
        [Ignore("Implementation pending - requires REST client factory refactoring")]
        public void Should_Return_Two_WorkItems()
        {
            _result.ShouldNotBeNull();
#pragma warning disable CA1829, CA1826 // Use Count property when available
            _result.Count().ShouldBe(2);
#pragma warning restore CA1829, CA1826
        }

        [TestMethod]
        [Ignore("Implementation pending - requires REST client factory refactoring")]
        public void Should_Have_Correct_WorkItem_Ids()
        {
            _result.ShouldNotBeNull();
            var ids = _result.Select(wi => wi.Id).ToArray();
            ids.ShouldContain(1);
            ids.ShouldContain(2);
        }

        [TestMethod]
        [Ignore("Implementation pending - requires REST client factory refactoring")]
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
