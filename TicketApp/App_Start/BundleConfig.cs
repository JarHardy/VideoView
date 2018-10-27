using System.Web;
using System.Web.Optimization;

namespace TicketApp
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {


            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
            "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                          "~/Scripts/angular/bootstrap.js",
                          "~/Scripts/angular/angular.js",
                          "~/Scripts/angular/angular-animate.js",
                          "~/Scripts/angular/ui-bootstrap-tpls-0.13.0.js",
                          "~/Scripts/angular/angular-route.js"));

            bundles.Add(new ScriptBundle("~/bundles/app").Include(
           "~/Scripts/app/app_module.js",
           "~/Scripts/app/*.js"));

            // controllers and directives for partials
            bundles.Add(new ScriptBundle("~/bundles/Content").Include(
                "~/Content/partials/homePage/homePageCtrls.js",
                 "~/Content/partials/videoUploadPage/videoUploadPageCtrl.js",
                "~/Content/partials/main/mainCtrl.js"
                 ));

            var style_bundle = new StyleBundle("~/bundles/CSS").Include(
                "~/Content/assets/styles/site.css",
                "~/Content/assets/styles/bootstrap.css");

            bundles.Add(style_bundle);

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                  "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));
        }
    }
}