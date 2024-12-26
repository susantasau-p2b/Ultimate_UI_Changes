///
/// Created by Sarika
///

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
using Training;
using P2b.Global;
using System.Text;
using P2BUltimate.Controllers;
using P2BUltimate.App_Start;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Collections;
using P2BUltimate.Security;





namespace P2BUltimate.Controllers.Training.MainController
{
    public class TrainingOrgController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();

        //
        // GET: /TrainingOrg/

        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/TrainingOrg/Index.cshtml");
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

                IEnumerable<OrgTraining> OrgTraining = null;
                if (gp.IsAutho == true)
                {
                    OrgTraining = db.OrgTraining.Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    OrgTraining = db.OrgTraining.ToList();
                }
                IEnumerable<OrgTraining> IE;

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = OrgTraining;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new Object[] { a.Id }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = OrgTraining;
                    Func<OrgTraining, string> orderfuc = (c => gp.sidx == "ID" ? c.Id.ToString() : "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id }).ToList();
                    }
                    totalRecords = OrgTraining.Count();
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

        /* ---------------------------- Training Category -------------------------*/

        public ActionResult GetLookupDetailsCategory(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Category.ToList();
                IEnumerable<Category> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Category.ToList();
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

        /* ---------------------------- Faculty Internal -------------------------*/

        public ActionResult GetLookupDetailsFacultyInternal(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.FacultyInternal.ToList();
                IEnumerable<FacultyInternal> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.FacultyInternal.ToList().Where(d => d.Name.Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.Name }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.Name }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }


        /* ---------------------------- Faculty External -------------------------*/

        public ActionResult GetLookupDetailsFacultyExternal(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.FacultyInternalExternal.ToList();
                IEnumerable<FacultyInternalExternal> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.FacultyInternalExternal.ToList().Where(d => d.FullDetails.Contains(data));

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

        /* ---------------------------- Training Calendar -------------------------*/

        public ActionResult GetLookupDetailsTrainingCalendar(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.YearlyProgramAssignment.Include(q => q.ProgramList).ToList();
                IEnumerable<YearlyProgramAssignment> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.YearlyProgramAssignment.ToList();

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

        /* ---------------------------- Training Schedule  -------------------------*/

        public ActionResult GetLookupDetailsTrainingSchedule(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TrainingSchedule.Include(e => e.Expenses).Include(e => e.Venue).Include(e => e.TrainingCalendar).ToList();
                IEnumerable<TrainingSchedule> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.TrainingSchedule.ToList().Where(d => d.FullDetails.Contains(data));

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


        /* ----------------------------Company -------------------------*/

        public ActionResult GetLookupDetailsCompany(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Company.ToList();
                IEnumerable<Company> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Company.ToList().Where(d => d.Name.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.Name }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.Name }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }


        /*---------------------------------------------------------- Create ---------------------------------------------- */
        [HttpPost]
        public ActionResult Create(OrgTraining ot, FormCollection form)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string Comp = form["CompanyList"];
                    if (Comp != null)
                    {
                        if (Comp != "")
                        {
                            int CompId = Convert.ToInt32(Comp);
                            var vals = db.Company.Where(e => e.Id == CompId).SingleOrDefault();
                            ot.Company = vals;
                        }
                    }
                    else
                    {
                        ot.Company = null;
                    }

                    // category
                    List<Category> lookupCategory = new List<Category>();
                    string ValCategory = form["CategoryList"];

                    if (ValCategory != null)
                    {
                        var ids = Utility.StringIdsToListIds(ValCategory);
                        foreach (var ca in ids)
                        {
                            var lookup_val = db.Category.Find(ca);
                            lookupCategory.Add(lookup_val);
                            ot.Category = lookupCategory;
                        }
                    }
                    else
                    {
                        ot.Category = null;
                    }

                    // FacultyInternalExternal
                    List<FacultyInternalExternal> lookupFext = new List<FacultyInternalExternal>();
                    string FactExter = form["FacultyExternalList"];
                    if (FactExter != null)
                    {
                        var ids = Utility.StringIdsToListIds(FactExter);
                        foreach (var ca in ids)
                        {
                            var lookup_val = db.FacultyInternalExternal.Find(ca);
                            lookupFext.Add(lookup_val);
                            ot.FacultyInternalExternal = lookupFext;
                        }
                    }
                    else
                    {
                        ot.FacultyInternalExternal = null;
                    }


                    // TrainingCalendar
                    List<YearlyProgramAssignment> lookupTrainingCalendar = new List<YearlyProgramAssignment>();
                    string TraiCal = form["TrainingCalendarList"];

                    if (TraiCal != null)
                    {
                        var ids = Utility.StringIdsToListIds(TraiCal);
                        foreach (var ca in ids)
                        {
                            var lookup_val = db.YearlyProgramAssignment.Find(ca);
                            lookupTrainingCalendar.Add(lookup_val);
                            ot.TrainingCalendar = lookupTrainingCalendar;
                        }
                    }
                    else
                    {
                        ot.TrainingCalendar = null;
                    }

                    // TrainingSchedule
                    List<TrainingSchedule> lookupTrainingSchedule = new List<TrainingSchedule>();
                    string TraiShe = form["TrainingScheduleList"];
                    if (TraiShe != null)
                    {
                        var ids = Utility.StringIdsToListIds(TraiShe);
                        foreach (var ca in ids)
                        {
                            var lookup_val = db.TrainingSchedule.Find(ca);
                            lookupTrainingSchedule.Add(lookup_val);
                            ot.TrainingSchedule = lookupTrainingSchedule;
                        }
                    }
                    else
                    {
                        ot.TrainingSchedule = null;
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            ot.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            OrgTraining ort = new OrgTraining()
                            {
                                TrainingCalendar = ot.TrainingCalendar,
                                TrainingSchedule = ot.TrainingSchedule,
                                FacultyInternalExternal = ot.FacultyInternalExternal,
                                Company = ot.Company,
                                Category = ot.Category,
                                DBTrack = ot.DBTrack
                            };
                            try
                            {
                                db.OrgTraining.Add(ort);
                                var a = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, ot.DBTrack);
                                DT_OrgTraining DT_Corp = (DT_OrgTraining)a;

                                db.Create(DT_Corp);
                                db.SaveChanges();

                                ts.Complete();
                                Msg.Add("  Data Created successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = ot.Id });
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


        public class CategoryDetails
        {
            public Array CategoryDetails_Id { get; set; }
            public Array CategoryDetails_val { get; set; }

        }

        public class FacultyExternalDtl
        {
            public Array FacultyExternalDtl_Id { get; set; }
            public Array FacultyExternalDtl_val { get; set; }

        }
        public class FacultyInternalDtl
        {
            public Array FacultyInternalDtl_Id { get; set; }
            public Array FacultyInternalDtl_val { get; set; }

        }

        public class TrainingCalendardtl
        {
            public Array TrainingCalendarDtl_Id { get; set; }
            public Array TrainingCalendarDtl_val { get; set; }

        }

        public class TrainingScheduledtl
        {
            public Array TrainingScheduleDtl_Id { get; set; }
            public Array TrainingScheduleDtl_val { get; set; }

        }



        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.OrgTraining
                .Include(e => e.Company)
                  .Include(e => e.Category)
                  .Include(e => e.FacultyInternalExternal)
                  .Include(e => e.TrainingCalendar)
                  .Include(e => e.TrainingSchedule)
                .Where(e => e.Id == data).ToList();

                var r = (from ca in Q
                         select new
                         {
                             Id = ca.Id,
                             Company_Id = ca.Company.Id == null ? 0 : ca.Company.Id,
                             Action = ca.DBTrack.Action
                         }).Distinct();


                List<CategoryDetails> cat = new List<CategoryDetails>();
                List<FacultyExternalDtl> fext = new List<FacultyExternalDtl>();
                List<FacultyInternalDtl> fint = new List<FacultyInternalDtl>();     //to b removed
                List<TrainingCalendardtl> trncal = new List<TrainingCalendardtl>();
                List<TrainingScheduledtl> trnsch = new List<TrainingScheduledtl>();

                var add_data = db.OrgTraining
                                 .Include(e => e.Company)
                  .Include(e => e.Category)
                  .Include(e => e.FacultyInternalExternal)
                  .Include(e => e.TrainingCalendar)
                  .Include(e => e.TrainingCalendar.Select(q => q.ProgramList))
                  .Include(e => e.TrainingSchedule)
                  .Include(e => e.TrainingSchedule.Select(q => q.City))
                  .Include(e => e.TrainingSchedule.Select(q => q.TrainingCalendar))
                  .Include(e => e.TrainingSchedule.Select(q => q.Venue))
                  .Include(e => e.TrainingSchedule.Select(q => q.Session))
                  .Include(e => e.TrainingSchedule.Select(q => q.Expenses))
                               .Where(e => e.Id == data).ToList();

                foreach (var ca in add_data)
                {
                    cat.Add(new CategoryDetails
                    {
                        CategoryDetails_Id = ca.Category.Select(e => e.Id.ToString()).ToArray(),
                        CategoryDetails_val = ca.Category.Select(e => e.FullDetails).ToArray()
                    });

                    fext.Add(new FacultyExternalDtl
                    {
                        FacultyExternalDtl_Id = ca.FacultyInternalExternal.Select(e => e.Id.ToString()).ToArray(),
                        FacultyExternalDtl_val = ca.FacultyInternalExternal.Select(e => e.FullDetails).ToArray()
                    });

                    //fint.Add(new FacultyInternalDtl
                    //{
                    //    FacultyInternalDtl_Id = ca.FacultyInternal.Select(e => e.Id.ToString()).ToArray(),
                    //    FacultyInternalDtl_val = ca.FacultyInternal.Select(e => e.Name).ToArray()
                    //});

                    trncal.Add(new TrainingCalendardtl
                    {
                        TrainingCalendarDtl_Id = ca.TrainingCalendar.Select(e => e.Id.ToString()).ToArray(),
                        TrainingCalendarDtl_val = ca.TrainingCalendar.Select(e => e.FullDetails).ToArray()
                    });
                    trnsch.Add(new TrainingScheduledtl
                    {
                        TrainingScheduleDtl_Id = ca.TrainingSchedule.Select(e => e.Id.ToString()).ToArray(),
                        TrainingScheduleDtl_val = ca.TrainingSchedule.Select(e => e.FullDetails).ToArray()
                    });
                }
                //TempData["RowVersion"] = db.EmpAcademicInfo.Find(data).RowVersion;

                //return this.Json(new Object[] { r, a, JsonRequestBehavior.AllowGet });


                var W = db.DT_OrgTraining
                    .Include(e => e.Company_Id)
                  .Include(e => e.Category_Id)
                  .Include(e => e.FacultyExternal_Id)
                  .Include(e => e.FacultyInternal_Id)
                  .Include(e => e.TrainingCalendar_Id)
                  .Include(e => e.TrainingSchedule_Id)
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Company_Val = e.Company_Id == 0 ? "" : db.Company.Where(x => x.Id == e.Company_Id).Select(x => x.Name).FirstOrDefault(),
                         //Category_Val = e.Category_Id == 0 ? "" : db.Category.Where(x => x.Id == e.Category_Id).Select(x => x.).FirstOrDefault(),
                         FacultyExternal_Val = e.FacultyExternal_Id == 0 ? "" : db.FacultyInternalExternal.Where(x => x.Id == e.FacultyExternal_Id).Select(x => x.FullDetails).FirstOrDefault(),
                         FacultyInternal_Val = e.FacultyInternal_Id == 0 ? "" : db.FacultyInternal.Where(x => x.Id == e.FacultyInternal_Id).Select(x => x.Name).FirstOrDefault(),
                         //    TrainingCalendar_Val = e.TrainingCalendar_Id == 0 ? "" : db.YearlyTrainingCalendar.Where(x => x.Id == e.TrainingCalendar_Id).Select(x => x.FullDetails).FirstOrDefault(),
                         TrainingSchedule_Val = e.TrainingSchedule_Id == 0 ? "" : db.TrainingSchedule.Where(x => x.Id == e.TrainingSchedule_Id).Select(x => x.FullDetails).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.OrgTraining.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;

                return Json(new Object[] { Q, add_data, W, Auth, "", cat, fext, fint, trncal, trnsch, JsonRequestBehavior.AllowGet });
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
                        //Corporate corp = db.Corporate.Find(auth_id);
                        //Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                        OrgTraining corp = db.OrgTraining
                             .Include(e => e.Company)
                  .Include(e => e.Category)
                  .Include(e => e.FacultyInternalExternal)
                  .Include(e => e.TrainingCalendar)
                  .Include(e => e.TrainingSchedule)
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

                        db.OrgTraining.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //db.SaveChanges();
                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corp.DBTrack);
                        DT_OrgTraining DT_Corp = (DT_OrgTraining)rtn_Obj;
                        //DT_Corp.Awards_Id = corp.Awards == null ? 0 : corp.Awards.ToString();
                        //DT_Corp.Hobby_Id = corp.Hobby == null ? 0 : corp.Hobby;
                        //DT_Corp.LanguageSkill_Id = corp.LanguageSkill == null ? 0 : corp.LanguageSkill;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();

                        ts.Complete();
                        return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                    }
                }
                else if (auth_action == "M")
                {

                    OrgTraining Old_Corp = db.OrgTraining
                            .Include(e => e.Company)
                  .Include(e => e.Category)
                  .Include(e => e.FacultyInternalExternal)

                  .Include(e => e.TrainingCalendar)
                  .Include(e => e.TrainingSchedule)
                          .Where(e => e.Id == auth_id).SingleOrDefault();


                    DT_OrgTraining Curr_Corp = db.DT_OrgTraining
                                                .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                .OrderByDescending(e => e.Id)
                                                .FirstOrDefault();

                    if (Curr_Corp != null)
                    {
                        OrgTraining corp = new OrgTraining();

                        string awrd = Curr_Corp.Company_Id == null ? null : Curr_Corp.Company_Id.ToString();
                        string hob = Curr_Corp.Category_Id == null ? null : Curr_Corp.Category_Id.ToString();
                        string langskl = Curr_Corp.FacultyExternal_Id == null ? null : Curr_Corp.FacultyExternal_Id.ToString();
                        string Qualdtl = Curr_Corp.FacultyInternal_Id == null ? null : Curr_Corp.FacultyInternal_Id.ToString();
                        string skll = Curr_Corp.TrainingCalendar_Id == null ? null : Curr_Corp.TrainingCalendar_Id.ToString();
                        string scolr = Curr_Corp.TrainingSchedule_Id == null ? null : Curr_Corp.TrainingSchedule_Id.ToString();

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

                                    int a = EditS(awrd, hob, langskl, auth_id, corp, corp.DBTrack, Qualdtl, skll, scolr);

                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (OrgTraining)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (OrgTraining)databaseEntry.ToObject();
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
                        OrgTraining corp = db.OrgTraining.AsNoTracking()
                         .Include(e => e.Company)
                  .Include(e => e.Category)
                  .Include(e => e.FacultyInternalExternal)
                  .Include(e => e.TrainingCalendar)
                  .Include(e => e.TrainingSchedule).FirstOrDefault(e => e.Id == auth_id);

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

                        db.OrgTraining.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                        DT_EmpAcademicInfo DT_Corp = (DT_EmpAcademicInfo)rtn_Obj;
                        //DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                        //DT_Corp.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                        //DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();
                        db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
                    }

                }
                return View();
            }
        }


        public int EditS(string awrd, string hob, string langskl, int data, OrgTraining c, DBTrack dbT, string Qualdtl, string skll, string scolr)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (awrd != null)
                {
                    if (awrd != "")
                    {
                        var val = db.Company.Find(int.Parse(awrd));
                        // var val = db.Awards.Where(e => e.Id == data).SingleOrDefault();

                        var r = (from ca in db.Company
                                 select new
                                 {
                                     Id = ca.Id,
                                     LookupVal = ca.Name
                                 }).Where(e => e.Id == data).Distinct();

                        var type = db.OrgTraining.Include(e => e.Company).Where(e => e.Id == data).SingleOrDefault();
                        IList<OrgTraining> typedetails = null;
                        if (type.Company != null)
                        {
                            typedetails = db.OrgTraining.Where(x => x.Company == type.Company && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.OrgTraining.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.Company = c.Company;
                            db.OrgTraining.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.OrgTraining.Include(e => e.Company).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.Company = null;
                            db.OrgTraining.Attach(s);
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
                    var BusiTypeDetails = db.OrgTraining.Include(e => e.Company).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.Company = null;
                        db.OrgTraining.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (hob != null)
                {
                    if (hob != "")
                    {
                        var val = db.Category.Find(int.Parse(hob));
                        //var val = db.Hobby.Where(e => e.Id == data).SingleOrDefault();

                        var r = (from ca in db.Category
                                 select new
                                 {
                                     Id = ca.Id,
                                     LookupVal = ca.Code
                                 }).Where(e => e.Id == data).Distinct();

                        var add = db.OrgTraining.Include(e => e.Category).Where(e => e.Id == data).SingleOrDefault();
                        IList<OrgTraining> addressdetails = null;
                        if (add.Category != null)
                        {
                            addressdetails = db.OrgTraining.Where(x => x.Category == add.Category && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.OrgTraining.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.Category = c.Category;
                                db.OrgTraining.Attach(s);
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
                    var addressdetails = db.OrgTraining.Include(e => e.Category).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.Category = null;
                        db.OrgTraining.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (langskl != null)
                {
                    if (langskl != "")
                    {
                        var val = db.FacultyInternalExternal.Find(int.Parse(langskl));
                        //var val = db.LanguageSkill.Where(e => e.Id == data).SingleOrDefault();

                        var r = (from ca in db.FacultyInternalExternal
                                 select new
                                 {
                                     Id = ca.Id,
                                     LookupVal = ca.FullDetails
                                 }).Where(e => e.Id == data).Distinct();

                        var add = db.OrgTraining.Include(e => e.FacultyInternalExternal).Where(e => e.Id == data).SingleOrDefault();
                        IList<OrgTraining> contactsdetails = null;
                        if (add.FacultyInternalExternal != null)
                        {
                            contactsdetails = db.OrgTraining.Where(x => x.FacultyInternalExternal == add.FacultyInternalExternal && x.Id == data).ToList();
                        }
                        else
                        {
                            contactsdetails = db.OrgTraining.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.FacultyInternalExternal = c.FacultyInternalExternal;
                            db.OrgTraining.Attach(s);
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
                    var contactsdetails = db.OrgTraining.Include(e => e.FacultyInternalExternal).Where(x => x.Id == data).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.FacultyInternalExternal = null;
                        db.OrgTraining.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                //if (Qualdtl != null)
                //{
                //    if (Qualdtl != "")
                //    {
                //        var val = db.FacultyInternal.Find(int.Parse(Qualdtl));
                //        // var val = db.Awards.Where(e => e.Id == data).SingleOrDefault();

                //        var r = (from ca in db.FacultyInternal
                //                 select new
                //                 {
                //                     Id = ca.Id,
                //                     LookupVal = ca.Name
                //                 }).Where(e => e.Id == data).Distinct();

                //        var type = db.OrgTraining.Where(e => e.Id == data).SingleOrDefault();
                //        IList<OrgTraining> typedetails = null;
                //        if (type.FacultyInternal != null)
                //        {
                //            typedetails = db.OrgTraining.Where(x => x.FacultyInternal == type.FacultyInternal && x.Id == data).ToList();
                //        }
                //        else
                //        {
                //            typedetails = db.OrgTraining.Where(x => x.Id == data).ToList();
                //        }
                //        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                //        foreach (var s in typedetails)
                //        {
                //            s.FacultyInternal = c.FacultyInternal;
                //            db.OrgTraining.Attach(s);
                //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                //            //await db.SaveChangesAsync();
                //            db.SaveChanges();
                //            TempData["RowVersion"] = s.RowVersion;
                //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                //        }
                //    }
                //    else
                //    {
                //        var BusiTypeDetails = db.OrgTraining.Where(x => x.Id == data).ToList();
                //        foreach (var s in BusiTypeDetails)
                //        {
                //            s.FacultyInternal = null;
                //            db.OrgTraining.Attach(s);
                //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                //            //await db.SaveChangesAsync();
                //            db.SaveChanges();
                //            TempData["RowVersion"] = s.RowVersion;
                //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                //        }
                //    }
                //}
                //else
                //{
                //    var BusiTypeDetails = db.OrgTraining.Where(x => x.Id == data).ToList();
                //    foreach (var s in BusiTypeDetails)
                //    {
                //        s.FacultyInternal = null;
                //        db.OrgTraining.Attach(s);
                //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                //        //await db.SaveChangesAsync();
                //        db.SaveChanges();
                //        TempData["RowVersion"] = s.RowVersion;
                //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                //    }
                //}


                if (skll != null)
                {
                    if (skll != "")
                    {
                        //  var val = db.YearlyTrainingCalendar.Find(int.Parse(skll));
                        // var val = db.Awards.Where(e => e.Id == data).SingleOrDefault();

                        //var r = (from ca in db.YearlyTrainingCalendar
                        //         select new
                        //         {
                        //             Id = ca.Id,
                        //             LookupVal = ca.FullDetails
                        //         }).Where(e => e.Id == data).Distinct();

                        var type = db.OrgTraining.Include(e => e.TrainingCalendar).Where(e => e.Id == data).SingleOrDefault();
                        IList<OrgTraining> typedetails = null;
                        if (type.TrainingCalendar != null)
                        {
                            typedetails = db.OrgTraining.Where(x => x.TrainingCalendar == type.TrainingCalendar && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.OrgTraining.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.TrainingCalendar = c.TrainingCalendar;
                            db.OrgTraining.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.OrgTraining.Include(e => e.TrainingCalendar).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.TrainingCalendar = null;
                            db.OrgTraining.Attach(s);
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
                    var BusiTypeDetails = db.OrgTraining.Include(e => e.TrainingCalendar).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.TrainingCalendar = null;
                        db.OrgTraining.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }



                if (scolr != null)
                {
                    if (scolr != "")
                    {
                        var val = db.TrainingSchedule.Find(int.Parse(scolr));
                        // var val = db.Awards.Where(e => e.Id == data).SingleOrDefault();

                        var r = (from ca in db.TrainingSchedule
                                 select new
                                 {
                                     Id = ca.Id,
                                     LookupVal = ca.FullDetails
                                 }).Where(e => e.Id == data).Distinct();

                        var type = db.OrgTraining.Include(e => e.TrainingSchedule).Where(e => e.Id == data).SingleOrDefault();
                        IList<OrgTraining> typedetails = null;
                        if (type.TrainingSchedule != null)
                        {
                            typedetails = db.OrgTraining.Where(x => x.TrainingSchedule == type.TrainingSchedule && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.OrgTraining.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.TrainingSchedule = c.TrainingSchedule;
                            db.OrgTraining.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.OrgTraining.Include(e => e.TrainingSchedule).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.TrainingSchedule = null;
                            db.OrgTraining.Attach(s);
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
                    var BusiTypeDetails = db.OrgTraining.Include(e => e.TrainingSchedule).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.TrainingSchedule = null;
                        db.OrgTraining.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                var CurCorp = db.OrgTraining.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    OrgTraining corp = new OrgTraining()
                    {
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.OrgTraining.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(OrgTraining c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var orgedit = db.OrgTraining.Include(e => e.Company)
                      .Include(e => e.Category)
                      .Include(e => e.FacultyInternalExternal)

                      .Include(e => e.TrainingCalendar)
                      .Include(e => e.TrainingSchedule).Where(e => e.Id == data).SingleOrDefault();

                    bool Auth = form["autho_allow"] == "true" ? true : false;

                    List<Category> lookupcat = new List<Category>();
                    string Valuescat = form["CategoryList"];


                    if (Valuescat != null)
                    {
                        var ids = Utility.StringIdsToListIds(Valuescat);
                        foreach (var ca in ids)
                        {
                            var lookup_val = db.Category.Find(ca);
                            lookupcat.Add(lookup_val);
                            orgedit.Category = lookupcat;
                        }
                    }
                    else
                    {
                        orgedit.Category = null;
                    }
                    //3
                    List<FacultyInternalExternal> lookupls = new List<FacultyInternalExternal>();
                    string Valuesls = form["FacultyExternalList"];


                    if (Valuesls != null)
                    {
                        var ids = Utility.StringIdsToListIds(Valuesls);
                        foreach (var ca in ids)
                        {
                            var lookup_val = db.FacultyInternalExternal.Find(ca);
                            lookupls.Add(lookup_val);
                            orgedit.FacultyInternalExternal = lookupls;
                        }
                    }
                    else
                    {
                        orgedit.FacultyInternalExternal = null;
                    }
                    //4
                    List<FacultyInternal> lookupqd = new List<FacultyInternal>();
                    string Valuesqd = form["FacultyInternalList"];

                    //5
                    List<YearlyProgramAssignment> lookupskl = new List<YearlyProgramAssignment>();
                    string Valuesskl = form["TrainingCalendarList"];

                    if (Valuesskl != null)
                    {
                        var ids = Utility.StringIdsToListIds(Valuesskl);
                        foreach (var ca in ids)
                        {
                            var lookup_val = db.YearlyProgramAssignment.Find(ca);
                            lookupskl.Add(lookup_val);
                            orgedit.TrainingCalendar = lookupskl;
                        }
                    }
                    else
                    {
                        orgedit.TrainingCalendar = null;
                    }
                    //6
                    List<TrainingSchedule> lookupscr = new List<TrainingSchedule>();
                    string Valuesscr = form["TrainingScheduleList"];

                    if (Valuesscr != null)
                    {
                        var ids = Utility.StringIdsToListIds(Valuesscr);
                        foreach (var ca in ids)
                        {
                            var lookup_val = db.TrainingSchedule.Find(ca);
                            lookupscr.Add(lookup_val);
                            orgedit.TrainingSchedule = lookupscr;
                        }
                    }
                    else
                    {
                        orgedit.TrainingSchedule = null;
                    }

                    string cmp = form["CompanyList"] == "0" ? "" : form["CompanyList"];

                    if (cmp != null)
                    {
                        if (cmp != "")
                        {
                            var val = db.Company.Find(int.Parse(cmp));
                            c.Company = val;
                        }
                    }

                    //2
                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    db.OrgTraining.Attach(orgedit);
                                    db.Entry(orgedit).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = orgedit.RowVersion;
                                    db.Entry(orgedit).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_Lookup = db.OrgTraining.Find(data);
                                    TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                    db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                    OrgTraining blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.OrgTraining.Where(e => e.Id == data)
                                            .Include(e => e.Company)
                                            .Include(e => e.Category)
                                            .Include(e => e.FacultyInternalExternal)
                                            .Include(e => e.TrainingCalendar)
                                            .Include(e => e.TrainingSchedule).SingleOrDefault();
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
                                    OrgTraining lk = new OrgTraining
                                    {
                                        Id = data,
                                        Company = orgedit.Company,
                                        Category = orgedit.Category,
                                        FacultyInternalExternal = orgedit.FacultyInternalExternal,
                                        TrainingCalendar = orgedit.TrainingCalendar,
                                        TrainingSchedule = orgedit.TrainingSchedule,
                                        DBTrack = c.DBTrack
                                    };


                                    db.OrgTraining.Attach(lk);
                                    db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                    // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                    //db.SaveChanges();
                                    db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

                                    // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


                                    using (var context = new DataBaseContext())
                                    {

                                        var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_OrgTraining DT_Corp = (DT_OrgTraining)obj;
                                        DT_Corp.Category_Id = blog.Company == null ? 0 : blog.Company.Id;
                                        //DT_Corp.BusinessType_Id = blog.BusinessType == null ? 0 : blog.BusinessType.Id;
                                        //DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;

                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);




                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (OrgTraining)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    var databaseValues = (OrgTraining)databaseEntry.ToObject();
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

                            OrgTraining blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            OrgTraining Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.OrgTraining.Where(e => e.Id == data).SingleOrDefault();
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
                            OrgTraining corp = new OrgTraining()
                            {
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "OrgTraining", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.OrgTraining.Where(e => e.Id == data)
                                    .Include(e => e.Company)
                      .Include(e => e.Category)
                      .Include(e => e.FacultyInternalExternal)

                      .Include(e => e.TrainingCalendar)
                      .Include(e => e.TrainingSchedule).SingleOrDefault();
                                DT_OrgTraining DT_Corp = (DT_OrgTraining)obj;
                                DT_Corp.Company_Id = DBTrackFile.ValCompare(Old_Corp.Company.Id, c.Company.Id);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                DT_Corp.Category_Id = DBTrackFile.ValCompare(Old_Corp.Category, c.Category); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                DT_Corp.FacultyExternal_Id = DBTrackFile.ValCompare(Old_Corp.FacultyInternalExternal, c.FacultyInternalExternal); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                DT_Corp.TrainingCalendar_Id = DBTrackFile.ValCompare(Old_Corp.TrainingCalendar, c.TrainingCalendar); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                DT_Corp.TrainingSchedule_Id = DBTrackFile.ValCompare(Old_Corp.TrainingSchedule, c.TrainingSchedule); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.OrgTraining.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();

                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = blog.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


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
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    OrgTraining corporates = db.OrgTraining.Include(e => e.Company)
                      .Include(e => e.Category)
                      .Include(e => e.FacultyInternalExternal)

                      .Include(e => e.TrainingCalendar)
                      .Include(e => e.TrainingSchedule).Where(e => e.Id == data).SingleOrDefault();

                    Company add = corporates.Company;


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
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corporates.DBTrack);
                            DT_OrgTraining DT_Corp = (DT_OrgTraining)rtn_Obj;
                            DT_Corp.Company_Id = corporates.Company == null ? 0 : corporates.Company.Id;
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
                        //var selectedRegions = corporates.Session;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(corporates.Session.Select(e => e.Id));
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
                                DT_OrgTraining DT_Corp = (DT_OrgTraining)rtn_Obj;
                                DT_Corp.Company_Id = add == null ? 0 : add.Id;

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
