using EssPortal.App_Start;
using EssPortal.Models;
using EssPortal.Security;
using P2b.Global;
using Payroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace EssPortal.Controllers
{
    public class EmpRegimeSchemeController : Controller
    {
        //
        // GET: /RegimeScheme/
        public ActionResult Index()
        {
            return View("~/Views/RegimeScheme/index.cshtml");
        }

        public ActionResult GetLookupDetailsRegimi(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.RegimiScheme
                    .Include(e => e.FinancialYear)
                    .Include(e => e.FinancialYear.Name)
                    .Include(e => e.Scheme).ToList();
                // IEnumerable<WeeklyOffCalendar> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.RegimiScheme.Include(e => e.FinancialYear)
                    .Include(e => e.Scheme)
                    .Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FinancialYear.FullDetails + " " + ca.Scheme.LookupVal.ToUpper() }).Distinct();
                //var result_1 = (from c in fall
                //                select new { c.Id, c.DepartmentCode, c.DepartmentName });
                return Json(r, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult Create(EmployeePayroll S, FormCollection form, String forwarddata) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    string RegimiSchemelist = form["RegimiSchemelist"] == "" ? null : form["RegimiSchemelist"];
                    int Emp = form["EmpLvnereq_Id"] == "0" ? 0 : Convert.ToInt32(form["EmpLvnereq_Id"]);

                  

                    if (RegimiSchemelist != null)
                    {
                        var ids1 = Utility.StringIdsToListIds(RegimiSchemelist);
                        var RegimiSchemellist = new List<RegimiScheme>();
                        foreach (var item in ids1)
                        {

                            int RegSchid = Convert.ToInt32(item);
                            var val = db.RegimiScheme.Find(RegSchid);
                            if (val != null)
                            {
                                RegimiSchemellist.Add(val);
                            }
                        }
                        S.RegimiScheme = RegimiSchemellist;
                    }
                    else
                    {
                        Msg.Add(" Kindly select RegimeScheme  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    int? FinYrId =  S.RegimiScheme.FirstOrDefault().FinancialYear_Id;

                    RegimiPolicy ORegPolicy = db.RegimiPolicy.Where(e => e.FinancialYear_Id == FinYrId).FirstOrDefault();
                    if (ORegPolicy != null && ORegPolicy.StartDate != null)
                    {
                        DateTime ApplDate =  ORegPolicy.StartDate.Value.AddDays(ORegPolicy.SpanPeriodInDays);
                        if (ApplDate != null && (DateTime.Now.Date >= ApplDate || DateTime.Now.Date <= ORegPolicy.StartDate.Value.Date))
                        {
                            Msg.Add(" Period for entry is over.  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        Msg.Add(" Please define Regime Policy for Selected Financial Year.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    //if (mAtoc < mAfromc)
                    //{
                    //    Msg.Add(" To date Should not be Less Than From Date  ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    S.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                    if (ModelState.IsValid)
                    {
                        if (Emp != 0)
                        {
                            

                                Employee OEmployee = null;
                                EmployeePayroll OEmployeePayroll = null;

                                OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                           .Where(r => r.Id == Emp).FirstOrDefault();

                                OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == Emp).FirstOrDefault();


                                RegimiScheme ORegimiScheme = new RegimiScheme();
                                List<RegimiScheme> OFAT = new List<RegimiScheme>();

                                var regimecheck = db.EmployeePayroll.Include(e => e.RegimiScheme).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                                var duplicaterec = regimecheck.RegimiScheme.Where(e => e.FinancialYear_Id == S.RegimiScheme.FirstOrDefault().FinancialYear_Id).FirstOrDefault();
                                if (duplicaterec != null)
                                {
                                    Msg.Add("You Have Already define Regime scheme " + OEmployee.EmpCode + " If you want Change Edit This employee and Change Scheme");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet); 
                                }
                                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                   new System.TimeSpan(0, 60, 0)))
                                {

                                    if (OEmployeePayroll == null)
                                    {
                                        EmployeePayroll OTEP = new EmployeePayroll()
                                        {
                                            Employee = db.Employee.Find(OEmployee.Id),
                                            RegimiScheme = S.RegimiScheme,
                                            DBTrack = S.DBTrack
                                        };
                                        db.EmployeePayroll.Add(OTEP);
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        var aa = db.EmployeePayroll.Include(e => e.RegimiScheme).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                                        OFAT.AddRange(S.RegimiScheme);
                                        if (aa.RegimiScheme.Count() > 0)
                                        {
                                            OFAT.AddRange(aa.RegimiScheme);
                                            //S.RegimiScheme.Add(aa.RegimiScheme);
                                        }
                                        aa.RegimiScheme = OFAT;
                                        db.EmployeePayroll.Attach(aa);
                                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        // db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                    }


                                    ts.Complete();
                                } 
                        }
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder("");
                        foreach (ModelState modelState in ModelState.Values)
                        {
                            foreach (ModelError error in modelState.Errors)
                            {
                                sb.Append(error.ErrorMessage);
                                sb.Append("." + "\n");
                            }
                        }
                        var errorMsg = sb.ToString();
                        Msg.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }


                    //Msg.Add("  Data Saved successfully");
                    //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    return Json(new { status = true, responseText = "Data Created Successfully." }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    throw;
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
        }

        public ActionResult EditSave(EmployeePayroll RS, string forwardadtad, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            var Emp = Convert.ToInt32(SessionManager.EmpId);
            string[] values = (forwardadtad.Split(new string[] { "," }, StringSplitOptions.None));
            int data = Convert.ToInt32(values[0]);
            string RegimiSchemelist = form["RegimiSchemelist"] == "" ? null : form["RegimiSchemelist"];
            using (DataBaseContext db = new DataBaseContext())
            {
                if (RegimiSchemelist != null)
                {
                    var ids1 = Utility.StringIdsToListIds(RegimiSchemelist);
                    var RegimiSchemellist = new List<RegimiScheme>();
                    foreach (var item in ids1)
                    {

                        int RegSchid = Convert.ToInt32(item);
                        var val = db.RegimiScheme.Find(RegSchid);
                        if (val != null)
                        {
                            RegimiSchemellist.Add(val);
                        }
                    }
                    RS.RegimiScheme = RegimiSchemellist;
                }
                else
                {
                    Msg.Add(" Kindly select RegimeScheme  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }


                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        Employee OEmployee = null;
                        EmployeePayroll OEmployeePayroll = null;

                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                   .Where(r => r.Id == Emp).FirstOrDefault();

                        OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == Emp).FirstOrDefault();


                        RegimiScheme ORegimiScheme = new RegimiScheme();
                        List<RegimiScheme> OFAT = new List<RegimiScheme>();

                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {

                            var aa = db.EmployeePayroll.Include(e => e.RegimiScheme).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                            OFAT.AddRange(RS.RegimiScheme);
                            if (aa.RegimiScheme.Count() > 0)
                            {
                                OFAT.AddRange(aa.RegimiScheme);
                            }
                            aa.RegimiScheme = RS.RegimiScheme;
                            db.EmployeePayroll.Attach(aa);
                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        ts.Complete();
                    }
                    Msg.Add("Record Updated");
                    return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }

        public class GetRegimeSchemeClass
        {
            public string FinancialYear { get; set; }
            public string Scheme { get; set; }
           
            public ChildGetRegimeSchemeClass RowData { get; set; }
        }

        public class ChildGetRegimeSchemeClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }

        }

        public ActionResult GetMyEmpRegimeScheme()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var db_data = db.EmployeePayroll
                    .Include(e => e.RegimiScheme)
                    .Include(e => e.RegimiScheme.Select(z => z.FinancialYear))
                    .Include(e => e.RegimiScheme.Select(z => z.FinancialYear.Name))
                    .Include(e => e.RegimiScheme.Select(t => t.Scheme)) 
                    .Where(e => e.Employee.Id == Emp)
                    .SingleOrDefault();

                List<GetRegimeSchemeClass> returndata = new List<GetRegimeSchemeClass>();
                returndata.Add(new GetRegimeSchemeClass
                {
                    FinancialYear = "FinancialYear",
                    Scheme = "Scheme"
                });

                if (db_data != null && db_data.RegimiScheme != null && db_data.RegimiScheme.Count() > 0)
                {
                    foreach (var item in db_data.RegimiScheme)
                    {
                        returndata.Add(new GetRegimeSchemeClass
                            {
                                RowData = new ChildGetRegimeSchemeClass
                                {
                                    LvNewReq = item.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString(),
                                    IsClose = "",
                                    Status = "",
                                    LvHead_Id = "",
                                },

                                FinancialYear = item.FinancialYear != null ? item.FinancialYear.FullDetails : "",
                                Scheme = item.Scheme != null ? item.Scheme.LookupVal : ""
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

        public ActionResult GetMyRegimeSchemeData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);

                var listOfObject = db.RegimiScheme
                    .Include(e => e.FinancialYear).Include(e => e.Scheme).Include(e => e.FinancialYear.Name)
                .Where(e => e.Id == id).AsEnumerable().Select
                (e => new
                {
                    Id = e.Id,
                    FinancialYear = e.FinancialYear.FullDetails,
                    Scheme = e.Scheme.LookupVal

                }).ToList();

                return Json(new { data = listOfObject, status = false }, JsonRequestBehavior.AllowGet);
            }
        }
	}
}