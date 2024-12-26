///
/// Created by Kapil
///

using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
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
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Core.MainController
{
     [AuthoriseManger]
    public class InsuranceProductController : Controller
    {
        //
        // GET: /InsuranceProduct/
      
        public ActionResult Index()
        {
            return View("~/Views/Shared/Core/_insuranceproduct.cshtml");
        }


        //private DataBaseContext db = new DataBaseContext();

        /*---------------------------------------------------------- Create ---------------------------------------------- */
        //[HttpPost]
        //public ActionResult Create(InsuranceProduct ip)
        //{
           

        //    if (ModelState.IsValid)
        //    {
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            //if (db.Unit.Any(o => o.Code == u.Code))
        //            //{
        //            //    return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
        //            //}
        //            //u.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
        //            InsuranceProduct ips = new InsuranceProduct()
        //            {
        //              Name = ip.Name,
        //              StartDate = ip.StartDate,
        //              DBTrack=ip.DBTrack
        //            };

        //            try
        //            {
        //                db.InsuranceProduct.Add(ips);
        //                //   DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, u.DBTrack);
        //                db.SaveChanges();
        //                //  DBTrackFile.DBTrackSave("Core/P2b.Global", "C", un, null, "Unit", null);
        //                ts.Complete();
        //                return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
        //                //using (TransactionScope ts = new TransactionScope())
        //                //{
        //                //    db.Unit.Add(un);
        //                //    db.SaveChanges();
        //                //    ts.Complete();
        //                //}

        //                // return Json(new Object[] { null, null, "Data saved successfully." }, JsonRequestBehavior.AllowGet);
        //            }
        //            catch (DbUpdateConcurrencyException)
        //            {
        //                return RedirectToAction("Create", new { concurrencyError = true, id = ip.Id });
        //            }
        //            catch (DataException /* dex */)
        //            {
        //                ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
        //                return View(ip);
        //            }
        //        }
        //    }

        //    else
        //    {
        //        StringBuilder sb = new StringBuilder("");
        //        foreach (ModelState modelState in ModelState.Values)
        //        {
        //            foreach (ModelError error in modelState.Errors)
        //            {
        //                sb.Append(error.ErrorMessage);
        //                sb.Append("." + "\n");
        //            }
        //        }
        //        var errorMsg = sb.ToString();
        //        return this.Json(new { msg = errorMsg });
        //    }

        //}

 
        [HttpPost]
        public ActionResult CreateSave(InsuranceProduct c) //Create submit
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
                            if (db.InsuranceProduct.Any(a => a.InsuranceProductDesc.ToString() == c.InsuranceProductDesc.ToString()
                                //&& a.StartDate.ToString() == c.StartDate.ToString()
                                ))
                            {
                                Msg.Add("Already Exists.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            InsuranceProduct insuranceproducts = new InsuranceProduct()
                            {
                                InsuranceProductDesc = c.InsuranceProductDesc,
                                StartDate = c.StartDate,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.InsuranceProduct.Add(insuranceproducts);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, c.DBTrack);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = insuranceproducts.Id, Val = insuranceproducts.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { insuranceproducts.Id,insuranceproducts.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
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
	}
}