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
    public class PreminaryEnquiryController : Controller
    {
        //
        // GET: /PreminaryEnquiry/

        public ActionResult Index()
        {
            return View("~/views/IR/MainViews/PreminaryEnquiry/Index.cshtml");
        }
        public ActionResult Partial()
        {
            //return View("~/Views/Shared/Core/EmployeeDocuments.cshtml");
            return View("~/views/Shared/IR/_EnquiryPanel.cshtml");
        }
        public class P2BGridData
        {
            public string CaseNo { get; set; }
            public string VictimName { get; set; }
            public string ProceedingStage { get; set; }
            public string CaseEnquiryDate { get; set; }
            public string Id { get; set; }
            public bool IsDropCase { get; set; }
            public string Narration { get; set; }
            public string PreliminaryEnquiryDate { get; set; }
            public string EnquiryPanelRecommendation { get; set; }
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
        public ActionResult Getcomplain(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string Complainno = "";
                if (data != "" && data != null)
                {
                    var query = db.EmpDisciplineProcedings
                        .Include(e => e.MisconductComplaint)
                       .Where(e => e.CaseNo == data).FirstOrDefault();

                    if (query != null)
                    {
                        Complainno = query.MisconductComplaint.ComplaintNo.ToString();
                    }
                }

                return Json(new Object[] { Complainno, JsonRequestBehavior.AllowGet });
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

        [HttpPost]
        public ActionResult Create(PreminaryEnquiry c, FormCollection form, string EmpIr)
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
                    string PreminaryEnquiryDate = form["PreminaryEnquiryDate"] == "0" ? "" : form["PreminaryEnquiryDate"];
                    string CaseEnquiryDate = form["CaseEnquiryDate"] == "0" ? "" : form["CaseEnquiryDate"];
                    var EmpDisciplineProcedings = new EmpDisciplineProcedings();
                    if (PreminaryEnquiryDate != null && PreminaryEnquiryDate != "")
                    {
                        var val = DateTime.Parse(PreminaryEnquiryDate);
                        c.PreminaryEnquiryDate = val;
                    }
                    if (CaseEnquiryDate != null && CaseEnquiryDate != "")
                    {
                        var val = DateTime.Parse(CaseEnquiryDate);
                        c.CaseEnquiryDate = val;
                    }
                    string EnquiryPanel = form["EnquiryPanelList"] == "" ? null : form["EnquiryPanelList"];
                    if (EnquiryPanel != null && EnquiryPanel != "")
                    {
                        int EnquiryPanelId = int.Parse(EnquiryPanel);
                        var val = db.EnquiryPanel.Include(e => e.EnquiryPanelType).Where(e => e.Id == EnquiryPanelId).SingleOrDefault();
                        c.EnquiryPanel = val;
                    }

                    string EmployeeDocuments = form["EmployeeDocumentslList"] == "0" ? null : form["EmployeeDocumentslList"];
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
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            #region Already preliminary enquiry records exist checking code
                            var EMPIR = db.EmployeeIR.Include(e => e.Employee).Include(e => e.EmpDisciplineProcedings).Include(e => e.EmpDisciplineProcedings.Select(s => s.PreminaryEnquiry)).Where(e => e.Id == EmpIrid).SingleOrDefault();
                            var EmpIrId = EMPIR.Id;

                            if (EMPIR != null)
                            {
                                var Empcode = EMPIR.Employee.EmpCode;
                                var chkEMPIR = EMPIR.EmpDisciplineProcedings.Select(a => new
                                {
                                    EDPid = a.Id,
                                    CaseNum = a.CaseNo,
                                    PreEnq = a.PreminaryEnquiry,

                                }).Where(e => e.CaseNum == caseNO).ToList();

                                foreach (var itemC in chkEMPIR)
                                {
                                    if (itemC.EDPid != 0 && itemC.CaseNum != null && itemC.PreEnq != null)
                                    {
                                        Msg.Add("Record already exist for case no : " + itemC.CaseNum + " & victim :: " + Empcode);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            #endregion

                            PreminaryEnquiry PreminaryEnquiry = new PreminaryEnquiry()
                            {
                                EnquiryPanelRecommendation = c.EnquiryPanelRecommendation,
                                PreminaryEnquiryDate = c.PreminaryEnquiryDate,
                                CaseEnquiryDate = c.CaseEnquiryDate,
                                CaseNo = Convert.ToString(Session["findcase"]),
                                Narration = c.Narration,
                                IsDropCase = c.IsDropCase,
                                EmployeeDocuments = c.EmployeeDocuments,
                                EnquiryPanel = c.EnquiryPanel,
                                DBTrack = c.DBTrack
                            };

                            var PreminaryEnquiryValidation = ValidateObj(PreminaryEnquiry);
                            if (PreminaryEnquiryValidation.Count > 0)
                            {
                                foreach (var item in PreminaryEnquiryValidation)
                                {

                                    Msg.Add("PreminaryEnquiry" + item);
                                }
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            db.PreminaryEnquiry.Add(PreminaryEnquiry);


                            try
                            {
                                db.SaveChanges();

                                #region  Add NEW Stages in EmpDisciplineProcedings
                               
                                //var EmpDisciplines = db.EmpDisciplineProcedings.Include(e => e.MisconductComplaint).Include(e => e.PreminaryEnquiry)
                                //    .Where(e => e.CaseNo == caseNO).FirstOrDefault();
                                var EmpDisciplines = db.EmployeeIR.Include(e => e.EmpDisciplineProcedings)
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.MisconductComplaint))
                                    

                                    .Where(e => e.Id == EmpIrid).FirstOrDefault().EmpDisciplineProcedings.Where(e => e.CaseNo == caseNO).FirstOrDefault();

                                EmpDisciplineProcedings EmpDiscipline = new EmpDisciplineProcedings()
                                {
                                    CaseNo = EmpDisciplines.CaseNo,
                                    CaseOpeningDate = EmpDisciplines.CaseOpeningDate,
                                    ProceedingStage = 2,
                                    MisconductComplaint = EmpDisciplines.MisconductComplaint,
                                    PreminaryEnquiry = db.PreminaryEnquiry.Find(PreminaryEnquiry.Id),
                                    DBTrack = c.DBTrack
                                };

                                db.EmpDisciplineProcedings.Add(EmpDiscipline);
                                db.SaveChanges();
                                Session["Empdispre_Id"] = EmpDiscipline.Id;



                                var EmpDisIR = new EmployeeIR();

                                EmpDisIR = db.EmployeeIR.Include(e => e.EmpDisciplineProcedings).Where(e => e.Id == EmpIrid).FirstOrDefault();
                                // var EmpDisIR = db.EmployeeIR.Where(e => e.Id == IRid).Select(r => r.EmpDisciplineProcedings.Where(w => caseNO.Contains(w.CaseNo.ToString()))).FirstOrDefault();
                                
                                EmpDisciplineProcedings = db.EmpDisciplineProcedings.Include(e => e.MisconductComplaint).Include(e => e.PreminaryEnquiry).Where(e => e.Id == EmpDiscipline.Id).SingleOrDefault();

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


                IEnumerable<P2BGridData> PreminaryEnquiry = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;
                //var oemployeedata = db.PreminaryEnquiry.OrderBy(e => e.CaseNo).ToList();

                var oemployeedata = db.EmployeeIR.Select(a => new
                {
                    oVictimName = a.Employee.EmpName.FullNameFML,

                    oEmpDiscipline = a.EmpDisciplineProcedings.Select(b => new
                    {
                        oCaseNo = b.CaseNo,
                        oProceedStage = b.ProceedingStage.ToString(),
                        oPreliminaryEnquiryDate = b.PreminaryEnquiry.PreminaryEnquiryDate,
                        oCaseEnquiryDate = b.PreminaryEnquiry.CaseEnquiryDate,
                        oEnquiryPanelRecommendation = b.PreminaryEnquiry.EnquiryPanelRecommendation,
                        oNarration = b.PreminaryEnquiry.Narration,
                        oId = b.PreminaryEnquiry.Id.ToString(),

                    }).ToList(),

                }).ToList();


                foreach (var itemIR in oemployeedata.Where(e => e.oEmpDiscipline.Count() > 0))
                {
                    foreach (var item in itemIR.oEmpDiscipline.Where(e => e.oProceedStage == "2").OrderBy(o => o.oCaseNo))
                    {
                        view = new P2BGridData()
                        {
                            CaseNo = item.oCaseNo.ToString(),
                            VictimName = itemIR.oVictimName.ToString(),
                            ProceedingStage = item.oProceedStage.ToString(),
                            PreliminaryEnquiryDate = item.oPreliminaryEnquiryDate.Value.ToString("dd/MM/yyyy"),
                            CaseEnquiryDate = item.oCaseEnquiryDate != null ? item.oCaseEnquiryDate.Value.ToString("dd/MM/yyyy") : "",
                            EnquiryPanelRecommendation = item.oEnquiryPanelRecommendation.ToString(),
                            Narration = item.oNarration.ToString(),
                            Id = item.oId.ToString(),

                        };

                        model.Add(view);
                    }
                }

                PreminaryEnquiry = model;

                IEnumerable<P2BGridData> IE;

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = PreminaryEnquiry;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE
                         .Where((e => (e.EnquiryPanelRecommendation.ToString().ToUpper().Contains(gp.searchString))
                                    || (e.VictimName.ToString().ToUpper().Contains(gp.searchString))
                                    || (e.ProceedingStage.ToString().ToUpper().Contains(gp.searchString))
                                    || (e.PreliminaryEnquiryDate.ToString().Contains(gp.searchString.ToUpper()))
                                   || (e.CaseEnquiryDate.ToString().Contains(gp.searchString.ToUpper()))
                                  || (e.CaseNo.ToString().Contains(gp.searchString))
                                   || (e.Narration.ToString().ToUpper().Contains(gp.searchString))
                                    || (e.IsDropCase.ToString().ToUpper().Contains(gp.searchString))
                                     || (e.Id.ToString().Contains(gp.searchString))
                                 )).ToList()
                        .Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, Convert.ToString(a.PreliminaryEnquiryDate), Convert.ToString(a.CaseEnquiryDate), a.EnquiryPanelRecommendation, a.Narration, a.IsDropCase, a.Id });
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, Convert.ToString(a.PreliminaryEnquiryDate), Convert.ToString(a.CaseEnquiryDate), a.EnquiryPanelRecommendation, a.Narration, a.IsDropCase, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();


                }
                else
                {
                    IE = PreminaryEnquiry;
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : "0");
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EnquiryPanelRecommendation" ? c.EnquiryPanelRecommendation.ToString() :
                                        gp.sidx == "VictimName" ? c.VictimName.ToString() : gp.sidx == "ProceedingStage" ? c.ProceedingStage.ToString() :
                                        gp.sidx == "PreliminaryEnquiryDate" ? c.PreliminaryEnquiryDate.ToString() :
                                        gp.sidx == "CaseEnquiryDate" ? c.CaseEnquiryDate.ToString() :
                                          gp.sidx == "CaseNo" ? c.CaseNo.ToString() :
                                           gp.sidx == "Narration" ? c.Narration.ToString() :
                                            gp.sidx == "IsDropCase" ? c.Narration.ToString() : "");

                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, Convert.ToString(a.PreliminaryEnquiryDate), Convert.ToString(a.CaseEnquiryDate), a.EnquiryPanelRecommendation, a.Narration, a.IsDropCase, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.PreliminaryEnquiryDate, a.CaseEnquiryDate, a.EnquiryPanelRecommendation, a.Narration, a.IsDropCase, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.PreliminaryEnquiryDate, a.CaseEnquiryDate, a.EnquiryPanelRecommendation, a.Narration, a.IsDropCase, a.Id }).ToList();
                    }
                    totalRecords = PreminaryEnquiry.Count();
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
                    PreminaryEnquiry corporates = db.PreminaryEnquiry.Include(e => e.EnquiryPanel)
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
                var returndata = db.PreminaryEnquiry
                                  .Include(e => e.EnquiryPanel)
                                  .Include(e => e.EnquiryPanel.EnquiryPanelType)
                                  .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        CaseNo = e.CaseNo,
                        PreminaryEnquiryDate = e.PreminaryEnquiryDate,
                        CaseEnquiryDate = e.CaseEnquiryDate,
                        IsDropCase = e.IsDropCase,
                        EnquiryPanelRecommendation = e.EnquiryPanelRecommendation,
                        Narration = e.Narration,
                        Enquirypanel_Id = e.EnquiryPanel.Id == null ? "" : e.EnquiryPanel.Id.ToString(),
                        EnquirypanelDetails = e.EnquiryPanel.FullDetails == null ? "" : e.EnquiryPanel.FullDetails,

                    }).ToList();

                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();
                var k = db.PreminaryEnquiry.Include(e => e.EmployeeDocuments)
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
        public async Task<ActionResult> EditSave(PreminaryEnquiry c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string EmployeeDocuments = form["EmployeeDocumentslList"] == "0" ? null : form["EmployeeDocumentslList"];
                    string EnquiryPanel = form["EnquiryPanelList"] == "" ? null : form["EnquiryPanelList"];
                    string PreminaryEnquiryDate = form["PreminaryEnquiryDate"] == "0" ? "" : form["PreminaryEnquiryDate"];
                    string CaseEnquiryDate = form["CaseEnquiryDate"] == "0" ? "" : form["CaseEnquiryDate"];

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;



                    if (PreminaryEnquiryDate != null)
                    {
                        if (PreminaryEnquiryDate != "")
                        {

                            var val = DateTime.Parse(PreminaryEnquiryDate);
                            c.PreminaryEnquiryDate = val;
                        }
                    }

                    if (CaseEnquiryDate != null)
                    {
                        if (CaseEnquiryDate != "")
                        {

                            var val = DateTime.Parse(CaseEnquiryDate);
                            c.CaseEnquiryDate = val;
                        }
                    }

                    if (EnquiryPanel != null)
                    {
                        if (EnquiryPanel != "")
                        {
                            var val = db.EnquiryPanel.Find(int.Parse(EnquiryPanel));
                            c.EnquiryPanel = val;

                            var add = db.PreminaryEnquiry.Include(e => e.EnquiryPanel).Where(e => e.Id == data).SingleOrDefault();
                            IList<PreminaryEnquiry> contactsdetails = null;
                            if (add.EnquiryPanel != null)
                            {
                                contactsdetails = db.PreminaryEnquiry.Where(x => x.EnquiryPanel.Id == add.EnquiryPanel.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                contactsdetails = db.PreminaryEnquiry.Where(x => x.Id == data).ToList();
                            }
                            foreach (var s in contactsdetails)
                            {
                                s.EnquiryPanel = c.EnquiryPanel;
                                db.PreminaryEnquiry.Attach(s);
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
                        var contactsdetails = db.PreminaryEnquiry.Include(e => e.EnquiryPanel).Where(x => x.Id == data).ToList();
                        foreach (var s in contactsdetails)
                        {
                            s.EnquiryPanel = null;
                            db.PreminaryEnquiry.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    var db_data = db.PreminaryEnquiry.Include(e => e.EmployeeDocuments).Where(e => e.Id == data).SingleOrDefault();
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
                                    PreminaryEnquiry blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.PreminaryEnquiry.Where(e => e.Id == data).SingleOrDefault();


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

                                    var m1 = db.PreminaryEnquiry.Include(e => e.EmployeeDocuments).Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.PreminaryEnquiry.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    var CurCorp = db.PreminaryEnquiry.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        // c.DBTrack = dbT;
                                        PreminaryEnquiry corp = new PreminaryEnquiry()
                                        {

                                            EnquiryPanelRecommendation = c.EnquiryPanelRecommendation,
                                            PreminaryEnquiryDate = c.PreminaryEnquiryDate,
                                            CaseEnquiryDate = c.CaseEnquiryDate,
                                            CaseNo = c.CaseNo,
                                            Narration = c.Narration,
                                            IsDropCase = c.IsDropCase,
                                            DBTrack = c.DBTrack,
                                            Id = data

                                        };


                                        db.PreminaryEnquiry.Attach(corp);
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
                                var clientValues = (PreminaryEnquiry)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (PreminaryEnquiry)databaseEntry.ToObject();
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

                            PreminaryEnquiry blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;


                            using (var context = new DataBaseContext())
                            {
                                blog = context.PreminaryEnquiry.Where(e => e.Id == data).SingleOrDefault();
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
                            PreminaryEnquiry corp = new PreminaryEnquiry()
                            {


                                EnquiryPanelRecommendation = c.EnquiryPanelRecommendation,
                                PreminaryEnquiryDate = c.PreminaryEnquiryDate,
                                CaseEnquiryDate = c.CaseEnquiryDate,
                                CaseNo = c.CaseNo,
                                Narration = c.Narration,
                                IsDropCase = c.IsDropCase,
                                DBTrack = c.DBTrack,
                                Id = data,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            blog.DBTrack = c.DBTrack;
                            db.PreminaryEnquiry.Attach(blog);
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