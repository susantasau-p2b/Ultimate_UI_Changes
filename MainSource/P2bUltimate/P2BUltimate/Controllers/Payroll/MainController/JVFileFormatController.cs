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
using System.Threading.Tasks;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class JVFileFormatController : Controller
    {
        //
        // GET: /JVFileFormat/
        //public ActionResult Index()
        //{
        //    return View();
        //}

        public ActionResult _JVFileFormatPartial()
        {
            return View("~/Views/Shared/Payroll/_JVFileFormat.cshtml");
        }

         public ActionResult Create(JVFileFormat c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            string FormatTypelist = form["FormatType_drop"] == "0" ? "" : form["FormatType_drop"];
            string Seperatorlist = form["Seperator_drop"] == "0" ? "" : form["Seperator_drop"];
            string CBSlist = form["CBS_drop"] == "0" ? "" : form["CBS_drop"];
            string Versionlist = form["Version_drop"] == "0" ? "" : form["Version_drop"];
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {

                        using (TransactionScope ts = new TransactionScope())
                        {
                            int CBSId = int.Parse(CBSlist);
                            int VersionId = int.Parse(Versionlist);
                            if (db.JVFileFormat.Any(o => o.CBS_Id == CBSId && o.Version_Id == VersionId))
                            {
                                Msg.Add("  Record Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            if (FormatTypelist != "" && FormatTypelist != null)
                            {
                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1075").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(FormatTypelist)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailTemplateFieldPlist));
                                c.FormatType = val;
                            }

                            if (Seperatorlist != "" && Seperatorlist != null)
                            {
                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1076").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Seperatorlist)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailTemplateFieldPlist));
                                c.Seperator = val;
                            }

                            if (CBSlist != "" && CBSlist != null)
                            {
                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1077").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(CBSlist)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailTemplateFieldPlist));
                                c.CBS = val;
                            }

                            if (Versionlist != "" && Versionlist != null)
                            {
                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1078").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Versionlist)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailTemplateFieldPlist));
                                c.Version = val;
                            }
                            JVFileFormat JVFileFormat = new JVFileFormat()
                            {
                                CBS = c.CBS,
                                FormatType = c.FormatType,
                                Seperator = c.Seperator,
                                Version = c.Version,
                                DBTrack = c.DBTrack
                            };
                            try
                            {

                                db.JVFileFormat.Add(JVFileFormat);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, c.DBTrack);
                                DT_JVFileFormat DT_DeptObj = (DT_JVFileFormat)rtn_Obj;
                                db.Create(DT_DeptObj);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = JVFileFormat.Id, Val = JVFileFormat.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Utility.JsonReturnClass { Id = JVFileFormat.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { LocationObj.Id, null, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            }
                            catch (DataException)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        //return this.Json(new { msg = errorMsg });
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

             using (DataBaseContext db = new DataBaseContext())
             {
                 var Q = db.JVFileFormat
                     .Where(e => e.Id == data).Select
                     (e => new
                     {
                         CBS_Id = e.CBS_Id,
                         FormatType_Id = e.FormatType_Id,
                         Seperator_Id = e.Seperator_Id,
                         Version_Id = e.Version_Id,
                         Action = e.DBTrack.Action
                     }).ToList();


                 var Corp = db.JVFileFormat.Find(data);
                 TempData["RowVersion"] = Corp.RowVersion;
                 var Auth = Corp.DBTrack.IsModified;
                 return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
             }
         }

         public async Task<ActionResult> EditSave(JVFileFormat ESOBJ, int data, FormCollection form)
         {
             List<string> Msg = new List<string>();

             string FormatType = form["FormatType_drop"] == "0" ? "" : form["FormatType_drop"];
             string Seperator = form["Seperator_drop"] == "0" ? "" : form["Seperator_drop"];
             string CBS = form["CBS_drop"] == "0" ? "" : form["CBS_drop"];
             string Version = form["Version_drop"] == "0" ? "" : form["Version_drop"];

             ESOBJ.CBS_Id = CBS != null && CBS != "" ? int.Parse(CBS) : 0;
             ESOBJ.FormatType_Id = FormatType != null && FormatType != "" ? int.Parse(FormatType) : 0;
             ESOBJ.Seperator_Id = Seperator != null && Seperator != "" ? int.Parse(Seperator) : 0;
             ESOBJ.Version_Id = Version != null && Version != "" ? int.Parse(Version) : 0;




             using (DataBaseContext db = new DataBaseContext())
             {
                 try
                 {
                     JVFileFormat JVFileForm = db.JVFileFormat.Find(data);
                     using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                     {

                         
                         TempData["CurrRowVersion"] = JVFileForm.RowVersion;
                         if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                         {
                             ESOBJ.DBTrack = new DBTrack
                             {
                                 CreatedBy = JVFileForm.DBTrack.CreatedBy == null ? null : JVFileForm.DBTrack.CreatedBy,
                                 CreatedOn = JVFileForm.DBTrack.CreatedOn == null ? null : JVFileForm.DBTrack.CreatedOn,
                                 Action = "M",
                                 ModifiedBy = SessionManager.UserName,
                                 ModifiedOn = DateTime.Now
                             };
                             JVFileForm.CBS_Id = ESOBJ.CBS_Id;
                             JVFileForm.FormatType_Id = ESOBJ.FormatType_Id;
                             JVFileForm.Seperator_Id = ESOBJ.Seperator_Id;
                             JVFileForm.Version_Id = ESOBJ.Version_Id;
                             JVFileForm.DBTrack = ESOBJ.DBTrack;

                             db.Entry(JVFileForm).State = System.Data.Entity.EntityState.Modified;
                             db.SaveChanges();
                         }
                         //}
                         ts.Complete();
                     }
                     Msg.Add("  Record Updated");
                     JVFileFormat ONewFormat = db.JVFileFormat.Include(e => e.CBS).Include(e => e.FormatType).Include(e => e.Version).FirstOrDefault();
                     return Json(new Utility.JsonReturnClass { Id = ONewFormat.Id, Val = ONewFormat.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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