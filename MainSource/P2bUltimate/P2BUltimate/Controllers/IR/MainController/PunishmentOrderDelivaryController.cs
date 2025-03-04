﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IR;
using P2b.Global;
using System.Data.Entity.Infrastructure;
using System.Text;
using System.Transactions;
using System.Data;
using System.Data.Entity;
using P2BUltimate.Models;
using P2BUltimate.App_Start;
using System.Threading.Tasks;
using P2BUltimate.Security;
//using System.Web.Script.Serialization;
using Payroll;


using System.ComponentModel.DataAnnotations;

namespace P2BUltimate.Controllers.IR.MainController
{
    public class PunishmentOrderDelivaryController : Controller
    {
        //
        // GET: /PunishmentOrderDelivary/
        public ActionResult Index()
        {
            return View("~/Views/IR/MainViews/PunishmentOrderDelivary/Index.cshtml");
        }
        public class P2BGridData
        {
            public string VictimName { get; set; }
            public string CaseNo { get; set; }
            public string ProceedingStage { get; set; }
            public string Id { get; set; }
            public string IsClosedServing { get; set; }
            public string IsNoticeRecd { get; set; }

            public string Narration { get; set; }
            public string PunishmentOrderServingDate { get; set; }
        }


        public List<string> ValidateObj(Object obj)
        {
            var errorList = new List<String>();
            var context = new ValidationContext(obj, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(obj, context, results, true);
            if (!isValid)
            {
                foreach (var validationResult in results)
                {
                    errorList.Add(validationResult.ErrorMessage);
                }
                return errorList;
            }
            else
            {
                return errorList;
            }
        }
        public ActionResult GetLookupPunishmentOrderServingMode(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ChargeSheetServingMode.Include(x => x.ChargeSheetServingModeName).ToList();
                IEnumerable<ChargeSheetServingMode> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.ChargeSheetServingMode.ToList();
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.ChargeSheetServingModeName.LookupVal.ToString().ToUpper() + "," + ca.ServingSeq }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.ServingSeq }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }
        public ActionResult GetLookupDetailsPunishmentOrderDeliveryDoc(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.EmployeeDocuments
                    .Include(e => e.DocumentType)
                    .ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.EmployeeDocuments.Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);

            }
        }
        public ActionResult GetLookupWitnessData(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Employee.Include(e => e.EmpName).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Employee.Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost]
        public ActionResult Create(PunishmentOrderDelivery c, FormCollection form, string EmpIr)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int comp_Id = 0;
                    comp_Id = Convert.ToInt32(Session["CompId"]);
                    var Company = new Company();
                    Company = db.Company.Where(e => e.Id == comp_Id).SingleOrDefault();
                    int EmpIrid = Convert.ToInt32(EmpIr);
                    string caseNO = Convert.ToString(Session["findcase"]);
                    string PunishmentOrderServingDate = form["PunishmentOrderServingDate"] == "0" ? "" : form["PunishmentOrderServingDate"];
                    if (PunishmentOrderServingDate != null && PunishmentOrderServingDate != "")
                    {
                        var val = DateTime.Parse(PunishmentOrderServingDate);
                        c.PunishmentOrderServingDate = val;
                    }
                    string PunishmentOrderServingModeList = form["PunishmentOrderServingModeList"] == "0" ? null : form["PunishmentOrderServingModeList"];
                    if (PunishmentOrderServingModeList != null && PunishmentOrderServingModeList != "")
                    {

                        int PunishmentOrderServingModeId = int.Parse(PunishmentOrderServingModeList);
                        var val = db.ChargeSheetServingMode.Include(e => e.ChargeSheetServingModeName).Where(e => e.Id == PunishmentOrderServingModeId).SingleOrDefault();
                        c.PunishmentOrderServingMode = val;
                    }

                    string ShowCauseNoticeList = form["ShowCauseNoticeList"] == "0" ? null : form["ShowCauseNoticeList"];
                    if (ShowCauseNoticeList != null && ShowCauseNoticeList != "")
                    {
                        var ids = Utility.StringIdsToListIds(ShowCauseNoticeList);
                        List<EmployeeDocuments> showcausenoticelist = new List<EmployeeDocuments>();
                        foreach (var ca in ids)
                        {
                            var showcausenoticelist_val = db.EmployeeDocuments.Find(ca);

                            showcausenoticelist.Add(showcausenoticelist_val);
                            c.PunishmentOrderNotice = showcausenoticelist;
                        }
                    }
                    else
                        c.PunishmentOrderNotice = null;

                    string WitnessList = form["WitnessList"] == "0" ? null : form["WitnessList"];
                    if (WitnessList != null && WitnessList != "")
                    {
                        var ids = Utility.StringIdsToListIds(WitnessList);
                        List<Witness> witnesslist = new List<Witness>();
                        foreach (var ca in ids)
                        {
                            int witID = Convert.ToInt32(ca);
                            // var witnesslist_val = db.EmployeeIR.Find(ca);
                            var witnesslist_val = db.EmployeeIR.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id == witID).SingleOrDefault();
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };

                            if (witnesslist_val != null)
                            {
                                Witness Objwit = new Witness()
                                {
                                    WitnessEmp = witnesslist_val,
                                    DBTrack = c.DBTrack
                                };
                                witnesslist.Add(Objwit);
                            }

                            c.Witness = witnesslist;
                        }
                    }
                    else
                        c.Witness = null;


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            #region Already PunishmentOrderDelivery enquiry records exist checking code
                            var EMPIR = db.EmployeeIR.Select(s => new
                            {
                                Irid = s.Id,
                                oEmpcode = s.Employee.EmpCode,
                                EDP = s.EmpDisciplineProcedings.Select(r => new
                                {
                                    EDPid = r.Id,
                                    CaseNum = r.CaseNo,
                                    POD = r.PunishmentOrderDelivery,
                                }).Where(e => e.CaseNum == caseNO).ToList(),

                            }).Where(e => e.Irid == EmpIrid).SingleOrDefault();


                            if (EMPIR != null)
                            {
                                var chkEMPIR = EMPIR.EDP.ToList();

                                foreach (var itemC in chkEMPIR.Where(e => e.POD != null))
                                {
                                    if (itemC.EDPid != 0 && itemC.CaseNum != null && itemC.POD != null)
                                    {
                                        Msg.Add("Record already exist for case no : " + itemC.CaseNum + " & victim :: " + EMPIR.oEmpcode);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            #endregion

                            PunishmentOrderDelivery PunishmentOrderDelivery = new PunishmentOrderDelivery()
                            {
                                IsClosedServing = c.IsClosedServing,
                                IsNoticeRecd = c.IsNoticeRecd,
                                IsWitnessReqd = c.IsWitnessReqd,
                                PunishmentOrderServingDate = c.PunishmentOrderServingDate,
                                PunishmentOrderServingMode = c.PunishmentOrderServingMode,
                                Narration = c.Narration,
                                Witness = c.Witness,
                                PunishmentOrderNotice = c.PunishmentOrderNotice,
                                DBTrack = c.DBTrack
                            };

                            var PunishmentOrderDeliveryValidation = ValidateObj(PunishmentOrderDelivery);
                            if (PunishmentOrderDeliveryValidation.Count > 0)
                            {
                                foreach (var item in PunishmentOrderDeliveryValidation)
                                {

                                    Msg.Add("PunishmentOrderDelivery" + item);
                                }
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            db.PunishmentOrderDelivery.Add(PunishmentOrderDelivery);
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                            //DT_MisconductComplaint DT_Corp = (DT_MisconductComplaint)rtn_Obj;
                            //db.Create(DT_Corp);
                            try
                            {
                                //db.MisconductComplaint.Add(MisConductComplaint);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, c.DBTrack);
                                db.SaveChanges();

                                #region  Add NEW Stages in EmpDisciplineProcedings

                                //var EmpDisciplines = db.EmpDisciplineProcedings.Include(e => e.MisconductComplaint)
                                //    .Include(e => e.PreminaryEnquiry).Include(e => e.PreminaryEnquiryAction)
                                //    .Include(e => e.ChargeSheet)
                                //    .Include(e => e.ChargeSheetReply).Include(e => e.ChargeSheetEnquiryNotice)
                                //    .Include(e => e.ChargeSheetEnquiryNoticeServing).Include(e => e.ChargeSheetEnquiryProceedings)
                                //    .Include(e => e.ChargeSheetEnquiryReport).Include(e => e.PostEnquiryPrerquisite).Include(e => e.FinalShowCauseNotice)
                                //    .Include(e => e.FinalShowCauseNoticeServing).Include(e => e.FinalShowCauseNoticeReply).Include(e => e.FinalShowCauseNoticeClarification)
                                //    .Include(e => e.FinalShowCauseNoticeClarificarionServing)
                                //    .Include(e => e.PunishmentOrder)
                                //    .Where(e => e.CaseNo == caseNO).ToList().LastOrDefault();

                                var EmpDisciplines = db.EmployeeIR.Include(e => e.EmpDisciplineProcedings)
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.MisconductComplaint))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.PreminaryEnquiry))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.PreminaryEnquiryAction))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheet))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheetReply))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheetEnquiryNotice))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheetEnquiryNoticeServing))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheetEnquiryProceedings))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheetEnquiryReport))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.PostEnquiryPrerquisite))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.FinalShowCauseNotice))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.FinalShowCauseNoticeServing))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.FinalShowCauseNoticeReply))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.FinalShowCauseNoticeClarification))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.FinalShowCauseNoticeClarificarionServing))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.PunishmentOrder))

                                    .Where(e => e.Id == EmpIrid).FirstOrDefault().EmpDisciplineProcedings.Where(e => e.CaseNo == caseNO).FirstOrDefault();

                                // For ChargeSheetServing Value beacuse of ICollection
                                //List<ChargeSheetServing> ChargesheetServeVal = new List<ChargeSheetServing>();
                                //foreach (var item in EmpDisciplines.ChargeSheetServing)
                                //{
                                //    var a = db.ChargeSheetServing.Find(item.Id);
                                //    ChargesheetServeVal.Add(a);
                                //}
                                var getEmpdisciplineForChargesheetserving = db.EmpDisciplineProcedings.Include(e => e.ChargeSheetServing).Where(e => e.CaseNo == caseNO && e.ProceedingStage == 5).FirstOrDefault();

                                EmpDisciplineProcedings EmpDiscipline = new EmpDisciplineProcedings()
                                {
                                    CaseNo = EmpDisciplines.CaseNo,
                                    CaseOpeningDate = EmpDisciplines.CaseOpeningDate,
                                    ProceedingStage = 18,
                                    MisconductComplaint = EmpDisciplines.MisconductComplaint,
                                    PreminaryEnquiry = EmpDisciplines.PreminaryEnquiry,
                                    PreminaryEnquiryAction = EmpDisciplines.PreminaryEnquiryAction,
                                    ChargeSheet = EmpDisciplines.ChargeSheet,
                                    ChargeSheetServing = getEmpdisciplineForChargesheetserving.ChargeSheetServing.ToList(),
                                    ChargeSheetReply = EmpDisciplines.ChargeSheetReply,
                                    ChargeSheetEnquiryNotice = EmpDisciplines.ChargeSheetEnquiryNotice,
                                    ChargeSheetEnquiryNoticeServing = EmpDisciplines.ChargeSheetEnquiryNoticeServing,
                                    ChargeSheetEnquiryProceedings = EmpDisciplines.ChargeSheetEnquiryProceedings,
                                    ChargeSheetEnquiryReport = EmpDisciplines.ChargeSheetEnquiryReport,
                                    PostEnquiryPrerquisite = EmpDisciplines.PostEnquiryPrerquisite,
                                    FinalShowCauseNotice = EmpDisciplines.FinalShowCauseNotice,
                                    FinalShowCauseNoticeServing = EmpDisciplines.FinalShowCauseNoticeServing,
                                    FinalShowCauseNoticeReply = EmpDisciplines.FinalShowCauseNoticeReply,
                                    FinalShowCauseNoticeClarification = EmpDisciplines.FinalShowCauseNoticeClarification,
                                    FinalShowCauseNoticeClarificarionServing = EmpDisciplines.FinalShowCauseNoticeClarificarionServing,
                                    PunishmentOrder = EmpDisciplines.PunishmentOrder,
                                    PunishmentOrderDelivery = db.PunishmentOrderDelivery.Find(PunishmentOrderDelivery.Id),
                                    DBTrack = c.DBTrack
                                };

                                db.EmpDisciplineProcedings.Add(EmpDiscipline);
                                db.SaveChanges();
                                Session["Empdispre_Id"] = EmpDiscipline.Id;



                                var EmpDisIR = new EmployeeIR();

                                EmpDisIR = db.EmployeeIR.Include(e => e.EmpDisciplineProcedings).Where(e => e.Id == EmpIrid).FirstOrDefault();


                                var EmpDisciplineProcedings = new EmpDisciplineProcedings();
                                EmpDisciplineProcedings = db.EmpDisciplineProcedings.Include(e => e.MisconductComplaint)
                                                            .Include(e => e.PreminaryEnquiry)
                                                            .Include(e => e.PreminaryEnquiryAction)
                                                            .Include(e => e.ChargeSheet)
                                                            .Where(e => e.Id == EmpDiscipline.Id).SingleOrDefault();


                                List<EmpDisciplineProcedings> aaa = new List<EmpDisciplineProcedings>();
                                aaa.Add(EmpDisciplineProcedings);
                                aaa.AddRange(EmpDisIR.EmpDisciplineProcedings);
                                EmpDisIR.EmpDisciplineProcedings = aaa;
                                db.EmployeeIR.Attach(EmpDisIR);
                                db.Entry(EmpDisIR).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(EmpDisIR).State = System.Data.Entity.EntityState.Detached;
                                #endregion

                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            //catch (DbUpdateConcurrencyException)
                            //{
                            //    return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            //}
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        // Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    PunishmentOrderDelivery corporates = db.PunishmentOrderDelivery
                                                            .Include(e => e.PunishmentOrderServingMode)
                                                            .Include(e => e.Witness)
                                                            .Include(e => e.PunishmentOrderNotice)
                                                       .Where(e => e.Id == data).SingleOrDefault();


                    if (corporates.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? true : false
                            };

                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        // var selectedRegions = corporates.Regions;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {



                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                            db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;


                            await db.SaveChangesAsync();



                            ts.Complete();
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
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
        public class returnEditClass
        {
            public Array PunishmentDeliveryDoc_Id { get; set; }
            public Array PunishmentDelivaryDocFullDetails { get; set; }
            public Array Witnessorderdelivery_Id { get; set; }
            public Array WitnessorderdeliveryFulldetails { get; set; }
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.PunishmentOrderDelivery

                                .Include(e => e.PunishmentOrderServingMode)
                                .Include(e => e.PunishmentOrderServingMode.ChargeSheetServingModeName)
                                .Include(e => e.Witness)
                                .Include(e => e.PunishmentOrderNotice)
                                .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        Narration = e.Narration,
                        PunishmentOrderServingDate = e.PunishmentOrderServingDate,
                        IsClosedServing = e.IsClosedServing,
                        IsNoticeRecd = e.IsNoticeRecd,
                        IsWitnessReqd = e.IsWitnessReqd,
                        PunishmentOrderServingMode_Id = e.PunishmentOrderServingMode != null ? e.PunishmentOrderServingMode.Id : 0,
                        PunishmentOrderServingModeDetails = e.PunishmentOrderServingMode.ChargeSheetServingModeName.LookupVal + "," + e.PunishmentOrderServingMode.ServingSeq == null ? "" : e.PunishmentOrderServingMode.ChargeSheetServingModeName.LookupVal + "," + e.PunishmentOrderServingMode.ServingSeq
                    }).ToList();

                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();

                var k = db.PunishmentOrderDelivery.Include(e => e.PunishmentOrderNotice)
                 .Include(e => e.PunishmentOrderNotice.Select(a => a.DocumentType))
                .Where(e => e.Id == data && e.PunishmentOrderNotice.Count > 0).ToList();
                foreach (var e in k)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        PunishmentDeliveryDoc_Id = e.PunishmentOrderNotice.Select(a => a.Id.ToString()).ToArray(),
                        PunishmentDelivaryDocFullDetails = e.PunishmentOrderNotice.Select(a => a.FullDetails).ToArray()
                    });
                }
                var m = db.PunishmentOrderDelivery.Include(e => e.Witness).Include(e => e.Witness.Select(a => a.WitnessEmp))
                  .Include(e => e.Witness.Select(a => a.WitnessEmp.Employee)).Include(e => e.Witness.Select(a => a.WitnessEmp.Employee.EmpName))
                  .Where(e => e.Id == data && e.Witness.Count > 0).ToList();
                foreach (var e in m)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        Witnessorderdelivery_Id = e.Witness.Select(a => a.Id.ToString()).ToArray(),
                        WitnessorderdeliveryFulldetails = e.Witness.Select(a => a.WitnessEmp.Employee.EmpName.FullNameFML).ToArray()
                    });
                }

                return Json(new Object[] { returndata, oreturnEditClass, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;

                var ModuleName = System.Web.HttpContext.Current.Session["ModuleType"];

                IEnumerable<P2BGridData> PunishmentOrderDelivery = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;
                //var oemployeedata = db.PunishmentOrderDelivery.OrderBy(e => e.Id).ToList();
                var oemployeedata = db.EmployeeIR.Select(a => new
                {
                    oVictimName = a.Employee.EmpName.FullNameFML,

                    oEmpDiscipline = a.EmpDisciplineProcedings.Select(b => new
                    {
                        oCaseNo = b.CaseNo,
                        oProceedStage = b.ProceedingStage.ToString(),
                        oPunishmentOrderServingDate = b.PunishmentOrderDelivery.PunishmentOrderServingDate,
                        oIsNoticeRecd = b.PunishmentOrderDelivery.IsNoticeRecd.ToString(),
                        oIsClosedServing = b.PunishmentOrderDelivery.IsClosedServing.ToString(),

                        oNarration = b.PunishmentOrderDelivery.Narration,
                        oId = b.PunishmentOrderDelivery.Id.ToString(),

                    }).ToList(),

                }).Where(e => e.oEmpDiscipline.Count() > 0).ToList();

                foreach (var itemIR in oemployeedata.Where(e => e.oEmpDiscipline.Count() > 0))
                {
                    foreach (var item in itemIR.oEmpDiscipline.Where(e => e.oProceedStage == "18").OrderBy(o => o.oCaseNo))
                    {
                        view = new P2BGridData()
                        {
                            CaseNo = item.oCaseNo,
                            VictimName = itemIR.oVictimName.ToString(),
                            ProceedingStage = item.oProceedStage.ToString(),
                            PunishmentOrderServingDate = item.oPunishmentOrderServingDate != null ? item.oPunishmentOrderServingDate.Value.ToString("dd/MM/yyyy") : "",
                            Id = item.oId.ToString(),
                            IsNoticeRecd = item.oIsNoticeRecd.ToString(),
                            IsClosedServing = item.oIsClosedServing.ToString(),

                            Narration = item.oNarration.ToString(),

                        };

                        model.Add(view);
                    }
                }

                PunishmentOrderDelivery = model;

                IEnumerable<P2BGridData> IE;

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = PunishmentOrderDelivery;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE
                         .Where((e => (e.PunishmentOrderServingDate.ToString().ToUpper().Contains(gp.searchString))
                             || (e.CaseNo.ToString().Contains(gp.searchString))
                                           || (e.VictimName.ToString().ToUpper().Contains(gp.searchString))
                                          || (e.ProceedingStage.ToString().ToUpper().Contains(gp.searchString))
                                    || (e.IsNoticeRecd.ToString().Contains(gp.searchString.ToUpper()))
                                   || (e.IsClosedServing.ToString().Contains(gp.searchString.ToUpper()))
                                   || (e.Narration.ToString().ToUpper().Contains(gp.searchString))

                                  || (e.Id.ToString().Contains(gp.searchString))
                                 )).ToList()
                        .Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.PunishmentOrderServingDate, a.IsNoticeRecd, a.IsClosedServing, a.Narration, a.Id });
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.PunishmentOrderServingDate, a.IsNoticeRecd, a.IsClosedServing, a.Narration, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();


                }
                else
                {
                    IE = PunishmentOrderDelivery;
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : "0");
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "PunishmentOrderServingDate" ? c.PunishmentOrderServingDate.ToString() :
                            gp.sidx == "CaseNo" ? c.CaseNo.ToString() :
                                         gp.sidx == "VictimName" ? c.VictimName.ToString() :
                                         gp.sidx == "ProceedingStage" ? c.ProceedingStage.ToString() :
                                        gp.sidx == "IsNoticeRecd" ? c.IsNoticeRecd.ToString() :
                                        gp.sidx == "IsClosedServing" ? c.IsClosedServing.ToString() :
                                          gp.sidx == "Narration" ? c.Narration.ToString() : "");

                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, Convert.ToString(a.PunishmentOrderServingDate), a.IsNoticeRecd, a.IsClosedServing, a.Narration, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.PunishmentOrderServingDate, a.IsNoticeRecd, a.IsClosedServing, a.Narration, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.PunishmentOrderServingDate, a.IsNoticeRecd, a.IsClosedServing, a.Narration, a.Id }).ToList();
                    }
                    totalRecords = PunishmentOrderDelivery.Count();
                }
                if (totalRecords > 0)
                {
                    totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
                }
                if (gp.page > totalPages)
                {
                    gp.page = totalPages;
                }
                var JsonData = new
                {
                    page = gp.page,
                    rows = jsonData,
                    records = totalRecords,
                    total = totalPages
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(PunishmentOrderDelivery c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string PunishmentOrderServingModeList = form["PunishmentOrderServingModeList"] == "0" ? null : form["PunishmentOrderServingModeList"];
                    string PunishmentOrderServingDate = form["PunishmentOrderServingDate"] == "0" ? "" : form["PunishmentOrderServingDate"];
                    string ShowCauseNoticeList = form["ShowCauseNoticeList"] == "0" ? null : form["ShowCauseNoticeList"];
                    string WitnessList = form["WitnessList"] == "0" ? null : form["WitnessList"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;



                    if (PunishmentOrderServingDate != null)
                    {
                        if (PunishmentOrderServingDate != "")
                        {

                            var val = DateTime.Parse(PunishmentOrderServingDate);
                            c.PunishmentOrderServingDate = val;
                        }
                    }
                    if (PunishmentOrderServingModeList != null)
                    {
                        if (PunishmentOrderServingModeList != "")
                        {
                            var val = db.ChargeSheetServingMode.Find(int.Parse(PunishmentOrderServingModeList));
                            c.PunishmentOrderServingMode = val;

                            var type = db.PunishmentOrderDelivery.Include(e => e.PunishmentOrderServingMode).Where(e => e.Id == data).SingleOrDefault();
                            IList<PunishmentOrderDelivery> typedetails = null;
                            if (type.PunishmentOrderServingMode != null)
                            {
                                typedetails = db.PunishmentOrderDelivery.Where(x => x.PunishmentOrderServingMode.Id == type.PunishmentOrderServingMode.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.PunishmentOrderDelivery.Where(x => x.Id == data).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.PunishmentOrderServingMode = c.PunishmentOrderServingMode;
                                db.PunishmentOrderDelivery.Attach(s);
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
                        var typedetails = db.PunishmentOrderDelivery.Include(e => e.PunishmentOrderServingMode).Where(x => x.Id == data).ToList();
                        foreach (var s in typedetails)
                        {
                            s.PunishmentOrderServingMode = null;
                            db.PunishmentOrderDelivery.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }


                    var db_data1 = db.PunishmentOrderDelivery.Include(e => e.Witness).Include(e => e.Witness.Select(w => w.WitnessEmp)).Where(e => e.Id == data).SingleOrDefault();
                    List<Witness> punishmentorderdeliverywitness = new List<Witness>();

                    if (WitnessList != null)
                    {
                        var ids = Utility.StringIdsToListIds(WitnessList);
                        foreach (var ca in ids)
                        {
                            // var punishmentorderdeliverywitness_val = db.EmployeeIR.Find(ca);
                            int witID = Convert.ToInt32(ca);
                            var punishmentorderdeliverywitness_val = db.EmployeeIR.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id == witID).SingleOrDefault();

                            c.DBTrack = new DBTrack { Action = "M", ModifiedBy = SessionManager.UserName, IsModified = true, ModifiedOn = DateTime.Now };
                            if (punishmentorderdeliverywitness_val != null)
                            {
                                Witness Objwit = new Witness()
                                {
                                    WitnessEmp = punishmentorderdeliverywitness_val,
                                    DBTrack = c.DBTrack
                                };
                                punishmentorderdeliverywitness.Add(Objwit);
                            }

                            db_data1.Witness = punishmentorderdeliverywitness;
                        }
                    }
                    else
                    {
                        db_data1.Witness = null;
                    }

                    var db_data2 = db.PunishmentOrderDelivery.Include(e => e.PunishmentOrderNotice).Where(e => e.Id == data).SingleOrDefault();
                    List<EmployeeDocuments> punishmentorderdeliverydoc = new List<EmployeeDocuments>();

                    if (ShowCauseNoticeList != null)
                    {
                        var ids = Utility.StringIdsToListIds(ShowCauseNoticeList);
                        foreach (var ca in ids)
                        {
                            var punishmentorderdeliverydoc_val = db.EmployeeDocuments.Find(ca);

                            punishmentorderdeliverydoc.Add(punishmentorderdeliverydoc_val);
                            db_data2.PunishmentOrderNotice = punishmentorderdeliverydoc;
                        }
                    }
                    else
                    {
                        db_data2.PunishmentOrderNotice = null;
                    }


                    if (Auth == false)
                    {
                        //if (ModelState.IsValid)
                        //{
                        try
                        {
                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                PunishmentOrderDelivery blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.PunishmentOrderDelivery.Where(e => e.Id == data).SingleOrDefault();


                                    originalBlogValues = context.Entry(blog).OriginalValues;
                                }

                                c.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };

                                var m1 = db.PunishmentOrderDelivery.Include(e => e.Witness).Where(e => e.Id == data).ToList();
                                foreach (var s in m1)
                                {
                                    // s.AppraisalPeriodCalendar = null;
                                    db.PunishmentOrderDelivery.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                                var CurCorp1 = db.PunishmentOrderDelivery.Find(data);
                                TempData["CurrRowVersion"] = CurCorp1.RowVersion;
                                db.Entry(CurCorp1).State = System.Data.Entity.EntityState.Detached;

                                var m2 = db.PunishmentOrderDelivery.Include(e => e.PunishmentOrderNotice).Where(e => e.Id == data).ToList();
                                foreach (var s in m2)
                                {
                                    // s.AppraisalPeriodCalendar = null;
                                    db.PunishmentOrderDelivery.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                                var CurCorp2 = db.PunishmentOrderDelivery.Find(data);
                                TempData["CurrRowVersion"] = CurCorp2.RowVersion;
                                db.Entry(CurCorp2).State = System.Data.Entity.EntityState.Detached;

                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    // c.DBTrack = dbT;
                                    PunishmentOrderDelivery corp = new PunishmentOrderDelivery()
                                    {
                                        IsClosedServing = c.IsClosedServing,
                                        IsNoticeRecd = c.IsNoticeRecd,
                                        IsWitnessReqd = c.IsWitnessReqd,
                                        PunishmentOrderServingDate = c.PunishmentOrderServingDate,
                                        Narration = c.Narration,
                                        Id = data,
                                        DBTrack = c.DBTrack,
                                        Witness = db_data1.Witness

                                    };

                                    db.PunishmentOrderDelivery.Attach(corp);
                                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated  ");
                                    //  return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.LocationObj.LocDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }


                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (PunishmentOrderDelivery)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (PunishmentOrderDelivery)databaseEntry.ToObject();
                                c.RowVersion = databaseValues.RowVersion;

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
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            PunishmentOrderDelivery blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;


                            using (var context = new DataBaseContext())
                            {
                                blog = context.PunishmentOrderDelivery.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            PunishmentOrderDelivery corp = new PunishmentOrderDelivery()
                            {

                                IsClosedServing = c.IsClosedServing,
                                IsNoticeRecd = c.IsNoticeRecd,
                                IsWitnessReqd = c.IsWitnessReqd,
                                PunishmentOrderServingDate = c.PunishmentOrderServingDate,
                                Narration = c.Narration,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };
                            blog.DBTrack = c.DBTrack;
                            db.PunishmentOrderDelivery.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, c.LocationObj.LocDesc, "Record Updated", JsonRequestBehavior.AllowGet });
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

    }
}