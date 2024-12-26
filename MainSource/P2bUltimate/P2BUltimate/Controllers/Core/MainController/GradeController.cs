///
/// Created by Tanushri
///


using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Core.MainController
{
     [AuthoriseManger]
    public class GradeController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/Grade/Index.cshtml");
        }

        public ActionResult LevelPartial()
        {
            return View("~/Views/Shared/Core/_Level.cshtml");
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

                            Grade AOBJ = db.Grade.Include(e => e.Levels)
                               .FirstOrDefault(e => e.Id == auth_id);

                            AOBJ.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = AOBJ.DBTrack.ModifiedBy != null ? AOBJ.DBTrack.ModifiedBy : null,
                                CreatedBy = AOBJ.DBTrack.CreatedBy != null ? AOBJ.DBTrack.CreatedBy : null,
                                CreatedOn = AOBJ.DBTrack.CreatedOn != null ? AOBJ.DBTrack.CreatedOn : null,
                                IsModified = AOBJ.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.Grade.Attach(AOBJ);
                            db.Entry(AOBJ).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(AOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, AOBJ.DBTrack);
                            DT_Grade DT_OBJ = (DT_Grade)rtn_Obj;

                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "M", AOBJ, null, "Grade", AOBJ.DBTrack);
                            //}
                            db.Create(DT_OBJ);
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = AOBJ.Id, Val = AOBJ.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { AOBJ.Id, AOBJ.Name, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        Grade Old_OBJ = db.Grade.Include(e => e.Levels)
                                                         .Where(e => e.Id == auth_id).SingleOrDefault();

                        DT_Grade Curr_OBJ = db.DT_Grade
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        Grade MOBJ = new Grade();

                        string OBJ = Curr_OBJ.Levels_Id == null ? null : Curr_OBJ.Levels_Id.ToString();
                        MOBJ.Name = Curr_OBJ.Name == null ? Old_OBJ.Name : Curr_OBJ.Name;
                        MOBJ.Code = Curr_OBJ.Code == null ? Old_OBJ.Code : Curr_OBJ.Code;

                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    // db.Configuration.AutoDetectChangesEnabled = false;
                                    MOBJ.DBTrack = new DBTrack
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

                                    int a = EditS(OBJ, auth_id, MOBJ, MOBJ.DBTrack);

                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = MOBJ.Id, Val = MOBJ.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { MOBJ.Id, MOBJ.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Grade)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (Grade)databaseEntry.ToObject();
                                    MOBJ.RowVersion = databaseValues.RowVersion;
                                }
                            }

                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Grade OBJ = db.Grade.Find(auth_id);
                            Grade DOBJ = db.Grade.AsNoTracking().Include(e => e.Levels)
                                                                        .FirstOrDefault(e => e.Id == auth_id);
                            // Level Var1 = DOBJ.Levels.ToLookup();
                            ICollection<Level> Var1 = DOBJ.Levels;
                            DOBJ.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = DOBJ.DBTrack.ModifiedBy != null ? DOBJ.DBTrack.ModifiedBy : null,
                                CreatedBy = DOBJ.DBTrack.CreatedBy != null ? DOBJ.DBTrack.CreatedBy : null,
                                CreatedOn = DOBJ.DBTrack.CreatedOn != null ? DOBJ.DBTrack.CreatedOn : null,
                                IsModified = DOBJ.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.Grade.Attach(DOBJ);
                            db.Entry(DOBJ).State = System.Data.Entity.EntityState.Deleted;
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {
                                DOBJ.Levels = Var1;
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", DOBJ, null, "Grade", DOBJ.DBTrack);
                            }


                            db.Entry(DOBJ).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record Authorised", JsonRequestBehavior.AllowGet });
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


        /*----------------------------- Grid View ------------------------------------- */

        public ActionResult Grid_Grade(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;
                    var Grade = db.Grade.ToList();
                    IEnumerable<Grade> IE;
                    if (!string.IsNullOrEmpty(gp.searchField))
                    {
                        IE = Grade;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.Name }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = Grade;
                        Func<Grade, string> orderfuc = (c => gp.sidx == "ID" ? c.Id.ToString() :
                                                            gp.sidx == "Code" ? Convert.ToString(c.Code) :
                                                            gp.sidx == "Name" ? Convert.ToString(c.Name) :
                                                                   "");

                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.Name }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.Name }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name }).ToList();
                        }
                        totalRecords = Grade.Count();
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
        }


        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }



        /*---------------------------------------------------------- Create ---------------------------------------------- */
        [HttpPost]
        public ActionResult Create(Grade gr, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int comp_Id = 0;
                    comp_Id = Convert.ToInt32(Session["CompId"]);
                    var Company = new Company();
                    Company = db.Company.Where(x => x.Id == comp_Id).SingleOrDefault();


                    gr.Levels = null;
                    List<Level> OBJ = new List<Level>();
                    string Values = form["LevelList"];

                    if (Values != null)
                    {
                        var ids = one_ids(Values);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.Level.Find(ca);
                            OBJ.Add(OBJ_val);
                            gr.Levels = OBJ;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            if (db.Grade.Any(o => o.Code == gr.Code))
                            {
                                Msg.Add("  Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            }
                            gr.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            Grade gra = new Grade()
                            {
                                Code = gr.Code,
                                Name = gr.Name,
                                Levels = gr.Levels,
                                DBTrack = gr.DBTrack
                            };

                            try
                            {

                                db.Grade.Add(gra);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, gr.DBTrack);
                                DT_Grade DT_OBJ = (DT_Grade)rtn_Obj;
                                db.Create(DT_OBJ);
                                db.SaveChanges();


                                if (Company != null)
                                {
                                    var objgrade = new List<Grade>();
                                    objgrade.Add(gra);
                                    Company.Grade = objgrade;
                                    db.Company.Attach(Company);
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Detached;

                                }
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });


                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = gr.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                                //return View(gr);
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
                        //var errorMsg = sb.ToString();
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        //return this.Json(new { msg = errorMsg });
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


        /* -------------------------- Level ----------------------*/
        //public ActionResult GetLookupLevel(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.Level.ToList();
        //        IEnumerable<Level> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.Level.ToList().Where(d => d.Name.Contains(data));
        //        }
        //        else
        //        {

        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = "Code :" + ca.Code + "Name :" + ca.Name }).Distinct();

        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.Code, c.Name }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }

        //}

        public ActionResult GetLookupLevel(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Level.ToList();
                IEnumerable<Level> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Level.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }              

        var r = (from ca in fall select new { srno = ca.Id, lookupvalue =ca.FullDetails  }).Distinct();
        return Json(r, JsonRequestBehavior.AllowGet);
               
               
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
                    Grade DOBJ = db.Grade.Where(e => e.Id == data).SingleOrDefault();

                    //Address add = DOBJ.Address;
                    //ContactDetails conDet = DOBJ.ContactDetails;
                    //LookupValue val = DOBJ.BusinessType;
                    //Grade DOBJ = db.Grade.Where(e => e.Id == data).SingleOrDefault();
                    if (DOBJ.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, DOBJ.DBTrack, DOBJ, null, "Grade");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = DOBJ.DBTrack.CreatedBy != null ? DOBJ.DBTrack.CreatedBy : null,
                                CreatedOn = DOBJ.DBTrack.CreatedOn != null ? DOBJ.DBTrack.CreatedOn : null,
                                IsModified = DOBJ.DBTrack.IsModified == true ? true : false
                            };
                            DOBJ.DBTrack = dbT;
                            db.Entry(DOBJ).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, DOBJ.DBTrack);
                            DT_Grade DT_Corp = (DT_Grade)rtn_Obj;
                            //DT_Corp.Address_Id = DOBJ.Address == null ? 0 : DOBJ.Address.Id;
                            //DT_Corp.BusinessType_Id = DOBJ.BusinessType == null ? 0 : DOBJ.BusinessType.Id;
                            //DT_Corp.ContactDetails_Id = DOBJ.ContactDetails == null ? 0 : DOBJ.ContactDetails.Id;
                            db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", DOBJ, null, "Grade", DOBJ.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", DOBJ, null, "Grade", DOBJ.DBTrack );
                            //}
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        //var selectedRegions = DOBJ.Regions;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(DOBJ.Regions.Select(e => e.Id));
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
                                    CreatedBy = DOBJ.DBTrack.CreatedBy != null ? DOBJ.DBTrack.CreatedBy : null,
                                    CreatedOn = DOBJ.DBTrack.CreatedOn != null ? DOBJ.DBTrack.CreatedOn : null,
                                    IsModified = DOBJ.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(DOBJ).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                DT_Grade DT_Corp = (DT_Grade)rtn_Obj;
                                //DT_Corp.Address_Id = add == null ? 0 : add.Id;
                                //DT_Corp.BusinessType_Id = val == null ? 0 : val.Id;
                                //DT_Corp.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
                                db.Create(DT_Corp);

                                await db.SaveChangesAsync();


                                //using (var context = new DataBaseContext())
                                //{
                                //    DOBJ.Address = add;
                                //    DOBJ.ContactDetails = conDet;
                                //    DOBJ.BusinessType = val;
                                //    DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", DOBJ, null, "Grade", dbT);
                                //}
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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

       

        //[HttpPost]
        //public ActionResult Delete(int data)
        //{
        //    Grade grade = db.Grade.Find(data);

        //    try
        //    {
        //        DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };
        //        db.Entry(grade).State = System.Data.Entity.EntityState.Deleted;
        //       // DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
        //        db.SaveChanges();
        //       // return this.Json(new {" ",  "Data removed.", JsonRequestBehavior.AllowGet });
        //        return Json(new Object[] { "","Data removed.", JsonRequestBehavior.AllowGet });
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        return RedirectToAction("Delete", new { concurrencyError = true, id = data });
        //    }
        //    catch (RetryLimitExceededException /* dex */)
        //    {
        //        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
        //    }
        //}


        public class LevelDetails
        {
            public string id { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public Array leveldetail { get; set; }
            public Array level_id { get; set; }
        }




        public class Level_CD
        {
            public Array Level_Id { get; set; }
            public Array Level_FullDetails { get; set; }
        }

       
        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Level_CD> return_data = new List<Level_CD>();
                var Grade = db.Grade
                    .Include(e => e.Levels)
                    .Where(e => e.Id == data).ToList();
                var r = (from ca in Grade
                         select new
                         {
                             Id = ca.Id,
                             Name = ca.Name,
                             Code = ca.Code,
                             Action = ca.DBTrack.Action
                         }).Distinct();


                var a = db.Grade.Include(e => e.Levels).Where(e => e.Id == data).Select(e => e.Levels).ToList();

                foreach (var ca in a)
                {
                    return_data.Add(
                new Level_CD
                {
                    Level_Id = ca.Select(e => e.Id.ToString()).ToArray(),
                    Level_FullDetails = ca.Select(e => e.FullDetails).ToArray()
                });
                }


                TempData["RowVersion"] = db.Grade.Find(data).RowVersion;

                var Old_Data = db.DT_Grade
                 .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M")
                 .Select
                 (e => new
                 {
                     DT_Id = e.Id,
                     Name = e.Name == null ? "" : e.Name,
                     Code = e.Code == null ? "" : e.Code,
                     Level_Val = e.Levels_Id == 0 ? "" : db.Level.Where(x => x.Id == e.Levels_Id).Select(x => x.FullDetails).FirstOrDefault()
                 }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Lkval = db.Grade.Find(data);
                TempData["RowVersion"] = Lkval.RowVersion;
                var Auth = Lkval.DBTrack.IsModified;

                return Json(new Object[] { r, return_data, Old_Data, Auth, JsonRequestBehavior.AllowGet });

                //return this.Json(new Object[] { vals, level, JsonRequestBehavior.AllowGet });
            }
        }


        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }


        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;

        //        IEnumerable<P2BGridData> salheadList = null;
        //        List<P2BGridData> model = new List<P2BGridData>();
        //        P2BGridData view = null;


        //        //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
        //        //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();
        //        int company_Id = 0;
        //        company_Id = Convert.ToInt32(Session["CompId"]);

        //        var BindCompList = db.Company.Include(e => e.Grade).Where(e => e.Id == company_Id).ToList();

        //        foreach (var z in BindCompList)
        //        {
        //            if (z.Grade != null && z.Grade.Count > 0)
        //            {

        //                foreach (var s in z.Grade)
        //                {
        //                    //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
        //                    view = new P2BGridData()
        //                    {
        //                        Id = s.Id,
        //                        Code = s.Code,
        //                        Name = s.Name

        //                    };
        //                    model.Add(view);

        //                }
        //            }

        //        }

        //        salheadList = model;

        //        IEnumerable<P2BGridData> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = salheadList;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "Id")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code,a.Name }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                if (gp.searchField == "Code")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code, a.Name }).Where((e => (e.Code.ToString().Contains(gp.searchString)))).ToList();
        //                if (gp.searchField == "Name")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code, a.Name }).Where((e => (e.Name.ToString().Contains(gp.searchString)))).ToList();

        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code != null ? Convert.ToString(a.Code) : "", a.Name != null ? Convert.ToString(a.Name) : "" }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = salheadList;
        //            Func<P2BGridData, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "Code" ? c.Id.ToString() :
        //                                 gp.sidx == "Name" ? c.Name.ToString() : ""

        //                                );
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.Code != null ? Convert.ToString(a.Code) : "", a.Name != null ? Convert.ToString(a.Name) : "" }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.Code != null ? Convert.ToString(a.Code) : "", a.Name != null ? Convert.ToString(a.Name) : "" }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code != null ? Convert.ToString(a.Code) : "", a.Name != null ? Convert.ToString(a.Name) : "" }).ToList();
        //            }
        //            totalRecords = salheadList.Count();
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
                int ParentId = 2;
                var jsonData = (Object)null;
                var LKVal = db.Grade.ToList();

                if (gp.IsAutho == true)
                {
                    LKVal = db.Grade.Include(e => e.Levels).AsNoTracking().Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    LKVal = db.Grade.Include(e => e.Levels).AsNoTracking().ToList(); ;
                }

                IEnumerable<Grade> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = LKVal;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                              || (e.Name.ToUpper().Contains(gp.searchString.ToUpper()))
                              || (e.Id.ToString().Contains(gp.searchString.ToUpper()))
                              ).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
                        //jsonData = IE.Select(a => new { a.Code, a.Name, a.Id }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Code.ToLower() == gp.searchString.ToLower()) || (e.Name.ToLower() == gp.searchString.ToLower()))).ToList();                      
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {

                    IE = LKVal;
                    Func<Grade, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.Code :
                                         gp.sidx == "Name" ? c.Name :

                                         "");
                    }
                    //Func<Grade, dynamic> orderfuc;
                    //if (gp.sidx == "Id")
                    //{
                    //    orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    //}
                    //else
                    //{
                    //    orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    //}



                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {

                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
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
        public async Task<ActionResult> EditSave(Grade ESOBJ, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    var db_data = db.Grade.Include(e => e.Levels).Where(e => e.Id == data).SingleOrDefault();
                    List<Level> Level = new List<Level>();
                    string Values = form["Levellist"];

                    if (Values != null)
                    {
                        var ids = one_ids(Values);
                        foreach (var ca in ids)
                        {
                            var ContactDetails_val = db.Level.Find(ca);
                            Level.Add(ContactDetails_val);
                            db_data.Levels = Level;
                        }
                    }
                    else
                    {
                        db_data.Levels = null;
                    }

                    db.Grade.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    TempData["RowVersion"] = db_data.RowVersion;
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;


                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                try
                                {
                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    Grade blog = null; // to retrieve old data                           
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.Grade.Where(e => e.Id == data)
                                                                .Include(e => e.Levels).SingleOrDefault();
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
                                  //  int a = EditS(Values, data, ESOBJ, ESOBJ.DBTrack);

                                    if (Values != null)
                                    {
                                        if (Values != "")
                                        {

                                            List<int> IDs = Values.Split(',').Select(e => int.Parse(e)).ToList();
                                            foreach (var k in IDs)
                                            {
                                                var value = db.Level.Find(k);
                                                ESOBJ.Levels = new List<Level>();
                                                ESOBJ.Levels.Add(value);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var Leveldetails = db.Grade.Include(e => e.Levels).Where(x => x.Id == data).ToList();
                                        foreach (var s in Leveldetails)
                                        {
                                            s.Levels = null;
                                            db.Grade.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }


                                    var CurOBJ = db.Grade.Find(data);
                                    TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                                    db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        ESOBJ.DBTrack = ESOBJ.DBTrack;
                                        Grade TOBJ = new Grade()
                                        {
                                            Code = ESOBJ.Code,
                                            Name = ESOBJ.Name,
                                            Id = data,
                                            DBTrack = ESOBJ.DBTrack
                                        };


                                        db.Grade.Attach(TOBJ);
                                        db.Entry(TOBJ).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(TOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                      
                                    }

                                    await db.SaveChangesAsync();
                                    using (var context = new DataBaseContext())
                                    {

                                        //To save data in history table 
                                        var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, ESOBJ, "Grade", ESOBJ.DBTrack);
                                        DT_Grade DT_GRD = (DT_Grade)Obj;
                                        db.DT_Grade.Add(DT_GRD);
                                        db.SaveChanges();
                                    }
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    // return Json(new Object[] { ESOBJ.Id, ESOBJ.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                }

                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (Grade)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (Grade)databaseEntry.ToObject();
                                        ESOBJ.RowVersion = databaseValues.RowVersion;
                                    }
                                }

                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                    }
                    else
                    {
                      

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            Grade blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            Grade Old_OBJ = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.Grade.Where(e => e.Id == data).SingleOrDefault();
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
                            Grade OBJ = new Grade()
                            {

                                Id = data,
                                Code = ESOBJ.Code,
                                Name = ESOBJ.Name,
                                DBTrack = ESOBJ.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, OBJ, "Grade", ESOBJ.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_OBJ = context.Grade.Where(e => e.Id == data)
                                   .Include(e => e.Levels).SingleOrDefault();
                                DT_Grade DT_OBJ = (DT_Grade)obj;

                                // DT_OBJ.InstituteType_Id = DBTrackFile.ValCompare(Old_OBJ.IsManualRotateShift, ESOBJ.IsManualRotateShift); //Old_OBJ.BusinessType == c.BusinessType ? 0 : Old_OBJ.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_OBJ.BusinessType.Id;
                                DT_OBJ.Levels_Id = DBTrackFile.ValCompare(Old_OBJ.Levels, ESOBJ.Levels); //Old_OBJ.Levels == c.Levels ? 0 : Old_OBJ.Levels == null && c.Levels != null ? c.Levels.Id : Old_OBJ.Levels.Id;
                                db.Create(DT_OBJ);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = ESOBJ.DBTrack;
                            db.Grade.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = OBJ.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { , , "Record Updated", JsonRequestBehavior.AllowGet });
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



        public int EditS(string Lvl, int data, Grade NOBJ, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Lvl != null)
                {
                    if (Lvl != "")
                    {

                        List<int> IDs = Lvl.Split(',').Select(e => int.Parse(e)).ToList();
                        foreach (var k in IDs)
                        {
                            var value = db.Level.Find(k);
                            NOBJ.Levels = new List<Level>();
                            NOBJ.Levels.Add(value);
                        }
                    }
                }
                else
                {
                    var Leveldetails = db.Grade.Include(e => e.Levels).Where(x => x.Id == data).ToList();
                    foreach (var s in Leveldetails)
                    {
                        s.Levels = null;
                        db.Grade.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                var CurOBJ = db.Grade.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    NOBJ.DBTrack = dbT;
                    Grade TOBJ = new Grade()
                    {
                        Code = NOBJ.Code,
                        Name = NOBJ.Name,
                        Id = data,
                        DBTrack = NOBJ.DBTrack
                    };


                    db.Grade.Attach(TOBJ);
                    db.Entry(TOBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(TOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }




        [HttpPost]
        public async Task<ActionResult> EditSave1(Grade g, int data, FormCollection form)
        //[HttpPost]
        //public ActionResult EditSave(Grade g, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    string level = form["Levellist"] == "0" ? "" : form["Levellist"];
                    if (level != null)
                    {
                        if (level != "")
                        {

                            List<int> IDs = level.Split(',').Select(e => int.Parse(e)).ToList();
                            foreach (var k in IDs)
                            {
                                var value = db.Level.Find(k);
                                g.Levels = new List<Level>();
                                g.Levels.Add(value);
                            }
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            try
                            {
                                //if (ModelState.IsValid)
                                //{
                                //    if (levels != null)
                                //    {
                                //        if (levels != "")
                                //        {
                                //            int LvelId = Convert.ToInt32(levels);
                                //            //var level = db.Grade.Include(e => e.Levels).Where(e => e.Id == LvelId).SingleOrDefault();
                                //              var lvelno = db.Grade.Where(e => e.Id == LvelId).SingleOrDefault();
                                //        }
                                //    }
                                //}

                                Grade jb = new Grade()
                                {
                                    Code = g.Code == null ? "" : g.Code.Trim(),
                                    Name = g.Name == null ? "" : g.Name.Trim(),
                                    Levels = g.Levels,
                                    Id = data
                                };
                                db.Entry(jb).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { null, null, "Data saved successfully." }, JsonRequestBehavior.AllowGet);
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Edit", new { concurrencyError = true, id = g.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to Edit.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //ModelState.AddModelError(string.Empty, "Unable to Edit. Try again, and if the problem persists contact your system administrator.");
                                //return RedirectToAction("Edit");
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
                        //var errorMsg = sb.ToString();
                        //return this.Json(new { msg = errorMsg });
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
    }
}