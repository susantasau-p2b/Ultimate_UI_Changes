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
using Recruitment;
using Payroll;

namespace P2BUltimate.Controllers.ManPower.MainController
{
    public class CTCDefinitionController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /CTCDefination/
        public ActionResult Index()
        {
            return View("~/Views/ManPower/MainViews/CTCDefinition/Index.cshtml");
        }
        [HttpPost]

        public ActionResult Create(CTCDefinition c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string salhd = form["SalaryComponentList"] == "0" ? "" : form["SalaryComponentList"];


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

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            CTCDefinition ctc = new CTCDefinition()
                            {
                                EffectiveDate = c.EffectiveDate,
                                EndDate = c.EndDate,
                                SalaryHead = c.SalaryHead,

                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.CTCDefinition.Add(ctc);
                                //   var rtn_Obj = DBTrackFile.DBTrackSave("ManPower", null, db.ChangeTracker,"");
                                //  DT_CtcDefinition DT_Corp = (DT_Corporate)rtn_Obj;

                                //   db.Create(DT_Corp);
                                db.SaveChanges();
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


                                ts.Complete();
                                //   return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);



                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                //  return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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


        public class salhddetails
        {
            public Array salhd_id { get; set; }
            public Array salhd_details { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.CTCDefinition
                    .Include(e => e.SalaryHead)

                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        EffectiveDate = e.EffectiveDate,
                        EndDate = e.EndDate,

                        //  Action = e.DBTrack.Action
                    }).ToList();
                List<salhddetails> objlist = new List<salhddetails>();
                var N = db.CTCDefinition.Where(e => e.Id == data).Select(e => e.SalaryHead).ToList();
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

                var Corp = db.Corporate.Find(data);
                //TempData["RowVersion"] = Corp.RowVersion;
                // var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, objlist, JsonRequestBehavior.AllowGet });
            }
        }

        //[HttpPost]
        //public async Task<ActionResult> EditSave(CTCDefinition c, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            string salhd = form["SalaryComponentList"] == "0" ? "" : form["SalaryComponentList"];

        //            //  bool Auth = form["Autho_Action"] == "" ? false : true;
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;


        //            if (salhd != null)
        //            {
        //                var ids = Utility.StringIdsToListIds(salhd);
        //                var HolidayList = new List<SalaryHead>();
        //                foreach (var item in ids)
        //                {

        //                    int HolidayListid = Convert.ToInt32(item);
        //                    var val = db.SalaryHead.Include(e => e.SalHeadOperationType).Include(e => e.Frequency).Include(e => e.ProcessType).Include(e => e.RoundingMethod)
        //                                        .Where(e => e.Id == HolidayListid).SingleOrDefault();
        //                    if (val != null)
        //                    {
        //                        HolidayList.Add(val);
        //                    }
        //                }
        //                c.SalaryHead = HolidayList;
        //            }

        //            if (Auth == false)
        //            {


        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {

        //                        //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            CTCDefinition blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.CTCDefinition.Where(e => e.Id == data).Include(e => e.SalaryHead)
        //                                                      .SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            //c.DBTrack = new DBTrack
        //                            //{
        //                            //    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                            //    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                            //    Action = "M",
        //                            //    ModifiedBy = SessionManager.UserName,
        //                            //    ModifiedOn = DateTime.Now
        //                            //};

        //                            int a = EditS(salhd, data, c);



        //                            //using (var context = new DataBaseContext())
        //                           // {
        //                                //                              var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                                //DT_Corporate DT_Corp = (DT_Corporate)obj;
        //                                //DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
        //                                //DT_Corp.BusinessType_Id = blog.BusinessType == null ? 0 : blog.BusinessType.Id;
        //                                //DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
        //                                //db.Create(DT_Corp);
        //                            //}
        //                                db.SaveChanges();
        //                            await db.SaveChangesAsync();
        //                            ts.Complete();


        //                            //   return Json(new Object[] { c.Id, c.EffectiveDate, "Record Updated", JsonRequestBehavior.AllowGet });
        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.EffectiveDate.Value.ToShortDateString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        }
        //                    }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (CTCDefinition)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {
        //                            //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                            Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (CTCDefinition)databaseEntry.ToObject();
        //                            //   c.RowVersion = databaseValues.RowVersion;

        //                        }
        //                    }

        //                    //  return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                }
        //            }
        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    CTCDefinition blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    CTCDefinition Old_Corp = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.CTCDefinition.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }
        //                    //c.DBTrack = new DBTrack
        //                    //{
        //                    //    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                    //    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                    //    Action = "M",
        //                    //    IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                    //    ModifiedBy = SessionManager.UserName,
        //                    //    ModifiedOn = DateTime.Now
        //                    //};

        //                    //if (TempData["RowVersion"] == null)
        //                    //{
        //                    //    TempData["RowVersion"] = blog.RowVersion;
        //                    //}

        //                    CTCDefinition corp = new CTCDefinition()
        //                    {
        //                        EffectiveDate = c.EffectiveDate,
        //                        EndDate = c.EndDate,
        //                        Id = data,
        //                        //   DBTrack = c.DBTrack,
        //                        //    RowVersion = (Byte[])TempData["RowVersion"]
        //                    };


        //                    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        //var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Corporate", c.DBTrack);
        //                        //// var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                        //Old_Corp = context.Corporate.Where(e => e.Id == data).Include(e => e.BusinessType)
        //                        //    .Include(e => e.Address).Include(e => e.ContactDetails).SingleOrDefault();
        //                        //DT_Corporate DT_Corp = (DT_Corporate)obj;
        //                        //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                        //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
        //                        //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        //                        //db.Create(DT_Corp);
        //                        //db.SaveChanges();
        //                    }
        //                    //blog.DBTrack = c.DBTrack;
        //                    //db.Corporate.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    //   db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    // return Json(new Object[] { blog.Id, c.EffectiveDate, "Record Updated", JsonRequestBehavior.AllowGet });
        //                    Msg.Add("  Record Updated");
        //                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.EffectiveDate.Value.ToShortDateString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

        //[HttpPost]
        //public int EditS(string salhd, int data, CTCDefinition c)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {

        //        {
        //            if (salhd != null)
        //            {
        //                if (salhd != "")
        //                {
        //                    var val = db.CTCDefinition.Find(int.Parse(salhd));
        //                    //var val = db.Hobby.Where(e => e.Id == data).SingleOrDefault();
        //                    c.CTCDefinition = val;

        //                    var r = (from ca in db.SalaryHead
        //                             select new
        //                             {
        //                                 Id = ca.Id,
        //                                 LookupVal = ca.FullDetails
        //                             }).Where(e => e.Id == data).Distinct();

        //                    var add = db.CTCDefinition.Include(e => e.SalaryHead).Where(e => e.Id == data).SingleOrDefault();
        //                    IList<CTCDefinition> contactdetails = null;
        //                    if (add.SalaryHead != null)
        //                    //{
        //                    //    contactdetails = db.FamilyDetails.Where(x => x.ContactDetails == add.ContactDetails && x.Id == data).ToList();
        //                    //}
        //                    //else
        //                    {
        //                        contactdetails = db.CTCDefinition.Where(x => x.Id == data).ToList();
        //                    }
        //                    if (contactdetails != null)
        //                    {
        //                        foreach (var s in contactdetails)
        //                        {
        //                            s.SalaryHead = c.SalaryHead;
        //                            db.CTCDefinition.Attach(s);
        //                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                            // await db.SaveChangesAsync(false);
        //                            db.SaveChanges();
        //                            //TempData["RowVersion"] = s.RowVersion;
        //                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                var contactdetails = db.CTCDefinition.Include(e => e.SalaryHead).Where(x => x.Id == data).ToList();
        //                foreach (var s in contactdetails)
        //                {
        //                    s.SalaryHead = null;
        //                    db.CTCDefinition.Attach(s);
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                    //await db.SaveChangesAsync();
        //                    db.SaveChanges();
        //                    //  TempData["RowVersion"] = s.RowVersion;
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                }
        //            }


        //            var CurCorp = db.CTCDefinition.Find(data);
        //            //  TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //            //   db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //            //   if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //            //  {
        //            // c.DBTrack = dbT;
        //            CTCDefinition corp = new CTCDefinition()
        //            {
        //                EffectiveDate = c.EffectiveDate,
        //                EndDate = c.EndDate,
        //                Id = data,
        //                //    DBTrack = c.DBTrack
        //            };


        //            db.CTCDefinition.Attach(corp);
        //            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //            return 1;
        //        }
        //        return 0;
        //    }
        //}

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
                IEnumerable<CTCDefinition> CTCDefinition = null;
                if (gp.IsAutho == true)
                {
                    CTCDefinition = db.CTCDefinition.Include(e => e.SalaryHead).ToList();
                }
                else
                {
                    CTCDefinition = db.CTCDefinition.Include(e => e.SalaryHead).AsNoTracking().ToList();
                }

                IEnumerable<CTCDefinition> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = CTCDefinition;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EffectiveDate.Value.ToShortDateString().ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.EndDate.Value.ToShortDateString().ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.EffectiveDate.Value.ToShortDateString(), a.EndDate.Value.ToShortDateString(), a.Id }).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EffectiveDate.Value.ToShortDateString(), a.EndDate.Value.ToShortDateString(), a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = CTCDefinition;
                    Func<CTCDefinition, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EffectiveDate" ? c.EffectiveDate.Value.ToShortDateString() :
                                         gp.sidx == "EndDate" ? c.EndDate.Value.ToShortDateString() :
                                         "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.EffectiveDate.Value.ToShortDateString()), Convert.ToString(a.EndDate.Value.ToShortDateString()), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.EffectiveDate.Value.ToShortDateString()), Convert.ToString(a.EndDate.Value.ToShortDateString()), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.EffectiveDate.Value.ToShortDateString()), Convert.ToString(a.EndDate.Value.ToShortDateString()), a.Id }).ToList();
                    }
                    totalRecords = CTCDefinition.Count();
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

        public ActionResult Getsalarycomponent(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.SalaryHead.Include(e => e.SalHeadOperationType).Include(e => e.Frequency).Include(e => e.ProcessType).Include(e => e.RoundingMethod).ToList();


                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.SalaryHead.Include(e => e.SalHeadOperationType).Include(e => e.Frequency).Include(e => e.ProcessType).Include(e => e.RoundingMethod).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }


                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(CTCDefinition c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<String> Msg = new List<String>();
                try
                {
                    string Addrs = form["SalaryComponentList"] == "0" ? "" : form["SalaryComponentList"];

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    List<SalaryHead> ObjITsection = new List<SalaryHead>();

                    CTCDefinition pd = null;
                    pd = db.CTCDefinition.Include(q => q.SalaryHead).Where(q => q.Id == data).SingleOrDefault();
                    if (Addrs != "")
                    {
                        var ids = Utility.StringIdsToListIds(Addrs);
                        foreach (var ca in ids)
                        {
                            var value = db.SalaryHead.Find(ca);
                            ObjITsection.Add(value);
                            pd.SalaryHead = ObjITsection;

                        }
                    }
                    else
                    {
                        pd.SalaryHead = null;

                    }
                    if (ModelState.IsValid)
                    {
                        //DbContextTransaction transaction = db.Database.BeginTransaction();

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            CTCDefinition blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.CTCDefinition.Where(e => e.Id == data).Include(e => e.SalaryHead).SingleOrDefault();
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
                            var m1 = db.CTCDefinition.Where(e => e.Id == data).ToList();
                            foreach (var s in m1)
                            {
                                // s.AppraisalPeriodCalendar = null;
                                db.CTCDefinition.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }

                            var CurCorp = db.CTCDefinition.Find(data);
                            TempData["CurrRowVersion"] = CurCorp.RowVersion;
                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;

                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                            {
                                CTCDefinition corp = new CTCDefinition()
                                {
                                    EffectiveDate = c.EffectiveDate,
                                    EndDate = c.EndDate,
                                    SalaryHead = pd.SalaryHead,
                                    Id = data,
                                    DBTrack = c.DBTrack
                                };

                                db.CTCDefinition.Attach(corp);
                                db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            }
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            ts.Complete();

                            Msg.Add("Record Updated Successfully.");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = blog.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                    }

                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var clientValues = (CTCDefinition)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (CTCDefinition)databaseEntry.ToObject();
                        c.RowVersion = databaseValues.RowVersion;

                    }
                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                return View();
            }
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    CTCDefinition corporates = db.CTCDefinition.Include(e => e.SalaryHead).Where(e => e.Id == data).SingleOrDefault();



                    if (corporates.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? true : false
                            };
                            corporates.DBTrack = dbT;
                            db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, corporates.DBTrack);
                            //   DT_AppraisalPublish DT_Corp = (DT_AppraisalPublish)rtn_Obj;
                            //  DT_Corp.AppraisalPeriodCalendar_Id = corporates.AppraisalPeriodCalendar == null ? 0 : corporates.AppraisalPeriodCalendar.Id;
                            //  db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            //Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                            db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                            // var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, dbT);
                            // DT_Corp = (DT_AppraisalPublish)rtn_Obj;
                            //   DT_Corp.AppraisalPeriodCalendar_Id = corporates.AppraisalPeriodCalendar == null ? 0 : corporates.AppraisalPeriodCalendar.Id;
                            // db.Create(DT_Corp);

                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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