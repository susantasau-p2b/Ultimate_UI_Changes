using P2b.Global;
using Payroll;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
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
using P2BUltimate.Security;

namespace P2BUltimate.Controllers
{
    public class QualificationDetailsController : Controller
    {
        //
        // GET: /QualificationDetails/

      //  private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/QualificationDetails/Index.cshtml");
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/Core/_QualificationDetails.cshtml");
        }


        //public ActionResult GetQualificationLKDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.Qualification.ToList();
        //        IEnumerable<Qualification> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.Qualification.ToList().Where(d => d.FullDetails.Contains(data));

        //        }
        //        else
        //        {
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullDetails }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //}
        public ActionResult GetQualificationLKDetails(List<int> SkipIds)
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

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Qualification.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult GetContactDetLKDetails(List<int> SkipIds)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.ContactDetails.Include(e => e.ContactNumbers).ToList();
        //        if (SkipIds != null)
        //        {
        //            foreach (var a in SkipIds)
        //            {
        //                if (fall == null)
        //                    fall = db.ContactDetails.Include(e => e.ContactNumbers).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
        //                else
        //                    fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

        //            }
        //        }
        //        var list1 = db.Corporate.ToList().Select(e => e.ContactDetails);
        //        var list2 = fall.Except(list1);

        //        var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();

        //        return Json(r, JsonRequestBehavior.AllowGet);


        //    }
        //    // return View();
        //}
        //public ActionResult GetQualificationLKDetails(List<int> SkipIds)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.Qualification.ToList();
        //        if (SkipIds != null)
        //        {
        //            foreach (var a in SkipIds)
        //            {
        //                if (fall == null)
        //                    fall = db.Qualification.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
        //                else
        //                    fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
        //            }
        //        }
        //        var list1 = db.QualificationDetails.ToList().Select(e => e.Qualification);
        //        //var list2 = fall.Except(list1);
        //        var r = (from ca in list1 select new { srno = ca.Id ,lookupvalue = ca.QualificationDetails }).Distinct();

        //        return Json(r, JsonRequestBehavior.AllowGet);

        //    }
        //    return View();
        //}

        //public ActionResult GetQualificationLKDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.Qualification.Include(e=>e.);
        //        IEnumerable<Qualification> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.Qualification.ToList().Where(d => d.FullDetails.Contains(data));

        //        }
        //        else
        //        {
        //            var list1 = db.QualificationDetails.ToList().Select(e => e.Qualification);
        //          //  var list2 = fall.Except(list1);

        //            //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
        //            //var result_1 = (from c in fall
        //            //                select new { c.Id, c.CorporateCode, c.CorporateName });
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullDetails }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    // return View();
        //}
        private MultiSelectList GetLookupValues(List<int> selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Qualification> lkval = new List<Qualification>();
                lkval = db.Qualification.ToList();
                return new MultiSelectList(lkval, "Id", "QualificationShortName", selectedValues);
            }
        }

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
                IEnumerable<QualificationDetails> Corporate = null;
                if (gp.IsAutho == true)
                {
                    Corporate = db.QualificationDetails.Include(e => e.Qualification).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Corporate = db.QualificationDetails.Include(e => e.Qualification).AsNoTracking().ToList();
                }

                IEnumerable<QualificationDetails> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Corporate;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.Institute, a.University }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Institute.ToLower() == gp.searchString.ToLower()) || (e.University.ToLower() == gp.searchString.ToLower()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Institute, a.University }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Corporate;
                    Func<QualificationDetails, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Institute" ? c.Institute :
                                         gp.sidx == "University" ? c.University :
                                         gp.sidx == "University" ? c.University : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Institute), Convert.ToString(a.University) }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Institute), Convert.ToString(a.University) }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Institute, a.University }).ToList();
                    }
                    totalRecords = Corporate.Count();
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
        public ActionResult Create(QualificationDetails lkval, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //string Qual = form["Qualificationlist"] == "0" ? "" : form["Qualificationlist"];
                    //if (Qual != null)
                    //{
                    //    List<int> IDs = Qual.Split(',').Select(e => int.Parse(e)).ToList();
                    //    foreach (var k in IDs)
                    //    {
                    //        var value = db.Qualification.Find(k);
                    //        lkval.Qualification = new List<Qualification>();
                    //        lkval.Qualification.Add(value);
                    //    }
                    //}


                    lkval.Qualification = null;
                    List<Qualification> OBJ = new List<Qualification>();
                    string Values = form["Qualificationlist"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.Qualification.Find(ca);
                            OBJ.Add(OBJ_val);
                            lkval.Qualification = OBJ;
                        }
                    }
                    if (Values == null || Values == "")
                    {
                        Msg.Add(" Enter Qualification ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }



                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            DateTime current = DateTime.Now;
                            if (lkval.PasingYear > current)
                            {
                                Msg.Add(" Passing Year Should Be Less Than Current Date  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            if (lkval.ResultSubmissionDate > current)
                            {
                                Msg.Add(" Submission Date Should Be Less Than Current Date  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            if (lkval.ResultPercentage != null)
                            {
                                if (lkval.ResultPercentage > 100)
                                {
                                    Msg.Add(" Enter percentage below 100%  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Enter percentage below 100%", JsonRequestBehavior.AllowGet });
                                }
                            }

                            lkval.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };


                            //lkval.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

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
                                DT_QualificationDetails DT_Corp = (DT_QualificationDetails)a;
                                // DT_Corp.SkillType_Id = lkval.SkillType == null ? 0 : lkval.SkillType.Id;
                                db.Create(DT_Corp);
                                db.SaveChanges();

                                ts.Complete();
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = qualdtl.Id, Val = qualdtl.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { qualdtl.Id, qualdtl.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = lkval.Id });
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


        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
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
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //eturn Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                    // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                }
                            }

                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                    CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                    IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

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
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);



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

        public class QualDetails
        {
            public Array Qual_Id { get; set; }
            public Array Qual_FullDetails { get; set; }

        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var un = db.QualificationDetails.Where(e => e.Id == data).Select
                    (e => new
                    {
                        Institute = e.Institute == null ? "" : e.Institute,
                        SpecilisedBranch = e.SpecilisedBranch == null ? "" : e.SpecilisedBranch,
                        University = e.University == null ? "" : e.University,
                        PasingYear = e.PasingYear,
                        ResultPercentage = e.ResultPercentage == null ? 0 : e.ResultPercentage,
                        ResultGradation = e.ResultGradation == null ? "" : e.ResultGradation,
                        SpecialFeature = e.SpecialFeature == null ? "" : e.SpecialFeature,
                        ResultSubmissionDate = e.ResultSubmissionDate,
                        Action = e.DBTrack.Action
                    }).ToList();


                List<QualDetails> contno = new List<QualDetails>();
                var k = db.QualificationDetails.Include(e => e.Qualification).Where(e => e.Id == data).Select(e => e.Qualification).ToList();
                foreach (var val in k)
                {
                    contno.Add(new QualDetails
                    {
                        Qual_Id = val.Select(e => e.Id.ToString()).ToArray(),
                        Qual_FullDetails = val.Select(e => e.FullDetails.ToString()).ToArray(),

                    });
                }
                // return Json(new object[] { contno }, JsonRequestBehavior.AllowGet);
                //TempData["RowVersion"] = db.IncrActivity.Find(data).RowVersion;

                //return Json(new Object[] { Q, add_data, JsonRequestBehavior.AllowGet });

                var W = db.DT_QualificationDetails
                    //.Include(e => e.IncrPolicy)
                    .Include(e => e.Qualification_Id)
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Institute = e.Institute == null ? "" : e.Institute,
                         SpecilisedBranch = e.SpecilisedBranch == null ? "" : e.SpecilisedBranch,
                         University = e.University == null ? "" : e.University,
                         PasingYear = e.PasingYear,
                         ResultPercentage = e.ResultPercentage == null ? 0 : e.ResultPercentage,
                         ResultGradation = e.ResultGradation == null ? "" : e.ResultGradation,
                         SpecialFeature = e.SpecialFeature == null ? "" : e.SpecialFeature,
                         ResultSubmissionDate = e.ResultSubmissionDate,
                         Qualification_val = e.Qualification_Id
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();



                var Corp = db.QualificationDetails.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { un, contno, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }



        //public ActionResult Edit(int data)
        //{
        //    var r = (from ca in db.QualificationDetails
        //             .Where(e => e.Id == data)
        //             select new
        //             {
        //                 Id = ca.Id,
        //                 SpecilisedBranch = ca.SpecilisedBranch,
        //                 Institute = ca.Institute,
        //                 University = ca.University,
        //                 ResultPercentage = ca.ResultPercentage,
        //                 ResultGradation = ca.ResultGradation,
        //                 PasingYear = ca.PasingYear,
        //                 ResultSubmissionDate = ca.ResultSubmissionDate,
        //                 SpecialFeature = ca.SpecialFeature,


        //             }).ToList();

        //    var a = db.QualificationDetails.Include(e =>e.Qualification).Where(e => e.Id == data).SingleOrDefault();
        //    var b = a.Qualification;

        //    var r1 = (from s in b
        //              select new
        //              {
        //                  Qual_Id = s.Id,
        //                  Qual_FullDetails = s.QualificationDesc
        //              }).ToList();
        //    TempData["RowVersion"] = db.QualificationDetails.Find(data).RowVersion;
        //    return Json(new object[] { r, r1 }, JsonRequestBehavior.AllowGet);

        //}


        public int EditS(int data, QualificationDetails c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CurCorp = db.QualificationDetails.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    QualificationDetails corp = new QualificationDetails()
                    {
                        Id = data,
                        Institute = c.Institute == null ? "" : c.Institute,
                        SpecilisedBranch = c.SpecilisedBranch == null ? "" : c.SpecilisedBranch,
                        University = c.University == null ? "" : c.University,
                        PasingYear = c.PasingYear,
                        ResultPercentage = c.ResultPercentage == null ? 0 : c.ResultPercentage,
                        FullDetails = c.ResultGradation == null ? "" : c.ResultGradation,
                        ResultGradation = c.FullDetails == null ? "" : c.FullDetails,
                        ResultSubmissionDate = c.ResultSubmissionDate,
                        DBTrack = c.DBTrack
                    };


                    db.QualificationDetails.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }



        [HttpPost]
        public async Task<ActionResult> EditSave(QualificationDetails c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var db_data = db.QualificationDetails.Include(e => e.Qualification)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    List<Qualification> lookupval = new List<Qualification>();
                    string Values = form["Qualificationlist"];


                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.Qualification.Find(ca);
                            lookupval.Add(Lookup_val);
                            db_data.Qualification = lookupval;
                        }
                    }
                    else
                    {
                        db_data.Qualification = null;
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
                                    db.QualificationDetails.Attach(db_data);
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_data.RowVersion;
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_Lookup = db.QualificationDetails.Find(data);
                                    TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                    db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        QualificationDetails blog = null; // to retrieve old data
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
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };
                                        QualificationDetails lk = new QualificationDetails
                                        {
                                            Id = data,
                                            Qualification = c.Qualification,
                                            Institute = c.Institute,
                                            PasingYear = c.PasingYear,
                                            ResultGradation = c.ResultGradation,
                                            ResultPercentage = c.ResultPercentage,
                                            ResultSubmissionDate = c.ResultSubmissionDate,
                                            University = c.University,
                                            SpecialFeature = c.SpecialFeature == null ? "" : c.SpecialFeature,
                                            SpecilisedBranch = c.SpecilisedBranch == null ? "" : c.SpecilisedBranch,
                                            DBTrack = c.DBTrack
                                        };


                                        db.QualificationDetails.Attach(lk);
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                        db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

                                        // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


                                        using (var context = new DataBaseContext())
                                        {

                                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                            DT_QualificationDetails DT_Corp = (DT_QualificationDetails)obj;

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
                                var clientValues = (QualificationDetails)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (QualificationDetails)databaseEntry.ToObject();
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

                            QualificationDetails blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            QualificationDetails Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.QualificationDetails.Where(e => e.Id == data).SingleOrDefault();
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
                            QualificationDetails qualificationDetails = new QualificationDetails()
                            {

                                Id = data,
                                Institute = c.Institute,
                                PasingYear = c.PasingYear,
                                ResultGradation = c.ResultGradation,
                                ResultPercentage = c.ResultPercentage,
                                ResultSubmissionDate = c.ResultSubmissionDate,
                                University = c.University,
                                SpecialFeature = c.SpecialFeature == null ? "" : c.SpecialFeature,
                                SpecilisedBranch = c.SpecilisedBranch == null ? "" : c.SpecilisedBranch,
                                DBTrack = c.DBTrack
                            };

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, qualificationDetails, "QualificationDetails", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.QualificationDetails.Where(e => e.Id == data)
                                    .Include(e => e.Qualification).SingleOrDefault();
                                DT_QualificationDetails DT_Corp = (DT_QualificationDetails)obj;
                                //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.QualificationDetails.Attach(blog);
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
        }

        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save Authorized data
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

                            QualificationDetails corp = db.QualificationDetails.Include(e => e.Qualification).FirstOrDefault(e => e.Id == auth_id);

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
                            Msg.Add("  Record Authorized");
                            return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { corp.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
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
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        //int a = EditS(awrd, hob, langskl, Qualdtl, skll,  scolr,auth_id, corp, corp.DBTrack);

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        Msg.Add("  Record Authorized");
                                        return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { corp.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (QualificationDetails)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (QualificationDetails)databaseEntry.ToObject();
                                        corp.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //  return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
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
                                AuthorizedBy = SessionManager.UserName,
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
                            Msg.Add(" Record Authorized ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Record Authorized", JsonRequestBehavior.AllowGet });
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