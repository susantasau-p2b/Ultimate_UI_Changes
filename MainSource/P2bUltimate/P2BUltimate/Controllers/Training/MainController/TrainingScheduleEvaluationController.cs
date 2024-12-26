///
/// Created by Tanushri
///


using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Payroll;
using Training;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Training.MainController
{
    public class TrainingScheduleEvaluationController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /TrainingExpenses/

        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/TrainingScheduleEvaluation/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Training/_TrainingExpenses.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(TrainingSchedule COBJ, FormCollection form, string batch)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            TrainingSchedule trs = db.TrainingSchedule.Where(e => e.TrainingBatchName == batch).SingleOrDefault();

                            COBJ.DBTrack = new DBTrack
                            {
                                CreatedBy = trs.DBTrack.CreatedBy == null ? null : trs.DBTrack.CreatedBy,
                                CreatedOn = trs.DBTrack.CreatedOn == null ? null : trs.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            if (trs != null)
                            {
                                trs.OverallFaultyRating = COBJ.OverallFaultyRating;
                                trs.OverallTrainingFeedback = COBJ.OverallTrainingFeedback;
                                trs.OverallTrainingRating = COBJ.OverallTrainingRating;
                                trs.FacultyFeedback = COBJ.FacultyFeedback;
                                trs.DBTrack = COBJ.DBTrack;
                            }
                            try
                            {
                                db.TrainingSchedule.Attach(trs);
                                db.Entry(trs).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(trs).State = System.Data.Entity.EntityState.Detached;


                                ts.Complete();

                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = trs.Id, Val = trs.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = COBJ.Id });
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
        public ActionResult Edit(int data, string batch)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var TrainingExpenses = db.TrainingSchedule
                                    .Where(e => e.Id == data).ToList();
                var r = (from ca in TrainingExpenses
                         select new
                         {

                             Id = ca.Id,
                             OverallFaultyRating = ca.OverallFaultyRating,
                             OverallTrainingFeedback = ca.OverallTrainingFeedback,
                             OverallTrainingRating = ca.OverallTrainingRating,
                             FacultyFeedback = ca.FacultyFeedback,
                             TrainingBatchName = ca.TrainingBatchName,
                             Batchname = batch,
                             Action = ca.DBTrack.Action
                         }).Distinct();

                var a = "";

                //var W = db.DT_TrainingExpenses
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,
                //         FoodFees = e.FoodFees == null ? 0.0 : e.FoodFees,
                //         MiscFees = e.MiscFees == null ? 0.0 : e.MiscFees,
                //         StayFees = e.StayFees == null ? 0.0 : e.StayFees,
                //         TrainingFees = e.TrainingFees == null ? 0.0 : e.TrainingFees,
                //         TravelFees = e.TravelFees == null ? 0.0 : e.TravelFees,
                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var LKup = db.TrainingSchedule.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, a, "", "", JsonRequestBehavior.AllowGet });
            }
        }



        [HttpPost]
        public async Task<ActionResult> EditSave(TrainingSchedule c, int data, FormCollection form, string batch) // Edit submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    //var BatchName = form["batchname"] == "" ? null : form["batchname"];
                    var BatchName = form["txtPBatchName1"] == "" ? null : form["txtPBatchName1"];
                    
                    TrainingSchedule db_data = db.TrainingSchedule.Where(e => e.Id == data).SingleOrDefault();

                    //int TrSchId = Convert.ToInt32(BatchName);
                    //TrainingSchedule trSch = db.TrainingSchedule.Find(TrSchId);
                    TrainingSchedule trSch = db.TrainingSchedule.Where(e => e.TrainingBatchName == BatchName).FirstOrDefault();
                    if (trSch != null)
                    {
                        if (trSch.IsBatchClose == true)
                        {
                            //return Json(new { status = false, responseText = "Batch is closed. You can't edit this record now..!" }, JsonRequestBehavior.AllowGet);
                            Msg.Add("Batch is closed. You can't edit this record now..!");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    } 


                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                TrainingSchedule blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                //int TrSchIdI = Convert.ToInt32(batch);
                                //TrainingSchedule trSchI = db.TrainingSchedule.Find(TrSchIdI);
                                //if (trSchI != null)
                                //{
                                //    if (trSchI.IsBatchClose == true)
                                //    {
                                //        return Json(new { status = false, responseText = "Batch is closed. You can't edit this record now..!" }, JsonRequestBehavior.AllowGet);
                                //    }
                                //}


                                using (var context = new DataBaseContext())
                                {
                                    blog = context.TrainingSchedule.Where(e => e.Id == data).SingleOrDefault();
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

                                var CurOBJ = db.TrainingSchedule.Find(data);
                                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    if (db_data != null)
                                    {
                                        db_data.OverallFaultyRating = c.OverallFaultyRating;
                                        db_data.OverallTrainingFeedback = c.OverallTrainingFeedback;
                                        db_data.OverallTrainingRating = c.OverallTrainingRating;
                                        db_data.FacultyFeedback = c.FacultyFeedback;
                                        db_data.DBTrack = c.DBTrack;
                                    }
                                    //TrainingSchedule db_data = new TrainingSchedule()
                                    //{
                                    //    Id = data,
                                    //    OverallFaultyRating = c.OverallFaultyRating,
                                    //    OverallTrainingFeedback = c.OverallTrainingFeedback,
                                    //    OverallTrainingRating =c.OverallTrainingRating,
                                    //    FacultyFeedback = c.FacultyFeedback,
                                    //    DBTrack = c.DBTrack
                                    //};

                                    db.TrainingSchedule.Attach(db_data);
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(db_data).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                }

                                await db.SaveChangesAsync();

                                ts.Complete();
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

                            TrainingSchedule blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;


                            using (var context = new DataBaseContext())
                            {
                                blog = context.TrainingSchedule.Where(e => e.Id == data).SingleOrDefault();
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

                            TrainingSchedule corp = new TrainingSchedule()
                            {
                                OverallFaultyRating = c.OverallFaultyRating,
                                OverallTrainingFeedback = c.OverallTrainingFeedback,
                                OverallTrainingRating = c.OverallTrainingRating,
                                FacultyFeedback = c.FacultyFeedback,
                                DBTrack = c.DBTrack,
                                Id = data,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];


                            blog.DBTrack = c.DBTrack;
                            db.TrainingSchedule.Attach(blog);
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
                    var clientValues = (Category)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (Category)databaseEntry.ToObject();
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

        public int EditS(int data, TrainingExpenses ESOBJ, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CurOBJ = db.TrainingExpenses.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    ESOBJ.DBTrack = dbT;
                    TrainingExpenses ESIOBJ = new TrainingExpenses()
                    {
                        Id = data,
                        FoodFees = ESOBJ.FoodFees,
                        MiscFees = ESOBJ.MiscFees,
                        StayFees = ESOBJ.StayFees,
                        TrainingFees = ESOBJ.TrainingFees,
                        TravelFees = ESOBJ.TravelFees,
                        DBTrack = ESOBJ.DBTrack
                    };

                    db.TrainingExpenses.Attach(ESIOBJ);
                    db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Delete(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {

                TrainingSchedule TrainingSchedule = db.TrainingSchedule.Where(e => e.Id == data).SingleOrDefault();
                try
                {

                    var chk = db.TrainingDetails.Include(e => e.TrainingSchedule).Where(e => e.TrainingSchedule.Id == data).ToList();
                    if (chk.Count() != 0)
                    {
                        Msg.Add("Unable to delete, record is used in TrainingDetails.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(TrainingSchedule).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }

                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                }

                catch (DataException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                int ParentId = 2;
                var jsonData = (Object)null;
                var LKVal = db.TrainingSchedule.ToList();

                if (gp.filter == null)
                {
                    //LKVal = db.TrainingSchedule.Where(e => e.FacultyFeedback != "" && e.OverallFaultyRating != "" && e.OverallTrainingFeedback != "" && e.OverallTrainingRating != "" ).AsNoTracking().ToList();
                }
                else
                {
                    LKVal = db.TrainingSchedule.Where(e => e.FacultyFeedback != "" && e.OverallFaultyRating != "" && e.OverallTrainingFeedback != "" && e.OverallTrainingRating != "" && e.TrainingBatchName == gp.filter).AsNoTracking().ToList();
                }


                IEnumerable<TrainingSchedule> IE;
                if (!string.IsNullOrEmpty(gp.searchString))
                {
                    IE = LKVal;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.OverallFaultyRating.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.FacultyFeedback.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.OverallTrainingRating.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.OverallTrainingFeedback.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.TrainingBatchName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))

                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new { a.OverallFaultyRating, a.FacultyFeedback, a.OverallTrainingRating, a.OverallTrainingFeedback, a.TrainingBatchName, a.Id }).ToList();

                        //jsonData = IE.Select(a => new { a.OverallFaultyRating, a.FacultyFeedback, a.OverallTrainingRating, a.OverallTrainingFeedback, a.Id }).Where((e => (e.Id.ToString() == gp.searchString) || (e.OverallFaultyRating.ToString() == gp.searchString.ToLower()) || (e.FacultyFeedback.ToString() == gp.searchString.ToLower()) || (e.OverallTrainingRating.ToString() == gp.searchString.ToLower()) || (e.OverallTrainingFeedback.ToString() == gp.searchString.ToLower())));
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.OverallFaultyRating, a.FacultyFeedback, a.OverallTrainingRating, a.OverallTrainingFeedback, a.TrainingBatchName, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = LKVal;
                    Func<TrainingSchedule, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "OverallFaultyRating" ? c.OverallFaultyRating.ToString() :
                                         gp.sidx == "FacultyFeedback " ? c.FacultyFeedback.ToString() :
                                         gp.sidx == "OverallTrainingRating " ? c.OverallTrainingRating.ToString() :
                                         gp.sidx == "OverallTrainingFeedback " ? c.OverallTrainingFeedback.ToString() :
                                         gp.sidx == "TrainingBatchName " ? c.TrainingBatchName.ToString() :

                                         "");
                    }

                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.OverallFaultyRating, a.FacultyFeedback, a.OverallTrainingRating, a.OverallTrainingFeedback, a.TrainingBatchName, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.OverallFaultyRating, a.FacultyFeedback, a.OverallTrainingRating, a.OverallTrainingFeedback, a.TrainingBatchName, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  a.OverallFaultyRating, a.FacultyFeedback, a.OverallTrainingRating, a.OverallTrainingFeedback, a.TrainingBatchName, a.Id }).ToList();
                    }
                    totalRecords = LKVal.Count();
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
                    total = totalPages,
                    p2bparam = ParentId
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (auth_action == "C")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        TrainingExpenses ESI = db.TrainingExpenses
                            .FirstOrDefault(e => e.Id == auth_id);

                        ESI.DBTrack = new DBTrack
                        {
                            Action = "C",
                            ModifiedBy = ESI.DBTrack.ModifiedBy != null ? ESI.DBTrack.ModifiedBy : null,
                            CreatedBy = ESI.DBTrack.CreatedBy != null ? ESI.DBTrack.CreatedBy : null,
                            CreatedOn = ESI.DBTrack.CreatedOn != null ? ESI.DBTrack.CreatedOn : null,
                            IsModified = ESI.DBTrack.IsModified == true ? false : false,
                            AuthorizedBy = SessionManager.UserName,
                            AuthorizedOn = DateTime.Now
                        };

                        db.TrainingExpenses.Attach(ESI);
                        db.Entry(ESI).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(ESI).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //db.SaveChanges();
                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, ESI.DBTrack);
                        DT_TrainingExpenses DT_OBJ = (DT_TrainingExpenses)rtn_Obj;

                        db.Create(DT_OBJ);
                        await db.SaveChangesAsync();

                        ts.Complete();
                        return Json(new Object[] { ESI.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                    }
                }
                else if (auth_action == "M")
                {

                    TrainingExpenses Old_OBJ = db.TrainingExpenses
                                            .Where(e => e.Id == auth_id).SingleOrDefault();


                    DT_TrainingExpenses Curr_OBJ = db.DT_TrainingExpenses
                                                .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                .OrderByDescending(e => e.Id)
                                                .FirstOrDefault();

                    if (Curr_OBJ != null)
                    {
                        TrainingExpenses TrainingExpenses = new TrainingExpenses();

                        TrainingExpenses.FoodFees = Curr_OBJ.FoodFees == 0.0 ? Old_OBJ.FoodFees : Curr_OBJ.FoodFees;
                        TrainingExpenses.MiscFees = Curr_OBJ.MiscFees == 0.0 ? Old_OBJ.MiscFees : Curr_OBJ.MiscFees;
                        TrainingExpenses.StayFees = Curr_OBJ.StayFees == 0.0 ? Old_OBJ.StayFees : Curr_OBJ.StayFees;
                        TrainingExpenses.TrainingFees = Curr_OBJ.TrainingFees == 0.0 ? Old_OBJ.TrainingFees : Curr_OBJ.TrainingFees;
                        TrainingExpenses.TravelFees = Curr_OBJ.TravelFees == 0.0 ? Old_OBJ.TravelFees : Curr_OBJ.TravelFees;
                        //      corp.Id = auth_id;

                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    // db.Configuration.AutoDetectChangesEnabled = false;
                                    TrainingExpenses.DBTrack = new DBTrack
                                    {
                                        CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
                                        CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = Old_OBJ.DBTrack.ModifiedBy == null ? null : Old_OBJ.DBTrack.ModifiedBy,
                                        ModifiedOn = Old_OBJ.DBTrack.ModifiedOn == null ? null : Old_OBJ.DBTrack.ModifiedOn,
                                        AuthorizedBy = SessionManager.UserName,
                                        AuthorizedOn = DateTime.Now,
                                        IsModified = false
                                    };

                                    int a = EditS(auth_id, TrainingExpenses, TrainingExpenses.DBTrack);

                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    return Json(new Object[] { TrainingExpenses.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (TrainingExpenses)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (TrainingExpenses)databaseEntry.ToObject();
                                    TrainingExpenses.RowVersion = databaseValues.RowVersion;
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
                        //TrainingExpenses corp = db.TrainingExpenses.Find(auth_id);
                        TrainingExpenses ESI = db.TrainingExpenses.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                        //Address add = corp.Address;
                        //ContactDetails conDet = corp.ContactDetails;
                        //SocialActivities val = corp.BusinessType;

                        ESI.DBTrack = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = ESI.DBTrack.ModifiedBy != null ? ESI.DBTrack.ModifiedBy : null,
                            CreatedBy = ESI.DBTrack.CreatedBy != null ? ESI.DBTrack.CreatedBy : null,
                            CreatedOn = ESI.DBTrack.CreatedOn != null ? ESI.DBTrack.CreatedOn : null,
                            IsModified = false,
                            AuthorizedBy = SessionManager.UserName,
                            AuthorizedOn = DateTime.Now
                        };

                        db.TrainingExpenses.Attach(ESI);
                        db.Entry(ESI).State = System.Data.Entity.EntityState.Deleted;


                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, ESI.DBTrack);
                        DT_TrainingExpenses DT_OBJ = (DT_TrainingExpenses)rtn_Obj;
                        //DT_OBJ.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                        //DT_OBJ.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                        //DT_OBJ.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                        db.Create(DT_OBJ);
                        await db.SaveChangesAsync();
                        db.Entry(ESI).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
                    }

                }
                return View();
            }
        }
    }
}
