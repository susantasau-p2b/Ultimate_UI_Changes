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
    public class ChargeSheetEnquiryProceedingsController : Controller
    {
        //
        // GET: /chargesheetenquiryproceeding/
        public ActionResult Index()
        {
            return View("~/Views/IR/MainViews/ChargeSheetEnquiryProceedings/Index.cshtml");
        }
        public ActionResult Partial()
        {
            //return View("~/Views/Shared/Core/EmployeeDocuments.cshtml");
            return View("~/views/Shared/IR/_EnquiryPanel.cshtml");
        }
        public class P2BGridData
        {
            public string VictimName { get; set; }
            public string CaseNo { get; set; }
            public string ProceedingStage { get; set; }
            public string EnquiryProceedingDate { get; set; }
            public string Narration { get; set; }
            public string IsEnquiryOver { get; set; }
            public string Id { get; set; }
            public string EnquiryProceedingTime { get; set; }


        }
        public ActionResult GetLookupDetailsEnquirypanelPresent(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.EnquiryPanel.Include(e => e.EnquiryPanelType).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.EnquiryPanel.Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);

            }
        }
        public ActionResult GetLookupDetailsWitnessPresent(List<int> SkipIds)
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
        public ActionResult Create(ChargeSheetEnquiryProceedings c, FormCollection form, string EmpIr)
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
                    string EnquiryProceedingDate = form["EnquiryProceedingDate"] == "0" ? "" : form["EnquiryProceedingDate"];
                    if (EnquiryProceedingDate != null && EnquiryProceedingDate != "")
                    {
                        var val = DateTime.Parse(EnquiryProceedingDate);
                        c.EnquiryProceedingDate = val;
                    }
                    string EnquiryProceedingTime = form["EnquiryProceedingTime"] == "0" ? "" : form["EnquiryProceedingTime"];
                    if (EnquiryProceedingTime != null && EnquiryProceedingTime != "")
                    {
                        var val = DateTime.Parse(EnquiryProceedingTime);
                        c.EnquiryProceedingTime = val;
                    }
                    var IsEmpReport = form["IsEmpReport"];
                    c.IsEmpReport = Convert.ToBoolean(IsEmpReport);

                    var IsEnquiryOver = form["IsEnquiryOver"];
                    c.IsEnquiryOver = Convert.ToBoolean(IsEnquiryOver);

                    string WitnessPresentList = form["WitnessPresentList"] == "" ? null : form["WitnessPresentList"];

                    if (WitnessPresentList != null && WitnessPresentList != "")
                    {
                        var ids = Utility.StringIdsToListIds(WitnessPresentList);
                        List<Witness> witness = new List<Witness>();
                        foreach (var ca in ids)
                        {
                            // var Employee_val = db.EmployeeIR.Find(ca);
                            int witID = Convert.ToInt32(ca);
                            var Employee_val = db.EmployeeIR.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id == witID).SingleOrDefault();
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };

                            if (Employee_val != null)
                            {
                                Witness objWit = new Witness()
                                {
                                    WitnessEmp = Employee_val,
                                    DBTrack = c.DBTrack
                                };
                                witness.Add(objWit);
                            }

                            c.WitnessPresent = witness;
                        }
                    }
                    else
                        c.WitnessPresent = null;


                    string EnquiryPanelPresentList = form["EnquiryPanelPresentList"] == "" ? null : form["EnquiryPanelPresentList"];

                    if (EnquiryPanelPresentList != null && EnquiryPanelPresentList != "")
                    {
                        var ids = Utility.StringIdsToListIds(EnquiryPanelPresentList);
                        List<EnquiryPanel> enquirypanelpresent = new List<EnquiryPanel>();
                        foreach (var ca in ids)
                        {
                            var EnquiryPanel_val = db.EnquiryPanel.Find(ca);
                            enquirypanelpresent.Add(EnquiryPanel_val);
                            c.EnquiryPanelPresent = enquirypanelpresent;
                        }
                    }
                    else
                        c.EnquiryPanelPresent = null;

                    string EmployeeDocuments = form["EmployeeDocumentsList"] == "0" ? null : form["EmployeeDocumentsList"];
                    if (EmployeeDocuments != null && EmployeeDocuments != "")
                    {
                        var ids = Utility.StringIdsToListIds(EmployeeDocuments);
                        List<EmployeeDocuments> lookupval = new List<EmployeeDocuments>();
                        foreach (var ca in ids)
                        {
                            var EmployeeDocuments_val = db.EmployeeDocuments.Find(ca);
                            lookupval.Add(EmployeeDocuments_val);
                            c.ProceedingProofDocuments = lookupval;
                        }
                    }
                    else
                        c.ProceedingProofDocuments = null;

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            #region Already ChargeSheetEnquiryProceedings enquiry records exist checking code
                            var EMPIR = db.EmployeeIR.Select(s => new
                            {
                                Irid = s.Id,
                                oEmpcode = s.Employee.EmpCode,
                                EDP = s.EmpDisciplineProcedings.Select(r => new
                                {
                                    EDPid = r.Id,
                                    CaseNum = r.CaseNo,
                                    CSEP = r.ChargeSheetEnquiryProceedings,
                                }).Where(e => e.CaseNum == caseNO).ToList(),

                            }).Where(e => e.Irid == EmpIrid).SingleOrDefault();


                            if (EMPIR != null)
                            {
                                var chkEMPIR = EMPIR.EDP.ToList();

                                foreach (var itemC in chkEMPIR.Where(e => e.CSEP != null))
                                {
                                    if (itemC.EDPid != 0 && itemC.CaseNum != null && itemC.CSEP != null)
                                    {
                                        Msg.Add("Record already exist for case no : " + itemC.CaseNum + " & victim :: " + EMPIR.oEmpcode);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            #endregion

                            ChargeSheetEnquiryProceedings ChargeSheetEnquiryProceedings = new ChargeSheetEnquiryProceedings()
                            {
                                EnquiryProceedingDate = c.EnquiryProceedingDate,
                                EnquiryProceedingTime = c.EnquiryProceedingTime,
                                IsEmpReport = c.IsEmpReport,
                                ProceedingProofDocuments = c.ProceedingProofDocuments,
                                IsEnquiryOver = c.IsEnquiryOver,
                                WitnessPresent = c.WitnessPresent,
                                EnquiryPanelPresent = c.EnquiryPanelPresent,
                                Narration = c.Narration,
                                DBTrack = c.DBTrack
                            };


                            var ChargeSheetEnquiryProceedingsValidation = ValidateObj(ChargeSheetEnquiryProceedings);
                            if (ChargeSheetEnquiryProceedingsValidation.Count > 0)
                            {
                                foreach (var item in ChargeSheetEnquiryProceedingsValidation)
                                {

                                    Msg.Add("ChargeSheetEnquiryProceedings" + item);
                                }
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            db.ChargeSheetEnquiryProceedings.Add(ChargeSheetEnquiryProceedings);

                            try
                            {

                                db.SaveChanges();

                                #region  Add NEW Stages in EmpDisciplineProcedings

                                //var EmpDisciplines = db.EmpDisciplineProcedings.Include(e => e.MisconductComplaint)
                                //    .Include(e => e.PreminaryEnquiry).Include(e => e.PreminaryEnquiryAction)
                                //    .Include(e => e.ChargeSheet).Include(e => e.ChargeSheetServing)
                                //    .Include(e => e.ChargeSheetReply).Include(e => e.ChargeSheetEnquiryNotice).Include(e => e.ChargeSheetEnquiryNoticeServing)
                                //    .Where(e => e.CaseNo == caseNO).ToList().LastOrDefault();
                                var EmpDisciplines = db.EmployeeIR.Include(e => e.EmpDisciplineProcedings)
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.MisconductComplaint))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.PreminaryEnquiry))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.PreminaryEnquiryAction))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheet))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheetReply))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheetEnquiryNotice))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheetEnquiryNoticeServing))

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
                                    ProceedingStage = EmpDisciplines.ProceedingStage + 1,
                                    MisconductComplaint = EmpDisciplines.MisconductComplaint,
                                    PreminaryEnquiry = EmpDisciplines.PreminaryEnquiry,
                                    PreminaryEnquiryAction = EmpDisciplines.PreminaryEnquiryAction,
                                    ChargeSheet = EmpDisciplines.ChargeSheet,
                                    ChargeSheetServing = getEmpdisciplineForChargesheetserving.ChargeSheetServing.ToList(),
                                    ChargeSheetReply = EmpDisciplines.ChargeSheetReply,
                                    ChargeSheetEnquiryNotice = EmpDisciplines.ChargeSheetEnquiryNotice,
                                    ChargeSheetEnquiryNoticeServing = EmpDisciplines.ChargeSheetEnquiryNoticeServing,
                                    ChargeSheetEnquiryProceedings = db.ChargeSheetEnquiryProceedings.Find(ChargeSheetEnquiryProceedings.Id),
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

                            }

                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
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

                IEnumerable<P2BGridData> ChargeSheetEnquiryProceedings = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;
                // var oemployeedata = db.ChargeSheetEnquiryProceedings.ToList();

                var oemployeedata = db.EmployeeIR.Select(a => new
                {
                    oVictimName = a.Employee.EmpName.FullNameFML,

                    oEmpDiscipline = a.EmpDisciplineProcedings.Select(b => new
                    {
                        oCaseNo = b.CaseNo,
                        oProceedStage = b.ProceedingStage.ToString(),
                        oEnquiryProceedingDate = b.ChargeSheetEnquiryProceedings.EnquiryProceedingDate,
                        oEnquiryProceedingTime = b.ChargeSheetEnquiryProceedings.EnquiryProceedingTime,
                        oIsEnquiryOver = b.ChargeSheetEnquiryProceedings.IsEnquiryOver.ToString(),

                        oNarration = b.ChargeSheetEnquiryProceedings.Narration,
                        oId = b.ChargeSheetEnquiryProceedings.Id.ToString(),

                    }).ToList(),

                }).ToList();

                foreach (var itemIR in oemployeedata.Where(e => e.oEmpDiscipline.Count() > 0))
                {
                    foreach (var item in itemIR.oEmpDiscipline.Where(e => e.oProceedStage == "9").OrderBy(o => o.oCaseNo))
                    {

                        view = new P2BGridData()
                        {
                            CaseNo = item.oCaseNo,
                            VictimName = itemIR.oVictimName.ToString(),
                            ProceedingStage = item.oProceedStage.ToString(),
                            EnquiryProceedingDate = item.oEnquiryProceedingDate != null ? item.oEnquiryProceedingDate.Value.ToString("dd/MM/yyyy") : "",
                            EnquiryProceedingTime = item.oEnquiryProceedingTime.Value.ToShortTimeString(),
                            IsEnquiryOver = item.oIsEnquiryOver.ToString(),
                            Narration = item.oNarration.ToString(),
                            Id = item.oId.ToString(),

                        };

                        model.Add(view);
                    }
                }


                ChargeSheetEnquiryProceedings = model;

                IEnumerable<P2BGridData> IE;

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = ChargeSheetEnquiryProceedings;
                    if (gp.searchOper.Equals("eq"))
                    {

                        jsonData = IE
                         .Where((e => (e.CaseNo.ToString().Contains(gp.searchString))
                                           || (e.VictimName.ToString().ToUpper().Contains(gp.searchString))
                                          || (e.ProceedingStage.ToString().ToUpper().Contains(gp.searchString))
                                  || (e.EnquiryProceedingDate.ToString().ToUpper().Contains(gp.searchString))
                                  || (e.EnquiryProceedingTime.ToString().ToUpper().Contains(gp.searchString))
                                  || (e.IsEnquiryOver.ToString().ToUpper().Contains(gp.searchString))
                                  || (e.Narration.ToString().ToUpper().Contains(gp.searchString))
                                  || (e.Id.ToString().Contains(gp.searchString))
                                 )).ToList()
                        .Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.EnquiryProceedingDate, a.EnquiryProceedingTime, a.IsEnquiryOver, a.Narration, a.Id });
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, Convert.ToString(a.EnquiryProceedingDate), Convert.ToString(a.EnquiryProceedingTime), Convert.ToString(a.IsEnquiryOver), a.Narration, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();


                }
                else
                {
                    IE = ChargeSheetEnquiryProceedings;
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
                                              gp.sidx == "EnquiryProceedingDate" ? c.EnquiryProceedingDate.ToString() :
                                              gp.sidx == "EnquiryProceedingTime" ? c.EnquiryProceedingTime.ToString() :
                                              gp.sidx == "IsEnquiryOver" ? c.IsEnquiryOver.ToString() :
                                              gp.sidx == "Narration" ? c.Narration.ToString() : "");

                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, Convert.ToString(a.EnquiryProceedingDate), Convert.ToString(a.EnquiryProceedingTime), Convert.ToString(a.IsEnquiryOver), a.Narration, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, Convert.ToString(a.EnquiryProceedingDate), Convert.ToString(a.EnquiryProceedingTime), Convert.ToString(a.IsEnquiryOver), a.Narration, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, Convert.ToString(a.EnquiryProceedingDate), Convert.ToString(a.EnquiryProceedingTime), Convert.ToString(a.IsEnquiryOver), a.Narration, a.Id }).ToList();
                    }
                    totalRecords = ChargeSheetEnquiryProceedings.Count();
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
                    ChargeSheetEnquiryProceedings corporates = db.ChargeSheetEnquiryProceedings
                                                         .Include(e => e.EnquiryPanelPresent)
                                                            .Include(e => e.ProceedingProofDocuments)
                                                                .Include(e => e.WitnessPresent)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    //Address add = corporates.Address;
                    //ContactDetails conDet = corporates.ContactDetails;
                    //LookupValue val = corporates.BusinessType;
                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
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
                            //corporates.DBTrack = dbT;
                            //db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack);
                            //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            //DT_Corp.Address_Id = corporates.Address == null ? 0 : corporates.Address.Id;
                            //DT_Corp.BusinessType_Id = corporates.BusinessType == null ? 0 : corporates.BusinessType.Id;
                            //DT_Corp.ContactDetails_Id = corporates.ContactDetails == null ? 0 : corporates.ContactDetails.Id;
                            //db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
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
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(corporates..Select(e => e.Id));
                            //    if (corpRegion.Count > 0)
                            //    {
                            //        Msg.Add(" Child record exists.Cannot remove it..  ");
                            //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //        // return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //    }
                            //}


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
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            //DT_Corp.Address_Id = add == null ? 0 : add.Id;
                            //DT_Corp.BusinessType_Id = val == null ? 0 : val.Id;
                            //DT_Corp.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
                            //db.Create(DT_Corp);

                            await db.SaveChangesAsync();


                            //using (var context = new DataBaseContext())
                            //{
                            //    corporates.Address = add;
                            //    corporates.ContactDetails = conDet;
                            //    corporates.BusinessType = val;
                            //    DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", dbT);
                            //}
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
            public Array Employeedoc_Id { get; set; }
            public Array EmployeedocFullDetails { get; set; }
            public Array Witness_Id { get; set; }
            public Array WitnessFullDetails { get; set; }
            public Array Enquirypanel_Id { get; set; }
            public Array EnquirypanelFullDetails { get; set; }

        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.ChargeSheetEnquiryProceedings
                                 .Include(e => e.EnquiryPanelPresent)
                                   .Include(e => e.ProceedingProofDocuments)
                                   .Include(e => e.WitnessPresent)
                                  .Where(e => e.Id == data).AsEnumerable()
                    .Select(e => new
                    {
                        EnquiryProceedingDate = e.EnquiryProceedingDate,
                        EnquiryProceedingTime = e.EnquiryProceedingTime.Value.ToShortTimeString(),
                        IsEmpReport = e.IsEmpReport,
                        Narration = e.Narration,
                        IsEnquiryOver = e.IsEnquiryOver,
                    }).ToList();

                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();
                var k = db.ChargeSheetEnquiryProceedings.Include(e => e.ProceedingProofDocuments)
                   .Include(e => e.ProceedingProofDocuments.Select(a => a.DocumentType))
                  .Where(e => e.Id == data && e.ProceedingProofDocuments.Count > 0).ToList();
                foreach (var e in k)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        Employeedoc_Id = e.ProceedingProofDocuments.Select(a => a.Id.ToString()).ToArray(),
                        EmployeedocFullDetails = e.ProceedingProofDocuments.Select(a => a.FullDetails).ToArray()
                    });
                }
                var m = db.ChargeSheetEnquiryProceedings.Include(e => e.WitnessPresent)
                    .Include(e => e.WitnessPresent.Select(a => a.WitnessEmp))
                   .Include(e => e.WitnessPresent.Select(a => a.WitnessEmp.Employee)).Include(e => e.WitnessPresent.Select(a => a.WitnessEmp.Employee.EmpName))
                  .Where(e => e.Id == data && e.WitnessPresent.Count > 0).ToList();
                foreach (var e in m)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        Witness_Id = e.WitnessPresent.Select(a => a.Id.ToString()).ToArray(),
                        WitnessFullDetails = e.WitnessPresent.Select(a => a.WitnessEmp.Employee.EmpName.FullNameFML).ToArray()
                    });
                }
                var p = db.ChargeSheetEnquiryProceedings.Include(e => e.EnquiryPanelPresent)
                  .Include(e => e.EnquiryPanelPresent.Select(a => a.EnquiryPanelType))
                 .Where(e => e.Id == data && e.EnquiryPanelPresent.Count > 0).ToList();
                foreach (var e in p)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        Enquirypanel_Id = e.EnquiryPanelPresent.Select(a => a.Id.ToString()).ToArray(),
                        EnquirypanelFullDetails = e.EnquiryPanelPresent.Select(a => a.FullDetails).ToArray()
                    });
                }


                return Json(new Object[] { returndata, oreturnEditClass, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(ChargeSheetEnquiryProceedings c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string EnquiryProceedingDate = form["EnquiryProceedingDate"] == "0" ? "" : form["EnquiryProceedingDate"];
                    string WitnessPresentList = form["WitnessPresentList"] == "" ? null : form["WitnessPresentList"];
                    string EnquiryPanelPresentList = form["EnquiryPanelPresentList"] == "" ? null : form["EnquiryPanelPresentList"];
                    string EmployeeDocuments = form["EmployeeDocumentsList"] == "0" ? null : form["EmployeeDocumentsList"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;



                    if (EnquiryProceedingDate != null)
                    {
                        if (EnquiryProceedingDate != "")
                        {

                            var val = DateTime.Parse(EnquiryProceedingDate);
                            c.EnquiryProceedingDate = val;
                        }
                    }

                    var db_data1 = db.ChargeSheetEnquiryProceedings.Include(e => e.WitnessPresent).Where(e => e.Id == data).SingleOrDefault();
                    List<Witness> employee = new List<Witness>();

                    if (WitnessPresentList != null)
                    {
                        var ids = Utility.StringIdsToListIds(WitnessPresentList);
                        foreach (var ca in ids)
                        {
                            int witID = Convert.ToInt32(ca);
                            c.DBTrack = new DBTrack { Action = "M", ModifiedBy = SessionManager.UserName, IsModified = true, ModifiedOn = DateTime.Now };
                            // var employee_val = db.EmployeeIR.Find(ca);
                            var employee_val = db.EmployeeIR.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id == witID).SingleOrDefault();
                            if (employee_val != null)
                            {
                                Witness objWit = new Witness()
                                {
                                    WitnessEmp = employee_val,
                                    DBTrack = c.DBTrack
                                };
                                employee.Add(objWit);
                            }

                            db_data1.WitnessPresent = employee;
                        }
                    }
                    else
                    {
                        db_data1.WitnessPresent = null;
                    }
                    var db_data2 = db.ChargeSheetEnquiryProceedings.Include(e => e.EnquiryPanelPresent).Where(e => e.Id == data).SingleOrDefault();
                    List<EnquiryPanel> enquirypanel = new List<EnquiryPanel>();

                    if (EnquiryPanelPresentList != null)
                    {
                        var ids = Utility.StringIdsToListIds(EnquiryPanelPresentList);
                        foreach (var ca in ids)
                        {
                            var enquirypanel_val = db.EnquiryPanel.Find(ca);

                            enquirypanel.Add(enquirypanel_val);
                            db_data2.EnquiryPanelPresent = enquirypanel;
                        }
                    }
                    else
                    {
                        db_data2.EnquiryPanelPresent = null;
                    }

                    var db_data3 = db.ChargeSheetEnquiryProceedings.Include(e => e.ProceedingProofDocuments).Where(e => e.Id == data).SingleOrDefault();
                    List<EmployeeDocuments> employeedocument = new List<EmployeeDocuments>();

                    if (EmployeeDocuments != null)
                    {
                        var ids = Utility.StringIdsToListIds(EmployeeDocuments);
                        foreach (var ca in ids)
                        {
                            var employeedocument_val = db.EmployeeDocuments.Find(ca);

                            employeedocument.Add(employeedocument_val);
                            db_data3.ProceedingProofDocuments = employeedocument;
                        }
                    }
                    else
                    {
                        db_data3.ProceedingProofDocuments = null;
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
                                    ChargeSheetEnquiryProceedings blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.ChargeSheetEnquiryProceedings.Where(e => e.Id == data).SingleOrDefault();


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

                                    var m1 = db.ChargeSheetEnquiryProceedings.Include(e => e.ProceedingProofDocuments).Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.ChargeSheetEnquiryProceedings.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }

                                    var m2 = db.ChargeSheetEnquiryProceedings.Include(e => e.WitnessPresent).Where(e => e.Id == data).ToList();
                                    foreach (var s in m2)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.ChargeSheetEnquiryProceedings.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    var m3 = db.ChargeSheetEnquiryProceedings.Include(e => e.EnquiryPanelPresent).Where(e => e.Id == data).ToList();
                                    foreach (var s in m3)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.ChargeSheetEnquiryProceedings.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }


                                    var CurCorp = db.ChargeSheetEnquiryProceedings.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        // c.DBTrack = dbT;
                                        ChargeSheetEnquiryProceedings corp = new ChargeSheetEnquiryProceedings()
                                        {

                                            EnquiryProceedingDate = c.EnquiryProceedingDate,
                                            EnquiryProceedingTime = c.EnquiryProceedingTime,
                                            IsEmpReport = c.IsEmpReport,
                                            IsEnquiryOver = c.IsEnquiryOver,
                                            Narration = c.Narration,
                                            Id = data,
                                            WitnessPresent = db_data1.WitnessPresent,
                                            DBTrack = c.DBTrack
                                        };


                                        db.ChargeSheetEnquiryProceedings.Attach(corp);
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
                                var clientValues = (ChargeSheetEnquiryProceedings)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (ChargeSheetEnquiryProceedings)databaseEntry.ToObject();
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

                            ChargeSheetEnquiryProceedings blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;


                            using (var context = new DataBaseContext())
                            {
                                blog = context.ChargeSheetEnquiryProceedings.Where(e => e.Id == data).SingleOrDefault();
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
                            ChargeSheetEnquiryProceedings corp = new ChargeSheetEnquiryProceedings()
                            {

                                EnquiryProceedingDate = c.EnquiryProceedingDate,
                                EnquiryProceedingTime = c.EnquiryProceedingTime,
                                IsEmpReport = c.IsEmpReport,
                                IsEnquiryOver = c.IsEnquiryOver,
                                Narration = c.Narration,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };



                            blog.DBTrack = c.DBTrack;
                            db.ChargeSheetEnquiryProceedings.Attach(blog);
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