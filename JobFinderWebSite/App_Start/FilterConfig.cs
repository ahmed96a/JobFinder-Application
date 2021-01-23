using System.Web;
using System.Web.Mvc;

namespace JobFinderWebSite
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());

            // MM "Token Module: - apply the CheckBearerToken globally"
            //--------------------------------------------------------
            filters.Add(new Custom.CustomFilters.CheckBearerToken());
            //--------------------------------------------------------
        }
    }
}
