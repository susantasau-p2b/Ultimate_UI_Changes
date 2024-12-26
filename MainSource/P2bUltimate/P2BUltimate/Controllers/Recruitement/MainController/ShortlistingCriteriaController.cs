using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Text;
using P2BUltimate.Models;
using System.Threading.Tasks;
using P2BUltimate.Security;
using P2b.Global;
using Recruitment;

namespace P2BUltimate.Controllers.Recruitement.MainController
{
    public class ShortlistingCriteriaController : Controller
    {
        List<String> Msg = new List<String>();

        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /ShortlistingCriteria/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Recruitement/_ShortlistingCriteria.cshtml");
        }

        [HttpPost]
        public ActionResult Create(ShortlistingCriteria ListCriteria, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    try
                    {

                        string FuncStruct = form["FuncStructlist"] == "0" ? "" : form["FuncStructlist"];
                        string ExpFilter = form["ExpFilterlist1"] == "0" ? "" : form["ExpFilterlist1"];
                        string RangeFilter = form["RangeFilterList"] == "0" ? "" : form["RangeFilterList"];
                        string SpecialCategory = form["SpecialCategoryList"] == "0" ? "" : form["SpecialCategoryList"];
                        string Category = form["CategorylistNew"] == "0" ? "" : form["CategorylistNew"];
                        string Skill = form["Skilllist"] == "0" ? "" : form["Skilllist"];
                        string Qualification = form["Qualificationlist"] == "0" ? "" : form["Qualificationlist"];
                        string MartialStatus = form["MaritalStatuslist"] == "0" ? "" : form["MaritalStatuslist"];
                        string Gender = form["Genderlist"] == "0" ? "" : form["Genderlist"];

                        if (FuncStruct != "" && FuncStruct != null)
                        {
                            var val = db.FuncStruct.Find(int.Parse(FuncStruct));
                            ListCriteria.FuncStruct = val;
                        }

                        if (ExpFilter != "" && ExpFilter != null)
                        {
                            var val = db.LookupValue.Find(int.Parse(ExpFilter));
                            ListCriteria.ExpFilter = val;
                        }
                        if (RangeFilter != "" && RangeFilter != null)
                        {
                            var val = db.LookupValue.Find(int.Parse(RangeFilter));
                            ListCriteria.RangeFilter = val;
                        }
                        if (SpecialCategory != "" && SpecialCategory != null)
                        {
                            var val = db.LookupValue.Find(int.Parse(SpecialCategory));
                            ListCriteria.SpecialCategory = val;
                        }
                        if (Category != "" && Category != null)
                        {
                            var val = db.LookupValue.Find(int.Parse(Category));
                            ListCriteria.Category = val;
                        }
                        if (MartialStatus != "" && MartialStatus != null)
                        {
                            var val = db.LookupValue.Find(int.Parse(MartialStatus));
                            ListCriteria.MaritalStatus = val;
                        }
                        if (Gender != "" && Gender != null)
                        {
                            var val = db.LookupValue.Find(int.Parse(Gender));
                            ListCriteria.Gender = val;
                        }

                        List<int> ids = null;
                        List<Skill> SkillObj = new List<Skill>();
                        List<Qualification> QualObj = new List<Qualification>();
                        if (Skill != null && Skill != "0" && Skill != "false")
                        {
                            ids = Utility.StringIdsToListIds(Skill);
                            foreach (var ca in ids)
                            {
                                var OBJ_val = db.Skill.Find(ca);
                                SkillObj.Add(OBJ_val);
                                ListCriteria.Skill = SkillObj;
                            }
                        }

                        if (Qualification != null && Qualification != "0" && Qualification != "false")
                        {
                            ids = Utility.StringIdsToListIds(Qualification);
                            foreach (var ca in ids)
                            {
                                var OBJ_val = db.Qualification.Find(ca);
                                QualObj.Add(OBJ_val);
                                ListCriteria.Qualification = QualObj;
                            }
                        }

                        if (ModelState.IsValid)
                        {
                            ListCriteria.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId };

                            ShortlistingCriteria ShortlistingCr = new ShortlistingCriteria()
                            {
                                AgeFrom = ListCriteria.AgeFrom,
                                AgeTo = ListCriteria.AgeTo,
                                Category = ListCriteria.Category,
                                CriteriaName = ListCriteria.CriteriaName,
                                ExpFilter = ListCriteria.ExpFilter,
                                ExpYearFrom = ListCriteria.ExpYearFrom,
                                ExpYearTo = ListCriteria.ExpYearTo,
                                FuncStruct = ListCriteria.FuncStruct,
                                Gender = ListCriteria.Gender,
                                MaritalStatus = ListCriteria.MaritalStatus,
                                Narration = ListCriteria.Narration,
                                NoOfVacancies = ListCriteria.NoOfVacancies,
                                Qualification = ListCriteria.Qualification,
                                RangeFilter = ListCriteria.RangeFilter,
                                RelaxationAge = ListCriteria.RelaxationAge,
                                Skill = ListCriteria.Skill,
                                SpecialCategory = ListCriteria.SpecialCategory,
                                DBTrack = ListCriteria.DBTrack
                            };


                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.ShortlistingCriteria.Add(ShortlistingCr);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Recruitment/Recruitment", null, db.ChangeTracker, ShortlistingCr.DBTrack);
                                //DT_ShortlistingCriteria DT_ShortlistingCr = (DT_ShortlistingCriteria)rtn_Obj;
                                //DT_ShortlistingCr.Category_Id = ShortlistingCr.Category == null ? 0 : ShortlistingCr.Category.Id;
                                //DT_ShortlistingCr.ExpFilter_Id = ShortlistingCr.ExpFilter == null ? 0 : ShortlistingCr.ExpFilter.Id;
                                //DT_ShortlistingCr.FuncStruct_Id = ShortlistingCr.FuncStruct == null ? 0 : ShortlistingCr.FuncStruct.Id;
                                //DT_ShortlistingCr.Gender_Id = ShortlistingCr.Gender == null ? 0 : ShortlistingCr.Gender.Id;
                                //DT_ShortlistingCr.MaritalStatus_Id = ShortlistingCr.MaritalStatus == null ? 0 : ShortlistingCr.MaritalStatus.Id;
                                //DT_ShortlistingCr.RangeFilter_Id = ShortlistingCr.RangeFilter == null ? 0 : ShortlistingCr.RangeFilter.Id;
                                //DT_ShortlistingCr.SpecialCategory_Id = ShortlistingCr.SpecialCategory == null ? 0 : ShortlistingCr.SpecialCategory.Id;

                                //db.Create(DT_ShortlistingCr);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("Data Saved successfully");
                                return Json(new Utility.JsonReturnClass { Id = ShortlistingCr.Id, Val = ShortlistingCr.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { add.Id, add.FullAddress,  }, JsonRequestBehavior.AllowGet);
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
                            // return Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        }
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Create", new { concurrencyError = true, id = ListCriteria.Id });
                    }
                    catch (Exception e)
                    {
                        Msg.Add(e.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
        public class Shortilistinginfo
        {
            public int  FuncStruct_Id { get; set; }
            public string  FuncStruct_val { get; set; }

        }
        public class QualificationDetails
        {
            public int Qualification_id { get; set; }
            public string Qualification_details { get; set; }
        }
        public class skillDetails
        {
            public int Skill_id { get; set; }
            public string Skill_details { get; set; }
        }
        [HttpPost]
        public ActionResult Edit(int data)
        {
            //var Q = db.RecruitJoinParaProcessResult
            //     .Include(e => e.RecruitJoiningPara)
            List<Shortilistinginfo> return_data = new List<Shortilistinginfo>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ShortlistingCriteria
                    .Include(e => e.Category)
                    .Include(e => e.ExpFilter)
                    .Include(e => e.FuncStruct)
                    .Include(e => e.Gender)
                    .Include(e => e.MaritalStatus)
                    .Include(e => e.Qualification)
                    .Include(e => e.Skill)
                    .Include(e => e.SpecialCategory)

            .Where(e => e.Id == data).Select
            (e => new
            {
                CriteriaName = e.CriteriaName == null ? "" : e.CriteriaName,
                FuncStructList = e.FuncStruct,
                txtNoOfVacancies = e.NoOfVacancies == null ? 0 : e.NoOfVacancies,
                ExpFilterList = e.ExpFilter.Id == null ? 0 : e.ExpFilter.Id,
                txtExpYearFrom = e.ExpYearFrom == null ? 0 : e.ExpYearFrom,
                txtExpYearTo = e.ExpYearTo == null ? 0 : e.ExpYearTo,
                Skilllist1 = e.Skill,
                RangeFilterList = e.RangeFilter == null ? 0 : e.RangeFilter.Id,
                txtAgeFrom = e.AgeFrom,
                txtAgeTo = e.AgeTo,
                SpecialCategoryList = e.SpecialCategory == null ? 0 : e.SpecialCategory.Id,
                CategoryList = e.Category == null ? 0 : e.Category.Id,
                GenderList = e.Gender == null ? 0 : e.Gender.Id,
                MaritalStatusList = e.MaritalStatus == null ? 0 : e.MaritalStatus.Id,
                txtRelaxationAge = e.RelaxationAge,
                txtNarration = e.Narration,
                Action = e.DBTrack.Action
            }).ToList();

                var ad_data = db.ShortlistingCriteria
                     .Include(e => e.Category)
                     .Include(e => e.ExpFilter)
                     .Include(e => e.FuncStruct)
                     .Include(e => e.FuncStruct.Job)
                     .Include(e => e.Gender)
                     .Include(e => e.MaritalStatus)
                     .Include(e => e.Qualification)
                     .Include(e => e.Skill)
                     .Include(e => e.SpecialCategory)
                      .Where(e => e.Id == data)
                     .SingleOrDefault();
                //{
                //    Qualification_Id = e.Qualification.Select(a => a.Id.ToString()),
                //    Qualification_val = e.Qualification.Select(a => a.FullDetails),
                //    Skill_Id = e.Skill.Select(a => a.Id.ToString()),
                //    Skill_val = e.Skill.Select(a => a.FullDetails),
                //    FuncStruct_Id = e.FuncStruct.Id.ToString().ToArray(),
                //    FuncStruct_val = e.FuncStruct.FullDetails.ToString().ToArray()

                //}).ToList();

                string[] para = { };
                List<QualificationDetails> Qualifiaction_list = new List<QualificationDetails>();
                List<skillDetails> Skill_list = new List<skillDetails>();
                if (ad_data != null)
                {
                    foreach (var ca in ad_data.Qualification)
                    {
                        Qualifiaction_list.Add(new QualificationDetails
                        {
                            Qualification_id = ca.Id,
                            Qualification_details = ca.FullDetails
                        });

                    }

                    foreach (var ca in ad_data.Skill)
                    {
                        Skill_list.Add(new skillDetails
                        {
                            Skill_id = ca.Id,
                            Skill_details = ca.FullDetails
                        });

                    }

                }

                if (ad_data != null)
                {
                    //foreach (var ca in ad_data)
                    //{
                    return_data.Add(new Shortilistinginfo
                    {
                        // Qualification_Id = ca.Qualification == null ? para : ca.Qualification.Select(a => a.Id.ToString()).ToArray(),
                        // Qualification_val = ca.Qualification == null ? para : ca.Qualification.Select(a => a.FullDetails).ToArray(),
                        // Skill_Id = ca.Skill == null ? para : ca.Skill.Select(a => a.Id.ToString()).ToArray(),
                        // Skill_val = ca.Skill == null ? para : ca.Skill.Select(a => a.FullDetails).ToArray(),
                        FuncStruct_Id = ad_data.FuncStruct.Id,
                        //FuncStruct_Id = ca.FuncStruct == null ? para : (ca.FuncStruct.Id.ToString()).ToArray(),
                        FuncStruct_val = ad_data.FuncStruct.FullDetails,

                    });

                    // }

                }


                var Corp = db.ShortlistingCriteria.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, Qualifiaction_list, "", Skill_list, return_data, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        //[HttpPost]
        //public async Task<ActionResult> EditSave(ShortlistingCriteria c, int data, FormCollection form) // Edit submit
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        { 
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;

        //            string FuncStruct = form["FuncStructList"] == "0" ? "" : form["FuncStructList"];
        //            string ExpFilter = form["ExpFilterList"] == "0" ? "" : form["ExpFilterList"];
        //            string RangeFilter = form["RangeFilterList"] == "0" ? "" : form["RangeFilterList"];
        //            string SpecialCategory = form["SpecialCategoryList"] == "0" ? "" : form["SpecialCategoryList"];
        //            string Category = form["CategoryList"] == "0" ? "" : form["CategoryList"];
        //            string Skill = form["SkillList"] == "0" ? "" : form["SkillList"];
        //            string Qualification = form["QualificationList"] == "0" ? "" : form["QualificationList"];

        //            if (FuncStruct != "" && FuncStruct != null)
        //            {
        //                var val = db.FuncStruct.Find(int.Parse(FuncStruct));
        //                c.FuncStruct = val;
        //            }

        //            if (ExpFilter != "" && ExpFilter != null)
        //            {
        //                var val = db.LookupValue.Find(int.Parse(ExpFilter));
        //                c.ExpFilter = val;
        //            }
        //            if (RangeFilter != "" && RangeFilter != null)
        //            {
        //                var val = db.LookupValue.Find(int.Parse(RangeFilter));
        //                c.RangeFilter = val;
        //            }
        //            if (SpecialCategory != "" && SpecialCategory != null)
        //            {
        //                var val = db.LookupValue.Find(int.Parse(SpecialCategory));
        //                c.SpecialCategory = val;
        //            }
        //            if (Category != "" && Category != null)
        //            {
        //                var val = db.LookupValue.Find(int.Parse(Category));
        //                c.Category = val;
        //            }

        //            List<int> ids = null;
        //            List<Skill> SkillObj = new List<Skill>();
        //            List<Qualification> QualObj = new List<Qualification>();
        //            if (Skill != null && Skill != "0" && Skill != "false")
        //            {
        //                ids = Utility.StringIdsToListIds(Skill);
        //                foreach (var ca in ids)
        //                {
        //                    var OBJ_val = db.Skill.Find(ca);
        //                    SkillObj.Add(OBJ_val);
        //                    c.Skill = SkillObj;
        //                }
        //            }

        //            if (Qualification != null && Qualification != "0" && Qualification != "false")
        //            {
        //                ids = Utility.StringIdsToListIds(Qualification);
        //                foreach (var ca in ids)
        //                {
        //                    var OBJ_val = db.Qualification.Find(ca);
        //                    QualObj.Add(OBJ_val);
        //                    c.Qualification = QualObj;
        //                }
        //            }
        //            var db_Data = db.ShortlistingCriteria
        //                 .Include(e => e.Category)
        //                 .Include(e => e.ExpFilter)
        //                 .Include(e => e.FuncStruct)
        //                 .Include(e => e.Gender)
        //                 .Include(e => e.MaritalStatus)
        //                 .Include(e => e.Qualification)
        //                 .Include(e => e.Skill)
        //                 .Include(e => e.SpecialCategory)
        //                 .Where(e => e.Id == data).SingleOrDefault();


        //            if (Auth == false)
        //            {


        //                if (ModelState.IsValid)
        //                {



        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                    {
        //                        db.ShortlistingCriteria.Attach(db_Data);
        //                        db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = db_Data.RowVersion;
        //                        db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

        //                        var Curr_Lookup = db.ShortlistingCriteria.Find(data);
        //                        TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
        //                        db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

        //                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                        {

        //                            ShortlistingCriteria blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.ShortlistingCriteria.Where(e => e.Id == data)
        //                                    .Include(e => e.Category)
        //                                    .Include(e => e.ExpFilter)
        //                                    .Include(e => e.FuncStruct)
        //                                    .Include(e => e.Gender)
        //                                    .Include(e => e.MaritalStatus)
        //                                    .Include(e => e.Qualification)
        //                                    .Include(e => e.Skill)
        //                                    .Include(e => e.SpecialCategory).SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            c.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };
        //                            ShortlistingCriteria lk = new ShortlistingCriteria
        //                            {
        //                                AgeFrom = c.AgeFrom,
        //                                AgeTo = c.AgeTo,
        //                                Category = c.Category,
        //                                CriteriaName = c.CriteriaName,
        //                                ExpFilter = c.ExpFilter,
        //                                ExpYearFrom = c.ExpYearFrom,
        //                                ExpYearTo = c.ExpYearTo,
        //                                FuncStruct = c.FuncStruct,
        //                                Gender = c.Gender,
        //                                MaritalStatus = c.MaritalStatus,
        //                                Narration = c.Narration,
        //                                NoOfVacancies = c.NoOfVacancies,
        //                                Qualification = c.Qualification,
        //                                RangeFilter = c.RangeFilter,
        //                                RelaxationAge = c.RelaxationAge,
        //                                Skill = c.Skill,
        //                                SpecialCategory = c.SpecialCategory,
        //                                DBTrack = c.DBTrack

        //                            };


        //                            db.ShortlistingCriteria.Attach(lk);
        //                            db.Entry(lk).State = System.Data.Entity.EntityState.Modified;
        //                            db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                            // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

        //                            // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


        //                            using (var context = new DataBaseContext())
        //                            {

        //                                var obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                                DT_ShortlistingCriteria DT_Corp = (DT_ShortlistingCriteria)obj;
        //                                DT_Corp.Category_Id = c.Category == null ? 0 : c.Category.Id;
        //                                DT_Corp.ExpFilter_Id = c.ExpFilter == null ? 0 : c.ExpFilter.Id;
        //                                DT_Corp.FuncStruct_Id = c.FuncStruct == null ? 0 : c.FuncStruct.Id;
        //                                DT_Corp.Gender_Id = c.Gender == null ? 0 : c.Gender.Id;
        //                                DT_Corp.MaritalStatus_Id = c.MaritalStatus == null ? 0 : c.MaritalStatus.Id;
        //                                DT_Corp.RangeFilter_Id = c.RangeFilter == null ? 0 : c.RangeFilter.Id;
        //                                DT_Corp.SpecialCategory_Id = c.SpecialCategory == null ? 0 : c.SpecialCategory.Id;
        //                                db.Create(DT_Corp);
        //                                db.SaveChanges();
        //                            }
        //                            await db.SaveChangesAsync();
        //                            ts.Complete();

        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = lk.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        }
        //                    }



        //                    List<string> MsgB = new List<string>();
        //                    MsgB.Add("Record modified by another user.So refresh it and try to save again.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);

        //                    //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    ShortlistingCriteria blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    ShortlistingCriteria Old_Corp = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.ShortlistingCriteria.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }
        //                    c.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };

        //                    if (TempData["RowVersion"] == null)
        //                    {
        //                        TempData["RowVersion"] = blog.RowVersion;
        //                    }

        //                    ShortlistingCriteria corp = new ShortlistingCriteria()
        //                    {
        //                        AgeFrom = c.AgeFrom,
        //                        AgeTo = c.AgeTo,
        //                        Category = c.Category,
        //                        CriteriaName = c.CriteriaName,
        //                        ExpFilter = c.ExpFilter,
        //                        ExpYearFrom = c.ExpYearFrom,
        //                        ExpYearTo = c.ExpYearTo,
        //                        FuncStruct = c.FuncStruct,
        //                        Gender = c.Gender,
        //                        MaritalStatus = c.MaritalStatus,
        //                        Narration = c.Narration,
        //                        NoOfVacancies = c.NoOfVacancies,
        //                        Qualification = c.Qualification,
        //                        RangeFilter = c.RangeFilter,
        //                        RelaxationAge = c.RelaxationAge,
        //                        Skill = c.Skill,
        //                        SpecialCategory = c.SpecialCategory,
        //                        DBTrack = c.DBTrack,
        //                        RowVersion = (Byte[])TempData["RowVersion"]
        //                    };


        //                    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        //var obj = DBTrackFile.ModifiedDataHistory("Recruitment/Recruitment", "M", blog, corp, "ShortlistingCriteria", c.DBTrack);
        //                        var obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                        // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                        Old_Corp = context.ShortlistingCriteria.Where(e => e.Id == data)
        //                            .Include(e => e.Category)
        //                            .Include(e => e.ExpFilter)
        //                            .Include(e => e.FuncStruct)
        //                            .Include(e => e.Gender)
        //                            .Include(e => e.MaritalStatus)
        //                            .Include(e => e.Qualification)
        //                            .Include(e => e.Skill)
        //                            .Include(e => e.SpecialCategory)
        //                            .SingleOrDefault();
        //                        DT_ShortlistingCriteria DT_Corp = (DT_ShortlistingCriteria)obj;
        //                        DT_Corp.Category_Id = c.Category == null ? 0 : c.Category.Id;
        //                        DT_Corp.ExpFilter_Id = c.ExpFilter == null ? 0 : c.ExpFilter.Id;
        //                        DT_Corp.FuncStruct_Id = c.FuncStruct == null ? 0 : c.FuncStruct.Id;
        //                        DT_Corp.Gender_Id = c.Gender == null ? 0 : c.Gender.Id;
        //                        DT_Corp.MaritalStatus_Id = c.MaritalStatus == null ? 0 : c.MaritalStatus.Id;
        //                        DT_Corp.RangeFilter_Id = c.RangeFilter == null ? 0 : c.RangeFilter.Id;
        //                        DT_Corp.SpecialCategory_Id = c.SpecialCategory == null ? 0 : c.SpecialCategory.Id;
        //                        db.Create(DT_Corp);
        //                        db.SaveChanges();
        //                    }
        //                    blog.DBTrack = c.DBTrack;
        //                    db.ShortlistingCriteria.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    Msg.Add("Record Updated Successfully.");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }

        //            }
        //        }
        //        catch (DbUpdateConcurrencyException ex)
        //        {
        //            var entry = ex.Entries.Single();
        //            var clientValues = (ShortlistingCriteria)entry.Entity;
        //            var databaseEntry = entry.GetDatabaseValues();
        //            if (databaseEntry == null)
        //            {
        //                Msg.Add(ex.Message);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //            }
        //            else
        //            {
        //                var databaseValues = (ShortlistingCriteria)databaseEntry.ToObject();
        //                c.RowVersion = databaseValues.RowVersion;

        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Msg.Add(e.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //        return View();
        //    }
        //}

        [HttpPost]
        public async Task<ActionResult> EditSave(ShortlistingCriteria c, int data, FormCollection form) // Edit submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    string FuncStruct = form["FuncStructList"] == "0" ? "" : form["FuncStructList"];
                    string ExpFilter = form["ExpFilterlist1"] == "0" ? "" : form["ExpFilterlist1"];
                    string RangeFilter = form["RangeFilterList"] == "0" ? "" : form["RangeFilterList"];
                    string SpecialCategory = form["SpecialCategoryList"] == "0" ? "" : form["SpecialCategoryList"];
                    string Category = form["CategorylistNew"] == "0" ? "" : form["CategorylistNew"];
                    string Skill = form["SkillList"] == "0" ? "" : form["SkillList"];
                    string Qualification = form["QualificationList"] == "0" ? "" : form["QualificationList"];
                    string Gender = form["Genderlist"] == "0" ? "" : form["Genderlist"];
                    string MaritalStatus = form["MaritalStatuslist"] == "0" ? "" : form["MaritalStatuslist"];


                    if (FuncStruct != "" && FuncStruct != null)
                    {
                        var val = db.FuncStruct.Find(int.Parse(FuncStruct));
                        c.FuncStruct = val;
                    }

                    if (ExpFilter != "" && ExpFilter != null)
                    {
                        var val = db.LookupValue.Find(int.Parse(ExpFilter));
                        c.ExpFilter = val;
                    }

                    if (RangeFilter != "" && RangeFilter != null)
                    {
                        var val = db.LookupValue.Find(int.Parse(RangeFilter));
                        c.RangeFilter = val;
                    }
                    if (SpecialCategory != "" && SpecialCategory != null)
                    {
                        var val = db.LookupValue.Find(int.Parse(SpecialCategory));
                        c.SpecialCategory = val;
                    }
                    if (Category != "" && Category != null)
                    {
                        var val = db.LookupValue.Find(int.Parse(Category));
                        c.Category = val;
                    }
                    if (Gender != "" && Gender != null)
                    {
                        var val = db.LookupValue.Find(int.Parse(Gender));
                        c.Gender = val;
                    }
                    if (MaritalStatus != "" && MaritalStatus != null)
                    {
                        var val = db.LookupValue.Find(int.Parse(MaritalStatus));
                        c.MaritalStatus = val;
                    }


                    var db_Data = db.ShortlistingCriteria

                      .Include(e => e.FuncStruct)
                      .Include(e => e.Qualification)
                      .Include(e => e.Skill)
                      .Where(e => e.Id == data).SingleOrDefault();
                    List<Skill> SkillList = new List<Skill>();

                    if (Skill != null)
                    {
                        var idss = Utility.StringIdsToListIds(Skill);
                        foreach (var ca in idss)
                        {
                            var Skills = db.Skill.Find(ca);
                            SkillList.Add(Skills);
                            db_Data.Skill = SkillList;
                        }
                    }
                    else
                    {
                        db_Data.Skill = null;
                    }

                    List<Qualification> QualificationList = new List<Qualification>();

                    if (Qualification != null)
                    {
                        var idss = Utility.StringIdsToListIds(Qualification);
                        foreach (var ca in idss)
                        {
                            var Qualifications = db.Qualification.Find(ca);
                            QualificationList.Add(Qualifications);
                            db_Data.Qualification = QualificationList;
                        }
                    }
                    else
                    {
                        db_Data.Qualification = null;
                    }


                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {

                                db.ShortlistingCriteria.Attach(db_Data);
                                db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = db_Data.RowVersion;
                                db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

                                ShortlistingCriteria blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.ShortlistingCriteria.Where(e => e.Id == data)
                                        .Include(e => e.Category)
                                        .Include(e => e.ExpFilter)
                                        .Include(e => e.FuncStruct)
                                        .Include(e => e.Gender)
                                        .Include(e => e.MaritalStatus)
                                        .Include(e => e.Qualification)
                                        .Include(e => e.Skill)
                                        .Include(e => e.SpecialCategory).SingleOrDefault();
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


                                if (ExpFilter != null)
                                {
                                    if (ExpFilter != "")
                                    {
                                        var val = db.LookupValue.Find(int.Parse(ExpFilter));
                                        c.ExpFilter = val;

                                        var type = db.ShortlistingCriteria.Include(e => e.ExpFilter).Where(e => e.Id == data).SingleOrDefault();
                                        IList<ShortlistingCriteria> typedetails = null;
                                        if (type.ExpFilter != null)
                                        {
                                            typedetails = db.ShortlistingCriteria.Where(x => x.ExpFilter.Id == type.ExpFilter.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.ShortlistingCriteria.Where(x => x.Id == data).ToList();
                                        }
                                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                        foreach (var s in typedetails)
                                        {
                                            s.ExpFilter = c.ExpFilter;
                                            db.ShortlistingCriteria.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    else
                                    {
                                        var BusiTypeDetails = db.ShortlistingCriteria.Include(e => e.ExpFilter).Where(x => x.Id == data).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.ExpFilter = null;
                                            db.ShortlistingCriteria.Attach(s);
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
                                    var BusiTypeDetails = db.ShortlistingCriteria.Include(e => e.ExpFilter).Where(x => x.Id == data).ToList();
                                    foreach (var s in BusiTypeDetails)
                                    {
                                        s.ExpFilter = null;
                                        db.ShortlistingCriteria.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }

                                /////

                                if (RangeFilter != null)
                                {
                                    if (RangeFilter != "")
                                    {
                                        var val = db.LookupValue.Find(int.Parse(RangeFilter));
                                        c.RangeFilter = val;

                                        var type = db.ShortlistingCriteria.Include(e => e.RangeFilter).Where(e => e.Id == data).SingleOrDefault();
                                        IList<ShortlistingCriteria> typedetails = null;
                                        if (type.RangeFilter != null)
                                        {
                                            typedetails = db.ShortlistingCriteria.Where(x => x.RangeFilter.Id == type.RangeFilter.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.ShortlistingCriteria.Where(x => x.Id == data).ToList();
                                        }
                                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                        foreach (var s in typedetails)
                                        {
                                            s.RangeFilter = c.RangeFilter;
                                            db.ShortlistingCriteria.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    else
                                    {
                                        var BusiTypeDetails = db.ShortlistingCriteria.Include(e => e.RangeFilter).Where(x => x.Id == data).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.RangeFilter = null;
                                            db.ShortlistingCriteria.Attach(s);
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
                                    var BusiTypeDetails = db.ShortlistingCriteria.Include(e => e.RangeFilter).Where(x => x.Id == data).ToList();
                                    foreach (var s in BusiTypeDetails)
                                    {
                                        s.RangeFilter = null;
                                        db.ShortlistingCriteria.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }

                                ////////

                                if (SpecialCategory != null)
                                {
                                    if (SpecialCategory != "")
                                    {
                                        var val = db.LookupValue.Find(int.Parse(SpecialCategory));
                                        c.SpecialCategory = val;

                                        var type = db.ShortlistingCriteria.Include(e => e.SpecialCategory).Where(e => e.Id == data).SingleOrDefault();
                                        IList<ShortlistingCriteria> typedetails = null;
                                        if (type.SpecialCategory != null)
                                        {
                                            typedetails = db.ShortlistingCriteria.Where(x => x.SpecialCategory.Id == type.SpecialCategory.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.ShortlistingCriteria.Where(x => x.Id == data).ToList();
                                        }
                                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                        foreach (var s in typedetails)
                                        {
                                            s.SpecialCategory = c.SpecialCategory;
                                            db.ShortlistingCriteria.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    else
                                    {
                                        var BusiTypeDetails = db.ShortlistingCriteria.Include(e => e.SpecialCategory).Where(x => x.Id == data).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.SpecialCategory = null;
                                            db.ShortlistingCriteria.Attach(s);
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
                                    var BusiTypeDetails = db.ShortlistingCriteria.Include(e => e.SpecialCategory).Where(x => x.Id == data).ToList();
                                    foreach (var s in BusiTypeDetails)
                                    {
                                        s.SpecialCategory = null;
                                        db.ShortlistingCriteria.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }
                                ////

                                if (Category != null)
                                {
                                    if (Category != "")
                                    {
                                        var val = db.LookupValue.Find(int.Parse(Category));
                                        c.Category = val;

                                        var type = db.ShortlistingCriteria.Include(e => e.Category).Where(e => e.Id == data).SingleOrDefault();
                                        IList<ShortlistingCriteria> typedetails = null;
                                        if (type.Category != null)
                                        {
                                            typedetails = db.ShortlistingCriteria.Where(x => x.Category.Id == type.Category.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.ShortlistingCriteria.Where(x => x.Id == data).ToList();
                                        }
                                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                        foreach (var s in typedetails)
                                        {
                                            s.Category = c.Category;
                                            db.ShortlistingCriteria.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    else
                                    {
                                        var BusiTypeDetails = db.ShortlistingCriteria.Include(e => e.Category).Where(x => x.Id == data).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.Category = null;
                                            db.ShortlistingCriteria.Attach(s);
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
                                    var BusiTypeDetails = db.ShortlistingCriteria.Include(e => e.Category).Where(x => x.Id == data).ToList();
                                    foreach (var s in BusiTypeDetails)
                                    {
                                        s.Category = null;
                                        db.ShortlistingCriteria.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }

                                //////
                                if (Gender != null)
                                {
                                    if (Gender != "")
                                    {
                                        var val = db.LookupValue.Find(int.Parse(Gender));
                                        c.Gender = val;

                                        var type = db.ShortlistingCriteria.Include(e => e.Gender).Where(e => e.Id == data).SingleOrDefault();
                                        IList<ShortlistingCriteria> typedetails = null;
                                        if (type.Gender != null)
                                        {
                                            typedetails = db.ShortlistingCriteria.Where(x => x.Gender.Id == type.Gender.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.ShortlistingCriteria.Where(x => x.Id == data).ToList();
                                        }
                                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                        foreach (var s in typedetails)
                                        {
                                            s.Gender = c.Gender;
                                            db.ShortlistingCriteria.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    else
                                    {
                                        var BusiTypeDetails = db.ShortlistingCriteria.Include(e => e.Gender).Where(x => x.Id == data).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.Gender = null;
                                            db.ShortlistingCriteria.Attach(s);
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
                                    var BusiTypeDetails = db.ShortlistingCriteria.Include(e => e.Gender).Where(x => x.Id == data).ToList();
                                    foreach (var s in BusiTypeDetails)
                                    {
                                        s.Gender = null;
                                        db.ShortlistingCriteria.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }
                                /////////

                                //////
                                if (MaritalStatus != null)
                                {
                                    if (MaritalStatus != "")
                                    {
                                        var val = db.LookupValue.Find(int.Parse(MaritalStatus));
                                        c.Gender = val;

                                        var type = db.ShortlistingCriteria.Include(e => e.MaritalStatus).Where(e => e.Id == data).SingleOrDefault();
                                        IList<ShortlistingCriteria> typedetails = null;
                                        if (type.MaritalStatus != null)
                                        {
                                            typedetails = db.ShortlistingCriteria.Where(x => x.MaritalStatus.Id == type.MaritalStatus.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.ShortlistingCriteria.Where(x => x.Id == data).ToList();
                                        }
                                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                        foreach (var s in typedetails)
                                        {
                                            s.MaritalStatus = c.MaritalStatus;
                                            db.ShortlistingCriteria.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    else
                                    {
                                        var BusiTypeDetails = db.ShortlistingCriteria.Include(e => e.MaritalStatus).Where(x => x.Id == data).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.MaritalStatus = null;
                                            db.ShortlistingCriteria.Attach(s);
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
                                    var BusiTypeDetails = db.ShortlistingCriteria.Include(e => e.MaritalStatus).Where(x => x.Id == data).ToList();
                                    foreach (var s in BusiTypeDetails)
                                    {
                                        s.MaritalStatus = null;
                                        db.ShortlistingCriteria.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }
                                /////////
                                if (FuncStruct != null)
                                {
                                    if (FuncStruct != "")
                                    {
                                        var val = db.FuncStruct.Find(int.Parse(FuncStruct));
                                        c.FuncStruct = val;

                                        var type = db.ShortlistingCriteria.Include(e => e.FuncStruct).Where(e => e.Id == data).SingleOrDefault();
                                        IList<ShortlistingCriteria> typedetails = null;
                                        if (type.FuncStruct != null)
                                        {
                                            typedetails = db.ShortlistingCriteria.Where(x => x.FuncStruct.Id == type.FuncStruct.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.ShortlistingCriteria.Where(x => x.Id == data).ToList();
                                        }
                                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                        foreach (var s in typedetails)
                                        {
                                            s.FuncStruct = c.FuncStruct;
                                            db.ShortlistingCriteria.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    else
                                    {
                                        var BusiTypeDetails = db.ShortlistingCriteria.Include(e => e.FuncStruct).Where(x => x.Id == data).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.FuncStruct = null;
                                            db.ShortlistingCriteria.Attach(s);
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
                                    var BusiTypeDetails = db.ShortlistingCriteria.Include(e => e.FuncStruct).Where(x => x.Id == data).ToList();
                                    foreach (var s in BusiTypeDetails)
                                    {
                                        s.FuncStruct = null;
                                        db.ShortlistingCriteria.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }
                                ///////////

                                var CurCorp = db.ShortlistingCriteria.Find(data);
                                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    ShortlistingCriteria corp = new ShortlistingCriteria()
                                    {
                                        AgeFrom = c.AgeFrom,
                                        AgeTo = c.AgeTo,
                                        CriteriaName = c.CriteriaName,
                                        ExpYearFrom = c.ExpYearFrom,
                                        ExpYearTo = c.ExpYearTo,
                                        FuncStruct = c.FuncStruct,
                                        Narration = c.Narration,
                                        NoOfVacancies = c.NoOfVacancies,
                                        RelaxationAge = c.RelaxationAge,
                                        Qualification = c.Qualification,
                                        Skill = c.Skill,


                                        Id = data,
                                        DBTrack = c.DBTrack
                                    };


                                    db.ShortlistingCriteria.Attach(corp);
                                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                }

                                using (var context = new DataBaseContext())
                                {
                                    var obj = DBTrackFile.DBTrackSave("Recruitment/Recruitment", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                    //DT_ShortlistingCriteria DT_Corp = (DT_ShortlistingCriteria)obj;
                                    //DT_Corp.FuncStruct_Id = blog.FuncStruct == null ? 0 : blog.FuncStruct.Id;
                                    //DT_Corp.Category_Id = blog.Category == null ? 0 : blog.Category.Id;
                                    //DT_Corp.ExpFilter_Id = blog.ExpFilter == null ? 0 : blog.ExpFilter.Id;

                                    //DT_Corp.Gender_Id = blog.Gender == null ? 0 : blog.Gender.Id;
                                    //DT_Corp.MaritalStatus_Id = blog.MaritalStatus == null ? 0 : blog.MaritalStatus.Id;
                                    //DT_Corp.RangeFilter_Id = blog.RangeFilter == null ? 0 : blog.RangeFilter.Id;
                                    //DT_Corp.SpecialCategory_Id = blog.SpecialCategory == null ? 0 : blog.SpecialCategory.Id;

                                    //db.Create(DT_Corp);
                                    db.SaveChanges();
                                }
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

                            ShortlistingCriteria blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            ShortlistingCriteria Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.ShortlistingCriteria.Where(e => e.Id == data)
                                        .Include(e => e.Category)
                                        .Include(e => e.ExpFilter)
                                        .Include(e => e.FuncStruct)
                                        .Include(e => e.Gender)
                                        .Include(e => e.MaritalStatus)
                                        .Include(e => e.Qualification)
                                        .Include(e => e.Skill)
                                        .Include(e => e.SpecialCategory).SingleOrDefault();
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

                            ShortlistingCriteria corp = new ShortlistingCriteria()
                            {
                                AgeFrom = c.AgeFrom,
                                AgeTo = c.AgeTo,
                                CriteriaName = c.CriteriaName,
                                ExpYearFrom = c.ExpYearFrom,
                                ExpYearTo = c.ExpYearTo,
                                FuncStruct = c.FuncStruct,
                                Narration = c.Narration,
                                NoOfVacancies = c.NoOfVacancies,
                                RelaxationAge = c.RelaxationAge,

                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Recruitement/Recruitement", "M", blog, corp, "ShortlistingCriteria", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.ShortlistingCriteria.Where(e => e.Id == data)
                                        .Include(e => e.Category)
                                        .Include(e => e.ExpFilter)
                                        .Include(e => e.FuncStruct)
                                        .Include(e => e.Gender)
                                        .Include(e => e.MaritalStatus)
                                        .Include(e => e.Qualification)
                                        .Include(e => e.Skill)
                                        .Include(e => e.SpecialCategory).SingleOrDefault();

                                DT_ShortlistingCriteria DT_Corp = (DT_ShortlistingCriteria)obj;
                                DT_Corp.FuncStruct_Id = blog.FuncStruct == null ? 0 : blog.FuncStruct.Id;
                                DT_Corp.Category_Id = blog.Category == null ? 0 : blog.Category.Id;
                                DT_Corp.ExpFilter_Id = blog.ExpFilter == null ? 0 : blog.ExpFilter.Id;

                                DT_Corp.Gender_Id = blog.Gender == null ? 0 : blog.Gender.Id;
                                DT_Corp.MaritalStatus_Id = blog.MaritalStatus == null ? 0 : blog.MaritalStatus.Id;
                                DT_Corp.RangeFilter_Id = blog.RangeFilter == null ? 0 : blog.RangeFilter.Id;
                                DT_Corp.SpecialCategory_Id = blog.SpecialCategory == null ? 0 : blog.SpecialCategory.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.ShortlistingCriteria.Attach(blog);
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
                    var clientValues = (ShortlistingCriteria)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (ShortlistingCriteria)databaseEntry.ToObject();
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

        public ActionResult GetQualificationLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Qualification.Include(e => e.QualificationType).ToList();
                IEnumerable<Qualification> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Qualification.ToList().Where(d => d.FullDetails.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetFuncStructLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.FuncStruct.Include(e => e.JobPosition)
                    .Include(e => e.Job)
                    .ToList();
                IEnumerable<FuncStruct> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.FuncStruct.ToList().Where(d => d.FullDetails.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }





        [HttpPost]
        public ActionResult GetSkillDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Skill.Include(e => e.SkillType).ToList();
                IEnumerable<Skill> all;
                if (!string.IsNullOrEmpty(data))
                {

                    all = db.Skill.Include(a => a.Id).Include(a => a.FullDetails).ToList().Where(d => d.FullDetails.Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}