using System;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.FunctionalTests
{
    public class ExamineRequestConnection : PersistentConnection
    {
        private string _acceptHeader, _testHeader;

        public override Task ProcessRequest(Hosting.HostContext context)
        {
            if (IsSendRequest(context.Request))
            {
                // _acceptHeader = context.Request.Headers.GetValues(System.Net.HttpRequestHeader.Accept.ToString())[0];
                _testHeader = context.Request.Headers.GetValues("test-header")[0];
            }
            return base.ProcessRequest(context);
        }

        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            return Connection.Send(connectionId, new
            {
                acceptHeader = _acceptHeader,
                testHeader = _testHeader
            });
        }

        private static bool IsSendRequest(IRequest request)
        {
            return request.Url.LocalPath.EndsWith("/send", StringComparison.OrdinalIgnoreCase);
        }
    }
}
