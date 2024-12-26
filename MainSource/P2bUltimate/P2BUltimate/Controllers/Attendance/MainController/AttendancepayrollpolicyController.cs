///
/// Created by Tanushri
///


using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using P2BUltimate.App_Start;
using System.Net;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Transactions;
using P2b.Global;
using System.Text;
using System.Threading.Tasks;
using P2BUltimate.Security;
using Attendance;


namespace P2BUltimate.Controllers.Attendance.MainController
{
    [AuthoriseManger]
    public class AttendancepayrollpolicyController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();
        //
        // GET: /Lookup/

        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/Attendancepayrollpolicy/Index.cshtml");
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Core/_Location.cshtml");

        }



        //private MultiSelectList GetLocationValues(List<int> selectedValues)
        //{
        //    List<Location> lkval = new List<Location>();
        //    lkval = db.Location.ToList();
        //    return new MultiSelectList(lkval, "Id", "FullDetails", selectedValues);
        //}






        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(AttendancePayrollPolicy NOBJ, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string PayScaleType = form["PayProcessGroupList"] == "0" ? "" : form["PayProcessGroupList"];
                    // string RoundingList = form["RoundingList"] == "0" ? "" : form["RoundingList"];
                    var LWPAdjustCurSal = form["LWPAdjustCurSal"];
                    NOBJ.LWPAdjustCurSal = Convert.ToBoolean(LWPAdjustCurSal);
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    var Company = new Company();
                    if (company_Id != null)
                    {
                        Company = db.Company.Where(e => e.Id == company_Id).SingleOrDefault();

                    }


                    if (PayScaleType != null)
                    {
                        if (PayScaleType != "")
                        {
                            var val = db.PayProcessGroup.Find(int.Parse(PayScaleType));
                            NOBJ.PayProcessGroup = val;
                        }
                    }

                    if (PayScaleType == null && PayScaleType == "")
                    {
                        Msg.Add("  Select PayProcessGroup  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new Object[] { "", "", "Select PayScaleType", JsonRequestBehavior.AllowGet });
                    }

                    //string Values = form["PayScaleArealist"];

                    List<Location> OBJ = new List<Location>();
                    string Values = form["PayScaleArealist"];





                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            NOBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            AttendancePayrollPolicy PayScale = new AttendancePayrollPolicy()
                            {
                                PayProcessGroup = NOBJ.PayProcessGroup,
                                LWPAdjustCurSal = NOBJ.LWPAdjustCurSal,
                                DBTrack = NOBJ.DBTrack
                            };
                            try
                            {

                                db.AttendancePayrollPolicy.Add(PayScale);

                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = PayScale.Id, Val = PayScale.LWPAdjustCurSal.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { PayScale.Id, PayScale.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = NOBJ.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                            PayScale PayScale = db.PayScale.Include(e => e.PayScaleArea.Select(r => r.LocationObj)).Include(e => e.PayScaleType)
                              .FirstOrDefault(e => e.Id == auth_id);
                            PayScale.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = PayScale.DBTrack.ModifiedBy != null ? PayScale.DBTrack.ModifiedBy : null,
                                CreatedBy = PayScale.DBTrack.CreatedBy != null ? PayScale.DBTrack.CreatedBy : null,
                                CreatedOn = PayScale.DBTrack.CreatedOn != null ? PayScale.DBTrack.CreatedOn : null,
                                IsModified = PayScale.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };
                            db.PayScale.Attach(PayScale);
                            db.Entry(PayScale).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(PayScale).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, PayScale.DBTrack);
                            DT_PayScale DT_OBJ = (DT_PayScale)rtn_Obj;
                            DT_OBJ.PayScaleType_Id = PayScale.PayScaleType == null ? 0 : PayScale.PayScaleType.Id;

                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = PayScale.Id, Val = PayScale.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { PayScale.Id, PayScale.FullDetails, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        PayScale Old_OBJ = db.PayScale.Include(e => e.PayScaleType)
                                                          .Include(e => e.PayScaleArea.Select(r => r.LocationObj))
                                                          .Where(e => e.Id == auth_id).SingleOrDefault();

                        // DT_PayScale Curr_OBJ = db.DT_PayScale.Include(e => e.PayScaleArea)  
                        DT_PayScale Curr_OBJ = db.DT_PayScale
                                              .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                              .OrderByDescending(e => e.Id)
                                              .FirstOrDefault();

                        PayScale PayScale = new PayScale();

                        string Arealst = Curr_OBJ.PayScaleArea_Id == null ? null : Curr_OBJ.PayScaleArea_Id.ToString();
                        string Typelst = Curr_OBJ.PayScaleType_Id == null ? null : Curr_OBJ.PayScaleType_Id.ToString();


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    // db.Configuration.AutoDetectChangesEnabled = false;
                                    PayScale.DBTrack = new DBTrack
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

                                    int a = EditS(Arealst, Typelst, "", auth_id, PayScale, PayScale.DBTrack);

                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = PayScale.Id, Val = PayScale.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { PayScale.Id, PayScale.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (PayScale)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (PayScale)databaseEntry.ToObject();
                                    PayScale.RowVersion = databaseValues.RowVersion;
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
                            //PayScale corp = db.PayScale.Find(auth_id);
                            PayScale PayScale = db.PayScale.AsNoTracking().Include(e => e.PayScaleArea.Select(r => r.LocationObj))
                                                                        .FirstOrDefault(e => e.Id == auth_id);
                            var selectedValues = PayScale.PayScaleArea;
                            PayScale.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = PayScale.DBTrack.ModifiedBy != null ? PayScale.DBTrack.ModifiedBy : null,
                                CreatedBy = PayScale.DBTrack.CreatedBy != null ? PayScale.DBTrack.CreatedBy : null,
                                CreatedOn = PayScale.DBTrack.CreatedOn != null ? PayScale.DBTrack.CreatedOn : null,
                                IsModified = PayScale.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };
                            db.PayScale.Attach(PayScale);
                            db.Entry(PayScale).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, PayScale.DBTrack);
                            DT_PayScale DT_OBJ = (DT_PayScale)rtn_Obj;
                            DT_OBJ.PayScaleType_Id = PayScale.PayScaleType == null ? 0 : PayScale.PayScaleType.Id;

                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();

                            db.Entry(PayScale).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record Authorised", JsonRequestBehavior.AllowGet });
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



        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Location.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public async Task<ActionResult> EditSave(AttendancePayrollPolicy P, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string TypeOBJ = form["PayProcessGroupList"] == "0" ? "" : form["PayProcessGroupList"];

                    var LWPAdjustCurSal = form["LWPAdjustCurSal"];
                    P.LWPAdjustCurSal = Convert.ToBoolean(LWPAdjustCurSal);
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    if (TypeOBJ != null)
                    {
                        if (TypeOBJ != "")
                        {
                            var val = db.PayProcessGroup.Find(int.Parse(TypeOBJ));
                            P.PayProcessGroup = val;
                            var typedetails = db.AttendancePayrollPolicy.Where(x => x.Id == data).ToList();
                            foreach (var s in typedetails)
                            {
                                s.PayProcessGroup = P.PayProcessGroup;
                                s.LWPAdjustCurSal = P.LWPAdjustCurSal;
                                db.AttendancePayrollPolicy.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                            Msg.Add("Record Updated.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                           

                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
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
        public class PayScale_PSA
        {
            public Array PayScale_id { get; set; }
            public Array PayScale_FullDetails { get; set; }
        }




        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<PayScale_PSA> return_data = new List<PayScale_PSA>();
                var Q = db.AttendancePayrollPolicy
                  .Include(e => e.PayProcessGroup)

                  .Where(e => e.Id == data).Select
                  (e => new
                  {
                      Id = e.Id,
                      LWPAdjustCurSal = e.LWPAdjustCurSal,
                      PayProcessGroup_Id = e.PayProcessGroup.Id == null ? 0 : e.PayProcessGroup.Id,
                  }).ToList();
                //var add_data = db.PayScale.Include(e => e.PayScaleArea).Where(e => e.Id == data).Select(e => e.PayScaleArea).SingleOrDefault();
                ////var a = db.PayScale.Include(e => e.BasicScaleDetails).Where(e => e.Id == data).Select(e => e.BasicScaleDetails).SingleOrDefault();
                //var BCDETAILS = (from ca in add_data
                //                 select new
                //                 {
                //                     Id = ca.Id,
                //                     PayScale_Id = ca.Id,
                //                     PayScale_FullDetails = ca.FullDetails
                //                 }).Distinct();
                //TempData["RowVersion"] = db.PayScale.Find(data).RowVersion;

                //var a = db.PayScale.Include(e => e.PayScaleType)
                //    .Include(e => e.PayScaleArea.Select(r => r.LocationObj))
                //    .Where(e => e.Id == data).Select(e => e.PayScaleArea.Select(t => t.LocationObj))
                //    .ToList();
                //if (a != null && a.Count > 0)
                //{
                //    foreach (var ca in a)
                //    {

                //        return_data.Add(
                //    new PayScale_PSA
                //    {
                //        PayScale_id = ca.Select(e => e.Id.ToString()).ToArray(),
                //        PayScale_FullDetails = ca.Select(e => e.LocDesc).ToArray()
                //    });
                //    }
                //}
                //var Old_Data = db.DT_PayScale
                //    //.Include(e => e.PayScaleArea)
                //    // .Include(e => e.PayScaleType) 
                // .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M")
                // .Select
                // (e => new
                // {
                //     DT_Id = e.Id,
                //     BasicScaleAppl = e.BasicScaleAppl,
                //     PayScaleADetails_Val = e.PayScaleArea_Id == 0 ? "" : db.Location.Where(x => x.Id == e.PayScaleArea_Id).Select(x => x.FullDetails).FirstOrDefault(),
                //     ////BasicScaleDetails_Val = e.BasicScaleDetails.Id == null ? "" : e.BasicScaleDetails.FullDetails,  
                //     PayScaleType_Val = e.PayScaleType_Id == 0 ? "" : db.LookupValue
                //                   .Where(x => x.Id == e.PayScaleType_Id)
                //                   .Select(x => x.LookupVal).FirstOrDefault(),
                // }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                //var Corp = db.PayScale.Find(data);
                //TempData["RowVersion"] = Corp.RowVersion;
                //var Auth = Corp.DBTrack.IsModified;


                return Json(new Object[] { Q, "", "", "", JsonRequestBehavior.AllowGet });
                //return this.Json(new Object[] { Q, BCDETAILS, JsonRequestBehavior.AllowGet });
            }
        }


        public int EditS(string TypeLS, string RoundingList, string Areal, int data, PayScale NOBJ, DBTrack dbT)
        {

            // db.Configuration.AutoDetectChangesEnabled = false;
            using (DataBaseContext db = new DataBaseContext())
            {

                if (TypeLS != null)
                {
                    if (TypeLS != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(TypeLS));
                        NOBJ.PayScaleType = val;

                        var type = db.PayScale.Include(e => e.PayScaleType).Where(e => e.Id == data).SingleOrDefault();
                        if (type.PayScaleType != null)
                        {
                            var typedetails = db.PayScale.Where(x => x.PayScaleType.Id == type.PayScaleType.Id && x.Id == data).ToList();
                            if (typedetails != null)
                            {
                                if (TypeLS == null || TypeLS == "")
                                {
                                    //return this.Json(new { msg = "select Pay Scale  type." });
                                }
                                foreach (var s in typedetails)
                                {
                                    s.PayScaleType = NOBJ.PayScaleType;
                                    db.PayScale.Attach(s);
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
                            var typedetails = db.PayScale.Where(x => x.Id == data).ToList();
                            foreach (var s in typedetails)
                            {
                                s.PayScaleType = NOBJ.PayScaleType;
                                db.PayScale.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                if (RoundingList != null)
                {
                    if (RoundingList != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(RoundingList));
                        NOBJ.Rounding = val;

                        var type = db.PayScale.Include(e => e.Rounding).Where(e => e.Id == data).SingleOrDefault();
                        if (type.Rounding != null)
                        {
                            var typedetails = db.PayScale.Where(x => x.Rounding.Id == type.Rounding.Id && x.Id == data).ToList();
                            if (typedetails != null)
                            {
                                foreach (var s in typedetails)
                                {
                                    s.Rounding = NOBJ.Rounding;
                                    db.PayScale.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                        }
                        else
                        {
                            var typedetails = db.PayScale.Where(x => x.Id == data).ToList();
                            foreach (var s in typedetails)
                            {
                                s.Rounding = NOBJ.Rounding;
                                db.PayScale.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                }

                var db_data = db.PayScale.Include(e => e.PayScaleArea.Select(r => r.LocationObj)).Where(e => e.Id == data).SingleOrDefault();
                List<Location> lookupval = new List<Location>();
                string Values = Areal;

                if (Values != null)
                {
                    var ids = Utility.StringIdsToListIds(Values);
                    foreach (var ca in ids)
                    {
                        var Lookup_val = db.Location.Include(e => e.LocationObj).Where(e => e.Id == ca).SingleOrDefault();
                        lookupval.Add(Lookup_val);
                        db_data.PayScaleArea = lookupval;
                    }
                }
                else
                {
                    db_data.PayScaleArea = null;
                }


                db.PayScale.Attach(db_data);
                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["RowVersion"] = db_data.RowVersion;
                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;


                var Curr_Corp = db.PayScale.Find(data);
                TempData["CurrRowVersion"] = Curr_Corp.RowVersion;
                db.Entry(Curr_Corp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    PayScale blog = blog = null;
                    DbPropertyValues originalBlogValues = null;

                    using (var context = new DataBaseContext())
                    {
                        blog = context.PayScale.Where(e => e.Id == data).SingleOrDefault();
                        originalBlogValues = context.Entry(blog).OriginalValues;
                    }

                    NOBJ.DBTrack = new DBTrack
                    {
                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                        Action = "M",
                        ModifiedBy = SessionManager.UserName,
                        ModifiedOn = DateTime.Now
                    };
                    PayScale TypeL = new PayScale()
                    {
                        BasicScaleAppl = NOBJ.BasicScaleAppl,
                        CPIAppl = NOBJ.CPIAppl,
                        PayScaleType = NOBJ.PayScaleType,
                        PayScaleArea = NOBJ.PayScaleArea,
                        ActualIndexAppl = NOBJ.ActualIndexAppl,
                        MultiplyingFactor = NOBJ.MultiplyingFactor,
                        Id = data,
                        DBTrack = NOBJ.DBTrack
                    };


                    db.PayScale.Attach(TypeL);
                    db.Entry(TypeL).State = System.Data.Entity.EntityState.Modified;
                    // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();
                    db.Entry(TypeL).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //  DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, NOBJ.DBTrack);
                    return 1;
                }
                return 0;
            }
        }

        // public ActionResult Delete(int data)
        //{
        //    try
        //    {
        //        //var ar = db.PayScale.Where(a => a.Id == data).Select(z => z.BasicScaleDetails).SingleOrDefault();
        //        PayScale basic = db.PayScale.Include(e => e.PayScaleArea).Where(w => w.Id == data).SingleOrDefault();
        //        //var basic_details_count = new HashSet<int>(basic.PayScaleArea.Select(e => e.Id));
        //        //if (basic_details_count.Count > 0)
        //        //{

        //        //    return Json(new Object[] { "", "Child record exists.Cannot delete..", JsonRequestBehavior.AllowGet });
        //        //    ///return this.Json(new { msg = "Child record exists.Cannot delete..", JsonRequestBehavior.AllowGet });                   
        //        //}
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            db.Entry(basic).State = System.Data.Entity.EntityState.Deleted;
        //            db.SaveChanges();
        //            ts.Complete();

        //        }
        //           return Json(new Object[] { "", "Record Deleted", JsonRequestBehavior.AllowGet });

        //        }

        //        catch (DataException /* dex */)
        //        {
        //            return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
        //        }            
        //}



        //[HttpPost]
        //public async Task<ActionResult> Delete(int data)
        //{
        //    List<string> Msg = new List<string>();
        //    try
        //    {
        //        PayScale PayScale = db.PayScale.Include(e => e.PayScaleArea).Include(e => e.PayScaleType)
        //                                           .Where(e => e.Id == data).SingleOrDefault();

        //        var add = PayScale.PayScaleArea;

        //        //PayScale PayScale = db.PayScale.Where(e => e.Id == data).SingleOrDefault();
        //        if (PayScale.DBTrack.IsModified == true)
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {
        //                //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, PayScale.DBTrack, PayScale, null, "PayScale");
        //                DBTrack dbT = new DBTrack
        //                {
        //                    Action = "D",
        //                    CreatedBy = PayScale.DBTrack.CreatedBy != null ? PayScale.DBTrack.CreatedBy : null,
        //                    CreatedOn = PayScale.DBTrack.CreatedOn != null ? PayScale.DBTrack.CreatedOn : null,
        //                    IsModified = PayScale.DBTrack.IsModified == true ? true : false
        //                };
        //                PayScale.DBTrack = dbT;
        //                db.Entry(PayScale).State = System.Data.Entity.EntityState.Modified;
        //                //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", PayScale, null, "PayScale", PayScale.DBTrack);
        //                await db.SaveChangesAsync();
        //                using (var context = new DataBaseContext())
        //                {
        //                    //  DBTrackFile.DBTrackSave("Core/P2b.Global", "D", PayScale, null, "PayScale", PayScale.DBTrack);
        //                }
        //                ts.Complete();
        //                Msg.Add("  Data removed successfully.  ");
        //                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
        //            }
        //        }
        //        else
        //        {
        //            var selectedRegions = PayScale.PayScaleArea;
        //            int company_Id = 0;
        //            company_Id = Convert.ToInt32(Session["CompId"]);
        //            var Companylist = db.Company.Include(e => e.PayScale).Where(e => e.Id == company_Id).SingleOrDefault();
        //            var paylist = Companylist.PayScale.Where(e => e.Id == PayScale.Id).SingleOrDefault();
        //            var v = paylist;
        //            //   var paylist = Companylist.PayScale.Where(e => e.Id == PayScale.Id).FirstOrDefault();
        //            //  paylist = null;
        //            db.SaveChanges();
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {
        //                if (selectedRegions != null)
        //                {
        //                    var Payscale = new HashSet<int>(PayScale.PayScaleArea.Select(e => e.Id));
        //                    if (Payscale.Count > 0)
        //                    {
        //                        Msg.Add(" Child record exists.Cannot remove it..  ");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        //return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
        //                        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
        //                    }
        //                }

        //                try
        //                {
        //                    DBTrack dbT = new DBTrack
        //                    {
        //                        Action = "D",
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now,
        //                        CreatedBy = PayScale.DBTrack.CreatedBy != null ? PayScale.DBTrack.CreatedBy : null,
        //                        CreatedOn = PayScale.DBTrack.CreatedOn != null ? PayScale.DBTrack.CreatedOn : null,
        //                        IsModified = PayScale.DBTrack.IsModified == true ? false : false,
        //                        AuthorizedBy = SessionManager.UserName,
        //                        AuthorizedOn = DateTime.Now
        //                    };
        //                    // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };
        //                    db.Entry(PayScale).State = System.Data.Entity.EntityState.Deleted;
        //                    await db.SaveChangesAsync();
        //                    using (var context = new DataBaseContext())
        //                    {
        //                        PayScale.PayScaleArea = add;
        //                        //  DBTrackFile.DBTrackSave("Core/P2b.Global", "D", PayScale, null, "PayScale", dbT);
        //                    }
        //                    ts.Complete();
        //                    Msg.Add("  Data removed successfully.  ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
        //                }
        //                catch (RetryLimitExceededException /* dex */)
        //                {
        //                    //Log the error (uncomment dex variable name and add a line here to write a log.)
        //                    //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
        //                    //return RedirectToAction("Delete");
        //                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogFile Logfile = new LogFile();
        //        ErrorLog Err = new ErrorLog()
        //        {
        //            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //            ExceptionMessage = ex.Message,
        //            ExceptionStackTrace = ex.StackTrace,
        //            LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //            LogTime = DateTime.Now
        //        };
        //        Logfile.CreateLogFile(Err);
        //        Msg.Add(ex.Message);
        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //    }
        //    return new EmptyResult();
        //}

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    AttendancePayrollPolicy AttendancePayrollPolicy = db.AttendancePayrollPolicy
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    var add = AttendancePayrollPolicy.PayProcessGroup;

                    //ocation payA = PayScale.PayScaleArea;
                    //PayScale PayScale = db.PayScale.Where(e => e.Id == data).SingleOrDefault();
                    if (AttendancePayrollPolicy.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, PayScale.DBTrack, PayScale, null, "PayScale");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = AttendancePayrollPolicy.DBTrack.CreatedBy != null ? AttendancePayrollPolicy.DBTrack.CreatedBy : null,
                                CreatedOn = AttendancePayrollPolicy.DBTrack.CreatedOn != null ? AttendancePayrollPolicy.DBTrack.CreatedOn : null,
                                IsModified = AttendancePayrollPolicy.DBTrack.IsModified == true ? true : false
                            };
                            AttendancePayrollPolicy.DBTrack = dbT;
                            db.Entry(AttendancePayrollPolicy).State = System.Data.Entity.EntityState.Modified;
                            
                            //using (var context = new DataBaseContext())
                            //{
                            //      DBTrackFile.DBTrackSave("Core/P2b.Global", "D", PayScale, null, "PayScale", PayScale.DBTrack);
                            //}
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                        }
                    }
                    else
                    {
                        var PayA = AttendancePayrollPolicy.PayProcessGroup;
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = AttendancePayrollPolicy.DBTrack.CreatedBy != null ? AttendancePayrollPolicy.DBTrack.CreatedBy : null,
                                    CreatedOn = AttendancePayrollPolicy.DBTrack.CreatedOn != null ? AttendancePayrollPolicy.DBTrack.CreatedOn : null,
                                    IsModified = AttendancePayrollPolicy.DBTrack.IsModified == true ? false : false,
                                    // AuthorizedBy = SessionManager.UserName,
                                    // AuthorizedOn = DateTime.Now
                                };
                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };
                                db.Entry(AttendancePayrollPolicy).State = System.Data.Entity.EntityState.Deleted;
                               
                                //using (var context = new DataBaseContext())
                                //{
                                //    PayScale.PayScaleArea = add;
                                //    //  DBTrackFile.DBTrackSave("Core/P2b.Global", "D", PayScale, null, "PayScale", dbT);
                                //}
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


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
                return new EmptyResult();
            }
        }


        public class P2BGridData
        {
            public int Id { get; set; }
            public bool LWPAdjustCurSal { get; set; }
            public string PayProcessGroupList { get; set; }
        }



        public ActionResult P2BGrid(P2BGrid_Parameters gp)
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

                    IEnumerable<P2BGridData> PayscaleList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;


                    //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                    //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();

                    var BindCompList = db.AttendancePayrollPolicy.Include(e => e.PayProcessGroup)
                       .ToList();

                    foreach (var z in BindCompList)
                    {
                        if (z.PayProcessGroup != null)
                        {
                            view = new P2BGridData()
                               {
                                   Id = z.Id,
                                   LWPAdjustCurSal = z.LWPAdjustCurSal,
                                   PayProcessGroupList = z.PayProcessGroup != null ? z.PayProcessGroup.FullDetails.ToString() : "",
                               };
                            model.Add(view);

                        }

                    }

                    PayscaleList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = PayscaleList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.LWPAdjustCurSal.ToString().ToUpper().Contains(gp.searchString.ToUpper()))

                               || (e.PayProcessGroupList.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.LWPAdjustCurSal, a.PayProcessGroupList, a.Id }).ToList();


                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;



                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.LWPAdjustCurSal), a.PayProcessGroupList != null ? Convert.ToString(a.PayProcessGroupList) : "", a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = PayscaleList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "LWPAdjustCurSal" ? c.LWPAdjustCurSal.ToString() :
                                             gp.sidx == "PayProcessGroupList" ? c.PayProcessGroupList.ToString() : ""

                                            );
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.LWPAdjustCurSal), a.PayProcessGroupList != null ? Convert.ToString(a.PayProcessGroupList) : "", a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.LWPAdjustCurSal), a.PayProcessGroupList != null ? Convert.ToString(a.PayProcessGroupList) : "", a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.LWPAdjustCurSal), a.PayProcessGroupList != null ? Convert.ToString(a.PayProcessGroupList) : "", a.Id }).ToList();
                        }
                        totalRecords = PayscaleList.Count();
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

    }
}
