using PipelineDebug.Model.Response;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace PipelineDebug.Security
{
    public class AdministratorOnlyAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext context)
        {
            return Sitecore.Context.IsAdministrator;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.OK, new BaseResponse { Status = ResponseStatus.Unauthorized, ErrorMessage = "Must be Sitecore admin" });
        }
    }
}
