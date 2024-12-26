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

using Payroll;


using System.ComponentModel.DataAnnotations;

namespace P2BUltimate.Controllers.IR.MainController
{
    public class ChargeSheetServingController : Controller
    {

        public ActionResult Index()
        {
            return View("~/Views/IR/MainViews/ChargeSheetServing/Index.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/views/Shared/IR/_chargesheetservingmode.cshtml");
        }
        public class P2BGridData
        {
            public string VictimName { get; set; }
            public string CaseNo { get; set; }
            public string ProceedingStage { get; set; }
            public string ChargeSheetServingDate { get; set; }
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
        public ActionResult Create(ChargeSheetServing c, FormCollection form, string EmpIr)
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
                    string ChargeSheetServingDate = form["ChargeSheetServingDate"] == "0" ? "" : form["ChargeSheetServingDate"];
                    if (ChargeSheetServingDate != null && ChargeSheetServingDate != "")
                    {
                        var val = DateTime.Parse(ChargeSheetServingDate);
                        c.ChargeSheetServingDate = val;
                    }
                    string Narration = form["Narration"] == "0" ? "" : form["Narration"];

                    string ChargeSheetServingMode = form["ChargeSheetServingMode"] == "" ? null : form["ChargeSheetServingMode"];
                    if (ChargeSheetServingMode != null && ChargeSheetServingMode != "")
                    {
                        int ChargeSheetServingModeId = int.Parse(ChargeSheetServingMode);
                        var val = db.ChargeSheetServingMode.Include(e => e.ChargeSheetServingModeName).Where(e => e.Id == ChargeSheetServingModeId).SingleOrDefault();
                        c.ChargeSheetServingMode = val;
                    }
                    string Witness = form["WitnessList"] == "" ? null : form["WitnessList"];
                    if (Witness != null)
                    {
                        var ids = Utility.StringIdsToListIds(Witness);
                        var Witnesslist = new List<Witness>();
                        foreach (var item in ids)
                        {

                            int Witnessid = Convert.ToInt32(item);
                            var val = db.EmployeeIR.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id == Witnessid).SingleOrDefault();
                            if (val != null)
                            {
                                c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };
                                Witness objwit = new Witness()
                                {
                                    WitnessEmp = val,
                                    DBTrack = c.DBTrack
                                };

                                Witnesslist.Add(objwit);
                            }
                        }
                        c.Witness = Witnesslist;
                    }
                    else
                        c.Witness = null;

                    using (TransactionScope ts = new TransactionScope())
                    {
                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        #region Already ChargeSheetServing enquiry records exist checking code
                            var EMPIR = db.EmployeeIR.Select(s => new
                            {
                                Irid = s.Id,
                                oEmpcode = s.Employee.EmpCode,
                                EDP = s.EmpDisciplineProcedings.Select(r => new
                                {
                                    EDPid = r.Id,
                                    CaseNum = r.CaseNo,
                                    CSS = r.ChargeSheetServing,
                                }).Where(e => e.CaseNum == caseNO).ToList(),

                            }).Where(e => e.Irid == EmpIrid).SingleOrDefault();
                           

                            if (EMPIR != null)
                            {
                                var chkEMPIR = EMPIR.EDP.ToList();

                                foreach (var itemC in chkEMPIR.Where(e => e.CSS != null))
                                {
                                    if (itemC.EDPid != 0 && itemC.CaseNum != null && itemC.CSS.Count() != 0)
                                    {
                                        Msg.Add("Record already exist for case no : " + itemC.CaseNum + " & victim :: " + EMPIR.oEmpcode);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            #endregion


                        ChargeSheetServing ChargeSheetServing = new ChargeSheetServing()
                        {
                            ChargeSheetServingDate = c.ChargeSheetServingDate,
                            ChargeSheetServingMode = c.ChargeSheetServingMode,
                            IsWitnessReqd = c.IsWitnessReqd,
                            Narration = c.Narration,
                            IsClosedServing = c.IsClosedServing,
                            Witness = c.Witness,
                            DBTrack = c.DBTrack
                        };


                        var ChargeSheetServingValidation = ValidateObj(ChargeSheetServing);
                        if (ChargeSheetServingValidation.Count > 0)
                        {
                            foreach (var item in ChargeSheetServingValidation)
                            {

                                Msg.Add("ChargeSheetServing" + item);
                            }
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        db.ChargeSheetServing.Add(ChargeSheetServing);


                        try
                        {

                            db.SaveChanges();

                            #region  Add NEW Stages in EmpDisciplineProcedings

                            
                            //var EmpDisciplines = db.EmpDisciplineProcedings.Include(e => e.MisconductComplaint)
                            //    .Include(e => e.PreminaryEnquiry).Include(e => e.PreminaryEnquiryAction)
                            //    .Include(e => e.ChargeSheet).Include(e => e.ChargeSheetServing)
                            //    .Where(e => e.CaseNo == caseNO).ToList().LastOrDefault();
                            var EmpDisciplines = db.EmployeeIR.Include(e => e.EmpDisciplineProcedings)
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.MisconductComplaint))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.PreminaryEnquiry))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.PreminaryEnquiryAction))
                                    .Include(e => e.EmpDisciplineProcedings.Select(a => a.ChargeSheet))
                                    

                                    .Where(e => e.Id == EmpIrid).FirstOrDefault().EmpDisciplineProcedings.Where(e => e.CaseNo == caseNO).FirstOrDefault();

                            // Session["Chargesheetdetails"] = EmpDisciplines.ChargeSheet.ChargeSheetDetails.ToString();
                            //List<ChargeSheetServing> chargesheetserving = db.ChargeSheetServing.Where(e => e.Id == ChargeSheetServing.Id).ToList();

                            EmpDisciplineProcedings EmpDiscipline = new EmpDisciplineProcedings()
                            {
                                CaseNo = EmpDisciplines.CaseNo,
                                CaseOpeningDate = EmpDisciplines.CaseOpeningDate,
                                ProceedingStage = 5,
                                MisconductComplaint = EmpDisciplines.MisconductComplaint,
                                PreminaryEnquiry = EmpDisciplines.PreminaryEnquiry,
                                PreminaryEnquiryAction = EmpDisciplines.PreminaryEnquiryAction,
                                ChargeSheet = EmpDisciplines.ChargeSheet,
                                ChargeSheetServing = db.ChargeSheetServing.Where(e => e.Id == ChargeSheetServing.Id).ToList(),
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
                                                        .Where(e => e.Id == EmpDiscipline.Id && e.ProceedingStage == EmpDiscipline.ProceedingStage).SingleOrDefault();


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

                        catch (DataException)
                        {
                            Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
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

                IEnumerable<P2BGridData> ChargeSheetServing = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;
               // var oemployeedata = db.ChargeSheetServing.ToList();

                var oemployeedata = db.EmployeeIR.Select(a => new
                {
                    oVictimName = a.Employee.EmpName.FullNameFML,

                    oEmpDiscipline = a.EmpDisciplineProcedings.Select(b => new
                    {
                        oCaseNo = b.CaseNo,
                        oProceedStage = b.ProceedingStage.ToString(),
                        oChargeSheetServing = b.ChargeSheetServing.Select(c => new { oChargeSheetServingDate = c.ChargeSheetServingDate, oChargeSheetServingId = c.Id.ToString()}).ToList(),
                        

                    }).ToList(),

                }).ToList();


                foreach (var itemIR in oemployeedata.Where(e => e.oEmpDiscipline.Count() > 0))
                {
                    foreach (var item in itemIR.oEmpDiscipline.Where(e => e.oChargeSheetServing.Count() > 0).OrderBy(o => o.oCaseNo))
                    {
                        foreach (var oCSSitem in item.oChargeSheetServing)
                        {
                            view = new P2BGridData()
                            {
                                CaseNo = item.oCaseNo,
                                VictimName = itemIR.oVictimName.ToString(),
                                ProceedingStage = item.oProceedStage.ToString(),
                                ChargeSheetServingDate = oCSSitem.oChargeSheetServingDate != null ? oCSSitem.oChargeSheetServingDate.Value.ToString("dd/MM/yyyy") : "",
                                Id = oCSSitem.oChargeSheetServingId.ToString(),
                            };
                        }
                        model.Add(view);
                    }
                }


                ChargeSheetServing = model;

                IEnumerable<P2BGridData> IE;

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = ChargeSheetServing;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE
                         .Where((e => (e.CaseNo.ToString().Contains(gp.searchString))
                                     || (e.VictimName.ToString().ToUpper().Contains(gp.searchString))
                                     || (e.ProceedingStage.ToString().ToUpper().Contains(gp.searchString))
                                     || (e.ChargeSheetServingDate.ToString().ToUpper().Contains(gp.searchString))

                                  || (e.Id.ToString().Contains(gp.searchString))
                                 )).ToList()
                        .Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ChargeSheetServingDate, a.Id });
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ChargeSheetServingDate, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();


                }
                else
                {
                    IE = ChargeSheetServing;
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : "0");
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "ChargeSheetServingDate" ? c.ChargeSheetServingDate.ToString() :
                                         gp.sidx == "CaseNo" ? c.CaseNo.ToString() :
                                         gp.sidx == "VictimName" ? c.VictimName.ToString() :
                                         gp.sidx == "ProceedingStage" ? c.ProceedingStage.ToString() :
                            "");

                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, Convert.ToString(a.ChargeSheetServingDate), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ChargeSheetServingDate, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ChargeSheetServingDate, a.Id }).ToList();
                    }
                    totalRecords = ChargeSheetServing.Count();
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
        public class returnEditClass
        {
            public Array Witness_Id { get; set; }
            public Array WitnessFullDetails { get; set; }
        }
        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.ChargeSheetServing
                                  .Include(e => e.ChargeSheetServingMode)
                                  .Include(e => e.ChargeSheetServingMode.ChargeSheetServingModeName)
                                  .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        IsWitnessReqd = e.IsWitnessReqd,
                        ChargeSheetServingDate = e.ChargeSheetServingDate,
                        Narration = e.Narration,
                        ChargeSheetServingMode_Id = e.ChargeSheetServingMode.Id == null ? "" : e.ChargeSheetServingMode.Id.ToString(),
                        ChargeSheetServingModeDetails = e.ChargeSheetServingMode.ChargeSheetServingModeName.LookupVal + "," + e.ChargeSheetServingMode.ServingSeq == null ? "" : e.ChargeSheetServingMode.ChargeSheetServingModeName.LookupVal + "," + e.ChargeSheetServingMode.ServingSeq
                    }).ToList();

                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();
                var return_data = db.ChargeSheetServing.Include(e => e.Witness)
                    .Include(e => e.Witness.Select(a => a.WitnessEmp))
                    .Include(e => e.Witness.Select(a => a.WitnessEmp.Employee)).Include(e => e.Witness.Select(a => a.WitnessEmp.Employee.EmpName))
                    .Where(e => e.Id == data && e.Witness.Count > 0).ToList();
                foreach (var e in return_data)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        Witness_Id = e.Witness.Select(a => a.Id.ToString()).ToArray(),
                        WitnessFullDetails = e.Witness.Select(a => "Code : " + a.WitnessEmp.Employee.EmpCode + ", Name : " + a.WitnessEmp.Employee.EmpName.FullNameFML).ToArray()
                    });
                }
                return Json(new Object[] { returndata, oreturnEditClass, JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(ChargeSheetServing c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string ChargeSheetServingMode = form["ChargeSheetServingMode"] == "" ? null : form["ChargeSheetServingMode"];

                    string ChargeSheetServingDate = form["ChargeSheetServingDate"] == "0" ? "" : form["ChargeSheetServingDate"];

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;



                    if (ChargeSheetServingDate != null)
                    {
                        if (ChargeSheetServingDate != "")
                        {

                            var val = DateTime.Parse(ChargeSheetServingDate);
                            c.ChargeSheetServingDate = val;
                        }
                    }


                    if (ChargeSheetServingMode != null)
                    {
                        if (ChargeSheetServingMode != "")
                        {
                            int ChargeSheetServingModeId = int.Parse(ChargeSheetServingMode);
                            var val = db.ChargeSheetServingMode.Include(e => e.ChargeSheetServingModeName).Where(e => e.Id == ChargeSheetServingModeId).SingleOrDefault();
                            c.ChargeSheetServingMode = val;
                            var add = db.ChargeSheetServing.Include(e => e.ChargeSheetServingMode).Include(e => e.ChargeSheetServingMode.ChargeSheetServingModeName).Where(e => e.Id == data).SingleOrDefault();
                            IList<ChargeSheetServing> contactsdetails = null;
                            if (add.ChargeSheetServingMode != null)
                            {
                                contactsdetails = db.ChargeSheetServing.Include(e => e.ChargeSheetServingMode).Where(x => x.ChargeSheetServingMode.ChargeSheetServingModeName.Id == add.ChargeSheetServingMode.ChargeSheetServingModeName.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                contactsdetails = db.ChargeSheetServing.Include(e => e.ChargeSheetServingMode).Where(x => x.Id == data).ToList();
                            }
                            foreach (var s in contactsdetails)
                            {
                                s.ChargeSheetServingMode = c.ChargeSheetServingMode;
                                db.ChargeSheetServing.Attach(s);
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
                        var contactsdetails = db.ChargeSheetServing.Include(e => e.ChargeSheetServingMode).Where(x => x.Id == data).ToList();
                        foreach (var s in contactsdetails)
                        {
                            s.ChargeSheetServingMode = null;
                            db.ChargeSheetServing.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }

                    var db_data = db.ChargeSheetServing.Include(e => e.Witness).Where(e => e.Id == data).SingleOrDefault();
                    List<Witness> witnesslist = new List<Witness>();
                    string Witness = form["WitnessList"] == "" ? null : form["WitnessList"];
                    if (Witness != null)
                    {
                        var ids = Utility.StringIdsToListIds(Witness);
                        foreach (var ca in ids)
                        {
                            c.DBTrack = new DBTrack { Action = "M", ModifiedBy = SessionManager.UserName, IsModified = true, ModifiedOn = DateTime.Now };
                            //var Lookup_val = db.EmployeeIR.Find(ca);
                            int empid = Convert.ToInt32(ca);
                            var empIR_val = db.EmployeeIR.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id == empid).SingleOrDefault();
                            Witness obj = new Witness()
                            {
                                WitnessEmp = empIR_val,
                                DBTrack = c.DBTrack
                            };

                            witnesslist.Add(obj);
                            db_data.Witness = witnesslist;
                        }
                    }
                    else
                    {
                        db_data.Witness = null;
                    }

                    if (Auth == false)
                    {
                        //if (ModelState.IsValid)
                        //{
                        try
                        {
                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                ChargeSheetServing blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.ChargeSheetServing.Where(e => e.Id == data).SingleOrDefault();


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

                                var m1 = db.ChargeSheetServing.Include(e => e.Witness).Where(e => e.Id == data).ToList();
                                foreach (var s in m1)
                                {
                                    // s.AppraisalPeriodCalendar = null;
                                    db.ChargeSheetServing.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                                var CurCorp = db.ChargeSheetServing.Find(data);
                                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    // c.DBTrack = dbT;
                                    ChargeSheetServing corp = new ChargeSheetServing()
                                    {



                                        IsWitnessReqd = c.IsWitnessReqd,

                                        IsClosedServing = c.IsClosedServing,

                                        ChargeSheetServingDate = c.ChargeSheetServingDate,

                                        Narration = c.Narration,
                                        // UnitId=c.UnitId,
                                        Id = data,
                                        DBTrack = c.DBTrack

                                    };


                                    db.ChargeSheetServing.Attach(corp);
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
                            var clientValues = (ChargeSheetServing)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (ChargeSheetServing)databaseEntry.ToObject();
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

                            ChargeSheetServing blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;


                            using (var context = new DataBaseContext())
                            {
                                blog = context.ChargeSheetServing.Where(e => e.Id == data).SingleOrDefault();
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
                            ChargeSheetServing corp = new ChargeSheetServing()
                            {

                                IsWitnessReqd = c.IsWitnessReqd,

                                IsClosedServing = c.IsClosedServing,
                                Narration = c.Narration,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };



                            blog.DBTrack = c.DBTrack;
                            db.ChargeSheetServing.Attach(blog);
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
                    ChargeSheetServing DLdata = db.ChargeSheetServing.Include(e => e.Witness).Where(e => e.Id == data).SingleOrDefault();



                    if (DLdata.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = DLdata.DBTrack.CreatedBy != null ? DLdata.DBTrack.CreatedBy : null,
                                CreatedOn = DLdata.DBTrack.CreatedOn != null ? DLdata.DBTrack.CreatedOn : null,
                                IsModified = DLdata.DBTrack.IsModified == true ? true : false
                            };
                            DLdata.DBTrack = dbT;
                            db.Entry(DLdata).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = DLdata.DBTrack.CreatedBy != null ? DLdata.DBTrack.CreatedBy : null,
                                    CreatedOn = DLdata.DBTrack.CreatedOn != null ? DLdata.DBTrack.CreatedOn : null,
                                    IsModified = DLdata.DBTrack.IsModified == true ? false : false

                                };



                                db.Entry(DLdata).State = System.Data.Entity.EntityState.Deleted;


                                await db.SaveChangesAsync();



                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                            }
                            catch (RetryLimitExceededException)
                            {

                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


                            }
                        }
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
    }
}