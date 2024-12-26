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
    public class EmailAttachmentController : Controller
    {

        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/EmailAttachment/Index.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(EmailAttachment c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string EmailAttachmentTypelist = form["EmailAttachmentTypelist"] == "0" ? "" : form["EmailAttachmentTypelist"];

                List<String> Msg = new List<String>();
                try
                {
                    if (EmailAttachmentTypelist != null)
                    {
                        if (EmailAttachmentTypelist != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "803").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(EmailAttachmentTypelist)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailAttachmentTypelist));
                            c.EmailAttachmentType = val;
                        }
                    }




                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            EmailAttachment corporate = new EmailAttachment()
                            {
                                EmailAttachmentType = c.EmailAttachmentType,
                                Name = c.Name,
                                DocPath = c.DocPath,
                                DBTrack = c.DBTrack
                            };

                            db.EmailAttachment.Add(corporate);
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                            //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            //DT_Corp.Address_Id = c.Address == null ? 0 : c.Address.Id;
                            //DT_Corp.BusinessType_Id = c.BusinessType == null ? 0 : c.BusinessType.Id;
                            //db.Create(DT_Corp);
                            db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


                            ts.Complete();
                            Msg.Add("Data Saved Successfully.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        //catch (DbUpdateConcurrencyException)
                        //{
                        //    return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                        //}


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
                        Msg.Add("Code Already Exists.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        //return this.Json(new { msg = errorMsg });
                    }
                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }

        }



        //public class EmailAddress1
        //{
        //    public Array Address_Id { get; set; }
        //    public Array Address_val { get; set; }

        //}

        [HttpPost]
        public ActionResult Edit(int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.EmailAttachment
                    .Include(e => e.EmailAttachmentType)

                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Name = e.Name,
                        DocPath = e.DocPath,
                        EmailAction_Id = e.EmailAttachmentType.Id == null ? 0 : e.EmailAttachmentType.Id,
                        Action = e.DBTrack.Action
                    }).ToList();

                //List<EmailAddress1> cat = new List<EmailAddress1>();

                //var add_data = db.Email
                //               .Include(e => e.ToAddresses).Where(e => e.Id == data).ToList();

                //foreach (var ca in add_data)
                //{
                //    cat.Add(new EmailAddress1
                //    {
                //        Address_Id = ca.ToAddresses.Select(e => e.Id.ToString()).ToArray(),
                //        Address_val = ca.ToAddresses.Select(e => e.Address).ToArray(),
                //    });
                //}




                var Corp = db.EmailAttachment.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(EmailAttachment c, int data, FormCollection form) // Edit submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string EmailAttachmentTypelist = form["EmailAttachmentTypelist"] == "0" ? "" : form["EmailAttachmentTypelist"];
                   
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    if (EmailAttachmentTypelist != null)
                    {
                        if (EmailAttachmentTypelist != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "803").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(EmailAttachmentTypelist)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailAttachmentTypelist));
                            c.EmailAttachmentType = val;
                            c.EmailAttachmentType_Id = Convert.ToInt32(EmailAttachmentTypelist);
                        }
                    }
                    else
                    {
                        c.EmailAttachmentType_Id = null;
                    }


                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {


                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                EmailAttachment blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.EmailAttachment.Where(e => e.Id == data).Include(e => e.EmailAttachmentType)
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

                                //  int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

                              

                               
                                EmailAttachment corp = new EmailAttachment()
                                    {
                                        EmailAttachmentType = c.EmailAttachmentType,
                                        EmailAttachmentType_Id=c.EmailAttachmentType_Id,
                                        Name = c.Name,
                                        DocPath=c.DocPath,
                                        DBTrack = c.DBTrack,
                                        Id = data
                                    };


                                db.EmailAttachment.Attach(corp);
                                db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                
                                db.SaveChanges();
                                await db.SaveChangesAsync();
                                ts.Complete();

                                Msg.Add("Record Updated Successfully.");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            List<string> MsgB = new List<string>();
                            MsgB.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            EmailAttachment blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            //Email Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.EmailAttachment.Where(e => e.Id == data).Include(e => e.EmailAttachmentType).SingleOrDefault();
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

                            EmailAttachment corp = new EmailAttachment()
                            {
                                EmailAttachmentType = c.EmailAttachmentType,
                                Name = c.Name,

                                DBTrack = c.DBTrack,
                                Id = data,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };



                           
                            blog.DBTrack = c.DBTrack;
                            db.EmailAttachment.Attach(blog);
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
                    var clientValues = (Corporate)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (Corporate)databaseEntry.ToObject();
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
                            //Corporate corp = db.Corporate.Find(auth_id);
                            //Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                            Corporate corp = db.Corporate.Include(e => e.Address)
                                .Include(e => e.ContactDetails)
                                .Include(e => e.BusinessType).FirstOrDefault(e => e.Id == auth_id);

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

                            db.Corporate.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            DT_Corp.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        Corporate Old_Corp = db.Corporate.Include(e => e.BusinessType)
                                                          .Include(e => e.Address)
                                                          .Include(e => e.ContactDetails).Where(e => e.Id == auth_id).SingleOrDefault();

                        //var W = db.DT_Corporate
                        //.Include(e => e.ContactDetails)
                        //.Include(e => e.Address)
                        //.Include(e => e.BusinessType)
                        //.Include(e => e.ContactDetails)
                        //.Where(e => e.Orig_Id == auth_id && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                        //(e => new
                        //{
                        //    DT_Id = e.Id,
                        //    Code = e.Code == null ? "" : e.Code,
                        //    Name = e.Name == null ? "" : e.Name,
                        //    BusinessType_Val = e.BusinessType.Id == null ? "" : e.BusinessType.LookupVal,
                        //    Address_Val = e.Address.Id == null ? "" : e.Address.FullAddress,
                        //    Contact_Val = e.ContactDetails.Id == null ? "" : e.ContactDetails.FullContactDetails,
                        //}).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                        DT_Corporate Curr_Corp = db.DT_Corporate
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Corp != null)
                        {
                            Corporate corp = new Corporate();

                            string Corp = Curr_Corp.BusinessType_Id == null ? null : Curr_Corp.BusinessType_Id.ToString();
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
                                        //var CurCorp = db.Corporate.Find(auth_id);
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
                                        //    Corporate corp = new Corporate()
                                        //    {
                                        //        Code = c.Code,
                                        //        Name = c.Name,
                                        //        Id = Convert.ToInt32(auth_id),
                                        //        DBTrack = c.DBTrack
                                        //    };


                                        //    db.Corporate.Attach(corp);
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
                                        Msg.Add(" Record Authorised ");
                                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (Corporate)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (Corporate)databaseEntry.ToObject();
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
                            List<string> Msgr = new List<string>();
                            Msgr.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);

                        }
                        // return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Corporate corp = db.Corporate.Find(auth_id);
                            Corporate corp = db.Corporate.AsNoTracking().Include(e => e.Address)
                                                                        .Include(e => e.BusinessType)
                                                                        .Include(e => e.ContactDetails).FirstOrDefault(e => e.Id == auth_id);

                            Address add = corp.Address;
                            ContactDetails conDet = corp.ContactDetails;
                            LookupValue val = corp.BusinessType;

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

                            db.Corporate.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            DT_Corp.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            //return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                return View();
            }
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            List<string> Msg = new List<string>();

            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<EmailAttachment> Corporate = null;
                if (gp.IsAutho == true)
                {
                    Corporate = db.EmailAttachment.Include(e => e.EmailAttachmentType).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Corporate = db.EmailAttachment.Include(e => e.EmailAttachmentType).AsNoTracking().ToList();
                }

                IEnumerable<EmailAttachment> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = Corporate;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmailAttachmentType != null ? e.EmailAttachmentType.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                                || (e.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.DocPath.ToString().Contains(gp.searchString.ToUpper()))
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.EmailAttachmentType != null ? a.EmailAttachmentType.LookupVal : "", a.Name, a.DocPath, a.Id }).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmailAttachmentType != null ? Convert.ToString(a.EmailAttachmentType.LookupVal) : "", a.Name, a.DocPath, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Corporate;
                    Func<EmailAttachment, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>
                                         gp.sidx == "EmailAttachmentType" ? c.EmailAttachmentType.LookupVal :
                                          gp.sidx == "Name" ? c.Name : "");

                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmailAttachmentType != null ? Convert.ToString(a.EmailAttachmentType.LookupVal) : "", Convert.ToString(a.Name), a.DocPath, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmailAttachmentType != null ? Convert.ToString(a.EmailAttachmentType.LookupVal) : "", Convert.ToString(a.Name), a.DocPath, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmailAttachmentType != null ? Convert.ToString(a.EmailAttachmentType.LookupVal) : "", Convert.ToString(a.Name), a.DocPath, a.Id }).ToList();
                    }
                    totalRecords = Corporate.Count();
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
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    EmailAttachment corporates = db.EmailAttachment.Include(e => e.EmailAttachmentType)

                                                       .Where(e => e.Id == data).SingleOrDefault();


                    LookupValue val = corporates.EmailAttachmentType;
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
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack);
                            //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            //DT_Corp.Address_Id = corporates.Address == null ? 0 : corporates.Address.Id;
                            //DT_Corp.BusinessType_Id = corporates.BusinessType == null ? 0 : corporates.BusinessType.Id;
                            //DT_Corp.ContactDetails_Id = corporates.ContactDetails == null ? 0 : corporates.ContactDetails.Id;
                            //db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            //DT_Corp.Address_Id = add == null ? 0 : add.Id;
                            //DT_Corp.BusinessType_Id = val == null ? 0 : val.Id;
                            //DT_Corp.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
                            //db.Create(DT_Corp);

                            await db.SaveChangesAsync();


                            //using (var context = new DataBaseContext())
                            //{
                            //    corporates.Address = add;
                            //    corporates.ContactDetails = conDet;
                            //    corporates.BusinessType = val;
                            //    DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", dbT);
                            //}
                            ts.Complete();
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.)
                    //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                    //return RedirectToAction("Delete");
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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
        public ActionResult GetAddressLKDetails(List<int> SkipIds)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = new List<EmailAddress>();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Email.Include(e => e.ToAddresses).Where(e => !e.Id.ToString().Contains(a.ToString())).SelectMany(h => h.ToAddresses).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var list1 = db.EmailAddress.ToList();
                var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.Address }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public int EditS(string Corp, string Addrs, string ContactDetails, int data, Corporate c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Corp != null)
                {
                    if (Corp != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Corp));
                        c.BusinessType = val;

                        var type = db.Corporate.Include(e => e.BusinessType).Where(e => e.Id == data).SingleOrDefault();
                        IList<Corporate> typedetails = null;
                        if (type.BusinessType != null)
                        {
                            typedetails = db.Corporate.Where(x => x.BusinessType.Id == type.BusinessType.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.Corporate.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.BusinessType = c.BusinessType;
                            db.Corporate.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.Corporate.Include(e => e.BusinessType).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.BusinessType = null;
                            db.Corporate.Attach(s);
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
                    var BusiTypeDetails = db.Corporate.Include(e => e.BusinessType).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.BusinessType = null;
                        db.Corporate.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
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

                        var add = db.Corporate.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                        IList<Corporate> addressdetails = null;
                        if (add.Address != null)
                        {
                            addressdetails = db.Corporate.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.Corporate.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.Address = c.Address;
                                db.Corporate.Attach(s);
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
                    var addressdetails = db.Corporate.Include(e => e.Address).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.Address = null;
                        db.Corporate.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
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

                        var add = db.Corporate.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                        IList<Corporate> contactsdetails = null;
                        if (add.ContactDetails != null)
                        {
                            contactsdetails = db.Corporate.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            contactsdetails = db.Corporate.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.ContactDetails = c.ContactDetails;
                            db.Corporate.Attach(s);
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
                    var contactsdetails = db.Corporate.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.ContactDetails = null;
                        db.Corporate.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                var CurCorp = db.Corporate.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    Corporate corp = new Corporate()
                    {
                        Code = c.Code,
                        Name = c.Name,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.Corporate.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }

        //[HttpPost]
        ////[ValidateAntiForgeryToken]
        //public ActionResult DeleteContactDetails(int? data, int? forwarddata)
        //{
        //    ContactDetails contDet = db.ContactDetails.Find(data);
        //    Corporate corp = db.Corporate.Find(forwarddata);
        //    try
        //    {
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            corp.ContactDetails = null;
        //            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //            ts.Complete();
        //        }
        //        //return Json(new Object[] { "", "", "Data removed.", JsonRequestBehavior.AllowGet });
        //        return Json(new { msg = "Data removed.", JsonRequestBehavior.AllowGet });
        //    }

        //    catch (DataException /* dex */)
        //    {
        //        //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
        //        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
        //        //ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
        //        //return RedirectToAction("Index");
        //    }
        //}


        //[HttpPost]
        ////[ValidateAntiForgeryToken]
        //public ActionResult DeleteAddress(int? data, int? forwarddata)
        //{
        //    Address addrs = db.Address.Find(data);
        //    Corporate corp = db.Corporate.Find(forwarddata);
        //    try
        //    {
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            corp.Address = null;
        //            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //            ts.Complete();
        //        }
        //        return Json(new Object[] { "", "", "Data removed.", JsonRequestBehavior.AllowGet });
        //       // return this.Json(new { msg = "Data deleted." });
        //    }

        //    catch (DataException /* dex */)
        //    {
        //        //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
        //        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
        //        //ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
        //        //return RedirectToAction("Index");
        //    }
        //}

        //public 

        public void RollBack()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //  var context = DataContextFactory.GetDataContext();
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
