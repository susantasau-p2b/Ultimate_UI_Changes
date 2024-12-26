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
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class SuspensionSalPolicyController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {

            return View("~/Views/Payroll/MainViews/SuspensionSalPolicy/Index.cshtml");

        }


        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        #region CREATE
        [HttpPost]
        public ActionResult Create(SuspensionSalPolicy S, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int company_Id=0;
                    //#ConvertLeaveHeadBallist,#ConvertLeaveHeadlist
                    var SuspensionWageslist = form["SuspensionWageslist"] == "0" ? "" : form["SuspensionWageslist"];
                    var DayRangelist = form["DayRangelist"] == "0" ? "" : form["DayRangelist"];

                    company_Id = Convert.ToInt32(Session["CompId"]);
                    var companypayroll = new CompanyPayroll();
                  //  companypayroll = db.CompanyPayroll.Include(e => e.SuspensionSalPolicy).Where(e => e.Company.Id == company_Id).SingleOrDefault();
                    companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == company_Id).SingleOrDefault();
                    if (SuspensionWageslist != null && SuspensionWageslist != "")
                    {
                        var value = db.Wages.Find(int.Parse(SuspensionWageslist));
                        S.SuspensionWages = value;

                    }

                    List<Range> ObjRange = new List<Range>();


                    if (DayRangelist != null && DayRangelist != " ")
                    {
                        var ids = one_ids(DayRangelist);
                        foreach (var ca in ids)
                        {
                            var value = db.Range.Find(ca);
                            ObjRange.Add(value);
                            S.DayRange = ObjRange;
                        }

                    }
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            S.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            if (db.SuspensionSalPolicy.Any(a => a.PolicyName == S.PolicyName && a.EffectiveDate == S.EffectiveDate))
                            {
                                Msg.Add(" Record Already Exist  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId };

                            SuspensionSalPolicy OBJSSP = new SuspensionSalPolicy()
                            {
                                DayRange = S.DayRange,
                                EffectiveDate = S.EffectiveDate,
                                PolicyName = S.PolicyName,
                                SuspensionWages = S.SuspensionWages,
                                DBTrack = S.DBTrack
                            };
                            try
                            {
                                db.SuspensionSalPolicy.Add(OBJSSP);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, L.DBTrack);
                                //DT_LvCreditPolicy DT_OBJ = (DT_LvCreditPolicy)rtn_Obj;
                                //db.Create(DT_OBJ);
                                db.SaveChanges();
                                List<SuspensionSalPolicy> Suspensionsalpolicy_list = new List<SuspensionSalPolicy>();
                                Suspensionsalpolicy_list.Add(OBJSSP);
                                if (companypayroll != null)
                                {
                                 
                                    companypayroll.SuspensionSalPolicy = Suspensionsalpolicy_list;
                                    db.CompanyPayroll.Attach(companypayroll);
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Detached;
                                }
                                ts.Complete();
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = OBJSSP.Id, Val = OBJSSP.PolicyName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { OBJSSP.Id, OBJSSP.PolicyName, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = S.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add("  Unable to create...  ");
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
                        //       Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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


        #endregion


        #region EDIT & EDITSAVE

        public class EditDetails
        {

            public Array DayRange_Id { get; set; }
            public Array Dayrange_FullDetails { get; set; }

        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.SuspensionSalPolicy
                    .Include(e => e.SuspensionWages)
                    .Include(e => e.DayRange)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {

                        EffectiveDate = e.EffectiveDate,
                        PolicyName = e.PolicyName,
                        SuspensionWages_Id = e.SuspensionWages.Id == null ? 0 : e.SuspensionWages.Id,
                        SuspensionWages_fullDetails = e.SuspensionWages.FullDetails == null ? "" : e.SuspensionWages.FullDetails,
                        Action = e.DBTrack.Action
                    }).ToList();

                List<EditDetails> ObjDayrange = new List<EditDetails>();

                var SSP = db.SuspensionSalPolicy.Include(e => e.DayRange).Where(e => e.Id == data).Select(e => e.DayRange).ToList();
                if (SSP != null)
                {
                    foreach (var ca in SSP)
                    {
                        ObjDayrange.Add(new EditDetails
                        {
                            DayRange_Id = ca.Select(e => e.Id).ToArray(),
                            Dayrange_FullDetails = ca.Select(e => e.FullDetails).ToArray()

                        });


                    }

                }


                var Corp = db.SuspensionSalPolicy.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, ObjDayrange, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        //[HttpPost]
        //public async Task<ActionResult> EditSave(SuspensionSalPolicy S, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            var SuspensionWageslist = form["SuspensionWageslist"] == "0" ? "" : form["SuspensionWageslist"];
        //            var DayRangelist = form["DayRangelist"] == "0" ? "" : form["DayRangelist"];
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;

        //            var type = db.SuspensionSalPolicy.Include(e => e.SuspensionWages).Where(e => e.Id == data).SingleOrDefault();
        //            if (Auth == false)
        //            {
        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {
        //                        //DbContextTransaction transaction = db.Database.BeginTransaction();
        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            //if (db.SuspensionSalPolicy.Any(a => a.PolicyName == S.PolicyName && a.EffectiveDate == type.EffectiveDate))
        //                            //if (db.SuspensionSalPolicy.Any(a => a.PolicyName == S.PolicyName))
        //                            //{
        //                            //    Msg.Add(" Record Already Exist  ");
        //                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                            //}
        //                            SuspensionSalPolicy blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.SuspensionSalPolicy.Where(e => e.Id == data).Include(e => e.SuspensionWages)
        //                                                        .Include(e => e.DayRange)
        //                                                        .SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            S.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };

        //                            if (SuspensionWageslist != null)
        //                            {
        //                                if (SuspensionWageslist != "")
        //                                {
        //                                    var val = db.Wages.Find(int.Parse(SuspensionWageslist));
        //                                    S.SuspensionWages = val;

        //                                    IList<SuspensionSalPolicy> typedetails = null;
        //                                    if (type.SuspensionWages != null)
        //                                    {
        //                                        typedetails = db.SuspensionSalPolicy.Where(x => x.SuspensionWages.Id == type.SuspensionWages.Id && x.Id == data).ToList();
        //                                    }
        //                                    else
        //                                    {
        //                                        typedetails = db.SuspensionSalPolicy.Where(x => x.Id == data).ToList();
        //                                    }
        //                                    //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //                                    foreach (var s in typedetails)
        //                                    {
        //                                        s.SuspensionWages = S.SuspensionWages;
        //                                        db.SuspensionSalPolicy.Attach(s);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        TempData["RowVersion"] = s.RowVersion;
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    var WFTypeDetails = db.SuspensionSalPolicy.Include(e => e.SuspensionWages).Where(x => x.Id == data).ToList();
        //                                    foreach (var s in WFTypeDetails)
        //                                    {
        //                                        s.SuspensionWages = null;
        //                                        db.SuspensionSalPolicy.Attach(s);
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
        //                                var SwagesDetails = db.SuspensionSalPolicy.Include(e => e.SuspensionWages).Where(x => x.Id == data).ToList();
        //                                foreach (var s in SwagesDetails)
        //                                {
        //                                    s.SuspensionWages = null;
        //                                    db.SuspensionSalPolicy.Attach(s);
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                    //await db.SaveChangesAsync();
        //                                    db.SaveChanges();
        //                                    TempData["RowVersion"] = s.RowVersion;
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                }
        //                            }
        //                            List<Range> ObjSSP = new List<Range>();
        //                            SuspensionSalPolicy rangedetails = null;
        //                            rangedetails = db.SuspensionSalPolicy.Include(e => e.DayRange).Where(e => e.Id == data).SingleOrDefault();
        //                            if (DayRangelist != null && DayRangelist != "")
        //                            {
        //                                var ids = Utility.StringIdsToListIds(DayRangelist);
        //                                foreach (var ca in ids)
        //                                {
        //                                    var DayRangelist_val = db.Range.Find(ca);
        //                                    ObjSSP.Add(DayRangelist_val);
        //                                    rangedetails.DayRange = ObjSSP;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                rangedetails.DayRange = null;
        //                            }

        //                            var CurCorp = db.SuspensionSalPolicy.Find(data);
        //                            TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            {

        //                                SuspensionSalPolicy suspensionsalpolicy = new SuspensionSalPolicy()
        //                                {

        //                                    Id = data,
        //                                    EffectiveDate = type.EffectiveDate,
        //                                    PolicyName = S.PolicyName,

        //                                    DBTrack = S.DBTrack
        //                                };
        //                                db.SuspensionSalPolicy.Attach(suspensionsalpolicy);
        //                                db.Entry(suspensionsalpolicy).State = System.Data.Entity.EntityState.Modified;
        //                                db.Entry(suspensionsalpolicy).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                            }

        //                            //await db.SaveChangesAsync();
        //                            db.SaveChanges();
        //                            ts.Complete();

        //                            Msg.Add("Record Updated Successfully.");
        //                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = blog.PolicyName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        }

        //                    }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (SuspensionSalPolicy)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {
        //                            Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (SuspensionSalPolicy)databaseEntry.ToObject();
        //                            S.RowVersion = databaseValues.RowVersion;

        //                        }
        //                    }
        //                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    SuspensionSalPolicy blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    SuspensionSalPolicy Old_Corp = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.SuspensionSalPolicy.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }
        //                    S.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };

        //                    if (TempData["RowVersion"] == null)
        //                    {
        //                        TempData["RowVersion"] = blog.RowVersion;
        //                    }

        //                    SuspensionSalPolicy corp = new SuspensionSalPolicy()
        //                    {
        //                        DayRange = S.DayRange,
        //                        EffectiveDate = type.EffectiveDate,
        //                        PolicyName = S.PolicyName,
                                
        //                        SuspensionWages = S.SuspensionWages,
        //                        Id = data,
        //                        DBTrack = S.DBTrack,
        //                        RowVersion = (Byte[])TempData["RowVersion"]
        //                    };


        //                    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        //var obj = DBTrackFile.ModifiedDataHistory("Leave/Leave", "M", blog, corp, "LvEncashReq", L.DBTrack);
        //                        //// var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                        //Old_Corp = context.LvCreditPolicy.Where(e => e.Id == data).Include(e => e.ConvertLeaveHead)
        //                        //    .Include(e => e.ConvertLeaveHeadBal).Include(e => e.ExcludeLeaveHeads).Include(e => e.CreditDate).SingleOrDefault();
        //                        //DT_LvCreditPolicy DT_Corp = (DT_LvCreditPolicy)obj;
        //                        //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;

        //                        //db.Create(DT_Corp);
        //                        //db.SaveChanges();
        //                    }
        //                    blog.DBTrack = S.DBTrack;
        //                    db.SuspensionSalPolicy.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    Msg.Add("  Record Updated");
        //                    return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = S.PolicyName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    // return Json(new Object[] { blog.Id, S.PolicyName, "Record Updated", JsonRequestBehavior.AllowGet });
        //                }

        //            }
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

        //        return View();
        //    }

        //}


        [HttpPost]
        public async Task<ActionResult> EditSave(SuspensionSalPolicy c, int data, FormCollection form) // Edit submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var SuspensionWageslist = form["SuspensionWageslist"] == "" ? "" : form["SuspensionWageslist"];
                    var DayRangelist = form["DayRangelist"] == "" ? "" : form["DayRangelist"];
                    
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    c.SuspensionWages_Id = SuspensionWageslist != "" && SuspensionWageslist != null ? int.Parse(SuspensionWageslist) : 0;

                 
                    var db_data = db.SuspensionSalPolicy.Include(e => e.DayRange).Include(e => e.SuspensionWages).Where(e => e.Id == data).SingleOrDefault();
                    List<Range> ObjBudsection = new List<Range>();

                    if ( DayRangelist != "")
                    {
                        var ids = Utility.StringIdsToListIds(DayRangelist);
                        foreach (var ba in ids)
                        {
                            var value = db.Range.Find(ba);
                            ObjBudsection.Add(value);
                            db_data.DayRange = ObjBudsection;
                        }
                    }
                    else
                    {
                        db_data.DayRange = null;
                    }

 
                    db.SuspensionSalPolicy.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    TempData["RowVersion"] = db_data.RowVersion; 

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                SuspensionSalPolicy blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;


                                blog = db.SuspensionSalPolicy.Where(e => e.Id == data).Include(e => e.DayRange)
                                                        .Include(e => e.SuspensionWages).SingleOrDefault();
                                originalBlogValues = db.Entry(blog).OriginalValues;

                                c.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };


                                var ESIOBJ = db.SuspensionSalPolicy.Find(data);
                                TempData["CurrRowVersion"] = ESIOBJ.RowVersion;

                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    ESIOBJ.EffectiveDate = c.EffectiveDate;
                                    ESIOBJ.PolicyName = c.PolicyName;
                                    ESIOBJ.SuspensionWages_Id = c.SuspensionWages_Id;
                                    ESIOBJ.Id = data;
                                    ESIOBJ.DBTrack = c.DBTrack; 
                                    db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified; 
                                }


                                var Obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, c, "SuspensionSalPolicy", c.DBTrack);
                                DT_SuspensionSalPolicy DT_Cat = (DT_SuspensionSalPolicy)Obj;
                                DT_Cat.SuspensionWages_Id = ESIOBJ.SuspensionWages != null ? ESIOBJ.SuspensionWages.Id : 0;
                                db.DT_SuspensionSalPolicy.Add(DT_Cat);
                                db.SaveChanges();

                                ts.Complete();
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet); 
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            SuspensionSalPolicy blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            SuspensionSalPolicy Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.SuspensionSalPolicy.Where(e => e.Id == data).Include(e => e.SuspensionWages).SingleOrDefault();
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

                            SuspensionSalPolicy corp = new SuspensionSalPolicy()
                            {
                                DayRange = c.DayRange,
                                EffectiveDate = c.EffectiveDate,
                                PolicyName = c.PolicyName,
                                SuspensionWages = c.SuspensionWages,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "Corporate", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.SuspensionSalPolicy.Where(e => e.Id == data).Include(e => e.DayRange)
                                    .Include(e => e.SuspensionWages).SingleOrDefault();
                                //DT_Corporate DT_Corp = (DT_Corporate)obj;
                                //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                //db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.SuspensionSalPolicy.Attach(blog);
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
                    var clientValues = (SuspensionSalPolicy)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (SuspensionSalPolicy)databaseEntry.ToObject();
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


        //[HttpPost]
        //public async Task<ActionResult> EditSave(SuspensionSalPolicy c, int data, FormCollection form) // Edit submit
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<String> Msg = new List<String>();
        //        try
        //        {
        //            var SuspensionWageslist = form["SuspensionWageslist"] == "0" ? "" : form["SuspensionWageslist"];
        //            var DayRangelist = form["DayRangelist"] == "0" ? "" : form["DayRangelist"];
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;


        //            List<SuspensionSalPolicy> ObjITsection = new List<SuspensionSalPolicy>();

        //            SuspensionSalPolicy pd = null;
        //            pd = db.SuspensionSalPolicy.Include(e => e.SuspensionWages).Where(e => e.Id == data).SingleOrDefault();
        //            //if (Addrs != "")
        //            //{
        //            //    var ids = Utility.StringIdsToListIds(Addrs);
        //            //    foreach (var ca in ids)
        //            //    {
        //            //        var value = db.SalaryHead.Find(ca);
        //            //        ObjITsection.Add(value);
        //            //        pd.SalaryHead = ObjITsection;

        //            //    }
        //            //}
        //            //else
        //            //{
        //            //    pd.SalaryHead = null;

        //            //}
        //            if (ModelState.IsValid)
        //            {
        //                //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    SuspensionSalPolicy blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.SuspensionSalPolicy.Where(e => e.Id == data).Include(e => e.SuspensionWages)
        //                                                           .Include(e => e.DayRange)
        //                                                           .SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }

        //                    c.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };
        //                    var m1 = db.SuspensionSalPolicy.Where(e => e.Id == data).ToList();
        //                    foreach (var s in m1)
        //                    {
        //                        // s.AppraisalPeriodCalendar = null;
        //                        db.SuspensionSalPolicy.Attach(s);
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                        //await db.SaveChangesAsync();
        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = s.RowVersion;
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                    }

        //                    var CurCorp = db.SuspensionSalPolicy.Find(data);
        //                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;

        //                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                    {
        //                        SuspensionSalPolicy corp = new SuspensionSalPolicy()
        //                        {
        //                            DayRange = c.DayRange,
        //                            EffectiveDate = c.EffectiveDate,
        //                            PolicyName = c.PolicyName,
        //                            SuspensionWages = c.SuspensionWages,
        //                            Id = data,
        //                            DBTrack = c.DBTrack,
        //                            RowVersion = (Byte[])TempData["RowVersion"]
        //                        };

        //                        db.SuspensionSalPolicy.Attach(corp);
        //                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    }
        //                    //await db.SaveChangesAsync();
        //                    db.SaveChanges();
        //                    ts.Complete();

        //                    Msg.Add("Record Updated Successfully.");
        //                    return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = blog.PolicyName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }

        //                Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //            }

        //        }
        //        catch (DbUpdateConcurrencyException ex)
        //        {
        //            var entry = ex.Entries.Single();
        //            var clientValues = (SuspensionSalPolicy)entry.Entity;
        //            var databaseEntry = entry.GetDatabaseValues();
        //            if (databaseEntry == null)
        //            {
        //                Msg.Add(ex.Message);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //            }
        //            else
        //            {
        //                var databaseValues = (SuspensionSalPolicy)databaseEntry.ToObject();
        //                c.RowVersion = databaseValues.RowVersion;

        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Msg.Add(e.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //        return View();
        //    }
        //}


        #endregion


        #region P2BGRID DETAILS

        public class P2BGridData
        {
            public int Id { get; set; }
            public string PolicyName { get; set; }
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

                    IEnumerable<P2BGridData> SSPList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;


                    //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                    //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);

                    var BindCompList = db.CompanyPayroll.Include(e => e.SuspensionSalPolicy).Where(e => e.Company.Id == company_Id).ToList();

                    foreach (var z in BindCompList)
                    {
                        if (z.SuspensionSalPolicy != null && z.SuspensionSalPolicy.Count > 0)
                        {

                            foreach (var s in z.SuspensionSalPolicy)
                            {
                                //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
                                view = new P2BGridData()
                                {
                                    Id = s.Id,
                                    PolicyName = s.PolicyName

                                };
                                model.Add(view);

                            }
                        }

                    }

                    SSPList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = SSPList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                                || (e.PolicyName.ToUpper().Contains(gp.searchString.ToUpper()))
                                ).Select(a => new Object[] { a.PolicyName, a.Id }).ToList();
                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  Convert.ToString(a.PolicyName), a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = SSPList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "PolicyName" ? c.PolicyName.ToString() : ""

                                            );
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] {  Convert.ToString(a.PolicyName), a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.PolicyName), a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.PolicyName), a.Id }).ToList();
                        }
                        totalRecords = SSPList.Count();
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


        public ActionResult P2BGrid1(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<SuspensionSalPolicy> skill = null;
                if (gp.IsAutho == true)
                {
                    skill = db.SuspensionSalPolicy.Include(e => e.SuspensionWages).Include(e => e.DayRange).Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    skill = db.SuspensionSalPolicy.ToList();
                }

                IEnumerable<SuspensionSalPolicy> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = skill;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new {  a.PolicyName, a.Id }).Where((e => (e.Id.ToString() == gp.searchString) || (e.PolicyName.ToLower() == gp.searchString.ToLower()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.PolicyName, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = skill;
                    Func<SuspensionSalPolicy, int> orderfuc = (c =>
                                                               gp.sidx == "Id" ? c.Id : 0);
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.PolicyName), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  Convert.ToString(a.PolicyName), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  a.PolicyName, a.Id }).ToList();
                    }
                    totalRecords = skill.Count();
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
        public ActionResult Delete(int data)
        {
             List<string> Msg = new List<string>();
             try
             {
                 using (DataBaseContext db = new DataBaseContext())
                 {
                     SuspensionSalPolicy SuspensionSalPolicy = db.SuspensionSalPolicy.Include(e => e.DayRange)
                                                          .Include(e => e.SuspensionWages)
                                                          .Where(e => e.Id == data).SingleOrDefault();



                     using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                     {

                         try
                         {
                             DBTrack dbT = new DBTrack
                             {
                                 Action = "D",
                                 ModifiedBy = SessionManager.UserName,
                                 ModifiedOn = DateTime.Now,
                                 CreatedBy = SuspensionSalPolicy.DBTrack.CreatedBy != null ? SuspensionSalPolicy.DBTrack.CreatedBy : null,
                                 CreatedOn = SuspensionSalPolicy.DBTrack.CreatedOn != null ? SuspensionSalPolicy.DBTrack.CreatedOn : null,
                                 IsModified = SuspensionSalPolicy.DBTrack.IsModified == true ? false : false//,

                             };



                             db.BasicLinkedDA.Where(e => e.Id == SuspensionSalPolicy.Id);
                             db.BasicLinkedDA = null;
                             db.Entry(SuspensionSalPolicy).State = System.Data.Entity.EntityState.Deleted;


                             db.SaveChanges();
                             ts.Complete();

                             Msg.Add("  Data removed successfully.  ");
                             return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                             // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                         }
                         catch (RetryLimitExceededException /* dex */)
                         {
                             // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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

            //using (DataBaseContext db = new DataBaseContext())
            //{
            //    try
            //    {
            //        SuspensionSalPolicy SuspensionSalPolicy = db.SuspensionSalPolicy.Include(e => e.DayRange)
            //                                              .Include(e => e.SuspensionWages)
            //                                              .Where(e => e.Id == data).SingleOrDefault();

            //        var id = int.Parse(Session["CompId"].ToString());
            //        var companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == id).SingleOrDefault();
            //        companypayroll.SuspensionSalPolicy.Where(e => e.Id == SuspensionSalPolicy.Id);
            //        companypayroll.SuspensionSalPolicy = null;
            //        db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
            //        db.SaveChanges();
            //        Msg.Add("  Data removed successfully.  ");
            //        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

            //        //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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


            //}
      
        #endregion
        }
    }
