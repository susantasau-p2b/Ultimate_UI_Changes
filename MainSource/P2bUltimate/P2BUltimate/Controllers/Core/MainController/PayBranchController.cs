///
/// Created by Kapil
///

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

namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class PayBranchController : Controller
    {
        //
        // GET: /PayBranch/
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/PayBranch/Index.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(Branch c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Category = form["Categorylist"] == "0" ? "" : form["Categorylist"];
                    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];

                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.Employee.Find(int.Parse(Category));
                            c.Incharge = val;
                        }
                    }

                    if (Addrs != null)
                    {
                        if (Addrs != "")
                        {
                            int AddId = Convert.ToInt32(Addrs);
                            var val = db.Address//.Include(e => e.Area)
                                //.Include(e => e.City)
                                //.Include(e => e.Country)
                                //.Include(e => e.District)
                                //.Include(e => e.State)
                                //.Include(e => e.StateRegion)
                                //.Include(e => e.Taluka)
                                                .Where(e => e.Id == AddId).SingleOrDefault();
                            c.Address = val;
                        }
                    }

                    if (ContactDetails != null)
                    {
                        if (ContactDetails != "")
                        {
                            int ContId = Convert.ToInt32(ContactDetails);
                            var val = db.ContactDetails.Include(e => e.ContactNumbers)
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            c.ContactDetails = val;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.Branch.Any(o => o.Code == c.Code))
                            {
                                Msg.Add("  Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            Branch Branch = new Branch()
                            {
                                Code = c.Code == null ? "" : c.Code.Trim(),
                                Name = c.Name == null ? "" : c.Name.Trim(),
                                Incharge = c.Incharge,
                                Address = c.Address,
                                IFSCCode = c.IFSCCode,
                                MICRCode = c.MICRCode,
                                ContactDetails = c.ContactDetails,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.Branch.Add(Branch);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                DT_Branch DT_Corp = (DT_Branch)rtn_Obj;
                                DT_Corp.Address_Id = c.Address == null ? 0 : c.Address.Id;
                                DT_Corp.Incharge_Id = c.Incharge == null ? 0 : c.Incharge.Id;
                                DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                                db.Create(DT_Corp);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[]{"","", "Data Saved Successfully.",JsonRequestBehavior.AllowGet });           
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
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
                        // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet }); 
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

        public ActionResult Createcontactdetails_partial()
        {
            return View("~/Views/Shared/Core/_Contactdetails.cshtml");
        }



        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.Branch
                    .Include(e => e.ContactDetails)
                    .Include(e => e.Address)
                    .Include(e => e.Incharge)
                    .Include(e => e.ContactDetails)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Code = e.Code,
                        Name = e.Name,
                        ifsccode = e.IFSCCode == null ? "" : e.IFSCCode,
                        micrcode = e.MICRCode == null ? "" : e.MICRCode,
                        Incharge_Id = e.Incharge.Id == null ? 0 : e.Incharge.Id,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.Branch
                  .Include(e => e.ContactDetails)
                    .Include(e => e.Address)
                    .Include(e => e.Incharge)
                    .Include(e => e.ContactDetails)
                    .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        Address_FullAddress = e.Address.FullAddress == null ? "" : e.Address.FullAddress,
                        Add_Id = e.Address.Id == null ? "" : e.Address.Id.ToString(),
                        Cont_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
                        FullContactDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails
                    }).ToList();


                var W = db.DT_Branch
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Code = e.Code == null ? "" : e.Code,
                         Name = e.Name == null ? "" : e.Name,
                         //Incharge_Val = e.Incharge_Id == 0 ? "" : db.Employee
                         //           .Where(x => x.Id == e.Incharge_Id)
                         //           .Select(x => x.EmpN).FirstOrDefault(),

                         Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
                         Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.Branch.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(Branch c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Corp = form["Categorylist"] == "0" ? "" : form["Categorylist"];
                    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    c.Incharge_Id = Corp != null && Corp != "" ? int.Parse(Corp) : 0;
                    c.Address_Id = Addrs != null && Addrs != "" ? int.Parse(Addrs) : 0;
                    c.ContactDetails_Id = ContactDetails != null && ContactDetails != "" ? int.Parse(ContactDetails) : 0;
                    

                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    Branch blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;
 
                                        blog = db.Branch.Where(e => e.Id == data).Include(e => e.Incharge)
                                                                .Include(e => e.Address)
                                                                .Include(e => e.ContactDetails).SingleOrDefault();
                                        originalBlogValues = db.Entry(blog).OriginalValues;
                                   
                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                   
                                    var CurCorp = db.Branch.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        CurCorp.Code = c.Code;
                                        CurCorp.Name = c.Name;
                                        CurCorp.Id = data;
                                        CurCorp.IFSCCode = c.IFSCCode;
                                        CurCorp.MICRCode = c.MICRCode;
                                        CurCorp.DBTrack = c.DBTrack;
                                        CurCorp.Incharge_Id = c.Incharge_Id != 0 ? c.Incharge_Id : null;
                                        CurCorp.Address_Id = c.Address_Id != 0 ? c.Address_Id : null;
                                        CurCorp.ContactDetails_Id = c.ContactDetails_Id != 0 ? c.ContactDetails_Id : null;

                                        db.Entry(CurCorp).State = System.Data.Entity.EntityState.Modified;
                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_Branch DT_Corp = (DT_Branch)obj;
                                        DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
                                        DT_Corp.Incharge_Id = blog.Incharge == null ? 0 : blog.Incharge.Id;
                                        DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
                                        db.Create(DT_Corp);
                                        db.SaveChanges();

                                    }
                                     
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { c.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Branch)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (Branch)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            Branch blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            Branch Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.Branch.Where(e => e.Id == data).SingleOrDefault();
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
                            Branch corp = new Branch()
                            {
                                Code = c.Code,
                                Name = c.Name,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Branch", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.Branch.Where(e => e.Id == data).Include(e => e.Incharge)
                                    .Include(e => e.Address).Include(e => e.ContactDetails).SingleOrDefault();
                                DT_Branch DT_Corp = (DT_Branch)obj;
                                DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                DT_Corp.Incharge_Id = DBTrackFile.ValCompare(Old_Corp.Incharge, c.Incharge); //Old_Corp.Incharge == c.Incharge ? 0 : Old_Corp.Incharge == null && c.Incharge != null ? c.Incharge.Id : Old_Corp.Incharge.Id;
                                DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.Branch.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { blog.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
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
                            //Branch corp = db.Branch.Find(auth_id);
                            //Branch corp = db.Branch.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                            Branch corp = db.Branch.Include(e => e.Address)
                                .Include(e => e.ContactDetails)
                                .Include(e => e.Incharge).FirstOrDefault(e => e.Id == auth_id);

                            corp.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                                CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                                CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                                IsModified = corp.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.Branch.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_Branch DT_Corp = (DT_Branch)rtn_Obj;
                            DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            DT_Corp.Incharge_Id = corp.Incharge == null ? 0 : corp.Incharge.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { corp.Id,  "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        Branch Old_Corp = db.Branch.Include(e => e.Incharge)
                                                          .Include(e => e.Address)
                                                          .Include(e => e.ContactDetails).Where(e => e.Id == auth_id).SingleOrDefault();

                        //var W = db.DT_Branch
                        //.Include(e => e.ContactDetails)
                        //.Include(e => e.Address)
                        //.Include(e => e.Incharge)
                        //.Include(e => e.ContactDetails)
                        //.Where(e => e.Orig_Id == auth_id && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                        //(e => new
                        //{
                        //    DT_Id = e.Id,
                        //    Code = e.Code == null ? "" : e.Code,
                        //    Name = e.Name == null ? "" : e.Name,
                        //    Incharge_Val = e.Incharge.Id == null ? "" : e.Incharge.LookupVal,
                        //    Address_Val = e.Address.Id == null ? "" : e.Address.FullAddress,
                        //    Contact_Val = e.ContactDetails.Id == null ? "" : e.ContactDetails.FullContactDetails,
                        //}).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                        DT_Branch Curr_Corp = db.DT_Branch
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Corp != null)
                        {
                            Branch corp = new Branch();

                            string Corp = Curr_Corp.Incharge_Id == null ? null : Curr_Corp.Incharge_Id.ToString();
                            string Addrs = Curr_Corp.Address_Id == null ? null : Curr_Corp.Address_Id.ToString();
                            string ContactDetails = Curr_Corp.ContactDetails_Id == null ? null : Curr_Corp.ContactDetails_Id.ToString();
                            corp.Name = Curr_Corp.Name == null ? Old_Corp.Name : Curr_Corp.Name;
                            corp.Code = Curr_Corp.Code == null ? Old_Corp.Code : Curr_Corp.Code;
                            //      corp.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        corp.DBTrack = new DBTrack
                                            {
                                                CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                                                CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                                                Action = "M",
                                                ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
                                                ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
                                                AuthorizedBy = SessionManager.UserName,
                                                AuthorizedOn = DateTime.Now,
                                                IsModified = false
                                            };

                                        int a = EditS(Corp, Addrs, ContactDetails, auth_id, corp, corp.DBTrack);
                                        //var CurCorp = db.Branch.Find(auth_id);
                                        //TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                        //db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                        //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                        //{
                                        //    c.DBTrack = new DBTrack
                                        //    {
                                        //        CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                                        //        CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                                        //        Action = "M",
                                        //        ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
                                        //        ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
                                        //        AuthorizedBy = SessionManager.UserName,
                                        //        AuthorizedOn = DateTime.Now,
                                        //        IsModified = false
                                        //    };
                                        //    Branch corp = new Branch()
                                        //    {
                                        //        Code = c.Code,
                                        //        Name = c.Name,
                                        //        Id = Convert.ToInt32(auth_id),
                                        //        DBTrack = c.DBTrack
                                        //    };


                                        //    db.Branch.Attach(corp);
                                        //    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;

                                        //    // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                        //    //db.SaveChanges();
                                        //    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        //    await db.SaveChangesAsync();
                                        //    //DisplayTrackedEntities(db.ChangeTracker);
                                        //    db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                                        //    ts.Complete();
                                        //    return Json(new Object[] { corp.Id, corp.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                        //}

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (Branch)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (Branch)databaseEntry.ToObject();
                                        corp.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                        {
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                        // return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Branch corp = db.Branch.Find(auth_id);
                            Branch corp = db.Branch.AsNoTracking().Include(e => e.Address)
                                                                        .Include(e => e.Incharge)
                                                                        .Include(e => e.ContactDetails).FirstOrDefault(e => e.Id == auth_id);

                            Address add = corp.Address;
                            ContactDetails conDet = corp.ContactDetails;
                            Employee val = corp.Incharge;

                            corp.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                                CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                                CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                                IsModified = false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.Branch.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_Branch DT_Corp = (DT_Branch)rtn_Obj;
                            DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            DT_Corp.Incharge_Id = corp.Incharge == null ? 0 : corp.Incharge.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
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
                IEnumerable<Branch> Branch = null;
                if (gp.IsAutho == true)
                {
                    Branch = db.Branch.Include(e => e.Incharge).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Branch = db.Branch.Include(e => e.Incharge).AsNoTracking().ToList();
                }

                IEnumerable<Branch> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Branch;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                               || (e.Name.ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();


                        //jsonData = IE.Select(a => new { a.Code, a.Name, a.Id }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Code.ToLower() == gp.searchString.ToLower()) || (e.Name.ToLower() == gp.searchString.ToLower()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.Incharge) != null ? Convert.ToString(a.Incharge.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Id, a.Incharge != null ? Convert.ToString(a.Incharge) : "" }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Branch;
                    Func<Branch, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.Code :
                                         gp.sidx == "Name" ? c.Name :
                                    "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id, a.Incharge != null ? Convert.ToString(a.Incharge) : "" }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id, a.Incharge != null ? Convert.ToString(a.Incharge) : "" }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Id, a.Incharge != null ? Convert.ToString(a.Incharge) : "" }).ToList();
                    }
                    totalRecords = Branch.Count();
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
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    Branch Branchs = db.Branch.Include(e => e.Address)
                                                       .Include(e => e.ContactDetails)
                                                       .Include(e => e.Incharge).Where(e => e.Id == data).SingleOrDefault();

                    Address add = Branchs.Address;
                    ContactDetails conDet = Branchs.ContactDetails;
                    Employee val = Branchs.Incharge;
                    //Branch Branchs = db.Branch.Where(e => e.Id == data).SingleOrDefault();
                    if (Branchs.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Branchs.DBTrack, Branchs, null, "Branch");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = Branchs.DBTrack.CreatedBy != null ? Branchs.DBTrack.CreatedBy : null,
                                CreatedOn = Branchs.DBTrack.CreatedOn != null ? Branchs.DBTrack.CreatedOn : null,
                                IsModified = Branchs.DBTrack.IsModified == true ? true : false
                            };
                            Branchs.DBTrack = dbT;
                            db.Entry(Branchs).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Branchs.DBTrack);
                            DT_Branch DT_Corp = (DT_Branch)rtn_Obj;
                            DT_Corp.Address_Id = Branchs.Address == null ? 0 : Branchs.Address.Id;
                            DT_Corp.Incharge_Id = Branchs.Incharge == null ? 0 : Branchs.Incharge.Id;
                            DT_Corp.ContactDetails_Id = Branchs.ContactDetails == null ? 0 : Branchs.ContactDetails.Id;
                            db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", Branchs, null, "Branch", Branchs.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", Branchs, null, "Branch", Branchs.DBTrack );
                            //}
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = Branchs.DBTrack.CreatedBy != null ? Branchs.DBTrack.CreatedBy : null,
                                    CreatedOn = Branchs.DBTrack.CreatedOn != null ? Branchs.DBTrack.CreatedOn : null,
                                    IsModified = Branchs.DBTrack.IsModified == true ? false : false//,
                                };

                                db.Entry(Branchs).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                DT_Branch DT_Corp = (DT_Branch)rtn_Obj;
                                DT_Corp.Address_Id = add == null ? 0 : add.Id;
                                DT_Corp.Incharge_Id = val == null ? 0 : val.Id;
                                DT_Corp.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
                                db.Create(DT_Corp);
                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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

        public ActionResult GetContactDetLKDetails(string data)
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
                    var list1 = db.Branch.ToList().Select(e => e.ContactDetails);
                    var list2 = fall.Except(list1);
                    var r = (from ca in list2 select new { srno = ca.Id, Employee = ca.FullContactDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullContactDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetAddressLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Address.Include(e => e.Area).Include(e => e.City).Include(e => e.Country)
                                     .Include(e => e.District).Include(e => e.State).Include(e => e.StateRegion)
                                     .Include(e => e.Taluka).ToList();
                IEnumerable<Address> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Address.ToList().Where(d => d.FullAddress.Contains(data));

                }
                else
                {
                    var list1 = db.Branch.ToList().Select(e => e.Address);
                    var list2 = fall.Except(list1);

                    var r = (from ca in list2 select new { srno = ca.Id, Employee = ca.FullAddress }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullAddress }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public int EditS(string Corp, string Addrs, string ContactDetails, int data, Branch c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Corp != null)
                {
                    if (Corp != "")
                    {
                        var val = db.Employee.Find(int.Parse(Corp));
                        c.Incharge = val;

                        var type = db.Branch.Include(e => e.Incharge).Where(e => e.Id == data).SingleOrDefault();
                        IList<Branch> typedetails = null;
                        if (type.Incharge != null)
                        {
                            typedetails = db.Branch.Where(x => x.Incharge.Id == type.Incharge.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.Branch.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in typedetails)
                        {
                            s.Incharge = c.Incharge;
                            db.Branch.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.Branch.Include(e => e.Incharge).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.Incharge = null;
                            db.Branch.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var BusiTypeDetails = db.Branch.Include(e => e.Incharge).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.Incharge = null;
                        db.Branch.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        var val = db.Address.Find(int.Parse(Addrs));
                        c.Address = val;

                        var add = db.Branch.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                        IList<Branch> addressdetails = null;
                        if (add.Address != null)
                        {
                            addressdetails = db.Branch.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.Branch.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.Address = c.Address;
                                db.Branch.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                }
                else
                {
                    var addressdetails = db.Branch.Include(e => e.Address).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.Address = null;
                        db.Branch.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (ContactDetails != null)
                {
                    if (ContactDetails != "")
                    {
                        var val = db.ContactDetails.Find(int.Parse(ContactDetails));
                        c.ContactDetails = val;

                        var add = db.Branch.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                        IList<Branch> contactsdetails = null;
                        if (add.ContactDetails != null)
                        {
                            contactsdetails = db.Branch.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            contactsdetails = db.Branch.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.ContactDetails = c.ContactDetails;
                            db.Branch.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var contactsdetails = db.Branch.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.ContactDetails = null;
                        db.Branch.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                var CurCorp = db.Branch.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    Branch corp = new Branch()
                    {
                        Code = c.Code,
                        Name = c.Name,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.Branch.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];

                    return 1;
                }
                return 0;
            }
        }



        public void RollBack()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var changedEntries = db.ChangeTracker.Entries()
                    .Where(x => x.State != System.Data.Entity.EntityState.Unchanged).ToList();

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Modified))
                {
                    entry.CurrentValues.SetValues(entry.OriginalValues);
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Added))
                {
                    entry.State = System.Data.Entity.EntityState.Detached;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Deleted))
                {
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }

            }


        }
    }
}





