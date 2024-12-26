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
    public class TalukaController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Taluka/Taluka_Index.cshtml");
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Core/_Taluka.cshtml");
        }

        //public ActionResult PopulateDropDownList(string data, string data2)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        String selected = null;
        //        if (data != "" && data != null && data != "0")
        //        {
        //            var filter = Convert.ToInt32(data);
        //            var qurey = db.District.Include(e => e.Talukas).Where(e => e.Id == filter).SingleOrDefault();
        //            if (data2 != "" && data2 != null && data2 != "0")
        //            {
        //                selected = data2;
        //            }
        //            SelectList s = new SelectList(qurey.Talukas, "Id", "FullDetails", selected);
        //            return Json(s, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            if (data2 != "")
        //            {
        //                selected = data2;
        //            }
        //            SelectList s = new SelectList(db.Taluka, "Id", "FullDetails", selected);
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
                      var qurey = db.District.Include(e => e.Talukas).Where(e => e.Id == filter).SingleOrDefault();
                      if (data2 != "" && data2 != null && data2 != "0")
                      {
                          selected = Convert.ToInt32(data2);
                      }
                      SelectList s = new SelectList(qurey.Talukas, "Id", "FullDetails", selected);
                      return Json(s, JsonRequestBehavior.AllowGet);
                  }
                else
                {
                    if (data2 != "")
                    {
                        int selected = Convert.ToInt32(data2);
                        var qurey = db.Taluka.Where(e => e.Id == selected).ToList();

                        SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                        return Json(s, JsonRequestBehavior.AllowGet);
                    }
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
        }


        [HttpPost]
        public ActionResult Create(Taluka TL, int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        TL.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        District dist = db.District.Find(data);
                        Taluka tal = new Taluka()
                        {
                            Code = TL.Code,
                            Name = TL.Name,
                            DBTrack = TL.DBTrack
                        };

                        dist.Talukas = new List<Taluka>();
                        try
                        {
                            if (db.Taluka.Any(o => o.Code.ToLower() == TL.Code.ToLower()))
                            {
                                var code = db.Taluka.Where(o => o.Code.ToLower() == TL.Code.ToLower()).SingleOrDefault();
                                Msg.Add("Code already exists for Taluka - " + code.Name + ".");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { "", "", "Code already exists for Taluka - " + code.Name + ".", JsonRequestBehavior.AllowGet });
                            }
                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.Taluka.Add(tal);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, TL.DBTrack);
                                DT_Taluka DT_Tal = (DT_Taluka)rtn_Obj;
                                db.Create(DT_Tal);
                                //// DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, TL.DBTrack);
                                db.SaveChanges();
                                //  DBTrackFile.DBTrackSave("Core/P2b.Global", "C", tal, null, "Taluka", null);
                                dist.Talukas.Add(tal);
                                db.District.Attach(dist);
                                db.Entry(dist).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                ts.Complete();
                            }
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = tal.Id, Val = tal.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] {tal.Id,tal.Name, "Data Saved Successfully." });
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = TL.Id });
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
    }
}
