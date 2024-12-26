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
    public class LanguageSkillController : Controller
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
            return View("~/Views/Shared/Core/_LanguageSkill.cshtml");
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
        //        var LKVal = db.LanguageSkill.ToList();

        //        if (gp.IsAutho == true)
        //        {
        //            LKVal = db.LanguageSkill.Include(e => e.SkillType).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            LKVal = db.LanguageSkill.Include(e => e.SkillType).AsNoTracking().ToList();
        //        }


        //        IEnumerable<LanguageSkill> IE;
        //        if (!string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = LKVal;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.SkillType }).Where((e => (e.Id.ToString() == gp.searchString) || (e.SkillType.LookupVal.ToString() == gp.searchString) ));
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.SkillType }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = LKVal;
        //            Func<LanguageSkill, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id: 0);
        //            }

        //            //Func<Lookup, string> orderfuc = (c =>
        //            //                                           gp.sidx == "Id" ? c.Id.ToString() :
        //            //                                           gp.sidx == "Code" ? c.Code :
        //            //                                           gp.sidx == "Name" ? c.Name : "");
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.SkillType }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.SkillType}).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.SkillType }).ToList();
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
                IEnumerable<LanguageSkill> Corporate = null;
                if (gp.IsAutho == true)
                {
                    Corporate = db.LanguageSkill.Include(e => e.SkillType).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Corporate = db.LanguageSkill.Include(e => e.SkillType).AsNoTracking().ToList();
                }

                IEnumerable<LanguageSkill> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = Corporate;
                    if (gp.searchOper.Equals("eq"))
                    {
                        if (gp.searchField == "Id")
                            jsonData = IE.Select(a => new { a.Id}).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                       

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.SkillType != null ? Convert.ToString(a.SkillType.LookupVal) : "" }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Corporate;
                    Func<LanguageSkill, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => 
                                        
                                         gp.sidx == "SkillType" ? c.SkillType.LookupVal : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.SkillType != null ? Convert.ToString(a.SkillType.LookupVal) : "" }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.SkillType != null ? Convert.ToString(a.SkillType.LookupVal) : "" }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.SkillType != null ? Convert.ToString(a.SkillType.LookupVal) : "" }).ToList();
                    }
                    totalRecords = Corporate.Count();
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

        //public ActionResult GetLanguageLKDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.Language.ToList();
        //        IEnumerable<Language> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.Language.ToList().Where(d => d.LanguageName.Contains(data));

        //        }
        //        else
        //        {
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.LanguageName }).Distinct();

        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.LanguageName }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
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

        //public ActionResult GetLanguageLKDetails(List<int> SkipIds)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.Language.ToList();
        //        if (SkipIds != null)
        //        {
        //            foreach (var a in SkipIds)
        //            {
        //                if (fall == null)
        //                    fall = db.Language.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
        //                else
        //                    fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

        //            }
        //        }
        //        var list1 = db.Language.ToList().Select(e => e.LanguageName);
        //      // var list2 = fall.Except(list1);

        //       // var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.LanguageName }).Distinct();

        //        return Json(list1, JsonRequestBehavior.AllowGet);


        //    }
        //    // return View();
        //}


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
        public ActionResult Create(LanguageSkill look, FormCollection form)
        {
            //qual,skill,hobby,lang,award,scolar
            List<Language> lookupval = new List<Language>();
            using (DataBaseContext db = new DataBaseContext())
            {
                string Values = form["Languagelist"];

                if (Values != null)
                {
                    var ids = Utility.StringIdsToListIds(Values);
                    foreach (var ca in ids)
                    {
                        var Lookup_val = db.Language.Find(ca);
                        lookupval.Add(Lookup_val);
                        look.Language = lookupval;
                    }
                }
                else
                {
                    look.Language = null;
                }

                string Category = form["SkillTypelist"] == "0" ? "" : form["SkillTypelist"];

                if (Category != null)
                {
                    if (Category != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "308").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Category)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(Category));
                        look.SkillType = val;
                    }
                }

                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                        LanguageSkill languageSkill = new LanguageSkill()
                        {

                            SkillType = look.SkillType,
                            Language = look.Language,
                            DBTrack = look.DBTrack
                        };
                        try
                        {
                            db.LanguageSkill.Add(languageSkill);
                            var a = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, look.DBTrack);
                            DT_LanguageSkill DT_Corp = (DT_LanguageSkill)a;
                            DT_Corp.SkillType_Id = look.SkillType == null ? 0 : look.SkillType.Id;
                            db.Create(DT_Corp);
                            db.SaveChanges();

                            ts.Complete();
                            Msg.Add("  Data Created successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = languageSkill.Id, Val = languageSkill.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                LanguageSkill corporates = db.LanguageSkill.Include(e => e.SkillType)
                                                   .Include(e => e.Language)
                                                   .Where(e => e.Id == data).SingleOrDefault();

                LookupValue val = corporates.SkillType;
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
                        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack);
                        DT_LanguageSkill DT_Corp = (DT_LanguageSkill)rtn_Obj;
                        //DT_Corp.Language_Id = corporates.Language == null ? 0 : corporates.Language.Id;

                        DT_Corp.SkillType_Id = corporates.SkillType == null ? 0 : corporates.SkillType.Id;
                        db.Create(DT_Corp);
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
                    var selectedRegions = corporates.Language;

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        if (selectedRegions != null)
                        {
                            var corpRegion = new HashSet<int>(corporates.Language.Select(e => e.Id));
                            if (corpRegion.Count > 0)
                            {
                                Msg.Add(" Child record exists.Cannot remove it. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }

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
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            DT_LanguageSkill DT_Corp = (DT_LanguageSkill)rtn_Obj;
                            //DT_Corp.Language_Id = add == null ? 0 : add.Id;
                            DT_Corp.SkillType_Id = val == null ? 0 : val.Id;

                            db.Create(DT_Corp);

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
                var un = db.LanguageSkill.Include(e => e.Language).Include(e => e.SkillType).Where(e => e.Id == data).Select
                    (e => new
                    {
                        SkillType_Id = e.SkillType.Id == null ? 0 : e.SkillType.Id,
                        Action = e.DBTrack.Action
                    }).ToList();


                List<LangDetails> contno = new List<LangDetails>();
                var k = db.LanguageSkill.Include(e => e.Language).Where(e => e.Id == data).Select(e => e.Language).ToList();
                foreach (var val in k)
                {
                    contno.Add(new LangDetails
                    {
                        Language_Id = val.Select(e => e.Id.ToString()).ToArray(),
                        Language_FullDetails = val.Select(e => e.LanguageName).ToArray()

                    });
                }
                //return Json(new object[] { contno }, JsonRequestBehavior.AllowGet);
                //TempData["RowVersion"] = db.IncrActivity.Find(data).RowVersion;

                //return Json(new Object[] { Q, add_data, JsonRequestBehavior.AllowGet });

                var W = db.DT_LanguageSkill
                    //.Include(e => e.IncrPolicy)
                    //.Include(e => e.IncrList)
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         SkillType_Val = e.SkillType_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.SkillType_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),
                         Language_val = e.Language_Id == 0 ? "" : db.Language.Where(x => x.Id == e.Language_Id).Select(x => x.LanguageName).FirstOrDefault(),
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.LanguageSkill.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { un, contno, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        
     

        [HttpPost]
        public async Task<ActionResult> EditSave(LanguageSkill c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var db_data = db.LanguageSkill.Include(e => e.Language).Include(e => e.SkillType)
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


                    string Category = form["SkillTypelist"] == "0" ? "" : form["SkillTypelist"];

                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Category));
                            c.SkillType = val;
                        }
                    }

                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Category));
                            c.SkillType = val;

                            var type = db.LanguageSkill
                                .Include(e => e.SkillType)
                                .Where(e => e.Id == data).SingleOrDefault();
                            IList<LanguageSkill> typedetails = null;
                            if (type.SkillType != null)
                            {
                                typedetails = db.LanguageSkill.Where(x => x.SkillType.Id == type.SkillType.Id && x.Id == data).ToList();
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
                            var Dtls = db.LanguageSkill.Include(e => e.SkillType).Where(x => x.Id == data).ToList();
                            foreach (var s in Dtls)
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
                                            blog = context.LanguageSkill.Where(e => e.Id == data).Include(e => e.Language).Include(e => e.SkillType)
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
                                            Language = c.Language,
                                            SkillType = c.SkillType,
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

                                        var fall = db.LanguageSkill.Include(e => e.Language).Include(e => e.SkillType).Where(e => e.Id == lk.Id).SingleOrDefault();
                                        ts.Complete();
                                       
                                            Msg.Add("  Record Updated");
                                            return Json(new Utility.JsonReturnClass { Id = lk.Id, Val = fall.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                      
                                        
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

                            LanguageSkill blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            LanguageSkill Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.LanguageSkill.Include(e => e.Language).Include(e => e.SkillType).Where(e => e.Id == data).SingleOrDefault();
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
                            LanguageSkill qualificationDetails = new LanguageSkill()
                            {

                                Id = data,
                                Language = c.Language,
                                SkillType = c.SkillType,
                                DBTrack = c.DBTrack
                            };

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, qualificationDetails, "QualificationDetails", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.LanguageSkill.Where(e => e.Id == data)
                                    .Include(e => e.Language).Include(e => e.SkillType).SingleOrDefault();
                                DT_LanguageSkill DT_Corp = (DT_LanguageSkill)obj;
                                //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.LanguageSkill.Attach(blog);
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


         
	}
}