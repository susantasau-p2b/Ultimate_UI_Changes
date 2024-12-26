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
using Training;
using P2BUltimate.Controllers;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Training.MainController
{
    public class TrainingMasterController : Controller
    {
        //
        // GET: /TrainingMaster/
        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/TrainingMaster/Index.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/Training/_ProgramList.cshtml");
        }

        //private DataBaseContext db = new DataBaseContext();


        [HttpPost]
        public ActionResult Create(TrainingMaster c, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<String> Msg = new List<String>();
                string ProgramList = form["ProgramDetailList"] == "0" ? "" : form["ProgramDetailList"];


                if (ProgramList != null)
                {
                    if (ProgramList != "")
                    {
                        int ContId = Convert.ToInt32(ProgramList);
                        var val = db.ProgramList.Where(e => e.Id == ContId).SingleOrDefault();
                        c.ProgramList = val;
                    }
                }

                c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };
                if (ModelState.IsValid)
                {
                    try
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (c.StartDate > c.EndDate)
                            {
                                Msg.Add("To Date should be greater than From Date.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { "", "", "To Date should be greater than From Date.", JsonRequestBehavior.AllowGet });
                            }
                            TrainingMaster trainingperiod = new TrainingMaster()
                            {

                                StartDate = c.StartDate,
                                EndDate = c.EndDate,
                                ProgramList = c.ProgramList,
                                DBTrack = c.DBTrack
                            };

                            db.TrainingMaster.Add(trainingperiod);
                            db.SaveChanges();



                            db.SaveChanges();
                            ts.Complete();
                        }
                        Msg.Add("Data Saved Successfully.");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { "", "", "Data saved successfully." }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new { Error = calendar.Id });
                        //return RedirectToAction("Index");
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                    }
                    catch (DataException /* dex */)
                    {
                        //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                        ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                        return View(c);
                    }
                }
                return View(c);
            }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            //string tableName = "Corporate";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.TrainingMaster
                    .Include(e => e.ProgramList)

                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        StartDate = e.StartDate,
                        EndDate = e.EndDate,

                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.TrainingMaster
                  .Include(e => e.ProgramList)

                    .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        ProgramList_FullDetails = e.ProgramList.FullDetails == null ? "" : e.ProgramList.FullDetails,
                        ProgramList_Id = e.ProgramList.Id == null ? "" : e.ProgramList.Id.ToString(),

                    }).ToList();

                return Json(new Object[] { Q, add_data, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(TrainingMaster c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string ProgramList = form["ProgramDetailList"] == "0" ? "" : form["ProgramDetailList"];
                //  bool Auth = form["autho_action"] == "" ? false : true;
                bool Auth = form["autho_allow"] == "true" ? true : false;


                List<String> Msg = new List<String>();


                if (ProgramList != null)
                {
                    if (ProgramList != "")
                    {
                        int ContId = Convert.ToInt32(ProgramList);
                        var val = db.ProgramList.Include(e => e.Budget)
                                            .Where(e => e.Id == ContId).SingleOrDefault();
                        c.ProgramList = val;
                    }
                }



                if (Auth == false)
                {


                    if (ModelState.IsValid)
                    {
                        try
                        {

                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                TrainingMaster blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.TrainingMaster.Where(e => e.Id == data)
                                                            .Include(e => e.ProgramList).SingleOrDefault();
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
                                c.StartDate = blog.StartDate;
                                c.EndDate = blog.EndDate;
                                int a = EditS(ProgramList, data, c, c.DBTrack);



                                using (var context = new DataBaseContext())
                                {

                                    var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);



                                    //DT_Corp = (DT_Corporate)obj;
                                    //DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
                                    //DT_Corp.BusinessType_Id = blog.BusinessType == null ? 0 : blog.BusinessType.Id;
                                    //DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
                                    //db.Create(DT_Corp);
                                    db.SaveChanges();
                                }
                                await db.SaveChangesAsync();
                                ts.Complete();



                                Msg.Add("  Data Updated successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { c.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (TrainingMaster)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add("Unable to save changes. The record was deleted by another user.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (TrainingMaster)databaseEntry.ToObject();
                                c.RowVersion = databaseValues.RowVersion;

                            }
                        }
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                    }
                }
                else
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        TrainingMaster blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        TrainingMaster Old_Corp = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.TrainingMaster.Where(e => e.Id == data).SingleOrDefault();
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

                        TrainingMaster corp = new TrainingMaster()
                        {
                            StartDate = c.StartDate,
                            EndDate = c.EndDate,
                            Id = data,
                            DBTrack = c.DBTrack,
                            RowVersion = (Byte[])TempData["RowVersion"]
                        };


                        //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                        //using (var context = new DataBaseContext())
                        //{
                        //    var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Corporate", c.DBTrack);
                        //    // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                        //    Old_Corp = context.Corporate.Where(e => e.Id == data).Include(e => e.BusinessType)
                        //        .Include(e => e.Address).Include(e => e.ContactDetails).SingleOrDefault();
                        //    DT_Corporate DT_Corp = (DT_Corporate)obj;
                        //    DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                        //    DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                        //    DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                        //    db.Create(DT_Corp);
                        //    //db.SaveChanges();
                        //}
                        blog.DBTrack = c.DBTrack;
                        db.TrainingMaster.Attach(blog);
                        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        ts.Complete();
                        Msg.Add("Record Updated Successfully.");
                        return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { blog.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                    }

                }
                return View();
            }
        }

        public int EditS(string ProgramList, int data, TrainingMaster c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (ProgramList != null)
                {
                    if (ProgramList != "")
                    {
                        var val = db.ProgramList.Find(int.Parse(ProgramList));
                        c.ProgramList = val;

                        var add = db.TrainingMaster.Include(e => e.ProgramList).Where(e => e.Id == data).SingleOrDefault();
                        IList<TrainingMaster> addressdetails = null;
                        if (add.ProgramList != null)
                        {
                            addressdetails = db.TrainingMaster.Where(x => x.ProgramList.Id == add.ProgramList.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.TrainingMaster.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.ProgramList = c.ProgramList;
                                db.TrainingMaster.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                // await db.SaveChangesAsync(false);
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                }
                else
                {
                    var addressdetails = db.TrainingMaster.Include(e => e.ProgramList).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.ProgramList = null;
                        db.TrainingMaster.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }



                var CurCorp = db.TrainingMaster.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    TrainingMaster corp = new TrainingMaster()
                    {
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.TrainingMaster.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Delete(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<String> Msg = new List<String>();
                TrainingMaster calendar = db.TrainingMaster.Find(data);
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(calendar).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }

                    Msg.Add("Data deleted.");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { "", "Data deleted.", JsonRequestBehavior.AllowGet });
                }

                catch (DataException /* dex */)
                {
                    Msg.Add("Unable to delete. Try again, and if the problem persists contact your system administrator.");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                    //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
                    //ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
                    //return RedirectToAction("Index");
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
                var Calendar = db.TrainingMaster.ToList();
                IEnumerable<TrainingMaster> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Calendar;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new Object[] { a.Id, a.StartDate.Value.ToString("dd/MM/yyyy"), a.EndDate.Value.ToString("dd/MM/yyyy") }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.StartDate.Value.ToString("dd/MM/yyyy"), a.EndDate.Value.ToString("dd/MM/yyyy") }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Calendar;
                    Func<TrainingMaster, string> orderfuc = (c =>
                                                               gp.sidx == "ID" ? c.Id.ToString() :

                                                               gp.sidx == "From Date" ? c.StartDate.ToString() :
                                                               gp.sidx == "To Date" ? c.EndDate.ToString() :
                                                                "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.StartDate.Value.ToString("dd/MM/yyyy"), a.EndDate.Value.ToString("dd/MM/yyyy") }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.StartDate.Value.ToString("dd/MM/yyyy"), a.EndDate.Value.ToString("dd/MM/yyyy") }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.StartDate.Value.ToString("dd/MM/yyyy"), a.EndDate.Value.ToString("dd/MM/yyyy") }).ToList();
                    }
                    totalRecords = Calendar.Count();
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

        //For Validating the form error
        public ActionResult ValidateForm(TrainingPeriod c, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (c.StartDate > c.EndDate)
                {
                    return Json(new { success = false, responseText = "To Date should be greater than From Date." }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new Object[] { "", "", "To Date should be greater than From Date.", JsonRequestBehavior.AllowGet });
                }

                if (db.TrainingPeriod.Any(o => o.StartDate == c.StartDate))
                {
                    //return this.Json(new Object[] { "", "", "From Date already exists.", JsonRequestBehavior.AllowGet });
                    return Json(new { success = false, responseText = "From Date already exists." }, JsonRequestBehavior.AllowGet);
                }

                if (db.TrainingPeriod.Any(o => o.EndDate == c.EndDate))
                {
                    //ModelState.AddModelError(string.Empty, "To Date already exists.");
                    // return this.Json(new Object[] { "", "", "To Date already exists.", JsonRequestBehavior.AllowGet });
                    return Json(new { success = false, responseText = "To Date already exists." }, JsonRequestBehavior.AllowGet);
                    // return View(c);
                }
                // for success
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                //for error
                //return Json(new { success = false, responseText = "Not Valid..!" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}