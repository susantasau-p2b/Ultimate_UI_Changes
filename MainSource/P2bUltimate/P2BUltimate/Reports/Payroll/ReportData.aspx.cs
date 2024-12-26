using Microsoft.Reporting.WebForms;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
//using P2BUltimate.Process;
using P2BUltimate.Security;
using ReportPayroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Entity;
using System.Web.Services;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Collections.Specialized;
using P2BUltimate.Process;
using IR;
using System.IO;
using P2B.SERVICES.Interface;
using P2B.SERVICES.Factory;



namespace P2BUltimate.Reports.Payroll
{
    public partial class ReportData : System.Web.UI.Page
    {
        readonly IP2BINI p2BINI;
        readonly REPORTSettings REPORT;

        public ReportData()
        {
            p2BINI = P2BINI.RegisterSettings();
            REPORT = new REPORTSettings(p2BINI.GetSectionValues("REPORTSettings"));
          
        }

         public class REPORTSettings
         {
             private string CompCode;
             private string PaySlip;
             private string SalRegister;

             public REPORTSettings(IDictionary<string, string> settinigs)
             {
                 if (settinigs.Count() > 0)
                 {
                     this.CompCode = settinigs.First(x => x.Key.Equals("CompCode")).Value;
                     this.PaySlip = settinigs.First(x => x.Key.Equals("PaySlip")).Value;
                     this.SalRegister = settinigs.First(x => x.Key.Equals("SalRegister")).Value;
                     
                 }

             }

             public string CCode { get { return CompCode; } }
             public string payslip { get { return PaySlip; } }
             public string salregister { get { return SalRegister; } }
         }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
            }
        }

        List<GenericField100> subreportdata;
        protected void ShowViewer_Click(object sender, EventArgs e)
        {
            var val = this.TextBox1.Value;
            var Paymnth = this.TextBox3.Value;
            if (val != "")
            {


                ReportfilterData reportdata = new ReportfilterData();
                reportdata = Reportfilter.Getreportdata(val, Paymnth);

                if (reportdata != null)
                {
                    dynamic reportdatawww1 = reportdata.p2bdatad;
                    string temp = Convert.ToString(reportdatawww1);

                    if (temp != "" && temp != null)
                    {
                        if (!temp.Equals("System.Collections.Generic.List`1[P2BUltimate.Models.Utility+ReportClass]"))
                        {
                            subreportdata = reportdatawww1;
                        }
                    }

                    // subreportdata = reportdatawww1;
                    //if ((REPORT.payslip== null ? "" : REPORT.payslip.ToUpper()) != (REPORT.salregister == null ? "" : REPORT.salregister.ToUpper()))
                    //{
                       
                    //}

                    //var CompCode = db.Company.SingleOrDefault();  ///ddd
                    using (DataBaseContext db = new DataBaseContext())
                    {
                        string CompCode = db.Company.Find(int.Parse(SessionManager.CompanyId)).Code;
                    if (reportdatawww1 != null && reportdatawww1.Count > 0)
                    {

                        if (REPORT != null && !string.IsNullOrEmpty(REPORT.CCode) && (REPORT.CCode.ToUpper() == CompCode.ToUpper()))
                        {
                          
                            if (reportdata.ReportName.ToUpper() == "PAYSLIP")
                            {
                                if (!string.IsNullOrEmpty(REPORT.payslip))
                                {
                                    reportdata.ReportName = REPORT.payslip;
                                }
                            }

                            if (reportdata.ReportName.ToUpper() == "SALREGISTER")
                            {
                                if (!string.IsNullOrEmpty(REPORT.salregister))
                                {
                                    reportdata.ReportName = REPORT.salregister;
                                }
                            }


                        }
                        
                        
                        ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Reports/" + reportdata.ModuleName + "/Rpt" + reportdata.ReportName + ".rdlc");
                        ReportViewer1.LocalReport.DataSources.Clear();
                        ReportDataSource rdc = null; // new ReportDataSource("P2BDataSet", reportdatawww1);
                        //List<GenericField100> MainReportDATA = new List<GenericField100>();
                        //MainReportDATA.Add(reportdatawww1[0]);
                        List<GenericField100> MainReportDATA = new List<GenericField100>();
                        if (temp != "" && temp != null)
                        {
                            if (!temp.Equals("System.Collections.Generic.List`1[P2BUltimate.Models.Utility+ReportClass]"))
                            {

                                if (reportdata.ReportName == "irvictim" || reportdata.ReportName == "irvictimcase")
                                {
                                    MainReportDATA.Add(reportdatawww1[0]);
                                    rdc = new ReportDataSource("P2BDataSet", MainReportDATA);
                                }
                                else
                                {
                                    rdc = new ReportDataSource("P2BDataSet", reportdatawww1);
                                }

                            }
                            else
                            {
                                rdc = new ReportDataSource("P2BDataSet", reportdatawww1);
                            }
                        }




                        //ReportDataSource rdc = new ReportDataSource("P2BDataSet", MainReportDATA);
                        Utility.ReportClass oReportClass = new Utility.ReportClass();
                        ReportParameter[] param = new ReportParameter[6];

                        param[0] = new ReportParameter("ReportCompanyName", oReportClass.ReportCompanyName, true);


                        ReportViewer1.LocalReport.EnableExternalImages = true;
                        Uri pathAsUri = new Uri(Server.MapPath("~/Content/Login/Images/PCBL.png"));
                        if (pathAsUri != null)
                        {
                            param[5] = new ReportParameter("ReportCompanyLogo", pathAsUri.AbsoluteUri);
                        }

                        param[1] = new ReportParameter("ReportCreatedBy", oReportClass.ReportCreatedBy, true);

                        string requiredPathchk = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                        bool existschk = System.IO.Directory.Exists(requiredPathchk);
                        string localPathchk;
                        if (!existschk)
                        {
                            string localPath = new Uri(requiredPathchk).LocalPath;
                            System.IO.Directory.CreateDirectory(localPath);
                        }
                        string pathchk = requiredPathchk + @"\ReportHeaderName" + ".ini";
                        localPathchk = new Uri(pathchk).LocalPath;

                        if (!System.IO.File.Exists(localPathchk))
                        {

                            using (var fs = new FileStream(localPathchk, FileMode.OpenOrCreate))
                            {
                                StreamWriter str = new StreamWriter(fs);
                                str.BaseStream.Seek(0, SeekOrigin.Begin);

                                str.Flush();
                                str.Close();
                                fs.Close();
                            }


                        }



                        if (reportdata.ReportName.ToUpper() != "EMPEIS")
                        {
                            if (reportdata.forithead.Count > 0)
                            {
                                string rpthdr = "";
                                string chk = reportdata.monthlyforrptname != null ? reportdata.monthlyforrptname : "";
                                if (chk != "")
                                // if (chk != "" && reportdata.TypewiseName != null)
                                {
                                    rpthdr = reportdata.ReportName.ToUpper() + " " + reportdata.TypewiseName.ToUpper() + "WISE ON " + chk;

                                    //ini code start
                                    using (var streamReader = new StreamReader(localPathchk))
                                    {
                                        string line;
                                        while ((line = streamReader.ReadLine()) != null)
                                        {
                                            string orignalreportname = line.Split('_')[0];
                                            string replaceheadername = line.Split('_')[1];
                                            if (orignalreportname.ToUpper() == reportdata.ReportName.ToUpper())
                                            {
                                                rpthdr = replaceheadername + " " + reportdata.TypewiseName.ToUpper() + "WISE ON " + chk;
                                            }
                                        }
                                    }
                                    //ini code end
                                }
                                else
                                {
                                    rpthdr = reportdata.ReportName.ToUpper() + " " + reportdata.TypewiseName.ToUpper() + "WISE REPORT ";

                                    //ini code start
                                    using (var streamReader = new StreamReader(localPathchk))
                                    {
                                        string line;
                                        while ((line = streamReader.ReadLine()) != null)
                                        {
                                            string orignalreportname = line.Split('_')[0];
                                            string replaceheadername = line.Split('_')[1];
                                            if (orignalreportname.ToUpper() == reportdata.ReportName.ToUpper())
                                            {
                                                rpthdr = replaceheadername + " " + reportdata.TypewiseName.ToUpper() + "WISE REPORT ";
                                            }
                                        }
                                    }
                                    //ini code end
                                }
                                param[4] = new ReportParameter("ReportHeaderName", rpthdr, true);
                            }
                            else
                            {
                                string rpthdr = "";
                                string chk = reportdata.monthlyforrptname != null ? reportdata.monthlyforrptname : "";
                                if (chk != "")
                                {
                                    rpthdr = reportdata.ReportName.ToUpper() + " FOR " + chk;
                                    //ini code start
                                    using (var streamReader = new StreamReader(localPathchk))
                                    {
                                        string line;
                                        while ((line = streamReader.ReadLine()) != null)
                                        {
                                            string orignalreportname = line.Split('_')[0];
                                            string replaceheadername = line.Split('_')[1];
                                            if (orignalreportname.ToUpper() == reportdata.ReportName.ToUpper())
                                            {
                                                rpthdr = replaceheadername + " FOR " + chk;
                                            }
                                        }
                                    }
                                    //ini code end
                                }
                                else
                                {
                                    rpthdr = reportdata.ReportName.ToUpper();

                                    //ini code start
                                    using (var streamReader = new StreamReader(localPathchk))
                                    {
                                        string line;
                                        while ((line = streamReader.ReadLine()) != null)
                                        {
                                            string orignalreportname = line.Split('_')[0];
                                            string replaceheadername = line.Split('_')[1];
                                            if (orignalreportname.ToUpper() == reportdata.ReportName.ToUpper())
                                            {
                                                rpthdr = replaceheadername + " FOR " + chk;
                                            }
                                        }
                                    }
                                    //ini code end
                                }


                                param[4] = new ReportParameter("ReportHeaderName", rpthdr, true);
                            }

                        }
                        if (reportdata.ReportName.ToUpper() == "EMPLOYEEINFORMATION")
                        {
                            param[2] = new ReportParameter("Field1", reportdata.HeadList[0].Split('.')[reportdata.HeadList[0].Split('.').Length - 1], true);
                            if (reportdata.HeadList.Count == 2)
                            {
                                param[3] = new ReportParameter("Field2", reportdata.HeadList[1].Split('.')[reportdata.HeadList[0].Split('.').Length - 1], true);
                            }
                            this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { param[0], param[1], param[2], param[3], param[4], param[5] });
                        }
                        else
                        {
                            if (reportdata.ReportName.ToUpper() != "EMPEIS")
                            {
                                if (reportdata.forithead.Count > 0)
                                {
                                    this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { param[0], param[1], param[4], param[5] });
                                }
                                else
                                {
                                    this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { param[0], param[1], param[4], param[5] });
                                }
                            }
                            else
                            {
                                this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { param[0], param[1], param[5] });
                            }

                        }
                        this.ReportViewer1.LocalReport.DataSources.Add(rdc);
                        //   reportdatawww1 = new List<dynamic>();
                        this.ReportViewer1.LocalReport.Refresh();

                        ReportViewer1.LocalReport.SubreportProcessing +=

                        new Microsoft.Reporting.WebForms.SubreportProcessingEventHandler(LocalReport_SubreportProcessing);

                        ReportViewer1.LocalReport.Refresh();
                    }
                    else
                    {
                        string script = "alert(\"No Data Found\");";
                        ScriptManager.RegisterStartupScript(this, GetType(), "ServerControlScript", script, true);
                    }
                }
            }
                else
                {
                    string script = "alert(\"No Data Found\");";
                    ScriptManager.RegisterStartupScript(this, GetType(), "ServerControlScript", script, true);

                }
            }

            var NewObj = new NameValueCollection();
            var val1 = this.textbox2.Value;
            if (val1 != "")
            {
                var ModuleName = HttpContext.Current.Session["ModuleType"];
                dynamic Gen100 = null;
                var ReportName = Request.QueryString["ReportName"];
                var employeeids = Request.QueryString["Employee"];
                if (ReportName.ToUpper() == "PENDINGLEAVECREDIT" || ReportName.ToUpper() == "LEAVECREDITTRAILBALANCE")
                {              	
                NewObj = HttpUtility.ParseQueryString(val1);
                var Leavehead = (String)null;
                Leavehead = NewObj["LvHead"];

                var salheadlist = new List<string>();
                if (Leavehead != null)
                {
                    var salrheads = new List<string>();
                    salrheads = Utility.StringIdsToListString(Leavehead);
                    foreach (var item in salrheads)
                    {
                        salheadlist.Add(Convert.ToString(item));
                    }
                }
                var emp = new List<int>();
                if (employeeids != null)
                {
                    emp = Utility.StringIdsToListIds(employeeids);
                }
                var z = new List<int>();
                foreach (var item in emp)
                {
                    z.Add(Convert.ToInt32(item));
                }

                var Creditdatelist = new List<string>();
                var Creditdate = (String)null;
                Creditdate = NewObj["CreditDatelist"];
                if (Creditdate != null)
                {
                    Creditdatelist.Add(Creditdate);
                }
                
                DateTime mFromDate = DateTime.Now;
                DateTime mToDate = DateTime.Now;
                DateTime pFromDate = DateTime.Now;
                DateTime pToDate = DateTime.Now;
                
                var LvFromdate1 = Request.QueryString["FromDate"];
                var LvTodate1 = Request.QueryString["ToDate"];

                DateTime? LvFromdate = null;
                DateTime? LvTodate = null;
                Boolean settlementemp = false;
                var settlementempstr = Request.QueryString["settlementemp"];
                if (settlementempstr != "")
                {
                    if (settlementempstr == "1")
                    {
                        settlementemp = true;
                    }

                }

                if (LvFromdate1 != "" && LvTodate1 != "")
                {
                    LvFromdate = Convert.ToDateTime(LvFromdate1);
                    LvTodate = Convert.ToDateTime(LvTodate1);
                }
                else
                {
                    LvFromdate = null;
                    LvTodate = null;
                }

                if (ReportName == "PENDINGLEAVECREDIT" || ReportName=="LEAVECREDITTRAILBALANCE")
                {
                    dynamic temp = System.Web.HttpContext.Current.Session["EmployeeLvIdReport"];
                    foreach (var emplvid in temp)
                    {
                        int ids1 = Convert.ToInt32(emplvid);
                        z.Add(ids1);
                    }                   
                }
                if (ReportName == "PENDINGLEAVECREDIT")
                {
                    Gen100 = LeaveReportGen.GenerateLeaveReport(Convert.ToInt32(SessionManager.CompLvId), ReportName.ToUpper(), z, Convert.ToDateTime(mFromDate), Convert.ToDateTime(mToDate), null, null, null, salheadlist, Creditdatelist, Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), Convert.ToDateTime(LvFromdate), Convert.ToDateTime(LvTodate), Convert.ToDateTime(LvTodate), settlementemp, "");
                }
                else
                {
                    Gen100 = LeaveReportGen.GenerateLeaveReport(Convert.ToInt32(SessionManager.CompLvId), ReportName.ToUpper(), z, Convert.ToDateTime(mFromDate), Convert.ToDateTime(mToDate), null, null, null, salheadlist, Creditdatelist, Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), Convert.ToDateTime(LvFromdate), Convert.ToDateTime(LvTodate), Convert.ToDateTime(LvTodate), settlementemp, "");

                }
                dynamic P2BData = Gen100;
                if (P2BData != null && P2BData.Count > 0)
                {
                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Reports/" + "Leave" + "/Rpt" + ReportName + ".rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportDataSource rdc = new ReportDataSource("P2BDataSet", P2BData);
                    Utility.ReportClass oReportClass = new Utility.ReportClass();
                    ReportParameter[] param = new ReportParameter[6];

                    param[0] = new ReportParameter("ReportCompanyName", oReportClass.ReportCompanyName, true);


                    ReportViewer1.LocalReport.EnableExternalImages = true;
                    Uri pathAsUri = new Uri(Server.MapPath("~/Content/Login/Images/PCBL.png"));
                    if (pathAsUri != null)
                    {
                        param[5] = new ReportParameter("ReportCompanyLogo", pathAsUri.AbsoluteUri);
                    }

                    param[1] = new ReportParameter("ReportCreatedBy", oReportClass.ReportCreatedBy, true);
                    string rpthdr = ReportName.ToUpper();
                    param[4] = new ReportParameter("ReportHeaderName", rpthdr, true);
                    this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { param[0], param[1], param[4], param[5] });                   
                    this.ReportViewer1.LocalReport.DataSources.Add(rdc);                  
                    this.ReportViewer1.LocalReport.Refresh();
                }
                else
                {
                    string script = "alert(\"No Data Found\");";
                    ScriptManager.RegisterStartupScript(this, GetType(), "ServerControlScript", script, true);
                }
                }         
            }
        }


        // *******************

        int j = 0;
        void LocalReport_SubreportProcessing(object sender, Microsoft.Reporting.WebForms.SubreportProcessingEventArgs e)
        {


            int iStageNo = Convert.ToInt32(e.Parameters["Fld100"].Values[0]);
            int subIndex = iStageNo - 1;

            string aa = Convert.ToString(iStageNo);

            string reportDataSourceName = string.Empty;
            object reportDataSourceValue = null;

            // remove all previously attached Datasources, since we want to attach a
            // new one
            e.DataSources.Clear();

            if (!String.IsNullOrEmpty(e.ReportPath))
            {
                List<GenericField100> Temp = new List<GenericField100>();
                reportDataSourceName = "P2BDataSet";
                reportDataSourceValue = Temp;
                e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
            }

            List<GenericField100> subtest = new List<GenericField100>();

            //List<GenericField100> Exceptsubtest = new List<GenericField100>();
            //Exceptsubtest.Add(subreportdata[0]);

            //// skip the 1st value of List subSet because 1st is for "mainReport" value.
            //var list1 = subreportdata.Except(Exceptsubtest);
            bool flag = false;
            foreach (var item1 in subreportdata)
            {
                subtest.Add(item1);
            }
            int subreportcount  = subtest.Count();
            

            #region FOR LOOP START
            //int i = 0;
            //for (i = j; i < subtest.Count(); i++)
            //{
            
            for (int i = 0; i < subtest.Count(); i++)
            {
                if (flag)
                {
                    break;
                }
                switch (e.ReportPath)
                {
                    case "misconductSubreport":
                        if (subtest[i].Fld100 == "1")
                        {
                            e.DataSources.Clear();
                            j = 1;
                            flag = true;
                            List<GenericField100> G1 = new List<GenericField100>();
                            G1.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G1;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue != null ? reportDataSourceValue : null
                            });
                        }

                        else
                        {
                            List<GenericField100> G1 = new List<GenericField100>();
                            //G1.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G1;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });

                        }


                        break;

                    case "preliminaryenquirySubreport":
                        if (subtest[i].Fld100 == "2")
                        {
                            e.DataSources.Clear();
                            j = 2;
                            flag = true;
                            List<GenericField100> G2 = new List<GenericField100>();
                            G2.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G2;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue != null ? reportDataSourceValue : null
                            });
                        }
                        else
                        {
                            List<GenericField100> G2 = new List<GenericField100>();
                            //G2.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G2;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }

                        break;

                    case "preliminaryenquiryactionSubreport":
                        if (subtest[i].Fld100 == "3")
                        {
                            e.DataSources.Clear();
                            j = 3;
                            flag = true;
                            List<GenericField100> G3 = new List<GenericField100>();
                            G3.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G3;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        else
                        {
                            List<GenericField100> G3 = new List<GenericField100>();
                            //G3.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G3;
                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        break;

                    case "chargesheetSubreport":
                        if (subtest[i].Fld100 == "4")
                        {
                            e.DataSources.Clear();
                            j = 4;
                            flag = true;
                            List<GenericField100> G4 = new List<GenericField100>();
                            G4.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G4;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        else
                        {
                            List<GenericField100> G4 = new List<GenericField100>();
                            //G4.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G4;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }


                        break;

                    case "chargesheetservingSubreport":
                        if (subtest[i].Fld100 == "5")
                        {
                            e.DataSources.Clear();
                            j = 5;
                            flag = true;
                            List<GenericField100> G5 = new List<GenericField100>();
                            G5.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G5;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        else
                        {
                            List<GenericField100> G5 = new List<GenericField100>();
                            //G5.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G5;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        break;

                    case "chargesheetreplySubreport":
                        if (subtest[i].Fld100 == "6")
                        {
                            e.DataSources.Clear();
                            j = 6;
                            flag = true;
                            List<GenericField100> G6 = new List<GenericField100>();
                            G6.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G6;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        else
                        {

                            List<GenericField100> G6 = new List<GenericField100>();
                            // G6.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G6;
                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        break;


                    case "chargesheetenquirynoticeSubreport":
                        if (subtest[i].Fld100 == "7")
                        {
                            e.DataSources.Clear();
                            j = 7;
                            flag = true;
                            List<GenericField100> G7 = new List<GenericField100>();
                            G7.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G7;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        else
                        {
                            List<GenericField100> G7 = new List<GenericField100>();
                            //G7.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G7;
                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        break;

                    case "chargesheetenquirynoticeservingSubreport":
                        if (subtest[i].Fld100 == "8")
                        {
                            e.DataSources.Clear();
                            j = 8;
                            flag = true;
                            List<GenericField100> G8 = new List<GenericField100>();
                            G8.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G8;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        else
                        {
                            List<GenericField100> G8 = new List<GenericField100>();
                            //G8.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G8;

                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        break;

                    case "chargesheetenquiryproceedingSubreport":
                        if (subtest[i].Fld100 == "9")
                        {
                            e.DataSources.Clear();
                            j = 9;
                            flag = true;
                            List<GenericField100> G9 = new List<GenericField100>();
                            G9.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G9;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        else
                        {
                            List<GenericField100> G9 = new List<GenericField100>();
                            //G9.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G9;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        break;

                    case "chargesheetenquiryreportSubreport":
                        if (subtest[i].Fld100 == "10")
                        {
                            e.DataSources.Clear();
                            j = 10;
                            flag = true;
                            List<GenericField100> G10 = new List<GenericField100>();
                            G10.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G10;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        else
                        {
                            List<GenericField100> G10 = new List<GenericField100>();
                            // G10.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G10;
                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        break;


                    case "postenquiryprequisiteSubreport":
                        if (subtest[i].Fld100 == "11")
                        {
                            e.DataSources.Clear();
                            j = 11;
                            flag = true;
                            List<GenericField100> G11 = new List<GenericField100>();
                            G11.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G11;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        else
                        {
                            List<GenericField100> G11 = new List<GenericField100>();
                            //G11.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G11;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });

                        }
                        break;


                    case "finalshowcausenoticeSubreport":
                        if (subtest[i].Fld100 == "12")
                        {
                            e.DataSources.Clear();
                            j = 12;
                            flag = true;
                            List<GenericField100> G12 = new List<GenericField100>();
                            G12.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G12;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        else
                        {
                            List<GenericField100> G12 = new List<GenericField100>();
                            //G12.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G12;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }

                        break;

                    case "finalshowcausenoticeservingSubreport":
                        if (subtest[i].Fld100 == "13")
                        {
                            e.DataSources.Clear();
                            j = 13;
                            flag = true;
                            List<GenericField100> G13 = new List<GenericField100>();
                            G13.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G13;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        else
                        {

                            List<GenericField100> G13 = new List<GenericField100>();
                            // G13.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G13;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue

                            });


                        }
                        break;

                    case "finalshowcausenoticereplySubreport":
                        if (subtest[i].Fld100 == "14")
                        {
                            e.DataSources.Clear();
                            j = 14;
                            flag = true;
                            List<GenericField100> G14 = new List<GenericField100>();
                            G14.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G14;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        else
                        {
                            List<GenericField100> G14 = new List<GenericField100>();
                            // G14.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G14;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }

                        break;

                    case "finalshowcausenoticeclarificationSubreport":
                        if (subtest[i].Fld100 == "15")
                        {
                            e.DataSources.Clear();
                            j = 15;
                            flag = true;
                            List<GenericField100> G15 = new List<GenericField100>();
                            G15.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G15;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        else
                        {
                            List<GenericField100> G15 = new List<GenericField100>();
                            //G15.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G15;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }

                        break;

                    case "finalshowcausenoticeclarificationservingSubreport":
                        if (subtest[i].Fld100 == "16")
                        {
                            e.DataSources.Clear();
                            j = 16;
                            flag = true;
                            List<GenericField100> G16 = new List<GenericField100>();
                            G16.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G16;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        else
                        {
                            List<GenericField100> G16 = new List<GenericField100>();
                            //G16.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G16;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        break;

                    case "punishmentorderSubreport":
                        if (subtest[i].Fld100 == "17")
                        {
                            e.DataSources.Clear();
                            j = 17;
                            flag = true;
                            List<GenericField100> G17 = new List<GenericField100>();
                            G17.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G17;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        else
                        {
                            List<GenericField100> G17 = new List<GenericField100>();
                            //G17.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G17;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        break;

                    case "punishmentorderdeliverySubreport":
                        if (subtest[i].Fld100 == "18")
                        {
                            e.DataSources.Clear();
                            j = 18;
                            flag = true;
                            List<GenericField100> G18 = new List<GenericField100>();
                            G18.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G18;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        else
                        {
                            List<GenericField100> G18 = new List<GenericField100>();
                            //G18.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G18;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        break;

                    case "punishmentorderappealSubreport":
                        if (subtest[i].Fld100 == "19")
                        {
                            e.DataSources.Clear();
                            j = 19;
                            flag = true;
                            List<GenericField100> G19 = new List<GenericField100>();
                            G19.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G19;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        else
                        {
                            List<GenericField100> G19 = new List<GenericField100>();
                            //G19.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G19;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        break;

                    case "punishmentorderappealreplySubreport":
                        if (subtest[i].Fld100 == "20")
                        {
                            e.DataSources.Clear();
                            j = 20;
                            flag = true;
                            List<GenericField100> G20 = new List<GenericField100>();
                            G20.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G20;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        else
                        {

                            List<GenericField100> G20 = new List<GenericField100>();
                            // G20.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G20;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        break;

                    case "punishmentorderimplementationSubreport":
                        if (subtest[i].Fld100 == "21")
                        {
                            e.DataSources.Clear();
                            j = 21;
                            flag = true;
                            List<GenericField100> G21 = new List<GenericField100>();
                            G21.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G21;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        else
                        {
                            List<GenericField100> G21 = new List<GenericField100>();
                            //G21.Add(subtest[i]);
                            reportDataSourceName = "P2BDataSet";
                            reportDataSourceValue = G21;


                            e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
                            {
                                Name = reportDataSourceName,
                                Value = reportDataSourceValue
                            });
                        }
                        break;


                }
            }




            //using (DataBaseContext db = new DataBaseContext())
            //{
            //    int iOrderNo = Convert.ToInt32(e.Parameters["Fld100"].Values[0]);

            //    var Stages = db.EmpDisciplineProcedings.Where(s => s.ProceedingStage == iOrderNo).Select(m => m.MisconductComplaint).FirstOrDefault();


            //    // remove all previously attached Datasources, since we want to attach a
            //    // new one
            //    e.DataSources.Clear();

            //    // Retrieve employeeFamily list based on EmpID
            //    //var lines = db.MisconductComplaint.Where(i => i.Id == Stages.Id).FirstOrDefault();
            //    //List<GenericField100> subtest = new List<GenericField100>();
            //    //foreach (var item1 in subreportdata)
            //    //{
            //    //    subtest.Add(item1);
            //    //}
            //    List<GenericField100> Exceptsubtest = new List<GenericField100>();

            //     Exceptsubtest.Add(subreportdata[0]);
            //    List<GenericField100> subtest = new List<GenericField100>();
            //    // skip the 1st value of List subSet because 1st is for "mainReport" value.
            //    var list1 = subreportdata.Except(Exceptsubtest);

            //     foreach (var item1 in list1)
            //    {
            //        subtest.Add(item1);
            //    }
            //    //subtest.Skip(1);

            //        ReportDataSource rdcsub = new ReportDataSource("P2BDataSet", subtest);
            //        e.DataSources.Add(rdcsub);

            //    //List<GenericField100> subtest = subreportdata[1];
            //    //ReportDataSource rdcsub = new ReportDataSource("P2BDataSet", subtest);
            //    //e.DataSources.Add(rdcsub);
            //    // add retrieved dataset or you can call it list to data source
            //    //e.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource()
            //    //{
            //    //    Name = "P2BDataSet",
            //    //    Value = subreportdata[1]
            //    //});

            //}





            #endregion

        }


    }
}