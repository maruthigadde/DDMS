using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using DDMS.WebService.Models.Common;

namespace DDMS.WebService.DDMSOperations
{
    public class HeaderValidateAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            Dictionary<string, dynamic> headerKeyValuePairs = new Dictionary<string, dynamic>();
            if (actionContext != null && actionContext.Request != null && actionContext.Request.Headers != null)
            {
                if (!ValidateHeader(actionContext, ref headerKeyValuePairs))
                {
                    HandleUnathorized(actionContext);
                    foreach (var field in headerKeyValuePairs)
                    {
                        if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                            actionContext.Response.Headers.Add(field.Key, field.Value.ToString());
                    }
                    return;
                }
            }
            else
            {
                HandleUnathorized(actionContext);

                headerKeyValuePairs.Add(HeaderConstants.Code, HeaderErrorConstants.CodeSender);
                headerKeyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                headerKeyValuePairs.Add(HeaderConstants.Node, actionContext.Request.GetRequestContext().VirtualPathRoot);
                headerKeyValuePairs.Add(HeaderConstants.ErrorDescription, HeaderErrorConstants.ErrorDescriptionInvalidRequest);
                headerKeyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.Unauthorized);
                foreach (var field in headerKeyValuePairs)
                {
                    if (!string.IsNullOrEmpty(field.Value.ToString()) && !string.IsNullOrWhiteSpace(field.Value.ToString()))
                        actionContext.Response.Headers.Add(field.Key, field.Value.ToString());
                }
                return;
            }
        }

        private static void HandleUnathorized(HttpActionContext actionContext)
        {
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
        }

        private bool ValidateHeader(HttpActionContext actionContext, ref Dictionary<string, dynamic> keyValuePairs)
        {
            Guid messageId = new Guid();
            DateTime collectedTimeStamp = new DateTime();
            try
            {
                if (!actionContext.Request.Headers.Contains(HeaderConstants.MessageId) || string.IsNullOrEmpty(actionContext.Request.Headers.GetValues(HeaderConstants.MessageId).First()))
                {
                    keyValuePairs.Add(HeaderConstants.Code, HeaderErrorConstants.CodeSender);
                    keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                    keyValuePairs.Add(HeaderConstants.Node, actionContext.Request.GetRequestContext().VirtualPathRoot);
                    keyValuePairs.Add(HeaderConstants.ErrorDescription, HeaderErrorConstants.ErrorDescriptionMessageIdRequired);
                    keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.Unauthorized);
                    return false;
                }
                if (!actionContext.Request.Headers.Contains(HeaderConstants.SiteId) || string.IsNullOrEmpty(actionContext.Request.Headers.GetValues(HeaderConstants.SiteId).First()))
                {
                    keyValuePairs.Add(HeaderConstants.Code, HeaderErrorConstants.CodeSender);
                    keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                    keyValuePairs.Add(HeaderConstants.Node, actionContext.Request.GetRequestContext().VirtualPathRoot);
                    keyValuePairs.Add(HeaderConstants.ErrorDescription, HeaderErrorConstants.ErrorDescriptionSiteIdRequired);
                    keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.Unauthorized);
                    return false;
                }
                if (!actionContext.Request.Headers.Contains(HeaderConstants.BusinessId) || string.IsNullOrEmpty(actionContext.Request.Headers.GetValues(HeaderConstants.BusinessId).First()))
                {
                    keyValuePairs.Add(HeaderConstants.Code, HeaderErrorConstants.CodeSender);
                    keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                    keyValuePairs.Add(HeaderConstants.Node, actionContext.Request.GetRequestContext().VirtualPathRoot);
                    keyValuePairs.Add(HeaderConstants.ErrorDescription, HeaderErrorConstants.ErrorDescriptionBusinessIdRequired);
                    keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.Unauthorized);
                    return false;
                }
                if (!actionContext.Request.Headers.Contains(HeaderConstants.CollectedTimeStamp) || string.IsNullOrEmpty(actionContext.Request.Headers.GetValues(HeaderConstants.CollectedTimeStamp).First()))
                {
                    keyValuePairs.Add(HeaderConstants.Code, HeaderErrorConstants.CodeSender);
                    keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                    keyValuePairs.Add(HeaderConstants.Node, actionContext.Request.GetRequestContext().VirtualPathRoot);
                    keyValuePairs.Add(HeaderConstants.ErrorDescription, HeaderErrorConstants.ErrorDescriptionCollectedTimeStampRequired);
                    keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.Unauthorized);
                    return false;
                }
                if (actionContext.Request.Headers.Contains(HeaderConstants.MessageId) && actionContext.Request.Headers.Contains(HeaderConstants.SiteId) && actionContext.Request.Headers.Contains(HeaderConstants.BusinessId) && actionContext.Request.Headers.Contains(HeaderConstants.CollectedTimeStamp))
                {
                    if (actionContext.Request.Headers.Contains(HeaderConstants.MessageId) && !string.IsNullOrEmpty(actionContext.Request.Headers.GetValues(HeaderConstants.MessageId).First()))
                    {
                        bool result = Guid.TryParse(actionContext.Request.Headers.GetValues(HeaderConstants.MessageId).First(), out messageId);
                        if (!result && messageId == Guid.Empty)
                        {
                            keyValuePairs.Add(HeaderConstants.Code, HeaderErrorConstants.CodeSender);
                            keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                            keyValuePairs.Add(HeaderConstants.Node, actionContext.Request.GetRequestContext().VirtualPathRoot);
                            keyValuePairs.Add(HeaderConstants.ErrorDescription, HeaderErrorConstants.ErrorDescriptionMessageIdInvalid);
                            keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.Unauthorized);
                            return false;
                        }
                    }

                    if (actionContext.Request.Headers.Contains(HeaderConstants.SiteId) && !string.IsNullOrEmpty(actionContext.Request.Headers.GetValues(HeaderConstants.SiteId).First()))
                    {
                        if (actionContext.Request.Headers.GetValues(HeaderConstants.SiteId).First().ToUpper() != HeaderValueConstants.SiteId.ToUpper())
                        {
                            keyValuePairs.Add(HeaderConstants.Code, HeaderErrorConstants.CodeSender);
                            keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                            keyValuePairs.Add(HeaderConstants.Node, actionContext.Request.GetRequestContext().VirtualPathRoot);
                            keyValuePairs.Add(HeaderConstants.ErrorDescription, HeaderErrorConstants.ErrorDescriptionSiteIdInvalid);
                            keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.Unauthorized);
                            return false;
                        }
                    }

                    if (actionContext.Request.Headers.Contains(HeaderConstants.BusinessId) && !string.IsNullOrEmpty(actionContext.Request.Headers.GetValues(HeaderConstants.BusinessId).First()))
                    {
                        if (actionContext.Request.Headers.GetValues(HeaderConstants.BusinessId).First().ToUpper() != HeaderValueConstants.BusinessId.ToUpper())
                        {
                            keyValuePairs.Add(HeaderConstants.Code, HeaderErrorConstants.CodeSender);
                            keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                            keyValuePairs.Add(HeaderConstants.Node, actionContext.Request.GetRequestContext().VirtualPathRoot);
                            keyValuePairs.Add(HeaderConstants.ErrorDescription, HeaderErrorConstants.ErrorDescriptionBusinessIdInvalid);
                            keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.Unauthorized);
                            return false;
                        }
                    }

                    if (actionContext.Request.Headers.Contains(HeaderConstants.CollectedTimeStamp) && !string.IsNullOrEmpty(actionContext.Request.Headers.GetValues(HeaderConstants.CollectedTimeStamp).First()))
                    {
                        bool result = DateTime.TryParse(actionContext.Request.Headers.GetValues(HeaderConstants.CollectedTimeStamp).First(), out collectedTimeStamp);
                        if (!result)
                        {
                            keyValuePairs.Add(HeaderConstants.Code, HeaderErrorConstants.CodeSender);
                            keyValuePairs.Add(HeaderConstants.ErrorType, HeaderErrorConstants.ErrorTypeSecurity);
                            keyValuePairs.Add(HeaderConstants.Node, actionContext.Request.GetRequestContext().VirtualPathRoot);
                            keyValuePairs.Add(HeaderConstants.ErrorDescription, HeaderErrorConstants.ErrorDescriptionCollectedTimeStampInvalid);
                            keyValuePairs.Add(HeaderConstants.ErrorCode, (int)HttpStatusCode.Unauthorized);
                            return false;
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
