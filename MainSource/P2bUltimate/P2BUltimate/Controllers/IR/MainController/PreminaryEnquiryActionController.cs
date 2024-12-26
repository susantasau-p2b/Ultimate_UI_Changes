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
    public class PreminaryEnquiryActionController : Controller
    {

        public ActionResult Index()
        {
            return View("~/views/IR/MainViews/PreminaryEnquiryAction/Index.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/views/Shared/IR/_EnquiryPanel.cshtml");
        }

        public class P2BGridData
        {
            public string CaseNo { get; set; }
            public string VictimName { get; set; }
            public string ProceedingStage { get; set; }
            public string Id { get; set; }
            public bool IsNotifyHRDept { get; set; }
            public bool IsSuspendEmp { get; set; }
            public string Narration { get; set; }
            public string PreliminaryEnquiryActionDate { get; set; }
            public string SuspensionDate { get; set; }
        }
        [HttpPost]

        public ActionResult GetLookupEnquirypanel(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.EnquiryPanel.Include(x => x.EnquiryPanelType).ToList();
                IEnumerable<EnquiryPanel> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.EnquiryPanel.ToList();
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.EnquiryPanelType }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);

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
        public ActionResult GetPreminaryEnquiry(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string EnquiryPanelRecomm = "";
                string EnquiryPanelPrelimiId = "";
                string EnquiryPanelPrelimi = "";
                if (data != "" && data != null)
                {
                    var query = db.EmpDisciplineProcedings
                        .Include(e => e.PreminaryEnquiry).Include(e => e.PreminaryEnquiry.EnquiryPanel)
                        .Include(e => e.PreminaryEnquiry.EnquiryPanel.EnquiryPanelType)
                       .Where(e => e.CaseNo == data && e.PreminaryEnquiry.Id != null).FirstOrDefault();

                    if (query != null)
                    {
                        EnquiryPanelRecomm = query.PreminaryEnquiry.EnquiryPanelRecommendation.ToString();
                        EnquiryPanelPrelimiId = query.PreminaryEnquiry.EnquiryPanel.Id.ToString();
                        EnquiryPanelPrelimi = query.PreminaryEnquiry.EnquiryPanel.FullDetails.ToString();
                    }
                }

                return Json(new Object[] { EnquiryPanelRecomm, EnquiryPanelPrelimiId, EnquiryPanelPrelimi, JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public ActionResult Create(PreminaryEnquiryAction c, FormCollection form, string EmpIr)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int comp_Id = 0;
                    comp_Id = Convert.ToInt32(Session["CompId"]);
                    string caseNO = Convert.ToString(Session["findcase"]);
                    var Company = new Company();
                    Company = db.Company.Where(e => e.Id == comp_Id).SingleOrDefault();
                    int EmpIrid = Convert.ToInt32(EmpIr);
                    string EnquiryPanel = form["EnquiryPanelList"] == "0" ? null : form["EnquiryPanelList"];
                    if (EnquiryPanel != null && EnquiryPanel != "")
                    {
                        int ContId = Convert.ToInt32(EnquiryPanel);
                        var val = db.EnquiryPanel.Include(e => e.EnquiryPanelType)
                                            .Where(e => e.Id == ContId).SingleOrDefault();
                        c.EnquiryPanel = val;
                    }
                    var IsSuspendEmp = form["IsSuspendEmp"];
                    c.IsSuspendEmp = Convert.ToBoolean(IsSuspendEmp);

                    var IsNotifyHRDept = form["IsNotifyHRDept"];
                    c.IsNotifyHRDept = Convert.ToBoolean(IsNotifyHRDept);

                    string EmployeeDocuments = form["EmployeeDocumentsList"] == "0" ? null : form["EmployeeDocumentsList"];
                    if (EmployeeDocuments != null && EmployeeDocuments != "")
                    {
                        var ids = Utility.StringIdsToListIds(EmployeeDocuments);
                        List<EmployeeDocuments> lookupval = new List<EmployeeDocuments>();
                        foreach (var ca in ids)
                        {
                            var EmployeeDocuments_val = db.EmployeeDocuments.Find(ca);
                            lookupval.Add(EmployeeDocuments_val);
                            c.EmployeeDocuments = lookupval;
                        }
                    }
                    else
                        c.EmployeeDocuments = null;


                    string PreminaryEnquiryActionDate = form["PreminaryEnquiryActionDate"] == "0" ? "" : form["PreminaryEnquiryActionDate"];
                    string SuspensionDate = form["SuspensionDate"] == "0" ? "" : form["SuspensionDate"];
                    if (PreminaryEnquiryActionDate != null && PreminaryEnquiryActionDate != "")
                    {
                        var val = DateTime.Parse(PreminaryEnquiryActionDate);
                        c.PreminaryEnquiryActionDate = val;
                    }
                    if (SuspensionDate != null && SuspensionDate != "")
                    {
                        var val = DateTime.Parse(SuspensionDate);
                        c.SuspensionDate = val;
                    }
                    else
                    {
                        c.SuspensionDate = null;
                    }
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            #region Already preliminary enquiry records exist checking code
                            var EMPIR = db.EmployeeIR.Include(e => e.Employee).Include(e => e.EmpDisciplineProcedings).Include(e => e.EmpDisciplineProcedings.Select(s => s.PreminaryEnquiryAction)).Where(e => e.Id == EmpIrid).SingleOrDefault();
                            var EmpIrId = EMPIR.Id;

                            if (EMPIR != null)
                            {
                                var Empcode = EMPIR.Employee.EmpCode;
                                var chkEMPIR = EMPIR.EmpDisciplineProcedings.Select(a => new
                                {
                                    EDPid = a.Id,
                                    CaseNum = a.CaseNo,
                                    PreEnqAct = a.PreminaryEnquiryAction,

                                }).Where(e => e.CaseNum == caseNO).ToList();

                                foreach (var itemC in chkEMPIR)
                                {
                                    if (itemC.EDPid != 0 && itemC.CaseNum != null && itemC.PreEnqAct != null)
                                    {
                                        Msg.Add("Record already exist for case no : " + itemC.CaseNum + " & victim :: " + Empcode);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            #endregion

                            PreminaryEnquiryAction PreminaryEnquiryAction = new PreminaryEnquiryAction()
                            {
                                PreminaryEnquiryActionDate = c.PreminaryEnquiryActionDate,
                                SuspensionDate = c.SuspensionDate,
                                IsNotifyHRDept = c.IsNotifyHRDept,
                                Narration = c.Narration,
                                IsSuspendEmp = c.IsSuspendEmp,
                                EmployeeDocuments = c.EmployeeDocuments,
                                EnquiryPanel = c.EnquiryPanel,
                                DBTrack = c.DBTrack
                            };


                            var PreminaryEnquiryActionValidation = ValidateObj(PreminaryEnquiryAction);
                            if (PreminaryEnquiryActionValidation.Count > 0)
                            {
                                foreach (var item in PreminaryEnquiryActionValidation)
                                {

                                    Msg.Add("PreminaryEnquiryAction" + item);
                                }
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            db.PreminaryEnquiryAction.Add(PreminaryEnquiryAction);

                            try
                            {

                                db.SaveChanges();

                                #region  Add NEW Stages in EmpDisciplineProcedings

                                //var EmpDisciplines = db.EmpDisciplineProcedings.Include(e => e.MisconductComplaint).Include(e => e.PreminaryEnquiry).Include(e => e.PreminaryEnquiryAction)
                                //    .Where(e => e.CaseNo == caseNO).ToList().LastOrDefault();

                                var EmpDisciplines = db.EmployeeIR.Include(e => e.EmpDisciplineProcedings)
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.MisconductComplaint))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.PreminaryEnquiry))


                                    .Where(e => e.Id == EmpIrid).FirstOrDefault().EmpDisciplineProcedings.Where(e => e.CaseNo == caseNO).FirstOrDefault();

                                EmpDisciplineProcedings EmpDiscipline = new EmpDisciplineProcedings()
                                {
                                    CaseNo = EmpDisciplines.CaseNo,
                                    CaseOpeningDate = EmpDisciplines.CaseOpeningDate,
                                    ProceedingStage = 3,
                                    MisconductComplaint = EmpDisciplines.MisconductComplaint,
                                    PreminaryEnquiry = EmpDisciplines.PreminaryEnquiry,
                                    PreminaryEnquiryAction = PreminaryEnquiryAction,//db.PreminaryEnquiryAction.Find(PreminaryEnquiryAction.Id),
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

                IEnumerable<P2BGridData> PreminaryEnquiryAction = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;
                //var oemployeedata = db.PreminaryEnquiryAction.OrderBy(e => e.Narration).ToList();

                var oemployeedata = db.EmployeeIR.Select(a => new
                {
                    oVictimName = a.Employee.EmpName.FullNameFML,

                    oEmpDiscipline = a.EmpDisciplineProcedings.Select(b => new
                    {
                        oCaseNo = b.CaseNo,
                        oProceedStage = b.ProceedingStage.ToString(),
                        oPreliminaryEnquiryActionDate = b.PreminaryEnquiryAction.PreminaryEnquiryActionDate,
                        oSuspensionDate = b.PreminaryEnquiryAction.SuspensionDate,
                        oIsSuspendEmp = b.PreminaryEnquiryAction != null ? b.PreminaryEnquiryAction.IsSuspendEmp == true ? true : false : false,
                        oIsNotifyHRDept = b.PreminaryEnquiryAction != null ? b.PreminaryEnquiryAction.IsNotifyHRDept == true ? true : false : false,
                        oNarration = b.PreminaryEnquiryAction.Narration,
                        oId = b.PreminaryEnquiryAction.Id.ToString(),

                    }).ToList(),

                }).ToList();


                foreach (var itemIR in oemployeedata.Where(e => e.oEmpDiscipline.Count() > 0))
                {
                    foreach (var item in itemIR.oEmpDiscipline.Where(e => e.oProceedStage == "3").OrderBy(o => o.oCaseNo))
                    {
                        view = new P2BGridData()
                        {
                            CaseNo = item.oCaseNo.ToString(),
                            VictimName = itemIR.oVictimName,
                            ProceedingStage = item.oProceedStage.ToString(),
                            PreliminaryEnquiryActionDate = item.oPreliminaryEnquiryActionDate.Value.ToString("dd/MM/yyyy"),
                            SuspensionDate = item.oSuspensionDate != null ? item.oSuspensionDate.Value.ToString("dd/MM/yyyy") : null,
                            Narration = item.oNarration.ToString(),
                            IsSuspendEmp = item.oIsSuspendEmp,
                            IsNotifyHRDept = item.oIsNotifyHRDept,
                            Id = item.oId.ToString(),

                        };

                        model.Add(view);
                    }
                }


                PreminaryEnquiryAction = model;

                IEnumerable<P2BGridData> IE;

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = PreminaryEnquiryAction;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE
                         .Where((e => (e.IsSuspendEmp.ToString().ToUpper().Contains(gp.searchString))
                             || (e.CaseNo.ToString().Contains(gp.searchString))
                             || (e.VictimName.ToString().ToUpper().Contains(gp.searchString))
                             || (e.ProceedingStage.ToString().ToUpper().Contains(gp.searchString))
                                 || (e.Narration.ToString().ToUpper().Contains(gp.searchString))
                                 || (e.PreliminaryEnquiryActionDate.ToString().ToUpper().Contains(gp.searchString))
                                 || (e.SuspensionDate.ToString().ToUpper().Contains(gp.searchString))
                                    || (e.IsNotifyHRDept.ToString().ToUpper().Contains(gp.searchString))
                                  || (e.Id.ToString().Contains(gp.searchString))
                                 )).ToList()
                        .Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.PreliminaryEnquiryActionDate, a.SuspensionDate, a.IsSuspendEmp, a.Narration, a.IsNotifyHRDept, a.Id });
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.PreliminaryEnquiryActionDate, a.SuspensionDate, Convert.ToString(a.IsSuspendEmp), a.Narration, Convert.ToString(a.IsNotifyHRDept), a.Id }).ToList();
                    }
                    totalRecords = IE.Count();


                }
                else
                {
                    IE = PreminaryEnquiryAction;
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : "0");
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "IsSuspendEmp" ? c.IsSuspendEmp.ToString() :
                            gp.sidx == "CaseNo" ? c.CaseNo.ToString() :
                            gp.sidx == "VictimName" ? c.VictimName.ToString() :
                            gp.sidx == "ProceedingStage" ? c.ProceedingStage.ToString() :
                                          gp.sidx == "IsNotifyHRDept" ? c.IsNotifyHRDept.ToString() :
                                            gp.sidx == "PreminaryEnquiryActionDate" ? c.PreliminaryEnquiryActionDate.ToString() :
                                              gp.sidx == "SuspensionDate" ? c.SuspensionDate.ToString() :
                                           gp.sidx == "Narration" ? c.Narration.ToString() : "");

                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, Convert.ToString(a.PreliminaryEnquiryActionDate), Convert.ToString(a.SuspensionDate), Convert.ToString(a.IsSuspendEmp), a.Narration, Convert.ToString(a.IsNotifyHRDept), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.PreliminaryEnquiryActionDate, a.SuspensionDate, Convert.ToString(a.IsSuspendEmp), a.Narration, Convert.ToString(a.IsNotifyHRDept), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.PreliminaryEnquiryActionDate, a.SuspensionDate, Convert.ToString(a.IsSuspendEmp), a.Narration, Convert.ToString(a.IsNotifyHRDept), a.Id }).ToList();
                    }
                    totalRecords = PreminaryEnquiryAction.Count();
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
                    PreminaryEnquiryAction corporates = db.PreminaryEnquiryAction.Include(e => e.EnquiryPanel)
                                                             .Include(e => e.EmployeeDocuments)
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
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.PreminaryEnquiryAction
                                  .Include(e => e.EnquiryPanel)
                                  .Include(e => e.EnquiryPanel.EnquiryPanelType)
                                  .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        IsSuspendEmp = e.IsSuspendEmp,
                        IsNotifyHRDept = e.IsNotifyHRDept,
                        PreminaryEnquiryActionDate = e.PreminaryEnquiryActionDate,
                        SuspensionDate = e.SuspensionDate,
                        Narration = e.Narration,
                        Enquirypanel_Id = e.EnquiryPanel.Id == null ? "" : e.EnquiryPanel.Id.ToString(),
                        EnquirypanelDetails = e.EnquiryPanel.FullDetails == null ? "" : e.EnquiryPanel.FullDetails,
                    }).ToList();

                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();
                var k = db.PreminaryEnquiryAction.Include(e => e.EmployeeDocuments)
                   .Include(e => e.EmployeeDocuments.Select(a => a.DocumentType))
                  .Where(e => e.Id == data && e.EmployeeDocuments.Count > 0).ToList();
                foreach (var e in k)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        Employeedoc_Id = e.EmployeeDocuments.Select(a => a.Id.ToString()).ToArray(),
                        EmployeedocFullDetails = e.EmployeeDocuments.Select(a => a.FullDetails).ToArray()
                    });
                }

                return Json(new Object[] { returndata, oreturnEditClass, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(PreminaryEnquiryAction c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    string EnquiryPanel = form["EnquiryPanelList"] == "0" ? null : form["EnquiryPanelList"];
                    string EmployeeDocuments = form["EmployeeDocumentsList"] == "0" ? null : form["EmployeeDocumentsList"];
                    string PreminaryEnquiryActionDate = form["PreminaryEnquiryActionDate"] == "0" ? "" : form["PreminaryEnquiryActionDate"];
                    string SuspensionDate = form["SuspensionDate"] == "0" ? "" : form["SuspensionDate"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;



                    if (PreminaryEnquiryActionDate != null)
                    {
                        if (PreminaryEnquiryActionDate != "")
                        {

                            var val = DateTime.Parse(PreminaryEnquiryActionDate);
                            c.PreminaryEnquiryActionDate = val;
                        }
                    }

                    if (SuspensionDate != null)
                    {
                        if (SuspensionDate != "")
                        {

                            var val = DateTime.Parse(SuspensionDate);
                            c.SuspensionDate = val;
                        }
                    }

                    if (EnquiryPanel != null)
                    {
                        if (EnquiryPanel != "")
                        {
                            var val = db.EnquiryPanel.Find(int.Parse(EnquiryPanel));
                            c.EnquiryPanel = val;

                            var add = db.PreminaryEnquiryAction.Include(e => e.EnquiryPanel).Where(e => e.Id == data).SingleOrDefault();
                            IList<PreminaryEnquiryAction> contactsdetails = null;
                            if (add.EnquiryPanel != null)
                            {
                                contactsdetails = db.PreminaryEnquiryAction.Where(x => x.EnquiryPanel.Id == add.EnquiryPanel.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                contactsdetails = db.PreminaryEnquiryAction.Where(x => x.Id == data).ToList();
                            }
                            foreach (var s in contactsdetails)
                            {
                                s.EnquiryPanel = c.EnquiryPanel;
                                db.PreminaryEnquiryAction.Attach(s);
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
                        var contactsdetails = db.PreminaryEnquiryAction.Include(e => e.EnquiryPanel).Where(x => x.Id == data).ToList();
                        foreach (var s in contactsdetails)
                        {
                            s.EnquiryPanel = null;
                            db.PreminaryEnquiryAction.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    var db_data = db.PreminaryEnquiryAction.Include(e => e.EmployeeDocuments).Where(e => e.Id == data).SingleOrDefault();
                    List<EmployeeDocuments> employeedoc = new List<EmployeeDocuments>();

                    if (EmployeeDocuments != null)
                    {
                        var ids = Utility.StringIdsToListIds(EmployeeDocuments);
                        foreach (var ca in ids)
                        {
                            var employeedoc_val = db.EmployeeDocuments.Find(ca);

                            employeedoc.Add(employeedoc_val);
                            db_data.EmployeeDocuments = employeedoc;
                        }
                    }
                    else
                    {
                        db_data.EmployeeDocuments = null;
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
                                    PreminaryEnquiryAction blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.PreminaryEnquiryAction.Where(e => e.Id == data).SingleOrDefault();


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

                                    var m1 = db.PreminaryEnquiryAction.Include(e => e.EmployeeDocuments).Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.PreminaryEnquiryAction.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    var CurCorp = db.PreminaryEnquiryAction.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        // c.DBTrack = dbT;
                                        PreminaryEnquiryAction corp = new PreminaryEnquiryAction()
                                        {


                                            PreminaryEnquiryActionDate = c.PreminaryEnquiryActionDate,
                                            SuspensionDate = c.SuspensionDate,
                                            IsNotifyHRDept = c.IsNotifyHRDept,
                                            Narration = c.Narration,
                                            DBTrack = c.DBTrack,
                                            IsSuspendEmp = c.IsSuspendEmp,
                                            Id = data

                                        };


                                        db.PreminaryEnquiryAction.Attach(corp);
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
                                var clientValues = (PreminaryEnquiryAction)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (PreminaryEnquiryAction)databaseEntry.ToObject();
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

                            PreminaryEnquiryAction blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;


                            using (var context = new DataBaseContext())
                            {
                                blog = context.PreminaryEnquiryAction.Where(e => e.Id == data).SingleOrDefault();
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
                            PreminaryEnquiryAction corp = new PreminaryEnquiryAction()
                            {


                                PreminaryEnquiryActionDate = c.PreminaryEnquiryActionDate,
                                SuspensionDate = c.SuspensionDate,
                                IsNotifyHRDept = c.IsNotifyHRDept,
                                Narration = c.Narration,
                                DBTrack = c.DBTrack,
                                IsSuspendEmp = c.IsSuspendEmp,
                                Id = data,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            blog.DBTrack = c.DBTrack;
                            db.PreminaryEnquiryAction.Attach(blog);
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