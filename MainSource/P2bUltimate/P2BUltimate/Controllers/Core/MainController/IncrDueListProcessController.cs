using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
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
using System.IO;

namespace P2BUltimate.Controllers.Core.MainController
{
    public class IncrDueListProcessController : Controller
    {

        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /IncrDueListProcess/
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/IncrDueListProcess/Index.cshtml");
        }

        public ActionResult ChkProcess(string typeofbtn, string month)
        {
            bool selected = false;
            // OBJ.Split(',').Select(e => int.Parse(e)).ToList();
            //var Month = month.Split('/')[0].StartsWith("0") == true ? month.Split('/')[0].Remove(0, 1) : month.Split('/')[0];
            //var Year = month.Split('/')[1];
            using (DataBaseContext db = new DataBaseContext())
            {
                
                var query = db.IncrDataCalc.ToList();

                if (query.Count > 0)
                {
                    selected = true;
                }
                var data = new
                {
                    status = selected,
                };
               
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string IncrementProcessDate { get; set; }
            public string IncrementOriginalDate { get; set; }
            public double OldBasic { get; set; }
            public double NewBasic { get; set; }
            public bool StagnantAppl { get; set; }
            public double StagnantCount { get; set; }
        }

        // [HttpPost]
        public ActionResult Create(string month, P2BGrid_Parameters gp, string typeofbtn, string forwardata)
        {
            string empid = forwardata.Replace("?","");
            using (DataBaseContext db = new DataBaseContext())
            {
                string Month = month == "" ? "" : month;
                List<string> Msg = new List<string>();
                string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                bool exists = System.IO.Directory.Exists(requiredPath);
                string localPath;
                if (!exists)
                {
                    localPath = new Uri(requiredPath).LocalPath;
                    System.IO.Directory.CreateDirectory(localPath);
                }
                string path = requiredPath + @"\InrementDate" + ".ini";
                localPath = new Uri(path).LocalPath;
                if (!System.IO.File.Exists(localPath))
                {

                    using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                    {
                        StreamWriter str = new StreamWriter(fs);
                        str.BaseStream.Seek(0, SeekOrigin.Begin);

                        str.Flush();
                        str.Close();
                        fs.Close();
                    }


                }
                // Satara shahakri bank LWP day add fix 30 day month not calenadar month start

                string requiredPaths = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                bool existss = System.IO.Directory.Exists(requiredPaths);
                string localPaths;
                if (!existss)
                {
                    localPaths = new Uri(requiredPaths).LocalPath;
                    System.IO.Directory.CreateDirectory(localPaths);
                }
                string paths = requiredPath + @"\InrementDateFix30days" + ".ini";
                localPaths = new Uri(paths).LocalPath;
                if (!System.IO.File.Exists(localPaths))
                {

                    using (var fs = new FileStream(localPaths, FileMode.OpenOrCreate))
                    {
                        StreamWriter str = new StreamWriter(fs);
                        str.BaseStream.Seek(0, SeekOrigin.Begin);

                        str.Flush();
                        str.Close();
                        fs.Close();
                    }


                }
                // Satara shahakri bank LWP day add fix 30 day month not calenadar month end

                try
                {
                    int CompId = 0;
                    if (!String.IsNullOrEmpty(SessionManager.CompanyId))
                    {
                        CompId = Convert.ToInt32(SessionManager.CompanyId);
                    }

                    if (typeofbtn == "display")
                    {
                        var IncrList = db.IncrDataCalc.ToList();
                        var EmpPayroll = db.EmployeePayroll.Where(e => e.IncrDataCalc != null).ToList();
                        if (IncrList.Count > 0)
                        {
                            db.EmployeePayroll.Where(x => x.IncrDataCalc != null).ToList().ForEach(x =>
                            {
                                x.IncrDataCalc = null;
                            });
                            db.SaveChanges();
                            //foreach (var a in EmpPayroll)
                            //{
                            //    var EmpPay = db.EmployeePayroll.Find(a.Id);
                            //    EmpPay.IncrDataCalc = null;
                            //    db.EmployeePayroll.Attach(EmpPay);
                            //    db.Entry(EmpPay).State = System.Data.Entity.EntityState.Modified;
                            //    db.SaveChanges();
                            //    db.Entry(EmpPay).State = System.Data.Entity.EntityState.Detached;
                            //}

                            db.IncrDataCalc.RemoveRange(IncrList);
                            db.SaveChanges();
                            //foreach (var incrL in IncrList)
                            //{
                            //    db.Entry(incrL).State = System.Data.Entity.EntityState.Deleted;
                            //}

                        }
                         var StagIncrList = db.StagIncrDataCalc.ToList();
                        var EmpPayrollstag = db.EmployeePayroll.Where(e => e.StagIncrDataCalc != null).ToList();
                        if (IncrList.Count > 0)
                        {
                            db.EmployeePayroll.Where(x => x.StagIncrDataCalc != null).ToList().ForEach(x =>
                            {
                                x.StagIncrDataCalc = null;
                            });
                            db.SaveChanges();
                            db.StagIncrDataCalc.RemoveRange(StagIncrList);
                            db.SaveChanges();
                        }
                        List<int> employeepayrollids = new List<int>();
                        var ObjIncrServBook = db.EmployeePayroll.Where(t => t.IncrementServiceBook.Count() > 0).Select(e => new
                        {
                            IncrementServiceBook = e.IncrementServiceBook.Where(s => s.IsHold == true).ToList(),
                            Id = e.Id
                        }).ToList();

                        employeepayrollids = ObjIncrServBook.Where(e => e.IncrementServiceBook.Count() > 0).Select(e => e.Id).ToList();
                       
                        //var EmpList = db.EmployeePayroll.Include(e => e.Employee.EmpOffInfo.PayScale).Include(e => e.EmpSalStruct).Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                        //    .Where(e => e.EmpSalStruct.Count > 0).Distinct().OrderBy(e => e.Employee.EmpCode).AsNoTracking().ToList();

                        //List<string> employeecode = new List<string>{"01302","00826","01282","01273"};
                      // && employeecode.Contains(e.EmployeePayroll.Employee.EmpCode)

                         var EmpList = db.EmpSalStruct
                            .Where(e => !employeepayrollids.Contains(e.EmployeePayroll.Id) && e.EmployeePayroll.Employee.ServiceBookDates!=null && e.EmployeePayroll.Employee.ServiceBookDates.ServiceLastDate == null
                            && e.EndDate == null )
                            .Include(e => e.EmployeePayroll.Employee)
                            .Include(e => e.EmployeePayroll.Employee.EmpOffInfo.PayScale)
                            //.Include(e => e.Employee.EmpOffInfo.PayScale)
                            //.Include(e => e.EmpSalStruct)
                           // .Include(e => e.EmpSalStructDetails)
                           .OrderBy(e=>e.Id)
                            .AsNoTracking()

                        .ToList();

                        foreach (var i in EmpList)
                        {
                            
                            // var OEmpSalStruct = i.EmpSalStruct.Where(e => e.EndDate == null).SingleOrDefault();
                            //if (i.EmpSalStructDetails.Count() > 0)
                            //{
                                var OPayScaleAgr = db.PayScaleAgreement.Include(e => e.PayScale).Include(e => e.IncrActivity)
                                    .Where(e => e.PayScale.Id == i.EmployeePayroll.Employee.EmpOffInfo.PayScale.Id && e.EndDate == null).AsNoTracking().ToList();
                                foreach (var j in OPayScaleAgr)
                                {
                                    if (j.IncrActivity.Count > 0)
                                    {
                                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                                                         new System.TimeSpan(0, 30, 0)))
                                        {
                                            Process.ServiceBook.ServiceBookProcess("",CompId, "INCREMENT_DUEDATE", null, null, null, null, i.EmployeePayroll.Id, "REGULAR", DateTime.Now.Date, true, false, j.Id, null); 
                                            ts.Complete();
                                        }
                                    }
                                }
                           // }
                        }
                        using (DataBaseContext db1 = new DataBaseContext())
                        {
                            try
                            {
                                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                                int pageSize = gp.rows;
                                int totalPages = 0;
                                int totalRecords = 0;
                                var jsonData = (Object)null;

                                IEnumerable<P2BGridData> IncrDueList = null;
                                List<P2BGridData> model = new List<P2BGridData>();
                                P2BGridData view = null;

                                var BindEmpList = db1.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.IncrDataCalc).ToList();

                                foreach (var z in BindEmpList)
                                {
                                    if (z.IncrDataCalc != null)
                                    {
                                        view = new P2BGridData()
                                        {
                                            Id = z.Employee.Id,
                                            Code = z.Employee.EmpCode,
                                            Name = z.Employee.EmpName.FullNameFML,
                                            IncrementOriginalDate = z.IncrDataCalc.OrignalIncrDate.Value.ToString("dd/MM/yyyy"),
                                            IncrementProcessDate = z.IncrDataCalc.ProcessIncrDate.Value.ToString("dd/MM/yyyy"),
                                            NewBasic = z.IncrDataCalc.NewBasic,
                                            OldBasic = z.IncrDataCalc.OldBasic,
                                            StagnantAppl = z.IncrDataCalc.StagnancyAppl,
                                            StagnantCount = z.IncrDataCalc.StagnancyCount
                                        };
                                        model.Add(view);
                                    }

                                }

                                IncrDueList = model;

                                IEnumerable<P2BGridData> IE;
                                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                                {
                                    IE = IncrDueList;
                                    if (gp.searchOper.Equals("eq"))
                                    {
                                        if (gp.searchField == "Id")
                                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                                        else if (gp.searchField == "EmpCode")
                                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.Code.ToString().Contains(gp.searchString)))).ToList();
                                        else if (gp.searchField == "EmpName")
                                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.Name.ToString().Contains(gp.searchString)))).ToList();
                                        else if (gp.searchField == "OldBasic")
                                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.OldBasic.ToString().Contains(gp.searchString)))).ToList();
                                        else if (gp.searchField == "NewBasic")
                                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.NewBasic.ToString().Contains(gp.searchString)))).ToList();
                                        else if (gp.searchField == "IncrementOriginalDate")
                                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.IncrementOriginalDate.Contains(gp.searchString)))).ToList();
                                        else if (gp.searchField == "IncrementProcessDate")
                                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.IncrementProcessDate.Contains(gp.searchString)))).ToList();
                                        else if (gp.searchField == "StagnantAppl")
                                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.StagnantAppl.ToString().Contains(gp.searchString)))).ToList();
                                        else if (gp.searchField == "StagnantCount")
                                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.StagnantCount.ToString().Contains(gp.searchString)))).ToList();

                                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                                    }
                                    if (pageIndex > 1)
                                    {
                                        int h = pageIndex * pageSize;
                                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).ToList();
                                    }
                                    totalRecords = IE.Count();
                                }
                                else
                                {
                                    IE = IncrDueList;
                                    Func<P2BGridData, dynamic> orderfuc;
                                    if (gp.sidx == "Id")
                                    {
                                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                                    }
                                    else
                                    {
                                        orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() :
                                                         gp.sidx == "Name" ? c.Name.ToString() :
                                                         gp.sidx == "OldBasic" ? c.OldBasic.ToString() :
                                                         gp.sidx == "NewBasic" ? c.NewBasic.ToString() :
                                                         gp.sidx == "IncrementOriginalDate" ? c.IncrementOriginalDate.ToString() :
                                                         gp.sidx == "IncrementProcessDate" ? c.IncrementProcessDate.ToString() :
                                                         gp.sidx == "StagnantAppl" ? c.StagnantAppl.ToString() :
                                                         gp.sidx == "StagnantCount" ? c.StagnantCount.ToString() : "");
                                    }
                                    //if (gp.sord == "asc")
                                    //{
                                        //IE = IE.OrderBy(orderfuc);
                                        jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.Name, a.IncrementOriginalDate != null ? Convert.ToString(a.IncrementOriginalDate) : "", a.IncrementProcessDate != null ? Convert.ToString(a.IncrementProcessDate) : "", a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).ToList();
                                    //}
                                    //else if (gp.sord == "desc")
                                    //{
                                    //    IE = IE.OrderByDescending(orderfuc);
                                    //    jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.Name, a.IncrementOriginalDate != null ? Convert.ToString(a.IncrementOriginalDate) : "", a.IncrementProcessDate != null ? Convert.ToString(a.IncrementProcessDate) : "", a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).ToList();
                                    //}
                                    if (pageIndex > 1)
                                    {
                                        int h = pageIndex * pageSize;
                                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name, a.IncrementOriginalDate != null ? Convert.ToString(a.IncrementOriginalDate) : "", a.IncrementProcessDate != null ? Convert.ToString(a.IncrementProcessDate) : "", a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).ToList();
                                    }
                                    totalRecords = IncrDueList.Count();
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

                    }
                    else if (typeofbtn == "process")
                    {
                      
                        if (gp.filter != null)
                        {
                            var Employees = Utility.StringIdsToListIds(empid);
                            var EmpList = db.EmployeePayroll
                                .Include(e => e.Employee.EmpOffInfo.PayScale)
                                                      .Where(e => Employees.Contains(e.Employee.Id)).Distinct().ToList();
                            //var EmpList = db.EmployeePayroll.Include(e => e.Employee.EmpOffInfo.PayScale).Distinct().ToList();

                            foreach (var i in EmpList)
                            {
                                var OPayScaleAgr = db.PayScaleAgreement.Include(e => e.PayScale).Include(e => e.IncrActivity)
                                    .Where(e => e.PayScale.Id == i.Employee.EmpOffInfo.PayScale.Id && e.EndDate == null).ToList();
                                foreach (var j in OPayScaleAgr)
                                {
                                    if (j.IncrActivity.Count > 0)
                                    {
                                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                                                         new System.TimeSpan(0, 30, 0)))
                                        {
                                            try
                                            {


                                                Process.ServiceBook.IncrementProcess("",i.Id, CompId, "REGULAR", DateTime.Now.Date, false, false);
                                                using (DataBaseContext db2 = new DataBaseContext())
                                                {
                                                    IncrementServiceBook OIncrementServiceBook = db2.EmployeePayroll.Include(e => e.IncrementServiceBook).Where(e => e.Id == i.Id).Select(e => e.IncrementServiceBook.OrderByDescending(a => a.Id).FirstOrDefault()).FirstOrDefault();
                                                    Process.ServiceBook.ServiceBookProcess("",CompId, "INCREMENT_RELEASE", OIncrementServiceBook.Id, null, null, null, i.Id, "INCREMENT", OIncrementServiceBook.ProcessIncrDate, false, false, j.Id, null);
                                                }


                                                ts.Complete();
                                                // return Json(new { sucess = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                            }
                                            catch (DataException ex)
                                            {
                                               // List<string> Msg = new List<string>();
                                                Msg.Add(ex.Message);
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
                                                return Json(new { status = false, Msg }, JsonRequestBehavior.AllowGet);
                                            }
                                        }
                                    }
                                }
                            }

                            return Json(new { status = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                        }


                        //else if (typeofbtn == "release")
                        //{
                        //    var Emp = Utility.StringIdsToListIds(gp.filter);
                        //    var ReleaseFlag = form["ReleaseFlag"] == "0" ? "" : form["ReleaseFlag"];

                        //    int CompId = 0;
                        //    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                        //    {
                        //        CompId = Convert.ToInt32(Session["CompId"]);
                        //    }

                        //    int Empid = 0;
                        //    if (Emp != null)
                        //    {
                        //        Empid = Emp;
                        //    }

                        //    Employee OEmployee = null;
                        //    EmployeePayroll OEmployeePayroll = null;
                        //    //int PayScaleAgrId = int.Parse(PayScaleAgr);
                        //    //var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgrId).SingleOrDefault();

                        //    OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                        //                    .Include(e => e.EmpOffInfo)
                        //                    .Where(r => r.Id == Empid).SingleOrDefault();

                        //    OEmployeePayroll = db.EmployeePayroll
                        //  .Where(e => e.Employee.Id == Empid).SingleOrDefault();
                        //    int IncrId = int.Parse(forwarddata);

                        //    IncrementServiceBook.ReleaseFlag = Convert.ToBoolean(ReleaseFlag);
                        //    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                        //                      new System.TimeSpan(0, 30, 0)))

                        //        if (IncrementServiceBook.ReleaseFlag == true)
                        //        {


                        //            try
                        //            {
                        //                IncrementServiceBook OIncrementServiceBook = db.IncrementServiceBook.Where(e => e.Id == IncrId).SingleOrDefault();
                        //                Process.ServiceBook.ServiceBookProcess(CompId, "INCREMENT_RELEASE", OIncrementServiceBook, null, null, null, OEmployeePayroll.Id, "INCREMENT", IncrementServiceBook.ReleaseDate, false, false, db);
                        //                ts.Complete();
                        //                return Json(new { success = true, responseText = "Data Updated Successfully." }, JsonRequestBehavior.AllowGet);
                        //            }
                        //            catch (DataException ex)
                        //            {
                        //                LogFile Logfile = new LogFile();
                        //                ErrorLog Err = new ErrorLog()
                        //                {
                        //                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        //                    ExceptionMessage = ex.Message,
                        //                    ExceptionStackTrace = ex.StackTrace,
                        //                    LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        //                    LogTime = DateTime.Now
                        //                };
                        //                Logfile.CreateLogFile(Err);
                        //                return Json(new { success = false, responseText = ex.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                        //            }

                        //        }
                        //        else
                        //        {
                        //            return Json(new { success = false, responseText = "" }, JsonRequestBehavior.AllowGet);

                        //        }



                        //}
                    }
                }
                catch (Exception ex)
                {

                   // List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);
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
                    return Json(new { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                return View();
            }
        }
        public ActionResult HoldIncrement(IncrementServiceBook IncrementServiceBook, FormCollection form, string ReleaseDate, string EmpId) //Create submit
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    try
                    {
                        //   var Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                        //var Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];

                        string IncrementActivity = form["IncrActivitylist"] == "0" ? "" : form["IncrActivitylist"];
                        string IncrementPolicy = form["incrpolicy"] == "0" ? "" : form["incrpolicy"];
                        string holdNarration = form["holdNarration"] == "0" ? "" : form["holdNarration"];
                        DateTime? currentdate = DateTime.Now;
                        var date = Convert.ToDateTime(ReleaseDate).ToString("MM/yyyy");

                        DateTime? HoldReleaseDate = Convert.ToDateTime(ReleaseDate);

                        int CompId = 0;
                        if (SessionManager.UserName != null)
                        {
                            CompId = Convert.ToInt32(Session["CompId"]);
                        }

                        List<int> ids = null;
                        if (EmpId != null && EmpId != "0" && EmpId != "false")
                        {
                            ids = Utility.StringIdsToListIds(EmpId);
                        }
                        else
                        {
                            Msg.Add(" Kindly select employee  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        ////////new 21/08/2019
                        //var check = db.CPIEntryT.Where(e => e.PayMonth == date).ToList();

                        //if (check.Count() == 0)
                        //{
                        //    Msg.Add("Kindly run CPI first and then try again");
                        //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //}
                        ///////////

                        Employee OEmployee = null;
                        EmployeePayroll OEmployeePayroll = null;

                        List<string> MsgCheck = new List<string>();
                        foreach (var i in ids)
                        {
                            OEmployee = db.Employee.Include(q => q.EmpName).Where(r => r.Id == i).AsNoTracking().AsParallel().SingleOrDefault();
                            OEmployeePayroll = db.EmployeePayroll.Include(e => e.IncrementServiceBook).Include(e => e.IncrDataCalc).Where(e => e.Employee.Id == OEmployee.Id).AsNoTracking().AsParallel().SingleOrDefault();

                            if (OEmployeePayroll.IncrementServiceBook.Any(d => d.ProcessIncrDate.Value.ToShortDateString() == OEmployeePayroll.IncrDataCalc.ProcessIncrDate.ToString()))
                            {
                                {
                                    MsgCheck.Add("Already increment done for employee " + OEmployee.EmpCode + " " + OEmployee.EmpName.FullNameFML + " on date= " + OEmployeePayroll.IncrDataCalc.ProcessIncrDate);
                                    // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            var IsHoldRecord = OEmployeePayroll.IncrementServiceBook.Where(e => e.IsHold == true).FirstOrDefault();
                            if (IsHoldRecord != null)
                            {
                                MsgCheck.Add("Already increment Hold this Employee" + " " + OEmployee.EmpCode + " " + OEmployee.EmpName.FullNameFML);

                            }
                        }
                        if (MsgCheck.Count > 0)
                        {
                            return Json(new Utility.JsonReturnClass { success = false, responseText = MsgCheck }, JsonRequestBehavior.AllowGet);
                        }
                        foreach (var i in ids)
                        {
                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                            .Include(e => e.EmpOffInfo)
                                            .Where(r => r.Id == i).SingleOrDefault();

                            OEmployeePayroll = db.EmployeePayroll.Include(e => e.IncrementServiceBook).Include(e=>e.IncrDataCalc).Where(e => e.Employee.Id == i).SingleOrDefault();


                            if (OEmployeePayroll.IncrementServiceBook.Any(d => d.ProcessIncrDate.Value.ToShortDateString() == OEmployeePayroll.IncrDataCalc.ProcessIncrDate.ToString()))
                            {
                                {
                                    Msg.Add("Alredy increment for Date= " + OEmployeePayroll.IncrDataCalc.ProcessIncrDate);
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }


                            //if (OEmployee.GeoStruct != null)
                            //    IncrementServiceBook.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);

                            //if (OEmployee.FuncStruct != null)
                            //    IncrementServiceBook.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);

                            //if (OEmployee.PayStruct != null)
                            //    IncrementServiceBook.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);

                            //int IncrementPolicyId = 0;
                            //int IncrActivityId = 0;
                            //string LookupVal = "REGULAR";
                            //if (IncrementActivity != null && IncrementActivity != "")
                            //{
                            //    IncrActivityId = int.Parse(IncrementActivity);
                            //    //LookupVal = db.LookupValue.Where(e => e.Id == IncrActivityId).Select(e => e.LookupVal.ToUpper()).SingleOrDefault();
                            //    LookupVal = db.IncrActivity.Where(e => e.Id == IncrActivityId).Select(e => e.IncrList.LookupVal.ToUpper()).SingleOrDefault();
                            //    IncrementServiceBook.IncrActivity = db.IncrActivity.Where(e => e.Id == IncrActivityId).SingleOrDefault();
                            //}

                            //if (IncrementPolicy != null && IncrementPolicy != "")
                            //{
                            //    IncrementPolicyId = int.Parse(IncrementPolicy);
                            //}


                            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                              new System.TimeSpan(0, 30, 0)))
                            {

                                Process.ServiceBook.IncrementProcess("",OEmployeePayroll.Id, CompId, "REGULAR", OEmployeePayroll.IncrDataCalc.ProcessIncrDate, false, false);

                                ts.Complete();
                            }
                            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                             new System.TimeSpan(0, 30, 0)))
                            {

                                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                                IncrementServiceBook OIncrementServiceBook = db.EmployeePayroll.Include(e => e.IncrementServiceBook).Where(e => e.Id == OEmployeePayroll.Id).Select(e => e.IncrementServiceBook.OrderByDescending(a => a.Id).FirstOrDefault()).FirstOrDefault(); ;
                                OIncrementServiceBook.IsHold = true;
                                OIncrementServiceBook.ReleaseDate = IncrementServiceBook.ReleaseDate;
                                db.IncrementServiceBook.Attach(OIncrementServiceBook);
                                db.Entry(OIncrementServiceBook).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = OIncrementServiceBook.RowVersion;
                                // db.Entry(OIncrementServiceBook).State = System.Data.Entity.EntityState.Detached;
                                IncrementHoldReleaseDetails incrementhold = new IncrementHoldReleaseDetails
                                {
                                    DBTrack = dbt,
                                    IncrementServiceBook = OIncrementServiceBook,
                                    HoldNarration = holdNarration,
                                    ReleaseDate = IncrementServiceBook.ReleaseDate,
                                    HoldDate = currentdate
                                };
                                db.IncrementHoldReleaseDetails.Add(incrementhold);
                                db.SaveChanges();

                                ts.Complete();
                            }
                        }
                        Msg.Add("Data Saved successfully");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

                        //     List<string> Msg = new List<string>();
                        Msg.Add(ex.Message);

                        //return Json(new Object[] { "", "", Msg }, JsonRequestBehavior.AllowGet);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile Logfile = new LogFile();
                ErrorLog Err = new ErrorLog()
                {
                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                    ExceptionMessage = ex.Message,
                    ExceptionStackTrace = ex.StackTrace,
                    LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                    LogTime = DateTime.Now
                };
                Logfile.CreateLogFile(Err);
                Msg.Add(ex.Message);
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;

        //        IEnumerable<P2BGridData> IncrDueList = null;
        //        List<P2BGridData> model = new List<P2BGridData>();
        //        P2BGridData view = null;

        //        string PayMonth = "";
        //        string Month = "";
        //        if (gp.filter != null)
        //            PayMonth = gp.filter;
        //        else
        //        {
        //            if (DateTime.Now.Date.Month < 10)
        //                Month = "0" + DateTime.Now.Date.Month;
        //            else
        //                Month = DateTime.Now.Date.Month.ToString();
        //            PayMonth = Month + "/" + DateTime.Now.Date.Year;
        //        }
        //        var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.IncrDataCalc).ToList();

        //        foreach (var z in BindEmpList)
        //        {
        //            if (z.IncrDataCalc != null)
        //            {
        //                string mon = z.IncrDataCalc.ProcessIncrDate.Value.Month.ToString().Length == 1 ? "0" + z.IncrDataCalc.ProcessIncrDate.Value.Month.ToString() : z.IncrDataCalc.ProcessIncrDate.Value.Month.ToString();
        //                string IncrMonYr = mon + "/" + z.IncrDataCalc.ProcessIncrDate.Value.Year;
        //                if (IncrMonYr == PayMonth)
        //                {
        //                    view = new P2BGridData()
        //                    {
        //                        Id = z.Employee.Id,
        //                        Code = z.Employee.EmpCode,
        //                        Name = z.Employee.EmpName.FullNameFML,
        //                        IncrementOriginalDate = z.IncrDataCalc.OrignalIncrDate.Value.ToString("dd/MM/yyyy"),
        //                        IncrementProcessDate = z.IncrDataCalc.ProcessIncrDate.Value.ToString("dd/MM/yyyy"),
        //                        NewBasic = z.IncrDataCalc.NewBasic,
        //                        OldBasic = z.IncrDataCalc.OldBasic,
        //                        StagnantAppl = z.IncrDataCalc.StagnancyAppl,
        //                        StagnantCount = z.IncrDataCalc.StagnancyCount
        //                    };
        //                    model.Add(view);
        //                }
        //            }

        //        }

        //        IncrDueList = model;

        //        IEnumerable<P2BGridData> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = IncrDueList;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "Id")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                if (gp.searchField == "EmpCode")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.Code.ToString().Contains(gp.searchString)))).ToList();
        //                if (gp.searchField == "EmpName")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.Name.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "OldBasic")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.OldBasic.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "NewBasic")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.NewBasic.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "IncrementOriginalDate")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.IncrementOriginalDate.Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "IncrementProcessDate")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.IncrementProcessDate.Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "StagnantAppl")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.StagnantAppl.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "StagnantCount")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).Where((e => (e.StagnantCount.ToString().Contains(gp.searchString)))).ToList();

        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = IncrDueList;
        //            Func<P2BGridData, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() :
        //                                 gp.sidx == "Name" ? c.Name.ToString() :
        //                                 gp.sidx == "OldBasic" ? c.OldBasic.ToString() :
        //                                 gp.sidx == "NewBasic" ? c.NewBasic.ToString() :
        //                                 gp.sidx == "IncrementOriginalDate" ? c.IncrementOriginalDate.ToString() :
        //                                 gp.sidx == "IncrementProcessDate" ? c.IncrementProcessDate.ToString() :
        //                                 gp.sidx == "StagnantAppl" ? c.StagnantAppl.ToString() :
        //                                 gp.sidx == "StagnantCount" ? c.StagnantCount.ToString() : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.Name, a.IncrementOriginalDate != null ? Convert.ToString(a.IncrementOriginalDate) : "", a.IncrementProcessDate != null ? Convert.ToString(a.IncrementProcessDate) : "", a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.Name, a.IncrementOriginalDate != null ? Convert.ToString(a.IncrementOriginalDate) : "", a.IncrementProcessDate != null ? Convert.ToString(a.IncrementProcessDate) : "", a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name, a.IncrementOriginalDate != null ? Convert.ToString(a.IncrementOriginalDate) : "", a.IncrementProcessDate != null ? Convert.ToString(a.IncrementProcessDate) : "", a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount }).ToList();
        //            }
        //            totalRecords = IncrDueList.Count();
        //        }
        //        if (totalRecords > 0)
        //        {
        //            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //        }
        //        if (gp.page > totalPages)
        //        {
        //            gp.page = totalPages;
        //        }
        //        var JsonData = new
        //        {
        //            page = gp.page,
        //            rows = jsonData,
        //            records = totalRecords,
        //            total = totalPages
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> IncrDueList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;

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
                    List<int> employeepayrollids = new List<int>();
                    var ObjIncrServBook = db.EmployeePayroll.Where(t => t.IncrementServiceBook.Count() > 0).Select(e => new
                    {
                        IncrementServiceBook = e.IncrementServiceBook.Where(s => s.IsHold == true).ToList(),
                        Id = e.Id
                    }).ToList();

                    employeepayrollids = ObjIncrServBook.Where(e => e.IncrementServiceBook.Count() > 0).Select(e => e.Id).ToList();
                    var BindEmpList = db.EmployeePayroll.Where(e => !employeepayrollids.Contains(e.Id)).Include(e => e.Employee.EmpName).Include(e => e.IncrDataCalc).ToList();

                    foreach (var z in BindEmpList)
                    {
                        if (z.IncrDataCalc != null)
                        {
                            string mon = z.IncrDataCalc.ProcessIncrDate.Value.Month.ToString().Length == 1 ? "0" + z.IncrDataCalc.ProcessIncrDate.Value.Month.ToString() : z.IncrDataCalc.ProcessIncrDate.Value.Month.ToString();
                            string IncrMonYr = mon + "/" + z.IncrDataCalc.ProcessIncrDate.Value.Year;
                            if (IncrMonYr == PayMonth)
                            {
                                view = new P2BGridData()
                                {
                                    Id = z.Employee.Id,
                                    Code = z.Employee.EmpCode,
                                    Name = z.Employee.EmpName.FullNameFML,
                                    IncrementOriginalDate = z.IncrDataCalc.OrignalIncrDate.Value.ToString("dd/MM/yyyy"),
                                    IncrementProcessDate = z.IncrDataCalc.ProcessIncrDate.Value.ToString("dd/MM/yyyy"),
                                    NewBasic = z.IncrDataCalc.NewBasic,
                                    OldBasic = z.IncrDataCalc.OldBasic,
                                    StagnantAppl = z.IncrDataCalc.StagnancyAppl,
                                    StagnantCount = z.IncrDataCalc.StagnancyCount
                                };
                                model.Add(view);
                            }
                        }

                    }

                    IncrDueList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = IncrDueList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                                || (e.Name.ToString().Contains(gp.searchString))
                                || (e.IncrementOriginalDate.ToString().Contains(gp.searchString))
                                || (e.IncrementProcessDate.ToString().Contains(gp.searchString))
                                || (e.OldBasic.ToString().Contains(gp.searchString))
                                || (e.NewBasic.ToString().Contains(gp.searchString))
                                || (e.StagnantAppl.ToString().Contains(gp.searchString))
                                || (e.StagnantCount.ToString().Contains(gp.searchString))
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.IncrementOriginalDate), Convert.ToString(a.IncrementProcessDate), Convert.ToString(a.OldBasic), Convert.ToString(a.NewBasic), Convert.ToString(a.StagnantAppl), Convert.ToString(a.StagnantCount), a.Id }).Where(a => a.Contains((gp.searchString))).ToList();

                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.IncrementOriginalDate, a.IncrementProcessDate, a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount, a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = IncrDueList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() :
                                             gp.sidx == "Name" ? c.Name.ToString() :
                                             gp.sidx == "IncrementOriginalDate" ? c.IncrementOriginalDate.ToString() :
                                             gp.sidx == "IncrementProcessDate" ? c.IncrementProcessDate.ToString() :
                                             gp.sidx == "OldBasic" ? c.OldBasic.ToString() :
                                             gp.sidx == "NewBasic" ? c.NewBasic.ToString() :
                                             gp.sidx == "StagnantAppl" ? c.StagnantAppl.ToString() :
                                             gp.sidx == "StagnantCount" ? c.StagnantCount.ToString() : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.IncrementOriginalDate != null ? Convert.ToString(a.IncrementOriginalDate) : "", a.IncrementProcessDate != null ? Convert.ToString(a.IncrementProcessDate) : "", a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount, a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.IncrementOriginalDate != null ? Convert.ToString(a.IncrementOriginalDate) : "", a.IncrementProcessDate != null ? Convert.ToString(a.IncrementProcessDate) : "", a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.IncrementOriginalDate != null ? Convert.ToString(a.IncrementOriginalDate) : "", a.IncrementProcessDate != null ? Convert.ToString(a.IncrementProcessDate) : "", a.OldBasic, a.NewBasic, a.StagnantAppl, a.StagnantCount, a.Id }).ToList();
                        }
                        totalRecords = IncrDueList.Count();
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
        }


        public ActionResult ChkIFManual(FormCollection form, string PayMonth)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();

                var check = db.CPIEntryT.Where(e => e.PayMonth == PayMonth).ToList();

                if (check.Count() == 0)
                {
                    Msg.Add("Kindly run CPI first and then try again");
                    //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    return Json(new { status = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                //return Json(new Utility.JsonReturnClass { success = true, responseText = "" }, JsonRequestBehavior.AllowGet);
                return Json(new { status = false, responseText = "" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
