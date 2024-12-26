///
/// Created by Tanushri
///

using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using System.Threading.Tasks;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Core.MainController
{

     [AuthoriseManger]
    public class LevelController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /Level/

        public ActionResult Index()
        {
			return View("~/Views/Core/MainViews/Level/Index.cshtml");
        }

        /*---------------------------------------------------------- Create ---------------------------------------------- */
        //[HttpPost]
        //public ActionResult CreateSave(Level le)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<string> Msg = new List<string>();
        //        try
        //        {
        //            int company_Id = 0;
        //            var Company = new Company();
        //            if (company_Id != null)
        //            {
        //                company_Id = Convert.ToInt32(Session["CompId"]);
        //            }

        //            if (ModelState.IsValid)
        //            {
        //                if (db.Level.Any(o => o.Code == le.Code))
        //                {
        //                    Msg.Add("  Code Already Exists.  ");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return this.Json(new Object[] { null, null, "Code Already Exists.", JsonRequestBehavior.AllowGet });
        //                }

        //                le.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };
        //                Level les = new Level()
        //                {
        //                    Code = le.Code,
        //                    Name = le.Name,
        //                    DBTrack = le.DBTrack
        //                };

        //                try
        //                {
        //                    using (TransactionScope ts = new TransactionScope())
        //                    {
        //                        db.Level.Add(les);
        //                        DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, le.DBTrack);
        //                        db.SaveChanges();

        //                        if (Company != null)
        //                        {
        //                            var ObjLevel = new List<Level>();
        //                            ObjLevel.Add(les);
        //                            Company.Level = ObjLevel;
        //                            db.Company.Attach(Company);
        //                            db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
        //                            db.SaveChanges();
        //                            db.Entry(Company).State = System.Data.Entity.EntityState.Detached;
        //                        }


        //                        ts.Complete();
        //                        Msg.Add("  Data Saved successfully  ");
        //                        return Json(new Utility.JsonReturnClass { Id = les.Id, Val = le.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
        //                        //return this.Json(new Object[] {,  , "Data saved successfully." }, JsonRequestBehavior.AllowGet);
        //                    }
        //                }
        //                catch (DbUpdateConcurrencyException)
        //                {
        //                    return RedirectToAction("Create", new { concurrencyError = true, id = le.Id });
        //                }
        //                catch (DataException /* dex */)
        //                {
        //                    Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //            else
        //            {
        //                StringBuilder sb = new StringBuilder("");
        //                foreach (ModelState modelState in ModelState.Values)
        //                {
        //                    foreach (ModelError error in modelState.Errors)
        //                    {
        //                        sb.Append(error.ErrorMessage);
        //                        sb.Append("." + "\n");
        //                    }
        //                }
        //                var errorMsg = sb.ToString();
        //                Msg.Add(errorMsg);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //var errorMsg = sb.ToString();
        //                //return this.Json(new { msg = errorMsg });
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LogFile Logfile = new LogFile();
        //            ErrorLog Err = new ErrorLog()
        //            {
        //                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                ExceptionMessage = ex.Message,
        //                ExceptionStackTrace = ex.StackTrace,
        //                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult CreateSave(Level c) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int company_Id = 0;
                    Company Company = new Company();
                    if (company_Id != null)
                    {
                        company_Id = Convert.ToInt32(Session["CompId"]);
                        Company = db.Company.Where(q=>q.Id==company_Id).SingleOrDefault();
                    }

                    if (ModelState.IsValid)
                    {
                        if (db.Level.Any(o => o.Code == c.Code))
                        {
                            Msg.Add("  Code Already Exists.  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return this.Json(new Object[] { null, null, "Code Already Exists.", JsonRequestBehavior.AllowGet });
                        }

                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            Level les = new Level()
                            {
                                Code = c.Code,
                                Name = c.Name,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.Level.Add(les);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                db.SaveChanges();

                                if (Company != null)
                                {
                                    List<Level> ObjLevel = new List<Level>();
                                    ObjLevel.Add(les);
                                    Company.Level = ObjLevel;
                                    db.Company.Attach(Company);
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Detached;
                                }

                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
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

                        // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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

        /* ------------------------------ Edit --------------------------- */

        //[HttpPost]
        //public ActionResult Edit(Level lvl)
        //{
        //    return this.Json(new { msg = "test", JsonRequestBehavior.AllowGet });
        //}

        //public ActionResult PopupGrid(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        int ParentId = Convert.ToInt32(gp.extraeditdata);
        //        var jsonData = (Object)null;
        //        ICollection<Level> Level = new List<Level>();
        //        var grade = db.Grade.Include(e => e.Levels).Where(e => e.Id == ParentId).SingleOrDefault();

        //        if (grade != null)
        //        {
        //            Level = grade.Levels;
        //        }
        //      // var Level = db.Level.ToList();
        //        IEnumerable<Level> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = Level;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new Object[] { "",a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name) }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = Level;
        //            Func<Level, string> orderfuc = (c =>
        //                                                       gp.sidx == "ID" ? c.Id.ToString() :
        //                                                       gp.sidx == "Code" ? c.Code :
        //                                                       gp.sidx == "Name" ? c.Name : "");
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { "",a.Id, a.Code, a.Name }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { "",a.Id, a.Code, a.Name }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {"", a.Id, a.Code, a.Name }).ToList();
        //            }
        //            totalRecords = Level.Count();
        //        }
        //        if (totalRecords > 0)
        //        {
        //            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //        }
        //        if (gp.page > totalPages)
        //        {
        //            gp.page = totalPages;
        //        }
        //        var JsonData = new
        //        {
        //            page = gp.page,
        //            rows = jsonData,
        //            records = totalRecords,
        //            total = totalPages,
        //            p2bparam = ParentId
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}


        /* ------------------------------------- Edit Save ------------------------------------------------- */

        //public ActionResult EditLevel_partial(int? data)
        //{

        //    var jp = (from ca in db.Level
        //              select new
        //              {
        //                  Id = ca.Id,
        //                  Code = ca.Code,
        //                  Name = ca.Name,

        //              }).Where(e => e.Id == data).Distinct();

        //    return Json(jp, JsonRequestBehavior.AllowGet);
        //}


        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.Level
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Code = e.Code,
                        Name = e.Name,
                        Action = e.DBTrack.Action
                    }).ToList();



                var W = db.DT_Level
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Code = e.Code == null ? "" : e.Code,
                         Name = e.Name == null ? "" : e.Name
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var level = db.Level.Find(data);
                TempData["RowVersion"] = level.RowVersion;
                var Auth = level.DBTrack.IsModified;
                return Json(new Object[] { Q, "", W, Auth, JsonRequestBehavior.AllowGet });
            }
        }



        //public ActionResult EditSave(Level jp, int data)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            Level blog = null; // to retrieve old data
        //            DbPropertyValues originalBlogValues = null;
        //            Level Old_OBJ = null;

        //            using (var context = new DataBaseContext())
        //            {
        //                blog = context.Level.Where(e => e.Id == data).SingleOrDefault();
        //                originalBlogValues = context.Entry(blog).OriginalValues;
        //            }
        //            jp.DBTrack = new DBTrack
        //            {
        //                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                Action = "M",
        //                IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                ModifiedBy = SessionManager.UserName,
        //                ModifiedOn = DateTime.Now
        //            };
        //            Level le = new Level()
        //            {
        //                Code = jp.Code,
        //                Name = jp.Name,
        //                 DBTrack = jp.DBTrack,
        //                Id = data
        //            };
        //            try
        //            {
        //                using (var context = new DataBaseContext())
        //                {
        //                    var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, le, "Level", jp.DBTrack);
        //                    // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                    Old_OBJ = context.Level.Where(e => e.Id == data).SingleOrDefault();

        //                    //DT_Level DT_OBJ = (DT_Level)obj;

        //                    //// DT_OBJ.InstituteType_Id = DBTrackFile.ValCompare(Old_OBJ.IsManualRotateShift, ESOBJ.IsManualRotateShift); //Old_OBJ.BusinessType == c.BusinessType ? 0 : Old_OBJ.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_OBJ.BusinessType.Id;
        //                    //DT_OBJ.Grades_Id = DBTrackFile.ValCompare(Old_OBJ.Grades, jp.Grades); //Old_OBJ.Levels == c.Levels ? 0 : Old_OBJ.Levels == null && c.Levels != null ? c.Levels.Id : Old_OBJ.Levels.Id;
        //                    //db.Create(DT_OBJ);
        //                    //db.SaveChanges();
        //                }
        //                blog.DBTrack = jp.DBTrack;
        //                db.Level.Attach(blog);
        //                db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                db.SaveChanges();
        //                ts.Complete();
        //                return Json(new Object[] { blog.Id, le.Name, "Record Updated", JsonRequestBehavior.AllowGet });
        //            }
        //            catch (DbUpdateConcurrencyException)
        //            {
        //                return RedirectToAction("Create", new { concurrencyError = true, id = le.Id });
        //            }
        //            catch (DataException /* dex */)
        //            {

        //                return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
        //            }
        //        }
        //    }
        //    else
        //    {
        //        StringBuilder sb = new StringBuilder("");
        //        foreach (ModelState modelState in ModelState.Values)
        //        {
        //            foreach (ModelError error in modelState.Errors)
        //            {
        //                sb.Append(error.ErrorMessage);
        //                sb.Append("." + "\n");
        //            }
        //        }
        //        var errorMsg = sb.ToString();
        //        return this.Json(new { msg = errorMsg, JsonRequestBehavior.AllowGet });
        //    }
        //}



		//public async Task<ActionResult> EditSave(Level ESOBJ, int data)
		//{

		//	try
		//	{
		//		if (ModelState.IsValid)
		//		{
		//			using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
		//			{
		//				//db.BasicScaleDetails.Attach(OBJ);
		//				//db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
		//				//db.SaveChanges();
		//				//db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
		//				//ts.Complete();
		//				var Curr_OBJ = db.Level.Find(data);
		//				TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
		//				db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;
                       
		//					Level blog = blog = null;
		//					DbPropertyValues originalBlogValues = null;

		//					using (var context = new DataBaseContext())
		//					{
		//						blog = context.Level.Where(e => e.Id == data).SingleOrDefault();
		//						originalBlogValues = context.Entry(blog).OriginalValues;
		//					}

		//					ESOBJ.DBTrack = new DBTrack
		//					{
		//						CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
		//						CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
		//						Action = "M",
		//						ModifiedBy = SessionManager.UserName,
		//						ModifiedOn = DateTime.Now
		//					};
		//					Level OBJ = new Level
		//					{
		//						Id = data,
		//						Code = ESOBJ.Code,
		//						Name = ESOBJ.Name,                            
		//						DBTrack = ESOBJ.DBTrack
		//					};


		//					db.Level.Attach(OBJ);
		//					db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;

		//					// db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
		//					//db.SaveChanges();
		//					db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
		//					////   DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
		//					await db.SaveChangesAsync();
		//					//DisplayTrackedEntities(db.ChangeTracker);
		//					db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
		//					ts.Complete();
		//					return Json(new Object[] { OBJ.Id, OBJ.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                        
		//			}
		//		}
		//		else
		//		{
		//			StringBuilder sb = new StringBuilder("");
		//			foreach (ModelState modelState in ModelState.Values)
		//			{
		//				foreach (ModelError error in modelState.Errors)
		//				{
		//					sb.Append(error.ErrorMessage);
		//					sb.Append("." + "\n");
		//				}
		//			}
		//			var errorMsg = sb.ToString();
		//			return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
		//			//return this.Json(new { msg = errorMsg });
		//		}
		//	}
		//	catch (DbUpdateConcurrencyException e)
		//	{
		//		throw e;
		//	}
		//	catch (DbUpdateException e)
		//	{
		//		throw e;
		//	}
		//}

		[HttpPost]
		public async Task<ActionResult> EditSave(Level LV, int data, FormCollection form) // Edit submit
		{
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {
                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    Level blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.Level.Where(e => e.Id == data).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    LV.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                  //  int a = EditS(data, LV, LV.DBTrack);

                                    var CurLevel = db.Level.Find(data);
                                    TempData["CurrRowVersion"] = CurLevel.RowVersion;
                                    db.Entry(CurLevel).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                      //  LV.DBTrack = dbT;
                                        Level level = new Level()
                                        {
                                            Code = LV.Code,
                                            Name = LV.Name,
                                            Id = data,
                                            DBTrack = LV.DBTrack
                                        };


                                        db.Level.Attach(level);
                                        db.Entry(level).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(level).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                      //  return 1;
                                    }

                                    using (var context = new DataBaseContext())
                                    {

                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, LV.DBTrack);
                                        DT_Level DT_Level = (DT_Level)obj;
                                        db.Create(DT_Level);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = data, Val = LV.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { , , "Record Updated", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Corporate)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (Corporate)databaseEntry.ToObject();
                                    LV.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
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
                            //var errorMsg = sb.ToString();
                            //return this.Json(new Object[] { LV.Id, LV.Name, errorMsg, JsonRequestBehavior.AllowGet });
                            //return this.Json(new { msg = errorMsg });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            Level blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            Level Old_Level = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.Level.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            LV.DBTrack = new DBTrack
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

                            Level level = new Level()
                            {
                                Code = LV.Code,
                                Name = LV.Name,
                                Id = data,
                                DBTrack = LV.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, level, "Level", LV.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Level = context.Level.Where(e => e.Id == data).SingleOrDefault();
                                DT_Level DT_Level = (DT_Level)obj;

                                db.Create(DT_Level);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = LV.DBTrack;
                            db.Level.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = LV.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] {, , "Record Updated", JsonRequestBehavior.AllowGet });
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
         



        /*---------------------------------------------- Delete ------------------------------------------------- */

		[HttpPost]
		public async Task<ActionResult> Delete(int data)
		{
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    Level level = db.Level.Where(e => e.Id == data).SingleOrDefault();


                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (level.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = level.DBTrack.CreatedBy != null ? level.DBTrack.CreatedBy : null,
                                CreatedOn = level.DBTrack.CreatedOn != null ? level.DBTrack.CreatedOn : null,
                                IsModified = level.DBTrack.IsModified == true ? true : false
                            };
                            level.DBTrack = dbT;
                            db.Entry(level).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, level.DBTrack);
                            DT_Level DT_Level = (DT_Level)rtn_Obj;

                            db.Create(DT_Level);
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
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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
                                    CreatedBy = level.DBTrack.CreatedBy != null ? level.DBTrack.CreatedBy : null,
                                    CreatedOn = level.DBTrack.CreatedOn != null ? level.DBTrack.CreatedOn : null,
                                    IsModified = level.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(level).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                DT_Level DT_Level = (DT_Level)rtn_Obj;
                                db.Create(DT_Level);

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
                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

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
            }
		}

        public ActionResult DeleteLevel(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                var qurey = db.Level.Find(data);
                using (TransactionScope ts = new TransactionScope())
                {
                    db.Level.Attach(qurey);
                    db.Entry(qurey).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                    db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                    ts.Complete();
                }
                Msg.Add("  Record Deleted");
                return Json(new Utility.JsonReturnClass { Id = qurey.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //return Json(new Object[] { qurey.Id, "Record Deleted", JsonRequestBehavior.AllowGet });
            }
        }
        public ActionResult edit(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.LookupValue.Where(e => e.Id == data).ToList();
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }


		[HttpPost]
		public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
		{
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            Level AOBJ = db.Level.FirstOrDefault(e => e.Id == auth_id);

                            AOBJ.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = AOBJ.DBTrack.ModifiedBy != null ? AOBJ.DBTrack.ModifiedBy : null,
                                CreatedBy = AOBJ.DBTrack.CreatedBy != null ? AOBJ.DBTrack.CreatedBy : null,
                                CreatedOn = AOBJ.DBTrack.CreatedOn != null ? AOBJ.DBTrack.CreatedOn : null,
                                IsModified = AOBJ.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.Level.Attach(AOBJ);
                            db.Entry(AOBJ).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(AOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, AOBJ.DBTrack);
                            DT_Grade DT_OBJ = (DT_Grade)rtn_Obj;

                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "M", AOBJ, null, "Grade", AOBJ.DBTrack);
                            //}
                            db.Create(DT_OBJ);
                            ts.Complete();
                            //return Json(new Object[] { , , "Record Authorised", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = AOBJ.Id, Val = AOBJ.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else if (auth_action == "M")
                    {

                        Level Old_OBJ = db.Level.Where(e => e.Id == auth_id).SingleOrDefault();

                        DT_Level Curr_OBJ = db.DT_Level
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        Level MOBJ = new Level();

                        MOBJ.Name = Curr_OBJ.Name == null ? Old_OBJ.Name : Curr_OBJ.Name;
                        MOBJ.Code = Curr_OBJ.Code == null ? Old_OBJ.Code : Curr_OBJ.Code;

                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    // db.Configuration.AutoDetectChangesEnabled = false;
                                    MOBJ.DBTrack = new DBTrack
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

                                  //  int a = EditS(auth_id, MOBJ, MOBJ.DBTrack);

                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = MOBJ.Id, Val = MOBJ.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return Json(new Object[] { , , "Record Updated", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Level)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (Grade)databaseEntry.ToObject();
                                    MOBJ.RowVersion = databaseValues.RowVersion;
                                }
                            }

                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Grade OBJ = db.Grade.Find(auth_id);
                            Level DOBJ = db.Level.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);
                            // Level Var1 = DOBJ.Levels.ToLookup();

                            DOBJ.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = DOBJ.DBTrack.ModifiedBy != null ? DOBJ.DBTrack.ModifiedBy : null,
                                CreatedBy = DOBJ.DBTrack.CreatedBy != null ? DOBJ.DBTrack.CreatedBy : null,
                                CreatedOn = DOBJ.DBTrack.CreatedOn != null ? DOBJ.DBTrack.CreatedOn : null,
                                IsModified = DOBJ.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.Level.Attach(DOBJ);
                            db.Entry(DOBJ).State = System.Data.Entity.EntityState.Deleted;
                            await db.SaveChangesAsync();



                            db.Entry(DOBJ).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record Authorised", JsonRequestBehavior.AllowGet });
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

        /*---------------------------------------------- Grid ------------------------------------------------- */
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
                var LKVal = db.Level.ToList();

                if (gp.IsAutho == true)
                {
                    LKVal = db.Level.AsNoTracking().Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    LKVal = db.Level.AsNoTracking().ToList(); 
                }

                IEnumerable<Level> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = LKVal;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                              || (e.Name.ToUpper().Contains(gp.searchString.ToUpper()))
                              || (e.Id.ToString().Contains(gp.searchString.ToUpper()))
                              ).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
                        //jsonData = IE.Select(a => new { a.Code, a.Name, a.Id }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Code.ToLower() == gp.searchString.ToLower()) || (e.Name.ToLower() == gp.searchString.ToLower()))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {

                    IE = LKVal;
                    Func<Level, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.Code :
                                         gp.sidx == "Name" ? c.Name :

                                         "");
                    }
                    //Func<Level, dynamic> orderfuc;
                    //if (gp.sidx == "Id")
                    //{
                    //    orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    //}
                    //else
                    //{
                    //    orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    //}



                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {

                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
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
                    total = totalPages
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}