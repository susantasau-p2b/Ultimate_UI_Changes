using P2b.Global;
using Payroll;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Text;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class CpiRuleDetailsController : Controller
    {
        //
        // GET: /Cpi_rule_details/
       // private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/CpiRuleDetails/Index.cshtml");
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/Payroll/_CpiRuleDetails.cshtml");
        }
        public ActionResult GetLookupDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.CPIRuleDetails.ToList();


                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.CPIRuleDetails.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var list1 = db.CPIRule.SelectMany(e => e.CPIRuleDetails).ToList();
                var list2 = fall.Except(list1);
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(CPIRuleDetails c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string CPIWageslist = form["CPIWageslist"] == "0" ? "" : form["CPIWageslist"];
                    string SalFrom = form["SalFrom"] == "0" ? "" : form["SalFrom"];
                    string SalTo = form["SalTo"] == "0" ? "" : form["SalTo"];
                    string IncrPercent = form["IncrPercent"] == "0" ? "" : form["IncrPercent"];
                    string AdditionalIncrAmount = form["AdditionalIncrAmount"] == "0" ? "" : form["AdditionalIncrAmount"];
                    string MinAmountIBase = form["MinAmountIBase"] == "0" ? "" : form["MinAmountIBase"];
                    string MaxAmountIBase = form["MaxAmountIBase"] == "0" ? "" : form["MaxAmountIBase"];
                    string ServiceFrom = form["ServiceFrom"] == "0" ? "" : form["ServiceFrom"];
                    string ServiceTo = form["ServiceTo"] == "0" ? "" : form["ServiceTo"];

                    if (c.IncrPercent != null)
                    {
                        if (c.IncrPercent > 100)
                        {
                            Msg.Add("  IncrPercent-Enter Below 100%  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { "", "", "IncrPercent-Enter Below 100%", JsonRequestBehavior.AllowGet });
                        }
                    }
                    if (CPIWageslist != null)
                    {
                        if (CPIWageslist != "")
                        {
                            int WId = Convert.ToInt32(CPIWageslist);
                            var val = db.Wages.Where(e => e.Id == WId).SingleOrDefault();


                            c.CPIWages = val;
                        }
                    }

                    if (SalFrom != null)
                    {
                        if (SalFrom != "")
                        {
                            var val = double.Parse(SalFrom);

                            c.SalFrom = val;
                        }
                    }

                    if (SalTo != null)
                    {
                        if (SalTo != "")
                        {
                            var val = double.Parse(SalTo);

                            c.SalTo = val;
                        }
                    }

                    if (ServiceFrom != null)
                    {
                        if (ServiceFrom != "")
                        {
                            c.ServiceFrom = ServiceFrom;
                        }
                    }

                    if (ServiceTo != null)
                    {
                        if (ServiceTo != "")
                        {
                            c.ServiceTo = ServiceTo;
                        }
                    }

                    if (IncrPercent != null)
                    {
                        if (IncrPercent != "")
                        {
                            var val = double.Parse(IncrPercent);

                            c.IncrPercent = val;
                        }
                    }


                    if (AdditionalIncrAmount != null)
                    {
                        if (AdditionalIncrAmount != "")
                        {
                            var val = double.Parse(AdditionalIncrAmount);

                            c.AdditionalIncrAmount = val;
                        }
                    }
                    if (MinAmountIBase != null)
                    {
                        if (MinAmountIBase != "")
                        {
                            var val = double.Parse(MinAmountIBase);

                            c.MinAmountIBase = val;
                        }
                    }
                    if (MaxAmountIBase != null)
                    {
                        if (MaxAmountIBase != "")
                        {
                            var val = double.Parse(MaxAmountIBase);

                            c.MaxAmountIBase = val;
                        }
                    }
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            CPIRuleDetails cp = new CPIRuleDetails()
                            {
                                CPIWages = c.CPIWages,
                                SalFrom = c.SalFrom,
                                SalTo = c.SalTo,
                                IncrPercent = c.IncrPercent,
                                AdditionalIncrAmount = c.AdditionalIncrAmount,
                                MinAmountIBase = c.MinAmountIBase,
                                MaxAmountIBase = c.MaxAmountIBase,
                                ServiceFrom = c.ServiceFrom,
                                ServiceTo = c.ServiceTo,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.CPIRuleDetails.Add(cp);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll", null, db.ChangeTracker, c.DBTrack);
                                //DT_CPIRuleDetails DT_Corp = (DT_CPIRuleDetails)rtn_Obj;
                                //DT_Corp.CPIWages_Id = c.CPIWages == null ? 0 : c.CPIWages.Id;                        
                                //db.Create(DT_Corp);
                                db.SaveChanges();
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);

                                var full_details = db.CPIRuleDetails.Include(e => e.CPIWages).Where(e => e.Id == cp.Id).SingleOrDefault();
                                ts.Complete();
                                //   return this.Json(new Object[] {full_details.Id,full_details.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = full_details.Id, Val = full_details.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
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
                        Msg.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        // Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new { msg = errorMsg });
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




        public ActionResult Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    //CPIRuleDetails CPIRuleDetails = db.CPIRuleDetails.Find(data);
                    //try
                    //{
                    //    db.Entry(CPIRuleDetails).State = System.Data.Entity.EntityState.Deleted;
                    //    //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                    //    db.SaveChanges();


                    //    return Json(new Object[] { "",  "Data removed.", JsonRequestBehavior.AllowGet });
                    //}
                    //catch (DbUpdateConcurrencyException)
                    //{
                    //    return RedirectToAction("Delete", new { concurrencyError = true, id = data });
                    //}
                    //catch (RetryLimitExceededException /* dex */)
                    //{
                    //    //Log the error (uncomment dex variable name and add a line here to write a log.)
                    //    //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                    //    //return RedirectToAction("Delete");
                    //    return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                    //}

                    CPIRuleDetails cPIRuleDetails = db.CPIRuleDetails.Find(data);

                    var selectedRegions = cPIRuleDetails.CPIWages;
                    ////if (selectedRegions != null)
                    ////{
                    ////    var corpRegion = new HashSet<int>(cPIRuleDetails.CPIWages.Select(e => e.Id));
                    ////    if (corpRegion.Count > 0)
                    ////    {
                    ////        return this.Json(new { msg = "Child record exists.Cannot remove it.", JsonRequestBehavior.AllowGet });
                    ////        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                    ////    }
                    ////}


                    try
                    {
                        DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };
                        db.Entry(cPIRuleDetails).State = System.Data.Entity.EntityState.Deleted;
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                        db.SaveChanges();
                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        // return this.Json(new { msg = "Data removed.", JsonRequestBehavior.AllowGet });
                        // return Json(new Object[] { "", "", "Data removed.", JsonRequestBehavior.AllowGet });
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Delete", new { concurrencyError = true, id = data });
                    }
                    catch (RetryLimitExceededException /* dex */)
                    {
                        //Log the error (uncomment dex variable name and add a line here to write a log.)
                        //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                        //return RedirectToAction("Delete");
                        //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });

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


        [HttpPost]
        public ActionResult Edit(int data)
        {
            //string tableName = "Corporate";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.CPIRuleDetails
                    .Include(e => e.CPIWages)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Id = e.Id,
                        SalFrom = e.SalFrom,
                        SalTo = e.SalTo,
                        IncrPercent = e.IncrPercent,
                        AdditionalIncrAmount = e.AdditionalIncrAmount,
                        MinAmountIBase = e.MinAmountIBase,
                        MaxAmountIBase = e.MaxAmountIBase,
                        ServiceFrom = e.ServiceFrom!=null? e.ServiceFrom:"0",
                        ServiceTo = e.ServiceTo!=null? e.ServiceTo:"0",
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.CPIRuleDetails
                  .Include(e => e.CPIWages)
                    .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        CPIWages_FullDetails = e.CPIWages.FullDetails == null ? "" : e.CPIWages.FullDetails,
                        CPIWages_Id = e.CPIWages.Id == null ? "" : e.CPIWages.Id.ToString(),

                    }).ToList();


                var W = db.DT_CPIRule
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id


                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var Corp = db.CPIRuleDetails.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }



        public ActionResult EditSave(int data, CPIRuleDetails cp, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (cp.IncrPercent != null)
                    {
                        if (cp.IncrPercent > 100)
                        {
                            //  return Json(new Object[] { "", "", "IncrPercent-Enter Below 100%", JsonRequestBehavior.AllowGet });
                            Msg.Add(" IncrPercent-Enter Below 100% ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    var id = Convert.ToInt32(data);
                    var db_data = db.CPIRuleDetails
                                    .Include(e => e.CPIWages)
                                    .Where(e => e.Id == id)
                                    .SingleOrDefault();

                    var wages_id = Convert.ToInt32(form["CPIWageslist"]);
                    var wages_val = db.Wages.Find(wages_id);
                    db_data.CPIWages = wages_val;

                    try
                    {
                        db.CPIRuleDetails.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                    }
                    catch (Exception)
                    {

                        throw;
                    }

                    db_data.Id = data;
                    db_data.IncrPercent = cp.IncrPercent;
                    db_data.SalFrom = cp.SalFrom;
                    db_data.SalTo = cp.SalTo;
                    db_data.ServiceFrom = cp.ServiceFrom;
                    db_data.ServiceTo = cp.ServiceTo;
                    db_data.AdditionalIncrAmount = cp.AdditionalIncrAmount;
                    db_data.MinAmountIBase = cp.MinAmountIBase;
                    db_data.MaxAmountIBase = cp.MaxAmountIBase;

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            try
                            {
                                db.CPIRuleDetails.Attach(db_data);
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = db_data.Id, Val = db_data.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return Json(new Object[] {db_data.Id,db_data.FullDetails,"Record upadated",JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateException)
                            {

                                throw;
                            }
                            catch (DBConcurrencyException)
                            {

                                throw;
                            }
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
                var ITSection10 = db.CPIRuleDetails.ToList();
                IEnumerable<CPIRuleDetails> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = ITSection10;
                    if (gp.searchOper.Equals("eq"))
                    {
                        //jsonData = IE.Select(a => new Object[] { a.Id, a.SalFrom, a.SalTo, a.IncrPercent, a.AdditionalIncrAmount }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        if (gp.searchField == "Id")
                        {
                            jsonData = IE.Where(e => e.Id.ToString().Contains(gp.searchString)).Select(a => new { a.Id, a.SalFrom, a.SalTo, a.IncrPercent, a.AdditionalIncrAmount }).ToList();
                        }
                        else if (gp.searchField == "SalFrom")
                        {
                            jsonData = IE.Where(e => e.SalFrom.ToString().Contains(gp.searchString)
                               ).Select(a => new { a.Id, a.SalFrom, a.SalTo, a.IncrPercent, a.AdditionalIncrAmount }).ToList();
                        }
                        else if (gp.searchField == "SalTo")
                        {
                            jsonData = IE.Where(e => e.SalTo.ToString().Contains(gp.searchString)
                              ).Select(a => new { a.Id, a.SalFrom, a.SalTo, a.IncrPercent, a.AdditionalIncrAmount }).ToList();
                        }
                        else if (gp.searchField == "IncrPercent")
                        {
                            jsonData = IE.Where(e => e.IncrPercent.ToString().Contains(gp.searchString)
                               ).Select(a => new { a.Id, a.SalFrom, a.SalTo, a.IncrPercent, a.AdditionalIncrAmount }).ToList();
                        }
                        else if (gp.searchField == "AdditionalIncrAmount")
                        {
                            jsonData = IE.Where(e => e.AdditionalIncrAmount.ToString().Contains(gp.searchString)
                              ).Select(a => new { a.Id, a.SalFrom, a.SalTo, a.IncrPercent, a.AdditionalIncrAmount }).ToList();
                        }
                        //jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                        //      || (e.SalFrom.ToString().Contains(gp.searchString))
                        //      || (e.SalTo.ToString().Contains(gp.searchString))
                        //      || (e.IncrPercent.ToString().Contains(gp.searchString))
                        //      || (e.AdditionalIncrAmount.ToString().Contains(gp.searchString))
                        //      ).Select(a => new { a.Id, a.SalFrom, a.SalTo, a.IncrPercent, a.AdditionalIncrAmount }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.SalFrom, a.SalTo, a.IncrPercent, a.AdditionalIncrAmount }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = ITSection10;
                    Func<CPIRuleDetails, string> orderfuc = (c =>
                                                               gp.sidx == "Id" ? c.Id.ToString() :
                                                               gp.sidx == "SalFrom" ? c.SalFrom.ToString() :
                                                               gp.sidx == "SalTo" ? c.SalTo.ToString() :
                                                               gp.sidx == "IncrPercent" ? c.IncrPercent.ToString() :
                                                               gp.sidx == "AdditionalIncrAmount" ? c.AdditionalIncrAmount.ToString() :
                                                                "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.SalFrom, a.SalTo, a.IncrPercent, a.AdditionalIncrAmount }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.SalFrom, a.SalTo, a.IncrPercent, a.AdditionalIncrAmount }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.SalFrom, a.SalTo, a.IncrPercent, a.AdditionalIncrAmount }).ToList();
                    }
                    totalRecords = ITSection10.Count();
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
