using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Transactions;
using P2b.Global;
using System.Text;
using P2BUltimate.App_Start;
using System.Threading.Tasks;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.DMS.MainController
{
    public class DMS_BulletinController : Controller
    {
        //
        // GET: /DMS_Bulletin/
        public ActionResult Index()
        {
            return View("~/Views/DMS/MainViews/DMS_Bulletin/Index.cshtml");
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            List<string> Msg = new List<string>();

            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<DMS_Bulletin> DMS_Bulletin = null;
                if (gp.IsAutho == true)
                {
                    DMS_Bulletin = db.DMS_Bulletin.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    DMS_Bulletin = db.DMS_Bulletin.AsNoTracking().ToList();
                }

                IEnumerable<DMS_Bulletin> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = DMS_Bulletin;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.PublishDate != null ? e.PublishDate.Value.ToString().Contains(gp.searchString.ToUpper()) : false)
                               || (e.ExpiryDate != null ? e.ExpiryDate.Value.ToString().Contains(gp.searchString.ToUpper()) : false)
                               || (e.Title != null ? e.Title.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.PublishDate != null ? a.PublishDate.Value.ToShortDateString() : "", a.ExpiryDate != null ? a.ExpiryDate.Value.ToShortDateString() : "", a.Title, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.PublishDate != null ? a.PublishDate.Value.ToShortDateString() : "", a.ExpiryDate != null ? a.ExpiryDate.Value.ToShortDateString() : "", a.Title, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = DMS_Bulletin;
                    Func<DMS_Bulletin, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>
                                         gp.sidx == "PublishDate" ? c.PublishDate.Value.ToShortDateString() :
                                         gp.sidx == "ExpiryDate" ? c.ExpiryDate.Value.ToShortDateString() :
                                         gp.sidx == "Title" ? c.Title : "");

                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.PublishDate != null ? a.PublishDate.Value.ToShortDateString() : "", a.ExpiryDate != null ? a.ExpiryDate.Value.ToShortDateString() : "", a.Title, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.PublishDate != null ? a.PublishDate.Value.ToShortDateString() : "", a.ExpiryDate != null ? a.ExpiryDate.Value.ToShortDateString() : "", a.Title, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.PublishDate != null ? a.PublishDate.Value.ToShortDateString() : "", a.ExpiryDate != null ? a.ExpiryDate.Value.ToShortDateString() : "", a.Title, a.Id }).ToList();
                    }
                    totalRecords = DMS_Bulletin.Count();
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
        // [ValidateAntiForgeryToken]
        public ActionResult Create(DMS_Bulletin B, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            { 
                List<String> Msg = new List<String>();
                try
                {
                     

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            B.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            DMS_Bulletin DMS_Bulletin = new DMS_Bulletin()
                            {
                                PublishDate = B.PublishDate,
                                Attachment = B.Attachment,
                                ExpiryDate = B.ExpiryDate,
                                Icon = B.Icon,
                                MessageContent = B.MessageContent,
                                Title = B.Title,
                                DBTrack = B.DBTrack
                            };

                            db.DMS_Bulletin.Add(DMS_Bulletin);
                            db.SaveChanges();
                            
                            ts.Complete();
                            Msg.Add("Data Saved Successfully.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                        Msg.Add("Code Already Exists.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                     
                    }
                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }

        }

        [HttpPost]
        public ActionResult Edit(int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.DMS_Bulletin 
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        PublishDate = e.PublishDate.Value,
                        ExpiryDate = e.ExpiryDate.Value,
                        Attachment = e.Attachment,
                        Icon = e.Icon,
                        MessageContent = e.MessageContent.Replace("<br />", System.Environment.NewLine),
                        Title = e.Title, 
                        Action = e.DBTrack.Action
                    }).ToList();


                var Corp = db.DMS_Bulletin.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(DMS_Bulletin c, int data, FormCollection form) // Edit submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                  
                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {


                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                DMS_Bulletin blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.DMS_Bulletin.Where(e => e.Id == data).FirstOrDefault();
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

 

                                var myString = c.MessageContent.Replace(System.Environment.NewLine, "<br />"); //add a line terminating ;




                                DMS_Bulletin bulletin = new DMS_Bulletin()
                                {
                                    Attachment = c.Attachment,
                                    ExpiryDate = c.ExpiryDate,
                                    Icon = c.Icon,
                                    MessageContent = myString,
                                    PublishDate = c.PublishDate,
                                    Title = c.Title, 
                                    DBTrack = c.DBTrack,
                                    Id = data
                                };


                                db.DMS_Bulletin.Attach(bulletin);
                                db.Entry(bulletin).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(bulletin).OriginalValues["RowVersion"] = TempData["RowVersion"];
                              
                                db.SaveChanges();
                                await db.SaveChangesAsync();
                                ts.Complete();

                                Msg.Add("Record Updated Successfully.");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

                            DMS_Bulletin blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            //Email Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.DMS_Bulletin.Where(e => e.Id == data).FirstOrDefault();
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
                            var myString = c.MessageContent.Replace(System.Environment.NewLine, "<br />"); //add a line terminating ;

                            DMS_Bulletin bulletin = new DMS_Bulletin()
                            {
                                Attachment = c.Attachment,
                                ExpiryDate = c.ExpiryDate,
                                Icon = c.Icon,
                                MessageContent = myString,
                                PublishDate = c.PublishDate,
                                Title = c.Title,
                                DBTrack = c.DBTrack,
                                Id = data
                            };

                            blog.DBTrack = c.DBTrack;
                            db.DMS_Bulletin.Attach(blog);
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
                    var clientValues = (Corporate)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (Corporate)databaseEntry.ToObject();
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

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    DMS_Bulletin DMS_Bulletin = db.DMS_Bulletin.Where(e => e.Id == data).FirstOrDefault();

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                       
                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now,
                            CreatedBy = DMS_Bulletin.DBTrack.CreatedBy != null ? DMS_Bulletin.DBTrack.CreatedBy : null,
                            CreatedOn = DMS_Bulletin.DBTrack.CreatedOn != null ? DMS_Bulletin.DBTrack.CreatedOn : null,
                            IsModified = DMS_Bulletin.DBTrack.IsModified == true ? false : false//,
                            //AuthorizedBy = SessionManager.UserName,
                            //AuthorizedOn = DateTime.Now
                        };

                        db.Entry(DMS_Bulletin).State = System.Data.Entity.EntityState.Deleted;
                    
                        await db.SaveChangesAsync();


                        ts.Complete();
                        //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        Msg.Add("  Data removed.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
	}
}