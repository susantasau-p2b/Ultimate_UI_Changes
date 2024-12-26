using CMS_SPS;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Transactions;
using P2BUltimate.Models;
using System.Data.Entity.Infrastructure;
using System.Data;
using System.Text;
using P2b.Global;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.CMS
{
    public class CompetencyEmployeeDataTController : Controller
    {
        //
        // GET: /CompetencyEmployeeDataT/
        public ActionResult Index()
        {
            Session["CriteriaData"] = null;
            return View("~/Views/CMS/MainViews/CompetencyEmployeeDataT/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/CMS/_EvaluationCriteria.cshtml");
        }

        public ActionResult GetBatchName(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var qurey = db.CompetencyModelAssignment.Where(e => e.CloseDate == null).ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "BatchName", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PopulateDropDownLisSubtype(string BatchName, string ModelObject)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
               
                if (BatchName != "" && BatchName != null && BatchName != "0")
                {
                    int? selected = null;
                    var filter = Convert.ToInt32(BatchName);

                   
                    var fall = db.CompetencyModelAssignment.Include(e => e.CompetencyModel)
                                          .Include(e => e.CompetencyModel.AppraisalAttributeModel)
                                          .Include(e => e.CompetencyModel.AppraisalBusinessApprisalModel)
                                          .Include(e => e.CompetencyModel.AppraisalKRAModel)
                                          .Include(e => e.CompetencyModel.AppraisalPotentialModel)
                                          .Include(e => e.CompetencyModel.PastExperienceModel)
                                          .Include(e => e.CompetencyModel.PersonnelModel)
                                          .Include(e => e.CompetencyModel.QualificationModel)
                                          .Include(e => e.CompetencyModel.ServiceModel)
                                          .Include(e => e.CompetencyModel.SkillModel)
                                          .Include(e => e.CompetencyModel.TrainingModel)
                                          .Where(e => e.Id == filter).FirstOrDefault();
                    var query2 = fall;
                    List<Item> items = new List<Item>();
                    if (query2.CompetencyModel.AppraisalAttributeModel != null && ModelObject == query2.CompetencyModel.AppraisalAttributeModel.ModelName)
                    {
                        var OObject = db.AppraisalAttributeModel.Where(e => e.Id == query2.CompetencyModel.AppraisalAttributeModel.Id)
                              .Include(e => e.AppraisalAttributeModelObject).Include(e => e.AppraisalAttributeModelObject.Select(t => t.AppraisalAttributeModel)).FirstOrDefault();
                        if                                                                                                                                                                                                                                                                                                                                    (OObject.AppraisalAttributeModelObject.Count() > 0)
                        {
                            foreach (var item in OObject.AppraisalAttributeModelObject)
                            {
                                int Id = item.Id;
                                string ModelName = item.AppraisalAttributeModel.LookupVal.ToString();
                                items.Add(new Item { Id = Id, Name = ModelName });
                            }
                        }
                           
                    }
                    if (query2.CompetencyModel.AppraisalBusinessApprisalModel != null && ModelObject == query2.CompetencyModel.AppraisalBusinessApprisalModel.ModelName)
                    {
                        var OObject = db.AppraisalBusinessAppraisalModel.Where(e => e.Id == query2.CompetencyModel.AppraisalBusinessApprisalModel.Id)
                             .Include(e => e.AppraisalBusinessAppraisalModelObject).Include(e => e.AppraisalBusinessAppraisalModelObject.Select(t => t.AppraisalBusinessAppraisalModel)).FirstOrDefault();
                        if (OObject.AppraisalBusinessAppraisalModelObject.Count() > 0)
                        {
                            foreach (var item in OObject.AppraisalBusinessAppraisalModelObject)
                            {
                                int Id = item.Id;
                                string ModelName = item.AppraisalBusinessAppraisalModel.LookupVal.ToString();
                                items.Add(new Item { Id = Id, Name = ModelName });
                            }
                        }
                       
                    }
                    if (query2.CompetencyModel.AppraisalKRAModel != null && ModelObject == query2.CompetencyModel.AppraisalKRAModel.ModelName)
                    {
                        var OObject = db.AppraisalKRAModel.Where(e => e.Id == query2.CompetencyModel.AppraisalKRAModel.Id)
                              .Include(e => e.AppraisalKRAModelObject).Include(e => e.AppraisalKRAModelObject.Select(t => t.AppraisalKRAModel)).FirstOrDefault();
                        if (OObject.AppraisalKRAModelObject.Count() > 0)
                        {
                            foreach (var item in OObject.AppraisalKRAModelObject)
                            {
                                int Id = item.Id;
                                string ModelName = item.AppraisalKRAModel.LookupVal.ToString();
                                items.Add(new Item { Id = Id, Name = ModelName });
                            }
                        }
                       
                    }
                    if (query2.CompetencyModel.AppraisalPotentialModel != null && ModelObject == query2.CompetencyModel.AppraisalPotentialModel.ModelName)
                    {
                        var OObject = db.AppraisalPotentialModel.Where(e => e.Id == query2.CompetencyModel.AppraisalPotentialModel.Id)
                             .Include(e => e.AppraisalPotentialModelObject).Include(e => e.AppraisalPotentialModelObject.Select(t => t.AppraisalPotentialModel)).FirstOrDefault();
                        if (OObject.AppraisalPotentialModelObject.Count() > 0)
                        {
                            foreach (var item in OObject.AppraisalPotentialModelObject)
                            {
                                int Id = item.Id;
                                string ModelName = item.AppraisalPotentialModel.LookupVal.ToString();
                                items.Add(new Item { Id = Id, Name = ModelName });
                            }
                        }
                       
                    }
                    if (query2.CompetencyModel.PastExperienceModel != null && ModelObject == query2.CompetencyModel.PastExperienceModel.ModelName)
                    {
                        var OObject = db.PastExperienceModel.Where(e => e.Id == query2.CompetencyModel.PastExperienceModel.Id)
                              .Include(e => e.PastExperienceModelObject).Include(e => e.PastExperienceModelObject.Select(t => t.PastExperienceModel)).FirstOrDefault();
                        if (OObject.PastExperienceModelObject.Count() > 0)
                        {
                            foreach (var item in OObject.PastExperienceModelObject)
                            {
                                int Id = item.Id;
                                string ModelName = item.PastExperienceModel.LookupVal.ToString();
                                items.Add(new Item { Id = Id, Name = ModelName });
                            }
                        }
                       
                    }
                    if (query2.CompetencyModel.PersonnelModel != null && ModelObject == query2.CompetencyModel.PersonnelModel.ModelName)
                    {
                            var OObject = db.PersonnelModel.Where(e => e.Id == query2.CompetencyModel.PersonnelModel.Id)
                                .Include(e => e.PersonnelModelObject).Include(e => e.PersonnelModelObject.Select(t => t.PersonnelModel)).FirstOrDefault();
                            if (OObject.PersonnelModelObject.Count() > 0)
                            {
                                foreach (var item in OObject.PersonnelModelObject)
                                {
                                    int Id = item.Id;
                                    string ModelName = item.PersonnelModel.LookupVal.ToString();
                                    items.Add(new Item { Id = Id, Name = ModelName });
                                } 
                            }
                    }
                    if (query2.CompetencyModel.QualificationModel != null && ModelObject == query2.CompetencyModel.QualificationModel.ModelName)
                    {
                        var OObject = db.QualificationModel.Where(e => e.Id == query2.CompetencyModel.QualificationModel.Id)
                             .Include(e => e.QualificationModelObject).Include(e => e.QualificationModelObject.Select(t => t.QualificationModel)).FirstOrDefault();
                        if (OObject.QualificationModelObject.Count() > 0)
                        {
                            foreach (var item in OObject.QualificationModelObject)
                            {
                                int Id = item.Id;
                                string ModelName = item.QualificationModel.LookupVal.ToString();
                                items.Add(new Item { Id = Id, Name = ModelName });
                            }
                        }
                       
                    }
                    if (query2.CompetencyModel.ServiceModel != null && ModelObject == query2.CompetencyModel.ServiceModel.ModelName)
                    {
                        var OObject = db.ServiceModel.Where(e => e.Id == query2.CompetencyModel.ServiceModel.Id)
                             .Include(e => e.ServiceModelObject).Include(e => e.ServiceModelObject.Select(t => t.ServiceModel)).FirstOrDefault();
                        if (OObject.ServiceModelObject.Count() > 0)
                        {
                            foreach (var item in OObject.ServiceModelObject)
                            {
                                int Id = item.Id;
                                string ModelName = item.ServiceModel.LookupVal.ToString();
                                items.Add(new Item { Id = Id, Name = ModelName });
                            }
                        }
                       
                    }
                    if (query2.CompetencyModel.SkillModel != null && ModelObject == query2.CompetencyModel.SkillModel.ModelName)
                    {
                        var OObject = db.SkillModel.Where(e => e.Id == query2.CompetencyModel.SkillModel.Id)
                              .Include(e => e.SkillModelObject).Include(e => e.SkillModelObject.Select(t => t.SkillModel)).FirstOrDefault();
                        if (OObject.SkillModelObject.Count() > 0)
                        {
                            foreach (var item in OObject.SkillModelObject)
                            {
                                int Id = item.Id;
                                string ModelName = item.SkillModel.LookupVal.ToString();
                                items.Add(new Item { Id = Id, Name = ModelName });
                            }
                        }
                       
                    }
                    if (query2.CompetencyModel.TrainingModel != null && ModelObject == query2.CompetencyModel.TrainingModel.ModelName)
                    {
                        var OObject = db.TrainingModel.Where(e => e.Id == query2.CompetencyModel.TrainingModel.Id)
                             .Include(e => e.TrainingModelObject).Include(e => e.TrainingModelObject.Select(t => t.TrainingModel)).FirstOrDefault();
                        if (OObject.TrainingModelObject.Count() > 0)
                        {
                            foreach (var item in OObject.TrainingModelObject)
                            {
                                int Id = item.Id;
                                string ModelName = item.TrainingModel.LookupVal.ToString();
                                items.Add(new Item { Id = Id, Name = ModelName });
                            }
                        }
                       
                    }

                    var selected1 = (Object)null;
                    //if (data2 != "" && data != "0" && data2 != "0")
                    //{
                    //    selected1 = Convert.ToInt32(data2);
                    //}

                    SelectList s = new SelectList(items, "Id", "Name", selected1);


                    return Json(s, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }


        public string GetModelNamelist(string BatchId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int filter = Convert.ToInt32(BatchId);
                var qurey = db.CompetencyModelAssignment.Include(e => e.CompetencyModel)
                    .Where(e => e.Id == filter && e.CompetencyModel != null).FirstOrDefault();
              
                return qurey.CompetencyModel.ModelName;
            }
        }
        public class Item
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string EvaluationType { get; set; }
        }

        public ActionResult PopulateDropDownListProcBatch(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                // var selected = (Object)null;

                //<<<<<<< .mine
                if (data != "" && data != null && data != "0")
                {
                    int? selected = null;
                    var filter = Convert.ToInt32(data);

                    if (data2 != "" && data2 != null && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);
                    }
                    var fall = db.CompetencyBatchProcessT
                                          .Where(e => e.BatchName_Id == filter).ToList();
                    var query2 = fall;
                    List<Item> items = new List<Item>();
                   
                    if (query2 != null)
                    {
                        foreach (var item in query2)
                        {
                            items.Add(new Item { Id = item.Id, Name = item.ProcessBatch });
                        }
                        
                        //.Add(new Item { Id = query2.CompetencyModel.TrainingModel.Id, Name = "TrainingModel" });
                    }

                    var selected1 = (Object)null;
                    if (data2 != "" && data != "0" && data2 != "0")
                    {
                        selected1 = Convert.ToInt32(data2);
                    }

                    SelectList s = new SelectList(items, "Id", "Name", selected1);


                    return Json(s, JsonRequestBehavior.AllowGet);
                }
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                // var selected = (Object)null;

                //<<<<<<< .mine
                if (data != "" && data != null && data != "0")
                {
                    int? selected = null;
                    var filter = Convert.ToInt32(data);

                    if (data2 != "" && data2 != null && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);
                    }
                    var fall = db.CompetencyModelAssignment.Include(e => e.CompetencyModel)
                                          .Include(e => e.CompetencyModel.AppraisalAttributeModel)
                                          .Include(e => e.CompetencyModel.AppraisalBusinessApprisalModel)
                                          .Include(e => e.CompetencyModel.AppraisalKRAModel)
                                          .Include(e => e.CompetencyModel.AppraisalPotentialModel)
                                          .Include(e => e.CompetencyModel.PastExperienceModel)
                                          .Include(e => e.CompetencyModel.PersonnelModel)
                                          .Include(e => e.CompetencyModel.QualificationModel)
                                          .Include(e => e.CompetencyModel.ServiceModel)
                                          .Include(e => e.CompetencyModel.SkillModel)
                                          .Include(e => e.CompetencyModel.TrainingModel)
                                          .Where(e => e.Id == filter).FirstOrDefault();
                    var query2 = fall;
                    List<Item> items = new List<Item>();
                    if (query2.CompetencyModel.AppraisalAttributeModel != null)
                    {
                        items.Add(new Item { Id = query2.CompetencyModel.AppraisalAttributeModel.Id, Name = query2.CompetencyModel.AppraisalAttributeModel.ModelName });
                        //items.Add(new Item { Id = query2.CompetencyModel.AppraisalAttributeModel.Id, Name = "AppraisalAttributeModel" });
                    }
                    if (query2.CompetencyModel.AppraisalBusinessApprisalModel != null)
                    {
                        items.Add(new Item { Id = query2.CompetencyModel.AppraisalBusinessApprisalModel.Id, Name = query2.CompetencyModel.AppraisalBusinessApprisalModel.ModelName });
                       // items.Add(new Item { Id = query2.CompetencyModel.AppraisalBusinessApprisalModel.Id, Name = "AppraisalBusinessApprisalModel" });
                    }
                    if (query2.CompetencyModel.AppraisalKRAModel != null)
                    {
                       items.Add(new Item { Id = query2.CompetencyModel.AppraisalKRAModel.Id, Name = query2.CompetencyModel.AppraisalKRAModel.ModelName });
                        //items.Add(new Item { Id = query2.CompetencyModel.AppraisalKRAModel.Id, Name = "AppraisalKRAModel" });
                    }
                    if (query2.CompetencyModel.AppraisalPotentialModel != null)
                    {
                        items.Add(new Item { Id = query2.CompetencyModel.AppraisalPotentialModel.Id, Name = query2.CompetencyModel.AppraisalPotentialModel.ModelName });
                        //items.Add(new Item { Id = query2.CompetencyModel.AppraisalPotentialModel.Id, Name = "AppraisalPotentialModel" });
                    }
                    if (query2.CompetencyModel.PastExperienceModel != null)
                    {
                        items.Add(new Item { Id = query2.CompetencyModel.PastExperienceModel.Id, Name = query2.CompetencyModel.PastExperienceModel.ModelName });
                        //items.Add(new Item { Id = query2.CompetencyModel.PastExperienceModel.Id, Name = "PastExperienceModel" });
                    }
                    if (query2.CompetencyModel.PersonnelModel != null)
                    {
                        items.Add(new Item { Id = query2.CompetencyModel.PersonnelModel.Id, Name = query2.CompetencyModel.PersonnelModel.ModelName });
                        //items.Add(new Item { Id = query2.CompetencyModel.PersonnelModel.Id, Name = "PersonnelModel" });
                    }
                    if (query2.CompetencyModel.QualificationModel != null)
                    {
                        items.Add(new Item { Id = query2.CompetencyModel.QualificationModel.Id, Name = query2.CompetencyModel.QualificationModel.ModelName });
                       // items.Add(new Item { Id = query2.CompetencyModel.QualificationModel.Id, Name = "QualificationModel" });
                    }
                    if (query2.CompetencyModel.ServiceModel != null)
                    {
                        items.Add(new Item { Id = query2.CompetencyModel.ServiceModel.Id, Name = query2.CompetencyModel.ServiceModel.ModelName });
                        //items.Add(new Item { Id = query2.CompetencyModel.ServiceModel.Id, Name = "ServiceModel" });
                    }
                    if (query2.CompetencyModel.SkillModel != null)
                    {
                        items.Add(new Item { Id = query2.CompetencyModel.SkillModel.Id, Name = query2.CompetencyModel.SkillModel.ModelName });
                       // items.Add(new Item { Id = query2.CompetencyModel.SkillModel.Id, Name = "SkillModel" });
                    }
                    if (query2.CompetencyModel.TrainingModel != null)
                    {
                        items.Add(new Item { Id = query2.CompetencyModel.TrainingModel.Id, Name = query2.CompetencyModel.TrainingModel.ModelName });
                        //.Add(new Item { Id = query2.CompetencyModel.TrainingModel.Id, Name = "TrainingModel" });
                    }

                    var selected1 = (Object)null;
                    if (data2 != "" && data != "0" && data2 != "0")
                    {
                        selected1 = Convert.ToInt32(data2);
                    }

                    SelectList s = new SelectList(items, "Id", "Name", selected1);


                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (data2 != "")
                    {
                        int selected = Convert.ToInt32(data2);
                        var fall = db.CompetencyModel
                   .Include(e => e.AppraisalAttributeModel)
                   .Include(e => e.AppraisalBusinessApprisalModel)
                   .Include(e => e.AppraisalKRAModel)
                   .Include(e => e.AppraisalPotentialModel)
                   .Include(e => e.PastExperienceModel)
                   .Include(e => e.PersonnelModel)
                   .Include(e => e.QualificationModel)
                   .Include(e => e.ServiceModel)
                   .Include(e => e.SkillModel)
                   .Include(e => e.TrainingModel)
                   .Where(e => e.Id == selected).FirstOrDefault();
                        var query2 = fall;
                        var qurey = db.State.Where(e => e.Id == selected).ToList();
                        List<Item> items = new List<Item>();
                        if (query2.PastExperienceModel != null)
                        {
                            int PastExperienceModelId = query2.PastExperienceModel.Id;
                            items.Add(new Item { Id = PastExperienceModelId, Name = "PastExperienceModel" });
                        }
                        if (query2.PersonnelModel != null)
                        {
                            int PersonnelModelId = query2.PersonnelModel.Id;
                            items.Add(new Item { Id = PersonnelModelId, Name = "PersonnelModel" });
                        }
                        if (query2.QualificationModel != null)
                        {
                            int QualificationModelId = query2.QualificationModel.Id;
                            items.Add(new Item { Id = QualificationModelId, Name = "QualificationModel" });
                        }
                        if (query2.ServiceModel != null)
                        {
                            int ServiceModelId = query2.ServiceModel.Id;
                            items.Add(new Item { Id = ServiceModelId, Name = "ServiceModel" });
                        }
                        if (query2.SkillModel != null)
                        {
                            int SkillModelId = query2.SkillModel.Id;
                            items.Add(new Item { Id = SkillModelId, Name = "SkillModel" });
                        }
                        if (query2.TrainingModel != null)
                        {
                            int TrainingModelId = query2.TrainingModel.Id;
                            items.Add(new Item { Id = TrainingModelId, Name = "TrainingModel" });
                        }

                        SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                        return Json(s, JsonRequestBehavior.AllowGet);
                    }
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
        }

        public class returndatagridDataclass //Parentgrid
        {
            public string Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
        }

        public ActionResult CompetencyEmployeeDataT_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.CompetencyEmployeeDataT.Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .ToList();

                    IEnumerable<CompetencyEmployeeDataT> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {

                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.BatchName.BatchName.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                  || (e.BatchName.BatchDescription.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<CompetencyEmployeeDataT, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.BatchName.BatchName : "");
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
                                EmpCode = item.Employee.EmpCode,
                                EmpName = item.Employee.EmpName.FullNameFML,
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

                                     select new[] { null, Convert.ToString(c.Id), c.BatchName.BatchName, };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }//for data reterivation
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
        public ActionResult GetEvaluationObjectList(List<int> SkipIds, string Apprlisttext, string Apprlistval)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                switch (Apprlisttext)
                {

                    case "AppraisalAttributeModel":
                        int AppAId = Convert.ToInt32(Apprlistval);
                        var qurey11 = db.CompetencyModel.Include(e => e.AppraisalAttributeModel)
                            .Include(e => e.AppraisalAttributeModel.AppraisalAttributeModelObject)
                            .Include(e => e.AppraisalAttributeModel.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel))
                            .Include(e => e.AppraisalAttributeModel.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.AppraisalAttributeModel.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.AppraisalAttributeModel.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                            .Where(e => e.AppraisalAttributeModel.Id == AppAId).FirstOrDefault();
                        var query12 = qurey11.AppraisalAttributeModel.AppraisalAttributeModelObject.ToList();
                        var query13 = query12.Select(r => r.CompetencyEvaluationModel).ToList();
                        var r1 = (from ca in query13 select new { srno = ca.Id, lookupvalue = ca.Criteria.LookupVal + " " + ca.DataSteps.LookupVal + " " + ca.CriteriaType.LookupVal }).Distinct();
                        return Json(r1, JsonRequestBehavior.AllowGet);
                        break;

                    case "AppraisalBusinessAppraisalModel":
                        int AppBId = Convert.ToInt32(Apprlistval);
                        var qurey21 = db.CompetencyModel.Include(e => e.AppraisalBusinessApprisalModel)
                            .Include(e => e.AppraisalBusinessApprisalModel.AppraisalBusinessAppraisalModelObject)
                            .Include(e => e.AppraisalBusinessApprisalModel.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel))
                            .Include(e => e.AppraisalBusinessApprisalModel.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.AppraisalBusinessApprisalModel.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.AppraisalBusinessApprisalModel.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                            .Where(e => e.AppraisalBusinessApprisalModel.Id == AppBId).FirstOrDefault();
                        var query22 = qurey21.AppraisalBusinessApprisalModel.AppraisalBusinessAppraisalModelObject.ToList();
                        var query23 = query22.Select(r => r.CompetencyEvaluationModel).ToList();
                        var r2 = (from ca in query23 select new { srno = ca.Id, lookupvalue = ca.Criteria.LookupVal + " " + ca.DataSteps.LookupVal + " " + ca.CriteriaType.LookupVal }).Distinct();
                        return Json(r2, JsonRequestBehavior.AllowGet);
                        break;
                    case "AppraisalKRAModel":
                        int AppCId = Convert.ToInt32(Apprlistval);
                        var qurey31 = db.CompetencyModel.Include(e => e.AppraisalKRAModel)
                            .Include(e => e.AppraisalKRAModel.AppraisalKRAModelObject)
                            .Include(e => e.AppraisalKRAModel.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel))
                            .Include(e => e.AppraisalKRAModel.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.AppraisalKRAModel.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.AppraisalKRAModel.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                            .Where(e => e.AppraisalKRAModel.Id == AppCId).FirstOrDefault();
                        var query32 = qurey31.AppraisalKRAModel.AppraisalKRAModelObject.ToList();
                        var query33 = query32.Select(r => r.CompetencyEvaluationModel).ToList();
                        var r3 = (from ca in query33 select new { srno = ca.Id, lookupvalue = ca.Criteria.LookupVal + " " + ca.DataSteps.LookupVal + " " + ca.CriteriaType.LookupVal }).Distinct();
                        return Json(r3, JsonRequestBehavior.AllowGet);
                        break;
                }
                return null;


            }

        }

        public static void DeleteEmpGenData(int OCompEmpData, string UserId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (OCompEmpData != 0)
                {
                    var CompEmpDataGen = new CompetencyEmployeeDataGeneration();
                    CompEmpDataGen = db.CompetencyEmployeeDataGeneration.Where(e => e.Id == OCompEmpData && e.DBTrack.CreatedBy == UserId).FirstOrDefault();
                    List<AppraisalAttributeModelObjectV> AppraisalAttributeModelObjectV = db.CompetencyEmployeeDataGeneration.Where(e => e.Id == OCompEmpData).Select(r => r.AppraisalAttributeModelObjectV.ToList()).FirstOrDefault();
                    List<AppraisalBusinessAppraisalModelObjectV> AppraisalBusinessAppraisalModelObjectV = db.CompetencyEmployeeDataGeneration.Where(e => e.Id == OCompEmpData).Select(r => r.AppraisalBusinessAppraisalModelObjectV.ToList()).FirstOrDefault();
                    List<AppraisalKRAModelObjectV> AppraisalKRAModelObjectV = db.CompetencyEmployeeDataGeneration.Where(e => e.Id == OCompEmpData).Select(r => r.AppraisalKRAModelObjectV.ToList()).FirstOrDefault();
                    List<AppraisalPotentialModelObjectV> AppraisalPotentialModelObjectV = db.CompetencyEmployeeDataGeneration.Where(e => e.Id == OCompEmpData).Select(r => r.AppraisalPotentialModelObjectV.ToList()).FirstOrDefault();
                    List<PastExperienceModelObjectV> PastExperienceModelObjectV = db.CompetencyEmployeeDataGeneration.Where(e => e.Id == OCompEmpData).Select(r => r.PastExperienceModelObjectV.ToList()).FirstOrDefault();
                    List<PersonnelModelObjectV> PersonnelModelObjectV = db.CompetencyEmployeeDataGeneration.Where(e => e.Id == OCompEmpData).Select(r => r.PersonnelModelObjectV.ToList()).FirstOrDefault();
                    List<QualificationModelObjectV> QualificationModelObjectV = db.CompetencyEmployeeDataGeneration.Where(e => e.Id == OCompEmpData).Select(r => r.QualificationModelObjectV.ToList()).FirstOrDefault();
                    List<ServiceModelObjectV> ServiceModelObjectV = db.CompetencyEmployeeDataGeneration.Where(e => e.Id == OCompEmpData).Select(r => r.ServiceModelObjectV.ToList()).FirstOrDefault();
                    List<SkillModelObjectV> SkillModelObjectV = db.CompetencyEmployeeDataGeneration.Where(e => e.Id == OCompEmpData).Select(r => r.SkillModelObjectV.ToList()).FirstOrDefault();
                    List<TrainingModelObjectV> TrainingModelObjectV = db.CompetencyEmployeeDataGeneration.Where(e => e.Id == OCompEmpData).Select(r => r.TrainingModelObjectV.ToList()).FirstOrDefault();

                    foreach (var j in AppraisalAttributeModelObjectV.ToList())
                    {
                        var ObjectValue = db.AppraisalAttributeModelObjectV.Where(e => e.Id == j.Id).Select(r => r.ObjectValue).FirstOrDefault();
                        j.ObjectValue = ObjectValue;

                        if (j.ObjectValue != null)
                        { db.ObjectValue.RemoveRange(j.ObjectValue); }

                        db.AppraisalAttributeModelObjectV.Remove(j);
                    }

                    foreach (var j in AppraisalBusinessAppraisalModelObjectV.ToList())
                    {
                        var ObjectValue = db.AppraisalBusinessAppraisalModelObjectV.Where(e => e.Id == j.Id).Select(r => r.ObjectValue).FirstOrDefault();
                        j.ObjectValue = ObjectValue;

                        if (j.ObjectValue != null)
                        { db.ObjectValue.RemoveRange(j.ObjectValue); }

                        db.AppraisalBusinessAppraisalModelObjectV.Remove(j);
                    }

                    foreach (var j in AppraisalKRAModelObjectV.ToList())
                    {
                        var ObjectValue = db.AppraisalKRAModelObjectV.Where(e => e.Id == j.Id).Select(r => r.ObjectValue).FirstOrDefault();
                        j.ObjectValue = ObjectValue;

                        if (j.ObjectValue != null)
                        { db.ObjectValue.RemoveRange(j.ObjectValue); }

                        db.AppraisalKRAModelObjectV.Remove(j);
                    }

                    foreach (var j in AppraisalPotentialModelObjectV.ToList())
                    {
                        var ObjectValue = db.AppraisalPotentialModelObjectV.Where(e => e.Id == j.Id).Select(r => r.ObjectValue).FirstOrDefault();
                        j.ObjectValue = ObjectValue;

                        if (j.ObjectValue != null)
                        { db.ObjectValue.RemoveRange(j.ObjectValue); }

                        db.AppraisalPotentialModelObjectV.Remove(j);
                    }

                    foreach (var j in PastExperienceModelObjectV.ToList())
                    {
                        var ObjectValue = db.PastExperienceModelObjectV.Where(e => e.Id == j.Id).Select(r => r.ObjectValue).FirstOrDefault();
                        j.ObjectValue = ObjectValue;

                        if (j.ObjectValue != null)
                        { db.ObjectValue.RemoveRange(j.ObjectValue); }

                        db.PastExperienceModelObjectV.Remove(j);
                    }

                    foreach (var j in PersonnelModelObjectV.ToList())
                    {
                        var ObjectValue = db.PersonnelModelObjectV.Where(e => e.Id == j.Id).Select(r => r.ObjectValue).FirstOrDefault();
                        j.ObjectValue = ObjectValue;

                        if (j.ObjectValue != null)
                        { db.ObjectValue.RemoveRange(j.ObjectValue); }

                        db.PersonnelModelObjectV.Remove(j);
                    }

                    foreach (var j in QualificationModelObjectV.ToList())
                    {
                        var ObjectValue = db.QualificationModelObjectV.Where(e => e.Id == j.Id).Select(r => r.ObjectValue).FirstOrDefault();
                        j.ObjectValue = ObjectValue;

                        if (j.ObjectValue != null)
                        { db.ObjectValue.RemoveRange(j.ObjectValue); }

                        db.QualificationModelObjectV.Remove(j);
                    }

                    foreach (var j in ServiceModelObjectV.ToList())
                    {
                        var ObjectValue = db.ServiceModelObjectV.Where(e => e.Id == j.Id).Select(r => r.ObjectValue).FirstOrDefault();
                        j.ObjectValue = ObjectValue;

                        if (j.ObjectValue != null)
                        { db.ObjectValue.RemoveRange(j.ObjectValue); }

                        db.ServiceModelObjectV.Remove(j);
                    }

                    foreach (var j in SkillModelObjectV.ToList())
                    {
                        var ObjectValue = db.SkillModelObjectV.Where(e => e.Id == j.Id).Select(r => r.ObjectValue).FirstOrDefault();
                        j.ObjectValue = ObjectValue;

                        if (j.ObjectValue != null)
                        { db.ObjectValue.RemoveRange(j.ObjectValue); }

                        db.SkillModelObjectV.Remove(j);
                    }

                    foreach (var j in TrainingModelObjectV)
                    {
                        var ObjectValue = db.TrainingModelObjectV.Where(e => e.Id == j.Id).Select(r => r.ObjectValue).FirstOrDefault();
                        j.ObjectValue = ObjectValue;

                        if (j.ObjectValue != null)
                        { db.ObjectValue.RemoveRange(j.ObjectValue); }
                    }

                    db.CompetencyEmployeeDataGeneration.Remove(CompEmpDataGen);
                    db.SaveChanges();
                }
            }
        }

        public static void DeleteCompData(int OCompEmpData, string UserId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (OCompEmpData != 0)
                {

                    var CompEmpDataGen = db.CompetencyEmployeeDataT.Where(e => e.Id == OCompEmpData && e.DBTrack.CreatedBy == UserId).FirstOrDefault();
                    List<AppraisalAttributeModelObjectT> AppraisalAttributeModelObjectT = db.CompetencyEmployeeDataT.Where(e => e.Id == OCompEmpData).Select(r => r.AppraisalAttributeModelObjectT.ToList()).FirstOrDefault();
                    List<AppraisalBusinessAppraisalModelObjectT> AppraisalBusinessAppraisalModelObjectT = db.CompetencyEmployeeDataT.Where(e => e.Id == OCompEmpData).Select(r => r.AppraisalBusinessAppraisalModelObjectT.ToList()).FirstOrDefault();
                    List<AppraisalKRAModelObjectT> AppraisalKRAModelObjectT = db.CompetencyEmployeeDataT.Where(e => e.Id == OCompEmpData).Select(r => r.AppraisalKRAModelObjectT.ToList()).FirstOrDefault();
                    List<AppraisalPotentialModelObjectT> AppraisalPotentialModelObjectT = db.CompetencyEmployeeDataT.Where(e => e.Id == OCompEmpData).Select(r => r.AppraisalPotentialModelObjectT.ToList()).FirstOrDefault();
                    List<PastExperienceModelObjectT> PastExperienceModelObjectT = db.CompetencyEmployeeDataT.Where(e => e.Id == OCompEmpData).Select(r => r.PastExperienceModelObjectT.ToList()).FirstOrDefault();
                    List<PersonnelModelObjectT> PersonnelModelObjectT = db.CompetencyEmployeeDataT.Where(e => e.Id == OCompEmpData).Select(r => r.PersonnelModelObjectT.ToList()).FirstOrDefault();
                    List<QualificationModelObjectT> QualificationModelObjectT = db.CompetencyEmployeeDataT.Where(e => e.Id == OCompEmpData).Select(r => r.QualificationModelObjectT.ToList()).FirstOrDefault();
                    List<ServiceModelObjectT> ServiceModelObjectT = db.CompetencyEmployeeDataT.Where(e => e.Id == OCompEmpData).Select(r => r.ServiceModelObjectT.ToList()).FirstOrDefault();
                    List<SkillModelObjectT> SkillModelObjectT = db.CompetencyEmployeeDataT.Where(e => e.Id == OCompEmpData).Select(r => r.SkillModelObjectT.ToList()).FirstOrDefault();
                    List<TrainingModelObjectT> TrainingModelObjectT = db.CompetencyEmployeeDataT.Where(e => e.Id == OCompEmpData).Select(r => r.TrainingModelObjectT.ToList()).FirstOrDefault();

                    var db_data = db.CompetencyEmployeeDataT.Where(e => e.Id == OCompEmpData && e.DBTrack.CreatedBy == UserId).FirstOrDefault();

                    foreach (var j in AppraisalAttributeModelObjectT.ToList())
                    {
                        var AppAttObjT = db.AppraisalAttributeModelObjectT.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                        if (AppAttObjT.ObjectValue != null)
                        {
                            AppAttObjT.ObjectValue = null;
                            db.AppraisalAttributeModelObjectT.Attach(AppAttObjT);
                            db.Entry(AppAttObjT).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        db.AppraisalAttributeModelObjectT.Remove(j);
                    }

                    foreach (var j in AppraisalBusinessAppraisalModelObjectT.ToList())
                    {
                        var AppBusiObjT = db.AppraisalBusinessAppraisalModelObjectT.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                        if (AppBusiObjT.ObjectValue != null)
                        {
                            AppBusiObjT.ObjectValue = null;
                            db.AppraisalBusinessAppraisalModelObjectT.Attach(AppBusiObjT);
                            db.Entry(AppBusiObjT).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        db.AppraisalBusinessAppraisalModelObjectT.Remove(j);
                    }

                    foreach (var j in AppraisalPotentialModelObjectT.ToList())
                    {
                        var AppPotObjT = db.AppraisalPotentialModelObjectT.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                        if (AppPotObjT.ObjectValue != null)
                        {
                            AppPotObjT.ObjectValue = null;
                            db.AppraisalPotentialModelObjectT.Attach(AppPotObjT);
                            db.Entry(AppPotObjT).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        db.AppraisalPotentialModelObjectT.Remove(j);
                    }

                    foreach (var j in AppraisalKRAModelObjectT.ToList())
                    {
                        var AppKRAObjT = db.AppraisalKRAModelObjectT.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                        if (AppKRAObjT.ObjectValue != null)
                        {
                            AppKRAObjT.ObjectValue = null;
                            db.AppraisalKRAModelObjectT.Attach(AppKRAObjT);
                            db.Entry(AppKRAObjT).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        db.AppraisalKRAModelObjectT.Remove(j);
                    }

                    foreach (var j in PastExperienceModelObjectT.ToList())
                    {
                        var PastExpObjT = db.PastExperienceModelObjectT.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                        if (PastExpObjT.ObjectValue != null)
                        {
                            PastExpObjT.ObjectValue = null;
                            db.PastExperienceModelObjectT.Attach(PastExpObjT);
                            db.Entry(PastExpObjT).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        db.PastExperienceModelObjectT.Remove(j);
                    }

                    foreach (var j in PersonnelModelObjectT.ToList())
                    {
                        var PersModelObjT = db.PersonnelModelObjectT.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                        if (PersModelObjT.ObjectValue != null)
                        {
                            PersModelObjT.ObjectValue = null;
                            db.PersonnelModelObjectT.Attach(PersModelObjT);
                            db.Entry(PersModelObjT).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        db.PersonnelModelObjectT.Remove(j);
                    }

                    foreach (var j in ServiceModelObjectT.ToList())
                    {
                        var ServModelObjT = db.ServiceModelObjectT.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                        if (ServModelObjT.ObjectValue != null)
                        {
                            ServModelObjT.ObjectValue = null;
                            db.ServiceModelObjectT.Attach(ServModelObjT);
                            db.Entry(ServModelObjT).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        db.ServiceModelObjectT.Remove(j);
                    }

                    foreach (var j in QualificationModelObjectT.ToList())
                    {
                        var QualModelObjT = db.QualificationModelObjectT.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                        if (QualModelObjT.ObjectValue != null)
                        {
                            QualModelObjT.ObjectValue = null;
                            db.QualificationModelObjectT.Attach(QualModelObjT);
                            db.Entry(QualificationModelObjectT).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        db.QualificationModelObjectT.Remove(j);
                    }

                    foreach (var j in TrainingModelObjectT.ToList())
                    {
                        var TrnModelObjT = db.TrainingModelObjectT.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                        if (TrnModelObjT.ObjectValue != null)
                        {
                            TrnModelObjT.ObjectValue = null;
                            db.TrainingModelObjectT.Attach(TrnModelObjT);
                            db.Entry(TrnModelObjT).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        db.TrainingModelObjectT.Remove(j);
                    }

                    foreach (var j in SkillModelObjectT.ToList())
                    {
                        var SkillModelObjT = db.SkillModelObjectT.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();


                        if (SkillModelObjT.ObjectValue != null)
                        {
                            SkillModelObjT.ObjectValue = null;
                            db.SkillModelObjectT.Attach(SkillModelObjT);
                            db.Entry(SkillModelObjT).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }

                        db.SkillModelObjectT.Remove(j);
                    }
                   
                  

                    db.CompetencyEmployeeDataT.Remove(CompEmpDataGen);
                    db.SaveChanges();
                }
            }
        }

        public static void DeleteBatchData(int OCompBatchData, string UserId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CompBatchDataT = db.CompetencyBatchProcessT.Where(e => e.Id == OCompBatchData && e.DBTrack.CreatedBy == UserId).FirstOrDefault();

                if (CompBatchDataT != null)
                {
                    CompBatchDataT.AppraisalAttributeModelObjectF = db.CompetencyBatchProcessT.Where(e => e.Id == OCompBatchData).Select(e => e.AppraisalAttributeModelObjectF).FirstOrDefault();
                    CompBatchDataT.AppraisalBusinessAppraisalModelObjectF = db.CompetencyBatchProcessT.Where(e => e.Id == OCompBatchData).Select(e => e.AppraisalBusinessAppraisalModelObjectF).FirstOrDefault();
                    CompBatchDataT.AppraisalKRAModelObjectF = db.CompetencyBatchProcessT.Where(e => e.Id == OCompBatchData).Select(e => e.AppraisalKRAModelObjectF).FirstOrDefault();
                    CompBatchDataT.AppraisalPotentialModelObjectF = db.CompetencyBatchProcessT.Where(e => e.Id == OCompBatchData).Select(e => e.AppraisalPotentialModelObjectF).FirstOrDefault();
                    CompBatchDataT.PastExperienceModelObjectF = db.CompetencyBatchProcessT.Where(e => e.Id == OCompBatchData).Select(e => e.PastExperienceModelObjectF).FirstOrDefault();
                    CompBatchDataT.PersonnelModelObjectF = db.CompetencyBatchProcessT.Where(e => e.Id == OCompBatchData).Select(e => e.PersonnelModelObjectF).FirstOrDefault();
                    CompBatchDataT.QualificationModelObjectF = db.CompetencyBatchProcessT.Where(e => e.Id == OCompBatchData).Select(e => e.QualificationModelObjectF).FirstOrDefault();
                    CompBatchDataT.ServiceModelObjectF = db.CompetencyBatchProcessT.Where(e => e.Id == OCompBatchData).Select(e => e.ServiceModelObjectF).FirstOrDefault();
                    CompBatchDataT.SkillModelObjectF = db.CompetencyBatchProcessT.Where(e => e.Id == OCompBatchData).Select(e => e.SkillModelObjectF).FirstOrDefault();
                    CompBatchDataT.TrainingModelObjectF = db.CompetencyBatchProcessT.Where(e => e.Id == OCompBatchData).Select(e => e.TrainingModelObjectF).FirstOrDefault();

                    if (CompBatchDataT.AppraisalAttributeModelObjectF != null)
                    {
                        foreach (var j in CompBatchDataT.AppraisalAttributeModelObjectF.ToList())
                        {
                            var AppAttObjValF = db.AppraisalAttributeModelObjectF.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                            if (AppAttObjValF.ObjectValue != null)
                            {
                                AppAttObjValF.ObjectValue = null;
                                db.AppraisalAttributeModelObjectF.Attach(AppAttObjValF);
                                db.Entry(AppAttObjValF).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            db.AppraisalAttributeModelObjectF.Remove(j);
                        }

                    }

                    if (CompBatchDataT.AppraisalBusinessAppraisalModelObjectF != null)
                    {
                        foreach (var j in CompBatchDataT.AppraisalBusinessAppraisalModelObjectF.ToList())
                        {
                            var  AppBusiObjValF = db.AppraisalBusinessAppraisalModelObjectF.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                            if (AppBusiObjValF.ObjectValue != null)
                            {
                                AppBusiObjValF.ObjectValue = null;
                                db.AppraisalBusinessAppraisalModelObjectF.Attach(AppBusiObjValF);
                                db.Entry(AppBusiObjValF).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            db.AppraisalBusinessAppraisalModelObjectF.Remove(j);
                        }

                    }

                    if (CompBatchDataT.AppraisalKRAModelObjectF != null)
                    {
                        foreach (var j in CompBatchDataT.AppraisalKRAModelObjectF.ToList())
                        {
                            var AppKRAObjValF = db.AppraisalKRAModelObjectF.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                            if (AppKRAObjValF.ObjectValue != null)
                            {
                                AppKRAObjValF.ObjectValue = null;
                                db.AppraisalKRAModelObjectF.Attach(AppKRAObjValF);
                                db.Entry(AppKRAObjValF).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            db.AppraisalKRAModelObjectF.Remove(j);
                        }

                    }

                    if (CompBatchDataT.AppraisalPotentialModelObjectF != null)
                    {
                        foreach (var j in CompBatchDataT.AppraisalPotentialModelObjectF.ToList())
                        {
                            var AppPotObjValF = db.AppraisalPotentialModelObjectF.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                            if (AppPotObjValF.ObjectValue != null)
                            {
                                AppPotObjValF.ObjectValue = null;
                                db.AppraisalPotentialModelObjectF.Attach(AppPotObjValF);
                                db.Entry(AppPotObjValF).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            db.AppraisalPotentialModelObjectF.Remove(j);
                        }

                    }

                    if (CompBatchDataT.PastExperienceModelObjectF != null)
                    {
                        foreach (var j in CompBatchDataT.PastExperienceModelObjectF.ToList())
                        {
                            var PastExpObjValF = db.PastExperienceModelObjectF.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                            if (PastExpObjValF.ObjectValue != null)
                            {
                                PastExpObjValF.ObjectValue = null;
                                db.PastExperienceModelObjectF.Attach(PastExpObjValF);
                                db.Entry(PastExpObjValF).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            db.PastExperienceModelObjectF.Remove(j);
                        }

                    }

                    if (CompBatchDataT.PersonnelModelObjectF != null)
                    {
                        foreach (var j in CompBatchDataT.PersonnelModelObjectF.ToList())
                        {
                            var PersObjValF = db.PersonnelModelObjectF.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                            if (PersObjValF.ObjectValue != null)
                            {
                                PersObjValF.ObjectValue = null;
                                db.PersonnelModelObjectF.Attach(PersObjValF);
                                db.Entry(PersObjValF).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            db.PersonnelModelObjectF.Remove(j);
                        }

                    }

                    if (CompBatchDataT.QualificationModelObjectF != null)
                    {
                        foreach (var j in CompBatchDataT.QualificationModelObjectF.ToList())
                        {
                            var QualObjValF = db.QualificationModelObjectF.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                            if (QualObjValF.ObjectValue != null)
                            {
                                QualObjValF.ObjectValue = null;
                                db.QualificationModelObjectF.Attach(QualObjValF);
                                db.Entry(QualObjValF).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            db.QualificationModelObjectF.Remove(j);
                        }

                    }

                    if (CompBatchDataT.ServiceModelObjectF != null)
                    {
                        foreach (var j in CompBatchDataT.ServiceModelObjectF.ToList())
                        {
                            var ServObjValF = db.ServiceModelObjectF.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                            if (ServObjValF.ObjectValue != null)
                            {
                                ServObjValF.ObjectValue = null;
                                db.ServiceModelObjectF.Attach(ServObjValF);
                                db.Entry(ServObjValF).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            db.ServiceModelObjectF.Remove(j);
                        }

                    }

                    if (CompBatchDataT.SkillModelObjectF != null)
                    {
                        foreach (var j in CompBatchDataT.SkillModelObjectF.ToList())
                        {
                            var SkillObjValF = db.SkillModelObjectF.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                            if (SkillObjValF.ObjectValue != null)
                            {
                                SkillObjValF.ObjectValue = null;
                                db.SkillModelObjectF.Attach(SkillObjValF);
                                db.Entry(SkillObjValF).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            db.SkillModelObjectF.Remove(j);
                        }

                    }

                    if (CompBatchDataT.TrainingModelObjectF != null)
                    {
                        foreach (var j in CompBatchDataT.TrainingModelObjectF.ToList())
                            {
                                var TrObjValF = db.TrainingModelObjectF.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                                if (TrObjValF.ObjectValue != null)
                                {
                                    TrObjValF.ObjectValue = null;
                                    db.TrainingModelObjectF.Attach(TrObjValF);
                                    db.Entry(TrObjValF).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                                db.TrainingModelObjectF.Remove(j);
                            }
                        
                    }

                    var CompEmpDataT = db.CompetencyBatchProcessT.Where(e => e.Id == OCompBatchData && e.DBTrack.CreatedBy == UserId).Select(r => r.CompetencyEmployeeDataT.ToList()).FirstOrDefault();

                    if (CompEmpDataT != null)
                    {

                        foreach (var item in CompEmpDataT)
                        {
                            item.AppraisalAttributeModelObjectT = db.CompetencyEmployeeDataT.Where(e => e.Id == item.Id).Select(e => e.AppraisalAttributeModelObjectT).FirstOrDefault();
                            item.AppraisalBusinessAppraisalModelObjectT = db.CompetencyEmployeeDataT.Where(e => e.Id == item.Id).Select(e => e.AppraisalBusinessAppraisalModelObjectT).FirstOrDefault();
                            item.AppraisalKRAModelObjectT = db.CompetencyEmployeeDataT.Where(e => e.Id == item.Id).Select(e => e.AppraisalKRAModelObjectT).FirstOrDefault();
                            item.AppraisalPotentialModelObjectT = db.CompetencyEmployeeDataT.Where(e => e.Id == item.Id).Select(e => e.AppraisalPotentialModelObjectT).FirstOrDefault();
                            item.PastExperienceModelObjectT = db.CompetencyEmployeeDataT.Where(e => e.Id == item.Id).Select(e => e.PastExperienceModelObjectT).FirstOrDefault();
                            item.PersonnelModelObjectT = db.CompetencyEmployeeDataT.Where(e => e.Id == item.Id).Select(e => e.PersonnelModelObjectT).FirstOrDefault();
                            item.QualificationModelObjectT = db.CompetencyEmployeeDataT.Where(e => e.Id == item.Id).Select(e => e.QualificationModelObjectT).FirstOrDefault();
                            item.ServiceModelObjectT = db.CompetencyEmployeeDataT.Where(e => e.Id == item.Id).Select(e => e.ServiceModelObjectT).FirstOrDefault();
                            item.SkillModelObjectT = db.CompetencyEmployeeDataT.Where(e => e.Id == item.Id).Select(e => e.SkillModelObjectT).FirstOrDefault();
                            item.TrainingModelObjectT = db.CompetencyEmployeeDataT.Where(e => e.Id == item.Id).Select(e => e.TrainingModelObjectT).FirstOrDefault();

                            #region AppraisalAttributeModelObjectT
                           
                            if (item.AppraisalAttributeModelObjectT != null)
                            {
                                foreach (var j in item.AppraisalAttributeModelObjectT.ToList())
                                {
                                    var AppAttObjValT = db.AppraisalAttributeModelObjectT.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                                    if (AppAttObjValT.ObjectValue != null)
                                    {
                                        AppAttObjValT.ObjectValue = null;
                                        db.AppraisalAttributeModelObjectT.Attach(AppAttObjValT);
                                        db.Entry(AppAttObjValT).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                    db.AppraisalAttributeModelObjectT.Remove(j);
                                }
                                
                            }
                            #endregion

                            #region AppraisalBusinessAppraisalModelObjectT
                            
                            if (item.AppraisalBusinessAppraisalModelObjectT != null)
                            {

                                foreach (var j in item.AppraisalBusinessAppraisalModelObjectT.ToList())
                                {
                                    var AppBusiObjValT = db.AppraisalBusinessAppraisalModelObjectT.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                                    if (AppBusiObjValT.ObjectValue != null)
                                    {
                                        AppBusiObjValT.ObjectValue = null;
                                        db.AppraisalBusinessAppraisalModelObjectT.Attach(AppBusiObjValT);
                                        db.Entry(AppBusiObjValT).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                    db.AppraisalBusinessAppraisalModelObjectT.Remove(j);
                                }
                            }
                            #endregion

                            #region AppraisalKRAModelObjectT
                            
                            if (item.AppraisalKRAModelObjectT != null)
                            {

                                foreach (var j in item.AppraisalKRAModelObjectT.ToList())
                                {
                                    var AppKRAObjValT = db.AppraisalKRAModelObjectT.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                                    if (AppKRAObjValT.ObjectValue != null)
                                    {
                                        AppKRAObjValT.ObjectValue = null;
                                        db.AppraisalKRAModelObjectT.Attach(AppKRAObjValT);
                                        db.Entry(AppKRAObjValT).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                    db.AppraisalKRAModelObjectT.Remove(j);
                                }
                                
                            }
                            #endregion

                            #region AppraisalPotentialModelObjectT
                            
                            if (item.AppraisalPotentialModelObjectT != null)
                            {
                                
                                    foreach (var j in item.AppraisalPotentialModelObjectT.ToList())
                                    {
                                        var AppPotObjValT = db.AppraisalPotentialModelObjectT.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                                        if (AppPotObjValT.ObjectValue != null)
                                        {
                                            AppPotObjValT.ObjectValue = null;
                                            db.AppraisalPotentialModelObjectT.Attach(AppPotObjValT);
                                            db.Entry(AppPotObjValT).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                        db.AppraisalPotentialModelObjectT.Remove(j);
                                    }
                                
                            }
                            #endregion

                            #region PastExperienceModelObjectT
                           
                            if (item.PastExperienceModelObjectT != null)
                            {
                                
                                    foreach (var j in item.PastExperienceModelObjectT.ToList())
                                    {
                                        var PastExpObjValT = db.PastExperienceModelObjectT.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                                        if (PastExpObjValT.ObjectValue != null)
                                        {
                                            PastExpObjValT.ObjectValue = null;
                                            db.PastExperienceModelObjectT.Attach(PastExpObjValT);
                                            db.Entry(PastExpObjValT).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                        db.PastExperienceModelObjectT.Remove(j);
                                    }
                                
                            }
                            #endregion

                            #region PersonnelModelObjectT
                            
                            if (item.PersonnelModelObjectT != null)
                            {
                                
                                    foreach (var j in item.PersonnelModelObjectT.ToList())
                                    {
                                        var PersObjValT = db.PersonnelModelObjectT.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                                        if (PersObjValT.ObjectValue != null)
                                        {
                                            PersObjValT.ObjectValue = null;
                                            db.PersonnelModelObjectT.Attach(PersObjValT);
                                            db.Entry(PersObjValT).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                        db.PersonnelModelObjectT.Remove(j);
                                    }
                            }
                            #endregion

                            #region QualificationModelObjectT
                           
                            if (item.QualificationModelObjectT != null)
                            {
                                
                                    foreach (var j in item.QualificationModelObjectT.ToList())
                                    {
                                        var QualObjValT = db.QualificationModelObjectT.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                                        if (QualObjValT.ObjectValue != null)
                                        {
                                            QualObjValT.ObjectValue = null;
                                            db.QualificationModelObjectT.Attach(QualObjValT);
                                            db.Entry(QualObjValT).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                        db.QualificationModelObjectT.Remove(j);
                                    }
                                
                            }
                            #endregion

                            #region ServiceModelObjectT
                            
                            if (item.ServiceModelObjectT != null)
                            {
                               
                                    foreach (var j in item.ServiceModelObjectT.ToList())
                                    {
                                        var ServObjValT = db.ServiceModelObjectT.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                                        if (ServObjValT.ObjectValue != null)
                                        {
                                            ServObjValT.ObjectValue = null;
                                            db.ServiceModelObjectT.Attach(ServObjValT);
                                            db.Entry(ServObjValT).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                        db.ServiceModelObjectT.Remove(j);
                                    }
                             
                            }
                            #endregion

                            #region SkillModelObjectT
                             
                            if (item.SkillModelObjectT != null)
                            {
                               
                                    foreach (var j in item.SkillModelObjectT.ToList())
                                    {
                                        var SkillObjValT = db.SkillModelObjectT.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                                        if (SkillObjValT.ObjectValue != null)
                                        {
                                            SkillObjValT.ObjectValue = null;
                                            db.SkillModelObjectT.Attach(SkillObjValT);
                                            db.Entry(SkillObjValT).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                        db.SkillModelObjectT.Remove(j);
                                    }
                                
                            }
                            #endregion

                            #region QualificationModelObjectT
                           
                            if (item.QualificationModelObjectT != null)
                            {
                                
                                    foreach (var j in item.QualificationModelObjectT.ToList())
                                    {
                                        var QualObjValT = db.QualificationModelObjectT.Where(e => e.Id == j.Id).Include(r => r.ObjectValue).FirstOrDefault();

                                        if (QualObjValT.ObjectValue != null)
                                        {
                                            QualObjValT.ObjectValue = null;
                                            db.QualificationModelObjectT.Attach(QualObjValT);
                                            db.Entry(QualObjValT).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                        db.QualificationModelObjectT.Remove(j);
                                    }
                                
                            }
                            #endregion

                            db.CompetencyEmployeeDataT.Remove(item);
                        }
                    }
                    db.CompetencyBatchProcessT.Remove(CompBatchDataT);

                    db.SaveChanges();
                }
            }
        }

        public ActionResult EmployeedataGeneration(string BatchId, string ModelSubObject)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                {
                    List<string> Msg = new List<string>();
                    
                    List<CompetencyEmployeeDataGeneration> CompetencyEmployeeDataGenerationList = new List<CompetencyEmployeeDataGeneration>();

                    int BatchIds = Convert.ToInt32(BatchId);
                    List<int> chk = db.CompetencyEmployeeDataGeneration.Include(e => e.BatchName)
                        .Where(e => e.BatchName.Id == BatchIds && e.DBTrack.CreatedBy == SessionManager.EmpId).Select(t => t.Id).ToList();
                    if (chk.Count() > 0)
                    {
                        foreach (var item in chk)
                        {
                            DeleteEmpGenData(item, SessionManager.EmpId);
                        }
                    }
                    // int ModelSubObjId = Convert.ToInt32(ModelSubObject);
                    //var vidsf = db.CompetencyModelAssignment.Include(e => e.CompetencyModelAssignment_OrgStructure).
                    //Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.GeoStruct)).
                    //Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.PayStruct)).
                    //Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.FuncStruct))
                    //.Include(e => e.CompetencyModel).Include(e => e.CompetencyModel.AppraisalAttributeModel)
                    //.Include(e => e.CompetencyModel.AppraisalBusinessApprisalModel)
                    //.Include(e => e.CompetencyModel.AppraisalKRAModel)
                    //.Include(e => e.CompetencyModel.AppraisalPotentialModel)
                    //.Include(e => e.CompetencyModel.PastExperienceModel)
                    //.Include(e => e.CompetencyModel.PersonnelModel)
                    //.Include(e => e.CompetencyModel.QualificationModel)
                    //.Include(e => e.CompetencyModel.ServiceModel)
                    //.Include(e => e.CompetencyModel.SkillModel)
                    //.Include(e => e.CompetencyModel.TrainingModel)
                    //.Where(e => e.Id == BatchIds).FirstOrDefault();

                    var vidsf = db.CompetencyModelAssignment.Find(BatchIds);
                    List<CompetencyModelAssignment_OrgStructure> CompOrgStruct = db.CompetencyModelAssignment.Where(e => e.Id == BatchIds).Select(t => t.CompetencyModelAssignment_OrgStructure.ToList()).FirstOrDefault();
                    vidsf.CompetencyModelAssignment_OrgStructure = CompOrgStruct;
                    vidsf.CompetencyModel = db.CompetencyModel.Find(vidsf.CompetencyModel_Id);

                    List<AppraisalAttributeModelObjectV> AppraisalAttModelObjectVlist = new List<AppraisalAttributeModelObjectV>();
                    List<AppraisalBusinessAppraisalModelObjectV> AppraisalBusiModelObjectVlist = new List<AppraisalBusinessAppraisalModelObjectV>() ;
                    List<AppraisalKRAModelObjectV> AppKRAModelObjectVlist = new List<AppraisalKRAModelObjectV>() ;
                    List<AppraisalPotentialModelObjectV> AppPotModelObjectVlist = new List<AppraisalPotentialModelObjectV>() ;
                    List<PastExperienceModelObjectV> PastExpModelObjectVlist = new List<PastExperienceModelObjectV>();
                    List<PersonnelModelObjectV> PersonnelModelObjectVlist = new List<PersonnelModelObjectV>();
                    List<QualificationModelObjectV> QualificationModelObjectVlist = new List<QualificationModelObjectV>();
                    List<ServiceModelObjectV> ServiceModelObjectVlist = new List<ServiceModelObjectV>();
                    List<SkillModelObjectV> SkillModelObjectVlist = new List<SkillModelObjectV>();
                    List<TrainingModelObjectV> TrainingModelObjectVlist = new List<TrainingModelObjectV>() ;
                                                                                                                            
                    AppraisalAttributeModelObjectV AppraisalAttModelObjectV = null;
                    AppraisalBusinessAppraisalModelObjectV AppraisalBusiModelObjectV = null;
                    AppraisalKRAModelObjectV AppKRAModelObjectV = null;
                    AppraisalPotentialModelObjectV AppPotModelObjectV = null;
                    PastExperienceModelObjectV PastExpModelObjectV = null;
                    PersonnelModelObjectV PersonnelModelObjectV = null;
                    QualificationModelObjectV QualificationModelObjectV = null;
                    ServiceModelObjectV ServiceModelObjectV = null;
                    SkillModelObjectV SkillModelObjectV = null;
                    TrainingModelObjectV TrainingModelObjectV = null;

                    var DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                    CompetencyModelAssignment batchname = db.CompetencyModelAssignment.Find(BatchIds);

                    var muidsg = vidsf.CompetencyModelAssignment_OrgStructure.ToList();
                    #region geostruct
                    if (muidsg != null && muidsg.Count() > 0)
                    {


                        if (muidsg != null)
                        {
                            if (muidsg.Where(e => e.GeoStruct_Id != null).Count() > 0)
                            {
                                List<int?> GeoId = muidsg.Select(t => t.GeoStruct_Id).ToList();
                                var EmpListg = db.Employee.Include(e => e.ServiceBookDates).Include(e => e.GeoStruct).Where(e => GeoId.Contains(e.GeoStruct.Id) && e.ServiceBookDates.ServiceLastDate == null).ToList().Select(r => r.Id);
                                foreach (var EmpgId in EmpListg)
                                {
                                    Employee employeeg = db.Employee.Find(EmpgId);


                                    #region PersModel
                                    if (vidsf.CompetencyModel.PersonnelModel_Id != null)
                                    {
                                        Employee OEmp = db.Employee.Where(e => e.Id == EmpgId).Include(e => e.MaritalStatus)
                                                       .Include(e => e.Gender).Include(e => e.EmpName).Include(e => e.ServiceBookDates).FirstOrDefault();
                                        if (OEmp != null)
                                        {
                                            PersonnelModel OPersModel = db.PersonnelModel.Include(e => e.PersonnelModelObject)
                                                  .Include(e => e.PersonnelModelObject.Select(t => t.PersonnelModel))
                                                  .Where(e => e.Id == vidsf.CompetencyModel.PersonnelModel_Id).FirstOrDefault();
                                            List<PersonnelModelObject> OPersModelObj = OPersModel.PersonnelModelObject.ToList();
                                            //List<PersonnelModelObject> OPersModelObj = db.PersonnelModelObject.Include(e => e.PersonnelModel).ToList();
                                            foreach (var item in OPersModelObj.ToList())
                                            {
                                                List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                ObjectValue OObjVal = new ObjectValue();
                                                if (item.PersonnelModel.LookupVal.ToUpper() == "JOININGDATE")
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OEmp.ServiceBookDates.JoiningDate.Value.ToShortDateString();
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PersonnelModelObjectV = new PersonnelModelObjectV();
                                                    {
                                                        PersonnelModelObjectV.PersonnelModelObject = item;
                                                        PersonnelModelObjectV.DBTrack = DBTrack;
                                                        PersonnelModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PersonnelModelObjectVlist.Add(PersonnelModelObjectV);
                                                }
                                                if (item.PersonnelModel.LookupVal.ToUpper() == "CONFIRMATIONDATE")
                                                {

                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OEmp.ServiceBookDates.ConfirmationDate.Value.ToShortDateString();
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PersonnelModelObjectV = new PersonnelModelObjectV();
                                                    {
                                                        PersonnelModelObjectV.PersonnelModelObject = item;
                                                        PersonnelModelObjectV.DBTrack = DBTrack;
                                                        PersonnelModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PersonnelModelObjectVlist.Add(PersonnelModelObjectV);

                                                }
                                                if (item.PersonnelModel.LookupVal.ToUpper() == "RETIREMENTDATE")
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OEmp.ServiceBookDates.RetirementDate.Value.ToShortDateString();
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PersonnelModelObjectV = new PersonnelModelObjectV();
                                                    {
                                                        PersonnelModelObjectV.PersonnelModelObject = item;
                                                        PersonnelModelObjectV.DBTrack = DBTrack;
                                                        PersonnelModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PersonnelModelObjectVlist.Add(PersonnelModelObjectV);
                                                }
                                                if (item.PersonnelModel.LookupVal.ToUpper() == "BIRTHDATE")
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OEmp.ServiceBookDates.BirthDate.Value.ToShortDateString();
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PersonnelModelObjectV = new PersonnelModelObjectV();
                                                    {
                                                        PersonnelModelObjectV.PersonnelModelObject = item;
                                                        PersonnelModelObjectV.DBTrack = DBTrack;
                                                        PersonnelModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PersonnelModelObjectVlist.Add(PersonnelModelObjectV);
                                                }
                                                if (item.PersonnelModel.LookupVal.ToUpper() == "PROBATIONDATE")
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OEmp.ServiceBookDates.ProbationDate.Value.ToShortDateString();
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PersonnelModelObjectV = new PersonnelModelObjectV();
                                                    {
                                                        PersonnelModelObjectV.PersonnelModelObject = item;
                                                        PersonnelModelObjectV.DBTrack = DBTrack;
                                                        PersonnelModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PersonnelModelObjectVlist.Add(PersonnelModelObjectV);
                                                }
                                                if (item.PersonnelModel.LookupVal.ToUpper() == "GENDER")
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OEmp.Gender.LookupVal;
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PersonnelModelObjectV = new PersonnelModelObjectV();
                                                    {
                                                        PersonnelModelObjectV.PersonnelModelObject = item;
                                                        PersonnelModelObjectV.DBTrack = DBTrack;
                                                        PersonnelModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PersonnelModelObjectVlist.Add(PersonnelModelObjectV);

                                                }
                                                if (item.PersonnelModel.LookupVal.ToUpper() == "MARITALSTATUS")
                                                {
                                                    if (OEmp.MaritalStatus != null)
                                                    {
                                                        OObjVal = new ObjectValue();
                                                        {
                                                            OObjVal.ObjectVal = OEmp.MaritalStatus.LookupVal;
                                                            OObjVal.DBTrack = DBTrack;
                                                        };
                                                        OObjVallist.Add(OObjVal);
                                                        PersonnelModelObjectV = new PersonnelModelObjectV();
                                                        {
                                                            PersonnelModelObjectV.PersonnelModelObject = item;
                                                            PersonnelModelObjectV.DBTrack = DBTrack;
                                                            PersonnelModelObjectV.ObjectValue = OObjVallist;
                                                        };
                                                        PersonnelModelObjectVlist.Add(PersonnelModelObjectV);
                                                    }

                                                }

                                            }
                                        }

                                    }
                                    #endregion

                                    #region AppraisalAtt
                                    if (vidsf.CompetencyModel.AppraisalAttributeModel_Id != null)
                                    {
                                        var OEmp = db.EmployeeAppraisal.Where(e => e.Employee.Id == EmpgId)
                                            .Include(e => e.EmpAppEvaluation).Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion))
                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating)))
                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment))))
                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment.AppCategory)))).FirstOrDefault();

                                        if (OEmp != null)
                                        {
                                            AppraisalAttributeModel OAppraisalAttModel = db.AppraisalAttributeModel.Include(e => e.AppraisalAttributeModelObject)
                                                  .Include(e => e.AppraisalAttributeModelObject.Select(t => t.AppraisalAttributeModel))
                                                  .Where(e => e.Id == vidsf.CompetencyModel.AppraisalAttributeModel_Id).FirstOrDefault();
                                            List<AppraisalAttributeModelObject> OAttrModelObj = OAppraisalAttModel.AppraisalAttributeModelObject.ToList();
                                            // List<AppraisalAttributeModelObject> OAttrModelObj = db.AppraisalAttributeModelObject.Include(e => e.AppraisalAttributeModel).ToList();
                                            foreach (var item in OAttrModelObj)
                                            {
                                                List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                ObjectValue OObjVal = new ObjectValue();
                                                foreach (var APP1 in OEmp.EmpAppEvaluation)
                                                {
                                                    foreach (var APP2 in APP1.EmpAppRatingConclusion)
                                                    {
                                                        foreach (var APP3 in APP2.EmpAppRating.Where(t => t.AppAssignment.AppCategory.AppMode.LookupVal.ToUpper() == "ATTRIBUTE"))
                                                        {

                                                            if (item.AppraisalAttributeModel.LookupVal.ToUpper() == APP3.AppAssignment.AppCategory.Code)
                                                            {
                                                                OObjVal = new ObjectValue();
                                                                {
                                                                    OObjVal.ObjectVal = APP3.RatingPoints.ToString();
                                                                    OObjVal.DBTrack = DBTrack;
                                                                };
                                                                OObjVallist.Add(OObjVal);
                                                                AppraisalAttModelObjectV = new AppraisalAttributeModelObjectV();
                                                                {
                                                                    AppraisalAttModelObjectV.AppraisalAttributeModelObject = item;
                                                                    AppraisalAttModelObjectV.DBTrack = DBTrack;
                                                                    AppraisalAttModelObjectV.ObjectValue = OObjVallist;
                                                                };
                                                                AppraisalAttModelObjectVlist.Add(AppraisalAttModelObjectV);
                                                            }
                                                        }
                                                    }
                                                }

                                            }
                                        }


                                    }
                                    #endregion

                                    #region AppraisalPotential
                                    if (vidsf.CompetencyModel.AppraisalPotentialModel_Id != null)
                                    {
                                        var OEmp = db.EmployeeAppraisal.Where(e => e.Employee.Id == EmpgId)
                                            .Include(e => e.EmpAppEvaluation).Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion))
                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating)))
                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment))))
                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment.AppCategory)))).FirstOrDefault();

                                        if (OEmp != null)
                                        {
                                            AppraisalPotentialModel OAppraisalPotModel = db.AppraisalPotentialModel.Include(e => e.AppraisalPotentialModelObject)
                                                  .Include(e => e.AppraisalPotentialModelObject.Select(t => t.AppraisalPotentialModel))
                                                  .Where(e => e.Id == vidsf.CompetencyModel.AppraisalPotentialModel_Id).FirstOrDefault();
                                            List<AppraisalPotentialModelObject> OPotModelObj = OAppraisalPotModel.AppraisalPotentialModelObject.ToList();
                                            // List<AppraisalAttributeModelObject> OAttrModelObj = db.AppraisalAttributeModelObject.Include(e => e.AppraisalAttributeModel).ToList();
                                            foreach (var item in OPotModelObj)
                                            {
                                                List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                ObjectValue OObjVal = new ObjectValue();
                                                foreach (var APP1 in OEmp.EmpAppEvaluation)
                                                {
                                                    foreach (var APP2 in APP1.EmpAppRatingConclusion)
                                                    {
                                                        foreach (var APP3 in APP2.EmpAppRating.Where(t => t.AppAssignment.AppCategory.AppMode.LookupVal.ToUpper() == "POTENTIAL"))
                                                        {

                                                            if (item.AppraisalPotentialModel.LookupVal.ToUpper() == APP3.AppAssignment.AppCategory.Code)
                                                            {
                                                                OObjVal = new ObjectValue();
                                                                {
                                                                    OObjVal.ObjectVal = APP3.RatingPoints.ToString();
                                                                    OObjVal.DBTrack = DBTrack;
                                                                };
                                                                OObjVallist.Add(OObjVal);
                                                                AppPotModelObjectV = new AppraisalPotentialModelObjectV();
                                                                {
                                                                    AppPotModelObjectV.AppraisalPotentialModelObject = item;
                                                                    AppPotModelObjectV.DBTrack = DBTrack;
                                                                    AppPotModelObjectV.ObjectValue = OObjVallist;
                                                                };
                                                                AppPotModelObjectVlist.Add(AppPotModelObjectV);
                                                            }
                                                        }
                                                    }
                                                }

                                            }
                                        }


                                    }
                                    #endregion

                                    #region AppraisalKRA
                                    if (vidsf.CompetencyModel.AppraisalKRAModel_Id != null)
                                    {
                                        var OEmp = db.EmployeeAppraisal.Where(e => e.Employee.Id == EmpgId)
                                            .Include(e => e.EmpAppEvaluation).Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion))
                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating)))
                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment))))
                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment.AppCategory)))).FirstOrDefault();

                                        if (OEmp != null)
                                        {
                                            AppraisalKRAModel OAppraisalKRAModel = db.AppraisalKRAModel.Include(e => e.AppraisalKRAModelObject)
                                                  .Include(e => e.AppraisalKRAModelObject.Select(t => t.AppraisalKRAModel))
                                                  .Where(e => e.Id == vidsf.CompetencyModel.AppraisalPotentialModel_Id).FirstOrDefault();
                                            List<AppraisalKRAModelObject> OKRAModelObj = OAppraisalKRAModel.AppraisalKRAModelObject.ToList();
                                            // List<AppraisalAttributeModelObject> OAttrModelObj = db.AppraisalAttributeModelObject.Include(e => e.AppraisalAttributeModel).ToList();
                                            foreach (var item in OKRAModelObj)
                                            {
                                                List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                ObjectValue OObjVal = new ObjectValue();
                                                foreach (var APP1 in OEmp.EmpAppEvaluation)
                                                {
                                                    foreach (var APP2 in APP1.EmpAppRatingConclusion)
                                                    {
                                                        foreach (var APP3 in APP2.EmpAppRating.Where(t => t.AppAssignment.AppCategory.AppMode.LookupVal.ToUpper() == "KRA"))
                                                        {

                                                            if (item.AppraisalKRAModel.LookupVal.ToUpper() == APP3.AppAssignment.AppCategory.Code)
                                                            {
                                                                OObjVal = new ObjectValue();
                                                                {
                                                                    OObjVal.ObjectVal = APP3.RatingPoints.ToString();
                                                                    OObjVal.DBTrack = DBTrack;
                                                                };
                                                                OObjVallist.Add(OObjVal);
                                                                AppKRAModelObjectV = new AppraisalKRAModelObjectV();
                                                                {
                                                                    AppKRAModelObjectV.AppraisalKRAModelObject = item;
                                                                    AppKRAModelObjectV.DBTrack = DBTrack;
                                                                    AppKRAModelObjectV.ObjectValue = OObjVallist;
                                                                };
                                                                AppKRAModelObjectVlist.Add(AppKRAModelObjectV);
                                                            }
                                                        }
                                                    }
                                                }

                                            }
                                        }


                                    }
                                    #endregion

                                    #region BusinessAppraisal
                                    #endregion

                                    #region PastExperience
                                    if (vidsf.CompetencyModel.PastExperienceModel_Id != null)
                                    {
                                        var OPrevEx = db.Employee.Where(e => e.Id == EmpgId)
                                                         .Include(e => e.PreCompExp).Include(e => e.PreCompExp.Select(t => t.ExperienceDetails)).FirstOrDefault()
                                                         .PreCompExp.OrderByDescending(e => e.FromDate).FirstOrDefault();
                                        if (OPrevEx != null)
                                        {
                                            PastExperienceModel OPastExpModel = db.PastExperienceModel.Include(e => e.PastExperienceModelObject)
                                                  .Include(e => e.PastExperienceModelObject.Select(t => t.PastExperienceModel))
                                                  .Where(e => e.Id == vidsf.CompetencyModel.PastExperienceModel_Id).FirstOrDefault();
                                            List<PastExperienceModelObject> OPastExpModelObj = OPastExpModel.PastExperienceModelObject.ToList();
                                            //List<PastExperienceModelObject> OPastExpModelObj = db.PastExperienceModelObject.Include(e => e.PastExperienceModel)
                                            //    .ToList();
                                            foreach (var item in OPastExpModelObj)
                                            {
                                                List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                ObjectValue OObjVal = new ObjectValue();
                                                if (item.PastExperienceModel.LookupVal.ToUpper() == "LEAVINGJOBPOSITION")
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OPrevEx.LeaveingJobPosition;
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PastExpModelObjectV = new PastExperienceModelObjectV();
                                                    {
                                                        PastExpModelObjectV.PastExperienceModelObject = item;
                                                        PastExpModelObjectV.DBTrack = DBTrack;
                                                        PastExpModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PastExpModelObjectVlist.Add(PastExpModelObjectV);
                                                }
                                                if (item.PastExperienceModel.LookupVal.ToUpper() == "JOININGJOBPOSITION")
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OPrevEx.JoiningJobPosition;
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PastExpModelObjectV = new PastExperienceModelObjectV();
                                                    {
                                                        PastExpModelObjectV.PastExperienceModelObject = item;
                                                        PastExpModelObjectV.DBTrack = DBTrack;
                                                        PastExpModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PastExpModelObjectVlist.Add(PastExpModelObjectV);
                                                }
                                                if (item.PastExperienceModel.LookupVal.ToUpper() == "YROFSERVICE")
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OPrevEx.YrOfService.ToString();
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PastExpModelObjectV = new PastExperienceModelObjectV();
                                                    {
                                                        PastExpModelObjectV.PastExperienceModelObject = item;
                                                        PastExpModelObjectV.DBTrack = DBTrack;
                                                        PastExpModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PastExpModelObjectVlist.Add(PastExpModelObjectV);
                                                }
                                                if (item.PastExperienceModel.LookupVal.ToUpper() == "SPECIALACHIEVEMENTS")
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OPrevEx.SpecialAchievements;
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PastExpModelObjectV = new PastExperienceModelObjectV();
                                                    {
                                                        PastExpModelObjectV.PastExperienceModelObject = item;
                                                        PastExpModelObjectV.DBTrack = DBTrack;
                                                        PastExpModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PastExpModelObjectVlist.Add(PastExpModelObjectV);
                                                }
                                                if (item.PastExperienceModel.LookupVal.ToUpper() == "ExperienceDetails")
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OPrevEx.ExperienceDetails.LookupVal;
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PastExpModelObjectV = new PastExperienceModelObjectV();
                                                    {
                                                        PastExpModelObjectV.PastExperienceModelObject = item;
                                                        PastExpModelObjectV.DBTrack = DBTrack;
                                                        PastExpModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PastExpModelObjectVlist.Add(PastExpModelObjectV);
                                                }
                                            }

                                        }
                                    }

                                    #endregion

                                    #region SkillModel
                                    if (vidsf.CompetencyModel.SkillModel_Id != null)
                                    {
                                        var OEmp = db.Employee.Include(e => e.EmpName).Include(e => e.EmpAcademicInfo).Include(e => e.EmpAcademicInfo.Skill)
                                            .Include(e => e.EmpAcademicInfo.Skill.Select(t => t.SkillType))
                                                    .Where(e => e.Id == EmpgId).FirstOrDefault();
                                        if (OEmp != null)
                                        {
                                            SkillModel OSkillModel = db.SkillModel.Include(e => e.SkillModelObject)
                                                  .Include(e => e.SkillModelObject.Select(t => t.SkillModel))
                                                  .Where(e => e.Id == vidsf.CompetencyModel.SkillModel_Id).FirstOrDefault();
                                            List<SkillModelObject> OSkillModelObj = OSkillModel.SkillModelObject.ToList();
                                            // List<SkillModelObject> OSkillModelObj = db.SkillModelObject.Include(e => e.SkillModel).ToList();
                                            foreach (var item in OSkillModelObj)
                                            {
                                                if (OEmp.EmpAcademicInfo != null && OEmp.EmpAcademicInfo.Skill.Count() > 0)
                                                {
                                                    List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                    ObjectValue OObjVal = new ObjectValue();
                                                    foreach (var itemQ in OEmp.EmpAcademicInfo.Skill)
                                                    {
                                                        if (itemQ.SkillType != null)
                                                        {
                                                            if (item.SkillModel.LookupVal.ToUpper() == itemQ.SkillType.LookupVal.ToUpper())
                                                            {
                                                                OObjVal = new ObjectValue();
                                                                {
                                                                    OObjVal.ObjectVal = itemQ.Name;
                                                                    OObjVal.DBTrack = DBTrack;
                                                                };
                                                                OObjVallist.Add(OObjVal);
                                                            }

                                                        }

                                                    }
                                                    SkillModelObjectV = new SkillModelObjectV();
                                                    {
                                                        SkillModelObjectV.SkillModelObject = item;
                                                        SkillModelObjectV.DBTrack = DBTrack;
                                                        SkillModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    SkillModelObjectVlist.Add(SkillModelObjectV);
                                                }

                                            }
                                        }
                                    }
                                    #endregion

                                    #region Qualification
                                    if (vidsf.CompetencyModel.QualificationModel_Id != null)
                                    {
                                        var OEmp = db.Employee.Include(e => e.EmpName).Include(e => e.EmpAcademicInfo).Include(e => e.EmpAcademicInfo.QualificationDetails)
                                                    .Include(e => e.EmpAcademicInfo.QualificationDetails.Select(t => t.Qualification))
                                                    .Where(e => e.Id == EmpgId).FirstOrDefault();

                                        if (OEmp != null)
                                        {
                                            QualificationModel OQualiModel = db.QualificationModel.Include(e => e.QualificationModelObject)
                                                    .Include(e => e.QualificationModelObject.Select(t => t.QualificationModel))
                                                    .Where(e => e.Id == vidsf.CompetencyModel.QualificationModel_Id).FirstOrDefault();
                                            List<QualificationModelObject> OQualiModelObj = OQualiModel.QualificationModelObject.ToList();
                                            foreach (var item in OQualiModelObj)
                                            {
                                                List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                ObjectValue OObjVal = new ObjectValue();
                                                if (OEmp.EmpAcademicInfo != null && OEmp.EmpAcademicInfo.QualificationDetails.Count() > 0)
                                                {
                                                    foreach (var itemQ in OEmp.EmpAcademicInfo.QualificationDetails)
                                                    {
                                                        foreach (var itemQ1 in itemQ.Qualification)
                                                        {
                                                            OObjVal = new ObjectValue();
                                                            {
                                                                OObjVal.ObjectVal = itemQ1.QualificationDesc;
                                                                OObjVal.DBTrack = DBTrack;
                                                            };
                                                            OObjVallist.Add(OObjVal);

                                                        }
                                                    }
                                                    QualificationModelObjectV = new QualificationModelObjectV();
                                                    {
                                                        QualificationModelObjectV.QualificationModelObject = item;
                                                        QualificationModelObjectV.DBTrack = DBTrack;
                                                        QualificationModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    QualificationModelObjectVlist.Add(QualificationModelObjectV);
                                                }

                                            }
                                        }
                                    }
                                    #endregion

                                    #region Training
                                    if (vidsf.CompetencyModel.TrainingModel_Id != null)
                                    {
                                        var OEmp = db.EmployeeTraining.Include(e => e.TrainingDetails)
                                            .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo))
                                            .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession)))
                                            .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession.TrainingProgramCalendar)))
                                            .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession.TrainingProgramCalendar.ProgramList)))
                                            //.Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule))
                                            //.Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule.TrainingSession))
                                            //.Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule.TrainingSession.Select(t => t.TrainingProgramCalendar)))
                                            //.Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule.TrainingSession.Select(t => t.TrainingProgramCalendar.ProgramList)))
                                                    .Where(e => e.Employee_Id == EmpgId).FirstOrDefault();
                                        if (OEmp != null)
                                        {
                                            var OTrCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "TRAININGCALENDAR" && e.Default == true).FirstOrDefault();
                                            TrainingModel OTrainingModel = db.TrainingModel.Include(e => e.TrainingModelObject)
                                                   .Include(e => e.TrainingModelObject.Select(t => t.TrainingModel))
                                                   .Where(e => e.Id == vidsf.CompetencyModel.TrainingModel_Id).FirstOrDefault();
                                            List<TrainingModelObject> OTrainingModelObj = OTrainingModel.TrainingModelObject.ToList();
                                            // List<TrainingModelObject> OTrainingModelObj = db.TrainingModelObject.Include(e => e.TrainingModel).ToList();
                                            foreach (var item in OTrainingModelObj)
                                            {
                                                List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                ObjectValue OObjVal = new ObjectValue();
                                                if (OEmp.TrainingDetails.Count() > 0)
                                                {
                                                    foreach (var itemT in OEmp.TrainingDetails)
                                                    {
                                                        foreach (var itemT1 in itemT.TrainigDetailSessionInfo)
                                                        {
                                                            if (itemT1.TrainingSession.TrainingProgramCalendar.StartDate >= OTrCalendar.FromDate && itemT1.TrainingSession.TrainingProgramCalendar.EndDate <= OTrCalendar.ToDate)
                                                            {
                                                                OObjVal = new ObjectValue();
                                                                {
                                                                    OObjVal.ObjectVal = itemT1.TrainingSession.TrainingProgramCalendar.ProgramList.Subject;
                                                                    OObjVal.DBTrack = DBTrack;
                                                                };
                                                                OObjVallist.Add(OObjVal);
                                                                TrainingModelObjectV = new TrainingModelObjectV();
                                                                {
                                                                    TrainingModelObjectV.TrainingModelObject = item;
                                                                    TrainingModelObjectV.DBTrack = DBTrack;
                                                                    TrainingModelObjectV.ObjectValue = OObjVallist;
                                                                };
                                                                TrainingModelObjectVlist.Add(TrainingModelObjectV);
                                                            }
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                    }
                                    #endregion

                                    #region ServiceModel
                                    if (vidsf.CompetencyModel.ServiceModel_Id != null)
                                    {
                                        Employee OEmp = db.Employee.Where(e => e.Id == EmpgId).Include(e => e.EmpName).Include(e => e.ServiceBookDates).FirstOrDefault();
                                        if (OEmp != null)
                                        {
                                            ServiceModel OServiceModel = db.ServiceModel.Include(e => e.ServiceModelObject)
                                                  .Include(e => e.ServiceModelObject.Select(t => t.ServiceModel))
                                                  .Where(e => e.Id == vidsf.CompetencyModel.ServiceModel_Id).FirstOrDefault();
                                            List<ServiceModelObject> OServModelObj = OServiceModel.ServiceModelObject.ToList();
                                            //List<ServiceModelObject> OServModelObj = db.ServiceModelObject.Include(e => e.ServiceModel).ToList();
                                            foreach (var item in OServModelObj)
                                            {
                                                List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                ObjectValue OObjVal = new ObjectValue();
                                                DateTime start = (DateTime)OEmp.ServiceBookDates.JoiningDate;
                                                DateTime end = DateTime.Now.Date;
                                                int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);

                                                double daysInEndMonth = (end.AddMonths(1) - end).Days;
                                                double months = compMonth + (start.Date.Day - end.Date.Day) / daysInEndMonth;
                                                double ServYear = Math.Round(months / 12, 2);
                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = ServYear.ToString();
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);
                                                ServiceModelObjectV = new ServiceModelObjectV();
                                                {
                                                    ServiceModelObjectV.ServiceModelObject = item;
                                                    ServiceModelObjectV.DBTrack = DBTrack;
                                                    ServiceModelObjectV.ObjectValue = OObjVallist;
                                                };
                                                ServiceModelObjectVlist.Add(ServiceModelObjectV);
                                            }
                                        }
                                    }
                                    #endregion

                                    // CompetencyModelAssignment batchname = db.CompetencyModelAssignment.Find(BatchIds);
                                    CompetencyEmployeeDataGeneration CompetencyEmployeeDataGenerationT = new CompetencyEmployeeDataGeneration();
                                    {
                                        CompetencyEmployeeDataGenerationT.Employee = employeeg;
                                        CompetencyEmployeeDataGenerationT.BatchName = batchname;
                                        CompetencyEmployeeDataGenerationT.ProcessDate = DateTime.Now;
                                        CompetencyEmployeeDataGenerationT.AppraisalAttributeModelObjectV = AppraisalAttModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.AppraisalBusinessAppraisalModelObjectV = AppraisalBusiModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.AppraisalKRAModelObjectV = AppKRAModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.AppraisalPotentialModelObjectV = AppPotModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.PastExperienceModelObjectV = PastExpModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.PersonnelModelObjectV = PersonnelModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.QualificationModelObjectV = QualificationModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.ServiceModelObjectV = ServiceModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.SkillModelObjectV = SkillModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.TrainingModelObjectV = TrainingModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.DBTrack = DBTrack;
                                    };
                                   // CompetencyEmployeeDataGenerationList.Add(CompetencyEmployeeDataGenerationT);
                                    db.CompetencyEmployeeDataGeneration.Add(CompetencyEmployeeDataGenerationT);
                                    db.SaveChanges();
                                }
                            }
                        }

                    }
#endregion

                    #region Paystruct
                    if (muidsg != null && muidsg.Count() > 0)
                    {

                        if (muidsg.Where(e => e.PayStruct_Id != null).Count() > 0)
                        {

                            List<int?> PayId = muidsg.Select(t => t.PayStruct_Id).ToList();
                            var EmpListp = db.Employee.Include(e => e.ServiceBookDates).Include(e => e.PayStruct).Where(e => PayId.Contains(e.PayStruct.Id) &&  e.ServiceBookDates.ServiceLastDate == null).ToList().Select(r => r.Id);

                            foreach (var EmppId in EmpListp)
                            {
                                AppraisalAttModelObjectVlist = new List<AppraisalAttributeModelObjectV>();
                                AppraisalBusiModelObjectVlist = new List<AppraisalBusinessAppraisalModelObjectV>();
                                AppKRAModelObjectVlist = new List<AppraisalKRAModelObjectV>();
                                AppPotModelObjectVlist = new List<AppraisalPotentialModelObjectV>();
                                PastExpModelObjectVlist = new List<PastExperienceModelObjectV>();
                                PersonnelModelObjectVlist = new List<PersonnelModelObjectV>();
                                QualificationModelObjectVlist = new List<QualificationModelObjectV>();
                                ServiceModelObjectVlist = new List<ServiceModelObjectV>();
                                SkillModelObjectVlist = new List<SkillModelObjectV>();
                                TrainingModelObjectVlist = new List<TrainingModelObjectV>();
                                Employee employeep = db.Employee.Find(EmppId);

                                #region PersModel
                                if (vidsf.CompetencyModel.PersonnelModel_Id != null)
                                {
                                    Employee OEmp = db.Employee.Where(e => e.Id == EmppId).Include(e => e.MaritalStatus)
                                                   .Include(e => e.Gender).Include(e => e.EmpName).Include(e => e.ServiceBookDates).FirstOrDefault();
                                    if (OEmp != null)
                                    {
                                        PersonnelModel OPersModel = db.PersonnelModel.Include(e => e.PersonnelModelObject)
                                              .Include(e => e.PersonnelModelObject.Select(t => t.PersonnelModel))
                                              .Where(e => e.Id == vidsf.CompetencyModel.PersonnelModel_Id).FirstOrDefault();
                                        List<PersonnelModelObject> OPersModelObj = OPersModel.PersonnelModelObject.ToList();
                                        //List<PersonnelModelObject> OPersModelObj = db.PersonnelModelObject.Include(e => e.PersonnelModel).ToList();
                                        foreach (var item in OPersModelObj.ToList())
                                        {
                                            List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                            ObjectValue OObjVal = new ObjectValue();
                                            if (item.PersonnelModel.LookupVal.ToUpper() == "JOININGDATE")
                                            {
                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = OEmp.ServiceBookDates.JoiningDate.Value.ToShortDateString();
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);
                                                PersonnelModelObjectV = new PersonnelModelObjectV();
                                                {
                                                    PersonnelModelObjectV.PersonnelModelObject = item;
                                                    PersonnelModelObjectV.DBTrack = DBTrack;
                                                    PersonnelModelObjectV.ObjectValue = OObjVallist;
                                                };
                                                PersonnelModelObjectVlist.Add(PersonnelModelObjectV);
                                            }
                                            if (item.PersonnelModel.LookupVal.ToUpper() == "CONFIRMATIONDATE")
                                            {

                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = OEmp.ServiceBookDates.ConfirmationDate.Value.ToShortDateString();
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);
                                                PersonnelModelObjectV = new PersonnelModelObjectV();
                                                {
                                                    PersonnelModelObjectV.PersonnelModelObject = item;
                                                    PersonnelModelObjectV.DBTrack = DBTrack;
                                                    PersonnelModelObjectV.ObjectValue = OObjVallist;
                                                };
                                                PersonnelModelObjectVlist.Add(PersonnelModelObjectV);

                                            }
                                            if (item.PersonnelModel.LookupVal.ToUpper() == "RETIREMENTDATE")
                                            {
                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = OEmp.ServiceBookDates.RetirementDate.Value.ToShortDateString();
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);
                                                PersonnelModelObjectV = new PersonnelModelObjectV();
                                                {
                                                    PersonnelModelObjectV.PersonnelModelObject = item;
                                                    PersonnelModelObjectV.DBTrack = DBTrack;
                                                    PersonnelModelObjectV.ObjectValue = OObjVallist;
                                                };
                                                PersonnelModelObjectVlist.Add(PersonnelModelObjectV);
                                            }
                                            if (item.PersonnelModel.LookupVal.ToUpper() == "BIRTHDATE")
                                            {
                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = OEmp.ServiceBookDates.BirthDate.Value.ToShortDateString();
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);
                                                PersonnelModelObjectV = new PersonnelModelObjectV();
                                                {
                                                    PersonnelModelObjectV.PersonnelModelObject = item;
                                                    PersonnelModelObjectV.DBTrack = DBTrack;
                                                    PersonnelModelObjectV.ObjectValue = OObjVallist;
                                                };
                                                PersonnelModelObjectVlist.Add(PersonnelModelObjectV);
                                            }
                                            if (item.PersonnelModel.LookupVal.ToUpper() == "PROBATIONDATE")
                                            {
                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = OEmp.ServiceBookDates.ProbationDate.Value.ToShortDateString();
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);
                                                PersonnelModelObjectV = new PersonnelModelObjectV();
                                                {
                                                    PersonnelModelObjectV.PersonnelModelObject = item;
                                                    PersonnelModelObjectV.DBTrack = DBTrack;
                                                    PersonnelModelObjectV.ObjectValue = OObjVallist;
                                                };
                                                PersonnelModelObjectVlist.Add(PersonnelModelObjectV);
                                            }
                                            if (item.PersonnelModel.LookupVal.ToUpper() == "GENDER")
                                            {
                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = OEmp.Gender.LookupVal;
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);
                                                PersonnelModelObjectV = new PersonnelModelObjectV();
                                                {
                                                    PersonnelModelObjectV.PersonnelModelObject = item;
                                                    PersonnelModelObjectV.DBTrack = DBTrack;
                                                    PersonnelModelObjectV.ObjectValue = OObjVallist;
                                                };
                                                PersonnelModelObjectVlist.Add(PersonnelModelObjectV);

                                            }
                                            if (item.PersonnelModel.LookupVal.ToUpper() == "MARITALSTATUS")
                                            {
                                                if (OEmp.MaritalStatus != null)
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OEmp.MaritalStatus.LookupVal;
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PersonnelModelObjectV = new PersonnelModelObjectV();
                                                    {
                                                        PersonnelModelObjectV.PersonnelModelObject = item;
                                                        PersonnelModelObjectV.DBTrack = DBTrack;
                                                        PersonnelModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PersonnelModelObjectVlist.Add(PersonnelModelObjectV);
                                                }
                                               
                                            }

                                        }
                                    }

                                }
                                #endregion

                                #region AppraisalAtt
                                if (vidsf.CompetencyModel.AppraisalAttributeModel_Id != null)
                                {
                                    var OEmp = db.EmployeeAppraisal.Where(e => e.Employee.Id == EmppId)
                                        .Include(e => e.EmpAppEvaluation).Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion))
                                        .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating)))
                                        .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment))))
                                        .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment.AppCategory)))).FirstOrDefault();

                                    if (OEmp != null)
                                    {
                                        AppraisalAttributeModel OAppraisalAttModel = db.AppraisalAttributeModel.Include(e => e.AppraisalAttributeModelObject)
                                              .Include(e => e.AppraisalAttributeModelObject.Select(t => t.AppraisalAttributeModel))
                                              .Where(e => e.Id == vidsf.CompetencyModel.AppraisalAttributeModel_Id).FirstOrDefault();
                                        List<AppraisalAttributeModelObject> OAttrModelObj = OAppraisalAttModel.AppraisalAttributeModelObject.ToList();
                                       // List<AppraisalAttributeModelObject> OAttrModelObj = db.AppraisalAttributeModelObject.Include(e => e.AppraisalAttributeModel).ToList();
                                        foreach (var item in OAttrModelObj)
                                        {
                                            List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                            ObjectValue OObjVal = new ObjectValue();
                                            foreach (var APP1 in OEmp.EmpAppEvaluation)
                                            {
                                                foreach (var APP2 in APP1.EmpAppRatingConclusion)
                                                {
                                                    foreach (var APP3 in APP2.EmpAppRating.Where(t => t.AppAssignment.AppCategory.AppMode.LookupVal.ToUpper() == "ATTRIBUTE"))
                                                    {

                                                        if (item.AppraisalAttributeModel.LookupVal.ToUpper() == APP3.AppAssignment.AppCategory.Code)
                                                        {
                                                            OObjVal = new ObjectValue();
                                                            {
                                                                OObjVal.ObjectVal = APP3.RatingPoints.ToString();
                                                                OObjVal.DBTrack = DBTrack;
                                                            };
                                                            OObjVallist.Add(OObjVal);
                                                            AppraisalAttModelObjectV = new AppraisalAttributeModelObjectV();
                                                            {
                                                                AppraisalAttModelObjectV.AppraisalAttributeModelObject = item;
                                                                AppraisalAttModelObjectV.DBTrack = DBTrack;
                                                                AppraisalAttModelObjectV.ObjectValue = OObjVallist;
                                                            };
                                                            AppraisalAttModelObjectVlist.Add(AppraisalAttModelObjectV);
                                                        }
                                                    }
                                                }
                                            }

                                        }
                                    }


                                }
                                #endregion

                                #region AppraisalPotential
                                if (vidsf.CompetencyModel.AppraisalPotentialModel_Id != null)
                                {
                                    var OEmp = db.EmployeeAppraisal.Where(e => e.Employee.Id == EmppId)
                                        .Include(e => e.EmpAppEvaluation).Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion))
                                        .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating)))
                                        .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment))))
                                        .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment.AppCategory)))).FirstOrDefault();

                                    if (OEmp != null)
                                    {
                                        AppraisalPotentialModel OAppraisalPotModel = db.AppraisalPotentialModel.Include(e => e.AppraisalPotentialModelObject)
                                              .Include(e => e.AppraisalPotentialModelObject.Select(t => t.AppraisalPotentialModel))
                                              .Where(e => e.Id == vidsf.CompetencyModel.AppraisalPotentialModel_Id).FirstOrDefault();
                                        List<AppraisalPotentialModelObject> OPotModelObj = OAppraisalPotModel.AppraisalPotentialModelObject.ToList();
                                        // List<AppraisalAttributeModelObject> OAttrModelObj = db.AppraisalAttributeModelObject.Include(e => e.AppraisalAttributeModel).ToList();
                                        foreach (var item in OPotModelObj)
                                        {
                                            List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                            ObjectValue OObjVal = new ObjectValue();
                                            foreach (var APP1 in OEmp.EmpAppEvaluation)
                                            {
                                                foreach (var APP2 in APP1.EmpAppRatingConclusion)
                                                {
                                                    foreach (var APP3 in APP2.EmpAppRating.Where(t => t.AppAssignment.AppCategory.AppMode.LookupVal.ToUpper() == "POTENTIAL"))
                                                    {

                                                        if (item.AppraisalPotentialModel.LookupVal.ToUpper() == APP3.AppAssignment.AppCategory.Code)
                                                        {
                                                            OObjVal = new ObjectValue();
                                                            {
                                                                OObjVal.ObjectVal = APP3.RatingPoints.ToString();
                                                                OObjVal.DBTrack = DBTrack;
                                                            };
                                                            OObjVallist.Add(OObjVal);
                                                            AppPotModelObjectV = new AppraisalPotentialModelObjectV();
                                                            {
                                                                AppPotModelObjectV.AppraisalPotentialModelObject = item;
                                                                AppPotModelObjectV.DBTrack = DBTrack;
                                                                AppPotModelObjectV.ObjectValue = OObjVallist;
                                                            };
                                                            AppPotModelObjectVlist.Add(AppPotModelObjectV);
                                                        }
                                                    }
                                                }
                                            }

                                        }
                                    }


                                }
                                #endregion

                                #region AppraisalKRA
                                if (vidsf.CompetencyModel.AppraisalKRAModel_Id != null)
                                {
                                    var OEmp = db.EmployeeAppraisal.Where(e => e.Employee.Id == EmppId)
                                        .Include(e => e.EmpAppEvaluation).Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion))
                                        .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating)))
                                        .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment))))
                                        .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment.AppCategory)))).FirstOrDefault();

                                    if (OEmp != null)
                                    {
                                        AppraisalKRAModel OAppraisalKRAModel = db.AppraisalKRAModel.Include(e => e.AppraisalKRAModelObject)
                                              .Include(e => e.AppraisalKRAModelObject.Select(t => t.AppraisalKRAModel))
                                              .Where(e => e.Id == vidsf.CompetencyModel.AppraisalPotentialModel_Id).FirstOrDefault();
                                        List<AppraisalKRAModelObject> OKRAModelObj = OAppraisalKRAModel.AppraisalKRAModelObject.ToList();
                                        // List<AppraisalAttributeModelObject> OAttrModelObj = db.AppraisalAttributeModelObject.Include(e => e.AppraisalAttributeModel).ToList();
                                        foreach (var item in OKRAModelObj)
                                        {
                                            List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                            ObjectValue OObjVal = new ObjectValue();
                                            foreach (var APP1 in OEmp.EmpAppEvaluation)
                                            {
                                                foreach (var APP2 in APP1.EmpAppRatingConclusion)
                                                {
                                                    foreach (var APP3 in APP2.EmpAppRating.Where(t => t.AppAssignment.AppCategory.AppMode.LookupVal.ToUpper() == "KRA"))
                                                    {

                                                        if (item.AppraisalKRAModel.LookupVal.ToUpper() == APP3.AppAssignment.AppCategory.Code)
                                                        {
                                                            OObjVal = new ObjectValue();
                                                            {
                                                                OObjVal.ObjectVal = APP3.RatingPoints.ToString();
                                                                OObjVal.DBTrack = DBTrack;
                                                            };
                                                            OObjVallist.Add(OObjVal);
                                                            AppKRAModelObjectV = new AppraisalKRAModelObjectV();
                                                            {
                                                                AppKRAModelObjectV.AppraisalKRAModelObject = item;
                                                                AppKRAModelObjectV.DBTrack = DBTrack;
                                                                AppKRAModelObjectV.ObjectValue = OObjVallist;
                                                            };
                                                            AppKRAModelObjectVlist.Add(AppKRAModelObjectV);
                                                        }
                                                    }
                                                }
                                            }

                                        }
                                    }


                                }
                                #endregion

                                #region BusinessAppraisal
                                #endregion

                                #region PastExperience
                                if (vidsf.CompetencyModel.PastExperienceModel_Id != null)
                                {
                                    var OPrevEx = db.Employee.Where(e => e.Id == EmppId)
                                                     .Include(e => e.PreCompExp).Include(e => e.PreCompExp.Select(t => t.ExperienceDetails)).FirstOrDefault()
                                                     .PreCompExp.OrderByDescending(e => e.FromDate).FirstOrDefault();
                                    if (OPrevEx != null)
                                    {
                                        PastExperienceModel OPastExpModel = db.PastExperienceModel.Include(e => e.PastExperienceModelObject)
                                              .Include(e => e.PastExperienceModelObject.Select(t => t.PastExperienceModel))
                                              .Where(e => e.Id == vidsf.CompetencyModel.PastExperienceModel_Id).FirstOrDefault();
                                        List<PastExperienceModelObject> OPastExpModelObj = OPastExpModel.PastExperienceModelObject.ToList();
                                        //List<PastExperienceModelObject> OPastExpModelObj = db.PastExperienceModelObject.Include(e => e.PastExperienceModel)
                                        //    .ToList();
                                        foreach (var item in OPastExpModelObj)
                                        {
                                            List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                            ObjectValue OObjVal = new ObjectValue();
                                            if (item.PastExperienceModel.LookupVal.ToUpper() == "LEAVINGJOBPOSITION")
                                            {
                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = OPrevEx.LeaveingJobPosition;
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);
                                                PastExpModelObjectV = new PastExperienceModelObjectV();
                                                {
                                                    PastExpModelObjectV.PastExperienceModelObject = item;
                                                    PastExpModelObjectV.DBTrack = DBTrack;
                                                    PastExpModelObjectV.ObjectValue = OObjVallist;
                                                };
                                                PastExpModelObjectVlist.Add(PastExpModelObjectV);
                                            }
                                            if (item.PastExperienceModel.LookupVal.ToUpper() == "JOININGJOBPOSITION")
                                            {
                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = OPrevEx.JoiningJobPosition;
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);
                                                PastExpModelObjectV = new PastExperienceModelObjectV();
                                                {
                                                    PastExpModelObjectV.PastExperienceModelObject = item;
                                                    PastExpModelObjectV.DBTrack = DBTrack;
                                                    PastExpModelObjectV.ObjectValue = OObjVallist;
                                                };
                                                PastExpModelObjectVlist.Add(PastExpModelObjectV);
                                            }
                                            if (item.PastExperienceModel.LookupVal.ToUpper() == "YROFSERVICE")
                                            {
                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = OPrevEx.YrOfService.ToString();
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);
                                                PastExpModelObjectV = new PastExperienceModelObjectV();
                                                {
                                                    PastExpModelObjectV.PastExperienceModelObject = item;
                                                    PastExpModelObjectV.DBTrack = DBTrack;
                                                    PastExpModelObjectV.ObjectValue = OObjVallist;
                                                };
                                                PastExpModelObjectVlist.Add(PastExpModelObjectV);
                                            }
                                            if (item.PastExperienceModel.LookupVal.ToUpper() == "SPECIALACHIEVEMENTS")
                                            {
                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = OPrevEx.SpecialAchievements;
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);
                                                PastExpModelObjectV = new PastExperienceModelObjectV();
                                                {
                                                    PastExpModelObjectV.PastExperienceModelObject = item;
                                                    PastExpModelObjectV.DBTrack = DBTrack;
                                                    PastExpModelObjectV.ObjectValue = OObjVallist;
                                                };
                                                PastExpModelObjectVlist.Add(PastExpModelObjectV);
                                            }
                                            if (item.PastExperienceModel.LookupVal.ToUpper() == "ExperienceDetails")
                                            {
                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = OPrevEx.ExperienceDetails.LookupVal;
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);
                                                PastExpModelObjectV = new PastExperienceModelObjectV();
                                                {
                                                    PastExpModelObjectV.PastExperienceModelObject = item;
                                                    PastExpModelObjectV.DBTrack = DBTrack;
                                                    PastExpModelObjectV.ObjectValue = OObjVallist;
                                                };
                                                PastExpModelObjectVlist.Add(PastExpModelObjectV);
                                            }
                                        }

                                    }
                                }

                                #endregion

                                #region SkillModel
                                if (vidsf.CompetencyModel.SkillModel_Id != null)
                                {
                                    var OEmp = db.Employee.Include(e => e.EmpName).Include(e => e.EmpAcademicInfo).Include(e => e.EmpAcademicInfo.Skill)
                                        .Include(e => e.EmpAcademicInfo.Skill.Select(t => t.SkillType))
                                                .Where(e => e.Id == EmppId).FirstOrDefault();
                                    if (OEmp != null)
                                    {
                                        SkillModel OSkillModel = db.SkillModel.Include(e => e.SkillModelObject)
                                              .Include(e => e.SkillModelObject.Select(t => t.SkillModel))
                                              .Where(e => e.Id == vidsf.CompetencyModel.SkillModel_Id).FirstOrDefault();
                                        List<SkillModelObject> OSkillModelObj = OSkillModel.SkillModelObject.ToList();
                                       // List<SkillModelObject> OSkillModelObj = db.SkillModelObject.Include(e => e.SkillModel).ToList();
                                        foreach (var item in OSkillModelObj)
                                        {
                                            if (OEmp.EmpAcademicInfo != null && OEmp.EmpAcademicInfo.Skill.Count() > 0)
                                            {
                                                List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                ObjectValue OObjVal = new ObjectValue();
                                                foreach (var itemQ in OEmp.EmpAcademicInfo.Skill)
                                                {
                                                    if (itemQ.SkillType != null)
                                                    {
                                                        if (item.SkillModel.LookupVal.ToUpper() == itemQ.SkillType.LookupVal.ToUpper())
                                                        {
                                                            OObjVal = new ObjectValue();
                                                            {
                                                                OObjVal.ObjectVal = itemQ.Name;
                                                                OObjVal.DBTrack = DBTrack;
                                                            };
                                                            OObjVallist.Add(OObjVal);
                                                        }

                                                    }
                                                    
                                                }
                                                SkillModelObjectV = new SkillModelObjectV();
                                                {
                                                    SkillModelObjectV.SkillModelObject = item;
                                                    SkillModelObjectV.DBTrack = DBTrack;
                                                    SkillModelObjectV.ObjectValue = OObjVallist;
                                                };
                                                SkillModelObjectVlist.Add(SkillModelObjectV);
                                            }

                                        }
                                    }
                                }
                                #endregion

                                #region Qualification
                                if (vidsf.CompetencyModel.QualificationModel_Id != null)
                                {
                                    var OEmp = db.Employee.Include(e => e.EmpName).Include(e => e.EmpAcademicInfo).Include(e => e.EmpAcademicInfo.QualificationDetails)
                                                .Include(e => e.EmpAcademicInfo.QualificationDetails.Select(t => t.Qualification))
                                                .Where(e => e.Id == EmppId).FirstOrDefault();

                                    if (OEmp != null)
                                    {
                                        QualificationModel OQualiModel = db.QualificationModel.Include(e => e.QualificationModelObject)
                                                .Include(e => e.QualificationModelObject.Select(t => t.QualificationModel))
                                                .Where(e => e.Id == vidsf.CompetencyModel.QualificationModel_Id).FirstOrDefault();
                                        List<QualificationModelObject> OQualiModelObj = OQualiModel.QualificationModelObject.ToList();
                                        foreach (var item in OQualiModelObj)
                                        {
                                            List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                            ObjectValue OObjVal = new ObjectValue();
                                            if (OEmp.EmpAcademicInfo != null && OEmp.EmpAcademicInfo.QualificationDetails.Count() > 0)
                                            {
                                                foreach (var itemQ in OEmp.EmpAcademicInfo.QualificationDetails)
                                                {
                                                    foreach (var itemQ1 in itemQ.Qualification)
                                                    {
                                                        OObjVal = new ObjectValue();
                                                        {
                                                            OObjVal.ObjectVal = itemQ1.QualificationDesc;
                                                            OObjVal.DBTrack = DBTrack;
                                                        };
                                                        OObjVallist.Add(OObjVal);

                                                    }
                                                }
                                                QualificationModelObjectV = new QualificationModelObjectV();
                                                {
                                                    QualificationModelObjectV.QualificationModelObject = item;
                                                    QualificationModelObjectV.DBTrack = DBTrack;
                                                    QualificationModelObjectV.ObjectValue = OObjVallist;
                                                };
                                                QualificationModelObjectVlist.Add(QualificationModelObjectV);
                                            }

                                        }
                                    }
                                }
                                #endregion

                                #region Training
                                if (vidsf.CompetencyModel.TrainingModel_Id != null)
                                {
                                    var OEmp = db.EmployeeTraining.Include(e => e.TrainingDetails)
                                        .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo))
                                        .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession)))
                                        .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession.TrainingProgramCalendar)))
                                        .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession.TrainingProgramCalendar.ProgramList)))
                                                //.Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule))
                                                //.Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule.TrainingSession))
                                                //.Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule.TrainingSession.Select(t => t.TrainingProgramCalendar)))
                                                //.Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule.TrainingSession.Select(t => t.TrainingProgramCalendar.ProgramList)))
                                                .Where(e => e.Employee_Id == EmppId).FirstOrDefault();
                                    if (OEmp != null)
                                    {
                                        var OTrCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "TRAININGCALENDAR" && e.Default == true).FirstOrDefault();
                                        TrainingModel OTrainingModel = db.TrainingModel.Include(e => e.TrainingModelObject)
                                               .Include(e => e.TrainingModelObject.Select(t => t.TrainingModel))
                                               .Where(e => e.Id == vidsf.CompetencyModel.TrainingModel_Id).FirstOrDefault();
                                        List<TrainingModelObject> OTrainingModelObj = OTrainingModel.TrainingModelObject.ToList();
                                       // List<TrainingModelObject> OTrainingModelObj = db.TrainingModelObject.Include(e => e.TrainingModel).ToList();
                                        foreach (var item in OTrainingModelObj)
                                        {
                                            List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                            ObjectValue OObjVal = new ObjectValue();
                                            if (OEmp.TrainingDetails.Count() > 0)
                                            {
                                                foreach (var itemT in OEmp.TrainingDetails)
                                                {
                                                    foreach (var itemT1 in itemT.TrainigDetailSessionInfo)
                                                    {
                                                        if (itemT1.TrainingSession.TrainingProgramCalendar.StartDate >= OTrCalendar.FromDate && itemT1.TrainingSession.TrainingProgramCalendar.EndDate <= OTrCalendar.ToDate)
                                                        {
                                                            OObjVal = new ObjectValue();
                                                            {
                                                                OObjVal.ObjectVal = itemT1.TrainingSession.TrainingProgramCalendar.ProgramList.Subject;
                                                                OObjVal.DBTrack = DBTrack;
                                                            };
                                                            OObjVallist.Add(OObjVal);
                                                            TrainingModelObjectV = new TrainingModelObjectV();
                                                            {
                                                                TrainingModelObjectV.TrainingModelObject = item;
                                                                TrainingModelObjectV.DBTrack = DBTrack;
                                                                TrainingModelObjectV.ObjectValue = OObjVallist;
                                                            };
                                                            TrainingModelObjectVlist.Add(TrainingModelObjectV);
                                                        }
                                                    }
                                                }

                                            }
                                        }
                                    }
                                }
                                #endregion

                                #region ServiceModel
                                if (vidsf.CompetencyModel.ServiceModel_Id != null)
                                {
                                    Employee OEmp = db.Employee.Where(e => e.Id == EmppId).Include(e => e.EmpName).Include(e => e.ServiceBookDates).FirstOrDefault();
                                    if (OEmp != null)
                                    {
                                        ServiceModel OServiceModel = db.ServiceModel.Include(e => e.ServiceModelObject)
                                              .Include(e => e.ServiceModelObject.Select(t => t.ServiceModel))
                                              .Where(e => e.Id == vidsf.CompetencyModel.ServiceModel_Id).FirstOrDefault();
                                        List<ServiceModelObject> OServModelObj = OServiceModel.ServiceModelObject.ToList();
                                        //List<ServiceModelObject> OServModelObj = db.ServiceModelObject.Include(e => e.ServiceModel).ToList();
                                        foreach (var item in OServModelObj)
                                        {
                                            List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                            ObjectValue OObjVal = new ObjectValue();
                                            DateTime start = (DateTime)OEmp.ServiceBookDates.JoiningDate;
                                            DateTime end = DateTime.Now.Date;
                                            int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);

                                            double daysInEndMonth = (end.AddMonths(1) - end).Days;
                                            double months = compMonth + (start.Date.Day - end.Date.Day) / daysInEndMonth;
                                            double ServYear = Math.Round(months / 12, 2);
                                            OObjVal = new ObjectValue();
                                            {
                                                OObjVal.ObjectVal = ServYear.ToString();
                                                OObjVal.DBTrack = DBTrack;
                                            };
                                            OObjVallist.Add(OObjVal);
                                            ServiceModelObjectV = new ServiceModelObjectV();
                                            {
                                                ServiceModelObjectV.ServiceModelObject = item;
                                                ServiceModelObjectV.DBTrack = DBTrack;
                                                ServiceModelObjectV.ObjectValue = OObjVallist;
                                            };
                                            ServiceModelObjectVlist.Add(ServiceModelObjectV);
                                        }
                                    }
                                }
                                #endregion

                                try
                                {

                                    CompetencyEmployeeDataGeneration CompetencyEmployeeDataGenerationT = new CompetencyEmployeeDataGeneration();
                                    {
                                        CompetencyEmployeeDataGenerationT.Employee = employeep;
                                        CompetencyEmployeeDataGenerationT.BatchName = batchname;
                                        CompetencyEmployeeDataGenerationT.ProcessDate = DateTime.Now;
                                        CompetencyEmployeeDataGenerationT.AppraisalAttributeModelObjectV = AppraisalAttModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.AppraisalBusinessAppraisalModelObjectV = AppraisalBusiModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.AppraisalKRAModelObjectV = AppKRAModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.AppraisalPotentialModelObjectV = AppPotModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.PastExperienceModelObjectV = PastExpModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.PersonnelModelObjectV = PersonnelModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.QualificationModelObjectV = QualificationModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.ServiceModelObjectV = ServiceModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.SkillModelObjectV = SkillModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.TrainingModelObjectV = TrainingModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.DBTrack = DBTrack;
                                    };
                                    //CompetencyEmployeeDataGenerationList.Add(CompetencyEmployeeDataGenerationT);
                                    db.CompetencyEmployeeDataGeneration.Add(CompetencyEmployeeDataGenerationT);
                                    db.SaveChanges();
                                }
                                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                                {
                                    Exception raise = dbEx;
                                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                                    {
                                        foreach (var validationError in validationErrors.ValidationErrors)
                                        {
                                            string message = string.Format("{0}:{1}",
                                                validationErrors.Entry.Entity.ToString(),
                                                validationError.ErrorMessage);
                                            // raise a new exception nesting  
                                            // the current instance as InnerException  
                                            raise = new InvalidOperationException(message, raise);
                                        }
                                    }
                                    throw raise;
                                }
                            }
                        }
                    }
                    #endregion

                    #region Funcstruct
                    if (muidsg != null && muidsg.Count() > 0)
                    {
                        if (muidsg.Select(r => r.FuncStruct_Id) != null)
                        {

                            if (muidsg.Where(e => e.FuncStruct_Id != null).Count() > 0)
                            {

                                List<int?> FunId = muidsg.Select(t => t.FuncStruct_Id).ToList();

                                var EmpListf = db.Employee.Include(e => e.ServiceBookDates).Include(e => e.FuncStruct).Where(e => FunId.Contains(e.PayStruct.Id) && e.ServiceBookDates.ServiceLastDate == null).ToList().Select(r => r.Id);

                                foreach (var EmpfId in EmpListf)
                                {
                                    #region PersModel
                                    if (vidsf.CompetencyModel.PersonnelModel_Id != null)
                                    {
                                        Employee OEmp = db.Employee.Where(e => e.Id == EmpfId).Include(e => e.MaritalStatus)
                                                       .Include(e => e.Gender).Include(e => e.EmpName).Include(e => e.ServiceBookDates).FirstOrDefault();
                                        if (OEmp != null)
                                        {
                                            PersonnelModel OPersModel = db.PersonnelModel.Include(e => e.PersonnelModelObject)
                                                  .Include(e => e.PersonnelModelObject.Select(t => t.PersonnelModel))
                                                  .Where(e => e.Id == vidsf.CompetencyModel.PersonnelModel_Id).FirstOrDefault();
                                            List<PersonnelModelObject> OPersModelObj = OPersModel.PersonnelModelObject.ToList();
                                            //List<PersonnelModelObject> OPersModelObj = db.PersonnelModelObject.Include(e => e.PersonnelModel).ToList();
                                            foreach (var item in OPersModelObj.ToList())
                                            {
                                                List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                ObjectValue OObjVal = new ObjectValue();
                                                if (item.PersonnelModel.LookupVal.ToUpper() == "JOININGDATE")
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OEmp.ServiceBookDates.JoiningDate.Value.ToShortDateString();
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PersonnelModelObjectV = new PersonnelModelObjectV();
                                                    {
                                                        PersonnelModelObjectV.PersonnelModelObject = item;
                                                        PersonnelModelObjectV.DBTrack = DBTrack;
                                                        PersonnelModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PersonnelModelObjectVlist.Add(PersonnelModelObjectV);
                                                }
                                                if (item.PersonnelModel.LookupVal.ToUpper() == "CONFIRMATIONDATE")
                                                {

                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OEmp.ServiceBookDates.ConfirmationDate.Value.ToShortDateString();
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PersonnelModelObjectV = new PersonnelModelObjectV();
                                                    {
                                                        PersonnelModelObjectV.PersonnelModelObject = item;
                                                        PersonnelModelObjectV.DBTrack = DBTrack;
                                                        PersonnelModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PersonnelModelObjectVlist.Add(PersonnelModelObjectV);

                                                }
                                                if (item.PersonnelModel.LookupVal.ToUpper() == "RETIREMENTDATE")
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OEmp.ServiceBookDates.RetirementDate.Value.ToShortDateString();
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PersonnelModelObjectV = new PersonnelModelObjectV();
                                                    {
                                                        PersonnelModelObjectV.PersonnelModelObject = item;
                                                        PersonnelModelObjectV.DBTrack = DBTrack;
                                                        PersonnelModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PersonnelModelObjectVlist.Add(PersonnelModelObjectV);
                                                }
                                                if (item.PersonnelModel.LookupVal.ToUpper() == "BIRTHDATE")
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OEmp.ServiceBookDates.BirthDate.Value.ToShortDateString();
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PersonnelModelObjectV = new PersonnelModelObjectV();
                                                    {
                                                        PersonnelModelObjectV.PersonnelModelObject = item;
                                                        PersonnelModelObjectV.DBTrack = DBTrack;
                                                        PersonnelModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PersonnelModelObjectVlist.Add(PersonnelModelObjectV);
                                                }
                                                if (item.PersonnelModel.LookupVal.ToUpper() == "PROBATIONDATE")
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OEmp.ServiceBookDates.ProbationDate.Value.ToShortDateString();
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PersonnelModelObjectV = new PersonnelModelObjectV();
                                                    {
                                                        PersonnelModelObjectV.PersonnelModelObject = item;
                                                        PersonnelModelObjectV.DBTrack = DBTrack;
                                                        PersonnelModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PersonnelModelObjectVlist.Add(PersonnelModelObjectV);
                                                }
                                                if (item.PersonnelModel.LookupVal.ToUpper() == "GENDER")
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OEmp.Gender.LookupVal;
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PersonnelModelObjectV = new PersonnelModelObjectV();
                                                    {
                                                        PersonnelModelObjectV.PersonnelModelObject = item;
                                                        PersonnelModelObjectV.DBTrack = DBTrack;
                                                        PersonnelModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PersonnelModelObjectVlist.Add(PersonnelModelObjectV);

                                                }
                                                if (item.PersonnelModel.LookupVal.ToUpper() == "MARITALSTATUS")
                                                {
                                                    if (OEmp.MaritalStatus != null)
                                                    {
                                                        OObjVal = new ObjectValue();
                                                        {
                                                            OObjVal.ObjectVal = OEmp.MaritalStatus.LookupVal;
                                                            OObjVal.DBTrack = DBTrack;
                                                        };
                                                        OObjVallist.Add(OObjVal);
                                                        PersonnelModelObjectV = new PersonnelModelObjectV();
                                                        {
                                                            PersonnelModelObjectV.PersonnelModelObject = item;
                                                            PersonnelModelObjectV.DBTrack = DBTrack;
                                                            PersonnelModelObjectV.ObjectValue = OObjVallist;
                                                        };
                                                        PersonnelModelObjectVlist.Add(PersonnelModelObjectV);
                                                    }

                                                }

                                            }
                                        }

                                    }
                                    #endregion

                                    #region AppraisalAtt
                                    if (vidsf.CompetencyModel.AppraisalAttributeModel_Id != null)
                                    {
                                        var OEmp = db.EmployeeAppraisal.Where(e => e.Employee.Id == EmpfId)
                                            .Include(e => e.EmpAppEvaluation).Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion))
                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating)))
                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment))))
                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment.AppCategory)))).FirstOrDefault();

                                        if (OEmp != null)
                                        {
                                            AppraisalAttributeModel OAppraisalAttModel = db.AppraisalAttributeModel.Include(e => e.AppraisalAttributeModelObject)
                                                  .Include(e => e.AppraisalAttributeModelObject.Select(t => t.AppraisalAttributeModel))
                                                  .Where(e => e.Id == vidsf.CompetencyModel.AppraisalAttributeModel_Id).FirstOrDefault();
                                            List<AppraisalAttributeModelObject> OAttrModelObj = OAppraisalAttModel.AppraisalAttributeModelObject.ToList();
                                            // List<AppraisalAttributeModelObject> OAttrModelObj = db.AppraisalAttributeModelObject.Include(e => e.AppraisalAttributeModel).ToList();
                                            foreach (var item in OAttrModelObj)
                                            {
                                                List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                ObjectValue OObjVal = new ObjectValue();
                                                foreach (var APP1 in OEmp.EmpAppEvaluation)
                                                {
                                                    foreach (var APP2 in APP1.EmpAppRatingConclusion)
                                                    {
                                                        foreach (var APP3 in APP2.EmpAppRating.Where(t => t.AppAssignment.AppCategory.AppMode.LookupVal.ToUpper() == "ATTRIBUTE"))
                                                        {

                                                            if (item.AppraisalAttributeModel.LookupVal.ToUpper() == APP3.AppAssignment.AppCategory.Code)
                                                            {
                                                                OObjVal = new ObjectValue();
                                                                {
                                                                    OObjVal.ObjectVal = APP3.RatingPoints.ToString();
                                                                    OObjVal.DBTrack = DBTrack;
                                                                };
                                                                OObjVallist.Add(OObjVal);
                                                                AppraisalAttModelObjectV = new AppraisalAttributeModelObjectV();
                                                                {
                                                                    AppraisalAttModelObjectV.AppraisalAttributeModelObject = item;
                                                                    AppraisalAttModelObjectV.DBTrack = DBTrack;
                                                                    AppraisalAttModelObjectV.ObjectValue = OObjVallist;
                                                                };
                                                                AppraisalAttModelObjectVlist.Add(AppraisalAttModelObjectV);
                                                            }
                                                        }
                                                    }
                                                }

                                            }
                                        }


                                    }
                                    #endregion

                                    #region AppraisalPotential
                                    if (vidsf.CompetencyModel.AppraisalPotentialModel_Id != null)
                                    {
                                        var OEmp = db.EmployeeAppraisal.Where(e => e.Employee.Id == EmpfId)
                                            .Include(e => e.EmpAppEvaluation).Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion))
                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating)))
                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment))))
                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment.AppCategory)))).FirstOrDefault();

                                        if (OEmp != null)
                                        {
                                            AppraisalPotentialModel OAppraisalPotModel = db.AppraisalPotentialModel.Include(e => e.AppraisalPotentialModelObject)
                                                  .Include(e => e.AppraisalPotentialModelObject.Select(t => t.AppraisalPotentialModel))
                                                  .Where(e => e.Id == vidsf.CompetencyModel.AppraisalPotentialModel_Id).FirstOrDefault();
                                            List<AppraisalPotentialModelObject> OPotModelObj = OAppraisalPotModel.AppraisalPotentialModelObject.ToList();
                                            // List<AppraisalAttributeModelObject> OAttrModelObj = db.AppraisalAttributeModelObject.Include(e => e.AppraisalAttributeModel).ToList();
                                            foreach (var item in OPotModelObj)
                                            {
                                                List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                ObjectValue OObjVal = new ObjectValue();
                                                foreach (var APP1 in OEmp.EmpAppEvaluation)
                                                {
                                                    foreach (var APP2 in APP1.EmpAppRatingConclusion)
                                                    {
                                                        foreach (var APP3 in APP2.EmpAppRating.Where(t => t.AppAssignment.AppCategory.AppMode.LookupVal.ToUpper() == "POTENTIAL"))
                                                        {

                                                            if (item.AppraisalPotentialModel.LookupVal.ToUpper() == APP3.AppAssignment.AppCategory.Code)
                                                            {
                                                                OObjVal = new ObjectValue();
                                                                {
                                                                    OObjVal.ObjectVal = APP3.RatingPoints.ToString();
                                                                    OObjVal.DBTrack = DBTrack;
                                                                };
                                                                OObjVallist.Add(OObjVal);
                                                                AppPotModelObjectV = new AppraisalPotentialModelObjectV();
                                                                {
                                                                    AppPotModelObjectV.AppraisalPotentialModelObject = item;
                                                                    AppPotModelObjectV.DBTrack = DBTrack;
                                                                    AppPotModelObjectV.ObjectValue = OObjVallist;
                                                                };
                                                                AppPotModelObjectVlist.Add(AppPotModelObjectV);
                                                            }
                                                        }
                                                    }
                                                }

                                            }
                                        }


                                    }
                                    #endregion

                                    #region AppraisalKRA
                                    if (vidsf.CompetencyModel.AppraisalKRAModel_Id != null)
                                    {
                                        var OEmp = db.EmployeeAppraisal.Where(e => e.Employee.Id == EmpfId)
                                            .Include(e => e.EmpAppEvaluation).Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion))
                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating)))
                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment))))
                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment.AppCategory)))).FirstOrDefault();

                                        if (OEmp != null)
                                        {
                                            AppraisalKRAModel OAppraisalKRAModel = db.AppraisalKRAModel.Include(e => e.AppraisalKRAModelObject)
                                                  .Include(e => e.AppraisalKRAModelObject.Select(t => t.AppraisalKRAModel))
                                                  .Where(e => e.Id == vidsf.CompetencyModel.AppraisalPotentialModel_Id).FirstOrDefault();
                                            List<AppraisalKRAModelObject> OKRAModelObj = OAppraisalKRAModel.AppraisalKRAModelObject.ToList();
                                            // List<AppraisalAttributeModelObject> OAttrModelObj = db.AppraisalAttributeModelObject.Include(e => e.AppraisalAttributeModel).ToList();
                                            foreach (var item in OKRAModelObj)
                                            {
                                                List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                ObjectValue OObjVal = new ObjectValue();
                                                foreach (var APP1 in OEmp.EmpAppEvaluation)
                                                {
                                                    foreach (var APP2 in APP1.EmpAppRatingConclusion)
                                                    {
                                                        foreach (var APP3 in APP2.EmpAppRating.Where(t => t.AppAssignment.AppCategory.AppMode.LookupVal.ToUpper() == "KRA"))
                                                        {

                                                            if (item.AppraisalKRAModel.LookupVal.ToUpper() == APP3.AppAssignment.AppCategory.Code)
                                                            {
                                                                OObjVal = new ObjectValue();
                                                                {
                                                                    OObjVal.ObjectVal = APP3.RatingPoints.ToString();
                                                                    OObjVal.DBTrack = DBTrack;
                                                                };
                                                                OObjVallist.Add(OObjVal);
                                                                AppKRAModelObjectV = new AppraisalKRAModelObjectV();
                                                                {
                                                                    AppKRAModelObjectV.AppraisalKRAModelObject = item;
                                                                    AppKRAModelObjectV.DBTrack = DBTrack;
                                                                    AppKRAModelObjectV.ObjectValue = OObjVallist;
                                                                };
                                                                AppKRAModelObjectVlist.Add(AppKRAModelObjectV);
                                                            }
                                                        }
                                                    }
                                                }

                                            }
                                        }


                                    }
                                    #endregion

                                    #region BusinessAppraisal
                                    #endregion

                                    #region PastExperience
                                    if (vidsf.CompetencyModel.PastExperienceModel_Id != null)
                                    {
                                        var OPrevEx = db.Employee.Where(e => e.Id == EmpfId)
                                                         .Include(e => e.PreCompExp).Include(e => e.PreCompExp.Select(t => t.ExperienceDetails)).FirstOrDefault()
                                                         .PreCompExp.OrderByDescending(e => e.FromDate).FirstOrDefault();
                                        if (OPrevEx != null)
                                        {
                                            PastExperienceModel OPastExpModel = db.PastExperienceModel.Include(e => e.PastExperienceModelObject)
                                                  .Include(e => e.PastExperienceModelObject.Select(t => t.PastExperienceModel))
                                                  .Where(e => e.Id == vidsf.CompetencyModel.PastExperienceModel_Id).FirstOrDefault();
                                            List<PastExperienceModelObject> OPastExpModelObj = OPastExpModel.PastExperienceModelObject.ToList();
                                            //List<PastExperienceModelObject> OPastExpModelObj = db.PastExperienceModelObject.Include(e => e.PastExperienceModel)
                                            //    .ToList();
                                            foreach (var item in OPastExpModelObj)
                                            {
                                                List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                ObjectValue OObjVal = new ObjectValue();
                                                if (item.PastExperienceModel.LookupVal.ToUpper() == "LEAVINGJOBPOSITION")
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OPrevEx.LeaveingJobPosition;
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PastExpModelObjectV = new PastExperienceModelObjectV();
                                                    {
                                                        PastExpModelObjectV.PastExperienceModelObject = item;
                                                        PastExpModelObjectV.DBTrack = DBTrack;
                                                        PastExpModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PastExpModelObjectVlist.Add(PastExpModelObjectV);
                                                }
                                                if (item.PastExperienceModel.LookupVal.ToUpper() == "JOININGJOBPOSITION")
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OPrevEx.JoiningJobPosition;
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PastExpModelObjectV = new PastExperienceModelObjectV();
                                                    {
                                                        PastExpModelObjectV.PastExperienceModelObject = item;
                                                        PastExpModelObjectV.DBTrack = DBTrack;
                                                        PastExpModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PastExpModelObjectVlist.Add(PastExpModelObjectV);
                                                }
                                                if (item.PastExperienceModel.LookupVal.ToUpper() == "YROFSERVICE")
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OPrevEx.YrOfService.ToString();
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PastExpModelObjectV = new PastExperienceModelObjectV();
                                                    {
                                                        PastExpModelObjectV.PastExperienceModelObject = item;
                                                        PastExpModelObjectV.DBTrack = DBTrack;
                                                        PastExpModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PastExpModelObjectVlist.Add(PastExpModelObjectV);
                                                }
                                                if (item.PastExperienceModel.LookupVal.ToUpper() == "SPECIALACHIEVEMENTS")
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OPrevEx.SpecialAchievements;
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PastExpModelObjectV = new PastExperienceModelObjectV();
                                                    {
                                                        PastExpModelObjectV.PastExperienceModelObject = item;
                                                        PastExpModelObjectV.DBTrack = DBTrack;
                                                        PastExpModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PastExpModelObjectVlist.Add(PastExpModelObjectV);
                                                }
                                                if (item.PastExperienceModel.LookupVal.ToUpper() == "ExperienceDetails")
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = OPrevEx.ExperienceDetails.LookupVal;
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                    PastExpModelObjectV = new PastExperienceModelObjectV();
                                                    {
                                                        PastExpModelObjectV.PastExperienceModelObject = item;
                                                        PastExpModelObjectV.DBTrack = DBTrack;
                                                        PastExpModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    PastExpModelObjectVlist.Add(PastExpModelObjectV);
                                                }
                                            }

                                        }
                                    }

                                    #endregion

                                    #region SkillModel
                                    if (vidsf.CompetencyModel.SkillModel_Id != null)
                                    {
                                        var OEmp = db.Employee.Include(e => e.EmpName).Include(e => e.EmpAcademicInfo).Include(e => e.EmpAcademicInfo.Skill)
                                            .Include(e => e.EmpAcademicInfo.Skill.Select(t => t.SkillType))
                                                    .Where(e => e.Id == EmpfId).FirstOrDefault();
                                        if (OEmp != null)
                                        {
                                            SkillModel OSkillModel = db.SkillModel.Include(e => e.SkillModelObject)
                                                  .Include(e => e.SkillModelObject.Select(t => t.SkillModel))
                                                  .Where(e => e.Id == vidsf.CompetencyModel.SkillModel_Id).FirstOrDefault();
                                            List<SkillModelObject> OSkillModelObj = OSkillModel.SkillModelObject.ToList();
                                            // List<SkillModelObject> OSkillModelObj = db.SkillModelObject.Include(e => e.SkillModel).ToList();
                                            foreach (var item in OSkillModelObj)
                                            {
                                                if (OEmp.EmpAcademicInfo != null && OEmp.EmpAcademicInfo.Skill.Count() > 0)
                                                {
                                                    List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                    ObjectValue OObjVal = new ObjectValue();
                                                    foreach (var itemQ in OEmp.EmpAcademicInfo.Skill)
                                                    {
                                                        if (itemQ.SkillType != null)
                                                        {
                                                            if (item.SkillModel.LookupVal.ToUpper() == itemQ.SkillType.LookupVal.ToUpper())
                                                            {
                                                                OObjVal = new ObjectValue();
                                                                {
                                                                    OObjVal.ObjectVal = itemQ.Name;
                                                                    OObjVal.DBTrack = DBTrack;
                                                                };
                                                                OObjVallist.Add(OObjVal);
                                                            }

                                                        }

                                                    }
                                                    SkillModelObjectV = new SkillModelObjectV();
                                                    {
                                                        SkillModelObjectV.SkillModelObject = item;
                                                        SkillModelObjectV.DBTrack = DBTrack;
                                                        SkillModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    SkillModelObjectVlist.Add(SkillModelObjectV);
                                                }

                                            }
                                        }
                                    }
                                    #endregion

                                    #region Qualification
                                    if (vidsf.CompetencyModel.QualificationModel_Id != null)
                                    {
                                        var OEmp = db.Employee.Include(e => e.EmpName).Include(e => e.EmpAcademicInfo).Include(e => e.EmpAcademicInfo.QualificationDetails)
                                                    .Include(e => e.EmpAcademicInfo.QualificationDetails.Select(t => t.Qualification))
                                                    .Where(e => e.Id == EmpfId).FirstOrDefault();

                                        if (OEmp != null)
                                        {
                                            QualificationModel OQualiModel = db.QualificationModel.Include(e => e.QualificationModelObject)
                                                    .Include(e => e.QualificationModelObject.Select(t => t.QualificationModel))
                                                    .Where(e => e.Id == vidsf.CompetencyModel.QualificationModel_Id).FirstOrDefault();
                                            List<QualificationModelObject> OQualiModelObj = OQualiModel.QualificationModelObject.ToList();
                                            foreach (var item in OQualiModelObj)
                                            {
                                                List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                ObjectValue OObjVal = new ObjectValue();
                                                if (OEmp.EmpAcademicInfo != null && OEmp.EmpAcademicInfo.QualificationDetails.Count() > 0)
                                                {
                                                    foreach (var itemQ in OEmp.EmpAcademicInfo.QualificationDetails)
                                                    {
                                                        foreach (var itemQ1 in itemQ.Qualification)
                                                        {
                                                            OObjVal = new ObjectValue();
                                                            {
                                                                OObjVal.ObjectVal = itemQ1.QualificationDesc;
                                                                OObjVal.DBTrack = DBTrack;
                                                            };
                                                            OObjVallist.Add(OObjVal);

                                                        }
                                                    }
                                                    QualificationModelObjectV = new QualificationModelObjectV();
                                                    {
                                                        QualificationModelObjectV.QualificationModelObject = item;
                                                        QualificationModelObjectV.DBTrack = DBTrack;
                                                        QualificationModelObjectV.ObjectValue = OObjVallist;
                                                    };
                                                    QualificationModelObjectVlist.Add(QualificationModelObjectV);
                                                }

                                            }
                                        }
                                    }
                                    #endregion

                                    #region Training
                                    if (vidsf.CompetencyModel.TrainingModel_Id != null)
                                    {
                                        var OEmp = db.EmployeeTraining.Include(e => e.TrainingDetails)
                                            .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo))
                                            .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession)))
                                            .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession.TrainingProgramCalendar)))
                                            .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession.TrainingProgramCalendar.ProgramList)))
                                            //.Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule))
                                            //.Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule.TrainingSession))
                                            //.Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule.TrainingSession.Select(t => t.TrainingProgramCalendar)))
                                            //.Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule.TrainingSession.Select(t => t.TrainingProgramCalendar.ProgramList)))
                                                    .Where(e => e.Employee_Id == EmpfId).FirstOrDefault();
                                        if (OEmp != null)
                                        {
                                            var OTrCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "TRAININGCALENDAR" && e.Default == true).FirstOrDefault();
                                            TrainingModel OTrainingModel = db.TrainingModel.Include(e => e.TrainingModelObject)
                                                   .Include(e => e.TrainingModelObject.Select(t => t.TrainingModel))
                                                   .Where(e => e.Id == vidsf.CompetencyModel.TrainingModel_Id).FirstOrDefault();
                                            List<TrainingModelObject> OTrainingModelObj = OTrainingModel.TrainingModelObject.ToList();
                                            // List<TrainingModelObject> OTrainingModelObj = db.TrainingModelObject.Include(e => e.TrainingModel).ToList();
                                            foreach (var item in OTrainingModelObj)
                                            {
                                                List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                ObjectValue OObjVal = new ObjectValue();
                                                if (OEmp.TrainingDetails.Count() > 0)
                                                {
                                                    foreach (var itemT in OEmp.TrainingDetails)
                                                    {
                                                        foreach (var itemT1 in itemT.TrainigDetailSessionInfo)
                                                        {
                                                            if (itemT1.TrainingSession.TrainingProgramCalendar.StartDate >= OTrCalendar.FromDate && itemT1.TrainingSession.TrainingProgramCalendar.EndDate <= OTrCalendar.ToDate)
                                                            {
                                                                OObjVal = new ObjectValue();
                                                                {
                                                                    OObjVal.ObjectVal = itemT1.TrainingSession.TrainingProgramCalendar.ProgramList.Subject;
                                                                    OObjVal.DBTrack = DBTrack;
                                                                };
                                                                OObjVallist.Add(OObjVal);
                                                                TrainingModelObjectV = new TrainingModelObjectV();
                                                                {
                                                                    TrainingModelObjectV.TrainingModelObject = item;
                                                                    TrainingModelObjectV.DBTrack = DBTrack;
                                                                    TrainingModelObjectV.ObjectValue = OObjVallist;
                                                                };
                                                                TrainingModelObjectVlist.Add(TrainingModelObjectV);
                                                            }
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                    }
                                    #endregion

                                    #region ServiceModel
                                    if (vidsf.CompetencyModel.ServiceModel_Id != null)
                                    {
                                        Employee OEmp = db.Employee.Where(e => e.Id == EmpfId).Include(e => e.EmpName).Include(e => e.ServiceBookDates).FirstOrDefault();
                                        if (OEmp != null)
                                        {
                                            ServiceModel OServiceModel = db.ServiceModel.Include(e => e.ServiceModelObject)
                                                  .Include(e => e.ServiceModelObject.Select(t => t.ServiceModel))
                                                  .Where(e => e.Id == vidsf.CompetencyModel.ServiceModel_Id).FirstOrDefault();
                                            List<ServiceModelObject> OServModelObj = OServiceModel.ServiceModelObject.ToList();
                                            //List<ServiceModelObject> OServModelObj = db.ServiceModelObject.Include(e => e.ServiceModel).ToList();
                                            foreach (var item in OServModelObj)
                                            {
                                                List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                ObjectValue OObjVal = new ObjectValue();
                                                DateTime start = (DateTime)OEmp.ServiceBookDates.JoiningDate;
                                                DateTime end = DateTime.Now.Date;
                                                int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);

                                                double daysInEndMonth = (end.AddMonths(1) - end).Days;
                                                double months = compMonth + (start.Date.Day - end.Date.Day) / daysInEndMonth;
                                                double ServYear = Math.Round(months / 12, 2);
                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = ServYear.ToString();
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);
                                                ServiceModelObjectV = new ServiceModelObjectV();
                                                {
                                                    ServiceModelObjectV.ServiceModelObject = item;
                                                    ServiceModelObjectV.DBTrack = DBTrack;
                                                    ServiceModelObjectV.ObjectValue = OObjVallist;
                                                };
                                                ServiceModelObjectVlist.Add(ServiceModelObjectV);
                                            }
                                        }
                                    }
                                    #endregion


                                    Employee employeef = db.Employee.Find(EmpfId);
                                   // CompetencyModelAssignment batchname = db.CompetencyModelAssignment.Find(BatchIds);
                                    CompetencyEmployeeDataGeneration CompetencyEmployeeDataGenerationT = new CompetencyEmployeeDataGeneration();
                                    {
                                        CompetencyEmployeeDataGenerationT.Employee = employeef;
                                        CompetencyEmployeeDataGenerationT.BatchName = batchname;
                                        CompetencyEmployeeDataGenerationT.ProcessDate = DateTime.Now;
                                        CompetencyEmployeeDataGenerationT.AppraisalAttributeModelObjectV = AppraisalAttModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.AppraisalBusinessAppraisalModelObjectV = AppraisalBusiModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.AppraisalKRAModelObjectV = AppKRAModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.AppraisalPotentialModelObjectV = AppPotModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.PastExperienceModelObjectV = PastExpModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.PersonnelModelObjectV = PersonnelModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.QualificationModelObjectV = QualificationModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.ServiceModelObjectV = ServiceModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.SkillModelObjectV = SkillModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.TrainingModelObjectV = TrainingModelObjectVlist;
                                        CompetencyEmployeeDataGenerationT.DBTrack = DBTrack;
                                    };
                                    db.CompetencyEmployeeDataGeneration.Add(CompetencyEmployeeDataGenerationT);
                                    db.SaveChanges();
                                    //CompetencyEmployeeDataGenerationList.Add(CompetencyEmployeeDataGenerationT);
                                }
                            }
                        }
                    }
                    #endregion
                    try 
                    {
                       
                        ts.Complete();
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                    {
                        Exception raise = dbEx;
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                string message = string.Format("{0}:{1}",
                                    validationErrors.Entry.Entity.ToString(),
                                    validationError.ErrorMessage);
                                // raise a new exception nesting  
                                // the current instance as InnerException  
                                raise = new InvalidOperationException(message, raise);
                            }
                        }
                        throw raise;
                    }
                    
                }
                //TempData["tempdata"] = CompetencyEmployeeDataGenerationList.Select(r => r.BatchName.Id).FirstOrDefault();

                return Json(new { status = 0, MSG = "Employee Data Generated Successfully." }, JsonRequestBehavior.AllowGet);

            }
        }




        public ActionResult GetEmpDataT(string EvaluationLookupIds, string modelObjecttype)
        {

            int EvaluationLookupIdss = Convert.ToInt32(EvaluationLookupIds);

            var dates = new Dictionary<string, string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                switch (modelObjecttype)
                {
                    case "AppraisalAttributeModel":

                        int SuccessionmodelIdA = db.CompetencyModel.Include(e => e.AppraisalAttributeModel)
                                               .Include(e => e.AppraisalAttributeModel.AppraisalAttributeModelObject)
                                               .Include(e => e.AppraisalAttributeModel.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel))
                                               .Include(e => e.AppraisalAttributeModel.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                                               .Include(e => e.AppraisalAttributeModel.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                                               .Include(e => e.AppraisalAttributeModel.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                                               .Where(e => e.AppraisalAttributeModel.AppraisalAttributeModelObject.FirstOrDefault().CompetencyEvaluationModel.Id == EvaluationLookupIdss).FirstOrDefault().Id;

                        var vidsa = db.CompetencyModelAssignment.Include(e => e.CompetencyModelAssignment_OrgStructure).
                        Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.GeoStruct)).
                        Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.PayStruct)).
                        Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.FuncStruct)).Where(e => e.CompetencyModel.Id == SuccessionmodelIdA).FirstOrDefault();
                        var muidsa = vidsa.CompetencyModelAssignment_OrgStructure.ToList();

                        if (muidsa != null && muidsa.Count() > 0)
                        {
                            var mUidg = muidsa.Select(e => e.GeoStruct).FirstOrDefault();

                            if (mUidg != null)
                            {
                                int GeoId = mUidg.Id;
                                var EmpListg = db.Employee.Include(e => e.EmpName).Where(t => t.GeoStruct_Id == GeoId).ToList();

                                foreach (var item in EmpListg)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }



                        }
                        if (muidsa != null && muidsa.Count() > 0)
                        {
                            var mUidp = muidsa.Select(e => e.PayStruct).FirstOrDefault();

                            if (mUidp != null)
                            {
                                int PeoId = mUidp.Id;
                                var EmpListp = db.Employee.Include(e => e.EmpName).Where(t => t.PayStruct_Id == PeoId).ToList();

                                foreach (var item in EmpListp)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }

                        }
                        if (muidsa != null && muidsa.Count() > 0)
                        {
                            var mUidf = muidsa.Select(e => e.FuncStruct).FirstOrDefault();

                            if (mUidf != null)
                            {
                                int FeoId = mUidf.Id;
                                var EmpListf = db.Employee.Include(e => e.EmpName).Where(t => t.GeoStruct_Id == FeoId).ToList();

                                foreach (var item in EmpListf)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }

                        }
                        return Json(new { dates }, JsonRequestBehavior.AllowGet);

                        break;

                    case "AppraisalBusinessApprisalModel":

                        int SuccessionmodelIdB = db.CompetencyModel.Include(e => e.AppraisalBusinessApprisalModel)
                                  .Include(e => e.AppraisalBusinessApprisalModel.AppraisalBusinessAppraisalModelObject)
                                  .Include(e => e.AppraisalBusinessApprisalModel.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel))
                                  .Include(e => e.AppraisalBusinessApprisalModel.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                                  .Include(e => e.AppraisalBusinessApprisalModel.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                                  .Include(e => e.AppraisalBusinessApprisalModel.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                                  .Where(e => e.AppraisalBusinessApprisalModel.AppraisalBusinessAppraisalModelObject.FirstOrDefault().CompetencyEvaluationModel.Id == EvaluationLookupIdss).FirstOrDefault().Id;

                        var vidsb = db.CompetencyModelAssignment.Include(e => e.CompetencyModelAssignment_OrgStructure).
                        Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.GeoStruct)).
                        Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.PayStruct)).
                        Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.FuncStruct)).Where(e => e.CompetencyModel.Id == SuccessionmodelIdB).FirstOrDefault();
                        var muidsb = vidsb.CompetencyModelAssignment_OrgStructure.ToList();

                        if (muidsb != null && muidsb.Count() > 0)
                        {
                            var mUidg = muidsb.Select(e => e.GeoStruct).FirstOrDefault();

                            if (mUidg != null)
                            {
                                int GeoId = mUidg.Id;
                                var EmpListg = db.Employee.Include(e => e.EmpName).Where(t => t.GeoStruct_Id == GeoId).ToList();

                                foreach (var item in EmpListg)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }



                        }
                        if (muidsb != null && muidsb.Count() > 0)
                        {
                            var mUidp = muidsb.Select(e => e.PayStruct).FirstOrDefault();

                            if (mUidp != null)
                            {
                                int PeoId = mUidp.Id;
                                var EmpListp = db.Employee.Include(e => e.EmpName).Where(t => t.PayStruct_Id == PeoId).ToList();

                                foreach (var item in EmpListp)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }

                        }
                        if (muidsb != null && muidsb.Count() > 0)
                        {
                            var mUidf = muidsb.Select(e => e.FuncStruct).FirstOrDefault();

                            if (mUidf != null)
                            {
                                int FeoId = mUidf.Id;
                                var EmpListf = db.Employee.Include(e => e.EmpName).Where(t => t.GeoStruct_Id == FeoId).ToList();

                                foreach (var item in EmpListf)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }

                        }
                        return Json(new { dates }, JsonRequestBehavior.AllowGet);
                        break;



                    case "AppraisalKRAModel":

                        int SuccessionmodelIdK = db.CompetencyModel.Include(e => e.AppraisalKRAModel)
                                  .Include(e => e.AppraisalKRAModel.AppraisalKRAModelObject)
                                  .Include(e => e.AppraisalKRAModel.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel))
                                  .Include(e => e.AppraisalKRAModel.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                                  .Include(e => e.AppraisalKRAModel.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                                  .Include(e => e.AppraisalKRAModel.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                                  .Where(e => e.AppraisalKRAModel.AppraisalKRAModelObject.FirstOrDefault().CompetencyEvaluationModel.Id == EvaluationLookupIdss).FirstOrDefault().Id;

                        var vidsk = db.CompetencyModelAssignment.Include(e => e.CompetencyModelAssignment_OrgStructure).
                       Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.GeoStruct)).
                       Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.PayStruct)).
                       Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.FuncStruct)).Where(e => e.CompetencyModel.Id == SuccessionmodelIdK).FirstOrDefault();
                        var muidsk = vidsk.CompetencyModelAssignment_OrgStructure.ToList();

                        if (muidsk != null && muidsk.Count() > 0)
                        {
                            var mUidg = muidsk.Select(e => e.GeoStruct).FirstOrDefault();

                            if (mUidg != null)
                            {
                                int GeoId = mUidg.Id;
                                var EmpListg = db.Employee.Include(e => e.EmpName).Where(t => t.GeoStruct_Id == GeoId).ToList();

                                foreach (var item in EmpListg)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }



                        }
                        if (muidsk != null && muidsk.Count() > 0)
                        {
                            var mUidp = muidsk.Select(e => e.PayStruct).FirstOrDefault();

                            if (mUidp != null)
                            {
                                int PeoId = mUidp.Id;
                                var EmpListp = db.Employee.Include(e => e.EmpName).Where(t => t.PayStruct_Id == PeoId).ToList();

                                foreach (var item in EmpListp)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }

                        }
                        if (muidsk != null && muidsk.Count() > 0)
                        {
                            var mUidf = muidsk.Select(e => e.FuncStruct).FirstOrDefault();

                            if (mUidf != null)
                            {
                                int FeoId = mUidf.Id;
                                var EmpListf = db.Employee.Include(e => e.EmpName).Where(t => t.GeoStruct_Id == FeoId).ToList();

                                foreach (var item in EmpListf)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }

                        }
                        return Json(new { dates }, JsonRequestBehavior.AllowGet);
                        break;

                    case "AppraisalPotentialModel":

                        int SuccessionmodelIdPo = db.CompetencyModel.Include(e => e.AppraisalPotentialModel)
                                  .Include(e => e.AppraisalPotentialModel.AppraisalPotentialModelObject)
                                  .Include(e => e.AppraisalPotentialModel.AppraisalPotentialModelObject.Select(r => r.CompetencyEvaluationModel))
                                  .Include(e => e.AppraisalPotentialModel.AppraisalPotentialModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                                  .Include(e => e.AppraisalPotentialModel.AppraisalPotentialModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                                  .Include(e => e.AppraisalPotentialModel.AppraisalPotentialModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                                  .Where(e => e.AppraisalPotentialModel.AppraisalPotentialModelObject.FirstOrDefault().CompetencyEvaluationModel.Id == EvaluationLookupIdss).FirstOrDefault().Id;

                        var vidspo = db.CompetencyModelAssignment.Include(e => e.CompetencyModelAssignment_OrgStructure).
                        Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.GeoStruct)).
                        Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.PayStruct)).
                        Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.FuncStruct)).Where(e => e.CompetencyModel.Id == SuccessionmodelIdPo).FirstOrDefault();
                        var muidspo = vidspo.CompetencyModelAssignment_OrgStructure.ToList();

                        if (muidspo != null && muidspo.Count() > 0)
                        {
                            var mUidg = muidspo.Select(e => e.GeoStruct).FirstOrDefault();

                            if (mUidg != null)
                            {
                                int GeoId = mUidg.Id;
                                var EmpListg = db.Employee.Include(e => e.EmpName).Where(t => t.GeoStruct_Id == GeoId).ToList();

                                foreach (var item in EmpListg)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }



                        }
                        if (muidspo != null && muidspo.Count() > 0)
                        {
                            var mUidp = muidspo.Select(e => e.PayStruct).FirstOrDefault();

                            if (mUidp != null)
                            {
                                int PeoId = mUidp.Id;
                                var EmpListp = db.Employee.Include(e => e.EmpName).Where(t => t.PayStruct_Id == PeoId).ToList();

                                foreach (var item in EmpListp)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }

                        }
                        if (muidspo != null && muidspo.Count() > 0)
                        {
                            var mUidf = muidspo.Select(e => e.FuncStruct).FirstOrDefault();

                            if (mUidf != null)
                            {
                                int FeoId = mUidf.Id;
                                var EmpListf = db.Employee.Include(e => e.EmpName).Where(t => t.GeoStruct_Id == FeoId).ToList();

                                foreach (var item in EmpListf)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }

                        }
                        return Json(new { dates }, JsonRequestBehavior.AllowGet);
                        break;
                    case "PastExperienceModel":

                        int SuccessionmodelIdPost = db.CompetencyModel.Include(e => e.PastExperienceModel)
                                  .Include(e => e.PastExperienceModel.PastExperienceModelObject)
                                  .Include(e => e.PastExperienceModel.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel))
                                  .Include(e => e.PastExperienceModel.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                                  .Include(e => e.PastExperienceModel.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                                  .Include(e => e.PastExperienceModel.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                                  .Where(e => e.PastExperienceModel.PastExperienceModelObject.FirstOrDefault().CompetencyEvaluationModel.Id == EvaluationLookupIdss).FirstOrDefault().Id;

                        var vidspost = db.CompetencyModelAssignment.Include(e => e.CompetencyModelAssignment_OrgStructure).
                        Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.GeoStruct)).
                        Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.PayStruct)).
                        Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.FuncStruct)).Where(e => e.CompetencyModel.Id == SuccessionmodelIdPost).FirstOrDefault();
                        var muidspost = vidspost.CompetencyModelAssignment_OrgStructure.ToList();

                        if (muidspost != null && muidspost.Count() > 0)
                        {
                            var mUidg = muidspost.Select(e => e.GeoStruct).FirstOrDefault();

                            if (mUidg != null)
                            {
                                int GeoId = mUidg.Id;
                                var EmpListg = db.Employee.Include(e => e.EmpName).Where(t => t.GeoStruct_Id == GeoId).ToList();

                                foreach (var item in EmpListg)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }



                        }
                        if (muidspost != null && muidspost.Count() > 0)
                        {
                            var mUidp = muidspost.Select(e => e.PayStruct).FirstOrDefault();

                            if (mUidp != null)
                            {
                                int PeoId = mUidp.Id;
                                var EmpListp = db.Employee.Include(e => e.EmpName).Where(t => t.PayStruct_Id == PeoId).ToList();

                                foreach (var item in EmpListp)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }

                        }
                        if (muidspost != null && muidspost.Count() > 0)
                        {
                            var mUidf = muidspost.Select(e => e.FuncStruct).FirstOrDefault();

                            if (mUidf != null)
                            {
                                int FeoId = mUidf.Id;
                                var EmpListf = db.Employee.Include(e => e.EmpName).Where(t => t.GeoStruct_Id == FeoId).ToList();

                                foreach (var item in EmpListf)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }

                        }
                        return Json(new { dates }, JsonRequestBehavior.AllowGet);
                        break;

                    case "PersonnelModel":

                        int SuccessionmodelIdPer = db.CompetencyModel.Include(e => e.PersonnelModel)
                                  .Include(e => e.PersonnelModel.PersonnelModelObject)
                                  .Include(e => e.PersonnelModel.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel))
                                  .Include(e => e.PersonnelModel.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                                  .Include(e => e.PersonnelModel.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                                  .Include(e => e.PersonnelModel.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                                  .Where(e => e.PersonnelModel.PersonnelModelObject.FirstOrDefault().CompetencyEvaluationModel.Id == EvaluationLookupIdss).FirstOrDefault().Id;

                        var vidsper = db.CompetencyModelAssignment.Include(e => e.CompetencyModelAssignment_OrgStructure).
                        Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.GeoStruct)).
                        Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.PayStruct)).
                        Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.FuncStruct)).Where(e => e.CompetencyModel.Id == SuccessionmodelIdPer).FirstOrDefault();
                        var muidsper = vidsper.CompetencyModelAssignment_OrgStructure.ToList();

                        if (muidsper != null && muidsper.Count() > 0)
                        {
                            var mUidg = muidsper.Select(e => e.GeoStruct).FirstOrDefault();

                            if (mUidg != null)
                            {
                                int GeoId = mUidg.Id;
                                var EmpListg = db.Employee.Include(e => e.EmpName).Where(t => t.GeoStruct_Id == GeoId).ToList();

                                foreach (var item in EmpListg)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }



                        }
                        if (muidsper != null && muidsper.Count() > 0)
                        {
                            var mUidp = muidsper.Select(e => e.PayStruct).FirstOrDefault();

                            if (mUidp != null)
                            {
                                int PeoId = mUidp.Id;
                                var EmpListp = db.Employee.Include(e => e.EmpName).Where(t => t.PayStruct_Id == PeoId).ToList();

                                foreach (var item in EmpListp)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }

                        }
                        if (muidsper != null && muidsper.Count() > 0)
                        {
                            var mUidf = muidsper.Select(e => e.FuncStruct).FirstOrDefault();

                            if (mUidf != null)
                            {
                                int FeoId = mUidf.Id;
                                var EmpListf = db.Employee.Include(e => e.EmpName).Where(t => t.GeoStruct_Id == FeoId).ToList();

                                foreach (var item in EmpListf)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }

                        }
                        return Json(new { dates }, JsonRequestBehavior.AllowGet);
                        break;

                    case "QualificationModel":

                        int SuccessionmodelIdQua = db.CompetencyModel.Include(e => e.QualificationModel)
                                  .Include(e => e.PersonnelModel.PersonnelModelObject)
                                  .Include(e => e.PersonnelModel.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel))
                                  .Include(e => e.PersonnelModel.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                                  .Include(e => e.PersonnelModel.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                                  .Include(e => e.PersonnelModel.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                                  .Where(e => e.PersonnelModel.PersonnelModelObject.FirstOrDefault().CompetencyEvaluationModel.Id == EvaluationLookupIdss).FirstOrDefault().Id;

                        var vidsqua = db.CompetencyModelAssignment.Include(e => e.CompetencyModelAssignment_OrgStructure).
                        Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.GeoStruct)).
                        Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.PayStruct)).
                        Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.FuncStruct)).Where(e => e.CompetencyModel.Id == SuccessionmodelIdQua).FirstOrDefault();
                        var muidsqua = vidsqua.CompetencyModelAssignment_OrgStructure.ToList();

                        if (muidsqua != null && muidsqua.Count() > 0)
                        {
                            var mUidg = muidsqua.Select(e => e.GeoStruct).FirstOrDefault();

                            if (mUidg != null)
                            {
                                int GeoId = mUidg.Id;
                                var EmpListg = db.Employee.Include(e => e.EmpName).Where(t => t.GeoStruct_Id == GeoId).ToList();

                                foreach (var item in EmpListg)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }



                        }
                        if (muidsqua != null && muidsqua.Count() > 0)
                        {
                            var mUidp = muidsqua.Select(e => e.PayStruct).FirstOrDefault();

                            if (mUidp != null)
                            {
                                int PeoId = mUidp.Id;
                                var EmpListp = db.Employee.Include(e => e.EmpName).Where(t => t.PayStruct_Id == PeoId).ToList();

                                foreach (var item in EmpListp)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }

                        }
                        if (muidsqua != null && muidsqua.Count() > 0)
                        {
                            var mUidf = muidsqua.Select(e => e.FuncStruct).FirstOrDefault();

                            if (mUidf != null)
                            {
                                int FeoId = mUidf.Id;
                                var EmpListf = db.Employee.Include(e => e.EmpName).Where(t => t.GeoStruct_Id == FeoId).ToList();

                                foreach (var item in EmpListf)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }

                        }
                        return Json(new { dates }, JsonRequestBehavior.AllowGet);
                        break;

                    case "ServiceModel":

                        int SuccessionmodelIdSer = db.CompetencyModel.Include(e => e.ServiceModel)
                                  .Include(e => e.PersonnelModel.PersonnelModelObject)
                                  .Include(e => e.PersonnelModel.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel))
                                  .Include(e => e.PersonnelModel.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                                  .Include(e => e.PersonnelModel.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                                  .Include(e => e.PersonnelModel.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                                  .Where(e => e.PersonnelModel.PersonnelModelObject.FirstOrDefault().CompetencyEvaluationModel.Id == EvaluationLookupIdss).FirstOrDefault().Id;

                        var vidsser = db.CompetencyModelAssignment.Include(e => e.CompetencyModelAssignment_OrgStructure).
                        Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.GeoStruct)).
                        Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.PayStruct)).
                        Include(e => e.CompetencyModelAssignment_OrgStructure.Select(r => r.FuncStruct)).Where(e => e.CompetencyModel.Id == SuccessionmodelIdSer).FirstOrDefault();
                        var muidsser = vidsser.CompetencyModelAssignment_OrgStructure.ToList();

                        if (muidsser != null && muidsser.Count() > 0)
                        {
                            var mUidg = muidsser.Select(e => e.GeoStruct).FirstOrDefault();

                            if (mUidg != null)
                            {
                                int GeoId = mUidg.Id;
                                var EmpListg = db.Employee.Include(e => e.EmpName).Where(t => t.GeoStruct_Id == GeoId).ToList();

                                foreach (var item in EmpListg)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }



                        }
                        if (muidsser != null && muidsser.Count() > 0)
                        {
                            var mUidp = muidsser.Select(e => e.PayStruct).FirstOrDefault();

                            if (mUidp != null)
                            {
                                int PeoId = mUidp.Id;
                                var EmpListp = db.Employee.Include(e => e.EmpName).Where(t => t.PayStruct_Id == PeoId).ToList();

                                foreach (var item in EmpListp)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }

                        }
                        if (muidsser != null && muidsser.Count() > 0)
                        {
                            var mUidf = muidsser.Select(e => e.FuncStruct).FirstOrDefault();

                            if (mUidf != null)
                            {
                                int FeoId = mUidf.Id;
                                var EmpListf = db.Employee.Include(e => e.EmpName).Where(t => t.GeoStruct_Id == FeoId).ToList();

                                foreach (var item in EmpListf)
                                {
                                    string EmpCode = item.EmpCode;
                                    string EmpName = item.EmpName.FullNameFML;
                                    dates.Add(EmpCode, EmpName);
                                }
                            }

                        }
                        return Json(new { dates }, JsonRequestBehavior.AllowGet);
                        break;

                }



                return null;

            }
        }
        public class EmpDataTClass
        {
            public string SNo { get; set; }
            public string ModelName { get; set; }
            public string ModelObjectType { get; set; }
            public string ModelObjectSubtype { get; set; }
            public string Criteria { get; set; }
            public string Data1 { get; set; }
            public string Data2 { get; set; }
        }

        public class EmpAnalysisDataClass
        {
            public string SNo { get; set; }
            public string EmpPhoto { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public string Location { get; set; }
            public string Department { get; set; }
            public string Designation { get; set; }
            public Dictionary<string, object> Fields { get; set; }

        }

        public ActionResult GetProcessBatchLKDetails(List<int> SkipIds, int BatchId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.CompetencyBatchProcessT.Where(e => e.BatchName_Id == BatchId && e.IsProcessComplete == false).ToList().OrderBy(e => e.Id);

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.CompetencyBatchProcessT.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList().OrderBy(e => e.Id);
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList().OrderBy(e => e.Id);
                    }
                }


                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.ProcessBatch }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }

        [HttpPost]
        public ActionResult createdata(List<EmpDataTClass> data, string BatchNamelist, string ProcessBatch)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string MSG = "";
                int BatchNameId = int.Parse(BatchNamelist);
                var OCompEmpDataT = db.CompetencyEmployeeDataGeneration.Include(e => e.BatchName)
                    .Include(e => e.AppraisalAttributeModelObjectV)
                    .Include(e => e.AppraisalAttributeModelObjectV.Select(t => t.AppraisalAttributeModelObject))
                    .Include(e => e.AppraisalAttributeModelObjectV.Select(t => t.ObjectValue))
                    .Include(e => e.AppraisalAttributeModelObjectV.Select(t => t.AppraisalAttributeModelObject.AppraisalAttributeModel))
                    .Include(e => e.AppraisalBusinessAppraisalModelObjectV)
                    .Include(e => e.AppraisalBusinessAppraisalModelObjectV.Select(t => t.ObjectValue))
                   .Include(e => e.AppraisalBusinessAppraisalModelObjectV.Select(t => t.AppraisalBusinessAppraisalModelObject))
                   .Include(e => e.AppraisalBusinessAppraisalModelObjectV.Select(t => t.AppraisalBusinessAppraisalModelObject.AppraisalBusinessAppraisalModel))
                    .Include(e => e.AppraisalKRAModelObjectV)
                    .Include(e => e.AppraisalKRAModelObjectV.Select(t => t.ObjectValue))
                    .Include(e => e.AppraisalKRAModelObjectV.Select(t => t.AppraisalKRAModelObject))
                    .Include(e => e.AppraisalKRAModelObjectV.Select(t => t.AppraisalKRAModelObject.AppraisalKRAModel))
                    .Include(e => e.AppraisalPotentialModelObjectV)
                    .Include(e => e.AppraisalPotentialModelObjectV.Select(t => t.ObjectValue))
                    .Include(e => e.AppraisalPotentialModelObjectV.Select(t => t.AppraisalPotentialModelObject))
                    .Include(e => e.AppraisalPotentialModelObjectV.Select(t => t.AppraisalPotentialModelObject.AppraisalPotentialModel))
                    .Include(e => e.PastExperienceModelObjectV)
                    .Include(e => e.PastExperienceModelObjectV.Select(t => t.ObjectValue))
                    .Include(e => e.PastExperienceModelObjectV.Select(t => t.PastExperienceModelObject))
                    .Include(e => e.PastExperienceModelObjectV.Select(t => t.PastExperienceModelObject.PastExperienceModel))
                    .Include(e => e.AppraisalPotentialModelObjectV.Select(t => t.AppraisalPotentialModelObject))
                    .Include(e => e.AppraisalPotentialModelObjectV.Select(t => t.AppraisalPotentialModelObject.AppraisalPotentialModel))
                    .Include(e => e.PersonnelModelObjectV)
                    .Include(e => e.PersonnelModelObjectV.Select(t => t.ObjectValue))
                    .Include(e => e.PersonnelModelObjectV.Select(t => t.PersonnelModelObject))
                    .Include(e => e.PersonnelModelObjectV.Select(t => t.PersonnelModelObject.PersonnelModel))
                    .Include(e => e.PersonnelModelObjectV.Select(t => t.PersonnelModelObject.CompetencyEvaluationModel))
                    .Include(e => e.PersonnelModelObjectV.Select(t => t.PersonnelModelObject.CompetencyEvaluationModel.CriteriaType))
                    .Include(e => e.QualificationModelObjectV)
                    .Include(e => e.QualificationModelObjectV.Select(t => t.ObjectValue))
                    .Include(e => e.QualificationModelObjectV.Select(t => t.QualificationModelObject))
                    .Include(e => e.QualificationModelObjectV.Select(t => t.QualificationModelObject.QualificationModel))
                    .Include(e => e.ServiceModelObjectV)
                    .Include(e => e.ServiceModelObjectV.Select(t => t.ObjectValue))
                    .Include(e => e.ServiceModelObjectV.Select(t => t.ServiceModelObject))
                    .Include(e => e.ServiceModelObjectV.Select(t => t.ServiceModelObject.ServiceModel))
                    .Include(e => e.SkillModelObjectV)
                    .Include(e => e.SkillModelObjectV.Select(t => t.ObjectValue))
                    .Include(e => e.SkillModelObjectV.Select(t => t.SkillModelObject))
                    .Include(e => e.SkillModelObjectV.Select(t => t.SkillModelObject.SkillModel))
                    .Include(e => e.TrainingModelObjectV)
                    .Include(e => e.TrainingModelObjectV.Select(t => t.ObjectValue))
                    .Include(e => e.TrainingModelObjectV.Select(t => t.TrainingModelObject))
                    .Include(e => e.TrainingModelObjectV.Select(t => t.TrainingModelObject.TrainingModel))
                    .Include(e => e.Employee)
                    .Include(e => e.Employee.EmpName)
                    .Where(e => e.BatchName.Id == BatchNameId)
                    .ToList();

                //List<CompetencyEmployeeDataGeneration> OCompEmpDataT = db.CompetencyEmployeeDataGeneration.Where(e => e.BatchName_Id == BatchNameId).ToList();
                //foreach (var item in OCompEmpDataT)
                //{
                //    item.Employee = db.Employee.Find(item.Employee_Id);
                //    item.Employee.EmpName = db.NameSingle.Find(item.Employee.EmpName_Id);
                //    item.AppraisalAttributeModelObjectV = db.CompetencyEmployeeDataGeneration.Where(e => e.Id == item.Id).Select(t => t.AppraisalAttributeModelObjectV).FirstOrDefault();
                //    foreach (var item1 in item.AppraisalAttributeModelObjectV)
                //    {
                //        item1.AppraisalAttributeModelObject = db.AppraisalAttributeModelObject.Find(item1.AppraisalAttributeModelObject_Id);
                //        item1.AppraisalAttributeModelObject.AppraisalAttributeModel = db.LookupValue.Find(item1.AppraisalAttributeModelObject.AppraisalAttributeModel_Id);
                //        item1.ObjectValue = db.AppraisalAttributeModelObjectV.Where(e => e.Id == item1.Id).Select(t => t.ObjectValue).FirstOrDefault();
                //    }
                //    item.AppraisalBusinessAppraisalModelObjectV = db.CompetencyEmployeeDataGeneration.Where(e => e.Id == item.Id).Select(t => t.AppraisalBusinessAppraisalModelObjectV).FirstOrDefault();
                //    foreach (var item1 in item.AppraisalBusinessAppraisalModelObjectV)
                //    {
                //        item1.AppraisalBusinessAppraisalModelObject = db.AppraisalBusinessAppraisalModelObject.Find(item1.AppraisalBusinessAppraisalModelObject_Id);
                //        item1.AppraisalBusinessAppraisalModelObject.AppraisalBusinessAppraisalModel = db.LookupValue.Find(item1.AppraisalBusinessAppraisalModelObject.AppraisalBusinessAppraisalModel_Id);
                //        item1.ObjectValue = db.AppraisalBusinessAppraisalModelObjectV.Where(e => e.Id == item1.Id).Select(t => t.ObjectValue).FirstOrDefault();
                //    }
                //    item.AppraisalKRAModelObjectV = db.CompetencyEmployeeDataGeneration.Where(e => e.Id == item.Id).Select(t => t.AppraisalKRAModelObjectV).FirstOrDefault();
                //    foreach (var item1 in item.AppraisalKRAModelObjectV)
                //    {
                //        item1.AppraisalKRAModelObject = db.AppraisalKRAModelObject.Find(item1.AppraisalKRAModelObject_Id);
                //        item1.AppraisalKRAModelObject.AppraisalKRAModel = db.LookupValue.Find(item1.AppraisalKRAModelObject.AppraisalKRAModel_Id);
                //        item1.ObjectValue = db.AppraisalKRAModelObjectV.Where(e => e.Id == item1.Id).Select(t => t.ObjectValue).FirstOrDefault();
                //    }
                //    item.AppraisalPotentialModelObjectV = db.CompetencyEmployeeDataGeneration.Where(e => e.Id == item.Id).Select(t => t.AppraisalPotentialModelObjectV).FirstOrDefault();
                //    foreach (var item1 in item.AppraisalPotentialModelObjectV)
                //    {
                //        item1.AppraisalPotentialModelObject = db.AppraisalPotentialModelObject.Find(item1.AppraisalPotentialModelObject_Id);
                //        item1.AppraisalPotentialModelObject.AppraisalPotentialModel = db.LookupValue.Find(item1.AppraisalPotentialModelObject.AppraisalPotentialModel_Id);
                //        item1.ObjectValue = db.AppraisalPotentialModelObjectV.Where(e => e.Id == item1.Id).Select(t => t.ObjectValue).FirstOrDefault();
                //    }
                //    item.PastExperienceModelObjectV = db.CompetencyEmployeeDataGeneration.Where(e => e.Id == item.Id).Select(t => t.PastExperienceModelObjectV).FirstOrDefault();
                //    foreach (var item1 in item.PastExperienceModelObjectV)
                //    {
                //        item1.PastExperienceModelObject = db.PastExperienceModelObject.Find(item1.PastExperienceModelObject_Id);
                //        item1.PastExperienceModelObject.PastExperienceModel = db.LookupValue.Find(item1.PastExperienceModelObject.PastExperienceModel_Id);
                //        item1.ObjectValue = db.PastExperienceModelObjectV.Where(e => e.Id == item1.Id).Select(t => t.ObjectValue).FirstOrDefault();
                //    }
                //    item.PersonnelModelObjectV = db.CompetencyEmployeeDataGeneration.Where(e => e.Id == item.Id).Select(t => t.PersonnelModelObjectV).FirstOrDefault();
                //    foreach (var item1 in item.PersonnelModelObjectV)
                //    {
                //        item1.PersonnelModelObject = db.PersonnelModelObject.Find(item1.PersonnelModelObject_Id);
                //        item1.PersonnelModelObject.PersonnelModel = db.LookupValue.Find(item1.PersonnelModelObject.PersonnelModel_Id);
                //        item1.ObjectValue = db.PersonnelModelObjectV.Where(e => e.Id == item1.Id).Select(t => t.ObjectValue).FirstOrDefault();
                //        item1.PersonnelModelObject.CompetencyEvaluationModel = db.CompetencyEvaluationModel.Find(item1.PersonnelModelObject.CompetencyEvaluationModel_Id);
                //        item1.PersonnelModelObject.CompetencyEvaluationModel.CriteriaType = db.LookupValue.Find(item1.PersonnelModelObject.CompetencyEvaluationModel.CriteriaType_Id);
                //    }
                //    item.QualificationModelObjectV = db.CompetencyEmployeeDataGeneration.Where(e => e.Id == item.Id).Select(t => t.QualificationModelObjectV).FirstOrDefault();
                //    foreach (var item1 in item.QualificationModelObjectV)
                //    {
                //        item1.QualificationModelObject = db.QualificationModelObject.Find(item1.QualificationModelObject_Id);
                //        item1.QualificationModelObject.QualificationModel = db.LookupValue.Find(item1.QualificationModelObject.QualificationModel_Id);
                //        item1.ObjectValue = db.QualificationModelObjectV.Where(e => e.Id == item1.Id).Select(t => t.ObjectValue).FirstOrDefault();
                //    }
                //    item.ServiceModelObjectV = db.CompetencyEmployeeDataGeneration.Where(e => e.Id == item.Id).Select(t => t.ServiceModelObjectV).FirstOrDefault();
                //    foreach (var item1 in item.ServiceModelObjectV)
                //    {
                //        item1.ServiceModelObject = db.ServiceModelObject.Find(item1.ServiceModelObject_Id);
                //        item1.ServiceModelObject.ServiceModel = db.LookupValue.Find(item1.ServiceModelObject.ServiceModel_Id);
                //        item1.ObjectValue = db.ServiceModelObjectV.Where(e => e.Id == item1.Id).Select(t => t.ObjectValue).FirstOrDefault();
                //    }
                //    item.SkillModelObjectV = db.CompetencyEmployeeDataGeneration.Where(e => e.Id == item.Id).Select(t => t.SkillModelObjectV).FirstOrDefault();
                //    foreach (var item1 in item.SkillModelObjectV)
                //    {
                //        item1.SkillModelObject = db.SkillModelObject.Find(item1.SkillModelObject_Id);
                //        item1.SkillModelObject.SkillModel = db.LookupValue.Find(item1.SkillModelObject.SkillModel_Id);
                //        item1.ObjectValue = db.AppraisalPotentialModelObjectV.Where(e => e.Id == item1.Id).Select(t => t.ObjectValue).FirstOrDefault();
                //    }
                //    item.TrainingModelObjectV = db.CompetencyEmployeeDataGeneration.Where(e => e.Id == item.Id).Select(t => t.TrainingModelObjectV).FirstOrDefault();
                //    foreach (var item1 in item.TrainingModelObjectV)
                //    {
                //        item1.TrainingModelObject = db.TrainingModelObject.Find(item1.TrainingModelObject_Id);
                //        item1.TrainingModelObject.TrainingModel = db.LookupValue.Find(item1.TrainingModelObject.TrainingModel_Id);
                //        item1.ObjectValue = db.TrainingModelObjectV.Where(e => e.Id == item1.Id).Select(t => t.ObjectValue).FirstOrDefault();
                //    }

                //}
                //var OCompModelAssign = db.CompetencyModelAssignment.Where(e => e.Id == BatchNameId)
                //  .Include(e => e.CompetencyModel.AppraisalAttributeModel)
                //  .Include(e => e.CompetencyModel.AppraisalBusinessApprisalModel)
                //  .Include(e => e.CompetencyModel.AppraisalKRAModel)
                //  .Include(e => e.CompetencyModel.AppraisalPotentialModel)
                //  .Include(e => e.CompetencyModel.PastExperienceModel)
                //  .Include(e => e.CompetencyModel.PersonnelModel)
                //  .Include(e => e.CompetencyModel.QualificationModel).Include(e => e.CompetencyModel.ServiceModel)
                //  .Include(e => e.CompetencyModel.SkillModel).Include(e => e.CompetencyModel.TrainingModel).FirstOrDefault();

                //var OEmployeePayroll = new EmployeePayroll();
                //OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == OEmployee.Id).FirstOrDefault();
                //var OEmp = db.EmployeePayroll.Where(e => e.Employee.Id == OEmployee.Id).Select(r => r.Employee).FirstOrDefault();
                //var OEmpOff = db.EmployeePayroll.Where(e => e.Employee.Id == OEmployee.Id).Select(r => r.Employee.EmpOffInfo).FirstOrDefault();
                //var NationalityID = db.Employee.Where(e => e.Id == OEmp.Id).Select(r => r.EmpOffInfo.NationalityID).FirstOrDefault();
                //var Gender = db.Employee.Where(e => e.Id == OEmp.Id).Select(r => r.Gender).FirstOrDefault();
                //var EmpName = db.Employee.Where(e => e.Id == OEmp.Id).Select(r => r.EmpName).FirstOrDefault();
                //var ServiceBookDates = db.Employee.Where(e => e.Id == OEmp.Id).Select(r => r.ServiceBookDates).FirstOrDefault();
                //List<ITProjection> ITProjection = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Select(r => r.ITProjection.Where(e => e.FinancialYear_Id == OFinancia.Id).ToList()).FirstOrDefault();
                //foreach (var i in ITProjection)
                //{
                //    var ITProjectionObj = db.ITProjection.Where(e => e.Id == i.Id).Select(r => r.FinancialYear).FirstOrDefault();
                //    var FinancialYear = db.Calendar.Where(e => e.Id == ITProjectionObj.Id).FirstOrDefault();
                //    i.FinancialYear = FinancialYear;

                //}

                var OCompModelAssign = new CompetencyModelAssignment();
                OCompModelAssign = db.CompetencyModelAssignment.Find(BatchNameId);
                OCompModelAssign.CompetencyModel = db.CompetencyModel.Find(OCompModelAssign.CompetencyModel_Id);
                OCompModelAssign.CompetencyModel.AppraisalAttributeModel = db.AppraisalAttributeModel.Find(OCompModelAssign.CompetencyModel.AppraisalAttributeModel_Id);
                OCompModelAssign.CompetencyModel.AppraisalBusinessApprisalModel = db.AppraisalBusinessAppraisalModel.Find(OCompModelAssign.CompetencyModel.AppraisalBusinessAppraisalModel_Id);
                OCompModelAssign.CompetencyModel.AppraisalKRAModel = db.AppraisalKRAModel.Find(OCompModelAssign.CompetencyModel.AppraisalKRAModel_Id);
                OCompModelAssign.CompetencyModel.AppraisalPotentialModel = db.AppraisalPotentialModel.Find(OCompModelAssign.CompetencyModel.AppraisalPotentialModel_Id);
                OCompModelAssign.CompetencyModel.PastExperienceModel = db.PastExperienceModel.Find(OCompModelAssign.CompetencyModel.PastExperienceModel_Id);
                OCompModelAssign.CompetencyModel.PersonnelModel = db.PersonnelModel.Find(OCompModelAssign.CompetencyModel.PersonnelModel_Id);
                OCompModelAssign.CompetencyModel.QualificationModel = db.QualificationModel.Find(OCompModelAssign.CompetencyModel.QualificationModel_Id);
                OCompModelAssign.CompetencyModel.ServiceModel = db.ServiceModel.Find(OCompModelAssign.CompetencyModel.ServiceModel_Id);
                OCompModelAssign.CompetencyModel.SkillModel = db.SkillModel.Find(OCompModelAssign.CompetencyModel.SkillModel_Id);
                OCompModelAssign.CompetencyModel.TrainingModel = db.TrainingModel.Find(OCompModelAssign.CompetencyModel.TrainingModel_Id);

                List<EmpAnalysisDataClass> returndata = new List<EmpAnalysisDataClass>();
                List<string> OModelObject = new List<string>();
                List<string> OModelObjectData = new List<string>();
                List<CompetencyEmployeeDataT> OCompEmpDataTList = new List<CompetencyEmployeeDataT>();

                AppraisalAttributeModelObjectF OAppAttModelF = null;
                AppraisalBusinessAppraisalModelObjectF OAppBusiModelF = null;
                AppraisalKRAModelObjectF OAppKRAModelF = null;
                AppraisalPotentialModelObjectF OAppPotentialModelF = null;
                PastExperienceModelObjectF OPastExpModelF = null;
                PersonnelModelObjectF OPersModelF = null;
                QualificationModelObjectF OQualModelF = null;
                ServiceModelObjectF OServModelF = null;
                SkillModelObjectF OSkillModelF = null;
                TrainingModelObjectF OTrainingModelF = null;

                List<AppraisalAttributeModelObjectF> OAppAttModellistF = new List<AppraisalAttributeModelObjectF>();
                List<AppraisalBusinessAppraisalModelObjectF> OAppBusiModellistF = new List<AppraisalBusinessAppraisalModelObjectF>();
                List<AppraisalKRAModelObjectF> OAppKRAModellistF = new List<AppraisalKRAModelObjectF>();
                List<AppraisalPotentialModelObjectF> OAppPotentialModellistF = new List<AppraisalPotentialModelObjectF>();
                List<PastExperienceModelObjectF> OPastExpModellistF = new List<PastExperienceModelObjectF>();
                List<PersonnelModelObjectF> OPersModellistF = new List<PersonnelModelObjectF>();
                List<QualificationModelObjectF> OQualModellistF = new List<QualificationModelObjectF>();
                List<ServiceModelObjectF> OServModellistF = new List<ServiceModelObjectF>();
                List<SkillModelObjectF> OSkillModellistF = new List<SkillModelObjectF>();
                List<TrainingModelObjectF> OTrainingModellistF = new List<TrainingModelObjectF>();

                try
                {
                    var DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                    if (ModelState.IsValid)
                    {
                      //  List<int> chk = db.CompetencyEmployeeDataT.Include(e => e.BatchName)
                      //.Where(e => e.BatchName.Id == BatchNameId && e.DBTrack.CreatedBy == SessionManager.EmpId).Select(t => t.Id).ToList();
                      //  if (chk.Count() > 0)
                      //  {
                      //      foreach (var item in chk)
                      //      {
                      //          DeleteCompData(item, SessionManager.EmpId);
                      //      }
                      //  }

                        List<int> chk = db.CompetencyBatchProcessT.Include(e => e.BatchName)
                      .Where(e => e.BatchName.Id == BatchNameId && e.DBTrack.CreatedBy == SessionManager.EmpId && e.ProcessBatch == ProcessBatch).Select(t => t.Id).ToList();
                        if (chk.Count() > 0)
                        {
                            foreach (var item in chk)
                            {
                                DeleteBatchData(item, SessionManager.EmpId);
                            }
                        }
                        
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 60, 0)))
                        {
                            

                            foreach (var item in OCompEmpDataT)
                            {
                                
                                var retdata = new EmpAnalysisDataClass();
                                retdata.Fields = new Dictionary<string, object>();

                                bool additem = false;
                                int count = 0;
                                AppraisalAttributeModelObjectT OAppAttModel = null;
                                AppraisalBusinessAppraisalModelObjectT OAppBusiModel = null;
                                AppraisalKRAModelObjectT OAppKRAModel = null;
                                AppraisalPotentialModelObjectT OAppPotentialModel = null;
                                PastExperienceModelObjectT OPastExpModel = null;
                                PersonnelModelObjectT OPersModel = null;
                                QualificationModelObjectT OQualModel = null;
                                ServiceModelObjectT OServModel = null;
                                SkillModelObjectT OSkillModel = null;
                                TrainingModelObjectT OTrainingModel = null;

                                List<AppraisalAttributeModelObjectT> OAppAttModellist = new List<AppraisalAttributeModelObjectT>();
                                List<AppraisalBusinessAppraisalModelObjectT> OAppBusiModellist = new List<AppraisalBusinessAppraisalModelObjectT>();
                                List<AppraisalKRAModelObjectT> OAppKRAModellist = new List<AppraisalKRAModelObjectT>();
                                List<AppraisalPotentialModelObjectT> OAppPotentialModellist = new List<AppraisalPotentialModelObjectT>();
                                List<PastExperienceModelObjectT> OPastExpModellist = new List<PastExperienceModelObjectT>();
                                List<PersonnelModelObjectT> OPersModellist = new List<PersonnelModelObjectT>();
                                List<QualificationModelObjectT> OQualModellist = new List<QualificationModelObjectT>();
                                List<ServiceModelObjectT> OServModellist = new List<ServiceModelObjectT>();
                                List<SkillModelObjectT> OSkillModellist = new List<SkillModelObjectT>();
                                List<TrainingModelObjectT> OTrainingModellist = new List<TrainingModelObjectT>();




                                foreach (var item1 in data)
                                {
                                    additem = false;
                                    #region AppraisalAtt
                                    if (item.AppraisalAttributeModelObjectV != null && OCompModelAssign.CompetencyModel.AppraisalAttributeModel != null && item1.ModelObjectType == OCompModelAssign.CompetencyModel.AppraisalAttributeModel.ModelName)
                                    {
                                        foreach (var itemP in item.AppraisalAttributeModelObjectV)
                                        {
                                            additem = false;
                                            List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                            ObjectValue OObjVal = new ObjectValue();

                                            List<ObjectValue> OObjVallistF = new List<ObjectValue>();
                                            ObjectValue OObjValF = new ObjectValue();

                                            bool alreadyExists = OAppAttModellistF.Any(x => x.AppraisalAttributeModelObject == itemP.AppraisalAttributeModelObject);
                                            if (alreadyExists == false)
                                            {
                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = item1.Data1.ToString();
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);

                                                OAppAttModelF = new AppraisalAttributeModelObjectF();
                                                {
                                                    OAppAttModelF.AppraisalAttributeModelObject = itemP.AppraisalAttributeModelObject;
                                                    OAppAttModelF.DBTrack = DBTrack;
                                                    OAppAttModelF.ObjectValue = OObjVallistF;
                                                };
                                                OTrainingModellistF.Add(OTrainingModelF);
                                            }
                                            if (itemP.AppraisalAttributeModelObject.AppraisalAttributeModel.LookupVal.ToUpper() == item1.ModelObjectSubtype.ToUpper())
                                            {
                                                foreach (var itemO in itemP.ObjectValue)
                                                {
                                                    if (item1.Criteria.ToUpper() == "EQUAL")
                                                    {
                                                        if (itemO.ObjectVal.ToUpper() == item1.Data1.ToUpper())
                                                        {
                                                            count += 1;
                                                            additem = true;
                                                            break;
                                                        }
                                                    }

                                                    if (item1.Criteria.ToUpper() == "LIKE")
                                                    {
                                                        if (itemO.ObjectVal.ToUpper().Contains(item1.Data1.ToUpper()))
                                                        {
                                                            count += 1;
                                                            additem = true;
                                                        }
                                                    }
                                                }

                                            }

                                            if (additem == true && retdata.Fields.ContainsKey(item1.ModelObjectSubtype) == false)
                                            {
                                                retdata.Fields.Add(item1.ModelObjectSubtype, item1.Data1.ToString());
                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = item1.Data1.ToString();
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);
                                            }

                                            if (additem == true)
                                            {
                                                OAppAttModel = new AppraisalAttributeModelObjectT();
                                                {
                                                    OAppAttModel.AppraisalAttributeModelObject = itemP.AppraisalAttributeModelObject;
                                                    OAppAttModel.DBTrack = DBTrack;
                                                    OAppAttModel.ObjectValue = OObjVallist;
                                                };
                                                OAppAttModellist.Add(OAppAttModel);
                                            }
                                        }
                                    }
                                    #endregion

                                    #region AppraisalBusiness
                                    if (item.AppraisalBusinessAppraisalModelObjectV != null && OCompModelAssign.CompetencyModel.AppraisalBusinessApprisalModel != null && item1.ModelObjectType == OCompModelAssign.CompetencyModel.AppraisalBusinessApprisalModel.ModelName)
                                    {
                                        foreach (var itemP in item.AppraisalBusinessAppraisalModelObjectV)
                                        {
                                            additem = false;
                                            List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                            ObjectValue OObjVal = new ObjectValue();

                                            List<ObjectValue> OObjVallistF = new List<ObjectValue>();
                                            ObjectValue OObjValF = new ObjectValue();

                                            bool alreadyExists = OAppBusiModellistF.Any(x => x.AppraisalBusinessAppraisalModelObject == itemP.AppraisalBusinessAppraisalModelObject);
                                            if (alreadyExists == false)
                                            {
                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = item1.Data1.ToString();
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);

                                                OAppBusiModelF = new AppraisalBusinessAppraisalModelObjectF();
                                                {
                                                    OAppBusiModelF.AppraisalBusinessAppraisalModelObject = itemP.AppraisalBusinessAppraisalModelObject;
                                                    OAppBusiModelF.DBTrack = DBTrack;
                                                    OAppBusiModelF.ObjectValue = OObjVallistF;
                                                };
                                                OAppBusiModellistF.Add(OAppBusiModelF);
                                            }
                                            if (itemP.AppraisalBusinessAppraisalModelObject.AppraisalBusinessAppraisalModel.LookupVal.ToUpper() == item1.ModelObjectSubtype.ToUpper())
                                            {
                                                foreach (var itemO in itemP.ObjectValue)
                                                {
                                                    if (item1.Criteria.ToUpper() == "EQUAL")
                                                    {
                                                        if (itemO.ObjectVal.ToUpper() == item1.Data1.ToUpper())
                                                        {
                                                            count += 1;
                                                            additem = true;
                                                            break;
                                                        }
                                                    }

                                                    if (item1.Criteria.ToUpper() == "LIKE")
                                                    {
                                                        if (itemO.ObjectVal.ToUpper().Contains(item1.Data1.ToUpper()))
                                                        {
                                                            count += 1;
                                                            additem = true;
                                                        }
                                                    }
                                                }

                                            }

                                            if (additem == true && retdata.Fields.ContainsKey(item1.ModelObjectSubtype) == false)
                                            {
                                                retdata.Fields.Add(item1.ModelObjectSubtype, item1.Data1.ToString());

                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = item1.Data1.ToString();
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);
                                            }

                                            if (additem == true)
                                            {
                                                OAppBusiModel = new AppraisalBusinessAppraisalModelObjectT();
                                                {
                                                    OAppBusiModel.AppraisalBusinessAppraisalModelObject = itemP.AppraisalBusinessAppraisalModelObject;
                                                    OAppBusiModel.DBTrack = DBTrack;
                                                    OAppBusiModel.ObjectValue = OObjVallist;
                                                };
                                                OAppBusiModellist.Add(OAppBusiModel);
                                            }
                                        }
                                    }
                                    #endregion

                                    #region AppraisalKRA
                                    if (item.AppraisalKRAModelObjectV != null && OCompModelAssign.CompetencyModel.AppraisalKRAModel != null && item1.ModelObjectType == OCompModelAssign.CompetencyModel.AppraisalKRAModel.ModelName)
                                    {
                                        foreach (var itemP in item.AppraisalKRAModelObjectV)
                                        {
                                            additem = false;
                                            List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                            ObjectValue OObjVal = new ObjectValue();

                                            List<ObjectValue> OObjVallistF = new List<ObjectValue>();
                                            ObjectValue OObjValF = new ObjectValue();

                                            bool alreadyExists = OAppKRAModellistF.Any(x => x.AppraisalKRAModelObject == itemP.AppraisalKRAModelObject);
                                            if (alreadyExists == false)
                                            {
                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = item1.Data1.ToString();
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);

                                                OAppKRAModelF = new AppraisalKRAModelObjectF();
                                                {
                                                    OAppKRAModelF.AppraisalKRAModelObject = itemP.AppraisalKRAModelObject;
                                                    OAppKRAModelF.DBTrack = DBTrack;
                                                    OAppKRAModelF.ObjectValue = OObjVallistF;
                                                };
                                                OTrainingModellistF.Add(OTrainingModelF);
                                            }
                                            if (itemP.AppraisalKRAModelObject.AppraisalKRAModel.LookupVal.ToUpper() == item1.ModelObjectSubtype.ToUpper())
                                            {
                                                foreach (var itemO in itemP.ObjectValue)
                                                {
                                                    if (item1.Criteria.ToUpper() == "EQUAL")
                                                    {
                                                        if (itemO.ObjectVal.ToUpper() == item1.Data1.ToUpper())
                                                        {
                                                            count += 1;
                                                            additem = true;
                                                            break;
                                                        }
                                                    }

                                                    if (item1.Criteria.ToUpper() == "LIKE")
                                                    {
                                                        if (itemO.ObjectVal.ToUpper().Contains(item1.Data1.ToUpper()))
                                                        {
                                                            count += 1;
                                                            additem = true;
                                                        }
                                                    }
                                                }

                                            }

                                            if (additem == true && retdata.Fields.ContainsKey(item1.ModelObjectSubtype) == false)
                                            {
                                                retdata.Fields.Add(item1.ModelObjectSubtype, item1.Data1.ToString());

                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = item1.Data1.ToString();
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);
                                            }

                                            if (additem == true)
                                            {
                                                OAppKRAModel = new AppraisalKRAModelObjectT();
                                                {
                                                    OAppKRAModel.AppraisalKRAModelObject = itemP.AppraisalKRAModelObject;
                                                    OAppKRAModel.DBTrack = DBTrack;
                                                    OAppKRAModel.ObjectValue = OObjVallist;
                                                };
                                                OAppKRAModellist.Add(OAppKRAModel);
                                            }
                                        }
                                    }
                                    #endregion

                                    #region AppraisalPotential
                                    if (item.AppraisalPotentialModelObjectV != null && OCompModelAssign.CompetencyModel.AppraisalPotentialModel != null && item1.ModelObjectType == OCompModelAssign.CompetencyModel.AppraisalPotentialModel.ModelName)
                                    {
                                        foreach (var itemP in item.AppraisalPotentialModelObjectV)
                                        {
                                            itemP.AppraisalPotentialModelObject = db.AppraisalPotentialModelObject.Find(itemP.AppraisalPotentialModelObject_Id);
                                            additem = false;
                                            List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                            ObjectValue OObjVal = new ObjectValue();

                                            List<ObjectValue> OObjVallistF = new List<ObjectValue>();
                                            ObjectValue OObjValF = new ObjectValue();

                                            bool alreadyExists = OAppPotentialModellistF.Any(x => x.AppraisalPotentialModelObject == itemP.AppraisalPotentialModelObject);
                                            if (alreadyExists == false)
                                            {
                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = item1.Data1.ToString();
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);

                                                OAppPotentialModelF = new AppraisalPotentialModelObjectF();
                                                {
                                                    OAppPotentialModelF.AppraisalPotentialModelObject = itemP.AppraisalPotentialModelObject;
                                                    OAppPotentialModelF.DBTrack = DBTrack;
                                                    OAppPotentialModelF.ObjectValue = OObjVallistF;
                                                };
                                                OTrainingModellistF.Add(OTrainingModelF);
                                            }
                                            if (itemP.AppraisalPotentialModelObject.AppraisalPotentialModel.LookupVal.ToUpper() == item1.ModelObjectSubtype.ToUpper())
                                            {
                                                foreach (var itemO in itemP.ObjectValue)
                                                {
                                                    if (item1.Criteria.ToUpper() == "EQUAL")
                                                    {
                                                        if (itemO.ObjectVal.ToUpper() == item1.Data1.ToUpper())
                                                        {
                                                            count += 1;
                                                            additem = true;
                                                            break;
                                                        }
                                                    }

                                                    if (item1.Criteria.ToUpper() == "LIKE")
                                                    {
                                                        if (itemO.ObjectVal.ToUpper().Contains(item1.Data1.ToUpper()))
                                                        {
                                                            count += 1;
                                                            additem = true;
                                                        }
                                                    }
                                                }

                                            }

                                            if (additem == true && retdata.Fields.ContainsKey(item1.ModelObjectSubtype) == false)
                                            {
                                                retdata.Fields.Add(item1.ModelObjectSubtype, item1.Data1.ToString());

                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = item1.Data1.ToString();
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);
                                            }

                                            if (additem == true)
                                            {
                                                OAppPotentialModel = new AppraisalPotentialModelObjectT();
                                                {
                                                    OAppPotentialModel.AppraisalPotentialModelObject = itemP.AppraisalPotentialModelObject;
                                                    OAppPotentialModel.DBTrack = DBTrack;
                                                    OAppPotentialModel.ObjectValue = OObjVallist;
                                                };
                                                OAppPotentialModellist.Add(OAppPotentialModel);
                                            }
                                        }
                                    }
                                    #endregion

                                    #region pastexp
                                    if (item.PastExperienceModelObjectV != null && OCompModelAssign.CompetencyModel.PastExperienceModel != null && item1.ModelObjectType == OCompModelAssign.CompetencyModel.PastExperienceModel.ModelName)
                                    {
                                        foreach (var itemP in item.PastExperienceModelObjectV)
                                        {
                                            additem = false;

                                            List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                            ObjectValue OObjVal = new ObjectValue();

                                            List<ObjectValue> OObjVallistF = new List<ObjectValue>();
                                            ObjectValue OObjValF = new ObjectValue();

                                            bool alreadyExists = OPastExpModellistF.Any(x => x.PastExperienceModelObject == itemP.PastExperienceModelObject);
                                            if (alreadyExists == false)
                                            {
                                                OPastExpModelF = new PastExperienceModelObjectF();
                                                {
                                                    OPastExpModelF.PastExperienceModelObject = itemP.PastExperienceModelObject;
                                                    OPastExpModelF.DBTrack = DBTrack;
                                                    OPastExpModelF.ObjectValue = OObjVallistF;
                                                };
                                                OPastExpModellistF.Add(OPastExpModelF);
                                            }

                                            if (itemP.PastExperienceModelObject.PastExperienceModel.LookupVal.ToUpper() == item1.ModelObjectSubtype.ToUpper())
                                            {
                                                foreach (var itemO in itemP.ObjectValue)
                                                {
                                                    if (item1.Criteria.ToUpper() == "EQUAL")
                                                    {
                                                        if (itemO.ObjectVal.ToUpper() == item1.Data1.ToUpper())
                                                        {
                                                            count += 1;
                                                            additem = true;
                                                            break;
                                                        }
                                                    }

                                                    if (item1.Criteria.ToUpper() == "LIKE")
                                                    {
                                                        if (itemO.ObjectVal.ToUpper().Contains(item1.Data1.ToUpper()))
                                                        {
                                                            count += 1;
                                                            additem = true;
                                                            break;

                                                        }
                                                    }
                                                }

                                            }

                                            if (additem == true && retdata.Fields.ContainsKey(item1.ModelObjectSubtype) == false)
                                            {
                                                retdata.Fields.Add(item1.ModelObjectSubtype, item1.Data1.ToString());

                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = item1.Data1.ToString();
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);


                                            }

                                            if (additem == true)
                                            {
                                                OPastExpModel = new PastExperienceModelObjectT();
                                                {
                                                    OPastExpModel.PastExperienceModelObject = itemP.PastExperienceModelObject;
                                                    OPastExpModel.DBTrack = DBTrack;
                                                    OPastExpModel.ObjectValue = OObjVallist;

                                                };
                                                OPastExpModellist.Add(OPastExpModel);
                                            }
                                        }
                                    }
                                    #endregion

                                    #region personnelmodel
                                    if (item.PersonnelModelObjectV != null && OCompModelAssign.CompetencyModel.PersonnelModel != null && item1.ModelObjectType == OCompModelAssign.CompetencyModel.PersonnelModel.ModelName)
                                    {
                                        foreach (var itemP in item.PersonnelModelObjectV)
                                        {
                                            additem = false;
                                            List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                            ObjectValue OObjVal = new ObjectValue();

                                            List<ObjectValue> OObjVallistF = new List<ObjectValue>();
                                            ObjectValue OObjValF = new ObjectValue();

                                            bool alreadyExists = OPersModellistF.Any(x => x.PersonnelModelObject == itemP.PersonnelModelObject);
                                            if (alreadyExists == false)
                                            {
                                                OObjValF = new ObjectValue();
                                                {
                                                    OObjValF.ObjectVal = item1.Data1.ToString();
                                                    OObjValF.DBTrack = DBTrack;
                                                };
                                                OObjVallistF.Add(OObjValF);

                                                OPersModelF = new PersonnelModelObjectF();
                                                {
                                                    OPersModelF.PersonnelModelObject = itemP.PersonnelModelObject;
                                                    OPersModelF.DBTrack = DBTrack;
                                                    OPersModelF.ObjectValue = OObjVallistF;
                                                };
                                                OPersModellistF.Add(OPersModelF);
                                            }
                                            if (itemP.PersonnelModelObject.PersonnelModel.LookupVal.ToUpper() == item1.ModelObjectSubtype.ToUpper())
                                            {
                                                foreach (var itemO in itemP.ObjectValue)
                                                {
                                                    //if (itemO.ObjectVal.ToUpper() == item1.Data1.ToUpper())
                                                    //{
                                                    string CrType = itemP.PersonnelModelObject.CompetencyEvaluationModel.CriteriaType.LookupVal.ToUpper();
                                                    if (CrType == "DATETIME")
                                                    {

                                                        if (item1.Criteria.ToUpper() == "GREATER THAN")
                                                        {

                                                            if (Convert.ToDateTime(itemO.ObjectVal) > Convert.ToDateTime(item1.Data1))
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                // break;
                                                            }
                                                        }
                                                        if (item1.Criteria.ToUpper() == "LESS THAN")
                                                        {
                                                            if (Convert.ToDateTime(itemO.ObjectVal) < Convert.ToDateTime(item1.Data1))
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                //break;
                                                            }
                                                        }
                                                        if (item1.Criteria.ToUpper() == "EQUAL")
                                                        {
                                                            if (Convert.ToDateTime(itemO.ObjectVal) == Convert.ToDateTime(item1.Data1))
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                //break;
                                                            }
                                                        }

                                                        if (item1.Criteria.ToUpper() == "NOT EQUAL")
                                                        {
                                                            if (Convert.ToDateTime(itemO.ObjectVal) != Convert.ToDateTime(item1.Data1))
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                //break;
                                                            }
                                                        }
                                                        if (item1.Criteria.ToUpper() == "BETWEEN")
                                                        {
                                                            if (Convert.ToDateTime(itemO.ObjectVal) >= Convert.ToDateTime(item1.Data1) && Convert.ToDateTime(itemO.ObjectVal) <= Convert.ToDateTime(item1.Data2))
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                // break;
                                                            }
                                                        }
                                                    }
                                                    if (CrType == "INT")
                                                    {

                                                        if (item1.Criteria.ToUpper() == "GREATER THAN")
                                                        {

                                                            if (Convert.ToInt32(itemO.ObjectVal) > Convert.ToInt32(item1.Data1))
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                // break;
                                                            }
                                                        }
                                                        if (item1.Criteria.ToUpper() == "LESS THAN")
                                                        {
                                                            if (Convert.ToInt32(itemO.ObjectVal) < Convert.ToInt32(item1.Data1))
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                //break;
                                                            }
                                                        }
                                                        if (item1.Criteria.ToUpper() == "EQUAL")
                                                        {
                                                            if (Convert.ToInt32(itemO.ObjectVal) == Convert.ToInt32(item1.Data1))
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                //break;
                                                            }
                                                        }

                                                        if (item1.Criteria.ToUpper() == "NOT EQUAL")
                                                        {
                                                            if (Convert.ToInt32(itemO.ObjectVal) != Convert.ToInt32(item1.Data1))
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                //break;
                                                            }
                                                        }
                                                        if (item1.Criteria.ToUpper() == "BETWEEN")
                                                        {
                                                            if (Convert.ToInt32(itemO.ObjectVal) >= Convert.ToInt32(item1.Data1) && Convert.ToInt32(itemO.ObjectVal) <= Convert.ToInt32(item1.Data2))
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                //break;
                                                            }
                                                        }
                                                    }
                                                    if (CrType == "DOUBLE")
                                                    {

                                                        if (item1.Criteria.ToUpper() == "GREATER THAN")
                                                        {

                                                            if (Convert.ToDouble(itemO.ObjectVal) > Convert.ToDouble(item1.Data1))
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                //break;
                                                            }
                                                        }
                                                        if (item1.Criteria.ToUpper() == "LESS THAN")
                                                        {
                                                            if (Convert.ToDouble(itemO.ObjectVal) < Convert.ToDouble(item1.Data1))
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                //break;
                                                            }
                                                        }
                                                        if (item1.Criteria.ToUpper() == "EQUAL")
                                                        {
                                                            if (Convert.ToDouble(itemO.ObjectVal) == Convert.ToDouble(item1.Data1))
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                //break;
                                                            }
                                                        }

                                                        if (item1.Criteria.ToUpper() == "NOT EQUAL")
                                                        {
                                                            if (Convert.ToDouble(itemO.ObjectVal) != Convert.ToDouble(item1.Data1))
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                //break;
                                                            }
                                                        }
                                                        if (item1.Criteria.ToUpper() == "BETWEEN")
                                                        {
                                                            if (Convert.ToDouble(itemO.ObjectVal) >= Convert.ToDouble(item1.Data1) && Convert.ToDouble(itemO.ObjectVal) <= Convert.ToDouble(item1.Data2))
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                //break;
                                                            }
                                                        }
                                                    }
                                                    if (CrType == "STRING")
                                                    {
                                                        if (item1.Criteria.ToUpper() == "LIKE")
                                                        {
                                                            if (itemO.ObjectVal.ToUpper().Contains(item1.Data1.ToUpper()))
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                //break;
                                                            }
                                                        }

                                                        if (item1.Criteria.ToUpper() == "EQUAL")
                                                        {
                                                            if (itemO.ObjectVal.ToUpper() == item1.Data1.ToUpper())
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                //break;
                                                            }
                                                        }
                                                        if (item1.Criteria.ToUpper() == "NOT EQUAL")
                                                        {
                                                            if (itemO.ObjectVal.ToUpper() != item1.Data1.ToUpper())
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                //break;
                                                            }
                                                        }

                                                    }

                                                    if (additem == true && retdata.Fields.ContainsKey(item1.ModelObjectSubtype) == false)
                                                    {
                                                        retdata.Fields.Add(item1.ModelObjectSubtype, itemO.ObjectVal.ToString());
                                                        OObjVal = new ObjectValue();
                                                        {
                                                            OObjVal.ObjectVal = itemO.ObjectVal.ToString();
                                                            OObjVal.DBTrack = DBTrack;
                                                        };
                                                        OObjVallist.Add(OObjVal);
                                                    }

                                                    //}
                                                }
                                            }

                                            if (additem == true)
                                            {

                                                OPersModel = new PersonnelModelObjectT();
                                                {
                                                    OPersModel.PersonnelModelObject = itemP.PersonnelModelObject;
                                                    OPersModel.DBTrack = DBTrack;
                                                    OPersModel.ObjectValue = OObjVallist;
                                                };
                                                OPersModellist.Add(OPersModel);
                                            }

                                        }
                                    }
                                    #endregion

                                    #region QualificationModel
                                    if (item.QualificationModelObjectV != null && OCompModelAssign.CompetencyModel.QualificationModel != null && item1.ModelObjectType == OCompModelAssign.CompetencyModel.QualificationModel.ModelName)
                                    {
                                        foreach (var itemP in item.QualificationModelObjectV)
                                        {
                                            additem = false;
                                            List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                            ObjectValue OObjVal = new ObjectValue();

                                            List<ObjectValue> OObjVallistF = new List<ObjectValue>();
                                            ObjectValue OObjValF = new ObjectValue();

                                            //bool alreadyExists = OQualModellistF.Any(x => x.QualificationModelObject == itemP.QualificationModelObject);
                                            //if (alreadyExists == false)
                                            //{
                                                OObjVal = new ObjectValue();
                                                {
                                                    OObjVal.ObjectVal = item1.Data1.ToString();
                                                    OObjVal.DBTrack = DBTrack;
                                                };
                                                OObjVallist.Add(OObjVal);

                                                OQualModelF = new QualificationModelObjectF();
                                                {
                                                    OQualModelF.QualificationModelObject = itemP.QualificationModelObject;
                                                    OQualModelF.DBTrack = DBTrack;
                                                    OQualModelF.ObjectValue = OObjVallistF;
                                                };
                                                OQualModellistF.Add(OQualModelF);

                                                if (itemP.QualificationModelObject.QualificationModel.LookupVal.ToUpper() == item1.ModelObjectSubtype.ToUpper())
                                                {
                                                    foreach (var itemO in itemP.ObjectValue)
                                                    {
                                                        if (item1.Criteria.ToUpper() == "EQUAL")
                                                        {
                                                            if (itemO.ObjectVal.ToUpper() == item1.Data1.ToUpper())
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                break;
                                                            }
                                                        }

                                                        if (item1.Criteria.ToUpper() == "LIKE")
                                                        {
                                                            if (itemO.ObjectVal.ToUpper().Contains(item1.Data1.ToUpper()))
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                            }
                                                        }
                                                    }

                                                }

                                                if (additem == true && retdata.Fields.ContainsKey(item1.ModelObjectSubtype) == false)
                                                {
                                                    retdata.Fields.Add(item1.ModelObjectSubtype, item1.Data1.ToString());

                                                }

                                                if (additem == true)
                                                {
                                                    OQualModel = new QualificationModelObjectT();
                                                    {
                                                        OQualModel.QualificationModelObject = itemP.QualificationModelObject;
                                                        OQualModel.DBTrack = DBTrack;
                                                    };
                                                    OQualModellist.Add(OQualModel);
                                                }
                                            //}
                                        }
                                    }
                                    #endregion

                                        #region SkillModel

                                        if (item.SkillModelObjectV != null && OCompModelAssign.CompetencyModel.SkillModel != null && item1.ModelObjectType == OCompModelAssign.CompetencyModel.SkillModel.ModelName)
                                        {
                                            foreach (var itemP in item.SkillModelObjectV)
                                            {
                                                additem = false;
                                                List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                ObjectValue OObjVal = new ObjectValue();

                                                List<ObjectValue> OObjVallistF = new List<ObjectValue>();
                                                ObjectValue OObjValF = new ObjectValue();

                                                bool alreadyExists = OSkillModellistF.Any(x => x.SkillModelObject == itemP.SkillModelObject);
                                                if (alreadyExists == false)
                                                {
                                                    OObjValF = new ObjectValue();
                                                    {
                                                        OObjValF.ObjectVal = item1.Data1.ToString();
                                                        OObjValF.DBTrack = DBTrack;
                                                    };
                                                    OObjVallistF.Add(OObjValF);

                                                    OSkillModelF = new SkillModelObjectF();
                                                    {
                                                        OSkillModelF.SkillModelObject = itemP.SkillModelObject;
                                                        OSkillModelF.DBTrack = DBTrack;
                                                        OSkillModelF.ObjectValue = OObjVallistF;
                                                    };
                                                    OSkillModellistF.Add(OSkillModelF);
                                                }

                                                if (itemP.SkillModelObject.SkillModel.LookupVal.ToUpper() == item1.ModelObjectSubtype.ToUpper())
                                                {
                                                    foreach (var itemO in itemP.ObjectValue)
                                                    {
                                                        if (item1.Criteria.ToUpper() == "EQUAL")
                                                        {
                                                            if (itemO.ObjectVal.ToUpper() == item1.Data1.ToUpper())
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                break;
                                                            }
                                                        }

                                                        if (item1.Criteria.ToUpper() == "LIKE")
                                                        {
                                                            if (itemO.ObjectVal.ToUpper().Contains(item1.Data1.ToUpper()))
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                break;
                                                            }
                                                        }
                                                    }

                                                }


                                                if (additem == true && retdata.Fields.ContainsKey(item1.ModelObjectSubtype) == false)
                                                {
                                                    retdata.Fields.Add(item1.ModelObjectSubtype, item1.Data1.ToString());

                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = item1.Data1.ToString();
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);


                                                }


                                                if (additem == true)
                                                {
                                                    OSkillModel = new SkillModelObjectT();
                                                    {
                                                        OSkillModel.SkillModelObject = itemP.SkillModelObject;
                                                        OSkillModel.DBTrack = DBTrack;
                                                        OSkillModel.ObjectValue = itemP.ObjectValue;
                                                    };
                                                    OSkillModellist.Add(OSkillModel);
                                                }
                                            }
                                        }

                                        #endregion

                                        #region ServiceModel

                                        if (item.ServiceModelObjectV != null && OCompModelAssign.CompetencyModel.ServiceModel != null && item1.ModelObjectType == OCompModelAssign.CompetencyModel.ServiceModel.ModelName)
                                        {
                                            foreach (var itemP in item.ServiceModelObjectV)
                                            {
                                                additem = false;
                                                List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                ObjectValue OObjVal = new ObjectValue();

                                                List<ObjectValue> OObjVallistF = new List<ObjectValue>();
                                                ObjectValue OObjValF = new ObjectValue();

                                                bool alreadyExists = OServModellistF.Any(x => x.ServiceModelObject == itemP.ServiceModelObject);
                                                if (alreadyExists == false)
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = item1.Data1.ToString();
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);

                                                    OServModelF = new ServiceModelObjectF();
                                                    {
                                                        OServModelF.ServiceModelObject = itemP.ServiceModelObject;
                                                        OServModelF.DBTrack = DBTrack;
                                                        OServModelF.ObjectValue = OObjVallistF;
                                                    };
                                                    OTrainingModellistF.Add(OTrainingModelF);
                                                }
                                                if (itemP.ServiceModelObject.ServiceModel.LookupVal.ToUpper() == item1.ModelObjectSubtype.ToUpper())
                                                {
                                                    foreach (var itemO in itemP.ObjectValue)
                                                    {
                                                        if (item1.Criteria.ToUpper() == "EQUAL")
                                                        {
                                                            if (itemO.ObjectVal.ToUpper() == item1.Data1.ToUpper())
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                break;
                                                            }
                                                        }

                                                        if (item1.Criteria.ToUpper() == "LIKE")
                                                        {
                                                            if (itemO.ObjectVal.ToUpper().Contains(item1.Data1.ToUpper()))
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                            }
                                                        }
                                                    }

                                                }


                                                if (additem == true && retdata.Fields.ContainsKey(item1.ModelObjectSubtype) == false)
                                                {
                                                    retdata.Fields.Add(item1.ModelObjectSubtype, item1.Data1.ToString());
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = item1.Data1.ToString();
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);


                                                }

                                                if (additem == true)
                                                {
                                                    OServModel = new ServiceModelObjectT();
                                                    {
                                                        OServModel.ServiceModelObject = itemP.ServiceModelObject;
                                                        OServModel.DBTrack = DBTrack;
                                                        OServModel.ObjectValue = OObjVallist;
                                                    };
                                                    OServModellist.Add(OServModel);
                                                }

                                            }
                                        }

                                        #endregion

                                        #region TrainingModel

                                        if (item.TrainingModelObjectV != null && OCompModelAssign.CompetencyModel.TrainingModel != null && item1.ModelObjectType == OCompModelAssign.CompetencyModel.TrainingModel.ModelName)
                                        {
                                            foreach (var itemP in item.TrainingModelObjectV)
                                            {
                                                additem = false;
                                                List<ObjectValue> OObjVallist = new List<ObjectValue>();
                                                ObjectValue OObjVal = new ObjectValue();

                                                List<ObjectValue> OObjVallistF = new List<ObjectValue>();
                                                ObjectValue OObjValF = new ObjectValue();

                                                bool alreadyExists = OTrainingModellistF.Any(x => x.TrainingModelObject == itemP.TrainingModelObject);
                                                if (alreadyExists == false)
                                                {
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = item1.Data1.ToString();
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);

                                                    OTrainingModelF = new TrainingModelObjectF();
                                                    {
                                                        OTrainingModelF.TrainingModelObject = itemP.TrainingModelObject;
                                                        OTrainingModelF.DBTrack = DBTrack;
                                                        OTrainingModelF.ObjectValue = OObjVallistF;
                                                    };
                                                    OTrainingModellistF.Add(OTrainingModelF);
                                                }

                                                if (itemP.TrainingModelObject.TrainingModel.LookupVal.ToUpper() == item1.ModelObjectSubtype.ToUpper())
                                                {
                                                    foreach (var itemO in itemP.ObjectValue)
                                                    {
                                                        if (item1.Criteria.ToUpper() == "EQUAL")
                                                        {
                                                            if (itemO.ObjectVal.ToUpper() == item1.Data1.ToUpper())
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                                break;
                                                            }
                                                        }

                                                        if (item1.Criteria.ToUpper() == "LIKE")
                                                        {
                                                            if (itemO.ObjectVal.ToUpper().Contains(item1.Data1.ToUpper()))
                                                            {
                                                                count += 1;
                                                                additem = true;
                                                            }
                                                        }
                                                    }

                                                }

                                                if (additem == true && retdata.Fields.ContainsKey(item1.ModelObjectSubtype) == false)
                                                {
                                                    retdata.Fields.Add(item1.ModelObjectSubtype, item1.Data1.ToString());
                                                    OObjVal = new ObjectValue();
                                                    {
                                                        OObjVal.ObjectVal = item1.Data1.ToString();
                                                        OObjVal.DBTrack = DBTrack;
                                                    };
                                                    OObjVallist.Add(OObjVal);
                                                }

                                                if (additem == true)
                                                {
                                                    OTrainingModel = new TrainingModelObjectT();
                                                    {
                                                        OTrainingModel.TrainingModelObject = itemP.TrainingModelObject;
                                                        OTrainingModel.DBTrack = DBTrack;
                                                        OTrainingModel.ObjectValue = OObjVallist;
                                                    };
                                                    OTrainingModellist.Add(OTrainingModel);
                                                }
                                            }
                                        }
                                        #endregion

                                    
                                    
                                }

                                    if (count == data.Count)
                                    {
                                        if (OModelObject.Count == 0)
                                        {
                                            foreach (var i in data)
                                            {
                                                OModelObject.Add(i.ModelObjectSubtype);
                                            }
                                        }



                                        //var CompetencyEmployeeDataTChk = db.CompetencyEmployeeDataT.Include(e => e.Employee).Include(e => e.BatchName)
                                        //    .Where(e => e.BatchName.Id == item.BatchName.Id && e.Employee.Id == item.Employee.Id).FirstOrDefault();

                                        //if (CompetencyEmployeeDataTChk == null)
                                        //{
                                        CompetencyEmployeeDataT CompetencyEmployeeDataT = new CompetencyEmployeeDataT();
                                        {
                                            CompetencyEmployeeDataT.DBTrack = DBTrack;
                                            CompetencyEmployeeDataT.Employee = item.Employee;
                                            CompetencyEmployeeDataT.ProcessDate = DateTime.Now;
                                            CompetencyEmployeeDataT.AppraisalAttributeModelObjectT = OAppAttModellist;
                                            CompetencyEmployeeDataT.AppraisalBusinessAppraisalModelObjectT = OAppBusiModellist;
                                            CompetencyEmployeeDataT.AppraisalKRAModelObjectT = OAppKRAModellist;
                                            CompetencyEmployeeDataT.AppraisalPotentialModelObjectT = OAppPotentialModellist;
                                            CompetencyEmployeeDataT.PastExperienceModelObjectT = OPastExpModellist;
                                            CompetencyEmployeeDataT.PersonnelModelObjectT = OPersModellist;
                                            CompetencyEmployeeDataT.QualificationModelObjectT = OQualModellist;
                                            CompetencyEmployeeDataT.ServiceModelObjectT = OServModellist;
                                            CompetencyEmployeeDataT.SkillModelObjectT = OSkillModellist;
                                            CompetencyEmployeeDataT.TrainingModelObjectT = OTrainingModellist;
                                            CompetencyEmployeeDataT.BatchName = item.BatchName;
                                        };
                                        //db.CompetencyEmployeeDataT.Add(CompetencyEmployeeDataT);
                                        //db.SaveChanges();
                                        OCompEmpDataTList.Add(CompetencyEmployeeDataT);
                                        //}

                                        bool chk1 = false;
                                        foreach (var item1 in returndata)
                                        {
                                            if (item1.EmpCode.Contains(item.Employee.EmpCode))
                                                chk1 = true;
                                        }

                                        if (chk1 == false)
                                        {
                                            GeoStruct OGeoStruct = db.GeoStruct.Find(item.Employee.GeoStruct_Id);
                                            FuncStruct OFuncStruct = db.FuncStruct.Find(item.Employee.FuncStruct_Id);
                                            LocationObj locobj = null;
                                            DepartmentObj deptobj = null;
                                            Job Jobobj = null;
                                            if (OGeoStruct.Location_Id != null)
                                            {
                                                locobj = db.Location.Include(e => e.LocationObj).Where(e => e.Id == OGeoStruct.Location_Id).FirstOrDefault().LocationObj;
                                            }
                                            if (OGeoStruct.Department_Id != null)
                                            {
                                                deptobj = db.Department.Include(e => e.DepartmentObj).Where(e => e.Id == OGeoStruct.Department_Id).FirstOrDefault().DepartmentObj;
                                            }
                                            if (OFuncStruct != null && OFuncStruct.Job_Id != null)
                                            {
                                                Jobobj = db.FuncStruct.Include(e => e.Job).Where(e => e.Id == OFuncStruct.Job_Id).FirstOrDefault().Job;
                                            }



                                            retdata.SNo = item.Id.ToString();
                                            retdata.EmpPhoto = item.Employee.Photo;
                                            retdata.EmpCode = item.Employee.EmpCode;
                                            retdata.EmpName = item.Employee.EmpName.FullNameFML;
                                            retdata.Location = locobj != null ? locobj.LocCode + "-" + locobj.LocDesc : "";
                                            retdata.Department = deptobj != null ? deptobj.DeptCode + "_" + deptobj.DeptDesc : "";
                                            retdata.Designation = Jobobj != null ? Jobobj.Code + "_" + Jobobj.Name : "";
                                            returndata.Add(retdata);
                                        }

                                    }

                                }





                                CompetencyBatchProcessT CompetencyBatchProcessT = new CompetencyBatchProcessT();
                                {
                                    CompetencyBatchProcessT.DBTrack = DBTrack;
                                    CompetencyBatchProcessT.ProcessDate = DateTime.Now;
                                    CompetencyBatchProcessT.ProcessBatch = ProcessBatch;
                                    CompetencyBatchProcessT.AppraisalAttributeModelObjectF = OAppAttModellistF;
                                    CompetencyBatchProcessT.AppraisalBusinessAppraisalModelObjectF = OAppBusiModellistF;
                                    CompetencyBatchProcessT.AppraisalKRAModelObjectF = OAppKRAModellistF;
                                    CompetencyBatchProcessT.AppraisalPotentialModelObjectF = OAppPotentialModellistF;
                                    CompetencyBatchProcessT.PastExperienceModelObjectF = OPastExpModellistF;
                                    CompetencyBatchProcessT.PersonnelModelObjectF = OPersModellistF;
                                    CompetencyBatchProcessT.QualificationModelObjectF = OQualModellistF;
                                    CompetencyBatchProcessT.ServiceModelObjectF = OServModellistF;
                                    CompetencyBatchProcessT.SkillModelObjectF = OSkillModellistF;
                                    CompetencyBatchProcessT.TrainingModelObjectF = OTrainingModellistF;
                                    CompetencyBatchProcessT.CompetencyEmployeeDataT = OCompEmpDataTList;
                                    CompetencyBatchProcessT.BatchName = db.CompetencyModelAssignment.Find(BatchNameId);
                                };
                                db.CompetencyBatchProcessT.Add(CompetencyBatchProcessT);
                                db.SaveChanges();
                                ts.Complete();

                            }

                            if (OModelObject.Count() == 0)
                            {
                                MSG = "No Record Found..!!";
                            }
                            //Session["CriteriaData"] = null;
                            //return Json(new { status = 0, MSG = MSG }, JsonRequestBehavior.AllowGet);
                            return Json(new { OModelObject, returndata, MSG }, JsonRequestBehavior.AllowGet);
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
                    MSG = (ex.Message.ToString());
                    return Json(new { OModelObject, returndata, MSG }, JsonRequestBehavior.AllowGet);
                    //return Json(new Utility.JsonReturnClass { success = false, responseText = MSG }, JsonRequestBehavior.AllowGet);
                }
            }
            return null;
        }

        [HttpPost]
        public ActionResult Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    CompetencyBatchProcessT CompBatch = db.CompetencyBatchProcessT
                                                     .Where(e => e.Id == data).FirstOrDefault();

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        DBTrack dbT = new DBTrack
                        {
                            Action = "M",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now,
                            CreatedBy = CompBatch.DBTrack.CreatedBy != null ? CompBatch.DBTrack.CreatedBy : null,
                            CreatedOn = CompBatch.DBTrack.CreatedOn != null ? CompBatch.DBTrack.CreatedOn : null,
                            IsModified = CompBatch.DBTrack.IsModified == true ? false : false

                        };
                        CompBatch.IsProcessComplete = true;
                        CompBatch.ProcessCompleteDate = DateTime.Now;
                        CompBatch.DBTrack = dbT;
                        db.Entry(CompBatch).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        ts.Complete();
                        Msg.Add("  Batch Closed.  ");
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

        //[HttpPost]
        //public ActionResult createdata(List<EmpDataTClass> data, string BatchNamelist)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        string MSG = "";

        //        int BatchNameId = int.Parse(BatchNamelist);
        //        var OCompEmpDataT = db.CompetencyEmployeeDataGeneration.Include(e => e.BatchName)
        //            .Include(e => e.AppraisalAttributeModelObjectV)
        //            .Include(e => e.AppraisalAttributeModelObjectV.AppraisalAttributeModelObject)
        //            .Include(e => e.AppraisalBusinessAppraisalModelObjectV)
        //            .Include(e => e.AppraisalBusinessAppraisalModelObjectV.AppraisalBusinessAppraisalModelObject)
        //            .Include(e => e.AppraisalKRAModelObjectV)
        //            .Include(e => e.AppraisalKRAModelObjectV.AppraisalKRAModelObject)
        //            .Include(e => e.AppraisalPotentialModelObjectV)
        //            .Include(e => e.AppraisalPotentialModelObjectV.AppraisalPotentialModelObject)
        //            .Include(e => e.PastExperienceModelObjectV)
        //            .Include(e => e.AppraisalPotentialModelObjectV.AppraisalPotentialModelObject)
        //            .Include(e => e.PersonnelModelObjectV)
        //            .Include(e => e.PersonnelModelObjectV.Select(t => t.PersonnelModelObject))
        //            .Include(e => e.QualificationModelObjectV)
        //            .Include(e => e.QualificationModelObjectV.QualificationModelObject)
        //            .Include(e => e.ServiceModelObjectV)
        //            .Include(e => e.ServiceModelObjectV.ServiceModelObject)
        //            .Include(e => e.SkillModelObjectV)
        //            .Include(e => e.SkillModelObjectV.SkillModelObject)
        //            .Include(e => e.TrainingModelObjectV)
        //            .Include(e => e.TrainingModelObjectV.TrainingModelObject)
        //            .Include(e => e.Employee)
        //            .Include(e => e.Employee.EmpName)
        //            .Where(e => e.BatchName.Id == BatchNameId)
        //            .ToList();
        //       // List<string> modelObjectNameList = new List<string>();

        //        var OCompModelAssign = db.CompetencyModelAssignment.Where(e => e.Id == BatchNameId)
        //          .Include(e => e.CompetencyModel.AppraisalAttributeModel)
        //          .Include(e => e.CompetencyModel.AppraisalBusinessApprisalModel)
        //          .Include(e => e.CompetencyModel.AppraisalKRAModel)
        //          .Include(e => e.CompetencyModel.AppraisalPotentialModel)
        //          .Include(e => e.CompetencyModel.PastExperienceModel)
        //          .Include(e => e.CompetencyModel.PersonnelModel)
        //          .Include(e => e.CompetencyModel.QualificationModel).Include(e => e.CompetencyModel.ServiceModel)
        //          .Include(e => e.CompetencyModel.SkillModel).Include(e => e.CompetencyModel.TrainingModel).FirstOrDefault();

        //        List<EmpAnalysisDataClass> returndata = new List<EmpAnalysisDataClass>();
        //        List<string> OModelObject = new List<string>();
        //        List<string> OModelObjectData = new List<string>();
        //        try
        //        {
        //            var DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
        //            if (ModelState.IsValid)
        //            {
                       

        //                    try
        //                    {
        //                        foreach (var item in OCompEmpDataT)
        //                        {

        //                            var retdata = new EmpAnalysisDataClass();
        //                            retdata.Fields = new Dictionary<string, object>();
        //                            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 60, 0)))
        //                            {
        //                                bool additem = false;
        //                                int count = 0;
        //                                AppraisalAttributeModelObjectT OAppAttModel = null;
        //                                AppraisalBusinessAppraisalModelObjectT OAppBusiModel = null;
        //                                AppraisalKRAModelObjectT OAppKRAModel = null;
        //                                AppraisalPotentialModelObjectT OAppPotentialModel = null;
        //                                PastExperienceModelObjectT OPastExpModel = null;
        //                                PersonnelModelObjectT OPersModel = null;
        //                                QualificationModelObjectT OQualModel = null;
        //                                ServiceModelObjectT OServModel = null;
        //                                SkillModelObjectT OSkillModel = null;
        //                                TrainingModelObjectT OTrainingModel = null;

        //                                foreach (var item1 in data)
        //                                {
        //                                    #region AppraisalAtt
        //                                    if (item.AppraisalAttributeModelObjectV != null && item1.ModelObjectType == OCompModelAssign.CompetencyModel.AppraisalAttributeModel.ModelName)
        //                                    {
        //                                        var OEmp = db.EmployeeAppraisal.Where(e => e.Employee.Id == item.Employee.Id)
        //                                            .Include(e => e.EmpAppEvaluation).Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion))
        //                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating)))
        //                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment))))
        //                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment.AppCategory)))).FirstOrDefault();
        //                                        foreach (var APP1 in OEmp.EmpAppEvaluation)
        //                                        {
        //                                            foreach (var APP2 in APP1.EmpAppRatingConclusion)
        //                                            {
        //                                                foreach (var APP3 in APP2.EmpAppRating.Where(t => t.AppAssignment.AppCategory.AppMode.LookupVal.ToUpper() == "ATTRIBUTE"))
        //                                                {
        //                                                    //foreach (var item1 in data)
        //                                                    //{

        //                                                    if (APP3.AppAssignment.AppCategory.Code == item1.ModelObjectSubtype.ToUpper())
        //                                                    {
        //                                                        if (item1.Criteria.ToUpper() == "GREATER THAN")
        //                                                        {
        //                                                            if (APP1.SecurePoints > Convert.ToInt32(item1.Data1))
        //                                                            {
        //                                                                additem = true;
        //                                                                retdata.Fields.Add(item1.ModelObjectSubtype, APP1.SecurePoints);
        //                                                            }
        //                                                        }
        //                                                        if (item1.Criteria.ToUpper() == "LESS THAN")
        //                                                        {
        //                                                            if (APP1.SecurePoints < Convert.ToInt32(item1.Data1))
        //                                                            {
        //                                                                additem = true;
        //                                                                retdata.Fields.Add(item1.ModelObjectSubtype, APP1.SecurePoints);
        //                                                            }
        //                                                        }
        //                                                        if (item1.Criteria.ToUpper() == "EQUAL")
        //                                                        {
        //                                                            if (APP1.SecurePoints == Convert.ToInt32(item1.Data1))
        //                                                            {
        //                                                                additem = true;
        //                                                                retdata.Fields.Add(item1.ModelObjectSubtype, APP1.SecurePoints);
        //                                                            }
        //                                                        }
        //                                                        if (item1.Criteria.ToUpper() == "NOT EQUAL")
        //                                                        {
        //                                                            if (APP1.SecurePoints != Convert.ToInt32(item1.Data1))
        //                                                            {
        //                                                                additem = true;
        //                                                                retdata.Fields.Add(item1.ModelObjectSubtype, APP1.SecurePoints);
        //                                                            }
        //                                                        }
        //                                                        if (item1.Criteria.ToUpper() == "BETWEEN")
        //                                                        {
        //                                                            if (APP1.SecurePoints >= Convert.ToInt32(item1.Data1) && APP1.SecurePoints <= Convert.ToInt32(item1.Data2))
        //                                                            {
        //                                                                additem = true;
        //                                                                retdata.Fields.Add(item1.ModelObjectSubtype, APP1.SecurePoints);
        //                                                            }
        //                                                        }

        //                                                    }
        //                                                    // }
        //                                                }
        //                                            }
        //                                        }
        //                                        if (additem == true)
        //                                        {
        //                                            OAppAttModel = new AppraisalAttributeModelObjectT();
        //                                            {
        //                                                OAppAttModel.AppraisalAttributeModelObject = item.AppraisalAttributeModelObjectV.AppraisalAttributeModelObject;
        //                                            };
        //                                        }

        //                                    }
        //                                    #endregion

        //                                    #region AppraisalBusiness
        //                                    if (item.AppraisalBusinessAppraisalModelObjectV != null && item1.ModelObjectType == OCompModelAssign.CompetencyModel.AppraisalBusinessApprisalModel.ModelName)
        //                                    {
        //                                        var OEmp = db.EmployeeAppraisal.Where(e => e.Employee.Id == item.Employee.Id)
        //                                            .Include(e => e.EmpAppEvaluation).Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion))
        //                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating)))
        //                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment))))
        //                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment.AppCategory)))).FirstOrDefault();
        //                                        foreach (var APP1 in OEmp.EmpAppEvaluation)
        //                                        {
        //                                            foreach (var APP2 in APP1.EmpAppRatingConclusion)
        //                                            {
        //                                                foreach (var APP3 in APP2.EmpAppRating.Where(t => t.AppAssignment.AppCategory.AppMode.LookupVal.ToUpper() == "ATTRIBUTE"))
        //                                                {
        //                                                    //foreach (var item1 in data)
        //                                                    //{
        //                                                        if (APP3.AppAssignment.AppCategory.Code == item1.ModelObjectSubtype.ToUpper())
        //                                                        {
        //                                                            if (item1.Criteria.ToUpper() == "GREATER THAN")
        //                                                            {
        //                                                                if (APP1.SecurePoints > Convert.ToInt32(item1.Data1))
        //                                                                {
        //                                                                    additem = true;
        //                                                                    retdata.Fields.Add(item1.ModelObjectSubtype, APP1.SecurePoints);
        //                                                                }
        //                                                            }
        //                                                            if (item1.Criteria.ToUpper() == "LESS THAN")
        //                                                            {
        //                                                                if (APP1.SecurePoints < Convert.ToInt32(item1.Data1))
        //                                                                {
        //                                                                    additem = true;
        //                                                                    retdata.Fields.Add(item1.ModelObjectSubtype, APP1.SecurePoints);
        //                                                                }
        //                                                            }
        //                                                            if (item1.Criteria.ToUpper() == "EQUAL")
        //                                                            {
        //                                                                if (APP1.SecurePoints == Convert.ToInt32(item1.Data1))
        //                                                                {
        //                                                                    additem = true;
        //                                                                    retdata.Fields.Add(item1.ModelObjectSubtype, APP1.SecurePoints);
        //                                                                }
        //                                                            }
        //                                                            if (item1.Criteria.ToUpper() == "NOT EQUAL")
        //                                                            {
        //                                                                if (APP1.SecurePoints != Convert.ToInt32(item1.Data1))
        //                                                                {
        //                                                                    additem = true;
        //                                                                    retdata.Fields.Add(item1.ModelObjectSubtype, APP1.SecurePoints);
        //                                                                }
        //                                                            }
        //                                                            if (item1.Criteria.ToUpper() == "BETWEEN")
        //                                                            {
        //                                                                if (APP1.SecurePoints >= Convert.ToInt32(item1.Data1) && APP1.SecurePoints <= Convert.ToInt32(item1.Data2))
        //                                                                {
        //                                                                    additem = true;
        //                                                                    retdata.Fields.Add(item1.ModelObjectSubtype, APP1.SecurePoints);
        //                                                                }
        //                                                            }

        //                                                        }
        //                                                    //}
        //                                                }
        //                                            }
        //                                        }

        //                                        if (additem == true)
        //                                        {
        //                                            OAppBusiModel = new AppraisalBusinessAppraisalModelObjectT();
        //                                            {
        //                                                OAppBusiModel.AppraisalBusinessAppraisalModelObject = item.AppraisalBusinessAppraisalModelObjectV.AppraisalBusinessAppraisalModelObject;
        //                                            };
        //                                        }
        //                                    }
        //                                    #endregion

        //                                    #region AppraisalKRA
        //                                    if (item.AppraisalKRAModelObjectV != null && item1.ModelObjectType == OCompModelAssign.CompetencyModel.AppraisalKRAModel.ModelName)
        //                                    {
        //                                        var OEmp = db.EmployeeAppraisal.Where(e => e.Employee.Id == item.Employee.Id)
        //                                            .Include(e => e.EmpAppEvaluation).Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion))
        //                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating)))
        //                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment))))
        //                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment.AppCategory)))).FirstOrDefault();
        //                                        foreach (var APP1 in OEmp.EmpAppEvaluation)
        //                                        {
        //                                            foreach (var APP2 in APP1.EmpAppRatingConclusion)
        //                                            {
        //                                                foreach (var APP3 in APP2.EmpAppRating.Where(t => t.AppAssignment.AppCategory.AppMode.LookupVal.ToUpper() == "ATTRIBUTE"))
        //                                                {
        //                                                    //foreach (var item1 in data)
        //                                                    //{
        //                                                        if (APP3.AppAssignment.AppCategory.Code == item1.ModelObjectSubtype.ToUpper())
        //                                                        {
        //                                                            if (item1.Criteria.ToUpper() == "GREATER THAN")
        //                                                            {
        //                                                                if (APP1.SecurePoints > Convert.ToInt32(item1.Data1))
        //                                                                {
        //                                                                    additem = true;
        //                                                                    retdata.Fields.Add(item1.ModelObjectSubtype, APP1.SecurePoints);
        //                                                                }
        //                                                            }
        //                                                            if (item1.Criteria.ToUpper() == "LESS THAN")
        //                                                            {
        //                                                                if (APP1.SecurePoints < Convert.ToInt32(item1.Data1))
        //                                                                {
        //                                                                    additem = true;
        //                                                                    retdata.Fields.Add(item1.ModelObjectSubtype, APP1.SecurePoints);
        //                                                                }
        //                                                            }
        //                                                            if (item1.Criteria.ToUpper() == "EQUAL")
        //                                                            {
        //                                                                if (APP1.SecurePoints == Convert.ToInt32(item1.Data1))
        //                                                                {
        //                                                                    additem = true;
        //                                                                    retdata.Fields.Add(item1.ModelObjectSubtype, APP1.SecurePoints);
        //                                                                }
        //                                                            }
        //                                                            if (item1.Criteria.ToUpper() == "NOT EQUAL")
        //                                                            {
        //                                                                if (APP1.SecurePoints != Convert.ToInt32(item1.Data1))
        //                                                                {
        //                                                                    additem = true;
        //                                                                    retdata.Fields.Add(item1.ModelObjectSubtype, APP1.SecurePoints);
        //                                                                }
        //                                                            }
        //                                                            if (item1.Criteria.ToUpper() == "BETWEEN")
        //                                                            {
        //                                                                if (APP1.SecurePoints >= Convert.ToInt32(item1.Data1) && APP1.SecurePoints <= Convert.ToInt32(item1.Data2))
        //                                                                {
        //                                                                    additem = true;
        //                                                                    retdata.Fields.Add(item1.ModelObjectSubtype, APP1.SecurePoints);
        //                                                                }
        //                                                            }

        //                                                        }
        //                                                    //}
        //                                                }
        //                                            }
        //                                        }
        //                                        if (additem == true)
        //                                        {
        //                                            OAppKRAModel = new AppraisalKRAModelObjectT();
        //                                            {
        //                                                OAppKRAModel.AppraisalKRAModelObject = item.AppraisalKRAModelObjectV.AppraisalKRAModelObject;
        //                                            };
        //                                        }

        //                                    }
        //                                    #endregion

        //                                    #region AppraisalPotential

        //                                    if (item.AppraisalPotentialModelObjectV != null && item1.ModelObjectType == OCompModelAssign.CompetencyModel.AppraisalPotentialModel.ModelName)
        //                                    {
        //                                        var OEmp = db.EmployeeAppraisal.Where(e => e.Employee.Id == item.Employee.Id)
        //                                            .Include(e => e.EmpAppEvaluation).Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion))
        //                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating)))
        //                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment))))
        //                                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(y => y.EmpAppRating.Select(u => u.AppAssignment.AppCategory)))).FirstOrDefault();
        //                                        foreach (var APP1 in OEmp.EmpAppEvaluation)
        //                                        {
        //                                            foreach (var APP2 in APP1.EmpAppRatingConclusion)
        //                                            {
        //                                                foreach (var APP3 in APP2.EmpAppRating.Where(t => t.AppAssignment.AppCategory.AppMode.LookupVal.ToUpper() == "POTENTIAL"))
        //                                                {
        //                                                    //foreach (var item1 in data)
        //                                                    //{
        //                                                        if (APP3.AppAssignment.AppCategory.Code == item1.ModelObjectSubtype.ToUpper())
        //                                                        {
        //                                                            if (item1.Criteria.ToUpper() == "GREATER THAN")
        //                                                            {
        //                                                                if (APP1.SecurePoints > Convert.ToInt32(item1.Data1))
        //                                                                {
        //                                                                    additem = true;
        //                                                                    retdata.Fields.Add(item1.ModelObjectSubtype, APP1.SecurePoints);
        //                                                                }
        //                                                            }
        //                                                            if (item1.Criteria.ToUpper() == "LESS THAN")
        //                                                            {
        //                                                                if (APP1.SecurePoints < Convert.ToInt32(item1.Data1))
        //                                                                {
        //                                                                    additem = true;
        //                                                                    retdata.Fields.Add(item1.ModelObjectSubtype, APP1.SecurePoints);
        //                                                                }
        //                                                            }
        //                                                            if (item1.Criteria.ToUpper() == "EQUAL")
        //                                                            {
        //                                                                if (APP1.SecurePoints == Convert.ToInt32(item1.Data1))
        //                                                                {
        //                                                                    additem = true;
        //                                                                    retdata.Fields.Add(item1.ModelObjectSubtype, APP1.SecurePoints);
        //                                                                }
        //                                                            }
        //                                                            if (item1.Criteria.ToUpper() == "NOT EQUAL")
        //                                                            {
        //                                                                if (APP1.SecurePoints != Convert.ToInt32(item1.Data1))
        //                                                                {
        //                                                                    additem = true;
        //                                                                    retdata.Fields.Add(item1.ModelObjectSubtype, APP1.SecurePoints);
        //                                                                }
        //                                                            }
        //                                                            if (item1.Criteria.ToUpper() == "BETWEEN")
        //                                                            {
        //                                                                if (APP1.SecurePoints >= Convert.ToInt32(item1.Data1) && APP1.SecurePoints <= Convert.ToInt32(item1.Data2))
        //                                                                {
        //                                                                    additem = true;
        //                                                                    retdata.Fields.Add(item1.ModelObjectSubtype, APP1.SecurePoints);
        //                                                                }
        //                                                            }

        //                                                        }
        //                                                    //}
        //                                                }
        //                                            }
        //                                        }
        //                                        if (additem == true)
        //                                        {
        //                                            OAppPotentialModel = new AppraisalPotentialModelObjectT();
        //                                            {
        //                                                OAppPotentialModel.AppraisalPotentialModelObject = item.AppraisalPotentialModelObjectV.AppraisalPotentialModelObject;
        //                                            };
        //                                        }

        //                                    }
        //                                    #endregion

        //                                    #region PastExp
        //                                    if (item.PastExperienceModelObjectV != null && item1.ModelObjectType == OCompModelAssign.CompetencyModel.PastExperienceModel.ModelName)
        //                                    {
                                                
        //                                        var OPrevEx = db.Employee.Where(e => e.Id == item.Employee.Id)
        //                                                  .Include(e => e.PreCompExp).FirstOrDefault().PreCompExp;
        //                                        if (OPrevEx != null)
        //                                        {

        //                                            if (item1.ModelObjectSubtype.ToUpper() == "LEAVINGJOBPOSITION")
        //                                            {
        //                                                // var Exp = OPrevEx.Where(e => e.LeaveingJobPosition.ToUpper() == item1.Data1.ToUpper()).FirstOrDefault();
        //                                                if (OPrevEx.Any(e => e.LeaveingJobPosition.ToUpper() == item1.Data1.ToUpper()) != false)
        //                                                {
        //                                                    count += 1;
        //                                                    additem = true;
        //                                                }
        //                                            }
        //                                            if (item1.ModelObjectSubtype.ToUpper() == "SPECIALACHIEVEMENTS")
        //                                            {
        //                                                // var Exp = OPrevEx.Where(e => e.SpecialAchievements.ToUpper() == item1.Data1.ToUpper()).FirstOrDefault();
        //                                                if (OPrevEx.Any(e => e.SpecialAchievements.ToUpper() == item1.Data1.ToUpper()) != false)
        //                                                {
        //                                                    count += 1;
        //                                                    additem = true;
        //                                                }
        //                                            }
        //                                            if (item1.ModelObjectSubtype.ToUpper() == "JOININGJOBPOSITION")
        //                                            {
        //                                                // var Exp = OPrevEx.Where(e => e.SpecialAchievements.ToUpper() == item1.Data1.ToUpper()).FirstOrDefault();
        //                                                if (OPrevEx.Any(e => e.JoiningJobPosition.ToUpper() == item1.Data1.ToUpper()) != false)
        //                                                {
        //                                                    count += 1;
        //                                                    additem = true;
        //                                                }
        //                                            }
        //                                            if (item1.ModelObjectSubtype.ToUpper() == "YROFSERVICE")
        //                                            {
        //                                                // var Exp = OPrevEx.Where(e => e.SpecialAchievements.ToUpper() == item1.Data1.ToUpper()).FirstOrDefault();
        //                                                if (OPrevEx.Any(e => e.YrOfService == Convert.ToDouble(item1.Data1)) != null)
        //                                                {
        //                                                    count += 1;
        //                                                    additem = true;
        //                                                }
        //                                            }

        //                                            if (additem == true && retdata.Fields.ContainsKey(item1.ModelObjectSubtype) == false)
        //                                            {
        //                                                retdata.Fields.Add(item1.ModelObjectSubtype, item1.Data1.ToString());
        //                                            }

        //                                        }
        //                                        if (additem == true)
        //                                        {
        //                                            OPastExpModel = new PastExperienceModelObjectT();
        //                                            {
        //                                                OPastExpModel.PastExperienceModelObject = item.PastExperienceModelObjectV.PastExperienceModelObject;
        //                                            };
        //                                        }
                                               
        //                                    }
        //                                    #endregion

        //                                    #region Personnelmodel
        //                                    if (item.PersonnelModelObjectV != null && item1.ModelObjectType == OCompModelAssign.CompetencyModel.PersonnelModel.ModelName)
        //                                    {

        //                                        //foreach (var item1 in data)
        //                                        //{
        //                                            additem = false;
        //                                            Employee OEmp = db.Employee.Where(e => e.Id == item.Employee.Id).Include(e => e.MaritalStatus)
        //                                                .Include(e => e.Gender).Include(e => e.EmpName).Include(e => e.ServiceBookDates).FirstOrDefault();
        //                                            if (item1.ModelObjectSubtype.ToUpper() == "JOININGDATE")
        //                                            {
        //                                                if (item1.Criteria.ToUpper() == "GREATER THAN")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.JoiningDate > Convert.ToDateTime(item1.Data1))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "LESS THAN")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.JoiningDate < Convert.ToDateTime(item1.Data1))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "EQUAL")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.JoiningDate == Convert.ToDateTime(item1.Data1))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "NOT EQUAL")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.JoiningDate != Convert.ToDateTime(item1.Data1))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "BETWEEN")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.JoiningDate >= Convert.ToDateTime(item1.Data1) && OEmp.ServiceBookDates.JoiningDate <= Convert.ToDateTime(item1.Data2))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (additem == true)
        //                                                {
        //                                                    //if (!OModelObject.Contains(item1.ModelObjectSubtype))
        //                                                    //{
        //                                                    //    OModelObject.Add(item1.ModelObjectSubtype);
        //                                                    // OModelObjectData.Add(OEmp.ServiceBookDates.JoiningDate.ToString());

        //                                                    retdata.Fields.Add(item1.ModelObjectSubtype, OEmp.ServiceBookDates.JoiningDate.Value.ToShortDateString());
        //                                                    //}
        //                                                }
        //                                            }

        //                                            if (item1.ModelObjectSubtype.ToUpper() == "CONFIRMATIONDATE")
        //                                            {
        //                                                if (item1.Criteria.ToUpper() == "GREATER THAN")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.ConfirmationDate > Convert.ToDateTime(item1.Data1))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "LESS THAN")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.ConfirmationDate < Convert.ToDateTime(item1.Data1))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "EQUAL")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.ConfirmationDate == Convert.ToDateTime(item1.Data1))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "NOT EQUAL")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.ConfirmationDate != Convert.ToDateTime(item1.Data1))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "BETWEEN")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.ConfirmationDate >= Convert.ToDateTime(item1.Data1) && OEmp.ServiceBookDates.ConfirmationDate <= Convert.ToDateTime(item1.Data2))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (additem == true && retdata.Fields.ContainsKey(item1.ModelObjectSubtype) == false)
        //                                                {
        //                                                    retdata.Fields.Add(item1.ModelObjectSubtype, OEmp.ServiceBookDates.ConfirmationDate.Value.ToShortDateString());
        //                                                }
        //                                            }

        //                                            if (item1.ModelObjectSubtype.ToUpper() == "RETIREMENTDATE")
        //                                            {
        //                                                if (item1.Criteria.ToUpper() == "GREATER THAN")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.RetirementDate > Convert.ToDateTime(item1.Data1))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "LESS THAN")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.RetirementDate < Convert.ToDateTime(item1.Data1))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "EQUAL")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.RetirementDate == Convert.ToDateTime(item1.Data1))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "NOT EQUAL")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.RetirementDate != Convert.ToDateTime(item1.Data1))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "BETWEEN")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.RetirementDate >= Convert.ToDateTime(item1.Data1) && OEmp.ServiceBookDates.RetirementDate <= Convert.ToDateTime(item1.Data2))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (additem == true && retdata.Fields.ContainsKey(item1.ModelObjectSubtype) == false)
        //                                                {
        //                                                    retdata.Fields.Add(item1.ModelObjectSubtype, OEmp.ServiceBookDates.RetirementDate.Value.ToShortDateString());
        //                                                }
        //                                            }

        //                                            if (item1.ModelObjectSubtype.ToUpper() == "BIRTHDATE")
        //                                            {

        //                                                if (item1.Criteria.ToUpper() == "GREATER THAN")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.BirthDate > Convert.ToDateTime(item1.Data1))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "LESS THAN")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.BirthDate < Convert.ToDateTime(item1.Data1))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "EQUAL")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.BirthDate == Convert.ToDateTime(item1.Data1))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "NOT EQUAL")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.BirthDate != Convert.ToDateTime(item1.Data1))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "BETWEEN")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.BirthDate >= Convert.ToDateTime(item1.Data1) && OEmp.ServiceBookDates.BirthDate <= Convert.ToDateTime(item1.Data2))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (additem == true && retdata.Fields.ContainsKey(item1.ModelObjectSubtype) == false)
        //                                                {
        //                                                    retdata.Fields.Add(item1.ModelObjectSubtype, OEmp.ServiceBookDates.BirthDate.Value.ToShortDateString());
        //                                                }
        //                                            }

        //                                            if (item1.ModelObjectSubtype.ToUpper() == "PROBATION DATE")
        //                                            {
        //                                                if (item1.Criteria.ToUpper() == "GREATER THAN")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.ProbationDate > Convert.ToDateTime(item1.Data1))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "LESS THAN")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.ProbationDate < Convert.ToDateTime(item1.Data1))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "EQUAL")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.ProbationDate == Convert.ToDateTime(item1.Data1))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "NOT EQUAL")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.ProbationDate != Convert.ToDateTime(item1.Data1))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "BETWEEN")
        //                                                {
        //                                                    if (OEmp.ServiceBookDates.ProbationDate >= Convert.ToDateTime(item1.Data1) && OEmp.ServiceBookDates.BirthDate <= Convert.ToDateTime(item1.Data2))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (additem == true && retdata.Fields.ContainsKey(item1.ModelObjectSubtype) == false)
        //                                                {
        //                                                    retdata.Fields.Add(item1.ModelObjectSubtype, OEmp.ServiceBookDates.BirthDate.Value.ToShortDateString());
        //                                                }
        //                                            }

        //                                            if (item1.ModelObjectSubtype.ToUpper() == "GENDER")
        //                                            {
        //                                                if (item1.Criteria.ToUpper() == "EQUAL")
        //                                                {
        //                                                    if (OEmp.Gender.LookupVal.ToUpper() == item1.Data1.ToUpper())
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "NOT EQUAL")
        //                                                {
        //                                                    if (OEmp.Gender.LookupVal.ToUpper() != item1.Data1.ToUpper())
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "LIKE")
        //                                                {
        //                                                    if (OEmp.Gender.LookupVal.ToUpper().Contains(item1.Data1.ToUpper()))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (additem == true && retdata.Fields.ContainsKey(item1.ModelObjectSubtype) == false)
        //                                                {
        //                                                    retdata.Fields.Add(item1.ModelObjectSubtype, OEmp.Gender.LookupVal.ToString());
        //                                                }

        //                                            }

        //                                            if (item1.ModelObjectSubtype.ToUpper() == "MARITALSTATUS")
        //                                            {
        //                                                if (item1.Criteria.ToUpper() == "EQUAL")
        //                                                {
        //                                                    if (OEmp.MaritalStatus != null && OEmp.MaritalStatus.LookupVal.ToUpper() == item1.Data1.ToUpper())
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "NOT EQUAL")
        //                                                {
        //                                                    if (OEmp.MaritalStatus != null && OEmp.MaritalStatus.LookupVal.ToUpper() != item1.Data1.ToUpper())
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (item1.Criteria.ToUpper() == "LIKE")
        //                                                {
        //                                                    if (OEmp.MaritalStatus != null && OEmp.MaritalStatus.LookupVal.ToUpper().Contains(item1.Data1.ToUpper()))
        //                                                    {
        //                                                        count += 1;
        //                                                        additem = true;
        //                                                    }
        //                                                }
        //                                                if (additem == true && retdata.Fields.ContainsKey(item1.ModelObjectSubtype) == false)
        //                                                {
        //                                                    retdata.Fields.Add(item1.ModelObjectSubtype, OEmp.MaritalStatus.LookupVal.ToString());
        //                                                }
        //                                            }


        //                                            if (additem == true)
        //                                            {
        //                                                //if (!OModelObject.Contains(item1.ModelObjectSubtype))
        //                                                //{
        //                                                //    OModelObject.Add(item1.ModelObjectSubtype); 
        //                                                //}

        //                                                OPersModel = new PersonnelModelObjectT();
        //                                                {
        //                                                    OPersModel.DBTrack = DBTrack;
        //                                                    OPersModel.PersonnelModelObject = item.PersonnelModelObjectV.PersonnelModelObject;
        //                                                };
        //                                            }
        //                                        //}

        //                                    }
        //                                    #endregion

        //                                    #region Qualification
        //                                    if (item.QualificationModelObjectV != null && item1.ModelObjectType == OCompModelAssign.CompetencyModel.QualificationModel.ModelName)
        //                                    {
        //                                        var OEmp = db.Employee.Include(e => e.EmpName).Include(e => e.EmpAcademicInfo).Include(e => e.EmpAcademicInfo.QualificationDetails)
        //                                            .Include(e => e.EmpAcademicInfo.QualificationDetails.Select(t => t.Qualification))
        //                                            .Where(e => e.Id == item.Employee.Id).FirstOrDefault();


        //                                        if (OEmp.EmpAcademicInfo != null && OEmp.EmpAcademicInfo.QualificationDetails.Count() > 0)
        //                                        {
        //                                            foreach (var itemQ in OEmp.EmpAcademicInfo.QualificationDetails)
        //                                            {
        //                                                //foreach (var item1 in data)
        //                                                //{
        //                                                    if (item1.Criteria.ToUpper() == "EQUAL")
        //                                                    {
        //                                                        var OQual = itemQ.Qualification.Where(t => t.QualificationDesc.ToUpper() == item1.Data1.ToUpper()).FirstOrDefault();
        //                                                        if (OQual != null)
        //                                                        {
        //                                                            count += 1;
        //                                                            additem = true;
        //                                                        }
        //                                                    }

        //                                                    if (item1.Criteria.ToUpper() == "LIKE")
        //                                                    {
        //                                                        var OQual = itemQ.Qualification.Where(t => t.QualificationDesc.ToUpper().Contains(item1.Data1.ToUpper())).FirstOrDefault();
        //                                                        if (OQual != null)
        //                                                        {
        //                                                            count += 1;
        //                                                            additem = true;
        //                                                        }
        //                                                    }

        //                                                    if (additem == true && retdata.Fields.ContainsKey(item1.ModelObjectSubtype) == false)
        //                                                    {
        //                                                        retdata.Fields.Add(item1.ModelObjectSubtype, item1.Data1);
        //                                                    }
        //                                                //}
        //                                            }
        //                                            if (additem == true)
        //                                            {
        //                                                OQualModel = new QualificationModelObjectT();
        //                                                {
        //                                                    OQualModel.DBTrack = DBTrack;
        //                                                    OQualModel.QualificationModelObject = item.QualificationModelObjectV.QualificationModelObject;
        //                                                };
        //                                            }
        //                                        }

        //                                    }
        //                                    #endregion

        //                                    #region Service
        //                                    if (item.ServiceModelObjectV != null && item1.ModelObjectType == OCompModelAssign.CompetencyModel.ServiceModel.ModelName)
        //                                    {
        //                                        //foreach (var item1 in data)
        //                                        //{
        //                                            Employee OEmp = db.Employee.Where(e => e.Id == item.Employee.Id).Include(e => e.EmpName).Include(e => e.ServiceBookDates).FirstOrDefault();
        //                                            double ServYear = Math.Round(Convert.ToDouble(OEmp.ServiceBookDates.JoiningDate - DateTime.Now), 2);


        //                                            if (item1.Criteria.ToUpper() == "GREATER THAN")
        //                                            {
        //                                                if (ServYear > Convert.ToInt32(item1.Data1))
        //                                                {
        //                                                    count += 1;
        //                                                    additem = true;
        //                                                }
        //                                            }
        //                                            if (item1.Criteria.ToUpper() == "LESS THAN")
        //                                            {
        //                                                if (ServYear < Convert.ToInt32(item1.Data1))
        //                                                {
        //                                                    count += 1;
        //                                                    additem = true;
        //                                                }
        //                                            }
        //                                            if (item1.Criteria.ToUpper() == "EQUAL")
        //                                            {
        //                                                if (ServYear == Convert.ToInt32(item1.Data1))
        //                                                {
        //                                                    count += 1;
        //                                                    additem = true;
        //                                                }
        //                                            }
        //                                            if (item1.Criteria.ToUpper() == "NOT EQUAL")
        //                                            {
        //                                                if (ServYear != Convert.ToInt32(item1.Data1))
        //                                                {
        //                                                    count += 1;
        //                                                    additem = true;
        //                                                }
        //                                            }
        //                                            if (item1.Criteria.ToUpper() == "BETWEEN")
        //                                            {
        //                                                if (ServYear >= Convert.ToInt32(item1.Data1) && ServYear <= Convert.ToInt32(item1.Data2))
        //                                                {
        //                                                    count += 1;
        //                                                    additem = true;
        //                                                }
        //                                            }
        //                                            if (additem == true && retdata.Fields.ContainsKey(item1.ModelObjectSubtype) == false)
        //                                            {
        //                                                retdata.Fields.Add(item1.ModelObjectSubtype, ServYear);
        //                                            }

        //                                        //}

        //                                        if (additem == true)
        //                                        {
        //                                            OServModel = new ServiceModelObjectT();
        //                                            {
        //                                                OServModel.DBTrack = DBTrack;
        //                                                OServModel.ServiceModelObject = item.ServiceModelObjectV.ServiceModelObject;
        //                                            };
        //                                        }
        //                                    }
        //                                    #endregion

        //                                    #region Skill
        //                                    if (item.SkillModelObjectV != null && item1.ModelObjectType == OCompModelAssign.CompetencyModel.SkillModel.ModelName)
        //                                    {
        //                                        var OEmp = db.Employee.Include(e => e.EmpName).Include(e => e.EmpAcademicInfo).Include(e => e.EmpAcademicInfo.Skill)
        //                                            .Where(e => e.Id == item.Employee.Id).FirstOrDefault();


        //                                        if (OEmp.EmpAcademicInfo != null && OEmp.EmpAcademicInfo.Skill.Count() > 0)
        //                                        {
        //                                            foreach (var itemQ in OEmp.EmpAcademicInfo.Skill)
        //                                            {
        //                                                //foreach (var item1 in data)
        //                                                //{
        //                                                    if (item1.Criteria.ToUpper() == "EQUAL")
        //                                                    {
        //                                                        if (itemQ.Name.ToUpper() == item1.Data1.ToUpper())
        //                                                        {
        //                                                            count += 1;
        //                                                            additem = true;
        //                                                        }
        //                                                    }

        //                                                    if (item1.Criteria.ToUpper() == "LIKE")
        //                                                    {
        //                                                        if (itemQ.Name.ToUpper().Contains(item1.Data1.ToUpper()))
        //                                                        {
        //                                                            count += 1;
        //                                                            additem = true;
        //                                                        }
        //                                                    }

        //                                                    if (additem == true && retdata.Fields.ContainsKey(item1.ModelObjectSubtype) == false)
        //                                                    {

        //                                                        retdata.Fields.Add(item1.ModelObjectSubtype, item1.Data1);
        //                                                    }
        //                                                //}
        //                                            }

        //                                            if (additem == true)
        //                                            {

        //                                                OSkillModel = new SkillModelObjectT();
        //                                                {
        //                                                    OSkillModel.DBTrack = DBTrack;
        //                                                    OSkillModel.SkillModelObject = item.SkillModelObjectV.SkillModelObject;
        //                                                };
        //                                            }
        //                                        }
        //                                    }
        //                                    #endregion

        //                                    #region Training
        //                                    if (item.TrainingModelObjectV != null&& item1.ModelObjectType == OCompModelAssign.CompetencyModel.TrainingModel.ModelName)
        //                                    {
        //                                        var db_data = db.EmployeeTraining.Include(e => e.TrainingDetails)
        //                                            .Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule))
        //                                            .Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule.TrainingSession))
        //                                            .Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule.TrainingSession.Select(t => t.TrainingProgramCalendar)))
        //                                            .Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule.TrainingSession.Select(t => t.TrainingProgramCalendar.ProgramList)))
        //                                            .Where(e => e.Employee_Id == item.Employee.Id).FirstOrDefault();

        //                                        var OTrCalendar = db.Calendar.Include(e => e.Name.LookupVal.ToUpper() == "TRAININGCALENDAR" && e.Default == true).FirstOrDefault();

        //                                        if (db_data.TrainingDetails.Count() > 0)
        //                                        {
        //                                            foreach (var itemT in db_data.TrainingDetails)
        //                                            {
        //                                                foreach (var itemT1 in itemT.TrainigDetailSessionInfo)
        //                                                {
        //                                                    if (itemT1.TrainingSession.TrainingProgramCalendar.StartDate >= OTrCalendar.FromDate && itemT1.TrainingSession.TrainingProgramCalendar.EndDate <= OTrCalendar.ToDate)
        //                                                    {
        //                                                        if (itemT1.TrainingSession.TrainingProgramCalendar.ProgramList.Subject.Contains(item1.Data1))
        //                                                        {
        //                                                            additem = true;
        //                                                            count += 1;
        //                                                            break;
        //                                                        }
        //                                                    }
        //                                                }
        //                                            }
        //                                            if (additem == true && retdata.Fields.ContainsKey(item1.ModelObjectSubtype) == false)
        //                                            {
        //                                                retdata.Fields.Add(item1.ModelObjectSubtype, item1.Data1);
        //                                            }
                                                    
        //                                        }
        //                                        if (additem == true)
        //                                        {
        //                                            OTrainingModel = new TrainingModelObjectT();
        //                                            {
        //                                                OTrainingModel.DBTrack = DBTrack;
        //                                                OTrainingModel.TrainingModelObject = item.TrainingModelObjectV.TrainingModelObject;
        //                                            };
        //                                        }
        //                                    }
        //                                    #endregion
        //                                }

        //                                if (additem == true && count == data.Count)
        //                                {
        //                                    if (OModelObject.Count ==0)
        //                                    {
        //                                        foreach (var i in data)
        //                                        {
        //                                            OModelObject.Add(i.ModelObjectSubtype);
        //                                        }  
        //                                    }

                                       
        //                                    var CompetencyEmployeeDataTChk = db.CompetencyEmployeeDataT.Include(e => e.Employee).Include(e => e.BatchName)
        //                                        .Where(e => e.BatchName.Id == item && e.Employee.Id == item.Employee.Id).FirstOrDefault();

        //                                    if (CompetencyEmployeeDataTChk == null)
        //                                    {
        //                                        CompetencyEmployeeDataT CompetencyEmployeeDataT = new CompetencyEmployeeDataT();
        //                                        {
        //                                            CompetencyEmployeeDataT.DBTrack = DBTrack;
        //                                            CompetencyEmployeeDataT.Employee = item.Employee;
        //                                            CompetencyEmployeeDataT.ProcessDate = DateTime.Now;
        //                                            CompetencyEmployeeDataT.AppraisalAttributeModelObjectT = OAppAttModel;
        //                                            CompetencyEmployeeDataT.AppraisalBusinessAppraisalModelObjectT = OAppBusiModel;
        //                                            CompetencyEmployeeDataT.AppraisalKRAModelObjectT = OAppKRAModel;
        //                                            CompetencyEmployeeDataT.AppraisalPotentialModelObjectT = OAppPotentialModel;
        //                                            CompetencyEmployeeDataT.PastExperienceModelObjectT = OPastExpModel;
        //                                            CompetencyEmployeeDataT.PersonnelModelObjectT = OPersModel;
        //                                            CompetencyEmployeeDataT.QualificationModelObjectT = OQualModel;
        //                                            CompetencyEmployeeDataT.ServiceModelObjectT = OServModel;
        //                                            CompetencyEmployeeDataT.SkillModelObjectT = OSkillModel;
        //                                            CompetencyEmployeeDataT.TrainingModelObjectT = OTrainingModel;
        //                                            CompetencyEmployeeDataT.BatchName = item.BatchName;
        //                                        };
        //                                        db.CompetencyEmployeeDataT.Add(CompetencyEmployeeDataT);
        //                                        db.SaveChanges();

        //                                    }

        //                                    bool chk = false;
        //                                    foreach (var item1 in returndata)
        //                                    {
        //                                        if (item1.EmpCode.Contains(item.Employee.EmpCode))
        //                                            chk = true;
        //                                    }

        //                                    if (chk == false)
        //                                    {
        //                                        GeoStruct OGeoStruct = db.GeoStruct.Find(item.Employee.GeoStruct_Id);
        //                                        FuncStruct OFuncStruct = db.FuncStruct.Find(item.Employee.FuncStruct_Id);
        //                                        LocationObj locobj = null;
        //                                        DepartmentObj deptobj = null;
        //                                        Job Jobobj = null;
        //                                        if (OGeoStruct.Location_Id != null)
        //                                        {
        //                                            locobj = db.Location.Include(e => e.LocationObj).Where(e => e.Id == OGeoStruct.Location_Id).FirstOrDefault().LocationObj;  
        //                                        }
        //                                        if (OGeoStruct.Department_Id != null)
        //                                        {
        //                                            deptobj = db.Department.Include(e => e.DepartmentObj).Where(e => e.Id == OGeoStruct.Department_Id).FirstOrDefault().DepartmentObj; 
        //                                        }
        //                                        if (OFuncStruct != null && OFuncStruct.Job_Id != null)
        //                                        {
        //                                            Jobobj = db.FuncStruct.Include(e => e.Job).Where(e => e.Id == OFuncStruct.Job_Id).FirstOrDefault().Job;
        //                                        }
                                                
                                                
                                                
        //                                        retdata.SNo = item.Id.ToString();
        //                                        retdata.EmpPhoto = item.Employee.Photo;
        //                                        retdata.EmpCode = item.Employee.EmpCode;
        //                                        retdata.EmpName = item.Employee.EmpName.FullNameFML;
        //                                        retdata.Location = locobj != null ? locobj.LocCode + "-" + locobj.LocDesc : "";
        //                                        retdata.Department = deptobj != null ? deptobj.DeptCode + "_" + deptobj.DeptDesc : "";
        //                                        retdata.Designation = Jobobj != null ? Jobobj.Code + "_" + Jobobj.Name : "";
        //                                        returndata.Add(retdata);
        //                                    }
                                           
        //                                }

        //                                ts.Complete();
        //                            }
        //                        }
        //                        if (OModelObject.Count() == 0)
        //                        {
        //                            MSG = "No Record Found..!!";
        //                        }
        //                        //Session["CriteriaData"] = null;
        //                        //return Json(new { status = 0, MSG = MSG }, JsonRequestBehavior.AllowGet);
        //                        return Json(new { OModelObject, returndata, MSG }, JsonRequestBehavior.AllowGet);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        LogFile Logfile = new LogFile();
        //                        ErrorLog Err = new ErrorLog()
        //                        {
        //                            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                            ExceptionMessage = ex.Message,
        //                            ExceptionStackTrace = ex.StackTrace,
        //                            LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                            LogTime = DateTime.Now
        //                        };
        //                        Logfile.CreateLogFile(Err);
        //                        //    List<string> Msg = new List<string>();
        //                        MSG = (ex.Message);
        //                        return Json(new { status = 0, MSG = MSG }, JsonRequestBehavior.AllowGet);
        //                    }
                        
        //            }
        //            else
        //            {
        //                StringBuilder sb = new StringBuilder("");
        //                foreach (ModelState modelState in ModelState.Values)
        //                {
        //                    foreach (ModelError error in modelState.Errors)
        //                    {
        //                        sb.Append(error.ErrorMessage);
        //                        sb.Append("." + "\n");
        //                    }
        //                }
        //                //var errorMsg = sb.ToString();
        //                //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
        //                List<string> MsgB = new List<string>();
        //                var errorMsg = sb.ToString();
        //                MsgB.Add(errorMsg);
        //                return Json(new { status = 0, MSG = MSG }, JsonRequestBehavior.AllowGet);
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
        //            MSG =(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = MSG }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}

        [HttpPost]
        public ActionResult DeleteData(string ModelObjType, string ModelObjSubType)
        {
            List<EmpDataTClass> OEvalDataClass = new List<EmpDataTClass>();
            if (Session["CriteriaData"] != null)
            {
                OEvalDataClass = (List<EmpDataTClass>)Session["CriteriaData"];
                foreach (var item in OEvalDataClass.ToList())
                {
                    if (item.ModelObjectType == ModelObjType)
                    {
                        if (item.ModelObjectSubtype == ModelObjSubType)
                        {
                            OEvalDataClass.Remove(item);
                        } 
                    }
                }
            }
           // OEvalDataClass.AddRange(data);
            Session["CriteriaData"] = OEvalDataClass;
            return Json(OEvalDataClass, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveData(List<EmpDataTClass> data)
        {
            List<EmpDataTClass> OEvalDataClass = new List<EmpDataTClass>();
            if (Session["CriteriaData"] != null)
            {
                OEvalDataClass = (List<EmpDataTClass>)Session["CriteriaData"];
                foreach (var item in OEvalDataClass.ToList())
                {
                    foreach (var item1 in data)
                    {
                        if (item.ModelObjectSubtype == item1.ModelObjectSubtype)
                        {
                            OEvalDataClass.Remove(item);
                        }
                    }
                }
            }
            OEvalDataClass.AddRange(data);
            Session["CriteriaData"] = OEvalDataClass;
            return Json(OEvalDataClass, JsonRequestBehavior.AllowGet);
        }
        public class EvaluationDataClass
        {
            public int Id { get; set; }
            public string ModelObject { get; set; }
            public string Criteria { get; set; }
            public string Data1 { get; set; }
            public string Data2 { get; set; }
        }

        public ActionResult GetExistigData(int BatchId, string ProcessBatch)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var OBatch = db.CompetencyBatchProcessT.Where(e => e.BatchName_Id == BatchId && e.ProcessBatch == ProcessBatch)
                    .Include(e => e.BatchName).Include(e => e.BatchName.CompetencyModel)
                    .Include(e => e.AppraisalAttributeModelObjectF)
                    .Include(e => e.AppraisalBusinessAppraisalModelObjectF)
                    .Include(e => e.AppraisalKRAModelObjectF)
                    .Include(e => e.AppraisalPotentialModelObjectF)
                    .Include(e => e.PastExperienceModelObjectF)
                    .Include(e => e.PersonnelModelObjectF)
                    .Include(e => e.QualificationModelObjectF).Include(e => e.ServiceModelObjectF)
                    .Include(e => e.SkillModelObjectF).Include(e => e.TrainingModelObjectF).FirstOrDefault();

                if (OBatch != null)
                {
                    int SNo = 1;
                    List<EmpDataTClass> returndata = new List<EmpDataTClass>();
                    if (OBatch.AppraisalAttributeModelObjectF != null && OBatch.AppraisalAttributeModelObjectF.Count() >0)
                    {
                        foreach (var item in OBatch.AppraisalAttributeModelObjectF)
                        {
                             AppraisalAttributeModelObjectF OExp = db.AppraisalAttributeModelObjectF.Where(e => e.Id == item.Id)
                            .Include(e => e.AppraisalAttributeModelObject)
                            .Include(e => e.AppraisalAttributeModelObject.AppraisalAttributeModel)
                            .Include(e => e.AppraisalAttributeModelObject.CompetencyEvaluationModel)
                            .Include(e => e.AppraisalAttributeModelObject.CompetencyEvaluationModel.Criteria)
                            .Include(e => e.ObjectValue).FirstOrDefault();

                             if (returndata.Count() > 0)
                             {
                                 SNo = Convert.ToInt32(returndata.LastOrDefault().SNo) + 1;
                             }

                             returndata.Add(new EmpDataTClass
                             {
                                 SNo = SNo.ToString(),
                                 ModelName = OBatch.BatchName.CompetencyModel.ModelName,
                                 ModelObjectType = db.AppraisalAttributeModel.Find(OBatch.BatchName.CompetencyModel.AppraisalAttributeModel_Id).ModelName,
                                 ModelObjectSubtype = OExp.AppraisalAttributeModelObject.AppraisalAttributeModel.LookupVal,
                                 Criteria = OExp.AppraisalAttributeModelObject.CompetencyEvaluationModel.Criteria.LookupVal.ToString(),
                                 Data1 = OExp.ObjectValue.FirstOrDefault().ObjectVal,
                                 Data2 = OExp.ObjectValue.Count() > 1 ? OExp.ObjectValue.LastOrDefault().ObjectVal : ""
                             });
                        }
                        
                      //  return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    if (OBatch.AppraisalBusinessAppraisalModelObjectF != null && OBatch.AppraisalBusinessAppraisalModelObjectF.Count() > 0)
                    {
                        foreach (var item in OBatch.AppraisalBusinessAppraisalModelObjectF)
                        {
                            AppraisalBusinessAppraisalModelObjectF OExp = db.AppraisalBusinessAppraisalModelObjectF.Where(e => e.Id == item.Id)
                            .Include(e => e.AppraisalBusinessAppraisalModelObject)
                            .Include(e => e.AppraisalBusinessAppraisalModelObject.CompetencyEvaluationModel)
                            .Include(e => e.AppraisalBusinessAppraisalModelObject.AppraisalBusinessAppraisalModel)
                            .Include(e => e.AppraisalBusinessAppraisalModelObject.CompetencyEvaluationModel.Criteria)
                            .Include(e => e.ObjectValue).FirstOrDefault();

                            if (returndata.Count() > 0)
                            {
                                SNo = Convert.ToInt32(returndata.LastOrDefault().SNo) + 1;
                            }

                            returndata.Add(new EmpDataTClass
                            {
                                SNo = SNo.ToString(),
                                ModelName = OBatch.BatchName.CompetencyModel.ModelName,
                                ModelObjectType = db.AppraisalBusinessAppraisalModel.Find(OBatch.BatchName.CompetencyModel.AppraisalBusinessAppraisalModel_Id).ModelName,
                                ModelObjectSubtype = OExp.AppraisalBusinessAppraisalModelObject.AppraisalBusinessAppraisalModel.LookupVal,
                                Criteria = OExp.AppraisalBusinessAppraisalModelObject.CompetencyEvaluationModel.Criteria.LookupVal.ToString(),
                                Data1 = OExp.ObjectValue.FirstOrDefault().ObjectVal,
                                Data2 = OExp.ObjectValue.Count() > 1 ? OExp.ObjectValue.LastOrDefault().ObjectVal : ""
                            });
                        }
                        

                      //  return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    if (OBatch.AppraisalKRAModelObjectF != null && OBatch.AppraisalKRAModelObjectF.Count() > 0)
                    {
                        foreach (var item in OBatch.AppraisalKRAModelObjectF)
                        {
                            AppraisalKRAModelObjectF OExp = db.AppraisalKRAModelObjectF.Where(e => e.Id == item.Id)
                            .Include(e => e.AppraisalKRAModelObject)
                            .Include(e => e.AppraisalKRAModelObject.CompetencyEvaluationModel)
                            .Include(e => e.AppraisalKRAModelObject.AppraisalKRAModel)
                            .Include(e => e.AppraisalKRAModelObject.CompetencyEvaluationModel.Criteria)
                            .Include(e => e.ObjectValue).FirstOrDefault();
                            if (returndata.Count() > 0)
                            {
                                SNo = Convert.ToInt32(returndata.LastOrDefault().SNo) + 1;
                            }

                            returndata.Add(new EmpDataTClass
                            {
                                SNo = SNo.ToString(),
                                ModelName = OBatch.BatchName.CompetencyModel.ModelName,
                                ModelObjectType = db.AppraisalKRAModel.Find(OBatch.BatchName.CompetencyModel.AppraisalKRAModel_Id).ModelName,
                                ModelObjectSubtype = OExp.AppraisalKRAModelObject.AppraisalKRAModel.LookupVal,
                                Criteria = OExp.AppraisalKRAModelObject.CompetencyEvaluationModel.Criteria.LookupVal.ToString(),
                                Data1 = OExp.ObjectValue.FirstOrDefault().ObjectVal,
                                Data2 = OExp.ObjectValue.Count() > 1 ? OExp.ObjectValue.LastOrDefault().ObjectVal : ""
                            });
                        }

                       // return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    if (OBatch.AppraisalPotentialModelObjectF != null && OBatch.AppraisalPotentialModelObjectF.Count() >0)
                    {
                        foreach (var item in OBatch.AppraisalPotentialModelObjectF)
                        {
                            AppraisalPotentialModelObjectF OExp = db.AppraisalPotentialModelObjectF.Where(e => e.Id == item.Id)
                            .Include(e => e.AppraisalPotentialModelObject)
                            .Include(e => e.AppraisalPotentialModelObject.CompetencyEvaluationModel)
                            .Include(e => e.AppraisalPotentialModelObject.AppraisalPotentialModel)
                            .Include(e => e.AppraisalPotentialModelObject.CompetencyEvaluationModel.Criteria)
                            .Include(e => e.ObjectValue).FirstOrDefault();

                            if (returndata.Count() > 0)
                            {
                                SNo = Convert.ToInt32(returndata.LastOrDefault().SNo) + 1;
                            }

                            returndata.Add(new EmpDataTClass
                            {
                                SNo = SNo.ToString(),
                                ModelName = OBatch.BatchName.CompetencyModel.ModelName,
                                ModelObjectType = db.AppraisalPotentialModel.Find(OBatch.BatchName.CompetencyModel.AppraisalPotentialModel_Id).ModelName,
                                ModelObjectSubtype = OExp.AppraisalPotentialModelObject.AppraisalPotentialModel.LookupVal,
                                Criteria = OExp.AppraisalPotentialModelObject.CompetencyEvaluationModel.Criteria.LookupVal.ToString(),
                                Data1 = OExp.ObjectValue.FirstOrDefault().ObjectVal,
                                Data2 = OExp.ObjectValue.Count() > 1 ? OExp.ObjectValue.LastOrDefault().ObjectVal : ""
                            });
                        }

                        //return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    if (OBatch.PastExperienceModelObjectF != null)
                    {
                        foreach (var item in OBatch.PastExperienceModelObjectF)
                        {
                            PastExperienceModelObjectF OExp = db.PastExperienceModelObjectF.Where(e => e.Id == item.Id)
                            .Include(e => e.PastExperienceModelObject)
                            .Include(e => e.PastExperienceModelObject.CompetencyEvaluationModel)
                            .Include(e => e.PastExperienceModelObject.PastExperienceModel)
                            .Include(e => e.PastExperienceModelObject.CompetencyEvaluationModel.Criteria)
                            .Include(e => e.PastExperienceModelObject.CompetencyEvaluationModel.CriteriaType)
                            .Include(e => e.PastExperienceModelObject.CompetencyEvaluationModel.DataSteps).FirstOrDefault();

                            if (returndata.Count() > 0)
                            {
                                SNo = Convert.ToInt32(returndata.LastOrDefault().SNo) + 1;
                            }

                            returndata.Add(new EmpDataTClass
                            {
                                SNo = SNo.ToString(),
                                ModelName = OBatch.BatchName.CompetencyModel.ModelName,
                                ModelObjectType = db.PastExperienceModel.Find(OBatch.BatchName.CompetencyModel.PastExperienceModel_Id).ModelName,
                                ModelObjectSubtype = OExp.PastExperienceModelObject.PastExperienceModel.LookupVal,
                                Criteria = OExp.PastExperienceModelObject.CompetencyEvaluationModel.Criteria.LookupVal.ToString(),
                                Data1 = OExp.ObjectValue.FirstOrDefault().ObjectVal,
                                Data2 = OExp.ObjectValue.Count() > 1 ? OExp.ObjectValue.LastOrDefault().ObjectVal : ""
                            });
                        }
                        //return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    if (OBatch.PersonnelModelObjectF != null)
                    {
                        foreach (var item in OBatch.PersonnelModelObjectF)
                        {
                            PersonnelModelObjectF OExp = db.PersonnelModelObjectF.Where(e => e.Id == item.Id)
                            .Include(e => e.PersonnelModelObject)
                            .Include(e => e.PersonnelModelObject.CompetencyEvaluationModel)
                            .Include(e => e.PersonnelModelObject.PersonnelModel)
                            .Include(e => e.PersonnelModelObject.CompetencyEvaluationModel.Criteria)
                            .Include(e => e.ObjectValue).FirstOrDefault();

                            if (returndata.Count() > 0)
                            {
                                SNo = Convert.ToInt32(returndata.LastOrDefault().SNo) + 1;
                            }

                            returndata.Add(new EmpDataTClass
                            {
                                SNo = SNo.ToString(),
                                ModelName = OBatch.BatchName.CompetencyModel.ModelName,
                                ModelObjectType = db.PersonnelModel.Find(OBatch.BatchName.CompetencyModel.PersonnelModel_Id).ModelName,
                                ModelObjectSubtype = OExp.PersonnelModelObject.PersonnelModel.LookupVal,
                                Criteria = OExp.PersonnelModelObject.CompetencyEvaluationModel.Criteria.LookupVal.ToString(),
                                Data1 = OExp.ObjectValue.FirstOrDefault().ObjectVal,
                                Data2 = OExp.ObjectValue.Count() > 1 ? OExp.ObjectValue.LastOrDefault().ObjectVal : ""
                            });
                        }
                       // return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    if (OBatch.QualificationModelObjectF != null)
                    {
                        foreach (var item in OBatch.QualificationModelObjectF)
                        {
                            QualificationModelObjectF OExp = db.QualificationModelObjectF.Where(e => e.Id == item.Id)
                            .Include(e => e.QualificationModelObject)
                            .Include(e => e.QualificationModelObject.CompetencyEvaluationModel)
                            .Include(e => e.QualificationModelObject.QualificationModel)
                            .Include(e => e.QualificationModelObject.CompetencyEvaluationModel.Criteria)
                            .Include(e => e.ObjectValue).FirstOrDefault();


                            if (returndata.Count() > 0)
                            {
                                SNo = Convert.ToInt32(returndata.LastOrDefault().SNo) + 1;
                            }

                            returndata.Add(new EmpDataTClass
                            {
                                SNo = SNo.ToString(),
                                ModelName = OBatch.BatchName.CompetencyModel.ModelName,
                                ModelObjectType = db.QualificationModel.Find(OBatch.BatchName.CompetencyModel.SkillModel_Id).ModelName,
                                ModelObjectSubtype = OExp.QualificationModelObject.QualificationModel.LookupVal,
                                Criteria = OExp.QualificationModelObject.CompetencyEvaluationModel.Criteria.LookupVal.ToString(),
                                Data1 = OExp.ObjectValue.FirstOrDefault().ObjectVal,
                                Data2 = OExp.ObjectValue.Count() > 1 ? OExp.ObjectValue.LastOrDefault().ObjectVal : ""
                            });
                        }

                       // return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    if (OBatch.ServiceModelObjectF != null)
                    {
                        foreach (var item in OBatch.ServiceModelObjectF)
                        {
                            ServiceModelObjectF OExp = db.ServiceModelObjectF.Where(e => e.Id == item.Id)
                            .Include(e => e.ServiceModelObject)
                            .Include(e => e.ServiceModelObject.CompetencyEvaluationModel)
                            .Include(e => e.ServiceModelObject.ServiceModel)
                            .Include(e => e.ServiceModelObject.CompetencyEvaluationModel.Criteria)
                            .Include(e => e.ObjectValue).FirstOrDefault();

                            if (returndata.Count() > 0)
                            {
                                SNo = Convert.ToInt32(returndata.LastOrDefault().SNo) + 1;
                            }

                            returndata.Add(new EmpDataTClass
                            {
                                SNo = SNo.ToString(),
                                ModelName = OBatch.BatchName.CompetencyModel.ModelName,
                                ModelObjectType = db.ServiceModel.Find(OBatch.BatchName.CompetencyModel.ServiceModel_Id).ModelName,
                                ModelObjectSubtype = OExp.ServiceModelObject.ServiceModel.LookupVal,
                                Criteria = OExp.ServiceModelObject.CompetencyEvaluationModel.Criteria.LookupVal.ToString(),
                                Data1 = OExp.ObjectValue.FirstOrDefault().ObjectVal,
                                Data2 = OExp.ObjectValue.Count() > 1 ? OExp.ObjectValue.LastOrDefault().ObjectVal : ""
                            });
                        }
                      //  return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    if (OBatch.SkillModelObjectF != null && OBatch.SkillModelObjectF.Count() > 0)
                    {
                        foreach (var item in OBatch.SkillModelObjectF)
                        {
                            SkillModelObjectF OExp = db.SkillModelObjectF.Where(e => e.Id == item.Id)
                            .Include(e => e.SkillModelObject)
                            .Include(e => e.SkillModelObject.CompetencyEvaluationModel)
                            .Include(e => e.SkillModelObject.SkillModel)
                            .Include(e => e.SkillModelObject.CompetencyEvaluationModel.Criteria)
                            .Include(e => e.ObjectValue).FirstOrDefault();

                            
                            if (returndata.Count() >0)
                            {
                                 SNo = Convert.ToInt32(returndata.LastOrDefault().SNo) + 1;   
                            }
                           
                            returndata.Add(new EmpDataTClass
                            {
                                SNo = SNo.ToString() ,
                                ModelName = OBatch.BatchName.CompetencyModel.ModelName,
                                ModelObjectType = db.SkillModel.Find(OBatch.BatchName.CompetencyModel.SkillModel_Id).ModelName,
                                ModelObjectSubtype = OExp.SkillModelObject.SkillModel.LookupVal,
                                Criteria = OExp.SkillModelObject.CompetencyEvaluationModel.Criteria.LookupVal.ToString(),
                                Data1 = OExp.ObjectValue.FirstOrDefault().ObjectVal,
                                Data2 = OExp.ObjectValue.Count() > 1 ? OExp.ObjectValue.LastOrDefault().ObjectVal : ""
                            });
                        }

                       // return Json(returndata, JsonRequestBehavior.AllowGet);
                    }

                    if (OBatch.TrainingModelObjectF != null)
                    {
                        foreach (var item in OBatch.TrainingModelObjectF)
                        {
                            TrainingModelObjectF OExp = db.TrainingModelObjectF.Where(e => e.Id == item.Id)
                            .Include(e => e.TrainingModelObject)
                            .Include(e => e.TrainingModelObject.CompetencyEvaluationModel)
                            .Include(e => e.TrainingModelObject.TrainingModel)
                            .Include(e => e.TrainingModelObject.CompetencyEvaluationModel.Criteria)
                            .Include(e => e.ObjectValue).FirstOrDefault();

                            if (returndata.Count() > 0)
                            {
                                SNo = Convert.ToInt32(returndata.LastOrDefault().SNo) + 1;
                            }

                            returndata.Add(new EmpDataTClass
                            {
                                SNo = SNo.ToString(),
                                ModelName = OBatch.BatchName.CompetencyModel.ModelName,
                                ModelObjectType = db.TrainingModel.Find(OBatch.BatchName.CompetencyModel.TrainingModel_Id).ModelName,
                                ModelObjectSubtype = OExp.TrainingModelObject.TrainingModel.LookupVal,
                                Criteria = OExp.TrainingModelObject.CompetencyEvaluationModel.Criteria.LookupVal.ToString(),
                                Data1 = OExp.ObjectValue.FirstOrDefault().ObjectVal,
                                Data2 = OExp.ObjectValue.Count() > 1 ? OExp.ObjectValue.LastOrDefault().ObjectVal : ""
                            });
                        }
                       // return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    Session["CriteriaData"] = returndata;
                    return Json(returndata, JsonRequestBehavior.AllowGet);
                }

            }
            return null;
        }

        public ActionResult GetModelData(int BatchId, int ModelId, int ModelobjId, string ModelName)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var OBatch = db.CompetencyModelAssignment.Where(e => e.Id == BatchId)
                    .Include(e => e.CompetencyModel.AppraisalAttributeModel)
                    .Include(e => e.CompetencyModel.AppraisalBusinessApprisalModel)
                    .Include(e => e.CompetencyModel.AppraisalKRAModel)
                    .Include(e => e.CompetencyModel.AppraisalPotentialModel)
                    .Include(e => e.CompetencyModel.PastExperienceModel)
                    .Include(e => e.CompetencyModel.PersonnelModel)
                    .Include(e => e.CompetencyModel.QualificationModel).Include(e => e.CompetencyModel.ServiceModel)
                    .Include(e => e.CompetencyModel.SkillModel).Include(e => e.CompetencyModel.TrainingModel).FirstOrDefault();

                //var OBatch = db.CompetencyModel.Where(e => e.Id == ModelId)
                //    .Include(e=>e.AppraisalAttributeModel)
                //    .Include(e=>e.AppraisalBusinessApprisalModel)
                //    .Include(e=>e.AppraisalKRAModel)
                //    .Include(e=>e.AppraisalPotentialModel)
                //    .Include(e=>e.PastExperienceModel)
                //    .Include(e => e.PersonnelModel)
                //    .Include(e => e.QualificationModel).Include(e => e.ServiceModel)
                //    .Include(e => e.SkillModel).Include(e => e.TrainingModel).FirstOrDefault();
                if (OBatch != null)
                {
                    
                    List<EvaluationDataClass> returndata = new List<EvaluationDataClass>();
                    if (OBatch.CompetencyModel.AppraisalAttributeModel != null && OBatch.CompetencyModel.AppraisalAttributeModel.ModelName == ModelName)
                    {
                        AppraisalAttributeModel OExp = db.AppraisalAttributeModel.Where(e => e.Id == OBatch.CompetencyModel.AppraisalAttributeModel.Id)
                            .Include(e => e.AppraisalAttributeModelObject)
                             .Include(e => e.AppraisalAttributeModelObject.Select(t => t.CompetencyEvaluationModel))
                            .Include(e => e.AppraisalAttributeModelObject.Select(t => t.AppraisalAttributeModel))
                            .Include(e => e.AppraisalAttributeModelObject.Select(t => t.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.AppraisalAttributeModelObject.Select(t => t.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.AppraisalAttributeModelObject.Select(t => t.CompetencyEvaluationModel.DataSteps)).FirstOrDefault();
                            

                        var query22 = OExp.AppraisalAttributeModelObject.Where(e => e.AppraisalAttributeModel.Id == ModelobjId).FirstOrDefault();

                            returndata.Add(new EvaluationDataClass
                            {
                                Id = query22.Id,
                                ModelObject = query22.AppraisalAttributeModel.LookupVal,
                                Criteria = query22.CompetencyEvaluationModel.Criteria.LookupVal.ToString(),
                                Data1 = query22.CompetencyEvaluationModel.DataSteps != null ? query22.CompetencyEvaluationModel.DataSteps.LookupVal.ToString() : ""
                            });
                        
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    if (OBatch.CompetencyModel.AppraisalBusinessApprisalModel != null && OBatch.CompetencyModel.AppraisalBusinessApprisalModel.ModelName == ModelName)
                    {
                        AppraisalBusinessAppraisalModel OExp = db.AppraisalBusinessAppraisalModel.Where(e => e.Id == OBatch.CompetencyModel.AppraisalBusinessApprisalModel.Id)
                            .Include(e => e.AppraisalBusinessAppraisalModelObject)
                            .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(t => t.CompetencyEvaluationModel))
                            .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(t => t.AppraisalBusinessAppraisalModel))
                            .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(t => t.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(t => t.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(t => t.CompetencyEvaluationModel.DataSteps)).FirstOrDefault();


                        var query22 = OExp.AppraisalBusinessAppraisalModelObject.Where(e => e.AppraisalBusinessAppraisalModel.Id == ModelobjId).FirstOrDefault();

                        
                            returndata.Add(new EvaluationDataClass
                            {
                                Id = query22.Id,
                                ModelObject = query22.AppraisalBusinessAppraisalModel.LookupVal,
                                Criteria = query22.CompetencyEvaluationModel.Criteria.LookupVal.ToString(),
                                Data1 = query22.CompetencyEvaluationModel.DataSteps != null ? query22.CompetencyEvaluationModel.DataSteps.LookupVal.ToString() : ""
                            });
                        
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    if (OBatch.CompetencyModel.AppraisalKRAModel != null && OBatch.CompetencyModel.AppraisalKRAModel.ModelName == ModelName)
                    {
                        AppraisalKRAModel OExp = db.AppraisalKRAModel.Where(e => e.Id == OBatch.CompetencyModel.AppraisalKRAModel.Id)
                            .Include(e => e.AppraisalKRAModelObject)
                             .Include(e => e.AppraisalKRAModelObject.Select(t => t.CompetencyEvaluationModel))
                            .Include(e => e.AppraisalKRAModelObject.Select(t => t.AppraisalKRAModel))
                            .Include(e => e.AppraisalKRAModelObject.Select(t => t.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.AppraisalKRAModelObject.Select(t => t.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.AppraisalKRAModelObject.Select(t => t.CompetencyEvaluationModel.DataSteps)).FirstOrDefault();


                        var query22 = OExp.AppraisalKRAModelObject.Where(e => e.Id == ModelobjId).FirstOrDefault();

                        
                            returndata.Add(new EvaluationDataClass
                            {
                                Id = query22.Id,
                                ModelObject = query22.AppraisalKRAModel.LookupVal,
                                Criteria = query22.CompetencyEvaluationModel.Criteria.LookupVal.ToString(),
                                Data1 = query22.CompetencyEvaluationModel.DataSteps != null ? query22.CompetencyEvaluationModel.DataSteps.LookupVal.ToString() : ""
                            });
                        
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    if (OBatch.CompetencyModel.AppraisalPotentialModel != null && OBatch.CompetencyModel.AppraisalPotentialModel.ModelName == ModelName)
                    {
                        AppraisalPotentialModel OExp = db.AppraisalPotentialModel.Where(e => e.Id == OBatch.CompetencyModel.AppraisalPotentialModel.Id)
                            .Include(e => e.AppraisalPotentialModelObject)
                            
                             .Include(e => e.AppraisalPotentialModelObject.Select(t => t.CompetencyEvaluationModel))
                            .Include(e => e.AppraisalPotentialModelObject.Select(t => t.AppraisalPotentialModel))
                            .Include(e => e.AppraisalPotentialModelObject.Select(t => t.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.AppraisalPotentialModelObject.Select(t => t.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.AppraisalPotentialModelObject.Select(t => t.CompetencyEvaluationModel.DataSteps)).FirstOrDefault();

                        var query22 = OExp.AppraisalPotentialModelObject.Where(e => e.Id == ModelobjId).FirstOrDefault();

                        
                            returndata.Add(new EvaluationDataClass
                            {
                                Id = query22.Id,
                                ModelObject = query22.AppraisalPotentialModel.LookupVal,
                                Criteria = query22.CompetencyEvaluationModel.Criteria.LookupVal.ToString(),
                                Data1 = query22.CompetencyEvaluationModel.DataSteps != null ? query22.CompetencyEvaluationModel.DataSteps.LookupVal.ToString() : ""
                            });
                        
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    if (OBatch.CompetencyModel.PastExperienceModel != null && OBatch.CompetencyModel.PastExperienceModel.ModelName == ModelName)
                    {
                        PastExperienceModel OExp = db.PastExperienceModel.Where(e => e.Id == OBatch.CompetencyModel.PastExperienceModel.Id)
                            .Include(e => e.PastExperienceModelObject)
                             .Include(e => e.PastExperienceModelObject.Select(t => t.CompetencyEvaluationModel))
                            .Include(e => e.PastExperienceModelObject.Select(t => t.PastExperienceModel))
                            .Include(e => e.PastExperienceModelObject.Select(t => t.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.PastExperienceModelObject.Select(t => t.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.PastExperienceModelObject.Select(t => t.CompetencyEvaluationModel.DataSteps)).FirstOrDefault();


                        var query22 = OExp.PastExperienceModelObject.Where(e => e.Id == ModelobjId).FirstOrDefault();

                        
                            returndata.Add(new EvaluationDataClass
                            {
                                Id = query22.Id,
                                ModelObject = query22.PastExperienceModel.LookupVal,
                                Criteria = query22.CompetencyEvaluationModel.Criteria.LookupVal.ToString(),
                                Data1 = query22.CompetencyEvaluationModel.DataSteps != null ? query22.CompetencyEvaluationModel.DataSteps.LookupVal.ToString() : ""
                            });
                        
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    if (OBatch.CompetencyModel.PersonnelModel != null && OBatch.CompetencyModel.PersonnelModel.ModelName == ModelName)
                    {
                        PersonnelModel OExp = db.PersonnelModel.Where(e => e.Id == OBatch.CompetencyModel.PersonnelModel.Id)
                            .Include(e => e.PersonnelModelObject)
                            .Include(e => e.PersonnelModelObject.Select(t => t.PersonnelModel))
                            .Include(e => e.PersonnelModelObject.Select(t => t.CompetencyEvaluationModel))
                            .Include(e => e.PersonnelModelObject.Select(t => t.CompetencyEvaluationModel))
                            .Include(e => e.PersonnelModelObject.Select(t => t.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.PersonnelModelObject.Select(t => t.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.PersonnelModelObject.Select(t => t.CompetencyEvaluationModel.DataSteps)).FirstOrDefault();

                        var query22 = OExp.PersonnelModelObject.Where(e => e.Id == ModelobjId).FirstOrDefault();

                        
                            returndata.Add(new EvaluationDataClass
                            {
                                Id = query22.Id,
                                ModelObject = query22.PersonnelModel.LookupVal,
                                Criteria = query22.CompetencyEvaluationModel.Criteria.LookupVal.ToString(),
                                Data1 = query22.CompetencyEvaluationModel.DataSteps != null ? query22.CompetencyEvaluationModel.DataSteps.LookupVal.ToString() : ""
                            });
                        
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    if (OBatch.CompetencyModel.QualificationModel != null && OBatch.CompetencyModel.QualificationModel.ModelName == ModelName)
                    {
                        QualificationModel OExp = db.QualificationModel.Where(e => e.Id == OBatch.CompetencyModel.QualificationModel.Id)
                            .Include(e => e.QualificationModelObject)
                            .Include(e => e.QualificationModelObject.Select(t => t.CompetencyEvaluationModel))
                            .Include(e => e.QualificationModelObject.Select(t => t.QualificationModel))
                            .Include(e => e.QualificationModelObject.Select(t => t.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.QualificationModelObject.Select(t => t.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.QualificationModelObject.Select(t => t.CompetencyEvaluationModel.DataSteps)).FirstOrDefault();

                        var query22 = OExp.QualificationModelObject.Where(e => e.Id == ModelobjId).FirstOrDefault();

                        
                            returndata.Add(new EvaluationDataClass
                            {
                                Id = query22.Id,
                                ModelObject = query22.QualificationModel.LookupVal,
                                Criteria = query22.CompetencyEvaluationModel.Criteria.LookupVal.ToString(),
                                Data1 = query22.CompetencyEvaluationModel.DataSteps != null ? query22.CompetencyEvaluationModel.DataSteps.LookupVal.ToString() : ""
                            });
                        
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    if (OBatch.CompetencyModel.ServiceModel != null && OBatch.CompetencyModel.ServiceModel.ModelName == ModelName)
                    {
                        ServiceModel OExp = db.ServiceModel.Where(e => e.Id == OBatch.CompetencyModel.ServiceModel.Id)
                            .Include(e => e.ServiceModelObject)
                            .Include(e => e.ServiceModelObject.Select(t => t.CompetencyEvaluationModel))
                            .Include(e => e.ServiceModelObject.Select(t => t.ServiceModel))
                            .Include(e => e.ServiceModelObject.Select(t => t.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.ServiceModelObject.Select(t => t.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.ServiceModelObject.Select(t => t.CompetencyEvaluationModel.DataSteps)).FirstOrDefault();

                        var query22 = OExp.ServiceModelObject.Where(e => e.Id == ModelobjId).FirstOrDefault();
                        
                            returndata.Add(new EvaluationDataClass
                            {
                                Id = query22.Id,
                                ModelObject = query22.ServiceModel.LookupVal,
                                Criteria = query22.CompetencyEvaluationModel.Criteria.LookupVal.ToString(),
                                Data1 = query22.CompetencyEvaluationModel.DataSteps != null ? query22.CompetencyEvaluationModel.DataSteps.LookupVal.ToString() : ""
                            });
                        
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    if (OBatch.CompetencyModel.SkillModel != null && OBatch.CompetencyModel.SkillModel.ModelName == ModelName)
                    {
                        SkillModel OExp = db.SkillModel.Where(e => e.Id == OBatch.CompetencyModel.SkillModel.Id)
                            .Include(e => e.SkillModelObject)
                            .Include(e => e.SkillModelObject.Select(t => t.CompetencyEvaluationModel))
                            .Include(e => e.SkillModelObject.Select(t => t.SkillModel))
                            .Include(e => e.SkillModelObject.Select(t => t.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.SkillModelObject.Select(t => t.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.SkillModelObject.Select(t => t.CompetencyEvaluationModel.DataSteps)).FirstOrDefault();

                        var query22 = OExp.SkillModelObject.Where(e => e.Id == ModelobjId).FirstOrDefault();
                       
                            returndata.Add(new EvaluationDataClass
                            {
                                Id = query22.Id,
                                ModelObject = query22.SkillModel.LookupVal,
                                Criteria = query22.CompetencyEvaluationModel.Criteria.LookupVal.ToString(),
                                Data1 = query22.CompetencyEvaluationModel.DataSteps != null ? query22.CompetencyEvaluationModel.DataSteps.LookupVal.ToString() : ""
                            });
                        
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }

                    if (OBatch.CompetencyModel.TrainingModel != null && OBatch.CompetencyModel.TrainingModel.ModelName == ModelName)
                    {
                        TrainingModel OQuali = db.TrainingModel.Where(e => e.Id == OBatch.CompetencyModel.TrainingModel.Id)
                            .Include(e => e.TrainingModelObject)
                            .Include(e => e.TrainingModelObject.Select(t => t.CompetencyEvaluationModel))
                            .Include(e => e.TrainingModelObject.Select(t => t.TrainingModel))
                            .Include(e => e.TrainingModelObject.Select(t => t.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.TrainingModelObject.Select(t => t.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.TrainingModelObject.Select(t => t.CompetencyEvaluationModel.DataSteps)).FirstOrDefault();

                        var query22 = OQuali.TrainingModelObject.Where(e => e.Id == ModelobjId).FirstOrDefault();

                        returndata.Add(new EvaluationDataClass
                        {
                            Id = query22.Id,
                            ModelObject = query22.TrainingModel.LookupVal,
                            Criteria = query22.CompetencyEvaluationModel.Criteria.LookupVal.ToString(),
                            Data1 = query22.CompetencyEvaluationModel.DataSteps != null ? query22.CompetencyEvaluationModel.DataSteps.LookupVal.ToString() : ""
                        });

                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                }

            }
            return null;
        }

        public class P2BGridData
        {
            public int Id { get; set; }
            public string ProcessBatch { get; set; }
            public string ProcessDate { get; set; }
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> SalaryList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;
                    string PayMonth = "";
                    string Month = "";
                    if (gp.filter != null)
                        PayMonth = gp.filter;
                    else
                    {
                        if (DateTime.Now.Date.Month < 10)
                            Month = "0" + DateTime.Now.Date.Month;
                        else
                            Month = DateTime.Now.Date.Month.ToString();
                        PayMonth = Month + "/" + DateTime.Now.Date.Year;
                    }

                   
                    var tt = db.CompetencyBatchProcessT.Include(e => e.BatchName)
                        .Where(e => e.BatchName.BatchName == PayMonth).ToList();
                 //   var BindEmpList = db.Employee.Include(e => e.EmpName).Where(e => tt.Contains(e.Id)).ToList();

                    foreach (var z in tt)
                    {
                       
                        view = new P2BGridData()
                        {
                            Id = z.Id,
                            ProcessBatch = z.ProcessBatch,
                            ProcessDate = z.ProcessDate.Value.ToShortDateString(),
                           
                        };
                        model.Add(view);
                      
                    }

                    SalaryList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = SalaryList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.ProcessBatch.ToString().Contains(gp.searchString))
                                  || (e.ProcessDate.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                  || (e.Id.ToString().Contains(gp.searchString))
                                  )
                              .Select(a => new Object[] { a.ProcessBatch, a.ProcessDate, a.Id }).ToList();



                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.ProcessBatch, a.ProcessDate, a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = SalaryList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "ProcessBatch" ? c.ProcessBatch.ToString() :
                                             gp.sidx == "ProcessDate" ? c.ProcessDate.ToString() : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.ProcessBatch, a.ProcessDate, a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.ProcessBatch, a.ProcessDate, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.ProcessBatch, a.ProcessDate, a.Id }).ToList();
                        }
                        totalRecords = SalaryList.Count();
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
        }
    
    }
}


