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
using Attendance;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Attendance.MainController
{
    public class OTPolicyController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();
        //
        // GET: /OTPolicy/

        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/OTPolicy/Index.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(OTPolicy COBJ, FormCollection form)
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
                            // if (db.OTPolicy.Any(o => o.Name == COBJ.Name) && db.OTPolicy.Any(o => o.BreakTime == COBJ.BreakTime))
                            if (db.OTPolicy.Any(o => o.OTPoilicyName.Replace(" ", String.Empty) == COBJ.OTPoilicyName.Replace(" ", String.Empty)))
                            {
                                Msg.Add("  Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            OTPolicy OTPolicy = new OTPolicy()
                            {

                                BreakTime = COBJ.BreakTime,
                                //  Name = COBJ.Name,
                                OTPoilicyName = COBJ.OTPoilicyName,
                                compulsoryStay = COBJ.compulsoryStay,
                                COffOTHours = COBJ.COffOTHours,
                                CompensatoryOff = COBJ.CompensatoryOff,
                                DBTrack = COBJ.DBTrack
                            };
                            try
                            {
                                db.OTPolicy.Add(OTPolicy);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, COBJ.DBTrack);
                                DT_OTPolicy DT_OBJ = (DT_OTPolicy)rtn_Obj;
                                db.Create(DT_OBJ);
                                db.SaveChanges();

                                ts.Complete();
                                //return this.Json(new Object[] { null, null, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Created successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = COBJ.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        // return this.Json(new { msg = errorMsg });
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

                var OTPolicy = db.OTPolicy
                                    .Where(e => e.Id == data).ToList();
                var r = (from ca in OTPolicy
                         select new
                         {

                             Id = ca.Id,
                             BreakTime = ca.BreakTime,
                             // Name = ca.Name,
                             OTPoilicyName = ca.OTPoilicyName,
                             compulsoryStay = ca.compulsoryStay,
                             COffOTHours = ca.COffOTHours,
                             CompensatoryOff = ca.CompensatoryOff,
                             Action = ca.DBTrack.Action
                         }).Distinct();

                var a = "";

                var W = db.DT_OTPolicy
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         BreakTime = e.BreakTime == null ? "" : e.BreakTime,
                         OTPoilicyName = e.Name == null ? "" : e.Name,
                         compulsoryStay = e.compulsoryStay == null ? "" : e.compulsoryStay,
                         COffOTHours = e.COffOTHours == null ? "" : e.COffOTHours,
                         CompensatoryOff = e.CompensatoryOff == null ? false : e.CompensatoryOff,
                         //  OTPoilicyName = e.Name == null ? false : e.Name,
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var LKup = db.OTPolicy.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, a, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> EditSave(OTPolicy ESOBJ, FormCollection form, int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    OTPolicy blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;
                                     
                                        blog = db.OTPolicy.Where(e => e.Id == data)
                                                                .SingleOrDefault();
                                        originalBlogValues = db.Entry(blog).OriginalValues;
                                     
                                    ESOBJ.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    //int a = EditS(data, ESOBJ, ESOBJ.DBTrack);

                                      var CurOBJ = db.OTPolicy.Find(data);
                                        TempData["CurrRowVersion"] = CurOBJ.RowVersion;

                                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                        {

                                                CurOBJ.Id = data;
                                                CurOBJ.BreakTime = ESOBJ.BreakTime;
                                                // Name = ESOBJ.Name,
                                                CurOBJ.OTPoilicyName = ESOBJ.OTPoilicyName;
                                                CurOBJ.compulsoryStay = ESOBJ.compulsoryStay;
                                                CurOBJ.COffOTHours = ESOBJ.COffOTHours;
                                                CurOBJ.CompensatoryOff = ESOBJ.CompensatoryOff;
                                                CurOBJ.DBTrack = ESOBJ.DBTrack;


                                                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Modified;
                                                db.Entry(CurOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];



                                            var obj = DBTrackFile.DBTrackSave("Attendance/Attendance", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                            DT_OTPolicy DT_OBJ = (DT_OTPolicy)obj;
                                            db.Create(DT_OBJ);
                                            db.SaveChanges();

                                            ts.Complete();
                                        }

                                    // return Json(new Object[] { ESOBJ.Id, ESOBJ.BreakTime, "Record Updated", JsonRequestBehavior.AllowGet });
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.BreakTime.Value.ToShortTimeString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }

                            //catch (DbUpdateException e) { throw e; }
                            //catch (DataException e) { throw e; }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (OTPolicy)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var databaseValues = (OTPolicy)databaseEntry.ToObject();
                                    ESOBJ.RowVersion = databaseValues.RowVersion;

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


                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            OTPolicy blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            OTPolicy Old_Obj = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.OTPolicy.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            ESOBJ.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            OTPolicy corp = new OTPolicy()
                            {
                                BreakTime = ESOBJ.BreakTime,
                                //  Name = ESOBJ.Name,
                                OTPoilicyName = ESOBJ.OTPoilicyName,
                                compulsoryStay = ESOBJ.compulsoryStay,
                                COffOTHours = ESOBJ.COffOTHours,
                                CompensatoryOff = ESOBJ.CompensatoryOff,
                                Id = data,
                                DBTrack = ESOBJ.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Attendance/Attendance", "M", blog, corp, "OTPolicy", ESOBJ.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Obj = context.OTPolicy.Where(e => e.Id == data)
                                    .SingleOrDefault();
                                DT_OTPolicy DT_Corp = (DT_OTPolicy)obj;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = ESOBJ.DBTrack;
                            db.OTPolicy.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            //  return Json(new Object[] { blog.Id, ESOBJ.BreakTime, "Record Updated", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.BreakTime.Value.ToShortTimeString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }

                    }
                    return View();
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

        public int EditS(int data, OTPolicy ESOBJ, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var CurOBJ = db.OTPolicy.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    ESOBJ.DBTrack = dbT;
                    OTPolicy ESIOBJ = new OTPolicy()
                    {
                        Id = data,
                        BreakTime = ESOBJ.BreakTime,
                        // Name = ESOBJ.Name,
                        OTPoilicyName = ESOBJ.OTPoilicyName,
                        compulsoryStay = ESOBJ.compulsoryStay,
                        COffOTHours = ESOBJ.COffOTHours,
                        CompensatoryOff = ESOBJ.CompensatoryOff,
                        DBTrack = ESOBJ.DBTrack
                    };

                    db.OTPolicy.Attach(ESIOBJ);
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
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    OTPolicy OTPolicy = db.OTPolicy.Where(e => e.Id == data).SingleOrDefault();
                    try
                    {
                        //var selectedValues = OTPolicy.SocialActivities;
                        //var lkValue = new HashSet<int>(OTPolicy.SocialActivities.Select(e => e.Id));
                        //if (lkValue.Count > 0)
                        //{
                        //    return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                        //}

                        using (TransactionScope ts = new TransactionScope())
                        {
                            db.Entry(OTPolicy).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            ts.Complete();
                        }
                        //   Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    catch (DataException /* dex */)
                    {
                        // return this.Json(new Object[] { "", "Unable to delete. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
                        Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
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
                var LKVal = db.OTPolicy.ToList();

                if (gp.IsAutho == true)
                {
                    LKVal = db.OTPolicy.AsNoTracking().Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    LKVal = db.OTPolicy.AsNoTracking().ToList();
                }


                IEnumerable<OTPolicy> IE;
                if (!string.IsNullOrEmpty(gp.searchString))
                {
                    IE = LKVal;
                    if (gp.searchOper.Equals("eq"))
                    {

                        jsonData = IE.Where(e => (e.OTPoilicyName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.BreakTime.Value.ToShortTimeString().Contains(gp.searchString))
                               || (e.compulsoryStay.Value.ToShortTimeString().Contains(gp.searchString))
                               || (e.COffOTHours.Value.ToShortTimeString().Contains(gp.searchString))
                               || (e.CompensatoryOff.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.OTPoilicyName, a.BreakTime.Value.ToShortTimeString(), a.compulsoryStay.Value.ToShortTimeString(), a.COffOTHours.Value.ToShortTimeString(), a.CompensatoryOff.ToString(), a.Id }).ToList();

                        //jsonData = IE.Select(a => new { a.Id, a.OTPoilicyName, a.BreakTime, a.compulsoryStay, a.COffOTHours, a.CompensatoryOff }).Where((e => (e.Id.ToString() == gp.searchString) || (e.BreakTime.ToString() == gp.searchString.ToLower()) || (e.compulsoryStay.ToString() == gp.searchString.ToLower()) || (e.COffOTHours.ToString() == gp.searchString.ToLower()) || (e.CompensatoryOff.ToString() == gp.searchString.ToLower())));
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.OTPoilicyName, a.BreakTime, a.compulsoryStay, a.COffOTHours, a.CompensatoryOff, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = LKVal;
                    Func<OTPolicy, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "OTPoilicyName" ? c.OTPoilicyName.ToString() :
                                         gp.sidx == "BreakTime" ? c.BreakTime.Value.ToString() :
                                         gp.sidx == "compulsoryStay" ? c.compulsoryStay.Value.ToString() :
                                         gp.sidx == "COffOTHours" ? c.COffOTHours.Value.ToString() :
                                         gp.sidx == "CompensatoryOff" ? c.CompensatoryOff.ToString() :
                                         "");
                    }

                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.OTPoilicyName, a.BreakTime.Value.ToString("hh:mm tt"), a.compulsoryStay.Value.ToString("hh:mm tt"), a.COffOTHours.Value.ToString("hh:mm tt"), a.CompensatoryOff, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.OTPoilicyName, a.BreakTime.Value.ToString("hh:mm tt"), a.compulsoryStay.Value.ToString("hh:mm tt"), a.COffOTHours.Value.ToString("hh:mm tt"), a.CompensatoryOff, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.OTPoilicyName, a.BreakTime.Value.ToString("hh:mm tt"), a.compulsoryStay.Value.ToString("hh:mm tt"), a.COffOTHours.Value.ToString("hh:mm tt"), a.CompensatoryOff, a.Id }).ToList();
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
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save Authorized data
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            OTPolicy ESI = db.OTPolicy
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

                            db.OTPolicy.Attach(ESI);
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(ESI).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, ESI.DBTrack);
                            DT_OTPolicy DT_OBJ = (DT_OTPolicy)rtn_Obj;

                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            // return Json(new Object[] { ESI.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                            Msg.Add(" Record Authorized ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else if (auth_action == "M")
                    {

                        OTPolicy Old_OBJ = db.OTPolicy
                                                .Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_OTPolicy Curr_OBJ = db.DT_OTPolicy
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_OBJ != null)
                        {
                            OTPolicy OTPolicy = new OTPolicy();

                            //     OTPolicy.BreakTime = Curr_OBJ.BreakTime ==null ? Old_OBJ.BreakTime.ToString() : Curr_OBJ.BreakTime.ToString();
                            //  OTPolicy.Name = Curr_OBJ.Name == null ? Old_OBJ.Name : Curr_OBJ.Name;
                            //     OTPolicy.compulsoryStay = Curr_OBJ.compulsoryStay == null ? Old_OBJ.compulsoryStay : Curr_OBJ.compulsoryStay;
                            //       OTPolicy.COffOTHours = Curr_OBJ.COffOTHours == null ? Old_OBJ.COffOTHours : Curr_OBJ.COffOTHours;
                            OTPolicy.CompensatoryOff = Curr_OBJ.CompensatoryOff == null ? Old_OBJ.CompensatoryOff : Curr_OBJ.CompensatoryOff;
                            //OTPolicy.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        OTPolicy.DBTrack = new DBTrack
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

                                        int a = EditS(auth_id, OTPolicy, OTPolicy.DBTrack);

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        // return Json(new Object[] { OTPolicy.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                                        Msg.Add(" Record Authorized ");
                                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (OTPolicy)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                    else
                                    {
                                        var databaseValues = (OTPolicy)databaseEntry.ToObject();
                                        OTPolicy.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


                                //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                            Msg.Add("  Data removed from history.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        // return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //OTPolicy corp = db.OTPolicy.Find(auth_id);
                            OTPolicy ESI = db.OTPolicy.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

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

                            db.OTPolicy.Attach(ESI);
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, ESI.DBTrack);
                            DT_OTPolicy DT_OBJ = (DT_OTPolicy)rtn_Obj;
                            //DT_OBJ.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            //DT_OBJ.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                            //DT_OBJ.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            // return Json(new Object[] { "", "Record Authorized", JsonRequestBehavior.AllowGet });
                            Msg.Add(" Record Authorized ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }

                    }
                    return View();
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
