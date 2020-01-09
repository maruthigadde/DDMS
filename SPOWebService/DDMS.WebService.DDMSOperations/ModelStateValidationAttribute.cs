using DDMS.WebService.Constants;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace DDMS.WebService.DDMSOperations
{
    public class ModelStateValidationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext httpActionContext)
        {
            if (!httpActionContext.ModelState.IsValid)
            {
                httpActionContext.Response = httpActionContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest, httpActionContext.ModelState);
                httpActionContext.Response.Headers.Add(HeaderConstants.MessageId, httpActionContext.Request.Headers.GetValues(HeaderConstants.MessageId).ToString());
                httpActionContext.Response.Headers.Add(HeaderConstants.SiteId, httpActionContext.Request.Headers.GetValues(HeaderConstants.SiteId).ToString());
                httpActionContext.Response.Headers.Add(HeaderConstants.BusinessId, httpActionContext.Request.Headers.GetValues(HeaderConstants.BusinessId).ToString());
                httpActionContext.Response.Headers.Add(HeaderConstants.CollectedTimeStamp, DateTime.Now.ToString());
                httpActionContext.Response.Headers.Add(HeaderConstants.Code, HeaderErrorConstants.CodeSender);
                httpActionContext.Response.Headers.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                httpActionContext.Response.Headers.Add(HeaderConstants.Node, httpActionContext.Request.GetRequestContext().VirtualPathRoot);
                httpActionContext.Response.Headers.Add(HeaderConstants.ErrorCode, Convert.ToString((int)HttpStatusCode.BadRequest));
                httpActionContext.Response.Headers.Add(HeaderConstants.ErrorDescription, httpActionContext.ModelState.ToString());
            }
        }
    }
}