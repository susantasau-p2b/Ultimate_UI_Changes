///
/// Created by Tanushri
///

using P2b.Global;
using Payroll;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Threading.Tasks;
using Training;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Training.MainController
{
    public class TrainingInstituteController : Controller
    {

        // private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/TrainingInstitute/Index.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/Training/_TrainingInstitute.cshtml");
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
                IEnumerable<TrainingInstitute> TrainingInstitute = null;
                if (gp.IsAutho == true)
                {
                    TrainingInstitute = db.TrainingInstitute.Include(e => e.InstituteType).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    TrainingInstitute = db.TrainingInstitute.Include(e => e.InstituteType).AsNoTracking().ToList();
                }

                IEnumerable<TrainingInstitute> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = TrainingInstitute;
                    if (gp.searchOper.Equals("eq"))
                    {
                        if (gp.searchField == "Id")
                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "Code")
                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name }).Where((e => (e.Code.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "Name")
                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name }).Where((e => (e.Name.ToString().Contains(gp.searchString)))).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name, a.InstituteType != null ? Convert.ToString(a.InstituteType.LookupVal) : "" }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = TrainingInstitute;
                    Func<TrainingInstitute, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.Code :
                                         gp.sidx == "Name" ? c.Name :
                                         gp.sidx == "InstituteType" ? c.InstituteType.LookupVal : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), a.InstituteType != null ? Convert.ToString(a.InstituteType.LookupVal) : "" }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), a.InstituteType != null ? Convert.ToString(a.InstituteType.LookupVal) : "" }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name, a.InstituteType != null ? Convert.ToString(a.InstituteType.LookupVal) : "" }).ToList();
                    }
                    totalRecords = TrainingInstitute.Count();
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


        public ActionResult GetContactDetLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ContactDetails.Include(e => e.ContactNumbers).ToList();
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
                var list1 = db.Corporate.ToList().Select(e => e.ContactDetails);
                var list2 = fall.Except(list1);

                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
            // return View();
        }
        //public ActionResult EditContactDetails_partial(int data)
        //{
        //    var val = db.ContactDetails.Where(e => e.Id == data).SingleOrDefault();

        //    var r = (from ca in db.ContactDetails
        //             select new
        //             {
        //                 Id = ca.Id,
        //                 FullDetails = ca.FullContactDetails
        //             }).Where(e => e.Id == data).Distinct();

        //    return Json(r, JsonRequestBehavior.AllowGet);
        //}








        //public ActionResult GetContactDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.ContactDetails.ToList();
        //        IEnumerable<TrainingInstitute> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.TrainingInstitute.ToList().Where(d => d.Name.Contains(data));

        //        }
        //        else
        //        {
        //            //var list1 = db.TrainingInstitute.ToList().SelectMany(e => e.ContactDetails);
        //            //var list2 = fall.Except(list1);
        //            //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
        //            //var list1 = db.TrainingInstitute.Include(e => e.ContactDetails).SelectMany(e => e.ContactDetails);
        //            //var list2 = fall.Except(list1);
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();
        //            //var result_1 = (from c in fall
        //            //                select new { c.Id, c.CorporateCode, c.CorporateName });
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.Name }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    // return View();
        //}
        public ActionResult GetContactDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ContactDetails.Include(e => e.ContactNumbers).ToList();
                IEnumerable<ContactDetails> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.ContactDetails.ToList().Where(d => d.FullContactDetails.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.FacultyInternalCode, c.FacultyInternalName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullContactDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            // return View();
        }





        //public ActionResult GetLookupDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        IEnumerable<ContactDetails>all;
        //        var fall = db.ContactDetails.ToList();
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.ContactDetails.ToList().Where(d => d.StartingSlab.ToString().Contains(data));
        //            var result = (from c in all
        //                          select new { c.Id, c.StartingSlab }).Distinct();
        //            return Json(result, JsonRequestBehavior.AllowGet);

        //       }
        //        else
        //        {
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.StartingSlab }).Distinct();
        //            //var result_1 = (from c in fall
        //            //                select new { c.Id, c.CorporateCode, c.CorporateName });
        //            return Json(r, JsonRequestBehavior.AllowGet);

        //        }

        //    }
        //    // return View();
        //}


        private MultiSelectList GetContactDetailsValues(List<int> selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<ContactDetails> lkval = new List<ContactDetails>();
                lkval = db.ContactDetails.ToList();
                return new MultiSelectList(lkval, "Id", "FullDetails", selectedValues);
            }
        }


        public class A
        {
            public string name { get; set; }
            public int myprop { get; set; }
        }
        public class B : A
        {
            public string name1 { get; set; }
            public int myprop1 { get; set; }
        }


        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(TrainingInstitute NOBJ, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    string LKVAL = form["InstituteTypelist"] == "0" ? "" : form["InstituteTypelist"];
                    if (LKVAL != null)
                    {
                        if (LKVAL != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(LKVAL));
                            NOBJ.InstituteType = val;
                        }
                    }

                    NOBJ.ContactDetails = null;
                    List<ContactDetails> OBJ = new List<ContactDetails>();
                    string Values = form["ContactDetailsList"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.ContactDetails.Find(ca);
                            OBJ.Add(OBJ_val);
                            NOBJ.ContactDetails = OBJ;
                        }
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            //if (db.TrainingInstitute.Any(o => o.Name == NOBJ.Name))
                            //{

                            //    Msg.Add("  TrainingInstitute Already Exists.  ");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //}

                            NOBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            TrainingInstitute TrainingInstitute = new TrainingInstitute()
                            {
                                Name = NOBJ.Name == null ? "" : NOBJ.Name.Trim(),
                                Code = NOBJ.Code == null ? "" : NOBJ.Code.Trim(),
                                ContactPerson = NOBJ.ContactPerson == null ? "" : NOBJ.ContactPerson.Trim(),
                                InstituteType = NOBJ.InstituteType,
                                ContactDetails = NOBJ.ContactDetails,
                                DBTrack = NOBJ.DBTrack
                            };
                            try
                            {
                                //db.TrainingInstitute.Add(TrainingInstitute);
                                ////DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, NOBJ.DBTrack);
                                //DBTrackFile.DBTrackSave("Training/Training", "C", TrainingInstitute, null, "TrainingInstitute", null);
                                //db.SaveChanges();
                                //ts.Complete();
                                //return Json(new Object[] { TrainingInstitute.Id, TrainingInstitute.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });


                                db.TrainingInstitute.Add(TrainingInstitute);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, NOBJ.DBTrack);
                                DT_TrainingInstitute DT_OBJ = (DT_TrainingInstitute)rtn_Obj;

                                DT_OBJ.InstituteType_Id = NOBJ.InstituteType == null ? 0 : NOBJ.InstituteType.Id;
                                db.Create(DT_OBJ);
                                db.SaveChanges();
                                ts.Complete();

                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = TrainingInstitute.Id, Val = TrainingInstitute.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = NOBJ.Id });
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






        public int EditS(string Lkval, string OBJ, int data, TrainingInstitute ESOBJ, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Lkval != null)
                {
                    if (Lkval != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Lkval));
                        ESOBJ.InstituteType = val;

                        var type = db.TrainingInstitute.Include(e => e.InstituteType).Where(e => e.Id == data).SingleOrDefault();
                        IList<TrainingInstitute> typedetails = null;
                        if (type.InstituteType != null)
                        {
                            typedetails = db.TrainingInstitute.Where(x => x.InstituteType.Id == type.InstituteType.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.TrainingInstitute.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.InstituteType = ESOBJ.InstituteType;
                            db.TrainingInstitute.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var InstituteTypeDetails = db.TrainingInstitute.Include(e => e.InstituteType).Where(x => x.Id == data).ToList();
                        foreach (var s in InstituteTypeDetails)
                        {
                            s.InstituteType = null;
                            db.TrainingInstitute.Attach(s);
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
                    var InstituteTypeDetails = db.TrainingInstitute.Include(e => e.InstituteType).Where(x => x.Id == data).ToList();
                    foreach (var s in InstituteTypeDetails)
                    {
                        s.InstituteType = null;
                        db.TrainingInstitute.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                //var db_data = db.TrainingInstitute.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                //List<ContactDetails> lookupval = new List<ContactDetails>();
                //string Values = OBJ;

                //if (Values != null)
                //{
                //    var ids = Utility.StringIdsToListIds(Values);
                //    foreach (var ca in ids)
                //    {
                //        var Lookup_val = db.ContactDetails.Find(ca);
                //        lookupval.Add(Lookup_val);
                //        db_data.ContactDetails = lookupval;
                //    }
                //}
                //else
                //{
                //    db_data.ContactDetails = null;
                //}

                if (OBJ != null)
                {
                    if (OBJ != "")
                    {

                        List<int> IDs = OBJ.Split(',').Select(e => int.Parse(e)).ToList();
                        foreach (var k in IDs)
                        {
                            var value = db.ContactDetails.Find(k);
                            ESOBJ.ContactDetails = new List<ContactDetails>();
                            ESOBJ.ContactDetails.Add(value);
                        }
                    }
                }
                else
                {
                    var Details = db.TrainingInstitute.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                    foreach (var s in Details)
                    {
                        s.ContactDetails = null;
                        db.TrainingInstitute.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                //db.TrainingInstitute.Attach(db_data);
                //db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
                //TempData["RowVersion"] = db_data.RowVersion;
                //db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;



                var CurOBJ = db.TrainingInstitute.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    ESOBJ.DBTrack = dbT;
                    TrainingInstitute BOBJ = new TrainingInstitute()
                    {
                        Name = ESOBJ.Name,
                        Code = ESOBJ.Code,
                        ContactPerson = ESOBJ.ContactPerson,
                        Id = data,
                        DBTrack = ESOBJ.DBTrack
                    };


                    db.TrainingInstitute.Attach(BOBJ);
                    db.Entry(BOBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(BOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    ////  DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }


        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            TrainingInstitute TrainingInstitute = db.TrainingInstitute.Include(e => e.ContactDetails)
                              .FirstOrDefault(e => e.Id == auth_id);
                            TrainingInstitute.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = TrainingInstitute.DBTrack.ModifiedBy != null ? TrainingInstitute.DBTrack.ModifiedBy : null,
                                CreatedBy = TrainingInstitute.DBTrack.CreatedBy != null ? TrainingInstitute.DBTrack.CreatedBy : null,
                                CreatedOn = TrainingInstitute.DBTrack.CreatedOn != null ? TrainingInstitute.DBTrack.CreatedOn : null,
                                IsModified = TrainingInstitute.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };
                            db.TrainingInstitute.Attach(TrainingInstitute);
                            db.Entry(TrainingInstitute).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(TrainingInstitute).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            ////db.SaveChanges();
                            //await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //    //  Lkval = db.TrainingInstitute.Include(e => e.ContactDetails)
                            //    //.Include(e => e.ContactDetails)
                            //    //.Include(e => e.InstituteType).FirstOrDefault(e => e.Id == auth_id);
                            //    //DBTrackFile.DBTrackSave("Training/Training", "M", TrainingInstitute, null, "TrainingInstitute", TrainingInstitute.DBTrack);
                            //}
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, TrainingInstitute.DBTrack);
                            DT_TrainingInstitute DT_OBJ = (DT_TrainingInstitute)rtn_Obj;

                            DT_OBJ.InstituteType_Id = TrainingInstitute.InstituteType == null ? 0 : TrainingInstitute.InstituteType.Id;
                            // DT_OBJ.ContactDetails_Id = TrainingInstitute.ContactDetails == null ? 0 : TrainingInstitute.ContactDetails.Id;
                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();


                            ts.Complete();


                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = TrainingInstitute.Id, Val = TrainingInstitute.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else if (auth_action == "M")
                    {
                        TrainingInstitute Old_OBJ = db.TrainingInstitute.Include(e => e.InstituteType)

                                                          .Include(e => e.ContactDetails).Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_TrainingInstitute Curr_OBJ = db.DT_TrainingInstitute
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_OBJ != null)
                        {
                            TrainingInstitute TrainingInstitute = new TrainingInstitute();

                            string LKVAL = Curr_OBJ.InstituteType_Id == null ? null : Curr_OBJ.InstituteType_Id.ToString();
                            string CONVAL = Curr_OBJ.ContactDetails_Id == null ? null : Curr_OBJ.ContactDetails_Id.ToString();
                            string ContactDetails = Curr_OBJ.ContactDetails_Id == null ? null : Curr_OBJ.ContactDetails_Id.ToString();
                            TrainingInstitute.Name = Curr_OBJ.Name == null ? Old_OBJ.Name : Curr_OBJ.Name;
                            TrainingInstitute.Code = Curr_OBJ.Code == null ? Old_OBJ.Code : Curr_OBJ.Code;
                            TrainingInstitute.ContactPerson = Curr_OBJ.ContactPerson == null ? Old_OBJ.ContactPerson : Curr_OBJ.ContactPerson;
                            //      corp.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        TrainingInstitute.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
                                            CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_OBJ.DBTrack.ModifiedBy == null ? null : Old_OBJ.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_OBJ.DBTrack.ModifiedOn == null ? null : Old_OBJ.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        int a = EditS(LKVAL, CONVAL, auth_id, TrainingInstitute, TrainingInstitute.DBTrack);


                                        await db.SaveChangesAsync();

                                        ts.Complete();

                                        Msg.Add(" Record Authorised ");
                                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (TrainingInstitute)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {

                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


                                    }
                                    else
                                    {
                                        var databaseValues = (TrainingInstitute)databaseEntry.ToObject();
                                        TrainingInstitute.RowVersion = databaseValues.RowVersion;
                                    }
                                }

                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else


                            Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //TrainingInstitute Lkval = db.TrainingInstitute.Find(auth_id);
                            TrainingInstitute TrainingInstitute = db.TrainingInstitute.AsNoTracking().Include(e => e.ContactDetails)
                                                                        .FirstOrDefault(e => e.Id == auth_id);
                            var selectedValues = TrainingInstitute.ContactDetails;
                            TrainingInstitute.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = TrainingInstitute.DBTrack.ModifiedBy != null ? TrainingInstitute.DBTrack.ModifiedBy : null,
                                CreatedBy = TrainingInstitute.DBTrack.CreatedBy != null ? TrainingInstitute.DBTrack.CreatedBy : null,
                                CreatedOn = TrainingInstitute.DBTrack.CreatedOn != null ? TrainingInstitute.DBTrack.CreatedOn : null,
                                IsModified = TrainingInstitute.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };
                            db.TrainingInstitute.Attach(TrainingInstitute);
                            db.Entry(TrainingInstitute).State = System.Data.Entity.EntityState.Deleted;
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {
                                TrainingInstitute.ContactDetails = selectedValues;
                                // DBTrackFile.DBTrackSave("Training/Training", "D", TrainingInstitute, null, "TrainingInstitute", TrainingInstitute.DBTrack);
                            }
                            db.Entry(TrainingInstitute).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

        //[HttpPost]
        //public async Task<ActionResult> EditSave(TrainingInstitute L, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //        try
        //        {

        //            string Corp = form["InstituteTypelist"] == "" ? "" : form["InstituteTypelist"];
        //            string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;

        //            var blog1 = db.TrainingInstitute.Where(e => e.Id == data).Include(e => e.ContactDetails).Include(e => e.InstituteType).SingleOrDefault();

        //            blog1.ContactDetails = null;

        //            List<ContactDetails> ContactDetailsa = new List<ContactDetails>();
        //            if (ContactDetails != "0")
        //            {
        //                var ids = Utility.StringIdsToListIds(ContactDetails);
        //                foreach (var ca in ids)
        //                {
        //                    var Doctor_val = db.ContactDetails.Find(ca);
        //                    ContactDetailsa.Add(Doctor_val);
        //                }
        //                blog1.ContactDetails = ContactDetailsa;
        //            }
        //            else
        //            {
        //                L.ContactDetails = null;
        //                blog1.ContactDetails = null;

        //            }

        //            if (ModelState.IsValid)
        //            {
        //                try
        //                {

        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                    {
        //                        //if (Corp != null)
        //                        //{
        //                        //    if (Corp != "")
        //                        //    {
        //                        //        var val = db.LookupValue.Find(int.Parse(Corp));
        //                        //        blog1.InstituteType = val;

        //                        //        var type = db.TrainingInstitute.Include(e => e.InstituteType).Where(e => e.Id == data).SingleOrDefault();
        //                        //        IList<TrainingInstitute> typedetails = null;
        //                        //        if (type.InstituteType != null)
        //                        //        {
        //                        //            typedetails = db.TrainingInstitute.Where(x => x.InstituteType.Id == type.InstituteType.Id && x.Id == data).ToList();
        //                        //        }
        //                        //        else
        //                        //        {
        //                        //            typedetails = db.TrainingInstitute.Where(x => x.Id == data).ToList();
        //                        //        }
        //                        //        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //                        //        foreach (var s in typedetails)
        //                        //        {
        //                        //            s.InstituteType = blog1.InstituteType;
        //                        //            db.TrainingInstitute.Attach(s);
        //                        //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                        //            //await db.SaveChangesAsync();
        //                        //            db.SaveChanges();
        //                        //            TempData["RowVersion"] = s.RowVersion;
        //                        //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                        //        }
        //                        //    }
        //                        //    else
        //                        //    {
        //                        //        var WFTypeDetails = db.TrainingInstitute.Include(e => e.InstituteType).Where(x => x.Id == data).ToList();
        //                        //        foreach (var s in WFTypeDetails)
        //                        //        {
        //                        //            s.InstituteType = null;
        //                        //            db.TrainingInstitute.Attach(s);
        //                        //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                        //            //await db.SaveChangesAsync();
        //                        //            db.SaveChanges();
        //                        //            TempData["RowVersion"] = s.RowVersion;
        //                        //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                        //        }
        //                        //    }
        //                        //}
        //                        //else
        //                        //{
        //                        //    var CreditdateypeDetails = db.TrainingInstitute.Include(e => e.InstituteType).Where(x => x.Id == data).ToList();
        //                        //    foreach (var s in CreditdateypeDetails)
        //                        //    {
        //                        //        s.InstituteType = null;
        //                        //        db.TrainingInstitute.Attach(s);
        //                        //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                        //        //await db.SaveChangesAsync();
        //                        //        db.SaveChanges();
        //                        //        TempData["RowVersion"] = s.RowVersion;
        //                        //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                        //    }
        //                        //}

        //                        if (Corp!= "")
        //                        {
        //                            var ids = Convert.ToInt32(Corp);
        //                            var val = db.LookupValue.Find(ids);
        //                            blog1.InstituteType = val;
        //                        }

        //                        var Curr_Lookup = db.TrainingInstitute.Find(data);
        //                        TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
        //                        db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

        //                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                        {

        //                            TrainingInstitute blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.TrainingInstitute.Where(e => e.Id == data).Include(e => e.ContactDetails).Include(e => e.InstituteType).SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            blog1.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog1.DBTrack.CreatedBy == null ? null : blog1.DBTrack.CreatedBy,
        //                                CreatedOn = blog1.DBTrack.CreatedOn == null ? null : blog1.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };

        //                            TrainingInstitute post = new TrainingInstitute()
        //                            {
        //                                Name = L.Name == null ? "" : L.Name.Trim(),
        //                                Code = L.Code == null ? "" : L.Code.Trim(),
        //                                ContactPerson = L.ContactPerson == null ? "" : L.ContactPerson.Trim(),
        //                                InstituteType = blog1.InstituteType,
        //                                Id = data,
        //                                ContactDetails = L.ContactDetails,
        //                                DBTrack = blog1.DBTrack
        //                            };

        //                            db.TrainingInstitute.Attach(post);
        //                            db.Entry(post).State = System.Data.Entity.EntityState.Modified;
        //                            db.Entry(post).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                            // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

        //                            // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);
        //                            using (var context = new DataBaseContext())
        //                            {
        //                                var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, blog1.DBTrack);
        //                                DT_TrainingInstitute DT_Corp = (DT_TrainingInstitute)obj;

        //                                db.Create(DT_Corp);
        //                                db.SaveChanges();
        //                            }

        //                            ts.Complete();
        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

        //                        }
        //                    }
        //                    // }
        //                }
        //                catch (DbUpdateConcurrencyException ex)
        //                {
        //                    var entry = ex.Entries.Single();
        //                    var clientValues = (TrainingInstitute)entry.Entity;
        //                    var databaseEntry = entry.GetDatabaseValues();
        //                    if (databaseEntry == null)
        //                    {
        //                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                    }
        //                    else
        //                    {
        //                        var databaseValues = (TrainingInstitute)databaseEntry.ToObject();
        //                        blog1.RowVersion = databaseValues.RowVersion;

        //                    }
        //                }
        //                Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });

        //            }
        //            return Json(new Object[] { "", "", "", JsonRequestBehavior.AllowGet });
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
        //}

        [HttpPost]
        public async Task<ActionResult> EditSave(TrainingInstitute c, int data, FormCollection form) // Edit submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    string Corp = form["InstituteTypelist"] == "" ? "" : form["InstituteTypelist"];
                    string trainingtype = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    var db_data = db.TrainingInstitute.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();

                    List<ContactDetails> ObjBudsection = new List<ContactDetails>();


                    if (trainingtype != null)
                    {
                        var ids = Utility.StringIdsToListIds(trainingtype);
                        foreach (var ba in ids)
                        {
                            var value = db.ContactDetails.Find(ba);
                            ObjBudsection.Add(value);
                            db_data.ContactDetails = ObjBudsection;
                        }
                    }
                    else
                    {
                        db_data.ContactDetails = null;
                    }

                    if (Corp != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Corp));
                        db_data.InstituteType = val;
                    }

                    db.TrainingInstitute.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    TempData["RowVersion"] = db_data.RowVersion;
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                TrainingInstitute blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.TrainingInstitute.Where(e => e.Id == data).Include(e => e.ContactDetails).SingleOrDefault();
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
                                //  int a = EditS(data, c, c.DBTrack);

                                var CurOBJ = db.TrainingInstitute.Find(data);
                                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    TrainingInstitute ESIOBJ = new TrainingInstitute()
                                    {
                                        Name = c.Name == null ? "" : c.Name.Trim(),
                                        Code = c.Code == null ? "" : c.Code.Trim(),
                                        ContactPerson = c.ContactPerson,
                                        InstituteType = db_data.InstituteType,
                                        Id = data,
                                        ContactDetails = c.ContactDetails,
                                        DBTrack = c.DBTrack
                                    };
                                    db.TrainingInstitute.Attach(ESIOBJ);
                                    db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                    await db.SaveChangesAsync();
                                    using (var context = new DataBaseContext())
                                    {
                                        //To save data in history table 
                                        var Obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, c, "TrainingInstitute", c.DBTrack);
                                        DT_TrainingInstitute DT_Cat = (DT_TrainingInstitute)Obj;
                                        db.DT_TrainingInstitute.Add(DT_Cat);
                                        db.SaveChanges();
                                    }
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = ESIOBJ.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }

                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            TrainingInstitute blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            TrainingInstitute Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.TrainingInstitute.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            if (TempData["RowVersion"] == null)
                            {
                                TempData["RowVersion"] = blog.RowVersion;
                            }

                            TrainingInstitute corp = new TrainingInstitute()
                            {
                                Name = c.Name == null ? "" : c.Name.Trim(),
                                Code = c.Code == null ? "" : c.Code.Trim(),
                                ContactPerson = c.ContactPerson,
                                InstituteType = db_data.InstituteType,
                                Id = data,
                                ContactDetails = c.ContactDetails,
                                RowVersion = (Byte[])TempData["RowVersion"],
                                DBTrack = c.DBTrack
                            };
                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "Corporate", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.TrainingInstitute.Where(e => e.Id == data).Include(e => e.InstituteType)
                                    .Include(e => e.ContactDetails).SingleOrDefault();
                                //DT_Corporate DT_Corp = (DT_Corporate)obj;
                                //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                //db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.TrainingInstitute.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("Record Updated Successfully.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var clientValues = (TrainingInstitute)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (TrainingInstitute)databaseEntry.ToObject();
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


        //public int EditS(string Corp, string ContactDetails, int data, Corporate c, DBTrack dbT)
        //{
        //    if (Corp != null)
        //    {
        //        if (Corp != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(Corp));
        //            c.BusinessType = val;

        //            var type = db.Corporate.Include(e => e.BusinessType).Where(e => e.Id == data).SingleOrDefault();
        //            IList<Corporate> typedetails = null;
        //            if (type.BusinessType != null)
        //            {
        //                typedetails = db.Corporate.Where(x => x.BusinessType.Id == type.BusinessType.Id && x.Id == data).ToList();
        //            }
        //            else
        //            {
        //                typedetails = db.Corporate.Where(x => x.Id == data).ToList();
        //            }
        //            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //            foreach (var s in typedetails)
        //            {
        //                s.BusinessType = c.BusinessType;
        //                db.Corporate.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }
        //        else
        //        {
        //            var BusiTypeDetails = db.Corporate.Include(e => e.BusinessType).Where(x => x.Id == data).ToList();
        //            foreach (var s in BusiTypeDetails)
        //            {
        //                s.BusinessType = null;
        //                db.Corporate.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var BusiTypeDetails = db.Corporate.Include(e => e.BusinessType).Where(x => x.Id == data).ToList();
        //        foreach (var s in BusiTypeDetails)
        //        {
        //            s.BusinessType = null;
        //            db.Corporate.Attach(s);
        //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //            //await db.SaveChangesAsync();
        //            db.SaveChanges();
        //            TempData["RowVersion"] = s.RowVersion;
        //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //        }
        //    }

        //    if (Addrs != null)
        //    {
        //        if (Addrs != "")
        //        {
        //            var val = db.Address.Find(int.Parse(Addrs));
        //            c.Address = val;

        //            var add = db.Corporate.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
        //            IList<Corporate> addressdetails = null;
        //            if (add.Address != null)
        //            {
        //                addressdetails = db.Corporate.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
        //            }
        //            else
        //            {
        //                addressdetails = db.Corporate.Where(x => x.Id == data).ToList();
        //            }
        //            if (addressdetails != null)
        //            {
        //                foreach (var s in addressdetails)
        //                {
        //                    s.Address = c.Address;
        //                    db.Corporate.Attach(s);
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                    // await db.SaveChangesAsync(false);
        //                    db.SaveChanges();
        //                    TempData["RowVersion"] = s.RowVersion;
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var addressdetails = db.Corporate.Include(e => e.Address).Where(x => x.Id == data).ToList();
        //        foreach (var s in addressdetails)
        //        {
        //            s.Address = null;
        //            db.Corporate.Attach(s);
        //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //            //await db.SaveChangesAsync();
        //            db.SaveChanges();
        //            TempData["RowVersion"] = s.RowVersion;
        //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //        }
        //    }

        //    if (ContactDetails != null)
        //    {
        //        if (ContactDetails != "")
        //        {
        //            var val = db.ContactDetails.Find(int.Parse(ContactDetails));
        //            c.ContactDetails = val;

        //            var add = db.Corporate.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
        //            IList<Corporate> contactsdetails = null;
        //            if (add.ContactDetails != null)
        //            {
        //                contactsdetails = db.Corporate.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
        //            }
        //            else
        //            {
        //                contactsdetails = db.Corporate.Where(x => x.Id == data).ToList();
        //            }
        //            foreach (var s in contactsdetails)
        //            {
        //                s.ContactDetails = c.ContactDetails;
        //                db.Corporate.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var contactsdetails = db.Corporate.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
        //        foreach (var s in contactsdetails)
        //        {
        //            s.ContactDetails = null;
        //            db.Corporate.Attach(s);
        //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //            //await db.SaveChangesAsync();
        //            db.SaveChanges();
        //            TempData["RowVersion"] = s.RowVersion;
        //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //        }
        //    }

        //    var CurCorp = db.Corporate.Find(data);
        //    TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //    {
        //        c.DBTrack = dbT;
        //        Corporate corp = new Corporate()
        //        {
        //            Code = c.Code,
        //            Name = c.Name,
        //            Id = data,
        //            DBTrack = c.DBTrack
        //        };


        //        db.Corporate.Attach(corp);
        //        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //        return 1;
        //    }
        //    return 0;
        //}
        [HttpPost]
        //[ValidateAntiForgeryToken]

        public ActionResult Createcontactdetails_partial()
        {
            return View("~/Views/Shared/Core/_Contactdetails.cshtml");
        }
        public ActionResult Editcontactdetails_partial(int data)
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

        public class TrainingInstitute_CD
        {
            public Array ContactDetails_Id { get; set; }
            public Array ContactDetails_FullDetails { get; set; }
        }


        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<TrainingInstitute_CD> return_data = new List<TrainingInstitute_CD>();
                var TrainingInstitute = db.TrainingInstitute
                    .Include(e => e.ContactDetails)
                                    .Include(e => e.InstituteType)
                    .Where(e => e.Id == data).ToList();

                var Q = db.TrainingInstitute
              .Include(e => e.ContactDetails)
              .Include(e => e.InstituteType)

              .Where(e => e.Id == data).Select
              (e => new
              {
                  Id = e.Id,
                  Name = e.Name,
                  Code = e.Code,
                  ContactPerson = e.ContactPerson,
                  InstituteType_ID = e.InstituteType.Id == null ? 0 : e.InstituteType.Id,
                  Action = e.DBTrack.Action
              }).ToList();



                var a = db.TrainingInstitute.Include(e => e.ContactDetails).Where(e => e.Id == data).Select(e => e.ContactDetails).ToList();

                foreach (var ca in a)
                {
                    return_data.Add(
                new TrainingInstitute_CD
                {
                    ContactDetails_Id = ca.Select(e => e.Id.ToString()).ToArray(),
                    ContactDetails_FullDetails = ca.Select(e => e.FullContactDetails).ToArray()
                });
                }
                //var a = db.TrainingInstitute.Include(e => e.ContactDetails).Where(e => e.Id == data).Select(e => e.ContactDetails).SingleOrDefault();
                //var BCDETAILS = (from ca in a
                //                 select new
                //                 {
                //                     Id = ca.Id,
                //                     ContactDetails_Id = ca.Id,
                //                     ContactDetails_FullDetails = ca.FullContactDetails
                //                 }).Distinct();

                TempData["RowVersion"] = db.TrainingInstitute.Find(data).RowVersion;

                var Old_Data = db.DT_TrainingInstitute
                 .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M")
                 .Select
                 (e => new
                 {
                     DT_Id = e.Id,
                     Name = e.Name == null ? "" : e.Name,
                     Code = e.Code == null ? "" : e.Code,
                     ContactPerson = e.ContactPerson == null ? "" : e.ContactPerson,
                     InstituteType_Val = e.InstituteType_Id == 0 ? "" : db.LookupValue
                                  .Where(x => x.Id == e.InstituteType_Id)
                                  .Select(x => x.LookupVal).FirstOrDefault(),
                     Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
                 }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Lkval = db.TrainingInstitute.Find(data);
                TempData["RowVersion"] = Lkval.RowVersion;
                var Auth = Lkval.DBTrack.IsModified;

                return Json(new Object[] { Q, return_data, Old_Data, Auth, JsonRequestBehavior.AllowGet });
            }
            //return this.Json(new Object[] { r, a, JsonRequestBehavior.AllowGet });
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    TrainingInstitute corporates = db.TrainingInstitute
                                                       .Include(e => e.ContactDetails)
                                                       .Include(e => e.InstituteType).Where(e => e.Id == data).SingleOrDefault();


                    //ContactDetails conDet = corporates.ContactDetails;
                    LookupValue val = corporates.InstituteType;
                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
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
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corporates.DBTrack);
                            DT_TrainingInstitute DT_Corp = (DT_TrainingInstitute)rtn_Obj;
                            // DT_Corp.Address_Id = corporates.Address == null ? 0 : corporates.Address.Id;
                            DT_Corp.InstituteType_Id = corporates.InstituteType == null ? 0 : corporates.InstituteType.Id;
                            // DT_Corp.ContactDetails_Id = corporates.ContactDetails == null ? 0 : corporates.ContactDetails.Id;
                            db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();

                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                        }
                    }
                    else
                    {
                        //var selectedRegions = corporates.Regions;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(corporates.Regions.Select(e => e.Id));
                            //    if (corpRegion.Count > 0)
                            //    {
                            //        return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //    }
                            //}

                            try
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
                                //var v = db.FacultyExternal.Where(a => a.TrainingInstitue.Id == corporates.Id).ToList();
                                //if (v.Count != 0)
                                //{
                                //    Msg.Add("child record exists.  ");
                                //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //}


                                db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, dbT);
                                DT_TrainingInstitute DT_Corp = (DT_TrainingInstitute)rtn_Obj;
                                //    DT_Corp.Address_Id = add == null ? 0 : add.Id;
                                DT_Corp.InstituteType_Id = val == null ? 0 : val.Id;
                                //  DT_Corp.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
                                db.Create(DT_Corp);

                                await db.SaveChangesAsync();


                                //using (var context = new DataBaseContext())
                                //{
                                //    corporates.Address = add;
                                //    corporates.ContactDetails = conDet;
                                //    corporates.InstituteType = val;
                                //    DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", dbT);
                                //}
                                ts.Complete();

                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }
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



        public ActionResult GetLookup(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TrainingInstitute.ToList();


                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.TrainingInstitute.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                //var list1 = db.FacultySpecialization.ToList();
                //var list2 = fall.Except(list1);
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }



    }
}