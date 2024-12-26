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
    public class HolidayCalendarController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Partial()
        {
            return View("~/Views/Shared/Core/_HolidayCalendar.cshtml");
        }
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/HolidayCalendar/Index.cshtml");
        }

        public ActionResult Create(HolidayCalendar FormHolidayCalendar, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string HoliCalendarDDL = form["HoliCalendarDDL"] == "0" ? null : form["HoliCalendarDDL"];
                    string HolidayListList = form["HolidayListList"] == null ? null : form["HolidayListList"];
                    int comp_Id = 0;
                    comp_Id = Convert.ToInt32(Session["CompId"]);
                    var oCompany = new Company();
                    int holiid = Convert.ToInt32(HoliCalendarDDL);
                    oCompany = db.Company.Include(e => e.Location)
                        .Where(e => e.Id == comp_Id).FirstOrDefault();

                    if (db.HolidayCalendar.Include(q => q.HoliCalendar).Any(e => e.HoliCalendar.Id == holiid && e.Name == FormHolidayCalendar.Name))
                    {
                        Msg.Add(" Data with this Calendar Year Already Exist. ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    Calendar cal = null;
                    if (HoliCalendarDDL != null)
                    {
                        int hdid = Convert.ToInt16(HoliCalendarDDL);
                        cal = db.Calendar.Include(a => a.Name).Where(q => q.Id == hdid).SingleOrDefault();//.Find(int.Parse(HoliCalendarDDL));
                        FormHolidayCalendar.HoliCalendar = cal;
                    }


                    //var alrDq = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "HOLIDAYCALENDAR")
                    //          .Any(q => ((q.FromDate <= cal.FromDate && q.ToDate <= cal.ToDate) || (q.FromDate <= cal.FromDate && q.ToDate >= cal.ToDate)) && (q.ToDate >= cal.FromDate));
                    //if (alrDq == true)
                    //{
                    //    Msg.Add("Year With this Period already exist.  ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    // *********** Start Duplicate HolidayList Checking Added on : 04-01-2024 By Anandrao **************
                    List<DateTime?> Datelist = new List<DateTime?>();
                    //  List<HolidayList> HOList = new List<HolidayList>();


                    if (HolidayListList != null && HolidayListList != "")
                    {
                        var HOidsList = Utility.StringIdsToListIds(HolidayListList);
                        foreach (var itemH in HOidsList)
                        {
                            var dbHOdetails = db.HolidayList.Find(itemH);
                            var getHOdate = dbHOdetails.HolidayDate;
                            Datelist.Add(getHOdate);

                        }
                        var aa = Datelist.Count();
                        var aaa = Convert.ToInt32(aa);
                        for (int i = 0; i < Datelist.Count(); i++)
                        {
                            for (int j = i + 1; j < Datelist.Count(); j++)
                            {
                                if (Datelist[i] == Datelist[j])
                                {
                                    Msg.Add("Duplicate Holiday Date Found : " + Datelist[j] + "  Please Keep Single Holiday Date.");


                                }
                            }

                        }
                        if (Msg.Count() > 0)
                        {
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }


                    }

                    // *********** End Duplicate HolidayList Checking Added on : 04-01-2024 By Anandrao **************



                    if (HolidayListList != null)
                    {
                        var ids = Utility.StringIdsToListIds(HolidayListList);
                        var HolidayList = new List<HolidayList>();
                        foreach (var item in ids)
                        {

                            int HolidayListid = Convert.ToInt32(item);
                            var val = db.HolidayList.Include(e => e.Holiday.HolidayName).Include(e => e.Holiday.HolidayType).Where(e => e.Id == HolidayListid).SingleOrDefault();
                            if (val != null)
                            {
                                HolidayList.Add(val);
                            }
                        }
                        FormHolidayCalendar.HolidayList = HolidayList;
                    }


                    //List<HolidayList> lookupLang = new List<HolidayList>();
                    //string Lang = form["HolidayListList"];

                    //if (Lang != null)
                    //{
                    //    var ids = Utility.StringIdsToListIds(Lang);
                    //    foreach (var ca in ids)
                    //    {
                    //        var Lookup_val = db.HolidayList.Include(e=>e.Holiday.HolidayName).Find(ca);

                    //        lookupLang.Add(Lookup_val);
                    //        FormHolidayCalendar.HolidayList = lookupLang;
                    //    }
                    //}
                    //else
                    //{
                    //    FormHolidayCalendar.HolidayList = null;
                    //}

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            FormHolidayCalendar.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            HolidayCalendar oHolidayCalendar = new HolidayCalendar()
                            {
                                HoliCalendar = FormHolidayCalendar.HoliCalendar,
                                HolidayList = FormHolidayCalendar.HolidayList,
                                DBTrack = FormHolidayCalendar.DBTrack,
                                Name = FormHolidayCalendar.Name
                            };
                            try
                            {



                                db.HolidayCalendar.Add(oHolidayCalendar);
                                db.SaveChanges();
                                List<HolidayCalendar> objholidaycalendar = new List<HolidayCalendar>();
                                if (oCompany != null)
                                {
                                    objholidaycalendar.Add(oHolidayCalendar);
                                    oCompany.HolidayCalendar = objholidaycalendar;
                                    db.Company.Attach(oCompany);
                                    db.Entry(oCompany).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    //foreach (var item in oCompany.Location)
                                    //{
                                    //    item.HolidayCalendar.Add(oHolidayCalendar);
                                    //    db.Location.Attach(item);
                                    //    db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                    //    db.SaveChanges();
                                    //   // db.Entry(item).State = System.Data.Entity.EntityState.Detached;
                                    //}
                                    db.Entry(oCompany).State = System.Data.Entity.EntityState.Detached;

                                }
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = FormHolidayCalendar.Id, Val = FormHolidayCalendar.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { , , "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
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
        //public ActionResult Edit(int data)
        //{
        //    var id = Convert.ToInt32(data);
        //    var data1 = db.Calendar.Where(e => e.Id == id).SingleOrDefault();
        //    var data2 = db.HolidayCalendar.Include(e => e.HolidayList)
        //        .Select(e => new
        //        {
        //            HolidayList_id = e.HolidayList.Select(a => a.Id.ToString()).ToArray(),
        //            HolidayList_val = e.HolidayList.Select(a => a.HolidayDate).ToArray()
        //        }).ToList();
        //    return Json(new Object[] { data1, data2 }, JsonRequestBehavior.AllowGet);
        //}


        public class holicalendarlistdetails
        {
            public string HolidayList_Id { get; set; }
            public string HolidayList_val { get; set; }
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.HolidayCalendar.Include(e => e.HolidayList).Where(e => e.Id == data).Select(e => new
                {

                    Holicalendar_Id = e.HoliCalendar != null ? e.HoliCalendar.Id : 0,
                    HolidayList_val = e.HoliCalendar != null ? e.HoliCalendar.FullDetails : "",
                    Name = e.Name

                }).ToList();
                List<holicalendarlistdetails> objlist = new List<holicalendarlistdetails>();
                var N = db.HolidayCalendar.Include(e => e.HolidayList)
                    .Include(q => q.HolidayList.Select(a => a.Holiday))
                    .Include(q => q.HolidayList.Select(a => a.Holiday.HolidayName))
                    .Include(q => q.HolidayList.Select(a => a.Holiday.HolidayType))
                    .Where(e => e.Id == data).SingleOrDefault();
                var holidaylist = N.HolidayList
                    .Where(q => q.Holiday != null && q.Holiday.HolidayName != null && q.Holiday.HolidayType != null)
                    .Select(e => new
                {
                    full = "HolidayName : " + e.Holiday.HolidayName.LookupVal + " ,HolidayDate : " + e.HolidayDate + " ,HolidayType : " + e.Holiday.HolidayType.LookupVal,
                    // full = "HolidayName :" + e.Holiday.HolidayName.LookupVal + "HolidayType :" + e.Holiday.HolidayType.LookupVal + "HolidayDate :" + e.HolidayDate,
                    Id = e.Id

                }).ToList();
                if (N != null)
                {
                    foreach (var ca in holidaylist)
                    {
                        objlist.Add(new holicalendarlistdetails
                        {
                            HolidayList_Id = ca.Id.ToString(),
                            HolidayList_val = ca.full.ToString()
                        });

                    }

                }
                var Corp = db.HolidayCalendar.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, objlist, "", Auth, JsonRequestBehavior.AllowGet });

            }
        }

        //public async Task<ActionResult> EditSave(HolidayCalendar FormHolidayCalendar, int data, FormCollection form) // Edit submit
        //{
        //    string HoliCalendarDDL = form["HoliCalendarDDL"] == "0" ? null : form["HoliCalendarDDL"];
        //    string HolidayListList = form["HolidayListList"] == null ? null : form["HolidayListList"];

        //    if (HoliCalendarDDL != null)
        //    {
        //        var val = db.Calendar.Find(int.Parse(HoliCalendarDDL));
        //        FormHolidayCalendar.HoliCalendar = val;
        //    }

        //    if (HolidayListList != null && HolidayListList != "" && HolidayListList != "0")
        //    {
        //        var ids = Utility.StringIdsToListIds(HolidayListList);
        //        var HolidayList = new List<HolidayList>();
        //        foreach (var item in ids)
        //        {

        //            int HolidayListid = Convert.ToInt32(item);
        //            //var val = db.HolidayList.Where(e => e.Id == HolidayListid).SingleOrDefault();
        //            var val = db.HolidayList.Find(HolidayListid);
        //            if (val != null)
        //            {
        //                HolidayList.Add(val);
        //            }
        //        }
        //        FormHolidayCalendar.HolidayList = HolidayList;
        //    }
        //    else 
        //    {
        //        FormHolidayCalendar.HolidayList = null;

        //    }

        //    if (ModelState.IsValid)
        //    {
        //        var db_data = db.HolidayCalendar
        //            .Include(e => e.HolidayList)
        //            .Include(e => e.HoliCalendar).Where(e => e.Id == data).SingleOrDefault();
        //        db_data.HoliCalendar = FormHolidayCalendar.HoliCalendar;
        //        db_data.HolidayList = FormHolidayCalendar.HolidayList;
        //        using (TransactionScope ts = new TransactionScope())
        //        {

        //            db.HolidayCalendar.Attach(db_data);
        //            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
        //        }
        //        return Json(new Object[] { db_data.Id, db_data.HoliCalendar.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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
        public async Task<ActionResult> EditSave(HolidayCalendar L, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string HoliCalendarDDL = form["HoliCalendarDDL"] == "0" ? null : form["HoliCalendarDDL"];
                    string HolidayListList = form["HolidayListList"] == null ? null : form["HolidayListList"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    if (HoliCalendarDDL != null && HoliCalendarDDL != "" && HoliCalendarDDL != "-Select-")
                    {


                        L.HoliCalendar_Id = int.Parse(HoliCalendarDDL);

                    }
                    else
                    {
                        L.HoliCalendar_Id = null;
                    }
                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    HolidayCalendar blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        //blog = context.HolidayCalendar.Where(e => e.Id == data).Include(e => e.HoliCalendar)
                                        //                        .Include(e => e.HolidayList).Include(e => e.HoliCalendar.Name)
                                        //                        .SingleOrDefault();
                                        blog = context.HolidayCalendar.Where(e => e.Id == data).Include(e => e.HoliCalendar).Include(e => e.HoliCalendar.Name)
                                                              .Include(e => e.HolidayList)
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

                                    if (HoliCalendarDDL != null && HoliCalendarDDL != "")
                                    {

                                        var val = db.Calendar.Find(int.Parse(HoliCalendarDDL));
                                        L.HoliCalendar = val;
                                        var type = db.HolidayCalendar.Include(e => e.HoliCalendar).Include(e => e.HoliCalendar.Name).Where(e => e.Id == data).SingleOrDefault();
                                        //  var type = db.HolidayCalendar.Include(e => e.HoliCalendar).Where(e => e.Id == data).SingleOrDefault();
                                        IList<HolidayCalendar> typedetails = null;
                                        if (type.HoliCalendar != null)
                                        {
                                            typedetails = db.HolidayCalendar.Include(e => e.HoliCalendar).Include(e => e.HolidayList).Include(e => e.HoliCalendar.Name).Where(x => x.HoliCalendar.Id == type.HoliCalendar.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.HolidayCalendar.Include(e => e.HoliCalendar).Include(e => e.HolidayList).Include(e => e.HoliCalendar.Name).Where(x => x.Id == data).ToList();
                                        }
                                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                        foreach (var s in typedetails)
                                        {
                                            s.HoliCalendar = L.HoliCalendar;
                                            s.Name = L.Name;
                                            db.HolidayCalendar.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    else
                                    {
                                        var WFTypeDetails = db.HolidayCalendar.Include(e => e.HoliCalendar).Include(e => e.HoliCalendar.Name).Where(x => x.Id == data).ToList();
                                        //var WFTypeDetails = db.HolidayCalendar.Include(e => e.HoliCalendar).Where(x => x.Id == data).ToList();
                                        foreach (var s in WFTypeDetails)
                                        {
                                            s.HoliCalendar = null;
                                            db.HolidayCalendar.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }


                                    // *********** Start Duplicate HolidayList Checking Added on : 04-01-2024 By Anandrao **************
                                    List<DateTime?> Datelist = new List<DateTime?>();
                                  //  List<HolidayList> HOList = new List<HolidayList>();
                                   

                                    if (HolidayListList != null && HolidayListList != "")
                                    {
                                        var HOidsList = Utility.StringIdsToListIds(HolidayListList);
                                        foreach (var itemH in HOidsList)
                                        {
                                            var dbHOdetails = db.HolidayList.Find(itemH);
                                            var getHOdate = dbHOdetails.HolidayDate;
                                            Datelist.Add(getHOdate);
                                           
                                        }
                                        var aa = Datelist.Count();
                                        var aaa = Convert.ToInt32(aa);
                                        for (int i = 0; i < Datelist.Count(); i++)
                                        {
                                            for (int j = i + 1; j < Datelist.Count(); j++)
                                            {
                                                if (Datelist[i] == Datelist[j])
                                                {
                                                    Msg.Add("Duplicate Holiday Date Found : " + Datelist[j] + "  Please Keep Single Holiday Date.");
                                                    

                                                }
                                            }

                                        }
                                        if (Msg.Count() > 0)
                                        {
                                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);    
                                        }
                                        

                                    }

                                    // *********** End Duplicate HolidayList Checking Added on : 04-01-2024 By Anandrao **************






                                    List<HolidayList> ObjHolidayList = new List<HolidayList>();
                                    HolidayCalendar HolidayCalendardetails = null;
                                    HolidayCalendardetails = db.HolidayCalendar.Include(e => e.HolidayList).Where(e => e.Id == data).SingleOrDefault();
                                    if (HolidayListList != null && HolidayListList != "")
                                    {
                                        var ids = Utility.StringIdsToListIds(HolidayListList);
                                        foreach (var ca in ids)
                                        {
                                            var HolidayListListvalue = db.HolidayList.Find(ca);
                                            ObjHolidayList.Add(HolidayListListvalue);
                                            HolidayCalendardetails.HolidayList = ObjHolidayList;
                                        }
                                    }
                                    else
                                    {
                                        HolidayCalendardetails.HolidayList = null;
                                    }

                                    db.HolidayCalendar.Attach(HolidayCalendardetails);
                                    db.Entry(HolidayCalendardetails).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = HolidayCalendardetails.RowVersion;
                                    db.Entry(HolidayCalendardetails).State = System.Data.Entity.EntityState.Detached;

                                    ////var CurCorp = db.HolidayCalendar.Find(data);
                                    ////TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    ////db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        HolidayCalendar HolidayCalendar = new HolidayCalendar()
                                        {

                                            Id = data,
                                            DBTrack = L.DBTrack,
                                            Name = L.Name,
                                            HoliCalendar_Id = L.HoliCalendar_Id
                                        };
                                        db.HolidayCalendar.Attach(HolidayCalendar);
                                        db.Entry(HolidayCalendar).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(HolidayCalendar).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    }
                                    // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                    using (var context = new DataBaseContext())
                                    {

                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    //ts.Complete();
                                    ////var qurey = db.HolidayCalendar.Include(e => e.HoliCalendar).Include(e => e.HolidayList).Where(e => e.Id == data).SingleOrDefault();
                                    //Msg.Add("  Record Updated");
                                    //return Json(new Utility.JsonReturnClass { Id = data, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                                    var query = db.HolidayCalendar.Include(e => e.HoliCalendar).Include(e => e.HolidayList).Where(e => e.Id == data).SingleOrDefault();
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = query.Id, Val = query.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }

                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (HolidayCalendar)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (HolidayCalendar)databaseEntry.ToObject();
                                    L.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            HolidayCalendar blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            HolidayCalendar Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.HolidayCalendar.Where(e => e.Id == data).SingleOrDefault();
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

                            HolidayCalendar corp = new HolidayCalendar()
                            {
                                HoliCalendar = L.HoliCalendar,
                                HolidayList = L.HolidayList,
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
                            db.HolidayCalendar.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = L.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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
            public string HoliCalendar { get; set; }
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
                IEnumerable<P2BGridClass> HolidayCalendar = null;
                var data = db.HolidayCalendar.Include(e => e.HoliCalendar).Include(e => e.HolidayList).ToList();
                List<P2BGridClass> holidaylist = new List<P2BGridClass>();
                foreach (var item in data)
                {
                    holidaylist.Add(new P2BGridClass
                    {
                        Id = item.Id,
                        HoliCalendar = item.HoliCalendar != null ? item.HoliCalendar.FullDetails : null,
                        Name = item.Name
                    });
                }
                HolidayCalendar = holidaylist;
                IEnumerable<P2BGridClass> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = HolidayCalendar;
                    if (gp.searchOper.Equals("eq"))
                    {
                        if (gp.searchField == "Id")
                            jsonData = IE.Select(a => new { a.Id, a.Name, a.HoliCalendar }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "HoliCalendar")
                            jsonData = IE.Select(a => new { a.Id, a.Name, a.HoliCalendar }).Where((e => (e.HoliCalendar.ToString().Contains(gp.searchString)))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.HoliCalendar }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = HolidayCalendar;
                    Func<P2BGridClass, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.HoliCalendar
                                          : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Name, a.HoliCalendar }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Name, a.HoliCalendar }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.HoliCalendar }).ToList();
                    }
                    totalRecords = HolidayCalendar.Count();
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

        public class returndatagridclass //Parentgrid
        {
            public int Id { get; set; }
            public string HoliCalendar { get; set; }
            public string Name { get; set; }
            //it was public Calendar HoliCalendar { get; set; }
            // LoanAdvRequest 
        }

        public class ChildDataClass
        {
            public String FullDetails { get; set; }
            // public ICollection<HolidayList> HolidayList { get; set; }
        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    List<returndatagridclass> model = new List<returndatagridclass>();
                    returndatagridclass view = null;

                    var all = db.HolidayCalendar.Include(e => e.HoliCalendar).Include(e => e.HoliCalendar.Name).ToList();

                    IEnumerable<HolidayCalendar> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => e.HoliCalendar.Id.ToString().Contains(param.sSearch)
                            || (e.HoliCalendar.ToString().Contains(param.sSearch))
                            || (e.Name.ToString().Contains(param.sSearch))
                            ).ToList();
                    }

                    foreach (var z in fall)
                    {
                        view = new returndatagridclass()
                        {
                            Id = z.Id,
                            HoliCalendar = z.HoliCalendar != null ? z.HoliCalendar.FullDetails : null,
                            Name = z.Name
                        };
                        model.Add(view);
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<returndatagridclass, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.HoliCalendar : "");
                    var sortcolumn = Request["sSortDir_0"];
                    var dcompanies = model
                              .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompanies.Count == 0)
                    {
                        List<returndatagridclass> result = new List<returndatagridclass>();
                        result = model;
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = model.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = from c in dcompanies

                                     select new[] { null, Convert.ToString(c.Id), c.HoliCalendar, c.Name };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = model.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }//for data reterivation
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


        public class returnDataClass
        {
            public Int32 Id { get; set; }
            public String HolidayList { get; set; }
            // public ICollection<HolidayList> HolidayList { get; set; }             
        }
        public ActionResult GridEditData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returnlist = new List<returnDataClass>();

                if (data != 0)
                {

                    var retrundata = db.HolidayCalendar.Include(e => e.HolidayList).Include(e => e.HolidayList.Select(a => a.Holiday)).Include(e => e.HolidayList.Select(a => a.Holiday.HolidayName)).Include(e => e.HolidayList.Select(a => a.Holiday.HolidayType)).Where(e => e.Id == data).SingleOrDefault();
                    foreach (var item in retrundata.HolidayList.OrderBy(e => e.HolidayDate))
                    {
                        if (item.Holiday != null)
                        {
                            returnlist.Add(new returnDataClass
                            {
                                Id = item.Id,
                                HolidayList = item.Holiday.FullDetails + ",HolidayDate :" + item.HolidayDate.Value.ToShortDateString()
                            });
                        }
                        else
                        {
                            return Json(new { status = false, responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    return Json(returnlist, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View();
                }
            }
        }

        public JsonResult EditGridDetails(int data, string filter)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var all = db.HolidayCalendar.Include(e => e.HolidayList).Where(e => e.Id == data).SingleOrDefault();

                if (all.HolidayList.Count > 0)
                {
                    List<ChildDataClass> returndata = new List<ChildDataClass>();
                    List<HolidayList> Data = new List<HolidayList>();

                    if (filter != "")
                    {
                        Data = all.HolidayList.Where(e => e.Holiday.ToString().Contains(filter)).ToList();
                    }
                    else
                    {
                        Data = all.HolidayList.ToList();
                    }

                    foreach (var item in Data)
                    {

                        returndata.Add(new ChildDataClass
                        {
                            FullDetails = item.FullDetails
                        });
                    }
                    return Json(returndata, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                }

            }

        }



        public ActionResult GridEditSave(HolidayCalendar ypay, FormCollection from, string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    var db_data = db.HolidayCalendar.Where(e => e.Id == id).SingleOrDefault();

                    var val = db.HolidayCalendar.Include(e => e.HoliCalendar).ToList();
                    foreach (var a in val)
                    {
                        if (a.Id == id)
                        {
                            var SalT = db.HolidayCalendar.Where(e => e.Id == a.Id).Select(e => e.HolidayList.Where(r => r.FullDetails == a.HolidayList.ToString()).FirstOrDefault()).SingleOrDefault();

                        }
                    }

                    db_data.HolidayList = ypay.HolidayList;
                    try
                    {
                        db.HolidayCalendar.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        return Json(new { status = true, data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception e)
                    {

                        return Json(new { status = false, responseText = e.InnerException.ToString() }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { status = false, responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult PopulateDropDownListCalendar(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "HOLIDAYCALENDAR" && e.Default == true).ToList();
                //  var qurey = db.Calendar.Include(e=>e.Name).ToList();
                var selected = "";
                if (!string.IsNullOrEmpty(data2))
                {
                    selected = data2;
                }
                var returndata = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateDropDownListCalendarEditview(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int hid = Convert.ToInt32(data2);
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "HOLIDAYCALENDAR" && e.Id == hid).ToList();
                //  var qurey = db.Calendar.Include(e=>e.Name).ToList();
                var selected = "";
                if (!string.IsNullOrEmpty(data2))
                {
                    selected = data2;
                }
                var returndata = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }


        //[HttpPost]
        //public async Task<ActionResult> Delete(int data)
        //{

        //    var HolidayCalendars = db.HolidayCalendar
        //        .Include(e => e.HoliCalendar)
        //        .Include(e => e.HolidayList)
        //        .Where(e => e.Id == data).SingleOrDefault();
        //    if (HolidayCalendars.HoliCalendar != null && HolidayCalendars.HolidayList != null)
        //    {
        //        return Json(new Object[] { "", "", "Child Record Exits..!" }, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        try
        //        {
        //            using (TransactionScope ts = new TransactionScope())
        //            {


        //                db.Entry(HolidayCalendars).State = System.Data.Entity.EntityState.Deleted;
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

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    HolidayCalendar holidaycalendar = db.HolidayCalendar.Include(e => e.HolidayList)
                                                       .Include(e => e.HoliCalendar)
                                                       .Where(e => e.Id == data).SingleOrDefault();


                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (holidaycalendar.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = holidaycalendar.DBTrack.CreatedBy != null ? holidaycalendar.DBTrack.CreatedBy : null,
                                CreatedOn = holidaycalendar.DBTrack.CreatedOn != null ? holidaycalendar.DBTrack.CreatedOn : null,
                                IsModified = holidaycalendar.DBTrack.IsModified == true ? true : false
                            };
                            holidaycalendar.DBTrack = dbT;
                            db.Entry(holidaycalendar).State = System.Data.Entity.EntityState.Modified;
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
                            //Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        var HolidayList = holidaycalendar.HolidayList;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            if (HolidayList != null)
                            {
                                var holidayList = new HashSet<int>(holidaycalendar.HolidayList.Select(e => e.Id));
                                if (holidayList.Count > 0)
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
                                    CreatedBy = holidaycalendar.DBTrack.CreatedBy != null ? holidaycalendar.DBTrack.CreatedBy : null,
                                    CreatedOn = holidaycalendar.DBTrack.CreatedOn != null ? holidaycalendar.DBTrack.CreatedOn : null,
                                    IsModified = holidaycalendar.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(holidaycalendar).State = System.Data.Entity.EntityState.Deleted;
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
                                //Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetLookup(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.HolidayCalendar.Include(e => e.HoliCalendar).Include(e => e.HolidayList).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.HolidayCalendar.Include(e => e.HoliCalendar).Include(e => e.HolidayList).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.HoliCalendar.FullDetails }).Distinct();

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

        public ActionResult ValidateForm(HolidayCalendar FormHolidayCalendar, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string HoliCalendarDDL = form["HoliCalendarDDL"] == "0" ? null : form["HoliCalendarDDL"];
                if (HoliCalendarDDL != null && HoliCalendarDDL != "-Select-")
                {

                    var value = db.Calendar.Find(int.Parse(HoliCalendarDDL));
                    FormHolidayCalendar.HoliCalendar = value;

                }
                else
                {
                    List<string> Msg = new List<string>();
                    Msg.Add("  Kindly select one value from Calendar.  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }



                if (db.HolidayCalendar.Any(e => e.HoliCalendar.Id == FormHolidayCalendar.HoliCalendar.Id))
                {
                    var Msg = new List<string>();
                    Msg.Add("Already Exist");
                    return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }

                return Json(new { success = true }, JsonRequestBehavior.AllowGet);

            }

        }

        public ActionResult GridDelete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var LvEP = db.HolidayList.Find(data);
                db.HolidayList.Remove(LvEP);
                db.SaveChanges();
                //return Json(new Object[] { "", "", " Record Deleted Successfully " }, JsonRequestBehavior.AllowGet);
                List<string> Msgr = new List<string>();
                Msgr.Add("Record Deleted Successfully  ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}