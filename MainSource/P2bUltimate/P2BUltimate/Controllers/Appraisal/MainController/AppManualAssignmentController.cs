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


namespace P2BUltimate.Controllers.Appraisal.MainController
{
    public class AppManualAssignmentController : Controller
    {         
          //  private DataBaseContext db = new DataBaseContext();
            public ActionResult Index()
            {
                return View("~/Views/Appraisal/MainViews/AppManualAssignment/Index.cshtml");
            }

            [HttpPost]
            public async Task<ActionResult> Delete(int data)
            {

                List<string> Msg = new List<string>();
                using (DataBaseContext db = new DataBaseContext())
                {
                    try
                    {
                        AppManualAssignment corporates = db.AppManualAssignment
                     .Include(e => e.AppCategory)
                     .Include(e => e.AppSubCategory)
                     .Include(e => e.AppraisalCalendar)
                     .Include(e => e.AppRatingObjective)
                     .Where(e => e.Id == data).SingleOrDefault();

                        // FuncStruct fun = corporates.FuncStruct;
                        if (corporates.DBTrack.IsModified == true)
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
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
                                // DT_AppAssignment DT_Corp = (DT_AppAssignment)rtn_Obj;
                                // DT_Corp.  = corporates.  == null ? 0 : corporates.AppMode.Id;
                                // db.Create(DT_Corp);

                                //  await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Data removed.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            // var selectedJobP = Postdetails.FuncStruct;
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
                                var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, dbT);
                                DT_AppAssignment DT_Corp = (DT_AppAssignment)rtn_Obj;
                                //  DT_Corp.AppMode_Id = val == null ? 0 : val.Id;
                                db.Create(DT_Corp);

                                //await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Data removed.  ");                                                                                             // the original place 
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


            public ActionResult GetAppRatingObjectiveLKDetails(string data)
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var fall = db.AppRatingObjective.ToList();
                    IEnumerable<AppRatingObjective> all;
                    if (!string.IsNullOrEmpty(data))
                    {
                        all = db.AppRatingObjective.ToList().Where(d => d.Id.ToString().Contains(data));
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


            public ActionResult getCalendar()
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "AppraisalCalendar".ToUpper() && e.Default == true).AsEnumerable()
                        .Select(e => new
                        {
                            Id = e.Id,
                            Lvcalendardesc = "From Date :" + e.FromDate.Value.ToShortDateString() + " To Date :" + e.ToDate.Value.ToShortDateString(),

                        }).SingleOrDefault();
                    return Json(qurey, JsonRequestBehavior.AllowGet);
                }
            }


            public ActionResult GetCalendarDetailLKDetails(string data)
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var fall = db.Calendar.ToList();
                    IEnumerable<Calendar> all;
                    if (!string.IsNullOrEmpty(data))
                    {
                        all = db.Calendar.ToList().Where(d => d.Id.ToString().Contains(data));
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

            [HttpPost]
            public ActionResult Create(AppManualAssignment c, FormCollection form) //Create submit
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    string AppraisalCalendar = form["AppraisalCalendarlist"] == "0" ? "" : form["AppraisalCalendarlist"];
                    string AppCategory = form["AppCategorylist"] == "0" ? "" : form["AppCategorylist"];
                    string AppSubCategory = form["AppSubCategorylist"] == "0" ? "" : form["AppSubCategorylist"];
                    string AppRatingObjective = form["AppRatingObjectivelist"] == "0" ? "" : form["AppRatingObjectivelist"];
                    List<String> Msg = new List<String>();
                    try
                    {

                        if (AppraisalCalendar == null)
                        {
                            //  return this.Json(new Object[] { null, null, "Country cannot be null.", JsonRequestBehavior.AllowGet });
                            Msg.Add("AppraisalCalendar cannot be null.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        if (AppCategory != null)
                        {
                            if (AppCategory != "")
                            {
                                AppCategory App = db.AppCategory.Find(Convert.ToInt32(AppCategory));
                                c.AppCategory = App;
                            }
                        }

                        if (AppSubCategory != null)
                        {
                            if (AppSubCategory != "")
                            {
                                AppSubCategory AppSub = db.AppSubCategory.Find(Convert.ToInt32(AppSubCategory));
                                c.AppSubCategory = AppSub;
                            }
                        }

                        if (AppraisalCalendar != null)
                        {
                            if (AppraisalCalendar != "")
                            {
                                int AddId = Convert.ToInt32(AppraisalCalendar);
                                var val = db.Calendar.Include(e => e.Name).Where(e => e.Id == AddId).SingleOrDefault();
                                c.AppraisalCalendar = val;
                            }
                        }

                        c.AppRatingObjective = null;
                        List<AppRatingObjective> cp = new List<AppRatingObjective>();
                        if (AppRatingObjective != null && AppRatingObjective != "")
                        {
                            var ids = Utility.StringIdsToListIds(AppRatingObjective);
                            foreach (var ca in ids)
                            {
                                var p_val = db.AppRatingObjective.Find(ca);
                                cp.Add(p_val);
                                c.AppRatingObjective = cp;

                            }
                        }

                        if (ModelState.IsValid)
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                var v = db.AppCategoryRating.Include(a => a.AppCategory).Where(d => d.AppCategory.Id == c.AppCategory.Id).SingleOrDefault();
                                if (v != null)
                                {
                                    if (v.MaxRatingPoints < c.MaxRatingPoints)
                                    {
                                        Msg.Add("maximum rating points should not be greater than category max points");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }



                                if (db.AppManualAssignment.Any(o => o.Id == c.Id))
                                {
                                    Msg.Add("Code Already Exists.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }

                                c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                AppManualAssignment AppAssign = new AppManualAssignment()
                                {
                                    MaxRatingPoints = c.MaxRatingPoints == null ? 0 : c.MaxRatingPoints,
                                    AppraisalCalendar = c.AppraisalCalendar,
                                    AppSubCategory = c.AppSubCategory,
                                    AppRatingObjective = c.AppRatingObjective,
                                    AppCategory = c.AppCategory,
                                    DBTrack = c.DBTrack
                                };

                                db.AppManualAssignment.Add(AppAssign);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, c.DBTrack);
                                // DT_AppAssignment DT_Corp = (DT_AppAssignment)rtn_Obj;
                                //  DT_Corp.AppraisalCalendar_Id = c.AppraisalCalendar == null ? 0 : c.AppraisalCalendar.Id;
                                ////  DT_Corp.AppCategory_Id = c.AppCategory == null ? 0 : c.AppCategory.Id;
                                //  DT_Corp.AppSubCategory_Id = c.AppSubCategory == null ? 0 : c.AppSubCategory.Id;
                                //   db.Create(DT_Corp);
                                db.SaveChanges();
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);  

                                ts.Complete();
                                Msg.Add("Data Saved Successfully.");
                                return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.Id.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                            //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                            //return this.Json(new { msg = errorMsg });
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
                    IEnumerable<AppManualAssignment> Corporate = null;
                    if (gp.IsAutho == true)
                    {
                        Corporate = db.AppManualAssignment.Include(e => e.AppraisalCalendar)
                            .Include(e => e.AppCategory)
                            .Include(e => e.AppSubCategory)
                            .Include(e => e.AppRatingObjective).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                    }
                    else
                    {
                        Corporate = db.AppManualAssignment.Include(e => e.AppraisalCalendar)
                            .Include(e => e.AppCategory)
                            .Include(e => e.AppSubCategory)
                            .Include(e => e.AppRatingObjective).AsNoTracking().ToList();
                    }

                    IEnumerable<AppManualAssignment> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = Corporate;
                        if (gp.searchOper.Equals("eq"))
                        {
                           jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                              || (e.MaxRatingPoints.ToString().Contains(gp.searchString))
                              || (e.AppCategory.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                              ).Select(a => new { a.Id, a.MaxRatingPoints, a.AppCategory }).ToList();
                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.MaxRatingPoints, a.AppCategory, "" }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = Corporate;
                        Func<AppManualAssignment, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "MaxRatingPoints" ? c.MaxRatingPoints.ToString() :
                                             gp.sidx == "AppCategory" ? c.AppCategory.ToString() : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.MaxRatingPoints), Convert.ToString(a.AppCategory), "" }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.MaxRatingPoints), Convert.ToString(a.AppCategory), "" }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.MaxRatingPoints, a.AppCategory, "" }).ToList();
                        }
                        totalRecords = Corporate.Count();
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


            public class IsRatingObjectiveDetailsC
            {
                public Array AppRatingObjective_Id { get; set; }
                public Array IsRatingObjective_FullDetails { get; set; }
                public Array AppraisalCalendar_Id { get; set; }
                public string AppraisalCalendar_FullDetails { get; set; }
            }

            [HttpPost]
            public ActionResult Edit(int data)
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    List<IsRatingObjectiveDetailsC> return_data = new List<IsRatingObjectiveDetailsC>();
                    var lookup = db.AppManualAssignment.Include(e => e.AppraisalCalendar)
                                        .Include(e => e.AppCategory)
                                        .Include(e => e.AppSubCategory)
                                        .Include(e => e.AppRatingObjective)
                     .Where(e => e.Id == data).ToList();
                    var r = (from e in lookup
                             select new
                             {
                                 MaxRatingPoints = e.MaxRatingPoints,
                                 AppraisalCalendar_Id = e.AppraisalCalendar == null ? "" : e.AppraisalCalendar.Id.ToString(),
                                 AppraisalCalendar_FullDetails = e.AppraisalCalendar.FullDetails.ToString(),
                                 AppCategory_Id = e.AppCategory == null ? 0 : e.AppCategory.Id,
                                 AppSubCategory_Id = e.AppSubCategory == null ? 0 : e.AppSubCategory.Id,
                                 //    AppRatingObjective_Id = e.AppRatingObjective,
                                 Action = e.DBTrack.Action
                             }).Distinct();

                    var a = db.AppManualAssignment.Include(e => e.AppraisalCalendar)
                                        .Include(e => e.AppCategory)
                                        .Include(e => e.AppSubCategory)
                     .Where(e => e.Id == data).ToList();
                    foreach (var ca in a)
                    {
                        return_data.Add(new IsRatingObjectiveDetailsC
                        {
                            AppRatingObjective_Id = ca.AppRatingObjective.Select(e => e.Id.ToString()).ToArray(),
                            IsRatingObjective_FullDetails = ca.AppRatingObjective.Select(e => e.FullDetails).ToArray(),
                            //AppraisalCalendar_Id = ca.AppraisalCalendar.Id.ToString().ToArray(),
                            //AppraisalCalendar_FullDetails = ca.AppraisalCalendar.FullDetails.ToString()


                        });
                    }
                    var LKup = db.AppManualAssignment.Find(data);
                    TempData["RowVersion"] = LKup.RowVersion;
                    var Auth = LKup.DBTrack.IsModified;

                    return this.Json(new Object[] { r, return_data, "", Auth, JsonRequestBehavior.AllowGet });
                }
            }

          
            [HttpPost]
            public async Task<ActionResult> EditSave(AppManualAssignment add, int data, FormCollection form) // Edit submit
            {
                List<string> Msg = new List<string>();
                using (DataBaseContext db = new DataBaseContext())
                {
                    try
                    {
                        string AppraisalCalendarp = form["AppraisalCalendarlist"] == "0" ? "" : form["AppraisalCalendarlist"];
                        // string AppCategory = form["AppCategorylist"] == "0" ? "" : form["AppCategorylist"];
                        // string AppSubCategory = form["AppSubCategorylist"] == "0" ? "" : form["AppSubCategorylist"];
                        string AppRatingObjective = form["AppRatingObjectivelist"] == "0" ? "" : form["AppRatingObjectivelist"];
                        string geo_id = form["geo_id"] == "0" ? "" : form["geo_id"];
                        string fun_id = form["fun_id"] == "0" ? "" : form["fun_id"];
                        string pay_id = form["pay_id"] == "0" ? "" : form["pay_id"];
                        bool Auth = form["Autho_Allow"] == "true" ? true : false;
                        try
                        {
                            var AppraisalCalendar = AppraisalCalendarp.Replace(",", string.Empty);

                            if (AppraisalCalendar == null)
                            {
                                //  return this.Json(new Object[] { null, null, "Country cannot be null.", JsonRequestBehavior.AllowGet });
                                Msg.Add("AppraisalCalendar cannot be null.");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            var predata = db.AppManualAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Where(e => e.Id == data).SingleOrDefault();
                            add.AppCategory = predata.AppCategory;
                            add.AppSubCategory = predata.AppSubCategory;
                            string AppCategory = predata.AppSubCategory.Id.ToString();
                            string AppSubCategory = predata.AppSubCategory.Id.ToString();

                            //if (AppCategory != null && AppCategory != "" && AppCategory != "0")
                            //{
                            //    var val = db.AppCategory.Find(int.Parse(AppCategory));
                            //    add.AppCategory = val;
                            //}

                            //if (AppSubCategory != null && AppSubCategory != "" && AppSubCategory != "0")
                            //{
                            //    var val = db.AppSubCategory.Find(int.Parse(AppSubCategory));
                            //    add.AppSubCategory = val;
                            //}

                            List<AppRatingObjective> ObjITsection = new List<AppRatingObjective>();
                            AppRatingObjective pd = null;
                            pd = db.AppRatingObjective.Include(e => e.ObjectiveWordings).Where(e => e.Id == data).SingleOrDefault();
                            if (AppRatingObjective != null && AppRatingObjective != "")
                            {
                                var ids = Utility.StringIdsToListIds(AppRatingObjective);
                                foreach (var ca in ids)
                                {
                                    var value = db.AppRatingObjective.Find(ca);
                                    ObjITsection.Add(value);
                                    add.AppRatingObjective = ObjITsection;

                                }
                            }
                            else
                            {
                                add.AppRatingObjective = null;
                            }


                            if (AppraisalCalendar != null && AppraisalCalendar != "")
                            {
                                int ContId = Convert.ToInt32(AppraisalCalendar);
                                var val = db.Calendar.Where(e => e.Id == ContId).SingleOrDefault();
                                add.AppraisalCalendar = val;

                            }
                            if (Auth == false)
                            {
                                if (ModelState.IsValid)
                                {
                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        AppManualAssignment blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.AppManualAssignment.Where(e => e.Id == data).Include(e => e.AppCategory)
                                        .Include(e => e.AppSubCategory).Include(e => e.AppRatingObjective).SingleOrDefault();
                                            originalBlogValues = context.Entry(blog).OriginalValues;
                                        }

                                        add.DBTrack = new DBTrack
                                        {
                                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };

                                        int a = EditS(AppraisalCalendar, AppCategory, AppSubCategory, AppRatingObjective, data, add, add.DBTrack);



                                        //using (var context = new DataBaseContext())
                                        //{
                                        //    var obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", originalBlogValues, db.ChangeTracker, add.DBTrack);
                                        //DT_AppAssignment DT_Addrs = (DT_AppAssignment)obj;
                                        //DT_Addrs.AppCategory_Id = blog.AppCategory == null ? 0 : blog.AppCategory.Id;
                                        //DT_Addrs.AppSubCategory_Id = blog.AppSubCategory == null ? 0 : blog.AppSubCategory.Id;
                                        //DT_Addrs.AppraisalCalendar_Id = blog.AppraisalCalendar == null ? 0 : blog.AppraisalCalendar.Id;
                                        //DT_Addrs.MaxRatingPoints = blog.MaxRatingPoints == null ? 0 : blog.MaxRatingPoints;
                                        //db.Create(DT_Addrs);
                                        //  }
                                        db.SaveChanges();
                                        await db.SaveChangesAsync();
                                        //var query = db.AppManualAssignment.Where(e => e.Id == data).Include(e => e.AppCategory)
                                        //.Include(e => e.AppSubCategory).Include(e => e.AppRatingObjective).SingleOrDefault();
                                        ts.Complete();
                                        // string FullAddress = Session["FullAddress"].ToString();
                                        Msg.Add("Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = blog.Id.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        //    return Json(new { blog.Id, query.FullAddress, , JsonRequestBehavior.AllowGet });
                                    }


                                    //  return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                                }
                            }
                            else
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {

                                    AppManualAssignment blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;
                                    AppManualAssignment Old_Addrs = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.AppManualAssignment.Where(e => e.Id == data).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }
                                    add.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        IsModified = blog.DBTrack.IsModified == true ? true : false,
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    AppManualAssignment L = new AppManualAssignment()
                                    {
                                        MaxRatingPoints = add.MaxRatingPoints == null ? 0 : add.MaxRatingPoints,
                                        AppraisalCalendar = add.AppraisalCalendar,
                                        AppSubCategory = add.AppSubCategory,
                                        AppRatingObjective = add.AppRatingObjective,
                                        AppCategory = add.AppCategory,
                                        DBTrack = add.DBTrack,
                                        Id = data
                                    };


                                    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.ModifiedDataHistory("Appraisal/Appraisal", "M", blog, add, "AppManualAssignment", add.DBTrack);
                                        // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                        Old_Addrs = context.AppManualAssignment.Where(e => e.Id == data).Include(e => e.AppCategory)
                                        .Include(e => e.AppSubCategory).Include(e => e.AppRatingObjective).SingleOrDefault();
                                        DT_AppAssignment DT_Addrs = (DT_AppAssignment)obj;
                                        DT_Addrs.AppCategory_Id = DBTrackFile.ValCompare(Old_Addrs.AppCategory, add.AppCategory);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                        DT_Addrs.AppraisalCalendar_Id = DBTrackFile.ValCompare(Old_Addrs.AppraisalCalendar, add.AppraisalCalendar); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                        DT_Addrs.AppSubCategory_Id = DBTrackFile.ValCompare(Old_Addrs.AppSubCategory, add.AppSubCategory);
                                        db.Create(DT_Addrs);
                                        //db.SaveChanges();
                                    }
                                    blog.DBTrack = add.DBTrack;
                                    db.AppManualAssignment.Attach(blog);
                                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    db.SaveChanges();
                                    ts.Complete();
                                    //  return Json(new Object[] { blog.Id, add.FullAddress, "Record Updated", JsonRequestBehavior.AllowGet });
                                    Msg.Add("Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = add.Id.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }

                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (AppManualAssignment)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(ex.Message);
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var databaseValues = (AppManualAssignment)databaseEntry.ToObject();
                                add.RowVersion = databaseValues.RowVersion;

                            }
                        }
                        catch (Exception e)
                        {
                            Msg.Add(e.Message);
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
            public int EditS(string AppraisalCalendar, string AppCategory, string AppSubCategory, string AppRatingObjective, int data, AppManualAssignment c, DBTrack dbT)
            {
                IList<AppManualAssignment> typedetails = null;
                using (DataBaseContext db = new DataBaseContext())
                {
                    if (AppCategory != null)
                    {
                        if (AppCategory != "")
                        {
                            var val = db.AppCategory.Find(int.Parse(AppCategory));
                            c.AppCategory = val;

                            var add = db.AppManualAssignment.Include(e => e.AppCategory).Where(e => e.Id == data).SingleOrDefault();
                            IList<AppManualAssignment> contactsdetails = null;
                            if (add.AppCategory != null)
                            {
                                contactsdetails = db.AppManualAssignment.Where(x => x.AppCategory.Id == add.AppCategory.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                contactsdetails = db.AppManualAssignment.Where(x => x.Id == data).ToList();
                            }
                            foreach (var s in contactsdetails)
                            {
                                s.AppCategory = c.AppCategory;
                                db.AppManualAssignment.Attach(s);
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
                        var contactsdetails = db.AppManualAssignment.Include(e => e.AppCategory).Where(x => x.Id == data).ToList();
                        foreach (var s in contactsdetails)
                        {
                            s.AppCategory = null;
                            db.AppManualAssignment.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }

                    if (AppSubCategory != null && AppSubCategory != "")
                    {
                        var val = db.AppSubCategory.Find(int.Parse(AppSubCategory));
                        c.AppSubCategory = val;

                        var type = db.AppManualAssignment.Include(e => e.AppSubCategory).Where(e => e.Id == data).SingleOrDefault();
                        if (type.AppSubCategory != null)
                        {
                            typedetails = db.AppManualAssignment.Where(x => x.AppSubCategory.Id == type.AppSubCategory.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.AppManualAssignment.Where(x => x.Id == data).ToList();
                        }

                        foreach (var s in typedetails)
                        {
                            s.AppSubCategory = c.AppSubCategory;
                            db.AppManualAssignment.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.AppManualAssignment.Include(e => e.AppSubCategory).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.AppSubCategory = null;
                            db.AppManualAssignment.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }

                    if (AppraisalCalendar != null)
                    {
                        if (AppraisalCalendar != "")
                        {
                            var val = db.Calendar.Find(int.Parse(AppraisalCalendar));
                            c.AppraisalCalendar = val;

                            var add = db.AppManualAssignment.Include(e => e.AppraisalCalendar).Where(e => e.Id == data).SingleOrDefault();
                            IList<AppManualAssignment> contactsdetails = null;
                            if (add.AppraisalCalendar != null)
                            {
                                contactsdetails = db.AppManualAssignment.Where(x => x.AppraisalCalendar.Id == add.AppraisalCalendar.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                contactsdetails = db.AppManualAssignment.Where(x => x.Id == data).ToList();
                            }
                            foreach (var s in contactsdetails)
                            {
                                s.AppraisalCalendar = c.AppraisalCalendar;
                                db.AppManualAssignment.Attach(s);
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
                        var contactsdetails = db.AppManualAssignment.Include(e => e.AppraisalCalendar).Where(x => x.Id == data).ToList();
                        foreach (var s in contactsdetails)
                        {
                            s.AppraisalCalendar = null;
                            db.AppManualAssignment.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }

                    List<AppRatingObjective> ObjiProd = new List<AppRatingObjective>();
                    AppAssignment insuranceProduct = null;
                    insuranceProduct = db.AppAssignment.Include(e => e.AppRatingObjective).Where(e => e.Id == data).SingleOrDefault();
                    if (AppRatingObjective != null && AppRatingObjective != "")
                    {
                        var ids = Utility.StringIdsToListIds(AppRatingObjective);
                        foreach (var ca in ids)
                        {
                            var value = db.AppRatingObjective.Find(ca);
                            ObjiProd.Add(value);
                            c.AppRatingObjective = ObjiProd;
                        }
                    }
                    else
                    {
                        insuranceProduct.AppRatingObjective = null;
                    }

                    var CurCorp = db.AppManualAssignment.Find(data);
                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                    {
                        c.DBTrack = dbT;
                        AppManualAssignment corp = new AppManualAssignment()
                        {
                            MaxRatingPoints = c.MaxRatingPoints == null ? 0 : c.MaxRatingPoints,
                            AppraisalCalendar = c.AppraisalCalendar,
                            AppSubCategory = c.AppSubCategory,
                            AppRatingObjective = c.AppRatingObjective,
                            AppCategory = c.AppCategory,
                            Id = data,
                            DBTrack = c.DBTrack
                        };


                        db.AppManualAssignment.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                        return 1;
                    }
                    return 0;
                }
            }


            public ActionResult GetFullDetailsGeo(FormCollection form)
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    string geo_id = form["geo_id"];
                    string corporate_ids = form["corporate-table"];
                    string region = form["region-table"];
                    string company = form["company-table"];
                    var Division = form["division-table"];
                    var Location = form["location-table"];
                    var Department = form["department-table"];
                    var group = form["group-table"];
                    var unit = form["unit-table"];

                    var getData = db.GeoStruct.Include(e => e.Location)
                        .Include(e => e.Region)
                        .Include(e => e.Unit)
                        .Include(e => e.Company)
                        .Include(e => e.Corporate)
                        .Include(e => e.Department)
                        //.Include(e => e.Location)
                        .Where(e => e.Id != null).ToList();

                    if (Division != null)
                    {
                        var id = Convert.ToInt32(Division);

                        getData = getData.Where(a => a.Division != null && a.Division.Id == id).ToList();
                    }
                    if (Location != null)
                    {
                        var id = Convert.ToInt32(Location);
                        getData = getData.Where(a => a.Location != null && a.Location.Id == id).ToList();
                    }
                    if (Department != null)
                    {
                        var id = Convert.ToInt32(Department);
                        getData = getData.Where(a => a.Department != null && a.Department.Id == id).ToList();
                    }
                    if (group != null)
                    {
                        var id = Convert.ToInt32(group);

                        getData = getData.Where(a => a.Group.Id == id).ToList();
                    }
                    if (unit != null)
                    {
                        var id = Convert.ToInt32(unit);

                        getData = getData.Where(a => a.Unit.Id == id).ToList();
                    }

                    //var div = db.GeoStruct.Where(a => a.Division.Id.ToString() == Division).FirstOrDefault();
                    //var Loc = db.GeoStruct.Where(a => a.Location.Id.ToString() == Location).FirstOrDefault();
                    //var Dep = db.GeoStruct.Where(a => a.Department.Id.ToString() == Department).FirstOrDefault();
                    if (getData != null)
                    {
                        var b = db.GeoStruct.Include(e => e.Region)
                        .Include(e => e.Unit)
                        .Include(e => e.Company)
                        .Include(e => e.Corporate)
                        .Include(e => e.Department).Where(e => e.Id != null).FirstOrDefault();
                        return Json(new { success = true, responseText = "", data = b }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, }, JsonRequestBehavior.AllowGet);

                    }
                }


            }

            public ActionResult GetFullDetailsFun(FormCollection form)
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    string fun_id = form["fun_id"];
                    string job = form["job-table"];
                    string jobposition = form["jobposition-table"];
                    string Company = form["Company-table"];

                    var getData = db.FuncStruct.Include(e => e.Company)
                        .Include(e => e.JobPosition)
                        .Include(e => e.Job)
                        .Where(e => e.Id != null).ToList();

                    //if (fun_id != null)
                    //{
                    //    var id = Convert.ToInt32(fun_id);
                    //    getData = getData.Where(a => a. .Id != null && a.Company.Id == id).ToList();
                    //}
                    if (job != null)
                    {
                        var id = Convert.ToInt32(job);
                        getData = getData.Where(a => a.Job.Id != null && a.Job.Id == id).ToList();
                    }
                    if (jobposition != null)
                    {
                        var id = Convert.ToInt32(jobposition);
                        getData = getData.Where(a => a.JobPosition.Id != null && a.JobPosition.Id == id).ToList();
                    }

                    if (Company != null)
                    {
                        var id = Convert.ToInt32(Company);
                        getData = getData.Where(a => a.Company.Id != null && a.Company.Id == id).ToList();
                    }
                    //var div = db.GeoStruct.Where(a => a.Division.Id.ToString() == Division).FirstOrDefault();
                    //var Loc = db.GeoStruct.Where(a => a.Location.Id.ToString() == Location).FirstOrDefault();
                    //var Dep = db.GeoStruct.Where(a => a.Department.Id.ToString() == Department).FirstOrDefault();
                    if (getData != null)
                    {
                        var b = db.FuncStruct.Include(e => e.Company)
                        .Include(e => e.JobPosition)
                        .Include(e => e.Job).Where(e => e.Id != null).FirstOrDefault();
                        return Json(new { success = true, responseText = "", data = b }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, }, JsonRequestBehavior.AllowGet);

                    }
                }


            }

            public ActionResult GetFullDetailsPay(FormCollection form)
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    string pay_id = form["pay_id"];
                    string grade = form["grade-table"];
                    string jobstatus = form["jobstatus-table"];
                    string level = form["level-table"];

                    var getData = db.PayStruct.Include(e => e.Company)
                        .Include(e => e.Grade)
                        .Include(e => e.JobStatus)
                        .Include(e => e.Level)
                        .Where(e => e.Id != null).ToList();

                    //if (pay_id != null)
                    //{
                    //    var id = Convert.ToInt32(pay_id);
                    //    getData = getData.Where(a => a.Company.Id != null && a.Company.Id == id).ToList();
                    //}
                    if (grade != null)
                    {
                        var id = Convert.ToInt32(grade);
                        getData = getData.Where(a => a.Grade.Id.ToString() == grade.ToString()).ToList();
                    }
                    if (jobstatus != null)
                    {
                        var id = Convert.ToInt32(jobstatus);
                        getData = getData.Where(a => a.JobStatus.Id != null && a.JobStatus.Id == id).ToList();
                    }
                    if (level != null)
                    {
                        var id = Convert.ToInt32(level);
                        getData = getData.Where(a => a.Level.Id != null && a.Level.Id == id).ToList();
                    }
                    //var div = db.GeoStruct.Where(a => a.Division.Id.ToString() == Division).FirstOrDefault();
                    //var Loc = db.GeoStruct.Where(a => a.Location.Id.ToString() == Location).FirstOrDefault();
                    //var Dep = db.GeoStruct.Where(a => a.Department.Id.ToString() == Department).FirstOrDefault();
                    if (getData != null)
                    {
                        var b = db.PayStruct.Include(e => e.Company)
                        .Include(e => e.Grade)
                        .Include(e => e.JobStatus)
                        .Include(e => e.Level).Where(e => e.Id != null).FirstOrDefault();

                        return Json(new { success = true, responseText = "", data = b }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, }, JsonRequestBehavior.AllowGet);

                    }
                }

            }
        } 	
}