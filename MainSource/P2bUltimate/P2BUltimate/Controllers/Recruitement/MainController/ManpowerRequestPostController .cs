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
using System.Reflection;
using P2BUltimate.Security;
using Recruitment;
namespace P2BUltimate.Controllers.Recruitement.MainController
{
    [AuthoriseManger]
    public class ManpowerRequestPostController : Controller
    {

        //  private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Recruitement/MainView/ManpowerRequestPost/Index.cshtml");
        }


        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(ManpowerRequestPost p, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string JobPosition = form["JobPositionMList"] == "0" ? "" : form["JobPositionMList"];
                string Qualification = form["QualificationListM"] == "0" ? "" : form["QualificationListM"];
                string Skill = form["SkillList"] == "0" ? "" : form["SkillList"];
                string CategoryPost = form["CategoryPostList"] == "0" ? "" : form["CategoryPostList"];
                string CategorySplPost = form["CategorySplPostList"] == "0" ? "" : form["CategorySplPostList"];
                string ExpFilter_Id = form["ExpFilterList_DDL"] == "0" ? "" : form["ExpFilterList_DDL"];
                string RangeFilter_Id = form["RangeFilterList_DDL"] == "0" ? "" : form["RangeFilterList_DDL"];
                string Gender = form["GenderList_DDL"] == "0" ? "" : form["GenderList_DDL"];
                string MStatus = form["MaritalStatusList_DDL"] == "0" ? "" : form["MaritalStatusList_DDL"];
                string PostSourceTypeList_DDL = form["PostSourceTypeList_DDL"] == "0" ? "" : form["PostSourceTypeList_DDL"];

                Calendar recruitcalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToString().ToUpper() == "RECRUITMENTCALENDAR").SingleOrDefault();


                RecruitYearlyCalendar recruityearlycalendar = db.RecruitYearlyCalendar
                                                              .Include(e => e.RecruitmentCalendar)
                                                              .Include(e => e.ManpowerRequestPost)
                                                            .Where(e => e.RecruitmentCalendar.Id == recruitcalendar.Id).SingleOrDefault();

                List<ManpowerRequestPost> OFAT = new List<ManpowerRequestPost>();
                List<String> Msg = new List<String>();
                try
                {
                    //p.FuncStruct.JobPosition = null;
                    //List<FuncStruct> job = new List<FuncStruct>();
                    //string val2 = form["JobPositionMList"];

                    //if (val2 != null && val2 != "")
                    //{
                    //    var ids = Utility.StringIdsToListIds(val2);
                    //    foreach (var ca in ids)
                    //    {
                    //        var OBJ_val = db.FuncStruct.Find(ca);
                    //        job.Add(OBJ_val);
                    //        p.FuncStruct = job;
                    //    }
                    //}

                    if (JobPosition != null)
                    {
                        if (JobPosition != "")
                        {
                            int ContId = Convert.ToInt32(JobPosition);
                            var val = db.FuncStruct.Include(e => e.Job).Include(e => e.JobPosition)
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            p.FuncStruct = val;
                        }
                    }


                    p.Qualification = null;
                    List<Qualification> OBJ = new List<Qualification>();
                    string Values = form["QualificationListM"];

                    if (Values != null && Values != "")
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.Qualification.Find(ca);
                            OBJ.Add(OBJ_val);
                            p.Qualification = OBJ;
                        }
                    }

                    p.Skill = null;
                    List<Skill> sk = new List<Skill>();
                    string Val1 = form["SkillList"];

                    if (Val1 != null && Val1 != "")
                    {
                        var ids = Utility.StringIdsToListIds(Val1);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.Skill.Find(ca);
                            sk.Add(OBJ_val);
                            p.Skill = sk;
                        }
                    }

                    p.CategoryPost = null;
                    List<CategoryPost> cp = new List<CategoryPost>();
                    string val3 = form["CategoryPostList"];

                    if (val3 != null && val3 != "")
                    {
                        var ids = Utility.StringIdsToListIds(val3);
                        foreach (var ca in ids)
                        {
                            var p_val = db.CategoryPost.Find(ca);
                            cp.Add(p_val);
                            p.CategoryPost = cp;
                        }
                    }

                    p.CategorySplPost = null;
                    List<CategorySplPost> csp = new List<CategorySplPost>();
                    string val4 = form["CategorySplPostList"];

                    if (val4 != null && val4 != "")
                    {
                        var ids = Utility.StringIdsToListIds(val4);
                        foreach (var ca in ids)
                        {
                            var p_val = db.CategorySplPost.Find(ca);
                            csp.Add(p_val);
                            p.CategorySplPost = csp;
                        }
                    }

                    if (ExpFilter_Id != null && ExpFilter_Id != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(ExpFilter_Id));
                        p.ExpFilter = val;
                    }

                    if (RangeFilter_Id != null && RangeFilter_Id != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(RangeFilter_Id));
                        p.RangeFilter = val;
                    }

                    if (MStatus != null && MStatus != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(MStatus));
                        p.MaritalStatus = val;
                    }

                    if (Gender != null && Gender != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Gender));
                        p.Gender = val;
                    }

                    if (PostSourceTypeList_DDL != null && PostSourceTypeList_DDL != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(PostSourceTypeList_DDL));
                        p.PostSourceType = val;
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            //if (db.PostDetails.Any(o => o.FuncStruct.JobPosition == p.FuncStruct.JobPosition))
                            //{
                            //    Msg.Add("Code Already Exists.");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}

                            p.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };                     //???????

                            ManpowerRequestPost Postdetails = new ManpowerRequestPost()
                            {
                                FuncStruct = p.FuncStruct,                     //true?//
                                RequestVacancies = p.RequestVacancies,
                                ExpYearFrom = p.ExpYearFrom,
                                ExpYearTo = p.ExpYearTo,
                                AgeFrom = p.AgeFrom,
                                AgeTo = p.AgeTo,
                                Narration = p.Narration == null ? "" : p.Narration.ToString(),
                                RangeFilter = p.RangeFilter,
                                ExpFilter = p.ExpFilter,
                                CategoryPost = p.CategoryPost,
                                CategorySplPost = p.CategorySplPost,
                                Skill = p.Skill,
                                Gender = p.Gender,
                                MaritalStatus = p.MaritalStatus,
                                Qualification = p.Qualification,
                                PostSourceType = p.PostSourceType,
                                PostCode = p.PostCode,
                                PostRequestDate = p.PostRequestDate,
                                DBTrack = p.DBTrack
                            };

                            db.ManpowerRequestPost.Add(Postdetails);
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, p.DBTrack);
                            //DT_ManpowerRequestPost DT_Post = (DT_PostDetails)rtn_Obj;
                            //DT_Post.Gender_Id = p.Gender == null ? 0 : p.Gender.Id;
                            //DT_Post.MaritalStatus_Id = p.MaritalStatus == null ? 0 : p.MaritalStatus.Id;
                            //DT_Post.ExpFilter_Id = p.ExpFilter == null ? 0 : p.ExpFilter.Id;
                            //DT_Post.RangeFilter_Id = p.RangeFilter == null ? 0 : p.RangeFilter.Id;

                            //db.Create(DT_Post);
                            db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);
                            OFAT.Add(Postdetails);
                            if (recruityearlycalendar.ManpowerRequestPost != null)
                            {
                                OFAT.AddRange(recruityearlycalendar.ManpowerRequestPost);
                            }
                            recruityearlycalendar.ManpowerRequestPost = OFAT;
                            db.RecruitYearlyCalendar.Attach(recruityearlycalendar);
                            db.Entry(recruityearlycalendar).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(recruityearlycalendar).State = System.Data.Entity.EntityState.Detached;

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
                        Msg.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        //return this.Json(new { msg = errorMsg });
                    }
                }
                catch (Exception e)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = e.Message,
                        ExceptionStackTrace = e.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public class PostDetailsD
        {
            public Array JobPositionDetails_Id { get; set; }
            public Array JobPositionDetails_val { get; set; }
            public Array QualificationDetails_Id { get; set; }
            public Array QualificationDetails_val { get; set; }
            public Array SkillDetails_Id { get; set; }
            public Array SkillDetails_val { get; set; }
            public Array CategoryDetails_Id { get; set; }
            public Array CategoryDetails_val { get; set; }
            public Array CategorySplDetails_Id { get; set; }
            public Array CategorySplDetails_val { get; set; }
        }

        public class JobPositionDetailsC
        {
            public Array JobPositionDetails_Id { get; set; }
            public Array JobPositionDetails_val { get; set; }

        }

        public class QualificationDetailsC
        {
            public Array QualificationDetails_Id { get; set; }
            public Array QualificationDetails_val { get; set; }

        }

        public class SkillDetailsC
        {
            public Array SkillDetails_Id { get; set; }
            public Array SkillDetails_val { get; set; }

        }
        public class CategoryDetailsC
        {
            public Array CategoryDetails_Id { get; set; }
            public Array CategoryDetails_val { get; set; }

        }

        public class CategorySplDetailsC
        {
            public Array CategorySplDetails_Id { get; set; }
            public Array CategorySplDetails_val { get; set; }

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
                var Q = db.ManpowerRequestPost.Include(e => e.FuncStruct)
                                                       .Include(e => e.FuncStruct.JobPosition)
                                                       .Include(e => e.FuncStruct.Job)
                                                       .Include(e => e.ExpFilter)
                                                       .Include(e => e.RangeFilter)
                                                       .Include(e => e.Qualification)
                                                       .Include(e => e.Skill)
                                                       .Include(e => e.Gender)
                                                       .Include(e => e.MaritalStatus)
                                                       .Include(e => e.CategoryPost)
                                                        .Include(e => e.PostSourceType)
                                                       .Include(e => e.CategorySplPost).Where(e => e.Id == data).Select
                    (e => new
                    {
                        // JobPosition_Id = e.FuncStruct.JobPosition.Id==null? 0 : e.FuncStruct.JobPosition.Id ,
                        Vaccancy = e.RequestVacancies,
                        ExpFilter_Id = e.ExpFilter.Id == null ? 0 : e.ExpFilter.Id,
                        ExpYearFrom = e.ExpYearFrom,
                        ExpYearTo = e.ExpYearTo,
                        RangeFilter_Id = e.RangeFilter.Id == null ? 0 : e.RangeFilter.Id,
                        AgeFrom = e.AgeFrom,
                        AgeTo = e.AgeTo,
                        Gender_Id = e.Gender.Id == null ? 0 : e.Gender.Id,
                        PostSourceType = e.PostSourceType.Id == null ? 0 : e.PostSourceType.Id,
                        MaritalStatus_Id = e.MaritalStatus.Id == null ? 0 : e.MaritalStatus.Id,
                        Narration = e.Narration,
                        PostRequestDate = e.PostRequestDate,
                        PostCode = e.PostCode,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.ManpowerRequestPost.Include(e => e.FuncStruct)
                                                       .Include(e => e.FuncStruct.JobPosition)
                                                       .Include(e => e.FuncStruct.Job)
                                                       .Include(e => e.ExpFilter)
                                                       .Include(e => e.RangeFilter)
                                                       .Include(e => e.Qualification)
                                                       .Include(e => e.Skill)
                                                       .Include(e => e.Gender)
                                                       .Include(e => e.MaritalStatus)
                                                       .Include(e => e.CategoryPost)
                                                       .Include(e => e.CategorySplPost)
                                                       .Include(e => e.CategoryPost.Select(e1 => e1.Category))
                                                       .Include(e => e.CategorySplPost.Select(e1 => e1.SpecialCategory))
                                                       .Where(e => e.Id == data).Select
                    (e => new
                    {
                        JobPos_FullAddress = e.FuncStruct == null ? "" : e.FuncStruct.FullDetails,
                        JobPos_Id = e.FuncStruct.Id == null ? "" : e.FuncStruct.Id.ToString(),


                        QualificationDetails_Id = e.Qualification.Select(b => b.Id.ToString()),
                        QualificationDetails_val = e.Qualification.Select(b => b.FullDetails),
                        SkillDetails_Id = e.Skill.Select(b => b.Id.ToString()),
                        SkillDetails_val = e.Skill.Select(b => b.Name),
                        CategoryDetails_Id = e.CategoryPost.Select(b => b.Id.ToString()),
                        CategoryDetails_val = e.CategoryPost.Select(b => b.FullDetails),
                        CategorySplDetails_Id = e.CategorySplPost.Select(b => b.Id.ToString()),
                        CategorySplDetails_val = e.CategorySplPost.Select(b => b.FullDetails),

                    }).ToList();

                //foreach (var ca in add_data)
                //{


                //}


                //List<PostDetailsD> pst =new List<PostDetailsD>();
                //var a = db.PostDetails.Include(e => e.FuncStruct)
                //                        .Include(e => e.FuncStruct.JobPosition)
                //                        .Include(e => e.FuncStruct.Job)
                //                        .Include(e => e.ExpFilter)
                //                        .Include(e => e.RangeFilter)
                //                        .Include(e => e.Qualification)
                //                        .Include(e => e.Skill)
                //                        .Include(e => e.Gender)
                //                        .Include(e => e.MaritalStatus)
                //                        .Include(e => e.CategoryPost.Select(e1=>e1.Category))
                //                        .Include(e => e.CategorySplPost.Select(e1=>e1.SpecialCategory)).Where(e => e.Id == data).ToList();
                //foreach (var ca in a)
                //{
                //    pst.Add(new PostDetailsD
                //    {

                //        QualificationDetails_Id = ca.Qualification.Select(e => e.Id.ToString()).ToArray(),
                //        QualificationDetails_val = ca.Qualification.Select(e => e.FullDetails).ToArray(),
                //        SkillDetails_Id = ca.Skill.Select(e => e.Id.ToString()).ToArray(),
                //        SkillDetails_val = ca.Skill.Select(e => e.Name).ToArray(),
                //        CategoryDetails_Id = ca.CategoryPost.Select(e => e.Id.ToString()).ToArray(),
                //        CategoryDetails_val = ca.CategoryPost.Select(e => e.FullDetails).ToArray(),
                //        CategorySplDetails_Id = ca.CategorySplPost.Select(e => e.Id.ToString()).ToArray(),
                //        CategorySplDetails_val = ca.CategorySplPost.Select(e => e.FullDetails).ToArray(),
                //     });
                //}

                //var W = db.DT_Corporate
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,
                //         Code = e.Code == null ? "" : e.Code,
                //         Name = e.Name == null ? "" : e.Name,
                //         BusinessType_Val = e.BusinessType_Id == 0 ? "" : db.LookupValue
                //                    .Where(x => x.Id == e.BusinessType_Id)
                //                    .Select(x => x.LookupVal).FirstOrDefault(),

                //         Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
                //         Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.ManpowerRequestPost.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        //[HttpPost]
        //public async Task<ActionResult> EditSave(PostDetails p, int data, FormCollection form) // Edit submit
        //{
        //    List<String> Msg = new List<String>();
        //    try
        //    {

        //        string JobP = form["JobPositionMList"] == "0" ? "" : form["JobPositionMList"];   
        //        string Qual = form["QualificationListM"] == "0" ? "" : form["QualificationListM"];
        //        string SkillL = form["SkillList"] == "0" ? "" : form["SkillList"];
        //        string CatPost = form["CategoryPostList"] == "0" ? "" : form["CategoryPostList"];
        //        string CatSplPost = form["CategoryPostList"] == "0" ? "" : form["CategoryPostList"];
        //        string ExpCriteria = form["ExpFilterList_DDL"] == "0" ? "" : form["ExpFilterList_DDL"];
        //        string AgeCriteria = form["RangeFilterList_DDL"] == "0" ? "" : form["RangeFilterList_DDL"];
        //        string Gender = form["GenderList_DDL"] == "0" ? "" : form["GenderList_DDL"];
        //        string MStatus = form["MaritalStatusList_DDL"] == "0" ? "" : form["MaritalStatusList_DDL"];
        //        //  bool Auth = form["Autho_Action"] == "" ? false : true;
        //        bool Auth = form["Autho_Allow"] == "true" ? true : false;


        //        if (ExpCriteria != null)
        //        {
        //            if (ExpCriteria != "")
        //            {
        //                var val = db.LookupValue.Find(int.Parse(ExpCriteria));
        //                p.ExpFilter = val;
        //            }
        //        }

        //        if (AgeCriteria != null)
        //        {
        //            if (AgeCriteria != "")
        //            {
        //                var val = db.LookupValue.Find(int.Parse(AgeCriteria));
        //                p.RangeFilter = val;
        //            }
        //        }

        //        if (MStatus != null)
        //        {
        //            if (MStatus != "")
        //            {
        //                var val = db.LookupValue.Find(int.Parse(MStatus));
        //                p.MaritalStatus = val;
        //            }
        //        }

        //        if (Gender != null)
        //        {
        //            if (Gender != "")
        //            {
        //                var val = db.LookupValue.Find(int.Parse(Gender));
        //                p.Gender = val;
        //            }
        //        }


        //    if (Qual != null)
        //    {
        //        List<int> IDs = Qual.Split(',').Select(e => int.Parse(e)).ToList();
        //        foreach (var k in IDs)
        //        {
        //                var value = db.Qualification.Find(k);
        //                p.Qualification = new List<Qualification>();
        //                p.Qualification.Add(value);
        //        }
        //     }

        //    if (SkillL != null)
        //    {
        //        List<int> IDs = SkillL.Split(',').Select(e => int.Parse(e)).ToList();
        //        foreach (var k in IDs)
        //        {
        //            var value = db.Skill.Find(k);
        //            p.Skill = new List<Skill>();
        //            p.Skill.Add(value);
        //        }
        //    }
        //    if (JobP != null)
        //    {
        //        if (JobP != "")
        //        {
        //            int ContId = Convert.ToInt32(JobP);
        //            var val = db.FuncStruct.Include(e => e.Job).Include(e => e.JobPosition)
        //                                .Where(e => e.Id == ContId).SingleOrDefault();
        //            p.FuncStruct = val;
        //        }
        //    }

        //    if (CatSplPost != null)
        //    {
        //        List<int> IDs = CatSplPost.Split(',').Select(e => int.Parse(e)).ToList();
        //        foreach (var k in IDs)
        //        {
        //            var value = db.CategorySplPost.Find(k);
        //            p.CategorySplPost = new List<CategorySplPost>();
        //            p.CategorySplPost.Add(value);
        //        }
        //    }

        //    if (CatPost != null)
        //    {
        //        List<int> IDs = CatPost.Split(',').Select(e => int.Parse(e)).ToList();
        //        foreach (var k in IDs)
        //        {
        //            var value = db.CategoryPost.Find(k);
        //            p.CategoryPost = new List<CategoryPost>();
        //            p.CategoryPost.Add(value);
        //        }
        //    }

        //        if (Auth == false)
        //        {  
        //            if (ModelState.IsValid)
        //            {    
        //                //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    PostDetails blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.PostDetails.Where(e => e.Id == data)
        //                            .Include(e=>e.FuncStruct)
        //                            .Include(e=>e.FuncStruct.Job)
        //                            .Include(e => e.FuncStruct.JobPosition)
        //                                           .Include(e => e.ExpFilter)
        //                                           .Include(e => e.RangeFilter)
        //                                           .Include(e => e.Qualification)
        //                                           .Include(e => e.Skill)
        //                                           .Include(e => e.Gender)
        //                                           .Include(e => e.MaritalStatus)
        //                                           .Include(e => e.CategoryPost)
        //                                           .Include(e => e.CategorySplPost).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }

        //                    p.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };

        //                    //int a = EditS(Corp, Addrs, ContactDetails, data, p, p.DBTrack);
        //                   // int a = EditS(JobP, ExpCriteria, AgeCriteria, Gender, MStatus, data, p, p.DBTrack);
        //                    int a = EditS(JobP, ExpCriteria, AgeCriteria, Gender, MStatus, data, p, p.DBTrack);       

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        //c.Id = data;

        //                        /// DBTrackFile.DBTrackSave("Core/P2b.Global", "M", blog, c, "Corporate", c.DBTrack);
        //                        //PropertyInfo[] fi = null;
        //                        //Dictionary<string, object> rt = new Dictionary<string, object>();
        //                        //fi = c.GetType().GetProperties();
        //                        ////foreach (var Prop in fi)
        //                        ////{
        //                        ////    if (Prop.Name != "Id" && Prop.Name != "DBTrack" && Prop.Name != "RowVersion")
        //                        ////    {
        //                        ////        rt.Add(Prop.Name, Prop.GetValue(c));
        //                        ////    }
        //                        ////}
        //                        //rt = blog.DetailedCompare(c);
        //                        //rt.Add("Orig_Id", c.Id);
        //                        //rt.Add("Action", "M");
        //                        //rt.Add("DBTrack", c.DBTrack);
        //                        //rt.Add("RowVersion", c.RowVersion);
        //                        //var aa = DBTrackFile.GetInstance("Core/P2b.Global", "DT_" + "Corporate", rt);
        //                        //DT_Corporate d = (DT_Corporate)aa;
        //                        //db.DT_Corporate.Add(d);
        //                        //db.SaveChanges();

        //                        //To save data in history table 
        //                        //var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, c, "Corporate", c.DBTrack);
        //                        //DT_Corporate DT_Corp = (DT_Corporate)Obj;
        //                        //db.DT_Corporate.Add(DT_Corp);
        //                        //db.SaveChanges();\


        //                        var obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", originalBlogValues, db.ChangeTracker, p.DBTrack);
        //                        DT_PostDetails DT_Post = (DT_PostDetails)obj;
        //                        DT_Post.ExpFilter_Id = blog.ExpFilter == null ? 0 : blog.ExpFilter.Id;
        //                        DT_Post.RangeFilter_Id = blog.RangeFilter == null ? 0 : blog.RangeFilter.Id;
        //                        DT_Post.Gender_Id = blog.Gender == null ? 0 : blog.Gender.Id;                                      // the declaratn for lookup is remain 
        //                        DT_Post.MaritalStatus_Id = blog.MaritalStatus == null ? 0 : blog.MaritalStatus.Id;
        //                        DT_Post.FuncStruct_Id = blog.FuncStruct == null ? 0 : blog.FuncStruct.Id;
        //                        db.Create(DT_Post);
        //                        db.SaveChanges();
        //                    }
        //                    await db.SaveChangesAsync();
        //                    ts.Complete();

        //                    Msg.Add("Record Updated Successfully.");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }
        //                Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //            }
        //        }
        //        else
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {

        //                 PostDetails blog = null; // to retrieve old data
        //                DbPropertyValues originalBlogValues = null;
        //                PostDetails Old_Post = null;

        //                using (var context = new DataBaseContext())
        //                {
        //                    blog = context.PostDetails.Where(e => e.Id == data).SingleOrDefault();
        //                    originalBlogValues = context.Entry(blog).OriginalValues;
        //                }
        //                p.DBTrack = new DBTrack
        //                {
        //                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                    Action = "M",
        //                    IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                    ModifiedBy = SessionManager.UserName,
        //                    ModifiedOn = DateTime.Now
        //                };

        //                if (TempData["RowVersion"] == null)
        //                {
        //                    TempData["RowVersion"] = blog.RowVersion;
        //                }

        //                PostDetails pd = new PostDetails()
        //                {
        //                    NoOfVacancies = p.NoOfVacancies,
        //                    ExpYearFrom = p.ExpYearFrom,
        //                    ExpYearTo = p.ExpYearTo,
        //                    AgeFrom = p.AgeFrom,
        //                    AgeTo = p.AgeTo,
        //                    Narration = p.Narration == null ? "" : p.Narration.ToString(),              //FuncStruct.JobPosition
        //                    ExpFilter = p.ExpFilter,
        //                    CategoryPost = p.CategoryPost,
        //                    CategorySplPost = p.CategorySplPost,
        //                    Skill = p.Skill,
        //                    Gender = p.Gender,
        //                    MaritalStatus = p.MaritalStatus,
        //                    Qualification = p.Qualification,
        //                    Id = data,
        //                    DBTrack = p.DBTrack,
        //                    RowVersion = (Byte[])TempData["RowVersion"]
        //                };


        //                //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                using (var context = new DataBaseContext())
        //                {
        //                    var obj = DBTrackFile.ModifiedDataHistory("Recruitement/Recruitement", "M", blog, pd, "PostDetails", p.DBTrack);
        //                    // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                    Old_Post = context.PostDetails.Where(e => e.Id == data).Include(e => e.FuncStruct.JobPosition)
        //                                           .Include(e => e.ExpFilter)
        //                                           .Include(e => e.RangeFilter)
        //                                           .Include(e => e.Qualification)
        //                                           .Include(e => e.Skill)
        //                                           .Include(e => e.Gender)
        //                                           .Include(e => e.MaritalStatus)
        //                                           .Include(e => e.CategoryPost)
        //                                           .Include(e => e.CategorySplPost).SingleOrDefault();
        //                    DT_PostDetails DT_PostD = (DT_PostDetails)obj;
        //                    DT_PostD.FuncStruct_Id = DBTrackFile.ValCompare(DT_PostD.FuncStruct_Id, p.FuncStruct.JobPosition);    //wrong
        //                    DT_PostD.ExpFilter_Id = DBTrackFile.ValCompare(DT_PostD.ExpFilter_Id, p.ExpFilter); 

        //                    DT_PostD.Gender_Id = DBTrackFile.ValCompare(DT_PostD. Gender_Id, p.  Gender);
        //                    DT_PostD.MaritalStatus_Id = DBTrackFile.ValCompare(DT_PostD.MaritalStatus_Id, p.MaritalStatus);
        //                    db.Create(DT_PostD);
        //                    //db.SaveChanges();
        //                }
        //                blog.DBTrack = p.DBTrack;
        //                db.PostDetails.Attach(blog);
        //                db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                db.SaveChanges();
        //                ts.Complete();
        //                Msg.Add("Record Updated Successfully.");
        //                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            }

        //        }
        //    }
        //    catch (DbUpdateConcurrencyException ex)
        //    {
        //        var entry = ex.Entries.Single();
        //        var clientValues = (Corporate)entry.Entity;
        //        var databaseEntry = entry.GetDatabaseValues();
        //        if (databaseEntry == null)
        //        {
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //        }
        //        else
        //        {
        //            var databaseValues = (Corporate)databaseEntry.ToObject();
        //            p.RowVersion = databaseValues.RowVersion;

        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Msg.Add(e.Message);
        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //    }
        //    return View();

        //}

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

                            PostDetails post = db.PostDetails.Include(e => e.FuncStruct.JobPosition)
                                                      .Include(e => e.ExpFilter)
                                                      .Include(e => e.RangeFilter)
                                                      .Include(e => e.Qualification)
                                                      .Include(e => e.Skill)
                                                      .Include(e => e.Gender)
                                                      .Include(e => e.MaritalStatus)
                                                      .Include(e => e.CategoryPost)
                                                      .Include(e => e.CategorySplPost).FirstOrDefault(e => e.Id == auth_id);

                            post.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = post.DBTrack.ModifiedBy != null ? post.DBTrack.ModifiedBy : null,
                                CreatedBy = post.DBTrack.CreatedBy != null ? post.DBTrack.CreatedBy : null,
                                CreatedOn = post.DBTrack.CreatedOn != null ? post.DBTrack.CreatedOn : null,
                                IsModified = post.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.PostDetails.Attach(post);
                            db.Entry(post).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(post).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, post.DBTrack);
                            DT_PostDetails DT_Post = (DT_PostDetails)rtn_Obj;
                            DT_Post.Gender_Id = post.Gender == null ? 0 : post.Gender.Id;
                            DT_Post.MaritalStatus_Id = post.MaritalStatus == null ? 0 : post.MaritalStatus.Id;
                            DT_Post.ExpFilter_Id = post.ExpFilter == null ? 0 : post.ExpFilter.Id;
                            DT_Post.RangeFilter_Id = post.RangeFilter == null ? 0 : post.RangeFilter.Id;

                            db.Create(DT_Post);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = DT_Post.Id, Val = post.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {
                        PostDetails Old_Post = db.PostDetails.Include(e => e.FuncStruct.JobPosition)
                                                     .Include(e => e.ExpFilter)
                                                     .Include(e => e.RangeFilter)
                                                     .Include(e => e.Qualification)
                                                     .Include(e => e.Skill)
                                                     .Include(e => e.Gender)
                                                     .Include(e => e.MaritalStatus)
                                                     .Include(e => e.CategoryPost)
                                                     .Include(e => e.CategorySplPost).Where(e => e.Id == auth_id).SingleOrDefault();

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

                        DT_PostDetails Curr_Post = db.DT_PostDetails
                                                   .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                   .OrderByDescending(e => e.Id)
                                                   .FirstOrDefault();

                        if (Curr_Post != null)
                        {
                            PostDetails post = new PostDetails();
                            string ExpFilter_Id = Curr_Post.ExpFilter_Id == null ? null : Curr_Post.ExpFilter_Id.ToString();
                            string RangeFilter_Id = Curr_Post.RangeFilter_Id == null ? null : Curr_Post.RangeFilter_Id.ToString();
                            string Gender = Curr_Post.Gender_Id == null ? null : Curr_Post.Gender_Id.ToString();                                      // the declaratn for lookup is remain 
                            string MStatus = Curr_Post.MaritalStatus_Id == null ? null : Curr_Post.MaritalStatus_Id.ToString();
                            string FuncStruct_Id = Curr_Post.FuncStruct_Id == null ? null : Curr_Post.FuncStruct_Id.ToString();                      //wrong
                            string Qual = null;
                            string JobP = null;
                            string CatPost = null;
                            string SkillL = null;
                            string CatSplPost = null;
                            //string Corp = Curr_Post.BusinessType_Id == null ? null : Curr_Post.BusinessType_Id.ToString();
                            //string Addrs = Curr_Post.Address_Id == null ? null : Curr_Post.Address_Id.ToString();
                            //string ContactDetails = Curr_Post.ContactDetails_Id == null ? null : Curr_Post.ContactDetails_Id.ToString();
                            //corp.Name = Curr_Post.Name == null ? Old_Post.Name : Curr_Post.Name;
                            //corp.Code = Curr_Post.Code == null ? Old_Post.Code : Curr_Post.Code;
                            //      corp.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        post.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_Post.DBTrack.CreatedBy == null ? null : Old_Post.DBTrack.CreatedBy,
                                            CreatedOn = Old_Post.DBTrack.CreatedOn == null ? null : Old_Post.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_Post.DBTrack.ModifiedBy == null ? null : Old_Post.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_Post.DBTrack.ModifiedOn == null ? null : Old_Post.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        // int a = EditS(Corp, Addrs, ContactDetails, auth_id, corp, corp.DBTrack);
                                        int a = EditS(JobP, ExpFilter_Id, RangeFilter_Id, Gender, MStatus, auth_id, post, post.DBTrack);
                                        //int a = EditS(JobP, ExpFilter_Id, RangeFilter_Id, Qua, Skills, Gender_Id, MaritalStatus_Id, Cat, SplCat, auth_id, corp, corp.DBTrack);
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
                                        post.RowVersion = databaseValues.RowVersion;
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

                            var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, corp.DBTrack);
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

        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    List<string> Msg = new List<string>();
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        IEnumerable<PostDetails> PostDetails = null;
        //        if (gp.IsAutho == true)
        //        {
        //            PostDetails = db.PostDetails.Include(e => e.Qualification)
        //                 .Include(e => e.Skill )
        //                 .Include(e => e.CategoryPost )
        //                 .Include(e => e.CategorySplPost )
        //                 .Include(e=> e.RangeFilter)
        //                 .Include(e=>e.ExpFilter)
        //                 .Include(e=>e.Gender)
        //                 .Include(e=>e.MaritalStatus)
        //                .Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            PostDetails = db.PostDetails.Include(e => e.Qualification)
        //                 .Include(e => e.Skill)
        //                 .Include(e => e.CategoryPost)
        //                 .Include(e => e.CategorySplPost)
        //                 .Include(e => e.RangeFilter)
        //                 .Include(e => e.ExpFilter)
        //                 .Include(e => e.Gender)
        //                 .Include(e => e.MaritalStatus)
        //                .Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }

        //        IEnumerable<PostDetails> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = PostDetails;
        //            if (gp.searchOper.Equals("eq"))
        //            {                          
        //                    jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
        //                        || (e.NoOfVacancies.ToString().Contains(gp.searchString))
        //                        ).Select(a => new { a.Id, a.NoOfVacancies, a.FuncStruct.JobPosition }).ToList();
        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.NoOfVacancies, a.FuncStruct.JobPosition ,a.ExpFilter != null ? Convert.ToString(a.ExpFilter.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = PostDetails;
        //            Func<PostDetails, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "JobPosition" ? c.FuncStruct.JobPosition.ToString()  :
        //                                 gp.sidx == "NoOfVacancies" ? c.NoOfVacancies.ToString() :
        //                                  gp.sidx == "BusinessType" ? c.ExpFilter.LookupVal : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.NoOfVacancies), Convert.ToString(a.FuncStruct.JobPosition), a.ExpFilter != null ? Convert.ToString(a.ExpFilter.LookupVal) : "" }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.NoOfVacancies), Convert.ToString(a.FuncStruct.JobPosition), a.ExpFilter != null ? Convert.ToString(a.ExpFilter.LookupVal) : "" }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.NoOfVacancies), Convert.ToString(a.FuncStruct.JobPosition), a.ExpFilter != null ? Convert.ToString(a.ExpFilter.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = PostDetails.Count();
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
            List<string> Msg = new List<string>();

            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<ManpowerRequestPost> PostDetails = null;
                if (gp.IsAutho == true)
                {
                    PostDetails = db.ManpowerRequestPost.Include(e => e.FuncStruct)
                                                  .Include(e => e.FuncStruct.Job)
                                                  .Include(e => e.FuncStruct.JobPosition)
                                                  //.Include(e => e.ExpFilter)
                                                  //.Include(e => e.RangeFilter)
                                                  //.Include(e => e.Qualification)
                                                  //.Include(e => e.Skill)
                                                  //.Include(e => e.Gender)
                                                  //.Include(e => e.MaritalStatus)
                                                  //.Include(e => e.CategoryPost)
                                                  .Include(e => e.PostSourceType)
                                                 // .Include(e => e.CategorySplPost)
                                                  .AsNoTracking().ToList();
                }
                else
                {
                    PostDetails = db.ManpowerRequestPost.Include(e => e.FuncStruct)
                                                  .Include(e => e.FuncStruct.Job)
                                                  .Include(e => e.FuncStruct.JobPosition)
                                                  //.Include(e => e.ExpFilter)
                                                  //.Include(e => e.RangeFilter)
                                                  //.Include(e => e.Qualification)
                                                  //.Include(e => e.Skill)
                                                  //.Include(e => e.Gender)
                                                  //.Include(e => e.MaritalStatus)
                                                  //.Include(e => e.CategoryPost)
                                                  .Include(e => e.PostSourceType)
                                                  //.Include(e => e.CategorySplPost)
                                                  .AsNoTracking().ToList();
                }

                IEnumerable<ManpowerRequestPost> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = PostDetails;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.PostSourceType != null  ? e.PostSourceType.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                                 || (e.FuncStruct.JobPosition.JobPositionDesc.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                 || (e.RequestVacancies.ToString().Contains(gp.searchString))
                                 || (e.PostRequestDate.Value.ToShortDateString().ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                 || (e.Id.ToString().Contains(gp.searchString))
                                 ).Select(a => new Object[] { a.PostSourceType == null ? "" : a.PostSourceType.LookupVal, a.FuncStruct.Job != null ? a.FuncStruct.Job.Name : "", a.RequestVacancies, a.PostRequestDate.Value.ToShortDateString(), a.Id }).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.PostSourceType == null ? "" : a.PostSourceType.LookupVal, a.FuncStruct.Job != null ? a.FuncStruct.Job.Name : "", a.RequestVacancies, a.PostRequestDate.Value.ToShortDateString(), a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = PostDetails;
                    Func<ManpowerRequestPost, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "SourceType" ? c.PostSourceType == null ? "" : c.PostSourceType.LookupVal :
                                            gp.sidx == "Jobdescription" ? c.FuncStruct.JobPosition.JobPositionDesc.ToString() :
                                            gp.sidx == "NoOfVacancies" ? c.RequestVacancies.ToString() :
                                            gp.sidx == "PostDate" ? c.PostRequestDate.Value.ToShortDateString() :
                                         "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.PostSourceType == null ? "" : a.PostSourceType.LookupVal, a.FuncStruct.Job != null ? a.FuncStruct.Job.Name : "", a.RequestVacancies, a.PostRequestDate.Value.ToShortDateString(), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.PostSourceType == null ? "" : a.PostSourceType.LookupVal, a.FuncStruct.Job != null ? a.FuncStruct.Job.Name : "", a.RequestVacancies, a.PostRequestDate.Value.ToShortDateString(), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.PostSourceType == null ? "" : a.PostSourceType.LookupVal, a.FuncStruct.Job != null ? a.FuncStruct.Job.Name : "", a.RequestVacancies, a.PostRequestDate.Value.ToShortDateString(), a.Id }).ToList();
                    }
                    totalRecords = PostDetails.Count();
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
                    ManpowerRequestPost Postdetails = db.ManpowerRequestPost.Include(e => e.FuncStruct.JobPosition)
                                                       .Include(e => e.FuncStruct.Job)
                                                       .Include(e => e.ExpFilter)
                                                       .Include(e => e.RangeFilter)
                                                       .Include(e => e.Qualification)
                                                       .Include(e => e.Skill)
                                                       .Include(e => e.Gender)
                                                       .Include(e => e.MaritalStatus)
                                                       .Include(e => e.CategoryPost)
                                                       .Include(e => e.PostSourceType)
                                                       .Include(e => e.CategorySplPost).Where(e => e.Id == data).SingleOrDefault();


                    //Qualification qua = Postdetails.Qualification;
                    //Skill skl = Postdetails.Skill;
                    //CategoryPost cp = Postdetails.CategoryPost;
                    //CategorySplPost csp = Postdetails.CategorySplPost;
                    LookupValue exp = Postdetails.ExpFilter;
                    LookupValue ag = Postdetails.RangeFilter;
                    LookupValue gen = Postdetails.Gender;
                    LookupValue ps = Postdetails.PostSourceType;
                    LookupValue marsts = Postdetails.MaritalStatus;
                    FuncStruct fun = Postdetails.FuncStruct;
                    //    Qualification qua = Postdetails.Qualification;
                    //ContactDetails conDet = corporates.ContactDetails;
                    //LookupValue val = corporates.BusinessType;
                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (Postdetails.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = Postdetails.DBTrack.CreatedBy != null ? Postdetails.DBTrack.CreatedBy : null,
                                CreatedOn = Postdetails.DBTrack.CreatedOn != null ? Postdetails.DBTrack.CreatedOn : null,
                                IsModified = Postdetails.DBTrack.IsModified == true ? true : false
                            };
                            Postdetails.DBTrack = dbT;
                            db.Entry(Postdetails).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, Postdetails.DBTrack);
                            DT_PostDetails DT_Post = (DT_PostDetails)rtn_Obj;
                            //DT_Post.ExpFilter_Id = Postdetails.ExpFilter == null ? 0 : Postdetails.ExpFilter.Id;
                            //DT_Post.RangeFilter_Id = Postdetails.RangeFilter == null ? 0 : Postdetails.RangeFilter.Id;
                            //DT_Post.Gender_Id = Postdetails.Gender == null ? 0 : Postdetails.Gender.Id;                                      // the declaratn for lookup is remain 
                            //DT_Post.MaritalStatus_Id = Postdetails.MaritalStatus == null ? 0 : Postdetails.MaritalStatus.Id;
                            //DT_Post.FuncStruct_Id = Postdetails.FuncStruct == null ? 0 : Postdetails.FuncStruct.Id;
                            db.Create(DT_Post);

                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        var selectedJobP = Postdetails.FuncStruct;
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            if (fun == null)
                            {
                                //if (selectedJobP != null)
                                //{
                                //    var corpRegion = new HashSet<int>(Postdetails.FuncStruct.JobPosition.Id.CompareTo();
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
                                    CreatedBy = Postdetails.DBTrack.CreatedBy != null ? Postdetails.DBTrack.CreatedBy : null,
                                    CreatedOn = Postdetails.DBTrack.CreatedOn != null ? Postdetails.DBTrack.CreatedOn : null,
                                    IsModified = Postdetails.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(Postdetails).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, dbT);
                                // var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                DT_PostDetails DT_Post = (DT_PostDetails)rtn_Obj;
                                DT_Post.ExpFilter_Id = exp == null ? 0 : exp.Id;
                                DT_Post.RangeFilter_Id = ag == null ? 0 : ag.Id;                                                             // the declaratn for lookup is remain 
                                DT_Post.Gender_Id = gen == null ? 0 : gen.Id;
                                DT_Post.MaritalStatus_Id = marsts == null ? 0 : marsts.Id;
                                DT_Post.FuncStruct_Id = fun == null ? 0 : 0;

                                db.Create(DT_Post);

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
                                Msg.Add("  Data removed.  ");                                                                                             // the original place 
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            if (fun.Job != null)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
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
        public ActionResult GetQualificationDetailLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Qualification.ToList();
                IEnumerable<Qualification> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Qualification.ToList().Where(d => d.QualificationShortName.Contains(data));

                }
                else
                {
                    //var data1 = db.QualificationDetails
                    //.Select(e => new
                    //{
                    //    value = "University :" + e.Qualification + ",Institute : " + e.University
                    //}).ToString();  
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }

                ////string univer = db.QualificationDetails.Include(a => a.University).ToString();
                ////string inst =  db.QualificationDetails.Include(a=>a.Institute).ToString();  
                //string ca121 = "University :"+fall.Select(a=>a.Institute) +",Institute :"+fall.Select(a=>a.University) ;  

                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetSkillLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Skill.ToList();
                IEnumerable<Skill> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Skill.ToList().Where(d => d.Name.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.Name }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.Name }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetCategoryDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.CategoryPost.Include(a => a.Category).ToList();
                IEnumerable<CategoryPost> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.CategoryPost.Include(a => a.Category).ToList();
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
        public ActionResult GetJobPositionDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.FuncStruct.Include(e => e.Job).Include(e => e.JobPosition).ToList();
                IEnumerable<FuncStruct> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.FuncStruct.Include(e => e.Job).Include(e => e.JobPosition).ToList().Where(d => d.FullDetails.Contains(data));                    // no full details 

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
        public ActionResult GetCategorySplDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.CategorySplPost.Include(a => a.SpecialCategory).ToList();
                IEnumerable<CategorySplPost> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.CategorySplPost.Include(a => a.SpecialCategory).ToList();

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



        public int EditS(string JobP, string ExpFilter_Id, string RangeFilter_Id, string Gender, string MStatus, int data, PostDetails p, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (JobP != null)
                {
                    if (JobP != "")
                    {
                        var val = db.FuncStruct.Find(int.Parse(JobP));
                        // var val = db.Awards.Where(e => e.Id == data).SingleOrDefault();

                        var r = (from ca in db.FuncStruct
                                 select new
                                 {
                                     Id = ca.Id,
                                     LookupVal = ca.JobPosition
                                 }).Where(e => e.Id == data).Distinct();

                        var type = db.PostDetails.Include(e => e.FuncStruct).Where(e => e.Id == data).SingleOrDefault();
                        IList<PostDetails> typedetails = null;
                        if (type.FuncStruct != null)
                        {
                            typedetails = db.PostDetails.Where(x => x.FuncStruct.Id == type.FuncStruct.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.PostDetails.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.FuncStruct = p.FuncStruct;
                            db.PostDetails.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.PostDetails.Include(e => e.FuncStruct).Include(e => e.FuncStruct.Job).Include(e => e.FuncStruct.JobPosition).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.FuncStruct = null;
                            db.PostDetails.Attach(s);
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
                    var BusiTypeDetails = db.PostDetails.Include(e => e.FuncStruct).Include(e => e.FuncStruct.Job).Include(e => e.FuncStruct.JobPosition).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.FuncStruct = null;
                        db.PostDetails.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                // End             

                if (ExpFilter_Id != null)
                {
                    if (ExpFilter_Id != "")
                    {
                        var val = db.Awards.Find(int.Parse(ExpFilter_Id));
                        // var val = db.Awards.Where(e => e.Id == data).SingleOrDefault();

                        var r = (from ca in db.Awards
                                 select new
                                 {
                                     Id = ca.Id,
                                     LookupVal = ca.Name
                                 }).Where(e => e.Id == data).Distinct();

                        var type = db.PostDetails.Include(e => e.ExpFilter).Where(e => e.Id == data).SingleOrDefault();
                        IList<PostDetails> typedetails = null;
                        if (type.ExpFilter != null)
                        {
                            typedetails = db.PostDetails.Where(x => x.ExpFilter.Id == type.ExpFilter.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.PostDetails.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.ExpFilter = p.ExpFilter;
                            db.PostDetails.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.PostDetails.Include(e => e.ExpFilter).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.ExpFilter = null;
                            db.PostDetails.Attach(s);
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
                    var BusiTypeDetails = db.PostDetails.Include(e => e.ExpFilter).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.ExpFilter = null;
                        db.PostDetails.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (RangeFilter_Id != null)
                {
                    if (RangeFilter_Id != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(RangeFilter_Id));
                        p.RangeFilter = val;

                        var type = db.PostDetails.Include(e => e.RangeFilter).Where(e => e.Id == data).SingleOrDefault();
                        IList<PostDetails> typedetails = null;
                        if (type.RangeFilter != null)
                        {
                            typedetails = db.PostDetails.Where(x => x.RangeFilter.Id == type.RangeFilter.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.PostDetails.Where(x => x.Id == data).ToList();
                        }
                        db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.RangeFilter = p.RangeFilter;
                            db.PostDetails.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            // await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.PostDetails.Include(e => e.RangeFilter).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.RangeFilter = null;
                            db.PostDetails.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            // await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var BusiTypeDetails = db.PostDetails.Include(e => e.RangeFilter).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.RangeFilter = null;
                        db.PostDetails.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //  await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                if (Gender != null)
                {
                    if (Gender != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Gender));
                        p.Gender = val;

                        var type = db.PostDetails.Include(e => e.Gender).Where(e => e.Id == data).SingleOrDefault();
                        IList<PostDetails> typedetails = null;
                        if (type.Gender != null)
                        {
                            typedetails = db.PostDetails.Where(x => x.Gender.Id == type.Gender.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.PostDetails.Where(x => x.Id == data).ToList();
                        }
                        db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.Gender = p.Gender;
                            db.PostDetails.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            // await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.PostDetails.Include(e => e.Gender).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.Gender = null;
                            db.PostDetails.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            // await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var BusiTypeDetails = db.PostDetails.Include(e => e.Gender).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.Gender = null;
                        db.PostDetails.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        // await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (MStatus != null)
                {
                    if (MStatus != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(MStatus));
                        p.MaritalStatus = val;

                        var type = db.PostDetails.Include(e => e.MaritalStatus).Where(e => e.Id == data).SingleOrDefault();
                        IList<PostDetails> typedetails = null;
                        if (type.MaritalStatus != null)
                        {
                            typedetails = db.PostDetails.Where(x => x.MaritalStatus.Id == type.MaritalStatus.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.PostDetails.Where(x => x.Id == data).ToList();
                        }
                        db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.MaritalStatus = p.MaritalStatus;
                            db.PostDetails.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            // await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.PostDetails.Include(e => e.MaritalStatus).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.MaritalStatus = null;
                            db.PostDetails.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //  await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var BusiTypeDetails = db.PostDetails.Include(e => e.MaritalStatus).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.MaritalStatus = null;
                        db.PostDetails.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        // await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                var CurCorp = db.PostDetails.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    p.DBTrack = dbT;
                    PostDetails corp = new PostDetails()
                    {
                        FuncStruct = p.FuncStruct,
                        //NoOfVacancies = p.NoOfVacancies,
                        ExpYearFrom = p.ExpYearFrom,
                        ExpYearTo = p.ExpYearTo,
                        AgeFrom = p.AgeFrom,
                        AgeTo = p.AgeTo,
                        Narration = p.Narration == null ? "" : p.Narration.ToString(),              //FuncStruct.JobPosition
                        ExpFilter = p.ExpFilter,
                        RangeFilter = p.RangeFilter,
                        CategoryPost = p.CategoryPost,
                        CategorySplPost = p.CategorySplPost,
                        Skill = p.Skill,
                        Gender = p.Gender,
                        MaritalStatus = p.MaritalStatus,
                        Qualification = p.Qualification,
                        Id = data,
                        DBTrack = p.DBTrack
                    };


                    db.PostDetails.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    // DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(ManpowerRequestPost L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
                try
                {
                    string JobPosition = form["JobPositionMList"] == "0" ? "" : form["JobPositionMList"];
                    string ExpFilter_Id = form["ExpFilterList_DDL"] == "0" ? "" : form["ExpFilterList_DDL"];
                    string RangeFilter_Id = form["RangeFilterList_DDL"] == "0" ? "" : form["RangeFilterList_DDL"];
                    string Gender = form["GenderList_DDL"] == "0" ? "" : form["GenderList_DDL"];
                    string MStatus = form["MaritalStatusList_DDL"] == "0" ? "" : form["MaritalStatusList_DDL"];
                    string PostSourceType = form["PostSourceTypeList_DDL"] == "0" ? "" : form["PostSourceTypeList_DDL"];
                    var blog1 = db.ManpowerRequestPost.Where(e => e.Id == data).Include(e => e.FuncStruct)
                                                    .Include(e => e.FuncStruct.JobPosition)
                                                    .Include(e => e.FuncStruct.Job)
                                                    .Include(e => e.ExpFilter)
                                                    .Include(e => e.RangeFilter)
                                                    .Include(e => e.Qualification)
                                                    .Include(e => e.Skill)
                                                    .Include(e => e.Gender)
                                                    .Include(e => e.MaritalStatus)
                                                    .Include(e => e.CategoryPost)
                                                    .Include(e => e.CategoryPost.Select(q => q.Category))
                                                    .Include(e => e.CategorySplPost)
                                                    .Include(e => e.PostSourceType)
                                                    .Include(e => e.CategorySplPost.Select(q => q.SpecialCategory))
                                                              .SingleOrDefault();

                    blog1.FuncStruct = null;
                    blog1.Skill = null;
                    blog1.Qualification = null;
                    blog1.CategoryPost = null;
                    blog1.CategorySplPost = null;

                    if (L.Narration != null)
                    {
                        blog1.Narration = L.Narration.ToString();
                    }
                    // blog1.Narration = L.Narration;
                    blog1.RequestVacancies = L.RequestVacancies;
                    blog1.ExpYearFrom = L.ExpYearFrom;
                    blog1.ExpYearTo = L.ExpYearTo;
                    blog1.AgeFrom = L.AgeFrom;
                    blog1.AgeTo = L.AgeTo;
                    blog1.PostCode = L.PostCode;
                    blog1.PostRequestDate = L.PostRequestDate;


                    if (JobPosition != null)
                    {
                        if (JobPosition != "")
                        {
                            int ContId = Convert.ToInt32(JobPosition);
                            var val = db.FuncStruct.Include(e => e.Job).Include(e => e.JobPosition)
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            blog1.FuncStruct = val;
                        }
                    }

                    List<Skill> ObjITsection = new List<Skill>();
                    ManpowerRequestPost pd = null;
                    pd = db.ManpowerRequestPost.Include(e => e.Skill).Where(e => e.Id == data).SingleOrDefault();
                    string Skill1 = form["SkillList"];
                    if (Skill1 != null && Skill1 != "")
                    {
                        var ids = Utility.StringIdsToListIds(Skill1);
                        foreach (var ca in ids)
                        {
                            var value = db.Skill.Find(ca);
                            ObjITsection.Add(value);
                            pd.Skill = ObjITsection;

                        }
                    }
                    else
                    {
                        pd.Skill = null;
                    }

                    List<Qualification> ObjQualification = new List<Qualification>();
                    ManpowerRequestPost pd1 = null;
                    pd1 = db.ManpowerRequestPost.Include(e => e.Qualification).Where(e => e.Id == data).SingleOrDefault();
                    string quali = form["QualificationListM"];
                    if (quali != null && quali != "")
                    {
                        var ids = Utility.StringIdsToListIds(quali);
                        foreach (var ca in ids)
                        {
                            var value = db.Qualification.Find(ca);
                            ObjQualification.Add(value);
                            pd1.Qualification = ObjQualification;

                        }
                    }
                    else
                    {
                        pd1.Qualification = null;
                    }

                    //List<Skill> AllergyVal = new List<Skill>();
                    //string Skill1 = form["SkillList"];
                    //if (Skill1 != null)
                    //{
                    //    var ids = Utility.StringIdsToListIds(Skill1);
                    //    foreach (var ca in ids)
                    //    {
                    //        var Allergy_val = db.Skill.Find(ca);
                    //        AllergyVal.Add(Allergy_val);
                    //        blog1.Skill = AllergyVal;
                    //    }
                    //}
                    //List<Qualification> qual = new List<Qualification>();
                    //string quali = form["QualificationListM"];
                    //if (quali != null)
                    //{
                    //    var ids = Utility.StringIdsToListIds(quali);
                    //    foreach (var ca in ids)
                    //    {
                    //        var Allergy_val = db.Qualification.Find(ca);
                    //        qual.Add(Allergy_val);
                    //        blog1.Qualification = qual;
                    //    }
                    //}

                    List<CategoryPost> cat = new List<CategoryPost>();
                    string catr = form["CategoryPostList"];
                    if (catr != null)
                    {
                        var ids = Utility.StringIdsToListIds(catr);
                        foreach (var ca in ids)
                        {
                            var Allergy_val = db.CategoryPost.Find(ca);
                            cat.Add(Allergy_val);
                            blog1.CategoryPost = cat;
                        }
                    }

                    List<CategorySplPost> cat1 = new List<CategorySplPost>();
                    string catr1 = form["CategorySplPostList"];
                    if (catr1 != null)
                    {
                        var ids = Utility.StringIdsToListIds(catr1);
                        foreach (var ca in ids)
                        {
                            var Allergy_val = db.CategorySplPost.Find(ca);
                            cat1.Add(Allergy_val);
                            blog1.CategorySplPost = cat1;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        try
                        {
                            //  using (DataBaseContext db = new DataBaseContext())
                            {
                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    // PostDetails blog = null; // to retrieve old data
                                    // DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        //blog = context.PostDetails.Where(e => e.Id == data).Include(e => e.FuncStruct)
                                        //              .Include(e => e.FuncStruct.JobPosition)                                    
                                        //              .Include(e=>e.FuncStruct.Job)
                                        //              .Include(e => e.ExpFilter)
                                        //              .Include(e => e.RangeFilter)
                                        //              .Include(e => e.Qualification)
                                        //              .Include(e => e.Skill)
                                        //              .Include(e => e.Gender)
                                        //              .Include(e => e.MaritalStatus)
                                        //              .Include(e => e.CategoryPost)
                                        //              .Include(e => e.CategoryPost.Select(q=>q.Category))
                                        //              .Include(e => e.CategorySplPost)
                                        //              .Include(e => e.CategorySplPost.Select(q => q.SpecialCategory))
                                        //                        .SingleOrDefault();
                                        //originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    blog1.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog1.DBTrack.CreatedBy == null ? null : blog1.DBTrack.CreatedBy,
                                        CreatedOn = blog1.DBTrack.CreatedOn == null ? null : blog1.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    if (ExpFilter_Id != null)
                                    {
                                        if (ExpFilter_Id != "")
                                        {
                                            var val = db.LookupValue.Find(int.Parse(ExpFilter_Id));
                                            blog1.ExpFilter = val;

                                            var type = db.ManpowerRequestPost.Include(e => e.ExpFilter).Where(e => e.Id == data).SingleOrDefault();
                                            IList<ManpowerRequestPost> typedetails = null;
                                            if (type.ExpFilter != null)
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.ExpFilter.Id == type.ExpFilter.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.ExpFilter = blog1.ExpFilter;
                                                db.ManpowerRequestPost.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var WFTypeDetails = db.ManpowerRequestPost.Include(e => e.ExpFilter).Where(x => x.Id == data).ToList();
                                            foreach (var s in WFTypeDetails)
                                            {
                                                s.ExpFilter = null;
                                                db.ManpowerRequestPost.Attach(s);
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
                                        var CreditdateypeDetails = db.ManpowerRequestPost.Include(e => e.ExpFilter).Where(x => x.Id == data).ToList();
                                        foreach (var s in CreditdateypeDetails)
                                        {
                                            s.ExpFilter = null;
                                            db.ManpowerRequestPost.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }


                                    if (RangeFilter_Id != null)
                                    {
                                        if (RangeFilter_Id != "")
                                        {
                                            var val = db.LookupValue.Find(int.Parse(RangeFilter_Id));
                                            blog1.RangeFilter = val;

                                            var type = db.ManpowerRequestPost.Include(e => e.RangeFilter).Where(e => e.Id == data).SingleOrDefault();
                                            IList<ManpowerRequestPost> typedetails = null;
                                            if (type.RangeFilter != null)
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.RangeFilter.Id == type.RangeFilter.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.RangeFilter = blog1.RangeFilter;
                                                db.ManpowerRequestPost.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var WFTypeDetails = db.ManpowerRequestPost.Include(e => e.RangeFilter).Where(x => x.Id == data).ToList();
                                            foreach (var s in WFTypeDetails)
                                            {
                                                s.RangeFilter = null;
                                                db.ManpowerRequestPost.Attach(s);
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
                                        var CreditdateypeDetails = db.ManpowerRequestPost.Include(e => e.RangeFilter).Where(x => x.Id == data).ToList();
                                        foreach (var s in CreditdateypeDetails)
                                        {
                                            s.RangeFilter = null;
                                            db.ManpowerRequestPost.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    if (Gender != null)
                                    {
                                        if (Gender != "")
                                        {
                                            var val = db.LookupValue.Find(int.Parse(Gender));
                                            blog1.Gender = val;

                                            var type = db.ManpowerRequestPost.Include(e => e.Gender).Where(e => e.Id == data).SingleOrDefault();
                                            IList<ManpowerRequestPost> typedetails = null;
                                            if (type.Gender != null)
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.Gender.Id == type.Gender.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.Gender = blog1.Gender;
                                                db.ManpowerRequestPost.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var WFTypeDetails = db.ManpowerRequestPost.Include(e => e.Gender).Where(x => x.Id == data).ToList();
                                            foreach (var s in WFTypeDetails)
                                            {
                                                s.Gender = null;
                                                db.ManpowerRequestPost.Attach(s);
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
                                        var CreditdateypeDetails = db.ManpowerRequestPost.Include(e => e.Gender).Where(x => x.Id == data).ToList();
                                        foreach (var s in CreditdateypeDetails)
                                        {
                                            s.Gender = null;
                                            db.ManpowerRequestPost.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    if (PostSourceType != null)
                                    {
                                        if (PostSourceType != "")
                                        {
                                            var val = db.LookupValue.Find(int.Parse(PostSourceType));
                                            blog1.PostSourceType = val;

                                            var type = db.ManpowerRequestPost.Include(e => e.PostSourceType).Where(e => e.Id == data).SingleOrDefault();
                                            IList<ManpowerRequestPost> typedetails = null;
                                            if (type.PostSourceType != null)
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.PostSourceType.Id == type.PostSourceType.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.PostSourceType = blog1.PostSourceType;
                                                db.ManpowerRequestPost.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var WFTypeDetails = db.ManpowerRequestPost.Include(e => e.PostSourceType).Where(x => x.Id == data).ToList();
                                            foreach (var s in WFTypeDetails)
                                            {
                                                s.PostSourceType = null;
                                                db.ManpowerRequestPost.Attach(s);
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
                                        var CreditdateypeDetails = db.ManpowerRequestPost.Include(e => e.Gender).Where(x => x.Id == data).ToList();
                                        foreach (var s in CreditdateypeDetails)
                                        {
                                            s.Gender = null;
                                            db.ManpowerRequestPost.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    if (MStatus != null)
                                    {
                                        if (MStatus != "")
                                        {
                                            var val = db.LookupValue.Find(int.Parse(MStatus));
                                            blog1.MaritalStatus = val;

                                            var type = db.ManpowerRequestPost.Include(e => e.MaritalStatus).Where(e => e.Id == data).SingleOrDefault();
                                            IList<ManpowerRequestPost> typedetails = null;
                                            if (type.MaritalStatus != null)
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.MaritalStatus.Id == type.MaritalStatus.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.MaritalStatus = blog1.MaritalStatus;
                                                db.ManpowerRequestPost.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var WFTypeDetails = db.ManpowerRequestPost.Include(e => e.MaritalStatus).Where(x => x.Id == data).ToList();
                                            foreach (var s in WFTypeDetails)
                                            {
                                                s.MaritalStatus = null;
                                                db.ManpowerRequestPost.Attach(s);
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
                                        var CreditdateypeDetails = db.ManpowerRequestPost.Include(e => e.MaritalStatus).Where(x => x.Id == data).ToList();
                                        foreach (var s in CreditdateypeDetails)
                                        {
                                            s.MaritalStatus = null;
                                            db.ManpowerRequestPost.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    if (JobPosition != null)
                                    {
                                        if (JobPosition != "")
                                        {
                                            var val = db.FuncStruct.Find(int.Parse(JobPosition));
                                            blog1.FuncStruct = val;

                                            var type = db.ManpowerRequestPost.Include(e => e.FuncStruct).Include(e => e.FuncStruct.Job).Include(e => e.FuncStruct.JobPosition).Where(e => e.Id == data).SingleOrDefault();
                                            IList<ManpowerRequestPost> typedetails = null;
                                            if (type.FuncStruct != null)
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.FuncStruct.Id == type.FuncStruct.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.FuncStruct = blog1.FuncStruct;
                                                db.ManpowerRequestPost.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var WFTypeDetails = db.ManpowerRequestPost.Include(e => e.FuncStruct).Include(e => e.FuncStruct.Job).Include(e => e.FuncStruct.JobPosition).Where(x => x.Id == data).ToList();
                                            foreach (var s in WFTypeDetails)
                                            {
                                                s.FuncStruct = null;
                                                db.ManpowerRequestPost.Attach(s);
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
                                        var CreditdateypeDetails = db.ManpowerRequestPost.Include(e => e.FuncStruct).Include(e => e.FuncStruct.Job).Include(e => e.FuncStruct.JobPosition).Where(x => x.Id == data).ToList();
                                        foreach (var s in CreditdateypeDetails)
                                        {
                                            s.FuncStruct = null;
                                            db.ManpowerRequestPost.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }


                                    var CurCorp = db.ManpowerRequestPost.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        ManpowerRequestPost post = new ManpowerRequestPost()
                                        {
                                            RequestVacancies = blog1.RequestVacancies,
                                            ExpYearFrom = blog1.ExpYearFrom,
                                            ExpYearTo = blog1.ExpYearTo,
                                            AgeFrom = blog1.AgeFrom,
                                            AgeTo = blog1.AgeTo,
                                            Skill = blog1.Skill,
                                            Qualification = blog1.Qualification,
                                            CategoryPost = blog1.CategoryPost,
                                            CategorySplPost = blog1.CategorySplPost,
                                            Narration = blog1.Narration,
                                            FuncStruct = blog1.FuncStruct,
                                            PostCode = blog1.PostCode,
                                            PostRequestDate = blog1.PostRequestDate,
                                            Id = data,
                                            DBTrack = blog1.DBTrack
                                        };
                                        db.ManpowerRequestPost.Attach(post);
                                        db.Entry(post).State = System.Data.Entity.EntityState.Modified;

                                        db.Entry(post).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        db.SaveChanges();

                                        await db.SaveChangesAsync();
                                        db.Entry(post).State = System.Data.Entity.EntityState.Detached;
                                        ts.Complete();
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = blog1.Id, Val = blog1.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                    }
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (PostDetails)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (PostDetails)databaseEntry.ToObject();
                                blog1.RowVersion = databaseValues.RowVersion;

                            }
                        }
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });

                    }
                    return Json(new Object[] { "", "", "", JsonRequestBehavior.AllowGet });
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
