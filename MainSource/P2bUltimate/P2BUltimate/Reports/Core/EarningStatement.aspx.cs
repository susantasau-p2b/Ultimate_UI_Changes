using Microsoft.Reporting.WebForms;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace P2BUltimate.Reports.Core
{
    public partial class EarningStatement : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                RenderReports();
            }

        }
        private void RenderReports()
        {
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var where = Request.QueryString["where"];
                    var qurey = "select * from QCompanyMaster";
                    var cmd = db.Database.SqlQuery<Utility.ReportClass>(qurey).ToList<Utility.ReportClass>();
                    //department = db.Company.ToList();
                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RPTReports/Company.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportDataSource rdc = new ReportDataSource("DataSet1", cmd);
                    ReportViewer1.LocalReport.DataSources.Add(rdc);
                    ReportViewer1.LocalReport.Refresh();
                }

            }

        }
    }
}