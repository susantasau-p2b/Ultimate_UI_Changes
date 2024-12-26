using EssPortal.App_Start;
using EssPortal.Models;
using EssPortal.Security;
using P2b.Global;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web.Mvc;

namespace EssPortal.Controllers
{
    public class VisaDetailsController : Controller
    {
        DataBaseContext db = new DataBaseContext();
        // GET: VisaDetails
        public ActionResult Index()
        {
            return View("~/Views/Shared/_VisaDetails/Index.cshtml"); //wrong
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/_VisaDetails.cshtml");
        }

        public ActionResult view_partial()
        {
            return View("~/Views/Shared/_VisaDetailsView.cshtml");
        }
        [HttpPost]
        public ActionResult Edit(string data)
        {
            if (data != "0")
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var Passport = db.Employee.Include(e => e.VisaDetails).Include(e => e.VisaDetails.Select(q => q.Country))
                    .Include(e => e.VisaDetails.Select(q => q.VisaType))
                                    .Where(e => e.Id == Emp).SingleOrDefault();
                int AID = Passport.VisaDetails.Select(s => s.Id).SingleOrDefault();
                var r = (from e in Passport.VisaDetails
                         select new
                         {
                             Id = e.Id,
                             IssueAt = e.IssueAt != null ? e.IssueAt : "",
                             IssueDate = e.IssueDate != null ? e.IssueDate.Value.ToShortDateString() : "",
                             ValidDate = e.ValidDate != null ? e.ValidDate.Value.ToShortDateString() : "",
                             VisaType_Id = e.VisaType.Id > 0 ? e.VisaType.Id : 0,
                             Country_Id = e.Country != null ? e.Country.Id : 0,
                             Action = e.DBTrack.Action
                         }).Distinct();

                var a = "";

                var Old_Data = db.DT_VisaDetails
                    // .Include(e => e.VisaType)
                    //  .Include(e => e.Country)                 
                      .Where(e => e.Orig_Id == id && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
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
            return View();
        }
        public ActionResult PopulateDropDownList(string data, string data2)
        {
            var qurey = db.Country.ToList();
            var selected = (Object)null;
            if (data2 != "" && data != "0" && data2 != "0")
            {
                selected = Convert.ToInt32(data2);
            }

            SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
            return Json(s, JsonRequestBehavior.AllowGet);
        }


        public ActionResult AddOrEdit(VisaDetails lkval, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Add = form["Add"] != null && form["Add"] != "" ? Convert.ToBoolean(form["Add"]) : true;
                var Id = form["auth_id"] != null && form["auth_id"] != "" ? Convert.ToInt32(form["auth_id"]) : 0;
                if (Add == true)
                {
                    //Add
                    var returnobj = Create(lkval, form);

                    return Json(returnobj, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //Edit
                    var returnobj = EditSave(lkval, Id, form);
                    return Json(returnobj, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public object Create(VisaDetails COBJ, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            try
            {
                string VisaType = form["VisaTypelist"] == "0" ? "" : form["VisaTypelist"];
                string Country = form["CountryList"] == "0" ? "" : form["CountryList"];
                int Emp = Convert.ToInt32(SessionManager.EmpId);

                if (VisaType != null)
                {
                    if (VisaType != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(VisaType));
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
                if (Emp != 0)
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
                                    return new { status = true, responseText = Msg };
                                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }

                        }
                        if (COBJ.IssueDate > COBJ.ValidDate)
                        {
                            Msg.Add("Issue Date Should be less than Valid Date.  ");
                            return new { status = true, responseText = Msg };
                            // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return this.Json(new Object[] { null, null, "Issue Date Should be less than Expiry Date.", JsonRequestBehavior.AllowGet });
                        }

                        DateTime current = DateTime.Now;
                        if (COBJ.IssueDate > current)
                        {

                            Msg.Add("  Issue Date Should be less than Current Date.  ");
                            return new { status = true, responseText = Msg };
                            //  return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, COBJ.DBTrack);
                            //DT_VisaDetails DT_OBJ = (DT_VisaDetails)rtn_Obj;
                            //db.Create(DT_OBJ);
                            //db.SaveChanges();

                            var a = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, COBJ.DBTrack);
                            db.SaveChanges();
                            DT_VisaDetails DT_Corp = (DT_VisaDetails)a;
                            DT_Corp.Orig_Id = VisaDetails.Id;
                            // DT_Corp.SkillType_Id = lkval.SkillType == null ? 0 : lkval.SkillType.Id;
                            db.Create(DT_Corp);
                            db.SaveChanges();

                            List<VisaDetails> empVisaDetails = new List<VisaDetails>();
                            empVisaDetails.Add(VisaDetails);
                            empdata.VisaDetails = empVisaDetails;
                            db.Employee.Attach(empdata);
                            db.Entry(empdata).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return new { status = true, responseText = Msg };
                            //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        public Object EditSave(VisaDetails ESOBJ, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            try
            {
                int Emp = Convert.ToInt32(SessionManager.EmpId);

                string VisaTypelst = form["VisaTypelist"] == "0" ? "" : form["VisaTypelist"];
                string Countrylst = form["CountryList"] == "0" ? "" : form["CountryList"];

                var Passport = db.Employee.Include(e => e.VisaDetails).Include(e => e.VisaDetails.Select(q => q.Country))
                             .Include(e => e.VisaDetails.Select(q => q.VisaType))
                       .Where(e => e.Id == Emp).SingleOrDefault();
                int AID = Passport.VisaDetails.Select(s => s.Id).SingleOrDefault();
                //   int AIDD = int.Parse(AID);
                bool Auth = form["autho_allow"] == "true" ? true : false;


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
                            if (ESOBJ.IssueDate > ESOBJ.ValidDate)
                            {
                                Msg.Add("Issue Date Should be less than Valid Date.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { null, null, "Issue Date Should be less than Expiry Date.", JsonRequestBehavior.AllowGet });
                            }

                            //DbContextTransaction transaction = db.Database.BeginTransaction();

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

                                int a = EditS(VisaTypelst, Countrylst, AID, ESOBJ, ESOBJ.DBTrack);

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
                                //  await db.SaveChangesAsync();
                                ts.Complete();

                                Msg.Add("  Record Updated successfully  ");
                                return new { status = true, responseText = Msg };
                                //return new { Id = ESOBJ.Id, Val = ESOBJ.IssueAt, status = true, responseText = Msg };
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
                            var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "VisaDetails", ESOBJ.DBTrack);
                            Old_Corp = context.VisaDetails.Where(e => e.Id == AID).Include(e => e.VisaType).Include(e => e.Country)
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
                        return new { Id = ESOBJ.Id, Val = ESOBJ.IssueAt, status = true, responseText = Msg };
                        // return Json(new Utility.JsonReturnClass { Id = blog.Id, data = ESOBJ.Id.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

        public int EditS(string VisaTypeLst, string CountryLst, int AID, VisaDetails ESOBJ, DBTrack dbT)
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

        public class returnDataClass
        {
            public Int32 EmpId { get; set; }
            public String val { get; set; }
            public List<string> vals { get; set; }
            public Int32 EmpLVid { get; set; }
            public bool Id3 { get; set; }
            public Int32 LvHead_Id { get; set; }
        }

        public class ChildGetLvNewReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }

        }

        public class GetLvNewReqClass
        {
            public string Emp { get; set; }
            public string VisaType { get; set; }
            public string IssueDate { get; set; }
            public string ValidDate { get; set; }
            public string IssueAt { get; set; }
            public string Country { get; set; }

            public ChildGetLvNewReqClass RowData { get; set; }
        }

        public class FormCollectionClass
        {
            public Int32 maintableid { get; set; }
            public string Approval { get; set; }
            public string ReasonApproval { get; set; }
        }

        //public ActionResult GetMyEmpVisa()
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var Emp = Convert.ToInt32(SessionManager.EmpId);
        //        var qurey = db.Employee.Include(e => e.VisaDetails).Include(e => e.VisaDetails.Select(q => q.Country)).Include(e => e.VisaDetails.Select(q => q.VisaType))
        //         .Where(e => e.Id == Emp && e.VisaDetails.Count > 0).SingleOrDefault();

        //        var ListreturnDataClass = new List<returnDataClass>();
        //        if (qurey != null && qurey.VisaDetails.Count > 0)
        //        {
        //            foreach (var item in qurey.VisaDetails)
        //            {
        //                var VisaType = item.VisaType != null ? item.VisaType.LookupVal.ToString() : "";
        //                var IssueDate = item != null ? item.IssueDate.Value.ToShortDateString() : "";
        //                var ValidDate = item != null ? item.ValidDate.Value.ToShortDateString() : "";
        //                var IssueAt = item != null ? item.IssueAt : "";
        //                var Country = item.Country != null ? item.Country.FullDetails : "";
        //                ListreturnDataClass.Add(new returnDataClass
        //                {
        //                    EmpId = item.Id,
        //                    val =
        //                    "VisaType :" + VisaType +
        //                    ", IssueDate :" + IssueDate +
        //                    ", ValidDate :" + ValidDate +
        //                    ", IssueAt :" + IssueAt +
        //                    ", Country :" + Country +
        //                    ""
        //                });
        //            }
        //        }
        //        if (ListreturnDataClass != null && ListreturnDataClass.Count > 0)
        //        {
        //            return Json(new { status = true, data = ListreturnDataClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { status = false, data = ListreturnDataClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}

        public ActionResult GetMyEmpVisa()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);
                var db_data = db.Employee.Include(e => e.VisaDetails)
                    .Include(e => e.VisaDetails.Select(q => q.Country)).Include(e => e.VisaDetails.Select(q => q.VisaType))
                 .Where(e => e.Id == Id).SingleOrDefault();

                if (db_data != null)
                {
                    List<GetLvNewReqClass> returndata = new List<GetLvNewReqClass>();
                    returndata.Add(new GetLvNewReqClass
                    {
                        VisaType = " VisaType ",
                        IssueDate = " IssueDate ",
                        ValidDate = " ValidDate ",
                        IssueAt = " IssueAt ",
                        Country = " Country ",

                    });
                    if (db_data.VisaDetails.Count > 0)
                    {

                        foreach (var item in db_data.VisaDetails.OrderByDescending(a => a.Id).ToList())
                        {

                            var Country = item.Country.FullDetails;
                            returndata.Add(new GetLvNewReqClass
                            {
                                RowData = new ChildGetLvNewReqClass
                                {
                                    LvNewReq = item.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString(),
                                    LvHead_Id = "",
                                },
                                VisaType = item.VisaType == null ? "" : item.VisaType.LookupVal.ToString(),
                                IssueDate = item.IssueDate != null ? item.IssueDate.Value.ToShortDateString() : null,
                                ValidDate = item.ValidDate != null ? item.ValidDate.Value.ToShortDateString() : null,
                                IssueAt = item.IssueAt == null ? "" : item.IssueAt.ToString(),
                                Country = Country == null ? "" : Country,

                            });
                        }
                    }
                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult GetNewEmpVisaDetailsReq()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                if (EmpIds == null && EmpIds.Count == 0)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Emps = db.Employee
                    .Where(e => EmpIds.Contains(e.Id))
                    .Include(e => e.VisaDetails)
                    .Include(e => e.VisaDetails.Select(q => q.VisaType))
                    .Include(e => e.VisaDetails.Select(q => q.Country))
                    .Include(e => e.EmpName)
                    .ToList();
                var returnDataClass = new List<returnDataClass>();
                List<GetLvNewReqClass> returndata = new List<GetLvNewReqClass>();


                returndata.Add(new GetLvNewReqClass
                {
                    Emp = "Employee Name",
                    VisaType = "VisaType",
                    IssueDate = "IssueDate",
                    ValidDate = "ValidDate",
                    IssueAt = "IssueAt",
                    Country = "Country",
                });


                foreach (var item in Emps)
                {
                    if (item.VisaDetails.Count > 0)
                    {
                        foreach (var singleQalification in item.VisaDetails)
                        {
                            int QId = singleQalification.Id;
                            var dt_data = db.DT_VisaDetails.Where(e => e.Orig_Id == QId && e.DBTrack.IsAuthorized == 0 && e.DBTrack.TrClosed == false).OrderByDescending(e => e.Id).FirstOrDefault();
                            if (dt_data != null)
                            {

                                //returnDataClass.Add(new returnDataClass
                                //{
                                //    EmpId = dt_data.Id,
                                //    //EmpId = item.Id,
                                //    EmpLVid = QId,
                                //    val = "EmpCode :" + item.EmpCode + "EmpName :" + item.EmpName.FullNameFML + " " + dt_data.FullDetails
                                //});
                                returndata.Add(new GetLvNewReqClass
                                {
                                    RowData = new ChildGetLvNewReqClass
                                    {
                                        LvNewReq = QId.ToString(),
                                        EmpLVid = item.Id.ToString(),
                                        LvHead_Id = "",
                                    },
                                    Emp = item.EmpName.FullNameFML.ToString(),
                                    VisaType = singleQalification != null ? singleQalification.VisaType != null ? singleQalification.VisaType.LookupVal.ToString() : "" : "",
                                    IssueDate = dt_data.IssueDate != null ? dt_data.IssueDate.Value.ToShortDateString() : null,
                                    ValidDate = dt_data.ValidDate != null ? dt_data.ValidDate.Value.ToShortDateString() : null,
                                    IssueAt = dt_data.IssueAt == null ? "" : dt_data.IssueAt.ToString(),
                                    Country = singleQalification != null ? singleQalification.Country != null ? singleQalification.Country.FullDetails : "" : "",

                                });

                            }

                        }
                    }
                }


                if (returndata != null && returndata.Count > 0)
                {
                    return Json(new { status = true, data = returndata, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = returndata, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult GetEmpReqDataVisaDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[1]);
                var maintableid = Convert.ToInt32(ids[0]);
                var empfind = db.Employee.Include(e => e.VisaDetails)
                    .Where(e => e.VisaDetails.Any(a => a.Id == maintableid))
                    .Select(e => new
                    {
                        EmpName = e.EmpCode + " " + e.EmpName.FullNameFML
                    }).FirstOrDefault();
                var Qualifictiondata = db.VisaDetails.Where(e => e.Id == maintableid)
                    .Select(e => new
                    {
                        VisaType = e.VisaType != null ? e.VisaType : null,
                        IssueAt = e.IssueAt != null ? e.IssueAt : "",
                        IssueDate = e.IssueDate != null ? e.IssueDate : null,
                        ValidDate = e.ValidDate != null ? e.ValidDate : null,
                        Country = e.Country != null ? e.Country.Name : "",
                    }).SingleOrDefault();
                var oVar = db.DT_VisaDetails.Where(e => e.Id == id).SingleOrDefault();
                var list = new
                {
                    Emp = empfind.EmpName,
                    VisaType = Qualifictiondata.VisaType != null ? Qualifictiondata.VisaType.LookupVal.ToString() : "",
                    IssueDate = oVar != null ? oVar.IssueDate != null ? oVar.IssueDate.Value.ToShortDateString() : Qualifictiondata.IssueDate.Value.ToShortDateString() : Qualifictiondata.IssueDate.Value.ToShortDateString(),
                    ValidDate = oVar != null ? oVar.ValidDate != null ? oVar.ValidDate.Value.ToShortDateString() : Qualifictiondata.ValidDate.Value.ToShortDateString() : Qualifictiondata.ValidDate.Value.ToShortDateString(),
                    IssueAt = oVar != null ? oVar.IssueAt != null ? oVar.IssueAt : Qualifictiondata.IssueAt : Qualifictiondata.IssueAt,
                    Country = Qualifictiondata.Country != null ? Qualifictiondata.Country : "",
                };
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UpdateStatusVisaDetails(FormCollectionClass oFormCollectionClass, FormCollection form, String data)
        {
            var ids = Utility.StringIdsToListString(data);
            var qualificationid = Convert.ToInt32(ids[0]);
            var maintableid = Convert.ToInt32(ids[1]);
            string Approval = oFormCollectionClass.Approval == null ? "false" : oFormCollectionClass.Approval;

            string ReasonApproval = oFormCollectionClass.ReasonApproval == null ? null : oFormCollectionClass.ReasonApproval;
            bool SanctionRejected = false;
            bool HrRejected = false;
            using (DataBaseContext db = new DataBaseContext())
            {
                var dataDT_QualificationDetails = db.DT_VisaDetails.Where(e => e.Id == qualificationid).SingleOrDefault();
                if (dataDT_QualificationDetails != null)
                {
                    dataDT_QualificationDetails.DBTrack.ApproveBy = Utility.GetUserData().EmpCode;
                    dataDT_QualificationDetails.DBTrack.ApproveDate = DateTime.Now;
                    dataDT_QualificationDetails.DBTrack.ApprovedComment = ReasonApproval;
                    if (Convert.ToBoolean(Approval) == true)
                    {
                        dataDT_QualificationDetails.DBTrack.IsAuthorized = 1;
                    }
                    else
                    {
                        dataDT_QualificationDetails.DBTrack.IsAuthorized = 2;
                    }
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        dataDT_QualificationDetails.DBTrack.TrClosed = true;
                        db.Entry(dataDT_QualificationDetails).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(dataDT_QualificationDetails).State = System.Data.Entity.EntityState.Detached;
                        VisaDetails Old = db.VisaDetails
                         .Where(e => e.Id == qualificationid).SingleOrDefault();

                        //if didnt work replace maintableid to qualificationid 
                        DT_VisaDetails Curr = db.DT_VisaDetails
                                                    .Where(e => e.Id == qualificationid)
                                                    .SingleOrDefault();
                        Old.Id = qualificationid;
                        Old.ValidDate = Curr.ValidDate == null ? Old.ValidDate : Curr.ValidDate;
                        Old.IssueAt = Curr.IssueAt == null ? Old.IssueAt : Curr.IssueAt;
                        Old.IssueDate = Curr.IssueDate == null ? Old.IssueDate : Curr.IssueDate;

                        Old.DBTrack = new DBTrack
                        {
                            CreatedBy = Old.DBTrack.CreatedBy == null ? null : Old.DBTrack.CreatedBy,
                            CreatedOn = Old.DBTrack.CreatedOn == null ? null : Old.DBTrack.CreatedOn,
                            Action = "M",
                            ModifiedBy = Old.DBTrack.ModifiedBy == null ? null : Old.DBTrack.ModifiedBy,
                            ModifiedOn = Old.DBTrack.ModifiedOn == null ? null : Old.DBTrack.ModifiedOn,
                            AuthorizedBy = Curr.DBTrack.ApproveBy,
                            AuthorizedOn = DateTime.Now,
                            IsModified = false,
                            ApproveBy = Curr.DBTrack.ApproveBy,
                            ApproveDate = Curr.DBTrack.ApproveDate,
                            ApprovedComment = Curr.DBTrack.ApprovedComment,
                            IsAuthorized = Curr.DBTrack.IsAuthorized,
                            TrClosed = true,
                        };
                        db.VisaDetails.Attach(Old);
                        db.Entry(Old).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(Old).State = System.Data.Entity.EntityState.Detached;

                        ts.Complete();
                        return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new Utility.JsonClass { status = false, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

    }
}