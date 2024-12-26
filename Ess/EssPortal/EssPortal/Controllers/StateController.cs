using EssPortal.App_Start;
using EssPortal.Models;
using EssPortal.Security;
using P2b.Global;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace EssPortal.Controllers
{
    public class StateController : Controller
    {
        //
        // GET: /State/
        public ActionResult Index()
        {
            return View();
        }


        // GET: /_State/
        public ActionResult Partial_State()
        {
            return View("~/Views/Shared/_State.cshtml");
        }


        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                // var selected = (Object)null;

                if (data != "" && data != null && data != "0")
                {
                    int? selected = null;
                    var filter = Convert.ToInt32(data);
                    var qurey = db.Country.Include(e => e.States).Where(e => e.Id == filter).SingleOrDefault();
                    if (data2 != "" && data2 != null && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);
                    }
                    SelectList s = new SelectList(qurey.States, "Id", "FullDetails", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (data2 != "")
                    {
                        int selected = Convert.ToInt32(data2);
                        var qurey = db.State.Where(e => e.Id == selected).ToList();

                        SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                        return Json(s, JsonRequestBehavior.AllowGet);
                    }
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
        }


        public ActionResult CreateState(State S, FormCollection form) //Create submit
        {
            //string Code = form["StateCode"] == "0" ? "" : form["StateCode"];
            //string Name = form["StateName"] == "0" ? "" : form["StateName"];


            //if (Code != null)
            //{
            //    if (Code != "")
            //    {
            //        var val = Code;
            //        S.Code = val;
            //    }
            //}

            //if (Name != null)
            //{
            //    if (Name != "")
            //    {

            //        var val = Name;                                      
            //        S.Name = val;
            //    }
            //}

            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.State.Any(o => o.Code == S.Code))
                            {
                                //return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


                            }

                            S.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            State state = new State()
                            {
                                Code = S.Code,
                                Name = S.Name,
                                DBTrack = S.DBTrack
                            };

                            try
                            {
                                db.State.Add(state);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, S.DBTrack);
                                DT_State DT_State = (DT_State)rtn_Obj;
                                db.Create(DT_State);
                                db.SaveChanges();
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


                                ts.Complete();
                                //  return this.Json(new Object[] { state.Id, S.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = state.Id, Val = S.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = S.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                //	return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        //return this.Json(new { msg = errorMsg });
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
        public ActionResult Create(State S, int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    S.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                    Country count = db.Country.Find(data);
                    count.States = new List<State>();
                    if (ModelState.IsValid)
                    {
                        State state = new State()
                        {
                            Code = S.Code,
                            Name = S.Name,
                            DBTrack = S.DBTrack
                        };

                        try
                        {
                            if (db.State.Any(o => o.Code.ToLower() == S.Code.ToLower()))
                            {
                                var code = db.State.Where(o => o.Code.ToLower() == S.Code.ToLower()).SingleOrDefault();
                                Msg.Add("  Code already exists for State - " + code.Name + ".  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { "", "", "Code already exists for State - " + code.Name + ".", JsonRequestBehavior.AllowGet });
                            }
                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.State.Add(state);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, S.DBTrack);
                                DT_State DT_State = (DT_State)rtn_Obj;
                                db.Create(DT_State);
                                ////DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, S.DBTrack);
                                db.SaveChanges();
                                // DBTrackFile.DBTrackSave("Core/P2b.Global", "C", state, null, "State", null);
                                count.States.Add(state);
                                db.State.Attach(state);
                                db.Entry(count).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(count).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                            }
                            //  return Json(new Object[] { state.Id, state.Name, "Data Saved successfully." });
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = state.Id, Val = state.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = S.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                            Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                        //return Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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

    }
}