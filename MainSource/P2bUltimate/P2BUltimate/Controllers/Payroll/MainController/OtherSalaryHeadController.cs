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
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class OtherSalaryHeadController : Controller
    {
        //  private DataBaseContext db = new DataBaseContext();
        //
        // GET: /OtherSalaryHead/

        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/OtherSalaryHead/Index.cshtml");
        }




        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(OtherSalaryHead COBJ, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string SalaryType = form["SalaryTypeList"] == "0" ? "" : form["SalaryTypeList"];
                    string RoundingMethod = form["RoundingMethodlist"] == "0" ? "" : form["RoundingMethodlist"];


                    if (SalaryType != null)
                    {
                        if (SalaryType != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(SalaryType));
                            COBJ.SalaryType = val;
                        }
                    }

                    if (RoundingMethod != null)
                    {
                        if (RoundingMethod != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(RoundingMethod));
                            COBJ.RoundingMethod = val;
                        }
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.OtherSalaryHead.Any(o => o.SalaryCode == COBJ.SalaryCode))
                            {
                                // return this.Json(new Object[] { null, null, "SalaryCode already exists.", JsonRequestBehavior.AllowGet });
                                Msg.Add("SalaryCode Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            OtherSalaryHead OtherSalaryHead = new OtherSalaryHead()
                            {
                                SalaryCode = COBJ.SalaryCode,
                                SalaryType = COBJ.SalaryType,
                                InPayslip = COBJ.InPayslip,
                                SalaryDesc = COBJ.SalaryDesc,
                                RoundDigit = COBJ.RoundDigit,
                                RoundingMethod = COBJ.RoundingMethod,
                                InITax = COBJ.InITax,
                                DBTrack = COBJ.DBTrack
                            };
                            try
                            {
                                db.OtherSalaryHead.Add(OtherSalaryHead);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, COBJ.DBTrack);
                                DT_OtherSalaryHead DT_OBJ = (DT_OtherSalaryHead)rtn_Obj;
                                DT_OBJ.RoundingMethod_Id = COBJ.RoundingMethod == null ? 0 : COBJ.RoundingMethod.Id;
                                DT_OBJ.SalaryType_Id = COBJ.SalaryType == null ? 0 : COBJ.SalaryType.Id;
                                db.Create(DT_OBJ);
                                db.SaveChanges();

                                ts.Complete();
                                //Msg.Add("  Data Saved successfully  ");return Json(new Utility.JsonReturnClass {  success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = COBJ.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                //   return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                                Msg.Add(" Unable to create.Try again, and if the problem persists, contact your system administrator");
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

                        // Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                var OtherSalaryHead = db.OtherSalaryHead
                                    .Include(e => e.SalaryType)
                                    .Include(e => e.RoundingMethod)
                                    .Where(e => e.Id == data).ToList();
                var r = (from ca in OtherSalaryHead
                         select new
                         {

                             Id = ca.Id,
                             RoundingMethod = ca.RoundingMethod.Id,
                             SalaryType = ca.SalaryType.Id,
                             SalaryCode = ca.SalaryCode,
                             SalaryDesc = ca.SalaryDesc,
                             RoundDigit = ca.RoundDigit,
                             InITax = ca.InITax,
                             InPayslip = ca.InPayslip,
                             Action = ca.DBTrack.Action
                         }).Distinct();

                var a = "";

                var W = db.DT_OtherSalaryHead
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         SalaryCode = e.SalaryCode == null ? "" : e.SalaryCode,
                         SalaryDesc = e.SalaryDesc == null ? "" : e.SalaryDesc,
                         RoundDigit = e.RoundDigit == null ? "" : e.RoundDigit.ToString(),
                         InPayslip = e.InPayslip,
                         InITax = e.InITax,
                         RoundingMethod = e.RoundingMethod_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.RoundingMethod_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),
                         SalaryType = e.SalaryType_Id == 0 ? "" : db.LookupValue
                                     .Where(x => x.Id == e.SalaryType_Id)
                                     .Select(x => x.LookupVal).FirstOrDefault(),

                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var LKup = db.OtherSalaryHead.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, a, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }





        //   [HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> EditSave(OtherSalaryHead ESOBJ, FormCollection form, int data)
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {

        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //            string SalaryType = form["SalaryTypeList"] == "0" ? "" : form["SalaryTypeList"];
        //            if (SalaryType != null)
        //            {
        //                if (SalaryType != "")
        //                {
        //                    var val = db.LookupValue.Find(int.Parse(SalaryType));
        //                    ESOBJ.SalaryType = val;
        //                }
        //            }
        //            string RoundingMethod = form["RoundingMethodlist"] == "0" ? "" : form["RoundingMethodlist"];
        //            if (RoundingMethod != null)
        //            {
        //                if (RoundingMethod != "")
        //                {
        //                    var val = db.LookupValue.Find(int.Parse(RoundingMethod));
        //                    ESOBJ.RoundingMethod = val;
        //                }
        //            }






        //            if (Auth == false)
        //            {
        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {
        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            OtherSalaryHead blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.OtherSalaryHead.Where(e => e.Id == data)
        //                                                        .Include(e => e.SalaryType)
        //                                                        .Include(e => e.RoundingMethod)
        //                                                        .SingleOrDefault();
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

        //                            int a = EditS(RoundingMethod, SalaryType, data, ESOBJ, ESOBJ.DBTrack);



        //                            using (var context = new DataBaseContext())
        //                            {
        //                                var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
        //                                DT_OtherSalaryHead DT_OBJ = (DT_OtherSalaryHead)obj;
        //                                DT_OBJ.SalaryType_Id = blog.SalaryType == null ? 0 : blog.SalaryType.Id;
        //                                DT_OBJ.RoundingMethod_Id = blog.RoundingMethod == null ? 0 : blog.RoundingMethod.Id;
        //                                db.Create(DT_OBJ);
        //                                db.SaveChanges();
        //                            }
        //                            await db.SaveChangesAsync();
        //                            ts.Complete();

        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = RoundingMethod.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                            //     return Json(new Object[] { ESOBJ.Id, ESOBJ.RoundingMethod.LookupVal, "Record Updated", JsonRequestBehavior.AllowGet });

        //                        }
        //                    }

        //                    //catch (DbUpdateException e) { throw e; }
        //                    //catch (DataException e) { throw e; }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (OtherSalaryHead)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {
        //                            // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                            Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (OtherSalaryHead)databaseEntry.ToObject();
        //                            ESOBJ.RowVersion = databaseValues.RowVersion;

        //                        }
        //                    }
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

        //                    // Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }
        //            }


        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    OtherSalaryHead blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    OtherSalaryHead Old_Obj = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.OtherSalaryHead.Where(e => e.Id == data).SingleOrDefault();
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
        //                    OtherSalaryHead corp = new OtherSalaryHead()
        //                    {
        //                        SalaryCode = ESOBJ.SalaryCode,
        //                        SalaryDesc = ESOBJ.SalaryDesc,
        //                        RoundDigit = ESOBJ.RoundDigit,
        //                        Id = data,
        //                        DBTrack = ESOBJ.DBTrack,
        //                        RowVersion = (Byte[])TempData["RowVersion"]
        //                    };


        //                    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        var obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, corp, "OtherSalaryHead", ESOBJ.DBTrack);
        //                        // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                        Old_Obj = context.OtherSalaryHead.Where(e => e.Id == data).Include(e => e.SalaryType)
        //                            .Include(e => e.RoundingMethod).SingleOrDefault();
        //                        DT_OtherSalaryHead DT_Corp = (DT_OtherSalaryHead)obj;
        //                        DT_Corp.SalaryType_Id = DBTrackFile.ValCompare(Old_Obj.SalaryType, ESOBJ.SalaryType);//Old_Obj.Address == c.Address ? 0 : Old_Obj.Address == null && c.Address != null ? c.Address.Id : Old_Obj.Address.Id;
        //                        DT_Corp.RoundingMethod_Id = DBTrackFile.ValCompare(Old_Obj.RoundingMethod, ESOBJ.RoundingMethod); //Old_Obj.BusinessType == c.BusinessType ? 0 : Old_Obj.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Obj.BusinessType.Id;                        
        //                        db.Create(DT_Corp);
        //                        //db.SaveChanges();
        //                    }
        //                    blog.DBTrack = ESOBJ.DBTrack;
        //                    db.OtherSalaryHead.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    // return Json(new Object[] { blog.Id, ESOBJ.SalaryDesc, "Record Updated", JsonRequestBehavior.AllowGet });
        //                    Msg.Add("  Record Updated");
        //                    return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = RoundingMethod.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
        public async Task<ActionResult> EditSave(OtherSalaryHead c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string SalaryType = form["SalaryTypelist"] == "0" ? "" : form["SalaryTypelist"];
                    string RoundingMethod = form["RoundingMethodlist"] == "0" ? "" : form["RoundingMethodlist"];
                    // string Frequency = form["Frequencylist"] == "0" ? "" : form["Frequencylist"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    if (SalaryType != null)
                    {
                        if (SalaryType != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(SalaryType));
                            c.SalaryType = val;
                        }
                    }

                    if (RoundingMethod != null)
                    {
                        if (RoundingMethod != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(RoundingMethod));
                            c.RoundingMethod = val;
                        }
                    }

                    //if (Frequency != null)
                    //{
                    //    if (Frequency != "")
                    //    {
                    //        var val = db.LookupValue.Find(int.Parse(Frequency));
                    //        c.RoundingMethod = val;
                    //    }
                    //}

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    OtherSalaryHead blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.OtherSalaryHead.Where(e => e.Id == data)
                                                                .Include(e => e.SalaryType)
                                                                .Include(e => e.RoundingMethod)
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

                                    // int a = EditS(Category, FactExte, data, c, c.DBTrack);

                                    //if (Category != null)
                                    //{
                                    //    if (Category != "")
                                    //    {
                                    //        var val = db.LookupValue.Find(int.Parse(Category));
                                    //        c.SessionType = val;

                                    //        var type = db.OtherSalaryHead.Include(e => e.SessionType).Where(e => e.Id == data).SingleOrDefault();
                                    //        IList<OtherSalaryHead> typedetails = null;
                                    //        if (type.SessionType != null)
                                    //        {
                                    //            typedetails = db.OtherSalaryHead.Where(x => x.SessionType.Id == type.SessionType.Id && x.Id == data).ToList();
                                    //        }
                                    //        else
                                    //        {
                                    //            typedetails = db.OtherSalaryHead.Where(x => x.Id == data).ToList();
                                    //        }
                                    //        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                    //        foreach (var s in typedetails)
                                    //        {
                                    //            s.SessionType = c.SessionType;
                                    //            db.OtherSalaryHead.Attach(s);
                                    //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //            //await db.SaveChangesAsync();
                                    //            db.SaveChanges();
                                    //            TempData["RowVersion"] = s.RowVersion;
                                    //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    //        }
                                    //    }
                                    //    else
                                    //    {
                                    //        var BusiTypeDetails = db.OtherSalaryHead.Include(e => e.SessionType).Where(x => x.Id == data).ToList();
                                    //        foreach (var s in BusiTypeDetails)
                                    //        {
                                    //            s.SessionType = null;
                                    //            db.OtherSalaryHead.Attach(s);
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
                                    //    var BusiTypeDetails = db.OtherSalaryHead.Include(e => e.SessionType).Where(x => x.Id == data).ToList();
                                    //    foreach (var s in BusiTypeDetails)
                                    //    {
                                    //        s.SessionType = null;
                                    //        db.OtherSalaryHead.Attach(s);
                                    //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //        //await db.SaveChangesAsync();
                                    //        db.SaveChanges();
                                    //        TempData["RowVersion"] = s.RowVersion;
                                    //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    //    }
                                    //}
                                    if (RoundingMethod != null)
                                    {
                                        if (RoundingMethod != "")
                                        {
                                            var val = db.LookupValue.Find(int.Parse(RoundingMethod));
                                            c.RoundingMethod = val;

                                            var type = db.OtherSalaryHead.Include(e => e.RoundingMethod)
                                                .Where(e => e.Id == data).SingleOrDefault();
                                            IList<OtherSalaryHead> typedetails = null;
                                            if (type.RoundingMethod != null)
                                            {
                                                typedetails = db.OtherSalaryHead.Where(x => x.RoundingMethod.Id == type.RoundingMethod.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.OtherSalaryHead.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.RoundingMethod = c.RoundingMethod;
                                                db.OtherSalaryHead.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var Dtls = db.OtherSalaryHead.Include(e => e.RoundingMethod).Where(x => x.Id == data).ToList();
                                            foreach (var s in Dtls)
                                            {
                                                s.RoundingMethod = null;
                                                db.OtherSalaryHead.Attach(s);
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
                                        var Dtls = db.OtherSalaryHead.Include(e => e.RoundingMethod).Where(x => x.Id == data).ToList();
                                        foreach (var s in Dtls)
                                        {
                                            s.RoundingMethod = null;
                                            db.OtherSalaryHead.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }



                                    if (SalaryType != null)
                                    {
                                        if (SalaryType != "")
                                        {
                                            var val = db.LookupValue.Find(int.Parse(SalaryType));
                                            c.SalaryType = val;

                                            var type = db.OtherSalaryHead
                                                .Include(e => e.SalaryType)
                                                .Where(e => e.Id == data).SingleOrDefault();
                                            IList<OtherSalaryHead> typedetails = null;
                                            if (type.SalaryType != null)
                                            {
                                                typedetails = db.OtherSalaryHead.Where(x => x.SalaryType.Id == type.SalaryType.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.OtherSalaryHead.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.SalaryType = c.SalaryType;
                                                db.OtherSalaryHead.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var Dtls = db.OtherSalaryHead.Include(e => e.SalaryType).Where(x => x.Id == data).ToList();
                                            foreach (var s in Dtls)
                                            {
                                                s.SalaryType = null;
                                                db.OtherSalaryHead.Attach(s);
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
                                        var Dtls = db.OtherSalaryHead.Include(e => e.SalaryType).Where(x => x.Id == data).ToList();
                                        foreach (var s in Dtls)
                                        {
                                            s.SalaryType = null;
                                            db.OtherSalaryHead.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }


                                    //if (Frequency != null)
                                    //{
                                    //    if (Frequency != "")
                                    //    {
                                    //        var val = db.LookupValue.Find(int.Parse(Frequency));
                                    //        c.Frequency = val;
                                    //        var type = db.OtherSalaryHead
                                    //            .Include(e => e.Frequency)
                                    //            .Where(e => e.Id == data).SingleOrDefault();
                                    //        IList<OtherSalaryHead> typedetails = null;
                                    //        if (type.Frequency != null)
                                    //        {
                                    //            typedetails = db.OtherSalaryHead.Where(x => x.Frequency.Id == type.Frequency.Id && x.Id == data).ToList();
                                    //        }
                                    //        else
                                    //        {
                                    //            typedetails = db.OtherSalaryHead.Where(x => x.Id == data).ToList();
                                    //        }
                                    //        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                    //        foreach (var s in typedetails)
                                    //        {
                                    //            //s.Frequency = c.Frequency;
                                    //            db.OtherSalaryHead.Attach(s);
                                    //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //            //await db.SaveChangesAsync();
                                    //            db.SaveChanges();
                                    //            TempData["RowVersion"] = s.RowVersion;
                                    //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    //        }
                                    //    }
                                    //    else
                                    //    {
                                    //        var Dtls = db.OtherSalaryHead.Include(e => e.).Where(x => x.Id == data).ToList();
                                    //        foreach (var s in Dtls)
                                    //        {
                                    //           // s.Frequency = null;
                                    //            db.OtherSalaryHead.Attach(s);
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
                                    //    var Dtls = db.OtherSalaryHead.Include(e => e.Frequency).Where(x => x.Id == data).ToList();
                                    //    foreach (var s in Dtls)
                                    //    {
                                    //        s.Frequency = null;
                                    //        db.OtherSalaryHead.Attach(s);
                                    //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //        //await db.SaveChangesAsync();
                                    //        db.SaveChanges();
                                    //        TempData["RowVersion"] = s.RowVersion;
                                    //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    //    }
                                    //}


                                    var m1 = db.OtherSalaryHead.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.OtherSalaryHead.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    var CurCorp = db.OtherSalaryHead.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        OtherSalaryHead corp = new OtherSalaryHead()
                                        {
                                            SalaryCode = c.SalaryCode,
                                            SalaryType = c.SalaryType,
                                            InPayslip = c.InPayslip,
                                            //Frequency = c.Frequency,
                                            SalaryDesc = c.SalaryDesc,
                                            RoundDigit = c.RoundDigit,
                                            RoundingMethod = c.RoundingMethod,
                                            InITax = c.InITax,
                                            DBTrack = c.DBTrack,
                                            Id = data
                                            // RowVersion = (Byte[])TempData["RowVersion"]
                                        };


                                        db.OtherSalaryHead.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    }

                                    using (var context = new DataBaseContext())
                                    {
                                        //    c.Id = data;



                                        //    var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        //    DT_PerkHead DT_Corp = (DT_PerkHead)obj;
                                        //    DT_Corp.Frequency_Id = blog.Frequency == null ? 0 : blog.Frequency.Id;
                                        //    DT_Corp.PerkType_Id = blog.SalaryType == null ? 0 : blog.SalaryType.Id;
                                        //    DT_Corp.RoundingMethod_Id = blog.RoundingMethod == null ? 0 : blog.RoundingMethod.Id;

                                        //    db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();



                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.RoundingMethod.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (OtherSalaryHead)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {

                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    var databaseValues = (OtherSalaryHead)databaseEntry.ToObject();
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

                            OtherSalaryHead blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            OtherSalaryHead Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.OtherSalaryHead.Where(e => e.Id == data).SingleOrDefault();
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

                            OtherSalaryHead corp = new OtherSalaryHead()
                            {
                                SalaryCode = c.SalaryCode,
                                SalaryType = c.SalaryType,
                                InPayslip = c.InPayslip,
                                // Frequency = c.Frequency,
                                SalaryDesc = c.SalaryDesc,
                                RoundDigit = c.RoundDigit,
                                RoundingMethod = c.RoundingMethod,
                                InITax = c.InITax,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"],
                                Id = data
                                // RowVersion = (Byte[])TempData["RowVersion"]
                            };

                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            //using (var context = new DataBaseContext())
                            //{
                            //    var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "Traiing Session", c.DBTrack);
                            //    // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                            //    Old_Corp = context.OtherSalaryHead.Where(e => e.Id == data).Include(e => e.Frequency)
                            //        .Include(e => e.SalaryType).SingleOrDefault();
                            //    DT_PerkHead DT_Corp = (DT_PerkHead)obj;
                            //  //  DT_Corp.Faculty_Id = DBTrackFile.ValCompare(Old_Corp.Faculty, c.Faculty);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                            //  //  DT_Corp.SessionType_Id = DBTrackFile.ValCompare(Old_Corp.SessionType, c.SessionType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;

                            //    db.Create(DT_Corp);
                            //    //db.SaveChanges();
                            //}
                            blog.DBTrack = c.DBTrack;
                            db.OtherSalaryHead.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();

                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.RoundingMethod.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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


        public int EditS(string RMVal, string PerkHVal, int data, OtherSalaryHead ESOBJ, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (RMVal != null)
                {
                    if (RMVal != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(RMVal));
                        ESOBJ.RoundingMethod = val;

                        var type = db.OtherSalaryHead.Include(e => e.RoundingMethod)
                            .Where(e => e.Id == data).SingleOrDefault();
                        IList<OtherSalaryHead> typedetails = null;
                        if (type.RoundingMethod != null)
                        {
                            typedetails = db.OtherSalaryHead.Where(x => x.RoundingMethod.Id == type.RoundingMethod.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.OtherSalaryHead.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.RoundingMethod = ESOBJ.RoundingMethod;
                            db.OtherSalaryHead.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.OtherSalaryHead.Include(e => e.RoundingMethod).Where(x => x.Id == data).ToList();
                        foreach (var s in Dtls)
                        {
                            s.RoundingMethod = null;
                            db.OtherSalaryHead.Attach(s);
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
                    var Dtls = db.OtherSalaryHead.Include(e => e.RoundingMethod).Where(x => x.Id == data).ToList();
                    foreach (var s in Dtls)
                    {
                        s.RoundingMethod = null;
                        db.OtherSalaryHead.Attach(s);
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
                        ESOBJ.SalaryType = val;

                        var type = db.OtherSalaryHead
                            .Include(e => e.SalaryType)
                            .Where(e => e.Id == data).SingleOrDefault();
                        IList<OtherSalaryHead> typedetails = null;
                        if (type.SalaryType != null)
                        {
                            typedetails = db.OtherSalaryHead.Where(x => x.SalaryType.Id == type.SalaryType.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.OtherSalaryHead.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.SalaryType = ESOBJ.SalaryType;
                            db.OtherSalaryHead.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.OtherSalaryHead.Include(e => e.SalaryType).Where(x => x.Id == data).ToList();
                        foreach (var s in Dtls)
                        {
                            s.SalaryType = null;
                            db.OtherSalaryHead.Attach(s);
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
                    var Dtls = db.OtherSalaryHead.Include(e => e.SalaryType).Where(x => x.Id == data).ToList();
                    foreach (var s in Dtls)
                    {
                        s.SalaryType = null;
                        db.OtherSalaryHead.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }






                var CurOBJ = db.OtherSalaryHead.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    ESOBJ.DBTrack = dbT;
                    OtherSalaryHead ESIOBJ = new OtherSalaryHead()
                    {
                        Id = data,
                        SalaryDesc = ESOBJ.SalaryDesc,
                        RoundDigit = ESOBJ.RoundDigit,
                        SalaryCode = ESOBJ.SalaryCode,
                        InITax = ESOBJ.InITax,
                        InPayslip = ESOBJ.InPayslip,
                        DBTrack = ESOBJ.DBTrack
                    };

                    db.OtherSalaryHead.Attach(ESIOBJ);
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
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                OtherSalaryHead OtherSalaryHead = db.OtherSalaryHead.Where(e => e.Id == data).SingleOrDefault();
                try
                {
                    //var selectedValues = OtherSalaryHead.SocialActivities;
                    //var lkValue = new HashSet<int>(OtherSalaryHead.SocialActivities.Select(e => e.Id));
                    //if (lkValue.Count > 0)
                    //{
                    //    return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                    //}

                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(OtherSalaryHead).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    //  return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                }

                catch (DataException /* dex */)
                {
                    //  return this.Json(new Object[] { "", "Unable to delete. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
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
                var LKVal = db.OtherSalaryHead.ToList();

                if (gp.IsAutho == true)
                {
                    LKVal = db.OtherSalaryHead.Include(e => e.SalaryType).Include(e => e.RoundingMethod).AsNoTracking().ToList();
                }
                else
                {
                    LKVal = db.OtherSalaryHead.Include(e => e.SalaryType).Include(e => e.RoundingMethod).AsNoTracking().ToList();
                }


                IEnumerable<OtherSalaryHead> IE;
                if (!string.IsNullOrEmpty(gp.searchString))
                {
                    IE = LKVal;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString)) || (e.SalaryCode.ToUpper().Contains(gp.searchString.ToUpper())) || (e.SalaryDesc.ToLower().Contains(gp.searchString.ToLower()))
                            || (e.SalaryType.LookupVal.ToLower().Contains(gp.searchString.ToLower()))
                            || (e.RoundingMethod.LookupVal.ToLower().Contains(gp.searchString.ToLower()))
                            || (e.RoundDigit.ToString().Contains(gp.searchString))).Select(a => new Object[] { a.Id, a.SalaryCode, a.SalaryDesc, a.SalaryType.LookupVal, a.RoundingMethod.LookupVal, a.RoundDigit });
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.SalaryCode, a.SalaryDesc, a.SalaryType.LookupVal, a.RoundingMethod.LookupVal, a.RoundDigit }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = LKVal;
                    Func<OtherSalaryHead, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "SalaryCode" ? c.SalaryCode.ToString() :
                                        gp.sidx == "SalaryDesc" ? c.SalaryDesc.ToString() :
                                         gp.sidx == "SalaryType" ? c.SalaryType.LookupVal.ToString() :
                                         gp.sidx == "RoundingMethod" ? c.RoundingMethod.LookupVal.ToString() :
                                         gp.sidx == "RoundDigit " ? c.RoundDigit.ToString() :
                                         "");
                    }

                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryCode, a.SalaryDesc, a.SalaryType.LookupVal, a.RoundingMethod.LookupVal, a.RoundDigit }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryCode, a.SalaryDesc, a.SalaryType.LookupVal, a.RoundingMethod.LookupVal, a.RoundDigit }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.SalaryCode, a.SalaryDesc, a.SalaryType.LookupVal, a.RoundingMethod.LookupVal, a.RoundDigit }).ToList();
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
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            OtherSalaryHead ESI = db.OtherSalaryHead
                                .Include(e => e.RoundingMethod)
                                .Include(e => e.SalaryType)
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

                            db.OtherSalaryHead.Attach(ESI);
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(ESI).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, ESI.DBTrack);
                            DT_OtherSalaryHead DT_OBJ = (DT_OtherSalaryHead)rtn_Obj;

                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            //  return Json(new Object[] { ESI.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = ESI.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else if (auth_action == "M")
                    {

                        OtherSalaryHead Old_OBJ = db.OtherSalaryHead
                                                .Include(e => e.RoundingMethod)
                                                .Include(e => e.SalaryType)
                                                .Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_OtherSalaryHead Curr_OBJ = db.DT_OtherSalaryHead
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_OBJ != null)
                        {
                            OtherSalaryHead OtherSalaryHead = new OtherSalaryHead();
                            string RoundingMethod = Curr_OBJ.RoundingMethod_Id == null ? null : Curr_OBJ.RoundingMethod_Id.ToString();
                            string SalaryType = Curr_OBJ.SalaryType_Id == null ? null : Curr_OBJ.SalaryType_Id.ToString();
                            OtherSalaryHead.SalaryCode = Curr_OBJ.SalaryCode == null ? Old_OBJ.SalaryCode : Curr_OBJ.SalaryCode;
                            OtherSalaryHead.SalaryDesc = Curr_OBJ.SalaryDesc == null ? Old_OBJ.SalaryDesc : Curr_OBJ.SalaryDesc;
                            OtherSalaryHead.RoundDigit = Curr_OBJ.RoundDigit == 0 ? Old_OBJ.RoundDigit : Curr_OBJ.RoundDigit;
                            OtherSalaryHead.InITax = Curr_OBJ.InITax == null ? Old_OBJ.InITax : Curr_OBJ.InITax;
                            OtherSalaryHead.InPayslip = Curr_OBJ.InPayslip == null ? Old_OBJ.InPayslip : Curr_OBJ.InPayslip;

                            //      corp.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        OtherSalaryHead.DBTrack = new DBTrack
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

                                        int a = EditS(RoundingMethod, SalaryType, auth_id, OtherSalaryHead, OtherSalaryHead.DBTrack);

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        // return Json(new Object[] { OtherSalaryHead.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = OtherSalaryHead.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (OtherSalaryHead)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                    else
                                    {
                                        var databaseValues = (OtherSalaryHead)databaseEntry.ToObject();
                                        OtherSalaryHead.RowVersion = databaseValues.RowVersion;
                                    }
                                }

                                //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }
                        else
                            //    return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //OtherSalaryHead corp = db.OtherSalaryHead.Find(auth_id);
                            OtherSalaryHead ESI = db.OtherSalaryHead.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

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

                            db.OtherSalaryHead.Attach(ESI);
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, ESI.DBTrack);
                            DT_OtherSalaryHead DT_OBJ = (DT_OtherSalaryHead)rtn_Obj;
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
