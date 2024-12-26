using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Transactions;
using P2b.Global;
using Training;
using System.Text;
using P2BUltimate.Controllers;
using P2BUltimate.App_Start;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using P2BUltimate.Models;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Training.MainController
{
    [AuthoriseManger]
    public class ProgramListController : Controller
    {
        List<string> Msg = new List<string>();
        public ActionResult Index()
        {
            //return View("~/Views/Shared/_ProgramList.cshtml");
            return View("~/Views/Training/MainViews/ProgramList/Index.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/Training/_ProgramList.cshtml");

        }

        public class BudgetData
        {
            public string BudgetDetails_Id { get; set; }
            public string BudgetDetails_val { get; set; }
            //public Array BudgetDetails_Id { get; set; }
            //public Array BudgetDetails_val { get; set; }
        }
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var Q = db.ProgramList
                    .Include(e => e.TrainingType)
                    .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        Subject = e.Subject,
                        SubjectDetails = e.SubjectDetails,
                        TrainingType_Id = e.TrainingType.Id == null ? 0 : e.TrainingType.Id,
                        Action = e.DBTrack.Action
                    }).ToList();
                List<BudgetData> budgetparam = new List<BudgetData>();

                var aa = db.ProgramList
                    //.Include(q => q.Budget)
                    .Where(a => a.Id == data).SingleOrDefault();
                //foreach (var item in aa.Budget)
                //{
                //    budgetparam.Add(new BudgetData
                //    {
                //        BudgetDetails_Id=item.Id.ToString(),
                //        BudgetDetails_val = item.FullDetails.ToString(),
                //        //BudgetDetails_Id = (item.Id.ToString()).ToArray(),
                //        //BudgetDetails_val = (item.FullDetails.ToString()).ToArray(),

                //    });
                //}
                var W = db.DT_ProgramList
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Subject = e.Subject == null ? "" : e.Subject,
                         SubjectDetails = e.SubjectDetails == null ? "" : e.SubjectDetails,
                         TrainingType_Val = e.TrainingType_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.TrainingType_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),
                         //  Budget_Val = e.Budget_Id == 0 ? "" : db.Budget.Where(x => x.Id == e.Budget_Id).Select(x => x.FullDetails).FirstOrDefault()

                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.ProgramList.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, budgetparam, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        //public async Task<ActionResult> EditSave(ProgramList c, int data, FormCollection form) // Edit submit
        //{

        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        string Corp = form["TrainingTypelist"] == "0" ? "" : form["TrainingTypelist"];
        //        string trainingtype = form["TrainingTypelist"] == "0" ? "" : form["TrainingTypelist"];
        //        // string ContactDetails = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
        //        //  bool Auth = form["Autho_Action"] == "" ? false : true;
        //        bool Auth = form["Autho_Allow"] == "true" ? true : false;

        //        var db_Data = db.ProgramList.Include(e => e.TrainingType)
        //              .Where(e => e.Id == data).SingleOrDefault();

        //        db_Data.TrainingType = null;


        //        if (trainingtype != null && trainingtype != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(trainingtype));
        //            db_Data.TrainingType = val;
        //        }

        //        List<Budget> lookupLang = new List<Budget>();
        //        string Lang = form["BudgetlistProg"];

        //        if (Lang == null)
        //        {
        //            Lang = form["BudgetlistP"];
        //        }
                
        //        if (Lang != null)
        //        {
        //            var ids = Utility.StringIdsToListIds(Lang);
        //            foreach (var ca in ids)
        //            {
        //                var Lookup_val = db.Budget.Where(e => e.Id == ca).SingleOrDefault();

        //                lookupLang.Add(Lookup_val);
        //            }
        //            db_Data.Budget = lookupLang;
        //        }
        //        else
        //        {
        //            db_Data.Budget = null;
        //        }



        //        if (Auth == false)
        //        {
        //            if (ModelState.IsValid)
        //            {
        //                try
        //                {

        //                    //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                    {


        //                        using (var context = new DataBaseContext())
        //                        {
        //                            db.ProgramList.Attach(db_Data);
        //                            db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
        //                            //db.SaveChanges();
        //                            TempData["RowVersion"] = db_Data.RowVersion;
        //                            db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

        //                            var Curr_OBJ = db.ProgramList.Find(data);
        //                            TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
        //                            db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

        //                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            {
        //                                ProgramList blog = null; // to retrieve old data
        //                                DbPropertyValues originalBlogValues = null;


        //                                blog = context.ProgramList.Where(e => e.Id == data).Include(e => e.TrainingType)

        //                                                       // .Include(e => e.Budget)
        //                                                        .SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;


        //                                c.DBTrack = new DBTrack
        //                                {
        //                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                    Action = "M",
        //                                    ModifiedBy = SessionManager.UserName,
        //                                    ModifiedOn = DateTime.Now
        //                                };

        //                                ProgramList lk = new ProgramList
        //                                {
        //                                    Id = data,
        //                                    Subject = db_Data.Subject,
        //                                    SubjectDetails = db_Data.SubjectDetails,
        //                                    Budget = db_Data.Budget,
        //                                    TrainingType = db_Data.TrainingType,

        //                                    DBTrack = c.DBTrack
        //                                };


        //                                db.ProgramList.Attach(lk);
        //                                db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

        //                                // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
        //                                //db.SaveChanges();
        //                                db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                                var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                                DT_ProgramList DT_LK = (DT_ProgramList)obj;
        //                                //  DT_LK.Allergy = lk.Allergy.Select(e => e.Id);
        //                                db.Create(DT_LK);
        //                                db.SaveChanges();
        //                                await db.SaveChangesAsync();
        //                                db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
        //                                ts.Complete();

        //                                Msg.Add("  Record Updated");
        //                                return Json(new Utility.JsonReturnClass { Id = lk.Id, Val = lk.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                            }
        //                        }
        //                    }
        //                }
        //                catch (DbUpdateConcurrencyException ex)
        //                {
        //                    var entry = ex.Entries.Single();
        //                    var clientValues = (Corporate)entry.Entity;
        //                    var databaseEntry = entry.GetDatabaseValues();
        //                    if (databaseEntry == null)
        //                    {
        //                        return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                    }
        //                    else
        //                    {
        //                        var databaseValues = (Corporate)databaseEntry.ToObject();
        //                        c.RowVersion = databaseValues.RowVersion;

        //                    }
        //                }

        //                return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //            }
        //        }
        //        else
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {

        //                ProgramList blog = null; // to retrieve old data
        //                DbPropertyValues originalBlogValues = null;
        //                ProgramList Old_Corp = null;

        //                using (var context = new DataBaseContext())
        //                {
        //                    blog = context.ProgramList.Where(e => e.Id == data).SingleOrDefault();
        //                    originalBlogValues = context.Entry(blog).OriginalValues;
        //                }
        //                c.DBTrack = new DBTrack
        //                {
        //                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                    Action = "M",
        //                    IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                    ModifiedBy = SessionManager.UserName,
        //                    ModifiedOn = DateTime.Now
        //                };

        //                if (TempData["RowVersion"] == null)
        //                {
        //                    TempData["RowVersion"] = blog.RowVersion;
        //                }

        //                ProgramList corp = new ProgramList()
        //                {
        //                    Subject = c.Subject,
        //                    SubjectDetails = c.SubjectDetails,
        //                    Id = data,
        //                    DBTrack = c.DBTrack,
        //                    RowVersion = (Byte[])TempData["RowVersion"]
        //                };


        //                //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                using (var context = new DataBaseContext())
        //                {
        //                    var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "Corporate", c.DBTrack);
        //                    // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                    Old_Corp = context.ProgramList.Where(e => e.Id == data).Include(e => e.TrainingType)
        //                        .SingleOrDefault();
        //                    DT_ProgramList DT_Corp = (DT_ProgramList)obj;
        //                    // DT_Corp.Budget_Id = DBTrackFile.ValCompare(Old_Corp.Budget, c.Budget);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                    DT_Corp.TrainingType_Id = DBTrackFile.ValCompare(Old_Corp.TrainingType, c.TrainingType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
        //                    //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        //                    db.Create(DT_Corp);
        //                    //db.SaveChanges();
        //                }
        //                blog.DBTrack = c.DBTrack;
        //                db.ProgramList.Attach(blog);
        //                db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                db.SaveChanges();
        //                ts.Complete();

        //                Msg.Add("  Record Updated");
        //                return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = blog.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            }

        //        }
        //        return View();
        //    }

        //}

        [HttpPost]
        public async Task<ActionResult> EditSave(ProgramList c, int data, FormCollection form) // Edit submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //var vb_data = db.ProgramList.Include(a => a.SubCategory).Include(a => a.Budget)
                    //    .Where(e => e.Id == data).SingleOrDefault();
                    string Corp = form["TrainingTypelist"] == "0" ? "" : form["TrainingTypelist"];
                    string trainingtype = form["TrainingTypelist"] == "0" ? "" : form["TrainingTypelist"];
                

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    var db_data = db.ProgramList
                        //.Include(e => e.Budget)
                        .Where(e => e.Id == data).SingleOrDefault();

                    //List<Budget> ObjBudsection = new List<Budget>();

                    //string Lang = form["BudgetlistProg"] == "0" ? "" : form["BudgetlistProg"]; 

                    //if (Lang == null)
                    //{
                    //    Lang = form["BudgetlistP"] == "0" ? "" : form["BudgetlistP"]; 
                    //}

                    //if (Lang != "")
                    //{
                    //    var ids = Utility.StringIdsToListIds(Lang);
                    //    foreach (var ba in ids)
                    //    {
                    //        var value = db.Budget.Find(ba);
                    //        ObjBudsection.Add(value);
                    //        db_data.Budget = ObjBudsection;
                    //    }
                    //}
                    //else
                    //{
                    //    db_data.Budget = null;
                    //}

                    if (trainingtype != null && trainingtype != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(trainingtype));
                        db_data.TrainingType = val;
                    }

                    db.ProgramList.Attach(db_data);
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
                                ProgramList blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.ProgramList.Where(e => e.Id == data)
                                        //.Include(e => e.Budget)
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
                                //  int a = EditS(data, c, c.DBTrack);

                                var CurOBJ = db.ProgramList.Find(data);
                                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    ProgramList ESIOBJ = new ProgramList()
                                    {
                                        Id = data,
                                        Subject = c.Subject,
                                        SubjectDetails = c.SubjectDetails,
                                        TrainingType=db_data.TrainingType,
                                        DBTrack = c.DBTrack
                                    };

                                    db.ProgramList.Attach(ESIOBJ);
                                    db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                }

                                await db.SaveChangesAsync();
                                using (var context = new DataBaseContext())
                                {
                                    //To save data in history table 
                                    var Obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, c, "ProgramList", c.DBTrack);
                                    DT_ProgramList DT_Cat = (DT_ProgramList)Obj;
                                    db.DT_ProgramList.Add(DT_Cat);
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

                            ProgramList blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            SubCategory Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.ProgramList.Where(e => e.Id == data).SingleOrDefault();
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
                             
                            ProgramList corp = new ProgramList()
                            {
                                Id = data,
                                Subject = c.Subject,
                                SubjectDetails = c.SubjectDetails,
                                TrainingType = db_data.TrainingType,
                                RowVersion = (Byte[])TempData["RowVersion"],
                                DBTrack = c.DBTrack,
                            };

                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "Corporate", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.SubCategory.Where(e => e.Id == data)
                                    //.Include(e => e.Budget)
                                    .Include(e => e.ProgramList).SingleOrDefault();
                                //DT_Corporate DT_Corp = (DT_Corporate)obj;
                                //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                //db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.ProgramList.Attach(blog);
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
                    var clientValues = (ProgramList)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (ProgramList)databaseEntry.ToObject();
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


        public int EditS(string Corp, string Addrs, int data, ProgramList c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Corp != null)
                {
                    if (Corp != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Corp));
                        c.TrainingType = val;

                        var type = db.ProgramList.Include(e => e.TrainingType).Where(e => e.Id == data).SingleOrDefault();
                        IList<ProgramList> typedetails = null;
                        if (type.TrainingType != null)
                        {
                            typedetails = db.ProgramList.Where(x => x.TrainingType.Id == type.TrainingType.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.ProgramList.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.TrainingType = c.TrainingType;
                            db.ProgramList.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.ProgramList.Include(e => e.TrainingType).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.TrainingType = null;
                            db.ProgramList.Attach(s);
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
                    var BusiTypeDetails = db.ProgramList.Include(e => e.TrainingType).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.TrainingType = null;
                        db.ProgramList.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                var CurCorp = db.ProgramList.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    ProgramList corp = new ProgramList()
                    {
                        Subject = c.Subject,
                        SubjectDetails = c.SubjectDetails,
                        Id = data,
                        DBTrack = c.DBTrack
                    };
                    db.ProgramList.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    return 1;
                }
                return 0;
            }
        }
        public ActionResult GetLookup(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ProgramList
                    .Include(e => e.TrainingType)
                  .ToList();


                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ProgramList.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                //var list1 = db.FacultySpecialization.ToList();
                //var list2 = fall.Except(list1);
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
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
                        //Corporate corp = db.Corporate.Find(auth_id);
                        //Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                        ProgramList corp = db.ProgramList
                           .Include(e => e.TrainingType).FirstOrDefault(e => e.Id == auth_id);

                        corp.DBTrack = new DBTrack
                        {
                            Action = "C",
                            ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                            CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                            CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                            IsModified = corp.DBTrack.IsModified == true ? false : false,
                            AuthorizedBy = SessionManager.EmpId,
                            AuthorizedOn = DateTime.Now
                        };

                        db.ProgramList.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //db.SaveChanges();
                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corp.DBTrack);
                        DT_ProgramList DT_Corp = (DT_ProgramList)rtn_Obj;

                        DT_Corp.TrainingType_Id = corp.TrainingType == null ? 0 : corp.TrainingType.Id;
                        //DT_Corp.Budget_Id = corp.Budget == null ? 0 : corp.Budget.Id;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();

                        ts.Complete();
                        //return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });

                        Msg.Add("  Record Authorised");
                        return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                }
                else if (auth_action == "M")
                {

                    ProgramList Old_Corp = db.ProgramList.Include(e => e.TrainingType)
                                                      .Where(e => e.Id == auth_id).SingleOrDefault();


                    DT_ProgramList Curr_Corp = db.DT_ProgramList
                                                .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                .OrderByDescending(e => e.Id)
                                                .FirstOrDefault();

                    if (Curr_Corp != null)
                    {
                        ProgramList corp = new ProgramList();

                        string Corp = Curr_Corp.TrainingType_Id == null ? null : Curr_Corp.TrainingType_Id.ToString();
                        string Budget = Curr_Corp.BudgetParameters_Id == null ? null : Curr_Corp.BudgetParameters_Id.ToString();
                        corp.Subject = Curr_Corp.Subject == null ? Old_Corp.Subject : Curr_Corp.Subject;
                        corp.SubjectDetails = Curr_Corp.SubjectDetails == null ? Old_Corp.SubjectDetails : Curr_Corp.SubjectDetails;
                        //      corp.Id = auth_id;

                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    // db.Configuration.AutoDetectChangesEnabled = false;
                                    corp.DBTrack = new DBTrack
                                    {
                                        CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                                        CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
                                        ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
                                        AuthorizedBy = SessionManager.EmpId,
                                        AuthorizedOn = DateTime.Now,
                                        IsModified = false
                                    };

                                    int a = EditS(Corp, Budget, auth_id, corp, corp.DBTrack);

                                    await db.SaveChangesAsync();

                                    ts.Complete();

                                    Msg.Add("  Record Authorised");
                                    return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (ProgramList)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (ProgramList)databaseEntry.ToObject();
                                    corp.RowVersion = databaseValues.RowVersion;
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
                        ProgramList corp = db.ProgramList.AsNoTracking().Include(e => e.TrainingType)
                                                                    .FirstOrDefault(e => e.Id == auth_id);

                        //Budget add = corp.Budget.ToString();
                        LookupValue val = corp.TrainingType;

                        corp.DBTrack = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                            CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                            CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                            IsModified = false,
                            AuthorizedBy = SessionManager.EmpId,
                            AuthorizedOn = DateTime.Now
                        };

                        db.ProgramList.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/P2b.Training", null, db.ChangeTracker, corp.DBTrack);
                        DT_ProgramList DT_Corp = (DT_ProgramList)rtn_Obj;
                        DT_Corp.TrainingType_Id = corp.TrainingType == null ? 0 : corp.TrainingType.Id;
                        //DT_Corp.Budget_Id = corp.Budget == null ? 0 : corp.Budget.Id;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();
                        db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        Msg.Add("  Record Authorised");
                        return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                IEnumerable<ProgramList> ProgramList = null;
                if (gp.IsAutho == true)
                {
                    ProgramList = db.ProgramList.Include(e => e.TrainingType).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    ProgramList = db.ProgramList.Include(e => e.TrainingType).AsNoTracking().ToList();
                }

                IEnumerable<ProgramList> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = ProgramList;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.Subject, a.SubjectDetails }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Subject.ToLower() == gp.searchString.ToLower()) || (e.SubjectDetails.ToLower() == gp.searchString.ToLower()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Subject, a.SubjectDetails }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = ProgramList;
                    Func<ProgramList, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Subject" ? c.Subject :
                                         gp.sidx == "SubjectDetails" ? c.SubjectDetails : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Subject), Convert.ToString(a.SubjectDetails) }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Subject), Convert.ToString(a.SubjectDetails) }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Subject, a.SubjectDetails }).ToList();
                    }
                    totalRecords = ProgramList.Count();
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
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                ProgramList corporates = db.ProgramList.Include(e => e.TrainingType).Where(e => e.Id == data).SingleOrDefault();

                //Budget add = corporates.Budget;

                LookupValue val = corporates.TrainingType;
                //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                if (corporates.DBTrack.IsModified == true)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "ProgramList");
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
                        DT_ProgramList DT_Corp = (DT_ProgramList)rtn_Obj;
                        //DT_Corp.Budget_Id = corporates.Budget == null ? 0 : corporates.Budget.Id;
                        DT_Corp.TrainingType_Id = corporates.TrainingType == null ? 0 : corporates.TrainingType.Id;

                        db.Create(DT_Corp);
                        // db.SaveChanges();
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                        await db.SaveChangesAsync();
                        //using (var context = new DataBaseContext())
                        //{
                        //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                        //}
                        ts.Complete();
                        Msg.Add("Data removed.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    // var selectedRegions = corporates.Budget;

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //if (selectedRegions != null)
                        //{
                        //    var corpRegion = new HashSet<int>(corporates.Budget.Select(e => e.Id));
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
                                ModifiedBy = SessionManager.EmpId,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = SessionManager.EmpId,
                                //AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.EmpId, ModifiedOn = DateTime.Now };

                            db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, dbT);
                            DT_ProgramList DT_Corp = (DT_ProgramList)rtn_Obj;
                            // DT_Corp.Budget_Id = BudgetPara == null ? 0 : Budget_Id.Id;
                            DT_Corp.TrainingType_Id = val == null ? 0 : val.Id;

                            db.Create(DT_Corp);

                            await db.SaveChangesAsync();

                            ts.Complete();
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet }); 
                            Msg.Add("Data removed.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            //Log the error (uncomment dex variable name and add a line here to write a log.)
                            //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                            //return RedirectToAction("Delete");
                            return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                        }
                    }
                }
            }
        }
        public ActionResult Create(ProgramList c, FormCollection form) //Create submit
        {
            string TrainingType = form["TrainingTypelist"] == "0" ? "" : form["TrainingTypelist"];
            //string Addrs = form["Budgetlist"] == "0" ? "" : form["Budgetlist"];

            using (DataBaseContext db = new DataBaseContext())
            {
                if (TrainingType != null)
                {
                    if (TrainingType != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(TrainingType));
                        c.TrainingType = val;
                    }
                }
                //List<Budget> lookupLang = new List<Budget>();
                //string Lang = form["BudgetlistProg"];
                //if (Lang == null)
                //{
                //    Lang = form["BudgetlistP"];
                //}
                //if (Lang != null)
                //{
                //    var ids = Utility.StringIdsToListIds(Lang);
                //    foreach (var ca in ids)
                //    {
                //        var Lookup_val = db.Budget.Find(ca);

                //        lookupLang.Add(Lookup_val);
                //    }
                //    c.Budget = lookupLang;
                //}
                //else
                //{
                //    // c.Budget = null;
                //}



                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        if (db.ProgramList.Any(o => o.Subject == c.Subject))
                        {
                            Msg.Add("Code Already Exists.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //  return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                        }

                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId, IsModified = false };

                        ProgramList corporate = new ProgramList()
                        {
                            Subject = c.Subject == null ? "" : c.Subject.Trim(),
                            SubjectDetails = c.SubjectDetails == null ? "" : c.SubjectDetails.Trim(),
                            TrainingType = c.TrainingType,
                            //Budget = c.Budget,

                            DBTrack = c.DBTrack
                        };
                        try
                        {
                            db.ProgramList.Add(corporate);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, c.DBTrack);
                            DT_ProgramList DT_Corp = (DT_ProgramList)rtn_Obj;
                            //DT_Corp.Budget_Id= c.Budget == null ? 0 : c.Budget.Id;
                            DT_Corp.TrainingType_Id = c.TrainingType == null ? 0 : c.TrainingType.Id;
                            db.Create(DT_Corp);
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data Created successfully.  ");
                            return Json(new Utility.JsonReturnClass { Id = corporate.Id, Val = corporate.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                    Msg.Add(errorMsg);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { msg = errorMsg });
                }
            }
        }
        public void RollBack()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //  var context = DataContextFactory.GetDataContext();
                var changedEntries = db.ChangeTracker.Entries()
                    .Where(x => x.State != System.Data.Entity.EntityState.Unchanged).ToList();

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Modified))
                {
                    entry.CurrentValues.SetValues(entry.OriginalValues);
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Added))
                {
                    entry.State = System.Data.Entity.EntityState.Detached;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Deleted))
                {
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }

            }
        }
    }

}
