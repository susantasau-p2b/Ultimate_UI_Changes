
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
using EMS;

namespace P2BUltimate.Controllers
{
    public class SeperationMasterController : Controller
    {
        //
        // GET: /SeperationMaster/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/SeperationMaster/Index.cshtml");
        }

        public ActionResult Create(SeperationMaster c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string typeofseparation = form["TypeOfSeperationList"] == "0" ? "" : form["TypeOfSeperationList"];
                string subtypeofseparation = form["SubTypeOfSeperationList"] == "0" ? "" : form["SubTypeOfSeperationList"];
               
                List<String> Msg = new List<String>();
                if (typeofseparation == null || typeofseparation == "")
                {
                    Msg.Add(" Kindly select Type of Seperation ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                if (subtypeofseparation == null || subtypeofseparation == "")
                {
                    Msg.Add(" Kindly select Sub Type of Seperation");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                try
                {
                    if (typeofseparation != null)
                    {
                        if (typeofseparation != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(typeofseparation));
                            c.TypeOfSeperation = val;
                        }
                    }

                    if (subtypeofseparation != null)
                    {
                        if (subtypeofseparation != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(subtypeofseparation));
                            c.SubTypeOfSeperation = val;
                        }
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            SeperationMaster sep = new SeperationMaster()
                            {
                                TypeOfSeperation = c.TypeOfSeperation,
                                SubTypeOfSeperation = c.SubTypeOfSeperation,
                                Narration = c.Narration,
                                DBTrack = c.DBTrack
                            };

                            db.SeperationMaster.Add(sep);
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("Data Saved Successfully.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                        Msg.Add("Code Already Exists.");
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
                var LKVal = db.SeperationMaster.ToList();

                if (gp.IsAutho == true)
                {
                    LKVal = db.SeperationMaster
                         .Include(e => e.TypeOfSeperation)
                         .Include(e => e.SubTypeOfSeperation)
                         .Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    LKVal = db.SeperationMaster
                         .Include(e => e.TypeOfSeperation)
                         .Include(e => e.SubTypeOfSeperation)
                         .Where(e => e.DBTrack.IsModified == false).AsNoTracking().ToList();
                }


                IEnumerable<SeperationMaster> IE;
                if (!string.IsNullOrEmpty(gp.searchString))
                {
                    IE = LKVal;
                    if (gp.searchOper.Equals("eq"))
                    {

                        jsonData = IE.Where(e => (e.TypeOfSeperation.LookupVal.ToUpper().Contains(gp.searchString.ToUpper()))
                             || (e.SubTypeOfSeperation.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                             || (e.Narration.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.TypeOfSeperation.LookupVal, a.SubTypeOfSeperation.LookupVal, a.Narration, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.TypeOfSeperation.LookupVal, a.SubTypeOfSeperation.LookupVal, a.Narration, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = LKVal;
                    Func<SeperationMaster, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>
                                         gp.sidx == "TypeOfSeperation" ? c.TypeOfSeperation.LookupVal :
                                         gp.sidx == "SubTypeOfSeperation" ? c.SubTypeOfSeperation.LookupVal :
                                         gp.sidx == "Narration" ? c.Narration :
                                         "");
                    }

                    //Func<BasicScale, string> orderfuc = (c =>
                    //                                           gp.sidx == "Id" ? c.Id.ToString() :
                    //                                           gp.sidx == "Code" ? c.Code :
                    //                                           gp.sidx == "ScaleName" ? c.ScaleName : "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.TypeOfSeperation.LookupVal, a.SubTypeOfSeperation.LookupVal, a.Narration, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.TypeOfSeperation.LookupVal, a.SubTypeOfSeperation.LookupVal, a.Narration, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.TypeOfSeperation.LookupVal, a.SubTypeOfSeperation.LookupVal, a.Narration, a.Id }).ToList();
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
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.SeperationMaster
                 .Include(e => e.TypeOfSeperation)
                 .Include(e => e.SubTypeOfSeperation)
                 .Where(e => e.Id == data).Select
                 (e => new
                 {
                     TypeOfSeperation_id = e.TypeOfSeperation.Id,
                     SubTypeOfSeperation = e.SubTypeOfSeperation.Id,
                     Narration = e.Narration
                 }).ToList();

                var Corp = db.SeperationMaster.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, Auth, JsonRequestBehavior.AllowGet });

            }

        }
        [HttpPost]
        public async Task<ActionResult> EditSave(SeperationMaster c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string typeofseparation = form["TypeOfSeperationList"] == "0" ? "" : form["TypeOfSeperationList"];
                    string subtypeofseparation = form["SubTypeOfSeperationList"] == "0" ? "" : form["SubTypeOfSeperationList"];
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;



                    if (typeofseparation != null)
                    {
                        if (typeofseparation != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(typeofseparation));
                            c.TypeOfSeperation = val;

                            var type = db.SeperationMaster.Include(e => e.TypeOfSeperation).Where(e => e.Id == data).SingleOrDefault();
                            IList<SeperationMaster> typedetails = null;
                            if (type.TypeOfSeperation != null)
                            {
                                typedetails = db.SeperationMaster.Where(x => x.TypeOfSeperation.Id == type.TypeOfSeperation.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.SeperationMaster.Where(x => x.Id == data).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.TypeOfSeperation = c.TypeOfSeperation;
                                db.SeperationMaster.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }

                    if (subtypeofseparation != null)
                    {
                        if (subtypeofseparation != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(subtypeofseparation));
                            c.SubTypeOfSeperation = val;

                            var type = db.SeperationMaster.Include(e => e.SubTypeOfSeperation).Where(e => e.Id == data).SingleOrDefault();
                            IList<SeperationMaster> typedetails = null;
                            if (type.SubTypeOfSeperation != null)
                            {
                                typedetails = db.SeperationMaster.Where(x => x.SubTypeOfSeperation.Id == type.SubTypeOfSeperation.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.SeperationMaster.Where(x => x.Id == data).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.SubTypeOfSeperation = c.SubTypeOfSeperation;
                                db.SeperationMaster.Attach(s);
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
                                    SeperationMaster blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.SeperationMaster.Where(e => e.Id == data).Include(e => e.SubTypeOfSeperation)
                                                                                    .Include(e => e.TypeOfSeperation)
                                                                .AsNoTracking().SingleOrDefault();
                                        //originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    var m1 = db.SeperationMaster.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.SeperationMaster.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    //int a = EditS(Category, PromoPolicy, data, c, c.DBTrack);
                                    var CurCorp = db.SeperationMaster.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {


                                        SeperationMaster corp = new SeperationMaster()
                                        {
                                            TypeOfSeperation = c.TypeOfSeperation,
                                            SubTypeOfSeperation = c.SubTypeOfSeperation,
                                            Narration = c.Narration,
                                            Id = data,
                                            DBTrack = c.DBTrack
                                        };

                                        db.SeperationMaster.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // return 1;
                                    }

                                    using (var context = new DataBaseContext())
                                    {
                                        //var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        //DT_SeperationMaster DT_Corp = (DT_IncrActivity)obj;
                                        //DT_Corp.IncrList_Id = blog.IncrList == null ? 0 : blog.IncrList.Id;
                                        //DT_Corp.IncrPolicy_Id = blog.IncrPolicy == null ? 0 : blog.IncrPolicy.Id;

                                        //db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass
                                    {
                                        Id = c.Id,
                                        Val = c.TypeOfSeperation.LookupVal.ToString(),
                                        success = true,
                                        responseText = Msg
                                    }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { c.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (PromoActivity)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (PromoActivity)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
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

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    SeperationMaster SeperationMaster = db.SeperationMaster.Include(e => e.TypeOfSeperation)
                                                                            .Include(e => e.SubTypeOfSeperation)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    db.Entry(SeperationMaster).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                    //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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
            }
        }



    }
}