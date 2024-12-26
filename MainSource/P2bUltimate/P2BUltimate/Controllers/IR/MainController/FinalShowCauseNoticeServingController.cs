using System;
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
    public class FinalShowCauseNoticeServingController : Controller
    {
        //
        // GET: /FinalShowCauseNoticeServing/
        public ActionResult Index()
        {
            return View("~/Views/IR/MainViews/FinalShowCauseNoticeServing/Index.cshtml");
        }
        public class P2BGridData
        {
            public string CaseNo { get; set; }
            public string VictimName { get; set; }
            public string ProceedingStage { get; set; }
            public string NoticeServingDate { get; set; }
            public string Narration { get; set; }
            public string IsNoticeRecd { get; set; }
            public string IsClosedServing { get; set; }
            public string Id { get; set; }

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
        public ActionResult GetLookupDetailsEmployeeDoc(List<int> SkipIds)
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
        public ActionResult GetLookupChargeSheetServingMode(string data)
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
        [HttpPost]
        public ActionResult Create(FinalShowCauseNoticeServing c, FormCollection form, string EmpIr) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    var companypayroll = new CompanyPayroll();
                    companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == company_Id).SingleOrDefault();
                    int EmpIrid = Convert.ToInt32(EmpIr);
                    string caseNO = Convert.ToString(Session["findcase"]);
                    string NoticeServingDate = form["NoticeServingDate"] == "0" ? "" : form["NoticeServingDate"];
                    string ShowCauseNoticelist = form["ShowCauseNoticelist"] == "0" ? null : form["ShowCauseNoticelist"];
                    string Witness = form["Witnesslist"] == "0" ? null : form["Witnesslist"];
                    string ShowCauseServingMode = form["ShowCauseServingModelist"] == "0" ? null : form["ShowCauseServingModelist"];

                    if (ShowCauseNoticelist != null && ShowCauseNoticelist != "")
                    {
                        var ids = Utility.StringIdsToListIds(ShowCauseNoticelist);
                        List<EmployeeDocuments> showcausenotice = new List<EmployeeDocuments>();
                        foreach (var ca in ids)
                        {
                            var showcausenotice_val = db.EmployeeDocuments.Find(ca);
                            showcausenotice.Add(showcausenotice_val);
                            c.ShowCauseNotice = showcausenotice;
                        }
                    }
                    else
                        c.ShowCauseNotice = null;

                    if (Witness != null && Witness != "")
                    {
                        var ids = Utility.StringIdsToListIds(Witness);
                        List<Witness> witnesslist = new List<Witness>();

                        foreach (var ca in ids)
                        {
                            int WitnessIds = Convert.ToInt32(ca);
                            // var val = db.Witness.Include(e => e.WitnessEmp).Where(e => e.Id == WitnessIds).SingleOrDefault();
                            var val = db.EmployeeIR.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id == WitnessIds).SingleOrDefault();
                            if (val != null)
                            {
                                c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };
                                Witness objWit = new Witness()
                                {
                                    WitnessEmp = val,
                                    DBTrack = c.DBTrack
                                };
                                witnesslist.Add(objWit);
                            }
                            c.Witness = witnesslist;
                        }
                    }
                    else
                        c.Witness = null;


                    if (ShowCauseServingMode != null && ShowCauseServingMode != "")
                    {
                        int ShowCauseServingModeId = int.Parse(ShowCauseServingMode);
                        var val = db.ChargeSheetServingMode.Include(e => e.ChargeSheetServingModeName).Where(e => e.Id == ShowCauseServingModeId).SingleOrDefault();
                        c.ShowCauseServingMode = val;
                    }

                    if (NoticeServingDate != null && NoticeServingDate != "")
                    {
                        var val = DateTime.Parse(NoticeServingDate);
                        c.NoticeServingDate = val;
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            #region Already FinalShowCauseNoticeServing enquiry records exist checking code
                            var EMPIR = db.EmployeeIR.Select(s => new
                            {
                                Irid = s.Id,
                                oEmpcode = s.Employee.EmpCode,
                                EDP = s.EmpDisciplineProcedings.Select(r => new
                                {
                                    EDPid = r.Id,
                                    CaseNum = r.CaseNo,
                                    FSNS = r.FinalShowCauseNoticeServing,
                                }).Where(e => e.CaseNum == caseNO).ToList(),

                            }).Where(e => e.Irid == EmpIrid).SingleOrDefault();


                            if (EMPIR != null)
                            {
                                var chkEMPIR = EMPIR.EDP.ToList();

                                foreach (var itemC in chkEMPIR.Where(e => e.FSNS != null))
                                {
                                    if (itemC.EDPid != 0 && itemC.CaseNum != null && itemC.FSNS != null)
                                    {
                                        Msg.Add("Record already exist for case no : " + itemC.CaseNum + " & victim :: " + EMPIR.oEmpcode);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            #endregion

                            FinalShowCauseNoticeServing FinalShowCauseNoticeServing = new FinalShowCauseNoticeServing()
                            {
                                NoticeServingDate = c.NoticeServingDate,
                                IsClosedServing = c.IsClosedServing,
                                IsNoticeRecd = c.IsNoticeRecd,
                                IsWitnessReqd = c.IsWitnessReqd,
                                Narration = c.Narration,
                                ShowCauseNotice = c.ShowCauseNotice,
                                ShowCauseServingMode = c.ShowCauseServingMode,
                                Witness = c.Witness,
                                DBTrack = c.DBTrack
                            };


                            var FinalShowCauseNoticeServingValidation = ValidateObj(FinalShowCauseNoticeServing);
                            if (FinalShowCauseNoticeServingValidation.Count > 0)
                            {
                                foreach (var item in FinalShowCauseNoticeServingValidation)
                                {

                                    Msg.Add("FinalShowCauseNoticeServing" + item);
                                }
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            db.FinalShowCauseNoticeServing.Add(FinalShowCauseNoticeServing);

                            try
                            {
                                db.SaveChanges();

                                #region  Add NEW Stages in EmpDisciplineProcedings

                                //var EmpDisciplines = db.EmpDisciplineProcedings.Include(e => e.MisconductComplaint)
                                //    .Include(e => e.PreminaryEnquiry).Include(e => e.PreminaryEnquiryAction)
                                //    .Include(e => e.ChargeSheet)
                                //    .Include(e => e.ChargeSheetReply).Include(e => e.ChargeSheetEnquiryNotice)
                                //    .Include(e => e.ChargeSheetEnquiryNoticeServing).Include(e => e.ChargeSheetEnquiryProceedings)
                                //    .Include(e => e.ChargeSheetEnquiryReport).Include(e => e.PostEnquiryPrerquisite).Include(e => e.FinalShowCauseNotice)
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
                                    ProceedingStage = 13,
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
                                    FinalShowCauseNoticeServing = db.FinalShowCauseNoticeServing.Find(FinalShowCauseNoticeServing.Id),
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
                    FinalShowCauseNoticeServing corporates = db.FinalShowCauseNoticeServing
                                                            .Include(e => e.ShowCauseNotice)
                                                            .Include(e => e.Witness)
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

                IEnumerable<P2BGridData> FinalShowCauseNoticeServing = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;
                //var oemployeedata = db.FinalShowCauseNoticeServing.OrderBy(e => e.Narration).ToList();
                var oemployeedata = db.EmployeeIR.Select(a => new
                {
                    oVictimName = a.Employee.EmpName.FullNameFML,

                    oEmpDiscipline = a.EmpDisciplineProcedings.Select(b => new
                    {
                        oCaseNo = b.CaseNo,
                        oProceedStage = b.ProceedingStage.ToString(),
                        oNoticeServingDate = b.FinalShowCauseNoticeServing.NoticeServingDate,
                        oIsNoticeRecd = b.FinalShowCauseNoticeServing.IsNoticeRecd.ToString(),
                        oIsClosedServing = b.FinalShowCauseNoticeServing.IsClosedServing.ToString(),
                        
                        oNarration = b.FinalShowCauseNoticeServing.Narration,
                        oId = b.FinalShowCauseNoticeServing.Id.ToString(),

                    }).ToList(),

                }).Where(e => e.oEmpDiscipline.Count() > 0).ToList();


                foreach (var itemIR in oemployeedata.Where(e => e.oEmpDiscipline.Count() > 0))
                {
                    foreach (var item in itemIR.oEmpDiscipline.Where(e => e.oProceedStage == "13").OrderBy(o => o.oCaseNo))
                    {
                        view = new P2BGridData()
                        {
                            CaseNo = item.oCaseNo,
                            VictimName = itemIR.oVictimName.ToString(),
                            ProceedingStage = item.oProceedStage.ToString(),
                            NoticeServingDate = item.oNoticeServingDate != null ? item.oNoticeServingDate.Value.ToString("dd/MM/yyyy") : "",
                            Id = item.oId.ToString(),
                            Narration = item.oNarration.ToString(),
                            IsNoticeRecd = item.oIsNoticeRecd.ToString(),
                            IsClosedServing = item.oIsClosedServing.ToString(),

                        };

                        model.Add(view);
                    }
                }

                FinalShowCauseNoticeServing = model;

                IEnumerable<P2BGridData> IE;

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = FinalShowCauseNoticeServing;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE
                         .Where((e => (e.CaseNo.ToString().Contains(gp.searchString))
                                           || (e.VictimName.ToString().ToUpper().Contains(gp.searchString))
                                          || (e.ProceedingStage.ToString().ToUpper().Contains(gp.searchString))
                             || (e.IsClosedServing.ToString().ToUpper().Contains(gp.searchString))

                                    || (e.IsNoticeRecd.ToString().Contains(gp.searchString.ToUpper()))
                                   || (e.NoticeServingDate.ToString().Contains(gp.searchString.ToUpper()))

                                   || (e.Narration.ToString().ToUpper().Contains(gp.searchString))

                                  || (e.Id.ToString().Contains(gp.searchString))
                                 )).ToList()
                        .Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.IsClosedServing, a.IsNoticeRecd, a.NoticeServingDate, a.Narration, a.Id });
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.IsClosedServing, a.IsNoticeRecd, a.NoticeServingDate, a.Narration, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();


                }
                else
                {
                    IE = FinalShowCauseNoticeServing;
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : "0");
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "CaseNo" ? c.CaseNo.ToString() :
                                         gp.sidx == "VictimName" ? c.VictimName.ToString() :
                                         gp.sidx == "ProceedingStage" ? c.ProceedingStage.ToString() :
                            gp.sidx == "IsClosedServing" ? c.IsClosedServing.ToString() :

                                        gp.sidx == "IsNoticeRecd" ? c.IsNoticeRecd.ToString() :
                                        gp.sidx == "NoticeServingDate" ? c.NoticeServingDate.ToString() :
                                        gp.sidx == "Narration" ? c.Narration.ToString() : "");

                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.IsClosedServing, a.IsNoticeRecd, Convert.ToString(a.NoticeServingDate), a.Narration, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.IsClosedServing, a.IsNoticeRecd, a.NoticeServingDate, a.Narration, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.IsClosedServing, a.IsNoticeRecd, a.NoticeServingDate, a.Narration, a.Id }).ToList();
                    }
                    totalRecords = FinalShowCauseNoticeServing.Count();
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
        public class returnEditclass
        {
            public Array shownoticedoc_Id { get; set; }
            public Array shownoticedocfulldetails { get; set; }
            public Array witness_Id { get; set; }
            public Array witnessfulldetails { get; set; }
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.FinalShowCauseNoticeServing
                                   .Include(e => e.ShowCauseServingMode)
                                   .Include(e => e.ShowCauseServingMode.ChargeSheetServingModeName)
                                  .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        Narration = e.Narration,
                        IsClosedServing = e.IsClosedServing,
                        IsNoticeRecd = e.IsNoticeRecd,
                        IsWitnessReqd = e.IsWitnessReqd,
                        NoticeServingDate = e.NoticeServingDate,
                        ShowCauseServingMode_Id = e.ShowCauseServingMode != null ? e.ShowCauseServingMode.Id : 0,
                        ShowCauseServingMode = e.ShowCauseServingMode.ChargeSheetServingModeName.LookupVal + "," + e.ShowCauseServingMode.ServingSeq == null ? "" : e.ShowCauseServingMode.ChargeSheetServingModeName.LookupVal + "," + e.ShowCauseServingMode.ServingSeq
                    }).ToList();

                List<returnEditclass> onreturnEditclass = new List<returnEditclass>();
                var k = db.FinalShowCauseNoticeServing.Include(e => e.ShowCauseNotice)
                    .Include(e => e.ShowCauseNotice.Select(r => r.DocumentType))
                    .Where(e => e.Id == data && e.ShowCauseNotice.Count > 0).ToList();
                foreach (var e in k)
                {
                    onreturnEditclass.Add(new returnEditclass
                    {
                        shownoticedoc_Id = e.ShowCauseNotice.Select(a => a.Id.ToString()).ToArray(),
                        shownoticedocfulldetails = e.ShowCauseNotice.Select(a => a.FullDetails).ToArray()
                    });
                }

                //var m = db.FinalShowCauseNoticeServing.Include(e => e.Witness)
                //    .Include(e => e.Witness.Select(q => q.Employee)).Include(e => e.Witness.Select(q => q.Employee.EmpName))
                //    .Where(e => e.Id == data && e.Witness.Count > 0).ToList();

                var m = db.FinalShowCauseNoticeServing.Include(e => e.Witness)
                    .Include(e => e.Witness.Select(w => w.WitnessEmp))
                    .Include(e => e.Witness.Select(q => q.WitnessEmp.Employee)).Include(e => e.Witness.Select(q => q.WitnessEmp.Employee.EmpName))
                    .Where(e => e.Id == data && e.Witness.Count > 0).ToList();
                foreach (var e in m)
                {
                    onreturnEditclass.Add(new returnEditclass
                    {
                        witness_Id = e.Witness.Select(a => a.Id.ToString()).ToArray(),
                        witnessfulldetails = e.Witness.Select(a => a.WitnessEmp.Employee.EmpName.FullNameFML).ToArray()
                    });
                }
                return Json(new Object[] { returndata, onreturnEditclass, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(FinalShowCauseNoticeServing c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string NoticeServingDate = form["NoticeServingDate"] == "0" ? "" : form["NoticeServingDate"];
                    string ShowCauseNoticelist = form["ShowCauseNoticelist"] == "0" ? null : form["ShowCauseNoticelist"];
                    string Witness = form["Witnesslist"] == "0" ? null : form["Witnesslist"];
                    string ShowCauseServingMode = form["ShowCauseServingModelist"] == "0" ? null : form["ShowCauseServingModelist"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    if (NoticeServingDate != null)
                    {
                        if (NoticeServingDate != "")
                        {

                            var val = DateTime.Parse(NoticeServingDate);
                            c.NoticeServingDate = val;
                        }
                    }
                    if (ShowCauseServingMode != null)
                    {
                        if (ShowCauseServingMode != "")
                        {
                            var val = db.ChargeSheetServingMode.Find(int.Parse(ShowCauseServingMode));
                            c.ShowCauseServingMode = val;

                            var type = db.FinalShowCauseNoticeServing.Include(e => e.ShowCauseServingMode).Where(e => e.Id == data).SingleOrDefault();
                            IList<FinalShowCauseNoticeServing> typedetails = null;
                            if (type.ShowCauseServingMode != null)
                            {
                                typedetails = db.FinalShowCauseNoticeServing.Where(x => x.ShowCauseServingMode.Id == type.ShowCauseServingMode.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.FinalShowCauseNoticeServing.Where(x => x.Id == data).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.ShowCauseServingMode = c.ShowCauseServingMode;
                                db.FinalShowCauseNoticeServing.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        else
                        {
                            var SrmdTypeDetails = db.FinalShowCauseNoticeServing.Include(e => e.ShowCauseServingMode).Where(x => x.Id == data).ToList();
                            foreach (var s in SrmdTypeDetails)
                            {
                                s.ShowCauseServingMode = null;
                                db.FinalShowCauseNoticeServing.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }

                    var db_data = db.FinalShowCauseNoticeServing.Include(e => e.Witness).Include(e => e.Witness.Select(w => w.WitnessEmp)).Where(e => e.Id == data).SingleOrDefault();
                    List<Witness> witnessdoc = new List<Witness>();
                    if (Witness != null)
                    {
                        var ids = Utility.StringIdsToListIds(Witness);
                        foreach (var ca in ids)
                        {
                            int WitnessIds = Convert.ToInt32(ca);
                            var witness_val = db.EmployeeIR.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id == WitnessIds).SingleOrDefault();
                            // var witness_val = db.EmployeeIR.Find(ca);
                            c.DBTrack = new DBTrack { Action = "M", ModifiedBy = SessionManager.UserName, IsModified = true, ModifiedOn = DateTime.Now };
                            if (witness_val != null)
                            {
                                Witness w = new Witness()
                                {
                                    WitnessEmp = witness_val,
                                    DBTrack = c.DBTrack
                                };

                                witnessdoc.Add(w);

                            }

                            db_data.Witness = witnessdoc;
                        }
                    }
                    else
                    {
                        db_data.Witness = null;
                    }

                    var db_data1 = db.FinalShowCauseNoticeServing.Include(e => e.ShowCauseNotice).Where(e => e.Id == data).SingleOrDefault();
                    List<EmployeeDocuments> showcausenoticedoc = new List<EmployeeDocuments>();
                    if (ShowCauseNoticelist != null)
                    {
                        var ids = Utility.StringIdsToListIds(ShowCauseNoticelist);
                        foreach (var ca in ids)
                        {
                            var showcausenoticedoc_val = db.EmployeeDocuments.Find(ca);

                            showcausenoticedoc.Add(showcausenoticedoc_val);
                            db_data.ShowCauseNotice = showcausenoticedoc;
                        }
                    }
                    else
                    {
                        db_data.ShowCauseNotice = null;
                    }


                    if (Auth == false)
                    {

                        try
                        {
                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                FinalShowCauseNoticeServing blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.FinalShowCauseNoticeServing.Where(e => e.Id == data).SingleOrDefault();


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

                                var m = db.FinalShowCauseNoticeServing.Include(e => e.Witness).Where(e => e.Id == data).ToList();
                                foreach (var s in m)
                                {
                                    // s.AppraisalPeriodCalendar = null;
                                    db.FinalShowCauseNoticeServing.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                                var m1 = db.FinalShowCauseNoticeServing.Include(e => e.ShowCauseNotice).Where(e => e.Id == data).ToList();
                                foreach (var s in m1)
                                {
                                    // s.AppraisalPeriodCalendar = null;
                                    db.FinalShowCauseNoticeServing.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                                var CurCorp = db.FinalShowCauseNoticeServing.Find(data);
                                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    // c.DBTrack = dbT;
                                    FinalShowCauseNoticeServing corp = new FinalShowCauseNoticeServing()
                                    {
                                        NoticeServingDate = c.NoticeServingDate,
                                        IsClosedServing = c.IsClosedServing,
                                        IsNoticeRecd = c.IsNoticeRecd,
                                        IsWitnessReqd = c.IsWitnessReqd,
                                        Narration = c.Narration,
                                        DBTrack = c.DBTrack,
                                        Witness = db_data.Witness,
                                        Id = data,

                                    };

                                    db.FinalShowCauseNoticeServing.Attach(corp);
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
                            var clientValues = (FinalShowCauseNoticeServing)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (FinalShowCauseNoticeServing)databaseEntry.ToObject();
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

                            FinalShowCauseNoticeServing blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;


                            using (var context = new DataBaseContext())
                            {
                                blog = context.FinalShowCauseNoticeServing.Where(e => e.Id == data).SingleOrDefault();
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
                            FinalShowCauseNoticeServing corp = new FinalShowCauseNoticeServing()
                            {

                                NoticeServingDate = c.NoticeServingDate,
                                IsClosedServing = c.IsClosedServing,
                                IsNoticeRecd = c.IsNoticeRecd,
                                IsWitnessReqd = c.IsWitnessReqd,
                                Narration = c.Narration,
                                DBTrack = c.DBTrack,
                                Id = data,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };

                            blog.DBTrack = c.DBTrack;
                            db.FinalShowCauseNoticeServing.Attach(blog);
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