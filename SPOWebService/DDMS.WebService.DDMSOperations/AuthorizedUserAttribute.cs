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
    public class AuthorizedUserAttribute : AuthorizeAttribute
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string SecurityGroup = "";
        public string Application { get; set; }
        protected override bool IsAuthorized(HttpActionContext httpContext)
        {
            Log.Info("Authorization Application :" + Application);
            SecurityGroup = ConfigurationManager.AppSettings.Get(Application + "SecurityGroup");
            Log.Info("Authorization SecurityGroup :" + SecurityGroup);
            if (String.IsNullOrEmpty(SecurityGroup))
            {
                return false;
            }

            Log.Info("Authorization UserIdentity :" + HttpContext.Current.User.Identity.Name);

            var context = new PrincipalContext(
                                  ContextType.Domain,
                                  SecurityGroup.Split('\\')[0]);
            Log.Info("Context Fetched");
            var userPrincipal = UserPrincipal.FindByIdentity(
                                   context,
                                   IdentityType.SamAccountName,
                                   HttpContext.Current.User.Identity.Name);
            Log.Info("User Principal Fetched");
            if (userPrincipal.IsMemberOf(context, IdentityType.Name, SecurityGroup.Split('\\')[1]))
            {
                Log.Info("User is a member of AD Group");
                return true;
            }
            else
            {
                Log.Info("User is not a member of AD Group");
                return false;
            }
        }

        //public override void OnAuthorization(HttpActionContext httpContext)
        //{
        //    Log.Info("Authorization Application :" + Application);
        //    SecurityGroup = ConfigurationManager.AppSettings.Get(Application + "SecurityGroup");
        //    Log.Info("Authorization SecurityGroup :" + SecurityGroup);
        //    if (String.IsNullOrEmpty(SecurityGroup))
        //        HandleUnathorized(httpContext);



        //    Log.Info("Authorization UserIdentity :" + HttpContext.Current.User.Identity.Name);



        //    var context = new PrincipalContext(
        //                          ContextType.Domain,
        //                          SecurityGroup.Split('\\')[0]);
        //    Log.Info("Context fetched");
        //    var userPrincipal = UserPrincipal.FindByIdentity(
        //                           context,
        //                           IdentityType.SamAccountName,
        //                           HttpContext.Current.User.Identity.Name);

        //    Log.Info("User principal identified");

        //    if (userPrincipal.IsMemberOf(context, IdentityType.Name, SecurityGroup.Split('\\')[1]))
        //        return;
        //    else
        //        HandleUnathorized(httpContext);
        //}


        private static void HandleUnathorized(HttpActionContext actionContext)
        {
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
        }
    }
}