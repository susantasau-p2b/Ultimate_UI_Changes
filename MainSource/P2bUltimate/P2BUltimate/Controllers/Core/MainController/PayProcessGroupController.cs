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
using System.Threading.Tasks;

namespace P2BUltimate.Controllers
{
     [AuthoriseManger]
    public class PayProcessGroupController : Controller
    {
       // private DataBaseContext db = new DataBaseContext();
        //
        // GET: /PayProcessGroup/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult _PayProcessGroupPartial()
        {
            return View("~/Views/Shared/Core/_PayProcessGroup.cshtml");
        }


        [HttpPost]
        public ActionResult Create(PayProcessGroup PayProcGrp, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    List<PayrollPeriod> PayrollPeriod = new List<PayrollPeriod>();
                    //  PayProcGrp.PayDate = Convert.ToInt32(form["PayDate_drop"] == "-Select-" ? "0" : form["PayDate_drop"]);
                    string paymnthconc = form["PayDate_drop"] == "0" ? "" : form["PayDate_drop"];
                    string PayFreq = form["PayFrequency_drop"] == "0" ? "" : form["PayFrequency_drop"];
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    var Company = new Company();
                    if (company_Id != null)
                    {
                        Company = db.Company.Where(e => e.Id == company_Id).SingleOrDefault();

                    }

                    if (PayFreq != null && PayFreq != "")
                    {
                        PayProcGrp.PayFrequency = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "421").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(PayFreq)).FirstOrDefault();  //db.LookupValue.Find(Convert.ToInt32(PayFreq));
                    }

                    if (paymnthconc != null && paymnthconc != "")
                    {
                        PayProcGrp.PayMonthConcept = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "550").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(paymnthconc)).FirstOrDefault();  //db.LookupValue.Find(Convert.ToInt32(paymnthconc));
                    }
                    string Values = form["PayrollPeriodlist"] == "0" ? "" : form["PayrollPeriodlist"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var PayrollPeriod_val = db.PayrollPeriod.Find(ca);
                            PayrollPeriod.Add(PayrollPeriod_val);
                            PayProcGrp.PayrollPeriod = PayrollPeriod;
                        }
                    }
                    else
                    {
                        PayProcGrp.PayrollPeriod = null;
                    }

                    if (ModelState.IsValid)
                    {
                        PayProcGrp.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        PayProcessGroup PayProcessGrp = new PayProcessGroup()
                        {
                            Name = PayProcGrp.Name,
                            //  PayDate = PayProcGrp.PayDate,
                            PayFrequency = PayProcGrp.PayFrequency,
                            PayMonthConcept = PayProcGrp.PayMonthConcept,
                            PayrollPeriod = PayProcGrp.PayrollPeriod,
                            DBTrack = PayProcGrp.DBTrack
                        };

                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.PayProcessGroup.Add(PayProcessGrp);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, PayProcessGrp.DBTrack);
                                DT_PayProcessGroup DT_PayProcessGrp = (DT_PayProcessGroup)rtn_Obj;
                                DT_PayProcessGrp.PayFrequency_Id = PayProcessGrp.PayFrequency == null ? 0 : PayProcessGrp.PayFrequency.Id;
                                DT_PayProcessGrp.PayMonthConcept_Id = PayProcessGrp.PayMonthConcept == null ? 0 : PayProcessGrp.PayMonthConcept.Id;
                                db.Create(DT_PayProcessGrp);
                                db.SaveChanges();

                                if (Company != null)
                                {
                                    var Payprocessgroup_list = new List<PayProcessGroup>();
                                    Payprocessgroup_list.Add(PayProcessGrp);
                                    Company.PayProcessGroup = Payprocessgroup_list;
                                    db.Company.Attach(Company);
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Detached;

                                }

                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = PayProcessGrp.Id, Val = PayProcessGrp.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { PayProcessGrp.Id, PayProcessGrp.FullDetails, "Data Saved successfully" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = PayProcessGrp.Id });
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

        public class PayrollPeriod1
        {
            public Array PayrollPeriod_Id { get; set; }
            public Array FullPayrollPeriodDetails { get; set; }



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
                var Q = db.PayProcessGroup
                    .Include(e => e.PayMonthConcept)
                   .Include(e => e.PayFrequency)
                    .Include(e => e.PayrollPeriod)

                    .Where(e => e.Id == data).Select
                    (e => new
                    {

                        Name = e.Name,
                        PayMonthConcept_Id = e.PayMonthConcept.Id == null ? 0 : e.PayMonthConcept.Id,
                        PayFrequency_Id = e.PayFrequency.Id == null ? 0 : e.PayFrequency.Id,
                        Action = e.DBTrack.Action
                    }).ToList();

                List<PayrollPeriod1> ContactDetails = new List<PayrollPeriod1>();

                var k = db.PayProcessGroup.Include(e => e.PayrollPeriod).Where(e => e.Id == data).ToList();
                foreach (var e in k)
                {
                    ContactDetails.Add(new PayrollPeriod1
                    {

                        PayrollPeriod_Id = e.PayrollPeriod.Select(a => a.Id.ToString()).ToArray(),
                        FullPayrollPeriodDetails = e.PayrollPeriod.Select(a => a.FullDetails).ToArray(),
                    });
                }

                var W = db.DT_PayProcessGroup
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,

                         Name = e.Name == null ? "" : e.Name,
                         PayFrequency_Val = e.PayFrequency_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.PayFrequency_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),
                         PayMonthConcept_Val = e.PayMonthConcept_Id == 0 ? "" : db.LookupValue
                       .Where(x => x.Id == e.PayMonthConcept_Id)
                       .Select(x => x.LookupVal).FirstOrDefault(),

                         PayrollPeriod_Val = e.PayrollPeriod == 0 ? "" : db.PayrollPeriod.Where(x => x.Id == e.PayrollPeriod).Select(x => x.FullDetails).FirstOrDefault(),

                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.PayProcessGroup.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, ContactDetails, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        //public async Task<ActionResult> EditSave(PayProcessGroup c, int data, FormCollection form) // Edit submit
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<string> Msg = new List<string>();
        //        try
        //        {
        //            //  List<PayrollPeriod> PayrollPeriod = new List<PayrollPeriod>();
        //            string paymnthconc = form["PayDate_drop"] == "0" ? "" : form["PayDate_drop"];
        //            string PayFreq = form["PayFrequency_drop"] == "0" ? "" : form["PayFrequency_drop"];

        //            //  bool Auth = form["Autho_Action"] == "" ? false : true;
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;

        //            if (PayFreq != null && PayFreq != "")
        //            {
        //                c.PayFrequency = db.LookupValue.Find(Convert.ToInt32(PayFreq));
        //            }

        //            if (paymnthconc != null && paymnthconc != "")
        //            {
        //                c.PayMonthConcept = db.LookupValue.Find(Convert.ToInt32(paymnthconc));
        //            }

        //            if (paymnthconc != null)
        //            {
        //                if (paymnthconc != "")
        //                {
        //                    var val = db.LookupValue.Find(int.Parse(paymnthconc));
        //                    c.PayMonthConcept = val;

        //                    var type = db.PayProcessGroup.Include(e => e.PayMonthConcept).Where(e => e.Id == data).SingleOrDefault();
        //                    IList<PayProcessGroup> typedetails = null;
        //                    if (type.PayMonthConcept != null)
        //                    {
        //                        typedetails = db.PayProcessGroup.Where(x => x.PayMonthConcept.Id == type.PayMonthConcept.Id && x.Id == data).ToList();
        //                    }
        //                    else
        //                    {
        //                        typedetails = db.PayProcessGroup.Where(x => x.Id == data).ToList();
        //                    }
        //                    //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //                    foreach (var s in typedetails)
        //                    {
        //                        s.PayMonthConcept = c.PayMonthConcept;
        //                        db.PayProcessGroup.Attach(s);
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                        //await db.SaveChangesAsync();
        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = s.RowVersion;
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                    }
        //                }
        //                else
        //                {
        //                    var BusiTypeDetails = db.PayProcessGroup.Include(e => e.PayMonthConcept).Where(x => x.Id == data).ToList();
        //                    foreach (var s in BusiTypeDetails)
        //                    {
        //                        s.PayMonthConcept = null;
        //                        db.PayProcessGroup.Attach(s);
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                        //await db.SaveChangesAsync();
        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = s.RowVersion;
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                    }
        //                }
        //            }
        //            if (PayFreq != null)
        //            {
        //                if (PayFreq != "")
        //                {
        //                    var val = db.LookupValue.Find(int.Parse(PayFreq));
        //                    c.PayFrequency = val;

        //                    var type = db.PayProcessGroup.Include(e => e.PayFrequency).Where(e => e.Id == data).SingleOrDefault();
        //                    IList<PayProcessGroup> typedetails = null;
        //                    if (type.PayFrequency != null)
        //                    {
        //                        typedetails = db.PayProcessGroup.Where(x => x.PayFrequency.Id == type.PayFrequency.Id && x.Id == data).ToList();
        //                    }
        //                    else
        //                    {
        //                        typedetails = db.PayProcessGroup.Where(x => x.Id == data).ToList();
        //                    }
        //                    //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //                    foreach (var s in typedetails)
        //                    {
        //                        s.PayFrequency = c.PayFrequency;
        //                        db.PayProcessGroup.Attach(s);
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                        //await db.SaveChangesAsync();
        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = s.RowVersion;
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                    }
        //                }
        //                else
        //                {
        //                    var BusiTypeDetails = db.PayProcessGroup.Include(e => e.PayFrequency).Where(x => x.Id == data).ToList();
        //                    foreach (var s in BusiTypeDetails)
        //                    {
        //                        s.PayFrequency = null;
        //                        db.PayProcessGroup.Attach(s);
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                        //await db.SaveChangesAsync();
        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = s.RowVersion;
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                    }
        //                }
        //            }
        //            c.PayrollPeriod = null;
        //            List<PayrollPeriod> OBJ = new List<PayrollPeriod>();
        //            string Values = form["PayrollPeriodlist"];

        //            if (Values != null)
        //            {
        //                var ids = Utility.StringIdsToListIds(Values);
        //                foreach (var ca in ids)
        //                {
        //                    var OBJ_val = db.PayrollPeriod.Find(ca);
        //                    OBJ.Add(OBJ_val);
        //                    c.PayrollPeriod = OBJ;
        //                }
        //            }

        //            if (Values != null)
        //            {
        //                if (Values != "")
        //                {
        //                    var val = db.PayrollPeriod.Find(int.Parse(Values));
        //                    //var val = db.Hobby.Where(e => e.Id == data).SingleOrDefault();

        //                    var r = (from ca in db.PayrollPeriod
        //                             select new
        //                             {
        //                                 Id = ca.Id,
        //                                 LookupVal = ca.FullDetails
        //                             }).Where(e => e.Id == data).Distinct();

        //                    var add = db.PayProcessGroup.Include(e => e.PayrollPeriod).Where(e => e.Id == data).SingleOrDefault();
        //                    IList<PayProcessGroup> contactdetails = null;
        //                    if (add.PayrollPeriod != null)
        //                    //{
        //                    //    contactdetails = db.FamilyDetails.Where(x => x.ContactDetails == add.ContactDetails && x.Id == data).ToList();
        //                    //}
        //                    //else
        //                    {
        //                        contactdetails = db.PayProcessGroup.Where(x => x.Id == data).ToList();
        //                    }
        //                    if (contactdetails != null)
        //                    {
        //                        foreach (var s in contactdetails)
        //                        {
        //                            s.PayrollPeriod = c.PayrollPeriod;
        //                            db.PayProcessGroup.Attach(s);
        //                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                            // await db.SaveChangesAsync(false);
        //                            db.SaveChanges();
        //                            TempData["RowVersion"] = s.RowVersion;
        //                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                var contactdetails = db.PayProcessGroup.Include(e => e.PayrollPeriod).Where(x => x.Id == data).ToList();
        //                foreach (var s in contactdetails)
        //                {
        //                    s.PayrollPeriod = null;
        //                    db.PayProcessGroup.Attach(s);
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                    //await db.SaveChangesAsync();
        //                    db.SaveChanges();
        //                    TempData["RowVersion"] = s.RowVersion;
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                }
        //            }
        //            //string Values = form["PayrollPeriodlist"];

        //            //if (Values != null)
        //            //{
        //            //    var ids = Utility.StringIdsToListIds(Values);
        //            //    foreach (var ca in ids)
        //            //    {
        //            //        var PayrollPeriod_val = db.PayrollPeriod.Find(ca);

        //            //        c.PayrollPeriod = PayrollPeriod;
        //            //    }
        //            //}
        //            //else
        //            //{
        //            //    c.PayrollPeriod = null;
        //            //}



                    
        //            if (Auth == false)
        //            {


        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {

        //                        //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            PayProcessGroup blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.PayProcessGroup.Where(e => e.Id == data).Include(e => e.PayFrequency)
        //                                                        .Include(e => e.PayMonthConcept)
        //                                                        .Include(e => e.PayrollPeriod).SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            c.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };

        //                           // int a = EditS(paymnthconc, PayFreq, Values, data, c, c.DBTrack);
        //                            var m1 = db.PayProcessGroup.Where(e => e.Id == data).ToList();
        //                            foreach (var s in m1)
        //                            {
        //                                // s.AppraisalPeriodCalendar = null;
        //                                db.PayProcessGroup.Attach(s);
        //                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                //await db.SaveChangesAsync();
        //                                db.SaveChanges();
        //                                TempData["RowVersion"] = s.RowVersion;
        //                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                            }
        //                            var CurCorp = db.PayProcessGroup.Find(data);
        //                            TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            {
        //                               // c.DBTrack = dbT;
        //                                PayProcessGroup corp = new PayProcessGroup()
        //                                {
        //                                    Name = c.Name,
        //                                    Id = data,
        //                                    PayFrequency = c.PayFrequency,
        //                                    PayMonthConcept = c.PayMonthConcept,
        //                                    PayrollPeriod = c.PayrollPeriod,
        //                                    DBTrack = c.DBTrack
        //                                };

        //                                db.PayProcessGroup.Attach(corp);
        //                                db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //                                db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                                //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                              //  return 1;

        //                            using (var context = new DataBaseContext())
        //                            {


        //                                var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                                DT_PayProcessGroup DT_Corp = (DT_PayProcessGroup)obj;
        //                                DT_Corp.PayMonthConcept_Id = blog.PayMonthConcept == null ? 0 : blog.PayMonthConcept.Id;
        //                                DT_Corp.PayFrequency_Id = blog.PayFrequency == null ? 0 : blog.PayFrequency.Id;

        //                                db.Create(DT_Corp);
        //                                db.SaveChanges();
        //                            }
        //                            await db.SaveChangesAsync();
        //                          //  var val = db.PayProcessGroup.Where(e => e.Id == corp.Id).SingleOrDefault();
        //                            ts.Complete();
        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            //return Json(new Object[] { c.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

        //                            }
        //                        }
        //                    }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (PayProcessGroup)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {
        //                            Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (PayProcessGroup)databaseEntry.ToObject();
        //                            c.RowVersion = databaseValues.RowVersion;

        //                        }
        //                    }
        //                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    PayProcessGroup blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    PayProcessGroup Old_Corp = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.PayProcessGroup.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }
        //                    c.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };

        //                    if (TempData["RowVersion"] == null)
        //                    {
        //                        TempData["RowVersion"] = blog.RowVersion;
        //                    }

        //                    PayProcessGroup corp = new PayProcessGroup()
        //                    {

        //                        Name = c.Name,
        //                        Id = data,
        //                        DBTrack = c.DBTrack,
        //                        RowVersion = (Byte[])TempData["RowVersion"]
        //                    };


        //                    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "PayProcessGroup", c.DBTrack);
        //                        // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                        Old_Corp = context.PayProcessGroup.Where(e => e.Id == data).Include(e => e.PayFrequency)
        //                            .Include(e => e.PayMonthConcept).Include(e => e.PayrollPeriod).SingleOrDefault();
        //                        DT_PayProcessGroup DT_Corp = (DT_PayProcessGroup)obj;
        //                        DT_Corp.PayrollPeriod = DBTrackFile.ValCompare(Old_Corp.PayrollPeriod, c.PayrollPeriod);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                        DT_Corp.PayFrequency_Id = DBTrackFile.ValCompare(Old_Corp.PayFrequency, c.PayFrequency); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
        //                        DT_Corp.PayMonthConcept_Id = DBTrackFile.ValCompare(Old_Corp.PayMonthConcept, c.PayMonthConcept); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        //                        db.Create(DT_Corp);
        //                        //db.SaveChanges();
        //                    }
        //                    blog.DBTrack = c.DBTrack;
        //                    db.PayProcessGroup.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    Msg.Add("  Record Updated");
        //                    return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return Json(new Object[] { blog.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
        //                }

        //            }
        //            return View();
        //        }
        //        catch (Exception ex)
        //        {
        //            LogFile Logfile = new LogFile();
        //            ErrorLog Err = new ErrorLog()
        //            {
        //                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                ExceptionMessage = ex.Message,
        //                ExceptionStackTrace = ex.StackTrace,
        //                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}
        public async Task<ActionResult> EditSave( PayProcessGroup c, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            string paymnthconc = form["PayDate_drop"] == "0" ? "" : form["PayDate_drop"];
            string PayFreq = form["PayFrequency_drop"] == "0" ? "" : form["PayFrequency_drop"];
            bool Auth = form["Autho_Allow"] == "true" ? true : false;
            string Values = form["PayrollPeriodlist"];

            c.PayMonthConcept_Id = paymnthconc != null && paymnthconc != "" ? int.Parse(paymnthconc) : 0;
            c.PayFrequency_Id = PayFreq != null && PayFreq != "" ? int.Parse(PayFreq) : 0;

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.PayProcessGroup.Include(e => e.PayrollPeriod).Where(e => e.Id == data).SingleOrDefault();
                        List<PayrollPeriod> PayrollPeriodno= new List<PayrollPeriod>();
                        if (Values != "" && Values != null)
                        {
                            var ids = Utility.StringIdsToListIds(Values);
                            foreach (var ca in ids)
                            {
                                var Values_val = db.PayrollPeriod.Find(ca);
                                PayrollPeriodno.Add(Values_val);
                            }
                            db_data.PayrollPeriod = PayrollPeriodno;
                        }
                        else
                        {
                            db_data.PayrollPeriod = null;
                        }
                        db.PayProcessGroup.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;

                        PayProcessGroup payprocessgroup = db.PayProcessGroup.Find(data);
                        TempData["CurrRowVersion"] = payprocessgroup.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = payprocessgroup.DBTrack.CreatedBy == null ? null : payprocessgroup.DBTrack.CreatedBy,
                                CreatedOn = payprocessgroup.DBTrack.CreatedOn == null ? null : payprocessgroup.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            payprocessgroup.PayMonthConcept_Id = c.PayMonthConcept_Id;
                            payprocessgroup.PayFrequency_Id = c.PayFrequency_Id;

                            payprocessgroup.Id = data;
                            payprocessgroup.Name = c.Name;
                            payprocessgroup.DBTrack = c.DBTrack;

                            db.Entry(payprocessgroup).State = System.Data.Entity.EntityState.Modified;


                            //using (var context = new DataBaseContext())
                            //{
                            PayProcessGroup blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;

                            blog = db.PayProcessGroup.Where(e => e.Id == data).Include(e => e.PayFrequency)
                                                    .Include(e => e.PayMonthConcept)
                                                    .SingleOrDefault();
                            originalBlogValues = db.Entry(blog).OriginalValues;
                            db.ChangeTracker.DetectChanges();
                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                            DT_PayProcessGroup DT_Corp = (DT_PayProcessGroup)obj;
                            DT_Corp.PayMonthConcept_Id = blog.PayMonthConcept == null ? 0 : blog.PayMonthConcept.Id;
                            DT_Corp.PayFrequency_Id = blog.PayFrequency == null ? 0 : blog.PayFrequency.Id;
                            db.Create(DT_Corp);
                            db.SaveChanges();
                        }
                        //}
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }
      
        public ActionResult GetPayrollPeriodLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.PayrollPeriod.ToList();
                IEnumerable<PayrollPeriod> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.PayrollPeriod.ToList().Where(d => d.FullDetails.Contains(data));

                }
                else
                {
                 //   var list1 = db.PayProcessGroup.SelectMany(e => e.PayrollPeriod).ToList();
                 //   var list2 = fall.Except(list1);

                    //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.CorporateCode, c.CorporateName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }

        
	}
}