
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
using P2BUltimate.Security;
using Recruitement;
namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class CanSocialInfoController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /EmpSocialInfo/

        public ActionResult Index()
        {
            return View("~/Views/Recruitement/MainView/CanSocialInfo/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Core/_SocialActivities.cshtml");
        }

        private MultiSelectList GetLookupValues(List<int> selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<SocialActivities> lkval = new List<SocialActivities>();
                lkval = db.SocialActivities.ToList();
                return new MultiSelectList(lkval, "Id", "PostHeld", selectedValues);
            }
        }

        //public String checkCode(String data)
        //{
        //    var qurey = db.EmpSocialInfo.Any(e => e.SocialActivities == data);
        //    if (qurey == true)
        //    {
        //        return "1";
        //    }
        //    else
        //    {
        //        return "0";
        //    }
        //}

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(EmpSocialInfo COBJ, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string Category = form["Categorylist"] == "0" ? "" : form["Categorylist"];
                    string Caste = form["Castelist"] == "0" ? "" : form["Castelist"];
                    string Religion = form["Religionlist"] == "0" ? "" : form["Religionlist"];
                    string SubCaste = form["SubCastelist"] == "0" ? "" : form["SubCastelist"];
                    int Emp = form["Candidate_table"] == "0" ? 0 : Convert.ToInt32(form["Candidate_table"]);
                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Category));
                            COBJ.Category = val;
                        }
                    }

                    if (Caste != null)
                    {
                        if (Caste != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Caste));
                            COBJ.Caste = val;
                        }
                    }
                    if (Religion != null)
                    {
                        if (Religion != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Religion));
                            COBJ.Religion = val;
                        }
                    }
                    if (SubCaste != null)
                    {
                        if (SubCaste != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(SubCaste));
                            COBJ.SubCaste = val;
                        }
                    }


                    COBJ.SocialActivities = null;
                    List<SocialActivities> OBJ = new List<SocialActivities>();
                    string Values = form["SocialActivitieslist"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.SocialActivities.Find(ca);
                            OBJ.Add(OBJ_val);
                            COBJ.SocialActivities = OBJ;
                        }
                    }

                    Candidate EmpData;
                    if (Emp != null && Emp != 0)
                    {
                        EmpData = db.Candidate.Find(Emp);
                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }


                    if (ModelState.IsValid)
                    {

                        using (TransactionScope ts = new TransactionScope())
                        {
                            //if (db.EmpSocialInfo.Any(o => o.Category == COBJ.Category))
                            //{                       
                            //    return this.Json(new Object[] { null, null, "Category already exists.", JsonRequestBehavior.AllowGet });
                            //}

                            COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            EmpSocialInfo EmpSocialInfo = new EmpSocialInfo()
                            {
                                Category = COBJ.Category,
                                Caste = COBJ.Caste,
                                SubCaste = COBJ.SubCaste,
                                Religion = COBJ.Religion,
                                SocialActivities = COBJ.SocialActivities,
                                DBTrack = COBJ.DBTrack
                            };
                            try
                            {



                                db.EmpSocialInfo.Add(EmpSocialInfo);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, COBJ.DBTrack);
                                DT_EmpSocialInfo DT_OBJ = (DT_EmpSocialInfo)rtn_Obj;
                                DT_OBJ.Religion_Id = COBJ.Religion == null ? 0 : COBJ.Religion.Id;
                                DT_OBJ.Caste_Id = COBJ.Caste == null ? 0 : COBJ.Caste.Id;
                                DT_OBJ.SubCaste_Id = COBJ.SubCaste == null ? 0 : COBJ.SubCaste.Id;
                                DT_OBJ.Category_Id = COBJ.Category == null ? 0 : COBJ.Category.Id;
                                db.Create(DT_OBJ);
                                db.SaveChanges();

                                if (EmpData != null)
                                {
                                    EmpData.CanSocialInfo = EmpSocialInfo;
                                    db.Candidate.Attach(EmpData);
                                    db.Entry(EmpData).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(EmpData).State = System.Data.Entity.EntityState.Detached;
                                }

                                ts.Complete();

                                Msg.Add("  Data Created successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new Object[] { null, null, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
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

                        // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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


        public class EmpSocialInfo_SA
        {
            public Array SA_id { get; set; }
            public Array SA_val { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<EmpSocialInfo_SA> return_data = new List<EmpSocialInfo_SA>();
                var EmpSocialInfo = db.Candidate.Include(e => e.CanSocialInfo).Include(e => e.CanSocialInfo.SocialActivities)
                                   .Include(e => e.CanSocialInfo.Category)
                                   .Include(e => e.CanSocialInfo.Religion)
                                   .Include(e => e.CanSocialInfo.Caste)
                                   .Include(e => e.CanSocialInfo.SubCaste)
                                   .Where(e => e.Id == data).ToList();

                var r = (from ca in EmpSocialInfo
                         select new
                         {
                             Id = ca.Id,
                             Religion = ca.CanSocialInfo.Religion.Id,
                             Category = ca.CanSocialInfo.Category.Id,
                             Caste = ca.CanSocialInfo.Caste.Id,
                             SubCaste = ca.CanSocialInfo.SubCaste.Id,
                             Action = ca.DBTrack.Action
                         }).Distinct();


                var a = db.Candidate.Include(e => e.CanSocialInfo).Include(e => e.CanSocialInfo.SocialActivities).Where(e => e.Id == data).Select(e => e.CanSocialInfo.SocialActivities).ToList();

                foreach (var ca in a)
                {
                    return_data.Add(
                new EmpSocialInfo_SA
                {
                    SA_id = ca.Select(e => e.Id.ToString()).ToArray(),
                    SA_val = ca.Select(e => e.FullDetails).ToArray()
                });
                }

                //var W = db.DT_CanSocialInfo
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,
                //         Category = e.Category_Id == 0 ? "" : db.LookupValue
                //                    .Where(x => x.Id == e.Category_Id)
                //                    .Select(x => x.LookupVal).FirstOrDefault(),
                //         Religion = e.Religion_Id == 0 ? "" : db.LookupValue
                //                    .Where(x => x.Id == e.Religion_Id)
                //                    .Select(x => x.LookupVal).FirstOrDefault(),
                //         Caste = e.Caste_Id == 0 ? "" : db.LookupValue
                //                     .Where(x => x.Id == e.Caste_Id)
                //                     .Select(x => x.LookupVal).FirstOrDefault(),
                //         SubCaste = e.SubCaste_Id == 0 ? "" : db.LookupValue
                //                    .Where(x => x.Id == e.SubCaste_Id)
                //                    .Select(x => x.LookupVal).FirstOrDefault(),
                //         SA_Val = e.SocialActivities_Id == 0 ? "" : db.SocialActivities.Where(x => x.Id == e.SocialActivities_Id).Select(x => x.FullDetails).FirstOrDefault(),

                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var LKup = db.Candidate.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, return_data, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }



        public ActionResult EditSocialActivities_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var val = db.SocialActivities.Where(e => e.Id == data).SingleOrDefault();

                var r = (from ca in db.SocialActivities
                         select new
                         {
                             Id = ca.Id,
                             FromPeriod = ca.FromPeriod,
                             ToPeriod = ca.ToPeriod,
                             PostHeld = ca.PostHeld,
                             InstituteName = ca.InstituteName,
                         }).Where(e => e.Id == data).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> EditSave1(EmpSocialInfo ESOBJ, FormCollection form, int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Values = form["SocialActivitieslist"];
                    bool Auth = form["autho_allow"] == "true" ? true : false;


                    var db_data = db.EmpSocialInfo.Include(e => e.SocialActivities).Where(e => e.Id == data).SingleOrDefault();
                    List<SocialActivities> OBJ = new List<SocialActivities>();
                    db_data.SocialActivities = null;
                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var lookup_val = db.SocialActivities.Find(ca);
                            OBJ.Add(lookup_val);
                            db_data.SocialActivities = OBJ;
                        }
                    }

                    string Category = form["Categorylist"] == "0" ? "" : form["Categorylist"];
                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Category));
                            ESOBJ.Category = val;
                        }
                    }
                    string Religion = form["Religionlist"] == "0" ? "" : form["Religionlist"];
                    if (Religion != null)
                    {
                        if (Religion != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Religion));
                            ESOBJ.Religion = val;
                        }
                    }
                    string Caste = form["Castelist"] == "0" ? "" : form["Castelist"];
                    if (Caste != null)
                    {
                        if (Caste != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Caste));
                            ESOBJ.Caste = val;
                        }
                    }
                    string SubCaste = form["SubCastelist"] == "0" ? "" : form["SubCastelist"];
                    if (SubCaste != null)
                    {
                        if (SubCaste != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(SubCaste));
                            ESOBJ.SubCaste = val;
                        }
                    }





                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    EmpSocialInfo blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.EmpSocialInfo.Where(e => e.Id == data)
                                                                .Include(e => e.SocialActivities)
                                                                .Include(e => e.Category)
                                                                .Include(e => e.Religion)
                                                                .Include(e => e.Caste)
                                                                .Include(e => e.SubCaste)
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

                                    int a = EditS(Religion, Category, Caste, SubCaste, Values, data, ESOBJ, ESOBJ.DBTrack);



                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                        DT_EmpSocialInfo DT_OBJ = (DT_EmpSocialInfo)obj;
                                        DT_OBJ.Caste_Id = blog.Caste == null ? 0 : blog.Caste.Id;
                                        DT_OBJ.Category_Id = blog.Category == null ? 0 : blog.Category.Id;
                                        DT_OBJ.Religion_Id = blog.Religion == null ? 0 : blog.Religion.Id;
                                        DT_OBJ.SubCaste_Id = blog.SubCaste == null ? 0 : blog.SubCaste.Id;
                                        db.Create(DT_OBJ);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.Religion.LookupVal, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //  return Json(new Object[] { ESOBJ.Id, ESOBJ.Religion.LookupVal, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }

                            //catch (DbUpdateException e) { throw e; }
                            //catch (DataException e) { throw e; }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (EmpSocialInfo)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {

                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (EmpSocialInfo)databaseEntry.ToObject();
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

                            //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            EmpSocialInfo blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            //EmpSocialInfo Old_LKup = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.EmpSocialInfo.Where(e => e.Id == data).SingleOrDefault();
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
                            EmpSocialInfo EmpSocialInfo = new EmpSocialInfo()
                            {
                                //Category = ESOBJ.Category,
                                //Religion = ESOBJ.Religion,
                                //Caste = ESOBJ.Caste,
                                //SubCaste = ESOBJ.SubCaste,
                                SocialActivities = db_data.SocialActivities,
                                Id = data,
                                DBTrack = ESOBJ.DBTrack
                            };


                            db.Entry(blog).State = System.Data.Entity.EntityState.Detached;
                            EmpSocialInfo.DBTrack = ESOBJ.DBTrack;
                            db.EmpSocialInfo.Attach(EmpSocialInfo);
                            db.Entry(EmpSocialInfo).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(EmpSocialInfo).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = ESOBJ.Religion.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { blog.Id, ESOBJ.Religion, "Record Updated", JsonRequestBehavior.AllowGet });
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
        public async Task<ActionResult> EditSave(EmpSocialInfo ESOBJ, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Values = form["SocialActivitieslist"];
                    bool Auth = form["autho_allow"] == "true" ? true : false;

                    var db_data = db.Candidate.Include(e => e.CanSocialInfo).Include(e => e.CanSocialInfo.SocialActivities)
                        .Include(e => e.CanSocialInfo.Category)
                        .Include(e => e.CanSocialInfo.Religion)
                        .Include(e => e.CanSocialInfo.Caste)
                        .Include(e => e.CanSocialInfo.SubCaste)
                        .Where(e => e.Id == data).SingleOrDefault();
                    int Eid = db_data.CanSocialInfo.Id;
                    // var db_data = db.EmpSocialInfo.Include(e => e.SocialActivities).Where(e => e.Id == data).SingleOrDefault();
                    List<SocialActivities> SOBJ = new List<SocialActivities>();
                    db_data.CanSocialInfo.SocialActivities = null;
                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var lookup_val = db.SocialActivities.Find(ca);
                            SOBJ.Add(lookup_val);
                            db_data.CanSocialInfo.SocialActivities = SOBJ;
                        }
                    }
                    var socialact = db_data.CanSocialInfo.SocialActivities;
                    string Category = form["Categorylist"] == "0" ? "" : form["Categorylist"];
                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Category));
                            ESOBJ.Category = val;
                        }
                    }
                    string Religion = form["Religionlist"] == "0" ? "" : form["Religionlist"];
                    if (Religion != null)
                    {
                        if (Religion != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Religion));
                            ESOBJ.Religion = val;
                        }
                    }
                    string Caste = form["Castelist"] == "0" ? "" : form["Castelist"];
                    if (Caste != null)
                    {
                        if (Caste != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Caste));
                            ESOBJ.Caste = val;
                        }
                    }
                    string SubCaste = form["SubCastelist"] == "0" ? "" : form["SubCastelist"];
                    if (SubCaste != null)
                    {
                        if (SubCaste != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(SubCaste));
                            ESOBJ.SubCaste = val;
                        }
                    }





                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();
                                db.EmpSocialInfo.Attach(db_data.CanSocialInfo);
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = db_data.RowVersion;
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                var Curr_OBJ = db.EmpSocialInfo.Find(Eid);
                                TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

                                //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                //{
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    EmpSocialInfo blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.EmpSocialInfo.Where(e => e.Id == Eid)
                                                                 .Include(e => e.SocialActivities)
                                                                 .Include(e => e.Category)
                                                                 .Include(e => e.Religion)
                                                                 .Include(e => e.Caste)
                                                                 .Include(e => e.SubCaste)
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
                                   // int a = EditS(Religion, Category, Caste, SubCaste, Values, Eid, ESOBJ, ESOBJ.DBTrack);


                                    if (Religion != null)
                                    {
                                        if (Religion != "")
                                        {
                                            var val = db.LookupValue.Find(int.Parse(Religion));
                                            ESOBJ.Religion = val;

                                            var type = db.EmpSocialInfo.Include(e => e.Religion)
                                                .Where(e => e.Id == Eid).SingleOrDefault();
                                            IList<EmpSocialInfo> typedetails = null;
                                            if (type.Religion != null)
                                            {
                                                typedetails = db.EmpSocialInfo.Where(x => x.Religion.Id == type.Religion.Id && x.Id == Eid).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.EmpSocialInfo.Where(x => x.Id == Eid).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.Religion = ESOBJ.Religion;
                                                db.EmpSocialInfo.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var Dtls = db.EmpSocialInfo.Include(e => e.Religion).Where(x => x.Id == Eid).ToList();
                                            foreach (var s in Dtls)
                                            {
                                                s.Religion = null;
                                                db.EmpSocialInfo.Attach(s);
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
                                        var Dtls = db.EmpSocialInfo.Include(e => e.Religion).Where(x => x.Id == Eid).ToList();
                                        foreach (var s in Dtls)
                                        {
                                            s.Religion = null;
                                            db.EmpSocialInfo.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }



                                    if (Category != null)
                                    {
                                        if (Category != "")
                                        {
                                            var val = db.LookupValue.Find(int.Parse(Category));
                                            ESOBJ.Religion = val;

                                            var type = db.EmpSocialInfo
                                                .Include(e => e.Category)
                                                .Where(e => e.Id == Eid).SingleOrDefault();
                                            IList<EmpSocialInfo> typedetails = null;
                                            if (type.Category != null)
                                            {
                                                typedetails = db.EmpSocialInfo.Where(x => x.Category.Id == type.Category.Id && x.Id == Eid).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.EmpSocialInfo.Where(x => x.Id == Eid).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.Category = ESOBJ.Religion;
                                                db.EmpSocialInfo.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var Dtls = db.EmpSocialInfo.Include(e => e.Category).Where(x => x.Id == Eid).ToList();
                                            foreach (var s in Dtls)
                                            {
                                                s.Category = null;
                                                db.EmpSocialInfo.Attach(s);
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
                                        var Dtls = db.EmpSocialInfo.Include(e => e.Religion).Where(x => x.Id == Eid).ToList();
                                        foreach (var s in Dtls)
                                        {
                                            s.Religion = null;
                                            db.EmpSocialInfo.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }


                                    if (Caste != null)
                                    {
                                        if (Caste != "")
                                        {
                                            var val = db.LookupValue.Find(int.Parse(Caste));
                                            ESOBJ.Caste = val;
                                            var type = db.EmpSocialInfo
                                                .Include(e => e.Caste)
                                                .Where(e => e.Id == Eid).SingleOrDefault();
                                            IList<EmpSocialInfo> typedetails = null;
                                            if (type.Caste != null)
                                            {
                                                typedetails = db.EmpSocialInfo.Where(x => x.Caste.Id == type.Caste.Id && x.Id == Eid).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.EmpSocialInfo.Where(x => x.Id == Eid).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.Caste = ESOBJ.Caste;
                                                db.EmpSocialInfo.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var Dtls = db.EmpSocialInfo.Include(e => e.Caste).Where(x => x.Id == Eid).ToList();
                                            foreach (var s in Dtls)
                                            {
                                                s.Caste = null;
                                                db.EmpSocialInfo.Attach(s);
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
                                        var Dtls = db.EmpSocialInfo.Include(e => e.Caste).Where(x => x.Id == Eid).ToList();
                                        foreach (var s in Dtls)
                                        {
                                            s.Caste = null;
                                            db.EmpSocialInfo.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }


                                    if (SubCaste != null)
                                    {
                                        if (SubCaste != "")
                                        {
                                            var val = db.LookupValue.Find(int.Parse(SubCaste));
                                            ESOBJ.SubCaste = val;

                                            var type = db.EmpSocialInfo
                                                .Include(e => e.SubCaste)
                                                .Where(e => e.Id == Eid).SingleOrDefault();
                                            IList<EmpSocialInfo> typedetails = null;
                                            if (type.SubCaste != null)
                                            {
                                                typedetails = db.EmpSocialInfo.Where(x => x.SubCaste.Id == type.SubCaste.Id && x.Id == Eid).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.EmpSocialInfo.Where(x => x.Id == Eid).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.SubCaste = ESOBJ.SubCaste;
                                                db.EmpSocialInfo.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var Dtls = db.EmpSocialInfo.Include(e => e.SubCaste).Where(x => x.Id == Eid).ToList();
                                            foreach (var s in Dtls)
                                            {
                                                s.SubCaste = null;
                                                db.EmpSocialInfo.Attach(s);
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
                                        var Dtls = db.EmpSocialInfo.Include(e => e.SubCaste).Where(x => x.Id == Eid).ToList();
                                        foreach (var s in Dtls)
                                        {
                                            s.SubCaste = null;
                                            db.EmpSocialInfo.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    var CurOBJ = db.EmpSocialInfo.Find(Eid);
                                    TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                                    db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        ESOBJ.DBTrack = ESOBJ.DBTrack;
                                        EmpSocialInfo ESIOBJ = new EmpSocialInfo()
                                        {
                                            Id = Eid,
                                            DBTrack = ESOBJ.DBTrack
                                        };

                                        db.EmpSocialInfo.Attach(ESIOBJ);
                                        db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                    }

                                    EmpSocialInfo OBJ = new EmpSocialInfo
                                    {
                                        Id = Eid,
                                        SocialActivities = socialact,
                                        DBTrack = ESOBJ.DBTrack
                                    };

                                    //db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
                                    // db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                    DT_EmpSocialInfo DT_OBJ = (DT_EmpSocialInfo)obj;
                                    DT_OBJ.Caste_Id = blog.Caste == null ? 0 : blog.Caste.Id;
                                    DT_OBJ.Category_Id = blog.Category == null ? 0 : blog.Category.Id;
                                    DT_OBJ.Religion_Id = blog.Religion == null ? 0 : blog.Religion.Id;
                                    DT_OBJ.SubCaste_Id = blog.SubCaste == null ? 0 : blog.SubCaste.Id;
                                    db.Create(DT_OBJ);
                                    db.SaveChanges();
                                    await db.SaveChangesAsync();
                                    db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.Religion.LookupVal, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return Json(new Object[] { ESOBJ.Id, ESOBJ.Religion.LookupVal, "Record Updated", JsonRequestBehavior.AllowGet });
                                }
                                // }
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

                            // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            EmpSocialInfo blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            EmpSocialInfo Old_OBJ = null;
                            using (var context = new DataBaseContext())
                            {
                                blog = context.EmpSocialInfo.Where(e => e.Id == Eid).SingleOrDefault();
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
                            EmpSocialInfo EmpSocialInfo = new EmpSocialInfo()
                            {
                                Id = Eid,
                                SocialActivities = socialact,
                                //Caste = blog.Caste,
                                //Category = blog.Category,
                                //Religion = blog.Religion,
                                //SubCaste = blog.SubCaste,
                                DBTrack = ESOBJ.DBTrack
                            };
                            db.Entry(db_data.CanSocialInfo).State = System.Data.Entity.EntityState.Detached;
                            db.EmpSocialInfo.Attach(EmpSocialInfo);
                            db.Entry(EmpSocialInfo).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(EmpSocialInfo).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            TempData["RowVersion"] = db_data.RowVersion;
                            db.Entry(EmpSocialInfo).State = System.Data.Entity.EntityState.Detached;

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, ESOBJ, "EmpSocialInfo", ESOBJ.DBTrack);
                                DT_EmpSocialInfo DT_OBJ = (DT_EmpSocialInfo)obj;
                                Old_OBJ = context.EmpSocialInfo.Where(e => e.Id == Eid).Include(e => e.Religion)
                                    .Include(e => e.SubCaste)
                                    .Include(e => e.Caste)
                                    .Include(e => e.Caste).SingleOrDefault();

                                DT_OBJ.Religion_Id = DBTrackFile.ValCompare(Old_OBJ.Religion, ESOBJ.Religion);//Old_OBJ.Address == c.Address ? 0 : Old_OBJ.Address == null && c.Address != null ? c.Address.Id : Old_OBJ.Address.Id;
                                DT_OBJ.SubCaste_Id = DBTrackFile.ValCompare(Old_OBJ.SubCaste, ESOBJ.SubCaste); //Old_OBJ.BusinessType == c.BusinessType ? 0 : Old_OBJ.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_OBJ.BusinessType.Id;
                                DT_OBJ.Caste_Id = DBTrackFile.ValCompare(Old_OBJ.Caste, ESOBJ.Caste); //Old_OBJ.SocialActivities == c.SocialActivities ? 0 : Old_OBJ.SocialActivities == null && c.SocialActivities != null ? c.SocialActivities.Id : Old_OBJ.SocialActivities.Id;
                                DT_OBJ.Category_Id = DBTrackFile.ValCompare(Old_OBJ.Category, ESOBJ.Category); //Old_OBJ.SocialActivities == c.SocialActivities ? 0 : Old_OBJ.SocialActivities == null && c.SocialActivities != null ? c.SocialActivities.Id : Old_OBJ.SocialActivities.Id;
                                db.Create(DT_OBJ);
                                //db.SaveChanges();
                            }


                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = ESOBJ.Religion.LookupVal, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { blog.Id, ESOBJ.Religion.LookupVal, "Record Updated", JsonRequestBehavior.AllowGet });
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
        public async Task<ActionResult> EditSave2(EmpSocialInfo c, int data, FormCollection form) // Edit submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Category = form["Categorylist"] == "0" ? "" : form["Categorylist"];
                    string Religion = form["Religionlist"] == "0" ? "" : form["Religionlist"];
                    string Caste = form["Castelist"] == "0" ? "" : form["Castelist"];
                    string SubCaste = form["SubCastelist"] == "0" ? "" : form["SubCastelist"];
                    string SocialActivity = form["SocialActivitieslist"] == "0" ? "" : form["SocialActivitieslist"];

                    //  bool Auth = form["autho_action"] == "" ? false : true;
                    bool Auth = form["autho_allow"] == "true" ? true : false;

                    var Obj = db.Candidate.Where(e => e.Id == data).Include(e => e.CanSocialInfo).Include(e=>e.CanSocialInfo.SocialActivities)
                        .Include(e=>e.CanSocialInfo.Caste).Include(e=>e.CanSocialInfo.Category).Include(e=>e.CanSocialInfo.Religion).Include(e=>e.CanSocialInfo.SubCaste).SingleOrDefault();

                    int SocId = Obj.CanSocialInfo.Id;

                    var socialActivity = db.EmpSocialInfo.Where(e=>e.Id == SocId).Include(e => e.SocialActivities).SingleOrDefault();

                    List<SocialActivities> SOBJ = new List<SocialActivities>();
                    socialActivity.SocialActivities = null;
                    if (SocialActivity != null)
                    {
                        var ids = Utility.StringIdsToListIds(SocialActivity);
                        foreach (var ca in ids)
                        {
                            var SocialAct_val = db.SocialActivities.Find(ca);
                            SOBJ.Add(SocialAct_val);
                            socialActivity.SocialActivities = SOBJ;
                        }
                    }
                  
                    if (Category != null || Category != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Category));
                        c.Category = val;
                    }

                    if (Caste != null || Caste != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Caste));
                        c.Caste = val;
                    }

                    if (Religion != null || Religion != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Religion));
                        c.Religion = val;
                    }

                    if (SubCaste != null || SubCaste != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(SubCaste));
                        c.SubCaste = val;
                    }

              

                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {


                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                EmpSocialInfo blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.EmpSocialInfo.Where(e => e.Id == SocId).Include(e => e.Caste).Include(e => e.Category)
                                        .Include(e => e.Religion).Include(e => e.SocialActivities).Include(e => e.SubCaste).SingleOrDefault();

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

                                //int a = EditS(Geostructlist, PostDetails, data, c, c.DBTrack);

                                if (SocialActivity != null)
                                {
                                    if (SocialActivity != "")
                                    {
                                        //var val = db.SocialActivities.Find(int.Parse(SocialActivity));
                                        //c.SocialActivities = val;

                                        //var add = db.RecruitBatchInitiator.Include(e => e.Geostruct).Where(e => e.Id == data).SingleOrDefault();
                                        var add = db.EmpSocialInfo.Where(e=>e.Id == SocId).Include(e=>e.SocialActivities).SingleOrDefault();
                                        IList<EmpSocialInfo> socialDetails = null;
                                        if (add.SocialActivities != null)
                                        {
                                            socialDetails = db.EmpSocialInfo.Where(e => e.Id == SocId).Include(e => e.SocialActivities).Include(e=>e.Caste).Include(e=>e.Category)
                                                .Include(e=>e.Religion).Include(e=>e.SubCaste).ToList();
                                        }
                                        else
                                        {
                                            socialDetails = db.EmpSocialInfo.Where(x => x.Id == SocId).ToList();
                                        }
                                        if (socialDetails != null)
                                        {
                                            foreach (var s in socialDetails)
                                            {
                                                s.SocialActivities = c.SocialActivities;
                                                db.EmpSocialInfo.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                // await db.SaveChangesAsync(false);
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var socialDetails = db.EmpSocialInfo.Where(e => e.Id == SocId).Include(e => e.SocialActivities).Include(e => e.Caste).Include(e => e.Category)
                                                .Include(e => e.Religion).Include(e => e.SubCaste).ToList();
                                    foreach (var s in socialDetails)
                                    {
                                        s.SocialActivities = null;
                                        db.EmpSocialInfo.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }

                               

                                var CurCorp = db.EmpSocialInfo.Find(SocId);
                                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    //c.DBTrack = dbT;
                                    EmpSocialInfo corp = new EmpSocialInfo()
                                    {
                                        Id = SocId,
                                        DBTrack = c.DBTrack
                                    };


                                    db.EmpSocialInfo.Attach(corp);
                                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                }



                                using (var context = new DataBaseContext())
                                {

                                    var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                    db.SaveChanges();
                                }
                                await db.SaveChangesAsync();
                                // var aaq = db.RecruitBatchInitiator.Include(e => e.JobSource).Where(e => e.Id == data).SingleOrDefault();
                                ts.Complete();
                                Msg.Add("Record Updated Successfully.");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

                            EmpSocialInfo blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            EmpSocialInfo Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.EmpSocialInfo.Where(e => e.Id == SocId).SingleOrDefault();
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

                            EmpSocialInfo corp = new EmpSocialInfo()
                            {
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "EmpSocialInfo", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                //Old_Corp = context.RecruitBatchInitiator.Where(e => e.Id == data).Include(e => e.PostDetails)
                                //    .Include(e => e.Geostruct).SingleOrDefault();
                                //DT_RecruitBatchInitiator DT_Corp = (DT_RecruitBatchInitiator)obj;
                                //DT_Corp.PostDetails_Id = DBTrackFile.ValCompare(Old_Corp.PostDetails, c.PostDetails);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.Geostruct_Id = DBTrackFile.ValCompare(Old_Corp.Geostruct, c.Geostruct); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //// DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                //db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.EmpSocialInfo.Attach(blog);
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
                    var clientValues = (EmpSocialInfo)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (EmpSocialInfo)databaseEntry.ToObject();
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


        [HttpPost]
        public async Task<ActionResult> EditSave1(EmpSocialInfo ESOBJ, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    List<string> Msg = new List<string>();
                    string Category = form["Categorylist"] == "0" ? "" : form["Categorylist"];
                    string Religion = form["Religionlist"] == "0" ? "" : form["Religionlist"];
                    string Caste = form["Castelist"] == "0" ? "" : form["Castelist"];
                    string SubCaste = form["SubCastelist"] == "0" ? "" : form["SubCastelist"];
                    string SocialActivity = form["SocialActivitieslist"] == "0" ? "" : form["SocialActivitieslist"];

                    var db_data = db.Candidate.Include(e => e.CanSocialInfo).Include(e => e.CanSocialInfo.SocialActivities)
                       .Include(e => e.CanSocialInfo.Category)
                       .Include(e => e.CanSocialInfo.Religion)
                                 .Include(e => e.CanSocialInfo.Caste)
                                 .Include(e => e.CanSocialInfo.SubCaste)
                                 .Where(e => e.Id == data).SingleOrDefault();

                    int AID = db_data.CanSocialInfo.Id;
                    bool Auth = form["autho_allow"] == "true" ? true : false;
                    //db_data.CanSocialInfo.SocialActivities = null;
                    //db_data.CanSocialInfo.Religion = null;
                    //db_data.CanSocialInfo.Category = null;
                    //db_data.CanSocialInfo.Caste = null;
                    //db_data.CanSocialInfo.SubCaste = null;

                    List<SocialActivities> SOBJ = new List<SocialActivities>();
                    if (SocialActivity != null)
                    {
                        var ids = Utility.StringIdsToListIds(SocialActivity);
                        foreach (var ca in ids)
                        {
                            var SocialAct_val = db.SocialActivities.Find(ca);
                            SOBJ.Add(SocialAct_val);
                            ESOBJ.SocialActivities = SOBJ;
                        }
                    }
                    var socAct = db_data.CanSocialInfo.SocialActivities;

                    if (Category != null || Category != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Category));
                        ESOBJ.Category = val;
                    }

                    if (Caste != null || Caste != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Caste));
                        ESOBJ.Caste = val;
                    }
                    if (Religion != null || Religion != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Religion));
                        ESOBJ.Religion = val;
                    }
                    if (SubCaste != null || SubCaste != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(SubCaste));
                        ESOBJ.SubCaste = val;
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
                                    db.EmpSocialInfo.Attach(db_data.CanSocialInfo);
                                    db.Entry(db_data.CanSocialInfo).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_data.CanSocialInfo.RowVersion;
                                    db.Entry(db_data.CanSocialInfo).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_Lookup = db.EmpSocialInfo.Find(AID);
                                    TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                    db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        EmpSocialInfo blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.EmpSocialInfo.Where(e => e.Id == AID).Include(e => e.Caste)
                                    .Include(e => e.Category)
                                    .Include(e => e.Religion)
                                      .Include(e => e.SocialActivities)
                                    .Include(e => e.SubCaste).SingleOrDefault();
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
                                        EmpSocialInfo lk = new EmpSocialInfo
                                        {
                                            Id = AID,
                                            Caste = ESOBJ.Caste,
                                            SubCaste = ESOBJ.SubCaste,
                                            Category = ESOBJ.Category,
                                            Religion = ESOBJ.Religion,
                                            SocialActivities = socAct,
                                            DBTrack = ESOBJ.DBTrack
                                        };


                                        db.EmpSocialInfo.Attach(lk);
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

                                        // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


                                        using (var context = new DataBaseContext())
                                        {

                                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                            DT_EmpSocialInfo DT_Corp = (DT_EmpSocialInfo)obj;

                                            db.Create(DT_Corp);
                                            db.SaveChanges();
                                        }
                                        await db.SaveChangesAsync();
                                        ts.Complete();

                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = lk.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (EmpSocialInfo)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    var databaseValues = (EmpSocialInfo)databaseEntry.ToObject();
                                    ESOBJ.RowVersion = databaseValues.RowVersion;

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

                            EmpSocialInfo blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            EmpSocialInfo Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.EmpSocialInfo.Where(e => e.Id == AID).SingleOrDefault();
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
                            EmpSocialInfo empSocialInfo = new EmpSocialInfo()
                            {
                                Id = AID,
                                Caste = ESOBJ.Caste,
                                SubCaste = ESOBJ.SubCaste,
                                Category = ESOBJ.Category,
                                Religion = ESOBJ.Religion,
                                SocialActivities = socAct,
                                DBTrack = ESOBJ.DBTrack
                            };

                            db.Entry(blog).State = System.Data.Entity.EntityState.Detached;
                            empSocialInfo.DBTrack = ESOBJ.DBTrack;
                            // db.EmpAcademicInfo.Attach(empAcademicInfo);                   
                            //db.Entry(empAcademicInfo).State = System.Data.Entity.EntityState.Modified;
                            //db.Entry(empAcademicInfo).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                    List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                return View();
            }
        }


        //[HttpPost]
        //public async Task<ActionResult> EditSave(EmpSocialInfo ESOBJ, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            string Values = form["SocialActivitieslist"];
        //            bool Auth = form["autho_allow"] == "true" ? true : false;


        //            var db_data = db.Candidate.Include(e => e.CanSocialInfo).Include(e => e.CanSocialInfo.SocialActivities)
        //                .Include(e => e.CanSocialInfo.Category)
        //                .Include(e => e.CanSocialInfo.Religion)
        //                          .Include(e => e.CanSocialInfo.Caste)
        //                          .Include(e => e.CanSocialInfo.SubCaste)
        //                          .Where(e => e.Id == data).SingleOrDefault();

        //            int Eid = db_data.CanSocialInfo.Id;
        //            // var db_data = db.EmpSocialInfo.Include(e => e.SocialActivities).Where(e => e.Id == data).SingleOrDefault();
        //            List<SocialActivities> SOBJ = new List<SocialActivities>();
        //            db_data.CanSocialInfo.SocialActivities = null;
        //            if (Values != null)
        //            {
        //                var ids = Utility.StringIdsToListIds(Values);
        //                foreach (var ca in ids)
        //                {
        //                    var lookup_val = db.SocialActivities.Find(ca);
        //                    SOBJ.Add(lookup_val);
        //                    db_data.CanSocialInfo.SocialActivities = SOBJ;
        //                }
        //            }
        //            var socialact = db_data.CanSocialInfo.SocialActivities;
        //            string Category = form["Categorylist"] == "0" ? "" : form["Categorylist"];
        //            if (Category != null)
        //            {
        //                if (Category != "")
        //                {
        //                    var val = db.LookupValue.Find(int.Parse(Category));
        //                    ESOBJ.Category = val;
        //                }
        //            }
        //            string Religion = form["Religionlist"] == "0" ? "" : form["Religionlist"];
        //            if (Religion != null)
        //            {
        //                if (Religion != "")
        //                {
        //                    var val = db.LookupValue.Find(int.Parse(Religion));
        //                    ESOBJ.Religion = val;
        //                }
        //            }
        //            string Caste = form["Castelist"] == "0" ? "" : form["Castelist"];
        //            if (Caste != null)
        //            {
        //                if (Caste != "")
        //                {
        //                    var val = db.LookupValue.Find(int.Parse(Caste));
        //                    ESOBJ.Caste = val;
        //                }
        //            }
        //            string SubCaste = form["SubCastelist"] == "0" ? "" : form["SubCastelist"];
        //            if (SubCaste != null)
        //            {
        //                if (SubCaste != "")
        //                {
        //                    var val = db.LookupValue.Find(int.Parse(SubCaste));
        //                    ESOBJ.SubCaste = val;
        //                }
        //            }





        //            if (Auth == false)
        //            {


        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {

        //                        //DbContextTransaction transaction = db.Database.BeginTransaction();
        //                        db.EmpSocialInfo.Attach(db_data.CanSocialInfo);
        //                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = db_data.RowVersion;
        //                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

        //                        var Curr_OBJ = db.EmpSocialInfo.Find(Eid);
        //                        TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
        //                        db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

        //                        //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                        //{
        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            EmpSocialInfo blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.EmpSocialInfo.Where(e => e.Id == Eid)
        //                                                         .Include(e => e.SocialActivities)
        //                                                         .Include(e => e.Category)
        //                                                         .Include(e => e.Religion)
        //                                                         .Include(e => e.Caste)
        //                                                         .Include(e => e.SubCaste)
        //                                                         .SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            ESOBJ.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };
        //                            //  int a = EditS(Religion, Category, Caste, SubCaste, Values, Eid, ESOBJ, ESOBJ.DBTrack);

        //                            if (Religion != null)
        //                            {
        //                                if (Religion != "")
        //                                {
        //                                    var val = db.LookupValue.Find(int.Parse(Religion));
        //                                    ESOBJ.Religion = val;

        //                                    var type = db.Candidate.Include(e => e.CanSocialInfo.Religion).Where(e => e.Id == data).SingleOrDefault();

        //                                    IList<EmpSocialInfo> typedetails = null;
        //                                    if (type.CanSocialInfo.Religion != null)
        //                                    {
        //                                        typedetails = db.Candidate.Where(x => x.CanSocialInfo.Religion.Id == type.CanSocialInfo.Religion.Id && x.Id == data).Select(e => e.CanSocialInfo).ToList();
        //                                    }
        //                                    else
        //                                    {
        //                                        typedetails = db.Candidate.Where(e => e.Id == data).Select(e => e.CanSocialInfo).ToList();
        //                                    }
        //                                    //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //                                    foreach (var s in typedetails)
        //                                    {
        //                                        s.Religion = ESOBJ.Religion;
        //                                        db.EmpSocialInfo.Attach(s);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        TempData["RowVersion"] = s.RowVersion;
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    //var Dtls = db.EmpSocialInfo.Include(e => e.Religion).Where(x => x.Id == data).ToList();
        //                                    var Dtls = db.Candidate.Where(e => e.Id == data).Include(e => e.CanSocialInfo).Include(e => e.CanSocialInfo.Religion).ToList();
        //                                    foreach (var s in Dtls)
        //                                    {
        //                                        s.CanSocialInfo.Religion = null;
        //                                        db.EmpSocialInfo.Attach(s.CanSocialInfo);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        TempData["RowVersion"] = s.RowVersion;
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                // var Dtls = db.EmpSocialInfo.Include(e => e.Religion).Where(x => x.Id == data).ToList();
        //                                var Dtls = db.Candidate.Include(e => e.CanSocialInfo).Include(e => e.CanSocialInfo.Religion).Where(e => e.Id == data).ToList();

        //                                foreach (var s in Dtls)
        //                                {
        //                                    s.CanSocialInfo.Religion = null;
        //                                    db.EmpSocialInfo.Attach(s.CanSocialInfo);
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                    //await db.SaveChangesAsync();
        //                                    db.SaveChanges();
        //                                    TempData["RowVersion"] = s.RowVersion;
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                }
        //                            }



        //                            if (Category != null)
        //                            {
        //                                if (Category != "")
        //                                {
        //                                    var val = db.LookupValue.Find(int.Parse(Category));
        //                                    ESOBJ.Religion = val;

        //                                    //var type = db.EmpSocialInfo
        //                                    //  .Include(e => e.Category)
        //                                    //   .Where(e => e.Id == data).SingleOrDefault();

        //                                    var type = db.Candidate.Where(e => e.Id == data).Include(e => e.CanSocialInfo).Include(e => e.CanSocialInfo.Category).SingleOrDefault();

        //                                    IList<EmpSocialInfo> typedetails = null;
        //                                    if (type.CanSocialInfo.Category != null)
        //                                    {
        //                                        // typedetails = db.EmpSocialInfo.Where(x => x.Category.Id == type.Category.Id && x.Id == data).ToList();
        //                                        typedetails = db.Candidate.Where(x => x.CanSocialInfo.Category.Id == type.CanSocialInfo.Category.Id && x.Id == data).Select(e => e.CanSocialInfo).ToList();
        //                                    }
        //                                    else
        //                                    {
        //                                        typedetails = db.Candidate.Where(e => e.Id == data).Select(b => b.CanSocialInfo).ToList();
        //                                        // typedetails = db.EmpSocialInfo.Where(x => x.Id == data).ToList();
        //                                    }
        //                                    //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //                                    foreach (var s in typedetails)
        //                                    {
        //                                        s.Category = ESOBJ.Religion;
        //                                        db.EmpSocialInfo.Attach(s);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        TempData["RowVersion"] = s.RowVersion;
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    // var Dtls = db.EmpSocialInfo.Include(e => e.Category).Where(x => x.Id == data).ToList();
        //                                    var Dtls = db.Candidate.Where(e => e.Id == data).Include(e => e.CanSocialInfo).Include(e => e.CanSocialInfo.Category).ToList();
        //                                    foreach (var s in Dtls)
        //                                    {
        //                                        s.CanSocialInfo.Category = null;
        //                                        db.EmpSocialInfo.Attach(s.CanSocialInfo);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        TempData["RowVersion"] = s.RowVersion;
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                //var Dtls = db.EmpSocialInfo.Include(e => e.Religion).Where(x => x.Id == data).ToList();
        //                                var Dtls = db.Candidate.Where(e => e.Id == data).Include(e => e.CanSocialInfo).Include(e => e.CanSocialInfo.Religion).ToList();
        //                                foreach (var s in Dtls)
        //                                {
        //                                    s.CanSocialInfo.Religion = null;
        //                                    db.EmpSocialInfo.Attach(s.CanSocialInfo);
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                    //await db.SaveChangesAsync();
        //                                    db.SaveChanges();
        //                                    TempData["RowVersion"] = s.RowVersion;
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                }
        //                            }


        //                            if (Caste != null)
        //                            {
        //                                if (Caste != "")
        //                                {
        //                                    var val = db.LookupValue.Find(int.Parse(Caste));
        //                                    ESOBJ.Caste = val;
        //                                    //var type = db.EmpSocialInfo
        //                                    //  .Include(e => e.Caste)
        //                                    //.Where(e => e.Id == data).SingleOrDefault();

        //                                    var type = db.Candidate.Where(e => e.Id == data).Include(e => e.CanSocialInfo)
        //                                        .Include(e => e.CanSocialInfo.Caste)
        //                                        .SingleOrDefault();

        //                                    IList<EmpSocialInfo> typedetails = null;
        //                                    if (type.CanSocialInfo.Caste != null)
        //                                    {
        //                                        //typedetails = db.EmpSocialInfo.Where(x => x.Caste.Id == type.Caste.Id && x.Id == data).ToList();
        //                                        typedetails = db.Candidate.Where(e => e.CanSocialInfo.Caste.Id == type.CanSocialInfo.Caste.Id && e.Id == data).Select(e => e.CanSocialInfo).ToList();
        //                                    }
        //                                    else
        //                                    {
        //                                        //typedetails = db.EmpSocialInfo.Where(x => x.Id == data).ToList();
        //                                        typedetails = db.Candidate.Where(e => e.Id == data).Select(e => e.CanSocialInfo).ToList();
        //                                    }
        //                                    //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //                                    foreach (var s in typedetails)
        //                                    {
        //                                        s.Caste = ESOBJ.Caste;
        //                                        db.EmpSocialInfo.Attach(s);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        TempData["RowVersion"] = s.RowVersion;
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    // var Dtls = db.EmpSocialInfo.Include(e => e.Caste).Where(x => x.Id == data).ToList();
        //                                    var Dtls = db.Candidate.Where(e => e.Id == data).Include(e => e.CanSocialInfo)
        //                                        .Include(e => e.CanSocialInfo.Caste).ToList();
        //                                    foreach (var s in Dtls)
        //                                    {
        //                                        s.CanSocialInfo.Caste = null;
        //                                        db.EmpSocialInfo.Attach(s.CanSocialInfo);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        TempData["RowVersion"] = s.RowVersion;
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                //var Dtls = db.EmpSocialInfo.Include(e => e.Caste).Where(x => x.Id == data).ToList();
        //                                var Dtls = db.Candidate.Where(e => e.Id == data).Include(e => e.CanSocialInfo).Include(e => e.CanSocialInfo.Caste).ToList();
        //                                foreach (var s in Dtls)
        //                                {
        //                                    s.CanSocialInfo.Caste = null;
        //                                    db.EmpSocialInfo.Attach(s.CanSocialInfo);
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                    //await db.SaveChangesAsync();
        //                                    db.SaveChanges();
        //                                    TempData["RowVersion"] = s.RowVersion;
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                }
        //                            }


        //                            if (SubCaste != null)
        //                            {
        //                                if (SubCaste != "")
        //                                {
        //                                    var val = db.LookupValue.Find(int.Parse(SubCaste));
        //                                    ESOBJ.SubCaste = val;

        //                                    //var type = db.EmpSocialInfo
        //                                    //    .Include(e => e.SubCaste)
        //                                    //    .Where(e => e.Id == data).SingleOrDefault();

        //                                    var type = db.Candidate.Where(e => e.Id == data).Include(e => e.CanSocialInfo).Include(e => e.CanSocialInfo.SubCaste).SingleOrDefault();
        //                                    IList<EmpSocialInfo> typedetails = null;
        //                                    if (type.CanSocialInfo.SubCaste != null)
        //                                    {
        //                                        // typedetails = db.EmpSocialInfo.Where(x => x.SubCaste.Id == type.Religion.Id && x.Id == data).ToList();
        //                                        typedetails = db.Candidate.Where(e => e.CanSocialInfo.SubCaste.Id == type.CanSocialInfo.Religion.Id && e.Id == data).Select(e => e.CanSocialInfo).ToList();
        //                                    }
        //                                    else
        //                                    {
        //                                        typedetails = db.Candidate.Where(x => x.Id == data).Select(e => e.CanSocialInfo).ToList();
        //                                    }
        //                                    //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //                                    foreach (var s in typedetails)
        //                                    {
        //                                        s.SubCaste = ESOBJ.Religion;
        //                                        db.EmpSocialInfo.Attach(s);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        TempData["RowVersion"] = s.RowVersion;
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    // var Dtls = db.EmpSocialInfo.Include(e => e.SubCaste).Where(x => x.Id == data).ToList();
        //                                    var Dtls = db.Candidate.Where(e => e.Id == data).Include(e => e.CanSocialInfo).Include(e => e.CanSocialInfo.SubCaste).ToList();

        //                                    foreach (var s in Dtls)
        //                                    {
        //                                        s.CanSocialInfo.SubCaste = null;
        //                                        db.EmpSocialInfo.Attach(s.CanSocialInfo);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        TempData["RowVersion"] = s.RowVersion;
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                //var Dtls = db.EmpSocialInfo.Include(e => e.SubCaste).Where(x => x.Id == data).ToList();
        //                                var Dtls = db.Candidate.Where(e => e.Id == data).Include(e => e.CanSocialInfo).Include(e => e.CanSocialInfo.SubCaste).ToList();
        //                                foreach (var s in Dtls)
        //                                {
        //                                    s.CanSocialInfo.SubCaste = null;
        //                                    db.EmpSocialInfo.Attach(s.CanSocialInfo);
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                    //await db.SaveChangesAsync();
        //                                    db.SaveChanges();
        //                                    TempData["RowVersion"] = s.RowVersion;
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                }
        //                            }




        //                            var CurOBJ = db.EmpSocialInfo.Find(data);

        //                            TempData["CurrRowVersion"] = CurOBJ.RowVersion;
        //                            db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
        //                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            {
        //                                //ESOBJ.DBTrack = dbT;
        //                                EmpSocialInfo ESIOBJ = new EmpSocialInfo()
        //                                {
        //                                    Id = data,
        //                                    DBTrack = ESOBJ.DBTrack
        //                                };

        //                                db.EmpSocialInfo.Attach(ESIOBJ);
        //                                db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
        //                                db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                                //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                            }


        //                            EmpSocialInfo OBJ = new EmpSocialInfo
        //                            {
        //                                Id = Eid,
        //                                SocialActivities = socialact,
        //                                DBTrack = ESOBJ.DBTrack
        //                            };

        //                            //db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
        //                            // db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
        //                            DT_EmpSocialInfo DT_OBJ = (DT_EmpSocialInfo)obj;
        //                            DT_OBJ.Caste_Id = blog.Caste == null ? 0 : blog.Caste.Id;
        //                            DT_OBJ.Category_Id = blog.Category == null ? 0 : blog.Category.Id;
        //                            DT_OBJ.Religion_Id = blog.Religion == null ? 0 : blog.Religion.Id;
        //                            DT_OBJ.SubCaste_Id = blog.SubCaste == null ? 0 : blog.SubCaste.Id;
        //                            db.Create(DT_OBJ);
        //                            db.SaveChanges();
        //                            await db.SaveChangesAsync();
        //                            db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
        //                            ts.Complete();
        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.Religion.LookupVal, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                            //return Json(new Object[] { ESOBJ.Id, ESOBJ.Religion.LookupVal, "Record Updated", JsonRequestBehavior.AllowGet });
        //                        }
        //                        // }
        //                    }
        //                    catch (DbUpdateException e) { throw e; }
        //                    catch (DataException e) { throw e; }
        //                }
        //                else
        //                {
        //                    StringBuilder sb = new StringBuilder("");
        //                    foreach (ModelState modelState in ModelState.Values)
        //                    {
        //                        foreach (ModelError error in modelState.Errors)
        //                        {
        //                            sb.Append(error.ErrorMessage);
        //                            sb.Append("." + "\n");
        //                        }
        //                    }
        //                    var errorMsg = sb.ToString();
        //                    Msg.Add(errorMsg);
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    EmpSocialInfo blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    EmpSocialInfo Old_OBJ = null;
        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.EmpSocialInfo.Where(e => e.Id == Eid).SingleOrDefault();
        //                        TempData["RowVersion"] = blog.RowVersion;
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }
        //                    ESOBJ.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };
        //                    EmpSocialInfo EmpSocialInfo = new EmpSocialInfo()
        //                    {
        //                        Id = Eid,
        //                        SocialActivities = socialact,
        //                        //Caste = blog.Caste,
        //                        //Category = blog.Category,
        //                        //Religion = blog.Religion,
        //                        //SubCaste = blog.SubCaste,
        //                        DBTrack = ESOBJ.DBTrack
        //                    };
        //                    db.Entry(db_data.CanSocialInfo).State = System.Data.Entity.EntityState.Detached;
        //                    db.EmpSocialInfo.Attach(EmpSocialInfo);
        //                    db.Entry(EmpSocialInfo).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(EmpSocialInfo).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    TempData["RowVersion"] = db_data.RowVersion;
        //                    db.Entry(EmpSocialInfo).State = System.Data.Entity.EntityState.Detached;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, ESOBJ, "EmpSocialInfo", ESOBJ.DBTrack);
        //                        DT_EmpSocialInfo DT_OBJ = (DT_EmpSocialInfo)obj;
        //                        Old_OBJ = context.EmpSocialInfo.Where(e => e.Id == Eid).Include(e => e.Religion)
        //                            .Include(e => e.SubCaste)
        //                            .Include(e => e.Caste)
        //                            .Include(e => e.Caste).SingleOrDefault();

        //                        DT_OBJ.Religion_Id = DBTrackFile.ValCompare(Old_OBJ.Religion, ESOBJ.Religion);//Old_OBJ.Address == c.Address ? 0 : Old_OBJ.Address == null && c.Address != null ? c.Address.Id : Old_OBJ.Address.Id;
        //                        DT_OBJ.SubCaste_Id = DBTrackFile.ValCompare(Old_OBJ.SubCaste, ESOBJ.SubCaste); //Old_OBJ.BusinessType == c.BusinessType ? 0 : Old_OBJ.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_OBJ.BusinessType.Id;
        //                        DT_OBJ.Caste_Id = DBTrackFile.ValCompare(Old_OBJ.Caste, ESOBJ.Caste); //Old_OBJ.SocialActivities == c.SocialActivities ? 0 : Old_OBJ.SocialActivities == null && c.SocialActivities != null ? c.SocialActivities.Id : Old_OBJ.SocialActivities.Id;
        //                        DT_OBJ.Category_Id = DBTrackFile.ValCompare(Old_OBJ.Category, ESOBJ.Category); //Old_OBJ.SocialActivities == c.SocialActivities ? 0 : Old_OBJ.SocialActivities == null && c.SocialActivities != null ? c.SocialActivities.Id : Old_OBJ.SocialActivities.Id;
        //                        db.Create(DT_OBJ);
        //                        //db.SaveChanges();
        //                    }


        //                    ts.Complete();
        //                    Msg.Add("  Record Updated");
        //                    return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = ESOBJ.Religion.LookupVal, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    //return Json(new Object[] { blog.Id, ESOBJ.Religion.LookupVal, "Record Updated", JsonRequestBehavior.AllowGet });
        //                }

        //            }
        //            return View();
        //        }
        //        catch (Exception ex)
        //        {
        //            LogFile Logfile = new LogFile();
        //            ErrorLog Err = new ErrorLog()
        //            {
        //                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                ExceptionMessage = ex.Message,
        //                ExceptionStackTrace = ex.StackTrace,
        //                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}






        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save Authorized data
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
                            //EmpSocialInfo OBJ = db.EmpSocialInfo.Find(auth_id);
                            //EmpSocialInfo OBJ = db.EmpSocialInfo.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                            EmpSocialInfo OBJ = db.EmpSocialInfo
                                                .Include(e => e.Religion)
                                                .Include(e => e.Category)
                                                .Include(e => e.Caste)
                                                .Include(e => e.SubCaste).
                                                FirstOrDefault(e => e.Id == auth_id);

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

                            db.EmpSocialInfo.Attach(OBJ);
                            db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, OBJ.DBTrack);
                            DT_EmpSocialInfo DT_OBJ = (DT_EmpSocialInfo)rtn_Obj;
                            DT_OBJ.Religion_Id = OBJ.Religion == null ? 0 : OBJ.Religion.Id;
                            DT_OBJ.Caste_Id = OBJ.Caste == null ? 0 : OBJ.Caste.Id;
                            DT_OBJ.Category_Id = OBJ.Category == null ? 0 : OBJ.Category.Id;
                            DT_OBJ.SubCaste_Id = OBJ.SubCaste == null ? 0 : OBJ.SubCaste.Id;
                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorized");
                            return Json(new Utility.JsonReturnClass { Id = OBJ.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { OBJ.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        EmpSocialInfo Old_OBJ = db.EmpSocialInfo
                                                .Include(e => e.Religion)
                                                .Include(e => e.Category)
                                                .Include(e => e.Caste)
                                                .Include(e => e.SubCaste)
                                                .Where(e => e.Id == auth_id).SingleOrDefault();

                        DT_EmpSocialInfo Curr_OBJ = db.DT_EmpSocialInfo
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_OBJ != null)
                        {
                            EmpSocialInfo OBJ = new EmpSocialInfo();

                            string Religion = Curr_OBJ.Religion_Id == null ? null : Curr_OBJ.Religion_Id.ToString();
                            string Caste = Curr_OBJ.Caste_Id == null ? null : Curr_OBJ.Caste_Id.ToString();
                            string Subcaste = Curr_OBJ.SubCaste_Id == null ? null : Curr_OBJ.SubCaste_Id.ToString();
                            string Category = Curr_OBJ.Category_Id == null ? null : Curr_OBJ.Category_Id.ToString();
                            string SA = Curr_OBJ.SocialActivities_Id == null ? null : Curr_OBJ.SocialActivities_Id.ToString();

                            //OBJ.Name = Curr_OBJ.Name == null ? Old_OBJ.Name : Curr_OBJ.Name;
                            //OBJ.Code = Curr_OBJ.Code == null ? Old_OBJ.Code : Curr_OBJ.Code;
                            //      OBJ.Id = auth_id;

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
                                        int a = EditS(Religion, Category, Caste, Subcaste, SA, auth_id, OBJ, OBJ.DBTrack);
                                        //int a = EditS(OBJ, Addrs, SocialActivities, auth_id, OBJ, OBJ.DBTrack);    
                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        Msg.Add("  Record Authorized");
                                        return Json(new Utility.JsonReturnClass { Id = OBJ.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        // return Json(new Object[] { OBJ.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (EmpSocialInfo)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (EmpSocialInfo)databaseEntry.ToObject();
                                        OBJ.RowVersion = databaseValues.RowVersion;
                                    }
                                }

                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
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
                            //EmpSocialInfo OBJ = db.EmpSocialInfo.Find(auth_id);
                            EmpSocialInfo OBJ = db.EmpSocialInfo.AsNoTracking()
                                                .Include(e => e.Religion)
                                                .Include(e => e.Category)
                                                .Include(e => e.Caste)
                                                .Include(e => e.SubCaste)
                                                .FirstOrDefault(e => e.Id == auth_id);

                            LookupValue val = OBJ.Caste;

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

                            db.EmpSocialInfo.Attach(OBJ);
                            db.Entry(OBJ).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, OBJ.DBTrack);
                            DT_EmpSocialInfo DT_OBJ = (DT_EmpSocialInfo)rtn_Obj;
                            DT_OBJ.Caste_Id = OBJ.Caste == null ? 0 : OBJ.Caste.Id;
                            DT_OBJ.SubCaste_Id = OBJ.SubCaste == null ? 0 : OBJ.SubCaste.Id;
                            DT_OBJ.Religion_Id = OBJ.Religion == null ? 0 : OBJ.Religion.Id;
                            DT_OBJ.Category_Id = OBJ.Category == null ? 0 : OBJ.Category.Id;
                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();
                            db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorized ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "Record Authorized", JsonRequestBehavior.AllowGet });
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











        public int EditS(string RVal, string CatVal, string CasteVal, string SubCasteVal, string SAVal, int data, EmpSocialInfo ESOBJ, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (RVal != null)
                {
                    if (RVal != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(RVal));
                        ESOBJ.Religion = val;

                        var type = db.EmpSocialInfo.Include(e => e.Religion)
                            .Where(e => e.Id == data).SingleOrDefault();
                        IList<EmpSocialInfo> typedetails = null;
                        if (type.Religion != null)
                        {
                            typedetails = db.EmpSocialInfo.Where(x => x.Religion.Id == type.Religion.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.EmpSocialInfo.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.Religion = ESOBJ.Religion;
                            db.EmpSocialInfo.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.EmpSocialInfo.Include(e => e.Religion).Where(x => x.Id == data).ToList();
                        foreach (var s in Dtls)
                        {
                            s.Religion = null;
                            db.EmpSocialInfo.Attach(s);
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
                    var Dtls = db.EmpSocialInfo.Include(e => e.Religion).Where(x => x.Id == data).ToList();
                    foreach (var s in Dtls)
                    {
                        s.Religion = null;
                        db.EmpSocialInfo.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }



                if (CatVal != null)
                {
                    if (CatVal != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(CatVal));
                        ESOBJ.Religion = val;

                        var type = db.EmpSocialInfo
                            .Include(e => e.Category)
                            .Where(e => e.Id == data).SingleOrDefault();
                        IList<EmpSocialInfo> typedetails = null;
                        if (type.Category != null)
                        {
                            typedetails = db.EmpSocialInfo.Where(x => x.Category.Id == type.Category.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.EmpSocialInfo.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.Category = ESOBJ.Religion;
                            db.EmpSocialInfo.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.EmpSocialInfo.Include(e => e.Category).Where(x => x.Id == data).ToList();
                        foreach (var s in Dtls)
                        {
                            s.Category = null;
                            db.EmpSocialInfo.Attach(s);
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
                    var Dtls = db.EmpSocialInfo.Include(e => e.Religion).Where(x => x.Id == data).ToList();
                    foreach (var s in Dtls)
                    {
                        s.Religion = null;
                        db.EmpSocialInfo.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                if (CasteVal != null)
                {
                    if (CasteVal != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(CasteVal));
                        ESOBJ.Caste = val;
                        var type = db.EmpSocialInfo
                            .Include(e => e.Caste)
                            .Where(e => e.Id == data).SingleOrDefault();
                        IList<EmpSocialInfo> typedetails = null;
                        if (type.Caste != null)
                        {
                            typedetails = db.EmpSocialInfo.Where(x => x.Caste.Id == type.Caste.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.EmpSocialInfo.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.Caste = ESOBJ.Caste;
                            db.EmpSocialInfo.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.EmpSocialInfo.Include(e => e.Caste).Where(x => x.Id == data).ToList();
                        foreach (var s in Dtls)
                        {
                            s.Caste = null;
                            db.EmpSocialInfo.Attach(s);
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
                    var Dtls = db.EmpSocialInfo.Include(e => e.Caste).Where(x => x.Id == data).ToList();
                    foreach (var s in Dtls)
                    {
                        s.Caste = null;
                        db.EmpSocialInfo.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                if (SubCasteVal != null)
                {
                    if (SubCasteVal != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(SubCasteVal));
                        ESOBJ.SubCaste = val;

                        var type = db.EmpSocialInfo
                            .Include(e => e.SubCaste)
                            .Where(e => e.Id == data).SingleOrDefault();
                        IList<EmpSocialInfo> typedetails = null;
                        if (type.SubCaste != null)
                        {
                            typedetails = db.EmpSocialInfo.Where(x => x.SubCaste.Id == type.Religion.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.EmpSocialInfo.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.SubCaste = ESOBJ.Religion;
                            db.EmpSocialInfo.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.EmpSocialInfo.Include(e => e.SubCaste).Where(x => x.Id == data).ToList();
                        foreach (var s in Dtls)
                        {
                            s.SubCaste = null;
                            db.EmpSocialInfo.Attach(s);
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
                    var Dtls = db.EmpSocialInfo.Include(e => e.SubCaste).Where(x => x.Id == data).ToList();
                    foreach (var s in Dtls)
                    {
                        s.SubCaste = null;
                        db.EmpSocialInfo.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                //if (SAVal != null)
                //{
                //    if (SAVal != "")
                //    {

                //        List<int> IDs = SAVal.Split(',').Select(e => int.Parse(e)).ToList();
                //        foreach (var k in IDs)
                //        {
                //            var value = db.SocialActivities.Find(k);
                //            ESOBJ.SocialActivities = new List<SocialActivities>();
                //            ESOBJ.SocialActivities.Add(value);
                //        }
                //    }
                //}
                //else
                //{
                //    var Details = db.EmpSocialInfo.Include(e => e.SocialActivities).Where(x => x.Id == data).ToList();
                //    foreach (var s in Details)
                //    {
                //        s.SocialActivities = null;
                //        db.EmpSocialInfo.Attach(s);
                //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                //        //await db.SaveChangesAsync();
                //        db.SaveChanges();
                //        TempData["RowVersion"] = s.RowVersion;
                //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                //    }
                //}



                var CurOBJ = db.EmpSocialInfo.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    ESOBJ.DBTrack = dbT;
                    EmpSocialInfo ESIOBJ = new EmpSocialInfo()
                    {
                        Id = data,
                        DBTrack = ESOBJ.DBTrack
                    };

                    db.EmpSocialInfo.Attach(ESIOBJ);
                    db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }



        [HttpPost]
        public async Task<ActionResult> Delete(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    EmpSocialInfo EmpSocialInfo = db.EmpSocialInfo.Include(e => e.SocialActivities).Where(e => e.Id == data).SingleOrDefault();

                    if (EmpSocialInfo.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = EmpSocialInfo.DBTrack.CreatedBy != null ? EmpSocialInfo.DBTrack.CreatedBy : null,
                                CreatedOn = EmpSocialInfo.DBTrack.CreatedOn != null ? EmpSocialInfo.DBTrack.CreatedOn : null,
                                IsModified = EmpSocialInfo.DBTrack.IsModified == true ? true : false
                            };
                            EmpSocialInfo.DBTrack = dbT;
                            db.Entry(EmpSocialInfo).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, EmpSocialInfo.DBTrack);
                            DT_EmpSocialInfo DT_OBJ = (DT_EmpSocialInfo)rtn_Obj;
                            db.Create(DT_OBJ);

                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            var selectedValues = EmpSocialInfo.SocialActivities;
                            var lkValue = new HashSet<int>(EmpSocialInfo.SocialActivities.Select(e => e.Id));
                            if (lkValue.Count > 0)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                            }
                            db.Entry(EmpSocialInfo).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // Msg.Add("  Data removed successfully.  ");
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



        //[ValidateAntiForgeryToken]
        public ActionResult DeleteOld(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    EmpSocialInfo EmpSocialInfo = db.EmpSocialInfo.Include(e => e.SocialActivities).Where(e => e.Id == data).SingleOrDefault();
                    try
                    {
                        var selectedValues = EmpSocialInfo.SocialActivities;
                        var lkValue = new HashSet<int>(EmpSocialInfo.SocialActivities.Select(e => e.Id));
                        if (lkValue.Count > 0)
                        {
                            Msg.Add(" Child record exists.Cannot remove it..  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                        }

                        using (TransactionScope ts = new TransactionScope())
                        {
                            db.Entry(EmpSocialInfo).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            ts.Complete();
                        }
                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        // Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    catch (DataException /* dex */)
                    {
                        //return this.Json(new Object[] { "", "Unable to delete. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
                        Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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


        //public ActionResult Getalldetails(string v, string x)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.EmpSocialInfo.ToList();
        //        IEnumerable<EmpSocialInfo> all;
        //        if (!string.IsNullOrEmpty(x))
        //        {
        //            all = db.EmpSocialInfo.ToList().Where(d => d.Category.Contains(x) || d.Religion.ToLower().Contains(x.ToLower()));

        //        }
        //        else
        //        {
        //            var r = (from ca in fall select new { srno = ca.Id, SocialActivities = ca.Category + " - " + ca.Religion }).Distinct();
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.Category, c.Religion }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //}
        //public ActionResult GetLookupValue(string data, string data2)
        //{
        //    SelectList s = (SelectList)null;
        //    var selected = "";
        //    if (data != "" && data != null)
        //    {
        //        var qurey = db.EmpSocialInfo.Include(e => e.SocialActivities)
        //            .Where(e => e.Id == data).SingleOrDefault();
        //        if (data2 != "" && data2 != "0")
        //        {
        //            selected = data2;
        //        }
        //        if (qurey != null)
        //        {
        //            s = new SelectList(qurey.SocialActivities, "Id", "OBJ", selected);
        //        }
        //    }
        //    return Json(s, JsonRequestBehavior.AllowGet);

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
        //        var LKVal = db.EmpSocialInfo.ToList();

        //        if (gp.IsAutho == true)
        //        {
        //            LKVal = db.EmpSocialInfo.Include(e => e.SocialActivities).Include(e => e.Category).Include(e => e.Religion).Include(e => e.Caste).Include(e => e.SubCaste).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            LKVal = db.EmpSocialInfo.Include(e => e.SocialActivities).Include(e => e.Category).Include(e => e.Religion).Include(e => e.Caste).Include(e => e.SubCaste).AsNoTracking().ToList();
        //        }


        //        IEnumerable<EmpSocialInfo> IE;
        //        if (!string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = LKVal;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.Category, a.Religion, a.Caste, a.SubCaste }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Category.LookupVal.ToLower() == gp.searchString) || (e.Religion.LookupVal.ToLower() == gp.searchString.ToLower()) || (e.Caste.LookupVal.ToLower() == gp.searchString.ToLower()) || (e.SubCaste.LookupVal.ToLower() == gp.searchString.ToLower())));
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Category.LookupVal, a.Religion.LookupVal, a.Caste.LookupVal, a.SubCaste.LookupVal }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = LKVal;
        //            Func<EmpSocialInfo, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "Category" ? c.Category.LookupVal.ToString() :
        //                                 gp.sidx == "Religion" ? c.Religion.LookupVal.ToString() :
        //                                 gp.sidx == "Caste" ? c.Caste.LookupVal.ToString() :
        //                                 gp.sidx == "SubCaste" ? c.SubCaste.LookupVal.ToString() :
        //                                 "");
        //            }

        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.Category.LookupVal, a.Religion.LookupVal, a.Caste.LookupVal, a.SubCaste.LookupVal }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.Category.LookupVal, a.Religion.LookupVal, a.Caste.LookupVal, a.SubCaste.LookupVal }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Category.LookupVal, a.Religion.LookupVal, a.Caste.LookupVal, a.SubCaste.LookupVal }).ToList();
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
        //        IEnumerable<Candidate> Candidate = null;
        //        if (gp.IsAutho == true)
        //        {
        //            Candidate = db.Candidate.Include(e => e.CanSocialInfo).Include(e => e.CanName).Where(e => e.DBTrack.IsModified == true).ToList();
        //        }
        //        else
        //        {
        //            Candidate = db.Candidate.Include(e => e.CanSocialInfo).Include(e => e.CanName).Where(e=>e.CanSocialInfo != null).ToList();
        //        }

        //        IEnumerable<Candidate> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = Candidate;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.CanName.FullNameFML, a.CanSocialInfo.Caste.LookupVal }).Where((e => (e.Id.ToString() == gp.searchString) || (e.FullNameFML.ToLower() == gp.searchString.ToLower()) || (e.LookupVal.ToString() == gp.searchString.ToLower()) )).ToList();
        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.PreferredHospital), Convert.ToString(a.IDMark), Convert.ToString(a.BloodGroup) != null ? Convert.ToString(a.BloodGroup.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.CanName.FullNameFML, a.CanSocialInfo != null ? a.CanSocialInfo.Caste.LookupVal : null, a.CanSocialInfo != null ? a.CanSocialInfo.SubCaste.LookupVal : "" }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = Candidate;
        //            Func<Candidate, int> orderfuc = (c =>
        //                                                       gp.sidx == "Id" ? c.Id : 0);
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.CanName.FullNameFML, a.CanSocialInfo != null ? a.CanSocialInfo.Caste.LookupVal : null, a.CanSocialInfo != null ? a.CanSocialInfo.SubCaste.LookupVal : "" }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.CanName.FullNameFML, a.CanSocialInfo != null ? a.CanSocialInfo.Caste.LookupVal : null, a.CanSocialInfo != null ? a.CanSocialInfo.SubCaste.LookupVal : "" }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.CanName.FullNameFML, a.CanSocialInfo != null ? a.CanSocialInfo.Caste.LookupVal : null, a.CanSocialInfo != null ? a.CanSocialInfo.SubCaste.LookupVal : "" }).ToList();
        //            }
        //            totalRecords = Candidate.Count();
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

                IEnumerable<Candidate> Employee = null;
                Employee = db.Candidate.Include(q => q.CanSocialInfo).Include(q => q.CanName).ToList();
                if (gp.IsAutho == true)
                {
                    Employee = db.Candidate.Include(q => q.CanSocialInfo).Include(q => q.CanName).Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    // Employee = db.Employee.Where(a=>a.Id==db.EmpAcademicInfo.Contains()).ToList();
                    Employee = db.Candidate.Include(q => q.CanSocialInfo).Include(q => q.CanName).Where(q => q.CanSocialInfo != null).ToList();
                }
                IEnumerable<Candidate> IE;

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Employee;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new Object[] { a.Id, a.CanCode, a.CanName.FullNameFML }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.CanCode, a.CanName.FullNameFML }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Employee;
                    Func<Candidate, string> orderfuc = (c => gp.sidx == "ID" ? c.Id.ToString() : "");



                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.CanCode, a.CanName.FullNameFML }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.CanCode, a.CanName.FullNameFML }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.CanCode, a.CanName.FullNameFML }).ToList();
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


        public ActionResult GetLookupDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.SocialActivities.ToList();
                IEnumerable<SocialActivities> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.SocialActivities.ToList().Where(d => d.InstituteName.Contains(data));
                }
                else
                {
                    //  var list1 = db.EmpSocialInfo.ToList().SelectMany(e => e.SocialActivities);
                    var list1 = db.EmpSocialInfo.SelectMany(e => e.SocialActivities).ToList();
                    var list2 = fall.Except(list1);
                    var r = (from ca in list2 select new { srno = ca.Id, SocialActivities = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }

                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
