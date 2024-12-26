///
/// Created by Tanushri
///

using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
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
using System.Collections.ObjectModel;
using Payroll;
using Training;
using P2BUltimate.Security;
using System.Web.Script.Serialization;

namespace P2BUltimate.Controllers.Training.MainController
{
    public class EmployeeTrainingHistoryController : Controller
    {
        List<string> Msg = new List<string>();
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /Category/

        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/EmployeeTrainingHistory/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Recruitement/_Category.cshtml");
        }
        public ActionResult Partial1()
        {
            return View("~/Views/Shared/Training/_Category.cshtml");
        }


        public class BudgetD
        {
            public Array Bud_Credit { get; set; }
            public Array Bud_Debit { get; set; }
        }










        public ActionResult GetTrainingProgramCalendarLKDetails(List<int> SkipIds)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = new List<TrainingProgramCalendar>();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.YearlyTrainingCalendar.Include(e => e.TrainigProgramCalendar).Include(e => e.TrainigProgramCalendar.Select(t => t.ProgramList)).Where(e => !e.Id.ToString().Contains(a.ToString())).SelectMany(h => h.TrainigProgramCalendar).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var list1 = db.TrainingProgramCalendar.Include(e => e.ProgramList).ToList();
                var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }





        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    Category corporates = db.Category
                        //.Include(e => e.Budget)
                                                       .Include(e => e.SubCategory).Where(e => e.Id == data).SingleOrDefault();

                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    var subcategory = corporates.SubCategory;
                    if (corporates.DBTrack.IsModified == true)
                    {

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            if (subcategory != null)
                            {
                                var objITSection = new HashSet<int>(corporates.SubCategory.Select(e => e.Id));
                                if (objITSection.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                    // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                }
                            }
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? true : false
                            };
                            corporates.DBTrack = dbT;
                            db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corporates.DBTrack);
                            //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            //DT_Corp.Address_Id = corporates.Address == null ? 0 : corporates.Address.Id;
                            //DT_Corp.BusinessType_Id = corporates.BusinessType == null ? 0 : corporates.BusinessType.Id;
                            //DT_Corp.ContactDetails_Id = corporates.ContactDetails == null ? 0 : corporates.ContactDetails.Id;
                            //db.Create(DT_Corp);
                            //// db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        var asq = corporates.SubCategory;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            if (asq != null)
                            {
                                var corpRegion = new HashSet<int>(corporates.SubCategory.Select(e => e.Id));
                                if (corpRegion.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }


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
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, dbT);
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
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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
                IEnumerable<Category> Category = null;
                if (gp.IsAutho == true)
                {
                    Category = db.Category.Include(e => e.SubCategory)
                        //.Include(e=>e.Budget)
                        .Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Category = db.Category.Include(e => e.SubCategory)
                        //.Include(e=>e.Budget)
                        .AsNoTracking().ToList();
                }

                IEnumerable<Category> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Category;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.Code, a.FullDetails }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Code.ToLower() == gp.searchString.ToLower()) || (e.FullDetails.ToLower() == gp.searchString.ToLower()))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Category;
                    Func<Category, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.Code :
                                         gp.sidx == "FullDetails" ? c.FullDetails.ToString() :
                                                                "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.FullDetails) }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.FullDetails) }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.FullDetails }).ToList();
                    }
                    //totalRecords = Category.Count();
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
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }

        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var compid = Convert.ToInt32(Session["CompId"].ToString());
                    var ab = db.CompanyTraining.Where(e => e.Company.Id == compid)
                      .Include(e => e.EmployeeTraining)
                      .Include(e => e.EmployeeTraining.Select(a => a.Employee))
                      .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails))
                      .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(r => r.TrainigDetailSessionInfo)))
                      .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession))))
                      .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession.TrainingProgramCalendar))))
                      .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(r => r.TrainingSchedule)))
                      .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(r => r.TrainingSchedule.TrainingSession)))
                      .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(r => r.TrainingSchedule.TrainingSession.Select(t => t.TrainingProgramCalendar))))
                       .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(r => r.TrainingSchedule.TrainingSession.Select(t => t.TrainingProgramCalendar.ProgramList))))

                      .Include(e => e.EmployeeTraining.Select(a => a.Employee.EmpName)).AsNoTracking().AsParallel()
                      .SingleOrDefault();

                    var all = ab.EmployeeTraining;
                    //for searchs
                    IEnumerable<EmployeeTraining> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => e.Employee.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.Employee.EmpCode.ToString().Contains(param.sSearch))
                                  || (e.Employee.EmpName.FullNameFML.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<EmployeeTraining, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.Employee.EmpCode : "");
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
                            foreach (var item1 in item.TrainingDetails)
                            {
                                
                                if (y == null)
                                {
                                    result.Add(new returndatagridclass
                                    {
                                        Id = item.Id.ToString(),
                                        Code = item.Employee != null ? item.Employee.EmpCode : null,
                                        Name = item.Employee != null && item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : null,

                                    });
                                }
                                else
                                {

                                    var ids = Utility.StringIdsToListIds(y);
                                    foreach (var ca in ids)
                                    {
                                        var id = Convert.ToInt32(ca);

                                        foreach (var item2 in item1.TrainigDetailSessionInfo)
                                        {
                                            if (item2.TrainingSession.TrainingProgramCalendar.Id == id)
                                            {
                                                result.Add(new returndatagridclass
                                                {
                                                    Id = item.Id.ToString(),
                                                    Code = item.Employee != null ? item.Employee.EmpCode : null,
                                                    Name = item.Employee != null && item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : null,

                                                });
                                            }
                                        }
                                    }
                                    
                                }
                               
                            }
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

                                     select new[] { null, Convert.ToString(c.Id), c.Employee.EmpCode, };
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

        public class LoanAdvReqChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Details { get; set; }

        }

        public class DeserializeClass
        {
            public string Id { get; set; }
            public string BatchName { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string ProgramList { get; set; }

        }

        public ActionResult Get_EmpTrHist(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int data1 = 0;
                    if (data != "")
                    {
                        data1 = Convert.ToInt32(data);
                    }
                  

                    var db_data = db.EmployeeTraining.Include(e => e.Employee)
                        .Include(e => e.TrainingDetails)
                        .Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule))
                        .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo))
                        .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession)))
                        .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession.TrainingProgramCalendar)))
                        .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession.TrainingProgramCalendar.ProgramList)))
                        .Where(e => e.Id == data1).SingleOrDefault();
                    //var db_data = db.TrainingDetails.Include(e => e.)
                    //  .Include(e => e.TrainigDetailSessionInfo)
                    //   .Include(e => e.TrainingSchedule)
                    //    .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data != null)
                    {
                        List<DeserializeClass> returndata = new List<DeserializeClass>();

                        // var obj = item2.AppAssignment.AppRatingObjective.Select(e => e.ObjectiveWordings.LookupVal.ToString()).SingleOrDefault();
                        foreach (var item in db_data.TrainingDetails)
                        {
                            foreach (var item1 in item.TrainigDetailSessionInfo)
                            {
                                returndata.Add(new DeserializeClass
                                {
                                    Id = item.Id.ToString(),
                                    BatchName = item.TrainingSchedule.TrainingBatchName,
                                    StartDate = item1.TrainingSession.TrainingProgramCalendar.StartDate.Value.ToShortDateString(),
                                    EndDate = item1.TrainingSession.TrainingProgramCalendar.EndDate.Value.ToShortDateString(),
                                    ProgramList = item1.TrainingSession.TrainingProgramCalendar.ProgramList.FullDetails
                                });
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



        public ActionResult GridDelete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                TrainingProgramCalendar TrainingProgramCalendar = db.TrainingProgramCalendar
                    //.Include(e => e.Budget)
                    .Include(e => e.ProgramList).Where(e => e.Id == data).SingleOrDefault();
                try
                {
                    //var selectedValues = SubCategory.Budget;
                    //var lkValue = new HashSet<int>(SubCategory.Budget.Select(e => e.Id));
                    //if (lkValue.Count > 0)
                    //{
                    //    Msg.Add("Child record exists.Cannot delete.");
                    //    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    //var lkValue1 = new HashSet<int>(TrainingProgramCalendar.ProgramList.Id);
                    //if (lkValue1.Count > 0)
                    //{
                    //    Msg.Add("Child record exists.Cannot delete.");
                    //    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(TrainingProgramCalendar).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                catch (DataException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
            }
        }

    }
}
