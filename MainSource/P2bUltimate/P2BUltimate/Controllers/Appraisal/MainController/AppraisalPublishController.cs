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
using System.Reflection;
using P2BUltimate.Security;
using Appraisal;

namespace P2BUltimate.Views.Appraisal.MainViews
{
    [AuthoriseManger]
    public class AppraisalPublishController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();

        //public ActionResult Index()
        //{
        //    return View("~/Views/Appraisal/MainViews/AppraisalPublish/Index.cshtml");
        //}

        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    List<string> Msg = new List<string>();
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        IEnumerable<AppraisalPublish> Corporate = null;
        //        if (gp.IsAutho == true)
        //        {
        //            Corporate = db.AppraisalPublish.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            Corporate = db.AppraisalPublish.AsNoTracking().ToList();
        //        }

        //        IEnumerable<AppraisalPublish> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = Corporate;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
        //                    || (e.SpanPeriod.ToString().Contains(gp.searchString))
        //                    || (e.Extension.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
        //                    ).Select(a => new { a.Id, a.SpanPeriod, a.Extension }).ToList();
        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.AppraisalPeriodCalendar.FromDate.Value.ToString("dd/MM/yyyy") + " - "+ a.AppraisalPeriodCalendar.ToDate.Value.ToString("dd/MM/yyyy"), a.PublishDate.Value.ToString("dd/MM/yyyy"), a.SpanPeriod, a.Extension, "" }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = Corporate;
        //            Func<AppraisalPublish, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "SpanPeriod" ? c. SpanPeriod :
        //                                 gp.sidx == "Extension" ? c.Extension  : 0);
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.SpanPeriod, a.Extension, 0 }).ToList();
        //                //jsonData = IE.Select(a => new Object[] { a.Id, a.AppraisalPeriodCalendar.FromDate.Value.ToString("dd/MM/yyyy") + " - " + a.AppraisalPeriodCalendar.ToDate.Value.ToString("dd/MM/yyyy"), a.PublishDate.Value.ToString("dd/MM/yyyy"), Convert.ToString(a.SpanPeriod), Convert.ToString(a.Extension), 0 }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.SpanPeriod, a.Extension, 0 }).ToList();
        //                // jsonData = IE.Select(a => new Object[] { a.Id, a.AppraisalPeriodCalendar.FromDate.Value.ToString("dd/MM/yyyy") + " - " + a.AppraisalPeriodCalendar.ToDate.Value.ToString("dd/MM/yyyy"), a.PublishDate.Value.ToString("dd/MM/yyyy"),  Convert.ToString(a.SpanPeriod), Convert.ToString(a.Extension), 0 }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.SpanPeriod, a.Extension, 0 }).ToList();
        //                // jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.AppraisalPeriodCalendar.FromDate.Value.ToString("dd/MM/yyyy") + " - " + a.AppraisalPeriodCalendar.ToDate.Value.ToString("dd/MM/yyyy"), a.PublishDate.Value.ToString("dd/MM/yyyy"),  a.SpanPeriod, a.Extension, 0 }).ToList();
        //            }
        //            totalRecords = Corporate.Count();
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
        //            total = totalPages
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //       LogFile Logfile = new LogFile();
        //        ErrorLog Err = new ErrorLog()
        //        {
        //            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //            ExceptionMessage = ex.Message,
        //            ExceptionStackTrace = ex.StackTrace,
        //            LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //            LogTime = DateTime.Now
        //        };
        //        Logfile.CreateLogFile(Err);
        //        Msg.Add(ex.Message);
        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //    }
        //}

        public ActionResult PopulateDropDownListCalendar(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).ToList();
                //  var qurey = db.Calendar.Include(e=>e.Name).ToList();
                var selected = "";
                if (!string.IsNullOrEmpty(data2))
                {
                    selected = data2;
                }
                var returndata = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Create(AppraisalPublish p, FormCollection form) //Create submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //string AppCalendardrop = form["AppCalendardrop"] == "0" ? null : form["AppCalendardrop"];
                    var IsTrClose = form["IsTrClose"] == "0" ? "" : form["IsTrClose"];

                    p.IsTrClose = Convert.ToBoolean(IsTrClose);

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.AppraisalPublish.Any(o => o.Id == p.Id))
                            {
                                Msg.Add("Code Already Exists.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            p.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };                     //???????

                            AppraisalPublish Appraisalpublish = new AppraisalPublish()
                            {
                               
                                PublishDate = p.PublishDate,
                                SpanPeriod = p.SpanPeriod == null ? 0 : p.SpanPeriod,
                                Extension = p.Extension == null ? 0 : p.Extension,
                                IsTrClose = p.IsTrClose,
                                Id = p.Id,
                                DBTrack = p.DBTrack
                            };

                            db.AppraisalPublish.Add(Appraisalpublish);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, p.DBTrack);

                            //DT_AppCategory DT_AppC = (DT_AppCategory)rtn_Obj;
                            //DT_AppC.AppMode_Id = p.AppMode == null ? 0 : p.AppMode.Id;
                            //db.Create(DT_AppC);
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("Data Saved Successfully.");
                            //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            return Json(new Utility.JsonReturnClass { Id = Appraisalpublish.Id, Val = Appraisalpublish.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                catch (Exception e)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = e.Message,
                        ExceptionStackTrace = e.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
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
                var Q = db.AppraisalPublish
                    //.Include(e => e.AppraisalPeriodCalendar)
                    .Where(e => e.Id == data).Select
                    (p => new
                    {
                        //AppraisalPeriodCalendar_Id = p.AppraisalPeriodCalendar != null ? p.AppraisalPeriodCalendar.Id : 0,
                        PublishDate = p.PublishDate,
                        SpanPeriod = p.SpanPeriod == null ? 0 : p.SpanPeriod,
                        Extension = p.Extension == null ? 0 : p.Extension,
                        IsTrClose = p.IsTrClose,
                        Id = p.Id,
                        Action = p.DBTrack.Action
                    }).ToList();

                var Corp = db.AppraisalPublish.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
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
                    AppraisalPublish corporates = db.AppraisalPublish
                        //.Include(e => e.AppraisalPeriodCalendar)
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
                            corporates.DBTrack = dbT;
                            db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, corporates.DBTrack);
                            //DT_AppraisalPublish DT_Corp = (DT_AppraisalPublish)rtn_Obj;
                            //DT_Corp.AppraisalPeriodCalendar_Id = corporates.AppraisalPeriodCalendar == null ? 0 : corporates.AppraisalPeriodCalendar.Id;
                            //db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            //Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {

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
                                //AuthorizedBy = SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                            db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, dbT);
                            //DT_AppraisalPublish DT_Corp = (DT_AppraisalPublish)rtn_Obj;
                            //DT_Corp.AppraisalPeriodCalendar_Id = corporates.AppraisalPeriodCalendar == null ? 0 : corporates.AppraisalPeriodCalendar.Id;
                            //db.Create(DT_Corp);

                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (RetryLimitExceededException /* dex */)
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

        [HttpPost]
        public async Task<ActionResult> EditSave(AppraisalPublish L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //string HoliCalendarDDL = form["AppCalendardrop"] == "0" ? null : form["AppCalendardrop"];
                    var IsTrClose = form["IsTrClose"] == "0" ? "" : form["IsTrClose"];

                    if (ModelState.IsValid)
                    {
                        try
                        {

                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                AppraisalPublish blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.AppraisalPublish.Where(e => e.Id == data)
                                        //.Include(e => e.AppraisalPeriodCalendar)
                                        .SingleOrDefault();
                                    originalBlogValues = context.Entry(blog).OriginalValues;
                                }

                                L.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };

                                //if (HoliCalendarDDL != null)
                                //{
                                //    if (HoliCalendarDDL != "")
                                //    {
                                //        var val = db.Calendar.Find(int.Parse(HoliCalendarDDL));
                                //        L.AppraisalPeriodCalendar = val;

                                //        var type = db.AppraisalPublish.Include(e => e.AppraisalPeriodCalendar).Where(e => e.Id == data).SingleOrDefault();
                                //        IList<AppraisalPublish> typedetails = null;
                                //        if (type.AppraisalPeriodCalendar != null)
                                //        {
                                //            typedetails = db.AppraisalPublish.Where(x => x.AppraisalPeriodCalendar.Id == type.AppraisalPeriodCalendar.Id && x.Id == data).ToList();
                                //        }
                                //        else
                                //        {
                                //            typedetails = db.AppraisalPublish.Where(x => x.Id == data).ToList();
                                //        }
                                //        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                //        foreach (var s in typedetails)
                                //        {
                                //            s.AppraisalPeriodCalendar = L.AppraisalPeriodCalendar;
                                //            db.AppraisalPublish.Attach(s);
                                //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //            //await db.SaveChangesAsync();
                                //            db.SaveChanges();
                                //            TempData["RowVersion"] = s.RowVersion;
                                //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                //        }
                                //    }
                                //    else
                                //    {
                                //        var WFTypeDetails = db.AppraisalPublish.Include(e => e.AppraisalPeriodCalendar).Where(x => x.Id == data).ToList();
                                //        foreach (var s in WFTypeDetails)
                                //        {
                                //            s.AppraisalPeriodCalendar = null;
                                //            db.AppraisalPublish.Attach(s);
                                //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //            //await db.SaveChangesAsync();
                                //            db.SaveChanges();
                                //            TempData["RowVersion"] = s.RowVersion;
                                //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                //        }
                                //    }
                                //}
                                //else
                                //{
                                //    var HoliCalendarDetails = db.AppraisalPublish.Include(e => e.AppraisalPeriodCalendar).Where(x => x.Id == data).ToList();
                                //    foreach (var s in HoliCalendarDetails)
                                //    {
                                //        //s.AppraisalPeriodCalendar = null;
                                //        s.AppraisalPeriodCalendar = L.AppraisalPeriodCalendar;
                                //        db.AppraisalPublish.Attach(s);
                                //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //        //await db.SaveChangesAsync();
                                //        db.SaveChanges();
                                //        TempData["RowVersion"] = s.RowVersion;
                                //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                //    }
                                //}

                                L.IsTrClose = Convert.ToBoolean(IsTrClose);
                                var CurCorp = db.AppraisalPublish.Find(data);
                                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {

                                    AppraisalPublish Appraisalpublish = new AppraisalPublish()
                                    {
                                        //AppraisalPeriodCalendar = L.AppraisalPeriodCalendar,
                                        PublishDate = L.PublishDate,
                                        SpanPeriod = L.SpanPeriod == null ? 0 : L.SpanPeriod,
                                        Extension = L.Extension == null ? 0 : L.Extension,
                                        IsTrClose = L.IsTrClose,
                                        Id = data,
                                        DBTrack = L.DBTrack
                                    };
                                    db.AppraisalPublish.Attach(Appraisalpublish);
                                    db.Entry(Appraisalpublish).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(Appraisalpublish).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                }
                                // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                using (var context = new DataBaseContext())
                                {
                                    // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                    //  dt_holiday DT_Corp = (DT_LvCreditPolicy)obj;
                                    //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;
                                    //db.Create(DT_Corp);
                                    db.SaveChanges();
                                }
                                await db.SaveChangesAsync();
                                var qurey = db.AppraisalPublish
                                    //.Include(e => e.AppraisalPeriodCalendar)
                                    .Where(e => e.Id == data).SingleOrDefault();

                                ts.Complete();
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = qurey.Id, Val = qurey.SpanPeriod.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (HolidayCalendar)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (HolidayCalendar)databaseEntry.ToObject();
                                L.RowVersion = databaseValues.RowVersion;

                            }
                        }
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
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
    }
}