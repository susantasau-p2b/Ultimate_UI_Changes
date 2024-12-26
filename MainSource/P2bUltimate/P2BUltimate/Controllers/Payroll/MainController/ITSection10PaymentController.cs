using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class ITSection10PaymentController : Controller
    {
      //  private DataBaseContext db = new DataBaseContext();
        //
        // GET: /ITSection10Payment/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ITSection10Payment/Index.cshtml");
        }

        public ActionResult partial() 
        {
            return View("~/Views/Shared/Payroll/_ITSection10Payment.cshtml");
        }

        public ActionResult partial_ITSection10()
        {
            return View("~/Views/Shared/Payroll/_ItSection10.cshtml");
        }

        public ActionResult GetITSectionByDefault()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITSection.Include(e => e.ITInvestments)
                    .Include(e => e.ITSectionList)
                    .Include(e => e.ITSectionListType)
                    .Where(e => e.ITSectionListType.LookupVal.ToUpper() == "SECTION10B").ToList();
                var returnpara = new
                {
                    Id = fall.Select(a => a.Id.ToString()).ToArray(),
                    FullDetails = fall.Select(a => a.FullDetails).ToArray()
                };

                return Json(returnpara, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetITSectionLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITSection.Include(e => e.ITInvestments).Include(e => e.ITSectionList)
                           .Include(e => e.ITSectionListType)
                           .Where(e => e.ITSectionList.LookupVal.ToUpper() == "SECTION10B").ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ITSection.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetITSection10LKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITSection10.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ITSection10.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }



        public ActionResult Create(ITSection10Payment ITSection10Payment, FormCollection form, String forwarddata) //Create submit
        {
   			List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                    string ITSection = form["ITSectionList"] == "0" ? "" : form["ITSectionList"];
                    string ITSection10 = form["ITSection10list"] == "0" ? "" : form["ITSection10list"];
                    string FianancialYear = form["FinancialYearList"] == "0" ? "" : form["FinancialYearList"];

                    if (FianancialYear != null && FianancialYear != "")
                    {
                        var value = db.Calendar.Find(int.Parse(FianancialYear));
                        ITSection10Payment.FinancialYear = value;

                    }
                    int CompId = 0;
                    if (Session["CompId"] != null)
                        CompId = int.Parse(Session["CompId"].ToString());

                    int id = 0;
                    if (Emp != null && Emp > 0)
                    {
                        id = Emp;
                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    //return Json(new { success = false, responseText = "Kindly select employee" }, JsonRequestBehavior.AllowGet);

                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;


                    OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                    .Include(e => e.EmpOffInfo)
                                    .Where(r => r.Id == Emp).SingleOrDefault();

                    OEmployeePayroll = db.EmployeePayroll.Include(e => e.ITSection10Payment).Include(e => e.ITSection10Payment.Select(r => r.ITSection10))
                       .Include(e => e.ITSection10Payment.Select(r => r.FinancialYear)).Where(e => e.Employee.Id == Emp).SingleOrDefault();

                    if (ITSection != null && ITSection != "")
                    {
                        int ITSectionId = int.Parse(ITSection);
                        ITSection10Payment.ITSection = db.ITSection.Where(e => e.Id == ITSectionId).SingleOrDefault();
                    }
                    else
                    {
                        Msg.Add("  IT Section not defined.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (ITSection10 != null && ITSection10 != "")
                    {
                        int ITSection10Id = int.Parse(ITSection10);
                        ITSection10Payment.ITSection10 = db.ITSection10.Where(e => e.Id == ITSection10Id).SingleOrDefault();
                    }
                    else
                    {
                        Msg.Add("  IT Section10 not defined.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    var calf = OEmployeePayroll.ITSection10Payment.Any(e => e.FinancialYear.Id == ITSection10Payment.FinancialYear.Id && e.InvestmentDate == ITSection10Payment.InvestmentDate && e.ITSection10.Id == int.Parse(ITSection10));
                    if (calf == true)
                    {
                        Msg.Add("  Data Already Exist For This Employee.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                      new System.TimeSpan(0, 30, 0)))
                    {

                        ITSection10Payment.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName.ToString(), IsModified = false };
                        try
                        {
                            ITSection10Payment ITSection10Pay = new ITSection10Payment()
                            {
                                FinancialYear = ITSection10Payment.FinancialYear,
                                InvestmentDate = ITSection10Payment.InvestmentDate,
                                ITSection = ITSection10Payment.ITSection,
                                ITSection10 = ITSection10Payment.ITSection10,
                                Narration = ITSection10Payment.Narration == null ? "" : ITSection10Payment.Narration,
                                ActualInvestment = ITSection10Payment.ActualInvestment,
                                DeclaredInvestment = ITSection10Payment.DeclaredInvestment,
                                DBTrack = ITSection10Payment.DBTrack
                            };


                            db.ITSection10Payment.Add(ITSection10Pay);
                            db.SaveChanges();


                            List<ITSection10Payment> ITSection10PayList = new List<ITSection10Payment>();
                            ITSection10PayList.Add(ITSection10Pay);
                            if (OEmployeePayroll.ITSection10Payment != null)
                            {
                                ITSection10PayList.AddRange(OEmployeePayroll.ITSection10Payment);
                            }
                            OEmployeePayroll.ITSection10Payment = ITSection10PayList;
                            db.EmployeePayroll.Attach(OEmployeePayroll);
                            db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Detached;

                            ts.Complete();

                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                        }
                        catch (DataException ex)
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
                            return Json(new { success = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
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
                //return View();
            }
        }

        public ActionResult GridDelete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var IT10c = db.ITSection10Payment.Find(data);
                db.ITSection10Payment.Remove(IT10c);
                db.SaveChanges();
                //return Json(new Object[] { "", "", " Record Deleted Successfully " }, JsonRequestBehavior.AllowGet);
                List<string> Msgs = new List<string>();
                Msgs.Add("Record Deleted Successfully ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
            }
        }
        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string JoiningDate { get; set; }
            public string Job { get; set; }
            public string Grade { get; set; }
            public string Location { get; set; }
        }

        public class ITSection10PayChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string InvestmentDate { get; set; }
            public double ActualInvestment { get; set; }
            public double DeclaredInvestment { get; set; }
            public string Narration { get; set; }
        }


        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj).Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct).Include(e => e.Employee.PayStruct.Grade)
                        .ToList();
                    // for searchs
                    IEnumerable<EmployeePayroll> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => e.Employee.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<EmployeePayroll, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.Employee.EmpCode : "");
                    var sortcolumn = Request["sSortDir_0"];
                    if (sortcolumn == "asc")
                    {
                        fall = fall.OrderBy(orderfunc);
                    }
                    else
                    {
                        fall = fall.OrderByDescending(orderfunc);
                    }
                    // Paging 
                    var dcompanies = fall
                            .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompanies.Count == 0)
                    {
                        List<returndatagridclass> result = new List<returndatagridclass>();
                        foreach (var item in fall)
                        {
                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                Code = item.Employee.EmpCode,
                                Name = item.Employee.EmpName.FullNameFML,
                                JoiningDate = item.Employee.ServiceBookDates.JoiningDate.Value.ToString(),
                                Job = item.Employee.FuncStruct.Job.Name,
                                Grade = item.Employee.PayStruct.Grade.Name,
                                Location = item.Employee.GeoStruct.Location.LocationObj.LocDesc
                            });
                        }
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = from c in dcompanies

                                     select new[] { null, Convert.ToString(c.Id), c.Employee.EmpCode, };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }//for data reterivation
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public ActionResult Get_ITSection10Payment(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.ITSection10Payment.Select(t => t.ITSection))
                        .Include(e => e.ITReliefPayment.Select(r => r.ITSection))
                        .Include(e => e.ITSection10Payment.Select(t => t.ITSection.ITSectionList))
                        .Where(e => e.Id == data).ToList();
                    if (db_data.Count > 0)
                    {
                        List<ITSection10PayChildDataClass> returndata = new List<ITSection10PayChildDataClass>();
                        foreach (var item in db_data.SelectMany(e => e.ITSection10Payment))
                        {
                            if (item.ITSection != null && item.ITSection.ITSectionList.LookupVal.ToUpper() == "SECTION10B")
                            {


                                returndata.Add(new ITSection10PayChildDataClass
                                {
                                    Id = item.Id,
                                    InvestmentDate = item.InvestmentDate != null ? item.InvestmentDate.Value.ToString("dd/MM/yyyy") : "",
                                    ActualInvestment = item.ActualInvestment,
                                    DeclaredInvestment = item.DeclaredInvestment,
                                    Narration = item.Narration
                                });
                            }
                        }
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
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
        }

        public JsonResult EditGridDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ITSection10Payment.Include(e => e.ITSection)
                    .Include(e => e.ITSection10)
                    .Include(e => e.FinancialYear)
                     .Where(e => e.Id == data).Select
                     (e => new
                     {

                         InvestmentDate = e.InvestmentDate,
                         ActualInvestment = e.ActualInvestment,
                         DeclaredInvestment = e.DeclaredInvestment,
                         Narration = e.Narration,
                         Action = e.DBTrack.Action
                     }).ToList();
                var ITSection10Payment = db.ITSection10Payment.Find(data);
                Session["RowVersion"] = ITSection10Payment.RowVersion;
                var Auth = ITSection10Payment.DBTrack.IsModified;
                //return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
                return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GridEditSave(ITSection10Payment ITP, FormCollection form, string data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    //EmployeePayroll OEmployeePayroll = null;
                    //OEmployeePayroll = db.EmployeePayroll.Include(e => e.ITSection10Payment).Include(e=>e.SalaryT)
                    //   .Include(e => e.ITSection10Payment.Select(r => r.FinancialYear)).Where(e => e. == Emp).SingleOrDefault();
                    int AID = int.Parse(data);
                    var v1 = db.ITSection10Payment.Where(a => a.Id == AID).SingleOrDefault();
                    //  var v = db.EmployeePayroll.Include(a => a.Employee).Include(a => a.SalaryT).Include(a => a.ITSection10Payment).Include(a => a.ITSection10Payment.Select(r => r.FinancialYear));

                    //   var calf = OEmployeePayroll.ITSection10Payment.Any(e => e.FinancialYear.Id == ITSection10Payment.FinancialYear.Id && e.InvestmentDate == ITSection10Payment.InvestmentDate);
                    //  if (calf==true  )
                    //  {
                    //      Msg.Add("  Data Already Exist For THis Employee.  ");
                    //       return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //  }
                    var DeclaredInvestment = form["ITSection10-DeclaredInvestment"] == " 0" ? "" : form["ITSection10-DeclaredInvestment"];
                    var ActualInvestment = form["ITSection10-ActualInvestment"] == " 0" ? "" : form["ITSection10-ActualInvestment"];
                    var Narration = form["ITSection10-Narration"] == "0" ? "" : form["ITSection10-Narration"];

                    ITP.DeclaredInvestment = Convert.ToDouble(DeclaredInvestment);
                    ITP.ActualInvestment = Convert.ToDouble(ActualInvestment);
                    ITP.Narration = Narration;
                    if (data != null)
                    {
                        var id = Convert.ToInt32(data);
                        var db_data = db.ITSection10Payment.Where(e => e.Id == id).SingleOrDefault();
                        db_data.ActualInvestment = ITP.ActualInvestment;
                        db_data.DeclaredInvestment = ITP.DeclaredInvestment;
                        db_data.Narration = ITP.Narration;
                        try
                        {
                            db.ITSection10Payment.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                            Msg.Add("  Record Updated");
                            return Json(new { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                            //return Json(new Utility.JsonReturnClass { data = db_data.ToString()  , status = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new { data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception e)
                        {

                            throw e;
                        }
                    }
                    else
                    {
                        // Msg.Add("  Data Is Null  ");
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        return Json(new { responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
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
        }
	}
}