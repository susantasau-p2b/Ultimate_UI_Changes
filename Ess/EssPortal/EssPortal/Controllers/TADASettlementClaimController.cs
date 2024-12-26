///
/// Created By Anandrao 
/// 

using EssPortal.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Text;
using System.Data.Entity;
using EssPortal.Models;
using Payroll;
using P2b.Global;
using System.Transactions;
using EssPortal.Security;

namespace EssPortal.Controllers
{
    public class TADASettlementClaimController : Controller
    {
        //
        // GET: /TADASettlementClaim/
        public ActionResult Index()
        {
            return View("~/Views/TADASettlementClaim/Index.cshtml");
        }

        public ActionResult PartialTadaSettelment()
        {
            return View("~/Views/Shared/_TadaSettlementPartial.cshtml");
        }

        // GET: /_JourneyDetails/
        public ActionResult Partial_JourneyDetails()
        {
            return View("~/Views/Shared/_JourneyDetails.cshtml");
        }

        // GET: /_TravelHotelBooking/
        public ActionResult Partial_TravelHotelBooking()
        {
            return View("~/Views/Shared/_TravelHotelBooking.cshtml");
        }

        public ActionResult TadaSettlementClaimPartialSanction()
        {
            return View("~/Views/Shared/_TADASettlementClaimReqonSanction.cshtml");
        }
        public ActionResult GetLookupPolicyname(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = (SelectList)null;
                var selected = "";
                if (data != "" && data != null)
                {

                    var qurey = db.Lookup.Where(e => e.Code == "3005").Select(e => e.LookupValues.Where(r => r.IsActive == true)).SingleOrDefault();
                    var query1 = qurey.Where(e => e.LookupVal.ToUpper() != "DIARY").ToList();
                    if (data2 != "" && data2 != "0")
                    {
                        selected = data2;
                    }
                    if (query1 != null)
                    {
                        s = new SelectList(query1, "Id", "LookupVal", selected);
                    }
                }
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public class GetTadasettlamts //childgrid
        {

            public double AdvanceAmt { get; set; }
            public double TAClaim { get; set; }
            public double TAElgmamt { get; set; }
            public double TASanctionAmt { get; set; }
            public double SettlementAmt { get; set; }
            public double DAClaim { get; set; }
            public double DAElgmamt { get; set; }
            public double DASanctionAmt { get; set; }
            public double HotelClaim { get; set; }
            public double HotelElgmamt { get; set; }
            public double HotelSanctionAmt { get; set; }
            public double MisexpClaim { get; set; }
            public double MisexpElgmamt { get; set; }
            public double MisexpSanctionAmt { get; set; }
            public double Noofdays { get; set; } //tour date
        }

        public class GetJourneyobjdata
        {
            public int Id { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string PlaceFrom { get; set; }
            public string PlaceTo { get; set; }
            public string ClaimAmt { get; set; }
            public string SettleAmt { get; set; }
        }

        public class GetMisExpenseObjdata
        {
            public int Id { get; set; }
            public string MisExpenseDate { get; set; }
            public string MisExpenseClaimAmt { get; set; }
            public string MisExpenseSettleAmt { get; set; }
        }

        public class GetHotelObjectdata
        {
            public int Id { get; set; }
            public string HotelDate { get; set; }
            public string HotelClaimAmt { get; set; }
            public string HotelSettleAmt { get; set; }
        }

        public class GetDAobjdata
        {
            public int Id { get; set; }
            public string DADate { get; set; }
            public string DAClaimAmt { get; set; }
            public string DASettleAmt { get; set; }
        }

        public ActionResult TADA_Settelment_Id(string SelectTadaIds)
        {
            TempData["Tada_settelment_ids"] = SelectTadaIds;
            return View();
        }

        public ActionResult GetJourneyObjList(string JourneyDetailsIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                int JourneyDetailsId = Convert.ToInt32(JourneyDetailsIds);
                var JourneyDetailsdata = db.JourneyDetails.Include(e => e.JourneyObject).Where(e => e.Id == JourneyDetailsId).FirstOrDefault();
                var journeyobjdata = JourneyDetailsdata.JourneyObject.ToList();
                List<GetJourneyobjdata> returndata = new List<GetJourneyobjdata>();
                if (journeyobjdata != null)
                {
                    foreach (var item in journeyobjdata)
                    {
                        returndata.Add(new GetJourneyobjdata
                        {
                            Id = item.Id,
                            FromDate = item.FromDate.ToString(),
                            ToDate = item.ToDate.ToString(),
                            PlaceFrom = item.PlaceFrom,
                            PlaceTo = item.PlaceTo,
                            ClaimAmt = item.TAClaimAmt.ToString(),
                            SettleAmt = item.TASettleAmt.ToString()

                        });
                    }

                }


                return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetMisExpenseObjList(string JourneyDetailsIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int JourneyDetailsId = Convert.ToInt32(JourneyDetailsIds);
                var JourneyDetailsdata = db.JourneyDetails.Include(e => e.MisExpenseObject).Where(e => e.Id == JourneyDetailsId).FirstOrDefault();
                var MisExpenseObjdata = JourneyDetailsdata.MisExpenseObject.ToList();
                List<GetMisExpenseObjdata> returndata = new List<GetMisExpenseObjdata>();
                if (MisExpenseObjdata != null)
                {
                    foreach (var item in MisExpenseObjdata)
                    {
                        returndata.Add(new GetMisExpenseObjdata
                        {
                            Id = item.Id,
                            MisExpenseDate = item.MisExpenseDate.ToString(),
                            MisExpenseClaimAmt = item.MisExpenseClaimAmt.ToString(),
                            MisExpenseSettleAmt = item.MisExpenseSettleAmt.ToString()
                        });
                    }

                }


                return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetHotelBookingList(string JourneyDetailsIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int JourneyDetailsId = Convert.ToInt32(JourneyDetailsIds);


                var JourneyObjdatas = db.JourneyDetails.Include(e => e.JourneyObject)
                     .Include(r => r.MisExpenseObject)
                     .Include(r => r.DAObject)
                     .Include(r => r.Travel_Hotel_Booking)
                     .Where(e => e.Id == JourneyDetailsId).SingleOrDefault();

                List<GetHotelObjectdata> returndata = new List<GetHotelObjectdata>();

                if (JourneyObjdatas.Travel_Hotel_Booking != null)
                {
                    var travelhotelobj = JourneyObjdatas.Travel_Hotel_Booking.ToList();

                    foreach (var item in travelhotelobj)
                    {
                        var hotelobject = db.TravelHotelBooking.Include(r => r.HotelObject).Where(e => e.Id == item.Id).FirstOrDefault();
                        if (hotelobject != null)
                        {
                            var HotelObjectdata = hotelobject.HotelObject.ToList();
                            if (HotelObjectdata != null)
                            {
                                foreach (var item1 in HotelObjectdata)
                                {
                                    returndata.Add(new GetHotelObjectdata
                                    {
                                        Id = item1.Id,
                                        HotelDate = item1.HotelDate.ToString(),
                                        HotelClaimAmt = item1.HotelClaimAmt.ToString(),
                                        HotelSettleAmt = item1.HotelSettleAmt.ToString()
                                    });
                                }

                            }
                        }

                    }
                }

                return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetDAobjList(string JourneyDetailsIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int JourneyDetailsId = Convert.ToInt32(JourneyDetailsIds);
                var JourneyDetailsdata = db.JourneyDetails.Include(e => e.DAObject).Where(e => e.Id == JourneyDetailsId).FirstOrDefault();
                var DAobjdatadata = JourneyDetailsdata.DAObject.ToList();
                List<GetDAobjdata> returndata = new List<GetDAobjdata>();
                if (DAobjdatadata != null)
                {
                    foreach (var item in DAobjdatadata)
                    {
                        returndata.Add(new GetDAobjdata
                        {
                            Id = item.Id,
                            DADate = item.DADate.ToString(),
                            DAClaimAmt = item.DAClaimAmt.ToString(),
                            DASettleAmt = item.DASettleAmt.ToString()
                        });
                    }

                }


                return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
            }
        }


        public class JourneyobjClass
        {
            public string SNo { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public string PlaceFrom { get; set; }
            public string PlaceTo { get; set; }
            public double ClaimAmt { get; set; }
            public double SettleAmt { get; set; }
            public int Id { get; set; }
        }

        public class MisExpenseObjClass
        {
            public string SNo { get; set; }
            public DateTime? MisExpenseDate { get; set; }
            public double MisExpenseClaimAmt { get; set; }
            public double MisExpenseSettleAmt { get; set; }
            public int Id { get; set; }
        }

        public class HotelObjectClass
        {
            public string SNo { get; set; }
            public DateTime? HotelDate { get; set; }
            public double HotelClaimAmt { get; set; }
            public double HotelSettleAmt { get; set; }
            public int Id { get; set; }
        }

        public class DAobjClass
        {
            public string SNo { get; set; }
            public DateTime? DADate { get; set; }
            public double DAClaimAmt { get; set; }
            public double DASettleAmt { get; set; }
            public int Id { get; set; }
        }

        [HttpPost]
        public ActionResult createJourneyObj(JourneyObject c, FormCollection form, string JoyobjId, string JoySettelmentAmt)
        {

            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {

                double JourneyobjSett = Convert.ToDouble(JoySettelmentAmt);
                int JourneyobjId = Convert.ToInt32(JoyobjId);

                try
                {

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.JourneyObject.Where(e => e.Id == JourneyobjId).SingleOrDefault();
                        if (JourneyobjSett > db_data.TAClaimAmt)
                        {
                            Msg.Add("Sanction amount should not Greater than Claim amount.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        TempData["RowVersion"] = db_data.RowVersion;
                        TempData["CurrRowVersion"] = db_data.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = db_data.DBTrack.CreatedBy == null ? null : db_data.DBTrack.CreatedBy,
                                CreatedOn = db_data.DBTrack.CreatedOn == null ? null : db_data.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            db_data.Id = JourneyobjId;
                            db_data.TASettleAmt = JourneyobjSett;
                            db_data.DBTrack = c.DBTrack;
                            db.JourneyObject.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();


                        }
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
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
        public ActionResult CreateMisExpenseObj(MisExpenseObject c, FormCollection form, string MisobjId, string MisExpenseSettleAmt)
        {

            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {

                double MisobjSett = Convert.ToDouble(MisExpenseSettleAmt);
                int MisobjIds = Convert.ToInt32(MisobjId);
                try
                {

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.MisExpenseObject.Where(e => e.Id == MisobjIds).SingleOrDefault();
                        if (MisobjSett > db_data.MisExpenseClaimAmt)
                        {
                            Msg.Add("Sanction amount should not Greater than Claim amount.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        TempData["RowVersion"] = db_data.RowVersion;
                        TempData["CurrRowVersion"] = db_data.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = db_data.DBTrack.CreatedBy == null ? null : db_data.DBTrack.CreatedBy,
                                CreatedOn = db_data.DBTrack.CreatedOn == null ? null : db_data.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            db_data.Id = MisobjIds;
                            db_data.MisExpenseSettleAmt = MisobjSett;
                            db_data.DBTrack = c.DBTrack;
                            db.MisExpenseObject.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                        }
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
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
        public ActionResult CreateHotelBooking(HotelObject c, FormCollection form, string HotelbjId, string HotelseSettleAmt)
        {

            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {

                double HotelobjSett = Convert.ToDouble(HotelseSettleAmt);
                int HotelbjIds = Convert.ToInt32(HotelbjId);
                try
                {



                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.HotelObject.Where(e => e.Id == HotelbjIds).SingleOrDefault();
                        if (HotelobjSett > db_data.HotelClaimAmt)
                        {
                            Msg.Add("Sanction amount should not Greater than Claim amount.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        TempData["RowVersion"] = db_data.RowVersion;
                        TempData["CurrRowVersion"] = db_data.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = db_data.DBTrack.CreatedBy == null ? null : db_data.DBTrack.CreatedBy,
                                CreatedOn = db_data.DBTrack.CreatedOn == null ? null : db_data.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            db_data.Id = HotelbjIds;
                            db_data.HotelSettleAmt = HotelobjSett;
                            db_data.DBTrack = c.DBTrack;
                            db.HotelObject.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
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
        public ActionResult CreateDAobj(DAObject c, FormCollection form, string DAbjId, string DAseSettleAmt)
        {

            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {

                double DAobjSett = Convert.ToDouble(DAseSettleAmt);
                int DAbjIds = Convert.ToInt32(DAbjId);


                try
                {


                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.DAObject.Where(e => e.Id == DAbjIds).SingleOrDefault();
                        if (DAobjSett > db_data.DAClaimAmt)
                        {
                            Msg.Add("Sanction amount should not Greater than Claim amount.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }


                        TempData["RowVersion"] = db_data.RowVersion;
                        TempData["CurrRowVersion"] = db_data.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = db_data.DBTrack.CreatedBy == null ? null : db_data.DBTrack.CreatedBy,
                                CreatedOn = db_data.DBTrack.CreatedOn == null ? null : db_data.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            db_data.Id = DAbjIds;
                            db_data.DASettleAmt = DAobjSett;
                            db_data.DBTrack = c.DBTrack;
                            db.DAObject.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                        }

                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
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
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult JourneyDetails_Id(string SelectId)
        {
            TempData["Journeydetails_ids"] = SelectId;
            return View();
        }

        public ActionResult GetLookupJourneyDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.JourneyDetails.ToList();
                IEnumerable<TravelHotelBooking> all;
                if (SkipIds != null)
                {
                    foreach (int a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.JourneyDetails.ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();

                    }
                }

                var list1 = db.LTCSettlementClaim.Where(e => e.JourneyDetails != null).ToList().Select(e => e.JourneyDetails);
                var list2 = db.TADASettlementClaim.Where(e => e.JourneyDetails != null).ToList().Select(e => e.JourneyDetails);
                var list3 = fall.Except(list1).Except(list2);

                var r = (from ca in list3 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }



        public ActionResult GetLookupTadaAdv(string data, string Empid)
        {
            int empids = Convert.ToInt32(Empid);
            using (DataBaseContext db = new DataBaseContext())
            {
                var employeedata = db.EmployeePayroll.Include(e => e.Employee)
                    .Include(e => e.TADAAdvanceClaim)
                    .Include(e => e.TADAAdvanceClaim.Select(c => c.TADAType))
                    .Where(e => e.Employee.Id == empids).ToList();

                var fall = employeedata.SelectMany(e => e.TADAAdvanceClaim).ToList().Where(e => e.TrClosed == true && e.TrReject == false);
                IEnumerable<TADAAdvanceClaim> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.TADAAdvanceClaim.ToList().Where(d => d.FullDetails.Contains(data));
                }
                else
                {
                    //var list1 = db.FamilyDetails.ToList();

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
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



        public class TADASettlClaimChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string ReqDate { get; set; }
            public string TADAType { get; set; }
            public double EligibleAmt { get; set; }
            public double AdvanceAmt { get; set; }
            public string Remark { get; set; }

        }

        [HttpPost]
        public ActionResult Create(TADASettlementClaim c, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    int comp_Id = 0;
                    comp_Id = Convert.ToInt32(Session["CompId"]);
                    var Company = new Company();
                    Company = db.Company.Where(e => e.Id == comp_Id).SingleOrDefault();
                    int Emp = Convert.ToInt32(SessionManager.EmpId);
                    string tadaadvanceclaimlist = form["TADAAdvanceClaimList"] == "0" ? "" : form["TADAAdvanceClaimList"];
                    string tadatypelist = form["TADATypelist"] == "0" ? "" : form["TADATypelist"];
                    // string lvreqlist = form["LvReqList"] == "0" ? "" : form["LvReqList"];
                    string journewdetailslist = form["JourneyDetailsList"] == "0" ? "" : form["JourneyDetailsList"];
                    string trhotelbookinglist = form["Travel_Hotel_BookingList"] == "0" ? "" : form["Travel_Hotel_BookingList"];
                    //  string LtcBlock = form["BlockId"] == "0" ? "" : form["BlockId"];


                    //string ddlIncharge = form["ddlIncharge"] == "" ? null : form["ddlIncharge"];
                    string ddlIncharge = form["Incharge_id"] == "" ? null : form["Incharge_id"];

                    if (tadaadvanceclaimlist != null && tadaadvanceclaimlist != "")
                    {
                        int tadaadvanceid = Convert.ToInt32(tadaadvanceclaimlist);
                        var value = db.TADAAdvanceClaim.Where(e => e.Id == tadaadvanceid).SingleOrDefault();
                        c.TADAAdvanceClaim = value;

                    }
                    //if (trhotelbookinglist != null)
                    //{
                    //    var ids = Utility.StringIdsToListIds(trhotelbookinglist);
                    //    var travelhotelbookinglist = new List<TravelHotelBooking>();
                    //    foreach (var item in ids)
                    //    {

                    //        int hotellistid = Convert.ToInt32(item);
                    //        var val = db.TravelHotelBooking.Include(e => e.HotelEligibilityPolicy).Where(e => e.Id == hotellistid).SingleOrDefault();
                    //        if (val != null)
                    //        {
                    //            travelhotelbookinglist.Add(val);
                    //        }
                    //    }
                    //    c.Travel_Hotel_Booking = travelhotelbookinglist;
                    //}

                    var EmpLTCStruct_Details = db.EmployeePayroll
                           .Include(e => e.TADASettlementClaim)
                            .Include(e => e.TADASettlementClaim.Select(x => x.JourneyDetails))
                          .Where(e => e.Employee.Id == Emp).FirstOrDefault();

                    var travelmodlist = EmpLTCStruct_Details.TADASettlementClaim.Select(r => r.JourneyDetails)
                        .ToList();

                    if (journewdetailslist != null)
                    {
                        if (journewdetailslist != "")
                        {
                            int ids = Convert.ToInt32(journewdetailslist);
                            var val = db.JourneyDetails.Where(e => e.Id == ids).SingleOrDefault();
                            c.JourneyDetails = val;
                        }
                    }

                    if (travelmodlist.Count() > 0)
                    {
                        if (journewdetailslist != null)
                        {
                            if (journewdetailslist != "")
                            {
                                int ids = Convert.ToInt32(journewdetailslist);
                                if (travelmodlist.Where(e => e.Id == ids).Count() != 0)
                                {
                                    Msg.Add("Can not insert duplicate entry");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                    }

                    if (tadatypelist != null && tadatypelist != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(tadatypelist));
                        c.TADAType = val;
                    }
                    var employeedata = db.Employee.Include(e => e.GeoStruct).Include(e => e.PayStruct).Include(e => e.FuncStruct).Where(e => e.Id == Emp).SingleOrDefault();
                    c.PayStruct = employeedata.PayStruct;
                    c.GeoStruct = employeedata.GeoStruct;
                    c.FuncStruct = employeedata.FuncStruct;
                    // int Blockid = int.Parse(LtcBlock);
                    // EmpLTCBlockT OEmpLTCBlockT = db.EmpLTCBlockT.Find(Blockid);

                    var a = db.EmployeePayroll.Include(e => e.EmpLTCBlock).Include(e => e.Employee)
                             .Include(e => e.EmpLTCBlock.Select(y => y.EmpLTCBlockT))
                             .Where(e => e.Employee.Id == Emp).AsNoTracking().SingleOrDefault().EmpLTCBlock.Where(e => e.IsBlockClose == false).FirstOrDefault();

                    ////var test = a.EmpLTCBlockT.Where(e => e.BlockPeriodStart >= OEmpLTCBlockT.BlockPeriodStart
                    ////   && e.BlockPeriodEnd <= OEmpLTCBlockT.BlockPeriodEnd).FirstOrDefault();
                    //if (c.SettlementAmt > c.DA_Eligible_Amt)
                    //{
                    //    Msg.Add("Settlement amount Should not be greater than Eligible amount.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    //if (c.DA_SanctionAmt > c.DA_Eligible_Amt)
                    //{
                    //    Msg.Add("Sanction amount Should not be greater than Eligible amount.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    //if (c.SettlementAmt > c.DA_SanctionAmt)
                    //{
                    //    Msg.Add("Settlement amount Should not be greater than sanction amount.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    // yearlypayment

                    var emppi = db.EmployeePayroll.Where(e => e.Employee.Id == Emp).SingleOrDefault();
                    //int salhdid = 0;
                    var fyyr = db.Calendar.Include(x => x.Name).Where(x => x.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && x.Default == true).SingleOrDefault();

                    //var QEmpSalStruct1 = db.EmpSalStruct//.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.EffectiveDate >= comparedate)
                    //                      .Include(e => e.EmpSalStructDetails)
                    //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                    //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalHeadFormula))
                    //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency))
                    //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Type))
                    //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod))
                    //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType))
                    //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.LvHead));//.ToList();
                    //var EmpSalStructTotal = QEmpSalStruct1.Where(e => e.EmployeePayroll.Id == emppi.Id && e.EndDate == null)
                    //                              .ToList();
                    //var OEmpSalStruct = EmpSalStructTotal.Where(e => e.EndDate == null).SingleOrDefault();
                    //if (OEmpSalStruct != null)
                    //{

                    //    salhdid = OEmpSalStruct.EmpSalStructDetails
                    //       .Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY"
                    //    && s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                    //    && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "TADA"
                    //    )).FirstOrDefault() != null ? OEmpSalStruct.EmpSalStructDetails
                    //       .Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY"
                    //    && s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                    //    && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "TADA"
                    //    )).FirstOrDefault().SalaryHead.Id : 0;
                    //}


                    //if (salhdid == 0)
                    //{
                    //    Msg.Add("Please Define TADA in salary Structure.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    FunctAllWFDetails oAttWFDetails = new FunctAllWFDetails
                    {
                        WFStatus = 0,
                        //Comments = c.Remark.ToString(),
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    };

                    List<FunctAllWFDetails> oAttWFDetails_List = new List<FunctAllWFDetails>();
                    oAttWFDetails_List.Add(oAttWFDetails);

                    if (ModelState.IsValid)
                    {


                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            TADASettlementClaim tadasettlement = new TADASettlementClaim()
                            {
                                DateOfAppl = c.DateOfAppl,
                                //  EncashmentAmount = c.EncashmentAmount,
                                JourneyDetails = c.JourneyDetails,
                                //   Travel_Hotel_Booking = c.Travel_Hotel_Booking,
                                TADAAdvanceClaim = c.TADAAdvanceClaim,
                                //NoOfDays = c.NoOfDays,
                                TADAType = c.TADAType,
                                //AdvAmt = c.AdvAmt,
                                //TA_Eligible_Amt = c.TA_Eligible_Amt,
                                //TA_Claim_Amt = c.TA_Claim_Amt,
                                //TA_SanctionAmt = c.TA_SanctionAmt,
                                //DA_Eligible_Amt = c.DA_Eligible_Amt,
                                //DA_Claim_Amt = c.DA_Claim_Amt,
                                //DA_SanctionAmt = c.DA_SanctionAmt,
                                //Hotel_Eligible_Amt = c.Hotel_Eligible_Amt,
                                //Hotel_Claim_Amt = c.Hotel_Claim_Amt,
                                //Hotel_SanctionAmt = c.Hotel_SanctionAmt,
                                //MisExpense_Eligible_Amt = c.MisExpense_Eligible_Amt,
                                //MisExpense_Claim_Amt = c.MisExpense_Claim_Amt,
                                //MisExpense_SanctionAmt = c.MisExpense_SanctionAmt,
                                //SettlementAmt = c.SettlementAmt,
                                //Remark = c.Remark,
                                FuncStruct = c.FuncStruct,
                                GeoStruct = c.GeoStruct,
                                PayStruct = c.PayStruct,
                                WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                                LTCWFDetails = oAttWFDetails_List,
                                DBTrack = c.DBTrack
                            };

                            db.TADASettlementClaim.Add(tadasettlement);
                            db.SaveChanges();

                            // yearly payment strat
                            //YearlyPaymentT ObjYPT = new YearlyPaymentT();
                            //{
                            //    ObjYPT.SalaryHead = db.SalaryHead.Find(salhdid);
                            //    ObjYPT.AmountPaid = c.SettlementAmt;
                            //    ObjYPT.FromPeriod = fyyr.FromDate;
                            //    ObjYPT.ToPeriod = fyyr.ToDate;
                            //    ObjYPT.ProcessMonth = Convert.ToDateTime(c.DateOfAppl).ToString("MM/yyyy");
                            //    ObjYPT.PayMonth = Convert.ToDateTime(c.DateOfAppl).ToString("MM/yyyy");
                            //    ObjYPT.ReleaseDate = c.DateOfAppl;
                            //    ObjYPT.ReleaseFlag = true;
                            //    ObjYPT.TDSAmount = 0;
                            //    ObjYPT.OtherDeduction = 0;
                            //    ObjYPT.FinancialYear = fyyr;
                            //    ObjYPT.DBTrack = c.DBTrack;
                            //    ObjYPT.FuncStruct = c.FuncStruct;
                            //    ObjYPT.GeoStruct = c.GeoStruct;
                            //    ObjYPT.PayStruct = c.PayStruct;

                            //}
                            //List<YearlyPaymentT> OYrlyPaylist = new List<YearlyPaymentT>();
                            //db.YearlyPaymentT.Add(ObjYPT);
                            //db.SaveChanges();
                            //OYrlyPaylist.Add(ObjYPT);

                            //yearly payment end
                            Employee OEmployee = null;
                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                           .Where(r => r.Id == Emp).AsNoTracking().SingleOrDefault();

                            List<TADASettlementClaim> TadaSettlementlist = new List<TADASettlementClaim>();
                            var aa = db.EmployeePayroll.Include(e => e.TADASettlementClaim).Include(e => e.YearlyPaymentT).Include(e => e.Employee).Where(e => e.Employee.Id == Emp).SingleOrDefault();
                            //if (aa.TADASettlementClaim.Count() > 0)
                            //{
                            //    TadaSettlementlist.AddRange(aa.TADASettlementClaim);
                            //}
                            //else
                            //{
                            //    TadaSettlementlist.Add(tadasettlement);
                            //}


                            //if (TadaSettlementlist.Count() == 0)
                            //{

                            //    TadaSettlementlist.Add(tadasettlement);
                            //    aa.TADASettlementClaim = TadaSettlementlist;
                            //    db.EmployeePayroll.Attach(aa);
                            //    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                            //    db.SaveChanges();
                            //    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                            //}


                            TadaSettlementlist.Add(db.TADASettlementClaim.Find(tadasettlement.Id));
                            if (aa == null)
                            {
                                EmployeePayroll OTEP = new EmployeePayroll()
                                {
                                    Employee = db.Employee.Find(OEmployee.Id),
                                    TADASettlementClaim = TadaSettlementlist,
                                    // DBTrack = S.DBTrack

                                };
                                db.EmployeePayroll.Add(OTEP);
                                db.SaveChanges();
                            }
                            else
                            {
                                var aa1 = db.EmployeePayroll.Find(aa.Id);
                                TadaSettlementlist.AddRange(aa1.TADASettlementClaim);
                                aa1.TADASettlementClaim = TadaSettlementlist;
                                //OEmployeePayroll.DBTrack = dbt;

                                db.EmployeePayroll.Attach(aa1);
                                db.Entry(aa1).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(aa1).State = System.Data.Entity.EntityState.Detached;

                            }



                            //if (aa.YearlyPaymentT.Count() > 0)
                            //{
                            //    OYrlyPaylist.AddRange(aa.YearlyPaymentT);
                            //}

                            // aa.TADASettlementClaim = TadaSettlementlist;
                            //aa.YearlyPaymentT = OYrlyPaylist;

                            //db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                            //db.SaveChanges();
                            //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            //Msg.Add("  Data Saved successfully  ");
                            //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            return Json(new Utility.JsonClass { status = true, responseText = " Data Saved successfully " }, JsonRequestBehavior.AllowGet);
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


        //[HttpPost]
        //public ActionResult Create(TADASettlementClaim c, FormCollection form)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<string> Msg = new List<string>();
        //        try
        //        {
        //            int comp_Id = 0;
        //            comp_Id = Convert.ToInt32(Session["CompId"]);
        //            var Company = new Company();
        //            Company = db.Company.Where(e => e.Id == comp_Id).SingleOrDefault();
        //            int Emp = Convert.ToInt32(SessionManager.EmpId);
        //            //int Emp = form["employee-table"] == "0" ? 0 : Convert.ToInt32(form["employee-table"]);
        //            //int Emp = form["EmpLvnereq_Id"] == "0" ? 0 : Convert.ToInt32(form["EmpLvnereq_Id"]);
        //            string tadaadvanceclaimlist = form["TADAAdvanceClaimList"] == "0" ? "" : form["TADAAdvanceClaimList"];
        //            string tadatypelist = form["TADATypelist"] == "0" ? "" : form["TADATypelist"];
        //            // string lvreqlist = form["LvReqList"] == "0" ? "" : form["LvReqList"];
        //            string journewdetailslist = form["JourneyDetailsList"] == "0" ? "" : form["JourneyDetailsList"];
        //            string trhotelbookinglist = form["Travel_Hotel_BookingList"] == "0" ? "" : form["Travel_Hotel_BookingList"];
        //            //  string LtcBlock = form["BlockId"] == "0" ? "" : form["BlockId"];


        //            //string ddlIncharge = form["ddlIncharge"] == "" ? null : form["ddlIncharge"];
        //            string ddlIncharge = form["Incharge_id"] == "" ? null : form["Incharge_id"];




        //            if (tadaadvanceclaimlist != null && tadaadvanceclaimlist != "")
        //            {
        //                int tadaadvanceid = Convert.ToInt32(tadaadvanceclaimlist);
        //                var value = db.TADAAdvanceClaim.Where(e => e.Id == tadaadvanceid).SingleOrDefault();
        //                c.TADAAdvanceClaim = value;

        //            }
        //            //if (trhotelbookinglist != null)
        //            //{
        //            //    var ids = Utility.StringIdsToListIds(trhotelbookinglist);
        //            //    var travelhotelbookinglist = new List<TravelHotelBooking>();
        //            //    foreach (var item in ids)
        //            //    {

        //            //        int hotellistid = Convert.ToInt32(item);
        //            //        var val = db.TravelHotelBooking.Include(e => e.HotelEligibilityPolicy).Where(e => e.Id == hotellistid).SingleOrDefault();
        //            //        if (val != null)
        //            //        {
        //            //            travelhotelbookinglist.Add(val);
        //            //        }
        //            //    }
        //            //    c.Travel_Hotel_Booking = travelhotelbookinglist;
        //            //}



        //            if (journewdetailslist != null)
        //            {
        //                if (journewdetailslist != "")
        //                {
        //                    int ids = Convert.ToInt32(journewdetailslist);
        //                    var val = db.JourneyDetails.Where(e => e.Id == ids).SingleOrDefault();
        //                    c.JourneyDetails = val;
        //                }
        //            }

        //            var EmpLTCStruct_Details = db.EmployeePayroll
        //                    .Include(e => e.TADASettlementClaim)
        //                     .Include(e => e.TADASettlementClaim.Select(x => x.JourneyDetails))
        //                   .Where(e => e.Employee.Id == Emp).FirstOrDefault();

        //            var travelmodlist = EmpLTCStruct_Details.TADASettlementClaim.Select(r => r.JourneyDetails)
        //                .ToList();


        //            if (travelmodlist.Count() > 0)
        //            {
        //                if (journewdetailslist != null)
        //                {
        //                    if (journewdetailslist != "")
        //                    {
        //                        int ids = Convert.ToInt32(journewdetailslist);
        //                        if (travelmodlist.Where(e => e.Id == ids).Count() != 0)
        //                        {
        //                            Msg.Add("Can not insert duplicate entry");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        }
        //                    }
        //                }
        //            }

        //            //if (lvreqlist != null)
        //            //{
        //            //    if (lvreqlist != "")
        //            //    {
        //            //        int ids = Convert.ToInt32(lvreqlist);
        //            //        var val = db.LvNewReq.Where(e => e.Id == ids).SingleOrDefault();
        //            //        c.LvNewReq = val;
        //            //    }
        //            //}

        //            if (tadatypelist != null && tadatypelist != "")
        //            {
        //                var val = db.LookupValue.Find(int.Parse(tadatypelist));
        //                c.TADAType = val;
        //            }
        //            var employeedata = db.Employee.Include(e => e.GeoStruct).Include(e => e.PayStruct).Include(e => e.FuncStruct).Where(e => e.Id == Emp).SingleOrDefault();
        //            c.PayStruct = employeedata.PayStruct;
        //            c.GeoStruct = employeedata.GeoStruct;
        //            c.FuncStruct = employeedata.FuncStruct;
        //            // int Blockid = int.Parse(LtcBlock);
        //            // EmpLTCBlockT OEmpLTCBlockT = db.EmpLTCBlockT.Find(Blockid);

        //            var a = db.EmployeePayroll.Include(e => e.EmpLTCBlock).Include(e => e.Employee)
        //                     .Include(e => e.EmpLTCBlock.Select(y => y.EmpLTCBlockT))
        //                     .Where(e => e.Employee.Id == Emp).AsNoTracking().SingleOrDefault().EmpLTCBlock.Where(e => e.IsBlockClose == false).FirstOrDefault();

        //            ////var test = a.EmpLTCBlockT.Where(e => e.BlockPeriodStart >= OEmpLTCBlockT.BlockPeriodStart
        //            ////   && e.BlockPeriodEnd <= OEmpLTCBlockT.BlockPeriodEnd).FirstOrDefault();

        //            //if (c.SettlementAmt > c.TA_Eligible_Amt)
        //            //{
        //            //    Msg.Add("Settlement amount Should not be greater than Eligible amount.");
        //            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            //}
        //            //if (c.TA_SanctionAmt > c.TA_Eligible_Amt)
        //            //{
        //            //    Msg.Add("Sanction amount Should not be greater than Eligible amount.");
        //            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            //}

        //            //if (c.SettlementAmt > c.TA_SanctionAmt)
        //            //{
        //            //    Msg.Add("Settlement amount Should not be greater than sanction amount.");
        //            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            //}

        //            // yearlypayment

        //            string TADATypelist = form["TADATypelist"] == "0" ? "" : form["TADATypelist"];
        //            string salcode = "";
        //            //if (TADATypelist != null && TADATypelist != "")
        //            //{
        //            //    var val = db.LookupValue.Find(int.Parse(TADATypelist));
        //            //    if (val.LookupValData != null && val.LookupValData != "")
        //            //    {
        //            //        salcode = val.LookupValData.ToString();
        //            //    }
        //            //}

        //            //if (salcode == "")
        //            //{
        //            //    Msg.Add("Please bind salary head in Tada type.");
        //            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            //}
        //            var emppi = db.EmployeePayroll.Where(e => e.Employee.Id == Emp).SingleOrDefault();
        //            int salhdid = 0;
        //            var fyyr = db.Calendar.Include(x => x.Name).Where(x => x.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && x.Default == true).SingleOrDefault();

        //            //var QEmpSalStruct1 = db.EmpSalStruct//.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.EffectiveDate >= comparedate)
        //            //                      .Include(e => e.EmpSalStructDetails)
        //            //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
        //            //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalHeadFormula))
        //            //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency))
        //            //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Type))
        //            //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod))
        //            //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType))
        //            //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.LvHead));//.ToList();
        //            //var EmpSalStructTotal = QEmpSalStruct1.Where(e => e.EmployeePayroll.Id == emppi.Id && e.EndDate == null)
        //            //                              .ToList();
        //            //var OEmpSalStruct = EmpSalStructTotal.Where(e => e.EndDate == null).SingleOrDefault();
        //            //if (OEmpSalStruct != null)
        //            //{

        //            //    salhdid = OEmpSalStruct.EmpSalStructDetails
        //            //       .Where(s => s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && s.SalaryHead.Code.ToUpper() == salcode.ToUpper()
        //            //    && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BMSTADA"
        //            //    )).FirstOrDefault() != null ? OEmpSalStruct.EmpSalStructDetails
        //            //       .Where(s => s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && s.SalaryHead.Code.ToUpper() == salcode.ToUpper()
        //            //    && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BMSTADA"
        //            //    )).FirstOrDefault().SalaryHead.Id : 0;
        //            //}


        //            //if (salhdid == 0)
        //            //{
        //            //    Msg.Add("Please Define BMSTADA in salary Structure.");
        //            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            //}
        //            FunctAllWFDetails oAttWFDetails = new FunctAllWFDetails
        //            {
        //                WFStatus = 0,
        //                Comments = c.Remark.ToString(),
        //                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
        //            };

        //            List<FunctAllWFDetails> oAttWFDetails_List = new List<FunctAllWFDetails>();
        //            oAttWFDetails_List.Add(oAttWFDetails);

        //            if (ModelState.IsValid)
        //            {


        //                using (TransactionScope ts = new TransactionScope())
        //                {
        //                    c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
        //                    TADASettlementClaim tadasettlement = new TADASettlementClaim()
        //                    {
        //                        DateOfAppl = c.DateOfAppl,
        //                        //  EncashmentAmount = c.EncashmentAmount,
        //                        JourneyDetails = c.JourneyDetails,
        //                        //   Travel_Hotel_Booking = c.Travel_Hotel_Booking,
        //                        TADAAdvanceClaim = c.TADAAdvanceClaim,
        //                        NoOfDays = c.NoOfDays,
        //                        TADAType = c.TADAType,
        //                        AdvAmt = c.AdvAmt,
        //                        TA_Eligible_Amt = c.TA_Eligible_Amt,
        //                        TA_Claim_Amt = c.TA_Claim_Amt,
        //                        TA_SanctionAmt = c.TA_SanctionAmt,
        //                        DA_Eligible_Amt = c.DA_Eligible_Amt,
        //                        DA_Claim_Amt = c.DA_Claim_Amt,
        //                        DA_SanctionAmt = c.DA_SanctionAmt,
        //                        Hotel_Eligible_Amt = c.Hotel_Eligible_Amt,
        //                        Hotel_Claim_Amt = c.Hotel_Claim_Amt,
        //                        Hotel_SanctionAmt = c.Hotel_SanctionAmt,
        //                        MisExpense_Eligible_Amt = c.MisExpense_Eligible_Amt,
        //                        MisExpense_Claim_Amt = c.MisExpense_Claim_Amt,
        //                        MisExpense_SanctionAmt = c.MisExpense_SanctionAmt,
        //                        SettlementAmt = c.SettlementAmt,
        //                        Remark = c.Remark,
        //                        FuncStruct = c.FuncStruct,
        //                        GeoStruct = c.GeoStruct,
        //                        PayStruct = c.PayStruct,
        //                        WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
        //                        LTCWFDetails = oAttWFDetails_List,
        //                        DBTrack = c.DBTrack
        //                    };

        //                    db.TADASettlementClaim.Add(tadasettlement);
        //                    db.SaveChanges();

        //                    // yearly payment strat
        //                    //YearlyPaymentT ObjYPT = new YearlyPaymentT();
        //                    //{
        //                    //    ObjYPT.SalaryHead = db.SalaryHead.Find(salhdid);
        //                    //    ObjYPT.AmountPaid = c.SettlementAmt;
        //                    //    ObjYPT.FromPeriod = fyyr.FromDate;
        //                    //    ObjYPT.ToPeriod = fyyr.ToDate;
        //                    //    ObjYPT.ProcessMonth = Convert.ToDateTime(c.DateOfAppl).ToString("MM/yyyy");
        //                    //    ObjYPT.PayMonth = Convert.ToDateTime(c.DateOfAppl).ToString("MM/yyyy");
        //                    //    ObjYPT.ReleaseDate = c.DateOfAppl;
        //                    //    ObjYPT.ReleaseFlag = true;
        //                    //    ObjYPT.TDSAmount = 0;
        //                    //    ObjYPT.OtherDeduction = 0;
        //                    //    ObjYPT.FinancialYear = fyyr;
        //                    //    ObjYPT.DBTrack = c.DBTrack;
        //                    //    ObjYPT.FuncStruct = c.FuncStruct;
        //                    //    ObjYPT.GeoStruct = c.GeoStruct;
        //                    //    ObjYPT.PayStruct = c.PayStruct;

        //                    //}
        //                    //List<YearlyPaymentT> OYrlyPaylist = new List<YearlyPaymentT>();
        //                    //db.YearlyPaymentT.Add(ObjYPT);
        //                    //db.SaveChanges();
        //                    //OYrlyPaylist.Add(ObjYPT);

        //                    //yearly payment end

        //                    List<TADASettlementClaim> TadaSettlementlist = new List<TADASettlementClaim>();
        //                    var aa = db.EmployeePayroll.Include(e => e.TADASettlementClaim).Include(e => e.YearlyPaymentT).Include(e => e.Employee).Where(e => e.Employee.Id == Emp).SingleOrDefault();
        //                    if (aa.TADASettlementClaim.Count() > 0)
        //                    {
        //                        TadaSettlementlist.AddRange(aa.TADASettlementClaim);
        //                    }
        //                    else
        //                    {
        //                        TadaSettlementlist.Add(tadasettlement);
        //                    }
        //                    // yearly payment strat
        //                    //if (aa.YearlyPaymentT.Count() > 0)
        //                    //{
        //                    //    OYrlyPaylist.AddRange(aa.YearlyPaymentT);
        //                    //}
        //                    // yearly payment strat

        //                    aa.TADASettlementClaim = TadaSettlementlist;
        //                    // yearly payment strat
        //                    //aa.YearlyPaymentT = OYrlyPaylist;
        //                    // yearly payment strat
        //                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
        //                    db.SaveChanges();
        //                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
        //                    ts.Complete();
        //                    Msg.Add("  Data Saved successfully  ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //            else
        //            {
        //                StringBuilder sb = new StringBuilder("");
        //                foreach (ModelState modelState in ModelState.Values)
        //                {
        //                    foreach (ModelError error in modelState.Errors)
        //                    {
        //                        sb.Append(error.ErrorMessage);
        //                        sb.Append("." + "\n");
        //                    }
        //                }
        //                var errorMsg = sb.ToString();
        //                Msg.Add(errorMsg);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LogFile Logfile = new LogFile();
        //            ErrorLog Err = new ErrorLog()
        //            {
        //                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                ExceptionMessage = ex.Message,
        //                ExceptionStackTrace = ex.StackTrace,
        //                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }

        //}

        public ActionResult GetLookupHotelEligibilityPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.HotelEligibilityPolicy.ToList();
                IEnumerable<HotelEligibilityPolicy> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.HotelEligibilityPolicy.ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetTravel_Hotel_Booking(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TravelHotelBooking.Include(e => e.HotelEligibilityPolicy).ToList();
                // IEnumerable<TravelHotelBooking> all;
                if (SkipIds != null)
                {
                    foreach (int a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.TravelHotelBooking.ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();

                    }
                }

                var list1 = db.LTCSettlementClaim.Where(e => e.Travel_Hotel_Booking.Count() > 0).ToList().SelectMany(e => e.Travel_Hotel_Booking);
                var list2 = db.TADASettlementClaim.Include(e => e.JourneyDetails).Include(e => e.JourneyDetails.Travel_Hotel_Booking).Where(e => e.JourneyDetails.Travel_Hotel_Booking.Count() > 0).ToList().SelectMany(e => e.JourneyDetails.Travel_Hotel_Booking);
                var list3 = fall.Except(list1).Except(list2);

                var r = (from ca in list3 select new { srno = ca.Id, lookupvalue = ca.HotelName + ", StartDate :" + ca.StartDate.Value.ToString("dd/MM/yyyy") }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public class ChildGetTADAsettleReqClass
        {

            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }
        }

        public class GetTAdasettleMyselfClass
        {
            public string TadaId { get; set; }
            public string ReqDate { get; set; }
            public string JoyStartDate { get; set; }
            public string JoyEndDate { get; set; }
            public string Status { get; set; }
        }

        public class GetTAdasettleReqClass
        {
            public string Emp { get; set; }
            public string ReqDate { get; set; }
            public string TADAType { get; set; }
            public string EligibleAmount { get; set; }
            public string AdvanceAmount { get; set; }
            public string Status { get; set; }
            public ChildGetTADAsettleReqClass RowData { get; set; }
        }

        public ActionResult Myself_datatadasettelment(string tadasettelmentids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string authority = Convert.ToString(Session["auho"]);

                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                List<GetTAdasettleMyselfClass> returndata = new List<GetTAdasettleMyselfClass>();
                int Ids = Convert.ToInt32(tadasettelmentids);

                var Tadasettleitems = db.TADASettlementClaim
                    .Include(e => e.JourneyDetails)
                    .Include(e => e.LTCWFDetails)
                    .Include(e => e.TADAType)
                    .Where(e => e.Id == Ids).SingleOrDefault();

                if (Tadasettleitems != null)
                {
                    int WfStatusNew = Tadasettleitems.LTCWFDetails.ToList().OrderByDescending(e => e.Id).FirstOrDefault().WFStatus;

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
                        StatusNarration = "Cancel";

                    if (authority.ToUpper() == "SANCTION" && WfStatusNew == 0)
                    {
                        GetTAdasettleMyselfClass ObjTADAsettleClaimRequest = new GetTAdasettleMyselfClass()
                        {
                            TadaId = Ids.ToString(),
                            JoyStartDate = Tadasettleitems.JourneyDetails != null ? Tadasettleitems.JourneyDetails.JourneyStart.Value.ToShortDateString() : "",
                            JoyEndDate = Tadasettleitems.JourneyDetails != null ? Tadasettleitems.JourneyDetails.JourneyEnd.Value.ToShortDateString() : "",
                            ReqDate = Tadasettleitems.DateOfAppl.Value.ToShortDateString(),
                            Status = StatusNarration
                        };
                        returndata.Add(ObjTADAsettleClaimRequest);
                    }
                    else if (authority.ToUpper() == "APPROVAL" && WfStatusNew == 1)
                    {
                        GetTAdasettleMyselfClass ObjTADAsettleClaimRequest = new GetTAdasettleMyselfClass()
                        {
                            TadaId = Ids.ToString(),
                            JoyStartDate = Tadasettleitems.JourneyDetails != null ? Tadasettleitems.JourneyDetails.JourneyStart.Value.ToShortDateString() : "",
                            JoyEndDate = Tadasettleitems.JourneyDetails != null ? Tadasettleitems.JourneyDetails.JourneyEnd.Value.ToShortDateString() : "",
                            ReqDate = Tadasettleitems.DateOfAppl.Value.ToShortDateString(),
                            Status = StatusNarration
                        };
                        returndata.Add(ObjTADAsettleClaimRequest);
                    }
                    else if (authority.ToUpper() == "MYSELF")
                    {
                        GetTAdasettleMyselfClass ObjTADAsettleClaimRequest = new GetTAdasettleMyselfClass()
                        {
                            TadaId = Ids.ToString(),
                            JoyStartDate = Tadasettleitems.JourneyDetails != null ? Tadasettleitems.JourneyDetails.JourneyStart.Value.ToShortDateString() : "",
                            JoyEndDate = Tadasettleitems.JourneyDetails != null ? Tadasettleitems.JourneyDetails.JourneyEnd.Value.ToShortDateString() : "",
                            ReqDate = Tadasettleitems.DateOfAppl.Value.ToShortDateString(),
                            Status = StatusNarration
                        };
                        returndata.Add(ObjTADAsettleClaimRequest);
                    }

                }

                return Json(returndata, JsonRequestBehavior.AllowGet);
            }

        }




        public ActionResult GetMyTAdasettlementClaimReq()   /// Get Created Data on Grid
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string authority = Convert.ToString(Session["auho"]);

                List<GetTAdasettleReqClass> OTADAsettlelaimlist = new List<GetTAdasettleReqClass>();
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);

                var db_data = db.EmployeePayroll
                      .Where(e => e.Id == Id)
                      .Include(e => e.TADASettlementClaim)
                      .Include(e => e.TADASettlementClaim.Select(a => a.LTCWFDetails))
                      .Include(e => e.TADASettlementClaim.Select(a => a.TADAType))

                     .FirstOrDefault();


                if (db_data != null)
                {
                    List<GetTAdasettleReqClass> returndata = new List<GetTAdasettleReqClass>();
                    returndata.Add(new GetTAdasettleReqClass
                    {

                        ReqDate = "Requisition Date",
                        TADAType = "TADAType",
                        EligibleAmount = "EligibleAmount",
                        AdvanceAmount = "AdvanceAmount",
                        Status = "Status"
                    });

                    var TadasettlementClaimlist = db_data.TADASettlementClaim.ToList();

                    foreach (var Tadasettleitems in TadasettlementClaimlist)
                    {
                        int WfStatusNew = Tadasettleitems.LTCWFDetails.ToList().OrderByDescending(e => e.Id).FirstOrDefault().WFStatus;

                        string Comments = Tadasettleitems.LTCWFDetails.Select(c => c.Comments).LastOrDefault();
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
                            StatusNarration = "Cancel";

                        if (authority.ToUpper() == "SANCTION" && WfStatusNew == 0)
                        {
                            GetTAdasettleReqClass ObjTADAsettleClaimRequest = new GetTAdasettleReqClass()
                            {
                                RowData = new ChildGetTADAsettleReqClass
                                {
                                    LvNewReq = Tadasettleitems.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString()

                                },

                                TADAType = Tadasettleitems.TADAType.LookupVal.ToString(),
                                EligibleAmount = Tadasettleitems.DA_Eligible_Amt.ToString() != "0" ? Tadasettleitems.DA_Eligible_Amt.ToString() : "0",
                                ReqDate = Tadasettleitems.DateOfAppl.Value.ToShortDateString(),
                                AdvanceAmount = Tadasettleitems.AdvAmt.ToString() != "0" ? Tadasettleitems.AdvAmt.ToString() : "0",
                                Status = StatusNarration
                            };
                            returndata.Add(ObjTADAsettleClaimRequest);
                        }
                        else if (authority.ToUpper() == "APPROVAL" && WfStatusNew == 1)
                        {
                            GetTAdasettleReqClass ObjTADAsettleClaimRequest = new GetTAdasettleReqClass()
                            {
                                RowData = new ChildGetTADAsettleReqClass
                                {
                                    LvNewReq = Tadasettleitems.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString()

                                },

                                TADAType = Tadasettleitems.TADAType.LookupVal.ToString(),
                                EligibleAmount = Tadasettleitems.DA_Eligible_Amt.ToString() != "0" ? Tadasettleitems.DA_Eligible_Amt.ToString() : "0",
                                ReqDate = Tadasettleitems.DateOfAppl.Value.ToShortDateString(),
                                AdvanceAmount = Tadasettleitems.AdvAmt.ToString() != "0" ? Tadasettleitems.AdvAmt.ToString() : "0",
                                Status = StatusNarration
                            };
                            returndata.Add(ObjTADAsettleClaimRequest);
                        }
                        else if (authority.ToUpper() == "MYSELF")
                        {
                            GetTAdasettleReqClass ObjTADAsettleClaimRequest = new GetTAdasettleReqClass()
                            {
                                RowData = new ChildGetTADAsettleReqClass
                                {
                                    LvNewReq = Tadasettleitems.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString()

                                },

                                TADAType = Tadasettleitems.TADAType.LookupVal.ToString(),
                                EligibleAmount = Tadasettleitems.DA_Eligible_Amt.ToString() != "0" ? Tadasettleitems.DA_Eligible_Amt.ToString() : "0",
                                ReqDate = Tadasettleitems.DateOfAppl.Value.ToShortDateString(),
                                AdvanceAmount = Tadasettleitems.AdvAmt.ToString() != "0" ? Tadasettleitems.AdvAmt.ToString() : "0",
                                Status = StatusNarration,
                            };
                            returndata.Add(ObjTADAsettleClaimRequest);
                        }

                    }

                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

            }

        }



        public class ChildGetTADASettlementReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }
            public string OdReqId { get; set; }

        }
        public class GetTADASettlementClaimClass1
        {
            public string EmpCode { get; set; }
            public string EmpName { get; set; }          
            public string ReqDate { get; set; }
            public string TADAType { get; set; }
            public string EligibleAmount { get; set; }
            public string AdvanceAmount { get; set; }

            public ChildGetTADASettlementReqClass RowData { get; set; }
        }

        public ActionResult GetTadaSettlementClaimReqonSanction(FormCollection form)
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
                // var returnDataClass = new List<returnDataClass>();

                List<GetTADASettlementClaimClass1> returndata = new List<GetTADASettlementClaimClass1>();
                returndata.Add(new GetTADASettlementClaimClass1
                {
                    EmpCode = "EmpCode",
                    EmpName = "EmpName",
                    ReqDate = "Requisition Date",
                    TADAType = "TADA Type",
                    EligibleAmount = "Eligible Amount",
                    AdvanceAmount = "Advance Amount",

                    RowData = new ChildGetTADASettlementReqClass
                    {
                        LvNewReq = "0",
                        EmpLVid = "0",
                        IsClose = "0",
                        LvHead_Id = "",
                    }
                });
                foreach (var item1 in EmpidsWithfunsub)
                {


                    var Emps = db.EmployeePayroll
                        .Where(e => (item1.ReportingEmployee.Contains(e.Employee.Id)))
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.Employee.ReportingStructRights)
                        .Include(e => e.Employee.ReportingStructRights.Select(b => b.AccessRights.ActionName))
                        .Include(e => e.Employee.ReportingStructRights.Select(b => b.FuncModules))
                        .Include(e => e.TADASettlementClaim)
                        .Include(e => e.TADASettlementClaim.Select(b => b.LTCWFDetails))
                        .Include(e => e.TADASettlementClaim.Select(w => w.WFStatus))
                        .Include(e => e.TADASettlementClaim.Select(w => w.TADAType))
                        .ToList();

                    foreach (var item in Emps)
                    {
                        if (item.TADASettlementClaim != null && item.TADASettlementClaim.Count() > 0)
                        {
                            var LvIds = UserManager.FilterTADASettlementClaim(item.TADASettlementClaim.OrderByDescending(e => e.DateOfAppl.Value.ToShortDateString()).ToList(),
                               Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId));

                            if (LvIds.Count() > 0)
                            {
                                var tadasettlementclaimreqdata = item.TADASettlementClaim.Where(e => LvIds.Contains(e.Id)).ToList();
                                foreach (var singletadaDetails in tadasettlementclaimreqdata)
                                {
                                    if (singletadaDetails.LTCWFDetails != null)
                                    {

                                        var session = Session["auho"].ToString().ToUpper();
                                        var EmpR = item.Employee.ReportingStructRights.Where(e => e.AccessRights != null && e.AccessRights.ActionName.LookupVal.ToUpper() == session && e.FuncModules.LookupVal.ToUpper() == item1.ModuleName.ToUpper())
                                        .Select(e => e.AccessRights.IsClose).FirstOrDefault();
                                        //if (stts.WFStatus == 0)
                                        //{

                                        returndata.Add(new GetTADASettlementClaimClass1
                                        {
                                            RowData = new ChildGetTADASettlementReqClass
                                            {
                                                LvNewReq = singletadaDetails.Id.ToString(),
                                                EmpLVid = item.Id.ToString(),
                                                IsClose = EmpR.ToString(),
                                                LvHead_Id = "",
                                            },
                                            EmpCode = item.Employee.EmpCode.ToString(),
                                            EmpName = item.Employee.EmpName.FullNameFML.ToString(),
                                            TADAType = singletadaDetails.TADAType.LookupVal.ToString(),
                                            EligibleAmount = singletadaDetails.DA_Eligible_Amt.ToString() != "0" ? singletadaDetails.DA_Eligible_Amt.ToString() : "0",
                                            ReqDate = singletadaDetails.DateOfAppl.Value.ToShortDateString(),
                                            AdvanceAmount = singletadaDetails.AdvAmt.ToString() != "0" ? singletadaDetails.AdvAmt.ToString() : "0",

                                        });
                                        //  }

                                    }
                                }
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



        public class TADASettlementClaimRequestData
        {
            public int WfStatusNew { get; set; }
            public int JourneyDetailsID { get; set; }
            public string JourneyDetailsFulldetails { get; set; }
            public int Travel_Hotel_Bookingid { get; set; }
            public string Travel_Hotel_Bookingdetails { get; set; }
            public string Status { get; set; }
            public string StatusNarration { get; set; }
            public string Emplvhead { get; set; }
            public int JourneyDetailsId { get; set; }
            public string JourneyDetails { get; set; }
            public int ReferencedocumentId { get; set; }
            public string Referencedocument { get; set; }
            public string Billno { get; set; }
            public string ReqDate { get; set; }
            public string Narration { get; set; }
            public string SanctionCode { get; set; }
            public string SanctionEmpname { get; set; }
            public string RecomendationCode { get; set; }
            public string RecomendationEmpname { get; set; }
            public double TADAAdvanceClaim { get; set; }
            public double Claim_Amt { get; set; }
            public double Travel_Hotel_Booking { get; set; }
            public double NoOfDays { get; set; }
            public double AdvAmt { get; set; }
            public double SettlementAmt { get; set; }
            public double SanctionAmt { get; set; }
            public double Eligible_Amt { get; set; }
            public int TADAType { get; set; }
            public bool TrClosed { get; set; }
            public bool TrReject { get; set; }
            public string SanctionComment { get; set; }
            public string ApporavalComment { get; set; }
            public string Wf { get; set; }
            public Int32 Id { get; set; }
            public Int32 Lvnewreq { get; set; }
            public double RatePerDay { get; set; }
            public string EmployeeName { get; set; }
            public string Remark { get; set; }
            public string Empcode { get; set; }
            public string Isclose { get; set; }
            public int EmployeeId { get; set; }
            public string Incharge { get; set; }
            public int TADAAdvanceClaimId { get; set; }
            public string TADAAdvanceClaimFulldetails { get; set; }


        }

        public ActionResult GetTADASettlementClaimData(string data)
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
                else if (authority.ToUpper() == "RECOMMENDATION")
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

                var EmpLvIdint = Convert.ToInt32(emplvId);

                var W = db.EmployeePayroll
                    .Include(e => e.TADASettlementClaim)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.TADASettlementClaim.Select(t => t.GeoStruct))
                    .Include(e => e.TADASettlementClaim.Select(t => t.FuncStruct))
                    .Include(e => e.TADASettlementClaim.Select(t => t.PayStruct))
                    .Include(e => e.TADASettlementClaim.Select(t => t.LTCWFDetails))
                    .Include(e => e.TADASettlementClaim.Select(t => t.WFStatus))
                    .Include(e => e.TADASettlementClaim.Select(t => t.TADAType))
                    .Include(e => e.TADASettlementClaim.Select(t => t.TADAAdvanceClaim))
                    //.Include(e => e.TADASettlementClaim.Select(t => t.JourneyDetails_Id))
                    .Include(e => e.TADASettlementClaim.Select(t => t.JourneyDetails))
                    //.Include(e => e.TADASettlementClaim.Select(t => t.Travel_Hotel_Booking))//28082023
                    //.Include(e => e.TADASettlementClaim.Select(t => t.Travel_Hotel_Booking.Select(a => a.HotelEligibilityPolicy)))

                    .Where(e => e.Employee.Id == EmpLvIdint).SingleOrDefault();

                if (W.TADASettlementClaim.Count() > 0)
                {

                    var v = W.TADASettlementClaim.Where(e => e.Id == id).Select(s => new TADASettlementClaimRequestData
                    {
                        EmployeeId = W.Employee.Id,
                        EmployeeName = W.Employee.EmpCode + " " + W.Employee.EmpName.FullNameFML,
                        Lvnewreq = s.Id,
                        Empcode = W.Employee.EmpCode,
                        TADAAdvanceClaimId = s.TADAAdvanceClaim != null ? s.TADAAdvanceClaim.Id : 0,
                        TADAAdvanceClaimFulldetails = s.TADAAdvanceClaim != null ? s.TADAAdvanceClaim.FullDetails.ToString() : "",
                        JourneyDetailsID = s.JourneyDetails != null ? s.JourneyDetails.Id : 0,
                        JourneyDetailsFulldetails = s.JourneyDetails != null ? s.JourneyDetails.FullDetails.ToString() : "",
                        TADAType = s.TADAType != null ? s.TADAType.Id : 0,
                        Eligible_Amt = s.DA_Eligible_Amt,
                        SanctionAmt = s.DA_SanctionAmt,
                        SettlementAmt = s.SettlementAmt,
                        AdvAmt = s.AdvAmt,
                        Claim_Amt = s.DA_Claim_Amt,
                        Remark = s.Remark,
                        NoOfDays = s.NoOfDays,
                        ReqDate = s.DateOfAppl != null ? s.DateOfAppl.Value.ToShortDateString() : null,
                        Isclose = status.ToString(),
                        TrClosed = s.TrClosed,
                        SanctionCode = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
                        SanctionComment = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => SanctionStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                        ApporavalComment = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => ApporvalStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                        Wf = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).LastOrDefault() : null,
                        RecomendationCode = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => RecomendationStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
                        RecomendationEmpname = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => RecomendationStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,

                    }).ToList();

                    TempData["TADAsettleClaim"] = v;
                }
                var TADAsettleClaimreturn = TempData["TADAsettleClaim"];

                return Json(TADAsettleClaimreturn, JsonRequestBehavior.AllowGet);
            }
        }




        public ActionResult Update_TADASettlementClaim(TADASettlementClaim HbReq, FormCollection form, String data)
        {
            var tadasettelment_ids = TempData["Tada_settelment_ids"];
            int TadaSettelmentids_id = Convert.ToInt32(tadasettelment_ids);

            string authority = form["authority"] == null ? null : form["authority"];
            string TadasettelmentCancel = form["IsCancel"] == "0" ? "" : form["IsCancel"];
            bool IsCanceltadasettelment = Convert.ToBoolean(TadasettelmentCancel);
            var isClose = form["isClose"] == null ? null : form["isClose"];
            if (authority == null && isClose == null)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
            var ids = Utility.StringIdsToListString(data);
            var Hbnewreqid = Convert.ToInt32(ids[0]);
            var EmpId = Convert.ToInt32(ids[1]);
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

            using (DataBaseContext db = new DataBaseContext())
            {
                var TadaSettelmentvar = db.TADASettlementClaim.Where(e => e.Id == TadaSettelmentids_id).SingleOrDefault();
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
                var journeydetailsIIdd = TempData["Journeydetails_ids"];
                int journeydetailsIIds = journeydetailsIIdd != null ? Convert.ToInt32(journeydetailsIIdd) : 0;
                var JourneyObjdata = db.JourneyDetails.Include(e => e.JourneyObject)
                    .Include(r => r.MisExpenseObject)
                    .Include(r => r.DAObject)
                    .Include(r => r.Travel_Hotel_Booking)
                    .Where(e => e.Id == journeydetailsIIds).SingleOrDefault();

                double HotelSetamts = 0;
                double TASetAmts = 0;
                double DASetAmts = 0;
                double MisSetAmts = 0;

                if (JourneyObjdata != null)
                {
                    if (JourneyObjdata.Travel_Hotel_Booking != null)
                    {
                        var hotelobj = JourneyObjdata.Travel_Hotel_Booking.ToList();

                        foreach (var item in hotelobj)
                        {
                            var hotelobject = db.TravelHotelBooking.Include(r => r.HotelObject).Where(e => e.Id == item.Id).FirstOrDefault();
                            if (hotelobject != null)
                            {
                                HotelSetamts = HotelSetamts + hotelobject.HotelObject.ToList().Select(r => r.HotelSettleAmt).Sum();
                            }

                        }
                    }

                    TASetAmts = JourneyObjdata.JourneyObject.ToList().Select(r => r.TASettleAmt).Sum();
                    DASetAmts = JourneyObjdata.DAObject.ToList().Select(r => r.DASettleAmt).Sum();
                    MisSetAmts = JourneyObjdata.MisExpenseObject.ToList().Select(r => r.MisExpenseSettleAmt).Sum();


                }



                var query = db.EmployeePayroll.Where(e => e.Id == EmpId)
                    .Include(e => e.TADASettlementClaim)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.TADASettlementClaim.Select(t => t.GeoStruct))
                    .Include(e => e.TADASettlementClaim.Select(t => t.FuncStruct))
                    .Include(e => e.TADASettlementClaim.Select(t => t.PayStruct))
                    .Include(e => e.TADASettlementClaim.Select(t => t.LTCWFDetails))
                    .Include(e => e.TADASettlementClaim.Select(t => t.WFStatus))
                    .Include(e => e.TADASettlementClaim.Select(t => t.TADAType))
                    .Include(e => e.TADASettlementClaim.Select(t => t.TADAAdvanceClaim))
                    .Include(e => e.TADASettlementClaim.Select(t => t.JourneyDetails))
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }

                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);

                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        var TADASettlementClaimList = query.TADASettlementClaim.Where(e => e.Id == Hbnewreqid).ToList();
                        int TADASettClids = db.TADASettlementClaim.Where(e => e.JourneyDetails_Id == journeydetailsIIds).Select(r => r.Id).SingleOrDefault();
                        var db_dataTadaSettlementclaiml = db.TADASettlementClaim.Include(e => e.TADAAdvanceClaim).Where(e => e.Id == TADASettClids).SingleOrDefault();
                        var db_datajourneyobjdetails = db.JourneyDetails.Where(e => e.Id == journeydetailsIIds).SingleOrDefault();
                        if (db_datajourneyobjdetails != null)
                        {
                            db_datajourneyobjdetails.Id = journeydetailsIIds;
                            db_datajourneyobjdetails.TASettleAmt = TASetAmts;
                            db_datajourneyobjdetails.DASettleAmt = DASetAmts;
                            db_datajourneyobjdetails.HotelSettleAmt = HotelSetamts;
                            db_datajourneyobjdetails.MisExpenseSettleAmt = MisSetAmts;
                            db_datajourneyobjdetails.DBTrack = new DBTrack { Action = "M", CreatedBy = SessionManager.UserName };
                            db.JourneyDetails.Attach(db_datajourneyobjdetails);
                            db.Entry(db_datajourneyobjdetails).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }

                        foreach (var Hbitems in TADASettlementClaimList)
                        {
                            List<FunctAllWFDetails> oFunctWFDetails_List = new List<FunctAllWFDetails>();
                            FunctAllWFDetails objFunctAllWFDetails = new FunctAllWFDetails();

                            if (authority.ToUpper() == "MYSELF")
                            {
                                if (IsCanceltadasettelment == true)
                                {
                                    if (TadaSettelmentvar != null)
                                    {
                                        int WfStatusNew = TadaSettelmentvar.LTCWFDetails.ToList().OrderByDescending(e => e.Id).FirstOrDefault().WFStatus;
                                        if (WfStatusNew == 0)
                                        {
                                            objFunctAllWFDetails = new FunctAllWFDetails
                                            {
                                                WFStatus = 6,
                                                Comments = ReasonMySelf,
                                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                            };
                                            oFunctWFDetails_List.Add(objFunctAllWFDetails);
                                            Hbitems.TrClosed = true;
                                        }
                                        else
                                        {
                                            return Json(new Utility.JsonClass { status = true, responseText = "Only Applied Data Can be Calcel..!" }, JsonRequestBehavior.AllowGet);
                                        }

                                    }

                                }
                                else
                                {
                                    Hbitems.TrClosed = false;
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
                                    objFunctAllWFDetails = new FunctAllWFDetails
                                    {
                                        WFStatus = 1,
                                        Comments = ReasonSanction,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                    };

                                    oFunctWFDetails_List.Add(objFunctAllWFDetails);

                                    Hbitems.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                                    Hbitems.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").SingleOrDefault();

                                    if (Convert.ToBoolean(isClose) == true)
                                    {
                                        double Noofdays = 0;
                                        double TAElgAmt = 0;
                                        double TAClaimAmt = 0;
                                        double TASetAmt = 0;
                                        double Hotelelgamt = 0;
                                        double HotelClaimamt = 0;
                                        double HotelSetamt = 0;
                                        double DAElgAmt = 0;
                                        double DAClaimAmt = 0;
                                        double DASetAmt = 0;
                                        double MisElgAmt = 0;
                                        double MisClaimAmt = 0;
                                        double MisSetAmt = 0;
                                        double Advanceamt = 0;

                                        var JourneyObjdatafinal = db.JourneyDetails.Include(e => e.JourneyObject)
                                        .Include(r => r.MisExpenseObject)
                                        .Include(r => r.DAObject)
                                       .Include(r => r.Travel_Hotel_Booking)
                                        .Where(e => e.Id == journeydetailsIIds).SingleOrDefault();
                                        if (JourneyObjdatafinal != null)
                                        {
                                            Noofdays = (JourneyObjdatafinal.JourneyEnd.Value.Date - JourneyObjdatafinal.JourneyStart.Value.Date).Days + 1;
                                            TAElgAmt = JourneyObjdatafinal.TAElligibleAmt;
                                            TAClaimAmt = JourneyObjdatafinal.TAClaimAmt;
                                            TASetAmt = JourneyObjdatafinal.TASettleAmt;
                                            Hotelelgamt = JourneyObjdatafinal.HotelElligibleAmt;
                                            HotelClaimamt = JourneyObjdatafinal.HotelClaimAmt;
                                            HotelSetamt = JourneyObjdatafinal.HotelSettleAmt;
                                            DAElgAmt = JourneyObjdatafinal.DAElligibleAmt;
                                            DAClaimAmt = JourneyObjdatafinal.DAClaimAmt;
                                            DASetAmt = JourneyObjdatafinal.DASettleAmt;
                                            MisElgAmt = JourneyObjdatafinal.MisExpenseElligibleAmt;
                                            MisClaimAmt = JourneyObjdatafinal.MisExpenseClaimAmt;
                                            MisSetAmt = JourneyObjdatafinal.MisExpenseSettleAmt;

                                            if (db_dataTadaSettlementclaiml.TADAAdvanceClaim != null)
                                            {
                                                Advanceamt = db_dataTadaSettlementclaiml.TADAAdvanceClaim.SanctionedAmt;
                                            }
                                        }
                                        if (db_dataTadaSettlementclaiml != null)
                                        {
                                            db_dataTadaSettlementclaiml.Id = TADASettClids;
                                            db_dataTadaSettlementclaiml.NoOfDays = Noofdays;
                                            db_dataTadaSettlementclaiml.DA_Eligible_Amt = DAElgAmt;
                                            db_dataTadaSettlementclaiml.DA_Claim_Amt = DAClaimAmt;
                                            db_dataTadaSettlementclaiml.DA_SanctionAmt = DASetAmt;
                                            db_dataTadaSettlementclaiml.TA_Eligible_Amt = TAElgAmt;
                                            db_dataTadaSettlementclaiml.TA_Claim_Amt = TAClaimAmt;
                                            db_dataTadaSettlementclaiml.TA_SanctionAmt = TASetAmt;
                                            db_dataTadaSettlementclaiml.Hotel_Eligible_Amt = Hotelelgamt;
                                            db_dataTadaSettlementclaiml.Hotel_Claim_Amt = HotelClaimamt;
                                            db_dataTadaSettlementclaiml.Hotel_SanctionAmt = HotelSetamt;
                                            db_dataTadaSettlementclaiml.MisExpense_Claim_Amt = MisElgAmt;
                                            db_dataTadaSettlementclaiml.MisExpense_Eligible_Amt = MisClaimAmt;
                                            db_dataTadaSettlementclaiml.MisExpense_SanctionAmt = MisSetAmt;
                                            db_dataTadaSettlementclaiml.SettlementAmt = (DASetAmt + TASetAmt + HotelSetamt + MisSetAmt) - Advanceamt;
                                            db_dataTadaSettlementclaiml.DBTrack = new DBTrack { Action = "M", CreatedBy = SessionManager.UserName };
                                            db.TADASettlementClaim.Attach(db_dataTadaSettlementclaiml);
                                            db.Entry(db_dataTadaSettlementclaiml).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();

                                            Employee OEmployee = null;
                                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                                           .Where(r => r.Id == EmpId).AsNoTracking().SingleOrDefault();

                                            List<TADASettlementClaim> TadaSettlementlist = new List<TADASettlementClaim>();
                                            var aa = db.EmployeePayroll.Include(e => e.TADASettlementClaim).Include(e => e.YearlyPaymentT).Include(e => e.Employee).Where(e => e.Employee.Id == EmpId).SingleOrDefault();
                                            TadaSettlementlist.Add(db.TADASettlementClaim.Find(db_dataTadaSettlementclaiml.Id));
                                            if (aa == null)
                                            {
                                                EmployeePayroll OTEP = new EmployeePayroll()
                                                {
                                                    Employee = db.Employee.Find(OEmployee.Id),
                                                    TADASettlementClaim = TadaSettlementlist,
                                                };
                                                db.EmployeePayroll.Add(OTEP);
                                                db.SaveChanges();
                                            }
                                            else
                                            {
                                                var aa1 = db.EmployeePayroll.Find(aa.Id);
                                                TadaSettlementlist.AddRange(aa1.TADASettlementClaim);
                                                aa1.TADASettlementClaim = TadaSettlementlist;
                                                db.EmployeePayroll.Attach(aa1);
                                                db.Entry(aa1).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                db.Entry(aa1).State = System.Data.Entity.EntityState.Detached;

                                            }
                                        }
                                    }

                                }
                                else if (Convert.ToBoolean(Sanction) == false)
                                {
                                    objFunctAllWFDetails = new FunctAllWFDetails
                                    {
                                        WFStatus = 2,
                                        Comments = ReasonSanction,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                    };
                                    oFunctWFDetails_List.Add(objFunctAllWFDetails);
                                    SanctionRejected = true;
                                }
                            }
                            else if (authority.ToUpper() == "APPROVAL")
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

                                    objFunctAllWFDetails = new FunctAllWFDetails
                                    {
                                        WFStatus = 3,
                                        Comments = ReasonApproval,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                    };
                                    oFunctWFDetails_List.Add(objFunctAllWFDetails);

                                    Hbitems.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                                    Hbitems.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault();

                                    if (Convert.ToBoolean(isClose) == true)
                                    {
                                        double Noofdays = 0;
                                        double TAElgAmt = 0;
                                        double TAClaimAmt = 0;
                                        double TASetAmt = 0;
                                        double Hotelelgamt = 0;
                                        double HotelClaimamt = 0;
                                        double HotelSetamt = 0;
                                        double DAElgAmt = 0;
                                        double DAClaimAmt = 0;
                                        double DASetAmt = 0;
                                        double MisElgAmt = 0;
                                        double MisClaimAmt = 0;
                                        double MisSetAmt = 0;
                                        double Advanceamt = 0;

                                        var JourneyObjdatafinal = db.JourneyDetails.Include(e => e.JourneyObject)
                                        .Include(r => r.MisExpenseObject)
                                        .Include(r => r.DAObject)
                                       .Include(r => r.Travel_Hotel_Booking)
                                        .Where(e => e.Id == journeydetailsIIds).SingleOrDefault();

                                        if (JourneyObjdatafinal != null)
                                        {
                                            Noofdays = (JourneyObjdatafinal.JourneyEnd.Value.Date - JourneyObjdatafinal.JourneyStart.Value.Date).Days + 1;
                                            TAElgAmt = JourneyObjdatafinal.TAElligibleAmt;
                                            TAClaimAmt = JourneyObjdatafinal.TAClaimAmt;
                                            TASetAmt = JourneyObjdatafinal.TASettleAmt;
                                            Hotelelgamt = JourneyObjdatafinal.HotelElligibleAmt;
                                            HotelClaimamt = JourneyObjdatafinal.HotelClaimAmt;
                                            HotelSetamt = JourneyObjdatafinal.HotelSettleAmt;
                                            DAElgAmt = JourneyObjdatafinal.DAElligibleAmt;
                                            DAClaimAmt = JourneyObjdatafinal.DAClaimAmt;
                                            DASetAmt = JourneyObjdatafinal.DASettleAmt;
                                            MisElgAmt = JourneyObjdatafinal.MisExpenseElligibleAmt;
                                            MisClaimAmt = JourneyObjdatafinal.MisExpenseClaimAmt;
                                            MisSetAmt = JourneyObjdatafinal.MisExpenseSettleAmt;

                                            if (db_dataTadaSettlementclaiml.TADAAdvanceClaim != null)
                                            {
                                                Advanceamt = db_dataTadaSettlementclaiml.TADAAdvanceClaim.SanctionedAmt;
                                            }
                                        }
                                        if (db_dataTadaSettlementclaiml != null)
                                        {
                                            db_dataTadaSettlementclaiml.Id = TADASettClids;
                                            db_dataTadaSettlementclaiml.NoOfDays = Noofdays;
                                            db_dataTadaSettlementclaiml.DA_Eligible_Amt = DAElgAmt;
                                            db_dataTadaSettlementclaiml.DA_Claim_Amt = DAClaimAmt;
                                            db_dataTadaSettlementclaiml.DA_SanctionAmt = DASetAmt;
                                            db_dataTadaSettlementclaiml.TA_Eligible_Amt = TAElgAmt;
                                            db_dataTadaSettlementclaiml.TA_Claim_Amt = TAClaimAmt;
                                            db_dataTadaSettlementclaiml.TA_SanctionAmt = TASetAmt;
                                            db_dataTadaSettlementclaiml.Hotel_Eligible_Amt = Hotelelgamt;
                                            db_dataTadaSettlementclaiml.Hotel_Claim_Amt = HotelClaimamt;
                                            db_dataTadaSettlementclaiml.Hotel_SanctionAmt = HotelSetamt;
                                            db_dataTadaSettlementclaiml.MisExpense_Claim_Amt = MisElgAmt;
                                            db_dataTadaSettlementclaiml.MisExpense_Eligible_Amt = MisClaimAmt;
                                            db_dataTadaSettlementclaiml.MisExpense_SanctionAmt = MisSetAmt;
                                            db_dataTadaSettlementclaiml.SettlementAmt = (DASetAmt + TASetAmt + HotelSetamt + MisSetAmt) - Advanceamt;
                                            db_dataTadaSettlementclaiml.DBTrack = new DBTrack { Action = "M", CreatedBy = SessionManager.UserName };
                                            db.TADASettlementClaim.Attach(db_dataTadaSettlementclaiml);
                                            db.Entry(db_dataTadaSettlementclaiml).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();

                                            Employee OEmployee = null;
                                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                                           .Where(r => r.Id == EmpId).AsNoTracking().SingleOrDefault();

                                            List<TADASettlementClaim> TadaSettlementlist = new List<TADASettlementClaim>();
                                            var aa = db.EmployeePayroll.Include(e => e.TADASettlementClaim).Include(e => e.YearlyPaymentT).Include(e => e.Employee).Where(e => e.Employee.Id == EmpId).SingleOrDefault();
                                            TadaSettlementlist.Add(db.TADASettlementClaim.Find(db_dataTadaSettlementclaiml.Id));
                                            if (aa == null)
                                            {
                                                EmployeePayroll OTEP = new EmployeePayroll()
                                                {
                                                    Employee = db.Employee.Find(OEmployee.Id),
                                                    TADASettlementClaim = TadaSettlementlist,
                                                };
                                                db.EmployeePayroll.Add(OTEP);
                                                db.SaveChanges();
                                            }
                                            else
                                            {
                                                var aa1 = db.EmployeePayroll.Find(aa.Id);
                                                TadaSettlementlist.AddRange(aa1.TADASettlementClaim);
                                                aa1.TADASettlementClaim = TadaSettlementlist;
                                                db.EmployeePayroll.Attach(aa1);
                                                db.Entry(aa1).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                db.Entry(aa1).State = System.Data.Entity.EntityState.Detached;

                                            }
                                        }
                                    }


                                }
                                else if (Convert.ToBoolean(Approval) == false)
                                {
                                    objFunctAllWFDetails = new FunctAllWFDetails
                                    {
                                        WFStatus = 4,
                                        Comments = ReasonApproval,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                    };
                                    oFunctWFDetails_List.Add(objFunctAllWFDetails);
                                    Hbitems.TrClosed = true;
                                    HrRejected = true;
                                }
                            }
                            else if (authority.ToUpper() == "RECOMMENDATION")
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

                                    var CheckAllreadyRecomendation = TADASettlementClaimList.Where(e => e.LTCWFDetails.Any(r => r.WFStatus == 7)).ToList();
                                    if (CheckAllreadyRecomendation.Count() > 0)
                                    {
                                        return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Recomendation....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                    }
                                    objFunctAllWFDetails = new FunctAllWFDetails
                                    {
                                        WFStatus = 7,
                                        Comments = ReasonRecomendation,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                    };
                                }
                                else if (Convert.ToBoolean(Recomendation) == false)
                                {
                                    var CheckAllreadyRecomendation = TADASettlementClaimList.Where(e => e.LTCWFDetails.Any(r => r.WFStatus == 8)).ToList();
                                    if (CheckAllreadyRecomendation.Count() > 0)
                                    {
                                        return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Rejected....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                    }

                                    objFunctAllWFDetails = new FunctAllWFDetails
                                    {
                                        WFStatus = 8,
                                        Comments = ReasonRecomendation,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                    };

                                    oFunctWFDetails_List.Add(objFunctAllWFDetails);
                                    Hbitems.TrClosed = true;
                                    SanctionRejected = true;
                                }
                            }
                            if (Hbitems.LTCWFDetails != null)
                            {
                                oFunctWFDetails_List.AddRange(Hbitems.LTCWFDetails);
                            }

                            Hbitems.LTCWFDetails = oFunctWFDetails_List;
                            db.TADASettlementClaim.Attach(Hbitems);
                            db.Entry(Hbitems).State = System.Data.Entity.EntityState.Modified;
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


    }
}