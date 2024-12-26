using P2b.Global;
using Payroll;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Text;
using P2BUltimate.Security;
using System.Threading.Tasks;
namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class IncrPolicyDetailsController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/IncrPolicyDetails/Index.cshtml");
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Core/_IncrPolicyDetails.cshtml");
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
                IEnumerable<IncrPolicyDetails> IncrPolicyDetails = null;
                if (gp.IsAutho == true)
                {
                    IncrPolicyDetails = db.IncrPolicyDetails.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    IncrPolicyDetails = db.IncrPolicyDetails.AsNoTracking().ToList();
                }

                IEnumerable<IncrPolicyDetails> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = IncrPolicyDetails;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.IncrAmount, a.IncrPercent, a.IncrSteps }).Where((e => (e.Id.ToString() == gp.searchString) || (e.IncrAmount.ToString() == gp.searchString.ToString()) || (e.IncrPercent.ToString() == gp.searchString.ToString()) || (e.IncrSteps.ToString() == gp.searchString.ToString()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.IncrAmount, a.IncrPercent, a.IncrSteps }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = IncrPolicyDetails;
                    Func<IncrPolicyDetails, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "IncrAmount" ? c.IncrAmount.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.IncrAmount), Convert.ToString(a.IncrPercent), a.IncrSteps }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.IncrAmount), Convert.ToString(a.IncrPercent), a.IncrSteps }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.IncrAmount, a.IncrPercent, a.IncrSteps }).ToList();
                    }
                    totalRecords = IncrPolicyDetails.Count();
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
        public ActionResult Create(IncrPolicyDetails c)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (c.IsIncrAmount == true && c.IsIncrPercent == true && c.IsIncrSteps == true)
                {
                    Msg.Add("please select Only one amount");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                else if (c.IsIncrAmount == false && c.IsIncrPercent == false && c.IsIncrSteps == false)
                {
                    Msg.Add("please select Only one amount");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                else if (c.IsIncrAmount == true && c.IsIncrPercent == true && c.IsIncrSteps == false)
                {
                    Msg.Add("please select Only one amount");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                else if (c.IsIncrAmount == false && c.IsIncrPercent == true && c.IsIncrSteps == true)
                {
                    Msg.Add("please select Only one amount");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                else if (c.IsIncrAmount == true && c.IsIncrPercent == false && c.IsIncrSteps == true)
                {
                    Msg.Add("please select Only one amount");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    try
                    {
                        if (ModelState.IsValid)
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                //if (db.IncrPolicyDetails.Any(o => o.IncrAmount == c.IncrAmount))
                                //{
                                //	return Json(new Object[] { "", "", "IncrAmount Already Exists.", JsonRequestBehavior.AllowGet });
                                //}

                                c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                IncrPolicyDetails _IncrPolicyDetails = new IncrPolicyDetails()
                                {
                                    IncrPercent = c.IncrPercent,
                                    IncrAmount = c.IncrAmount,
                                    IncrSteps = c.IncrSteps,
                                    IsIncrAmount = c.IsIncrAmount,
                                    IsIncrPercent = c.IsIncrPercent,
                                    IsIncrSteps = c.IsIncrSteps,
                                    DBTrack = c.DBTrack
                                };
                                try
                                {
                                    db.IncrPolicyDetails.Add(_IncrPolicyDetails);
                                    //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                    //DT_IncrPromoPolicyDetails DT_Corp = (DT_IncrPromoPolicyDetails)rtn_Obj;
                                    //db.Create(DT_Corp);
                                    db.Create(_IncrPolicyDetails);
                                    db.SaveChanges();
                                    //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


                                    ts.Complete();
                                    Msg.Add("  Data Saved successfully  ");
                                    return Json(new Utility.JsonReturnClass { Id = _IncrPolicyDetails.Id, Val = _IncrPolicyDetails.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return this.Json(new Object[] { _IncrPolicyDetails.Id, _IncrPolicyDetails.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                }
                                catch (DbUpdateConcurrencyException)
                                {
                                    return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                                }
                                catch (DataException /* dex */)
                                {
                                    Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                            //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    IncrPolicyDetails corporates = db.IncrPolicyDetails.Where(e => e.Id == data).SingleOrDefault();

                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
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
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack);
                            DT_RegIncrPolicy DT_Corp = (DT_RegIncrPolicy)rtn_Obj;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
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
                                    CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                    CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                    IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                DT_RegIncrPolicy DT_Corp = (DT_RegIncrPolicy)rtn_Obj;

                                db.Create(DT_Corp);

                                await db.SaveChangesAsync();

                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
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


        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.IncrPolicyDetails
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        IncrPercent = e.IncrPercent,
                        IncrAmount = e.IncrAmount,
                        IncrSteps = e.IncrSteps,
                        IsIncrAmount = e.IsIncrAmount,
                        IsIncrPercent = e.IsIncrPercent,
                        IsIncrSteps = e.IsIncrSteps,
                        DBTrack = e.DBTrack,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.IncrPolicyDetails
                    .Where(e => e.Id == data)
                    .ToList();


                //var W = db.DT_IncrPromoPolicyDetails
                //	 .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //	 (e => new
                //	 {
                //		 DT_Id=e.Orig_Id,
                //		IncrPercent = e.IncrPercent,
                //			IncrAmount = e.IncrAmount,
                //			IncrSteps = e.IncrSteps,
                //			IsIncrAmount = e.IsIncrAmount,
                //			IsIncrPercent = e.IsIncrPercent,
                //			IsIncrSteps = e.IsIncrSteps,
                //			DBTrack = e.DBTrack
                //	 }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.IncrPolicyDetails.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        //[HttpPost]
        //public async Task<ActionResult> EditSave(IncrPolicyDetails R, int data, FormCollection form) // Edit submit
        //{
        //       List<string> Msg = new List<string>();
        //            try{
        //    bool Auth = form["Autho_Allow"] == "true" ? true : false;

        //    if (Auth == false)
        //    {


        //        if (ModelState.IsValid)
        //        {
        //            try
        //            {

        //                //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    IncrPolicyDetails blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.IncrPolicyDetails.Where(e => e.Id == data).SingleOrDefault();


        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }

        //                    R.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };
        //                    var CurCorp = db.IncrPolicyDetails.Find(data);
        //                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                    {

        //                        IncrPolicyDetails IncrPolicyDetails = new IncrPolicyDetails()
        //                        {
        //                            IncrPercent = R.IncrPercent,
        //                            IncrAmount = R.IncrAmount,
        //                            IncrSteps = R.IncrSteps,
        //                            IsIncrAmount = R.IsIncrAmount,
        //                            IsIncrPercent = R.IsIncrPercent,
        //                            IsIncrSteps = R.IsIncrSteps,
        //                            DBTrack = R.DBTrack,
        //                        };


        //                        db.IncrPolicyDetails.Attach(IncrPolicyDetails);
        //                        db.Entry(IncrPolicyDetails).State = System.Data.Entity.EntityState.Modified;
        //                        db.Entry(IncrPolicyDetails).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                    }

        //                    //int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);



        //                    using (var context = new DataBaseContext())
        //                    {

        //                        //var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, R.DBTrack);
        //                        //DT_RegIncrPolicy DT_obj = (DT_RegIncrPolicy)obj;
        //                        //db.Create(DT_obj);
        //                        db.SaveChanges();
        //                    }
        //                    await db.SaveChangesAsync();
        //                    ts.Complete();
        //                    Msg.Add("  Record Updated");
        //            return Json(new Utility.JsonReturnClass { Id = R .Id   , Val =R  .FullDetails , success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    //return Json(new Object[] { R.Id, R.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

        //                }
        //            }
        //            catch (DbUpdateConcurrencyException ex)
        //            {
        //                var entry = ex.Entries.Single();
        //                var clientValues = (IncrPolicyDetails)entry.Entity;
        //                var databaseEntry = entry.GetDatabaseValues();
        //                if (databaseEntry == null)
        //                {
        //                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                }
        //                else
        //                {
        //                    var databaseValues = (IncrPolicyDetails)databaseEntry.ToObject();
        //                    R.RowVersion = databaseValues.RowVersion;

        //                }
        //            }
        //            Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //        }
        //    }
        //    else
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {

        //            IncrPolicyDetails blog = null; // to retrieve old data
        //            DbPropertyValues originalBlogValues = null;
        //            IncrPolicyDetails Old_Corp = null;

        //            using (var context = new DataBaseContext())
        //            {
        //                blog = context.IncrPolicyDetails.Where(e => e.Id == data).SingleOrDefault();
        //                originalBlogValues = context.Entry(blog).OriginalValues;
        //            }
        //            R.DBTrack = new DBTrack
        //            {
        //                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                Action = "M",
        //                IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                ModifiedBy = SessionManager.UserName,
        //                ModifiedOn = DateTime.Now
        //            };

        //            if (TempData["RowVersion"] == null)
        //            {
        //                TempData["RowVersion"] = blog.RowVersion;
        //            }
        //            IncrPolicyDetails IncrPolicyDetails = new IncrPolicyDetails()
        //            {
        //                IncrPercent = R.IncrPercent,
        //                IncrAmount = R.IncrAmount,
        //                IncrSteps = R.IncrSteps,
        //                IsIncrAmount = R.IsIncrAmount,
        //                IsIncrPercent = R.IsIncrPercent,
        //                IsIncrSteps = R.IsIncrSteps,
        //                DBTrack = R.DBTrack,
        //                RowVersion = (Byte[])TempData["RowVersion"]
        //            };



        //            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //            using (var context = new DataBaseContext())
        //            {
        //                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, IncrPolicyDetails, "IncrPolicyDetails", R.DBTrack);
        //                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                Old_Corp = context.IncrPolicyDetails.Where(e => e.Id == data).SingleOrDefault();
        //                DT_RegIncrPolicy DT_Corp = (DT_RegIncrPolicy)obj;
        //                db.Create(DT_Corp);
        //                //db.SaveChanges();
        //            }
        //            blog.DBTrack = R.DBTrack;
        //            db.IncrPolicyDetails.Attach(blog);
        //            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            db.SaveChanges();
        //            ts.Complete();
        //             Msg.Add("  Record Updated");
        //            return Json(new Utility.JsonReturnClass { Id = blog .Id   , Val =  R.FullDetails , success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            //return Json(new Object[] { blog.Id, R.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
        //        }

        //    }
        //            }
        //            catch (Exception ex)
        //            {
        //                LogFile Logfile = new LogFile();
        //                ErrorLog Err = new ErrorLog()
        //                {
        //                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                    ExceptionMessage = ex.Message,
        //                    ExceptionStackTrace = ex.StackTrace,
        //                    LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                    LogTime = DateTime.Now
        //                };
        //                Logfile.CreateLogFile(Err);
        //                Msg.Add(ex.Message);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            } 
        //    return View();

        //}

        public async Task<ActionResult> EditSave(IncrPolicyDetails val, int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                List<string> Msg = new List<string>();
                if (val.IsIncrAmount == true && val.IsIncrPercent == true && val.IsIncrSteps == true)
                {
                    Msg.Add("please select Only one amount");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                else if (val.IsIncrAmount == false && val.IsIncrPercent == false && val.IsIncrSteps == false)
                {
                    Msg.Add("please select Only one amount");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                else if (val.IsIncrAmount == true && val.IsIncrPercent == true && val.IsIncrSteps == false)
                {
                    Msg.Add("please select Only one amount");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                else if (val.IsIncrAmount == false && val.IsIncrPercent == true && val.IsIncrSteps == true)
                {
                    Msg.Add("please select Only one amount");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                else if (val.IsIncrAmount == true && val.IsIncrPercent == false && val.IsIncrSteps == true)
                {
                    Msg.Add("please select Only one amount");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    try
                    {
                        try
                        {
                            if (ModelState.IsValid)
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    var Curr_LKValue = db.IncrPolicyDetails.Find(data);
                                    TempData["CurrRowVersion"] = Curr_LKValue.RowVersion;
                                    db.Entry(Curr_LKValue).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        IncrPolicyDetails blog = blog = null;
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.IncrPolicyDetails.Where(e => e.Id == data).SingleOrDefault();
                                            originalBlogValues = context.Entry(blog).OriginalValues;
                                        }

                                        val.DBTrack = new DBTrack
                                        {
                                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };
                                        IncrPolicyDetails lkval = new IncrPolicyDetails
                                        {
                                            Id = data,
                                            IncrAmount = val.IncrAmount,
                                            IncrPercent = val.IncrPercent,
                                            IncrSteps = val.IncrSteps,
                                            IsIncrAmount = val.IsIncrAmount,
                                            IsIncrPercent = val.IsIncrPercent,
                                            IsIncrSteps = val.IsIncrSteps,
                                            DBTrack = val.DBTrack
                                        };


                                        db.IncrPolicyDetails.Attach(lkval);
                                        db.Entry(lkval).State = System.Data.Entity.EntityState.Modified;

                                        // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                        //db.SaveChanges();
                                        db.Entry(lkval).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, val.DBTrack);
                                        await db.SaveChangesAsync();
                                        //DisplayTrackedEntities(db.ChangeTracker);
                                        db.Entry(lkval).State = System.Data.Entity.EntityState.Detached;
                                        ts.Complete();
                                        //return Json(new Object[] { lkval.Id, lkval.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = lkval.Id, Val = lkval.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }
                            }
                            return View();
                        }
                        catch (DbUpdateConcurrencyException e) { throw e; }
                        catch (DbUpdateException e) { throw e; }
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



        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            //if (auth_action == "C")
            //{
            //	using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            //	{
            //		//Corporate corp = db.Corporate.Find(auth_id);
            //		//Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

            //		IncrPolicyDetails corp = db.IncrPolicyDetails.FirstOrDefault(e => e.Id == auth_id);

            //		corp.DBTrack = new DBTrack
            //		{
            //			Action = "C",
            //			ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
            //			CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
            //			CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
            //			IsModified = corp.DBTrack.IsModified == true ? false : false,
            //			AuthorizedBy = SessionManager.UserName,
            //			AuthorizedOn = DateTime.Now
            //		};

            //		db.IncrPolicyDetails.Attach(corp);
            //		db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
            //		db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
            //		//db.SaveChanges();
            //		var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
            //		DT_RegIncrPolicy DT_Corp = (DT_RegIncrPolicy)rtn_Obj;

            //		db.Create(DT_Corp);
            //		await db.SaveChangesAsync();

            //		ts.Complete();
            //		return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
            //	}
            //}
            //else if (auth_action == "M")
            //{

            //	IncrPolicyDetails Old_Corp = db.IncrPolicyDetails.Where(e => e.Id == auth_id).SingleOrDefault();


            //	DT_IncrPromoPolicyDetails Curr_Corp = db.DT_IncrPromoPolicyDetails
            //								.Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
            //								.OrderByDescending(e => e.Orig_Id)
            //								.FirstOrDefault();

            //	if (Curr_Corp != null)
            //	{
            //		IncrPolicyDetails corp = new IncrPolicyDetails();

            //		corp.IncrAmount = Curr_Corp.IncrAmount == null ? Old_Corp.IncrPercent : Curr_Corp.IncrAmount;
            //		corp.IncrPercent = Curr_Corp.IncrPercent == null ? Old_Corp.IncrAmount : Curr_Corp.IncrPercent;
            //		corp.IncrSteps = Curr_Corp.IncrSteps == null ? Old_Corp.IncrSteps :Curr_Corp.IncrSteps;
            //		corp.IsIncrAmount = Curr_Corp.IsIncrAmount == null ? Old_Corp.IsIncrAmount : Curr_Corp.IsIncrAmount;
            //		corp.IsIncrPercent = Curr_Corp.IsIncrPercent == null ? Old_Corp.IsIncrPercent : Curr_Corp.IsIncrPercent;
            //		corp.IncrSteps = Curr_Corp.IsIncrSteps == null ? Old_Corp.IncrSteps : Curr_Corp.IncrSteps;
            //	//	corp.IsNewScaleAction = Curr_Corp.IsNewScaleAction == null ? Old_Corp.IsNewScaleAction : Curr_Corp.IsNewScaleAction;

            //	//	corp.IsOldScaleAction = Curr_Corp.IsOldScaleAction == null ? Old_Corp.IsOldScaleAction : Curr_Corp.IsOldScaleAction;
            //	//	corp.IsLWPIncl = Curr_Corp.IsLWPIncl == null ? Old_Corp.IsLWPIncl : Curr_Corp.IsLWPIncl;
            //	///	corp.IsMidMonthEffect = Curr_Corp.IsMidMonthEffect == null ? Old_Corp.IsMidMonthEffect : Curr_Corp.IsMidMonthEffect;
            //		// corp.IsMidQuarterEffect = Curr_Corp.IsMidQuarterEffect == null ? Old_Corp.IsMidQuarterEffect : Curr_Corp.IsMidQuarterEffect;
            //		//corp.MidMonthLockDay = Curr_Corp.MidMonthLockDay == null ? Old_Corp.IsJoiningDate : Curr_Corp.IsJoiningDate;
            //		//  corp.NextMonStartDay = Curr_Corp.NextMonStartDay == null ? Old_Corp.NextMonStartDay : Curr_Corp.NextMonStartDay;


            //		//      corp.Id = auth_id;

            //		if (ModelState.IsValid)
            //		{
            //			try
            //			{

            //				//DbContextTransaction transaction = db.Database.BeginTransaction();

            //				using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            //				{
            //					// db.Configuration.AutoDetectChangesEnabled = false;
            //					corp.DBTrack = new DBTrack
            //					{
            //						CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
            //						CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
            //						Action = "M",
            //						ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
            //						ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
            //						AuthorizedBy = SessionManager.UserName,
            //						AuthorizedOn = DateTime.Now,
            //						IsModified = false
            //					};

            //					int a = EditS(auth_id, corp, corp.DBTrack);

            //					await db.SaveChangesAsync();

            //					ts.Complete();
            //					return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
            //				}
            //			}
            //			catch (DbUpdateConcurrencyException ex)
            //			{
            //				var entry = ex.Entries.Single();
            //				var clientValues = (IncrPolicyDetails)entry.Entity;
            //				var databaseEntry = entry.GetDatabaseValues();
            //				if (databaseEntry == null)
            //				{
            //					return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
            //				}
            //				else
            //				{
            //					var databaseValues = (IncrPolicyDetails)databaseEntry.ToObject();
            //					corp.RowVersion = databaseValues.RowVersion;
            //				}
            //			}

            //			return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
            //		}
            //	}
            //	else
            //		return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
            //}
            //else if (auth_action == "D")
            //{
            //	using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            //	{
            //		//Corporate corp = db.Corporate.Find(auth_id);
            //		IncrPolicyDetails corp = db.IncrPolicyDetails.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);
            //		corp.DBTrack = new DBTrack
            //		{
            //			Action = "D",
            //			ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
            //			CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
            //			CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
            //			IsModified = false,
            //			AuthorizedBy = SessionManager.UserName,
            //			AuthorizedOn = DateTime.Now
            //		};

            //		db.IncrPolicyDetails.Attach(corp);
            //		db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


            //		var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
            //		DT_RegIncrPolicy DT_Corp = (DT_RegIncrPolicy)rtn_Obj;
            //		db.Create(DT_Corp);
            //		await db.SaveChangesAsync();
            //		db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
            //		ts.Complete();
            //		return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
            return View();
        }


        public int EditS(int data, IncrPolicyDetails c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CurCorp = db.IncrPolicyDetails.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    IncrPolicyDetails corp = new IncrPolicyDetails()
                    {
                        IncrPercent = c.IncrPercent,
                        IncrAmount = c.IncrAmount,
                        IncrSteps = c.IncrSteps,
                        IsIncrAmount = c.IsIncrAmount,
                        IsIncrPercent = c.IsIncrPercent,
                        IsIncrSteps = c.IsIncrSteps,
                        DBTrack = c.DBTrack,
                        Id = data,
                    };


                    db.IncrPolicyDetails.Attach(corp);
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
        public ActionResult GetLookupIncrPolicyDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.IncrPolicyDetails.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.IncrPolicyDetails.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
    }
}