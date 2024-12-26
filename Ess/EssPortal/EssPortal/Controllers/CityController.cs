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
    public class CityController : Controller
    {
        //
        // GET: /City/
        public ActionResult Index()
        {
            return View();
        }

        // GET: /_City/
        public ActionResult Partial_City()
        {
            return View("~/Views/Shared/_City.cshtml");
        }


        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //String selected = null;
                if (data != "" && data != null && data != "0")
                {
                    int? selected = null;
                    var filter = Convert.ToInt32(data);
                    var qurey = db.Taluka.Include(e => e.Cities).Where(e => e.Id == filter).SingleOrDefault();
                    if (data2 != "" && data2 != null && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);

                    }

                    SelectList s = new SelectList(qurey.Cities, "Id", "FullDetails", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (data2 != "")
                    {
                        int selected = Convert.ToInt32(data2);
                        var qurey = db.City.Where(e => e.Id == selected).ToList();

                        SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                        return Json(s, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var qurey = db.City.ToList();

                        SelectList s = new SelectList(qurey, "Id", "FullDetails", "");
                        return Json(s, JsonRequestBehavior.AllowGet);
                    }
                    return Json("", JsonRequestBehavior.AllowGet);
                }

            }
        }



        [HttpPost]
        public ActionResult Create(City C, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string CityData = form["Category_drop"] == "0" ? "" : form["Category_drop"];
                    if (CityData != null && CityData != "")
                    {
                        int ContId = Convert.ToInt32(CityData);
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "401").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(CityData)).FirstOrDefault(); // db.LookupValue.Find(ContId);
                        C.Category = val;
                    }
                    if (ModelState.IsValid)
                    {
                        C.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        Taluka tal = db.Taluka.Find(data);
                        City city = new City()
                        {
                            Code = C.Code,
                            Name = C.Name,
                            Category = C.Category,
                            DBTrack = C.DBTrack
                        };
                        tal.Cities = new List<City>();
                        try
                        {


                            if (db.City.Any(o => o.Code.ToLower() == C.Code.ToLower()))
                            {
                                var code = db.City.Where(o => o.Code.ToLower() == C.Code.ToLower()).SingleOrDefault();
                                Msg.Add("Code already exists for City - " + code.Name + ".");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //	return this.Json(new Object[] { "", "", "Code already exists for City - " + code.Name + ".", JsonRequestBehavior.AllowGet });
                            }
                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.City.Add(city);
                                db.SaveChanges();
                                // DBTrackFile.DBTrackSave("Core/P2b.Global", "C", city, null, "City", null);
                                tal.Cities.Add(city);
                                db.Taluka.Attach(tal);
                                db.Entry(tal).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                ts.Complete();
                            }
                            //return Json(new Object[] { city.Id,city.Name, "Data Saved Successfully." });
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = city.Id, Val = city.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = C.Id });
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


        [HttpPost]
        public ActionResult Createfortrip(City C, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var id = form["Category_drop"] != "" ? 0 : Convert.ToInt32(form["Category_drop"]);
                    var qurey = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "401").FirstOrDefault().LookupValues.Where(e => e.Id == id).FirstOrDefault();
                    if (ModelState.IsValid)
                    {
                        C.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        //      Taluka tal = db.Taluka.Find(data);
                        City city = new City()
                        {
                            Code = C.Code,
                            Name = C.Name,
                            Category = C.Category,
                            DBTrack = C.DBTrack
                        };
                        //tal.Cities = new List<City>();
                        try
                        {

                            if (db.City.Any(o => o.Code.ToLower() == C.Code.ToLower()))
                            {
                                var code = db.City.Where(o => o.Code.ToLower() == C.Code.ToLower()).SingleOrDefault();
                                Msg.Add("  Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { "", "", "Code already exists for City - " + code.Name + ".", JsonRequestBehavior.AllowGet });
                            }
                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.City.Add(city);
                                db.SaveChanges();
                                // DBTrackFile.DBTrackSave("Core/P2b.Global", "C", city, null, "City", null);
                                //tal.Cities.Add(city);
                                //db.Taluka.Attach(tal);
                                //db.Entry(tal).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();
                                ts.Complete();
                            }
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { city.Id, city.Name, "Data Saved Successfully." });
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = C.Id });
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

        [HttpPost]
        public ActionResult Createfortrainng(City count, int data, FormCollection form)
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
                            City country = new City()
                            {
                                Code = count.Code,
                                Name = count.Name,
                                Category = count.Category,
                                DBTrack = count.DBTrack
                            };
                            try
                            {
                                if (db.City.Any(o => o.Code.ToLower() == count.Code.ToLower()))
                                {
                                    var code = db.City.Where(o => o.Code.ToLower() == count.Code.ToLower()).SingleOrDefault();
                                    Msg.Add("  Code Already Exists.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return this.Json(new Object[] { "", "", "Code already exists for Country - " + code.Name + ".", JsonRequestBehavior.AllowGet });
                                }

                                if (db.City.Any(o => o.Name.ToLower() == count.Name.ToLower()))
                                {
                                    Msg.Add("  Name Already Exists.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return this.Json(new Object[] { "", "", "Name already exists.", JsonRequestBehavior.AllowGet });
                                }
                                db.City.Add(country);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, count.DBTrack);
                                DT_City DT_Count = (DT_City)rtn_Obj;
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