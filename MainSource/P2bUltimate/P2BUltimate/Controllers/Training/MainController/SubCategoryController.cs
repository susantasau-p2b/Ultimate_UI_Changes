///
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

namespace P2BUltimate.Controllers.Training.MainController
{
    public class SubCategoryController : Controller
    {
        List<string> Msg = new List<string>(); //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/SubCategory/Index.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/Training/_SubCategory.cshtml");
        }

        public class BudgetD
        {
            public Array Bud_Credit { get; set; }
            public Array Bud_Debit { get; set; }
        }
        public ActionResult Create(SubCategory COBJ, FormCollection form)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        if (db.SubCategory.Any(o => o.Code == COBJ.Code))
                        {
                            Msg.Add("  Code Already Exists.  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        string TrainingType = form["ProgramListlist"] == "0" ? "" : form["ProgramListlist"];

                        if (TrainingType == null)
                        {
                            TrainingType = form["ProgramListlistS"] == "0" ? "" : form["ProgramListlistS"];
                        }

                        //string Budgetlist = form["BudgetlistS"] == "0" ? "" : form["BudgetlistS"];

                        if (TrainingType != null)
                        {
                            if (TrainingType != "")
                            {
                                var ids = Utility.StringIdsToListIds(TrainingType);
                                List<ProgramList> SubCategoryList = new List<ProgramList>();
                                foreach (var item in ids)
                                {
                                    var val = db.ProgramList.Find(item);
                                    SubCategoryList.Add(val);
                                }
                                COBJ.ProgramList = SubCategoryList;
                            }
                        }
                        //if (Budgetlist != null)
                        //{
                        //    var ids = Utility.StringIdsToListIds(Budgetlist);
                        //    List<Budget> BudList = new List<Budget>();
                        //    foreach (var item in ids)
                        //    {
                        //        var val = db.Budget.Find(item);
                        //        BudList.Add(val);
                        //    }
                        //    COBJ.Budget = BudList;
                        //}

                        List<BudgetD> pst = new List<BudgetD>();
                        if (COBJ.ProgramList != null)
                        {

                            foreach (var data in COBJ.ProgramList)
                            {
                                var aas = db.ProgramList
                                    //.Include(a => a.Budget)
                                    .Where(e => e.Id == data.Id).ToList();
                                //foreach (var ca in aas)
                                //{
                                //pst.Add(new BudgetD
                                //{
                                //    Bud_Credit = ca.Budget.Select(e => e.BudgetCredit).ToArray(),
                                //    Bud_Debit = ca.Budget.Select(e => e.BudgetDebit).ToArray(),
                                //});

                                //var bud = COBJ.Budget.ToList();

                                //foreach (var b in bud)
                                //{
                                //    int Bc = Int32.Parse(b.BudgetCredit.ToString());
                                //    int Bd = Int32.Parse(b.BudgetDebit.ToString());

                                //    foreach (var k in pst)
                                //    {
                                //        foreach (var Ba in k.Bud_Credit)
                                //        {
                                //            int Bct = Int32.Parse(Ba.ToString());
                                //            if (Bct > Bc)
                                //            {
                                //                Msg.Add("Programlist budget should be same or under subcategory budget");
                                //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //            }
                                //        }
                                //        foreach (var Bb in k.Bud_Debit)
                                //        {
                                //            int Bdt = Int32.Parse(Bb.ToString());
                                //            if (Bdt > Bd)
                                //            {
                                //                Msg.Add("Programlist budget should be same or under subcategory budget");
                                //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //            }
                                //        }
                                //    }
                                //}
                                //}


                            }
                        }





                        COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId, IsModified = false };

                        //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId };

                        SubCategory SubCategory = new SubCategory()
                        {

                            Code = COBJ.Code,
                            Details = COBJ.Details,
                            ProgramList = COBJ.ProgramList,
                            //Budget = COBJ.Budget,
                            DBTrack = COBJ.DBTrack
                        };
                        try
                        {
                            db.SubCategory.Add(SubCategory);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, COBJ.DBTrack);
                            DT_SubCategory DT_OBJ = (DT_SubCategory)rtn_Obj;
                            db.Create(DT_OBJ);
                            db.SaveChanges();

                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, Id = SubCategory.Id, Val = SubCategory.FullDetails, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = COBJ.Id });
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
                    Msg.Add(errorMsg);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        public class ProgramList1
        {
            public Array ProgramList_Id { get; set; }
            public Array ProgramList_val { get; set; }
            public Array Budget_Id { get; set; }
            public Array Budget_val { get; set; }
        }

        public ActionResult GetLookupDetailsProgramList(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var fall = db.SubCategory.Include(a => a.ProgramList).SelectMany(a => a.ProgramList).ToList();
                var fall = db.ProgramList.ToList();
                IEnumerable<ProgramList> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.ProgramList.ToList();

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

        public ActionResult Edit(int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var SubCategory = db.SubCategory
                                    .Where(e => e.Id == data).ToList();
                var r = (from ca in SubCategory
                         select new
                         {

                             Id = ca.Id,
                             Code = ca.Code,
                             Details = ca.Details,
                             Action = ca.DBTrack.Action
                         }).Distinct();


                List<ProgramList1> fext = new List<ProgramList1>();


                var add_data = db.SubCategory
                               .Include(e => e.ProgramList)
                    //.Include(e => e.Budget)


                             .Where(e => e.Id == data).ToList();



                foreach (var ca in add_data)
                {
                    fext.Add(new ProgramList1
                    {
                        ProgramList_Id = ca.ProgramList.Select(e => e.Id.ToString()).ToArray(),
                        ProgramList_val = ca.ProgramList.Select(e => e.FullDetails).ToArray(),
                        //Budget_Id = ca.Budget.Select(e => e.Id.ToString()).ToArray(),
                        //Budget_val = ca.Budget.Select(e => e.FullDetails.ToString()).ToArray()
                    });
                }


                var W = db.DT_SubCategory
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Code = e.Code,
                         Details = e.Details,

                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var LKup = db.SubCategory.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, fext, "", W, Auth, JsonRequestBehavior.AllowGet });
            }
        }
        public async Task<ActionResult> EditSave1(SubCategory ESOBJ, FormCollection form, int data)
        {

            using (DataBaseContext db = new DataBaseContext())
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
                                SubCategory blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.SubCategory.Where(e => e.Id == data)
                                                            .SingleOrDefault();
                                    originalBlogValues = context.Entry(blog).OriginalValues;
                                }

                                ESOBJ.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.EmpId,
                                    ModifiedOn = DateTime.Now
                                };

                                int a = EditS(data, ESOBJ, ESOBJ.DBTrack);



                                using (var context = new DataBaseContext())
                                {
                                    var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                    //DT_SubCategory DT_OBJ = (DT_SubCategory)obj;
                                    //  db.Create(DT_OBJ);
                                    db.SaveChanges();
                                }
                                await db.SaveChangesAsync();
                                ts.Complete();


                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }

                        //catch (DbUpdateException e) { throw e; }
                        //catch (DataException e) { throw e; }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (SubCategory)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var databaseValues = (SubCategory)databaseEntry.ToObject();
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
                    }
                }


                else
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        SubCategory blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        SubCategory Old_Obj = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.SubCategory.Where(e => e.Id == data).SingleOrDefault();
                            originalBlogValues = context.Entry(blog).OriginalValues;
                        }
                        ESOBJ.DBTrack = new DBTrack
                        {
                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                            Action = "M",
                            IsModified = blog.DBTrack.IsModified == true ? true : false,
                            ModifiedBy = SessionManager.EmpId,
                            ModifiedOn = DateTime.Now
                        };
                        SubCategory corp = new SubCategory()
                        {
                            Code = ESOBJ.Code,
                            Details = ESOBJ.Details,
                            Id = data,
                            DBTrack = ESOBJ.DBTrack,
                            RowVersion = (Byte[])TempData["RowVersion"]
                        };


                        //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                        using (var context = new DataBaseContext())
                        {
                            var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "SubCategory", ESOBJ.DBTrack);
                            // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                            Old_Obj = context.SubCategory.Where(e => e.Id == data)
                                .SingleOrDefault();
                            DT_SubCategory DT_Corp = (DT_SubCategory)obj;
                            db.Create(DT_Corp);
                            //db.SaveChanges();
                        }
                        blog.DBTrack = ESOBJ.DBTrack;
                        db.SubCategory.Attach(blog);
                        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        ts.Complete();
                        Msg.Add("  Record Updated");
                        return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = ESOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                }
                return View();
            }
        }

        //[HttpPost]
        //public async Task<ActionResult> EditSave(SubCategory c, int data, FormCollection form) // Edit submit
        //{
        //    List<String> Msg = new List<String>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            string ProgramList = form["ProgramListlist"] == "0" ? "" : form["ProgramListlist"];
        //            if (ProgramList == null)
        //            {
        //                ProgramList = form["ProgramListlistS"] == "0" ? "" : form["ProgramListlistS"];
        //            }

        //            string BudgetList = form["Budgetlist"] == "0" ? "" : form["Budgetlist"];
        //            if (BudgetList == null)
        //            {
        //                BudgetList = form["BudgetlistS"] == "0" ? "" : form["BudgetlistS"];
        //            }

        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;



        //            var db_data = db.SubCategory.Include(e => e.ProgramList).Include(e => e.Budget).Where(e => e.Id == data).SingleOrDefault();
        //            List<ProgramList> SOBJ = new List<ProgramList>();
        //            db_data.ProgramList = null;
        //            if (ProgramList != null)
        //            {
        //                var ids = Utility.StringIdsToListIds(ProgramList);
        //                foreach (var ca in ids)
        //                {
        //                    var Lookup_val = db.ProgramList.Find(ca);
        //                    SOBJ.Add(Lookup_val);
        //                    db_data.ProgramList = SOBJ;
        //                }
        //            }


        //            var db_data1 = db.SubCategory.Include(e => e.ProgramList).Include(e => e.Budget).Where(e => e.Id == data).SingleOrDefault();
        //            List<Budget> SOBJ1 = new List<Budget>();
        //            db_data1.Budget = null;
        //            if (BudgetList != null)
        //            {
        //                var ids = Utility.StringIdsToListIds(BudgetList);
        //                foreach (var ca in ids)
        //                {
        //                    var Lookup_val = db.Budget.Find(ca);
        //                    SOBJ1.Add(Lookup_val);
        //                    db_data1.Budget = SOBJ1;
        //                }
        //            }

        //            if (Auth == false)
        //            {
        //                if (ModelState.IsValid)
        //                {
        //                    //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                    {
        //                        SubCategory blog = null; // to retrieve old data
        //                        DbPropertyValues originalBlogValues = null;

        //                        using (var context = new DataBaseContext())
        //                        {
        //                            blog = context.SubCategory.Where(e => e.Id == data).Include(e => e.ProgramList).Include(e => e.Budget).SingleOrDefault();

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

        //                        //int a = EditS(Geostructlist, PostDetails, data, c, c.DBTrack);

        //                        var CurCorp = db.SubCategory.Find(data);
        //                        TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                        db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                        {
        //                            //c.DBTrack = dbT;
        //                            SubCategory corp = new SubCategory()
        //                            {
        //                                Code = c.Code,
        //                                Details = c.Details == null ? "" : c.Details,
        //                                ProgramList = db_data.ProgramList,
        //                                Budget = db_data1.Budget,
        //                                DBTrack = c.DBTrack,
        //                                Id = data,
        //                            };

        //                            db.SubCategory.Attach(corp);
        //                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                            //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                        }



        //                        using (var context = new DataBaseContext())
        //                        {

        //                            var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                            db.SaveChanges();
        //                        }
        //                        await db.SaveChangesAsync();
        //                        // var aaq = db.SubCategory.Include(e => e.JobSource).Where(e => e.Id == data).SingleOrDefault();
        //                        ts.Complete();
        //                        Msg.Add("Record Updated Successfully.");
        //                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    }
        //                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    SubCategory blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    SubCategory Old_Corp = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.SubCategory.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }
        //                    c.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };

        //                    if (TempData["RowVersion"] == null)
        //                    {
        //                        TempData["RowVersion"] = blog.RowVersion;
        //                    }


        //                    SubCategory corp = new SubCategory()
        //                    {
        //                        Code = c.Code,
        //                        Details = c.Details == null ? "" : c.Details,
        //                        ProgramList = db_data.ProgramList,
        //                        Budget = db_data1.Budget,
        //                        DBTrack = c.DBTrack,
        //                        Id = data,
        //                        RowVersion = (Byte[])TempData["RowVersion"]
        //                    };


        //                    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        var obj = DBTrackFile.ModifiedDataHistory("Recruitement/Recruitement", "M", blog, corp, "SubCategory", c.DBTrack);
        //                        // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                        //Old_Corp = context.SubCategory.Where(e => e.Id == data).Include(e => e.PostDetails)
        //                        //    .Include(e => e.Geostruct).SingleOrDefault();
        //                        //DT_SubCategory DT_Corp = (DT_SubCategory)obj;
        //                        //DT_Corp.PostDetails_Id = DBTrackFile.ValCompare(Old_Corp.PostDetails, c.PostDetails);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                        //DT_Corp.Geostruct_Id = DBTrackFile.ValCompare(Old_Corp.Geostruct, c.Geostruct); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
        //                        //// DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        //                        //db.Create(DT_Corp);
        //                        //db.SaveChanges();
        //                    }
        //                    blog.DBTrack = c.DBTrack;
        //                    db.SubCategory.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    Msg.Add("Record Updated Successfully.");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }

        //            }
        //        }
        //        catch (DbUpdateConcurrencyException ex)
        //        {
        //            var entry = ex.Entries.Single();
        //            var clientValues = (SubCategory)entry.Entity;
        //            var databaseEntry = entry.GetDatabaseValues();
        //            if (databaseEntry == null)
        //            {
        //                Msg.Add(ex.Message);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //            }
        //            else
        //            {
        //                var databaseValues = (SubCategory)databaseEntry.ToObject();
        //                c.RowVersion = databaseValues.RowVersion;

        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Msg.Add(e.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //        return View();
        //    }
        //}

        //[HttpPost]
        //public async Task<ActionResult> EditSave(SubCategory c, int data, FormCollection form) // Edit submit
        //{
        //    List<String> Msg = new List<String>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            //var vb_data = db.SubCategory.Include(a => a.SubCategory).Include(a => a.Budget)
        //            //    .Where(e => e.Id == data).SingleOrDefault();

        //            string ProgramList = form["ProgramListlistS"] == "0" ? "" : form["ProgramListlistS"];
        //            if (ProgramList == null)
        //            {
        //                ProgramList = form["ProgramListlistS"] == "0" ? "" : form["ProgramListlistS"];
        //            }

        //            //string BudgetList = form["Budgetlist"] == "0" ? "" : form["Budgetlist"];
        //            //if (BudgetList == null)
        //            //{
        //            //    BudgetList = form["BudgetlistS"] == "0" ? "" : form["BudgetlistS"];
        //            //}


        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //            var db_data = db.SubCategory
        //                //.Include(e => e.Budget)
        //                .Include(e => e.ProgramList).Where(e => e.Id == data).SingleOrDefault();

        //            //var SubBudget = db_data.SubCategory.Select(e => e.Budget).ToString();


        //            //List<ProgramList> ObjITsection = new List<ProgramList>();

        //            //if (ProgramList != "" && ProgramList != null)
        //            //{
        //            //    var ids = Utility.StringIdsToListIds(ProgramList);
        //            //    foreach (var ca in ids)
        //            //    {
        //            //        var value = db.ProgramList.Find(ca);
        //            //        ObjITsection.Add(value);
        //            //        db_data.ProgramList = ObjITsection;

        //            //    }
        //            //}
        //            //else
        //            //{
        //            //    db_data.ProgramList = null;
        //            //}

        //            //List<Budget> ObjBudsection = new List<Budget>();

        //            //if ( BudgetList != ""&& BudgetList != null)
        //            //{
        //            //    var ids = Utility.StringIdsToListIds(BudgetList);
        //            //    foreach (var ba in ids)
        //            //    {
        //            //        var value = db.Budget.Find(ba);
        //            //        ObjBudsection.Add(value);
        //            //        db_data.Budget = ObjBudsection;
        //            //    }
        //            //}
        //            //else
        //            //{
        //            //    db_data.Budget = null;
        //            //}





        //            db.SubCategory.Attach(db_data);
        //            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //            TempData["RowVersion"] = db_data.RowVersion;
        //            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

        //            if (Auth == false)
        //            {
        //                if (ModelState.IsValid)
        //                {
        //                    //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                    {
        //                        SubCategory blog = null; // to retrieve old data
        //                        DbPropertyValues originalBlogValues = null;

        //                        using (var context = new DataBaseContext())
        //                        {
        //                            blog = context.SubCategory.Where(e => e.Id == data)
        //                                //.Include(e => e.Budget)
        //                                                    .Include(e => e.ProgramList).SingleOrDefault();
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


        //                        //  int a = EditS(data, c, c.DBTrack);

        //                        var CurOBJ = db.SubCategory.Find(data);
        //                        TempData["CurrRowVersion"] = CurOBJ.RowVersion;
        //                        db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
        //                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                        {
        //                            SubCategory ESIOBJ = new SubCategory()
        //                            {
        //                                Id = data,
        //                                Code = c.Code,
        //                                Details = c.Details,
        //                                DBTrack = c.DBTrack
        //                            };

        //                            db.SubCategory.Attach(ESIOBJ);
        //                            db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
        //                            db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                            //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                        }

        //                        await db.SaveChangesAsync();
        //                        using (var context = new DataBaseContext())
        //                        {

        //                            //To save data in history table 
        //                            var Obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, c, "SubCategory", c.DBTrack);
        //                            DT_SubCategory DT_Cat = (DT_SubCategory)Obj;
        //                            db.DT_SubCategory.Add(DT_Cat);
        //                            db.SaveChanges();
        //                        }
        //                        ts.Complete();
        //                        Msg.Add("  Record Updated");
        //                        return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    }

        //                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    SubCategory blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    SubCategory Old_Corp = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.SubCategory.Where(e => e.Id == data).Include(e => e.ProgramList).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }
        //                    c.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };

        //                    if (TempData["RowVersion"] == null)
        //                    {
        //                        TempData["RowVersion"] = blog.RowVersion;
        //                    }

        //                    SubCategory corp = new SubCategory()
        //                    {
        //                        Code = c.Code,
        //                        Details = c.Details == null ? "" : c.Details,
        //                        ProgramList = c.ProgramList,
        //                        DBTrack = c.DBTrack,
        //                        Id = data,
        //                        RowVersion = (Byte[])TempData["RowVersion"]
        //                    };


        //                    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "Corporate", c.DBTrack);
        //                        // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                        Old_Corp = context.SubCategory.Where(e => e.Id == data)
        //                            //.Include(e => e.Budget)
        //                            .Include(e => e.ProgramList).SingleOrDefault();
        //                        //DT_Corporate DT_Corp = (DT_Corporate)obj;
        //                        //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                        //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
        //                        //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        //                        //db.Create(DT_Corp);
        //                        //db.SaveChanges();
        //                    }
        //                    blog.DBTrack = c.DBTrack;
        //                    db.SubCategory.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    Msg.Add("Record Updated Successfully.");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }

        //            }
        //        }
        //        catch (DbUpdateConcurrencyException ex)
        //        {
        //            var entry = ex.Entries.Single();
        //            var clientValues = (SubCategory)entry.Entity;
        //            var databaseEntry = entry.GetDatabaseValues();
        //            if (databaseEntry == null)
        //            {
        //                Msg.Add(ex.Message);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //            }
        //            else
        //            {
        //                var databaseValues = (SubCategory)databaseEntry.ToObject();
        //                c.RowVersion = databaseValues.RowVersion;

        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Msg.Add(e.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //        return View();
        //    }
        //}

        //[HttpPost]
        //public async Task<ActionResult> EditSave(SubCategory c, int data, FormCollection form) // Edit submit
        //{
        //    List<String> Msg = new List<String>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {

        //        bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //        string ProgramList = form["ProgramListlistS"] == "0" ? "" : form["ProgramListlistS"];
        //        if (ProgramList == null)
        //        {
        //            ProgramList = form["ProgramListlistS"] == "0" ? "" : form["ProgramListlistS"];
        //        }

        //        var db_data = db.SubCategory
        //            //.Include(e => e.Budget)
        //            .Include(e => e.ProgramList).Where(e => e.Id == data).SingleOrDefault();

        //        List<ProgramList> ObjITsection = new List<ProgramList>();
        //        string Values = form["ProgramListlistS"];

        //        if (Values != null && Values != "")
        //        {
        //            var ids = Utility.StringIdsToListIds(Values);
        //            foreach (var ca in ids)
        //            {
        //                var value = db.ProgramList.Find(ca);
        //                ObjITsection.Add(value);
        //                db_data.ProgramList = ObjITsection;

        //            }
        //        }
        //        else
        //        {
        //            db_data.ProgramList = null;
        //        }

        //        var std = db.SubCategory.Find(data);
        //        std.Code = c.Code;
        //        std.Details = c.Details;
        //        std.ProgramList = db_data.ProgramList;

        //        int? CatId = db.SubCategory.Where(e => e.Id == data).FirstOrDefault().Category_Id;

        //        List<SubCategory> SubCatList = new List<SubCategory>();
        //        Category Cat = db.Category.Include(e => e.SubCategory).Where(e => e.Id == CatId).FirstOrDefault();
        //        SubCatList.AddRange(Cat.SubCategory);
        //        //SubCatList.Add(std);
        //        Cat.SubCategory = SubCatList;

        //        db.SaveChanges();

        //        return View();
        //    }
        //}


        //[HttpPost] ////new
        //public async Task<ActionResult> EditSave(SubCategory c, int data, FormCollection form) // Edit submit
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<String> Msg = new List<String>();
        //        try
        //        {
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //            string ProgramList = form["ProgramListlistS"] == "0" ? "" : form["ProgramListlistS"];
        //            if (ProgramList == null)
        //            {
        //                ProgramList = form["ProgramListlistS"] == "0" ? "" : form["ProgramListlistS"];
        //            }

        //            var db_data = db.SubCategory
        //                //.Include(e => e.Budget)
        //                .Include(e => e.ProgramList).Where(e => e.Id == data).SingleOrDefault();

        //            List<ProgramList> ObjITsection = new List<ProgramList>();
        //            string Values = form["ProgramListlistS"];

        //            if (Values != null && Values != "")
        //            {
        //                var ids = Utility.StringIdsToListIds(Values);
        //                foreach (var ca in ids)
        //                {
        //                    var value = db.ProgramList.Find(ca);
        //                    ObjITsection.Add(value);
        //                    db_data.ProgramList = ObjITsection;

        //                }
        //            }
        //            else
        //            {
        //                db_data.ProgramList = null;
        //            }




        //            if (ModelState.IsValid)
        //            {
        //                //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    SubCategory blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.SubCategory.Where(e => e.Id == data)
        //                                                .Include(e => e.ProgramList).SingleOrDefault();
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


        //                    //var CurCorp = db.SubCategory.Find(data);
        //                    //TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                    //db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;

        //                    //SubCategory corp = new SubCategory()
        //                    //{
        //                    //    Code = c.Code,
        //                    //    Details = c.Details == null ? "" : c.Details,
        //                    //    ProgramList = pd.ProgramList,

        //                    //    DBTrack = c.DBTrack,
        //                    //    Id = data,
        //                    //};


        //                    //db.SubCategory.Attach(corp);
        //                    //db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //                    //db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    //await db.SaveChangesAsync();

        //                    var std = db.SubCategory.Find(data);
        //                    TempData["CurrRowVersion"] = std.RowVersion;
        //                    std.Code = c.Code;
        //                    std.Details = c.Details;
        //                    std.ProgramList = db_data.ProgramList;

        //                    int? CatId = db.SubCategory.Where(e => e.Id == data).FirstOrDefault().Category_Id;

        //                    List<SubCategory> SubCatList = new List<SubCategory>();
        //                    Category Cat = db.Category.Include(e => e.SubCategory).Where(e => e.Id == CatId).FirstOrDefault();
        //                    SubCatList.AddRange(Cat.SubCategory);
        //                    //SubCatList.Add(std);
        //                    Cat.SubCategory = SubCatList;

        //                    db.SaveChanges();

        //                    ts.Complete();

        //                    Msg.Add("Record Updated Successfully.");
        //                    return Json(new { status = true, Id = std.Id, Val = std.FullDetails, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return Json(new Utility.JsonReturnClass { Id = std.Id, Val = std.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }

        //                Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //            }

        //        }
        //        catch (DbUpdateConcurrencyException ex)
        //        {
        //            var entry = ex.Entries.Single();
        //            var clientValues = (ProgramList)entry.Entity;
        //            var databaseEntry = entry.GetDatabaseValues();
        //            if (databaseEntry == null)
        //            {
        //                Msg.Add(ex.Message);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //            }
        //            else
        //            {
        //                var databaseValues = (ProgramList)databaseEntry.ToObject();
        //                c.RowVersion = databaseValues.RowVersion;

        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Msg.Add(e.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //        return View();
        //    }
        //}

        [HttpPost]
        public async Task<ActionResult> EditSave(SubCategory c, int data, FormCollection form) // Edit submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //var vb_data = db.SubCategory.Include(a => a.SubCategory).Include(a => a.Budget)
                    //    .Where(e => e.Id == data).SingleOrDefault();

                    string ProgramList = form["ProgramListlist"] == "0" ? "" : form["ProgramListlist"];
                    if (ProgramList == null)
                    {
                        ProgramList = form["ProgramListlistS"] == "0" ? "" : form["ProgramListlistS"];
                    }

                    

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    var db_data = db.SubCategory.Include(e => e.ProgramList).Where(e => e.Id == data).SingleOrDefault();

                    //var SubBudget = db_data.SubCategory.Select(e => e.Budget).ToString();


                    List<ProgramList> ObjITsection = new List<ProgramList>();

                    if (ProgramList != "" && ProgramList != null)
                    {
                        var ids = Utility.StringIdsToListIds(ProgramList);
                        foreach (var ca in ids)
                        {
                            var value = db.ProgramList.Find(ca);
                            ObjITsection.Add(value);
                            db_data.ProgramList = ObjITsection;

                        }
                    }
                    else
                    {
                        db_data.ProgramList = null;
                    }

                    
                    db.SubCategory.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    TempData["RowVersion"] = db_data.RowVersion;
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                SubCategory blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.SubCategory.Where(e => e.Id == data)
                                                            .Include(e => e.ProgramList).SingleOrDefault();
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


                                //  int a = EditS(data, c, c.DBTrack);

                                var CurOBJ = db.SubCategory.Find(data);
                                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    SubCategory ESIOBJ = new SubCategory()
                                    {
                                        Id = data,
                                        Code = c.Code,
                                        Details = c.Details,
                                        DBTrack = c.DBTrack
                                    };

                                    db.SubCategory.Attach(ESIOBJ);
                                    db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                }

                                await db.SaveChangesAsync();
                                using (var context = new DataBaseContext())
                                {

                                    //To save data in history table 
                                    var Obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, c, "SubCategory", c.DBTrack);
                                    DT_SubCategory DT_Cat = (DT_SubCategory)Obj;
                                    db.DT_SubCategory.Add(DT_Cat);
                                    db.SaveChanges();
                                }
                                ts.Complete();
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

                            SubCategory blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            SubCategory Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.SubCategory.Where(e => e.Id == data).Include(e => e.ProgramList).SingleOrDefault();
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

                            SubCategory corp = new SubCategory()
                            {
                                Code = c.Code,
                                Details = c.Details == null ? "" : c.Details,
                                ProgramList = c.ProgramList,
                                DBTrack = c.DBTrack,
                                Id = data,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "Corporate", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.SubCategory.Where(e => e.Id == data)
                                    .Include(e => e.ProgramList).SingleOrDefault();
                                //DT_Corporate DT_Corp = (DT_Corporate)obj;
                                //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                //db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.SubCategory.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("Record Updated Successfully.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var clientValues = (SubCategory)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (SubCategory)databaseEntry.ToObject();
                        c.RowVersion = databaseValues.RowVersion;

                    }
                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                return View();
            }
        }
        public ActionResult GridDelete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SubCategory SubCategory = db.SubCategory
                    //.Include(e => e.Budget)
                    .Include(e => e.ProgramList).Where(e => e.Id == data).SingleOrDefault();
                try
                {
                    //var selectedValues = SubCategory.Budget;
                    //var lkValue = new HashSet<int>(SubCategory.Budget.Select(e => e.Id));
                    //if (lkValue.Count > 0)
                    //{
                    //    Msg.Add("Child record exists.Cannot delete.");
                    //    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    var lkValue1 = new HashSet<int>(SubCategory.ProgramList.Select(e => e.Id));
                    if (lkValue1.Count > 0)
                    {
                        Msg.Add("Child record exists.Cannot delete.");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(SubCategory).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                catch (DataException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
            }
        }

        public int EditS(int data, SubCategory ESOBJ, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CurOBJ = db.SubCategory.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    ESOBJ.DBTrack = dbT;
                    SubCategory ESIOBJ = new SubCategory()
                    {
                        Id = data,
                        Code = ESOBJ.Code,
                        Details = ESOBJ.Details,
                        DBTrack = ESOBJ.DBTrack
                    };

                    db.SubCategory.Attach(ESIOBJ);
                    db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }
        public ActionResult Delete(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SubCategory SubCategory = db.SubCategory
                    //.Include(e => e.Budget)
                    .Include(e => e.ProgramList).Where(e => e.Id == data).SingleOrDefault();
                try
                {
                    //var selectedValues = SubCategory.Budget;
                    //var lkValue = new HashSet<int>(SubCategory.Budget.Select(e => e.Id));
                    //if (lkValue.Count > 0)
                    //{
                    //    Msg.Add("Child record exists.Cannot delete.");
                    //    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    var lkValue1 = new HashSet<int>(SubCategory.ProgramList.Select(e => e.Id));
                    if (lkValue1.Count > 0)
                    {
                        Msg.Add("Child record exists.Cannot delete.");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(SubCategory).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                catch (DataException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
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
                var LKVal = db.SubCategory.ToList();

                if (gp.IsAutho == true)
                {
                    LKVal = db.SubCategory.AsNoTracking().Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    LKVal = db.SubCategory.AsNoTracking().ToList();
                }


                IEnumerable<SubCategory> IE;
                if (!string.IsNullOrEmpty(gp.searchString))
                {
                    IE = LKVal;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.Code, a.Details }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Code == gp.searchString.ToLower()) || (e.Details == gp.searchString.ToLower())));
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Details }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = LKVal;
                    Func<SubCategory, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.Code :
                                         gp.sidx == "Details " ? c.Details :

                                         "");
                    }

                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.Details }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.Details }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Details }).ToList();
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
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (auth_action == "C")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        SubCategory ESI = db.SubCategory
                            .FirstOrDefault(e => e.Id == auth_id);

                        ESI.DBTrack = new DBTrack
                        {
                            Action = "C",
                            ModifiedBy = ESI.DBTrack.ModifiedBy != null ? ESI.DBTrack.ModifiedBy : null,
                            CreatedBy = ESI.DBTrack.CreatedBy != null ? ESI.DBTrack.CreatedBy : null,
                            CreatedOn = ESI.DBTrack.CreatedOn != null ? ESI.DBTrack.CreatedOn : null,
                            IsModified = ESI.DBTrack.IsModified == true ? false : false,
                            AuthorizedBy = SessionManager.EmpId,
                            AuthorizedOn = DateTime.Now
                        };

                        db.SubCategory.Attach(ESI);
                        db.Entry(ESI).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(ESI).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //db.SaveChanges();
                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, ESI.DBTrack);
                        DT_SubCategory DT_OBJ = (DT_SubCategory)rtn_Obj;

                        db.Create(DT_OBJ);
                        await db.SaveChangesAsync();

                        ts.Complete();
                        Msg.Add("  Record Authorised");
                        return Json(new Utility.JsonReturnClass { Id = ESI.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (auth_action == "M")
                {

                    SubCategory Old_OBJ = db.SubCategory
                                            .Where(e => e.Id == auth_id).SingleOrDefault();


                    DT_SubCategory Curr_OBJ = db.DT_SubCategory
                                                .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                .OrderByDescending(e => e.Id)
                                                .FirstOrDefault();

                    if (Curr_OBJ != null)
                    {
                        SubCategory SubCategory = new SubCategory();

                        SubCategory.Code = Curr_OBJ.Code == null ? Old_OBJ.Code : Curr_OBJ.Code;
                        SubCategory.Details = Curr_OBJ.Details == null ? Old_OBJ.Details : Curr_OBJ.Details;

                        //      corp.Id = auth_id;

                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    // db.Configuration.AutoDetectChangesEnabled = false;
                                    SubCategory.DBTrack = new DBTrack
                                    {
                                        CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
                                        CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = Old_OBJ.DBTrack.ModifiedBy == null ? null : Old_OBJ.DBTrack.ModifiedBy,
                                        ModifiedOn = Old_OBJ.DBTrack.ModifiedOn == null ? null : Old_OBJ.DBTrack.ModifiedOn,
                                        AuthorizedBy = SessionManager.EmpId,
                                        AuthorizedOn = DateTime.Now,
                                        IsModified = false
                                    };

                                    int a = EditS(auth_id, SubCategory, SubCategory.DBTrack);

                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    Msg.Add("  Record Authorised");
                                    return Json(new Utility.JsonReturnClass { Id = SubCategory.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (SubCategory)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    var databaseValues = (SubCategory)databaseEntry.ToObject();
                                    SubCategory.RowVersion = databaseValues.RowVersion;
                                }
                            }

                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                }
                else if (auth_action == "D")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //SubCategory corp = db.SubCategory.Find(auth_id);
                        SubCategory ESI = db.SubCategory.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

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
                            AuthorizedBy = SessionManager.EmpId,
                            AuthorizedOn = DateTime.Now
                        };

                        db.SubCategory.Attach(ESI);
                        db.Entry(ESI).State = System.Data.Entity.EntityState.Deleted;


                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, ESI.DBTrack);
                        DT_SubCategory DT_OBJ = (DT_SubCategory)rtn_Obj;
                        //DT_OBJ.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                        //DT_OBJ.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                        //DT_OBJ.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                        db.Create(DT_OBJ);
                        await db.SaveChangesAsync();
                        db.Entry(ESI).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        Msg.Add(" Record Authorised ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                return View();
            }
        }




        public class returnDataClass
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Details { get; set; }
        }

        public ActionResult GridEditData(int data)
        {
            var returnlist = new List<returnDataClass>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != null && data != 0)
                {
                    var retrundataList = db.SubCategory.Where(e => e.Id == data).ToList();
                    foreach (var a in retrundataList)
                    {
                        returnlist.Add(new returnDataClass()
                        {
                            Id = a.Id,
                            Code = a.Code,
                            Details = a.Details,

                        });
                    }


                    List<ProgramList1> fext = new List<ProgramList1>();


                    var add_data = db.SubCategory
                                   .Include(e => e.ProgramList)
                                 .Where(e => e.Id == data).ToList();

                    foreach (var ca in add_data)
                    {
                        fext.Add(new ProgramList1
                        {
                            ProgramList_Id = ca.ProgramList.Select(e => e.Id.ToString()).ToArray(),
                            ProgramList_val = ca.ProgramList.Select(e => e.FullDetails).ToArray(),

                        });
                    }


                    // return Json(new { returndata = returnlist }, JsonRequestBehavior.AllowGet);
                    return this.Json(new Object[] { returnlist, fext, "", "", "", JsonRequestBehavior.AllowGet });
                }
                else
                {
                    return View();
                }
            }
        }
    }
}
