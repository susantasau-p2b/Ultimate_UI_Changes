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
using Payroll;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class PTaxMasterController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/PTaxMaster/Index.cshtml");
        }

        public ActionResult WagerangePartial()
        {
            return View("~/Views/Shared/Payroll/_wagesrange.cshtml");
        }

        public ActionResult WagePartial()
        {
            return View("~/Views/Shared/Payroll/_Wages.cshtml");
        }

        public ActionResult StatutoryEffectiveMonthsPartial()
        {
            return View("~/Views/Shared/Payroll/_StatutoryEffectiveMonths.cshtml");
        }


        public ActionResult GetProcessPTWageMasterDetails(List<int> SkipIds)
        {
            return View();

        }

        public ActionResult GetStatutoryEffectiveMonths(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.StatutoryEffectiveMonths.Include(e => e.Gender).Include(e => e.EffectiveMonth).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.StatutoryEffectiveMonths.Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }
                var list1 = db.PTaxMaster.Include(e => e.PTStatutoryEffectiveMonths).SelectMany(e => e.PTStatutoryEffectiveMonths).ToList();
                var list2 = fall.Except(list1);
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.EffectiveMonth.LookupVal + " - " + (ca.Gender != null ? ca.Gender.LookupVal : "") }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetState(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.State.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.State.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var list1 = db.PTaxMaster.Include(e => e.States).Select(e => e.States).ToList();
                var list2 = fall.Except(list1);
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Create(PTaxMaster _PTMaster, FormCollection form) //Create submit
        {
            var Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var Stateslist = form["Stateslist"] == "0" ? "" : form["Stateslist"];
                    var ProcessTypeList = Convert.ToString(form["ProcessTypeList"]);
                    string PTWageRangelist = form["PTWageRangelist"];
                    string PTWagesMasterlist = form["PTWagesMasterlist"];
                    string StatutoryEffectiveMonthslist = form["StatutoryEffectiveMonthslist"];
                    var id = int.Parse(Session["CompId"].ToString());
                    var companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == id).SingleOrDefault();


                    if (Stateslist != "0" && Stateslist != "" && Stateslist != null)
                    {
                        var val = db.State.Find(int.Parse(Stateslist));
                        _PTMaster.States = val;

                    }
                    if (ProcessTypeList != "0" && ProcessTypeList != " " && ProcessTypeList != null)
                    {
                        if (ProcessTypeList != "")
                        {

                            var val = db.LookupValue.Find(int.Parse(ProcessTypeList));
                            _PTMaster.Frequency = val;
                        }
                    }


                    if (PTWagesMasterlist != null && PTWagesMasterlist != " ")
                    {

                        var val = db.Wages.Find(int.Parse(PTWagesMasterlist));
                        _PTMaster.PTWagesMaster = val;

                    }
                    List<StatutoryEffectiveMonths> StEffectivemonth = new List<StatutoryEffectiveMonths>();
                    if (StatutoryEffectiveMonthslist != null && StatutoryEffectiveMonthslist != "")
                    {
                        var ids = Utility.StringIdsToListIds(StatutoryEffectiveMonthslist);
                        foreach (var ca in ids)
                        {
                            var val = db.StatutoryEffectiveMonths.Find(ca);
                            StEffectivemonth.Add(val);
                            _PTMaster.PTStatutoryEffectiveMonths = StEffectivemonth;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (_PTMaster.States == null)
                            {
                                //return this.Json(new Object[] { "", "", "States is required.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  States is required ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            if (_PTMaster.PTStatutoryEffectiveMonths == null)
                            {
                                // return this.Json(new Object[] { "", "", "PTStatutoryEffectiveMonths is required.", JsonRequestBehavior.AllowGet });
                                Msg.Add(" PTStatutoryEffectiveMonths is required. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            if (_PTMaster.PTWagesMaster == null)
                            {
                                Msg.Add(" PTWagesMaster is required. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { "", "", "PTWagesMaster is required.", JsonRequestBehavior.AllowGet });

                            }


                            _PTMaster.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            PTaxMaster PTM = new PTaxMaster()
                            {
                                EffectiveDate = _PTMaster.EffectiveDate,
                                //EndDate = _PTMaster.EndDate,
                                Frequency = _PTMaster.Frequency,
                                PTWagesMaster = _PTMaster.PTWagesMaster,
                                States = _PTMaster.States,
                                PTStatutoryEffectiveMonths = _PTMaster.PTStatutoryEffectiveMonths,
                                DBTrack = _PTMaster.DBTrack
                            };
                            try
                            {
                                db.PTaxMaster.Add(PTM);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, _PTMaster.DBTrack);
                                DT_PTaxMaster DT_PTaxMaster = (DT_PTaxMaster)rtn_Obj;
                                DT_PTaxMaster.Frequency_Id = _PTMaster.Frequency == null ? 0 : _PTMaster.Frequency.Id;
                                DT_PTaxMaster.PTWagesMaster_Id = _PTMaster.PTWagesMaster == null ? 0 : _PTMaster.PTWagesMaster.Id;
                                DT_PTaxMaster.States_Id = _PTMaster.States == null ? 0 : _PTMaster.States.Id;
                                //DT_PTaxMaster.StatutoryEffectiveMonths_Id = PTMaster.StatutoryEffectiveMonths == null ? 0 : Convert.ToInt32(PTMaster.StatutoryEffectiveMonths); ;
                                db.Create(DT_PTaxMaster);
                                db.SaveChanges();
                                if (companypayroll != null)
                                {
                                    List<PTaxMaster> pfmasterlist = new List<PTaxMaster>();
                                    pfmasterlist.Add(PTM);
                                    companypayroll.PTaxMaster = pfmasterlist;
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Detached;
                                }
                                ts.Complete();
                                Msg.Add("Data Saved successfully");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = _PTMaster.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Utility.JsonReturnClass { success = false, responseText = "Unable to create. Try again, and if the problem persists contact your system administrator.." }, JsonRequestBehavior.AllowGet);
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


        //public class GridClass
        //{
        //    public string PTWagesMaster { get; set; }
        //    public string States { get; set; }
        //    public string Frequency { get; set; }
        //    public int Id { get; set; }

        //}
        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        IEnumerable<GridClass> ptaxmaster = null;

        //        var compid = int.Parse(Session["CompId"].ToString());
        //        var data = db.CompanyPayroll.Include(e => e.PTaxMaster)
        //            .Include(e=>e.PTaxMaster.Select(t=>t.Frequency))
        //            .Include(e => e.PTaxMaster.Select(t => t.PTWagesMaster))
        //            .Include(e => e.PTaxMaster.Select(t => t.States))
        //            .Include(e => e.PTaxMaster.Select(t => t.PTStatutoryEffectiveMonths))
        //            .Where(e => e.Company.Id == compid)
        //            .Select(e => e.PTaxMaster).SingleOrDefault();
        //        List<GridClass> model = new List<GridClass>();
        //        foreach (var item in data)
        //        {
        //            model.Add(new GridClass
        //            {
        //                Id = item.Id,
        //                Frequency = item.Frequency != null ? item.Frequency.LookupVal : null,
        //                States = item.States != null ? item.States.FullDetails : null,
        //                PTWagesMaster = item.PTWagesMaster != null ? item.PTWagesMaster.FullDetails : null,
        //            });
        //        }
        //        ptaxmaster = model;
        //        //if (gp.IsAutho == true)
        //        //{
        //        //    ptaxmaster = db.PTaxMaster.Include(e => e.States).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        //}
        //        //else
        //        //{
        //        //    ptaxmaster = db.PTaxMaster.Include(e => e.States).AsNoTracking().ToList();
        //        //}

        //        IEnumerable<GridClass> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = ptaxmaster;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "Id")
        //                    jsonData = IE.Select(a => new { a.Id, a.Frequency, a.PTWagesMaster, a.States }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == " Process Type")
        //                    jsonData = IE.Select(a => new { a.Id, a.Frequency, a.PTWagesMaster, a.States }).Where((e => (e.Frequency.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "PTWagesMaster")
        //                    jsonData = IE.Select(a => new { a.Id, a.Frequency, a.PTWagesMaster, a.States }).Where((e => (e.States.ToString().Contains(gp.searchString)))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Frequency, a.States, a.PTWagesMaster != null }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = ptaxmaster;
        //            Func<GridClass, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "Process Type" ? c.Frequency :
        //                gp.sidx == "State" ? c.States : ""

        //                );

        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.States }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.States != null ? a.States : "" }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.States != null ? a.States : "" }).ToList();
        //            }
        //            totalRecords = ptaxmaster.Count();
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


        public class P2BGridData
        {
            public int Id { get; set; }

            public string StateList { get; set; }
            public string PTWagesMaster { get; set; }
        }


        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> PTAXMASTERList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;


                    //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                    //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);

                    var BindCompList = db.CompanyPayroll.Include(e => e.PTaxMaster)
                                      .Include(e => e.PTaxMaster.Select(t => t.States))
                                      .Include(e => e.PTaxMaster.Select(t => t.PTWagesMaster))
                                      .Where(e => e.Company.Id == company_Id).ToList();

                    foreach (var z in BindCompList)
                    {
                        if (z.PTaxMaster != null)
                        {

                            foreach (var E in z.PTaxMaster)
                            {
                                //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
                                view = new P2BGridData()
                                {
                                    Id = E.Id,
                                    StateList = E.States != null ? Convert.ToString(E.States.FullDetails) : "",
                                    PTWagesMaster = E.PTWagesMaster != null ? Convert.ToString(E.PTWagesMaster.FullDetails) : ""

                                };
                                model.Add(view);

                            }
                        }

                    }

                    PTAXMASTERList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = PTAXMASTERList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.StateList.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.PTWagesMaster.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.StateList, a.PTWagesMaster, a.Id }).ToList();
                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.StateList, a.PTWagesMaster, a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = PTAXMASTERList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c =>    gp.sidx == "StateList" ? c.StateList.ToString() :
                                                gp.sidx == "PTWagesMaster" ? c.PTWagesMaster.ToString() : ""

                                            );
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.StateList, a.PTWagesMaster, a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.StateList, a.PTWagesMaster, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.StateList, a.PTWagesMaster, a.Id }).ToList();
                        }
                        totalRecords = PTAXMASTERList.Count();
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
        }
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
              List<string> Msg = new List<string>();
              using (DataBaseContext db = new DataBaseContext())
              {
                  try
                  {
                      if (auth_action == "C")
                      {
                          using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                          {


                              PTaxMaster PTMaster = db.PTaxMaster.Include(e => e.Frequency)
                              .Include(e => e.PTWagesMaster).Include(e => e.States).Include(e => e.PTStatutoryEffectiveMonths).FirstOrDefault(e => e.Id == auth_id);

                              PTMaster.DBTrack = new DBTrack
                              {
                                  Action = "C",
                                  ModifiedBy = PTMaster.DBTrack.ModifiedBy != null ? PTMaster.DBTrack.ModifiedBy : null,
                                  CreatedBy = PTMaster.DBTrack.CreatedBy != null ? PTMaster.DBTrack.CreatedBy : null,
                                  CreatedOn = PTMaster.DBTrack.CreatedOn != null ? PTMaster.DBTrack.CreatedOn : null,
                                  IsModified = PTMaster.DBTrack.IsModified == true ? false : false,
                                  AuthorizedBy = SessionManager.UserName,
                                  AuthorizedOn = DateTime.Now
                              };

                              db.PTaxMaster.Attach(PTMaster);
                              db.Entry(PTMaster).State = System.Data.Entity.EntityState.Modified;
                              db.Entry(PTMaster).OriginalValues["RowVersion"] = TempData["RowVersion"];
                              //db.SaveChanges();
                              var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, PTMaster.DBTrack);
                              DT_PTaxMaster DT_PTaxMaster = (DT_PTaxMaster)rtn_Obj;
                              DT_PTaxMaster.Frequency_Id = PTMaster.Frequency == null ? 0 : PTMaster.Frequency.Id;
                              DT_PTaxMaster.PTWagesMaster_Id = PTMaster.PTWagesMaster == null ? 0 : PTMaster.PTWagesMaster.Id;
                              DT_PTaxMaster.States_Id = PTMaster.States == null ? 0 : PTMaster.States.Id;

                              db.Create(DT_PTaxMaster);
                              await db.SaveChangesAsync();

                              ts.Complete();
                              Msg.Add("  Record Authorised");
                              return Json(new Utility.JsonReturnClass { Id = PTMaster.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                              //return Json(new Object[] { PTMaster.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                          }
                      }
                      else if (auth_action == "M")
                      {

                          PTaxMaster Old_PTMaster = db.PTaxMaster.Include(e => e.Frequency)
                          .Include(e => e.PTWagesMaster).Include(e => e.States).Include(e => e.PTStatutoryEffectiveMonths).Where(e => e.Id == auth_id).SingleOrDefault();


                          DT_PTaxMaster Curr_PTMaster = db.DT_PTaxMaster
                          .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                          .OrderByDescending(e => e.Id)
                          .FirstOrDefault();

                          if (Curr_PTMaster != null)
                          {
                              PTaxMaster PTM = new PTaxMaster();

                              string Frequency = Curr_PTMaster.Frequency_Id == null ? null : Curr_PTMaster.Frequency_Id.ToString();
                              //string PTWageRange = Curr_PTMaster. == null ? null : Curr_PTMaster.PTWageRange_Id.ToString();
                              string PTWagesMaster = Curr_PTMaster.PTWagesMaster_Id == null ? null : Curr_PTMaster.PTWagesMaster_Id.ToString();
                              // corp.Id = auth_id;

                              if (ModelState.IsValid)
                              {
                                  try
                                  {

                                      //DbContextTransaction transaction = db.Database.BeginTransaction();

                                      using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                      {
                                          // db.Configuration.AutoDetectChangesEnabled = false;
                                          PTM.DBTrack = new DBTrack
                                          {
                                              CreatedBy = Old_PTMaster.DBTrack.CreatedBy == null ? null : Old_PTMaster.DBTrack.CreatedBy,
                                              CreatedOn = Old_PTMaster.DBTrack.CreatedOn == null ? null : Old_PTMaster.DBTrack.CreatedOn,
                                              Action = "M",
                                              ModifiedBy = Old_PTMaster.DBTrack.ModifiedBy == null ? null : Old_PTMaster.DBTrack.ModifiedBy,
                                              ModifiedOn = Old_PTMaster.DBTrack.ModifiedOn == null ? null : Old_PTMaster.DBTrack.ModifiedOn,
                                              AuthorizedBy = SessionManager.UserName,
                                              AuthorizedOn = DateTime.Now,
                                              IsModified = false
                                          };

                                          await db.SaveChangesAsync();
                                          ts.Complete();
                                          Msg.Add("  Record Authorised");
                                          return Json(new Utility.JsonReturnClass { Id = PTM.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                          //return Json(new Object[] { PTM.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                      }
                                  }
                                  catch (DbUpdateConcurrencyException ex)
                                  {
                                      var entry = ex.Entries.Single();
                                      var clientValues = (Corporate)entry.Entity;
                                      var databaseEntry = entry.GetDatabaseValues();
                                      if (databaseEntry == null)
                                      {
                                          Msg.Add("Unable to save changes. The record was deleted by another user.");
                                          return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                      }
                                      else
                                      {
                                          var databaseValues = (Corporate)databaseEntry.ToObject();
                                          PTM.RowVersion = databaseValues.RowVersion;
                                      }
                                  }

                                  Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                  return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                              }
                          }
                          else
                              Msg.Add("Data removed from history");
                          return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                      }
                      else if (auth_action == "D")
                      {
                          using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                          {
                              //Corporate corp = db.Corporate.Find(auth_id);
                              PTaxMaster PTMaster = db.PTaxMaster.AsNoTracking().Include(e => e.Frequency)
                              .Include(e => e.PTWagesMaster).Include(e => e.States).Include(e => e.PTStatutoryEffectiveMonths).FirstOrDefault(e => e.Id == auth_id);



                              PTMaster.DBTrack = new DBTrack
                              {
                                  Action = "D",
                                  ModifiedBy = PTMaster.DBTrack.ModifiedBy != null ? PTMaster.DBTrack.ModifiedBy : null,
                                  CreatedBy = PTMaster.DBTrack.CreatedBy != null ? PTMaster.DBTrack.CreatedBy : null,
                                  CreatedOn = PTMaster.DBTrack.CreatedOn != null ? PTMaster.DBTrack.CreatedOn : null,
                                  IsModified = false,
                                  AuthorizedBy = SessionManager.UserName,
                                  AuthorizedOn = DateTime.Now
                              };

                              db.PTaxMaster.Attach(PTMaster);
                              db.Entry(PTMaster).State = System.Data.Entity.EntityState.Deleted;


                              var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, PTMaster.DBTrack);
                              DT_PTaxMaster DT_PTaxMaster = (DT_PTaxMaster)rtn_Obj;
                              DT_PTaxMaster.Frequency_Id = PTMaster.Frequency == null ? 0 : PTMaster.Frequency.Id;
                              DT_PTaxMaster.PTWagesMaster_Id = PTMaster.PTWagesMaster == null ? 0 : PTMaster.PTWagesMaster.Id;
                              db.Create(DT_PTaxMaster);
                              await db.SaveChangesAsync();
                              db.Entry(PTMaster).State = System.Data.Entity.EntityState.Detached;
                              ts.Complete();
                              Msg.Add(" Record Authorised ");
                              return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                              //return Json(new Utility.JsonReturnClass { success = true, responseText = "Record Authorised" }, JsonRequestBehavior.AllowGet);
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
        private class Ptaxmaster_data
        {
            public String PTWagesMaster_Id { get; set; }
            public String PTWagesMaster_FullDetails { get; set; }
            public String States_Name { get; set; }
            public String State_Id { get; set; }
            public Array Effectivemonth_id { get; set; }
            public Array effective_data { get; set; }
        }
        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.PTaxMaster
                .Include(e => e.Frequency)
                .Include(e => e.PTWagesMaster)
                .Include(e => e.States)
                .Include(e => e.PTStatutoryEffectiveMonths).Where(e => e.Id == data).AsEnumerable()
                .Select(e => new
                {
                    EndDate = e.EndDate == null ? null : e.EndDate.Value.ToShortDateString(),
                    EffectiveDate = e.EffectiveDate == null ? null : e.EffectiveDate.Value.ToShortDateString(),
                    ProcessType_id = e.Frequency == null ? 0 : e.Frequency.Id,
                    Action = e.DBTrack.Action
                }).ToList();

                var add_data = db.PTaxMaster
                .Include(e => e.Frequency)
                .Include(e => e.PTWagesMaster)
                .Include(e => e.States)
                .Include(e => e.PTStatutoryEffectiveMonths).Include(x => x.PTStatutoryEffectiveMonths.Select(e => e.EffectiveMonth)).Include(x => x.PTStatutoryEffectiveMonths.Select(e => e.Gender))
                .Where(e => e.Id == data).SingleOrDefault();
                List<Ptaxmaster_data> returndata = new List<Ptaxmaster_data>();
                if (add_data != null)
                {
                    returndata.Add(new Ptaxmaster_data
                    {
                        PTWagesMaster_Id = add_data.PTWagesMaster == null ? null : add_data.PTWagesMaster.Id.ToString(),
                        PTWagesMaster_FullDetails = add_data.PTWagesMaster.FullDetails == null ? "" : add_data.PTWagesMaster.FullDetails,
                        States_Name = add_data.States == null ? "" : add_data.States.Name,
                        State_Id = add_data.States == null ? "" : add_data.States.Id.ToString(),
                        Effectivemonth_id = add_data.PTStatutoryEffectiveMonths.Select(e => e.Id.ToString() == null ? null : e.Id.ToString()).ToArray(),
                        effective_data = add_data.PTStatutoryEffectiveMonths.Select(e => e.EffectiveMonth == null ? null : e.EffectiveMonth.LookupVal + " - " + (e.Gender != null ? e.Gender.LookupVal : "")).ToArray(),
                    });
                }

                var W = db.DT_PTaxMaster
                .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                (e => new
                {
                    DT_Id = e.Id,

                    Process_Val = e.Frequency_Id == 0 ? null : db.LookupValue
                    .Where(x => x.Id == e.Frequency_Id)
                    .Select(x => x.LookupVal).FirstOrDefault(),

                    State_Val = e.States_Id == 0 ? null : db.State
                    .Where(x => x.Id == e.States_Id).FirstOrDefault(),



                    //PTWageRange_Val = e.PTWageRange_Id == 0 ? null : db.Range
                    //.Where(x => x.Id == e.PTWageRange_Id)
                    //.FirstOrDefault(),

                }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Ptmaster = db.PTaxMaster.Find(data);
                TempData["RowVersion"] = Ptmaster.RowVersion;
                var Auth = Ptmaster.DBTrack.IsModified;
                return Json(new Object[] { Q, returndata, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        public int EditS(string Frequency, string State, string PTWageRangeList, string PTWagesMaster, int data, PTaxMaster PT, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Frequency != null)
                {
                    if (Frequency != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Frequency));
                        PT.Frequency = val;
                        var Ptypes = db.PTaxMaster.Include(e => e.Frequency).Where(e => e.Id == data).SingleOrDefault();
                        IList<PTaxMaster> PTdetails = null;
                        if (Ptypes.Frequency != null)
                        {
                            PTdetails = db.PTaxMaster.Where(x => x.Frequency.Id == Ptypes.Frequency.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            PTdetails = db.PTaxMaster.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in PTdetails)
                        {
                            s.Frequency = PT.Frequency;
                            db.PTaxMaster.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.PTaxMaster.Include(e => e.Frequency).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.Frequency = null;
                            db.PTaxMaster.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var BusiTypeDetails = db.PTaxMaster.Include(e => e.Frequency).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.Frequency = null;
                        db.PTaxMaster.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (State != null)
                {
                    if (State != "")
                    {
                        var val = db.State.Find(int.Parse(State));
                        PT.States = val;

                        var Stypes = db.PTaxMaster.Include(e => e.States).Where(e => e.Id == data).SingleOrDefault();
                        IList<PTaxMaster> Stypedetails = null;
                        if (Stypes.States != null)
                        {
                            Stypedetails = db.PTaxMaster.Where(x => x.States.Id == Stypes.States.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            Stypedetails = db.PTaxMaster.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in Stypedetails)
                        {
                            s.States = PT.States;
                            db.PTaxMaster.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.PTaxMaster.Include(e => e.States).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.States = null;
                            db.PTaxMaster.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var BusiTypeDetails = db.PTaxMaster.Include(e => e.States).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.States = null;
                        db.PTaxMaster.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (PTWagesMaster != null)
                {
                    if (PTWagesMaster != "")
                    {
                        var val = db.Wages.Find(int.Parse(PTWagesMaster));
                        PT.PTWagesMaster = val;

                        var WMtypes = db.PTaxMaster.Include(e => e.PTWagesMaster).Where(e => e.Id == data).SingleOrDefault();
                        IList<PTaxMaster> WMtypedetails = null;
                        if (WMtypes.PTWagesMaster != null)
                        {
                            WMtypedetails = db.PTaxMaster.Where(x => x.PTWagesMaster.Id == WMtypes.PTWagesMaster.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            WMtypedetails = db.PTaxMaster.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in WMtypedetails)
                        {
                            s.PTWagesMaster = PT.PTWagesMaster;
                            db.PTaxMaster.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.PTaxMaster.Include(e => e.PTWagesMaster).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.PTWagesMaster = null;
                            db.PTaxMaster.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var BusiTypeDetails = db.PTaxMaster.Include(e => e.PTWagesMaster).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.States = null;
                        db.PTaxMaster.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                var CurPTMaster = db.PTaxMaster.Find(data);
                TempData["CurrRowVersion"] = CurPTMaster.RowVersion;
                db.Entry(CurPTMaster).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    PT.DBTrack = dbT;
                    PTaxMaster ptmaster = new PTaxMaster()
                    {
                        Id = data,
                        Frequency = PT.Frequency,
                        PTWagesMaster = PT.PTWagesMaster,
                        States = PT.States,
                        PTStatutoryEffectiveMonths = PT.PTStatutoryEffectiveMonths,
                        DBTrack = PT.DBTrack
                    };
                    db.PTaxMaster.Attach(ptmaster);
                    db.Entry(ptmaster).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(ptmaster).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    return 1;
                }
                return 0;
            }
        }


        [HttpPost]
        public async Task<ActionResult> EditSave(PTaxMaster PTM, int data, FormCollection form) // Edit submit
        {
             List<string> Msg = new List<string>();

                     string Stateslist = form["Stateslist"];
                     string ProcessTypeList = form["ProcessTypeList"];
                     //string PTWageRangelist = form["PTWageRangelist"];
                     string PTWagesMasterlist = form["PTWagesMasterlist"];
                     string StatutoryEffectiveMonthslist = form["StatutoryEffectiveMonthslist"];
                     bool Auth = form["Autho_Allow"] == "true" ? true : false;

                     PTM.States_Id = Stateslist != null && Stateslist != "" ? int.Parse(Stateslist) : 0;
                     PTM.Frequency_Id = ProcessTypeList != null && ProcessTypeList != "" ? int.Parse(ProcessTypeList) : 0;
                     PTM.PTWagesMaster_Id = PTWagesMasterlist != null && PTWagesMasterlist != "" ? int.Parse(PTWagesMasterlist) : 0;

                     if (Stateslist == null)
                     {
                         //return this.Json(new Object[] { "", "", "States is required.", JsonRequestBehavior.AllowGet });
                         Msg.Add("  States is required ");
                         return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                     }
                     if (StatutoryEffectiveMonthslist == null)
                     {
                         // return this.Json(new Object[] { "", "", "PTStatutoryEffectiveMonths is required.", JsonRequestBehavior.AllowGet });
                         Msg.Add(" PTStatutoryEffectiveMonths is required. ");
                         return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                     }
                     if (PTWagesMasterlist == null)
                     {
                         Msg.Add(" PTWagesMaster is required. ");
                         return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                         //return this.Json(new Object[] { "", "", "PTWagesMaster is required.", JsonRequestBehavior.AllowGet });

                     }

                     
                   using (DataBaseContext db = new DataBaseContext())
                    {
                      try
                         {
                     //var db_data = db.PTaxMaster.Include(e => e.Frequency).Include(e => e.PTWagesMaster).Include(e => e.States).Include(e => e.StatutoryEffectiveMonths).Where(e => e.Id == data).SingleOrDefault();
                     //string Stateslist = form["Stateslist"];
                     //string ProcessTypeList = form["ProcessTypeList"];
                     //string PTWageRangelist = form["PTWageRangelist"];
                     //string PTWagesMasterlist = form["PTWagesMasterlist"];
                     //string StatutoryEffectiveMonthslist = form["StatutoryEffectiveMonthslist"];
                     //bool Auth = form["Autho_Allow"] == "true" ? true : false;
                     //if (Stateslist != "0" && Stateslist != "" && Stateslist != null)
                     //{
                     //    var val = db.State.Find(int.Parse(Stateslist));
                     //    PTM.States = val;

                     //}
                     //if (ProcessTypeList != "0" && ProcessTypeList != " " && ProcessTypeList != null)
                     //{
                     //    if (ProcessTypeList != "")
                     //    {
                     //        var val = db.LookupValue.Find(int.Parse(ProcessTypeList));
                     //        PTM.Frequency = val;
                     //    }
                     //}



                     //if (PTWagesMasterlist != null && PTWagesMasterlist != " ")
                     //{

                     //    var val = db.Wages.Find(int.Parse(PTWagesMasterlist));
                     //    PTM.PTWagesMaster = val;

                     //}
                     //List<StatutoryEffectiveMonths> StEffectivemonth = new List<StatutoryEffectiveMonths>();
                     //if (StatutoryEffectiveMonthslist != null && StatutoryEffectiveMonthslist != "")
                     //{
                     //    var ids = Utility.StringIdsToListIds(StatutoryEffectiveMonthslist);
                     //    foreach (var ca in ids)
                     //    {
                     //        var val = db.StatutoryEffectiveMonths.Find(ca);
                     //        StEffectivemonth.Add(val);
                     //        PTM.PTStatutoryEffectiveMonths = StEffectivemonth;
                     //    }
                     //}
                     if (Auth == false)
                     {

                         if (ModelState.IsValid)
                         {
                             try
                             {
                                 using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                 {
                                     var db_data = db.PTaxMaster.Include(e => e.Frequency).Include(e => e.PTWagesMaster).Include(e => e.States).Include(e =>e.PTStatutoryEffectiveMonths).Where(e => e.Id == data).SingleOrDefault();

                                    List<StatutoryEffectiveMonths> statutoryeffectivemonths = new List<StatutoryEffectiveMonths>();

                                    if (StatutoryEffectiveMonthslist != "" && StatutoryEffectiveMonthslist != null)
                                      {
                                          var ids = Utility.StringIdsToListIds(StatutoryEffectiveMonthslist);
                                      foreach (var ca in ids)
                                      {
                                       var Values_val = db.StatutoryEffectiveMonths.Find(ca);
                                           statutoryeffectivemonths.Add(Values_val);
                                      }
                                       db_data.PTStatutoryEffectiveMonths = statutoryeffectivemonths;
                                     }
                                     else
                                     {
                                      db_data.PTStatutoryEffectiveMonths = null;
                                     }
                                    if (PTM.States_Id == 0)
                                    {
                                        db_data.States = null;
                                    }
                                    if (PTM.Frequency_Id == 0)
                                    {
                                        db_data.Frequency = null;
                                    }
                                    if (PTM.PTWagesMaster_Id == 0)
                                    {
                                        db_data.PTWagesMaster = null;
                                    }

                                    db.PTaxMaster.Attach(db_data);
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_data.RowVersion;
                                    PTaxMaster ptaxmaster = db.PTaxMaster.Find(data);
                                    TempData["CurrRowVersion"] = ptaxmaster.RowVersion;
                       
                                     //PTaxMaster blog = null; // to retrieve old data
                                     //DbPropertyValues originalBlogValues = null;

                                     //using (var context = new DataBaseContext())
                                     //{
                                     // blog = context.PTaxMaster.Where(e => e.Id == data).Include(e => e.Frequency)
                                     // .Include(e => e.PTWagesMaster)
                                     // .Include(e => e.States)
                                     // .Include(e => e.StatutoryEffectiveMonths)
                                     // .SingleOrDefault();
                                     // originalBlogValues = context.Entry(blog).OriginalValues;
                                     //}

                                     //PTM.DBTrack = new DBTrack
                                     //{
                                     // CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                     // CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                     // Action = "M",
                                     // ModifiedBy = SessionManager.UserName,
                                     // ModifiedOn = DateTime.Now
                                     //};

                                     //if (ProcessTypeList != null)
                                     //{
                                     //    if (ProcessTypeList != "")
                                     //    {
                                     //        var val = db.LookupValue.Find(int.Parse(ProcessTypeList));
                                     //        PTM.Frequency = val;
                                     //        var Ptypes = db.PTaxMaster.Include(e => e.Frequency).Where(e => e.Id == data).SingleOrDefault();
                                     //        IList<PTaxMaster> PTdetails = null;
                                     //        if (Ptypes.Frequency != null)
                                     //        {
                                     //            PTdetails = db.PTaxMaster.Where(x => x.Frequency.Id == Ptypes.Frequency.Id && x.Id == data).ToList();
                                     //        }
                                     //        else
                                     //        {
                                     //            PTdetails = db.PTaxMaster.Where(x => x.Id == data).ToList();
                                     //        }
                                     //        foreach (var s in PTdetails)
                                     //        {
                                     //            s.Frequency = PTM.Frequency;
                                     //            db.PTaxMaster.Attach(s);
                                     //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                     //            db.SaveChanges();
                                     //            TempData["RowVersion"] = s.RowVersion;
                                     //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                     //        }
                                     //    }
                                     //    else
                                     //    {
                                     //        var BusiTypeDetails = db.PTaxMaster.Include(e => e.Frequency).Where(x => x.Id == data).ToList();
                                     //        foreach (var s in BusiTypeDetails)
                                     //        {
                                     //            s.Frequency = null;
                                     //            db.PTaxMaster.Attach(s);
                                     //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                     //            db.SaveChanges();
                                     //            TempData["RowVersion"] = s.RowVersion;
                                     //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                     //        }
                                     //    }
                                     //}
                                     //else
                                     //{
                                     //    var BusiTypeDetails = db.PTaxMaster.Include(e => e.Frequency).Where(x => x.Id == data).ToList();
                                     //    foreach (var s in BusiTypeDetails)
                                     //    {
                                     //        s.Frequency = null;
                                     //        db.PTaxMaster.Attach(s);
                                     //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                     //        db.SaveChanges();
                                     //        TempData["RowVersion"] = s.RowVersion;
                                     //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                     //    }
                                     //}

                                     //if (Stateslist != null)
                                     //{
                                     //    if (Stateslist != "")
                                     //    {
                                     //        var val = db.State.Find(int.Parse(Stateslist));
                                     //        PTM.States = val;

                                     //        var Stypes = db.PTaxMaster.Include(e => e.States).Where(e => e.Id == data).SingleOrDefault();
                                     //        IList<PTaxMaster> Stypedetails = null;
                                     //        if (Stypes.States != null)
                                     //        {
                                     //            Stypedetails = db.PTaxMaster.Where(x => x.States.Id == Stypes.States.Id && x.Id == data).ToList();
                                     //        }
                                     //        else
                                     //        {
                                     //            Stypedetails = db.PTaxMaster.Where(x => x.Id == data).ToList();
                                     //        }
                                     //        foreach (var s in Stypedetails)
                                     //        {
                                     //            s.States = PTM.States;
                                     //            db.PTaxMaster.Attach(s);
                                     //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                     //            db.SaveChanges();
                                     //            TempData["RowVersion"] = s.RowVersion;
                                     //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                     //        }
                                     //    }
                                     //    else
                                     //    {
                                     //        var BusiTypeDetails = db.PTaxMaster.Include(e => e.States).Where(x => x.Id == data).ToList();
                                     //        foreach (var s in BusiTypeDetails)
                                     //        {
                                     //            s.States = null;
                                     //            db.PTaxMaster.Attach(s);
                                     //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                     //            db.SaveChanges();
                                     //            TempData["RowVersion"] = s.RowVersion;
                                     //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                     //        }
                                     //    }
                                     //}
                                     //else
                                     //{
                                     //    var BusiTypeDetails = db.PTaxMaster.Include(e => e.States).Where(x => x.Id == data).ToList();
                                     //    foreach (var s in BusiTypeDetails)
                                     //    {
                                     //        s.States = null;
                                     //        db.PTaxMaster.Attach(s);
                                     //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                     //        db.SaveChanges();
                                     //        TempData["RowVersion"] = s.RowVersion;
                                     //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                     //    }
                                     //}

                                     //if (PTWagesMasterlist != null)
                                     //{
                                     //    if (PTWagesMasterlist != "")
                                     //    {
                                     //        var val = db.Wages.Find(int.Parse(PTWagesMasterlist));
                                     //        PTM.PTWagesMaster = val;

                                     //        var WMtypes = db.PTaxMaster.Include(e => e.PTWagesMaster).Where(e => e.Id == data).SingleOrDefault();
                                     //        IList<PTaxMaster> WMtypedetails = null;
                                     //        if (WMtypes.PTWagesMaster != null)
                                     //        {
                                     //            WMtypedetails = db.PTaxMaster.Where(x => x.PTWagesMaster.Id == WMtypes.PTWagesMaster.Id && x.Id == data).ToList();
                                     //        }
                                     //        else
                                     //        {
                                     //            WMtypedetails = db.PTaxMaster.Where(x => x.Id == data).ToList();
                                     //        }
                                     //        foreach (var s in WMtypedetails)
                                     //        {
                                     //            s.PTWagesMaster = PTM.PTWagesMaster;
                                     //            db.PTaxMaster.Attach(s);
                                     //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                     //            db.SaveChanges();
                                     //            TempData["RowVersion"] = s.RowVersion;
                                     //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                     //        }
                                     //    }
                                     //    else
                                     //    {
                                     //        var BusiTypeDetails = db.PTaxMaster.Include(e => e.PTWagesMaster).Where(x => x.Id == data).ToList();
                                     //        foreach (var s in BusiTypeDetails)
                                     //        {
                                     //            s.PTWagesMaster = null;
                                     //            db.PTaxMaster.Attach(s);
                                     //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                     //            db.SaveChanges();
                                     //            TempData["RowVersion"] = s.RowVersion;
                                     //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                     //        }
                                     //    }
                                     //}
                                     //else
                                     //{
                                     //    var BusiTypeDetails = db.PTaxMaster.Include(e => e.PTWagesMaster).Where(x => x.Id == data).ToList();
                                     //    foreach (var s in BusiTypeDetails)
                                     //    {
                                     //        s.PTWagesMaster = null;
                                     //        db.PTaxMaster.Attach(s);
                                     //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                     //        db.SaveChanges();
                                     //        TempData["RowVersion"] = s.RowVersion;
                                     //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                     //    }
                                     //}
                                     //if (StatutoryEffectiveMonthslist != null)
                                     //{
                                     //    var a = Utility.StringIdsToListIds(StatutoryEffectiveMonthslist);
                                     //    List<StatutoryEffectiveMonths> sta = new List<StatutoryEffectiveMonths>();
                                     //    foreach (var i in a)
                                     //    {
                                     //        var val = db.StatutoryEffectiveMonths.Find(i);
                                     //        sta.Add(val);
                                     //    }
                                     //    PTM.PTStatutoryEffectiveMonths = sta;
                                     //    var WMtypes = db.PTaxMaster.Include(e => e.PTStatutoryEffectiveMonths).Where(e => e.Id == data).ToList();

                                     //    foreach (var s in WMtypes)
                                     //    {
                                     //        s.PTStatutoryEffectiveMonths = PTM.PTStatutoryEffectiveMonths;
                                     //        db.PTaxMaster.Attach(s);
                                     //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                     //        db.SaveChanges();
                                     //        TempData["RowVersion"] = s.RowVersion;
                                     //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                     //    }
                                     //}
                                     //else
                                     //{
                                     //    var BusiTypeDetails = db.PTaxMaster.Include(e => e.PTStatutoryEffectiveMonths).Where(x => x.Id == data).ToList();
                                     //    foreach (var s in BusiTypeDetails)
                                     //    {
                                     //        s.PTStatutoryEffectiveMonths = null;
                                     //        db.PTaxMaster.Attach(s);
                                     //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                     //        db.SaveChanges();
                                     //        TempData["RowVersion"] = s.RowVersion;
                                     //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                     //    }
                                     //}

                                    
                                     if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                     {
                                         PTaxMaster blog = null; // to retrieve old data
                                         DbPropertyValues originalBlogValues = null;

                                         //using (var context = new DataBaseContext())
                                         //{
                                             blog = db.PTaxMaster.Where(e => e.Id == data).Include(e => e.Frequency)
                                             .Include(e => e.PTWagesMaster)
                                             .Include(e => e.States)
                                             .Include(e => e.PTStatutoryEffectiveMonths)
                                             .SingleOrDefault();
                                             originalBlogValues = db.Entry(blog).OriginalValues;
                                         //}

                                         PTM.DBTrack = new DBTrack
                                         {
                                             CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                             CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                             Action = "M",
                                             ModifiedBy = SessionManager.UserName,
                                             ModifiedOn = DateTime.Now
                                         };

                                        if (PTM.States_Id != 0)
                                        ptaxmaster.States_Id = PTM.States_Id != null ? PTM.States_Id :0;
                                        if (PTM.Frequency_Id != 0)
                                        ptaxmaster.Frequency_Id = PTM.Frequency_Id!= null ? PTM.Frequency_Id :0;
                                        if (PTM.PTWagesMaster_Id != 0)
                                        ptaxmaster.PTWagesMaster_Id = PTM.PTWagesMaster_Id!= null ? PTM.PTWagesMaster_Id :0;

                                         //PTaxMaster ptmaster = new PTaxMaster()
                                         //{
                                             ptaxmaster.Id = data;
                                             ptaxmaster.EffectiveDate = PTM.EffectiveDate;
                                             //EndDate=PTM.EndDate,
                                             //Frequency = PTM.Frequency,
                                             //PTWagesMaster = PTM.PTWagesMaster,
                                             //States = PTM.States,
                                             //PTStatutoryEffectiveMonths = PTM.PTStatutoryEffectiveMonths,
                                             ptaxmaster.DBTrack = PTM.DBTrack;
                                        // };
                                         //db.PTaxMaster.Attach(ptmaster);
                                         //db.Entry(ptmaster).State = System.Data.Entity.EntityState.Modified;
                                         //db.Entry(ptmaster).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                           db.Entry(ptaxmaster).State = System.Data.Entity.EntityState.Modified;
                                         //using (var context = new DataBaseContext())
                                         //{
                                           blog = db.PTaxMaster.Where(e => e.Id == data).Include(e =>e.States)
                                                    .Include(e =>e.Frequency).Include(e=>e.PTWagesMaster)
                                                    .SingleOrDefault();
                                              originalBlogValues = db.Entry(blog).OriginalValues;
                                              db.ChangeTracker.DetectChanges();

                                             var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, PTM.DBTrack);
                                             DT_PTaxMaster DT_PTmaster = (DT_PTaxMaster)obj;
                                             DT_PTmaster.Frequency_Id = blog.Frequency == null ? 0 : blog.Frequency.Id;
                                             DT_PTmaster.States_Id = blog.States == null ? 0 : blog.States.Id;
                                             DT_PTmaster.PTWagesMaster_Id = blog.PTWagesMaster == null ? 0 : blog.PTWagesMaster.Id;
                                             db.Create(DT_PTmaster);
                                             db.SaveChanges();
                                         //}
                                         await db.SaveChangesAsync();
                                         ts.Complete();
                                         Msg.Add("  Record Updated");
                                         return Json(new Utility.JsonReturnClass { Id = PTM.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                         //return Json(new Object[] {, , "Record Updated", JsonRequestBehavior.AllowGet });
                                     }
                                 }
                             }
                             catch (DbUpdateConcurrencyException ex)
                             {
                                 var entry = ex.Entries.Single();
                                 var clientValues = (SalaryHead)entry.Entity;
                                 var databaseEntry = entry.GetDatabaseValues();
                                 if (databaseEntry == null)
                                 {
                                     Msg.Add("Unable to save changes. The record was deleted by another user.");
                                     return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                 }
                                 else
                                 {
                                     var databaseValues = (SalaryHead)databaseEntry.ToObject();
                                     PTM.RowVersion = databaseValues.RowVersion;

                                 }
                             }

                             Msg.Add("Record modified by another user.So refresh it and try to save again.");
                             return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                         }
                     }
                     else
                     {
                         using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                         {

                             PTaxMaster blog = null; // to retrieve old data
                             DbPropertyValues originalBlogValues = null;
                             PTaxMaster Old_Corp = null;

                             //using (var context = new DataBaseContext())
                             //{
                                 blog = db.PTaxMaster.Where(e => e.Id == data).SingleOrDefault();
                                 originalBlogValues = db.Entry(blog).OriginalValues;
                             //}
                             PTM.DBTrack = new DBTrack
                             {
                                 CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                 CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                 Action = "M",
                                 IsModified = blog.DBTrack.IsModified == true ? true : false,
                                 ModifiedBy = SessionManager.UserName,
                                 ModifiedOn = DateTime.Now
                             };
                             PTaxMaster ptaxmaster = db.PTaxMaster.Find(data);
                             //PTaxMaster corp = new PTaxMaster()
                             //{
                             //    Frequency = PTM.Frequency,
                                 //PTWagesMaster = PTM.PTWagesMaster,
                                 //States = PTM.States,
                                 //PTStatutoryEffectiveMonths = PTM.PTStatutoryEffectiveMonths,
                                   ptaxmaster.Id = data;
                                   ptaxmaster.EffectiveDate = PTM.EffectiveDate;
                                   ptaxmaster.DBTrack = PTM.DBTrack;
                                 //RowVersion = (Byte[])TempData["RowVersion"]
                             //};


                             //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                             //using (var context = new DataBaseContext())
                             //{
                                 //var obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, corp, "PTaxMaster", PTM.DBTrack);
                                  //var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                 var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, PTM.DBTrack);
                                 Old_Corp = db.PTaxMaster.Where(e => e.Id == data).Include(e => e.Frequency)
                                 .Include(e => e.PTWagesMaster)
                                 .Include(e => e.States)
                                 .Include(e => e.PTStatutoryEffectiveMonths).SingleOrDefault();
                                 DT_PTaxMaster DT_PTMaster = (DT_PTaxMaster)obj;
                                 DT_PTMaster.Frequency_Id = DBTrackFile.ValCompare(Old_Corp.Frequency, PTM.Frequency);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                 DT_PTMaster.PTWagesMaster_Id = DBTrackFile.ValCompare(Old_Corp.PTWagesMaster, PTM.PTWagesMaster);
                                 DT_PTMaster.States_Id = DBTrackFile.ValCompare(Old_Corp.States, PTM.States);
                                 DT_PTMaster.PTStatutoryEffectiveMonths_Id = DBTrackFile.ValCompare(Old_Corp.PTStatutoryEffectiveMonths, PTM.PTStatutoryEffectiveMonths);
                                 db.Create(DT_PTMaster);
                             //}
                             blog.DBTrack = PTM.DBTrack;
                             db.PTaxMaster.Attach(blog);
                             db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                             db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                             db.SaveChanges();
                             ts.Complete();
                             //   return Json(new Object[] { blog.Id, PTM.States, "Record Updated", JsonRequestBehavior.AllowGet });
                             Msg.Add("  Record Updated");
                             return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = PTM.States.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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


        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
               List<string> Msg = new List<string>();
               using (DataBaseContext db = new DataBaseContext())
               {
                   try
                   {
                       PTaxMaster ptmaster = db.PTaxMaster.Include(e => e.Frequency)
                       .Include(e => e.PTWagesMaster)
                       .Include(e => e.States)
                       .Include(e => e.PTStatutoryEffectiveMonths)
                       .Where(e => e.Id == data).SingleOrDefault();

                       State val = ptmaster.States;
                       LookupValue val1 = ptmaster.Frequency;
                       Wages val2 = ptmaster.PTWagesMaster;

                       var id = int.Parse(Session["CompId"].ToString());
                       var companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == id).SingleOrDefault();
                       companypayroll.PTaxMaster.Where(e => e.Id == ptmaster.Id);
                       companypayroll.PTaxMaster = null;
                       db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                       db.SaveChanges();
                       Msg.Add("  Data removed successfully.  ");
                       return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                       //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                       if (ptmaster.DBTrack.IsModified == true)
                       {
                           using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                           {
                               DBTrack dbT = new DBTrack
                               {
                                   Action = "D",
                                   CreatedBy = ptmaster.DBTrack.CreatedBy != null ? ptmaster.DBTrack.CreatedBy : null,
                                   CreatedOn = ptmaster.DBTrack.CreatedOn != null ? ptmaster.DBTrack.CreatedOn : null,
                                   IsModified = ptmaster.DBTrack.IsModified == true ? true : false
                               };
                               ptmaster.DBTrack = dbT;
                               db.Entry(ptmaster).State = System.Data.Entity.EntityState.Modified;
                               var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, ptmaster.DBTrack);
                               DT_PTaxMaster DT_PTMaster = (DT_PTaxMaster)rtn_Obj;
                               DT_PTMaster.Frequency_Id = ptmaster.Frequency == null ? 0 : ptmaster.Frequency.Id;
                               DT_PTMaster.States_Id = ptmaster.States == null ? 0 : ptmaster.States.Id;
                               db.Create(DT_PTMaster);
                               await db.SaveChangesAsync();
                               ts.Complete();
                               Msg.Add("  Data removed successfully.  ");
                               return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                               //eturn Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                           }
                       }
                       else
                       {

                           using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                           {
                               try
                               {
                                   DBTrack dbT = new DBTrack
                                   {
                                       Action = "D",
                                       ModifiedBy = SessionManager.UserName,
                                       ModifiedOn = DateTime.Now,
                                       CreatedBy = ptmaster.DBTrack.CreatedBy != null ? ptmaster.DBTrack.CreatedBy : null,
                                       CreatedOn = ptmaster.DBTrack.CreatedOn != null ? ptmaster.DBTrack.CreatedOn : null,
                                       IsModified = ptmaster.DBTrack.IsModified == true ? false : false//,
                                   };

                                   db.Entry(ptmaster).State = System.Data.Entity.EntityState.Deleted;
                                   var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);
                                   DT_PTaxMaster DT_PTMaster = (DT_PTaxMaster)rtn_Obj;
                                   DT_PTMaster.Frequency_Id = val1 == null ? 0 : val1.Id;
                                   DT_PTMaster.States_Id = val == null ? 0 : val.Id;
                                   DT_PTMaster.PTWagesMaster_Id = val2 == null ? 0 : val2.Id;
                                   db.Create(DT_PTMaster);

                                   await db.SaveChangesAsync();
                                   ts.Complete();
                                   Msg.Add("  Data removed successfully.  ");
                                   return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                   // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                               }
                               catch (RetryLimitExceededException /* dex */)
                               {
                                   Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                   return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                   // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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
    }
}