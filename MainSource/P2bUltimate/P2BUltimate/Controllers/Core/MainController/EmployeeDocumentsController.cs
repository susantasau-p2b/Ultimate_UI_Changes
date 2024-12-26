using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Text;
using P2BUltimate.Models;
using System.Threading.Tasks;
using P2BUltimate.Security;
using P2b.Global;
using Recruitment;
using System.IO;
using System.Reflection;
using System.Configuration;

namespace P2BUltimate.Controllers.Recruitement.MainController
{

    public class EmployeeDocumentsController : Controller
    {
        List<String> Msg = new List<String>();

        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /EmployeeDocuments/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Core/_EmployeeDocuments.cshtml");
        }

        [HttpPost]
        public ActionResult Create(HttpPostedFileBase[] files, EmployeeDocuments CanDoc, FormCollection form, string EmpCode)
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

                        string DocType = form["DocTypeList"] == "0" ? "" : form["DocTypeList"];
                        string SubDocName = form["SubDocList"] == "0" ? "" : form["SubDocList"];

                        string txtUploadDate = form["txtUploadDate"] == "0" ? "" : form["txtUploadDate"];
                        string txtApprovedDate = form["txtApprovedDate"] == "0" ? "" : form["txtApprovedDate"];

                        string LookupSubDocName = "";
                        string LookupDocName = "";
                        //string ServerSavePath = "";
                        string NewPath = "";
                        string deletefilepath = "";


                        if (DocType != "" && DocType != null)
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1070").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(DocType)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(DocType));
                            CanDoc.DocumentType = val;
                            LookupDocName = CanDoc.DocumentType.LookupVal;
                        }

                        if (SubDocName != "" && SubDocName != null)
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1071").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(SubDocName)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(SubDocName));
                            CanDoc.SubDocumentName = val;
                            LookupSubDocName = CanDoc.SubDocumentName.LookupVal;
                        }



                        if (ModelState.IsValid)
                        {

                            ///fileupload
                            ///
                            string empcode = EmpCode;
                            var Emp_Id = db.Employee.Where(e => e.EmpCode == EmpCode).SingleOrDefault().Id;
                            string EmpId = Convert.ToString(Emp_Id);
                            var Module_Name= HttpContext.Session["ModuleType"];
                            string ModuleName = Module_Name.ToString();

                            var allowedExtensions = new[] { ".Jpg", ".png", ".jpg", ".jpeg", ".pdf", ".JPG", ".PNG",".JPEG",".PDF"};
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

                                    //String FolderName = "EmpCode" + empcode + "\\";

                                    //var InputFileName = Path.GetFileName(file.FileName);
                                    //ServerSavePath = InvestmentUploadFile(FolderName);
                                    //string ServerMappath = ServerSavePath + newfilename;

                                    String FolderName = EmpId + "\\" + ModuleName + "\\" + LookupDocName + "\\";

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
                                    NewPath = FolderName + newfilename;
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
                                ApprovedDate = Convert.ToDateTime(txtApprovedDate),
                                IsApproved = CanDoc.IsApproved,
                                SubDocumentName = CanDoc.SubDocumentName,
                                UploadDate = Convert.ToDateTime(txtUploadDate),
                                DBTrack = CanDoc.DBTrack
                            };


                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.EmployeeDocuments.Add(CandidateDocs);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, CanDoc.DBTrack);
                                //DT_EmployeeDocuments DT_CanDocs = (DT_EmployeeDocuments)rtn_Obj;
                                //DT_CanDocs.DocumentType_Id = CanDoc.DocumentType == null ? 0 : CanDoc.DocumentType.Id;
                                //DT_CanDocs.SubDocumentName_Id = CanDoc.SubDocumentName == null ? 0 : CanDoc.SubDocumentName.Id;
                                //db.Create(DT_CanDocs);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("Data Saved successfully");
                                return Json(new Utility.JsonReturnClass { Id = CandidateDocs.Id, Val = CandidateDocs.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { add.Id, add.FullAddress,  }, JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        public ActionResult Edit(int data)
        {
            //var Q = db.RecruitJoinParaProcessResult
            //     .Include(e => e.RecruitJoiningPara)
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.EmployeeDocuments
                    .Include(e => e.DocumentType)
                    .Include(e => e.SubDocumentName)

            .Where(e => e.Id == data).Select
            (e => new
            {
                DocTypeList = e.DocumentType.Id,
                SubDocList = e.SubDocumentName.Id,
                txtUploadDate = e.UploadDate,
                txtDocPath = e.DocumentPath,
                IsApproved = e.IsApproved,
                txtApprovedDate = e.ApprovedDate,
                Action = e.DBTrack.Action
            }).ToList();




                var Corp = db.EmployeeDocuments.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        public async Task<ActionResult> EditSave(HttpPostedFileBase[] files, EmployeeDocuments c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                { // string ContactDetails = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    //string DocumentType = form["DocTypeList"] == null ? "" : form["DocTypeList"];
                    //string SubDocumentName = form["SubDocList"] == null ? "" : form["SubDocList"];
                    var DocumentType = form["DocTypeList"] == "0" ? "" : form["DocTypeList"];
                    var SubDocumentName = form["SubDocList"] == "0" ? "" : form["SubDocList"];

                    string extension, newfilename = "";
                    Int32 Count = 0;

                    string LookupSubDocName = "";
                    //string ServerSavePath = "";
                    string NewPath = "";
                    string deletefilepath = "";

                    var db_Data = db.EmployeeDocuments
                        .Include(e => e.DocumentType)
                        .Include(e => e.SubDocumentName)
                        .Where(e => e.Id == data).SingleOrDefault();
 
                    if (DocumentType != null && DocumentType != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1070").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(DocumentType)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(DocumentType));
                        c.DocumentType = val;
                    }

                    if (SubDocumentName != null && SubDocumentName != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1071").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(SubDocumentName)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(SubDocumentName));
                        c.SubDocumentName = val;
                    }
                    string EmpCode = "";

                    List<Employee> OEmpList = db.Employee.Include(e => e.EmployeeDocuments).Where(e => e.EmployeeDocuments.Count > 0).ToList();

                    foreach (var a in OEmpList)
                    {
                        if (a.EmployeeDocuments.Any(r => r.Id == data))
                        {
                            EmpCode = a.EmpCode;
                            break;
                        }
                    }

             

                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            

                            var allowedExtensions = new[] { ".Jpg", ".png", ".jpg", ".jpeg", ".pdf", ".JPG", ".PNG", ".JPEG", ".PDF" };
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

                                    String FolderName = "EmpCode" + EmpCode+"\\";

                                    //var InputFileName = Path.GetFileName(file.FileName);
                                    //ServerSavePath = InvestmentUploadFile(FolderName);
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

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                db.EmployeeDocuments.Attach(db_Data);
                                db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = db_Data.RowVersion;
                                db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

                                var Curr_Lookup = db.EmployeeDocuments.Find(data);
                                TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {

                                    EmployeeDocuments blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.EmployeeDocuments.Where(e => e.Id == data)
                                     .Include(e => e.DocumentType)
                                      .Include(e => e.SubDocumentName).SingleOrDefault();
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
                                    EmployeeDocuments lk = new EmployeeDocuments
                                    {


                                        DocumentPath = c.DocumentPath,
                                        DocumentType = c.DocumentType,
                                        ApprovedDate = c.ApprovedDate,
                                        IsApproved = c.IsApproved,
                                        SubDocumentName = c.SubDocumentName,
                                        UploadDate = c.UploadDate,
                                        DBTrack = c.DBTrack


                                    };


                                    db.EmployeeDocuments.Attach(lk);
                                    db.Entry(lk).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

                                    // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


                                    using (var context = new DataBaseContext())
                                    {

                                        //var obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        //DT_EmployeeDocuments DT_Corp = (DT_EmployeeDocuments)obj;

                                        //db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = lk.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }



                            List<string> MsgB = new List<string>();
                            MsgB.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            EmployeeDocuments blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            EmployeeDocuments Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.EmployeeDocuments.Where(e => e.Id == data).SingleOrDefault();
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

                            if (TempData["RowVersion"] == null)
                            {
                                TempData["RowVersion"] = blog.RowVersion;
                            }

                            EmployeeDocuments corp = new EmployeeDocuments()
                            {
                                DocumentPath = c.DocumentPath,
                                DocumentType = c.DocumentType,
                                ApprovedDate = c.ApprovedDate,
                                IsApproved = c.IsApproved,
                                SubDocumentName = c.SubDocumentName,
                                UploadDate = c.UploadDate,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                // var obj = DBTrackFile.ModifiedDataHistory("Recruitment/Recruitment", "M", blog, corp, "EmployeeDocuments", c.DBTrack);
                                // // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                // Old_Corp = context.EmployeeDocuments.Where(e => e.Id == data)
                                //     .Include(e => e.DocumentType)
                                //       .Include(e => e.SubDocumentName)
                                //     .SingleOrDefault();
                                //// DT_EmployeeDocuments DT_Corp = (DT_EmployeeDocuments)obj;
                                // DT_Corp.DocumentType_Id = c.DocumentType == null ? 0 : c.DocumentType.Id;
                                // DT_Corp.SubDocumentName_Id = c.SubDocumentName == null ? 0 : c.SubDocumentName.Id;
                                // db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.EmployeeDocuments.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("Record Updated Successfully.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var clientValues = (EmployeeDocuments)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (EmployeeDocuments)databaseEntry.ToObject();
                        c.RowVersion = databaseValues.RowVersion;

                    }
                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                return View();

            }


        }

        public ActionResult GetCandDocsLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.EmployeeDocuments.Include(e => e.DocumentType).Include(e => e.SubDocumentName).ToList();
                IEnumerable<EmployeeDocuments> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.EmployeeDocuments.ToList().Where(d => d.FullDetails.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue ="DocType: " + ca.DocumentType.LookupVal.ToString() + ",SubDocName : " + ca.SubDocumentName.LookupVal.ToString()  }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }


        public string InvestmentUploadFile(string FolderName)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\EmployeeDocuments\\" + FolderName + "\\";
            String localPath = "";
            bool exists = System.IO.Directory.Exists(requiredPath);
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            return localPath;
        }


        public ActionResult GetCompImage(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string localpath = "";

                EmployeeDocuments employeedocuments = null;

                int subid = Convert.ToInt32(data);

                employeedocuments = db.EmployeeDocuments.Where(e => e.Id == subid).SingleOrDefault();
                if (employeedocuments.DocumentPath != null)
                {
                    localpath = employeedocuments.DocumentPath;
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
                    return View("File Not Found");
                    //return Content("File Not Found");
                    //return Json(new { success = false, responseText = "File Not Found..!" }, JsonRequestBehavior.AllowGet);
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
                        //return Json(new { data = base64ImageRepresentation, status = extension }, JsonRequestBehavior.AllowGet);

                        JsonRequestBehavior behaviou = new JsonRequestBehavior();
                        return new JsonResult()
                        {
                            Data = base64ImageRepresentation,
                            MaxJsonLength = 86753090,
                            JsonRequestBehavior = behaviou
                        };
                       
                    }
                    if (extension.ToUpper() == ".PNG")
                    {
                        //return File(file.FullName, "image/png", file.Name + " ");
                        byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                        string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        //return Json(new { data = base64ImageRepresentation, status = extension }, JsonRequestBehavior.AllowGet);

                        JsonRequestBehavior behaviou = new JsonRequestBehavior();
                        return new JsonResult()
                        {
                            Data = base64ImageRepresentation,
                            MaxJsonLength = 86753090,
                            JsonRequestBehavior = behaviou
                        };
                        
                    }
                    if (extension.ToUpper() == ".JPEG")
                    {
                        //return File(file.FullName, "image/png", file.Name + " ");
                        byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                        string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        //return Json(new { data = base64ImageRepresentation, status = extension }, JsonRequestBehavior.AllowGet);

                        JsonRequestBehavior behaviou = new JsonRequestBehavior();
                        return new JsonResult()
                        {
                            Data = base64ImageRepresentation,
                            MaxJsonLength = 86753090,
                            JsonRequestBehavior = behaviou
                        };

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

        public ActionResult CheckUploadFile(string id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string localpath = "";
                EmployeeDocuments employeedocuments = null;

                int subid = Convert.ToInt32(id);
                employeedocuments = db.EmployeeDocuments.Where(e => e.Id == subid).SingleOrDefault();
                if (employeedocuments.DocumentPath != null)
                {
                    localpath = employeedocuments.DocumentPath;
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
                
                FileInfo file = new FileInfo(localpath);
                bool exists = file.Exists;
                string extension = Path.GetExtension(file.Name);
                if (exists)
                {
                    return Json(new { success = true, fileextension = extension }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, responseText = "File Not Uploaded..!" }, JsonRequestBehavior.AllowGet);

                }
            }
            return null;
        }


    }
}