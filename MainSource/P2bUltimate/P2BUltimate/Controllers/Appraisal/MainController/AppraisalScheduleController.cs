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
using System.Reflection;
using P2BUltimate.Security;
using Appraisal;

namespace P2BUltimate.Views.Appraisal.MainViews
{
    [AuthoriseManger]
    public class AppraisalScheduleController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Appraisal/MainViews/AppraisalSchedule/Index.cshtml");
        }


        public ActionResult partial()
        {
            return View("~/Views/Shared/Appraisal/_AppraisalPublish.cshtml");
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            List<string> Msg = new List<string>();
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<AppraisalSchedule> Corporate = null;
                if (gp.IsAutho == true)
                {
                    Corporate = db.AppraisalSchedule.Include(e => e.AppraisalPeriodCalendar).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Corporate = db.AppraisalSchedule.Include(e => e.AppraisalPeriodCalendar).AsNoTracking().ToList();
                }

                IEnumerable<AppraisalSchedule> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = Corporate;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e =>
                            (e.BatchCode.ToString().Contains(gp.searchString.ToString()))
                            || (e.BatchName.ToString().ToUpper().Contains(gp.searchString.ToUpper())) ||(e.Id.ToString().Contains(gp.searchString.ToString()))
                            ).Select(a => new { a.BatchCode, a.BatchName, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.AppraisalPeriodCalendar.FromDate.Value.ToString("dd/MM/yyyy") + " - " + a.AppraisalPeriodCalendar.ToDate.Value.ToString("dd/MM/yyyy"), a.BatchCode, a.BatchName, "", a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Corporate;
                    Func<AppraisalSchedule, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "BatchCode" ? c.BatchCode :
                                         gp.sidx == "BatchName" ? c.BatchName : null);
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.BatchCode, a.BatchName,a.Id, 0  }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, a.AppraisalPeriodCalendar.FromDate.Value.ToString("dd/MM/yyyy") + " - " + a.AppraisalPeriodCalendar.ToDate.Value.ToString("dd/MM/yyyy"), a.PublishDate.Value.ToString("dd/MM/yyyy"), Convert.ToString(a.SpanPeriod), Convert.ToString(a.Extension), 0 }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.BatchCode, a.BatchName, a.Id, 0 }).ToList();
                        // jsonData = IE.Select(a => new Object[] { a.Id, a.AppraisalPeriodCalendar.FromDate.Value.ToString("dd/MM/yyyy") + " - " + a.AppraisalPeriodCalendar.ToDate.Value.ToString("dd/MM/yyyy"), a.PublishDate.Value.ToString("dd/MM/yyyy"),  Convert.ToString(a.SpanPeriod), Convert.ToString(a.Extension), 0 }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Select(a => new Object[] { a.BatchCode, a.BatchName, a.Id,0 }).ToList();
                        // jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.AppraisalPeriodCalendar.FromDate.Value.ToString("dd/MM/yyyy") + " - " + a.AppraisalPeriodCalendar.ToDate.Value.ToString("dd/MM/yyyy"), a.PublishDate.Value.ToString("dd/MM/yyyy"),  a.SpanPeriod, a.Extension, 0 }).ToList();
                    }
                    totalRecords = Corporate.Count();
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

        public ActionResult PopulateDropDownListCalendar(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).ToList();
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

        [HttpPost]
        public ActionResult Create(AppraisalSchedule p, FormCollection form) //Create submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string AppCalendardrop = form["AppCalendardrop"] == "0" ? null : form["AppCalendardrop"];

                    string AppraisalPublishlist = form["AppraisalPublishlist"] == "0" ? "" : form["AppraisalPublishlist"];

                    string geo_id = form["geo_id"] == "0" ? "" : form["geo_id"];
                    string fun_id = form["fun_id"] == "0" ? "" : form["fun_id"];
                    string pay_id = form["pay_id"] == "0" ? "" : form["pay_id"];


                    if (AppCalendardrop != null)
                    {
                        var val = db.Calendar.Find(int.Parse(AppCalendardrop));
                        p.AppraisalPeriodCalendar = val;
                    }

                    int CompId = 0;
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = int.Parse(Session["CompId"].ToString());
                    }


                    CompanyAppraisal OCompanyPayroll = null;
                    List<AppraisalPublish> OFAT = new List<AppraisalPublish>();
                    OCompanyPayroll = db.CompanyAppraisal.Where(e => e.Company.Id == CompId).SingleOrDefault();

                    //if (p.AppraisalPublish.Select(q => q.PublishDate) <= p.AppraisalPeriodCalendar.FromDate || p.AppraisalPublish.Select(q => q.PublishDate) >= p.AppraisalPeriodCalendar.ToDate)
                    //{
                    //    Msg.Add("Publish date should be between Appraisal Calendar.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}


                    p.AppraisalPublish = null;
                    List<AppraisalPublish> OBJ = new List<AppraisalPublish>();

                    if (AppraisalPublishlist != null && AppraisalPublishlist != "")
                    {
                        var ids = Utility.StringIdsToListIds(AppraisalPublishlist);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.AppraisalPublish.Find(ca);
                            OBJ.Add(OBJ_val);
                            p.AppraisalPublish = OBJ;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0,30,0)))
                        {
                            if (db.AppraisalSchedule.Any(o => o.Id == p.Id))
                            {
                                Msg.Add("Code Already Exists.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            p.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };                     //???????

                            if (geo_id != null && geo_id != "")
                            {
                                var ids = Utility.StringIdsToListIds(geo_id);
                                //var fun = db.FuncStruct.Where(e => e.Company != null && e.Company.Id == Comp_Id).Distinct().FirstOrDefault();
                                //SalHead.FuncStruct = fun;

                                //var pay = db.PayStruct.Where(e => e.Company != null && e.Company.Id == Comp_Id).Distinct().FirstOrDefault();
                                //SalHead.PayStruct = pay;
                                foreach (var G in ids)
                                {
                                    var geo = db.GeoStruct.Find(G);
                                    p.GeoStruct = geo;

                                    p.FuncStruct = null;
                                    p.PayStruct = null;

                                    AppraisalSchedule AppraisalScheduleForGeo = new AppraisalSchedule()
                                    {
                                        FuncStruct = p.FuncStruct,
                                        GeoStruct = p.GeoStruct,
                                        PayStruct = p.PayStruct,
                                        AppraisalPeriodCalendar = p.AppraisalPeriodCalendar,
                                        BatchCode = p.BatchCode,
                                        BatchName = p.BatchName,
                                        AppraisalPublish = p.AppraisalPublish,
                                        //Id = p.Id,
                                        DBTrack = p.DBTrack
                                    };

                                    db.AppraisalSchedule.Add(AppraisalScheduleForGeo);
                                    db.SaveChanges();

                                    var aa = db.AppraisalSchedule.Find(AppraisalScheduleForGeo.Id);
                                    //aa.SalAttendance = null;
                                    if (aa.AppraisalPublish.Count() > 0)
                                    {
                                        OFAT.AddRange(aa.AppraisalPublish);
                                    }
                                    aa.AppraisalPublish = OFAT;
                                    //OEmployeePayroll.DBTrack = dbt;
                                    db.AppraisalSchedule.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                    // OFAT.Add(db.AppraisalSchedule.Find(AppraisalScheduleForGeo.Id));
                                }

                            }

                            if (fun_id != null && fun_id != "")
                            {
                                //var geo = db.GeoStruct.Where(e => e.Company != null && e.Company.Id == Comp_Id).Distinct().FirstOrDefault();
                                //SalHead.GeoStruct = geo;

                                //var pay = db.PayStruct.Where(e => e.Company != null && e.Company.Id == Comp_Id).Distinct().FirstOrDefault();
                                //SalHead.PayStruct = pay;

                                var ids = Utility.StringIdsToListIds(fun_id);
                                foreach (var F in ids)
                                {
                                    var fun = db.FuncStruct.Find(F);
                                    p.FuncStruct = fun;

                                    p.GeoStruct = null;
                                    p.PayStruct = null;

                                    AppraisalSchedule AppraisalScheduleFunct = new AppraisalSchedule()
                                    {
                                        FuncStruct = p.FuncStruct,
                                        GeoStruct = p.GeoStruct,
                                        PayStruct = p.PayStruct,
                                        AppraisalPeriodCalendar = p.AppraisalPeriodCalendar,
                                        BatchCode = p.BatchCode,
                                        BatchName = p.BatchName,
                                        AppraisalPublish = p.AppraisalPublish,
                                        //Id = p.Id,
                                        DBTrack = p.DBTrack
                                    };

                                    db.AppraisalSchedule.Add(AppraisalScheduleFunct);
                                    db.SaveChanges();

                                    var aa = db.AppraisalSchedule.Find(AppraisalScheduleFunct.Id);
                                    //aa.SalAttendance = null;
                                    if (aa.AppraisalPublish.Count() > 0)
                                    {
                                        OFAT.AddRange(aa.AppraisalPublish);
                                    }
                                    aa.AppraisalPublish = OFAT;
                                    //OEmployeePayroll.DBTrack = dbt;
                                    db.AppraisalSchedule.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                    //OFAT.Add(db.AppraisalSchedule.Find(AppraisalScheduleFunct.Id));
                                }
                            }

                            if (pay_id != null && pay_id != "")
                            {
                                var ids = Utility.StringIdsToListIds(pay_id);

                                //var geo = db.GeoStruct.Where(e => e.Company != null && e.Company.Id == Comp_Id).Distinct().FirstOrDefault();
                                //SalHead.GeoStruct = geo;

                                //var fun = db.FuncStruct.Where(e => e.Company != null && e.Company.Id == Comp_Id).Distinct().FirstOrDefault();
                                //SalHead.FuncStruct = fun;

                                foreach (var P in ids)
                                {
                                    var pay = db.PayStruct.Find(P);
                                    p.PayStruct = pay;

                                    p.GeoStruct = null;
                                    p.FuncStruct = null;

                                    AppraisalSchedule AppraisalSchedulePayt = new AppraisalSchedule()
                                    {
                                        FuncStruct = p.FuncStruct,
                                        GeoStruct = p.GeoStruct,
                                        PayStruct = p.PayStruct,
                                        AppraisalPeriodCalendar = p.AppraisalPeriodCalendar,
                                        BatchCode = p.BatchCode,
                                        BatchName = p.BatchName,
                                        AppraisalPublish = p.AppraisalPublish,
                                        // Id = p.Id,
                                        DBTrack = p.DBTrack
                                    };

                                    db.AppraisalSchedule.Add(AppraisalSchedulePayt);
                                    db.SaveChanges();



                                    //OFAT.Add(db.AppraisalSchedule.Find(AppraisalSchedulePayt.Id));

                                    var aa = db.AppraisalSchedule.Find(AppraisalSchedulePayt.Id);
                                    //aa.SalAttendance = null;
                                    if (aa.AppraisalPublish.Count() > 0) 
                                    {
                                        OFAT.AddRange(aa.AppraisalPublish);
                                    }
                                    
                                    aa.AppraisalPublish = OFAT;
                                    //OEmployeePayroll.DBTrack = dbt;
                                    db.AppraisalSchedule.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                }
                            }

                            if (geo_id == null && pay_id == null && fun_id == null || geo_id == "" && pay_id == "" && fun_id == "")
                            {
                                var geoList = db.GeoStruct.Where(e => e.Company != null && e.Company.Id == CompId).ToList();

                                foreach (var geo in geoList)
                                {
                                    p.GeoStruct = geo;

                                    p.PayStruct = null;
                                    p.FuncStruct = null;

                                    AppraisalSchedule AppraisalScheduleFunct = new AppraisalSchedule()
                                    {

                                        FuncStruct = p.FuncStruct,
                                        GeoStruct = p.GeoStruct,
                                        PayStruct = p.PayStruct,
                                        AppraisalPeriodCalendar = p.AppraisalPeriodCalendar,
                                        BatchCode = p.BatchCode,
                                        BatchName = p.BatchName,
                                        AppraisalPublish = p.AppraisalPublish,
                                        // Id = p.Id,
                                        DBTrack = p.DBTrack

                                    };

                                    db.AppraisalSchedule.Add(AppraisalScheduleFunct);
                                    db.SaveChanges();
                                    //OFAT.AddRange(p.AppraisalPublish);
                                    var aa = db.AppraisalSchedule.Find(AppraisalScheduleFunct.Id);

                                    if (aa.AppraisalPublish.Count() > 0)
                                    {
                                        OFAT.AddRange(aa.AppraisalPublish);
                                    }
                                       
                                    aa.AppraisalPublish = OFAT;
                                   
                                    db.AppraisalSchedule.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                    // OFAT.Add(db.AppraisalSchedule.Find(AppraisalScheduleFunct.Id));
                                }
                            }

                            ts.Complete();
                            Msg.Add("Data Saved Successfully.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                    }
                }
                catch (Exception e)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = e.Message,
                        ExceptionStackTrace = e.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public class AppraisalPublish1
        {
            public Array AppraisalPublish_Id { get; set; }
            public Array AppraisalPublish_val { get; set; }

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
                var Q = db.AppraisalSchedule
                    .Include(e => e.AppraisalPeriodCalendar)
                    .Where(e => e.Id == data).Select
                    (p => new
                    {
                        AppraisalPeriodCalendar_Id = p.AppraisalPeriodCalendar != null ? p.AppraisalPeriodCalendar.Id : 0,
                        BatchCode = p.BatchCode,
                        BatchName = p.BatchName,

                        GeoStruct = p.GeoStruct,
                        FuncStruct = p.FuncStruct,
                        PayStruct = p.PayStruct,
                        Id = p.Id,
                        Action = p.DBTrack.Action
                    }).ToList();

                List<AppraisalPublish1> app = new List<AppraisalPublish1>();


                var add_data = db.AppraisalSchedule

                               .Include(e => e.AppraisalPublish).Where(e => e.Id == data).ToList();

                foreach (var ca in add_data)
                {
                    app.Add(new AppraisalPublish1
                    {
                        AppraisalPublish_Id = ca.AppraisalPublish.Select(e => e.Id.ToString()).ToArray(),
                        AppraisalPublish_val = ca.AppraisalPublish.Select(e => e.FullDetails).ToArray(),

                    });

                }

                var Corp = db.AppraisalSchedule.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, app, "", Auth, JsonRequestBehavior.AllowGet });
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
                    AppraisalSchedule corporates = db.AppraisalSchedule.Include(e => e.AppraisalPeriodCalendar).Where(e => e.Id == data).SingleOrDefault();


                    if (corporates.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? true : false
                            };
                            corporates.DBTrack = dbT;
                            db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, corporates.DBTrack);
                            //DT_AppraisalPublish DT_Corp = (DT_AppraisalPublish)rtn_Obj;
                            //DT_Corp.AppraisalPeriodCalendar_Id = corporates.AppraisalPeriodCalendar == null ? 0 : corporates.AppraisalPeriodCalendar.Id;
                            //db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            //Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                            db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, dbT);
                            //DT_AppraisalPublish DT_Corp = (DT_AppraisalPublish)rtn_Obj;
                            //DT_Corp.AppraisalPeriodCalendar_Id = corporates.AppraisalPeriodCalendar == null ? 0 : corporates.AppraisalPeriodCalendar.Id;
                            //db.Create(DT_Corp);

                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        public async Task<ActionResult> EditSave(AppraisalSchedule L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string HoliCalendarDDL = form["AppCalendardrop"] == "0" ? null : form["AppCalendardrop"];
                    var IsTrClose = form["IsTrClose"] == "0" ? "" : form["IsTrClose"];

                    var db_data = db.AppraisalSchedule.Include(e => e.AppraisalPublish).Where(e => e.Id == data).SingleOrDefault();

                    List<AppraisalPublish> ObjITsection = new List<AppraisalPublish>();
                    string Values = form["AppraisalPublishlist"];

                    if (Values != null && Values != "")
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var value = db.AppraisalPublish.Find(ca);
                            ObjITsection.Add(value);
                            L.AppraisalPublish = ObjITsection;

                        }
                    }
                    else
                    {
                        L.AppraisalPublish = null;
                    }






                    if (ModelState.IsValid)
                    {
                        try
                        {

                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                AppraisalSchedule blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.AppraisalSchedule.Where(e => e.Id == data).Include(e => e.AppraisalPeriodCalendar).SingleOrDefault();
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

                                if (HoliCalendarDDL != null)
                                {
                                    if (HoliCalendarDDL != "")
                                    {
                                        var val = db.Calendar.Find(int.Parse(HoliCalendarDDL));
                                        L.AppraisalPeriodCalendar = val;

                                        var type = db.AppraisalSchedule.Include(e => e.AppraisalPeriodCalendar).Where(e => e.Id == data).SingleOrDefault();
                                        IList<AppraisalSchedule> typedetails = null;
                                        if (type.AppraisalPeriodCalendar != null)
                                        {
                                            typedetails = db.AppraisalSchedule.Where(x => x.AppraisalPeriodCalendar.Id == type.AppraisalPeriodCalendar.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.AppraisalSchedule.Where(x => x.Id == data).ToList();
                                        }
                                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                        foreach (var s in typedetails)
                                        {
                                            s.AppraisalPeriodCalendar = L.AppraisalPeriodCalendar;
                                            db.AppraisalSchedule.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    else
                                    {
                                        var WFTypeDetails = db.AppraisalSchedule.Include(e => e.AppraisalPeriodCalendar).Where(x => x.Id == data).ToList();
                                        foreach (var s in WFTypeDetails)
                                        {
                                            s.AppraisalPeriodCalendar = null;
                                            db.AppraisalSchedule.Attach(s);
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
                                    var HoliCalendarDetails = db.AppraisalSchedule.Include(e => e.AppraisalPeriodCalendar).Where(x => x.Id == data).ToList();
                                    foreach (var s in HoliCalendarDetails)
                                    {
                                        //s.AppraisalPeriodCalendar = null;
                                        s.AppraisalPeriodCalendar = L.AppraisalPeriodCalendar;
                                        db.AppraisalSchedule.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }

                                List<AppraisalPublish> ObjITSection = new List<AppraisalPublish>();

                                AppraisalSchedule AppraisalSchedule = null;
                                AppraisalSchedule = db.AppraisalSchedule.Include(e => e.AppraisalPublish).Where(e => e.Id == data).SingleOrDefault();
                                if (Values != null && Values != "")
                                {
                                    var ids = Utility.StringIdsToListIds(Values);
                                    foreach (var ca in ids)
                                    {
                                        var ITSeclist = db.AppraisalPublish.Find(ca);
                                        ObjITSection.Add(ITSeclist);
                                        AppraisalSchedule.AppraisalPublish = ObjITSection;
                                    }
                                }
                                else
                                {
                                    AppraisalSchedule.AppraisalPublish = null;
                                }


                                //L.IsTrClose = Convert.ToBoolean(IsTrClose);
                                var CurCorp = db.AppraisalSchedule.Find(data);
                                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {

                                    AppraisalSchedule Appraisalschedule = new AppraisalSchedule()
                                    {
                                        AppraisalPeriodCalendar = L.AppraisalPeriodCalendar,
                                        AppraisalPublish = L.AppraisalPublish,
                                        BatchCode = L.BatchCode,
                                        BatchName = L.BatchName,
                                        Id = data,
                                        DBTrack = L.DBTrack
                                    };
                                    db.AppraisalSchedule.Attach(Appraisalschedule);
                                    db.Entry(Appraisalschedule).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(Appraisalschedule).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                }
                                // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                using (var context = new DataBaseContext())
                                {
                                    // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                    //  dt_holiday DT_Corp = (DT_LvCreditPolicy)obj;
                                    //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;
                                    //db.Create(DT_Corp);
                                    db.SaveChanges();
                                }
                                await db.SaveChangesAsync();
                                var qurey = db.AppraisalSchedule.Include(e => e.AppraisalPeriodCalendar).Where(e => e.Id == data).SingleOrDefault();

                                ts.Complete();
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass
                                {
                                    Id = qurey.Id
                                        //, Val = qurey.SpanPeriod.ToString()
                                    ,
                                    success = true,
                                    responseText = Msg
                                }, JsonRequestBehavior.AllowGet);

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


        public ActionResult GetAppraisalPublishDetails(List<int> SkipIds)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = new List<AppraisalPublish>();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.AppraisalSchedule.Include(e => e.AppraisalPublish).Where(e => !e.Id.ToString().Contains(a.ToString())).SelectMany(h => h.AppraisalPublish).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var list1 = db.AppraisalPublish.ToList();
                var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }

        }
    }
}