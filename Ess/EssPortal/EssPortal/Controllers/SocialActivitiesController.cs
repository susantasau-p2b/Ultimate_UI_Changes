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
    public class SocialActivitiesController : Controller
    {
        // GET: SocialActivities
        public ActionResult Index()
        {
            return View("~/Views/Shared/_SocialActivities.cshtml");
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/_SocialActivities.cshtml");
        }

        public ActionResult view_partial()
        {
            return View("~/Views/Shared/_SocialActivitiesView.cshtml");
        }
        DataBaseContext db = new DataBaseContext();

        public ActionResult AddOrEdit(SocialActivities lkval, FormCollection form)
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

        public ActionResult Edit(string data)
        {
            if (data != "0")
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var Passport = db.Employee.Include(e => e.EmpSocialInfo).Include(e => e.EmpSocialInfo.SocialActivities)
                                    .Where(e => e.Id == Emp).SingleOrDefault();
                var r = (from e in Passport.EmpSocialInfo.SocialActivities
                         select new
                         {
                             Id = e.Id,
                             InstituteName = e.InstituteName != null ? e.InstituteName : "",
                             FromPeriod = e.FromPeriod != null ? e.FromPeriod.Value.ToShortDateString() : "",
                             ToPeriod = e.ToPeriod != null ? e.ToPeriod.Value.ToShortDateString() : "",
                             PostHeld = e.PostHeld != null ? e.PostHeld : "",
                             Action = e.DBTrack.Action
                         }).Distinct();


                //var Old_Data = db.DT_PassportDetails
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,
                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var EOBJ = db.SocialActivities.Find(id);
                TempData["RowVersion"] = EOBJ.RowVersion;
                var Auth = EOBJ.DBTrack.IsModified;
                return Json(new Object[] { r, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
            return View();
        }

        [HttpPost]
        public Object Create(SocialActivities OBJ, FormCollection form) //Create submit
        {

            List<string> Msg = new List<string>();
            try
            {

                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {

                        if (OBJ.FromPeriod > OBJ.ToPeriod)
                        {

                            Msg.Add("  From Period should be less than To Period.  ");
                            return new { status = true, responseText = Msg };
                            //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }

                        if (db.SocialActivities.Any(o => o.InstituteName == OBJ.InstituteName))
                        {
                            Msg.Add("SocialActivities Already Exists. ");
                            return Json(new { status = false, valid = true, responseText = Msg });
                            //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        OBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };
                        SocialActivities COBJ = new SocialActivities()
                        {
                            Id = OBJ.Id,
                            FromPeriod = OBJ.FromPeriod,
                            PostHeld = OBJ.PostHeld,
                            InstituteName = OBJ.InstituteName,
                            ToPeriod = OBJ.ToPeriod,
                            DBTrack = OBJ.DBTrack
                        };
                        try
                        {
                            db.SocialActivities.Add(COBJ);
                            var a = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, COBJ.DBTrack);
                            db.SaveChanges();
                            DT_SocialActivities DT_Corp = (DT_SocialActivities)a;
                            DT_Corp.Orig_Id = COBJ.Id;
                            //// DT_Corp.SkillType_Id = lkval.SkillType == null ? 0 : lkval.SkillType.Id;
                            db.Create(DT_Corp);
                            db.SaveChanges();
                            var empid = Convert.ToInt32(SessionManager.EmpId);
                            var EmpAcedemicDataChk = db.Employee.Include(e => e.EmpSocialInfo)
                                .Include(e => e.EmpSocialInfo.SocialActivities)
                                .Where(e => e.Id == empid).SingleOrDefault();
                            if (EmpAcedemicDataChk != null && EmpAcedemicDataChk.EmpAcademicInfo != null)
                            {
                                if (EmpAcedemicDataChk.EmpSocialInfo.SocialActivities != null && EmpAcedemicDataChk.EmpSocialInfo.SocialActivities.Count > 0)
                                {
                                    EmpAcedemicDataChk.EmpSocialInfo.SocialActivities.Add(COBJ);
                                }
                                else
                                {
                                    EmpAcedemicDataChk.EmpSocialInfo.SocialActivities = new List<SocialActivities> { COBJ };
                                }
                            }
                            else
                            {
                                var oEmpAcademicInfo = new EmpSocialInfo();
                                oEmpAcademicInfo.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                oEmpAcademicInfo.SocialActivities = new List<SocialActivities> { COBJ };
                                EmpAcedemicDataChk.EmpSocialInfo = oEmpAcademicInfo;
                            }
                            db.Entry(EmpAcedemicDataChk).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return new { status = true, responseText = Msg };
                            //   return Json(new { status = true, valid = false, responseText = Msg });
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = OBJ.Id });
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
                    Msg.Add(errorMsg); return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        public Object EditSave(SocialActivities c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            try
            {
                int auth_id = form["auth_id"] != null ? Convert.ToInt32(form["auth_id"]) : 0;
                var db_data = db.SocialActivities.Where(e => e.Id == auth_id).SingleOrDefault();
                int Emp = Convert.ToInt32(SessionManager.EmpId);
                string Qual = form["Qualificationlist"] == "0" ? "" : form["Qualificationlist"];
                bool Auth = form["autho_allow"] == "true" ? true : false;

                //string Category = form["SkillTypelist"] == "0" ? "" : form["SkillTypelist"];

                //if (Category != null)
                //{
                //    if (Category != "")
                //    {
                //        var val = db.LookupValue.Find(int.Parse(Category));
                //        c.SkillType = val;
                //    }
                //}
                if (Auth == false)
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                db.SocialActivities.Attach(db_data);
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = db_data.RowVersion;
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                var Curr_Lookup = db.SocialActivities.Find(auth_id);
                                TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    SocialActivities blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.SocialActivities.Where(e => e.Id == auth_id)
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
                                    SocialActivities lk = new SocialActivities
                                    {
                                        Id = auth_id,
                                        FromPeriod = c.FromPeriod,
                                        PostHeld = c.PostHeld,
                                        InstituteName = c.InstituteName,
                                        ToPeriod = c.ToPeriod,
                                        DBTrack = c.DBTrack
                                    };

                                    db.SocialActivities.Attach(lk);
                                    db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                    db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    using (var context = new DataBaseContext())
                                    {
                                        db.SaveChanges();
                                    }
                                    //        await db.SaveChangesAsync();
                                    ts.Complete();

                                    //Msg.Add("  Record Updated");
                                    //return new { Id = lk.Id, Val = lk.FullDetails, status = true, responseText = Msg };
                                    Msg.Add("  Record Updated successfully  ");
                                    return new { status = true, responseText = Msg };
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (SocialActivities)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                return new { status = true, responseText = Msg };
                                //   return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (SocialActivities)databaseEntry.ToObject();
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

                        SocialActivities blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        SocialActivities Old_Corp = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.SocialActivities.Where(e => e.Id == auth_id).SingleOrDefault();
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
                        SocialActivities qualificationDetails = new SocialActivities()
                        {


                            Id = auth_id,
                            FromPeriod = c.FromPeriod,
                            PostHeld = c.PostHeld,
                            InstituteName = c.InstituteName,
                            ToPeriod = c.ToPeriod,
                            DBTrack = c.DBTrack
                        };

                        using (var context = new DataBaseContext())
                        {
                            var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, qualificationDetails, "SocialActivities", c.DBTrack);
                            // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                            Old_Corp = context.SocialActivities.Where(e => e.Id == auth_id)
                            .SingleOrDefault();
                            DT_SocialActivities DT_Corp = (DT_SocialActivities)obj;
                            //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                            //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                            //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                            db.Create(DT_Corp);
                            //db.SaveChanges();
                        }
                        blog.DBTrack = c.DBTrack;
                        db.SocialActivities.Attach(blog);
                        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];

                        db.SaveChanges();
                        ts.Complete();

                        Msg.Add("  Record Updated");
                        return new { Id = blog.Id, Val = c.FullDetails, status = true, responseText = Msg };
                        //  return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new Object[] { blog.Id, c.Institute, "Record Updated", JsonRequestBehavior.AllowGet });
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

        public class FormCollectionClass
        {
            public Int32 maintableid { get; set; }
            public string Approval { get; set; }
            public string ReasonApproval { get; set; }
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
            public string FromPeriod { get; set; }
            public string ToPeriod { get; set; }
            public string InstituteName { get; set; }
            public string PostHeld { get; set; }

            public ChildGetLvNewReqClass RowData { get; set; }
        }

        //public ActionResult GetMyEmpSocialActivities()
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var Emp = Convert.ToInt32(SessionManager.EmpId);
        //        var qurey = db.Employee.Include(e => e.EmpSocialInfo)
        //         .Include(e => e.EmpSocialInfo.SocialActivities)
        //         .Where(e => e.Id == Emp && e.EmpSocialInfo.SocialActivities.Count > 0).SingleOrDefault();

        //        var ListreturnDataClass = new List<returnDataClass>();
        //        if (qurey != null && qurey.EmpSocialInfo.SocialActivities.Count > 0)
        //        {
        //            foreach (var item in qurey.EmpSocialInfo.SocialActivities)
        //            {
        //                var FromPeriod = item != null ? item.FromPeriod.Value.ToShortDateString() : null;
        //                var ToPeriod = item != null ? item.ToPeriod.Value.ToShortDateString() : null;
        //                var InstituteName = item != null ? item.InstituteName : null;
        //                var PostHeld = item != null ? item.PostHeld : null;
        //                ListreturnDataClass.Add(new returnDataClass
        //                {
        //                    EmpId = item.Id,
        //                    val =
        //                    "FromPeriod :" + FromPeriod +
        //                    ", ToPeriod :" + ToPeriod +
        //                    ", InstituteName :" + InstituteName +
        //                    ", PostHeld :" + PostHeld +
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

        public ActionResult GetMyEmpSocialActivities()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);
                var db_data = db.Employee
                      .Where(e => e.Id == Id)
                .Include(e => e.EmpSocialInfo)
                 .Include(e => e.EmpSocialInfo.SocialActivities)
                     .SingleOrDefault();

                if (db_data != null)
                {
                    List<GetLvNewReqClass> returndata = new List<GetLvNewReqClass>();
                    returndata.Add(new GetLvNewReqClass
                    {
                        FromPeriod = "FromPeriod ",
                        ToPeriod = " ToPeriod ",
                        InstituteName = " InstituteName ",
                        PostHeld = " PostHeld ",

                    });
                    if (db_data.EmpSocialInfo != null)
                    {
                        if (db_data.EmpSocialInfo.SocialActivities != null && db_data.EmpSocialInfo.SocialActivities.Count > 0)
                        {
                            foreach (var item in db_data.EmpSocialInfo.SocialActivities.OrderByDescending(a => a.Id).ToList())
                            {


                                returndata.Add(new GetLvNewReqClass
                                {
                                    RowData = new ChildGetLvNewReqClass
                                    {
                                        LvNewReq = item.Id.ToString(),
                                        EmpLVid = db_data.Id.ToString(),
                                        LvHead_Id = "",
                                    },
                                    InstituteName = item.InstituteName == null ? "" : item.InstituteName.ToString(),
                                    PostHeld = item.PostHeld != null ? item.PostHeld.ToString() : null,
                                    FromPeriod = item.FromPeriod != null ? item.FromPeriod.Value.ToShortDateString() : null,
                                    ToPeriod = item.ToPeriod != null ? item.ToPeriod.Value.ToShortDateString() : null,
                                });
                            }
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

        //public ActionResult GetNewEmpSocialActivitiesReq()
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        LookupValue FuncModule = new LookupValue();
        //        if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
        //        {
        //            var id = Convert.ToString(Session["user-module"]);
        //            FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
        //        }
        //        if (FuncModule == null)
        //        {
        //            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        //        }
        //        var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
        //        if (EmpIds == null && EmpIds.Count == 0)
        //        {
        //            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        //        }
        //        var Emps = db.Employee
        //            .Where(e => EmpIds.Contains(e.Id))
        //            .Include(e => e.EmpSocialInfo)
        //            .Include(e => e.EmpSocialInfo.SocialActivities)
        //            .Include(e => e.EmpName)
        //            .ToList();
        //        var returnDataClass = new List<returnDataClass>();
        //        foreach (var item in Emps)
        //        {
        //            if (item.EmpSocialInfo != null && item.EmpSocialInfo.SocialActivities != null)
        //            {
        //                foreach (var singleQalification in item.EmpSocialInfo.SocialActivities)
        //                {
        //                    int QId = singleQalification.Id;
        //                    var dt_data = db.DT_SocialActivities.Where(e => e.Orig_Id == QId && e.DBTrack.IsAuthorized == 0 && e.DBTrack.TrClosed == false).OrderByDescending(e => e.Id).FirstOrDefault();
        //                    if (dt_data != null)
        //                    {
        //                        returnDataClass.Add(new returnDataClass
        //                        {
        //                            EmpId = dt_data.Id,
        //                            //EmpId = item.Id,
        //                            EmpLVid = QId,
        //                            val = "EmpCode :" + item.EmpCode + "EmpName :" + item.EmpName.FullNameFML + " " + dt_data.FullDetails
        //                        });
        //                    }

        //                }
        //            }
        //        }

        //        if (returnDataClass != null && returnDataClass.Count > 0)
        //        {
        //            return Json(new { status = true, data = returnDataClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { status = false, data = returnDataClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}

        public ActionResult GetNewEmpSocialActivitiesReq()
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
                    .Include(e => e.EmpSocialInfo)
                    .Include(e => e.EmpSocialInfo.SocialActivities)
                    .Include(e => e.EmpName)
                    .ToList();
                var returnDataClass = new List<returnDataClass>();
                List<GetLvNewReqClass> returndata = new List<GetLvNewReqClass>();


                returndata.Add(new GetLvNewReqClass
                {
                    Emp = "Employee Name",
                    InstituteName = "InstituteName",
                    PostHeld = "PostHeld",
                    FromPeriod = "FromPeriod",
                    ToPeriod = "ToPeriod",
                });


                foreach (var item in Emps)
                {
                    if (item.EmpSocialInfo != null && item.EmpSocialInfo.SocialActivities != null)
                    {
                        foreach (var singleQalification in item.EmpSocialInfo.SocialActivities)
                        {
                            int QId = singleQalification.Id;
                            var dt_data = db.DT_SocialActivities.Where(e => e.Orig_Id == QId && e.DBTrack.IsAuthorized == 0 && e.DBTrack.TrClosed == false).OrderByDescending(e => e.Id).FirstOrDefault();
                            if (dt_data != null)
                            {
                                returndata.Add(new GetLvNewReqClass
                                 {
                                     RowData = new ChildGetLvNewReqClass
                                     {
                                         LvNewReq = QId.ToString(),
                                         EmpLVid = item.Id.ToString(),
                                         LvHead_Id = "",
                                     },
                                     Emp = item.EmpName.FullNameFML.ToString(),
                                     FromPeriod = dt_data.FromPeriod == null ? "" : dt_data.FromPeriod.Value.ToShortDateString(),
                                     InstituteName = dt_data.InstituteName != null ? dt_data.InstituteName : "",
                                     PostHeld = dt_data.PostHeld != null ? dt_data.PostHeld : "",
                                     ToPeriod = dt_data.ToPeriod == null ? "" : dt_data.ToPeriod.Value.ToShortDateString(),
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

        public ActionResult GetEmpReqDataSocialActivities(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[1]);
                var maintableid = Convert.ToInt32(ids[0]);
                var empfind = db.Employee.Include(e => e.EmpSocialInfo)
                    .Where(e => e.EmpSocialInfo.SocialActivities.Any(a => a.Id == maintableid))
                    .Select(e => new
                    {
                        EmpName = e.EmpCode + " " + e.EmpName.FullNameFML
                    }).FirstOrDefault();
                var Qualifictiondata = db.SocialActivities.Where(e => e.Id == maintableid)
                    .Select(e => new
                    {
                        InstituteName = e.InstituteName != null ? e.InstituteName : "",
                        PostHeld = e.PostHeld != null ? e.PostHeld : "",
                        FromPeriod = e.FromPeriod != null ? e.FromPeriod : null,
                        ToPeriod = e.ToPeriod != null ? e.ToPeriod : null,
                    }).SingleOrDefault();
                var oVar = db.DT_SocialActivities.Where(e => e.Id == id).SingleOrDefault();
                var list = new
                {
                    Emp = empfind.EmpName,
                    InstituteName = oVar.InstituteName != null ? oVar.InstituteName : Qualifictiondata.InstituteName,
                    PostHeld = oVar.PostHeld != null ? oVar.PostHeld : Qualifictiondata.PostHeld,
                    FromPeriod = oVar.FromPeriod != null ? oVar.FromPeriod.Value.ToShortDateString() : Qualifictiondata.FromPeriod.Value.ToShortDateString(),
                    ToPeriod = oVar.ToPeriod != null ? oVar.ToPeriod.Value.ToShortDateString() : Qualifictiondata.ToPeriod.Value.ToShortDateString(),
                };
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UpdateStatusSocialActivities(FormCollectionClass oFormCollectionClass, FormCollection form, String data)
        {
            var ids = Utility.StringIdsToListString(data);
            var qualificationid = Convert.ToInt32(ids[1]);
            var maintableid = Convert.ToInt32(ids[0]);
            string Approval = oFormCollectionClass.Approval == null ? "false" : oFormCollectionClass.Approval;

            string ReasonApproval = oFormCollectionClass.ReasonApproval == null ? null : oFormCollectionClass.ReasonApproval;
            bool SanctionRejected = false;
            bool HrRejected = false;
            using (DataBaseContext db = new DataBaseContext())
            {
                var dataDT_QualificationDetails = db.DT_SocialActivities.Where(e => e.Id == qualificationid).SingleOrDefault();
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
                        SocialActivities Old = db.SocialActivities
                         .Where(e => e.Id == qualificationid).SingleOrDefault();

                        //if didnt work replace maintableid to qualificationid 

                        DT_SocialActivities Curr = db.DT_SocialActivities
                                                    .Where(e => e.Id == qualificationid)
                                                    .SingleOrDefault();
                        Old.Id = qualificationid;
                        Old.InstituteName = Curr.InstituteName == null ? Old.InstituteName : Curr.InstituteName;
                        Old.PostHeld = Curr.PostHeld == null ? Old.PostHeld : Curr.PostHeld;
                        Old.FromPeriod = Curr.FromPeriod == null ? Old.FromPeriod : Curr.FromPeriod;
                        Old.ToPeriod = Curr.ToPeriod == null ? Old.ToPeriod : Curr.ToPeriod;

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
                        db.SocialActivities.Attach(Old);
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

        public ActionResult UpdateStatusLang(FormCollectionClass oFormCollectionClass, FormCollection form, String data)
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
                var dataDT_QualificationDetails = db.DT_LanguageSkill.Where(e => e.Orig_Id == qualificationid).SingleOrDefault();
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
                        LanguageSkill Old = db.LanguageSkill
                          .Include(e => e.Language).Include(e => e.SkillType)
                         .Where(e => e.Id == qualificationid).SingleOrDefault();


                        DT_LanguageSkill Curr = db.DT_LanguageSkill
                                                    .Where(e => e.Id == qualificationid)
                                                    .SingleOrDefault();
                        Old.Id = qualificationid;


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
                        db.LanguageSkill.Attach(Old);
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