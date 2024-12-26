using P2b.Global;
using P2BUltimate.App_Start;
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
using System.Net;
using P2BUltimate.Models;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Core.MainController
{
     [AuthoriseManger]
    public class ContactDetailsController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /ContactDetails/

        private MultiSelectList GetContactNos(List<int> selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<ContactNumbers> Nos = new List<ContactNumbers>();
                Nos = db.ContactNumbers.ToList();
                return new MultiSelectList(Nos, "Id", "FullContactNumbers", selectedValues);
            }
        }

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/ContactDetails/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Core/_ContactDetails.cshtml");
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var r = (from ca in db.ContactDetails
                         .Where(e => e.Id == data)
                         select new
                         {
                             Id = ca.Id,
                             EmailId = ca.EmailId,
                             FaxNo = ca.FaxNo,
                             Website = ca.Website
                         }).ToList();

                var a = db.ContactDetails.Include(e => e.ContactNumbers).Where(e => e.Id == data).SingleOrDefault();
                var b = a.ContactNumbers;

                var r1 = (from s in b
                          select new
                          {
                              Id = s.Id,
                              FullContactNumbers = s.FullContactNumbers
                          }).ToList();
                TempData["RowVersion"] = db.ContactDetails.Find(data).RowVersion;
                return Json(new object[] { r, r1 }, JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult CreateSave(ContactDetails ContDetails, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string Values = form["ContactNos_List"];


                    if (Values != null)
                    {
                        List<int> IDs = Values.Split(',').Select(s => int.Parse(s)).ToList();
                        ViewBag.Numbers = GetContactNos(IDs);
                    }
                    else
                    {
                        ViewBag.Numbers = GetContactNos(null);
                    }

                    if (ModelState.IsValid)
                    {
                        ContDetails.ContactNumbers = new List<ContactNumbers>();
                        if (ViewBag.Numbers != null)
                        {
                            foreach (var val in ViewBag.Numbers)
                            {
                                if (val.Selected == true)
                                {
                                    var valToAdd = db.ContactNumbers.Find(int.Parse(val.Value));
                                    ContDetails.ContactNumbers.Add(valToAdd);
                                }
                            }
                        }
                        using (TransactionScope ts = new TransactionScope())
                        {
                            ContDetails.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            ContactDetails ContactDet = new ContactDetails()
                            {
                                EmailId = ContDetails.EmailId,
                                FaxNo = ContDetails.FaxNo,
                                Website = ContDetails.Website,
                                ContactNumbers = ContDetails.ContactNumbers,
                                DBTrack = ContDetails.DBTrack
                            };
                            try
                            {
                                if ((ContactDet.EmailId == "" || ContactDet.EmailId == null))
                                {
                                    if ((ContactDet.FaxNo == "" || ContactDet.FaxNo == null))
                                    {
                                        if ((ContactDet.Website == "" || ContactDet.Website == null))
                                        {
                                            if ((ContactDet.ContactNumbers.Count == 0))
                                            {
                                                Msg.Add("  Data should not be blank.  ");
                                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                                //return this.Json(new Object[] { "", "", "Data should not be blank." }, JsonRequestBehavior.AllowGet);
                                            }
                                        }
                                    }
                                }
                                db.ContactDetails.Add(ContactDet);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, ContDetails.DBTrack);
                                DT_ContactDetails DT_Corp = (DT_ContactDetails)rtn_Obj;
                                db.Create(DT_Corp);
                                db.SaveChanges();
                                ts.Complete();
                                //return this.Json(new { msg = "Data saved successfully." });

                                List<string> Msgs = new List<string>();
                                Msgs.Add("Data Saved successfully");
                                return Json(new Utility.JsonReturnClass { Id = ContactDet.Id, Val = ContactDet.FullContactDetails, success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] {, , "Data Saved SucessFully" }, JsonRequestBehavior.AllowGet);


                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = ContactDet.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                                //ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                                //return View(count);
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
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

                        // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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
                return View();
            }
        }


        public ActionResult EditSave(ContactDetails ContDetails, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //string Values = form["ContactNosList"];


                    //if (Values != null)
                    //{
                    //    List<int> IDs = Values.Split(',').Select(s => int.Parse(s)).ToList();
                    //    ViewBag.Numbers = GetContactNos(IDs);
                    //}
                    //else
                    //{
                    //    ViewBag.Numbers = GetContactNos(null);
                    //}

                    var db_data = db.ContactDetails.Include(e => e.ContactNumbers).Where(e => e.Id == data).SingleOrDefault();
                    List<ContactNumbers> contactNos = new List<ContactNumbers>();
                    string Values = form["ContactNos_List"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var Contact_Nos = db.ContactNumbers.Find(ca);
                            contactNos.Add(Contact_Nos);
                            db_data.ContactNumbers = contactNos;
                        }
                    }
                    else
                    {
                        db_data.ContactNumbers = null;
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            db.ContactDetails.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = db_data.RowVersion;
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                            ContactDetails blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            using (var context = new DataBaseContext())
                            {
                                blog = context.ContactDetails.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }

                            ContDetails.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };



                            ContactDetails ContactDet = new ContactDetails()
                            {
                                EmailId = ContDetails.EmailId,
                                FaxNo = ContDetails.FaxNo,
                                Website = ContDetails.Website,
                                ContactNumbers = ContDetails.ContactNumbers,
                                Id = data,
                                DBTrack = ContDetails.DBTrack
                            };
                            try
                            {
                                db.Entry(ContactDet).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(ContactDet).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                using (var context = new DataBaseContext())
                                {
                                    var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ContDetails.DBTrack);
                                    DT_ContactDetails DT_Corp = (DT_ContactDetails)obj;
                                    db.Create(DT_Corp);
                                }
                                db.SaveChanges();
                                ts.Complete();
                                //return this.Json(new { msg = "Data saved successfully." });
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = ContactDet.Id, Val = ContactDet.FullContactDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new Object[] { ContactDet.Id, ContactDet.FullContactDetails, "Data Updated SucessFully" }, JsonRequestBehavior.AllowGet);


                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = ContactDet.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                                //ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                                //return View(count);
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
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
                        return this.Json(new { msg = errorMsg, JsonRequestBehavior.AllowGet });
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
                return View();
            }
        }

        public ActionResult Get_ContactDetailsLookupvalue(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ContactDetails.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ContactDetails.Include(e => e.ContactNumbers).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
