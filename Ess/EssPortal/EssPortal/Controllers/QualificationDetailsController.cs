using P2b.Global;
using Payroll;
using EssPortal.App_Start;
using EssPortal.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Threading.Tasks;
using EssPortal.Security;

namespace EssPortal.Controllers
{
    public class QualificationDetailsController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/QualificationDetails/Index.cshtml");
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/_QualificationDetails.cshtml");
        }
        public ActionResult view_partial()
        {
            return View("~/Views/Shared/_QualificationDetailsView.cshtml");
        }
        public ActionResult GetQualificationLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                List<int> qid = new List<int>();
                var chkQualiDetailsNullinQualification = db.QualificationDetails.Include(e => e.Qualification).ToList();
                //   var ax = chkQualiDetailsNullinQualification.Where(e => e.Qualification.Select(a => a.Id) != null).ToList().Select(s => s.Qualification).ToList();

                foreach (var item in chkQualiDetailsNullinQualification)
                {
                    var qq = item.Qualification.ToList();
                    foreach (var item1 in qq)
                    {
                        qid.Add(item1.Id);
                    }
                }
                var fall = db.Qualification.Where(e => !qid.Contains(e.Id)).ToList();


               
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
        private MultiSelectList GetLookupValues(List<int> selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Qualification> lkval = new List<Qualification>();
                lkval = db.Qualification.ToList();
                return new MultiSelectList(lkval, "Id", "QualificationShortName", selectedValues);
            }
        }
        public Object Create(QualificationDetails lkval, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string Qual = form["Qualificationlist"] == "0" ? "" : form["Qualificationlist"];
                if (Qual != null)
                {
                    List<int> IDs = Qual.Split(',').Select(e => int.Parse(e)).ToList();
                    foreach (var k in IDs)
                    {
                        var value = db.Qualification.Find(k);
                        lkval.Qualification = new List<Qualification>();
                        lkval.Qualification.Add(value);
                    }
                }
                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        lkval.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        QualificationDetails qualdtl = new QualificationDetails
                        {
                            Qualification = lkval.Qualification,
                            Institute = lkval.Institute,
                            PasingYear = lkval.PasingYear,
                            ResultGradation = lkval.ResultGradation,
                            ResultPercentage = lkval.ResultPercentage,
                            ResultSubmissionDate = lkval.ResultSubmissionDate,
                            University = lkval.University,
                            SpecialFeature = lkval.SpecialFeature,
                            SpecilisedBranch = lkval.SpecilisedBranch,
                            FullDetails = lkval.FullDetails,
                            DBTrack = lkval.DBTrack
                        };
                        try
                        {
                            db.QualificationDetails.Add(qualdtl);
                            var a = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, lkval.DBTrack);
                            db.SaveChanges();
                            DT_QualificationDetails DT_Corp = (DT_QualificationDetails)a;
                            DT_Corp.Orig_Id = qualdtl.Id;
                            // DT_Corp.SkillType_Id = lkval.SkillType == null ? 0 : lkval.SkillType.Id;
                            db.Create(DT_Corp);
                            db.SaveChanges();
                            var empid = Convert.ToInt32(SessionManager.EmpId);
                            var EmpAcedemicDataChk = db.Employee.Include(e => e.EmpAcademicInfo)
                                .Include(e => e.EmpAcademicInfo.QualificationDetails)
                                .Where(e => e.Id == empid).SingleOrDefault();
                            if (EmpAcedemicDataChk != null && EmpAcedemicDataChk.EmpAcademicInfo != null)
                            {
                                if (EmpAcedemicDataChk.EmpAcademicInfo.QualificationDetails != null)
                                {
                                    EmpAcedemicDataChk.EmpAcademicInfo.QualificationDetails.Add(qualdtl);
                                }
                                else
                                {
                                    EmpAcedemicDataChk.EmpAcademicInfo.QualificationDetails = new List<QualificationDetails> { qualdtl };
                                }
                            }
                            else
                            {
                                var oEmpAcademicInfo = new EmpAcademicInfo();
                                oEmpAcademicInfo.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                oEmpAcademicInfo.QualificationDetails = new List<QualificationDetails> { qualdtl };
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
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                QualificationDetails corporates = db.QualificationDetails.Include(e => e.Qualification)
                                                                                                .Where(e => e.Id == data).SingleOrDefault();

                //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                if (corporates.DBTrack.IsModified == true)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                            CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                            IsModified = corporates.DBTrack.IsModified == true ? true : false
                        };
                        corporates.DBTrack = dbT;
                        db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
                        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack);
                        DT_QualificationDetails DT_Corp = (DT_QualificationDetails)rtn_Obj;
                        //DT_Corp.Language_Id = corporates.Language == null ? 0 : corporates.Language.Id;


                        db.Create(DT_Corp);
                        // db.SaveChanges();
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                        await db.SaveChangesAsync();
                        //using (var context = new DataBaseContext())
                        //{
                        //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                        //}
                        ts.Complete();
                        return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    }
                }
                else
                {
                    var selectedRegions = corporates.Qualification;

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        if (selectedRegions != null)
                        {
                            var corpRegion = new HashSet<int>(corporates.Qualification.Select(e => e.Id));
                            if (corpRegion.Count > 0)
                            {
                                return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            }
                        }

                        try
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = "0029",
                                ModifiedOn = DateTime.Now,
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = "0029",
                                //AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = "0029", ModifiedOn = DateTime.Now };

                            db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            DT_QualificationDetails DT_Corp = (DT_QualificationDetails)rtn_Obj;
                            //DT_Corp.Language_Id = add == null ? 0 : add.Id;
                            // DT_Corp.SkillType_Id = val == null ? 0 : val.Id;

                            db.Create(DT_Corp);

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
        public class QualDetails
        {
            public Array Qual_Id { get; set; }
            public Array Qual_FullDetails { get; set; }
            //Location
        }
        public ActionResult AddOrEdit(QualificationDetails lkval, FormCollection form)
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
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var qurey = db.Employee.Include(e => e.EmpAcademicInfo)
                    .Include(e => e.EmpAcademicInfo.QualificationDetails)
                    .Include(e => e.EmpAcademicInfo.QualificationDetails.Select(a => a.Qualification))
                    .Where(e => e.Id == Emp && e.EmpAcademicInfo != null)
                    .AsEnumerable().Select(e => new
                    {
                        oQualificationDetails = e.EmpAcademicInfo.QualificationDetails.Where(w => w.Id == id).SingleOrDefault(),
                        DBTrack = e.DBTrack
                    }).SingleOrDefault();

                var returndata = (Object)null;
                var ListQualDetails = new List<QualDetails>();
                var returnCurrentData = (Object)null;
                if (qurey != null)
                {
                    if (qurey.oQualificationDetails != null)
                    {
                        returndata = new
                        {
                            id = qurey.oQualificationDetails.Id,
                            Institute = qurey.oQualificationDetails.Institute == null ? "" : qurey.oQualificationDetails.Institute,
                            SpecilisedBranch = qurey.oQualificationDetails.SpecilisedBranch == null ? "" : qurey.oQualificationDetails.SpecilisedBranch,
                            University = qurey.oQualificationDetails.University == null ? "" : qurey.oQualificationDetails.University,
                            PasingYear = qurey.oQualificationDetails.PasingYear != null ? qurey.oQualificationDetails.PasingYear.Value.ToShortDateString() : null,
                            ResultPercentage = qurey.oQualificationDetails.ResultPercentage == null ? 0 : qurey.oQualificationDetails.ResultPercentage,
                            ResultGradation = qurey.oQualificationDetails.ResultGradation == null ? "" : qurey.oQualificationDetails.ResultGradation,
                            SpecialFeature = qurey.oQualificationDetails.SpecialFeature == null ? "" : qurey.oQualificationDetails.SpecialFeature,
                            ResultSubmissionDate = qurey.oQualificationDetails.ResultSubmissionDate != null ? qurey.oQualificationDetails.ResultSubmissionDate.Value.ToShortDateString() : null,
                            Action = qurey.oQualificationDetails.DBTrack.Action,
                            isauth = true,
                            Add = false
                        };
                        //   var k = qurey.oQualificationDetails.Qualification.ToList();
                        var k = db.QualificationDetails.Include(e => e.Qualification).Where(e => e.Id == id).Select(e => e.Qualification).ToList();
                        foreach (var val in k)
                        {
                            ListQualDetails.Add(new QualDetails
                            {
                                Qual_Id = val.Select(a => a.Id.ToString()).ToArray(),
                                Qual_FullDetails = val.Select(a => a.QualificationDesc.ToString()).ToArray(),
                            });
                        }
                        //curr data
                        var dt_data = db.DT_QualificationDetails.Where(e => e.Orig_Id == qurey.oQualificationDetails.Id && e.DBTrack.IsAuthorized == 0).OrderByDescending(e => e.Id).FirstOrDefault();
                        if (dt_data != null)
                        {
                            returnCurrentData = new
                            {
                                Institute = dt_data.Institute == null ? qurey.oQualificationDetails.Institute == null ? null : qurey.oQualificationDetails.Institute : dt_data.Institute,
                                SpecilisedBranch = dt_data.SpecilisedBranch == null ? qurey.oQualificationDetails.SpecilisedBranch == null ? null : qurey.oQualificationDetails.SpecilisedBranch : dt_data.SpecilisedBranch,
                                University = dt_data.University == null ? qurey.oQualificationDetails.University == null ? null : qurey.oQualificationDetails.University : dt_data.University,
                                PasingYear = dt_data.PasingYear != null ? dt_data.PasingYear.Value.ToShortDateString() : qurey.oQualificationDetails.PasingYear != null ? qurey.oQualificationDetails.PasingYear.Value.ToShortDateString() : null,
                                ResultPercentage = dt_data.ResultPercentage == 0 ? qurey.oQualificationDetails.ResultPercentage == 0 ? 0 : qurey.oQualificationDetails.ResultPercentage : dt_data.ResultPercentage,
                                ResultGradation = dt_data.ResultGradation == null ? qurey.oQualificationDetails.ResultGradation == null ? null : qurey.oQualificationDetails.ResultGradation : dt_data.ResultGradation,
                                SpecialFeature = dt_data.SpecialFeature == null ? qurey.oQualificationDetails.SpecialFeature == null ? null : qurey.oQualificationDetails.SpecialFeature : dt_data.SpecialFeature,
                                ResultSubmissionDate = dt_data.ResultSubmissionDate != null ? dt_data.ResultSubmissionDate.Value.ToShortDateString() : qurey.oQualificationDetails.ResultSubmissionDate != null ? qurey.oQualificationDetails.ResultSubmissionDate.Value.ToShortDateString() : null,
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
        public Object EditSave(QualificationDetails c, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            string Values = form["Qualificationlist"];
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.QualificationDetails.Include(e => e.Qualification).Where(e => e.Id == data).SingleOrDefault();
                        List<Qualification> qualificationlist = new List<Qualification>();
                        if (Values != "" && Values != null)
                        {
                            var ids = Utility.StringIdsToListIds(Values);
                            foreach (var ca in ids)
                            {
                                var Values_val = db.Qualification.Find(ca);
                                qualificationlist.Add(Values_val);
                            }
                            db_data.Qualification = qualificationlist;
                        }
                        else
                        {
                            db_data.Qualification = null;
                        }

                        db.QualificationDetails.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;

                        QualificationDetails getOldData = db.QualificationDetails.Find(data);
                        TempData["CurrRowVersion"] = getOldData.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                Action = "M",
                                ModifiedBy = "0029",
                                ModifiedOn = DateTime.Now
                            };
                            getOldData.Id = data;
                            getOldData.Qualification = db_data.Qualification;
                            getOldData.Institute = c.Institute;
                            getOldData.PasingYear = c.PasingYear;
                            getOldData.ResultGradation = c.ResultGradation;
                            getOldData.ResultPercentage = c.ResultPercentage;
                            getOldData.ResultSubmissionDate = c.ResultSubmissionDate;
                            getOldData.University = c.University;
                            getOldData.SpecialFeature = c.SpecialFeature == null ? "" : c.SpecialFeature;
                            getOldData.SpecilisedBranch = c.SpecilisedBranch == null ? "" : c.SpecilisedBranch;
                            getOldData.DBTrack = c.DBTrack;
                            db.Entry(getOldData).State = System.Data.Entity.EntityState.Modified;

                            QualificationDetails blog = null;
                            DbPropertyValues originalBlogValues = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.QualificationDetails.Where(e => e.Id == data).Include(e => e.Qualification)
                                                  .SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }

                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                            };

                            QualificationDetails qualificationDetails = new QualificationDetails()
                            {
                               
                                Institute = c.Institute,
                                PasingYear = c.PasingYear,
                                ResultGradation = c.ResultGradation,
                                ResultPercentage = c.ResultPercentage,
                                ResultSubmissionDate = c.ResultSubmissionDate,
                                University = c.University,
                                SpecialFeature = c.SpecialFeature == null ? "" : c.SpecialFeature,
                                SpecilisedBranch = c.SpecilisedBranch == null ? "" : c.SpecilisedBranch,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };

                            var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, qualificationDetails, "QualificationDetails", c.DBTrack);
                            DT_QualificationDetails DT_Corp = (DT_QualificationDetails)obj;
                            db.Create(DT_Corp);
                        }
                        db.SaveChanges();
                        ts.Complete();
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

            return new { status = true, responseText = "Record Updated" };
        }
       
        public async Task<ActionResult> AuthSave(FormCollection form) //to save authorised data
        {
            //int auth_id, Boolean isauth, String auth_action
            int auth_id = Convert.ToInt32(form["auth_id"]);
            Boolean isauth = Convert.ToBoolean(form["isauth"]);
            String auth_action = Convert.ToString(form["auth_action"]);
            using (DataBaseContext db = new DataBaseContext())
            {
                if (auth_action == "C")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //Corporate corp = db.Corporate.Find(auth_id);
                        //Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                        QualificationDetails corp = db.QualificationDetails.Include(e => e.Qualification).FirstOrDefault(e => e.Id == auth_id);

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

                        db.QualificationDetails.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //db.SaveChanges();
                        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                        DT_QualificationDetails DT_Corp = (DT_QualificationDetails)rtn_Obj;
                        // DT_Corp.SkillType_Id = corp.SkillType == null ? 0 : corp.SkillType.Id;
                        //DT_Corp.Hobby_Id = corp.Hobby == null ? 0 : corp.Hobby;
                        //DT_Corp.LanguageSkill_Id = corp.LanguageSkill == null ? 0 : corp.LanguageSkill;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();

                        ts.Complete();
                        return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                    }
                }
                else if (auth_action == "M")
                {

                    QualificationDetails Old_Corp = db.QualificationDetails
                            .Include(e => e.Qualification)
                           .Where(e => e.Id == auth_id).SingleOrDefault();


                    DT_QualificationDetails Curr_Corp = db.DT_QualificationDetails
                                                .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                .OrderByDescending(e => e.Id)
                                                .FirstOrDefault();

                    if (Curr_Corp != null)
                    {
                        QualificationDetails corp = new QualificationDetails();

                        //string awrd = Curr_Corp.SkillType_Id == null ? null : Curr_Corp.SkillType_Id.ToString();
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
                                        AuthorizedBy = "0029",
                                        AuthorizedOn = DateTime.Now,
                                        IsModified = false
                                    };

                                    //int a = EditS(awrd, hob, langskl, Qualdtl, skll,  scolr,auth_id, corp, corp.DBTrack);

                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (QualificationDetails)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (QualificationDetails)databaseEntry.ToObject();
                                    corp.RowVersion = databaseValues.RowVersion;
                                }
                            }

                            return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                        return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                }
                else if (auth_action == "D")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //Corporate corp = db.Corporate.Find(auth_id);
                        QualificationDetails corp = db.QualificationDetails.AsNoTracking()
                           .Include(e => e.Qualification)
                            .FirstOrDefault(e => e.Id == auth_id);

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
                            AuthorizedBy = "0029",
                            AuthorizedOn = DateTime.Now
                        };

                        db.QualificationDetails.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                        DT_QualificationDetails DT_Corp = (DT_QualificationDetails)rtn_Obj;
                        // DT_Corp.SkillType_Id = corp.SkillType == null ? 0 : corp.SkillType.Id;
                        //DT_Corp.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                        //DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();
                        db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
                    }

                }
                return View();

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
        public class returnDataClass
        {
            public Int32 EmpId { get; set; }
            public String val { get; set; }
            public Int32 Id2 { get; set; }
            public bool Id3 { get; set; }
            public Int32 LvHead_Id { get; set; }
        }
        public ActionResult GetMyEmpQulification1()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var qurey = db.Employee.Include(e => e.EmpAcademicInfo)
                 .Include(e => e.EmpAcademicInfo.QualificationDetails)
                 .Include(e => e.EmpAcademicInfo.QualificationDetails.Select(a => a.Qualification))
                 .Where(e => e.Id == Emp && e.EmpAcademicInfo != null).SingleOrDefault();

                var ListreturnDataClass = new List<returnDataClass>();
                if (qurey != null && qurey.EmpAcademicInfo != null && qurey.EmpAcademicInfo.QualificationDetails.Count > 0)
                {
                    foreach (var item in qurey.EmpAcademicInfo.QualificationDetails)
                    {
                        var PasingYear = item != null ? item.PasingYear.Value.ToShortDateString() : null;
                        var Institute = item.Institute != null ? item.Institute : null;
                        var ResultGradation = item.ResultGradation != null ? item.ResultGradation : null;
                        var ResultPercentage = item.ResultPercentage != 0 ? item.ResultPercentage : 0;
                        var ResultSubmissionDate = item.ResultSubmissionDate != null ? item.ResultSubmissionDate.Value.ToShortDateString() : null;
                        var SpecialFeature = item.SpecialFeature != null ? item.SpecialFeature : null;
                        var SpecilisedBranch = item.SpecilisedBranch != null ? item.SpecilisedBranch : null;
                        var University = item.University != null ? item.University : null;
                        var Qualification = item.Qualification.Count > 0 ? item.Qualification.Select(e => e.FullDetails).ToArray() : null;
                        var Qualificationstring = Qualification != null ? string.Join(",", Qualification) : null;
                        ListreturnDataClass.Add(new returnDataClass
                        {
                            EmpId = item.Id,
                            val =
                            "PasingYear :" + PasingYear +
                            ", Institute :" + Institute +
                            ", ResultGradation :" + ResultGradation +
                            ", ResultPercentage :" + ResultPercentage +
                            ", ResultSubmissionDate :" + ResultSubmissionDate +
                            ", SpecialFeature :" + SpecialFeature +
                            ", SpecilisedBranch :" + SpecilisedBranch +
                            ", University :" + University +
                            ", Qualification :" + Qualificationstring +
                            "",

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
            public string PasingYear { get; set; }
            public string Institute { get; set; }
            public string ResultGradation { get; set; }
            public string ResultPercentage { get; set; }
            public string ResultSubmissionDate { get; set; }
            public string SpecialFeature { get; set; }
            public string SpecilisedBranch { get; set; }
            public string University { get; set; }
            public string Qualification { get; set; }

            public ChildGetLvNewReqClass RowData { get; set; }
        }


        public ActionResult GetMyEmpQulification()
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
                    .Include(a => a.EmpAcademicInfo)
                      .Include(a => a.EmpAcademicInfo.QualificationDetails)
                        .Include(a => a.EmpAcademicInfo.QualificationDetails.Select(b => b.Qualification))
                     .SingleOrDefault();


                if (db_data.EmpAcademicInfo != null)
                {
                    List<GetLvNewReqClass> returndata = new List<GetLvNewReqClass>();
                    returndata.Add(new GetLvNewReqClass
                    {
                        Institute = "Institute",
                        PasingYear = "Passing Year",
                        ResultGradation = "Result Graduation",
                        ResultPercentage = "Result Percentage",
                        ResultSubmissionDate = "Result SubmissionDate",
                        SpecialFeature = "Special Feature",
                        SpecilisedBranch = "Specialized Branch",
                        University = "University",
                        Qualification = "Qualification"


                    });

                    foreach (var item in db_data.EmpAcademicInfo.QualificationDetails.OrderByDescending(a => a.Id).ToList())
                    {
                        var qual = item.Qualification.Select(a => a.FullDetails).ToList();
                        if (qual != null)
                        {
                            var arralist = qual.ToArray().Distinct();
                            var qualdet = arralist != null ? string.Join(",", arralist) : null;
                            returndata.Add(new GetLvNewReqClass
                            {
                                RowData = new ChildGetLvNewReqClass
                                {
                                    LvNewReq = item.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString(),
                                    LvHead_Id = "",
                                },
                                Institute = item.Institute == null ? "" : item.Institute.ToString(),
                                PasingYear = item.PasingYear != null ? item.PasingYear.Value.ToShortDateString() : null,
                                ResultGradation = item.ResultGradation == null ? "" : item.ResultGradation.ToString(),
                                ResultPercentage = item.ResultPercentage != null ? item.ResultPercentage.ToString() : null,
                                ResultSubmissionDate = item.ResultSubmissionDate != null ? item.ResultSubmissionDate.Value.ToShortDateString() : null,
                                SpecialFeature = item.SpecialFeature == null ? "" : item.SpecialFeature.ToString(),
                                SpecilisedBranch = item.SpecilisedBranch == null ? "" : item.SpecilisedBranch.ToString(),
                                University = item.University == null ? "" : item.University.ToString(),
                                Qualification = qualdet

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

    }
}