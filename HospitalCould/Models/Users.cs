

using MongoDB.Bson;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
namespace HospitalCloud
{
    public class User
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    public class CustomPrincipal : ICustomPrincipal
    {
        private IIdentity ICustomIdentity { get; set; }

        public CustomPrincipal(string username)
        {
            this.ICustomIdentity = new GenericIdentity(username);
        }
        public bool IsInRole(string role)
        {

            if (Role == role)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public string UserId { get; set; }
        public string Username { get; set; }

        public string UserImage { get; set; }

        public string Role { get; set; }

        public IIdentity Identity
        {
            get { return ICustomIdentity; }
        }
    }

    public interface ICustomPrincipal : IPrincipal
    {
        string UserId { get; set; }
        string Username { get; set; }
        string Role { get; set; }
        string UserImage { get; set; }

    }
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        public string UsersConfigKey { get; set; }
        public string RolesConfigKey { get; set; }

        protected virtual CustomPrincipal CurrentUser
        {
            get { return HttpContext.Current.User as CustomPrincipal; }
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAuthenticated)
            {

                if (!(Roles == CurrentUser.Role || CurrentUser.Role == "Administrator"))
                {
                    filterContext.Result = new RedirectToRouteResult(new
                 RouteValueDictionary(new { controller = "Error", action = "AccessDenied" }));

                    base.OnAuthorization(filterContext); //returns to login url

                }
            }

        }

    }
}