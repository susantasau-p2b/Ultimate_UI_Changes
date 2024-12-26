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
    public class PunishmentOrderApealReplyController : Controller
    {
        //
        // GET: /PunishmentOrderApealReply/
        public ActionResult Index()
        {
            return View("~/Views/IR/MainViews/PunishmentOrderApealReply/Index.cshtml");
        }
        public class P2BGridData
        {
            public string VictimName { get; set; }
            public string CaseNo { get; set; }
            public string ProceedingStage { get; set; }
            public string Id { get; set; }
            public string Narration { get; set; }
            public string PunishmentOrderApealReplyDate { get; set; }
            public string PunishmentOrderApealReplyDetails { get; set; }
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
        [HttpPost]
        public ActionResult Create(PunishmentOrderApealReply c, FormCollection form, string EmpIr)
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
                    string caseNO = Convert.ToString(Session["findcase"]);
                    int EmpIrid = Convert.ToInt32(EmpIr);
                    string PunishmentOrderApealReplyDate = form["PunishmentOrderApealReplyDate"] == "0" ? "" : form["PunishmentOrderApealReplyDate"];
                    if (PunishmentOrderApealReplyDate != null && PunishmentOrderApealReplyDate != "")
                    {
                        var val = DateTime.Parse(PunishmentOrderApealReplyDate);
                        c.PunishmentOrderApealReplyDate = val;
                    }
                    string PunishmentOrderApealDoc = form["PunishmentOrderApealReplyDocList"] == "0" ? null : form["PunishmentOrderApealReplyDocList"];
                    if (PunishmentOrderApealDoc != null && PunishmentOrderApealDoc != "")
                    {
                        var ids = Utility.StringIdsToListIds(PunishmentOrderApealDoc);
                        List<EmployeeDocuments> lookupval = new List<EmployeeDocuments>();
                        foreach (var ca in ids)
                        {
                            var EmployeeDocuments_val = db.EmployeeDocuments.Find(ca);
                            lookupval.Add(EmployeeDocuments_val);
                            c.PunishmentOrderApealReplyDoc = lookupval;
                        }
                    }
                    else
                        c.PunishmentOrderApealReplyDoc = null;
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            #region Already PunishmentOrderApealReply enquiry records exist checking code
                            var EMPIR = db.EmployeeIR.Select(s => new
                            {
                                Irid = s.Id,
                                oEmpcode = s.Employee.EmpCode,
                                EDP = s.EmpDisciplineProcedings.Select(r => new
                                {
                                    EDPid = r.Id,
                                    CaseNum = r.CaseNo,
                                    POAR = r.PunishmentOrderApealReply,
                                }).Where(e => e.CaseNum == caseNO).ToList(),

                            }).Where(e => e.Irid == EmpIrid).SingleOrDefault();


                            if (EMPIR != null)
                            {
                                var chkEMPIR = EMPIR.EDP.ToList();

                                foreach (var itemC in chkEMPIR.Where(e => e.POAR != null))
                                {
                                    if (itemC.EDPid != 0 && itemC.CaseNum != null && itemC.POAR != null)
                                    {
                                        Msg.Add("Record already exist for case no : " + itemC.CaseNum + " & victim :: " + EMPIR.oEmpcode);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            #endregion

                            PunishmentOrderApealReply PunishmentOrderApealReply = new PunishmentOrderApealReply()
                            {

                                Narration = c.Narration,
                                PunishmentOrderApealReplyDetails = c.PunishmentOrderApealReplyDetails,
                                PunishmentOrderApealReplyDate = c.PunishmentOrderApealReplyDate,
                                PunishmentOrderApealReplyDoc = c.PunishmentOrderApealReplyDoc,
                                DBTrack = c.DBTrack
                            };
                            var PunishmentOrderApealReplyValidation = ValidateObj(PunishmentOrderApealReply);
                            if (PunishmentOrderApealReplyValidation.Count > 0)
                            {
                                foreach (var item in PunishmentOrderApealReplyValidation)
                                {

                                    Msg.Add("PunishmentOrderApealReply" + item);
                                }
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            db.PunishmentOrderApealReply.Add(PunishmentOrderApealReply);

                            try
                            {

                                db.SaveChanges();

                                #region  Add NEW Stages in EmpDisciplineProcedings

                                //var EmpDisciplines = db.EmpDisciplineProcedings.Include(e => e.MisconductComplaint)
                                //    .Include(e => e.PreminaryEnquiry).Include(e => e.PreminaryEnquiryAction)
                                //    .Include(e => e.ChargeSheet).Include(e => e.ChargeSheetServing)
                                //    .Include(e => e.ChargeSheetReply).Include(e => e.ChargeSheetEnquiryNotice)
                                //    .Include(e => e.ChargeSheetEnquiryNoticeServing).Include(e => e.ChargeSheetEnquiryProceedings)
                                //    .Include(e => e.ChargeSheetEnquiryReport).Include(e => e.PostEnquiryPrerquisite).Include(e => e.FinalShowCauseNotice)
                                //    .Include(e => e.FinalShowCauseNoticeServing).Include(e => e.FinalShowCauseNoticeReply).Include(e => e.FinalShowCauseNoticeClarification)
                                //    .Include(e => e.FinalShowCauseNoticeClarificarionServing)
                                //    .Include(e => e.PunishmentOrder).Include(e => e.PunishmentOrderDelivery)
                                //    .Include(e => e.PunishmentOrderApeal)
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
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.PunishmentOrderDelivery))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.PunishmentOrderApeal))
                                    

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
                                    ProceedingStage = 20,
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
                                    PunishmentOrderDelivery = EmpDisciplines.PunishmentOrderDelivery,
                                    PunishmentOrderApeal = EmpDisciplines.PunishmentOrderApeal,
                                    PunishmentOrderApealReply = db.PunishmentOrderApealReply.Find(PunishmentOrderApealReply.Id),
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

                IEnumerable<P2BGridData> PunishmentOrderApealReply = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;
                //var oemployeedata = db.PunishmentOrderApealReply.OrderBy(e => e.Id).ToList();

                var oemployeedata = db.EmployeeIR.Select(a => new
                {
                    oVictimName = a.Employee.EmpName.FullNameFML,

                    oEmpDiscipline = a.EmpDisciplineProcedings.Select(b => new
                    {
                        oCaseNo = b.CaseNo,
                        oProceedStage = b.ProceedingStage.ToString(),
                        oPunishmentOrderApealReplyDate = b.PunishmentOrderApealReply.PunishmentOrderApealReplyDate,
                        oPunishmentOrderApealReplyDetails = b.PunishmentOrderApealReply.PunishmentOrderApealReplyDetails,

                        oNarration = b.PunishmentOrderApealReply.Narration,
                        oId = b.PunishmentOrderApealReply.Id.ToString(),

                    }).ToList(),

                }).ToList();

                foreach (var itemIR in oemployeedata.Where(e => e.oEmpDiscipline.Count() > 0))
                {
                    foreach (var item in itemIR.oEmpDiscipline.Where(e => e.oProceedStage == "20").OrderBy(o => o.oCaseNo))
                    {
                        view = new P2BGridData()
                        {
                            CaseNo = item.oCaseNo,
                            VictimName = itemIR.oVictimName.ToString(),
                            ProceedingStage = item.oProceedStage.ToString(),
                            PunishmentOrderApealReplyDate = item.oPunishmentOrderApealReplyDate != null ? item.oPunishmentOrderApealReplyDate.Value.ToString("dd/MM/yyyy") : "",
                            Id = item.oId.ToString(),
                            PunishmentOrderApealReplyDetails = item.oPunishmentOrderApealReplyDetails.ToString(),
                            Narration = item.oNarration.ToString(),

                        };

                        model.Add(view);
                    }
                }

                PunishmentOrderApealReply = model;

                IEnumerable<P2BGridData> IE;

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = PunishmentOrderApealReply;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE
                         .Where((e => (e.CaseNo.ToString().Contains(gp.searchString))
                                           || (e.VictimName.ToString().ToUpper().Contains(gp.searchString))
                                          || (e.ProceedingStage.ToString().ToUpper().Contains(gp.searchString))
                             || (e.Narration.ToString().ToUpper().Contains(gp.searchString))
                                    || (e.PunishmentOrderApealReplyDate.ToString().Contains(gp.searchString.ToUpper()))
                                   || (e.PunishmentOrderApealReplyDetails.ToString().Contains(gp.searchString.ToUpper()))


                                  || (e.Id.ToString().Contains(gp.searchString))
                                 )).ToList()
                        .Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.PunishmentOrderApealReplyDate, a.PunishmentOrderApealReplyDetails, a.Narration, a.Id });
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.PunishmentOrderApealReplyDate, a.PunishmentOrderApealReplyDetails, a.Narration, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();


                }
                else
                {
                    IE = PunishmentOrderApealReply;
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
                                         gp.sidx == "PunishmentOrderApealReplyDate" ? c.PunishmentOrderApealReplyDate.ToString() :

                                        gp.sidx == "PunishmentOrderApealReplyDetails" ? c.PunishmentOrderApealReplyDetails.ToString() :

                                        gp.sidx == "Narration" ? c.Narration.ToString() : "");

                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, Convert.ToString(a.PunishmentOrderApealReplyDate), a.PunishmentOrderApealReplyDetails, a.Narration, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.PunishmentOrderApealReplyDate, a.PunishmentOrderApealReplyDetails, a.Narration, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.PunishmentOrderApealReplyDate, a.PunishmentOrderApealReplyDetails, a.Narration, a.Id }).ToList();
                    }
                    totalRecords = PunishmentOrderApealReply.Count();
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
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    PunishmentOrderApealReply corporates = db.PunishmentOrderApealReply

                                                                .Include(e => e.PunishmentOrderApealReplyDoc)
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
        public class returnEditClass
        {
            public Array Employeedoc_Id { get; set; }
            public Array EmployeedocFullDetails { get; set; }
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.PunishmentOrderApealReply

                                   .Include(e => e.PunishmentOrderApealReplyDoc)
                                  .Where(e => e.Id == data).AsEnumerable()
                    .Select(e => new
                    {
                        PunishmentOrderApealReplyDate = e.PunishmentOrderApealReplyDate,
                        PunishmentOrderApealReplyDetails = e.PunishmentOrderApealReplyDetails,
                        Narration = e.Narration,
                    }).ToList();


                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();
                var k = db.PunishmentOrderApealReply.Include(e => e.PunishmentOrderApealReplyDoc)
                   .Include(e => e.PunishmentOrderApealReplyDoc.Select(a => a.DocumentType))
                  .Where(e => e.Id == data && e.PunishmentOrderApealReplyDoc.Count > 0).ToList();
                foreach (var e in k)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        Employeedoc_Id = e.PunishmentOrderApealReplyDoc.Select(a => a.Id.ToString()).ToArray(),
                        EmployeedocFullDetails = e.PunishmentOrderApealReplyDoc.Select(a => a.FullDetails).ToArray()
                    });
                }

                return Json(new Object[] { returndata, oreturnEditClass, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(PunishmentOrderApealReply c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string PunishmentOrderApealDoc = form["PunishmentOrderApealReplyDocList"] == "0" ? null : form["PunishmentOrderApealReplyDocList"];

                    string PunishmentOrderApealReplyDate = form["PunishmentOrderApealReplyDate"] == "0" ? "" : form["PunishmentOrderApealReplyDate"];

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    if (PunishmentOrderApealReplyDate != null)
                    {
                        if (PunishmentOrderApealReplyDate != "")
                        {

                            var val = DateTime.Parse(PunishmentOrderApealReplyDate);
                            c.PunishmentOrderApealReplyDate = val;
                        }
                    }

                    var db_data = db.PunishmentOrderApealReply.Include(e => e.PunishmentOrderApealReplyDoc).Where(e => e.Id == data).SingleOrDefault();
                    List<EmployeeDocuments> punishmentorderapealreplydoc = new List<EmployeeDocuments>();
                    if (PunishmentOrderApealDoc != null)
                    {
                        var ids = Utility.StringIdsToListIds(PunishmentOrderApealDoc);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.EmployeeDocuments.Find(ca);

                            punishmentorderapealreplydoc.Add(Lookup_val);
                            db_data.PunishmentOrderApealReplyDoc = punishmentorderapealreplydoc;
                        }
                    }
                    else
                    {
                        db_data.PunishmentOrderApealReplyDoc = null;
                    }

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    PunishmentOrderApealReply blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.PunishmentOrderApealReply.Where(e => e.Id == data).SingleOrDefault();


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

                                    var m1 = db.PunishmentOrderApealReply.Include(e => e.PunishmentOrderApealReplyDoc).Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.PunishmentOrderApealReply.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    var CurCorp = db.PunishmentOrderApealReply.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        // c.DBTrack = dbT;
                                        PunishmentOrderApealReply corp = new PunishmentOrderApealReply()
                                        {
                                            Narration = c.Narration,
                                            PunishmentOrderApealReplyDetails = c.PunishmentOrderApealReplyDetails,
                                            PunishmentOrderApealReplyDate = c.PunishmentOrderApealReplyDate,
                                            PunishmentOrderApealReplyDoc = c.PunishmentOrderApealReplyDoc,
                                            Id = data,
                                            DBTrack = c.DBTrack

                                        };


                                        db.PunishmentOrderApealReply.Attach(corp);
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
                                var clientValues = (PunishmentOrderApealReply)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (PunishmentOrderApealReply)databaseEntry.ToObject();
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
                            // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            PunishmentOrderApealReply blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;


                            using (var context = new DataBaseContext())
                            {
                                blog = context.PunishmentOrderApealReply.Where(e => e.Id == data).SingleOrDefault();
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
                            PunishmentOrderApealReply corp = new PunishmentOrderApealReply()
                            {

                                Narration = c.Narration,
                                PunishmentOrderApealReplyDetails = c.PunishmentOrderApealReplyDetails,
                                PunishmentOrderApealReplyDate = c.PunishmentOrderApealReplyDate,
                                PunishmentOrderApealReplyDoc = c.PunishmentOrderApealReplyDoc,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };

                            blog.DBTrack = c.DBTrack;
                            db.PunishmentOrderApealReply.Attach(blog);
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