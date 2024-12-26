using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Recruitment;
using System.Web.Mvc;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System.Transactions;
using P2b.Global;
using P2BUltimate.Security;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;


namespace P2BUltimate.Controllers.Recruitment.MainController
{
    public class RecruitEvaluationParaController : Controller
    {
        List<String> Msg = new List<String>();
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /RecruitEvaluationPara/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Recruitement/_RecruitEvaluationPara.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(RecruitEvaluationPara c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Category = form["CategorylistEvalpara"] == "0" ? "" : form["CategorylistEvalpara"];

                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Category));
                            c.RecruitEvalPara = val;
                        }
                    }

                    c.SelectionPanel = null;
                    List<SelectionPanel> OBJ = new List<SelectionPanel>();
                    string Values = form["SelectionPanellist"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.SelectionPanel.Find(ca);
                            OBJ.Add(OBJ_val);
                            c.SelectionPanel = OBJ;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            RecruitEvaluationPara corporate = new RecruitEvaluationPara()
                            {

                                SelectionPanel = c.SelectionPanel,
                                RecruitEvalPara = c.RecruitEvalPara,
                                Stage = c.Stage,
                                DBTrack = c.DBTrack
                            };

                            db.RecruitEvaluationPara.Add(corporate);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Recruitment/Recruitment", null, db.ChangeTracker, c.DBTrack);
                            //DT_RecruitEvaluationPara DT_Corp = (DT_RecruitEvaluationPara)rtn_Obj;
                            //DT_Corp.RecruitEvalPara_Id = c.RecruitEvalPara == null ? 0 : c.RecruitEvalPara.Id;
                            //db.Create(DT_Corp);
                            db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


                            ts.Complete();
                            Msg.Add("Data Saved Successfully.");
                            return Json(new Utility.JsonReturnClass { Id = corporate.Id, Val = corporate.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public class RecruitSelectionPanel
        {
            public Array RE_id { get; set; }
            public Array RE_val { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            //string tableName = "Corporate";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.RecruitEvaluationPara
             .Include(e => e.RecruitEvalPara)

             .Where(e => e.Id == data).Select
             (e => new
             {

                 RecruitEvalPara_Id = e.RecruitEvalPara.Id == null ? 0 : e.RecruitEvalPara.Id,
                 Stage = e.Stage,
                 Action = e.DBTrack.Action
             }).ToList();

                List<RecruitSelectionPanel> return_data = new List<RecruitSelectionPanel>();


                var a = db.RecruitEvaluationPara.Include(e => e.RecruitEvalPara).Where(e => e.Id == data).Select(e => e.SelectionPanel).ToList();

                foreach (var ca in a)
                {
                    return_data.Add(
                new RecruitSelectionPanel
                {
                    RE_id = ca.Select(e => e.Id.ToString()).ToArray(),
                    RE_val = ca.Select(e => e.FullDetails).ToArray()
                });
                }

                var Corp = db.RecruitEvaluationPara.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, return_data, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(RecruitEvaluationPara c, int data, FormCollection form) // Edit submit
        {

            // string ContactDetails = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
            //  bool Auth = form["Autho_Action"] == "" ? false : true;
            using (DataBaseContext db = new DataBaseContext())
            {
                bool Auth = form["Autho_Allow"] == "true" ? true : false;
                string Corp = form["CategorylistEvalpara"] == "0" ? "" : form["CategorylistEvalpara"];

                var db_Data = db.RecruitEvaluationPara.Include(e => e.RecruitEvalPara).Include(e => e.SelectionPanel)
                     .Where(e => e.Id == data).SingleOrDefault();
                db_Data.RecruitEvalPara = null;
                db_Data.SelectionPanel = null;

                if (Corp != null)
                {
                    if (Corp != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Corp));
                        db_Data.RecruitEvalPara = val;
                    }
                }


                List<SelectionPanel> job_agency = new List<SelectionPanel>();
                string j_agency = form["SelectionPanellist"];

                if (j_agency != null)
                {
                    var ids = Utility.StringIdsToListIds(j_agency);
                    foreach (var ca in ids)
                    {
                        var Lookup_val = db.SelectionPanel.Find(ca);

                        job_agency.Add(Lookup_val);
                        db_Data.SelectionPanel = job_agency;
                    }
                }
                else
                {
                    db_Data.SelectionPanel = null;
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
                                using (var context = new DataBaseContext())
                                {
                                    db.RecruitEvaluationPara.Attach(db_Data);
                                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_Data.RowVersion;
                                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_OBJ = db.RecruitEvaluationPara.Find(data);
                                    TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                    db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        RecruitEvaluationPara blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;


                                        blog = context.RecruitEvaluationPara.Where(e => e.Id == data).Include(e => e.RecruitEvalPara)
                                                                .Include(e => e.SelectionPanel)
                                                              .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;


                                        c.DBTrack = new DBTrack
                                        {
                                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };

                                        RecruitEvaluationPara lk = new RecruitEvaluationPara
                                        {
                                            Id = data,

                                            RecruitEvalPara = db_Data.RecruitEvalPara,
                                            SelectionPanel = db_Data.SelectionPanel,
                                            Stage = db_Data.Stage,
                                            DBTrack = c.DBTrack
                                        };


                                        db.RecruitEvaluationPara.Attach(lk);
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                        // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                        //db.SaveChanges();
                                        db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                        var obj = DBTrackFile.DBTrackSave("Recruitment/Recruitment", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        //DT_RecruitEvaluationPara DT_LK = (DT_RecruitEvaluationPara)obj;
                                        //DT_LK.RecruitEvalPara_Id = c.RecruitEvalPara == null ? 0 : c.RecruitEvalPara.Id;
                                        //db.Create(DT_LK);
                                        db.SaveChanges();
                                        await db.SaveChangesAsync();
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
                                        var aaq = db.RecruitEvaluationPara.Include(e => e.RecruitEvalPara).Include(e => e.SelectionPanel).Where(e => e.Id == data).SingleOrDefault();
                                        ts.Complete();
                                        Msg.Add("Record Updated Successfully.");
                                        return Json(new Utility.JsonReturnClass { Id = aaq.Id, Val = aaq.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (RecruitEvaluationPara)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (RecruitEvaluationPara)databaseEntry.ToObject();
                                c.RowVersion = databaseValues.RowVersion;

                            }
                        }

                        return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                    }
                }
                else
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        RecruitEvaluationPara blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        RecruitEvaluationPara Old_Corp = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.RecruitEvaluationPara.Where(e => e.Id == data).SingleOrDefault();
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

                        RecruitEvaluationPara corp = new RecruitEvaluationPara()
                        {

                            Id = data,
                            RecruitEvalPara = blog.RecruitEvalPara,
                            Stage = blog.Stage,
                            DBTrack = c.DBTrack,
                            RowVersion = (Byte[])TempData["RowVersion"]
                        };


                        //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                        using (var context = new DataBaseContext())
                        {
                            var obj = DBTrackFile.ModifiedDataHistory("Recruitement/Recruitement", "M", blog, corp, "JobSource", c.DBTrack);
                            // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                            Old_Corp = context.RecruitEvaluationPara.Where(e => e.Id == data).Include(e => e.RecruitEvalPara)
                                .Include(e => e.SelectionPanel).SingleOrDefault();
                            DT_RecruitEvaluationPara DT_Corp = (DT_RecruitEvaluationPara)obj;
                            DT_Corp.RecruitEvalPara_Id = DBTrackFile.ValCompare(Old_Corp.RecruitEvalPara, c.RecruitEvalPara);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;

                            db.Create(DT_Corp);
                            db.SaveChanges();
                        }
                        blog.DBTrack = c.DBTrack;
                        db.RecruitEvaluationPara.Attach(blog);
                        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        var aaq = db.RecruitEvaluationPara.Include(e => e.RecruitEvalPara).Include(e => e.SelectionPanel).Where(e => e.Id == data).SingleOrDefault();
                        ts.Complete();
                        Msg.Add("Record Updated Successfully.");
                        return Json(new Utility.JsonReturnClass { Id = aaq.Id, Val = aaq.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                }
                return View();
            }
        }


        public ActionResult GetLookup(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.SelectionPanel.Include(e => e.Employee).Include(e => e.ExternalSelector)

                                     .ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.SelectionPanel.Include(e => e.Employee).Include(e => e.ExternalSelector).Include(e => e.MaxPoints)
                                .Include(e => e.PanelName).Include(e => e.PanelType).Include(e => e.SelectionCriteria)
                                   .Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
            // return View();
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    RecruitEvaluationPara recruitEva = db.RecruitEvaluationPara.Include(e => e.RecruitEvalPara)
                        .Include(e => e.SelectionPanel).Include(e => e.Stage)


                                                        .Where(e => e.Id == data).SingleOrDefault();



                    if (recruitEva.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = recruitEva.DBTrack.CreatedBy != null ? recruitEva.DBTrack.CreatedBy : null,
                                CreatedOn = recruitEva.DBTrack.CreatedOn != null ? recruitEva.DBTrack.CreatedOn : null,
                                IsModified = recruitEva.DBTrack.IsModified == true ? true : false
                            };
                            recruitEva.DBTrack = dbT;
                            db.Entry(recruitEva).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Recruitment/Recruitment", null, db.ChangeTracker, recruitEva.DBTrack);
                            DT_RecruitEvaluationPara DT_OBJ = (DT_RecruitEvaluationPara)rtn_Obj;
                            db.Create(DT_OBJ);

                            await db.SaveChangesAsync();
                            ts.Complete();

                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {


                            var lkValue1 = new HashSet<int>(recruitEva.SelectionPanel.Select(e => e.Id));
                            if (lkValue1.Count > 0)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                            }





                            db.Entry(recruitEva).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            ts.Complete();

                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    db.Entry(recruitEva).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                    // ts.Complete();

                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
            }
        }
    }
}