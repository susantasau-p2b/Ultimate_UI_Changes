using P2b.Global;
using EssPortal.App_Start;
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
using EssPortal.Models;
using EssPortal.Security;
namespace EssPortal.Controllers
{
    [AuthoriseManger]
    public class ContactNumbersController : Controller
    {
        //  private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/_ContactNumber.cshtml");
        }
        public ActionResult CreateSave(ContactNumbers ContNos)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
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

                            var id = Convert.ToInt32(SessionManager.EmpId);
                            var EmpData = db.Employee.Include(e => e.PerContact.ContactNumbers).Where(e => e.Id == id).SingleOrDefault();
                            ContactDetails oContactDetails = new ContactDetails();
                            oContactDetails.ContactNumbers = new List<ContactNumbers> { ConNumbers };
                            oContactDetails.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            if (EmpData.PerContact == null)
                            {
                                EmpData.PerContact = oContactDetails;
                            }
                            if (EmpData.PerContact.ContactNumbers != null)
                            {
                                EmpData.PerContact.ContactNumbers.Add(ConNumbers);
                            }
                            else
                            {
                                EmpData.PerContact.ContactNumbers = new List<ContactNumbers> { ConNumbers };
                            }

                            db.Entry(EmpData).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(EmpData).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            //return this.Json(new { msg = "Data saved successfully." });
                            return this.Json(new Object[] { ConNumbers.Id, ConNumbers.FullContactNumbers, "Data Saved SucessFully" }, JsonRequestBehavior.AllowGet);


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

                            return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
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
                    return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                }
            }
        }
        public ActionResult Edit(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data == null)
                {
                    return this.Json(new { msg = HttpStatusCode.BadRequest, JsonRequestBehavior.AllowGet });
                }
                var r = (from ca in db.ContactNumbers
                         select new
                         {
                             Id = ca.Id,
                             MobileNo = ca.MobileNo,
                             LandlineNo = ca.LandlineNo
                         }).Where(e => e.Id == data).Distinct();

                TempData["RowVersion"] = db.ContactNumbers.Find(data).RowVersion;
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult EditSave(ContactNumbers ContNos, int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        ContactNumbers blog = null;
                        DbPropertyValues originalBlogValues = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.ContactNumbers.Where(e => e.Id == data).SingleOrDefault();
                            originalBlogValues = context.Entry(blog).OriginalValues;
                        }

                        if ((ContNos.LandlineNo == "" || ContNos.LandlineNo == null))
                        {
                            if ((ContNos.MobileNo == "" || ContNos.MobileNo == null))
                            {
                                return this.Json(new Object[] { data, blog.FullContactNumbers, "Data should not be blank." }, JsonRequestBehavior.AllowGet);
                            }
                        }

                        ContNos.DBTrack = new DBTrack
                        {
                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                            Action = "M",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now
                        };
                        ContactNumbers ConNumbers = new ContactNumbers()
                        {
                            LandlineNo = ContNos.LandlineNo,
                            MobileNo = ContNos.MobileNo,
                            Id = data,
                            DBTrack = ContNos.DBTrack
                        };
                        try
                        {
                            db.Entry(ConNumbers).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(ConNumbers).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ContNos.DBTrack);
                                DT_ContactNumbers DT_ContNos = (DT_ContactNumbers)obj;
                                db.Create(DT_ContNos);
                            }
                            db.SaveChanges();
                            ts.Complete();
                            return this.Json(new Object[] { ConNumbers.Id, ConNumbers.FullContactNumbers, "Data updated SucessFully" }, JsonRequestBehavior.AllowGet);
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = ConNumbers.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
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
                    return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                }
            }
        }

        public ActionResult Get_ContactNumbersLookupValue(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var EmpId = Convert.ToInt32(SessionManager.EmpId);
                var fall = db.Employee
                    .Include(e => e.CorContact)
                    .Include(e => e.ResContact)
                    .Include(e => e.PerContact)
                    .Where(e => e.Id == EmpId)
                    .SelectMany(e => e.PerContact.ContactNumbers)
                    .ToList();
                // var fall = db.ContactNumbers.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Employee
                                .Include(e => e.CorContact.ContactNumbers)
                                .Include(e => e.ResContact.ContactNumbers)
                                .Include(e => e.PerContact.ContactNumbers)
                                .Where(e => e.Id == EmpId && !e.Id.ToString().Contains(a.ToString())).SelectMany(e => e.PerContact.ContactNumbers).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                //var list1 = db.Employee
                //                .Include(e => e.CorContact.ContactNumbers)
                //                .Include(e => e.ResContact.ContactNumbers)
                //                .Include(e => e.PerContact.ContactNumbers).SelectMany(e => e.PerContact.ContactNumbers).ToList();
                //var list2 = fall.Except(list1);
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullContactNumbers }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
