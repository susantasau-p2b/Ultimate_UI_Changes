using P2b.Global;
using Payroll;
using EssPortal.App_Start;
using EssPortal.Models;
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
using System.Threading.Tasks;
using EssPortal.Security;
namespace EssPortal.Controllers.Core.MainController
{
    public class QualificationController : Controller
    {
        private DataBaseContext db = new DataBaseContext();
        public ActionResult partial()
        {
            return View("~/Views/Shared/_Qualification.cshtml");
        }
        public ActionResult Create(Qualification lkval, FormCollection form)
        {
            string Category = form["Qualificationlist1"] == "0" ? "" : form["Qualificationlist1"];


            if (Category != null)
            {
                if (Category != "")
                {
                    var val = db.LookupValue.Find(int.Parse(Category));
                    lkval.QualificationType = val;
                }
            }

            lkval.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };


            Qualification qualificn = new Qualification
            {
                QualificationType = lkval.QualificationType,
                QualificationShortName = lkval.QualificationShortName,
                QualificationDesc = lkval.QualificationDesc,
                FullDetails = lkval.FullDetails,
                DBTrack = lkval.DBTrack
            };
            try
            {
                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Qualification.Add(qualificn);
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", qualificn, null, "Qualification", null);
                        db.SaveChanges();
                        ts.Complete();
                    }
                }
                return Json(new Object[] { qualificn.Id, qualificn.QualificationShortName, "Record Created", JsonRequestBehavior.AllowGet });

            }
            catch (DataException e) { throw e; }
            catch (DBConcurrencyException e) { throw e; }
        }
        public ActionResult delete(int? data)
        {
            var qurey = db.Qualification.Find(data);
            using (TransactionScope ts = new TransactionScope())
            {
                //db.LookupValue.Attach(qurey);
                //db.Entry(qurey).State = System.Data.Entity.EntityState.Deleted;
                //db.SaveChanges();
                //db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                //ts.Complete();

                //DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = "0029", ModifiedOn = DateTime.Now };
                db.Entry(qurey).State = System.Data.Entity.EntityState.Deleted;
                //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                db.SaveChanges();
                ts.Complete();
                return this.Json(new { msg = "Data removed.", JsonRequestBehavior.AllowGet });
            }
        }
        public ActionResult edit(int? data)
        {
            var qurey = db.Qualification.Where(e => e.Id == data).ToList();
            var adddata = db.Qualification.Include(e => e.QualificationType).Where(e => e.Id == data).Select(e => new
            {
                QualificationShortName = e.QualificationShortName,
                QualificationDesc = e.QualificationDesc,
                BusinessType_Id = e.QualificationType.Id == null ? 0 : e.QualificationType.Id,
            });

            // TempData["RowVersion"] = db.StagIncrPolicy.Find(data).RowVersion;
            // return Json(qurey, JsonRequestBehavior.AllowGet);
            return Json(adddata, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(Qualification c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.Qualification.Include(a => a.QualificationType).Where(e => e.Id == data).SingleOrDefault();
                    string Category = form["Qualificationlist1"] == "0" ? "" : form["Qualificationlist1"];


                    //if (Category != null)
                    //{
                    //    if (Category != "")
                    //    {
                    //        var val = db.LookupValue.Find(int.Parse(Category));
                    //        c.QualificationType = val;
                    //    }
                    //}
                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Category));
                            db_data.QualificationType = val;

                            var type = db.Qualification.Include(e => e.QualificationType).Where(e => e.Id == data).SingleOrDefault();
                            IList<Qualification> typedetails = null;
                            if (type.QualificationType != null)
                            {
                                typedetails = db.Qualification.Where(x => x.QualificationType.Id == type.QualificationType.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.Qualification.Where(x => x.Id == data).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.QualificationType = db_data.QualificationType;
                                db.Qualification.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                    bool Auth = form["autho_allow"] == "true" ? true : false;


                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    db.Qualification.Attach(db_data);
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_data.RowVersion;
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_Lookup = db.Qualification.Find(data);
                                    TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                    db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        Qualification blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.Qualification.Include(a => a.QualificationType).Where(e => e.Id == data)
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
                                        Qualification lk = new Qualification
                                        {

                                            QualificationType = c.QualificationType,
                                            QualificationShortName = c.QualificationShortName,
                                            QualificationDesc = c.QualificationDesc,
                                            Id = data,
                                            DBTrack = c.DBTrack,

                                        };


                                        db.Qualification.Attach(lk);
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                        db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

                                        // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


                                        using (var context = new DataBaseContext())
                                        {

                                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                            DT_Qualification DT_Corp = (DT_Qualification)obj;

                                            db.Create(DT_Corp);
                                            db.SaveChanges();
                                        }
                                        await db.SaveChangesAsync();
                                        ts.Complete();

                                        Msg.Add("Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = lk.Id, Val = lk.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { lk.Id, "", "Record Updated", JsonRequestBehavior.AllowGet });
                                    }
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Qualification)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (Qualification)databaseEntry.ToObject();
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

                            Qualification blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            Qualification Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.Qualification.Include(e => e.QualificationType).Where(e => e.Id == data).SingleOrDefault();
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
                            Qualification qualificationDetails = new Qualification()
                            {

                                Id = data,
                                QualificationType = c.QualificationType,
                                QualificationShortName = c.QualificationShortName,
                                QualificationDesc = c.QualificationDesc,
                                DBTrack = c.DBTrack
                            };

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, qualificationDetails, "Qualification", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.Qualification.Include(e => e.QualificationType).Where(e => e.Id == data)
                                 .SingleOrDefault();
                                DT_Qualification DT_Corp = (DT_Qualification)obj;
                                //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.Qualification.Attach(blog);
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
        public int EditS(int data, Qualification c, DBTrack dbT)
        {
            var CurCorp = db.Qualification.Find(data);
            TempData["CurrRowVersion"] = CurCorp.RowVersion;
            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
            {
                c.DBTrack = dbT;
                var db_data = db.Qualification.Include(e => e.QualificationType).Where(e => e.Id == data).SingleOrDefault();


                db_data.QualificationShortName = c.QualificationShortName;
                db_data.QualificationDesc = c.QualificationDesc;
                db_data.QualificationType = c.QualificationType;
                db_data.DBTrack = c.DBTrack;


                db.Qualification.Attach(db_data);
                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                db.Entry(db_data).OriginalValues["RowVersion"] = TempData["RowVersion"];
                db.SaveChanges();
                //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                return 1;
            }
            return 0;
        }
    }
}