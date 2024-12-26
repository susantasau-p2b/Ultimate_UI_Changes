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
    public class ChargeSheetServingStatusController : Controller
    {
        //
        // GET: /ChargeSheetServingStatus/
        public ActionResult Index()
        {
            return View("~/Views/IR/MainViews/ChargeSheetServingStatus/Index.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/IR/chargesheetservingmode.cshtml");
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
        public ActionResult Create(ChargeSheetServingStatus c, FormCollection form)
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
                    string Narration = form["Narration"] == "0" ? "" : form["Narration"];
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            ChargeSheetServingStatus ChargeSheetServingStatus = new ChargeSheetServingStatus()
                            {
                                Narration = c.Narration,
                                IsChargeSheetRead=c.IsChargeSheetRead,
                                IsCloseServingChargeSheet=c.IsCloseServingChargeSheet,
                                DBTrack = c.DBTrack
                            };


                            var ChargeSheetServingStatusValidation = ValidateObj(ChargeSheetServingStatus);
                            if (ChargeSheetServingStatusValidation.Count > 0)
                            {
                                foreach (var item in ChargeSheetServingStatusValidation)
                                {

                                    Msg.Add("ChargeSheetServingStatus" + item);
                                }
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            db.ChargeSheetServingStatus.Add(ChargeSheetServingStatus);
                           
                            try
                            {
                                db.SaveChanges();
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
                   var ChargeSheetServingStatus = new List<ChargeSheetServingStatus>();
                   ChargeSheetServingStatus = db.ChargeSheetServingStatus.ToList();
                   IEnumerable<ChargeSheetServingStatus> IE;

                   if (!string.IsNullOrEmpty(gp.searchField))
                   {
                       IE = ChargeSheetServingStatus;
                       if (gp.searchOper.Equals("eq"))
                       {
                           jsonData = IE
                            .Where((e => (e.IsChargeSheetRead.ToString().ToUpper().Contains(gp.searchString))
                                         || (e.IsCloseServingChargeSheet.ToString().ToUpper().Contains(gp.searchString))
                                         || (e.Narration.ToString().ToUpper().Contains(gp.searchString))
                                         || (e.Id.ToString().Contains(gp.searchString))
                                    )).ToList()
                           .Select(a => new Object[] { a.IsChargeSheetRead, a.IsCloseServingChargeSheet,  a.Narration,  a.Id });
                       }
                       if (pageIndex > 1)
                       {
                           int h = pageIndex * pageSize;
                           jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.IsChargeSheetRead, a.IsCloseServingChargeSheet,  a.Narration,  a.Id }).ToList();
                       }
                       totalRecords = IE.Count();


                   }
                   else
                   {
                       IE = ChargeSheetServingStatus;
                       Func<ChargeSheetServingStatus, dynamic> orderfuc;
                       if (gp.sidx == "Id")
                       {
                           orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                       }
                       else
                       {
                           orderfuc = (c => gp.sidx == "IsChargeSheetRead" ? c.IsChargeSheetRead.ToString() :
                                           gp.sidx == "IsCloseServingChargeSheet" ? c.IsCloseServingChargeSheet.ToString() :
                                            gp.sidx == "Narration" ? c.Narration.ToString() : "");

                       }
                       if (gp.sord == "asc")
                       {
                           IE = IE.OrderBy(orderfuc);
                           jsonData = IE.Select(a => new Object[] { a.IsChargeSheetRead, a.IsCloseServingChargeSheet,  a.Narration,  a.Id }).ToList();
                       }
                       else if (gp.sord == "desc")
                       {
                           IE = IE.OrderByDescending(orderfuc);
                           jsonData = IE.Select(a => new Object[] { a.IsChargeSheetRead, a.IsCloseServingChargeSheet,  a.Narration, a.Id }).ToList();
                       }
                       if (pageIndex > 1)
                       {
                           int h = pageIndex * pageSize;
                           jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.IsChargeSheetRead, a.IsCloseServingChargeSheet,  a.Narration,  a.Id }).ToList();
                       }
                       totalRecords = ChargeSheetServingStatus.Count();
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
                       ChargeSheetServingStatus corporates = db.ChargeSheetServingStatus
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
         [HttpPost]
           public ActionResult Edit(int data)
           {
               using (DataBaseContext db = new DataBaseContext())
               {
                   var returndata = db.ChargeSheetServingStatus
                                    .Where(e => e.Id == data).AsEnumerable()
                       .Select(e => new
                       {
                           IsChargeSheetRead = e.IsChargeSheetRead,
                           IsCloseServingChargeSheet = e.IsCloseServingChargeSheet,
                            Narration = e.Narration,
                       }).ToList();
                   return Json(new Object[] { returndata, "", "", "", JsonRequestBehavior.AllowGet });
               }
           }
           [HttpPost]
           public ActionResult EditSave(ChargeSheetServingStatus c, int data, FormCollection form)
           {
               using (DataBaseContext db = new DataBaseContext())
               {
                   List<string> Msg = new List<string>();
                   try
                   {

                       var db_data = db.ChargeSheetServingStatus
                           .Where(a => a.Id == data).SingleOrDefault();
                      
                       c.DBTrack = new DBTrack
                       {
                           CreatedBy = db_data.DBTrack.CreatedBy == null ? null : db_data.DBTrack.CreatedBy,
                           CreatedOn = db_data.DBTrack.CreatedOn == null ? null : db_data.DBTrack.CreatedOn,
                           Action = "M",
                           ModifiedBy = SessionManager.UserName,
                           ModifiedOn = DateTime.Now
                       };

                       db_data.IsChargeSheetRead = c.IsChargeSheetRead;
                       db_data.IsCloseServingChargeSheet = c.IsCloseServingChargeSheet;
                       db_data.Narration = c.Narration;
                       db_data.DBTrack = c.DBTrack;

                       using (TransactionScope ts = new TransactionScope())
                       {
                           db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                           db.SaveChanges();
                           db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                           ts.Complete();
                       }
                       Msg.Add("  Data Saved successfully  ");
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