using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
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


namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class HotelEligibilitypolicyController : Controller
    {
        //
        // GET: /HotelElligibilitypolicy/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/HotelEligibilitypolicy/Index.cshtml");
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
                    var Tds = db.HotelEligibilityPolicy.Include(e => e.HotelType).Include(e => e.RoomType).ToList();
                    IEnumerable<HotelEligibilityPolicy> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = Tds;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                               || (e.HotelEligibilityCode.ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.HotelType.LookupVal.ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.RoomType.LookupVal.ToString().Contains(gp.searchString))
                               || (e.Lodging_Eligible_Amt_PerDay.ToString().Contains(gp.searchString))
                               || (e.Food_Eligible_Amt_PerDay.ToString().ToUpper().Contains(gp.searchString.ToUpper())
                               )
                               ).Select(a => new Object[] { a.HotelEligibilityCode, a.HotelType != null ? Convert.ToString(a.HotelType.LookupVal) : "", a.RoomType != null ? Convert.ToString(a.RoomType.LookupVal) : "", a.Lodging_Eligible_Amt_PerDay, a.Food_Eligible_Amt_PerDay, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.HotelEligibilityCode, a.HotelType != null ? Convert.ToString(a.HotelType.LookupVal) : "", a.RoomType != null ? Convert.ToString(a.RoomType.LookupVal) : "", a.Lodging_Eligible_Amt_PerDay, a.Food_Eligible_Amt_PerDay, a.Id }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = Tds;
                        Func<HotelEligibilityPolicy, string> orderfuc = (c =>
                                                                   gp.sidx == "HotelEligibilityCode" ? c.HotelEligibilityCode.ToString() :
                                                                   gp.sidx == "Lodging_Eligible_Amt_PerDay" ? Convert.ToString(c.Lodging_Eligible_Amt_PerDay) :
                                                                   gp.sidx == "Food_Eligible_Amt_PerDay" ? Convert.ToString(c.Food_Eligible_Amt_PerDay) :
                                                                   gp.sidx == "Id" ? c.Id.ToString() :
                                                                   "");
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.HotelEligibilityCode, a.HotelType != null ? Convert.ToString(a.HotelType.LookupVal) : "", a.RoomType != null ? Convert.ToString(a.RoomType.LookupVal) : "", a.Lodging_Eligible_Amt_PerDay, a.Food_Eligible_Amt_PerDay, a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.HotelEligibilityCode, a.HotelType != null ? Convert.ToString(a.HotelType.LookupVal) : "", a.RoomType != null ? Convert.ToString(a.RoomType.LookupVal) : "", a.Lodging_Eligible_Amt_PerDay, a.Food_Eligible_Amt_PerDay, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.HotelEligibilityCode, a.HotelType != null ? Convert.ToString(a.HotelType.LookupVal) : "", a.RoomType != null ? Convert.ToString(a.RoomType.LookupVal) : "", a.Lodging_Eligible_Amt_PerDay, a.Food_Eligible_Amt_PerDay, a.Id }).ToList();
                        }
                        totalRecords = Tds.Count();
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


        public ActionResult GetLookupDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                IEnumerable<HotelEligibilityPolicy> all;
                var fall = db.HotelEligibilityPolicy.ToList();
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.HotelEligibilityPolicy.ToList().Where(d => d.Lodging_Eligible_Amt_PerDay.ToString().Contains(data));
                    var result = (from c in all
                                  select new { c.Id, c.Lodging_Eligible_Amt_PerDay }).Distinct();
                    return Json(result, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.Lodging_Eligible_Amt_PerDay }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);

                }

            }
        }

        public ActionResult edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.HotelEligibilityPolicy
                    .Include(e => e.HotelType)
                     .Include(e => e.RoomType)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        HotelEligibilityCode = e.HotelEligibilityCode,
                        Lodging_Eligible_Amt_PerDay = e.Lodging_Eligible_Amt_PerDay,
                        Food_Eligible_Amt_PerDay = e.Food_Eligible_Amt_PerDay,
                        RoomType_Id = e.RoomType.Id == null ? 0 : e.RoomType.Id,
                        HotelType_Id = e.HotelType.Id == null ? 0 : e.HotelType.Id,
                        Action = e.DBTrack.Action
                    }).ToList();

                ////var lookupval = db.ITTDS.Include(e => e.Category)
                ////   .Where(e => e.Id == data).Select(e => new { Category_Id=e.Category.Id }).ToList();
                return Json(new Object[] { Q, "", "", JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create(HotelEligibilityPolicy td, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var HotelType_Id = form["HotelTypelist"];
                    int HotelTypeId = Convert.ToInt32(HotelType_Id);
                    if (HotelTypeId == 0)
                    {
                        Msg.Add("  Please Select Hotel Type.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { "", "Please Select Category.", JsonRequestBehavior.AllowGet });
                    }
                    var RoomType_Id = form["RoomTypelist"];
                    int RoomTypeId = Convert.ToInt32(RoomType_Id);
                    if (RoomTypeId == 0)
                    {
                        Msg.Add("  Please Select Room Type.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { "", "Please Select Category.", JsonRequestBehavior.AllowGet });
                    }


                    if (HotelType_Id != null && HotelType_Id != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(HotelType_Id));
                        td.HotelType = val;
                    }
                    if (RoomType_Id != null && RoomType_Id != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(RoomType_Id));
                        td.RoomType = val;
                    }


                    if (db.HotelEligibilityPolicy.Any(o => o.HotelEligibilityCode == td.HotelEligibilityCode && o.HotelType.Id == HotelTypeId))
                    {
                        Msg.Add("Hotel code,hoteltype  and room type already exists.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new object[] { null, null, "IncomeRangeFrom already exists.", JsonRequestBehavior.AllowGet });
                    }




                    if (ModelState.IsValid)
                    {
                        td.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        HotelEligibilityPolicy HotelEligibilityPolicy = new HotelEligibilityPolicy()
                        {
                            // IncomeFrom = td.IncomeFrom == null ? "" : td.IncomeFrom.Trim(),
                            HotelEligibilityCode = td.HotelEligibilityCode,
                            HotelType = td.HotelType,
                            RoomType = td.RoomType,
                            Lodging_Eligible_Amt_PerDay = td.Lodging_Eligible_Amt_PerDay,
                            Food_Eligible_Amt_PerDay = td.Food_Eligible_Amt_PerDay,
                            DBTrack = td.DBTrack
                        };
                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {

                                db.HotelEligibilityPolicy.Add(HotelEligibilityPolicy);
                                db.SaveChanges();
                                ts.Complete();
                            }
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = HotelEligibilityPolicy.Id, Val = HotelEligibilityPolicy.HotelEligibilityCode.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new object[]{tds.Id,tds.IncomeRangeFrom,"Data saved successfully." }, JsonRequestBehavior.AllowGet);
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = td.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                            //return View();
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

        //public ActionResult EditSave(HotelEligibilityPolicy td, int data, FormCollection form)
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            var HotelType_Id = form["HotelTypelist"];
        //            var RoomType_Id = form["RoomTypelist"];
        //            int HotelTypeId = Convert.ToInt32(HotelType_Id);
        //            int RoomTypeId = Convert.ToInt32(RoomType_Id);
        //            if (HotelType_Id != null && HotelType_Id != "")
        //            {
        //                var val = db.LookupValue.Find(int.Parse(HotelType_Id));
        //                td.HotelType = val;
        //            }
        //            if (RoomType_Id != null && RoomType_Id != "")
        //            {
        //                var val = db.LookupValue.Find(int.Parse(RoomType_Id));
        //                td.RoomType = val;
        //            }


        //            // string new_cat_id = form["Cat"] == "" ? "0" : form["Cat"];
        //            //var new_cat_value = db.LookupValue.Find(Int32.Parse(new_cat_id));
        //            //var cat_val = db.HotelEligibilityPolicy.Include(e => e.HotelType).Include(e => e.RoomType).Where(e => e.Id == data).Select(e => e.HotelType).SingleOrDefault();
        //            //td.HotelType = cat_val;
        //            //var cat_val1 = db.HotelEligibilityPolicy.Include(e => e.HotelType).Include(e => e.RoomType).Where(e => e.Id == data).Select(e => e.RoomType).SingleOrDefault();
        //            //td.RoomType = cat_val1;

        //            var db_data = db.HotelEligibilityPolicy.Include(e => e.HotelType).Include(e => e.RoomType).Where(e => e.Id == data).SingleOrDefault();

        //            db_data.HotelType = td.HotelType;
        //            db_data.RoomType = td.RoomType;
        //            db.HotelEligibilityPolicy.Attach(db_data);
        //            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

        //            db_data.Lodging_Eligible_Amt_PerDay = td.Lodging_Eligible_Amt_PerDay;
        //            db_data.Food_Eligible_Amt_PerDay = td.Food_Eligible_Amt_PerDay;

        //            try
        //            {
        //                if (ModelState.IsValid)
        //                {
        //                    using (TransactionScope ts = new TransactionScope())
        //                    {

        //                        db.HotelEligibilityPolicy.Attach(db_data);
        //                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
        //                        ts.Complete();
        //                        Msg.Add("  Data Updated successfully  ");
        //                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        // return Json(new object[]{null,null,"Data saved successfully."});
        //                    }
        //                }
        //            }
        //            catch (DbUpdateConcurrencyException)
        //            {
        //                return RedirectToAction("Edit", new { concurrencyError = true, id = td.Id });
        //            }
        //            catch (DataException)
        //            {
        //                Msg.Add(" Unable to Edit. Try again, and if the problem persists, see your system administrator");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //ModelState.AddModelError(string.Empty, "Unable to Edit. Try again, and if the problem persists contact your system administrator.");
        //                //return RedirectToAction("Edit");
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
        //[ValidateAntiForgeryToken]
        public ActionResult Delete(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                HotelEligibilityPolicy HEP = db.HotelEligibilityPolicy.Find(data);
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(HEP).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                }

                catch (DataException /* dex */)
                {
                    //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //  return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
                    //ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
                    //return RedirectToAction("Index");
                }
            }
        }
        public ActionResult getHoteleligibilitypolicydata(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var add = db.HotelEligibilityPolicy
                                      .Where(e => e.Id == data).SingleOrDefault();

                var r = (from ca in db.HotelEligibilityPolicy
                         select new
                         {
                             Id = ca.Id,
                             HotelEligibilityCode = ca.HotelEligibilityCode,
                             HotelType = ca.HotelType == null ? 0 : ca.HotelType.Id,
                             RoomType = ca.RoomType == null ? 0 : ca.RoomType.Id,
                             Lodging_Eligible_Amt_PerDay = ca.Lodging_Eligible_Amt_PerDay,
                             Food_Eligible_Amt_PerDay = ca.Food_Eligible_Amt_PerDay,
                         }).Where(e => e.Id == data).SingleOrDefault();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
        }

        [HttpPost]
        public ActionResult EditSave(HotelEligibilityPolicy data1, int data, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    //Calendar c = db.Calendar.Find(data);
                    string HotelType = form["HotelTypelist"] == "0" ? "" : form["HotelTypelist"];
                    string RoomType = form["RoomTypelist"] == "0" ? "" : form["RoomTypelist"];
                    //var Name = form["Name_drop"] == "0" ? 0 : Convert.ToInt32(form["Name_drop"]);

                    //if (Name != 0)
                    //{
                    //    data1.Name = db.LookupValue.Find(Name);
                    //}

                    var db_data = db.HotelEligibilityPolicy
                         .Include(q => q.HotelType)
                         .Include(q => q.RoomType)
                         .Where(a => a.Id == data).SingleOrDefault();
                    if (HotelType != null)
                    {
                        if (HotelType != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(HotelType));
                            db_data.HotelType = val;
                        }
                    }

                    if (RoomType != null)
                    {
                        if (RoomType != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(RoomType));
                            db_data.RoomType = val;
                        }
                    }
                    //var alrdy = db.Calendar.Include(a => a.Name).Where(e => e.Name.LookupVal.ToString().ToUpper() == db_data.Name.LookupVal.ToString().ToUpper() && e.Default == true && data1.Default == true).Count();

                    //if (alrdy > 0)
                    //{
                    //    Msg.Add("   Default  Year already exist. ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //    // return this.Json(new Object[] { "", "", "From Date already exists.", JsonRequestBehavior.AllowGet });
                    //}
                    data1.DBTrack = new DBTrack
                    {
                        CreatedBy = db_data.DBTrack.CreatedBy == null ? null : db_data.DBTrack.CreatedBy,
                        CreatedOn = db_data.DBTrack.CreatedOn == null ? null : db_data.DBTrack.CreatedOn,
                        Action = "M",
                        ModifiedBy = SessionManager.UserName,
                        ModifiedOn = DateTime.Now
                    };

                    db_data.HotelEligibilityCode = data1.HotelEligibilityCode;
                    db_data.HotelType = db_data.HotelType;
                    db_data.RoomType = db_data.RoomType;
                    db_data.Lodging_Eligible_Amt_PerDay = data1.Lodging_Eligible_Amt_PerDay;
                    db_data.Food_Eligible_Amt_PerDay = data1.Food_Eligible_Amt_PerDay;
                    db_data.DBTrack = db_data.DBTrack;

                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                    }
                    Msg.Add("  Data Saved successfully  ");
                    return Json(new Utility.JsonReturnClass { Id = db_data.Id, Val = db_data.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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