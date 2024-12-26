///
/// Created by Sarika
///

using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Collections;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Core.MainController
{
    public class ScolarshipController : Controller
    {
        List<String> Msg = new List<string>();
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/Scolarship/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Core/_scolarship.cshtml");
        }

        public ActionResult Create(Scolarship c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        DateTime d = DateTime.Now;
                        if (c.IssueDate > d)
                        {
                            Msg.Add("IssueDate Should Not Be Greater Than Current Date");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        if (db.Scolarship.Any(o => o.Name == c.Name))
                        {
                            Msg.Add("  Code Already Exists.  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        Scolarship awrd = new Scolarship()
                        {
                            Details = c.Details == null ? "" : c.Details.Trim(),
                            Name = c.Name == null ? "" : c.Name.Trim(),
                            IssueDate = c.IssueDate,
                            DBTrack = c.DBTrack
                        };
                        try
                        {
                            db.Scolarship.Add(awrd);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                            DT_Scolarship DT_Corp = (DT_Scolarship)rtn_Obj;

                            db.Create(DT_Corp);
                            db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Scolarship", null);


                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = awrd.Id, Val = awrd.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                        }
                        catch (DataException /* dex */)
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
                IEnumerable<Scolarship> awd = null;
                if (gp.IsAutho == true)
                {
                    awd = db.Scolarship.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    awd = db.Scolarship.AsNoTracking().ToList();
                }

                IEnumerable<Scolarship> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = awd;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.Name, a.IssueDate }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Name.ToLower() == gp.searchString.ToLower()) || (e.IssueDate.ToString() == gp.searchString.ToLower()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.IssueDate }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = awd;
                    Func<Scolarship, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Name" ? c.Name :
                                         gp.sidx == "Name" ? c.Name : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name),a.IssueDate.Value.ToShortDateString() }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), a.IssueDate.Value.ToShortDateString() }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name,
                       
                        }).ToList();
                    }
                    totalRecords = awd.Count();
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
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var Q = db.Scolarship
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Name = e.Name,
                        Details = e.Details == null ? "" : e.Details,
                        IssueDate = e.IssueDate,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.Scolarship
                    .Where(e => e.Id == data).ToList();


                var W = db.DT_Scolarship
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Name = e.Name == null ? "" : e.Name,
                         Details = e.Details == null ? "" : e.Details,
                         IssueDate = e.IssueDate
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.Scolarship.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                Scolarship Aw = db.Scolarship.Where(e => e.Id == data).SingleOrDefault();


                //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                if (Aw.DBTrack.IsModified == true)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            CreatedBy = Aw.DBTrack.CreatedBy != null ? Aw.DBTrack.CreatedBy : null,
                            CreatedOn = Aw.DBTrack.CreatedOn != null ? Aw.DBTrack.CreatedOn : null,
                            IsModified = Aw.DBTrack.IsModified == true ? true : false
                        };
                        Aw.DBTrack = dbT;
                        db.Entry(Aw).State = System.Data.Entity.EntityState.Modified;
                        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Aw.DBTrack);
                        DT_Scolarship DT_Corp = (DT_Scolarship)rtn_Obj;

                        db.Create(DT_Corp);
                        // db.SaveChanges();
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                        await db.SaveChangesAsync();
                        //using (var context = new DataBaseContext())
                        //{
                        //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                        //}
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
                                CreatedBy = Aw.DBTrack.CreatedBy != null ? Aw.DBTrack.CreatedBy : null,
                                CreatedOn = Aw.DBTrack.CreatedOn != null ? Aw.DBTrack.CreatedOn : null,
                                IsModified = Aw.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                            db.Entry(Aw).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            DT_Scolarship DT_Corp = (DT_Scolarship)rtn_Obj;

                            db.Create(DT_Corp);

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
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
        }

        public async Task<ActionResult> EditSave1(Scolarship c, int data, FormCollection form) // Edit submit
        {
            bool Auth = form["Autho_Allow"] == "true" ? true : false;

            using (DataBaseContext db = new DataBaseContext())
            {
                if (Auth == false)
                {


                    if (ModelState.IsValid)
                    {
                        try
                        {

                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                Scolarship blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.Scolarship.Where(e => e.Id == data).SingleOrDefault();
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

                                int a = EditS(data, c, c.DBTrack);



                                using (var context = new DataBaseContext())
                                {

                                    var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                    DT_Scolarship DT_Corp = (DT_Scolarship)obj;
                                    db.Create(DT_Corp);
                                    db.SaveChanges();
                                }
                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (Scolarship)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var databaseValues = (Scolarship)databaseEntry.ToObject();
                                c.RowVersion = databaseValues.RowVersion;

                            }
                        }

                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        Scolarship blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        Scolarship Old_Corp = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.Scolarship.Where(e => e.Id == data).SingleOrDefault();
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
                        Scolarship corp = new Scolarship()
                        {
                            Name = c.Name,
                            Details = c.Details,
                            IssueDate = c.IssueDate,
                            Id = data,
                            DBTrack = c.DBTrack,
                            RowVersion = (Byte[])TempData["RowVersion"]
                        };


                        //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                        using (var context = new DataBaseContext())
                        {
                            var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Scolarship", c.DBTrack);
                            // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                            Old_Corp = context.Scolarship.Where(e => e.Id == data).SingleOrDefault();
                            DT_Scolarship DT_Corp = (DT_Scolarship)obj;
                            db.Create(DT_Corp);
                            //db.SaveChanges();
                        }
                        blog.DBTrack = c.DBTrack;
                        db.Scolarship.Attach(blog);
                        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        ts.Complete();
                        Msg.Add("  Record Updated");
                        return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                }
                return View();
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(Scolarship c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.Scolarship.Where(e => e.Id == data).SingleOrDefault();

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();
                                DateTime d = DateTime.Now;
                                if (c.IssueDate > d)
                                {
                                    Msg.Add("IssueDate Should Not Be Greater Than Current Date");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    db.Scolarship.Attach(db_data);
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_data.RowVersion;
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_Lookup = db.Scolarship.Find(data);
                                    TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                    db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        Scolarship blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.Scolarship.Where(e => e.Id == data)
                                                              .SingleOrDefault();
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
                                        Scolarship lk = new Scolarship
                                        {

                                            Name = c.Name,
                                            Details = c.Details,
                                            IssueDate = c.IssueDate,
                                            Id = data,
                                            DBTrack = c.DBTrack,

                                        };


                                        db.Scolarship.Attach(lk);
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                        db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

                                        // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


                                        using (var context = new DataBaseContext())
                                        {

                                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                            DT_Scolarship DT_Corp = (DT_Scolarship)obj;

                                            db.Create(DT_Corp);
                                            db.SaveChanges();
                                        }
                                        await db.SaveChangesAsync();
                                        ts.Complete();

                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = lk.Id, Val = lk.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { lk.Id, "", "Record Updated", JsonRequestBehavior.AllowGet });
                                    }
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Scolarship)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (Scolarship)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

                                }
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

                            Scolarship blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            Scolarship Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.Scolarship.Where(e => e.Id == data).SingleOrDefault();
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
                            Scolarship qualificationDetails = new Scolarship()
                            {

                                Id = data,
                                Name = c.Name,
                                Details = c.Details,
                                IssueDate = c.IssueDate,
                                DBTrack = c.DBTrack
                            };

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, qualificationDetails, "Scolarship", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.Scolarship.Where(e => e.Id == data)
                                 .SingleOrDefault();
                                DT_Scolarship DT_Corp = (DT_Scolarship)obj;
                                //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.Scolarship.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();

                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, c.Institute, "Record Updated", JsonRequestBehavior.AllowGet });
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

                return View();
            }
        }



        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save Authorized data
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (auth_action == "C")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //Corporate corp = db.Corporate.Find(auth_id);
                        //Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                        Scolarship corp = db.Scolarship.FirstOrDefault(e => e.Id == auth_id);

                        corp.DBTrack = new DBTrack
                        {
                            Action = "C",
                            ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                            CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                            CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                            IsModified = corp.DBTrack.IsModified == true ? false : false,
                            AuthorizedBy = SessionManager.UserName,
                            AuthorizedOn = DateTime.Now
                        };

                        db.Scolarship.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //db.SaveChanges();
                        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                        DT_Scolarship DT_Corp = (DT_Scolarship)rtn_Obj;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();

                        ts.Complete();
                        return Json(new Object[] { corp.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                    }
                }
                else if (auth_action == "M")
                {

                    Scolarship Old_Corp = db.Scolarship.Where(e => e.Id == auth_id).SingleOrDefault();


                    DT_Scolarship Curr_Corp = db.DT_Scolarship
                                                .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                .OrderByDescending(e => e.Id)
                                                .FirstOrDefault();

                    if (Curr_Corp != null)
                    {
                        Scolarship corp = new Scolarship();

                        corp.Name = Curr_Corp.Name == null ? Old_Corp.Name : Curr_Corp.Name;
                        corp.Details = Curr_Corp.Details == null ? Old_Corp.Details : Curr_Corp.Details;
                        corp.IssueDate = Curr_Corp.IssueDate == null ? Old_Corp.IssueDate : Curr_Corp.IssueDate;
                        //      corp.Id = auth_id;

                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    // db.Configuration.AutoDetectChangesEnabled = false;
                                    corp.DBTrack = new DBTrack
                                    {
                                        CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                                        CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
                                        ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
                                        AuthorizedBy = SessionManager.UserName,
                                        AuthorizedOn = DateTime.Now,
                                        IsModified = false
                                    };

                                    int a = EditS(auth_id, corp, corp.DBTrack);

                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    return Json(new Object[] { corp.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Scolarship)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (Scolarship)databaseEntry.ToObject();
                                    corp.RowVersion = databaseValues.RowVersion;
                                }
                            }

                            return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                        return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                }
                else if (auth_action == "D")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //Corporate corp = db.Corporate.Find(auth_id);
                        Scolarship corp = db.Scolarship.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                        corp.DBTrack = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                            CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                            CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                            IsModified = false,
                            AuthorizedBy = SessionManager.UserName,
                            AuthorizedOn = DateTime.Now
                        };

                        db.Scolarship.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                        DT_Scolarship DT_Corp = (DT_Scolarship)rtn_Obj;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();
                        db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return Json(new Object[] { "", "Record Authorized", JsonRequestBehavior.AllowGet });
                    }

                }
                return View();
            }
        }


        public int EditS(int data, Scolarship c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CurCorp = db.Scolarship.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    Scolarship corp = new Scolarship()
                    {
                        Name = c.Name,
                        Details = c.Details,
                        IssueDate = c.IssueDate,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.Scolarship.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }
        public void RollBack()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //  var context = DataContextFactory.GetDataContext();
                var changedEntries = db.ChangeTracker.Entries()
                    .Where(x => x.State != System.Data.Entity.EntityState.Unchanged).ToList();

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Modified))
                {
                    entry.CurrentValues.SetValues(entry.OriginalValues);
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Added))
                {
                    entry.State = System.Data.Entity.EntityState.Detached;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Deleted))
                {
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }

            }
        }
	}
}