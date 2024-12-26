using CMS_SPS;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Transactions;
using P2b.Global;
using P2BUltimate.Security;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure;

namespace P2BUltimate.Controllers.CMS
{
    public class PersonnelModelController : Controller
    {
        // GET: PersonnelModel
        public ActionResult Index()
        {     
            return View("~/Views/CMS/MainViews/PersonnelModel/index.cshtml");
        }
        public ActionResult PersonnelModelPartial()
        {
            return View("~/Views/Shared/CMS/PersonnelModelObject.cshtml");
        }
        public class returnDataClass
        {
            public int Id { get; set; }
            public string PersonnelModel { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }
        public ActionResult GetLookupPersonnelModelObject(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.PersonnelModelObject.Include(e => e.PersonnelModel)
                   .Include(e => e.CompetencyEvaluationModel).Include(e => e.CompetencyEvaluationModel.Criteria)
                   .Include(e => e.CompetencyEvaluationModel.CriteriaType)
                   .Include(e => e.CompetencyEvaluationModel.DataSteps).ToList();

                if (fall != null)
                {
                    List<returnDataClass> returndata = new List<returnDataClass>();

                    foreach (var item in fall)
                    {
                        returndata.Add(new returnDataClass
                        {
                            Id = item.Id,
                            PersonnelModel = item.PersonnelModel != null ? item.PersonnelModel.LookupVal : "",
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                        });

                    }
                    var res = (from ca in returndata
                               select new
                               {
                                   srno = ca.Id,
                                   lookupvalue = ca.PersonnelModel + " " + ca.Creteriases + " " + ca.CreteriaTypeses + " " + ca.DataStepses
                               }).ToList();

                    var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();

                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                return null;

            }
        }
        [HttpPost]
        public ActionResult Create(PersonnelModel c, FormCollection form)
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    CompanyCMS_SPS companycms_sps = new CompanyCMS_SPS();
                    companycms_sps = db.CompanyCMS_SPS.Include(e => e.PersonnelModel).Where(e => e.Company.Id == company_Id).FirstOrDefault();

                    string PersonnelModelObjectList = form["PersonnelModelObjectList"] == "0" ? null : form["PersonnelModelObjectList"];
                    if (PersonnelModelObjectList != null && PersonnelModelObjectList != "")
                    {
                        var ids = Utility.StringIdsToListIds(PersonnelModelObjectList);
                        var AppraisalPersonnelModelObjectlist = new List<PersonnelModelObject>();
                        foreach (var item in ids)
                        {
                            var returndata = db.PersonnelModelObject.Include(e => e.PersonnelModel)
                                .Include(e => e.CompetencyEvaluationModel).Include(e => e.CompetencyEvaluationModel.Criteria)
                                .Include(e => e.CompetencyEvaluationModel.CriteriaType).Include(e => e.CompetencyEvaluationModel.DataSteps)
                                .Where(e => e.Id == item).SingleOrDefault();

                            if (returndata != null && returndata.PersonnelModel != null && returndata.CompetencyEvaluationModel != null)
                            {
                                var appraisalpersonnelmodel = returndata.PersonnelModel;
                                var evaluationmodel = returndata.CompetencyEvaluationModel;
                                c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };
                                PersonnelModelObject objpersonnelmodel = new PersonnelModelObject()
                                {
                                    PersonnelModel = appraisalpersonnelmodel,
                                    CompetencyEvaluationModel = evaluationmodel,
                                    DBTrack = c.DBTrack
                                };
                                AppraisalPersonnelModelObjectlist.Add(objpersonnelmodel);
                            }
                        }
                        c.PersonnelModelObject = AppraisalPersonnelModelObjectlist;
                    }
                    else
                    {
                        Msg.Add("please select personnel object");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                     if (PersonnelModelObjectList != null && PersonnelModelObjectList != "")
                        {
                        var ids = Utility.StringIdsToListIds(PersonnelModelObjectList);
                        List<int> AuthorList = new List<int>();
                        foreach (var ca in ids)
                        {
                            var PersonnelModelList = db.PersonnelModelObject.Include(e => e.PersonnelModel).Where(e => e.Id == ca).FirstOrDefault();
                            if (PersonnelModelList != null)
                            {
                                int AppraisalPersonnelModelId = PersonnelModelList.PersonnelModel != null ? PersonnelModelList.PersonnelModel.Id : 0;
                                int PersonnelFullDetailsId = db.LookupValue.Where(e => e.Id == AppraisalPersonnelModelId).SingleOrDefault().Id;
                                AuthorList.Add(PersonnelFullDetailsId);

                            }

                        }

                        var duplicateExists = AuthorList.GroupBy(x => x)
                                             .Where(g => g.Count() > 1)
                                              .Select(y => y.Key)
                                               .ToList();
                        if (duplicateExists.Count() > 0)
                        {
                            Msg.Add("duplicate personnel Object not allow");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }


                    var CodeValue = db.PersonnelModel.Where(e => e.Code == c.Code).FirstOrDefault();
                    if (CodeValue != null)
                    {
                        Msg.Add("Already Code Is Insert");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            PersonnelModel PersonnelModel = new PersonnelModel()
                            {
                                Code = c.Code,
                                ModelName = c.ModelName,
                                ModelDescription = c.ModelDescription,
                                CreatedDate = DateTime.Now.Date,
                                PersonnelModelObject = c.PersonnelModelObject,
                                DBTrack = c.DBTrack
                            };

                            db.PersonnelModel.Add(PersonnelModel);
                            db.SaveChanges();

                            if (companycms_sps != null)
                            {
                                List<PersonnelModel> appraisalpersonnelmodel_list = new List<PersonnelModel>();
                                appraisalpersonnelmodel_list.Add(PersonnelModel);
                                if (companycms_sps.PersonnelModel != null)
                                {
                                    appraisalpersonnelmodel_list.AddRange(companycms_sps.PersonnelModel);
                                }
                                companycms_sps.PersonnelModel = appraisalpersonnelmodel_list;
                                db.CompanyCMS_SPS.Attach(companycms_sps);
                                db.Entry(companycms_sps).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(companycms_sps).State = System.Data.Entity.EntityState.Detached;

                            }
                            try
                            {
                          

                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
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
        public class returnEditClass
        {
            public string PersonnelModelObject_Id { get; set; }
            public string PersonnelModelObject_FullDetails { get; set; }
        }
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    PersonnelModel personnelmodels = db.PersonnelModel
                                                      .Include(e => e.PersonnelModelObject)
                                                     .Where(e => e.Id == data).SingleOrDefault();

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {



                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now,
                            CreatedBy = personnelmodels.DBTrack.CreatedBy != null ? personnelmodels.DBTrack.CreatedBy : null,
                            CreatedOn = personnelmodels.DBTrack.CreatedOn != null ? personnelmodels.DBTrack.CreatedOn : null,
                            IsModified = personnelmodels.DBTrack.IsModified == true ? false : false//,

                        };
                        db.Entry(personnelmodels).State = System.Data.Entity.EntityState.Deleted;
                        await db.SaveChangesAsync();
                        ts.Complete();
                        Msg.Add("  Data removed.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    //}
                }
                catch (RetryLimitExceededException /* dex */)
                {

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
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                var returndata = db.PersonnelModel.Include(e => e.PersonnelModelObject)
                     .Where(e => e.Id == data).ToList();

                foreach (var item in returndata)
                {
                    result.Add(new returndatagridDataclass
                    {
                        Id = item.Id.ToString(),
                        Code = item.Code,
                        ModelName = item.ModelName,
                        ModelDescription = item.ModelDescription,
                        CreatedDate = item.CreatedDate.Value.ToShortDateString()

                    });
                }
                var return_data = db.PersonnelModel.Include(e => e.PersonnelModelObject)
                    .Include(e => e.PersonnelModelObject.Select(r => r.PersonnelModel))
                      .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel))
                        .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                          .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                    .Where(e => e.Id == data && e.PersonnelModelObject.Count > 0).SingleOrDefault();

                List<returnDataClass> returndatases = new List<returnDataClass>();
                if (return_data != null && return_data.PersonnelModelObject != null)
                {
                    foreach (var item in return_data.PersonnelModelObject)
                    {
                        returndatases.Add(new returnDataClass
                        {
                            Id = item.Id,
                            PersonnelModel = item.PersonnelModel != null ? item.PersonnelModel.LookupVal : "",
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                        });
                    }
                }
                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();
                if (returndatases != null)
                {

                    foreach (var item1 in returndatases)
                    {
                        oreturnEditClass.Add(new returnEditClass
                        {
                            PersonnelModelObject_Id = item1.Id.ToString(),
                            PersonnelModelObject_FullDetails = item1.PersonnelModel + " " + item1.Creteriases + " " + item1.CreteriaTypeses + "" + item1.DataStepses
                        });
                    }
                }

                return Json(new Object[] { result, oreturnEditClass, JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(PersonnelModel c, int data, FormCollection form)
        {

            List<string> Msg = new List<string>();
            string PersonnelModelObjectList = form["PersonnelModelObjectList"] == "0" ? null : form["PersonnelModelObjectList"];
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        if (PersonnelModelObjectList != null && PersonnelModelObjectList != "")
                        {
                            var ids = Utility.StringIdsToListIds(PersonnelModelObjectList);
                            List<int> AuthorList = new List<int>();
                            foreach (var ca in ids)
                            {
                                var PersonnelModelList = db.PersonnelModelObject.Include(e => e.PersonnelModel).Where(e => e.Id == ca).FirstOrDefault();
                                if (PersonnelModelList != null)
                                {
                                    int AppraisalPersonnelModelId = PersonnelModelList.PersonnelModel != null ? PersonnelModelList.PersonnelModel.Id : 0;
                                    int PersonnelFullDetailsId = db.LookupValue.Where(e => e.Id == AppraisalPersonnelModelId).SingleOrDefault().Id;
                                    AuthorList.Add(PersonnelFullDetailsId);

                                }

                            }

                            var duplicateExists = AuthorList.GroupBy(x => x)
                                                 .Where(g => g.Count() > 1)
                                                  .Select(y => y.Key)
                                                   .ToList();
                            if (duplicateExists.Count() > 0)
                            {
                                Msg.Add("duplicate personnel Object not allow");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        var db_data = db.PersonnelModel.Include(e => e.PersonnelModelObject).Where(e => e.Id == data).SingleOrDefault();
                        List<PersonnelModelObject> CMS_PersonnelModelObject = new List<PersonnelModelObject>();
                        if (PersonnelModelObjectList != "" && PersonnelModelObjectList != null)
                        {
                            var ids = Utility.StringIdsToListIds(PersonnelModelObjectList);
                            foreach (var ca in ids)
                            {
                                var Values_val = db.PersonnelModelObject.Find(ca);
                                CMS_PersonnelModelObject.Add(Values_val);
                            }
                            db_data.PersonnelModelObject = CMS_PersonnelModelObject;
                        }
                        else
                        {
                            Msg.Add("please select personnel object");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        db.PersonnelModel.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        TempData["RowVersion"] = db_data.RowVersion;
                        PersonnelModel CMS_PersonnelModel = db.PersonnelModel.Find(data);
                        TempData["CurrRowVersion"] = CMS_PersonnelModel.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = CMS_PersonnelModel.DBTrack.CreatedBy == null ? null : CMS_PersonnelModel.DBTrack.CreatedBy,
                                CreatedOn = CMS_PersonnelModel.DBTrack.CreatedOn == null ? null : CMS_PersonnelModel.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            CMS_PersonnelModel.Id = data;
                            CMS_PersonnelModel.Code = c.Code;
                            CMS_PersonnelModel.ModelName = c.ModelName;
                            CMS_PersonnelModel.ModelDescription = c.ModelDescription;
                            CMS_PersonnelModel.CreatedDate = c.CreatedDate;
                            CMS_PersonnelModel.DBTrack = c.DBTrack;
                            CMS_PersonnelModel.PersonnelModelObject = db_data.PersonnelModelObject;
                            db.PersonnelModel.Attach(CMS_PersonnelModel);
                            db.Entry(CMS_PersonnelModel).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
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
        public class returndatagridDataclass //Parentgrid
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string ModelName { get; set; }
            public string ModelDescription { get; set; }
            public string CreatedDate { get; set; }

        }
        public class returndatagridChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string PersonnelModel { get; set; }
            public string EvaluationModel { get; set; }

        }
        public ActionResult PersonnelModel_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.PersonnelModel.Include(e => e.PersonnelModelObject)
                       .Include(e => e.PersonnelModelObject.Select(r => r.PersonnelModel))
                       .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel))
                       .ToList();

                    IEnumerable<PersonnelModel> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.Code.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                  || (e.ModelName.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  ).ToList();
                    }
                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<PersonnelModel, string> orderfunc = (c =>
                                                               Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                               sortindex == 1 ? c.Code : "");
                    var sortcolumn = Request["sSortDir_0"];
                    if (sortcolumn == "asc")
                    {
                        fall = fall.OrderBy(orderfunc);
                    }
                    else
                    {
                        fall = fall.OrderByDescending(orderfunc);
                    }

                    // Paging 
                    var dcompanies = fall
                            .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompanies.Count == 0)
                    {
                        List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                        foreach (var item in fall)
                        {
                            result.Add(new returndatagridDataclass
                            {
                                Id = item.Id.ToString(),
                                Code = item.Code,
                                ModelName = item.ModelName,
                                ModelDescription = item.ModelDescription,
                                CreatedDate = item.CreatedDate.Value.ToShortDateString()
                            });
                        }
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = from c in dcompanies
                                     select new[] { null, Convert.ToString(c.Id), c.Code, };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result

                        }, JsonRequestBehavior.AllowGet);



                    }

                }
                catch (Exception e)
                {
                    List<string> Msg = new List<string>();
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
        public ActionResult A_PersonnelModel_Grid(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.PersonnelModel
                        .Include(e => e.PersonnelModelObject)
                        .Include(e => e.PersonnelModelObject.Select(r => r.PersonnelModel))
                        .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel))
                        .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                        .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                        .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                        .Where(e => e.Id == data).SingleOrDefault();

                    if (db_data.PersonnelModelObject != null)
                    {

                        List<returnDataClass> returndata = new List<returnDataClass>();
                        foreach (var item in db_data.PersonnelModelObject)
                        {
                            returndata.Add(new returnDataClass
                            {
                                Id = item.Id,
                                PersonnelModel = item.PersonnelModel != null ? item.PersonnelModel.LookupVal : "",
                                DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                                Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                                CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                            });
                        }
                        List<returndatagridChildDataClass> returndatases = new List<returndatagridChildDataClass>();
                        foreach (var item1 in returndata)
                        {
                            returndatases.Add(new returndatagridChildDataClass
                            {
                                Id = item1.Id,
                                PersonnelModel = item1.PersonnelModel,
                                EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

                            });
                        }

                        return Json(returndatases, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
                    }

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
    }
}