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
    public class WeeklyOffCalendarController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/WeeklyOffCalendar/Index.cshtml");
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Core/_WeeklyOffCalendar.cshtml");
        }

        //public ActionResult PopulateDropDownListCalendar(string data, string data2)
        //{
        //    var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "weaklyofcalendar" && e.Default == true);
        //    var selected = "";
        //    if (!string.IsNullOrEmpty(data))
        //    {
        //        selected = data2;
        //    }
        //    var returndata = new SelectList(qurey, "id", "FullDetails", selected);
        //    return Json(returndata, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public ActionResult Create(WeeklyOffCalendar frmWeeklyOffCalendar, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int WOCalendar_DDL = form["WOCalendar_DDL"] == "0" ? 0 : Convert.ToInt32(form["WOCalendar_DDL"]);
                    string WeeklyOffListList = form["WeeklyOffListList"] == null ? null : form["WeeklyOffListList"];

                    if (db.WeeklyOffCalendar.Include(q => q.WOCalendar).Any(e => e.WOCalendar.Id == WOCalendar_DDL && e.Name == frmWeeklyOffCalendar.Name))
                    {
                        Msg.Add(" Data with this Calendar Year Already Exist. ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    int comp_Id = 0;
                    comp_Id = Convert.ToInt32(Session["CompId"]);
                    var Company = new Company();
                    Company = db.Company.Include(e => e.Location).Where(e => e.Id == comp_Id).FirstOrDefault();
                    if (WOCalendar_DDL != null)
                    {
                        var val = db.Calendar.Include(e=>e.Name).Where(e=>e.Id==WOCalendar_DDL).SingleOrDefault();
                        frmWeeklyOffCalendar.WOCalendar = val;
                    }

                    //if (WeeklyOffListList != null)
                    //{
                    //    var ids = Utility.StringIdsToListIds(WeeklyOffListList);
                    //    var WeeklyOffList = new List<WeeklyOffList>();
                    //    foreach (var item in ids)
                    //    {

                    //        int WeeklyOffListid = Convert.ToInt32(item);
                    //        var val = db.WeeklyOffList.Find(WeeklyOffListid);
                    //        WeeklyOffList.Add(val);


                    //    }
                    //    frmWeeklyOffCalendar.WeeklyOffList = WeeklyOffList;
                    //}


                    if (WeeklyOffListList != null)
                    {
                        var ids = Utility.StringIdsToListIds(WeeklyOffListList);
                        var Weeklylist = new List<WeeklyOffList>();
                        foreach (var item in ids)
                        {

                            int WeeklyListid = Convert.ToInt32(item);
                            var val = db.WeeklyOffList.Include(e => e.WeekDays).Include(e => e.WeeklyOffStatus).Where(e => e.Id == WeeklyListid).SingleOrDefault();
                            if (val != null)
                            {
                                Weeklylist.Add(val);
                            }
                        }
                        frmWeeklyOffCalendar.WeeklyOffList = Weeklylist;
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            frmWeeklyOffCalendar.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            WeeklyOffCalendar WeeklyOffCalendar = new WeeklyOffCalendar()
                            {
                                WOCalendar = frmWeeklyOffCalendar.WOCalendar,
                                WeeklyOffList = frmWeeklyOffCalendar.WeeklyOffList,
                                DBTrack = frmWeeklyOffCalendar.DBTrack,
                                Name = frmWeeklyOffCalendar.Name 
                            };
                            try
                            {
                                db.WeeklyOffCalendar.Add(WeeklyOffCalendar);
                                db.SaveChanges();
                                List<WeeklyOffCalendar> objWeeklyOffCalendar = new List<WeeklyOffCalendar>();
                                if (Company != null)
                                {
                                    objWeeklyOffCalendar.Add(WeeklyOffCalendar);
                                    Company.WeeklyOffCalendar = objWeeklyOffCalendar;

                                    //foreach (var item in Company.Location)
                                    //{
                                    //    item.WeeklyOffCalendar.Add(WeeklyOffCalendar);
                                    //}
                                    db.Company.Attach(Company);
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Detached;

                                }
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = frmWeeklyOffCalendar.Id, Val = frmWeeklyOffCalendar.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { , , "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = "" });
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

        public class WeeklyoffListtdetails
        {
            public Array WeeklyoffList_Id { get; set; }
            public Array WeeklyoffList_val { get; set; }
        }

        //public ActionResult Edit(int data)
        //{
        //    var Q = db.WeeklyOffCalendar.Include(e => e.WOCalendar).Where(e => e.Id == data).Select(e => new
        //    {

        //        WOCalendar_Id = e.WOCalendar != null ? e.WOCalendar.Id : 0,


        //    }).ToList();
        //    List<WeeklyoffListtdetails> objlist = new List<WeeklyoffListtdetails>();



        //    var N = db.WeeklyOffCalendar.Include(e=>e.WeeklyOffList).Include(e=>e.WeeklyOffList.Select(r=>r.WeekDays))
        //        .Include(e=>e.WeeklyOffList.Select(r=>r.WeeklyOffStatus))
        //        .Where(e => e.Id == data).ToList();
        //  //  var NA = N.Select(e => e.WeeklyOffList).ToList();

        //    if (N != null && N.Count > 0)
        //    {
        //        foreach (var ca in N)
        //        {
        //            objlist.Add(new WeeklyoffListtdetails
        //            {

        //                WeeklyoffList_Id = ca.Select(e => e.Id).ToArray(),
        //                WeeklyoffList_val = ca.Select(e => e.FullDetails).ToArray()
        //            });
        //        }
        //    }
        //    var Corp = db.WeeklyOffCalendar.Find(data);
        //    TempData["RowVersion"] = Corp.RowVersion;
        //    var Auth = Corp.DBTrack.IsModified;
        //    return Json(new Object[] { Q, objlist, "", Auth, JsonRequestBehavior.AllowGet });
        //}
        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.WeeklyOffCalendar.Include(e => e.WOCalendar).Where(e => e.Id == data).Select(e => new
                {
                    WOCalendar_Id = e.WOCalendar != null ? e.WOCalendar.Id : 0,
                    Name = e.Name
                }).ToList();

                List<WeeklyoffListtdetails> pst = new List<WeeklyoffListtdetails>();

                var a = db.WeeklyOffCalendar.Include(e => e.WeeklyOffList).Include(e => e.WeeklyOffList.Select(r => r.WeekDays))
                    .Include(e => e.WeeklyOffList.Select(r => r.WeeklyOffStatus)).Where(e => e.Id == data).ToList();
                foreach (var ca in a)
                {
                    pst.Add(new WeeklyoffListtdetails
                    {
                        WeeklyoffList_Id = ca.WeeklyOffList.Select(e => e.Id.ToString()).ToArray(),
                        WeeklyoffList_val = ca.WeeklyOffList.Select(e => e.FullDetails).ToArray(),
                    });
                }

                var Corp = db.WeeklyOffCalendar.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, pst, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        //[HttpPost]
        //public async Task<ActionResult> EditSave(WeeklyOffCalendar frmWeeklyOffCalendar, int data, FormCollection form) // Edit submit
        //{
        //    string WOCalendar_DDL = form["WOCalendar_DDL"] == "0" ? null : form["WOCalendar_DDL"];
        //    string WeeklyOffListList = form["WeeklyOffListList"] == null ? null : form["WeeklyOffListList"];

        //    if (WOCalendar_DDL != null)
        //    {
        //        var val = db.Calendar.Find(int.Parse(WOCalendar_DDL));
        //        frmWeeklyOffCalendar.WOCalendar = val;
        //    }

        //    if (WeeklyOffListList != null)
        //    {
        //        var ids = Utility.StringIdsToListIds(WeeklyOffListList);
        //        var WeeklyOffList = new List<WeeklyOffList>();
        //        foreach (var item in ids)
        //        {

        //            int WeeklyOffListid = Convert.ToInt32(item);
        //            var val = db.WeeklyOffList.Where(e => e.Id == WeeklyOffListid).SingleOrDefault();
        //            if (val != null)
        //            {
        //                WeeklyOffList.Add(val);
        //            }
        //        }
        //        frmWeeklyOffCalendar.WeeklyOffList = WeeklyOffList;
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        var db_data = db.WeeklyOffCalendar
        //            .Include(e => e.WeeklyOffList)
        //            .Include(e => e.WOCalendar).Where(e => e.Id == data).SingleOrDefault();
        //        db_data.WOCalendar = frmWeeklyOffCalendar.WOCalendar;
        //        db_data.WeeklyOffList = frmWeeklyOffCalendar.WeeklyOffList;
        //        using (TransactionScope ts = new TransactionScope())
        //        {

        //            db.WeeklyOffCalendar.Attach(db_data);
        //            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
        //        }
        //        return Json(new Object[] { db_data.Id, db_data.WOCalendar.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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
        //        Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //    }

        //}

        [HttpPost]
        public async Task<ActionResult> EditSave(WeeklyOffCalendar L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string WOCalendar_DDL = form["WOCalendar_DDL"] == "0" ? null : form["WOCalendar_DDL"];
                    string WeeklyOffListList = form["WeeklyOffListList"] == null ? null : form["WeeklyOffListList"];

                    if (WOCalendar_DDL != null && WOCalendar_DDL != "" && WOCalendar_DDL != "-Select-")
                    {


                        L.WOCalendar_Id = int.Parse(WOCalendar_DDL);

                    }
                    else
                    {
                        L.WOCalendar_Id = null;
                    }

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    WeeklyOffCalendar blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.WeeklyOffCalendar.Where(e => e.Id == data).Include(e => e.WOCalendar).Include(e=>e.WOCalendar.Name)
                                                                .Include(e => e.WeeklyOffList)
                                                                .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    L.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    if (WOCalendar_DDL != null)
                                    {
                                        if (WOCalendar_DDL != "")
                                        {
                                            var val = db.Calendar.Find(int.Parse(WOCalendar_DDL));
                                            L.WOCalendar = val;
                                            var type = db.WeeklyOffCalendar.Include(e => e.WOCalendar).Include(e => e.WOCalendar.Name).Where(e => e.Id == data).SingleOrDefault();
                                           // var type = db.WeeklyOffCalendar.Include(e => e.WOCalendar).Where(e => e.Id == data).SingleOrDefault();
                                            IList<WeeklyOffCalendar> typedetails = null;
                                            if (type.WOCalendar != null)
                                            {
                                                typedetails = db.WeeklyOffCalendar.Include(e => e.WOCalendar).Include(e => e.WeeklyOffList).Include(e => e.WOCalendar.Name).Where(x => x.WOCalendar.Id == type.WOCalendar.Id && x.Id == data).ToList();

                                               // typedetails = db.WeeklyOffCalendar.Where(x => x.WOCalendar.Id == type.WOCalendar.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.WeeklyOffCalendar.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.WOCalendar = L.WOCalendar;
                                                db.WeeklyOffCalendar.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var WFTypeDetails = db.WeeklyOffCalendar.Include(e => e.WOCalendar).Include(e => e.WOCalendar.Name).Where(x => x.Id == data).ToList();
                                          //  var WFTypeDetails = db.WeeklyOffCalendar.Include(e => e.WOCalendar).Where(x => x.Id == data).ToList();
                                            foreach (var s in WFTypeDetails)
                                            {
                                                s.WOCalendar = null;
                                                db.WeeklyOffCalendar.Attach(s);
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
                                        var WeeklyOffCalendarDetails = db.WeeklyOffCalendar.Include(e => e.WOCalendar).Where(x => x.Id == data).ToList();
                                        foreach (var s in WeeklyOffCalendarDetails)
                                        {
                                            s.WOCalendar = null;
                                            db.WeeklyOffCalendar.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    List<WeeklyOffList> ObjWeeeklyoffList = new List<WeeklyOffList>();
                                    WeeklyOffCalendar WeeklyOffCalendardetails = null;
                                    WeeklyOffCalendardetails = db.WeeklyOffCalendar.Include(e => e.WeeklyOffList).Where(e => e.Id == data).SingleOrDefault();
                                    if (WeeklyOffListList != null && WeeklyOffListList != "")
                                    {
                                        var ids = Utility.StringIdsToListIds(WeeklyOffListList);
                                        foreach (var ca in ids)
                                        {
                                            var WeeklyOffListlistvalue = db.WeeklyOffList.Find(ca);
                                            ObjWeeeklyoffList.Add(WeeklyOffListlistvalue);
                                            WeeklyOffCalendardetails.WeeklyOffList = ObjWeeeklyoffList;
                                        }
                                    }
                                    else
                                    {
                                        WeeklyOffCalendardetails.WeeklyOffList = null;
                                    }

                                    var CurCorp = db.WeeklyOffCalendar.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        WeeklyOffCalendar WeeklyOffCalendar = new WeeklyOffCalendar()
                                        {

                                            Name = L.Name,
                                            Id = data,
                                            DBTrack = L.DBTrack,
                                            WOCalendar=L.WOCalendar,
                                            WOCalendar_Id=L.WOCalendar_Id
                                        };
                                        db.WeeklyOffCalendar.Attach(WeeklyOffCalendar);
                                        db.Entry(WeeklyOffCalendar).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(WeeklyOffCalendar).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    }
                                    // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                    using (var context = new DataBaseContext())
                                    {
                                        //var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                        //dt_holiday DT_Corp = (DT_LvCreditPolicy)obj;
                                        //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;
                                        //db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    var query=db.WeeklyOffCalendar.Include(e => e.WOCalendar).Include(e => e.WeeklyOffList).Where(e => e.Id == data).SingleOrDefault();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id =query.Id, Val = query.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] {, , "Record Updated", JsonRequestBehavior.AllowGet });

                                }

                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (WeeklyOffCalendar)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (WeeklyOffCalendar)databaseEntry.ToObject();
                                    L.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            WeeklyOffCalendar blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            WeeklyOffCalendar Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.WeeklyOffCalendar.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            L.DBTrack = new DBTrack
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

                            WeeklyOffCalendar corp = new WeeklyOffCalendar()
                            {
                                Name = L.Name,
                                WOCalendar = L.WOCalendar,
                                WeeklyOffList = L.WeeklyOffList,
                                Id = data,
                                DBTrack = L.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                //var obj = DBTrackFile.ModifiedDataHistory("Leave/Leave", "M", blog, corp, "LvEncashReq", L.DBTrack);
                                //// var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                //Old_Corp = context.LvCreditPolicy.Where(e => e.Id == data).Include(e => e.ConvertLeaveHead)
                                //    .Include(e => e.ConvertLeaveHeadBal).Include(e => e.ExcludeLeaveHeads).Include(e => e.CreditDate).SingleOrDefault();
                                //DT_LvCreditPolicy DT_Corp = (DT_LvCreditPolicy)obj;
                                //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;

                                //db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = L.DBTrack;
                            db.WeeklyOffCalendar.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            return Json(new Object[] { blog.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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

        public class P2BGridClass
        {
            public int Id { get; set; }
            public string WOCalendar { get; set; }
            public string Name { get; set; }
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
                IEnumerable<P2BGridClass> WeeklyOffCalendar = null;
                var data = db.WeeklyOffCalendar.Include(e => e.WOCalendar).Include(e => e.WOCalendar.Name).Include(e => e.WeeklyOffList).ToList();

                List<P2BGridClass> WeeklyOffList = new List<P2BGridClass>();
                foreach (var item in data)
                {
                    WeeklyOffList.Add(new P2BGridClass
                    {
                        Id = item.Id,
                        WOCalendar = item.WOCalendar != null ? item.WOCalendar.FullDetails : null,
                        Name = item.Name
                    });
                }
                WeeklyOffCalendar = WeeklyOffList;
                IEnumerable<P2BGridClass> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = WeeklyOffCalendar;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e =>  (e.WOCalendar.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.Id.ToString().Contains(gp.searchString))
                                || (e.Name.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.WOCalendar, a.Name, a.Id }).ToList();
                            //jsonData = IE.Select(a => new { a.WOCalendar, a.Id }).Where((e => (e.WOCalendar.ToString().Contains(gp.searchString)))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.WOCalendar, a.Name, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = WeeklyOffCalendar;
                    Func<P2BGridClass, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "WeeklyOffCalendar" ? c.WOCalendar
                                          : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.WOCalendar, a.Name, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.WOCalendar,a.Name, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.WOCalendar,a.Name, a.Id }).ToList();
                    }
                    totalRecords = WeeklyOffCalendar.Count();
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

        //[HttpPost]
        //public async Task<ActionResult> Delete(int data)
        //{

        //    var WeeklyOffCalendars = db.WeeklyOffCalendar
        //        .Include(e => e.WOCalendar)
        //        .Include(e => e.WeeklyOffList)
        //        .Where(e => e.Id == data).SingleOrDefault();
        //    if (WeeklyOffCalendars.WOCalendar != null && WeeklyOffCalendars.WeeklyOffList != null)
        //    {
        //        return Json(new Object[] { "", "", "Child Record Exits..!" }, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        try
        //        {
        //            using (TransactionScope ts = new TransactionScope())
        //            {


        //                db.Entry(WeeklyOffCalendars).State = System.Data.Entity.EntityState.Deleted;
        //                db.SaveChanges();
        //                return Json(new Object[] { "", "", "Record Deleted..!" }, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //        catch (Exception e)
        //        {

        //            return Json(new Object[] { "", "", e.InnerException.ToString() }, JsonRequestBehavior.AllowGet);
        //        }

        //    }

        //}


        public ActionResult GetLookup(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.WeeklyOffCalendar.Include(e => e.WOCalendar).Include(e => e.WeeklyOffList).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.WeeklyOffCalendar.Include(e => e.WOCalendar).Include(e => e.WeeklyOffList).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.WOCalendar.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
            // return View();
        }

        public void RollBack()
        {

            //  var context = DataContextFactory.GetDataContext();
            using (DataBaseContext db = new DataBaseContext())
            {
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


        public ActionResult PopulateDropDownListCalendar(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToString().ToUpper() == "WeeklyOFFCalendar" && e.Default == true).ToList();
                            
                var selected = "";
                if (!string.IsNullOrEmpty(data2))
                {
                    selected = data2;
                }
                var returndata = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateDropDownListCalendareditview(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                int cid = Convert.ToInt32(data2);
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToString().ToUpper() == "WeeklyOFFCalendar" && e.Id == cid).ToList();

                var selected = "";
                if (!string.IsNullOrEmpty(data2))
                {
                    selected = data2;
                }
                var returndata = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(returndata, JsonRequestBehavior.AllowGet);
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
                    WeeklyOffCalendar WeeklyOffCalendar = db.WeeklyOffCalendar.Include(e => e.WeeklyOffList)
                                                       .Include(e => e.WOCalendar)
                                                       .Where(e => e.Id == data).SingleOrDefault();


                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (WeeklyOffCalendar.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = WeeklyOffCalendar.DBTrack.CreatedBy != null ? WeeklyOffCalendar.DBTrack.CreatedBy : null,
                                CreatedOn = WeeklyOffCalendar.DBTrack.CreatedOn != null ? WeeklyOffCalendar.DBTrack.CreatedOn : null,
                                IsModified = WeeklyOffCalendar.DBTrack.IsModified == true ? true : false
                            };
                            WeeklyOffCalendar.DBTrack = dbT;
                            db.Entry(WeeklyOffCalendar).State = System.Data.Entity.EntityState.Modified;
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, holidaycalendar.DBTrack);
                            //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            //DT_Corp.Address_Id = corporates.Address == null ? 0 : corporates.Address.Id;
                            //DT_Corp.BusinessType_Id = corporates.BusinessType == null ? 0 : corporates.BusinessType.Id;
                            //DT_Corp.ContactDetails_Id = corporates.ContactDetails == null ? 0 : corporates.ContactDetails.Id;
                            //db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        var WeeklyOffList = WeeklyOffCalendar.WeeklyOffList;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            if (WeeklyOffList != null)
                            {
                                var WeeklyOffListDetails = new HashSet<int>(WeeklyOffCalendar.WeeklyOffList.Select(e => e.Id));
                                if (WeeklyOffListDetails.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                    // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                }
                            }

                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = WeeklyOffCalendar.DBTrack.CreatedBy != null ? WeeklyOffCalendar.DBTrack.CreatedBy : null,
                                    CreatedOn = WeeklyOffCalendar.DBTrack.CreatedOn != null ? WeeklyOffCalendar.DBTrack.CreatedOn : null,
                                    IsModified = WeeklyOffCalendar.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(WeeklyOffCalendar).State = System.Data.Entity.EntityState.Deleted;
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
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

        public ActionResult ValidateForm(WeeklyOffCalendar L, FormCollection form)
        {
            //string WOCalendar_DDL = form["WOCalendar_DDL"] == "0" ? null : form["WOCalendar_DDL"];
            //string WeeklyOffListList = form["WeeklyOffListList"] == null ? null : form["WeeklyOffListList"];
            //if (WOCalendar_DDL != null && WOCalendar_DDL != "-Select-")
            //{
            //    var value = db.Calendar.Find(int.Parse(WOCalendar_DDL));
            //    L.WOCalendar = value;
            //}
            //if (db.WeeklyOffCalendar.Any(e => e.WOCalendar.Id == L.WOCalendar.Id))
            //{
            //    var Msg = new List<string>();
            //    Msg.Add("WeeklyOfCalendar Already Exist");
            //    return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            //}
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }


    }
}