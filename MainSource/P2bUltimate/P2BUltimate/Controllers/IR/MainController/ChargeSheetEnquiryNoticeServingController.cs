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
    public class ChargeSheetEnquiryNoticeServingController : Controller
    {
        //
        // GET: /ChargeSheetEnquiryNoticeServing/
        public ActionResult Index()
        {
            return View("~/Views/IR/MainViews/ChargeSheetEnquiryNoticeServing/Index.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/IR/_chargesheetservingmode.cshtml");
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
        public ActionResult GetLookupDetailsWitness(List<int> SkipIds)
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
        public ActionResult Create(ChargeSheetEnquiryNoticeServing c, FormCollection form, string EmpIr) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    string caseNO = Convert.ToString(Session["findcase"]);
                    var companypayroll = new CompanyPayroll();
                    companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == company_Id).SingleOrDefault();
                    int EmpIrid = Convert.ToInt32(EmpIr);
                    string ChargeSheetServingMode = form["ChargeSheetservingList"] == "0" ? "" : form["ChargeSheetservingList"];
                    string Witness = form["WitnessList"] == "0" ? "" : form["WitnessList"];
                    string EnquiryNotice = form["EnquiryNoticeList"] == "0" ? null : form["EnquiryNoticeList"];


                    if (EnquiryNotice != null && EnquiryNotice != "")
                    {
                        var ids = Utility.StringIdsToListIds(EnquiryNotice);
                        List<EmployeeDocuments> enquirynotice = new List<EmployeeDocuments>();
                        foreach (var ca in ids)
                        {
                            var enquirynotice_val = db.EmployeeDocuments.Find(ca);
                            enquirynotice.Add(enquirynotice_val);
                            c.EnquiryNotice = enquirynotice;
                        }
                    }
                    else
                        c.EnquiryNotice = null;


                    if (ChargeSheetServingMode != null && ChargeSheetServingMode != "")
                    {
                        int ContId = Convert.ToInt32(ChargeSheetServingMode);
                        var val = db.ChargeSheetServingMode.Include(e => e.ChargeSheetServingModeName)
                                            .Where(e => e.Id == ContId).SingleOrDefault();
                        c.ChargeSheetServingMode = val;
                    }


                    if (Witness != null && Witness != "")
                    {
                        var ids = Utility.StringIdsToListIds(Witness);
                        List<Witness> witness = new List<Witness>();
                        foreach (var ca in ids)
                        {
                            // var witness_val = db.EmployeeIR.Find(ca);
                            int witID = Convert.ToInt32(ca);
                            var witness_val = db.EmployeeIR.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id == witID).SingleOrDefault();

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };

                            if (witness_val != null)
                            {
                                Witness objw = new Witness()
                                {
                                    WitnessEmp = witness_val,
                                    DBTrack = c.DBTrack
                                };
                                witness.Add(objw);
                            }

                            c.Witness = witness;
                        }
                    }
                    else
                        c.Witness = null;


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            #region Already ChargeSheetEnquiryNoticeServing enquiry records exist checking code
                            var EMPIR = db.EmployeeIR.Select(s => new
                            {
                                Irid = s.Id,
                                oEmpcode = s.Employee.EmpCode,
                                EDP = s.EmpDisciplineProcedings.Select(r => new
                                {
                                    EDPid = r.Id,
                                    CaseNum = r.CaseNo,
                                    CSENS = r.ChargeSheetEnquiryNoticeServing,
                                }).Where(e => e.CaseNum == caseNO).ToList(),

                            }).Where(e => e.Irid == EmpIrid).SingleOrDefault();


                            if (EMPIR != null)
                            {
                                var chkEMPIR = EMPIR.EDP.ToList();

                                foreach (var itemC in chkEMPIR.Where(e => e.CSENS != null))
                                {
                                    if (itemC.EDPid != 0 && itemC.CaseNum != null && itemC.CSENS != null)
                                    {
                                        Msg.Add("Record already exist for case no : " + itemC.CaseNum + " & victim :: " + EMPIR.oEmpcode);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            #endregion

                            ChargeSheetEnquiryNoticeServing ChargeSheetEnquiryNoticeServing = new ChargeSheetEnquiryNoticeServing()
                            {
                                Narration = c.Narration,
                                ChargeSheetServingMode = c.ChargeSheetServingMode,
                                EnquiryNotice = c.EnquiryNotice,
                                Witness = c.Witness,
                                IsWitnessReqd = c.IsWitnessReqd,
                                IsClosedServing = c.IsClosedServing,
                                IsChargeSheetRecd = c.IsChargeSheetRecd,
                                DBTrack = c.DBTrack
                            };
                            var ChargeSheetEnquiryNoticeServingValidation = ValidateObj(ChargeSheetEnquiryNoticeServing);
                            if (ChargeSheetEnquiryNoticeServingValidation.Count > 0)
                            {
                                foreach (var item in ChargeSheetEnquiryNoticeServingValidation)
                                {

                                    Msg.Add("ChargeSheetEnquiryNoticeServing" + item);
                                }
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            db.ChargeSheetEnquiryNoticeServing.Add(ChargeSheetEnquiryNoticeServing);

                            try
                            {
                                db.SaveChanges();

                                #region  Add NEW Stages in EmpDisciplineProcedings


                                //var EmpDisciplines = db.EmpDisciplineProcedings.Include(e => e.MisconductComplaint)
                                //    .Include(e => e.PreminaryEnquiry).Include(e => e.PreminaryEnquiryAction)
                                //    .Include(e => e.ChargeSheet)
                                //    .Include(e => e.ChargeSheetReply).Include(e => e.ChargeSheetEnquiryNotice)

                                //    .Where(e => e.CaseNo == caseNO).ToList().LastOrDefault();
                                var EmpDisciplines = db.EmployeeIR.Include(e => e.EmpDisciplineProcedings)
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.MisconductComplaint))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.PreminaryEnquiry))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.PreminaryEnquiryAction))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheet))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheetReply))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheetEnquiryNotice))

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
                                    ProceedingStage = 8,
                                    MisconductComplaint = EmpDisciplines.MisconductComplaint,
                                    PreminaryEnquiry = EmpDisciplines.PreminaryEnquiry,
                                    PreminaryEnquiryAction = EmpDisciplines.PreminaryEnquiryAction,
                                    ChargeSheet = EmpDisciplines.ChargeSheet,
                                    ChargeSheetServing = getEmpdisciplineForChargesheetserving.ChargeSheetServing.ToList(),
                                    ChargeSheetReply = EmpDisciplines.ChargeSheetReply,
                                    ChargeSheetEnquiryNotice = EmpDisciplines.ChargeSheetEnquiryNotice,
                                    ChargeSheetEnquiryNoticeServing = db.ChargeSheetEnquiryNoticeServing.Find(ChargeSheetEnquiryNoticeServing.Id),
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

        public class P2BGridData
        {
            public string VictimName { get; set; }
            public string CaseNo { get; set; }
            public string Id { get; set; }
            public string ProceedingStage { get; set; }
            public string EnquiryNoticeServingDate { get; set; }
            public string ChargeSheetServingModeName { get; set; }
            public string IsWitnessReqd { get; set; }
            public string IsClosedServing { get; set; }
            public string IsChargeSheetRecd { get; set; }
            public string Narration { get; set; }
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

                IEnumerable<P2BGridData> ChargeSheetEnquiryNoticeServing = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;


                var oemployeedata = db.EmployeeIR.Select(a => new
                {
                    oVictimName = a.Employee.EmpName.FullNameFML,

                    oEmpDiscipline = a.EmpDisciplineProcedings.Select(b => new
                    {
                        oCaseNo = b.CaseNo,
                        oProceedStage = b.ProceedingStage.ToString(),
                        oChargeSheetServingModeName = b.ChargeSheetEnquiryNoticeServing.ChargeSheetServingMode.ChargeSheetServingModeName.LookupVal,
                        oEnquiryNoticeServingDate = b.ChargeSheetEnquiryNoticeServing.EnquiryNoticeServingDate,
                        oIsWitnessReqd = b.ChargeSheetEnquiryNoticeServing.IsWitnessReqd.ToString(),
                        oIsChargeSheetRecd = b.ChargeSheetEnquiryNoticeServing.IsChargeSheetRecd.ToString(),
                        oIsClosedServing = b.ChargeSheetEnquiryNoticeServing.IsClosedServing.ToString(),
                        oNarration = b.ChargeSheetEnquiryNoticeServing.Narration,
                        oId = b.ChargeSheetEnquiryNoticeServing.Id.ToString(),

                    }).ToList(),

                }).ToList();

                foreach (var itemIR in oemployeedata.Where(e => e.oEmpDiscipline.Count() > 0))
                {
                    foreach (var item in itemIR.oEmpDiscipline.Where(e => e.oProceedStage == "8").OrderBy(o => o.oCaseNo))
                    {
                        view = new P2BGridData()
                        {
                            CaseNo = item.oCaseNo,
                            VictimName = itemIR.oVictimName.ToString(),
                            ProceedingStage = item.oProceedStage.ToString(),
                            ChargeSheetServingModeName = item.oChargeSheetServingModeName != null ? item.oChargeSheetServingModeName : "",
                            EnquiryNoticeServingDate = item.oEnquiryNoticeServingDate != null ? item.oEnquiryNoticeServingDate.Value.ToString("dd/MM/yyyy") : "",
                            IsWitnessReqd = item.oIsWitnessReqd.ToString(),
                            IsChargeSheetRecd = item.oIsChargeSheetRecd.ToString(),
                            IsClosedServing = item.oIsClosedServing.ToString(),
                            Narration = item.oNarration.ToString(),

                            Id = item.oId.ToString(),

                        };

                        model.Add(view);
                    }
                }
                ChargeSheetEnquiryNoticeServing = model;
                IEnumerable<P2BGridData> IE;

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = ChargeSheetEnquiryNoticeServing;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE
                         .Where((e => (e.EnquiryNoticeServingDate.ToString().ToUpper().Contains(gp.searchString))
                                         || (e.CaseNo.ToString().Contains(gp.searchString))
                                         || (e.VictimName.ToString().ToUpper().Contains(gp.searchString))
                                         || (e.ChargeSheetServingModeName.ToString().ToUpper().Contains(gp.searchString))
                                         || (e.ProceedingStage.ToString().ToUpper().Contains(gp.searchString))
                                         || (e.IsWitnessReqd.ToString().Contains(gp.searchString.ToUpper()))
                                         || (e.IsClosedServing.ToString().ToUpper().Contains(gp.searchString))
                                         || (e.Narration.ToString().ToUpper().Contains(gp.searchString))
                                         || (e.IsChargeSheetRecd.ToString().ToUpper().Contains(gp.searchString))

                                         || (e.Id.ToString().Contains(gp.searchString))
                                 )).ToList()
                        .Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ChargeSheetServingModeName, a.IsWitnessReqd, a.IsClosedServing, a.IsChargeSheetRecd, a.Narration, a.Id });
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ChargeSheetServingModeName, a.IsWitnessReqd, a.IsClosedServing, a.IsChargeSheetRecd, a.Narration, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();


                }
                else
                {
                    IE = ChargeSheetEnquiryNoticeServing;
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
                                         gp.sidx == "ChargeSheetServingModeName" ? c.ChargeSheetServingModeName.ToString() :
                                         gp.sidx == "IsWitnessReqd" ? c.IsWitnessReqd.ToString() :
                                         gp.sidx == "IsClosedServing" ? c.IsClosedServing.ToString() :
                                         gp.sidx == "IsChargeSheetRecd" ? c.IsChargeSheetRecd.ToString() :
                                         gp.sidx == "Narration" ? c.Narration.ToString() : "");

                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ChargeSheetServingModeName, a.IsWitnessReqd, a.IsClosedServing, a.IsChargeSheetRecd, a.Narration, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ChargeSheetServingModeName, a.IsWitnessReqd, a.IsClosedServing, a.IsChargeSheetRecd, a.Narration, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ChargeSheetServingModeName, a.IsWitnessReqd, a.IsClosedServing, a.IsChargeSheetRecd, a.Narration, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
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
        public async Task<ActionResult> EditSave(ChargeSheetEnquiryNoticeServing c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string ChargeSheetServingMode = form["ChargeSheetservingList"] == "0" ? "" : form["ChargeSheetservingList"];
                    string Witness = form["WitnessList"] == "0" ? "" : form["WitnessList"];
                    string EnquiryNotice = form["EnquiryNoticeList"] == "0" ? null : form["EnquiryNoticeList"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    if (ChargeSheetServingMode != null)
                    {
                        if (ChargeSheetServingMode != "")
                        {
                            var val = db.ChargeSheetServingMode.Find(int.Parse(ChargeSheetServingMode));
                            c.ChargeSheetServingMode = val;

                            var add = db.ChargeSheetEnquiryNoticeServing.Include(e => e.ChargeSheetServingMode).Where(e => e.Id == data).SingleOrDefault();
                            IList<ChargeSheetEnquiryNoticeServing> contactsdetails = null;
                            if (add.ChargeSheetServingMode != null)
                            {
                                contactsdetails = db.ChargeSheetEnquiryNoticeServing.Where(x => x.ChargeSheetServingMode.Id == add.ChargeSheetServingMode.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                contactsdetails = db.ChargeSheetEnquiryNoticeServing.Where(x => x.Id == data).ToList();
                            }
                            foreach (var s in contactsdetails)
                            {
                                s.ChargeSheetServingMode = c.ChargeSheetServingMode;
                                db.ChargeSheetEnquiryNoticeServing.Attach(s);
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
                        var contactsdetails = db.ChargeSheetEnquiryNoticeServing.Include(e => e.ChargeSheetServingMode).Where(x => x.Id == data).ToList();
                        foreach (var s in contactsdetails)
                        {
                            s.ChargeSheetServingMode = null;
                            db.ChargeSheetEnquiryNoticeServing.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    var db_data1 = db.ChargeSheetEnquiryNoticeServing.Include(e => e.EnquiryNotice).Where(e => e.Id == data).SingleOrDefault();
                    List<EmployeeDocuments> enquirynotice = new List<EmployeeDocuments>();
                    if (EnquiryNotice != null)
                    {
                        var ids = Utility.StringIdsToListIds(EnquiryNotice);
                        foreach (var ca in ids)
                        {
                            var enquirynotice_val = db.EmployeeDocuments.Find(ca);

                            enquirynotice.Add(enquirynotice_val);
                            db_data1.EnquiryNotice = enquirynotice;
                        }
                    }
                    else
                    {
                        db_data1.EnquiryNotice = null;
                    }
                    var db_data2 = db.ChargeSheetEnquiryNoticeServing.Include(e => e.Witness).Where(e => e.Id == data).SingleOrDefault();
                    List<Witness> witness = new List<Witness>();
                    if (Witness != null)
                    {
                        var ids = Utility.StringIdsToListIds(Witness);
                        foreach (var ca in ids)
                        {
                            // var witness_val = db.EmployeeIR.Find(ca);
                            int empid = Convert.ToInt32(ca);
                            var witness_val = db.EmployeeIR.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id == empid).SingleOrDefault();
                            c.DBTrack = new DBTrack { Action = "M", ModifiedBy = SessionManager.UserName, IsModified = false, ModifiedOn = DateTime.Now };

                            if (witness_val != null)
                            {
                                Witness objwit = new Witness()
                                {
                                    WitnessEmp = witness_val,
                                    DBTrack = c.DBTrack
                                };
                                witness.Add(objwit);

                            }


                            db_data2.Witness = witness;
                        }
                    }
                    else
                    {
                        db_data2.Witness = null;
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
                                    ChargeSheetEnquiryNoticeServing blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.ChargeSheetEnquiryNoticeServing.Where(e => e.Id == data).SingleOrDefault();


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

                                    var m1 = db.ChargeSheetEnquiryNoticeServing.Include(e => e.EnquiryNotice).Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.ChargeSheetEnquiryNoticeServing.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    var CurCorp1 = db.ChargeSheetEnquiryNoticeServing.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp1.RowVersion;
                                    db.Entry(CurCorp1).State = System.Data.Entity.EntityState.Detached;

                                    var m2 = db.ChargeSheetEnquiryNoticeServing.Include(e => e.Witness).Where(e => e.Id == data).ToList();
                                    foreach (var s in m2)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.ChargeSheetEnquiryNoticeServing.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    var CurCorp2 = db.ChargeSheetEnquiryNoticeServing.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp2.RowVersion;
                                    db.Entry(CurCorp2).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        // c.DBTrack = dbT;
                                        ChargeSheetEnquiryNoticeServing corp = new ChargeSheetEnquiryNoticeServing()
                                        {



                                            Narration = c.Narration,
                                            IsWitnessReqd = c.IsWitnessReqd,
                                            IsClosedServing = c.IsClosedServing,
                                            IsChargeSheetRecd = c.IsChargeSheetRecd,
                                            Id = data,
                                            DBTrack = c.DBTrack,
                                            Witness = db_data2.Witness

                                        };


                                        db.ChargeSheetEnquiryNoticeServing.Attach(corp);
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
                                var clientValues = (ChargeSheetEnquiryNoticeServing)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (ChargeSheetEnquiryNoticeServing)databaseEntry.ToObject();
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

                            ChargeSheetEnquiryNoticeServing blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;


                            using (var context = new DataBaseContext())
                            {
                                blog = context.ChargeSheetEnquiryNoticeServing.Where(e => e.Id == data).SingleOrDefault();
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
                            ChargeSheetEnquiryNoticeServing corp = new ChargeSheetEnquiryNoticeServing()
                            {

                                Narration = c.Narration,
                                IsWitnessReqd = c.IsWitnessReqd,
                                IsClosedServing = c.IsClosedServing,
                                IsChargeSheetRecd = c.IsChargeSheetRecd,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };



                            blog.DBTrack = c.DBTrack;
                            db.ChargeSheetEnquiryNoticeServing.Attach(blog);
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
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    ChargeSheetEnquiryNoticeServing corporates = db.ChargeSheetEnquiryNoticeServing
                                                                  .Include(e => e.ChargeSheetServingMode)
                                                                   .Include(e => e.EnquiryNotice)
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
            public Array EmployeeDoc_Id { get; set; }
            public Array EmployeeDocFullDetails { get; set; }
            public Array Witness_Id { get; set; }
            public Array WitnessFullDetails { get; set; }
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.ChargeSheetEnquiryNoticeServing
                                       .Include(e => e.ChargeSheetServingMode)
                                       .Include(e => e.ChargeSheetServingMode.ChargeSheetServingModeName)
                                       .Include(e => e.EnquiryNotice)

                                       .Where(e => e.Id == data)
                    .Select(e => new
                    {


                        IsWitnessReqd = e.IsWitnessReqd,
                        IsClosedServing = e.IsClosedServing,
                        IsChargeSheetRecd = e.IsChargeSheetRecd,
                        Narration = e.Narration,
                        ChargeSheetServingMode_Id = e.ChargeSheetServingMode.Id == null ? "" : e.ChargeSheetServingMode.Id.ToString(),
                        ChargeSheetServingModeDetails = e.ChargeSheetServingMode.ChargeSheetServingModeName.LookupVal + "," + e.ChargeSheetServingMode.ServingSeq == null ? "" : e.ChargeSheetServingMode.ChargeSheetServingModeName.LookupVal + "," + e.ChargeSheetServingMode.ServingSeq
                    }).ToList();

                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();
                var return_data = db.ChargeSheetEnquiryNoticeServing.Include(e => e.Witness)
                    .Include(e => e.Witness.Select(a => a.WitnessEmp))
                    .Include(e => e.Witness.Select(a => a.WitnessEmp.Employee)).Include(e => e.Witness.Select(a => a.WitnessEmp.Employee.EmpName))
                  .Where(e => e.Id == data && e.Witness.Count > 0).ToList();
                foreach (var e in return_data)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        Witness_Id = e.Witness.Select(a => a.Id.ToString()).ToArray(),
                        WitnessFullDetails = e.Witness.Select(a => a.WitnessEmp.Employee.EmpName.FullNameFML).ToArray()
                    });
                }
                var k = db.ChargeSheetEnquiryNoticeServing.Include(e => e.EnquiryNotice)
               .Include(e => e.EnquiryNotice.Select(a => a.DocumentType))
              .Where(e => e.Id == data && e.EnquiryNotice.Count > 0).ToList();

                foreach (var e in k)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        EmployeeDoc_Id = e.EnquiryNotice.Select(a => a.Id.ToString()).ToArray(),
                        EmployeeDocFullDetails = e.EnquiryNotice.Select(a => a.FullDetails).ToArray()
                    });
                }


                return Json(new Object[] { returndata, oreturnEditClass, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }


    }
}