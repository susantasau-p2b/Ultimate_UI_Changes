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
    public class chargesheetenquirynoticeController : Controller
    {
        //
        // GET: /chargesheetenquirynotice/
        public ActionResult Index()
        {
            return View("~/Views/IR/MainViews/chargesheetenquirynotice/index.cshtml");
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

            public string EnquiryAddress { get; set; }
            public string EnquiryNoticeIssueDate { get; set; }
            public string EnquiryNoticeNo { get; set; }
            public string EnquiryPlace { get; set; }
            public string EnquiryScheduleDate { get; set; }
            public string EnquiryScheduleTime { get; set; }
            public string Narration { get; set; }
            public string ReplyPeriod { get; set; }
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

        public ActionResult GetLookupDetailsEnquirypanel(List<int> SkipIds)
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
        public ActionResult GetLookupDetailsAddress(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Address.Include(e => e.Country).Include(e => e.State).Include(e => e.StateRegion)
                    .Include(e => e.District).Include(e => e.Taluka).Include(e => e.City).Include(e => e.Area).ToList();
                IEnumerable<Address> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Address.ToList().Where(d => d.FullAddress.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.Address3 }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public ActionResult Create(ChargeSheetEnquiryNotice c, FormCollection form, string EmpIr)
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
                    string EnquiryNoticeIssueDate = form["EnquiryNoticeIssueDate"] == "0" ? "" : form["EnquiryNoticeIssueDate"];
                    if (EnquiryNoticeIssueDate != null && EnquiryNoticeIssueDate != "")
                    {
                        var val = DateTime.Parse(EnquiryNoticeIssueDate);
                        c.EnquiryNoticeIssueDate = val;
                    }
                    string EnquiryScheduleDate = form["EnquiryScheduleDate"] == "0" ? "" : form["EnquiryScheduleDate"];
                    if (EnquiryScheduleDate != null && EnquiryScheduleDate != "")
                    {
                        var val = DateTime.Parse(EnquiryScheduleDate);
                        c.EnquiryScheduleDate = val;
                    }
                    string EnquiryScheduleTime = form["EnquiryScheduleTime"] == "0" ? "" : form["EnquiryScheduleTime"];
                    if (EnquiryScheduleTime != null && EnquiryScheduleTime != "")
                    {
                        var val = DateTime.Parse(EnquiryScheduleTime);
                        c.EnquiryScheduleTime = val;
                    }

                    string Addrs = form["EnquiryAddressList"] == "0" ? "" : form["EnquiryAddressList"];
                    if (Addrs != null && Addrs != "")
                    {
                        int AddId = Convert.ToInt32(Addrs);
                        var val = db.Address.Include(e => e.Area)
                                            .Include(e => e.City)
                                            .Include(e => e.Country)
                                            .Include(e => e.District)
                                            .Include(e => e.State)
                                            .Include(e => e.StateRegion)
                                            .Include(e => e.Taluka)
                                            .Where(e => e.Id == AddId).SingleOrDefault();
                        c.EnquiryAddress = val;
                    }


                    string EnquiryPanelList = form["EnquiryPanelList"] == "0" ? null : form["EnquiryPanelList"];
                    if (EnquiryPanelList != null && EnquiryPanelList != "")
                    {
                        var ids = Utility.StringIdsToListIds(EnquiryPanelList);
                        List<EnquiryPanel> enquirypanel = new List<EnquiryPanel>();
                        foreach (var ca in ids)
                        {
                            var enquirypanel_val = db.EnquiryPanel.Find(ca);
                            enquirypanel.Add(enquirypanel_val);
                            c.EnquiryPanel = enquirypanel;
                        }
                    }
                    else
                        c.EnquiryPanel = null;


                    string WitnessList = form["WitnessList"] == "0" ? null : form["WitnessList"];
                    if (WitnessList != null && WitnessList != "")
                    {
                        var ids = Utility.StringIdsToListIds(WitnessList);
                        List<Witness> witness = new List<Witness>();
                        foreach (var ca in ids)
                        {
                            int witID = Convert.ToInt32(ca);
                            // var witness_val = db.EmployeeIR.Find(ca);
                            var witness_val = db.EmployeeIR.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id == witID).SingleOrDefault();
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };

                            if (witness_val != null)
                            {
                                Witness objWit = new Witness()
                                {
                                    WitnessEmp = witness_val,
                                    DBTrack = c.DBTrack
                                };
                                witness.Add(objWit);

                            }

                            c.Witness = witness;
                        }
                    }
                    else { c.Witness = null; }



                    string EmployeeDocuments = form["EmployeeDocumentsList"] == "0" ? null : form["EmployeeDocumentsList"];
                    if (EmployeeDocuments != null && EmployeeDocuments != "")
                    {
                        var ids = Utility.StringIdsToListIds(EmployeeDocuments);
                        List<EmployeeDocuments> suplementrydoc = new List<EmployeeDocuments>();
                        foreach (var ca in ids)
                        {
                            var suplementrydoc_val = db.EmployeeDocuments.Find(ca);
                            suplementrydoc.Add(suplementrydoc_val);
                            c.EmployeeDocuments = suplementrydoc;
                        }
                    }
                    else
                        c.EmployeeDocuments = null;

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            #region Already ChargeSheetEnquiryNotice enquiry records exist checking code
                            var EMPIR = db.EmployeeIR.Select(s => new
                            {
                                Irid = s.Id,
                                oEmpcode = s.Employee.EmpCode,
                                EDP = s.EmpDisciplineProcedings.Select(r => new
                                {
                                    EDPid = r.Id,
                                    CaseNum = r.CaseNo,
                                    CSEN = r.ChargeSheetEnquiryNotice,
                                }).Where(e => e.CaseNum == caseNO).ToList(),

                            }).Where(e => e.Irid == EmpIrid).SingleOrDefault();


                            if (EMPIR != null)
                            {
                                var chkEMPIR = EMPIR.EDP.ToList();

                                foreach (var itemC in chkEMPIR.Where(e => e.CSEN != null))
                                {
                                    if (itemC.EDPid != 0 && itemC.CaseNum != null && itemC.CSEN != null)
                                    {
                                        Msg.Add("Record already exist for case no : " + itemC.CaseNum + " & victim :: " + EMPIR.oEmpcode);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            #endregion

                            ChargeSheetEnquiryNotice ChargeSheetEnquiryNotice = new ChargeSheetEnquiryNotice()
                            {
                                Narration = c.Narration,
                                ReplyPeriod = c.ReplyPeriod,
                                EnquiryNoticeNo = c.EnquiryNoticeNo,
                                EnquiryPlace = c.EnquiryPlace,
                                EnquiryNoticeIssueDate = c.EnquiryNoticeIssueDate,
                                EnquiryScheduleDate = c.EnquiryScheduleDate,
                                EnquiryScheduleTime = c.EnquiryScheduleTime,
                                EmployeeDocuments = c.EmployeeDocuments,
                                EnquiryPanel = c.EnquiryPanel,
                                Witness = c.Witness,
                                EnquiryAddress = c.EnquiryAddress,
                                DBTrack = c.DBTrack
                            };
                            var ChargeSheetEnquiryNoticeValidation = ValidateObj(ChargeSheetEnquiryNotice);
                            if (ChargeSheetEnquiryNoticeValidation.Count > 0)
                            {
                                foreach (var item in ChargeSheetEnquiryNoticeValidation)
                                {

                                    Msg.Add("ChargeSheetEnquiryNotice" + item);
                                }
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            db.ChargeSheetEnquiryNotice.Add(ChargeSheetEnquiryNotice);

                            try
                            {

                                db.SaveChanges();


                                #region  Add NEW Stages in EmpDisciplineProcedings

                                var EmpDisciplines = db.EmployeeIR.Include(e => e.EmpDisciplineProcedings)
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.MisconductComplaint))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.PreminaryEnquiry))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.PreminaryEnquiryAction))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheet))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheetReply))

                                    .Where(e => e.Id == EmpIrid).FirstOrDefault().EmpDisciplineProcedings.Where(e => e.CaseNo == caseNO).FirstOrDefault();

                                //var EmpDisciplines = db.EmpDisciplineProcedings.Include(e => e.MisconductComplaint)
                                //    .Include(e => e.ChargeSheetReply)
                                //    .Include(e => e.PreminaryEnquiry).Include(e => e.PreminaryEnquiryAction)
                                //    .Include(e => e.ChargeSheet)
                                //    .Where(e => e.CaseNo == caseNO).ToList().LastOrDefault();

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
                                    ProceedingStage = 7,
                                    MisconductComplaint = EmpDisciplines.MisconductComplaint,
                                    PreminaryEnquiry = EmpDisciplines.PreminaryEnquiry,
                                    PreminaryEnquiryAction = EmpDisciplines.PreminaryEnquiryAction,
                                    ChargeSheet = EmpDisciplines.ChargeSheet,
                                    ChargeSheetServing = getEmpdisciplineForChargesheetserving.ChargeSheetServing.ToList(),
                                    ChargeSheetReply = EmpDisciplines.ChargeSheetReply,
                                    ChargeSheetEnquiryNotice = db.ChargeSheetEnquiryNotice.Find(ChargeSheetEnquiryNotice.Id),
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


                IEnumerable<P2BGridData> ChargeSheetEnquiryNotice = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;
                // var oemployeedata = db.ChargeSheetEnquiryNotice.OrderBy(e => e.EnquiryNoticeNo).ToList();
                var oemployeedata = db.EmployeeIR.Select(a => new
                {
                    oVictimName = a.Employee.EmpName.FullNameFML,

                    oEmpDiscipline = a.EmpDisciplineProcedings.Select(b => new
                    {
                        oCaseNo = b.CaseNo,
                        oProceedStage = b.ProceedingStage.ToString(),
                        oEnquiryNoticeIssueDate = b.ChargeSheetEnquiryNotice.EnquiryNoticeIssueDate,
                        oEnquiryNoticeNo = b.ChargeSheetEnquiryNotice.EnquiryNoticeNo,
                        oEnquiryPlace = b.ChargeSheetEnquiryNotice.EnquiryPlace,
                        oEnquiryScheduleDate = b.ChargeSheetEnquiryNotice.EnquiryScheduleDate,
                        oReplyPeriod = b.ChargeSheetEnquiryNotice.ReplyPeriod.ToString(),
                        oEnquiryScheduleTime = b.ChargeSheetEnquiryNotice.EnquiryScheduleTime,
                        oNarration = b.ChargeSheetEnquiryNotice.Narration,
                        oId = b.ChargeSheetEnquiryNotice.Id.ToString(),

                    }).ToList(),

                }).ToList();

                foreach (var itemIR in oemployeedata.Where(e => e.oEmpDiscipline.Count() > 0))
                {
                    foreach (var item in itemIR.oEmpDiscipline.Where(e => e.oProceedStage == "7").OrderBy(o => o.oCaseNo))
                    {
                        view = new P2BGridData()
                        {
                            CaseNo = item.oCaseNo,
                            VictimName = itemIR.oVictimName.ToString(),
                            ProceedingStage = item.oProceedStage.ToString(),
                            EnquiryNoticeIssueDate = item.oEnquiryNoticeIssueDate != null ? item.oEnquiryNoticeIssueDate.Value.ToString("dd/MM/yyyy") : "",
                            EnquiryNoticeNo = item.oEnquiryNoticeNo.ToString(),
                            EnquiryPlace = item.oEnquiryPlace.ToString(),
                            EnquiryScheduleDate = item.oEnquiryScheduleDate != null ? item.oEnquiryScheduleDate.Value.ToString("dd/MM/yyyy") : "",
                            EnquiryScheduleTime = item.oEnquiryScheduleTime != null ? item.oEnquiryScheduleTime.Value.ToShortTimeString() : "",
                            Narration = item.oNarration.ToString(),
                            ReplyPeriod = item.oReplyPeriod.ToString(),
                            Id = item.oId.ToString(),

                        };

                        model.Add(view);
                    }
                }


                ChargeSheetEnquiryNotice = model;

                IEnumerable<P2BGridData> IE;


                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = ChargeSheetEnquiryNotice;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE
                         .Where((e => (e.EnquiryNoticeIssueDate.ToString().ToUpper().Contains(gp.searchString))
                             ||(e.CaseNo.ToString().Contains(gp.searchString))
                                           || (e.VictimName.ToString().ToUpper().Contains(gp.searchString))
                                          || (e.ProceedingStage.ToString().ToUpper().Contains(gp.searchString))
                                   || (e.EnquiryNoticeNo.ToString().Contains(gp.searchString.ToUpper()))
                                   || (e.EnquiryPlace.ToString().Contains(gp.searchString.ToUpper()))
                                   || (e.EnquiryScheduleDate.ToString().ToUpper().Contains(gp.searchString))
                                   || (e.EnquiryScheduleTime.ToString().ToUpper().Contains(gp.searchString))
                                   || (e.Narration.ToString().ToUpper().Contains(gp.searchString))
                                   || (e.ReplyPeriod.ToString().ToUpper().Contains(gp.searchString))
                                   || (e.Id.ToString().Contains(gp.searchString))

                                 )).ToList()
                        .Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.EnquiryNoticeIssueDate, a.EnquiryNoticeNo, a.EnquiryPlace, a.EnquiryScheduleDate, a.EnquiryScheduleTime, a.Narration, a.ReplyPeriod, a.Id });
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.EnquiryNoticeIssueDate, a.EnquiryNoticeNo, a.EnquiryPlace, a.EnquiryScheduleDate, a.EnquiryScheduleTime, a.Narration, a.ReplyPeriod, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();


                }
                else
                {
                    IE = ChargeSheetEnquiryNotice;
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : "0");
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EnquiryNoticeIssueDate" ? c.EnquiryNoticeIssueDate.ToString() :
                                            gp.sidx == "CaseNo" ? c.CaseNo.ToString() :
                                         gp.sidx == "VictimName" ? c.VictimName.ToString() :
                                         gp.sidx == "ProceedingStage" ? c.ProceedingStage.ToString() :
                                          gp.sidx == "EnquiryNoticeNo" ? c.EnquiryNoticeNo.ToString() :
                                            gp.sidx == "EnquiryPlace" ? c.EnquiryPlace.ToString() :
                                              gp.sidx == "EnquiryScheduleDate" ? c.EnquiryScheduleDate.ToString() :
                                               gp.sidx == "EnquiryScheduleTime" ? c.EnquiryScheduleTime.ToString() :
                                                gp.sidx == "Narration" ? c.Narration.ToString() :
                                                  gp.sidx == "ReplyPeriod" ? c.ReplyPeriod.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, Convert.ToString(a.EnquiryNoticeIssueDate), a.EnquiryNoticeNo, a.EnquiryPlace, Convert.ToString(a.EnquiryScheduleDate), Convert.ToString(a.EnquiryScheduleTime), a.Narration, a.ReplyPeriod, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, Convert.ToString(a.EnquiryNoticeIssueDate), a.EnquiryNoticeNo, a.EnquiryPlace, Convert.ToString(a.EnquiryScheduleDate), Convert.ToString(a.EnquiryScheduleTime), a.Narration, a.ReplyPeriod, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, Convert.ToString(a.EnquiryNoticeIssueDate), a.EnquiryNoticeNo, a.EnquiryPlace, Convert.ToString(a.EnquiryScheduleDate), Convert.ToString(a.EnquiryScheduleTime), a.Narration, a.ReplyPeriod, a.Id }).ToList();
                    }
                    totalRecords = ChargeSheetEnquiryNotice.Count();
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
                    ChargeSheetEnquiryNotice corporates = db.ChargeSheetEnquiryNotice.Include(e => e.EnquiryPanel)
                                                             .Include(e => e.EmployeeDocuments)
                                                              .Include(e => e.Witness)
                                                              .Include(e => e.EnquiryAddress)

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
            public Array EmployeeNoticedoc_Id { get; set; }
            public Array EmployeeNoticedocFullDetails { get; set; }

            public Array EnquiryPanelNotice_Id { get; set; }
            public Array EnquiryPanelNoticeFullDetails { get; set; }

            public Array WitnessNotice_Id { get; set; }
            public Array WitnessNoticeFullDetails { get; set; }


        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.ChargeSheetEnquiryNotice
                                  .Include(e => e.EnquiryPanel)
                                  .Include(e => e.EmployeeDocuments)
                                  .Include(e => e.Witness)
                                  .Include(e => e.EnquiryAddress)
                                  .Where(e => e.Id == data).AsEnumerable()
                    .Select(e => new
                    {
                        ReplyPeriod = e.ReplyPeriod,
                        EnquiryNoticeNo = e.EnquiryNoticeNo,
                        EnquiryPlace = e.EnquiryPlace,
                        EnquiryNoticeIssueDate = e.EnquiryNoticeIssueDate,
                        EnquiryScheduleDate = e.EnquiryScheduleDate,
                        Narration = e.Narration,
                        EnquiryScheduleTime = e.EnquiryScheduleTime.Value.ToShortTimeString(),
                        AddressFullAddress = e.EnquiryAddress == null ? "" : e.EnquiryAddress.FullAddress,
                        Address_Id = e.EnquiryAddress != null ? e.EnquiryAddress.Id : 0,

                    }).ToList();

                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();

                var k = db.ChargeSheetEnquiryNotice.Include(e => e.EmployeeDocuments)
                   .Include(e => e.EmployeeDocuments.Select(a => a.DocumentType))
                  .Where(e => e.Id == data && e.EmployeeDocuments.Count > 0).ToList();
                foreach (var e in k)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        EmployeeNoticedoc_Id = e.EmployeeDocuments.Select(a => a.Id.ToString()).ToArray(),
                        EmployeeNoticedocFullDetails = e.EmployeeDocuments.Select(a => a.FullDetails).ToArray()
                    });
                }
                var m = db.ChargeSheetEnquiryNotice.Include(e => e.EnquiryPanel)
                     .Include(e => e.EnquiryPanel.Select(a => a.EnquiryPanelType))
                 .Where(e => e.Id == data && e.EnquiryPanel.Count > 0).ToList();
                foreach (var e in m)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        EnquiryPanelNotice_Id = e.EnquiryPanel.Select(a => a.Id.ToString()).ToArray(),
                        EnquiryPanelNoticeFullDetails = e.EnquiryPanel.Select(a => a.FullDetails).ToArray()
                    });
                }
                var q = db.ChargeSheetEnquiryNotice.Include(e => e.Witness)
                    .Include(e => e.Witness.Select(a => a.WitnessEmp))
                 .Include(e => e.Witness.Select(a => a.WitnessEmp.Employee)).Include(e => e.Witness.Select(a => a.WitnessEmp.Employee.EmpName))
                .Where(e => e.Id == data && e.Witness.Count > 0).ToList();
                foreach (var e in q)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        WitnessNotice_Id = e.Witness.Select(a => a.Id.ToString()).ToArray(),
                        WitnessNoticeFullDetails = e.Witness.Select(a => a.WitnessEmp.Employee.EmpName.FullNameFML).ToArray()
                    });
                }

                return Json(new Object[] { returndata, oreturnEditClass, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(ChargeSheetEnquiryNotice c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    string EnquiryNoticeIssueDate = form["EnquiryNoticeIssueDate"] == "0" ? "" : form["EnquiryNoticeIssueDate"];
                    string EnquiryScheduleDate = form["EnquiryScheduleDate"] == "0" ? "" : form["EnquiryScheduleDate"];
                    string EnquiryScheduleTime = form["EnquiryScheduleTime"] == "0" ? "" : form["EnquiryScheduleTime"];

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;



                    if (EnquiryNoticeIssueDate != null)
                    {
                        if (EnquiryNoticeIssueDate != "")
                        {

                            var val = DateTime.Parse(EnquiryNoticeIssueDate);
                            c.EnquiryNoticeIssueDate = val;
                        }
                    }

                    if (EnquiryScheduleDate != null)
                    {
                        if (EnquiryScheduleDate != "")
                        {

                            var val = DateTime.Parse(EnquiryScheduleDate);
                            c.EnquiryScheduleDate = val;
                        }
                    }

                    if (EnquiryScheduleTime != null)
                    {
                        if (EnquiryScheduleTime != "")
                        {

                            var val = DateTime.Parse(EnquiryScheduleTime);
                            c.EnquiryScheduleTime = val;
                        }
                    }
                    string Addrs = form["EnquiryAddressList"] == "0" ? "" : form["EnquiryAddressList"];
                    if (Addrs != null)
                    {
                        if (Addrs != "")
                        {
                            var val = db.Address.Find(int.Parse(Addrs));
                            c.EnquiryAddress = val;

                            var add = db.ChargeSheetEnquiryNotice.Include(e => e.EnquiryAddress).Where(e => e.Id == data).SingleOrDefault();
                            IList<ChargeSheetEnquiryNotice> addressdetails = null;
                            if (add.EnquiryAddress != null)
                            {
                                addressdetails = db.ChargeSheetEnquiryNotice.Where(x => x.EnquiryAddress.Id == add.EnquiryAddress.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                addressdetails = db.ChargeSheetEnquiryNotice.Where(x => x.Id == data).ToList();
                            }
                            if (addressdetails != null)
                            {
                                foreach (var s in addressdetails)
                                {
                                    s.EnquiryAddress = c.EnquiryAddress;
                                    db.ChargeSheetEnquiryNotice.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    // await db.SaveChangesAsync(false);
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                        }
                    }
                    else
                    {
                        var addressdetails = db.ChargeSheetEnquiryNotice.Include(e => e.EnquiryAddress).Where(x => x.Id == data).ToList();
                        foreach (var s in addressdetails)
                        {
                            s.EnquiryAddress = null;
                            db.ChargeSheetEnquiryNotice.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }

                    var db_data1 = db.ChargeSheetEnquiryNotice.Include(e => e.EmployeeDocuments).Where(e => e.Id == data).SingleOrDefault();
                    List<EmployeeDocuments> employeedoc = new List<EmployeeDocuments>();
                    string EmployeeDocuments = form["EmployeeDocumentsList"] == "0" ? null : form["EmployeeDocumentsList"];
                    if (EmployeeDocuments != null)
                    {
                        var ids = Utility.StringIdsToListIds(EmployeeDocuments);
                        foreach (var ca in ids)
                        {
                            var employeedoc_val = db.EmployeeDocuments.Find(ca);

                            employeedoc.Add(employeedoc_val);
                            db_data1.EmployeeDocuments = employeedoc;
                        }
                    }
                    else
                    {
                        db_data1.EmployeeDocuments = null;
                    }

                    var db_data2 = db.ChargeSheetEnquiryNotice.Include(e => e.Witness).Where(e => e.Id == data).SingleOrDefault();
                    List<Witness> witness = new List<Witness>();
                    string WitnessList = form["WitnessList"] == "0" ? null : form["WitnessList"];
                    if (WitnessList != null)
                    {
                        var ids = Utility.StringIdsToListIds(WitnessList);
                        foreach (var ca in ids)
                        {
                            int Witnessid = Convert.ToInt32(ca);
                            //var witness_val = db.EmployeeIR.Find(ca);
                            var witness_val = db.EmployeeIR.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id == Witnessid).SingleOrDefault();
                            c.DBTrack = new DBTrack { Action = "M", ModifiedBy = SessionManager.UserName, IsModified = true, ModifiedOn = DateTime.Now };

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
                    var db_data3 = db.ChargeSheetEnquiryNotice.Include(e => e.EnquiryPanel).Where(e => e.Id == data).SingleOrDefault();
                    List<EnquiryPanel> enquirypanel = new List<EnquiryPanel>();
                    string EnquiryPanelList = form["EnquiryPanelList"] == "0" ? null : form["EnquiryPanelList"];
                    if (EnquiryPanelList != null)
                    {
                        var ids = Utility.StringIdsToListIds(EnquiryPanelList);
                        foreach (var ca in ids)
                        {
                            var enquirypanel_val = db.EnquiryPanel.Find(ca);

                            enquirypanel.Add(enquirypanel_val);
                            db_data3.EnquiryPanel = enquirypanel;
                        }
                    }
                    else
                    {
                        db_data3.EnquiryPanel = null;
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
                                    ChargeSheetEnquiryNotice blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.ChargeSheetEnquiryNotice.Where(e => e.Id == data).SingleOrDefault();


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

                                    var m1 = db.ChargeSheetEnquiryNotice.Include(e => e.EmployeeDocuments).Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.ChargeSheetEnquiryNotice.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    var CurCorp1 = db.ChargeSheetEnquiryNotice.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp1.RowVersion;
                                    db.Entry(CurCorp1).State = System.Data.Entity.EntityState.Detached;
                                    var m2 = db.ChargeSheetEnquiryNotice.Include(e => e.Witness).Where(e => e.Id == data).ToList();
                                    foreach (var s in m2)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.ChargeSheetEnquiryNotice.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    var CurCorp2 = db.ChargeSheetEnquiryNotice.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp2.RowVersion;
                                    db.Entry(CurCorp2).State = System.Data.Entity.EntityState.Detached;
                                    var m3 = db.ChargeSheetEnquiryNotice.Include(e => e.EnquiryPanel).Where(e => e.Id == data).ToList();
                                    foreach (var s in m3)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.ChargeSheetEnquiryNotice.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    var CurCorp3 = db.ChargeSheetEnquiryNotice.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp3.RowVersion;
                                    db.Entry(CurCorp3).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        // c.DBTrack = dbT;
                                        ChargeSheetEnquiryNotice corp = new ChargeSheetEnquiryNotice()
                                        {

                                            EnquiryNoticeNo = c.EnquiryNoticeNo,
                                            ReplyPeriod = c.ReplyPeriod,
                                            EnquiryPlace = c.EnquiryPlace,
                                            EnquiryNoticeIssueDate = c.EnquiryNoticeIssueDate,
                                            EnquiryScheduleDate = c.EnquiryScheduleDate,
                                            EnquiryScheduleTime = c.EnquiryScheduleTime,
                                            Narration = c.Narration,
                                            DBTrack = c.DBTrack,
                                            Id = data
                                        };

                                        db.ChargeSheetEnquiryNotice.Attach(corp);
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
                                var clientValues = (ChargeSheetEnquiryNotice)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (ChargeSheetEnquiryNotice)databaseEntry.ToObject();
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

                            ChargeSheetEnquiryNotice blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;


                            using (var context = new DataBaseContext())
                            {
                                blog = context.ChargeSheetEnquiryNotice.Where(e => e.Id == data).SingleOrDefault();
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
                            ChargeSheetEnquiryNotice corp = new ChargeSheetEnquiryNotice()
                            {


                                ReplyPeriod = c.ReplyPeriod,
                                EnquiryNoticeNo = c.EnquiryNoticeNo,
                                EnquiryPlace = c.EnquiryPlace,
                                EnquiryNoticeIssueDate = c.EnquiryNoticeIssueDate,
                                EnquiryScheduleDate = c.EnquiryScheduleDate,
                                EnquiryScheduleTime = c.EnquiryScheduleTime,
                                Narration = c.Narration,
                                DBTrack = c.DBTrack,
                                Id = data,
                                RowVersion = (Byte[])TempData["RowVersion"]

                            };



                            blog.DBTrack = c.DBTrack;
                            db.ChargeSheetEnquiryNotice.Attach(blog);
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