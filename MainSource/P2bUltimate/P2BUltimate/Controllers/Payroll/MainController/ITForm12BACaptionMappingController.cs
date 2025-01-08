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
    public class ITForm12BACaptionMappingController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /ITForm12BACaptionMapping/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ITForm12BACaptionMapping/Index.cshtml");
        }
        public ActionResult Create(ITForm12BACaptionMapping c, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var perqname = form["CategoryList_DDL"] == null ? "" : form["CategoryList_DDL"];
                string salhd = form["AddressList"];
                try
                {
                    List<string> Msg = new List<string>();

                    if (perqname != null)
                    {
                        var val = db.LookupValue.Find(int.Parse(perqname));
                        c.PerquisiteName = val;
                    }

                    c.SalaryHead = null;
                    List<SalaryHead> salaryhd = new List<SalaryHead>();
                    if (salhd != null)
                    {
                        var ids = Utility.StringIdsToListIds(salhd);
                        foreach (var item in ids)
                        {
                            var salhdd = db.SalaryHead.Find(item);
                            salaryhd.Add(salhdd);
                            
                        }
                        c.SalaryHead = salaryhd;
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.ITForm12BACaptionMapping.Any(o => o.SrNo == c.SrNo))
                            {
                                Msg.Add("  Sr No Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            if (db.ITForm12BACaptionMapping.Any(o => o.PerquisiteName.Id == c.PerquisiteName.Id))
                            {
                                Msg.Add("  Perquisite Name Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = true };

                            ITForm12BACaptionMapping Itform = new ITForm12BACaptionMapping()
                            {
                                SrNo = c.SrNo,
                                PerquisiteName = c.PerquisiteName,
                                SalaryHead = c.SalaryHead,
                                DBTrack = c.DBTrack

                            };

                            db.ITForm12BACaptionMapping.Add(Itform);
                            //   var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, c.DBTrack);
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("Data Created Successfully");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder("");
                        foreach (ModelState modelstate in ModelState.Values)
                        {
                            foreach (var item in modelstate.Errors)
                            {
                                sb.Append(item.ErrorMessage);
                                sb.Append("." + "/n");
                            }
                        }
                        var errormsg = sb.ToString();
                        Msg.Add(errormsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }


        public ActionResult Getsalarycomponent(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.SalaryHead.Include(e => e.SalHeadOperationType)
                    .Include(e => e.Frequency).Include(e => e.ProcessType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "PERK").ToList();


                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.SalaryHead.Include(e => e.SalHeadOperationType).Include(e => e.Frequency).Include(e => e.ProcessType).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }


                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }

        public class salhddetails
        {
            public Array salhd_id { get; set; }
            public Array salhd_details { get; set; }
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var itform = db.ITForm12BACaptionMapping.Include(a => a.PerquisiteName).Include(a => a.SalaryHead).Where(a => a.Id == data).SingleOrDefault();

                var Q = db.ITForm12BACaptionMapping.Include(a => a.PerquisiteName)
                    .Include(a => a.SalaryHead)
                    .Where(a => a.Id == data)
                    .Select
                    (e => new
                    {
                        SrNo = e.SrNo,
                        BusinessType_Id = e.PerquisiteName.Id == null ? 0 : e.PerquisiteName.Id,
                        Action = e.DBTrack.Action
                    }).ToList();

                List<salhddetails> objlist = new List<salhddetails>();
                var N = db.ITForm12BACaptionMapping.Where(e => e.Id == data).Select(e => e.SalaryHead).ToList();
                if (N != null && N.Count > 0)
                {
                    foreach (var ca in N)
                    {
                        objlist.Add(new salhddetails
                        {

                            salhd_id = ca.Select(e => e.Id).ToArray(),
                            salhd_details = ca.Select(e => e.FullDetails).ToArray()
                        });
                    }
                }

                var Corp = db.ITForm12BACaptionMapping.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, objlist, JsonRequestBehavior.AllowGet });

                // return View();
            }
        }


        [HttpPost]
        public async Task<ActionResult> EditSave(ITForm12BACaptionMapping c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {


                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    var perqname = form["CategoryList_DDL"] == null ? "" : form["CategoryList_DDL"];
                    string salhd = form["AddressList"];


                    if (perqname != null)
                    {
                        var val = db.LookupValue.Find(int.Parse(perqname));
                        c.PerquisiteName = val;
                    }

                    c.SalaryHead = null;
                    if (salhd != null)
                    {
                        var ids = Utility.StringIdsToListIds(salhd);
                        var HolidayList = new List<SalaryHead>();
                        foreach (var item in ids)
                        {

                            int HolidayListid = Convert.ToInt32(item);
                            var val = db.SalaryHead.Include(e => e.SalHeadOperationType).Include(e => e.Frequency).Include(e => e.ProcessType).Include(e => e.RoundingMethod)
                                                .Where(e => e.Id == HolidayListid).SingleOrDefault();
                            if (val != null)
                            {
                                HolidayList.Add(val);
                            }
                        }
                        c.SalaryHead = HolidayList;
                    }

                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    ITForm12BACaptionMapping blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.ITForm12BACaptionMapping.Where(e => e.Id == data).Include(e => e.SalaryHead).Include(e => e.PerquisiteName)
                                                              .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    //int a = EditS(perqname, salhd, data, c, c.DBTrack);

                                    if (perqname != null)
                                    {
                                        if (perqname != "")
                                        {
                                            var val = db.LookupValue.Find(int.Parse(perqname));
                                            c.PerquisiteName = val;

                                            var type = db.ITForm12BACaptionMapping.Include(e => e.PerquisiteName).Where(e => e.Id == data).SingleOrDefault();
                                            IList<ITForm12BACaptionMapping> typedetails = null;
                                            if (type.PerquisiteName != null)
                                            {
                                                typedetails = db.ITForm12BACaptionMapping.Where(x => x.PerquisiteName.Id == type.PerquisiteName.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.ITForm12BACaptionMapping.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.PerquisiteName = c.PerquisiteName;
                                                db.ITForm12BACaptionMapping.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var BusiTypeDetails = db.ITForm12BACaptionMapping.Include(e => e.PerquisiteName).Where(x => x.Id == data).ToList();
                                            foreach (var s in BusiTypeDetails)
                                            {
                                                s.PerquisiteName = null;
                                                db.ITForm12BACaptionMapping.Attach(s);
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
                                        var BusiTypeDetails = db.ITForm12BACaptionMapping.Include(e => e.PerquisiteName).Where(x => x.Id == data).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.PerquisiteName = null;
                                            db.ITForm12BACaptionMapping.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }



                                    if (salhd != null)
                                    {
                                        if (salhd != "")
                                        {
                                           // var val = db.ITForm12BACaptionMapping.Find(int.Parse(salhd));
                                            //var val = db.Hobby.Where(e => e.Id == data).SingleOrDefault();

                                            //var r = (from ca in db.SalaryHead
                                            //         select new
                                            //         {
                                            //             Id = ca.Id,
                                            //             LookupVal = ca.FullDetails
                                            //         }).Where(e => e.Id == data).Distinct();

                                            var add = db.ITForm12BACaptionMapping.Include(e => e.PerquisiteName).Include(e => e.SalaryHead).Where(e => e.Id == data).SingleOrDefault();
                                            IList<ITForm12BACaptionMapping> contactdetails = null;
                                            if (add.SalaryHead != null)
                                            {
                                                contactdetails = db.ITForm12BACaptionMapping.Where(x => x.Id == data).ToList();
                                            }
                                            if (contactdetails != null)
                                            {
                                                foreach (var s in contactdetails)
                                                {
                                                    s.SalaryHead = c.SalaryHead;
                                                    db.ITForm12BACaptionMapping.Attach(s);
                                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                    // await db.SaveChangesAsync(false);
                                                    db.SaveChanges();
                                                    //TempData["RowVersion"] = s.RowVersion;
                                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var contactdetails = db.ITForm12BACaptionMapping.Include(e => e.SalaryHead).Where(x => x.Id == data).ToList();
                                        foreach (var s in contactdetails)
                                        {
                                            s.SalaryHead = null;
                                            db.ITForm12BACaptionMapping.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            //  TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    var m1 = db.ITForm12BACaptionMapping.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.ITForm12BACaptionMapping.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    var CurCorp = db.ITForm12BACaptionMapping.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        ITForm12BACaptionMapping corp = new ITForm12BACaptionMapping()
                                        {
                                            SrNo = c.SrNo,
                                            Id = data,
                                            DBTrack = c.DBTrack
                                        };


                                        db.ITForm12BACaptionMapping.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                    }

                                    using (var context = new DataBaseContext())
                                    {
                                        // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        //DT_Corporate DT_Corp = (DT_Corporate)obj;
                                        //DT_Corp.Address_Id = blog.PerquisiteName == null ? 0 : blog.PerquisiteName.Id;
                                        //DT_Corp.BusinessType_Id = blog.BusinessType == null ? 0 : blog.BusinessType.Id;
                                        //DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
                                        //db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();


                                    //   return Json(new Object[] { c.Id, c.EffectiveDate, "Record Updated", JsonRequestBehavior.AllowGet });
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.SrNo.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (ITForm12BACaptionMapping)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var databaseValues = (ITForm12BACaptionMapping)databaseEntry.ToObject();
                                    //   c.RowVersion = databaseValues.RowVersion;

                                }
                            }

                            //  return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            ITForm12BACaptionMapping blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            ITForm12BACaptionMapping Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.ITForm12BACaptionMapping.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            //c.DBTrack = new DBTrack
                            //{
                            //    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                            //    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                            //    Action = "M",
                            //    IsModified = blog.DBTrack.IsModified == true ? true : false,
                            //    ModifiedBy = SessionManager.UserName,
                            //    ModifiedOn = DateTime.Now
                            //};

                            //if (TempData["RowVersion"] == null)
                            //{
                            //    TempData["RowVersion"] = blog.RowVersion;
                            //}

                            ITForm12BACaptionMapping corp = new ITForm12BACaptionMapping()
                            {
                                SrNo = c.SrNo,
                                Id = data,
                                //   DBTrack = c.DBTrack,
                                //    RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                //var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "ITForm12BACaptionMapping", c.DBTrack);
                                //// var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                //Old_Corp = context.ITForm12BACaptionMapping.Where(e => e.Id == data).Include(e => e.BusinessType)
                                //    .Include(e => e.PerquisiteName).Include(e => e.ContactDetails).SingleOrDefault();
                                //DT_Corporate DT_Corp = (DT_Corporate)obj;
                                //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.PerquisiteName, c.PerquisiteName);//Old_Corp.PerquisiteName == c.PerquisiteName ? 0 : Old_Corp.PerquisiteName == null && c.PerquisiteName != null ? c.PerquisiteName.Id : Old_Corp.PerquisiteName.Id;
                                //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                //db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            //blog.DBTrack = c.DBTrack;
                            //db.ITForm12BACaptionMapping.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            //   db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            // return Json(new Object[] { blog.Id, c.EffectiveDate, "Record Updated", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.SrNo.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }

                    }
                    return View();
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
        public int EditS(string perqname, string salhd, int data, ITForm12BACaptionMapping c, DBTrack dbT)
        {
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    if (perqname != null)
                    {
                        if (perqname != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(perqname));
                            c.PerquisiteName = val;

                            var type = db.ITForm12BACaptionMapping.Include(e => e.PerquisiteName).Where(e => e.Id == data).SingleOrDefault();
                            IList<ITForm12BACaptionMapping> typedetails = null;
                            if (type.PerquisiteName != null)
                            {
                                typedetails = db.ITForm12BACaptionMapping.Where(x => x.PerquisiteName.Id == type.PerquisiteName.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.ITForm12BACaptionMapping.Where(x => x.Id == data).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.PerquisiteName = c.PerquisiteName;
                                db.ITForm12BACaptionMapping.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        else
                        {
                            var BusiTypeDetails = db.ITForm12BACaptionMapping.Include(e => e.PerquisiteName).Where(x => x.Id == data).ToList();
                            foreach (var s in BusiTypeDetails)
                            {
                                s.PerquisiteName = null;
                                db.ITForm12BACaptionMapping.Attach(s);
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
                        var BusiTypeDetails = db.ITForm12BACaptionMapping.Include(e => e.PerquisiteName).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.PerquisiteName = null;
                            db.ITForm12BACaptionMapping.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }



                    if (salhd != null)
                    {
                        if (salhd != "")
                        {
                            var val = db.ITForm12BACaptionMapping.Find(int.Parse(salhd));
                            //var val = db.Hobby.Where(e => e.Id == data).SingleOrDefault();

                            var r = (from ca in db.SalaryHead
                                     select new
                                     {
                                         Id = ca.Id,
                                         LookupVal = ca.FullDetails
                                     }).Where(e => e.Id == data).Distinct();

                            var add = db.ITForm12BACaptionMapping.Include(e => e.PerquisiteName).Include(e => e.SalaryHead).Where(e => e.Id == data).SingleOrDefault();
                            IList<ITForm12BACaptionMapping> contactdetails = null;
                            if (add.SalaryHead != null)
                            {
                                contactdetails = db.ITForm12BACaptionMapping.Where(x => x.Id == data).ToList();
                            }
                            if (contactdetails != null)
                            {
                                foreach (var s in contactdetails)
                                {
                                    s.SalaryHead = c.SalaryHead;
                                    db.ITForm12BACaptionMapping.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    // await db.SaveChangesAsync(false);
                                    db.SaveChanges();
                                    //TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                        }
                    }
                    else
                    {
                        var contactdetails = db.ITForm12BACaptionMapping.Include(e => e.SalaryHead).Where(x => x.Id == data).ToList();
                        foreach (var s in contactdetails)
                        {
                            s.SalaryHead = null;
                            db.ITForm12BACaptionMapping.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            //  TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }


                    var CurCorp = db.ITForm12BACaptionMapping.Find(data);
                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                    {
                        c.DBTrack = dbT;
                        ITForm12BACaptionMapping corp = new ITForm12BACaptionMapping()
                        {
                            SrNo = c.SrNo,
                            Id = data,
                            DBTrack = c.DBTrack
                        };


                        db.ITForm12BACaptionMapping.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                        return 1;
                    }
                    return 0;
                }
            }
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<ITForm12BACaptionMapping> ITForm12BACaptionMapping = null;
                if (gp.IsAutho == true)
                {
                    ITForm12BACaptionMapping = db.ITForm12BACaptionMapping.Include(e => e.PerquisiteName).Include(e => e.SalaryHead).ToList();
                }
                else
                {
                    ITForm12BACaptionMapping = db.ITForm12BACaptionMapping.Include(e => e.PerquisiteName).Include(e => e.SalaryHead).AsNoTracking().ToList();
                }

                IEnumerable<ITForm12BACaptionMapping> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = ITForm12BACaptionMapping;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.SrNo.ToString().Contains(gp.searchString))
                            || (e.PerquisiteName.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.SrNo, a.PerquisiteName.LookupVal, a.Id }).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.SrNo.ToString(), a.PerquisiteName != null ? Convert.ToString(a.PerquisiteName.LookupVal) : "", a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = ITForm12BACaptionMapping;
                    Func<ITForm12BACaptionMapping, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>   gp.sidx == "SrNo" ? c.SrNo.ToString() :
                                           gp.sidx == "PerquisiteName" ? c.PerquisiteName.LookupVal.ToString() :

                                         "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.SrNo), a.PerquisiteName != null ? Convert.ToString(a.PerquisiteName.LookupVal) : "", a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.SrNo), a.PerquisiteName != null ? Convert.ToString(a.PerquisiteName.LookupVal) : "", a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.SrNo), a.PerquisiteName != null ? Convert.ToString(a.PerquisiteName.LookupVal) : "", a.Id }).ToList();
                    }
                    totalRecords = ITForm12BACaptionMapping.Count();
                }
                if (totalRecords > 0)
                {
                    totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
                }
                if (gp.page > totalPages)
                {
                    gp.page = totalPages;
                }
                var JsonData = new
                {
                    page = gp.page,
                    rows = jsonData,
                    records = totalRecords,
                    total = totalPages
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost]
        public async Task<ActionResult> Delete(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //    var EmpSocialInfo = db.Employee.Include(e => e.EmpSocialInfo).Include(e => e.EmpSocialInfo.SocialActivities)
                    //                  .Include(e => e.EmpSocialInfo.Category)
                    //                  .Include(e => e.EmpSocialInfo.Religion)
                    //                  .Include(e => e.EmpSocialInfo.Caste)
                    //                  .Include(e => e.EmpSocialInfo.SubCaste)
                    //                  .Where(e => e.Id == data).SingleOrDefault();

                    var EmpSocialInfo = db.ITForm12BACaptionMapping.Include(a => a.PerquisiteName).Include(a => a.SalaryHead).Where(a => a.Id == data).SingleOrDefault();


                    // var EmpSocialInfo = db.EmpSocialInfo.Include(e=>e.Caste).Include(e=>e.Category).Include(e=>e.SubCaste).Include(e=>e.Religion).Include(e => e.SocialActivities).Where(e => e.Id == data).SingleOrDefault();

                    //if (EmpSocialInfo.DBTrack.IsModified == true)
                    //{
                    //    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    //    {
                    //        //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                    //        DBTrack dbT = new DBTrack
                    //        {
                    //            Action = "D",
                    //            CreatedBy = EmpSocialInfo.DBTrack.CreatedBy != null ? EmpSocialInfo.DBTrack.CreatedBy : null,
                    //            CreatedOn = EmpSocialInfo.DBTrack.CreatedOn != null ? EmpSocialInfo.DBTrack.CreatedOn : null,
                    //            IsModified = EmpSocialInfo.DBTrack.IsModified == true ? true : false
                    //        };
                    //        EmpSocialInfo.DBTrack = dbT;
                    //        db.Entry(EmpSocialInfo).State = System.Data.Entity.EntityState.Modified;
                    //      //  var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, EmpSocialInfo.DBTrack);
                    //      //  DT_ITForm12BACaptionMapping DT_OBJ = (DT_ITForm12BACaptionMapping)rtn_Obj;
                    //        //db.Create(DT_OBJ);

                    //        await db.SaveChangesAsync();
                    //        ts.Complete();
                    //        Msg.Add("  Data removed successfully.  ");
                    //        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    //        //Msg.Add("  Data removed successfully.  ");
                    //        //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    //    }
                    //}
                    //else
                    //{
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        //  var v = EmpSocialInfo.Where(a => a.EmpSocialInfo.Id == EmpSocialInfo.Id).ToList();
                        //  db.Employee.RemoveRange(v);
                        // db.SaveChanges();


                        var selectedValues = EmpSocialInfo.SalaryHead;
                        var lkValue = new HashSet<int>(EmpSocialInfo.SalaryHead.Select(e => e.Id));
                        if (lkValue.Count > 0)
                        {
                            Msg.Add(" Child record exists.Cannot remove it..  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                        }
                        //  db.Entry(EmpSocialInfo.SalaryHead).State = System.Data.Entity.EntityState.Deleted;
                        db.ITForm12BACaptionMapping.Remove(EmpSocialInfo);

                        //if (v!=null)
                        //{
                        //Msg.Add("child record Exist in Employee Master");
                        //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //}
                        db.SaveChanges();
                        ts.Complete();
                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        // Msg.Add("  Data removed successfully.  ");
                        // return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    //}
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
