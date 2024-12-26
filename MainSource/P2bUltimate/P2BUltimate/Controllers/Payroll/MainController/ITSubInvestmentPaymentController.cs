using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Collections;
using System.Reflection;
using P2BUltimate.Security;
using Payroll;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class ITSubInvestmentPaymentController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /ITSubInvestmentPayment/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CreateITSubInvPay_partial()
        {
            return View("~/Views/Shared/Payroll/_ITSubInvestmentPayment.cshtml");
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create(ITSubInvestmentPayment OBJ, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var ITSubInvestmentlist = form["ITSubInvestmentlist"] == "0" ? "" : form["ITSubInvestmentlist"];

                    if (ITSubInvestmentlist != null && ITSubInvestmentlist != "")
                    {
                        var value = db.ITSubInvestment.Find(int.Parse(ITSubInvestmentlist));
                        OBJ.ITSubInvestment = value;
                    }

                    if (ModelState.IsValid)
                    {
                        OBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        ITSubInvestmentPayment ITSubInvestmentPay = new ITSubInvestmentPayment()
                        {
                            ActualInvestment = OBJ.ActualInvestment,
                            DeclaredInvestment = OBJ.DeclaredInvestment,
                            InvestmentDate = OBJ.InvestmentDate,
                            ITSubInvestment = OBJ.ITSubInvestment,
                            Narration = OBJ.Narration,
                            DBTrack = OBJ.DBTrack
                        };
                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                if (db.ITSubInvestmentPayment.Any(o => o.InvestmentDate == OBJ.InvestmentDate))
                                {
                                    Msg.Add("  Code Already Exists.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    // return Json(new Object[] { null, null, "Investment already exists." });
                                }
                                db.ITSubInvestmentPayment.Add(ITSubInvestmentPay);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, OBJ.DBTrack);
                                //DT_ITSubInvestmentPayment DT_Corp = (DT_ITSubInvestmentPayment)rtn_Obj;
                                //DT_Corp.ITSubInvestment_Id = OBJ.ITSubInvestment == null ? 0 : OBJ.ITSubInvestment.Id;
                                //db.Create(DT_Corp);
                                db.SaveChanges();
                                ts.Complete();
                            }
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = ITSubInvestmentPay.Id, Val = ITSubInvestmentPay.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { ITSubInvestmentPay.Id, ITSubInvestmentPay.Narration, "Data saved successfully." });
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = OBJ.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            Msg.Add(" Unable to edit. Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return this.Json(new { msg = "Unable to edit. Try again, and if the problem persists contact your system administrator." });
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

                        // return this.Json(new { msg = errorMsg });
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
        public async Task<ActionResult> EditSave(ITSubInvestmentPayment L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
                try
                {
                    var ITSubInvestmentlist = form["ITSubInvestmentlist"] == "0" ? "" : form["ITSubInvestmentlist"];
                    var blog1 = db.ITSubInvestmentPayment.Where(e => e.Id == data).Include(e => e.ITSubInvestment).SingleOrDefault();

                    //var sad = Utility.StringIdsToListIds(ITSubInvestmentlist);
                    //if (L.Narration != null)
                    //{
                    //    blog1.Narration = L.Narration.ToString();
                    //}
                    //if (L.InvestmentDate != null)
                    //{
                    //    blog1.InvestmentDate = L.InvestmentDate;
                    //}
                    //if (L.DeclaredInvestment != null)
                    //{
                    //    blog1.DeclaredInvestment = L.DeclaredInvestment;
                    //}
                    //if (L.ActualInvestment != null)
                    //{
                    //    blog1.ActualInvestment = L.ActualInvestment;
                    //}

                    if (ITSubInvestmentlist != null && ITSubInvestmentlist != "")
                    {
                        int ContId = Convert.ToInt32(ITSubInvestmentlist);
                        var val = db.ITSubInvestment.Where(e => e.Id == ContId).SingleOrDefault();
                        L.ITSubInvestment = val;
                    }

                    if (ITSubInvestmentlist != null)
                    {
                        if (ITSubInvestmentlist != "")
                        {
                            var val = db.ITSubInvestment.Find(int.Parse(ITSubInvestmentlist));
                            L.ITSubInvestment = val;

                            var add = db.ITSubInvestmentPayment.Include(e => e.ITSubInvestment).Where(e => e.Id == data).SingleOrDefault();
                            IList<ITSubInvestmentPayment> addressdetails = null;
                            if (add.ITSubInvestment != null)
                            {
                                addressdetails = db.ITSubInvestmentPayment.Where(x => x.ITSubInvestment.Id == add.ITSubInvestment.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                addressdetails = db.ITSubInvestmentPayment.Where(x => x.Id == data).ToList();
                            }
                            if (addressdetails != null)
                            {
                                foreach (var s in addressdetails)
                                {
                                    s.ITSubInvestment = L.ITSubInvestment;
                                    db.ITSubInvestmentPayment.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    // await db.SaveChangesAsync(false);
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                        }
                    }
                    else
                    {
                        var addressdetails = db.ITSubInvestmentPayment.Include(e => e.ITSubInvestment).Where(x => x.Id == data).ToList();
                        foreach (var s in addressdetails)
                        {
                            s.ITSubInvestment = null;
                            db.ITSubInvestmentPayment.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    using (var context = new DataBaseContext())
                                    {
                                        //blog = context.PostDetails.Where(e => e.Id == data).Include(e => e.FuncStruct)
                                        //              .Include(e => e.FuncStruct.JobPosition)                                    
                                        //              .Include(e=>e.FuncStruct.Job)
                                        //              .Include(e => e.ExpFilter)
                                        //              .Include(e => e.RangeFilter)
                                        //              .Include(e => e.Qualification)
                                        //              .Include(e => e.Skill)
                                        //              .Include(e => e.Gender)
                                        //              .Include(e => e.MaritalStatus)
                                        //              .Include(e => e.CategoryPost)
                                        //              .Include(e => e.CategoryPost.Select(q=>q.Category))
                                        //              .Include(e => e.CategorySplPost)
                                        //              .Include(e => e.CategorySplPost.Select(q => q.SpecialCategory))
                                        //                        .SingleOrDefault();
                                        //originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    L.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog1.DBTrack.CreatedBy == null ? null : blog1.DBTrack.CreatedBy,
                                        CreatedOn = blog1.DBTrack.CreatedOn == null ? null : blog1.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    var m1 = db.ITSubInvestmentPayment.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.ITSubInvestmentPayment.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }

                                    var CurCorp = db.ITSubInvestmentPayment.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                     db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        ITSubInvestmentPayment post = new ITSubInvestmentPayment()
                                        {
                                            ActualInvestment = L.ActualInvestment,
                                            DeclaredInvestment = L.DeclaredInvestment,
                                            InvestmentDate = L.InvestmentDate,
                                            //ITSubInvestment = L.ITSubInvestment,
                                            Narration = L.Narration, 
                                            Id = data,
                                            DBTrack = L.DBTrack
                                        };
                                        db.ITSubInvestmentPayment.Attach(post);
                                        db.Entry(post).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(post).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // return 1;
                                        db.SaveChanges();
                                        await db.SaveChangesAsync();
                                        ts.Complete();
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = post.Id, Val = post.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                    }
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (ITSubInvestmentPayment)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (ITSubInvestmentPayment)databaseEntry.ToObject();
                                blog1.RowVersion = databaseValues.RowVersion;

                            }
                        }
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });

                    }
                    return Json(new Object[] { "", "", "", JsonRequestBehavior.AllowGet });
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


        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ITSubInvestmentPayment.Include(e => e.ITSubInvestment).Where(e => e.Id == data).Select
                    (e => new
                    {
                        // JobPosition_Id = e.FuncStruct.JobPosition.Id==null? 0 : e.FuncStruct.JobPosition.Id ,
                        ActualInvestment = e.ActualInvestment,
                        DeclaredInvestment = e.DeclaredInvestment,
                        InvestmentDate = e.InvestmentDate,
                        Narration = e.Narration == null ? "" : e.Narration,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.ITSubInvestmentPayment.Include(e => e.ITSubInvestment).Where(e => e.Id == data)
                   .Select(e => new
                   {
                       ITSubInvestment_Val = e.ITSubInvestment.FullDetails == null ? "" : e.ITSubInvestment.FullDetails,
                       ITSubInvestment_Id = e.ITSubInvestment.Id == null ? "" : e.ITSubInvestment.Id.ToString(),
                   }).ToList();

                //var W = db.DT_Corporate
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,
                //         Code = e.Code == null ? "" : e.Code,
                //         Name = e.Name == null ? "" : e.Name,
                //         BusinessType_Val = e.BusinessType_Id == 0 ? "" : db.LookupValue
                //                    .Where(x => x.Id == e.BusinessType_Id)
                //                    .Select(x => x.LookupVal).FirstOrDefault(),

                //         Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
                //         Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.ITSubInvestmentPayment.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }
    }
}