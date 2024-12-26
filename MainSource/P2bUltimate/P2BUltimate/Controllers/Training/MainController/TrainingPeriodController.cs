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
using System.Reflection;
using Training;
using P2BUltimate.Controllers;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Training.MainController
{
    public class TrainingPeriodController : Controller
    {
       
        //
        // GET: /TrainingPeriod/
        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/TrainingPeriod/Index.cshtml");
        }
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.TrainingPeriod.Where(e => e.Id == data).AsEnumerable()
                    .Select(e => new
                    {
                        StartDate = e.StartDate,
                        EndDate = e.EndDate

                    }).ToList();
                return Json(new Object[] { returndata, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public ActionResult Create(TrainingPeriod c, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };
                if (ModelState.IsValid)
                {
                    TrainingPeriod trainingperiod = new TrainingPeriod()
                    {

                        StartDate = c.StartDate,
                        EndDate = c.EndDate,

                        DBTrack = c.DBTrack
                    };
                    try
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (c.StartDate > c.EndDate)
                            {
                                return this.Json(new Object[] { "", "", "To Date should be greater than From Date.", JsonRequestBehavior.AllowGet });
                            }


                            db.TrainingPeriod.Add(trainingperiod);
                            db.SaveChanges();



                            db.SaveChanges();
                            ts.Complete();
                        }
                        return Json(new Object[] { "", "", "Data saved successfully." }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new { Error = calendar.Id });
                        //return RedirectToAction("Index");
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                    }
                    catch (DataException /* dex */)
                    {
                        //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                        ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                        return View(c);
                    }
                }
                return View(c);
            }
        }

   
        [HttpPost]
        // GET: /Calendar/Edit/5
        public ActionResult EditSave(TrainingPeriod data1, int data, FormCollection form)
        {
            //Calendar c = db.Calendar.Find(data);
            using (DataBaseContext db = new DataBaseContext())
            {
                var db_data = db.TrainingPeriod.Find(data);
                db_data.StartDate = data1.StartDate;
                db_data.EndDate = data1.EndDate;

                if (ModelState.IsValid)
                {
                    TrainingPeriod blog = null; // to retrieve old data
                    DbPropertyValues originalBlogValues = null;
                    TrainingPeriod Old_Corp = null;

                    using (var context = new DataBaseContext())
                    {
                        blog = context.TrainingPeriod.Where(e => e.Id == data).SingleOrDefault();
                        originalBlogValues = context.Entry(blog).OriginalValues;
                    }
                    data1.DBTrack = new DBTrack
                    {
                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                        Action = "M",
                        ModifiedBy = SessionManager.UserName,
                        ModifiedOn = DateTime.Now
                    };
                    TrainingPeriod calendar = new TrainingPeriod()
                    {
                        StartDate = data1.StartDate,
                        EndDate = data1.EndDate,


                        Id = data,
                    };
                    try
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                        }

                        return this.Json(new Object[] { "", "", "Data saved successfully." });
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Edit", new { concurrencyError = true, id = data1.Id });
                    }
                    catch (DataException /* dex */)
                    {
                        //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                        ModelState.AddModelError(string.Empty, "Unable to edit. Try again, and if the problem persists contact your system administrator.");
                        return View(data1);
                    }
                }
                return View(data1);
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Delete(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                TrainingPeriod calendar = db.TrainingPeriod.Find(data);
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(calendar).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    return Json(new Object[] { "", "Data deleted.", JsonRequestBehavior.AllowGet });
                }

                catch (DataException /* dex */)
                {
                    //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                    return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
                    //ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
                    //return RedirectToAction("Index");
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
                var Calendar = db.TrainingPeriod.ToList();
                IEnumerable<TrainingPeriod> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Calendar;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.StartDate), Convert.ToString(a.EndDate) }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id,  a.StartDate, a.EndDate }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Calendar;
                    Func<TrainingPeriod, string> orderfuc = (c =>
                                                               gp.sidx == "ID" ? c.Id.ToString() :
                                                              
                                                               gp.sidx == "From Date" ? c.StartDate.ToString() :
                                                               gp.sidx == "To Date" ? c.EndDate.ToString() :
                                                                "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.StartDate.ToString(), a.EndDate.ToString() }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.StartDate.ToString(), a.EndDate.ToString()  }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.StartDate, a.EndDate}).ToList();
                    }
                    totalRecords = Calendar.Count();
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

        //For Validating the form error
        public ActionResult ValidateForm(TrainingPeriod c, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (c.StartDate > c.EndDate)
                {
                    return Json(new { success = false, responseText = "To Date should be greater than From Date." }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new Object[] { "", "", "To Date should be greater than From Date.", JsonRequestBehavior.AllowGet });
                }

                if (db.TrainingPeriod.Any(o => o.StartDate == c.StartDate))
                {
                    //return this.Json(new Object[] { "", "", "From Date already exists.", JsonRequestBehavior.AllowGet });
                    return Json(new { success = false, responseText = "From Date already exists." }, JsonRequestBehavior.AllowGet);
                }

                if (db.TrainingPeriod.Any(o => o.EndDate == c.EndDate))
                {
                    //ModelState.AddModelError(string.Empty, "To Date already exists.");
                    // return this.Json(new Object[] { "", "", "To Date already exists.", JsonRequestBehavior.AllowGet });
                    return Json(new { success = false, responseText = "To Date already exists." }, JsonRequestBehavior.AllowGet);
                    // return View(c);
                }
                // for success
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                //for error
                //return Json(new { success = false, responseText = "Not Valid..!" }, JsonRequestBehavior.AllowGet);
            }
        }
	}
}