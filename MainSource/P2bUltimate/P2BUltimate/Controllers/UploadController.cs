using P2b.Global;
using Payroll;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Text;
using P2BUltimate.Security;
using Newtonsoft.Json;
using System.IO;
using System.Configuration;


namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class UploadController : Controller
    {
        // GET: Upload

        public ActionResult Index()
        {
            return View("~/Views/Shared/_Upload.cshtml");
            //D:\LATESTCHECKOUT\P2bUltimate\P2BUltimate\Views\Shared\_Upload.cshtml
        }
        public ActionResult UploadInvestment()
        {
            return View("~/Views/Shared/_UploadInvestment.cshtml");
            //D:\LATESTCHECKOUT\P2bUltimate\P2BUltimate\Views\Shared\_Upload.cshtml
        }
        public ActionResult Imageviewr()
        {
            return View("~/Views/Shared/_ImageViewer.cshtml");
            //D:\LATESTCHECKOUT\P2bUltimate\P2BUltimate\Views\Shared\_Upload.cshtml
        }
        public string EmpUploadFile(string FolderName)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\Images\\EmpProFiles\\profile\\" + FolderName + "\\";
            String localPath = "";
            bool exists = System.IO.Directory.Exists(requiredPath);
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            return localPath;
        }

        //[HttpPost]
        //public ActionResult UploadFiles(HttpPostedFileBase[] files, FormCollection form)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var aa = form["type"] != null ? form["type"] : null;
        //        Int32 Count = 0;
        //        if (aa == null)
        //        {
        //            return Json(new { success = false, responseText = "Select Type..!" }, JsonRequestBehavior.AllowGet);
        //        }
        //        var lookup = Convert.ToInt32(aa);
        //        var allowedExtensions = new[] { ".Jpg", ".png", ".jpg", "jpeg" };
        //        foreach (HttpPostedFileBase file in files)
        //        {
        //            var ext = Path.GetExtension(file.FileName);
        //            if (!allowedExtensions.Contains(ext))
        //            {
        //                return Json(new { success = false, responseText = "File Type Is Not Supported..!" }, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //        foreach (HttpPostedFileBase file in files)
        //        {
        //            using (DataBaseContext db = new DataBaseContext())
        //            {
        //                if (file != null)
        //                {
        //                    var LookupVal = db.LookupValue.Where(e => e.Id == lookup).SingleOrDefault();
        //                    String FolderName = "";
        //                    if (LookupVal != null)
        //                    {
        //                        FolderName = LookupVal.LookupVal;
        //                        var InputFileName = Path.GetFileName(file.FileName);
        //                        var ServerSavePath = UploadFile(FolderName) + InputFileName;
        //                        var FileName = Path.GetFileNameWithoutExtension(ServerSavePath);
        //                        if (db.Employee.Any(e => e.EmpCode == FileName.Trim()))
        //                        {
        //                            EmployeeDocuments oEmpDocument = new EmployeeDocuments
        //                            {
        //                                DocumentPath = ServerSavePath,
        //                                DocumentType = db.LookupValue.Where(e => e.Id == lookup).SingleOrDefault(),
        //                                UploadDate = DateTime.Now,
        //                                DBTrack = { Action = "C", CreatedBy = SessionManager.EmpId, IsModified = false }
        //                            };
        //                            db.EmployeeDocuments.Add(oEmpDocument);
        //                            db.SaveChanges();
        //                            var oEmployee = db.Employee.Where(e => e.EmpCode == FileName.Trim()).SingleOrDefault();
        //                            oEmployee.EmployeeDocuments.Add(oEmpDocument);
        //                            db.Entry(oEmployee).State = System.Data.Entity.EntityState.Modified;
        //                            db.SaveChanges();
        //                            file.SaveAs(ServerSavePath);
        //                            Count++;
        //                        }
        //                    }

        //                }
        //                else
        //                {

        //                }
        //            }
        //        }
        //        if (Count > 0)
        //        {

        //            return Json(new { success = true, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { success = false, responseText = "Something is Wrong..!" }, JsonRequestBehavior.AllowGet);

        //        }
        //    }
        //    return View();
        //}

        [HttpPost]
        public ActionResult UploadFiles(HttpPostedFileBase[] files, EmployeeDocuments CanDoc, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    try
                    {
                        string extension, newfilename = "";
                        Int32 Count = 0;

                        string DocType = form["type"] == "0" ? "" : form["type"];
                        string SubDocName = form["SubDocList"] == "0" ? "" : form["SubDocList"];

                        string LookupSubDocName = "";
                        //string ServerSavePath = "";
                        string NewPath = "";
                        string deletefilepath = "";


                        if (DocType != "" && DocType != null)
                        {
                            var val = db.LookupValue.Find(int.Parse(DocType));
                            CanDoc.DocumentType = val;
                        }

                        if (SubDocName != "" && SubDocName != null)
                        {
                            var val = db.LookupValue.Find(int.Parse(SubDocName));
                            CanDoc.SubDocumentName = val;
                            LookupSubDocName = CanDoc.SubDocumentName.LookupVal;
                        }

                        List<EmployeeDocuments> empdocument = new List<EmployeeDocuments>();

                        if (ModelState.IsValid)
                        {

                            ///fileupload
                            ///



                            int empcode1 = Convert.ToInt32(SessionManager.EmpId);

                            Employee empcode = db.Employee.Include(e => e.EmployeeDocuments).Where(e => e.Id == empcode1).FirstOrDefault();

                            var allowedExtensions = new[] { ".Jpg", ".png", ".jpg", ".jpeg", ".JPG", ".PNG", ".JPG", ".JPEG" };
                            foreach (HttpPostedFileBase file in files)
                            {
                                if (file == null)
                                {
                                    return Json(new { success = false, responseText = "Please Select The File..!" }, JsonRequestBehavior.AllowGet);
                                }
                                extension = Path.GetExtension(file.FileName);
                                if (!allowedExtensions.Contains(extension))
                                {
                                    return Json(new { success = false, responseText = "File Type Is Not Supported..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }

                            foreach (HttpPostedFileBase file in files)
                            {

                                if (file != null)
                                {
                                    extension = Path.GetExtension(file.FileName);

                                    newfilename = LookupSubDocName + extension;

                                    String FolderName = "EmpCode" + empcode.EmpCode + "\\";

                                    //var InputFileName = Path.GetFileName(file.FileName);
                                    //ServerSavePath = EmpUploadFile(FolderName);
                                    //string ServerMappath = ServerSavePath + newfilename;

                                    var ServerSavePath = ConfigurationManager.AppSettings["EmployeeDocuments"];
                                    if (ServerSavePath == null)
                                    {
                                        return Json(new { success = false, responseText = "Please contact the admin to define the document path." }, JsonRequestBehavior.AllowGet);
                                    }
                                    string ServerMappath = ServerSavePath + FolderName + newfilename;
                                    deletefilepath = ServerMappath;

                                    if (deletefilepath != null)
                                    {
                                        FileInfo File = new FileInfo(deletefilepath);
                                        bool exists = File.Exists;
                                        if (exists)
                                        {
                                            System.IO.File.Delete(deletefilepath);
                                        }
                                    }

                                    if (!Directory.Exists(ServerSavePath + FolderName))
                                    {
                                        Directory.CreateDirectory(ServerSavePath + FolderName);
                                    }

                                    file.SaveAs(Path.Combine(ServerMappath));
                                    NewPath = ServerMappath;
                                    //NewPath = Path.Combine(ServerSavePath, newfilename);
                                    //file.SaveAs(Path.Combine(ServerSavePath, newfilename));

                                    Count++;
                                }
                                else
                                {

                                }
                            }

                            /////

                            CanDoc.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId };

                            EmployeeDocuments CandidateDocs = new EmployeeDocuments()
                            {
                                DocumentPath = NewPath,
                                DocumentType = CanDoc.DocumentType,
                                ApprovedDate = DateTime.Now,
                                IsApproved = true,
                                SubDocumentName = CanDoc.SubDocumentName,
                                UploadDate = DateTime.Now,
                                DBTrack = CanDoc.DBTrack
                            };

                            db.EmployeeDocuments.Add(CandidateDocs);
                            db.SaveChanges();

                            empdocument.Add(db.EmployeeDocuments.Find(CandidateDocs.Id));
                            if (empcode.EmployeeDocuments.Count() > 0)
                            {
                                empdocument.AddRange(empcode.EmployeeDocuments);
                            }
                            empcode.EmployeeDocuments = empdocument;
                            db.Employee.Attach(empcode);
                            db.Entry(empcode).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();


                            using (TransactionScope ts = new TransactionScope())
                            {

                                ts.Complete();
                                Msg.Add("Data Saved successfully");
                                return Json(new Utility.JsonReturnClass { Id = CandidateDocs.Id, Val = CandidateDocs.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                            // return Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        }
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Create", new { concurrencyError = true, id = CanDoc.Id });
                    }
                    catch (Exception e)
                    {
                        Msg.Add(e.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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


        public string InvestmentUploadFile(string FolderName)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\Images\\" + FolderName + "\\";
            String localPath = "";
            bool exists = System.IO.Directory.Exists(requiredPath);
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            return localPath;
        }
        [HttpPost]
        public ActionResult InvestmentUpload(HttpPostedFileBase[] files, FormCollection form, string data, string Id, string SubId)
        {
            if (ModelState.IsValid)
            {

                string extension, newfilename, deletefilepath = "";
                Int32 Count = 0;
                string investmentid = "0", subinvestmentid = "0", fromdate = "", todate = "", OFinYr = "",EmpId="";
                ITInvestmentPayment itinvestment = null;
                ITSubInvestmentPayment itsubinvestment = null;
                var Module_Name = HttpContext.Session["ModuleType"];
                string ModuleName = Module_Name.ToString();
                using (DataBaseContext db = new DataBaseContext())
                {
                    if (Id != null)
                    {
                        int itid = Convert.ToInt32(Id);
                        itinvestment = db.ITInvestmentPayment.Include(e => e.EmployeePayroll)
                            .Include(e => e.EmployeePayroll.Employee).Include(e => e.ITInvestment)
                            .Include(e => e.FinancialYear).Include(e => e.ITSubInvestmentPayment)                           
                            .Where(e => e.Id == itid).SingleOrDefault();

                        if (itinvestment.ITInvestment != null)
                        {
                            int Emp_Id = itinvestment.EmployeePayroll.Employee.Id;
                            EmpId = Convert.ToString(Emp_Id);
                            investmentid = itinvestment.ITInvestment.Id.ToString();
                        }
                        OFinYr = itinvestment.FinancialYear.Id.ToString();
                        fromdate = itinvestment.FinancialYear.FromDate.Value.Year.ToString();
                        todate = itinvestment.FinancialYear.ToDate.Value.Year.ToString();
                        deletefilepath = itinvestment.Path;
                    }
                    if (SubId != null && SubId != "")
                    {
                        int subid = Convert.ToInt32(SubId);
                        itsubinvestment = db.ITSubInvestmentPayment.Where(e => e.Id == subid).SingleOrDefault();                         
                        subinvestmentid = SubId;
                        deletefilepath = itsubinvestment.Path;
                    }
                    var allowedExtensions = new[] { ".Jpg", ".png", ".jpg", ".jpeg", ".pdf", ".JPG", ".PNG", ".JPG", ".JPEG", ".PDF" };
                    foreach (HttpPostedFileBase file in files)
                    {
                        if (file == null)
                        {
                            return Json(new { success = false, responseText = "Please Select The File..!" }, JsonRequestBehavior.AllowGet);
                        }
                        extension = Path.GetExtension(file.FileName);
                        if (!allowedExtensions.Contains(extension))
                        {
                            return Json(new { success = false, responseText = "File Type Is Not Supported..!" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    foreach (HttpPostedFileBase file in files)
                    {

                        if (file != null)
                        {
                            extension = Path.GetExtension(file.FileName);
                            newfilename =  investmentid + "_" + subinvestmentid + "_" + Id + extension;

                            String FolderName = EmpId +"\\" +ModuleName +"\\"+"InvestmentCertificate"+"\\"+"FinancialYear" + fromdate + "-" + todate;

                            string ServerSavePath = ConfigurationManager.AppSettings["EmployeeDocuments"];
                            if (ServerSavePath == null)
                            {
                                return Json(new { success = false, responseText = "Please contact the admin to define the document path." }, JsonRequestBehavior.AllowGet);
                            }
                            //var InputFileName = Path.GetFileName(file.FileName);
                            //string ServerSavePath = InvestmentUploadFile(FolderName);                      
                            //string ServerMappath = ServerSavePath + newfilename;
                            string ServerMappath = ServerSavePath + FolderName+"\\"+ newfilename;

                            deletefilepath = ServerMappath;

                            if (deletefilepath != null && deletefilepath != "")
                            {
                                FileInfo File = new FileInfo(deletefilepath);

                                bool exists = File.Exists;
                                if (exists)
                                {
                                    System.IO.File.Delete(deletefilepath);
                                }
                            }
                            if (!Directory.Exists(ServerSavePath + FolderName))
                            {
                                Directory.CreateDirectory(ServerSavePath + FolderName);
                            }
                            //file.SaveAs(Path.Combine(ServerSavePath, newfilename));
                            file.SaveAs(Path.Combine(ServerMappath));

                            if (Id != null && SubId == "")
                            {
                                db.ITInvestmentPayment.Attach(itinvestment);
                                db.Entry(itinvestment).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = itinvestment.RowVersion;
                                db.Entry(itinvestment).State = System.Data.Entity.EntityState.Detached;
                                itinvestment.DBTrack = new DBTrack
                                {
                                    CreatedBy = itinvestment.DBTrack.CreatedBy == null ? null : itinvestment.DBTrack.CreatedBy,
                                    CreatedOn = itinvestment.DBTrack.CreatedOn == null ? null : itinvestment.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };
                                ITInvestmentPayment ContactDet = itinvestment;
                                ContactDet.Path = FolderName + "\\" + newfilename;
                                ContactDet.DBTrack = itinvestment.DBTrack;

                                db.Entry(ContactDet).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(ContactDet).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                db.SaveChanges();
                            }
                            else
                            {
                                db.ITSubInvestmentPayment.Attach(itsubinvestment);
                                db.Entry(itsubinvestment).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = itsubinvestment.RowVersion;
                                db.Entry(itsubinvestment).State = System.Data.Entity.EntityState.Detached;
                                itsubinvestment.DBTrack = new DBTrack
                                {
                                    CreatedBy = itsubinvestment.DBTrack.CreatedBy == null ? null : itsubinvestment.DBTrack.CreatedBy,
                                    CreatedOn = itsubinvestment.DBTrack.CreatedOn == null ? null : itsubinvestment.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };
                                ITSubInvestmentPayment ContactDet = itsubinvestment;
                                ContactDet.Path = FolderName + "\\" + newfilename;
                                ContactDet.DBTrack = itsubinvestment.DBTrack;

                                db.Entry(ContactDet).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(ContactDet).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                db.SaveChanges();
                            }
                            Count++;
                        }
                        else
                        {

                        }
                    }
                    if (Count > 0)
                    {
                        return Json(new { success = true, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                    }
                    //else
                    //{
                    //    return Json(new { success = false, responseText = "Something is Wrong..!" }, JsonRequestBehavior.AllowGet);

                    //}
                }
                return View();

            }
            return View();
        }

        public ActionResult CheckUploadFile(string id, string SubId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string localpath = "";
                ITInvestmentPayment itinvestment = null;
                ITSubInvestmentPayment itsubinvestment = null;
                if (id != null && id != "" && SubId == "")
                {
                    int itid = Convert.ToInt32(id);
                    itinvestment = db.ITInvestmentPayment.Where(e => e.Id == itid).SingleOrDefault();
                }

                //newfilename = OFinYr + "_" + investmentid + "_" + subinvestmentid + "_" + id + ".jpg";
                //String FolderName = "FinancialYear" + fromdate + "-" + todate + "\\Investment\\" + newfilename;
                //string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(
                //System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\Images\\" + FolderName;
                if (SubId == "")
                {

                    if (itinvestment.Path != null)
                    {
                        localpath = itinvestment.Path;
                        FileInfo File = new FileInfo(localpath);
                        bool iExists = File.Exists;
                        if (iExists)
                        {
                            localpath = localpath;
                        }
                        else
                        {
                            localpath = ConfigurationManager.AppSettings["EmployeeDocuments"] + localpath;
                        }
                    }
                    else
                    {
                        return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    int subid = Convert.ToInt32(SubId);
                    itsubinvestment = db.ITSubInvestmentPayment.Where(e => e.Id == subid).SingleOrDefault();
                    if (itsubinvestment.Path != null)
                    {
                        localpath = itsubinvestment.Path;
                    }
                    else
                    {
                        return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                    }

                }
                FileInfo file = new FileInfo(localpath);
                bool exists = file.Exists;
                string extension = Path.GetExtension(file.Name);
                if (exists)
                {
                    return Json(new { success = true, fileextension = extension }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);

                }
            }
            return null;
        }

        public ActionResult GetCompImage(string id, string SubId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string localpath = "";
                ITInvestmentPayment itinvestment = null;
                ITSubInvestmentPayment itsubinvestment = null;
                if (id != null && id != "" && SubId == "")
                {
                    int itid = Convert.ToInt32(id);
                    itinvestment = db.ITInvestmentPayment.Where(e => e.Id == itid).SingleOrDefault();
                }
                if (SubId == "")
                {

                    if (itinvestment.Path != null)
                    {
                        localpath = itinvestment.Path;
                        if (localpath != null)
                        {
                            FileInfo File = new FileInfo(localpath);
                            bool iExists = File.Exists;
                            if (iExists)
                            {
                                localpath = localpath;
                            }
                            else
                            {
                                localpath = ConfigurationManager.AppSettings["EmployeeDocuments"] + localpath;
                            }
                        }

                    }
                    else
                    {
                        return View("File Not Found");
                        // return Json(new { success = false, responseText = "File Not Found..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    int subid = Convert.ToInt32(SubId);
                    itsubinvestment = db.ITSubInvestmentPayment.Where(e => e.Id == subid).SingleOrDefault();
                    if (itsubinvestment.Path != null)
                    {
                        localpath = itsubinvestment.Path;
                        if (localpath != null)
                        {
                            FileInfo File = new FileInfo(localpath);
                            bool iExists = File.Exists;
                            if (iExists)
                            {
                                localpath = localpath;
                            }
                            else
                            {
                                localpath = ConfigurationManager.AppSettings["EmployeeDocuments"] + localpath;
                            }
                        }
                    }
                    else
                    {
                        return View("File Not Found");
                        //return Content("File Not Found");
                        //return Json(new { success = false, responseText = "File Not Found..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                FileInfo file = new FileInfo(localpath);
                bool exists = file.Exists;
                string extension = Path.GetExtension(file.Name);

                if (exists)
                {
                    if (extension.ToUpper() == ".PDF")
                    {
                        return File(file.FullName, "application/pdf", file.Name + " ");
                        //string pdf="pdf";
                        //byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");

                        //string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        //return Json(new { data = base64ImageRepresentation, status = pdf }, JsonRequestBehavior.AllowGet);
                    }
                    if (extension.ToUpper() == ".JPG")
                    {
                        // return File(file.FullName, "image/png", file.Name + " ");
                        byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                        string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        return Json(new { data = base64ImageRepresentation, status = extension }, JsonRequestBehavior.AllowGet);
                    }
                    if (extension.ToUpper() == ".PNG")
                    {
                        //return File(file.FullName, "image/png", file.Name + " ");
                        byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                        string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        return Json(new { data = base64ImageRepresentation, status = extension }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Content("File Not Found");
                    //return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                }
                return null;
            }


        }
        //public ActionResult CheckSalaryHeadLink(string id, string SubId)
        //{
        //    ITInvestmentPayment itinvestment = null;
        //    ITSubInvestmentPayment itsubinvestment = null;
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        if (id != null && id != "")
        //        {
        //            int itid = Convert.ToInt32(id);
        //            itinvestment = db.ITInvestmentPayment.Include(e=>e.ITInvestment).Where(e => e.Id == itid).SingleOrDefault();

        //            if (itinvestment.ITInvestment!=null)
        //            {
        //                int Investmentid = itinvestment.ITInvestment.Id;

        //                var checksalaryheadlink = db.ITInvestment.Where(e => e.Id == Investmentid && e.SalaryHead != null).SingleOrDefault();

        //                if (checksalaryheadlink!=null)
        //                {
        //                return Json(new { status=true }, JsonRequestBehavior.AllowGet);

        //                }
        //                else
        //                {
        //                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //        }
        //    }
        //    return null;

        //}

        // [HttpPost]
        //public ActionResult Index(FormCollection fc, HttpPostedFileBase file)
        //{
        //    //  tbl_details tbl = new tbl_details();
        //    var allowedExtensions = new[] {  
        //        ".Jpg", ".png", ".jpg", "jpeg"  
        //    };
        //    //tbl.Id = fc["Id"].ToString();
        //    //tbl.Image_url = file.ToString(); //getting complete URL
        //    //tbl.Name = fc["Name"].ToString();
        //    var fileName = Path.GetFileName(file.FileName); //getting only file name(ex-ganesh.jpg)  
        //    var ext = Path.GetExtension(file.FileName); //getting the extension(ex-.jpg)  
        //    if (allowedExtensions.Contains(ext)) //check what type of extension  
        //    {
        //        string name = Path.GetFileNameWithoutExtension(fileName);
        //        //getting file name without extension  
        //        string myfile = name + "_" + ext; //appending the name with id  
        //        // store the file inside ~/project folder(Img)  
        //        var localPath = UploadFile();
        //        var path = Path.Combine(localPath, myfile);
        //        // tbl.Image_url = path;
        //        //obj.tbl_details.Add(tbl);
        //        //obj.SaveChanges();
        //        file.SaveAs(path);
        //    }
        //    else
        //    {
        //        ViewBag.message = "Please choose only Image file";
        //    }
        //    return View();




    }
}
