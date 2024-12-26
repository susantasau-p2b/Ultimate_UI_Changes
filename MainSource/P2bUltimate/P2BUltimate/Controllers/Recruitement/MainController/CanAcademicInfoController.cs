

using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Recruitement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

    namespace P2BUltimate.Controllers.Recruitement.MainController
    {
        [AuthoriseManger]
        public class CanAcademicInfoController : Controller
        {
            //private DataBaseContext db = new DataBaseContext();
            // GET: /EmpAcademicInfo/
            public ActionResult Index()
            {
                //return View();
                return View("~/Views/Recruitement/MainView/CanAcademicInfo/Index.cshtml");
            }

            //public ActionResult QualDtl_partial()
            //{
            //    return View("~/Views/Shared/_QualificationDetails.cshtml");
            //}


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

            public ActionResult GetHobbyLKDetails(string data)
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var fall = db.Hobby.ToList();
                    IEnumerable<Hobby> all;
                    if (!string.IsNullOrEmpty(data))
                    {
                        all = db.Hobby.ToList().Where(d => d.HobbyName.Contains(data));

                    }
                    else
                    {
                        var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.HobbyName }).Distinct();

                        return Json(r, JsonRequestBehavior.AllowGet);
                    }
                    var result = (from c in all
                                  select new { c.Id, c.HobbyName }).Distinct();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }


            public ActionResult GetLanguageSkillLKDetails(string data)
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var fall = db.LanguageSkill.Include(e => e.Language).ToList();
                    IEnumerable<LanguageSkill> all;
                    if (!string.IsNullOrEmpty(data))
                    {
                        all = db.LanguageSkill.ToList().Where(d => d.FullDetails.Contains(data));

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

            //public ActionResult GetQualificationDetailLKDetails(string data)
            //{
            //    using (DataBaseContext db = new DataBaseContext())
            //    {
            //        var fall = db.QualificationDetails.ToList();
            //        IEnumerable<QualificationDetails> all;
            //        if (!string.IsNullOrEmpty(data))
            //        {
            //            all = db.QualificationDetails.ToList().Where(d => d.Institute.Contains(data));

            //        }
            //        else
            //        {
            //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.Institute }).Distinct();

            //            return Json(r, JsonRequestBehavior.AllowGet);
            //        }
            //        var result = (from c in all
            //                      select new { c.Id, c.Institute }).Distinct();
            //        return Json(result, JsonRequestBehavior.AllowGet);
            //    }
            //}

            public ActionResult GetQualificationDetailLKDetails(string data)
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var fall = db.Qualification.ToList();
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
            public ActionResult GetAwardsLKDetails(string data)
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var fall = db.Awards.ToList();
                    IEnumerable<Awards> all;
                    if (!string.IsNullOrEmpty(data))
                    {
                        all = db.Awards.ToList().Where(d => d.Name.Contains(data));

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

            public ActionResult GetScolarshipLKDetails(string data)
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var fall = db.Scolarship.ToList();
                    IEnumerable<Scolarship> all;
                    if (!string.IsNullOrEmpty(data))
                    {
                        all = db.Scolarship.ToList().Where(d => d.Name.Contains(data));

                    }
                    else
                    {
                        var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.Name }).Distinct();

                        return Json(r, JsonRequestBehavior.AllowGet);
                    }
                    var result = (from c in all
                                  select new { c.Id, c.FullDetails }).Distinct();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }

            public class AcademicDetails
            {
                public Array AwardsDetails_Id { get; set; }
                public Array AwardsDetails_val { get; set; }
                public Array HobbyDetails_Id { get; set; }
                public Array HobbyDetails_val { get; set; }
                public Array SkillDetails_Id { get; set; }
                public Array SkillDetails_val { get; set; }
                public Array LanguageSkillDtl_Id { get; set; }
                public Array LanguageSkillDtl_val { get; set; }
                public Array QualificationDetailsdtl_Id { get; set; }
                public Array QualificationDetailsdtl_val { get; set; }
                public Array Scolarshipdtl_Id { get; set; }
                public Array Scolarshipdtl_val { get; set; }

            }

            public class AwardsDetails
            {
                public Array AwardsDetails_Id { get; set; }
                public Array AwardsDetails_val { get; set; }

            }

            public class HobbyDetails
            {
                public Array HobbyDetails_Id { get; set; }
                public Array HobbyDetails_val { get; set; }

            }

            public class SkillDetails
            {
                public Array SkillDetails_Id { get; set; }
                public Array SkillDetails_val { get; set; }

            }
            public class LanguageSkillDtl
            {
                public Array LanguageSkillDtl_Id { get; set; }
                public Array LanguageSkillDtl_val { get; set; }

            }

            public class QualificationDetailsdtl
            {
                public Array QualificationDetailsdtl_Id { get; set; }
                public Array QualificationDetailsdtl_val { get; set; }

            }

            public class Scolarshipdtl
            {
                public Array Scolarshipdtl_Id { get; set; }
                public Array Scolarshipdtl_val { get; set; }

            }

            public ActionResult Edit(int data, FormCollection f)
            {
                using (DataBaseContext db = new DataBaseContext())
                {

                    var Candidate = db.Candidate.Include(e => e.CanAcademicInfo)
                       .Include(e => e.CanAcademicInfo.QualificationDetails)
                       .Include(e => e.CanAcademicInfo.Hobby)
                       .Include(e => e.CanAcademicInfo.LanguageSkill)
                       .Include(e => e.CanAcademicInfo.Skill)
                       .Include(e => e.CanAcademicInfo.Scolarship)
                       .Include(e => e.CanAcademicInfo.Awards)
                       .Where(e => e.Id == data).ToList();




                    var r = (from ca in Candidate
                             select new
                             {
                                 Id = ca.Id,
                                 Action = ca.DBTrack.Action
                             }).Distinct();

                    List<AcademicDetails> awd = new List<AcademicDetails>();

                    var a = db.Candidate.Include(e => e.CanAcademicInfo)
                        .Include(e => e.CanAcademicInfo.QualificationDetails)
                        .Include(e => e.CanAcademicInfo.Hobby)
                        .Include(e => e.CanAcademicInfo.LanguageSkill)
                        .Include(e => e.CanAcademicInfo.LanguageSkill.Select(b => b.Language))
                        .Include(e => e.CanAcademicInfo.LanguageSkill.Select(b => b.SkillType))
                        .Include(e => e.CanAcademicInfo.Skill)
                        .Include(e => e.CanAcademicInfo.Scolarship)
                        .Include(e => e.CanAcademicInfo.Awards)
                        .Where(e => e.Id == data).ToList();


                    //List<AwardsDetails> awd = new List<AwardsDetails>();
                    //List<HobbyDetails> hob = new List<HobbyDetails>();
                    //List<SkillDetails> skl = new List<SkillDetails>();
                    //List<LanguageSkillDtl> lanskl = new List<LanguageSkillDtl>();
                    //List<QualificationDetailsdtl> Qualdtl = new List<QualificationDetailsdtl>();
                    //List<Scolarshipdtl> sclr = new List<Scolarshipdtl>();



                    foreach (var ca in a)
                    {
                        awd.Add(new AcademicDetails
                        {
                            AwardsDetails_Id = ca.CanAcademicInfo.Awards.Select(e => e.Id.ToString()).ToArray(),
                            AwardsDetails_val = ca.CanAcademicInfo.Awards.Select(e => e.FullDetails).ToArray(),
                            HobbyDetails_Id = ca.CanAcademicInfo.Hobby.Select(e => e.Id.ToString()).ToArray(),
                            HobbyDetails_val = ca.CanAcademicInfo.Hobby.Select(e => e.HobbyName).ToArray(),
                            SkillDetails_Id = ca.CanAcademicInfo.Skill.Select(e => e.Id.ToString()).ToArray(),
                            SkillDetails_val = ca.CanAcademicInfo.Skill.Select(e => e.Name).ToArray(),
                            LanguageSkillDtl_Id = ca.CanAcademicInfo.LanguageSkill.Select(e => e.Id.ToString()).ToArray(),
                            LanguageSkillDtl_val = ca.CanAcademicInfo.LanguageSkill.Select(e => e.FullDetails).ToArray(),
                            Scolarshipdtl_Id = ca.CanAcademicInfo.Scolarship.Select(e => e.Id.ToString()).ToArray(),
                            Scolarshipdtl_val = ca.CanAcademicInfo.Scolarship.Select(e => e.FullDetails).ToArray(),
                            QualificationDetailsdtl_Id = ca.CanAcademicInfo.QualificationDetails.Select(e => e.Id.ToString()).ToArray(),
                            QualificationDetailsdtl_val = ca.CanAcademicInfo.QualificationDetails.Select(e => e.FullDetails).ToArray()
                        });
                    }
                    //TempData["RowVersion"] = db.EmpAcademicInfo.Find(data).RowVersion;

                    //return this.Json(new Object[] { r, a, JsonRequestBehavior.AllowGet });


                    //var W = db.DT_Employee.Include(e=>e.EmpAcademicInfo_Id)
                    //  //  .Include(e => e.EmpAcademicInfo_Id.Awards_Id)
                    //  //.Include(e => e.EmpAcademicInfo_Id.Hobby_Id)
                    //  //.Include(e => e.EmpAcademicInfo_Id.LanguageSkill_Id)
                    //  //.Include(e => e.EmpAcademicInfo_Id.QualificationDetails_Id)
                    //  //.Include(e => e.EmpAcademicInfo_Id.Scolarship_Id)
                    //  //.Include(e => e.EmpAcademicInfo_Id.Skill_Id)
                    //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                    //     (e => new
                    //     {
                    //         DT_Id = e.Id,
                    //         Awards_val = e.Awards_Id == 0 ? "" : db.Awards.Where(x => x.Id == e.Awards_Id).Select(x => x.FullDetails).FirstOrDefault(),
                    //         Hobby_val = e.Hobby_Id == 0 ? "" : db.Hobby.Where(x => x.Id == e.Hobby_Id).Select(x => x.HobbyName).FirstOrDefault(),
                    //         LanguageSkill_val = e.LanguageSkill_Id == 0 ? "" : db.LanguageSkill.Where(x => x.Id == e.LanguageSkill_Id).Select(x => x.FullDetails).FirstOrDefault(),
                    //         QualificationDetails_val = e.QualificationDetails_Id == 0 ? "" : db.QualificationDetails.Where(x => x.Id == e.QualificationDetails_Id).Select(x => x.FullDetails).FirstOrDefault(),
                    //         Skill_val = e.Skill_Id == 0 ? "" : db.Skill.Where(x => x.Id == e.Skill_Id).Select(x => x.Name).FirstOrDefault(),
                    //         Scolarship_val = e.Scolarship_Id == 0 ? "" : db.Scolarship.Where(x => x.Id == e.Scolarship_Id).Select(x => x.Name).FirstOrDefault(),
                    //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                    var Corp = db.Employee.Find(data);
                    TempData["RowVersion"] = Corp.RowVersion;
                    var Auth = Corp.DBTrack.IsModified;
                    return Json(new Object[] { r, awd, "", Auth, JsonRequestBehavior.AllowGet });
                }
            }

            //public ActionResult Edit(int data)
            //{


            //    var EmpAcademic = db.EmpAcademicInfo
            //         .Include(e => e.Awards)
            //      .Include(e => e.Hobby)
            //      .Include(e => e.LanguageSkill)
            //      .Include(e => e.QualificationDetails)
            //      .Include(e => e.Scolarship)
            //      .Include(e => e.Skill)
            //        .Where(e => e.Id == data).ToList();

            //    var r = (from ca in EmpAcademic
            //             select new
            //             {
            //                 Id = ca.Id,
            //                 Action = ca.DBTrack.Action
            //             }).Distinct();
            //    List<AcademicDetails> awd = new List<AcademicDetails>();

            //    //List<AwardsDetails> awd = new List<AwardsDetails>();
            //    //List<HobbyDetails> hob = new List<HobbyDetails>();
            //    //List<SkillDetails> skl = new List<SkillDetails>();
            //    //List<LanguageSkillDtl> lanskl = new List<LanguageSkillDtl>();
            //    //List<QualificationDetailsdtl> Qualdtl = new List<QualificationDetailsdtl>();
            //    //List<Scolarshipdtl> sclr = new List<Scolarshipdtl>();

            //    var a = db.EmpAcademicInfo
            //         .Include(e => e.Awards)
            //      .Include(e => e.Hobby)
            //      .Include(e => e.LanguageSkill).Include(e => e.LanguageSkill.Select(s => s.SkillType))
            //      .Include(e => e.LanguageSkill.Select(s => s.Language))
            //      .Include(e => e.QualificationDetails)
            //      .Include(e => e.Scolarship)
            //      .Include(e => e.Skill)
            //        .Where(e => e.Id == data).ToList();
            //    foreach (var ca in a)
            //    {
            //        awd.Add(new AcademicDetails
            //        {
            //            AwardsDetails_Id = ca.Awards.Select(e => e.Id.ToString()).ToArray(),
            //            AwardsDetails_val = ca.Awards.Select(e => e.FullDetails).ToArray(),
            //            HobbyDetails_Id = ca.Hobby.Select(e => e.Id.ToString()).ToArray(),
            //            HobbyDetails_val = ca.Hobby.Select(e => e.HobbyName).ToArray(),
            //            SkillDetails_Id = ca.Skill.Select(e => e.Id.ToString()).ToArray(),
            //            SkillDetails_val = ca.Skill.Select(e => e.Name).ToArray(),
            //            LanguageSkillDtl_Id = ca.LanguageSkill.Select(e => e.Id.ToString()).ToArray(),
            //            LanguageSkillDtl_val = ca.LanguageSkill.Select(e => e.FullDetails).ToArray(),
            //            Scolarshipdtl_Id = ca.Scolarship.Select(e => e.Id.ToString()).ToArray(),
            //            Scolarshipdtl_val = ca.Scolarship.Select(e => e.FullDetails).ToArray(),
            //            QualificationDetailsdtl_Id = ca.QualificationDetails.Select(e => e.Id.ToString()).ToArray(),
            //            QualificationDetailsdtl_val = ca.QualificationDetails.Select(e => e.FullDetails).ToArray()
            //        });
            //    }
            //    //TempData["RowVersion"] = db.EmpAcademicInfo.Find(data).RowVersion;

            //    //return this.Json(new Object[] { r, a, JsonRequestBehavior.AllowGet });


            //    var W = db.DT_EmpAcademicInfo
            //        .Include(e => e.Awards_Id)
            //      .Include(e => e.Hobby_Id)
            //      .Include(e => e.LanguageSkill_Id)
            //      .Include(e => e.QualificationDetails_Id)
            //      .Include(e => e.Scolarship_Id)
            //      .Include(e => e.Skill_Id)
            //         .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
            //         (e => new
            //         {
            //             DT_Id = e.Id,
            //             Awards_val = e.Awards_Id == 0 ? "" : db.Awards.Where(x => x.Id == e.Awards_Id).Select(x => x.FullDetails).FirstOrDefault(),
            //             Hobby_val = e.Hobby_Id == 0 ? "" : db.Hobby.Where(x => x.Id == e.Hobby_Id).Select(x => x.HobbyName).FirstOrDefault(),
            //             LanguageSkill_val = e.LanguageSkill_Id == 0 ? "" : db.LanguageSkill.Where(x => x.Id == e.LanguageSkill_Id).Select(x => x.FullDetails).FirstOrDefault(),
            //             QualificationDetails_val = e.QualificationDetails_Id == 0 ? "" : db.QualificationDetails.Where(x => x.Id == e.QualificationDetails_Id).Select(x => x.FullDetails).FirstOrDefault(),
            //             Skill_val = e.Skill_Id == 0 ? "" : db.Skill.Where(x => x.Id == e.Skill_Id).Select(x => x.Name).FirstOrDefault(),
            //             Scolarship_val = e.Scolarship_Id == 0 ? "" : db.Scolarship.Where(x => x.Id == e.Scolarship_Id).Select(x => x.Name).FirstOrDefault(),
            //         }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

            //    var Corp = db.EmpAcademicInfo.Find(data);
            //    TempData["RowVersion"] = Corp.RowVersion;
            //    var Auth = Corp.DBTrack.IsModified;
            //    return Json(new Object[] { r, awd, W, Auth, JsonRequestBehavior.AllowGet });



            //}


            [HttpPost]
            //[ValidateAntiForgeryToken]
            public async Task<ActionResult> Delete(int? data)
            {
                List<string> Msg = new List<string>();
                using (DataBaseContext db = new DataBaseContext())
                {
                    try
                    {
                        EmpAcademicInfo EmpAcademicInfo = db.EmpAcademicInfo.Include(e => e.QualificationDetails)
                            .Include(e => e.Hobby)
                            .Include(e => e.Skill)
                            .Include(e => e.LanguageSkill)
                            .Include(e => e.Scolarship)
                            .Include(e => e.Awards)
                                                            .Where(e => e.Id == data).SingleOrDefault();



                        if (EmpAcademicInfo.DBTrack.IsModified == true)
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    CreatedBy = EmpAcademicInfo.DBTrack.CreatedBy != null ? EmpAcademicInfo.DBTrack.CreatedBy : null,
                                    CreatedOn = EmpAcademicInfo.DBTrack.CreatedOn != null ? EmpAcademicInfo.DBTrack.CreatedOn : null,
                                    IsModified = EmpAcademicInfo.DBTrack.IsModified == true ? true : false
                                };
                                EmpAcademicInfo.DBTrack = dbT;
                                db.Entry(EmpAcademicInfo).State = System.Data.Entity.EntityState.Modified;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, EmpAcademicInfo.DBTrack);
                                DT_EmpAcademicInfo DT_OBJ = (DT_EmpAcademicInfo)rtn_Obj;
                                db.Create(DT_OBJ);

                                await db.SaveChangesAsync();
                                ts.Complete();

                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }
                        else
                        {

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                var selectedValues = EmpAcademicInfo.QualificationDetails;
                                var lkValue = new HashSet<int>(EmpAcademicInfo.QualificationDetails.Select(e => e.Id));
                                if (lkValue.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }

                                var lkValue1 = new HashSet<int>(EmpAcademicInfo.Hobby.Select(e => e.Id));
                                if (lkValue1.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }

                                var lkValue2 = new HashSet<int>(EmpAcademicInfo.Skill.Select(e => e.Id));
                                if (lkValue2.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }

                                var lkValue3 = new HashSet<int>(EmpAcademicInfo.LanguageSkill.Select(e => e.Id));
                                if (lkValue3.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                var lkValue4 = new HashSet<int>(EmpAcademicInfo.Awards.Select(e => e.Id));
                                if (lkValue4.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                var lkValue5 = new HashSet<int>(EmpAcademicInfo.Scolarship.Select(e => e.Id));
                                if (lkValue5.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                db.Entry(EmpAcademicInfo).State = System.Data.Entity.EntityState.Deleted;
                                db.SaveChanges();
                                ts.Complete();

                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        db.Entry(EmpAcademicInfo).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        // ts.Complete();

                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

            //        IEnumerable<EmpAcademicInfo> EmpAcademicInfo = null;
            //        if (gp.IsAutho == true)
            //        {
            //            EmpAcademicInfo = db.EmpAcademicInfo.Where(e => e.DBTrack.IsModified == true).ToList();
            //        }
            //        else
            //        {
            //            EmpAcademicInfo = db.EmpAcademicInfo.ToList();
            //        }
            //        IEnumerable<EmpAcademicInfo> IE;

            //        if (!string.IsNullOrEmpty(gp.searchField))
            //        {
            //            IE = EmpAcademicInfo;
            //            if (gp.searchOper.Equals("eq"))
            //            {
            //                jsonData = IE.Select(a => new Object[] { a.Id, a.Hobby, a.Skill }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
            //            }
            //            if (pageIndex > 1)
            //            {
            //                int h = pageIndex * pageSize;
            //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Hobby, a.Skill }).ToList();
            //            }
            //            totalRecords = IE.Count();
            //        }
            //        else
            //        {
            //            IE = EmpAcademicInfo;
            //            Func<EmpAcademicInfo, string> orderfuc = (c =>
            //                                                       gp.sidx == "ID" ? c.Id.ToString() : ""
            //                                                       );



            //            if (gp.sord == "asc")
            //            {
            //                IE = IE.OrderBy(orderfuc);
            //                jsonData = IE.Select(a => new Object[] { a.Id, a.Hobby, a.Skill }).ToList();
            //            }
            //            else if (gp.sord == "desc")
            //            {
            //                IE = IE.OrderByDescending(orderfuc);
            //                jsonData = IE.Select(a => new Object[] { a.Id, a.Hobby, a.Skill }).ToList();
            //            }
            //            if (pageIndex > 1)
            //            {
            //                int h = pageIndex * pageSize;
            //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Hobby, a.Skill }).ToList();
            //            }
            //            totalRecords = EmpAcademicInfo.Count();
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
                    IEnumerable<Candidate> Candidate = null;
                    if (gp.IsAutho == true)
                    {
                        Candidate = db.Candidate.Include(q => q.CanAcademicInfo).Include(q => q.CanName).Where(e => e.DBTrack.IsModified == true).ToList();
                    }
                    else
                    {
                        Candidate = db.Candidate.Include(q => q.CanAcademicInfo).Include(q => q.CanName).Where(q => q.CanAcademicInfo != null).ToList();
                    }

                    IEnumerable<Candidate> IE;
                    if (!string.IsNullOrEmpty(gp.searchField))
                    {
                        IE = Candidate;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Select(a => new { a.Id, a.CanName.FullNameFML }).Where((e => (e.Id.ToString() == gp.searchString) )).ToList();
                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.PreferredHospital), Convert.ToString(a.IDMark), Convert.ToString(a.BloodGroup) != null ? Convert.ToString(a.BloodGroup.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.CanName.FullNameFML }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = Candidate;
                        Func<Candidate, int> orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, a.CanName.FullNameFML }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, a.CanName.FullNameFML }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.CanName.FullNameFML}).ToList();
                        }
                        totalRecords = Candidate.Count();
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
            // [ValidateAntiForgeryToken]
            public ActionResult Create(EmpAcademicInfo look, FormCollection form)
            {
                List<string> Msg = new List<string>();
                using (DataBaseContext db = new DataBaseContext())
                {
                    try
                    {
                        //qual,skill,hobby,lang,award,scolar
                        List<QualificationDetails> lookupval = new List<QualificationDetails>();
                        string Values = form["QualificationDetailsList"];
                        int Emp = form["Candidate_table"] == "0" ? 0 : Convert.ToInt32(form["Candidate_table"]);

                        Candidate EmpData;
                        if (Emp != null && Emp != 0)
                        {
                            EmpData = db.Candidate.Find(Emp);
                        }
                        else
                        {
                            Msg.Add("  Kindly select employee  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        if (Values != null)
                        {
                            var ids = Utility.StringIdsToListIds(Values);
                            foreach (var ca in ids)
                            {
                                var lookup_val = db.QualificationDetails.Find(ca);
                                lookupval.Add(lookup_val);
                                look.QualificationDetails = lookupval;
                            }
                        }
                        else
                        {
                            look.QualificationDetails = null;
                        }

                        // skill
                        List<Skill> lookupSkill = new List<Skill>();
                        string ValSkill = form["SkillList"];

                        if (ValSkill != null)
                        {
                            var ids = Utility.StringIdsToListIds(ValSkill);
                            foreach (var ca in ids)
                            {
                                var lookup_val = db.Skill.Find(ca);
                                lookupSkill.Add(lookup_val);
                                look.Skill = lookupSkill;
                            }
                        }
                        else
                        {
                            look.Skill = null;
                        }

                        //hobby
                        List<Hobby> lookupHobby = new List<Hobby>();
                        string ValHobby = form["HobbyList"];

                        if (ValHobby != null)
                        {
                            var ids = Utility.StringIdsToListIds(ValHobby);
                            foreach (var ca in ids)
                            {
                                var lookup_val = db.Hobby.Find(ca);
                                lookupHobby.Add(lookup_val);
                                look.Hobby = lookupHobby;
                            }
                        }
                        else
                        {
                            look.Hobby = null;
                        }

                        //,lang
                        List<LanguageSkill> lookupLang = new List<LanguageSkill>();
                        string Lang = form["LanguageSkillList"];

                        if (Lang != null)
                        {
                            var ids = Utility.StringIdsToListIds(Lang);
                            foreach (var ca in ids)
                            {
                                var lookup_val = db.LanguageSkill.Find(ca);
                                lookupLang.Add(lookup_val);
                                look.LanguageSkill = lookupLang;
                            }
                        }
                        else
                        {
                            look.LanguageSkill = null;
                        }

                        //,award
                        List<Awards> lookupaward = new List<Awards>();
                        string award = form["AwardsList"];

                        if (award != null)
                        {
                            var ids = Utility.StringIdsToListIds(award);
                            foreach (var ca in ids)
                            {
                                var lookup_val = db.Awards.Find(ca);
                                lookupaward.Add(lookup_val);
                                look.Awards = lookupaward;
                            }
                        }
                        else
                        {
                            look.Awards = null;
                        }


                        //scolar
                        List<Scolarship> lookupScolar = new List<Scolarship>();
                        string Scolar = form["ScolarshipList"];

                        if (Scolar != null)
                        {
                            var ids = Utility.StringIdsToListIds(Scolar);
                            foreach (var ca in ids)
                            {
                                var lookup_val = db.Scolarship.Find(ca);
                                lookupScolar.Add(lookup_val);
                                look.Scolarship = lookupScolar;
                            }
                        }
                        else
                        {
                            look.Scolarship = null;
                        }


                        if (ModelState.IsValid)
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                                EmpAcademicInfo empAcademicInfo = new EmpAcademicInfo()
                                {
                                    QualificationDetails = look.QualificationDetails,
                                    Scolarship = look.Scolarship,
                                    Awards = look.Awards,
                                    Skill = look.Skill,
                                    Hobby = look.Hobby,
                                    LanguageSkill = look.LanguageSkill,
                                    DBTrack = look.DBTrack
                                };
                                try
                                {
                                    db.EmpAcademicInfo.Add(empAcademicInfo);
                                    var a = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, look.DBTrack);
                                    DT_EmpAcademicInfo DT_Corp = (DT_EmpAcademicInfo)a;

                                    db.Create(DT_Corp);
                                    db.SaveChanges();

                                    if (EmpData != null)
                                    {
                                        EmpData.CanAcademicInfo = empAcademicInfo;
                                        db.Candidate.Attach(EmpData);
                                        db.Entry(EmpData).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(EmpData).State = System.Data.Entity.EntityState.Detached;
                                    }

                                    ts.Complete();
                                    Msg.Add("  Data Created successfully.  ");
                                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }

                                catch (DbUpdateConcurrencyException)
                                {
                                    return RedirectToAction("Create", new { concurrencyError = true, id = look.Id });
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
                            Msg.Add(errorMsg);
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
            [HttpPost]
            public async Task<ActionResult> EditSave(EmpAcademicInfo c, int data, FormCollection form) // Edit submit
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    try
                    {
                        List<string> Msg = new List<string>();

                        var db_data = db.EmpAcademicInfo.Include(e => e.Awards)
                                    .Include(e => e.Hobby)
                                    .Include(e => e.LanguageSkill.Select(r => r.SkillType))
                                      .Include(e => e.QualificationDetails)
                                    .Include(e => e.Skill)
                                    .Include(e => e.Scolarship).Where(e => e.Id == data).SingleOrDefault();

                        bool Auth = form["autho_allow"] == "true" ? true : false;

                        List<Awards> lookupval = new List<Awards>();
                        string Values = form["AwardsList"];


                        if (Values != null)
                        {
                            var ids = Utility.StringIdsToListIds(Values);
                            foreach (var ca in ids)
                            {
                                var lookup_val = db.Awards.Find(ca);
                                lookupval.Add(lookup_val);
                                db_data.Awards = lookupval;
                            }
                        }
                        else
                        {
                            db_data.Awards = null;
                        }
                        //2
                        List<Hobby> lookuphobby = new List<Hobby>();
                        string Valueshobby = form["HobbyList"];


                        if (Valueshobby != null)
                        {
                            var ids = Utility.StringIdsToListIds(Valueshobby);
                            foreach (var ca in ids)
                            {
                                var lookup_val = db.Hobby.Find(ca);
                                lookuphobby.Add(lookup_val);
                                db_data.Hobby = lookuphobby;
                            }
                        }
                        else
                        {
                            db_data.Hobby = null;
                        }
                        //3
                        List<LanguageSkill> lookupls = new List<LanguageSkill>();
                        string Valuesls = form["LanguageSkillList"];


                        if (Valuesls != null)
                        {
                            var ids = Utility.StringIdsToListIds(Valuesls);
                            foreach (var ca in ids)
                            {
                                var lookup_val = db.LanguageSkill.Find(ca);
                                lookupls.Add(lookup_val);
                                db_data.LanguageSkill = lookupls;
                            }
                        }
                        else
                        {
                            db_data.LanguageSkill = null;
                        }
                        //4
                        List<QualificationDetails> lookupqd = new List<QualificationDetails>();
                        string Valuesqd = form["QualificationDetailsList"];


                        if (Valuesqd != null)
                        {
                            var ids = Utility.StringIdsToListIds(Valuesqd);
                            foreach (var ca in ids)
                            {
                                var lookup_val = db.QualificationDetails.Find(ca);
                                lookupqd.Add(lookup_val);
                                db_data.QualificationDetails = lookupqd;
                            }
                        }
                        else
                        {
                            db_data.QualificationDetails = null;
                        }
                        //5
                        List<Skill> lookupskl = new List<Skill>();
                        string Valuesskl = form["SkillList"];

                        if (Valuesskl != null)
                        {
                            var ids = Utility.StringIdsToListIds(Valuesskl);
                            foreach (var ca in ids)
                            {
                                var lookup_val = db.Skill.Find(ca);
                                lookupskl.Add(lookup_val);
                                db_data.Skill = lookupskl;
                            }
                        }
                        else
                        {
                            db_data.Skill = null;
                        }
                        //6
                        List<Scolarship> lookupscr = new List<Scolarship>();
                        string Valuesscr = form["ScolarshipList"];

                        if (Valuesscr != null)
                        {
                            var ids = Utility.StringIdsToListIds(Valuesscr);
                            foreach (var ca in ids)
                            {
                                var lookup_val = db.Scolarship.Find(ca);
                                lookupscr.Add(lookup_val);
                                db_data.Scolarship = lookupscr;
                            }
                        }
                        else
                        {
                            db_data.Scolarship = null;
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
                                        db.EmpAcademicInfo.Attach(db_data);
                                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        TempData["RowVersion"] = db_data.RowVersion;
                                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                        var Curr_Lookup = db.EmpAcademicInfo.Find(data);
                                        TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                        db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                        {

                                            EmpAcademicInfo blog = null; // to retrieve old data
                                            DbPropertyValues originalBlogValues = null;

                                            using (var context = new DataBaseContext())
                                            {
                                                blog = context.EmpAcademicInfo.Where(e => e.Id == data).Include(e => e.Awards)
                                        .Include(e => e.Hobby)
                                        .Include(e => e.LanguageSkill)
                                          .Include(e => e.QualificationDetails)
                                        .Include(e => e.Skill)
                                        .Include(e => e.Scolarship).SingleOrDefault();
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
                                            EmpAcademicInfo lk = new EmpAcademicInfo
                                            {
                                                Id = data,
                                                Awards = db_data.Awards,
                                                Hobby = db_data.Hobby,
                                                LanguageSkill = db_data.LanguageSkill,
                                                QualificationDetails = db_data.QualificationDetails,
                                                Skill = db_data.Skill,
                                                Scolarship = db_data.Scolarship,
                                                DBTrack = c.DBTrack
                                            };


                                            db.EmpAcademicInfo.Attach(lk);
                                            db.Entry(lk).State = System.Data.Entity.EntityState.Modified;
                                            db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                            // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

                                            // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


                                            using (var context = new DataBaseContext())
                                            {

                                                var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                                DT_EmpAcademicInfo DT_Corp = (DT_EmpAcademicInfo)obj;

                                                db.Create(DT_Corp);
                                                db.SaveChanges();
                                            }
                                            await db.SaveChangesAsync();
                                            ts.Complete();

                                            Msg.Add("  Record Updated");
                                            return Json(new Utility.JsonReturnClass { Id = lk.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (EmpAcademicInfo)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        var databaseValues = (EmpAcademicInfo)databaseEntry.ToObject();
                                        c.RowVersion = databaseValues.RowVersion;

                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {

                                EmpAcademicInfo blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;
                                EmpAcademicInfo Old_Corp = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.EmpAcademicInfo.Where(e => e.Id == data).SingleOrDefault();
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
                                EmpAcademicInfo empAcademicInfo = new EmpAcademicInfo()
                                {

                                    Id = data,
                                    DBTrack = c.DBTrack
                                };

                                db.Entry(blog).State = System.Data.Entity.EntityState.Detached;
                                empAcademicInfo.DBTrack = c.DBTrack;
                                // db.EmpAcademicInfo.Attach(empAcademicInfo);                   
                                //db.Entry(empAcademicInfo).State = System.Data.Entity.EntityState.Modified;
                                //db.Entry(empAcademicInfo).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = blog.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                        List<string> Msg = new List<string>();
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    return View();
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
                                //Corporate corp = db.Corporate.Find(auth_id);
                                //Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                                EmpAcademicInfo corp = db.EmpAcademicInfo
                                    .Include(e => e.Awards)
                                    .Include(e => e.Hobby)
                                    .Include(e => e.LanguageSkill)
                                      .Include(e => e.QualificationDetails)
                                    .Include(e => e.Skill)
                                    .Include(e => e.Scolarship)
                                    .FirstOrDefault(e => e.Id == auth_id);

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

                                db.EmpAcademicInfo.Attach(corp);
                                db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                //db.SaveChanges();
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                                DT_EmpAcademicInfo DT_Corp = (DT_EmpAcademicInfo)rtn_Obj;
                                //DT_Corp.Awards_Id = corp.Awards == null ? 0 : corp.Awards.ToString();
                                //DT_Corp.Hobby_Id = corp.Hobby == null ? 0 : corp.Hobby;
                                //DT_Corp.LanguageSkill_Id = corp.LanguageSkill == null ? 0 : corp.LanguageSkill;
                                db.Create(DT_Corp);
                                await db.SaveChangesAsync();

                                ts.Complete();
                                Msg.Add("  Record Authorized");
                                return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else if (auth_action == "M")
                        {

                            EmpAcademicInfo Old_Corp = db.EmpAcademicInfo
                                    .Include(e => e.Awards)
                                    .Include(e => e.Hobby)
                                    .Include(e => e.LanguageSkill)
                                      .Include(e => e.QualificationDetails)
                                    .Include(e => e.Skill)
                                    .Include(e => e.Scolarship)
                                  .Where(e => e.Id == auth_id).SingleOrDefault();


                            DT_EmpAcademicInfo Curr_Corp = db.DT_EmpAcademicInfo
                                                        .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                        .OrderByDescending(e => e.Id)
                                                        .FirstOrDefault();

                            if (Curr_Corp != null)
                            {
                                EmpAcademicInfo corp = new EmpAcademicInfo();

                                //string awrd = Curr_Corp.Awards_Id == null ? null : Curr_Corp.Awards_Id.ToString();
                                //string hob = Curr_Corp.Hobby_Id == null ? null : Curr_Corp.Hobby_Id.ToString();
                                //string langskl = Curr_Corp.LanguageSkill_Id == null ? null : Curr_Corp.LanguageSkill_Id.ToString();
                                //string Qualdtl = Curr_Corp.QualificationDetails_Id == null ? null : Curr_Corp.QualificationDetails_Id.ToString();
                                //string skll = Curr_Corp.Skill_Id == null ? null : Curr_Corp.Skill_Id.ToString();
                                //string scolr = Curr_Corp.Scolarship_Id == null ? null : Curr_Corp.Scolarship_Id.ToString();

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

                                            //int a = EditS(awrd, hob, langskl, Qualdtl, skll,  scolr,auth_id, corp, corp.DBTrack);

                                            await db.SaveChangesAsync();

                                            ts.Complete();
                                            Msg.Add("  Record Authorized");
                                            return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                    catch (DbUpdateConcurrencyException ex)
                                    {
                                        var entry = ex.Entries.Single();
                                        var clientValues = (EmpAcademicInfo)entry.Entity;
                                        var databaseEntry = entry.GetDatabaseValues();
                                        if (databaseEntry == null)
                                        {
                                            Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            var databaseValues = (EmpAcademicInfo)databaseEntry.ToObject();
                                            corp.RowVersion = databaseValues.RowVersion;
                                        }
                                    }
                                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }
                        else if (auth_action == "D")
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                //Corporate corp = db.Corporate.Find(auth_id);
                                EmpAcademicInfo corp = db.EmpAcademicInfo.AsNoTracking()
                                   .Include(e => e.Awards)
                                    .Include(e => e.Hobby)
                                    .Include(e => e.LanguageSkill)
                                      .Include(e => e.QualificationDetails)
                                    .Include(e => e.Skill)
                                    .Include(e => e.Scolarship).FirstOrDefault(e => e.Id == auth_id);

                                //Awards add = corp.Awards.ToString();
                                //Hobby conDet = corp.Hobby;
                                //LanguageSkill val = corp.LanguageSkill;

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

                                db.EmpAcademicInfo.Attach(corp);
                                db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                                DT_EmpAcademicInfo DT_Corp = (DT_EmpAcademicInfo)rtn_Obj;
                                //DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                                //DT_Corp.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                                db.Create(DT_Corp);
                                await db.SaveChangesAsync();
                                db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                                Msg.Add(" Record Authorized ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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


            public int EditS(string awrd, string hob, string langskl, string Qualdtl, string skll, string scolr, int data, EmpAcademicInfo c, DBTrack dbT)
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    if (awrd != null)
                    {
                        if (awrd != "")
                        {
                            var val = db.Awards.Find(int.Parse(awrd));
                            // var val = db.Awards.Where(e => e.Id == data).SingleOrDefault();

                            var r = (from ca in db.Awards
                                     select new
                                     {
                                         Id = ca.Id,
                                         LookupVal = ca.Name
                                     }).Where(e => e.Id == data).Distinct();

                            var type = db.EmpAcademicInfo.Include(e => e.Awards).Where(e => e.Id == data).SingleOrDefault();
                            IList<EmpAcademicInfo> typedetails = null;
                            if (type.Awards != null)
                            {
                                typedetails = db.EmpAcademicInfo.Where(x => x.Awards == type.Awards && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.EmpAcademicInfo.Where(x => x.Id == data).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.Awards = c.Awards;
                                db.EmpAcademicInfo.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        else
                        {
                            var BusiTypeDetails = db.EmpAcademicInfo.Include(e => e.Awards).Where(x => x.Id == data).ToList();
                            foreach (var s in BusiTypeDetails)
                            {
                                s.Awards = null;
                                db.EmpAcademicInfo.Attach(s);
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
                        var BusiTypeDetails = db.EmpAcademicInfo.Include(e => e.Awards).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.Awards = null;
                            db.EmpAcademicInfo.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }

                    if (hob != null)
                    {
                        if (hob != "")
                        {
                            var val = db.Awards.Find(int.Parse(hob));
                            //var val = db.Hobby.Where(e => e.Id == data).SingleOrDefault();

                            var r = (from ca in db.Hobby
                                     select new
                                     {
                                         Id = ca.Id,
                                         LookupVal = ca.HobbyName
                                     }).Where(e => e.Id == data).Distinct();

                            var add = db.EmpAcademicInfo.Include(e => e.Hobby).Where(e => e.Id == data).SingleOrDefault();
                            IList<EmpAcademicInfo> addressdetails = null;
                            if (add.Hobby != null)
                            {
                                addressdetails = db.EmpAcademicInfo.Where(x => x.Hobby == add.Hobby && x.Id == data).ToList();
                            }
                            else
                            {
                                addressdetails = db.EmpAcademicInfo.Where(x => x.Id == data).ToList();
                            }
                            if (addressdetails != null)
                            {
                                foreach (var s in addressdetails)
                                {
                                    s.Hobby = c.Hobby;
                                    db.EmpAcademicInfo.Attach(s);
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
                        var addressdetails = db.EmpAcademicInfo.Include(e => e.Hobby).Where(x => x.Id == data).ToList();
                        foreach (var s in addressdetails)
                        {
                            s.Hobby = null;
                            db.EmpAcademicInfo.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }

                    if (langskl != null)
                    {
                        if (langskl != "")
                        {
                            var val = db.Awards.Find(int.Parse(langskl));
                            //var val = db.LanguageSkill.Where(e => e.Id == data).SingleOrDefault();

                            var r = (from ca in db.LanguageSkill
                                     select new
                                     {
                                         Id = ca.Id,
                                         LookupVal = ca.Language
                                     }).Where(e => e.Id == data).Distinct();

                            var add = db.EmpAcademicInfo.Include(e => e.LanguageSkill).Where(e => e.Id == data).SingleOrDefault();
                            IList<EmpAcademicInfo> contactsdetails = null;
                            if (add.LanguageSkill != null)
                            {
                                contactsdetails = db.EmpAcademicInfo.Where(x => x.LanguageSkill == add.LanguageSkill && x.Id == data).ToList();
                            }
                            else
                            {
                                contactsdetails = db.EmpAcademicInfo.Where(x => x.Id == data).ToList();
                            }
                            foreach (var s in contactsdetails)
                            {
                                s.LanguageSkill = c.LanguageSkill;
                                db.EmpAcademicInfo.Attach(s);
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
                        var contactsdetails = db.EmpAcademicInfo.Include(e => e.LanguageSkill).Where(x => x.Id == data).ToList();
                        foreach (var s in contactsdetails)
                        {
                            s.LanguageSkill = null;
                            db.EmpAcademicInfo.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }

                    if (Qualdtl != null)
                    {
                        if (Qualdtl != "")
                        {
                            var val = db.QualificationDetails.Find(int.Parse(Qualdtl));
                            // var val = db.Awards.Where(e => e.Id == data).SingleOrDefault();

                            var r = (from ca in db.QualificationDetails
                                     select new
                                     {
                                         Id = ca.Id,
                                         LookupVal = ca.FullDetails
                                     }).Where(e => e.Id == data).Distinct();

                            var type = db.EmpAcademicInfo.Include(e => e.QualificationDetails).Where(e => e.Id == data).SingleOrDefault();
                            IList<EmpAcademicInfo> typedetails = null;
                            if (type.QualificationDetails != null)
                            {
                                typedetails = db.EmpAcademicInfo.Where(x => x.QualificationDetails == type.QualificationDetails && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.EmpAcademicInfo.Where(x => x.Id == data).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.QualificationDetails = c.QualificationDetails;
                                db.EmpAcademicInfo.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        else
                        {
                            var BusiTypeDetails = db.EmpAcademicInfo.Include(e => e.QualificationDetails).Where(x => x.Id == data).ToList();
                            foreach (var s in BusiTypeDetails)
                            {
                                s.QualificationDetails = null;
                                db.EmpAcademicInfo.Attach(s);
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
                        var BusiTypeDetails = db.EmpAcademicInfo.Include(e => e.QualificationDetails).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.QualificationDetails = null;
                            db.EmpAcademicInfo.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }


                    if (skll != null)
                    {
                        if (skll != "")
                        {
                            var val = db.Skill.Find(int.Parse(skll));
                            // var val = db.Awards.Where(e => e.Id == data).SingleOrDefault();

                            var r = (from ca in db.Skill
                                     select new
                                     {
                                         Id = ca.Id,
                                         LookupVal = ca.Name
                                     }).Where(e => e.Id == data).Distinct();

                            var type = db.EmpAcademicInfo.Include(e => e.Skill).Where(e => e.Id == data).SingleOrDefault();
                            IList<EmpAcademicInfo> typedetails = null;
                            if (type.Skill != null)
                            {
                                typedetails = db.EmpAcademicInfo.Where(x => x.Skill == type.Skill && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.EmpAcademicInfo.Where(x => x.Id == data).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.Skill = c.Skill;
                                db.EmpAcademicInfo.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        else
                        {
                            var BusiTypeDetails = db.EmpAcademicInfo.Include(e => e.Skill).Where(x => x.Id == data).ToList();
                            foreach (var s in BusiTypeDetails)
                            {
                                s.Skill = null;
                                db.EmpAcademicInfo.Attach(s);
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
                        var BusiTypeDetails = db.EmpAcademicInfo.Include(e => e.Skill).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.Skill = null;
                            db.EmpAcademicInfo.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }



                    if (scolr != null)
                    {
                        if (scolr != "")
                        {
                            var val = db.Scolarship.Find(int.Parse(scolr));
                            // var val = db.Awards.Where(e => e.Id == data).SingleOrDefault();

                            var r = (from ca in db.Scolarship
                                     select new
                                     {
                                         Id = ca.Id,
                                         LookupVal = ca.Name
                                     }).Where(e => e.Id == data).Distinct();

                            var type = db.EmpAcademicInfo.Include(e => e.Scolarship).Where(e => e.Id == data).SingleOrDefault();
                            IList<EmpAcademicInfo> typedetails = null;
                            if (type.Scolarship != null)
                            {
                                typedetails = db.EmpAcademicInfo.Where(x => x.Scolarship == type.Scolarship && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.EmpAcademicInfo.Where(x => x.Id == data).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.Scolarship = c.Scolarship;
                                db.EmpAcademicInfo.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        else
                        {
                            var BusiTypeDetails = db.EmpAcademicInfo.Include(e => e.Scolarship).Where(x => x.Id == data).ToList();
                            foreach (var s in BusiTypeDetails)
                            {
                                s.Scolarship = null;
                                db.EmpAcademicInfo.Attach(s);
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
                        var BusiTypeDetails = db.EmpAcademicInfo.Include(e => e.Scolarship).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.Scolarship = null;
                            db.EmpAcademicInfo.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }


                    var CurCorp = db.EmpAcademicInfo.Find(data);
                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                    {
                        c.DBTrack = dbT;
                        EmpAcademicInfo corp = new EmpAcademicInfo()
                        {
                            Id = data,
                            DBTrack = c.DBTrack
                        };


                        db.EmpAcademicInfo.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                        return 1;
                    }
                    return 0;
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
        }
    }