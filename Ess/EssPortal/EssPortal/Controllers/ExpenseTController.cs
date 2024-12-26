///
/// Created By Anandrao 
/// 


using EssPortal.App_Start;
using EssPortal.Models;
using EssPortal.Security;
using P2b.Global;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using P2B.EExMS;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Configuration;


namespace EssPortal.Controllers
{
    public class ExpenseTController : Controller
    {
        //
        // GET: /ExpenseT/
        public ActionResult Index()
        {
            return View("~/Views/ExpenseT/Index.cshtml");
        }

        public ActionResult expenseTPartialSanction()
        {
            return View("~/Views/Shared/_ExpenseTReqOnSanction.cshtml");
        }

        public ActionResult GetExpCalendarDrop(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var qurey1 = db.ExpenseCalendar.Include(e => e.Calendar).Where(e => e.Calendar.Default == true).ToList();
                var selected1 = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected1 = Convert.ToInt32(data2);
                }

                SelectList s1 = new SelectList(qurey1, "Id", "FullDetails", selected1);

                return Json(s1, JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult getBudgetAllocated(string EmpID, string ExpenseTypelookup, string ExpenseCalendarids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int expensecalids = Convert.ToInt32(ExpenseCalendarids);

                double sanctionamount = 0;
                int empid = Convert.ToInt32(EmpID);
                var getEmpGeoT = db.Employee.Include(e => e.GeoStruct)
                    .Include(e => e.GeoStruct.Location)
                    .Include(e => e.GeoStruct.Division)
                    .Include(e => e.GeoStruct.Department)
                    .Include(e => e.GeoStruct.Group)
                    .Include(e => e.GeoStruct.Unit)
                    .Where(e => e.Id == empid).SingleOrDefault();

                ExpenseBudget Findexpbudget = null;

                Findexpbudget = db.ExpenseBudget.
                Include(e => e.GeoStruct).
                Include(e => e.ExpenseType).
                Include(e => e.ExpenseCalendar).
                Include(e => e.ExpenseCalendar.Calendar)
                .Where(e => e.GeoStruct.Id == getEmpGeoT.GeoStruct.Id && e.ExpenseType.LookupVal == ExpenseTypelookup && e.ExpenseCalendar.Id == expensecalids).FirstOrDefault();

                if (Findexpbudget != null)
                {
                    sanctionamount = Findexpbudget.SanctionAmount;
                }
                else
                {
                    if (getEmpGeoT != null && getEmpGeoT.GeoStruct != null)
                    {


                        var allgeoid = db.GeoStruct.Where(e => e.Location_Id == getEmpGeoT.GeoStruct.Location_Id).ToList();
                        foreach (var Geoid in allgeoid)
                        {
                            if (Geoid.Unit_Id != null)
                            {
                                Findexpbudget = db.ExpenseBudget.Include(e => e.GeoStruct).Include(e => e.ExpenseType).Include(e => e.ExpenseCalendar).Include(e => e.ExpenseCalendar.Calendar).Where(e => e.GeoStruct.Id == Geoid.Id && e.ExpenseType.LookupVal == ExpenseTypelookup && e.ExpenseCalendar.Id == expensecalids).FirstOrDefault();
                                if (Findexpbudget != null)
                                {
                                    sanctionamount = Findexpbudget.SanctionAmount;
                                    break;
                                }

                            }
                            if (Geoid.Group_Id != null)
                            {
                                Findexpbudget = db.ExpenseBudget.Include(e => e.GeoStruct).Include(e => e.ExpenseType).Include(e => e.ExpenseCalendar).Include(e => e.ExpenseCalendar.Calendar).Where(e => e.GeoStruct.Id == Geoid.Id && e.ExpenseType.LookupVal == ExpenseTypelookup && e.ExpenseCalendar.Id == expensecalids).FirstOrDefault();
                                if (Findexpbudget != null)
                                {
                                    sanctionamount = Findexpbudget.SanctionAmount;
                                    break;
                                }

                            }
                            if (Geoid.Department_Id != null)
                            {
                                Findexpbudget = db.ExpenseBudget.Include(e => e.GeoStruct).Include(e => e.ExpenseType).Include(e => e.ExpenseCalendar).Include(e => e.ExpenseCalendar.Calendar).Where(e => e.GeoStruct.Id == Geoid.Id && e.ExpenseType.LookupVal == ExpenseTypelookup && e.ExpenseCalendar.Id == expensecalids).FirstOrDefault();
                                if (Findexpbudget != null)
                                {
                                    sanctionamount = Findexpbudget.SanctionAmount;
                                    break;
                                }

                            }
                            if (Geoid.Location_Id != null)
                            {
                                Findexpbudget = db.ExpenseBudget.Include(e => e.GeoStruct).Include(e => e.ExpenseType).Include(e => e.ExpenseCalendar).Include(e => e.ExpenseCalendar.Calendar).Where(e => e.GeoStruct.Id == Geoid.Id && e.ExpenseType.LookupVal == ExpenseTypelookup && e.ExpenseCalendar.Id == expensecalids).FirstOrDefault();
                                if (Findexpbudget != null)
                                {
                                    sanctionamount = Findexpbudget.SanctionAmount;
                                    break;
                                }

                            }
                            if (Geoid.Division_Id != null)
                            {
                                Findexpbudget = db.ExpenseBudget.Include(e => e.GeoStruct).Include(e => e.ExpenseType).Include(e => e.ExpenseCalendar).Include(e => e.ExpenseCalendar.Calendar).Where(e => e.GeoStruct.Id == Geoid.Id && e.ExpenseType.LookupVal == ExpenseTypelookup && e.ExpenseCalendar.Id == expensecalids).FirstOrDefault();
                                if (Findexpbudget != null)
                                {
                                    sanctionamount = Findexpbudget.SanctionAmount;
                                    break;
                                }

                            }
                        }



                    }

                }

                if (Findexpbudget != null)
                {
                    TempData["OExpenseBudget"] = Findexpbudget.Id;
                    TempData["ExpenseBudget"] = Findexpbudget.Id;
                }

                return Json(sanctionamount, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult getTotalAmt(string IdEmp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                Double ExpAmount = 0;
                Double sanctionamount = 0;
                int empid = Convert.ToInt32(IdEmp);
                List<double> ListofSanAmount = new List<double>();
                List<double> ListofExpAmount = new List<double>();
                
                ExpenseBudget Findexpbudget = null;
                var CalendarId = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "EXPENSECALENDAR" && e.Default == true).SingleOrDefault().Id;
                var ExpenseCalendarId = db.ExpenseCalendar.Where(e => e.Calendar.Id == CalendarId).SingleOrDefault().Id;             
                var getEmpGeoT = db.Employee.Include(e => e.GeoStruct)
                    .Include(e => e.GeoStruct.Location)
                    .Include(e => e.GeoStruct.Division)
                    .Include(e => e.GeoStruct.Department)
                    .Include(e => e.GeoStruct.Group)
                    .Include(e => e.GeoStruct.Unit)
                    .Where(e => e.Id == empid).SingleOrDefault();             
                var qurey = db.Lookup.Where(e => e.Code == "718").Select(e => e.LookupValues.Where(r => r.IsActive == true)).SingleOrDefault();
                foreach (var item in qurey)
                {
                     ExpAmount = 0;
                     sanctionamount = 0;
                    Findexpbudget = db.ExpenseBudget.
                    Include(e => e.GeoStruct).
                    Include(e => e.ExpenseType).
                    Include(e => e.ExpenseCalendar).
                    Include(e => e.ExpenseCalendar.Calendar)
                    .Where(e => e.GeoStruct.Location_Id == getEmpGeoT.GeoStruct.Location_Id && e.ExpenseType.LookupVal == item.LookupVal && e.ExpenseCalendar.Id == ExpenseCalendarId).FirstOrDefault();

                    if (Findexpbudget != null)
                    {
                        sanctionamount = Findexpbudget.SanctionAmount;

                        var OExpensT = db.ExpenseT.Include(e => e.ExpenseBudget).Include(e => e.ExpenseBudget.ExpenseType)
                                                .Where(e => e.ExpenseBudget.Id == Findexpbudget.Id && e.TrReject == false).GroupBy(e => e.ExpenseBudget.ExpenseType.Id).FirstOrDefault();

                        if (OExpensT != null)
                        {
                            ExpAmount = OExpensT.Select(s => s.ExpenseAmount).Sum();
                        }
                        else
                        {
                            ExpAmount = 0;
                        }
                    }
                    else
                    {
                        if (getEmpGeoT != null && getEmpGeoT.GeoStruct != null)
                        {


                            var allgeoid = db.GeoStruct.Where(e => e.Location_Id == getEmpGeoT.GeoStruct.Location_Id).ToList();
                            foreach (var Geoid in allgeoid)
                            {
                                if (Geoid.Unit_Id != null)
                                {
                                    Findexpbudget = db.ExpenseBudget.Include(e => e.GeoStruct).Include(e => e.ExpenseType).Include(e => e.ExpenseCalendar).Include(e => e.ExpenseCalendar.Calendar).Where(e => e.GeoStruct.Id == Geoid.Id && e.ExpenseType.LookupVal == item.LookupVal && e.ExpenseCalendar.Id == ExpenseCalendarId).FirstOrDefault();
                                    if (Findexpbudget != null)
                                    {
                                        sanctionamount = Findexpbudget.SanctionAmount;

                                        var OExpensT = db.ExpenseT.Include(e => e.ExpenseBudget).Include(e => e.ExpenseBudget.ExpenseType)
                                                 .Where(e => e.ExpenseBudget.Id == Findexpbudget.Id && e.TrReject == false).GroupBy(e => e.ExpenseBudget.ExpenseType.Id).FirstOrDefault();

                                        if (OExpensT != null)
                                        {
                                            ExpAmount = OExpensT.Select(s => s.ExpenseAmount).Sum();
                                        }
                                        else
                                        {
                                            ExpAmount = 0;
                                        }
                                        
                                        break;
                                    }

                                }
                                if (Geoid.Group_Id != null)
                                {
                                    Findexpbudget = db.ExpenseBudget.Include(e => e.GeoStruct).Include(e => e.ExpenseType).Include(e => e.ExpenseCalendar).Include(e => e.ExpenseCalendar.Calendar).Where(e => e.GeoStruct.Location_Id == Geoid.Location_Id && e.ExpenseType.LookupVal == item.LookupVal && e.ExpenseCalendar.Id == ExpenseCalendarId).FirstOrDefault();
                                    if (Findexpbudget != null)
                                    {
                                        sanctionamount = Findexpbudget.SanctionAmount;
                                        var OExpensT = db.ExpenseT.Include(e => e.ExpenseBudget).Include(e => e.ExpenseBudget.ExpenseType)
                                                .Where(e => e.ExpenseBudget.Id == Findexpbudget.Id && e.TrReject == false).GroupBy(e => e.ExpenseBudget.ExpenseType.Id).FirstOrDefault();

                                        if (OExpensT != null)
                                        {
                                            ExpAmount = OExpensT.Select(s => s.ExpenseAmount).Sum();
                                        }
                                        else
                                        {
                                            ExpAmount = 0;
                                        }

                                        break;
                                    }

                                }
                                if (Geoid.Department_Id != null)
                                {
                                    Findexpbudget = db.ExpenseBudget.Include(e => e.GeoStruct).Include(e => e.ExpenseType).Include(e => e.ExpenseCalendar).Include(e => e.ExpenseCalendar.Calendar).Where(e => e.GeoStruct.Location_Id == Geoid.Location_Id && e.ExpenseType.LookupVal == item.LookupVal && e.ExpenseCalendar.Id == ExpenseCalendarId).FirstOrDefault();
                                    if (Findexpbudget != null)
                                    {
                                        sanctionamount = Findexpbudget.SanctionAmount;
                                        var OExpensT = db.ExpenseT.Include(e => e.ExpenseBudget).Include(e => e.ExpenseBudget.ExpenseType)
                                                .Where(e => e.ExpenseBudget.Id == Findexpbudget.Id && e.TrReject == false).GroupBy(e => e.ExpenseBudget.ExpenseType.Id).FirstOrDefault();

                                        if (OExpensT != null)
                                        {
                                            ExpAmount = OExpensT.Select(s => s.ExpenseAmount).Sum();
                                        }
                                        else
                                        {
                                            ExpAmount = 0;
                                        }
                                        break;
                                    }

                                }
                                if (Geoid.Location_Id != null)
                                {
                                    Findexpbudget = db.ExpenseBudget.Include(e => e.GeoStruct).Include(e => e.ExpenseType).Include(e => e.ExpenseCalendar).Include(e => e.ExpenseCalendar.Calendar).Where(e => e.GeoStruct.Location_Id == Geoid.Location_Id && e.ExpenseType.LookupVal == item.LookupVal && e.ExpenseCalendar.Id == ExpenseCalendarId).FirstOrDefault();
                                    if (Findexpbudget != null)
                                    {
                                        sanctionamount = Findexpbudget.SanctionAmount;
                                        var OExpensT = db.ExpenseT.Include(e => e.ExpenseBudget).Include(e => e.ExpenseBudget.ExpenseType)
                                                .Where(e => e.ExpenseBudget.Id == Findexpbudget.Id && e.TrReject == false).GroupBy(e => e.ExpenseBudget.ExpenseType.Id).FirstOrDefault();

                                        if (OExpensT != null)
                                        {
                                            ExpAmount = OExpensT.Select(s => s.ExpenseAmount).Sum();
                                        }
                                        else
                                        {
                                            ExpAmount = 0;
                                        }
                                        break;
                                    }

                                }
                                if (Geoid.Division_Id != null)
                                {
                                    Findexpbudget = db.ExpenseBudget.Include(e => e.GeoStruct).Include(e => e.ExpenseType).Include(e => e.ExpenseCalendar).Include(e => e.ExpenseCalendar.Calendar).Where(e => e.GeoStruct.Location_Id == Geoid.Location_Id && e.ExpenseType.LookupVal == item.LookupVal && e.ExpenseCalendar.Id == ExpenseCalendarId).FirstOrDefault();
                                    if (Findexpbudget != null)
                                    {
                                        sanctionamount = Findexpbudget.SanctionAmount;
                                        var OExpensT = db.ExpenseT.Include(e => e.ExpenseBudget).Include(e => e.ExpenseBudget.ExpenseType)
                                                   .Where(e => e.ExpenseBudget.Id == Findexpbudget.Id && e.TrReject == false).GroupBy(e => e.ExpenseBudget.ExpenseType.Id).FirstOrDefault();

                                        if (OExpensT != null)
                                        {
                                            ExpAmount = OExpensT.Select(s => s.ExpenseAmount).Sum();
                                        }
                                        else
                                        {
                                            ExpAmount = 0;
                                        }
                                        break;
                                    }

                                }
                            }
                        }

                    }

                    ListofSanAmount.Add(sanctionamount);
                    ListofExpAmount.Add(ExpAmount);   
                }
                var value1 = ListofSanAmount.Sum();
                var value2 = ListofExpAmount.Sum();

                return Json(new Object[] { value1, value2, "", JsonRequestBehavior.AllowGet });
            }

        }
        public ActionResult getUtilizedAmount(string EmpID)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                //bool Flag = true;
                //int empid = Convert.ToInt32(EmpID);
                //var getEmpGeo = db.Employee.
                //    Include(e => e.GeoStruct).
                //    Where(e => e.Id == empid).Select(g => g.GeoStruct).SingleOrDefault();
                Double ExpAmount = 0;
                var OExpensBudget = TempData["OExpenseBudget"];
                int OExpenseBud_Id = Convert.ToInt32(OExpensBudget);
                ExpenseBudget OExpBudget = null;
                OExpBudget = db.ExpenseBudget.Include(e => e.GeoStruct)
                                            .Include(e => e.ExpenseCalendar)
                                            .Include(e => e.ExpenseCalendar.Calendar)
                                            .Where(e => e.Id == OExpenseBud_Id).SingleOrDefault();
                if (OExpBudget != null)
                {
                    var ExpCal = db.ExpenseCalendar.Include(e => e.Calendar).Where(e => e.Calendar.Default == true).SingleOrDefault();
                    var BudgetAmt = db.ExpenseBudget.Include(e => e.GeoStruct).Include(e => e.ExpenseType).Where(e => e.GeoStruct.Id == OExpBudget.GeoStruct.Id && e.ExpenseCalendar.Id == ExpCal.Id).FirstOrDefault();

                    //ExpenseT OExpensT = null;

                    if (BudgetAmt != null)
                    {
                        var OExpensT = db.ExpenseT.Include(e => e.ExpenseBudget).Include(e => e.ExpenseBudget.ExpenseType)
                                        .Where(e => e.ExpenseBudget.Id == BudgetAmt.Id && e.TrReject == false).GroupBy(e => e.ExpenseBudget.ExpenseType.Id).FirstOrDefault();

                        if (OExpensT != null)
                        {
                            ExpAmount = OExpensT.Select(s => s.ExpenseAmount).Sum();
                        }
                        else
                        {
                            ExpAmount = 0;
                        }

                    }

                }



                return Json(ExpAmount, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult Create(ExpenseT c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int Emp = form["EmpLvnereq_Id"] == "0" ? 0 : Convert.ToInt32(form["EmpLvnereq_Id"]);
                    //  string Emp = form["employee-table"] == "0" ? "" : form["employee-table"];
                    string Country = form["CountryList"] == "0" ? "" : form["CountryList"];
                    string ExpenseAmount = form["ExpenseAmount"] == "0" ? "" : form["ExpenseAmount"];
                    // string MonthYearlist = form["MonthYearlist"] == "0" ? "" : form["MonthYearlist"];
                    string ExEntryDate = form["EntryDate"] == "0" ? "" : form["EntryDate"];
                    string Narration = form["Narration"] == "" ? "" : form["Narration"];

                    string BudgetAllocated = form["BudgetAllocated"] == "0" ? "" : form["BudgetAllocated"];
                    string UtilizedBudget = form["UtilizedBudget"] == "0" ? "" : form["UtilizedBudget"];
                    string empdoclist = form["CandidateDocumentslist"] == "0" ? "" : form["CandidateDocumentslist"];

                    double BudgetAllocatedamount = Convert.ToDouble(BudgetAllocated);

                    if (Session["FilesPath"] != null)
                    {
                        c.FilePath = Session["FilesPath"].ToString();
                    }

                    Double BudgetAllo = 0;
                    if (BudgetAllocated != null && BudgetAllocated != "")
                    {
                        BudgetAllo = Convert.ToDouble(BudgetAllocated) / 12;//Monthly Budget 
                        BudgetAllo = Math.Round(BudgetAllo, 0);
                    }

                    Double UtilizedBud = 0;
                    if (UtilizedBudget != null && UtilizedBudget != "")
                    {
                        UtilizedBud = Convert.ToDouble(UtilizedBudget);
                    }


                    if (ExEntryDate != null)
                    {
                        if (ExEntryDate != "")
                        {
                            var exEntryDT = Convert.ToDateTime(ExEntryDate);
                            c.EntryDate = exEntryDT;

                            var MonthYY = exEntryDT.ToString("MM/yyyy");
                            c.MonthYear = MonthYY;
                        }
                    }

                    if (Narration != null && Narration != "")
                    {
                        var Narr = Convert.ToString(Narration);
                        c.Narration = Narr;
                    }
                    else
                    {
                        c.Narration = null;
                    }

                    if (ExpenseAmount != null)
                    {
                        if (ExpenseAmount != "")
                        {
                            var ExpAmtByEmp = Convert.ToDouble(ExpenseAmount);
                            if ((ExpAmtByEmp + UtilizedBud) <= BudgetAllocatedamount)
                            {
                                c.ExpenseAmount = ExpAmtByEmp;
                            }
                            else
                            {
                                if (Session["FilesPath"] == null)
                                {
                                    Msg.Add("  ExpenseAmount Should be less than Allocated Budget Amount. if excess then upload expense proof ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    c.ExpenseAmount = ExpAmtByEmp;
                                }
                            }

                        }
                        else
                        {
                            c.ExpenseAmount = 0;
                        }
                    }


                    Employee OEmployee = null;

                    OEmployee = db.Employee.Include(e => e.GeoStruct)
                                           .Where(r => r.Id == Emp).SingleOrDefault();
                    //bool Flag = true;
                    //ExpenseBudget ExpBudget = null;
                    //ExpBudget = db.ExpenseBudget.Include(e => e.GeoStruct)
                    //                            .Include(e => e.ExpenseCalendar)
                    //                            .Include(e => e.ExpenseCalendar.Calendar)
                    //                            .Where(e => e.GeoStruct.Id == OEmployee.GeoStruct.Id && e.ExpenseCalendar.Calendar.Default == Flag).SingleOrDefault();

                    var ExpBudgetT = TempData["ExpenseBudget"];
                    int ExpenseBud_Id = Convert.ToInt32(ExpBudgetT);
                    ExpenseBudget ExpBudget = null;
                    ExpBudget = db.ExpenseBudget.Include(e => e.GeoStruct)
                                                .Include(e => e.ExpenseCalendar)
                                                .Include(e => e.ExpenseCalendar.Calendar)
                                                .Where(e => e.Id == ExpenseBud_Id).SingleOrDefault();

                    if (ExpBudget != null)
                    {
                        c.ExpenseBudget = ExpBudget;
                    }
                    else
                    {
                        c.ExpenseBudget = null;
                    }

                    EmployeeExpense GetEmployeeExpense = null;
                    GetEmployeeExpense = new EmployeeExpense
                    {
                        Employee = db.Employee.Where(e => e.Id == Emp).SingleOrDefault(),
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    };


                    ExMSWFDetails oAttWFDetails = new ExMSWFDetails
                    {
                        WFStatus = 0,
                        Comments = c.Narration.ToString(),
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    };

                    List<ExMSWFDetails> oAttWFDetails_List = new List<ExMSWFDetails>();
                    oAttWFDetails_List.Add(oAttWFDetails);



                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            List<ExpenseT> ExpenseTRequest = new List<ExpenseT>();
                            ExpenseT bns = new ExpenseT()
                            {

                                EntryDate = c.EntryDate,
                                ExpenseAmount = c.ExpenseAmount,
                                ExpenseBudget = c.ExpenseBudget,
                                EmployeeExpense = GetEmployeeExpense,
                                MonthYear = c.MonthYear,
                                Narration = c.Narration,
                                WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").SingleOrDefault(), //db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                                GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id),
                                FilePath = c.FilePath != null ? c.FilePath : "",
                                ExMSWFDetails = oAttWFDetails_List,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.ExpenseT.Add(bns);
                                db.SaveChanges();

                                ts.Complete();

                                //return Json(new Utility.JsonReturnClass { Id = bns.Id, Val = bns.HotelDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                return Json(new Utility.JsonClass { status = true, responseText = " Data Saved successfully " }, JsonRequestBehavior.AllowGet);

                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                //   return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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


        public class GetExpneseTReqClass
        {
            public string Emp { get; set; }
            public string ReqDate { get; set; }
            public string EntryDate { get; set; }
            public string BudgetAllocated { get; set; }
            public string UtilizedAmt { get; set; }
            public string ExpenseAmt { get; set; }
            public string MonthYEAR { get; set; }
            public string Status { get; set; }


            public ChildGetExpneseTreqClass RowData { get; set; }
        }


        public class ChildGetExpneseTreqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }

        }

        #region Get ExpenseT Details request On MySelf Dropdown
        public ActionResult GetMyExpenseTReq()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string authority = Convert.ToString(Session["auho"]);

                List<GetExpneseTReqClass> OExpenseTlist = new List<GetExpneseTReqClass>();
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);

                var getEmpId = db.EmployeePayroll.Include(e => e.Employee)
                      .Where(e => e.Id == Id).Select(m => m.Employee).SingleOrDefault();


                var db_data = db.ExpenseT
                        .Include(e => e.EmployeeExpense)
                        .Include(e => e.EmployeeExpense.Employee)
                        .Include(e => e.ExpenseBudget)
                        .Include(e => e.ExpenseBudget.ExpenseType)
                        .Include(e => e.ExMSWFDetails)
                        .Where(e => e.EmployeeExpense.Employee.Id == getEmpId.Id)
                        .ToList();


                if (db_data != null)
                {
                    List<GetExpneseTReqClass> returndata = new List<GetExpneseTReqClass>();
                    returndata.Add(new GetExpneseTReqClass
                    {

                        EntryDate = "Entry Date",
                        BudgetAllocated = "BudgetAllocated",
                        UtilizedAmt = "Utilized Amount",
                        ExpenseAmt = "Expense Amount",
                        MonthYEAR = "MonthYear",
                        Status = "Status"

                    });

                    var ExpenseTReqDetailslist = db_data;

                    Double UtilizeDAmt = 0;
                    foreach (var Expenseitems in ExpenseTReqDetailslist)
                    {
                        var tempUtilizedAmt = db_data.Where(e => e.Id == Expenseitems.Id && e.TrReject == false).GroupBy(e => e.ExpenseBudget.ExpenseType.Id).FirstOrDefault();
                        Double getUtilizeAMT = 0;
                        if (tempUtilizedAmt != null)
                        {
                            getUtilizeAMT = tempUtilizedAmt.Select(u => u.ExpenseAmount).SingleOrDefault();
                            UtilizeDAmt = (UtilizeDAmt + getUtilizeAMT);
                        }
                        else
                        {
                            getUtilizeAMT = 0;
                        }


                        int WfStatusNew = Expenseitems.ExMSWFDetails.Select(w => w.WFStatus).LastOrDefault();
                        string Comments = Expenseitems.ExMSWFDetails.Select(c => c.Comments).LastOrDefault();

                        string StatusNarration = "";
                        if (WfStatusNew == 0)
                            StatusNarration = "Applied";
                        else if (WfStatusNew == 1)
                            StatusNarration = "Sanctioned";
                        else if (WfStatusNew == 2)
                            StatusNarration = "Rejected by Sanction";
                        else if (WfStatusNew == 3)
                            StatusNarration = "Approved";
                        else if (WfStatusNew == 4)
                            StatusNarration = "Rejected by Approval";
                        else if (WfStatusNew == 5)
                            StatusNarration = "Approved By HRM (M)";
                        else if (WfStatusNew == 6)
                            StatusNarration = "Cancelled";

                        if (authority.ToUpper() == "SANCTION" && WfStatusNew == 0)
                        {
                            GetExpneseTReqClass ObjGetExpneseTReqClass = new GetExpneseTReqClass()
                            {
                                RowData = new ChildGetExpneseTreqClass
                                {
                                    LvNewReq = Expenseitems.Id.ToString(),
                                    EmpLVid = Expenseitems.EmployeeExpense.Employee.Id.ToString()

                                },

                                EntryDate = Expenseitems.EntryDate.ToShortDateString(),
                                BudgetAllocated = Expenseitems.ExpenseBudget != null && Expenseitems.ExpenseBudget.SanctionAmount != 0 ? Expenseitems.ExpenseBudget.SanctionAmount.ToString() : "0",
                                UtilizedAmt = UtilizeDAmt.ToString(),
                                ExpenseAmt = Expenseitems.ExpenseAmount.ToString(),
                                MonthYEAR = Expenseitems.MonthYear,
                                Status = StatusNarration

                            };
                            returndata.Add(ObjGetExpneseTReqClass);
                        }

                        else if (authority.ToUpper() == "APPROVAL" && WfStatusNew == 1)
                        {
                            GetExpneseTReqClass ObjGetExpneseTReqClass = new GetExpneseTReqClass()
                            {
                                RowData = new ChildGetExpneseTreqClass
                                {
                                    LvNewReq = Expenseitems.Id.ToString(),
                                    EmpLVid = Expenseitems.EmployeeExpense.Employee.Id.ToString()

                                },

                                EntryDate = Expenseitems.EntryDate.ToShortDateString(),
                                BudgetAllocated = Expenseitems.ExpenseBudget != null && Expenseitems.ExpenseBudget.SanctionAmount != 0 ? Expenseitems.ExpenseBudget.SanctionAmount.ToString() : "0",
                                UtilizedAmt = UtilizeDAmt.ToString(),
                                ExpenseAmt = Expenseitems.ExpenseAmount.ToString(),
                                MonthYEAR = Expenseitems.MonthYear,
                                Status = StatusNarration

                            };
                            returndata.Add(ObjGetExpneseTReqClass);
                        }

                        else if (authority.ToUpper() == "MYSELF")
                        {
                            GetExpneseTReqClass ObjGetExpneseTReqClass = new GetExpneseTReqClass()
                            {
                                RowData = new ChildGetExpneseTreqClass
                                {
                                    LvNewReq = Expenseitems.Id.ToString(),
                                    EmpLVid = Expenseitems.EmployeeExpense.Employee.Id.ToString()

                                },

                                EntryDate = Expenseitems.EntryDate.ToShortDateString(),
                                BudgetAllocated = Expenseitems.ExpenseBudget != null && Expenseitems.ExpenseBudget.SanctionAmount != 0 ? Expenseitems.ExpenseBudget.SanctionAmount.ToString() : "0",
                                UtilizedAmt = UtilizeDAmt.ToString(),
                                ExpenseAmt = Expenseitems.ExpenseAmount.ToString(),
                                MonthYEAR = Expenseitems.MonthYear,
                                Status = StatusNarration

                            };
                            returndata.Add(ObjGetExpneseTReqClass);
                        }

                    }

                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

            }

        } /// Get Created Data on Grid
        #endregion



        public class returnDataClass
        {
            public Int32 EmpId { get; set; }
            public String val { get; set; }
            public List<string> vals { get; set; }
            public Int32 EmpLVid { get; set; }
            public bool Id3 { get; set; }
            public Int32 LvHead_Id { get; set; }

        }

        public class ChildGetExpTReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }
            public string OdReqId { get; set; }

        }

        public class GetExpenseTClass1
        {

            public string EntryDate { get; set; }
            public string BudgetAllocated { get; set; }
            public string UtilizedAmt { get; set; }
            public string ExpenseAmt { get; set; }
            public string MonthYEAR { get; set; }


            public ChildGetExpTReqClass RowData { get; set; }
        }

        #region Get ExpenseT Details request On Santion Dropdown
        public ActionResult GetExpenseTReqOnSanction(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                string AccessRight = "";
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    FuncModule = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "601").FirstOrDefault()
                        .LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }
                // var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);
                if (EmpidsWithfunsub == null && EmpidsWithfunsub.Count == 0)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var returnDataClass = new List<returnDataClass>();

                List<GetExpenseTClass1> returndata = new List<GetExpenseTClass1>();
                returndata.Add(new GetExpenseTClass1
                {

                    EntryDate = "Entry Date",
                    BudgetAllocated = "BudgetAllocated",
                    UtilizedAmt = "Utilized Amount",
                    ExpenseAmt = "Expense Amount",
                    MonthYEAR = "MonthYear",

                    RowData = new ChildGetExpTReqClass
                    {
                        LvNewReq = "0",
                        EmpLVid = "0",
                        IsClose = "0",
                        LvHead_Id = "",
                    }
                });
                foreach (var item1 in EmpidsWithfunsub)
                {


                    var Emps = db.ExpenseT
                        .Where(e => (item1.ReportingEmployee.Contains(e.EmployeeExpense.Employee.Id)))
                        .Include(e => e.EmployeeExpense.Employee)
                        .Include(e => e.EmployeeExpense.Employee.ReportingStructRights)
                        .Include(e => e.EmployeeExpense.Employee.ReportingStructRights.Select(b => b.AccessRights.ActionName))
                        .Include(e => e.EmployeeExpense.Employee.ReportingStructRights.Select(b => b.FuncModules))
                        .Include(e => e.EmployeeExpense.Employee.EmpName)
                        .Include(e => e.ExMSWFDetails)
                        .Include(e => e.WFStatus)
                        .Include(e => e.ExpenseBudget)
                        .Include(e => e.ExpenseBudget.ExpenseType)
                        .Include(e => e.GeoStruct)
                        .ToList();

                    Double UtilizeDAmt = 0;
                    foreach (var item in Emps)
                    {
                        if (item != null)
                        {

                            var LvIds = UserManager.FilterExpenaseT((Emps.OrderByDescending(e => e.EntryDate.ToShortDateString()).ToList()),
                              Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId));

                            if (LvIds.Count() > 0 && LvIds.Contains(item.Id))
                            {

                                var tempUtilizedAmt = Emps.Where(e => LvIds.Contains(e.Id)).GroupBy(e => e.ExpenseBudget.ExpenseType.Id).FirstOrDefault();
                                Double getUtilizeAMT = 0;
                                if (tempUtilizedAmt != null)
                                {
                                    getUtilizeAMT = tempUtilizedAmt.Where(e => e.Id == item.Id).Select(u => u.ExpenseAmount).FirstOrDefault();
                                    if (getUtilizeAMT != 0)
                                    {
                                        UtilizeDAmt = (UtilizeDAmt + getUtilizeAMT);
                                    }

                                }
                                else
                                {
                                    getUtilizeAMT = 0;
                                }

                                var session = Session["auho"].ToString().ToUpper();
                                var EmpR = item.EmployeeExpense.Employee.ReportingStructRights.Where(e => e.AccessRights != null && e.AccessRights.ActionName.LookupVal.ToUpper() == session && e.FuncModules.LookupVal.ToUpper() == item1.ModuleName.ToUpper())
                                .Select(e => e.AccessRights.IsClose).FirstOrDefault();
                                //if (stts.WFStatus == 0)
                                //{

                                returndata.Add(new GetExpenseTClass1
                                {
                                    RowData = new ChildGetExpTReqClass
                                    {
                                        LvNewReq = item.Id.ToString(),
                                        EmpLVid = item.EmployeeExpense.Employee.Id.ToString(),
                                        IsClose = EmpR.ToString(),
                                        LvHead_Id = "",
                                    },

                                    EntryDate = item.EntryDate.ToShortDateString(),
                                    BudgetAllocated = item.ExpenseBudget != null && item.ExpenseBudget.SanctionAmount != 0 ? item.ExpenseBudget.SanctionAmount.ToString() : "0",
                                    UtilizedAmt = UtilizeDAmt.ToString(),
                                    ExpenseAmt = item.ExpenseAmount.ToString(),
                                    MonthYEAR = item.MonthYear,



                                });
                                //  }

                            }
                        }
                    }
                }

                if (returndata != null && returndata.Count > 0)
                {

                    return Json(new { status = true, data = returndata, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = returndata, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        #endregion


        public class EmpExpenseTREquestdata
        {

            public string Status { get; set; }
            public string Emplvhead { get; set; }
            public string ExpenseCalendarList { get; set; }
            public int ExpenseType { get; set; }
            public double BudgetAllocated { get; set; }
            public double UtilizedBudget { get; set; }
            public string EntryDate { get; set; }
            public double ExpenseAmount { get; set; }
            public string Narration { get; set; }
            public string FilePathName { get; set; }


            public bool TrClosed { get; set; }
            public bool TrReject { get; set; }
            public string SanctionCode { get; set; }
            public string SanctionComment { get; set; }
            public string ApporavalComment { get; set; }
            public string Wf { get; set; }
            public string RecomendationCode { get; set; }
            public string RecomendationEmpname { get; set; }
            public Int32 Id { get; set; }
            public Int32 Lvnewreq { get; set; }
            public double RatePerDay { get; set; }
            public string EmployeeName { get; set; }
            public string SpecialRemark { get; set; }
            public string Empcode { get; set; }
            public string Isclose { get; set; }
            public int EmployeeId { get; set; }
            public string Incharge { get; set; }
        }

        #region Get ExpenseT Details request On Santion Dropdown For Sanction Bind DATA

        public ActionResult GetExpenseTData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string authority = Convert.ToString(Session["auho"]);
                var WfStatus = new List<Int32>();
                var SanctionStatus = new List<Int32>();
                var ApporvalStatus = new List<Int32>();
                var RecomendationStatus = new List<Int32>();
                if (authority.ToUpper() == "SANCTION")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "APPROVAL")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                    RecomendationStatus.Add(7);
                    RecomendationStatus.Add(8);
                }
                else if (authority.ToUpper() == "RECOMENDATION")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "MYSELF")
                {
                    SanctionStatus.Add(1);
                    SanctionStatus.Add(2);

                    ApporvalStatus.Add(3);
                    ApporvalStatus.Add(4);
                }
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var status = ids.Count > 0 ? ids[2] : null;
                var emplvId = ids.Count > 0 ? ids[1] : null;
                //var LvHeadId = ids.Count > 0 ? ids[3] : null;

                //var lvheadidint = Convert.ToInt32(LvHeadId);
                var EmpLvIdint = Convert.ToInt32(emplvId);

                var W = db.ExpenseT
                    .Include(e => e.EmployeeExpense.Employee.EmpName)
                    .Include(e => e.EmployeeExpense.Employee.GeoStruct)
                    .Include(e => e.EmployeeExpense.Employee.FuncStruct)
                    .Include(e => e.EmployeeExpense.Employee.PayStruct)
                    .Include(e => e.ExMSWFDetails)
                    .Include(e => e.ExpenseBudget)
                    .Include(e => e.ExpenseBudget.ExpenseCalendar)
                    .Include(e => e.ExpenseBudget.ExpenseType)
                    .Include(e => e.WFStatus)
                    .Include(e => e.GeoStruct)
                    .Where(e => e.EmployeeExpense.Employee.Id == EmpLvIdint && e.Id == id).SingleOrDefault();

                var BudgetAmt = db.ExpenseBudget.Include(e => e.ExpenseCalendar)
                    .Include(e => e.GeoStruct).Include(e => e.ExpenseType)
                    .Where(e => e.GeoStruct.Id == W.GeoStruct.Id && e.ExpenseCalendar.Id == W.ExpenseBudget.ExpenseCalendar.Id)
                    .FirstOrDefault();

                Double ExpAmount = 0;
                if (BudgetAmt != null)
                {
                    var OExpensT = db.ExpenseT.Include(e => e.ExpenseBudget).Include(e => e.ExpenseBudget.ExpenseType)
                                    .Where(e => e.ExpenseBudget.Id == BudgetAmt.Id && e.TrReject == false).GroupBy(e => e.ExpenseBudget.ExpenseType.Id).FirstOrDefault();

                    if (OExpensT != null)
                    {
                        ExpAmount = OExpensT.Select(s => s.ExpenseAmount).Sum();
                    }
                    else
                    {
                        ExpAmount = 0;
                    }

                }


                List<EmpExpenseTREquestdata> EmpExpenseTREquestdataClassList = new List<EmpExpenseTREquestdata>();

                EmpExpenseTREquestdataClassList.Add(new EmpExpenseTREquestdata
                {
                    EmployeeId = W.EmployeeExpense.Employee.Id,
                    EmployeeName = W.EmployeeExpense.Employee.EmpCode + " " + W.EmployeeExpense.Employee.EmpName.FullNameFML,
                    Lvnewreq = W.Id,
                    Empcode = W.EmployeeExpense.Employee.EmpCode,


                    ExpenseCalendarList = W.ExpenseBudget != null && W.ExpenseBudget.ExpenseCalendar != null ? W.ExpenseBudget.ExpenseCalendar.Id.ToString() : "",
                    ExpenseType = W.ExpenseBudget.ExpenseType.Id,
                    BudgetAllocated = W.ExpenseBudget != null && W.ExpenseBudget.SanctionAmount != 0 ? W.ExpenseBudget.SanctionAmount : 0,
                    UtilizedBudget = ExpAmount != 0 ? ExpAmount : 0,
                    EntryDate = W.EntryDate.ToShortDateString(),
                    ExpenseAmount = W.ExpenseAmount != 0 ? W.ExpenseAmount : 0,
                    Narration = W.Narration != null ? W.Narration.ToString() : "",

                    FilePathName = W.FilePath != null ? W.FilePath.ToString() : "No File Uploaded !!!",
                    Isclose = status.ToString(),

                    TrClosed = W.TrClosed,
                    SanctionCode = W.ExMSWFDetails != null && W.ExMSWFDetails.Count > 0 ? W.ExMSWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
                    SanctionComment = W.ExMSWFDetails != null && W.ExMSWFDetails.Count > 0 ? W.ExMSWFDetails.Where(z => SanctionStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                    ApporavalComment = W.ExMSWFDetails != null && W.ExMSWFDetails.Count > 0 ? W.ExMSWFDetails.Where(z => ApporvalStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                    Wf = W.ExMSWFDetails != null && W.ExMSWFDetails.Count > 0 ? W.ExMSWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).LastOrDefault() : null,
                    RecomendationCode = W.ExMSWFDetails != null && W.ExMSWFDetails.Count > 0 ? W.ExMSWFDetails.Where(z => RecomendationStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
                    RecomendationEmpname = W.ExMSWFDetails != null && W.ExMSWFDetails.Count > 0 ? W.ExMSWFDetails.Where(z => RecomendationStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                    // Incharge = s.Incharge != null ? s.Incharge.EmpCode + ' ' + s.Incharge.EmpName.FullDetails.ToString() : null
                });


                return Json(EmpExpenseTREquestdataClassList, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        public ActionResult Update_EXPENSETReq(ExpenseT ExpTReq, FormCollection form, String data)
        {

            string authority = form["authority"] == null ? null : form["authority"];
            var isClose = form["isClose"] == null ? null : form["isClose"];
            if (authority == null && isClose == null)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }

            string ExpensetCancel = form["IsCancel"] == "0" ? "" : form["IsCancel"];
            bool IsCancelexpenset = Convert.ToBoolean(ExpensetCancel);


            var ids = Utility.StringIdsToListString(data);
            var Hbnewreqid = Convert.ToInt32(ids[0]);
            var EmpPayrollId = Convert.ToInt32(ids[1]);
            string Sanction = form["Sanction"];
            string ReasonSanction = form["ReasonSanction"];
            string HR = form["HR"] == null ? null : form["HR"];
            string MySelf = form["MySelf"] == null ? "false" : form["MySelf"];
            string ReasonMySelf = form["ReasonMySelf"] == null ? null : form["ReasonMySelf"];
            string Approval = form["Approval"];
            string ReasonApproval = form["ReasonApproval"];
            string ReasonHr = form["ReasonHr"] == null ? null : form["ReasonHr"];
            string Recomendation = form["Recomendation"];
            string ReasonRecomendation = form["ReasonRecomendation"];
            bool SanctionRejected = false;
            bool HrRejected = false;
            string SanInchargeid = form["SanIncharge_id"];
            string RecInchargeid = form["RecIncharge_id"];
            string AppInchargeid = form["AppIncharge_id"];



            //bool self = false;
            using (DataBaseContext db = new DataBaseContext())
            {

                var ExpenseTvar = db.ExpenseT.Include(e => e.ExMSWFDetails)
                                          .Include(e => e.WFStatus)
                                    .Where(e => e.Id == Hbnewreqid).SingleOrDefault();
                //  access right no of levaefrom days and to days check start
                LookupValue FuncModule = new LookupValue();
                string AccessRight = "";

                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    var lookupdata = db.Lookup
                               .Include(e => e.LookupValues)
                               .Where(e => e.Code == "601").AsNoTracking().FirstOrDefault();
                    FuncModule = lookupdata.LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();


                }

                var query = db.ExpenseT.Where(e => e.Id == Hbnewreqid)
                    .Include(e => e.ExpenseBudget)
                    .Include(e => e.ExpenseBudget.ExpenseCalendar)
                    .Include(e => e.ExpenseBudget.ExpenseType)
                    .Include(e => e.EmployeeExpense.Employee.EmpName)

                    .Include(e => e.ExMSWFDetails)
                    .Include(e => e.EmployeeExpense)
                    .Include(e => e.GeoStruct)
                    .Include(e => e.WFStatus)

                    .ToList();



                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }

                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);


                bool TrClosed = false;

                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        var ExpenseTList = query.ToList();
                        //if someone reject lv
                        foreach (var Exptitems in ExpenseTList)
                        {
                            List<ExMSWFDetails> oExMSWFDetails_List = new List<ExMSWFDetails>();
                            ExMSWFDetails objExMSWFDetails = new ExMSWFDetails();
                            if (authority.ToUpper() == "MYSELF")
                            {
                                if (IsCancelexpenset == true)
                                {
                                    if (ExpenseTvar != null)
                                    {
                                        int WfStatusNew = ExpenseTvar.ExMSWFDetails.ToList().OrderByDescending(e => e.Id).FirstOrDefault().WFStatus;
                                        if (WfStatusNew == 0)
                                        {
                                            objExMSWFDetails = new ExMSWFDetails
                                            {
                                                WFStatus = 6,
                                                Comments = ReasonMySelf,
                                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                            };
                                            oExMSWFDetails_List.Add(objExMSWFDetails);

                                            Exptitems.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "6").FirstOrDefault();
                                            Exptitems.TrClosed = true;
                                            Exptitems.TrReject = true;
                                        }
                                        else
                                        {
                                            return Json(new Utility.JsonClass { status = true, responseText = "Only Applied Data Can be Cancel..!" }, JsonRequestBehavior.AllowGet);
                                        }

                                    }

                                }
                                else
                                {
                                    Exptitems.TrClosed = false;
                                }



                            }
                            if (authority.ToUpper() == "SANCTION")
                            {

                                if (Sanction == null)
                                {
                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Select the Sanction Status" }, JsonRequestBehavior.AllowGet);
                                }
                                if (ReasonSanction == "")
                                {
                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Enter the Reason" }, JsonRequestBehavior.AllowGet);
                                }
                                if (Convert.ToBoolean(Sanction) == true)
                                {
                                    //sanction yes -1

                                    objExMSWFDetails = new ExMSWFDetails
                                    {
                                        WFStatus = 1,
                                        Comments = ReasonSanction,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                    };

                                    oExMSWFDetails_List.Add(objExMSWFDetails);

                                    Exptitems.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                                    Exptitems.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault();


                                }
                                else if (Convert.ToBoolean(Sanction) == false)
                                {

                                    objExMSWFDetails = new ExMSWFDetails
                                    {
                                        WFStatus = 2,
                                        Comments = ReasonSanction,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                    };
                                    oExMSWFDetails_List.Add(objExMSWFDetails);

                                    Exptitems.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "2").FirstOrDefault();
                                    Exptitems.TrClosed = true;
                                    Exptitems.TrReject = true;

                                    //SanctionRejected = true;
                                }
                            }
                            else if (authority.ToUpper() == "APPROVAL")//Hr
                            {
                                if (Approval == null)
                                {
                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Select the Approval Status" }, JsonRequestBehavior.AllowGet);
                                }
                                if (ReasonApproval == "")
                                {
                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Enter the Reason" }, JsonRequestBehavior.AllowGet);
                                }
                                if (Convert.ToBoolean(Approval) == true)
                                {
                                    //approval yes-3
                                    //var CheckAllreadySanction = BAAppTarget.Where(e => e.BA_WorkFlow.Any(r => r.WFStatus == 3)).ToList();
                                    //if (CheckAllreadySanction.Count() > 0)
                                    //{
                                    //    return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Approved....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                    //}
                                    objExMSWFDetails = new ExMSWFDetails
                                    {
                                        WFStatus = 3,
                                        Comments = ReasonApproval,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                    };
                                    oExMSWFDetails_List.Add(objExMSWFDetails);

                                    Exptitems.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                                    Exptitems.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault();
                                    //qurey.BA_WorkFlow.Add(AppWFDetails);
                                    //  qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                                }
                                else if (Convert.ToBoolean(Approval) == false)
                                {

                                    objExMSWFDetails = new ExMSWFDetails
                                    {
                                        WFStatus = 4,
                                        Comments = ReasonApproval,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                    };
                                    oExMSWFDetails_List.Add(objExMSWFDetails);
                                    //  qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();

                                    Exptitems.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "4").FirstOrDefault();
                                    Exptitems.TrClosed = true;
                                    Exptitems.TrReject = true;
                                }
                            }
                            else if (authority.ToUpper() == "RECOMENDATION")
                            {

                                if (Recomendation == null)
                                {
                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Select The Recomendation Status" }, JsonRequestBehavior.AllowGet);
                                }
                                if (ReasonRecomendation == "")
                                {
                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The Reason" }, JsonRequestBehavior.AllowGet);
                                }
                                if (Convert.ToBoolean(Recomendation) == true)
                                {
                                    //Recomendation yes -7
                                    var CheckAllreadyRecomendation = ExpenseTList.Where(e => e.ExMSWFDetails.Any(r => r.WFStatus == 7)).ToList();
                                    if (CheckAllreadyRecomendation.Count() > 0)
                                    {
                                        return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Recomendation....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                    }
                                    objExMSWFDetails = new ExMSWFDetails
                                    {
                                        WFStatus = 7,
                                        Comments = ReasonRecomendation,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                    };
                                    //qurey.BA_WorkFlow.Add(AppWFDetails);

                                }
                                else if (Convert.ToBoolean(Recomendation) == false)
                                {
                                    //Recommendation no -8

                                    var CheckAllreadyRecomendation = ExpenseTList.Where(e => e.ExMSWFDetails.Any(r => r.WFStatus == 8)).ToList();
                                    if (CheckAllreadyRecomendation.Count() > 0)
                                    {
                                        return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Rejected....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                    }
                                    objExMSWFDetails = new ExMSWFDetails
                                    {
                                        WFStatus = 8,
                                        Comments = ReasonRecomendation,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                    };
                                    oExMSWFDetails_List.Add(objExMSWFDetails);

                                    //   qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                                    TrClosed = true;
                                    SanctionRejected = true;
                                }
                            }
                            if (Exptitems.ExMSWFDetails != null)
                            {
                                oExMSWFDetails_List.AddRange(Exptitems.ExMSWFDetails);
                            }

                            Exptitems.ExMSWFDetails = oExMSWFDetails_List;
                            db.ExpenseT.Attach(Exptitems);
                            db.Entry(Exptitems).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            //db.Entry(x).State = System.Data.Entity.EntityState.Detached;   
                        }
                        //qurey.ToList().ForEach(x =>/
                        //{

                        //});

                        //db.BA_TargetT.Attach(qurey);
                        //db.Entry(qurey).State = System.Data.Entity.EntityState.Modified;
                        //db.SaveChanges();
                        //db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception e)
                {
                    return Json(new Utility.JsonClass { status = false, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        [HttpPost]
        public ActionResult FilesUpload(HttpPostedFileBase[] files, FormCollection form, string data)
        {
            if (ModelState.IsValid)
            {
                string Id = form["EmpLvnereq_Id"] == null ? null : form["EmpLvnereq_Id"];
                string EmpCode = "";
                string extension, newfilename, deletefilepath = "";
                Int32 Count = 0;

                string ExEntryDate = form["EntryDate"] == "0" ? "" : form["EntryDate"];

                int Empid = int.Parse(Id);
                using (DataBaseContext db = new DataBaseContext())
                {
                    if (Id != null)
                    {
                        int itid = Convert.ToInt32(Id);
                        EmpCode = db.Employee.Find(itid).EmpCode;
                    }

                    var allowedExtensions = new[] { ".Jpg", ".png", ".jpg", ".jpeg", ".pdf" };
                    foreach (HttpPostedFileBase file in files)
                    {
                        if (file == null)
                        {
                            return Json(new { success = false, responseText = "Please Select The File..!" }, JsonRequestBehavior.AllowGet);
                        }
                        extension = Path.GetExtension(file.FileName);
                        if (!allowedExtensions.Contains(extension))
                        {
                            return Json(new { success = false, responseText = "File Type Is Not Supported..!" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    string Module_Name = Convert.ToString(Session["user-module"]);
                    string ModuleName = Module_Name.ToUpper();

                    foreach (HttpPostedFileBase file in files)
                    {
                        if (file != null)
                        {

                            extension = Path.GetExtension(file.FileName);
                            newfilename = ExEntryDate.Replace("/", "") + extension;
                            String FolderName = Empid + "\\" + ModuleName + "\\" + "ExpenseT";

                            //var InputFileName = Path.GetFileName(file.FileName);
                            //string ServerSavePath = ConfigurationManager.AppSettings["DocumentPath"];
                            //string ServerMappath = ServerSavePath + FolderName + newfilename;
                            string ServerSavePath = ConfigurationManager.AppSettings["EmployeeDocuments"];
                            if (ServerSavePath == null)
                            {
                                return Json(new { success = false, responseText = "Please contact the admin to define the document path." }, JsonRequestBehavior.AllowGet);
                            }
                            string ServerMappath = ServerSavePath + FolderName +"\\"+ newfilename;
                            deletefilepath = ServerMappath;

                            if (deletefilepath != null && deletefilepath != "")
                            {
                                FileInfo File = new FileInfo(deletefilepath);
                                bool exists = File.Exists;
                                if (exists)
                                {
                                    System.IO.File.Delete(deletefilepath);
                                }
                            }

                            if (!Directory.Exists(ServerSavePath + FolderName))
                            {
                                Directory.CreateDirectory(ServerSavePath + FolderName);
                            }
                            file.SaveAs(Path.Combine(ServerMappath));

                            Session["FilesPath"] = FolderName + "\\" + newfilename;

                            Count++;
                        }
                        else
                        {

                        }
                    }

                    if (Count > 0)
                    {
                        return Json(new { success = true, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                    }
                    //else
                    //{
                    //    return Json(new { success = false, responseText = "Something is Wrong..!" }, JsonRequestBehavior.AllowGet);

                    //}
                }
                return View();

            }
            return View();
        }


        [HttpPost]
        public ActionResult Filename(string filepath)
        {
            if (filepath != null && filepath != "")
            {
                if (filepath != null)
                {
                    FileInfo File = new FileInfo(filepath);
                    bool iExists = File.Exists;
                    if (iExists)
                    {
                        filepath = filepath;
                    }
                    else
                    {
                        filepath = ConfigurationManager.AppSettings["EmployeeDocuments"] + filepath;
                    }
                }
                return Json(new { data = filepath }, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        public ActionResult CheckUploadFile(string filepath)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string localpath = "";

                if (filepath != null)
                {
                    localpath = filepath;

                    if (localpath != null)
                    {
                        FileInfo File = new FileInfo(localpath);
                        bool iExists = File.Exists;
                        if (iExists)
                        {
                            localpath = localpath;
                        }
                        else
                        {
                            localpath = ConfigurationManager.AppSettings["EmployeeDocuments"] + localpath;
                        }
                    }
                }

                FileInfo file = new FileInfo(localpath);
                bool exists = file.Exists;
                string extension = Path.GetExtension(file.Name);
                if (exists)
                {
                    return Json(new { success = true, fileextension = extension }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, responseText = "File Not Uploaded..!" }, JsonRequestBehavior.AllowGet);

                }
            }
            return null;
        }

        public ActionResult Imageviewr()
        {
            return View("~/Views/Shared/_ImageViewer.cshtml");
            //D:\LATESTCHECKOUT\P2bUltimate\P2BUltimate\Views\Shared\_Upload.cshtml
        }

        public ActionResult GetCompImage(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string localpath = "";

                if (data != null)
                {
                    localpath = data;

                    if (localpath != null)
                    {
                        FileInfo File = new FileInfo(localpath);
                        bool iExists = File.Exists;
                        if (iExists)
                        {
                            localpath = localpath;
                        }
                        else
                        {
                            localpath = ConfigurationManager.AppSettings["EmployeeDocuments"] + localpath;
                        }
                    }
                }
                else
                {
                    return View("File Not Found");
                    //return Content("File Not Found");
                    //return Json(new { success = false, responseText = "File Not Found..!" }, JsonRequestBehavior.AllowGet);
                }

                FileInfo file = new FileInfo(localpath);
                bool exists = file.Exists;
                string extension = Path.GetExtension(file.Name);

                if (exists)
                {
                    if (extension.ToUpper() == ".PDF")
                    {
                        return File(file.FullName, "application/pdf", file.Name + " ");


                        //string pdf="pdf";
                        //byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");

                        //string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        //return Json(new { data = base64ImageRepresentation, status = pdf }, JsonRequestBehavior.AllowGet);
                    }
                    if (extension.ToUpper() == ".JPG")
                    {
                        // return File(file.FullName, "image/png", file.Name + " ");
                        byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                        string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        //return Json(new { data = base64ImageRepresentation, status = extension }, JsonRequestBehavior.AllowGet);

                        JsonRequestBehavior behaviou = new JsonRequestBehavior();
                        return new JsonResult()
                        {
                            Data = base64ImageRepresentation,
                            MaxJsonLength = 86753090,
                            JsonRequestBehavior = behaviou
                        };

                    }
                    if (extension.ToUpper() == ".PNG")
                    {
                        //return File(file.FullName, "image/png", file.Name + " ");
                        byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                        string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        //return Json(new { data = base64ImageRepresentation, status = extension }, JsonRequestBehavior.AllowGet);

                        JsonRequestBehavior behaviou = new JsonRequestBehavior();
                        return new JsonResult()
                        {
                            Data = base64ImageRepresentation,
                            MaxJsonLength = 86753090,
                            JsonRequestBehavior = behaviou
                        };

                    }
                    if (extension.ToUpper() == ".JPEG")
                    {
                        //return File(file.FullName, "image/png", file.Name + " ");
                        byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                        string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        //return Json(new { data = base64ImageRepresentation, status = extension }, JsonRequestBehavior.AllowGet);

                        JsonRequestBehavior behaviou = new JsonRequestBehavior();
                        return new JsonResult()
                        {
                            Data = base64ImageRepresentation,
                            MaxJsonLength = 86753090,
                            JsonRequestBehavior = behaviou
                        };

                    }
                }
                else
                {
                    return Content("File Not Found");
                    //return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                }
                return null;
            }


        }





    }
}