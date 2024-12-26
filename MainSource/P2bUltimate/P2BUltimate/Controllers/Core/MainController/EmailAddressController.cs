///
/// Created by Sarika
///

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
using System.Text;
using System.Threading.Tasks;
using P2BUltimate.Security;


namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class EmailAddressController : Controller
    {
        //
        // GET: /LanguageSkill/
        List<string> Msg = new List<string>();
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/LanguageSkill/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Core/_EmailAddress.cshtml");
        }

        public ActionResult GetAuthorityLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "809").ToList().SelectMany(e => e.LookupValues).Take(5).OrderBy(e => e.Id);

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Lookup.Include(e => e.LookupValues).Where(e => !e.Id.ToString().Contains(a.ToString()) && e.Code =="809").ToList().SelectMany(e => e.LookupValues).Take(5).OrderBy(e => e.Id);
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList().Take(5).OrderBy(e => e.Id);
                    }
                }


                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
            // return View();
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
        //        var jsonData = (Object)null;
        //        IEnumerable<LanguageSkill> Corporate = null;
        //        if (gp.IsAutho == true)
        //        {
        //            Corporate = db.LanguageSkill.Include(e => e.SkillType).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            Corporate = db.LanguageSkill.Include(e => e.SkillType).AsNoTracking().ToList();
        //        }

        //        IEnumerable<LanguageSkill> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = Corporate;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "Id")
        //                    jsonData = IE.Select(a => new { a.Id}).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();


        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.SkillType != null ? Convert.ToString(a.SkillType.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = Corporate;
        //            Func<LanguageSkill, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => 

        //                                 gp.sidx == "SkillType" ? c.SkillType.LookupVal : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.SkillType != null ? Convert.ToString(a.SkillType.LookupVal) : "" }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.SkillType != null ? Convert.ToString(a.SkillType.LookupVal) : "" }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.SkillType != null ? Convert.ToString(a.SkillType.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = Corporate.Count();
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
        //            total = totalPages
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}


        public ActionResult GetLanguageLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Language.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Language.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.LanguageName }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }




        private MultiSelectList GetLookupValues(List<int> selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Language> lkval = new List<Language>();
                lkval = db.Language.ToList();
                return new MultiSelectList(lkval, "Id", "LanguageName", selectedValues);
            }

        }
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(EmailAddress look, FormCollection form)
        {



            using (DataBaseContext db = new DataBaseContext())
            {

                string Category = form["EmailAddressTypelist"] == "0" ? "" : form["EmailAddressTypelist"];

                if (Category != null)
                {
                    if (Category != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "801").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Category)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailAttachmentTypelist));
                        look.EmailAddressType = val;
                    }
                }

                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                        EmailAddress emailaddress = new EmailAddress()
                        {

                            EmailAddressType = look.EmailAddressType,
                            Address = look.Address,
                            DBTrack = look.DBTrack
                        };
                        try
                        {
                            db.EmailAddress.Add(emailaddress);
                            //var a = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, look.DBTrack);
                            //DT_LanguageSkill DT_Corp = (DT_LanguageSkill)a;
                            //DT_Corp.SkillType_Id = look.SkillType == null ? 0 : look.SkillType.Id;
                            //db.Create(DT_Corp);
                            db.SaveChanges();

                            ts.Complete();
                            Msg.Add("  Data Created successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = emailaddress.Id, Val = emailaddress.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = look.Id });
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

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                EmailAddress corporates = db.EmailAddress.Include(e => e.EmailAddressType)
                                                   .Where(e => e.Id == data).SingleOrDefault();

                LookupValue val = corporates.EmailAddressType;
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
                        //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack);
                        //DT_LanguageSkill DT_Corp = (DT_LanguageSkill)rtn_Obj;
                        //DT_Corp.Language_Id = corporates.Language == null ? 0 : corporates.Language.Id;

                        //DT_Corp.SkillType_Id = corporates.SkillType == null ? 0 : corporates.SkillType.Id;
                        //db.Create(DT_Corp);
                        // db.SaveChanges();
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                        await db.SaveChangesAsync();
                        //using (var context = new DataBaseContext())
                        //{
                        //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                        //}
                        ts.Complete();
                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        try
                        {
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
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            //DT_LanguageSkill DT_Corp = (DT_LanguageSkill)rtn_Obj;
                            ////DT_Corp.Language_Id = add == null ? 0 : add.Id;
                            //DT_Corp.SkillType_Id = val == null ? 0 : val.Id;

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
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
        }

        public class LangDetails
        {
            public Array Language_Id { get; set; }
            public Array Language_FullDetails { get; set; }

        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var un = db.EmailAddress.Include(e => e.EmailAddressType).Where(e => e.Id == data).Select
                    (e => new
                    {
                        EmailAddressType_Id = e.EmailAddressType.Id == null ? 0 : e.EmailAddressType.Id,
                        Address = e.Address,
                        Action = e.DBTrack.Action
                    }).ToList();


                var Corp = db.EmailAddress.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { un, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        public async Task<ActionResult> EditSave1(LanguageSkill c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var db_data = db.LanguageSkill.Include(e => e.Language)
                            .Include(e => e.SkillType)
                            .Where(e => e.Id == data).SingleOrDefault();

                bool Auth = form["Autho_Allow"] == "true" ? true : false;

                List<Language> lookupval = new List<Language>();
                string Values = form["Languagelist"];


                if (Values != null)
                {
                    var ids = Utility.StringIdsToListIds(Values);
                    foreach (var ca in ids)
                    {
                        var Lookup_val = db.Language.Find(ca);
                        lookupval.Add(Lookup_val);
                        db_data.Language = lookupval;
                    }
                }
                else
                {
                    db_data.Language = null;
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
                                db.LanguageSkill.Attach(db_data);
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = db_data.RowVersion;
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                var Curr_Lookup = db.LanguageSkill.Find(data);
                                TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {

                                    LanguageSkill blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.LanguageSkill.Where(e => e.Id == data).Include(e => e.Language)
                                .Include(e => e.SkillType)
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
                                    LanguageSkill lk = new LanguageSkill
                                    {
                                        Id = data,
                                        Language = db_data.Language,
                                        SkillType = db_data.SkillType,
                                        DBTrack = c.DBTrack
                                    };


                                    db.LanguageSkill.Attach(lk);
                                    db.Entry(lk).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

                                    // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


                                    using (var context = new DataBaseContext())
                                    {

                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_LanguageSkill DT_Corp = (DT_LanguageSkill)obj;

                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();


                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = lk.Id, Val = lk.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (LanguageSkill)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var databaseValues = (LanguageSkill)databaseEntry.ToObject();
                                c.RowVersion = databaseValues.RowVersion;
                            }
                        }
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        LanguageSkill blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        LanguageSkill Old_Corp = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.LanguageSkill.Where(e => e.Id == data).SingleOrDefault();
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
                        LanguageSkill empAcademicInfo = new LanguageSkill()
                        {

                            Id = data,
                            SkillType = c.SkillType,
                            DBTrack = c.DBTrack
                        };

                        db.Entry(blog).State = System.Data.Entity.EntityState.Detached;
                        empAcademicInfo.DBTrack = c.DBTrack;
                        // db.EmpAcademicInfo.Attach(empAcademicInfo);                   
                        //db.Entry(empAcademicInfo).State = System.Data.Entity.EntityState.Modified;
                        //db.Entry(empAcademicInfo).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        ts.Complete();
                        Msg.Add("  Record Updated");
                        return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = "", success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                return View();
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(EmailAddress c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var db_data = db.EmailAddress.Include(e => e.EmailAddressType)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    string Category = form["EmailAddressTypelist"] == "0" ? "" : form["EmailAddressTypelist"];

                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "801").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Category)).FirstOrDefault(); 
                            c.EmailAddressType = val;
                        }
                    }

                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "801").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Category)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(Category));
                            c.EmailAddressType = val;

                            var type = db.EmailAddress
                                .Include(e => e.EmailAddressType)
                                .Where(e => e.Id == data).SingleOrDefault();
                            IList<EmailAddress> typedetails = null;
                            if (type.EmailAddressType != null)
                            {
                                typedetails = db.EmailAddress.Where(x => x.EmailAddressType.Id == type.EmailAddressType.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.EmailAddress.Where(x => x.Id == data).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.EmailAddressType = c.EmailAddressType;
                                db.EmailAddress.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        else
                        {
                            var Dtls = db.EmailAddress.Include(e => e.EmailAddressType).Where(x => x.Id == data).ToList();
                            foreach (var s in Dtls)
                            {
                                s.EmailAddressType = null;
                                db.EmailAddress.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
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
                                    db.EmailAddress.Attach(db_data);
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_data.RowVersion;
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_Lookup = db.EmailAddress.Find(data);
                                    TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                    db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        EmailAddress blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.EmailAddress.Where(e => e.Id == data).Include(e => e.EmailAddressType)
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
                                        EmailAddress lk = new EmailAddress
                                        {
                                            Id = data,
                                            Address = c.Address,
                                            EmailAddressType = c.EmailAddressType,
                                            DBTrack = c.DBTrack
                                        };


                                        db.EmailAddress.Attach(lk);
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                        db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

                                        // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


                                        //using (var context = new DataBaseContext())
                                        //{

                                        //var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        //DT_LanguageSkill DT_Corp = (DT_LanguageSkill)obj;

                                        //db.Create(DT_Corp);
                                        db.SaveChanges();
                                        //}
                                        await db.SaveChangesAsync();

                                        var fall = db.EmailAddress.Include(e => e.EmailAddressType).Where(e => e.Id == lk.Id).SingleOrDefault();
                                        ts.Complete();

                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = lk.Id, Val = fall.Address, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                                        //return Json(new Object[] { lk.Id, "", "Record Updated", JsonRequestBehavior.AllowGet });
                                    }
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (LanguageSkill)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (LanguageSkill)databaseEntry.ToObject();
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

                            EmailAddress blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            EmailAddress Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.EmailAddress.Include(e => e.EmailAddressType).Where(e => e.Id == data).SingleOrDefault();
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
                            EmailAddress qualificationDetails = new EmailAddress()
                            {

                                Id = data,
                                Address = c.Address,
                                EmailAddressType = c.EmailAddressType,
                                DBTrack = c.DBTrack
                            };

                            //using (var context = new DataBaseContext())
                            //{
                            //    var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, qualificationDetails, "QualificationDetails", c.DBTrack);
                            //    // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                            //    Old_Corp = context.EmailAddress.Where(e => e.Id == data)
                            //        .Include(e => e.EmailAddressType).SingleOrDefault();
                            //    //DT_LanguageSkill DT_Corp = (DT_LanguageSkill)obj;
                            //    //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                            //    //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                            //    //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                            //   // db.Create(DT_Corp);
                            //    //db.SaveChanges();
                            //}
                            blog.DBTrack = c.DBTrack;
                            db.EmailAddress.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();

                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.Address, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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


        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save Authorized data
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (auth_action == "C")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //Corporate corp = db.Corporate.Find(auth_id);
                        //Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                        LanguageSkill corp = db.LanguageSkill
                            .Include(e => e.Language)
                            .Include(e => e.SkillType)
                            .FirstOrDefault(e => e.Id == auth_id);

                        corp.DBTrack = new DBTrack
                        {
                            Action = "C",
                            ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                            CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                            CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                            IsModified = corp.DBTrack.IsModified == true ? false : false,
                            AuthorizedBy = SessionManager.UserName,
                            AuthorizedOn = DateTime.Now
                        };

                        db.LanguageSkill.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //db.SaveChanges();
                        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                        DT_LanguageSkill DT_Corp = (DT_LanguageSkill)rtn_Obj;
                        DT_Corp.SkillType_Id = corp.SkillType == null ? 0 : corp.SkillType.Id;
                        //DT_Corp.Hobby_Id = corp.Hobby == null ? 0 : corp.Hobby;
                        //DT_Corp.LanguageSkill_Id = corp.LanguageSkill == null ? 0 : corp.LanguageSkill;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();

                        ts.Complete();
                        Msg.Add("  Record Authorised");
                        return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (auth_action == "M")
                {

                    LanguageSkill Old_Corp = db.LanguageSkill
                            .Include(e => e.Language)
                            .Include(e => e.SkillType)
                           .Where(e => e.Id == auth_id).SingleOrDefault();


                    DT_LanguageSkill Curr_Corp = db.DT_LanguageSkill
                                                .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                .OrderByDescending(e => e.Id)
                                                .FirstOrDefault();

                    if (Curr_Corp != null)
                    {
                        LanguageSkill corp = new LanguageSkill();

                        string awrd = Curr_Corp.SkillType_Id == null ? null : Curr_Corp.SkillType_Id.ToString();
                        //string hob = Curr_Corp.Hobby_Id == null ? null : Curr_Corp.Hobby_Id.ToString();
                        //string langskl = Curr_Corp.LanguageSkill_Id == null ? null : Curr_Corp.LanguageSkill_Id.ToString();
                        //string Qualdtl = Curr_Corp.QualificationDetails_Id == null ? null : Curr_Corp.QualificationDetails_Id.ToString();
                        //string skll = Curr_Corp.Skill_Id == null ? null : Curr_Corp.Skill_Id.ToString();
                        //string scolr = Curr_Corp.Scolarship_Id == null ? null : Curr_Corp.Scolarship_Id.ToString();

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
                                        AuthorizedBy = SessionManager.UserName,
                                        AuthorizedOn = DateTime.Now,
                                        IsModified = false
                                    };

                                    //int a = EditS(awrd, hob, langskl, Qualdtl, skll,  scolr,auth_id, corp, corp.DBTrack);

                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    Msg.Add("  Record Authorised");
                                    return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (LanguageSkill)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    var databaseValues = (LanguageSkill)databaseEntry.ToObject();
                                    corp.RowVersion = databaseValues.RowVersion;
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
                        //Corporate corp = db.Corporate.Find(auth_id);
                        LanguageSkill corp = db.LanguageSkill.AsNoTracking()
                           .Include(e => e.Language)
                            .Include(e => e.SkillType)
                            .FirstOrDefault(e => e.Id == auth_id);

                        //Awards add = corp.Awards.ToString();
                        //Hobby conDet = corp.Hobby;
                        //LanguageSkill val = corp.LanguageSkill;

                        corp.DBTrack = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                            CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                            CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                            IsModified = false,
                            AuthorizedBy = SessionManager.UserName,
                            AuthorizedOn = DateTime.Now
                        };

                        db.LanguageSkill.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                        DT_LanguageSkill DT_Corp = (DT_LanguageSkill)rtn_Obj;
                        DT_Corp.SkillType_Id = corp.SkillType == null ? 0 : corp.SkillType.Id;
                        //DT_Corp.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                        //DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();
                        db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        Msg.Add(" Record Authorised ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                }
                return View();
            }
        }


        public int EditS(string awrd, int data, LanguageSkill c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (awrd != null)
                {
                    if (awrd != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(awrd));
                        // var val = db.Awards.Where(e => e.Id == data).SingleOrDefault();

                        var r = (from ca in db.LookupValue
                                 select new
                                 {
                                     Id = ca.Id,
                                     LookupVal = ca.LookupVal
                                 }).Where(e => e.Id == data).Distinct();

                        var type = db.LanguageSkill.Include(e => e.SkillType).Where(e => e.Id == data).SingleOrDefault();
                        IList<LanguageSkill> typedetails = null;
                        if (type.SkillType != null)
                        {
                            typedetails = db.LanguageSkill.Where(x => x.SkillType == type.SkillType && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.LanguageSkill.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.SkillType = c.SkillType;
                            db.LanguageSkill.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.LanguageSkill.Include(e => e.SkillType).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.SkillType = null;
                            db.LanguageSkill.Attach(s);
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
                    var BusiTypeDetails = db.LanguageSkill.Include(e => e.SkillType).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.SkillType = null;
                        db.LanguageSkill.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }




                var CurCorp = db.LanguageSkill.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    LanguageSkill corp = new LanguageSkill()
                    {
                        Id = data,
                        SkillType = c.SkillType,
                        DBTrack = c.DBTrack
                    };


                    db.LanguageSkill.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }
        public void RollBack()
        {

            //  var context = DataContextFactory.GetDataContext();
            using (DataBaseContext db = new DataBaseContext())
            {
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