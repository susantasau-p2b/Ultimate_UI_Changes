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
using Recruitment;


namespace P2BUltimate.Controllers.recruitment.MainController
{
    public class RecruitInitiatorController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Recruitement/MainView/RecruitInitiator/Index.cshtml");
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/Recruitement/_RecruitBatchInitiator.cshtml");
        }

        [HttpPost]
        public ActionResult Create(RecruitInitiator p, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<String> Msg = new List<String>();
                try
                {
                    //string WFStatusdrop = form["WFStatusdrop"] == "0" ? null : form["WFStatusdrop"];
                    //var TrClosed = form["TrClosed"] == "0" ? "" : form["TrClosed"];
                    //var TrReject = form["TrReject"] == "0" ? "" : form["TrReject"];

                    //if (WFStatusdrop != null && WFStatusdrop != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(WFStatusdrop));
                    //    p.WFStatus = val;
                    //}

                    //p.TrClosed = Convert.ToBoolean(TrClosed);
                    //p.TrReject = Convert.ToBoolean(TrReject);

                    RecruitYearlyCalendar yearlyrecruitmentCalendar = null;
                    Calendar val = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "RECRUITMENTCALENDAR" && e.Default == true).SingleOrDefault();
                    yearlyrecruitmentCalendar = db.RecruitYearlyCalendar
                        .Include(e => e.RecruitmentCalendar)
                        .Include(e => e.RecruitInitiator).Where(e => e.RecruitmentCalendar.Id == val.Id).SingleOrDefault();
                    p.RecruitExpenses = null;
                    List<RecruitExpenses> OBJ = new List<RecruitExpenses>();
                    string Values = form["RecruitExpensesListM"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.RecruitExpenses.Find(ca);
                            OBJ.Add(OBJ_val);
                            p.RecruitExpenses = OBJ;
                        }
                    }
                    List<RecruitBatchInitiator> OBJ1 = new List<RecruitBatchInitiator>();
                    string Values1 = form["RecruitBatchInitiatorListM"];
                    if (Values1 != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values1);
                        foreach (var ab in ids)
                        {
                            var OBJ1_val = db.RecruitBatchInitiator.Find(ab);
                            OBJ1.Add(OBJ1_val);
                            p.RecruitBatchInitiator = OBJ1;
                        }

                    }


                    if (p.PublishDate < p.initiatedDate)
                    {
                        Msg.Add("Publish Date should be greater than initiate Date");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    List<RecruitInitiator> OFAT = new List<RecruitInitiator>();
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            //if (db.AppraisalPublish.Any(o => o.Id == p.Id))
                            //{
                            //    Msg.Add("Code Already Exists.");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}

                            p.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };                     //???????

                            RecruitInitiator RecruitInitiator = new RecruitInitiator()
                            {
                                AdvertiseReferenceNo = p.AdvertiseReferenceNo,
                                PublishDate = p.PublishDate,
                                initiatedDate = p.initiatedDate,
                                //CloseAdvDate = p.CloseAdvDate,
                                //WFStatus = p.WFStatus,
                                //TrClosed = p.TrClosed,
                                //TrReject = p.TrReject,
                                Narration = p.Narration,
                                RecruitExpenses = p.RecruitExpenses,
                                RecruitBatchInitiator = p.RecruitBatchInitiator,
                                Id = p.Id,
                                DBTrack = p.DBTrack
                            };

                            db.RecruitInitiator.Add(RecruitInitiator);
                            db.SaveChanges();
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, p.DBTrack);
                            OFAT.Add(db.RecruitInitiator.Find(RecruitInitiator.Id));
                            if (yearlyrecruitmentCalendar.RecruitInitiator != null)
                            {
                                OFAT.AddRange(yearlyrecruitmentCalendar.RecruitInitiator);
                            }
                            yearlyrecruitmentCalendar.RecruitInitiator = OFAT;
                            //OEmployeePayroll.DBTrack = dbt;
                            db.RecruitYearlyCalendar.Attach(yearlyrecruitmentCalendar);
                            db.Entry(yearlyrecruitmentCalendar).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(yearlyrecruitmentCalendar).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
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
        public class P2BGridData
        {
            public int Id { get; set; }
            public string initiatedDate { get; set; }
            public string AdvertiseReferenceNo { get; set; }
            public string Narration { get; set; }

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
                IEnumerable<P2BGridData> Corporate = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;
                string PayMonth = "";

                //if (gp.IsAutho == true)
                //{
                //    Corporate = db.RecruitInitiator.Include(e => e.WFStatus).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                //}
                //else
                //{
                //    Corporate = db.RecruitInitiator.Include(e => e.WFStatus).AsNoTracking().ToList();
                //}
                if (gp.filter != null)
                    PayMonth = gp.filter;

                if (PayMonth != null && PayMonth != "")
                {
                    var recruityearlyid = db.Calendar.Find(int.Parse(PayMonth));
                    List<RecruitYearlyCalendar> recruitdata = db.RecruitYearlyCalendar
                                                         .Include(e => e.RecruitmentCalendar)
                                                     .Include(e => e.RecruitInitiator).Where(e => e.RecruitmentCalendar.Id == recruityearlyid.Id).ToList();

                    foreach (var item in recruitdata)
                    {
                        foreach (var item1 in item.RecruitInitiator)
                        {

                            view = new P2BGridData()
                            {
                                Id = item1.Id,
                                initiatedDate = item1.initiatedDate.Value.ToShortDateString(),
                                AdvertiseReferenceNo = item1.AdvertiseReferenceNo,
                                Narration = item1.Narration == null ? "" : item1.Narration
                            };

                            model.Add(view);
                        }

                    }
                }
                else
                {
                    var Recruitinitiatordata = db.RecruitInitiator.ToList();
                    foreach (var item1 in Recruitinitiatordata)
                    {

                        view = new P2BGridData()
                        {
                            Id = item1.Id,
                            initiatedDate = item1.initiatedDate.Value.ToShortDateString(),
                            AdvertiseReferenceNo = item1.AdvertiseReferenceNo,
                            Narration = item1.Narration == null ? "" : item1.Narration
                        };

                        model.Add(view);
                    }
                }
                Corporate = model;
                IEnumerable<P2BGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = Corporate;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.initiatedDate.ToString().Contains(gp.searchString))
                                || (e.AdvertiseReferenceNo.ToString().Contains(gp.searchString))
                                || (e.Narration.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.initiatedDate, a.AdvertiseReferenceNo, a.Narration, a.Id }).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.initiatedDate, a.AdvertiseReferenceNo, a.Narration, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Corporate;
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "PublishDate" ? c.initiatedDate :
                                         gp.sidx == "AdvertiseReferenceNo" ? c.AdvertiseReferenceNo :
                                         gp.sidx == "Narration" ? c.Narration : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.initiatedDate, a.AdvertiseReferenceNo, Convert.ToString(a.Narration), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.initiatedDate, a.AdvertiseReferenceNo, Convert.ToString(a.Narration), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.initiatedDate, a.AdvertiseReferenceNo, a.Narration, a.Id }).ToList();
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

        public class RecruitInitiatorD
        {
            public Array RecruitBatchInitiator_Id { get; set; }
            public Array RecruitBatchInitiator_val { get; set; }
            public Array RecruitExpenses_Id { get; set; }
            public Array RecruitExpenses_val { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            //string tableName = "Corporate";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);
            List<RecruitInitiatorD> pst = new List<RecruitInitiatorD>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.RecruitInitiator
                    .Include(e => e.WFStatus)
                    .Include(e => e.RecruitBatchInitiator)
                    .Include(e => e.RecruitExpenses)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        AdvertiseReferenceNo = e.AdvertiseReferenceNo,
                        initiatedDate = e.initiatedDate,
                        PublishDate = e.PublishDate,
                        CloseAdvDate = e.CloseAdvDate,
                        TrClosed = e.TrClosed,
                        TrReject = e.TrReject,
                        Narration = e.Narration != null ? e.Narration : "",
                        WFStatus_Id = e.WFStatus.Id == null ? 0 : e.WFStatus.Id,
                        Action = e.DBTrack.Action
                    }).ToList();



                var a = db.RecruitInitiator.Include(e => e.WFStatus)
                    .Include(e => e.RecruitBatchInitiator)
                    .Include(e => e.RecruitExpenses.Select(e1 => e1.ExpenseAccount))
                    .Include(e => e.RecruitExpenses.Select(e1 => e1.SourceOfExpense))
                    .Where(e => e.Id == data).ToList();
                foreach (var ca in a)
                {
                    pst.Add(new RecruitInitiatorD
                    {

                        RecruitBatchInitiator_Id = ca.RecruitBatchInitiator.Select(e => e.Id.ToString()).ToArray(),
                        RecruitBatchInitiator_val = ca.RecruitBatchInitiator.Select(e => e.FullDetails).ToArray(),
                        RecruitExpenses_Id = ca.RecruitExpenses.Select(e => e.Id.ToString()).ToArray(),
                        RecruitExpenses_val = ca.RecruitExpenses.Select(e => e.FullDetails).ToArray(),

                    });
                }
                var Corp = db.RecruitInitiator.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, pst, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(RecruitInitiator ESOBJ, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    var db_data = db.RecruitInitiator.Include(e => e.WFStatus)
                                    .Include(e => e.RecruitBatchInitiator)
                                    .Include(e => e.RecruitExpenses.Select(e1 => e1.ExpenseAccount))
                                    .Include(e => e.RecruitExpenses.Select(e1 => e1.SourceOfExpense))
                                    .Where(e => e.Id == data).SingleOrDefault();


                    var TrClosed = form["TrClosed"] == "0" ? "" : form["TrClosed"];
                    var TrReject = form["TrReject"] == "0" ? "" : form["TrReject"];

                    string Values = form["RecruitExpensesListM"];
                    List<RecruitExpenses> SOBJ = new List<RecruitExpenses>();
                    db_data.RecruitExpenses = null;
                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.RecruitExpenses.Find(ca);
                            SOBJ.Add(Lookup_val);
                            db_data.RecruitExpenses = SOBJ;
                        }
                    }
                    else
                    {
                        db_data.RecruitExpenses = null;
                    }
                    string Values1 = form["RecruitBatchInitiatorListM"];
                    List<RecruitBatchInitiator> SOBJ1 = new List<RecruitBatchInitiator>();
                    db_data.RecruitBatchInitiator = null;
                    if (Values1 != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values1);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.RecruitBatchInitiator.Find(ca);
                            SOBJ1.Add(Lookup_val);
                            db_data.RecruitBatchInitiator = SOBJ1;
                        }
                    }
                    else
                    {
                        db_data.RecruitBatchInitiator = null;
                    }
                    // string Category = form["Categorylist"] == "0" ? "" : form["Categorylist"];
                    string CatVal = form["WFStatusdrop"] == "0" ? null : form["WFStatusdrop"];
                    if (CatVal != null)
                    {
                        if (CatVal != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(CatVal));
                            ESOBJ.WFStatus = val;
                        }
                    }


                    if (ESOBJ.PublishDate < ESOBJ.initiatedDate)
                    {
                        Msg.Add("Publish Date should be greater than initiate Date");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }


                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();
                                db.RecruitInitiator.Attach(db_data);
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = db_data.RowVersion;
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                var Curr_OBJ = db.RecruitInitiator.Find(data);
                                TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

                                //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                //{
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    RecruitInitiator blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.RecruitInitiator.Where(e => e.Id == data)
                                                                .Include(e => e.WFStatus)
                                                                .Include(e => e.RecruitBatchInitiator)
                                                                .Include(e => e.RecruitExpenses.Select(e1 => e1.ExpenseAccount))
                                                                .Include(e => e.RecruitExpenses.Select(e1 => e1.SourceOfExpense))
                                                                 .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    ESOBJ.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    //int a = EditS(CatVal, data, ESOBJ, ESOBJ.DBTrack);


                                    if (CatVal != null)
                                    {
                                        if (CatVal != "")
                                        {
                                            var val = db.LookupValue.Find(int.Parse(CatVal));
                                            ESOBJ.WFStatus = val;

                                            var type = db.RecruitInitiator
                                                .Include(e => e.WFStatus)
                                                .Where(e => e.Id == data).SingleOrDefault();
                                            IList<RecruitInitiator> typedetails = null;
                                            if (type.WFStatus != null)
                                            {
                                                typedetails = db.RecruitInitiator.Where(x => x.WFStatus.Id == type.WFStatus.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.RecruitInitiator.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.WFStatus = ESOBJ.WFStatus;
                                                db.RecruitInitiator.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var Dtls = db.RecruitInitiator.Include(e => e.WFStatus).Where(x => x.Id == data).ToList();
                                            foreach (var s in Dtls)
                                            {
                                                s.WFStatus = null;
                                                db.RecruitInitiator.Attach(s);
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
                                        var Dtls = db.RecruitInitiator.Include(e => e.WFStatus).Where(x => x.Id == data).ToList();
                                        foreach (var s in Dtls)
                                        {
                                            s.WFStatus = null;
                                            db.RecruitInitiator.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    var CurOBJ = db.RecruitInitiator.Find(data);
                                    TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                                    db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        RecruitInitiator ESIOBJ = new RecruitInitiator()
                                        {
                                            Id = data,
                                            AdvertiseReferenceNo = ESOBJ.AdvertiseReferenceNo,
                                            CloseAdvDate = ESOBJ.CloseAdvDate,
                                            initiatedDate = ESOBJ.initiatedDate,
                                            Narration = ESOBJ.Narration,
                                            TrClosed = ESOBJ.TrClosed,
                                            TrReject = ESOBJ.TrReject,
                                            PublishDate = ESOBJ.PublishDate,
                                            DBTrack = ESOBJ.DBTrack
                                        };

                                        db.RecruitInitiator.Attach(ESIOBJ);
                                        db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                    }


                                    RecruitInitiator OBJ = new RecruitInitiator
                                    {
                                        Id = data,
                                        RecruitExpenses = db_data.RecruitExpenses,
                                        RecruitBatchInitiator = db_data.RecruitBatchInitiator,
                                        DBTrack = ESOBJ.DBTrack
                                    };

                                    //db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
                                    // db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    //var obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                    //DT_RecruitInitiator DT_OBJ = (DT_RecruitInitiator)obj;
                                    //DT_OBJ.WFStatus_Id = blog.WFStatus == null ? 0 : blog.WFStatus.Id;
                                    //db.Create(DT_OBJ);
                                    db.SaveChanges();
                                    await db.SaveChangesAsync();
                                    db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return Json(new Object[] { ESOBJ.Id, ESOBJ.Religion.LookupVal, "Record Updated", JsonRequestBehavior.AllowGet });
                                }
                                // }
                            }
                            catch (DbUpdateException e) { throw e; }
                            catch (DataException e) { throw e; }
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
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            RecruitInitiator blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            RecruitInitiator Old_OBJ = null;
                            using (var context = new DataBaseContext())
                            {
                                blog = context.RecruitInitiator.Where(e => e.Id == data).SingleOrDefault();
                                TempData["RowVersion"] = blog.RowVersion;
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            ESOBJ.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            RecruitInitiator RecruitInitiator = new RecruitInitiator()
                            {
                                Id = data,
                                RecruitExpenses = db_data.RecruitExpenses,
                                RecruitBatchInitiator = db_data.RecruitBatchInitiator,
                                DBTrack = ESOBJ.DBTrack
                            };
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                            db.RecruitInitiator.Attach(RecruitInitiator);
                            db.Entry(RecruitInitiator).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(RecruitInitiator).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            TempData["RowVersion"] = db_data.RowVersion;
                            db.Entry(RecruitInitiator).State = System.Data.Entity.EntityState.Detached;

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Recruitement/Recruitement", "M", blog, ESOBJ, "RecruitInitiator", ESOBJ.DBTrack);
                                DT_RecruitInitiator DT_OBJ = (DT_RecruitInitiator)obj;
                                Old_OBJ = context.RecruitInitiator.Where(e => e.Id == data).Include(e => e.WFStatus)
                                                                .Include(e => e.RecruitBatchInitiator)
                                                                .Include(e => e.RecruitExpenses.Select(e1 => e1.ExpenseAccount))
                                                                .Include(e => e.RecruitExpenses.Select(e1 => e1.SourceOfExpense)).SingleOrDefault();

                                DT_OBJ.WFStatus_Id = DBTrackFile.ValCompare(Old_OBJ.WFStatus, ESOBJ.WFStatus);//Old_OBJ.Address == c.Address ? 0 : Old_OBJ.Address == null && c.Address != null ? c.Address.Id : Old_OBJ.Address.Id;
                                db.Create(DT_OBJ);
                                //db.SaveChanges();
                            }


                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = ESOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { blog.Id, ESOBJ.Religion.LookupVal, "Record Updated", JsonRequestBehavior.AllowGet });
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

        [HttpPost]
        public async Task<ActionResult> EditSave1(RecruitInitiator L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string WFStatusdrop = form["WFStatusdrop"] == "0" ? null : form["WFStatusdrop"];
                    var TrClosed = form["TrClosed"] == "0" ? "" : form["TrClosed"];
                    var TrReject = form["TrReject"] == "0" ? "" : form["TrReject"];
                    L.RecruitExpenses = null;
                    List<RecruitExpenses> OBJ = new List<RecruitExpenses>();
                    string Values = form["RecruitExpensesListM"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.RecruitExpenses.Find(ca);
                            OBJ.Add(OBJ_val);
                            L.RecruitExpenses = OBJ;
                        }
                    }
                    else
                    {
                        L.RecruitExpenses = null;
                    }
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                RecruitInitiator blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.RecruitInitiator.Where(e => e.Id == data).Include(e => e.WFStatus).SingleOrDefault();
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

                                if (WFStatusdrop != null && WFStatusdrop != "")
                                {
                                    var val = db.LookupValue.Find(int.Parse(WFStatusdrop));
                                    L.WFStatus = val;
                                }

                                L.TrClosed = Convert.ToBoolean(TrClosed);
                                L.TrReject = Convert.ToBoolean(TrReject);
                                var CurCorp = db.RecruitInitiator.Find(data);
                                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {

                                    RecruitInitiator RecruitInitiator = new RecruitInitiator()
                                    {
                                        AdvertiseReferenceNo = L.AdvertiseReferenceNo,
                                        PublishDate = L.PublishDate,
                                        initiatedDate = L.initiatedDate,
                                        CloseAdvDate = L.CloseAdvDate,
                                        WFStatus = L.WFStatus,
                                        TrClosed = L.TrClosed,
                                        TrReject = L.TrReject,
                                        Narration = L.Narration,
                                        Id = data,
                                        DBTrack = L.DBTrack
                                    };
                                    db.RecruitInitiator.Attach(RecruitInitiator);
                                    db.Entry(RecruitInitiator).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(RecruitInitiator).OriginalValues["RowVersion"] = TempData["RowVersion"];
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
                                //  var qurey = db.RecruitInitiator.Include(e => e.WFStatus).Where(e => e.Id == data).SingleOrDefault();

                                ts.Complete();
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = L.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

        public int EditS(string CatVal, int data, RecruitInitiator ESOBJ, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (CatVal != null)
                {
                    if (CatVal != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(CatVal));
                        ESOBJ.WFStatus = val;

                        var type = db.RecruitInitiator
                            .Include(e => e.WFStatus)
                            .Where(e => e.Id == data).SingleOrDefault();
                        IList<RecruitInitiator> typedetails = null;
                        if (type.WFStatus != null)
                        {
                            typedetails = db.RecruitInitiator.Where(x => x.WFStatus.Id == type.WFStatus.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.RecruitInitiator.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.WFStatus = ESOBJ.WFStatus;
                            db.RecruitInitiator.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.RecruitInitiator.Include(e => e.WFStatus).Where(x => x.Id == data).ToList();
                        foreach (var s in Dtls)
                        {
                            s.WFStatus = null;
                            db.RecruitInitiator.Attach(s);
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
                    var Dtls = db.RecruitInitiator.Include(e => e.WFStatus).Where(x => x.Id == data).ToList();
                    foreach (var s in Dtls)
                    {
                        s.WFStatus = null;
                        db.RecruitInitiator.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }





                var CurOBJ = db.RecruitInitiator.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    ESOBJ.DBTrack = dbT;
                    RecruitInitiator ESIOBJ = new RecruitInitiator()
                    {
                        Id = data,
                        AdvertiseReferenceNo = ESOBJ.AdvertiseReferenceNo,
                        CloseAdvDate = ESOBJ.CloseAdvDate,
                        initiatedDate = ESOBJ.initiatedDate,
                        Narration = ESOBJ.Narration,
                        TrClosed = ESOBJ.TrClosed,
                        TrReject = ESOBJ.TrReject,
                        PublishDate = ESOBJ.PublishDate,
                        DBTrack = ESOBJ.DBTrack
                    };

                    db.RecruitInitiator.Attach(ESIOBJ);
                    db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //var EmpSocialInfo = db.Employee.Include(e => e.EmpSocialInfo).Include(e => e.EmpSocialInfo.SocialActivities)
                    //              .Include(e => e.EmpSocialInfo.Category)
                    //              .Include(e => e.EmpSocialInfo.Religion)
                    //              .Include(e => e.EmpSocialInfo.Caste)
                    //              .Include(e => e.EmpSocialInfo.SubCaste)
                    //              .Where(e => e.Id == data).SingleOrDefault();

                    var EmpSocialInfo = db.RecruitInitiator.Include(e => e.WFStatus)
                             .Include(e => e.RecruitBatchInitiator)
                             .Include(e => e.RecruitExpenses.Select(e1 => e1.ExpenseAccount))
                             .Include(e => e.RecruitExpenses.Select(e1 => e1.SourceOfExpense))
                             .Where(e => e.Id == data).SingleOrDefault();

                    // var EmpSocialInfo = db.EmpSocialInfo.Include(e=>e.Caste).Include(e=>e.Category).Include(e=>e.SubCaste).Include(e=>e.Religion).Include(e => e.SocialActivities).Where(e => e.Id == data).SingleOrDefault();

                    if (EmpSocialInfo.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = EmpSocialInfo.DBTrack.CreatedBy != null ? EmpSocialInfo.DBTrack.CreatedBy : null,
                                CreatedOn = EmpSocialInfo.DBTrack.CreatedOn != null ? EmpSocialInfo.DBTrack.CreatedOn : null,
                                IsModified = EmpSocialInfo.DBTrack.IsModified == true ? true : false
                            };
                            EmpSocialInfo.DBTrack = dbT;
                            db.Entry(EmpSocialInfo).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, EmpSocialInfo.DBTrack);
                            DT_RecruitInitiator DT_OBJ = (DT_RecruitInitiator)rtn_Obj;
                            db.Create(DT_OBJ);

                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //Msg.Add("  Data removed successfully.  ");
                            //  return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            //  var v = EmpSocialInfo.Where(a => a.EmpSocialInfo.Id == EmpSocialInfo.Id).ToList();
                            //  db.Employee.RemoveRange(v);
                            // db.SaveChanges();


                            var selectedValues = EmpSocialInfo.RecruitBatchInitiator;
                            var lkValue = new HashSet<int>(EmpSocialInfo.RecruitBatchInitiator.Select(e => e.Id));
                            if (lkValue.Count > 0)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                            }

                            var selectedValues1 = EmpSocialInfo.RecruitExpenses;
                            var lkValue1 = new HashSet<int>(EmpSocialInfo.RecruitExpenses.Select(e => e.Id));
                            if (lkValue1.Count > 0)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                            }
                            db.Entry(EmpSocialInfo).State = System.Data.Entity.EntityState.Deleted;


                            //if (v!=null)
                            //{
                            //Msg.Add("child record Exist in Employee Master");
                            //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //}
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // Msg.Add("  Data removed successfully.  ");
                            //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

        [HttpPost]
        public ActionResult GetRecruitExpensesLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.RecruitExpenses.Include(a => a.ExpenseAccount).Include(a => a.SourceOfExpense).ToList();
                IEnumerable<RecruitExpenses> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.RecruitExpenses.ToList().Where(d => d.Narration.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public class Recruitbatchdata
        {
            public int Id { get; set; }
            public string Fulldetails { get; set; }
        }
        [HttpPost]
        public ActionResult GetRecruitBatchInitiatorLKDetails(List<int> SkipIds)
        {
            List<Recruitbatchdata> data = new List<Recruitbatchdata>();
            Recruitbatchdata model = null;
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.RecruitBatchInitiator
                            .Include(e => e.PostDetails)
                            .Include(e => e.PostDetails.FuncStruct)
                            .Include(e => e.PostDetails.FuncStruct.Job)
                             .ToList();

                foreach (var item in fall)
                {

                    string fulldetails = item.FullDetails + ",Post:" + item.PostDetails.FuncStruct.Job.Name;
                    model = new Recruitbatchdata
                    {
                        Id = item.Id,
                        Fulldetails = fulldetails
                    };
                    data.Add(model);
                }
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.RecruitBatchInitiator
                            .Include(e => e.PostDetails)
                            .Include(e => e.PostDetails.FuncStruct)
                            .Include(e => e.PostDetails.FuncStruct.Job).Where(e => e.Id != a)
                             .ToList();
                        else
                            data = data.Where(e => e.Id != a).ToList();
                    }
                }
                var r = (from ca in data select new { srno = ca.Id, lookupvalue = ca.Fulldetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetRecruitBatchInitiatorLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.RecruitBatchInitiator.ToList();
                IEnumerable<RecruitBatchInitiator> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.RecruitBatchInitiator.ToList().Where(d => d.FullDetails.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

    }
}