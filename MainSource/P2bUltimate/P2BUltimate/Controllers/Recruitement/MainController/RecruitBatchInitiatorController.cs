using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Recruitment;
using System.Web.Mvc;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System.Transactions;
using P2b.Global;
using P2BUltimate.Security;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;

namespace P2BUltimate.Controllers.Recruitment.MainController
{
    [AuthoriseManger]
    public class RecruitBatchInitiatorController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();
        //
        // GET: /RecruitmentBatchInstitute/
        public ActionResult Index()
        {
            return View("~/Views/Recruitement/MainView/RecruitBatchInitiator/Index.cshtml");
            //~/Views/Recruitement/MainView/RecruitBatchInitiator/Index.cshtml
        }
        public ActionResult PostDetailPartial()
        {
            return View("~/Views/Shared/Recruitment/_PostDetails.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(RecruitBatchInitiator c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string Geostructlist = form["Geostructlist"] == "0" ? "" : form["Geostructlist"];
                string PostDetails = form["PostDetailslist"] == "0" ? "" : form["PostDetailslist"];
                string ResumeCollection = form["ResumeCollectionlist"] == "0" ? "" : form["ResumeCollectionlist"];
                string Jobinsideordlist = form["JobInsideOrglist"] == "0" ? "" : form["JobInsideOrglist"];
                string Jobportallist = form["JobPortallist"] == "0" ? "" : form["JobPortallist"];
                string jobnewspapaerlist = form["JobNewsPaperlist"] == "0" ? "" : form["JobNewsPaperlist"];
                string jobagencylist = form["JobAgencylist"] == "0" ? "" : form["JobAgencylist"];

                List<String> Msg = new List<String>();
                
                if (PostDetails == null)
                {
                    Msg.Add("Please Select Post Details");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                string RecruitEvaluationProcess = form["RecruitEvaluationProcessDetailslist"] == "0" ? "" : form["RecruitEvaluationProcessDetailslist"];
                try
                {
                    //if (Geostructlist != null)
                    //{
                    //    if (Geostructlist != "")
                    //    {
                    //        int PostId = Convert.ToInt32(Geostructlist);
                    //        var val = db.GeoStruct//.Include(e => e.Area)
                    //            //.Include(e => e.City)
                    //            //.Include(e => e.Country)
                    //            //.Include(e => e.District)
                    //            //.Include(e => e.State)
                    //            //.Include(e => e.StateRegion)
                    //            //.Include(e => e.Taluka)
                    //                            .Where(e => e.Id == PostId).SingleOrDefault();
                    //        c.Geostruct = val;
                    //    }
                    //}

                    if (PostDetails != null)
                    {
                        if (PostDetails != "")
                        {
                            int PostId = Convert.ToInt32(PostDetails);
                            var val = db.PostDetails//.Include(e => e.Area)
                                //.Include(e => e.City)
                                //.Include(e => e.Country)
                                //.Include(e => e.District)
                                //.Include(e => e.State)
                                //.Include(e => e.StateRegion)
                                //.Include(e => e.Taluka)
                                                .Where(e => e.Id == PostId).SingleOrDefault();
                            c.PostDetails = val;
                        }
                    }



                    c.JobAgency = null;
                    List<JobAgency> OBJ = new List<JobAgency>();

                    if (jobagencylist != null)
                    {
                        var ids = Utility.StringIdsToListIds(jobagencylist);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.JobAgency.Find(ca);
                            OBJ.Add(OBJ_val);
                            c.JobAgency = OBJ;
                        }
                    }
                    c.JobInsideOrg = null;
                    List<JobInsideOrg> JobInsideOrg = new List<JobInsideOrg>();

                    if (Jobinsideordlist != null)
                    {
                        var ids = Utility.StringIdsToListIds(Jobinsideordlist);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.JobInsideOrg.Find(ca);
                            JobInsideOrg.Add(OBJ_val);
                            c.JobInsideOrg = JobInsideOrg;
                        }
                    }
                    c.JobNewsPaper = null;
                    List<JobNewsPaper> JobNewsPaper = new List<JobNewsPaper>();
                    if (jobnewspapaerlist != null)
                    {
                        var ids = Utility.StringIdsToListIds(jobnewspapaerlist);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.JobNewsPaper.Find(ca);
                            JobNewsPaper.Add(OBJ_val);
                            c.JobNewsPaper = JobNewsPaper;
                        }
                    }
                    c.JobPortal = null;
                    List<JobPortal> JobPortal = new List<JobPortal>();
                    if (Jobportallist != null)
                    {
                        var ids = Utility.StringIdsToListIds(Jobportallist);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.JobPortal.Find(ca);
                            JobPortal.Add(OBJ_val);
                            c.JobPortal = JobPortal;
                        }
                    }

                    c.RecruitEvaluationProcess = null;
                    List<RecruitEvaluationProcess> OBJ1 = new List<RecruitEvaluationProcess>();
                    string Values1 = form["RecruitEvaluationProcessDetailslist"];

                    if (Values1 != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values1);
                        foreach (var ca in ids)
                        {
                            var OBJ_val1 = db.RecruitEvaluationProcess.Find(ca);
                            OBJ1.Add(OBJ_val1);
                            c.RecruitEvaluationProcess = OBJ1;
                        }
                    }

                    c.ResumeCollection = null;
                    List<ResumeCollection> OBJ2 = new List<ResumeCollection>();


                    if (ResumeCollection != null)
                    {
                        var ids = Utility.StringIdsToListIds(ResumeCollection);
                        foreach (var ca in ids)
                        {
                            var OBJ_val1 = db.ResumeCollection.Find(ca);
                            OBJ2.Add(OBJ_val1);
                            c.ResumeCollection = OBJ2;
                        }
                    }

                    string CandidateDocumentslist = form["CandidateDocumentslist"] == "0" ? "" : form["CandidateDocumentslist"];
                    List<CandidateDocuments> ObjEmployeeDocuments = new List<CandidateDocuments>();
                    if (CandidateDocumentslist != null && CandidateDocumentslist != "")
                    {
                        var ids = Utility.StringIdsToListIds(CandidateDocumentslist);
                        foreach (var ca in ids)
                        {
                            var value = db.CandidateDocuments.Find(ca);
                            ObjEmployeeDocuments.Add(value);
                            c.CandidateDocuments = ObjEmployeeDocuments;
                        }

                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            //if (db.RecruitBatchInitiator.Any(o => o.Code == c.Code))
                            //{
                            //    Msg.Add("Code Already Exists.");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            RecruitBatchInitiator recruitmentintiator = new RecruitBatchInitiator()
                            {

                                BatchName = c.BatchName,
                                JobReferenceNo = c.JobReferenceNo == null ? "" : c.JobReferenceNo,
                                initiatedDate = c.initiatedDate,
                                PublishDate = c.PublishDate,
                                BatchCloseDate = c.BatchCloseDate,
                                Narration = c.Narration,
                                //Geostruct = c.Geostruct,
                                PostDetails = c.PostDetails,
                                //JobSource = c.JobSource,
                                JobAgency = c.JobAgency,
                                JobInsideOrg = c.JobInsideOrg,
                                JobNewsPaper = c.JobNewsPaper,
                                JobPortal = c.JobPortal,
                                RecruitEvaluationProcess = c.RecruitEvaluationProcess,
                                CandidateDocuments = c.CandidateDocuments,

                                DBTrack = c.DBTrack
                            };

                            db.RecruitBatchInitiator.Add(recruitmentintiator);
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, c.DBTrack);
                            //DT_RecruitBatchInitiator DT_Corp = (DT_RecruitBatchInitiator)rtn_Obj;
                            //DT_Corp.PostDetails_Id = c.PostDetails == null ? 0 : c.PostDetails.Id;
                            //DT_Corp.Geostruct_Id = c.Geostruct == null ? 0 : c.Geostruct.Id;
                            //DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                            //db.Create(DT_Corp);
                            db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


                            ts.Complete();
                            Msg.Add("Data Saved Successfully.");
                            return Json(new Utility.JsonReturnClass { Id = recruitmentintiator.Id, Val = recruitmentintiator.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                        Msg.Add("Code Already Exists.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        //return this.Json(new { msg = errorMsg });
                    }
                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //public ActionResult Editcontactdetails_partial(int data)
        //{
        //    var r = (from ca in db.ContactDetails
        //             .Where(e => e.Id == data)
        //             select new
        //             {
        //                 Id = ca.Id,
        //                 EmailId = ca.EmailId,
        //                 FaxNo = ca.FaxNo,
        //                 Website = ca.Website
        //             }).ToList();

        //    var a = db.ContactDetails.Include(e => e.ContactNumbers).Where(e => e.Id == data).SingleOrDefault();
        //    var b = a.ContactNumbers;

        //    var r1 = (from s in b
        //              select new
        //              {
        //                  Id = s.Id,
        //                  FullContactNumbers = s.FullContactNumbers
        //              }).ToList();

        //    TempData["RowVersion"] = db.ContactDetails.Find(data).RowVersion;
        //    return Json(new object[] { r, r1 }, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult Createcontactdetails_partial()
        //{
        //    return View("~/Views/Shared/Core/_Contactdetails.cshtml");
        //}

        public class RecruitJobSource
        {
            public Array SA_id { get; set; }
            public Array SA_val { get; set; }
        }
        public class ResumeCollections
        {
            public Array RC_id { get; set; }
            public Array RC_val { get; set; }
        }
        public class RRecruitEvaluationProcess
        {
            public Array SA_val { get; set; }
            public Array SA_id { get; set; }
            public Array JOBINSIDEORG_ID { get; set; }
            public Array JOBINSIDEORG_VAL { get; set; }
            public Array JOBPORTAL_val { get; set; }
            public Array JOBPORTAL_ID { get; set; }
            public Array JOBNEWPAPER_ID { get; set; }
            public Array JOBNEWSPAPER_VAL { get; set; }
            public Array JOBAGENCY_ID { get; set; }
            public Array JOBAGENCY_VAL { get; set; }
        }

        public ActionResult GetLookup(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.JobSource.Include(e => e.JobAgency).Include(e => e.JobInsideOrg).Include(e => e.JobNewsPaper)
                                   .Include(e => e.JobPortal)
                                     .ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.JobSource.Include(e => e.JobAgency).Include(e => e.JobInsideOrg).Include(e => e.JobNewsPaper)
                                   .Include(e => e.JobPortal).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
            // return View();
        }

        public ActionResult GetLookupRecruitEvalProcess(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.RecruitEvaluationProcess.Include(e => e.RecruitEvaluationPara).Include(e => e.RecruitJoiningPara).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.RecruitEvaluationProcess.Include(e => e.RecruitEvaluationPara).Include(e => e.RecruitJoiningPara).Include(e => e.Name)
                                   .Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
            // return View();
        }

        public class returnDataClass
        {
            public Array Can_Id { get; set; }
            public Array Can_Full { get; set; }

        }
        public class salhddetails
        {
            public int SA_id { get; set; }
            public string SA_val { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            List<RecruitJobSource> return_data = new List<RecruitJobSource>();

            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.RecruitBatchInitiator
                    //.Include(e => e.initiatedDate).Include(e => e.Name).Include(e => e.Narration).Include(e => e.PublishDate)
                    //.Include(e => e.ReferenceNo).Include(e => e.TrClosed).Include(e => e.TrReject)
                    .Where(e => e.Id == data).ToList();

                foreach (var ca in a)
                {
                    return_data.Add(
                new RecruitJobSource
                {
                    //SA_id = ca.JobSource.Select(e => e.Id).ToArray(),
                    // SA_val = ca.JobSource.Select(e => e.JobPortal.Select(q => q.FullDetails)).ToArray()
                });
                }


                //var gs = db.RecruitBatchInitiator
                //    // .Include(e => e.Geostruct.Company)
                //    // .Include(e => e.Geostruct.Corporate)
                //    // .Include(e => e.Geostruct.Department)
                //    // .Include(e => e.Geostruct.Division)
                //    .Where(e => e.Id == data)
                //    .Select(e => new
                //    {
                //        //SA_id = e.Geostruct.Id == null ? "" : e.Geostruct.Id.ToString(),
                //        // SA_val = e.Geostruct.FullDetails == null ? "" : e.Geostruct.FullDetails.ToString()
                //    })
                //    .ToList();


                List<ResumeCollections> return_data1 = new List<ResumeCollections>();

                var rc = db.RecruitBatchInitiator.Include(e => e.ResumeCollection)
                    .Include(e => e.ResumeCollection.Select(t => t.CandidateDocuments))
                    .Include(e => e.ResumeCollection.Select(t => t.RecruitEvaluationProcessResult))
                    .Include(e => e.ResumeCollection.Select(t => t.ResumeSortlistingStatus))
                    .Include(e => e.ResumeCollection.Select(t => t.ShortlistingCriteria))
                    .Include(e => e.ResumeCollection.Select(t => t.ModeOfResume))
                    .Include(e => e.ResumeCollection.Select(t => t.EmpCTCStruct))
                    .Include(e => e.ResumeCollection.Select(t => t.Candidate))
                    .Where(e => e.Id == data).ToList();
                foreach (var ca in rc)
                {
                    return_data1.Add(
                new ResumeCollections
                {
                    RC_id = ca.ResumeCollection.Select(e => e.Id).ToArray(),
                    RC_val = ca.ResumeCollection.Select(q => q.FullDetails).ToArray()
                });
                }

                List<RRecruitEvaluationProcess> return_data2 = new List<RRecruitEvaluationProcess>();
                List<salhddetails> objlist = new List<salhddetails>();
                string[] para = { };
                var a1 = db.RecruitBatchInitiator
                    .Include(e => e.RecruitEvaluationProcess)
                    .Include(e => e.JobInsideOrg)
                    .Include(e => e.JobInsideOrg.Select(t => t.ContactPerson))
                    .Include(e => e.JobNewsPaper)
                    .Include(e => e.JobNewsPaper.Select(t => t.ContactPerson))
                    .Include(e => e.JobPortal)
                    .Include(e => e.JobPortal.Select(t => t.ContactPerson))
                    .Include(e => e.JobAgency)
                    .Include(e => e.JobAgency.Select(t => t.ContactPerson))
                    .Where(e => e.Id == data).SingleOrDefault();

                if (a1 != null)
                {
                    foreach (var ca in a1.RecruitEvaluationProcess)
                    {
                        objlist.Add(new salhddetails
                        {

                            SA_id = ca.Id,
                            SA_val = ca.FullDetails


                        });

                    }

                }
                if (a1 != null)
                {

                    return_data2.Add(
                new RRecruitEvaluationProcess
                {
                    SA_id = a1.RecruitEvaluationProcess == null ? para : a1.RecruitEvaluationProcess.Select(e => e.Id.ToString()).ToArray(),
                    SA_val = a1.RecruitEvaluationProcess == null ? para : a1.RecruitEvaluationProcess.Select(e => e.FullDetails).ToArray(),
                    JOBAGENCY_ID = a1.JobAgency == null ? para : a1.JobAgency.Select(e => e.Id.ToString()).ToArray(),
                    JOBAGENCY_VAL = a1.JobAgency == null ? para : a1.JobAgency.Select(e => e.FullDetails).ToArray(),
                    JOBINSIDEORG_ID = a1.JobInsideOrg == null ? para : a1.JobInsideOrg.Select(e => e.Id.ToString()).ToArray(),
                    JOBINSIDEORG_VAL = a1.JobInsideOrg == null ? para : a1.JobInsideOrg.Select(e => e.FullDetails).ToArray(),
                    JOBNEWPAPER_ID = a1.JobNewsPaper == null ? para : a1.JobNewsPaper.Select(e => e.Id.ToString()).ToArray(),
                    JOBNEWSPAPER_VAL = a1.JobNewsPaper == null ? para : a1.JobNewsPaper.Select(e => e.FullDetails).ToArray(),
                    JOBPORTAL_ID = a1.JobPortal == null ? para : a1.JobPortal.Select(e => e.Id.ToString()).ToArray(),
                    JOBPORTAL_val = a1.JobPortal == null ? para : a1.JobPortal.Select(e => e.FullDetails).ToArray(),

                });
                }

                var Q = db.RecruitBatchInitiator
                    .Where(e => e.Id == data).Select
                    (e => new
                    {

                        Batchname = e.BatchName,
                        ReferenceNo = e.JobReferenceNo,
                        Narration = e.Narration,
                        initiatedDate = e.initiatedDate,
                        PublishDate = e.PublishDate,
                        BatchCloseDate = e.BatchCloseDate,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.RecruitBatchInitiator
                  .Include(e => e.PostDetails).Include(e => e.PostDetails.RangeFilter).Include(e => e.PostDetails.ExpFilter)
                  .Include(e => e.CandidateDocuments)
                .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        PostDetails_Full = e.PostDetails.FullDetails == null ? "" : e.PostDetails.FullDetails,
                        Add_Id = e.PostDetails.Id == null ? "" : e.PostDetails.Id.ToString(),
                        //Cont_Id = e.Geostruct.Id == null ? "" : e.Geostruct.Id.ToString(),
                        // FullGeostructDetails = e.Geostruct.Location == null ? "" : e.Geostruct.Location.FullDetails



                    }).ToList();

                var add_data3 = db.RecruitBatchInitiator
                   .Where(e => e.Id == data)
                   .Include(e => e.CandidateDocuments)
                 .ToList();

                var ListreturnDataClass = new List<returnDataClass>();
                foreach (var item in add_data3)
                {
                    ListreturnDataClass.Add(new returnDataClass
                    {
                        Can_Id = item.CandidateDocuments.Select(e => e.Id.ToString()).ToArray(),
                        Can_Full = item.CandidateDocuments.Select(e => e.FullDetails).ToArray(),

                    });
                }


                var W = db.DT_RecruitBatchInitiator
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,

                         BatchName = e.BatchName == null ? "" : e.BatchName,
                         ReferenceNo = e.JobReferenceNo == null ? "" : e.JobReferenceNo,
                         //BusinessType_Val = e.BusinessType_Id == 0 ? "" : db.LookupValue
                         //           .Where(x => x.Id == e.BusinessType_Id)
                         //           .Select(x => x.LookupVal).FirstOrDefault(),

                         Address_Val = e.PostDetails_Id == 0 ? "" : db.PostDetails.Where(x => x.Id == e.PostDetails_Id).Select(x => x.FullDetails).FirstOrDefault(),
                         // Contact_Val = e.Geostruct_Id == 0 ? "" : db.GeoStruct.Where(x => x.Id == e.Geostruct_Id).Select(x => x.FullDetails).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.RecruitBatchInitiator.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, return_data, objlist, return_data2, W, Auth, return_data1, ListreturnDataClass, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(RecruitBatchInitiator c, int data, FormCollection form) // Edit submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Geostructlist = form["Geostructlist"] == "0" ? "" : form["Geostructlist"];
                    string PostDetails = form["PostDetailslist"] == "0" ? "" : form["PostDetailslist"];
                    string CandidateDocumentslist = form["CandidateDocumentslist"] == "0" ? "" : form["CandidateDocumentslist"];
                    string Jobinsideordlist = form["JobInsideOrglist"] == "0" ? "" : form["JobInsideOrglist"];
                    string Jobportallist = form["JobPortallist"] == "0" ? "" : form["JobPortallist"];
                    string jobnewspapaerlist = form["JobNewsPaperlist"] == "0" ? "" : form["JobNewsPaperlist"];
                    string jobagencylist = form["JobAgencylist"] == "0" ? "" : form["JobAgencylist"];

                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;



                    //if (Geostructlist != null)
                    //{
                    //    if (Geostructlist != "")
                    //    {
                    //        int AddId = Convert.ToInt32(Geostructlist);
                    //        var val = db.GeoStruct.Include(e => e.Company)
                    //                            .Include(e => e.Corporate)
                    //                            .Include(e => e.Department)
                    //                            .Include(e => e.Division)
                    //                            .Include(e => e.Group)
                    //                            .Include(e => e.Location)
                    //                            .Include(e => e.Region)
                    //                            .Where(e => e.Id == AddId).SingleOrDefault();
                    //        c.Geostruct = val;
                    //    }
                    // }

                    if (PostDetails != null)
                    {
                        if (PostDetails != "")
                        {
                            int ContId = Convert.ToInt32(PostDetails);
                            var val = db.PostDetails
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            c.PostDetails = val;
                        }
                    }


                    string Values = form["JobSourceDetailslist"];

                    //var db_data = db.RecruitBatchInitiator.Include(e => e.JobSource).Where(e => e.Id == data).SingleOrDefault();
                    //List<JobSource> SOBJ = new List<JobSource>();
                    //db_data.JobSource = null;
                    //if (Values != null)
                    //{
                    //    var ids = Utility.StringIdsToListIds(Values);
                    //    foreach (var ca in ids)
                    //    {
                    //        var Lookup_val = db.JobSource.Find(ca);
                    //        SOBJ.Add(Lookup_val);
                    //        db_data.JobSource = SOBJ;
                    //    }
                    //}


                    string Values1 = form["RecruitEvaluationProcessDetailslist"];



                    var db_data1 = db.RecruitBatchInitiator
                        .Include(e => e.RecruitEvaluationProcess)
                        .Include(e => e.JobAgency)
                        .Include(e => e.JobPortal)
                        .Include(e => e.JobNewsPaper)
                        .Include(e => e.JobInsideOrg)
                        .Where(e => e.Id == data).SingleOrDefault();
                    List<RecruitEvaluationProcess> ObjQualification = new List<RecruitEvaluationProcess>();
                    RecruitBatchInitiator pd1 = null;
                    pd1 = db.RecruitBatchInitiator.Include(e => e.RecruitEvaluationProcess).Where(e => e.Id == data).SingleOrDefault();
                    if (Values1 != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values1);
                        foreach (var ca in ids)
                        {
                            var value = db.RecruitEvaluationProcess.Find(ca);
                            ObjQualification.Add(value);
                            pd1.RecruitEvaluationProcess = ObjQualification;
                            db_data1.RecruitEvaluationProcess = ObjQualification;
                        }
                    }
                    else
                    {
                        pd1.RecruitEvaluationProcess = null;
                    }
                    //List<RecruitEvaluationProcess> SOBJ1 = new List<RecruitEvaluationProcess>();
                    //db_data1.RecruitEvaluationProcess = null;
                    //if (Values1 != null)
                    //{
                    //    var ids = Utility.StringIdsToListIds(Values1);
                    //    foreach (var ca in ids)
                    //    {
                    //        var Lookup_val = db.RecruitEvaluationProcess.Find(ca);
                    //        SOBJ1.Add(Lookup_val);
                    //        db_data1.RecruitEvaluationProcess = SOBJ1;
                    //    }
                    //}

                    //  var db_data = db.RecruitBatchInitiator.Include(e => e.JobInsideOrg).Where(e => e.Id == data).SingleOrDefault();
                    List<JobInsideOrg> SOBJ = new List<JobInsideOrg>();
                    db_data1.JobInsideOrg = null;
                    if (Jobinsideordlist != null)
                    {
                        var ids = Utility.StringIdsToListIds(Jobinsideordlist);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.JobInsideOrg.Find(ca);
                            SOBJ.Add(Lookup_val);
                            db_data1.JobInsideOrg = SOBJ;
                        }
                    }

                    // var jobportaldata = db.RecruitBatchInitiator.Include(e => e.JobPortal).Where(e => e.Id == data).SingleOrDefault();
                    List<JobPortal> jobportalOBJ = new List<JobPortal>();
                    db_data1.JobPortal = null;
                    if (Jobportallist != null)
                    {
                        var ids = Utility.StringIdsToListIds(Jobportallist);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.JobPortal.Find(ca);
                            jobportalOBJ.Add(Lookup_val);
                            db_data1.JobPortal = jobportalOBJ;
                        }
                    }

                    //var Jobnewpaperdata = db.RecruitBatchInitiator.Include(e => e.JobNewsPaper).Where(e => e.Id == data).SingleOrDefault();
                    List<JobNewsPaper> JobNewsPaperOBJ = new List<JobNewsPaper>();
                    db_data1.JobNewsPaper = null;
                    if (jobnewspapaerlist != null)
                    {
                        var ids = Utility.StringIdsToListIds(jobnewspapaerlist);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.JobNewsPaper.Find(ca);
                            JobNewsPaperOBJ.Add(Lookup_val);
                            db_data1.JobNewsPaper = JobNewsPaperOBJ;
                        }
                    }

                    // var jobagencydata = db.RecruitBatchInitiator.Include(e => e.JobAgency).Where(e => e.Id == data).SingleOrDefault();
                    List<JobAgency> JobAgencySOBJ = new List<JobAgency>();
                    db_data1.JobAgency = null;
                    if (jobagencylist != null)
                    {
                        var ids = Utility.StringIdsToListIds(jobagencylist);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.JobAgency.Find(ca);
                            JobAgencySOBJ.Add(Lookup_val);
                            db_data1.JobAgency = JobAgencySOBJ;
                        }
                    }

                    string Values2 = form["ResumeCollectionlist"] == "0" ? "" : form["ResumeCollectionlist"];
                    //var db_data2 = db.RecruitBatchInitiator.Include(e => e.ResumeCollection).Where(e => e.Id == data).SingleOrDefault();
                    //List<ResumeCollection> SOBJ2 = new List<ResumeCollection>();
                    //db_data.ResumeCollection = null;
                    //if (Values2 != null)
                    //{
                    //    var ids = Utility.StringIdsToListIds(Values2);
                    //    foreach (var ca in ids)
                    //    {
                    //        var Lookup_val = db.ResumeCollection.Find(ca);
                    //        SOBJ2.Add(Lookup_val);
                    //        db_data.ResumeCollection = SOBJ2;
                    //    }
                    // }

                    //if (ContactDetails != null)
                    //{
                    //    if (ContactDetails != "")
                    //    {
                    //        int ContId = Convert.ToInt32(ContactDetails);
                    //        var val = db.ContactDetails.Include(e => e.ContactNumbers)
                    //                            .Where(e => e.Id == ContId).SingleOrDefault();
                    //        c.ContactDetails = val;
                    //    }
                    //}





                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {


                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                RecruitBatchInitiator blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.RecruitBatchInitiator.Where(e => e.Id == data)
                                                            .Include(e => e.PostDetails).SingleOrDefault();

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

                                //int a = EditS(Geostructlist, PostDetails, data, c, c.DBTrack);

                                //if (Geostructlist != null)
                                //{
                                //    if (Geostructlist != "")
                                //    {
                                //        var val = db.GeoStruct.Find(int.Parse(Geostructlist));
                                //        c.Geostruct = val;

                                //        var add = db.RecruitBatchInitiator.Include(e => e.Geostruct).Where(e => e.Id == data).SingleOrDefault();
                                //        IList<RecruitBatchInitiator> addressdetails = null;
                                //        if (add.Geostruct != null)
                                //        {
                                //            addressdetails = db.RecruitBatchInitiator.Where(x => x.Geostruct.Id == add.Geostruct.Id && x.Id == data).ToList();
                                //        }
                                //        else
                                //        {
                                //            addressdetails = db.RecruitBatchInitiator.Where(x => x.Id == data).ToList();
                                //        }
                                //        if (addressdetails != null)
                                //        {
                                //            foreach (var s in addressdetails)
                                //            {
                                //                s.Geostruct = c.Geostruct;
                                //                db.RecruitBatchInitiator.Attach(s);
                                //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //                // await db.SaveChangesAsync(false);
                                //                db.SaveChanges();
                                //                TempData["RowVersion"] = s.RowVersion;
                                //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                //            }
                                //        }
                                //    }
                                //}
                                //else
                                //{
                                //    var addressdetails = db.RecruitBatchInitiator.Include(e => e.Geostruct).Where(x => x.Id == data).ToList();
                                //    foreach (var s in addressdetails)
                                //    {
                                //        s.Geostruct = null;
                                //        db.RecruitBatchInitiator.Attach(s);
                                //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //        //await db.SaveChangesAsync();
                                //        db.SaveChanges();
                                //        TempData["RowVersion"] = s.RowVersion;
                                //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                //    }
                                //}

                                if (PostDetails != null)
                                {
                                    if (PostDetails != "")
                                    {
                                        var val = db.PostDetails.Find(int.Parse(PostDetails));
                                        c.PostDetails = val;

                                        var add = db.RecruitBatchInitiator.Include(e => e.PostDetails).Where(e => e.Id == data).SingleOrDefault();
                                        IList<RecruitBatchInitiator> contactsdetails = null;
                                        if (add.PostDetails != null)
                                        {
                                            contactsdetails = db.RecruitBatchInitiator.Where(x => x.PostDetails.Id == add.PostDetails.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            contactsdetails = db.RecruitBatchInitiator.Where(x => x.Id == data).ToList();
                                        }
                                        foreach (var s in contactsdetails)
                                        {
                                            s.PostDetails = c.PostDetails;
                                            db.RecruitBatchInitiator.Attach(s);
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
                                    var contactsdetails = db.RecruitBatchInitiator.Include(e => e.PostDetails).Where(x => x.Id == data).ToList();
                                    foreach (var s in contactsdetails)
                                    {
                                        s.PostDetails = null;
                                        db.RecruitBatchInitiator.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }

                                List<CandidateDocuments> ObjCandidateDocuments = new List<CandidateDocuments>();

                                if (CandidateDocumentslist != null && CandidateDocumentslist != "")
                                {
                                    var ids = Utility.StringIdsToListIds(CandidateDocumentslist);

                                    foreach (var ca in ids)
                                    {
                                        var ITSeclist = db.CandidateDocuments.Find(ca);
                                        ObjCandidateDocuments.Add(ITSeclist);
                                        c.CandidateDocuments = ObjCandidateDocuments;

                                    }
                                }



                                var CurCorp = db.RecruitBatchInitiator.Find(data);
                                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    //c.DBTrack = dbT;
                                    RecruitBatchInitiator corp = new RecruitBatchInitiator()
                                    {

                                        BatchName = c.BatchName,
                                        JobReferenceNo = c.JobReferenceNo,
                                        Narration = c.Narration,
                                        initiatedDate = c.initiatedDate,
                                        PublishDate = c.PublishDate,
                                        BatchCloseDate = c.BatchCloseDate,
                                        CandidateDocuments = c.CandidateDocuments,
                                        JobInsideOrg = db_data1.JobInsideOrg,
                                        JobAgency = db_data1.JobAgency,
                                        JobNewsPaper = db_data1.JobNewsPaper,
                                        JobPortal = db_data1.JobPortal,
                                        RecruitEvaluationProcess = db_data1.RecruitEvaluationProcess,
                                        Id = data,
                                        DBTrack = c.DBTrack
                                    };


                                    db.RecruitBatchInitiator.Attach(corp);
                                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                }



                                using (var context = new DataBaseContext())
                                {

                                    var obj = DBTrackFile.DBTrackSave("Recruitment/Recruitment", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                    db.SaveChanges();
                                }
                                await db.SaveChangesAsync();
                                // var aaq = db.RecruitBatchInitiator.Include(e => e.JobSource).Where(e => e.Id == data).SingleOrDefault();
                                ts.Complete();
                                Msg.Add("Record Updated Successfully.");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            RecruitBatchInitiator blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            RecruitBatchInitiator Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.RecruitBatchInitiator.Where(e => e.Id == data).SingleOrDefault();
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

                            RecruitBatchInitiator corp = new RecruitBatchInitiator()
                            {

                                BatchName = c.BatchName,
                                JobReferenceNo = c.JobReferenceNo,
                                Narration = c.Narration,
                                initiatedDate = c.initiatedDate,
                                PublishDate = c.PublishDate,
                                BatchCloseDate = c.BatchCloseDate,

                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Recruitement/Recruitement", "M", blog, corp, "RecruitBatchInitiator", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                //Old_Corp = context.RecruitBatchInitiator.Where(e => e.Id == data).Include(e => e.PostDetails)
                                //    .Include(e => e.Geostruct).SingleOrDefault();
                                //DT_RecruitBatchInitiator DT_Corp = (DT_RecruitBatchInitiator)obj;
                                //DT_Corp.PostDetails_Id = DBTrackFile.ValCompare(Old_Corp.PostDetails, c.PostDetails);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.Geostruct_Id = DBTrackFile.ValCompare(Old_Corp.Geostruct, c.Geostruct); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //// DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                //db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.RecruitBatchInitiator.Attach(blog);
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
                    var clientValues = (RecruitBatchInitiator)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (RecruitBatchInitiator)databaseEntry.ToObject();
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

        // [HttpPost]
        //public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<string> Msg = new List<string>();
        //        try
        //        {
        //            if (auth_action == "C")
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    //Corporate corp = db.Corporate.Find(auth_id);
        //                    //Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

        //                    RecruitBatchInitiator corp = db.RecruitBatchInitiator.Include(e => e.PostDetails)
        //                        .Include(e => e.Geostruct)
        //                        .FirstOrDefault(e => e.Id == auth_id);

        //                    corp.DBTrack = new DBTrack
        //                    {
        //                        Action = "C",
        //                        ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
        //                        CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
        //                        CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
        //                        IsModified = corp.DBTrack.IsModified == true ? false : false,
        //                        AuthorizedBy = SessionManager.UserName,
        //                        AuthorizedOn = DateTime.Now
        //                    };

        //                    db.RecruitBatchInitiator.Attach(corp);
        //                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    //db.SaveChanges();
        //                    var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, corp.DBTrack);
        //                    DT_RecruitBatchInitiator DT_Corp = (DT_RecruitBatchInitiator)rtn_Obj;
        //                    DT_Corp.PostDetails_Id = corp.PostDetails == null ? 0 : corp.PostDetails.Id;
        //                    DT_Corp.Geostruct_Id = corp.Geostruct == null ? 0 : corp.Geostruct.Id;
        //                    //DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
        //                    db.Create(DT_Corp);
        //                    await db.SaveChangesAsync();

        //                    ts.Complete();
        //                    Msg.Add("  Record Authorised");
        //                    return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //            else if (auth_action == "M")
        //            {

        //                RecruitBatchInitiator Old_Corp = db.RecruitBatchInitiator.Include(e => e.PostDetails)
        //                                                  .Include(e => e.Geostruct)
        //                                                  .Where(e => e.Id == auth_id).SingleOrDefault();

        //                //var W = db.DT_Corporate
        //                //.Include(e => e.ContactDetails)
        //                //.Include(e => e.Address)
        //                //.Include(e => e.BusinessType)
        //                //.Include(e => e.ContactDetails)
        //                //.Where(e => e.Orig_Id == auth_id && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
        //                //(e => new
        //                //{
        //                //    DT_Id = e.Id,
        //                //    Code = e.Code == null ? "" : e.Code,
        //                //    Name = e.Name == null ? "" : e.Name,
        //                //    BusinessType_Val = e.BusinessType.Id == null ? "" : e.BusinessType.LookupVal,
        //                //    Address_Val = e.Address.Id == null ? "" : e.Address.FullAddress,
        //                //    Contact_Val = e.ContactDetails.Id == null ? "" : e.ContactDetails.FullContactDetails,
        //                //}).OrderByDescending(e => e.DT_Id).FirstOrDefault();

        //                DT_RecruitBatchInitiator Curr_Corp = db.DT_RecruitBatchInitiator
        //                                            .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
        //                                            .OrderByDescending(e => e.Id)
        //                                            .FirstOrDefault();

        //                if (Curr_Corp != null)
        //                {
        //                    RecruitBatchInitiator corp = new RecruitBatchInitiator();

        //                    string Geostructlist = Curr_Corp.Geostruct_Id == null ? null : Curr_Corp.Geostruct_Id.ToString();
        //                    string PostDetails = Curr_Corp.PostDetails_Id == null ? null : Curr_Corp.PostDetails_Id.ToString();
        //                    //  string ContactDetails = Curr_Corp.ContactDetails_Id == null ? null : Curr_Corp.ContactDetails_Id.ToString();
        //                    corp.Name = Curr_Corp.Name == null ? Old_Corp.Name : Curr_Corp.Name;
        //                    //  corp.Code = Curr_Corp.Code == null ? Old_Corp.Code : Curr_Corp.Code;
        //                    //      corp.Id = auth_id;

        //                    if (ModelState.IsValid)
        //                    {
        //                        try
        //                        {

        //                            //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                            {
        //                                // db.Configuration.AutoDetectChangesEnabled = false;
        //                                corp.DBTrack = new DBTrack
        //                                {
        //                                    CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
        //                                    CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
        //                                    Action = "M",
        //                                    ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
        //                                    ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
        //                                    AuthorizedBy = SessionManager.UserName,
        //                                    AuthorizedOn = DateTime.Now,
        //                                    IsModified = false
        //                                };
        //                                //    int a = EditS(Geostructlist, PostDetails, data, c, c.DBTrack);

        //                                int a = EditS(Geostructlist, PostDetails, auth_id, corp, corp.DBTrack);
        //                                //var CurCorp = db.Corporate.Find(auth_id);
        //                                //TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                                //db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //                                //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                                //{
        //                                //    c.DBTrack = new DBTrack
        //                                //    {
        //                                //        CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
        //                                //        CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
        //                                //        Action = "M",
        //                                //        ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
        //                                //        ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
        //                                //        AuthorizedBy = SessionManager.UserName,
        //                                //        AuthorizedOn = DateTime.Now,
        //                                //        IsModified = false
        //                                //    };
        //                                //    Corporate corp = new Corporate()
        //                                //    {
        //                                //        Code = c.Code,
        //                                //        Name = c.Name,
        //                                //        Id = Convert.ToInt32(auth_id),
        //                                //        DBTrack = c.DBTrack
        //                                //    };


        //                                //    db.Corporate.Attach(corp);
        //                                //    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;

        //                                //    // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
        //                                //    //db.SaveChanges();
        //                                //    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                                //    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                                //    await db.SaveChangesAsync();
        //                                //    //DisplayTrackedEntities(db.ChangeTracker);
        //                                //    db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
        //                                //    ts.Complete();
        //                                //    return Json(new Object[] { corp.Id, corp.Name, "Record Updated", JsonRequestBehavior.AllowGet });
        //                                //}

        //                                await db.SaveChangesAsync();

        //                                ts.Complete();
        //                                Msg.Add(" Record Authorised ");
        //                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                                //return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
        //                            }
        //                        }
        //                        catch (DbUpdateConcurrencyException ex)
        //                        {
        //                            var entry = ex.Entries.Single();
        //                            var clientValues = (RecruitBatchInitiator)entry.Entity;
        //                            var databaseEntry = entry.GetDatabaseValues();
        //                            if (databaseEntry == null)
        //                            {
        //                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
        //                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                            }
        //                            else
        //                            {
        //                                var databaseValues = (RecruitBatchInitiator)databaseEntry.ToObject();
        //                                corp.RowVersion = databaseValues.RowVersion;
        //                            }
        //                        }
        //                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                    }
        //                }
        //                else
        //                {
        //                    List<string> Msgr = new List<string>();
        //                    Msgr.Add("  Data removed.  ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);

        //                }
        //                // return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
        //            }
        //            else if (auth_action == "D")
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    //Corporate corp = db.Corporate.Find(auth_id);
        //                    RecruitBatchInitiator corp = db.RecruitBatchInitiator.AsNoTracking().Include(e => e.Geostruct)
        //                                                                .Include(e => e.PostDetails)
        //                                                              .FirstOrDefault(e => e.Id == auth_id);

        //                    GeoStruct add = corp.Geostruct;
        //                    PostDetails conDet = corp.PostDetails;
        //                    //   LookupValue val = corp.BusinessType;

        //                    corp.DBTrack = new DBTrack
        //                    {
        //                        Action = "D",
        //                        ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
        //                        CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
        //                        CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
        //                        IsModified = false,
        //                        AuthorizedBy = SessionManager.UserName,
        //                        AuthorizedOn = DateTime.Now
        //                    };

        //                    db.RecruitBatchInitiator.Attach(corp);
        //                    db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


        //                    var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, corp.DBTrack);
        //                    DT_RecruitBatchInitiator DT_Corp = (DT_RecruitBatchInitiator)rtn_Obj;
        //                    DT_Corp.Geostruct_Id = corp.Geostruct == null ? 0 : corp.Geostruct.Id;
        //                    DT_Corp.PostDetails_Id = corp.PostDetails == null ? 0 : corp.PostDetails.Id;
        //                    //DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
        //                    db.Create(DT_Corp);
        //                    await db.SaveChangesAsync();
        //                    db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
        //                    ts.Complete();
        //                    //return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
        //                    Msg.Add(" Record Authorised ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
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
        //        return View();
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
                IEnumerable<RecruitBatchInitiator> Corporate = null;
                if (gp.IsAutho == true)
                {
                    Corporate = db.RecruitBatchInitiator.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Corporate = db.RecruitBatchInitiator.AsNoTracking().ToList();
                }

                IEnumerable<RecruitBatchInitiator> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = Corporate;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.BatchName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                 || (e.JobReferenceNo.ToString().Contains(gp.searchString))
                                 || (e.initiatedDate.Value.ToShortDateString().ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                 || (e.Id.ToString().Contains(gp.searchString))
                                 ).Select(a => new Object[] { a.BatchName, a.JobReferenceNo, a.initiatedDate.Value.ToShortDateString(), a.Id }).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.BatchName, a.JobReferenceNo, a.initiatedDate.Value.ToShortDateString(), a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Corporate;
                    Func<RecruitBatchInitiator, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "BatchName" ? c.BatchName :
                                         gp.sidx == "JobReferenceNo" ? c.JobReferenceNo :
                                         gp.sidx == "InitiatedDate" ? c.initiatedDate.Value.ToShortDateString() :
                                        "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.BatchName), Convert.ToString(a.JobReferenceNo), a.initiatedDate.Value.ToShortDateString(), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.BatchName), Convert.ToString(a.JobReferenceNo), a.initiatedDate.Value.ToShortDateString(), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.BatchName), Convert.ToString(a.JobReferenceNo), a.initiatedDate.Value.ToShortDateString(), a.Id }).ToList();
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
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    RecruitBatchInitiator corporates = db.RecruitBatchInitiator
                                                       .Include(e => e.PostDetails)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    PostDetails conDet = corporates.PostDetails;
                    //  LookupValue val = corporates.BusinessType;
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
                            var rtn_Obj = DBTrackFile.DBTrackSave("Recruitment/Recruitment", null, db.ChangeTracker, corporates.DBTrack);
                            //DT_RecruitBatchInitiator DT_Corp = (DT_RecruitBatchInitiator)rtn_Obj;
                            //DT_Corp.Geostruct_Id = corporates.Geostruct == null ? 0 : corporates.Geostruct.Id;
                            //DT_Corp.PostDetails_Id = corporates.PostDetails == null ? 0 : corporates.PostDetails.Id;
                            //DT_Corp.ContactDetails_Id = corporates.ContactDetails == null ? 0 : corporates.ContactDetails.Id;
                            // db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        //   var selectedRegions = corporates;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(corporates.PostDetails.Select(e => e.Id));
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
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                            db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Recruitment/Recruitment", null, db.ChangeTracker, dbT);
                            //  DT_RecruitBatchInitiator DT_Corp = (DT_RecruitBatchInitiator)rtn_Obj;
                            // DT_Corp.Geostruct_Id = add == null ? 0 : add.Id;

                            // DT_Corp.PostDetails_Id = conDet == null ? 0 : conDet.Id;
                            // db.Create(DT_Corp);

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


        //public int EditS(string geoStructlist, string postDetalis, int data, RecruitBatchInitiator c, DBTrack dbT)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {

        //        if (geoStructlist != null)
        //        {
        //            if (geoStructlist != "")
        //            {
        //                var val = db.GeoStruct.Find(int.Parse(geoStructlist));
        //                c.Geostruct = val;

        //                var add = db.RecruitBatchInitiator.Include(e => e.Geostruct).Where(e => e.Id == data).SingleOrDefault();
        //                IList<RecruitBatchInitiator> addressdetails = null;
        //                if (add.Geostruct != null)
        //                {
        //                    addressdetails = db.RecruitBatchInitiator.Where(x => x.Geostruct.Id == add.Geostruct.Id && x.Id == data).ToList();
        //                }
        //                else
        //                {
        //                    addressdetails = db.RecruitBatchInitiator.Where(x => x.Id == data).ToList();
        //                }
        //                if (addressdetails != null)
        //                {
        //                    foreach (var s in addressdetails)
        //                    {
        //                        s.Geostruct = c.Geostruct;
        //                        db.RecruitBatchInitiator.Attach(s);
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                        // await db.SaveChangesAsync(false);
        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = s.RowVersion;
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            var addressdetails = db.RecruitBatchInitiator.Include(e => e.Geostruct).Where(x => x.Id == data).ToList();
        //            foreach (var s in addressdetails)
        //            {
        //                s.Geostruct = null;
        //                db.RecruitBatchInitiator.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }

        //        if (postDetalis != null)
        //        {
        //            if (postDetalis != "")
        //            {
        //                var val = db.PostDetails.Find(int.Parse(postDetalis));
        //                c.PostDetails = val;

        //                var add = db.RecruitBatchInitiator.Include(e => e.PostDetails).Where(e => e.Id == data).SingleOrDefault();
        //                IList<RecruitBatchInitiator> contactsdetails = null;
        //                if (add.PostDetails != null)
        //                {
        //                    contactsdetails = db.RecruitBatchInitiator.Where(x => x.PostDetails.Id == add.PostDetails.Id && x.Id == data).ToList();
        //                }
        //                else
        //                {
        //                    contactsdetails = db.RecruitBatchInitiator.Where(x => x.Id == data).ToList();
        //                }
        //                foreach (var s in contactsdetails)
        //                {
        //                    s.PostDetails = c.PostDetails;
        //                    db.RecruitBatchInitiator.Attach(s);
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                    //await db.SaveChangesAsync();
        //                    db.SaveChanges();
        //                    TempData["RowVersion"] = s.RowVersion;
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            var contactsdetails = db.RecruitBatchInitiator.Include(e => e.PostDetails).Where(x => x.Id == data).ToList();
        //            foreach (var s in contactsdetails)
        //            {
        //                s.PostDetails = null;
        //                db.RecruitBatchInitiator.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }

        //        var CurCorp = db.RecruitBatchInitiator.Find(data);
        //        TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //        db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //        {
        //            c.DBTrack = dbT;
        //            RecruitBatchInitiator corp = new RecruitBatchInitiator()
        //            {

        //                BatchName = c.BatchName,
        //                JobReferenceNo = c.JobReferenceNo,
        //                Narration = c.Narration,
        //                initiatedDate = c.initiatedDate,
        //                PublishDate = c.PublishDate,
        //                BatchCloseDate = c.BatchCloseDate,
        //                Id = data,
        //                DBTrack = c.DBTrack
        //            };


        //            db.RecruitBatchInitiator.Attach(corp);
        //            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //            return 1;
        //        }
        //        return 0;
        //    }
        //}

        public ActionResult GetLookupPostDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.PostDetails
                               .Include(e => e.FuncStruct)
                               .Include(e => e.FuncStruct.Job)
                               .Include(e => e.FuncStruct.Job.JobPosition)
                    //.Include(e => e.CategoryPost).Include(e => e.CategorySplPost).Include(e => e.Qualification)
                    //.Include(e => e.Skill).Include(e => e.FuncStruct)
                                     .ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.PostDetails
                                //.Include(e => e.CategoryPost).Include(e => e.CategorySplPost).Include(e => e.Qualification)
                                //.Include(e => e.Skill).Include(e => e.FuncStruct).Include(e => e.AgeFrom).Include(e => e.AgeTo).Include(e => e.ExpFilter)
                                //.Include(e => e.ExpYearFrom).Include(e => e.Gender).Include(e => e.MaritalStatus).Include(e => e.Narration)
                                //.Include(e => e.NoOfVacancies).Include(e => e.RangeFilter)
                                   .Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
            // return View();
        }

        public ActionResult GetLookup_ResumeCollection(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ResumeCollection
                    .Include(e => e.Candidate)
                    .Include(e => e.CandidateDocuments)
                    .Include(e => e.EmpCTCStruct)
                    .Include(e => e.ModeOfResume)
                    .Include(e => e.RecruitEvaluationProcessResult)
                    .Include(e => e.RecruitJoinParaProcessResult)
                    .Include(e => e.ResumeSortlistingStatus)
                    .Include(e => e.ShortlistingCriteria)
                                     .ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ResumeCollection
                                .Include(e => e.Candidate)
                                .Include(e => e.CandidateDocuments)
                                .Include(e => e.EmpCTCStruct)
                                .Include(e => e.ModeOfResume)
                                .Include(e => e.RecruitEvaluationProcessResult)
                                .Include(e => e.RecruitJoinParaProcessResult)
                                .Include(e => e.ResumeSortlistingStatus)
                                .Include(e => e.ShortlistingCriteria)
                                   .Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
            // return View();
        }

        //[HttpPost]
        ////[ValidateAntiForgeryToken]
        //public ActionResult DeleteContactDetails(int? data, int? forwarddata)
        //{
        //    ContactDetails contDet = db.ContactDetails.Find(data);
        //    Corporate corp = db.Corporate.Find(forwarddata);
        //    try
        //    {
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            corp.ContactDetails = null;
        //            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //            ts.Complete();
        //        }
        //        //return Json(new Object[] { "", "", "Data removed.", JsonRequestBehavior.AllowGet });
        //        return Json(new { msg = "Data removed.", JsonRequestBehavior.AllowGet });
        //    }

        //    catch (DataException /* dex */)
        //    {
        //        //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
        //        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
        //        //ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
        //        //return RedirectToAction("Index");
        //    }
        //}


        //[HttpPost]
        ////[ValidateAntiForgeryToken]
        //public ActionResult DeleteAddress(int? data, int? forwarddata)
        //{
        //    Address addrs = db.Address.Find(data);
        //    Corporate corp = db.Corporate.Find(forwarddata);
        //    try
        //    {
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            corp.Address = null;
        //            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //            ts.Complete();
        //        }
        //        return Json(new Object[] { "", "", "Data removed.", JsonRequestBehavior.AllowGet });
        //       // return this.Json(new { msg = "Data deleted." });
        //    }

        //    catch (DataException /* dex */)
        //    {
        //        //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
        //        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
        //        //ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
        //        //return RedirectToAction("Index");
        //    }
        //}

        //public 

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
        //public ActionResult GetLookup_location(List<int> SkipIds)
        //{
        //    var fall = db.GeoStruct.Include(e => e.Company).Include(e => e.Department.DepartmentObj).Include(e => e.Department).Include(e => e.Location).Include(e => e.Location.LocationObj)
        //        .Include(e => e.Region).Include(e => e.Unit).ToList();

        //    if (SkipIds != null)
        //    {
        //        foreach (var a in SkipIds)
        //        {
        //            if (fall == null)
        //            {
        //                fall = db.GeoStruct.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

        //            }
        //            else
        //            {
        //                fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
        //            }
        //        }
        //    }
        //    var list1 = db.GeoStruct.SelectMany(e => e.FullDetails).ToList();
        //    var list2 = fall.Except(list1);
        //    var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
        //    return Json(r, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult GetLookup_location(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Location.Include(e => e.LocationObj).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                        {
                            fall = db.Location.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                        }
                        else
                        {
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        }
                    }
                }
                var list1 = db.Division.SelectMany(e => e.Locations).ToList();
                var list2 = fall.Except(list1);
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
    }
}