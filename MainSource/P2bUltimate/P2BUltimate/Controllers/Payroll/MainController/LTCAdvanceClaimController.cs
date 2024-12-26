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
    [AuthoriseManger]
    public class LTCAdvanceClaimController : Controller
    {
        List<String> Msg = new List<string>();
        //
        // GET: /LTCAdvanceClaim/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/LTCAdvanceClaim/Index.cshtml");
        }

        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Payroll/_LTCAdvClaimGridPartial.cshtml");
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

        public class LvNewReqData
        {
            public int Id { get; set; }
            public string FullDetails { get; set; }

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

        public ActionResult ValidateForm(FormCollection form)
        {
            string LvReq = form["LvReqList"] == "0" ? "" : form["LvReqList"];
            int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
            List<string> Msg = new List<string>();

            if (Emp == null)
            {
                Msg.Add("Please select employee");
            }
            if (LvReq == null)
            {
                Msg.Add("Please select leave requisition");
            }


            if (Msg.Count > 0)
            {
                return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);

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
                    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
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

                    //if (LTCAdvClaim.LvNewReq.FromDate <= LTCAdvClaim.TravelStartDate && LTCAdvClaim.LvNewReq.ToDate >= LTCAdvClaim.TravelEndDate)
                    //{
                    //    Msg.Add("Leave requisition date should be between travel start date & end date.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
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
                                LTCSanctionedAmt = LTCAdvClaim.LTCSanctionedAmt
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
                                Msgs.Add("Data Saved successfully");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
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

        public class returnDataClass
        {
            public string ProposedPlace { get; set; }
            public double EligibleAmount { get; set; }
            public double AdvanceAmount { get; set; }
            public string Remark { get; set; }

        }

        public ActionResult GridEditData(int data)
        {
            var returnlist = new List<returnDataClass>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != 0)
                {
                    var retrundataList = db.LTCAdvanceClaim.Where(e => e.Id == data).FirstOrDefault();

                    returnlist.Add(new returnDataClass()
                    {
                        ProposedPlace = retrundataList.ProposedPlace,
                        EligibleAmount = retrundataList.LTC_Eligible_Amt,
                        AdvanceAmount = retrundataList.LTCAdvAmt,
                        Remark = retrundataList.Remark

                    });

                    return Json(new { returndata = returnlist }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View();
                }
            }
        }

        public ActionResult GridEditSave(LTCAdvanceClaim LTCAdvanceClaim, FormCollection form, string data)
        {
            List<string> Msgs = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
                var LTCAdvClaimData = db.LTCAdvanceClaim.Where(e => e.Id == Id).SingleOrDefault();
                string ProposedPlace = form["ProposedPlace"];

                double LTCAdvanceAmount = Convert.ToDouble(form["LTCAdvanceAmount"]);
                string Remark = form["Remark"];

                LTCAdvClaimData.ProposedPlace = ProposedPlace;
                LTCAdvClaimData.LTCAdvAmt = LTCAdvanceAmount;
                LTCAdvClaimData.Remark = Remark;

                using (TransactionScope ts = new TransactionScope())
                {

                    LTCAdvClaimData.DBTrack = new DBTrack
                    {
                        CreatedBy = LTCAdvClaimData.DBTrack.CreatedBy == null ? null : LTCAdvClaimData.DBTrack.CreatedBy,
                        CreatedOn = LTCAdvClaimData.DBTrack.CreatedOn == null ? null : LTCAdvClaimData.DBTrack.CreatedOn,
                        Action = "M",
                        ModifiedBy = SessionManager.UserName,
                        ModifiedOn = DateTime.Now
                    };


                    try
                    {
                        db.LTCAdvanceClaim.Attach(LTCAdvClaimData);
                        db.Entry(LTCAdvClaimData).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(LTCAdvClaimData).State = System.Data.Entity.EntityState.Detached;

                        ts.Complete();
                        return this.Json(new { status = true, responseText = "Data Updated Successfully.", JsonRequestBehavior.AllowGet });
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Create", new { concurrencyError = true, id = LTCAdvClaimData.Id });
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

        [HttpPost]
        public async Task<ActionResult> Delete(int data, int data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LTCAdvanceClaim LTCAdvClaim = db.LTCAdvanceClaim.Where(e => e.Id == data).SingleOrDefault();

                if (LTCAdvClaim.IsClaimSettle == true)
                {
                    return this.Json(new { status = true, valid = true, responseText = "You cannot delete as claim is already settled.", JsonRequestBehavior.AllowGet });
                }

                var selectedRegions = db.LTCSettlementClaim.Include(e => e.LTCAdvanceClaim).Where(e => e.LTCAdvanceClaim.Id == data).SingleOrDefault();

                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    if (selectedRegions != null)
                    {

                        return this.Json(new { status = true, valid = true, responseText = "You cannot delete as you have done settlement for this advance.", JsonRequestBehavior.AllowGet });

                    }

                    try
                    {
                        var OEMPLTCBlockT = db.EmployeePayroll.Where(e => e.Id == data2).Include(e => e.EmpLTCBlock)
                            .Include(e => e.EmpLTCBlock.Select(r => r.EmpLTCBlockT))
                            .Include(e => e.EmpLTCBlock.Select(r => r.EmpLTCBlockT.Select(t => t.LTCAdvanceClaim)))
                            .Include(e => e.EmpLTCBlock.Select(r => r.EmpLTCBlockT.Select(t => t.LTCSettlementClaim))).FirstOrDefault();

                        foreach (var a in OEMPLTCBlockT.EmpLTCBlock.ToList())
                        {
                            //int AdvId = a.EmpLTCBlockT.Where(e => e.IsBlockClose == false).FirstOrDefault().LTCAdvanceClaim.Id;
                            foreach (var b in a.EmpLTCBlockT.Where(e => e.IsBlockClose == false))
                            {
                                foreach (var c in b.LTCAdvanceClaim)
                                {
                                    if (c.Id == data)
                                    {
                                        b.LTCAdvanceClaim.Remove(c);
                                        db.EmpLTCBlockT.Attach(b);
                                        db.Entry(b).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(b).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }

                            }

                        }

                        db.LTCAdvanceClaim.Remove(LTCAdvClaim);
                        db.SaveChanges();
                        //await db.SaveChangesAsync();


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
    }
}