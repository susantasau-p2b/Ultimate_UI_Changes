///
/// Created by Tanushri
///


using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using P2BUltimate.App_Start;
using System.Net;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Transactions;
using P2b.Global;
using System.Text;
using System.Threading.Tasks;
using P2BUltimate.Security;


namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class PayScaleController : Controller
    {
       // private DataBaseContext db = new DataBaseContext();
        //
        // GET: /Lookup/

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/PayScale/Index.cshtml");
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Core/_Location.cshtml");

        }



        //private MultiSelectList GetLocationValues(List<int> selectedValues)
        //{
        //    List<Location> lkval = new List<Location>();
        //    lkval = db.Location.ToList();
        //    return new MultiSelectList(lkval, "Id", "FullDetails", selectedValues);
        //}


        public ActionResult GetPayscaleAreaDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Location.Include(e => e.LocationObj).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Location.Include(e => e.LocationObj).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var list1 = db.PayScale.Include(e => e.PayScaleArea.Select(t => t.LocationObj)).SelectMany(e => e.PayScaleArea);
                var list2 = fall.Except(list1);
                var P = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(P, JsonRequestBehavior.AllowGet);
            }
        }

  

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(PayScale NOBJ, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string PayScaleType = form["PayScaleTypeList_DDL"] == "0" ? "" : form["PayScaleTypeList_DDL"];
                    string RoundingList_DDL = form["RoundingList_DDL"] == "0" ? "" : form["RoundingList_DDL"];
                    var ActualIndexAppl = form["ActualIndexAppl"];
                    NOBJ.ActualIndexAppl = Convert.ToBoolean(ActualIndexAppl);
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    var Company = new Company();
                    if (company_Id != null)
                    {
                        Company = db.Company.Where(e => e.Id == company_Id).SingleOrDefault();

                    }

                    if (RoundingList_DDL != null && RoundingList_DDL != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "422").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(RoundingList_DDL)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(RoundingList_DDL));
                        NOBJ.Rounding = val;

                    }
                    if (PayScaleType != null)
                    {
                        if (PayScaleType != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "423").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(RoundingList_DDL)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(PayScaleType));
                            NOBJ.PayScaleType = val;
                        }
                    }

                    if (PayScaleType == null && PayScaleType == "")
                    {
                        Msg.Add("  Select PayScaleType  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new Object[] { "", "", "Select PayScaleType", JsonRequestBehavior.AllowGet });
                    }

                    //string Values = form["PayScaleAreaList"];
                    NOBJ.PayScaleArea = null;
                    List<Location> OBJ = new List<Location>();
                    string Values = form["PayScaleAreaList"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.Location.Include(e => e.LocationObj).Where(e => e.Id == ca).SingleOrDefault();
                            OBJ.Add(OBJ_val);
                            NOBJ.PayScaleArea = OBJ;
                        }
                    }

                    //if (Values != null)
                    //{
                    //    List<int> IDs = Values.Split(',').Select(s => int.Parse(s)).ToList();
                    //    ViewBag.LookupVal = GetLocationValues(IDs);
                    //}
                    //else
                    //{
                    //    ViewBag.LookupVal = GetLocationValues(null);
                    //}

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            NOBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            PayScale PayScale = new PayScale()
                            {
                                BasicScaleAppl = NOBJ.BasicScaleAppl,
                                CPIAppl = NOBJ.CPIAppl,
                                PayScaleType = NOBJ.PayScaleType,
                                PayScaleArea = NOBJ.PayScaleArea,
                                ActualIndexAppl = NOBJ.ActualIndexAppl,
                                MultiplyingFactor = NOBJ.MultiplyingFactor,
                                Rounding = NOBJ.Rounding,
                                DBTrack = NOBJ.DBTrack
                            };
                            try
                            {

                                db.PayScale.Add(PayScale);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, NOBJ.DBTrack);
                                DT_PayScale DT_OBJ = (DT_PayScale)rtn_Obj;
                                DT_OBJ.PayScaleType_Id = NOBJ.PayScaleType == null ? 0 : NOBJ.PayScaleType.Id;
                                db.Create(DT_OBJ);
                                db.SaveChanges();
                                if (Company != null)
                                {
                                    var payscale_list = new List<PayScale>();
                                    payscale_list.Add(PayScale);
                                    Company.PayScale = payscale_list;
                                    db.Company.Attach(Company);
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Detached;

                                }

                                ts.Complete();
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = PayScale.Id, Val = PayScale.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { PayScale.Id, PayScale.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = NOBJ.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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

        public ActionResult GetContactDetLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ContactDetails.ToList();
                IEnumerable<ContactDetails> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.ContactDetails.ToList().Where(d => d.FullContactDetails.Contains(data));

                }
                else
                {
                    var list1 = db.Corporate.ToList().Select(e => e.ContactDetails);
                    //var list2 = fall.Except(list1);
                    //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                    var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.CorporateCode, c.CorporateName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullContactDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }


        public ActionResult GetLocationDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Location.Include(e=>e.LocationObj).ToList();
                IEnumerable<Location> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Location.Include(e => e.LocationObj).ToList().Where(d => d.FullDetails.Contains(data));

                }
                else
                {
                    //var list1 = db.PayScale.ToList().Select(e => e.PayScaleArea);
                    //var list1 = db.PayScale.ToList();
                    //var list2 = fall.Except(list1);
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
                            PayScale PayScale = db.PayScale.Include(e => e.PayScaleArea.Select(r => r.LocationObj)).Include(e => e.PayScaleType)
                              .FirstOrDefault(e => e.Id == auth_id);
                            PayScale.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = PayScale.DBTrack.ModifiedBy != null ? PayScale.DBTrack.ModifiedBy : null,
                                CreatedBy = PayScale.DBTrack.CreatedBy != null ? PayScale.DBTrack.CreatedBy : null,
                                CreatedOn = PayScale.DBTrack.CreatedOn != null ? PayScale.DBTrack.CreatedOn : null,
                                IsModified = PayScale.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };
                            db.PayScale.Attach(PayScale);
                            db.Entry(PayScale).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(PayScale).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, PayScale.DBTrack);
                            DT_PayScale DT_OBJ = (DT_PayScale)rtn_Obj;
                            DT_OBJ.PayScaleType_Id = PayScale.PayScaleType == null ? 0 : PayScale.PayScaleType.Id;

                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = PayScale.Id, Val = PayScale.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { PayScale.Id, PayScale.FullDetails, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        PayScale Old_OBJ = db.PayScale.Include(e => e.PayScaleType)
                                                          .Include(e => e.PayScaleArea.Select(r => r.LocationObj))
                                                          .Where(e => e.Id == auth_id).SingleOrDefault();

                        // DT_PayScale Curr_OBJ = db.DT_PayScale.Include(e => e.PayScaleArea)  
                        DT_PayScale Curr_OBJ = db.DT_PayScale
                                              .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                              .OrderByDescending(e => e.Id)
                                              .FirstOrDefault();

                        PayScale PayScale = new PayScale();

                        string Arealst = Curr_OBJ.PayScaleArea_Id == null ? null : Curr_OBJ.PayScaleArea_Id.ToString();
                        string Typelst = Curr_OBJ.PayScaleType_Id == null ? null : Curr_OBJ.PayScaleType_Id.ToString();


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    // db.Configuration.AutoDetectChangesEnabled = false;
                                    PayScale.DBTrack = new DBTrack
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

                                    //int a = EditS(Arealst, Typelst, "", auth_id, PayScale, PayScale.DBTrack);

                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = PayScale.Id, Val = PayScale.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { PayScale.Id, PayScale.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (PayScale)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (PayScale)databaseEntry.ToObject();
                                    PayScale.RowVersion = databaseValues.RowVersion;
                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //PayScale corp = db.PayScale.Find(auth_id);
                            PayScale PayScale = db.PayScale.AsNoTracking().Include(e => e.PayScaleArea.Select(r => r.LocationObj))
                                                                        .FirstOrDefault(e => e.Id == auth_id);
                            var selectedValues = PayScale.PayScaleArea;
                            PayScale.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = PayScale.DBTrack.ModifiedBy != null ? PayScale.DBTrack.ModifiedBy : null,
                                CreatedBy = PayScale.DBTrack.CreatedBy != null ? PayScale.DBTrack.CreatedBy : null,
                                CreatedOn = PayScale.DBTrack.CreatedOn != null ? PayScale.DBTrack.CreatedOn : null,
                                IsModified = PayScale.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };
                            db.PayScale.Attach(PayScale);
                            db.Entry(PayScale).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, PayScale.DBTrack);
                            DT_PayScale DT_OBJ = (DT_PayScale)rtn_Obj;
                            DT_OBJ.PayScaleType_Id = PayScale.PayScaleType == null ? 0 : PayScale.PayScaleType.Id;

                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();

                            db.Entry(PayScale).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record Authorised", JsonRequestBehavior.AllowGet });
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



        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Location.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public async Task<ActionResult> EditSave(PayScale P, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string TypeOBJ = form["PayScaleTypeList_DDL"] == "0" ? "" : form["PayScaleTypeList_DDL"];
                    string RoundingList_DDL = form["RoundingList_DDL"] == "0" ? "" : form["RoundingList_DDL"];
                    string AreaOBJ = form["PayScaleAreaList"] == "0" ? "" : form["PayScaleAreaList"];
                    var ActualIndexAppl = form["ActualIndexAppl"];
                    P.ActualIndexAppl = Convert.ToBoolean(ActualIndexAppl);
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    P.PayScaleType_Id = TypeOBJ != null && TypeOBJ != "" ? int.Parse(TypeOBJ) : 0;
                    P.Rounding_Id = RoundingList_DDL != null && RoundingList_DDL != "" ? int.Parse(RoundingList_DDL) : 0;

                    var db_data = db.PayScale.Include(e => e.PayScaleArea.Select(r => r.LocationObj)).Include(e => e.PayScaleType).Where(e => e.Id == data).SingleOrDefault();
                    List<Location> LocationDetails = new List<Location>();
                    string Values = form["PayScaleAreaList"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var LocationDetails_val = db.Location.Include(e => e.LocationObj).Where(e => e.Id == ca).SingleOrDefault();
                            LocationDetails.Add(LocationDetails_val);
                            db_data.PayScaleArea = LocationDetails;
                        }
                    }
                    else
                    {
                        db_data.PayScaleArea = null;
                    }
                    db.PayScale.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified; 
                    db.SaveChanges();
                    TempData["RowVersion"] = db_data.RowVersion; 


                    if (Auth == false)
                    {
                         
                    if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    PayScale blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;


                                    blog = db.PayScale.Where(e => e.Id == data).Include(e => e.PayScaleType)
                                                            .Include(e => e.PayScaleArea.Select(r => r.LocationObj)).SingleOrDefault();
                                    originalBlogValues = db.Entry(blog).OriginalValues;


                                    P.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    var CurCorp = db.PayScale.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    { 
                                        CurCorp.BasicScaleAppl = P.BasicScaleAppl;
                                        CurCorp.CPIAppl = P.CPIAppl;  
                                        CurCorp.ActualIndexAppl = P.ActualIndexAppl;
                                        CurCorp.MultiplyingFactor = P.MultiplyingFactor;
                                        CurCorp.Id = data;
                                        CurCorp.DBTrack = P.DBTrack;
                                        CurCorp.PayScaleType_Id = P.PayScaleType_Id != 0 ? P.PayScaleType_Id : null;
                                        CurCorp.Rounding_Id = P.Rounding_Id != 0 ? P.Rounding_Id : null;


                                        db.Entry(CurCorp).State = System.Data.Entity.EntityState.Modified;
                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, P.DBTrack);
                                        DT_PayScale DT_Corp = (DT_PayScale)obj;
                                        DT_Corp.PayScaleArea_Id = blog.PayScaleType == null ? 0 : blog.PayScaleType.Id;
                                        //DT_Corp.PayScaleType_Id = blog.PayScaleArea == null ? 0 : blog.PayScaleArea.Id;
                                        db.Create(DT_Corp);
                                        db.SaveChanges();


                                        ts.Complete();
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = CurCorp.Id, Val = CurCorp.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { c.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                    }
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (PromoActivity)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (PromoActivity)databaseEntry.ToObject();
                                    P.RowVersion = databaseValues.RowVersion;

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

                            PayScale blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            PayScale Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.PayScale.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            P.DBTrack = new DBTrack
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

                            PayScale payscale = new PayScale()
                            {
                                BasicScaleAppl = P.BasicScaleAppl,
                                CPIAppl = P.CPIAppl,
                                PayScaleArea = P.PayScaleArea,
                                PayScaleType = P.PayScaleType,
                                ActualIndexAppl = P.ActualIndexAppl,
                                MultiplyingFactor = P.MultiplyingFactor,
                                Rounding = P.Rounding,
                                Id = data,
                                DBTrack = P.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, payscale, "Payscale", P.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.PayScale.Where(e => e.Id == data).Include(e => e.PayScaleType).SingleOrDefault();
                                DT_PayScale DT_Corp = (DT_PayScale)obj;

                                DT_Corp.PayScaleType_Id = DBTrackFile.ValCompare(Old_Corp.PayScaleType, P.PayScaleType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;

                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = P.DBTrack;
                            db.PayScale.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = P.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { blog.Id, P.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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
        public class PayScale_PSA
        {
            public Array PayScale_id { get; set; }
            public Array PayScale_FullDetails { get; set; }
        }




        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<PayScale_PSA> return_data = new List<PayScale_PSA>();
                var Q = db.PayScale
                  .Include(e => e.PayScaleArea.Select(r => r.LocationObj))
                  .Include(e => e.PayScaleType)
                  .Where(e => e.Id == data).Select
                  (e => new
                  {
                      Id = e.Id,
                      BasicScaleAppl = e.BasicScaleAppl,
                      CPIAppl = e.CPIAppl,
                      ActualIndexAppl = e.ActualIndexAppl,
                      MultiplyingFactor = e.MultiplyingFactor,
                      Roundinglist_Id = e.Rounding.Id == null ? 0 : e.Rounding.Id,
                      PayScaleType_Id = e.PayScaleType.Id == null ? 0 : e.PayScaleType.Id,
                  }).ToList();
                //var add_data = db.PayScale.Include(e => e.PayScaleArea).Where(e => e.Id == data).Select(e => e.PayScaleArea).SingleOrDefault();
                ////var a = db.PayScale.Include(e => e.BasicScaleDetails).Where(e => e.Id == data).Select(e => e.BasicScaleDetails).SingleOrDefault();
                //var BCDETAILS = (from ca in add_data
                //                 select new
                //                 {
                //                     Id = ca.Id,
                //                     PayScale_Id = ca.Id,
                //                     PayScale_FullDetails = ca.FullDetails
                //                 }).Distinct();
                //TempData["RowVersion"] = db.PayScale.Find(data).RowVersion;

                var a = db.PayScale.Include(e => e.PayScaleType)
                    .Include(e => e.PayScaleArea.Select(r => r.LocationObj))
                    .Where(e => e.Id == data).Select(e => e.PayScaleArea.Select(t => t.LocationObj))
                    .ToList();
                if (a != null && a.Count > 0)
                {
                    foreach (var ca in a)
                    {

                        return_data.Add(
                    new PayScale_PSA
                    {
                        PayScale_id = ca.Select(e => e.Id.ToString()).ToArray(),
                        PayScale_FullDetails = ca.Select(e => e.LocDesc).ToArray()
                    });
                    }
                }
                var Old_Data = db.DT_PayScale
                    //.Include(e => e.PayScaleArea)
                    // .Include(e => e.PayScaleType) 
                 .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M")
                 .Select
                 (e => new
                 {
                     DT_Id = e.Id,
                     BasicScaleAppl = e.BasicScaleAppl,
                     PayScaleADetails_Val = e.PayScaleArea_Id == 0 ? "" : db.Location.Where(x => x.Id == e.PayScaleArea_Id).Select(x => x.FullDetails).FirstOrDefault(),
                     ////BasicScaleDetails_Val = e.BasicScaleDetails.Id == null ? "" : e.BasicScaleDetails.FullDetails,  
                     PayScaleType_Val = e.PayScaleType_Id == 0 ? "" : db.LookupValue
                                   .Where(x => x.Id == e.PayScaleType_Id)
                                   .Select(x => x.LookupVal).FirstOrDefault(),
                 }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.PayScale.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;


                return Json(new Object[] { Q, return_data, Old_Data, Auth, JsonRequestBehavior.AllowGet });
                //return this.Json(new Object[] { Q, BCDETAILS, JsonRequestBehavior.AllowGet });
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
                    PayScale PayScale = db.PayScale.Include(e => e.PayScaleArea).Include(e => e.PayScaleType)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    var add = PayScale.PayScaleArea;

                    //ocation payA = PayScale.PayScaleArea;
                    //PayScale PayScale = db.PayScale.Where(e => e.Id == data).SingleOrDefault();
                    if (PayScale.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, PayScale.DBTrack, PayScale, null, "PayScale");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = PayScale.DBTrack.CreatedBy != null ? PayScale.DBTrack.CreatedBy : null,
                                CreatedOn = PayScale.DBTrack.CreatedOn != null ? PayScale.DBTrack.CreatedOn : null,
                                IsModified = PayScale.DBTrack.IsModified == true ? true : false
                            };
                            PayScale.DBTrack = dbT;
                            db.Entry(PayScale).State = System.Data.Entity.EntityState.Modified;
                            DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, PayScale.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //      DBTrackFile.DBTrackSave("Core/P2b.Global", "D", PayScale, null, "PayScale", PayScale.DBTrack);
                            //}
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                        }
                    }
                    else
                    {
                        var PayA = PayScale.PayScaleArea;
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            try
                            {
                                if (PayA != null)
                                {
                                    var corpRegion = new HashSet<int>(PayScale.PayScaleArea.Select(e => e.Id));
                                    if (corpRegion.Count > 0)
                                    {
                                        Msg.Add(" Child record exists.Cannot remove it..  ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = PayScale.DBTrack.CreatedBy != null ? PayScale.DBTrack.CreatedBy : null,
                                    CreatedOn = PayScale.DBTrack.CreatedOn != null ? PayScale.DBTrack.CreatedOn : null,
                                    IsModified = PayScale.DBTrack.IsModified == true ? false : false,
                                    // AuthorizedBy = SessionManager.UserName,
                                    // AuthorizedOn = DateTime.Now
                                };
                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };
                                db.Entry(PayScale).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                await db.SaveChangesAsync();
                                //using (var context = new DataBaseContext())
                                //{
                                //    PayScale.PayScaleArea = add;
                                //    //  DBTrackFile.DBTrackSave("Core/P2b.Global", "D", PayScale, null, "PayScale", dbT);
                                //}
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


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
                return new EmptyResult();
            }
        }


        public class P2BGridData
        {
            public int Id { get; set; }
            public bool BasicScaleAppl { get; set; }
            public bool CPIAppl { get; set; }
            public string PayscaleType { get; set; }
        }



        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> PayscaleList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;


                    //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                    //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();

                    var BindCompList = db.Company.Include(e => e.PayScale).Include(e => e.PayScale.Select(t => t.PayScaleType)).ToList();

                    foreach (var z in BindCompList)
                    {
                        if (z.PayScale != null)
                        {

                            foreach (var P in z.PayScale)
                            {
                                //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
                                view = new P2BGridData()
                                {
                                    Id = P.Id,
                                    BasicScaleAppl = P.BasicScaleAppl,
                                    CPIAppl = P.CPIAppl,
                                    PayscaleType = P.PayScaleType != null ? P.PayScaleType.LookupVal.ToString() : "",
                                };
                                model.Add(view);

                            }
                        }

                    }

                    PayscaleList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = PayscaleList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.BasicScaleAppl.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.CPIAppl.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.PayscaleType.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.BasicScaleAppl, a.CPIAppl, a.PayscaleType, a.Id }).ToList();

                            //if (gp.searchField == "Id")
                            //    jsonData = IE.Select(a => new { a.BasicScaleAppl, a.CPIAppl, a.Id }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                            //if (gp.searchField == "Name")
                            //    jsonData = IE.Select(a => new { a.BasicScaleAppl, a.CPIAppl, a.Id }).Where((e => (e.BasicScaleAppl.ToString().Contains(gp.searchString)))).ToList();
                            //if (gp.searchField == "Default")
                            //    jsonData = IE.Select(a => new { a.BasicScaleAppl, a.CPIAppl, a.Id }).Where((e => (e.CPIAppl.ToString().Contains(gp.searchString)))).ToList();




                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;

                            

                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.BasicScaleAppl), Convert.ToString(a.CPIAppl), a.PayscaleType != null ? Convert.ToString(a.PayscaleType) : "", a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = PayscaleList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "BasicScaleAppl" ? c.BasicScaleAppl.ToString() :
                                             gp.sidx == "CPIAppl" ? c.CPIAppl.ToString() :
                                             gp.sidx == "PayscaleType" ? c.PayscaleType.ToString() : ""

                                            );
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.BasicScaleAppl), Convert.ToString(a.CPIAppl), a.PayscaleType != null ? Convert.ToString(a.PayscaleType) : "", a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.BasicScaleAppl), Convert.ToString(a.CPIAppl), a.PayscaleType != null ? Convert.ToString(a.PayscaleType) : "", a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.BasicScaleAppl), Convert.ToString(a.CPIAppl), a.PayscaleType != null ? Convert.ToString(a.PayscaleType) : "", a.Id }).ToList();
                        }
                        totalRecords = PayscaleList.Count();
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


            //public ActionResult P2BGrid(P2BGrid_Parameters gp)
            //{
            //    try
            //    {
            //        DataBaseContext db = new DataBaseContext();
            //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
            //        int pageSize = gp.rows;
            //        int totalPages = 0;
            //        int totalRecords = 0;
            //        int ParentId = 2;
            //        var jsonData = (Object)null;
            //        var LKVal = db.PayScale.ToList();

            //        if (gp.IsAutho == true)
            //        {
            //            LKVal = db.PayScale.Include(e => e.PayScaleArea).Include(e => e.PayScaleType).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
            //        }
            //        else
            //        {
            //            LKVal = db.PayScale.Include(e => e.PayScaleArea).Include(e => e.PayScaleType).AsNoTracking().ToList();
            //        }

            //        IEnumerable<PayScale> IE;
            //        if (!string.IsNullOrEmpty(gp.searchString))
            //        {
            //            IE = LKVal;
            //            if (gp.searchOper.Equals("eq"))
            //            {
            //                jsonData = IE.Select(a => new { a.Id, a.BasicScaleAppl, a.CPIAppl }).Where((e => (e.Id.ToString() == gp.searchString) || (e.BasicScaleAppl.ToString() == gp.searchString) || (e.CPIAppl.ToString() == gp.searchString.ToLower())));
            //            }
            //            if (pageIndex > 1)
            //            {
            //                int h = pageIndex * pageSize;
            //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.BasicScaleAppl, a.CPIAppl }).ToList();
            //            }
            //            totalRecords = IE.Count();
            //        }
            //        else
            //        {


            //            IE = LKVal;
            //            Func<PayScale, dynamic> orderfuc;
            //            if (gp.sidx == "Id")
            //            {
            //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
            //            }
            //            else
            //            {
            //                orderfuc = (c => gp.sidx == "Id" ? c.Id.ToString() :
            //                                gp.sidx == "BasicScaleAppl" ? c.BasicScaleAppl.ToString() :
            //                                gp.sidx == "CPIAppl" ? c.CPIAppl.ToString() : "");
            //            }
            //            if (gp.sord == "asc")
            //            {
            //                IE = IE.OrderBy(orderfuc);
            //                jsonData = IE.Select(a => new Object[] { a.Id, a.BasicScaleAppl, a.CPIAppl }).ToList();
            //            }
            //            else if (gp.sord == "desc")
            //            {
            //                IE = IE.OrderByDescending(orderfuc);
            //                jsonData = IE.Select(a => new Object[] { a.Id, a.BasicScaleAppl, a.CPIAppl }).ToList();
            //            }
            //            if (pageIndex > 1)
            //            {
            //                int h = pageIndex * pageSize;
            //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.BasicScaleAppl, a.CPIAppl }).ToList();
            //            }
            //            totalRecords = LKVal.Count();
            //        }
            //        if (totalRecords > 0)
            //        {
            //            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
            //        }
            //        if (gp.page > totalPages)
            //        {
            //            gp.page = totalPages;
            //        }
            //        var JsonData = new
            //        {
            //            page = gp.page,
            //            rows = jsonData,
            //            records = totalRecords,
            //            total = totalPages,
            //            p2bparam = ParentId
            //        };
            //        return Json(JsonData, JsonRequestBehavior.AllowGet);
            //    }
            //    catch (Exception ex)
            //    {
            //        throw ex;
            //    }
            //}

        }

    }
}
