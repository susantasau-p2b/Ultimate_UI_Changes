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

namespace P2BUltimate.Controllers.Appraisal.MainController
{
    public class EmployeeTargetSettingController : Controller
    {
        //
        // GET: /DivisionTargetSetting/
        public ActionResult Index()
        {
            HttpContext.Session["GridData"] = null;
            return View("~/Views/Appraisal/MainViews/EmployeeTargetSetting/Index.cshtml");
        }
        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Appraisal/_EmpTargetSettingPartial.cshtml");
        }
       
        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string StartPeriod { get; set; }
            public string EndPeriod { get; set; }

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
        public ActionResult CategoryDrop(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = (SelectList)null;
                var selected = "";
                //var id = Convert.ToInt32(data);
                var qurey = db.BA_Category.ToList(); // added by rekha 26-12-16
                if (data2 != "" && data2 != "0")
                {
                    selected = data2;
                }
                if (qurey != null)
                {
                    s = new SelectList(qurey, "Id", "FullDetails", selected);
                }
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult SubCategoryDrop(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = (SelectList)null;
                var selected = "";
                //var id = Convert.ToInt32(data);
                var qurey = db.BA_SubCategory.ToList(); // added by rekha 26-12-16
                if (data2 != "" && data2 != "0")
                {
                    selected = data2;
                }
                if (qurey != null)
                {
                    s = new SelectList(qurey, "Id", "FullDetails", selected);
                }
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public class returnDataClass
        {


            public string Category { get; set; }
            public string SubCategory { get; set; }
            public double TargetAccounts { get; set; }
            public double TargetAmount { get; set; }
            public double TargetCompliance { get; set; }

        }
        public ActionResult GridEditData(int data)
        {
            var returnlist = new List<returnDataClass>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != null && data != 0)
                {
                    var retrundataList = db.BA_EmpTarget.Where(e => e.Id == data).Include(e => e.BA_Category).Include(e => e.BA_SubCategory).ToList();
                    foreach (var a in retrundataList)
                    {
                        returnlist.Add(new returnDataClass()
                        {
                            Category = a.BA_Category != null ? a.BA_Category.Id.ToString() : null,
                            SubCategory = a.BA_SubCategory != null ? a.BA_SubCategory.Id.ToString() : null,
                            TargetAccounts = a.TargetAccounts,
                            TargetAmount = a.TargetAmount,
                            TargetCompliance = a.TargetCompliance

                            
                        });
                    }
                    return Json(new { returndata = returnlist }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View();
                }
            }
        }
        public ActionResult GridEditSave(BA_EmpTarget BA_EmpTarget, FormCollection form, string data)
        {
            List<string> Msgs = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
                var BA_EmpTargetData = db.BA_EmpTarget.Where(e => e.Id == Id).SingleOrDefault();
                BA_EmpTargetData.TargetAccounts = BA_EmpTarget.TargetAccounts;
                BA_EmpTargetData.TargetAmount = BA_EmpTarget.TargetAmount;
                BA_EmpTargetData.TargetCompliance = BA_EmpTarget.TargetCompliance;


                string Category = form["Category"] == "0" ? "" : form["Category"];
                if (Category != "0")
                {
                    var id = Convert.ToInt32(Category);
                    var Categorydata = db.BA_Category.Where(e => e.Id == id).SingleOrDefault();
                    BA_EmpTargetData.BA_Category = Categorydata;
                }
                string SubCategory = form["Subcategory"] == "0" ? "" : form["Subcategory"];
                if (SubCategory != "0")
                {
                    var id = Convert.ToInt32(SubCategory);
                    var SubCategorydata = db.BA_SubCategory.Where(e => e.Id == id).SingleOrDefault();
                    BA_EmpTargetData.BA_SubCategory = SubCategorydata;
                }
                using (TransactionScope ts = new TransactionScope())
                {

                    BA_EmpTargetData.DBTrack = new DBTrack
                    {
                        CreatedBy = BA_EmpTargetData.DBTrack.CreatedBy == null ? null : BA_EmpTargetData.DBTrack.CreatedBy,
                        CreatedOn = BA_EmpTargetData.DBTrack.CreatedOn == null ? null : BA_EmpTargetData.DBTrack.CreatedOn,
                        Action = "M",
                        ModifiedBy = SessionManager.UserName,
                        ModifiedOn = DateTime.Now
                    };
                    try
                    {
                        db.BA_EmpTarget.Attach(BA_EmpTargetData);
                        db.Entry(BA_EmpTargetData).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(BA_EmpTargetData).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return this.Json(new { status = true, responseText = "Data Updated Successfully.", JsonRequestBehavior.AllowGet });
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
                        List<string> Msg = new List<string>();
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }
        public ActionResult Get_TargetSettingRequest(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int id = Convert.ToInt32(Session["CompId"]);
                    var GeoStructTar =db.BA_EmpTarget.Where(e => e.Id == data).SingleOrDefault();

                    var db_data = db.BA_EmpTarget
                      .Where(e => e.EmployeeAppraisal_Id == GeoStructTar.EmployeeAppraisal_Id)
                        .Include(e => e.BA_Category)
                        .Include(e => e.BA_SubCategory)
                        .ToList();

                   // db_data = db_data.GroupBy(e => e.BA_Category).Select(e => e.FirstOrDefault()).ToList();


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

        public ActionResult Get_SubCategory(string cat_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                int CatId = int.Parse(cat_id);
                var CatList = db.BA_Category.Where(e => e.Id == CatId).Include(e => e.BA_SubCategory).FirstOrDefault();
                foreach (var item in CatList.BA_SubCategory)
	            {
		            returndata.Add(new Utility.returndataclass
                    {
                        code = item.Id.ToString(),
                        value = item.FullDetails,
                    });
                }

                if (returndata != null && returndata.Count() > 0)
                {
                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "subcategory-table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult PopulateCategoryList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.BA_Category.ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
               
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetDivisionDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //   List<Employee> data = new List<Employee>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                db.Database.CommandTimeout = 3600;
                var empdata = db.CompanyPayroll.Where(e => e.Company.Id == compid)
                  .Include(e => e.EmployeePayroll)
                  .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                  .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                  .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates)).OrderBy(e => e.Id)
                  .AsNoTracking().AsParallel()
                  .SingleOrDefault();
                var emp = empdata.EmployeePayroll.Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null).Select(e => e.Employee).AsParallel().ToList();
                var fun_data = emp.Select(e => new
                {
                    code = e.Id.ToString(),
                    value = e.FullDetails.ToString()

                }).ToList();

                //var fun_data = db.Employee.Select(e => new
                //{
                //    code = e.Id.ToString(),
                //    value = e.FullDetails + " - " + e.FullDetails.ToString() 

                //}).ToList();
                return Json(fun_data, JsonRequestBehavior.AllowGet);
            }

        }
        public class CategoryClass
        {
            public string SNo { get; set; }
            public string Category { get; set; }
            public string SubCategory { get; set; }
            public string Accounts { get; set; }
            public string Amount { get; set; }
            public string Compliance { get; set; }
            public string CustomerId { get; set; }
            public string AccountNo { get; set; }
        }

        public ActionResult createdata(List<CategoryClass> data, string FrequencyList, string Division, DateTime StartDate, DateTime EndDate)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> MSG = new List<string>();
                List<BA_EmpTarget> BA_GeoStructTargetList = new List<BA_EmpTarget>();
                int count = 0;
                foreach (var item2 in data)
                {
                    if (item2.Accounts == "0" && item2.Amount == "0" && item2.Compliance == "0")
                    {
                        count = count + 1;
                    }
                }

                if (count == data.Count())
                {
                     MSG.Add("Kindly update the records. ");
                     return Json(new { status = 0, MSG = MSG }, JsonRequestBehavior.AllowGet);
                }

                try
                {
                   
                    int FreId = int.Parse(FrequencyList);
                   

                    if (Division != null)
                    {
                        var ids = Utility.StringIdsToListIds(Division);
                        foreach (var item in ids)
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                new System.TimeSpan(0, 30, 0)))
                            {
                                BA_EmpTarget OGeoStructTarget = new BA_EmpTarget();
                                //  var geo = db.GeoStruct.Include(e => e.Division).Where(e => e.Division_Id == item).ToList();
                                var geo = db.EmployeeAppraisal.Include(e => e.Employee).Where(e => e.Employee.Id == item).FirstOrDefault();
                              //  var geo = db.EmployeeAppraisal.Where(e => e.Employee.Id == item).FirstOrDefault();

                                var BA_CalendarVal = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).FirstOrDefault();

                                if (BA_CalendarVal == null)
                                {
                                    MSG.Add("  Please Define Appraisal Calendar.   ");
                                    return Json(new { status = false, MSG = MSG }, JsonRequestBehavior.AllowGet);
                                }

                                foreach (var item2 in data)
                                {
                                    if (item2.Accounts != "0" || item2.Amount != "0"  || item2.Compliance != "0")
                                    {
                                        BA_EmpTarget OBA_EmpTarget = db.BA_EmpTarget.Where(e => e.EmployeeAppraisal_Id == geo.Id && e.BA_Category.Name == item2.Category
                                            && e.BA_SubCategory.Name == item2.SubCategory && e.StartDate == StartDate
                                            && e.EndDate == EndDate).FirstOrDefault();
                                        if (OBA_EmpTarget == null)
                                        {
                                            OGeoStructTarget.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                            OGeoStructTarget.TargetFrequency = db.LookupValue.Find(FreId);
                                            OGeoStructTarget.StartDate = StartDate;
                                            OGeoStructTarget.EndDate = EndDate;

                                            OGeoStructTarget.BA_Category = db.BA_Category.Where(e => e.Name == item2.Category).FirstOrDefault();
                                            OGeoStructTarget.BA_SubCategory = db.BA_SubCategory.Where(e => e.Name == item2.SubCategory).FirstOrDefault();
                                            OGeoStructTarget.TargetAccounts = Convert.ToDouble(item2.Accounts);
                                            OGeoStructTarget.TargetAmount = Convert.ToDouble(item2.Amount);
                                            OGeoStructTarget.TargetCompliance = Convert.ToDouble(item2.Compliance);

                                            OGeoStructTarget.BA_Calendar = BA_CalendarVal != null ? BA_CalendarVal : null;

                                            db.BA_EmpTarget.Add(OGeoStructTarget);
                                            db.SaveChanges();
                                            List<BA_EmpTarget> OFAT = new List<BA_EmpTarget>();
                                            OFAT.Add(OGeoStructTarget);
                                            int CompId = Convert.ToInt32(SessionManager.CompanyId);
                                            var aa = db.EmployeeAppraisal.Include(e => e.BA_EmpTarget)
                                                .Where(e => e.Id == geo.Id).FirstOrDefault();
                                            if (aa.BA_EmpTarget != null)
                                            {
                                                OFAT.AddRange(aa.BA_EmpTarget);
                                            }
                                            aa.BA_EmpTarget = OFAT;
                                            db.EmployeeAppraisal.Attach(aa);
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();  
                                        }
                                    }
                                    

                                }
                                ts.Complete();
                                //}
                            }
                        }



                        HttpContext.Session["GridData"] = null;
                        MSG.Add("  Record Updated  ");
                        return Json(new { status = 1, MSG = MSG }, JsonRequestBehavior.AllowGet);


                    }
                }
                catch (Exception e)
                {
                    MSG.Add(e.InnerException.Message.ToString());
                    return Json(MSG, JsonRequestBehavior.AllowGet);

                }
                return View();
            }



        }

        public ActionResult GetCategoryData(string Categoryids, string SubCategoryids)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                List<int> id = Utility.StringIdsToListIds(SubCategoryids);
                var data = HttpContext.Session["GridData"] as List<FormChildDataClass>;
                int CatId = Convert.ToInt32(Categoryids);
                
                List<FormChildDataClass> returndata = new List<FormChildDataClass>();
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        returndata.Add(new FormChildDataClass
                        {
                            Id = item.Id,
                            Category = item.Category,
                            SubCategory = item.SubCategory,
                        });

                    }
                }

                var CatData = db.BA_Category.Include(e => e.BA_SubCategory).Where(e => e.Id == CatId).FirstOrDefault();
                foreach (int item in id)
                {
                    if (CatData != null)
                    {
                        var SubCat = CatData.BA_SubCategory.Where(e => e.Id == item).FirstOrDefault();
                        if (SubCat != null)
                        {
                            returndata.Add(new FormChildDataClass
                            {
                                Id = item,
                                Category = CatData.Name,
                                SubCategory = SubCat.Name,
                            });
                        }
                        else
                        {
                            returndata.Add(new FormChildDataClass
                            {
                                Id = item,
                                Category = CatData.Name,
                                SubCategory = "",
                            });
                        }
                       
                    }
                }
                HttpContext.Session["GridData"] = returndata;
                return Json(new { returndata }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Create(BA_EmpTarget c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Frequency = form["Frequencylist"] == "0" ? "" : form["Frequencylist"];
                    string TableData = form["TableData"] == "0" ? "" : form["TableData"];
                    string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];


                    int CompId = 0;
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = int.Parse(Session["CompId"].ToString());
                    }

                    if (Frequency != "" && Frequency != null)
                    {

                    }

                    if (TableData != "" && TableData != null)
                    {

                    }
                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = Utility.StringIdsToListIds(Emp);
                    }
                    else
                    {
                        Msg.Add(" Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new { success = false, responseText = "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                    }

                    CompanyAppraisal OCompanyAppraisal = null;
                    List<AppAssignment> OFAT = new List<AppAssignment>();
                    OCompanyAppraisal = db.CompanyAppraisal.Where(e => e.Company.Id == CompId).SingleOrDefault();




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

                                //if (geo_id != null && geo_id != "")
                                //{
                                //    var ids = Utility.StringIdsToListIds(geo_id);

                                //    foreach (var G in ids)
                                //    {
                                //        var geo = db.GeoStruct.Find(G);
                                //        c.GeoStruct = geo;

                                //        c.FuncStruct = null;
                                //        c.PayStruct = null;

                                //        c.AppRatingObjective = null;
                                //        List<AppRatingObjective> cp = new List<AppRatingObjective>();
                                //        if (AppRatingObjective != null && AppRatingObjective != "")
                                //        {
                                //            var ids1 = Utility.StringIdsToListIds(AppRatingObjective);
                                //            foreach (var ca in ids1)
                                //            {
                                //                var p_val = db.AppRatingObjective.Find(ca);
                                //                cp.Add(p_val);
                                //                c.AppRatingObjective = cp;

                                //            }
                                //        }

                                //        AppAssignment AppAssigngeo = new AppAssignment()
                                //        {
                                //            MaxRatingPoints = c.MaxRatingPoints == null ? 0 : c.MaxRatingPoints,
                                //            AppraisalCalendar = c.AppraisalCalendar,
                                //            AppSubCategory = c.AppSubCategory,
                                //            AppRatingObjective = c.AppRatingObjective,
                                //            AppCategory = c.AppCategory,
                                //            GeoStruct = c.GeoStruct,
                                //            FuncStruct = c.FuncStruct,
                                //            PayStruct = c.PayStruct,
                                //            DBTrack = c.DBTrack
                                //        };

                                //        db.AppAssignment.Add(AppAssigngeo);
                                //        db.SaveChanges();

                                //        OFAT.Add(db.AppAssignment.Find(AppAssigngeo.Id));
                                //    }

                                //}

                                //if (fun_id != null && fun_id != "")
                                //{

                                //    var ids = Utility.StringIdsToListIds(fun_id);
                                //    foreach (var F in ids)
                                //    {
                                //        var fun = db.FuncStruct.Find(F);
                                //        c.FuncStruct = fun;

                                //        c.GeoStruct = null;
                                //        c.PayStruct = null;

                                //        c.AppRatingObjective = null;
                                //        List<AppRatingObjective> cp = new List<AppRatingObjective>();
                                //        if (AppRatingObjective != null && AppRatingObjective != "")
                                //        {
                                //            var ids1 = Utility.StringIdsToListIds(AppRatingObjective);
                                //            foreach (var ca in ids1)
                                //            {
                                //                var p_val = db.AppRatingObjective.Find(ca);
                                //                cp.Add(p_val);
                                //                c.AppRatingObjective = cp;

                                //            }
                                //        }

                                //        AppAssignment AppAssignfun = new AppAssignment()
                                //        {
                                //            MaxRatingPoints = c.MaxRatingPoints == null ? 0 : c.MaxRatingPoints,
                                //            AppraisalCalendar = c.AppraisalCalendar,
                                //            AppSubCategory = c.AppSubCategory,
                                //            AppRatingObjective = c.AppRatingObjective,
                                //            AppCategory = c.AppCategory,
                                //            GeoStruct = c.GeoStruct,
                                //            FuncStruct = c.FuncStruct,
                                //            PayStruct = c.PayStruct,
                                //            DBTrack = c.DBTrack
                                //        };

                                //        db.AppAssignment.Add(AppAssignfun);
                                //        db.SaveChanges();

                                //        OFAT.Add(db.AppAssignment.Find(AppAssignfun.Id));
                                //    }
                                //}

                                //if (pay_id != null && pay_id != "")
                                //{
                                //    var ids = Utility.StringIdsToListIds(pay_id);

                                //    foreach (int P in ids.ToList())
                                //    {
                                //        var pay = db.PayStruct.Find(P);
                                //        c.PayStruct = pay;

                                //        c.GeoStruct = null;
                                //        c.FuncStruct = null;

                                //        c.AppRatingObjective = null;
                                //        List<AppRatingObjective> cp = new List<AppRatingObjective>();
                                //        if (AppRatingObjective != null && AppRatingObjective != "")
                                //        {
                                //            var ids1 = Utility.StringIdsToListIds(AppRatingObjective);
                                //            foreach (var ca in ids1)
                                //            {
                                //                var p_val = db.AppRatingObjective.Find(ca);
                                //                cp.Add(p_val);
                                //                c.AppRatingObjective = cp;

                                //            }
                                //        }

                                //        AppAssignment AppAssignPay = new AppAssignment()
                                //        {
                                //            MaxRatingPoints = c.MaxRatingPoints,
                                //            AppraisalCalendar = c.AppraisalCalendar,
                                //            AppSubCategory = c.AppSubCategory,
                                //            AppRatingObjective = c.AppRatingObjective,
                                //            AppCategory = c.AppCategory,
                                //            GeoStruct = c.GeoStruct,
                                //            FuncStruct = c.FuncStruct,
                                //            PayStruct = c.PayStruct,
                                //            DBTrack = c.DBTrack
                                //        };

                                //        db.AppAssignment.Add(AppAssignPay);
                                //        db.SaveChanges();

                                //        OFAT.Add(db.AppAssignment.Find(AppAssignPay.Id));
                                //    }
                                //}



                                //if (OCompanyAppraisal == null)
                                //{
                                //    CompanyAppraisal OTEP = new CompanyAppraisal()
                                //    {
                                //        Company = db.Company.Find(SessionManager.CompanyId),
                                //        AppAssignment = OFAT,
                                //        DBTrack = c.DBTrack

                                //    };


                                //    db.CompanyAppraisal.Add(OTEP);
                                //    db.SaveChanges();
                                //}
                                //else
                                //{
                                //    var aa = db.CompanyAppraisal.Find(OCompanyAppraisal.Id);
                                //    if (aa.AppAssignment != null)
                                //        OFAT.AddRange(aa.AppAssignment);
                                //    aa.AppAssignment = OFAT;
                                //    db.CompanyAppraisal.Attach(aa);
                                //    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                //    db.SaveChanges();
                                //    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                //}

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

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var id = int.Parse(SessionManager.CompApprId);
                    //List<BA_GeoStructTarget> BA_GeoStructTarget = db.BA_GeoStructTarget.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Division)
                    //    .Where(e => e.GeoStruct.Division_Id != null).OrderBy(e => e.Id).AsNoTracking().AsParallel().ToList();

                    //var all = BA_GeoStructTarget.GroupBy(e => e.GeoStruct.Division_Id).Select(e => e.FirstOrDefault()).ToList();

                    List<BA_EmpTarget> BA_GeoStructTarget = db.BA_EmpTarget.Include(e=>e.EmployeeAppraisal)
                        .Include(e => e.EmployeeAppraisal.Employee)
                        .Include(e => e.EmployeeAppraisal.Employee.EmpName)
                        .Where(e => e.EmployeeAppraisal_Id != null).OrderBy(e => e.Id).AsNoTracking().AsParallel().ToList();

                    var all = BA_GeoStructTarget.GroupBy(e => e.EmployeeAppraisal_Id).Select(e => e.FirstOrDefault()).ToList();


                    IEnumerable<BA_EmpTarget> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        // fall = all.Where(e => e.Employee.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.EmployeeAppraisal.Employee.EmpCode.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                  || (e.EmployeeAppraisal.Employee.EmpName.FullNameFML.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  || (e.StartDate.Value.ToString("dd/MM/yyyy").Contains(param.sSearch))
                                  || (e.EndDate.Value.ToString("dd/MM/yyyy").Contains(param.sSearch))
                            // || (e.Employee.PayStruct.Grade.Name.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                            //  || (e.Employee.GeoStruct.Location.LocationObj.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<BA_EmpTarget, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.EmployeeAppraisal.Employee.EmpCode : "");
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
                                Code = item.EmployeeAppraisal.Employee != null && item.EmployeeAppraisal.Employee.EmpCode != null ? item.EmployeeAppraisal.Employee.EmpCode : null,
                                Name = item.EmployeeAppraisal.Employee != null && item.EmployeeAppraisal.Employee.EmpName != null ? item.EmployeeAppraisal.Employee.EmpName.FullNameFML : null,
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

                                     select new[] { null, Convert.ToString(c.Id), c.EmployeeAppraisal.Employee.EmpCode, };
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