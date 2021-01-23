using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JobFinderWebSite.Custom.CustomFilters
{
    // MM "Token Module: - if the user is authenticated and the bearerToken not exist then logout the user".
    //-----------------------------------
    public class CheckBearerToken : FilterAttribute, IAuthorizationFilter
    {        
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if(filterContext.HttpContext.User.Identity.IsAuthenticated && filterContext.HttpContext.Request.Cookies["BearerToken"] == null)
            {
                // We use that result property to set the result that will returned by the action method.
                // Here the type of the reult that we can return {https://csharp-video-tutorials.blogspot.com/2013/08/part-78-different-types-of-actionresult.html}

                filterContext.HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                filterContext.Result = new RedirectResult("~/Account/Login");
            }
        }
    }
    //-----------------------------------
}