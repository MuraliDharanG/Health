using HospitalCloud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace HospitalCould
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            try
            {
                var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                if (authTicket.UserData == "OAuth") return;
                var serializeModel =
                    serializer.Deserialize<User>(authTicket.UserData);
                var newUser = new CustomPrincipal(authTicket.Name)
                {

                    Username = serializeModel.UserName,
                    UserId = serializeModel.UserId,
                    Role = serializeModel.Role,

                };
                HttpContext.Current.User = newUser;

            }
            catch (Exception ex)
            {
                Response.Write("<script>" + ex.Message + "</script>");
                //HttpContext.Current.RewritePath("~\\Login\\Login");
            }

        }
    }
}