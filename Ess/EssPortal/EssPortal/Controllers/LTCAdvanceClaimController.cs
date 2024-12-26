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

namespace EssPortal.Controllers
{
    public class LTCAdvanceClaimController : Controller
    {
        //
        // GET: /LTCAdvanceClaim/
        public ActionResult Index()
        {
            return View("~/Views/LTCAdvanceClaim/Index.cshtml");
        }

        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/_LTCAdvClaimGridPartial.cshtml");
        }

        public ActionResult ltcadvanceclaimPartialSanction()
        {
            return View("~/Views/Shared/_LTCadvanceClaimReqOnSanction.cshtml");
        }



        public JsonResult GetEmpLTCBlock(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var a = db.PayScaleAgreement.Find(int.Parse(data));
                int Emp_Id = int.Parse(data);
                var a = db.EmployeePayroll.Include(e => e.EmpLTCBlock)
                    .Include(e => e.EmpLTCBlock.Select(y => y.EmpLTCBlockT))
                    .Where(e => e.Id == Emp_Id).AsNoTracking().SingleOrDefault().EmpLTCBlock.Where(e => e.IsBlockClose == false).FirstOrDefault();

                var b = a.EmpLTCBlockT.Where(e => e.IsBlockClose == false).OrderBy(e => e.BlockPeriodStart).FirstOrDefault();

                return Json(b, JsonRequestBehavior.AllowGet);
            }
        }




        public JsonResult GetEmpSalStruct(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var a = db.PayScaleAgreement.Find(int.Parse(data));
                int Emp_Id = int.Parse(data);

                var QEmpSalStruct1 = db.EmpSalStruct//.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.EffectiveDate >= comparedate)
                                            .Include(e => e.EmpSalStructDetails)
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalHeadFormula))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Type))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.LvHead));//.ToList();
                var EmpSalStructTotal = QEmpSalStruct1.Where(e => e.EmployeePayroll.Id == Emp_Id && e.EndDate == null)
                                              .ToList();
                var OEmpSalStruct = EmpSalStructTotal.Where(e => e.EndDate == null).SingleOrDefault();
                if (OEmpSalStruct != null)
                {

                    var b = OEmpSalStruct.EmpSalStructDetails
                        .Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY"
                     && s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                     && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LTC"

                     )).FirstOrDefault().Amount;
                    return Json(b, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

            }
        }


        public ActionResult GetLookupValue(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = (SelectList)null;
                var selected = "";
                if (data != "" && data != null)
                {
                    // var qurey = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == data).SingleOrDefault();
                    var qurey = db.Lookup.Where(e => e.Code == data).Select(e => e.LookupValues.Where(r => r.IsActive == true && r.LookupVal.ToUpper() == "AVAILMENT")).SingleOrDefault(); // added by rekha 26-12-16
                    if (data2 != "" && data2 != "0")
                    {
                        selected = data2;
                    }
                    if (qurey != null)
                    {
                        s = new SelectList(qurey, "Id", "LookupVal", selected);
                    }
                }
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }




        public class LTCAdvClaimChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string BlockPeriod { get; set; }
            public string ReqDate { get; set; }
            public string LTCType { get; set; }
            public double EligibleAmt { get; set; }
            public double AdvanceAmt { get; set; }
            public string Remark { get; set; }
            public string ProposedPlace { get; set; }
            public string ClaimSettleStatus { get; set; }
            public string TravelStartDate { get; set; }
            public string TravelEndDate { get; set; }
        }

        public ActionResult Get_LTCAdvClaim(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.EmpLTCBlock)
                        .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT))
                        .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim)))
                        .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim.Select(c => c.LTC_TYPE))))
                        .Where(e => e.Id == data).FirstOrDefault();
                    if (db_data.EmpLTCBlock != null)
                    {
                        List<LTCAdvClaimChildDataClass> returndata = new List<LTCAdvClaimChildDataClass>();
                        var LTCAdvClaimlist = db_data.EmpLTCBlock;
                        foreach (var item in db_data.EmpLTCBlock.OrderByDescending(e => e.Id))
                        {
                            foreach (var item1 in item.EmpLTCBlockT)
                            {
                                if (item1.LTCAdvanceClaim != null)
                                {
                                    foreach (var item2 in item1.LTCAdvanceClaim)
                                    {
                                        returndata.Add(new LTCAdvClaimChildDataClass
                                        {
                                            Id = item2.Id,
                                            BlockPeriod = item1.FullDetails,
                                            ReqDate = item2.DateOfAppl.Value != null ? item2.DateOfAppl.Value.ToShortDateString() : "",
                                            LTCType = item2.LTC_TYPE != null ? item2.LTC_TYPE.LookupVal.ToString() : null,
                                            EligibleAmt = item2.LTC_Eligible_Amt,
                                            AdvanceAmt = item2.LTCAdvAmt,
                                            Remark = item2.Remark,
                                            ProposedPlace = item2.ProposedPlace,
                                            ClaimSettleStatus = item2.IsClaimSettle == true ? "true" : "false",
                                            TravelStartDate = item2.TravelStartDate.Value != null ? item2.TravelStartDate.Value.ToShortDateString() : "",
                                            TravelEndDate = item2.TravelEndDate.Value != null ? item2.TravelEndDate.Value.ToShortDateString() : ""
                                        });
                                    }
                                }
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


        public class LvNewReqData
        {
            public int Id { get; set; }
            public string FullDetails { get; set; }

        }



        [HttpPost]
        public ActionResult GetLVReqLKDetails(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
                int block_Id = int.Parse(data2);
                int Count = 0;
                List<LvNewReqData> model = new List<LvNewReqData>();
                LvNewReqData view = null;

                var OEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq)
                                        .Include(e => e.LvCancelReq)
                                 .Include(e => e.LvCancelReq.Select(t => t.WFStatus))
                               .Include(e => e.LvCancelReq.Select(t => t.LvNewReq))
                    .Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                    .Include(e => e.LvNewReq.Select(r => r.WFStatus))
                    .Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
                  .Where(e => e.Employee.Id == Id).SingleOrDefault();

                var lvcancelreqidslist = OEmployeeLeave.LvCancelReq.Where(e => e.WFStatus.LookupVal == "0" || e.WFStatus.LookupVal == "1").Select(e => e.LvNewReq.Id).ToList();
                // var DefCal = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                //Cl max date
                List<int> lvcode = OEmployeeLeave.LvNewReq.Where(e => e.LvCreditDate != null).Select(e => e.LeaveHead.Id).Distinct().ToList();
                foreach (var item in lvcode)
                {
                    DateTime? lvcrdate = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == item && e.LvCreditDate != null).Max(e => e.LvCreditDate.Value);
                    if (lvcrdate != null)
                    {
                        EmpLTCBlockT OEmpLTCBlockT = db.EmpLTCBlockT.Find(block_Id);
                        DateTime? BlockStartchk = OEmpLTCBlockT.BlockPeriodStart;
                        DateTime? BlockEndchk = OEmpLTCBlockT.BlockPeriodEnd;
                        var OEmpLTCBlock = db.EmployeePayroll.Include(e => e.EmpLTCBlock).Where(e => e.Employee.Id == Id).SingleOrDefault();
                        var OEmpLTCBlockper = OEmpLTCBlock.EmpLTCBlock.Where(e => e.BlockStart >= BlockStartchk.Value && BlockEndchk.Value <= e.BlockEnd).FirstOrDefault();

                        if (OEmpLTCBlockper != null)
                        {
                            //DateTime? BlockStart = OEmpLTCBlockT.BlockPeriodStart;
                            //DateTime? BlockEnd = OEmpLTCBlockT.BlockPeriodEnd;

                            DateTime? BlockStart = OEmpLTCBlockper.BlockStart;
                            DateTime? BlockEnd = OEmpLTCBlockper.BlockEnd;

                            //var OEmpLeave = OEmployeeLeave.LvNewReq.Where(e => e.LeaveCalendar.Id == DefCal.Id && e.IsCancel == false && e.TrClosed == true && e.LvOrignal == null && e.WFStatus.LookupVal != "2").ToList();

                            //var query = OEmployeeLeave.LvNewReq.Where(e => e.LeaveCalendar.Id == DefCal.Id && e.IsCancel == true && e.TrClosed == true && e.LvOrignal != null).ToList();
                            var OEmpLeave = OEmployeeLeave.LvNewReq.Where(e => !lvcancelreqidslist.Contains(e.Id) && e.LeaveHead.Id == item && e.IsCancel == false && e.TrClosed == true && e.LvOrignal == null && e.WFStatus.LookupVal != "2" && e.ReqDate >= BlockStart.Value && e.ReqDate <= BlockEnd.Value).ToList();

                            var query = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == item && e.IsCancel == true && e.TrClosed == true && e.LvOrignal != null && e.ReqDate >= BlockStart.Value && e.ReqDate <= BlockEnd.Value).ToList();


                            foreach (var a in OEmpLeave)
                            {
                                //if (query.Count > 0)
                                //{
                                Count = 0;
                                foreach (var b in query)
                                {
                                    if (b.LvOrignal.Id == a.Id)
                                    {
                                        Count = 1;
                                    }
                                }
                                if (Count == 0)
                                {

                                    view = new LvNewReqData()
                                    {
                                        Id = a.Id,
                                        FullDetails = a.FullDetails
                                    };
                                    model.Add(view);
                                }
                                //}
                                //else
                                //{
                                //    view = new LvNewReqData()
                                //    {
                                //        Id = a.Id,
                                //        FullDetails = a.FullDetails
                                //    };
                                //    model.Add(view);
                                //}
                            }
                        }
                    }
                }

                var selected = "";
                if (data2 != null)
                {
                    selected = data2;
                }
                if (model != null && model.Count() > 0)
                {
                    SelectList s = new SelectList(model, "Id", "FullDetails", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }




        [HttpPost]
        public ActionResult Create(LTCAdvanceClaim LTCAdvClaim, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();

                try
                {
                    string LvReqList = form["LvReqList"] == "0" ? "" : form["LvReqList"];
                    int Emp = form["EmpLvnereq_Id"] == "0" ? 0 : Convert.ToInt32(form["EmpLvnereq_Id"]);
                    string LtcType = form["LTCTypelist"] == "0" ? "" : form["LTCTypelist"];
                    string LtcBlock = form["BlockId"] == "0" ? "" : form["BlockId"];
                    string Ltcadvamt = form["LTCAdvAmt"] == "0" ? "0" : form["LTCAdvAmt"];
                    string LtcSancamt = form["LTCSanctionedAmt"] == "0" ? "0" : form["LTCSanctionedAmt"];

                    if (LvReqList != null && LvReqList != "")
                    {
                        var val = db.LvNewReq.Find(int.Parse(LvReqList));
                        LTCAdvClaim.LvNewReq = val;
                    }
                    else
                    {
                        Msg.Add("  Please select leave requisition.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (LtcBlock == null && LtcBlock == "")
                    {
                        Msg.Add("  Please select LTC Block.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (Convert.ToDouble(LtcSancamt) > Convert.ToDouble(Ltcadvamt))
                    {
                        Msg.Add("  Sanction amount should not be greater than Ltc advance amount.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    if (LtcType != null && LtcType != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(LtcType));
                        LTCAdvClaim.LTC_TYPE = val;
                    }

                    int Blockid = int.Parse(LtcBlock);
                    EmpLTCBlockT OEmpLTCBlockT = db.EmpLTCBlockT.Find(Blockid);

                    var a = db.EmployeePayroll.Include(e => e.EmpLTCBlock).Include(e => e.Employee)
                .Include(e => e.EmpLTCBlock.Select(y => y.EmpLTCBlockT))
                .Where(e => e.Employee.Id == Emp).AsNoTracking().SingleOrDefault().EmpLTCBlock.Where(e => e.IsBlockClose == false).FirstOrDefault();

                    var test = a.EmpLTCBlockT.Where(e => e.BlockPeriodStart >= OEmpLTCBlockT.BlockPeriodStart
                       && e.BlockPeriodEnd <= OEmpLTCBlockT.BlockPeriodEnd).FirstOrDefault();


                    if (test.LTCAdvanceClaim != null)
                    {
                        Msg.Add("Record Already Exists.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }


                    if (LTCAdvClaim.TravelStartDate >= LTCAdvClaim.LvNewReq.FromDate && LTCAdvClaim.TravelStartDate <= LTCAdvClaim.LvNewReq.ToDate)
                    {

                    }
                    else
                    {
                        //Msg.Add("Travel start date should be between Leave requisition start date & Leave requisition end date.");
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    if (LTCAdvClaim.TravelEndDate >= LTCAdvClaim.LvNewReq.FromDate && LTCAdvClaim.TravelEndDate <= LTCAdvClaim.LvNewReq.ToDate)
                    {

                    }
                    else
                    {
                        //Msg.Add("Travel End date should be between Leave requisition start date & Leave requisition end date.");
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }


                    FunctAllWFDetails oAttWFDetails = new FunctAllWFDetails
                    {
                        WFStatus = 0,
                        Comments = LTCAdvClaim.Remark.ToString(),
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    };

                    List<FunctAllWFDetails> oAttWFDetails_List = new List<FunctAllWFDetails>();
                    oAttWFDetails_List.Add(oAttWFDetails);



                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            LTCAdvClaim.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };


                            LTCAdvanceClaim LTCAdvanceClaim = new LTCAdvanceClaim()
                            {
                                DateOfAppl = LTCAdvClaim.DateOfAppl,
                                IsClaimSettle = LTCAdvClaim.IsClaimSettle,
                                LTC_Eligible_Amt = LTCAdvClaim.LTC_Eligible_Amt,
                                LTC_TYPE = LTCAdvClaim.LTC_TYPE,
                                LTCAdvAmt = LTCAdvClaim.LTCAdvAmt,
                                LvNewReq = LTCAdvClaim.LvNewReq,
                                ProposedPlace = LTCAdvClaim.ProposedPlace,
                                Remark = LTCAdvClaim.Remark,
                                DBTrack = LTCAdvClaim.DBTrack,
                                TravelStartDate = LTCAdvClaim.TravelStartDate,
                                TravelEndDate = LTCAdvClaim.TravelEndDate,
                                LTCSanctionedAmt = LTCAdvClaim.LTCSanctionedAmt,
                                LTCWFDetails = oAttWFDetails_List
                            };
                            try
                            {
                                db.LTCAdvanceClaim.Add(LTCAdvanceClaim);
                                db.SaveChanges();


                                List<LTCAdvanceClaim> LTCAdvanceClaimlist = new List<LTCAdvanceClaim>();
                                var aa = db.EmpLTCBlockT.Include(e => e.LTCAdvanceClaim).Where(e => e.Id == test.Id).SingleOrDefault();
                                if (aa.LTCAdvanceClaim.Count() > 0)
                                {
                                    LTCAdvanceClaimlist.AddRange(aa.LTCAdvanceClaim);
                                }
                                else
                                {
                                    LTCAdvanceClaimlist.Add(LTCAdvanceClaim);
                                }
                                aa.LTCAdvanceClaim = LTCAdvanceClaimlist;
                                db.EmpLTCBlockT.Attach(aa);
                                db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                ts.Complete();
                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                List<string> Msgs = new List<string>();
                                //Msgs.Add("Data Saved successfully");
                                //return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                                return Json(new Utility.JsonClass { status = true, responseText = " Data Saved successfully " }, JsonRequestBehavior.AllowGet);
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = LTCAdvanceClaim.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                                //    List<string> Msg = new List<string>();
                                Msg.Add(ex.Message);
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
                        //var errorMsg = sb.ToString();
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        List<string> MsgB = new List<string>();
                        var errorMsg = sb.ToString();
                        MsgB.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);
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



        public class ChildGetLTCAdvanceClaimReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }

        }



        public class GetLTCAdvanceClaimReqClass
        {
            public string Emp { get; set; }
            public string ReqDate { get; set; }
            public string LTCType { get; set; }
            public string TravelStartDate { get; set; }
            public string TravelEndDate { get; set; }
            public string ProposedPlace { get; set; }
            public string AdvanceAmount { get; set; }
            public string Status { get; set; }


            public ChildGetLTCAdvanceClaimReqClass RowData { get; set; }
        }



        public ActionResult GetMyLTCAdvanceClaimReq()   /// Get Created Data on Grid for Myself
        {
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    string authority = Convert.ToString(Session["auho"]);

                    List<GetLTCAdvanceClaimReqClass> OLTCAdvanceClaimlist = new List<GetLTCAdvanceClaimReqClass>();
                    if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                    {
                        return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                    }
                    var Id = Convert.ToInt32(SessionManager.EmpLvId);




                    var db_data = db.EmployeePayroll
                            .Include(e => e.EmpLTCBlock)
                            .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT))
                            .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim)))
                            .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim.Select(c => c.LTC_TYPE))))
                            .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim.Select(c => c.LTCWFDetails))))
                            .Where(e => e.Id == Id).FirstOrDefault();




                    if (db_data.EmpLTCBlock != null)
                    {
                        List<GetLTCAdvanceClaimReqClass> returndata = new List<GetLTCAdvanceClaimReqClass>();
                        returndata.Add(new GetLTCAdvanceClaimReqClass
                        {

                            ReqDate = "Requisition Date",
                            LTCType = "LTCType",
                            TravelStartDate = "Travel StartDate",
                            TravelEndDate = "Travel EndDate",
                            ProposedPlace = "ProposedPlace",
                            AdvanceAmount = "AdvanceAmount",
                            Status = "Status"
                        });


                        foreach (var item in db_data.EmpLTCBlock)
                        {
                            var LTCAdvancelist = item.EmpLTCBlockT.ToList();

                            if (LTCAdvancelist.Count() > 0)
                            {
                                var LTCAdvanceclaimlist = LTCAdvancelist.Select(d => d.LTCAdvanceClaim).ToList();
                                foreach (var LTCAdvitems in LTCAdvanceclaimlist)
                                {
                                    if (LTCAdvitems.Count() > 0)
                                    {
                                        foreach (var LTCWitems in LTCAdvitems)
                                        {
                                            //var LTCWFDetailitems = LTCWitems.LTCWFDetails.ToList();
                                            //foreach (var items in LTCWFDetailitems)
                                            //{
                                            int WfStatusNew = LTCWitems.LTCWFDetails.Select(e => e.WFStatus).LastOrDefault();
                                            string Comments = LTCWitems.LTCWFDetails.Select(e => e.Comments).LastOrDefault();


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



                                            if (authority.ToUpper() == "SANCTION" && WfStatusNew == 0)
                                            {
                                                GetLTCAdvanceClaimReqClass ObjLTCAdvanceClaimRequest = new GetLTCAdvanceClaimReqClass()
                                                {
                                                    RowData = new ChildGetLTCAdvanceClaimReqClass
                                                    {
                                                        LvNewReq = LTCWitems.LvNewReq_Id.ToString(),
                                                        EmpLVid = db_data.Id.ToString()

                                                    },
                                                    ProposedPlace = LTCWitems.ProposedPlace.ToString(),
                                                    LTCType = LTCWitems.LTC_TYPE.LookupVal.ToString(),
                                                    TravelStartDate = LTCWitems.TravelStartDate.Value.ToShortDateString(),
                                                    TravelEndDate = LTCWitems.TravelEndDate.Value.ToShortDateString(),
                                                    ReqDate = LTCWitems.DateOfAppl.Value.ToShortDateString(),
                                                    AdvanceAmount = LTCWitems.LTCAdvAmt.ToString() != "0" ? LTCWitems.LTCAdvAmt.ToString() : "0",
                                                    Status = StatusNarration
                                                };
                                                returndata.Add(ObjLTCAdvanceClaimRequest);
                                            }

                                            else if (authority.ToUpper() == "APPROVAL" && WfStatusNew == 1)
                                            {
                                                GetLTCAdvanceClaimReqClass ObjLTCAdvanceClaimRequest = new GetLTCAdvanceClaimReqClass()
                                                {
                                                    RowData = new ChildGetLTCAdvanceClaimReqClass
                                                    {
                                                        LvNewReq = LTCWitems.LvNewReq_Id.ToString(),
                                                        EmpLVid = db_data.Id.ToString()

                                                    },
                                                    ProposedPlace = LTCWitems.ProposedPlace.ToString(),
                                                    LTCType = LTCWitems.LTC_TYPE.LookupVal.ToString(),
                                                    TravelStartDate = LTCWitems.TravelStartDate.Value.ToShortDateString(),
                                                    TravelEndDate = LTCWitems.TravelEndDate.Value.ToShortDateString(),
                                                    ReqDate = LTCWitems.DateOfAppl.Value.ToShortDateString(),
                                                    AdvanceAmount = LTCWitems.LTCAdvAmt.ToString() != "0" ? LTCWitems.LTCAdvAmt.ToString() : "0",
                                                    Status = StatusNarration
                                                };
                                                returndata.Add(ObjLTCAdvanceClaimRequest);
                                            }
                                            else if (authority.ToUpper() == "MYSELF")
                                            {
                                                GetLTCAdvanceClaimReqClass ObjLTCAdvanceClaimRequest = new GetLTCAdvanceClaimReqClass()
                                                {
                                                    RowData = new ChildGetLTCAdvanceClaimReqClass
                                                    {
                                                        LvNewReq = LTCWitems.LvNewReq_Id.ToString(),
                                                        EmpLVid = db_data.Id.ToString()

                                                    },
                                                    ProposedPlace = LTCWitems.ProposedPlace.ToString(),
                                                    LTCType = LTCWitems.LTC_TYPE.LookupVal.ToString(),
                                                    TravelStartDate = LTCWitems.TravelStartDate.Value.ToShortDateString(),
                                                    TravelEndDate = LTCWitems.TravelEndDate.Value.ToShortDateString(),
                                                    ReqDate = LTCWitems.DateOfAppl.Value.ToShortDateString(),
                                                    AdvanceAmount = LTCWitems.LTCAdvAmt.ToString() != "0" ? LTCWitems.LTCAdvAmt.ToString() : "0",
                                                    Status = StatusNarration
                                                };
                                                returndata.Add(ObjLTCAdvanceClaimRequest);
                                            }

                                            //  }
                                        }
                                    }
                                }

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
            catch (Exception Ex)
            {
                throw Ex;
            }

        }


        public class ChildGetLTCadvReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }
            public string OdReqId { get; set; }

        }

        public class GetLTCadvClass1
        {

            public string ReqDate { get; set; }
            public string LTCType { get; set; }
            public string TravelStartDate { get; set; }
            public string TravelEndDate { get; set; }
            public string ProposedPlace { get; set; }
            public string AdvanceAmount { get; set; }

            public ChildGetLTCadvReqClass RowData { get; set; }
        }


        #region Get Employee LTCadvanceClaim request On Sanction Dropdown

        public ActionResult GetLTCadvanceClaimReqOnSanction(FormCollection form)
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
                //var returnDataClass = new List<returnDataClass>();

                List<GetLTCadvClass1> returndata = new List<GetLTCadvClass1>();
                returndata.Add(new GetLTCadvClass1
                {
                    ReqDate = "Requisition Date",
                    LTCType = "LTCType",
                    TravelStartDate = "Travel StartDate",
                    TravelEndDate = "Travel EndDate",
                    ProposedPlace = "ProposedPlace",
                    AdvanceAmount = "AdvanceAmount",

                    RowData = new ChildGetLTCadvReqClass
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
                        .Include(e => e.Employee.ReportingStructRights)
                        .Include(e => e.Employee.ReportingStructRights.Select(b => b.AccessRights.ActionName))
                        .Include(e => e.Employee.ReportingStructRights.Select(b => b.FuncModules))
                        .Include(e => e.EmpLTCBlock)
                        .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT))
                        .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim)))
                        .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim.Select(c => c.LTC_TYPE))))
                        .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim.Select(c => c.LTCWFDetails))))
                        .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim.Select(c => c.WFStatus))))
                        .ToList();




                    foreach (var item in Emps)
                    {
                        foreach (var ltcblockitem in item.EmpLTCBlock)
                        {
                            foreach (var ltcblockTitem in ltcblockitem.EmpLTCBlockT)
                            {

                                if (ltcblockTitem.LTCAdvanceClaim != null && ltcblockTitem.LTCAdvanceClaim.Count() > 0)
                                {
                                    var LvIds = UserManager.FilterLTCadvanceCLAIM(ltcblockTitem.LTCAdvanceClaim.OrderByDescending(e => e.DateOfAppl.Value.ToShortDateString()).ToList(),
                                       Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId));

                                    if (LvIds.Count() > 0)
                                    {
                                        var singleltcadvancereqdata = ltcblockTitem.LTCAdvanceClaim.Where(e => LvIds.Contains(e.Id)).ToList();
                                        foreach (var singleltcadvDetails in singleltcadvancereqdata)
                                        {
                                            if (singleltcadvDetails.LTCWFDetails != null)
                                            {
                                                //int QId = singleODDetails.Id;
                                                // var procd = db.ProcessedData.Include(e => e.MusterRemarks).Where(e => e.Id == singleODDetails.ProcessedData.Id).SingleOrDefault();
                                                var session = Session["auho"].ToString().ToUpper();
                                                var EmpR = item.Employee.ReportingStructRights.Where(e => e.AccessRights != null && e.AccessRights.ActionName.LookupVal.ToUpper() == session && e.FuncModules.LookupVal.ToUpper() == item1.ModuleName.ToUpper())
                                                .Select(e => e.AccessRights.IsClose).FirstOrDefault();
                                                //if (stts.WFStatus == 0)
                                                //{

                                                returndata.Add(new GetLTCadvClass1
                                                {
                                                    RowData = new ChildGetLTCadvReqClass
                                                    {
                                                        LvNewReq = singleltcadvDetails.Id.ToString(),
                                                        EmpLVid = item.Id.ToString(),
                                                        IsClose = EmpR.ToString(),
                                                        LvHead_Id = "",
                                                    },

                                                    LTCType = singleltcadvDetails.LTC_TYPE.LookupVal,
                                                    TravelStartDate = singleltcadvDetails.TravelStartDate.Value.ToShortDateString(),
                                                    TravelEndDate = singleltcadvDetails.TravelEndDate.Value.ToShortDateString(),
                                                    ReqDate = singleltcadvDetails.DateOfAppl.Value.ToShortDateString(),
                                                    ProposedPlace = singleltcadvDetails.ProposedPlace,
                                                    AdvanceAmount = singleltcadvDetails.LTCAdvAmt != 0 ? singleltcadvDetails.LTCAdvAmt.ToString() : "0",


                                                });
                                                //  }

                                            }
                                        }
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

        #endregion

        public class EmpLTCadvclaimREquestdata
        {

            public string Status { get; set; }
            public string Emplvhead { get; set; }
            public string LTCBlock { get; set; }

            public int EmpLTCBlockId { get; set; }
            public int LTCType_ID { get; set; }
            public int LeaveRequisition_ID { get; set; }

            public string Travel_Start_Date { get; set; }
            public string Travel_End_Date { get; set; }
            public string Request_Date { get; set; }
            public string ProposedPlace { get; set; }
            public string Familymembername { get; set; }
            public string Narration { get; set; }
            public string SanctionCode { get; set; }
            public string SanctionEmpname { get; set; }
            public string RecomendationCode { get; set; }
            public string RecomendationEmpname { get; set; }
            public double LTCAdvAmt { get; set; }
            public double LTC_Eligible_Amt { get; set; }
            public double LTCSanctionedAmt { get; set; }
            public string Remark { get; set; }
           
            public bool TrClosed { get; set; }
            public bool TrReject { get; set; }
            public string SanctionComment { get; set; }
            public string ApporavalComment { get; set; }
            public string Wf { get; set; }
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

        #region Get Employee LTCadvanceClaim request On Sanction Bind DATA

        public ActionResult GetLTCadvanceClaimData(string data)
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
                //var LvHeadId = ids.Count > 0 ? ids[3] : null;

                //var lvheadidint = Convert.ToInt32(LvHeadId);
                var EmpLvIdint = Convert.ToInt32(emplvId);

                var W = db.EmployeePayroll
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.EmpLTCBlock)
                        .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT))
                        .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim)))
                        .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim.Select(c => c.LTC_TYPE))))
                        .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim.Select(c => c.LTCWFDetails))))
                        .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim.Select(c => c.WFStatus))))
                        .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim.Select(c => c.LvNewReq))))

                    .Where(e => e.Employee.Id == EmpLvIdint).ToList();

                List<LTCAdvanceClaim> vv = new List<LTCAdvanceClaim>();
                foreach (var ltcblockitem in W)
                {
                    foreach (var ltcBlockTitem in ltcblockitem.EmpLTCBlock)
                    {
                        foreach (var ltcadvClaimitem in ltcBlockTitem.EmpLTCBlockT)
                        {
                            if (ltcadvClaimitem.LTCAdvanceClaim.Count() > 0)
                            {
                                
                           
                            var v = ltcadvClaimitem.LTCAdvanceClaim.Where(e => e.Id == id).Select(s => new EmpLTCadvclaimREquestdata
                            {
                                EmployeeId = ltcblockitem.Employee.Id,
                                EmployeeName = ltcblockitem.Employee.EmpCode + " " + ltcblockitem.Employee.EmpName.FullNameFML,
                                Lvnewreq = s.Id,
                                Empcode = ltcblockitem.Employee.EmpCode,

                                EmpLTCBlockId = ltcadvClaimitem.Id,
                                LTCType_ID = s.LTC_TYPE.Id,
                                LeaveRequisition_ID = s.LvNewReq.Id,


                                Travel_Start_Date = s.TravelStartDate != null ? s.TravelStartDate.Value.ToShortDateString() : null,
                                Travel_End_Date = s.TravelEndDate != null ? s.TravelEndDate.Value.ToShortDateString() : null,
                                Request_Date = s.DateOfAppl != null ? s.DateOfAppl.Value.ToShortDateString() : null,
                                ProposedPlace = s.ProposedPlace.ToString(),
                                //Status = status,

                                LTC_Eligible_Amt = s.LTC_Eligible_Amt != 0 ? s.LTC_Eligible_Amt : 0,
                                LTCSanctionedAmt = s.LTCSanctionedAmt != 0 ? s.LTCSanctionedAmt : 0,
                                Remark = s.Remark.ToString(),

                                Isclose = status.ToString(),
                                //Id = s.Id,
                                TrClosed = s.TrClosed,
                                SanctionCode = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
                                SanctionComment = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => SanctionStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                                ApporavalComment = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => ApporvalStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                                Wf = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).LastOrDefault() : null,
                                RecomendationCode = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => RecomendationStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
                                RecomendationEmpname = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => RecomendationStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                                // Incharge = s.Incharge != null ? s.Incharge.EmpCode + ' ' + s.Incharge.EmpName.FullDetails.ToString() : null
                            }).ToList();
                            TempData["LTCdata"] = v;

                            }
                            
                           // return Json(v, JsonRequestBehavior.AllowGet);

                        }

                    }
                }
                var vvv = TempData["LTCdata"];
                return Json(vvv, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion



        public ActionResult Update_LTCadvanceClaimReq(LTCAdvanceClaim LtcadvReq, FormCollection form, String data)
        {

            string authority = form["authority"] == null ? null : form["authority"];
            var isClose = form["isClose"] == null ? null : form["isClose"];
            if (authority == null && isClose == null)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
            var ids = Utility.StringIdsToListString(data);
            var LTCadvnewreqid = Convert.ToInt32(ids[0]);
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

                var query = db.EmployeePayroll.Where(e => e.Id == EmpPayrollId)
                    .Include(e => e.EmpLTCBlock)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT))
                    .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim)))
                    .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim.Select(c => c.LTC_TYPE))))
                    .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim.Select(c => c.LTCWFDetails))))
                    .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim.Select(c => c.WFStatus))))
                    .Include(e => e.EmpLTCBlock.Select(a => a.EmpLTCBlockT.Select(b => b.LTCAdvanceClaim.Select(c => c.LvNewReq))))
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
                        foreach (var ltcblockitem in query)
                        {
                            foreach (var ltcBlockTitem in ltcblockitem.EmpLTCBlock)
                            {
                                foreach (var ltcadvClaimitem in ltcBlockTitem.EmpLTCBlockT)
                                {
                                    if (ltcadvClaimitem.LTCAdvanceClaim.Count() > 0)
                                    {

                                        var LTCadvclaimList = ltcadvClaimitem.LTCAdvanceClaim.Where(e => e.Id == LTCadvnewreqid).ToList();
                                        //if someone reject lv
                                        foreach (var LTCADVitems in LTCadvclaimList)
                                        {
                                            List<FunctAllWFDetails> oFunctWFDetails_List = new List<FunctAllWFDetails>();
                                            FunctAllWFDetails objLTCWFDetails = new FunctAllWFDetails();
                                            if (authority.ToUpper() == "MYSELF")
                                            {
                                                TrClosed = false;
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

                                                    objLTCWFDetails = new FunctAllWFDetails
                                                    {
                                                        WFStatus = 1,
                                                        Comments = ReasonSanction,
                                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                                    };

                                                    oFunctWFDetails_List.Add(objLTCWFDetails);

                                                    LTCADVitems.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                                                    LTCADVitems.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault();


                                                }
                                                else if (Convert.ToBoolean(Sanction) == false)
                                                {

                                                    objLTCWFDetails = new FunctAllWFDetails
                                                    {
                                                        WFStatus = 2,
                                                        Comments = ReasonSanction,
                                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                                    };
                                                    oFunctWFDetails_List.Add(objLTCWFDetails);


                                                    SanctionRejected = true;
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
                                                    objLTCWFDetails = new FunctAllWFDetails
                                                    {
                                                        WFStatus = 3,
                                                        Comments = ReasonApproval,
                                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                                    };
                                                    oFunctWFDetails_List.Add(objLTCWFDetails);

                                                    LTCADVitems.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                                                    LTCADVitems.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault();
                                                    //qurey.BA_WorkFlow.Add(AppWFDetails);
                                                    //  qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                                                }
                                                else if (Convert.ToBoolean(Approval) == false)
                                                {
                                                    //approval no-4
                                                    //var LvWFDetails = new LvWFDetails
                                                    //{
                                                    //    WFStatus = 4,
                                                    //    Comments = ReasonApproval,
                                                    //    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                                    //};
                                                    //qurey.LvWFDetails.Add(LvWFDetails);

                                                    //qurey.LvWFDetails.Add(LvWFDetails);
                                                    //var CheckAllreadySanction = BAAppTarget.Where(e => e.BA_WorkFlow.Any(r => r.WFStatus == 4)).ToList();
                                                    //if (CheckAllreadySanction.Count() > 0)
                                                    //{
                                                    //    return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Rejected....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                                    //}
                                                    objLTCWFDetails = new FunctAllWFDetails
                                                    {
                                                        WFStatus = 4,
                                                        Comments = ReasonApproval,
                                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                                    };
                                                    oFunctWFDetails_List.Add(objLTCWFDetails);
                                                    //  qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                                                    TrClosed = true;
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
                                                    //Recomendation yes -7
                                                    var CheckAllreadyRecomendation = LTCadvclaimList.Where(e => e.LTCWFDetails.Any(r => r.WFStatus == 7)).ToList();
                                                    if (CheckAllreadyRecomendation.Count() > 0)
                                                    {
                                                        return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Recomendation....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                                    }
                                                    objLTCWFDetails = new FunctAllWFDetails
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

                                                    var CheckAllreadyRecomendation = LTCadvclaimList.Where(e => e.LTCWFDetails.Any(r => r.WFStatus == 8)).ToList();
                                                    if (CheckAllreadyRecomendation.Count() > 0)
                                                    {
                                                        return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Rejected....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                                    }
                                                    objLTCWFDetails = new FunctAllWFDetails
                                                    {
                                                        WFStatus = 8,
                                                        Comments = ReasonRecomendation,
                                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                                    };
                                                    oFunctWFDetails_List.Add(objLTCWFDetails);

                                                    //   qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                                                    TrClosed = true;
                                                    SanctionRejected = true;
                                                }
                                            }
                                            if (LTCADVitems.LTCWFDetails != null)
                                            {
                                                oFunctWFDetails_List.AddRange(LTCADVitems.LTCWFDetails);
                                            }

                                            LTCADVitems.LTCWFDetails = oFunctWFDetails_List;
                                            db.LTCAdvanceClaim.Attach(LTCADVitems);
                                            db.Entry(LTCADVitems).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            //db.Entry(x).State = System.Data.Entity.EntityState.Detached;   
                                        }
                                    }
                                }
                            }
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