﻿///
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
using Appraisal;

namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class PassportDetailsController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /PassportDetails/

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/PassportDetails/Index.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(PassportDetails COBJ, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                try
                {
                    if (ModelState.IsValid)
                    {
                        Employee empdata;
                        if (Emp != null && Emp != 0)
                        {
                            empdata = db.Employee.Find(Emp);
                        }
                        else
                        {
                            Msg.Add("  Kindly select employee  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }


                        using (TransactionScope ts = new TransactionScope())
                        {
                            var EmpSocialInfo1 = db.Employee.Include(e => e.PassportDetails)
                                              .Where(e => e.Id != null).ToList();
                            foreach (var item in EmpSocialInfo1)
                            {
                                if (item.PassportDetails.Count != 0 && empdata.PassportDetails.Count != 0)
                                {
                                    int aid = item.PassportDetails.Select(a => a.Id).FirstOrDefault();
                                    int bid = empdata.PassportDetails.Select(a => a.Id).FirstOrDefault();

                                    if (aid == bid)
                                    {
                                        var v = empdata.EmpCode;
                                        Msg.Add("Record Already Exist For Employee Code=" + v);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }

                            }

                            if (COBJ.IssueDate > COBJ.ExpiryDate)
                            {
                                Msg.Add(" Issue Date Should be less than Expiry Date.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { null, null, "Issue Date Should be less than Expiry Date.", JsonRequestBehavior.AllowGet });
                            }
                            DateTime current = DateTime.Now;
                            if (COBJ.IssueDate > current)
                            {

                                Msg.Add("  Issue Date Should be less than Current Date.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            PassportDetails PassportDetails = new PassportDetails()
                            {

                                PassportNo = COBJ.PassportNo == null ? "" : COBJ.PassportNo,
                                IssuePlace = COBJ.IssuePlace == null ? "" : COBJ.IssuePlace,
                                IssueDate = COBJ.IssueDate,
                                ExpiryDate = COBJ.ExpiryDate,
                                DBTrack = COBJ.DBTrack
                            };
                            try
                            {
                                db.PassportDetails.Add(PassportDetails);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, COBJ.DBTrack);
                                DT_PassportDetails DT_OBJ = (DT_PassportDetails)rtn_Obj;
                                db.Create(DT_OBJ);
                                db.SaveChanges();

                                List<PassportDetails> empPassportDetails = new List<PassportDetails>();
                                empPassportDetails.Add(PassportDetails);
                                empdata.PassportDetails = empPassportDetails;
                                db.Employee.Attach(empdata);
                                db.Entry(empdata).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                                ts.Complete();
                                Msg.Add("  Data Created successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { null, null, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = COBJ.Id });
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
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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

        //[HttpPost]
        //public ActionResult Edit(int data)
        //{
        //    var PassportDetails = db.PassportDetails
        //                        .Where(e => e.Id == data).ToList();
        //    var r = (from ca in PassportDetails
        //             select new
        //             {

        //                 Id = ca.Id,
        //                 PassportNo = ca.PassportNo,
        //                 IssuePlace = ca.IssuePlace,
        //                 IssueDate = ca.IssueDate,
        //                 ExpiryDate = ca.ExpiryDate,
        //                 Action = ca.DBTrack.Action
        //             }).Distinct();

        //    var a = "";

        //    var W = db.DT_PassportDetails
        //         .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
        //         (e => new
        //         {
        //             DT_Id = e.Id,
        //             PassportNo = e.PassportNo == null ? "" : e.PassportNo,
        //             IssueDate=e.IssueDate,
        //             ExpiryDate=e.ExpiryDate,
        //             IssuePlace = e.IssuePlace == null ? "" : e.IssuePlace.ToString(),
        //         }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
        //    var LKup = db.PassportDetails.Find(data);
        //    TempData["RowVersion"] = LKup.RowVersion;
        //    var Auth = LKup.DBTrack.IsModified;

        //    return this.Json(new Object[] { r, a, W, Auth, JsonRequestBehavior.AllowGet });
        //}

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Passport = db.Employee.Include(e => e.PassportDetails)
                                    .Where(e => e.Id == data).SingleOrDefault();

                int AID = Passport.PassportDetails.Select(s => s.Id).SingleOrDefault();

                var r = (from ca in Passport.PassportDetails
                         select new
                         {

                             Id = ca.Id,
                             PassportNo = ca.PassportNo,
                             IssuePlace = ca.IssuePlace,
                             IssueDate = ca.IssueDate,
                             ExpiryDate = ca.ExpiryDate,
                             Action = ca.DBTrack.Action
                         }).Distinct();

                var a = "";

                var W = db.DT_PassportDetails
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         PassportNo = e.PassportNo == null ? "" : e.PassportNo,
                         IssueDate = e.IssueDate,
                         ExpiryDate = e.ExpiryDate,
                         IssuePlace = e.IssuePlace == null ? "" : e.IssuePlace.ToString(),
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var LKup = db.PassportDetails.Find(AID);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, a, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> EditSave1(PassportDetails ESOBJ, FormCollection form, int data)
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
                                    PassportDetails blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.PassportDetails.Where(e => e.Id == data)
                                                                .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    ESOBJ.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    int a = EditS(data, ESOBJ, ESOBJ.DBTrack);



                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave(" Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                        DT_PassportDetails DT_OBJ = (DT_PassportDetails)obj;
                                        db.Create(DT_OBJ);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.PassportNo, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { ESOBJ.Id, ESOBJ.PassportNo, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }

                            //catch (DbUpdateException e) { throw e; }
                            //catch (DataException e) { throw e; }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (PassportDetails)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (PassportDetails)databaseEntry.ToObject();
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

                            // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        }
                    }


                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            PassportDetails blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            PassportDetails Old_Obj = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.PassportDetails.Where(e => e.Id == data).SingleOrDefault();
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
                            PassportDetails corp = new PassportDetails()
                            {
                                PassportNo = ESOBJ.PassportNo,
                                IssuePlace = ESOBJ.IssuePlace,
                                IssueDate = ESOBJ.IssueDate,
                                ExpiryDate = ESOBJ.ExpiryDate,
                                Id = data,
                                DBTrack = ESOBJ.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "PassportDetails", ESOBJ.DBTrack);
                                //var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);

                                Old_Obj = context.PassportDetails.Where(e => e.Id == data)
                                     .SingleOrDefault();
                                DT_PassportDetails DT_Corp = (DT_PassportDetails)obj;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = ESOBJ.DBTrack;
                            db.PassportDetails.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = ESOBJ.PassportNo, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { blog.Id, ESOBJ.PassportNo, "Record Updated", JsonRequestBehavior.AllowGet });
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
        [HttpPost]
        public async Task<ActionResult> EditSave(PassportDetails ESOBJ, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var Passport = db.Employee.Include(e => e.PassportDetails)
                           .Where(e => e.Id == data).SingleOrDefault();
                    int AID = Passport.PassportDetails.Select(s => s.Id).SingleOrDefault();
                    //   int AIDD = int.Parse(AID);
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
                                    if (ESOBJ.IssueDate > ESOBJ.ExpiryDate)
                                    {
                                        Msg.Add(" Issue Date Should be less than Expiry Date.  ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return this.Json(new Object[] { null, null, "Issue Date Should be less than Expiry Date.", JsonRequestBehavior.AllowGet });
                                    }

                                    PassportDetails blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.PassportDetails.Where(e => e.Id == AID)
                                                               .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    ESOBJ.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                   // int a = EditS(AID, ESOBJ, ESOBJ.DBTrack);
                                    var CurOBJ = db.PassportDetails.Find(AID);
                                    TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                                    db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                       // ESOBJ.DBTrack = dbT;
                                        PassportDetails ESIOBJ = new PassportDetails()
                                        {
                                            Id = AID,
                                            PassportNo = ESOBJ.PassportNo,
                                            IssuePlace = ESOBJ.IssuePlace,
                                            IssueDate = ESOBJ.IssueDate,
                                            ExpiryDate = ESOBJ.ExpiryDate,
                                            DBTrack = ESOBJ.DBTrack
                                        };

                                        db.PassportDetails.Attach(ESIOBJ);
                                        db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
 
                                    }
                                    //await db.SaveChangesAsync();

                                    using (var context = new DataBaseContext())
                                    {
                                        ////var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, ESOBJ, "VisaDetails", ESOBJ.DBTrack);
                                        ////DT_VisaDetails DT_VTD = (DT_VisaDetails)Obj;
                                        ////db.DT_VisaDetails.Add(DT_VTD);

                                        //var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                        //DT_VisaDetails DT_OBJ = (DT_VisaDetails)obj;
                                        ////DT_OBJ.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
                                        //DT_OBJ.VisaType_Id = blog.VisaType == null ? 0 : blog.VisaType.Id;
                                        //DT_OBJ.Country_Id = blog.Country == null ? 0 : blog.Country.Id;
                                        //db.Create(DT_OBJ);
                                        //db.SaveChanges();
                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                        DT_PassportDetails DT_OBJ = (DT_PassportDetails)obj;

                                        db.Create(DT_OBJ);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { ESOBJ.Id, ESOBJ.IssueAt, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (PassportDetails)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (PassportDetails)databaseEntry.ToObject();
                                    ESOBJ.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        //    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        //    {
                        //        VisaDetails Old_OBJ = db.VisaDetails.Include(e => e.VisaType)
                        //                                            .Include(e => e.Country)
                        //                                           .Where(e => e.Id == data).SingleOrDefault();

                        //        VisaDetails Curr_OBJ = ESOBJ;
                        //        ESOBJ.DBTrack = new DBTrack
                        //        {
                        //            CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
                        //            CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
                        //            Action = "M",
                        //            IsModified = Old_OBJ.DBTrack.IsModified == true ? true : false,
                        //            //ModifiedBy = SessionManager.UserName,
                        //            //ModifiedOn = DateTime.Now
                        //        };
                        //        Old_OBJ.DBTrack = ESOBJ.DBTrack;

                        //        db.Entry(Old_OBJ).State = System.Data.Entity.EntityState.Modified;
                        //        db.SaveChanges();
                        //        using (var context = new DataBaseContext())
                        //        {
                        //            //DBTrackFile.DBTrackSave("Core/P2b.Global", "M", Old_OBJ, Curr_OBJ, "VisaDetails", ESOBJ.DBTrack);
                        //        }

                        //        ts.Complete();
                        //        return Json(new Object[] { Old_OBJ.Id, ESOBJ.IssueDate, "Record Updated", JsonRequestBehavior.AllowGet });
                        //    }

                        //}
                        //return View();


                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            PassportDetails blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            PassportDetails Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.PassportDetails.Where(e => e.Id == AID).SingleOrDefault();
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
                            PassportDetails corp = new PassportDetails()
                            {
                                PassportNo = ESOBJ.PassportNo,
                                IssuePlace = ESOBJ.IssuePlace,
                                IssueDate = ESOBJ.IssueDate,
                                ExpiryDate = ESOBJ.ExpiryDate,
                                Id = AID,
                                DBTrack = ESOBJ.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "PassportDetails", ESOBJ.DBTrack);
                                Old_Corp = context.PassportDetails.Where(e => e.Id == AID)
                                    .SingleOrDefault();
                                DT_PassportDetails DT_Corp = (DT_PassportDetails)obj;
                                db.Create(DT_Corp);
                            }
                            blog.DBTrack = ESOBJ.DBTrack;
                            db.PassportDetails.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, data = ESOBJ.Id.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { blog.Id, ESOBJ.Id, "Record Updated", JsonRequestBehavior.AllowGet });
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

        public int EditS(int AID, PassportDetails ESOBJ, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CurOBJ = db.PassportDetails.Find(AID);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    ESOBJ.DBTrack = dbT;
                    PassportDetails ESIOBJ = new PassportDetails()
                    {
                        Id = AID,
                        PassportNo = ESOBJ.PassportNo,
                        IssuePlace = ESOBJ.IssuePlace,
                        IssueDate = ESOBJ.IssueDate,
                        ExpiryDate = ESOBJ.ExpiryDate,
                        DBTrack = ESOBJ.DBTrack
                    };

                    db.PassportDetails.Attach(ESIOBJ);
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
                var Passport = db.Employee.Include(e => e.PassportDetails)
                                  .Where(e => e.Id == data).SingleOrDefault();

                int AID = Passport.PassportDetails.Select(s => s.Id).SingleOrDefault();

                PassportDetails PassportDetails = db.PassportDetails.Where(e => e.Id == AID).SingleOrDefault();
                try
                {
                    //var selectedValues = PassportDetails.SocialActivities;
                    //var lkValue = new HashSet<int>(PassportDetails.SocialActivities.Select(e => e.Id));
                    //if (lkValue.Count > 0)
                    //{
                    //    return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                    //}

                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(PassportDetails).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }

                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                catch (DataException /* dex */)
                {
                    // return this.Json(new Object[] { "", "Unable to delete. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
                    Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        int ParentId = 2;
        //        var jsonData = (Object)null;
        //        var LKVal = db.PassportDetails.ToList();

        //        if (gp.IsAutho == true)
        //        {
        //            LKVal = db.PassportDetails.AsNoTracking().Where(e => e.DBTrack.IsModified == true).ToList();
        //        }
        //        else
        //        {
        //            LKVal = db.PassportDetails.AsNoTracking().ToList();
        //        }


        //        IEnumerable<PassportDetails> IE;
        //        if (!string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = LKVal;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.PassportNo, a.IssuePlace, a.IssueDate, a.ExpiryDate }).Where((e => (e.Id.ToString() == gp.searchString) || (e.PassportNo.ToLower() == gp.searchString.ToLower()) || (e.IssuePlace.ToString() == gp.searchString.ToLower()) || (e.IssueDate.ToString() == gp.searchString.ToLower()) || (e.ExpiryDate.ToString() == gp.searchString.ToLower())));
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {a.Id, Convert.ToString(a.PassportNo), Convert.ToString(a.IssuePlace), Convert.ToString(a.IssueDate), Convert.ToString(a.ExpiryDate)}).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = LKVal;
        //            Func<PassportDetails, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "PassportNo" ? c.PassportNo.ToString() :
        //                                 gp.sidx == "IssuePlace" ? c.IssuePlace.ToString() :

        //                                 "");
        //            }

        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.PassportNo), Convert.ToString(a.IssuePlace), Convert.ToString(a.IssueDate), Convert.ToString(a.ExpiryDate) }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.PassportNo), Convert.ToString(a.IssuePlace), Convert.ToString(a.IssueDate), Convert.ToString(a.ExpiryDate) }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.PassportNo, a.IssuePlace, a.IssueDate, a.ExpiryDate }).ToList();
        //            }
        //            totalRecords = LKVal.Count();
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

                IEnumerable<Employee> Employee = null;
                Employee = db.Employee.Include(q => q.PassportDetails).Include(q => q.EmpName).ToList();
                if (gp.IsAutho == true)
                {
                    Employee = db.Employee.Include(q => q.PassportDetails).Include(q => q.EmpName).Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    // Employee = db.Employee.Where(a=>a.Id==db.EmpAcademicInfo.Contains()).ToList();
                    Employee = db.Employee.Include(q => q.PassportDetails).Where(q => q.PassportDetails.Count > 0).ToList();
                }
                IEnumerable<Employee> IE;

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Employee;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString.ToString())) 
                                || (e.EmpName.FullNameFML.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.Id.ToString().Contains(gp.searchString.ToString())))
                            .Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Employee;
                    Func<Employee, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.EmpName.FullNameFML.ToString() :
                                          "");
                    }


                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    totalRecords = Employee.Count();
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
                            PassportDetails ESI = db.PassportDetails
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

                            db.PassportDetails.Attach(ESI);
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(ESI).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave(" Core/P2b.Global", null, db.ChangeTracker, ESI.DBTrack);
                            DT_PassportDetails DT_OBJ = (DT_PassportDetails)rtn_Obj;

                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorized");
                            return Json(new Utility.JsonReturnClass { Id = ESI.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { ESI.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        PassportDetails Old_OBJ = db.PassportDetails
                                                .Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_PassportDetails Curr_OBJ = db.DT_PassportDetails
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_OBJ != null)
                        {
                            PassportDetails PassportDetails = new PassportDetails();

                            PassportDetails.PassportNo = Curr_OBJ.PassportNo == null ? Old_OBJ.PassportNo : Curr_OBJ.PassportNo;
                            PassportDetails.IssuePlace = Curr_OBJ.IssuePlace == null ? Old_OBJ.IssuePlace : Curr_OBJ.IssuePlace;
                            PassportDetails.IssueDate = Curr_OBJ.IssueDate == null ? Old_OBJ.IssueDate : Curr_OBJ.IssueDate;
                            PassportDetails.ExpiryDate = Curr_OBJ.ExpiryDate == null ? Old_OBJ.ExpiryDate : Curr_OBJ.ExpiryDate;



                            //      corp.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        PassportDetails.DBTrack = new DBTrack
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

                                        int a = EditS(auth_id, PassportDetails, PassportDetails.DBTrack);

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        Msg.Add("  Record Authorized");
                                        return Json(new Utility.JsonReturnClass { Id = PassportDetails.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { PassportDetails.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (PassportDetails)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (PassportDetails)databaseEntry.ToObject();
                                        PassportDetails.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                        {
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        // return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //PassportDetails corp = db.Passport.Find(auth_id);
                            PassportDetails ESI = db.PassportDetails.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

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

                            db.PassportDetails.Attach(ESI);
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave(" Core/P2b.Global", null, db.ChangeTracker, ESI.DBTrack);
                            DT_PassportDetails DT_OBJ = (DT_PassportDetails)rtn_Obj;
                            //DT_OBJ.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            //DT_OBJ.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                            //DT_OBJ.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorized ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "Record Authorized", JsonRequestBehavior.AllowGet });
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
