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
using P2BUltimate.Security;
using Payroll;
using Recruitment;

namespace P2BUltimate.Controllers.ManPower.MainController
{
    public class ManPowerProvisionController : Controller
    {
        //  private DataBaseContext db = new DataBaseContext();
        //
        // GET: /ManPowerProvision/
        public ActionResult Index()
        {
            return View("~/Views/ManPower/MainViews/ManPowerProvision/Index.cshtml");
        }
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(ManPowerDetailsBatch c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string manpowerdet = form["ManPowerPostDatalist"] == "0" ? "" : form["ManPowerPostDatalist"];
                    string BatchName = form["BatchName"] == "0" ? "" : form["BatchName"];
                    string ProcessDate = form["ProcessDate"] == "0" ? "" : form["ProcessDate"];

                    if (BatchName == "")
                    {
                        Msg.Add("Please Enter BatchName");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (manpowerdet == null)
                    {
                        Msg.Add("Please Select  Manpower Budget");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    ManPowerBudget ManPowerBudget = null;
                    string funid = form["fun_id"] == "0" ? null : form["fun_id"];
                    if (funid == "")
                    {
                        Msg.Add("Apply the Function Struct Filter");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    List<int> funids = null;
                    List<int> Manpowerbudgetid = null;

                    if (funid != null)
                    {
                        funids = Utility.StringIdsToListIds(funid);
                    }
                    List<ManPowerBudget> ManPowerBudgetf = new List<ManPowerBudget>();
                    if (manpowerdet != null)
                    {
                        Manpowerbudgetid = Utility.StringIdsToListIds(manpowerdet);
                    }

                    Calendar calendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToString().ToUpper() == "RECRUITMENTCALENDAR" && e.Default == true).SingleOrDefault();
                    RecruitYearlyCalendar recrut = db.RecruitYearlyCalendar
                                                .Include(e => e.ManPowerDetailsBatch)
                                                .Where(e => e.RecruitmentCalendar.Id == calendar.Id).SingleOrDefault();

                    ManPowerBudgetf = db.ManPowerBudget
                               .Include(e => e.FuncStruct)
                               .Include(e => e.GeoStruct)
                               .Include(e => e.GeoStruct.Location)
                               .Include(e => e.FuncStruct.Job)
                               .Include(e => e.FuncStruct.Job.JobPosition)
                              .Where(e => Manpowerbudgetid.Contains(e.Id)).ToList();

                    //if (manpowerdet != null)
                    //{
                    //    var ids = Utility.StringIdsToListIds(manpowerdet);
                    //    var HolidayList = new List<ManPowerDetailsBatch>();
                    //    foreach (var item in ids)
                    //    {

                    //        int HolidayListid = Convert.ToInt32(item);
                    //        var val = db.ManPowerDetailsBatch.Include(e => e.ManPowerPostData)
                    //                            .Where(e => e.Id == HolidayListid).SingleOrDefault();
                    //        if (val != null)
                    //        {
                    //            HolidayList.Add(val);
                    //        }
                    //    }
                    //    c.ManPowerDetailsBatch = HolidayList;
                    //}


                    if (ModelState.IsValid)
                    {
                        //using (TransactionScope ts = new TransactionScope())
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                        new System.TimeSpan(0, 30, 0)))
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            //foreach (var item1 in funids)
                            //{
                            List<ManPowerPostData> b = new List<ManPowerPostData>();
                            foreach (var item in ManPowerBudgetf)
                            {
                                if (item != null)
                                {
                                    double CurrentCTC = 0;
                                    double totalCTC = 0;
                                    double ExcessCTC = 0;
                                    int vacantpost = 0;
                                    int excesspost = 0;
                                    double CurrentCTCMonthly = 0;
                                    double CurrentCTCYearly = 0;
                                    if (item.GeoStruct.Location != null)
                                    {
                                        List<int> geoids = db.GeoStruct
                                          .Include(e => e.Location)
                                          .Where(e => e.Location.Id == item.GeoStruct.Location.Id).Select(e => e.Id).ToList();

                                        var FilledPost = db.Employee
                                            .Include(a => a.FuncStruct)
                                            .Include(a => a.ServiceBookDates)
                                            .Where(e => e.FuncStruct.Id == item.FuncStruct.Id && geoids.Contains(e.GeoStruct.Id) && e.ServiceBookDates.ServiceLastDate == null).ToList();

                                        var test = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.FuncStruct).Where(q => q.Employee.FuncStruct.Id == item.FuncStruct.Id).ToList();
                                        foreach (var aa in test)
                                        {
                                            //var a = db.EmployeePayroll.Include(e => e.EmpSalStruct)
                                            //    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                                            //    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency))) .Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                                            //    .Where(e => e.Id == aa.Id).SingleOrDefault();

                                            var a = db.EmpSalStruct
                                             .Include(e => e.EmpSalStructDetails)
                                             .Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency))
                                             .Where(e => e.EndDate == null && e.EmployeePayroll_Id == aa.Id).SingleOrDefault();

                                            if (a != null)
                                            {
                                                CurrentCTCMonthly = CurrentCTCMonthly + a.EmpSalStructDetails.Where(t => t.SalaryHead.Frequency.LookupVal.ToString().ToUpper() == "MONTHLY").Sum(e => e.Amount);
                                                CurrentCTCYearly = CurrentCTCYearly + a.EmpSalStructDetails.Where(t => t.SalaryHead.Frequency.LookupVal.ToString().ToUpper() == "YEARLY").Sum(e => e.Amount);
                                            }
                                            CurrentCTC = (CurrentCTCMonthly * 12) + CurrentCTCYearly;

                                        }
                                        if (item.BudgetAmount >= CurrentCTC)
                                        {
                                            ExcessCTC = item.BudgetAmount - CurrentCTC;
                                        }
                                        if (item.SanctionedPosts >= FilledPost.Count())
                                        {
                                            vacantpost = item.SanctionedPosts - FilledPost.Count();
                                        }
                                        else
                                        {
                                            excesspost = FilledPost.Count() - item.SanctionedPosts;
                                        }
                                        totalCTC = CurrentCTC + item.BudgetAmount;
                                        if (item.SanctionedPosts >= FilledPost.Count())
                                        {
                                            ManPowerPostData manpowerdata = new ManPowerPostData
                                             {
                                                 FilledPosts = FilledPost.Count(),
                                                 VacantPosts = vacantpost,
                                                 BudgetCTC = item.BudgetAmount,
                                                 SanctionedPosts = item.SanctionedPosts,
                                                 ExcessPosts = excesspost,
                                                 ExcessCTC = ExcessCTC,
                                                 CurrentCTC = CurrentCTC,
                                                 TotalCTC = totalCTC,
                                                 ManPowerBudget = item,
                                                 DBTrack = c.DBTrack

                                             };
                                            db.ManPowerPostData.Add(manpowerdata);
                                            db.SaveChanges();

                                            b.Add(db.ManPowerPostData.Find(manpowerdata.Id));
                                        }

                                    }
                                }
                            }


                            List<ManPowerDetailsBatch> mdb1 = new List<ManPowerDetailsBatch>();
                            //mdb1.Add(new ManPowerDetailsBatch
                            //{

                            //    BatchName = c.BatchName,
                            //    ProcessDate = c.ProcessDate,
                            //    ManPowerPostData = b,
                            //    DBTrack = c.DBTrack
                            //});

                            ManPowerDetailsBatch manpowerdetails = new ManPowerDetailsBatch
                            {
                                BatchName = c.BatchName,
                                ProcessDate = c.ProcessDate,
                                ManPowerPostData = b,
                                DBTrack = c.DBTrack
                            };
                            db.ManPowerDetailsBatch.Add(manpowerdetails);
                            db.SaveChanges();

                            mdb1.Add(db.ManPowerDetailsBatch.Find(manpowerdetails.Id));

                            if (recrut.ManPowerDetailsBatch != null)
                            {
                                mdb1.AddRange(recrut.ManPowerDetailsBatch);
                            }
                            recrut.ManPowerDetailsBatch = mdb1;
                            //OEmployeePayroll.DBTrack = dbt;
                            db.RecruitYearlyCalendar.Attach(recrut);
                            db.Entry(recrut).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(recrut).State = System.Data.Entity.EntityState.Detached;
                            //ManPowerDetailsBatch mdb = new ManPowerDetailsBatch()
                            //{
                            //    BatchName = batchnm,
                            //    ProcessDate =DateTime.Parse(Procesddate),
                            //    ManPowerPostData=b,
                            //     DBTrack=c.DBTrack
                            //};
                            //  db.ManPowerDetailsBatch.Add(mdb);
                            //  db.SaveChanges();

                            //ManPowerPostData ctc = new ManPowerPostData()
                            //{
                            //    ActionDate = ActionDatee,
                            //    ActionMovement = c.ActionMovement,
                            //    ActionRecruitement = c.ActionRecruitement,
                            //    ManPowerDetailsBatch = mdb1,

                            //    DBTrack = c.DBTrack
                            //};

                            //db.ManPowerProvision.Add(ctc);
                            //   var rtn_Obj = DBTrackFile.DBTrackSave("ManPower", null, db.ChangeTracker,"");
                            //  DT_CtcDefinition DT_Corp = (DT_Corporate)rtn_Obj;

                            //   db.Create(DT_Corp);
                            db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);

                            // }
                            ts.Complete();
                            //  return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
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
                        return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        //return this.Json(new { msg = errorMsg });
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                }
                catch (DataException /* dex */)
                {
                    //  return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                    Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
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
        public class salhddetails
        {
            public Array ManPowerDetailsBatch_id { get; set; }
            public Array ManPowerDetailsBatch_details { get; set; }
        }

        // [HttpPost]
        //public ActionResult Edit(int data)
        //{

        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var Q = db.ManPowerProvision
        //            .Include(e => e.ManPowerDetailsBatch)

        //            .Where(e => e.Id == data).Select
        //            (e => new
        //            {
        //                ActionDate = e.ActionDate,
        //                ActionMovement = e.ActionMovement,
        //                ActionRecruitement = e.ActionRecruitement
        //                //  Action = e.DBTrack.Action
        //            }).ToList();
        //        List<salhddetails> objlist = new List<salhddetails>();
        //        var N = db.ManPowerProvision.Where(e => e.Id == data).Select(e => e.ManPowerDetailsBatch).ToList();
        //        if (N != null && N.Count > 0)
        //        {
        //            foreach (var ca in N)
        //            {
        //                objlist.Add(new salhddetails
        //                {

        //                    ManPowerDetailsBatch_id = ca.Select(e => e.Id).ToArray(),
        //                    ManPowerDetailsBatch_details = ca.Select(e => e.ManPowerPostData).ToArray()


        //                });

        //            }

        //        }

        //        //var W = db.DT_Corporate
        //        //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
        //        //     (e => new
        //        //     {
        //        //         DT_Id = e.Id,
        //        //         Code = e.Code == null ? "" : e.Code,
        //        //         Name = e.Name == null ? "" : e.Name,
        //        //         BusinessType_Val = e.BusinessType_Id == 0 ? "" : db.LookupValue
        //        //                    .Where(x => x.Id == e.BusinessType_Id)
        //        //                    .Select(x => x.LookupVal).FirstOrDefault(),

        //        //         Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
        //        //         Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
        //        //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

        //        var Corp = db.ManPowerProvision.Find(data);
        //        //TempData["RowVersion"] = Corp.RowVersion;
        //        // var Auth = Corp.DBTrack.IsModified;
        //        return Json(new Object[] { Q, objlist, JsonRequestBehavior.AllowGet });
        //    }
        //}
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
                IEnumerable<ManPowerDetailsBatch> ManPowerDetailsBatch = null;
                if (gp.IsAutho == true)
                {
                    ManPowerDetailsBatch = db.ManPowerDetailsBatch.ToList();
                }
                else
                {
                    ManPowerDetailsBatch = db.ManPowerDetailsBatch.AsNoTracking().ToList();
                }

                IEnumerable<ManPowerDetailsBatch> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = ManPowerDetailsBatch;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.BatchName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                 || (e.ProcessDate.Value.ToShortDateString().ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                 || (e.Id.ToString().Contains(gp.searchString))
                                 ).Select(a => new Object[] { a.BatchName, a.ProcessDate.Value.ToShortDateString(), a.Id }).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.BatchName, a.ProcessDate.Value.ToShortDateString(), a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = ManPowerDetailsBatch;
                    Func<ManPowerDetailsBatch, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "BatchName" ? c.BatchName :
                                         gp.sidx == "ProcessDate" ? c.ProcessDate.Value.ToShortDateString() :
                                         "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.BatchName, a.ProcessDate.Value.ToShortDateString(), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.BatchName, a.ProcessDate.Value.ToShortDateString(), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.BatchName, a.ProcessDate.Value.ToShortDateString(), a.Id }).ToList();
                    }
                    totalRecords = ManPowerDetailsBatch.Count();
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

        //public ActionResult Load(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        IEnumerable<ManPowerPostData> ManPowerPostData = null;
        //        if (gp.IsAutho == true)
        //        {
        //            ManPowerPostData = db.ManPowerPostData.ToList();
        //        }
        //        else
        //        {
        //            ManPowerPostData = db.ManPowerPostData.AsNoTracking().ToList();
        //        }

        //        IEnumerable<ManPowerPostData> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = ManPowerPostData;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
        //                          || (e.BudgetCTC.ToString().Contains(gp.searchString))
        //                          || (e.CurrentCTC.ToString().Contains(gp.searchString))
        //                          || (e.ExcessCTC.ToString().Contains(gp.searchString))
        //                          || (e.ExcessPosts.ToString().Contains(gp.searchString))
        //                 || (e.FilledPosts.ToString().Contains(gp.searchString))
        //                 || (e.SanctionedPosts.ToString().Contains(gp.searchString))
        //                 || (e.VacantPosts.ToString().Contains(gp.searchString))
        //                 || (e.TotalCTC.ToString().Contains(gp.searchString))
        //             ).Select(a => new Object[] { Convert.ToString(a.Id), Convert.ToString(a.BudgetCTC), Convert.ToString(a.CurrentCTC), Convert.ToString(a.ExcessCTC), Convert.ToString(a.ExcessPosts), Convert.ToString(a.FilledPosts), Convert.ToString(a.SanctionedPosts), Convert.ToString(a.VacantPosts), Convert.ToString(a.TotalCTC) }).Where(a => a.Contains((gp.searchString))).ToList();
        //            }


        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.BudgetCTC, a.CurrentCTC, a.ExcessCTC, a.ExcessPosts, a.FilledPosts, a.SanctionedPosts, a.VacantPosts, a.TotalCTC }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = ManPowerPostData;
        //            Func<ManPowerPostData, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "BudgetCTC" ? c.BudgetCTC.ToString() :
        //                                 gp.sidx == "CurrentCTC" ? c.CurrentCTC.ToString() :
        //                                    gp.sidx == "ExcessCTC" ? c.CurrentCTC.ToString() :
        //                                       gp.sidx == "ExcessPosts" ? c.CurrentCTC.ToString() :
        //                                          gp.sidx == "FilledPosts" ? c.CurrentCTC.ToString() :
        //                                             gp.sidx == "SanctionedPosts" ? c.CurrentCTC.ToString() :
        //                                                gp.sidx == "VacantPosts" ? c.CurrentCTC.ToString() :
        //                                                   gp.sidx == "TotalCTC" ? c.CurrentCTC.ToString() :
        //                                 "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.BudgetCTC), Convert.ToString(a.CurrentCTC), Convert.ToString(a.ExcessCTC), Convert.ToString(a.ExcessPosts), Convert.ToString(a.FilledPosts), Convert.ToString(a.SanctionedPosts), Convert.ToString(a.VacantPosts), Convert.ToString(a.TotalCTC) }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.BudgetCTC), Convert.ToString(a.CurrentCTC), Convert.ToString(a.ExcessCTC), Convert.ToString(a.ExcessPosts), Convert.ToString(a.FilledPosts), Convert.ToString(a.SanctionedPosts), Convert.ToString(a.VacantPosts), Convert.ToString(a.TotalCTC) }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.BudgetCTC), Convert.ToString(a.CurrentCTC), Convert.ToString(a.ExcessCTC), Convert.ToString(a.ExcessPosts), Convert.ToString(a.FilledPosts), Convert.ToString(a.SanctionedPosts), Convert.ToString(a.VacantPosts), Convert.ToString(a.TotalCTC) }).ToList();
        //            }
        //            totalRecords = ManPowerPostData.Count();
        //        }
        //        if (totalRecords > 0)
        //        {
        //            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //        }
        //        if (gp.page > totalPages)
        //        {
        //            gp.page = totalPages;
        //        }
        //        var JsonData = new
        //        {
        //            page = gp.page,
        //            rows = jsonData,
        //            records = totalRecords,
        //            total = totalPages
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        public ActionResult GetLookupLvHeadObj(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ManPowerBudget
                    .Include(e => e.GeoStruct)
                    .Include(e => e.FuncStruct)
                    .Include(e => e.FuncStruct.Job)
                    .Include(e => e.GeoStruct.Department.DepartmentObj)
                    .Include(e => e.GeoStruct.Location.LocationObj)
                    .ToList();
                IEnumerable<ManPowerBudget> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ManPowerBudget.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall
                         select new
                         {
                             srno = ca.Id,
                             lookupvalue = ca.FullDetails
                         }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }

        }

        public class manprov
        {
            public double BudgetAmount { get; set; }
            public double SanctionedPosts { get; set; }
            public string jobname { get; set; }
            public double filledpost { get; set; }
            public double vacantpost { get; set; }
            public double ExcessPost { get; set; }
            public double CurrentCTC { get; set; }
            public double ExcessCTC { get; set; }
            public double TotalCTC { get; set; }
            public int Id { get; set; }
        }

        // [HttpPost]
        public ActionResult Load(P2BGrid_Parameters gp, string extraeditdata, FormCollection form)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                List<int> Manpowerbudgetid = null;
                if (extraeditdata != "")
                {
                    Manpowerbudgetid = Utility.StringIdsToListIds(extraeditdata);
                }

                //ManPowerBudget ManPowerBudget = null;
                IEnumerable<manprov> ManPower = null;
                List<ManPowerBudget> ManPowerBudgetf = new List<ManPowerBudget>();
                // List<ManPowerBudget> funcountf = new List<ManPowerBudget>();
                // List<int> funcount =new List<int>();

                // List<int> funids = null;

                //if (gp.id != null)
                //{
                //    funids = Utility.StringIdsToListIds(gp.id);
                //}


                //if (gp.id == null)
                //{
                //    if (gp.IsAutho == true)
                //    {
                //        ManPowerBudgetf = db.ManPowerBudget.Include(e => e.FuncStruct).Include(e => e.FuncStruct.Job).AsNoTracking().ToList();
                //        // funcountf = db.ManPowerBudget.Include(e => e.FuncStruct.Id).ToList();
                //    }
                //    else
                //    {
                //        ManPowerBudgetf = db.ManPowerBudget.Include(e => e.FuncStruct).Include(e => e.FuncStruct.Job).AsNoTracking().ToList();
                //        //funcountf = db.ManPowerBudget.Include(e => e.FuncStruct.Id).ToList();
                //    }
                //}
                //else
                //{
                //    if (gp.IsAutho == true)
                //    {
                //        foreach (var item in funids)
                //        {
                //            ManPowerBudget = db.ManPowerBudget.Where(q => q.FuncStruct.Id == item).Include(e => e.FuncStruct).Include(e => e.FuncStruct.Job).AsNoTracking().SingleOrDefault();
                //            // funcount.Add( ManPowerBudget.FuncStruct.Id);
                //            ManPowerBudgetf.Add(ManPowerBudget);
                //        }
                //    }
                //    else
                //    {
                //        foreach (var item in funids)
                //        {
                //            ManPowerBudget = db.ManPowerBudget.Where(q => q.FuncStruct.Id == item).Include(e => e.FuncStruct).Include(e => e.FuncStruct.Job).AsNoTracking().SingleOrDefault();
                //            // funcount.Add(ManPowerBudget.FuncStruct.Id);
                //            ManPowerBudgetf.Add(ManPowerBudget);
                //        }
                //    }
                //}

                ManPowerBudgetf = db.ManPowerBudget
                               .Include(e => e.FuncStruct)
                               .Include(e => e.GeoStruct)
                               .Include(e => e.GeoStruct.Location)
                               .Include(e => e.FuncStruct.Job)
                               .Include(e => e.FuncStruct.Job.JobPosition)
                               .Where(e => Manpowerbudgetid.Contains(e.Id)).ToList();
                List<manprov> b = new List<manprov>();
                var view = new manprov();


                // var oempl = "";
                foreach (var item in ManPowerBudgetf)
                {
                    if (item != null)
                    {
                        double CurrentCTC = 0;
                        double totalCTC = 0;
                        double ExcessCTC = 0;
                        int vacantpost = 0;
                        int excesspost = 0;
                        double CurrentCTCMonthly = 0;
                        double CurrentCTCYearly = 0;
                        double Halfyearly = 0;
                        double Quarterly = 0;
                        if (item.GeoStruct.Location != null)
                        {


                            List<int> geoids = db.GeoStruct
                                              .Include(e => e.Location)
                                              .Where(e => e.Location.Id == item.GeoStruct.Location.Id).Select(e => e.Id).ToList();

                            var FilledPost = db.Employee
                                .Include(a => a.FuncStruct)
                                .Include(a => a.GeoStruct)
                                .Include(a => a.ServiceBookDates)
                                .Where(e => e.FuncStruct.Id == item.FuncStruct.Id && geoids.Contains(e.GeoStruct.Id) && e.ServiceBookDates.ServiceLastDate == null).ToList();

                            List<int> FilledPostemp = db.Employee
                                .Include(a => a.FuncStruct)
                                .Include(a => a.GeoStruct)
                                .Include(a => a.ServiceBookDates)
                                .Where(e => e.FuncStruct.Id == item.FuncStruct.Id && geoids.Contains(e.GeoStruct.Id) && e.ServiceBookDates.ServiceLastDate == null).Select(q => q.Id).ToList();


                            var test = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.FuncStruct).Where(q => q.Employee.FuncStruct.Id == item.FuncStruct.Id && FilledPostemp.Contains(q.Employee.Id)).ToList();
                            foreach (var aa in test)
                            {
                                //var a = db.EmployeePayroll.Include(e => e.EmpSalStruct)
                                //    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                                //    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency)))
                                //    .Where(e => e.Id == aa.Id).SingleOrDefault();
                                var a = db.EmpSalStruct
                                    .Include(e => e.EmpSalStructDetails)
                                    .Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency))
                                    .Where(e => e.EndDate == null && e.EmployeePayroll_Id == aa.Id).SingleOrDefault();

                                var dataempload = db.CTCDefinition.Include(r => r.SalaryHead).SelectMany(e => e.SalaryHead).ToList();
                                var datapass = dataempload.Select(r => r.Id);
                                if (a != null)
                                {
                                    CurrentCTCMonthly = CurrentCTCMonthly + a.EmpSalStructDetails.Where(t => t.SalaryHead.Frequency.LookupVal.ToString().ToUpper() == "MONTHLY" && datapass.Contains(t.SalaryHead.Id)).Sum(e => e.Amount);
                                    CurrentCTCYearly = CurrentCTCYearly + a.EmpSalStructDetails.Where(t => t.SalaryHead.Frequency.LookupVal.ToString().ToUpper() == "YEARLY" && datapass.Contains(t.SalaryHead.Id)).Sum(e => e.Amount);
                                    Halfyearly = Halfyearly + a.EmpSalStructDetails.Where(t => t.SalaryHead.Frequency.LookupVal.ToString().ToUpper() == "HALFYEARLY" && datapass.Contains(t.SalaryHead.Id)).Sum(e => e.Amount);
                                    Quarterly = Quarterly + a.EmpSalStructDetails.Where(t => t.SalaryHead.Frequency.LookupVal.ToString().ToUpper() == "QUATERLY" && datapass.Contains(t.SalaryHead.Id)).Sum(e => e.Amount);
                                    
                                }
                                CurrentCTC = (CurrentCTCMonthly * 12) + (Halfyearly * 2) + (Quarterly * 4) + CurrentCTCYearly;
                            }


                            if (item.BudgetAmount >= CurrentCTC)
                            {
                                ExcessCTC = item.BudgetAmount - CurrentCTC;
                            }
                            if (item.SanctionedPosts >= FilledPost.Count())
                            {
                                vacantpost = item.SanctionedPosts - FilledPost.Count();
                            }
                            else
                            {
                                excesspost = FilledPost.Count() - item.SanctionedPosts;
                            }
                            totalCTC = CurrentCTC + item.BudgetAmount;
                            if (item.SanctionedPosts >= FilledPost.Count())
                            {
                                b.Add(new manprov
                                {
                                    // if (item.SanctionedPosts>FilledPost)

                                    filledpost = FilledPost.Count(),
                                    vacantpost = vacantpost,
                                    BudgetAmount = item.BudgetAmount,
                                    jobname = item.FuncStruct.FullDetails,
                                    Id = item.Id,
                                    SanctionedPosts = item.SanctionedPosts,
                                    //ExcessPost = FilledPost - item.SanctionedPosts
                                    ExcessPost = excesspost,
                                    ExcessCTC = ExcessCTC,
                                    CurrentCTC = CurrentCTC,
                                    TotalCTC = totalCTC

                                });

                            }

                        }
                    }
                }
                //b.Add(oempl);
                ManPower = b;


                IEnumerable<manprov> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = ManPower;
                    if (gp.searchOper.Equals("eq"))
                    {
                        if (gp.searchField == "Id")

                            jsonData = IE.Select(a => new { a.Id, a.BudgetAmount, a.SanctionedPosts, a.jobname, a.filledpost, a.vacantpost, a.ExcessPost, a.CurrentCTC, a.ExcessCTC, a.TotalCTC }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "BudgetAmount")
                            jsonData = IE.Select(a => new { a.Id, a.BudgetAmount, a.SanctionedPosts, a.jobname, a.filledpost, a.vacantpost, a.ExcessPost, a.CurrentCTC, a.ExcessCTC, a.TotalCTC }).Where((e => (e.BudgetAmount.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "SanctionedPosts")
                            jsonData = IE.Select(a => new { a.Id, a.BudgetAmount, a.SanctionedPosts, a.jobname, a.filledpost, a.vacantpost, a.ExcessPost, a.CurrentCTC, a.ExcessCTC, a.TotalCTC }).Where((e => (e.SanctionedPosts.ToString().Contains(gp.searchString)))).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.BudgetAmount, a.SanctionedPosts, a.jobname, a.filledpost, a.vacantpost, a.ExcessPost, a.CurrentCTC, a.ExcessCTC, a.TotalCTC }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = ManPower;
                    Func<manprov, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "BudgetAmount" ? c.BudgetAmount.ToString() :
                                         gp.sidx == "SanctionedPosts" ? c.SanctionedPosts.ToString() :
                                           "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.BudgetAmount), Convert.ToString(a.SanctionedPosts), Convert.ToString(a.jobname), Convert.ToString(a.filledpost), Convert.ToString(a.vacantpost), Convert.ToString(a.ExcessPost), Convert.ToString(a.CurrentCTC), Convert.ToString(a.ExcessCTC), Convert.ToString(a.TotalCTC) }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.BudgetAmount), Convert.ToString(a.SanctionedPosts), Convert.ToString(a.jobname), Convert.ToString(a.filledpost), Convert.ToString(a.vacantpost), Convert.ToString(a.ExcessPost), Convert.ToString(a.CurrentCTC), Convert.ToString(a.ExcessCTC), Convert.ToString(a.TotalCTC) }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.BudgetAmount, a.SanctionedPosts, a.jobname, a.filledpost, a.vacantpost, a.ExcessPost, a.CurrentCTC, a.ExcessCTC, a.TotalCTC }).ToList();
                    }
                    totalRecords = ManPowerBudgetf.Count();
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

        // [HttpPost]
        //public async Task<ActionResult> Delete(int data)
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            ManPowerProvision corporates = db.ManPowerProvision.Include(e => e.ManPowerDetailsBatch).
        //                Where(e => e.Id == data).SingleOrDefault();


        //            if (corporates.DBTrack.IsModified == true)
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
        //                    DBTrack dbT = new DBTrack
        //                    {
        //                        Action = "D",
        //                        CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
        //                        CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
        //                        IsModified = corporates.DBTrack.IsModified == true ? true : false
        //                    };
        //                    corporates.DBTrack = dbT;
        //                    db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
        //                    var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, corporates.DBTrack);
        //                    //   DT_AppraisalPublish DT_Corp = (DT_AppraisalPublish)rtn_Obj;
        //                    //  DT_Corp.AppraisalPeriodCalendar_Id = corporates.AppraisalPeriodCalendar == null ? 0 : corporates.AppraisalPeriodCalendar.Id;
        //                    //  db.Create(DT_Corp);
        //                    await db.SaveChangesAsync();
        //                    ts.Complete();
        //                    //Msg.Add("  Data removed successfully.  ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    Msg.Add("  Data removed.  ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //            else
        //            {

        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    DBTrack dbT = new DBTrack
        //                    {
        //                        Action = "D",
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now,
        //                        CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
        //                        CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
        //                        IsModified = corporates.DBTrack.IsModified == true ? false : false//,
        //                        //AuthorizedBy = SessionManager.UserName,
        //                        //AuthorizedOn = DateTime.Now
        //                    };

        //                    // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

        //                    db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
        //                    // var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, dbT);
        //                    // DT_Corp = (DT_AppraisalPublish)rtn_Obj;
        //                    //   DT_Corp.AppraisalPeriodCalendar_Id = corporates.AppraisalPeriodCalendar == null ? 0 : corporates.AppraisalPeriodCalendar.Id;
        //                    // db.Create(DT_Corp);

        //                    await db.SaveChangesAsync();
        //                    ts.Complete();
        //                    Msg.Add("  Data removed.  ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //        }
        //        catch (RetryLimitExceededException /* dex */)
        //        {
        //            Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        //    }
        //}

    }
}