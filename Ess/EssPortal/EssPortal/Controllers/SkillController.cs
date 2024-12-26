///
/// Created by Kapil
///
using P2b.Global;
using EssPortal.App_Start;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using EssPortal.Models;
using System.Threading.Tasks;
using EssPortal.Security;
namespace EssPortal.Controllers.Core.MainController
{
    public class SkillController : Controller
    {
        public ActionResult Partial()
        {
            return View("~/Views/Shared/_Skill.cshtml");
        }
        public ActionResult Partial_View()
        {
            return View("~/Views/Shared/_SkillView.cshtml");
        }
        public class returnDataClass
        {
            public Int32 EmpId { get; set; }
            public String val { get; set; }
            public Int32 Id2 { get; set; }
            public bool Id3 { get; set; }
            public Int32 LvHead_Id { get; set; }
        }
        public ActionResult GetSkill()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var qurey = db.Employee
                    .Include(e => e.EmpAcademicInfo.Skill)
                    .Include(e => e.EmpAcademicInfo.Skill.Select(a => a.SkillType))
                    .Where(e => e.Id == Emp)
                    .SingleOrDefault();

                var ListreturnDataClass = new List<returnDataClass>();
                if (qurey != null && qurey.EmpAcademicInfo != null && qurey.EmpAcademicInfo.Skill != null && qurey.EmpAcademicInfo.Skill.Count > 0)
                {
                    foreach (var item in qurey.EmpAcademicInfo.Skill)
                    {
                        var Name = item.Name != null ? item.Name : null;
                        var SkillType = item.SkillType != null ? item.SkillType.LookupVal : null;
                        ListreturnDataClass.Add(new returnDataClass
                        {
                            EmpId = item.Id,
                            val =
                            "Name :" + Name +
                            ", SkillType :" + SkillType + ""
                        });
                    }
                }
                if (ListreturnDataClass != null && ListreturnDataClass.Count > 0)
                {
                    return Json(new { status = true, data = ListreturnDataClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = ListreturnDataClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        [HttpPost]
        public Object Create(Skill lkval, FormCollection form) //Create submit
        {
            lkval.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId, IsModified = false };

            Skill LookupValue = new Skill
            {
                Id = lkval.Id,
                Name = lkval.Name,
                SkillType = lkval.SkillType,
                FullDetails = lkval.FullDetails,
                DBTrack = lkval.DBTrack
            };
            try
            {
                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            db.Skill.Add(LookupValue);
                            var a = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, lkval.DBTrack);
                            db.SaveChanges();
                            DT_Skill DT_LKVal = (DT_Skill)a;
                            DT_LKVal.Orig_Id = LookupValue.Id;
                            db.Create(DT_LKVal);
                            db.SaveChanges();
                            var empid = Convert.ToInt32(SessionManager.EmpId);
                            var EmpAcedemicDataChk = db.Employee.Include(e => e.EmpAcademicInfo)
                                .Include(e => e.EmpAcademicInfo.Skill)
                                .Where(e => e.Id == empid).SingleOrDefault();
                            if (EmpAcedemicDataChk != null && EmpAcedemicDataChk.EmpAcademicInfo != null)
                            {
                                if (EmpAcedemicDataChk.EmpAcademicInfo.Skill != null)
                                {
                                    EmpAcedemicDataChk.EmpAcademicInfo.Skill.Add(LookupValue);
                                }
                                else
                                {
                                    EmpAcedemicDataChk.EmpAcademicInfo.Skill = new List<Skill> { LookupValue };
                                }
                            }
                            else
                            {
                                var oEmpAcademicInfo = new EmpAcademicInfo();
                                oEmpAcademicInfo.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId, IsModified = false };
                                oEmpAcademicInfo.Skill = new List<Skill> { LookupValue };
                                EmpAcedemicDataChk.EmpAcademicInfo = oEmpAcademicInfo;
                            }
                            db.Entry(EmpAcedemicDataChk).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            ts.Complete();
                            return Json(new { status = true, responseText = "Data Created Successfully." });
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
                    return Json(new { status = false, responseText = errorMsg });
                }

            }
            catch (DataException e) { throw e; }
            catch (DBConcurrencyException e) { throw e; }
        }


        //public ActionResult Edit1(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var ids = Utility.StringIdsToListString(data);
        //        var id = Convert.ToInt32(ids[0]);
        //        var Emp = Convert.ToInt32(SessionManager.EmpId);
        //        var qurey = db.Employee.Include(e => e.EmpAcademicInfo)
        //            .Include(e => e.EmpAcademicInfo.Skill)
        //            .Include(e => e.EmpAcademicInfo.Skill.Select(a => a.SkillType))
        //            .Where(e => e.Id == Emp && e.EmpAcademicInfo != null && e.EmpAcademicInfo.Skill.Any(a => a.Id == id))
        //            .AsEnumerable().Select(e => new
        //            {
        //                id = qurey.Id,
        //                FamilyDetails = e.EmpAcademicInfo.Skill.Where(a => a.Id == id).SingleOrDefault(),
        //            }).SingleOrDefault();
        //        var listofdata = qurey != null && qurey.FamilyDetails != null ? qurey.FamilyDetails : null;
        //      //  return Json(new Object[] { qurey, listofdata, JsonRequestBehavior.AllowGet });
        //        return Json(new Object[] { qurey, listofdata, "", "", JsonRequestBehavior.AllowGet });


        //    }
        //}

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.Skill.Where(e => e.Id == data).Select
                 (e => new
                 {
                     Name = e.Name,
                     Action = e.DBTrack.Action
                 }).ToList();

                var add_data = db.Skill.Where(e => e.Id == data)
                   .ToList();

                //TempData["RowVersion"] = db.IncrActivity.Find(data).RowVersion;

                //return Json(new Object[] { Q, add_data, JsonRequestBehavior.AllowGet });

                var W = db.DT_Skill
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Name = e.Name == null ? "" : e.Name,
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.Skill.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });

            }

        }


        public int EditS(int data, Skill c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CurCorp = db.Skill.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    Skill corp = new Skill()
                    {
                        Name = c.Name,
                        Id = data,
                        DBTrack = c.DBTrack
                    };

                    db.Skill.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }



        [HttpPost]
        public Object EditSave(Skill c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {


                bool Auth = form["autho_allow"] == "true" ? true : false;
                //bool Auth = true;


                if (Auth == false)
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            var db_data = db.Skill.Include(e => e.SkillType)
                                                   .Where(e => e.Id == data).SingleOrDefault();


                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                db.Skill.Attach(db_data);
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = db_data.RowVersion;
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                var Curr_Lookup = db.Skill.Find(data);
                                TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;
                                List<string> Msg = new List<string>();
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {

                                    Skill blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.Skill.Include(e => e.SkillType)
                                                   .Where(e => e.Id == data).SingleOrDefault();

                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = "0029",
                                        ModifiedOn = DateTime.Now
                                    };
                                    Skill lk = new Skill
                                    {
                                        Id = data,
                                        Name = c.Name,

                                        DBTrack = c.DBTrack,
                                        FullDetails = c.FullDetails
                                    };


                                    db.Skill.Attach(lk);
                                    db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                    db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];


                                    using (var context = new DataBaseContext())
                                    {

                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_Skill DT_Corp = (DT_Skill)obj;

                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    //  await db.SaveChangesAsync();
                                    Msg.Add("Record Updated");
                                    var skilldata = db.Skill.Include(e => e.SkillType)
                                                   .Where(e => e.Id == data).SingleOrDefault();
                                    ts.Complete();
                                    return Json(new Utility.JsonReturnClass { Id = skilldata.Id, Val = skilldata.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                   // return new { status = true, responseText = "Record Updated" };
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (Skill)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                return new { status = true, responseText = "Unable to save changes. The record was deleted by another user." };
                            }
                            else
                            {
                                var databaseValues = (Skill)databaseEntry.ToObject();
                                c.RowVersion = databaseValues.RowVersion;

                            }
                        }

                        return new { status = true, responseText = "Record modified by another user.So refresh it and try to save again." };
                    }
                }
                else
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        Skill blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        Skill Old_Corp = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.Skill.Where(e => e.Id == data).SingleOrDefault();
                            originalBlogValues = context.Entry(blog).OriginalValues;
                        }
                        c.DBTrack = new DBTrack
                        {
                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                            Action = "M",
                            IsModified = blog.DBTrack.IsModified == true ? true : false,
                            ModifiedBy = "0029",
                            ModifiedOn = DateTime.Now
                        };
                        Skill qualificationDetails = new Skill()
                        {

                            Id = data,

                            DBTrack = c.DBTrack,
                            RowVersion = (Byte[])TempData["RowVersion"]
                        };

                        using (var context = new DataBaseContext())
                        {
                            var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, qualificationDetails, "Skill", c.DBTrack);
                            Old_Corp = context.Skill.Where(e => e.Id == data)
                              .SingleOrDefault();
                            DT_Skill DT_Corp = (DT_Skill)obj;
                            db.Create(DT_Corp);
                        }
                        blog.DBTrack = c.DBTrack;
                        db.Skill.Attach(blog);
                        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                        // db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        ts.Complete();
                        return new { status = true, responseText = "Record Updated" };
                    }
                }
                return new Object[] { };
            }
        }


        //[HttpPost]
        //public Object  EditSave1(Skill c, int data, FormCollection form) // Edit submit
        //{
        //    //  bool Auth = form["autho_action"] == "" ? false : true;
        //    bool Auth = form["autho_allow"] == "true" ? true : false;
        //    using (DataBaseContext db = new DataBaseContext())
        //    {

        //        if (Auth == false)
        //        {
        //            if (ModelState.IsValid)
        //            {
        //                try
        //                {

        //                    //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                    {
        //                        Skill blog = null; // to retrieve old data
        //                        // DbPropertyValues originalBlogValues = null;

        //                        using (var context = new DataBaseContext())
        //                        {
        //                            blog = context.Skill.Where(e => e.Id == data)
        //                                                    .AsNoTracking().SingleOrDefault();
        //                            // originalBlogValues = context.Entry(blog).OriginalValues;
        //                        }

        //                        c.DBTrack = new DBTrack
        //                        {
        //                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                            Action = "M",
        //                            ModifiedBy = "0029",
        //                            ModifiedOn = DateTime.Now
        //                        };

        //                        int a = EditS(data, c, c.DBTrack);

        //                        await db.SaveChangesAsync();

        //                        using (var context = new DataBaseContext())
        //                        {

        //                            //To save data in history table 
        //                            var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, c, "Skill", c.DBTrack);
        //                            DT_Skill DT_Corp = (DT_Skill)Obj;
        //                            db.DT_Skill.Add(DT_Corp);
        //                            db.SaveChanges();
        //                        }

        //                        ts.Complete();


        //                       // return Json(new Object[] { status = true, responseText = "Record Updated", JsonRequestBehavior.AllowGet });
        //                        return Json(new { status = true, responseText = "Record Updated", JsonRequestBehavior.AllowGet });
        //                    }
        //                }
        //                catch (DbUpdateConcurrencyException ex)
        //                {
        //                    var entry = ex.Entries.Single();
        //                    var clientValues = (Skill)entry.Entity;
        //                    var databaseEntry = entry.GetDatabaseValues();
        //                    if (databaseEntry == null)
        //                    {
        //                        return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                    }
        //                    else
        //                    {
        //                        var databaseValues = (Skill)databaseEntry.ToObject();
        //                        c.RowVersion = databaseValues.RowVersion;

        //                    }
        //                }

        //                return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //            }
        //        }
        //        else
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {
        //                Skill Old_Corp = db.Skill.Where(e => e.Id == data).SingleOrDefault();

        //                Skill Curr_Corp = c;
        //                c.DBTrack = new DBTrack
        //                {
        //                    CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
        //                    CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
        //                    Action = "M",
        //                    IsModified = Old_Corp.DBTrack.IsModified == true ? true : false,
        //                    //ModifiedBy = "0029",
        //                    //ModifiedOn = DateTime.Now
        //                };
        //                Old_Corp.DBTrack = c.DBTrack;

        //                db.Entry(Old_Corp).State = System.Data.Entity.EntityState.Modified;
        //                db.SaveChanges();
        //                using (var context = new DataBaseContext())
        //                {
        //                    //DBTrackFile.DBTrackSave("Core/P2b.Global", "M", Old_Corp, Curr_Corp, "Skill", c.DBTrack);
        //                }

        //                ts.Complete();
        //                return Json(new Object[] { Old_Corp.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
        //            }

        //        }
        //        return View();

        //    }
        //}

        private int EditS(int data, Hobby c, DBTrack dBTrack)
        {
            throw new NotImplementedException();
        }
        public ActionResult AddOrEdit(Skill lkval, FormCollection form)
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
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (auth_action == "C")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        Skill corp = db.Skill.FirstOrDefault(e => e.Id == auth_id);

                        corp.DBTrack = new DBTrack
                        {
                            Action = "C",
                            ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                            CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                            CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                            IsModified = corp.DBTrack.IsModified == true ? false : false,
                            AuthorizedBy = "0029",
                            AuthorizedOn = DateTime.Now
                        };

                        db.Skill.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //db.SaveChanges();

                        await db.SaveChangesAsync();


                        using (var context = new DataBaseContext())
                        {

                            ////DBTrackFile.DBTrackSave("Core/P2b.Global", "M", corp, null, "Skill", corp.DBTrack);
                        }

                        ts.Complete();
                        return Json(new Object[] { corp.Id, corp.Name, "Record Authorised", JsonRequestBehavior.AllowGet });
                    }
                }
                else if (auth_action == "M")
                {

                    Skill Old_Corp = db.Skill.Where(e => e.Id == auth_id).SingleOrDefault();


                    DT_Skill Curr_Corp = db.DT_Skill
                                                .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                .OrderByDescending(e => e.Id)
                                                .FirstOrDefault();

                    Skill corp_hobby = new Skill();


                    corp_hobby.Name = Curr_Corp.Name == null ? Old_Corp.Name : Curr_Corp.Name;
                    //corp_hobby.Id = auth_id;

                    if (ModelState.IsValid)
                    {
                        try
                        {

                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                // db.Configuration.AutoDetectChangesEnabled = false;
                                corp_hobby.DBTrack = new DBTrack
                                {
                                    CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                                    CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
                                    ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
                                    AuthorizedBy = "0029",
                                    AuthorizedOn = DateTime.Now,
                                    IsModified = false
                                };

                                int a = EditS(auth_id, corp_hobby, corp_hobby.DBTrack);

                                await db.SaveChangesAsync();

                                ts.Complete();
                                return Json(new Object[] { corp_hobby.Id, corp_hobby.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (Skill)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (Hobby)databaseEntry.ToObject();
                                corp_hobby.RowVersion = databaseValues.RowVersion;
                            }
                        }

                        return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                    }
                }
                else if (auth_action == "D")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //Corporate corp = db.Corporate.Find(auth_id);
                        Skill corp = db.Skill.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                        corp.DBTrack = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                            CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                            CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                            IsModified = corp.DBTrack.IsModified == true ? false : false,
                            AuthorizedBy = "0029",
                            AuthorizedOn = DateTime.Now
                        };

                        db.Skill.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;
                        await db.SaveChangesAsync();
                        using (var context = new DataBaseContext())
                        {

                            ////DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corp, null, "Skill", corp.DBTrack);
                        }


                        db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return Json(new Object[] { "", "", "Record Authorised", JsonRequestBehavior.AllowGet });
                    }

                }
                return View();
            }
        }
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                Skill skill = db.Skill.Where(e => e.Id == data).SingleOrDefault();

                //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                if (skill.DBTrack.IsModified == true)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            CreatedBy = skill.DBTrack.CreatedBy != null ? skill.DBTrack.CreatedBy : null,
                            CreatedOn = skill.DBTrack.CreatedOn != null ? skill.DBTrack.CreatedOn : null,
                            IsModified = skill.DBTrack.IsModified == true ? true : false
                        };
                        skill.DBTrack = dbT;
                        db.Entry(skill).State = System.Data.Entity.EntityState.Modified;
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                        await db.SaveChangesAsync();
                        using (var context = new DataBaseContext())
                        {
                            ////DBTrackFile.DBTrackSave("Core/P2b.Global", "D", skill, null, "Skill", skill.DBTrack);
                        }
                        ts.Complete();
                        return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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
                                ModifiedBy = "0029",
                                ModifiedOn = DateTime.Now,
                                CreatedBy = skill.DBTrack.CreatedBy != null ? skill.DBTrack.CreatedBy : null,
                                CreatedOn = skill.DBTrack.CreatedOn != null ? skill.DBTrack.CreatedOn : null,
                                IsModified = skill.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = "0029",
                                AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = "0029", ModifiedOn = DateTime.Now };

                            db.Entry(skill).State = System.Data.Entity.EntityState.Deleted;
                            await db.SaveChangesAsync();


                            using (var context = new DataBaseContext())
                            {

                                ////DBTrackFile.DBTrackSave("Core/P2b.Global", "D", skill, null, "Skill", dbT);
                            }
                            ts.Complete();
                            return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            //Log the error (uncomment dex variable name and add a line here to write a log.)
                            //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                            //return RedirectToAction("Delete");
                            return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                        }
                    }
                }

            }
        }

        public ActionResult GetMyEmpSkill()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var qurey = db.Employee.Include(e => e.EmpAcademicInfo)
                 .Include(e => e.EmpAcademicInfo.Skill)

                 .Where(e => e.Id == Emp && e.EmpAcademicInfo != null).SingleOrDefault();

                var ListreturnDataClass = new List<returnDataClass>();
                if (qurey != null && qurey.EmpAcademicInfo != null && qurey.EmpAcademicInfo.Skill.Count > 0)
                {
                    foreach (var item in qurey.EmpAcademicInfo.Skill)
                    {

                        var Name = item.Name != null ? item.Name : null;

                        ListreturnDataClass.Add(new returnDataClass
                        {
                            EmpId = item.Id,
                            val =
                            "Name :" + Name 
                    
                        });
                    }
                }
                if (ListreturnDataClass != null && ListreturnDataClass.Count > 0)
                {
                    return Json(new { status = true, data = ListreturnDataClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = ListreturnDataClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public class GetSkillClass
        {
            public string Emp { get; set; }
            public string SkillName { get; set; }
            public string LvHead { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public ChildGetLvNewReqClass RowData { get; set; }
        }

        public class ChildGetLvNewReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }

        }

        public ActionResult GetMySkillNew()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);
                //var db_data = db.EmployeeLeave
                //      .Where(e => e.Id == Id)
                //      .Include(e => e.LvNewReq.Select(a => a.LvWFDetails))
                //      .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                //     .SingleOrDefault();

                var db_data = db.Employee
                 .Where(e => e.Id == Id).Include(e => e.EmpAcademicInfo)
                 .Include(e => e.EmpAcademicInfo.Skill)
                 .SingleOrDefault();

                if (db_data.EmpAcademicInfo != null)
                {
                    List<GetSkillClass> returndata = new List<GetSkillClass>();
                    returndata.Add(new GetSkillClass
                    {
                        SkillName = "Skill Name",
                        //LvHead = "Leave Head",
                        //FromDate = "From Date",
                        //ToDate = "To Date"
                    });
                    foreach (var item in db_data.EmpAcademicInfo.Skill.OrderByDescending(a => a.Name).ToList())
                    {
                        var SkillName = item.Name != null ? item.Name.ToString() : null;


                        returndata.Add(new GetSkillClass
                        {
                            RowData = new ChildGetLvNewReqClass
                            {
                                LvNewReq = item.Id.ToString(),
                                EmpLVid = db_data.Id.ToString(),
                                LvHead_Id = "",
                            },
                            SkillName = item.Name.ToString()

                        });


                    }
                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
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