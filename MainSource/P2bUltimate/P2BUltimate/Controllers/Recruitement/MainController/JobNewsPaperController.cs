using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Recruitment;
using System.Web.Mvc;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System.Transactions;
using P2b.Global;
using P2BUltimate.Security;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;

namespace P2BUltimate.Controllers.Recruitment.MainController
{
    public class JobNewsPaperController : Controller
    {
        //  private DataBaseContext db = new DataBaseContext();
        //
        public ActionResult Index()
        {
            return View("~/Views/Recruitement/MainView/JobNewsPaper/Index.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/Recruitement/_JobNewsPaper.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(JobNewsPaper c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string NewsPapAddr = form["NewPaperAddressList"] == "0" ? "" : form["NewPaperAddressList"];
                string ContactDetails = form["ContactDetailslistJN"] == "0" ? "" : form["ContactDetailslistJN"];
                //string namesingle = form["ContactPersonlistJN"] == "0" ? "" : form["ContactPersonlistJN"];
                int EmpName_FName = form["EmpName_id"] == "" ? 0 : Convert.ToInt32(form["EmpName_id"]);
                var id = int.Parse(Session["CompId"].ToString());
                var companyRecruitment = db.CompanyRecruitment.Where(e => e.Company.Id == id).SingleOrDefault();
                List<String> Msg = new List<String>();
                try
                {


                    if (NewsPapAddr != null)
                    {
                        if (NewsPapAddr != "")
                        {
                            int AddId = Convert.ToInt32(NewsPapAddr);
                            var val = db.Address//.Include(e => e.Area)
                                //.Include(e => e.City)
                                //.Include(e => e.Country)
                                //.Include(e => e.District)
                                //.Include(e => e.State)
                                //.Include(e => e.StateRegion)
                                //.Include(e => e.Taluka)
                                                .Where(e => e.Id == AddId).SingleOrDefault();
                            c.NewPaperAddress = val;
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
                    if (EmpName_FName != 0)
                    {
                        c.ContactPerson = db.NameSingle.Find(EmpName_FName);
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            //if (db.Corporate.Any(o => o.Code == c.Code))
                            //{
                            //    Msg.Add("Code Already Exists.");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            JobNewsPaper corporate = new JobNewsPaper()
                            {

                                Name = c.Name == null ? "" : c.Name.Trim(),
                                NewPaperAddress = c.NewPaperAddress,
                                ContactDetails = c.ContactDetails,
                                ContactPerson = c.ContactPerson,
                                GSTNo = c.GSTNo,
                                PANNo = c.PANNo,
                                DBTrack = c.DBTrack,
                                Id = c.Id
                            };

                            db.JobNewsPaper.Add(corporate);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Recruitment/Recruitment", null, db.ChangeTracker, c.DBTrack);
                            //DT_JobNewsPaper DT_Corp = (DT_JobNewsPaper)rtn_Obj;
                            //DT_Corp.NewPaperAddress_Id = c.NewPaperAddress == null ? 0 : c.NewPaperAddress.Id;
                            //DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                            //DT_Corp.ContactPerson_Id = c.ContactPerson == null ? 0 : c.ContactPerson.Id;
                            //db.Create(DT_Corp);
                            db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);

                            if (companyRecruitment != null)
                            {
                                List<JobNewsPaper> JobNewsPaperlist = new List<JobNewsPaper>();
                                JobNewsPaperlist.Add(corporate);
                                companyRecruitment.JobNewsPaper = JobNewsPaperlist;
                                db.Entry(companyRecruitment).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(companyRecruitment).State = System.Data.Entity.EntityState.Detached;
                            }
                            ts.Complete();
                            Msg.Add("Data Saved Successfully.");
                            return Json(new Utility.JsonReturnClass { Id = corporate.Id, Val = corporate.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                var Q = db.JobNewsPaper
                    .Include(e => e.NewPaperAddress)
                    .Include(e => e.ContactDetails)
                    .Include(e => e.ContactPerson)

                    .Where(e => e.Id == data).Select
                    (e => new
                    {

                        Name = e.Name,
                        PANNo = e.PANNo,
                        GSTNo = e.GSTNo,
                        ContactPerson_Name = e.ContactPerson.FullNameFML,
                        ContactPerson_Id = e.ContactPerson.Id,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.JobNewsPaper
                  .Include(e => e.ContactDetails)
                    .Include(e => e.NewPaperAddress)

                    .Include(e => e.ContactPerson)
                    .Include(e => e.NewPaperAddress.Area)
                    .Include(e => e.NewPaperAddress.City)
                    .Include(e => e.NewPaperAddress.Country)
                    .Include(e => e.NewPaperAddress.District)
                    .Include(e => e.NewPaperAddress.State)
                    .Include(e => e.NewPaperAddress.StateRegion)
                    .Include(e => e.NewPaperAddress.Taluka)
                    .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        NewsP_FullAddress = e.NewPaperAddress.FullAddress == null ? "" : e.NewPaperAddress.FullAddress,
                        Add_Id = e.NewPaperAddress.Id == null ? "" : e.NewPaperAddress.Id.ToString(),
                        Cont_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
                        ContPer_Id = e.ContactPerson.Id == null ? "" : e.ContactPerson.Id.ToString(),
                        FullContactDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails,
                        FullContactPersonDetail = e.ContactPerson.FullDetails == null ? "" : e.ContactPerson.FullDetails
                    }).ToList();


                //var W = db.DT_JobNewsPaper
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,

                //         Name = e.Name == null ? "" : e.Name,
                //         PANNo = e.PANNo == null ? "" : e.PANNo,
                //         GSTNo = e.GSTNo == null ? "" : e.GSTNo,



                //         News_Address_Val = e.NewPaperAddress_Id == 0 ? "" : db.Address.Where(x => x.Id == e.NewPaperAddress_Id).Select(x => x.FullAddress).FirstOrDefault(),
                //         Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault(),
                //         ContactPerson_Val = e.ContactPerson_Id == 0 ? "" : db.NameSingle.Where(x => x.Id == e.ContactPerson_Id).Select(x => x.FullDetails).FirstOrDefault()
                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.JobNewsPaper.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(JobNewsPaper c, int data, FormCollection form) // Edit submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string ContPer = form["ContactPersonlistJN"] == "0" ? "" : form["ContactPersonlistJN"];
                    string Addrs = form["NewPaperAddressList"] == "0" ? "" : form["NewPaperAddressList"];
                    string ContactDetails = form["ContactDetailslistJN"] == "0" ? "" : form["ContactDetailslistJN"];
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    int EmpName_FName = form["EmpName_id"] == "" ? 0 : Convert.ToInt32(form["EmpName_id"]);



                    if (Addrs != null)
                    {
                        if (Addrs != "")
                        {
                            var val = db.Address.Find(int.Parse(Addrs));
                            c.NewPaperAddress = val;

                            var add = db.JobNewsPaper.Include(e => e.NewPaperAddress).Where(e => e.Id == data).SingleOrDefault();
                            IList<JobNewsPaper> addressdetails = null;
                            if (add.NewPaperAddress != null)
                            {
                                addressdetails = db.JobNewsPaper.Where(x => x.NewPaperAddress.Id == add.NewPaperAddress.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                addressdetails = db.JobNewsPaper.Where(x => x.Id == data).ToList();
                            }
                            if (addressdetails != null)
                            {
                                foreach (var s in addressdetails)
                                {
                                    s.NewPaperAddress = c.NewPaperAddress;
                                    db.JobNewsPaper.Attach(s);
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
                        var addressdetails = db.JobNewsPaper.Include(e => e.NewPaperAddress).Where(x => x.Id == data).ToList();
                        foreach (var s in addressdetails)
                        {
                            s.NewPaperAddress = null;
                            db.JobNewsPaper.Attach(s);
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

                            var add = db.JobNewsPaper.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                            IList<JobNewsPaper> contactsdetails = null;
                            if (add.ContactDetails != null)
                            {
                                contactsdetails = db.JobNewsPaper.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                contactsdetails = db.JobNewsPaper.Where(x => x.Id == data).ToList();
                            }
                            foreach (var s in contactsdetails)
                            {
                                s.ContactDetails = c.ContactDetails;
                                db.JobNewsPaper.Attach(s);
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
                        var contactsdetails = db.JobNewsPaper.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                        foreach (var s in contactsdetails)
                        {
                            s.ContactDetails = null;
                            db.JobNewsPaper.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }


                    if (EmpName_FName != 0)
                    {
                        //c.ContactPerson = db.NameSingle.Find(EmpName_FName);

                        var val = db.NameSingle.Find(EmpName_FName);
                        c.ContactPerson = val;

                        var add = db.JobNewsPaper.Include(e => e.ContactPerson).Where(e => e.Id == data).SingleOrDefault();
                        IList<JobNewsPaper> ContactPerson = null;
                        if (add.ContactPerson != null)
                        {
                            ContactPerson = db.JobNewsPaper.Where(x => x.ContactPerson.Id == add.ContactPerson.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            ContactPerson = db.JobNewsPaper.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in ContactPerson)
                        {
                            s.ContactPerson = c.ContactPerson;
                            db.JobNewsPaper.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var ContactPerson = db.JobNewsPaper.Include(e => e.ContactPerson).Where(x => x.Id == data).ToList();
                        foreach (var s in ContactPerson)
                        {
                            s.ContactPerson = null;
                            db.JobNewsPaper.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }

                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {


                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                JobNewsPaper blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.JobNewsPaper.Where(e => e.Id == data)

                                                            .Include(e => e.NewPaperAddress)
                                                            .Include(e => e.ContactDetails)
                                                            .Include(e => e.ContactPerson).SingleOrDefault();
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

                                // int a = EditS(Addrs, ContactDetails, ContPer, data, c, c.DBTrack);
                                var CurCorp = db.JobNewsPaper.Find(data);
                                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    JobNewsPaper corp = new JobNewsPaper()
                                    {
                                        Name = c.Name,
                                        PANNo = c.PANNo,
                                        Id = data,
                                        GSTNo = c.GSTNo,
                                        DBTrack = c.DBTrack
                                    };
                                    db.JobNewsPaper.Attach(corp);
                                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                }


                                using (var context = new DataBaseContext())
                                {
                                    //c.Id = data;

                                    /// DBTrackFile.DBTrackSave("Core/P2b.Global", "M", blog, c, "Corporate", c.DBTrack);
                                    //PropertyInfo[] fi = null;
                                    //Dictionary<string, object> rt = new Dictionary<string, object>();
                                    //fi = c.GetType().GetProperties();
                                    ////foreach (var Prop in fi)
                                    ////{
                                    ////    if (Prop.Name != "Id" && Prop.Name != "DBTrack" && Prop.Name != "RowVersion")
                                    ////    {
                                    ////        rt.Add(Prop.Name, Prop.GetValue(c));
                                    ////    }
                                    ////}
                                    //rt = blog.DetailedCompare(c);
                                    //rt.Add("Orig_Id", c.Id);
                                    //rt.Add("Action", "M");
                                    //rt.Add("DBTrack", c.DBTrack);
                                    //rt.Add("RowVersion", c.RowVersion);
                                    //var aa = DBTrackFile.GetInstance("Core/P2b.Global", "DT_" + "Corporate", rt);
                                    //DT_Corporate d = (DT_Corporate)aa;
                                    //db.DT_Corporate.Add(d);
                                    //db.SaveChanges();

                                    //To save data in history table 
                                    //var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, c, "Corporate", c.DBTrack);
                                    //DT_Corporate DT_Corp = (DT_Corporate)Obj;
                                    //db.DT_Corporate.Add(DT_Corp);
                                    //db.SaveChanges();\


                                    var obj = DBTrackFile.DBTrackSave("Recruitment/Recruitment", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                    //DT_JobNewsPaper DT_Corp = (DT_JobNewsPaper)obj;
                                    //DT_Corp.NewPaperAddress_Id = blog.NewPaperAddress == null ? 0 : blog.NewPaperAddress.Id;
                                    //DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactPerson.Id;
                                    //DT_Corp.ContactPerson_Id = blog.ContactPerson == null ? 0 : blog.ContactPerson.Id;
                                    //db.Create(DT_Corp);
                                    db.SaveChanges();
                                }
                                await db.SaveChangesAsync();
                                ts.Complete();

                                Msg.Add("Record Updated Successfully.");
                                return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

                            JobNewsPaper blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            JobNewsPaper Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.JobNewsPaper.Where(e => e.Id == data).SingleOrDefault();
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

                            JobNewsPaper corp = new JobNewsPaper()
                            {

                                Name = c.Name,
                                PANNo = c.PANNo,
                                GSTNo = c.GSTNo,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Recruitement/Recruitement", "M", blog, corp, "JobNewsPaper", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.JobNewsPaper.Where(e => e.Id == data)
                                    .Include(e => e.NewPaperAddress).Include(e => e.ContactDetails).Include(e => e.ContactPerson).SingleOrDefault();
                                //DT_JobNewsPaper DT_Corp = (DT_JobNewsPaper)obj;
                                //DT_Corp.NewPaperAddress_Id= DBTrackFile.ValCompare(Old_Corp.NewPaperAddress, c.NewPaperAddress);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //DT_Corp.ContactPerson_Id = DBTrackFile.ValCompare(Old_Corp.ContactPerson, c.ContactPerson); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                //db.Create(DT_Corp);
                                db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.JobNewsPaper.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("Record Updated Successfully.");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var clientValues = (JobNewsPaper)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (JobNewsPaper)databaseEntry.ToObject();
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


        public int EditS(string Addrs, string ContactDetails, string ContPer, int data, JobNewsPaper c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        var val = db.Address.Find(int.Parse(Addrs));
                        c.NewPaperAddress = val;

                        var add = db.JobNewsPaper.Include(e => e.NewPaperAddress).Where(e => e.Id == data).SingleOrDefault();
                        IList<JobNewsPaper> addressdetails = null;
                        if (add.NewPaperAddress != null)
                        {
                            addressdetails = db.JobNewsPaper.Where(x => x.NewPaperAddress.Id == add.NewPaperAddress.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.JobNewsPaper.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.NewPaperAddress = c.NewPaperAddress;
                                db.JobNewsPaper.Attach(s);
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
                    var addressdetails = db.JobNewsPaper.Include(e => e.NewPaperAddress).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.NewPaperAddress = null;
                        db.JobNewsPaper.Attach(s);
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

                        var add = db.JobNewsPaper.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                        IList<JobNewsPaper> contactsdetails = null;
                        if (add.ContactDetails != null)
                        {
                            contactsdetails = db.JobNewsPaper.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            contactsdetails = db.JobNewsPaper.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.ContactDetails = c.ContactDetails;
                            db.JobNewsPaper.Attach(s);
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
                    var contactsdetails = db.JobNewsPaper.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.ContactDetails = null;
                        db.JobNewsPaper.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (ContPer != null)
                {
                    if (ContPer != "")
                    {
                        var val = db.NameSingle.Find(int.Parse(ContPer));
                        c.ContactPerson = val;

                        var add = db.JobNewsPaper.Include(e => e.ContactPerson).Where(e => e.Id == data).SingleOrDefault();
                        IList<JobNewsPaper> contactsdetails = null;
                        if (add.ContactPerson != null)
                        {
                            contactsdetails = db.JobNewsPaper.Where(x => x.ContactPerson.Id == add.ContactPerson.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            contactsdetails = db.JobNewsPaper.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.ContactPerson = c.ContactPerson;
                            db.JobNewsPaper.Attach(s);
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
                    var contactsdetails = db.JobNewsPaper.Include(e => e.ContactPerson).Where(x => x.Id == data).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.ContactPerson = null;
                        db.JobNewsPaper.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                var CurCorp = db.JobNewsPaper.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    JobNewsPaper corp = new JobNewsPaper()
                    {

                        Name = c.Name,
                        PANNo = c.PANNo,
                        GSTNo = c.GSTNo,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.JobNewsPaper.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
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
                IEnumerable<JobNewsPaper> JobNewsPaper = null;
                if (gp.IsAutho == true)
                {
                    JobNewsPaper = db.JobNewsPaper.Include(e => e.ContactPerson)
                        .Include(e => e.NewPaperAddress)
                        .Include(e => e.NewPaperAddress.City)
                        .Include(e => e.ContactDetails)
                        .Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    JobNewsPaper = db.JobNewsPaper.Include(e => e.ContactPerson)
                        .Include(e => e.NewPaperAddress)
                        .Include(e => e.NewPaperAddress.City)
                        .Include(e => e.ContactDetails)
                        .AsNoTracking().ToList();
                }

                IEnumerable<JobNewsPaper> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {

                    IE = JobNewsPaper;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.ContactPerson != null? e.ContactPerson.FullNameFML.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                               || (e.NewPaperAddress != null ? e.NewPaperAddress.City.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                               || (e.ContactDetails != null ? e.ContactDetails.EmailId.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.Name, a.ContactPerson != null ? a.ContactPerson.FullNameFML : "", a.NewPaperAddress != null ? a.NewPaperAddress.City.Name : "", a.ContactDetails != null ? a.ContactDetails.EmailId : "", a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Name, a.ContactPerson != null ? a.ContactPerson.FullNameFML : "", a.NewPaperAddress == null ? "" : a.NewPaperAddress.City == null ? "" : a.NewPaperAddress.City.Name, a.ContactDetails == null ? "" : a.ContactDetails.EmailId, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = JobNewsPaper;
                    Func<JobNewsPaper, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Name" ? c.Name :
                                         gp.sidx == "ContactPerson" ? c.ContactPerson == null ? "" : c.ContactPerson.FullNameFML :
                                         gp.sidx == "City" ? c.NewPaperAddress == null ? "" : c.NewPaperAddress.City == null ? "" : c.NewPaperAddress.City.Name :
                                         gp.sidx == "Email" ? c.ContactDetails == null ? "" : c.ContactDetails.EmailId : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Name, a.ContactPerson != null ? a.ContactPerson.FullNameFML : "", a.NewPaperAddress == null ? "" : a.NewPaperAddress.City == null ? "" : a.NewPaperAddress.City.Name, a.ContactDetails == null ? "" : a.ContactDetails.EmailId, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Name), a.ContactPerson != null ? a.ContactPerson.FullNameFML : "", a.NewPaperAddress == null ? "" : a.NewPaperAddress.City == null ? "" : a.NewPaperAddress.City.Name, a.ContactDetails == null ? "" : a.ContactDetails.EmailId, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Name, a.ContactPerson != null ? a.ContactPerson.FullNameFML : "", a.NewPaperAddress == null ? "" : a.NewPaperAddress.City == null ? "" : a.NewPaperAddress.City.Name, a.ContactDetails == null ? "" : a.ContactDetails.EmailId, a.Id }).ToList();
                    }
                    totalRecords = JobNewsPaper.Count();
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


        public void RollBack()
        {

            //  var context = DataContextFactory.GetDataContext();
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

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();

                try
                {
                    JobNewsPaper corporates = db.JobNewsPaper.Include(e => e.NewPaperAddress)
                                                       .Include(e => e.ContactDetails)
                                                       .Include(e => e.ContactPerson).Where(e => e.Id == data).SingleOrDefault();

                    //Address add = corporates.NewPaperAddress;
                    //ContactDetails conDet = corporates.ContactDetails;
                    //NameSingle val = corporates.ContactPerson;
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
                            var rtn_Obj = DBTrackFile.DBTrackSave("Recruitment/Recruitment", null, db.ChangeTracker, corporates.DBTrack);
                            DT_JobNewsPaper DT_Corp = (DT_JobNewsPaper)rtn_Obj;
                            DT_Corp.NewPaperAddress_Id = corporates.NewPaperAddress == null ? 0 : corporates.NewPaperAddress.Id;
                            DT_Corp.ContactDetails_Id = corporates.ContactDetails == null ? 0 : corporates.ContactDetails.Id;
                            DT_Corp.ContactPerson_Id = corporates.ContactPerson == null ? 0 : corporates.ContactPerson.Id;
                            db.Create(DT_Corp);
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
                        //var selectedRegions = corporates.Regions;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(corporates.Regions.Select(e => e.Id));
                            //    if (corpRegion.Count > 0)
                            //    {
                            //        Msg.Add(" Child record exists.Cannot remove it..  ");
                            //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //        // return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //    }
                            //}


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
                            var rtn_Obj = DBTrackFile.DBTrackSave("Recruitment/Recruitment", null, db.ChangeTracker, dbT);
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            //DT_JobNewsPaper DT_Corp = (DT_JobNewsPaper)rtn_Obj;
                            //DT_Corp.NewPaperAddress_Id = add == null ? 0 : add.Id;
                            //DT_Corp.ContactPerson_Id = val == null ? 0 : val.Id;
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
        public ActionResult GetContactDetLKDetails(List<int> SkipIds)
        {
            List<string> Msg = new List<string>();
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
                var list1 = db.JobNewsPaper.ToList().Select(e => e.ContactDetails);
                var list2 = fall.Except(list1);

                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
            // return View();
        }

        [HttpPost]
        public ActionResult GetAddressLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.Address.Include(e => e.Area).Include(e => e.City).Include(e => e.Country)
                                     .Include(e => e.District).Include(e => e.State).Include(e => e.StateRegion)
                                     .Include(e => e.Taluka).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Address.Include(e => e.Area).Include(e => e.City).Include(e => e.Country)
                                    .Include(e => e.District).Include(e => e.State).Include(e => e.StateRegion)
                                    .Include(e => e.Taluka).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var list1 = db.JobNewsPaper.ToList().Select(e => e.NewPaperAddress);
                var list2 = fall.Except(list1);

                //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLookupJobNewsPaper_EmpName()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var rall = db.JobNewsPaper.Select(p => new
                {
                    ContactPersonId = p.ContactPerson.Id
                }).ToList();
                var JobNewsPaperEmpNameId = rall.Select(w => w.ContactPersonId).ToList();
                var lall = db.NameSingle.Select(q => new
                {
                    FullNameFML = q.FullNameFML,
                    NameSingleId = q.Id
                }).Where(e => !JobNewsPaperEmpNameId.Contains(e.NameSingleId)).ToList();

                var r = (from ca in lall select new { srno = ca.NameSingleId, lookupvalue = ca.FullNameFML }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
    }
}