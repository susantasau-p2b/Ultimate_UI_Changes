using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Appraisal;
using System.Transactions;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Appraisal.MainController
{
    public class DepartmentwiseTargetController : Controller
    {
        //
        // GET: /DepartmentwiseTarget/
        public ActionResult Index()
        {
            return View("~/Views/Appraisal/MainViews/DepartmentwiseTarget/Index.cshtml");
        }
        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string StartPeriod { get; set; }
            public string EndPeriod { get; set; }

        }

        public ActionResult FindLocationlist()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Location.Select(r => new
                {
                    Id = r.Id,
                    FullDetails = r.FullDetails
                }).ToList();
                var result = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public class GetCodeName
        {
            public string code { get; set; }
            public string value { get; set; }
        }
        public ActionResult GetDepartmentDetails(string data, string locids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<GetCodeName> returndata = new List<GetCodeName>();
                int locid = Convert.ToInt32(locids);
                var fun_data = db.Location.Where(e => e.Id == locid).Select(e => new
                {
                    department = e.Department.Select(f => new
                    {
                        departmentId = f.Id,
                        DepCode = f.DepartmentObj.DeptCode,
                        DepDesc = f.DepartmentObj.DeptDesc
                    }).ToList()
                }).SingleOrDefault();

                if (fun_data != null)
                {
                    foreach (var item in fun_data.department)
                    {
                        returndata.Add(new GetCodeName
                        {
                            code = item.departmentId.ToString(),
                            value = item.DepCode + " - " + item.DepDesc
                        });
                    }
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult PopulateDropDownCategoryList(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
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
                        else
                        {
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

        public ActionResult createdata(List<CategoryClass> data, string FrequencyList, string Department, DateTime StartDate, DateTime EndDate)
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

                        if (!string.IsNullOrEmpty(Department))
                        {
                            var ids = Utility.StringIdsToListIds(Department);
                            foreach (var item in ids)
                            {
                                var geo = db.GeoStruct.Include(e => e.Department).Where(e => e.Department_Id == item).ToList();
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
                            int CompId = Convert.ToInt32(SessionManager.CompanyId);
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

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var id = int.Parse(SessionManager.CompApprId);
                    List<BA_GeoStructTarget> BA_GeoStructTarget = db.BA_GeoStructTarget
                        .Include(e => e.GeoStruct)
                        .Include(e => e.GeoStruct.Department)
                        .Include(e => e.GeoStruct.Department.DepartmentObj)
                        .Where(e => e.GeoStruct.Department_Id != null).OrderBy(e => e.Id).AsNoTracking().AsParallel().ToList();

                    var all = BA_GeoStructTarget.GroupBy(e => e.GeoStruct.Department_Id).Select(e => e.FirstOrDefault()).ToList();
                    IEnumerable<BA_GeoStructTarget> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {

                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.GeoStruct.Department.DepartmentObj.DeptCode.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                  || (e.GeoStruct.Department.DepartmentObj.DeptDesc.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  || (e.StartDate.Value.ToString("dd/MM/yyyy").Contains(param.sSearch))
                                  || (e.EndDate.Value.ToString("dd/MM/yyyy").Contains(param.sSearch))

                                  ).ToList();
                    }


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
                                Code = item.GeoStruct == null ? "" : item.GeoStruct.Department == null ? "" : item.GeoStruct.Department.DepartmentObj == null ? "" : item.GeoStruct.Department.DepartmentObj.DeptCode,
                                Name = item.GeoStruct == null ? "" : item.GeoStruct.Department == null ? "" : item.GeoStruct.Department.DepartmentObj == null ? "" : item.GeoStruct.Department.DepartmentObj.DeptDesc,
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
                    }
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