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

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class JVNonStandardEmpController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /JVNonStandardEmp/

        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/Range/Index.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/Payroll/_JVNonStandardEmp.cshtml");
        }

        public ActionResult JVNonStandardEmpCreate(JVNonStandardEmp jv, FormCollection form)
        {
            List<string> Msg = new List<string>();
            bool Irregular = form["Irregular"] != null ? Convert.ToBoolean(form["Irregular"]) : false;
            //if (Irregular == true)
            //{
            string BranchIn_table = form["EmpIn-table"] != null ? form["EmpIn-table"] : null;
            string EmpLocIn_table = form["EmpLocIn-table"] != null ? form["EmpLocIn-table"] : null;
            int AccountType_drop = form["AccountType_drop"] != null ? Convert.ToInt32(form["AccountType_drop"]) : 0;
            //string JVLocIdForNonStd = form["JVLocIdForNonStd"];
            //Int32 Struct_Sel = Convert.ToInt32(form["Group"]);
            //string job_table = form["job-table"] != null ? form["job-table"] : null;
            // string jobposition_table = form["jobposition-table"] != null ? form["jobposition-table"] : null;
            List<int> Emplist = new List<int>();
            // List<int> Loclist = new List<int>();
            // Int32 BranchOut = 0;
            Int32 Job = 0;
            //Int32 JobPosition = 0;
            if (BranchIn_table != null)
            {
                Emplist = Utility.StringIdsToListIds(BranchIn_table);
            }

          
            //if (EmpLocIn_table != null)
            //{
            //    Loclist = Utility.StringIdsToListIds(EmpLocIn_table);
            //}
            //if (BranchOut_table != null)
            //{
            //    var temp = Utility.StringIdsToListIds(BranchOut_table);
            //    BranchOut = temp[0];
            //}

            JVNonStandardEmp JVNonStandardEmp = new JVNonStandardEmp();
            JVNonStandardEmp.ProductCode = jv.ProductCode;
            JVNonStandardEmp.AccountNo = jv.AccountNo;
            JVNonStandardEmp.SubAccountNo = jv.SubAccountNo;
            JVNonStandardEmp.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

            try
            {
                // List<string> emppass=new List<string>();
                string emppass = "";
                // string emppass1 = EmpLocIn_table;
                using (TransactionScope ts = new TransactionScope())
                {
                    using (DataBaseContext db = new DataBaseContext())
                    {
                        if (EmpLocIn_table != null)
                        {
                            int locid = Convert.ToInt32(EmpLocIn_table);
                            var loc = db.Branch.Where(q => q.Id == locid).SingleOrDefault();
                            JVNonStandardEmp.Branch = loc;
                        }
                        foreach (var i in Emplist)
                        {

                            var tempemppayroll = db.EmployeePayroll.Include(q => q.Employee).Include(q => q.Employee.EmpName).Where(q => q.Id == i).SingleOrDefault();
                            if (AccountType_drop != 0)
                            {
                                var value = db.LookupValue.Find(AccountType_drop);
                                JVNonStandardEmp.AccountType = value;
                            }
                            //if (db.JVNonStandardEmp.Include(q => q.EmployeePayroll).Any(a => a.EmployeePayroll.Id == i && a.ProductCode == jv.ProductCode))
                            //{
                            //    return Json(new { status = false, responseText = "Data already exixt for " + tempemppayroll.Employee.FullDetails }, JsonRequestBehavior.AllowGet);
                            //}

                            //    emppass.Add(tempemppayroll.Employee.FullDetails);
                            // emppass.Add(tempemppayroll.Id.ToString());
                            JVNonStandardEmp.EmployeePayroll = tempemppayroll;
                            db.JVNonStandardEmp.Add(JVNonStandardEmp);
                            db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);

                            if (emppass == "")
                            {
                                emppass = JVNonStandardEmp.Id.ToString();
                            }
                            else
                            {
                                emppass = emppass + "," + JVNonStandardEmp.Id.ToString();
                            }
                        }

                    }
                    ts.Complete();
                    var datad = new
                    {
                        emp = emppass,
                        loc = EmpLocIn_table
                    };
                    return Json(new { status = true, data = datad, responseText = "Data Save Successfully" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                // return RedirectToAction("Create", new { concurrencyError = true, id = jv.Id });
            }
            catch (Exception ex)
            {
                throw ex;
                Msg.Add(ex.Message);
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
            // }
            //else
            //{
            //    Msg.Add("Select only one record");
            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            //}
            return null;
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(Range COBJ, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.Range.Any(o => o.RangeTo == COBJ.RangeTo))
                            {
                                //  return this.Json(new Object[] { null, null, "Record already exists.", JsonRequestBehavior.AllowGet });
                                Msg.Add("Record already exists..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            Range Range = new Range()
                            {

                                RangeFrom = COBJ.RangeFrom,
                                RangeTo = COBJ.RangeTo,
                                EmpShareAmount = COBJ.EmpShareAmount,
                                EmpSharePercentage = COBJ.EmpSharePercentage,
                                CompShareAmount = COBJ.CompShareAmount,
                                CompSharePercentage = COBJ.CompSharePercentage,
                                DBTrack = COBJ.DBTrack
                            };
                            try
                            {
                                db.Range.Add(Range);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, COBJ.DBTrack);
                                DT_Range DT_OBJ = (DT_Range)rtn_Obj;
                                db.Create(DT_OBJ);
                                db.SaveChanges();

                                ts.Complete();
                                //  return this.Json(new Object[] { Range.Id, Range.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = Range.Id, Val = Range.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = COBJ.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        Msg.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new { msg = errorMsg });
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

                var Range = db.Range
                                    .Where(e => e.Id == data).ToList();
                var r = (from ca in Range
                         select new
                         {

                             Id = ca.Id,
                             RangeFrom = ca.RangeFrom,
                             RangeTo = ca.RangeTo,
                             EmpShareAmount = ca.EmpShareAmount,
                             EmpSharePercentage = ca.EmpSharePercentage,
                             CompShareAmount = ca.CompShareAmount,
                             CompSharePercentage = ca.CompSharePercentage,
                             Action = ca.DBTrack.Action
                         }).Distinct();

                var a = "";

                var W = db.DT_Range
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         RangeFrom = e.RangeFrom == null ? 0.0 : e.RangeFrom,
                         RangeTo = e.RangeTo == null ? 0.0 : e.RangeTo,
                         EmpShareAmount = e.EmpShareAmount == null ? 0.0 : e.EmpShareAmount,
                         EmpSharePercentage = e.EmpSharePercentage == null ? 0.0 : e.EmpSharePercentage,
                         CompShareAmount = e.CompShareAmount == null ? 0.0 : e.CompShareAmount,
                         CompSharePercentage = e.CompSharePercentage == null ? 0.0 : e.CompSharePercentage,
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var LKup = db.Range.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, a, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }




        //[HttpPost]
        ////[ValidateAntiForgeryToken]
        //public async Task<ActionResult> EditSave(Range ESOBJ, FormCollection form, int data)
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {

        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //            if (Auth == false)
        //            {
        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {
        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            Range blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.Range.Where(e => e.Id == data)
        //                                                        .SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            ESOBJ.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };

        //                            int a = EditS(data, ESOBJ, ESOBJ.DBTrack);



        //                            using (var context = new DataBaseContext())
        //                            {
        //                                var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
        //                                DT_Range DT_OBJ = (DT_Range)obj;
        //                                db.Create(DT_OBJ);
        //                                db.SaveChanges();
        //                            }
        //                            await db.SaveChangesAsync();
        //                            ts.Complete();

        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = data, Val = ESOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                            //   return Json(new Object[] { ESOBJ.Id, ESOBJ.RangeFrom, "Record Updated", JsonRequestBehavior.AllowGet });

        //                        }
        //                    }

        //                    //catch (DbUpdateException e) { throw e; }
        //                    //catch (DataException e) { throw e; }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (Range)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {
        //                            //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                            Msg.Add(" Unable to save changes. The record was deleted by another user. ");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (Range)databaseEntry.ToObject();
        //                            ESOBJ.RowVersion = databaseValues.RowVersion;

        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    StringBuilder sb = new StringBuilder("");
        //                    foreach (ModelState modelState in ModelState.Values)
        //                    {
        //                        foreach (ModelError error in modelState.Errors)
        //                        {
        //                            sb.Append(error.ErrorMessage);
        //                            sb.Append("." + "\n");
        //                        }
        //                    }
        //                    var errorMsg = sb.ToString();
        //                    Msg.Add(errorMsg);
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
        //                }
        //            }


        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    Range blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    Range Old_Obj = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.Range.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }
        //                    ESOBJ.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };
        //                    Range corp = new Range()
        //                    {
        //                        RangeFrom = ESOBJ.RangeFrom,
        //                        RangeTo = ESOBJ.RangeTo,
        //                        EmpShareAmount = ESOBJ.EmpShareAmount,
        //                        EmpSharePercentage = ESOBJ.EmpSharePercentage,
        //                        CompShareAmount = ESOBJ.CompShareAmount,
        //                        CompSharePercentage = ESOBJ.CompSharePercentage,
        //                        Id = data,
        //                        DBTrack = ESOBJ.DBTrack,
        //                        RowVersion = (Byte[])TempData["RowVersion"]
        //                    };


        //                    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        var obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, corp, "Range", ESOBJ.DBTrack);
        //                        // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                        Old_Obj = context.Range.Where(e => e.Id == data)
        //                            .SingleOrDefault();
        //                        DT_Range DT_Corp = (DT_Range)obj;
        //                        db.Create(DT_Corp);
        //                        //db.SaveChanges();
        //                    }
        //                    blog.DBTrack = ESOBJ.DBTrack;
        //                    db.Range.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    // return Json(new Object[] { blog.Id, ESOBJ.RangeFrom, "Record Updated", JsonRequestBehavior.AllowGet });
        //                    Msg.Add("  Record Updated");
        //                    return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = ESOBJ.RangeFrom.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                }

        //            }
        //            return View();
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

        [HttpPost]
        public async Task<ActionResult> EditSave(Range c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<String> Msg = new List<String>();
                try
                {
                    // string Addrs = form["SalaryComponentList"] == "0" ? "" : form["SalaryComponentList"];

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    //List<SalaryHead> ObjITsection = new List<SalaryHead>();

                    Range pd = null;
                    pd = db.Range.Where(q => q.Id == data).SingleOrDefault();
                    //if (Addrs != "")
                    //{
                    //    var ids = Utility.StringIdsToListIds(Addrs);
                    //    foreach (var ca in ids)
                    //    {
                    //        var value = db.SalaryHead.Find(ca);
                    //        ObjITsection.Add(value);
                    //        pd.SalaryHead = ObjITsection;

                    //    }
                    //}
                    //else
                    //{
                    //    pd.SalaryHead = null;

                    //}
                    if (ModelState.IsValid)
                    {
                        //DbContextTransaction transaction = db.Database.BeginTransaction();

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            Range blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.Range.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }

                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            var m1 = db.Range.Where(e => e.Id == data).ToList();
                            foreach (var s in m1)
                            {
                                // s.AppraisalPeriodCalendar = null;
                                db.Range.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }

                            var CurCorp = db.Range.Find(data);
                            TempData["CurrRowVersion"] = CurCorp.RowVersion;
                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;

                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                            {
                                Range corp = new Range()
                                {
                                    RangeFrom = c.RangeFrom,
                                    RangeTo = c.RangeTo,
                                    EmpShareAmount = c.EmpShareAmount,
                                    EmpSharePercentage = c.EmpSharePercentage,
                                    CompShareAmount = c.CompShareAmount,
                                    CompSharePercentage = c.CompSharePercentage,
                                    Id = data,
                                    DBTrack = c.DBTrack
                                };

                                db.Range.Attach(corp);
                                db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            }
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            ts.Complete();

                            Msg.Add("Record Updated Successfully.");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = blog.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                    }

                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var clientValues = (Range)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (Range)databaseEntry.ToObject();
                        c.RowVersion = databaseValues.RowVersion;

                    }
                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                return View();
            }
        }



        //public int EditS(int data, Range ESOBJ, DBTrack dbT)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var CurOBJ = db.Range.Find(data);
        //        TempData["CurrRowVersion"] = CurOBJ.RowVersion;
        //        db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
        //        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //        {
        //            ESOBJ.DBTrack = dbT;
        //            Range ESIOBJ = new Range()
        //            {
        //                Id = data,
        //                RangeFrom = ESOBJ.RangeFrom,
        //                RangeTo = ESOBJ.RangeTo,
        //                EmpShareAmount = ESOBJ.EmpShareAmount,
        //                EmpSharePercentage = ESOBJ.EmpSharePercentage,
        //                CompShareAmount = ESOBJ.CompShareAmount,
        //                CompSharePercentage = ESOBJ.CompSharePercentage,
        //                DBTrack = ESOBJ.DBTrack
        //            };

        //            db.Range.Attach(ESIOBJ);
        //            db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //            return 1;
        //        }
        //        return 0;
        //    }
        //}



        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Delete(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                Range Range = db.Range.Where(e => e.Id == data).SingleOrDefault();
                try
                {
                    //var selectedValues = Range.SocialActivities;
                    //var lkValue = new HashSet<int>(Range.SocialActivities.Select(e => e.Id));
                    //if (lkValue.Count > 0)
                    //{
                    //    return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                    //}

                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(Range).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    //  return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                }

                catch (DataException /* dex */)
                {
                    //   return this.Json(new Object[] { "", "Unable to delete. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
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
                var LKVal = db.Range.ToList();

                if (gp.IsAutho == true)
                {
                    LKVal = db.Range.AsNoTracking().Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    LKVal = db.Range.AsNoTracking().ToList();
                }


                IEnumerable<Range> IE;
                if (!string.IsNullOrEmpty(gp.searchString))
                {
                    IE = LKVal;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.RangeFrom, a.RangeTo, a.EmpShareAmount, a.EmpSharePercentage, a.CompShareAmount }).Where((e => (e.Id.ToString() == gp.searchString) || (e.RangeFrom.ToString() == gp.searchString.ToLower()) || (e.RangeTo.ToString() == gp.searchString.ToLower()) || (e.EmpShareAmount.ToString() == gp.searchString.ToLower()) || (e.EmpSharePercentage.ToString() == gp.searchString.ToLower()) || (e.CompShareAmount.ToString() == gp.searchString.ToLower())));
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.RangeFrom, a.RangeTo, a.EmpShareAmount, a.EmpSharePercentage, a.CompShareAmount }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = LKVal;
                    Func<Range, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "RangeFrom" ? c.RangeFrom.ToString() :
                                         gp.sidx == "RangeTo " ? c.RangeTo.ToString() :
                                         gp.sidx == "EmpShareAmount " ? c.EmpShareAmount.ToString() :
                                         gp.sidx == "EmpSharePercentage " ? c.EmpSharePercentage.ToString() :
                                         gp.sidx == "CompShareAmount " ? c.CompShareAmount.ToString() :
                                         "");
                    }

                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.RangeFrom, a.RangeTo, a.EmpShareAmount, a.EmpSharePercentage, a.CompShareAmount }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.RangeFrom, a.RangeTo, a.EmpShareAmount, a.EmpSharePercentage, a.CompShareAmount }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.RangeFrom, a.RangeTo, a.EmpShareAmount, a.EmpSharePercentage, a.CompShareAmount }).ToList();
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
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost]
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
                            Range ESI = db.Range
                                .FirstOrDefault(e => e.Id == auth_id);

                            ESI.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = ESI.DBTrack.ModifiedBy != null ? ESI.DBTrack.ModifiedBy : null,
                                CreatedBy = ESI.DBTrack.CreatedBy != null ? ESI.DBTrack.CreatedBy : null,
                                CreatedOn = ESI.DBTrack.CreatedOn != null ? ESI.DBTrack.CreatedOn : null,
                                IsModified = ESI.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.Range.Attach(ESI);
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(ESI).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, ESI.DBTrack);
                            DT_Range DT_OBJ = (DT_Range)rtn_Obj;

                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            //return Json(new Object[] { ESI.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = ESI.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else if (auth_action == "M")
                    {

                        Range Old_OBJ = db.Range
                                                .Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_Range Curr_OBJ = db.DT_Range
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_OBJ != null)
                        {
                            Range Range = new Range();

                            Range.RangeFrom = Curr_OBJ.RangeFrom == 0.0 ? Old_OBJ.RangeFrom : Curr_OBJ.RangeFrom;
                            Range.RangeTo = Curr_OBJ.RangeTo == 0.0 ? Old_OBJ.RangeTo : Curr_OBJ.RangeTo;
                            Range.EmpShareAmount = Curr_OBJ.EmpShareAmount == 0.0 ? Old_OBJ.EmpShareAmount : Curr_OBJ.EmpShareAmount;
                            Range.EmpSharePercentage = Curr_OBJ.EmpSharePercentage == 0.0 ? Old_OBJ.EmpSharePercentage : Curr_OBJ.EmpSharePercentage;
                            Range.CompShareAmount = Curr_OBJ.CompShareAmount == 0.0 ? Old_OBJ.CompShareAmount : Curr_OBJ.CompShareAmount;
                            Range.CompSharePercentage = Curr_OBJ.CompSharePercentage == 0.0 ? Old_OBJ.CompSharePercentage : Curr_OBJ.CompSharePercentage;
                            //      corp.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        Range.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
                                            CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_OBJ.DBTrack.ModifiedBy == null ? null : Old_OBJ.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_OBJ.DBTrack.ModifiedOn == null ? null : Old_OBJ.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        //int a = EditS(auth_id, Range, Range.DBTrack);

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        //  return Json(new Object[] { Range.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = Range.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (Range)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                    else
                                    {
                                        var databaseValues = (Range)databaseEntry.ToObject();
                                        Range.RowVersion = databaseValues.RowVersion;
                                    }
                                }

                                // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }
                        else
                            //return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Range corp = db.Range.Find(auth_id);
                            Range ESI = db.Range.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                            //Address add = corp.Address;
                            //ContactDetails conDet = corp.ContactDetails;
                            //SocialActivities val = corp.BusinessType;

                            ESI.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = ESI.DBTrack.ModifiedBy != null ? ESI.DBTrack.ModifiedBy : null,
                                CreatedBy = ESI.DBTrack.CreatedBy != null ? ESI.DBTrack.CreatedBy : null,
                                CreatedOn = ESI.DBTrack.CreatedOn != null ? ESI.DBTrack.CreatedOn : null,
                                IsModified = false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.Range.Attach(ESI);
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, ESI.DBTrack);
                            DT_Range DT_OBJ = (DT_Range)rtn_Obj;
                            //DT_OBJ.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            //DT_OBJ.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                            //DT_OBJ.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            //    return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
    }
}

