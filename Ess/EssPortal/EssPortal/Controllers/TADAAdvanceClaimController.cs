///
/// Created By Anandrao 
/// 

using EssPortal.App_Start;
using EssPortal.Models;
using Payroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Entity;
using System.Web.Mvc;
using P2b.Global;
using System.Transactions;
using EssPortal.Security;
using System.Data.Entity.Infrastructure;
using System.Text;

namespace EssPortal.Controllers
{
    public class TADAAdvanceClaimController : Controller
    {
        //
        // GET: /TADAAdvanceClaim/
        public ActionResult Index()
        {
            return View("~/Views/TADAAdvanceClaim/Index.cshtml");
        }


        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/_TADAAdvClaimGridPartial.cshtml");
        }


        public ActionResult tadaAdvanceClaimPartialSanction()
        {
            return View("~/Views/Shared/_TadaAdavanceClaimOnSanction.cshtml");
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



        public class TADAAdvClaimChildDataClass //childgrid
        {
            public int Id { get; set; }

            public string ReqDate { get; set; }
            public string TADAType { get; set; }
            public double EligibleAmt { get; set; }
            public double AdvanceAmt { get; set; }
            public string Remark { get; set; }
            public string ProposedPlace { get; set; }
            public string ClaimSettleStatus { get; set; }
            public string TravelStartDate { get; set; }
            public string TravelEndDate { get; set; }
        }



        public ActionResult Get_TADAAdvClaim(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.TADAAdvanceClaim)
                        .Include(e => e.TADAAdvanceClaim.Select(c => c.TADAType))

                        .Where(e => e.Id == data).FirstOrDefault();
                    if (db_data.TADAAdvanceClaim != null)
                    {
                        List<TADAAdvClaimChildDataClass> returndata = new List<TADAAdvClaimChildDataClass>();
                        var LTCAdvClaimlist = db_data.TADAAdvanceClaim;

                        foreach (var item2 in LTCAdvClaimlist)
                        {
                            returndata.Add(new TADAAdvClaimChildDataClass
                            {
                                Id = item2.Id,

                                ReqDate = item2.DateOfAppl.Value != null ? item2.DateOfAppl.Value.ToShortDateString() : "",
                                TADAType = item2.TADAType != null ? item2.TADAType.LookupVal.ToString() : null,
                                EligibleAmt = item2.EligibleAmt,
                                AdvanceAmt = item2.AdvAmt,
                                Remark = item2.Remark,
                                ProposedPlace = item2.ProposedPlace,
                                ClaimSettleStatus = item2.IsClaimSettle == true ? "true" : "false",
                                TravelStartDate = item2.TravelStartDate.Value != null ? item2.TravelStartDate.Value.ToShortDateString() : "",
                                TravelEndDate = item2.TravelEndDate.Value != null ? item2.TravelEndDate.Value.ToShortDateString() : ""
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



        public JsonResult GetEmpSalStruct(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var a = db.PayScaleAgreement.Find(int.Parse(data));
                int Emp_Id = int.Parse(data);

                //var QEmpSalStruct1 = db.EmpSalStruct//.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.EffectiveDate >= comparedate)
                //                            .Include(e => e.EmpSalStructDetails)
                //                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                //                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalHeadFormula))
                //                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency))
                //                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Type))
                //                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod))
                //                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType))
                //                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.LvHead));//.ToList();
                //var EmpSalStructTotal = QEmpSalStruct1.Where(e => e.EmployeePayroll.Id == Emp_Id && e.EndDate == null)
                //                              .ToList();
                //var OEmpSalStruct = EmpSalStructTotal.Where(e => e.EndDate == null).SingleOrDefault();
                //if (OEmpSalStruct != null)
                //{

                //    var b = OEmpSalStruct.EmpSalStructDetails
                //        .Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY"
                //     && s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                //     && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "TADA"

                //     )).FirstOrDefault().Amount;
                //    return Json(b, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                //}
                return null;

            }
        }





        [HttpPost]
        public ActionResult Create(TADAAdvanceClaim TADAAdvClaim, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();

                try
                {
                    // string LvReqList = form["LvReqList"] == "0" ? "" : form["LvReqList"];
                    int Emp = form["EmpLvnereq_Id"] == "0" ? 0 : Convert.ToInt32(form["EmpLvnereq_Id"]);
                    string TadaType = form["TADATypelist"] == "0" ? "" : form["TADATypelist"];
                    string advamt = form["AdvAmt"] == "0" ? "0" : form["AdvAmt"];
                    string Sancamt = form["SanctionedAmt"] == "0" ? "0" : form["SanctionedAmt"];


                    if (Convert.ToDouble(Sancamt) > Convert.ToDouble(advamt))
                    {
                        Msg.Add("  Sanction amount should not be greater than Tada advance amount.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }




                    if (TadaType != null && TadaType != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(TadaType));
                        TADAAdvClaim.TADAType = val;
                    }



                    EmployeePayroll EmpData;
                    if (Emp != 0)
                    {
                        int em = Convert.ToInt32(Emp);
                        // EmpData = db.Employee.Include(q => q.FamilyDetails).Where(q => q.Id == em).SingleOrDefault();
                        EmpData = db.EmployeePayroll.Include(q => q.TADAAdvanceClaim).Where(e => e.Employee.Id == em).SingleOrDefault();

                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (TADAAdvClaim.TravelEndDate < TADAAdvClaim.TravelStartDate)
                    {
                        Msg.Add("Travel End date Should Not be Less than Start Date");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    Employee OEmployee = null;

                    OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                           .Where(r => r.Id == Emp).SingleOrDefault();



                    FunctAllWFDetails oAttWFDetails = new FunctAllWFDetails
                    {
                        WFStatus = 0,
                        Comments = TADAAdvClaim.Remark.ToString(),
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    };

                    List<FunctAllWFDetails> oAttWFDetails_List = new List<FunctAllWFDetails>();
                    oAttWFDetails_List.Add(oAttWFDetails);



                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            TADAAdvClaim.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            List<TADAAdvanceClaim> TADAAdvanceClaim = new List<TADAAdvanceClaim>();

                            TADAAdvanceClaim TADAAdvanceClaim1 = new TADAAdvanceClaim()
                            {
                                DateOfAppl = TADAAdvClaim.DateOfAppl,
                                IsClaimSettle = TADAAdvClaim.IsClaimSettle,
                                EligibleAmt = TADAAdvClaim.EligibleAmt,
                                TADAType = TADAAdvClaim.TADAType,
                                AdvAmt = TADAAdvClaim.AdvAmt,
                                // LvNewReq = TADAAdvClaim.LvNewReq,
                                ProposedPlace = TADAAdvClaim.ProposedPlace,
                                Remark = TADAAdvClaim.Remark,
                                DBTrack = TADAAdvClaim.DBTrack,
                                TravelStartDate = TADAAdvClaim.TravelStartDate,
                                TravelEndDate = TADAAdvClaim.TravelEndDate,
                                SanctionedAmt = TADAAdvClaim.SanctionedAmt,
                                LTCWFDetails = oAttWFDetails_List
                            };
                            try
                            {
                                db.TADAAdvanceClaim.Add(TADAAdvanceClaim1);
                                db.SaveChanges();
                                TADAAdvanceClaim.Add(db.TADAAdvanceClaim.Find(TADAAdvanceClaim1.Id));

                                //  List<TADAAdvanceClaim> TADAAdvanceClaimlist = new List<TADAAdvanceClaim>();

                                if (EmpData.TADAAdvanceClaim.Count() > 0)
                                {
                                    TADAAdvanceClaim.AddRange(EmpData.TADAAdvanceClaim);
                                }

                                EmpData.TADAAdvanceClaim = TADAAdvanceClaim;
                                db.EmployeePayroll.Attach(EmpData);
                                db.Entry(EmpData).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(EmpData).State = System.Data.Entity.EntityState.Detached;

                                ts.Complete();
                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                List<string> Msgs = new List<string>();
                                //Msgs.Add("Data Saved successfully");
                                //return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                                return Json(new Utility.JsonClass { status = true, responseText = " Data Saved successfully " }, JsonRequestBehavior.AllowGet);
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = TADAAdvanceClaim1.Id });
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





        public class ChildGetTADAadvanceReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }

        }



        public class GetTAdaAdvanceReqClass
        {
            public string Emp { get; set; }
            public string ReqDate { get; set; }
            public string TADAType { get; set; }
            public string TravelStartDate { get; set; }
            public string TravelEndDate { get; set; }
            public string ProposedPlace { get; set; }
            public string AdvanceAmount { get; set; }
            public string Status { get; set; }


            public ChildGetTADAadvanceReqClass RowData { get; set; }
        }



        public ActionResult GetMyTAdaAdvanceClaimReq()   /// Get Created Data on Grid
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string authority = Convert.ToString(Session["auho"]);

                List<GetTAdaAdvanceReqClass> OTADAadvanceclaimlist = new List<GetTAdaAdvanceReqClass>();
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);

                var db_data = db.EmployeePayroll
                      .Where(e => e.Id == Id)
                      .Include(e => e.TADAAdvanceClaim)
                      .Include(e => e.TADAAdvanceClaim.Select(a => a.LTCWFDetails))
                      .Include(e => e.TADAAdvanceClaim.Select(a => a.TADAType))

                     .FirstOrDefault();


                if (db_data != null)
                {
                    List<GetTAdaAdvanceReqClass> returndata = new List<GetTAdaAdvanceReqClass>();
                    returndata.Add(new GetTAdaAdvanceReqClass
                    {

                        ReqDate = "Requisition Date",
                        TADAType = "TADAType",
                        TravelStartDate = "Travel StartDate",
                        TravelEndDate = "Travel EndDate",
                        ProposedPlace = "ProposedPlace",
                        AdvanceAmount = "AdvanceAmount",
                        Status = "Status"
                    });

                    var TadaAdvanceClaimlist = db_data.TADAAdvanceClaim.ToList();

                    foreach (var TadaADVitems in TadaAdvanceClaimlist)
                    {
                        int WfStatusNew = TadaADVitems.LTCWFDetails.Select(w => w.WFStatus).LastOrDefault();
                        string Comments = TadaADVitems.LTCWFDetails.Select(c => c.Comments).LastOrDefault();

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
                            GetTAdaAdvanceReqClass ObjTADAadvancecClaimRequest = new GetTAdaAdvanceReqClass()
                            {
                                RowData = new ChildGetTADAadvanceReqClass
                                {
                                    LvNewReq = TadaADVitems.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString()

                                },
                                ProposedPlace = TadaADVitems.ProposedPlace.ToString(),
                                TADAType = TadaADVitems.TADAType.LookupVal.ToString(),
                                TravelStartDate = TadaADVitems.TravelStartDate.Value.ToShortDateString(),
                                TravelEndDate = TadaADVitems.TravelEndDate.Value.ToShortDateString(),
                                ReqDate = TadaADVitems.DateOfAppl.Value.ToShortDateString(),
                                AdvanceAmount = TadaADVitems.AdvAmt.ToString() != "0" ? TadaADVitems.AdvAmt.ToString() : "0",
                                Status = StatusNarration
                            };
                            returndata.Add(ObjTADAadvancecClaimRequest);
                        }
                        else if (authority.ToUpper() == "APPROVAL" && WfStatusNew == 1)
                        {
                            GetTAdaAdvanceReqClass ObjTADAadvancecClaimRequest = new GetTAdaAdvanceReqClass()
                            {
                                RowData = new ChildGetTADAadvanceReqClass
                                {
                                    LvNewReq = TadaADVitems.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString()

                                },
                                ProposedPlace = TadaADVitems.ProposedPlace.ToString(),
                                TADAType = TadaADVitems.TADAType.LookupVal.ToString(),
                                TravelStartDate = TadaADVitems.TravelStartDate.Value.ToShortDateString(),
                                TravelEndDate = TadaADVitems.TravelEndDate.Value.ToShortDateString(),
                                ReqDate = TadaADVitems.DateOfAppl.Value.ToShortDateString(),
                                AdvanceAmount = TadaADVitems.AdvAmt.ToString() != "0" ? TadaADVitems.AdvAmt.ToString() : "0",
                                Status = StatusNarration
                            };
                            returndata.Add(ObjTADAadvancecClaimRequest);
                        }
                        else if (authority.ToUpper() == "MYSELF")
                        {
                            GetTAdaAdvanceReqClass ObjTADAadvancecClaimRequest = new GetTAdaAdvanceReqClass()
                            {
                                RowData = new ChildGetTADAadvanceReqClass
                                {
                                    LvNewReq = TadaADVitems.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString()

                                },
                                ProposedPlace = TadaADVitems.ProposedPlace.ToString(),
                                TADAType = TadaADVitems.TADAType.LookupVal.ToString(),
                                TravelStartDate = TadaADVitems.TravelStartDate.Value.ToShortDateString(),
                                TravelEndDate = TadaADVitems.TravelEndDate.Value.ToShortDateString(),
                                ReqDate = TadaADVitems.DateOfAppl.Value.ToShortDateString(),
                                AdvanceAmount = TadaADVitems.AdvAmt.ToString() != "0" ? TadaADVitems.AdvAmt.ToString() : "0",
                                Status = StatusNarration
                            };
                            returndata.Add(ObjTADAadvancecClaimRequest);
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



        #region Get Employee Hotel Book request On Sanction Dropdown

        public class ChildGetTadaAdvClaimClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }
            public string OdReqId { get; set; }

        }

        public class GetTadaAdvClaimClass1
        {
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public string ReqDate { get; set; }
            public string TADAType { get; set; }
            public string TravelStartDate { get; set; }
            public string TravelEndDate { get; set; }
            public string ProposedPlace { get; set; }
            public string AdvanceAmount { get; set; }

            public ChildGetTadaAdvClaimClass RowData { get; set; }
        }

        public ActionResult GetTadaAdvanceClaimOnSanction(FormCollection form)
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

                List<GetTadaAdvClaimClass1> returndata = new List<GetTadaAdvClaimClass1>();
                returndata.Add(new GetTadaAdvClaimClass1
                {
                    
                    ReqDate = "Requisition Date",
                    TADAType = "TADAType",
                    TravelStartDate = "Travel StartDate",
                    TravelEndDate = "Travel EndDate",
                    ProposedPlace = "ProposedPlace",
                    AdvanceAmount = "AdvanceAmount",
                    EmpCode = "EmpCode",
                    EmpName = "EmpName",
                    RowData = new ChildGetTadaAdvClaimClass
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
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.TADAAdvanceClaim)
                        .Include(e => e.TADAAdvanceClaim.Select(b => b.TADAType))
                        .Include(e => e.TADAAdvanceClaim.Select(b => b.LTCWFDetails))
                        .Include(e => e.TADAAdvanceClaim.Select(w => w.WFStatus))
                        .ToList();



                    foreach (var item in Emps)
                    {
                        if (item.TADAAdvanceClaim != null && item.TADAAdvanceClaim.Count() > 0)
                        {
                            var LvIds = UserManager.FilterTADAadvanceCLAIM(item.TADAAdvanceClaim.OrderByDescending(e => e.DateOfAppl.Value.ToShortDateString()).ToList(),
                               Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId));

                            if (LvIds.Count() > 0)
                            {
                                var tadaAdvanceclaimdata = item.TADAAdvanceClaim.Where(e => LvIds.Contains(e.Id)).ToList();
                                foreach (var singleTadaAdvClaimDetails in tadaAdvanceclaimdata)
                                {
                                    if (singleTadaAdvClaimDetails.LTCWFDetails != null)
                                    {
                                        //int QId = singleODDetails.Id;
                                        // var procd = db.ProcessedData.Include(e => e.MusterRemarks).Where(e => e.Id == singleODDetails.ProcessedData.Id).SingleOrDefault();
                                        var session = Session["auho"].ToString().ToUpper();
                                        var EmpR = item.Employee.ReportingStructRights.Where(e => e.AccessRights != null && e.AccessRights.ActionName.LookupVal.ToUpper() == session && e.FuncModules.LookupVal.ToUpper() == item1.ModuleName.ToUpper())
                                        .Select(e => e.AccessRights.IsClose).FirstOrDefault();
                                        //if (stts.WFStatus == 0)
                                        //{

                                        returndata.Add(new GetTadaAdvClaimClass1
                                        {
                                            RowData = new ChildGetTadaAdvClaimClass
                                            {
                                                LvNewReq = singleTadaAdvClaimDetails.Id.ToString(),
                                                EmpLVid = item.Id.ToString(),
                                                IsClose = EmpR.ToString(),
                                                LvHead_Id = "",
                                            },

                                            ReqDate = singleTadaAdvClaimDetails.DateOfAppl.Value.ToShortDateString(),
                                            TADAType = singleTadaAdvClaimDetails.TADAType.LookupVal,
                                            TravelStartDate = singleTadaAdvClaimDetails.TravelStartDate.Value.ToShortDateString(),
                                            TravelEndDate = singleTadaAdvClaimDetails.TravelEndDate.Value.ToShortDateString(),
                                            ProposedPlace = singleTadaAdvClaimDetails.ProposedPlace,
                                            AdvanceAmount = singleTadaAdvClaimDetails.AdvAmt != 0 ? singleTadaAdvClaimDetails.AdvAmt.ToString() : "0",
                                            EmpCode=item.Employee.EmpCode,
                                            EmpName=item.Employee.EmpName.FullNameFML
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
        #endregion



        #region Get Employee Hotel Book request On Sanction Bind DATA

        public class EmpTADAadvClaimdata
        {

            public string Status { get; set; }
            public string Emplvhead { get; set; }
            public int TadaType { get; set; }
            public string Travel_Start_Date { get; set; }
            public string Travel_End_Date { get; set; }
            public string Request_Date { get; set; }
            public string ProposedPlace { get; set; }

            public string Start_Date { get; set; }
            public string End_Date { get; set; }
            public string SanctionCode { get; set; }
            public string SanctionEmpname { get; set; }
            public string RecomendationCode { get; set; }
            public string RecomendationEmpname { get; set; }
            public double AdvAmt { get; set; }
            public double EligibleAmt { get; set; }

            public double SanctionedAmt { get; set; }
            public bool TrClosed { get; set; }
            public bool TrReject { get; set; }
            public string SanctionComment { get; set; }
            public string ApporavalComment { get; set; }
            public string Wf { get; set; }
            public Int32 Id { get; set; }
            public Int32 Lvnewreq { get; set; }
           
            public string EmployeeName { get; set; }
            public string Remark { get; set; }
            public string Empcode { get; set; }
            public string Isclose { get; set; }
            public int EmployeeId { get; set; }
            public string Incharge { get; set; }
        }

        public ActionResult GetTADAadvanceCLaimData(string data)
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
                    .Include(e => e.TADAAdvanceClaim)
                    .Include(e => e.TADAAdvanceClaim.Select(b => b.TADAType))
                    .Include(e => e.TADAAdvanceClaim.Select(b => b.LTCWFDetails))
                    .Include(e => e.TADAAdvanceClaim.Select(w => w.WFStatus))

                    .Where(e => e.Employee.Id == EmpLvIdint).SingleOrDefault();

                var v = W.TADAAdvanceClaim.Where(e => e.Id == id).Select(s => new EmpTADAadvClaimdata
                {
                    EmployeeId = W.Employee.Id,
                    EmployeeName = W.Employee.EmpCode + " " + W.Employee.EmpName.FullNameFML,
                    Lvnewreq = s.Id,
                    Empcode = W.Employee.EmpCode,
                    Request_Date = s.DateOfAppl != null ? s.DateOfAppl.Value.ToShortDateString() : null,
                    ProposedPlace = s.ProposedPlace,
                    Travel_Start_Date = s.TravelStartDate != null ? s.TravelStartDate.Value.ToShortDateString() : null,
                    Travel_End_Date = s.TravelEndDate != null ? s.TravelEndDate.Value.ToShortDateString() : null,
                    TadaType = s.TADAType.Id,
                    AdvAmt = s.AdvAmt,
                    EligibleAmt = s.EligibleAmt,
                    SanctionedAmt = s.SanctionedAmt,
                    
                    //Status = status,

                   
                    Remark = s.Remark,
                    //EmpContactNO = s.ContactNo != null ? s.ContactNo.FullContactNumbers : null,
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


                return Json(v, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion



        public ActionResult Update_TADAadvanceClaim(TADAAdvanceClaim TADAadvReq, FormCollection form, String data)
        {

            string authority = form["authority"] == null ? null : form["authority"];
            var isClose = form["isClose"] == null ? null : form["isClose"];
            if (authority == null && isClose == null)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
            var ids = Utility.StringIdsToListString(data);
            var TADAadvnewreqid = Convert.ToInt32(ids[0]);
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
                    .Include(e => e.TADAAdvanceClaim)
                    .Include(e => e.Employee.EmpName)

                    .Include(e => e.TADAAdvanceClaim)
                    .Include(e => e.TADAAdvanceClaim.Select(b => b.TADAType))
                    .Include(e => e.TADAAdvanceClaim.Select(b => b.LTCWFDetails))
                    .Include(e => e.TADAAdvanceClaim.Select(w => w.WFStatus))
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


                bool TrClosed = false;

                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        var TadaAdvanceClaimList = query.TADAAdvanceClaim.Where(e => e.Id == TADAadvnewreqid).ToList();
                        //if someone reject lv
                        foreach (var tadaadvclaimitems in TadaAdvanceClaimList)
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

                                    tadaadvclaimitems.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                                    tadaadvclaimitems.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault();


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

                                    tadaadvclaimitems.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                                    tadaadvclaimitems.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault();
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
                                    var CheckAllreadyRecomendation = TadaAdvanceClaimList.Where(e => e.LTCWFDetails.Any(r => r.WFStatus == 7)).ToList();
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

                                    var CheckAllreadyRecomendation = TadaAdvanceClaimList.Where(e => e.LTCWFDetails.Any(r => r.WFStatus == 8)).ToList();
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
                            if (tadaadvclaimitems.LTCWFDetails != null)
                            {
                                oFunctWFDetails_List.AddRange(tadaadvclaimitems.LTCWFDetails);
                            }

                            tadaadvclaimitems.LTCWFDetails = oFunctWFDetails_List;
                            db.TADAAdvanceClaim.Attach(tadaadvclaimitems);
                            db.Entry(tadaadvclaimitems).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            //db.Entry(x).State = System.Data.Entity.EntityState.Detached;   
                        }
                        
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