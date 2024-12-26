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
using Attendance;
using Leave;
using P2BUltimate.Security;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2b.Global;

namespace P2BUltimate.Controllers.Leave.MainController
{
    public class PrefixSuffixActionController : Controller
    {
        //
        // GET: /PrefixSuffixAction/
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(PrefixSuffixAction L, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                List<string> Msg = new List<string>();
                var FixedDayDebit = form["IsFixedDayDebit"] == "0" ? "" : form["IsFixedDayDebit"];
                var ActualDayDebit  = form["IsActualDayDebit"] == "0" ? "" : form["IsActualDayDebit"];
                var WaiveOffDayDebit = form["IsWaiveOffDayDebit"] == "0" ? "" : form["IsWaiveOffDayDebit"];
                L.IsActualDayDebit_PrefixSuffixAction = Convert.ToBoolean(ActualDayDebit);
                L.IsFixedDayDebit_PrefixSuffixAction = Convert.ToBoolean(FixedDayDebit);
                L.IsWaiveOffDayDebit_PrefixSuffixAction = Convert.ToBoolean(WaiveOffDayDebit);
                try
                {
                   
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            PrefixSuffixAction OBJPrefixSuffixAction = new PrefixSuffixAction()
                            {
                                FixedDebitDay = L.FixedDebitDay,
                                IsActualDayDebit_PrefixSuffixAction = L.IsActualDayDebit_PrefixSuffixAction,
                                IsFixedDayDebit_PrefixSuffixAction = L.IsFixedDayDebit_PrefixSuffixAction,
                                IsWaiveOffDayDebit_PrefixSuffixAction = L.IsWaiveOffDayDebit_PrefixSuffixAction,
                                WaiveOffDebitDay = L.WaiveOffDebitDay,
                                DBTrack = L.DBTrack
                            };
                            try
                            {
                                db.PrefixSuffixAction.Add(OBJPrefixSuffixAction);

                                db.SaveChanges();



                                ts.Complete();
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = OBJPrefixSuffixAction.Id, Val = OBJPrefixSuffixAction.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { OBJLVDP.Id, OBJLVDP.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = L.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        // Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                var Q = db.PrefixSuffixAction
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        FixedDebitDay = e.FixedDebitDay,
                        IsActualDayDebit = e.IsActualDayDebit_PrefixSuffixAction,
                        IsFixedDayDebit = e.IsFixedDayDebit_PrefixSuffixAction,
                        IsWaiveOffDayDebit = e.IsWaiveOffDayDebit_PrefixSuffixAction,
                        WaiveOffDebitDay = e.WaiveOffDebitDay,
                        Action = e.DBTrack.Action
                    }).ToList();


                var Corp = db.PrefixSuffixAction.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        public async Task<ActionResult> EditSave(PrefixSuffixAction ESOBJ, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            var FixedDayDebit = form["IsFixedDayDebit"] == "0" ? "" : form["IsFixedDayDebit"];
            var ActualDayDebit = form["IsActualDayDebit"] == "0" ? "" : form["IsActualDayDebit"];
            var WaiveOffDayDebit = form["IsWaiveOffDayDebit"] == "0" ? "" : form["IsWaiveOffDayDebit"];
            ESOBJ.IsActualDayDebit_PrefixSuffixAction = Convert.ToBoolean(ActualDayDebit);
            ESOBJ.IsFixedDayDebit_PrefixSuffixAction = Convert.ToBoolean(FixedDayDebit);
            ESOBJ.IsWaiveOffDayDebit_PrefixSuffixAction = Convert.ToBoolean(WaiveOffDayDebit);
           

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        PrefixSuffixAction ObjPrefixSufixAction = db.PrefixSuffixAction.Find(data);
                        TempData["CurrRowVersion"] = ObjPrefixSufixAction.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            ESOBJ.DBTrack = new DBTrack
                            {
                                CreatedBy = ObjPrefixSufixAction.DBTrack.CreatedBy == null ? null : ObjPrefixSufixAction.DBTrack.CreatedBy,
                                CreatedOn = ObjPrefixSufixAction.DBTrack.CreatedOn == null ? null : ObjPrefixSufixAction.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            ObjPrefixSufixAction.FixedDebitDay = ESOBJ.FixedDebitDay;
                            ObjPrefixSufixAction.IsActualDayDebit_PrefixSuffixAction = ESOBJ.IsActualDayDebit_PrefixSuffixAction;
                            ObjPrefixSufixAction.IsFixedDayDebit_PrefixSuffixAction = ESOBJ.IsFixedDayDebit_PrefixSuffixAction;
                            ObjPrefixSufixAction.IsWaiveOffDayDebit_PrefixSuffixAction = ESOBJ.IsWaiveOffDayDebit_PrefixSuffixAction;
                            ObjPrefixSufixAction.WaiveOffDebitDay = ESOBJ.WaiveOffDebitDay;
                            ObjPrefixSufixAction.DBTrack = ESOBJ.DBTrack;

                            db.Entry(ObjPrefixSufixAction).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        //}
                        ts.Complete();

                        Msg.Add("  Record Updated");
                        return Json(new Utility.JsonReturnClass { Id = ObjPrefixSufixAction.Id, Val = ObjPrefixSufixAction.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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