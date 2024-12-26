///
/// Created by Kapil
///

using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Transactions;
using Training;
using System.Text;
using P2BUltimate.App_Start;
using P2b.Global;
using P2BUltimate.Models;
using System.Threading.Tasks;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Training.MainController
{
    public class CreateEmployeeController : Controller
    {
        public ActionResult Index1()
        {
            return View("~/Views/Shared/_Session.cshtml");
        }

        public ActionResult CreateFacultyExternal_partial()
        {
            return View("~/Views/Shared/Training/_FacultyInternalExternal.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Training/_CreateEmployee.cshtml");
        }

        //private DataBaseContext db = new DataBaseContext();

        // GET: /TrainingSession/
        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/TrainingSession/Index.cshtml");
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
                IEnumerable<TrainingSession> TrainingSession = null;
                if (gp.IsAutho == true)
                {
                    TrainingSession = db.TrainingSession.Include(e => e.SessionType).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    TrainingSession = db.TrainingSession.Include(e => e.SessionType).AsNoTracking().ToList();
                }

                IEnumerable<TrainingSession> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = TrainingSession;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.SessionDate, a.StartTime }).Where((e => (e.Id.ToString() == gp.searchString))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.SessionDate), Convert.ToString(a.StartTime), Convert.ToString(a.SessionType) != null ? Convert.ToString(a.SessionType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.SessionDate, a.StartTime, a.SessionType != null ? Convert.ToString(a.SessionType.LookupVal) : "" }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = TrainingSession;
                    Func<TrainingSession, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "SessionDate" ? Convert.ToString(c.SessionDate) :
                                         gp.sidx == "StartTime" ? Convert.ToString(c.StartTime) :
                                         gp.sidx == "SessionType" ? c.SessionType.LookupVal : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id,a.SessionDate.Value.ToShortDateString(), Convert.ToString(a.StartTime), Convert.ToString(a.EndTime), a.SessionType != null ? Convert.ToString(a.SessionType.LookupVal) : "" }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id,a.SessionDate.Value.ToShortDateString(), Convert.ToString(a.StartTime), Convert.ToString(a.EndTime), a.SessionType != null ? Convert.ToString(a.SessionType.LookupVal) : "" }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.SessionDate.Value.ToShortDateString(), a.StartTime, Convert.ToString(a.EndTime), a.SessionType != null ? Convert.ToString(a.SessionType.LookupVal) : "" }).ToList();
                    }
                    totalRecords = TrainingSession.Count();
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




        /*---------------------------------------------------------- Create ---------------------------------------------- */

        [HttpPost]
        public ActionResult Create(TrainingSession c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    string Category = form["Categorylist1"] == "0" ? "" : form["Categorylist1"];
                    string FactExte = form["Faculty_List"] == "0" ? "" : form["Faculty_List"];
                    string tothrs = form["TotalHours"] == "0" ? "" : form["TotalHours"];
                    if (tothrs != null)
                    {

                        c.TotalHours = tothrs.ToString();
                    }

                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Category));
                            c.SessionType = val;
                        }
                    }

                    if (FactExte != null)
                    {
                        if (FactExte != "")
                        {
                            int FactId = Convert.ToInt32(FactExte);
                            var vals = db.FacultyInternalExternal.Where(e => e.Id == FactId).SingleOrDefault();
                            c.Faculty = vals;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            TrainingSession trnsession = new TrainingSession()
                            {
                                EndTime = c.EndTime,
                                Faculty = c.Faculty,
                                SessionDate = c.SessionDate,
                                SessionType = c.SessionType,
                                StartTime = c.StartTime,
                                TotalHours = c.TotalHours,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.TrainingSession.Add(trnsession);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, c.DBTrack);
                                DT_TrainingSession DT_Corp = (DT_TrainingSession)rtn_Obj;
                                DT_Corp.Faculty_Id = c.Faculty == null ? 0 : c.Faculty.Id;
                                DT_Corp.SessionType_Id = c.SessionType == null ? 0 : c.SessionType.Id;
                                db.Create(DT_Corp);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = trnsession.Id, Val = trnsession.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


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
                var Q = db.TrainingSession
                    .Include(e => e.SessionType)
                    .Include(e => e.Faculty)
                      .Where(e => e.Id == data).Select
                    (e => new
                    {
                        EndTime = e.EndTime,
                        SessionDate = e.SessionDate,
                        StartTime = e.StartTime,
                        TotalHours = e.TotalHours,
                        SessionType_Id = e.SessionType.Id == null ? 0 : e.SessionType.Id,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.TrainingSession
                    .Include(e => e.SessionType)
                    .Include(e => e.Faculty)
                    .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        Faculty_Details = e.Faculty.FullDetails == null ? "" : e.Faculty.FullDetails,
                        Faculty_Id = e.Faculty.Id == null ? "" : e.Faculty.Id.ToString(),
                    }).ToList();


                var W = db.DT_TrainingSession
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         EndTime = e.EndTime,
                         SessionDate = e.SessionDate,
                         StartTime = e.StartTime,
                         TotalHours = e.TotalHours,
                         SessionType_Val = e.SessionType_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.SessionType_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),

                         //Faculty_Val = e.Faculty_Id == 0 ? "" : db.FacultyExternal.Where(x => x.Id == e.Faculty_Id).Select(x => x.FullDetails).FirstOrDefault(),
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.TrainingSession.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        //[HttpPost]
        //public async Task<ActionResult> EditSave(TrainingSession c, int data, FormCollection form) // Edit submit
        //{
        //    string Category = form["Categorylist1"] == "0" ? "" : form["Categorylist1"];
        //    string FactExte = form["Faculty_List"] == "0" ? "" : form["Faculty_List"];
        //    bool Auth = form["Autho_Allow"] == "true" ? true : false;

        //    if (Category != null)
        //    {
        //        if (Category != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(Category));
        //            c.SessionType = val;
        //        }
        //    }

        //    if (FactExte != null)
        //    {
        //        if (FactExte != "")
        //        {
        //            int ContId = Convert.ToInt32(FactExte);
        //            var val = db.FacultyExternal
        //                                .Where(e => e.Id == ContId).SingleOrDefault();
        //            c.Faculty = val;
        //        }
        //    }


        //    if (Auth == false)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            try
        //            {

        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    TrainingSession blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.TrainingSession.Where(e => e.Id == data).Include(e => e.SessionType)
        //                                                .Include(e => e.Faculty)
        //                                                .SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }

        //                    c.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };

        //                    int a = EditS(Category, FactExte, data, c, c.DBTrack);

        //                    using (var context = new DataBaseContext())
        //                    {

        //                        var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                        DT_TrainingSession DT_Corp = (DT_TrainingSession)obj;
        //                        DT_Corp.Faculty_Id = blog.Faculty == null ? 0 : blog.Faculty.Id;
        //                        DT_Corp.SessionType_Id = blog.SessionType == null ? 0 : blog.SessionType.Id;
        //                        var DT_VAL=Convert.ToString(DT_Corp);
        //                        db.Create(DT_VAL);
        //                        db.SaveChanges();
        //                    }
        //                    await db.SaveChangesAsync();
        //                    ts.Complete();


        //                    return Json(new Object[] { c.Id, null, "Record Updated", JsonRequestBehavior.AllowGet });

        //                }
        //            }
        //            catch (DbUpdateConcurrencyException ex)
        //            {
        //                var entry = ex.Entries.Single();
        //                var clientValues = (TrainingSession)entry.Entity;
        //                var databaseEntry = entry.GetDatabaseValues();
        //                if (databaseEntry == null)
        //                {
        //                    return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                }
        //                else
        //                {
        //                    var databaseValues = (TrainingSession)databaseEntry.ToObject();
        //                    c.RowVersion = databaseValues.RowVersion;

        //                }
        //            }

        //            return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //        }
        //    }
        //    else
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {

        //            TrainingSession blog = null; // to retrieve old data
        //            DbPropertyValues originalBlogValues = null;
        //            TrainingSession Old_Corp = null;

        //            using (var context = new DataBaseContext())
        //            {
        //                blog = context.TrainingSession.Where(e => e.Id == data).SingleOrDefault();
        //                originalBlogValues = context.Entry(blog).OriginalValues;
        //            }
        //            c.DBTrack = new DBTrack
        //            {
        //                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                Action = "M",
        //                IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                ModifiedBy = SessionManager.UserName,
        //                ModifiedOn = DateTime.Now
        //            };
        //            TrainingSession corp = new TrainingSession()
        //            {
        //                EndTime = c.EndTime,
        //                SessionDate =c.SessionDate,
        //                StartTime = c.StartTime,
        //                TotalHours = c.TotalHours,
        //                Id = data,
        //                DBTrack = c.DBTrack,
        //                RowVersion = (Byte[])TempData["RowVersion"]
        //            };

        //            using (var context = new DataBaseContext())
        //            {
        //                var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "TrainingSession", c.DBTrack);
        //                Old_Corp = context.TrainingSession.Where(e => e.Id == data).Include(e => e.SessionType)
        //                    .Include(e => e.Faculty).SingleOrDefault();
        //                DT_TrainingSession DT_Corp = (DT_TrainingSession)obj;
        //                DT_Corp.Faculty_Id = DBTrackFile.ValCompare(Old_Corp.Faculty, c.Faculty);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                DT_Corp.SessionType_Id = DBTrackFile.ValCompare(Old_Corp.SessionType, c.SessionType); //Old_Corp.SessionType == c.SessionType ? 0 : Old_Corp.SessionType == null && c.SessionType != null ? c.SessionType.Id : Old_Corp.SessionType.Id;
        //                db.Create(DT_Corp);
        //            }
        //            blog.DBTrack = c.DBTrack;
        //            db.TrainingSession.Attach(blog);
        //            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            db.SaveChanges();
        //            ts.Complete();
        //            return Json(new Object[] { blog.Id, null, "Record Updated", JsonRequestBehavior.AllowGet });
        //        }

        //    }
        //    return View();
        //}

        [HttpPost]
        public async Task<ActionResult> EditSave(TrainingSession c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string Category = form["Categorylist1"] == "0" ? "" : form["Categorylist1"];
                    string FactExte = form["Faculty_List"] == "0" ? "" : form["Faculty_List"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Category));
                            c.SessionType = val;
                        }
                    }

                    if (FactExte != null)
                    {
                        if (FactExte != "")
                        {
                            int ContId = Convert.ToInt32(FactExte);
                            var val = db.FacultyInternalExternal
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            c.Faculty = val;
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
                                    TrainingSession blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.TrainingSession.Where(e => e.Id == data).Include(e => e.Faculty)
                                                                .Include(e => e.SessionType)
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

                                  // int a = EditS(Category, FactExte, data, c, c.DBTrack);

                                    if (Category != null)
                                    {
                                        if (Category != "")
                                        {
                                            var val = db.LookupValue.Find(int.Parse(Category));
                                            c.SessionType = val;

                                            var type = db.TrainingSession.Include(e => e.SessionType).Where(e => e.Id == data).SingleOrDefault();
                                            IList<TrainingSession> typedetails = null;
                                            if (type.SessionType != null)
                                            {
                                                typedetails = db.TrainingSession.Where(x => x.SessionType.Id == type.SessionType.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.TrainingSession.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.SessionType = c.SessionType;
                                                db.TrainingSession.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var BusiTypeDetails = db.TrainingSession.Include(e => e.SessionType).Where(x => x.Id == data).ToList();
                                            foreach (var s in BusiTypeDetails)
                                            {
                                                s.SessionType = null;
                                                db.TrainingSession.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var BusiTypeDetails = db.TrainingSession.Include(e => e.SessionType).Where(x => x.Id == data).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.SessionType = null;
                                            db.TrainingSession.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    if (FactExte != null)
                                    {
                                        if (FactExte != "")
                                        {
                                            var val = db.FacultyInternalExternal.Find(int.Parse(FactExte));
                                            c.Faculty = val;

                                            var add = db.TrainingSession.Include(e => e.Faculty).Where(e => e.Id == data).SingleOrDefault();
                                            IList<TrainingSession> addressdetails = null;
                                            if (add.Faculty != null)
                                            {
                                                addressdetails = db.TrainingSession.Where(x => x.Faculty.Id == add.Faculty.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                addressdetails = db.TrainingSession.Where(x => x.Id == data).ToList();
                                            }
                                            if (addressdetails != null)
                                            {
                                                foreach (var s in addressdetails)
                                                {
                                                    s.Faculty = c.Faculty;
                                                    db.TrainingSession.Attach(s);
                                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                    TempData["RowVersion"] = s.RowVersion;
                                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var addressdetails = db.TrainingSession.Include(e => e.Faculty).Where(x => x.Id == data).ToList();
                                        foreach (var s in addressdetails)
                                        {
                                            s.Faculty = null;
                                            db.TrainingSession.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    var CurCorp = db.TrainingSession.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        TrainingSession corp = new TrainingSession()
                                        {
                                            EndTime = c.EndTime,
                                            Faculty = c.Faculty,
                                            SessionDate = c.SessionDate,
                                            SessionType = c.SessionType,
                                            StartTime = c.StartTime,
                                            TotalHours = c.TotalHours,
                                            Id = data,
                                            DBTrack = c.DBTrack
                                        };


                                        db.TrainingSession.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    }

                                    using (var context = new DataBaseContext())
                                    {
                                        c.Id = data;

                                        /// DBTrackFile.DBTrackSave("Core/P2b.Global", "M", blog, c, "Corporate", c.DBTrack);
                                        //PropertyInfo[] fi = null;
                                        //Dictionary<string, object> rt = new Dictionary<string, object>();
                                        //fi = c.GetType().GetProperties();
                                        ////foreach (var Prop in fi)
                                        ////{
                                        ////    if (Prop.Name != "Id" && Prop.Name != "DBTrack" && Prop.Name != "RowVersion")
                                        ////    {
                                        ////        rt.Add(Prop.Name, Prop.GetValue(c));
                                        ////    }
                                        ////}
                                        //rt = blog.DetailedCompare(c);
                                        //rt.Add("Orig_Id", c.Id);
                                        //rt.Add("Action", "M");
                                        //rt.Add("DBTrack", c.DBTrack);
                                        //rt.Add("RowVersion", c.RowVersion);
                                        //var aa = DBTrackFile.GetInstance("Core/P2b.Global", "DT_" + "Corporate", rt);
                                        //DT_Corporate d = (DT_Corporate)aa;
                                        //db.DT_Corporate.Add(d);
                                        //db.SaveChanges();

                                        //To save data in history table 
                                        //var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, c, "Corporate", c.DBTrack);
                                        //DT_Corporate DT_Corp = (DT_Corporate)Obj;
                                        //db.DT_Corporate.Add(DT_Corp);
                                        //db.SaveChanges();\


                                        var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_TrainingSession DT_Corp = (DT_TrainingSession)obj;
                                        DT_Corp.Faculty_Id = blog.Faculty == null ? 0 : blog.Faculty.Id;
                                        DT_Corp.SessionType_Id = blog.SessionType == null ? 0 : blog.SessionType.Id;

                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();



                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (TrainingSession)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {

                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    var databaseValues = (TrainingSession)databaseEntry.ToObject();
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

                            TrainingSession blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            TrainingSession Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.TrainingSession.Where(e => e.Id == data).SingleOrDefault();
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

                            TrainingSession corp = new TrainingSession()
                            {
                                TotalHours = c.TotalHours,
                                StartTime = c.StartTime,
                                EndTime = c.EndTime,
                                SessionDate = c.SessionDate,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "Traiing Session", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.TrainingSession.Where(e => e.Id == data).Include(e => e.SessionType)
                                    .Include(e => e.Faculty).SingleOrDefault();
                                DT_TrainingSession DT_Corp = (DT_TrainingSession)obj;
                                DT_Corp.Faculty_Id = DBTrackFile.ValCompare(Old_Corp.Faculty, c.Faculty);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                DT_Corp.SessionType_Id = DBTrackFile.ValCompare(Old_Corp.SessionType, c.SessionType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;

                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.TrainingSession.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();

                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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


        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (auth_action == "C")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //TrainingSession corp = db.TrainingSession.Find(auth_id);
                        //TrainingSession corp = db.TrainingSession.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                        TrainingSession corp = db.TrainingSession
                            .Include(e => e.Faculty)
                            .Include(e => e.SessionType).FirstOrDefault(e => e.Id == auth_id);

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

                        db.TrainingSession.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //db.SaveChanges();
                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corp.DBTrack);
                        DT_TrainingSession DT_Corp = (DT_TrainingSession)rtn_Obj;
                        DT_Corp.SessionType_Id = corp.SessionType == null ? 0 : corp.SessionType.Id;
                        DT_Corp.Faculty_Id = corp.Faculty == null ? 0 : corp.Faculty.Id;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();

                        ts.Complete();
                        return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                    }
                }
                else if (auth_action == "M")
                {

                    TrainingSession Old_Corp = db.TrainingSession.Include(e => e.SessionType)
                                                      .Include(e => e.Faculty).Where(e => e.Id == auth_id).SingleOrDefault();

                    //var W = db.DT_TrainingSession
                    //.Include(e => e.Faculty)
                    //.Include(e => e.Address)
                    //.Include(e => e.SessionType)
                    //.Include(e => e.Faculty)
                    //.Where(e => e.Orig_Id == auth_id && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                    //(e => new
                    //{
                    //    DT_Id = e.Id,
                    //    Code = e.Code == null ? "" : e.Code,
                    //    Name = e.Name == null ? "" : e.Name,
                    //    SessionType_Val = e.SessionType.Id == null ? "" : e.SessionType.LookupVal,
                    //    Address_Val = e.Address.Id == null ? "" : e.Address.FullAddress,
                    //    Contact_Val = e.Faculty.Id == null ? "" : e.Faculty.FullFaculty,
                    //}).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                    DT_TrainingSession Curr_Corp = db.DT_TrainingSession
                                                .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                .OrderByDescending(e => e.Id)
                                                .FirstOrDefault();

                    if (Curr_Corp != null)
                    {
                        TrainingSession corp = new TrainingSession();

                        string Corp = Curr_Corp.SessionType_Id == null ? null : Curr_Corp.SessionType_Id.ToString();
                        string Faculty = Curr_Corp.Faculty_Id == null ? null : Curr_Corp.Faculty_Id.ToString();
                        corp.EndTime = Curr_Corp.EndTime == null ? Old_Corp.EndTime : Curr_Corp.EndTime;
                        corp.StartTime = Curr_Corp.StartTime == null ? Old_Corp.StartTime : Curr_Corp.StartTime;
                        corp.SessionDate = Curr_Corp.SessionDate == null ? Old_Corp.SessionDate : Curr_Corp.SessionDate;

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

                                    int a = EditS(Corp, Faculty, auth_id, corp, corp.DBTrack);
                                    //var CurCorp = db.TrainingSession.Find(auth_id);
                                    //TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    //db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    //{
                                    //    c.DBTrack = new DBTrack
                                    //    {
                                    //        CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                                    //        CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                                    //        Action = "M",
                                    //        ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
                                    //        ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
                                    //        AuthorizedBy = SessionManager.UserName,
                                    //        AuthorizedOn = DateTime.Now,
                                    //        IsModified = false
                                    //    };
                                    //    TrainingSession corp = new TrainingSession()
                                    //    {
                                    //        Code = c.Code,
                                    //        Name = c.Name,
                                    //        Id = Convert.ToInt32(auth_id),
                                    //        DBTrack = c.DBTrack
                                    //    };


                                    //    db.TrainingSession.Attach(corp);
                                    //    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;

                                    //    // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                    //    //db.SaveChanges();
                                    //    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    //    //// DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                    //    await db.SaveChangesAsync();
                                    //    //DisplayTrackedEntities(db.ChangeTracker);
                                    //    db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                                    //    ts.Complete();
                                    //    return Json(new Object[] { corp.Id, corp.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                    //}

                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (TrainingSession)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (TrainingSession)databaseEntry.ToObject();
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
                        //TrainingSession corp = db.TrainingSession.Find(auth_id);
                        TrainingSession corp = db.TrainingSession.AsNoTracking()
                                                                    .Include(e => e.SessionType)
                                                                    .Include(e => e.Faculty).FirstOrDefault(e => e.Id == auth_id);

                        FacultyInternalExternal conDet = corp.Faculty;
                        LookupValue val = corp.SessionType;

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

                        db.TrainingSession.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corp.DBTrack);
                        DT_TrainingSession DT_Corp = (DT_TrainingSession)rtn_Obj;
                        DT_Corp.SessionType_Id = corp.SessionType == null ? 0 : corp.SessionType.Id;
                        DT_Corp.Faculty_Id = corp.Faculty == null ? 0 : corp.Faculty.Id;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();
                        db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
                    }

                }
                return View();
            }
        }

        public int EditS(string Corp, string faculty, int data, TrainingSession c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Corp != null)
                {
                    if (Corp != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Corp));
                        c.SessionType = val;

                        var type = db.TrainingSession.Include(e => e.SessionType).Where(e => e.Id == data).SingleOrDefault();
                        IList<TrainingSession> typedetails = null;
                        if (type.SessionType != null)
                        {
                            typedetails = db.TrainingSession.Where(x => x.SessionType.Id == type.SessionType.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.TrainingSession.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.SessionType = c.SessionType;
                            db.TrainingSession.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.TrainingSession.Include(e => e.SessionType).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.SessionType = null;
                            db.TrainingSession.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var BusiTypeDetails = db.TrainingSession.Include(e => e.SessionType).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.SessionType = null;
                        db.TrainingSession.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (faculty != null)
                {
                    if (faculty != "")
                    {
                        var val = db.FacultyInternalExternal.Find(int.Parse(faculty));
                        c.Faculty = val;

                        var add = db.TrainingSession.Include(e => e.Faculty).Where(e => e.Id == data).SingleOrDefault();
                        IList<TrainingSession> addressdetails = null;
                        if (add.Faculty != null)
                        {
                            addressdetails = db.TrainingSession.Where(x => x.Faculty.Id == add.Faculty.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.TrainingSession.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.Faculty = c.Faculty;
                                db.TrainingSession.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                }
                else
                {
                    var addressdetails = db.TrainingSession.Include(e => e.Faculty).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.Faculty = null;
                        db.TrainingSession.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                var CurCorp = db.TrainingSession.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    TrainingSession corp = new TrainingSession()
                    {
                        EndTime = c.EndTime,
                        Faculty = c.Faculty,
                        SessionDate = c.SessionDate,
                        SessionType = c.SessionType,
                        StartTime = c.StartTime,
                        TotalHours = c.TotalHours,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.TrainingSession.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    return 1;
                }
                return 0;
            }
        }


        /* ---------------------------- GetLookup Faculty External -------------------------*/

        public ActionResult GetLookupDetailsFacultyExternal(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.FacultyInternalExternal.ToList();
                IEnumerable<FacultyInternalExternal> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.FacultyInternalExternal.ToList();

                }
                else
                {

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public void RollBack()
        {

            //  var context = DataContextFactory.GetDataContext();
            using (DataBaseContext db = new DataBaseContext())
            {
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

        //public string totalhours(string a,string b)
        //{
        //    var v = db.TrainingSession.Select(c => c.StartTime==a.StartTime).ToList();
        //}
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    TrainingSession TrainingSessions = db.TrainingSession
                                                       .Include(e => e.Faculty)
                                                       .Include(e => e.SessionType).Where(e => e.Id == data).SingleOrDefault();


                    LookupValue val = TrainingSessions.SessionType;
                    //var chkd = db.TrainingSchedule.Include(e => e.Session).ToList();
                    //foreach (var item in chkd)
                    //{
                    //    item..
                    //}
                    //TrainingSession TrainingSessions = db.TrainingSession.Where(e => e.Id == data).SingleOrDefault();
                    if (TrainingSessions.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, TrainingSessions.DBTrack, TrainingSessions, null, "TrainingSession");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = TrainingSessions.DBTrack.CreatedBy != null ? TrainingSessions.DBTrack.CreatedBy : null,
                                CreatedOn = TrainingSessions.DBTrack.CreatedOn != null ? TrainingSessions.DBTrack.CreatedOn : null,
                                IsModified = TrainingSessions.DBTrack.IsModified == true ? true : false
                            };
                            TrainingSessions.DBTrack = dbT;
                            db.Entry(TrainingSessions).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, TrainingSessions.DBTrack);
                            DT_TrainingSession DT_Corp = (DT_TrainingSession)rtn_Obj;
                            DT_Corp.SessionType_Id = TrainingSessions.SessionType == null ? 0 : TrainingSessions.SessionType.Id;
                            DT_Corp.Faculty_Id = TrainingSessions.Faculty == null ? 0 : TrainingSessions.Faculty.Id;
                            db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Training/Training", "D", TrainingSessions, null, "TrainingSession", TrainingSessions.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Training/Training", "D", TrainingSessions, null, "TrainingSession", TrainingSessions.DBTrack );
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
                                    CreatedBy = TrainingSessions.DBTrack.CreatedBy != null ? TrainingSessions.DBTrack.CreatedBy : null,
                                    CreatedOn = TrainingSessions.DBTrack.CreatedOn != null ? TrainingSessions.DBTrack.CreatedOn : null,
                                    IsModified = TrainingSessions.DBTrack.IsModified == true ? false : false//,
                                };

                                db.Entry(TrainingSessions).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, dbT);
                                DT_TrainingSession DT_Corp = (DT_TrainingSession)rtn_Obj;
                                DT_Corp.SessionType_Id = val == null ? 0 : val.Id;
                                db.Create(DT_Corp);
                                await db.SaveChangesAsync();
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