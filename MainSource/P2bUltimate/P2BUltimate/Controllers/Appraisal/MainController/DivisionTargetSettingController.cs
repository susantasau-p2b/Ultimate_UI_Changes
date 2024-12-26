using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
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
using Appraisal;
using System.Web.Script.Serialization;

namespace P2BUltimate.Controllers.Appraisal.MainController
{
    public class DivisionTargetSettingController : Controller
    {
        //
        // GET: /DivisionTargetSetting/
        public ActionResult Index()
        {
            return View("~/Views/Appraisal/MainViews/DivisionTargetSetting/Index.cshtml");
        }

        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string StartPeriod { get; set; }
            public string EndPeriod { get; set; }
            
        }

        public ActionResult PopulateDropDownLocList(List<int> SkipIds)
        {
            //using (DataBaseContext db = new DataBaseContext())
            //{
            //    var qurey = db.Location.Include(e => e.LocationObj).ToList();
            //    var selected = (Object)null;
            //    if (data2 != "" && data != "0" && data2 != "0")
            //    {
            //        selected = Convert.ToInt32(data2);
            //    }

            //    SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
            //    return Json(s, JsonRequestBehavior.AllowGet);
            //}

            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.Location.Include(e => e.LocationObj).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Location.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PopulateDropDownCategoryList(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.BA_Category.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.BA_Category.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetDivisionDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fun_data = db.Division.Select(e => new
                {
                    code = e.Id.ToString(),
                    value = e.Code + " - " + e.Name.ToString() 

                }).ToList();
                return Json(fun_data, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetCategoryData(string Categoryids)
        {
            
            using (DataBaseContext db = new DataBaseContext())
            {
                List<int> id = Utility.StringIdsToListIds(Categoryids);
                var dates = new Dictionary<string, string>();
                foreach (int item in id)
                {
                    var CatData = db.BA_Category.Include(e => e.BA_SubCategory).Where(e => e.Id == item).FirstOrDefault();
                    
                    if (CatData != null)
                    {
                        if (CatData.BA_SubCategory.Count() > 0)
                        {
                            foreach (var item1 in CatData.BA_SubCategory)
                            {
                                dates.Add(CatData.Name, item1.Name);
                            }
                        }
                        else {
                            dates.Add(CatData.Name, "");
                        }
                    }
                }
                return Json(new { dates }, JsonRequestBehavior.AllowGet);
            }
        }

        public class FormChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string Category { get; set; }
            public string SubCategory { get; set; }
            public string Accounts { get; set; }
            public string Amount { get; set; }
            public string Compliance { get; set; }
        }


        public ActionResult Get_TargetSettingRequest(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int id = Convert.ToInt32(Session["CompId"]);
                    var GeoStructTar = db.BA_GeoStructTarget.Include(e => e.GeoStruct).Where(e => e.Id == data).SingleOrDefault();

                    var db_data = db.BA_GeoStructTarget
                      .Where(e => e.GeoStruct.Division_Id == GeoStructTar.GeoStruct.Division_Id)
                        .Include(e => e.GeoStruct)
                        .Include(e => e.GeoStruct.Division)
                        .Include(e => e.BA_Category)
                        .Include(e => e.BA_SubCategory)
                        .ToList();

                    db_data = db_data.GroupBy(e => e.BA_Category).Select(e => e.FirstOrDefault()).ToList();


                    if (db_data != null)
                    {
                        List<FormChildDataClass> returndata = new List<FormChildDataClass>();

                        
                        foreach (var item in db_data)
                        {
                            returndata.Add(new FormChildDataClass
                            {
                                Id = item.Id,
                                Category = item.BA_Category != null ? item.BA_Category.FullDetails : "",
                                SubCategory = item.BA_SubCategory != null ? item.BA_SubCategory.FullDetails : "",
                                Accounts = item.TargetAccounts.ToString(),
                                Amount = item.TargetAmount.ToString(),
                                Compliance = item.TargetCompliance.ToString(),
                            });
                        }
                        //}
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

        public class CategoryClass
        {
            public string SNo { get; set; }
            public string Category { get; set; }
            public string SubCategory { get; set; }
            public string Accounts{ get; set; }
            public string Amount { get; set; }
            public string Compliance { get; set; }
        }

        [HttpPost]
        public ActionResult ChkApplCategory(string CatName)
        {
            bool IsAccountAppl = false;
            bool IsAmountAppl = false;
            bool IsComplianceAppl = false;
            using (DataBaseContext db = new DataBaseContext())
            {
                BA_Category BA_Category = db.BA_Category.Where(e => e.Name == CatName).FirstOrDefault();
                if (BA_Category != null)
                {
                    IsAccountAppl = BA_Category.IsAccountAppl;
                    IsAmountAppl = BA_Category.IsAmountAppl;
                    IsComplianceAppl = BA_Category.IsComplianceAppl;

                }
            }
            return Json(new Object[] { IsAccountAppl, IsAmountAppl, IsComplianceAppl, JsonRequestBehavior.AllowGet });
        }

        public ActionResult createdata(List<CategoryClass> data, string FrequencyList, string Division, DateTime StartDate, DateTime EndDate)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> MSG = new List<string>();
                List<BA_GeoStructTarget> BA_GeoStructTargetList = new List<BA_GeoStructTarget>();

                
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                          new System.TimeSpan(0, 30, 0)))
                    {

                        try
                        {
                            
                            int FreId = int.Parse(FrequencyList);
                            


                            if (Division != null)
                            {
                                var ids = Utility.StringIdsToListIds(Division);
                                foreach (var item in ids)
                                {
                                    var geo = db.GeoStruct.Include(e => e.Division).Where(e => e.Division_Id == item).ToList();
                                    foreach (var item1 in geo)
                                    {
                                        BA_GeoStructTarget OGeoStructTarget = new BA_GeoStructTarget();
                                        OGeoStructTarget.GeoStruct = item1;
                                        OGeoStructTarget.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        OGeoStructTarget.TargetFrequency = db.LookupValue.Find(FreId);
                                        OGeoStructTarget.StartDate = StartDate;
                                        OGeoStructTarget.EndDate = EndDate;
                                        OGeoStructTarget.BA_Calendar = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).FirstOrDefault();
                                        foreach (var item2 in data)
                                        {
                                            OGeoStructTarget.BA_Category = db.BA_Category.Where(e => e.Name == item2.Category).FirstOrDefault();
                                            OGeoStructTarget.BA_SubCategory = db.BA_SubCategory.Where(e => e.Name == item2.SubCategory).FirstOrDefault();
                                            OGeoStructTarget.TargetAccounts = Convert.ToDouble(item2.Accounts);
                                            OGeoStructTarget.TargetAmount = Convert.ToDouble(item2.Amount);
                                            OGeoStructTarget.TargetCompliance = Convert.ToDouble(item2.Compliance);
                                            db.BA_GeoStructTarget.Add(OGeoStructTarget);
                                            db.SaveChanges();
                                            BA_GeoStructTargetList.Add(OGeoStructTarget);
                                        }
                                    }
                                }
                            }


                            try
                            {

                                List<BA_GeoStructTarget> OFAT = new List<BA_GeoStructTarget>();
                                OFAT.AddRange(BA_GeoStructTargetList);
                                int CompId =Convert.ToInt32(SessionManager.CompanyId);
                                var aa = db.CompanyAppraisal.Include(e => e.BA_GeoStructTarget).Include(e => e.Company)
                                    .Where(e => e.Company.Id == CompId).FirstOrDefault();
                                if (aa.BA_GeoStructTarget != null)
                                {
                                    OFAT.AddRange(aa.BA_GeoStructTarget);
                                }
                                aa.BA_GeoStructTarget = OFAT;
                                db.CompanyAppraisal.Attach(aa);
                                db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();

                            }
                            catch (Exception e)
                            {
                                MSG.Add(e.InnerException.Message.ToString());
                                return Json(MSG, JsonRequestBehavior.AllowGet);
                            }

                            MSG.Add("  Record Updated  ");
                            return Json(new { status = 0, MSG = MSG }, JsonRequestBehavior.AllowGet);


                        }
                        catch (Exception e)
                        {
                            MSG.Add(e.InnerException.Message.ToString());
                            return Json(MSG, JsonRequestBehavior.AllowGet);

                        }
                        return View();
                    }
               
            }
        }

        //[HttpPost]
        //public ActionResult Create(BA_GeoStructTarget c, FormCollection form) //Create submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            string Frequency = form["Frequencylist"] == "0" ? "" : form["Frequencylist"];
        //            var TableData = form["TableData"] == "0" ? "" : form["TableData"];
        //            string Division = form["DivisionT-table"] == "0" ? "" : form["DivisionT-table"];
                   
        //            var Serialize = new JavaScriptSerializer();
        //        var deserialize = Serialize.Deserialize<CategoryClass>(TableData);

        //            int CompId = 0;
        //            if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
        //            {
        //                CompId = int.Parse(Session["CompId"].ToString());
        //            }

        //            if (Frequency != "" && Frequency != null)
        //            {
                        
        //            }

        //            List<CategoryClass> OCatClass = new List<CategoryClass>();
        //            if (deserialize != null)
        //            {
                        
        //            }

                  

        //            //List<CategoryClass> data =List<CategoryClass> (TableData);
        //            //RoasterClass item in data

        //            CompanyAppraisal OCompanyAppraisal = null;
        //            List<AppAssignment> OFAT = new List<AppAssignment>();
        //            OCompanyAppraisal = db.CompanyAppraisal.Where(e => e.Company.Id == CompId).SingleOrDefault();

                     


        //            if (ModelState.IsValid)
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
        //                                  new System.TimeSpan(0, 30, 0)))
        //                {
        //                    //if (db.AppAssignment.Any(o => o.AppraisalCalendar.Id == c.AppraisalCalendar.Id && o.AppCategory.Id == c.AppCategory.Id && o.AppSubCategory.Id == c.AppSubCategory.Id))
        //                    //{
        //                    //    Msg.Add("Record Already Exists.");
        //                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //}

        //                    try
        //                    {
        //                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

        //                          if (Division != "" && Division != null)
        //            {
        //                var ids = Utility.StringIdsToListIds(Division);
        //                foreach (var item in ids)
        //                {
        //                    var geo = db.GeoStruct.Include(e => e.Division).Where(e => e.Division_Id == item).FirstOrDefault();
        //                    c.GeoStruct = geo;
        //                    foreach (var item1 in OCatClass)
        //                    {
        //                       // c.BA_Category = item1.Category;
        //                    }
        //                }
        //            }

        //                        //if (geo_id != null && geo_id != "")
        //                        //{
        //                        //    var ids = Utility.StringIdsToListIds(geo_id);

        //                        //    foreach (var G in ids)
        //                        //    {
        //                        //        var geo = db.GeoStruct.Find(G);
        //                        //        c.GeoStruct = geo;

        //                        //        c.FuncStruct = null;
        //                        //        c.PayStruct = null;

        //                        //        c.AppRatingObjective = null;
        //                        //        List<AppRatingObjective> cp = new List<AppRatingObjective>();
        //                        //        if (AppRatingObjective != null && AppRatingObjective != "")
        //                        //        {
        //                        //            var ids1 = Utility.StringIdsToListIds(AppRatingObjective);
        //                        //            foreach (var ca in ids1)
        //                        //            {
        //                        //                var p_val = db.AppRatingObjective.Find(ca);
        //                        //                cp.Add(p_val);
        //                        //                c.AppRatingObjective = cp;

        //                        //            }
        //                        //        }

        //                        //        AppAssignment AppAssigngeo = new AppAssignment()
        //                        //        {
        //                        //            MaxRatingPoints = c.MaxRatingPoints == null ? 0 : c.MaxRatingPoints,
        //                        //            AppraisalCalendar = c.AppraisalCalendar,
        //                        //            AppSubCategory = c.AppSubCategory,
        //                        //            AppRatingObjective = c.AppRatingObjective,
        //                        //            AppCategory = c.AppCategory,
        //                        //            GeoStruct = c.GeoStruct,
        //                        //            FuncStruct = c.FuncStruct,
        //                        //            PayStruct = c.PayStruct,
        //                        //            DBTrack = c.DBTrack
        //                        //        };

        //                        //        db.AppAssignment.Add(AppAssigngeo);
        //                        //        db.SaveChanges();

        //                        //        OFAT.Add(db.AppAssignment.Find(AppAssigngeo.Id));
        //                        //    }

        //                        //}

        //                        //if (fun_id != null && fun_id != "")
        //                        //{

        //                        //    var ids = Utility.StringIdsToListIds(fun_id);
        //                        //    foreach (var F in ids)
        //                        //    {
        //                        //        var fun = db.FuncStruct.Find(F);
        //                        //        c.FuncStruct = fun;

        //                        //        c.GeoStruct = null;
        //                        //        c.PayStruct = null;

        //                        //        c.AppRatingObjective = null;
        //                        //        List<AppRatingObjective> cp = new List<AppRatingObjective>();
        //                        //        if (AppRatingObjective != null && AppRatingObjective != "")
        //                        //        {
        //                        //            var ids1 = Utility.StringIdsToListIds(AppRatingObjective);
        //                        //            foreach (var ca in ids1)
        //                        //            {
        //                        //                var p_val = db.AppRatingObjective.Find(ca);
        //                        //                cp.Add(p_val);
        //                        //                c.AppRatingObjective = cp;

        //                        //            }
        //                        //        }

        //                        //        AppAssignment AppAssignfun = new AppAssignment()
        //                        //        {
        //                        //            MaxRatingPoints = c.MaxRatingPoints == null ? 0 : c.MaxRatingPoints,
        //                        //            AppraisalCalendar = c.AppraisalCalendar,
        //                        //            AppSubCategory = c.AppSubCategory,
        //                        //            AppRatingObjective = c.AppRatingObjective,
        //                        //            AppCategory = c.AppCategory,
        //                        //            GeoStruct = c.GeoStruct,
        //                        //            FuncStruct = c.FuncStruct,
        //                        //            PayStruct = c.PayStruct,
        //                        //            DBTrack = c.DBTrack
        //                        //        };

        //                        //        db.AppAssignment.Add(AppAssignfun);
        //                        //        db.SaveChanges();

        //                        //        OFAT.Add(db.AppAssignment.Find(AppAssignfun.Id));
        //                        //    }
        //                        //}

        //                        //if (pay_id != null && pay_id != "")
        //                        //{
        //                        //    var ids = Utility.StringIdsToListIds(pay_id);

        //                        //    foreach (int P in ids.ToList())
        //                        //    {
        //                        //        var pay = db.PayStruct.Find(P);
        //                        //        c.PayStruct = pay;

        //                        //        c.GeoStruct = null;
        //                        //        c.FuncStruct = null;

        //                        //        c.AppRatingObjective = null;
        //                        //        List<AppRatingObjective> cp = new List<AppRatingObjective>();
        //                        //        if (AppRatingObjective != null && AppRatingObjective != "")
        //                        //        {
        //                        //            var ids1 = Utility.StringIdsToListIds(AppRatingObjective);
        //                        //            foreach (var ca in ids1)
        //                        //            {
        //                        //                var p_val = db.AppRatingObjective.Find(ca);
        //                        //                cp.Add(p_val);
        //                        //                c.AppRatingObjective = cp;

        //                        //            }
        //                        //        }

        //                        //        AppAssignment AppAssignPay = new AppAssignment()
        //                        //        {
        //                        //            MaxRatingPoints = c.MaxRatingPoints,
        //                        //            AppraisalCalendar = c.AppraisalCalendar,
        //                        //            AppSubCategory = c.AppSubCategory,
        //                        //            AppRatingObjective = c.AppRatingObjective,
        //                        //            AppCategory = c.AppCategory,
        //                        //            GeoStruct = c.GeoStruct,
        //                        //            FuncStruct = c.FuncStruct,
        //                        //            PayStruct = c.PayStruct,
        //                        //            DBTrack = c.DBTrack
        //                        //        };

        //                        //        db.AppAssignment.Add(AppAssignPay);
        //                        //        db.SaveChanges();

        //                        //        OFAT.Add(db.AppAssignment.Find(AppAssignPay.Id));
        //                        //    }
        //                        //}



        //                        //if (OCompanyAppraisal == null)
        //                        //{
        //                        //    CompanyAppraisal OTEP = new CompanyAppraisal()
        //                        //    {
        //                        //        Company = db.Company.Find(SessionManager.CompanyId),
        //                        //        AppAssignment = OFAT,
        //                        //        DBTrack = c.DBTrack

        //                        //    };


        //                        //    db.CompanyAppraisal.Add(OTEP);
        //                        //    db.SaveChanges();
        //                        //}
        //                        //else
        //                        //{
        //                        //    var aa = db.CompanyAppraisal.Find(OCompanyAppraisal.Id);
        //                        //    if (aa.AppAssignment != null)
        //                        //        OFAT.AddRange(aa.AppAssignment);
        //                        //    aa.AppAssignment = OFAT;
        //                        //    db.CompanyAppraisal.Attach(aa);
        //                        //    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
        //                        //    db.SaveChanges();
        //                        //    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

        //                        //}

        //                        ts.Complete();
        //                        Msg.Add("  Data Saved successfully  ");
        //                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
        //                    }
        //                    catch (DataException /* dex */)
        //                    {
        //                        //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
        //                        //ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
        //                        //return View(level);
        //                        Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                StringBuilder sb = new StringBuilder("");
        //                foreach (ModelState modelState in ModelState.Values)
        //                {
        //                    foreach (ModelError error in modelState.Errors)
        //                    {
        //                        sb.Append(error.ErrorMessage);
        //                        sb.Append("." + "\n");
        //                    }
        //                }
        //                var errorMsg = sb.ToString();
        //                Msg.Add(errorMsg);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //return this.Json(new { msg = errorMsg });
        //            }
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
        //        //  return View();
        //    }
        //}

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var id = int.Parse(SessionManager.CompApprId);
                    List<BA_GeoStructTarget> BA_GeoStructTarget = db.BA_GeoStructTarget.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Division)
                        .Where(e => e.GeoStruct.Division_Id != null).OrderBy(e => e.Id).AsNoTracking().AsParallel().ToList();

                    var all = BA_GeoStructTarget.GroupBy(e => e.GeoStruct.Division_Id).Select(e => e.FirstOrDefault()).ToList();

                    //var all = db.BA_GeoStructTarget.Include(e => e.TargetFrequency)
                    //    .Include(e => e.GeoStruct)
                    //    .Include(e => e.GeoStruct.Division)
                    //    .ToList();
                    // for searchs
                    IEnumerable<BA_GeoStructTarget> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        // fall = all.Where(e => e.Employee.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.GeoStruct.Division.Code.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                  || (e.GeoStruct.Division.Name.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  || (e.StartDate.Value.ToString("dd/MM/yyyy").Contains(param.sSearch))
                                  || (e.EndDate.Value.ToString("dd/MM/yyyy").Contains(param.sSearch))
                            // || (e.Employee.PayStruct.Grade.Name.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                            //  || (e.Employee.GeoStruct.Location.LocationObj.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<BA_GeoStructTarget, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.GeoStruct.Division.Code : "");
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
                                Code = item.GeoStruct != null && item.GeoStruct.Division != null ? item.GeoStruct.Division.Code : null,
                                Name = item.GeoStruct != null && item.GeoStruct.Division != null ? item.GeoStruct.Division.Name : null,
                                StartPeriod = item.StartDate != null ? item.StartDate.Value.ToString("dd/MM/yyyy") : null,
                                EndPeriod = item.EndDate != null ? item.EndDate.Value.ToString("dd/MM/yyyy") : null,
                                
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

                                     select new[] { null, Convert.ToString(c.Id), c.GeoStruct.Division.Code, };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }//for data reterivation
                }
                catch (Exception e)
                {
                    List<string> Msg = new List<string>();
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

       
	}
}