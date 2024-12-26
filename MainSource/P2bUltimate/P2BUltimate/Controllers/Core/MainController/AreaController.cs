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
    public class AreaController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/Area/Area_Index.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/Core/_Area.cshtml");
        }
        //public ActionResult PopulateDropDownList(string data, string data2)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        String selected = null;
        //        if (data != "" && data != null && data != "0")
        //        {
        //            var filter = Convert.ToInt32(data);
        //            var qurey = db.City.Include(e => e.Areas).Where(e => e.Id == filter).SingleOrDefault();
        //            if (data2 != "" && data2 != null && data2 != "0")
        //            {
        //                selected = data2;
        //            }

        //            SelectList s = new SelectList(qurey.Areas, "Id", "FullDetails", selected);
        //            return Json(s, JsonRequestBehavior.AllowGet);
        //        }
        //        {
        //            if (data2 != "")
        //            {
        //                selected = data2;
        //            }
        //            SelectList s = new SelectList(db.Area, "Id", "FullDetails", selected);
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
                    var qurey = db.City.Include(e => e.Areas).Where(e => e.Id == filter).SingleOrDefault();
                    if (data2 != "" && data2 != null && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);
                    }

                    SelectList s = new SelectList(qurey.Areas, "Id", "FullDetails", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (data2 != "")
                    {
                        int selected = Convert.ToInt32(data2);
                        var qurey = db.Area.Where(e => e.Id == selected).ToList();

                        SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                        return Json(s, JsonRequestBehavior.AllowGet);
                    }
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public ActionResult Create(Area A, int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        A.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        City city = db.City.Find(data);
                        Area area = new Area()
                        {
                            Code = A.Code,
                            Name = A.Name,
                            PinCode = A.PinCode,
                            DBTrack = A.DBTrack
                        };

                        city.Areas = new List<Area>();


                        try
                        {

                            if (db.Area.Any(o => o.Code.ToLower() == A.Code.ToLower()))
                            {
                                var code = db.Area.Where(o => o.Code.ToLower() == A.Code.ToLower()).SingleOrDefault();
                                Msg.Add("Code already exists for Area - " + code.Name + ".");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { "", "", "Code already exists for Area - " + code.Name + ".", JsonRequestBehavior.AllowGet });
                            }


                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.Area.Add(area);
                                ////  DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, A.DBTrack);
                                //db.SaveChanges();
                                ///// DBTrackFile.DBTrackSave("Core/P2b.Global", "C", city, null, "Area", null);
                                city.Areas.Add(area);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, A.DBTrack);
                                DT_Area DT_Area = (DT_Area)rtn_Obj;
                                db.Create(DT_Area);
                                db.SaveChanges();
                                db.City.Attach(city);
                                db.Entry(city).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                ts.Complete();
                            }
                            //  return Json(new Object[] {area.Id,area.Name,"Data Saved successfully." });
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = area.Id, Val = area.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = A.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            //    return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
