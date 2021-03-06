﻿using System.Web;
using System.Web.Optimization;

namespace JobFinderWebSite
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js",
                      "~/Scripts/all.js")); // Font Awesome

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/Content/all.css")); // Font Awesome


            // "MM" AdminLTE DashBoard
            //--------------------------------

            bundles.Add(new ScriptBundle("~/AdminLTE/js").Include(
                "~/Content/AdminLTE/plugins/jquery/jquery.min.js",
                "~/Content/AdminLTE/plugins/bootstrap/js/bootstrap.bundle.min.js",
                "~/Content/AdminLTE/dist/js/adminlte.min.js",
                "~/Content/AdminLTE/dist/js/demo.js"));

            bundles.Add(new StyleBundle("~/AdminLTE/css").Include(
                "~/Content/AdminLTE/plugins/fontawesome-free/css/all.min.css",
                "~/Content/AdminLTE/dist/css/ionicons.min.css",
                "~/Content/AdminLTE/dist/css/adminlte.min.css"));
            //--------------------------------
        }
    }
}
