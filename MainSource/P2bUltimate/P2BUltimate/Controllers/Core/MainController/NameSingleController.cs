using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System.Web.Helpers;
using System.Web.Script.Serialization;
using P2BUltimate.Models;
using System.Transactions;
using System.Data.Entity.Infrastructure;
using System.Text;
using System.Data;
using P2BUltimate.Security;


namespace P2BUltimate.Controllers.Core.MainController
{
    public class NameSingleController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/Core/_NameSingle.cshtml");
        }
        public ActionResult Create(NameSingle td, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Category = form["title"];
                    //   string Category = form["title"] == "0" ? "" : form["title"];
                    if (Category != null && Category != "-Select-")
                    {
                        if (Category != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "100").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Category)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(Category));
                            td.EmpTitle = val;
                        }
                    }
                    if (td.EmpTitle == null)
                    {
                        td.EmpTitle.LookupVal = "";
                    }
                    if (td.MName == null)
                    {
                        td.MName = "";
                    }
                    if (td.LName == null)
                    {
                        td.LName = "";
                    }
                    if (ModelState.IsValid)
                    {
                        //if (db.Employee.Include(e => e.EmpName).Any(o =>  o.EmpName.FName == td.FName && o.EmpName.MName == td.MName && o.EmpName.LName == td.LName))
                        //{
                        //    Msg.Add("  Person with this Name Already Exists.  ");
                        //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //}
                        var UsedEmpName = db.Employee.Select(e => e.EmpName).ToList();
                        var All = db.NameSingle.ToList();
                        var union = All.Except(UsedEmpName).ToList();
                        if (union.Any(o => o.EmpTitle != null && o.EmpTitle.LookupVal.ToString() == td.EmpTitle.LookupVal.ToString() &&
                                            (o.FName != null && o.FName == td.FName) &&
                                         (o.MName != null && o.MName == td.MName) &&
                                            (o.LName != null && o.LName == td.LName)))
                        {
                            Msg.Add("  Person with this Name Already Exists.  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        //else
                        //{
                        NameSingle nds = new NameSingle()
                        {

                            FName = td.FName == null ? "" : td.FName.Trim(),
                            LName = td.LName == null ? "" : td.LName.Trim(),
                            EmpTitle = td.EmpTitle,
                            MName = td.MName == null ? "" : td.MName.Trim(),

                        };
                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.NameSingle.Add(nds);
                                db.SaveChanges();
                                ts.Complete();

                            }
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = nds.Id, Val = nds.FullNameFML, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { nds.Id, nds.FullNameFML, "Data saved successfully." }, JsonRequestBehavior.AllowGet);

                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = td.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                            return View(td);
                        }

                        //}
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
        public ActionResult PopulateDropDownList(String data, String data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                String selected = "";
                if (data2 != null)
                {
                    selected = data2;
                }
                SelectList s = new SelectList(db.NameSingle, "Id", "FullNameFML", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult edit(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var id = Convert.ToInt32(data);
                var db_data = db.NameSingle
                    .Include(e => e.EmpTitle)
                    .Where(e => e.Id == id)
                    .Select(e => new
                    {
                        FName = e.FName,
                        MName = e.MName,
                        LName = e.LName,
                        EmpTitle = e.EmpTitle != null ? e.EmpTitle.Id.ToString() : null,
                    }).SingleOrDefault();
                return Json(db_data, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult EditSave(NameSingle ContDetails, int data, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var id = Convert.ToInt32(data);
                    var db_data = db.NameSingle.Include(e => e.EmpTitle).Where(e => e.Id == id).SingleOrDefault();
                    string Category = form["title"];
                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "100").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Category)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(Category));
                            ContDetails.EmpTitle = val;
                            db_data.EmpTitle = ContDetails.EmpTitle;
                        }
                    }
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            db.NameSingle.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = db_data.RowVersion;
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                            NameSingle blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            using (var context = new DataBaseContext())
                            {
                                blog = context.NameSingle.Where(e => e.Id == id).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }

                            //ContDetails.DBtrack= new DBTrack
                            //{
                            //    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                            //    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                            //    Action = "M",
                            //    ModifiedBy = SessionManager.UserName,
                            //    ModifiedOn = DateTime.Now
                            //};



                            NameSingle ContactDet = new NameSingle()
                            {
                                EmpTitle_Id = ContDetails.EmpTitle.Id,
                                FName = ContDetails.FName,
                                MName = ContDetails.MName,
                                LName = ContDetails.LName,
                                Id = id 
                                //DBTrack = ContDetails.DBTrack
                            };
                            try
                            {
                                db.Entry(ContactDet).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(ContactDet).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                //using (var context = new DataBaseContext())
                                //{
                                //    var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ContDetails.DBTrack);
                                //    DT_ContactDetails DT_Corp = (DT_ContactDetails)obj;
                                //    db.Create(DT_Corp);
                                //}
                                db.SaveChanges();
                                ts.Complete();
                                //return this.Json(new { msg = "Data saved successfully." });
                                // return this.Json(new Object[] { ContactDet.Id, ContactDet.FullDetails, "Data Updated SucessFully" }, JsonRequestBehavior.AllowGet);
                                Msg.Add("  Data Updated successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = ContactDet.Id, Val = ContactDet.FullNameFML, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                            }
                            catch (DbUpdateConcurrencyException ex)
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
                                // return RedirectToAction("Create", new { concurrencyError = true, id = ContactDet.Id });

                            }
                            catch (DataException /* dex */)
                            {
                                //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                                //ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                                //return View(count);
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