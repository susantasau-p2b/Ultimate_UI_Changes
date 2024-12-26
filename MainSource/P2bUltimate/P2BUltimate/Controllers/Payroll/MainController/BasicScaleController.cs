
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
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class BasicScaleController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();


        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/BasicScale/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/_Lookupvalues.cshtml");
        }


        public String checkCode(String data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.BasicScale.Any(e => e.ScaleName == data);
                if (qurey == true)
                {
                    return "1";
                }
                else
                {
                    return "0";
                }
            }
        }

        [HttpPost]
        public ActionResult Create(BasicScale look, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    List<BasicScaleDetails> lookupval = new List<BasicScaleDetails>();
                    string Values = form["BSCALEDETAILS_List"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.BasicScaleDetails.Find(ca);
                            lookupval.Add(Lookup_val);
                            look.BasicScaleDetails = lookupval;
                        }
                    }
                    else
                    {
                        look.BasicScaleDetails = null;
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            //look.BasicScaleDetails = new List<LookupValue>();
                            //if (ViewBag.LookupVal != null)
                            //{
                            //    foreach (var val in ViewBag.LookupVal)
                            //    {
                            //        if (val.Selected == true)
                            //        {
                            //            var valToAdd = db.BasicScaleDetails.Find(int.Parse(val.Value));
                            //            look.BasicScaleDetails.Add(valToAdd);
                            //        }
                            //    }
                            //}

                            if (db.BasicScale.Any(o => o.ScaleName == look.ScaleName))
                            {
                                //return this.Json(new { msg = "Code already exists." });
                                Msg.Add("  ScaleName Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { null, null, "ScaleName already exists.", JsonRequestBehavior.AllowGet });

                            }

                            look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            BasicScale BasicScale = new BasicScale()
                            {
                                //Code = look. == null ? "" : look.Code.Trim(),
                                ScaleName = look.ScaleName == null ? "" : look.ScaleName.Trim(),
                                BasicScaleDetails = look.BasicScaleDetails,
                                DBTrack = look.DBTrack
                            };
                            try
                            {
                                db.BasicScale.Add(BasicScale);
                                var a = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, look.DBTrack);
                                DT_BasicScale DT_OBJ = (DT_BasicScale)a;
                                //ICollection<Int32> test = new Collection<Int32>();
                                //if (BasicScale.BasicScaleDetails.Count > 0)
                                //{
                                //    foreach (var i in BasicScale.BasicScaleDetails)
                                //    {
                                //        test.Add(i.Id);
                                //    }

                                //}
                                // DT_OBJ.BasicScaleDetails = test;
                                db.Create(DT_OBJ);
                                db.SaveChanges();


                                //for (int i = 0; i < IDs.Count; i++)
                                //{
                                //    var LkList = db.DT_LookupValue.Where(e => e.Orig_Id == 15).SingleOrDefault();
                                //    LkList.Lookup_Id = BasicScale.Id;
                                //    db.Entry(LkList).State = System.Data.Entity.EntityState.Modified;
                                //    db.SaveChanges();
                                //}


                                //// db.BasicScale.Add(BasicScale);

                                //////// DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, look.DBTrack);
                                //// db.SaveChanges();
                                /////// DBTrackFile.DBTrackSave("Payroll/Payroll", "C", BasicScale, null, "BasicScale", null);
                                ts.Complete();
                                //  Msg.Add("  Data Saved successfully  ");return Json(new Utility.JsonReturnClass {  success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                Msg.Add("  Data Created successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = look.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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

        public class BasicScale_BasicScaleDetails
        {
            public Array BSDL_Id { get; set; }
            public Array BSDL_FullDetails { get; set; }

        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<BasicScale_BasicScaleDetails> return_data = new List<BasicScale_BasicScaleDetails>();
                var BasicScale = db.BasicScale.Include(e => e.BasicScaleDetails).Where(e => e.Id == data).ToList();
                var r = (from ca in BasicScale
                         select new
                         {
                             Id = ca.Id,
                             ScaleName = ca.ScaleName,
                             Action = ca.DBTrack.Action
                         }).Distinct();

                var basicdetails_data = db.BasicScale.Include(e => e.BasicScaleDetails).Where(e => e.Id == data).Select(e => e.BasicScaleDetails.OrderBy(x=>x.StartingSlab)).ToList();
                foreach (var item in basicdetails_data)
                {
                    return_data.Add(new BasicScale_BasicScaleDetails
                    {
                        BSDL_Id = item.Select(e => e.Id.ToString()).ToArray(),
                        BSDL_FullDetails = item.Select(e => e.FullDetails).ToArray()
                    });
                }

                var W = db.DT_BasicScale
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         ScaleName = e.ScaleName == null ? "" : e.ScaleName
                         //BasicScaleDetails_Val
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var LKup = db.BasicScale.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, return_data, W, Auth, JsonRequestBehavior.AllowGet });
                //return this.Json(new Object[] { r, a, JsonRequestBehavior.AllowGet });

                //var BasicScale = db.BasicScale.Include(e => e.BasicScaleDetails).Where(e => e.Id == data).ToList();
                //List<Lookup_lookupval> return_data = new List<Lookup_lookupval>();
                //var r = (from ca in BasicScale
                //         select new
                //         {
                //             Id = ca.Id,
                //             ScaleName = ca.ScaleName,
                //             Code = ca.Code

                //         }).Distinct();

                //var a = db.BasicScale.Include(e => e.BasicScaleDetails).Where(e => e.Id == data).Select(e => e.BasicScaleDetails).SingleOrDefault();

                //foreach (var ca in a)
                //{
                //    return_data.Add(
                //    new Lookup_lookupval
                //    {
                //        lookupval_id = ca.Select(e => e.Id.ToString()).ToArray(),
                //        lookupval_val = ca.Select(e => e.LookupVal).ToArray()
                //    });
                //}

                //var W = db.DT_BasicScale
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,
                //         Code = e.Code == null ? "" : e.Code,
                //         ScaleName = e.ScaleName == null ? "" : e.ScaleName
                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                //var LKup = db.BasicScale.Find(data);
                //TempData["RowVersion"] = LKup.RowVersion;
                //var Auth = LKup.DBTrack.IsModified;

                //return Json(new Object[] { r, a, W, Auth, JsonRequestBehavior.AllowGet });
                // return this.Json(new Object[] { r, a, JsonRequestBehavior.AllowGet });


                ////List<Lookup_lookupval> return_data = new List<Lookup_lookupval>();

                ////var Q = db.BasicScale
                ////   .Where(e => e.Id == data).Select
                ////   (e => new
                ////   {
                ////       Code = e.Code,
                ////       ScaleName = e.ScaleName,
                ////       Action = e.DBTrack.Action
                ////   }).ToList();

                ////var a = db.BasicScale.Include(e => e.BasicScaleDetails).Where(e => e.Id == data).Select(e => e.BasicScaleDetails).ToList();
                ////foreach (var ca in a)
                ////{
                ////    return_data.Add(
                ////        new Lookup_lookupval
                ////        {
                ////            lookupval_id = ca.Select(e => e.Id.ToString()).ToArray(),
                ////            lookupval_val = ca.Select(e => e.LookupVal).ToArray()
                ////        });
                ////}


                ////var W = db.DT_BasicScale
                ////     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                ////     (e => new
                ////     {
                ////         DT_Id = e.Id,
                ////         Code = e.Code == null ? "" : e.Code,
                ////         ScaleName = e.ScaleName == null ? "" : e.ScaleName,
                ////         //BusinessType_Val = e.BusinessType_Id == 0 ? "" : db.BasicScaleDetails
                ////         //           .Where(x => x.Id == e.BusinessType_Id)
                ////         //           .Select(x => x.LookupVal).FirstOrDefault(),

                ////         //Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
                ////         //Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
                ////     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                ////var LK = db.BasicScale.Find(data);
                ////TempData["RowVersion"] = LK.RowVersion;
                ////var Auth = LK.DBTrack.IsModified;
                ////return Json(new Object[] { Q, return_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        public ActionResult EditLookupVal_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var val = db.BasicScaleDetails.Where(e => e.Id == data).SingleOrDefault();

                var r = (from ca in db.BasicScaleDetails
                         select new
                         {
                             Id = ca.Id,
                             StartingSlab = ca.StartingSlab,
                             EndingSlab = ca.EndingSlab,
                             IncrementAmount = ca.IncrementAmount,
                             IncrementCount = ca.IncrementCount,
                         }).Where(e => e.Id == data).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> EditSave(BasicScale ESOBJ, FormCollection form, int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.BasicScale.Include(e => e.BasicScaleDetails).Where(e => e.Id == data).SingleOrDefault();
                    List<BasicScaleDetails> lookupval = new List<BasicScaleDetails>();
                    string Values = form["BSCALEDETAILS_List"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.BasicScaleDetails.Find(ca);
                            lookupval.Add(Lookup_val);
                            db_data.BasicScaleDetails = lookupval;
                        }
                    }
                    else
                    {
                        db_data.BasicScaleDetails = null;
                    }





                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    db.BasicScale.Attach(db_data);
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_data.RowVersion;
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_OBJ = db.BasicScale.Find(data);
                                    TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                    db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        BasicScale blog = blog = null;
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.BasicScale.Where(e => e.Id == data).SingleOrDefault();
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
                                        BasicScale OBJ = new BasicScale
                                        {
                                            Id = data,
                                            BasicScaleDetails = db_data.BasicScaleDetails,
                                            ScaleName = ESOBJ.ScaleName,
                                            DBTrack = ESOBJ.DBTrack
                                        };
                                        db.BasicScale.Attach(OBJ);
                                        db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                        DT_BasicScale DT_OBJ = (DT_BasicScale)obj;
                                        db.Create(DT_OBJ);
                                        db.SaveChanges();
                                        await db.SaveChangesAsync();
                                        db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
                                        ts.Complete();
                                        // return Json(new Object[] { OBJ.Id, OBJ.ScaleName, "Record Updated", JsonRequestBehavior.AllowGet });
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = OBJ.Id, Val = OBJ.ScaleName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }
                            }

                            catch (DbUpdateException e) { throw e; }
                            catch (DataException e) { throw e; }
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
                            // Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            BasicScale blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            //BasicScale Old_LKup = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.BasicScale.Where(e => e.Id == data).SingleOrDefault();
                                TempData["RowVersion"] = blog.RowVersion;
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
                            BasicScale BasicScale = new BasicScale()
                            {
                                Id = data,
                                BasicScaleDetails = db_data.BasicScaleDetails,
                                ScaleName = blog.ScaleName,
                                DBTrack = ESOBJ.DBTrack
                            };
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                            db.BasicScale.Attach(BasicScale);
                            db.Entry(BasicScale).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(BasicScale).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            TempData["RowVersion"] = db_data.RowVersion;
                            db.Entry(BasicScale).State = System.Data.Entity.EntityState.Detached;


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, ESOBJ, "BasicScale", ESOBJ.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                //Old_LKup = context.BasicScale.Where(e => e.Id == data).Include(e => e.BasicScaleDetails).SingleOrDefault();
                                DT_BasicScale DT_LKup = (DT_BasicScale)obj;

                                db.Create(DT_LKup);
                                //db.SaveChanges();
                            }

                            ts.Complete();
                            //  return Json(new Object[] { blog.Id, ESOBJ.ScaleName, "Record Updated", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = ESOBJ.ScaleName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    return View();
                    //if (Auth == false)
                    //{
                    //    if (ModelState.IsValid)
                    //    {
                    //        try
                    //        {
                    //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    //            {
                    //                //db.BasicScale.Attach(lk);
                    //                //db.Entry(lk).State = System.Data.Entity.EntityState.Modified;
                    //                //db.SaveChanges();
                    //                //db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
                    //                //ts.Complete();

                    //                db.BasicScale.Attach(db_data);
                    //                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    //                db.SaveChanges();
                    //                TempData["RowVersion"] = db_data.RowVersion;
                    //                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                    //                var Curr_OBJ = db.BasicScale.Find(data);
                    //                TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                    //                db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

                    //                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                    //                {
                    //                    BasicScale blog = blog = null;
                    //                    DbPropertyValues originalBlogValues = null;

                    //                    using (var context = new DataBaseContext())
                    //                    {
                    //                        blog = context.BasicScale.Where(e => e.Id == data).SingleOrDefault();
                    //                        originalBlogValues = context.Entry(blog).OriginalValues;
                    //                    }

                    //                    ESOBJ.DBTrack = new DBTrack
                    //                    {
                    //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                    //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                    //                        Action = "M",
                    //                        ModifiedBy = SessionManager.UserName,
                    //                        ModifiedOn = DateTime.Now
                    //                    };
                    //                    BasicScale OBJ = new BasicScale
                    //                    {
                    //                        Id = data,
                    //                        BasicScaleDetails = db_data.BasicScaleDetails,
                    //                        ScaleName = ESOBJ.ScaleName,                                   
                    //                        DBTrack = ESOBJ.DBTrack
                    //                    };


                    //                    db.BasicScale.Attach(OBJ);
                    //                    db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;

                    //                    // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                    //                    //db.SaveChanges();
                    //                    db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];

                    //                    var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                    //                    DT_BasicScale DT_OBJ = (DT_BasicScale)obj;
                    //                    //  DT_LK.BasicScaleDetails = lk.BasicScaleDetails.Select(e => e.Id);
                    //                    db.Create(DT_OBJ);
                    //                    db.SaveChanges();
                    //                    ////DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                    //                    await db.SaveChangesAsync();
                    //                    //DisplayTrackedEntities(db.ChangeTracker);
                    //                    db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
                    //                    ts.Complete();
                    //                    return Json(new Object[] { OBJ.Id, OBJ.ScaleName, "Record Updated", JsonRequestBehavior.AllowGet });
                    //                }
                    //            }
                    //        }

                    //        catch (DbUpdateException e) { throw e; }
                    //        catch (DataException e) { throw e; }
                    //    }
                    //    else
                    //    {
                    //        StringBuilder sb = new StringBuilder("");
                    //        foreach (ModelState modelState in ModelState.Values)
                    //        {
                    //            foreach (ModelError error in modelState.Errors)
                    //            {
                    //                sb.Append(error.ErrorMessage);
                    //                sb.Append("." + "\n");
                    //            }
                    //        }
                    //        var errorMsg = sb.ToString();
                    //        Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //    }
                    //}
                    //else
                    //{
                    //    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    //    {
                    //        BasicScale blog = null; // to retrieve old data
                    //        DbPropertyValues originalBlogValues = null;
                    //        //BasicScale Old_LKup = null;

                    //        using (var context = new DataBaseContext())
                    //        {
                    //            blog = context.BasicScale.Where(e => e.Id == data).SingleOrDefault();
                    //            TempData["RowVersion"] = blog.RowVersion;
                    //            originalBlogValues = context.Entry(blog).OriginalValues;
                    //        }
                    //        ESOBJ.DBTrack = new DBTrack
                    //        {
                    //            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                    //            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                    //            Action = "M",
                    //            IsModified = blog.DBTrack.IsModified == true ? true : false,
                    //            ModifiedBy = SessionManager.UserName,
                    //            ModifiedOn = DateTime.Now
                    //        };
                    //        BasicScale BasicScale = new BasicScale()
                    //        {
                    //            ScaleName = blog.ScaleName,
                    //            Id = data,
                    //            DBTrack = ESOBJ.DBTrack,
                    //            BasicScaleDetails = db_data.BasicScaleDetails
                    //        };
                    //        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                    //        db.BasicScale.Attach(BasicScale);
                    //        db.Entry(BasicScale).State = System.Data.Entity.EntityState.Modified;
                    //        db.Entry(BasicScale).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //        db.SaveChanges();
                    //        TempData["RowVersion"] = db_data.RowVersion;
                    //        db.Entry(BasicScale).State = System.Data.Entity.EntityState.Detached;


                    //        //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                    //        using (var context = new DataBaseContext())
                    //        {
                    //            var obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, ESOBJ, "BasicScale", ESOBJ.DBTrack);
                    //            // var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);

                    //            //Old_LKup = context.BasicScale.Where(e => e.Id == data).Include(e => e.BasicScaleDetails).SingleOrDefault();
                    //            DT_BasicScale DT_LKup = (DT_BasicScale)obj;

                    //            db.Create(DT_LKup);
                    //            //db.SaveChanges();
                    //        }
                    //        ////db.Entry(blog).State = System.Data.Entity.EntityState.Detached;
                    //        ////BasicScale.DBTrack = ESOBJ.DBTrack;
                    //        ////db.BasicScale.Attach(BasicScale);
                    //        ////db.Entry(BasicScale).State = System.Data.Entity.EntityState.Modified;
                    //        ////db.Entry(BasicScale).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //        ////db.SaveChanges();
                    //        ts.Complete();
                    //        return Json(new Object[] { blog.Id, ESOBJ.ScaleName, "Record Updated", JsonRequestBehavior.AllowGet });
                    //    }
                    //}
                    //return View();
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
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    BasicScale BasicScale = db.BasicScale.Include(e => e.BasicScaleDetails).Where(e => e.Id == data).SingleOrDefault();

                    if (BasicScale.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = BasicScale.DBTrack.CreatedBy != null ? BasicScale.DBTrack.CreatedBy : null,
                                CreatedOn = BasicScale.DBTrack.CreatedOn != null ? BasicScale.DBTrack.CreatedOn : null,
                                IsModified = BasicScale.DBTrack.IsModified == true ? true : false
                            };
                            BasicScale.DBTrack = dbT;
                            db.Entry(BasicScale).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, BasicScale.DBTrack);
                            DT_BasicScale DT_OBJ = (DT_BasicScale)rtn_Obj;
                            db.Create(DT_OBJ);

                            await db.SaveChangesAsync();
                            ts.Complete();
                            // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            var selectedValues = BasicScale.BasicScaleDetails;
                            var lkValue = new HashSet<int>(BasicScale.BasicScaleDetails.Select(e => e.Id));
                            if (lkValue.Count > 0)
                            {
                                // return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });

                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            db.Entry(BasicScale).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            ts.Complete();
                            //  return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

        public ActionResult GetLookupValue(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = (SelectList)null;
                var selected = "";
                if (data != "" && data != null)
                {
                    var qurey = db.BasicScale.Include(e => e.BasicScaleDetails)
                        .Where(e => e.Id.ToString() == data).SingleOrDefault();
                    if (data2 != "" && data2 != "0")
                    {
                        selected = data2;
                    }
                    if (qurey != null)
                    {
                        s = new SelectList(qurey.BasicScaleDetails, "Id", "LookupVal", selected);
                    }
                }
                return Json(s, JsonRequestBehavior.AllowGet);
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
                int ParentId = 2;
                var jsonData = (Object)null;
                var LKVal = db.BasicScale.ToList();

                if (gp.IsAutho == true)
                {
                    LKVal = db.BasicScale.Include(e => e.BasicScaleDetails).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    LKVal = db.BasicScale.Include(e => e.BasicScaleDetails).AsNoTracking().ToList();
                }


                IEnumerable<BasicScale> IE;
                if (!string.IsNullOrEmpty(gp.searchString))
                {
                    IE = LKVal;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                            || (e.ScaleName.ToUpper().Contains(gp.searchString.ToUpper()))
                            ).Select(a => new { a.Id, a.ScaleName }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.ScaleName }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = LKVal;
                    Func<BasicScale, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>
                                         gp.sidx == "ScaleName" ? c.ScaleName : "");
                    }

                    //Func<BasicScale, string> orderfuc = (c =>
                    //                                           gp.sidx == "Id" ? c.Id.ToString() :
                    //                                           gp.sidx == "Code" ? c.Code :
                    //                                           gp.sidx == "ScaleName" ? c.ScaleName : "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.ScaleName, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.ScaleName, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  a.ScaleName, a.Id }).ToList();
                    }
                    totalRecords = LKVal.Count();
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
                    total = totalPages,
                    p2bparam = ParentId
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult GetBasicScaleDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.BasicScaleDetails.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.BasicScaleDetails.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                //var list1 = db.BasicScale.Include(e => e.BasicScaleDetails).SelectMany(e => e.BasicScaleDetails).ToList();
                //var list2 = fall.Except(list1);

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult GetBasicRuleLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.BasicScale.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.BasicScale.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                //var list1 = db.BasicScale.Include(e => e.BasicScaleDetails).SelectMany(e => e.BasicScaleDetails).ToList();
                //var list2 = fall.Except(list1);

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.ScaleName }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            BasicScale OBJ = db.BasicScale.Include(e => e.BasicScaleDetails).FirstOrDefault(e => e.Id == auth_id);

                            OBJ.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = OBJ.DBTrack.ModifiedBy != null ? OBJ.DBTrack.ModifiedBy : null,
                                CreatedBy = OBJ.DBTrack.CreatedBy != null ? OBJ.DBTrack.CreatedBy : null,
                                CreatedOn = OBJ.DBTrack.CreatedOn != null ? OBJ.DBTrack.CreatedOn : null,
                                IsModified = OBJ.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.BasicScale.Attach(OBJ);
                            db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, OBJ.DBTrack);
                            DT_BasicScale DT_OBJ = (DT_BasicScale)rtn_Obj;

                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            //return Json(new Object[] { OBJ.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = OBJ.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else if (auth_action == "M")
                    {

                        BasicScale Old_OBJ = db.BasicScale.Include(e => e.BasicScaleDetails).Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_BasicScale Curr_OBJ = db.DT_BasicScale
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_OBJ != null)
                        {
                            BasicScale OBJ = new BasicScale();


                            OBJ.ScaleName = Curr_OBJ.ScaleName == null ? Old_OBJ.ScaleName : Curr_OBJ.ScaleName;

                            OBJ.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        OBJ.DBTrack = new DBTrack
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

                                        //int a = EditS(Corp, Addrs, ContactDetails, auth_id, corp, corp.DBTrack);

                                        db.Entry(Old_OBJ).State = System.Data.Entity.EntityState.Detached;
                                        db.BasicScale.Attach(OBJ);
                                        db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        db.SaveChanges();
                                        db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        //   return Json(new Object[] { OBJ.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = OBJ.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (Corporate)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                    else
                                    {
                                        var databaseValues = (Corporate)databaseEntry.ToObject();
                                        OBJ.RowVersion = databaseValues.RowVersion;
                                    }
                                }

                                //    return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }
                        else
                            // return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed from history ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Corporate corp = db.Corporate.Find(auth_id);
                            BasicScale OBJ = db.BasicScale.AsNoTracking().Include(e => e.BasicScaleDetails).FirstOrDefault(e => e.Id == auth_id);

                            //Address add = corp.Address;
                            //ContactDetails conDet = corp.ContactDetails;
                            //LookupValue val = corp.BusinessType;

                            OBJ.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = OBJ.DBTrack.ModifiedBy != null ? OBJ.DBTrack.ModifiedBy : null,
                                CreatedBy = OBJ.DBTrack.CreatedBy != null ? OBJ.DBTrack.CreatedBy : null,
                                CreatedOn = OBJ.DBTrack.CreatedOn != null ? OBJ.DBTrack.CreatedOn : null,
                                IsModified = false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.BasicScale.Attach(OBJ);
                            db.Entry(OBJ).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, OBJ.DBTrack);
                            DT_BasicScale DT_OBJ = (DT_BasicScale)rtn_Obj;
                            //DT_OBJ.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            //DT_OBJ.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                            //DT_OBJ.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();
                            db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            //  return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
