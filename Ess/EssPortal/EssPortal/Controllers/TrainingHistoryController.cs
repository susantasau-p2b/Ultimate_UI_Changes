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
using Training;
using Payroll;
namespace EssPortal.Controllers.Tarining.MainController
{
    public class TrainingHistoryController : Controller
    {
        public ActionResult Partial()
        {
            return View("~/Views/Shared/_TrainingHistory.cshtml");
        }
        public ActionResult Partial_View()
        {
            return View("~/Views/Shared/_TrainingHistoryView.cshtml");
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

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        [HttpPost]
        public Object Create(YearlyTrainingCalendar lkval, FormCollection form) //Create submit
        {
            lkval.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId, IsModified = false };
            string TrainigProgramCalendar = form["TrainigProgramCalendarlist"] == null ? null : form["TrainigProgramCalendarlist"];
            YearlyTrainingCalendar LookupValue = new YearlyTrainingCalendar
            {
                Id = lkval.Id,
                TrainigProgramCalendar = lkval.TrainigProgramCalendar,
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
                            db.YearlyTrainingCalendar.Add(LookupValue);
                            var a = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, lkval.DBTrack);
                            db.SaveChanges();
                            DT_YearlyTrainingCalendar DT_LKVal = (DT_YearlyTrainingCalendar)a;
                            DT_LKVal.Orig_Id = LookupValue.Id;
                            db.Create(DT_LKVal);
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
        public class ChildGetLvNewReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }

        }
        public class GetLvNewReqClass
        {
            public string BatchName { get; set; }
            public string StartDate { get; set; }
            public string ProgramName { get; set; }
            public string EndDate { get; set; }
            public string ProgramList { get; set; }

            public ChildGetLvNewReqClass RowData { get; set; }
        }
        public ActionResult GetMyTrainingHistory()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);
                var db_data = db.EmployeeTraining.Include(e => e.Employee)
                        .Include(e => e.TrainingDetails)
                        .Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule))
                        .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo))
                        .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession)))
                        .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession.TrainingProgramCalendar)))
                        .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession.TrainingProgramCalendar.ProgramList)))
                        .Where(e => e.Employee.Id == Id).SingleOrDefault();

                List<GetLvNewReqClass> returndata = new List<GetLvNewReqClass>();
                returndata.Add(new GetLvNewReqClass
                {
                    BatchName = "BatchName",
                    StartDate = "StartDate",
                    EndDate = "EndDate",
                    ProgramList = "ProgramList"
                });

                foreach (var item in db_data.TrainingDetails)
                {
                    foreach (var item1 in item.TrainigDetailSessionInfo.Where(e =>e.IsCancelled != true))
                    {
                        returndata.Add(new GetLvNewReqClass
                        {
                            RowData = new ChildGetLvNewReqClass
                                 {
                                     LvNewReq = item.Id.ToString(),
                                     EmpLVid = db_data.Id.ToString(),
                                     // IsClose = Status,
                                     // Status = Status,
                                     // LvHead_Id = item.LeaveHead.Id.ToString(),
                                 },
                            BatchName = item.TrainingSchedule.TrainingBatchName,
                            StartDate = item1.TrainingSession.TrainingProgramCalendar.StartDate.Value.ToShortDateString(),
                            EndDate = item1.TrainingSession.TrainingProgramCalendar.EndDate.Value.ToShortDateString(),
                            ProgramList = item1.TrainingSession.TrainingProgramCalendar.ProgramList.FullDetails
                        });

                    }
                }
                return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                
            }
        }

        public ActionResult Edit(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var qurey = db.Employee.Include(e => e.EmpAcademicInfo)
                    .Include(e => e.EmpAcademicInfo.Skill)

                    .Where(e => e.Id == Emp && e.EmpAcademicInfo != null)
                    .AsEnumerable().Select(e => new
                    {
                        Skill = e.EmpAcademicInfo.Skill.Where(w => w.Id == id).SingleOrDefault(),
                        DBTrack = e.DBTrack
                    }).SingleOrDefault();

                var returndata = (Object)null;
                var returnCurrentData = (Object)null;
                if (qurey != null)
                {
                    if (qurey.Skill != null)
                    {
                        returndata = new
                        {
                            id = qurey.Skill.Id,
                            Name = qurey.Skill.Name,
                            Action = qurey.Skill.DBTrack.Action,
                            isauth = true,
                            Add = false
                        };

                        //curr data
                        var dt_data = db.DT_Skill.Where(e => e.Orig_Id == qurey.Skill.Id && e.DBTrack.IsAuthorized == 0).OrderByDescending(e => e.Id).FirstOrDefault();
                        if (dt_data != null)
                        {
                            returnCurrentData = new
                            {
                                Action = qurey.DBTrack.Action,
                            };
                        }
                    }
                    else
                    {
                        returndata = new
                        {
                            Add = true,
                        };
                    }

                    return Json(new Object[] { returndata, returnCurrentData, "", JsonRequestBehavior.AllowGet });
                }
                return Json(new Object[] { returndata, returnCurrentData, "", JsonRequestBehavior.AllowGet });
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



        //[HttpPost]
        //public Object EditSave(Skill c, int data, FormCollection form) // Edit submit
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {


        //        // bool Auth = form["autho_allow"] == "true" ? true : false;
        //        bool Auth = true;


        //        if (Auth == false)
        //        {
        //            if (ModelState.IsValid)
        //            {
        //                try
        //                {
        //                    var db_data = db.Skill.Include(e => e.SkillType)
        //                                           .Where(e => e.Id == data).SingleOrDefault();


        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                    {
        //                        db.Skill.Attach(db_data);
        //                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = db_data.RowVersion;
        //                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

        //                        var Curr_Lookup = db.Skill.Find(data);
        //                        TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
        //                        db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

        //                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                        {

        //                            Skill blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.Skill.Include(e => e.SkillType)
        //                                           .Where(e => e.Id == data).SingleOrDefault();

        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            c.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = "0029",
        //                                ModifiedOn = DateTime.Now
        //                            };
        //                            Skill lk = new Skill
        //                            {
        //                                Id = data,
        //                                Name = c.Name,

        //                                DBTrack = c.DBTrack,
        //                                FullDetails = c.FullDetails
        //                            };


        //                            db.Skill.Attach(lk);
        //                            db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

        //                            db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];


        //                            using (var context = new DataBaseContext())
        //                            {

        //                                var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                                DT_Skill DT_Corp = (DT_Skill)obj;

        //                                db.Create(DT_Corp);
        //                                db.SaveChanges();
        //                            }
        //                            //  await db.SaveChangesAsync();
        //                            ts.Complete();


        //                            return new { status = true, responseText = "Record Updated" };
        //                        }
        //                    }
        //                }
        //                catch (DbUpdateConcurrencyException ex)
        //                {
        //                    var entry = ex.Entries.Single();
        //                    var clientValues = (Skill)entry.Entity;
        //                    var databaseEntry = entry.GetDatabaseValues();
        //                    if (databaseEntry == null)
        //                    {
        //                        return new { status = true, responseText = "Unable to save changes. The record was deleted by another user." };
        //                    }
        //                    else
        //                    {
        //                        var databaseValues = (Skill)databaseEntry.ToObject();
        //                        c.RowVersion = databaseValues.RowVersion;

        //                    }
        //                }

        //                return new { status = true, responseText = "Record modified by another user.So refresh it and try to save again." };
        //            }
        //        }
        //        else
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {

        //                Skill blog = null; // to retrieve old data
        //                DbPropertyValues originalBlogValues = null;
        //                Skill Old_Corp = null;

        //                using (var context = new DataBaseContext())
        //                {
        //                    blog = context.Skill.Where(e => e.Id == data).SingleOrDefault();
        //                    originalBlogValues = context.Entry(blog).OriginalValues;
        //                }
        //                c.DBTrack = new DBTrack
        //                {
        //                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                    Action = "M",
        //                    IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                    ModifiedBy = "0029",
        //                    ModifiedOn = DateTime.Now
        //                };
        //                Skill qualificationDetails = new Skill()
        //                {

        //                    Id = data,

        //                    DBTrack = c.DBTrack,
        //                    RowVersion = (Byte[])TempData["RowVersion"]
        //                };

        //                using (var context = new DataBaseContext())
        //                {
        //                    var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, qualificationDetails, "Skill", c.DBTrack);
        //                    Old_Corp = context.Skill.Where(e => e.Id == data)
        //                      .SingleOrDefault();
        //                    DT_Skill DT_Corp = (DT_Skill)obj;
        //                    db.Create(DT_Corp);
        //                }
        //                blog.DBTrack = c.DBTrack;
        //                db.Skill.Attach(blog);
        //                db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                // db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                db.SaveChanges();
        //                ts.Complete();
        //                return new { status = true, responseText = "Record Updated" };
        //            }
        //        }
        //        return new Object[] { };
        //    }
        //}


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
        public ActionResult AddOrEdit(YearlyTrainingCalendar lkval, FormCollection form)
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
                    var returnobj = Create(lkval, form);
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
                            "Name :" + Name +
                            ""
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

        [HttpPost]
        public ActionResult GetSubInvPayLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.YearlyTrainingCalendar.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.YearlyTrainingCalendar.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
    }
}