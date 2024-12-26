using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class TADASettlementClaimController : Controller
    {
        //
        // GET: /TADASettlmentClaim/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/TADASettlementClaim/Index.cshtml");
        }
        public ActionResult partial_TADAAdvanceclaim()
        {
            return View("~/Views/Shared/Payroll/_TADAAdvanceClaim.cshtml");
        }
        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Payroll/_TADASettlClaimGridPartial.cshtml");
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
        public class GetTadasettlamt //childgrid
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
        public ActionResult GridEditData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //var empltcblockdata = db.EmpLTCBlockT.Select(e => e.LTCSettlementClaim.Where(t => t.Id == data)).ToList();


                    var Q = db.TADASettlementClaim
                        .Include(e => e.JourneyDetails)
                        //  .Include(e => e.Travel_Hotel_Booking)
                        .Include(e => e.TADAAdvanceClaim)
                        //.Include(e=>e.FamilyDetails)
                        //.Include(e=>e.EmployeeDocuments)
                     .Where(e => e.Id == data).Select
                     (c => new
                     {
                         Id = c.Id,
                         // BlockPeriod = c.FullDetails,
                         ReqDate = c.DateOfAppl,
                         Noofdays = c.NoOfDays,
                         TADAType = c.TADAType != null ? c.TADAType.Id : 0,
                         // lvreqid = c.LvNewReq == null ? 0 : c.LvNewReq.Id,
                         TA_Eligible_Amt = c.TA_Eligible_Amt,
                         AdvAmt = c.AdvAmt,
                         Remark = c.Remark,
                         TA_Claim_Amt = c.TA_Claim_Amt,
                         EncashmentAmount = c.EncashmentAmount == null ? 0 : c.EncashmentAmount,
                         TA_SanctionAmt = c.TA_SanctionAmt == null ? 0 : c.TA_SanctionAmt,
                         SettlementAmt = c.SettlementAmt == null ? 0 : c.SettlementAmt,
                         LTCAdvAmt = c.AdvAmt == null ? 0 : c.AdvAmt,
                         JourneydetailsId = c.JourneyDetails == null ? 0 : c.JourneyDetails.Id,
                         JourneydetailsFulldetails = c.JourneyDetails == null ? "" : c.JourneyDetails.FullDetails,
                         TADAAdvanceClaimid = c.TADAAdvanceClaim == null ? 0 : c.TADAAdvanceClaim.Id,
                         LTCAdvanceClaimfulldetails = c.TADAAdvanceClaim == null ? "" : c.TADAAdvanceClaim.FullDetails,
                         //travelhoteldetails = c.Travel_Hotel_Booking.Select(n => new
                         //{
                         //    id = n.Id,
                         //    fulldetails = "HotelName:" + n.HotelName + ",HotelDesc" + n.HotelDesc

                         //})

                     }).SingleOrDefault();


                    return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {

                    throw;
                }
                //return Json(new Object[] { Q, return_data, return_dataDoc, JsonRequestBehavior.AllowGet });
                //return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GridEditSave(TADASettlementClaim c, FormCollection form, string data)
        {
            List<string> Msgs = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
                string tadaadvanceclaimlist = form["TADAAdvanceClaimListPartial"] == "0" ? "" : form["TADAAdvanceClaimListPartial"];
                string tadatypelist = form["TADATypelistPartial"] == "0" ? "" : form["TADATypelistPartial"];
                //string lvreqlist = form["LvReqListPartial"] == "0" ? "" : form["LvReqListPartial"];
                string journewdetailslist = form["JourneyDetailsListPartial"] == "0" ? "" : form["JourneyDetailsListPartial"];
                string hotelbookinglist = form["HotelBookingDetailsListPartial"] == "0" ? "" : form["HotelBookingDetailsListPartial"];
                //string LtcBlock = form["BlockIdPartial"] == "0" ? "" : form["BlockIdPartial"];

                var TADASettleClaimData = db.TADASettlementClaim
                     .Include(e => e.JourneyDetails)
                    // .Include(e => e.Travel_Hotel_Booking)
                     .Include(e => e.TADAAdvanceClaim)
                    .Where(e => e.Id == Id).SingleOrDefault();

                if (tadaadvanceclaimlist != null && tadaadvanceclaimlist != "")
                {
                    int tadaadvanceid = Convert.ToInt32(tadaadvanceclaimlist);
                    var value = db.TADAAdvanceClaim.Where(e => e.Id == tadaadvanceid).SingleOrDefault();
                    TADASettleClaimData.TADAAdvanceClaim = value;

                }
                else
                {
                    TADASettleClaimData.TADAAdvanceClaim = null;
                }
                //if (hotelbookinglist != null)
                //{
                //    var ids = Utility.StringIdsToListIds(hotelbookinglist);
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
                //    TADASettleClaimData.Travel_Hotel_Booking = travelhotelbookinglist;
                //}
                //else
                //{
                //    TADASettleClaimData.Travel_Hotel_Booking = null;
                //}


                if (journewdetailslist != null)
                {
                    if (journewdetailslist != "")
                    {
                        int ids = Convert.ToInt32(journewdetailslist);
                        var val = db.JourneyDetails.Where(e => e.Id == ids).SingleOrDefault();
                        TADASettleClaimData.JourneyDetails = val;
                    }
                }
                else
                {
                    TADASettleClaimData.JourneyDetails = null;
                }
                //if (lvreqlist != null)
                //{
                //    if (lvreqlist != "")
                //    {
                //        int ids = Convert.ToInt32(lvreqlist);
                //        var val = db.LvNewReq.Where(e => e.Id == ids).SingleOrDefault();
                //        LTCSettleClaimData.LvNewReq = val;
                //    }
                //}
                //else
                //{
                //    LTCSettleClaimData.Travel_Hotel_Booking = null;
                //}
                if (tadatypelist != null && tadatypelist != "")
                {
                    var val = db.LookupValue.Find(int.Parse(tadatypelist));
                    TADASettleClaimData.TADAType = val;
                }
                else
                {
                    TADASettleClaimData.TADAType = null;
                }

                TADASettleClaimData.JourneyDetails = TADASettleClaimData.JourneyDetails;
                TADASettleClaimData.TADAAdvanceClaim = TADASettleClaimData.TADAAdvanceClaim;
                // TADASettleClaimData.Travel_Hotel_Booking = TADASettleClaimData.Travel_Hotel_Booking;
                TADASettleClaimData.Remark = c.Remark;
                //  TADASettleClaimData.LvNewReq = TADASettleClaimData.LvNewReq;
                TADASettleClaimData.NoOfDays = c.NoOfDays;
                TADASettleClaimData.TADAType = TADASettleClaimData.TADAType;
                TADASettleClaimData.AdvAmt = c.AdvAmt;
                TADASettleClaimData.SettlementAmt = c.SettlementAmt;
                TADASettleClaimData.TA_SanctionAmt = c.TA_SanctionAmt;
                TADASettleClaimData.EncashmentAmount = c.EncashmentAmount;
                TADASettleClaimData.DateOfAppl = c.DateOfAppl;
                TADASettleClaimData.TA_Eligible_Amt = c.TA_Eligible_Amt;
                TADASettleClaimData.TA_Claim_Amt = c.TA_Claim_Amt;
                using (TransactionScope ts = new TransactionScope())
                {

                    TADASettleClaimData.DBTrack = new DBTrack
                    {
                        CreatedBy = TADASettleClaimData.DBTrack.CreatedBy == null ? null : TADASettleClaimData.DBTrack.CreatedBy,
                        CreatedOn = TADASettleClaimData.DBTrack.CreatedOn == null ? null : TADASettleClaimData.DBTrack.CreatedOn,
                        Action = "M",
                        ModifiedBy = SessionManager.UserName,
                        ModifiedOn = DateTime.Now
                    };


                    try
                    {
                        db.TADASettlementClaim.Attach(TADASettleClaimData);
                        db.Entry(TADASettleClaimData).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(TADASettleClaimData).State = System.Data.Entity.EntityState.Detached;

                        ts.Complete();
                        return this.Json(new { status = true, responseText = "Data Updated Successfully.", JsonRequestBehavior.AllowGet });
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Create", new { concurrencyError = true, id = TADASettleClaimData.Id });
                    }
                    catch (DataException /* dex */)
                    {
                        return this.Json(new { status = false, responseText = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        List<string> Msg = new List<string>();
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
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

        public ActionResult Get_TADASettlementClaim(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //var db_data = db.EmployeePayroll
                    //    .Include(e => e.EmpLTCBlock)
                    //    .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT))
                    //    .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim)))
                    //    .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim.Select(c => c.LTC_TYPE))))
                    //    .Where(e => e.Id == data).FirstOrDefault();
                    var db_data = db.EmployeePayroll
                       .Include(e => e.TADASettlementClaim)
                        .Include(e => e.TADASettlementClaim.Select(c => c.TADAType))
                       .Where(e => e.Id == data).FirstOrDefault();

                    if (db_data.TADASettlementClaim != null)
                    {
                        List<TADASettlClaimChildDataClass> returndata = new List<TADASettlClaimChildDataClass>();
                        var LTCAdvClaimlist = db_data.TADASettlementClaim;
                        foreach (var item in db_data.TADASettlementClaim.OrderByDescending(e => e.Id))
                        {

                            returndata.Add(new TADASettlClaimChildDataClass
                            {
                                Id = item.Id,
                                ReqDate = item.DateOfAppl.Value != null ? item.DateOfAppl.Value.ToShortDateString() : "",
                                TADAType = item.TADAType != null ? item.TADAType.LookupVal.ToString() : null,
                                EligibleAmt = item.TA_Eligible_Amt,
                                AdvanceAmt = item.AdvAmt,
                                Remark = item.Remark,

                            });



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
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                TADASettlementClaim TadasettleclaimReq = db.TADASettlementClaim.Where(e => e.Id == data).SingleOrDefault();



                // var selectedTravelhotelbook = TadasettleclaimReq.Travel_Hotel_Booking;

                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {


                    try
                    {
                        //  db.TravelHotelBooking.RemoveRange(selectedTravelhotelbook);
                        db.TADASettlementClaim.Remove(TadasettleclaimReq);

                        //   db.Entry(LoanAdvReq).State = System.Data.Entity.EntityState.Deleted;


                        await db.SaveChangesAsync();


                        ts.Complete();
                        return this.Json(new { status = true, responseText = "Data removed.", JsonRequestBehavior.AllowGet });

                    }
                    catch (RetryLimitExceededException /* dex */)
                    {
                        //Log the error (uncomment dex variable name and add a line here to write a log.)
                        //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                        //return RedirectToAction("Delete");
                        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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
                        List<string> Msg = new List<string>();
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
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
                    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                    string tadaadvanceclaimlist = form["TADAAdvanceClaimList"] == "0" ? "" : form["TADAAdvanceClaimList"];
                    string tadatypelist = form["TADATypelist"] == "0" ? "" : form["TADATypelist"];
                    // string lvreqlist = form["LvReqList"] == "0" ? "" : form["LvReqList"];
                    string journewdetailslist = form["JourneyDetailsList"] == "0" ? "" : form["JourneyDetailsList"];
                    string trhotelbookinglist = form["Travel_Hotel_BookingList"] == "0" ? "" : form["Travel_Hotel_BookingList"];
                    //  string LtcBlock = form["BlockId"] == "0" ? "" : form["BlockId"];


                    //string Incharge_DDL = form["Incharge_DDL"] == "" ? null : form["Incharge_DDL"];
                    string Incharge_DDL = form["Incharge_Id"] == "" ? null : form["Incharge_Id"];




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



                    if (journewdetailslist != null)
                    {
                        if (journewdetailslist != "")
                        {
                            int ids = Convert.ToInt32(journewdetailslist);
                            var val = db.JourneyDetails.Where(e => e.Id == ids).SingleOrDefault();
                            c.JourneyDetails = val;
                        }
                    }

                    var EmpLTCStruct_Details = db.EmployeePayroll
                            .Include(e => e.TADASettlementClaim)
                             .Include(e => e.TADASettlementClaim.Select(x => x.JourneyDetails))
                           .Where(e => e.Employee.Id == Emp).FirstOrDefault();

                    var travelmodlist = EmpLTCStruct_Details.TADASettlementClaim.Select(r => r.JourneyDetails)
                        .ToList();


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

                    //if (lvreqlist != null)
                    //{
                    //    if (lvreqlist != "")
                    //    {
                    //        int ids = Convert.ToInt32(lvreqlist);
                    //        var val = db.LvNewReq.Where(e => e.Id == ids).SingleOrDefault();
                    //        c.LvNewReq = val;
                    //    }
                    //}

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

                    //if (c.SettlementAmt > c.TA_Eligible_Amt)
                    //{
                    //    Msg.Add("Settlement amount Should not be greater than Eligible amount.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    //if (c.TA_SanctionAmt > c.TA_Eligible_Amt)
                    //{
                    //    Msg.Add("Sanction amount Should not be greater than Eligible amount.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    //if (c.SettlementAmt > c.TA_SanctionAmt)
                    //{
                    //    Msg.Add("Settlement amount Should not be greater than sanction amount.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    // yearlypayment

                    string TADATypelist = form["TADATypelist"] == "0" ? "" : form["TADATypelist"];
                    string salcode = "";
                    //if (TADATypelist != null && TADATypelist != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(TADATypelist));
                    //    if (val.LookupValData != null && val.LookupValData != "")
                    //    {
                    //        salcode = val.LookupValData.ToString();
                    //    }
                    //}

                    //if (salcode == "")
                    //{
                    //    Msg.Add("Please bind salary head in Tada type.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    var emppi = db.EmployeePayroll.Where(e => e.Employee.Id == Emp).SingleOrDefault();
                    int salhdid = 0;
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
                    //       .Where(s => s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && s.SalaryHead.Code.ToUpper() == salcode.ToUpper()
                    //    && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BMSTADA"
                    //    )).FirstOrDefault() != null ? OEmpSalStruct.EmpSalStructDetails
                    //       .Where(s => s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && s.SalaryHead.Code.ToUpper() == salcode.ToUpper()
                    //    && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BMSTADA"
                    //    )).FirstOrDefault().SalaryHead.Id : 0;
                    //}


                    //if (salhdid == 0)
                    //{
                    //    Msg.Add("Please Define BMSTADA in salary Structure.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    FunctAllWFDetails oAttWFDetails = new FunctAllWFDetails
                    {
                        WFStatus = 5,
                        Comments = c.Remark.ToString(),
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
                                NoOfDays = c.NoOfDays,
                                TADAType = c.TADAType,
                                AdvAmt = c.AdvAmt,
                                TA_Eligible_Amt = c.TA_Eligible_Amt,
                                TA_Claim_Amt = c.TA_Claim_Amt,
                                TA_SanctionAmt = c.TA_SanctionAmt,
                                DA_Eligible_Amt = c.DA_Eligible_Amt,
                                DA_Claim_Amt = c.DA_Claim_Amt,
                                DA_SanctionAmt = c.DA_SanctionAmt,
                                Hotel_Eligible_Amt = c.Hotel_Eligible_Amt,
                                Hotel_Claim_Amt = c.Hotel_Claim_Amt,
                                Hotel_SanctionAmt = c.Hotel_SanctionAmt,
                                MisExpense_Eligible_Amt = c.MisExpense_Eligible_Amt,
                                MisExpense_Claim_Amt = c.MisExpense_Claim_Amt,
                                MisExpense_SanctionAmt = c.MisExpense_SanctionAmt,
                                SettlementAmt = c.SettlementAmt,
                                Remark = c.Remark,
                                FuncStruct = c.FuncStruct,
                                GeoStruct = c.GeoStruct,
                                PayStruct = c.PayStruct,
                                WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                                LTCWFDetails = oAttWFDetails_List,
                                TrClosed=true,
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

                            List<TADASettlementClaim> TadaSettlementlist = new List<TADASettlementClaim>();
                            var aa = db.EmployeePayroll.Include(e => e.TADASettlementClaim).Include(e => e.YearlyPaymentT).Include(e => e.Employee).Where(e => e.Employee.Id == Emp).SingleOrDefault();
                            if (aa.TADASettlementClaim.Count() > 0)
                            {
                                TadaSettlementlist.AddRange(aa.TADASettlementClaim);
                            }
                            
                                TadaSettlementlist.Add(tadasettlement);
                            
                            // yearly payment strat
                            //if (aa.YearlyPaymentT.Count() > 0)
                            //{
                            //    OYrlyPaylist.AddRange(aa.YearlyPaymentT);
                            //}
                            // yearly payment strat

                            aa.TADASettlementClaim = TadaSettlementlist;
                            // yearly payment strat
                            //aa.YearlyPaymentT = OYrlyPaylist;
                            // yearly payment strat
                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        public ActionResult GetTADAAmount(FormCollection form, int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string journewdetailslist = form["JourneyDetailsList"] == "0" ? "" : form["JourneyDetailsList"];
                    string tadaadvanceclaimlist = form["TADAAdvanceClaimList"] == "0" ? "" : form["TADAAdvanceClaimList"];
                    string travelhotelbookinglist = form["Travel_Hotel_BookingList"] == "0" ? "" : form["Travel_Hotel_BookingList"];
                    string TADATypelist = form["TADATypelist"] == "0" ? "" : form["TADATypelist"];
                    string salcode = "";
                    if (TADATypelist != null && TADATypelist != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(TADATypelist));
                        if (val.LookupValData != null && val.LookupValData != "")
                        {
                            salcode = val.LookupValData.ToString();
                        }

                    }
                    double advanceclaimamt = 0;
                    double DAElgAmt = 0;
                    double DAClaimAmt = 0;
                    double DASetAmt = 0;
                    double TAElgAmt = 0;
                    double TAClaimAmt = 0;
                    double TASetAmt = 0;
                    double Hotelelgamt = 0;
                    double HotelClaimamt = 0;
                    double HotelSetamt = 0;
                    double MisElgAmt = 0;
                    double MisClaimAmt = 0;
                    double MisSetAmt = 0;
                    double Noofdays = 0;
                    //double TAclaimamt = 0;
                    //double TAElgamt = 0;
                    //double TASettamt = 0;
                    //double Hoteltotalamt = 0;
                    //double advanceclaimamt = 0;
                    //double hotelamount = 0;
                    //double hotelelgamount = 0;
                    //double foodeligibleamt = 0;
                    //double TADASettleamt = 0;
                    //double Lodging_Eligible_Amt_PerDay = 0;
                    //double TadaEligibleamt = 0;
                    //double Noofdays = 0;
                    //double EncashmentAmt = 0;
                    //double TotalMinute=0;

                    //var QEmpSalStruct1 = db.EmpSalStruct//.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.EffectiveDate >= comparedate)
                    //                       .Include(e => e.EmpSalStructDetails)
                    //                       .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                    //                       .Include(e => e.EmpSalStructDetails.Select(q => q.SalHeadFormula))
                    //                       .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency))
                    //                       .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Type))
                    //                       .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod))
                    //                       .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType))
                    //                       .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.LvHead));//.ToList();
                    //var EmpSalStructTotal = QEmpSalStruct1.Where(e => e.EmployeePayroll.Id == data && e.EndDate == null)
                    //                              .ToList();
                    //var OEmpSalStruct = EmpSalStructTotal.Where(e => e.EndDate == null).SingleOrDefault();
                    //if (OEmpSalStruct != null)
                    //{

                    //    TadaEligibleamt = OEmpSalStruct.EmpSalStructDetails
                    //       .Where(s => s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && s.SalaryHead.Code.ToUpper() == salcode.ToUpper()
                    //    && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BMSTADA"

                    //    )).FirstOrDefault().Amount;

                    //    // For TADA Salheadformula Define Not Require As Discuss with Sir

                    //}

                    if (tadaadvanceclaimlist != null && tadaadvanceclaimlist != "")
                    {
                        int tadaadvanceid = Convert.ToInt32(tadaadvanceclaimlist);
                        var advanceclaimdata = db.TADAAdvanceClaim.Where(e => e.Id == tadaadvanceid).SingleOrDefault();
                        if (advanceclaimdata != null)
                        {
                            advanceclaimamt = advanceclaimdata.AdvAmt;
                        }
                        // c.LTCAdvanceClaim = value;

                    }
                    //if (travelhotelbookinglist != null)
                    //{
                    //    var ids = Utility.StringIdsToListIds(travelhotelbookinglist);
                    //    var travelhotelbookinglisttada = new List<TravelHotelBooking>();
                    //    var travelhotelbookinlist = db.TravelHotelBooking.Include(e => e.HotelEligibilityPolicy).Where(e => ids.Contains(e.Id)).ToList();
                    //    if (travelhotelbookinlist.Count() > 0)
                    //    {
                    //        var hoteleligibilitypolicy = travelhotelbookinlist.Where(e => e.HotelEligibilityPolicy != null).Select(e => e.HotelEligibilityPolicy);
                    //        hotelamount = travelhotelbookinlist.Sum(t => t.BillAmount);
                    //        hotelelgamount = travelhotelbookinlist.Sum(t => t.Elligible_BillAmount);

                    //        //if (hoteleligibilitypolicy != null)
                    //        //{
                    //        //    foodeligibleamt = hoteleligibilitypolicy.Sum(e => e.Food_Eligible_Amt_PerDay);
                    //        //    //Lodging_Eligible_Amt_PerDay = hoteleligibilitypolicy.Sum(e => e.Lodging_Eligible_Amt_PerDay);
                    //        //}
                    //    }
                    //   // Hoteltotalamt = hotelamount + foodeligibleamt + Lodging_Eligible_Amt_PerDay;

                    //}



                    if (journewdetailslist != null)
                    {
                        if (journewdetailslist != "")
                        {
                            int ids = Convert.ToInt32(journewdetailslist);
                            var JourneyObjdata = db.JourneyDetails.Include(e => e.JourneyObject).Where(e => e.Id == ids).SingleOrDefault();
                            Noofdays = (JourneyObjdata.JourneyEnd.Value.Date - JourneyObjdata.JourneyStart.Value.Date).Days + 1;

                            TAElgAmt = JourneyObjdata.TAElligibleAmt;
                            TAClaimAmt = JourneyObjdata.TAClaimAmt;
                            TASetAmt = JourneyObjdata.TASettleAmt;

                            Hotelelgamt = JourneyObjdata.HotelElligibleAmt;
                            HotelClaimamt = JourneyObjdata.HotelClaimAmt;
                            HotelSetamt = JourneyObjdata.HotelSettleAmt;

                            DAElgAmt = JourneyObjdata.DAElligibleAmt;
                            DAClaimAmt = JourneyObjdata.DAClaimAmt;
                            DASetAmt = JourneyObjdata.DASettleAmt;

                            MisElgAmt = JourneyObjdata.MisExpenseElligibleAmt;
                            MisClaimAmt = JourneyObjdata.MisExpenseClaimAmt;
                            MisSetAmt = JourneyObjdata.MisExpenseSettleAmt;


                        }
                    }

                    // TADASettleamt = Hoteltotalamt + TAclaimamt;
                    List<GetTadasettlamt> returndata = new List<GetTadasettlamt>();

                    returndata.Add(new GetTadasettlamt
                    {
                        AdvanceAmt = advanceclaimamt,
                        TAElgmamt = TAElgAmt,
                        TAClaim = TAClaimAmt,
                        TASanctionAmt = TASetAmt,

                        DAElgmamt = DAElgAmt,
                        DAClaim = DAClaimAmt,
                        DASanctionAmt = DASetAmt,

                        HotelElgmamt = Hotelelgamt,
                        HotelClaim = HotelClaimamt,
                        HotelSanctionAmt = HotelSetamt,

                        MisexpElgmamt = MisElgAmt,
                        MisexpClaim = MisClaimAmt,
                        MisexpSanctionAmt = MisSetAmt,
                        Noofdays = Noofdays,
                        SettlementAmt = (TASetAmt + DASetAmt + HotelSetamt + MisSetAmt) - advanceclaimamt,

                        // TADAClaimamt=hotelamount+TAclaimamt,//travel hotel bill amount+journey TAclaim amt
                        // TADAElgmamt = hotelelgamount + TAElgamt,// travel hotel elg amount+ journey elg amount
                        //// SanctionAmt = (hotelelgamount + TAElgamt) <= (hotelamount + TAclaimamt) ? (hotelelgamount + TAElgamt) : (hotelamount + TAclaimamt),// elg amount and claim amount which ever is less
                        // SanctionAmt = TadaEligibleamt , // da formula amount
                        //SettlementAmt = ((hotelelgamount + TAElgamt) <= (hotelamount + TAclaimamt) ? (hotelelgamount + TAElgamt) : (hotelamount + TAclaimamt)) - advanceclaimamt,//sanction amount-advance amount

                    });
                    return Json(returndata, JsonRequestBehavior.AllowGet);

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
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

                //var employeedata = db.Employee
                //          .Include(e => e.FamilyDetails)
                //          .Include(e => e.FamilyDetails.Select(t => t.MemberName))
                //              .Where(e => e.Id == empids).ToList();
                var fall = employeedata.SelectMany(e => e.TADAAdvanceClaim).ToList();

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

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.EmployeePayroll.Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj)
                        //.Include(e => e.Employee.FuncStruct)
                        //   .Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct)
                        //  .Include(e => e.Employee.PayStruct.Grade)
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
                        // fall = all.Where(e => e.Employee.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.Employee.EmpCode.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                  || (e.Employee.EmpName.FullNameFML.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                            //   || (e.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM/yyyy").Contains(param.sSearch))
                            //   || (e.Employee.FuncStruct.Job.Name.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                            // || (e.Employee.PayStruct.Grade.Name.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                            //  || (e.Employee.GeoStruct.Location.LocationObj.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  ).ToList();
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
                                Code = item.Employee != null ? item.Employee.EmpCode : null,
                                Name = item.Employee != null && item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : null,
                                //    JoiningDate = item.Employee.ServiceBookDates.JoiningDate != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM/yyyy") : null,
                                //    Job = item.Employee.FuncStruct != null && item.Employee.FuncStruct.Job != null ? item.Employee.FuncStruct.Job.Name : null,
                                //   Grade = item.Employee.PayStruct != null && item.Employee.PayStruct.Grade != null ? item.Employee.PayStruct.Grade.Name : null,
                                Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null && item.Employee.GeoStruct.Location.LocationObj != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : null,
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
                    List<string> Msg = new List<string>();
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = e.Message,
                        ExceptionStackTrace = e.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }



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
    }
}