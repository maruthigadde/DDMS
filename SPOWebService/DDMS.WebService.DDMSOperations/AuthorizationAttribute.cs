using System;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using log4net;

namespace DDMS.WebService.DDMSOperations
{
    public class AuthorizationAttribute : AuthorizeAttribute
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string SecurityGroup = "";
        private string Application = "";
        public override void OnAuthorization(HttpActionContext httpContext)
        {
            Application = ConfigurationManager.AppSettings.Get("Application");
            Log.Info("Authorization Application :" + Application);
            SecurityGroup = ConfigurationManager.AppSettings.Get(Application + "SecurityGroup");
            Log.Info("Authorization SecurityGroup :" + SecurityGroup);
            if (String.IsNullOrEmpty(SecurityGroup))
                HandleUnathorized(httpContext);

            Log.Info("Authorization UserIdentity :" + HttpContext.Current.User.Identity.Name);

            var context = new PrincipalContext(
                                  ContextType.Domain,
                                  HttpContext.Current.User.Identity.Name.Split('\\')[0]);
            var userPrincipal = UserPrincipal.FindByIdentity(
                                   context,
                                   IdentityType.SamAccountName,
                                   HttpContext.Current.User.Identity.Name);

            if (userPrincipal.IsMemberOf(context, IdentityType.Name, SecurityGroup))
                return;
            else
                HandleUnathorized(httpContext);
        }

        private static void HandleUnathorized(HttpActionContext actionContext)
        {
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
        }
    }
}