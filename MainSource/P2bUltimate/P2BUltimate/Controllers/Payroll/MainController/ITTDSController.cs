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


namespace P2BUltimate.Controllers
{
    public class ITTDSController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ITTDS/Index.cshtml");
        }
        public ActionResult tds_partial()
        {
            return View("~/Views/Shared/_Tdsmaster.cshtml");
        }
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Grid_TDSMaster(P2BGrid_Parameters gp)
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
                    var Tds = db.ITTDS.Include(e => e.Category).ToList();
                    IEnumerable<ITTDS> IE;
                    if (!string.IsNullOrEmpty(gp.searchField))
                    {
                        IE = Tds;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Category.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.IncomeRangeFrom.ToString().Contains(gp.searchString))
                               || (e.IncomeRangeTo.ToString().Contains(gp.searchString))
                               || (e.Percentage.ToString().Contains(gp.searchString))
                               || (e.Amount.ToString().Contains(gp.searchString))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { Convert.ToString(a.Category.LookupVal), a.IncomeRangeFrom, a.IncomeRangeTo, a.Percentage, a.Amount, a.Id }).ToList();

                            //jsonData = IE.Select(a => new Object[] { a.Category != null ? Convert.ToString(a.Category.LookupVal) : "", a.IncomeRangeFrom, a.IncomeRangeTo, a.Percentage, a.Amount, a.Id, a.EduCessPercent, a.EduCessAmount, a.SurchargePercent, a.SurchargeAmount }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Category != null ? Convert.ToString(a.Category.LookupVal) : "", a.IncomeRangeFrom, a.IncomeRangeTo, a.Percentage, a.Amount, a.Id, a.EduCessPercent, a.EduCessAmount, a.SurchargePercent, a.SurchargeAmount }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = Tds;
                        Func<ITTDS, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Category" ? c.Category.LookupVal.ToString() :
                                             gp.sidx == "IncomeRangeFrom" ? c.IncomeRangeFrom.ToString() :
                                             gp.sidx == "IncomeRangeTo" ? c.IncomeRangeTo.ToString() :
                                             gp.sidx == "Percentage" ? c.Percentage.ToString() :
                                             gp.sidx == "Amount" ? c.Amount.ToString() :
                                             "");
                        }
                        //Func<ITTDS, string> orderfuc = (c =>
                        //                                           gp.sidx == "Id" ? c.Id.ToString() :
                        //                                           gp.sidx == "Category" ? c.Category.ToString() :
                        //                                           gp.sidx == "IncomeRangeFrom" ? c.IncomeRangeFrom.ToString() :
                        //                                           gp.sidx == "IncomeRangeTo" ? Convert.ToString(c.IncomeRangeTo) :
                        //                                           gp.sidx == "Percentage" ? Convert.ToString(c.Percentage) :
                        //                                           gp.sidx == "Amount" ? Convert.ToString(c.Amount) :
                        //                                           "");
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Category != null ? Convert.ToString(a.Category.LookupVal) : "", a.IncomeRangeFrom, a.IncomeRangeTo, a.Percentage, a.Amount, a.Id, a.EduCessPercent, a.EduCessAmount, a.SurchargePercent, a.SurchargeAmount }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Category != null ? Convert.ToString(a.Category.LookupVal) : "", a.IncomeRangeFrom, a.IncomeRangeTo, a.Percentage, a.Amount, a.Id, a.EduCessPercent, a.EduCessAmount, a.SurchargePercent, a.SurchargeAmount }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Category != null ? Convert.ToString(a.Category.LookupVal) : "", a.IncomeRangeFrom, a.IncomeRangeTo, a.Percentage, a.Amount, a.Id, a.EduCessPercent, a.EduCessAmount, a.SurchargePercent, a.SurchargeAmount }).ToList();
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

        //End//  // TDS Master //
        ////var Q = db.ITTDS
        ////        .Include(e => e.Category)
        ////        .Where(e => e.Id == data).Select
        ////        (e => new
        ////        {
        ////            IncomeRangeFrom = e.IncomeRangeFrom,
        ////            IncomeRangeTo = e.IncomeRangeFrom,
        ////            Percentage = e.Percentage,
        ////            Amount = e.Amount,
        ////            EduCessPercent = e.EduCessPercent,
        ////            EduCessAmount = e.EduCessAmount,
        ////            SurchargePercent = e.SurchargePercent,
        ////            SurchargeAmount = e.SurchargeAmount,
        ////            Category_Id = e.Category.Id == null ? 0 : e.Category.Id,
        ////            Action = e.DBTrack.Action
        ////        }).ToList();


        public ActionResult GetLookupDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                IEnumerable<ITTDS> all;
                var fall = db.ITTDS.ToList();
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.ITTDS.ToList().Where(d => d.IncomeRangeFrom.ToString().Contains(data));
                    var result = (from c in all
                                  select new { c.Id, c.IncomeRangeFrom }).Distinct();
                    return Json(result, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.IncomeRangeFrom }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);

                }

            }
        }

        public ActionResult edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ITTDS
                    .Include(e => e.Category)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        IncomeRangeFrom = e.IncomeRangeFrom,
                        IncomeRangeTo = e.IncomeRangeTo,
                        Percentage = e.Percentage,
                        Amount = e.Amount,
                        EduCessPercent = e.EduCessPercent,
                        EduCessAmount = e.EduCessAmount,
                        SurchargePercent = e.SurchargePercent,
                        SurchargeAmount = e.SurchargeAmount,
                        Category_Id = e.Category.Id == null ? 0 : e.Category.Id,
                        Action = e.DBTrack.Action
                    }).ToList();

                ////var lookupval = db.ITTDS.Include(e => e.Category)
                ////   .Where(e => e.Id == data).Select(e => new { Category_Id=e.Category.Id }).ToList();
                return Json(new Object[] { Q, "", "", JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create(ITTDS td, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var category_id = form["Cat"];
                    int CategoryId = Convert.ToInt32(category_id);
                    if (CategoryId == 0)
                    {
                        Msg.Add("  Please Select Category.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { "", "Please Select Category.", JsonRequestBehavior.AllowGet });
                    }
                    if (td.IncomeRangeFrom > td.IncomeRangeTo)
                    {
                        Msg.Add(" Income To should be greater than Income From.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new object[]{null,null,"To Date should be greater than From Date.", JsonRequestBehavior.AllowGet });
                        //return View(c);
                    }
                    //  var qwe = db.ITTDS.Where(e => e.Category.Id == CategoryId).Select(q => q.Category.LookupVal).ToList();
                    //var alrDq = db.ITTDS.Where(e => e.Category.Id == CategoryId)
                    //.Any(q => ((q.IncomeRangeFrom <= td.IncomeRangeTo && q.IncomeRangeTo <= td.IncomeRangeTo) || (q.IncomeRangeFrom <= td.IncomeRangeFrom && q.IncomeRangeTo >= td.IncomeRangeTo)) && (q.IncomeRangeTo >= td.IncomeRangeFrom));
                    var alrDq = db.ITTDS.Where(e => e.Category.Id == CategoryId).ToList();
                    double Incomerangefrom = 0;
                    double incomerangeto = 0;
                  //  IT TDS Save event checking message comment because two year tax slab will differ(eg oneyear slab 0-250000 may be other year 0-300000)
                    //if (alrDq.Count() > 0)
                    //{
                    //    Incomerangefrom = alrDq.FirstOrDefault().IncomeRangeFrom;
                    //    incomerangeto = alrDq.LastOrDefault().IncomeRangeTo;

                    //    if (td.IncomeRangeFrom > Incomerangefrom && td.IncomeRangeFrom < incomerangeto)
                    //    {
                    //        Msg.Add(" TDS DATA With this already exist.  ");
                    //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //    }
                    //    if (td.IncomeRangeTo < incomerangeto)
                    //    {
                    //        Msg.Add(" TDS DATA With this already exist.  ");
                    //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //    }
                    //}

                    //if (db.ITTDS.Any(o => o.IncomeRangeFrom == td.IncomeRangeFrom && o.Category.Id == CategoryId))
                    //{
                    //    Msg.Add("IncomeRangeFrom already exists.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //    //return this.Json(new object[] { null, null, "IncomeRangeFrom already exists.", JsonRequestBehavior.AllowGet });
                    //}

                    //if (db.ITTDS.Any(o => o.IncomeRangeTo == td.IncomeRangeTo && o.Category.Id == CategoryId))
                    //{
                    //    Msg.Add("IncomeRangeTo already exists.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //    //return this.Json(new object[] { null, null, "IncomeRangeTo already exists.", JsonRequestBehavior.AllowGet });
                    //}
                    if (category_id != null)
                    {
                        var cat_id = Convert.ToInt32(category_id);
                        var category_value = db.LookupValue.Find(cat_id);
                        td.Category = category_value;
                    }

                    if (db.ITTDS.Any(o => o.IncomeRangeFrom == td.IncomeRangeFrom && o.IncomeRangeTo == td.IncomeRangeTo && o.Category.LookupVal.ToUpper() == td.Category.LookupVal.ToUpper()))
                    {
                        //return this.Json(new { msg = "Code already exists." });
                        Msg.Add("  Record Already Exists.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new Object[] { null, null, "ScaleName already exists.", JsonRequestBehavior.AllowGet });

                    }
                    if (ModelState.IsValid)
                    {
                        td.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        ITTDS tds = new ITTDS()
                        {
                            // IncomeFrom = td.IncomeFrom == null ? "" : td.IncomeFrom.Trim(),
                            IncomeRangeFrom = td.IncomeRangeFrom,
                            Category = td.Category,
                            IncomeRangeTo = td.IncomeRangeTo,
                            Percentage = td.Percentage,
                            Amount = td.Amount,
                            EduCessPercent = td.EduCessPercent,
                            EduCessAmount = td.EduCessAmount,
                            SurchargePercent = td.SurchargePercent,
                            SurchargeAmount = td.SurchargeAmount,
                            DBTrack = td.DBTrack
                        };
                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {

                                db.ITTDS.Add(tds);
                                db.SaveChanges();
                                ts.Complete();
                            }
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = tds.Id, Val = tds.IncomeRangeFrom.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

        public ActionResult EditSave(ITTDS td, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    // string new_cat_id = form["Cat"] == "" ? "0" : form["Cat"];
                    //var new_cat_value = db.LookupValue.Find(Int32.Parse(new_cat_id));
                    var cat_val = db.ITTDS.Include(e => e.Category).Where(e => e.Id == data).Select(e => e.Category).SingleOrDefault();
                    td.Category = cat_val;
                    //if (db.ITTDS.Any(o => o.IncomeRangeFrom == td.IncomeRangeFrom && o.IncomeRangeTo == td.IncomeRangeTo && o.Category.LookupVal.ToUpper() == td.Category.LookupVal.ToUpper()))
                    //{
                    //    //return this.Json(new { msg = "Code already exists." });
                    //    Msg.Add("  Record Already Exists.  ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //    // return this.Json(new Object[] { null, null, "ScaleName already exists.", JsonRequestBehavior.AllowGet });

                    //}
                    var db_data = db.ITTDS.Include(e => e.Category).Where(e => e.Id == data).SingleOrDefault();

                    db_data.Category = td.Category;
                    db.ITTDS.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                    db_data.IncomeRangeFrom = td.IncomeRangeFrom;
                    db_data.Category = td.Category;
                    db_data.IncomeRangeTo = td.IncomeRangeTo;
                    db_data.Percentage = td.Percentage;
                    db_data.Amount = td.Amount;
                    db_data.EduCessPercent = td.EduCessPercent;
                    db_data.EduCessAmount = td.EduCessAmount;
                    db_data.SurchargePercent = td.SurchargePercent;
                    db_data.SurchargeAmount = td.SurchargeAmount;
                    try
                    {
                        if (ModelState.IsValid)
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {

                                db.ITTDS.Attach(db_data);
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                                Msg.Add("  Data Updated successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new object[]{null,null,"Data saved successfully."});
                            }
                        }
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Edit", new { concurrencyError = true, id = td.Id });
                    }
                    catch (DataException)
                    {
                        Msg.Add(" Unable to Edit. Try again, and if the problem persists, see your system administrator");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //ModelState.AddModelError(string.Empty, "Unable to Edit. Try again, and if the problem persists contact your system administrator.");
                        //return RedirectToAction("Edit");
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
        //[ValidateAntiForgeryToken]
        public ActionResult Delete(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                ITTDS tds = db.ITTDS.Find(data);
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(tds).State = System.Data.Entity.EntityState.Deleted;
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
        public ActionResult PopulateLookupDropDownList(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int id = data == "" ? 0 : Convert.ToInt32(data);
                var dropvalue = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "7").SingleOrDefault();
                List<SelectListItem> value = new List<SelectListItem>();
                if (dropvalue != null)
                {
                    foreach (var l in dropvalue.LookupValues)
                    {
                        if (l.IsActive == true)
                        {
                            value.Add(new SelectListItem
                            {
                                Text = l.LookupVal,
                                Value = l.Id.ToString(),
                                Selected = (l.Id == id ? true : false)
                            });
                        }
                    }
                }
                return Json(value, JsonRequestBehavior.AllowGet);
            }

        }
    }

}
