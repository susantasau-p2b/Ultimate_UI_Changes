///
/// Created by Sarika
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
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class NomineeBenefitController : Controller
    {

        //private DataBaseContext db = new DataBaseContext();

        //
        // GET: /NomineeBenefit/
        public ActionResult Index()
        {
            return View("~/Views/Shared/Core/_Nomines_benifits.cshtml");
        }


        private MultiSelectList GetContactNos(List<int> selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<ContactNumbers> Nos = new List<ContactNumbers>();
                Nos = db.ContactNumbers.ToList();
                return new MultiSelectList(Nos, "Id", "FullContactNumbers", selectedValues);
            }
        }

        [HttpPost]
        public ActionResult Create(NomineeBenefit nb, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Category = form["Type_Nomines"] == "0" ? "" : form["Type_Nomines"];

                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "311").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Category)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(Category));
                            nb.BenefitType = val;
                        }

                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            nb.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };
                            NomineeBenefit nbs = new NomineeBenefit()
                            {
                                BenefitPerc = nb.BenefitPerc,
                                BenefitType = nb.BenefitType,
                                DBTrack = nb.DBTrack
                            };
                            try
                            {
                                db.NomineeBenefit.Add(nbs);
                                // DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, look.DBTrack);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = nbs.Id, Val = nbs.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { nbs.Id, nbs.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = nbs.Id });
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
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var un = db.NomineeBenefit.Include(e => e.BenefitType).Where(e => e.Id == data).Select
                    (e => new
                    {
                        BenefitType_Id = e.BenefitType.Id == null ? 0 : e.BenefitType.Id,
                        BenefitPerc = e.BenefitPerc == null ? 0 : e.BenefitPerc,

                        Action = e.DBTrack.Action
                    }).ToList();



                var k = db.NomineeBenefit.Include(e => e.BenefitType).Where(e => e.Id == data).ToList();



                //var W = db.DT_NomineeBenefit
                //    //.Include(e => e.IncrPolicy)
                //    //.Include(e => e.IncrList)
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,
                //         BenefitType_Val = e.BenefitType_Id == 0 ? "" : db.LookupValue
                //                    .Where(x => x.Id == e.BenefitType_Id)
                //                    .Select(x => x.LookupVal).FirstOrDefault(),

                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                //var Corp = db.LanguageSkill.Find(data);
                //TempData["RowVersion"] = Corp.RowVersion;
                //var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { un, JsonRequestBehavior.AllowGet });
            }
        }

        //[HttpPost]
        //public async Task<ActionResult> EditSave(NomineeBenefit c, int data, FormCollection form) // Edit submit
        //{
        //    var db_data = db.NomineeBenefit.Include(e => e.BenefitType)

        //                .Where(e => e.Id == data).SingleOrDefault();

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
        //                    db.NomineeBenefit.Attach(db_data);
        //                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //                    db.SaveChanges();
        //                    TempData["RowVersion"] = db_data.RowVersion;
        //                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

        //                    var Curr_Lookup = db.NomineeBenefit.Find(data);
        //                    TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
        //                    db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

        //                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                    {

        //                        NomineeBenefit blog = null; // to retrieve old data
        //                        DbPropertyValues originalBlogValues = null;

        //                        using (var context = new DataBaseContext())
        //                        {
        //                            blog = context.NomineeBenefit.Where(e => e.Id == data).Include(e => e.BenefitType)

        //                  .SingleOrDefault();
        //                            originalBlogValues = context.Entry(blog).OriginalValues;
        //                        }

        //                        c.DBTrack = new DBTrack
        //                        {
        //                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                            Action = "M",
        //                            ModifiedBy = SessionManager.UserName,
        //                            ModifiedOn = DateTime.Now
        //                        };

        //                        int a = EditS(db_data, data, c, c.DBTrack);

        //                       // NomineeBenefit lk = new NomineeBenefit
        //                       // {
        //                       //     Id = data,
        //                       //     BenefitPerc = db_data.BenefitPerc,
        //                       //     BenefitType = db_data.BenefitType,
        //                       //     DBTrack = c.DBTrack
        //                       // };


        //                       // db.NomineeBenefit.Attach(lk);
        //                       // db.Entry(lk).State = System.Data.Entity.EntityState.Modified;
        //                       // db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                       //// int a = EditS(db_data,data, c, c.DBTrack);

        //                       // // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


        //                        using (var context = new DataBaseContext())
        //                        {

        //                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                            DT_NomineeBenefit DT_Corp = (DT_NomineeBenefit)obj;

        //                            db.Create(DT_Corp);
        //                            db.SaveChanges();
        //                        }
        //                        await db.SaveChangesAsync();
        //                        ts.Complete();


        //                        return Json(new Object[] { c.Id,c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
        //                    }
        //                }
        //            }
        //            catch (DbUpdateConcurrencyException ex)
        //            {
        //                var entry = ex.Entries.Single();
        //                var clientValues = (NomineeBenefit)entry.Entity;
        //                var databaseEntry = entry.GetDatabaseValues();
        //                if (databaseEntry == null)
        //                {
        //                    return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                }
        //                else
        //                {
        //                    var databaseValues = (NomineeBenefit)databaseEntry.ToObject();
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

        //            NomineeBenefit blog = null; // to retrieve old data
        //            DbPropertyValues originalBlogValues = null;
        //            NomineeBenefit Old_Corp = null;

        //            using (var context = new DataBaseContext())
        //            {
        //                blog = context.NomineeBenefit.Where(e => e.Id == data).SingleOrDefault();
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
        //            NomineeBenefit empAcademicInfo = new NomineeBenefit()
        //            {

        //                Id = data,
        //                BenefitType = c.BenefitType,
        //                BenefitPerc=c.BenefitPerc,
        //                DBTrack = c.DBTrack
        //            };

        //            db.Entry(blog).State = System.Data.Entity.EntityState.Detached;
        //            empAcademicInfo.DBTrack = c.DBTrack;
        //            // db.EmpAcademicInfo.Attach(empAcademicInfo);                   
        //            //db.Entry(empAcademicInfo).State = System.Data.Entity.EntityState.Modified;
        //            //db.Entry(empAcademicInfo).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            db.SaveChanges();
        //            ts.Complete();
        //            return Json(new Object[] { blog.Id, "", "Record Updated", JsonRequestBehavior.AllowGet });
        //        }
        //    }
        //    return View();
        //}
        [HttpPost]
        public async Task<ActionResult> EditSave1(NomineeBenefit c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Corp = form["Type_Nomines"] == "0" ? "" : form["Type_Nomines"];

                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    if (Corp != null)
                    {
                        if (Corp != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "311").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Corp)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(Corp));
                            c.BenefitType = val;
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
                                    NomineeBenefit blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.NomineeBenefit.Where(e => e.Id == data)
                                                                      .Include(e => e.BenefitType)
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

                                    int a = EditS(Corp, data, c, c.DBTrack);



                                    using (var context = new DataBaseContext())
                                    {
                                        //c.Id = data;

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


                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_NomineeBenefit DT_Corp = (DT_NomineeBenefit)obj;

                                        DT_Corp.BenefitType_Id = blog.BenefitType == null ? 0 : blog.BenefitType.Id;

                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return Json(new Object[] { c.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (NomineeBenefit)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (NomineeBenefit)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            NomineeBenefit blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            NomineeBenefit Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.NomineeBenefit.Where(e => e.Id == data).SingleOrDefault();
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

                            NomineeBenefit corp = new NomineeBenefit()
                            {

                                BenefitPerc = c.BenefitPerc,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Corporate", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.NomineeBenefit.Where(e => e.Id == data).Include(e => e.BenefitType)
                                   .SingleOrDefault();
                                DT_NomineeBenefit DT_Corp = (DT_NomineeBenefit)obj;

                                DT_Corp.BenefitType_Id = DBTrackFile.ValCompare(Old_Corp.BenefitType, c.BenefitType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;

                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.NomineeBenefit.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { blog.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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
        public async Task<ActionResult> EditSave(NomineeBenefit c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.NomineeBenefit.Include(e => e.BenefitType)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;



                    string Corp = form["Type_Nomines"] == "0" ? "" : form["Type_Nomines"];


                    if (Corp != null)
                    {
                        if (Corp != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Corp));
                            c.BenefitType = val;
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
                                    db.NomineeBenefit.Attach(db_data);
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_data.RowVersion;
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_Lookup = db.NomineeBenefit.Find(data);
                                    TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                    db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        NomineeBenefit blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.NomineeBenefit.Where(e => e.Id == data).Include(e => e.BenefitType)
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
                                        NomineeBenefit lk = new NomineeBenefit
                                        {
                                            Id = data,
                                            BenefitType = c.BenefitType,
                                            BenefitPerc = c.BenefitPerc,
                                            DBTrack = c.DBTrack
                                        };


                                        db.NomineeBenefit.Attach(lk);
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                        db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

                                        // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


                                        using (var context = new DataBaseContext())
                                        {

                                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                            DT_NomineeBenefit DT_Corp = (DT_NomineeBenefit)obj;

                                            db.Create(DT_Corp);
                                            db.SaveChanges();
                                        }
                                        await db.SaveChangesAsync();
                                        ts.Complete();

                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = lk.Id, Val = lk.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { lk.Id, "", "Record Updated", JsonRequestBehavior.AllowGet });
                                    }
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (NomineeBenefit)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (NomineeBenefit)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            NomineeBenefit blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            NomineeBenefit Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.NomineeBenefit.Include(e => e.BenefitType).Where(e => e.Id == data).SingleOrDefault();
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
                            NomineeBenefit qualificationDetails = new NomineeBenefit()
                            {

                                Id = data,
                                BenefitType = c.BenefitType,
                                BenefitPerc = c.BenefitPerc,
                                DBTrack = c.DBTrack
                            };

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, qualificationDetails, "NomineeBenefit", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.NomineeBenefit.Where(e => e.Id == data)
                                    .Include(e => e.BenefitType).SingleOrDefault();
                                DT_NomineeBenefit DT_Corp = (DT_NomineeBenefit)obj;
                                //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.NomineeBenefit.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();

                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, c.Institute, "Record Updated", JsonRequestBehavior.AllowGet });
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



        public int EditS(string db_data, int data, NomineeBenefit c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (db_data != null)
                {
                    if (db_data != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(db_data));
                        c.BenefitType = val;

                        var type = db.NomineeBenefit.Include(e => e.BenefitType).Where(e => e.Id == data).SingleOrDefault();
                        IList<NomineeBenefit> typedetails = null;
                        if (type.BenefitType != null)
                        {
                            typedetails = db.NomineeBenefit.Where(x => x.BenefitType.Id == type.BenefitType.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.NomineeBenefit.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.BenefitType = c.BenefitType;
                            db.NomineeBenefit.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.NomineeBenefit.Include(e => e.BenefitType).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.BenefitType = null;
                            db.NomineeBenefit.Attach(s);
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
                    var BusiTypeDetails = db.NomineeBenefit.Include(e => e.BenefitType).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.BenefitType = null;
                        db.NomineeBenefit.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                var CurCorp = db.NomineeBenefit.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    NomineeBenefit corp = new NomineeBenefit()
                    {

                        BenefitPerc = c.BenefitPerc,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.NomineeBenefit.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }


        public ActionResult GetlookupBenefit(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.NomineeBenefit.ToList();
                IEnumerable<NomineeBenefit> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.NomineeBenefit.ToList().Where(d => d.FullDetails.Contains(data));
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
            // return View();
        }


    }
}