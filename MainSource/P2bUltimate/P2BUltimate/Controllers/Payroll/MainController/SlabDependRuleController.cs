
using P2BUltimate.App_Start;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace P2BUltimate.Controllers
{
    public class SlabDependRuleController : Controller
    {
       // private DataBaseContext db = new DataBaseContext();
        //
        // GET: /SlabDependRule/

        public ActionResult Index()
        {
            return View();
        }

        


        //public ActionResult CreateSave(SlabDependRule slab,FormCollection form)
        //{
        //    string Category = form["Category"] == "0" ? "" : form["Category"];
        //    string Addrs = form["AddList"];
        //    string radio = form["radio"];

        //    if (Category != null)
        //    {
        //        if (Category != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(Category));
        //            c.BusinessType = val;
        //        }
        //    }

        //    if (Addrs != null)
        //    {
        //        if (Addrs != "")
        //        {
        //            int AddId = Convert.ToInt32(Addrs);
        //            var val = db.Address.Include(e => e.Area)
        //                                .Include(e => e.City)
        //                                .Include(e => e.Country)
        //                                .Include(e => e.District)
        //                                .Include(e => e.State)
        //                                .Include(e => e.StateRegion)
        //                                .Include(e => e.Taluka)
        //                                .Where(e => e.Id == AddId).SingleOrDefault();
        //            c.Address = val;
        //        }
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            if (db.Corporate.Any(o => o.Code == c.Code))
        //            {
        //                //ModelState.AddModelError(string.Empty, "Code already exists.");
        //                return this.Json(new { msg = "Code already exists." });
        //                //return View(c);
        //            }


        //            Corporate corporate = new Corporate()
        //            {
        //                Code = c.Code == null ? "" : c.Code.Trim(),
        //                Name = c.Name == null ? "" : c.Name.Trim(),
        //                BusinessType = c.BusinessType,
        //                Address = c.Address,
        //                ContactDetails = c.ContactDetails
        //            };
        //            try
        //            {
        //                db.Corporate.Add(corporate);
        //                db.SaveChanges();
        //                ts.Complete();
        //                return this.Json(new { msg = "Data saved successfully." });
        //            }

        //            catch (DbUpdateConcurrencyException)
        //            {
        //                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
        //            }
        //            catch (DataException /* dex */)
        //            {
        //                //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
        //                //ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
        //                //return View(level);
        //                return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
        //            }
        //        }
        //    }
        //    else
        //    {
        //        StringBuilder sb = new StringBuilder("");
        //        foreach (ModelState modelState in ModelState.Values)
        //        {
        //            foreach (ModelError error in modelState.Errors)
        //            {
        //                sb.Append(error.ErrorMessage);
        //                sb.Append("." + "\n");
        //            }
        //        }
        //        var errorMsg = sb.ToString();
        //        return this.Json(new { msg = errorMsg });
        //    }

        //}

    }
}
