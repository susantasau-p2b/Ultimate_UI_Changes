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
using System.IO;

namespace P2BUltimate.Controllers.IR.MainController
{
    public class MisConductComplaintController : Controller
    {


        // GET: /MisConductComplaint/
        public ActionResult Index()
        {
            return View("~/views/IR/MainViews/MisConductComplaint/Index.cshtml");
        }
        public ActionResult Partial()
        {
            //return View("~/Views/Shared/Core/EmployeeDocuments.cshtml");
            return View("~/views/Shared/IR/_OffenseObject.cshtml");
        }
        public class P2BGridData
        {
            public string VictimName { get; set; }
            public string CaseNo { get; set; }
            public int Id { get; set; }

            public int ProceedingStage { get; set; }
            public string ComplaintDate { get; set; }
            public string ComplaintNo { get; set; }
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


        public ActionResult GetComplaintApplicantLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Employee.Include(e => e.EmpName).Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                    .ToList();
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

                // Session["ComplaintAppli"] = r;

                return Json(r, JsonRequestBehavior.AllowGet);

            }

        }






        //public ActionResult GetComplaintApplicantLKDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.ComplaintApplicant.Include(x => x.Employee).Include(x => x.Employee.EmpName).ToList();
        //        IEnumerable<ComplaintApplicant> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.ComplaintApplicant.ToList();
        //        }
        //        else
        //        {
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullDetails }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);

        //    }
        //}
        public ActionResult GetOffenceobjectLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.OffenseObject.Include(x => x.OffenceName).Include(x => x.OffenceType).ToList();
                IEnumerable<OffenseObject> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.OffenseObject.ToList();
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.OffenceType.LookupVal.ToString().ToUpper() + "," + ca.OffenceName.LookupVal.ToString().ToUpper() + "," + ca.DischargeOffencesCount }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.DischargeOffencesCount }).Distinct();
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

        public ActionResult GetLookupVictim(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Employee.Include(e => e.EmpName)
                    .ToList();
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
        public ActionResult Create(MisconductComplaint c, FormCollection form)
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
                    string OffenseObject = form["OffenseObjectList"] == "0" ? "" : form["OffenseObjectList"];
                    string victimList = form["victimList"] == "0" ? "" : form["victimList"];
                    string EmployeeDocuments = form["DocumentsProofList"] == "0" ? "" : form["DocumentsProofList"];
                    string ComplaintApplicant = form["ComplaintApplicantList"] == "0" ? "" : form["ComplaintApplicantList"];
                    string ComplaintDate = form["ComplaintDate"] == "0" ? "" : form["ComplaintDate"];

                    // string filess = form["files"] == null ? null : form["files"];


                    if (OffenseObject != null && OffenseObject != "")
                    {
                        var val = db.OffenseObject.Find(int.Parse(OffenseObject));
                        c.OffenseObject = val;
                    }

                    var victimids = new List<int>();
                    if (victimList != null && victimList != "")
                    {
                        var a = Utility.StringIdsToListIds(victimList);
                        foreach (var item in a)
                        {
                            var victimid = db.Employee.Find(item);
                            victimids.Add(victimid.Id);
                        }

                    }


                    //if (ComplaintApplicant != null && ComplaintApplicant != "")
                    //{
                    //    var val = db.ComplaintApplicant.Find(int.Parse(ComplaintApplicant));
                    //    c.ComplaintApplicant = val;
                    //}
                    if (ComplaintDate != null && ComplaintDate != "")
                    {
                        var val = DateTime.Parse(ComplaintDate);
                        c.ComplaintDate = val;
                    }
                    if (EmployeeDocuments != null && EmployeeDocuments != "")
                    {
                        var ids = Utility.StringIdsToListIds(EmployeeDocuments);
                        List<EmployeeDocuments> EmployeeDocumentsval = new List<EmployeeDocuments>();
                        foreach (var ca in ids)
                        {
                            var EmployeeDocuments_val = db.EmployeeDocuments.Find(ca);
                            EmployeeDocumentsval.Add(EmployeeDocuments_val);
                            c.EmployeeDocuments = EmployeeDocumentsval;
                        }
                    }
                    else
                        c.EmployeeDocuments = null;
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            #region Already misconduct records exist checking code

                            //var EMPIRMis = db.EmployeeIR.Include(e => e.EmpDisciplineProcedings).Include(e => e.EmpDisciplineProcedings.Select(s => s.MisconductComplaint)).ToList();
                            //foreach (var itemz in EMPIRMis)
                            //{
                            //    foreach (var itemedp in itemz.EmpDisciplineProcedings.Where(e => e.CaseNo == caseNO && e.MisconductComplaint != null))
                            //    {
                            //        if (itemedp != null)
                            //        {
                            //            Msg.Add("Misconduct complaint already exist for the case_no : " + itemedp.CaseNo+".");
                            //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //        }
                            //    }
                            //}

                            var getEmpIrList = db.EmployeeIR.Include(e => e.Employee).Include(e => e.EmpDisciplineProcedings).Include(e => e.EmpDisciplineProcedings.Select(s => s.MisconductComplaint)).Where(e => victimids.Contains(e.Employee.Id)).ToList();
                            //var EmpIrId = EMPIR.Id;
                            foreach (var iRItem in getEmpIrList)
                            {
                                if (iRItem != null)
                                {
                                    var Empcode = iRItem.Employee.EmpCode;
                                    var chkEMPIR = iRItem.EmpDisciplineProcedings.Select(a => new
                                    {
                                        EDPid = a.Id,
                                        CaseNum = a.CaseNo,
                                        Miscond = a.MisconductComplaint,


                                    }).Where(e => e.CaseNum == caseNO).ToList();


                                    foreach (var itemC in chkEMPIR)
                                    {
                                        if (itemC.EDPid != 0 && itemC.CaseNum != null && itemC.Miscond != null)
                                        {
                                            Msg.Add("Record already exist for case no : " + itemC.CaseNum + " & victim :: " + Empcode);
                                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                }
                            }


                            #endregion


                            #region Create Complaint Applicant on Creation of MisConduct Complaint

                            int ComplaintAppID = Convert.ToInt32(ComplaintApplicant);

                            Employee complaintapp = db.Employee.Include(e => e.GeoStruct)
                                                    .Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                                    .Where(e => e.Id == ComplaintAppID).FirstOrDefault();
                            ComplaintApplicant OcomplaintApplicant = new ComplaintApplicant()
                            {
                                Employee = complaintapp,
                                GeoStruct = complaintapp.GeoStruct,
                                FuncStruct = complaintapp.FuncStruct,
                                PayStruct = complaintapp.PayStruct,
                                DBTrack = c.DBTrack
                            };

                            db.ComplaintApplicant.Add(OcomplaintApplicant);


                            #endregion


                            MisconductComplaint MisConductComplaint = new MisconductComplaint()
                            {
                                OffenseObject = c.OffenseObject,
                                EmployeeDocuments = c.EmployeeDocuments,
                                ComplaintApplicant = OcomplaintApplicant,
                                GeoStruct = complaintapp.GeoStruct,
                                FuncStruct = complaintapp.FuncStruct,
                                PayStruct = complaintapp.PayStruct,
                                Narration = c.Narration,
                                ComplaintNo = c.ComplaintNo,
                                ComplaintDate = c.ComplaintDate,
                                DBTrack = c.DBTrack
                            };


                            var MisconductComplaintValidation = ValidateObj(MisConductComplaint);
                            if (MisconductComplaintValidation.Count > 0)
                            {
                                foreach (var item in MisconductComplaintValidation)
                                {

                                    Msg.Add("MisConductComplaint" + item);
                                }
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            db.MisconductComplaint.Add(MisConductComplaint);

                            try
                            {
                                db.SaveChanges();

                                int EmpDis_Id = 0;
                                EmpDis_Id = Convert.ToInt32(Session["EmpdisId"]);


                                var EmpDisciplineProcedings = new EmpDisciplineProcedings();
                                var EmployeeIRs = new EmployeeIR();
                                List<EmployeeIR> EmployeeList = new List<EmployeeIR>();


                                List<EmpDisciplineProcedings> myEmpDisList = new List<EmpDisciplineProcedings>();


                                var EmpDisIR = db.EmployeeIR.Where(e => victimids.Contains(e.Employee.Id)).ToList();
                                foreach (var itemEmpDisIR in EmpDisIR)
                                {
                                    Session["EmpDisIR"] = itemEmpDisIR.Id;
                                    EmpDisciplineProcedings = db.EmpDisciplineProcedings.Include(e => e.MisconductComplaint).Where(e => e.CaseNo == caseNO).FirstOrDefault();

                                    if (EmpDisciplineProcedings != null && EmpDisciplineProcedings.ProceedingStage == 0)
                                    {
                                        EmpDisciplineProcedings.MisconductComplaint = MisConductComplaint;
                                        EmpDisciplineProcedings.ProceedingStage = 1;
                                        EmpDisciplineProcedings.DBTrack = new DBTrack { ModifiedOn = DateTime.Now, Action = "M" };
                                        myEmpDisList.Add(EmpDisciplineProcedings);

                                        myEmpDisList.AddRange(itemEmpDisIR.EmpDisciplineProcedings);

                                        itemEmpDisIR.EmpDisciplineProcedings = myEmpDisList;
                                        db.EmployeeIR.Attach(itemEmpDisIR);
                                        db.Entry(itemEmpDisIR).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(itemEmpDisIR).State = System.Data.Entity.EntityState.Detached;
                                        myEmpDisList.Clear();
                                    }
                                    else
                                    {
                                        EmpDisciplineProcedings myNewObj = new EmpDisciplineProcedings()
                                        {
                                            CaseNo = EmpDisciplineProcedings.CaseNo,
                                            CaseOpeningDate = EmpDisciplineProcedings.CaseOpeningDate,
                                            MisconductComplaint = MisConductComplaint,
                                            ProceedingStage = 1,
                                            DBTrack = new DBTrack { CreatedOn = DateTime.Now, CreatedBy = SessionManager.EmpId, Action = "C" },

                                        };
                                        db.EmpDisciplineProcedings.Add(myNewObj);
                                        db.SaveChanges();

                                        myEmpDisList.Add(myNewObj);
                                        myEmpDisList.AddRange(itemEmpDisIR.EmpDisciplineProcedings);

                                        itemEmpDisIR.EmpDisciplineProcedings = myEmpDisList;
                                        db.EmployeeIR.Attach(itemEmpDisIR);
                                        db.Entry(itemEmpDisIR).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(itemEmpDisIR).State = System.Data.Entity.EntityState.Detached;
                                        myEmpDisList.Clear();

                                    }

                                }


                                db.SaveChanges();
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





        //public string InvestmentUploadFile(string FolderName)
        //{
        //    string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(
        //        System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\IRDoc\\" + FolderName + "\\";
        //    String localPath = "";
        //    bool exists = System.IO.Directory.Exists(requiredPath);
        //    if (!exists)
        //    {
        //        localPath = new Uri(requiredPath).LocalPath;
        //        System.IO.Directory.CreateDirectory(localPath);
        //    }
        //    return localPath;
        //}




        //[HttpPost]
        //public ActionResult MiscounductDocUpload(HttpPostedFileBase[] files, FormCollection form, string data, string Id, string SubId)
        //{
        //    if (ModelState.IsValid)
        //    {

        //        string extension, newfilename, deletefilepath = "";
        //        Int32 Count = 0;
        //        string misconductid = "0", complaintdate = "", todate = "", OFinYr = "";
        //        MisconductComplaint itinvestment = null;
        //        string caseNO = Convert.ToString(Session["findcase"]);
        //        Id = caseNO;
        //        //ITSubInvestmentPayment itsubinvestment = null;
        //        using (DataBaseContext db = new DataBaseContext())
        //        {
        //            if (Id != null)
        //            {
        //                //int Casenum = Convert.ToInt32(Id);
        //                var empDIs = db.EmpDisciplineProcedings.Where(e => e.CaseNo == Id).Select(m => m.MisconductComplaint).SingleOrDefault();
        //                itinvestment = db.MisconductComplaint.Where(e => e.Id == empDIs.Id).SingleOrDefault();
        //                if (itinvestment != null)
        //                {
        //                    misconductid = itinvestment.Id.ToString();
        //                }

        //                //OFinYr = itinvestment.FinancialYear.Id.ToString();
        //                complaintdate = itinvestment.ComplaintDate.Value.ToShortDateString();
        //                //todate = itinvestment.FinancialYear.ToDate.Value.Year.ToString();
        //                deletefilepath = "\\P2BUltimate\\IRDoc\\IR\\" + complaintdate + "-" + "\\MisconductComplaint\\"; 
        //            }
        //            //if (SubId != null && SubId != "")
        //            //{
        //            //    int subid = Convert.ToInt32(SubId);
        //            //    itsubinvestment = db.ITSubInvestmentPayment.Where(e => e.Id == subid).SingleOrDefault();
        //            //    subinvestmentid = SubId;
        //            //    deletefilepath = itsubinvestment.Path;
        //            //}
        //            var allowedExtensions = new[] { ".Jpg", ".png", ".jpg", ".jpeg", ".pdf", ".JPG", ".PNG", ".JPG", ".JPEG", ".PDF" };
        //            foreach (HttpPostedFileBase file in files)
        //            {
        //                if (file == null)
        //                {
        //                    return Json(new { success = false, responseText = "Please Select The File..!" }, JsonRequestBehavior.AllowGet);
        //                }
        //                extension = Path.GetExtension(file.FileName);
        //                if (!allowedExtensions.Contains(extension))
        //                {
        //                    return Json(new { success = false, responseText = "File Type Is Not Supported..!" }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //            foreach (HttpPostedFileBase file in files)
        //            {

        //                if (file != null)
        //                {
        //                    extension = Path.GetExtension(file.FileName);
        //                    newfilename =  misconductid + "_" + Id + extension;
        //                    String FolderName = "IR" + complaintdate + "-" + "\\MisconductComplaint\\";

        //                    var InputFileName = Path.GetFileName(file.FileName);
        //                    string ServerSavePath = InvestmentUploadFile(FolderName);
        //                    string ServerMappath = ServerSavePath + newfilename;
        //                    if (deletefilepath != null)
        //                    {
        //                        FileInfo File = new FileInfo(deletefilepath);
        //                        bool exists = File.Exists;
        //                        if (exists)
        //                        {
        //                            System.IO.File.Delete(deletefilepath);
        //                        }
        //                    }
        //                    file.SaveAs(Path.Combine(ServerSavePath, newfilename));
        //                    if (Id != null && SubId == "")
        //                    {
        //                        db.MisconductComplaint.Attach(itinvestment);
        //                        db.Entry(itinvestment).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = itinvestment.RowVersion;
        //                        db.Entry(itinvestment).State = System.Data.Entity.EntityState.Detached;
        //                        itinvestment.DBTrack = new DBTrack
        //                        {
        //                            CreatedBy = itinvestment.DBTrack.CreatedBy == null ? null : itinvestment.DBTrack.CreatedBy,
        //                            CreatedOn = itinvestment.DBTrack.CreatedOn == null ? null : itinvestment.DBTrack.CreatedOn,
        //                            Action = "M",
        //                            ModifiedBy = SessionManager.UserName,
        //                            ModifiedOn = DateTime.Now
        //                        };
        //                        MisconductComplaint ContactDet = itinvestment;
        //                        ContactDet.Narration = ServerMappath;
        //                        ContactDet.DBTrack = itinvestment.DBTrack;

        //                        db.Entry(ContactDet).State = System.Data.Entity.EntityState.Modified;
        //                        db.Entry(ContactDet).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                        db.SaveChanges();
        //                    }


        //                    Count++;
        //                }
        //                else
        //                {

        //                }
        //            }
        //            if (Count > 0)
        //            {
        //                return Json(new { success = true, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
        //            }
        //            //else
        //            //{
        //            //    return Json(new { success = false, responseText = "Something is Wrong..!" }, JsonRequestBehavior.AllowGet);

        //            //}
        //        }
        //        return View();

        //    }
        //    return View();
        //}



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

                IEnumerable<P2BGridData> MisconductComplaint = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;
                //var oemployeedata = db.MisconductComplaint.OrderBy(e => e.ComplaintNo).ToList();

                var oemployeedata = db.EmployeeIR.Select(a => new
                {
                    oVictimName = a.Employee.EmpName.FullNameFML,

                    oEmpDiscipline = a.EmpDisciplineProcedings.Select(b => new
                    {
                        oCaseNo = b.CaseNo,
                        oProceedStage = b.ProceedingStage,
                        oComplaintDate = b.MisconductComplaint.ComplaintDate,
                        oComplaintNo = b.MisconductComplaint.ComplaintNo,
                        oNarration = b.MisconductComplaint.Narration,
                        oId = b.MisconductComplaint.Id,

                    }).ToList(),

                }).ToList();

                foreach (var itemIR in oemployeedata.Where(e => e.oEmpDiscipline.Count() > 0))
                {
                    foreach (var item in itemIR.oEmpDiscipline.Where(e => e.oProceedStage == 1).OrderBy(o => o.oCaseNo))
                    {
                        view = new P2BGridData()
                        {
                            CaseNo = item.oCaseNo,
                            VictimName = itemIR.oVictimName.ToString(),
                            ProceedingStage = item.oProceedStage,
                            ComplaintDate = item.oComplaintDate !=  null ? item.oComplaintDate.Value.ToString("dd/MM/yyyy") : "", 
                            ComplaintNo = item.oComplaintNo.ToString(),
                            Narration = item.oNarration != null ?  item.oNarration : "" ,
                            Id = item.oId,

                        };

                        model.Add(view);
                    }
                }

                MisconductComplaint = model;

                IEnumerable<P2BGridData> IE;

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = MisconductComplaint;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE
                         .Where((e => (e.ComplaintNo.ToString().ToUpper().Contains(gp.searchString))
                                  || (e.ComplaintDate.ToString().ToUpper().Contains(gp.searchString))
                                  || (e.Narration.ToString().ToUpper().Contains(gp.searchString))
                                  || (e.CaseNo.ToString().Contains(gp.searchString))
                                  || (e.VictimName.ToString().ToUpper().Contains(gp.searchString))

                                  || (e.ProceedingStage.ToString().Contains(gp.searchString))
                                  || (e.Id.ToString().Contains(gp.searchString))
                                 )).ToList()
                        .Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ComplaintNo, a.ComplaintDate, a.Narration, a.Id });
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ComplaintNo, a.ComplaintDate, a.Narration, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();


                }
                else
                {
                    IE = MisconductComplaint;
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "ComplaintNo" ? c.ComplaintNo.ToString() :
                                          gp.sidx == "ComplaintDate" ? c.ComplaintNo.ToString() :
                                          gp.sidx == "CaseNo" ? c.CaseNo.ToString() :
                                          gp.sidx == "VictimName" ? c.VictimName.ToString() :
                                           gp.sidx == "ProceedingStage" ? c.ProceedingStage.ToString() :
                                          gp.sidx == "Narration" ? c.Narration.ToString() : "");


                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ComplaintNo, Convert.ToString(a.ComplaintDate), a.Narration, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ComplaintNo, a.ComplaintDate, a.Narration, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CaseNo, a.VictimName, a.ProceedingStage, a.ComplaintNo, a.ComplaintDate, a.Narration, a.Id }).ToList();
                    }
                    totalRecords = MisconductComplaint.Count();
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
                    MisconductComplaint corporates = db.MisconductComplaint.Include(e => e.OffenseObject)
                                                             .Include(e => e.ComplaintApplicant)
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

        public class returnEditClassTwo
        {
            public Array ReturnVictim_Id { get; set; }
            public Array VictimFulldetails { get; set; }
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.MisconductComplaint

                            .Include(e => e.OffenseObject)
                            .Include(e => e.ComplaintApplicant)
                                  .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        Narration = e.Narration,
                        ComplaintDate = e.ComplaintDate,
                        ComplaintNo = e.ComplaintNo,
                        OffenseObject_Id = e.OffenseObject.Id == null ? "" : e.OffenseObject.Id.ToString(),
                        OffenseObjectDetails = e.OffenseObject.OffenceType.LookupVal.ToString().ToUpper() + "," + e.OffenseObject.OffenceName.LookupVal.ToString().ToUpper() + "," + e.OffenseObject.DischargeOffencesCount == null ? "" : e.OffenseObject.OffenceType.LookupVal.ToString().ToUpper() + "," + e.OffenseObject.OffenceName.LookupVal.ToString().ToUpper() + "," + e.OffenseObject.DischargeOffencesCount,
                        ComplaintApplicant_Id = e.ComplaintApplicant.Id == null ? "" : e.ComplaintApplicant.Id.ToString(),
                        ComplaintApplicantDetails = e.ComplaintApplicant.Employee.Id == null ? "" : e.ComplaintApplicant.Employee.FullDetails.ToString(),
                    }).ToList();

                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();
                var k = db.MisconductComplaint.Include(e => e.EmployeeDocuments)
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

                List<returnEditClassTwo> oreturnEditClasstwo = new List<returnEditClassTwo>();
                oreturnEditClasstwo.Add(new returnEditClassTwo
                   {
                       ReturnVictim_Id = db.EmployeeIR.Include(e => e.Employee).Select(a => a.Id.ToString()).ToArray(),
                       VictimFulldetails = db.EmployeeIR.Include(e => e.Employee).Select(a => a.Employee.FullDetails).ToArray()
                   });
                return Json(new Object[] { returndata, oreturnEditClass, oreturnEditClasstwo, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        //public async Task<ActionResult> EditSave(MisconductComplaint c, int data, FormCollection form) // Edit submit
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<string> Msg = new List<string>();
        //        try
        //        {
        //            string OffenseObject = form["OffenseObjectList"] == "0" ? "" : form["OffenseObjectList"];
        //            string victimList = form["victimList"] == "0" ? "" : form["victimList"];
        //            string ComplaintApplicant = form["ComplaintApplicantList"] == "0" ? "" : form["ComplaintApplicantList"];
        //            string EmployeeDocuments = form["DocumentsProofList"] == "0" ? "" : form["DocumentsProofList"];
        //            string ComplaintDate = form["ComplaintDate"] == "0" ? "" : form["ComplaintDate"];
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;

        //            if (ComplaintDate != null)
        //            {
        //                if (ComplaintDate != "")
        //                {

        //                    var val = DateTime.Parse(ComplaintDate);
        //                    c.ComplaintDate = val;
        //                }
        //            }





        //            if (OffenseObject != null)
        //            {
        //                if (OffenseObject != "")
        //                {
        //                    var val = db.OffenseObject.Find(int.Parse(OffenseObject));
        //                    c.OffenseObject = val;

        //                    var add = db.MisconductComplaint.Include(e => e.OffenseObject).Where(e => e.Id == data).SingleOrDefault();
        //                    IList<MisconductComplaint> contactsdetails = null;
        //                    if (add.OffenseObject != null)
        //                    {
        //                        contactsdetails = db.MisconductComplaint.Where(x => x.OffenseObject.Id == add.OffenseObject.Id && x.Id == data).ToList();
        //                    }
        //                    else
        //                    {
        //                        contactsdetails = db.MisconductComplaint.Where(x => x.Id == data).ToList();
        //                    }
        //                    foreach (var s in contactsdetails)
        //                    {
        //                        s.OffenseObject = c.OffenseObject;
        //                        db.MisconductComplaint.Attach(s);
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                        //await db.SaveChangesAsync();
        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = s.RowVersion;
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                var contactsdetails = db.MisconductComplaint.Include(e => e.OffenseObject).Where(x => x.Id == data).ToList();
        //                foreach (var s in contactsdetails)
        //                {
        //                    s.OffenseObject = null;
        //                    db.MisconductComplaint.Attach(s);
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                    //await db.SaveChangesAsync();
        //                    db.SaveChanges();
        //                    TempData["RowVersion"] = s.RowVersion;
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                }
        //            }
        //            if (ComplaintApplicant != null)
        //            {
        //                if (ComplaintApplicant != "")
        //                {
        //                    var val = db.ComplaintApplicant.Find(int.Parse(ComplaintApplicant));
        //                    c.ComplaintApplicant = val;

        //                    var add = db.MisconductComplaint.Include(e => e.ComplaintApplicant).Where(e => e.Id == data).SingleOrDefault();
        //                    IList<MisconductComplaint> contactsdetails = null;
        //                    if (add.OffenseObject != null)
        //                    {
        //                        contactsdetails = db.MisconductComplaint.Where(x => x.ComplaintApplicant.Id == add.ComplaintApplicant.Id && x.Id == data).ToList();
        //                    }
        //                    else
        //                    {
        //                        contactsdetails = db.MisconductComplaint.Where(x => x.Id == data).ToList();
        //                    }
        //                    foreach (var s in contactsdetails)
        //                    {
        //                        s.ComplaintApplicant = c.ComplaintApplicant;
        //                        db.MisconductComplaint.Attach(s);
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                        //await db.SaveChangesAsync();
        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = s.RowVersion;
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                var contactsdetails = db.MisconductComplaint.Include(e => e.ComplaintApplicant).Where(x => x.Id == data).ToList();
        //                foreach (var s in contactsdetails)
        //                {
        //                    s.ComplaintApplicant = null;
        //                    db.MisconductComplaint.Attach(s);
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                    //await db.SaveChangesAsync();
        //                    db.SaveChanges();
        //                    TempData["RowVersion"] = s.RowVersion;
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                }
        //            }

        //            var db_data = db.MisconductComplaint.Include(e => e.EmployeeDocuments).Where(e => e.Id == data).SingleOrDefault();
        //            List<EmployeeDocuments> employeedoc = new List<EmployeeDocuments>();

        //            if (EmployeeDocuments != null)
        //            {
        //                var ids = Utility.StringIdsToListIds(EmployeeDocuments);
        //                foreach (var ca in ids)
        //                {
        //                    var EmployeeDocuments_val = db.EmployeeDocuments.Find(ca);

        //                    employeedoc.Add(EmployeeDocuments_val);
        //                    db_data.EmployeeDocuments = employeedoc;
        //                }
        //            }
        //            else
        //            {
        //                db_data.EmployeeDocuments = null;
        //            }

        //            if (Auth == false)
        //            {
        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {
        //                        //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            MisconductComplaint blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.MisconductComplaint.Where(e => e.Id == data).SingleOrDefault();


        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            c.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };

        //                            var m1 = db.MisconductComplaint.Include(e => e.EmployeeDocuments).Where(e => e.Id == data).ToList();
        //                            foreach (var s in m1)
        //                            {
        //                                // s.AppraisalPeriodCalendar = null;
        //                                db.MisconductComplaint.Attach(s);
        //                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                //await db.SaveChangesAsync();
        //                                db.SaveChanges();
        //                                TempData["RowVersion"] = s.RowVersion;
        //                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                            }
        //                            var CurCorp = db.MisconductComplaint.Find(data);
        //                            TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            {
        //                                // c.DBTrack = dbT;
        //                                MisconductComplaint corp = new MisconductComplaint()
        //                                {

        //                                    Narration = c.Narration,
        //                                    ComplaintNo = c.ComplaintNo,
        //                                    ComplaintDate = c.ComplaintDate,
        //                                    Id = data,
        //                                    DBTrack = c.DBTrack

        //                                };


        //                                db.MisconductComplaint.Attach(corp);
        //                                db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //                                db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];



        //                                await db.SaveChangesAsync();
        //                                ts.Complete();
        //                                Msg.Add("  Record Updated  ");
        //                                //  return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.LocationObj.LocDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                            }


        //                        }
        //                    }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (MisconductComplaint)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {
        //                            Msg.Add(" Unable to save changes. The record was deleted by another user. ");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (MisconductComplaint)databaseEntry.ToObject();
        //                            c.RowVersion = databaseValues.RowVersion;

        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        LogFile Logfile = new LogFile();
        //                        ErrorLog Err = new ErrorLog()
        //                        {
        //                            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                            ExceptionMessage = ex.Message,
        //                            ExceptionStackTrace = ex.StackTrace,
        //                            LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                            LogTime = DateTime.Now
        //                        };
        //                        Logfile.CreateLogFile(Err);
        //                        Msg.Add(ex.Message);
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    }
        //                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    MisconductComplaint blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;


        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.MisconductComplaint.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }
        //                    c.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };
        //                    MisconductComplaint corp = new MisconductComplaint()
        //                    {

        //                        Narration = c.Narration,
        //                        ComplaintNo = c.ComplaintNo,
        //                        ComplaintDate = c.ComplaintDate,
        //                        Id = data,
        //                        DBTrack = c.DBTrack,
        //                        RowVersion = (Byte[])TempData["RowVersion"]
        //                    };



        //                    blog.DBTrack = c.DBTrack;
        //                    db.MisconductComplaint.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    Msg.Add("  Record Updated  ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return Json(new Object[] { blog.Id, c.LocationObj.LocDesc, "Record Updated", JsonRequestBehavior.AllowGet });
        //                }

        //            }
        //            return View();
        //        }
        //        catch (Exception ex)
        //        {
        //            LogFile Logfile = new LogFile();
        //            ErrorLog Err = new ErrorLog()
        //            {
        //                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                ExceptionMessage = ex.Message,
        //                ExceptionStackTrace = ex.StackTrace,
        //                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}
        public async Task<ActionResult> EditSave(MisconductComplaint c, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        string OffenseObject = form["OffenseObjectList"] == "0" ? "" : form["OffenseObjectList"];
                        string ComplaintApplicant = form["ComplaintApplicantList"] == "0" ? "" : form["ComplaintApplicantList"];
                        string EmployeeDocuments = form["DocumentsProofList"] == "0" ? "" : form["DocumentsProofList"];
                        string ComplaintDate = form["ComplaintDate"] == "0" ? "" : form["ComplaintDate"];
                        string victimList = form["victimList"] == "0" ? "" : form["victimList"];

                        if (OffenseObject != null && OffenseObject != "")
                        {
                            int AppamId = Convert.ToInt32(OffenseObject);
                            var val = db.OffenseObject
                                                .Where(e => e.Id == AppamId).SingleOrDefault();
                            c.OffenseObject = val;
                        }

                        if (ComplaintApplicant != null && ComplaintApplicant != "")
                        {
                            int AppamId = Convert.ToInt32(ComplaintApplicant);
                            var val = db.ComplaintApplicant
                                                .Where(e => e.Id == AppamId).SingleOrDefault();
                            c.ComplaintApplicant = val;
                        }

                        if (ComplaintDate != null)
                        {
                            if (ComplaintDate != "")
                            {

                                var val = DateTime.Parse(ComplaintDate);
                                c.ComplaintDate = val;
                            }
                        }
                        var victimids = 0;
                        if (victimList != null && victimList != "")
                        {
                            var victimid = db.Employee.Find(int.Parse(victimList));
                            victimids = Convert.ToInt32(victimid.Id);
                            //db.EmployeeIR.Include(e => e.Employee).Where(e => e.id) = val;
                        }

                        if (EmployeeDocuments != null && EmployeeDocuments != "")
                        {
                            var ids = Utility.StringIdsToListIds(EmployeeDocuments);
                            List<EmployeeDocuments> EmployeeDocuments1 = new List<EmployeeDocuments>();
                            foreach (var ca in ids)
                            {
                                var EmployeeDocuments1_val = db.EmployeeDocuments.Find(ca);
                                EmployeeDocuments1.Add(EmployeeDocuments1_val);
                                c.EmployeeDocuments = EmployeeDocuments1;
                            }
                        }
                        else
                            c.EmployeeDocuments = null;

                        var db_data = db.MisconductComplaint.Where(e => e.Id == data).SingleOrDefault();
                        TempData["RowVersion"] = db_data.RowVersion;
                        MisconductComplaint MisconductComplaint = db.MisconductComplaint.Find(data);
                        TempData["CurrRowVersion"] = MisconductComplaint.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = MisconductComplaint.DBTrack.CreatedBy == null ? null : MisconductComplaint.DBTrack.CreatedBy,
                                CreatedOn = MisconductComplaint.DBTrack.CreatedOn == null ? null : MisconductComplaint.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            MisconductComplaint.Id = data;
                            MisconductComplaint.ComplaintApplicant = c.ComplaintApplicant;
                            MisconductComplaint.OffenseObject = c.OffenseObject;
                            MisconductComplaint.ComplaintDate = c.ComplaintDate;
                            MisconductComplaint.EmployeeDocuments = c.EmployeeDocuments;
                            MisconductComplaint.ComplaintNo = c.ComplaintNo;
                            MisconductComplaint.Narration = c.Narration;
                            MisconductComplaint.DBTrack = c.DBTrack;


                            #region Edit Victim IDs

                            Employee oEMP = new Employee();
                            oEMP = db.Employee.Where(e => e.Id == victimids).FirstOrDefault();
                            EmployeeIR EmployeeIR = db.EmployeeIR.Include(e => e.Employee).Where(e => e.Employee.Id == oEMP.Id).FirstOrDefault();

                            //var EmployeeIRs = new EmployeeIR();
                            //List<EmployeeIR> EmployeeList = new List<EmployeeIR>();

                            ////Login OLogin = db.Login.Where(e => e.Id == DBloginIds.Id).FirstOrDefault();
                            if (oEMP != null)
                            {
                                EmployeeIR.Employee = oEMP;

                            }
                            db.EmployeeIR.Attach(EmployeeIR);

                            #endregion

                            db.Entry(MisconductComplaint).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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




