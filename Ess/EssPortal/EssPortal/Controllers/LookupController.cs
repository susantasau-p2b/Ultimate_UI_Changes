using P2b.Global;
using EssPortal.App_Start;
using EssPortal.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using EssPortal.Security;
namespace EssPortal.Controllers
{
    [AuthoriseManger]
    public class LookupController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/Lookup/Index.cshtml");
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/Core/_Lookupvalues.cshtml");
        }
        private MultiSelectList GetLookupValues(List<int> selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<LookupValue> lkval = new List<LookupValue>();
                lkval = db.LookupValue.ToList();
                return new MultiSelectList(lkval, "Id", "LookupVal", selectedValues);
            }
        }
        public String checkCode(String data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Lookup.Any(e => e.Code == data);
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
        public ActionResult Create(Lookup look, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<LookupValue> lookupval = new List<LookupValue>();
                string Values = form["LookupValueslist"];

                if (Values != null)
                {
                    var ids = Utility.StringIdsToListIds(Values);
                    foreach (var ca in ids)
                    {
                        var lookup_val = db.LookupValue.Find(ca);
                        lookupval.Add(lookup_val);
                        look.LookupValues = lookupval;
                    }
                }
                else
                {
                    look.LookupValues = null;
                }

                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //look.LookupValues = new List<LookupValue>();
                        //if (ViewBag.LookupVal != null)
                        //{
                        //    foreach (var val in ViewBag.LookupVal)
                        //    {
                        //        if (val.Selected == true)
                        //        {
                        //            var valToAdd = db.LookupValue.Find(int.Parse(val.Value));
                        //            look.LookupValues.Add(valToAdd);
                        //        }
                        //    }
                        //}

                        if (db.Lookup.Any(o => o.Code == look.Code))
                        {
                            //return this.Json(new { msg = "Code already exists." });
                            return this.Json(new Object[] { null, null, "Code already exists.", JsonRequestBehavior.AllowGet });

                        }

                        look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                        Lookup lookup = new Lookup()
                        {
                            Code = look.Code == null ? "" : look.Code.Trim(),
                            Name = look.Name == null ? "" : look.Name.Trim(),
                            LookupValues = look.LookupValues,
                            DBTrack = look.DBTrack
                        };
                        try
                        {
                            db.Lookup.Add(lookup);
                            var a = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, look.DBTrack);
                            DT_Lookup DT_Lookup = (DT_Lookup)a;
                            //ICollection<Int32> test = new Collection<Int32>();
                            //if (lookup.LookupValues.Count > 0)
                            //{
                            //    foreach (var i in lookup.LookupValues)
                            //    {
                            //        test.Add(i.Id);
                            //    }

                            //}
                            // DT_Corp.LookupValues = test;
                            db.Create(DT_Lookup);
                            db.SaveChanges();


                            //for (int i = 0; i < IDs.Count; i++)
                            //{
                            //    var LkList = db.DT_LookupValue.Where(e => e.Orig_Id == 15).SingleOrDefault();
                            //    LkList.Lookup_Id = lookup.Id;
                            //    db.Entry(LkList).State = System.Data.Entity.EntityState.Modified;
                            //    db.SaveChanges();
                            //}


                            //// db.Lookup.Add(lookup);

                            //////// DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, look.DBTrack);
                            //// db.SaveChanges();
                            /////// DBTrackFile.DBTrackSave("Core/P2b.Global", "C", lookup, null, "Lookup", null);
                            ts.Complete();
                            return this.Json(new Object[] { null, null, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                        }

                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = look.Id });
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
                    // return this.Json(new { msg = errorMsg });
                }

            }
        }
        public class lookup_lookupval
        {
            public Array lookupval_id { get; set; }
            public Array lookupval_val { get; set; }

        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                List<lookup_lookupval> return_data = new List<lookup_lookupval>();
                var lookup = db.Lookup.Include(e => e.LookupValues).Where(e => e.Id == data).ToList();
                var r = (from ca in lookup
                         select new
                         {
                             Id = ca.Id,
                             Name = ca.Name,
                             Code = ca.Code,
                             Action = ca.DBTrack.Action
                         }).Distinct();

                var a = db.Lookup.Include(e => e.LookupValues).Where(e => e.Id == data).ToList();

                foreach (var ca in a)
                {
                    return_data.Add(
                new lookup_lookupval
                {
                    lookupval_id = ca.LookupValues.Select(e => e.Id.ToString()).ToArray(),
                    lookupval_val = ca.LookupValues.Select(e => e.LookupVal).ToArray()
                });
                }

                var W = db.DT_Lookup
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Code = e.Code == null ? "" : e.Code,
                         Name = e.Name == null ? "" : e.Name
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var LKup = db.Lookup.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, return_data, W, Auth, JsonRequestBehavior.AllowGet });
                //return this.Json(new Object[] { r, a, JsonRequestBehavior.AllowGet });

                //var lookup = db.Lookup.Include(e => e.LookupValues).Where(e => e.Id == data).ToList();
                //List<lookup_lookupval> return_data = new List<lookup_lookupval>();
                //var r = (from ca in lookup
                //         select new
                //         {
                //             Id = ca.Id,
                //             Name = ca.Name,
                //             Code = ca.Code

                //         }).Distinct();

                //var a = db.Lookup.Include(e => e.LookupValues).Where(e => e.Id == data).Select(e => e.LookupValues).SingleOrDefault();

                //foreach (var ca in a)
                //{
                //    return_data.Add(
                //    new lookup_lookupval
                //    {
                //        lookupval_id = ca.Select(e => e.Id.ToString()).ToArray(),
                //        lookupval_val = ca.Select(e => e.LookupVal).ToArray()
                //    });
                //}

                //var W = db.DT_Lookup
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,
                //         Code = e.Code == null ? "" : e.Code,
                //         Name = e.Name == null ? "" : e.Name
                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                //var LKup = db.Lookup.Find(data);
                //TempData["RowVersion"] = LKup.RowVersion;
                //var Auth = LKup.DBTrack.IsModified;

                //return Json(new Object[] { r, a, W, Auth, JsonRequestBehavior.AllowGet });
                // return this.Json(new Object[] { r, a, JsonRequestBehavior.AllowGet });


                ////List<lookup_lookupval> return_data = new List<lookup_lookupval>();

                ////var Q = db.Lookup
                ////   .Where(e => e.Id == data).Select
                ////   (e => new
                ////   {
                ////       Code = e.Code,
                ////       Name = e.Name,
                ////       Action = e.DBTrack.Action
                ////   }).ToList();

                ////var a = db.Lookup.Include(e => e.LookupValues).Where(e => e.Id == data).Select(e => e.LookupValues).ToList();
                ////foreach (var ca in a)
                ////{
                ////    return_data.Add(
                ////        new lookup_lookupval
                ////        {
                ////            lookupval_id = ca.Select(e => e.Id.ToString()).ToArray(),
                ////            lookupval_val = ca.Select(e => e.LookupVal).ToArray()
                ////        });
                ////}


                ////var W = db.DT_Lookup
                ////     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                ////     (e => new
                ////     {
                ////         DT_Id = e.Id,
                ////         Code = e.Code == null ? "" : e.Code,
                ////         Name = e.Name == null ? "" : e.Name,
                ////         //BusinessType_Val = e.BusinessType_Id == 0 ? "" : db.LookupValue
                ////         //           .Where(x => x.Id == e.BusinessType_Id)
                ////         //           .Select(x => x.LookupVal).FirstOrDefault(),

                ////         //Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
                ////         //Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
                ////     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                ////var LK = db.Lookup.Find(data);
                ////TempData["RowVersion"] = LK.RowVersion;
                ////var Auth = LK.DBTrack.IsModified;
                ////return Json(new Object[] { Q, return_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }
        public ActionResult EditLookupVal_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var val = db.LookupValue.Where(e => e.Id == data).SingleOrDefault();

                var r = (from ca in db.LookupValue
                         select new
                         {
                             Id = ca.Id,
                             LookupVal = ca.LookupVal
                         }).Where(e => e.Id == data).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<ActionResult> EditSave(Lookup look, FormCollection form, int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var db_data = db.Lookup.Include(e => e.LookupValues).Where(e => e.Id == data).SingleOrDefault();
                List<LookupValue> lookupval = new List<LookupValue>();
                string Values = form["LookupValueslist"];
                bool Auth = form["autho_allow"] == "true" ? true : false;

                if (Values != null)
                {
                    var ids = Utility.StringIdsToListIds(Values);
                    foreach (var ca in ids)
                    {
                        var lookup_val = db.LookupValue.Find(ca);
                        lookupval.Add(lookup_val);
                        db_data.LookupValues = lookupval;
                    }
                }
                else
                {
                    db_data.LookupValues = null;
                }

                if (Auth == false)
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                //db.Lookup.Attach(lk);
                                //db.Entry(lk).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();
                                //db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
                                //ts.Complete();

                                db.Lookup.Attach(db_data);
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = db_data.RowVersion;
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                var Curr_Lookup = db.Lookup.Find(data);
                                TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    Lookup blog = blog = null;
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.Lookup.Where(e => e.Id == data).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    look.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    Lookup lk = new Lookup
                                    {
                                        Id = data,
                                        LookupValues = db_data.LookupValues,
                                        Name = look.Name,
                                        Code = look.Code,
                                        DBTrack = look.DBTrack
                                    };


                                    db.Lookup.Attach(lk);
                                    db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                    // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                    //db.SaveChanges();
                                    db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                    var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, look.DBTrack);
                                    DT_Lookup DT_LK = (DT_Lookup)obj;
                                    //  DT_LK.LookupValues = lk.LookupValues.Select(e => e.Id);
                                    db.Create(DT_LK);
                                    db.SaveChanges();
                                    ////DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, look.DBTrack);
                                    await db.SaveChangesAsync();
                                    //DisplayTrackedEntities(db.ChangeTracker);
                                    db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
                                    ts.Complete();
                                    return Json(new Object[] { lk.Id, lk.Name, "Record Updated", JsonRequestBehavior.AllowGet });
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
                        return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                    }
                }
                else
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        Lookup blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        //Lookup Old_LKup = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.Lookup.Where(e => e.Id == data).SingleOrDefault();
                            TempData["RowVersion"] = blog.RowVersion;
                            originalBlogValues = context.Entry(blog).OriginalValues;
                        }
                        look.DBTrack = new DBTrack
                        {
                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                            Action = "M",
                            IsModified = blog.DBTrack.IsModified == true ? true : false,
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now
                        };
                        Lookup lookup = new Lookup()
                        {
                            Code = blog.Code,
                            Name = blog.Name,
                            Id = data,
                            DBTrack = look.DBTrack,
                            LookupValues = db_data.LookupValues
                        };
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        db.Lookup.Attach(lookup);
                        db.Entry(lookup).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(lookup).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;
                        db.Entry(lookup).State = System.Data.Entity.EntityState.Detached;


                        //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                        using (var context = new DataBaseContext())
                        {
                            var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, look, "Lookup", look.DBTrack);
                            // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                            //Old_LKup = context.Lookup.Where(e => e.Id == data).Include(e => e.LookupValues).SingleOrDefault();
                            DT_Lookup DT_LKup = (DT_Lookup)obj;

                            db.Create(DT_LKup);
                            //db.SaveChanges();
                        }
                        ////db.Entry(blog).State = System.Data.Entity.EntityState.Detached;
                        ////lookup.DBTrack = look.DBTrack;
                        ////db.Lookup.Attach(lookup);
                        ////db.Entry(lookup).State = System.Data.Entity.EntityState.Modified;
                        ////db.Entry(lookup).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        ////db.SaveChanges();
                        ts.Complete();
                        return Json(new Object[] { blog.Id, look.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                    }
                }
                return View();
            }
        }
        public async Task<ActionResult> Delete(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                Lookup lookup = db.Lookup.Include(e => e.LookupValues).Where(e => e.Id == data).SingleOrDefault();

                if (lookup.DBTrack.IsModified == true)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            CreatedBy = lookup.DBTrack.CreatedBy != null ? lookup.DBTrack.CreatedBy : null,
                            CreatedOn = lookup.DBTrack.CreatedOn != null ? lookup.DBTrack.CreatedOn : null,
                            IsModified = lookup.DBTrack.IsModified == true ? true : false
                        };
                        lookup.DBTrack = dbT;
                        db.Entry(lookup).State = System.Data.Entity.EntityState.Modified;
                        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, lookup.DBTrack);
                        DT_Lookup DT_LKup = (DT_Lookup)rtn_Obj;
                        db.Create(DT_LKup);

                        await db.SaveChangesAsync();
                        ts.Complete();
                        return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    }
                }
                else
                {

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var selectedValues = lookup.LookupValues;
                        var lkValue = new HashSet<int>(lookup.LookupValues.Select(e => e.Id));
                        if (lkValue.Count > 0)
                        {
                            return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                        }
                        db.Entry(lookup).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                        return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    }
                }
                //Lookup lookup = db.Lookup.Include(e => e.LookupValues).Where(e => e.Id == data).SingleOrDefault();
                //try
                //{
                //    var selectedValues = lookup.LookupValues;
                //    var lkValue = new HashSet<int>(lookup.LookupValues.Select(e => e.Id));
                //    if (lkValue.Count > 0)
                //    {
                //        return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                //    }

                //    using (TransactionScope ts = new TransactionScope())
                //    {
                //        db.Entry(lookup).State = System.Data.Entity.EntityState.Deleted;
                //        db.SaveChanges();
                //        ts.Complete();
                //    }
                //    return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet }); 
                //}

                //catch (DataException /* dex */)
                //{
                //    return this.Json(new Object[] { "", "Unable to delete. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });

                //}
            }
        }
        public ActionResult Getalldetails(string v, string x)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Corporate.ToList();
                IEnumerable<Corporate> all;
                if (!string.IsNullOrEmpty(x))
                {
                    all = db.Corporate.ToList().Where(d => d.Code.Contains(x) || d.Name.ToLower().Contains(x.ToLower()));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.Code + " - " + ca.Name }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.Code, c.Name }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
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
                    // var qurey = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == data).SingleOrDefault();
                    var qurey = db.Lookup.Where(e => e.Code == data).Select(e => e.LookupValues.Where(r => r.IsActive == true)).SingleOrDefault(); // added by rekha 26-12-16
                    if (data2 != "" && data2 != "0")
                    {
                        selected = data2;
                    }
                    if (qurey != null)
                    {
                        s = new SelectList(qurey, "Id", "LookupVal", selected);
                    }
                }
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetLookupValueForManpowerRequest(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = (SelectList)null;
                var selected = "";
                if (data != "" && data != null)
                {
                    // var qurey = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == data).SingleOrDefault();
                    var qurey = db.Lookup.Where(e => e.Code == data).Select(e => e.LookupValues.Where(r => r.IsActive == true && r.LookupVal.ToString().ToUpper() == data2.ToUpper())).SingleOrDefault(); // added by rekha 26-12-16
                    if (data2 != "" && data2 != "0")
                    {
                        selected = qurey.Select(e=>e.Id).ToString();
                    }
                    if (qurey != null)
                    {
                        s = new SelectList(qurey, "Id", "LookupVal", selected);
                    }
                }
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetLookupDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LookupValue.Where(e => e.IsActive == true).ToList();


                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LookupValue.Where(e => !e.Id.ToString().Contains(a.ToString()) && e.IsActive == true).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString()) && e.IsActive == true).ToList();

                    }
                }
                var list1 = db.Lookup.SelectMany(e => e.LookupValues).ToList();
                var list2 = fall.Except(list1);
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (auth_action == "C")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        Lookup Lkup = db.Lookup.Include(e => e.LookupValues).FirstOrDefault(e => e.Id == auth_id);

                        Lkup.DBTrack = new DBTrack
                        {
                            Action = "C",
                            ModifiedBy = Lkup.DBTrack.ModifiedBy != null ? Lkup.DBTrack.ModifiedBy : null,
                            CreatedBy = Lkup.DBTrack.CreatedBy != null ? Lkup.DBTrack.CreatedBy : null,
                            CreatedOn = Lkup.DBTrack.CreatedOn != null ? Lkup.DBTrack.CreatedOn : null,
                            IsModified = Lkup.DBTrack.IsModified == true ? false : false,
                            AuthorizedBy = SessionManager.UserName,
                            AuthorizedOn = DateTime.Now
                        };

                        db.Lookup.Attach(Lkup);
                        db.Entry(Lkup).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(Lkup).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //db.SaveChanges();
                        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Lkup.DBTrack);
                        DT_Lookup DT_Lkup = (DT_Lookup)rtn_Obj;

                        db.Create(DT_Lkup);
                        await db.SaveChangesAsync();

                        ts.Complete();
                        return Json(new Object[] { Lkup.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                    }
                }
                else if (auth_action == "M")
                {

                    Lookup Old_LKup = db.Lookup.Include(e => e.LookupValues).Where(e => e.Id == auth_id).SingleOrDefault();


                    DT_Lookup Curr_LKup = db.DT_Lookup
                                                .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                .OrderByDescending(e => e.Id)
                                                .FirstOrDefault();

                    if (Curr_LKup != null)
                    {
                        Lookup LKup = new Lookup();


                        LKup.Name = Curr_LKup.Name == null ? Old_LKup.Name : Curr_LKup.Name;
                        LKup.Code = Curr_LKup.Code == null ? Old_LKup.Code : Curr_LKup.Code;
                        LKup.Id = auth_id;

                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    // db.Configuration.AutoDetectChangesEnabled = false;
                                    LKup.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_LKup.DBTrack.CreatedBy == null ? null : Old_LKup.DBTrack.CreatedBy,
                                            CreatedOn = Old_LKup.DBTrack.CreatedOn == null ? null : Old_LKup.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_LKup.DBTrack.ModifiedBy == null ? null : Old_LKup.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_LKup.DBTrack.ModifiedOn == null ? null : Old_LKup.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                    //int a = EditS(Corp, Addrs, ContactDetails, auth_id, corp, corp.DBTrack);

                                    db.Entry(Old_LKup).State = System.Data.Entity.EntityState.Detached;
                                    db.Lookup.Attach(LKup);
                                    db.Entry(LKup).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(LKup).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    db.SaveChanges();
                                    db.Entry(LKup).State = System.Data.Entity.EntityState.Detached;

                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    return Json(new Object[] { LKup.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Corporate)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (Corporate)databaseEntry.ToObject();
                                    LKup.RowVersion = databaseValues.RowVersion;
                                }
                            }

                            return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                        return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                }
                else if (auth_action == "D")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //Corporate corp = db.Corporate.Find(auth_id);
                        Lookup LKup = db.Lookup.AsNoTracking().Include(e => e.LookupValues).FirstOrDefault(e => e.Id == auth_id);

                        //Address add = corp.Address;
                        //ContactDetails conDet = corp.ContactDetails;
                        //LookupValue val = corp.BusinessType;

                        LKup.DBTrack = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = LKup.DBTrack.ModifiedBy != null ? LKup.DBTrack.ModifiedBy : null,
                            CreatedBy = LKup.DBTrack.CreatedBy != null ? LKup.DBTrack.CreatedBy : null,
                            CreatedOn = LKup.DBTrack.CreatedOn != null ? LKup.DBTrack.CreatedOn : null,
                            IsModified = false,
                            AuthorizedBy = SessionManager.UserName,
                            AuthorizedOn = DateTime.Now
                        };

                        db.Lookup.Attach(LKup);
                        db.Entry(LKup).State = System.Data.Entity.EntityState.Deleted;


                        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, LKup.DBTrack);
                        DT_Lookup DT_LKup = (DT_Lookup)rtn_Obj;
                        //DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                        //DT_Corp.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                        //DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                        db.Create(DT_LKup);
                        await db.SaveChangesAsync();
                        db.Entry(LKup).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
                    }
                }
                return View();
            }
        }
    }
}
