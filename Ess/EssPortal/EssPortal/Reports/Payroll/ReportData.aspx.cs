using EssPortal.App_Start;
using EssPortal.Models;
using EssPortal.Process;
using EssPortal.Security;
using ReportPayroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Entity;
using Microsoft.Reporting.WebForms;

namespace EssPortal.Reports.Payroll
{
    public partial class ReportData : System.Web.UI.Page
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
            using (DataBaseContext db = new DataBaseContext())
            {
                var ReportName = Request.QueryString["ReportName"];
                var FieldCount = Request.QueryString["FieldCount"];
                var Reportype = Request.QueryString["Reportype"];
                var monthly = Request.QueryString["Monthly"];
                var employeeids = Request.QueryString["Employee"];
                var paymonth = Request.QueryString["Paymonth"];
                var ModuleName = Session["ModuleType"] == null ? "Payroll" : Session["ModuleType"];
                var emp = new List<int>();
                var months = new List<string>();
                if (employeeids != null)
                {

                    emp = Utility.StringIdsToListIds(employeeids);
                }
                if (monthly != null)
                {
                    months = Utility.StringIdsToListString(monthly);
                }
                if (paymonth != null)
                {
                    months = Utility.StringIdsToListString(paymonth);
                }
                var mFromDate = Request.QueryString["formdate"];
                var mToDate = Request.QueryString["todate"];
                var z = new List<int>();
                foreach (var item in emp)
                {
                    z.Add(Convert.ToInt32(item));
                }
                var monthllist = new List<string>();
                foreach (var item in months)
                {
                    monthllist.Add(Convert.ToString(item));
                }
                var emplvidslist = new List<int>();

                // var Gen100 = new List<GenericField100>();
                dynamic Gen100 = null;
                //if (ModuleName != null)
                //{
                //    if (ModuleName.ToString().Trim().ToUpper() == "EPMS")
                //        ModuleName = "Payroll";
                //    if (ModuleName.ToString().Trim().ToUpper() == "ELMS")
                //        ModuleName = "Leave";
                //}

                //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                //              new System.TimeSpan(0, 60, 0)))
                //{
                ReportName = ReportName.Remove(0, 3);
                //// if (ModuleName == "Payroll")
                if (ReportName.ToUpper() == "ANNUALSALARY" || ReportName.ToUpper() == "BONUSCHKT" || ReportName.ToUpper() == "ITPROJECTION")
                {
                    Gen100 = ReportRDLCObjectClass.GenerateAnnualStatementReport(Convert.ToInt32(SessionManager.CompPayrollId), z, ReportName.ToUpper(), Convert.ToDateTime(mFromDate), Convert.ToDateTime(mToDate), db);
                }
                else if (ReportName.ToUpper() == "FUNCTATTENDANCET" || ReportName.ToUpper() == "PAYSLIPR")
                {
                    Gen100 = ReportRDLCObjectClass.GeneratePayrollReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), db);

                }
                else if (ReportName.ToUpper() == "SALARYREGISTER")
                {

                    var SanctionEmpGeostructid = db.Employee.Where(e => z.Contains(e.Id)).Select(e => e.GeoStruct.Id).FirstOrDefault();
                    var where = " Where ";
                    if (monthly != null)
                    {
                        where += " PayMonth ='" + monthly + "'";  //changed by vinayak 

                    }

                    if (employeeids != null)
                    {
                        List<int?> employeetotalno = db.SalaryT.Where(q => q.Geostruct.Id == SanctionEmpGeostructid && q.PayMonth == monthly).Select(e => e.EmployeePayroll_Id).ToList();
                        if (employeetotalno.Count() > 0)
                        {
                            string joined = string.Join(",", employeetotalno);
                            if (joined != "")
                            {

                                where += "And EmployeePayroll_Id IN (" + joined + ") And EarnAmount<>0";
                            }
                        }
                    }
                    var qurey = "select LocCode, LocDesc, EmpCode,FullNameFML, Grade_Code,Job_Name, PayMonth, LookupVal,EarnHead,EarnAmount,TotalEarning,  TotalDeduction, EPS_Share,ER_Share,TotalNet,DeptCode,DeptDesc from SalaryDetails" + where + " ORDER BY SEQNO";
                    if (qurey != "")
                    {
                        db.Database.CommandTimeout = 180;
                        var cmd = db.Database.SqlQuery<Utility.ReportClass>(qurey).ToList<Utility.ReportClass>();
                        if (cmd.Count() > 0)
                        {
                            Gen100 = cmd;
                        }
                    }
                }
                //         var month = monthllist[0];

                //         Gen100 = ReportRDLCObjectClass.GenerateBankStatement(Convert.ToInt32(SessionManager.CompPayrollId), z, month, db);
                //     }
                //     else if (ReportName.ToUpper() == "SALREGISTER")
                //         Gen100 = ReportRDLCObjectClass.GenerateSalRegister(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, db);
                //     else
                //         Gen100 = ReportRDLCObjectClass.GeneratePayrollReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), db);

                // //if (ModuleName == "Leave")
                // //{
                //     foreach (var item in z)
                //     {
                //         var id = Convert.ToInt32(item);
                //         var data = db.EmployeeLeave.Include(e => e.Employee).Where(e => e.Employee.Id == id).Select(e => e.Id).SingleOrDefault();
                //         emplvidslist.Add(data);
                //     }
                //     Gen100 = ReportRDLCObjectClass.GenerateLeaveReport(Convert.ToInt32(SessionManager.CompLvId), ReportName.ToUpper(), emplvidslist, Convert.ToDateTime(mFromDate), Convert.ToDateTime(mToDate), db);

                // }

                // //if (ModuleName == "Core")
                //     Gen100 = ReportRDLCObjectClass.GenerateCoreReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), db);
                //    ts.Complete();
                //}



                //   ReportName = "PayslipR";

                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Reports/" + "Rpt" + ReportName + ".rdlc");

                ReportViewer1.LocalReport.DataSources.Clear();

                //                if (Gen100 != null && Gen100.Count > 0)
                //                {

                //                    ReportDataSource rdc = new ReportDataSource("P2BDataSet", Gen100);
                //                    Utility.ReportClass oReportClass = new Utility.ReportClass();
                //                    ReportParameter[] param = new ReportParameter[5];
                //                    param[0] = new ReportParameter("ReportCompanyName", oReportClass.ReportCompanyName, true);
                //                    param[1] = new ReportParameter("ReportCreatedBy", oReportClass.ReportCreatedBy, true);
                //                    ReportViewer1.LocalReport.EnableExternalImages = true;
                //                    Uri pathAsUri = new Uri(Server.MapPath("~/Content/Login/Images/PCBL.png"));

                //                    if (pathAsUri != null)
                //                    {
                //                        param[4] = new ReportParameter("ReportCompanyLogo", pathAsUri.AbsoluteUri);
                //                    }
                //                    this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { param[0], param[1], param[4] });
                //                    ReportViewer1.LocalReport.DataSources.Add(rdc);
                //                    ReportViewer1.LocalReport.Refresh();
                //                }
                //                else
                //                {
                //                    var nodata = new Utility.ReportClass();
                //                    List<Utility.ReportClass> newdatalist = new List<Utility.ReportClass>();
                //                    Utility.ReportClass oReportClass = new Utility.ReportClass();
                //                    newdatalist.Add(nodata);
                //                    ReportDataSource rdc = new ReportDataSource("P2BDataSet", newdatalist);
                //                    ReportParameter[] param = new ReportParameter[5];
                //                    param[0] = new ReportParameter("ReportCompanyName", "No Data Found", true);
                //                    param[1] = new ReportParameter("ReportCreatedBy", oReportClass.ReportCreatedBy, true);
                //                    this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { param[0], param[1] });
                //                    ReportViewer1.LocalReport.DataSources.Add(rdc);
                //                    ReportViewer1.LocalReport.Refresh();
                //                }
                //            }
                //        }
                //    }
                //}

                if (Gen100 != null && Gen100.Count > 0)
                {
                    ReportDataSource rdc = new ReportDataSource("P2BDataSet", Gen100);

                    Utility.ReportClass oReportClass = new Utility.ReportClass();
                    ReportParameter[] param = new ReportParameter[5];
                    param[0] = new ReportParameter("ReportCompanyName", oReportClass.ReportCompanyName, true);
                    param[1] = new ReportParameter("ReportCreatedBy", oReportClass.ReportCreatedBy, true);
                    ReportViewer1.LocalReport.EnableExternalImages = true;
                    Uri pathAsUri = new Uri(Server.MapPath("~/Content/Login/Images/PCBL.png"));

                    if (pathAsUri != null)
                    {
                        param[4] = new ReportParameter("ReportCompanyLogo", pathAsUri.AbsoluteUri);
                    }

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { param[0], param[1], param[4] });
                    ReportViewer1.LocalReport.DataSources.Add(rdc);
                    ReportViewer1.LocalReport.Refresh();
                }
                else
                {
                    var nodata = new Utility.ReportClass();
                    List<Utility.ReportClass> newdatalist = new List<Utility.ReportClass>();
                    Utility.ReportClass oReportClass = new Utility.ReportClass();
                    newdatalist.Add(nodata);
                    ReportDataSource rdc = new ReportDataSource("P2BDataSet", newdatalist);

                    ReportParameter[] param = new ReportParameter[5];
                    param[0] = new ReportParameter("ReportCompanyName", "No Data Found", true);
                    param[1] = new ReportParameter("ReportCreatedBy",  oReportClass.ReportCreatedBy , true);

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { param[0], param[1] });
                    ReportViewer1.LocalReport.DataSources.Add(rdc);
                    ReportViewer1.LocalReport.Refresh();
                }

                // Method to filter export options for NKGSB
                //Utility.ReportClass oReportClass1 = new Utility.ReportClass();
                //if (oReportClass1.ReportCompanyName.ToUpper() == "NKGSB CO OP BANK LTD")
                //{
                //    FilterExportOptions(); 
                //}

                FilterExportOptions(); 
            }
        }


        // Filter export options to show only PDF
        private void FilterExportOptions()
        {
            foreach (RenderingExtension extension in ReportViewer1.LocalReport.ListRenderingExtensions())
            {
                if (!extension.Name.Equals("PDF", StringComparison.OrdinalIgnoreCase))
                {
                    System.Reflection.FieldInfo fieldInfo = extension.GetType().GetField("m_isVisible", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    if (fieldInfo != null)
                    {
                        fieldInfo.SetValue(extension, false);
                    }
                }
            }
        }
    }
    
}