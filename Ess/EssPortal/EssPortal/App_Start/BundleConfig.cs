using System.Web;
using System.Web.Optimization;

namespace EssPortal
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            //P2B Scripts
            bundles.Add(new ScriptBundle("~/bundles/P2B_Scripts").Include(
                "~/Scripts/P2B_Scripts/datetimepicker.js",
                "~/Scripts/P2B_Scripts/grid.locale-en.js",
                "~/Scripts/P2B_Scripts/jquery.jqGrid.js",
                "~/Scripts/P2B_Scripts/jQuery.mbValidations.js",
                "~/Scripts/P2B_Scripts/Standard.js",
                "~/Scripts/webcam.js"));

            //P2B CSS
            bundles.Add(new StyleBundle("~/Content/P2B_CSS")
                .Include("~/Content/P2B_CSS/External.css",
                "~/Content/P2B_CSS/Normalize.css",
                "~/Content/P2B_CSS/Standard.css",
                "~/Content/popup_form.css",
                "~/Content/P2B_CSS/font-awesome-4.6.3/font-awesome-4.6.3/",
                "~/Content/P2B_CSS/font-awesome-4.6.3/font-awesome-4.6.3/css/font-awesome.css",
                "~/Content/P2B_CSS/jquery-ui.css"
                ));

            //bundles.Add(new StyleBundle("~/bundles/bootstrap_css")
            //    .Include("~/Content/P2B_CSS/bootstrap.css"));

            //bundles.Add(new ScriptBundle("~/bundles/bootstrap_script")
            //    .Include( "~/Scripts/P2B_Scripts/bootstrap.js"));

            BundleTable.EnableOptimizations = false;
        }
    }
}