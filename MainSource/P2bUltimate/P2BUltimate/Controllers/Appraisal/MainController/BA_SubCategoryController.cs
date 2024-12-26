using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using Appraisal;

namespace P2BUltimate.Controllers.BusinessApprisalCategory.MainController
{
    public class BA_SubCategoryController : Controller
    {
        //
        // GET: /BA_SubCategory/
       
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(BA_SubCategory c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            BA_SubCategory BA_SubCategory = new BA_SubCategory()
                            {
                                Code = c.Code,
                                Name = c.Name,
                                DBTrack = c.DBTrack
                            };

                            db.BA_SubCategory.Add(BA_SubCategory);

                            try
                            {

                                db.SaveChanges();

                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = BA_SubCategory.Id, Val = BA_SubCategory.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            catch (DataException)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.BA_SubCategory
                                  .Where(e => e.Id == data).AsEnumerable()
                    .Select(e => new
                    {
                        Code = e.Code,
                        Name = e.Name,
                    }).ToList();

                return Json(new Object[] { returndata, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(BA_SubCategory c, int data, FormCollection form)
        {

            List<string> Msg = new List<string>(); 
          
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.BA_SubCategory.Where(e => e.Id == data).SingleOrDefault();                        
                        TempData["RowVersion"] = db_data.RowVersion;
                        BA_SubCategory ba_subcategory = db.BA_SubCategory.Find(data);
                        TempData["CurrRowVersion"] = ba_subcategory.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = ba_subcategory.DBTrack.CreatedBy == null ? null : ba_subcategory.DBTrack.CreatedBy,
                                CreatedOn = ba_subcategory.DBTrack.CreatedOn == null ? null : ba_subcategory.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            ba_subcategory.Id = data;
                            ba_subcategory.Code = c.Code;
                            ba_subcategory.Name = c.Name;
                            ba_subcategory.DBTrack = c.DBTrack;
                            db.Entry(ba_subcategory).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { Id = data, Val =c.FullDetails , success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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