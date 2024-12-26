using CMS_SPS;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Transactions;
using P2BUltimate.Models;
using System.Data.Entity.Infrastructure;
using System.Data;
using System.Text;
using P2b.Global;
using P2BUltimate.Security;
using System.Web.Script.Serialization;
using Payroll;

namespace P2BUltimate.Controllers.CMS
{
    public class SuccessionPostActionController : Controller
    {
        // GET: CompetencyPostAction
        public ActionResult Index()
        {
            return View("~/Views/CMS/MainViews/SuccessionPostAction/index.cshtml");
        }

        public class returndatagridDataclass //Parentgrid
        {
            public string Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
        }

        public ActionResult GetTrainingCategory(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var fall = db.Category.ToList();
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }

        }

     
        [HttpPost]
        public ActionResult createdata(string BatchNamelist, string EmpId, string ProcessDate, string ReplacedEmpId)
        {
            List<string> Msg = new List<string>();
            int ProcDateId = Convert.ToInt32(ProcessDate);
            int BatchId = Convert.ToInt32(BatchNamelist);
            int ids = Convert.ToInt32(EmpId);
            using (DataBaseContext db = new DataBaseContext())
            {
                using (TransactionScope ts = new TransactionScope())
                {
                    Employee OEmp = db.Employee.Where(e => e.EmpCode == ReplacedEmpId).FirstOrDefault();
                    DBTrack dbtrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                    try
                    {
                        SuccessionPostAction SuccessionPostAction = new SuccessionPostAction()
                        {
                            ActionDate = DateTime.Now,
                            NewFuncStruct = db.FuncStruct.Find(OEmp.FuncStruct_Id),
                            NewGeoStruct = db.GeoStruct.Find(OEmp.GeoStruct_Id),
                            NewPayStruct = db.PayStruct.Find(OEmp.PayStruct_Id),
                            DBTrack = dbtrack
                        };
                        db.SuccessionPostAction.Add(SuccessionPostAction);
                        db.SaveChanges();

                        DateTime? ProcDate = db.SuccessionBatchProcessT.Find(ProcDateId).ProcessDate;
                        
                            var OCompEmpData = db.SuccessionEmployeeDataT.Include(e => e.Employee).Include(e => e.BatchName)
                                .Where(e => e.Employee.Id == ids && e.BatchName.Id == BatchId).FirstOrDefault();
                            if (OCompEmpData != null && OCompEmpData.ProcessDate.Value.Date == ProcDate.Value.Date)
                            {
                                OCompEmpData.SuccessionPostAction = SuccessionPostAction;
                                db.SuccessionEmployeeDataT.Attach(OCompEmpData);
                                db.Entry(OCompEmpData).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(OCompEmpData).State = System.Data.Entity.EntityState.Detached;
                            }
                        

                        ts.Complete();
                        Msg.Add("Data Saved successfully");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                        //    List<string> Msg = new List<string>();
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return null;
        }

        [HttpPost]
        public ActionResult Create(SuccessionPostAction c, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                string BatchName = form["BatchNameList"] == "0" ? "" : form["BatchNameList"];
                string ProcessDate = form["ProcessDateList"] == "0" ? "" : form["ProcessDateList"];

                int ProcDateId = Convert.ToInt32(ProcessDate);
                int BatchId = Convert.ToInt32(BatchName);
                List<int> ids = null;
                DateTime? ProcDate = db.SuccessionEmployeeDataT.Find(ProcDateId).ProcessDate;
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
                try
                {
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            try
                            {
                                SuccessionPostAction SuccessionPostAction = new SuccessionPostAction()
                                {
                                    //Name = c.Name,
                                    //IsTransferRecomment = c.IsTransferRecomment,
                                    //IsTrainingRecommend = c.IsTrainingRecommend,
                                    //IsPromotionRecomment = c.IsPromotionRecomment,
                                    //IsOfficiatingRecomment = c.IsOfficiatingRecomment,
                                    //IsOfficiatingComplete = c.IsOfficiatingComplete,
                                    //IsPromotionComplete = c.IsPromotionComplete,
                                    //IsTrainingComplete = c.IsTrainingComplete,
                                    //IsTransferComplete = c.IsTransferComplete,
                                    //TransferCloseDate = c.TransferCloseDate,
                                    //TrainingCloseDate = c.TrainingCloseDate,
                                    //OfficiatingCloseDate = c.OfficiatingCloseDate,
                                    //PromotionCloseDate = c.PromotionCloseDate,
                                    DBTrack = c.DBTrack
                                };
                                db.SuccessionPostAction.Add(SuccessionPostAction);
                                db.SaveChanges();


                                foreach (var item in ids)
                                {
                                    var OCompEmpData = db.SuccessionEmployeeDataT.Include(e => e.Employee).Include(e => e.BatchName)
                                        .Where(e => e.Employee.Id == item && e.BatchName.Id == BatchId).FirstOrDefault();
                                    if (OCompEmpData != null && OCompEmpData.ProcessDate.Value.Date == ProcDate.Value.Date)
                                    {
                                        OCompEmpData.SuccessionPostAction = SuccessionPostAction;
                                        db.SuccessionEmployeeDataT.Attach(OCompEmpData);
                                        db.Entry(OCompEmpData).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(OCompEmpData).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }
                                
                                ts.Complete();
                                Msg.Add("Data Saved successfully");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                                //    List<string> Msg = new List<string>();
                                Msg.Add(ex.Message);
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                        //var errorMsg = sb.ToString();
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        List<string> MsgB = new List<string>();
                        var errorMsg = sb.ToString();
                        MsgB.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);
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

        public class EmpDataClass
        {
            public int Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public string EmpPhoto { get; set; }
            public string Location { get; set; }
            public string Department { get; set; }
            public string Designation { get; set; }
        }

        public ActionResult LoadEmp(P2BGrid_Parameters gp)
        {
            try
            {
                DateTime? dt = null;
                string monthyr = "";
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<EmpDataClass> EmpList = null;
                List<EmpDataClass> model = new List<EmpDataClass>();
                EmpDataClass view = null;
                string PayMonth = "";
                string Month = "";
                if (gp.filter != null)
                    PayMonth = gp.filter;
                else
                {
                    if (DateTime.Now.Date.Month < 10)
                        Month = "0" + DateTime.Now.Date.Month;
                    else
                        Month = DateTime.Now.Date.Month.ToString();
                    PayMonth = Month + "/" + DateTime.Now.Date.Year;
                }

                //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();

                var compid = Convert.ToInt32(Session["CompId"].ToString());
                var empdata = db.CompanyPayroll
                    .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates))
                    .Where(e => e.Company.Id == compid).SingleOrDefault();

                var emp = empdata.EmployeePayroll.Select(e => e.Employee).ToList();

                foreach (var z in emp)
                {

                    view = new EmpDataClass()
                    {
                        Id = z.Id,
                        EmpCode = z.EmpCode,
                        EmpName = z.EmpName.FullNameFML
                    };
                    if (z.ServiceBookDates.ServiceLastDate == null)
                    {
                        model.Add(view);
                    }
                    else if (Convert.ToDateTime("01/" + z.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + PayMonth))
                    {
                        model.Add(view);
                    }
                    //else if (z.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy") != PayMonth)
                    //{
                    //    model.Add(view);
                    //}


                }
                EmpList = model;

                IEnumerable<EmpDataClass> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmpList;
                    if (gp.searchOper.Equals("eq"))
                    {

                        jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString))
                              || (e.EmpName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                              || (e.Id.ToString().Contains(gp.searchString))
                              ).Select(a => new Object[] { a.EmpCode, a.EmpName, a.Id }).ToList();

                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmpList;
                    Func<EmpDataClass, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>
                                         gp.sidx == "EmployeeCode" ? c.EmpCode.ToString() :
                                         gp.sidx == "EmployeeName" ? c.EmpName.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.EmpCode), a.EmpName, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.EmpCode), a.EmpName, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.EmpCode), a.EmpName, a.Id }).ToList();
                    }
                    totalRecords = EmpList.Count();
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

        public ActionResult Get_Employelist_h(string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime? dt = null;
                string monthyr = "";
                DateTime? dtChk = null;
                
                var Serialize = new JavaScriptSerializer();
                var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);

                
                    dt = Convert.ToDateTime("01/" + DateTime.Now.ToString("MM/yyyy")).AddMonths(-1);
                    monthyr = dt.Value.ToString("MM/yyyy");
                    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                var empdata = db.CompanyPayroll
                .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.GeoStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.FuncStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.PayStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates))
                    .Where(e => e.Company.Id == compid).AsNoTracking().AsParallel().FirstOrDefault();
                List<EmployeePayroll> List_all = new List<EmployeePayroll>();
                List<EmployeePayroll> All_Emp = new List<EmployeePayroll>();
                if (deserialize.GeoStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.GeoStruct);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var List_all_temp = empdata.EmployeePayroll.Where(e => e.Employee.GeoStruct != null && e.Employee.GeoStruct.Id == id).ToList();
                        if (List_all_temp != null && List_all_temp.Count != 0)
                        {
                            List_all.AddRange(List_all_temp);
                        }
                    }
                }
                if (deserialize.PayStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.PayStruct);
                    var List_all_temp = new List<EmployeePayroll>();
                    if (List_all.Count > 0)
                    {
                        List_all_temp = List_all.Where(e => e.Employee.PayStruct != null && one_id.Contains(e.Employee.PayStruct.Id)).ToList();
                    }
                    else
                    {
                        List_all_temp = empdata.EmployeePayroll.Where(e => e.Employee.PayStruct != null && one_id.Contains(e.Employee.PayStruct.Id)).ToList();
                    }
                    if (List_all_temp != null && List_all_temp.Count != 0)
                    {
                        List_all = List_all_temp;
                    }

                }
                if (deserialize.FunStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.FunStruct);
                    var List_all_temp = new List<EmployeePayroll>();
                    if (List_all.Count > 0)
                    {
                        List_all_temp = List_all.Where(e => e.Employee.FuncStruct != null && one_id.Contains(e.Employee.FuncStruct.Id)).ToList();
                    }
                    else
                    {
                        List_all_temp = empdata.EmployeePayroll.Where(e => e.Employee.FuncStruct != null && one_id.Contains(e.Employee.FuncStruct.Id)).ToList();
                    }
                    //var List_all_temp = List_all.Where(e => e.Employee.FuncStruct != null && one_id.Contains(e.Employee.FuncStruct.Id)).ToList();
                    if (List_all_temp != null && List_all_temp.Count != 0)
                    {
                        List_all = List_all_temp;
                    }

                }

                List<EmpDataClass> returndata = new List<EmpDataClass>();
                if (List_all != null && List_all.Count != 0)
                {
                    List<Employee> data = new List<Employee>();
                    data = List_all.Select(e => e.Employee).ToList();
                    if (deserialize.CheckAll == "check")
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            GeoStruct OGeoStruct = db.GeoStruct.Find(item.GeoStruct_Id);
                            FuncStruct OFuncStruct = db.FuncStruct.Find(item.FuncStruct_Id);
                            LocationObj locobj = null;
                            DepartmentObj deptobj = null;
                            Job Jobobj = null;
                            if (OGeoStruct.Location_Id != null)
                            {
                                locobj = db.Location.Include(e => e.LocationObj).Where(e => e.Id == OGeoStruct.Location_Id).FirstOrDefault().LocationObj;
                            }
                            if (OGeoStruct.Department_Id != null)
                            {
                                deptobj = db.Department.Include(e => e.DepartmentObj).Where(e => e.Id == OGeoStruct.Department_Id).FirstOrDefault().DepartmentObj;
                            }
                            if (OFuncStruct != null && OFuncStruct.Job_Id != null)
                            {
                                Jobobj = db.FuncStruct.Include(e => e.Job).Where(e => e.Id == OFuncStruct.Job_Id).FirstOrDefault().Job;
                            }

                            returndata.Add(new EmpDataClass
                            {
                                Id = item.Id,
                                EmpPhoto = item.Photo,
                                EmpCode = item.EmpCode,
                                EmpName = item.EmpName.FullNameFML,
                                Location = locobj != null ? locobj.LocCode + "-" + locobj.LocDesc : "",
                                Department = deptobj != null ? deptobj.DeptCode + "_" + deptobj.DeptDesc : "",
                                Designation = Jobobj != null ? Jobobj.Code + "_" + Jobobj.Name : "",
                            });
                        }
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            if (item.ServiceBookDates.ServiceLastDate == null || (item.ServiceBookDates.ServiceLastDate != null &&
                                item.ServiceBookDates.ServiceLastDate.Value > dtChk))
                            {
                                GeoStruct OGeoStruct = db.GeoStruct.Find(item.GeoStruct_Id);
                                FuncStruct OFuncStruct = db.FuncStruct.Find(item.FuncStruct_Id);
                                LocationObj locobj = null;
                                DepartmentObj deptobj = null;
                                Job Jobobj = null;
                                if (OGeoStruct.Location_Id != null)
                                {
                                    locobj = db.Location.Include(e => e.LocationObj).Where(e => e.Id == OGeoStruct.Location_Id).FirstOrDefault().LocationObj;
                                }
                                if (OGeoStruct.Department_Id != null)
                                {
                                    deptobj = db.Department.Include(e => e.DepartmentObj).Where(e => e.Id == OGeoStruct.Department_Id).FirstOrDefault().DepartmentObj;
                                }
                                if (OFuncStruct != null && OFuncStruct.Job_Id != null)
                                {
                                    Jobobj = db.FuncStruct.Include(e => e.Job).Where(e => e.Id == OFuncStruct.Job_Id).FirstOrDefault().Job;
                                }
                                returndata.Add(new EmpDataClass
                                {
                                    Id = item.Id,
                                    EmpPhoto = item.Photo,
                                    EmpCode = item.EmpCode,
                                    EmpName = item.EmpName.FullNameFML,
                                    Location = locobj != null ? locobj.LocCode + "-" + locobj.LocDesc : "",
                                    Department = deptobj != null ? deptobj.DeptCode + "_" + deptobj.DeptDesc : "",
                                    Designation = Jobobj != null ? Jobobj.Code + "_" + Jobobj.Name : "",
                                });
                            }
                        }
                    }

                    return Json(returndata, JsonRequestBehavior.AllowGet);
                }
                if (List_all.Count == 0)
                {
                    List<Employee> data = new List<Employee>();
                    data = db.Employee.Include(e => e.ServiceBookDates).Include(e => e.EmpName).ToList();
                    if (deserialize.CheckAll == "check")
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            GeoStruct OGeoStruct = db.GeoStruct.Find(item.GeoStruct_Id);
                            FuncStruct OFuncStruct = db.FuncStruct.Find(item.FuncStruct_Id);
                            LocationObj locobj = null;
                            DepartmentObj deptobj = null;
                            Job Jobobj = null;
                            if (OGeoStruct.Location_Id != null)
                            {
                                locobj = db.Location.Include(e => e.LocationObj).Where(e => e.Id == OGeoStruct.Location_Id).FirstOrDefault().LocationObj;
                            }
                            if (OGeoStruct.Department_Id != null)
                            {
                                deptobj = db.Department.Include(e => e.DepartmentObj).Where(e => e.Id == OGeoStruct.Department_Id).FirstOrDefault().DepartmentObj;
                            }
                            if (OFuncStruct != null && OFuncStruct.Job_Id != null)
                            {
                                Jobobj = db.FuncStruct.Include(e => e.Job).Where(e => e.Id == OFuncStruct.Job_Id).FirstOrDefault().Job;
                            }
                            returndata.Add(new EmpDataClass
                            {
                                Id = item.Id,
                                EmpPhoto = "",
                                EmpCode = item.EmpCode,
                                EmpName = item.EmpName.FullNameFML,
                                Location = locobj != null ? locobj.LocCode + "-" + locobj.LocDesc : "",
                                Department = deptobj != null ? deptobj.DeptCode + "_" + deptobj.DeptDesc : "",
                                Designation = Jobobj != null ? Jobobj.Code + "_" + Jobobj.Name : "",
                            });
                        }
                    }
                    else
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            if (item.ServiceBookDates.ServiceLastDate == null || (item.ServiceBookDates.ServiceLastDate != null &&
                                item.ServiceBookDates.ServiceLastDate.Value > dtChk))
                            {
                                GeoStruct OGeoStruct = db.GeoStruct.Find(item.GeoStruct_Id);
                                FuncStruct OFuncStruct = db.FuncStruct.Find(item.FuncStruct_Id);
                                LocationObj locobj = null;
                                DepartmentObj deptobj = null;
                                Job Jobobj = null;
                                if (OGeoStruct.Location_Id != null)
                                {
                                    locobj = db.Location.Include(e => e.LocationObj).Where(e => e.Id == OGeoStruct.Location_Id).FirstOrDefault().LocationObj;
                                }
                                if (OGeoStruct.Department_Id != null)
                                {
                                    deptobj = db.Department.Include(e => e.DepartmentObj).Where(e => e.Id == OGeoStruct.Department_Id).FirstOrDefault().DepartmentObj;
                                }
                                if (OFuncStruct != null && OFuncStruct.Job_Id != null)
                                {
                                    Jobobj = db.FuncStruct.Include(e => e.Job).Where(e => e.Id == OFuncStruct.Job_Id).FirstOrDefault().Job;
                                }
                                returndata.Add(new EmpDataClass
                                {
                                    Id = item.Id,
                                    EmpPhoto = item.Photo,
                                    EmpCode = item.EmpCode,
                                    EmpName = item.EmpName.FullNameFML,
                                    Location = locobj != null ? locobj.LocCode + "-" + locobj.LocDesc : "",
                                    Department = deptobj != null ? deptobj.DeptCode + "_" + deptobj.DeptDesc : "",
                                    Designation = Jobobj != null ? Jobobj.Code + "_" + Jobobj.Name : "",
                                });
                            }
                        }
                    }

                   
                    return Json(returndata, JsonRequestBehavior.AllowGet);
                    //return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }

                else
                {
                    return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }      


        public ActionResult Get_Employelist(string BatchName, string ProcessDate, string ProcessBatch)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                int BatchId = Convert.ToInt32(BatchName);
                DateTime ProcDate = Convert.ToDateTime(ProcessDate);

                var OBatchT = db.SuccessionBatchProcessT.Include(e => e.SuccessionEmployeeDataT)
                    .Include(e => e.SuccessionEmployeeDataT).Include(e => e.SuccessionEmployeeDataT.Select(t => t.Employee))
                    .Include(e => e.SuccessionEmployeeDataT.Select(t => t.Employee.EmpName))
                    .Where(e => e.BatchName_Id == BatchId & e.ProcessBatch == ProcessBatch).SingleOrDefault();

                if (OBatchT.ProcessDate.Value.Date == ProcDate.Date)
                {
                    var muidsa = OBatchT.SuccessionEmployeeDataT.Where(e => e.SuccessionPostAction_Id == null).ToList();
                    
                    if (muidsa != null && muidsa.Count() > 0)
                    {
                        foreach (var item in muidsa)
                        {

                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Employee.Id.ToString(),
                                value = item.Employee.FullDetails,
                            });
                        }

                    }

                }

              
                if (returndata.Count() > 0)
                {
                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }


                return null;
            }
        }

        public class returnGridChildClass
        {
            public int Id { get; set; }
            public string IsTrainingRecommend { get; set; }
            public string IsTransferRecomment { get; set; }
            public string IsOfficiatingRecomment { get; set; }
            public string IsPromotionRecomment { get; set; }


        }

        public ActionResult A_SuccessionModel_Grid(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var db_data = db.SuccessionEmployeeDataT
                 .Include(e => e.SuccessionPostAction)
                    .Where(e => e.Id == data).FirstOrDefault();

                List<returnGridChildClass> returndatases = new List<returnGridChildClass>();
                if (db_data != null)
                {
                    returndatases.Add(new returnGridChildClass
                    {                                                         
                        Id = db_data.Id,
                        //IsOfficiatingRecomment = db_data.SuccessionPostAction.IsOfficiatingRecomment.ToString(),
                        //IsPromotionRecomment = db_data.SuccessionPostAction.IsPromotionRecomment.ToString(),
                        //IsTrainingRecommend = db_data.SuccessionPostAction.IsTrainingRecommend.ToString(),
                        //IsTransferRecomment = db_data.SuccessionPostAction.IsTransferRecomment.ToString(),

                    });
                }
                return Json(returndatases, JsonRequestBehavior.AllowGet);
            }
        }



        public ActionResult GetProcessDate(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = Convert.ToInt32(data);
                var qurey = db.SuccessionBatchProcessT.Include(e => e.BatchName).Where(e => e.BatchName.Id == Id).ToList();

              
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "ProcessDate", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetProcessBatch(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = Convert.ToInt32(data);
                var qurey = db.SuccessionBatchProcessT.Include(e => e.BatchName).Where(e => e.BatchName.Id == Id).ToList();

             
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "ProcessBatch", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult SuccessionEmployeeDataT_Grid(ParamModel param, string selectedtextcmn)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var all = db.SuccessionEmployeeDataT.Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName).Where(e => e.SuccessionPostAction != null && e.BatchName.BatchName == selectedtextcmn)
                        .ToList();



                    IEnumerable<SuccessionEmployeeDataT> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {

                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.BatchName.BatchName.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                  || (e.BatchName.BatchDescription.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<SuccessionEmployeeDataT, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.BatchName.BatchName : "");
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
                        List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                        foreach (var item in fall)
                        {
                            result.Add(new returndatagridDataclass
                            {
                                Id = item.Id.ToString(),
                                EmpCode = item.Employee.EmpCode,
                                EmpName = item.Employee.EmpName.FullNameFML,
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

                                     select new[] { null, Convert.ToString(c.Id), c.BatchName.BatchName, };
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