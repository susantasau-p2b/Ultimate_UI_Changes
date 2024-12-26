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
    public class chargesheetreplyController : Controller
    {
        //
        // GET: /chargesheetreply/
        public ActionResult Index()
        {
            return View("~/Views/IR/MainViews/chargesheetreply/Index.cshtml");
        }
        public class P2BGridData
        {
            public string VictimName { get; set; }
            public string CaseNo { get; set; }
            public string ProceedingStage { get; set; }

            public string Id { get; set; }
            public string IsDropChargeSheet { get; set; }
            public string IsEnquiryStart { get; set; }
            public string IsNotifyHR { get; set; }
            public string IsPleadGuilty { get; set; }
            public string IsReplySatisfactory { get; set; }
            public string Narration { get; set; }
            public string PenaltyType { get; set; }
            public string PunishmentDetails { get; set; }
            public string PunishmentType { get; set; }
            public string ReplyDate { get; set; }
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

        public ActionResult GetChargeSheetData(string EmpIr)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                try
                {
                    int EmpIrId = Convert.ToInt32(EmpIr);
                    string caseNO = Convert.ToString(Session["findcase"]);
                    //var EmpDisciplines = db.EmpDisciplineProcedings.Include(e => e.MisconductComplaint)
                    //    .Include(e => e.PreminaryEnquiry).Include(e => e.PreminaryEnquiryAction)
                    //    .Include(e => e.ChargeSheet).Include(e => e.ChargeSheetServing)
                    //    .Where(e => e.CaseNo == caseNO ).ToList().LastOrDefault();

                    var EMPIRs = db.EmployeeIR.Select(s => new
                    {
                        Irid = s.Id,
                        oEmpcode = s.Employee.EmpCode,
                        EDP = s.EmpDisciplineProcedings.Select(r => new
                        {
                            EDPid = r.Id,
                            CaseNum = r.CaseNo,
                            oChargeSheetDetails = r.ChargeSheet.ChargeSheetDetails.ToString(),
                        }).Where(e => e.CaseNum == caseNO).ToList(),

                    }).Where(e => e.Irid == EmpIrId).SingleOrDefault();


                    //var getChargesheetdetails = EmpDisciplines.ChargeSheet.ChargeSheetDetails.ToString();
                    if (EMPIRs != null && EMPIRs.EDP.Count() > 0)
                    {
                        foreach (var item in EMPIRs.EDP)
                        {
                            var chk = item.oChargeSheetDetails;
                            if (chk != "" && chk != null)
                            {
                                var getChargesheetdetails = item.oChargeSheetDetails.ToString();

                                return Json(getChargesheetdetails, JsonRequestBehavior.AllowGet);
                            }
                        }
                        
                        
                    }
                    return null;

                }
                catch (Exception Ex)
                {

                    throw Ex;
                }

            }
            //return View();
        }






        [HttpPost]
        public ActionResult Create(ChargeSheetReply c, FormCollection form, string EmpIr)
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
                    string Narration = form["Narration"] == "0" ? "" : form["Narration"];
                    string PunishmentDetails = form["PunishmentDetails"] == "0" ? "" : form["PunishmentDetails"];
                    string ReplyDate = form["ReplyDate"] == "0" ? "" : form["ReplyDate"];
                    if (ReplyDate != null && ReplyDate != "")
                    {
                        var val = DateTime.Parse(ReplyDate);
                        c.ReplyDate = val;
                    }
                   
                    string PunishmentType = form["PunishmentTypelist"] == "" ? null : form["PunishmentTypelist"];

                    if (PunishmentType != null && PunishmentType != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(PunishmentType));
                        c.PunishmentType = val;

                    }
                     string PenaltyType = form["PenaltyTypelist"] == "" ? null : form["PenaltyTypelist"];

                    if (PenaltyType != null && PenaltyType != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(PenaltyType));
                        c.PenaltyType = val;

                    }
                    string EmployeeDocuments = form["ChargeSheetReplyDocList"] == "0" ? null : form["ChargeSheetReplyDocList"];
                      if (EmployeeDocuments != null && EmployeeDocuments != "")
                    {
                        var ids = Utility.StringIdsToListIds(EmployeeDocuments);
                        List<EmployeeDocuments> lookupval = new List<EmployeeDocuments>();
                        foreach (var ca in ids)
                        {
                            var EmployeeDocuments_val = db.EmployeeDocuments.Find(ca);
                            lookupval.Add(EmployeeDocuments_val);
                            c.ChargeSheetReplyDoc = lookupval;
                        }
                    }
                    else
                        c.ChargeSheetReplyDoc = null;
                     if (ModelState.IsValid)
                      {
                          using (TransactionScope ts = new TransactionScope())
                          {
                              c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                              #region Already ChargeSheetReply enquiry records exist checking code
                              var EMPIR = db.EmployeeIR.Select(s => new
                              {
                                  Irid = s.Id,
                                  oEmpcode= s.Employee.EmpCode,
                                  EDP = s.EmpDisciplineProcedings.Select(r => new
                                  {
                                      EDPid = r.Id,
                                      CaseNum = r.CaseNo,
                                      CSR = r.ChargeSheetReply,
                                  }).Where(e => e.CaseNum == caseNO).ToList(),

                              }).Where(e => e.Irid == EmpIrid).SingleOrDefault();


                              if (EMPIR != null)
                              {
                                  var chkEMPIR = EMPIR.EDP.ToList();

                                  foreach (var itemC in chkEMPIR.Where(e => e.CSR != null))
                                  {
                                      if (itemC.EDPid != 0 && itemC.CaseNum != null && itemC.CSR != null)
                                      {
                                          Msg.Add("Record already exist for case no : " + itemC.CaseNum + " & victim :: " + EMPIR.oEmpcode);
                                          return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                      }
                                  }
                              }
                              #endregion

                              ChargeSheetReply ChargeSheetReply = new ChargeSheetReply()
                              {
                                  ReplyDate = c.ReplyDate,
                                  PunishmentType=c.PunishmentType,
                                  PenaltyType=c.PenaltyType,
                                  PunishmentDetails=c.PunishmentDetails,
                                  ChargeSheetReplyDoc = c.ChargeSheetReplyDoc,
                                  IsDropChargeSheet=c.IsDropChargeSheet,
                                  IsEnquiryStart=c.IsEnquiryStart,
                                  IsNotifyHR=c.IsNotifyHR,
                                  IsPleadGuilty=c.IsPleadGuilty,
                                  IsReplySatisfactory=c.IsReplySatisfactory,
                                  Narration=c.Narration,
                                  DBTrack = c.DBTrack
                              };


                              var ChargeSheetReplyValidation = ValidateObj(ChargeSheetReply);
                              if (ChargeSheetReplyValidation.Count > 0)
                              {
                                  foreach (var item in ChargeSheetReplyValidation)
                                  {

                                      Msg.Add("ChargeSheetReply" + item);
                                  }
                                  return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                              }
                              db.ChargeSheetReply.Add(ChargeSheetReply);
                              
                              try
                              {
                                  
                                  db.SaveChanges();

                                  #region  Add NEW Stages in EmpDisciplineProcedings

                                  
                                  //var EmpDisciplines = db.EmpDisciplineProcedings.Include(e => e.MisconductComplaint)
                                  //    .Include(e => e.PreminaryEnquiry).Include(e => e.PreminaryEnquiryAction)
                                  //    .Include(e => e.ChargeSheet)
                                  //    .Where(e => e.CaseNo == caseNO).ToList().LastOrDefault();
                                  var EmpDisciplines = db.EmployeeIR.Include(e => e.EmpDisciplineProcedings)
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.MisconductComplaint))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.PreminaryEnquiry))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.PreminaryEnquiryAction))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheet))
                                   

                                    .Where(e => e.Id == EmpIrid).FirstOrDefault().EmpDisciplineProcedings.Where(e => e.CaseNo == caseNO).FirstOrDefault();


                                  // For ChargeSheetServing Value beacuse of ICollection
                                 // List<ChargeSheetServing> ChargesheetServeVal = new List<ChargeSheetServing>();
                                  var getEmpdisciplineForChargesheetserving = db.EmpDisciplineProcedings.Include(e => e.ChargeSheetServing).Where(e => e.CaseNo == caseNO && e.ProceedingStage == 5).FirstOrDefault();
                                  
                                  //foreach (var item in EmpDisciplines.ChargeSheetServing)
                                  //{
                                  //    var a = db.ChargeSheetServing.Find(item.Id);
                                  //    ChargesheetServeVal.Add(a);
                                  //}
                                 
                                  
                                  EmpDisciplineProcedings EmpDiscipline = new EmpDisciplineProcedings()
                                  {
                                      CaseNo = EmpDisciplines.CaseNo,
                                      CaseOpeningDate = EmpDisciplines.CaseOpeningDate,
                                      ProceedingStage = 6,
                                      MisconductComplaint = EmpDisciplines.MisconductComplaint,
                                      PreminaryEnquiry = EmpDisciplines.PreminaryEnquiry,
                                      PreminaryEnquiryAction = EmpDisciplines.PreminaryEnquiryAction,
                                      ChargeSheet = EmpDisciplines.ChargeSheet,
                                     // ChargeSheetServing = ChargesheetServeVal, 
                                      ChargeSheetServing = getEmpdisciplineForChargesheetserving.ChargeSheetServing.ToList(),
                                      ChargeSheetReply = db.ChargeSheetReply.Find(ChargeSheetReply.Id),
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
                IEnumerable<P2BGridData> ChargeSheetReply = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;
                //var oemployeedata = db.ChargeSheetReply.Include(e=>e.PunishmentType).Include(e=>e.PenaltyType).OrderBy(e => e.Narration).ToList();

                var oemployeedata = db.EmployeeIR.Select(a => new
                {
                    oVictimName = a.Employee.EmpName.FullNameFML,

                    oEmpDiscipline = a.EmpDisciplineProcedings.Select(b => new
                    {
                        oCaseNo = b.CaseNo,
                        oProceedStage = b.ProceedingStage.ToString(),
                        oReplyDate = b.ChargeSheetReply.ReplyDate,
                        oIsDropChargeSheet = b.ChargeSheetReply.IsDropChargeSheet.ToString(),
                        oIsEnquiryStart = b.ChargeSheetReply.IsEnquiryStart.ToString(),
                        oIsNotifyHR = b.ChargeSheetReply.IsNotifyHR.ToString(),
                        oIsPleadGuilty = b.ChargeSheetReply.IsPleadGuilty.ToString(),
                        oIsReplySatisfactory = b.ChargeSheetReply.IsReplySatisfactory.ToString(),
                        oPenaltyType = b.ChargeSheetReply.PenaltyType.LookupVal,
                        oPunishmentType = b.ChargeSheetReply.PunishmentType.LookupVal,
                        oPunishmentDetails = b.ChargeSheetReply.PunishmentDetails,
                        oNarration = b.ChargeSheetReply.Narration,
                        oId = b.ChargeSheetReply.Id.ToString(),

                    }).ToList(),

                }).ToList();

                foreach (var itemIR in oemployeedata.Where(e => e.oEmpDiscipline.Count() > 0))
                {
                    foreach (var item in itemIR.oEmpDiscipline.Where(e => e.oProceedStage == "6").OrderBy(o => o.oCaseNo))
                    {
                        view = new P2BGridData()
                        {
                            CaseNo = item.oCaseNo,
                            VictimName = itemIR.oVictimName.ToString(),
                            ProceedingStage = item.oProceedStage.ToString(),
                            ReplyDate = item.oReplyDate != null ? item.oReplyDate.Value.ToString("dd/MM/yyyy") : "",
                            Id = item.oId.ToString(),
                            Narration = item.oNarration.ToString(),
                            PunishmentDetails = item.oPunishmentDetails.ToString(),
                            IsDropChargeSheet = item.oIsDropChargeSheet.ToString(),
                            IsEnquiryStart = item.oIsEnquiryStart.ToString(),
                            IsNotifyHR = item.oIsNotifyHR.ToString(),
                            IsPleadGuilty = item.oIsPleadGuilty.ToString(),
                            IsReplySatisfactory = item.oIsReplySatisfactory.ToString(),
                            PenaltyType = item.oPenaltyType.ToString(),
                            PunishmentType = item.oPunishmentType.ToString()
                        };

                        model.Add(view);
                    }
                }

                ChargeSheetReply = model;

                IEnumerable<P2BGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = ChargeSheetReply;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE
                         .Where((e => (e.CaseNo.ToString().Contains(gp.searchString))
                                           || (e.VictimName.ToString().ToUpper().Contains(gp.searchString))
                                          || (e.ProceedingStage.ToString().ToUpper().Contains(gp.searchString))
                                         || (e.ReplyDate.ToString().ToUpper().Contains(gp.searchString))
                                        || (e.PunishmentType.ToString().ToUpper().Contains(gp.searchString))
                                       || (e.PenaltyType.ToString().ToUpper().Contains(gp.searchString))
                                     || (e.PunishmentDetails.ToString().ToUpper().Contains(gp.searchString))
                                   || (e.IsDropChargeSheet.ToString().ToUpper().Contains(gp.searchString))
                                     || (e.IsEnquiryStart.ToString().ToUpper().Contains(gp.searchString))
                                       || (e.IsNotifyHR.ToString().ToUpper().Contains(gp.searchString))
                                         || (e.IsPleadGuilty.ToString().ToUpper().Contains(gp.searchString))
                                           || (e.IsReplySatisfactory.ToString().ToUpper().Contains(gp.searchString))
                                             || (e.Narration.ToString().ToUpper().Contains(gp.searchString))
                                               || (e.Id.ToString().Contains(gp.searchString))
                                 )).ToList()
                        .Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, Convert.ToString(a.ReplyDate), a.PunishmentType, a.PenaltyType, a.PunishmentDetails, a.IsDropChargeSheet, a.IsEnquiryStart, a.IsNotifyHR, a.IsPleadGuilty, a.IsReplySatisfactory, a.Narration, a.Id });
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, Convert.ToString(a.ReplyDate), a.PunishmentType, a.PenaltyType, a.PunishmentDetails, a.IsDropChargeSheet, a.IsEnquiryStart, a.IsNotifyHR, a.IsPleadGuilty, a.IsReplySatisfactory, a.Narration, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();


                }
                else
                {
                    IE = ChargeSheetReply;
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
                                         gp.sidx == "ReplyDate" ? c.ReplyDate.ToString() :
                                         gp.sidx == "PunishmentType" ? c.PunishmentType != null ? c.PunishmentType.ToString() : null :
                                         gp.sidx == "PenaltyType" ? c.PenaltyType != null ? c.PenaltyType.ToString() : null :
                                         gp.sidx == "PunishmentDetails" ? c.PunishmentDetails.ToString() :
                                         gp.sidx == "IsDropChargeSheet" ? c.IsDropChargeSheet.ToString() :
                                         gp.sidx == "IsEnquiryStart" ? c.IsEnquiryStart.ToString() :
                                         gp.sidx == "IsNotifyHR" ? c.IsNotifyHR.ToString() :
                                         gp.sidx == "IsPleadGuilty" ? c.IsPleadGuilty.ToString() :
                                         gp.sidx == "IsReplySatisfactory" ? c.IsReplySatisfactory.ToString() :
                                         gp.sidx == "Narration" ? c.Narration.ToString() : "");

                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ReplyDate, a.PunishmentType, a.PenaltyType, a.PunishmentDetails, a.IsDropChargeSheet, a.IsEnquiryStart, a.IsNotifyHR, a.IsPleadGuilty, a.IsReplySatisfactory, a.Narration, a.Id }).ToList();
                     
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);

                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ReplyDate, a.PunishmentType, a.PenaltyType, a.PunishmentDetails, a.IsDropChargeSheet, a.IsEnquiryStart, a.IsNotifyHR, a.IsPleadGuilty, a.IsReplySatisfactory, a.Narration, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, Convert.ToString(a.ReplyDate), a.PunishmentType, a.PenaltyType, a.PunishmentDetails, a.IsDropChargeSheet, a.IsEnquiryStart, a.IsNotifyHR, a.IsPleadGuilty, a.IsReplySatisfactory, a.Narration, a.Id }).ToList();
                    }
                    totalRecords = ChargeSheetReply.Count();
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
        public async Task<ActionResult> EditSave(ChargeSheetReply c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string PunishmentDetails = form["PunishmentDetails"] == "0" ? "" : form["PunishmentDetails"];
                    string PunishmentType = form["PunishmentTypelist"] == "" ? null : form["PunishmentTypelist"];
                    string PenaltyType = form["PenaltyTypelist"] == "" ? null : form["PenaltyTypelist"];
                    string ReplyDate = form["ReplyDate"] == "0" ? "" : form["ReplyDate"];
                    string Narration = form["Narration"] == "0" ? "" : form["Narration"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;



                    if (ReplyDate != null)
                    {
                        if (ReplyDate != "")
                        {

                            var val = DateTime.Parse(ReplyDate);
                            c.ReplyDate = val;
                        }
                    }


                    if (PunishmentType != null)
                    {
                        if (PunishmentType != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(PunishmentType));
                            c.PunishmentType = val;

                            var type = db.ChargeSheetReply.Include(e =>e.PunishmentType).Where(e => e.Id == data).SingleOrDefault();
                            IList<ChargeSheetReply> typedetails = null;
                            if (type.PunishmentType != null)
                            {
                                typedetails = db.ChargeSheetReply.Where(x => x.PunishmentType.Id == type.PunishmentType.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.ChargeSheetReply.Where(x => x.Id == data).ToList();
                            }

                            foreach (var s in typedetails)
                            {
                                s.PunishmentType = c.PunishmentType;
                                db.ChargeSheetReply.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        else
                        {
                            var PunishmentTypeDetails = db.ChargeSheetReply.Include(e => e.PunishmentType).Where(x => x.Id == data).ToList();
                            foreach (var s in PunishmentTypeDetails)
                            {
                                s.PunishmentType = null;
                                db.ChargeSheetReply.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }

                    if (PenaltyType != null)
                    {
                        if (PenaltyType != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(PenaltyType));
                            c.PenaltyType = val;

                            var type = db.ChargeSheetReply.Include(e => e.PenaltyType).Where(e => e.Id == data).SingleOrDefault();
                            IList<ChargeSheetReply> typedetails = null;
                            if (type.PenaltyType != null)
                            {
                                typedetails = db.ChargeSheetReply.Where(x => x.PenaltyType.Id == type.PenaltyType.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.ChargeSheetReply.Where(x => x.Id == data).ToList();
                            }

                            foreach (var s in typedetails)
                            {
                                s.PenaltyType = c.PenaltyType;
                                db.ChargeSheetReply.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        else
                        {
                            var PenaltyTypeDetails = db.ChargeSheetReply.Include(e => e.PenaltyType).Where(x => x.Id == data).ToList();
                            foreach (var s in PenaltyTypeDetails)
                            {
                                s.PenaltyType = null;
                                db.ChargeSheetReply.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }

                    var db_data = db.ChargeSheetReply.Include(e =>e.ChargeSheetReplyDoc).Where(e => e.Id == data).SingleOrDefault();
                    List<EmployeeDocuments> employeedoc = new List<EmployeeDocuments>();
                    string EmployeeDocuments = form["ChargeSheetReplyDocList"] == "0" ? null : form["ChargeSheetReplyDocList"];
                    if (EmployeeDocuments != null)
                    {
                        var ids = Utility.StringIdsToListIds(EmployeeDocuments);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.EmployeeDocuments.Find(ca);

                            employeedoc.Add(Lookup_val);
                            db_data.ChargeSheetReplyDoc = employeedoc;
                        }
                    }
                    else
                    {
                        db_data.ChargeSheetReplyDoc = null;
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
                                    ChargeSheetReply blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.ChargeSheetReply.Where(e => e.Id == data).SingleOrDefault();

                                                                
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

                                    var m1 = db.ChargeSheetReply.Include(e => e.ChargeSheetReplyDoc).Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.ChargeSheetReply.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    var CurCorp = db.ChargeSheetReply.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        // c.DBTrack = dbT;
                                        ChargeSheetReply corp = new ChargeSheetReply()
                                        {

                                            
                                           
                                            
                                            PunishmentDetails=c.PunishmentDetails,
                                            ReplyDate = c.ReplyDate,
                                            IsDropChargeSheet = c.IsDropChargeSheet,
                                            IsEnquiryStart = c.IsEnquiryStart,
                                            IsNotifyHR = c.IsNotifyHR,
                                            IsPleadGuilty = c.IsPleadGuilty,
                                            IsReplySatisfactory = c.IsReplySatisfactory,
                                            Narration = c.Narration,
                                            // UnitId=c.UnitId,
                                            Id = data,
                                            DBTrack = c.DBTrack

                                        };


                                        db.ChargeSheetReply.Attach(corp);
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
                                var clientValues = (ChargeSheetReply)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (ChargeSheetReply)databaseEntry.ToObject();
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

                            ChargeSheetReply blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;


                            using (var context = new DataBaseContext())
                            {
                                blog = context.ChargeSheetReply.Where(e => e.Id == data).SingleOrDefault();
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
                            ChargeSheetReply corp = new ChargeSheetReply()
                            {

                                PunishmentDetails = c.PunishmentDetails,
                                ReplyDate = c.ReplyDate,
                                IsDropChargeSheet = c.IsDropChargeSheet,
                                IsEnquiryStart = c.IsEnquiryStart,
                                IsNotifyHR = c.IsNotifyHR,
                                IsPleadGuilty = c.IsPleadGuilty,
                                IsReplySatisfactory = c.IsReplySatisfactory,
                                Narration = c.Narration,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };



                            blog.DBTrack = c.DBTrack;
                            db.ChargeSheetReply.Attach(blog);
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
                    ChargeSheetReply corporates = db.ChargeSheetReply
                                                             .Include(e => e.ChargeSheetReplyDoc)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    
                    if (corporates.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
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
            public Array Employeedoc_Id { get; set; }
            public Array EmployeedocFullDetails { get; set; }
        }

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.ChargeSheetReply.Include(e=>e.PenaltyType).Include(e=>e.PunishmentType)
                                  .Include(e => e.ChargeSheetReplyDoc)
                                  .Where(e => e.Id == data).AsEnumerable()
                    .Select(e => new
                    {
                        IsDropChargeSheet = e.IsDropChargeSheet,
                        IsEnquiryStart = e.IsEnquiryStart,
                        IsNotifyHR = e.IsNotifyHR,
                        IsPleadGuilty = e.IsPleadGuilty,
                        IsReplySatisfactory = e.IsReplySatisfactory,
                        PenaltyType_Id = e.PenaltyType != null ? e.PenaltyType.Id : 0,
                        PunishmentDetails=e.PunishmentDetails,
                        PunishmentType_Id = e.PunishmentType != null ? e.PunishmentType.Id : 0,
                        ReplyDate=e.ReplyDate,
                        Narration = e.Narration,
                    }).ToList();

                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();
                var k = db.ChargeSheetReply.Include(e => e.ChargeSheetReplyDoc)
                   .Include(e => e.ChargeSheetReplyDoc.Select(a =>a.DocumentType))
                  .Where(e => e.Id == data && e.ChargeSheetReplyDoc.Count > 0).ToList();
                foreach (var e in k)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        Employeedoc_Id = e.ChargeSheetReplyDoc.Select(a => a.Id.ToString()).ToArray(),
                        EmployeedocFullDetails = e.ChargeSheetReplyDoc.Select(a => a.FullDetails).ToArray()
                    });
                }



                return Json(new Object[] { returndata,oreturnEditClass, JsonRequestBehavior.AllowGet });
            }
        }


        
      





                                  



	}
}