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
using Attendance;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Attendance.MainController
{
    public class RemarkConfigController : Controller
    {
      //  private DataBaseContext db = new DataBaseContext();
        //
        // GET: /RemarkConfig/

        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/RemarkConfig/Index.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(RemarkConfig COBJ, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var MusterRemarks = form["MusterRemarkslist"] == "0" ? "" : form["MusterRemarkslist"];
                    var AlterMusterRemark = form["AlterMusterRemarklist"] == "0" ? "" : form["AlterMusterRemarklist"];
                    var PresentStatus = form["PresentStatuslist"] == "0" ? "" : form["PresentStatuslist"];

                    var IsAppl = form["IsAppl"] == "0" ? "" : form["IsAppl"];
                    var IsODAppl = form["IsODAppl"] == "0" ? "" : form["IsODAppl"];
                    var IsODTEAppl = form["IsODTEAppl"] == "0" ? "" : form["IsODTEAppl"];
                    COBJ.IsAppl = Convert.ToBoolean(IsAppl);
                    COBJ.IsODAppl = Convert.ToBoolean(IsODAppl);
                    COBJ.IsODTimeEntryAppl = Convert.ToBoolean(IsODTEAppl);
                    //CompanyAttendance comp;
                    // List<RemarkConfig> remconf = new List<RemarkConfig>();
                    // remconf.Add(COBJ);
                    //comp.RemarkConfig = remconf;
                    //db.CompanyAttendance.Add(remconf);

                    if (MusterRemarks != null && MusterRemarks != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "611").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(MusterRemarks)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(MusterRemarks));
                            COBJ.MusterRemarks = val; 
                    }

                    if (AlterMusterRemark != null && AlterMusterRemark != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "611").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(AlterMusterRemark)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(AlterMusterRemark));
                            COBJ.AlterMusterRemark = val; 
                    }

                    if (PresentStatus != null && PresentStatus != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1012").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(PresentStatus)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(PresentStatus));
                            COBJ.PresentStatus = val; 
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            //if (db.RemarkConfig.Any(o => o.IsODAppl == COBJ.IsODAppl) && db.RemarkConfig.Any(o => o.IsAppl == COBJ.IsAppl))
                            //{
                            //    return this.Json(new Object[] { null, null, "Record already exists.", JsonRequestBehavior.AllowGet });
                            //}

                            COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            RemarkConfig RemarkConfig = new RemarkConfig()
                            {

                                IsAppl = COBJ.IsAppl,
                                IsODAppl = COBJ.IsODAppl,
                                IsODTimeEntryAppl = COBJ.IsODTimeEntryAppl,
                                AlterMusterRemark = COBJ.AlterMusterRemark,
                                RemarkDesc = COBJ.RemarkDesc,
                                MusterRemarks = COBJ.MusterRemarks,
                                PresentStatus = COBJ.PresentStatus,
                                DBTrack = COBJ.DBTrack
                            };
                            try
                            {
                                db.RemarkConfig.Add(RemarkConfig);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, COBJ.DBTrack);
                                DT_RemarkConfig DT_OBJ = (DT_RemarkConfig)rtn_Obj;
                                db.Create(DT_OBJ);
                                var id = Convert.ToInt32(SessionManager.CompanyId);
                                var CompanyAtt = db.CompanyAttendance.Where(e => e.Company.Id == id).SingleOrDefault();

                                CompanyAtt.RemarkConfig = RemarkConfig;
                                db.Entry(CompanyAtt).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                                ts.Complete();
                                // return this.Json(new Object[] { "", "", "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Created successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = COBJ.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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
        [HttpPost]
        public ActionResult Edit(int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {

                var RemarkConfig = db.RemarkConfig.Include(a => a.AlterMusterRemark).Include(a => a.MusterRemarks).Include(a => a.PresentStatus)

                                    .Where(e => e.Id == data).ToList();
                var r = (from ca in RemarkConfig
                         select new
                         {


                             //Altermusterremark=ca.AlterMusterRemark.Id,
                             //    MusterRemark=ca.MusterRemarks.Id!= null ? ca.MusterRemarks.Id:0,
                             //  MusterRemark = ca.MusterRemarks.Id,
                             //  PresentStatus=ca.PresentStatus.Id,
                             //    Altermusterremark = ca.Altermusterremark.Id == null ? 0 : ca.Altermusterremark.Id,

                             Id = ca.Id,
                             ApplicableRemark = ca.IsAppl,
                             ODApplicable = ca.IsODAppl,
                             IsODTimeEntryAppl = ca.IsODTimeEntryAppl,
                             Altermusterremark = ca.AlterMusterRemark.Id != null ? ca.AlterMusterRemark.Id : 0,
                             MusterRemark = ca.MusterRemarks.Id != null ? ca.MusterRemarks.Id : 0,
                             PresentStatus = ca.PresentStatus.Id != null ? ca.PresentStatus.Id : 0,

                             // MusterRemark = ca.MusterRemarks.Id ,
                             //  Altermusterremark = ca.AlterMusterRemark.Id,
                             // PresentStatus = ca.PresentStatus.Id,
                             RemarkDesc = ca.RemarkDesc == null ? "" : ca.RemarkDesc,

                             //Name = ca.Name == null ? "" : ca.Name,
                             //RemarkId = ca.AlterMusterRemark == null ? "" : ca.AlterMusterRemark,
                             //RemarkDesc = ca.RemarkDesc == null ? "" : ca.RemarkDesc,
                             //SelectedRemark = ca.SelectedRemark == null ? "" : ca.SelectedRemark,

                             // Name = ca.Name == null ? "" : ca.Name,
                             //  RemarkId = ca.AlterMusterRemark,

                             // SelectedRemark = ca.MusterRemarks,
                             Action = ca.DBTrack.Action
                         }).ToList().Distinct();



                var W = db.DT_RemarkConfig
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         ApplicableRemark = e.ApplicableRemark,
                         // ApplicableRemark = e.IsAppl == null ? "" : e.IsAppl,
                         //  ApplicableRemark = e.IsAppl == null ? false : e.IsAppl,
                         // Name = e.Name,
                         RemarkId = e.RemarkId,
                         RemarkDesc = e.RemarkDesc,
                         SelectedRemark = e.SelectedRemark,

                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var LKup = db.RemarkConfig.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, "", W, Auth, JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(RemarkConfig ESOBJ, FormCollection form, int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    var MusterRemarks = form["MusterRemarkslist"] == "0" ? "" : form["MusterRemarkslist"];
                    var AlterMusterRemark = form["AlterMusterRemarklist"] == "0" ? "" : form["AlterMusterRemarklist"];
                    var PresentStatus = form["PresentStatuslist"] == "0" ? "" : form["PresentStatuslist"];
                    var IsODTEAppl = form["IsODTEAppl"] == "0" ? "" : form["IsODTEAppl"];

                    ESOBJ.IsODTimeEntryAppl = Convert.ToBoolean(IsODTEAppl);

                    ESOBJ.MusterRemarks_Id = MusterRemarks != null && MusterRemarks != "" ? int.Parse(MusterRemarks) : 0;
                    ESOBJ.AlterMusterRemark_Id = AlterMusterRemark != null && AlterMusterRemark != "" ? int.Parse(AlterMusterRemark) : 0;
                    ESOBJ.PresentStatus_Id = PresentStatus != null && PresentStatus != "" ? int.Parse(PresentStatus) : 0; 

                   
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                   
                                    var CurOBJ = db.RemarkConfig.Find(data);
                                    TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        ESOBJ.DBTrack = new DBTrack
                                        {
                                            CreatedBy = CurOBJ.DBTrack.CreatedBy == null ? null : CurOBJ.DBTrack.CreatedBy,
                                            CreatedOn = CurOBJ.DBTrack.CreatedOn == null ? null : CurOBJ.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };


                                        CurOBJ.Id = data;
                                        CurOBJ.IsAppl = ESOBJ.IsAppl;
                                        CurOBJ.IsODAppl = ESOBJ.IsODAppl;
                                        CurOBJ.IsODTimeEntryAppl = ESOBJ.IsODTimeEntryAppl;
                                        CurOBJ.RemarkDesc = ESOBJ.RemarkDesc;
                                        CurOBJ.MusterRemarks_Id = ESOBJ.MusterRemarks_Id;
                                        CurOBJ.PresentStatus_Id = ESOBJ.PresentStatus_Id;
                                        CurOBJ.AlterMusterRemark_Id = ESOBJ.AlterMusterRemark_Id;
                                        db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Modified;

                                        RemarkConfig blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;


                                        blog = db.RemarkConfig.Where(e => e.Id == data)
                                                                .SingleOrDefault();
                                        originalBlogValues = db.Entry(blog).OriginalValues;

                                        var obj = DBTrackFile.DBTrackSave("Attendance/Attendance", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                        DT_RemarkConfig DT_OBJ = (DT_RemarkConfig)obj;
                                        db.Create(DT_OBJ);
                                        db.SaveChanges();
                                    }

                                
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.IsAppl.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    // return Json(new Object[] { ESOBJ.Id, ESOBJ.IsAppl, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }

                            //catch (DbUpdateException e) { throw e; }
                            //catch (DataException e) { throw e; }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (RemarkConfig)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    //   return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var databaseValues = (RemarkConfig)databaseEntry.ToObject();
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
                            return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        }
                    }


                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            RemarkConfig blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            RemarkConfig Old_Obj = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.RemarkConfig.Where(e => e.Id == data).SingleOrDefault();
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
                            RemarkConfig corp = new RemarkConfig()
                            {
                                //IsAppl = ESOBJ.IsAppl,
                                //Name = ESOBJ.Name,
                                //RemarkId = ESOBJ.RemarkId,
                                //RemarkDesc = ESOBJ.RemarkDesc,
                                //SelectedRemark = ESOBJ.SelectedRemark,
                                //Id = data,
                                //DBTrack = ESOBJ.DBTrack,
                                //RowVersion = (Byte[])TempData["RowVersion"]
                                IsAppl = ESOBJ.IsAppl,
                                //    Name = ESOBJ.Name,
                                IsODAppl = ESOBJ.IsODAppl,
                                RemarkDesc = ESOBJ.RemarkDesc,
                                MusterRemarks = ESOBJ.MusterRemarks,
                                Id = data,
                                DBTrack = ESOBJ.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Attendance/Attendance", "M", blog, corp, "RemarkConfig", ESOBJ.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Obj = context.RemarkConfig.Where(e => e.Id == data)
                                    .SingleOrDefault();
                                DT_RemarkConfig DT_Corp = (DT_RemarkConfig)obj;
                                db.Create(DT_Corp);
                                db.SaveChanges();
                            }
                            blog.DBTrack = ESOBJ.DBTrack;
                            db.RemarkConfig.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            //  return Json(new Object[] { blog.Id, ESOBJ.IsAppl, "Record Updated", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.IsAppl.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
        public int EditS(string MusterRemarks, string AlterMusterRemark, string PresentStatus, int data, RemarkConfig ESOBJ, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (MusterRemarks != null)
                {
                    if (MusterRemarks != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(MusterRemarks));
                        ESOBJ.MusterRemarks = val;

                        var type = db.RemarkConfig.Include(e => e.MusterRemarks).Where(e => e.Id == data).SingleOrDefault();
                        IList<RemarkConfig> typedetails = null;
                        if (type.MusterRemarks != null)
                        {
                            typedetails = db.RemarkConfig.Where(x => x.MusterRemarks.Id == type.MusterRemarks.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.RemarkConfig.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.MusterRemarks = ESOBJ.MusterRemarks;
                            db.RemarkConfig.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.RemarkConfig.Include(e => e.MusterRemarks).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.MusterRemarks = null;
                            db.RemarkConfig.Attach(s);
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
                    var BusiTypeDetails = db.RemarkConfig.Include(e => e.MusterRemarks).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.MusterRemarks = null;
                        db.RemarkConfig.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (AlterMusterRemark != null)
                {
                    if (AlterMusterRemark != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(AlterMusterRemark));
                        ESOBJ.AlterMusterRemark = val;

                        var type = db.RemarkConfig.Include(e => e.AlterMusterRemark).Where(e => e.Id == data).SingleOrDefault();
                        IList<RemarkConfig> typedetails = null;
                        if (type.AlterMusterRemark != null)
                        {
                            typedetails = db.RemarkConfig.Where(x => x.AlterMusterRemark.Id == type.AlterMusterRemark.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.RemarkConfig.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.AlterMusterRemark = ESOBJ.AlterMusterRemark;
                            db.RemarkConfig.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.RemarkConfig.Include(e => e.AlterMusterRemark).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.AlterMusterRemark = null;
                            db.RemarkConfig.Attach(s);
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
                    var BusiTypeDetails = db.RemarkConfig.Include(e => e.AlterMusterRemark).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.AlterMusterRemark = null;
                        db.RemarkConfig.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                if (PresentStatus != null)
                {
                    if (PresentStatus != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(PresentStatus));
                        ESOBJ.PresentStatus = val;

                        var type = db.RemarkConfig.Include(e => e.PresentStatus).Where(e => e.Id == data).SingleOrDefault();
                        IList<RemarkConfig> typedetails = null;
                        if (type.PresentStatus != null)
                        {
                            typedetails = db.RemarkConfig.Where(x => x.PresentStatus.Id == type.PresentStatus.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.RemarkConfig.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.PresentStatus = ESOBJ.PresentStatus;
                            db.RemarkConfig.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.RemarkConfig.Include(e => e.PresentStatus).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.PresentStatus = null;
                            db.RemarkConfig.Attach(s);
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
                    var BusiTypeDetails = db.RemarkConfig.Include(e => e.PresentStatus).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.PresentStatus = null;
                        db.RemarkConfig.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }



                var CurOBJ = db.RemarkConfig.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    ESOBJ.DBTrack = dbT;
                    RemarkConfig ESIOBJ = new RemarkConfig()
                    {
                        Id = data,
                        //ApplicableRemark = ESOBJ.ApplicableRemark,
                        //Name = ESOBJ.Name,
                        // RemarkId = ESOBJ.RemarkId,
                        // RemarkDesc = ESOBJ.RemarkDesc,
                        // SelectedRemark = ESOBJ.SelectedRemark,

                        IsAppl = ESOBJ.IsAppl,
                        //    Name = ESOBJ.Name,
                        IsODAppl = ESOBJ.IsODAppl,
                        RemarkDesc = ESOBJ.RemarkDesc,
                        MusterRemarks = ESOBJ.MusterRemarks,

                        DBTrack = ESOBJ.DBTrack
                    };

                    db.RemarkConfig.Attach(ESIOBJ);
                    db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }

        //  [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Delete(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    RemarkConfig RemarkConfig = db.RemarkConfig.Where(e => e.Id == data).SingleOrDefault();
                    try
                    {
                        //var selectedValues = RemarkConfig.SocialActivities;
                        //var lkValue = new HashSet<int>(RemarkConfig.SocialActivities.Select(e => e.Id));
                        //if (lkValue.Count > 0)
                        //{
                        //    return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                        //}

                        using (TransactionScope ts = new TransactionScope())
                        {
                            db.Entry(RemarkConfig).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            ts.Complete();
                        }
                        // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        Msg.Add("  Data removed .  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    catch (DataException /* dex */)
                    {
                        // return this.Json(new Object[] { "", "Unable to delete. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
                        Msg.Add(" Child record exists.Cannot delete.");
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
                var LKVal = db.RemarkConfig.ToList();

                if (gp.IsAutho == true)
                {
                    LKVal = db.RemarkConfig.AsNoTracking().Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    LKVal = db.RemarkConfig.Include(e => e.AlterMusterRemark).Include(e => e.MusterRemarks).Include(e => e.PresentStatus).AsNoTracking().ToList();
                }


                IEnumerable<RemarkConfig> IE;
                if (!string.IsNullOrEmpty(gp.searchString))
                {
                    IE = LKVal;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.IsAppl.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.IsODAppl.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.AlterMusterRemark != null ? e.AlterMusterRemark.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                               || (e.RemarkDesc.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.MusterRemarks != null ? e.MusterRemarks.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                               || (e.PresentStatus != null ? e.PresentStatus.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.IsAppl, a.IsODAppl, a.AlterMusterRemark != null ? a.AlterMusterRemark.LookupVal : "",  a.RemarkDesc, a.MusterRemarks != null ? a.MusterRemarks.LookupVal : "", a.PresentStatus != null ? a.PresentStatus.LookupVal : "", a.Id }).ToList();


                        //jsonData = IE.Select(a => new { a.Id }).Where((e => (e.Id.ToString() == gp.searchString)));
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.IsAppl, a.IsODAppl, a.AlterMusterRemark != null ? Convert.ToString(a.AlterMusterRemark.LookupVal) : "", a.RemarkDesc, a.MusterRemarks != null ? Convert.ToString(a.MusterRemarks.LookupVal) : "", a.PresentStatus != null ? Convert.ToString(a.PresentStatus.LookupVal) : "", a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = LKVal;
                    Func<RemarkConfig, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "ApplicableRemark" ? c.IsAppl.ToString() :
                                         gp.sidx == "ODApplicable" ? c.IsODAppl.ToString() :
                                         gp.sidx == "RemarkId" ? c.AlterMusterRemark.LookupVal :
                                         gp.sidx == "RemarkDesc" ? c.RemarkDesc.ToString() :
                                         gp.sidx == "MusterRemarks" ? c.MusterRemarks.LookupVal :
                                         gp.sidx == "PresentStatus" ? c.PresentStatus.LookupVal :
                                         "");
                    }

                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.IsAppl, a.IsODAppl, a.AlterMusterRemark != null ? Convert.ToString(a.AlterMusterRemark.LookupVal) : "", a.RemarkDesc, a.MusterRemarks != null ? Convert.ToString(a.MusterRemarks.LookupVal) : "", a.PresentStatus != null ? Convert.ToString(a.PresentStatus.LookupVal) : "", a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.IsAppl, a.IsODAppl, a.AlterMusterRemark != null ? Convert.ToString(a.AlterMusterRemark.LookupVal) : "", a.RemarkDesc, a.MusterRemarks != null ? Convert.ToString(a.MusterRemarks.LookupVal) : "", a.PresentStatus != null ? Convert.ToString(a.PresentStatus.LookupVal) : "", a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.IsAppl, a.IsODAppl, a.AlterMusterRemark != null ? Convert.ToString(a.AlterMusterRemark.LookupVal) : "", a.RemarkDesc, a.MusterRemarks != null ? Convert.ToString(a.MusterRemarks.LookupVal) : "", a.PresentStatus != null ? Convert.ToString(a.PresentStatus.LookupVal) : "", a.Id }).ToList();
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

        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            RemarkConfig ESI = db.RemarkConfig
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

                            db.RemarkConfig.Attach(ESI);
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(ESI).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, ESI.DBTrack);
                            DT_RemarkConfig DT_OBJ = (DT_RemarkConfig)rtn_Obj;

                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            // return Json(new Object[] { ESI.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                        }
                    }
                    else if (auth_action == "M")
                    {

                        RemarkConfig Old_OBJ = db.RemarkConfig
                                                .Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_RemarkConfig Curr_OBJ = db.DT_RemarkConfig
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_OBJ != null)
                        {
                            RemarkConfig RemarkConfig = new RemarkConfig();

                            //  RemarkConfig.ApplicableRemark = Curr_OBJ.ApplicableRemark == null ? Old_OBJ.ApplicableRemark : Curr_OBJ.ApplicableRemark;
                            //     RemarkConfig.Name = Curr_OBJ.Name == null ? Old_OBJ.Name : Curr_OBJ.Name;
                            //   RemarkConfig.RemarkId = Curr_OBJ.RemarkId == null? Old_OBJ.RemarkId : Curr_OBJ.RemarkId;
                            //  RemarkConfig.RemarkDesc = Curr_OBJ.RemarkDesc ==null ? Old_OBJ.RemarkDesc : Curr_OBJ.RemarkDesc;
                            // RemarkConfig.SelectedRemark = Curr_OBJ.SelectedRemark ==null ? Old_OBJ.SelectedRemark : Curr_OBJ.SelectedRemark;

                            //     RemarkConfig.IsAppl = Curr_OBJ.ApplicableRemark == null ? Old_OBJ.IsAppl : Curr_OBJ.ApplicableRemark;
                            //  RemarkConfig.Name = Curr_OBJ.Name == null ? Old_OBJ.Name : Curr_OBJ.Name;
                            //   RemarkConfig.RemarkId = Curr_OBJ.RemarkId == null? Old_OBJ.RemarkId : Curr_OBJ.RemarkId;
                            //     RemarkConfig.RemarkDesc = Curr_OBJ.RemarkDesc ==null ? Old_OBJ.RemarkDesc : Curr_OBJ.RemarkDesc;
                            //    RemarkConfig.SelectedRemark = Curr_OBJ.SelectedRemark ==null ? Old_OBJ.SelectedRemark : Curr_OBJ.SelectedRemark;

                            //      corp.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        RemarkConfig.DBTrack = new DBTrack
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

                                        //   int a = EditS(auth_id, RemarkConfig, RemarkConfig.DBTrack);

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        //  return Json(new Object[] { RemarkConfig.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                        Msg.Add(" Record Authorised ");
                                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (RemarkConfig)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                    else
                                    {
                                        var databaseValues = (RemarkConfig)databaseEntry.ToObject();
                                        RemarkConfig.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                            Msg.Add("  Data removed from history.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //  return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //RemarkConfig corp = db.RemarkConfig.Find(auth_id);
                            RemarkConfig ESI = db.RemarkConfig.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

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

                            db.RemarkConfig.Attach(ESI);
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, ESI.DBTrack);
                            DT_RemarkConfig DT_OBJ = (DT_RemarkConfig)rtn_Obj;
                            //DT_OBJ.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            //DT_OBJ.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                            //DT_OBJ.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            // return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
    }
}