///
/// Created by Tanushri
///

using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class EmpSocialInfoController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /EmpSocialInfo/

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/EmpSocialInfo/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Core/_SocialActivities.cshtml");
        }

        private MultiSelectList GetLookupValues(List<int> selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<SocialActivities> lkval = new List<SocialActivities>();
                lkval = db.SocialActivities.ToList();
                return new MultiSelectList(lkval, "Id", "PostHeld", selectedValues);
            }
        }

        //public String checkCode(String data)
        //{
        //    var qurey = db.EmpSocialInfo.Any(e => e.SocialActivities == data);
        //    if (qurey == true)
        //    {
        //        return "1";
        //    }
        //    else
        //    {
        //        return "0";
        //    }
        //}

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(EmpSocialInfo COBJ, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Category = form["Categorylist"] == "0" ? "" : form["Categorylist"];
                    string Caste = form["Castelist"] == "0" ? "" : form["Castelist"];
                    string Religion = form["Religionlist"] == "0" ? "" : form["Religionlist"];
                    string SubCaste = form["SubCastelist"] == "0" ? "" : form["SubCastelist"];

                    string Emp1 = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];

                    int id1 = 0;
                    if (Emp1 != "")
                    {
                        id1 = Convert.ToInt32(Emp1);
                    }
                    else
                    {
                       
                        Msg.Add(" Kindly select employee");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    Employee OEmployee = null;



                    OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                    .Include(e => e.EmpOffInfo)
                                    .Where(r => r.Id == id1).SingleOrDefault();



                

                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "104").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Category)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(Category));
                            COBJ.Category = val;
                        }
                    }

                    if (Caste != null)
                    {
                        if (Caste != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "105").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Caste)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(Caste));
                            COBJ.Caste = val;
                        }
                    }
                    if (Religion != null)
                    {
                        if (Religion != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "103").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Religion)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(Religion));
                            COBJ.Religion = val;
                        }
                    }
                    if (SubCaste != null)
                    {
                        if (SubCaste != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "106").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(SubCaste)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(SubCaste));
                            COBJ.SubCaste = val;
                        }
                    }


                    COBJ.SocialActivities = null;
                    List<SocialActivities> OBJ = new List<SocialActivities>();
                    string Values = form["SocialActivitieslist"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.SocialActivities.Find(ca);
                            OBJ.Add(OBJ_val);
                            COBJ.SocialActivities = OBJ;
                        }
                    }

                    Employee EmpData;
                    if (OEmployee.Id != null && OEmployee.Id != 0)
                    {
                        EmpData = db.Employee.Find(OEmployee.Id);
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
                            var EmpSocialInfo1 = db.Employee.Include(e => e.EmpSocialInfo).Include(e => e.EmpSocialInfo.SocialActivities)
                                  .Include(e => e.EmpSocialInfo.Category)
                                  .Include(e => e.EmpSocialInfo.Religion)
                                  .Include(e => e.EmpSocialInfo.Caste)
                                  .Include(e => e.EmpSocialInfo.SubCaste)
                                  .Where(e => e.Id != null).ToList();
                            foreach (var item in EmpSocialInfo1)
                            {
                                if (item.EmpSocialInfo != null && EmpData.EmpSocialInfo != null)
                                {
                                    if (item.EmpSocialInfo.Id == EmpData.EmpSocialInfo.Id)
                                    {
                                        var v = EmpData.EmpCode;
                                        Msg.Add("Record Already Exist For Employee Code=" + v);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }

                            }



                            COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            EmpSocialInfo EmpSocialInfo = new EmpSocialInfo()
                            {
                                Category = COBJ.Category,
                                Caste = COBJ.Caste,
                                SubCaste = COBJ.SubCaste,
                                Religion = COBJ.Religion,
                                SocialActivities = OBJ,
                                DBTrack = COBJ.DBTrack
                            };
                            try
                            {



                                db.EmpSocialInfo.Add(EmpSocialInfo);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, COBJ.DBTrack);
                                DT_EmpSocialInfo DT_OBJ = (DT_EmpSocialInfo)rtn_Obj;
                                DT_OBJ.Religion_Id = COBJ.Religion == null ? 0 : COBJ.Religion.Id;
                                DT_OBJ.Caste_Id = COBJ.Caste == null ? 0 : COBJ.Caste.Id;
                                DT_OBJ.SubCaste_Id = COBJ.SubCaste == null ? 0 : COBJ.SubCaste.Id;
                                DT_OBJ.Category_Id = COBJ.Category == null ? 0 : COBJ.Category.Id;
                                db.Create(DT_OBJ);
                                db.SaveChanges();

                                if (EmpData != null)
                                {
                                    EmpData.EmpSocialInfo = EmpSocialInfo;
                                    db.Employee.Attach(EmpData);
                                    db.Entry(EmpData).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(EmpData).State = System.Data.Entity.EntityState.Detached;
                                }

                                ts.Complete();

                                Msg.Add("  Data Created successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new Object[] { null, null, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = COBJ.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        // return this.Json(new { msg = errorMsg });
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

        public class EmpSocialInfo_SA
        {
            public Array SA_id { get; set; }
            public Array SA_val { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<EmpSocialInfo_SA> return_data = new List<EmpSocialInfo_SA>();
                var EmpSocialInfo = db.Employee.Include(e => e.EmpSocialInfo).Include(e => e.EmpSocialInfo.SocialActivities)
                                    .Include(e => e.EmpSocialInfo.Category)
                                    .Include(e => e.EmpSocialInfo.Religion)
                                    .Include(e => e.EmpSocialInfo.Caste)
                                    .Include(e => e.EmpSocialInfo.SubCaste)
                                    .Where(e => e.Id == data).ToList();
                var EmpSocialInfo1 = db.Employee.Include(e => e.EmpSocialInfo).Include(e => e.EmpSocialInfo.SocialActivities)
                                  .Include(e => e.EmpSocialInfo.Category)
                                  .Include(e => e.EmpSocialInfo.Religion)
                                  .Include(e => e.EmpSocialInfo.Caste)
                                  .Include(e => e.EmpSocialInfo.SubCaste)
                                  .Where(e => e.Id == data).SingleOrDefault();


                var r = (from ca in EmpSocialInfo
                         select new
                         {
                             Id = ca.Id,
                             Religion = ca.EmpSocialInfo.Religion == null ? 0 : ca.EmpSocialInfo.Religion.Id,
                             Category = ca.EmpSocialInfo.Category == null ? 0 : ca.EmpSocialInfo.Category.Id,
                             Caste = ca.EmpSocialInfo.Caste == null ? 0 : ca.EmpSocialInfo.Caste.Id,
                             SubCaste = ca.EmpSocialInfo.SubCaste == null ? 0 : ca.EmpSocialInfo.SubCaste.Id,
                             Action = ca.DBTrack.Action
                         }).Distinct();

                var a = db.Employee.Include(e => e.EmpSocialInfo).Include(e => e.EmpSocialInfo.SocialActivities).Where(e => e.Id == data).Select(e => e.EmpSocialInfo.SocialActivities).ToList();

                foreach (var ca in a)
                {
                    return_data.Add(
                new EmpSocialInfo_SA
                {
                    SA_id = ca.Select(e => e.Id.ToString()).ToArray(),
                    SA_val = ca.Select(e => e.FullDetails).ToArray()
                });
                }


                var W = db.DT_EmpSocialInfo
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Category = e.Category_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.Category_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),
                         Religion = e.Religion_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.Religion_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),
                         Caste = e.Caste_Id == 0 ? "" : db.LookupValue
                                     .Where(x => x.Id == e.Caste_Id)
                                     .Select(x => x.LookupVal).FirstOrDefault(),
                         SubCaste = e.SubCaste_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.SubCaste_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),
                         SA_Val = e.SocialActivities_Id == 0 ? "" : db.SocialActivities.Where(x => x.Id == e.SocialActivities_Id).Select(x => x.FullDetails).FirstOrDefault(),

                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var LKup = db.EmpSocialInfo.Find(EmpSocialInfo1.EmpSocialInfo.Id);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;
                return this.Json(new Object[] { r, return_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        public ActionResult EditSocialActivities_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var val = db.SocialActivities.Where(e => e.Id == data).SingleOrDefault();

                var r = (from ca in db.SocialActivities
                         select new
                         {
                             Id = ca.Id,
                             FromPeriod = ca.FromPeriod,
                             ToPeriod = ca.ToPeriod,
                             PostHeld = ca.PostHeld,
                             InstituteName = ca.InstituteName,
                         }).Where(e => e.Id == data).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);

            }
        }



        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> EditSave1(EmpSocialInfo ESOBJ, FormCollection form, int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Values = form["SocialActivitieslist"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    var db_data = db.EmpSocialInfo.Include(e => e.SocialActivities).Where(e => e.Id == data).SingleOrDefault();
                    List<SocialActivities> OBJ = new List<SocialActivities>();
                    db_data.SocialActivities = null;
                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.SocialActivities.Find(ca);
                            OBJ.Add(Lookup_val);
                            db_data.SocialActivities = OBJ;
                        }
                    }

                    string Category = form["Categorylist"] == "0" ? "" : form["Categorylist"];
                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Category));
                            ESOBJ.Category = val;
                        }
                    }
                    string Religion = form["Religionlist"] == "0" ? "" : form["Religionlist"];
                    if (Religion != null)
                    {
                        if (Religion != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Religion));
                            ESOBJ.Religion = val;
                        }
                    }
                    string Caste = form["Castelist"] == "0" ? "" : form["Castelist"];
                    if (Caste != null)
                    {
                        if (Caste != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Caste));
                            ESOBJ.Caste = val;
                        }
                    }
                    string SubCaste = form["SubCastelist"] == "0" ? "" : form["SubCastelist"];
                    if (SubCaste != null)
                    {
                        if (SubCaste != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(SubCaste));
                            ESOBJ.SubCaste = val;
                        }
                    }





                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    EmpSocialInfo blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.EmpSocialInfo.Where(e => e.Id == data)
                                                                .Include(e => e.SocialActivities)
                                                                .Include(e => e.Category)
                                                                .Include(e => e.Religion)
                                                                .Include(e => e.Caste)
                                                                .Include(e => e.SubCaste)
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

                                    int a = EditS(Religion, Category, Caste, SubCaste, Values, data, ESOBJ, ESOBJ.DBTrack);



                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                        DT_EmpSocialInfo DT_OBJ = (DT_EmpSocialInfo)obj;
                                        DT_OBJ.Caste_Id = blog.Caste == null ? 0 : blog.Caste.Id;
                                        DT_OBJ.Category_Id = blog.Category == null ? 0 : blog.Category.Id;
                                        DT_OBJ.Religion_Id = blog.Religion == null ? 0 : blog.Religion.Id;
                                        DT_OBJ.SubCaste_Id = blog.SubCaste == null ? 0 : blog.SubCaste.Id;
                                        db.Create(DT_OBJ);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.Religion.LookupVal, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //  return Json(new Object[] { ESOBJ.Id, ESOBJ.Religion.LookupVal, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }

                            //catch (DbUpdateException e) { throw e; }
                            //catch (DataException e) { throw e; }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (EmpSocialInfo)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {

                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (EmpSocialInfo)databaseEntry.ToObject();
                                    ESOBJ.RowVersion = databaseValues.RowVersion;

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
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            EmpSocialInfo blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            //EmpSocialInfo Old_LKup = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.EmpSocialInfo.Where(e => e.Id == data).SingleOrDefault();
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
                            EmpSocialInfo EmpSocialInfo = new EmpSocialInfo()
                            {
                                //Category = ESOBJ.Category,
                                //Religion = ESOBJ.Religion,
                                //Caste = ESOBJ.Caste,
                                //SubCaste = ESOBJ.SubCaste,
                                SocialActivities = db_data.SocialActivities,
                                Id = data,
                                DBTrack = ESOBJ.DBTrack
                            };


                            db.Entry(blog).State = System.Data.Entity.EntityState.Detached;
                            EmpSocialInfo.DBTrack = ESOBJ.DBTrack;
                            db.EmpSocialInfo.Attach(EmpSocialInfo);
                            db.Entry(EmpSocialInfo).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(EmpSocialInfo).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = ESOBJ.Religion.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { blog.Id, ESOBJ.Religion, "Record Updated", JsonRequestBehavior.AllowGet });
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

        //[HttpPost]
        //public async Task<ActionResult> EditSave(EmpSocialInfo ESOBJ, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            string Values = form["SocialActivitieslist"];
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;

        //            var db_data = db.Employee.Include(e => e.EmpSocialInfo).Include(e => e.EmpSocialInfo.SocialActivities)
        //                          .Include(e => e.EmpSocialInfo.Category)
        //                          .Include(e => e.EmpSocialInfo.Religion)
        //                          .Include(e => e.EmpSocialInfo.Caste)
        //                          .Include(e => e.EmpSocialInfo.SubCaste)
        //                          .Where(e => e.Id == data).SingleOrDefault();
        //            int Eid = db_data.EmpSocialInfo.Id;
        //            // var db_data = db.EmpSocialInfo.Include(e => e.SocialActivities).Where(e => e.Id == data).SingleOrDefault();
        //            List<SocialActivities> SOBJ = new List<SocialActivities>();
        //            db_data.EmpSocialInfo.SocialActivities = null;
        //            if (Values != null)
        //            {
        //                var ids = Utility.StringIdsToListIds(Values);
        //                foreach (var ca in ids)
        //                {
        //                    var Lookup_val = db.SocialActivities.Find(ca);
        //                    SOBJ.Add(Lookup_val);
        //                    db_data.EmpSocialInfo.SocialActivities = SOBJ;
        //                }
        //            }
        //            var socialact = db_data.EmpSocialInfo.SocialActivities;
        //            string Category = form["Categorylist"] == "0" ? "" : form["Categorylist"];
        //            if (Category != null)
        //            {
        //                if (Category != "")
        //                {
        //                    var val = db.LookupValue.Find(int.Parse(Category));
        //                    ESOBJ.Category = val;
        //                }
        //            }
        //            string Religion = form["Religionlist"] == "0" ? "" : form["Religionlist"];
        //            if (Religion != null)
        //            {
        //                if (Religion != "")
        //                {
        //                    var val = db.LookupValue.Find(int.Parse(Religion));
        //                    ESOBJ.Religion = val;
        //                }
        //            }
        //            string Caste = form["Castelist"] == "0" ? "" : form["Castelist"];
        //            if (Caste != null)
        //            {
        //                if (Caste != "")
        //                {
        //                    var val = db.LookupValue.Find(int.Parse(Caste));
        //                    ESOBJ.Caste = val;
        //                }
        //            }
        //            string SubCaste = form["SubCastelist"] == "0" ? "" : form["SubCastelist"];
        //            if (SubCaste != null)
        //            {
        //                if (SubCaste != "")
        //                {
        //                    var val = db.LookupValue.Find(int.Parse(SubCaste));
        //                    ESOBJ.SubCaste = val;
        //                }
        //            }





        //            if (Auth == false)
        //            {


        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {

        //                        //DbContextTransaction transaction = db.Database.BeginTransaction();
        //                        db.EmpSocialInfo.Attach(db_data.EmpSocialInfo);
        //                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = db_data.RowVersion;
        //                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

        //                        var Curr_OBJ = db.EmpSocialInfo.Find(Eid);
        //                        TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
        //                        db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

        //                        //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                        //{
        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            EmpSocialInfo blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.EmpSocialInfo.Where(e => e.Id == Eid)
        //                                                         .Include(e => e.SocialActivities)
        //                                                         .Include(e => e.Category)
        //                                                         .Include(e => e.Religion)
        //                                                         .Include(e => e.Caste)
        //                                                         .Include(e => e.SubCaste)
        //                                                         .SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            ESOBJ.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };
        //                        //    int a = EditS(Religion, Category, Caste, SubCaste, Values, Eid, ESOBJ, ESOBJ.DBTrack);


        //                            if (Religion != null)
        //                            {
        //                                if (Religion != "")
        //                                {
        //                                    var val = db.LookupValue.Find(int.Parse(Religion));
        //                                    ESOBJ.Religion = val;

        //                                    var type = db.EmpSocialInfo.Include(e => e.Religion)
        //                                        .Where(e => e.Id == Eid).SingleOrDefault();
        //                                    IList<EmpSocialInfo> typedetails = null;
        //                                    if (type.Religion != null)
        //                                    {
        //                                        typedetails = db.EmpSocialInfo.Where(x => x.Religion.Id == type.Religion.Id && x.Id == Eid).ToList();
        //                                    }
        //                                    else
        //                                    {
        //                                        typedetails = db.EmpSocialInfo.Where(x => x.Id == Eid).ToList();
        //                                    }
        //                                    //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //                                    foreach (var s in typedetails)
        //                                    {
        //                                        s.Religion = ESOBJ.Religion;
        //                                        db.EmpSocialInfo.Attach(s);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        TempData["RowVersion"] = s.RowVersion;
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    var Dtls = db.EmpSocialInfo.Include(e => e.Religion).Where(x => x.Id == Eid).ToList();
        //                                    foreach (var s in Dtls)
        //                                    {
        //                                        s.Religion = null;
        //                                        db.EmpSocialInfo.Attach(s);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        TempData["RowVersion"] = s.RowVersion;
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                var Dtls = db.EmpSocialInfo.Include(e => e.Religion).Where(x => x.Id == Eid).ToList();
        //                                foreach (var s in Dtls)
        //                                {
        //                                    s.Religion = null;
        //                                    db.EmpSocialInfo.Attach(s);
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                    //await db.SaveChangesAsync();
        //                                    db.SaveChanges();
        //                                    TempData["RowVersion"] = s.RowVersion;
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                }
        //                            }



        //                            if (Category != null)
        //                            {
        //                                if (Category != "")
        //                                {
        //                                    var val = db.LookupValue.Find(int.Parse(Category));
        //                                    ESOBJ.Religion = val;

        //                                    var type = db.EmpSocialInfo
        //                                        .Include(e => e.Category)
        //                                        .Where(e => e.Id == Eid).SingleOrDefault();
        //                                    IList<EmpSocialInfo> typedetails = null;
        //                                    if (type.Category != null)
        //                                    {
        //                                        typedetails = db.EmpSocialInfo.Where(x => x.Category.Id == type.Category.Id && x.Id == Eid).ToList();
        //                                    }
        //                                    else
        //                                    {
        //                                        typedetails = db.EmpSocialInfo.Where(x => x.Id == Eid).ToList();
        //                                    }
        //                                    //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //                                    foreach (var s in typedetails)
        //                                    {
        //                                        s.Category = ESOBJ.Religion;
        //                                        db.EmpSocialInfo.Attach(s);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        TempData["RowVersion"] = s.RowVersion;
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    var Dtls = db.EmpSocialInfo.Include(e => e.Category).Where(x => x.Id == Eid).ToList();
        //                                    foreach (var s in Dtls)
        //                                    {
        //                                        s.Category = null;
        //                                        db.EmpSocialInfo.Attach(s);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        TempData["RowVersion"] = s.RowVersion;
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                var Dtls = db.EmpSocialInfo.Include(e => e.Religion).Where(x => x.Id == Eid).ToList();
        //                                foreach (var s in Dtls)
        //                                {
        //                                    s.Religion = null;
        //                                    db.EmpSocialInfo.Attach(s);
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                    //await db.SaveChangesAsync();
        //                                    db.SaveChanges();
        //                                    TempData["RowVersion"] = s.RowVersion;
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                }
        //                            }


        //                            if (Caste != null)
        //                            {
        //                                if (Caste != "")
        //                                {
        //                                    var val = db.LookupValue.Find(int.Parse(Caste));
        //                                    ESOBJ.Caste = val;
        //                                    var type = db.EmpSocialInfo
        //                                        .Include(e => e.Caste)
        //                                        .Where(e => e.Id == Eid).SingleOrDefault();
        //                                    IList<EmpSocialInfo> typedetails = null;
        //                                    if (type.Caste != null)
        //                                    {
        //                                        typedetails = db.EmpSocialInfo.Where(x => x.Caste.Id == type.Caste.Id && x.Id == Eid).ToList();
        //                                    }
        //                                    else
        //                                    {
        //                                        typedetails = db.EmpSocialInfo.Where(x => x.Id == Eid).ToList();
        //                                    }
        //                                    //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //                                    foreach (var s in typedetails)
        //                                    {
        //                                        s.Caste = ESOBJ.Caste;
        //                                        db.EmpSocialInfo.Attach(s);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        TempData["RowVersion"] = s.RowVersion;
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    var Dtls = db.EmpSocialInfo.Include(e => e.Caste).Where(x => x.Id == Eid).ToList();
        //                                    foreach (var s in Dtls)
        //                                    {
        //                                        s.Caste = null;
        //                                        db.EmpSocialInfo.Attach(s);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        TempData["RowVersion"] = s.RowVersion;
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                var Dtls = db.EmpSocialInfo.Include(e => e.Caste).Where(x => x.Id == Eid).ToList();
        //                                foreach (var s in Dtls)
        //                                {
        //                                    s.Caste = null;
        //                                    db.EmpSocialInfo.Attach(s);
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                    //await db.SaveChangesAsync();
        //                                    db.SaveChanges();
        //                                    TempData["RowVersion"] = s.RowVersion;
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                }
        //                            }


        //                            if (SubCaste != null)
        //                            {
        //                                if (SubCaste != "")
        //                                {
        //                                    var val = db.LookupValue.Find(int.Parse(SubCaste));
        //                                    ESOBJ.SubCaste = val;

        //                                    var type = db.EmpSocialInfo
        //                                        .Include(e => e.SubCaste)
        //                                        .Where(e => e.Id == Eid).SingleOrDefault();
        //                                    IList<EmpSocialInfo> typedetails = null;
        //                                    if (type.SubCaste != null)
        //                                    {
        //                                        typedetails = db.EmpSocialInfo.Where(x => x.SubCaste.Id == type.SubCaste.Id && x.Id == Eid).ToList();
        //                                    }
        //                                    else
        //                                    {
        //                                        typedetails = db.EmpSocialInfo.Where(x => x.Id == Eid).ToList();
        //                                    }
        //                                    //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //                                    foreach (var s in typedetails)
        //                                    {
        //                                        s.SubCaste = ESOBJ.SubCaste;
        //                                        db.EmpSocialInfo.Attach(s);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        TempData["RowVersion"] = s.RowVersion;
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    var Dtls = db.EmpSocialInfo.Include(e => e.SubCaste).Where(x => x.Id == Eid).ToList();
        //                                    foreach (var s in Dtls)
        //                                    {
        //                                        s.SubCaste = null;
        //                                        db.EmpSocialInfo.Attach(s);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        TempData["RowVersion"] = s.RowVersion;
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                var Dtls = db.EmpSocialInfo.Include(e => e.SubCaste).Where(x => x.Id == Eid).ToList();
        //                                foreach (var s in Dtls)
        //                                {
        //                                    s.SubCaste = null;
        //                                    db.EmpSocialInfo.Attach(s);
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                    //await db.SaveChangesAsync();
        //                                    db.SaveChanges();
        //                                    TempData["RowVersion"] = s.RowVersion;
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                }
        //                            }

        //                            var CurOBJ = db.EmpSocialInfo.Find(Eid);
        //                            TempData["CurrRowVersion"] = CurOBJ.RowVersion;
        //                            db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
        //                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            {
        //                                ESOBJ.DBTrack = ESOBJ.DBTrack;
        //                                EmpSocialInfo ESIOBJ = new EmpSocialInfo()
        //                                {
        //                                    Id = Eid,
        //                                    DBTrack = ESOBJ.DBTrack
        //                                };

        //                                db.EmpSocialInfo.Attach(ESIOBJ);
        //                                db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
        //                                db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                                //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                            }

        //                            EmpSocialInfo OBJ = new EmpSocialInfo
        //                            {
        //                                Id = Eid,
        //                                SocialActivities = socialact,
        //                                DBTrack = ESOBJ.DBTrack
        //                            };

        //                            //db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
        //                            // db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
        //                            DT_EmpSocialInfo DT_OBJ = (DT_EmpSocialInfo)obj;
        //                            DT_OBJ.Caste_Id = blog.Caste == null ? 0 : blog.Caste.Id;
        //                            DT_OBJ.Category_Id = blog.Category == null ? 0 : blog.Category.Id;
        //                            DT_OBJ.Religion_Id = blog.Religion == null ? 0 : blog.Religion.Id;
        //                            DT_OBJ.SubCaste_Id = blog.SubCaste == null ? 0 : blog.SubCaste.Id;
        //                            db.Create(DT_OBJ);
        //                            db.SaveChanges();
        //                            await db.SaveChangesAsync();
        //                            db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
        //                            ts.Complete();
        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.Religion.LookupVal, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                            //return Json(new Object[] { ESOBJ.Id, ESOBJ.Religion.LookupVal, "Record Updated", JsonRequestBehavior.AllowGet });
        //                        }
        //                        // }
        //                    }
        //                    catch (DbUpdateException e) { throw e; }
        //                    catch (DataException e) { throw e; }
        //                }
        //                else
        //                {
        //                    StringBuilder sb = new StringBuilder("");
        //                    foreach (ModelState modelState in ModelState.Values)
        //                    {
        //                        foreach (ModelError error in modelState.Errors)
        //                        {
        //                            sb.Append(error.ErrorMessage);
        //                            sb.Append("." + "\n");
        //                        }
        //                    }
        //                    var errorMsg = sb.ToString();
        //                    Msg.Add(errorMsg);
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    EmpSocialInfo blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    EmpSocialInfo Old_OBJ = null;
        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.EmpSocialInfo.Where(e => e.Id == Eid).SingleOrDefault();
        //                        TempData["RowVersion"] = blog.RowVersion;
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }
        //                    ESOBJ.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };
        //                    EmpSocialInfo EmpSocialInfo = new EmpSocialInfo()
        //                    {
        //                        Id = Eid,
        //                        SocialActivities = socialact,
        //                        //Caste = blog.Caste,
        //                        //Category = blog.Category,
        //                        //Religion = blog.Religion,
        //                        //SubCaste = blog.SubCaste,
        //                        DBTrack = ESOBJ.DBTrack
        //                    };
        //                    db.Entry(db_data.EmpSocialInfo).State = System.Data.Entity.EntityState.Detached;
        //                    db.EmpSocialInfo.Attach(EmpSocialInfo);
        //                    db.Entry(EmpSocialInfo).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(EmpSocialInfo).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    TempData["RowVersion"] = db_data.RowVersion;
        //                    db.Entry(EmpSocialInfo).State = System.Data.Entity.EntityState.Detached;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, ESOBJ, "EmpSocialInfo", ESOBJ.DBTrack);
        //                        DT_EmpSocialInfo DT_OBJ = (DT_EmpSocialInfo)obj;
        //                        Old_OBJ = context.EmpSocialInfo.Where(e => e.Id == Eid).Include(e => e.Religion)
        //                            .Include(e => e.SubCaste)
        //                            .Include(e => e.Caste)
        //                            .Include(e => e.Caste).SingleOrDefault();

        //                        DT_OBJ.Religion_Id = DBTrackFile.ValCompare(Old_OBJ.Religion, ESOBJ.Religion);//Old_OBJ.Address == c.Address ? 0 : Old_OBJ.Address == null && c.Address != null ? c.Address.Id : Old_OBJ.Address.Id;
        //                        DT_OBJ.SubCaste_Id = DBTrackFile.ValCompare(Old_OBJ.SubCaste, ESOBJ.SubCaste); //Old_OBJ.BusinessType == c.BusinessType ? 0 : Old_OBJ.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_OBJ.BusinessType.Id;
        //                        DT_OBJ.Caste_Id = DBTrackFile.ValCompare(Old_OBJ.Caste, ESOBJ.Caste); //Old_OBJ.SocialActivities == c.SocialActivities ? 0 : Old_OBJ.SocialActivities == null && c.SocialActivities != null ? c.SocialActivities.Id : Old_OBJ.SocialActivities.Id;
        //                        DT_OBJ.Category_Id = DBTrackFile.ValCompare(Old_OBJ.Category, ESOBJ.Category); //Old_OBJ.SocialActivities == c.SocialActivities ? 0 : Old_OBJ.SocialActivities == null && c.SocialActivities != null ? c.SocialActivities.Id : Old_OBJ.SocialActivities.Id;
        //                        db.Create(DT_OBJ);
        //                        //db.SaveChanges();
        //                    }


        //                    ts.Complete();
        //                    Msg.Add("  Record Updated");
        //                    return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = ESOBJ.Religion.LookupVal, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    //return Json(new Object[] { blog.Id, ESOBJ.Religion.LookupVal, "Record Updated", JsonRequestBehavior.AllowGet });
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

        [HttpPost]
        public async Task<ActionResult> EditSave(EmpSocialInfo ESOBJ, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                       
                        string Religion = form["Religionlist"] == "0" ? "" : form["Religionlist"];
                        string Category = form["Categorylist"] == "0" ? "" : form["Categorylist"];
                        string Caste = form["Castelist"] == "0" ? "" : form["Castelist"];
                        string SubCaste = form["SubCastelist"] == "0" ? "" : form["SubCastelist"];
                        string Values = form["SocialActivitieslist"];



                        if (Religion != null && Religion != "")
                        {
                            int Religionid = Convert.ToInt32(Religion);
                            LookupValue ReligionType = db.LookupValue.Find(Religionid);
                            ESOBJ.Religion = ReligionType;
                        }


                        if (Category != null && Category != "")
                        {
                            int Categoryid = Convert.ToInt32(Category);
                            LookupValue CategoryType = db.LookupValue.Find(Categoryid);
                            ESOBJ.Category = CategoryType;
                        }

                        if (Caste != null && Caste != "")
                        {
                            int Casteid = Convert.ToInt32(Caste);
                            LookupValue CasteType = db.LookupValue.Find(Casteid);
                            ESOBJ.Caste = CasteType;
                        }

                        if (SubCaste != null && SubCaste != "")
                        {
                            int SubCasteid = Convert.ToInt32(SubCaste);
                            LookupValue SubCasteType = db.LookupValue.Find(SubCasteid);
                            ESOBJ.SubCaste = SubCasteType;
                        }

                        List<SocialActivities> SocialActivitieslist = new List<SocialActivities>();
                        if (Values != null)
                        {
                            var ids = Utility.StringIdsToListIds(Values);
                            foreach (var ca in ids)
                            {
                                var Lookup_val = db.SocialActivities.Find(ca);
                                SocialActivitieslist.Add(Lookup_val);
                                
                            }
                            ESOBJ.SocialActivities = SocialActivitieslist;
                        }


                        var Employee = db.Employee.Include(e => e.EmpSocialInfo).Include(e => e.EmpSocialInfo.SocialActivities)
                                          .Include(e => e.EmpSocialInfo.Category)
                                          .Include(e => e.EmpSocialInfo.Religion)
                                          .Include(e => e.EmpSocialInfo.Caste)
                                          .Include(e => e.EmpSocialInfo.SubCaste)
                                          .Where(e => e.Id == data).SingleOrDefault();
                        

                        var db_data = db.EmpSocialInfo.Include(e => e.Category)
                                          .Include(e => e.Religion)
                                          .Include(e => e.Caste)
                                          .Include(e => e.SubCaste).Where(e => e.Id == Employee.EmpSocialInfo.Id).SingleOrDefault();

                        TempData["RowVersion"] = db_data.RowVersion;
                        EmpSocialInfo EmpSocialInfodata = db.EmpSocialInfo.Find(db_data.Id);
                        TempData["CurrRowVersion"] = EmpSocialInfodata.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            ESOBJ.DBTrack = new DBTrack
                             {
                                 CreatedBy = EmpSocialInfodata.DBTrack.CreatedBy == null ? null : EmpSocialInfodata.DBTrack.CreatedBy,
                                 CreatedOn = EmpSocialInfodata.DBTrack.CreatedOn == null ? null : EmpSocialInfodata.DBTrack.CreatedOn,
                                 Action = "M",
                                 ModifiedBy = SessionManager.UserName,
                                 ModifiedOn = DateTime.Now
                             };


                            EmpSocialInfodata.Id = db_data.Id;
                            EmpSocialInfodata.Religion = ESOBJ.Religion;

                            EmpSocialInfodata.Category = ESOBJ.Category;
                            EmpSocialInfodata.Caste = ESOBJ.Caste;
                            EmpSocialInfodata.SubCaste = ESOBJ.SubCaste;
                            EmpSocialInfodata.SocialActivities = SocialActivitieslist;
                            EmpSocialInfodata.DBTrack = ESOBJ.DBTrack;

                            db.Entry(EmpSocialInfodata).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();


                        }
                        ts.Complete();
                        Msg.Add(" Record Updated Successfully. ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                            //EmpSocialInfo OBJ = db.EmpSocialInfo.Find(auth_id);
                            //EmpSocialInfo OBJ = db.EmpSocialInfo.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                            EmpSocialInfo OBJ = db.EmpSocialInfo
                                                .Include(e => e.Religion)
                                                .Include(e => e.Category)
                                                .Include(e => e.Caste)
                                                .Include(e => e.SubCaste).
                                                FirstOrDefault(e => e.Id == auth_id);

                            OBJ.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = OBJ.DBTrack.ModifiedBy != null ? OBJ.DBTrack.ModifiedBy : null,
                                CreatedBy = OBJ.DBTrack.CreatedBy != null ? OBJ.DBTrack.CreatedBy : null,
                                CreatedOn = OBJ.DBTrack.CreatedOn != null ? OBJ.DBTrack.CreatedOn : null,
                                IsModified = OBJ.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.EmpSocialInfo.Attach(OBJ);
                            db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, OBJ.DBTrack);
                            DT_EmpSocialInfo DT_OBJ = (DT_EmpSocialInfo)rtn_Obj;
                            DT_OBJ.Religion_Id = OBJ.Religion == null ? 0 : OBJ.Religion.Id;
                            DT_OBJ.Caste_Id = OBJ.Caste == null ? 0 : OBJ.Caste.Id;
                            DT_OBJ.Category_Id = OBJ.Category == null ? 0 : OBJ.Category.Id;
                            DT_OBJ.SubCaste_Id = OBJ.SubCaste == null ? 0 : OBJ.SubCaste.Id;
                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorized");
                            return Json(new Utility.JsonReturnClass { Id = OBJ.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { OBJ.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        EmpSocialInfo Old_OBJ = db.EmpSocialInfo
                                                .Include(e => e.Religion)
                                                .Include(e => e.Category)
                                                .Include(e => e.Caste)
                                                .Include(e => e.SubCaste)
                                                .Where(e => e.Id == auth_id).SingleOrDefault();

                        DT_EmpSocialInfo Curr_OBJ = db.DT_EmpSocialInfo
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_OBJ != null)
                        {
                            EmpSocialInfo OBJ = new EmpSocialInfo();

                            string Religion = Curr_OBJ.Religion_Id == null ? null : Curr_OBJ.Religion_Id.ToString();
                            string Caste = Curr_OBJ.Caste_Id == null ? null : Curr_OBJ.Caste_Id.ToString();
                            string Subcaste = Curr_OBJ.SubCaste_Id == null ? null : Curr_OBJ.SubCaste_Id.ToString();
                            string Category = Curr_OBJ.Category_Id == null ? null : Curr_OBJ.Category_Id.ToString();
                            string SA = Curr_OBJ.SocialActivities_Id == null ? null : Curr_OBJ.SocialActivities_Id.ToString();

                            //OBJ.Name = Curr_OBJ.Name == null ? Old_OBJ.Name : Curr_OBJ.Name;
                            //OBJ.Code = Curr_OBJ.Code == null ? Old_OBJ.Code : Curr_OBJ.Code;
                            //      OBJ.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        OBJ.DBTrack = new DBTrack
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
                                        int a = EditS(Religion, Category, Caste, Subcaste, SA, auth_id, OBJ, OBJ.DBTrack);
                                        //int a = EditS(OBJ, Addrs, SocialActivities, auth_id, OBJ, OBJ.DBTrack);    
                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        Msg.Add("  Record Authorized");
                                        return Json(new Utility.JsonReturnClass { Id = OBJ.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        // return Json(new Object[] { OBJ.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (EmpSocialInfo)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (EmpSocialInfo)databaseEntry.ToObject();
                                        OBJ.RowVersion = databaseValues.RowVersion;
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
                            //EmpSocialInfo OBJ = db.EmpSocialInfo.Find(auth_id);
                            EmpSocialInfo OBJ = db.EmpSocialInfo.AsNoTracking()
                                                .Include(e => e.Religion)
                                                .Include(e => e.Category)
                                                .Include(e => e.Caste)
                                                .Include(e => e.SubCaste)
                                                .FirstOrDefault(e => e.Id == auth_id);

                            LookupValue val = OBJ.Caste;

                            OBJ.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = OBJ.DBTrack.ModifiedBy != null ? OBJ.DBTrack.ModifiedBy : null,
                                CreatedBy = OBJ.DBTrack.CreatedBy != null ? OBJ.DBTrack.CreatedBy : null,
                                CreatedOn = OBJ.DBTrack.CreatedOn != null ? OBJ.DBTrack.CreatedOn : null,
                                IsModified = false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.EmpSocialInfo.Attach(OBJ);
                            db.Entry(OBJ).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, OBJ.DBTrack);
                            DT_EmpSocialInfo DT_OBJ = (DT_EmpSocialInfo)rtn_Obj;
                            DT_OBJ.Caste_Id = OBJ.Caste == null ? 0 : OBJ.Caste.Id;
                            DT_OBJ.SubCaste_Id = OBJ.SubCaste == null ? 0 : OBJ.SubCaste.Id;
                            DT_OBJ.Religion_Id = OBJ.Religion == null ? 0 : OBJ.Religion.Id;
                            DT_OBJ.Category_Id = OBJ.Category == null ? 0 : OBJ.Category.Id;
                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();
                            db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorized ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "Record Authorized", JsonRequestBehavior.AllowGet });
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











        public int EditS(string RVal, string CatVal, string CasteVal, string SubCasteVal, string SAVal, int Eid, EmpSocialInfo ESOBJ, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (RVal != null)
                {
                    if (RVal != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(RVal));
                        ESOBJ.Religion = val;

                        var type = db.EmpSocialInfo.Include(e => e.Religion)
                            .Where(e => e.Id == Eid).SingleOrDefault();
                        IList<EmpSocialInfo> typedetails = null;
                        if (type.Religion != null)
                        {
                            typedetails = db.EmpSocialInfo.Where(x => x.Religion.Id == type.Religion.Id && x.Id == Eid).ToList();
                        }
                        else
                        {
                            typedetails = db.EmpSocialInfo.Where(x => x.Id == Eid).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.Religion = ESOBJ.Religion;
                            db.EmpSocialInfo.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.EmpSocialInfo.Include(e => e.Religion).Where(x => x.Id == Eid).ToList();
                        foreach (var s in Dtls)
                        {
                            s.Religion = null;
                            db.EmpSocialInfo.Attach(s);
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
                    var Dtls = db.EmpSocialInfo.Include(e => e.Religion).Where(x => x.Id == Eid).ToList();
                    foreach (var s in Dtls)
                    {
                        s.Religion = null;
                        db.EmpSocialInfo.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }



                if (CatVal != null)
                {
                    if (CatVal != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(CatVal));
                        ESOBJ.Religion = val;

                        var type = db.EmpSocialInfo
                            .Include(e => e.Category)
                            .Where(e => e.Id == Eid).SingleOrDefault();
                        IList<EmpSocialInfo> typedetails = null;
                        if (type.Category != null)
                        {
                            typedetails = db.EmpSocialInfo.Where(x => x.Category.Id == type.Category.Id && x.Id == Eid).ToList();
                        }
                        else
                        {
                            typedetails = db.EmpSocialInfo.Where(x => x.Id == Eid).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.Category = ESOBJ.Religion;
                            db.EmpSocialInfo.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.EmpSocialInfo.Include(e => e.Category).Where(x => x.Id == Eid).ToList();
                        foreach (var s in Dtls)
                        {
                            s.Category = null;
                            db.EmpSocialInfo.Attach(s);
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
                    var Dtls = db.EmpSocialInfo.Include(e => e.Religion).Where(x => x.Id == Eid).ToList();
                    foreach (var s in Dtls)
                    {
                        s.Religion = null;
                        db.EmpSocialInfo.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                if (CasteVal != null)
                {
                    if (CasteVal != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(CasteVal));
                        ESOBJ.Caste = val;
                        var type = db.EmpSocialInfo
                            .Include(e => e.Caste)
                            .Where(e => e.Id == Eid).SingleOrDefault();
                        IList<EmpSocialInfo> typedetails = null;
                        if (type.Caste != null)
                        {
                            typedetails = db.EmpSocialInfo.Where(x => x.Caste.Id == type.Caste.Id && x.Id == Eid).ToList();
                        }
                        else
                        {
                            typedetails = db.EmpSocialInfo.Where(x => x.Id == Eid).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.Caste = ESOBJ.Caste;
                            db.EmpSocialInfo.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.EmpSocialInfo.Include(e => e.Caste).Where(x => x.Id == Eid).ToList();
                        foreach (var s in Dtls)
                        {
                            s.Caste = null;
                            db.EmpSocialInfo.Attach(s);
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
                    var Dtls = db.EmpSocialInfo.Include(e => e.Caste).Where(x => x.Id == Eid).ToList();
                    foreach (var s in Dtls)
                    {
                        s.Caste = null;
                        db.EmpSocialInfo.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                if (SubCasteVal != null)
                {
                    if (SubCasteVal != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(SubCasteVal));
                        ESOBJ.SubCaste = val;

                        var type = db.EmpSocialInfo
                            .Include(e => e.SubCaste)
                            .Where(e => e.Id == Eid).SingleOrDefault();
                        IList<EmpSocialInfo> typedetails = null;
                        if (type.SubCaste != null)
                        {
                            typedetails = db.EmpSocialInfo.Where(x => x.SubCaste.Id == type.SubCaste.Id && x.Id == Eid).ToList();
                        }
                        else
                        {
                            typedetails = db.EmpSocialInfo.Where(x => x.Id == Eid).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.SubCaste = ESOBJ.SubCaste;
                            db.EmpSocialInfo.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.EmpSocialInfo.Include(e => e.SubCaste).Where(x => x.Id == Eid).ToList();
                        foreach (var s in Dtls)
                        {
                            s.SubCaste = null;
                            db.EmpSocialInfo.Attach(s);
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
                    var Dtls = db.EmpSocialInfo.Include(e => e.SubCaste).Where(x => x.Id == Eid).ToList();
                    foreach (var s in Dtls)
                    {
                        s.SubCaste = null;
                        db.EmpSocialInfo.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                //if (SAVal != null)
                //{
                //    if (SAVal != "")
                //    {

                //        List<int> IDs = SAVal.Split(',').Select(e => int.Parse(e)).ToList();
                //        foreach (var k in IDs)
                //        {
                //            var value = db.SocialActivities.Find(k);
                //            ESOBJ.SocialActivities = new List<SocialActivities>();
                //            ESOBJ.SocialActivities.Add(value);
                //        }
                //    }
                //}
                //else
                //{
                //    var Details = db.EmpSocialInfo.Include(e => e.SocialActivities).Where(x => x.Id == data).ToList();
                //    foreach (var s in Details)
                //    {
                //        s.SocialActivities = null;
                //        db.EmpSocialInfo.Attach(s);
                //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                //        //await db.SaveChangesAsync();
                //        db.SaveChanges();
                //        TempData["RowVersion"] = s.RowVersion;
                //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                //    }
                //}



                var CurOBJ = db.EmpSocialInfo.Find(Eid);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    ESOBJ.DBTrack = dbT;
                    EmpSocialInfo ESIOBJ = new EmpSocialInfo()
                    {
                        Id = Eid,
                        DBTrack = ESOBJ.DBTrack
                    };

                    db.EmpSocialInfo.Attach(ESIOBJ);
                    db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
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
                    var EmpSocialInfo = db.Employee.Include(e => e.EmpSocialInfo).Include(e => e.EmpSocialInfo.SocialActivities)
                                  .Include(e => e.EmpSocialInfo.Category)
                                  .Include(e => e.EmpSocialInfo.Religion)
                                  .Include(e => e.EmpSocialInfo.Caste)
                                  .Include(e => e.EmpSocialInfo.SubCaste)
                                  .Where(e => e.Id == data).SingleOrDefault();



                    // var EmpSocialInfo = db.EmpSocialInfo.Include(e=>e.Caste).Include(e=>e.Category).Include(e=>e.SubCaste).Include(e=>e.Religion).Include(e => e.SocialActivities).Where(e => e.Id == data).SingleOrDefault();

                    if (EmpSocialInfo.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = EmpSocialInfo.DBTrack.CreatedBy != null ? EmpSocialInfo.DBTrack.CreatedBy : null,
                                CreatedOn = EmpSocialInfo.DBTrack.CreatedOn != null ? EmpSocialInfo.DBTrack.CreatedOn : null,
                                IsModified = EmpSocialInfo.DBTrack.IsModified == true ? true : false
                            };
                            EmpSocialInfo.DBTrack = dbT;
                            db.Entry(EmpSocialInfo).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, EmpSocialInfo.DBTrack);
                            DT_EmpSocialInfo DT_OBJ = (DT_EmpSocialInfo)rtn_Obj;
                            db.Create(DT_OBJ);

                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //Msg.Add("  Data removed successfully.  ");
                            //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            //  var v = EmpSocialInfo.Where(a => a.EmpSocialInfo.Id == EmpSocialInfo.Id).ToList();
                            //  db.Employee.RemoveRange(v);
                            // db.SaveChanges();


                            var selectedValues = EmpSocialInfo.EmpSocialInfo.SocialActivities;
                            var lkValue = new HashSet<int>(EmpSocialInfo.EmpSocialInfo.SocialActivities.Select(e => e.Id));
                            if (lkValue.Count > 0)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                            }
                            db.Entry(EmpSocialInfo.EmpSocialInfo).State = System.Data.Entity.EntityState.Deleted;


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



        //[ValidateAntiForgeryToken]
        public ActionResult DeleteOld(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    EmpSocialInfo EmpSocialInfo = db.EmpSocialInfo.Include(e => e.SocialActivities).Where(e => e.Id == data).SingleOrDefault();
                    try
                    {
                        var selectedValues = EmpSocialInfo.SocialActivities;
                        var lkValue = new HashSet<int>(EmpSocialInfo.SocialActivities.Select(e => e.Id));
                        if (lkValue.Count > 0)
                        {
                            Msg.Add(" Child record exists.Cannot remove it..  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                        }

                        using (TransactionScope ts = new TransactionScope())
                        {
                            db.Entry(EmpSocialInfo).State = System.Data.Entity.EntityState.Deleted;
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
                        //return this.Json(new Object[] { "", "Unable to delete. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
                        Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
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


        //public ActionResult Getalldetails(string v, string x)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.EmpSocialInfo.ToList();
        //        IEnumerable<EmpSocialInfo> all;
        //        if (!string.IsNullOrEmpty(x))
        //        {
        //            all = db.EmpSocialInfo.ToList().Where(d => d.Category.Contains(x) || d.Religion.ToLower().Contains(x.ToLower()));

        //        }
        //        else
        //        {
        //            var r = (from ca in fall select new { srno = ca.Id, SocialActivities = ca.Category + " - " + ca.Religion }).Distinct();
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.Category, c.Religion }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //}
        //public ActionResult GetLookupValue(string data, string data2)
        //{
        //    SelectList s = (SelectList)null;
        //    var selected = "";
        //    if (data != "" && data != null)
        //    {
        //        var qurey = db.EmpSocialInfo.Include(e => e.SocialActivities)
        //            .Where(e => e.Id == data).SingleOrDefault();
        //        if (data2 != "" && data2 != "0")
        //        {
        //            selected = data2;
        //        }
        //        if (qurey != null)
        //        {
        //            s = new SelectList(qurey.SocialActivities, "Id", "OBJ", selected);
        //        }
        //    }
        //    return Json(s, JsonRequestBehavior.AllowGet);

        //}
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
        //        var LKVal = db.EmpSocialInfo.ToList();

        //        if (gp.IsAutho == true)
        //        {
        //            LKVal = db.EmpSocialInfo.Include(e => e.SocialActivities).Include(e => e.Category).Include(e => e.Religion).Include(e => e.Caste).Include(e => e.SubCaste).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            LKVal = db.EmpSocialInfo.Include(e => e.SocialActivities).Include(e => e.Category).Include(e => e.Religion).Include(e => e.Caste).Include(e => e.SubCaste).AsNoTracking().ToList();
        //        }


        //        IEnumerable<EmpSocialInfo> IE;
        //        if (!string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = LKVal;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.Category, a.Religion, a.Caste, a.SubCaste }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Category.LookupVal.ToLower() == gp.searchString) || (e.Religion.LookupVal.ToLower() == gp.searchString.ToLower()) || (e.Caste.LookupVal.ToLower() == gp.searchString.ToLower()) || (e.SubCaste.LookupVal.ToLower() == gp.searchString.ToLower())));
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Category.LookupVal, a.Religion.LookupVal, a.Caste.LookupVal, a.SubCaste.LookupVal }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = LKVal;
        //            Func<EmpSocialInfo, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "Category" ? c.Category.LookupVal.ToString() :
        //                                 gp.sidx == "Religion" ? c.Religion.LookupVal.ToString() :
        //                                 gp.sidx == "Caste" ? c.Caste.LookupVal.ToString() :
        //                                 gp.sidx == "SubCaste" ? c.SubCaste.LookupVal.ToString() :
        //                                 "");
        //            }

        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.Category.LookupVal, a.Religion.LookupVal, a.Caste.LookupVal, a.SubCaste.LookupVal }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.Category.LookupVal, a.Religion.LookupVal, a.Caste.LookupVal, a.SubCaste.LookupVal }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Category.LookupVal, a.Religion.LookupVal, a.Caste.LookupVal, a.SubCaste.LookupVal }).ToList();
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

                IEnumerable<Employee> Employee = null;
                Employee = db.Employee.Include(q => q.EmpSocialInfo).Include(q => q.EmpName).ToList();
                if (gp.IsAutho == true)
                {
                    Employee = db.Employee.Include(q => q.EmpSocialInfo).Include(q => q.EmpName).Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    // Employee = db.Employee.Where(a=>a.Id==db.EmpAcademicInfo.Contains()).ToList();
                    Employee = db.Employee.Include(q => q.EmpSocialInfo).Include(q => q.ServiceBookDates).Where(q => q.EmpSocialInfo != null && q.ServiceBookDates.ServiceLastDate == null).ToList();
                }
                IEnumerable<Employee> IE;

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Employee;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString.ToString()))
                               || (e.EmpName.FullNameFML.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                  || (e.Id.ToString().Contains(gp.searchString.ToString())))
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
                    IE = Employee;
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
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    totalRecords = Employee.Count();
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


        public ActionResult GetLookupDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.SocialActivities.ToList();
                IEnumerable<SocialActivities> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.SocialActivities.ToList().Where(d => d.InstituteName.Contains(data));
                }
                else
                {
                    //  var list1 = db.EmpSocialInfo.ToList().SelectMany(e => e.SocialActivities);
                    var list1 = db.EmpSocialInfo.SelectMany(e => e.SocialActivities).ToList();
                    var list2 = fall.Except(list1);
                    var r = (from ca in list2 select new { srno = ca.Id, SocialActivities = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }

                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }





        //[HttpPost]
        //public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save Authorized data
        //{
        //    if (auth_action == "C")
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {
        //            EmpSocialInfo ESI = db.EmpSocialInfo.Include(e => e.SocialActivities)
        //                .Include(e=>e.Category)
        //                .Include(e => e.Religion)
        //                .Include(e => e.SubCaste)
        //                .Include(e => e.Caste) 
        //                .FirstOrDefault(e => e.Id == auth_id);

        //            ESI.DBTrack = new DBTrack
        //            {
        //                Action = "C",
        //                ModifiedBy = ESI.DBTrack.ModifiedBy != null ? ESI.DBTrack.ModifiedBy : null,
        //                CreatedBy = ESI.DBTrack.CreatedBy != null ? ESI.DBTrack.CreatedBy : null,
        //                CreatedOn = ESI.DBTrack.CreatedOn != null ? ESI.DBTrack.CreatedOn : null,
        //                IsModified = ESI.DBTrack.IsModified == true ? false : false,
        //                AuthorizedBy = SessionManager.UserName,
        //                AuthorizedOn = DateTime.Now
        //            };

        //            db.EmpSocialInfo.Attach(ESI);
        //            db.Entry(ESI).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(ESI).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            //db.SaveChanges();
        //            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, ESI.DBTrack);
        //            DT_EmpSocialInfo DT_OBJ = (DT_EmpSocialInfo)rtn_Obj;

        //            db.Create(DT_OBJ);
        //            await db.SaveChangesAsync();

        //            ts.Complete();
        //            return Json(new Object[] { ESI.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
        //        }
        //    }
        //    else if (auth_action == "M")
        //    {

        //        EmpSocialInfo Old_OBJ = db.EmpSocialInfo.Include(e => e.SocialActivities)
        //                                    .Include(e => e.Category)
        //                                    .Include(e => e.Religion)
        //                                    .Include(e => e.SubCaste)
        //                                    .Include(e => e.Caste) 
        //                                    .Where(e => e.Id == auth_id).SingleOrDefault();


        //        DT_EmpSocialInfo Curr_OBJ = db.DT_EmpSocialInfo
        //                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
        //                                    .OrderByDescending(e => e.Id)
        //                                    .FirstOrDefault();

        //        if (Curr_OBJ != null)
        //        {
        //            EmpSocialInfo ESI = new EmpSocialInfo();
        //            string Category = Curr_OBJ.Category_Id == 0 ? null : Curr_OBJ.Category_Id.ToString();
        //            string Religion = Curr_OBJ.Religion_Id == 0? null : Curr_OBJ.Religion_Id.ToString();
        //            string Caste = Curr_OBJ.Caste_Id == 0? null : Curr_OBJ.Caste_Id.ToString();
        //            string SubCaste = Curr_OBJ.SubCaste_Id == 0 ? null : Curr_OBJ.SubCaste_Id.ToString();



        //            //      corp.Id = auth_id;

        //            if (ModelState.IsValid)
        //            {
        //                try
        //                {

        //                    //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                    {
        //                        // db.Configuration.AutoDetectChangesEnabled = false;
        //                        ESI.DBTrack = new DBTrack
        //                        {
        //                            CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
        //                            CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
        //                            Action = "M",
        //                            ModifiedBy = Old_OBJ.DBTrack.ModifiedBy == null ? null : Old_OBJ.DBTrack.ModifiedBy,
        //                            ModifiedOn = Old_OBJ.DBTrack.ModifiedOn == null ? null : Old_OBJ.DBTrack.ModifiedOn,
        //                            AuthorizedBy = SessionManager.UserName,
        //                            AuthorizedOn = DateTime.Now,
        //                            IsModified = false
        //                        };

        //                        //int a = EditS(Corp, Addrs, SocialActivities, auth_id, corp, corp.DBTrack);

        //                        await db.SaveChangesAsync();

        //                        ts.Complete();
        //                        return Json(new Object[] { ESI.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
        //                    }
        //                }
        //                catch (DbUpdateConcurrencyException ex)
        //                {
        //                    var entry = ex.Entries.Single();
        //                    var clientValues = (EmpSocialInfo)entry.Entity;
        //                    var databaseEntry = entry.GetDatabaseValues();
        //                    if (databaseEntry == null)
        //                    {
        //                        return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                    }
        //                    else
        //                    {
        //                        var databaseValues = (EmpSocialInfo)databaseEntry.ToObject();
        //                        ESI.RowVersion = databaseValues.RowVersion;
        //                    }
        //                }

        //                return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //            }
        //        }
        //        else
        //            return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
        //    }
        //    else if (auth_action == "D")
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {
        //            //EmpSocialInfo corp = db.EmpSocialInfo.Find(auth_id);
        //            EmpSocialInfo ESI = db.EmpSocialInfo.AsNoTracking().Include(e => e.SocialActivities).FirstOrDefault(e => e.Id == auth_id);

        //            //Address add = corp.Address;
        //            //SocialActivities conDet = corp.SocialActivities;
        //            //SocialActivities val = corp.BusinessType;

        //            ESI.DBTrack = new DBTrack
        //            {
        //                Action = "D",
        //                ModifiedBy = ESI.DBTrack.ModifiedBy != null ? ESI.DBTrack.ModifiedBy : null,
        //                CreatedBy = ESI.DBTrack.CreatedBy != null ? ESI.DBTrack.CreatedBy : null,
        //                CreatedOn = ESI.DBTrack.CreatedOn != null ? ESI.DBTrack.CreatedOn : null,
        //                IsModified = false,
        //                AuthorizedBy = SessionManager.UserName,
        //                AuthorizedOn = DateTime.Now
        //            };

        //            db.EmpSocialInfo.Attach(ESI);
        //            db.Entry(ESI).State = System.Data.Entity.EntityState.Deleted;


        //            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, ESI.DBTrack);
        //            DT_EmpSocialInfo DT_OBJ= (DT_EmpSocialInfo)rtn_Obj;
        //            //DT_OBJ.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
        //            //DT_OBJ.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
        //            //DT_OBJ.ContactDetails_Id = corp.SocialActivities == null ? 0 : corp.SocialActivities.Id;
        //            db.Create(DT_OBJ);
        //            await db.SaveChangesAsync();
        //            db.Entry(ESI).State = System.Data.Entity.EntityState.Detached;
        //            ts.Complete();
        //            return Json(new Object[] { "", "Record Authorized", JsonRequestBehavior.AllowGet });
        //        }

        //    }
        //    return View();

        //}
    }
}
