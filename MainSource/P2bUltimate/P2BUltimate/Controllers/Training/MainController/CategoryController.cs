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
    public class CategoryController : Controller
    {
        List<string> Msg = new List<string>();
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /Category/

        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/Category/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Recruitement/_Category.cshtml");
        }
        public ActionResult Partial1()
        {
            return View("~/Views/Shared/Training/_Category.cshtml");
        }

        //[HttpPost]       
        //public ActionResult Create(Category COBJ, FormCollection form)
        //{


        //    if (ModelState.IsValid)
        //    {
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            if (db.Category.Any(o => o.Code == COBJ.Code))
        //            {
        //                Msg.Add("  Code Already Exists.  ");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            }

        //            COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId, IsModified = false };

        //            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId };
        //            string TrainingType = form["SubCategorylist"] == "0" ? "" : form["SubCategorylist"];

        //            //string Addrs = form["BudgetParameterslist"] == "0" ? "" : form["BudgetParameterslist"];


        //            if (TrainingType != null)
        //            {
        //                if (TrainingType != "")
        //                {
        //                    var ids = Utility.StringIdsToListIds(TrainingType);
        //                    List<SubCategory> SubCategoryList = new List<SubCategory>();
        //                    foreach (var item in ids)
        //                    {
        //                        var val = db.SubCategory.Find(item);
        //                        SubCategoryList.Add(val);
        //                    }
        //                    COBJ.SubCategory = SubCategoryList;
        //                }
        //            }
        //            Category Category = new Category()
        //            {

        //                Code = COBJ.Code,
        //                Details = COBJ.Details,
        //                Budget=COBJ.Budget,
        //                SubCategory = COBJ.SubCategory,
        //                DBTrack = COBJ.DBTrack

        //            };
        //            try
        //            {
        //                db.Category.Add(Category);
        //                var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, COBJ.DBTrack);
        //                DT_Category DT_OBJ = (DT_Category)rtn_Obj;
        //                db.Create(DT_OBJ);
        //                db.SaveChanges();

        //                ts.Complete();
        //                Msg.Add("  Data Saved successfully  ");
        //                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            }

        //            catch (DbUpdateConcurrencyException)
        //            {
        //                return RedirectToAction("Create", new { concurrencyError = true, id = COBJ.Id });
        //            }
        //            catch (DataException /* dex */)
        //            {
        //                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            }
        //        }
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
        //        Msg.Add(errorMsg);
        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //    }

        //}
        public class BudgetD
        {
            public Array Bud_Credit { get; set; }
            public Array Bud_Debit { get; set; }
        }
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(Category p, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string SubCategorylist = form["SubCategorylist"] == "0" ? "" : form["SubCategorylist"];
                string Budgetlist = form["BudgetlistC"] == "0" ? "" : form["BudgetlistC"];

                var id = int.Parse(Session["CompId"].ToString());
                var companypayroll = db.CompanyTraining.Where(e => e.Company.Id == id).SingleOrDefault();

                List<String> Msg = new List<String>();
                try
                {
                    //p.FuncStruct.JobPosition = null;
                    //List<FuncStruct> job = new List<FuncStruct>();
                    //string val2 = form["JobPositionMlist"];

                    //if (val2 != null && val2 != "")
                    //{
                    //    var ids = Utility.StringIdsToListIds(val2);
                    //    foreach (var ca in ids)
                    //    {
                    //        var OBJ_val = db.FuncStruct.Find(ca);
                    //        job.Add(OBJ_val);
                    //        p.FuncStruct = job;
                    //    }
                    //}


                    p.SubCategory = null;
                    List<SubCategory> OBJ = new List<SubCategory>();

                    if (SubCategorylist != null && SubCategorylist != "")
                    {
                        var ids = Utility.StringIdsToListIds(SubCategorylist);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.SubCategory.Find(ca);
                            OBJ.Add(OBJ_val);
                            p.SubCategory = OBJ;
                        }
                    }

                    //p.Budget = null;
                    //List<Budget> OBJ1 = new List<Budget>();

                    //if (Budgetlist != null && Budgetlist != "")
                    //{
                    //    var ids = Utility.StringIdsToListIds(Budgetlist);
                    //    foreach (var ba in ids)
                    //    {
                    //        var OBJ1_val = db.Budget.Find(ba);
                    //        OBJ1.Add(OBJ1_val);
                    //        p.Budget = OBJ1;
                    //    }
                    //}

                    //List<BudgetD> pst = new List<BudgetD>();
                    //foreach (var data in p.SubCategory)
                    //{
                    //    //var aas = db.ProgramList.Include(a => a.Budget).Where(e => e.Id == data.Id).ToList();
                    //    var aas = db.SubCategory.Where(e => e.Id == data.Id).ToList();
                    //    foreach (var ca in aas)
                    //    {
                    //        pst.Add(new BudgetD
                    //        {
                    //            Bud_Credit = ca.Budget.Select(e => e.BudgetCredit).ToArray(),
                    //            Bud_Debit = ca.Budget.Select(e => e.BudgetDebit).ToArray(),
                    //        });

                    //        var bud = p.Budget.ToList();

                    //        foreach (var b in bud)
                    //        {
                    //            int Bc = Int32.Parse(b.BudgetCredit.ToString());
                    //            int Bd = Int32.Parse(b.BudgetDebit.ToString());

                    //            foreach (var k in pst)
                    //            {
                    //                foreach (var Ba in k.Bud_Credit)
                    //                {
                    //                    int Bct = Int32.Parse(Ba.ToString());
                    //                    if (Bct > Bc)
                    //                    {
                    //                        Msg.Add("Subcategory budget should be same or under subcategory budget");
                    //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //                    }
                    //                }
                    //                foreach (var Bb in k.Bud_Debit)
                    //                {
                    //                    int Bdt = Int32.Parse(Bb.ToString());
                    //                    if (Bdt > Bd)
                    //                    {
                    //                        Msg.Add("Subcategory budget should be same or under subcategory budget");
                    //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //                    }
                    //                }
                    //            }
                    //        }



                    //    }
                    //}


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            //if (db.PostDetails.Any(o => o.FuncStruct.JobPosition == p.FuncStruct.JobPosition))
                            //{
                            //    Msg.Add("Code Already Exists.");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}

                            p.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };                     //???????

                            Category Postdetails = new Category()
                            {
                                Code = p.Code,
                                Details = p.Details == null ? "" : p.Details,
                                SubCategory = p.SubCategory,
                                //Budget = p.Budget,
                                DBTrack = p.DBTrack
                            };

                            db.Category.Add(Postdetails);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, p.DBTrack);
                            //DT_Category DT_Post = (DT_Category)rtn_Obj;
                            // db.Create(DT_Post);
                            db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);
                            if (companypayroll != null)
                            {
                                List<Category> pfmasterlist = new List<Category>();
                                pfmasterlist.Add(Postdetails);
                                companypayroll.Category = pfmasterlist;
                                db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(companypayroll).State = System.Data.Entity.EntityState.Detached;
                            }

                            ts.Complete();
                            Msg.Add("Data Saved Successfully.");
                            return Json(new Utility.JsonReturnClass { success = true, Val = Postdetails.FullDetails, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        //catch (DbUpdateConcurrencyException)
                        //{
                        //    return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                        //}
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
                catch (Exception e)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = e.Message,
                        ExceptionStackTrace = e.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        public class SubCategory1
        {
            public Array Category_Id { get; set; }
            public Array Category_val { get; set; }
            public Array Budget_Id { get; set; }
            public Array Budget_val { get; set; }
        }

        //public class Budget1
        //{
        //    public Array Budget_Id { get; set; }
        //    public Array Budget_val { get; set; }
        //}
        [HttpPost]
        public ActionResult Edit(int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var Category = db.Category.Include(a => a.SubCategory)
                    //.Include(a => a.Budget)
                                    .Where(e => e.Id == data).ToList();
                var r = (from ca in Category
                         select new
                         {

                             Id = ca.Id,
                             Code = ca.Code,

                             Details = ca.Details,
                             Action = ca.DBTrack.Action
                         }).Distinct();
                List<SubCategory1> cat = new List<SubCategory1>();
                //List<Budget1> fext = new List<Budget1>();

                var add_data = db.Category
                    //.Include(e => e.Budget)
                               .Include(e => e.SubCategory).Where(e => e.Id == data).ToList();

                foreach (var ca in add_data)
                {
                    cat.Add(new SubCategory1
                    {
                        Category_Id = ca.SubCategory.Select(e => e.Id.ToString()).ToArray(),
                        Category_val = ca.SubCategory.Select(e => e.FullDetails).ToArray(),
                        //Budget_Id = ca.Budget.Select(e => e.Id.ToString()).ToArray(),
                        //Budget_val = ca.Budget.Select(e => e.FullDetails).ToArray()
                    });

                }

                var W = db.DT_Category
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Code = e.Code,
                         Details = e.Details,

                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var LKup = db.Category.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, cat, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        //[HttpPost]
        //public async Task<ActionResult> EditSave(Category c, int data, FormCollection form) // Edit submit
        //{
        //    List<String> Msg = new List<String>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            //var vb_data = db.Category.Include(a => a.SubCategory).Include(a => a.Budget)
        //            //    .Where(e => e.Id == data).SingleOrDefault();


        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //            var db_data = db.Category
        //                //.Include(e => e.Budget)
        //                .Include(e => e.SubCategory).Where(e => e.Id == data).SingleOrDefault();

        //            //var SubBudget = db_data.SubCategory.Select(e => e.Budget).ToString();


        //            List<SubCategory> ObjITsection = new List<SubCategory>();
        //            string Values = form["SubCategorylist"];

        //            if (Values != null && Values != "")
        //            {
        //                var ids = Utility.StringIdsToListIds(Values);
        //                foreach (var ca in ids)
        //                {
        //                    var value = db.SubCategory.Find(ca);
        //                    ObjITsection.Add(value);
        //                    db_data.SubCategory = ObjITsection;

        //                }
        //            }
        //            else
        //            {
        //                db_data.SubCategory = null;
        //            }

        //            //List<Budget> ObjBudsection = new List<Budget>();
        //            //string Values1 = form["BudgetlistC"];



        //            //if (Values1 != null && Values1 != "")
        //            //{
        //            //    var ids = Utility.StringIdsToListIds(Values1);
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

        //            db.Category.Attach(db_data);
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
        //                        Category blog = null; // to retrieve old data
        //                        DbPropertyValues originalBlogValues = null;

        //                        using (var context = new DataBaseContext())
        //                        {
        //                            blog = context.Category.Where(e => e.Id == data)
        //                                //.Include(e => e.Budget)
        //                                                    .Include(e => e.SubCategory).SingleOrDefault();
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

        //                        var CurOBJ = db.Category.Find(data);
        //                        TempData["CurrRowVersion"] = CurOBJ.RowVersion;
        //                        db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
        //                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                        {
        //                            Category ESIOBJ = new Category()
        //                            {
        //                                Id = data,
        //                                Code = c.Code,
        //                                Details = c.Details,
        //                                DBTrack = c.DBTrack
        //                            };

        //                            db.Category.Attach(ESIOBJ);
        //                            db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
        //                            db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                            //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                        }

        //                        await db.SaveChangesAsync();
        //                        using (var context = new DataBaseContext())
        //                        {

        //                            //To save data in history table 
        //                            var Obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, c, "Category", c.DBTrack);
        //                            DT_Category DT_Cat = (DT_Category)Obj;
        //                            db.DT_Category.Add(DT_Cat);
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

        //                    Category blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    SubCategory Old_Corp = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.Category.Where(e => e.Id == data).Include(e => e.SubCategory).SingleOrDefault();
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

        //                    Category corp = new Category()
        //                    {
        //                        Code = c.Code,
        //                        Details = c.Details == null ? "" : c.Details,
        //                        SubCategory = c.SubCategory,
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
        //                    db.Category.Attach(blog);
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
        //            var clientValues = (Category)entry.Entity;
        //            var databaseEntry = entry.GetDatabaseValues();
        //            if (databaseEntry == null)
        //            {
        //                Msg.Add(ex.Message);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //            }
        //            else
        //            {
        //                var databaseValues = (Category)databaseEntry.ToObject();
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
        //public async Task<ActionResult> EditSave(Category c, int data, FormCollection form) // Edit submit
        //{
        //    List<String> Msg = new List<String>();

        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //        var db_data = db.Category
        //            //.Include(e => e.Budget)
        //            .Include(e => e.SubCategory).Where(e => e.Id == data).SingleOrDefault();
                
        //        List<SubCategory> ObjITsection = new List<SubCategory>();
        //        string Values = form["SubCategorylist"];

        //        if (Values != null && Values != "")
        //        {
        //            var ids = Utility.StringIdsToListIds(Values);
        //            foreach (var ca in ids)
        //            {
        //                var value = db.SubCategory.Find(ca);
        //                ObjITsection.Add(value);
        //                db_data.SubCategory = ObjITsection;

        //            }
        //        }
        //        else
        //        {
        //            db_data.SubCategory = null;
        //        }

        //        var std = db.Category.Find(data);
        //        std.Code = c.Code;
        //        std.Details = c.Details;
        //        std.SubCategory = db_data.SubCategory;
        //        db.SaveChanges();

        //        return View();
        //    }
        //}

        //[HttpPost] /////new
        //public async Task<ActionResult> EditSave(Category c, int data, FormCollection form) // Edit submit
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<String> Msg = new List<String>();
        //        try
        //        {
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //            var db_data = db.Category
        //                //.Include(e => e.Budget)
        //                .Include(e => e.SubCategory).Where(e => e.Id == data).SingleOrDefault();

        //            List<SubCategory> ObjITsection = new List<SubCategory>();
        //            string Values = form["SubCategorylist"];

        //            if (Values != null && Values != "")
        //            {
        //                var ids = Utility.StringIdsToListIds(Values);
        //                foreach (var ca in ids)
        //                {
        //                    var value = db.SubCategory.Find(ca);
        //                    ObjITsection.Add(value);
        //                    db_data.SubCategory = ObjITsection;

        //                }
        //            }
        //            else
        //            {
        //                db_data.SubCategory = null;
        //            }




        //            if (ModelState.IsValid)
        //            {
        //                //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    Category blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.Category.Where(e => e.Id == data)
        //                                                .Include(e => e.SubCategory).SingleOrDefault();
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


        //                    var std = db.Category.Find(data);
        //                    TempData["CurrRowVersion"] = std.RowVersion;
        //                    std.Code = c.Code;
        //                    std.Details = c.Details;
        //                    std.SubCategory = db_data.SubCategory;
        //                    db.SaveChanges();


        //                    ts.Complete();

        //                    Msg.Add("Record Updated Successfully.");
        //                    return Json(new Utility.JsonReturnClass { Id = std.Id, Val = std.Code, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }

        //                //Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
        public async Task<ActionResult> EditSave(Category c, int data, FormCollection form) // Edit submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //var vb_data = db.Category.Include(a => a.SubCategory).Include(a => a.Budget)
                    //    .Where(e => e.Id == data).SingleOrDefault();


                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    var db_data = db.Category.Include(e => e.SubCategory).Where(e => e.Id == data).SingleOrDefault();

                    //var SubBudget = db_data.SubCategory.Select(e => e.Budget).ToString();


                    List<SubCategory> ObjITsection = new List<SubCategory>();
                    string Values = form["SubCategorylist"];

                    if (Values != null && Values != "")
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var value = db.SubCategory.Find(ca);
                            ObjITsection.Add(value);
                            db_data.SubCategory = ObjITsection;

                        }
                    }
                    else
                    {
                        db_data.SubCategory = null;
                    }

                    
                    db.Category.Attach(db_data);
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
                                Category blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.Category.Where(e => e.Id == data)
                                                            .Include(e => e.SubCategory).SingleOrDefault();
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

                                var CurOBJ = db.Category.Find(data);
                                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    Category ESIOBJ = new Category()
                                    {
                                        Id = data,
                                        Code = c.Code,
                                        Details = c.Details,
                                        DBTrack = c.DBTrack
                                    };

                                    db.Category.Attach(ESIOBJ);
                                    db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                }

                                await db.SaveChangesAsync();
                                using (var context = new DataBaseContext())
                                {

                                    //To save data in history table 
                                    var Obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, c, "Category", c.DBTrack);
                                    DT_Category DT_Cat = (DT_Category)Obj;
                                    db.DT_Category.Add(DT_Cat);
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

                            Category blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            SubCategory Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.Category.Where(e => e.Id == data).Include(e => e.SubCategory).SingleOrDefault();
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

                            Category corp = new Category()
                            {
                                Code = c.Code,
                                Details = c.Details == null ? "" : c.Details,
                                SubCategory = c.SubCategory,
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
                            db.Category.Attach(blog);
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
                    var clientValues = (Category)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (Category)databaseEntry.ToObject();
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

        public int EditS(int data, Category ESOBJ, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CurOBJ = db.Category.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    ESOBJ.DBTrack = dbT;
                    Category ESIOBJ = new Category()
                    {
                        Id = data,
                        Code = ESOBJ.Code,
                        Details = ESOBJ.Details,
                        DBTrack = ESOBJ.DBTrack
                    };

                    db.Category.Attach(ESIOBJ);
                    db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }

        public ActionResult GetSubCategoryLKDetails(List<int> SkipIds)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = new List<SubCategory>();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Category.Include(e => e.SubCategory).Where(e => !e.Id.ToString().Contains(a.ToString())).SelectMany(h => h.SubCategory).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var list1 = db.SubCategory.ToList();
                var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult GetBudgetDetails(List<int> SkipIds)
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = new List<Budget>();
        //        if (SkipIds != null)
        //        {
        //            foreach (var a in SkipIds)
        //            {
        //                if (fall == null)
        //                    fall = db.Category
        //                        //.Include(e => e.Budget)
        //                        .Where(e=> !e.Id.ToString().Contains(a.ToString())).SelectMany(h => h.Budget).ToList();
        //                else
        //                    fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

        //            }
        //        }
        //        var list1 = db.Budget.ToList();
        //        var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
        //        return Json(r, JsonRequestBehavior.AllowGet);
        //    }
        //}


        //[HttpPost]
        ////[ValidateAntiForgeryToken]
        //public ActionResult Delete(int? data)
        //{
        //    Category Category = db.Category.Where(e => e.Id == data).SingleOrDefault();
        //    try
        //    {
        //        //var selectedValues = Category.SocialActivities;
        //        //var lkValue = new HashSet<int>(Category.SocialActivities.Select(e => e.Id));
        //        //if (lkValue.Count > 0)
        //        //{
        //        //    return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
        //        //}

        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            db.Entry(Category).State = System.Data.Entity.EntityState.Deleted;
        //            db.SaveChanges();
        //            ts.Complete();
        //        }
        //        Msg.Add("Data removed.");
        //        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //       // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
        //    }

        //    catch (DataException /* dex */)
        //    {
        //        Msg.Add("Unable to delete. Try again, and if the problem persists contact your system administrator.");
        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        // return this.Json(new Object[] { "", "Unable to delete. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });

        //    }
        //}


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
        //        var LKVal = db.Category.ToList();

        //        if (gp.IsAutho == true)
        //        {
        //            LKVal = db.Category.AsNoTracking().Where(e => e.DBTrack.IsModified == true).ToList();
        //        }
        //        else
        //        {
        //            LKVal = db.Category.AsNoTracking().ToList();
        //        }


        //        IEnumerable<Category> IE;
        //        if (!string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = LKVal;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.Code, a.Details }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Code.ToString() == gp.searchString.ToLower()) || (e.Details.ToString() == gp.searchString.ToLower())));
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Details }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = LKVal;
        //            Func<Category, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() :
        //                                 gp.sidx == "Details " ? c.Details.ToString() :

        //                                 "");
        //            }

        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.Details }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.Details }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Details }).ToList();
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

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    Category corporates = db.Category
                        //.Include(e => e.Budget)
                                                       .Include(e => e.SubCategory).Where(e => e.Id == data).SingleOrDefault();

                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    var subcategory = corporates.SubCategory;
                    if (corporates.DBTrack.IsModified == true)
                    {

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            if (subcategory != null)
                            {
                                var objITSection = new HashSet<int>(corporates.SubCategory.Select(e => e.Id));
                                if (objITSection.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                    // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                }
                            }
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? true : false
                            };
                            corporates.DBTrack = dbT;
                            db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corporates.DBTrack);
                            //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            //DT_Corp.Address_Id = corporates.Address == null ? 0 : corporates.Address.Id;
                            //DT_Corp.BusinessType_Id = corporates.BusinessType == null ? 0 : corporates.BusinessType.Id;
                            //DT_Corp.ContactDetails_Id = corporates.ContactDetails == null ? 0 : corporates.ContactDetails.Id;
                            //db.Create(DT_Corp);
                            //// db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        var asq = corporates.SubCategory;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            if (asq != null)
                            {
                                var corpRegion = new HashSet<int>(corporates.SubCategory.Select(e => e.Id));
                                if (corpRegion.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }


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
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, dbT);
                            //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            //DT_Corp.Address_Id = add == null ? 0 : add.Id;
                            //DT_Corp.BusinessType_Id = val == null ? 0 : val.Id;
                            //DT_Corp.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
                            //db.Create(DT_Corp);

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
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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
                IEnumerable<Category> Category = null;
                if (gp.IsAutho == true)
                {
                    Category = db.Category.Include(e => e.SubCategory)
                        //.Include(e=>e.Budget)
                        .Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Category = db.Category.Include(e => e.SubCategory)
                        //.Include(e=>e.Budget)
                        .AsNoTracking().ToList();
                }

                IEnumerable<Category> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Category;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.Code, a.FullDetails }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Code.ToLower() == gp.searchString.ToLower()) || (e.FullDetails.ToLower() == gp.searchString.ToLower()))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Category;
                    Func<Category, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.Code :
                                         gp.sidx == "FullDetails" ? c.FullDetails.ToString() :
                                                                "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.FullDetails) }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.FullDetails) }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.FullDetails }).ToList();
                    }
                    //totalRecords = Category.Count();
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
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (auth_action == "C")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        Category ESI = db.Category
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

                        db.Category.Attach(ESI);
                        db.Entry(ESI).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(ESI).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //db.SaveChanges();
                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, ESI.DBTrack);
                        DT_Category DT_OBJ = (DT_Category)rtn_Obj;

                        db.Create(DT_OBJ);
                        await db.SaveChangesAsync();

                        ts.Complete();
                        Msg.Add("  Record Authorised");
                        return Json(new Utility.JsonReturnClass { Id = ESI.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (auth_action == "M")
                {

                    Category Old_OBJ = db.Category
                                            .Where(e => e.Id == auth_id).SingleOrDefault();


                    DT_Category Curr_OBJ = db.DT_Category
                                                .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                .OrderByDescending(e => e.Id)
                                                .FirstOrDefault();

                    if (Curr_OBJ != null)
                    {
                        Category Category = new Category();

                        Category.Code = Curr_OBJ.Code == null ? Old_OBJ.Code : Curr_OBJ.Code;
                        Category.Details = Curr_OBJ.Details == null ? Old_OBJ.Details : Curr_OBJ.Details;

                        //      corp.Id = auth_id;

                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    // db.Configuration.AutoDetectChangesEnabled = false;
                                    Category.DBTrack = new DBTrack
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

                                    int a = EditS(auth_id, Category, Category.DBTrack);

                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    Msg.Add("  Record Authorised");
                                    return Json(new Utility.JsonReturnClass { Id = Category.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Category)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    var databaseValues = (Category)databaseEntry.ToObject();
                                    Category.RowVersion = databaseValues.RowVersion;
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
                        //Category corp = db.Category.Find(auth_id);
                        Category ESI = db.Category.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

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

                        db.Category.Attach(ESI);
                        db.Entry(ESI).State = System.Data.Entity.EntityState.Deleted;


                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, ESI.DBTrack);
                        DT_Category DT_OBJ = (DT_Category)rtn_Obj;
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


        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Details { get; set; }

        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.Category.Include(e => e.SubCategory)
                        .ToList();
                    // for searchs
                    IEnumerable<Category> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {

                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.Code.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                  || (e.Details.ToString().ToUpper().Contains(param.sSearch.ToUpper()))

                                  ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<Category, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.Code.ToString() : "");
                    var sortcolumn = Request["sSortDir_0"];
                    if (sortcolumn == "asc")
                    {
                        fall = fall.OrderBy(orderfunc);
                    }
                    else
                    {
                        fall = fall.OrderBy(orderfunc);
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
                                Code = item.Code != null ? item.Code : null,
                                Details = item.Details != null ? item.Details : null,

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

                                     select new[] { null, Convert.ToString(c.Id), c.Code,c.Details };
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
                    List<string> Msg = new List<string>();
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = e.Message,
                        ExceptionStackTrace = e.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public class LoanAdvReqChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Details { get; set; }

        }

        public ActionResult Get_SubCat(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.Category
                        .Include(e => e.SubCategory)
                        .Where(e => e.Id == data)
                        .SingleOrDefault();

                    if (db_data.SubCategory != null)
                    {
                        List<LoanAdvReqChildDataClass> returndata = new List<LoanAdvReqChildDataClass>();

                        foreach (var item in db_data.SubCategory.OrderByDescending(e => e.Id))
                        {
                            returndata.Add(new LoanAdvReqChildDataClass
                            {
                                Id = item.Id,
                                Code = item.Code != null ? item.Code : "",
                                Details = item.Details != null ? item.Details : "",

                            });
                        }
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
                    }

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
    }
}
