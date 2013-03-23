using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNet.SignalR.FunctionalTests.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.AspNet.SignalR.Tests
{
    public class ConnectionFacts : HostedTest
    {
        [Theory]
        //[InlineData(HostType.IISExpress, TransportType.Websockets)]
        [InlineData(HostType.IISExpress, TransportType.ServerSentEvents)]
        public void RequestHeadersSetCorrectly(HostType hostType, TransportType transportType)
        {
            using (var host = CreateHost(hostType, transportType))
            {
                // Arrange
                var mre = new ManualResetEventSlim(false);
                host.Initialize();
                var connection = CreateConnection(host, "/examine-request");

                connection.Received += (arg) =>
                {
                    JObject headers = JsonConvert.DeserializeObject<JObject>(arg);
                    // Assert.Equal("accept", (string) headers["acceptHeader"]);
                    Assert.Equal("test-header", (string) headers["testHeader"]);
                    mre.Set();
                };

                connection.Start(host.Transport).Wait();

                // Setting headers
                connection.Headers = new Dictionary<string, string>();
                // connection.Headers.Add(System.Net.HttpRequestHeader.Accept.ToString(), "accept");
                connection.Headers.Add("test-header", "test-header");

                connection.Send("Hello");

                // Assert
                Assert.True(mre.Wait(TimeSpan.FromSeconds(10)));

                // Clean-up
                mre.Dispose();
                connection.Stop();
            }
        }
    }
}
