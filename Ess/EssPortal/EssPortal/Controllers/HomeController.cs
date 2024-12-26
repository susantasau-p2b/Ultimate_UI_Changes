using EssPortal.App_Start;
using P2b.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using EssPortal.Models;
using EssPortal.Security;
using Newtonsoft.Json;
using EssPortal.Process;
using System.IO;

namespace EssPortal.Controllers
{
    [AuthoriseManger]
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            Session["TempEmpId"] = SessionManager.EmpId; 
            return View();
        }
        //public JsonResult GetJsondata()
        //{
        //    EssPortal.Models.ChartModel.ChartDataSource chartdatasource = new EssPortal.Models.ChartModel.ChartDataSource();
        //    EssPortal.Models.ChartModel.ChartParameter chartparameter = new EssPortal.Models.ChartModel.ChartParameter();
        //    //chartparameter.theme = "fint";
        //    chartparameter.caption = "Monthly Revenue";
        //    //   chartparameter.bgColor = "#F8F8F8";
        //    chartparameter.xAxisName = "Month";
        //    chartparameter.yAxisName = "Amount (In USD)";
        //    chartparameter.numberPrefix = "$";
        //    chartparameter.subCaption = "Last year";

        //    chartdatasource.chart = chartparameter;

        //    var data = new List<EssPortal.Models.ChartModel.Data>();
        //    data.Add(new EssPortal.Models.ChartModel.Data { label = "jan", value = "420000" });
        //    data.Add(new EssPortal.Models.ChartModel.Data { label = "Feb", value = "810000" });

        //    chartdatasource.Data = data.ToArray();
        //    var output = JsonConvert.SerializeObject(chartdatasource);
        //    return Json(output, JsonRequestBehavior.AllowGet);

        //}
        public class returndataclass
        {
            public string parent { get; set; }
            public Array child { get; set; }
        }
        public ActionResult Getdetails()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var data = db.Lookup.Include(e => e.LookupValues).ToList();
                var returnlist = new List<returndataclass>();
                foreach (var item in data)
                {
                    returnlist.Add(new returndataclass
                    {
                        parent = item.Name,
                        child = item.LookupValues.Select(e => e.LookupVal).ToArray(),
                    });
                }
                return Json(new { status = true, data = returnlist, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult SetModel(string data, string autho)
        {
            //data -modulename
            //autho drop down val
            var EmpId = 0;
            var CompId = 0;
            if (!string.IsNullOrEmpty(SessionManager.EmpId))
            {
                EmpId = Convert.ToInt32(SessionManager.EmpId);
                CompId = Convert.ToInt32(SessionManager.CompanyId);
            }
            else
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
            if (!String.IsNullOrEmpty(data) && data == "home")
            {
                Session["user-module"] = data;
                return Json(new { status = true }, JsonRequestBehavior.AllowGet);
            }

            if (!String.IsNullOrEmpty(autho) && autho == "MySelf")
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var OEmployee = db.Employee
                      .Include(e => e.EmpName)
                      .Include(e => e.FuncStruct)
                      .Include(e => e.ReportingStructRights.Select(r => r.AccessRights))
                      .Include(e => e.ReportingStructRights.Select(r => r.FuncModules))
                      .Where(e => e.Id == EmpId)
                      .SingleOrDefault();

                    var FuncModules = OEmployee.ReportingStructRights.Select(e => e.FuncModules.LookupVal).ToList();
                    if (FuncModules.Contains(data.ToUpper()))
                    {
                        Session["user-module"] = data;
                        Session["auho"] = autho;
                        return Json(new { status = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            List<ReportingStructRights> oReportingStructRights = PortalRights.ScanRights(0, EmpId, CompId);
            if (oReportingStructRights.Count == 0)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }

            var Reportingstruct = oReportingStructRights.Where(e => e.FuncModules.LookupVal.ToUpper() == data.ToUpper() && e.AccessRights.ActionName.LookupVal.ToUpper() == autho.ToUpper()).ToList();
            if (Reportingstruct != null && Reportingstruct.Count() > 0)
            {
                //bool leavclose = false;
                //if (Reportingstruct.AccessRights.IsClose == true)
                //{
                //    leavclose = true;
                //}
                Session["user-module"] = data;
                Session["auho"] = autho;
                return Json(new { status = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetTTProjectionMonths()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (SessionManager.EmpId == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var self = Convert.ToInt32(SessionManager.EmpId);

                //List<String> oPayMonth = new List<String>();
                var OPayslipData = db.EmployeePayroll
                    .Include(e => e.Employee)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.ITProjection)
                            .Include(e => e.ITProjection.Select(r => r.FinancialYear))
                    .Where(e => e.Employee.Id == self).SingleOrDefault();
                if (OPayslipData == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (OPayslipData.ITProjection != null && OPayslipData.ITProjection.Count() != 0)
                    {
                        List<ELMSController.AttachDataClass> ListPayMonth = new List<ELMSController.AttachDataClass>();
                        foreach (var ca1 in OPayslipData.ITProjection.OrderByDescending(e => e.Id))
                        {

                            ListPayMonth.Add(new ELMSController.AttachDataClass { Id = ca1.FinancialYear.Id.ToString(), val = ca1.FinancialYear.FullDetails, Empname = OPayslipData.Employee.EmpName.FullNameFML });
                        }
                        var a = ListPayMonth.GroupBy(e => e.EmpId).Select(e => e.First()).ToList();
                        return Json(new { status = true, data = a }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }

        public JsonResult GetItform16Months()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (SessionManager.EmpId == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var self = Convert.ToInt32(SessionManager.EmpId);

                //List<String> oPayMonth = new List<String>();
                var OPayslipData = db.EmployeePayroll
                    .Include(e => e.Employee)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.ITForm16Data)
                            .Include(e => e.ITProjection.Select(r => r.FinancialYear))
                    .Where(e => e.Employee.Id == self).SingleOrDefault();
                if (OPayslipData == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (OPayslipData.ITForm16Data != null && OPayslipData.ITForm16Data.Count() != 0)
                    {
                        List<ELMSController.AttachDataClass> ListPayMonth = new List<ELMSController.AttachDataClass>();
                        foreach (var ca1 in OPayslipData.ITForm16Data.Where(e => e.IsLocked == true).OrderByDescending(e => e.Id))
                        {

                            ListPayMonth.Add(new ELMSController.AttachDataClass { Id = ca1.FinancialYear.Id.ToString(), val = ca1.FinancialYear.FullDetails, Empname = OPayslipData.Employee.EmpName.FullNameFML });
                        }
                        var a = ListPayMonth.GroupBy(e => e.EmpId).Select(e => e.First()).ToList();
                        return Json(new { status = true, data = a }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }
		
        public ActionResult GetEmpPdf(string forwardata, string forwardata1, string forwardata2, string PDFname)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int empid = Convert.ToInt32(forwardata);
                string EmpCode = db.Employee.Where(e => e.Id == empid).Select(e => e.EmpCode).FirstOrDefault();
                //var EmpCode = forwardata;
                //var EmpCode = db.Employee.Where(e => e.Id == Empid).FirstOrDefault().EmpCode;
                string FolderName = "FromPeriod " + Convert.ToDateTime(forwardata1).ToShortDateString() + " To " + Convert.ToDateTime(forwardata2).ToShortDateString();

                string FolderName1 = FolderName.Replace("/", "-")+"\\";


                string requiredPath = string.Empty;
                if (PDFname == "ITForm16")
                {
                     requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(
                                  System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Form16\\" + FolderName1; ///empcode.pdf
                }

                if (PDFname == "ITForm12BA")
                {
                    requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(
                        System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\FORM12BA\\" + FolderName1; 
                }
                string localPath = new Uri(requiredPath).LocalPath;

                String[] allfiles = System.IO.Directory.GetFiles(localPath, "*" + EmpCode + ".pdf*", System.IO.SearchOption.AllDirectories);
                if (allfiles.Length == 0)
                {
                    return JavaScript("alert('No data Found..!!')");
                }
                string path = allfiles[0];

                FileInfo file = new FileInfo(path);
                bool exists = file.Exists;
                string extension = Path.GetExtension(file.Name);

                if (exists)
                {
                    if (extension == ".pdf")
                    {
                        return File(file.FullName, "application/pdf", file.Name + " ");
                    }
                }
                return null;
            }
        }

        public JsonResult GetAnualSalaryMonths()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (SessionManager.EmpId == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var self = Convert.ToInt32(SessionManager.EmpId);

                //List<String> oPayMonth = new List<String>();
                var OPayslipData = db.EmployeePayroll
                            .Include(e => e.AnnualSalary).Include(e => e.AnnualSalary.Select(a => a.FinancialYear))
                  .Where(e => e.Employee.Id == self).SingleOrDefault();
                if (OPayslipData == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (OPayslipData.AnnualSalary != null && OPayslipData.AnnualSalary.Count() != 0)
                    {
                        List<ELMSController.AttachDataClass> ListPayMonth = new List<ELMSController.AttachDataClass>();
                        foreach (var ca1 in OPayslipData.AnnualSalary.OrderByDescending(e => e.Id))
                        {
                            ListPayMonth.Add(new ELMSController.AttachDataClass { Id = ca1.FinancialYear.Id.ToString(), val = ca1.FinancialYear.FullDetails });
                        }
                        var a = ListPayMonth.GroupBy(e => e.EmpId).Select(e => e.First()).ToList();
                        return Json(new { status = true, data = a }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }



        public JsonResult GetPayslipMonths()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (SessionManager.EmpId == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var self = Convert.ToInt32(SessionManager.EmpId);

                //List<String> oPayMonth = new List<String>();
                var OPayslipData = db.EmployeePayroll
                    .Include(e => e.SalaryT.Select(r => r.PayslipR))
                    .Include(e => e.SalaryT.Select(r => r.PayslipR.Select(t => t.PaySlipDetailEarnR)))
                    .Include(e => e.SalaryT.Select(r => r.PayslipR.Select(t => t.PaySlipDetailDedR)))
                    .Include(e => e.SalaryT.Select(r => r.PayslipR.Select(t => t.PaySlipDetailLeaveR)))
                    .Where(e => e.Employee.Id == self).SingleOrDefault();
                if (OPayslipData == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (OPayslipData.SalaryT != null && OPayslipData.SalaryT.Count() != 0)
                    {
                        List<ELMSController.AttachDataClass> ListPayMonth = new List<ELMSController.AttachDataClass>();
                        var item1 = OPayslipData.SalaryT.Where(e => e.ReleaseDate != null).ToList();
                        foreach (var ca1 in item1)
                        {
                            if (ca1.PayslipR != null && ca1.PayslipR.Count() != 0)
                            {
                                foreach (var item in ca1.PayslipR.OrderByDescending(e => e.Id))
                                {
                                    ListPayMonth.Add(new ELMSController.AttachDataClass { Id = item.PayMonth, val = item.PayMonth, Empname = item.EmpName });
                                }
                            }
                        }
                        ListPayMonth.Reverse();
                        return Json(new { status = true, data = ListPayMonth }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }
        public ActionResult GetEmpPayslip(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    var lookupdata = db.Lookup
                            .Include(e => e.LookupValues)
                            .Where(e => e.Code == "601").SingleOrDefault();
                    FuncModule = lookupdata.LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                if (EmpIds == null && EmpIds.Count == 0)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var OPayslipData = db.EmployeePayroll
                    .Include(e => e.Employee)
                     .Include(e => e.SalaryT.Select(r => r.PayslipR))
                     .Include(e => e.SalaryT.Select(r => r.PayslipR.Select(t => t.PaySlipDetailEarnR)))
                     .Include(e => e.SalaryT.Select(r => r.PayslipR.Select(t => t.PaySlipDetailDedR)))
                     .Include(e => e.SalaryT.Select(r => r.PayslipR.Select(t => t.PaySlipDetailLeaveR)));
                /// .Where(e => EmpIds.Contains(e.Employee.Id))
                //  .ToList();

                var emps = OPayslipData.Where(e => EmpIds.Contains(e.Employee.Id)).AsNoTracking().ToList();
                List<ELMSController.AttachDataClass> ListPayMonth = new List<ELMSController.AttachDataClass>();
                foreach (var ca in emps)
                {
                    if (ca.SalaryT != null && ca.SalaryT.Count() != 0)
                    {

                        var paysliplast = ca.SalaryT.Where(e => e.PayMonth == data && e.ReleaseDate != null).ToList();
                        foreach (var ca1 in paysliplast)
                        {
                            if (ca1.PayslipR != null)
                            {
                                var item1 = ca1.PayslipR.OrderByDescending(e => e.Id).LastOrDefault();
                                string empdetails = item1.PayMonth + "  " + item1.EmpCode;
                                // foreach (var item in item1)
                                // {
                                ListPayMonth.Add(new ELMSController.AttachDataClass { Id = item1.PayMonth, EmpId = ca.Employee.Id, val = empdetails, Empname = item1.EmpName });
                                // }
                            }
                        }

                    }
                }
                ListPayMonth.Reverse();
                return Json(new { status = true, data = ListPayMonth }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetEmpItprojection(string data, string data1, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    var lookupdata = db.Lookup
                           .Include(e => e.LookupValues)
                           .Where(e => e.Code == "601").SingleOrDefault();
                    FuncModule = lookupdata.LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                if (EmpIds == null && EmpIds.Count == 0)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var OITPROJECT = db.EmployeePayroll
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.ITProjection)
                        .Include(e => e.ITProjection.Select(r => r.FinancialYear));
                //.Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                //.ToList();
                /// .Where(e => EmpIds.Contains(e.Employee.Id))
                //  .ToList();

                var emps = OITPROJECT.Where(e => EmpIds.Contains(e.Employee.Id)).AsNoTracking().ToList();
                List<ELMSController.AttachDataClass> ListPayMonth = new List<ELMSController.AttachDataClass>();
                foreach (var ca in emps)
                {
                    if (ca.ITProjection != null && ca.ITProjection.Count() != 0)
                    {
                        var mstart = Convert.ToDateTime(data1);//Convert.ToDateTime("01/" + (mFromDate.Take(1).SingleOrDefault().ToString())).Date;
                        var mend = Convert.ToDateTime(data2);//Convert.ToDateTime("01/" + (mPayMonth.Take(m).SingleOrDefault().ToString())).Date;
                        var ca1 = ca.ITProjection.Where(e => e.FinancialYear.FromDate.Value <= mstart && e.FinancialYear.ToDate.Value >= mend).LastOrDefault();
                        //foreach (var ca1 in ca1)
                        //{
                        if (ca1 != null)
                        {
                            if (ca1.FinancialYear != null)
                            {
                                // var item1 = ca1..OrderByDescending(e => e.Id).LastOrDefault();
                                string empdetails = ca.Employee.EmpCode + "  " + ca.Employee.EmpName.FullNameFML;
                                // foreach (var item in item1)
                                // {
                                ListPayMonth.Add(new ELMSController.AttachDataClass { Id = ca1.FinancialYear.Id.ToString(), EmpId = ca.Employee.Id, val = ca1.FinancialYear.FullDetails, Empname = empdetails });
                                // }
                            }
                        }
                        //}

                    }
                }
                ListPayMonth.Reverse();
                return Json(new { status = true, data = ListPayMonth }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetEmpSalary()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var EmpId = Convert.ToInt32(SessionManager.EmpId);
                EssPortal.Models.ChartModel.ChartDataSource chartdatasource = new EssPortal.Models.ChartModel.ChartDataSource();
                EssPortal.Models.ChartModel.ChartParameter chartparameter = new EssPortal.Models.ChartModel.ChartParameter();
                var data = new List<EssPortal.Models.ChartModel.Data>();
                var dataset = new List<EssPortal.Models.ChartModel.dataset2>();
                var label = new List<String>();
                var datas = new List<String>();
                var SalData = db.EmployeePayroll
                    .Where(e =>
                        e.Employee.Id == EmpId &&
                        e.Employee.ServiceBookDates.ServiceLastDate == null)
                   .Select(e => new
                   {
                       PayMonth = e.SalaryT.Where(x=>x.ReleaseDate!=null).OrderByDescending(t => t.Id).Select(a => a.PayMonth),

                   }).ToList();

                if (SalData != null)
                {
                    foreach (var item in SalData.SelectMany(e => e.PayMonth).Distinct().Take(4))
                    {
                        label.Add(item);
                    }
                }
                List<EssPortal.Models.ChartModel.dataset> odataset_TotalEarning = new List<EssPortal.Models.ChartModel.dataset>();
                //   var GradeList = SalData.SelectMany(a => a.PayMonth).ToList();
                var vTotalEarning = db.EmployeePayroll
                    .Where(e => e.Employee.Id == EmpId)
                    .Select(e => new
                    {
                        SalaryT = e.SalaryT.Where(a => label.Contains(a.PayMonth) && a.ReleaseDate!=null)
                    }).ToList();
                List<string> odata_TotalEarning = new List<string>();

                foreach (var item in vTotalEarning.SelectMany(e => e.SalaryT).OrderByDescending(a => a.Id))
                {
                    odata_TotalEarning.Add(item.TotalEarning.ToString());
                }
                dataset.Add(new ChartModel.dataset2
                {
                    label = "TotalEarning",
                    backgroundColor = "#c9cbcf",
                    borderColor = "#c9cbcf",
                    data = odata_TotalEarning.ToArray(),
                });
                var vTotalDeduction = db.EmployeePayroll
                 .Where(e => e.Employee.Id == EmpId
                 )
                 .Select(e => new
                 {
                     SalaryT = e.SalaryT.Where(a => label.Contains(a.PayMonth) && a.ReleaseDate!=null)
                 }).ToList();

                List<EssPortal.Models.ChartModel.dataset> odataset_TotalDeduction = new List<EssPortal.Models.ChartModel.dataset>();
                List<string> odata_TotalDeduction = new List<string>();

                foreach (var item in vTotalDeduction.SelectMany(e => e.SalaryT).OrderByDescending(a => a.Id))
                {
                    odata_TotalDeduction.Add(item.TotalDeduction.ToString());
                }
                dataset.Add(new ChartModel.dataset2
                {
                    label = "TotalDeduction",
                    data = odata_TotalDeduction.ToArray(),
                    backgroundColor = "#ff6384",
                    borderColor = "#ff6384",

                });
                var vTotalNet = db.EmployeePayroll
               .Where(e => e.Employee.Id == EmpId
               )
               .Select(e => new
               {
                   SalaryT = e.SalaryT.Where(a => label.Contains(a.PayMonth) && a.ReleaseDate!=null)
               }).ToList();

                List<EssPortal.Models.ChartModel.dataset> odataset_TotalNet = new List<EssPortal.Models.ChartModel.dataset>();
                List<string> odata_TotalNet = new List<string>();
                foreach (var item in vTotalNet.SelectMany(e => e.SalaryT).OrderByDescending(a => a.Id))
                {
                    odata_TotalNet.Add(item.TotalNet.ToString());
                }
                dataset.Add(new ChartModel.dataset2
                {
                    label = "TotalNet",
                    data = odata_TotalNet.ToArray(),
                    backgroundColor = "#36a2eb",
                    borderColor = "#36a2eb",

                });
                chartdatasource.datasets = dataset.ToArray();
                chartdatasource.labels = label.ToArray();
                var output = JsonConvert.SerializeObject(chartdatasource);
                return Json(output, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetSalSummry()
        {
            return Json(null, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetFunctAttendanceMonths()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (SessionManager.EmpId == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var self = Convert.ToInt32(SessionManager.EmpId);

                //List<String> oPayMonth = new List<String>();
                var OEmpolyeePayrollData = db.EmployeePayroll
                    .Include(e => e.FunctAttendanceT.Select(r => r.PayProcessGroup))
                   .Include(e => e.FunctAttendanceT.Select(r => r.SalaryHead))
                   .Include(e => e.FunctAttendanceT.Select(r => r.GeoStruct))
                   .Include(e => e.FunctAttendanceT.Select(r => r.FuncStruct))
                   .Include(e => e.FunctAttendanceT.Select(r => r.PayStruct))
                   .Include(e => e.FunctAttendanceT.Select(r => r.EmpSalStruct))
                    .Where(e => e.Employee.Id == self).SingleOrDefault();
                if (OEmpolyeePayrollData == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (OEmpolyeePayrollData.FunctAttendanceT != null && OEmpolyeePayrollData.FunctAttendanceT.Count() != 0)
                    {
                        List<ELMSController.AttachDataClass> ListPayMonth = new List<ELMSController.AttachDataClass>();
                        foreach (var ca1 in OEmpolyeePayrollData.FunctAttendanceT)
                        {
                            if (ca1 != null)
                            {

                                ListPayMonth.Add(new ELMSController.AttachDataClass { val = ca1.PayMonth });

                            }
                        }
                        ListPayMonth.Reverse();
                        return Json(new { status = true, data = ListPayMonth }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }
        public JsonResult GetITProjectionFinancialYear()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (SessionManager.EmpId == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var self = Convert.ToInt32(SessionManager.EmpId);

                //List<String> oPayMonth = new List<String>();
                var OEmpolyeeITProjectionData = db.EmployeePayroll
                   .Include(e => e.ITProjection)
                   .Include(e => e.ITProjection.Select(t => t.FinancialYear))
                    //.Include(e=>e.ITProjection.Select(t=>t.SalayHead))
                    .Where(e => e.Employee.Id == self).SingleOrDefault();
                if (OEmpolyeeITProjectionData == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (OEmpolyeeITProjectionData.ITProjection != null && OEmpolyeeITProjectionData.ITProjection.Count() != 0)
                    {
                        List<ELMSController.AttachDataClass> ListPayMonth = new List<ELMSController.AttachDataClass>();
                        foreach (var ca1 in OEmpolyeeITProjectionData.ITProjection)
                        {
                            if (ca1 != null)
                            {

                                ListPayMonth.Add(new ELMSController.AttachDataClass { val = ca1.FinancialYear.FullDetails });

                            }
                        }
                        ListPayMonth.Reverse();
                        return Json(new { status = true, data = ListPayMonth }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }
        public ActionResult GetCompImage()
        {
            var a = Utility.GetImage("company");
            if (!string.IsNullOrEmpty(a))
            {
                return Json(new { data = a, status = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetEmpImage()
        {
            var a = Utility.GetImage("employee");
            if (!string.IsNullOrEmpty(a))
            {
                JsonRequestBehavior behaviou = new JsonRequestBehavior();
                //return Json(new { data = a, status = true,ma }, JsonRequestBehavior.AllowGet);
                return new JsonResult()
                {
                    Data = a,
                    MaxJsonLength = 86753090,
                    JsonRequestBehavior = behaviou
                };

            }
            else
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetUserData()
        {
            var a = Utility.GetUserData();
            if (a != null)
            {
                return Json(new { data = a }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { data = a }, JsonRequestBehavior.AllowGet);
            }
        }
        public class GradeWiseEmp
        {
            public string Grade { get; set; }
            public Int32 EmpCount { get; set; }
        }
        public ActionResult GetEmpSalaryPayMonthWise()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    //FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                    FuncModule = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "601").FirstOrDefault()
                       .LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                if (EmpIds == null || EmpIds.Count == 0)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

                EssPortal.Models.ChartModel.ChartDataSource chartdatasource = new EssPortal.Models.ChartModel.ChartDataSource();
                EssPortal.Models.ChartModel.ChartParameter chartparameter = new EssPortal.Models.ChartModel.ChartParameter();
                var data = new List<EssPortal.Models.ChartModel.Data>();
                var dataset = new List<EssPortal.Models.ChartModel.dataset2>();
                //var category = new List<EssPortal.Models.ChartModel.category>();
                var label = new List<String>();
                var datas = new List<String>();
                var SalData = db.EmployeePayroll
                    .Where(e =>
              EmpIds.Contains(e.Employee.Id) &&
                   e.Employee.ServiceBookDates.ServiceLastDate == null)
                   .Select(e => new
                   {
                       PayMonth = e.SalaryT.Select(a => a.PayMonth),
                   }).ToList();

                if (SalData != null)
                {
                    foreach (var item in SalData.SelectMany(e => e.PayMonth).Distinct())
                    {
                        label.Add(item);
                    }
                }
                List<EssPortal.Models.ChartModel.dataset> odataset_TotalEarning = new List<EssPortal.Models.ChartModel.dataset>();
                //   var GradeList = SalData.SelectMany(a => a.PayMonth).ToList();
                var vTotalEarning = db.EmployeePayroll
                    .Where(e => EmpIds.Contains(e.Employee.Id))
                    .Select(e => new
                    {
                        SalaryT = e.SalaryT.Where(a => label.Contains(a.PayMonth))
                    }).ToList();
                List<string> odata_TotalEarning = new List<string>();

                foreach (var item in vTotalEarning.SelectMany(e => e.SalaryT).OrderByDescending(a => a.Id))
                {
                    odata_TotalEarning.Add(item.TotalEarning.ToString());
                }
                dataset.Add(new ChartModel.dataset2
                {
                    label = "TotalEarning",
                    backgroundColor = "#c9cbcf",
                    data = odata_TotalEarning.ToArray(),
                });
                var vTotalDeduction = db.EmployeePayroll
                 .Where(e => EmpIds.Contains(e.Employee.Id)
                 )
                 .Select(e => new
                 {
                     SalaryT = e.SalaryT.Where(a => label.Contains(a.PayMonth))
                 }).ToList();

                List<EssPortal.Models.ChartModel.dataset> odataset_TotalDeduction = new List<EssPortal.Models.ChartModel.dataset>();
                List<string> odata_TotalDeduction = new List<string>();

                foreach (var item in vTotalDeduction.SelectMany(e => e.SalaryT).OrderByDescending(a => a.Id))
                {
                    odata_TotalDeduction.Add(item.TotalDeduction.ToString());
                }
                dataset.Add(new ChartModel.dataset2
                {
                    label = "TotalDeduction",
                    data = odata_TotalDeduction.ToArray(),
                    backgroundColor = "#ff6384",
                });
                var vTotalNet = db.EmployeePayroll
               .Where(e => EmpIds.Contains(e.Employee.Id)
               )
               .Select(e => new
               {
                   SalaryT = e.SalaryT.Where(a => label.Contains(a.PayMonth))
               }).ToList();

                List<EssPortal.Models.ChartModel.dataset> odataset_TotalNet = new List<EssPortal.Models.ChartModel.dataset>();
                List<string> odata_TotalNet = new List<string>();
                foreach (var item in vTotalNet.SelectMany(e => e.SalaryT).OrderByDescending(a => a.Id))
                {
                    odata_TotalNet.Add(item.TotalNet.ToString());
                }
                dataset.Add(new ChartModel.dataset2
                {
                    label = "TotalNet",
                    data = odata_TotalNet.ToArray(),
                    backgroundColor = "#36a2eb",
                });
                chartdatasource.datasets = dataset.ToArray();
                chartdatasource.labels = label.ToArray();
                var output = JsonConvert.SerializeObject(chartdatasource);
                return Json(output, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetEmpSalaryGradeWise()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                  //  FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                    FuncModule = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "601").FirstOrDefault()
                       .LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                if (EmpIds == null || EmpIds.Count == 0)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

                EssPortal.Models.ChartModel.ChartDataSource chartdatasource = new EssPortal.Models.ChartModel.ChartDataSource();
                EssPortal.Models.ChartModel.ChartParameter chartparameter = new EssPortal.Models.ChartModel.ChartParameter();
                var data = new List<EssPortal.Models.ChartModel.Data>();
                var dataset = new List<EssPortal.Models.ChartModel.dataset2>();
                //var category = new List<EssPortal.Models.ChartModel.category>();
                var label = new List<String>();
                var datas = new List<String>();
                var SalData = db.EmployeePayroll
                    .Where(e =>
                   e.Employee.PayStruct != null &&
                   e.Employee.ServiceBookDates.ServiceLastDate == null)
                   .Select(e => new
                   {
                       Grade = e.Employee.PayStruct.Grade.Name,
                   }).ToList();

                if (SalData != null)
                {
                    foreach (var item in SalData.Select(e => e.Grade).Distinct())
                    {
                        label.Add(item);
                    }
                }
                List<EssPortal.Models.ChartModel.dataset> odataset_TotalEarning = new List<EssPortal.Models.ChartModel.dataset>();
                var GradeList = SalData.Select(a => a.Grade.ToUpper()).ToList();
                var vTotalEarning = db.EmployeePayroll
                    .Where(e => GradeList.Contains(e.Employee.PayStruct.Grade.Name.ToUpper()))
                    .Select(e => new
                    {
                        SalaryT = e.SalaryT
                    }).ToList();
                List<string> odata_TotalEarning = new List<string>();

                foreach (var item in vTotalEarning.SelectMany(e => e.SalaryT).OrderByDescending(a => a.Id))
                {
                    odata_TotalEarning.Add(item.TotalEarning.ToString());
                }
                dataset.Add(new ChartModel.dataset2
                {
                    label = "TotalEarning",
                    backgroundColor = "#c9cbcf",
                    data = odata_TotalEarning.ToArray(),
                });
                var vTotalDeduction = db.EmployeePayroll
                 .Where(e => GradeList.Contains(e.Employee.PayStruct.Grade.Name.ToUpper())
                 )
                 .Select(e => new
                 {
                     SalaryT = e.SalaryT
                 }).ToList();

                List<EssPortal.Models.ChartModel.dataset> odataset_TotalDeduction = new List<EssPortal.Models.ChartModel.dataset>();
                List<string> odata_TotalDeduction = new List<string>();

                foreach (var item in vTotalDeduction.SelectMany(e => e.SalaryT).OrderByDescending(a => a.Id))
                {
                    odata_TotalDeduction.Add(item.TotalDeduction.ToString());
                }
                dataset.Add(new ChartModel.dataset2
                {
                    label = "TotalDeduction",
                    data = odata_TotalDeduction.ToArray(),
                    backgroundColor = "#ff6384",
                });
                var vTotalNet = db.EmployeePayroll
               .Where(e => GradeList.Contains(e.Employee.PayStruct.Grade.Name.ToUpper())
               )
               .Select(e => new
               {
                   SalaryT = e.SalaryT
               }).ToList();

                List<EssPortal.Models.ChartModel.dataset> odataset_TotalNet = new List<EssPortal.Models.ChartModel.dataset>();
                List<string> odata_TotalNet = new List<string>();
                foreach (var item in vTotalNet.SelectMany(e => e.SalaryT).OrderByDescending(a => a.Id))
                {
                    odata_TotalNet.Add(item.TotalNet.ToString());
                }
                dataset.Add(new ChartModel.dataset2
                {
                    label = "TotalNet",
                    data = odata_TotalNet.ToArray(),
                    backgroundColor = "#36a2eb",
                });
                chartdatasource.datasets = dataset.ToArray();
                chartdatasource.labels = label.ToArray();
                var output = JsonConvert.SerializeObject(chartdatasource);
                return Json(output, JsonRequestBehavior.AllowGet);
            }
        }
        public class returnEmpPwdClass
        {
            public Int32 Id { get; set; }
            public string val { get; set; }
        }
        public ActionResult GetFy(Int32 data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Id == data).SingleOrDefault();
                var returnData = new
                {
                    ToDate = qurey.ToDate != null ? qurey.ToDate.Value.ToShortDateString() : null,
                    FromDate = qurey.FromDate != null ? qurey.FromDate.Value.ToShortDateString() : null,
                };
                return Json(returnData, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetPath(Int32 data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.DMS_Bulletin.Where(e => e.Id == data).FirstOrDefault();
                var returnData = new
                {
                   Path = query.Attachment
                };
                return Json(returnData, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetEmpPwdReset()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                if (EmpIds == null || EmpIds.Count == 0)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var EmpResetRequstData = db.Employee.Where(e => EmpIds.Contains(e.Id) && e.Login != null && e.Login.IsActive == false)
                    .Select(e => new
                    {
                        EmpCode = e.EmpCode,
                        EmpName = e.EmpName,
                        Login = e.Login,
                        LoginDetails = e.Login.LoginDetails,
                        LogRegister = e.LogRegister
                    }).ToList();
                List<returnEmpPwdClass> ListreturnEmpPwdClass = new List<returnEmpPwdClass>();
                foreach (var item in EmpResetRequstData)
                {
                    ListreturnEmpPwdClass.Add(new returnEmpPwdClass
                    {
                        Id = item.Login.Id,
                        val = "EmpCode :" + item.EmpCode + " EmpName :" + item.EmpName.FullNameFML + " Date :" + item.LogRegister.LastOrDefault().LogInDate.Value.ToShortDateString()
                    });
                }
                return Json(new { status = true, data = ListreturnEmpPwdClass }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult UpdateLoginDetails(Int32 data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Login.Where(e => e.Id == data).SingleOrDefault();
                qurey.IsActive = true;
                db.Login.Attach(qurey);
                db.Entry(qurey).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                return Json(new { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
            }
        }

        
        public ActionResult GetDocument(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returnImg = "";

                var localPath = "";
                if (data == null)
                {
                    return Json(new { status = false });
                }
                var id = Convert.ToInt32(SessionManager.EmpId);
                var Emp = db.Employee.Where(e => e.Id == id).SingleOrDefault();
                var doc = db.EmpDocument.Where(e => e.Type.LookupVal.ToUpper() == data.ToUpper() &&
                    e.EmpCode == Emp.EmpCode).ToList()
                    .OrderByDescending(e => e.Id).FirstOrDefault();
                if (doc != null)
                {
                    localPath = doc.Path;
                }


                //string newPath = new Uri(localPath).LocalPath;
                System.IO.FileInfo file = new System.IO.FileInfo(localPath);
                if (file.Exists)
                {
                    byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                    string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                    returnImg = base64ImageRepresentation;
                }
                else
                {
                    return null;
                }
                //return Json(new { status = true, data = returnImg }, JsonRequestBehavior.AllowGet);
                JsonRequestBehavior behaviou = new JsonRequestBehavior();
                //return Json(new { data = a, status = true,ma }, JsonRequestBehavior.AllowGet);
                return new JsonResult()
                {
                    Data = returnImg,
                    MaxJsonLength = 86753090,
                    JsonRequestBehavior = behaviou
                };

            }
        }

        public class GetBulletinData
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public string PublishDate { get; set; }
            public string Attachment { get; set; }
            public ChildGetBirthdayListClass RowData { get; set; }
        }

        public class GetBirthdayData
        {
            public string Emp { get; set; }
            public string Location { get; set; }
            public string Department { get; set; }
            public string Birthdate { get; set; }
        }

        public class ChildGetLvNewReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }

        }

        public JsonResult GetBulletinList()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (SessionManager.EmpId == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var self = Convert.ToInt32(SessionManager.EmpId);

                //List<String> oPayMonth = new List<String>();
                var db_data = db.DMS_Bulletin.Where(e => e.ExpiryDate >= DateTime.Now).ToList();
                if (db_data == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (db_data != null && db_data.Count() > 0)
                    {
                        List<ELMSController.AttachDataClass> ListPayMonth = new List<ELMSController.AttachDataClass>();
                       
                        foreach (var ca1 in db_data)
                        {
                            string Filename = Path.GetFileName(ca1.Attachment);
                            ListPayMonth.Add(new ELMSController.AttachDataClass { Id = ca1.Id.ToString(), val = ca1.PublishDate.Value.ToString("dd/MM/yyy"), Empname = ca1.Title + " " + Filename });
                                
                        }
                   
                        return Json(new { status = true, data = ListPayMonth }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }

        //public ActionResult GetBulletinList()
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
        //        {
        //            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        //        }

        //        var db_data = db.DMS_Bulletin.Where(e => e.ExpiryDate >= DateTime.Now).ToList();

        //        if (db_data != null && db_data.Count() > 0)
        //        {
        //            List<GetBulletinData> returndata = new List<GetBulletinData>();
        //            returndata.Add(new GetBulletinData
        //            {
        //                Title = "Title",
        //                Message = "Message",
        //                PublishDate = "PublishDate",
        //                Attachment = "Attachment"
        //            });

        //            foreach (var item1 in db_data)
        //            {
        //                string filename = Path.GetFileName(item1.Attachment);
        //                returndata.Add(new GetBulletinData
        //                {
        //                    RowData = new ChildGetBirthdayListClass
        //                    {
        //                        LvNewReq = item1.Id.ToString(),

        //                    },
        //                    Attachment = filename,
        //                    Message = item1.MessageContent,
        //                    PublishDate = item1.PublishDate.Value.ToShortDateString(),
        //                    Title = item1.Title
        //                });
                             
        //            }
        //            return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}

        //public class ChildGetLvNewReqClass
        //{
        //    public string LvNewReq { get; set; }
        //    public string EmpLVid { get; set; }
        //    public string Status { get; set; }
        //    public string LvHead_Id { get; set; }
        //    public string IsClose { get; set; }

        //}

        public class GetBirthdayListClass
        {
            public string EmpName { get; set; } 
            public string Location { get; set; }
            public string BirthDate { get; set; }
            public string EmailId { get; set; }
            public string Phone { get; set; }
            public ChildGetBirthdayListClass RowData { get; set; }
        }

        public class ChildGetBirthdayListClass
        {
            public string LvNewReq { get; set; }
            public int EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }

        }
        public ActionResult GetBirthdayList()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

                var db_data = db.Employee.Include(e => e.ServiceBookDates).Include(e => e.EmpName).Include(e => e.GeoStruct).Include(e => e.GeoStruct.Location.LocationObj)
                    .Include(e => e.PerContact.ContactNumbers).Include(e => e.CorContact.ContactNumbers).Include(e => e.ResContact.ContactNumbers)
                    .Where(e => e.ServiceBookDates.BirthDate.Value.Month == DateTime.Now.Month && e.ServiceBookDates.BirthDate.Value.Day == DateTime.Now.Day
                    && e.ServiceBookDates.ServiceLastDate == null).ToList();

                if (db_data != null && db_data.Count() > 0)
                {
                    List<GetBirthdayListClass> returndata = new List<GetBirthdayListClass>();
                    returndata.Add(new GetBirthdayListClass
                    {
                        EmpName = "Name",
                        BirthDate = "Birthday",
                        Location = "Location",
                        EmailId = "Email ID",
                        Phone = "Phone", 
                    });

                    string Phone = "", EmailId="";

                    foreach (var item1 in db_data)
                    {
                        if (item1.PerContact != null && item1.PerContact.ContactNumbers.Count() > 0)
                        {
                            EmailId = item1.PerContact.EmailId;
                            Phone = item1.PerContact.ContactNumbers.FirstOrDefault().MobileNo;
                        }
                        if (item1.ResContact != null && item1.ResContact.ContactNumbers.Count() > 0)
                        {
                            EmailId = item1.ResContact.EmailId;
                            Phone = item1.ResContact.ContactNumbers.FirstOrDefault().MobileNo;
                        }
                        if (item1.CorContact != null && item1.CorContact.ContactNumbers.Count() > 0)
                        {
                            EmailId = item1.CorContact.EmailId;
                            Phone = item1.CorContact.ContactNumbers.FirstOrDefault().MobileNo;
                        }
                        returndata.Add(new GetBirthdayListClass
                        {
                            RowData = new ChildGetBirthdayListClass
                            {
                                LvNewReq = item1.Id.ToString(),
                            },

                            EmpName = item1.EmpCode + " - " + item1.EmpName != null ? item1.EmpName.FullNameFML : "",
                            BirthDate =item1.ServiceBookDates != null && item1.ServiceBookDates.BirthDate != null ? item1.ServiceBookDates.BirthDate.Value.ToShortDateString() : null,
                            Location = item1.GeoStruct != null && item1.GeoStruct.Location != null && item1.GeoStruct.Location.LocationObj != null ? item1.GeoStruct.Location.LocationObj.LocDesc : null,
                            EmailId = EmailId != "" ? EmailId : "",
                            Phone = Phone != "" ? Phone : ""
                        });

                    }
                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}
