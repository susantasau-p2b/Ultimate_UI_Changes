using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Xml;
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
using Payroll;
using Leave;


namespace P2BUltimate.Controllers.Core.MainController
{
    public class HolidayListController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/HolidayList/Index.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/Core/_HolidayList.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(HolidayList FormHolidayList, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string HolidayList = form["HolidayList"] == "0" ? null : form["HolidayList"];

                    if (HolidayList != null)
                    {
                        var id = Convert.ToInt32(HolidayList);
                        var val = db.Holiday.Include(e => e.HolidayName).Include(e => e.HolidayType).Where(e => e.Id == id).FirstOrDefault();
                        var valid = db.Holiday.Find(id);
                        FormHolidayList.Holiday = val;
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            FormHolidayList.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            HolidayList holidayList = new HolidayList()
                            {
                                Holiday = FormHolidayList.Holiday,
                                HolidayDate = FormHolidayList.HolidayDate,
                                DBTrack = FormHolidayList.DBTrack

                            };
                            try
                            {
                                db.HolidayList.Add(holidayList);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = holidayList.Id, Val = holidayList.Holiday.FullDetails + ", " + holidayList.HolidayDate, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //var query = db.HolidayList.Include(e => e.Holiday).Include(e=>e.Holiday.HolidayName).Where(e => e.Holiday != null && e.Holiday.HolidayName != null && e.Id == HolidayList.Id).SingleOrDefault();
                                // return Json(new Object[] { HolidayList.Id,HolidayList.Holiday.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });

                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = "" });
                            }
                            catch (DataException e)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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

        public ActionResult ValidateForm(HolidayList frmHolidaylist, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string HolidayList = form["HolidayList"] == "0" ? null : form["HolidayList"];
                if (HolidayList != null)
                {
                    var value = db.Holiday.Find(int.Parse(HolidayList));
                    frmHolidaylist.Holiday = value;
                }
                if (db.HolidayList.Any(e => e.Holiday.Id == frmHolidaylist.Holiday.Id && e.HolidayDate == frmHolidaylist.HolidayDate))
                {

                    var Msg = new List<string>();
                    Msg.Add("Holiday List Already Exist");
                    return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


                }
                return Json(new { success = true, }, JsonRequestBehavior.AllowGet);


            }


        }


        //public ActionResult Edit(int data)
        //{
        //    var id = Convert.ToInt32(data);
        //    var data1 = db.HolidayList.Where(e => e.Id == id).SingleOrDefault();
        //    var data2 = db.HolidayList.Include(e => e.HolidayDate)
        //        .Select(e => new
        //        {
        //            Holiday_id = e.Holiday.Id.ToString(),
        //            Holiday_val = e.Holiday.FullDetails
        //        }).ToList();
        //    return Json(new Object[] { data1, data2 }, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.HolidayList.Include(e => e.Holiday.HolidayName).Include(e => e.Holiday.HolidayType).Where(e => e.Id == data)
                        .Select(e => new
                        {
                            HolidayId = e.Holiday.Id,
                            HolidayVal = e.Holiday.FullDetails,
                            holidaydate = e.HolidayDate

                        }).ToList();

                var Corp = db.HolidayList.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        //public ActionResult Edit(int data)
        //{
        //    var Q = db.HolidayList.Include(e => e.Holiday.HolidayName).Include(e => e.Holiday.HolidayType).Where(e => e.Id == data)
        //            .Select(e => new
        //            {
        //                //HolidayId = e.Holiday.Id,
        //                //HolidayVal = e.Holiday.FullDetails,
        //                HolidayDate = e.HolidayDate

        //            }).ToList();
        //    var addata = db.HolidayList.Include(e => e.Holiday).Include(e => e.Holiday.HolidayName).Include(e => e.Holiday.HolidayType).Select(e => new
        //    {
        //        Holli_FullDetails = e.Holiday.FullDetails == null ? "" : e.Holiday.FullDetails,
        //        Holli_Id = e.Holiday.Id == null ? "" : e.Holiday.Id.ToString(),


        //    }).ToList();
        //    var Corp = db.HolidayList.Find(data);
        //    TempData["RowVersion"] = Corp.RowVersion;
        //    var Auth = Corp.DBTrack.IsModified;
        //    return Json(new Object[] { Q, addata, "", Auth, JsonRequestBehavior.AllowGet });

        //}

        //public ActionResult Edit(int data)
        //{
        //    var Q = db.HolidayList.Include(e => e.Holiday.HolidayName).Include(e => e.Holiday.HolidayType).Where(e => e.Id == data)
        //            .Select(e => new
        //            {
        //                //Holli_Id = e.Holiday.Id,
        //                //Holli_FullDetails = e.Holiday.FullDetails,
        //                HolidayDate = e.HolidayDate

        //            }).ToList();
        //    //.Include(e => e.Holiday.HolidayName).Include(e => e.Holiday.HolidayType)
        //    var addata = db.HolidayList.Include(e => e.Holiday.HolidayName).Include(e => e.Holiday.HolidayType).Where(e => e.Id == data).Select(e => new
        //    {
        //        Holli_FullDetails = e.Holiday.FullDetails == null ? "" : e.Holiday.FullDetails,
        //        Holli_Id = e.Holiday.Id == null ? "" : e.Holiday.Id.ToString(),


        //    }).ToList();
        //    var Corp = db.HolidayList.Find(data);
        //    TempData["RowVersion"] = Corp.RowVersion;
        //    var Auth = Corp.DBTrack.IsModified;
        //    return Json(new Object[] { Q, addata, "", Auth, JsonRequestBehavior.AllowGet });

        //}

        //[HttpPost]
        //public async Task<ActionResult> EditSave(HolidayList FormHolidayList, int data, FormCollection form) // Edit submit
        //{
        //    string HolidayList = form["HolidayList"] == "0" ? null : form["HolidayList"];

        //    if (HolidayList != null)
        //    {
        //        var val = db.Holiday.Find(int.Parse(HolidayList));
        //        FormHolidayList.Holiday = val;
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        var db_data = db.HolidayList
        //            .Include(e => e.Holiday)
        //            .Where(e => e.Id == data).SingleOrDefault();
        //        db_data.Holiday = FormHolidayList.Holiday;
        //        db_data.HolidayDate = FormHolidayList.HolidayDate;
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            db.HolidayList.Attach(db_data);
        //            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
        //        }
        //        return Json(new Object[] { db_data.Id, db_data.HolidayDate.Value.ToShortDateString(), "Record Updated", JsonRequestBehavior.AllowGet });
        //    }
        //    else
        //    {
        //        StringBuilder sb = new StringBuilder("");
        //        foreach (ModelState modelState in ModelState.Values)
        //        {
        //            foreach (ModelError error in modelState.Errors)
        //            {
        //                sb.Append(error.ErrorMessage);
        //                sb.Append("." + "\n");
        //            }
        //        }
        //        var errorMsg = sb.ToString();
        //        return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });

        //    }

        //}

        [HttpPost]
        public async Task<ActionResult> EditSave(HolidayList P, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {

                string HolidayList = form["HolidayList"] == "0" ? null : form["HolidayList"];

                //if (HolidayList != null)
                //{
                //    var val = db.Holiday.Find(int.Parse(HolidayList));
                //    P.Holiday = val;
                //}

                var db_data = db.HolidayList.Include(e=>e.Holiday).Include(e => e.Holiday.HolidayName).Include(e => e.Holiday.HolidayType).Where(e => e.Id == data).SingleOrDefault();
                List<HolidayList> LocationDetails = new List<HolidayList>();
                string Values = form["HolidayList"];

                if (Values != null)
                {
                    var ids = Utility.StringIdsToListIds(Values);
                    foreach (var ca in ids)
                    {
                        var LocationDetails_val = db.Holiday.Include(e => e.HolidayName).Include(e => e.HolidayType).Where(e => e.Id == ca).SingleOrDefault();

                        db_data.Holiday = LocationDetails_val;
                    }
                }
                else
                {
                    db_data.Holiday = null;
                }
                string holidayfull = "Holiday name:" + db_data.Holiday.HolidayName.LookupVal.ToString() + ", Holiday type:" + db_data.Holiday.HolidayType.LookupVal.ToString() + ", ";
                //if (HolidayList != null)
                //{
                //    if (HolidayList != "")
                //    {
                //        int AddId = Convert.ToInt32(HolidayList);
                //        var val = db.HolidayList.Include(e => e.Holiday)

                //                            .Where(e => e.Id == AddId).SingleOrDefault();
                //        FormHolidayList.FullDetails = val;
                //    }
                //}
                //if (ModelState.IsValid)
                //{

                //    var db_data = db.HolidayList
                //        .Include(e => e.Holiday.HolidayName)
                //           .Include(e => e.Holiday.HolidayType)
                //        .Where(e => e.Id == data).SingleOrDefault();
                //    db_data.Holiday = FormHolidayList.Holiday;
                //    db_data.HolidayDate = FormHolidayList.HolidayDate;



                //    using (TransactionScope ts = new TransactionScope())
                //    {
                //        db.HolidayList.Attach(db_data);
                //        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                //        db.SaveChanges();
                //        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                //    }
                //    var vbdata = db_data;
                //    Msg.Add("  Record Updated");
                //    return Json(new Utility.JsonReturnClass { Id = vbdata.Id, Val = vbdata.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                //    //return Json(new Object[] { db_data.Id, db_data.Holiday.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                //}
                if (ModelState.IsValid)
                {
                    try
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            HolidayList blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.HolidayList.Where(e => e.Id == data).Include(e => e.Holiday).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }

                            P.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            var m1 = db.HolidayList.Where(e => e.Id == data).ToList();
                            foreach (var s in m1)
                            {
                                // s.AppraisalPeriodCalendar = null;
                                db.HolidayList.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                            //int a = EditS(Category, PromoPolicy, data, c, c.DBTrack);
                            var CurCorp = db.HolidayList.Find(data);
                            TempData["CurrRowVersion"] = CurCorp.RowVersion;
                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                            {

                                HolidayList corp = new HolidayList()
                                    {
                                        HolidayDate = P.HolidayDate,
                                        Holiday_Id = db_data.Holiday_Id,
                                        Id = data,
                                        DBTrack = P.DBTrack
                                    };

                                db.HolidayList.Attach(corp);
                                db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                // return 1;

                                holidayfull += corp.HolidayDate;
                                //using (var context = new DataBaseContext())
                                //{
                                //    var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, P.DBTrack);
                                //    DT_Holiday DT_Corp = (DT_Holiday)obj;

                                //   db.Create(DT_Corp);
                                //   db.SaveChanges();
                                //}
                                db.SaveChanges();
                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = holidayfull, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { c.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                            }
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
                            P.RowVersion = databaseValues.RowVersion;

                        }
                    }
                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
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

        }
        public class P2BGridClass
        {
            public int Id { get; set; }
            public string Holiday { get; set; }
            public string HolidayDate { get; set; }

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
                IEnumerable<P2BGridClass> HolidayList = null;
                var data = db.HolidayList.Include(e => e.Holiday).ToList();
                List<P2BGridClass> holidaylist = new List<P2BGridClass>();
                foreach (var item in data)
                {
                    holidaylist.Add(new P2BGridClass
                    {
                        Id = item.Id,
                        HolidayDate = item.HolidayDate != null ? item.HolidayDate.Value.ToShortDateString() : null,
                        Holiday = item.Holiday != null ? item.Holiday.FullDetails : null
                    });
                }
                HolidayList = holidaylist;
                IEnumerable<P2BGridClass> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = HolidayList;
                    if (gp.searchOper.Equals("eq"))
                    {
                        if (gp.searchField == "Id")
                            jsonData = IE.Select(a => new { a.Id, a.Holiday, a.HolidayDate }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "Holiday")
                            jsonData = IE.Select(a => new { a.Id, a.Holiday, a.HolidayDate }).Where((e => (e.Holiday.ToString().Contains(gp.searchString)))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Holiday }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = HolidayList;
                    Func<P2BGridClass, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Holiday" ? c.Holiday : gp.sidx == "HolidayDate" ? c.HolidayDate
                                          : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Holiday, a.HolidayDate }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Holiday, a.HolidayDate }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Holiday, a.HolidayDate }).ToList();
                    }
                    totalRecords = HolidayList.Count();
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
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var HolidayLists = db.HolidayList
                        .Include(e => e.Holiday)
                        .Where(e => e.Id == data).SingleOrDefault();
                    if (HolidayLists.Holiday != null)
                    {
                        Msg.Add(" Child record exists.Cannot remove it..  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        // return Json(new Object[] { "", "", "Child Record Exits..!" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.Entry(HolidayLists).State = System.Data.Entity.EntityState.Deleted;
                                db.SaveChanges();
                                Msg.Add(" Record Deleted ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "", "Record Deleted..!" }, JsonRequestBehavior.AllowGet);
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
                            //return Json(new Object[] { "", "", e.InnerException.ToString() }, JsonRequestBehavior.AllowGet);
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
        }


        public ActionResult GetLookup(List<int> SkipIds, string Param, string Param1)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int HOCalendardropID = Convert.ToInt32(Param);
                var Holicalendarid = db.HolidayCalendar
                                     .Include(e => e.HoliCalendar)
                                     .Include(e => e.HolidayList)
                                     .Include(e => e.HolidayList.Select(t => t.Holiday))
                                     .Include(e => e.HolidayList.Select(t => t.Holiday.HolidayName))
                                     .Include(e => e.HolidayList.Select(t => t.Holiday.HolidayType))
                                     .Where(e => e.HoliCalendar.Id == HOCalendardropID && e.Name == Param1 ).SingleOrDefault();

                // var fall = db.HolidayList.Include(e => e.Holiday.HolidayName).Include(e => e.Holiday.HolidayType).ToList();
                if (Holicalendarid != null)
                {


                    var Holidaydata = db.HolidayCalendar.SelectMany(t => t.HolidayList).ToList();
                    List<int> Holiofflistid = Holidaydata.Select(e => e.Id).ToList();
                    //var fall = db.HolidayList.Include(e => e.Holiday).Include(e => e.Holiday.HolidayType).Include(e => e.Holiday.HolidayName).Where(e => !Holiofflistid.Contains(e.Id)).ToList();
                    var fall = db.HolidayList.Include(e => e.Holiday).Include(e => e.Holiday.HolidayType).Include(e => e.Holiday.HolidayName).ToList();
                    if (SkipIds != null)
                    {
                        foreach (int a in SkipIds)
                        {
                            //if (fall == null)
                            //    fall = db.HolidayList.Include(e => e.Holiday).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                            //else
                            fall = fall.Where(e => e.Id != a).ToList();
                        }
                    }

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.Holiday.FullDetails + ", " + ca.HolidayDate }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var Holidaydata = db.HolidayCalendar.Include(e => e.HoliCalendar).Where(e => e.HoliCalendar.Id == HOCalendardropID).SelectMany(t => t.HolidayList).ToList();
                    List<int> Holiofflistid = Holidaydata.Select(e => e.Id).ToList();
                    var fall = db.HolidayList.Include(e => e.Holiday).Include(e => e.Holiday.HolidayType).Include(e => e.Holiday.HolidayName).Where(e => Holiofflistid.Contains(e.Id)).ToList();
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.Holiday.FullDetails + ", " + ca.HolidayDate }).Distinct();
                   
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
            }

            // return View();
        }


        //public ActionResult GetLookup(List<int> SkipIds)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.HolidayList.Include(e => e.Holiday.HolidayName).Include(e => e.Holiday.HolidayType).ToList();
        //        if (SkipIds != null)
        //        {
        //            foreach (var a in SkipIds)
        //            {
        //                if (fall == null)
        //                    fall = db.HolidayList.Include(e => e.Holiday).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
        //                else
        //                    fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

        //            }
        //        }

        //        var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.Holiday.HolidayName }).Distinct();

        //        return Json(r, JsonRequestBehavior.AllowGet);


        //    }
        //    // return View();
        //}

        public void RollBack()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //  var context = DataContextFactory.GetDataContext();
                var changedEntries = db.ChangeTracker.Entries()
                    .Where(x => x.State != System.Data.Entity.EntityState.Unchanged).ToList();

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Modified))
                {
                    entry.CurrentValues.SetValues(entry.OriginalValues);
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Added))
                {
                    entry.State = System.Data.Entity.EntityState.Detached;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Deleted))
                {
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }

            }
        }

    }
}