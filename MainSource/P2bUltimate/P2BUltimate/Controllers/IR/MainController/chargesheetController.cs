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
    public class chargesheetController : Controller
    {
        //
        // GET: /chargesheet/
        public ActionResult Index()
        {
            return View("~/views/IR/MainViews/chargesheet/Index.cshtml");
        }
        public class P2BGridData
        {
            public string VictimName { get; set; }
            public string CaseNo { get; set; }
            public string Id { get; set; }
            public string ChargeSheetNo { get; set; }
            public string ProceedingStage { get; set; }
            public string ReplyPeriod { get; set; }
            public string ChargeSheetDetails { get; set; }
            public string ChargeSheetDate { get; set; }
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
        public ActionResult GetPreminaryEnquiryAction(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string EnquiryPanelRecomm = "";
                string suspend = "";
                string HRNotify = "";
                if (data != "" && data != null)
                {
                    var query = db.EmpDisciplineProcedings
                        .Include(e => e.PreminaryEnquiryAction)
                       .Where(e => e.CaseNo == data && e.PreminaryEnquiryAction.Id != null).FirstOrDefault();

                    if (query != null)
                    {
                        if (query.PreminaryEnquiryAction.IsSuspendEmp == true)
                        {
                            suspend = "Employee Suspended. " + "Suspended Date " + query.PreminaryEnquiryAction.SuspensionDate.Value.ToShortDateString();
                        }
                        if (query.PreminaryEnquiryAction.IsNotifyHRDept == true)
                        {
                            HRNotify = "NotifyHRDept " + query.PreminaryEnquiryAction.Narration.ToString();
                        }
                        EnquiryPanelRecomm = suspend + "" + HRNotify;
                    }
                }

                return Json(new Object[] { EnquiryPanelRecomm, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public ActionResult create(ChargeSheet c, FormCollection form, string EmpIr)
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

                    string ChargeSheetDate = form["ChargeSheetDate"] == "0" ? "" : form["ChargeSheetDate"];
                    string EmployeeDocuments = form["EmployeeDocumentsList"] == "0" ? null : form["EmployeeDocumentsList"];

                    if (ChargeSheetDate != null && ChargeSheetDate != "")
                    {
                        var val = DateTime.Parse(ChargeSheetDate);
                        c.ChargeSheetDate = val;
                    }
                    if (EmployeeDocuments != null && EmployeeDocuments != "")
                    {
                        var ids = Utility.StringIdsToListIds(EmployeeDocuments);
                        List<EmployeeDocuments> chargesheetemployeedoc = new List<EmployeeDocuments>();
                        foreach (var ca in ids)
                        {
                            var EmployeeDocuments_val = db.EmployeeDocuments.Find(ca);
                            chargesheetemployeedoc.Add(EmployeeDocuments_val);
                            c.EmployeeDocuments = chargesheetemployeedoc;
                        }
                    }
                    else
                        c.EmployeeDocuments = null;

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            #region Already chargesheet enquiry records exist checking code
                            var EMPIR = db.EmployeeIR.Select(s => new
                            {
                                Irid = s.Id,
                                Empcode = s.Employee.EmpCode,
                                EDP = s.EmpDisciplineProcedings.Select(r => new
                                {
                                    EDPid = r.Id,
                                    CaseNum = r.CaseNo,
                                    CS = r.ChargeSheet,
                                }).Where(e => e.CaseNum == caseNO).ToList(),

                            }).Where(e => e.Irid == EmpIrid).SingleOrDefault();


                            if (EMPIR != null)
                            {
                                var chkEMPIR = EMPIR.EDP.ToList();

                                foreach (var itemC in chkEMPIR.Where(e => e.CS != null))
                                {
                                    if (itemC.EDPid != 0 && itemC.CaseNum != null && itemC.CS != null)
                                    {
                                        Msg.Add("Record already exist for case no : " + itemC.CaseNum + " & victim :: " + EMPIR.Empcode);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            #endregion
                            ChargeSheet ChargeSheet = new ChargeSheet()
                            {
                                ChargeSheetNo = c.ChargeSheetNo,
                                ChargeSheetDate = c.ChargeSheetDate,
                                Narration = c.Narration,
                                ReplyPeriod = c.ReplyPeriod,
                                EmployeeDocuments = c.EmployeeDocuments,
                                ChargeSheetDetails = c.ChargeSheetDetails,
                                DBTrack = c.DBTrack
                            };


                            var ChargeSheetValidation = ValidateObj(ChargeSheet);
                            if (ChargeSheetValidation.Count > 0)
                            {
                                foreach (var item in ChargeSheetValidation)
                                {

                                    Msg.Add("ChargeSheet" + item);
                                }
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            db.ChargeSheet.Add(ChargeSheet);

                            try
                            {

                                db.SaveChanges();

                                #region  Add NEW Stages in EmpDisciplineProcedings

                                var EmpDisciplines = db.EmployeeIR.Include(e => e.EmpDisciplineProcedings)
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.MisconductComplaint))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.PreminaryEnquiry))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.PreminaryEnquiryAction))

                                    .Where(e => e.Id == EmpIrid).FirstOrDefault().EmpDisciplineProcedings.Where(e => e.CaseNo == caseNO).FirstOrDefault();

                                
                                //var EmpDisciplines = db.EmpDisciplineProcedings.Include(e => e.MisconductComplaint)
                                //    .Include(e => e.PreminaryEnquiry).Include(e => e.PreminaryEnquiryAction).Include(e => e.ChargeSheet)
                                //    .Where(e => e.CaseNo == caseNO).ToList().LastOrDefault();

                                EmpDisciplineProcedings EmpDiscipline = new EmpDisciplineProcedings()
                                {
                                    CaseNo = EmpDisciplines.CaseNo,
                                    CaseOpeningDate = EmpDisciplines.CaseOpeningDate,
                                    ProceedingStage = 4,
                                    MisconductComplaint = EmpDisciplines.MisconductComplaint,
                                    PreminaryEnquiry = EmpDisciplines.PreminaryEnquiry,
                                    PreminaryEnquiryAction = EmpDisciplines.PreminaryEnquiryAction,
                                    ChargeSheet = db.ChargeSheet.Find(ChargeSheet.Id),
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

                IEnumerable<P2BGridData> ChargeSheet = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;
                // var oemployeedata_old = db.ChargeSheet.OrderBy(e => e.ChargeSheetNo).ToList();

                var oemployeedata = db.EmployeeIR.Select(a => new
                {
                    oVictimName = a.Employee.EmpName.FullNameFML,

                    oEmpDiscipline = a.EmpDisciplineProcedings.Select(b => new
                    {
                        oCaseNo = b.CaseNo,
                        oProceedStage = b.ProceedingStage.ToString(),
                        oChargeSheetDate = b.ChargeSheet.ChargeSheetDate,
                        oChargeSheetNo = b.ChargeSheet.ChargeSheetNo,
                        oReplyPeriod = b.ChargeSheet.ReplyPeriod.ToString(),
                        oChargeSheetDetails = b.ChargeSheet.ChargeSheetDetails,
                        oNarration = b.ChargeSheet.Narration,
                        oId = b.ChargeSheet.Id.ToString(),

                    }).ToList(),

                }).ToList();

                foreach (var itemIR in oemployeedata.Where(e => e.oEmpDiscipline.Count() > 0))
                {
                    foreach (var item in itemIR.oEmpDiscipline.Where(e => e.oProceedStage == "4").OrderBy(o => o.oCaseNo))
                    {
                        view = new P2BGridData()
                        {
                            CaseNo = item.oCaseNo,
                            VictimName = itemIR.oVictimName.ToString(),
                            ProceedingStage = item.oProceedStage.ToString(),
                            ChargeSheetDate = item.oChargeSheetDate.Value.ToString("dd/MM/yyyy"),
                            ChargeSheetNo = item.oChargeSheetNo.ToString(),
                            ReplyPeriod = item.oReplyPeriod.ToString(),
                            ChargeSheetDetails = item.oChargeSheetDetails.ToString(),
                            Narration = item.oNarration.ToString(),

                            Id = item.oId.ToString(),

                        };

                        model.Add(view);
                    }
                }

                ChargeSheet = model;

                IEnumerable<P2BGridData> IE;

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = ChargeSheet;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE
                         .Where((e => (e.CaseNo.ToString().Contains(gp.searchString))
                                     || (e.VictimName.ToString().ToUpper().Contains(gp.searchString))
                                     || (e.ProceedingStage.ToString().ToUpper().Contains(gp.searchString))
                                    || (e.ChargeSheetDate.ToString().ToUpper().Contains(gp.searchString))
                                   || (e.ChargeSheetNo.ToString().ToUpper().Contains(gp.searchString))
                                  || (e.ReplyPeriod.ToString().ToUpper().Contains(gp.searchString))
                                   || (e.ChargeSheetDetails.ToString().ToUpper().Contains(gp.searchString))
                                    || (e.Narration.ToString().ToUpper().Contains(gp.searchString))
                                     || (e.Id.ToString().Contains(gp.searchString))
                                 )).ToList()
                        .Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ChargeSheetDate, a.ChargeSheetNo, a.ReplyPeriod, a.ChargeSheetDetails, a.Narration, a.Id });
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ChargeSheetDate, a.ChargeSheetNo, a.ReplyPeriod, a.ChargeSheetDetails, a.Narration, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();


                }
                else
                {
                    IE = ChargeSheet;
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
                                         gp.sidx == "ChargeSheetDate" ? c.ChargeSheetDate.ToString() :
                                         gp.sidx == "ChargeSheetNo" ? c.ChargeSheetNo.ToString() :
                                         gp.sidx == "ReplyPeriod" ? c.ReplyPeriod.ToString() :
                                         gp.sidx == "ChargeSheetDetails" ? c.ChargeSheetDetails.ToString() :
                                         gp.sidx == "Narration" ? c.Narration.ToString() : "");

                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ChargeSheetDate, a.ChargeSheetNo, a.ReplyPeriod, a.ChargeSheetDetails, a.Narration, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ChargeSheetDate, a.ChargeSheetNo, a.ReplyPeriod, a.ChargeSheetDetails, a.Narration, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ChargeSheetDate, a.ChargeSheetNo, a.ReplyPeriod, a.ChargeSheetDetails, a.Narration, a.Id }).ToList();
                    }
                    totalRecords = ChargeSheet.Count();
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
                    ChargeSheet corporates = db.ChargeSheet
                                                      .Include(e => e.EmployeeDocuments)
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
            public Array ChargesheetEmployeedoc_Id { get; set; }
            public Array ChargesheetEmployeedocFullDetails { get; set; }
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.ChargeSheet

                                  .Include(e => e.EmployeeDocuments)
                                  .Where(e => e.Id == data).AsEnumerable()
                    .Select(e => new
                    {
                        ChargeSheetNo = e.ChargeSheetNo,
                        ChargeSheetDate = e.ChargeSheetDate,
                        ReplyPeriod = e.ReplyPeriod,
                        ChargeSheetDetails = e.ChargeSheetDetails,
                        Narration = e.Narration,
                    }).ToList();

                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();
                var k = db.ChargeSheet.Include(e => e.EmployeeDocuments)
                   .Include(e => e.EmployeeDocuments.Select(a => a.DocumentType))
                  .Where(e => e.Id == data && e.EmployeeDocuments.Count > 0).ToList();
                foreach (var e in k)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        ChargesheetEmployeedoc_Id = e.EmployeeDocuments.Select(a => a.Id.ToString()).ToArray(),
                        ChargesheetEmployeedocFullDetails = e.EmployeeDocuments.Select(a => a.FullDetails).ToArray()
                    });
                }
                return Json(new Object[] { returndata, oreturnEditClass, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(ChargeSheet c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string ChargeSheetDate = form["ChargeSheetDate"] == "0" ? "" : form["ChargeSheetDate"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;



                    if (ChargeSheetDate != null)
                    {
                        if (ChargeSheetDate != "")
                        {

                            var val = DateTime.Parse(ChargeSheetDate);
                            c.ChargeSheetDate = val;
                        }
                    }

                    var db_data = db.ChargeSheet.Include(e => e.EmployeeDocuments).Where(e => e.Id == data).SingleOrDefault();
                    string EmployeeDocuments = form["EmployeeDocumentsList"] == "0" ? null : form["EmployeeDocumentsList"]; ;
                    List<EmployeeDocuments> enquirychargesheetdoc = new List<EmployeeDocuments>();
                    if (EmployeeDocuments != null)
                    {
                        var ids = Utility.StringIdsToListIds(EmployeeDocuments);
                        foreach (var ca in ids)
                        {
                            var enquiryreportdoc_val = db.EmployeeDocuments.Find(ca);

                            enquirychargesheetdoc.Add(enquiryreportdoc_val);

                            db_data.EmployeeDocuments = enquirychargesheetdoc;
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

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    //ChargeSheet blog = null; // to retrieve old data
                                    //DbPropertyValues originalBlogValues = null;

                                    //using (var context = new DataBaseContext())
                                    //{
                                    //    blog = context.ChargeSheet.Where(e => e.Id == data).SingleOrDefault();


                                    //    originalBlogValues = context.Entry(blog).OriginalValues;
                                    //}

                                    c.DBTrack = new DBTrack
                                    {
                                        //CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        //CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    var m1 = db.ChargeSheet.Include(e => e.EmployeeDocuments).Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.ChargeSheet.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    var CurCorp = db.ChargeSheet.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        // c.DBTrack = dbT;
                                        ChargeSheet corp = new ChargeSheet()
                                        {
                                            ChargeSheetDate = c.ChargeSheetDate,
                                            ChargeSheetNo = c.ChargeSheetNo,
                                            Narration = c.Narration,
                                            ReplyPeriod = c.ReplyPeriod,
                                            ChargeSheetDetails = c.ChargeSheetDetails,
                                            DBTrack = c.DBTrack,
                                            // UnitId=c.UnitId,
                                            Id = data,

                                        };
                                        db.ChargeSheet.Attach(corp);
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
                                var clientValues = (ChargeSheet)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (ChargeSheet)databaseEntry.ToObject();
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

                            //ChargeSheet blog = null; // to retrieve old data
                            //DbPropertyValues originalBlogValues = null;


                            //using (var context = new DataBaseContext())
                            //{
                            //    blog = context.ChargeSheet.Where(e => e.Id == data).SingleOrDefault();
                            //    originalBlogValues = context.Entry(blog).OriginalValues;
                            //}
                            c.DBTrack = new DBTrack
                            {
                                //CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                //CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = db_data.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            ChargeSheet corp = new ChargeSheet()
                            {


                                Id = data,
                                ChargeSheetDate = c.ChargeSheetDate,
                                ChargeSheetNo = c.ChargeSheetNo,
                                Narration = c.Narration,
                                ReplyPeriod = c.ReplyPeriod,
                                ChargeSheetDetails = c.ChargeSheetDetails,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };

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