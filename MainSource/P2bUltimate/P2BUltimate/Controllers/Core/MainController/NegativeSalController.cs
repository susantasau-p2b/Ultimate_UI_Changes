///
/// Created by Tanushri
///

using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Collections;
using System.Data.Entity.Core.Objects;
using Payroll;
using P2BUltimate.Security;


namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class NegativeSalController : Controller
    {
        //  private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/NegativeSal/Index.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(NegSalAct NOBJ, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int company_Id;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    var companypayroll = new CompanyPayroll();
                    companypayroll = db.CompanyPayroll.Include(e => e.NegSalAct).Where(e => e.Company.Id == company_Id).SingleOrDefault();

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.NegSalAct.Any(o => o.NegSalActname == NOBJ.NegSalActname))
                            {
                                Msg.Add("  Neg Sal Actname Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Neg Sal Actname Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                            NOBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };



                            NegSalAct NegSalAct = new NegSalAct()
                            {
                                NegSalActname = NOBJ.NegSalActname == null ? "" : NOBJ.NegSalActname.Trim(),
                                MinAmount = NOBJ.MinAmount,
                                SalPercentage = NOBJ.SalPercentage,
                                EffectiveDate = NOBJ.EffectiveDate,
                                DBTrack = NOBJ.DBTrack
                            };
                            try
                            {
                                db.NegSalAct.Add(NegSalAct);
                                DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, NOBJ.DBTrack);
                                db.SaveChanges();

                                List<NegSalAct> NegSalAct_list = new List<NegSalAct>();
                                NegSalAct_list.Add(NegSalAct);
                                if (companypayroll != null)
                                {
                                    companypayroll.NegSalAct = NegSalAct_list;
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Detached;
                                }




                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
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
                        //var errorMsg = sb.ToString();
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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

        [HttpPost]
        public ActionResult Edit(int data)
        {
            //string tableName = "NegSalAct";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);
            using (DataBaseContext db = new DataBaseContext())
            {
                //var Q = db.NegSalAct
                //    .Where(e => e.Id == data);
                var Q = db.NegSalAct
                .Where(e => e.Id == data).Select
                 (e => new
                 {
                     EffectiveDate = e.EffectiveDate,
                     NegSalActname = e.NegSalActname == null ? "" : e.NegSalActname,
                     MinAmount = e.MinAmount == null ? 0 : e.MinAmount,
                     SalPercentage = e.SalPercentage == null ? 0 : e.SalPercentage,
                     //Action = e.DBTrack.Action
                 }).ToList();
                //var add_data = db.NegSalAct
                //    .Where(e => e.Id == data); 
                var lkup = db.NegSalAct.Find(data);
                TempData["RowVersion"] = lkup.RowVersion;
                return Json(new Object[] { Q, JsonRequestBehavior.AllowGet });
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
                var NegSalAct = db.NegSalAct.ToList();


                IEnumerable<NegSalAct> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = NegSalAct;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.NegSalActname.ToUpper().Contains(gp.searchString.ToUpper()))
                             || (e.MinAmount.ToString().Contains(gp.searchString))
                             || (e.SalPercentage.ToString().Contains(gp.searchString))
                             || (e.Id.ToString().Contains(gp.searchString.ToString()))
                             ).Select(a => new Object[] { a.NegSalActname, a.MinAmount, a.SalPercentage, a.Id }).ToList();

                        //jsonData = IE.Select(a => new Object[]                         //jsonData = IE.Select(a => new { a.NegSalActname, a.MinAmount,a.SalPercentage ,a.Id }).Where((e => (e.Id.ToString() == gp.searchString) || (e.NegSalActname.ToLower() == gp.searchString.ToLower()) || (e.MinAmount.ToString() == gp.searchString)   || (e.SalPercentage.ToString() == gp.searchString) )).ToList();{ a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.NegSalActname, a.MinAmount, a.SalPercentage, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = NegSalAct;
                    Func<NegSalAct, string> orderfuc = (c => gp.sidx == "Id" ? c.Id.ToString() :
                                                               gp.sidx == "NegSalActname" ? c.NegSalActname.ToString() :
                                                               gp.sidx == "MinAmount" ? c.MinAmount.ToString() :
                                                                gp.sidx == "SalPercentage" ? c.SalPercentage.ToString() : ""
                                                                );
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.NegSalActname), Convert.ToString(a.MinAmount), Convert.ToString(a.SalPercentage), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.NegSalActname), Convert.ToString(a.MinAmount), Convert.ToString(a.SalPercentage), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.NegSalActname, a.MinAmount, a.SalPercentage, a.Id }).ToList();
                    }
                    totalRecords = NegSalAct.Count();
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

        public async Task<ActionResult> EditSave(NegSalAct c, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            bool Auth = form["Autho_Allow"] == "true" ? true : false;



            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int company_Id;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    var companypayroll = new CompanyPayroll();
                    companypayroll = db.CompanyPayroll.Include(e => e.NegSalAct).Where(e => e.Company.Id == company_Id).SingleOrDefault();
                    c.CompanyPayroll_Id = companypayroll.Id;
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.NegSalAct.Include(e => e.CompanyPayroll).Include(e => e.CompanyPayroll.Company).Where(e => e.Id == data).SingleOrDefault();

                        if (c.CompanyPayroll_Id != 0)
                        {
                            db_data.CompanyPayroll_Id = c.CompanyPayroll_Id;
                        }
                        else
                        {
                            db_data.CompanyPayroll_Id = null;
                        }

                        db.NegSalAct.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;
                        NegSalAct negsalact = db.NegSalAct.Find(data);
                        TempData["CurrRowVersion"] = negsalact.RowVersion;

                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            NegSalAct blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;

                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = negsalact.DBTrack.CreatedBy == null ? null : negsalact.DBTrack.CreatedBy,
                                CreatedOn = negsalact.DBTrack.CreatedOn == null ? null : negsalact.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                        
                            negsalact.CompanyPayroll_Id = c.CompanyPayroll_Id != null ? c.CompanyPayroll_Id : 0;
                            negsalact.Id = data;
                            negsalact.MinAmount = c.MinAmount;
                            negsalact.SalPercentage = c.SalPercentage;
                            negsalact.NegSalActname = c.NegSalActname;
                            negsalact.EffectiveDate = c.EffectiveDate;
                            negsalact.DBTrack = c.DBTrack;
                            db.Entry(negsalact).State = System.Data.Entity.EntityState.Modified;
                            //using (var context = new DataBaseContext())
                            //{
                            blog = db.NegSalAct.Where(e => e.Id == data).Include(e => e.CompanyPayroll)
                                                    
                                                    .SingleOrDefault();
                            originalBlogValues = db.Entry(blog).OriginalValues;
                            db.ChangeTracker.DetectChanges();
                            var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                            DT_NegSalAct DT_Corp = (DT_NegSalAct)obj;
                            db.SaveChanges();                                                  
                        }
                        //}
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.NegSalActname, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }
        //[HttpPost]
        //public async Task<ActionResult> EditSave(NegSalAct NOBJ, int data, FormCollection form)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<string> Msg = new List<string>();
        //        if (ModelState.IsValid)
        //        {
        //            try
        //            {

        //                //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    var Curr_OBJ = db.NegSalAct.Find(data);
        //                    TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
        //                    db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;
        //                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                    {
        //                        NegSalAct blog = blog = null;
        //                        DbPropertyValues originalBlogValues = null;

        //                        using (var context = new DataBaseContext())
        //                        {
        //                            blog = context.NegSalAct.Where(e => e.Id == data).SingleOrDefault();
        //                            originalBlogValues = context.Entry(blog).OriginalValues;
        //                        }

        //                        NOBJ.DBTrack = new DBTrack
        //                        {
        //                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                            Action = "M",
        //                            ModifiedBy = SessionManager.UserName,
        //                            ModifiedOn = DateTime.Now
        //                        };
        //                        NegSalAct EOBJ = new NegSalAct()
        //                        {
        //                            NegSalActname = NOBJ.NegSalActname,
        //                            MinAmount = NOBJ.MinAmount,
        //                            SalPercentage = NOBJ.SalPercentage,
        //                            EffectiveDate = NOBJ.EffectiveDate,
        //                            Id = data,
        //                            DBTrack = NOBJ.DBTrack
        //                        };


        //                        db.NegSalAct.Attach(EOBJ);
        //                        db.Entry(EOBJ).State = System.Data.Entity.EntityState.Modified;

        //                        // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
        //                        //db.SaveChanges();
        //                        db.Entry(EOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                        DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, NOBJ.DBTrack);
        //                        await db.SaveChangesAsync();
        //                        //DisplayTrackedEntities(db.ChangeTracker);
        //                        db.Entry(EOBJ).State = System.Data.Entity.EntityState.Detached;
        //                        ts.Complete();
        //                        Msg.Add("  Record Updated");
        //                        return Json(new Utility.JsonReturnClass { Id = EOBJ.Id, Val = EOBJ.NegSalActname, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        // return Json(new Object[] { EOBJ.Id, EOBJ.NegSalActname, "Record Updated", JsonRequestBehavior.AllowGet });
        //                    }
        //                }
        //            }
        //            catch (DbUpdateConcurrencyException ex)
        //            {
        //                var entry = ex.Entries.Single();
        //                var clientValues = (NegSalAct)entry.Entity;
        //                var databaseEntry = entry.GetDatabaseValues();
        //                if (databaseEntry == null)
        //                {
        //                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    // Msg.Add(" Unable to save changes. The record was deleted by another user. ");
        //                    //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                }
        //                else
        //                {
        //                    var databaseValues = (NegSalAct)databaseEntry.ToObject();
        //                    NOBJ.RowVersion = databaseValues.RowVersion;
        //                }
        //            }
        //            Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //            // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });

        //            //db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //            //await db.SaveChangesAsync();
        //            //return Json(new Object[] { "", "", "Data saved successfully.", JsonRequestBehavior.AllowGet });

        //        }
        //        return View();
        //    }
        //}

        [HttpPost]
        public ActionResult Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                NegSalAct EOBJ = db.NegSalAct.Find(data);
                var selectedRegions = "";

                if (selectedRegions != "")
                {

                }


                try
                {
                    DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };
                    db.Entry(EOBJ).State = System.Data.Entity.EntityState.Deleted;
                    DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);
                    db.SaveChanges();

                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { msg = "Data removed.", JsonRequestBehavior.AllowGet });
                    //return Json(new Object[] { "",  "Data removed.", JsonRequestBehavior.AllowGet });
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
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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