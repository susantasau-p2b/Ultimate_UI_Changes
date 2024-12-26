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
using Payroll;

namespace P2BUltimate.Controllers.Appraisal.MainController
{
    public class AppAssignmentController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public string geoa;
        public string paya;
        public string funa;

        public ActionResult Index()
        {
            return View("~/Views/Appraisal/MainViews/AppAssignment/Index.cshtml");
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    AppAssignment AppAssign = db.AppAssignment
                 .Include(e => e.AppCategory)
                 .Include(e => e.AppSubCategory)
                 .Include(e => e.AppraisalCalendar)
                 .Include(e => e.AppRatingObjective)
                 .Where(e => e.Id == data).SingleOrDefault();

                    var AppAssignlist = db.AppAssignment.Where(e => e.AppCategory.Id == AppAssign.AppCategory.Id && e.AppSubCategory.Id == AppAssign.AppSubCategory.Id)
                        .ToList();

                    // FuncStruct fun = corporates.FuncStruct;
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        foreach (var item in AppAssignlist)
                        {
                            if (AppAssign.DBTrack.IsModified == true)
                            {

                                DBTrack dbT = new DBTrack
                               {
                                   Action = "D",
                                   CreatedBy = item.DBTrack.CreatedBy != null ? item.DBTrack.CreatedBy : null,
                                   CreatedOn = item.DBTrack.CreatedOn != null ? item.DBTrack.CreatedOn : null,
                                   IsModified = item.DBTrack.IsModified == true ? true : false
                               };
                                item.DBTrack = dbT;
                                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, item.DBTrack);
                                DT_AppAssignment DT_Corp = (DT_AppAssignment)rtn_Obj;
                                // DT_Corp.  = corporates.  == null ? 0 : corporates.AppMode.Id;
                                db.Create(DT_Corp);

                                //  await db.SaveChangesAsync();

                            }
                            else
                            {
                                // var selectedJobP = Postdetails.FuncStruct;

                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = item.DBTrack.CreatedBy != null ? item.DBTrack.CreatedBy : null,
                                    CreatedOn = item.DBTrack.CreatedOn != null ? item.DBTrack.CreatedOn : null,
                                    IsModified = item.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, dbT);
                                DT_AppAssignment DT_Corp = (DT_AppAssignment)rtn_Obj;
                                //  DT_Corp.AppMode_Id = val == null ? 0 : val.Id;
                                db.Create(DT_Corp);

                                //await db.SaveChangesAsync();


                            }
                        }
                        ts.Complete();
                    }

                    Msg.Add("  Data removed.  ");                                                                                             // the original place 
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                return null;
            }
        }


        public ActionResult GetAppRatingObjectiveLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.AppRatingObjective.Include(e => e.ObjectiveWordings).ToList();
                IEnumerable<AppRatingObjective> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.AppRatingObjective.ToList().Where(d => d.Id.ToString().Contains(data));
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


        [HttpPost]
        public ActionResult GetLookupAppCategory(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var result = (object)null;
                var fall = db.AppCategory.Include(e => e.AppSubCategory).ToList();
                IEnumerable<AppCategory> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.AppCategory.ToList().Where(d => d.FullDetails.Contains(data));

                }
                else
                {
                    result = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                }
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }


        [HttpPost]
        public ActionResult GetLookupAppSUBCategory(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string getdata = string.Empty;
                if (data != null && data != "")
                {
                    var tempData = data.Replace("Description", "!");
                    string[] singleTRdata = tempData.Split('!');
                    char[] chardata = singleTRdata[1].ToCharArray();

                    for (int i = 0; i < chardata.Length; i++)
                    {

                        if (chardata[i] == 'C')
                        {
                            break;
                        }
                        getdata += chardata[i];
                    }

                }


                var result = (object)null;
                int AppCatId = Convert.ToInt32(getdata);
                var fall = db.AppCategory.Include(e => e.AppSubCategory).Where(e => e.Id == AppCatId).Select(s => s.AppSubCategory).ToList();
                IEnumerable<AppCategory> all;

                if (fall.Count() > 0)
                {
                    result = (from ca in fall select new { srno = ca.Select(e => e.Id), lookupvalue = ca.Select(f => f.FullDetails) }).Distinct();

                }

                var a = fall.Select(r => new { IDS = r.First().Id.ToString() });
                var VAL = a.Select(s => s.IDS).FirstOrDefault();
                //Session["AppSubCat"] = VAL;
                //TempData["AppSubCat"] = VAL;
                return Json(result, JsonRequestBehavior.AllowGet);
                

            }

        }




        public ActionResult GetCalendarDetailLKDetails()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).AsEnumerable()
                    .Select(e => new
                    {
                        Id = e.Id,
                        Lvcalendardesc = "From Date :" + e.FromDate.Value.ToShortDateString() + " To Date :" + e.ToDate.Value.ToShortDateString(),

                    }).SingleOrDefault();
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }



        public ActionResult MaxRatingBind(int data)
        {
            //int data = Convert.ToInt32(TempData["AppSubCat"]);
            List<IsRatingObjectiveDetailsC> return_data = new List<IsRatingObjectiveDetailsC>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data > 0)
                {
                    var filter = Convert.ToInt32(data);
                    var qurey = db.AppSubCategoryRating.Include(e => e.AppSubCategory).Where(e => e.AppSubCategory.Id == filter).Select(q => q.MaxPoints).SingleOrDefault();
                    if (qurey != 0)
                    {
                        var a = db.AppSubCategoryRating.Include(e => e.AppRatingObjective).Include(e => e.AppRatingObjective.Select(ed => ed.ObjectiveWordings))
                                   .Include(e => e.AppSubCategory)
                        .Where(e => e.AppSubCategory.Id == filter).ToList();
                        foreach (var ca in a.Select(q => q.AppRatingObjective))
                        {

                            //  var obj = ca.Select(e => e.ObjectiveWordings.LookupVal.ToString()).SingleOrDefault();
                            //  var aaa = "Objective Wordings :" + obj != null ? obj : "" + ", Rating Points :" + ca.Select(e => e.RatingPoints);
                            return_data.Add(new IsRatingObjectiveDetailsC
                            {
                                AppRatingObjective_Id = ca.Select(e => e.Id.ToString()).ToArray(),
                                IsRatingObjective_FullDetails = ca.Select(e => e.FullDetails).ToArray(),
                            });
                        }
                        return this.Json(new Object[] { qurey, return_data, "", "", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        return Json(new Utility.JsonReturnClass { success = false, responseText = "AppSubCategoryRating is not defined for selected AppCategory...!" }, JsonRequestBehavior.AllowGet);
                    }



                }
                return View();
            }
        }

        //public ActionResult GetCalendarDetailLKDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall =db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).ToList();
        //        IEnumerable<Calendar> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.Calendar.ToList().Where(d => d.Id.ToString().Contains(data));
        //        }
        //        else
        //        {
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullDetails }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //}
        public class FuncStruct1
        {
            public Company Company { get; set; }
            public DBTrack DBTrack { get; set; }
            public string FullDetails { get; set; }

            public int Id { get; set; }
            public Job Job { get; set; }
            public JobPosition JobPosition { get; set; }

            public byte[] RowVersion { get; set; }
        }
        [HttpPost]
        public ActionResult Create(AppAssignment c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string AppraisalCalendar = form["AppraisalCalendarlist"] == "0" ? "" : form["AppraisalCalendarlist"];
                    string AppCategory = form["AppCategorylist"] == "0" ? "" : form["AppCategorylist"];
                    string AppSubCategory = form["AppSubCategorylist"] == "0" ? "" : form["AppSubCategorylist"];
                    string AppRatingObjective = form["AppRatingObjectivelist"] == "0" ? "" : form["AppRatingObjectivelist"];
                    string geo_id = form["geo_id"] == "" ? null : form["geo_id"];
                    string fun_id = form["fun_id"] == "" ? null : form["fun_id"];
                    string pay_id = form["pay_id"] == "" ? null : form["pay_id"];
                    var lai = new List<int>();
                    var lai1 = new List<int>();
                    var lai2 = new List<int>();


                    int CompId = 0;
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = int.Parse(Session["CompId"].ToString());
                    }


                    CompanyAppraisal OCompanyAppraisal = null;
                    List<AppAssignment> OFAT = new List<AppAssignment>();
                    OCompanyAppraisal = db.CompanyAppraisal.Where(e => e.Company.Id == CompId).SingleOrDefault();

                    if (AppraisalCalendar == null)
                    {
                        //  return this.Json(new Object[] { null, null, "Country cannot be null.", JsonRequestBehavior.AllowGet });
                        Msg.Add("AppraisalCalendar cannot be null.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (geo_id == null && fun_id == null && pay_id == null)
                    {
                        Msg.Add("Kindly select filter.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (AppCategory != null && AppCategory != "")
                    {
                        AppCategory App = db.AppCategory.Find(Convert.ToInt32(AppCategory));
                        c.AppCategory = App;
                    }

                    if (AppSubCategory != null && AppSubCategory != "")
                    {
                        AppSubCategory AppSub = db.AppSubCategory.Find(Convert.ToInt32(AppSubCategory));
                        c.AppSubCategory = AppSub;
                    }


                    if (AppraisalCalendar != null && AppraisalCalendar != "")
                    {
                        int AddId = Convert.ToInt32(AppraisalCalendar);
                        var val = db.Calendar.Include(e => e.Name).Where(e => e.Id == AddId).SingleOrDefault();
                        c.AppraisalCalendar = val;
                    }




                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                          new System.TimeSpan(0, 30, 0)))
                        {
                            //if (db.AppAssignment.Any(o => o.AppraisalCalendar.Id == c.AppraisalCalendar.Id && o.AppCategory.Id == c.AppCategory.Id && o.AppSubCategory.Id == c.AppSubCategory.Id))
                            //{
                            //    Msg.Add("Record Already Exists.");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}

                            try
                            {
                                c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                if ((geo_id != null && geo_id != "") && (pay_id != null && pay_id != "") && (fun_id != null && fun_id != ""))
                                {
                                    var ids = Utility.StringIdsToListIds(geo_id);
                                    var payids = Utility.StringIdsToListIds(pay_id);
                                    var funids = Utility.StringIdsToListIds(fun_id);

                                    foreach (var G in ids)
                                    {
                                        foreach (var P in payids)
                                        {
                                            foreach (var F in funids)
                                            {
                                                var geo = db.GeoStruct.Find(G);
                                                c.GeoStruct = geo;

                                                var pay = db.PayStruct.Find(P);
                                                c.PayStruct = pay;

                                                var fun = db.FuncStruct.Find(F);
                                                c.FuncStruct = fun;

                                                c.AppRatingObjective = null;
                                                List<AppRatingObjective> cp = new List<AppRatingObjective>();

                                                if (AppRatingObjective != null && AppRatingObjective != "")
                                                {
                                                    var ids1 = Utility.StringIdsToListIds(AppRatingObjective);
                                                    foreach (var ca in ids1)
                                                    {
                                                        var p_val = db.AppRatingObjective.Find(ca);
                                                        cp.Add(p_val);
                                                        c.AppRatingObjective = cp;

                                                    }
                                                }

                                                AppAssignment AppAssigngeo = new AppAssignment()
                                                {
                                                    MaxRatingPoints = c.MaxRatingPoints == null ? 0 : c.MaxRatingPoints,
                                                    AppraisalCalendar = c.AppraisalCalendar,
                                                    AppSubCategory = c.AppSubCategory,
                                                    AppRatingObjective = c.AppRatingObjective,
                                                    AppCategory = c.AppCategory,
                                                    GeoStruct = c.GeoStruct,
                                                    FuncStruct = c.FuncStruct,
                                                    PayStruct = c.PayStruct,
                                                    DBTrack = c.DBTrack
                                                };

                                                db.AppAssignment.Add(AppAssigngeo);
                                                db.SaveChanges();

                                                OFAT.Add(db.AppAssignment.Find(AppAssigngeo.Id));
                                            }

                                        }

                                    }

                                }


                                if ((geo_id != null && geo_id != "") && (pay_id != null && pay_id != "") && (fun_id == null || fun_id == ""))
                                {
                                    var ids = Utility.StringIdsToListIds(geo_id);
                                    var payids = Utility.StringIdsToListIds(pay_id);

                                    foreach (var G in ids)
                                    {
                                        foreach (var P in payids)
                                        {
                                            var geo = db.GeoStruct.Find(G);
                                            c.GeoStruct = geo;

                                            var pay = db.PayStruct.Find(P);
                                            c.PayStruct = pay;

                                            c.FuncStruct = null;

                                            c.AppRatingObjective = null;
                                            List<AppRatingObjective> cp = new List<AppRatingObjective>();

                                            if (AppRatingObjective != null && AppRatingObjective != "")
                                            {
                                                var ids1 = Utility.StringIdsToListIds(AppRatingObjective);
                                                foreach (var ca in ids1)
                                                {
                                                    var p_val = db.AppRatingObjective.Find(ca);
                                                    cp.Add(p_val);
                                                    c.AppRatingObjective = cp;

                                                }
                                            }
                                            AppAssignment AppAssigngeo = new AppAssignment()
                                            {
                                                MaxRatingPoints = c.MaxRatingPoints == null ? 0 : c.MaxRatingPoints,
                                                AppraisalCalendar = c.AppraisalCalendar,
                                                AppSubCategory = c.AppSubCategory,
                                                AppRatingObjective = c.AppRatingObjective,
                                                AppCategory = c.AppCategory,
                                                GeoStruct = c.GeoStruct,
                                                FuncStruct = c.FuncStruct,
                                                PayStruct = c.PayStruct,
                                                DBTrack = c.DBTrack
                                            };

                                            db.AppAssignment.Add(AppAssigngeo);
                                            db.SaveChanges();

                                            OFAT.Add(db.AppAssignment.Find(AppAssigngeo.Id));

                                        }

                                    }

                                }

                                if ((fun_id != null && fun_id != "") && (pay_id != null && pay_id != "") && (geo_id == null || geo_id == ""))
                                {

                                    var ids = Utility.StringIdsToListIds(fun_id);
                                    var payids = Utility.StringIdsToListIds(pay_id);
                                    foreach (var F in ids)
                                    {
                                        foreach (var P in payids)
                                        {
                                            var fun = db.FuncStruct.Find(F);
                                            c.FuncStruct = fun;

                                            c.GeoStruct = null;

                                            var pay = db.PayStruct.Find(P);
                                            c.PayStruct = pay;

                                            c.AppRatingObjective = null;
                                            List<AppRatingObjective> cp = new List<AppRatingObjective>();
                                            if (AppRatingObjective != null && AppRatingObjective != "")
                                            {
                                                var ids1 = Utility.StringIdsToListIds(AppRatingObjective);
                                                foreach (var ca in ids1)
                                                {
                                                    var p_val = db.AppRatingObjective.Find(ca);
                                                    cp.Add(p_val);
                                                    c.AppRatingObjective = cp;

                                                }
                                            }

                                            AppAssignment AppAssignfun = new AppAssignment()
                                            {
                                                MaxRatingPoints = c.MaxRatingPoints == null ? 0 : c.MaxRatingPoints,
                                                AppraisalCalendar = c.AppraisalCalendar,
                                                AppSubCategory = c.AppSubCategory,
                                                AppRatingObjective = c.AppRatingObjective,
                                                AppCategory = c.AppCategory,
                                                GeoStruct = c.GeoStruct,
                                                FuncStruct = c.FuncStruct,
                                                PayStruct = c.PayStruct,
                                                DBTrack = c.DBTrack
                                            };

                                            db.AppAssignment.Add(AppAssignfun);
                                            db.SaveChanges();

                                            OFAT.Add(db.AppAssignment.Find(AppAssignfun.Id));
                                        }

                                    }
                                }


                                if ((fun_id != null && fun_id != "") && (geo_id != null && geo_id != "") && (pay_id == null || pay_id == ""))
                                {
                                    var ids = Utility.StringIdsToListIds(fun_id);
                                    var geoids = Utility.StringIdsToListIds(geo_id);

                                    foreach (int F in ids)
                                    {
                                        foreach (var G in geoids)
                                        {
                                            var fun = db.FuncStruct.Find(F);
                                            c.FuncStruct = fun;
                                            var geo = db.GeoStruct.Find(G);
                                            c.GeoStruct = geo;

                                            c.PayStruct = null;


                                            c.AppRatingObjective = null;
                                            List<AppRatingObjective> cp = new List<AppRatingObjective>();
                                            if (AppRatingObjective != null && AppRatingObjective != "")
                                            {
                                                var ids1 = Utility.StringIdsToListIds(AppRatingObjective);
                                                foreach (var ca in ids1)
                                                {
                                                    var p_val = db.AppRatingObjective.Find(ca);
                                                    cp.Add(p_val);
                                                    c.AppRatingObjective = cp;

                                                }
                                            }

                                            AppAssignment AppAssignPay = new AppAssignment()
                                            {
                                                MaxRatingPoints = c.MaxRatingPoints,
                                                AppraisalCalendar = c.AppraisalCalendar,
                                                AppSubCategory = c.AppSubCategory,
                                                AppRatingObjective = c.AppRatingObjective,
                                                AppCategory = c.AppCategory,
                                                GeoStruct = c.GeoStruct,
                                                FuncStruct = c.FuncStruct,
                                                PayStruct = c.PayStruct,
                                                DBTrack = c.DBTrack
                                            };

                                            db.AppAssignment.Add(AppAssignPay);
                                            db.SaveChanges();

                                            OFAT.Add(db.AppAssignment.Find(AppAssignPay.Id));
                                        }

                                    }
                                }

                                #region Only Single Filter Apply Code Start

                                // Apply Geo
                                if ((geo_id != null && geo_id != "") && (pay_id == null || pay_id == "") && (fun_id == null || fun_id == ""))
                                {
                                    var ids = Utility.StringIdsToListIds(geo_id);

                                    foreach (var G in ids)
                                    {
                                        var geo = db.GeoStruct.Find(G);
                                        c.GeoStruct = geo;

                                        c.PayStruct = null;
                                        c.FuncStruct = null;

                                        c.AppRatingObjective = null;
                                        List<AppRatingObjective> cp = new List<AppRatingObjective>();

                                        if (AppRatingObjective != null && AppRatingObjective != "")
                                        {
                                            var ids1 = Utility.StringIdsToListIds(AppRatingObjective);
                                            foreach (var ca in ids1)
                                            {
                                                var p_val = db.AppRatingObjective.Find(ca);
                                                cp.Add(p_val);
                                                c.AppRatingObjective = cp;

                                            }
                                        }
                                        AppAssignment AppAssigngeo = new AppAssignment()
                                        {
                                            MaxRatingPoints = c.MaxRatingPoints == null ? 0 : c.MaxRatingPoints,
                                            AppraisalCalendar = c.AppraisalCalendar,
                                            AppSubCategory = c.AppSubCategory,
                                            AppRatingObjective = c.AppRatingObjective,
                                            AppCategory = c.AppCategory,
                                            GeoStruct = c.GeoStruct,
                                            FuncStruct = c.FuncStruct,
                                            PayStruct = c.PayStruct,
                                            DBTrack = c.DBTrack
                                        };

                                        db.AppAssignment.Add(AppAssigngeo);
                                        db.SaveChanges();

                                        OFAT.Add(db.AppAssignment.Find(AppAssigngeo.Id));

                                    }

                                }

                                //Apply Pay
                                if ((pay_id != null && pay_id != "") && (geo_id == null || geo_id == "") && (fun_id == null || fun_id == ""))
                                {
                                    var payids = Utility.StringIdsToListIds(pay_id);

                                    foreach (var P in payids)
                                    {
                                        var pay = db.PayStruct.Find(P);
                                        c.PayStruct = pay;

                                        c.GeoStruct = null;
                                        c.FuncStruct = null;

                                        c.AppRatingObjective = null;
                                        List<AppRatingObjective> cp = new List<AppRatingObjective>();

                                        if (AppRatingObjective != null && AppRatingObjective != "")
                                        {
                                            var ids1 = Utility.StringIdsToListIds(AppRatingObjective);
                                            foreach (var ca in ids1)
                                            {
                                                var p_val = db.AppRatingObjective.Find(ca);
                                                cp.Add(p_val);
                                                c.AppRatingObjective = cp;

                                            }
                                        }
                                        AppAssignment AppAssigngeo = new AppAssignment()
                                        {
                                            MaxRatingPoints = c.MaxRatingPoints == null ? 0 : c.MaxRatingPoints,
                                            AppraisalCalendar = c.AppraisalCalendar,
                                            AppSubCategory = c.AppSubCategory,
                                            AppRatingObjective = c.AppRatingObjective,
                                            AppCategory = c.AppCategory,
                                            GeoStruct = c.GeoStruct,
                                            FuncStruct = c.FuncStruct,
                                            PayStruct = c.PayStruct,
                                            DBTrack = c.DBTrack
                                        };

                                        db.AppAssignment.Add(AppAssigngeo);
                                        db.SaveChanges();

                                        OFAT.Add(db.AppAssignment.Find(AppAssigngeo.Id));

                                    }

                                }

                                //Apply Fun
                                if ((fun_id != null && fun_id != "") && (geo_id == null || geo_id == "") && (pay_id == null || pay_id == ""))
                                {
                                    var funids = Utility.StringIdsToListIds(fun_id);

                                    foreach (var F in funids)
                                    {
                                        var fun = db.FuncStruct.Find(F);
                                        c.FuncStruct = fun;

                                        c.PayStruct = null;
                                        c.GeoStruct = null;


                                        c.AppRatingObjective = null;
                                        List<AppRatingObjective> cp = new List<AppRatingObjective>();

                                        if (AppRatingObjective != null && AppRatingObjective != "")
                                        {
                                            var ids1 = Utility.StringIdsToListIds(AppRatingObjective);
                                            foreach (var ca in ids1)
                                            {
                                                var p_val = db.AppRatingObjective.Find(ca);
                                                cp.Add(p_val);
                                                c.AppRatingObjective = cp;

                                            }
                                        }
                                        AppAssignment AppAssigngeo = new AppAssignment()
                                        {
                                            MaxRatingPoints = c.MaxRatingPoints == null ? 0 : c.MaxRatingPoints,
                                            AppraisalCalendar = c.AppraisalCalendar,
                                            AppSubCategory = c.AppSubCategory,
                                            AppRatingObjective = c.AppRatingObjective,
                                            AppCategory = c.AppCategory,
                                            GeoStruct = c.GeoStruct,
                                            FuncStruct = c.FuncStruct,
                                            PayStruct = c.PayStruct,
                                            DBTrack = c.DBTrack
                                        };

                                        db.AppAssignment.Add(AppAssigngeo);
                                        db.SaveChanges();

                                        OFAT.Add(db.AppAssignment.Find(AppAssigngeo.Id));

                                    }

                                }

                                #endregion Only Single Filter Apply Code End




                                if (OCompanyAppraisal == null)
                                {
                                    CompanyAppraisal OTEP = new CompanyAppraisal()
                                    {
                                        Company = db.Company.Find(SessionManager.CompanyId),
                                        AppAssignment = OFAT,
                                        DBTrack = c.DBTrack

                                    };


                                    db.CompanyAppraisal.Add(OTEP);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    var aa = db.CompanyAppraisal.Find(OCompanyAppraisal.Id);
                                    if (aa.AppAssignment != null)
                                    {
                                        OFAT.AddRange(aa.AppAssignment);
                                    }

                                    aa.AppAssignment = OFAT;
                                    db.CompanyAppraisal.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                }

                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DataException /* dex */)
                            {
                                //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                                //ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                                //return View(level);
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
                //  return View();
            }
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
                IEnumerable<AppAssignment> Corporate = null;
                if (gp.IsAutho == true)
                {
                    Corporate = db.AppAssignment.Include(e => e.AppraisalCalendar)
                        .Include(e => e.AppCategory)
                        .Include(e => e.AppSubCategory)
                        .Include(e => e.AppRatingObjective).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Corporate = db.AppAssignment.Include(e => e.AppraisalCalendar)
                        .Include(e => e.AppCategory)
                        .Include(e => e.AppSubCategory)
                        .Include(e => e.AppRatingObjective).AsNoTracking().ToList();
                }

                IEnumerable<AppAssignment> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = Corporate;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                              || (e.AppCategory.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                              || (e.AppSubCategory.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                              || (e.MaxRatingPoints.ToString().Contains(gp.searchString))

                              ).Select(a => new { a.Id, a.AppCategory.Name, a.AppSubCategory.FullDetails, a.MaxRatingPoints, a.AppCategory }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.AppCategory.Name, a.AppSubCategory.Name, a.MaxRatingPoints }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Corporate;
                    Func<AppAssignment, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "MaxRatingPoints" ? c.MaxRatingPoints.ToString() :
                                         gp.sidx == "AppCategory" ? c.AppCategory.Name.ToString() :
                                         gp.sidx == "AppSubCategory" ? c.AppSubCategory.Name.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.AppCategory.Name, a.AppSubCategory.Name, a.MaxRatingPoints }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.AppCategory.Name, a.AppSubCategory.Name, a.MaxRatingPoints }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.AppCategory.Name, a.AppSubCategory.Name, a.MaxRatingPoints }).ToList();
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
                throw ex;
            }

        }


        public class IsRatingObjectiveDetailsC
        {
            public Array AppRatingObjective_Id { get; set; }
            public Array IsRatingObjective_FullDetails { get; set; }
            public Array AppraisalCalendar_Id { get; set; }
            public string AppraisalCalendar_FullDetails { get; set; }

            public string AppCategory_Id { get; set; }
            public string AppCategory_Id_FullDetails { get; set; }
            public string AppSubCategory_Id_Id { get; set; }
            public string AppSubCategory_FullDetails { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            List<IsRatingObjectiveDetailsC> return_data = new List<IsRatingObjectiveDetailsC>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var lookup = db.AppAssignment.Include(e => e.AppraisalCalendar)
                                    .Include(e => e.AppCategory)
                                    .Include(e => e.AppSubCategory)
                                    .Include(e => e.AppRatingObjective)
                                    .Include(e => e.AppRatingObjective.Select(t => t.ObjectiveWordings))
                 .Where(e => e.Id == data).ToList();
                var r = (from e in lookup
                         select new
                         {
                             MaxRatingPoints = e.MaxRatingPoints,
                             AppraisalCalendar_Id = e.AppraisalCalendar == null ? null : e.AppraisalCalendar.Id.ToString(),
                             AppraisalCalendar_FullDetails = e.AppraisalCalendar.FullDetails.ToString(),
                             AppCategory_Id = e.AppCategory == null ? 0 : e.AppCategory.Id,
                             AppSubCategory_Id = e.AppSubCategory == null ? 0 : e.AppSubCategory.Id,

                             Action = e.DBTrack.Action
                         }).Distinct();

                var a = db.AppAssignment.Include(e => e.AppraisalCalendar)
                                    .Include(e => e.AppCategory)
                                    .Include(e => e.AppSubCategory)
                 .Where(e => e.Id == data).ToList();
                foreach (var ca in a)
                {
                    return_data.Add(new IsRatingObjectiveDetailsC
                    {
                        AppRatingObjective_Id = ca.AppRatingObjective.Select(e => e.Id.ToString()).ToArray(),
                        IsRatingObjective_FullDetails = ca.AppRatingObjective.Select(e => e.FullDetails).ToArray(),
                        AppraisalCalendar_Id = ca.AppraisalCalendar.Id.ToString().ToArray(),
                        AppraisalCalendar_FullDetails = ca.AppraisalCalendar.FullDetails.ToString(),

                        AppCategory_Id = ca.AppCategory.Id.ToString(),
                        AppCategory_Id_FullDetails = ca.AppCategory.FullDetails.ToString(),
                        AppSubCategory_Id_Id = ca.AppSubCategory.Id.ToString(),
                        AppSubCategory_FullDetails = ca.AppSubCategory.FullDetails.ToString()


                    });
                }


                var LKup = db.AppAssignment.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, return_data, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        //[HttpPost]
        //public async Task<ActionResult> EditSave(AppAssignment L, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //        try
        //        {
        //            string AppraisalCalendar = form["AppraisalCalendarlist"] == "0" ? "" : form["AppraisalCalendarlist"];
        //            string AppCategory = form["AppCategorylist"] == "0" ? "" : form["AppCategorylist"];
        //            string AppSubCategory = form["AppSubCategorylist"] == "0" ? "" : form["AppSubCategorylist"];
        //            string AppRatingObjective = form["AppRatingObjectivelist"] == "0" ? "" : form["AppRatingObjectivelist"];
        //            string geo_id = form["geo_id"] == "0" ? "" : form["geo_id"];
        //            string fun_id = form["fun_id"] == "0" ? "" : form["fun_id"];
        //            string pay_id = form["pay_id"] == "0" ? "" : form["pay_id"];
        //            var blog1 = db.AppAssignment.Where(e => e.Id == data).Include(e => e.AppraisalCalendar)
        //                        .Include(e => e.AppCategory)
        //                        .Include(e => e.AppSubCategory)
        //                        .Include(e => e.AppRatingObjective).SingleOrDefault();

        //            blog1.FuncStruct = null;
        //            blog1.AppRatingObjective = null;
        //            blog1.AppraisalCalendar = null;
        //            blog1.GeoStruct = null;
        //            blog1.PayStruct = null;

        //            if (AppCategory != null)
        //            {
        //                if (AppCategory != "")
        //                {
        //                    AppCategory App = db.AppCategory.Find(Convert.ToInt32(AppCategory));
        //                    L.AppCategory = App;
        //                }
        //            }

        //            if (AppSubCategory != null)
        //            {
        //                if (AppSubCategory != "")
        //                {
        //                    AppSubCategory AppSub = db.AppSubCategory.Find(Convert.ToInt32(AppSubCategory));
        //                    L.AppSubCategory = AppSub;
        //                }
        //            }


        //            if (AppraisalCalendar != null)
        //            {
        //                if (AppraisalCalendar != "")
        //                {
        //                    int AddId = Convert.ToInt32(AppraisalCalendar);
        //                    var val = db.Calendar.Include(e => e.Name).Where(e => e.Id == AddId).SingleOrDefault();
        //                    L.AppraisalCalendar = val;
        //                }
        //            }

        //            L.AppRatingObjective = null;
        //            List<AppRatingObjective> cp = new List<AppRatingObjective>();
        //            if (AppRatingObjective != null && AppRatingObjective != "")
        //            {
        //                var ids = Utility.StringIdsToListIds(AppRatingObjective);
        //                foreach (var ca in ids)
        //                {
        //                    var p_val = db.AppRatingObjective.Find(ca);
        //                    cp.Add(p_val);
        //                    L.AppRatingObjective = cp;

        //                }
        //            }
        //            blog1.MaxRatingPoints = L.MaxRatingPoints;



        //            if (ModelState.IsValid)
        //            {
        //                try
        //                {
        //                    {
        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            using (var context = new DataBaseContext())
        //                            {
        //                                //blog = context.PostDetails.Where(e => e.Id == data).Include(e => e.FuncStruct)
        //                                //              .Include(e => e.FuncStruct.JobPosition)                                    
        //                                //              .Include(e=>e.FuncStruct.Job)
        //                                //              .Include(e => e.ExpFilter)
        //                                //              .Include(e => e.RangeFilter)
        //                                //              .Include(e => e.Qualification)
        //                                //              .Include(e => e.Skill)
        //                                //              .Include(e => e.Gender)
        //                                //              .Include(e => e.MaritalStatus)
        //                                //              .Include(e => e.CategoryPost)
        //                                //              .Include(e => e.CategoryPost.Select(q=>q.Category))
        //                                //              .Include(e => e.CategorySplPost)
        //                                //              .Include(e => e.CategorySplPost.Select(q => q.SpecialCategory))
        //                                //                        .SingleOrDefault();
        //                                //originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            blog1.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog1.DBTrack.CreatedBy == null ? null : blog1.DBTrack.CreatedBy,
        //                                CreatedOn = blog1.DBTrack.CreatedOn == null ? null : blog1.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };

        //                            var CurCorp = db.AppAssignment.Find(data);
        //                            TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            {

        //                                AppAssignment post = new AppAssignment()
        //                                {
        //                                    MaxRatingPoints = L.MaxRatingPoints == null ? 0 : L.MaxRatingPoints,
        //                                    AppraisalCalendar = L.AppraisalCalendar,
        //                                    AppSubCategory = L.AppSubCategory,
        //                                    AppRatingObjective = L.AppRatingObjective,
        //                                    AppCategory = L.AppCategory,
        //                                    GeoStruct = L.GeoStruct,
        //                                    FuncStruct = L.FuncStruct,
        //                                    PayStruct = L.PayStruct,
        //                                    Id = data,
        //                                    DBTrack = blog1.DBTrack
        //                                };
        //                                db.AppAssignment.Attach(post);
        //                                db.Entry(post).State = System.Data.Entity.EntityState.Modified;

        //                                db.Entry(post).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                                db.SaveChanges();

        //                                await db.SaveChangesAsync();
        //                                db.Entry(post).State = System.Data.Entity.EntityState.Detached;
        //                                var b = db.AppAssignment.Where(e => e.Id == data).Include(e => e.AppraisalCalendar)
        //                         .Include(e => e.AppCategory)
        //                         .Include(e => e.AppSubCategory)
        //                         .Include(e => e.AppRatingObjective).SingleOrDefault();
        //                                ts.Complete();
        //                                Msg.Add("  Record Updated");
        //                                return Json(new Utility.JsonReturnClass { Id = b.Id, Val = b.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                                // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

        //                            }
        //                        }
        //                    }
        //                }
        //                catch (DbUpdateConcurrencyException ex)
        //                {
        //                    var entry = ex.Entries.Single();
        //                    var clientValues = (AppAssignment)entry.Entity;
        //                    var databaseEntry = entry.GetDatabaseValues();
        //                    if (databaseEntry == null)
        //                    {
        //                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                    }
        //                    else
        //                    {
        //                        var databaseValues = (AppAssignment)databaseEntry.ToObject();
        //                        blog1.RowVersion = databaseValues.RowVersion;

        //                    }
        //                }
        //                Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });

        //            }
        //            return Json(new Object[] { "", "", "", JsonRequestBehavior.AllowGet });
        //        }
        //        catch (Exception ex)
        //        {
        //            LogFile Logfile = new LogFile();
        //            ErrorLog Err = new ErrorLog()
        //            {
        //                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                ExceptionMessage = ex.Message,
        //                ExceptionStackTrace = ex.StackTrace,
        //                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //}

        [HttpPost]
        public async Task<ActionResult> EditSave(AppAssignment add, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string AppraisalCalendarp = form["AppraisalCalendarlist"] == "0" ? "" : form["AppraisalCalendarlist"];
                    //string AppCategory = form["AppCategorylist"] == "0" ? "" : form["AppCategorylist"];
                    //string AppSubCategory = form["AppSubCategorylist"] == "0" ? "" : form["AppSubCategorylist"];
                    string AppRatingObjective = form["AppRatingObjectivelist"] == "0" ? "" : form["AppRatingObjectivelist"];
                    string geo_id = form["geo_id"] == "0" ? "" : form["geo_id"];
                    string fun_id = form["fun_id"] == "0" ? "" : form["fun_id"];
                    string pay_id = form["pay_id"] == "0" ? "" : form["pay_id"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    try
                    {
                        var AppraisalCalendar = AppraisalCalendarp.Replace(",", string.Empty);
                        if (AppraisalCalendar == null)
                        {
                            //  return this.Json(new Object[] { null, null, "Country cannot be null.", JsonRequestBehavior.AllowGet });
                            Msg.Add("AppraisalCalendar cannot be null.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        var predata = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Where(e => e.Id == data).SingleOrDefault();
                        add.AppCategory = predata.AppCategory;
                        add.AppSubCategory = predata.AppSubCategory;
                        string AppCategory = predata.AppSubCategory.Id.ToString();
                        string AppSubCategory = predata.AppSubCategory.Id.ToString();
                        //if (AppCategory != null && AppCategory != "" && AppCategory != "0")
                        //{
                        //    var val = db.AppCategory.Find(int.Parse(AppCategory));
                        //    add.AppCategory = val;
                        //}

                        //if (AppSubCategory != null && AppSubCategory != "" && AppSubCategory != "0")
                        //{
                        //    var val = db.AppSubCategory.Find(int.Parse(AppSubCategory));
                        //    add.AppSubCategory = val;
                        //}

                        List<AppRatingObjective> ObjITsection = new List<AppRatingObjective>();
                        AppRatingObjective pd = null;
                        pd = db.AppRatingObjective.Include(e => e.ObjectiveWordings).Where(e => e.Id == data).SingleOrDefault();
                        if (AppRatingObjective != null && AppRatingObjective != "")
                        {
                            var ids = Utility.StringIdsToListIds(AppRatingObjective);
                            foreach (var ca in ids)
                            {
                                var value = db.AppRatingObjective.Find(ca);
                                ObjITsection.Add(value);
                                add.AppRatingObjective = ObjITsection;

                            }
                        }
                        else
                        {
                            add.AppRatingObjective = null;
                        }

                        if (Auth == false)
                        {
                            if (ModelState.IsValid)
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    AppAssignment blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.AppAssignment.Where(e => e.Id == data).Include(e => e.AppCategory)
                                    .Include(e => e.AppSubCategory).Include(e => e.AppRatingObjective).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    add.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    int a = EditS(AppraisalCalendar, AppCategory, AppSubCategory, AppRatingObjective, data, add, add.DBTrack);




                                    db.SaveChanges();
                                    await db.SaveChangesAsync();
                                    var query = db.AppAssignment.Where(e => e.Id == data).Include(e => e.AppCategory)
                                    .Include(e => e.AppSubCategory).Include(e => e.AppRatingObjective).SingleOrDefault();
                                    ts.Complete();

                                    Msg.Add("Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = query.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                                }



                            }
                        }
                        else
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {

                                AppAssignment blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;
                                AppAssignment Old_Addrs = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.AppAssignment.Where(e => e.Id == data).SingleOrDefault();
                                    originalBlogValues = context.Entry(blog).OriginalValues;
                                }
                                add.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    IsModified = blog.DBTrack.IsModified == true ? true : false,
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };
                                AppAssignment L = new AppAssignment()
                                {
                                    MaxRatingPoints = add.MaxRatingPoints == null ? 0 : add.MaxRatingPoints,
                                    AppraisalCalendar = add.AppraisalCalendar,
                                    AppSubCategory = add.AppSubCategory,
                                    AppRatingObjective = add.AppRatingObjective,
                                    AppCategory = add.AppCategory,
                                    GeoStruct = add.GeoStruct,
                                    FuncStruct = add.FuncStruct,
                                    PayStruct = add.PayStruct,
                                    DBTrack = add.DBTrack,
                                    Id = data
                                };


                                //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                using (var context = new DataBaseContext())
                                {
                                    var obj = DBTrackFile.ModifiedDataHistory("Appraisal/Appraisal", "M", blog, add, "AppAssignment", add.DBTrack);
                                    // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                    Old_Addrs = context.AppAssignment.Where(e => e.Id == data).Include(e => e.AppCategory)
                                    .Include(e => e.AppSubCategory).Include(e => e.AppRatingObjective).SingleOrDefault();
                                    DT_AppAssignment DT_Addrs = (DT_AppAssignment)obj;
                                    DT_Addrs.AppCategory_Id = DBTrackFile.ValCompare(Old_Addrs.AppCategory, add.AppCategory);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                    DT_Addrs.AppraisalCalendar_Id = DBTrackFile.ValCompare(Old_Addrs.AppraisalCalendar, add.AppraisalCalendar); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                    DT_Addrs.AppSubCategory_Id = DBTrackFile.ValCompare(Old_Addrs.AppSubCategory, add.AppSubCategory); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                    DT_Addrs.FuncStruct_Id = DBTrackFile.ValCompare(Old_Addrs.FuncStruct, add.FuncStruct);
                                    DT_Addrs.GeoStruct_Id = DBTrackFile.ValCompare(Old_Addrs.GeoStruct, add.GeoStruct);
                                    DT_Addrs.PayStruct_Id = DBTrackFile.ValCompare(Old_Addrs.PayStruct, add.PayStruct);
                                    db.Create(DT_Addrs);
                                    //db.SaveChanges();
                                }
                                blog.DBTrack = add.DBTrack;
                                db.AppAssignment.Attach(blog);
                                db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                db.SaveChanges();
                                ts.Complete();
                                //  return Json(new Object[] { blog.Id, add.FullAddress, "Record Updated", JsonRequestBehavior.AllowGet });
                                Msg.Add("Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = add.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        var entry = ex.Entries.Single();
                        var clientValues = (AppAssignment)entry.Entity;
                        var databaseEntry = entry.GetDatabaseValues();
                        if (databaseEntry == null)
                        {
                            Msg.Add(ex.Message);
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var databaseValues = (AppAssignment)databaseEntry.ToObject();
                            add.RowVersion = databaseValues.RowVersion;

                        }
                    }
                    catch (Exception e)
                    {
                        Msg.Add(e.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
        public int EditS(string AppraisalCalendar, string AppCategory, string AppSubCategory, string AppRatingObjective, int data, AppAssignment c, DBTrack dbT)
        {
            IList<AppAssignment> typedetails = null;
            using (DataBaseContext db = new DataBaseContext())
            {
                if (AppCategory != null)
                {
                    if (AppCategory != "")
                    {
                        var val = db.AppCategory.Find(int.Parse(AppCategory));
                        c.AppCategory = val;

                        var add = db.AppAssignment.Include(e => e.AppCategory).Where(e => e.Id == data).SingleOrDefault();
                        IList<AppAssignment> contactsdetails = null;
                        if (add.AppCategory != null)
                        {
                            contactsdetails = db.AppAssignment.Where(x => x.AppCategory.Id == add.AppCategory.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            contactsdetails = db.AppAssignment.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.AppCategory = c.AppCategory;
                            db.AppAssignment.Attach(s);
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
                    var contactsdetails = db.AppAssignment.Include(e => e.AppCategory).Where(x => x.Id == data).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.AppCategory = null;
                        db.AppAssignment.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (AppSubCategory != null && AppSubCategory != "")
                {
                    var val = db.AppSubCategory.Find(int.Parse(AppSubCategory));
                    c.AppSubCategory = val;

                    var type = db.AppAssignment.Include(e => e.AppSubCategory).Where(e => e.Id == data).SingleOrDefault();
                    if (type.AppSubCategory != null)
                    {
                        typedetails = db.AppAssignment.Where(x => x.AppSubCategory.Id == type.AppSubCategory.Id && x.Id == data).ToList();
                    }
                    else
                    {
                        typedetails = db.AppAssignment.Where(x => x.Id == data).ToList();
                    }

                    foreach (var s in typedetails)
                    {
                        s.AppSubCategory = c.AppSubCategory;
                        db.AppAssignment.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                else
                {
                    var BusiTypeDetails = db.AppAssignment.Include(e => e.AppSubCategory).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.AppSubCategory = null;
                        db.AppAssignment.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                if (AppraisalCalendar != null)
                {
                    if (AppraisalCalendar != "")
                    {
                        var val = db.Calendar.Find(int.Parse(AppraisalCalendar));
                        c.AppraisalCalendar = val;

                        var add = db.AppAssignment.Include(e => e.AppraisalCalendar).Where(e => e.Id == data).SingleOrDefault();
                        IList<AppAssignment> contactsdetails = null;
                        if (add.AppraisalCalendar != null)
                        {
                            contactsdetails = db.AppAssignment.Where(x => x.AppraisalCalendar.Id == add.AppraisalCalendar.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            contactsdetails = db.AppAssignment.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.AppraisalCalendar = c.AppraisalCalendar;
                            db.AppAssignment.Attach(s);
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
                    var contactsdetails = db.AppAssignment.Include(e => e.AppraisalCalendar).Where(x => x.Id == data).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.AppraisalCalendar = null;
                        db.AppAssignment.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                List<AppRatingObjective> ObjiProd = new List<AppRatingObjective>();
                AppAssignment insuranceProduct = null;
                insuranceProduct = db.AppAssignment.Include(e => e.AppRatingObjective).Where(e => e.Id == data).SingleOrDefault();
                if (AppRatingObjective != null && AppRatingObjective != "")
                {
                    var ids = Utility.StringIdsToListIds(AppRatingObjective);
                    foreach (var ca in ids)
                    {
                        var value = db.AppRatingObjective.Find(ca);
                        ObjiProd.Add(value);
                        c.AppRatingObjective = ObjiProd;
                    }
                }
                else
                {
                    insuranceProduct.AppRatingObjective = null;
                }

                var CurCorp = db.AppAssignment.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    AppAssignment corp = new AppAssignment()
                    {
                        MaxRatingPoints = c.MaxRatingPoints == null ? 0 : c.MaxRatingPoints,
                        AppraisalCalendar = c.AppraisalCalendar,
                        AppSubCategory = c.AppSubCategory,
                        AppRatingObjective = c.AppRatingObjective,
                        AppCategory = c.AppCategory,
                        GeoStruct = c.GeoStruct,
                        FuncStruct = c.FuncStruct,
                        PayStruct = c.PayStruct,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.AppAssignment.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }


        public ActionResult GetFullDetailsGeo(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var company_ids = form["company-table"] != null ? form["company-table"] : null;
                var division_ids = form["division-table"] != null ? form["division-table"] : null;
                var location_ids = form["location-table"] != null ? form["location-table"] : null;
                var department_ids = form["department-table"] != null ? form["department-table"] : null;
                var group_ids = form["group-table"] != null ? form["group-table"] : null;
                var unit_ids = form["unit-table"] != null ? form["unit-table"] : null;
                if (company_ids == null && division_ids == null && location_ids == null && department_ids == null && group_ids == null && unit_ids == null)
                {
                    return Json("GeoStruct Id Not Found", JsonRequestBehavior.AllowGet);
                }
                var comp_id = Convert.ToInt32(company_ids);

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                List<int> Geo_list = new List<int>();
                if (division_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(division_ids);
                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db.GeoStruct.Where(e => e.Division != null && e.Division.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                }

                if (location_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(location_ids);
                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db.GeoStruct.Where(e => e.Location != null && e.Location.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                }

                if (department_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(department_ids);
                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db.GeoStruct.Where(e => e.Department != null && e.Department.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                }

                if (group_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(group_ids);
                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db.GeoStruct.Where(e => e.Group != null && e.Group.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                }
                if (unit_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(unit_ids);
                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db.GeoStruct.Where(e => e.Unit != null && e.Unit.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                }
                var comp_single_data = Geo_list.Distinct().ToList();
                List<string> geo_id = new List<string>();
                if (comp_single_data.Count != 0)
                {
                    foreach (var item in comp_single_data)
                    {
                        geo_id.Add(item.ToString());
                    }
                    return Json(geo_id, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Geo Id Not Found", JsonRequestBehavior.AllowGet);
                }
            }

            ////////string geo_id = form["geo_id"] != null ? form["geo_id"] : null;
            ////////string corporate_ids = form["corporate-table"] != null ? form["corporate-table"] : null;
            ////////string region = form["region-table"] != null ? form["region-table"] : null;

            ////////string company = form["company-table"] != null ? form["company-table"] : null;
            ////////string Division = form["division-table"] != null ? form["division-table"] : null;
            ////////string Location = form["location-table"] != null ? form["location-table"] : null;

            ////////string Department = form["department-table"] != null ? form["department-table"] : null;
            ////////string group = form["group-table"] != null ? form["group-table"] : null;
            ////////string unit = form["unit-table"] != null ? form["unit-table"] : null;

            ////////if (geo_id == null && corporate_ids == null && region == null && company == null && Division == null && Location == null && Department == null && group == null && unit == null)
            ////////{
            ////////    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            ////////}


            ////////var getData = db.GeoStruct.Include(e => e.Location)
            ////////    .Include(e => e.Region)
            ////////    .Include(e => e.Unit)
            ////////    .Include(e => e.Company)
            ////////    .Include(e => e.Corporate)
            ////////    .Include(e => e.Department)
            ////////    //.Include(e => e.Location)
            ////////    .Where(e => e.Id != null).ToList();

            ////////if (Division != null)
            ////////{
            ////////    var id = Convert.ToInt32(Division);

            ////////    getData = getData.Where(a => a.Division != null && a.Division.Id == id).ToList();
            ////////}
            ////////if (Location != null)
            ////////{
            ////////    var id = Convert.ToInt32(Location);
            ////////    getData = getData.Where(a => a.Location != null && a.Location.Id == id).ToList();
            ////////}
            ////////if (Department != null)
            ////////{
            ////////    var id = Convert.ToInt32(Department);
            ////////    getData = getData.Where(a => a.Department != null && a.Department.Id == id).ToList();
            ////////}
            ////////if (group != null)
            ////////{
            ////////    var id = Convert.ToInt32(group);

            ////////    getData = getData.Where(a => a.Group.Id == id).ToList();
            ////////}
            ////////if (unit != null)
            ////////{
            ////////    var id = Convert.ToInt32(unit);

            ////////    getData = getData.Where(a => a.Unit.Id == id).ToList();
            ////////}

            //////////var div = db.GeoStruct.Where(a => a.Division.Id.ToString() == Division).FirstOrDefault();
            //////////var Loc = db.GeoStruct.Where(a => a.Location.Id.ToString() == Location).FirstOrDefault();
            //////////var Dep = db.GeoStruct.Where(a => a.Department.Id.ToString() == Department).FirstOrDefault();
            ////////if (getData != null)
            ////////{
            ////////    return Json(new { success = true, responseText = "", data = getData.Select(E => E.Id) }, JsonRequestBehavior.AllowGet);
            ////////}
            ////////else
            ////////{
            ////////    return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            ////////}
        }



        public ActionResult GetFullDetailsFun(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var job_ids = form["job-table"] != null ? form["job-table"] : null;
                var jobposition_ids = form["jobposition-table"] != null ? form["jobposition-table"] : null;
                if (job_ids == null && jobposition_ids == null)
                {
                    return Json("FunStruct Id Not Found", JsonRequestBehavior.AllowGet);
                }
                List<int> FuncStruct_List = new List<int>();
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (job_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(job_ids);

                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db.FuncStruct.Where(e => e.Job != null && e.Job.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                FuncStruct_List.Add(ca);
                            }
                        }
                    }
                }
                if (jobposition_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(jobposition_ids);
                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db.FuncStruct.Where(e => e.JobPosition != null && e.JobPosition.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                FuncStruct_List.Add(ca);
                            }
                        }
                    }
                }

                var comp_single_data = FuncStruct_List.Distinct().ToList();
                List<string> geo_id = new List<string>();
                if (comp_single_data.Count != 0)
                {
                    foreach (var item in comp_single_data)
                    {
                        geo_id.Add(item.ToString());
                    }
                    return Json(geo_id, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("FunStruct Id Not Found", JsonRequestBehavior.AllowGet);
                }

                ////////string fun_id = form["fun_id"] != null ? form["fun_id"] : null;
                ////////string job = form["job-table"] != null ? form["job-table"] : null;
                ////////string jobposition = form["jobposition-table"] != null ? form["jobposition-table"] : null;
                ////////string Company = form["Company-table"] != null ? form["Company-table"] : null;
                ////////if (fun_id == null && job == null && jobposition == null && Company == null)
                ////////{
                ////////    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                ////////}
                ////////var getData = db.FuncStruct.Include(e => e.Company)
                ////////    .Include(e => e.JobPosition)
                ////////    .Include(e => e.Job)
                ////////    .Where(e => e.Id != null).ToList();

                //////////if (fun_id != null)
                //////////{
                //////////    var id = Convert.ToInt32(fun_id);
                //////////    getData = getData.Where(a => a. .Id != null && a.Company.Id == id).ToList();
                //////////}
                ////////if (job != null)
                ////////{
                ////////    var id = Convert.ToInt32(job);
                ////////    getData = getData.Where(a => a.Job.Id != null && a.Job.Id == id).ToList();
                ////////}
                ////////if (jobposition != null)
                ////////{
                ////////    var id = Convert.ToInt32(jobposition);
                ////////    getData = getData.Where(a => a.JobPosition.Id != null && a.JobPosition.Id == id).ToList();
                ////////}

                ////////if (Company != null)
                ////////{
                ////////    var id = Convert.ToInt32(Company);
                ////////    getData = getData.Where(a => a.Company.Id != null && a.Company.Id == id).ToList();
                ////////}
                //////////var div = db.GeoStruct.Where(a => a.Division.Id.ToString() == Division).FirstOrDefault();
                //////////var Loc = db.GeoStruct.Where(a => a.Location.Id.ToString() == Location).FirstOrDefault();
                //////////var Dep = db.GeoStruct.Where(a => a.Department.Id.ToString() == Department).FirstOrDefault();
                ////////if (getData != null)
                ////////{
                ////////    return Json(new { success = true, responseText = "", data = getData.Select(E => E.Id) }, JsonRequestBehavior.AllowGet);
                ////////}
                ////////else
                ////////{
                ////////    return Json(new { success = false }, JsonRequestBehavior.AllowGet);

                ////////}

            }


        }

        public ActionResult GetFullDetailsPay(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //////string pay_id = form["pay_id"] != null ? form["pay_id"] : null;
                //////string grade = form["grade-table"] != null ? form["grade-table"] : null;
                //////string jobstatus = form["jobstatus-table"] != null ? form["jobstatus-table"] : null;
                //////string level = form["level-table"] != null ? form["level-table"] : null;
                //////if (grade == null && jobstatus == null && level == null)
                //////{
                //////    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                //////}
                //////var getData = db.PayStruct.Include(e => e.Company)
                //////    .Include(e => e.Grade)
                //////    .Include(e => e.JobStatus)
                //////    .Include(e => e.Level)
                //////    .Where(e => e.Id != null).ToList();

                ////////if (pay_id != null)
                ////////{
                ////////    var id = Convert.ToInt32(pay_id);
                ////////    getData = getData.Where(a => a.Company.Id != null && a.Company.Id == id).ToList();
                ////////}
                //////if (grade != null)
                //////{
                //////    var id = Convert.ToInt32(grade);
                //////    getData = getData.Where(a => a.Grade.Id.ToString() == grade.ToString()).ToList();
                //////}
                //////if (jobstatus != null)
                //////{
                //////    var id = Convert.ToInt32(jobstatus);
                //////    getData = getData.Where(a => a.JobStatus.Id != null && a.JobStatus.Id == id).ToList();
                //////}
                //////if (level != null)
                //////{
                //////    var id = Convert.ToInt32(level);
                //////    getData = getData.Where(a => a.Level.Id != null && a.Level.Id == id).ToList();
                //////}
                ////////var div = db.GeoStruct.Where(a => a.Division.Id.ToString() == Division).FirstOrDefault();
                ////////var Loc = db.GeoStruct.Where(a => a.Location.Id.ToString() == Location).FirstOrDefault();
                ////////var Dep = db.GeoStruct.Where(a => a.Department.Id.ToString() == Department).FirstOrDefault();
                //////if (getData != null)
                //////{
                //////    return Json(new { success = true, responseText = "", data = getData.Select(E => E.Id) }, JsonRequestBehavior.AllowGet);
                //////}
                //////else
                //////{
                //////    return Json(new { success = false }, JsonRequestBehavior.AllowGet);

                //////}


                var grade_ids = form["grade-table"] != null ? form["grade-table"] : null; ;
                var level_ids = form["level-table"] != null ? form["level-table"] : null;
                var jobstatus_ids = form["jobstatus-table"] != null ? form["jobstatus-table"] : null;

                if (grade_ids == null && level_ids == null && jobstatus_ids == null)
                {
                    return Json("PayStruct Id Not Found", JsonRequestBehavior.AllowGet);
                }

                List<int> PayStruct_List = new List<int>();
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (grade_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(grade_ids);
                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db.PayStruct.Where(e => e.Grade != null && e.Grade.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                PayStruct_List.Add(ca);
                            }
                        }
                    }
                }
                if (level_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(level_ids);
                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db.PayStruct.Where(e => e.Level != null && e.Level.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                PayStruct_List.Add(ca);
                            }
                        }
                    }
                }
                if (jobstatus_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(jobstatus_ids);
                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db.PayStruct.Where(e => e.JobStatus != null && e.JobStatus.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                PayStruct_List.Add(ca);
                            }
                        }
                    }
                }
                var comp_single_data = PayStruct_List.Distinct().ToList();
                List<string> geo_id = new List<string>();
                if (comp_single_data.Count != 0)
                {
                    foreach (var item in comp_single_data)
                    {
                        geo_id.Add(item.ToString());
                    }
                    return Json(new Object[] { geo_id }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("PayStruct Id Not Found", JsonRequestBehavior.AllowGet);
                }

            }


        }

        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string AppCategory { get; set; }
            public string AppSubCategory { get; set; }
            public double MaxRatingPoints { get; set; }
        }


        public class FormChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string GeoStruct { get; set; }
            public string PayStruct { get; set; }
            public string FuncStruct { get; set; }
        }

        public ActionResult Formula_Grid(ParamModel param, string y)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var id = int.Parse(Session["CompId"].ToString());
                    var Sal = db.CompanyAppraisal.Where(e => e.Company.Id == id).Include(e => e.AppAssignment).Include(e => e.AppAssignment.Select(r => r.AppCategory)
                        ).Include(e => e.AppAssignment.Select(r => r.AppSubCategory)).SingleOrDefault();
                    var Sal1 = Sal.AppAssignment;
                    var all = Sal1.GroupBy(e => e.AppSubCategory).Select(e => e.FirstOrDefault()).ToList();
                    // for searchs
                    IEnumerable<AppAssignment> fall;
                    string DependRule = "";
                    if (param.sSearch == null)
                    {
                        fall = all;

                    }
                    else
                    {
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                            || (e.AppCategory.Name.ToUpper().Contains(param.sSearch.ToUpper()))
                            || (e.AppSubCategory.Name.ToString().Contains(param.sSearch))
                            || (e.MaxRatingPoints.ToString().Contains(param.sSearch))
                            ).ToList();

                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<AppAssignment, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.AppCategory.Name : "");
                    var sortcolumn = Request["sSortDir_0"];
                    if (sortcolumn == "asc")
                    {
                        fall = fall.OrderBy(orderfunc);
                    }
                    else
                    {
                        fall = fall.OrderByDescending(orderfunc);
                    }
                    // Paging 
                    var dcompanies = fall
                            .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompanies.Count == 0)
                    {
                        List<returndatagridclass> result = new List<returndatagridclass>();
                        foreach (var item in fall)
                        {
                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                AppCategory = item.AppCategory != null && item.AppCategory.Name != null ? item.AppCategory.Name : null,
                                AppSubCategory = item.AppSubCategory != null && item.AppSubCategory.Name != null ? item.AppSubCategory.Name : null,
                                MaxRatingPoints = item.MaxRatingPoints
                            });
                        }
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = from c in dcompanies

                                     select new[] { null, Convert.ToString(c.Id), c.AppCategory.Name };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
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

        public ActionResult Get_FormulaStructDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int id = Convert.ToInt32(Session["CompId"]);
                    var AppAssign = db.AppAssignment.Where(e => e.Id == data).SingleOrDefault();

                    var db_data = db.CompanyAppraisal
                        .Include(e => e.AppAssignment).Include(e => e.AppAssignment.Select(r => r.AppCategory))
                        .Include(e => e.AppAssignment.Select(r => r.AppSubCategory))
                        .Include(e => e.AppAssignment.Select(r => r.GeoStruct))
                        .Include(e => e.AppAssignment.Select(r => r.GeoStruct.Location))
                        .Include(e => e.AppAssignment.Select(r => r.GeoStruct.Department))
                        .Include(e => e.AppAssignment.Select(r => r.GeoStruct.Company))
                        .Include(e => e.AppAssignment.Select(r => r.GeoStruct.Corporate))
                        .Include(e => e.AppAssignment.Select(r => r.GeoStruct.Division))
                        .Include(e => e.AppAssignment.Select(r => r.GeoStruct.Group))
                        .Include(e => e.AppAssignment.Select(r => r.GeoStruct.Region))
                        .Include(e => e.AppAssignment.Select(r => r.GeoStruct.Unit))
                        .Include(e => e.AppAssignment.Select(r => r.PayStruct))
                        .Include(e => e.AppAssignment.Select(r => r.PayStruct.Grade))
                        .Include(e => e.AppAssignment.Select(r => r.PayStruct.Level))
                        .Include(e => e.AppAssignment.Select(r => r.PayStruct.Company))
                        .Include(e => e.AppAssignment.Select(r => r.PayStruct.JobStatus))
                        .Include(e => e.AppAssignment.Select(r => r.FuncStruct))
                        .Include(e => e.AppAssignment.Select(r => r.FuncStruct.Job))
                        .Include(e => e.AppAssignment.Select(r => r.FuncStruct.JobPosition))
                        .Include(e => e.AppAssignment.Select(r => r.FuncStruct.JobPosition))
                        .Include(e => e.AppAssignment.Select(r => r.FuncStruct.Company))
                        .Where(e => e.Id == id)
                       .ToList();



                    if (db_data != null)
                    {
                        List<FormChildDataClass> returndata = new List<FormChildDataClass>();

                        foreach (var a in db_data)
                        {
                            var SalFor = a.AppAssignment.Where(e => e.AppSubCategory.Id == AppAssign.AppSubCategory.Id);
                            foreach (var item in SalFor)
                            {
                                if (item.FuncStruct != null || item.PayStruct != null || item.GeoStruct != null)
                                {
                                    returndata.Add(new FormChildDataClass
                                    {
                                        Id = item.Id,
                                        GeoStruct = item.GeoStruct != null ? item.GeoStruct.FullDetails : "",
                                        FuncStruct = item.FuncStruct != null ? item.FuncStruct.FullDetails : "",
                                        PayStruct = item.PayStruct != null ? item.PayStruct.FullDetails : "",

                                    });
                                }
                            }
                        }
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
                    }

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
    }
}