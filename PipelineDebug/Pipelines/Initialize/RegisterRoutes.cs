using Sitecore.Pipelines;
using System.Web.Http;
using System.Web.Routing;

namespace PipelineDebug.Pipelines.Initialize
{
    public class RegisterRoutes
    {
        public void Process(PipelineArgs args)
        {
            RouteTable.Routes.MapHttpRoute("PipelineDebug", "pipelinedebug/{action}", new
            {
                controller = "PipelineDebug"
            });
        }
    }
}
