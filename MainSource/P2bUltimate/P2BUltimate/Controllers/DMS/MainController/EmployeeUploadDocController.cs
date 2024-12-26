using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using P2BUltimate.Models;
using System.IO;
using P2b.Global;
using System.Text;
using System.Transactions;
using P2BUltimate.Security;
using System.Data.Entity.Infrastructure;
using System.Data;
using System.Configuration;

namespace P2BUltimate.Controllers.DMS.MainController
{
    public class EmployeeUploadDocController : Controller
    {
        //
        // GET: /EmployeeUploadDoc/
        public ActionResult Index()
        {
            return View("~/Views/DMS/MainViews/EmployeeUploadDoc/Index.cshtml");
        }

        public ActionResult DisplaySearchResults(string searchText)
        {
            var model = "";
            return PartialView("SearchResults", model);
        }

        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (Object)null;
                SelectList s = (SelectList)null;

                if (data != "" && data != null && data != "0")
                {
                    var filter = Convert.ToInt32(data);
                    var qurey = db.LookupValue.Where(e => e.Id == filter).FirstOrDefault().LookupVal;
                    var qurey1 = db.Lookup.Where(e => e.Code == "791").Include(e => e.LookupValues).FirstOrDefault().LookupValues.Where(t => t.LookupValData.ToUpper() == qurey.ToUpper());
                    if (data2 != "" && data2 != null && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);
                    }
                    s = new SelectList(qurey1, "Id", "LookupVal", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var query = db.AppSubCategory.ToList();
                    if (data2 != "" && data2 != null && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);
                    }
                    s = new SelectList(query, "Id", "FullDetails", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public ActionResult FileUpload(HttpPostedFileBase[] files, FormCollection form, string data, string Id, string SubId)
        {
            if (ModelState.IsValid)
            {
                List<string> Msg = new List<string>();
                string FuncModulelist = form["FuncModulelist"] == "0" ? "" : form["FuncModulelist"];
                string DocTypelist = form["DocTypelist"] == "0" ? "" : form["DocTypelist"];
                string SubDocTypelist = form["SubDocTypelist"] == "0" ? "" : form["SubDocTypelist"];
                string Emp = form["employee-table"] == "0" ? "" : form["employee-table"];
                int EmpId = 0;
                if (Emp != null && Emp != "0" && Emp != "false")
                {
                    EmpId = int.Parse(Emp);
                }
                else
                {
                    Msg.Add("  Please Select Employee  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { "", "", "Please Select Employee" }, JsonRequestBehavior.AllowGet);
                }

                
                Session["FilePath"] = null;
                string extension, newfilename, deletefilepath = "";
                Int32 Count = 0;

                string EmpCode = "", FuncModule = "", DocType = "", SubDoc = "";

                var allowedExtensions = new[] { ".Jpg", ".png", ".jpg", ".jpeg", ".pdf" };
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
                using (DataBaseContext db = new DataBaseContext())
                {
                    EmpCode = db.Employee.Find(EmpId).EmpCode;
                    FuncModule = db.LookupValue.Find(int.Parse(FuncModulelist)).LookupVal.ToUpper();
                    DocType = db.LookupValue.Find(int.Parse(DocTypelist)).LookupVal.ToUpper();
                    SubDoc = db.LookupValue.Find(int.Parse(SubDocTypelist)).LookupVal.ToUpper();
                }
                
                foreach (HttpPostedFileBase file in files)
                {

                    if (file != null)
                    {

                        extension = Path.GetExtension(file.FileName);
                        //newfilename = Id + extension;
                        // String FolderName = "LeaveCertificate" + "\\" + empcode + "\\";
                        newfilename = EmpCode + "-" + DocType + "-" + SubDoc.Replace("/", "") + extension;
                        String FolderName = "EmpCode" + EmpCode + "\\" + FuncModule + "\\" + DocType + "\\" + SubDoc + "\\";

                        var InputFileName = Path.GetFileName(file.FileName);
                        string ServerSavePath = ConfigurationManager.AppSettings["DocumentPath"]; //InvestmentUploadFile(FolderName);
                        //string ServerSavePath = InvestmentUploadFile(FolderName);
                        //  string ServerMappath = ServerSavePath + newfilename;

                        string ServerMappath = ServerSavePath + FolderName + newfilename;
                        if (deletefilepath != null && deletefilepath != "")
                        {
                            FileInfo File = new FileInfo(deletefilepath);
                            bool exists = File.Exists;
                            if (exists)
                            {
                                System.IO.File.Delete(deletefilepath);
                            }
                        }
                        bool existschkE = System.IO.Directory.Exists(ServerSavePath + FolderName);

                        if (!existschkE)
                        {
                            System.IO.Directory.CreateDirectory(ServerSavePath + FolderName);
                        }
                        file.SaveAs(Path.Combine(ServerMappath));
                        Session["FilePath"] = ServerMappath;

                        
                    }
                }
               // this.PDFViewer.AnnotationImageSource = "~/Viewer/Upload/" + Session("UserName") + "/Images/" + Session("FName");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Create(EmployeeDMS_Doc EmpDoc, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string FuncModulelist = form["FuncModulelist"] == "0" ? "" : form["FuncModulelist"];
                    string DocTypelist = form["DocTypelist"] == "0" ? "" : form["DocTypelist"];
                    string SubDocTypelist = form["SubDocTypelist"] == "0" ? "" : form["SubDocTypelist"];
                    string Employee = form["employee-table"] == "0" ? "" : form["employee-table"];
                    List<EmployeeDMS_SubDoc> SubDocval = new List<EmployeeDMS_SubDoc>();

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            DBTrack DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            int FuncModule_Id = Convert.ToInt32(FuncModulelist);
                            int DocType_Id = Convert.ToInt32(DocTypelist);
                            int SubDocType_Id =   Convert.ToInt32(SubDocTypelist);

                           
                            var EmpDMSDoc = db.EmployeeDMS_Doc.Where(e => e.DocumentType_Id == DocType_Id && e.FunctionalPackage_Id == FuncModule_Id).FirstOrDefault();
                            if (EmpDMSDoc != null)
                            {
                                var EmpDMS_SubDoc = db.EmployeeDMS_SubDoc.Where(e => e.EmployeeDMS_Doc_Id == EmpDMSDoc.Id).FirstOrDefault();

                                FileInfo File = new FileInfo(Session["FilePath"].ToString());

                                EmployeeDMS_SubDoc EmpDMSSubDoc = new EmployeeDMS_SubDoc()
                                {
                                    DocumentPath = Session["FilePath"].ToString(),
                                    FileName = File.Name,
                                    SubDocumentDesc = EmpDMS_SubDoc.SubDocumentDesc,
                                    SubDocName = EmpDMS_SubDoc.SubDocName,
                                    DBTrack = EmpDoc.DBTrack,
                                    
                                };
                                SubDocval.Add(EmpDMSSubDoc);

                                EmployeeDMS_Doc EmpDMS_Doc = new EmployeeDMS_Doc()
                                {
                                    DocumentType = db.LookupValue.Find(DocType_Id),
                                    DocumentTypeDesc = EmpDMSDoc.DocumentTypeDesc,
                                    EmployeeDMS_SubDoc = SubDocval,
                                    FunctionalPackage = db.LookupValue.Find(FuncModule_Id),
                                    Document_Size_Appl = EmpDMSDoc.Document_Size_Appl,
                                    DBTrack = EmpDoc.DBTrack,
                                    Employee = db.Employee.Find(int.Parse(Employee))
                                };
                                try
                                {
                                    db.EmployeeDMS_Doc.Add(EmpDMS_Doc);
                                    db.SaveChanges();
                                    ts.Complete();
                                    Msg.Add("  Data Saved successfully  ");
                                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                }
                                catch (DbUpdateConcurrencyException)
                                {
                                    return RedirectToAction("Create", new { concurrencyError = true, id = EmpDoc.Id });
                                }
                                catch (DataException /* dex */)
                                {
                                    Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                                }
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
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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
            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
	}
}