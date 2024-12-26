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
    public class CountryController : Controller
    {
        //
        // GET: /Country/
        public ActionResult Index()
        {
            return View();
        }

        // GET: /_Country/
        public ActionResult Partial_Country()
        {
            return View("~/Views/Shared/_Country.cshtml");
        }



        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                if (data != "" && data != null && data != "0")
                {
                    var qurey = db.Country.ToList();
                    var selected = (Object)null;
                    if (data2 != "" && data != "0" && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);
                    }

                    SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }

                else
                {
                    if (data2 != "")
                    {
                        int selected = Convert.ToInt32(data2);
                        var qurey = db.Country.Where(e => e.Id == selected).ToList();

                        SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                        return Json(s, JsonRequestBehavior.AllowGet);
                    }
                    return Json("", JsonRequestBehavior.AllowGet);
                }

            }
        }

        public ActionResult Create(Country count)
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
                            count.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            Country country = new Country()
                            {
                                Code = count.Code,
                                Name = count.Name,
                                CurrencyCode = count.CurrencyCode,
                                CurrencyDesc = count.CurrencyDesc,
                                ISDCode = count.ISDCode,
                                DBTrack = count.DBTrack
                            };
                            try
                            {
                                if (db.Country.Any(o => o.Code.ToLower() == count.Code.ToLower()))
                                {
                                    var code = db.Country.Where(o => o.Code.ToLower() == count.Code.ToLower()).SingleOrDefault();
                                    Msg.Add("  Code Already Exists.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return this.Json(new Object[] { "", "", "Code already exists for Country - " + code.Name + ".", JsonRequestBehavior.AllowGet });
                                }

                                if (db.Country.Any(o => o.Name.ToLower() == count.Name.ToLower()))
                                {
                                    Msg.Add("  Name Already Exists.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return this.Json(new Object[] { "", "", "Name already exists.", JsonRequestBehavior.AllowGet });
                                }
                                db.Country.Add(country);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, count.DBTrack);
                                DT_Country DT_Count = (DT_Country)rtn_Obj;
                                db.Create(DT_Count);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = country.Id, Val = country.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] {country.Id,country.Name,"Data Saved Succesfully",JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = count.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
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