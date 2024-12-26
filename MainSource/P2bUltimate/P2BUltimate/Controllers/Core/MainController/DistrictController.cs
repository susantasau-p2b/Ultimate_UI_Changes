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

namespace P2BUltimate.Controllers
{
     [AuthoriseManger]
    public class DistrictController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /District/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/Core/_District.cshtml");
        }
        //public ActionResult PopulateDropDownList(string data,string data2)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        String selected = null;
        //        if (data != "" && data != null && data != "0")
        //        {
        //            var filter = Convert.ToInt32(data);
        //            var qurey = db.StateRegion.Include(e => e.Districts).Where(e => e.Id == filter).SingleOrDefault();
        //            if (data2 != "" && data2 != null && data2 != "0")
        //            {
        //                selected = data2;
        //            }
        //            SelectList s = new SelectList(qurey.Districts, "Id", "FullDetails", selected);
        //            return Json(s, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            if (data2 != "")
        //            {
        //                selected = data2;
        //            }
        //            SelectList s = new SelectList(db.District, "Id", "FullDetails", selected);
        //            return Json(s, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}
        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //String selected = null;
                if (data != "" && data != null && data != "0")
                {
                    int? selected = null;
                    var filter = Convert.ToInt32(data);
                    var qurey = db.StateRegion.Include(e => e.Districts).Where(e => e.Id == filter).SingleOrDefault();
                    if (data2 != "" && data2 != null && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);
                    }
                    SelectList s = new SelectList(qurey.Districts, "Id", "FullDetails", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (data2 != "")
                    {
                        int selected = Convert.ToInt32(data2);
                        var qurey = db.District.Where(e => e.Id == selected).ToList();

                        SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                        return Json(s, JsonRequestBehavior.AllowGet);
                    }
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
        }



        public ActionResult Create(District dist, int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        dist.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        StateRegion stateRegion = db.StateRegion.Find(data);
                        District district = new District()
                        {
                            Code = dist.Code,
                            Name = dist.Name,
                            DBTrack = dist.DBTrack
                        };

                        stateRegion.Districts = new List<District>();
                        try
                        {
                            if (db.District.Any(o => o.Code.ToLower() == dist.Code.ToLower()))
                            {
                                var code = db.City.Where(o => o.Code.ToLower() == dist.Code.ToLower()).SingleOrDefault();
                                Msg.Add("  Code already exists for District - " + code.Name + ". ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { "", "", "Code already exists for District - " + code.Name + ".", JsonRequestBehavior.AllowGet });
                            }
                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.District.Add(district);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dist.DBTrack);
                                DT_District DT_Count = (DT_District)rtn_Obj;
                                db.Create(DT_Count);
                                ////  DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dist.DBTrack);
                                db.SaveChanges();
                                // DBTrackFile.DBTrackSave("Core/P2b.Global", "C", district, null, "District", null);
                                stateRegion.Districts.Add(district);
                                db.StateRegion.Attach(stateRegion);
                                db.Entry(stateRegion).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                ts.Complete();


                            }
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = district.Id, Val = district.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { district.Id,district.Name, "Data saved successfully." });
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = dist.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        //eturn Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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
