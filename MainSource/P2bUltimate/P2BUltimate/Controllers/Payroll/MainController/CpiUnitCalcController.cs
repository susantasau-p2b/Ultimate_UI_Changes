using P2b.Global;
using Payroll;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Threading.Tasks;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Payroll.MainController
{
     [AuthoriseManger]
    public class CpiUnitCalcController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult partial()
        {

            return View("~/Views/Shared/Payroll/_CpiUnitCalcList.cshtml");
        }

        public ActionResult Create(CPIUnitCalc c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string BaseIndex = form["BaseIndex"] == "0" ? "" : form["BaseIndex"];
                    string IndexMaxCeiling = form["IndexMaxCeiling"] == "0" ? "" : form["IndexMaxCeiling"];
                    string Unit = form["Unit"] == "0" ? "" : form["Unit"];



                    if (BaseIndex != "" && BaseIndex != null)
                    {
                        var val = double.Parse(BaseIndex);
                        c.BaseIndex = val;

                    }

                    if (IndexMaxCeiling != "" && IndexMaxCeiling != null)
                    {
                        var val = double.Parse(IndexMaxCeiling);
                        c.IndexMaxCeiling = val;

                    }

                    if (Unit != "" && Unit != null)
                    {
                        var val = double.Parse(Unit);
                        c.Unit = val;

                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            CPIUnitCalc CPIUnitCalc = new CPIUnitCalc()
                            {
                                BaseIndex = c.BaseIndex,
                                IndexMaxCeiling = c.IndexMaxCeiling,
                                Unit = c.Unit,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.CPIUnitCalc.Add(CPIUnitCalc);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, c.DBTrack);
                                //dt_cpu DT_CPIRule = (DT_CPIRule)rtn_Obj;
                                //DT_CPIRule.CPIRuleDeatils_Id = cp.CPIRuleDetails == null ? 0 : cp.CPIRuleDetails.ToString();
                                //DT_PTaxMaster.StatutoryEffectiveMonths_Id = PTMaster.StatutoryEffectiveMonths == null ? 0 : Convert.ToInt32(PTMaster.StatutoryEffectiveMonths); ;
                                //db.Create(DT_CPIRule);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = CPIUnitCalc.Id, Val = CPIUnitCalc.FullDetails.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new Object[] { CPIUnitCalc.Id, CPIUnitCalc.FullDetails.ToString(), "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetLookupDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.CPIUnitCalc.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.CPIUnitCalc.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var list1 = db.CPIRule.ToList().SelectMany(e => e.CPIUnitCalc);
                var list2 = fall.Except(list1);

                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);

            }
            // return View();
        }

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CPIUNITCalc = db.CPIUnitCalc.Where(e => e.Id == data).ToList();
                var r = (from ca in CPIUNITCalc
                         select new
                         {
                             Id = ca.Id,
                             BaseIndex = ca.BaseIndex,
                             IndexMaxCeiling = ca.IndexMaxCeiling,
                             Unit = ca.Unit,

                         }).Distinct();
                var a = db.CPIUnitCalc.Where(e => e.Id == data).Select(e => e.Unit).SingleOrDefault();
                //TempData["RowVersion"] = db.CPIRuleDetails.Find(data).RowVersion;

                return this.Json(new Object[] { r, "", JsonRequestBehavior.AllowGet });
            }

        }


        public ActionResult EditSave(int data, CPIUnitCalc cp, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var id = Convert.ToInt32(data);
                    var db_data = db.CPIUnitCalc
                                    .Where(e => e.Id == id)
                                    .SingleOrDefault();


                    try
                    {
                        db.CPIUnitCalc.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                    }
                    catch (Exception)
                    {

                        throw;
                    }

                    db_data.Id = data;
                    db_data.BaseIndex = cp.BaseIndex;
                    db_data.IndexMaxCeiling = cp.IndexMaxCeiling;
                    db_data.Unit = cp.Unit;


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            try
                            {
                                db.CPIUnitCalc.Attach(db_data);
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = db_data.Id, Val = db_data.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return Json(new Object[] { db_data.Id, db_data.FullDetails, "Record upadated", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateException)
                            {

                                throw;
                            }
                            catch (DBConcurrencyException)
                            {

                                throw;
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

                return View();
            }
        }
    }
}