using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Text;
using P2BUltimate.Models;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Core.MainController
{
     [AuthoriseManger]
    public class StateRegionController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/Stateregion/Stateregion_Index.cshtml");
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Core/_Stateregion.cshtml");
        }

        //public ActionResult PopulateDropDownList(string data,string data2)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        String selected = null;
        //        if (data != "" && data != null && data != "0")
        //        {
        //            var filter = Convert.ToInt32(data);
        //            var qurey = db.State.Include(e => e.StateRegions).Where(e => e.Id == filter).SingleOrDefault();
        //            if (data2 != "" && data2 != null && data2 != "0")
        //            {
        //                selected = data2;
        //            }
        //            SelectList s = new SelectList(qurey.StateRegions, "Id", "FullDetails", selected);
        //            return Json(s, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            if (data2 != "")
        //            {
        //                selected = data2;
        //            }
        //            SelectList s = new SelectList(db.StateRegion, "Id", "FullDetails", selected);
        //            return Json(s, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}
        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                // String selected = null;
                if (data != "" && data != null && data != "0")
                {
                    int? selected = null;
                    var filter = Convert.ToInt32(data);
                    var qurey = db.State.Include(e => e.StateRegions).Where(e => e.Id == filter).SingleOrDefault();
                    if (data2 != "" && data2 != null && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);
                    }
                    SelectList s = new SelectList(qurey.StateRegions, "Id", "FullDetails", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (data2 != "")
                    {
                        int selected = Convert.ToInt32(data2);
                        var qurey = db.StateRegion.Where(e => e.Id == selected).ToList();

                        SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                        return Json(s, JsonRequestBehavior.AllowGet);
                    }
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
        }



        [HttpPost]
        public ActionResult Create(StateRegion SR, int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var state = db.State.Find(data);
                    if (ModelState.IsValid)
                    {
                        SR.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        StateRegion stateRegion = new StateRegion()
                        {
                            Code = SR.Code,
                            Name = SR.Name,
                            DBTrack = SR.DBTrack
                        };

                        state.StateRegions = new List<StateRegion>();
                        try
                        {
                            if (db.StateRegion.Any(o => o.Code.ToLower() == SR.Code.ToLower()))
                            {
                                var code = db.StateRegion.Where(o => o.Code.ToLower() == SR.Code.ToLower()).SingleOrDefault();
                                Msg.Add("Code already exists for StateRegion - " + code.Name + ".");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { "", "", "Code already exists for StateRegion - " + code.Name + ".", JsonRequestBehavior.AllowGet });
                            }
                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.StateRegion.Add(stateRegion);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, SR.DBTrack);
                                DT_StateRegion DT_Reg = (DT_StateRegion)rtn_Obj;
                                db.Create(DT_Reg);
                                ////  DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, SR.DBTrack);
                                db.SaveChanges();
                                //  DBTrackFile.DBTrackSave("Core/P2b.Global", "C", stateRegion, null, "StateRegion", null);
                                state.StateRegions.Add(stateRegion);
                                db.State.Attach(state);
                                db.Entry(state).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = stateRegion.Id, Val = stateRegion.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return Json(new Object[] { stateRegion.Id, stateRegion.Name, "Data Saved successfully." });

                            }
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = SR.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        // return Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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
