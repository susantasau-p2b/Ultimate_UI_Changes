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
namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class CurrencyConversionController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /CurrencyConversion/

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/CurrencyConversion/Index.cshtml");
        }
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(CurrencyConversion COBJ, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string BaseCountry = form["BaseCountrylist"] == "0" ? "" : form["BaseCountrylist"];
                    if (BaseCountry == null || BaseCountry == "")
                    {
                        Msg.Add("  BaseCountry cannot be null  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new Object[] { null, null, "BaseCountry cannot be null", JsonRequestBehavior.AllowGet });
                    }

                    if (BaseCountry != null)
                    {
                        if (BaseCountry != "")
                        {
                            Country OBJ = db.Country.Find(Convert.ToInt32(BaseCountry));
                            COBJ.BaseCountry = OBJ;
                        }
                    }

                    string ConvertCountry = form["ConvertCountrylist"] == "0" ? "" : form["ConvertCountrylist"];
                    if (ConvertCountry == null || ConvertCountry == "")
                    {
                        Msg.Add(" ConvertCountry cannot be null  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new Object[] { null, null, "ConvertCountry cannot be null", JsonRequestBehavior.AllowGet });
                    }
                    if (ConvertCountry != null)
                    {
                        if (ConvertCountry != "")
                        {
                            Country OBJ = db.Country.Find(Convert.ToInt32(ConvertCountry));
                            COBJ.ConvertCountry = OBJ;
                        }
                    }




                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            if (db.CurrencyConversion.Any(o => o.BaseCountry.Id == COBJ.BaseCountry.Id) && db.CurrencyConversion.Any(o => o.ConvertCountry.Id == COBJ.ConvertCountry.Id))
                            {
                                //  return this.Json(new Object[] { null, null, "Record already exists.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Record Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }



                            COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            CurrencyConversion CurrencyConversion = new CurrencyConversion()
                            {
                                Name = COBJ.Name,
                                ConvertCountry = COBJ.ConvertCountry,
                                BaseCountry = COBJ.BaseCountry,
                                DBTrack = COBJ.DBTrack
                            };
                            try
                            {
                                db.CurrencyConversion.Add(CurrencyConversion);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, COBJ.DBTrack);
                                DT_CurrencyConversion DT_OBJ = (DT_CurrencyConversion)rtn_Obj;
                                DT_OBJ.ConvertCountry_Id = COBJ.ConvertCountry == null ? 0 : COBJ.ConvertCountry.Id;
                                DT_OBJ.BaseCountry_Id = COBJ.BaseCountry == null ? 0 : COBJ.BaseCountry.Id;
                                db.Create(DT_OBJ);
                                db.SaveChanges();

                                ts.Complete();
                                //   return this.Json(new Object[] { null, null, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Created successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = COBJ.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                //  return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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

                var CurrencyConversion = db.CurrencyConversion
                                    .Include(e => e.BaseCountry)
                                    .Include(e => e.ConvertCountry)
                                    .Where(e => e.Id == data).ToList();
                var r = (from ca in CurrencyConversion
                         select new
                         {

                             Id = ca.Id,
                             BaseCountry_Id = ca.BaseCountry.Id.ToString(),
                             ConvertCountry_Id = ca.ConvertCountry.Id.ToString(),
                             Name = ca.Name,
                             Action = ca.DBTrack.Action
                         }).Distinct();

                var a = "";

                var W = db.DT_CurrencyConversion
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Name = e.Name == null ? "" : e.Name,
                         ConvertCountry__Val = e.ConvertCountry_Id == 0 ? "" : db.Country.Where(x => x.Id == e.ConvertCountry_Id).Select(x => x.Name).FirstOrDefault(),
                         //ConvertCountry__Val = e.ConvertCountry_Id == null ? "" : e.ConvertCountry_Id.ToString(),
                         BaseCountry_Val = e.BaseCountry_Id == null ? "" : db.Country.Where(x => x.Id == e.BaseCountry_Id).Select(x => x.Name).FirstOrDefault(),
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var LKup = db.CurrencyConversion.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, a, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }




        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> EditSave(CurrencyConversion ESOBJ, FormCollection form, int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    String BaseCountry = "";
                    BaseCountry = form["BaseCountrylist"];
                    if (BaseCountry != null && BaseCountry != "" && BaseCountry != "0")
                    {
                        Country OBJ = db.Country.Find(Convert.ToInt32(BaseCountry));
                        ESOBJ.BaseCountry = OBJ;
                    }
                    String ConvertCountry = "";
                    ConvertCountry = form["ConvertCountrylist"];
                    if (ConvertCountry != null && ConvertCountry != "" && ConvertCountry != "0")
                    {
                        Country OBJ = db.Country.Find(Convert.ToInt32(ConvertCountry));
                        ESOBJ.ConvertCountry = OBJ;
                    }


                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    CurrencyConversion blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.CurrencyConversion.Where(e => e.Id == data)
                                                                .Include(e => e.BaseCountry)
                                                                .Include(e => e.ConvertCountry)
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

                                    //int a = EditS(BaseCountry, ConvertCountry, data, ESOBJ, ESOBJ.DBTrack);

                                    if (BaseCountry != null)
                                    {
                                        if (BaseCountry != "")
                                        {
                                            var val = db.Country.Find(int.Parse(BaseCountry));
                                            ESOBJ.BaseCountry = val;

                                            var add = db.CurrencyConversion.Include(e => e.BaseCountry).Where(e => e.Id == data).SingleOrDefault();
                                            IList<CurrencyConversion> GeoStructdetails = null;
                                            if (add.BaseCountry != null)
                                            {
                                                GeoStructdetails = db.CurrencyConversion.Where(x => x.BaseCountry.Id == add.BaseCountry.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                GeoStructdetails = db.CurrencyConversion.Where(x => x.Id == data).ToList();
                                            }
                                            if (GeoStructdetails != null)
                                            {
                                                foreach (var s in GeoStructdetails)
                                                {
                                                    s.BaseCountry = ESOBJ.BaseCountry;
                                                    db.CurrencyConversion.Attach(s);
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
                                        var GeoStructdetails = db.CurrencyConversion.Include(e => e.BaseCountry).Where(x => x.Id == data).ToList();
                                        foreach (var s in GeoStructdetails)
                                        {
                                            s.BaseCountry = null;
                                            db.CurrencyConversion.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    //***********************************************************************************************************
                                    //*************************** Code For One to One********************************************
                                    if (ConvertCountry != null)
                                    {
                                        if (ConvertCountry != "")
                                        {
                                            var val = db.Country.Find(int.Parse(ConvertCountry));
                                            ESOBJ.ConvertCountry = val;

                                            var add = db.CurrencyConversion.Include(e => e.ConvertCountry).Where(e => e.Id == data).SingleOrDefault();
                                            IList<CurrencyConversion> ConvertCountry1 = null;
                                            if (add.ConvertCountry != null)
                                            {
                                                ConvertCountry1 = db.CurrencyConversion.Where(x => x.ConvertCountry.Id == add.ConvertCountry.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                ConvertCountry1 = db.CurrencyConversion.Where(x => x.Id == data).ToList();
                                            }
                                            foreach (var s in ConvertCountry1)
                                            {
                                                s.ConvertCountry = ESOBJ.ConvertCountry;
                                                db.CurrencyConversion.Attach(s);
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
                                        var ConvertCountry1 = db.CurrencyConversion.Include(e => e.ConvertCountry).Where(x => x.Id == data).ToList();
                                        foreach (var s in ConvertCountry1)
                                        {
                                            s.ConvertCountry = null;
                                            db.CurrencyConversion.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    //***********************************************************************************************************

                                    var CurOBJ = db.CurrencyConversion.Find(data);
                                    TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                                    db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        CurrencyConversion ESIOBJ = new CurrencyConversion()
                                        {
                                            Id = data,
                                            Name = ESOBJ.Name,
                                            DBTrack = ESOBJ.DBTrack
                                        };

                                        db.CurrencyConversion.Attach(ESIOBJ);
                                        db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                    }

                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                        DT_CurrencyConversion DT_OBJ = (DT_CurrencyConversion)obj;
                                        DT_OBJ.ConvertCountry_Id = blog.ConvertCountry == null ? 0 : blog.ConvertCountry.Id;
                                        DT_OBJ.BaseCountry_Id = blog.BaseCountry == null ? 0 : blog.BaseCountry.Id;
                                        db.Create(DT_OBJ);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();


                                    //  return Json(new Object[] { ESOBJ.Id, ESOBJ.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }

                            //catch (DbUpdateException e) { throw e; }
                            //catch (DataException e) { throw e; }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (CurrencyConversion)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var databaseValues = (CurrencyConversion)databaseEntry.ToObject();
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
                        }
                    }


                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            CurrencyConversion blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            CurrencyConversion Old_Obj = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.CurrencyConversion.Where(e => e.Id == data).SingleOrDefault();
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
                            CurrencyConversion corp = new CurrencyConversion()
                            {
                                Id = data,
                                DBTrack = ESOBJ.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "CurrencyConversion", ESOBJ.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("CCore/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Obj = context.CurrencyConversion.Where(e => e.Id == data).Include(e => e.BaseCountry)
                                    .Include(e => e.ConvertCountry).SingleOrDefault();
                                DT_CurrencyConversion DT_OBJ = (DT_CurrencyConversion)obj;
                                DT_OBJ.ConvertCountry_Id = DBTrackFile.ValCompare(Old_Obj.ConvertCountry, ESOBJ.ConvertCountry);//Old_Obj.Address == c.Address ? 0 : Old_Obj.Address == null && c.Address != null ? c.Address.Id : Old_Obj.Address.Id;
                                DT_OBJ.BaseCountry_Id = DBTrackFile.ValCompare(Old_Obj.BaseCountry, ESOBJ.BaseCountry); //Old_Obj.BusinessType == c.BusinessType ? 0 : Old_Obj.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Obj.BusinessType.Id;                        

                                db.Create(DT_OBJ);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = ESOBJ.DBTrack;
                            db.CurrencyConversion.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            //  return Json(new Object[] { blog.Id, ESOBJ.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = ESOBJ.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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



        public int EditS(string GEO, string FUNC, int data, CurrencyConversion ESOBJ, DBTrack dbT)
        {
            //*************************** Code For One to One********************************************
            using (DataBaseContext db = new DataBaseContext())
            {
                if (GEO != null)
                {
                    if (GEO != "")
                    {
                        var val = db.Country.Find(int.Parse(GEO));
                        ESOBJ.BaseCountry = val;

                        var add = db.CurrencyConversion.Include(e => e.BaseCountry).Where(e => e.Id == data).SingleOrDefault();
                        IList<CurrencyConversion> GeoStructdetails = null;
                        if (add.BaseCountry != null)
                        {
                            GeoStructdetails = db.CurrencyConversion.Where(x => x.BaseCountry.Id == add.BaseCountry.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            GeoStructdetails = db.CurrencyConversion.Where(x => x.Id == data).ToList();
                        }
                        if (GeoStructdetails != null)
                        {
                            foreach (var s in GeoStructdetails)
                            {
                                s.BaseCountry = ESOBJ.BaseCountry;
                                db.CurrencyConversion.Attach(s);
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
                    var GeoStructdetails = db.CurrencyConversion.Include(e => e.BaseCountry).Where(x => x.Id == data).ToList();
                    foreach (var s in GeoStructdetails)
                    {
                        s.BaseCountry = null;
                        db.CurrencyConversion.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                //***********************************************************************************************************
                //*************************** Code For One to One********************************************
                if (FUNC != null)
                {
                    if (FUNC != "")
                    {
                        var val = db.Country.Find(int.Parse(FUNC));
                        ESOBJ.ConvertCountry = val;

                        var add = db.CurrencyConversion.Include(e => e.ConvertCountry).Where(e => e.Id == data).SingleOrDefault();
                        IList<CurrencyConversion> ConvertCountry = null;
                        if (add.ConvertCountry != null)
                        {
                            ConvertCountry = db.CurrencyConversion.Where(x => x.ConvertCountry.Id == add.ConvertCountry.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            ConvertCountry = db.CurrencyConversion.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in ConvertCountry)
                        {
                            s.ConvertCountry = ESOBJ.ConvertCountry;
                            db.CurrencyConversion.Attach(s);
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
                    var ConvertCountry = db.CurrencyConversion.Include(e => e.ConvertCountry).Where(x => x.Id == data).ToList();
                    foreach (var s in ConvertCountry)
                    {
                        s.ConvertCountry = null;
                        db.CurrencyConversion.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                //***********************************************************************************************************





                var CurOBJ = db.CurrencyConversion.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    ESOBJ.DBTrack = dbT;
                    CurrencyConversion ESIOBJ = new CurrencyConversion()
                    {
                        Id = data,
                        Name = ESOBJ.Name,
                        DBTrack = ESOBJ.DBTrack
                    };

                    db.CurrencyConversion.Attach(ESIOBJ);
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
                try
                {
                    CurrencyConversion CurrencyConversion = db.CurrencyConversion.Where(e => e.Id == data).SingleOrDefault();
                    try
                    {
                        //var selectedValues = CurrencyConversion.SocialActivities;
                        //var lkValue = new HashSet<int>(CurrencyConversion.SocialActivities.Select(e => e.Id));
                        //if (lkValue.Count > 0)
                        //{
                        //    return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                        //}

                        using (TransactionScope ts = new TransactionScope())
                        {
                            db.Entry(CurrencyConversion).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            ts.Complete();
                        }
                        //   return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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

        public ActionResult PopulateDropDownListBC(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Country.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult PopulateDropDownListCC(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Country.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
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
                var LKVal = db.CurrencyConversion.ToList();

                if (gp.IsAutho == true)
                {
                    LKVal = db.CurrencyConversion.Include(e => e.BaseCountry).Include(e => e.ConvertCountry).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    LKVal = db.CurrencyConversion.Include(e => e.BaseCountry).Include(e => e.ConvertCountry).AsNoTracking().ToList();
                }


                IEnumerable<CurrencyConversion> IE;
                if (!string.IsNullOrEmpty(gp.searchString))
                {
                    IE = LKVal;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Name.ToUpper().Contains(gp.searchString.ToUpper()))
                            //|| (e.LocationObj.LocDesc.ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.Name, a.Id }).ToList();

                        //jsonData = IE.Select(a => new { a.Name, a.Id }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Name.ToString() == gp.searchString.ToLower())));
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;


                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Name, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = LKVal;
                    Func<CurrencyConversion, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>
                                         gp.sidx == "Name" ? c.Name.ToString() :

                                         "");
                    }

                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Name, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Name, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Name, a.Id }).ToList();
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
                            CurrencyConversion ESI = db.CurrencyConversion
                                .Include(e => e.ConvertCountry)
                                .Include(e => e.BaseCountry)
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

                            db.CurrencyConversion.Attach(ESI);
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(ESI).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, ESI.DBTrack);
                            DT_CurrencyConversion DT_OBJ = (DT_CurrencyConversion)rtn_Obj;

                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            // return Json(new Object[] { ESI.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = ESI.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else if (auth_action == "M")
                    {

                        CurrencyConversion Old_OBJ = db.CurrencyConversion
                                                .Include(e => e.ConvertCountry)
                                .Include(e => e.BaseCountry)
                                                .Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_CurrencyConversion Curr_OBJ = db.DT_CurrencyConversion
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_OBJ != null)
                        {
                            CurrencyConversion CurrencyConversion = new CurrencyConversion();
                            string ConvertCountry = Curr_OBJ.ConvertCountry_Id == null ? null : Curr_OBJ.ConvertCountry_Id.ToString();
                            string BaseCountry = Curr_OBJ.BaseCountry_Id == null ? null : Curr_OBJ.BaseCountry_Id.ToString();

                            CurrencyConversion.Name = Curr_OBJ.Name == null ? Old_OBJ.Name : Curr_OBJ.Name;

                            //      corp.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        CurrencyConversion.DBTrack = new DBTrack
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

                                        int a = EditS(BaseCountry, ConvertCountry, auth_id, CurrencyConversion, CurrencyConversion.DBTrack);

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        //    return Json(new Object[] { CurrencyConversion.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = CurrencyConversion.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (CurrencyConversion)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                    else
                                    {
                                        var databaseValues = (CurrencyConversion)databaseEntry.ToObject();
                                        CurrencyConversion.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


                                //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                            Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //  return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //CurrencyConversion corp = db.Currency_Conversion.Find(auth_id);
                            CurrencyConversion ESI = db.CurrencyConversion.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

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

                            db.CurrencyConversion.Attach(ESI);
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, ESI.DBTrack);
                            DT_CurrencyConversion DT_OBJ = (DT_CurrencyConversion)rtn_Obj;
                            //DT_OBJ.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            //DT_OBJ.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                            //DT_OBJ.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            //   return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
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
