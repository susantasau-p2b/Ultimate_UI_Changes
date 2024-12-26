using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Collections;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using Leave;
using P2BUltimate.Security;
using Payroll;
namespace P2BUltimate.Controllers.Leave
{
    [AuthoriseManger]
    public class LvOpenBalController : Controller
    {

        // private DataBaseContext db = new DataBaseContext();

        // GET: /LeaveOpeningBalance/
        public ActionResult Index()
        {
            return View("~/Views/Leave/MainViews/LvOpenBal/Index.cshtml");
        }

        public JsonResult Polulate_LeaveCalendar(string data)
        {
            //int id = int.Parse(data);
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").ToList();
                return Json(a, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLookupLvHead(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LvHead.ToList();
                IEnumerable<LvHead> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LvHead.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }

        }


        //public ActionResult GetLookupCalendar(List<int> SkipIds)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.Calendar.Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();



        //       // var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
        //        return Json(fall, JsonRequestBehavior.AllowGet);


        //    }

        //}

        [HttpPost]
        public ActionResult GetLookupCalendar(List<int> SkipIds)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default.ToString() == "True").ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Calendar.Include(e => e.Name).Where(e => !e.Id.ToString().Contains(a.ToString()) && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default.ToString() == "True").ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = "Name :" + ca.Name.LookupVal.ToString() + ", From Date :" + ca.FromDate.Value.ToShortDateString() + ", To Date :" + ca.ToDate.Value.ToShortDateString() + ", Default :" + ca.Default.ToString() }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult getCalendarL()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).AsEnumerable()
                    .Select(e => new
                    {
                        Id = e.Id,
                        Lvcalendardesc = "From Date :" + e.FromDate.Value.ToShortDateString() + " To Date :" + e.ToDate.Value.ToShortDateString(),

                    }).SingleOrDefault();
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }

        //Create New Request openining balance //
        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }
        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Leave/_LvOpenBalGridPartial.cshtml");
        }
        public ActionResult GridEditData(int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.LvOpenBal
                  .Where(e => e.Id == data).Select
                  (e => new
                  {
                      LvCredit = e.LvCredit,
                      LvCreditDate = e.LvCreditDate,
                      LvOccurances = e.LvOccurances,
                      LvOpening = e.LvOpening,
                      LvUtilized = e.LvUtilized,

                  }).SingleOrDefault();
                return Json(Q, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GridEditSave(LvOpenBal L, string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != null)
                {
                    var ids = Utility.StringIdsToListIds(data);

                    int empidintrnal = Convert.ToInt32(ids[0]);
                    int empidMain = Convert.ToInt32(ids[1]);

                    //var id = Convert.ToInt32(data);
                    var db_data = db.LvOpenBal.Include(e => e.LvHead)
                  .Where(e => e.Id == empidintrnal).SingleOrDefault();

                    var lvcalendarid = db.Calendar.Include(e => e.Name)
                      .Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                    var a = db.EmployeeLeave.Include(e => e.LvNewReq).Include(e => e.LvNewReq.Select(q => q.LeaveHead)).Include(e => e.LvOpenBal).Include(e => e.LvOpenBal.Select(q => q.LvHead))
                   .Where(e => e.Employee.Id == empidMain).SingleOrDefault();

                    var aa = a.LvNewReq.Where(e => e.LeaveHead.Id == db_data.LvHead.Id && e.LeaveCalendar.Id == lvcalendarid.Id).ToList();

                    if (aa != null)
                    {
                        return Json(new { status = false, responseText = "The requisition is applied. you cannot make changes now." }, JsonRequestBehavior.AllowGet);
                        //Msg.Add(" Reference for this record exists.Cannot edit it..  ");
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    db_data.LvCredit = L.LvCredit;
                    db_data.LvCreditDate = L.LvCreditDate;
                    db_data.LvOccurances = L.LvOccurances;
                    db_data.LvOpening = L.LvOpening;
                    db_data.LvUtilized = L.LvUtilized;
                    try
                    {
                        db.LvOpenBal.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        return Json(new { status = true, data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception e)
                    {

                        return Json(new { status = false, responseText = e.InnerException.ToString() }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { status = false, responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        /*---------------------------------------------------------- Create ---------------------------------------------- */

        public ActionResult Create(LvOpenBal L, FormCollection form, String forwarddata) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    //string LvCalendarlist = form["LvCalendarlist"] == "0" ? "" : form["LvCalendarlist"];
                    string LvHeadlist = form["LvHeadlist"] == "0" ? "" : form["LvHeadlist"];
                    string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];

                    var Cal = db.Calendar.Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = one_ids(Emp);
                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new Object[] { "", "", "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                    }

                    string getLvOPtype = string.Empty;
                    if (LvHeadlist != null && LvHeadlist != "")
                    {
                        int LvHeadID = Convert.ToInt32(LvHeadlist);
                        var value = db.LvHead.Include(e => e.LvHeadOprationType).Where(e => e.Id == LvHeadID).FirstOrDefault();
                        L.LvHead = value;
                        // u.LeaveHead = leavehead;
                        if (L.LvHead.LvHeadOprationType != null)
                        {
                            getLvOPtype = L.LvHead.LvHeadOprationType.LookupVal.ToUpper().ToString();
                        }

                    }
                    else
                    {
                        Msg.Add("  Kindly select Leave Head  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    foreach (var itemEmpid in ids)
                    {
                        var EmpP = db.Employee.Include(e => e.Gender).Where(e => e.Id == itemEmpid).FirstOrDefault();
                        if (!String.IsNullOrEmpty(getLvOPtype))
                        {
                            if (EmpP.Gender.LookupVal.ToUpper() == "MALE" && (getLvOPtype == "ML" || getLvOPtype == "MCL"))
                            {
                                Msg.Add("  For " + EmpP.EmpCode + " can not be create ML or MCL leave open bal. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            if (EmpP.Gender.LookupVal.ToUpper() == "FEMALE" && getLvOPtype == "PTL")
                            {
                                Msg.Add("  For " + EmpP.EmpCode + " can not be create PTL leave open bal. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            } 
                        }
                       
                    }

                    L.LvCalendar = Cal;
                    var Comp_Id = 0;
                    Double Leaveclosebal = 0;
                    Comp_Id = Convert.ToInt32(Session["CompId"]);
                    var Z = db.CompanyPayroll.Where(e => e.Company.Id == Comp_Id).Include(e => e.EmployeePayroll.Select(f => f.Employee)).SingleOrDefault();
                    Employee OEmployee = null;
                    EmployeeLeave OEmployeePayroll = null;

                    L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId, IsModified = false };

                    LvOpenBal Lvob = new LvOpenBal()
                    {
                        AboveServiceStepsCount = L.AboveServiceStepsCount,
                        LvBank = L.LvBank,
                        LvBankOccuance = L.LvBankOccuance,
                        LvCalendar = L.LvCalendar,
                        LVCount = L.LVCount,
                        LvCredit = L.LvCredit,
                        LvCreditDate = L.LvCreditDate,
                        LvEncash = L.LvEncash,
                        LvHead = L.LvHead,
                        LvLapseBal = L.LvLapseBal,
                        LvOccurances = L.LvOccurances,
                        LvOpening = L.LvOpening,
                        LvUtilized = L.LvUtilized,
                        MaxDays = L.MaxDays,
                        PrefixCount = L.PrefixCount,
                        SufixCount = L.SufixCount,

                        DBTrack = L.DBTrack


                    };


                    LvNewReq lvnew = new LvNewReq()
                    {
                        ReqDate = System.DateTime.Now,
                        OpenBal = L.LvOpening,
                        LVCount = L.LVCount,
                        LvOccurances = L.LvOccurances,
                        TrClosed = false,
                        Narration = "Leave Opening Balance",
                        WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "0").FirstOrDefault(),
                        DBTrack = L.DBTrack,
                        LeaveCalendar = L.LvCalendar,
                        LeaveHead = L.LvHead,
                        CloseBal = L.LvOpening,
                        LvCreditDate = L.LvCreditDate,

                    };
                    if (ModelState.IsValid)
                    {
                        if (ids != null)
                        {
                            foreach (var i in ids)
                            {
                                OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                            .Where(r => r.Id == i).SingleOrDefault();

                                OEmployeePayroll
                                = db.EmployeeLeave
                                .Include(e => e.LvNewReq)
                                .Include(e => e.LvNewReq.Select(t => t.LeaveHead))
                                .Include(e => e.LvNewReq.Select(t => t.LeaveCalendar))
                                .Where(e => e.Employee.Id == i).SingleOrDefault();

                                // next credit date start
                                int OEmpLvID = db.EmployeeLeave.Where(e => e.Employee.Id == OEmployee.Id).FirstOrDefault().Id;

                                EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct
                                      .Include(e => e.EmployeeLvStructDetails)
                                      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                                      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
                                      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy))
                                      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.CreditDate))
                                      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.ConvertLeaveHead))
                                      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.LvHead))
                                      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.ConvertLeaveHeadBal))
                                      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.ExcludeLeaveHeads))
                                      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead.LvHeadOprationType))
                                          .Where(e => e.EndDate == null && e.EmployeeLeave_Id == OEmpLvID).SingleOrDefault();

                                LvCreditPolicy oLvCreditPolicy = null;
                                if (OLvSalStruct != null)
                                {
                                    oLvCreditPolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == L.LvHead.Id && e.LvHeadFormula != null && e.LvHeadFormula.LvCreditPolicy != null).Select(r => r.LvHeadFormula.LvCreditPolicy).FirstOrDefault();
                                    if (oLvCreditPolicy != null)
                                    {

                                        if (oLvCreditPolicy.Accumalation == true)
                                        {
                                            var exitlvcheck = OEmployeePayroll.LvNewReq.Where(e => e.LeaveHead.Id == L.LvHead.Id).OrderByDescending(e => e.DBTrack.CreatedOn).FirstOrDefault();
                                            if (exitlvcheck != null)
                                            {

                                                double ClosingBal = OEmployeePayroll.LvNewReq.Where(e => e.LeaveHead.Id == L.LvHead.Id).OrderByDescending(e => e.DBTrack.CreatedOn).FirstOrDefault().CloseBal;
                                                L.LvOpening = L.LvOpening + ClosingBal;
                                                lvnew.CloseBal = L.LvOpening + L.LvCredit;
                                            }
                                        }
                                    }
                                }
                                if (oLvCreditPolicy != null)
                                {
                                    lvnew.LvCreditNextDate = L.LvCreditDate.Value.AddMonths(oLvCreditPolicy.ProCreditFrequency);
                                }
                                else
                                {
                                    Msg.Add("Please Assign Leave credit Policy in Leave Structure");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }


                                // next credit date end

                                //var CheckNewLvCodeInLvNewreq = OEmployeePayroll.LvNewReq.Where(e => e.LeaveHead.Id == L.LvHead.Id && e.LeaveCalendar.Id == L.LvCalendar.Id).Count();
                                //if (CheckNewLvCodeInLvNewreq > 0)
                                //{
                                //    Msg.Add("This Leave Head Already Exist in Current Year!!!");
                                //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //}

                                var CheckNxtRecordInLvNewreq = OEmployeePayroll.LvNewReq.Where(e => e.LeaveHead.Id == L.LvHead.Id).OrderByDescending(e => e.DBTrack.CreatedOn).FirstOrDefault();
                                if (CheckNxtRecordInLvNewreq != null && CheckNxtRecordInLvNewreq.DBTrack.CreatedOn >= L.LvCreditDate)
                                {
                                    Msg.Add("Leave Credit date should be greater than " + CheckNxtRecordInLvNewreq.DBTrack.CreatedOn.Value.Date.ToString("dd/MM/yyyy") + " !");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }

                                var emplo = Convert.ToInt32(Emp);
                                var datafor = db.EmployeeLeave.Include(e => e.LvOpenBal)
                                    .Where(a => a.Employee.Id == emplo &&
                                        a.LvOpenBal.Any(aa => (aa.LvHead.Id == L.LvHead.Id)
                                            && (aa.LvCalendar.Id == L.LvCalendar.Id))).Count();

                                if (datafor > 0)
                                {
                                    Msg.Add(" The OpenBal For this employee with " + L.LvHead.LvName + ", And " + L.LvCalendar.FullDetails + " Already Generated");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }

                                var datareqfor = db.EmployeeLeave.Include(e => e.LvNewReq)
                                    .Where(a => a.Employee.Id == emplo &&
                                        a.LvNewReq.Any(aa => (aa.LeaveHead.Id == L.LvHead.Id)
                                            && (aa.LeaveCalendar.Id == L.LvCalendar.Id))).Count();

                                if (datareqfor > 0)
                                {
                                    Msg.Add(" The leave Requisition For this employee with " + L.LvHead.LvName + ", And " + L.LvCalendar.FullDetails + " Already Generated");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }

                                using (TransactionScope ts = new TransactionScope())
                                {
                                    try
                                    {
                                        db.LvOpenBal.Add(Lvob);
                                        db.SaveChanges();

                                        lvnew.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);
                                        lvnew.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);
                                        lvnew.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);

                                        db.LvNewReq.Add(lvnew);
                                        db.SaveChanges();

                                        List<LvOpenBal> OFAT = new List<LvOpenBal>();
                                        OFAT.Add(db.LvOpenBal.Find(Lvob.Id));

                                        List<LvNewReq> OLVNEW = new List<LvNewReq>();

                                        OLVNEW.Add(lvnew);
                                        if (OEmployeePayroll == null)
                                        {
                                            EmployeeLeave OTEP = new EmployeeLeave()
                                            {
                                                Employee = db.Employee.Find(OEmployee.Id),
                                                LvOpenBal = OFAT,
                                                DBTrack = L.DBTrack

                                            };


                                            db.EmployeeLeave.Add(OTEP);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            var aa = db.EmployeeLeave.Find(OEmployeePayroll.Id);
                                            OLVNEW.AddRange(aa.LvNewReq);
                                            aa.LvOpenBal = OFAT;
                                            aa.LvNewReq = OLVNEW;
                                            //OEmployeePayroll.DBTrack = dbt;
                                            db.EmployeeLeave.Attach(aa);
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                        }
                                        ts.Complete();


                                    }
                                    catch (DataException ex)
                                    {
                                        //LogFile Logfile = new LogFile();
                                        //ErrorLog Err = new ErrorLog()
                                        //{
                                        //    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                        //    ExceptionMessage = ex.Message,
                                        //    ExceptionStackTrace = ex.StackTrace,
                                        //    LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                        //    LogTime = DateTime.Now
                                        //};
                                        //Logfile.CreateLogFile(Err);
                                        //return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                                    }


                                }
                            }
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                        }
                        Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new Object[] { "", "", "Unable to create...", JsonRequestBehavior.AllowGet });
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
                        // Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new { msg = errorMsg });
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


        //[HttpPost]
        //public ActionResult Create(LvOpenBal L, FormCollection form)
        //{
        //    string LvCalendarlist = form["LvCalendarlist"] == "0" ? "" : form["LvCalendarlist"];
        //    string LvHeadlist = form["LvHeadlist"] == "0" ? "" : form["LvHeadlist"];

        //    if (LvHeadlist != null && LvHeadlist!="")
        //    {

        //            var value = db.LvHead.Find(int.Parse(LvHeadlist));
        //            L.LvHead = value;
        //           // u.LeaveHead = leavehead;

        //    }

        //    if (LvCalendarlist != null && LvCalendarlist != "")
        //    {

        //        var value = db.Calendar.Find(int.Parse(LvCalendarlist));
        //        L.LvCalendar = value;
        //        // u.LeaveHead = leavehead;

        //    }

        //    if (ModelState.IsValid)
        //    {
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //           L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
        //            LvOpenBal un = new LvOpenBal()
        //            {
        //               AboveServiceStepsCount = L.AboveServiceStepsCount,
        //               LvBank = L.LvBank,
        //               LvBankOccuance =L.LvBankOccuance,
        //               LvCalendar =L.LvCalendar,
        //               LVCount =L.LVCount,
        //               LvCredit =L.LvCredit,
        //               LvCreditDate =L.LvCreditDate,
        //               LvEncash =L.LvEncash,
        //               LvHead =L.LvHead,
        //               LvLapseBal =L.LvLapseBal,
        //               LvOccurances =L.LvOccurances,
        //               LvOpening =L.LvOpening,
        //               LvUtilized =L.LvUtilized,
        //               MaxDays = L.MaxDays,
        //               PrefixCount =L.PrefixCount,
        //               SufixCount =L.SufixCount,

        //               DBTrack  = L.DBTrack


        //            };
        //            try
        //            {
        //                db.LvOpenBal.Add(un);
        //                db.SaveChanges();
        //                ts.Complete();
        //                return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
        //            }
        //            catch (DbUpdateConcurrencyException)
        //            {
        //                return RedirectToAction("Create", new { concurrencyError = true, id = L.Id });
        //            }
        //            catch (DataException /* dex */)
        //            {
        //                ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
        //                return View();
        //            }
        //        }
        //    }

        //    else
        //    {
        //        StringBuilder sb = new StringBuilder("");
        //        foreach (ModelState modelState in ModelState.Values)
        //        {
        //            foreach (ModelError error in modelState.Errors)
        //            {
        //                sb.Append(error.ErrorMessage);
        //                sb.Append("." + "\n");
        //            }
        //        }
        //        var errorMsg = sb.ToString();
        //        return this.Json(new { msg = errorMsg });
        //    }

        //}

        //  [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var Q = db.LvOpenBal
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        AboveServiceStepsCount = e.AboveServiceStepsCount,
                        LvBank = e.LvBank,
                        LvBankOccuance = e.LvBankOccuance,
                        LvCalendar_Id = e.LvCalendar.Id == null ? 0 : e.LvCalendar.Id,
                        LvCalendar_FullDetails = e.LvCalendar.FullDetails == null ? "" : e.LvCalendar.FullDetails,
                        LVCount = e.LVCount,
                        LvCredit = e.LvCredit,
                        LvClosing = e.LvClosing,
                        LvCreditDate = e.LvCreditDate,
                        LvEncash = e.LvEncash,
                        LvHead_Id = e.LvHead.Id == null ? 0 : e.LvHead.Id,
                        LvHead_FullDetails = e.LvHead.FullDetails == null ? "" : e.LvHead.FullDetails,
                        LvLapseBal = e.LvLapseBal,
                        LvOccurances = e.LvOccurances,
                        LvOpening = e.LvOpening,
                        LvUtilized = e.LvUtilized,
                        MaxDays = e.MaxDays,
                        PrefixCount = e.PrefixCount,
                        SufixCount = e.SufixCount,
                        Action = e.DBTrack.Action
                    }).ToList();

                var W = db.DT_LvOpenBal
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,



                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.LvOpenBal.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(LvOpenBal L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //string LvCalendarlist = form["LvCalendarlist"] == "0" ? "" : form["LvCalendarlist"];
                    string LvHeadlist = form["LvHeadlist"] == "0" ? "" : form["LvHeadlist"];

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    LvOpenBal OBJLvOpenBal = db.LvOpenBal.Include(e => e.LvHead).Where(e => e.Id == data).SingleOrDefault();
                    // var lvcalendarid = db.Calendar.Include(e => e.Name)
                    //   .Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                    // var a = db.EmployeeLeave .Include(e => e.LvNewReq)
                    //.Where(e =>e.Employee.id, e.LeaveCalendar.Id == lvcalendarid.Id && e.LeaveHead.Id == OBJLvOpenBal.LvHead.Id).Count();
                    // if (a > 0)
                    // {
                    //     Msg.Add(" Reference for this record exists.Cannot edit it..  ");
                    //     return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // }

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    LvOpenBal blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.LvOpenBal.Where(e => e.Id == data).Include(e => e.LvCalendar)
                                                                .Include(e => e.LvHead)
                                                                .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    L.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };



                                    if (LvHeadlist != null)
                                    {
                                        if (LvHeadlist != "")
                                        {
                                            var val = db.LvHead.Find(int.Parse(LvHeadlist));
                                            L.LvHead = val;

                                            var type = db.LvOpenBal.Include(e => e.LvHead).Where(e => e.Id == data).SingleOrDefault();
                                            IList<LvOpenBal> typedetails = null;
                                            if (type.LvCalendar != null)
                                            {
                                                typedetails = db.LvOpenBal.Where(x => x.LvHead.Id == type.LvHead.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.LvOpenBal.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.LvHead = L.LvHead;
                                                db.LvOpenBal.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var ClTypeDetails = db.LvOpenBal.Include(e => e.LvHead).Where(x => x.Id == data).ToList();
                                            foreach (var s in ClTypeDetails)
                                            {
                                                s.LvHead = null;
                                                db.LvOpenBal.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var ClTypeDetails = db.LvOpenBal.Include(e => e.LvHead).Where(x => x.Id == data).ToList();
                                        foreach (var s in ClTypeDetails)
                                        {
                                            s.LvHead = null;
                                            db.LvOpenBal.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    var CurCorp = db.LvOpenBal.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        LvOpenBal LvCreditPolicy = new LvOpenBal()
                                        {
                                            AboveServiceStepsCount = L.AboveServiceStepsCount,
                                            LvBank = L.LvBank,
                                            LvBankOccuance = L.LvBankOccuance,
                                            LvCalendar = L.LvCalendar,
                                            LVCount = L.LVCount,
                                            LvCredit = L.LvCredit,
                                            LvCreditDate = L.LvCreditDate,
                                            LvEncash = L.LvEncash,
                                            LvHead = L.LvHead,
                                            LvLapseBal = L.LvLapseBal,
                                            LvOccurances = L.LvOccurances,
                                            LvOpening = L.LvOpening,
                                            LvUtilized = L.LvUtilized,
                                            MaxDays = L.MaxDays,
                                            PrefixCount = L.PrefixCount,
                                            SufixCount = L.SufixCount,
                                            Id = data,
                                            DBTrack = L.DBTrack
                                        };
                                        db.LvOpenBal.Attach(LvCreditPolicy);
                                        db.Entry(LvCreditPolicy).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(LvCreditPolicy).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    }
                                    // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                    using (var context = new DataBaseContext())
                                    {
                                        //var obj = DBTrackFile.DBTrackSave("Leave/Leave", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                        //DT_LvOpenBal DT_Corp = (DT_LvOpenBal)obj;
                                        //DT_Corp. = blog.CreditDate == null ? 0 : blog.CreditDate.Id;
                                        //db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = L.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                }

                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (LvCreditPolicy)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (LvEncashReq)databaseEntry.ToObject();
                                    L.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            LvOpenBal blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            LvOpenBal Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.LvOpenBal.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            L.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            if (TempData["RowVersion"] == null)
                            {
                                TempData["RowVersion"] = blog.RowVersion;
                            }

                            LvOpenBal corp = new LvOpenBal()
                            {
                                AboveServiceStepsCount = L.AboveServiceStepsCount,
                                LvBank = L.LvBank,
                                LvBankOccuance = L.LvBankOccuance,
                                LvCalendar = L.LvCalendar,
                                LVCount = L.LVCount,
                                LvCredit = L.LvCredit,
                                LvCreditDate = L.LvCreditDate,
                                LvEncash = L.LvEncash,
                                LvHead = L.LvHead,
                                LvLapseBal = L.LvLapseBal,
                                LvOccurances = L.LvOccurances,
                                LvOpening = L.LvOpening,
                                LvUtilized = L.LvUtilized,
                                MaxDays = L.MaxDays,
                                PrefixCount = L.PrefixCount,
                                SufixCount = L.SufixCount,
                                Id = data,
                                DBTrack = L.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Leave/Leave", "M", blog, corp, "LvEncashReq", L.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                //Old_Corp = context.LvOpenBal.Where(e => e.Id == data).Include(e => e.LvCalendar)
                                //    .Include(e => e.ConvertLeaveHeadBal).Include(e => e.ExcludeLeaveHeads).Include(e => e.CreditDate).SingleOrDefault();
                                //DT_LvCreditPolicy DT_Corp = (DT_LvCreditPolicy)obj;
                                //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;

                                //db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = L.DBTrack;
                            db.LvOpenBal.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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
                return View();
            }

        }


        public class P2BGridData
        {
            public int Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public double LvOpening { get; set; }
            public double LvCredit { get; set; }
            public double LvUtilized { get; set; }
            public string LvHead { get; set; }
        }


        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    int comp_Id = 0;

        //    comp_Id = Convert.ToInt32(Session["CompId"]);
        //    var Z = db.CompanyPayroll.Where(e => e.Company.Id == comp_Id).Include(e => e.EmployeePayroll.Select(f => f.Employee)).SingleOrDefault();
        //    try
        //    {
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;

        //        IEnumerable<P2BGridData> LvOpenList = null;
        //        List<P2BGridData> model = new List<P2BGridData>();
        //        P2BGridData view = null;
        //        foreach (var ID in Z.EmployeePayroll.Select(e => e.Employee.Id))
        //        {
        //            var BindempleaveList = db.EmployeeLeave.Include(e=>e.Employee).Include(e=>e.Employee.EmpName).Include(e => e.LvOpenBal).Include(e=>e.LvOpenBal.Select(t=>t.LvHead)).Where(e => e.Employee.Id == ID).ToList();

        //            foreach (var z in BindempleaveList)
        //            {
        //                if (z.LvOpenBal != null)
        //                {

        //                    foreach (var E in z.LvOpenBal)
        //                    {
        //                        view = new P2BGridData()
        //                        {

        //                            Id = E.Id,
        //                            EmpCode = z.Employee.EmpCode,
        //                            EmpName = z.Employee.EmpName.FullNameFML,
        //                            LvOpening = E.LvOpening,
        //                            LvUtilized = E.LvUtilized,
        //                            LvCredit = E.LvCredit,
        //                            LvHead = E.LvHead.FullDetails,

        //                        };
        //                        model.Add(view);

        //                    }
        //                }

        //            }
        //        }
        //        LvOpenList = model;

        //        IEnumerable<P2BGridData> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = LvOpenList;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "Id")
        //                    jsonData = IE.Select(a => new { a.Id,a.EmpCode,a.EmpName,a.LvHead, a.LvOpening, a.LvCredit, a.LvUtilized }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                if (gp.searchField == "LvOpening")
        //                    jsonData = IE.Select(a => new { a.Id,a.EmpCode,a.EmpName,a.LvHead, a.LvOpening, a.LvCredit, a.LvUtilized }).Where((e => (e.LvOpening.ToString().Contains(gp.searchString)))).ToList();
        //                if (gp.searchField == "LvCredit")
        //                    jsonData = IE.Select(a => new { a.Id, a.EmpCode, a.EmpName, a.LvHead, a.LvOpening, a.LvCredit, a.LvUtilized }).Where((e => (e.LvCredit.ToString().Contains(gp.searchString)))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EmpCode != null ? Convert.ToString(a.EmpCode) : "", a.EmpName != null ? Convert.ToString(a.EmpName) : "",a.LvHead!= null ? Convert.ToString(a.LvHead):"", Convert.ToString(a.LvOpening), Convert.ToString(a.LvCredit), Convert.ToString(a.LvUtilized) }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = LvOpenList;
        //            Func<P2BGridData, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id.ToString() :
        //                                 gp.sidx == "lvopen" ? c.LvOpening.ToString() : ""

        //                                );
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.EmpCode != null ? Convert.ToString(a.EmpCode) : "", a.EmpName != null ? Convert.ToString(a.EmpName) : "", a.LvHead != null ? Convert.ToString(a.LvHead) : "", Convert.ToString(a.LvOpening), Convert.ToString(a.LvCredit), Convert.ToString(a.LvUtilized) }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.EmpCode != null ? Convert.ToString(a.EmpCode) : "", a.EmpName != null ? Convert.ToString(a.EmpName) : "", a.LvHead != null ? Convert.ToString(a.LvHead) : "", Convert.ToString(a.LvOpening), Convert.ToString(a.LvCredit), Convert.ToString(a.LvUtilized) }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EmpCode != null ? Convert.ToString(a.EmpCode) : "", a.EmpName != null ? Convert.ToString(a.EmpName) : "", a.LvHead != null ? Convert.ToString(a.LvHead) : "", Convert.ToString(a.LvOpening), Convert.ToString(a.LvCredit), Convert.ToString(a.LvUtilized) }).ToList();
        //            }
        //            totalRecords = LvOpenList.Count();
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


        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        IEnumerable<LvOpenBal> lencash = null;
        //        if (gp.IsAutho == true)
        //        {
        //            lencash = db.LvOpenBal.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            lencash = db.LvOpenBal.AsNoTracking().ToList();
        //        }

        //        IEnumerable<LvOpenBal> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = lencash;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.LvOpening, a.LvCredit, a.LvUtilized }).Where((e => (e.Id.ToString() == gp.searchString) || (e.LvOpening.ToString() == gp.searchString.ToLower()))).ToList();
        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.LvOpening, a.LvCredit, a.LvUtilized }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = lencash;
        //            Func<LvOpenBal, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "LvOpening" ? c.LvOpening.ToString() : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.LvOpening), Convert.ToString(a.LvCredit), Convert.ToString(a.LvUtilized) }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.LvOpening), Convert.ToString(a.LvCredit), Convert.ToString(a.LvUtilized) }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.LvOpening), Convert.ToString(a.LvCredit), Convert.ToString(a.LvUtilized) }).ToList();
        //            }
        //            totalRecords = lencash.Count();
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

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    LvOpenBal OBJLvOpenBal = db.LvOpenBal.Include(e => e.LvCalendar)
                                                       .Include(e => e.LvHead)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    var a = db.LvNewReq.Include(e => e.LeaveCalendar).Include(e => e.LeaveHead)
                   .Where(e => e.LeaveHead.Id == OBJLvOpenBal.LvHead.Id && e.Narration.ToUpper() != "LEAVE OPENING BALANCE" && e.LeaveCalendar_Id == OBJLvOpenBal.LvCalendar_Id).ToList();
                    if (a.Count > 0)
                    {
                        Msg.Add(" Reference for this record exists.Cannot remove it..  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (OBJLvOpenBal.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = OBJLvOpenBal.DBTrack.CreatedBy != null ? OBJLvOpenBal.DBTrack.CreatedBy : null,
                                CreatedOn = OBJLvOpenBal.DBTrack.CreatedOn != null ? OBJLvOpenBal.DBTrack.CreatedOn : null,
                                IsModified = OBJLvOpenBal.DBTrack.IsModified == true ? true : false
                            };
                            OBJLvOpenBal.DBTrack = dbT;
                            db.Entry(OBJLvOpenBal).State = System.Data.Entity.EntityState.Modified;

                            await db.SaveChangesAsync();

                            var LvNewReqDel = db.LvNewReq.Include(e => e.LeaveCalendar).Include(e => e.LeaveHead)
                  .Where(e => e.LeaveHead.Id == OBJLvOpenBal.LvHead.Id && e.Narration.ToUpper() == "LEAVE OPENING BALANCE" && e.LeaveCalendar_Id == OBJLvOpenBal.LvCalendar_Id).FirstOrDefault();

                            db.LvNewReq.Remove(LvNewReqDel);
                            db.SaveChanges();

                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = OBJLvOpenBal.DBTrack.CreatedBy != null ? OBJLvOpenBal.DBTrack.CreatedBy : null,
                                    CreatedOn = OBJLvOpenBal.DBTrack.CreatedOn != null ? OBJLvOpenBal.DBTrack.CreatedOn : null,
                                    IsModified = OBJLvOpenBal.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.EmpId,
                                    //AuthorizedOn = DateTime.Now
                                };
                                db.Entry(OBJLvOpenBal).State = System.Data.Entity.EntityState.Deleted;
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, OBJLvOpenBal.DBTrack);
                                //DT_LvCreditPolicy DT_Corp = (DT_LvCreditPolicy)rtn_Obj;
                                //DT_Corp.CreditDate_Id = OBJLvOpenBal.CreditDate == null ? 0 : OBJLvOpenBal.CreditDate.Id;
                                //db.Create(DT_Corp);
                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                            }
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
        }

        [HttpPost]
        public async Task<ActionResult> GridDelete(string data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var ids = Utility.StringIdsToListIds(data);

                    int empidintrnal = Convert.ToInt32(ids[0]);
                    int empidMain = Convert.ToInt32(ids[1]);

                    //var id = Convert.ToInt32(data);
                    var db_data = db.LvOpenBal.Include(e => e.LvHead)
                  .Where(e => e.Id == empidintrnal).SingleOrDefault();

                    var lvcalendarid = db.Calendar.Include(e => e.Name)
                      .Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

                    var a = db.EmployeeLeave.Include(e => e.LvNewReq).Include(e => e.LvNewReq.Select(q => q.LeaveHead)).Include(e => e.LvNewReq.Select(q => q.LeaveCalendar))
                        //.Include(e => e.LvOpenBal).Include(e => e.LvOpenBal.Select(q => q.LvHead))
                   .Where(e => e.Employee.Id == empidMain).SingleOrDefault();

                    var aa = a.LvNewReq.Where(e => e.LeaveHead.Id == db_data.LvHead.Id && e.LeaveCalendar.Id == lvcalendarid.Id && e.Narration.ToUpper() != "LEAVE OPENING BALANCE").ToList();

                    if (aa != null && aa.Count() > 0)
                    {
                        return Json(new { status = false, responseText = "Reference for this record exists.Cannot remove it.. " }, JsonRequestBehavior.AllowGet);
                    }
                    LvOpenBal OBJLvOpenBal = db.LvOpenBal.Include(e => e.LvCalendar)
                                                       .Include(e => e.LvHead)
                                                       .Where(e => e.Id == empidintrnal).SingleOrDefault();

                    //var a = db.LvNewReq.Include(e => e.LeaveCalendar).Include(e => e.LeaveHead)
                    //                        .Where(e => e.LeaveHead.Id == OBJLvOpenBal.LvHead.Id).ToList();
                    //if (a.Count > 0)
                    //{
                    //    Msg.Add(" Reference for this record exists.Cannot remove it..  ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (OBJLvOpenBal.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = OBJLvOpenBal.DBTrack.CreatedBy != null ? OBJLvOpenBal.DBTrack.CreatedBy : null,
                                CreatedOn = OBJLvOpenBal.DBTrack.CreatedOn != null ? OBJLvOpenBal.DBTrack.CreatedOn : null,
                                IsModified = OBJLvOpenBal.DBTrack.IsModified == true ? true : false
                            };
                            OBJLvOpenBal.DBTrack = dbT;
                            db.Entry(OBJLvOpenBal).State = System.Data.Entity.EntityState.Modified;

                            await db.SaveChangesAsync();


                            var LvNewReqDel = db.LvNewReq.Include(e => e.LeaveCalendar).Include(e => e.LeaveHead)
                  .Where(e => e.LeaveHead.Id == OBJLvOpenBal.LvHead.Id && e.Narration.ToUpper() == "LEAVE OPENING BALANCE" && e.LeaveCalendar_Id == OBJLvOpenBal.LvCalendar_Id).FirstOrDefault();

                            db.LvNewReq.Remove(LvNewReqDel);
                            db.SaveChanges();

                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = OBJLvOpenBal.DBTrack.CreatedBy != null ? OBJLvOpenBal.DBTrack.CreatedBy : null,
                                    CreatedOn = OBJLvOpenBal.DBTrack.CreatedOn != null ? OBJLvOpenBal.DBTrack.CreatedOn : null,
                                    IsModified = OBJLvOpenBal.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.EmpId,
                                    //AuthorizedOn = DateTime.Now
                                };

                                var LvNewReqDel = a.LvNewReq.Where(e => e.LeaveHead.Id == OBJLvOpenBal.LvHead.Id && e.Narration.ToUpper() == "LEAVE OPENING BALANCE" && e.LeaveCalendar_Id == OBJLvOpenBal.LvCalendar_Id).FirstOrDefault();

                                //db.LvNewReq.Remove(LvNewReqDel);
                                db.Entry(LvNewReqDel).State = System.Data.Entity.EntityState.Deleted;
                                db.Entry(OBJLvOpenBal).State = System.Data.Entity.EntityState.Deleted;
                                db.SaveChanges();
                                await db.SaveChangesAsync();
                                ts.Complete();
                                //  Msg.Add("  Data removed successfully.  ");
                                //    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                                return this.Json(new { status = true, responseText = "Data removed.", JsonRequestBehavior.AllowGet });


                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                            }
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
        }
        public class returndatagridclass
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string JoiningDate { get; set; }
            public string Job { get; set; }
            public string Grade { get; set; }
            public string Location { get; set; }
        }

        public class LvOpenBalChildDataClass
        {
            public int Id { get; set; }
            public string LvHead { get; set; }
            public string LvOpening { get; set; }
            public string LvCredit { get; set; }
            public string LvUtilized { get; set; }
        }
        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var all = db.EmployeeLeave.Where(e => e.Employee.ServiceBookDates != null && e.Employee.ServiceBookDates.ServiceLastDate == null)
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.GeoStruct)
                        .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                        .Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job).
                        Include(e => e.Employee.PayStruct)
                        .Include(e => e.Employee.PayStruct.Grade).AsNoTracking()
                        .ToList();
                    // for searchs
                    IEnumerable<EmployeeLeave> fall;

                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        //  fall = all.Where(e => e.Employee.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                   || (e.Employee.EmpCode.Contains(param.sSearch))
                                   || (e.Employee.EmpName.FullNameFML.ToUpper().Contains(param.sSearch.ToUpper()))
                                   ).ToList();
                    }
                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<EmployeeLeave, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.Employee.EmpCode :

                                                                "");
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
                                Code = item.Employee != null ? item.Employee.EmpCode : "",
                                Name = item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : "",
                                JoiningDate = item.Employee.ServiceBookDates != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : "",
                                Job = item.Employee.FuncStruct != null ? item.Employee.FuncStruct.Job != null ? item.Employee.FuncStruct.Job.Name : "" : "",
                                Grade = item.Employee.PayStruct != null ? item.Employee.PayStruct.Grade != null ? item.Employee.PayStruct.Grade.Name : "" : "",
                                Location = item.Employee.GeoStruct != null ? item.Employee.GeoStruct.Location != null ? item.Employee.GeoStruct.Location.LocationObj != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : "" : "" : "",
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
                catch (Exception ex)
                {
                    List<string> Msg = new List<string>();
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

        public ActionResult Get_LvOpenBal(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeeLeave
                        .Include(e => e.LvOpenBal.Select(r => r.LvHead))
                        .Include(e => e.LvNewReq)
                        .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data != null)
                    {
                        List<LvOpenBalChildDataClass> returndata = new List<LvOpenBalChildDataClass>();
                        //var lvutilized = db.EmployeeLeave.Include(e => e.LvNewReq).Where(e => e.Employee.Id == data)
                        //                 .Select(e => e.LvNewReq).SingleOrDefault();
                        //var lvutilizedays = lvutilized.Select(e => e.DebitDays).SingleOrDefault();
                        foreach (var item in db_data.LvOpenBal)
                        {
                            returndata.Add(new LvOpenBalChildDataClass
                            {
                                Id = item.Id,
                                LvHead = item.LvHead != null ? item.LvHead.LvCode : null,
                                LvOpening = item.LvOpening.ToString(),
                                LvCredit = item.LvCredit.ToString(),
                                LvUtilized = item.LvUtilized.ToString()
                            });
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
                    List<string> Msg = new List<string>();
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