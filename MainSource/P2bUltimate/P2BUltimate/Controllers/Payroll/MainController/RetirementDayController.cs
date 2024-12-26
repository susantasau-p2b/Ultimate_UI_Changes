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
    public class RetirementDayController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/RetirementDay/Index.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(RetirementDay R, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string ServiceEndMonLastDay = form["ServiceEndMonLastDay"] == "0" ? "" : form["ServiceEndMonLastDay"];
                    string ServiceEndMonRegDay = form["ServiceEndMonRegDay"] == "0" ? "" : form["ServiceEndMonRegDay"];
                    string FirstdayExclude = form["FirstdayExclude"] == "0" ? "" : form["FirstdayExclude"];


                    R.ServiceEndMonLastDay = Convert.ToBoolean(ServiceEndMonLastDay);
                    R.ServiceEndMonRegDay = Convert.ToBoolean(ServiceEndMonRegDay);
                    R.FirstdayExclude = Convert.ToBoolean(FirstdayExclude);
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            R.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            RetirementDay retirementD = new RetirementDay()
                            {
                                ServiceEndMonLastDay = R.ServiceEndMonLastDay,
                                ServiceEndMonRegDay = R.ServiceEndMonRegDay,
                                FirstdayExclude = R.FirstdayExclude,
                                Year = R.Year,
                                DBTrack = R.DBTrack
                            };
                            try
                            {
                                db.RetirementDay.Add(retirementD);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                                //DT_Corp.Address_Id = c.Address == null ? 0 : c.Address.Id;
                                //DT_Corp.BusinessType_Id = c.BusinessType == null ? 0 : c.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                                //db.Create(DT_Corp);
                                db.SaveChanges();
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = R.Id });
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


        public ActionResult Edit(int data)
        {
            //string tableName = "Corporate";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.RetirementDay
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        ServiceEndMonLastDay = e.ServiceEndMonLastDay,
                        ServiceEndMonRegDay = e.ServiceEndMonRegDay,
                        FirstdayExclude = e.FirstdayExclude,
                        Year = e.Year,
                        Action = e.DBTrack.Action
                    }).ToList();

                var Corp = db.RetirementDay.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(RetirementDay R, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string ServiceEndMonLastDay = form["ServiceEndMonLastDay"] == "0" ? "" : form["ServiceEndMonLastDay"];
                    string ServiceEndMonRegDay = form["ServiceEndMonRegDay"] == "0" ? "" : form["ServiceEndMonRegDay"];
                    string FirstdayExclude = form["FirstdayExclude"] == "0" ? "" : form["FirstdayExclude"];

                    R.ServiceEndMonLastDay = Convert.ToBoolean(ServiceEndMonLastDay);
                    R.ServiceEndMonRegDay = Convert.ToBoolean(ServiceEndMonRegDay);
                    R.FirstdayExclude = Convert.ToBoolean(FirstdayExclude);
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;




                    if (ModelState.IsValid)
                    {
                        try
                        {

                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                RetirementDay blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.RetirementDay.Where(e => e.Id == data)
                                                            .SingleOrDefault();
                                    originalBlogValues = context.Entry(blog).OriginalValues;
                                }

                                R.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };



                                var CurCorp = db.RetirementDay.Find(data);
                                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {

                                    RetirementDay rD = new RetirementDay()
                                    {
                                        ServiceEndMonLastDay = R.ServiceEndMonLastDay,
                                        ServiceEndMonRegDay = R.ServiceEndMonRegDay,
                                        FirstdayExclude = R.FirstdayExclude,
                                        Year = R.Year,
                                        Id = data,
                                        DBTrack = R.DBTrack
                                    };
                                    db.RetirementDay.Attach(rD);
                                    db.Entry(rD).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(rD).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                }
                                // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                using (var context = new DataBaseContext())
                                {
                                    //var obj = DBTrackFile.DBTrackSave("Leave/Leave", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                    //DT_LvCreditPolicy DT_Corp = (DT_LvCreditPolicy)obj;
                                    //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;
                                    //db.Create(DT_Corp);
                                    db.SaveChanges();
                                }
                                await db.SaveChangesAsync();
                                ts.Complete();
                                //  return Json(new Object[] { R.Id, R.ServiceEndMonRegDay, "Record Updated", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = R.Id, Val = R.ServiceEndMonRegDay.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (RetirementDay)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (RetirementDay)databaseEntry.ToObject();
                                R.RowVersion = databaseValues.RowVersion;

                            }
                        }
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
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
                IEnumerable<RetirementDay> RetirementDay = null;
                if (gp.IsAutho == true)
                {
                    RetirementDay = db.RetirementDay.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    RetirementDay = db.RetirementDay.AsNoTracking().ToList();
                }

                IEnumerable<RetirementDay> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = RetirementDay;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                            || (e.ServiceEndMonLastDay.ToString().ToLower().Contains(gp.searchString.ToLower()))
                            || (e.ServiceEndMonRegDay.ToString().ToLower().Contains(gp.searchString.ToLower()))
                             || (e.FirstdayExclude.ToString().ToLower().Contains(gp.searchString.ToLower()))
                            || (e.Year.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.ServiceEndMonLastDay.ToString(), a.ServiceEndMonRegDay.ToString(), a.FirstdayExclude.ToString(), a.Year, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.ServiceEndMonLastDay, a.ServiceEndMonRegDay,a.FirstdayExclude, a.Year , a.Id}).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = RetirementDay;
                    Func<RetirementDay, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "ServiceEndMonLastDay" ? c.ServiceEndMonLastDay.ToString() :
                                         gp.sidx == "ServiceEndMonRegDay" ? c.ServiceEndMonRegDay.ToString() :
                                         gp.sidx == "FirstdayExclude" ? c.FirstdayExclude.ToString() :
                                         gp.sidx == "Year" ? c.Year.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.ServiceEndMonLastDay, a.ServiceEndMonRegDay,a.FirstdayExclude, a.Year , a.Id}).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.ServiceEndMonLastDay, a.ServiceEndMonRegDay, a.FirstdayExclude,a.Year, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  a.ServiceEndMonLastDay, a.ServiceEndMonRegDay,a.FirstdayExclude, a.Year , a.Id}).ToList();
                    }
                    totalRecords = RetirementDay.Count();
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
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                RetirementDay retirementday = db.RetirementDay
                                             .Where(e => e.Id == data)
                                             .SingleOrDefault();


                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {

                    try
                    {
                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now,
                            CreatedBy = retirementday.DBTrack.CreatedBy != null ? retirementday.DBTrack.CreatedBy : null,
                            CreatedOn = retirementday.DBTrack.CreatedOn != null ? retirementday.DBTrack.CreatedOn : null,
                            IsModified = retirementday.DBTrack.IsModified == true ? false : false//,
                            //AuthorizedBy = SessionManager.UserName,
                            //AuthorizedOn = DateTime.Now
                        };

                        // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                        db.Entry(retirementday).State = System.Data.Entity.EntityState.Deleted;
                        //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);
                        //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                        //DT_Corp.Address_Id = add == null ? 0 : add.Id;
                        //DT_Corp.BusinessType_Id = val == null ? 0 : val.Id;
                        //DT_Corp.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
                        //db.Create(DT_Corp);

                        await db.SaveChangesAsync();


                        //using (var context = new DataBaseContext())
                        //{
                        //    corporates.Address = add;
                        //    corporates.ContactDetails = conDet;
                        //    corporates.BusinessType = val;
                        //    DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                        //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", dbT);
                        //}
                        ts.Complete();
                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

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
                }
            }
        }

    }
}