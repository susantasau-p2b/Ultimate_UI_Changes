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
using System.Collections;
using EssPortal.Security;

namespace EssPortal.Controllers
{
    public class LanguageSkillController : Controller
    {
        private DataBaseContext db = new DataBaseContext();
        //
        // GET: /Language/
        public ActionResult partial()
        {
            return View("~/Views/Shared/_LanguageSkill.cshtml");
        }

        public ActionResult view_partial()
        {
            return View("~/Views/Shared/_LanguageSkillView.cshtml");
        }

        public Object Create(LanguageSkill lkval, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {



                //string Values = form["Languagelist"] == "0" ? "" : form["Languagelist"];
                //if (Values != null)
                //{
                //    List<int> IDs = Values.Split(',').Select(e => int.Parse(e)).ToList();
                //    foreach (var k in IDs)
                //    {
                //        var value = db.Language.Find(k);
                //        lkval.Language = new List<Language>();
                //        lkval.Language.Add(value);
                //    }
                //}



                List<Language> lookupval = new List<Language>();
                string Values = form["Languagelist"];

                if (Values != null)
                {
                    var ids = Utility.StringIdsToListIds(Values);
                    foreach (var ca in ids)
                    {
                        var lookup_val = db.Language.Find(ca);
                        lookupval.Add(lookup_val);
                        lkval.Language = lookupval;
                    }
                }
                else
                {
                    lkval.Language = null;
                }




                string Category = form["SkillTypelist"] == "0" ? "" : form["SkillTypelist"];

                if (Category != null)
                {
                    if (Category != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Category));
                        lkval.SkillType = val;
                    }
                }


                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        lkval.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        LanguageSkill qualdtl = new LanguageSkill
                        {
                            Language = lkval.Language,
                            SkillType = lkval.SkillType,
                            FullDetails = lkval.FullDetails,
                            DBTrack = lkval.DBTrack
                        };
                        try
                        {
                            db.LanguageSkill.Add(qualdtl);
                            var a = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, lkval.DBTrack);
                            db.SaveChanges();
                            DT_LanguageSkill DT_Corp = (DT_LanguageSkill)a;
                            DT_Corp.Orig_Id = qualdtl.Id;
                            // DT_Corp.SkillType_Id = lkval.SkillType == null ? 0 : lkval.SkillType.Id;
                            db.Create(DT_Corp);
                            db.SaveChanges();
                            var empid = Convert.ToInt32(SessionManager.EmpId);
                            var EmpAcedemicDataChk = db.Employee.Include(e => e.EmpAcademicInfo)
                                .Include(e => e.EmpAcademicInfo.LanguageSkill)
                                .Where(e => e.Id == empid).SingleOrDefault();
                            if (EmpAcedemicDataChk != null && EmpAcedemicDataChk.EmpAcademicInfo != null)
                            {
                                if (EmpAcedemicDataChk.EmpAcademicInfo.LanguageSkill != null)
                                {
                                    EmpAcedemicDataChk.EmpAcademicInfo.LanguageSkill.Add(qualdtl);
                                }
                                else
                                {
                                    EmpAcedemicDataChk.EmpAcademicInfo.LanguageSkill = new List<LanguageSkill> { qualdtl };
                                }
                            }
                            else
                            {
                                var oEmpAcademicInfo = new EmpAcademicInfo();
                                oEmpAcademicInfo.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                oEmpAcademicInfo.LanguageSkill = new List<LanguageSkill> { qualdtl };
                                EmpAcedemicDataChk.EmpAcademicInfo = oEmpAcademicInfo;
                            }
                            db.Entry(EmpAcedemicDataChk).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            ts.Complete();
                            return new { status = true, responseText = "Data Created Successfully." };
                        }

                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = lkval.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                    return new { status = false, responseText = errorMsg };
                }
            }
        }


        [HttpPost]
        public Object Create1(LanguageSkill lkval, FormCollection form)
        {

            var empid = Convert.ToInt32(SessionManager.EmpId);
            var EmpAcedemicDataChk = db.Employee.Include(e => e.EmpAcademicInfo)
                .Include(e => e.EmpAcademicInfo.LanguageSkill)
                  .Include(e => e.EmpAcademicInfo.LanguageSkill)
                .Where(e => e.Id == empid).SingleOrDefault();



            var EmpSocialInfo1 = db.Employee.Include(e => e.VisaDetails).Include(e => e.VisaDetails.Select(q => q.Country))
                                         .Include(e => e.VisaDetails.Select(q => q.VisaType))
                                            .Where(e => e.Id != null).ToList();
            Employee empdata = new Employee();
            if (empid != null && empid != 0)
            {
                empdata = db.Employee.Find(empid);
            }
            foreach (var item in EmpSocialInfo1)
            {
                if (item.EmpAcademicInfo != null && empdata.EmpAcademicInfo.LanguageSkill.Count != 0)
                {
                    if (item.EmpAcademicInfo.LanguageSkill != null && empdata.EmpAcademicInfo.LanguageSkill.Count != 0)
                    {
                        int aid = item.EmpAcademicInfo.LanguageSkill.Select(a => a.Id).FirstOrDefault();
                        int bid = empdata.EmpAcademicInfo.LanguageSkill.Select(a => a.Id).FirstOrDefault();

                        if (aid == bid)
                        {
                            return new { status = false, responseText = "Record Already Exist." };

                        }
                    }
                }

            }


            string Values = form["Languagelist"] == "0" ? "" : form["Languagelist"];
            if (Values != null)
            {
                List<int> IDs = Values.Split(',').Select(e => int.Parse(e)).ToList();
                foreach (var k in IDs)
                {
                    var value = db.Language.Find(k);
                    lkval.Language = new List<Language>();
                    lkval.Language.Add(value);
                }
            }



            string Category = form["SkillTypelist"] == "0" ? "" : form["SkillTypelist"];

            if (Category != null)
            {
                if (Category != "")
                {
                    var val = db.LookupValue.Find(int.Parse(Category));
                    lkval.SkillType = val;
                }
            }

            if (ModelState.IsValid)
            {
                using (TransactionScope ts = new TransactionScope())
                {
                    lkval.DBTrack = new DBTrack { Action = "C", CreatedBy = "0029", IsModified = false };

                    //lkval.DBTrack = new DBTrack { Action = "C", CreatedBy = "0029" };


                    LanguageSkill languageSkill = new LanguageSkill()
                    {

                        SkillType = lkval.SkillType,
                        Language = lkval.Language,
                        DBTrack = lkval.DBTrack
                    };
                    try
                    {
                        db.LanguageSkill.Add(languageSkill);
                        var a = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, lkval.DBTrack);
                        db.SaveChanges();
                        DT_LanguageSkill DT_Corp = (DT_LanguageSkill)a;
                        DT_Corp.Orig_Id = languageSkill.Id;
                        db.Create(DT_Corp);
                        db.SaveChanges();






                        if (EmpAcedemicDataChk != null && EmpAcedemicDataChk.EmpAcademicInfo != null)
                        {
                            if (EmpAcedemicDataChk.EmpAcademicInfo.LanguageSkill != null)
                            {
                                EmpAcedemicDataChk.EmpAcademicInfo.LanguageSkill.Add(lkval);
                            }
                            else
                            {
                                EmpAcedemicDataChk.EmpAcademicInfo.LanguageSkill = new List<LanguageSkill> { lkval };
                            }
                        }
                        else
                        {
                            var oEmpAcademicInfo = new EmpAcademicInfo();
                            oEmpAcademicInfo.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            oEmpAcademicInfo.LanguageSkill = new List<LanguageSkill> { lkval };
                            EmpAcedemicDataChk.EmpAcademicInfo = oEmpAcademicInfo;

                        }
                        db.Entry(EmpAcedemicDataChk).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        ts.Complete();
                        return new { status = true, responseText = "Data Created Successfully." };


                    }

                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Create", new { concurrencyError = true, id = lkval.Id });
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

        public ActionResult AddOrEdit(LanguageSkill lkval, FormCollection form)
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


        public class LangDetails
        {
            public Array Language_Id { get; set; }
            public Array Language_FullDetails { get; set; }

        }


        public ActionResult Edit(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var qurey = db.Employee.Include(e => e.EmpAcademicInfo)
                    .Include(e => e.EmpAcademicInfo.LanguageSkill)
                    .Include(e => e.EmpAcademicInfo.LanguageSkill.Select(a => a.Language))
                       .Include(e => e.EmpAcademicInfo.LanguageSkill.Select(a => a.SkillType))
                    .Where(e => e.Id == Emp && e.EmpAcademicInfo != null)
                    .AsEnumerable().Select(e => new
                    {
                        oQualificationDetails = e.EmpAcademicInfo.LanguageSkill.Where(w => w.Id == id).SingleOrDefault(),
                        DBTrack = e.DBTrack
                    }).SingleOrDefault();

                var returndata = (Object)null;
                var ListQualDetails = new List<LangDetails>();
                var returnCurrentData = (Object)null;
                if (qurey != null)
                {
                    if (qurey.oQualificationDetails != null)
                    {
                        returndata = new
                        {
                            id = qurey.oQualificationDetails.Id,
                            SkillType = qurey.oQualificationDetails.SkillType == null ? "" : qurey.oQualificationDetails.SkillType.Id.ToString(),
                            Action = qurey.oQualificationDetails.DBTrack.Action,
                            isauth = true,
                            Add = false
                        };


                        var k = db.LanguageSkill.Include(e => e.Language).Where(e => e.Id == id).Select(e => e.Language).ToList();
                        foreach (var val in k)
                        {
                            ListQualDetails.Add(new LangDetails
                            {
                                Language_Id = val.Select(e => e.Id.ToString()).ToArray(),
                                Language_FullDetails = val.Select(e => e.LanguageName).ToArray()

                            });
                        }




                        //curr data
                        var dt_data = db.DT_LanguageSkill.Where(e => e.Orig_Id == qurey.oQualificationDetails.Id && e.DBTrack.IsAuthorized == 0).OrderByDescending(e => e.Id).FirstOrDefault();
                        if (dt_data != null)
                        {
                            returnCurrentData = new
                            {
                                DT_Id = dt_data.Id,
                                SkillType_Val = dt_data.SkillType_Id == 0 ? "" : db.LookupValue
                                           .Where(x => x.Id == dt_data.SkillType_Id)
                                           .Select(x => x.LookupVal).FirstOrDefault(),
                                Language_val = dt_data.Language_Id == 0 ? "" : db.Language.Where(x => x.Id == dt_data.Language_Id).Select(x => x.LanguageName).FirstOrDefault(),



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

                    return Json(new Object[] { returndata, ListQualDetails, returnCurrentData, "", JsonRequestBehavior.AllowGet });
                }
                return Json(new Object[] { returndata, ListQualDetails, returnCurrentData, "", JsonRequestBehavior.AllowGet });
            }
        }



        public ActionResult Edit1(int data)
        {
            var un = db.LanguageSkill.Include(e => e.Language).Include(e => e.SkillType).Where(e => e.Id == data).Select
                (e => new
                {
                    SkillType_Id = e.SkillType.Id == null ? 0 : e.SkillType.Id,
                    Action = e.DBTrack.Action
                }).ToList();


            List<LangDetails> contno = new List<LangDetails>();
            var k = db.LanguageSkill.Include(e => e.Language).Where(e => e.Id == data).Select(e => e.Language).ToList();
            foreach (var val in k)
            {
                contno.Add(new LangDetails
                {
                    Language_Id = val.Select(e => e.Id.ToString()).ToArray(),
                    Language_FullDetails = val.Select(e => e.LanguageName).ToArray()

                });
            }
            //return Json(new object[] { contno }, JsonRequestBehavior.AllowGet);
            //TempData["RowVersion"] = db.IncrActivity.Find(data).RowVersion;

            //return Json(new Object[] { Q, add_data, JsonRequestBehavior.AllowGet });

            var W = db.DT_LanguageSkill
                //.Include(e => e.IncrPolicy)
                //.Include(e => e.IncrList)
                 .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                 (e => new
                 {
                     DT_Id = e.Id,
                     SkillType_Val = e.SkillType_Id == 0 ? "" : db.LookupValue
                                .Where(x => x.Id == e.SkillType_Id)
                                .Select(x => x.LookupVal).FirstOrDefault(),
                     Language_val = e.Language_Id == 0 ? "" : db.Language.Where(x => x.Id == e.Language_Id).Select(x => x.LanguageName).FirstOrDefault(),
                 }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

            var Corp = db.LanguageSkill.Find(data);
            TempData["RowVersion"] = Corp.RowVersion;
            var Auth = Corp.DBTrack.IsModified;
            return Json(new Object[] { un, contno, W, Auth, JsonRequestBehavior.AllowGet });

        }

        [HttpPost]
        public async Task<ActionResult> EditSave1(LanguageSkill c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            try
            {
                var db_data = db.LanguageSkill.Include(e => e.Language).Include(e => e.SkillType)
                                                   .Where(e => e.Id == data).SingleOrDefault();

                bool Auth = form["autho_allow"] == "true" ? true : false;

                List<Language> lookupval = new List<Language>();
                string Values = form["Languagelist"];


                if (Values != null)
                {
                    var ids = Utility.StringIdsToListIds(Values);
                    foreach (var ca in ids)
                    {
                        var lookup_val = db.Language.Find(ca);
                        lookupval.Add(lookup_val);
                        db_data.Language = lookupval;
                    }
                }
                else
                {
                    db_data.Language = null;
                }


                string Category = form["SkillTypelist"] == "0" ? "" : form["SkillTypelist"];

                if (Category != null)
                {
                    if (Category != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Category));
                        c.SkillType = val;
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
                                db.LanguageSkill.Attach(db_data);
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = db_data.RowVersion;
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                var Curr_Lookup = db.LanguageSkill.Find(data);
                                TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {

                                    LanguageSkill blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.LanguageSkill.Where(e => e.Id == data).Include(e => e.Language).Include(e => e.SkillType)
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
                                    LanguageSkill lk = new LanguageSkill
                                    {
                                        Id = data,
                                        Language = c.Language,
                                        SkillType = c.SkillType,
                                        DBTrack = c.DBTrack
                                    };


                                    db.LanguageSkill.Attach(lk);
                                    db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                    db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

                                    // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


                                    using (var context = new DataBaseContext())
                                    {

                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_LanguageSkill DT_Corp = (DT_LanguageSkill)obj;

                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = lk.Id, Val = lk.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { lk.Id, "", "Record Updated", JsonRequestBehavior.AllowGet });
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (LanguageSkill)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (LanguageSkill)databaseEntry.ToObject();
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

                        LanguageSkill blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        LanguageSkill Old_Corp = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.LanguageSkill.Include(e => e.Language).Include(e => e.SkillType).Where(e => e.Id == data).SingleOrDefault();
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
                        LanguageSkill qualificationDetails = new LanguageSkill()
                        {

                            Id = data,
                            Language = c.Language,
                            SkillType = c.SkillType,
                            DBTrack = c.DBTrack
                        };

                        using (var context = new DataBaseContext())
                        {
                            var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, qualificationDetails, "QualificationDetails", c.DBTrack);
                            // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                            Old_Corp = context.LanguageSkill.Where(e => e.Id == data)
                                .Include(e => e.Language).Include(e => e.SkillType).SingleOrDefault();
                            DT_LanguageSkill DT_Corp = (DT_LanguageSkill)obj;
                            //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                            //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                            //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                            db.Create(DT_Corp);
                            //db.SaveChanges();
                        }
                        blog.DBTrack = c.DBTrack;
                        db.LanguageSkill.Attach(blog);
                        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        ts.Complete();

                        Msg.Add("  Record Updated");
                        return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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


        [HttpPost]
        public Object EditSave(LanguageSkill c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {


                // bool Auth = form["autho_allow"] == "true" ? true : false;
                bool Auth = true;


                if (Auth == false)
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            var db_data = db.LanguageSkill.Include(e => e.Language).Include(e => e.SkillType)
                                                   .Where(e => e.Id == data).SingleOrDefault();
                            List<Language> lookupval = new List<Language>();
                            string Values = form["Languagelist"];
                            if (Values != null)
                            {
                                var ids = Utility.StringIdsToListIds(Values);
                                foreach (var ca in ids)
                                {
                                    var lookup_val = db.Language.Find(ca);
                                    lookupval.Add(lookup_val);
                                    db_data.Language = lookupval;
                                }
                            }
                            else
                            {
                                db_data.Language = null;
                            }

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                db.LanguageSkill.Attach(db_data);
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = db_data.RowVersion;
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                var Curr_Lookup = db.LanguageSkill.Find(data);
                                TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {

                                    LanguageSkill blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.LanguageSkill.Include(e => e.Language).Include(e => e.SkillType)
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
                                    LanguageSkill lk = new LanguageSkill
                                    {
                                        Id = data,
                                        Language = c.Language,
                                        SkillType = c.SkillType,
                                        DBTrack = c.DBTrack,
                                        FullDetails = c.FullDetails
                                    };


                                    db.LanguageSkill.Attach(lk);
                                    db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                    db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];


                                    using (var context = new DataBaseContext())
                                    {

                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_LanguageSkill DT_Corp = (DT_LanguageSkill)obj;

                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    //  await db.SaveChangesAsync();
                                    ts.Complete();


                                    return new { status = true, responseText = "Record Updated" };
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (LanguageSkill)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                return new { status = true, responseText = "Unable to save changes. The record was deleted by another user." };
                            }
                            else
                            {
                                var databaseValues = (LanguageSkill)databaseEntry.ToObject();
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

                        LanguageSkill blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        LanguageSkill Old_Corp = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.LanguageSkill.Where(e => e.Id == data).SingleOrDefault();
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
                        LanguageSkill qualificationDetails = new LanguageSkill()
                        {

                            Id = data,
                            SkillType = c.SkillType,
                            DBTrack = c.DBTrack,
                            RowVersion = (Byte[])TempData["RowVersion"]
                        };

                        using (var context = new DataBaseContext())
                        {
                            var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, qualificationDetails, "LanguageSkill", c.DBTrack);
                            Old_Corp = context.LanguageSkill.Where(e => e.Id == data)
                                .Include(e => e.Language).SingleOrDefault();
                            DT_LanguageSkill DT_Corp = (DT_LanguageSkill)obj;
                            db.Create(DT_Corp);
                        }
                        blog.DBTrack = c.DBTrack;
                        db.LanguageSkill.Attach(blog);
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
        public class returnDataClass
        {
            public Int32 EmpId { get; set; }
            public String val { get; set; }
            public Int32 Id2 { get; set; }
            public bool Id3 { get; set; }
            public Int32 LvHead_Id { get; set; }
        }
        public ActionResult GetMyEmpLanguageSkill()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var qurey = db.Employee.Include(e => e.EmpAcademicInfo)
                 .Include(e => e.EmpAcademicInfo.LanguageSkill.Select(a => a.Language))
                 .Include(e => e.EmpAcademicInfo.LanguageSkill.Select(a => a.SkillType))
                 .Where(e => e.Id == Emp && e.EmpAcademicInfo != null).SingleOrDefault();

                var ListreturnDataClass = new List<returnDataClass>();
                if (qurey != null && qurey.EmpAcademicInfo != null && qurey.EmpAcademicInfo.LanguageSkill.Count > 0)
                {
                    foreach (var item in qurey.EmpAcademicInfo.LanguageSkill)
                    {

                        var Name = item.SkillType != null ? item.SkillType.LookupVal.ToString() : null;
                        var Language = item.Language.Count > 0 ? item.Language.Select(e => e.LanguageName).ToArray() : null;
                        var Languagestring = Language != null ? string.Join(",", Language) : null;
                        ListreturnDataClass.Add(new returnDataClass
                        {
                            EmpId = item.Id,
                            val =
                            "Name :" + Name + " " +
                            "Language :" + Languagestring +
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



        public class GetLANGSkillClass
        {
            public string Emp { get; set; }
            public string LanguageSkillName { get; set; }
            public string LanguageSkillType { get; set; }
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

        public ActionResult GetMyEmpLanguageSkillNew()
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
                 .Include(e => e.EmpAcademicInfo.LanguageSkill.Select(a => a.Language))
                 .Include(e => e.EmpAcademicInfo.LanguageSkill.Select(a => a.SkillType))
                 .SingleOrDefault();

                if (db_data.EmpAcademicInfo != null)
                {
                    List<GetLANGSkillClass> returndata = new List<GetLANGSkillClass>();
                    returndata.Add(new GetLANGSkillClass
                    {
                        LanguageSkillName = "LanguageSkill Name",
                        LanguageSkillType = "Skill Type",
                        //LvHead = "Leave Head",
                        //FromDate = "From Date",
                        //ToDate = "To Date"
                    });
                    foreach (var item in db_data.EmpAcademicInfo.LanguageSkill.OrderByDescending(a => a.Id).ToList())
                    {
                        var LangSkillName = item.Language != null ? item.Language.Select(l => l.LanguageName).ToArray() : null;
                        var LangSkills = LangSkillName[0].ToString();
                        var LangSkillType = item.SkillType != null ? item.SkillType.LookupVal.ToString() : null;

                        returndata.Add(new GetLANGSkillClass
                        {
                            RowData = new ChildGetLvNewReqClass
                            {
                                LvNewReq = item.Id.ToString(),
                                EmpLVid = db_data.Id.ToString(),
                                LvHead_Id = "",
                            },
                            LanguageSkillName = LangSkills,
                            LanguageSkillType = LangSkillType.ToString(),

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



        public ActionResult GetLanguageLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Language.ToList();
                IEnumerable<Language> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Language.ToList().Where(d => d.LanguageName.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.LanguageName }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.LanguageName }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        private MultiSelectList GetLookupValues(List<int> selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Language> lkval = new List<Language>();
                lkval = db.Language.ToList();
                return new MultiSelectList(lkval, "Id", "LanguageName", selectedValues);
            }
        }
    }
}