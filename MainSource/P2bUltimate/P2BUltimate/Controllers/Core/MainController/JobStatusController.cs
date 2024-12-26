
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
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class JobStatusController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /JobStatus/

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/JobStatus/Index.cshtml");
        }


        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(JobStatus COBJ, FormCollection form)
        {

            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string EmpActingStatus = form["EmpActingStatuslist"] == "0" ? "" : form["EmpActingStatuslist"];
                    string EmpStatus = form["EmpStatuslist"] == "0" ? "" : form["EmpStatuslist"];
                    int comp_Id = Convert.ToInt32(Session["CompID"]);
                    var Company = new Company();
                    Company = db.Company.Where(e => e.Id == comp_Id).SingleOrDefault();
                    if (EmpActingStatus != null && EmpActingStatus != "" && EmpStatus != null && EmpStatus != "")
                    {
                        if (EmpActingStatus != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "102").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(EmpActingStatus)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(EmpActingStatus));
                            COBJ.EmpActingStatus = val;
                        }

                        if (EmpStatus != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "101").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(EmpStatus)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(EmpStatus));
                            COBJ.EmpStatus = val;
                        }



                        if (ModelState.IsValid)
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                if (db.JobStatus.Any(a => a.EmpStatus.LookupVal.ToString() == COBJ.EmpStatus.LookupVal.ToString() && a.EmpActingStatus.LookupVal.ToString() == COBJ.EmpActingStatus.LookupVal.ToString()))
                                {
                                    Msg.Add("Already Exists.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }

                                COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                                JobStatus JobStatus = new JobStatus()
                                {
                                    EmpActingStatus = COBJ.EmpActingStatus,
                                    EmpStatus = COBJ.EmpStatus,
                                    DBTrack = COBJ.DBTrack
                                };
                                try
                                {
                                    db.JobStatus.Add(JobStatus);
                                    var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, COBJ.DBTrack);
                                    DT_JobStatus DT_OBJ = (DT_JobStatus)rtn_Obj;
                                    DT_OBJ.EmpStatus_Id = COBJ.EmpStatus == null ? 0 : COBJ.EmpStatus.Id;
                                    DT_OBJ.EmpActingStatus_Id = COBJ.EmpActingStatus == null ? 0 : COBJ.EmpActingStatus.Id;
                                    db.Create(DT_OBJ);
                                    db.SaveChanges();


                                    if (Company != null)
                                    {
                                        var Objjobstatus = new List<JobStatus>();
                                        Objjobstatus.Add(JobStatus);
                                        Company.JobStatus = Objjobstatus;
                                        db.Company.Attach(Company);
                                        db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(Company).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    ts.Complete();
                                    Msg.Add("  Data Created successfully  ");
                                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return this.Json(new Object[] { null, null, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
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
                            //var errorMsg = sb.ToString();
                            //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                            // return this.Json(new { msg = errorMsg });
                        }
                    }

                    else
                    {
                        var errorMsg = "EmpActingStatus , EmpStatus Can't Be Empty....";
                        Msg.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });

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

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var JobStatus = db.JobStatus
                                    .Include(e => e.EmpActingStatus)
                                    .Include(e => e.EmpStatus)
                                    .Where(e => e.Id == data).ToList();
                var r = (from ca in JobStatus
                         select new
                         {

                             Id = ca.Id,
                             EmpStatus = ca.EmpStatus.Id,
                             EmpActingStatus = ca.EmpActingStatus.Id,
                             Action = ca.DBTrack.Action
                         }).Distinct();

                var a = "";

                var W = db.DT_JobStatus
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         EmpStatus = e.EmpStatus_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.EmpStatus_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),
                         EmpActingStatus = e.EmpActingStatus_Id == 0 ? "" : db.LookupValue
                                     .Where(x => x.Id == e.EmpActingStatus_Id)
                                     .Select(x => x.LookupVal).FirstOrDefault(),

                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var LKup = db.JobStatus.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, a, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }




        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> EditSave(JobStatus ESOBJ, FormCollection form, int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    string EmpStatus = form["EmpStatuslist"] == "0" ? "" : form["EmpStatuslist"];
                    string EmpActingStatus = form["EmpActingStatuslist"] == "0" ? "" : form["EmpActingStatuslist"];
                    if (EmpActingStatus != null && EmpActingStatus != "" && EmpStatus != null && EmpStatus != "")
                    {
                        ESOBJ.EmpActingStatus_Id = int.Parse(EmpActingStatus);
                        ESOBJ.EmpStatus_Id = int.Parse(EmpStatus);
                        if (EmpActingStatus != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "102").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(EmpActingStatus)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(EmpActingStatus));
                            ESOBJ.EmpActingStatus = val;
                        }
                        if (EmpActingStatus != null)
                        {
                            if (EmpActingStatus != "")
                            {
                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "102").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(EmpActingStatus)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmpActingStatus));
                                ESOBJ.EmpActingStatus = val;

                                var type = db.JobStatus
                                    .Include(e => e.EmpActingStatus)
                                    .Where(e => e.Id == data).SingleOrDefault();
                                IList<JobStatus> typedetails = null;
                                if (type.EmpActingStatus != null)
                                {
                                    typedetails = db.JobStatus.Where(x => x.EmpActingStatus.Id == type.EmpActingStatus.Id && x.Id == data).ToList();
                                }
                                else
                                {
                                    typedetails = db.JobStatus.Where(x => x.Id == data).ToList();
                                }
                                //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                foreach (var s in typedetails)
                                {
                                    s.EmpActingStatus = ESOBJ.EmpActingStatus;
                                    db.JobStatus.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                            else
                            {
                                var Dtls = db.JobStatus.Include(e => e.EmpActingStatus).Where(x => x.Id == data).ToList();
                                foreach (var s in Dtls)
                                {
                                    s.EmpActingStatus = null;
                                    db.JobStatus.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                        }

                        if (EmpStatus != null)
                        {
                            if (EmpStatus != "")
                            {
                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "101").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(EmpStatus)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmpStatus));
                                ESOBJ.EmpStatus = val;
                            }
                        }

                        if (EmpStatus != null)
                        {
                            if (EmpStatus != "")
                            {
                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "101").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(EmpStatus)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(EmpStatus));
                                ESOBJ.EmpStatus = val;

                                var type = db.JobStatus.Include(e => e.EmpStatus)
                                    .Where(e => e.Id == data).SingleOrDefault();
                                IList<JobStatus> typedetails = null;
                                if (type.EmpStatus != null)
                                {
                                    typedetails = db.JobStatus.Where(x => x.EmpStatus.Id == type.EmpStatus.Id && x.Id == data).ToList();
                                }
                                else
                                {
                                    typedetails = db.JobStatus.Where(x => x.Id == data).ToList();
                                }
                                //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                foreach (var s in typedetails)
                                {
                                    s.EmpStatus = ESOBJ.EmpStatus;
                                    db.JobStatus.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                            else
                            {
                                var Dtls = db.JobStatus.Include(e => e.EmpStatus).Where(x => x.Id == data).ToList();
                                foreach (var s in Dtls)
                                {
                                    s.EmpStatus = null;
                                    db.JobStatus.Attach(s);
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
                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        JobStatus blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.JobStatus.Where(e => e.Id == data)
                                                                    .Include(e => e.EmpActingStatus)
                                                                    .Include(e => e.EmpStatus)
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

                                        //    int a = EditS(EmpStatus, EmpActingStatus, data, ESOBJ, ESOBJ.DBTrack);

                                        var CurOBJ = db.JobStatus.Find(data);
                                        TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                                        db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                        {
                                            // ESOBJ.DBTrack = dbT;
                                            JobStatus ESIOBJ = new JobStatus()
                                            {
                                                Id = data,
                                                DBTrack = ESOBJ.DBTrack,
                                                EmpActingStatus_Id = ESOBJ.EmpActingStatus_Id,
                                                EmpStatus_Id = ESOBJ.EmpStatus_Id
                                            };

                                            db.JobStatus.Attach(ESIOBJ);
                                            db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                                            db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                            //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                            //  return 1;
                                        }

                                        using (var context = new DataBaseContext())
                                        {
                                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                            DT_JobStatus DT_OBJ = (DT_JobStatus)obj;
                                            DT_OBJ.EmpActingStatus_Id = blog.EmpActingStatus == null ? 0 : blog.EmpActingStatus.Id;
                                            DT_OBJ.EmpStatus_Id = blog.EmpStatus == null ? 0 : blog.EmpStatus.Id;
                                            db.Create(DT_OBJ);
                                            db.SaveChanges();
                                        }
                                        await db.SaveChangesAsync();
                                        ts.Complete();

                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.EmpStatus.LookupVal, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        //return Json(new Object[] { ESOBJ.Id, ESOBJ.EmpStatus.LookupVal, "Record Updated", JsonRequestBehavior.AllowGet });

                                    }
                                }

                                //catch (DbUpdateException e) { throw e; }
                                //catch (DataException e) { throw e; }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (JobStatus)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (JobStatus)databaseEntry.ToObject();
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
                                //var errorMsg = sb.ToString();
                                //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                                var errorMsg = sb.ToString();
                                Msg.Add(errorMsg);
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }

                        else
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {

                                JobStatus blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;
                                JobStatus Old_Obj = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.JobStatus.Where(e => e.Id == data).SingleOrDefault();
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
                                JobStatus corp = new JobStatus()
                                {
                                    Id = data,
                                    DBTrack = ESOBJ.DBTrack,
                                    RowVersion = (Byte[])TempData["RowVersion"]
                                };


                                //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                using (var context = new DataBaseContext())
                                {
                                    var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "JobStatus", ESOBJ.DBTrack);
                                    // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                    Old_Obj = context.JobStatus.Where(e => e.Id == data).Include(e => e.EmpActingStatus)
                                        .Include(e => e.EmpStatus).SingleOrDefault();
                                    DT_JobStatus DT_Corp = (DT_JobStatus)obj;
                                    DT_Corp.EmpActingStatus_Id = DBTrackFile.ValCompare(Old_Obj.EmpActingStatus, ESOBJ.EmpActingStatus);//Old_Obj.Address == c.Address ? 0 : Old_Obj.Address == null && c.Address != null ? c.Address.Id : Old_Obj.Address.Id;
                                    DT_Corp.EmpStatus_Id = DBTrackFile.ValCompare(Old_Obj.EmpStatus, ESOBJ.EmpStatus); //Old_Obj.BusinessType == c.BusinessType ? 0 : Old_Obj.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Obj.BusinessType.Id;                        
                                    db.Create(DT_Corp);
                                    //db.SaveChanges();
                                }
                                blog.DBTrack = ESOBJ.DBTrack;
                                db.JobStatus.Attach(blog);
                                db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = ESOBJ.EmpStatus.LookupVal, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return Json(new Object[] { blog.Id, ESOBJ.EmpStatus.LookupVal, "Record Updated", JsonRequestBehavior.AllowGet });
                            }

                        }
                    }
                    else
                    {
                        var errorMsg = "EmpActingStatus , EmpStatus Can't Be Empty....";
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
                return View();
            }
        }



        public int EditS(string RMVal, string PerkHVal, int data, JobStatus ESOBJ, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (RMVal != null)
                {
                    if (RMVal != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(RMVal));
                        ESOBJ.EmpStatus = val;

                        var type = db.JobStatus.Include(e => e.EmpStatus)
                            .Where(e => e.Id == data).SingleOrDefault();
                        IList<JobStatus> typedetails = null;
                        if (type.EmpStatus != null)
                        {
                            typedetails = db.JobStatus.Where(x => x.EmpStatus.Id == type.EmpStatus.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.JobStatus.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.EmpStatus = ESOBJ.EmpStatus;
                            db.JobStatus.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.JobStatus.Include(e => e.EmpStatus).Where(x => x.Id == data).ToList();
                        foreach (var s in Dtls)
                        {
                            s.EmpStatus = null;
                            db.JobStatus.Attach(s);
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
                    var Dtls = db.JobStatus.Include(e => e.EmpStatus).Where(x => x.Id == data).ToList();
                    foreach (var s in Dtls)
                    {
                        s.EmpStatus = null;
                        db.JobStatus.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }



                if (PerkHVal != null)
                {
                    if (PerkHVal != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(PerkHVal));
                        ESOBJ.EmpActingStatus = val;

                        var type = db.JobStatus
                            .Include(e => e.EmpActingStatus)
                            .Where(e => e.Id == data).SingleOrDefault();
                        IList<JobStatus> typedetails = null;
                        if (type.EmpActingStatus != null)
                        {
                            typedetails = db.JobStatus.Where(x => x.EmpActingStatus.Id == type.EmpActingStatus.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.JobStatus.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.EmpActingStatus = ESOBJ.EmpActingStatus;
                            db.JobStatus.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.JobStatus.Include(e => e.EmpActingStatus).Where(x => x.Id == data).ToList();
                        foreach (var s in Dtls)
                        {
                            s.EmpActingStatus = null;
                            db.JobStatus.Attach(s);
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
                    var Dtls = db.JobStatus.Include(e => e.EmpActingStatus).Where(x => x.Id == data).ToList();
                    foreach (var s in Dtls)
                    {
                        s.EmpActingStatus = null;
                        db.JobStatus.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                var CurOBJ = db.JobStatus.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    ESOBJ.DBTrack = dbT;
                    JobStatus ESIOBJ = new JobStatus()
                    {
                        Id = data,
                        DBTrack = ESOBJ.DBTrack
                    };

                    db.JobStatus.Attach(ESIOBJ);
                    db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }



        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Delete(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                JobStatus JobStatus = db.JobStatus.Where(e => e.Id == data).SingleOrDefault();
                try
                {
                    //var selectedValues = JobStatus.SocialActivities;
                    //var lkValue = new HashSet<int>(JobStatus.SocialActivities.Select(e => e.Id));
                    //if (lkValue.Count > 0)
                    //{
                    //    return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                    //}

                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(JobStatus).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                }

                catch (DataException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new Object[] { "", "Unable to delete. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });

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


        public class P2BGridData
        {
            public int Id { get; set; }
            public string Empstatus { get; set; }
            public string EmpsActingstatus { get; set; }
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

        //        var BindCompList = db.Company.Include(e => e.JobStatus.Select(z => z.EmpStatus)).Include(e => e.JobStatus.Select(f => f.EmpActingStatus)).Where(e => e.Id == company_Id).ToList();

        //        foreach (var z in BindCompList)
        //        {
        //            if (z.JobStatus != null && z.JobStatus.Count > 0)
        //            {

        //                foreach (var s in z.JobStatus)
        //                {
        //                    //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
        //                    view = new P2BGridData()
        //                    {
        //                        Id = s.Id,
        //                        Empstatus = s.EmpStatus != null ? s.EmpStatus.LookupVal : null,
        //                        EmpsActingstatus = s.EmpActingStatus != null ? s.EmpActingStatus.LookupVal : null

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
        //                    jsonData = IE.Select(a => new { a.Id, a.Empstatus }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                if (gp.searchField == "FullDetails")
        //                    jsonData = IE.Select(a => new { a.Id, a.Empstatus }).Where((e => (e.Empstatus.ToString().Contains(gp.searchString)))).ToList();

        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Empstatus != null ? Convert.ToString(a.Empstatus) : "", a.Empstatus != null ? Convert.ToString(a.EmpsActingstatus) : "" }).ToList();
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
        //                                 gp.sidx == "Empstatus" ? c.Empstatus.ToString() : ""

        //                                );
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.Empstatus != null ? Convert.ToString(a.Empstatus) : "", a.Empstatus != null ? Convert.ToString(a.EmpsActingstatus) : "" }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.Empstatus != null ? Convert.ToString(a.Empstatus) : "", a.Empstatus != null ? Convert.ToString(a.EmpsActingstatus) : "" }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Empstatus != null ? Convert.ToString(a.Empstatus) : "", a.Empstatus != null ? Convert.ToString(a.EmpsActingstatus) : "" }).ToList();
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
        //        var LKVal = db.JobStatus.ToList();

        //        if (gp.IsAutho == true)
        //        {
        //            LKVal = db.JobStatus.Include(e => e.EmpActingStatus).Include(e => e.EmpStatus).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            LKVal = db.JobStatus.Include(e => e.EmpActingStatus).Include(e => e.EmpStatus).AsNoTracking().ToList();
        //        }


        //        IEnumerable<JobStatus> IE;
        //        if (!string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = LKVal;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.EmpActingStatus, a.EmpStatus }).Where((e => (e.Id.ToString() == gp.searchString) || (e.EmpActingStatus.LookupVal.ToLower() == gp.searchString) || (e.EmpStatus.LookupVal.ToLower() == gp.searchString.ToLower())));
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EmpActingStatus.LookupVal, a.EmpStatus.LookupVal }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = LKVal;
        //            Func<JobStatus, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c =>
        //                                 gp.sidx == "EmpActingStatus" ? c.EmpActingStatus.LookupVal.ToString() :
        //                                 gp.sidx == "EmpStatus" ? c.EmpStatus.LookupVal.ToString() :

        //                                 "");
        //            }

        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.EmpActingStatus.LookupVal, a.EmpStatus.LookupVal }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.EmpActingStatus.LookupVal, a.EmpStatus.LookupVal }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EmpActingStatus.LookupVal, a.EmpStatus.LookupVal }).ToList();
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
                IEnumerable<JobStatus> JobStatus = null;
                if (gp.IsAutho == true)
                {
                    JobStatus = db.JobStatus.Include(e => e.EmpStatus).Include(e => e.EmpActingStatus).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    JobStatus = db.JobStatus.Include(e => e.EmpStatus).Include(e => e.EmpActingStatus).AsNoTracking().ToList();
                }

                IEnumerable<JobStatus> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = JobStatus;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmpStatus.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                          || (e.EmpActingStatus.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                          || (e.Id.ToString().Contains(gp.searchString.ToUpper()))
                          ).Select(a => new Object[] { a.EmpStatus.LookupVal, a.EmpActingStatus.LookupVal, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpStatus != null ? Convert.ToString(a.EmpStatus.LookupVal) : "", a.EmpActingStatus != null ? Convert.ToString(a.EmpActingStatus.LookupVal) : "", a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = JobStatus;
                    Func<JobStatus, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpStatus" ? c.EmpStatus.LookupVal :
                                         gp.sidx == "EmpActingStatus" ? c.EmpActingStatus.LookupVal : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpStatus != null ? Convert.ToString(a.EmpStatus.LookupVal) : "", a.EmpActingStatus != null ? Convert.ToString(a.EmpActingStatus.LookupVal) : "", a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpStatus != null ? Convert.ToString(a.EmpStatus.LookupVal) : "", a.EmpActingStatus != null ? Convert.ToString(a.EmpActingStatus.LookupVal) : "", a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpStatus != null ? Convert.ToString(a.EmpStatus.LookupVal) : "", a.EmpActingStatus != null ? Convert.ToString(a.EmpActingStatus.LookupVal) : "", a.Id }).ToList();
                    }
                    totalRecords = JobStatus.Count();
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
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            JobStatus ESI = db.JobStatus
                                .Include(e => e.EmpStatus)
                                .Include(e => e.EmpActingStatus)
                                .FirstOrDefault(e => e.Id == auth_id);

                            ESI.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = ESI.DBTrack.ModifiedBy != null ? ESI.DBTrack.ModifiedBy : null,
                                CreatedBy = ESI.DBTrack.CreatedBy != null ? ESI.DBTrack.CreatedBy : null,
                                CreatedOn = ESI.DBTrack.CreatedOn != null ? ESI.DBTrack.CreatedOn : null,
                                IsModified = ESI.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.JobStatus.Attach(ESI);
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(ESI).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, ESI.DBTrack);
                            DT_JobStatus DT_OBJ = (DT_JobStatus)rtn_Obj;

                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = ESI.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { ESI.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        JobStatus Old_OBJ = db.JobStatus
                                                .Include(e => e.EmpStatus)
                                                .Include(e => e.EmpActingStatus)
                                                .Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_JobStatus Curr_OBJ = db.DT_JobStatus
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_OBJ != null)
                        {
                            JobStatus JobStatus = new JobStatus();
                            string EmpStatus = Curr_OBJ.EmpStatus_Id == null ? null : Curr_OBJ.EmpStatus_Id.ToString();
                            string EmpActingStatus = Curr_OBJ.EmpActingStatus_Id == null ? null : Curr_OBJ.EmpActingStatus_Id.ToString();


                            //      corp.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        JobStatus.DBTrack = new DBTrack
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

                                        int a = EditS(EmpStatus, EmpActingStatus, auth_id, JobStatus, JobStatus.DBTrack);

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = JobStatus.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { JobStatus.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (JobStatus)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (JobStatus)databaseEntry.ToObject();
                                        JobStatus.RowVersion = databaseValues.RowVersion;
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
                        //return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                        {

                        }
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //JobStatus corp = db.JobStatus.Find(auth_id);
                            JobStatus ESI = db.JobStatus.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

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
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.JobStatus.Attach(ESI);
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, ESI.DBTrack);
                            DT_JobStatus DT_OBJ = (DT_JobStatus)rtn_Obj;
                            //DT_OBJ.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            //DT_OBJ.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                            //DT_OBJ.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
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
    }
}
