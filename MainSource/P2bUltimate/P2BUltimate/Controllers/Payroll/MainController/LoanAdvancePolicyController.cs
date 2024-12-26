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
using Leave;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class LoanAdvancePolicyController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();
        //
        // GET: /LoanAdvancePolicy/
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(LoanAdvancePolicy c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string LoanSanctionWages = form["LoanSanctionWageslist"] == "0" ? "" : form["LoanSanctionWageslist"];
                    string InterestType = form["InterestTypelist"] == "0" ? "" : form["InterestTypelist"];

                    if (LoanSanctionWages != null && LoanSanctionWages != "")
                    {
                        var val = db.Wages.Find(int.Parse(LoanSanctionWages));
                        c.LoanSanctionWages = val;
                    }

                    if (InterestType != null && InterestType != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(InterestType));
                        c.InterestType = val;
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.LoanAdvancePolicy.Any(o => o.EffectiveDate == c.EffectiveDate))
                            {
                                Msg.Add("  record Already exists  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Record Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            LoanAdvancePolicy LoanAdvPolicy = new LoanAdvancePolicy()
                            {
                                EffectiveDate = c.EffectiveDate,
                                EndDate = c.EndDate,
                                FineAmt = c.FineAmt,
                                GovtIntRate = c.GovtIntRate,
                                IntAppl = c.IntAppl,
                                InterestType = c.InterestType,
                                IntRate = c.IntRate,
                                IsFine = c.IsFine,
                                IsFixAmount = c.IsFixAmount,
                                IsLoanLimit = c.IsLoanLimit,
                                IsOnWages = c.IsOnWages,
                                IsPerkOnInt = c.IsPerkOnInt,
                                LoanSanctionWages = c.LoanSanctionWages,
                                MaxLoanAmount = c.MaxLoanAmount,
                                NoOfTimes = c.NoOfTimes,
                                YrsOfServ = c.YrsOfServ,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.LoanAdvancePolicy.Add(LoanAdvPolicy);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, c.DBTrack);
                                //DT_LoanAdvancePolicy DT_LoanAdvPolicy = (DT_LoanAdvancePolicy)rtn_Obj;
                                //DT_LoanAdvPolicy.InterestType_Id = c.InterestType == null ? 0 : c.InterestType.Id;
                                //DT_LoanAdvPolicy.LoanSanctionWages_Id = c.LoanSanctionWages == null ? 0 : c.LoanSanctionWages.Id;
                                //db.Create(DT_LoanAdvPolicy);
                                db.SaveChanges();
                                ts.Complete();

                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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

                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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
                var Q = db.LoanAdvancePolicy
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        EffectiveDate = e.EffectiveDate,
                        EndDate = e.EndDate,
                        FineAmt = e.FineAmt,
                        GovtIntRate = e.GovtIntRate,
                        IntAppl = e.IntAppl,
                        InterestType_Id = e.InterestType_Id == null ? 0 : e.InterestType_Id,
                        IntRate = e.IntRate,
                        IsFine = e.IsFine,
                        IsFixAmount = e.IsFixAmount,
                        IsLoanLimit = e.IsLoanLimit,
                        IsOnWages = e.IsOnWages,
                        IsPerkOnInt = e.IsPerkOnInt,
                        MaxLoanAmount = e.MaxLoanAmount,
                        NoOfTimes = e.NoOfTimes,
                        YrsOfServ = e.YrsOfServ,
                        Action = e.DBTrack.Action
                    }).ToList();




                var LoanAdvancePolicy = db.LoanAdvancePolicy.Find(data);
                TempData["RowVersion"] = LoanAdvancePolicy.RowVersion;
                var Auth = LoanAdvancePolicy.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        public async Task<ActionResult> EditSave(LoanAdvancePolicy c, int data, FormCollection form) // Edit submit 
        {
            List<string> Msg = new List<string>();
            string LoanSanctionWages = form["LoanSanctionWageslist"] == "0" ? "" : form["LoanSanctionWageslist"];
            string InterestType = form["InterestTypelist"] == "0" ? "" : form["InterestTypelist"];

            c.InterestType_Id = InterestType != null && InterestType != "" ? int.Parse(InterestType) : 0;
            c.LoanSanctionWages_Id = LoanSanctionWages != null && LoanSanctionWages != "" ? int.Parse(LoanSanctionWages) : 0;



            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        LoanAdvancePolicy LoanPolicy = db.LoanAdvancePolicy.Find(data);
                        TempData["CurrRowVersion"] = LoanPolicy.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = LoanPolicy.DBTrack.CreatedBy == null ? null : LoanPolicy.DBTrack.CreatedBy,
                                CreatedOn = LoanPolicy.DBTrack.CreatedOn == null ? null : LoanPolicy.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            LoanPolicy.EffectiveDate = c.EffectiveDate;
                            LoanPolicy.EndDate = c.EndDate;
                            LoanPolicy.FineAmt = c.FineAmt;
                            LoanPolicy.GovtIntRate = c.GovtIntRate;
                            LoanPolicy.IntAppl = c.IntAppl;
                            LoanPolicy.InterestType_Id = c.InterestType_Id;
                            LoanPolicy.IntRate = c.IntRate;
                            LoanPolicy.IsFine = c.IsFine;
                            LoanPolicy.IsFixAmount = c.IsFixAmount;
                            LoanPolicy.IsLoanLimit = c.IsLoanLimit;
                            LoanPolicy.IsOnWages = c.IsOnWages;
                            LoanPolicy.IsPerkOnInt = c.IsPerkOnInt;
                            //if (LoanPolicy.LoanSanctionWages_Id != 0)
                            //{
                            //     LoanPolicy.LoanSanctionWages_Id = c.LoanSanctionWages_Id;
                            //}
                         
                            LoanPolicy.MaxLoanAmount = c.MaxLoanAmount;
                            LoanPolicy.NoOfTimes = c.NoOfTimes;
                            LoanPolicy.YrsOfServ = c.YrsOfServ;
                            LoanPolicy.Id = data;
                            LoanPolicy.DBTrack = c.DBTrack; 
                            db.Entry(LoanPolicy).State = System.Data.Entity.EntityState.Modified;
                            // db.SaveChanges();

                            //using (var context = new DataBaseContext())
                            //{
                            LoanAdvancePolicy blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;

                            blog = db.LoanAdvancePolicy.Where(e => e.Id == data).Include(e => e.InterestType)
                                                    .Include(e => e.LoanSanctionWages).FirstOrDefault();
                            originalBlogValues = db.Entry(blog).OriginalValues;
                            db.ChangeTracker.DetectChanges();
                            var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                            DT_LoanAdvancePolicy DT_Corp = (DT_LoanAdvancePolicy)obj;
                            DT_Corp.InterestType_Id = blog.InterestType == null ? 0 : blog.InterestType.Id;
                            //if (blog.LoanSanctionWages != null)
                            //{
                            //    DT_Corp.LoanSanctionWages_Id = blog.LoanSanctionWages == null ? 0 : blog.LoanSanctionWages.Id;
                            //}
                            


                            db.Create(DT_Corp);
                            db.SaveChanges();
                        }
                        //}
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { Id = data, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }

    }
}