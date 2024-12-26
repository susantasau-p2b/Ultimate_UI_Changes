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
using System.Data.Entity.Core.Objects;
using System.Reflection;
using P2BUltimate.Security;
using Payroll;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class ITSection10SalHeadsController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /ITSection10SalHead/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PopulateSalHeadDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.SalaryHead.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            //string tableName = "Corporate";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ITSection10SalHeads
                    .Include(e => e.Frequency)
                    .Include(e => e.SalHead)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Amount = e.Amount,
                        AutoPick = e.AutoPick,
                        Frequency_Id = e.Frequency.Id == null ? 0 : e.Frequency.Id,
                        Months = e.Months,
                        Percent = e.Percent,
                        SalaryHead_Id = e.SalHead.Id == null ? 0 : e.SalHead.Id,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.ITSection10SalHeads
                  .Include(e => e.Frequency)
                    .Include(e => e.SalHead)
                    .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        //Address_FullAddress = e.Address.FullAddress == null ? "" : e.Address.FullAddress,
                        SalaryHead_Id = e.SalHead.Id == null ? 0 : e.SalHead.Id,
                        //Cont_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
                        //FullContactDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails
                    }).ToList();



                var Corp = db.ITSection10SalHeads.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(ITSection10SalHeads L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
                try
                {
                    var SalHeadlist = form["SalHeadlist"] == "0" ? "" : form["SalHeadlist"];
                    var Frequencylist = form["Frequencylist"] == "0" ? "" : form["Frequencylist"];
                    var Monthslist = form["StatutoryEffectiveMonthsList"] == "0" ? "" : form["StatutoryEffectiveMonthsList"];
                    var blog1 = db.ITSection10SalHeads.Include(e => e.Frequency).Include(e => e.SalHead).Where(e => e.Id == data).SingleOrDefault();
                    if (SalHeadlist != null && SalHeadlist != "")
                    {
                        var value = db.SalaryHead.Find(int.Parse(SalHeadlist));
                        L.SalHead = value;
                        blog1.SalHead = L.SalHead;
                    }

                    if (Frequencylist != null && Frequencylist != "")
                    {
                        var value = db.LookupValue.Find(int.Parse(Frequencylist));
                        L.Frequency = value;
                    }




                    if (L.Amount != null)
                    {
                        blog1.Amount = L.Amount;
                    }
                    if (L.Percent != null)
                    {
                        blog1.Percent = L.Percent;
                    }
                    blog1.AutoPick = L.AutoPick;
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

                                    blog1.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog1.DBTrack.CreatedBy == null ? null : blog1.DBTrack.CreatedBy,
                                        CreatedOn = blog1.DBTrack.CreatedOn == null ? null : blog1.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    if (Frequencylist != null)
                                    {
                                        if (Frequencylist != "")
                                        {
                                            var val = db.LookupValue.Find(int.Parse(Frequencylist));
                                            blog1.Frequency = val;

                                            var type = db.ITSection10SalHeads.Include(e => e.Frequency).Where(e => e.Id == data).SingleOrDefault();
                                            IList<ITSection10SalHeads> typedetails = null;
                                            if (type.Frequency != null)
                                            {
                                                typedetails = db.ITSection10SalHeads.Where(x => x.Frequency.Id == type.Frequency.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.ITSection10SalHeads.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.Frequency = blog1.Frequency;
                                                db.ITSection10SalHeads.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var WFTypeDetails = db.ITSection10SalHeads.Include(e => e.Frequency).Where(x => x.Id == data).ToList();
                                            foreach (var s in WFTypeDetails)
                                            {
                                                s.Frequency = null;
                                                db.ITSection10SalHeads.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var CreditdateypeDetails = db.ITSection10SalHeads.Include(e => e.Frequency).Where(x => x.Id == data).ToList();
                                        foreach (var s in CreditdateypeDetails)
                                        {
                                            s.Frequency = null;
                                            db.ITSection10SalHeads.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    var CurCorp = db.ITSection10SalHeads.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    //   db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        var post = db.ITSection10SalHeads.Include(e => e.Frequency).Include(e => e.SalHead).Where(e => e.Id == data).SingleOrDefault();
                                        //ITSection10SalHeads post = new ITSection10SalHeads()
                                        //{
                                        //    Amount = L.Amount,
                                        //    AutoPick = L.AutoPick,
                                        //    Frequency = L.Frequency,
                                        //    Months = L.Months,
                                        //    Percent = L.Percent,
                                        //    SalHead = L.SalHead,
                                        //    Id = data,
                                        //    DBTrack = L.DBTrack,
                                        //    RowVersion = (Byte[])TempData["RowVersion"]
                                        //};
                                        blog1.RowVersion = (Byte[])TempData["RowVersion"];
                                        db.ITSection10SalHeads.Attach(post);
                                        db.Entry(post).State = System.Data.Entity.EntityState.Modified;

                                        db.Entry(post).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        db.SaveChanges();

                                        await db.SaveChangesAsync();
                                        var as1 = db.ITSection10SalHeads.Include(e => e.Frequency).Include(e => e.SalHead).Where(e => e.Id == post.Id).SingleOrDefault();
                                        db.Entry(post).State = System.Data.Entity.EntityState.Detached;
                                        var asd1 = db.ITSection10SalHeads.Include(e => e.Frequency).Include(e => e.SalHead).Where(e => e.Id == data).SingleOrDefault();
                                        ts.Complete();
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = asd1.Id, Val = asd1.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                    }
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (ITSection10SalHeads)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (ITSection10SalHeads)databaseEntry.ToObject();
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
        //[ValidateAntiForgeryToken]
        public ActionResult Create(ITSection10SalHeads OBJ, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var SalHeadlist = form["SalHeadlist"] == "0" ? "" : form["SalHeadlist"];
                    var Frequencylist = form["Frequencylist"] == "0" ? "" : form["Frequencylist"];
                    var Monthslist = form["StatutoryEffectiveMonthsList"] == "0" ? "" : form["StatutoryEffectiveMonthsList"];

                    if (SalHeadlist != null && SalHeadlist != "")
                    {
                        var value = db.SalaryHead.Find(int.Parse(SalHeadlist));
                        OBJ.SalHead = value;
                    }

                    if (Frequencylist != null && Frequencylist != "")
                    {
                        var value = db.LookupValue.Find(int.Parse(Frequencylist));
                        OBJ.Frequency = value;
                    }

                    List<int> ids = null;
                    OBJ.Months = new List<StatutoryEffectiveMonths>();

                    if (Monthslist != null && Monthslist != "0" && Monthslist != "false")
                    {
                        ids = Utility.StringIdsToListIds(Monthslist);
                        foreach (var value in ids)
                        {
                            var itsub_val = db.StatutoryEffectiveMonths.Find(value);
                            OBJ.Months.Add(itsub_val);
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        OBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        ITSection10SalHeads ITSection10SalHeads = new ITSection10SalHeads()
                        {
                            Amount = OBJ.Amount,
                            AutoPick = OBJ.AutoPick,
                            Frequency = OBJ.Frequency,
                            Months = OBJ.Months,
                            Percent = OBJ.Percent,
                            SalHead = OBJ.SalHead,
                            DBTrack = OBJ.DBTrack
                        };
                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                //if (db.ITSubInvestmentPayment.Any(o => o.InvestmentDate == OBJ.InvestmentDate))
                                //{
                                //    return Json(new Object[] { null, null, "Investment already exists." });
                                //}
                                db.ITSection10SalHeads.Add(ITSection10SalHeads);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, OBJ.DBTrack);
                                //DT_ITSection10SalHeads DT_Corp = (DT_ITSection10SalHeads)rtn_Obj;
                                //DT_Corp.SalHead_Id = OBJ.SalHead == null ? 0 : OBJ.SalHead.Id;
                                //DT_Corp.Frequency_Id = OBJ.Frequency == null ? 0 : OBJ.Frequency.Id;
                                //db.Create(DT_Corp);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = ITSection10SalHeads.Id, Val = ITSection10SalHeads.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { ITSection10SalHeads.Id, ITSection10SalHeads.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });

                            }
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = OBJ.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            Msg.Add(" Unable to edit. Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return this.Json(new { msg = "Unable to edit. Try again, and if the problem persists contact your system administrator." });
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
    }
}