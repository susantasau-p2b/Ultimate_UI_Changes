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
    [AuthoriseManger]
    public class ITReliefPaymentController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();
        //
        // GET: /ITReliefPayment/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ITReliefPayment/Index.cshtml");
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/Payroll/_ITReliefPayment.cshtml");


        }
        public ActionResult GetITSectionByDefault()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITSection
                   .Include(e => e.ITInvestments)
                   .Include(e => e.ITSectionList)
                   .Include(e => e.ITSectionListType)
                   .Where(e => e.ITSectionListType.LookupVal.ToUpper() == "RELIEF")
                   .ToList();
                var returnpara = new
                {
                    Id = fall.Select(a => a.Id.ToString()).ToArray(),
                    FullDetails = fall.Select(a => a.FullDetails).ToArray()
                };

                return Json(returnpara, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetITSectionLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITSection
                    .Include(e => e.ITInvestments)
                    .Include(e => e.ITSectionList)
                    .Include(e => e.ITSectionListType)
                    .Where(e => e.ITSectionListType.LookupVal.ToUpper() == "RELIEF")
                    .ToList();

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

        #region Create
        public ActionResult Create(ITReliefPayment ITReliefPayment, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                    string ITSection = form["ITSectionlist"] == "0" ? "" : form["ITSectionlist"];
                    string FinancialYearList = form["FinancialYearList"] == "0" ? "" : form["FinancialYearList"];
                    if (FinancialYearList != null && FinancialYearList != "")
                    {
                        var value = db.Calendar.Find(int.Parse(FinancialYearList));
                        ITReliefPayment.FinancialYear = value;

                    }
                    int CompId = 0;
                    if (Session["CompId"] != null)
                        CompId = int.Parse(Session["CompId"].ToString());

                    int id = 0;
                    if (Emp != null && Emp != 0)
                    {
                        id = Emp;
                    }
                    else
                    {
                        //return Json(new Object[] { "", "", "Kindly Select Employee." }, JsonRequestBehavior.AllowGet);
                        Msg.Add(" Kindly Select Employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;


                    OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                    .Include(e => e.EmpOffInfo)
                                    .Where(r => r.Id == Emp).SingleOrDefault();

                    //  OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == Emp).SingleOrDefault();

                    OEmployeePayroll = db.EmployeePayroll.Include(e => e.ITReliefPayment)
                           .Include(e => e.ITReliefPayment.Select(r => r.FinancialYear)).Where(e => e.Employee.Id == Emp).SingleOrDefault();

                    var calf = OEmployeePayroll.ITReliefPayment.Any(e => e.FinancialYear.Id == ITReliefPayment.FinancialYear.Id && e.InvestmentDate == ITReliefPayment.InvestmentDate);
                    if (calf == true)
                    {
                        Msg.Add("  Data Already Exist For THis Employee.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    int ITSectionId = 0;
                    if (ITSection != null && ITSection != "")
                    {
                        ITSectionId = int.Parse(ITSection);
                        ITReliefPayment.ITSection = db.ITSection.Where(e => e.Id == ITSectionId).SingleOrDefault();
                    }
                    else
                    {
                        Msg.Add("  IT Section not defined.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }


                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                      new System.TimeSpan(0, 30, 0)))
                    {

                        ITReliefPayment.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName.ToString(), IsModified = false };
                        try
                        {
                            ITReliefPayment ITReliefPay = new ITReliefPayment()
                            {
                                FinancialYear = ITReliefPayment.FinancialYear,
                                InvestmentDate = ITReliefPayment.InvestmentDate,
                                ITSection = ITReliefPayment.ITSection,
                                Narration = ITReliefPayment.Narration,
                                PaymentName = ITReliefPayment.PaymentName,
                                ReliefAmount = ITReliefPayment.ReliefAmount,
                                DBTrack = ITReliefPayment.DBTrack
                            };


                            db.ITReliefPayment.Add(ITReliefPay);
                            db.SaveChanges();


                            List<ITReliefPayment> ITReliefPayList = new List<ITReliefPayment>();
                            ITReliefPayList.Add(ITReliefPay);
                            OEmployeePayroll.ITReliefPayment = ITReliefPayList;
                            db.EmployeePayroll.Attach(OEmployeePayroll);
                            db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Detached;


                            ts.Complete();


                            //   return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                    return View();
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
        #endregion

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

        public class ITReliefPayChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string PaymentName { get; set; }
            public string InvestmentDate { get; set; }
            public double ReliefAmount { get; set; }
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
                        .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
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
                                Job = item.Employee.FuncStruct!= null  && item.Employee.FuncStruct.Job != null ? item.Employee.FuncStruct.Job.Name.ToString() :"",
                                Grade = item.Employee.PayStruct != null && item.Employee.PayStruct.Grade!=null ? item.Employee.PayStruct.Grade.Name:"",
                                Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null && item.Employee.GeoStruct.Location.LocationObj!=null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc:""
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

        public ActionResult Get_ITReliefPayment(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.ITReliefPayment)
                        .Include(e => e.ITReliefPayment.Select(r => r.ITSection))
                        .Include(e => e.ITReliefPayment.Select(t => t.ITSection.ITSectionListType))
                        .Include(e => e.ITReliefPayment.Select(t => t.ITSection.ITSectionList))
                        .Where(e => e.Id == data).ToList();
                    if (db_data.Count > 0)
                    {
                        List<ITReliefPayChildDataClass> returndata = new List<ITReliefPayChildDataClass>();
                        foreach (var item in db_data.SelectMany(e => e.ITReliefPayment))
                        {
                            if (item.ITSection != null && item.ITSection.ITSectionListType.LookupVal.ToUpper() == "RELIEF")
                            {
                                returndata.Add(new ITReliefPayChildDataClass
                                {
                                    Id = item.Id,
                                    InvestmentDate = item.InvestmentDate != null ? item.InvestmentDate.Value.ToString("dd/MM/yyyy") : "",
                                    PaymentName = item.PaymentName,
                                    ReliefAmount = item.ReliefAmount,
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
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public JsonResult EditGridDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ITReliefPayment
                     .Include(e => e.ITSection)
                    .Include(e => e.FinancialYear)
                     .Where(e => e.Id == data).Select
                     (e => new
                     {
                         InvestmentDate = e.InvestmentDate,
                         PaymentName = e.PaymentName,
                         ReliefAmount = e.ReliefAmount,
                         Narration = e.Narration,
                         Action = e.DBTrack.Action
                     }).ToList();
                var ITReliefPayment = db.ITReliefPayment.Find(data);
                Session["RowVersion"] = ITReliefPayment.RowVersion;
                var Auth = ITReliefPayment.DBTrack.IsModified;
                //return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
                return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GridDelete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var LvEP = db.ITReliefPayment.Find(data);
                db.ITReliefPayment.Remove(LvEP);
                db.SaveChanges();
                //return Json(new Object[] { "", "", " Record Deleted Successfully " }, JsonRequestBehavior.AllowGet);
                List<string> Msgr = new List<string>();
                Msgr.Add("Record Deleted Successfully  ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GridEditSave(ITReliefPayment ITP, FormCollection form, string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ReliefAmount = form["ITReliefPayment-ReliefAmount"] == " 0" ? "" : form["ITReliefPayment-ReliefAmount"];
                var Narration = form["ITReliefPayment-Narration"] == "0" ? "" : form["ITReliefPayment-Narration"];
                ITP.ReliefAmount = Convert.ToDouble(ReliefAmount);
                ITP.Narration = Narration;
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    var db_data = db.ITReliefPayment.Where(e => e.Id == id).SingleOrDefault();
                    db_data.ReliefAmount = ITP.ReliefAmount;
                    db_data.Narration = ITP.Narration;

                    try
                    {
                        db.ITReliefPayment.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        return Json(new { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                        //  return Json(new { data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);

                    }
                    catch (Exception e)
                    {

                        throw e;
                    }
                }
                else
                {
                    return Json(new { responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}