

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
    public class DiaryAdvanceClaimController : Controller
    {
        //
        // GET: /TADAAdvanceClaim/
        public ActionResult Index()
        {
            return View("~/Views/DiaryAdvanceClaim/Index.cshtml");
        }


        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/_TADAAdvClaimGridPartial.cshtml");
        }


        public ActionResult DIARYAdvanceClaimPartialSanction()
        {
            return View("~/Views/Shared/DIARYAdvanceClaim.cshtml");
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



        public class DIARYAdvClaimChildDataClass //childgrid
        {
            public int Id { get; set; }

            public string ReqDate { get; set; }
            public string DIARYType { get; set; }
            public double EligibleAmt { get; set; }
            public double AdvanceAmt { get; set; }
            public string Remark { get; set; }
            public string ProposedPlace { get; set; }
            public string ClaimSettleStatus { get; set; }
            public string TravelStartDate { get; set; }
            public string TravelEndDate { get; set; }
        }



        public ActionResult Get_DIARYAdvClaim(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.DIARYAdvanceClaim)
                        .Include(e => e.DIARYAdvanceClaim.Select(c => c.DIARYType))

                        .Where(e => e.Id == data).FirstOrDefault();
                    if (db_data.DIARYAdvanceClaim != null)
                    {
                        List<DIARYAdvClaimChildDataClass> returndata = new List<DIARYAdvClaimChildDataClass>();
                        var DiaryAdvClaimlist = db_data.DIARYAdvanceClaim;

                        foreach (var item2 in DiaryAdvClaimlist)
                        {
                            returndata.Add(new DIARYAdvClaimChildDataClass
                            {
                                Id = item2.Id,

                                ReqDate = item2.DateOfAppl.Value != null ? item2.DateOfAppl.Value.ToShortDateString() : "",
                                DIARYType = item2.DIARYType != null ? item2.DIARYType.LookupVal.ToString() : null,
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
                     && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "TADA"

                     )).FirstOrDefault().Amount;
                    return Json(b, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

            }
        }





        [HttpPost]
        public ActionResult Create(DIARYAdvanceClaim DIARYAdvClaim, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();

                try
                {
                    // string LvReqList = form["LvReqList"] == "0" ? "" : form["LvReqList"];
                    int Emp = form["EmpLvnereq_Id"] == "0" ? 0 : Convert.ToInt32(form["EmpLvnereq_Id"]);
                    string DIARYType = form["DIARYTypelist"] == "0" ? "" : form["DIARYTypelist"];
                    string advamt = form["AdvAmt"] == "0" ? "0" : form["AdvAmt"];
                    string Sancamt = form["SanctionedAmt"] == "0" ? "0" : form["SanctionedAmt"];


                    if (Convert.ToDouble(Sancamt) > Convert.ToDouble(advamt))
                    {
                        Msg.Add("  Sanction amount should not be greater than Tada advance amount.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }




                    if (DIARYType != null && DIARYType != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(DIARYType));
                        DIARYAdvClaim.DIARYType = val;
                    }



                    EmployeePayroll EmpData;
                    if (Emp != 0)
                    {
                        int em = Convert.ToInt32(Emp);
                        // EmpData = db.Employee.Include(q => q.FamilyDetails).Where(q => q.Id == em).SingleOrDefault();
                        EmpData = db.EmployeePayroll.Include(q => q.DIARYAdvanceClaim).Where(e => e.Employee.Id == em).SingleOrDefault();

                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (DIARYAdvClaim.TravelEndDate < DIARYAdvClaim.TravelStartDate)
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
                        Comments = DIARYAdvClaim.Remark.ToString(),
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    };

                    List<FunctAllWFDetails> oAttWFDetails_List = new List<FunctAllWFDetails>();
                    oAttWFDetails_List.Add(oAttWFDetails);



                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            DIARYAdvClaim.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            List<DIARYAdvanceClaim> DIARYAdvanceClaim = new List<DIARYAdvanceClaim>();

                            DIARYAdvanceClaim DIARYAdvanceClaim1 = new DIARYAdvanceClaim()
                            {
                                DateOfAppl = DIARYAdvClaim.DateOfAppl,
                                IsClaimSettle = DIARYAdvClaim.IsClaimSettle,
                                EligibleAmt = DIARYAdvClaim.EligibleAmt,
                                DIARYType = DIARYAdvClaim.DIARYType,
                                AdvAmt = DIARYAdvClaim.AdvAmt,
                                // LvNewReq = TADAAdvClaim.LvNewReq,
                                ProposedPlace = DIARYAdvClaim.ProposedPlace,
                                Remark = DIARYAdvClaim.Remark,
                                DBTrack = DIARYAdvClaim.DBTrack,
                                TravelStartDate = DIARYAdvClaim.TravelStartDate,
                                TravelEndDate = DIARYAdvClaim.TravelEndDate,
                                SanctionedAmt = DIARYAdvClaim.SanctionedAmt,
                                LTCWFDetails = oAttWFDetails_List
                            };
                            try
                            {
                                db.DIARYAdvanceClaim.Add(DIARYAdvanceClaim1);
                                db.SaveChanges();
                                DIARYAdvanceClaim.Add(db.DIARYAdvanceClaim.Find(DIARYAdvanceClaim1.Id));

                                //  List<TADAAdvanceClaim> TADAAdvanceClaimlist = new List<TADAAdvanceClaim>();

                                if (EmpData.DIARYAdvanceClaim.Count() > 0)
                                {
                                    DIARYAdvanceClaim.AddRange(EmpData.DIARYAdvanceClaim);
                                }

                                EmpData.DIARYAdvanceClaim = DIARYAdvanceClaim;
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
                                return RedirectToAction("Create", new { concurrencyError = true, id = DIARYAdvanceClaim1.Id });
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





        public class ChildGetDIARYadvanceReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }

        }



        public class GetDIARYAdvanceReqClass
        {
            public string Emp { get; set; }
            public string ReqDate { get; set; }
            public string DIARYType { get; set; }
            public string TravelStartDate { get; set; }
            public string TravelEndDate { get; set; }
            public string ProposedPlace { get; set; }
            public string AdvanceAmount { get; set; }
            public string Status { get; set; }


            public ChildGetDIARYadvanceReqClass RowData { get; set; }
        }



        public ActionResult GetMyDIARYAdvanceClaimReq()   /// Get Created Data on Grid
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string authority = Convert.ToString(Session["auho"]);

                List<GetDIARYAdvanceReqClass> DIARYadvanceclaimlist = new List<GetDIARYAdvanceReqClass>();
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);


                var db_data = db.EmployeePayroll
                      .Where(e => e.Id == Id)
                      .Include(e => e.DIARYAdvanceClaim)
                      .Include(e => e.DIARYAdvanceClaim.Select(a => a.LTCWFDetails))
                      .Include(e => e.DIARYAdvanceClaim.Select(a => a.DIARYType))
                     .FirstOrDefault();


                if (db_data != null)
                {
                    List<GetDIARYAdvanceReqClass> returndata = new List<GetDIARYAdvanceReqClass>();
                    returndata.Add(new GetDIARYAdvanceReqClass
                    {

                        ReqDate = "Requisition Date",
                        DIARYType = "DIARYType",
                        TravelStartDate = "Travel StartDate",
                        TravelEndDate = "Travel EndDate",
                        ProposedPlace = "ProposedPlace",
                        AdvanceAmount = "AdvanceAmount",
                        Status = "Status"
                    });

                    var DIARYAdvanceClaimlist = db_data.DIARYAdvanceClaim.ToList();

                    foreach (var DIARYADVitems in DIARYAdvanceClaimlist)
                    {
                        int WfStatusNew = DIARYADVitems.LTCWFDetails.Select(w => w.WFStatus).LastOrDefault();
                        string Comments = DIARYADVitems.LTCWFDetails.Select(c => c.Comments).LastOrDefault();

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
                            GetDIARYAdvanceReqClass ObjDIARYadvancecClaimRequest = new GetDIARYAdvanceReqClass()
                            {
                                RowData = new ChildGetDIARYadvanceReqClass
                                {
                                    LvNewReq = DIARYADVitems.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString()

                                },
                                ProposedPlace = DIARYADVitems.ProposedPlace.ToString(),
                                DIARYType = DIARYADVitems.DIARYType.LookupVal.ToString(),
                                TravelStartDate = DIARYADVitems.TravelStartDate.Value.ToShortDateString(),
                                TravelEndDate = DIARYADVitems.TravelEndDate.Value.ToShortDateString(),
                                ReqDate = DIARYADVitems.DateOfAppl.Value.ToShortDateString(),
                                AdvanceAmount = DIARYADVitems.AdvAmt.ToString() != "0" ? DIARYADVitems.AdvAmt.ToString() : "0",
                                Status = StatusNarration
                            };
                            returndata.Add(ObjDIARYadvancecClaimRequest);
                        }
                        else if (authority.ToUpper() == "APPROVAL" && WfStatusNew == 1)
                        {
                            GetDIARYAdvanceReqClass ObjDIARYadvancecClaimRequest = new GetDIARYAdvanceReqClass()
                            {
                                RowData = new ChildGetDIARYadvanceReqClass
                                {
                                    LvNewReq = DIARYADVitems.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString()

                                },
                                ProposedPlace = DIARYADVitems.ProposedPlace.ToString(),
                                DIARYType = DIARYADVitems.DIARYType.LookupVal.ToString(),
                                TravelStartDate = DIARYADVitems.TravelStartDate.Value.ToShortDateString(),
                                TravelEndDate = DIARYADVitems.TravelEndDate.Value.ToShortDateString(),
                                ReqDate = DIARYADVitems.DateOfAppl.Value.ToShortDateString(),
                                AdvanceAmount = DIARYADVitems.AdvAmt.ToString() != "0" ? DIARYADVitems.AdvAmt.ToString() : "0",
                                Status = StatusNarration
                            };
                            returndata.Add(ObjDIARYadvancecClaimRequest);
                        }
                        else if (authority.ToUpper() == "MYSELF")
                        {
                            GetDIARYAdvanceReqClass ObjDIARYadvancecClaimRequest = new GetDIARYAdvanceReqClass()
                            {
                                RowData = new ChildGetDIARYadvanceReqClass
                                {
                                    LvNewReq = DIARYADVitems.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString()

                                },
                                ProposedPlace = DIARYADVitems.ProposedPlace.ToString(),
                                DIARYType = DIARYADVitems.DIARYType.LookupVal.ToString(),
                                TravelStartDate = DIARYADVitems.TravelStartDate.Value.ToShortDateString(),
                                TravelEndDate = DIARYADVitems.TravelEndDate.Value.ToShortDateString(),
                                ReqDate = DIARYADVitems.DateOfAppl.Value.ToShortDateString(),
                                AdvanceAmount = DIARYADVitems.AdvAmt.ToString() != "0" ? DIARYADVitems.AdvAmt.ToString() : "0",
                                Status = StatusNarration
                            };
                            returndata.Add(ObjDIARYadvancecClaimRequest);
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

        public class ChildGetDIARYAdvClaimClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }
            public string OdReqId { get; set; }

        }

        public class GetDIARYAdvClaimClass1
        {
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public string ReqDate { get; set; }
            public string DIARYType { get; set; }
            public string TravelStartDate { get; set; }
            public string TravelEndDate { get; set; }
            public string ProposedPlace { get; set; }
            public string AdvanceAmount { get; set; }

            public ChildGetDIARYAdvClaimClass RowData { get; set; }
        }

        public ActionResult GetDIARYAdvanceClaimOnSanction(FormCollection form)
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

                List<GetDIARYAdvClaimClass1> returndata = new List<GetDIARYAdvClaimClass1>();
                returndata.Add(new GetDIARYAdvClaimClass1
                {
                    ReqDate = "Requisition Date",
                    DIARYType = "DIARYType",
                    TravelStartDate = "Travel StartDate",
                    TravelEndDate = "Travel EndDate",
                    ProposedPlace = "ProposedPlace",
                    AdvanceAmount = "AdvanceAmount",
                    EmpCode="EmpCode",
                    EmpName="EmpName",
                    RowData = new ChildGetDIARYAdvClaimClass
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
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.DIARYAdvanceClaim)
                        .Include(e => e.DIARYAdvanceClaim.Select(b => b.DIARYType))
                        .Include(e => e.DIARYAdvanceClaim.Select(b => b.LTCWFDetails))
                        .Include(e => e.DIARYAdvanceClaim.Select(w => w.WFStatus))
                        .ToList();



                    foreach (var item in Emps)
                    {
                        if (item.DIARYAdvanceClaim != null && item.DIARYAdvanceClaim.Count() > 0)
                        {
                            var LvIds = UserManager.FilterDIARYAdvanceClaim(item.DIARYAdvanceClaim.OrderByDescending(e => e.DateOfAppl.Value.ToShortDateString()).ToList(),
                               Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId));

                            if (LvIds.Count() > 0)
                            {
                                var DIARYAdvanceclaimdata = item.DIARYAdvanceClaim.Where(e => LvIds.Contains(e.Id)).ToList();
                                foreach (var singleDIARYAdvClaimDetails in DIARYAdvanceclaimdata)
                                {
                                    if (singleDIARYAdvClaimDetails.LTCWFDetails != null)
                                    {
                                        //int QId = singleODDetails.Id;
                                        // var procd = db.ProcessedData.Include(e => e.MusterRemarks).Where(e => e.Id == singleODDetails.ProcessedData.Id).SingleOrDefault();
                                        var session = Session["auho"].ToString().ToUpper();
                                        var EmpR = item.Employee.ReportingStructRights.Where(e => e.AccessRights != null && e.AccessRights.ActionName.LookupVal.ToUpper() == session && e.FuncModules.LookupVal.ToUpper() == item1.ModuleName.ToUpper())
                                        .Select(e => e.AccessRights.IsClose).FirstOrDefault();
                                        //if (stts.WFStatus == 0)
                                        //{

                                        returndata.Add(new GetDIARYAdvClaimClass1
                                        {
                                            RowData = new ChildGetDIARYAdvClaimClass
                                            {
                                                LvNewReq = singleDIARYAdvClaimDetails.Id.ToString(),
                                                EmpLVid = item.Id.ToString(),
                                                IsClose = EmpR.ToString(),
                                                LvHead_Id = "",
                                            },

                                            ReqDate = singleDIARYAdvClaimDetails.DateOfAppl.Value.ToShortDateString(),
                                            DIARYType = singleDIARYAdvClaimDetails.DIARYType.LookupVal,
                                            TravelStartDate = singleDIARYAdvClaimDetails.TravelStartDate.Value.ToShortDateString(),
                                            TravelEndDate = singleDIARYAdvClaimDetails.TravelEndDate.Value.ToShortDateString(),
                                            ProposedPlace = singleDIARYAdvClaimDetails.ProposedPlace,
                                            AdvanceAmount = singleDIARYAdvClaimDetails.AdvAmt != 0 ? singleDIARYAdvClaimDetails.AdvAmt.ToString() : "0",
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

        public class EmpDIARYadvClaimdata
        {

            public string Status { get; set; }
            public string Emplvhead { get; set; }
            public int DIARYType { get; set; }
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

        public ActionResult GetDIARYadvanceCLaimData(string data)
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
                    .Include(e => e.DIARYAdvanceClaim)
                    .Include(e => e.DIARYAdvanceClaim.Select(b => b.DIARYType))
                    .Include(e => e.DIARYAdvanceClaim.Select(b => b.LTCWFDetails))
                    .Include(e => e.DIARYAdvanceClaim.Select(w => w.WFStatus))

                    .Where(e => e.Employee.Id == EmpLvIdint).SingleOrDefault();

                var v = W.DIARYAdvanceClaim.Where(e => e.Id == id).Select(s => new EmpDIARYadvClaimdata
                {
                    EmployeeId = W.Employee.Id,
                    EmployeeName = W.Employee.EmpCode + " " + W.Employee.EmpName.FullNameFML,
                    Lvnewreq = s.Id,
                    Empcode = W.Employee.EmpCode,

                    Request_Date = s.DateOfAppl != null ? s.DateOfAppl.Value.ToShortDateString() : null,
                    ProposedPlace = s.ProposedPlace,
                    Travel_Start_Date = s.TravelStartDate != null ? s.TravelStartDate.Value.ToShortDateString() : null,
                    Travel_End_Date = s.TravelEndDate != null ? s.TravelEndDate.Value.ToShortDateString() : null,
                    DIARYType = s.DIARYType.Id,
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



        public ActionResult Update_DIARYadvanceClaim(DIARYAdvanceClaim DIARYadvReq, FormCollection form, String data)
        {

            string authority = form["authority"] == null ? null : form["authority"];
            var isClose = form["isClose"] == null ? null : form["isClose"];
            if (authority == null && isClose == null)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
            var ids = Utility.StringIdsToListString(data);
            var DIARYadvnewreqid = Convert.ToInt32(ids[0]);
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
                    .Include(e => e.DIARYAdvanceClaim)
                    .Include(e => e.Employee.EmpName)

                    .Include(e => e.DIARYAdvanceClaim)
                    .Include(e => e.DIARYAdvanceClaim.Select(b => b.DIARYType))
                    .Include(e => e.DIARYAdvanceClaim.Select(b => b.LTCWFDetails))
                    .Include(e => e.DIARYAdvanceClaim.Select(w => w.WFStatus))
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
                        var DIARYAdvanceClaimList = query.DIARYAdvanceClaim.Where(e => e.Id == DIARYadvnewreqid).ToList();
                        //if someone reject lv
                        foreach (var DIARYadvclaimitems in DIARYAdvanceClaimList)
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

                                    DIARYadvclaimitems.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                                    DIARYadvclaimitems.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault();


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

                                    DIARYadvclaimitems.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                                    DIARYadvclaimitems.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault();
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
                                    var CheckAllreadyRecomendation = DIARYAdvanceClaimList.Where(e => e.LTCWFDetails.Any(r => r.WFStatus == 7)).ToList();
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

                                    var CheckAllreadyRecomendation = DIARYAdvanceClaimList.Where(e => e.LTCWFDetails.Any(r => r.WFStatus == 8)).ToList();
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
                            if (DIARYadvclaimitems.LTCWFDetails != null)
                            {
                                oFunctWFDetails_List.AddRange(DIARYadvclaimitems.LTCWFDetails);
                            }

                            DIARYadvclaimitems.LTCWFDetails = oFunctWFDetails_List;
                            db.DIARYAdvanceClaim.Attach(DIARYadvclaimitems);
                            db.Entry(DIARYadvclaimitems).State = System.Data.Entity.EntityState.Modified;
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