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
    public class AccessRightsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/_AccessRights.cshtml");
        }
        public ActionResult Create(AccessRights COBJ, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string ActionName = form["ActionName-drop"] == "0" ? "" : form["ActionName-drop"];
                    string LvNoOfDaysFrom = form["LvNoOfDaysFrom"] == "0" ? "0" : form["LvNoOfDaysFrom"];
                    string LvNoOfDaysTo = form["LvNoOfDaysTo"] == "0" ? "0" : form["LvNoOfDaysTo"];

                    int comp_Id = Convert.ToInt32(Session["CompID"]);
                    var Company = new Company();
                    Company = db.Company.Where(e => e.Id == comp_Id).SingleOrDefault();
                    if (ActionName != null && ActionName != "")
                    {
                        if (ActionName != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "603").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(ActionName)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(ActionName));
                            COBJ.ActionName = val;
                        }
                        COBJ.LvNoOfDaysFrom = int.Parse(LvNoOfDaysFrom);
                        COBJ.LvNoOfDaysTo = int.Parse(LvNoOfDaysTo);

                        if (ModelState.IsValid)
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {

                                COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                AccessRights AccessRights = new AccessRights()
                                {
                                    IsApproveRejectAppl = COBJ.IsApproveRejectAppl,
                                    IsClose = COBJ.IsClose,
                                    ActionName = COBJ.ActionName,
                                    IsComments = COBJ.IsComments,
                                    LvNoOfDaysFrom=COBJ.LvNoOfDaysFrom,
                                    LvNoOfDaysTo=COBJ.LvNoOfDaysTo,
                                    DBTrack = COBJ.DBTrack
                                };
                                try
                                {
                                    db.AccessRights.Add(AccessRights);
                                    //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, COBJ.DBTrack);
                                    //DT_JobStatus DT_OBJ = (DT_JobStatus)rtn_Obj;
                                    //DT_OBJ.EmpStatus_Id = COBJ.GeoFuncList == null ? 0 : COBJ.GeoFuncList.Id;
                                    //DT_OBJ.EmpActingStatus_Id = COBJ.ActionName == null ? 0 : COBJ.ActionName.Id;
                                    //db.Create(DT_OBJ);
                                    db.SaveChanges();
                                    ts.Complete();
                                    //return this.Json(new Object[] { AccessRights.Id, AccessRights.ActionName.LookupVal, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                                    Msg.Add("  Data Created successfully  ");
                                    var fulldetails = "ActionName :" + AccessRights.ActionName.LookupVal + ",IsApproveRejectAppl :" + AccessRights.IsApproveRejectAppl + ",IsClose :" + AccessRights.IsClose + ",IsComments :" + AccessRights.IsComments + "";
                                    return Json(new Utility.JsonReturnClass { Id = AccessRights.Id, Val = fulldetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }

                                catch (DbUpdateConcurrencyException)
                                {
                                    return RedirectToAction("Create", new { concurrencyError = true, id = COBJ.Id });
                                }
                                catch (DataException /* dex */)
                                {
                                    //   return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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

                            //var errorMsg = sb.ToString();
                            // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                            // return this.Json(new { msg = errorMsg });
                        }
                    }

                    else
                    {
                        Msg.Add("  ActionName , GeoFuncList Can't Be Empty....  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //var errorMsg = "ActionName , GeoFuncList Can't Be Empty....";
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });

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
        public ActionResult Edit(int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var AccessRights = db.AccessRights
                                    .Include(e => e.ActionName)
                    // .Include(e => e.GeoFuncList)
                                    .Where(e => e.Id == data).SingleOrDefault();

                var a = "";

                var W = db.DT_JobStatus
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         GeoFuncList = e.EmpStatus_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.EmpStatus_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),
                         ActionName = e.EmpActingStatus_Id == 0 ? "" : db.LookupValue
                                     .Where(x => x.Id == e.EmpActingStatus_Id)
                                     .Select(x => x.LookupVal).FirstOrDefault(),

                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var LKup = db.AccessRights.Find(data);
                ////TempData["RowVersion"] = LKup.RowVersion;
                //var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { AccessRights, a, W, "", JsonRequestBehavior.AllowGet });
            }
        }
        public async Task<ActionResult> EditSave(AccessRights ESOBJ, FormCollection form, int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    string GeoFuncList = form["GeoFuncList-drop"] == "0" ? "" : form["GeoFuncList-drop"];
                    string ActionName = form["ActionName-drop"] == "0" ? "" : form["ActionName-drop"];
                   
                    if (ActionName != null && ActionName != "")
                    {
                        if (ActionName != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "603").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(ActionName)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(ActionName));
                            ESOBJ.ActionName = val;
                        }
                        

                        if (Auth == false)
                        {
                            if (ModelState.IsValid)
                            {
                                try
                                {
                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        //AccessRights blog = null; // to retrieve old data
                                        //DbPropertyValues originalBlogValues = null;

                                        //using (var context = new DataBaseContext())
                                        //{
                                        //    blog = context.AccessRights.Where(e => e.Id == data)
                                        //                            .Include(e => e.ActionName)
                                        //                            .Include(e => e.GeoFuncList)
                                        //                            .SingleOrDefault();
                                        //    originalBlogValues = context.Entry(blog).OriginalValues;
                                        //}

                                        //ESOBJ.DBTrack = new DBTrack
                                        //{
                                        //    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        //    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        //    Action = "M",
                                        //    ModifiedBy = SessionManager.UserName,
                                        //    ModifiedOn = DateTime.Now
                                        //};

                                         int a = EditS(ActionName, data, ESOBJ);

                                         if (ActionName != null)
                                        {
                                            if (ActionName != "")
                                            {
                                                var val = db.LookupValue.Find(int.Parse(ActionName));
                                                ESOBJ.ActionName = val;

                                                var type = db.AccessRights
                                                    .Include(e => e.ActionName)
                                                    .Where(e => e.Id == data).SingleOrDefault();
                                                IList<AccessRights> typedetails = null;
                                                if (type.ActionName != null)
                                                {
                                                    typedetails = db.AccessRights.Where(x => x.ActionName.Id == type.ActionName.Id && x.Id == data).ToList();
                                                }
                                                else
                                                {
                                                    typedetails = db.AccessRights.Where(x => x.Id == data).ToList();
                                                }
                                                //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                                foreach (var s in typedetails)
                                                {
                                                    s.ActionName = ESOBJ.ActionName;
                                                    db.AccessRights.Attach(s);
                                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                    //await db.SaveChangesAsync();
                                                    db.SaveChanges();
                                                    //TempData["RowVersion"] = s.RowVersion;
                                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                                }
                                            }
                                            else
                                            {
                                                var Dtls = db.AccessRights.Include(e => e.ActionName).Where(x => x.Id == data).ToList();
                                                foreach (var s in Dtls)
                                                {
                                                    s.ActionName = null;
                                                    db.AccessRights.Attach(s);
                                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                    //await db.SaveChangesAsync();
                                                    db.SaveChanges();
                                                    //TempData["RowVersion"] = s.RowVersion;
                                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var Dtls = db.AccessRights.Include(e => e.ActionName).Where(x => x.Id == data).ToList();
                                            foreach (var s in Dtls)
                                            {
                                                s.ActionName = null;
                                                db.AccessRights.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                //TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        var db_data = db.AccessRights.Include(e => e.ActionName).Where(e => e.Id == data).SingleOrDefault();

                                        db_data.IsApproveRejectAppl = ESOBJ.IsApproveRejectAppl;
                                        db_data.IsClose = ESOBJ.IsClose;
                                        db_data.IsComments = ESOBJ.IsComments;
                                        db_data.LvNoOfDaysFrom = ESOBJ.LvNoOfDaysFrom;
                                        db_data.LvNoOfDaysTo = ESOBJ.LvNoOfDaysTo;
                                        db.AccessRights.Attach(db_data);
                                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                        using (var context = new DataBaseContext())
                                        {
                                            //var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                            //DT_JobStatus DT_OBJ = (DT_JobStatus)obj;
                                            //DT_OBJ.EmpActingStatus_Id = blog.ActionName == null ? 0 : blog.ActionName.Id;
                                            //DT_OBJ.EmpStatus_Id = blog.GeoFuncList == null ? 0 : blog.GeoFuncList.Id;
                                            //db.Create(DT_OBJ);
                                            //db.SaveChanges();
                                        }
                                        await db.SaveChangesAsync();
                                        ts.Complete();
                                        //   return Json(new Object[] { ESOBJ.Id, ESOBJ.ActionName.LookupVal, "Record Updated", JsonRequestBehavior.AllowGet });

                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.ActionName.LookupVal, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }

                                //catch (DbUpdateException e) { throw e; }
                                //catch (DataException e) { throw e; }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (AccessRights)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                    else
                                    {
                                        var databaseValues = (AccessRights)databaseEntry.ToObject();
                                        //ESOBJ.RowVersion = databaseValues.RowVersion;

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


                        else
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {


                                db.SaveChanges();
                                ts.Complete();
                                //    return Json(new Object[] { "", "", "Record Updated", JsonRequestBehavior.AllowGet });
                                Msg.Add(" Record Updated ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                        }
                    }
                    else
                    {
                        //var errorMsg = "ActionName , GeoFuncList Can't Be Empty....";
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        Msg.Add("  ActionName , GeoFuncList Can't Be Empty...  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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



        public int EditS(string PerkHVal, int data, AccessRights ESOBJ)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                if (PerkHVal != null)
                {
                    if (PerkHVal != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(PerkHVal));
                        ESOBJ.ActionName = val;

                        var type = db.AccessRights
                            .Include(e => e.ActionName)
                            .Where(e => e.Id == data).SingleOrDefault();
                        IList<AccessRights> typedetails = null;
                        if (type.ActionName != null)
                        {
                            typedetails = db.AccessRights.Where(x => x.ActionName.Id == type.ActionName.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.AccessRights.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.ActionName = ESOBJ.ActionName;
                            db.AccessRights.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            //TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.AccessRights.Include(e => e.ActionName).Where(x => x.Id == data).ToList();
                        foreach (var s in Dtls)
                        {
                            s.ActionName = null;
                            db.AccessRights.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            //TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var Dtls = db.AccessRights.Include(e => e.ActionName).Where(x => x.Id == data).ToList();
                    foreach (var s in Dtls)
                    {
                        s.ActionName = null;
                        db.AccessRights.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        //TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                var db_data = db.AccessRights.Include(e => e.ActionName).Where(e => e.Id == data).SingleOrDefault();

                db_data.IsApproveRejectAppl = ESOBJ.IsApproveRejectAppl;
                db_data.IsClose = ESOBJ.IsClose;
                db_data.IsComments = ESOBJ.IsComments;
                db.AccessRights.Attach(db_data);
                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                //var CurOBJ = db.AccessRights.Find(data);
                ////TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                //db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                //{
                //    ESOBJ.DBTrack = dbT;
                //    AccessRights ESIOBJ = new AccessRights()
                //    {
                //        Id = data,
                //        DBTrack = ESOBJ.DBTrack
                //    };

                //    db.AccessRights.Attach(ESIOBJ);
                //    db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                //    db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                //    //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                //    return 1;
                //}
                return 0;
            }
        }

        public ActionResult Delete(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    AccessRights AccessRights = db.AccessRights.Where(e => e.Id == data).SingleOrDefault();
                    try
                    {
                        //var selectedValues = AccessRights.SocialActivities;
                        //var lkValue = new HashSet<int>(AccessRights.SocialActivities.Select(e => e.Id));
                        //if (lkValue.Count > 0)
                        //{
                        //    return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                        //}

                        using (TransactionScope ts = new TransactionScope())
                        {
                            db.Entry(AccessRights).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            ts.Complete();
                        }

                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                    }

                    catch (DataException /* dex */)
                    {
                        // return this.Json(new Object[] { "", "Unable to delete. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
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


        //private DataBaseContext db = new DataBaseContext();

        public ActionResult GetLookup(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.AccessRights
                    .Include(e => e.ActionName)
                    //.Include(e => e.IsApproveRejectAppl)
                    //.Include(e => e.IsClose)
                    //.Include(e => e.IsComments)
                    .ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.AccessRights.Include(e => e.ActionName).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                //var fulldetails = "ActionName :" + ca.ActionName.LookupVal + ",IsApproveRejectAppl :" + ca.IsApproveRejectAppl + ",IsClose :" + ca.IsClose + ",IsComments :" + ca.IsComments + "";
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = "ActionName :" + ca.ActionName.LookupVal + ",IsApproveRejectAppl :" + ca.IsApproveRejectAppl + ",IsClose :" + ca.IsClose + ",LvNoOfDaysFrom :" + ca.LvNoOfDaysFrom + ",LvNoOfDaysTo :" + ca.LvNoOfDaysTo + ",IsComments :" + ca.IsComments + "" }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
    }
}