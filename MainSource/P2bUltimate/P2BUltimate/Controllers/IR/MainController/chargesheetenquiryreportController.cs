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
    public class chargesheetenquiryreportController : Controller
    {
        //
        // GET: /chargesheetenquiryreport/
        public ActionResult Index()
        {
            return View("~/Views/IR/MainViews/chargesheetenquiryreport/Index.cshtml");
        }
        public class P2BGridData
        {
            public string VictimName { get; set; }
            public string CaseNo { get; set; }

            public string ProceedingStage { get; set; }
            public string Id { get; set; }
            public string IsCloseCase { get; set; }
            public string IsEmpGuilty { get; set; }
            public string IsNotifyHRDept { get; set; }

            public string ReportSubmissionDate { get; set; }
            public string EnquiryReportDetails { get; set; }
            public string SuspensionRevokeDate { get; set; }
            public string CaseCloseDate { get; set; }
            public string Narration { get; set; }
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
        [HttpPost]
        public ActionResult Create(ChargeSheetEnquiryReport c, FormCollection form, string EmpIr)
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
                    string Narration = form["Narration"] == "0" ? "" : form["Narration"];
                    string EnquiryReportDetails = form["EnquiryReportDetails"] == "0" ? "" : form["EnquiryReportDetails"];
                    string ReportSubmissionDate = form["ReportSubmissionDate"] == "0" ? "" : form["ReportSubmissionDate"];
                    if (ReportSubmissionDate != null && ReportSubmissionDate != "")
                    {
                        var val = DateTime.Parse(ReportSubmissionDate);
                        c.ReportSubmissionDate = val;
                    }
                    string SuspensionRevokeDate = form["SuspensionRevokeDate"] == "0" ? "" : form["SuspensionRevokeDate"];
                    if (SuspensionRevokeDate != null && SuspensionRevokeDate != "")
                    {
                        var val = DateTime.Parse(SuspensionRevokeDate);
                        c.SuspensionRevokeDate = val;
                    }
                    else
                    {
                        c.SuspensionRevokeDate = null;
                    }
                    string CaseCloseDate = form["CaseCloseDate"] == "0" ? "" : form["CaseCloseDate"];
                    if (CaseCloseDate != null && CaseCloseDate != "")
                    {
                        var val = DateTime.Parse(CaseCloseDate);
                        c.CaseCloseDate = val;
                    }
                    else
                    {
                        c.CaseCloseDate = null;
                    }
                    string EmployeeDocuments = form["EnquiryReportDocList"] == "0" ? null : form["EnquiryReportDocList"];
                    if (EmployeeDocuments != null && EmployeeDocuments != "")
                    {
                        var ids = Utility.StringIdsToListIds(EmployeeDocuments);
                        List<EmployeeDocuments> lookupval = new List<EmployeeDocuments>();
                        foreach (var ca in ids)
                        {
                            var EmployeeDocuments_val = db.EmployeeDocuments.Find(ca);
                            lookupval.Add(EmployeeDocuments_val);
                            c.EnquiryReportDoc = lookupval;
                        }
                    }
                    else
                        c.EnquiryReportDoc = null;
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            #region Already ChargeSheetEnquiryReport enquiry records exist checking code
                            var EMPIR = db.EmployeeIR.Select(s => new
                            {
                                Irid = s.Id,
                                oEmpcode = s.Employee.EmpCode,
                                EDP = s.EmpDisciplineProcedings.Select(r => new
                                {
                                    EDPid = r.Id,
                                    CaseNum = r.CaseNo,
                                    CSER = r.ChargeSheetEnquiryReport,
                                }).Where(e => e.CaseNum == caseNO).ToList(),

                            }).Where(e => e.Irid == EmpIrid).SingleOrDefault();


                            if (EMPIR != null)
                            {
                                var chkEMPIR = EMPIR.EDP.ToList();

                                foreach (var itemC in chkEMPIR.Where(e => e.CSER != null))
                                {
                                    if (itemC.EDPid != 0 && itemC.CaseNum != null && itemC.CSER != null)
                                    {
                                        Msg.Add("Record already exist for case no : " + itemC.CaseNum + " & victim :: " + EMPIR.oEmpcode);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            #endregion

                            ChargeSheetEnquiryReport ChargeSheetEnquiryReport = new ChargeSheetEnquiryReport()
                            {
                                Narration = c.Narration,
                                EnquiryReportDoc = c.EnquiryReportDoc,
                                EnquiryReportDetails = c.EnquiryReportDetails,
                                ReportSubmissionDate = c.ReportSubmissionDate,
                                SuspensionRevokeDate = c.SuspensionRevokeDate,
                                CaseCloseDate = c.CaseCloseDate,
                                IsCloseCase = c.IsCloseCase,
                                IsEmpGuilty = c.IsEmpGuilty,
                                IsNotifyHRDept = c.IsNotifyHRDept,
                                DBTrack = c.DBTrack
                            };
                            var ChargeSheetEnquiryReportValidation = ValidateObj(ChargeSheetEnquiryReport);
                            if (ChargeSheetEnquiryReportValidation.Count > 0)
                            {
                                foreach (var item in ChargeSheetEnquiryReportValidation)
                                {

                                    Msg.Add("ChargeSheetEnquiryReport" + item);
                                }
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            db.ChargeSheetEnquiryReport.Add(ChargeSheetEnquiryReport);

                            try
                            {


                                db.SaveChanges();

                                #region  Add NEW Stages in EmpDisciplineProcedings


                                //var EmpDisciplines = db.EmpDisciplineProcedings.Include(e => e.MisconductComplaint)
                                //     .Include(e => e.PreminaryEnquiry).Include(e => e.PreminaryEnquiryAction)
                                //     .Include(e => e.ChargeSheet)
                                //     .Include(e => e.ChargeSheetReply).Include(e => e.ChargeSheetEnquiryNotice).Include(e => e.ChargeSheetEnquiryNoticeServing)
                                //     .Include(e => e.ChargeSheetEnquiryProceedings)
                                //     .Where(e => e.CaseNo == caseNO).ToList().LastOrDefault();

                                var EmpDisciplines = db.EmployeeIR.Include(e => e.EmpDisciplineProcedings)
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.MisconductComplaint))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.PreminaryEnquiry))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.PreminaryEnquiryAction))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheet))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheetReply))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheetEnquiryNotice))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheetEnquiryNoticeServing))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheetEnquiryProceedings))

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
                                    ProceedingStage = 10,
                                    MisconductComplaint = EmpDisciplines.MisconductComplaint,
                                    PreminaryEnquiry = EmpDisciplines.PreminaryEnquiry,
                                    PreminaryEnquiryAction = EmpDisciplines.PreminaryEnquiryAction,
                                    ChargeSheet = EmpDisciplines.ChargeSheet,
                                    ChargeSheetServing = getEmpdisciplineForChargesheetserving.ChargeSheetServing.ToList(),
                                    ChargeSheetReply = EmpDisciplines.ChargeSheetReply,
                                    ChargeSheetEnquiryNotice = EmpDisciplines.ChargeSheetEnquiryNotice,
                                    ChargeSheetEnquiryNoticeServing = EmpDisciplines.ChargeSheetEnquiryNoticeServing,
                                    ChargeSheetEnquiryProceedings = EmpDisciplines.ChargeSheetEnquiryProceedings,
                                    ChargeSheetEnquiryReport = db.ChargeSheetEnquiryReport.Find(ChargeSheetEnquiryReport.Id),
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

                IEnumerable<P2BGridData> ChargeSheetEnquiryReport = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;
                // var oemployeedata = db.ChargeSheetEnquiryReport.OrderBy(e => e.Narration).ToList();
                var oemployeedata = db.EmployeeIR.Select(a => new
                {
                    oVictimName = a.Employee.EmpName.FullNameFML,

                    oEmpDiscipline = a.EmpDisciplineProcedings.Select(b => new
                    {
                        oCaseNo = b.CaseNo,
                        oProceedStage = b.ProceedingStage.ToString(),
                        oReportSubmissionDate = b.ChargeSheetEnquiryReport.ReportSubmissionDate,
                        oSuspensionRevokeDate = b.ChargeSheetEnquiryReport.SuspensionRevokeDate,
                        oCaseCloseDate = b.ChargeSheetEnquiryReport.CaseCloseDate,
                        oIsCloseCase = b.ChargeSheetEnquiryReport.IsCloseCase.ToString(),
                        oIsEmpGuilty = b.ChargeSheetEnquiryReport.IsEmpGuilty.ToString(),
                        oIsNotifyHRDept = b.ChargeSheetEnquiryReport.IsNotifyHRDept.ToString(),

                        oNarration = b.ChargeSheetEnquiryReport.Narration,
                        oId = b.ChargeSheetEnquiryReport.Id.ToString(),

                    }).ToList(),

                }).ToList();

                foreach (var itemIR in oemployeedata.Where(e => e.oEmpDiscipline.Count() > 0))
                {
                    foreach (var item in itemIR.oEmpDiscipline.Where(e => e.oProceedStage == "10").OrderBy(o => o.oCaseNo))
                    {
                        view = new P2BGridData()
                  {
                      CaseNo = item.oCaseNo,
                      VictimName = itemIR.oVictimName.ToString(),
                      ProceedingStage = item.oProceedStage.ToString(),
                      ReportSubmissionDate =item.oReportSubmissionDate != null ? item.oReportSubmissionDate.Value.ToString("dd/MM/yyyy") : "",
                      SuspensionRevokeDate = item.oSuspensionRevokeDate != null ? item.oSuspensionRevokeDate.Value.ToString("dd/MM/yyyy") : null,
                      CaseCloseDate = item.oCaseCloseDate != null ? item.oCaseCloseDate.Value.ToString("dd/MM/yyyy") : null,
                      Id = item.oId.ToString(),

                      Narration = item.oNarration.ToString(),
                      IsCloseCase = item.oIsCloseCase.ToString(),
                      IsEmpGuilty = item.oIsEmpGuilty.ToString(),
                      IsNotifyHRDept = item.oIsNotifyHRDept.ToString(),
                  };
                        model.Add(view);
                    }
                }

                ChargeSheetEnquiryReport = model;

                IEnumerable<P2BGridData> IE;


                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = ChargeSheetEnquiryReport;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE
                         .Where((e => (e.CaseNo.ToString().Contains(gp.searchString))
                                           || (e.VictimName.ToString().ToUpper().Contains(gp.searchString))
                                          || (e.ProceedingStage.ToString().ToUpper().Contains(gp.searchString))
                             || (e.ReportSubmissionDate.ToString().ToUpper().Contains(gp.searchString))
                               || (e.SuspensionRevokeDate.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.CaseCloseDate.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                  || (e.IsCloseCase.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                   || (e.IsEmpGuilty.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                    || (e.IsNotifyHRDept.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                        || (e.Narration.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                  || (e.Id.ToString().Contains(gp.searchString))
                                 )).ToList()
                        .Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ReportSubmissionDate, a.SuspensionRevokeDate, a.CaseCloseDate, a.IsCloseCase, a.IsEmpGuilty, a.IsNotifyHRDept, a.Narration, a.Id });
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ReportSubmissionDate, a.SuspensionRevokeDate, a.CaseCloseDate, a.IsCloseCase, a.IsEmpGuilty, a.IsNotifyHRDept, a.Narration, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();


                }
                else
                {
                    IE = ChargeSheetEnquiryReport;
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
                            gp.sidx == "ReportSubmissionDate" ? c.ReportSubmissionDate.ToString() :
                             gp.sidx == "SuspensionRevokeDate" ? c.SuspensionRevokeDate.ToString() :
                              gp.sidx == "CaseCloseDate" ? c.CaseCloseDate.ToString() :
                               gp.sidx == "IsCloseCase" ? c.IsCloseCase.ToString() :
                                gp.sidx == "IsEmpGuilty" ? c.IsEmpGuilty.ToString() :
                                 gp.sidx == "IsNotifyHRDept" ? c.IsNotifyHRDept.ToString() :

                            gp.sidx == "Narration" ? c.Narration.ToString() : "");


                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, Convert.ToString(a.ReportSubmissionDate), Convert.ToString(a.SuspensionRevokeDate), Convert.ToString(a.CaseCloseDate), a.IsCloseCase, a.IsEmpGuilty, a.IsNotifyHRDept, a.Narration, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ReportSubmissionDate, a.SuspensionRevokeDate, a.CaseCloseDate, a.IsCloseCase, a.IsEmpGuilty, a.IsNotifyHRDept, a.Narration, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ReportSubmissionDate, a.SuspensionRevokeDate, a.CaseCloseDate, a.IsCloseCase, a.IsEmpGuilty, a.IsNotifyHRDept, a.Narration, a.Id }).ToList();
                    }
                    totalRecords = ChargeSheetEnquiryReport.Count();
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
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    ChargeSheetEnquiryReport corporates = db.ChargeSheetEnquiryReport
                                                             .Include(e => e.EnquiryReportDoc)

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
            public Array EmployeeReportdoc_Id { get; set; }
            public Array EmployeeReportdocFullDetails { get; set; }
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.ChargeSheetEnquiryReport

                                  .Include(e => e.EnquiryReportDoc)

                                  .Where(e => e.Id == data).AsEnumerable()
                    .Select(e => new
                    {
                        Narration = e.Narration,
                        ReportSubmissionDate = e.ReportSubmissionDate,
                        SuspensionRevokeDate = e.SuspensionRevokeDate,
                        CaseCloseDate = e.CaseCloseDate,
                        IsCloseCase = e.IsCloseCase,
                        IsEmpGuilty = e.IsEmpGuilty,
                        IsNotifyHRDept = e.IsNotifyHRDept,
                    }).ToList();

                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();
                var k = db.ChargeSheetEnquiryReport.Include(e => e.EnquiryReportDoc)
                   .Include(e => e.EnquiryReportDoc.Select(a => a.DocumentType))
                  .Where(e => e.Id == data && e.EnquiryReportDoc.Count > 0).ToList();
                foreach (var e in k)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        EmployeeReportdoc_Id = e.EnquiryReportDoc.Select(a => a.Id.ToString()).ToArray(),
                        EmployeeReportdocFullDetails = e.EnquiryReportDoc.Select(a => a.FullDetails).ToArray()
                    });
                }

                return Json(new Object[] { returndata, oreturnEditClass, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(ChargeSheetEnquiryReport c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string ReportSubmissionDate = form["ReportSubmissionDate"] == "0" ? "" : form["ReportSubmissionDate"];
                    string SuspensionRevokeDate = form["SuspensionRevokeDate"] == "0" ? "" : form["SuspensionRevokeDate"];
                    string CaseCloseDate = form["CaseCloseDate"] == "0" ? "" : form["CaseCloseDate"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;



                    if (ReportSubmissionDate != null)
                    {
                        if (ReportSubmissionDate != "")
                        {

                            var val = DateTime.Parse(ReportSubmissionDate);
                            c.ReportSubmissionDate = val;
                        }
                    }
                    if (SuspensionRevokeDate != null)
                    {
                        if (SuspensionRevokeDate != "")
                        {

                            var val = DateTime.Parse(SuspensionRevokeDate);
                            c.SuspensionRevokeDate = val;
                        }
                    }

                    if (CaseCloseDate != null)
                    {
                        if (CaseCloseDate != "")
                        {

                            var val = DateTime.Parse(CaseCloseDate);
                            c.CaseCloseDate = val;
                        }
                    }

                    var db_data = db.ChargeSheetEnquiryReport.Include(e => e.EnquiryReportDoc).Where(e => e.Id == data).SingleOrDefault();
                    string EnquiryReportDoc = form["EnquiryReportDocList"] == "0" ? null : form["EnquiryReportDocList"];
                    List<EmployeeDocuments> enquiryreportdoc = new List<EmployeeDocuments>();
                    if (EnquiryReportDoc != null)
                    {
                        var ids = Utility.StringIdsToListIds(EnquiryReportDoc);
                        foreach (var ca in ids)
                        {
                            var enquiryreportdoc_val = db.EmployeeDocuments.Find(ca);

                            enquiryreportdoc.Add(enquiryreportdoc_val);

                            db_data.EnquiryReportDoc = enquiryreportdoc;
                        }
                    }
                    else
                    {
                        db_data.EnquiryReportDoc = null;
                    }

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    //ChargeSheetReply blog = null; // to retrieve old data
                                    //DbPropertyValues originalBlogValues = null;

                                    //using (var context = new DataBaseContext())
                                    //{
                                    //    blog = context.ChargeSheetReply.Where(e => e.Id == data).SingleOrDefault();


                                    //    originalBlogValues = context.Entry(blog).OriginalValues;
                                    //}

                                    c.DBTrack = new DBTrack
                                    {
                                        //CreatedBy = db_data.DBTrack.CreatedBy == null ? null : db_data.DBTrack.CreatedBy,
                                        //CreatedOn = db_data.DBTrack.CreatedOn == null ? null : db_data.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    var m1 = db.ChargeSheetEnquiryReport.Include(e => e.EnquiryReportDoc).Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.ChargeSheetEnquiryReport.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    var CurCorp = db.ChargeSheetEnquiryReport.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        // c.DBTrack = dbT;
                                        ChargeSheetEnquiryReport corp = new ChargeSheetEnquiryReport()
                                        {




                                            Narration = c.Narration,
                                            EnquiryReportDetails = c.EnquiryReportDetails,
                                            ReportSubmissionDate = c.ReportSubmissionDate,
                                            SuspensionRevokeDate = c.SuspensionRevokeDate,
                                            CaseCloseDate = c.CaseCloseDate,
                                            IsCloseCase = c.IsCloseCase,
                                            IsEmpGuilty = c.IsEmpGuilty,
                                            IsNotifyHRDept = c.IsNotifyHRDept,
                                            DBTrack = c.DBTrack,
                                            // UnitId=c.UnitId,
                                            Id = data,


                                        };


                                        db.ChargeSheetEnquiryReport.Attach(corp);
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
                                var clientValues = (ChargeSheetEnquiryReport)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (ChargeSheetEnquiryReport)databaseEntry.ToObject();
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

                            //ChargeSheetReply blog = null; // to retrieve old data
                            //DbPropertyValues originalBlogValues = null;


                            //using (var context = new DataBaseContext())
                            //{
                            //    blog = context.ChargeSheetReply.Where(e => e.Id == data).SingleOrDefault();
                            //    originalBlogValues = context.Entry(blog).OriginalValues;
                            //}
                            c.DBTrack = new DBTrack
                            {
                                //CreatedBy = db_data.DBTrack.CreatedBy == null ? null : db_data.DBTrack.CreatedBy,
                                //CreatedOn = db_data.DBTrack.CreatedOn == null ? null : db_data.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = db_data.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            ChargeSheetEnquiryReport corp = new ChargeSheetEnquiryReport()
                            {

                                Narration = c.Narration,
                                EnquiryReportDetails = c.EnquiryReportDetails,
                                ReportSubmissionDate = c.ReportSubmissionDate,
                                SuspensionRevokeDate = c.SuspensionRevokeDate,
                                CaseCloseDate = c.CaseCloseDate,
                                IsCloseCase = c.IsCloseCase,
                                IsEmpGuilty = c.IsEmpGuilty,
                                IsNotifyHRDept = c.IsNotifyHRDept,
                                DBTrack = c.DBTrack,
                                Id = data,

                                RowVersion = (Byte[])TempData["RowVersion"]
                            };




                            //db.ChargeSheetEnquiryReport.Attach(db_data);
                            //db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            //db.Entry(db_data).OriginalValues["RowVersion"] = TempData["RowVersion"];
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