
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
using P2BUltimate.Security;
using System.Threading.Tasks;
using System.Text;

namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class WagesController : Controller
    {
        //
        // GET: /Wages/
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/Payroll/_Wages.cshtml");
        }
        public ActionResult GetLookupDetails(string data)
        {
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var fall = db.Wages.ToList();
                    IEnumerable<Wages> all;
                    if (!string.IsNullOrEmpty(data))
                    {
                        all = db.Wages.ToList().Where(d => d.WagesName.ToString().Contains(data));

                    }
                    else
                    {
                        var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.WagesName }).Distinct();
                        return Json(r, JsonRequestBehavior.AllowGet);
                    }
                    var result = (from c in all
                                  select new { c.Id, c.Percentage }).Distinct();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //public ActionResult EditSave (Wages ESOBJ, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;

        //            var db_data = db.Wages.Include(e => e.RateMaster).Where(e => e.Id == data).SingleOrDefault();
        //            List<RateMaster> RateMaster = new List<RateMaster>();
        //            string Values = form["RateMasterlist"];

        //            if (Values != null)
        //            {
        //                var ids = Utility.StringIdsToListIds(Values);
        //                foreach (var ca in ids)
        //                {
        //                    var RateMaster_val = db.RateMaster.Find(ca);
        //                    RateMaster.Add(RateMaster_val);
        //                    db_data.RateMaster = RateMaster;
        //                }
        //            }
        //            else
        //            {
        //                db_data.RateMaster = null;
        //            }

        //            db.Wages.Attach(db_data);
        //            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //            TempData["RowVersion"] = db_data.RowVersion;
        //            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;


        //            if (Auth == false)
        //            {
        //                if (ModelState.IsValid)
        //                {
        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                    {
        //                        try
        //                        {
        //                            //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                            Wages blog = null; // to retrieve old data                           
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.Wages.Where(e => e.Id == data)
        //                                                        .Include(e => e.RateMaster).SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }
        //                            ESOBJ.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };

        //                            if (Values != null && Values != "")
        //                            {

        //                                List<int> IDs = Values.Split(',').Select(e => int.Parse(e)).ToList();
        //                                foreach (var k in IDs)
        //                                {
        //                                    var value = db.RateMaster.Find(k);
        //                                    ESOBJ.RateMaster = new List<RateMaster>();
        //                                    ESOBJ.RateMaster.Add(value);
        //                                }
        //                            }
        //                            else
        //                            {
        //                                var RateMasterdetails = db.Wages.Include(e => e.RateMaster).Where(x => x.Id == data).ToList();
        //                                foreach (var s in RateMasterdetails)
        //                                {
        //                                    s.RateMaster = null;
        //                                    db.Wages.Attach(s);
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                    db.SaveChanges();
        //                                    TempData["RowVersion"] = s.RowVersion;
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                }
        //                            }


        //                            var CurOBJ = db.Wages.Find(data);
        //                            TempData["CurrRowVersion"] = CurOBJ.RowVersion;
        //                            db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
        //                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            {
        //                                Wages TOBJ = new Wages()
        //                                {
        //                                    CeilingMin = ESOBJ.CeilingMin,
        //                                    CeilingMax = ESOBJ.CeilingMax,
        //                                    Percentage = ESOBJ.Percentage,
        //                                    RateMaster = ESOBJ.RateMaster,
        //                                    WagesName = ESOBJ.WagesName,
        //                                    Id = data,
        //                                    DBTrack = ESOBJ.DBTrack
        //                                };


        //                                db.Wages.Attach(TOBJ);
        //                                db.Entry(TOBJ).State = System.Data.Entity.EntityState.Modified;
        //                                db.Entry(TOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                                //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                            }
        //                            using (var context = new DataBaseContext())
        //                            {

        //                                //To save data in history table 
        //                                var Obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, ESOBJ, "Wages", ESOBJ.DBTrack);
        //                                DT_Wages DT_GRD = (DT_Wages)Obj;
        //                                db.DT_Wages.Add(DT_GRD);
        //                                db.SaveChanges();
        //                            }
        //                            ts.Complete();
        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = data, Val = ESOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                            // return Json(new Object[] { ESOBJ.Id, ESOBJ.Name, "Record Updated", JsonRequestBehavior.AllowGet });
        //                        }

        //                        catch (DbUpdateConcurrencyException ex)
        //                        {
        //                            var entry = ex.Entries.Single();
        //                            var clientValues = (Grade)entry.Entity;
        //                            var databaseEntry = entry.GetDatabaseValues();
        //                            if (databaseEntry == null)
        //                            {
        //                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
        //                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                            }
        //                            else
        //                            {
        //                                var databaseValues = (Grade)databaseEntry.ToObject();
        //                                ESOBJ.RowVersion = databaseValues.RowVersion;
        //                            }
        //                        }

        //                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                    }
        //                }
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
        //        return View();
        //    }
        //}
        [HttpPost]
        public ActionResult EditSave(Wages ContDetails, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var db_data = db.Wages.Include(e => e.RateMaster).Where(e => e.Id == data).SingleOrDefault();
                    List<RateMaster> contactNos = new List<RateMaster>();
                    string Values = form["RateMasterlist"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var Contact_Nos = db.RateMaster.Find(ca);
                            contactNos.Add(Contact_Nos);
                            db_data.RateMaster = contactNos;
                        }
                    }
                    else
                    {
                        db_data.RateMaster = null;
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            db.Wages.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = db_data.RowVersion;
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                            Wages blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            using (var context = new DataBaseContext())
                            {
                                blog = context.Wages.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }

                            ContDetails.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            Wages ContactDet = new Wages()
                             {
                                 CeilingMin = ContDetails.CeilingMin,
                                 CeilingMax = ContDetails.CeilingMax,
                                 Percentage = ContDetails.Percentage,
                                 RateMaster = ContDetails.RateMaster,
                                 WagesName = ContDetails.WagesName,
                                 Id = data,
                                 DBTrack = ContDetails.DBTrack
                             };

                            try
                            {
                                db.Entry(ContactDet).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(ContactDet).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                db.SaveChanges();
                                ts.Complete();
                                //return this.Json(new { msg = "Data saved successfully." });
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = ContactDet.Id, Val = ContactDet.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = ContactDet.Id });
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
                        return this.Json(new { msg = errorMsg, JsonRequestBehavior.AllowGet });
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


        public int EditS(string RateMast, int data, Wages NOBJ, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (RateMast != null && RateMast != "")
                {

                    List<int> IDs = RateMast.Split(',').Select(e => int.Parse(e)).ToList();
                    foreach (var k in IDs)
                    {
                        var value = db.RateMaster.Find(k);
                        NOBJ.RateMaster = new List<RateMaster>();
                        NOBJ.RateMaster.Add(value);
                    }
                }
                else
                {
                    var RateMasterdetails = db.Wages.Include(e => e.RateMaster).Where(x => x.Id == data).ToList();
                    foreach (var s in RateMasterdetails)
                    {
                        s.RateMaster = null;
                        db.Wages.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                var CurOBJ = db.Wages.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    NOBJ.DBTrack = dbT;
                    Wages TOBJ = new Wages()
                    {
                        CeilingMin = NOBJ.CeilingMin,
                        CeilingMax = NOBJ.CeilingMax,
                        Percentage = NOBJ.Percentage,
                        RateMaster = NOBJ.RateMaster,
                        WagesName = NOBJ.WagesName,
                        Id = data,
                        DBTrack = NOBJ.DBTrack
                    };


                    db.Wages.Attach(TOBJ);
                    db.Entry(TOBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(TOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }

        public class wages_rate
        {
            public string CeilingMax { get; set; }
            public string CeilingMin { get; set; }
            public string Percentage { get; set; }
            public string WagesName { get; set; }
            public Array rate_id { get; set; }
            public Array RateCode { get; set; }
        }


        //public ActionResult edit(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<wages_rate> r = new List<wages_rate>();
        //        var id = Convert.ToInt32(data);
        //        var return_value = db.Wages.Include(e => e.RateMaster).Where(e => e.Id == id).Select(e => new { e.Percentage, e.WagesName, e.CeilingMin, e.CeilingMax, e.RateMaster }).ToList();
        //        foreach (var ca in return_value)
        //        {
        //            r.Add(new wages_rate
        //            {
        //                WagesName = ca.WagesName,
        //                CeilingMax = ca.CeilingMax.ToString(),
        //                CeilingMin = ca.CeilingMin.ToString(),
        //                Percentage = ca.Percentage.ToString(),
        //                rate_id = ca.RateMaster.Select(e => e.Id).ToArray(),
        //                RateCode = ca.RateMaster.Select(e => e.Code).ToArray(),
        //            });
        //        }
        //        return Json(r, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public class RateMaster_Val
        {
            public Array Rate_id { get; set; }
            public Array Rate_val { get; set; }
        }

        //[HttpPost]
        //public ActionResult Edit(int data)
        //{
        //    //string tableName = "Corporate";

        //    //    // Fetch the table records dynamically
        //    //    var tableData = db.GetType()
        //    //    .GetProperty(tableName)
        //    //    .GetValue(db, null);
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var Q = db.Wages.Where(e => e.Id == data)
        //            .Select(e => new
        //            {
        //                Percentage = e.Percentage,
        //                WagesName = e.WagesName,
        //                CeilingMin = e.CeilingMin,
        //                CeilingMax = e.CeilingMax,
        //                // RateMaster= e.RateMaster,
        //                Action = e.DBTrack.Action
        //            }).ToList();

        //        List<RateMaster_Val> return_data = new List<RateMaster_Val>();
        //        var add_data = db.Wages.Include(e => e.RateMaster).Where(e => e.Id == data).ToList();

        //        foreach (var ca in add_data)
        //        {
        //            return_data.Add(
        //            new RateMaster_Val
        //            {
        //                Rate_id = ca.RateMaster.Count > 0 ? ca.RateMaster.Select(e => e.Id.ToString()).ToArray() : null,
        //                Rate_val = ca.RateMaster.Count > 0 ? ca.RateMaster.Select(e => e.FullDetails.ToString()).ToArray() : null,
        //            });
        //        }
        //        var W = db.DT_RateMaster
        //            .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
        //            (e => new
        //            {
        //                DT_Id = e.Id,
        //                Code = e.Code == null ? "" : e.Code,
        //            }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

        //        var Corp = db.Wages.Find(data);
        //        TempData["RowVersion"] = Corp.RowVersion;
        //        var Auth = Corp.DBTrack.IsModified;
        //        return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
        //    }
        //}

        [HttpPost]
        public ActionResult Edit(int data)
        { 
            using (DataBaseContext db = new DataBaseContext())
            {
                var Disease = db.Wages.Include(e => e.RateMaster).Where(e => e.Id == data).ToList();
                var r = (from e in Disease
                         select new
                         {
                             Id = e.Id,
                             Percentage = e.Percentage,
                             WagesName = e.WagesName,
                             CeilingMin = e.CeilingMin,
                             CeilingMax = e.CeilingMax,
                             // RateMaster= e.RateMaster,
                             Action = e.DBTrack.Action
                         }).Distinct();

                var k = db.Wages.Include(e => e.RateMaster).Where(e => e.Id == data).Select(e => e.RateMaster).SingleOrDefault();
                var BCDETAILS = (from ca in k
                                 select new
                                 {
                                     Id = ca.Id,
                                     Rate_id = ca.Id,
                                     Rate_val = ca.FullDetails
                                 }).Distinct();
               
                var Old_Data = db.DT_Wages
                 .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M")
                 .Select
                 (e => new
                 {
                     DT_Id = e.Id,
                                        
                 }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.Wages.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { r, BCDETAILS, Old_Data, Auth, JsonRequestBehavior.AllowGet });
           }
        }

        public ActionResult delete(string data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var id = Convert.ToInt32(data);
                    var qurey = db.Wages.Include(e => e.RateMaster).Where(e => e.Id == id).SingleOrDefault();
                    db.Wages.Attach(qurey);
                    db.Entry(qurey).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                    db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                    Msg.Add("  Record removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                Msg.Add("  Data removed successfully.  ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                // return Json(new { msg = "Record Deleted", JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public ActionResult CreateSave(Wages Wage, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (Wage.Percentage != null)
                    {
                        if (Wage.Percentage > 100)
                        {
                            Msg.Add(" Enter percentage below 100%  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Enter percentage below 100%", JsonRequestBehavior.AllowGet });
                        }
                    }
                    if (ModelState.IsValid)
                    {
                        string Rate_ids = form["RateMasterlist"];

                        if (Rate_ids != "" && Rate_ids != null)
                        {
                            var ids = Utility.StringIdsToListIds(Rate_ids);

                            Wage.RateMaster = new List<RateMaster>();
                            if (ids != null)
                            {
                                foreach (var value in ids)
                                {
                                    var itsub_val = db.RateMaster.Find(value);
                                    Wage.RateMaster.Add(itsub_val);
                                }
                            }
                        }

                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.Wages.Any(o => o.WagesName == Wage.WagesName))
                            {
                                Msg.Add("  Wages Name Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Wages Name Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                            Wage.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };


                            Wages wages = new Wages()
                            {
                                WagesName = Wage.WagesName,
                                CeilingMax = Wage.CeilingMax,
                                Percentage = Wage.Percentage,
                                RateMaster = Wage.RateMaster,
                                CeilingMin = Wage.CeilingMin,
                                DBTrack = Wage.DBTrack
                            };

                            try
                            {
                                db.Wages.Add(wages);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, Wage.DBTrack);
                                DT_Wages DT_Wages = (DT_Wages)rtn_Obj;
                                db.Create(DT_Wages);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = wages.Id, Val = wages.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return Json(new Object[] { wages.Id, wages.FullDetails, "Record Succesfully Created" });
                            }
                            catch (DBConcurrencyException e) { throw e; }
                            catch (DbUpdateException e) { throw e; }
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
                var RateMaster = db.Wages.ToList();
                IEnumerable<Wages> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = RateMaster;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.WagesName), Convert.ToString(a.CeilingMin), Convert.ToString(a.CeilingMax) }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.WagesName), Convert.ToString(a.CeilingMin), Convert.ToString(a.CeilingMax) }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = RateMaster;
                    Func<Wages, string> orderfuc = (OBJ =>
                                                               gp.sidx == "Id" ? OBJ.Id.ToString() :
                                                               gp.sidx == "RateCode" ? OBJ.WagesName :
                                                               gp.sidx == "Percentage" ? OBJ.Percentage.ToString() :
                                                                gp.sidx == "Amount" ? OBJ.CeilingMin.ToString() :
                                                               gp.sidx == "SalHead" ? OBJ.CeilingMax.ToString() : "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.WagesName), Convert.ToString(a.CeilingMin), Convert.ToString(a.CeilingMax) }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.WagesName), Convert.ToString(a.CeilingMin), Convert.ToString(a.CeilingMax) }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.WagesName), Convert.ToString(a.CeilingMin), Convert.ToString(a.CeilingMax) }).ToList();
                    }
                    totalRecords = RateMaster.Count();
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


        public ActionResult GetWagesLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.Wages.Include(e => e.RateMaster).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Wages.Include(e => e.RateMaster).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }


                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }
    }
}
