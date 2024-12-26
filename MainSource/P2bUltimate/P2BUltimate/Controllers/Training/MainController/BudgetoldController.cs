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
using Training;
using P2BUltimate.Security;


namespace P2BUltimate.Controllers.Training.MainController
{
    [AuthoriseManger]
    public class BudgetController : Controller
    {
        //  private DataBaseContext db = new DataBaseContext();
        // GET: /Budget/
        List<string> Msg = new List<string>();
        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/Budget/Index.cshtml");
        }


        public ActionResult GetLookup(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Budget.ToList();


                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Budget.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                //var list1 = db.FacultySpecialization.ToList();
                //var list2 = fall.Except(list1);
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult partial()
        {
            return View("~/Views/Shared/Training/_Budget.cshtml");
        }
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(Budget NOBJ, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    if (ModelState.IsValid)
                    {
                        //var aa = db.Budget.Any(e => e.BudgetCredit == NOBJ.BudgetCredit && e.BudgetDebit == NOBJ.BudgetDebit);
                        //if (aa == true)
                        //{
                        //    Msg.Add("Code Already Exists.");
                        //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //}
                        using (TransactionScope ts = new TransactionScope())
                        {
                            NOBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId };

                            Budget Budget = new Budget()
                            {
                                BudgetCredit = NOBJ.BudgetCredit,
                                BudgetDebit = NOBJ.BudgetDebit,
                                DBTrack = NOBJ.DBTrack
                            };
                            try
                            {

                                db.Budget.Add(Budget);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = Budget.Id, Val = Budget.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = NOBJ.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
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
                        var errorMsg = sb.ToString();
                        return Json(new Utility.JsonReturnClass { success = false, responseText = errorMsg }, JsonRequestBehavior.AllowGet);

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
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.Budget
                        .Where(e => e.Id == data).Select
                    (e => new
                    {
                        BudgetCredit = e.BudgetCredit,
                        BudgetDebit = e.BudgetDebit,


                        Action = e.DBTrack.Action
                    }).ToList();
                var Corp = db.Budget.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]


        public async Task<ActionResult> EditSave(Budget L, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    bool Auth = form["autho_allow"] == "true" ? true : false;

                    if (Auth == false)
                    {

                        if (ModelState.IsValid)
                        {
                            try
                            {
                                //DbContextTransaction transaction = db.Database.BeginTransaction();
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    Budget blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.Budget.Where(e => e.Id == data)
                                                                .SingleOrDefault();
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


                                    var CurCorp = db.Budget.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        Budget budgetparam = new Budget()
                                        {
                                            BudgetCredit = L.BudgetCredit,
                                            BudgetDebit = L.BudgetDebit,
                                            Id = data,
                                            DBTrack = L.DBTrack
                                        };
                                        db.Budget.Attach(budgetparam);
                                        db.Entry(budgetparam).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(budgetparam).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    }
                                    // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                    using (var context = new DataBaseContext())
                                    {
                                        // var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                        var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                        // DT_Budget DT_Corp = (DT_Budget)obj;
                                        //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;
                                        // db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    var asd = db.Budget.Where(a => a.Id == data).SingleOrDefault();
                                    ts.Complete();
                                    //   return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                                    Msg.Add("Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = asd.Id, Val = asd.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }

                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Budget)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                //else
                                //{
                                //    var databaseValues = (LvEncashReq)databaseEntry.ToObject();
                                //    L.RowVersion = databaseValues.RowVersion;

                                //}
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            Budget blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            Budget Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.Budget.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            L.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            if (TempData["RowVersion"] == null)
                            {
                                TempData["RowVersion"] = blog.RowVersion;
                            }

                            Budget corp = new Budget()
                            {
                                BudgetCredit = L.BudgetCredit,
                                BudgetDebit = L.BudgetDebit,
                                Id = data,
                                DBTrack = L.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];


                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "Budget", L.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.Budget.Where(e => e.Id == data).Include(e => e.FullDetails)
                                   .SingleOrDefault();
                                DT_Budget DT_Corp = (DT_Budget)obj;
                                //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;

                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = L.DBTrack;
                            db.Budget.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            //return Json(new Object[] { blog.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                            Msg.Add("Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = blog.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                int ParentId = 2;
                var jsonData = (Object)null;
                var LKVal = db.Budget.ToList();

                if (gp.IsAutho == true)
                {
                    LKVal = db.Budget.AsNoTracking().Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    LKVal = db.Budget.AsNoTracking().ToList();
                }
                IEnumerable<Budget> IE;
                if (!string.IsNullOrEmpty(gp.searchString))
                {
                    IE = LKVal;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.BudgetCredit, a.BudgetDebit }).Where((e => (e.Id.ToString() == gp.searchString) || (e.BudgetCredit.ToString() == gp.searchString.ToLower()) || (e.BudgetDebit.ToString() == gp.searchString.ToLower())));
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.BudgetCredit, a.BudgetDebit }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = LKVal;
                    Func<Budget, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "BudgetCredit" ? c.BudgetCredit.ToString() :
                                         gp.sidx == "BudgetDebit " ? c.BudgetDebit.ToString() :

                                         "");
                    }

                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.BudgetCredit, a.BudgetDebit }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.BudgetCredit, a.BudgetDebit }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.BudgetCredit, a.BudgetDebit }).ToList();
                    }
                    totalRecords = LKVal.Count();
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
                    total = totalPages,
                    p2bparam = ParentId
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);


            }
            catch (Exception)
            {

                throw;
            }
        }
        public ActionResult Delete(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                Budget Budget = db.Budget.Where(e => e.Id == data).SingleOrDefault();
                try
                {
                    //var selectedValues = SubCategory.SocialActivities;
                    //var lkValue = new HashSet<int>(SubCategory.SocialActivities.Select(e => e.Id));
                    //if (lkValue.Count > 0)
                    //{
                    //    return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                    //}

                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(Budget).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully.  ");
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