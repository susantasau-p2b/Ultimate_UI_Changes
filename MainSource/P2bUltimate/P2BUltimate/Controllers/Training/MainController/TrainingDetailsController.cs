///
/// Created by Kapil
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
using Training;
using P2BUltimate.Controllers;
using P2BUltimate.Security;


namespace P2BUltimate.Controllers.Training.MainController
{
    public class TrainingDetailsController : Controller
    {

        // GET: /FacultyExternal/
        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/TrainingDetails/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Training/_TrainingEvaluation.cshtml");
        }

        public ActionResult Get(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LvEncashReq.ToList();
                IEnumerable<LvEncashReq> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.LvEncashReq.ToList().Where(d => d.FullDetails.Contains(data));
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

              //private DataBaseContext db = new DataBaseContext();

        [HttpPost]
        public ActionResult Create(TrainingDetails c, FormCollection form)
        {

            //List<TrainingSchedule> lookupLang = new List<TrainingSchedule>();
            using (DataBaseContext db = new DataBaseContext())
            {
                string Lang = form["TrainingScheduleList"] == "0" ? "" : form["TrainingScheduleList"];

                if (Lang != null)
                {
                    List<int> IDs = Lang.Split(',').Select(e => int.Parse(e)).ToList();
                    foreach (var k in IDs)
                    {
                        var value = db.TrainingSchedule.Find(k);
                        c.TrainingSchedule = new List<TrainingSchedule>();
                        c.TrainingSchedule.Add(value);
                    }
                }


                //if (Lang != null)
                //{
                //    var ids = Utility.StringIdsToListIds(Lang);
                //    foreach (var ca in ids)
                //    {
                //        var lookup_val = db.TrainingSchedule.Find(ca);

                //        lookupLang.Add(lookup_val);
                //        c.TrainingSchedule = lookupLang;
                //    }
                //}
                //else
                //{
                //    c.TrainingSchedule = null;
                //}



                string Addrs = form["TrainingEvaluationList"] == "0" ? "" : form["TrainingEvaluationList"];


                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        int AddId = Convert.ToInt32(Addrs);
                        var val = db.TrainingEvaluation

                                            .Where(e => e.Id == AddId).SingleOrDefault();
                        c.TrainingEvaluation = val;
                    }
                }

                string employee = form["Employeelist"] == "0" ? "" : form["Employeelist"];


                if (employee != null)
                {
                    if (employee != "")
                    {
                        int AddId = Convert.ToInt32(Addrs);
                        var val = db.Employee

                                            .Where(e => e.Id == AddId).SingleOrDefault();
                        // c.EmployeeID = val;
                    }
                }
                var IsCancelled = form["IsCancelled"] == "0" ? "" : form["IsCancelled"];
                var IsPresent = form["IsPresent"] == "0" ? "" : form["IsPresent"];

                c.IsCancelled = Convert.ToBoolean(IsCancelled);
                c.IsPresent = Convert.ToBoolean(IsPresent);


                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //if (db.TrainingSchedule.Any(o => o.Session == c.Session))
                        //{
                        //    return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                        //}

                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        TrainingDetails trnschedule = new TrainingDetails()
                        {

                            BatchName = c.BatchName,
                            CancelReason = c.CancelReason,
                            FacultyFeedback = c.FacultyFeedback,
                            FaultyRating = c.FaultyRating,
                            // EmployeeID = c.EmployeeID,
                            TrainingEvaluation = c.TrainingEvaluation,
                            TrainingSchedule = c.TrainingSchedule,
                            IsCancelled = c.IsCancelled,
                            IsPresent = c.IsPresent,

                            FullDetails = c.FullDetails,
                            DBTrack = c.DBTrack
                        };
                        try
                        {
                            db.TrainingDetails.Add(trnschedule);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, c.DBTrack);
                            DT_TrainingDetails DT_Corp = (DT_TrainingDetails)rtn_Obj;

                            // DT_Corp.EmployeeID = c.EmployeeID == null ? 0 : c.EmployeeID.Id;
                            DT_Corp.TrainingEvaluation_Id = c.TrainingEvaluation == null ? 0 : c.TrainingEvaluation.Id;

                            db.Create(DT_Corp);
                            db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


                            ts.Complete();
                            return this.Json(new Object[] { trnschedule.Id, trnschedule.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
        }

        public class returnLookupClass
        {
            public Array trnSch_id { get; set; }
            public Array trnSch_details { get; set; }
          
        }
        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.TrainingDetails
                    .Include(e => e.TrainingSchedule)
                    //  .Include(e => e.EmployeeID)
                    .Include(e => e.TrainingEvaluation)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        BatchName = e.BatchName,
                        CancelReason = e.CancelReason,
                        FacultyFeedback = e.FacultyFeedback,
                        FaultyRating = e.FaultyRating,


                        IsCancelled = e.IsCancelled,
                        IsPresent = e.IsPresent,


                        Action = e.DBTrack.Action,
                        // Emp_id = e.EmployeeID == null ? "" : e.EmployeeID.Id.ToString(),
                        //  Emp_details = e.EmployeeID == null ? "" : e.EmployeeID.FullDetails,
                        Eval_id = e.TrainingEvaluation == null ? "" : e.TrainingEvaluation.Id.ToString(),
                        Eval_details = e.TrainingEvaluation == null ? "" : e.TrainingEvaluation.FullDetails,

                    }).ToList();


                List<returnLookupClass> faculty = new List<returnLookupClass>();
                var k = db.TrainingDetails.Include(e => e.TrainingSchedule)
                    .Include(e => e.TrainingSchedule.Select(a => a.City))
                    .Include(e => e.TrainingSchedule.Select(a => a.Expenses))
                    .Include(e => e.TrainingSchedule.Select(a => a.TrainingCalendar))
                    .Include(e => e.TrainingSchedule.Select(a => a.Venue))
                    .Where(b => b.Id == data).ToList();
                foreach (var v in k)
                {
                    faculty.Add(new returnLookupClass
                    {
                        trnSch_id = v.TrainingSchedule.Select(e => e.Id.ToString()).ToArray(),
                        trnSch_details = v.TrainingSchedule.Select(e => e.FullDetails).ToArray(),
                    });
                }
                var W = db.DT_TrainingDetails
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         BatchName = e.BatchName == null ? "" : e.BatchName,
                         CancelReason = e.CancelReason == null ? "" : e.CancelReason,
                         FacultyFeedback = e.FacultyFeedback == null ? "" : e.FacultyFeedback,
                         FaultyRating = e.FaultyRating == null ? "" : e.FaultyRating,


                         FactSpec_Val = e.TrainingEvaluation_Id == 0 ? "" : db.TrainingEvaluation.Where(x => x.Id == e.TrainingEvaluation_Id).Select(x => x.FullDetails).FirstOrDefault(),
                         TraInst_Val = e.TrainingSchedule_Id == 0 ? "" : db.TrainingSchedule.Where(x => x.Id == e.TrainingSchedule_Id).Select(x => x.FullDetails).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.TrainingDetails.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, faculty, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        //public int EditS(string Corp, string Addrs, string ContactDetails, int data, FacultyExternal c, DBTrack dbT)
        //{
        //    if (Corp != null)
        //    {
        //        if (Corp != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(Corp));
        //            c.FacultyType = val;

        //            var type = db.FacultyExternal.Include(e => e.FacultyType).Where(e => e.Id == data).SingleOrDefault();
        //            IList<FacultyExternal> typedetails = null;
        //            if (type.FacultyType != null)
        //            {
        //                typedetails = db.FacultyExternal.Where(x => x.FacultyType.Id == type.FacultyType.Id && x.Id == data).ToList();
        //            }
        //            else
        //            {
        //                typedetails = db.FacultyExternal.Where(x => x.Id == data).ToList();
        //            }
        //            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //            foreach (var s in typedetails)
        //            {
        //                s.FacultyType = c.FacultyType;
        //                db.FacultyExternal.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }
        //        else
        //        {
        //            var BusiTypeDetails = db.FacultyExternal.Include(e => e.FacultyType).Where(x => x.Id == data).ToList();
        //            foreach (var s in BusiTypeDetails)
        //            {
        //                s.FacultyType = null;
        //                db.FacultyExternal.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var BusiTypeDetails = db.FacultyExternal.Include(e => e.FacultyType).Where(x => x.Id == data).ToList();
        //        foreach (var s in BusiTypeDetails)
        //        {
        //            s.FacultyType = null;
        //            db.FacultyExternal.Attach(s);
        //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //            //await db.SaveChangesAsync();
        //            db.SaveChanges();
        //            TempData["RowVersion"] = s.RowVersion;
        //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //        }
        //    }

        //    if (Addrs != null)
        //    {
        //        if (Addrs != "")
        //        {
        //            var val = db.Address.Find(int.Parse(Addrs));
        //            c.Address = val;

        //            var add = db.Corporate.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
        //            IList<Corporate> addressdetails = null;
        //            if (add.Address != null)
        //            {
        //                addressdetails = db.Corporate.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
        //            }
        //            else
        //            {
        //                addressdetails = db.Corporate.Where(x => x.Id == data).ToList();
        //            }
        //            if (addressdetails != null)
        //            {
        //                foreach (var s in addressdetails)
        //                {
        //                    s.Address = c.Address;
        //                    db.Corporate.Attach(s);
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                    // await db.SaveChangesAsync(false);
        //                    db.SaveChanges();
        //                    TempData["RowVersion"] = s.RowVersion;
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var addressdetails = db.Corporate.Include(e => e.Address).Where(x => x.Id == data).ToList();
        //        foreach (var s in addressdetails)
        //        {
        //            s.Address = null;
        //            db.Corporate.Attach(s);
        //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //            //await db.SaveChangesAsync();
        //            db.SaveChanges();
        //            TempData["RowVersion"] = s.RowVersion;
        //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //        }
        //    }

        //    if (ContactDetails != null)
        //    {
        //        if (ContactDetails != "")
        //        {
        //            var val = db.ContactDetails.Find(int.Parse(ContactDetails));
        //            c.ContactDetails = val;

        //            var add = db.Corporate.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
        //            IList<Corporate> contactsdetails = null;
        //            if (add.ContactDetails != null)
        //            {
        //                contactsdetails = db.Corporate.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
        //            }
        //            else
        //            {
        //                contactsdetails = db.Corporate.Where(x => x.Id == data).ToList();
        //            }
        //            foreach (var s in contactsdetails)
        //            {
        //                s.ContactDetails = c.ContactDetails;
        //                db.Corporate.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var contactsdetails = db.Corporate.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
        //        foreach (var s in contactsdetails)
        //        {
        //            s.ContactDetails = null;
        //            db.Corporate.Attach(s);
        //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //            //await db.SaveChangesAsync();
        //            db.SaveChanges();
        //            TempData["RowVersion"] = s.RowVersion;
        //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //        }
        //    }

        //    var CurCorp = db.Corporate.Find(data);
        //    TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //    {
        //        c.DBTrack = dbT;
        //        Corporate corp = new Corporate()
        //        {
        //            Code = c.Code,
        //            Name = c.Name,
        //            Id = data,
        //            DBTrack = c.DBTrack
        //        };


        //        db.Corporate.Attach(corp);
        //        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //        return 1;
        //    }
        //    return 0;
        //}

        [HttpPost]
        public async Task<ActionResult> EditSave(TrainingDetails c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string Addrs = form["TrainingEvaluationList"] == "0" ? "" : form["TrainingEvaluationList"];


                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        int AddId = Convert.ToInt32(Addrs);
                        var val = db.TrainingEvaluation

                                            .Where(e => e.Id == AddId).SingleOrDefault();
                        c.TrainingEvaluation = val;
                    }
                }

                string employee = form["Employeelist"] == "0" ? "" : form["Employeelist"];


                if (employee != null)
                {
                    if (employee != "")
                    {
                        int AddId = Convert.ToInt32(Addrs);
                        var val = db.Employee

                                            .Where(e => e.Id == AddId).SingleOrDefault();
                        // c.EmployeeID = val;
                    }
                }
                // string ContactDetails = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
                //  bool Auth = form["autho_action"] == "" ? false : true;
                bool Auth = form["autho_allow"] == "true" ? true : false;

                var db_Data = db.TrainingDetails.Include(e => e.TrainingEvaluation).Include(e => e.TrainingSchedule)
                     .Where(e => e.Id == data).SingleOrDefault();
                db_Data.TrainingEvaluation = null;
                db_Data.TrainingSchedule = null;
                //db_Data.EmployeeID = null;

                List<TrainingSchedule> lookupLang = new List<TrainingSchedule>();
                string Lang = form["TrainingScheduleList"];

                if (Lang != null)
                {
                    var ids = Utility.StringIdsToListIds(Lang);
                    foreach (var ca in ids)
                    {
                        var lookup_val = db.TrainingSchedule.Find(ca);

                        lookupLang.Add(lookup_val);
                        c.TrainingSchedule = lookupLang;
                    }
                }
                else
                {
                    c.TrainingSchedule = null;
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


                                using (var context = new DataBaseContext())
                                {
                                    db.TrainingDetails.Attach(db_Data);
                                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_Data.RowVersion;
                                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_OBJ = db.TrainingDetails.Find(data);
                                    TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                    db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        TrainingDetails blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;


                                        blog = context.TrainingDetails.Where(e => e.Id == data).Include(e => e.TrainingSchedule)
                                                                .Include(e => e.TrainingEvaluation)
                                                                .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;


                                        c.DBTrack = new DBTrack
                                        {
                                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };

                                        TrainingDetails lk = new TrainingDetails
                                        {
                                            BatchName = db_Data.BatchName,
                                            CancelReason = db_Data.CancelReason,
                                            FacultyFeedback = db_Data.FacultyFeedback,
                                            FaultyRating = db_Data.FaultyRating,
                                            // EmployeeID = db_Data.EmployeeID,
                                            TrainingEvaluation = db_Data.TrainingEvaluation,
                                            TrainingSchedule = db_Data.TrainingSchedule,
                                            IsCancelled = db_Data.IsCancelled,
                                            IsPresent = db_Data.IsPresent,
                                            DBTrack = db_Data.DBTrack,
                                            Id = db_Data.Id
                                        };


                                        db.TrainingDetails.Attach(lk);
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                        // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                        //db.SaveChanges();
                                        db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                        var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_TrainingDetails DT_LK = (DT_TrainingDetails)obj;
                                        //  DT_LK.Allergy = lk.Allergy.Select(e => e.Id);
                                        db.Create(DT_LK);
                                        db.SaveChanges();
                                        await db.SaveChangesAsync();
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
                                        ts.Complete();
                                        return Json(new Object[] { lk.Id, lk.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                                    }
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (TrainingDetails)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (TrainingDetails)databaseEntry.ToObject();
                                c.RowVersion = databaseValues.RowVersion;

                            }
                        }

                        return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                    }
                }
                else
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        TrainingDetails blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        TrainingDetails Old_Corp = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.TrainingDetails.Where(e => e.Id == data).SingleOrDefault();
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

                        TrainingDetails corp = new TrainingDetails()
                        {
                            BatchName = c.BatchName,
                            CancelReason = c.CancelReason,
                            FacultyFeedback = c.FacultyFeedback,
                            FaultyRating = c.FaultyRating,

                            IsCancelled = c.IsCancelled,
                            IsPresent = c.IsPresent,
                            Id = data,
                            DBTrack = c.DBTrack,
                            RowVersion = (Byte[])TempData["RowVersion"]
                        };

                        //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        using (var context = new DataBaseContext())
                        {
                            var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "TrainingDetails", c.DBTrack);
                            // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                            Old_Corp = context.TrainingDetails.Where(e => e.Id == data).Include(e => e.TrainingSchedule)
                                .Include(e => e.TrainingEvaluation).SingleOrDefault();
                            DT_TrainingDetails DT_Corp = (DT_TrainingDetails)obj;
                            DT_Corp.TrainingEvaluation_Id = DBTrackFile.ValCompare(Old_Corp.TrainingEvaluation, c.TrainingEvaluation);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                            DT_Corp.TrainingSchedule_Id = DBTrackFile.ValCompare(Old_Corp.TrainingSchedule, c.TrainingSchedule); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                            //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                            db.Create(DT_Corp);
                            //db.SaveChanges();
                        }
                        blog.DBTrack = c.DBTrack;
                        db.TrainingDetails.Attach(blog);
                        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        ts.Complete();
                        return Json(new Object[] { blog.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                    }
                }
                return View();
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
                IEnumerable<TrainingDetails> TrainingDetails = null;
                if (gp.IsAutho == true)
                {
                    TrainingDetails = db.TrainingDetails.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    TrainingDetails = db.TrainingDetails.AsNoTracking().ToList();
                }

                IEnumerable<TrainingDetails> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = TrainingDetails;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.BatchName }).Where((e => (e.Id.ToString() == gp.searchString) || (e.BatchName.ToLower() == gp.searchString.ToLower()) )).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.BatchName}).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = TrainingDetails;
                    Func<TrainingDetails, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "BatchName" ? c.BatchName 
                                        :"");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.BatchName)}).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.BatchName)}).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.BatchName}).ToList();
                    }
                    totalRecords = TrainingDetails.Count();
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
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                TrainingDetails corporates = db.TrainingDetails.Include(e => e.TrainingEvaluation)
                                                   .Include(e => e.TrainingSchedule)

                                                .Where(e => e.Id == data).SingleOrDefault();

                TrainingEvaluation add = corporates.TrainingEvaluation;

                // Employee val = corporates.EmployeeID;

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
                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corporates.DBTrack);
                        DT_TrainingDetails DT_Corp = (DT_TrainingDetails)rtn_Obj;
                        DT_Corp.TrainingEvaluation_Id = corporates.TrainingEvaluation == null ? 0 : corporates.TrainingEvaluation.Id;
                        //   DT_Corp.TrainingSchedule_Id = corporates.TrainingSchedule == null ? 0 : corporates.TrainingSchedule.Id;
                        //DT_Corp.EmployeeID = corporates.EmployeeID == null ? 0 : corporates.EmployeeID.Id;
                        db.Create(DT_Corp);
                        // db.SaveChanges();
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                        await db.SaveChangesAsync();
                        //using (var context = new DataBaseContext())
                        //{
                        //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                        //}
                        ts.Complete();
                        return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    }
                }
                else
                {
                    //var selectedRegions = corporates.Session;

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //if (selectedRegions != null)
                        //{
                        //    var corpRegion = new HashSet<int>(corporates.Session.Select(e => e.Id));
                        //    if (corpRegion.Count > 0)
                        //    {
                        //        return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                        //        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                        //    }
                        //}

                        try
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = "0029",
                                ModifiedOn = DateTime.Now,
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = "0029",
                                //AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = "0029", ModifiedOn = DateTime.Now };

                            db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, dbT);
                            DT_TrainingDetails DT_Corp = (DT_TrainingDetails)rtn_Obj;
                            DT_Corp.TrainingEvaluation_Id = add == null ? 0 : add.Id;
                            //    DT_Corp.TrainingSchedule_Id = val == null ? 0 : val.Id;

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
                            return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                        }
                    }
                }
            }
        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj).Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct).Include(e => e.Employee.PayStruct.Grade)
                        .ToList();
                    // for searchs
                    IEnumerable<EmployeePayroll> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        //fall = all.Where(e => (e.Employee.EmpCode == param.sSearch) || (e.Employee.EmpName.FullNameFML.ToUpper() == param.sSearch.ToUpper())).ToList();
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                               || (e.Employee.EmpCode.Contains(param.sSearch))
                               || (e.Employee.EmpName.FullNameFML.ToUpper().Contains(param.sSearch.ToUpper()))
                               ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<EmployeePayroll, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.Employee.EmpCode : "");
                    var sortcolumn = Request["sSortDir_0"];
                    if (sortcolumn == "asc")
                    {
                        fall = fall.OrderBy(orderfunc);
                    }
                    else
                    {
                        fall = fall.OrderByDescending(orderfunc);
                    }
                    // Paging 
                    var dcompanies = fall
                            .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompanies.Count == 0)
                    {
                        List<returndatagridclass> result = new List<returndatagridclass>();
                        foreach (var item in fall)
                        {
                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                Code = item.Employee.EmpCode,
                                Name = item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : null,
                                JoiningDate = item.Employee.ServiceBookDates != null && item.Employee.ServiceBookDates.JoiningDate != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : null,
                                Job = item.Employee.FuncStruct != null && item.Employee.FuncStruct.Job != null ? item.Employee.FuncStruct.Job.Name : null,
                                Grade = item.Employee.PayStruct != null && item.Employee.PayStruct.Grade != null ? item.Employee.PayStruct.Grade.Name : null,
                                Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null && item.Employee.GeoStruct.Location.LocationObj != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : null
                            });
                        }
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = from c in dcompanies

                                     select new[] { null, Convert.ToString(c.Id), c.Employee.EmpCode, };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }//for data reterivation
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
    }
}
