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
    public class ContactNumbersController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /ContactNumbers/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Core/_contactno.cshtml");
        }


        public ActionResult CreateSave(ContactNumbers ContNos)
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
                            var check = db.ContactNumbers.Where(e => e.MobileNo == ContNos.MobileNo).FirstOrDefault();
                            if (check != null)
                            {
                                Msg.Add(" MobileNo already exists.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            ContNos.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            ContactNumbers ConNumbers = new ContactNumbers()
                            {
                                LandlineNo = ContNos.LandlineNo,
                                MobileNo = ContNos.MobileNo,
                                DBTrack = ContNos.DBTrack

                            };
                            try
                            {
                                if ((ConNumbers.LandlineNo == "" || ConNumbers.LandlineNo == null))
                                {
                                    if ((ConNumbers.MobileNo == "" || ConNumbers.MobileNo == null))
                                    {
                                        return this.Json(new Object[] { "", "", "Data should not be blank." }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                db.ContactNumbers.Add(ConNumbers);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, ContNos.DBTrack);
                                DT_ContactNumbers DT_ContNos = (DT_ContactNumbers)rtn_Obj;
                                db.Create(DT_ContNos);
                                db.SaveChanges();
                                ts.Complete();
                                //return this.Json(new { msg = "Data saved successfully." });
                                //  return this.Json(new Object[] { ConNumbers.Id, ConNumbers.FullContactNumbers, "Data Saved SucessFully" }, JsonRequestBehavior.AllowGet);
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = ConNumbers.Id, Val = ConNumbers.FullContactNumbers, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = ConNumbers.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                                //ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                                //return View(count);

                                //  return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
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
                if (data == null)
                {
                    return this.Json(new { msg = HttpStatusCode.BadRequest, JsonRequestBehavior.AllowGet });
                    //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var Category = db.ContactNumbers.Where(e => e.Id == data).ToList();
                var r = (from ca in Category
                         select new
                         {
                             Id = ca.Id,
                             MobileNo = ca.MobileNo,
                             LandlineNo = ca.LandlineNo
                         }).Distinct();

                //    TempData["RowVersion"] = db.ContactNumbers.Find(data).RowVersion;

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult EditSave(ContactNumbers ContNos,int data)
        //{
        //      List<string> Msg = new List<string>();
        //      using (DataBaseContext db = new DataBaseContext())
        //      {
        //          try
        //          {

        //              if (ModelState.IsValid)
        //              {
        //                  using (TransactionScope ts = new TransactionScope())
        //                  {
        //                      ContactNumbers blog = null; // to retrieve old data
        //                      DbPropertyValues originalBlogValues = null;

        //                      using (var context = new DataBaseContext())
        //                      {
        //                          blog = context.ContactNumbers.Where(e => e.Id == data).SingleOrDefault();
        //                          originalBlogValues = context.Entry(blog).OriginalValues;
        //                      }

        //                      if ((ContNos.LandlineNo == "" || ContNos.LandlineNo == null))
        //                      {
        //                          if ((ContNos.MobileNo == "" || ContNos.MobileNo == null))
        //                          {
        //                              //  return this.Json(new Object[] { data, blog.FullContactNumbers, "Data should not be blank." }, JsonRequestBehavior.AllowGet);
        //                              Msg.Add("  Data Created successfully  ");
        //                              return Json(new Utility.JsonReturnClass { Id = data, Val = blog.FullContactNumbers, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


        //                          }
        //                      }

        //                      ContNos.DBTrack = new DBTrack
        //                      {
        //                          CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                          CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                          Action = "M",
        //                          ModifiedBy = SessionManager.UserName,
        //                          ModifiedOn = DateTime.Now
        //                      };



        //                      ContactNumbers ConNumbers = new ContactNumbers()
        //                      {
        //                          LandlineNo = ContNos.LandlineNo,
        //                          MobileNo = ContNos.MobileNo,
        //                          Id = data,
        //                          DBTrack = ContNos.DBTrack
        //                      };
        //                      try
        //                      {
        //                          db.Entry(ConNumbers).State = System.Data.Entity.EntityState.Modified;
        //                          db.Entry(ConNumbers).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                          using (var context = new DataBaseContext())
        //                          {
        //                              var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ContNos.DBTrack);
        //                              DT_ContactNumbers DT_ContNos = (DT_ContactNumbers)obj;
        //                              db.Create(DT_ContNos);
        //                          }
        //                          db.SaveChanges();
        //                          ts.Complete();
        //                          //return this.Json(new { msg = "Data saved successfully." });
        //                          //return this.Json(new Object[] { ConNumbers.Id, ConNumbers.FullContactNumbers, "Data updated SucessFully" }, JsonRequestBehavior.AllowGet);
        //                          Msg.Add("  Data Saved successfully  ");
        //                          return Json(new Utility.JsonReturnClass { Id = ConNumbers.Id, Val = ConNumbers.FullContactNumbers, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                      }
        //                      catch (DbUpdateConcurrencyException)
        //                      {
        //                          return RedirectToAction("Create", new { concurrencyError = true, id = ConNumbers.Id });
        //                      }
        //                      catch (DataException /* dex */)
        //                      {
        //                          //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
        //                          //ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
        //                          //return View(count);

        //                          //  return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
        //                          Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
        //                          return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


        //                      }
        //                  }
        //              }
        //              else
        //              {
        //                  StringBuilder sb = new StringBuilder("");
        //                  foreach (ModelState modelState in ModelState.Values)
        //                  {
        //                      foreach (ModelError error in modelState.Errors)
        //                      {
        //                          sb.Append(error.ErrorMessage);
        //                          sb.Append("." + "\n");
        //                      }
        //                  }
        //                  var errorMsg = sb.ToString();
        //                  Msg.Add(errorMsg);
        //                  return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //              }
        //          }
        //          catch (Exception ex)
        //          {
        //              LogFile Logfile = new LogFile();
        //              ErrorLog Err = new ErrorLog()
        //              {
        //                  ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                  ExceptionMessage = ex.Message,
        //                  ExceptionStackTrace = ex.StackTrace,
        //                  LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                  LogTime = DateTime.Now
        //              };
        //              Logfile.CreateLogFile(Err);
        //              Msg.Add(ex.Message);
        //              return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //          }
        //      }
        //}

        public ActionResult EditSave(ContactNumbers ContDetails, int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.ContactNumbers.Where(e => e.Id == data).SingleOrDefault();

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            db.ContactNumbers.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = db_data.RowVersion;
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                            ContactNumbers blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            using (var context = new DataBaseContext())
                            {
                                blog = context.ContactNumbers.Where(e => e.Id == data).SingleOrDefault();
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

                            ContactNumbers ContactDet = new ContactNumbers()
                            {
                                MobileNo = ContDetails.MobileNo,
                                LandlineNo = ContDetails.LandlineNo,
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
                                    DT_ContactNumbers DT_Corp = (DT_ContactNumbers)obj;
                                    db.Create(DT_Corp);
                                }
                                db.SaveChanges();
                                ts.Complete();
                                //return this.Json(new { msg = "Data saved successfully." });
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = ContactDet.Id, Val = ContactDet.FullContactNumbers, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = ContactDet.Id });
                            }
                            catch (DataException /* dex */)
                            {
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


        public ActionResult Get_ContactNumbersLookupValue(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ContactNumbers.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ContactNumbers.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var list1 = db.ContactDetails.SelectMany(e => e.ContactNumbers).ToList();
                var list2 = fall.Except(list1);
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullContactNumbers }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        //[HttpPost]
        //public ActionResult Delete(int data, int forwarddata)
        //{
        //    ContactNumbers ConDet = db.ContactNumbers.Find(forwarddata);;
        //    var co = db.ContactNumbers.Include(e => e.ContactNumbers).Where(e => e.Id == forwarddata).SingleOrDefault();

        //    var selectedNos = co.ContactNumbers.Select(e => e.Id);
        //    ViewBag.Nos = GetContactNos(selectedNos.ToList());

        //    ConDet.ContactNumbers = co.ContactNumbers;
        //    try
        //    {
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            foreach (var no in ViewBag.Nos)
        //            {
        //                if (int.Parse(no.Value) == data)
        //                {
        //                    var noToRemove = db.ContactNumbers.Find(data);
        //                    ConDet.ContactNumbers.Remove(noToRemove);
        //                    break;
        //                }
        //            }

        //            db.Entry(co).State = System.Data.Entity.EntityState.Detached;

        //            ContactNumbers det = new ContactNumbers() 
        //            { 
        //                EmailId = co.EmailId,
        //                FaxNo = co.FaxNo,
        //                Website = co.Website,
        //                Id = forwarddata 
        //            };
        //            if (db.Entry(det).State == System.Data.Entity.EntityState.Detached)
        //                db.ContactNumbers.Attach(det);


        //            det.ContactNumbers = ConDet.ContactNumbers;
        //            db.SaveChanges();


        //            db.Entry(det).State = System.Data.Entity.EntityState.Modified;
        //            DBTrack dbT = new DBTrack
        //            {
        //                Action = "D",
        //                ModifiedBy = SessionManager.UserName,
        //                ModifiedOn = DateTime.Now,
        //                CreatedBy = co.DBTrack.CreatedBy != null ? co.DBTrack.CreatedBy : null,
        //                CreatedOn = co.DBTrack.CreatedOn != null ? co.DBTrack.CreatedOn : null,
        //                IsModified = co.DBTrack.IsModified == true ? false : false//,
        //                //AuthorizedBy = SessionManager.UserName,
        //                //AuthorizedOn = DateTime.Now
        //            };

        //            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

        //            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
        //            DT_ContactNumbers DT_ContNos = (DT_ContactNumbers)rtn_Obj;

        //            db.Create(DT_ContNos);

        //            db.SaveChanges();
        //            ts.Complete();
        //        }

        //        return this.Json(new { msg = "Data deleted." });
        //    }

        //    catch (DataException /* dex */)
        //    {
        //        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
        //    }
        //}
    }
}
