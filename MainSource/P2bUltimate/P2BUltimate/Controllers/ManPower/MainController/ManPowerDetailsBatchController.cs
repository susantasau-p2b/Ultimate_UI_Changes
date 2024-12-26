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
using Payroll;
using Recruitment;
namespace P2BUltimate.Controllers.ManPower.MainController
{
    public class ManPowerDetailsBatchController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /ManPowerDetailsBatch/
        public ActionResult Index()
        {
            return View("~/Views/ManPower/MainViews/ManPowerDetailsBatch/Index.cshtml");
        }


        public ActionResult Create(ManPowerDetailsBatch c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string salhd = form["ManPowerPostDatalist"] == "0" ? "" : form["ManPowerPostDatalist"];


                    if (salhd != null)
                    {
                        var ids = Utility.StringIdsToListIds(salhd);
                        var HolidayList = new List<ManPowerPostData>();
                        foreach (var item in ids)
                        {

                            int HolidayListid = Convert.ToInt32(item);
                            var val = db.ManPowerPostData.Where(e => e.Id == HolidayListid).SingleOrDefault();
                            if (val != null)
                            {
                                HolidayList.Add(val);
                            }
                        }
                        c.ManPowerPostData = HolidayList;
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            ManPowerDetailsBatch ctc = new ManPowerDetailsBatch()
                            {

                                BatchName = c.BatchName,
                                ProcessDate = c.ProcessDate,
                                ManPowerPostData = c.ManPowerPostData,

                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.ManPowerDetailsBatch.Add(ctc);
                                //   var rtn_Obj = DBTrackFile.DBTrackSave("ManPower", null, db.ChangeTracker,"");
                                //  DT_CtcDefinition DT_Corp = (DT_Corporate)rtn_Obj;

                                //   db.Create(DT_Corp);
                                db.SaveChanges();
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


                                ts.Complete();
                                //   return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                //  return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
        public class salhddetails
        {
            public int salhd_id { get; set; }
            public string salhd_details { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var Q = db.ManPowerDetailsBatch
                    .Include(e => e.ManPowerPostData)

                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        BatchName = e.BatchName,
                        ProcessDate = e.ProcessDate,
                        ActionDate = e.ActionDate,
                        IsCloseBatch = e.IsCloseBatch,
                        ActionMovement = e.ActionMovement,
                        ActionRecruitment = e.ActionRecruitment
                    }).ToList();
                List<salhddetails> objlist = new List<salhddetails>();
                var N = db.ManPowerDetailsBatch.Include(e => e.ManPowerPostData).Where(e => e.Id == data).SingleOrDefault();
                //  var N = db.ManPowerDetailsBatch.Include(e => e.ManPowerPostData.Select(b => b.FullDetails)).Where(e => e.Id == data).ToList();
                if (N != null)
                {
                    foreach (var ca in N.ManPowerPostData)
                    {
                        objlist.Add(new salhddetails
                        {

                            salhd_id = ca.Id,
                            salhd_details = ca.FullDetails


                        });

                    }

                }

                //var W = db.DT_Corporate
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,
                //         Code = e.Code == null ? "" : e.Code,
                //         Name = e.Name == null ? "" : e.Name,
                //         BusinessType_Val = e.BusinessType_Id == 0 ? "" : db.LookupValue
                //                    .Where(x => x.Id == e.BusinessType_Id)
                //                    .Select(x => x.LookupVal).FirstOrDefault(),

                //         Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
                //         Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.ManPowerDetailsBatch.Find(data);
                //TempData["RowVersion"] = Corp.RowVersion;
                // var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, objlist, JsonRequestBehavior.AllowGet });
            }
        }


        //[HttpPost]
        //public async Task<ActionResult> EditSave(ManPowerDetailsBatch c, int data, FormCollection form) // Edit submit
        //{
        //      List<string> Msg = new List<string>();
        //            try{
        //    string salhd = form["ManPowerPostDatalist"] == "0" ? "" : form["ManPowerPostDatalist"];


        //    if (salhd != null)
        //    {
        //        var ids = Utility.StringIdsToListIds(salhd);
        //        var HolidayList = new List<ManPowerPostData>();
        //        foreach (var item in ids)
        //        {

        //            int HolidayListid = Convert.ToInt32(item);
        //            var val = db.ManPowerPostData
        //                                .Where(e => e.Id == HolidayListid).SingleOrDefault();
        //            if (val != null)
        //            {
        //                HolidayList.Add(val);
        //            }
        //        }
        //        c.ManPowerPostData = HolidayList;
        //    } bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //    if (Auth == false)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            try
        //            {

        //                //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    ManPowerDetailsBatch blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.ManPowerDetailsBatch.Where(e => e.Id == data).Include(e => e.ManPowerPostData)

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


        //                    List<ManPowerPostData> ObjHolidayList = new List<ManPowerPostData>();
        //                    ManPowerDetailsBatch ManPowerBudgetdetails = null;
        //                    ManPowerBudgetdetails = db.ManPowerDetailsBatch.Include(e => e.ManPowerPostData).Where(e => e.Id == data).SingleOrDefault();
        //                    if (salhd != null && salhd != "")
        //                    {
        //                        var ids = Utility.StringIdsToListIds(salhd);
        //                        foreach (var ca in ids)
        //                        {
        //                            var HolidayListListvalue = db.ManPowerPostData.Find(ca);
        //                            ObjHolidayList.Add(HolidayListListvalue);
        //                            ManPowerBudgetdetails.ManPowerPostData = ObjHolidayList;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        ManPowerBudgetdetails.ManPowerPostData = null;
        //                    }

        //                    var CurCorp = db.ManPowerDetailsBatch.Find(data);
        //                    // TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;


        //                    ManPowerDetailsBatch ManPowerDetailsBatch = new ManPowerDetailsBatch()
        //                    {

        //                        Id = data,
        //                        BatchName = c.BatchName,
        //                        ProcessDate = c.ProcessDate,

        //                         DBTrack = c.DBTrack
        //                    };
        //                    db.ManPowerDetailsBatch.Attach(ManPowerDetailsBatch);
        //                    db.Entry(ManPowerDetailsBatch).State = System.Data.Entity.EntityState.Modified;
        //                  //  db.Entry(ManPowerDetailsBatch).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
        //                    using (var context = new DataBaseContext())
        //                    {
        //                        //var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, L.DBTrack);
        //                        //dt_holiday DT_Corp = (DT_LvCreditPolicy)obj;
        //                        //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;
        //                        //db.Create(DT_Corp);
        //                        db.SaveChanges();
        //                    }
        //                    await db.SaveChangesAsync();
        //                    ts.Complete();
        //                    //var qurey = db.HolidayCalendar.Include(e => e.HoliCalendar).Include(e => e.HolidayList).Where(e => e.Id == data).SingleOrDefault();
        //                   // return Json(new Object[] { c.Id, c.BatchName, "Record Updated", JsonRequestBehavior.AllowGet });
        //                     Msg.Add("  Record Updated");
        //            return Json(new Utility.JsonReturnClass { Id = c .Id   , Val = c.BatchName , success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                }

        //            }
        //            catch (DbUpdateConcurrencyException ex)
        //            {
        //                var entry = ex.Entries.Single();
        //                var clientValues = (ManPowerDetailsBatch)entry.Entity;
        //                var databaseEntry = entry.GetDatabaseValues();
        //                if (databaseEntry == null)
        //                {
        //                   // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                Msg.Add(" Unable to save changes. The record was deleted by another user.");				
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                }
        //                else
        //                {
        //                    var databaseValues = (ManPowerDetailsBatch)databaseEntry.ToObject();
        //                    ///   c.RowVersion = databaseValues.RowVersion;

        //                }
        //            }

        //            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //       Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


        //        }
        //    }
        //    else
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {

        //            ManPowerDetailsBatch blog = null; // to retrieve old data
        //            DbPropertyValues originalBlogValues = null;
        //            ManPowerDetailsBatch Old_Corp = null;

        //            using (var context = new DataBaseContext())
        //            {
        //                blog = context.ManPowerDetailsBatch.Where(e => e.Id == data).SingleOrDefault();
        //                originalBlogValues = context.Entry(blog).OriginalValues;
        //            }
        //            //L.DBTrack = new DBTrack
        //            //{
        //            //    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //            //    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //            //    Action = "M",
        //            //    IsModified = blog.DBTrack.IsModified == true ? true : false,
        //            //    ModifiedBy = SessionManager.UserName,
        //            //    ModifiedOn = DateTime.Now
        //            //};

        //            //if (TempData["RowVersion"] == null)
        //            //{
        //            //    TempData["RowVersion"] = blog.RowVersion;
        //            //}

        //            ManPowerDetailsBatch corp = new ManPowerDetailsBatch()
        //            {
        //                ManPowerPostData = c.ManPowerPostData,

        //                Id = data,
        //                //   DBTrack = L.DBTrack,
        //                //  RowVersion = (Byte[])TempData["RowVersion"]
        //            };


        //            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //            using (var context = new DataBaseContext())
        //            {
        //                //var obj = DBTrackFile.ModifiedDataHistory("Leave/Leave", "M", blog, corp, "LvEncashReq", L.DBTrack);
        //                //// var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                //Old_Corp = context.LvCreditPolicy.Where(e => e.Id == data).Include(e => e.ConvertLeaveHead)
        //                //    .Include(e => e.ConvertLeaveHeadBal).Include(e => e.ExcludeLeaveHeads).Include(e => e.CreditDate).SingleOrDefault();
        //                //DT_LvCreditPolicy DT_Corp = (DT_LvCreditPolicy)obj;
        //                //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;

        //                //db.Create(DT_Corp);
        //                //db.SaveChanges();
        //            }
        //            //    blog.DBTrack = L.DBTrack;
        //            db.ManPowerDetailsBatch.Attach(blog);
        //            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            db.SaveChanges();
        //            ts.Complete();
        //           // return Json(new Object[] { blog.Id, c.BatchName, "Record Updated", JsonRequestBehavior.AllowGet });
        //           Msg.Add("  Record Updated");
        //            return Json(new Utility.JsonReturnClass { Id =  blog.Id   , Val =  c.BatchName , success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //        }

        //    }
        //    return View();
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

        //}

        [HttpPost]
        public async Task<ActionResult> EditSave(ManPowerDetailsBatch L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
                try
                {
                    string salhd = form["ManPowerPostDatalist"] == "0" ? "" : form["ManPowerPostDatalist"];

                    var blog1 = db.ManPowerDetailsBatch.Where(e => e.Id == data).Include(e => e.ManPowerPostData).SingleOrDefault();

                    blog1.ManPowerPostData = null;

                    // blog1.Narration = L.Narration;
                    //blog1.NoOfVacancies = L.NoOfVacancies;
                    blog1.BatchName = L.BatchName;
                    blog1.ProcessDate = L.ProcessDate;
                    blog1.IsCloseBatch = L.IsCloseBatch;
                    blog1.ActionMovement = L.ActionMovement;
                    blog1.ActionRecruitment = L.ActionRecruitment;
                    blog1.ActionDate = L.ActionDate;


                    List<ManPowerPostData> ObjQualification = new List<ManPowerPostData>();
                    ManPowerDetailsBatch pd1 = null;
                    pd1 = db.ManPowerDetailsBatch.Include(e => e.ManPowerPostData).Where(e => e.Id == data).SingleOrDefault();
                    if (salhd != null && salhd != "")
                    {
                        var ids = Utility.StringIdsToListIds(salhd);
                        foreach (var ca in ids)
                        {
                            var value = db.ManPowerPostData.Find(ca);
                            ObjQualification.Add(value);
                            pd1.ManPowerPostData = ObjQualification;
                            blog1.ManPowerPostData = ObjQualification;
                        }
                    }
                    else
                    {
                        pd1.ManPowerPostData = null;
                    }

                    if (ModelState.IsValid)
                    {
                        try
                        {
                            {

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {

                                    blog1.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog1.DBTrack.CreatedBy == null ? null : blog1.DBTrack.CreatedBy,
                                        CreatedOn = blog1.DBTrack.CreatedOn == null ? null : blog1.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    var m1 = db.ManPowerDetailsBatch.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.ManPowerDetailsBatch.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    var CurCorp = db.ManPowerDetailsBatch.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        ManPowerDetailsBatch post = new ManPowerDetailsBatch()
                                        {
                                            BatchName = blog1.BatchName,
                                            ProcessDate = blog1.ProcessDate,
                                            ActionDate = blog1.ActionDate,
                                            ActionMovement = blog1.ActionMovement,
                                            ActionRecruitment = blog1.ActionRecruitment,
                                            IsCloseBatch = blog1.IsCloseBatch,
                                            ManPowerPostData = blog1.ManPowerPostData,
                                            Id = data,
                                            DBTrack = blog1.DBTrack
                                        };
                                        db.ManPowerDetailsBatch.Attach(post);
                                        db.Entry(post).State = System.Data.Entity.EntityState.Modified;

                                        db.Entry(post).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    }
                                    db.SaveChanges();

                                    await db.SaveChangesAsync();
                                    //  db.Entry(post).State = System.Data.Entity.EntityState.Detached;
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = blog1.Id, Val = blog1.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });


                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (ManPowerDetailsBatch)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (ManPowerDetailsBatch)databaseEntry.ToObject();
                                blog1.RowVersion = databaseValues.RowVersion;

                            }
                        }
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });

                    }
                    return Json(new Object[] { "", "", "", JsonRequestBehavior.AllowGet });
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

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    ManPowerDetailsBatch corporates = db.ManPowerDetailsBatch.Include(e => e.ManPowerPostData)
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
                            var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, corporates.DBTrack);
                            //DT_AppraisalPublish DT_Corp = (DT_AppraisalPublish)rtn_Obj;
                            // DT_Corp.AppraisalPeriodCalendar_Id = corporates.AppraisalPeriodCalendar == null ? 0 : corporates.AppraisalPeriodCalendar.Id;
                            //  db.Create(DT_Corp);
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
                            //  DT_AppraisalPublish DT_Corp = (DT_AppraisalPublish)rtn_Obj;
                            // DT_Corp.AppraisalPeriodCalendar_Id = corporates.AppraisalPeriodCalendar == null ? 0 : corporates.AppraisalPeriodCalendar.Id;
                            // db.Create(DT_Corp);

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


        //[HttpPost]
        //public async Task<ActionResult> EditSave(ManPowerDetailsBatch add, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            string salhd = form["ManPowerPostDatalist"] == "0" ? "" : form["ManPowerPostDatalist"];


        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //            try
        //            {

        //                var db_data = db.ManPowerDetailsBatch.Include(e => e.ManPowerPostData).Where(e => e.Id == data).SingleOrDefault();


        //                List<ManPowerPostData> ObjITsection = new List<ManPowerPostData>();
        //                ManPowerPostData pd = null;
        //                pd = db.ManPowerPostData.Where(e => e.Id == data).SingleOrDefault();
        //                if (salhd != null && salhd != "")
        //                {
        //                    var ids = Utility.StringIdsToListIds(salhd);
        //                    foreach (var ca in ids)
        //                    {
        //                        var value = db.ManPowerPostData.Find(ca);
        //                        ObjITsection.Add(value);
        //                        db_data.ManPowerPostData = ObjITsection;

        //                    }
        //                }
        //                else
        //                {
        //                    db_data.ManPowerPostData = null;
        //                }


        //                db.ManPowerDetailsBatch.Attach(db_data);
        //                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //                db.SaveChanges();
        //                TempData["RowVersion"] = db_data.RowVersion;
        //                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;


        //                if (Auth == false)
        //                {
        //                    if (ModelState.IsValid)
        //                    {
        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            ManPowerDetailsBatch blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.ManPowerDetailsBatch.Where(e => e.Id == data)
        //                            .Include(e => e.ManPowerPostData).SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            add.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };

        //                            int a = EditS(salhd, data, add, add.DBTrack);



        //                            using (var context = new DataBaseContext())
        //                            {
        //                                //   var obj = DBTrackFile.DBTrackSave("ManPower/ManPower", originalBlogValues, db.ChangeTracker, add.DBTrack);
        //                                //dt_manpow DT_Addrs = (DT_AppCategoryRating)obj;
        //                                //DT_Addrs.AppCategory_Id = blog.AppCategory == null ? 0 : blog.AppCategory.Id;
        //                                //db.Create(DT_Addrs);

        //                                db.SaveChanges();
        //                            }
        //                            await db.SaveChangesAsync();
        //                            ts.Complete();
        //                            Msg.Add("Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = blog.Id.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                            //    return Json(new { blog.Id, query.FullAddress, , JsonRequestBehavior.AllowGet });
        //                        }


        //                        //  return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                    }
        //                }

        //            }
        //            catch (DbUpdateConcurrencyException ex)
        //            {
        //                var entry = ex.Entries.Single();
        //                var clientValues = (ManPowerDetailsBatch)entry.Entity;
        //                var databaseEntry = entry.GetDatabaseValues();
        //                if (databaseEntry == null)
        //                {
        //                    Msg.Add(ex.Message);
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }
        //                else
        //                {
        //                    var databaseValues = (ManPowerDetailsBatch)databaseEntry.ToObject();
        //                    add.RowVersion = databaseValues.RowVersion;

        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                Msg.Add(e.Message);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
        //            }
        //            return View();

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
        public int EditS(string salhd, int data, ManPowerDetailsBatch c, DBTrack dbT)
        {
            IList<ManPowerDetailsBatch> typedetails = null;

            using (DataBaseContext db = new DataBaseContext())
            {
                var CurCorp = db.ManPowerDetailsBatch.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    ManPowerDetailsBatch corp = new ManPowerDetailsBatch()
                    {
                        BatchName = c.BatchName == null ? "" : c.BatchName,
                        ProcessDate = c.ProcessDate,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.ManPowerDetailsBatch.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }


        //[HttpPost]
        //public ActionResult GetManPowerPostDataLKDetails(List<int> SkipIds)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        //List<string> Ids = SkipIds.ToString();
        //        var fall = db.ManPowerPostData.ToList();

        //        if (SkipIds != null)
        //        {
        //            foreach (var a in SkipIds)
        //            {
        //                if (fall == null)
        //                    fall = db.ManPowerPostData.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
        //                else
        //                    fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

        //            }
        //        }

        //        var list1 = db.ManPowerDetailsBatch.ToList().Select(e => e.ManPowerPostData);
        //        var list2 = fall.Except(list1);

        //        //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
        //        var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();

        //        return Json(r, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public ActionResult GetLookupLvHeadObj(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ManPowerPostData.ToList();
                IEnumerable<ManPowerPostData> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ManPowerPostData.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


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
                IEnumerable<ManPowerDetailsBatch> ManPowerDetailsBatch = null;
                if (gp.IsAutho == true)
                {
                    ManPowerDetailsBatch = db.ManPowerDetailsBatch.Include(e => e.ManPowerPostData).ToList();
                }
                else
                {
                    ManPowerDetailsBatch = db.ManPowerDetailsBatch.Include(e => e.ManPowerPostData).AsNoTracking().ToList();
                }

                IEnumerable<ManPowerDetailsBatch> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = ManPowerDetailsBatch;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.ProcessDate.Value.ToShortDateString().ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.BatchName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.ProcessDate.Value.ToShortDateString(), a.BatchName, a.Id }).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.ProcessDate.Value.ToShortDateString(), a.BatchName, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = ManPowerDetailsBatch;
                    Func<ManPowerDetailsBatch, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "ProcessDate" ? c.ProcessDate.Value.ToShortDateString() :
                                         gp.sidx == "BatchName" ? c.BatchName :
                                         "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.ProcessDate.Value.ToShortDateString()), Convert.ToString(a.BatchName), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.ProcessDate.Value.ToShortDateString()), Convert.ToString(a.BatchName), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.ProcessDate.Value.ToShortDateString()), Convert.ToString(a.BatchName), a.Id }).ToList();
                    }
                    totalRecords = ManPowerDetailsBatch.Count();
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