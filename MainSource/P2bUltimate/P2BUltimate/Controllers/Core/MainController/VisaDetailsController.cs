///
/// Created by Tanushri
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
    public class VisaDetailsController : Controller
    {

        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/VisaDetails/Index.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(VisaDetails COBJ, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string VisaType = form["VisaTypelist"] == "0" ? "" : form["VisaTypelist"];
                    string Country = form["CountryList_DDL"] == "0" ? "" : form["CountryList_DDL"];
                    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);


                    if (VisaType != null)
                    {
                        if (VisaType != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "316").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(VisaType)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(VisaType));
                            COBJ.VisaType = val;
                        }
                    }

                    if (Country != null)
                    {
                        if (Country != "")
                        {
                            int AddId = Convert.ToInt32(Country);
                            var val = db.Country
                                                .Where(e => e.Id == AddId).SingleOrDefault();
                            COBJ.Country = val;
                        }
                    }

                    Employee empdata;
                    if (Emp != null && Emp != 0)
                    {
                        empdata = db.Employee.Find(Emp);
                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }



                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            var EmpSocialInfo1 = db.Employee.Include(e => e.VisaDetails).Include(e => e.VisaDetails.Select(q => q.Country))
                                               .Include(e => e.VisaDetails.Select(q => q.VisaType))
                                                  .Where(e => e.Id != null).ToList();
                            foreach (var item in EmpSocialInfo1)
                            {
                                if (item.VisaDetails.Count != 0 && empdata.VisaDetails.Count != 0)
                                {
                                    int aid = item.VisaDetails.Select(a => a.Id).FirstOrDefault();
                                    int bid = empdata.VisaDetails.Select(a => a.Id).FirstOrDefault();

                                    if (aid == bid)
                                    {
                                        var v = empdata.EmpCode;
                                        Msg.Add("Record Already Exist For Employee Code=" + v);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }

                            }
                            if (COBJ.IssueDate > COBJ.ValidDate)
                            {
                                Msg.Add("Issue Date Should be less than Valid Date.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { null, null, "Issue Date Should be less than Expiry Date.", JsonRequestBehavior.AllowGet });
                            }

                            DateTime current = DateTime.Now;
                            if (COBJ.IssueDate > current)
                            {

                                Msg.Add("  Issue Date Should be less than Current Date.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            VisaDetails VisaDetails = new VisaDetails()
                            {
                                IssueAt = COBJ.IssueAt == null ? "" : COBJ.IssueAt.Trim(),
                                IssueDate = COBJ.IssueDate,
                                ValidDate = COBJ.ValidDate,
                                VisaType = COBJ.VisaType,
                                Country = COBJ.Country,
                                DBTrack = COBJ.DBTrack
                            };
                            try
                            {
                                db.VisaDetails.Add(VisaDetails);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, COBJ.DBTrack);
                                DT_VisaDetails DT_OBJ = (DT_VisaDetails)rtn_Obj;
                                db.Create(DT_OBJ);
                                db.SaveChanges();

                                List<VisaDetails> empVisaDetails = new List<VisaDetails>();
                                empVisaDetails.Add(VisaDetails);
                                empdata.VisaDetails = empVisaDetails;
                                db.Employee.Attach(empdata);
                                db.Entry(empdata).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = COBJ.Id });
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

        //public ActionResult Editcontactdetails_partial(int data)
        //{
        //    var r = (from ca in db.VisaType
        //             .Where(e => e.Id == data)
        //             select new
        //             {
        //                 Id = ca.Id,
        //                 EmailId = ca.EmailId,
        //                 FaxNo = ca.FaxNo,
        //                 Website = ca.Website
        //             }).ToList();

        //    var a = db.VisaType.Include(e => e.ContactNumbers).Where(e => e.Id == data).SingleOrDefault();
        //    var b = a.ContactNumbers;

        //    var r1 = (from s in b
        //              select new
        //              {
        //                  Id = s.Id,
        //                  FullContactNumbers = s.FullContactNumbers
        //              }).ToList();

        //    TempData["RowVersion"] = db.VisaType.Find(data).RowVersion;
        //    return Json(new object[] { r, r1 }, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult Createcontactdetails_partial()
        //{
        //    return View("~/Views/Shared/_Contactdetails.cshtml");
        //}

        //[HttpPost]
        //public ActionResult Edit(int data)
        //{

        //    var Q = db.VisaDetails                
        //        .Include(e => e.Country)
        //        .Include(e => e.VisaType)              
        //        .Where(e => e.Id == data).Select
        //        (e => new
        //        {
        //            IssueAt = e.IssueAt,
        //            IssueDate = e.IssueDate,
        //            ValidDate = e.ValidDate,
        //            VisaType_Id = e.VisaType.Id,
        //              Country_Id = e.Country.Id ,
        //            Action = e.DBTrack.Action
        //        }).ToList();

        //    var add_data = db.VisaDetails             
        //        .Include(e => e.Country)
        //        .Include(e => e.VisaType)              
        //        .Where(e => e.Id == data)
        //        .Select(e => new
        //        {
        //            Country_FullDetails= e.Country.Name == null ? "" : e.Country.Name,
        //            Country_Id = e.Country.Id == null ? "" : e.Country.Id.ToString(),                    
        //        }).ToList();

        //    var Old_Data = db.DT_VisaDetails 
        //       // .Include(e => e.VisaType)
        //       //  .Include(e => e.Country)                 
        //         .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
        //         (e => new
        //         {
        //             DT_Id = e.Id,
        //             IssueAt = e.IssueAt == null ? "" : e.IssueAt,
        //             IssueDate = e.IssueDate,
        //             ValidDate = e.ValidDate,
        //             VisaType_Val = e.VisaType_Id == 0 ? "" : db.LookupValue
        //                      .Where(x => x.Id == e.VisaType_Id)
        //                      .Select(x => x.LookupVal).FirstOrDefault(),
        //             Country_Val = e.Country_Id == 0 ? "" : db.Country.Where(x => x.Id == e.Country_Id).Select(x => x.Name).FirstOrDefault()
        //                                                }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
        //            var EOBJ = db.VisaDetails.Find(data);
        //            TempData["RowVersion"] = EOBJ.RowVersion;
        //            var Auth = EOBJ.DBTrack.IsModified;
        //            return Json(new Object[] { Q, "", Old_Data, Auth, JsonRequestBehavior.AllowGet });
        //}

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Passport = db.Employee.Include(e => e.VisaDetails).Include(e => e.VisaDetails.Select(q => q.Country))
                    .Include(e => e.VisaDetails.Select(q => q.VisaType))
                                    .Where(e => e.Id == data).SingleOrDefault();
                int AID = Passport.VisaDetails.Select(s => s.Id).SingleOrDefault();
                var r = (from e in Passport.VisaDetails
                         select new
                         {
                             Id = e.Id,
                             IssueAt = e.IssueAt,
                             IssueDate = e.IssueDate,
                             ValidDate = e.ValidDate,
                             VisaType_Id = e.VisaType.Id,
                             Country_Id = e.Country.Id,
                             Action = e.DBTrack.Action
                         }).Distinct();

                var a = "";

                var Old_Data = db.DT_VisaDetails
                    // .Include(e => e.VisaType)
                    //  .Include(e => e.Country)                 
                      .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                      (e => new
                      {
                          DT_Id = e.Id,
                          IssueAt = e.IssueAt == null ? "" : e.IssueAt,
                          IssueDate = e.IssueDate,
                          ValidDate = e.ValidDate,
                          VisaType_Val = e.VisaType_Id == 0 ? "" : db.LookupValue
                                   .Where(x => x.Id == e.VisaType_Id)
                                   .Select(x => x.LookupVal).FirstOrDefault(),
                          Country_Val = e.Country_Id == 0 ? "" : db.Country.Where(x => x.Id == e.Country_Id).Select(x => x.Name).FirstOrDefault()
                      }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var LKup = db.VisaDetails.Find(AID);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, a, Old_Data, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        public async Task<ActionResult> EditSave1(VisaDetails ESOBJ, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string VisaTypelst = form["VisaTypelist"] == "0" ? "" : form["VisaTypelist"];
                    string Countrylst = form["CountryList_DDL"] == "0" ? "" : form["CountryList_DDL"];

                    var Passport = db.Employee.Include(e => e.VisaDetails).Include(e => e.VisaDetails.Select(q => q.Country))
           .Include(e => e.VisaDetails.Select(q => q.VisaType))
                           .Where(e => e.Id == data).SingleOrDefault();
                    // var AID = Passport.VisaDetails.Select(s => s.Id).ToString();
                    // int AIDD = int.Parse(AID);
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    if (VisaTypelst != null)
                    {
                        if (VisaTypelst != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(VisaTypelst));
                            ESOBJ.VisaType = val;
                        }
                    }

                    if (Countrylst != null)
                    {
                        if (Countrylst != "")
                        {
                            int AddId = Convert.ToInt32(Countrylst);
                            var val = db.Country.Where(e => e.Id == AddId).SingleOrDefault();
                            ESOBJ.Country = val;
                        }
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
                                    VisaDetails blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.VisaDetails.Where(e => e.Id == data).Include(e => e.VisaType).Include(e => e.Country)
                                                               .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    ESOBJ.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    int a = EditS(VisaTypelst, Countrylst, data, ESOBJ, ESOBJ.DBTrack);

                                    //await db.SaveChangesAsync();

                                    using (var context = new DataBaseContext())
                                    {
                                        ////var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, ESOBJ, "VisaDetails", ESOBJ.DBTrack);
                                        ////DT_VisaDetails DT_VTD = (DT_VisaDetails)Obj;
                                        ////db.DT_VisaDetails.Add(DT_VTD);

                                        //var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                        //DT_VisaDetails DT_OBJ = (DT_VisaDetails)obj;
                                        ////DT_OBJ.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
                                        //DT_OBJ.VisaType_Id = blog.VisaType == null ? 0 : blog.VisaType.Id;
                                        //DT_OBJ.Country_Id = blog.Country == null ? 0 : blog.Country.Id;
                                        //db.Create(DT_OBJ);
                                        //db.SaveChanges();
                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                        DT_VisaDetails DT_OBJ = (DT_VisaDetails)obj;
                                        DT_OBJ.VisaType_Id = blog.VisaType == null ? 0 : blog.VisaType.Id;
                                        DT_OBJ.Country_Id = blog.Country == null ? 0 : blog.Country.Id;
                                        db.Create(DT_OBJ);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.IssueAt, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { ESOBJ.Id, ESOBJ.IssueAt, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (VisaDetails)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (VisaDetails)databaseEntry.ToObject();
                                    ESOBJ.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        //    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        //    {
                        //        VisaDetails Old_OBJ = db.VisaDetails.Include(e => e.VisaType)
                        //                                            .Include(e => e.Country)
                        //                                           .Where(e => e.Id == data).SingleOrDefault();

                        //        VisaDetails Curr_OBJ = ESOBJ;
                        //        ESOBJ.DBTrack = new DBTrack
                        //        {
                        //            CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
                        //            CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
                        //            Action = "M",
                        //            IsModified = Old_OBJ.DBTrack.IsModified == true ? true : false,
                        //            //ModifiedBy = SessionManager.UserName,
                        //            //ModifiedOn = DateTime.Now
                        //        };
                        //        Old_OBJ.DBTrack = ESOBJ.DBTrack;

                        //        db.Entry(Old_OBJ).State = System.Data.Entity.EntityState.Modified;
                        //        db.SaveChanges();
                        //        using (var context = new DataBaseContext())
                        //        {
                        //            //DBTrackFile.DBTrackSave("Core/P2b.Global", "M", Old_OBJ, Curr_OBJ, "VisaDetails", ESOBJ.DBTrack);
                        //        }

                        //        ts.Complete();
                        //        return Json(new Object[] { Old_OBJ.Id, ESOBJ.IssueDate, "Record Updated", JsonRequestBehavior.AllowGet });
                        //    }

                        //}
                        //return View();


                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            VisaDetails blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            VisaDetails Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.VisaDetails.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            ESOBJ.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            VisaDetails corp = new VisaDetails()
                            {
                                IssueAt = ESOBJ.IssueAt,
                                IssueDate = ESOBJ.IssueDate,
                                ValidDate = ESOBJ.ValidDate,
                                VisaType = ESOBJ.VisaType,
                                Country = ESOBJ.Country,
                                Id = data,
                                DBTrack = ESOBJ.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "VisaDetails", ESOBJ.DBTrack);
                                Old_Corp = context.VisaDetails.Where(e => e.Id == data).Include(e => e.VisaType).Include(e => e.Country)
                                    .SingleOrDefault();
                                DT_VisaDetails DT_Corp = (DT_VisaDetails)obj;
                                DT_Corp.VisaType_Id = DBTrackFile.ValCompare(Old_Corp.VisaType, ESOBJ.VisaType); //Old_Corp.VenuType == c.VenuType ? 0 : Old_Corp.VenuType == null && c.VenuType != null ? c.VenuType.Id : Old_Corp.VenuType.Id;
                                DT_Corp.Country_Id = DBTrackFile.ValCompare(Old_Corp.Country, ESOBJ.Country); //Old_Corp.LvHeadObj == c.LvHeadObj ? 0 : Old_Corp.LvHeadObj == null && c.LvHeadObj != null ? c.LvHeadObj.Id : Old_Corp.LvHeadObj.Id;
                                db.Create(DT_Corp);
                            }
                            blog.DBTrack = ESOBJ.DBTrack;
                            db.VisaDetails.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, data = ESOBJ.Id.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { blog.Id, ESOBJ.Id, "Record Updated", JsonRequestBehavior.AllowGet });
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
        public async Task<ActionResult> EditSave(VisaDetails ESOBJ, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string VisaTypelst = form["VisaTypelist"] == "0" ? "" : form["VisaTypelist"];
                    string Countrylst = form["CountryList_DDL"] == "0" ? "" : form["CountryList_DDL"];

                    var Passport = db.Employee.Include(e => e.VisaDetails).Include(e => e.VisaDetails.Select(q => q.Country))
           .Include(e => e.VisaDetails.Select(q => q.VisaType))
                           .Where(e => e.Id == data).SingleOrDefault();
                    int AID = Passport.VisaDetails.Select(s => s.Id).SingleOrDefault();
                    //   int AIDD = int.Parse(AID);
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    if (VisaTypelst != null)
                    {
                        if (VisaTypelst != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "316").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(VisaTypelst)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(VisaTypelst));
                            ESOBJ.VisaType = val;
                        }
                    }

                    if (Countrylst != null)
                    {
                        if (Countrylst != "")
                        {
                            int AddId = Convert.ToInt32(Countrylst);
                            var val = db.Country.Where(e => e.Id == AddId).SingleOrDefault();
                            ESOBJ.Country = val;
                        }
                    }
                    if (VisaTypelst != null)
                    {
                        if (VisaTypelst != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "316").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(VisaTypelst)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(VisaTypelst));
                            ESOBJ.VisaType = val;

                            var type = db.VisaDetails.Include(e => e.VisaType).Where(e => e.Id == AID).SingleOrDefault();
                            IList<VisaDetails> typedetails = null;
                            if (type.VisaType != null)
                            {
                                typedetails = db.VisaDetails.Where(x => x.VisaType.Id == type.VisaType.Id && x.Id == AID).ToList();
                            }
                            else
                            {
                                typedetails = db.VisaDetails.Where(x => x.Id == AID).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.VisaType = ESOBJ.VisaType;
                                db.VisaDetails.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }

                    if (Countrylst != null)
                    {
                        if (Countrylst != "")
                        {
                            var val = db.Country.Find(int.Parse(Countrylst));
                            ESOBJ.Country = val;

                            var add = db.VisaDetails.Include(e => e.Country).Where(e => e.Id == AID).SingleOrDefault();
                            IList<VisaDetails> addressdetails = null;
                            if (add.Country != null)
                            {
                                addressdetails = db.VisaDetails.Where(x => x.Country.Id == add.Country.Id && x.Id == AID).ToList();
                            }
                            else
                            {
                                addressdetails = db.VisaDetails.Where(x => x.Id == AID).ToList();
                            }
                            if (addressdetails != null)
                            {
                                foreach (var s in addressdetails)
                                {
                                    s.Country = ESOBJ.Country;
                                    db.VisaDetails.Attach(s);
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
                        var addressdetails = db.VisaDetails.Include(e => e.Country).Where(x => x.Id == AID).ToList();
                        foreach (var s in addressdetails)
                        {
                            s.Country = null;
                            db.VisaDetails.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    

                    if (Auth == false)
                    {
                        //if (ModelState.IsValid)
                        //{
                        //    try
                        //    {
                        //        if (ESOBJ.IssueDate > ESOBJ.ValidDate)
                        //        {
                        //            Msg.Add("Issue Date Should be less than Valid Date.  ");
                        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //            //return this.Json(new Object[] { null, null, "Issue Date Should be less than Expiry Date.", JsonRequestBehavior.AllowGet });
                        //        }

                        //        //DbContextTransaction transaction = db.Database.BeginTransaction();

                        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        //        {
                        //            VisaDetails blog = null; // to retrieve old data
                        //            DbPropertyValues originalBlogValues = null;

                        //            using (var context = new DataBaseContext())
                        //            {
                        //                blog = context.VisaDetails.Where(e => e.Id == AID).Include(e => e.VisaType).Include(e => e.Country)
                        //                                       .SingleOrDefault();
                        //                originalBlogValues = context.Entry(blog).OriginalValues;
                        //            }

                        //            ESOBJ.DBTrack = new DBTrack
                        //            {
                        //                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                        //                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                        //                Action = "M",
                        //                ModifiedBy = SessionManager.UserName,
                        //                ModifiedOn = DateTime.Now
                        //            };

                        //            int a = EditS(VisaTypelst, Countrylst, AID, ESOBJ, ESOBJ.DBTrack);

                        //            //await db.SaveChangesAsync();

                        //            using (var context = new DataBaseContext())
                        //            {
                        //                var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, ESOBJ, "VisaDetails", ESOBJ.DBTrack);
                        //                DT_VisaDetails DT_VTD = (DT_VisaDetails)Obj;
                        //                db.DT_VisaDetails.Add(DT_VTD);

                        //                var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                        //                DT_VisaDetails DT_OBJ = (DT_VisaDetails)obj;
                        //                DT_OBJ.VisaType_Id = blog.VisaType == null ? 0 : blog.VisaType.Id;
                        //                DT_OBJ.Country_Id = blog.Country == null ? 0 : blog.Country.Id;
                        //                //DT_OBJ.Country_Id = blog.Country == null ? 0 : blog.Country.Id;
                        //                db.Create(DT_OBJ);
                        //                db.SaveChanges();
                        //                //var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                        //                //DT_VisaDetails DT_OBJ = (DT_VisaDetails)obj;
                        //                //DT_OBJ.VisaType_Id = blog.VisaType == null ? 0 : blog.VisaType.Id;
                        //                //DT_OBJ.Country_Id = blog.Country == null ? 0 : blog.Country.Id;
                        //                //db.Create(DT_OBJ);
                        //                //db.SaveChanges();
                        //            }
                        //            await db.SaveChangesAsync();
                        //            ts.Complete();

                        //            Msg.Add("  Record Updated");
                        //            return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.IssueAt, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //            //return Json(new Object[] { ESOBJ.Id, ESOBJ.IssueAt, "Record Updated", JsonRequestBehavior.AllowGet });

                        //        }
                        //    }
                        //    catch (DbUpdateConcurrencyException ex)
                        //    {
                        //        var entry = ex.Entries.Single();
                        //        var clientValues = (VisaDetails)entry.Entity;
                        //        var databaseEntry = entry.GetDatabaseValues();
                        //        if (databaseEntry == null)
                        //        {
                        //            Msg.Add(" Unable to save changes. The record was deleted by another user.");
                        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //            //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                        //        }
                        //        else
                        //        {
                        //            var databaseValues = (VisaDetails)databaseEntry.ToObject();
                        //            ESOBJ.RowVersion = databaseValues.RowVersion;

                        //        }
                        //    }
                        //    Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //    //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        //}

                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    VisaDetails blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                         blog = context.VisaDetails.Where(e => e.Id == AID).Include(e => e.VisaType).Include(e => e.Country)
                                        .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    ESOBJ.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    var m1 = db.VisaDetails.Where(e => e.Id == AID).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.VisaDetails.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    //int a = EditS(Category, PromoPolicy, data, c, c.DBTrack);
                                    
                                    
                                    var CurCorp = db.VisaDetails.Find(AID);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        VisaDetails corp = new VisaDetails()
                                        {
                                            IssueAt = ESOBJ.IssueAt,
                                            IssueDate = ESOBJ.IssueDate,
                                            ValidDate = ESOBJ.ValidDate,
                                            Id = AID,
                                            DBTrack = ESOBJ.DBTrack
                                        };

                                        db.VisaDetails.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // return 1;

                                        using (var context = new DataBaseContext())
                                        {
                                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                            DT_VisaDetails DT_Corp = (DT_VisaDetails)obj;
                                            DT_Corp.VisaType_Id = blog.VisaType == null ? 0 : blog.VisaType.Id;
                                            DT_Corp.Country_Id = blog.Country == null ? 0 : blog.Country.Id;

                                            db.Create(DT_Corp);
                                            db.SaveChanges();
                                        }
                                        db.SaveChanges();
                                        await db.SaveChangesAsync();
                                        ts.Complete();
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                                    ESOBJ.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }



                    }
                    else
                    {
                        //    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        //    {
                        //        VisaDetails Old_OBJ = db.VisaDetails.Include(e => e.VisaType)
                        //                                            .Include(e => e.Country)
                        //                                           .Where(e => e.Id == data).SingleOrDefault();

                        //        VisaDetails Curr_OBJ = ESOBJ;
                        //        ESOBJ.DBTrack = new DBTrack
                        //        {
                        //            CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
                        //            CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
                        //            Action = "M",
                        //            IsModified = Old_OBJ.DBTrack.IsModified == true ? true : false,
                        //            //ModifiedBy = SessionManager.UserName,
                        //            //ModifiedOn = DateTime.Now
                        //        };
                        //        Old_OBJ.DBTrack = ESOBJ.DBTrack;

                        //        db.Entry(Old_OBJ).State = System.Data.Entity.EntityState.Modified;
                        //        db.SaveChanges();
                        //        using (var context = new DataBaseContext())
                        //        {
                        //            //DBTrackFile.DBTrackSave("Core/P2b.Global", "M", Old_OBJ, Curr_OBJ, "VisaDetails", ESOBJ.DBTrack);
                        //        }

                        //        ts.Complete();
                        //        return Json(new Object[] { Old_OBJ.Id, ESOBJ.IssueDate, "Record Updated", JsonRequestBehavior.AllowGet });
                        //    }

                        //}
                        //return View();


                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            VisaDetails blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            VisaDetails Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.VisaDetails.Where(e => e.Id == AID).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            ESOBJ.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            VisaDetails corp = new VisaDetails()
                            {
                                IssueAt = ESOBJ.IssueAt,
                                IssueDate = ESOBJ.IssueDate,
                                ValidDate = ESOBJ.ValidDate,
                                VisaType = ESOBJ.VisaType,
                                Country = ESOBJ.Country,
                                Id = AID,
                                DBTrack = ESOBJ.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                //var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "VisaDetails", ESOBJ.DBTrack);
                                //Old_Corp = context.VisaDetails.Where(e => e.Id == AID).Include(e => e.VisaType).Include(e => e.Country)
                                    //.SingleOrDefault();
                                var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                DT_VisaDetails DT_Corp = (DT_VisaDetails)obj;
                               // DT_Corp.VisaType_Id = DBTrackFile.ValCompare(Old_Corp.VisaType, ESOBJ.VisaType); //Old_Corp.VenuType == c.VenuType ? 0 : Old_Corp.VenuType == null && c.VenuType != null ? c.VenuType.Id : Old_Corp.VenuType.Id;
                               // DT_Corp.Country_Id = DBTrackFile.ValCompare(Old_Corp.Country, ESOBJ.Country); //Old_Corp.LvHeadObj == c.LvHeadObj ? 0 : Old_Corp.LvHeadObj == null && c.LvHeadObj != null ? c.LvHeadObj.Id : Old_Corp.LvHeadObj.Id;

                                DT_Corp.VisaType_Id = blog.VisaType == null ? 0 : blog.VisaType.Id;
                                DT_Corp.Country_Id = blog.Country == null ? 0 : blog.Country.Id;
                                
                                db.Create(DT_Corp);
                            }
                            blog.DBTrack = ESOBJ.DBTrack;
                            db.VisaDetails.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, data = ESOBJ.Id.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { blog.Id, ESOBJ.Id, "Record Updated", JsonRequestBehavior.AllowGet });
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
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save Authorized data
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //VisaDetails corp = db.VisaDetails.Find(auth_id);
                            //VisaDetails corp = db.VisaDetails.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                            VisaDetails corp = db.VisaDetails.Include(e => e.Country)
                                .Include(e => e.VisaType).FirstOrDefault(e => e.Id == auth_id);

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

                            db.VisaDetails.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_VisaDetails DT_OBJ = (DT_VisaDetails)rtn_Obj;
                            //DT_OBJ.Address_Id = OBJ.Address == null ? 0 : OBJ.Address.Id;
                            DT_OBJ.VisaType_Id = corp.VisaType == null ? 0 : corp.VisaType.Id;
                            DT_OBJ.Country_Id = corp.Country == null ? 0 : corp.Country.Id;
                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();


                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "M", corp, null, "VisaDetails", corp.DBTrack);
                            //}
                            ts.Complete();
                            Msg.Add("  Record Authorized");
                            return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.IssueDate.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { corp.Id, corp.IssueDate, "Record Authorized", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        VisaDetails Old_OBJ = db.VisaDetails.Include(e => e.VisaType)
                                                          .Include(e => e.Country)
                                                          .Include(e => e.VisaType).Where(e => e.Id == auth_id).SingleOrDefault();



                        DT_VisaDetails Curr_OBJ = db.DT_VisaDetails
                            //.Include(e => e.VisaType)
                            //                    .Include(e => e.Country)
                            //                      .Include(e => e.VisaType)
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        VisaDetails corp = new VisaDetails();
                        string Corp = Curr_OBJ.VisaType_Id == null ? null : Curr_OBJ.VisaType_Id.ToString();
                        string Addrs = Curr_OBJ.Country_Id == null ? null : Curr_OBJ.Country_Id.ToString();
                        string VisaType = Curr_OBJ.VisaType_Id == null ? null : Curr_OBJ.VisaType_Id.ToString();
                        corp.IssueDate = Curr_OBJ.IssueDate == null ? Old_OBJ.IssueDate : Curr_OBJ.IssueDate;
                        corp.IssueAt = Curr_OBJ.IssueAt == null ? Old_OBJ.IssueAt : Curr_OBJ.IssueAt;
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
                                        CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
                                        CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = Old_OBJ.DBTrack.ModifiedBy == null ? null : Old_OBJ.DBTrack.ModifiedBy,
                                        ModifiedOn = Old_OBJ.DBTrack.ModifiedOn == null ? null : Old_OBJ.DBTrack.ModifiedOn,
                                        AuthorizedBy = SessionManager.UserName,
                                        AuthorizedOn = DateTime.Now,
                                        IsModified = false
                                    };

                                    int a = EditS(Corp, Addrs, auth_id, corp, corp.DBTrack);
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Authorized");
                                    return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { corp.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (VisaDetails)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (VisaDetails)databaseEntry.ToObject();
                                    corp.RowVersion = databaseValues.RowVersion;
                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //VisaDetails corp = db.VisaDetails.Find(auth_id);
                            VisaDetails corp = db.VisaDetails.AsNoTracking().Include(e => e.Country)
                                                                        .Include(e => e.VisaType)
                                                                         .FirstOrDefault(e => e.Id == auth_id);

                            Country add = corp.Country;
                            LookupValue val = corp.VisaType;

                            corp.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                                CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                                CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                                IsModified = corp.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.VisaDetails.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_VisaDetails DT_OBJ = (DT_VisaDetails)rtn_Obj;
                            DT_OBJ.VisaType_Id = corp.VisaType == null ? 0 : corp.VisaType.Id;
                            DT_OBJ.Country_Id = corp.Country == null ? 0 : corp.Country.Id;
                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();
                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorized ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record Authorized", JsonRequestBehavior.AllowGet });
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

        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        IEnumerable<VisaDetails> VisaDetails = null;
        //        if (gp.IsAutho == true)
        //        {
        //            VisaDetails = db.VisaDetails.Include(e => e.VisaType).Include(e=>e.Country).Where(e => e.DBTrack.IsModified == true).ToList();
        //        }
        //        else
        //        {
        //            VisaDetails = db.VisaDetails.Include(e => e.VisaType).Include(e => e.Country).ToList();
        //        }

        //        IEnumerable<VisaDetails> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = VisaDetails;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.IssueAt, a.IssueDate, a.ValidDate }).Where((e => (e.Id.ToString() == gp.searchString) || (e.IssueAt.ToLower() == gp.searchString.ToLower()) || (e.IssueDate.ToString() == gp.searchString.ToLower()) || (e.ValidDate.ToString() == gp.searchString.ToLower()))).ToList();
        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.IssueAt), Convert.ToString(a.IssueDate), Convert.ToString(a.VisaType) != null ? Convert.ToString(a.VisaType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.IssueAt, a.IssueDate, a.ValidDate }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = VisaDetails;
        //            Func<VisaDetails, int> orderfuc = (c =>
        //                                                       gp.sidx == "Id" ? c.Id : 0);
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.IssueAt), Convert.ToString(a.IssueDate), Convert.ToString(a.ValidDate) }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.IssueAt), Convert.ToString(a.IssueDate), Convert.ToString(a.ValidDate) }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.IssueAt, a.IssueDate, Convert.ToString(a.ValidDate) }).ToList();
        //            }
        //            totalRecords = VisaDetails.Count();
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
        //            total = totalPages
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
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
                IEnumerable<Employee> VisaDetails = null;
                if (gp.IsAutho == true)
                {
                    VisaDetails = db.Employee.Include(e => e.VisaDetails).Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    VisaDetails = db.Employee.Include(q => q.VisaDetails).Include(q => q.EmpName).Where(q => q.VisaDetails.Count > 0).ToList();
                }

                IEnumerable<Employee> IE;

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = VisaDetails;
                    if (gp.searchOper.Equals("eq")) 
                    {
                        jsonData = IE.Where(e => 
                            (e.EmpCode.ToString().Contains(gp.searchString.ToString())) 
                               || (e.EmpName.FullNameFML.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                             ||  (e.Id.ToString().Contains(gp.searchString.ToString())))
                           .Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = VisaDetails;
                    Func<Employee, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.EmpName.FullNameFML.ToString() :
                                          "");
                    }



                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    totalRecords = VisaDetails.Count();
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


                var Passport = db.Employee.Include(e => e.VisaDetails).Include(e => e.VisaDetails.Select(q => q.Country))
                              .Include(e => e.VisaDetails.Select(q => q.VisaType))
                              .Where(e => e.Id == data).SingleOrDefault();
                int AID = Passport.VisaDetails.Select(s => s.Id).SingleOrDefault();
                VisaDetails VisaDetails = db.VisaDetails.Where(e => e.Id == AID).SingleOrDefault();
                try
                {

                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(VisaDetails).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }

                catch (DataException /* dex */)
                {
                    // return this.Json(new Object[] { "", "Unable to delete. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete1(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {


                    var Passport = db.Employee.Include(e => e.VisaDetails)
                                  .Include(e => e.VisaDetails.Select(q => q.Country))
                                  .Include(e => e.VisaDetails.Select(q => q.VisaType))
                                  .Where(e => e.Id == data).SingleOrDefault();


                    if (Passport.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = Passport.DBTrack.CreatedBy != null ? Passport.DBTrack.CreatedBy : null,
                                CreatedOn = Passport.DBTrack.CreatedOn != null ? Passport.DBTrack.CreatedOn : null,
                                IsModified = Passport.DBTrack.IsModified == true ? true : false
                            };
                            Passport.DBTrack = dbT;
                            db.Entry(Passport).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Passport.DBTrack);
                            DT_VisaDetails DT_OBJ = (DT_VisaDetails)rtn_Obj;
                            db.Create(DT_OBJ);

                            await db.SaveChangesAsync();
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

                            var lkValue = new HashSet<int>(Passport.VisaDetails.Select(e => e.Id));


                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = Passport.DBTrack.CreatedBy != null ? Passport.DBTrack.CreatedBy : null,
                                CreatedOn = Passport.DBTrack.CreatedOn != null ? Passport.DBTrack.CreatedOn : null,
                                IsModified = Passport.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };

                            db.Entry(Passport.VisaDetails).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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

        //public ActionResult GetContactDetLKDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.VisaType.Include(e => e.ContactNumbers).ToList();
        //        IEnumerable<VisaType> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.VisaType.ToList().Where(d => d.FullContactDetails.Contains(data));

        //        }
        //        else
        //        {
        //            var list1 = db.VisaDetails.ToList().Select(e => e.VisaType);
        //            //var list2 = fall.Except(list1);
        //            //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
        //            var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();
        //            //var result_1 = (from c in fall
        //            //                select new { c.Id, c.CorporateCode, c.CorporateName });
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullContactDetails }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    // return View();
        //}


        //public ActionResult GetAddressLKDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.Country.Include(e => e.Area).Include(e => e.City).Include(e => e.Country)
        //                             .Include(e => e.District).Include(e => e.State).Include(e => e.StateRegion)
        //                             .Include(e => e.Taluka).ToList();
        //        IEnumerable<Country> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.Country.ToList().Where(d => d.FullAddress.Contains(data));

        //        }
        //        else
        //        {
        //            var list1 = db.VisaDetails.Include(e => e.Country.Area).ToList().Select(e => e.Country);
        //            var list2 = fall.Except(list1);

        //            //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
        //            var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();
        //            //var result_1 = (from c in fall
        //            //                select new { c.Id, c.CorporateCode, c.CorporateName });
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullAddress }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    // return View();
        //}

        public int EditS(string VisaTypeLst, string CountryLst, int AID, VisaDetails ESOBJ, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (VisaTypeLst != null)
                {
                    if (VisaTypeLst != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(VisaTypeLst));
                        ESOBJ.VisaType = val;

                        var type = db.VisaDetails.Include(e => e.VisaType).Where(e => e.Id == AID).SingleOrDefault();
                        IList<VisaDetails> typedetails = null;
                        if (type.VisaType != null)
                        {
                            typedetails = db.VisaDetails.Where(x => x.VisaType.Id == type.VisaType.Id && x.Id == AID).ToList();
                        }
                        else
                        {
                            typedetails = db.VisaDetails.Where(x => x.Id == AID).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.VisaType = ESOBJ.VisaType;
                            db.VisaDetails.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }

                if (CountryLst != null)
                {
                    if (CountryLst != "")
                    {
                        var val = db.Country.Find(int.Parse(CountryLst));
                        ESOBJ.Country = val;

                        var add = db.VisaDetails.Include(e => e.Country).Where(e => e.Id == AID).SingleOrDefault();
                        IList<VisaDetails> addressdetails = null;
                        if (add.Country != null)
                        {
                            addressdetails = db.VisaDetails.Where(x => x.Country.Id == add.Country.Id && x.Id == AID).ToList();
                        }
                        else
                        {
                            addressdetails = db.VisaDetails.Where(x => x.Id == AID).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.Country = ESOBJ.Country;
                                db.VisaDetails.Attach(s);
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
                    var addressdetails = db.VisaDetails.Include(e => e.Country).Where(x => x.Id == AID).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.Country = null;
                        db.VisaDetails.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }



                var CurCorp = db.VisaDetails.Find(AID);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    ESOBJ.DBTrack = dbT;
                    VisaDetails corp = new VisaDetails()
                    {
                        IssueAt = ESOBJ.IssueAt,
                        IssueDate = ESOBJ.IssueDate,
                        ValidDate = ESOBJ.ValidDate,
                        Id = AID,
                        DBTrack = ESOBJ.DBTrack
                    };


                    db.VisaDetails.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }

        ////public 

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

    }


}
