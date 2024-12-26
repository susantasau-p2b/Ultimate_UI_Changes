using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Process;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using P2BUltimate.Security;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.IO.Compression;
using Ionic.Zip;
using System.Net;
using System.Configuration;
using Renci.SshNet;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class JVProcessDataController : Controller
    {
        //  private DataBaseContext db = new DataBaseContext();
        //
        // GET: /JVProcessData/
        public ActionResult Index()
        {
            // db.RefreshAllEntites(RefreshMode.StoreWins);
            return View("~/Views/Payroll/MainViews/JVProcessData/Index.cshtml");
        }



        //public ActionResult GetJVNameLKDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var id = Convert.ToInt32(SessionManager.CompanyId);
        //        var Comp_jv_data = db.CompanyPayroll.Include(e => e.JVParameter).Where(e => e.Company.Id == id).SingleOrDefault();
        //        var jv_data = Comp_jv_data.JVParameter.Select(e => new
        //        {
        //            code = e.Id.ToString(),
        //            value = "JVCode: " + e.JVProductCode.ToString() + ",JVName: " + e.JVName.ToString(),

        //        }).ToList();
        //        return Json(jv_data, JsonRequestBehavior.AllowGet);
        //    }

        //}

        public ActionResult GetJVNameLKInPayslipDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var id = Convert.ToInt32(SessionManager.CompanyId);
                var Comp_jv_data = db.CompanyPayroll
                    .Include(e => e.JVParameter)
                    .Include(e => e.JVParameter.Select(x => x.JVGroup))
                    .Include(q => q.JVParameter.Select(a => a.SalaryHead))
                    .Include(e => e.JVParameter.Select(a => a.PaymentBank))
                    .Where(e => e.Company.Id == id)
                    .SingleOrDefault();

                int PaybankId = Convert.ToInt32(data);

                var jv_data1 = Comp_jv_data.JVParameter
                    .Where(q => q.SalaryHead.Any(a => a.InPayslip == true && q.PaymentBank_Id == PaybankId && q.JVGroup.LookupVal.ToUpper()=="PAYMENTBANK")).Select(e => new
                    {
                        code = e.Id.ToString(),
                        value = "JVCode: " + e.JVProductCode.ToString() + ",JVName: " + e.JVName.ToString(),

                    }).ToList();
                var jv_data2 = Comp_jv_data.JVParameter
                   .Where(q => q.SalaryHead.Any(a => a.InPayslip == true  && q.JVGroup.LookupVal.ToUpper() != "PAYMENTBANK")).Select(e => new
                   {
                       code = e.Id.ToString(),
                       value = "JVCode: " + e.JVProductCode.ToString() + ",JVName: " + e.JVName.ToString(),

                   }).ToList();
                var jv_data = jv_data1.Union(jv_data2).ToList();
                return Json(jv_data, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetJVNameLKNotInPayslipDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var id = Convert.ToInt32(SessionManager.CompanyId);
                var Comp_jv_data = db.CompanyPayroll
                    .Include(e => e.JVParameter)
                    .Include(e => e.JVParameter.Select(x => x.JVGroup))
                    .Include(q => q.JVParameter.Select(a => a.SalaryHead))
                     .Include(e => e.JVParameter.Select(a => a.PaymentBank))
                    .Where(e => e.Company.Id == id)
                    .SingleOrDefault();

                int PaybankId = Convert.ToInt32(data);

                var jv_data1 = Comp_jv_data.JVParameter
                    .Where(q => q.SalaryHead.All(a => a.InPayslip == false && q.PaymentBank_Id == PaybankId && q.JVGroup.LookupVal.ToUpper() == "PAYMENTBANK")).Select(e => new
                    {
                        code = e.Id.ToString(),
                        value = "JVCode: " + e.JVProductCode.ToString() + ",JVName: " + e.JVName.ToString(),

                    }).ToList();
                var jv_data2 = Comp_jv_data.JVParameter
                  .Where(q => q.SalaryHead.All(a => a.InPayslip == false && q.JVGroup.LookupVal.ToUpper() != "PAYMENTBANK")).Select(e => new
                  {
                      code = e.Id.ToString(),
                      value = "JVCode: " + e.JVProductCode.ToString() + ",JVName: " + e.JVName.ToString(),

                  }).ToList();
                var jv_data = jv_data1.Union(jv_data2).ToList();
                return Json(jv_data, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.JVProcessDataSummary
                    .Where(e => e.Id == data).Select
                    (e => new
                    {

                        BatchName = e.BatchName,
                        CreditAmount = e.CreditAmount,
                        DebitAmount = e.DebitAmount,
                        JVFileName = e.JVFileName,
                        ProcessDate = e.ProcessDate,
                        ProcessMonth = e.ProcessMonth,
                        ReleaseDate = e.ReleaseDate,
                        Action = e.DBTrack.Action
                    }).ToList();
                //var Corp = db.JVProcessData.Find(data);
                //TempData["RowVersion"] = Corp.RowVersion;
                //Session["Id"] = Corp.Id;
                //var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string JVParams = form["JVParameter-table"] == "0" ? "" : form["JVParameter-table"];
                string mPayMonth = form["Create-month"] == "0" ? "" : form["Create-month"];
                string mBatchName = form["Batch"] == "0" ? "" : form["Batch"];
                string jvparameterslist = form["jvnamelist"] == "0" ? "" : form["jvnamelist"];
                string checkifyearlyA = form["AppearPayslip"] == "0" ? "" : form["AppearPayslip"];
                bool apprpayslip = Convert.ToBoolean(checkifyearlyA);
                string PaymentBank = form["PaymentBankC_drop"] == "0" ? "" : form["PaymentBankC_drop"];
                int PayBankId = PaymentBank != "" ? Convert.ToInt32(PaymentBank) : 0; ;
                string mFromPeriod = form["txtFromPeriod"] == "0" ? "" : form["txtFromPeriod"];
                string mToPeriod = form["txtToPeriod"] == "0" ? "" : form["txtToPeriod"];
                try
                {
                    List<string> Msg = new List<string>();
                    int CompId = 0;
                    if (apprpayslip==false)
                    {
                        if (mFromPeriod == "" && mToPeriod == "")
                        {
                            Msg.Add("Please Enter From date and ToDate " );
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        DateTime fromdate = Convert.ToDateTime(mFromPeriod);
                        DateTime Todate = Convert.ToDateTime(mToPeriod);
                        if (fromdate.Date> Todate.Date)
                        {
                            Msg.Add("From period should be less than To period ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        } 
                    }
                   
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = int.Parse(Session["CompId"].ToString());
                    }

                    List<int> ids = new List<int>();
                    if (JVParams != null && JVParams != "0" && JVParams != "false")
                    {
                        String itsec_id = JVParams;
                        ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
                        //  ids.Add(2517);

                    }
                    //   List<int> _JVCodeList = new List<int>();

                    //var JVList = db.JVParameter.Include(e => e.SalaryHead).Where(e => ids.Contains(e.Id)).ToList();
                    //foreach (var item in JVList)
                    //{
                    //    string Code = db.JVParameter.Where(e => e.Id == item.Id).SingleOrDefault().JVProductCode;
                    //    _JVCodeList.Add(Code);
                    //}
                    //foreach (var item in JVList)
                    //{
                    //    if (item.Irregular)
                    //    {
                    //        _JVCodeList = new List<string>();
                    //        var aa = item.SalaryHead.Select(a => a.Id.ToString()).ToList();
                    //        foreach (var item2 in aa)
                    //        {
                    //            _JVCodeList.Add(item2);
                    //        }
                    //    }
                    //}
                    //var EmpList = db.EmployeePayroll.Include(e => e.SalaryT).AsNoTracking().AsParallel().ToList();

                    List<int> OEmployeePayrollIds = new List<int>();
                    List<int> OEmployeePayrollIdsNotInsal = new List<int>();

                    //if (EmpList.Count > 0)
                    //{
                    //    foreach (var i in EmpList)
                    //    {
                    //        var SalT = i.SalaryT.Where(e => e.PayMonth == mPayMonth).SingleOrDefault();
                    //        if (SalT != null)
                    //        {
                    //            OEmployeePayrollIds.Add(i.Id);
                    //        }
                    //    }
                    //}

                    /////new added 18/10/2019

                    List<SalEarnDedT> OCheck2 = new List<SalEarnDedT>();
                    

                    var OCheck1 = db.SalaryT.Include(e => e.SalEarnDedT)
                       .Include(e => e.SalEarnDedT.Select(q => q.SalaryHead))
                        .Where(e => e.PayMonth == mPayMonth).ToList();

                    var OCheckSalEarnDed = OCheck1.SelectMany(e => e.SalEarnDedT).Where(e => e.Amount > 0).ToList().OrderBy(r => r.SalaryHead.Id).Select(r => r.SalaryHead.Id).Distinct();


                    var JVSalHeadCheck = db.JVParameter.Include(e => e.SalaryHead).ToList();

                    var OCheckSalHead = JVSalHeadCheck.SelectMany(e => e.SalaryHead).ToList().OrderBy(r => r.Id).Select(r => r.Id).Distinct();

                    var result = OCheckSalEarnDed.Where(p => OCheckSalHead.All(p2 => p2 != p));

                    List<SalaryHead> OEarn = new List<SalaryHead>();
                    List<SalaryHead> ODed = new List<SalaryHead>();

                    foreach (var item in result)
                    {

                        SalaryHead salhead = db.SalaryHead.Include(x => x.SalHeadOperationType).Where(e => e.Id == item && e.InPayslip == true).Include(e => e.Type).SingleOrDefault();
                        if (salhead != null)
                        {

                            if (salhead.Type.LookupVal.ToUpper() == "EARNING" && salhead.SalHeadOperationType.LookupVal.ToUpper() != "ARREAREARN" )
                            {
                                OEarn.Add(salhead);
                            }
                            else if (salhead.Type.LookupVal.ToUpper() == "DEDUCTION" && salhead.SalHeadOperationType.LookupVal.ToUpper() != "ARREARDED" && salhead.SalHeadOperationType.LookupVal.ToUpper() != "OFFDED")
                            {
                                ODed.Add(salhead);
                            }
                        }

                    }

                    int count = 0;
                    if (result != null)
                    {
                        foreach (var item in JVSalHeadCheck)
                        {
                            if (item.JVName.ToUpper() == "GROSS")
                            {
                                count = 1;
                                var OdedCheck = ODed.Distinct().ToList();
                                foreach (var ca in OdedCheck)
                                {
                                   Msg.Add("Please define this head first :" + ca.FullDetails + ",");
                                }
                                break;
                            }

                        }
                        if (count == 0)
                        {
                            var OEarnCheck = OEarn.Distinct().ToList();
                            foreach (var ca1 in OEarnCheck)
                            {
                               Msg.Add("Please define this head first :" + ca1.FullDetails + ",");
                            }
                        }

                        if (Msg.Count() > 0)
                        {
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    List<int> jvid = new List<int>();

                    var jvfile = db.JVFileName.ToList();
                    if (jvfile!=null && jvfile.Count()>0)
                    {
                        var jvhead = db.JVFileName.Include(e => e.JVHeadList).ToList();
                        foreach (var item in jvhead)
                        {
                            var jid = item.JVHeadList.ToList();

                            foreach (var j in jid)
                            {
                                jvid.Add(j.Id);
                            }


                        }
                        var exceptedval = ids.Except(jvid).ToList();
                      
                        var listLvs = db.JVParameter.Where(e => exceptedval.Contains(e.Id)).OrderBy(e => e.Id).ToList();
                        foreach (var item in listLvs)
                        {
                                Msg.Add("Please Map in JVfilename JV head first :" + item.JVName + ",");

                        }
                        if (Msg.Count() > 0)
                        {
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                    }

                    //////////////////////////////////////////////////////////////////
                    List<int> idst = null;
                    if (PayBankId != 0)
                    {
                        idst = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpOffInfo)
                       .Where(e => e.Employee.EmpOffInfo.Bank_Id == PayBankId).Select(e => e.Id).ToList();
                    }
                    else
                    {
                        idst = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpOffInfo)
                       .Select(e => e.Id).ToList();
                    }
                   

                    OEmployeePayrollIds = db.SalaryT.Where(e => e.PayMonth == mPayMonth && idst.Contains(e.EmployeePayroll.Id)).AsNoTracking().AsParallel().Select(q => q.EmployeePayroll_Id.Value).ToList();
                    // OEmployeePayrollIds = db.SalaryT.Where(e => e.PayMonth == mPayMonth).AsNoTracking().AsParallel().Select(q => q.EmployeePayroll_Id.Value).ToList();
                    OEmployeePayrollIdsNotInsal = db.EmployeePayroll.Include(q => q.YearlyPaymentT).Where(q => !OEmployeePayrollIds.Contains(q.Id) && q.YearlyPaymentT.Any(s => s.PayMonth == mPayMonth)).Select(q => q.Id).ToList();

                    int OCompanyPayrollId = Convert.ToInt32(SessionManager.CompPayrollId);
                    //db.CompanyPayroll.Where(e => e.Company.Id == CompId).SingleOrDefault().Id;
                    //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 60, 0)))
                    //{
                    GenerateJV.ProcessJV(mPayMonth, OCompanyPayrollId, OEmployeePayrollIds, OEmployeePayrollIdsNotInsal, mBatchName, ids, apprpayslip, mFromPeriod, mToPeriod);
                    //  ts.Complete();
                    // }
                }
                catch (Exception ex)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {

                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);
                    // return Json(new Object[] { "", "", Msg }, JsonRequestBehavior.AllowGet);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                List<string> Msgs = new List<string>();
                Msgs.Add("Data Saved successfully");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Download(int id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    JVProcessDataSummary OJVProcessData = db.JVProcessDataSummary.Where(e => e.Id == id).SingleOrDefault();
                    string FilePath = OJVProcessData.JVFileName;
                    string BatchName = OJVProcessData.BatchName;
                    bool exist = false;
                   // JVParameter OJVParameter = db.JVParameter.Where(e => e.SourceType.LookupVal.ToUpper() == "DISTRIBUTED").FirstOrDefault();
                    var OJVParameter = db.JVFileName.ToList();

                    if (OJVParameter != null && OJVParameter.Count()>1)
                    {
                        exist = true;
                    }
                    //string filename = FilePath.Substring(FilePath.LastIndexOf("\\") + 1);
                    //string content = string.Empty;
                    //using (var stream = new StreamReader(Server.MapPath("~/JVFile/" + filename)))
                    //{
                    //    content = stream.ReadToEnd();
                    //    //System.Diagnostics.Process.Start("notepad.exe", Server.MapPath("~/JVFile/" + filename));
                    //}

                    // return View("~/Views/Payroll/MainViews/PFECRSummaryR/Index.cshtml");
                    //return FilePath, BatchName;
                    return Json(new Object[] { FilePath, BatchName, exist, JsonRequestBehavior.AllowGet });

                }
                catch (Exception ex)
                {

                    string content = string.Empty;
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    content = "LogFile Created";
                    //return View("~/Views/Payroll/MainViews/PFECRSummaryR/Index.cshtml");
                    return Json(new Object[] { content, "", JsonRequestBehavior.AllowGet });
                }
            }
        }

        public ActionResult ChkDist()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    bool exist = false;
                    JVParameter OJVParameter = db.JVParameter.Where(e => e.SourceType.LookupVal.ToUpper() == "DISTRIBUTED").FirstOrDefault();

                    if (OJVParameter != null)
                    {
                        exist = true;
                    }
                    return Json(exist, JsonRequestBehavior.AllowGet);

                }
                catch (Exception ex)
                {

                    string content = string.Empty;
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    content = "LogFile Created";
                    //return View("~/Views/Payroll/MainViews/PFECRSummaryR/Index.cshtml");
                    return Json(new Object[] { content, "", JsonRequestBehavior.AllowGet });
                }
            }
        }

        public class FileModelNew
        {
            public string FileName { get; set; }
            public string FilePath { get; set; }
            public bool IsSelected { get; set; }
        }

        public ActionResult DownloadFile(string fileName, string BatchName, bool exist)
        {

            if (exist == true)
            {
                string fileSavePath = Path.GetDirectoryName(fileName);
                string[] filePaths = Directory.GetFiles(fileSavePath);
                List<FileModelNew> files = new List<FileModelNew>();
                foreach (string filePath in filePaths)
                {
                    if (filePath.Contains(BatchName) == true)
                    {
                        files.Add(new FileModelNew()
                        {
                            FileName = Path.GetFileName(filePath),
                            FilePath = filePath,
                            IsSelected = true
                        });
                    }
                }

                using (ZipFile zip = new ZipFile())
                {
                    zip.AlternateEncodingUsage = ZipOption.AsNecessary;
                    zip.AddDirectoryByName("Files");
                    foreach (FileModelNew file in files)
                    {
                        if (file.IsSelected)
                        {
                            string newname = "";
                            if (file.FilePath.Contains("RD"))
                            {
                                string[] filenamesp = file.FileName.Split('_');
                                int startindex = file.FilePath.LastIndexOf(@"\");
                                newname = file.FilePath.Remove(startindex) + @"\" + "RD_" + filenamesp[5].Replace(".txt", "") + "_" + filenamesp[1] + ".txt";
                            }
                            else
                            {
                                int startindex = file.FilePath.IndexOf(BatchName);
                                newname = file.FilePath.Remove(startindex - 1, BatchName.Length + 1);
                            }
                          
                            
                            if (System.IO.File.Exists(newname) )
                            {
                                System.IO.File.Delete(newname);
                            }
                            System.IO.File.Move(file.FilePath, newname);
                            zip.AddFile(newname, "Files");
                        }
                    }
                    string zipName = String.Format("Zip_{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        zip.Save(memoryStream);
                        return File(memoryStream.ToArray(), "application/zip", zipName);
                    }
                }  
            }
            else
            {
                string localPath = new Uri(fileName).LocalPath;
                System.IO.FileInfo file = new System.IO.FileInfo(localPath);
                if (file.Exists)
                    return File(file.FullName, "text/plain", file.Name);
                else
                    return HttpNotFound();
            }

            return HttpNotFound();
            //FileDownloads obj = new FileDownloads();
            //var filesCol = obj.GetFile(fileName, BatchName).ToList();
            //using (var memoryStream = new MemoryStream())
            //{
            //    using (var ziparchive = new ZipArchive(memoryStream, ZipArchiveMode.Update, true))
            //    {
            //        for (int i = 0; i < filesCol.Count; i++)
            //        {
            //            ziparchive.CreateEntry(filesCol[i].FileName);
            //        }
            //    }
            //    return File(memoryStream.ToArray(), "application/zip", "Attachments.zip");
            //}
           
            //string localPath = new Uri(fileName).LocalPath;
            //System.IO.FileInfo file = new System.IO.FileInfo(localPath);
            //if (file.Exists)
            //    return File(file.FullName, "text/plain", file.Name);
            //else
            //    return HttpNotFound();
        }

        public string ViewJVFile(int id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string FilePath = db.JVProcessDataSummary.Where(e => e.Id == id).Select(e => e.JVFileName).SingleOrDefault();
                    string filename = FilePath.Substring(FilePath.LastIndexOf("\\") + 1);
                    string content = string.Empty;
                    using (var stream = new StreamReader(Server.MapPath("~/JVFile/" + filename)))
                    {
                        content = stream.ReadToEnd();
                        //System.Diagnostics.Process.Start("notepad.exe", Server.MapPath("~/JVFile/" + filename));
                    }

                    // return View("~/Views/Payroll/MainViews/PFECRSummaryR/Index.cshtml");
                    return content;

                }
                catch (Exception ex)
                {
                    string content = string.Empty;
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    content = "LogFile Created";
                    //return View("~/Views/Payroll/MainViews/PFECRSummaryR/Index.cshtml");
                    return content;
                }
            }
        }

        public ActionResult CreateFile(int PaymentBankId, string mPayMonth, int Id) //Create submit
        {
            P2BUltimate.Models.JVFile OJVFile = new P2BUltimate.Models.JVFile();
            var mfilestring = "";
            List<JVProcessData> OJVProcessData = new List<JVProcessData>();
            using (DataBaseContext db = new DataBaseContext())
            {
                JVProcessDataSummary OVProcessdatasum = db.JVProcessDataSummary.Where(e => e.Id == Id).FirstOrDefault();
                string BatchName = OVProcessdatasum.BatchName;
                string DistributedCode = "";

                if (OVProcessdatasum.CreditAmount == OVProcessdatasum.DebitAmount)
                {
                    if (PaymentBankId != 0)
                    {
                        DistributedCode = db.Bank.Where(e => e.Id == PaymentBankId).FirstOrDefault().Code;
                        OJVProcessData = db.JVProcessData.Include(e => e.JVParameter).Where(e => e.DistributedCode == DistributedCode && e.ProcessMonth == mPayMonth && e.BatchName == BatchName).ToList();
                    }
                    else
                    {
                        OJVProcessData = db.JVProcessData.Include(e => e.JVParameter).Where(e => e.ProcessMonth == mPayMonth && e.BatchName == BatchName).ToList();
                    }


                    var jvfilename = db.JVFileName.Include(e => e.JVFileFormat).Include(e => e.JVFileFormat.Seperator).Include(e => e.JVFileFormat.CBS).Include(e => e.JVFileFormat.FormatType).Include(e => e.JVField).Include(e => e.JVHeadList)
                        .Include(e => e.JVField.Select(t => t.Name)).Include(e => e.JVField.Select(t => t.Value)).Include(e => e.JVField.Select(t => t.PaddingChar))
                        .Include(e => e.JVField.Select(t => t.PaddingSide)).Include(e => e.JVField.Select(t => t.ConcatData)).Include(e => e.JVField.Select(t => t.SplitData))
                        .Include(e => e.JVField.Select(t => t.ConcatDataValue)).Include(e => e.JVField.Select(t => t.SplitDataValue))
                        .Include(e => e.JVField.Select(t => t.ConcatDataPaddingSide)).Include(e => e.JVField.Select(t => t.SplitDataPaddingSide))
                        .ToList();
                    string errorMsg = "";

                    foreach (var jvname in jvfilename)
                    {
                        if (jvname.JVHeadList != null && jvname.JVHeadList.Count() > 0)
                        {
                            var JvparaIdList = jvname.JVHeadList.Select(e => e.Id).ToList();
                            var OJVProcessDataFile = OJVProcessData.Where(e => JvparaIdList.Contains(e.JVParameter_Id.Value)).ToList();

                            if (OJVProcessDataFile.Count > 0)
                            {
                                mfilestring = OJVFile.CreateJVFile(OJVProcessDataFile, mPayMonth, DistributedCode, jvname, BatchName);
                                string ftpIP = ConfigurationManager.AppSettings["FtpServerIP"];
                                if (ftpIP != null)
                                {
                                   
                                    errorMsg = SFTPUpload(mfilestring);
                                    if (errorMsg != "")
                                    {
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = errorMsg }, JsonRequestBehavior.AllowGet);
                                    }
                                }

                            }
                        }
                    }
                    if (mfilestring.Contains("No Data Found"))
                    {
                        return Json(new Utility.JsonReturnClass { success = false, responseText = "No Data Found.." }, JsonRequestBehavior.AllowGet);
                    }
                    OVProcessdatasum.JVFileName = mfilestring;
                    db.JVProcessDataSummary.Attach(OVProcessdatasum);
                    db.Entry(OVProcessdatasum).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                else 
                {
                    return Json(new Utility.JsonReturnClass { success = false, responseText = "Please check the file as amount not matched." }, JsonRequestBehavior.AllowGet);
                }
                
                
            }
            return Json(new Utility.JsonReturnClass { success = true, responseText = "File generated successfully." }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Polulate_PaymentBank(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.Bank.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<JVProcessDataSummary> JVProcessDataSummary = null;

                string PayMonth = "";
                string Month = "";
                if (gp.filter != null)
                    PayMonth = gp.filter;
                else
                {
                    if (DateTime.Now.Date.Month < 10)
                        Month = "0" + DateTime.Now.Date.Month;
                    else
                        Month = DateTime.Now.Date.Month.ToString();
                    PayMonth = Month + "/" + DateTime.Now.Date.Year;
                }

                if (gp.IsAutho == true)
                {
                    JVProcessDataSummary = db.JVProcessDataSummary.Where(e => e.DBTrack.IsModified == true && e.ProcessMonth == PayMonth).AsNoTracking().ToList();
                }
                else
                {
                    JVProcessDataSummary = db.JVProcessDataSummary.Where(e => e.ProcessMonth == PayMonth).AsNoTracking().ToList();
                }

                IEnumerable<JVProcessDataSummary> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = JVProcessDataSummary;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.BatchName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                        || (e.CreditAmount.ToString().Contains(gp.searchString))
                        || (e.DebitAmount.ToString().Contains(gp.searchString))
                        || (e.ProcessMonth.ToString().Contains(gp.searchString))
                        || (e.ProcessDate.ToString().Contains(gp.searchString))
                        || (e.ReleaseDate.ToString().Contains(gp.searchString))
                        || (e.JVFileName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                         || (e.ReleaseFrom.ToString().Contains(gp.searchString))
                          || (e.ReleaseTo.ToString().Contains(gp.searchString))
                        || (e.Id.ToString().Contains(gp.searchString))
                        ).Select(a => new Object[] { a.BatchName, a.CreditAmount, a.DebitAmount, a.ProcessMonth, a.ProcessDate.Value.ToShortDateString(), a.ReleaseDate, a.JVFileName, a.ReleaseFrom,a.ReleaseTo, a.Id }).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.BatchName, a.CreditAmount, a.DebitAmount, a.ProcessMonth, a.ProcessDate, a.ReleaseDate, a.JVFileName, a.ReleaseFrom, a.ReleaseTo, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = JVProcessDataSummary;
                    Func<JVProcessDataSummary, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "BatchName" ? c.BatchName.ToString() :
                                         gp.sidx == "CreditAmount" ? c.CreditAmount.ToString() :
                                         gp.sidx == "DebitAmount" ? c.DebitAmount.ToString() :
                                         gp.sidx == "ProcessMonth" ? c.ProcessMonth.ToString() :
                                         gp.sidx == "ProcessDate" ? c.ProcessDate.ToString() :
                                         gp.sidx == "ReleaseDate" ? c.ReleaseDate.ToString() :
                                         gp.sidx == "JVFileName" ? c.JVFileName.ToString() :
                                          gp.sidx == "ReleaseFrom" ? c.ReleaseFrom.ToString() :
                                           gp.sidx == "ReleaseTo" ? c.ReleaseTo.ToString() : ""
                                         );
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.BatchName, a.CreditAmount, a.DebitAmount, a.ProcessMonth, a.ProcessDate.Value.ToString("dd/MM/yyyy"), a.ReleaseDate != null ? a.ReleaseDate.Value.ToString("dd/MM/yyyy") : null, a.JVFileName, a.ReleaseFrom != null ? a.ReleaseFrom.Value.ToString("dd/MM/yyyy") : null, a.ReleaseTo != null ? a.ReleaseTo.Value.ToString("dd/MM/yyyy") : null, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.BatchName, a.CreditAmount, a.DebitAmount, a.ProcessMonth, a.ProcessDate.Value.ToString("dd/MM/yyyy"), a.ReleaseDate != null ? a.ReleaseDate.Value.ToString("dd/MM/yyyy") : null, a.JVFileName, a.ReleaseFrom != null ? a.ReleaseFrom.Value.ToString("dd/MM/yyyy") : null, a.ReleaseTo != null ? a.ReleaseTo.Value.ToString("dd/MM/yyyy") : null, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.BatchName, a.CreditAmount, a.DebitAmount, a.ProcessMonth, a.ProcessDate.Value.ToString("dd/MM/yyyy"), a.ReleaseDate != null ? a.ReleaseDate.Value.ToString("dd/MM/yyyy") : null, a.JVFileName, a.ReleaseFrom != null ? a.ReleaseFrom.Value.ToString("dd/MM/yyyy") : null, a.ReleaseTo != null ? a.ReleaseTo.Value.ToString("dd/MM/yyyy") : null, a.Id }).ToList();
                    }
                    totalRecords = JVProcessDataSummary.Count();
                }
                if (totalRecords > 0)
                {
                    totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
                }
                if (gp.page > totalPages)
                {
                    gp.page = totalPages;
                }
                var JsonData = new
                {
                    page = gp.page,
                    rows = jsonData,
                    records = totalRecords,
                    total = totalPages
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

       // [HttpPost]
        public string FTPUpload(string postedFile)
        {
            // FTP Server URL
            //string ftp = "ftp://ftp." + ConfigurationManager.AppSettings["FtpServerIP"] + ".com/";
            string ftp = "ftp://" + ConfigurationManager.AppSettings["FtpServerIP"];

            // FTP Folder name. Leave blank if you want to upload to root folder
            // (really blank, not "/" !)
            string ftpFolder = ConfigurationManager.AppSettings["ftpFolderName"];
            byte[] fileBytes = null;    
            string ftpUserName = ConfigurationManager.AppSettings["ftpUserName"];
            string ftpPassword = ConfigurationManager.AppSettings["ftpPassword"]; ;

            // read the File and convert it to Byte array.
            string fileName = Path.GetFileName(postedFile);
            fileBytes = System.IO.File.ReadAllBytes(postedFile);
            string errorMsg = "";

            try
            {
                // create FTP Request
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftp + ftpFolder + fileName);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                // enter FTP Server credentials
                request.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
                request.ContentLength = fileBytes.Length;
                request.UsePassive = true;
                request.UseBinary = true;   // or FALSE for ASCII files
                request.ServicePoint.ConnectionLimit = fileBytes.Length;
                request.EnableSsl = false;

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(fileBytes, 0, fileBytes.Length);
                    requestStream.Close();
                }
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                response.Close();
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message.ToString();
               // throw new Exception((ex.Response as FtpWebResponse).StatusDescription);
            }
            return errorMsg;
            //return View();
        }

        public string SFTPUpload(string sourcefile)
        {
            string host = "", username = "", password = "", errorMsg="";
            int port = 0;
            string destinationpath = ConfigurationManager.AppSettings["ftpFolderName"];
            host = ConfigurationManager.AppSettings["ftpHostName"];
            username = ConfigurationManager.AppSettings["ftpUserName"];
            password = ConfigurationManager.AppSettings["ftpPassword"];
            port = Convert.ToInt32(ConfigurationManager.AppSettings["ftpPortName"]);
            try 
            {
                using (SftpClient client = new SftpClient(host, port, username, password))
                {
                    client.Connect();
                    client.ChangeDirectory(destinationpath);

                    if (System.IO.File.Exists(destinationpath))
                    {
                        errorMsg = "File already exists.";
                        return errorMsg;
                    }
                    using (FileStream fs = new FileStream(sourcefile, FileMode.Open))
                    {
                        client.BufferSize = 4 * 1024;
                        client.UploadFile(fs, Path.GetFileName(sourcefile));
                    }
                    client.Dispose();
                }
            }
            catch (Exception ex) 
            {
                errorMsg = ex.Message.ToString();
            }

            return errorMsg;
        }

    }
}