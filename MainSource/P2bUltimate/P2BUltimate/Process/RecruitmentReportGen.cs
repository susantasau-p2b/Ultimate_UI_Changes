using Attendance;
using Microsoft.Reporting.WebForms;
using Leave;
using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
using ReportPayroll;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using Training;
using Recruitment;
using P2B.PFTRUST;
using CMS_SPS;



namespace P2BUltimate.Process
{
    public class RecruitmentReportGen
    {
        public class EncashHeadData
        {
            public SalaryHead SalHead { get; set; }
            public double Amount { get; set; }
        };
        public static List<GenericField100> GenerateRecruitmentReport(int CompanyPayrollId, List<int> EmpPayrollIdList, List<string> mPayMonth, string mObjectName, int CompanyId, List<string> oth_idlist, List<string> salheadlist, List<string> loanadvidlist, List<string> forithead, List<string> SpecialGroupslist, DateTime mFromDate, DateTime mToDate, DateTime pFromDate, DateTime pToDate, List<string> salheadlistLevel1, List<string> salheadlistLevel2, string ReportType)
        {

            List<GenericField100> OGenericPayrollStatement = new List<GenericField100>();
            using (DataBaseContext db = new DataBaseContext())
            {
                switch (mObjectName)
                {
                    case "CMSOBJ":

                        List<string> ModelType = new List<string>();

                        if (salheadlist.Count() > 0)
                        {
                            var Lookupvaluesdetails = db.Lookup.Where(e => e.Code == "507").Select(e => e.LookupValues).SingleOrDefault();
                            ModelType = Lookupvaluesdetails.Where(e => salheadlist.Contains(e.LookupVal)).Select(r => r.LookupVal).ToList();
                        }
                        else
                        {
                            var Lookupvaluesdetails = db.Lookup.Where(e => e.Code == "507").Select(e => e.LookupValues).SingleOrDefault();
                            ModelType = Lookupvaluesdetails.Select(r => r.LookupVal).ToList();
                        }

                        foreach (var sa in ModelType)
                        {
                            if (sa.ToUpper() == "APPRAISALATTRIBUTEMODEL")
                            {
                                var A1 = db.AppraisalAttributeModel
                                    .Include(e => e.AppraisalAttributeModelObject)
                                    .Include(e => e.AppraisalAttributeModelObject.Select(r => r.AppraisalAttributeModel))
                                    .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel))
                                    .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                                    .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                                    .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                                    .ToList();

                                if (salheadlistLevel1.Count() > 0)
                                {
                                    A1 = A1.Where(z => salheadlistLevel1.Contains(z.Code)).ToList();
                                }
                                else
                                {
                                    A1 = A1.ToList();
                                }
                                foreach (var item1A1 in A1)
                                {
                                    foreach (var item2A1 in item1A1.AppraisalAttributeModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                          {
                                              Fld3 = "AppraisalAttributeModel",
                                              Fld4 = item1A1.Code == null ? "" : item1A1.Code,
                                              Fld5 = item1A1.ModelName == null ? "" : item1A1.ModelName,
                                              Fld6 = "AppraisalAttributeModelObject",
                                              Fld7 = item2A1.AppraisalAttributeModel.LookupVal == null ? "" : item2A1.AppraisalAttributeModel.LookupVal,
                                              Fld8 = item2A1.CompetencyEvaluationModel.FullDetails == null ? "" : item2A1.CompetencyEvaluationModel.FullDetails,
                                          };

                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                    salheadlistLevel1.Remove(item1A1.Code);
                                }

                            }

                            else if (sa.ToUpper() == "APPRAISALBUSINESSAPPRAISALMODEL")
                            {

                                var A2 = db.AppraisalBusinessAppraisalModel
                                    .Include(e => e.AppraisalBusinessAppraisalModelObject)
                                    .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.AppraisalBusinessAppraisalModel))
                                    .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel))
                                    .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                                    .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                                    .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                                    .ToList();

                                if (salheadlistLevel1.Count() > 0)
                                {
                                    A2 = A2.Where(z => salheadlistLevel1.Contains(z.Code)).ToList();
                                }
                                else
                                {
                                    A2 = A2.ToList();
                                }

                                foreach (var item1A2 in A2)
                                {
                                    foreach (var item2A2 in item1A2.AppraisalBusinessAppraisalModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld3 = "AppraisalBusinessAppraisalModel",
                                            Fld4 = item1A2.Code == null ? "" : item1A2.Code,
                                            Fld5 = item1A2.ModelName == null ? "" : item1A2.ModelName,
                                            Fld6 = "AppraisalBusinessAppraisalModelObject",
                                            Fld7 = item2A2.AppraisalBusinessAppraisalModel.LookupVal == null ? "" : item2A2.AppraisalBusinessAppraisalModel.LookupVal,
                                            Fld8 = item2A2.CompetencyEvaluationModel.FullDetails == null ? "" : item2A2.CompetencyEvaluationModel.FullDetails,
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                    salheadlistLevel1.Remove(item1A2.Code);
                                }

                            }

                            else if (sa.ToUpper() == "APPRAISALKRAMODEL")
                            {

                                var A3 = db.AppraisalKRAModel
                                         .Include(e => e.AppraisalKRAModelObject)
                                         .Include(e => e.AppraisalKRAModelObject.Select(r => r.AppraisalKRAModel))
                                         .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel))
                                         .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                                         .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                                         .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                                         .ToList();

                                if (salheadlistLevel1.Count() > 0)
                                {
                                    A3 = A3.Where(z => salheadlistLevel1.Contains(z.Code)).ToList();
                                }
                                else
                                {
                                    A3 = A3.ToList();
                                }
                                foreach (var item1A3 in A3)
                                {
                                    foreach (var item2A3 in item1A3.AppraisalKRAModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld3 = "AppraisalKRAModel",
                                            Fld4 = item1A3.Code == null ? "" : item1A3.Code,
                                            Fld5 = item1A3.ModelName == null ? "" : item1A3.ModelName,
                                            Fld6 = "AppraisalKRAModelObject",
                                            Fld7 = item2A3.AppraisalKRAModel.LookupVal == null ? "" : item2A3.AppraisalKRAModel.LookupVal,
                                            Fld8 = item2A3.CompetencyEvaluationModel.FullDetails == null ? "" : item2A3.CompetencyEvaluationModel.FullDetails,
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                    salheadlistLevel1.Remove(item1A3.Code);
                                }

                            }
                            else if (sa.ToUpper() == "APPRAISALPOTENTIALMODEL")
                            {


                                var A4 = db.AppraisalPotentialModel
                                        .Include(e => e.AppraisalPotentialModelObject)
                                        .Include(e => e.AppraisalPotentialModelObject.Select(r => r.AppraisalPotentialModel))
                                        .Include(e => e.AppraisalPotentialModelObject.Select(r => r.CompetencyEvaluationModel))
                                        .Include(e => e.AppraisalPotentialModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                                        .Include(e => e.AppraisalPotentialModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                                        .Include(e => e.AppraisalPotentialModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                                        .ToList();
                                if (salheadlistLevel1.Count() > 0)
                                {
                                    A4 = A4.Where(z => salheadlistLevel1.Contains(z.Code)).ToList();
                                }
                                else
                                {
                                    A4 = A4.ToList();
                                }

                                foreach (var item1A4 in A4)
                                {
                                    foreach (var item2A4 in item1A4.AppraisalPotentialModelObject)
                                    {

                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld3 = "AppraisalPotentialModel",
                                            Fld4 = item1A4.Code == null ? "" : item1A4.Code,
                                            Fld5 = item1A4.ModelName == null ? "" : item1A4.ModelName,
                                            Fld6 = "AppraisalPotentialModelObject",
                                            Fld7 = item2A4.AppraisalPotentialModel.LookupVal == null ? "" : item2A4.AppraisalPotentialModel.LookupVal,
                                            Fld8 = item2A4.CompetencyEvaluationModel.FullDetails == null ? "" : item2A4.CompetencyEvaluationModel.FullDetails,
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                    salheadlistLevel1.Remove(item1A4.Code);
                                }

                            }

                            else if (sa.ToUpper() == "PASTEXPERIENCEMODEL")
                            {

                                var A5 = db.PastExperienceModel
                                      .Include(e => e.PastExperienceModelObject)
                                      .Include(e => e.PastExperienceModelObject.Select(r => r.PastExperienceModel))
                                      .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel))
                                      .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                                      .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                                      .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                                      .ToList();

                                if (salheadlistLevel1.Count() > 0)
                                {
                                    A5 = A5.Where(z => salheadlistLevel1.Contains(z.Code)).ToList();
                                }
                                else
                                {
                                    A5 = A5.ToList();
                                }
                                foreach (var item1A5 in A5)
                                {
                                    foreach (var item2A5 in item1A5.PastExperienceModelObject)
                                    {

                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld3 = "PastExperienceModel",
                                            Fld4 = item1A5.Code == null ? "" : item1A5.Code,
                                            Fld5 = item1A5.ModelName == null ? "" : item1A5.ModelName,
                                            Fld6 = "PastExperienceModelObject",
                                            Fld7 = item2A5.PastExperienceModel.LookupVal == null ? "" : item2A5.PastExperienceModel.LookupVal,
                                            Fld8 = item2A5.CompetencyEvaluationModel.FullDetails == null ? "" : item2A5.CompetencyEvaluationModel.FullDetails,
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                    salheadlistLevel1.Remove(item1A5.Code);
                                }

                            }

                            else if (sa.ToUpper() == "PERSONNELMODEL")
                            {
                                var A6 = db.PersonnelModel
                                      .Include(e => e.PersonnelModelObject)
                                      .Include(e => e.PersonnelModelObject.Select(r => r.PersonnelModel))
                                      .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel))
                                      .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                                      .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                                      .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                                      .ToList();

                                if (salheadlistLevel1.Count() > 0)
                                {
                                    A6 = A6.Where(z => salheadlistLevel1.Contains(z.Code)).ToList();
                                }
                                else
                                {
                                    A6 = A6.ToList();
                                }
                                foreach (var item1A6 in A6)
                                {
                                    foreach (var item2A6 in item1A6.PersonnelModelObject)
                                    {

                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld3 = "PersonnelModel",
                                            Fld4 = item1A6.Code == null ? "" : item1A6.Code,
                                            Fld5 = item1A6.ModelName == null ? "" : item1A6.ModelName,
                                            Fld6 = "PersonnelModelObject",
                                            Fld7 = item2A6.PersonnelModel.LookupVal == null ? "" : item2A6.PersonnelModel.LookupVal,
                                            Fld8 = item2A6.CompetencyEvaluationModel.FullDetails == null ? "" : item2A6.CompetencyEvaluationModel.FullDetails,
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                    salheadlistLevel1.Remove(item1A6.Code);
                                }
                            }

                            else if (sa.ToUpper() == "QUALIFICATIONMODEL")
                            {
                                var A7 = db.QualificationModel
                                     .Include(e => e.QualificationModelObject)
                                     .Include(e => e.QualificationModelObject.Select(r => r.QualificationModel))
                                     .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel))
                                     .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                                     .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                                     .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                                     .ToList();

                                if (salheadlistLevel1.Count() > 0)
                                {
                                    A7 = A7.Where(z => salheadlistLevel1.Contains(z.Code)).ToList();
                                }
                                else
                                {
                                    A7 = A7.ToList();
                                }
                                foreach (var item1A7 in A7)
                                {
                                    foreach (var item2A7 in item1A7.QualificationModelObject)
                                    {

                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld3 = "QualificationModel",
                                            Fld4 = item1A7.Code == null ? "" : item1A7.Code,
                                            Fld5 = item1A7.ModelName == null ? "" : item1A7.ModelName,
                                            Fld6 = "QualificationModelObject",
                                            Fld7 = item2A7.QualificationModel.LookupVal == null ? "" : item2A7.QualificationModel.LookupVal,
                                            Fld8 = item2A7.CompetencyEvaluationModel.FullDetails == null ? "" : item2A7.CompetencyEvaluationModel.FullDetails,
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                    salheadlistLevel1.Remove(item1A7.Code);
                                }

                            }

                            else if (sa.ToUpper() == "SERVICEMODEL")
                            {

                                var A8 = db.ServiceModel
                                    .Include(e => e.ServiceModelObject)
                                    .Include(e => e.ServiceModelObject.Select(r => r.ServiceModel))
                                    .Include(e => e.ServiceModelObject.Select(r => r.CompetencyEvaluationModel))
                                    .Include(e => e.ServiceModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                                    .Include(e => e.ServiceModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                                    .Include(e => e.ServiceModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                                    .ToList();

                                if (salheadlistLevel1.Count() > 0)
                                {
                                    A8 = A8.Where(z => salheadlistLevel1.Contains(z.Code)).ToList();
                                }
                                else
                                {
                                    A8 = A8.ToList();
                                }

                                foreach (var item1A8 in A8)
                                {
                                    foreach (var item2A8 in item1A8.ServiceModelObject)
                                    {

                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld3 = "ServiceModel",
                                            Fld4 = item1A8.Code == null ? "" : item1A8.Code,
                                            Fld5 = item1A8.ModelName == null ? "" : item1A8.ModelName,
                                            Fld6 = "ServiceModelObject",
                                            Fld7 = item2A8.ServiceModel.LookupVal == null ? "" : item2A8.ServiceModel.LookupVal,
                                            Fld8 = item2A8.CompetencyEvaluationModel.FullDetails == null ? "" : item2A8.CompetencyEvaluationModel.FullDetails,
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                    salheadlistLevel1.Remove(item1A8.Code);
                                }

                            }

                            else if (sa.ToUpper() == "SKILLMODEL")
                            {
                                var A9 = db.SkillModel
                                   .Include(e => e.SkillModelObject)
                                   .Include(e => e.SkillModelObject.Select(r => r.SkillModel))
                                   .Include(e => e.SkillModelObject.Select(r => r.CompetencyEvaluationModel))
                                   .Include(e => e.SkillModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                                   .Include(e => e.SkillModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                                   .Include(e => e.SkillModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                                   .ToList();


                                if (salheadlistLevel1.Count() > 0)
                                {
                                    A9 = A9.Where(z => salheadlistLevel1.Contains(z.Code)).ToList();
                                }
                                else
                                {
                                    A9 = A9.ToList();
                                }

                                foreach (var item1A9 in A9)
                                {
                                    foreach (var item2A9 in item1A9.SkillModelObject)
                                    {

                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld3 = "SkillModel",
                                            Fld4 = item1A9.Code == null ? "" : item1A9.Code,
                                            Fld5 = item1A9.ModelName == null ? "" : item1A9.ModelName,
                                            Fld6 = "SkillModelObject",
                                            Fld7 = item2A9.SkillModel.LookupVal == null ? "" : item2A9.SkillModel.LookupVal,
                                            Fld8 = item2A9.CompetencyEvaluationModel.FullDetails == null ? "" : item2A9.CompetencyEvaluationModel.FullDetails,
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                    salheadlistLevel1.Remove(item1A9.Code);
                                }

                            }

                            else if (sa.ToUpper() == "TRAININGMODEL")
                            {
                                var A10 = db.TrainingModel
                                  .Include(e => e.TrainingModelObject)
                                  .Include(e => e.TrainingModelObject.Select(r => r.TrainingModel))
                                  .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel))
                                  .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                                  .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                                  .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                                  .ToList();

                                if (salheadlistLevel1.Count() > 0)
                                {
                                    A10 = A10.Where(z => salheadlistLevel1.Contains(z.Code)).ToList();
                                }
                                else
                                {
                                    A10 = A10.ToList();
                                }

                                foreach (var item1A10 in A10)
                                {
                                    foreach (var item2A10 in item1A10.TrainingModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld3 = "TrainingModel",
                                            Fld4 = item1A10.Code == null ? "" : item1A10.Code,
                                            Fld5 = item1A10.ModelName == null ? "" : item1A10.ModelName,
                                            Fld6 = "TrainingModelObject",
                                            Fld7 = item2A10.TrainingModel.LookupVal == null ? "" : item2A10.TrainingModel.LookupVal,
                                            Fld8 = item2A10.CompetencyEvaluationModel.FullDetails == null ? "" : item2A10.CompetencyEvaluationModel.FullDetails,
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                    salheadlistLevel1.Remove(item1A10.Code);
                                }

                            }
                            else
                            {

                            }

                        }

                        return OGenericPayrollStatement;

                        break;

                    case "COMPETENCYMODEL":

                        var IE = db.CompetencyModel.Select(e => new
                        {
                            OCompetencyModelCode = e.Code,
                            OCompetenncyModelName = e.ModelName,
                            OCompetenncyMdelCreatedDate = e.CreatedDate,
                            OAppraisalAttributeModel = e.AppraisalAttributeModel,
                            OAttributeModelCode = e.AppraisalAttributeModel.Code,
                            OAttributeModelName = e.AppraisalAttributeModel.ModelName,
                            OAppraisalAttributeModelObject = e.AppraisalAttributeModel.AppraisalAttributeModelObject.Select(d => new
                            {
                                OALookupAppraisalAttributeModel = d.AppraisalAttributeModel.LookupVal,
                                OACompetencyEvaluationModelCriteria = d.CompetencyEvaluationModel.Criteria.LookupVal,
                                OACompetencyEvaluationModelCriteriaType = d.CompetencyEvaluationModel.CriteriaType.LookupVal,
                                OACompetencyEvaluationModelDataSteps = d.CompetencyEvaluationModel.DataSteps.LookupVal

                            }).ToList(),
                            OAppraisalBusinessApprisalModel = e.AppraisalBusinessApprisalModel,
                            OBusinessApprisalModelCode = e.AppraisalBusinessApprisalModel.Code,
                            OBusinessApprisalModelName = e.AppraisalBusinessApprisalModel.ModelName,
                            OAppraisalBusinessAppraisalModelObject = e.AppraisalBusinessApprisalModel.AppraisalBusinessAppraisalModelObject.Select(d => new
                            {
                                OLookupAppraisalBusinessAppraisalModel = d.AppraisalBusinessAppraisalModel.LookupVal,
                                OBCompetencyEvaluationModelCriteria = d.CompetencyEvaluationModel.Criteria.LookupVal,
                                OBCompetencyEvaluationModelCriteriaType = d.CompetencyEvaluationModel.CriteriaType.LookupVal,
                                OBCompetencyEvaluationModelDataSteps = d.CompetencyEvaluationModel.DataSteps.LookupVal

                            }).ToList(),
                            OAppraisalKRAModel = e.AppraisalKRAModel,
                            OKRAModelCode = e.AppraisalKRAModel.Code,
                            OKRAModelName = e.AppraisalKRAModel.ModelName,
                            OAppraisalKRAModelObject = e.AppraisalKRAModel.AppraisalKRAModelObject.Select(d => new
                            {
                                OLookupAppraisalKRAModel = d.AppraisalKRAModel.LookupVal,
                                OKCompetencyEvaluationModelCriteria = d.CompetencyEvaluationModel.Criteria.LookupVal,
                                OKCompetencyEvaluationModelCriteriaType = d.CompetencyEvaluationModel.CriteriaType.LookupVal,
                                OKCompetencyEvaluationModelDataSteps = d.CompetencyEvaluationModel.DataSteps.LookupVal

                            }).ToList(),
                            OAppraisalPotentialModel = e.AppraisalPotentialModel,
                            OPotentialModelCode = e.AppraisalPotentialModel.Code,
                            OPotentialModelName = e.AppraisalPotentialModel.ModelName,
                            OAppraisalPotentialModelObject = e.AppraisalPotentialModel.AppraisalPotentialModelObject.Select(d => new
                            {
                                OLookupAppraisalPotentialModel = d.AppraisalPotentialModel.LookupVal,
                                OPoCompetencyEvaluationModelCriteria = d.CompetencyEvaluationModel.Criteria.LookupVal,
                                OPoCompetencyEvaluationModelCriteriaType = d.CompetencyEvaluationModel.CriteriaType.LookupVal,
                                OPoCompetencyEvaluationModelDataSteps = d.CompetencyEvaluationModel.DataSteps.LookupVal

                            }).ToList(),
                            OPastExperienceModel = e.PastExperienceModel,
                            OPastExperienceModelCode = e.PastExperienceModel.Code,
                            OPastExperienceModelName = e.PastExperienceModel.ModelName,
                            OPastExperienceModelObject = e.PastExperienceModel.PastExperienceModelObject.Select(d => new
                            {
                                OLookupPastExperienceModel = d.PastExperienceModel.LookupVal,
                                OPaCompetencyEvaluationModelCriteria = d.CompetencyEvaluationModel.Criteria.LookupVal,
                                OPaCompetencyEvaluationModelCriteriaType = d.CompetencyEvaluationModel.CriteriaType.LookupVal,
                                OPaCompetencyEvaluationModelDataSteps = d.CompetencyEvaluationModel.DataSteps.LookupVal

                            }).ToList(),
                            OPersonnelModel = e.PersonnelModel,
                            OPersonnelModelCode = e.PersonnelModel.Code,
                            OPersonnelModelName = e.PersonnelModel.ModelName,
                            OPersonnelModelObject = e.PersonnelModel.PersonnelModelObject.Select(d => new
                            {
                                OLookupPersonnelModell = d.PersonnelModel.LookupVal,
                                OPerCompetencyEvaluationModelCriteria = d.CompetencyEvaluationModel.Criteria.LookupVal,
                                OPerCompetencyEvaluationModelCriteriaType = d.CompetencyEvaluationModel.CriteriaType.LookupVal,
                                OPerCompetencyEvaluationModelDataSteps = d.CompetencyEvaluationModel.DataSteps.LookupVal

                            }).ToList(),
                            OQualificationModel = e.QualificationModel,
                            OQualificationModelCode = e.QualificationModel.Code,
                            OQualificationModelName = e.QualificationModel.ModelName,
                            OQualificationModelObject = e.QualificationModel.QualificationModelObject.Select(d => new
                            {
                                OLookupQualificationModel = d.QualificationModel.LookupVal,
                                OQCompetencyEvaluationModelCriteria = d.CompetencyEvaluationModel.Criteria.LookupVal,
                                OQCompetencyEvaluationModelCriteriaType = d.CompetencyEvaluationModel.CriteriaType.LookupVal,
                                OQCompetencyEvaluationModelDataSteps = d.CompetencyEvaluationModel.DataSteps.LookupVal

                            }).ToList(),
                            OServiceModel = e.ServiceModel,
                            OServiceModelCode = e.ServiceModel.Code,
                            OServiceModelName = e.ServiceModel.ModelName,
                            OServiceModelObject = e.ServiceModel.ServiceModelObject.Select(d => new
                            {
                                OLookupServiceModel = d.ServiceModel.LookupVal,
                                OSerCompetencyEvaluationModelCriteria = d.CompetencyEvaluationModel.Criteria.LookupVal,
                                OSerCompetencyEvaluationModelCriteriaType = d.CompetencyEvaluationModel.CriteriaType.LookupVal,
                                OSerCompetencyEvaluationModelDataSteps = d.CompetencyEvaluationModel.DataSteps.LookupVal

                            }).ToList(),
                            OSkillModel = e.SkillModel,
                            OSkillModelCode = e.SkillModel.Code,
                            OSkillModelName = e.SkillModel.ModelName,
                            OSkillModelObject = e.SkillModel.SkillModelObject.Select(d => new
                            {
                                OLookupSkillModel = d.SkillModel.LookupVal,
                                OSkiCompetencyEvaluationModelCriteria = d.CompetencyEvaluationModel.Criteria.LookupVal,
                                OSkiCompetencyEvaluationModelCriteriaType = d.CompetencyEvaluationModel.CriteriaType.LookupVal,
                                OSkiCompetencyEvaluationModelDataSteps = d.CompetencyEvaluationModel.DataSteps.LookupVal

                            }).ToList(),
                            OTrainingModel = e.TrainingModel,
                            OTrainingModelCode = e.TrainingModel.Code,
                            OTrainingModelName = e.TrainingModel.ModelName,
                            OTrainingModelObject = e.TrainingModel.TrainingModelObject.Select(d => new
                            {
                                OLookupTrainingModel = d.TrainingModel.LookupVal,
                                OTCompetencyEvaluationModelCriteria = d.CompetencyEvaluationModel.Criteria.LookupVal,
                                OTCompetencyEvaluationModelCriteriaType = d.CompetencyEvaluationModel.CriteriaType.LookupVal,
                                OTCompetencyEvaluationModelDataSteps = d.CompetencyEvaluationModel.DataSteps.LookupVal

                            }).ToList()

                        }).ToList();

                        if (salheadlist.Count() > 0)
                        {
                            IE = IE.Where(e => salheadlist.Contains(e.OCompetencyModelCode)).ToList();

                        }
                        else
                        {
                            IE = IE.ToList();
                        }

                        if (IE == null && IE.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var item in IE)
                            {
                                if (item.OAppraisalAttributeModelObject.Count() > 0)
                                {
                                    foreach (var item1 in item.OAppraisalAttributeModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld1 = item.OCompetencyModelCode == null ? "" : item.OCompetencyModelCode,
                                            Fld2 = item.OCompetenncyModelName == null ? "" : item.OCompetenncyModelName,
                                            Fld3 = item.OCompetenncyMdelCreatedDate == null ? "" : item.OCompetenncyMdelCreatedDate.Value.ToShortDateString(),
                                            Fld4 = "AppraisalAttributeModel",
                                            Fld5 = (item.OAttributeModelCode + item.OAttributeModelName) == null ? "" : (item.OAttributeModelCode + " " + item.OAttributeModelName),
                                            Fld6 = "AppraisalAttributeModelObject",
                                            Fld7 = item1.OALookupAppraisalAttributeModel == null ? "" : item1.OALookupAppraisalAttributeModel,
                                            Fld8 = (item1.OACompetencyEvaluationModelCriteria + item1.OACompetencyEvaluationModelCriteriaType + item1.OACompetencyEvaluationModelDataSteps) == null ? "" : ("Evaluation" + " " + "Criteria :" + item1.OACompetencyEvaluationModelCriteria + " ," + "CriteriaType :" + item1.OACompetencyEvaluationModelCriteriaType + " ," + "DataSteps :" + item1.OACompetencyEvaluationModelDataSteps)
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                }

                                if (item.OAppraisalBusinessAppraisalModelObject.Count() > 0)
                                {
                                    foreach (var item1 in item.OAppraisalBusinessAppraisalModelObject)
                                    {

                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld1 = item.OCompetencyModelCode == null ? "" : item.OCompetencyModelCode,
                                            Fld2 = item.OCompetenncyModelName == null ? "" : item.OCompetenncyModelName,
                                            Fld3 = item.OCompetenncyMdelCreatedDate == null ? "" : item.OCompetenncyMdelCreatedDate.Value.ToShortDateString(),
                                            Fld4 = "AppraisalBusinessApprisalModel",
                                            Fld5 = (item.OBusinessApprisalModelCode + item.OBusinessApprisalModelName) == null ? "" : (item.OBusinessApprisalModelCode + " " + item.OBusinessApprisalModelName),
                                            Fld6 = "AppraisalBusinessAppraisalModelObject",
                                            Fld7 = item1.OLookupAppraisalBusinessAppraisalModel == null ? "" : item1.OLookupAppraisalBusinessAppraisalModel,
                                            Fld8 = (item1.OBCompetencyEvaluationModelCriteria + item1.OBCompetencyEvaluationModelCriteriaType + item1.OBCompetencyEvaluationModelDataSteps) == null ? "" : ("Evaluation" + " " + "Criteria :" + item1.OBCompetencyEvaluationModelCriteria + " ," + "CriteriaType :" + item1.OBCompetencyEvaluationModelCriteriaType + ", " + "DataSteps :" + item1.OBCompetencyEvaluationModelDataSteps)
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                }

                                if (item.OAppraisalKRAModelObject.Count() > 0)
                                {
                                    foreach (var item1 in item.OAppraisalKRAModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld1 = item.OCompetencyModelCode == null ? "" : item.OCompetencyModelCode,
                                            Fld2 = item.OCompetenncyModelName == null ? "" : item.OCompetenncyModelName,
                                            Fld3 = item.OCompetenncyMdelCreatedDate == null ? "" : item.OCompetenncyMdelCreatedDate.Value.ToShortDateString(),
                                            Fld4 = "AppraisalKRAModel",
                                            Fld5 = (item.OKRAModelCode + item.OKRAModelName) == null ? "" : (item.OKRAModelCode + item.OKRAModelName),
                                            Fld6 = "AppraisalKRAModelObject",
                                            Fld7 = item1.OLookupAppraisalKRAModel == null ? "" : item1.OLookupAppraisalKRAModel,
                                            Fld8 = (item1.OKCompetencyEvaluationModelCriteria + item1.OKCompetencyEvaluationModelCriteriaType + item1.OKCompetencyEvaluationModelDataSteps) == null ? "" : ("Evaluation" + " " + "Criteria :" + item1.OKCompetencyEvaluationModelCriteria + "," + "CriteriaType :" + item1.OKCompetencyEvaluationModelCriteriaType + ", " + "DataSteps :" + item1.OKCompetencyEvaluationModelDataSteps)
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                }

                                if (item.OAppraisalPotentialModelObject.Count() > 0)
                                {
                                    foreach (var item1 in item.OAppraisalPotentialModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld1 = item.OCompetencyModelCode == null ? "" : item.OCompetencyModelCode,
                                            Fld2 = item.OCompetenncyModelName == null ? "" : item.OCompetenncyModelName,
                                            Fld3 = item.OCompetenncyMdelCreatedDate == null ? "" : item.OCompetenncyMdelCreatedDate.Value.ToShortDateString(),
                                            Fld4 = "AppraisalPotentialModel",
                                            Fld5 = (item.OPotentialModelCode + item.OPotentialModelName) == null ? "" : (item.OPotentialModelCode + " " + item.OPotentialModelName),
                                            Fld6 = "AppraisalPotentialModelObject",
                                            Fld7 = item1.OLookupAppraisalPotentialModel == null ? "" : item1.OLookupAppraisalPotentialModel,
                                            Fld8 = (item1.OPoCompetencyEvaluationModelCriteria + item1.OPoCompetencyEvaluationModelCriteriaType + item1.OPoCompetencyEvaluationModelDataSteps) == null ? "" : ("Evaluation" + " " + "Criteria :" + item1.OPoCompetencyEvaluationModelCriteria + "," + "CriteriaType :" + item1.OPoCompetencyEvaluationModelCriteriaType + "," + "DataSteps :" + item1.OPoCompetencyEvaluationModelDataSteps)
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                }

                                if (item.OPastExperienceModelObject.Count() > 0)
                                {
                                    foreach (var item1 in item.OPastExperienceModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld1 = item.OCompetencyModelCode == null ? "" : item.OCompetencyModelCode,
                                            Fld2 = item.OCompetenncyModelName == null ? "" : item.OCompetenncyModelName,
                                            Fld3 = item.OCompetenncyMdelCreatedDate == null ? "" : item.OCompetenncyMdelCreatedDate.Value.ToShortDateString(),
                                            Fld4 = "PastExperienceModel",
                                            Fld5 = (item.OPastExperienceModelCode + item.OPastExperienceModelName) == null ? "" : (item.OPastExperienceModelCode + " " + item.OPastExperienceModelName),
                                            Fld6 = "PastExperienceModelObject",
                                            Fld7 = item1.OLookupPastExperienceModel == null ? "" : item1.OLookupPastExperienceModel,
                                            Fld8 = (item1.OPaCompetencyEvaluationModelCriteria + item1.OPaCompetencyEvaluationModelCriteriaType + item1.OPaCompetencyEvaluationModelDataSteps) == null ? "" : ("Evaluation" + " " + "Criteria :" + item1.OPaCompetencyEvaluationModelCriteria + "," + "CriteriaType :" + item1.OPaCompetencyEvaluationModelCriteriaType + "," + "DataSteps :" + item1.OPaCompetencyEvaluationModelDataSteps)
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                }

                                if (item.OPersonnelModelObject.Count() > 0)
                                {
                                    foreach (var item1 in item.OPersonnelModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld1 = item.OCompetencyModelCode == null ? "" : item.OCompetencyModelCode,
                                            Fld2 = item.OCompetenncyModelName == null ? "" : item.OCompetenncyModelName,
                                            Fld3 = item.OCompetenncyMdelCreatedDate == null ? "" : item.OCompetenncyMdelCreatedDate.Value.ToShortDateString(),
                                            Fld4 = "PersonnelModel",
                                            Fld5 = (item.OPersonnelModelCode + item.OPersonnelModelName) == null ? "" : (item.OPersonnelModelCode + " " + item.OPersonnelModelName),
                                            Fld6 = "PersonnelModelObject",
                                            Fld7 = item1.OLookupPersonnelModell == null ? "" : item1.OLookupPersonnelModell,
                                            Fld8 = (item1.OPerCompetencyEvaluationModelCriteria + item1.OPerCompetencyEvaluationModelCriteriaType + item1.OPerCompetencyEvaluationModelDataSteps) == null ? "" : ("Evaluation" + " " + "Criteria :" + item1.OPerCompetencyEvaluationModelCriteria + "," + "CriteriaType :" + item1.OPerCompetencyEvaluationModelCriteriaType + "," + "DataSteps :" + item1.OPerCompetencyEvaluationModelDataSteps)
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                }

                                if (item.OQualificationModelObject.Count() > 0)
                                {
                                    foreach (var item1 in item.OQualificationModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld1 = item.OCompetencyModelCode == null ? "" : item.OCompetencyModelCode,
                                            Fld2 = item.OCompetenncyModelName == null ? "" : item.OCompetenncyModelName,
                                            Fld3 = item.OCompetenncyMdelCreatedDate == null ? "" : item.OCompetenncyMdelCreatedDate.Value.ToShortDateString(),
                                            Fld4 = "QualificationModel",
                                            Fld5 = (item.OQualificationModelCode + item.OQualificationModelName) == null ? "" : (item.OQualificationModelCode + " " + item.OQualificationModelName),
                                            Fld6 = "QualificationModelObject",
                                            Fld7 = item1.OLookupQualificationModel == null ? "" : item1.OLookupQualificationModel,
                                            Fld8 = (item1.OQCompetencyEvaluationModelCriteria + item1.OQCompetencyEvaluationModelCriteriaType + item1.OQCompetencyEvaluationModelDataSteps) == null ? "" : ("Evaluation" + " " + "Criteria :" + item1.OQCompetencyEvaluationModelCriteria + "," + "CriteriaType :" + item1.OQCompetencyEvaluationModelCriteriaType + "," + "DataSteps :" + item1.OQCompetencyEvaluationModelDataSteps)
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                }

                                if (item.OServiceModelObject.Count() > 0)
                                {
                                    foreach (var item1 in item.OServiceModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld1 = item.OCompetencyModelCode == null ? "" : item.OCompetencyModelCode,
                                            Fld2 = item.OCompetenncyModelName == null ? "" : item.OCompetenncyModelName,
                                            Fld3 = item.OCompetenncyMdelCreatedDate == null ? "" : item.OCompetenncyMdelCreatedDate.Value.ToShortDateString(),
                                            Fld4 = "ServiceModel",
                                            Fld5 = (item.OServiceModelCode + item.OServiceModelName) == null ? "" : (item.OServiceModelCode + " " + item.OServiceModelName),
                                            Fld6 = "ServiceModelObject",
                                            Fld7 = item1.OLookupServiceModel == null ? "" : item1.OLookupServiceModel,
                                            Fld8 = (item1.OSerCompetencyEvaluationModelCriteria + item1.OSerCompetencyEvaluationModelCriteriaType + item1.OSerCompetencyEvaluationModelDataSteps) == null ? "" : ("Evaluation" + " " + "Criteria :" + item1.OSerCompetencyEvaluationModelCriteria + "," + "CriteriaType :" + item1.OSerCompetencyEvaluationModelCriteriaType + "," + "DataSteps :" + item1.OSerCompetencyEvaluationModelDataSteps)
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                }

                                if (item.OSkillModelObject.Count() > 0)
                                {
                                    foreach (var item1 in item.OSkillModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld1 = item.OCompetencyModelCode == null ? "" : item.OCompetencyModelCode,
                                            Fld2 = item.OCompetenncyModelName == null ? "" : item.OCompetenncyModelName,
                                            Fld3 = item.OCompetenncyMdelCreatedDate == null ? "" : item.OCompetenncyMdelCreatedDate.Value.ToShortDateString(),
                                            Fld4 = "SkillModel",
                                            Fld5 = (item.OSkillModelCode + item.OSkillModelName) == null ? "" : (item.OSkillModelCode + " " + item.OSkillModelName),
                                            Fld6 = "SkillModelObject",
                                            Fld7 = item1.OLookupSkillModel == null ? "" : item1.OLookupSkillModel,
                                            Fld8 = (item1.OSkiCompetencyEvaluationModelCriteria + item1.OSkiCompetencyEvaluationModelCriteriaType + item1.OSkiCompetencyEvaluationModelDataSteps) == null ? "" : ("Evaluation" + " " + "Criteria :" + item1.OSkiCompetencyEvaluationModelCriteria + "," + "CriteriaType :" + item1.OSkiCompetencyEvaluationModelCriteriaType + "," + "DataSteps :" + item1.OSkiCompetencyEvaluationModelDataSteps)
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                }

                                if (item.OTrainingModelObject.Count() > 0)
                                {
                                    foreach (var item1 in item.OTrainingModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld1 = item.OCompetencyModelCode == null ? "" : item.OCompetencyModelCode,
                                            Fld2 = item.OCompetenncyModelName == null ? "" : item.OCompetenncyModelName,
                                            Fld3 = item.OCompetenncyMdelCreatedDate == null ? "" : item.OCompetenncyMdelCreatedDate.Value.ToShortDateString(),
                                            Fld4 = "TrainingModel",
                                            Fld5 = (item.OTrainingModelCode + item.OTrainingModelName) == null ? "" : (item.OTrainingModelCode + " " + item.OTrainingModelName),
                                            Fld6 = "TrainingModelObject",
                                            Fld7 = item1.OLookupTrainingModel == null ? "" : item1.OLookupTrainingModel,
                                            Fld8 = (item1.OTCompetencyEvaluationModelCriteria + item1.OTCompetencyEvaluationModelCriteriaType + item1.OTCompetencyEvaluationModelDataSteps) == null ? "" : ("Evaluation" + " " + "Criteria :" + item1.OTCompetencyEvaluationModelCriteria + "," + "CriteriaType :" + item1.OTCompetencyEvaluationModelCriteriaType + "," + "DataSteps :" + item1.OTCompetencyEvaluationModelDataSteps)
                                        };

                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                }
                            }
                        }

                        return OGenericPayrollStatement;

                        break;


                    case "COMPETENCYMODELASSIGNMENT":

                        List<GenericField100> OGenericCompetencyModelAssignment = new List<GenericField100>();

                        var IB = db.CompetencyModelAssignment.Select(d => new
                        {
                            OCompetencyModelAssignmentId = d.Id,
                            OBatchName = d.BatchName,
                            OBatchDescription = d.BatchDescription,
                            OCompetencyModelCode = d.CompetencyModel.Code,
                            OCompetencyModelName = d.CompetencyModel.ModelName,
                            OCompetencyModelAssignment_OrgStructure = d.CompetencyModelAssignment_OrgStructure.Select(r => new
                            {
                                OCompetencyModelAssignment_OrgStructureId = r.Id,
                                OGeoStruct = r.GeoStruct.FullDetailsLD,
                                OPayStruct = r.PayStruct.FullDetails,
                                OFuncStruct = r.FuncStruct.FullDetails

                            }).ToList()

                        }).ToList();

                        var IIB = IB.OrderBy(d => d.OCompetencyModelAssignmentId).ToList();

                        if (salheadlist.Count() > 0)
                        {
                            IIB = IIB.Where(e => salheadlist.Contains(e.OBatchName)).ToList();
                        }
                        else
                        {
                            IIB = IIB.ToList();
                        }

                        if (IIB == null && IIB.Count() == 0)
                        {
                            return null;
                        }

                        else
                        {
                            foreach (var item in IIB)
                            {
                                if (item.OCompetencyModelAssignment_OrgStructure.Count() > 0)
                                {
                                    foreach (var item1 in item.OCompetencyModelAssignment_OrgStructure)
                                    {
                                        GenericField100 GenCompetencyModelAssignment100 = new GenericField100()
                                        {
                                            Fld1 = item.OBatchName == null ? "" : item.OBatchName,
                                            Fld2 = item.OBatchDescription == null ? "" : item.OBatchDescription,
                                            Fld3 = (item.OCompetencyModelCode + item.OCompetencyModelName) == null ? "" : (item.OCompetencyModelCode + " " + item.OCompetencyModelName),
                                            Fld4 = item1.OGeoStruct == null ? "" : "GeoStruct :" + item1.OGeoStruct,
                                            Fld5 = item1.OPayStruct == null ? "" : "PayStruct :" + item1.OPayStruct,
                                            Fld6 = item1.OFuncStruct == null ? "" : "FuncStruct :" + item1.OFuncStruct

                                        };

                                        OGenericCompetencyModelAssignment.Add(GenCompetencyModelAssignment100);

                                    }
                                }
                            }
                        }

                        return OGenericCompetencyModelAssignment;

                        break;

                    case "COMPETENCYEMPLOYEEDATAT":

                        List<GenericField100> OGenericCompetencyEmployeeDataT = new List<GenericField100>();

                        var OCompetencyBatchProcessT = db.CompetencyBatchProcessT.Select(z => new
                        {
                            z_ProcessBatch = z.ProcessBatch,
                            z_CompetencyEmployeeDataT = z.CompetencyEmployeeDataT.Select(y => new
                            {
                                y_Employee_Id = y.Employee_Id,
                                y_EmpCode = y.Employee.EmpCode,
                                y_EmpName = y.Employee.EmpName.FullNameFML,
                                y_LocDesc = y.Employee.GeoStruct.Location.LocationObj.LocDesc,
                                y_DeptDesc = y.Employee.GeoStruct.Department.DepartmentObj.DeptDesc,
                                y_JobName = y.Employee.FuncStruct.Job.Name,
                                y_BatchName = y.BatchName.BatchName,
                                y_AppraisalAttributeModelObjectT = y.AppraisalAttributeModelObjectT.Select(w => new
                                {
                                    w_AppraisalAttributeModelObject_Id = w.AppraisalAttributeModelObject_Id
                                }).ToList(),
                                y_AppraisalBusinessAppraisalModelObjectT = y.AppraisalBusinessAppraisalModelObjectT.Select(w => new
                                {
                                    w_AppraisalBusinessAppraisalModelObject_Id = w.AppraisalBusinessAppraisalModelObject_Id
                                }).ToList(),
                                y_AppraisalKRAModelObjectT = y.AppraisalKRAModelObjectT.Select(w => new
                                {
                                    w_AppraisalKRAModelObject_Id = w.AppraisalKRAModelObject_Id
                                }).ToList(),
                                y_AppraisalPotentialModelObjectT = y.AppraisalPotentialModelObjectT.Select(w => new
                                {
                                    w_AppraisalPotentialModelObject_Id = w.AppraisalPotentialModelObject_Id
                                }).ToList(),
                                y_PastExperienceModelObjectT = y.PastExperienceModelObjectT.Select(w => new
                                {
                                    w_PastExperienceModelObject_Id = w.PastExperienceModelObject_Id
                                }).ToList(),
                                y_PersonnelModelObjectT = y.PersonnelModelObjectT.Select(w => new
                                {
                                    w_PersonnelModelObject_Id = w.PersonnelModelObject_Id
                                }).ToList(),
                                y_QualificationModelObjectT = y.QualificationModelObjectT.Select(w => new
                                {
                                    w_QualificationModelObject_Id = w.QualificationModelObject_Id
                                }).ToList(),
                                y_ServiceModelObjectT = y.ServiceModelObjectT.Select(w => new
                                {
                                    w_ServiceModelObject_Id = w.ServiceModelObject_Id
                                }).ToList(),
                                y_SkillModelObjectT = y.SkillModelObjectT.Select(w => new
                                {
                                    w_SkillModelObject_Id = w.SkillModelObject_Id
                                }).ToList(),
                                y_TrainingModelObjectT = y.TrainingModelObjectT.Select(w => new
                                {
                                    w_TrainingModelObject_Id = w.TrainingModelObject_Id
                                }).ToList()
                            }).ToList()
                        }).ToList();


                        if (salheadlist.Count() > 0)
                        {
                            OCompetencyBatchProcessT = OCompetencyBatchProcessT.Where(e => salheadlist.Contains(e.z_ProcessBatch)).ToList();
                        }

                        else
                        {
                            OCompetencyBatchProcessT = OCompetencyBatchProcessT.ToList();
                        }


                        if (OCompetencyBatchProcessT.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var item in OCompetencyBatchProcessT)
                            {
                                foreach (var item1 in item.z_CompetencyEmployeeDataT)
                                {
                                    foreach (var item2 in item1.y_AppraisalAttributeModelObjectT)
                                    {
                                        var IH = db.CompetencyEmployeeDataGeneration.Include(e => e.Employee).Where(e => e.Employee.Id == item1.y_Employee_Id).Select(d => new
                                       {
                                           IHBatchName = d.BatchName.BatchName,
                                           OAppraisalAttributeModelObjectV = d.AppraisalAttributeModelObjectV.Select(r => new
                                           {
                                               IHAppraisalAttributeModelObjectId = r.AppraisalAttributeModelObject_Id,
                                               OAppraisalAttributeModelLookupval = r.AppraisalAttributeModelObject.AppraisalAttributeModel.LookupVal,

                                               OAbjectValue = r.ObjectValue.Select(t => new
                                               {
                                                   OAObjectVal = t.ObjectVal,

                                               }).ToList(),

                                           }).Where(r => r.IHAppraisalAttributeModelObjectId == item2.w_AppraisalAttributeModelObject_Id).ToList()

                                       }).Where(e => item1.y_BatchName.Contains(e.IHBatchName)).ToList();

                                        foreach (var item3 in IH)
                                        {
                                            foreach (var item4 in item3.OAppraisalAttributeModelObjectV)
                                            {
                                                foreach (var item5 in item4.OAbjectValue)
                                                {
                                                    GenericField100 GenCompetencyEmployeeDataT100 = new GenericField100()
                                                   {
                                                       Fld1 = item1.y_BatchName == null ? "" : item1.y_BatchName,
                                                       Fld4 = item1.y_EmpCode == null ? "" : item1.y_EmpCode,
                                                       Fld5 = item1.y_EmpName == null ? "" : item1.y_EmpName,
                                                       Fld6 = item1.y_LocDesc == null ? "" : item1.y_LocDesc,
                                                       Fld7 = item1.y_DeptDesc == null ? "" : item1.y_DeptDesc,
                                                       Fld8 = item1.y_JobName == null ? "" : item1.y_JobName,
                                                       Fld2 = "AppraisalAttributeModelObjectV",
                                                       Fld9 = item4.OAppraisalAttributeModelLookupval == null ? "" : item4.OAppraisalAttributeModelLookupval,
                                                       Fld3 = item5.OAObjectVal == null ? "" : item5.OAObjectVal

                                                   };

                                                    OGenericCompetencyEmployeeDataT.Add(GenCompetencyEmployeeDataT100);
                                                }
                                            }
                                        }
                                    }

                                    foreach (var item2 in item1.y_AppraisalBusinessAppraisalModelObjectT)
                                    {
                                        var IH = db.CompetencyEmployeeDataGeneration.Include(e => e.Employee).Where(e => e.Employee.Id == item1.y_Employee_Id).Select(d => new
                                        {
                                            IHBatchName = d.BatchName.BatchName,
                                            OAppraisalBusinessAppraisalModelObjectV = d.AppraisalBusinessAppraisalModelObjectV.Select(r => new
                                            {
                                                IHAppraisalBusinessAppraisalModelObjectId = r.AppraisalBusinessAppraisalModelObject_Id,
                                                OAppraisalBusinessAppraisalModelLookupval = r.AppraisalBusinessAppraisalModelObject.AppraisalBusinessAppraisalModel.LookupVal,

                                                OBbjectValue = r.ObjectValue.Select(t => new
                                                {
                                                    OBObjectVal = t.ObjectVal
                                                }).ToList(),
                                            }).Where(r => r.IHAppraisalBusinessAppraisalModelObjectId == item2.w_AppraisalBusinessAppraisalModelObject_Id).ToList(),

                                        }).Where(e => item1.y_BatchName.Contains(e.IHBatchName)).ToList();

                                        foreach (var item3 in IH)
                                        {
                                            foreach (var item4 in item3.OAppraisalBusinessAppraisalModelObjectV)
                                            {
                                                foreach (var item5 in item4.OBbjectValue)
                                                {
                                                    GenericField100 GenCompetencyEmployeeDataT100 = new GenericField100()
                                                    {
                                                        Fld1 = item1.y_BatchName == null ? "" : item1.y_BatchName,
                                                        Fld4 = item1.y_EmpCode == null ? "" : item1.y_EmpCode,
                                                        Fld5 = item1.y_EmpName == null ? "" : item1.y_EmpName,
                                                        Fld6 = item1.y_LocDesc == null ? "" : item1.y_LocDesc,
                                                        Fld7 = item1.y_DeptDesc == null ? "" : item1.y_DeptDesc,
                                                        Fld8 = item1.y_JobName == null ? "" : item1.y_JobName,
                                                        Fld2 = "AppraisalBusinessAppraisalModelObjectV",
                                                        Fld9 = item4.OAppraisalBusinessAppraisalModelLookupval == null ? "" : item4.OAppraisalBusinessAppraisalModelLookupval,
                                                        Fld3 = item5.OBObjectVal == null ? "" : item5.OBObjectVal

                                                    };

                                                    OGenericCompetencyEmployeeDataT.Add(GenCompetencyEmployeeDataT100);
                                                }
                                            }
                                        }
                                    }

                                    foreach (var item2 in item1.y_AppraisalKRAModelObjectT)
                                    {
                                        var IH = db.CompetencyEmployeeDataGeneration.Include(e => e.Employee).Where(e => e.Employee.Id == item1.y_Employee_Id).Select(d => new
                                        {
                                            IHBatchName = d.BatchName.BatchName,
                                            OAppraisalKRAModelObjectV = d.AppraisalKRAModelObjectV.Select(r => new
                                            {
                                                IHAppraisalKRAModelObjectId = r.AppraisalKRAModelObject_Id,
                                                OAppraisalKRAModelLookupval = r.AppraisalKRAModelObject.AppraisalKRAModel.LookupVal,

                                                OKbjectValue = r.ObjectValue.Select(t => new
                                                {
                                                    OKObjectVal = t.ObjectVal
                                                }).ToList(),
                                            }).Where(r => r.IHAppraisalKRAModelObjectId == item2.w_AppraisalKRAModelObject_Id).ToList(),

                                        }).Where(e => item1.y_BatchName.Contains(e.IHBatchName)).ToList();

                                        foreach (var item3 in IH)
                                        {
                                            foreach (var item4 in item3.OAppraisalKRAModelObjectV)
                                            {
                                                foreach (var item5 in item4.OKbjectValue)
                                                {
                                                    GenericField100 GenCompetencyEmployeeDataT100 = new GenericField100()
                                                    {
                                                        Fld1 = item1.y_BatchName == null ? "" : item1.y_BatchName,
                                                        Fld4 = item1.y_EmpCode == null ? "" : item1.y_EmpCode,
                                                        Fld5 = item1.y_EmpName == null ? "" : item1.y_EmpName,
                                                        Fld6 = item1.y_LocDesc == null ? "" : item1.y_LocDesc,
                                                        Fld7 = item1.y_DeptDesc == null ? "" : item1.y_DeptDesc,
                                                        Fld8 = item1.y_JobName == null ? "" : item1.y_JobName,
                                                        Fld2 = "AppraisalKRAModelObjectV",
                                                        Fld9 = item4.OAppraisalKRAModelLookupval == null ? "" : item4.OAppraisalKRAModelLookupval,
                                                        Fld3 = item5.OKObjectVal == null ? "" : item5.OKObjectVal

                                                    };

                                                    OGenericCompetencyEmployeeDataT.Add(GenCompetencyEmployeeDataT100);
                                                }
                                            }
                                        }
                                    }

                                    foreach (var item2 in item1.y_AppraisalPotentialModelObjectT)
                                    {
                                        var IH = db.CompetencyEmployeeDataGeneration.Include(e => e.Employee).Where(e => e.Employee.Id == item1.y_Employee_Id).Select(d => new
                                        {
                                            IHBatchName = d.BatchName.BatchName,
                                            OAppraisalPotentialModelObjectV = d.AppraisalPotentialModelObjectV.Select(r => new
                                            {
                                                IHAppraisalPotentialModelObject = r.AppraisalPotentialModelObject_Id,
                                                OAppraisalPotentialModelLookupval = r.AppraisalPotentialModelObject.AppraisalPotentialModel.LookupVal,

                                                OPobjectValue = r.ObjectValue.Select(t => new
                                                {
                                                    OPoObjectVal = t.ObjectVal
                                                }).ToList(),
                                            }).Where(r => r.IHAppraisalPotentialModelObject == item2.w_AppraisalPotentialModelObject_Id).ToList(),

                                        }).Where(e => item1.y_BatchName.Contains(e.IHBatchName)).ToList();

                                        foreach (var item3 in IH)
                                        {
                                            foreach (var item4 in item3.OAppraisalPotentialModelObjectV)
                                            {
                                                foreach (var item5 in item4.OPobjectValue)
                                                {
                                                    GenericField100 GenCompetencyEmployeeDataT100 = new GenericField100()
                                                    {
                                                        Fld1 = item1.y_BatchName == null ? "" : item1.y_BatchName,
                                                        Fld4 = item1.y_EmpCode == null ? "" : item1.y_EmpCode,
                                                        Fld5 = item1.y_EmpName == null ? "" : item1.y_EmpName,
                                                        Fld6 = item1.y_LocDesc == null ? "" : item1.y_LocDesc,
                                                        Fld7 = item1.y_DeptDesc == null ? "" : item1.y_DeptDesc,
                                                        Fld8 = item1.y_JobName == null ? "" : item1.y_JobName,
                                                        Fld2 = "AppraisalPotentialModelObjectV",
                                                        Fld9 = item4.OAppraisalPotentialModelLookupval == null ? "" : item4.OAppraisalPotentialModelLookupval,
                                                        Fld3 = item5.OPoObjectVal == null ? "" : item5.OPoObjectVal
                                                    };

                                                    OGenericCompetencyEmployeeDataT.Add(GenCompetencyEmployeeDataT100);
                                                }
                                            }
                                        }
                                    }

                                    foreach (var item2 in item1.y_PastExperienceModelObjectT)
                                    {
                                        var IH = db.CompetencyEmployeeDataGeneration.Include(e => e.Employee).Where(e => e.Employee.Id == item1.y_Employee_Id).Select(d => new
                                        {
                                            IHBatchName = d.BatchName.BatchName,
                                            OPastExperienceModelObjectV = d.PastExperienceModelObjectV.Select(r => new
                                            {
                                                IHPastExperienceModelObjectId = r.PastExperienceModelObject_Id,
                                                OPastExperienceModelLookupval = r.PastExperienceModelObject.PastExperienceModel.LookupVal,

                                                OPabjectValue = r.ObjectValue.Select(t => new
                                                {
                                                    OPoObjectVal = t.ObjectVal
                                                }).ToList(),
                                            }).Where(r => r.IHPastExperienceModelObjectId == item2.w_PastExperienceModelObject_Id).ToList(),

                                        }).Where(e => item1.y_BatchName.Contains(e.IHBatchName)).ToList();

                                        foreach (var item3 in IH)
                                        {
                                            foreach (var item4 in item3.OPastExperienceModelObjectV)
                                            {
                                                foreach (var item5 in item4.OPabjectValue)
                                                {
                                                    GenericField100 GenCompetencyEmployeeDataT100 = new GenericField100()
                                                    {
                                                        Fld1 = item1.y_BatchName == null ? "" : item1.y_BatchName,
                                                        Fld4 = item1.y_EmpCode == null ? "" : item1.y_EmpCode,
                                                        Fld5 = item1.y_EmpName == null ? "" : item1.y_EmpName,
                                                        Fld6 = item1.y_LocDesc == null ? "" : item1.y_LocDesc,
                                                        Fld7 = item1.y_DeptDesc == null ? "" : item1.y_DeptDesc,
                                                        Fld8 = item1.y_JobName == null ? "" : item1.y_JobName,
                                                        Fld2 = "PastExperienceModelObjectV",
                                                        Fld9 = item4.OPastExperienceModelLookupval == null ? "" : item4.OPastExperienceModelLookupval,
                                                        Fld3 = item5.OPoObjectVal == null ? "" : item5.OPoObjectVal

                                                    };

                                                    OGenericCompetencyEmployeeDataT.Add(GenCompetencyEmployeeDataT100);
                                                }
                                            }
                                        }
                                    }

                                    foreach (var item2 in item1.y_PersonnelModelObjectT)
                                    {
                                        var IH = db.CompetencyEmployeeDataGeneration.Include(e => e.Employee).Where(e => e.Employee.Id == item1.y_Employee_Id).Select(d => new
                                        {
                                            IHBatchName = d.BatchName.BatchName,
                                            OPersonnelModelObjectV = d.PersonnelModelObjectV.Select(r => new
                                            {
                                                IHPersonnelModelObjectId = r.PersonnelModelObject_Id,
                                                OPersonnelModelLookupval = r.PersonnelModelObject.PersonnelModel.LookupVal,

                                                OPerbjectValue = r.ObjectValue.Select(t => new
                                                {
                                                    OPerObjectVal = t.ObjectVal
                                                }).ToList(),
                                            }).Where(r => r.IHPersonnelModelObjectId == item2.w_PersonnelModelObject_Id).ToList(),

                                        }).Where(e => item1.y_BatchName.Contains(e.IHBatchName)).ToList();

                                        foreach (var item3 in IH)
                                        {
                                            foreach (var item4 in item3.OPersonnelModelObjectV)
                                            {
                                                foreach (var item5 in item4.OPerbjectValue)
                                                {
                                                    GenericField100 GenCompetencyEmployeeDataT100 = new GenericField100()
                                                    {
                                                        Fld1 = item1.y_BatchName == null ? "" : item1.y_BatchName,
                                                        Fld4 = item1.y_EmpCode == null ? "" : item1.y_EmpCode,
                                                        Fld5 = item1.y_EmpName == null ? "" : item1.y_EmpName,
                                                        Fld6 = item1.y_LocDesc == null ? "" : item1.y_LocDesc,
                                                        Fld7 = item1.y_DeptDesc == null ? "" : item1.y_DeptDesc,
                                                        Fld8 = item1.y_JobName == null ? "" : item1.y_JobName,
                                                        Fld2 = "PersonnelModelObjectV",
                                                        Fld9 = item4.OPersonnelModelLookupval == null ? "" : item4.OPersonnelModelLookupval,
                                                        Fld3 = item5.OPerObjectVal == null ? "" : item5.OPerObjectVal


                                                    };

                                                    OGenericCompetencyEmployeeDataT.Add(GenCompetencyEmployeeDataT100);
                                                }
                                            }
                                        }
                                    }

                                    foreach (var item2 in item1.y_QualificationModelObjectT)
                                    {
                                        var IH = db.CompetencyEmployeeDataGeneration.Include(e => e.Employee).Where(e => e.Employee.Id == item1.y_Employee_Id).Select(d => new
                                        {
                                            IHBatchName = d.BatchName.BatchName,
                                            OQualificationModelObjectV = d.QualificationModelObjectV.Select(r => new
                                            {
                                                IHQualificationModelObjectId = r.QualificationModelObject_Id,
                                                OQualificationModelLookupval = r.QualificationModelObject.QualificationModel.LookupVal,

                                                OQuabjectValue = r.ObjectValue.Select(t => new
                                                {
                                                    OQObjectVal = t.ObjectVal
                                                }).ToList(),
                                            }).Where(r => r.IHQualificationModelObjectId == item2.w_QualificationModelObject_Id).ToList(),

                                        }).Where(e => item1.y_BatchName.Contains(e.IHBatchName)).ToList();

                                        foreach (var item3 in IH)
                                        {
                                            foreach (var item4 in item3.OQualificationModelObjectV)
                                            {
                                                foreach (var item5 in item4.OQuabjectValue)
                                                {
                                                    GenericField100 GenCompetencyEmployeeDataT100 = new GenericField100()
                                                    {
                                                        Fld1 = item1.y_BatchName == null ? "" : item1.y_BatchName,
                                                        Fld4 = item1.y_EmpCode == null ? "" : item1.y_EmpCode,
                                                        Fld5 = item1.y_EmpName == null ? "" : item1.y_EmpName,
                                                        Fld6 = item1.y_LocDesc == null ? "" : item1.y_LocDesc,
                                                        Fld7 = item1.y_DeptDesc == null ? "" : item1.y_DeptDesc,
                                                        Fld8 = item1.y_JobName == null ? "" : item1.y_JobName,
                                                        Fld2 = "QualificationModelObjectV",
                                                        Fld9 = item4.OQualificationModelLookupval == null ? "" : item4.OQualificationModelLookupval,
                                                        Fld3 = item5.OQObjectVal == null ? "" : item5.OQObjectVal

                                                    };
                                                    OGenericCompetencyEmployeeDataT.Add(GenCompetencyEmployeeDataT100);
                                                }
                                            }
                                        }
                                    }

                                    foreach (var item2 in item1.y_ServiceModelObjectT)
                                    {
                                        var IH = db.CompetencyEmployeeDataGeneration.Include(e => e.Employee).Where(e => e.Employee.Id == item1.y_Employee_Id).Select(d => new
                                        {
                                            IHBatchName = d.BatchName.BatchName,
                                            OServiceModelObjectV = d.ServiceModelObjectV.Select(r => new
                                            {
                                                IHServiceModelObjectId = r.ServiceModelObject_Id,
                                                OServiceModelLookupval = r.ServiceModelObject.ServiceModel.LookupVal,

                                                OSerbjectValue = r.ObjectValue.Select(t => new
                                                {
                                                    OSerObjectVal = t.ObjectVal
                                                }).ToList(),
                                            }).Where(r => r.IHServiceModelObjectId == item2.w_ServiceModelObject_Id).ToList(),

                                        }).Where(e => item1.y_BatchName.Contains(e.IHBatchName)).ToList();

                                        foreach (var item3 in IH)
                                        {
                                            foreach (var item4 in item3.OServiceModelObjectV)
                                            {
                                                foreach (var item5 in item4.OSerbjectValue)
                                                {
                                                    GenericField100 GenCompetencyEmployeeDataT100 = new GenericField100()
                                                    {
                                                        Fld1 = item1.y_BatchName == null ? "" : item1.y_BatchName,
                                                        Fld4 = item1.y_EmpCode == null ? "" : item1.y_EmpCode,
                                                        Fld5 = item1.y_EmpName == null ? "" : item1.y_EmpName,
                                                        Fld6 = item1.y_LocDesc == null ? "" : item1.y_LocDesc,
                                                        Fld7 = item1.y_DeptDesc == null ? "" : item1.y_DeptDesc,
                                                        Fld8 = item1.y_JobName == null ? "" : item1.y_JobName,
                                                        Fld2 = "ServiceModelObjectV",
                                                        Fld9 = item4.OServiceModelLookupval == null ? "" : item4.OServiceModelLookupval,
                                                        Fld3 = item5.OSerObjectVal == null ? "" : item5.OSerObjectVal

                                                    };
                                                    OGenericCompetencyEmployeeDataT.Add(GenCompetencyEmployeeDataT100);
                                                }
                                            }
                                        }
                                    }

                                    foreach (var item2 in item1.y_SkillModelObjectT)
                                    {
                                        var IH = db.CompetencyEmployeeDataGeneration.Include(e => e.Employee).Where(e => e.Employee.Id == item1.y_Employee_Id).Select(d => new
                                        {
                                            IHBatchName = d.BatchName.BatchName,
                                            OSkillModelObjectV = d.SkillModelObjectV.Select(r => new
                                            {
                                                IHSkillModelObjectId = r.SkillModelObject_Id,
                                                OSkillModelLookupval = r.SkillModelObject.SkillModel.LookupVal,

                                                OSkibjectValue = r.ObjectValue.Select(t => new
                                                {
                                                    OSkiObjectVal = t.ObjectVal
                                                }).ToList(),
                                            }).Where(r => r.IHSkillModelObjectId == item2.w_SkillModelObject_Id).ToList(),

                                        }).Where(e => item1.y_BatchName.Contains(e.IHBatchName)).ToList();

                                        foreach (var item3 in IH)
                                        {
                                            foreach (var item4 in item3.OSkillModelObjectV)
                                            {
                                                foreach (var item5 in item4.OSkibjectValue)
                                                {
                                                    GenericField100 GenCompetencyEmployeeDataT100 = new GenericField100()
                                                    {
                                                        Fld1 = item1.y_BatchName == null ? "" : item1.y_BatchName,
                                                        Fld4 = item1.y_EmpCode == null ? "" : item1.y_EmpCode,
                                                        Fld5 = item1.y_EmpName == null ? "" : item1.y_EmpName,
                                                        Fld6 = item1.y_LocDesc == null ? "" : item1.y_LocDesc,
                                                        Fld7 = item1.y_DeptDesc == null ? "" : item1.y_DeptDesc,
                                                        Fld8 = item1.y_JobName == null ? "" : item1.y_JobName,
                                                        Fld2 = "SkillModelObjectV",
                                                        Fld9 = item4.OSkillModelLookupval == null ? "" : item4.OSkillModelLookupval,
                                                        Fld3 = item5.OSkiObjectVal == null ? "" : item5.OSkiObjectVal

                                                    };
                                                    OGenericCompetencyEmployeeDataT.Add(GenCompetencyEmployeeDataT100);
                                                }
                                            }
                                        }
                                    }

                                    foreach (var item2 in item1.y_TrainingModelObjectT)
                                    {
                                        var IH = db.CompetencyEmployeeDataGeneration.Include(e => e.Employee).Where(e => e.Employee.Id == item1.y_Employee_Id).Select(d => new
                                        {
                                            IHBatchName = d.BatchName.BatchName,
                                            OTrainingModelObjectV = d.TrainingModelObjectV.Select(r => new
                                            {
                                                IHTrainingModelObjectId = r.TrainingModelObject_Id,
                                                OTrainingModelLookupval = r.TrainingModelObject.TrainingModel.LookupVal,
                                                OTbjectValue = r.ObjectValue.Select(t => new
                                                {
                                                    OTObjectVal = t.ObjectVal
                                                }).ToList(),
                                            }).Where(r => r.IHTrainingModelObjectId == item2.w_TrainingModelObject_Id).ToList()

                                        }).Where(e => item1.y_BatchName.Contains(e.IHBatchName)).ToList();

                                        foreach (var item3 in IH)
                                        {
                                            foreach (var item4 in item3.OTrainingModelObjectV)
                                            {
                                                foreach (var item5 in item4.OTbjectValue)
                                                {
                                                    GenericField100 GenCompetencyEmployeeDataT100 = new GenericField100()
                                                    {
                                                        Fld1 = item1.y_BatchName == null ? "" : item1.y_BatchName,
                                                        Fld4 = item1.y_EmpCode == null ? "" : item1.y_EmpCode,
                                                        Fld5 = item1.y_EmpName == null ? "" : item1.y_EmpName,
                                                        Fld6 = item1.y_LocDesc == null ? "" : item1.y_LocDesc,
                                                        Fld7 = item1.y_DeptDesc == null ? "" : item1.y_DeptDesc,
                                                        Fld8 = item1.y_JobName == null ? "" : item1.y_JobName,
                                                        Fld2 = "TrainingModelObjectV",
                                                        Fld9 = item4.OTrainingModelLookupval == null ? "" : item4.OTrainingModelLookupval,
                                                        Fld3 = item5.OTObjectVal == null ? "" : item5.OTObjectVal

                                                    };

                                                    OGenericCompetencyEmployeeDataT.Add(GenCompetencyEmployeeDataT100);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        return OGenericCompetencyEmployeeDataT;
                        break;

                    case "SUCCESSIONMODEL":

                        var IS = db.SuccessionModel.Select(e => new
                        {
                            OSuccessionModelCode = e.Code,
                            OSuccessionModelName = e.ModelName,
                            OSuccessionMdelCreatedDate = e.CreatedDate,
                            OAppraisalAttributeModel = e.AppraisalAttributeModel,
                            OAttributeModelCode = e.AppraisalAttributeModel.Code,
                            OAttributeModelName = e.AppraisalAttributeModel.ModelName,
                            OAppraisalAttributeModelObject = e.AppraisalAttributeModel.AppraisalAttributeModelObject.Select(d => new
                            {
                                OALookupAppraisalAttributeModel = d.AppraisalAttributeModel.LookupVal,
                                OACompetencyEvaluationModelCriteria = d.CompetencyEvaluationModel.Criteria.LookupVal,
                                OACompetencyEvaluationModelCriteriaType = d.CompetencyEvaluationModel.CriteriaType.LookupVal,
                                OACompetencyEvaluationModelDataSteps = d.CompetencyEvaluationModel.DataSteps.LookupVal

                            }).ToList(),
                            OAppraisalBusinessApprisalModel = e.AppraisalBusinessApprisalModel,
                            OBusinessApprisalModelCode = e.AppraisalBusinessApprisalModel.Code,
                            OBusinessApprisalModelName = e.AppraisalBusinessApprisalModel.ModelName,
                            OAppraisalBusinessAppraisalModelObject = e.AppraisalBusinessApprisalModel.AppraisalBusinessAppraisalModelObject.Select(d => new
                            {
                                OLookupAppraisalBusinessAppraisalModel = d.AppraisalBusinessAppraisalModel.LookupVal,
                                OBCompetencyEvaluationModelCriteria = d.CompetencyEvaluationModel.Criteria.LookupVal,
                                OBCompetencyEvaluationModelCriteriaType = d.CompetencyEvaluationModel.CriteriaType.LookupVal,
                                OBCompetencyEvaluationModelDataSteps = d.CompetencyEvaluationModel.DataSteps.LookupVal

                            }).ToList(),
                            OAppraisalKRAModel = e.AppraisalKRAModel,
                            OKRAModelCode = e.AppraisalKRAModel.Code,
                            OKRAModelName = e.AppraisalKRAModel.ModelName,
                            OAppraisalKRAModelObject = e.AppraisalKRAModel.AppraisalKRAModelObject.Select(d => new
                            {
                                OLookupAppraisalKRAModel = d.AppraisalKRAModel.LookupVal,
                                OKCompetencyEvaluationModelCriteria = d.CompetencyEvaluationModel.Criteria.LookupVal,
                                OKCompetencyEvaluationModelCriteriaType = d.CompetencyEvaluationModel.CriteriaType.LookupVal,
                                OKCompetencyEvaluationModelDataSteps = d.CompetencyEvaluationModel.DataSteps.LookupVal

                            }).ToList(),
                            OAppraisalPotentialModel = e.AppraisalPotentialModel,
                            OPotentialModelCode = e.AppraisalPotentialModel.Code,
                            OPotentialModelName = e.AppraisalPotentialModel.ModelName,
                            OAppraisalPotentialModelObject = e.AppraisalPotentialModel.AppraisalPotentialModelObject.Select(d => new
                            {
                                OLookupAppraisalPotentialModel = d.AppraisalPotentialModel.LookupVal,
                                OPoCompetencyEvaluationModelCriteria = d.CompetencyEvaluationModel.Criteria.LookupVal,
                                OPoCompetencyEvaluationModelCriteriaType = d.CompetencyEvaluationModel.CriteriaType.LookupVal,
                                OPoCompetencyEvaluationModelDataSteps = d.CompetencyEvaluationModel.DataSteps.LookupVal

                            }).ToList(),
                            OPastExperienceModel = e.PastExperienceModel,
                            OPastExperienceModelCode = e.PastExperienceModel.Code,
                            OPastExperienceModelName = e.PastExperienceModel.ModelName,
                            OPastExperienceModelObject = e.PastExperienceModel.PastExperienceModelObject.Select(d => new
                            {
                                OLookupPastExperienceModel = d.PastExperienceModel.LookupVal,
                                OPaCompetencyEvaluationModelCriteria = d.CompetencyEvaluationModel.Criteria.LookupVal,
                                OPaCompetencyEvaluationModelCriteriaType = d.CompetencyEvaluationModel.CriteriaType.LookupVal,
                                OPaCompetencyEvaluationModelDataSteps = d.CompetencyEvaluationModel.DataSteps.LookupVal

                            }).ToList(),
                            OPersonnelModel = e.PersonnelModel,
                            OPersonnelModelCode = e.PersonnelModel.Code,
                            OPersonnelModelName = e.PersonnelModel.ModelName,
                            OPersonnelModelObject = e.PersonnelModel.PersonnelModelObject.Select(d => new
                            {
                                OLookupPersonnelModell = d.PersonnelModel.LookupVal,
                                OPerCompetencyEvaluationModelCriteria = d.CompetencyEvaluationModel.Criteria.LookupVal,
                                OPerCompetencyEvaluationModelCriteriaType = d.CompetencyEvaluationModel.CriteriaType.LookupVal,
                                OPerCompetencyEvaluationModelDataSteps = d.CompetencyEvaluationModel.DataSteps.LookupVal

                            }).ToList(),
                            OQualificationModel = e.QualificationModel,
                            OQualificationModelCode = e.QualificationModel.Code,
                            OQualificationModelName = e.QualificationModel.ModelName,
                            OQualificationModelObject = e.QualificationModel.QualificationModelObject.Select(d => new
                            {
                                OLookupQualificationModel = d.QualificationModel.LookupVal,
                                OQCompetencyEvaluationModelCriteria = d.CompetencyEvaluationModel.Criteria.LookupVal,
                                OQCompetencyEvaluationModelCriteriaType = d.CompetencyEvaluationModel.CriteriaType.LookupVal,
                                OQCompetencyEvaluationModelDataSteps = d.CompetencyEvaluationModel.DataSteps.LookupVal

                            }).ToList(),
                            OServiceModel = e.ServiceModel,
                            OServiceModelCode = e.ServiceModel.Code,
                            OServiceModelName = e.ServiceModel.ModelName,
                            OServiceModelObject = e.ServiceModel.ServiceModelObject.Select(d => new
                            {
                                OLookupServiceModel = d.ServiceModel.LookupVal,
                                OSerCompetencyEvaluationModelCriteria = d.CompetencyEvaluationModel.Criteria.LookupVal,
                                OSerCompetencyEvaluationModelCriteriaType = d.CompetencyEvaluationModel.CriteriaType.LookupVal,
                                OSerCompetencyEvaluationModelDataSteps = d.CompetencyEvaluationModel.DataSteps.LookupVal

                            }).ToList(),
                            OSkillModel = e.SkillModel,
                            OSkillModelCode = e.SkillModel.Code,
                            OSkillModelName = e.SkillModel.ModelName,
                            OSkillModelObject = e.SkillModel.SkillModelObject.Select(d => new
                            {
                                OLookupSkillModel = d.SkillModel.LookupVal,
                                OSkiCompetencyEvaluationModelCriteria = d.CompetencyEvaluationModel.Criteria.LookupVal,
                                OSkiCompetencyEvaluationModelCriteriaType = d.CompetencyEvaluationModel.CriteriaType.LookupVal,
                                OSkiCompetencyEvaluationModelDataSteps = d.CompetencyEvaluationModel.DataSteps.LookupVal

                            }).ToList(),
                            OTrainingModel = e.TrainingModel,
                            OTrainingModelCode = e.TrainingModel.Code,
                            OTrainingModelName = e.TrainingModel.ModelName,
                            OTrainingModelObject = e.TrainingModel.TrainingModelObject.Select(d => new
                            {
                                OLookupTrainingModel = d.TrainingModel.LookupVal,
                                OTCompetencyEvaluationModelCriteria = d.CompetencyEvaluationModel.Criteria.LookupVal,
                                OTCompetencyEvaluationModelCriteriaType = d.CompetencyEvaluationModel.CriteriaType.LookupVal,
                                OTCompetencyEvaluationModelDataSteps = d.CompetencyEvaluationModel.DataSteps.LookupVal

                            }).ToList()

                        }).ToList();

                        if (salheadlist.Count() > 0)
                        {
                            IS = IS.Where(e => salheadlist.Contains(e.OSuccessionModelCode)).ToList();

                        }
                        else
                        {
                            IS = IS.ToList();
                        }

                        if (IS == null && IS.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var item in IS)
                            {
                                if (item.OAppraisalAttributeModelObject.Count() > 0)
                                {
                                    foreach (var item1 in item.OAppraisalAttributeModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld1 = item.OSuccessionModelCode == null ? "" : item.OSuccessionModelCode,
                                            Fld2 = item.OSuccessionModelName == null ? "" : item.OSuccessionModelName,
                                            Fld3 = item.OSuccessionMdelCreatedDate == null ? "" : item.OSuccessionMdelCreatedDate.Value.ToShortDateString(),
                                            Fld4 = "AppraisalAttributeModel",
                                            Fld5 = (item.OAttributeModelCode + item.OAttributeModelName) == null ? "" : (item.OAttributeModelCode + " " + item.OAttributeModelName),
                                            Fld6 = "AppraisalAttributeModelObject",
                                            Fld7 = item1.OALookupAppraisalAttributeModel == null ? "" : item1.OALookupAppraisalAttributeModel,
                                            Fld8 = (item1.OACompetencyEvaluationModelCriteria + item1.OACompetencyEvaluationModelCriteriaType + item1.OACompetencyEvaluationModelDataSteps) == null ? "" : ("Evaluation" + " " + "Criteria :" + item1.OACompetencyEvaluationModelCriteria + " ," + "CriteriaType :" + item1.OACompetencyEvaluationModelCriteriaType + " ," + "DataSteps :" + item1.OACompetencyEvaluationModelDataSteps)
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                }

                                if (item.OAppraisalBusinessAppraisalModelObject.Count() > 0)
                                {
                                    foreach (var item1 in item.OAppraisalBusinessAppraisalModelObject)
                                    {

                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld1 = item.OSuccessionModelCode == null ? "" : item.OSuccessionModelCode,
                                            Fld2 = item.OSuccessionModelName == null ? "" : item.OSuccessionModelName,
                                            Fld3 = item.OSuccessionMdelCreatedDate == null ? "" : item.OSuccessionMdelCreatedDate.Value.ToShortDateString(),
                                            Fld4 = "AppraisalBusinessApprisalModel",
                                            Fld5 = (item.OBusinessApprisalModelCode + item.OBusinessApprisalModelName) == null ? "" : (item.OBusinessApprisalModelCode + " " + item.OBusinessApprisalModelName),
                                            Fld6 = "AppraisalBusinessAppraisalModelObject",
                                            Fld7 = item1.OLookupAppraisalBusinessAppraisalModel == null ? "" : item1.OLookupAppraisalBusinessAppraisalModel,
                                            Fld8 = (item1.OBCompetencyEvaluationModelCriteria + item1.OBCompetencyEvaluationModelCriteriaType + item1.OBCompetencyEvaluationModelDataSteps) == null ? "" : ("Evaluation" + " " + "Criteria :" + item1.OBCompetencyEvaluationModelCriteria + " ," + "CriteriaType :" + item1.OBCompetencyEvaluationModelCriteriaType + ", " + "DataSteps :" + item1.OBCompetencyEvaluationModelDataSteps)
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                }

                                if (item.OAppraisalKRAModelObject.Count() > 0)
                                {
                                    foreach (var item1 in item.OAppraisalKRAModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld1 = item.OSuccessionModelCode == null ? "" : item.OSuccessionModelCode,
                                            Fld2 = item.OSuccessionModelName == null ? "" : item.OSuccessionModelName,
                                            Fld3 = item.OSuccessionMdelCreatedDate == null ? "" : item.OSuccessionMdelCreatedDate.Value.ToShortDateString(),
                                            Fld4 = "AppraisalKRAModel",
                                            Fld5 = (item.OKRAModelCode + item.OKRAModelName) == null ? "" : (item.OKRAModelCode + item.OKRAModelName),
                                            Fld6 = "AppraisalKRAModelObject",
                                            Fld7 = item1.OLookupAppraisalKRAModel == null ? "" : item1.OLookupAppraisalKRAModel,
                                            Fld8 = (item1.OKCompetencyEvaluationModelCriteria + item1.OKCompetencyEvaluationModelCriteriaType + item1.OKCompetencyEvaluationModelDataSteps) == null ? "" : ("Evaluation" + " " + "Criteria :" + item1.OKCompetencyEvaluationModelCriteria + "," + "CriteriaType :" + item1.OKCompetencyEvaluationModelCriteriaType + ", " + "DataSteps :" + item1.OKCompetencyEvaluationModelDataSteps)
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                }

                                if (item.OAppraisalPotentialModelObject.Count() > 0)
                                {
                                    foreach (var item1 in item.OAppraisalPotentialModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld1 = item.OSuccessionModelCode == null ? "" : item.OSuccessionModelCode,
                                            Fld2 = item.OSuccessionModelName == null ? "" : item.OSuccessionModelName,
                                            Fld3 = item.OSuccessionMdelCreatedDate == null ? "" : item.OSuccessionMdelCreatedDate.Value.ToShortDateString(),
                                            Fld4 = "AppraisalPotentialModel",
                                            Fld5 = (item.OPotentialModelCode + item.OPotentialModelName) == null ? "" : (item.OPotentialModelCode + " " + item.OPotentialModelName),
                                            Fld6 = "AppraisalPotentialModelObject",
                                            Fld7 = item1.OLookupAppraisalPotentialModel == null ? "" : item1.OLookupAppraisalPotentialModel,
                                            Fld8 = (item1.OPoCompetencyEvaluationModelCriteria + item1.OPoCompetencyEvaluationModelCriteriaType + item1.OPoCompetencyEvaluationModelDataSteps) == null ? "" : ("Evaluation" + " " + "Criteria :" + item1.OPoCompetencyEvaluationModelCriteria + "," + "CriteriaType :" + item1.OPoCompetencyEvaluationModelCriteriaType + "," + "DataSteps :" + item1.OPoCompetencyEvaluationModelDataSteps)
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                }

                                if (item.OPastExperienceModelObject.Count() > 0)
                                {
                                    foreach (var item1 in item.OPastExperienceModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld1 = item.OSuccessionModelCode == null ? "" : item.OSuccessionModelCode,
                                            Fld2 = item.OSuccessionModelName == null ? "" : item.OSuccessionModelName,
                                            Fld3 = item.OSuccessionMdelCreatedDate == null ? "" : item.OSuccessionMdelCreatedDate.Value.ToShortDateString(),
                                            Fld4 = "PastExperienceModel",
                                            Fld5 = (item.OPastExperienceModelCode + item.OPastExperienceModelName) == null ? "" : (item.OPastExperienceModelCode + " " + item.OPastExperienceModelName),
                                            Fld6 = "PastExperienceModelObject",
                                            Fld7 = item1.OLookupPastExperienceModel == null ? "" : item1.OLookupPastExperienceModel,
                                            Fld8 = (item1.OPaCompetencyEvaluationModelCriteria + item1.OPaCompetencyEvaluationModelCriteriaType + item1.OPaCompetencyEvaluationModelDataSteps) == null ? "" : ("Evaluation" + " " + "Criteria :" + item1.OPaCompetencyEvaluationModelCriteria + "," + "CriteriaType :" + item1.OPaCompetencyEvaluationModelCriteriaType + "," + "DataSteps :" + item1.OPaCompetencyEvaluationModelDataSteps)
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                }

                                if (item.OPersonnelModelObject.Count() > 0)
                                {
                                    foreach (var item1 in item.OPersonnelModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld1 = item.OSuccessionModelCode == null ? "" : item.OSuccessionModelCode,
                                            Fld2 = item.OSuccessionModelName == null ? "" : item.OSuccessionModelName,
                                            Fld3 = item.OSuccessionMdelCreatedDate == null ? "" : item.OSuccessionMdelCreatedDate.Value.ToShortDateString(),
                                            Fld4 = "PersonnelModel",
                                            Fld5 = (item.OPersonnelModelCode + item.OPersonnelModelName) == null ? "" : (item.OPersonnelModelCode + " " + item.OPersonnelModelName),
                                            Fld6 = "PersonnelModelObject",
                                            Fld7 = item1.OLookupPersonnelModell == null ? "" : item1.OLookupPersonnelModell,
                                            Fld8 = (item1.OPerCompetencyEvaluationModelCriteria + item1.OPerCompetencyEvaluationModelCriteriaType + item1.OPerCompetencyEvaluationModelDataSteps) == null ? "" : ("Evaluation" + " " + "Criteria :" + item1.OPerCompetencyEvaluationModelCriteria + "," + "CriteriaType :" + item1.OPerCompetencyEvaluationModelCriteriaType + "," + "DataSteps :" + item1.OPerCompetencyEvaluationModelDataSteps)
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                }

                                if (item.OQualificationModelObject.Count() > 0)
                                {
                                    foreach (var item1 in item.OQualificationModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld1 = item.OSuccessionModelCode == null ? "" : item.OSuccessionModelCode,
                                            Fld2 = item.OSuccessionModelName == null ? "" : item.OSuccessionModelName,
                                            Fld3 = item.OSuccessionMdelCreatedDate == null ? "" : item.OSuccessionMdelCreatedDate.Value.ToShortDateString(),
                                            Fld4 = "QualificationModel",
                                            Fld5 = (item.OQualificationModelCode + item.OQualificationModelName) == null ? "" : (item.OQualificationModelCode + " " + item.OQualificationModelName),
                                            Fld6 = "QualificationModelObject",
                                            Fld7 = item1.OLookupQualificationModel == null ? "" : item1.OLookupQualificationModel,
                                            Fld8 = (item1.OQCompetencyEvaluationModelCriteria + item1.OQCompetencyEvaluationModelCriteriaType + item1.OQCompetencyEvaluationModelDataSteps) == null ? "" : ("Evaluation" + " " + "Criteria :" + item1.OQCompetencyEvaluationModelCriteria + "," + "CriteriaType :" + item1.OQCompetencyEvaluationModelCriteriaType + "," + "DataSteps :" + item1.OQCompetencyEvaluationModelDataSteps)
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                }

                                if (item.OServiceModelObject.Count() > 0)
                                {
                                    foreach (var item1 in item.OServiceModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld1 = item.OSuccessionModelCode == null ? "" : item.OSuccessionModelCode,
                                            Fld2 = item.OSuccessionModelName == null ? "" : item.OSuccessionModelName,
                                            Fld3 = item.OSuccessionMdelCreatedDate == null ? "" : item.OSuccessionMdelCreatedDate.Value.ToShortDateString(),
                                            Fld4 = "ServiceModel",
                                            Fld5 = (item.OServiceModelCode + item.OServiceModelName) == null ? "" : (item.OServiceModelCode + " " + item.OServiceModelName),
                                            Fld6 = "ServiceModelObject",
                                            Fld7 = item1.OLookupServiceModel == null ? "" : item1.OLookupServiceModel,
                                            Fld8 = (item1.OSerCompetencyEvaluationModelCriteria + item1.OSerCompetencyEvaluationModelCriteriaType + item1.OSerCompetencyEvaluationModelDataSteps) == null ? "" : ("Evaluation" + " " + "Criteria :" + item1.OSerCompetencyEvaluationModelCriteria + "," + "CriteriaType :" + item1.OSerCompetencyEvaluationModelCriteriaType + "," + "DataSteps :" + item1.OSerCompetencyEvaluationModelDataSteps)
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                }
                                if (item.OSkillModelObject.Count() > 0)
                                {
                                    foreach (var item1 in item.OSkillModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld1 = item.OSuccessionModelCode == null ? "" : item.OSuccessionModelCode,
                                            Fld2 = item.OSuccessionModelName == null ? "" : item.OSuccessionModelName,
                                            Fld3 = item.OSuccessionMdelCreatedDate == null ? "" : item.OSuccessionMdelCreatedDate.Value.ToShortDateString(),
                                            Fld4 = "SkillModel",
                                            Fld5 = (item.OSkillModelCode + item.OSkillModelName) == null ? "" : (item.OSkillModelCode + " " + item.OSkillModelName),
                                            Fld6 = "SkillModelObject",
                                            Fld7 = item1.OLookupSkillModel == null ? "" : item1.OLookupSkillModel,
                                            Fld8 = (item1.OSkiCompetencyEvaluationModelCriteria + item1.OSkiCompetencyEvaluationModelCriteriaType + item1.OSkiCompetencyEvaluationModelDataSteps) == null ? "" : ("Evaluation" + " " + "Criteria :" + item1.OSkiCompetencyEvaluationModelCriteria + "," + "CriteriaType :" + item1.OSkiCompetencyEvaluationModelCriteriaType + "," + "DataSteps :" + item1.OSkiCompetencyEvaluationModelDataSteps)
                                        };
                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                }

                                if (item.OTrainingModelObject.Count() > 0)
                                {
                                    foreach (var item1 in item.OTrainingModelObject)
                                    {
                                        GenericField100 ObjGenericField100 = new GenericField100()
                                        {
                                            Fld1 = item.OSuccessionModelCode == null ? "" : item.OSuccessionModelCode,
                                            Fld2 = item.OSuccessionModelName == null ? "" : item.OSuccessionModelName,
                                            Fld3 = item.OSuccessionMdelCreatedDate == null ? "" : item.OSuccessionMdelCreatedDate.Value.ToShortDateString(),
                                            Fld4 = "TrainingModel",
                                            Fld5 = (item.OTrainingModelCode + item.OTrainingModelName) == null ? "" : (item.OTrainingModelCode + " " + item.OTrainingModelName),
                                            Fld6 = "TrainingModelObject",
                                            Fld7 = item1.OLookupTrainingModel == null ? "" : item1.OLookupTrainingModel,
                                            Fld8 = (item1.OTCompetencyEvaluationModelCriteria + item1.OTCompetencyEvaluationModelCriteriaType + item1.OTCompetencyEvaluationModelDataSteps) == null ? "" : ("Evaluation" + " " + "Criteria :" + item1.OTCompetencyEvaluationModelCriteria + "," + "CriteriaType :" + item1.OTCompetencyEvaluationModelCriteriaType + "," + "DataSteps :" + item1.OTCompetencyEvaluationModelDataSteps)
                                        };

                                        OGenericPayrollStatement.Add(ObjGenericField100);
                                    }
                                }

                            }
                        }

                        return OGenericPayrollStatement;

                        break;

                    case "SUCCESSIONMODELASSIGNMENT":

                        List<GenericField100> OGenericSuccessionModelAssignment = new List<GenericField100>();

                        var IC = db.SuccessionModelAssignment.Select(d => new
                        {
                            OSuccessionModelAssignmentId = d.Id,
                            OBatchName = d.BatchName,
                            OBatchDescription = d.BatchDescription,
                            OSuccessionModelCode = d.SuccessionModel.Code,
                            OSuccessionModelName = d.SuccessionModel.ModelName,
                            OSuccessionModelAssignment_OrgStructure = d.SussessionModelAssignment_OrgStructure.Select(r => new
                            {
                                OSuccessionModelAssignment_OrgStructureId = r.Id,
                                OGeoStruct = r.GeoStruct.FullDetailsLD,
                                OPayStruct = r.PayStruct.FullDetails,
                                OFuncStruct = r.FuncStruct.FullDetails

                            }).ToList()

                        }).ToList();

                        var ICT = IC.OrderBy(d => d.OSuccessionModelAssignmentId).ToList();

                        if (salheadlist.Count() > 0)
                        {
                            ICT = ICT.Where(e => salheadlist.Contains(e.OBatchName)).ToList();
                        }
                        else
                        {
                            ICT = ICT.ToList();
                        }

                        if (ICT == null && ICT.Count() == 0)
                        {
                            return null;
                        }

                        else
                        {
                            foreach (var item in ICT)
                            {
                                if (item.OSuccessionModelAssignment_OrgStructure.Count() > 0)
                                {
                                    foreach (var item1 in item.OSuccessionModelAssignment_OrgStructure)
                                    {
                                        GenericField100 GenSuccessionModelAssignment100 = new GenericField100()
                                        {
                                            Fld1 = item.OBatchName == null ? "" : item.OBatchName,
                                            Fld2 = item.OBatchDescription == null ? "" : item.OBatchDescription,
                                            Fld3 = (item.OSuccessionModelCode + item.OSuccessionModelName) == null ? "" : (item.OSuccessionModelCode + item.OSuccessionModelName),
                                            Fld4 = item1.OGeoStruct == null ? "" : "GeoStruct :" + item1.OGeoStruct,
                                            Fld5 = item1.OPayStruct == null ? "" : "PayStruct :" + item1.OPayStruct,
                                            Fld6 = item1.OFuncStruct == null ? "" : "FuncStruct :" + item1.OFuncStruct

                                        };

                                        OGenericSuccessionModelAssignment.Add(GenSuccessionModelAssignment100);
                                    }
                                }
                            }
                        }

                        return OGenericSuccessionModelAssignment;

                        break;

                    case "SUCCESSIONEMPLOYEEDATAT":

                        List<GenericField100> OGenericSuccessionEmployeeDataT = new List<GenericField100>();

                        var OSuccessionBatchProcessT = db.SuccessionBatchProcessT.Select(z => new
                        {
                            z_ProcessBatch = z.ProcessBatch,
                            z_SuccessionEmployeeDataT = z.SuccessionEmployeeDataT.Select(y => new
                            {
                                y_Employee_Id = y.Employee_Id,
                                y_EmpCode = y.Employee.EmpCode,
                                y_EmpName = y.Employee.EmpName.FullNameFML,
                                y_LocDesc = y.Employee.GeoStruct.Location.LocationObj.LocDesc,
                                y_DeptDesc = y.Employee.GeoStruct.Department.DepartmentObj.DeptDesc,
                                y_JobName = y.Employee.FuncStruct.Job.Name,
                                y_BatchName = y.BatchName.BatchName,
                                y_AppraisalAttributeModelObjectT = y.AppraisalAttributeModelObjectT.Select(w => new
                                {
                                    w_AppraisalAttributeModelObject_Id = w.AppraisalAttributeModelObject_Id
                                }).ToList(),
                                y_AppraisalBusinessAppraisalModelObjectT = y.AppraisalBusinessAppraisalModelObjectT.Select(w => new
                                {
                                    w_AppraisalBusinessAppraisalModelObject_Id = w.AppraisalBusinessAppraisalModelObject_Id
                                }).ToList(),
                                y_AppraisalKRAModelObjectT = y.AppraisalKRAModelObjectT.Select(w => new
                                {
                                    w_AppraisalKRAModelObject_Id = w.AppraisalKRAModelObject_Id
                                }).ToList(),
                                y_AppraisalPotentialModelObjectT = y.AppraisalPotentialModelObjectT.Select(w => new
                                {
                                    w_AppraisalPotentialModelObject_Id = w.AppraisalPotentialModelObject_Id
                                }).ToList(),
                                y_PastExperienceModelObjectT = y.PastExperienceModelObjectT.Select(w => new
                                {
                                    w_PastExperienceModelObject_Id = w.PastExperienceModelObject_Id
                                }).ToList(),
                                y_PersonnelModelObjectT = y.PersonnelModelObjectT.Select(w => new
                                {
                                    w_PersonnelModelObject_Id = w.PersonnelModelObject_Id
                                }).ToList(),
                                y_QualificationModelObjectT = y.QualificationModelObjectT.Select(w => new
                                {
                                    w_QualificationModelObject_Id = w.QualificationModelObject_Id
                                }).ToList(),
                                y_ServiceModelObjectT = y.ServiceModelObjectT.Select(w => new
                                {
                                    w_ServiceModelObject_Id = w.ServiceModelObject_Id
                                }).ToList(),
                                y_SkillModelObjectT = y.SkillModelObjectT.Select(w => new
                                {
                                    w_SkillModelObject_Id = w.SkillModelObject_Id
                                }).ToList(),
                                y_TrainingModelObjectT = y.TrainingModelObjectT.Select(w => new
                                {
                                    w_TrainingModelObject_Id = w.TrainingModelObject_Id
                                }).ToList()
                            }).ToList()
                        }).ToList();


                        if (salheadlist.Count() > 0)
                        {
                            OSuccessionBatchProcessT = OSuccessionBatchProcessT.Where(e => salheadlist.Contains(e.z_ProcessBatch)).ToList();
                        }

                        else
                        {
                            OSuccessionBatchProcessT = OSuccessionBatchProcessT.ToList();
                        }


                        if (OSuccessionBatchProcessT.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var item in OSuccessionBatchProcessT)
                            {
                                foreach (var item1 in item.z_SuccessionEmployeeDataT)
                                {
                                    foreach (var item2 in item1.y_AppraisalAttributeModelObjectT)
                                    {
                                        var IH = db.SuccessionEmployeeDataGeneration.Include(e => e.Employee).Where(e => e.Employee.Id == item1.y_Employee_Id).Select(d => new
                                        {
                                            IHBatchName = d.BatchName.BatchName,
                                            OAppraisalAttributeModelObjectV = d.AppraisalAttributeModelObjectV.Select(r => new
                                            {
                                                IHAppraisalAttributeModelObjectId = r.AppraisalAttributeModelObject_Id,
                                                OAppraisalAttributeModelLookupval = r.AppraisalAttributeModelObject.AppraisalAttributeModel.LookupVal,

                                                OAbjectValue = r.ObjectValue.Select(t => new
                                                {
                                                    OAObjectVal = t.ObjectVal,

                                                }).ToList(),

                                            }).Where(r => r.IHAppraisalAttributeModelObjectId == item2.w_AppraisalAttributeModelObject_Id).ToList()

                                        }).Where(e => item1.y_BatchName.Contains(e.IHBatchName)).ToList();

                                        foreach (var item3 in IH)
                                        {
                                            foreach (var item4 in item3.OAppraisalAttributeModelObjectV)
                                            {
                                                foreach (var item5 in item4.OAbjectValue)
                                                {
                                                    GenericField100 GenSuccessionEmployeeDataT100 = new GenericField100()
                                                    {
                                                        Fld1 = item1.y_BatchName == null ? "" : item1.y_BatchName,
                                                        Fld4 = item1.y_EmpCode == null ? "" : item1.y_EmpCode,
                                                        Fld5 = item1.y_EmpName == null ? "" : item1.y_EmpName,
                                                        Fld6 = item1.y_LocDesc == null ? "" : item1.y_LocDesc,
                                                        Fld7 = item1.y_DeptDesc == null ? "" : item1.y_DeptDesc,
                                                        Fld8 = item1.y_JobName == null ? "" : item1.y_JobName,
                                                        Fld2 = "AppraisalAttributeModelObjectV",
                                                        Fld9 = item4.OAppraisalAttributeModelLookupval == null ? "" : item4.OAppraisalAttributeModelLookupval,
                                                        Fld3 = item5.OAObjectVal == null ? "" : item5.OAObjectVal

                                                    };

                                                    OGenericSuccessionEmployeeDataT.Add(GenSuccessionEmployeeDataT100);
                                                }
                                            }
                                        }
                                    }

                                    foreach (var item2 in item1.y_AppraisalBusinessAppraisalModelObjectT)
                                    {
                                        var IH = db.SuccessionEmployeeDataGeneration.Include(e => e.Employee).Where(e => e.Employee.Id == item1.y_Employee_Id).Select(d => new
                                        {
                                            IHBatchName = d.BatchName.BatchName,
                                            OAppraisalBusinessAppraisalModelObjectV = d.AppraisalBusinessAppraisalModelObjectV.Select(r => new
                                            {
                                                IHAppraisalBusinessAppraisalModelObjectId = r.AppraisalBusinessAppraisalModelObject_Id,
                                                OAppraisalBusinessAppraisalModelLookupval = r.AppraisalBusinessAppraisalModelObject.AppraisalBusinessAppraisalModel.LookupVal,

                                                OBbjectValue = r.ObjectValue.Select(t => new
                                                {
                                                    OBObjectVal = t.ObjectVal
                                                }).ToList(),
                                            }).Where(r => r.IHAppraisalBusinessAppraisalModelObjectId == item2.w_AppraisalBusinessAppraisalModelObject_Id).ToList(),

                                        }).Where(e => item1.y_BatchName.Contains(e.IHBatchName)).ToList();

                                        foreach (var item3 in IH)
                                        {
                                            foreach (var item4 in item3.OAppraisalBusinessAppraisalModelObjectV)
                                            {
                                                foreach (var item5 in item4.OBbjectValue)
                                                {
                                                    GenericField100 GenSuccessionEmployeeDataT100 = new GenericField100()
                                                    {
                                                        Fld1 = item1.y_BatchName == null ? "" : item1.y_BatchName,
                                                        Fld4 = item1.y_EmpCode == null ? "" : item1.y_EmpCode,
                                                        Fld5 = item1.y_EmpName == null ? "" : item1.y_EmpName,
                                                        Fld6 = item1.y_LocDesc == null ? "" : item1.y_LocDesc,
                                                        Fld7 = item1.y_DeptDesc == null ? "" : item1.y_DeptDesc,
                                                        Fld8 = item1.y_JobName == null ? "" : item1.y_JobName,
                                                        Fld2 = "AppraisalBusinessAppraisalModelObjectV",
                                                        Fld9 = item4.OAppraisalBusinessAppraisalModelLookupval == null ? "" : item4.OAppraisalBusinessAppraisalModelLookupval,
                                                        Fld3 = item5.OBObjectVal == null ? "" : item5.OBObjectVal

                                                    };

                                                    OGenericSuccessionEmployeeDataT.Add(GenSuccessionEmployeeDataT100);
                                                }
                                            }
                                        }
                                    }

                                    foreach (var item2 in item1.y_AppraisalKRAModelObjectT)
                                    {
                                        var IH = db.SuccessionEmployeeDataGeneration.Include(e => e.Employee).Where(e => e.Employee.Id == item1.y_Employee_Id).Select(d => new
                                        {
                                            IHBatchName = d.BatchName.BatchName,
                                            OAppraisalKRAModelObjectV = d.AppraisalKRAModelObjectV.Select(r => new
                                            {
                                                IHAppraisalKRAModelObjectId = r.AppraisalKRAModelObject_Id,
                                                OAppraisalKRAModelLookupval = r.AppraisalKRAModelObject.AppraisalKRAModel.LookupVal,

                                                OKbjectValue = r.ObjectValue.Select(t => new
                                                {
                                                    OKObjectVal = t.ObjectVal
                                                }).ToList(),
                                            }).Where(r => r.IHAppraisalKRAModelObjectId == item2.w_AppraisalKRAModelObject_Id).ToList(),

                                        }).Where(e => item1.y_BatchName.Contains(e.IHBatchName)).ToList();

                                        foreach (var item3 in IH)
                                        {
                                            foreach (var item4 in item3.OAppraisalKRAModelObjectV)
                                            {
                                                foreach (var item5 in item4.OKbjectValue)
                                                {
                                                    GenericField100 GenSuccessionEmployeeDataT100 = new GenericField100()
                                                    {
                                                        Fld1 = item1.y_BatchName == null ? "" : item1.y_BatchName,
                                                        Fld4 = item1.y_EmpCode == null ? "" : item1.y_EmpCode,
                                                        Fld5 = item1.y_EmpName == null ? "" : item1.y_EmpName,
                                                        Fld6 = item1.y_LocDesc == null ? "" : item1.y_LocDesc,
                                                        Fld7 = item1.y_DeptDesc == null ? "" : item1.y_DeptDesc,
                                                        Fld8 = item1.y_JobName == null ? "" : item1.y_JobName,
                                                        Fld2 = "AppraisalKRAModelObjectV",
                                                        Fld9 = item4.OAppraisalKRAModelLookupval == null ? "" : item4.OAppraisalKRAModelLookupval,
                                                        Fld3 = item5.OKObjectVal == null ? "" : item5.OKObjectVal

                                                    };

                                                    OGenericSuccessionEmployeeDataT.Add(GenSuccessionEmployeeDataT100);
                                                }
                                            }
                                        }
                                    }

                                    foreach (var item2 in item1.y_AppraisalPotentialModelObjectT)
                                    {
                                        var IH = db.SuccessionEmployeeDataGeneration.Include(e => e.Employee).Where(e => e.Employee.Id == item1.y_Employee_Id).Select(d => new
                                        {
                                            IHBatchName = d.BatchName.BatchName,
                                            OAppraisalPotentialModelObjectV = d.AppraisalPotentialModelObjectV.Select(r => new
                                            {
                                                IHAppraisalPotentialModelObject = r.AppraisalPotentialModelObject_Id,
                                                OAppraisalPotentialModelLookupval = r.AppraisalPotentialModelObject.AppraisalPotentialModel.LookupVal,

                                                OPobjectValue = r.ObjectValue.Select(t => new
                                                {
                                                    OPoObjectVal = t.ObjectVal
                                                }).ToList(),
                                            }).Where(r => r.IHAppraisalPotentialModelObject == item2.w_AppraisalPotentialModelObject_Id).ToList(),

                                        }).Where(e => item1.y_BatchName.Contains(e.IHBatchName)).ToList();

                                        foreach (var item3 in IH)
                                        {
                                            foreach (var item4 in item3.OAppraisalPotentialModelObjectV)
                                            {
                                                foreach (var item5 in item4.OPobjectValue)
                                                {
                                                    GenericField100 GenSuccessionEmployeeDataT100 = new GenericField100()
                                                    {
                                                        Fld1 = item1.y_BatchName == null ? "" : item1.y_BatchName,
                                                        Fld4 = item1.y_EmpCode == null ? "" : item1.y_EmpCode,
                                                        Fld5 = item1.y_EmpName == null ? "" : item1.y_EmpName,
                                                        Fld6 = item1.y_LocDesc == null ? "" : item1.y_LocDesc,
                                                        Fld7 = item1.y_DeptDesc == null ? "" : item1.y_DeptDesc,
                                                        Fld8 = item1.y_JobName == null ? "" : item1.y_JobName,
                                                        Fld2 = "AppraisalPotentialModelObjectV",
                                                        Fld9 = item4.OAppraisalPotentialModelLookupval == null ? "" : item4.OAppraisalPotentialModelLookupval,
                                                        Fld3 = item5.OPoObjectVal == null ? "" : item5.OPoObjectVal
                                                    };

                                                    OGenericSuccessionEmployeeDataT.Add(GenSuccessionEmployeeDataT100);
                                                }
                                            }
                                        }
                                    }

                                    foreach (var item2 in item1.y_PastExperienceModelObjectT)
                                    {
                                        var IH = db.SuccessionEmployeeDataGeneration.Include(e => e.Employee).Where(e => e.Employee.Id == item1.y_Employee_Id).Select(d => new
                                        {
                                            IHBatchName = d.BatchName.BatchName,
                                            OPastExperienceModelObjectV = d.PastExperienceModelObjectV.Select(r => new
                                            {
                                                IHPastExperienceModelObjectId = r.PastExperienceModelObject_Id,
                                                OPastExperienceModelLookupval = r.PastExperienceModelObject.PastExperienceModel.LookupVal,

                                                OPabjectValue = r.ObjectValue.Select(t => new
                                                {
                                                    OPoObjectVal = t.ObjectVal
                                                }).ToList(),
                                            }).Where(r => r.IHPastExperienceModelObjectId == item2.w_PastExperienceModelObject_Id).ToList(),

                                        }).Where(e => item1.y_BatchName.Contains(e.IHBatchName)).ToList();

                                        foreach (var item3 in IH)
                                        {
                                            foreach (var item4 in item3.OPastExperienceModelObjectV)
                                            {
                                                foreach (var item5 in item4.OPabjectValue)
                                                {
                                                    GenericField100 GenSuccessionEmployeeDataT100 = new GenericField100()
                                                    {
                                                        Fld1 = item1.y_BatchName == null ? "" : item1.y_BatchName,
                                                        Fld4 = item1.y_EmpCode == null ? "" : item1.y_EmpCode,
                                                        Fld5 = item1.y_EmpName == null ? "" : item1.y_EmpName,
                                                        Fld6 = item1.y_LocDesc == null ? "" : item1.y_LocDesc,
                                                        Fld7 = item1.y_DeptDesc == null ? "" : item1.y_DeptDesc,
                                                        Fld8 = item1.y_JobName == null ? "" : item1.y_JobName,
                                                        Fld2 = "PastExperienceModelObjectV",
                                                        Fld9 = item4.OPastExperienceModelLookupval == null ? "" : item4.OPastExperienceModelLookupval,
                                                        Fld3 = item5.OPoObjectVal == null ? "" : item5.OPoObjectVal

                                                    };

                                                    OGenericSuccessionEmployeeDataT.Add(GenSuccessionEmployeeDataT100);
                                                }
                                            }
                                        }
                                    }

                                    foreach (var item2 in item1.y_PersonnelModelObjectT)
                                    {
                                        var IH = db.SuccessionEmployeeDataGeneration.Include(e => e.Employee).Where(e => e.Employee.Id == item1.y_Employee_Id).Select(d => new
                                        {
                                            IHBatchName = d.BatchName.BatchName,
                                            OPersonnelModelObjectV = d.PersonnelModelObjectV.Select(r => new
                                            {
                                                IHPersonnelModelObjectId = r.PersonnelModelObject_Id,
                                                OPersonnelModelLookupval = r.PersonnelModelObject.PersonnelModel.LookupVal,

                                                OPerbjectValue = r.ObjectValue.Select(t => new
                                                {
                                                    OPerObjectVal = t.ObjectVal
                                                }).ToList(),
                                            }).Where(r => r.IHPersonnelModelObjectId == item2.w_PersonnelModelObject_Id).ToList(),

                                        }).Where(e => item1.y_BatchName.Contains(e.IHBatchName)).ToList();

                                        foreach (var item3 in IH)
                                        {
                                            foreach (var item4 in item3.OPersonnelModelObjectV)
                                            {
                                                foreach (var item5 in item4.OPerbjectValue)
                                                {
                                                    GenericField100 GenSuccessionEmployeeDataT100 = new GenericField100()
                                                    {
                                                        Fld1 = item1.y_BatchName == null ? "" : item1.y_BatchName,
                                                        Fld4 = item1.y_EmpCode == null ? "" : item1.y_EmpCode,
                                                        Fld5 = item1.y_EmpName == null ? "" : item1.y_EmpName,
                                                        Fld6 = item1.y_LocDesc == null ? "" : item1.y_LocDesc,
                                                        Fld7 = item1.y_DeptDesc == null ? "" : item1.y_DeptDesc,
                                                        Fld8 = item1.y_JobName == null ? "" : item1.y_JobName,
                                                        Fld2 = "PersonnelModelObjectV",
                                                        Fld9 = item4.OPersonnelModelLookupval == null ? "" : item4.OPersonnelModelLookupval,
                                                        Fld3 = item5.OPerObjectVal == null ? "" : item5.OPerObjectVal


                                                    };

                                                    OGenericSuccessionEmployeeDataT.Add(GenSuccessionEmployeeDataT100);
                                                }
                                            }
                                        }
                                    }

                                    foreach (var item2 in item1.y_QualificationModelObjectT)
                                    {
                                        var IH = db.SuccessionEmployeeDataGeneration.Include(e => e.Employee).Where(e => e.Employee.Id == item1.y_Employee_Id).Select(d => new
                                        {
                                            IHBatchName = d.BatchName.BatchName,
                                            OQualificationModelObjectV = d.QualificationModelObjectV.Select(r => new
                                            {
                                                IHQualificationModelObjectId = r.QualificationModelObject_Id,
                                                OQualificationModelLookupval = r.QualificationModelObject.QualificationModel.LookupVal,

                                                OQuabjectValue = r.ObjectValue.Select(t => new
                                                {
                                                    OQObjectVal = t.ObjectVal
                                                }).ToList(),
                                            }).Where(r => r.IHQualificationModelObjectId == item2.w_QualificationModelObject_Id).ToList(),

                                        }).Where(e => item1.y_BatchName.Contains(e.IHBatchName)).ToList();

                                        foreach (var item3 in IH)
                                        {
                                            foreach (var item4 in item3.OQualificationModelObjectV)
                                            {
                                                foreach (var item5 in item4.OQuabjectValue)
                                                {
                                                    GenericField100 GenSuccessionEmployeeDataT100 = new GenericField100()
                                                    {
                                                        Fld1 = item1.y_BatchName == null ? "" : item1.y_BatchName,
                                                        Fld4 = item1.y_EmpCode == null ? "" : item1.y_EmpCode,
                                                        Fld5 = item1.y_EmpName == null ? "" : item1.y_EmpName,
                                                        Fld6 = item1.y_LocDesc == null ? "" : item1.y_LocDesc,
                                                        Fld7 = item1.y_DeptDesc == null ? "" : item1.y_DeptDesc,
                                                        Fld8 = item1.y_JobName == null ? "" : item1.y_JobName,
                                                        Fld2 = "QualificationModelObjectV",
                                                        Fld9 = item4.OQualificationModelLookupval == null ? "" : item4.OQualificationModelLookupval,
                                                        Fld3 = item5.OQObjectVal == null ? "" : item5.OQObjectVal

                                                    };
                                                    OGenericSuccessionEmployeeDataT.Add(GenSuccessionEmployeeDataT100);
                                                }
                                            }
                                        }
                                    }

                                    foreach (var item2 in item1.y_ServiceModelObjectT)
                                    {
                                        var IH = db.SuccessionEmployeeDataGeneration.Include(e => e.Employee).Where(e => e.Employee.Id == item1.y_Employee_Id).Select(d => new
                                        {
                                            IHBatchName = d.BatchName.BatchName,
                                            OServiceModelObjectV = d.ServiceModelObjectV.Select(r => new
                                            {
                                                IHServiceModelObjectId = r.ServiceModelObject_Id,
                                                OServiceModelLookupval = r.ServiceModelObject.ServiceModel.LookupVal,

                                                OSerbjectValue = r.ObjectValue.Select(t => new
                                                {
                                                    OSerObjectVal = t.ObjectVal
                                                }).ToList(),
                                            }).Where(r => r.IHServiceModelObjectId == item2.w_ServiceModelObject_Id).ToList(),

                                        }).Where(e => item1.y_BatchName.Contains(e.IHBatchName)).ToList();

                                        foreach (var item3 in IH)
                                        {
                                            foreach (var item4 in item3.OServiceModelObjectV)
                                            {
                                                foreach (var item5 in item4.OSerbjectValue)
                                                {
                                                    GenericField100 GenSuccessionEmployeeDataT100 = new GenericField100()
                                                    {
                                                        Fld1 = item1.y_BatchName == null ? "" : item1.y_BatchName,
                                                        Fld4 = item1.y_EmpCode == null ? "" : item1.y_EmpCode,
                                                        Fld5 = item1.y_EmpName == null ? "" : item1.y_EmpName,
                                                        Fld6 = item1.y_LocDesc == null ? "" : item1.y_LocDesc,
                                                        Fld7 = item1.y_DeptDesc == null ? "" : item1.y_DeptDesc,
                                                        Fld8 = item1.y_JobName == null ? "" : item1.y_JobName,
                                                        Fld2 = "ServiceModelObjectV",
                                                        Fld9 = item4.OServiceModelLookupval == null ? "" : item4.OServiceModelLookupval,
                                                        Fld3 = item5.OSerObjectVal == null ? "" : item5.OSerObjectVal

                                                    };
                                                    OGenericSuccessionEmployeeDataT.Add(GenSuccessionEmployeeDataT100);
                                                }
                                            }
                                        }
                                    }

                                    foreach (var item2 in item1.y_SkillModelObjectT)
                                    {
                                        var IH = db.SuccessionEmployeeDataGeneration.Include(e => e.Employee).Where(e => e.Employee.Id == item1.y_Employee_Id).Select(d => new
                                        {
                                            IHBatchName = d.BatchName.BatchName,
                                            OSkillModelObjectV = d.SkillModelObjectV.Select(r => new
                                            {
                                                IHSkillModelObjectId = r.SkillModelObject_Id,
                                                OSkillModelLookupval = r.SkillModelObject.SkillModel.LookupVal,

                                                OSkibjectValue = r.ObjectValue.Select(t => new
                                                {
                                                    OSkiObjectVal = t.ObjectVal
                                                }).ToList(),
                                            }).Where(r => r.IHSkillModelObjectId == item2.w_SkillModelObject_Id).ToList(),

                                        }).Where(e => item1.y_BatchName.Contains(e.IHBatchName)).ToList();

                                        foreach (var item3 in IH)
                                        {
                                            foreach (var item4 in item3.OSkillModelObjectV)
                                            {
                                                foreach (var item5 in item4.OSkibjectValue)
                                                {
                                                    GenericField100 GenSuccessionEmployeeDataT100 = new GenericField100()
                                                    {
                                                        Fld1 = item1.y_BatchName == null ? "" : item1.y_BatchName,
                                                        Fld4 = item1.y_EmpCode == null ? "" : item1.y_EmpCode,
                                                        Fld5 = item1.y_EmpName == null ? "" : item1.y_EmpName,
                                                        Fld6 = item1.y_LocDesc == null ? "" : item1.y_LocDesc,
                                                        Fld7 = item1.y_DeptDesc == null ? "" : item1.y_DeptDesc,
                                                        Fld8 = item1.y_JobName == null ? "" : item1.y_JobName,
                                                        Fld2 = "SkillModelObjectV",
                                                        Fld9 = item4.OSkillModelLookupval == null ? "" : item4.OSkillModelLookupval,
                                                        Fld3 = item5.OSkiObjectVal == null ? "" : item5.OSkiObjectVal

                                                    };
                                                    OGenericSuccessionEmployeeDataT.Add(GenSuccessionEmployeeDataT100);
                                                }
                                            }
                                        }
                                    }

                                    foreach (var item2 in item1.y_TrainingModelObjectT)
                                    {
                                        var IH = db.SuccessionEmployeeDataGeneration.Include(e => e.Employee).Where(e => e.Employee.Id == item1.y_Employee_Id).Select(d => new
                                        {
                                            IHBatchName = d.BatchName.BatchName,
                                            OTrainingModelObjectV = d.TrainingModelObjectV.Select(r => new
                                            {
                                                IHTrainingModelObjectId = r.TrainingModelObject_Id,
                                                OTrainingModelLookupval = r.TrainingModelObject.TrainingModel.LookupVal,
                                                OTbjectValue = r.ObjectValue.Select(t => new
                                                {
                                                    OTObjectVal = t.ObjectVal
                                                }).ToList(),
                                            }).Where(r => r.IHTrainingModelObjectId == item2.w_TrainingModelObject_Id).ToList()

                                        }).Where(e => item1.y_BatchName.Contains(e.IHBatchName)).ToList();

                                        foreach (var item3 in IH)
                                        {
                                            foreach (var item4 in item3.OTrainingModelObjectV)
                                            {
                                                foreach (var item5 in item4.OTbjectValue)
                                                {
                                                    GenericField100 GenCompetencyEmployeeDataT100 = new GenericField100()
                                                    {
                                                        Fld1 = item1.y_BatchName == null ? "" : item1.y_BatchName,
                                                        Fld4 = item1.y_EmpCode == null ? "" : item1.y_EmpCode,
                                                        Fld5 = item1.y_EmpName == null ? "" : item1.y_EmpName,
                                                        Fld6 = item1.y_LocDesc == null ? "" : item1.y_LocDesc,
                                                        Fld7 = item1.y_DeptDesc == null ? "" : item1.y_DeptDesc,
                                                        Fld8 = item1.y_JobName == null ? "" : item1.y_JobName,
                                                        Fld2 = "TrainingModelObjectV",
                                                        Fld9 = item4.OTrainingModelLookupval == null ? "" : item4.OTrainingModelLookupval,
                                                        Fld3 = item5.OTObjectVal == null ? "" : item5.OTObjectVal

                                                    };

                                                    OGenericSuccessionEmployeeDataT.Add(GenCompetencyEmployeeDataT100);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        return OGenericSuccessionEmployeeDataT;
                        break;


                    #region PFEmpLedger code start

                    case "PASSBOOK":

                        var OPFEmployeeLedData = new List<EmployeePFTrust>();

                        foreach (var item in EmpPayrollIdList)
                        {
                            var OPFLedgData_t = db.EmployeePFTrust
                               .Include(e => e.Employee)
                                //.Include(e=>e.Employee.PerAddr)
                                //.Include(e=>e.Employee.ServiceBookDates)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.PFTEmployeeLedger)                               
                                // .Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.PayStruct)                              
                                 .Where(e => e.Employee_Id == item).AsNoTracking()
                                 .FirstOrDefault();

                            List<PFTEmployeeLedger> PFTEmpLedger = new List<PFTEmployeeLedger>();
                            if (OPFLedgData_t != null)
                            {
                                OPFLedgData_t.Employee.EmpName = db.NameSingle.Find(OPFLedgData_t.Employee.EmpName_Id);
                                OPFLedgData_t.Employee.EmpOffInfo = db.EmpOff.Find(OPFLedgData_t.Employee.EmpOffInfo_Id);
                                OPFLedgData_t.Employee.EmpOffInfo.NationalityID = db.NationalityID.Find(OPFLedgData_t.Employee.EmpOffInfo.NationalityID);
                                OPFLedgData_t.Employee.ServiceBookDates = db.ServiceBookDates.Find(OPFLedgData_t.Employee.ServiceBookDates_Id);
                                OPFLedgData_t.Employee.PerAddr = db.Address.Find(OPFLedgData_t.Employee.PerAddr_Id);
                                OPFLedgData_t.PFTEmployeeLedger = db.PFTEmployeeLedger.Include(e => e.PassbookActivity).Where(e => e.EmployeePFTrust_Id == OPFLedgData_t.Id).ToList();
                                OPFEmployeeLedData.Add(OPFLedgData_t);
                            }
                        }

                        if (OPFEmployeeLedData == null || OPFEmployeeLedData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            //var month = false;
                            var emp = false;
                            var dept = false;
                            var loca = false;
                            var comp = false;
                            var grp = false;
                            var unit = false;
                            var div = false;
                            var regn = false;
                            var grade = false;
                            var lvl = false;
                            var jobstat = false;
                            var job = false;
                            var jobpos = false;
                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();


                            foreach (var item in vc)
                            {

                                //if (item.LookupVal.ToUpper() == "MONTH")
                                //{
                                //    month = true;
                                //}
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {

                                    loca = true;
                                }
                                if (item.LookupVal.ToUpper() == "EMPLOYEE")
                                {
                                    emp = true;
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                }
                                if (item.LookupVal.ToUpper() == "COMPANY")
                                {
                                    comp = true;
                                }
                                if (item.LookupVal.ToUpper() == "GROUP")
                                {
                                    grp = true;
                                }
                                if (item.LookupVal.ToUpper() == "UNIT")
                                {
                                    unit = true;
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                }
                                if (item.LookupVal.ToUpper() == "REGION")
                                {
                                    regn = true;
                                }
                                if (item.LookupVal.ToUpper() == "GRADE")
                                {
                                    grade = true;
                                }
                                if (item.LookupVal.ToUpper() == "LEVEL")
                                {
                                    lvl = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBSTATUS")
                                {
                                    jobstat = true;
                                }

                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBPOSITION")
                                {
                                    jobpos = true;
                                }
                            }


                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();
                            var PassbookLoanIDValue = new List<string>();
                            // PassbookLoanIDValue.Add("LOAN DEBIT BALANCE");// surat dcc loan closing balance show
                            List<int> PassbookLoanID = new List<int>();
                            PassbookLoanID = db.LookupValue.Where(e => PassbookLoanIDValue.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();


                            foreach (var ca in OPFEmployeeLedData)
                            {
                                if (ca.PFTEmployeeLedger != null && ca.PFTEmployeeLedger.Count() != 0)
                                {

                                    // var OPFTEMPLEDData = ca.PFTEmployeeLedger.Where(e => e.PostingDate >= pFromDate && e.PostingDate <= pToDate).ToList();
                                    var OPFTEMPLEDData = ca.PFTEmployeeLedger.Where(e => e.CalcDate >= pFromDate && e.CalcDate <= pToDate && PassbookLoanID.Contains(e.PassbookActivity.Id) == false).OrderBy(e => e.Id).ToList();

                                    if (OPFTEMPLEDData != null && OPFTEMPLEDData.Count() != 0)
                                    {
                                        int? geoid = ca.Employee.GeoStruct_Id;

                                        int? payid = ca.Employee.PayStruct_Id;

                                        int? funid = ca.Employee.FuncStruct_Id;

                                        GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                        PayStruct paystruct = db.PayStruct.Find(payid);

                                        FuncStruct funstruct = db.FuncStruct.Find(funid);

                                        GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                        if (GeoDataInd != null)
                                        {


                                            foreach (var ca1 in OPFTEMPLEDData)
                                            {
                                                string Activity = "";
                                                double ownpf = 0;
                                                double ownerpf = 0;
                                                double VPF = 0;
                                                double ownpfint = 0;
                                                double ownerpfint = 0;
                                                double VPFint = 0;
                                                double totalLoanAmount = 0;
                                                double totalPFbalance = 0;
                                                double totalIntbalance = 0;
                                                double TDSbal = 0;
                                                double Finalbalance = 0;
                                                double Pension = 0;

                                                if (ca1.PassbookActivity.LookupVal.ToUpper() == "PF BALANCE" && ca1.Narration == "Opening Balance")
                                                {
                                                    Activity = "Opening Balance";
                                                    ownpf = ca1.OwnOpenBal;
                                                    ownerpf = ca1.OwnerOpenBal;
                                                    VPF = ca1.VPFIntOpenBal;
                                                    ownpfint = ca1.OwnIntOpenBal;
                                                    ownerpfint = ca1.OwnerIntOpenBal;
                                                    VPFint = ca1.VPFIntOpenBal;
                                                    totalPFbalance = ca1.OwnOpenBal + ca1.OwnerOpenBal + ca1.VPFOpenBal;
                                                    totalIntbalance = ca1.TotalIntOpenBal;
                                                    TDSbal = ca1.TDSAmount;
                                                    Finalbalance = (ca1.OwnOpenBal + ca1.OwnerOpenBal + ca1.VPFOpenBal + ca1.TotalIntOpenBal - ca1.TDSAmount);
                                                }
                                                else if (ca1.PassbookActivity.LookupVal.ToUpper() == "PF BALANCE" || ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN DEBIT BALANCE" || ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN CREDIT BALANCE" || ca1.PassbookActivity.LookupVal.ToUpper() == "INTEREST BALANCE" || ca1.PassbookActivity.LookupVal.ToUpper() == "SETTLEMENT BALANCE")
                                                {
                                                    if (ca1.Narration != "Opening Balance")
                                                    {
                                                        Activity = "Closing Balance";
                                                        ownpf = ca1.OwnCloseBal;
                                                        ownerpf = ca1.OwnerCloseBal;
                                                        VPF = ca1.VPFCloseBal;
                                                        ownpfint = ca1.OwnIntCloseBal;
                                                        ownerpfint = ca1.OwnerIntCloseBal;
                                                        VPFint = ca1.VPFIntCloseBal;
                                                        totalPFbalance = ca1.OwnCloseBal + ca1.OwnerCloseBal + ca1.VPFCloseBal;
                                                        totalIntbalance = ca1.TotalIntCloseBal;
                                                        TDSbal = ca1.TDSAmount;
                                                        Finalbalance = (ca1.OwnCloseBal + ca1.OwnerCloseBal + ca1.VPFCloseBal + ca1.TotalIntCloseBal - ca1.TDSAmount);
                                                    }

                                                }
                                                else if (ca1.PassbookActivity.LookupVal.ToUpper() == "MONTHLY PF POSTING" || ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN DEBIT POSTING" || ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN CREDIT POSTING" || ca1.PassbookActivity.LookupVal.ToUpper() == "INTEREST POSTING" || ca1.PassbookActivity.LookupVal.ToUpper() == "SETTLEMENT POSTING")
                                                {
                                                    Activity = "Transaction";
                                                    if (ca1.PassbookActivity.LookupVal.ToUpper() == "MONTHLY PF POSTING")
                                                    {
                                                        ownpf = ca1.OwnPFMonthly;
                                                        ownerpf = ca1.OwnerPFMonthly;
                                                        VPF = ca1.VPFAmountMonthly;
                                                        TDSbal = ca1.TDSAmount;
                                                        Pension = ca1.PensionAmount;
                                                    }
                                                    if (ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN DEBIT POSTING")
                                                    {
                                                        ownpf = ca1.OwnPFLoan;
                                                        ownerpf = ca1.OwnerPFLoan;
                                                        VPF = ca1.VPFPFLoan;
                                                        ownpfint = ca1.OwnIntPFLoan;
                                                        ownerpfint = ca1.OwnerIntPFLoan;
                                                        VPFint = ca1.VPFIntPFLoan;
                                                        totalLoanAmount = ca1.LoanAmountDebit;
                                                        TDSbal = ca1.TDSAmount;
                                                    }
                                                    if (ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN CREDIT POSTING")
                                                    {

                                                    }
                                                    if (ca1.PassbookActivity.LookupVal.ToUpper() == "INTEREST POSTING" || ca1.PassbookActivity.LookupVal.ToUpper() == "SETTLEMENT POSTING")
                                                    {
                                                        ownpfint = ca1.OwnPFInt + ca1.OwnIntOnInt;
                                                        ownerpfint = ca1.OwnerPFInt + ca1.OwnerIntOnInt;
                                                        VPFint = ca1.VPFInt + ca1.VPFIntOnInt;
                                                        TDSbal = ca1.TDSAmount;
                                                    }

                                                }
                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                {


                                                    Fld2 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                                    Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                    Fld4 = GeoDataInd.FuncStruct_Job_Name,
                                                    Fld5 = ca1.PassbookActivity != null ? ca1.PassbookActivity.LookupVal : "",
                                                    Fld6 = GeoDataInd.GeoStruct_Location_Name,
                                                    Fld7 = ca.Employee.PerAddr != null ? ca.Employee.PerAddr.FullAddress : "",
                                                    Fld8 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.JoiningDate != null ? ca.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : "",
                                                    Fld9 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.RetirementDate != null ? ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString() : "",
                                                    Fld10 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.BirthDate != null ? ca.Employee.ServiceBookDates.BirthDate.Value.ToShortDateString() : "",
                                                    Fld11 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.PFJoingDate != null ? ca.Employee.ServiceBookDates.PFJoingDate.Value.ToShortDateString() : "",
                                                    Fld12 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.PensionJoingDate != null ? ca.Employee.ServiceBookDates.PensionJoingDate.Value.ToShortDateString() : "",
                                                    Fld13 = ca.Employee.EmpOffInfo != null && ca.Employee.EmpOffInfo.NationalityID != null ? ca.Employee.EmpOffInfo.NationalityID.PensionNo.ToString() : "",

                                                    Fld14 = ca1.MonthYear == null ? "" : ca1.MonthYear.ToString(),
                                                    Fld15 = ca1.Id.ToString(),
                                                    Fld43 = Activity.ToString(),
                                                    Fld16 = ownpf.ToString(),
                                                    Fld17 = ownerpf.ToString(),
                                                    Fld18 = VPF.ToString(),
                                                    Fld19 = ownpfint.ToString(),
                                                    Fld20 = ownerpfint.ToString(),
                                                    Fld21 = VPFint.ToString(),
                                                    Fld22 = ca1.PassbookActivity.LookupVal.ToUpper(),
                                                    Fld23 = totalLoanAmount.ToString(),
                                                    Fld24 = totalPFbalance.ToString(),
                                                    Fld25 = totalIntbalance.ToString(),
                                                    Fld26 = TDSbal.ToString(),
                                                    Fld27 = Finalbalance.ToString(),
                                                    Fld28 = Pension.ToString(),
                                                    //Fld16 = ca1.OwnPFMonthly.ToString() != null ? ca1.OwnPFMonthly.ToString() : "0",
                                                    //Fld17 = ca1.OwnPFInt.ToString() != null ? ca1.OwnPFInt.ToString() : "0",
                                                    //Fld18 = ca1.OwnIntOnInt.ToString() != null ? ca1.OwnIntOnInt.ToString() : "0",
                                                    //Fld19 = ca1.OwnCloseBal.ToString() != null ? ca1.OwnCloseBal.ToString() : "0",


                                                    //Fld20 = ca1.OwnerPFMonthly.ToString() != null ? ca1.OwnerPFMonthly.ToString() : "0",
                                                    //Fld21 = ca1.OwnerPFInt.ToString() != null ? ca1.OwnerPFInt.ToString() : "0",
                                                    //Fld22 = ca1.OwnerIntOnInt.ToString() != null ? ca1.OwnerIntOnInt.ToString() : "0",
                                                    //Fld23 = ca1.OwnerCloseBal.ToString() != null ? ca1.OwnerCloseBal.ToString() : "0",

                                                    //Fld24 = ca1.VPFAmountMonthly.ToString() != null ? ca1.VPFAmountMonthly.ToString() : "0",
                                                    //Fld25 = ca1.VPFInt.ToString() != null ? ca1.VPFInt.ToString() : "0",
                                                    //Fld26 = ca1.VPFIntOnInt.ToString() != null ? ca1.VPFIntOnInt.ToString() : "0",
                                                    //Fld27 = ca1.VPFCloseBal.ToString() != null ? ca1.VPFCloseBal.ToString() : "0",

                                                    //Fld28 = (ca1.OwnCloseBal + ca1.OwnerCloseBal + ca1.VPFCloseBal).ToString(),
                                                    //Fld29 = ca1.TDSAmount.ToString() != null ? ca1.TDSAmount.ToString() : "0",
                                                    //Fld30 = ca1.PensionAmount.ToString() != null ? ca1.PensionAmount.ToString() : "0",
                                                    //Fld31 = (ca1.OwnCloseBal + ca1.OwnerCloseBal + ca1.VPFCloseBal + ca1.TotalIntCloseBal - ca1.TDSAmount).ToString(),

                                                    //Fld32 = ca1.OwnPFLoan.ToString() != null ? ca1.OwnPFLoan.ToString() : "0",
                                                    //Fld33 = ca1.OwnIntPFLoan.ToString() != null ? ca1.OwnIntPFLoan.ToString() : "0",
                                                    //Fld34 = ca1.OwnerPFLoan.ToString() != null ? ca1.OwnerPFLoan.ToString() : "0",
                                                    //Fld35 = ca1.OwnerIntPFLoan.ToString() != null ? ca1.OwnerIntPFLoan.ToString() : "0",
                                                    //Fld36 = ca1.VPFPFLoan.ToString() != null ? ca1.VPFPFLoan.ToString() : "0",
                                                    //Fld37 = ca1.VPFIntPFLoan.ToString() != null ? ca1.VPFIntPFLoan.ToString() : "0",
                                                    //Fld38 = ca1.LoanAmountDebit.ToString() != null ? ca1.LoanAmountDebit.ToString() : "0",

                                                    //Fld39 = ca1.OwnIntCloseBal.ToString() != null ? ca1.OwnIntCloseBal.ToString() : "0",
                                                    //Fld40 = ca1.OwnerIntCloseBal.ToString() != null ? ca1.OwnerIntCloseBal.ToString() : "0",
                                                    //Fld41 = ca1.VPFIntCloseBal.ToString() != null ? ca1.VPFIntCloseBal.ToString() : "0",
                                                    //Fld42 = ca1.TotalIntCloseBal.ToString() != null ? ca1.TotalIntCloseBal.ToString() : "0",


                                                };

                                                //if (month)
                                                //{
                                                //    OGenericObjStatement.Fld100 = ca1.MonthYear.ToString();
                                                //}
                                                if (comp)
                                                {
                                                    OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                }
                                                if (dept)
                                                {
                                                    OGenericObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                }
                                                if (grp)
                                                {
                                                    OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                }
                                                if (grade)
                                                {
                                                    OGenericObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    OGenericObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    OGenericObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    OGenericObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                }
                                                if (emp)
                                                {
                                                    OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                }

                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                            }

                                        }

                                    }


                                }

                            }

                            return OGenericPayrollStatement;
                        }

                        break;

                    case "PASSBOOKEMPLOYEEWISE":
                        var OPFEmployeeLedboardemp = new List<EmployeePFTrust>();

                        foreach (var item in EmpPayrollIdList)
                        {
                            var OPFLedgData_t = db.EmployeePFTrust
                               .Include(e => e.Employee)
                                //.Include(e=>e.Employee.PerAddr)
                                //.Include(e=>e.Employee.ServiceBookDates)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.PFTEmployeeLedger)                               
                                // .Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.PayStruct)                              
                                 .Where(e => e.Employee_Id == item).AsNoTracking()
                                 .FirstOrDefault();

                            List<PFTEmployeeLedger> PFTEmpLedger = new List<PFTEmployeeLedger>();
                            if (OPFLedgData_t != null)
                            {
                                OPFLedgData_t.Employee.EmpName = db.NameSingle.Find(OPFLedgData_t.Employee.EmpName_Id);
                                OPFLedgData_t.Employee.EmpOffInfo = db.EmpOff.Find(OPFLedgData_t.Employee.EmpOffInfo_Id);
                                OPFLedgData_t.Employee.EmpOffInfo.NationalityID = db.NationalityID.Find(OPFLedgData_t.Employee.EmpOffInfo.NationalityID);
                                OPFLedgData_t.Employee.ServiceBookDates = db.ServiceBookDates.Find(OPFLedgData_t.Employee.ServiceBookDates_Id);
                                OPFLedgData_t.Employee.PerAddr = db.Address.Find(OPFLedgData_t.Employee.PerAddr_Id);
                                OPFLedgData_t.PFTEmployeeLedger = db.PFTEmployeeLedger.Include(e => e.PassbookActivity).Where(e => e.EmployeePFTrust_Id == OPFLedgData_t.Id && e.CalcDate >= pFromDate && e.CalcDate <= pToDate).ToList();
                                OPFEmployeeLedboardemp.Add(OPFLedgData_t);
                            }
                        }

                        if (OPFEmployeeLedboardemp == null || OPFEmployeeLedboardemp.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            //var month = false;
                            var emp = false;
                            var dept = false;
                            var loca = false;
                            var comp = false;
                            var grp = false;
                            var unit = false;
                            var div = false;
                            var regn = false;
                            var grade = false;
                            var lvl = false;
                            var jobstat = false;
                            var job = false;
                            var jobpos = false;
                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();


                            foreach (var item in vc)
                            {

                                //if (item.LookupVal.ToUpper() == "MONTH")
                                //{
                                //    month = true;
                                //}
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {

                                    loca = true;
                                }
                                if (item.LookupVal.ToUpper() == "EMPLOYEE")
                                {
                                    emp = true;
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                }
                                if (item.LookupVal.ToUpper() == "COMPANY")
                                {
                                    comp = true;
                                }
                                if (item.LookupVal.ToUpper() == "GROUP")
                                {
                                    grp = true;
                                }
                                if (item.LookupVal.ToUpper() == "UNIT")
                                {
                                    unit = true;
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                }
                                if (item.LookupVal.ToUpper() == "REGION")
                                {
                                    regn = true;
                                }
                                if (item.LookupVal.ToUpper() == "GRADE")
                                {
                                    grade = true;
                                }
                                if (item.LookupVal.ToUpper() == "LEVEL")
                                {
                                    lvl = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBSTATUS")
                                {
                                    jobstat = true;
                                }

                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBPOSITION")
                                {
                                    jobpos = true;
                                }
                            }


                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();
                            var PassbookLoanIDValue = new List<string>();
                            PassbookLoanIDValue.Add("LOAN DEBIT BALANCE");
                            PassbookLoanIDValue.Add("LOAN CREDIT BALANCE");
                            PassbookLoanIDValue.Add("INTEREST BALANCE");
                            PassbookLoanIDValue.Add("SETTLEMENT BALANCE");
                            PassbookLoanIDValue.Add("PF BALANCE");

                            List<int> PassbookLoanID = new List<int>();
                            PassbookLoanID = db.LookupValue.Where(e => PassbookLoanIDValue.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();

                            foreach (var ca in OPFEmployeeLedboardemp)
                            {
                                if (ca.PFTEmployeeLedger != null && ca.PFTEmployeeLedger.Count() != 0)
                                {
                                    var OPFTEMPLEDData = ca.PFTEmployeeLedger.Where(e => e.CalcDate >= pFromDate && e.CalcDate <= pToDate && PassbookLoanID.Contains(e.PassbookActivity.Id) == true).OrderBy(e => e.Id).ToList();

                                    if (OPFTEMPLEDData != null && OPFTEMPLEDData.Count() != 0)
                                    {
                                        int? geoid = ca.Employee.GeoStruct_Id;

                                        int? payid = ca.Employee.PayStruct_Id;

                                        int? funid = ca.Employee.FuncStruct_Id;

                                        GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                        PayStruct paystruct = db.PayStruct.Find(payid);

                                        FuncStruct funstruct = db.FuncStruct.Find(funid);

                                        GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                        if (GeoDataInd != null)
                                        {



                                            foreach (var ca1 in OPFTEMPLEDData)
                                            {
                                                string Activity = "";
                                                double ownpf = 0;
                                                double ownerpf = 0;
                                                double VPF = 0;
                                                double ownpfint = 0;
                                                double ownerpfint = 0;
                                                double VPFint = 0;
                                                double totalLoanAmount = 0;
                                                double totalPFbalance = 0;
                                                double totalIntbalance = 0;
                                                double TDSbal = 0;
                                                double Finalbalance = 0;
                                                double Pension = 0;

                                                if (ca1.PassbookActivity.LookupVal.ToUpper() == "PF BALANCE" && ca1.Narration == "Opening Balance")
                                                {
                                                    Activity = "Opening Balance";
                                                    ownpf = ca1.OwnOpenBal;
                                                    ownerpf = ca1.OwnerOpenBal;
                                                    VPF = ca1.VPFIntOpenBal;
                                                    ownpfint = ca1.OwnIntOpenBal;
                                                    ownerpfint = ca1.OwnerIntOpenBal;
                                                    VPFint = ca1.VPFIntOpenBal;
                                                    totalPFbalance = ca1.OwnOpenBal + ca1.OwnerOpenBal + ca1.VPFOpenBal;
                                                    totalIntbalance = ca1.TotalIntOpenBal;
                                                    TDSbal = ca1.TDSAmount;
                                                    Finalbalance = (ca1.OwnOpenBal + ca1.OwnerOpenBal + ca1.VPFOpenBal + ca1.TotalIntOpenBal - ca1.TDSAmount);
                                                }
                                                else if (ca1.PassbookActivity.LookupVal.ToUpper() == "PF BALANCE" || ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN DEBIT BALANCE" || ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN CREDIT BALANCE" || ca1.PassbookActivity.LookupVal.ToUpper() == "INTEREST BALANCE" || ca1.PassbookActivity.LookupVal.ToUpper() == "SETTLEMENT BALANCE")
                                                {
                                                    if (ca1.Narration != "Opening Balance")
                                                    {
                                                        Activity = "Closing Balance";
                                                        ownpf = ca1.OwnCloseBal;
                                                        ownerpf = ca1.OwnerCloseBal;
                                                        VPF = ca1.VPFCloseBal;
                                                        ownpfint = ca1.OwnIntCloseBal;
                                                        ownerpfint = ca1.OwnerIntCloseBal;
                                                        VPFint = ca1.VPFIntCloseBal;
                                                        totalPFbalance = ca1.OwnCloseBal + ca1.OwnerCloseBal + ca1.VPFCloseBal;
                                                        totalIntbalance = ca1.TotalIntCloseBal;
                                                        TDSbal = ca1.TDSAmount;
                                                        Finalbalance = (ca1.OwnCloseBal + ca1.OwnerCloseBal + ca1.VPFCloseBal + ca1.TotalIntCloseBal - ca1.TDSAmount);
                                                    }

                                                }
                                                else if (ca1.PassbookActivity.LookupVal.ToUpper() == "MONTHLY PF POSTING" || ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN DEBIT POSTING" || ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN CREDIT POSTING" || ca1.PassbookActivity.LookupVal.ToUpper() == "INTEREST POSTING" || ca1.PassbookActivity.LookupVal.ToUpper() == "SETTLEMENT POSTING")
                                                {
                                                    Activity = "Transaction";
                                                    if (ca1.PassbookActivity.LookupVal.ToUpper() == "MONTHLY PF POSTING")
                                                    {
                                                        ownpf = ca1.OwnPFMonthly;
                                                        ownerpf = ca1.OwnerPFMonthly;
                                                        VPF = ca1.VPFAmountMonthly;
                                                        TDSbal = ca1.TDSAmount;
                                                        Pension = ca1.PensionAmount;
                                                    }
                                                    if (ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN DEBIT POSTING")
                                                    {
                                                        ownpf = ca1.OwnPFLoan;
                                                        ownerpf = ca1.OwnerPFLoan;
                                                        VPF = ca1.VPFPFLoan;
                                                        ownpfint = ca1.OwnIntPFLoan;
                                                        ownerpfint = ca1.OwnerIntPFLoan;
                                                        VPFint = ca1.VPFIntPFLoan;
                                                        totalLoanAmount = ca1.LoanAmountDebit;
                                                        TDSbal = ca1.TDSAmount;
                                                    }
                                                    if (ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN CREDIT POSTING")
                                                    {

                                                    }
                                                    if (ca1.PassbookActivity.LookupVal.ToUpper() == "INTEREST POSTING" || ca1.PassbookActivity.LookupVal.ToUpper() == "SETTLEMENT POSTING")
                                                    {
                                                        ownpfint = ca1.OwnPFInt;
                                                        ownerpfint = ca1.OwnerPFInt;
                                                        VPFint = ca1.VPFInt;
                                                        TDSbal = ca1.TDSAmount;
                                                    }

                                                }



                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                {


                                                    Fld2 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                                    Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                    Fld4 = GeoDataInd.FuncStruct_Job_Name,
                                                    Fld5 = ca1.PassbookActivity != null ? ca1.PassbookActivity.LookupVal : "",
                                                    Fld6 = GeoDataInd.GeoStruct_Location_Name,
                                                    Fld7 = ca.Employee.PerAddr != null ? ca.Employee.PerAddr.FullAddress : "",
                                                    Fld8 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.JoiningDate != null ? ca.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : "",
                                                    Fld9 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.RetirementDate != null ? ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString() : "",
                                                    Fld10 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.BirthDate != null ? ca.Employee.ServiceBookDates.BirthDate.Value.ToShortDateString() : "",
                                                    Fld11 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.PFJoingDate != null ? ca.Employee.ServiceBookDates.PFJoingDate.Value.ToShortDateString() : "",
                                                    Fld12 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.PensionJoingDate != null ? ca.Employee.ServiceBookDates.PensionJoingDate.Value.ToShortDateString() : "",
                                                    Fld13 = ca.Employee.EmpOffInfo != null && ca.Employee.EmpOffInfo.NationalityID != null ? ca.Employee.EmpOffInfo.NationalityID.PensionNo.ToString() : "",

                                                    Fld14 = ca1.MonthYear == null ? "" : ca1.MonthYear.ToString(),
                                                    Fld15 = ca1.Id.ToString(),
                                                    Fld43 = Activity.ToString(),
                                                    Fld16 = ownpf.ToString(),
                                                    Fld17 = ownerpf.ToString(),
                                                    Fld18 = VPF.ToString(),
                                                    Fld19 = ownpfint.ToString(),
                                                    Fld20 = ownerpfint.ToString(),
                                                    Fld21 = VPFint.ToString(),
                                                    Fld22 = ca1.PassbookActivity.LookupVal.ToUpper(),
                                                    Fld23 = totalLoanAmount.ToString(),
                                                    Fld24 = totalPFbalance.ToString(),
                                                    Fld25 = totalIntbalance.ToString(),
                                                    Fld26 = TDSbal.ToString(),
                                                    Fld27 = Finalbalance.ToString(),
                                                    Fld28 = Pension.ToString(),
                                                    Fld29 = ca1.CalcDate.ToShortDateString(),



                                                };

                                                //if (month)
                                                //{
                                                //    OGenericObjStatement.Fld100 = ca1.MonthYear.ToString();
                                                //}
                                                if (comp)
                                                {
                                                    OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                }
                                                if (dept)
                                                {
                                                    OGenericObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                }
                                                if (grp)
                                                {
                                                    OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                }
                                                if (grade)
                                                {
                                                    OGenericObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    OGenericObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    OGenericObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    OGenericObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                }
                                                if (emp)
                                                {
                                                    OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                }

                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                            }
                                        }

                                    }




                                }

                            }

                            return OGenericPayrollStatement;
                        }

                        break;
                    case "PASSBOOKMONTHWISE":

                        var OPFEmployeeLedboard = new List<EmployeePFTrust>();

                        foreach (var item in EmpPayrollIdList)
                        {
                            var OPFLedgData_t = db.EmployeePFTrust
                               .Include(e => e.Employee)
                                //.Include(e=>e.Employee.PerAddr)
                                //.Include(e=>e.Employee.ServiceBookDates)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.PFTEmployeeLedger)                               
                                // .Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.PayStruct)                              
                                 .Where(e => e.Employee_Id == item).AsNoTracking()
                                 .FirstOrDefault();

                            List<PFTEmployeeLedger> PFTEmpLedger = new List<PFTEmployeeLedger>();
                            if (OPFLedgData_t != null)
                            {
                                OPFLedgData_t.Employee.EmpName = db.NameSingle.Find(OPFLedgData_t.Employee.EmpName_Id);
                                OPFLedgData_t.Employee.EmpOffInfo = db.EmpOff.Find(OPFLedgData_t.Employee.EmpOffInfo_Id);
                                OPFLedgData_t.Employee.EmpOffInfo.NationalityID = db.NationalityID.Find(OPFLedgData_t.Employee.EmpOffInfo.NationalityID);
                                OPFLedgData_t.Employee.ServiceBookDates = db.ServiceBookDates.Find(OPFLedgData_t.Employee.ServiceBookDates_Id);
                                OPFLedgData_t.Employee.PerAddr = db.Address.Find(OPFLedgData_t.Employee.PerAddr_Id);
                                OPFLedgData_t.PFTEmployeeLedger = db.PFTEmployeeLedger.Include(e => e.PassbookActivity).Where(e => e.EmployeePFTrust_Id == OPFLedgData_t.Id && e.CalcDate >= pFromDate && e.CalcDate <= pToDate).ToList();
                                OPFEmployeeLedboard.Add(OPFLedgData_t);
                            }
                        }

                        if (OPFEmployeeLedboard == null || OPFEmployeeLedboard.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            //var month = false;
                            var emp = false;
                            var dept = false;
                            var loca = false;
                            var comp = false;
                            var grp = false;
                            var unit = false;
                            var div = false;
                            var regn = false;
                            var grade = false;
                            var lvl = false;
                            var jobstat = false;
                            var job = false;
                            var jobpos = false;
                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();


                            foreach (var item in vc)
                            {

                                //if (item.LookupVal.ToUpper() == "MONTH")
                                //{
                                //    month = true;
                                //}
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {

                                    loca = true;
                                }
                                if (item.LookupVal.ToUpper() == "EMPLOYEE")
                                {
                                    emp = true;
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                }
                                if (item.LookupVal.ToUpper() == "COMPANY")
                                {
                                    comp = true;
                                }
                                if (item.LookupVal.ToUpper() == "GROUP")
                                {
                                    grp = true;
                                }
                                if (item.LookupVal.ToUpper() == "UNIT")
                                {
                                    unit = true;
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                }
                                if (item.LookupVal.ToUpper() == "REGION")
                                {
                                    regn = true;
                                }
                                if (item.LookupVal.ToUpper() == "GRADE")
                                {
                                    grade = true;
                                }
                                if (item.LookupVal.ToUpper() == "LEVEL")
                                {
                                    lvl = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBSTATUS")
                                {
                                    jobstat = true;
                                }

                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBPOSITION")
                                {
                                    jobpos = true;
                                }
                            }


                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();
                            var PassbookLoanIDValue = new List<string>();
                            PassbookLoanIDValue.Add("LOAN DEBIT BALANCE");
                            PassbookLoanIDValue.Add("LOAN CREDIT BALANCE");
                            PassbookLoanIDValue.Add("INTEREST BALANCE");
                            PassbookLoanIDValue.Add("SETTLEMENT BALANCE");
                            PassbookLoanIDValue.Add("PF BALANCE");

                            List<int> PassbookLoanID = new List<int>();
                            PassbookLoanID = db.LookupValue.Where(e => PassbookLoanIDValue.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();

                            foreach (var ca in OPFEmployeeLedboard)
                            {
                                if (ca.PFTEmployeeLedger != null && ca.PFTEmployeeLedger.Count() != 0)
                                {
                                    var OPFTEMPLEDData = ca.PFTEmployeeLedger.Where(e => e.CalcDate >= pFromDate && e.CalcDate <= pToDate && PassbookLoanID.Contains(e.PassbookActivity.Id) == true).OrderBy(e => e.Id).ToList();

                                    if (OPFTEMPLEDData != null && OPFTEMPLEDData.Count() != 0)
                                    {
                                        int? geoid = ca.Employee.GeoStruct_Id;

                                        int? payid = ca.Employee.PayStruct_Id;

                                        int? funid = ca.Employee.FuncStruct_Id;

                                        GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                        PayStruct paystruct = db.PayStruct.Find(payid);

                                        FuncStruct funstruct = db.FuncStruct.Find(funid);

                                        GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                        if (GeoDataInd != null)
                                        {

                                            if (salheadlist != null && salheadlist.Count() != 0)
                                            {
                                                foreach (var item2 in salheadlist)
                                                {
                                                    var OPFTEMPLEDDataind = OPFTEMPLEDData.Where(e => e.PassbookActivity.LookupVal.ToUpper() == item2.ToUpper()).ToList();
                                                    foreach (var ca1 in OPFTEMPLEDDataind)
                                                    {
                                                        string Activity = "";
                                                        double ownpf = 0;
                                                        double ownerpf = 0;
                                                        double VPF = 0;
                                                        double ownpfint = 0;
                                                        double ownerpfint = 0;
                                                        double VPFint = 0;
                                                        double totalLoanAmount = 0;
                                                        double totalPFbalance = 0;
                                                        double totalIntbalance = 0;
                                                        double TDSbal = 0;
                                                        double Finalbalance = 0;
                                                        double Pension = 0;

                                                        if (ca1.PassbookActivity.LookupVal.ToUpper() == "PF BALANCE" && ca1.Narration == "Opening Balance")
                                                        {
                                                            Activity = "Opening Balance";
                                                            ownpf = ca1.OwnOpenBal;
                                                            ownerpf = ca1.OwnerOpenBal;
                                                            VPF = ca1.VPFIntOpenBal;
                                                            ownpfint = ca1.OwnIntOpenBal;
                                                            ownerpfint = ca1.OwnerIntOpenBal;
                                                            VPFint = ca1.VPFIntOpenBal;
                                                            totalPFbalance = ca1.OwnOpenBal + ca1.OwnerOpenBal + ca1.VPFOpenBal;
                                                            totalIntbalance = ca1.TotalIntOpenBal;
                                                            TDSbal = ca1.TDSAmount;
                                                            Finalbalance = (ca1.OwnOpenBal + ca1.OwnerOpenBal + ca1.VPFOpenBal + ca1.TotalIntOpenBal - ca1.TDSAmount);
                                                        }
                                                        else if (ca1.PassbookActivity.LookupVal.ToUpper() == "PF BALANCE" || ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN DEBIT BALANCE" || ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN CREDIT BALANCE" || ca1.PassbookActivity.LookupVal.ToUpper() == "INTEREST BALANCE" || ca1.PassbookActivity.LookupVal.ToUpper() == "SETTLEMENT BALANCE")
                                                        {
                                                            if (ca1.Narration != "Opening Balance")
                                                            {
                                                                Activity = "Closing Balance";
                                                                ownpf = ca1.OwnCloseBal;
                                                                ownerpf = ca1.OwnerCloseBal;
                                                                VPF = ca1.VPFCloseBal;
                                                                ownpfint = ca1.OwnIntCloseBal;
                                                                ownerpfint = ca1.OwnerIntCloseBal;
                                                                VPFint = ca1.VPFIntCloseBal;
                                                                totalPFbalance = ca1.OwnCloseBal + ca1.OwnerCloseBal + ca1.VPFCloseBal;
                                                                totalIntbalance = ca1.TotalIntCloseBal;
                                                                TDSbal = ca1.TDSAmount;
                                                                Finalbalance = (ca1.OwnCloseBal + ca1.OwnerCloseBal + ca1.VPFCloseBal + ca1.TotalIntCloseBal - ca1.TDSAmount);
                                                            }

                                                        }
                                                        else if (ca1.PassbookActivity.LookupVal.ToUpper() == "MONTHLY PF POSTING" || ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN DEBIT POSTING" || ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN CREDIT POSTING" || ca1.PassbookActivity.LookupVal.ToUpper() == "INTEREST POSTING" || ca1.PassbookActivity.LookupVal.ToUpper() == "SETTLEMENT POSTING")
                                                        {
                                                            Activity = "Transaction";
                                                            if (ca1.PassbookActivity.LookupVal.ToUpper() == "MONTHLY PF POSTING")
                                                            {
                                                                ownpf = ca1.OwnPFMonthly;
                                                                ownerpf = ca1.OwnerPFMonthly;
                                                                VPF = ca1.VPFAmountMonthly;
                                                                TDSbal = ca1.TDSAmount;
                                                                Pension = ca1.PensionAmount;
                                                            }
                                                            if (ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN DEBIT POSTING")
                                                            {
                                                                ownpf = ca1.OwnPFLoan;
                                                                ownerpf = ca1.OwnerPFLoan;
                                                                VPF = ca1.VPFPFLoan;
                                                                ownpfint = ca1.OwnIntPFLoan;
                                                                ownerpfint = ca1.OwnerIntPFLoan;
                                                                VPFint = ca1.VPFIntPFLoan;
                                                                totalLoanAmount = ca1.LoanAmountDebit;
                                                                TDSbal = ca1.TDSAmount;
                                                            }
                                                            if (ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN CREDIT POSTING")
                                                            {

                                                            }
                                                            if (ca1.PassbookActivity.LookupVal.ToUpper() == "INTEREST POSTING" || ca1.PassbookActivity.LookupVal.ToUpper() == "SETTLEMENT POSTING")
                                                            {
                                                                ownpfint = ca1.OwnPFInt;
                                                                ownerpfint = ca1.OwnerPFInt;
                                                                VPFint = ca1.VPFInt;
                                                                TDSbal = ca1.TDSAmount;
                                                            }

                                                        }



                                                        GenericField100 OGenericObjStatement = new GenericField100()
                                                        {


                                                            Fld2 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                                            Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                            Fld4 = GeoDataInd.FuncStruct_Job_Name,
                                                            Fld5 = ca1.PassbookActivity != null ? ca1.PassbookActivity.LookupVal : "",
                                                            Fld6 = GeoDataInd.GeoStruct_Location_Name,
                                                            Fld7 = ca.Employee.PerAddr != null ? ca.Employee.PerAddr.FullAddress : "",
                                                            Fld8 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.JoiningDate != null ? ca.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : "",
                                                            Fld9 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.RetirementDate != null ? ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString() : "",
                                                            Fld10 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.BirthDate != null ? ca.Employee.ServiceBookDates.BirthDate.Value.ToShortDateString() : "",
                                                            Fld11 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.PFJoingDate != null ? ca.Employee.ServiceBookDates.PFJoingDate.Value.ToShortDateString() : "",
                                                            Fld12 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.PensionJoingDate != null ? ca.Employee.ServiceBookDates.PensionJoingDate.Value.ToShortDateString() : "",
                                                            Fld13 = ca.Employee.EmpOffInfo != null && ca.Employee.EmpOffInfo.NationalityID != null ? ca.Employee.EmpOffInfo.NationalityID.PensionNo.ToString() : "",

                                                            Fld14 = ca1.MonthYear == null ? "" : ca1.MonthYear.ToString(),
                                                            Fld15 = ca1.Id.ToString(),
                                                            Fld43 = Activity.ToString(),
                                                            Fld16 = ownpf.ToString(),
                                                            Fld17 = ownerpf.ToString(),
                                                            Fld18 = VPF.ToString(),
                                                            Fld19 = ownpfint.ToString(),
                                                            Fld20 = ownerpfint.ToString(),
                                                            Fld21 = VPFint.ToString(),
                                                            Fld22 = ca1.PassbookActivity.LookupVal.ToUpper(),
                                                            Fld23 = totalLoanAmount.ToString(),
                                                            Fld24 = totalPFbalance.ToString(),
                                                            Fld25 = totalIntbalance.ToString(),
                                                            Fld26 = TDSbal.ToString(),
                                                            Fld27 = Finalbalance.ToString(),
                                                            Fld28 = Pension.ToString(),
                                                            Fld29 = ca1.CalcDate.ToShortDateString(),



                                                        };

                                                        //if (month)
                                                        //{
                                                        //    OGenericObjStatement.Fld100 = ca1.MonthYear.ToString();
                                                        //}
                                                        if (comp)
                                                        {
                                                            OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                        }
                                                        if (div)
                                                        {
                                                            OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                        }
                                                        if (loca)
                                                        {
                                                            OGenericObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                        }
                                                        if (dept)
                                                        {
                                                            OGenericObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                        }
                                                        if (grp)
                                                        {
                                                            OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                        }
                                                        if (unit)
                                                        {
                                                            OGenericObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                        }
                                                        if (grade)
                                                        {
                                                            OGenericObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                        }
                                                        if (lvl)
                                                        {
                                                            OGenericObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                        }
                                                        if (jobstat)
                                                        {
                                                            OGenericObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                        }
                                                        if (job)
                                                        {
                                                            OGenericObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                        }
                                                        if (jobpos)
                                                        {
                                                            OGenericObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                        }
                                                        if (emp)
                                                        {
                                                            OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                        }

                                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                foreach (var ca1 in OPFTEMPLEDData)
                                                {
                                                    string Activity = "";
                                                    double ownpf = 0;
                                                    double ownerpf = 0;
                                                    double VPF = 0;
                                                    double ownpfint = 0;
                                                    double ownerpfint = 0;
                                                    double VPFint = 0;
                                                    double totalLoanAmount = 0;
                                                    double totalPFbalance = 0;
                                                    double totalIntbalance = 0;
                                                    double TDSbal = 0;
                                                    double Finalbalance = 0;
                                                    double Pension = 0;

                                                    if (ca1.PassbookActivity.LookupVal.ToUpper() == "PF BALANCE" && ca1.Narration == "Opening Balance")
                                                    {
                                                        Activity = "Opening Balance";
                                                        ownpf = ca1.OwnOpenBal;
                                                        ownerpf = ca1.OwnerOpenBal;
                                                        VPF = ca1.VPFIntOpenBal;
                                                        ownpfint = ca1.OwnIntOpenBal;
                                                        ownerpfint = ca1.OwnerIntOpenBal;
                                                        VPFint = ca1.VPFIntOpenBal;
                                                        totalPFbalance = ca1.OwnOpenBal + ca1.OwnerOpenBal + ca1.VPFOpenBal;
                                                        totalIntbalance = ca1.TotalIntOpenBal;
                                                        TDSbal = ca1.TDSAmount;
                                                        Finalbalance = (ca1.OwnOpenBal + ca1.OwnerOpenBal + ca1.VPFOpenBal + ca1.TotalIntOpenBal - ca1.TDSAmount);
                                                    }
                                                    else if (ca1.PassbookActivity.LookupVal.ToUpper() == "PF BALANCE" || ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN DEBIT BALANCE" || ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN CREDIT BALANCE" || ca1.PassbookActivity.LookupVal.ToUpper() == "INTEREST BALANCE" || ca1.PassbookActivity.LookupVal.ToUpper() == "SETTLEMENT BALANCE")
                                                    {
                                                        if (ca1.Narration != "Opening Balance")
                                                        {
                                                            Activity = "Closing Balance";
                                                            ownpf = ca1.OwnCloseBal;
                                                            ownerpf = ca1.OwnerCloseBal;
                                                            VPF = ca1.VPFCloseBal;
                                                            ownpfint = ca1.OwnIntCloseBal;
                                                            ownerpfint = ca1.OwnerIntCloseBal;
                                                            VPFint = ca1.VPFIntCloseBal;
                                                            totalPFbalance = ca1.OwnCloseBal + ca1.OwnerCloseBal + ca1.VPFCloseBal;
                                                            totalIntbalance = ca1.TotalIntCloseBal;
                                                            TDSbal = ca1.TDSAmount;
                                                            Finalbalance = (ca1.OwnCloseBal + ca1.OwnerCloseBal + ca1.VPFCloseBal + ca1.TotalIntCloseBal - ca1.TDSAmount);
                                                        }

                                                    }
                                                    else if (ca1.PassbookActivity.LookupVal.ToUpper() == "MONTHLY PF POSTING" || ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN DEBIT POSTING" || ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN CREDIT POSTING" || ca1.PassbookActivity.LookupVal.ToUpper() == "INTEREST POSTING" || ca1.PassbookActivity.LookupVal.ToUpper() == "SETTLEMENT POSTING")
                                                    {
                                                        Activity = "Transaction";
                                                        if (ca1.PassbookActivity.LookupVal.ToUpper() == "MONTHLY PF POSTING")
                                                        {
                                                            ownpf = ca1.OwnPFMonthly;
                                                            ownerpf = ca1.OwnerPFMonthly;
                                                            VPF = ca1.VPFAmountMonthly;
                                                            TDSbal = ca1.TDSAmount;
                                                            Pension = ca1.PensionAmount;
                                                        }
                                                        if (ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN DEBIT POSTING")
                                                        {
                                                            ownpf = ca1.OwnPFLoan;
                                                            ownerpf = ca1.OwnerPFLoan;
                                                            VPF = ca1.VPFPFLoan;
                                                            ownpfint = ca1.OwnIntPFLoan;
                                                            ownerpfint = ca1.OwnerIntPFLoan;
                                                            VPFint = ca1.VPFIntPFLoan;
                                                            totalLoanAmount = ca1.LoanAmountDebit;
                                                            TDSbal = ca1.TDSAmount;
                                                        }
                                                        if (ca1.PassbookActivity.LookupVal.ToUpper() == "LOAN CREDIT POSTING")
                                                        {

                                                        }
                                                        if (ca1.PassbookActivity.LookupVal.ToUpper() == "INTEREST POSTING" || ca1.PassbookActivity.LookupVal.ToUpper() == "SETTLEMENT POSTING")
                                                        {
                                                            ownpfint = ca1.OwnPFInt;
                                                            ownerpfint = ca1.OwnerPFInt;
                                                            VPFint = ca1.VPFInt;
                                                            TDSbal = ca1.TDSAmount;
                                                        }

                                                    }



                                                    GenericField100 OGenericObjStatement = new GenericField100()
                                                    {


                                                        Fld2 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                                        Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                        Fld4 = GeoDataInd.FuncStruct_Job_Name,
                                                        Fld5 = ca1.PassbookActivity != null ? ca1.PassbookActivity.LookupVal : "",
                                                        Fld6 = GeoDataInd.GeoStruct_Location_Name,
                                                        Fld7 = ca.Employee.PerAddr != null ? ca.Employee.PerAddr.FullAddress : "",
                                                        Fld8 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.JoiningDate != null ? ca.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : "",
                                                        Fld9 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.RetirementDate != null ? ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString() : "",
                                                        Fld10 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.BirthDate != null ? ca.Employee.ServiceBookDates.BirthDate.Value.ToShortDateString() : "",
                                                        Fld11 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.PFJoingDate != null ? ca.Employee.ServiceBookDates.PFJoingDate.Value.ToShortDateString() : "",
                                                        Fld12 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.PensionJoingDate != null ? ca.Employee.ServiceBookDates.PensionJoingDate.Value.ToShortDateString() : "",
                                                        Fld13 = ca.Employee.EmpOffInfo != null && ca.Employee.EmpOffInfo.NationalityID != null ? ca.Employee.EmpOffInfo.NationalityID.PensionNo.ToString() : "",

                                                        Fld14 = ca1.MonthYear == null ? "" : ca1.MonthYear.ToString(),
                                                        Fld15 = ca1.Id.ToString(),
                                                        Fld43 = Activity.ToString(),
                                                        Fld16 = ownpf.ToString(),
                                                        Fld17 = ownerpf.ToString(),
                                                        Fld18 = VPF.ToString(),
                                                        Fld19 = ownpfint.ToString(),
                                                        Fld20 = ownerpfint.ToString(),
                                                        Fld21 = VPFint.ToString(),
                                                        Fld22 = ca1.PassbookActivity.LookupVal.ToUpper(),
                                                        Fld23 = totalLoanAmount.ToString(),
                                                        Fld24 = totalPFbalance.ToString(),
                                                        Fld25 = totalIntbalance.ToString(),
                                                        Fld26 = TDSbal.ToString(),
                                                        Fld27 = Finalbalance.ToString(),
                                                        Fld28 = Pension.ToString(),
                                                        Fld29 = ca1.CalcDate.ToShortDateString(),



                                                    };

                                                    //if (month)
                                                    //{
                                                    //    OGenericObjStatement.Fld100 = ca1.MonthYear.ToString();
                                                    //}
                                                    if (comp)
                                                    {
                                                        OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                    }
                                                    if (div)
                                                    {
                                                        OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                    }
                                                    if (loca)
                                                    {
                                                        OGenericObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                    }
                                                    if (dept)
                                                    {
                                                        OGenericObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                    }
                                                    if (grp)
                                                    {
                                                        OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                    }
                                                    if (unit)
                                                    {
                                                        OGenericObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                    }
                                                    if (grade)
                                                    {
                                                        OGenericObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                    }
                                                    if (lvl)
                                                    {
                                                        OGenericObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                    }
                                                    if (jobstat)
                                                    {
                                                        OGenericObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                    }
                                                    if (job)
                                                    {
                                                        OGenericObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                    }
                                                    if (jobpos)
                                                    {
                                                        OGenericObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                    }
                                                    if (emp)
                                                    {
                                                        OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                    }

                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                                }
                                            }

                                        }

                                    }


                                }

                            }

                            return OGenericPayrollStatement;
                        }

                        break;

                    case "SETTELMENTPROPOSAL":

                        var OPFEmployeeLedDetails = new List<EmployeePFTrust>();

                        foreach (var item in EmpPayrollIdList)
                        {
                            var OPFLedgData_t = db.EmployeePFTrust
                               .Include(e => e.Employee)
                                //.Include(e=>e.Employee.PerAddr)
                                //.Include(e=>e.Employee.ServiceBookDates)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.PFTEmployeeLedger)                               
                                // .Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.PayStruct)                              
                                 .Where(e => e.Employee_Id == item).AsNoTracking()
                                 .FirstOrDefault();

                            List<EmpSettlementPFT> PFTEmpLedger = new List<EmpSettlementPFT>();
                            if (OPFLedgData_t != null)
                            {
                                OPFLedgData_t.Employee.EmpName = db.NameSingle.Find(OPFLedgData_t.Employee.EmpName_Id);
                                OPFLedgData_t.Employee.EmpOffInfo = db.EmpOff.Find(OPFLedgData_t.Employee.EmpOffInfo_Id);
                                OPFLedgData_t.Employee.EmpOffInfo.NationalityID = db.NationalityID.Find(OPFLedgData_t.Employee.EmpOffInfo.NationalityID);
                                OPFLedgData_t.Employee.EmpOffInfo.Branch = db.Branch.Find(OPFLedgData_t.Employee.EmpOffInfo.Branch_Id);
                                OPFLedgData_t.Employee.ServiceBookDates = db.ServiceBookDates.Find(OPFLedgData_t.Employee.ServiceBookDates_Id);
                                OPFLedgData_t.Employee.PerAddr = db.Address.Find(OPFLedgData_t.Employee.PerAddr_Id);
                                OPFLedgData_t.EmpSettlementPFT = db.EmpSettlementPFT.Include(e => e.PassbookActivity).Where(e => e.EmployeePFTrust_Id == OPFLedgData_t.Id).ToList();
                                OPFEmployeeLedDetails.Add(OPFLedgData_t);
                            }
                        }

                        if (OPFEmployeeLedDetails == null || OPFEmployeeLedDetails.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            //var month = false;
                            var emp = false;
                            var dept = false;
                            var loca = false;
                            var comp = false;
                            var grp = false;
                            var unit = false;
                            var div = false;
                            var regn = false;
                            var grade = false;
                            var lvl = false;
                            var jobstat = false;
                            var job = false;
                            var jobpos = false;
                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();


                            foreach (var item in vc)
                            {

                                //if (item.LookupVal.ToUpper() == "MONTH")
                                //{
                                //    month = true;
                                //}
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {

                                    loca = true;
                                }
                                if (item.LookupVal.ToUpper() == "EMPLOYEE")
                                {
                                    emp = true;
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                }
                                if (item.LookupVal.ToUpper() == "COMPANY")
                                {
                                    comp = true;
                                }
                                if (item.LookupVal.ToUpper() == "GROUP")
                                {
                                    grp = true;
                                }
                                if (item.LookupVal.ToUpper() == "UNIT")
                                {
                                    unit = true;
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                }
                                if (item.LookupVal.ToUpper() == "REGION")
                                {
                                    regn = true;
                                }
                                if (item.LookupVal.ToUpper() == "GRADE")
                                {
                                    grade = true;
                                }
                                if (item.LookupVal.ToUpper() == "LEVEL")
                                {
                                    lvl = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBSTATUS")
                                {
                                    jobstat = true;
                                }

                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBPOSITION")
                                {
                                    jobpos = true;
                                }
                            }


                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in OPFEmployeeLedDetails)
                            {
                                if (ca.EmpSettlementPFT != null && ca.EmpSettlementPFT.Count() != 0)
                                {
                                    var PassbookLoanIDValue = new List<string>();
                                    PassbookLoanIDValue.Add("SETTLEMENT BALANCE");
                                    List<int> PassbookLoanID = new List<int>();
                                    PassbookLoanID = db.LookupValue.Where(e => PassbookLoanIDValue.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();
                                    var OPFTEMPLEDData = ca.EmpSettlementPFT.Where(e => e.IsPaymentLock == true && e.CalcDate >= pFromDate && e.CalcDate <= pToDate && PassbookLoanID.Contains(e.PassbookActivity.Id) == true).OrderBy(e => e.Id).ToList();

                                    //   var OPFTEMPLEDData = ca.PFTEmployeeLedger.Where(e => e.PostingDate >= pFromDate && e.PostingDate <= pToDate).ToList();

                                    if (OPFTEMPLEDData != null && OPFTEMPLEDData.Count() != 0)
                                    {
                                        int? geoid = ca.Employee.GeoStruct_Id;

                                        int? payid = ca.Employee.PayStruct_Id;

                                        int? funid = ca.Employee.FuncStruct_Id;

                                        GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                        PayStruct paystruct = db.PayStruct.Find(payid);

                                        FuncStruct funstruct = db.FuncStruct.Find(funid);

                                        GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                        if (GeoDataInd != null)
                                        {


                                            foreach (var ca1 in OPFTEMPLEDData)
                                            {
                                                double totamt = (ca1.OwnCloseBal + ca1.OwnIntCloseBal + ca1.OwnerCloseBal + ca1.OwnerIntCloseBal + ca1.VPFCloseBal + ca1.VPFIntCloseBal);
                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                {


                                                    Fld2 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                                    Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                    Fld4 = GeoDataInd.GeoStruct_Location_Name,
                                                    Fld5 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.JoiningDate != null ? ca.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : "",
                                                    Fld6 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.RetirementDate != null ? ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString() : "",
                                                    Fld7 = GeoDataInd.FuncStruct_Job_Name,
                                                    Fld8 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.PFJoingDate != null ? ca.Employee.ServiceBookDates.PFJoingDate.Value.ToShortDateString() : "",
                                                    Fld9 = ca1.OwnCloseBal.ToString(),
                                                    Fld10 = ca1.OwnIntCloseBal.ToString(),
                                                    Fld11 = ca1.OwnerCloseBal.ToString(),
                                                    Fld12 = ca1.OwnerIntCloseBal.ToString(),
                                                    Fld13 = ca1.VPFCloseBal.ToString(),
                                                    Fld14 = ca1.VPFIntCloseBal.ToString(),
                                                    Fld15 = (ca1.OwnCloseBal + ca1.OwnIntCloseBal + ca1.OwnerCloseBal + ca1.OwnerIntCloseBal + ca1.VPFCloseBal + ca1.VPFIntCloseBal).ToString(),
                                                    // Fld16 = ca1.OwnCloseBal.ToString() != null ? ca1.OwnCloseBal.ToString() : "0",
                                                    Fld17 = (ca1.OwnCloseBal + ca1.OwnIntCloseBal).ToString(),
                                                    Fld18 = (ca1.OwnerCloseBal + ca1.OwnerIntCloseBal).ToString(),
                                                    Fld19 = (ca1.VPFCloseBal + ca1.VPFIntCloseBal).ToString(),
                                                    Fld20 = (ca1.OwnCloseBal + ca1.OwnIntCloseBal + ca1.VPFCloseBal + ca1.VPFIntCloseBal).ToString(),
                                                    Fld16 = ca1.Actualtax.ToString(),
                                                    // Fld21 = (ca1.OwnerCloseBal + ca1.OwnerIntCloseBal).ToString(),
                                                    Fld22 = ca.Employee.EmpOffInfo != null && ca.Employee.EmpOffInfo.AccountNo != null ? ca.Employee.EmpOffInfo.AccountNo.ToString() : "",
                                                    Fld23 = NumToWords.ConvertAmount(totamt),
                                                    Fld24 = ca1.Cheque_no.ToString(),
                                                    Fld25 = ca1.ChequeIssueDate.ToShortDateString(),
                                                    Fld26 = NumToWords.ConvertAmount(ca1.Actualtax),
                                                    Fld27 = (totamt - ca1.Actualtax).ToString(),
                                                    Fld28 = NumToWords.ConvertAmount(totamt - ca1.Actualtax),
                                                    Fld29 = ca.Employee.EmpOffInfo != null && ca.Employee.EmpOffInfo.Branch != null ? ca.Employee.EmpOffInfo.Branch.Code : "",

                                                };

                                                //if (month)
                                                //{
                                                //    OGenericObjStatement.Fld100 = ca1.MonthYear.ToString();
                                                //}
                                                if (comp)
                                                {
                                                    OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                }
                                                if (dept)
                                                {
                                                    OGenericObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                }
                                                if (grp)
                                                {
                                                    OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                }
                                                if (grade)
                                                {
                                                    OGenericObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    OGenericObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    OGenericObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    OGenericObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                }
                                                if (emp)
                                                {
                                                    OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                }

                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                            }

                                        }

                                    }


                                }

                            }

                            return OGenericPayrollStatement;
                        }

                        break;

                    case "LOANPROPOSAL":

                        var OPFEmployeeLedLoan = new List<EmployeePFTrust>();

                        foreach (var item in EmpPayrollIdList)
                        {
                            var OPFLedgData_t = db.EmployeePFTrust
                               .Include(e => e.Employee)
                                //.Include(e=>e.Employee.PerAddr)
                                //.Include(e=>e.Employee.ServiceBookDates)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.PFTEmployeeLedger)                               
                                // .Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.PayStruct)                              
                                 .Where(e => e.Employee_Id == item).AsNoTracking()
                                 .FirstOrDefault();

                            List<PFTEmployeeLedger> PFTEmpLedger = new List<PFTEmployeeLedger>();
                            if (OPFLedgData_t != null)
                            {
                                OPFLedgData_t.Employee.EmpName = db.NameSingle.Where(e => e.Id == OPFLedgData_t.Employee.EmpName_Id).FirstOrDefault();
                                OPFLedgData_t.Employee.EmpOffInfo = db.EmpOff.Where(e => e.Id == OPFLedgData_t.Employee.EmpOffInfo_Id).FirstOrDefault();
                                OPFLedgData_t.Employee.EmpOffInfo.NationalityID = db.NationalityID.Find(OPFLedgData_t.Employee.EmpOffInfo.NationalityID);
                                OPFLedgData_t.Employee.ServiceBookDates = db.ServiceBookDates.Find(OPFLedgData_t.Employee.ServiceBookDates_Id);
                                OPFLedgData_t.PFTEmployeeLedger = db.PFTEmployeeLedger.Include(e => e.PassbookActivity).Where(e => e.EmployeePFTrust_Id == OPFLedgData_t.Id).AsNoTracking().ToList();
                                OPFEmployeeLedLoan.Add(OPFLedgData_t);
                            }
                        }

                        if (OPFEmployeeLedLoan == null || OPFEmployeeLedLoan.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            //var month = false;
                            var emp = false;
                            var dept = false;
                            var loca = false;
                            var comp = false;
                            var grp = false;
                            var unit = false;
                            var div = false;
                            var regn = false;
                            var grade = false;
                            var lvl = false;
                            var jobstat = false;
                            var job = false;
                            var jobpos = false;
                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();


                            foreach (var item in vc)
                            {

                                //if (item.LookupVal.ToUpper() == "MONTH")
                                //{
                                //    month = true;
                                //}
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {

                                    loca = true;
                                }
                                if (item.LookupVal.ToUpper() == "EMPLOYEE")
                                {
                                    emp = true;
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                }
                                if (item.LookupVal.ToUpper() == "COMPANY")
                                {
                                    comp = true;
                                }
                                if (item.LookupVal.ToUpper() == "GROUP")
                                {
                                    grp = true;
                                }
                                if (item.LookupVal.ToUpper() == "UNIT")
                                {
                                    unit = true;
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                }
                                if (item.LookupVal.ToUpper() == "REGION")
                                {
                                    regn = true;
                                }
                                if (item.LookupVal.ToUpper() == "GRADE")
                                {
                                    grade = true;
                                }
                                if (item.LookupVal.ToUpper() == "LEVEL")
                                {
                                    lvl = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBSTATUS")
                                {
                                    jobstat = true;
                                }

                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBPOSITION")
                                {
                                    jobpos = true;
                                }
                            }


                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in OPFEmployeeLedLoan)
                            {
                                if (ca.PFTEmployeeLedger != null && ca.PFTEmployeeLedger.Count() != 0)
                                {
                                    List<EmpSalStructDetails> OSalaryHeadTotalProjected = new List<EmpSalStructDetails>();
                                    try
                                    {
                                        var employeepayroll = db.EmployeePayroll.Where(e => e.Employee_Id == ca.Employee_Id).SingleOrDefault();
                                        var lasstructid = db.EmpSalStruct.Where(x => x.EmployeePayroll_Id == employeepayroll.Id).OrderByDescending(e => e.Id).AsNoTracking().FirstOrDefault();

                                        List<EmpSalStruct> EmpSalStructTotal = new List<EmpSalStruct>();
                                        EmpSalStruct OSalCurrentStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == employeepayroll.Id && e.Id == lasstructid.Id).SingleOrDefault();
                                        if (OSalCurrentStruct != null)
                                        {


                                            List<EmpSalStructDetails> EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == OSalCurrentStruct.Id).ToList();

                                            foreach (var EmpSalStructDetailsItem in EmpSalStructDetails)
                                            {
                                                //var SalHeadTmp = new SalaryHead();
                                                PayScaleAssignment PayScaleAssignmentObj = db.PayScaleAssignment.Where(e => e.Id == EmpSalStructDetailsItem.PayScaleAssignment_Id).FirstOrDefault();
                                                EmpSalStructDetailsItem.PayScaleAssignment = PayScaleAssignmentObj;
                                                SalHeadFormula SalaryHeadFormulaObj = db.SalHeadFormula.Where(e => e.Id == EmpSalStructDetailsItem.SalHeadFormula_Id).FirstOrDefault();
                                                EmpSalStructDetailsItem.SalHeadFormula = SalaryHeadFormulaObj;
                                                SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsItem.SalaryHead_Id).SingleOrDefault();
                                                EmpSalStructDetailsItem.SalaryHead = SalaryHead;
                                                LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                                                SalaryHead.SalHeadOperationType = SalHeadOperationType;
                                                LookupValue Type = db.LookupValue.Where(e => e.Id == SalaryHead.Type_Id).SingleOrDefault();
                                                SalaryHead.Type = Type;
                                                LookupValue Frequency = db.LookupValue.Where(e => e.Id == SalaryHead.Frequency_Id).SingleOrDefault();
                                                SalaryHead.Frequency = Frequency;
                                                LookupValue ProcessType = db.LookupValue.Where(e => e.Id == SalaryHead.ProcessType_Id).SingleOrDefault();
                                                SalaryHead.ProcessType = ProcessType;
                                                LookupValue RoundingMethod = db.LookupValue.Where(e => e.Id == SalaryHead.RoundingMethod_Id).SingleOrDefault();
                                                SalaryHead.RoundingMethod = RoundingMethod;
                                                List<LvHead> LvHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsItem.SalaryHead_Id).Select(e => e.LvHead.ToList()).SingleOrDefault();// to be check for output
                                                SalaryHead.LvHead = LvHead;



                                            }
                                        }
                                        OSalaryHeadTotalProjected = OSalCurrentStruct.EmpSalStructDetails.ToList();
                                    }
                                    catch (Exception ex)
                                    {
                                        throw ex;
                                    }



                                    var OPFTEMPLEDData = db.LoanAdvRequestPFT.Include(e => e.LoanAdvanceHeadPFT).Where(e => e.EmployeePFTrust_Id == ca.Id && e.SanctionedDate >= pFromDate && e.SanctionedDate <= pToDate).ToList();
                                    var Lastledgerdata = ca.PFTEmployeeLedger.OrderByDescending(e => e.Id).FirstOrDefault();

                                    if (OPFTEMPLEDData != null && OPFTEMPLEDData.Count() != 0)
                                    {
                                        int? geoid = ca.Employee.GeoStruct_Id;

                                        int? payid = ca.Employee.PayStruct_Id;

                                        int? funid = ca.Employee.FuncStruct_Id;

                                        GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                        PayStruct paystruct = db.PayStruct.Find(payid);

                                        FuncStruct funstruct = db.FuncStruct.Find(funid);

                                        GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                        if (GeoDataInd != null)
                                        {
                                            if (salheadlist != null && salheadlist.Count() != 0)
                                            {
                                                foreach (var item2 in salheadlist)
                                                {
                                                    var loanwg = db.LoanAdvanceHeadPFT.Include(x => x.LoanAdvancePolicyPFT).Where(e => e.Name == item2).SingleOrDefault();
                                                    int wageid = loanwg.LoanAdvancePolicyPFT.LoanSanctionWages_Id.Value;
                                                    Wages loanWages = db.Wages.Include(x => x.RateMaster).Include(x => x.RateMaster.Select(z => z.SalHead)).Where(e => e.Id == wageid).FirstOrDefault();
                                                    double Loanwages = Process.SalaryHeadGenProcess.WagecalcDirect(loanWages, null, OSalaryHeadTotalProjected);

                                                    var loanadvhead = OPFTEMPLEDData.Where(e => e.LoanAdvanceHeadPFT.Name == item2).ToList();
                                                    // foreach (var ca1 in OPFTEMPLEDData.Where(e => e.PassbookActivity.LookupVal.ToUpper() == item2.ToUpper()))
                                                    foreach (var ca1 in loanadvhead)
                                                    {
                                                        GenericField100 OGenericObjStatement = new GenericField100()
                                                        {
                                                            Fld2 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                                            Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                            Fld4 = GeoDataInd.FuncStruct_Job_Name,
                                                            Fld5 = GeoDataInd.GeoStruct_Location_Name,
                                                            Fld6 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.PFJoingDate != null ? ca.Employee.ServiceBookDates.PFJoingDate.Value.ToShortDateString() : "",
                                                            Fld7 = loanwg.Name,
                                                            Fld9 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.RetirementDate != null ? ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString() : "",
                                                            Fld10 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.BirthDate != null ? ca.Employee.ServiceBookDates.BirthDate.Value.ToShortDateString() : "",
                                                            Fld11 = DateTime.Now.Date.ToShortDateString(),
                                                            Fld12 = Loanwages.ToString(),
                                                            Fld13 = loanwg.LoanAdvancePolicyPFT.NoOfTimesPFTContribution.ToString(),
                                                            Fld14 = (Loanwages * loanwg.LoanAdvancePolicyPFT.NoOfTimesPFTContribution).ToString(),
                                                            Fld15 = (Lastledgerdata.OwnCloseBal + Lastledgerdata.OwnerCloseBal + Lastledgerdata.VPFCloseBal + Lastledgerdata.OwnIntCloseBal + Lastledgerdata.OwnerIntCloseBal + Lastledgerdata.VPFIntCloseBal).ToString(),
                                                            Fld16 = (Lastledgerdata.OwnCloseBal + Lastledgerdata.OwnerCloseBal + Lastledgerdata.VPFCloseBal + Lastledgerdata.OwnIntCloseBal + Lastledgerdata.OwnerIntCloseBal + Lastledgerdata.VPFIntCloseBal - 1000).ToString(),
                                                            Fld17 = Lastledgerdata.OwnCloseBal.ToString(),
                                                            Fld18 = Lastledgerdata.VPFCloseBal.ToString(),
                                                            Fld19 = Lastledgerdata.OwnerCloseBal.ToString(),
                                                            Fld20 = Lastledgerdata.OwnIntCloseBal.ToString(),
                                                            Fld21 = Lastledgerdata.VPFIntCloseBal.ToString(),
                                                            Fld22 = Lastledgerdata.OwnerIntCloseBal.ToString(),
                                                            Fld23 = ca1.LoanAppliedAmount.ToString(),
                                                            Fld24 = ca1.LoanSanctionedAmount.ToString(),
                                                            Fld25 = ca.Employee.EmpOffInfo.AccountNo.ToString(),
                                                            Fld26 = NumToWords.ConvertAmount(ca1.LoanSanctionedAmount),

                                                        };

                                                        //if (month)
                                                        //{
                                                        //    OGenericObjStatement.Fld100 = ca1.MonthYear.ToString();
                                                        //}
                                                        if (comp)
                                                        {
                                                            OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                        }
                                                        if (div)
                                                        {
                                                            OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                        }
                                                        if (loca)
                                                        {
                                                            OGenericObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                        }
                                                        if (dept)
                                                        {
                                                            OGenericObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                        }
                                                        if (grp)
                                                        {
                                                            OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                        }
                                                        if (unit)
                                                        {
                                                            OGenericObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                        }
                                                        if (grade)
                                                        {
                                                            OGenericObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                        }
                                                        if (lvl)
                                                        {
                                                            OGenericObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                        }
                                                        if (jobstat)
                                                        {
                                                            OGenericObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                        }
                                                        if (job)
                                                        {
                                                            OGenericObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                        }
                                                        if (jobpos)
                                                        {
                                                            OGenericObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                        }
                                                        if (emp)
                                                        {
                                                            OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                        }

                                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                                    }

                                                }
                                            }
                                            else
                                            {
                                                foreach (var ca1 in OPFTEMPLEDData)
                                                {
                                                    var loanwg = db.LoanAdvanceHeadPFT.Include(x => x.LoanAdvancePolicyPFT).Where(e => e.Name == ca1.LoanAdvanceHeadPFT.Name).SingleOrDefault();
                                                    int wageid = loanwg.LoanAdvancePolicyPFT.LoanSanctionWages_Id.Value;
                                                    Wages loanWages = db.Wages.Include(x => x.RateMaster).Include(x => x.RateMaster.Select(z => z.SalHead)).Where(e => e.Id == wageid).FirstOrDefault();
                                                    double Loanwages = Process.SalaryHeadGenProcess.WagecalcDirect(loanWages, null, OSalaryHeadTotalProjected);

                                                    GenericField100 OGenericObjStatement = new GenericField100()
                                                    {


                                                        Fld2 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                                        Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                        Fld4 = GeoDataInd.FuncStruct_Job_Name,
                                                        Fld5 = GeoDataInd.GeoStruct_Location_Name,
                                                        Fld6 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.PFJoingDate != null ? ca.Employee.ServiceBookDates.PFJoingDate.Value.ToShortDateString() : "",
                                                        Fld7 = loanwg.Name,
                                                        Fld9 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.RetirementDate != null ? ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString() : "",
                                                        Fld10 = ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.BirthDate != null ? ca.Employee.ServiceBookDates.BirthDate.Value.ToShortDateString() : "",
                                                        Fld11 = DateTime.Now.Date.ToShortDateString(),
                                                        Fld12 = Loanwages.ToString(),
                                                        Fld13 = loanwg.LoanAdvancePolicyPFT.NoOfTimesPFTContribution.ToString(),
                                                        Fld14 = (Loanwages * loanwg.LoanAdvancePolicyPFT.NoOfTimesPFTContribution).ToString(),
                                                        Fld15 = (Lastledgerdata.OwnCloseBal + Lastledgerdata.OwnerCloseBal + Lastledgerdata.VPFCloseBal + Lastledgerdata.OwnIntCloseBal + Lastledgerdata.OwnerIntCloseBal + Lastledgerdata.VPFIntCloseBal).ToString(),
                                                        Fld16 = (Lastledgerdata.OwnCloseBal + Lastledgerdata.OwnerCloseBal + Lastledgerdata.VPFCloseBal + Lastledgerdata.OwnIntCloseBal + Lastledgerdata.OwnerIntCloseBal + Lastledgerdata.VPFIntCloseBal - 1000).ToString(),
                                                        Fld17 = Lastledgerdata.OwnCloseBal.ToString(),
                                                        Fld18 = Lastledgerdata.VPFCloseBal.ToString(),
                                                        Fld19 = Lastledgerdata.OwnerCloseBal.ToString(),
                                                        Fld20 = Lastledgerdata.OwnIntCloseBal.ToString(),
                                                        Fld21 = Lastledgerdata.VPFIntCloseBal.ToString(),
                                                        Fld22 = Lastledgerdata.OwnerIntCloseBal.ToString(),
                                                        Fld23 = ca1.LoanAppliedAmount.ToString(),
                                                        Fld24 = ca1.LoanSanctionedAmount.ToString(),
                                                        Fld25 = ca.Employee.EmpOffInfo.AccountNo.ToString(),
                                                        Fld26 = NumToWords.ConvertAmount(ca1.LoanSanctionedAmount),

                                                    };

                                                    //if (month)
                                                    //{
                                                    //    OGenericObjStatement.Fld100 = ca1.MonthYear.ToString();
                                                    //}
                                                    if (comp)
                                                    {
                                                        OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                    }
                                                    if (div)
                                                    {
                                                        OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                    }
                                                    if (loca)
                                                    {
                                                        OGenericObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                    }
                                                    if (dept)
                                                    {
                                                        OGenericObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                    }
                                                    if (grp)
                                                    {
                                                        OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                    }
                                                    if (unit)
                                                    {
                                                        OGenericObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                    }
                                                    if (grade)
                                                    {
                                                        OGenericObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                    }
                                                    if (lvl)
                                                    {
                                                        OGenericObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                    }
                                                    if (jobstat)
                                                    {
                                                        OGenericObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                    }
                                                    if (job)
                                                    {
                                                        OGenericObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                    }
                                                    if (jobpos)
                                                    {
                                                        OGenericObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                    }
                                                    if (emp)
                                                    {
                                                        OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                    }

                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                                }

                                            }
                                        }
                                    }


                                }

                            }

                            return OGenericPayrollStatement;
                        }

                        break;

                    case "PFTLEDGERSTATEMENT":
                        var EmployeePFTrustObjlist = db.EmployeePFTrust
                           .Include(e => e.Employee)
                           .Include(e => e.Employee.EmpName)
                           .Include(e => e.PFTEmployeeLedger)
                           .Where(e => EmpPayrollIdList.Contains(e.Employee_Id.Value)).ToList();
                        if (EmployeePFTrustObjlist == null || EmployeePFTrustObjlist.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            //var month = false;
                            var emp = false;
                            var dept = false;
                            var loca = false;
                            var comp = false;
                            var grp = false;
                            var unit = false;
                            var div = false;
                            var regn = false;
                            var grade = false;
                            var lvl = false;
                            var jobstat = false;
                            var job = false;
                            var jobpos = false;
                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();
                            foreach (var item in vc)
                            {

                                //if (item.LookupVal.ToUpper() == "MONTH")
                                //{
                                //    month = true;
                                //}
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {

                                    loca = true;
                                }
                                if (item.LookupVal.ToUpper() == "EMPLOYEE")
                                {
                                    emp = true;
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                }
                                if (item.LookupVal.ToUpper() == "COMPANY")
                                {
                                    comp = true;
                                }
                                if (item.LookupVal.ToUpper() == "GROUP")
                                {
                                    grp = true;
                                }
                                if (item.LookupVal.ToUpper() == "UNIT")
                                {
                                    unit = true;
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                }
                                if (item.LookupVal.ToUpper() == "REGION")
                                {
                                    regn = true;
                                }
                                if (item.LookupVal.ToUpper() == "GRADE")
                                {
                                    grade = true;
                                }
                                if (item.LookupVal.ToUpper() == "LEVEL")
                                {
                                    lvl = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBSTATUS")
                                {
                                    jobstat = true;
                                }

                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBPOSITION")
                                {
                                    jobpos = true;
                                }
                            }


                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in EmployeePFTrustObjlist)
                            {

                                int? geoid = ca.Employee.GeoStruct_Id;

                                int? payid = ca.Employee.PayStruct_Id;

                                int? funid = ca.Employee.FuncStruct_Id;

                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                PayStruct paystruct = db.PayStruct.Find(payid);

                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                if (GeoDataInd != null)
                                {
                                    var OPFTEmployeeLedgerlist = ca.PFTEmployeeLedger.Where(e => e.PostingDate >= pFromDate && e.PostingDate <= pToDate).ToList();
                                    foreach (var item in OPFTEmployeeLedgerlist)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        {
                                            Fld2 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                            Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                            Fld4 = item.MonthYear.ToString() == null ? "" : item.MonthYear.ToString(),
                                            Fld5 = item.PFWages.ToString() == null ? "" : item.PFWages.ToString(),
                                            Fld6 = item.OwnPFMonthly.ToString() == null ? "" : item.OwnPFMonthly.ToString(),
                                            Fld7 = item.OwnerPFMonthly.ToString() == null ? "" : item.OwnerPFMonthly.ToString(),
                                            Fld8 = item.VPFAmountMonthly.ToString() == null ? "" : item.VPFAmountMonthly.ToString(),
                                            Fld9 = item.PFAmountMonthly.ToString() == null ? "" : item.PFAmountMonthly.ToString(),
                                            Fld10 = item.TDSAmount.ToString() == null ? "" : item.TDSAmount.ToString(),
                                            //
                                            Fld11 = item.OwnPFInt.ToString() == null ? "" : item.OwnPFInt.ToString(),
                                            Fld12 = item.VPFInt.ToString() == null ? "" : item.VPFInt.ToString(),
                                            Fld13 = item.PFInt.ToString() == null ? "" : item.PFInt.ToString(),
                                            Fld14 = item.TotPFInt.ToString() == null ? "" : item.TotPFInt.ToString(),
                                            Fld15 = item.IntOnInt.ToString() == null ? "" : item.IntOnInt.ToString(),
                                            Fld16 = item.PensionAmount.ToString() == null ? "" : item.PensionAmount.ToString(),
                                            Fld17 = item.OwnOpenBal.ToString() == null ? "" : item.OwnOpenBal.ToString(),
                                            Fld18 = item.OwnCloseBal.ToString() == null ? "" : item.OwnCloseBal.ToString(),
                                            Fld19 = item.OwnerOpenBal.ToString() == null ? "" : item.OwnerOpenBal.ToString(),
                                            Fld20 = item.OwnerCloseBal.ToString() == null ? "" : item.OwnerCloseBal.ToString(),
                                            //
                                            Fld21 = item.VPFOpenBal.ToString() == null ? "" : item.VPFOpenBal.ToString(),
                                            Fld22 = item.VPFCloseBal.ToString() == null ? "" : item.VPFCloseBal.ToString(),
                                            Fld23 = item.PFOpenBal.ToString() == null ? "" : item.PFOpenBal.ToString(),
                                            Fld24 = item.PFCloseBal.ToString() == null ? "" : item.PFCloseBal.ToString(),
                                            Fld25 = item.PFIntOpenBal.ToString() == null ? "" : item.PFIntOpenBal.ToString(),
                                            Fld26 = item.PFIntCloseBal.ToString() == null ? "" : item.PFIntCloseBal.ToString(),
                                            Fld27 = item.OwnIntOpenBal.ToString() == null ? "" : item.OwnIntOpenBal.ToString(),
                                            Fld28 = item.OwnIntCloseBal.ToString() == null ? "" : item.OwnIntCloseBal.ToString(),
                                            Fld29 = item.OwnerIntOpenBal.ToString() == null ? "" : item.OwnerIntOpenBal.ToString(),
                                            Fld30 = item.OwnerIntCloseBal.ToString() == null ? "" : item.OwnerIntCloseBal.ToString(),
                                            //
                                            Fld31 = item.VPFIntOpenBal.ToString() == null ? "" : item.VPFIntOpenBal.ToString(),
                                            Fld32 = item.VPFIntCloseBal.ToString() == null ? "" : item.VPFIntCloseBal.ToString(),
                                            Fld33 = item.TotalIntOpenBal.ToString() == null ? "" : item.TotalIntOpenBal.ToString(),
                                            Fld34 = item.TotalIntCloseBal.ToString() == null ? "" : item.TotalIntCloseBal.ToString(),
                                            Fld35 = item.OwnPFLoan.ToString() == null ? "" : item.OwnPFLoan.ToString(),
                                            Fld36 = item.OwnerPFLoan.ToString() == null ? "" : item.OwnerPFLoan.ToString(),
                                            Fld37 = item.VPFPFLoan.ToString() == null ? "" : item.VPFPFLoan.ToString(),
                                            Fld38 = item.OwnIntPFLoan.ToString() == null ? "" : item.OwnIntPFLoan.ToString(),
                                            Fld39 = item.OwnerIntPFLoan.ToString() == null ? "" : item.OwnerIntPFLoan.ToString(),
                                            Fld40 = item.VPFIntPFLoan.ToString() == null ? "" : item.VPFIntPFLoan.ToString(),
                                            //
                                            Fld41 = item.IntOnIntPFLoan.ToString() == null ? "" : item.IntOnIntPFLoan.ToString(),
                                            Fld42 = item.LoanAmountDebit.ToString() == null ? "" : item.LoanAmountDebit.ToString(),
                                            Fld43 = item.LoanAmountCredit.ToString() == null ? "" : item.LoanAmountCredit.ToString(),
                                            Fld44 = item.IntonIntOpenBal.ToString() == null ? "" : item.IntonIntOpenBal.ToString(),
                                            Fld45 = item.IntonIntCloseBal.ToString() == null ? "" : item.IntonIntCloseBal.ToString(),
                                            Fld46 = item.Narration == null ? "" : item.Narration,
                                            Fld47 = item.IsPassbookClose == true ? "Yes" : "No",
                                            Fld48 = item.TDSIncome.ToString() == null ? "" : item.TDSIncome.ToString(),
                                            Fld49 = item.IsTDSAppl == true ? "Yes" : "No",
                                            Fld50 = item.OwnerPFInt.ToString() == null ? "" : item.OwnerPFInt.ToString(),
                                            Fld51 = item.OwnIntOnInt.ToString() == null ? "" : item.OwnIntOnInt.ToString(),
                                            Fld52 = item.OwnerIntOnInt.ToString() == null ? "" : item.OwnerIntOnInt.ToString(),
                                            Fld53 = item.VPFIntOnInt.ToString() == null ? "" : item.VPFIntOnInt.ToString(),

                                        };
                                        //if (month)
                                        //{
                                        //    OGenericObjStatement.Fld100 = ca1.MonthYear.ToString();
                                        //}
                                        if (comp)
                                        {
                                            OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                        }
                                        if (div)
                                        {
                                            OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                        }
                                        if (loca)
                                        {
                                            OGenericObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                        }
                                        if (dept)
                                        {
                                            OGenericObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                        }
                                        if (grp)
                                        {
                                            OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                        }
                                        if (unit)
                                        {
                                            OGenericObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                        }
                                        if (grade)
                                        {
                                            OGenericObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                        }
                                        if (lvl)
                                        {
                                            OGenericObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                        }
                                        if (jobstat)
                                        {
                                            OGenericObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                        }
                                        if (job)
                                        {
                                            OGenericObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                        }
                                        if (jobpos)
                                        {
                                            OGenericObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                        }
                                        if (emp)
                                        {
                                            OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                        }

                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }

                                }

                            }
                        }

                        return OGenericPayrollStatement;
                        break;



                    case "INTERESTSTATEMENT":

                        var OPFEmployeeIntstatement = new List<EmployeePFTrust>();

                        foreach (var item in EmpPayrollIdList)
                        {
                            var OPFLedgData_t = db.EmployeePFTrust
                               .Include(e => e.Employee)
                                //.Include(e=>e.Employee.PerAddr)
                                //.Include(e=>e.Employee.ServiceBookDates)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.PFTEmployeeLedger)                               
                                // .Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.PayStruct)                              
                                 .Where(e => e.Employee_Id == item).AsNoTracking()
                                 .FirstOrDefault();

                            List<PFTEmployeeLedger> PFTEmpLedger = new List<PFTEmployeeLedger>();
                            if (OPFLedgData_t != null)
                            {
                                OPFLedgData_t.Employee.EmpName = db.NameSingle.Find(OPFLedgData_t.Employee.EmpName_Id);
                                //OPFLedgData_t.Employee.EmpOffInfo = db.EmpOff.Find(OPFLedgData_t.Employee.EmpOffInfo_Id);
                                //OPFLedgData_t.Employee.EmpOffInfo.NationalityID = db.NationalityID.Find(OPFLedgData_t.Employee.EmpOffInfo.NationalityID);
                                //OPFLedgData_t.Employee.ServiceBookDates = db.ServiceBookDates.Find(OPFLedgData_t.Employee.ServiceBookDates_Id);
                                //OPFLedgData_t.Employee.PerAddr = db.Address.Find(OPFLedgData_t.Employee.PerAddr_Id);
                                OPFLedgData_t.PFTEmployeeLedger = db.PFTEmployeeLedger.Include(e => e.PassbookActivity).Where(e => e.EmployeePFTrust_Id == OPFLedgData_t.Id).AsNoTracking().ToList();
                                OPFEmployeeIntstatement.Add(OPFLedgData_t);
                            }
                        }

                        if (OPFEmployeeIntstatement == null || OPFEmployeeIntstatement.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            //var month = false;
                            var emp = false;
                            var dept = false;
                            var loca = false;
                            var comp = false;
                            var grp = false;
                            var unit = false;
                            var div = false;
                            var regn = false;
                            var grade = false;
                            var lvl = false;
                            var jobstat = false;
                            var job = false;
                            var jobpos = false;
                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();


                            foreach (var item in vc)
                            {

                                //if (item.LookupVal.ToUpper() == "MONTH")
                                //{
                                //    month = true;
                                //}
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {

                                    loca = true;
                                }
                                if (item.LookupVal.ToUpper() == "EMPLOYEE")
                                {
                                    emp = true;
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                }
                                if (item.LookupVal.ToUpper() == "COMPANY")
                                {
                                    comp = true;
                                }
                                if (item.LookupVal.ToUpper() == "GROUP")
                                {
                                    grp = true;
                                }
                                if (item.LookupVal.ToUpper() == "UNIT")
                                {
                                    unit = true;
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                }
                                if (item.LookupVal.ToUpper() == "REGION")
                                {
                                    regn = true;
                                }
                                if (item.LookupVal.ToUpper() == "GRADE")
                                {
                                    grade = true;
                                }
                                if (item.LookupVal.ToUpper() == "LEVEL")
                                {
                                    lvl = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBSTATUS")
                                {
                                    jobstat = true;
                                }

                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBPOSITION")
                                {
                                    jobpos = true;
                                }
                            }

                            var PassbookLoanIDValue = new List<string>();
                            PassbookLoanIDValue.Add("LOAN DEBIT BALANCE");
                            PassbookLoanIDValue.Add("LOAN CREDIT BALANCE");
                            PassbookLoanIDValue.Add("PF BALANCE");
                            PassbookLoanIDValue.Add("INTEREST BALANCE");
                            PassbookLoanIDValue.Add("SETTLEMENT BALANCE");
                            List<int> PassbookLoanID = new List<int>();
                            PassbookLoanID = db.LookupValue.Where(e => PassbookLoanIDValue.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();

                            var PassbookLoanIDValueINT = new List<string>();
                            PassbookLoanIDValueINT.Add("INTEREST POSTING");
                            PassbookLoanIDValueINT.Add("SETTLEMENT POSTING");
                            List<int> PassbookLoanIDINT = new List<int>();
                            PassbookLoanIDINT = db.LookupValue.Where(e => PassbookLoanIDValueINT.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();

                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in OPFEmployeeIntstatement)
                            {
                                if (ca.PFTEmployeeLedger != null && ca.PFTEmployeeLedger.Count() != 0)
                                {
                                    var mstart = mFromDate;
                                    var mend = mToDate;
                                    String mPeriodRange = "";
                                    double Grossownclosebal = 0;
                                    double Grossownerclosebal = 0;
                                    double GrossVPFclosebal = 0;

                                    double Intownclosebal = 0;
                                    double Intownerclosebal = 0;
                                    double IntVPFclosebal = 0;

                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    for (DateTime mTempDate = mstart; mTempDate <= mend; mTempDate = mTempDate.AddMonths(1))
                                    {
                                        double ownclosebal = 0;
                                        double ownerclosebal = 0;
                                        double VPFclosebal = 0;

                                        double ownintclosebal = 0;
                                        double ownerintclosebal = 0;
                                        double VPFintclosebal = 0;
                                        mPeriodRange = Convert.ToDateTime(mTempDate).ToString("MM/yyyy");

                                        var OPFTEMPLEDData = ca.PFTEmployeeLedger.Where(e => e.MonthYear == mPeriodRange && PassbookLoanID.Contains(e.PassbookActivity.Id) == true).OrderBy(e => e.Id).LastOrDefault();

                                        var OPFTEMPLEDDataINT = ca.PFTEmployeeLedger.Where(e => e.MonthYear == mPeriodRange && PassbookLoanIDINT.Contains(e.PassbookActivity.Id) == true).OrderBy(e => e.Id).LastOrDefault();

                                        var PFPART = new List<string>();
                                        PFPART.Add("OWN");
                                        PFPART.Add("OWNER");
                                        PFPART.Add("VPF");
                                        foreach (var PFP in PFPART)
                                        {
                                            if (PFP == "OWN")
                                            {
                                                if (OPFTEMPLEDData != null)
                                                {
                                                    ownclosebal = OPFTEMPLEDData.OwnCloseBal;
                                                    ownerclosebal = OPFTEMPLEDData.OwnerCloseBal;
                                                    VPFclosebal = OPFTEMPLEDData.VPFCloseBal;


                                                }
                                                if (OPFTEMPLEDDataINT != null)
                                                {
                                                    ownintclosebal = ownintclosebal + OPFTEMPLEDDataINT.OwnPFInt + OPFTEMPLEDDataINT.OwnIntOnInt;
                                                }



                                                if (GeoDataInd != null)
                                                {

                                                    GenericField100 OGenericObjStatement = new GenericField100()
                                                    {

                                                        Fld2 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                                        Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),

                                                        Fld4 = "OWN",
                                                        Fld10 = mTempDate.ToString("MMMM"),
                                                        Fld8 = ownclosebal.ToString(),
                                                        Fld29 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                                        Fld9 = ownintclosebal.ToString(),

                                                    };


                                                    if (comp)
                                                    {
                                                        OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                    }
                                                    if (div)
                                                    {
                                                        OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                    }
                                                    if (loca)
                                                    {
                                                        OGenericObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                    }
                                                    if (dept)
                                                    {
                                                        OGenericObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                    }
                                                    if (grp)
                                                    {
                                                        OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                    }
                                                    if (unit)
                                                    {
                                                        OGenericObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                    }
                                                    if (grade)
                                                    {
                                                        OGenericObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                    }
                                                    if (lvl)
                                                    {
                                                        OGenericObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                    }
                                                    if (jobstat)
                                                    {
                                                        OGenericObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                    }
                                                    if (job)
                                                    {
                                                        OGenericObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                    }
                                                    if (jobpos)
                                                    {
                                                        OGenericObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                    }
                                                    if (emp)
                                                    {
                                                        OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                    }

                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                                }

                                            }
                                            if (PFP == "OWNER")
                                            {
                                                if (OPFTEMPLEDData != null)
                                                {
                                                    ownclosebal = OPFTEMPLEDData.OwnCloseBal;
                                                    ownerclosebal = OPFTEMPLEDData.OwnerCloseBal;
                                                    VPFclosebal = OPFTEMPLEDData.VPFCloseBal;


                                                }
                                                if (OPFTEMPLEDDataINT != null)
                                                {
                                                    ownerintclosebal = ownerintclosebal + OPFTEMPLEDDataINT.OwnerPFInt + OPFTEMPLEDDataINT.OwnerIntOnInt;
                                                }



                                                if (GeoDataInd != null)
                                                {

                                                    GenericField100 OGenericObjStatement = new GenericField100()
                                                    {

                                                        Fld2 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                                        Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),

                                                        Fld4 = "OWNER",
                                                        Fld10 = mTempDate.ToString("MMMM"),
                                                        Fld8 = ownerclosebal.ToString(),
                                                        Fld29 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                                        Fld9 = ownerintclosebal.ToString(),

                                                    };


                                                    if (comp)
                                                    {
                                                        OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                    }
                                                    if (div)
                                                    {
                                                        OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                    }
                                                    if (loca)
                                                    {
                                                        OGenericObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                    }
                                                    if (dept)
                                                    {
                                                        OGenericObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                    }
                                                    if (grp)
                                                    {
                                                        OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                    }
                                                    if (unit)
                                                    {
                                                        OGenericObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                    }
                                                    if (grade)
                                                    {
                                                        OGenericObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                    }
                                                    if (lvl)
                                                    {
                                                        OGenericObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                    }
                                                    if (jobstat)
                                                    {
                                                        OGenericObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                    }
                                                    if (job)
                                                    {
                                                        OGenericObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                    }
                                                    if (jobpos)
                                                    {
                                                        OGenericObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                    }
                                                    if (emp)
                                                    {
                                                        OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                    }

                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                                }

                                            }
                                            if (PFP == "VPF")
                                            {
                                                if (OPFTEMPLEDData != null)
                                                {
                                                    ownclosebal = OPFTEMPLEDData.OwnCloseBal;
                                                    ownerclosebal = OPFTEMPLEDData.OwnerCloseBal;
                                                    VPFclosebal = OPFTEMPLEDData.VPFCloseBal;


                                                }
                                                if (OPFTEMPLEDDataINT != null)
                                                {
                                                    VPFintclosebal = VPFintclosebal + OPFTEMPLEDDataINT.VPFInt + OPFTEMPLEDDataINT.VPFIntOnInt;
                                                }



                                                if (GeoDataInd != null)
                                                {

                                                    GenericField100 OGenericObjStatement = new GenericField100()
                                                    {

                                                        Fld2 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                                        Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),

                                                        Fld4 = "VPF",
                                                        Fld10 = mTempDate.ToString("MMMM"),
                                                        Fld8 = VPFclosebal.ToString(),
                                                        Fld29 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                                        Fld9 = VPFintclosebal.ToString(),

                                                    };


                                                    if (comp)
                                                    {
                                                        OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                    }
                                                    if (div)
                                                    {
                                                        OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                    }
                                                    if (loca)
                                                    {
                                                        OGenericObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                    }
                                                    if (dept)
                                                    {
                                                        OGenericObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                    }
                                                    if (grp)
                                                    {
                                                        OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                    }
                                                    if (unit)
                                                    {
                                                        OGenericObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                    }
                                                    if (grade)
                                                    {
                                                        OGenericObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                    }
                                                    if (lvl)
                                                    {
                                                        OGenericObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                    }
                                                    if (jobstat)
                                                    {
                                                        OGenericObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                    }
                                                    if (job)
                                                    {
                                                        OGenericObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                    }
                                                    if (jobpos)
                                                    {
                                                        OGenericObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                    }
                                                    if (emp)
                                                    {
                                                        OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                    }

                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                                }

                                            }
                                        }




                                    }


                                }

                            }

                            return OGenericPayrollStatement;
                        }

                        break;


                    case "INTERESTPRODUCT":

                        var OPFEmployeeIntproduct = new List<EmployeePFTrust>();

                        foreach (var item in EmpPayrollIdList)
                        {
                            var OPFLedgData_t = db.EmployeePFTrust
                               .Include(e => e.Employee)
                                //.Include(e=>e.Employee.PerAddr)
                                //.Include(e=>e.Employee.ServiceBookDates)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.PFTEmployeeLedger)                               
                                // .Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.PayStruct)                              
                                 .Where(e => e.Employee_Id == item).AsNoTracking()
                                 .FirstOrDefault();

                            List<PFTEmployeeLedger> PFTEmpLedger = new List<PFTEmployeeLedger>();
                            if (OPFLedgData_t != null)
                            {
                                OPFLedgData_t.Employee.EmpName = db.NameSingle.Find(OPFLedgData_t.Employee.EmpName_Id);
                                //OPFLedgData_t.Employee.EmpOffInfo = db.EmpOff.Find(OPFLedgData_t.Employee.EmpOffInfo_Id);
                                //OPFLedgData_t.Employee.EmpOffInfo.NationalityID = db.NationalityID.Find(OPFLedgData_t.Employee.EmpOffInfo.NationalityID);
                                //OPFLedgData_t.Employee.ServiceBookDates = db.ServiceBookDates.Find(OPFLedgData_t.Employee.ServiceBookDates_Id);
                                //OPFLedgData_t.Employee.PerAddr = db.Address.Find(OPFLedgData_t.Employee.PerAddr_Id);
                                OPFLedgData_t.PFTEmployeeLedger = db.PFTEmployeeLedger.Include(e => e.PassbookActivity).Where(e => e.EmployeePFTrust_Id == OPFLedgData_t.Id).AsNoTracking().ToList();
                                OPFEmployeeIntproduct.Add(OPFLedgData_t);
                            }
                        }

                        if (OPFEmployeeIntproduct == null || OPFEmployeeIntproduct.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            //var month = false;
                            var emp = false;
                            var dept = false;
                            var loca = false;
                            var comp = false;
                            var grp = false;
                            var unit = false;
                            var div = false;
                            var regn = false;
                            var grade = false;
                            var lvl = false;
                            var jobstat = false;
                            var job = false;
                            var jobpos = false;
                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();


                            foreach (var item in vc)
                            {

                                //if (item.LookupVal.ToUpper() == "MONTH")
                                //{
                                //    month = true;
                                //}
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {

                                    loca = true;
                                }
                                if (item.LookupVal.ToUpper() == "EMPLOYEE")
                                {
                                    emp = true;
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                }
                                if (item.LookupVal.ToUpper() == "COMPANY")
                                {
                                    comp = true;
                                }
                                if (item.LookupVal.ToUpper() == "GROUP")
                                {
                                    grp = true;
                                }
                                if (item.LookupVal.ToUpper() == "UNIT")
                                {
                                    unit = true;
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                }
                                if (item.LookupVal.ToUpper() == "REGION")
                                {
                                    regn = true;
                                }
                                if (item.LookupVal.ToUpper() == "GRADE")
                                {
                                    grade = true;
                                }
                                if (item.LookupVal.ToUpper() == "LEVEL")
                                {
                                    lvl = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBSTATUS")
                                {
                                    jobstat = true;
                                }

                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBPOSITION")
                                {
                                    jobpos = true;
                                }
                            }


                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in OPFEmployeeIntproduct)
                            {
                                if (ca.PFTEmployeeLedger != null && ca.PFTEmployeeLedger.Count() != 0)
                                {
                                    var mstart = mFromDate;
                                    var mend = mToDate;
                                    String mPeriodRange = "";
                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);
                                    var PassbookLoanIDValueINT = new List<string>();
                                    PassbookLoanIDValueINT.Add("INTEREST POSTING");
                                    PassbookLoanIDValueINT.Add("SETTLEMENT POSTING");
                                    List<int> PassbookLoanIDINT = new List<int>();
                                    PassbookLoanIDINT = db.LookupValue.Where(e => PassbookLoanIDValueINT.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();
                                    double ownintclosebal = 0;
                                    double ownerintclosebal = 0;
                                    double VPFintclosebal = 0;
                                    double Totint = 0;
                                    for (DateTime mTempDate = mstart; mTempDate <= mend; mTempDate = mTempDate.AddMonths(1))
                                    {

                                        mPeriodRange = Convert.ToDateTime(mTempDate).ToString("MM/yyyy");


                                        var OPFTEMPLEDDataINT = ca.PFTEmployeeLedger.Where(e => e.MonthYear == mPeriodRange && PassbookLoanIDINT.Contains(e.PassbookActivity.Id) == true).OrderBy(e => e.Id).LastOrDefault();

                                        var PFPART = new List<string>();
                                        PFPART.Add("OWN");
                                        PFPART.Add("OWNER");
                                        PFPART.Add("VPF");
                                        foreach (var PFP in PFPART)
                                        {
                                            if (PFP == "OWN")
                                            {

                                                if (OPFTEMPLEDDataINT != null)
                                                {
                                                    ownintclosebal = ownintclosebal + OPFTEMPLEDDataINT.OwnPFInt + OPFTEMPLEDDataINT.OwnIntOnInt;
                                                }


                                            }
                                            if (PFP == "OWNER")
                                            {

                                                if (OPFTEMPLEDDataINT != null)
                                                {
                                                    ownerintclosebal = ownerintclosebal + OPFTEMPLEDDataINT.OwnerPFInt + OPFTEMPLEDDataINT.OwnerIntOnInt;
                                                }


                                            }
                                            if (PFP == "VPF")
                                            {

                                                if (OPFTEMPLEDDataINT != null)
                                                {
                                                    VPFintclosebal = VPFintclosebal + OPFTEMPLEDDataINT.VPFInt + OPFTEMPLEDDataINT.VPFIntOnInt;
                                                }


                                            }
                                        }




                                    }
                                    Totint = ownintclosebal + ownerintclosebal + VPFintclosebal;
                                    if (GeoDataInd != null)
                                    {

                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        {

                                            Fld2 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                            Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),

                                            Fld4 = ownintclosebal.ToString(),
                                            Fld5 = ownerintclosebal.ToString(),
                                            Fld6 = VPFintclosebal.ToString(),
                                            Fld7 = Totint.ToString(),

                                        };


                                        if (comp)
                                        {
                                            OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                        }
                                        if (div)
                                        {
                                            OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                        }
                                        if (loca)
                                        {
                                            OGenericObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                        }
                                        if (dept)
                                        {
                                            OGenericObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                        }
                                        if (grp)
                                        {
                                            OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                        }
                                        if (unit)
                                        {
                                            OGenericObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                        }
                                        if (grade)
                                        {
                                            OGenericObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                        }
                                        if (lvl)
                                        {
                                            OGenericObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                        }
                                        if (jobstat)
                                        {
                                            OGenericObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                        }
                                        if (job)
                                        {
                                            OGenericObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                        }
                                        if (jobpos)
                                        {
                                            OGenericObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                        }
                                        if (emp)
                                        {
                                            OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                        }

                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }



                                }

                            }

                            return OGenericPayrollStatement;
                        }

                        break;


                    case "SETTELMENTREGISTER":

                        var OPFEmployeesettregister = new List<EmployeePFTrust>();

                        foreach (var item in EmpPayrollIdList)
                        {
                            var OPFLedgData_t = db.EmployeePFTrust
                               .Include(e => e.Employee)
                                //.Include(e=>e.Employee.PerAddr)
                                //.Include(e=>e.Employee.ServiceBookDates)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.PFTEmployeeLedger)                               
                                // .Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.PayStruct)                              
                                 .Where(e => e.Employee_Id == item).AsNoTracking()
                                 .FirstOrDefault();

                            List<EmpSettlementPFT> PFTEmpLedger = new List<EmpSettlementPFT>();
                            if (OPFLedgData_t != null)
                            {
                                OPFLedgData_t.Employee.EmpName = db.NameSingle.Find(OPFLedgData_t.Employee.EmpName_Id);
                                OPFLedgData_t.Employee.EmpOffInfo = db.EmpOff.Find(OPFLedgData_t.Employee.EmpOffInfo_Id);
                                OPFLedgData_t.Employee.EmpOffInfo.NationalityID = db.NationalityID.Find(OPFLedgData_t.Employee.EmpOffInfo.NationalityID);
                                OPFLedgData_t.Employee.ServiceBookDates = db.ServiceBookDates.Find(OPFLedgData_t.Employee.ServiceBookDates_Id);
                                OPFLedgData_t.Employee.PerAddr = db.Address.Find(OPFLedgData_t.Employee.PerAddr_Id);
                                // OPFLedgData_t.PFTEmployeeLedger = db.PFTEmployeeLedger.Include(e => e.PassbookActivity).Where(e => e.Id == OPFLedgData_t.Id).ToList();
                                OPFLedgData_t.EmpSettlementPFT = db.EmpSettlementPFT.Include(e => e.PassbookActivity).Where(e => e.EmployeePFTrust_Id == OPFLedgData_t.Id).AsNoTracking().ToList();
                                OPFEmployeesettregister.Add(OPFLedgData_t);
                            }
                        }

                        if (OPFEmployeesettregister == null || OPFEmployeesettregister.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            //var month = false;
                            var emp = false;
                            var dept = false;
                            var loca = false;
                            var comp = false;
                            var grp = false;
                            var unit = false;
                            var div = false;
                            var regn = false;
                            var grade = false;
                            var lvl = false;
                            var jobstat = false;
                            var job = false;
                            var jobpos = false;
                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();


                            foreach (var item in vc)
                            {

                                //if (item.LookupVal.ToUpper() == "MONTH")
                                //{
                                //    month = true;
                                //}
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {

                                    loca = true;
                                }
                                if (item.LookupVal.ToUpper() == "EMPLOYEE")
                                {
                                    emp = true;
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                }
                                if (item.LookupVal.ToUpper() == "COMPANY")
                                {
                                    comp = true;
                                }
                                if (item.LookupVal.ToUpper() == "GROUP")
                                {
                                    grp = true;
                                }
                                if (item.LookupVal.ToUpper() == "UNIT")
                                {
                                    unit = true;
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                }
                                if (item.LookupVal.ToUpper() == "REGION")
                                {
                                    regn = true;
                                }
                                if (item.LookupVal.ToUpper() == "GRADE")
                                {
                                    grade = true;
                                }
                                if (item.LookupVal.ToUpper() == "LEVEL")
                                {
                                    lvl = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBSTATUS")
                                {
                                    jobstat = true;
                                }

                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBPOSITION")
                                {
                                    jobpos = true;
                                }
                            }

                            var PassbookLoanIDValueINT = new List<string>();
                            PassbookLoanIDValueINT.Add("SETTLEMENT BALANCE");
                            List<int> PassbookLoanIDINT = new List<int>();
                            PassbookLoanIDINT = db.LookupValue.Where(e => PassbookLoanIDValueINT.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();


                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in OPFEmployeesettregister)
                            {
                                if (ca.EmpSettlementPFT != null && ca.EmpSettlementPFT.Count() != 0)
                                {
                                    var OPFTEMPLEDData = ca.EmpSettlementPFT.Where(e => e.SettlementDate >= pFromDate && e.SettlementDate <= pToDate && PassbookLoanIDINT.Contains(e.PassbookActivity.Id) == true && e.IsPaymentLock == true).ToList();

                                    if (OPFTEMPLEDData != null && OPFTEMPLEDData.Count() != 0)
                                    {
                                        int? geoid = ca.Employee.GeoStruct_Id;

                                        int? payid = ca.Employee.PayStruct_Id;

                                        int? funid = ca.Employee.FuncStruct_Id;

                                        GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                        PayStruct paystruct = db.PayStruct.Find(payid);

                                        FuncStruct funstruct = db.FuncStruct.Find(funid);

                                        GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                        if (GeoDataInd != null)
                                        {


                                            foreach (var ca1 in OPFTEMPLEDData)
                                            {
                                                string settmonth = Convert.ToDateTime(ca1.SettlementDate).ToString("MM/yyyy");
                                                double totpaid = ca1.OwnCloseBal + ca1.OwnIntCloseBal + ca1.OwnerCloseBal + ca1.OwnerIntCloseBal + ca1.VPFCloseBal + ca1.VPFIntCloseBal;
                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                {


                                                    Fld2 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                                    Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                    Fld4 = ca1.SeperationDate.ToShortDateString(),
                                                    Fld5 = ca.Employee.ServiceBookDates.RetirementDate == null ? "" : ca.Employee.ServiceBookDates.RetirementDate.Value.Date.ToShortDateString(),
                                                    Fld6 = ca1.ChequeIssueDate == null ? "" : ca1.ChequeIssueDate.Date.ToShortDateString(),
                                                    Fld7 = ca1.OwnCloseBal.ToString() != null ? ca1.OwnCloseBal.ToString() : "0",
                                                    Fld8 = ca1.OwnIntCloseBal.ToString() != null ? ca1.OwnIntCloseBal.ToString() : "0",
                                                    Fld9 = ca1.OwnerCloseBal.ToString() != null ? ca1.OwnerCloseBal.ToString() : "0",
                                                    Fld10 = ca1.OwnerIntCloseBal.ToString() != null ? ca1.OwnerIntCloseBal.ToString() : "0",
                                                    Fld11 = ca1.VPFCloseBal.ToString() != null ? ca1.VPFCloseBal.ToString() : "0",
                                                    Fld12 = ca1.VPFIntCloseBal.ToString() != null ? ca1.VPFIntCloseBal.ToString() : "0",
                                                    Fld13 = totpaid.ToString(),
                                                    Fld14 = settmonth.ToString(),
                                                    Fld15 = ca1.Cheque_no == null ? "" : ca1.Cheque_no.ToString(),

                                                };

                                                //if (month)
                                                //{
                                                //    OGenericObjStatement.Fld100 = ca1.MonthYear.ToString();
                                                //}
                                                if (comp)
                                                {
                                                    OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                }
                                                if (dept)
                                                {
                                                    OGenericObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                }
                                                if (grp)
                                                {
                                                    OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                }
                                                if (grade)
                                                {
                                                    OGenericObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    OGenericObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    OGenericObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    OGenericObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                }
                                                if (emp)
                                                {
                                                    OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                }

                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                            }

                                        }

                                    }


                                }

                            }

                            return OGenericPayrollStatement;
                        }

                        break;


                    case "LOANREGISTERSTATEMENT":

                        var OPFEmployeeloanregstate = new List<EmployeePFTrust>();

                        foreach (var item in EmpPayrollIdList)
                        {
                            var OPFLedgData_t = db.EmployeePFTrust
                               .Include(e => e.Employee)
                                //.Include(e=>e.Employee.PerAddr)
                                //.Include(e=>e.Employee.ServiceBookDates)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.PFTEmployeeLedger)                               
                                // .Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.PayStruct)                              
                                 .Where(e => e.Employee_Id == item).AsNoTracking()
                                 .FirstOrDefault();

                            List<LoanAdvRequestPFT> PFTEmpLedger = new List<LoanAdvRequestPFT>();
                            if (OPFLedgData_t != null)
                            {
                                OPFLedgData_t.Employee.EmpName = db.NameSingle.Find(OPFLedgData_t.Employee.EmpName_Id);
                                OPFLedgData_t.Employee.EmpOffInfo = db.EmpOff.Find(OPFLedgData_t.Employee.EmpOffInfo_Id);
                                //OPFLedgData_t.Employee.EmpOffInfo.NationalityID = db.NationalityID.Find(OPFLedgData_t.Employee.EmpOffInfo.NationalityID);
                                //OPFLedgData_t.Employee.ServiceBookDates = db.ServiceBookDates.Find(OPFLedgData_t.Employee.ServiceBookDates_Id);
                                //OPFLedgData_t.Employee.PerAddr = db.Address.Find(OPFLedgData_t.Employee.PerAddr_Id);
                                OPFLedgData_t.LoanAdvRequestPFT = db.LoanAdvRequestPFT.Include(e => e.LoanAdvanceHeadPFT).Include(e => e.LoanWFDetails).Where(e => e.EmployeePFTrust_Id == OPFLedgData_t.Id).ToList();
                                OPFEmployeeloanregstate.Add(OPFLedgData_t);
                            }
                        }

                        if (OPFEmployeeloanregstate == null || OPFEmployeeloanregstate.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            //var month = false;
                            var emp = false;
                            var dept = false;
                            var loca = false;
                            var comp = false;
                            var grp = false;
                            var unit = false;
                            var div = false;
                            var regn = false;
                            var grade = false;
                            var lvl = false;
                            var jobstat = false;
                            var job = false;
                            var jobpos = false;
                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();


                            foreach (var item in vc)
                            {

                                //if (item.LookupVal.ToUpper() == "MONTH")
                                //{
                                //    month = true;
                                //}
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {

                                    loca = true;
                                }
                                if (item.LookupVal.ToUpper() == "EMPLOYEE")
                                {
                                    emp = true;
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                }
                                if (item.LookupVal.ToUpper() == "COMPANY")
                                {
                                    comp = true;
                                }
                                if (item.LookupVal.ToUpper() == "GROUP")
                                {
                                    grp = true;
                                }
                                if (item.LookupVal.ToUpper() == "UNIT")
                                {
                                    unit = true;
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                }
                                if (item.LookupVal.ToUpper() == "REGION")
                                {
                                    regn = true;
                                }
                                if (item.LookupVal.ToUpper() == "GRADE")
                                {
                                    grade = true;
                                }
                                if (item.LookupVal.ToUpper() == "LEVEL")
                                {
                                    lvl = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBSTATUS")
                                {
                                    jobstat = true;
                                }

                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBPOSITION")
                                {
                                    jobpos = true;
                                }
                            }


                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in OPFEmployeeloanregstate)
                            {
                                if (ca.LoanAdvRequestPFT != null && ca.LoanAdvRequestPFT.Count() != 0)
                                {
                                    var OPFTEMPLEDData = ca.LoanAdvRequestPFT.Where(e => e.SanctionedDate >= pFromDate && e.SanctionedDate <= pToDate).ToList();

                                    if (OPFTEMPLEDData != null && OPFTEMPLEDData.Count() != 0)
                                    {
                                        int? geoid = ca.Employee.GeoStruct_Id;

                                        int? payid = ca.Employee.PayStruct_Id;

                                        int? funid = ca.Employee.FuncStruct_Id;

                                        GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                        PayStruct paystruct = db.PayStruct.Find(payid);

                                        FuncStruct funstruct = db.FuncStruct.Find(funid);

                                        GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                        if (GeoDataInd != null)
                                        {


                                            foreach (var ca1 in OPFTEMPLEDData)
                                            {
                                                var wfstatus = ca1.LoanWFDetails.OrderByDescending(e => e.Id).FirstOrDefault().WFStatus;
                                                string wf = "";
                                                if (wfstatus == 0)
                                                {
                                                    wf = "Applied";
                                                }
                                                if (wfstatus == 1)
                                                {
                                                    wf = "Sanction";
                                                }
                                                if (wfstatus == 2)
                                                {
                                                    wf = "Sanction Rejected";
                                                }

                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                {


                                                    Fld2 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                                    Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                    Fld4 = ca1.SanctionedDate.Value.Date.ToShortDateString(),
                                                    Fld5 = ca1.ChequeDate == null ? "" : ca1.ChequeDate.Value.Date.ToShortDateString(),
                                                    Fld6 = ca1.LoanSanctionedAmount.ToString(),
                                                    Fld7 = ca1.OwnPFAmount.ToString(),
                                                    Fld8 = ca1.OwnPFIntAmount.ToString(),
                                                    Fld9 = ca1.OwnerPFAmount.ToString(),
                                                    Fld10 = ca1.OwnerPFIntAmount.ToString(),
                                                    Fld11 = ca1.VPFAmount.ToString(),
                                                    Fld12 = ca1.VPFIntAmount.ToString(),
                                                    Fld13 = ca1.LoanAdvanceHeadPFT.Name.ToString(),
                                                    Fld14 = wf.ToString(),

                                                };

                                                //if (month)
                                                //{
                                                //    OGenericObjStatement.Fld100 = ca1.MonthYear.ToString();
                                                //}
                                                if (comp)
                                                {
                                                    OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                }
                                                if (dept)
                                                {
                                                    OGenericObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                }
                                                if (grp)
                                                {
                                                    OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                }
                                                if (grade)
                                                {
                                                    OGenericObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    OGenericObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    OGenericObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    OGenericObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                }
                                                if (emp)
                                                {
                                                    OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                }

                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                            }

                                        }

                                    }


                                }

                            }

                            return OGenericPayrollStatement;
                        }

                        break;

                    case "EMPLOYEEPFLOANHISTORY":

                        var OPFEmployeepfloanhistory = new List<EmployeePFTrust>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OPFEmployeepfloanhistory_temp = db.EmployeePFTrust
                                                                  .Include(e => e.Employee)
                                                                  .Where(e => e.Employee_Id == item)
                                                                  .AsNoTracking()
                                                                  .FirstOrDefault();
                            if (OPFEmployeepfloanhistory_temp != null)
                            {
                                OPFEmployeepfloanhistory_temp.Employee.EmpName = db.NameSingle.Find(OPFEmployeepfloanhistory_temp.Employee.EmpName_Id);
                                OPFEmployeepfloanhistory_temp.Employee.EmpOffInfo = db.EmpOff.Find(OPFEmployeepfloanhistory_temp.Employee.EmpOffInfo_Id);
                                OPFEmployeepfloanhistory_temp.LoanAdvRequestPFT = db.LoanAdvRequestPFT.Include(e => e.LoanAdvanceHeadPFT).Include(e => e.LoanWFDetails).Where(e => e.EmployeePFTrust_Id == OPFEmployeepfloanhistory_temp.Id).ToList();
                                OPFEmployeepfloanhistory.Add(OPFEmployeepfloanhistory_temp);
                            }
                        }

                        if (OPFEmployeepfloanhistory == null || OPFEmployeepfloanhistory.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            var emp = false;
                            var dept = false;
                            var loca = false;
                            var comp = false;
                            var grp = false;
                            var unit = false;
                            var div = false;
                            var regn = false;
                            var grade = false;
                            var lvl = false;
                            var jobstat = false;
                            var job = false;
                            var jobpos = false;
                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();


                            foreach (var item in vc)
                            {
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {

                                    loca = true;
                                }
                                if (item.LookupVal.ToUpper() == "EMPLOYEE")
                                {
                                    emp = true;
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                }
                                if (item.LookupVal.ToUpper() == "COMPANY")
                                {
                                    comp = true;
                                }
                                if (item.LookupVal.ToUpper() == "GROUP")
                                {
                                    grp = true;
                                }
                                if (item.LookupVal.ToUpper() == "UNIT")
                                {
                                    unit = true;
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                }
                                if (item.LookupVal.ToUpper() == "REGION")
                                {
                                    regn = true;
                                }
                                if (item.LookupVal.ToUpper() == "GRADE")
                                {
                                    grade = true;
                                }
                                if (item.LookupVal.ToUpper() == "LEVEL")
                                {
                                    lvl = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBSTATUS")
                                {
                                    jobstat = true;
                                }

                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBPOSITION")
                                {
                                    jobpos = true;
                                }
                            }
                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();
                            foreach (var data in OPFEmployeepfloanhistory)
                            {
                                if (data.LoanAdvRequestPFT != null && data.LoanAdvRequestPFT.Count() != 0)
                                {
                                    var OPEEmpLoanTemp = data.LoanAdvRequestPFT.Where(e => e.SanctionedDate >= pFromDate && e.SanctionedDate <= pToDate).ToList();

                                    if (OPEEmpLoanTemp != null && OPEEmpLoanTemp.Count() != 0)
                                    {
                                        int? geoid = data.Employee.GeoStruct_Id;

                                        int? payid = data.Employee.PayStruct_Id;

                                        int? funid = data.Employee.FuncStruct_Id;

                                        GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                        PayStruct paystruct = db.PayStruct.Find(payid);

                                        FuncStruct funstruct = db.FuncStruct.Find(funid);

                                        GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                        if (GeoDataInd != null)
                                        {


                                            foreach (var ca1 in OPEEmpLoanTemp)
                                            {


                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                {


                                                    Fld2 = data.Employee.EmpCode == null ? "" : data.Employee.EmpCode.ToString(),
                                                    Fld3 = data.Employee.EmpName.FullNameFML == null ? "" : data.Employee.EmpName.FullNameFML.ToString(),
                                                    //Fld4 = ca1.Narration == null ? "" : ca1.Narration.ToString(),
                                                    Fld4 = ca1.LoanAdvanceHeadPFT.Name == null ? "" : ca1.LoanAdvanceHeadPFT.Name.ToString(),
                                                    Fld5 = ca1.RequisitionDate.Value.Date.ToShortDateString(),
                                                    Fld8 = ca1.SanctionedDate.Value.Date.ToShortDateString(),
                                                    Fld13 = ca1.LoanSanctionedAmount.ToString(),
                                                };

                                                if (comp)
                                                {
                                                    OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                }
                                                if (dept)
                                                {
                                                    OGenericObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                }
                                                if (grp)
                                                {
                                                    OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                }
                                                if (grade)
                                                {
                                                    OGenericObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    OGenericObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    OGenericObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    OGenericObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                }
                                                if (emp)
                                                {
                                                    OGenericObjStatement.Fld88 = data.Employee.EmpName.FullNameFML;
                                                }

                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                            }

                                        }

                                    }


                                }

                            }

                            return OGenericPayrollStatement;
                        }

                        break;

                    #endregion PFEmpLedger code end

                    case "INTERVIEWDETAILS":
                        //   var cand = db.RecruitBatchInitiator.Include(e => e.ResumeCollection.Select(q => q.Candidate.Id)).ToList();
                        var Oschedule1 = db.RecruitInitiator
                            .Include(e => e.RecruitBatchInitiator)
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.PostDetails))
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.PostDetails.FuncStruct))
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.PostDetails.FuncStruct.Job))
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.PostDetails.Qualification))
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.ResumeCollection))
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.ResumeCollection.Select(t => t.RecruitJoinParaProcessResult)))
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.ResumeCollection.Select(t => t.Candidate)))
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.ResumeCollection.Select(y => y.Candidate.CanName)))
                            //      .Where(e=>e.ResumeCollection.Select(q => q.Candidate.Id!=null))
                                     .ToList();
                        if (Oschedule1 == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca1 in Oschedule1)
                            {
                                foreach (var ca in ca1.RecruitBatchInitiator)
                                {
                                    foreach (var ca2 in ca.ResumeCollection)
                                    {
                                        foreach (var ca3 in ca2.RecruitJoinParaProcessResult)
                                        {

                                            var qual = ca.PostDetails.Qualification.Select(r => r.QualificationDesc).FirstOrDefault();
                                            var post = ca.PostDetails.FuncStruct.Job.Name;
                                            var exp = ca2.YrsofExperience;
                                            var intvdate = ca2.InterviewDate != null ? ca2.InterviewDate.Value.ToShortDateString() : "";
                                            var canname = ca.ResumeCollection.Select(e => e.Candidate.CanName.FullNameFML).FirstOrDefault();
                                            var ActivityLetterIssue = ca3.ActivityLetterIssue;
                                            var Selected = ca3.ActivityAccepted;
                                            var ActivityAcceptedDate = ca3.ActivityAcceptedDate.Value.ToShortDateString();
                                            GenericField100 OGeneticApprStatement = new GenericField100()
                                            {
                                                Fld1 = ca.JobReferenceNo == null ? "" : ca.JobReferenceNo,//ref no
                                                Fld2 = ca.JobReferenceNo.ToString(), //reqi no
                                                Fld3 = post == null ? "" : post,//post for
                                                Fld4 = qual == null ? "" : qual,//qual
                                                Fld5 = exp == null ? "" : exp.ToString(), //exp
                                                Fld6 = ca1.AdvertiseReferenceNo == null ? "" : ca1.AdvertiseReferenceNo,    //appln no
                                                Fld7 = canname == null ? "" : canname,//candidate name
                                                Fld8 = intvdate == null ? "" : intvdate,//Interview Date
                                                //Fld9 = ca2.//   Is Interview Taken 
                                                Fld10 = Selected == null ? "" : Selected.ToString(),// Is Selected
                                                Fld11 = ActivityLetterIssue == null ? "" : ActivityLetterIssue.ToString(),// Offer Letter Give
                                                Fld12 = Selected == null ? "" : Selected.ToString(),//Accepted
                                                Fld13 = ActivityAcceptedDate == null ? "" : ActivityAcceptedDate.ToString()// Praposed Join Date 

                                            };
                                            OGenericPayrollStatement.Add(OGeneticApprStatement);
                                        }
                                    }
                                }
                            }
                        }
                        return OGenericPayrollStatement;
                        break;


                    case "CANDIDATEMASTER":
                        // recurt 
                        var Candidateemp = new List<Candidate>();
                        foreach (var cid in EmpPayrollIdList)
                        {


                            var OCandidateMaster = db.Candidate
                                //.Include(e => e.BeforeMarriageName)
                                .Include(e => e.CanName)
                                .Include(e => e.CorAddr)
                                .Include(e => e.PerAddr)
                                .Include(e => e.ResAddr)
                                //.Include(e => e.FatherName)
                                //.Include(e => e.HusbandName)
                                //.Include(e => e.MotherName)
                                .Include(e => e.MaritalStatus)
                                .Include(e => e.CorContact)
                                .Include(e => e.PerContact)
                                .Include(e => e.ResContact)
                                .Include(e => e.Gender)
                                 .Include(a => a.CanAcademicInfo)
                                .Include(e => e.CanName)
                                .Include(e => e.CanAcademicInfo.QualificationDetails)
                                .Include(e => e.CanAcademicInfo.QualificationDetails.Select(t => t.Qualification))
                               .Include(e => e.CanAcademicInfo.QualificationDetails.Select(t => t.Qualification.Select(y => y.QualificationType)))
                               .Where(x => x.Id == cid)
                                .AsNoTracking()
                              .FirstOrDefault();

                            if (OCandidateMaster != null)
                            {
                                Candidateemp.Add(OCandidateMaster);
                            }

                        }

                        if (Candidateemp == null)
                        {
                            return null;
                        }
                        else
                        {
                            var qualif = "";
                            foreach (var ca in Candidateemp)
                            {
                                if (SpecialGroupslist.Count() == 0 && salheadlist.Count() == 0)
                                {
                                    var caq = ca.CanAcademicInfo.QualificationDetails.ToList();
                                    if (caq != null)
                                    {
                                        foreach (var item in caq)
                                        {
                                            var aa = item.Qualification.ToList();
                                            foreach (var item1 in aa)
                                            {
                                                qualif = item1.QualificationShortName.ToString();
                                                GenericField100 OGeneticApprStatement = new GenericField100()
                                                {
                                                    Fld1 = ca.CanCode,
                                                    Fld4 = ca.CanName != null ? ca.CanName.FullNameFML : null,
                                                    Fld9 = ca.Gender != null ? ca.Gender.LookupVal : null,
                                                    Fld10 = ca.MaritalStatus != null ? ca.MaritalStatus.LookupVal : null,
                                                    Fld11 = ca.ResAddr != null ? ca.ResAddr.FullAddress : null,
                                                    Fld12 = ca.PerAddr != null ? ca.PerAddr.FullAddress : null,
                                                    Fld13 = ca.CorAddr != null ? ca.CorAddr.FullAddress : null,
                                                    Fld14 = ca.CorContact != null ? ca.CorContact.FullContactDetails : null,
                                                    Fld15 = ca.PerContact != null ? ca.PerContact.FullContactDetails : null,
                                                    Fld16 = ca.ResContact != null ? ca.ResContact.FullContactDetails : null,
                                                    Fld17 = qualif == null ? "" : qualif

                                                };
                                                OGenericPayrollStatement.Add(OGeneticApprStatement);
                                            }
                                        }
                                    }
                                }
                                else if (SpecialGroupslist.Count() == 0 && salheadlist.Count() != 0)
                                {
                                    var caq = ca.CanAcademicInfo.QualificationDetails.ToList();
                                    if (caq != null)
                                    {
                                        foreach (var item in caq)
                                        {
                                            if (salheadlist.Count() > 0)
                                            {
                                                foreach (var quali in salheadlist)
                                                {
                                                    var aa = item.Qualification.Where(e => e.QualificationShortName.ToUpper() == quali.ToUpper().ToString()).ToList();

                                                    foreach (var item1 in aa)
                                                    {
                                                        qualif = item1.QualificationShortName.ToString();
                                                        GenericField100 OGeneticApprStatement = new GenericField100()
                                                        {
                                                            Fld1 = ca.CanCode,
                                                            Fld4 = ca.CanName != null ? ca.CanName.FullNameFML : null,
                                                            Fld9 = ca.Gender != null ? ca.Gender.LookupVal : null,
                                                            Fld10 = ca.MaritalStatus != null ? ca.MaritalStatus.LookupVal : null,
                                                            Fld11 = ca.ResAddr != null ? ca.ResAddr.FullAddress : null,
                                                            Fld12 = ca.PerAddr != null ? ca.PerAddr.FullAddress : null,
                                                            Fld13 = ca.CorAddr != null ? ca.CorAddr.FullAddress : null,
                                                            Fld14 = ca.CorContact != null ? ca.CorContact.FullContactDetails : null,
                                                            Fld15 = ca.PerContact != null ? ca.PerContact.FullContactDetails : null,
                                                            Fld16 = ca.ResContact != null ? ca.ResContact.FullContactDetails : null,
                                                            Fld17 = qualif == null ? "" : qualif

                                                        };
                                                        OGenericPayrollStatement.Add(OGeneticApprStatement);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (SpecialGroupslist.Count() != 0 && salheadlist.Count() == 0)
                                {
                                    if (SpecialGroupslist.Count() > 0)
                                    {
                                        foreach (var item3 in SpecialGroupslist)
                                        {
                                            if (ca.Gender != null && ca.Gender.LookupVal.ToUpper().ToString() == item3.ToUpper().ToString())
                                            {
                                                var caq = ca.CanAcademicInfo.QualificationDetails.ToList();
                                                if (caq != null)
                                                {
                                                    foreach (var item in caq)
                                                    {
                                                        var aa = item.Qualification.ToList();
                                                        foreach (var item1 in aa)
                                                        {
                                                            qualif = item1.QualificationShortName.ToString();
                                                            GenericField100 OGeneticApprStatement = new GenericField100()
                                                            {
                                                                Fld1 = ca.CanCode,
                                                                Fld4 = ca.CanName != null ? ca.CanName.FullNameFML : null,
                                                                Fld9 = ca.Gender != null ? ca.Gender.LookupVal : null,
                                                                Fld10 = ca.MaritalStatus != null ? ca.MaritalStatus.LookupVal : null,
                                                                Fld11 = ca.ResAddr != null ? ca.ResAddr.FullAddress : null,
                                                                Fld12 = ca.PerAddr != null ? ca.PerAddr.FullAddress : null,
                                                                Fld13 = ca.CorAddr != null ? ca.CorAddr.FullAddress : null,
                                                                Fld14 = ca.CorContact != null ? ca.CorContact.FullContactDetails : null,
                                                                Fld15 = ca.PerContact != null ? ca.PerContact.FullContactDetails : null,
                                                                Fld16 = ca.ResContact != null ? ca.ResContact.FullContactDetails : null,
                                                                Fld17 = qualif == null ? "" : qualif

                                                            };
                                                            OGenericPayrollStatement.Add(OGeneticApprStatement);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (SpecialGroupslist.Count() != 0 && salheadlist.Count() != 0)
                                {
                                    if (SpecialGroupslist.Count() > 0)
                                    {
                                        foreach (var item3 in SpecialGroupslist)
                                        {
                                            if (ca.Gender != null && ca.Gender.LookupVal.ToUpper().ToString() == item3.ToUpper().ToString())
                                            {
                                                var caq = ca.CanAcademicInfo.QualificationDetails.ToList();
                                                if (caq != null)
                                                {
                                                    foreach (var item in caq)
                                                    {
                                                        if (salheadlist.Count() > 0)
                                                        {
                                                            foreach (var quali in salheadlist)
                                                            {
                                                                var aa = item.Qualification.Where(e => e.QualificationShortName.ToUpper() == quali.ToUpper().ToString()).ToList();

                                                                foreach (var item1 in aa)
                                                                {
                                                                    qualif = item1.QualificationShortName.ToString();
                                                                    GenericField100 OGeneticApprStatement = new GenericField100()
                                                                    {
                                                                        Fld1 = ca.CanCode,
                                                                        Fld4 = ca.CanName != null ? ca.CanName.FullNameFML : null,
                                                                        Fld9 = ca.Gender != null ? ca.Gender.LookupVal : null,
                                                                        Fld10 = ca.MaritalStatus != null ? ca.MaritalStatus.LookupVal : null,
                                                                        Fld11 = ca.ResAddr != null ? ca.ResAddr.FullAddress : null,
                                                                        Fld12 = ca.PerAddr != null ? ca.PerAddr.FullAddress : null,
                                                                        Fld13 = ca.CorAddr != null ? ca.CorAddr.FullAddress : null,
                                                                        Fld14 = ca.CorContact != null ? ca.CorContact.FullContactDetails : null,
                                                                        Fld15 = ca.PerContact != null ? ca.PerContact.FullContactDetails : null,
                                                                        Fld16 = ca.ResContact != null ? ca.ResContact.FullContactDetails : null,
                                                                        Fld17 = qualif == null ? "" : qualif

                                                                    };
                                                                    OGenericPayrollStatement.Add(OGeneticApprStatement);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }


                            }
                        }
                        return OGenericPayrollStatement;
                        break;
                    case "MANPOWERYEARLYBUDGET":
                        //var cid = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "RECRUITMENTCALENDAR" && e.FromDate>=pFromDate && e.ToDate<=pToDate).ToList().Select(z=>z.Id);
                        var cidr = db.RecruitYearlyCalendar.Include(e => e.RecruitmentCalendar).Where(e => e.RecruitmentCalendar.FromDate >= pFromDate && e.RecruitmentCalendar.ToDate <= pToDate).ToList().Select(z => z.Id);
                        if (cidr.Count() > 0)
                        {
                            var OTOTEmpdedData1_Temp11 = new List<RecruitYearlyCalendar>();

                            foreach (var item in cidr)
                            {
                                var manyearlybudget = db.RecruitYearlyCalendar.Include(e => e.ManPowerBudget)
                                .Include(e => e.ManPowerBudget.Select(x => x.GeoStruct))
                                .Include(e => e.ManPowerBudget.Select(x => x.GeoStruct.Location))
                                .Include(e => e.ManPowerBudget.Select(x => x.GeoStruct.Location.LocationObj))
                                 .Include(e => e.ManPowerBudget.Select(x => x.FuncStruct))
                                 .Include(e => e.ManPowerBudget.Select(x => x.FuncStruct.JobPosition))
                                  .Include(e => e.ManPowerBudget.Select(x => x.PayStruct))
                                  .Include(e => e.ManPowerBudget.Select(x => x.PayStruct.Grade))
                             .Where(e => e.Id == item)
                              .SingleOrDefault();
                                if (manyearlybudget != null)
                                {
                                    OTOTEmpdedData1_Temp11.Add(manyearlybudget);
                                }

                            }
                            if (OTOTEmpdedData1_Temp11 == null || OTOTEmpdedData1_Temp11.Count() == 0)
                            {

                                return null;
                            }
                            else
                            {
                                foreach (var item in OTOTEmpdedData1_Temp11)
                                {
                                    var budg = item.ManPowerBudget.ToList();
                                    foreach (var item1 in budg)
                                    {
                                        GenericField100 OGeneticApprStatement = new GenericField100()
                                        {
                                            Fld1 = item.RecruitmentCalendar.FromDate.Value.ToShortDateString(),
                                            Fld2 = item.RecruitmentCalendar.ToDate.Value.ToShortDateString(),
                                            Fld3 = item1.SanctionedPosts.ToString(),
                                            Fld4 = item1.BudgetAmount.ToString(),
                                            Fld5 = item1.GeoStruct == null ? "" : item1.GeoStruct.Location.LocationObj.LocDesc.ToString(),
                                            Fld6 = item1.PayStruct == null ? "" : item1.PayStruct.Grade == null ? "" : item1.PayStruct.Grade.Name.ToString(),
                                            Fld7 = item1.FuncStruct == null ? "" : item1.FuncStruct.JobPosition == null ? "" : item1.FuncStruct.JobPosition.JobPositionDesc.ToString()


                                        };
                                        OGenericPayrollStatement.Add(OGeneticApprStatement);

                                    }

                                }
                            }


                        }

                        return OGenericPayrollStatement;
                        break;
                    case "MANPOWERANALYSIS":
                        if (salheadlist.Count > 0)
                        {
                            foreach (var batch in salheadlist)
                            {
                                var manpoweranalysis = db.ManPowerDetailsBatch
                                    .Include(e => e.ManPowerPostData)
                                    .Include(e => e.ManPowerPostData.Select(x => x.ManPowerBudget))
                                    .Include(e => e.ManPowerPostData.Select(x => x.ManPowerBudget.GeoStruct.Location.LocationObj))
                                    .Include(e => e.ManPowerPostData.Select(x => x.ManPowerBudget.PayStruct.Grade))
                                    .Include(e => e.ManPowerPostData.Select(x => x.ManPowerBudget.FuncStruct.JobPosition))
                                    .Where(z => z.ProcessDate >= pFromDate && z.ProcessDate <= pToDate && z.BatchName.ToUpper() == batch.ToUpper().ToString())
                                    .ToList();
                                if (manpoweranalysis == null)
                                {
                                    return null;
                                }
                                else
                                {
                                    foreach (var item in manpoweranalysis)
                                    {
                                        var manpanalysis = item.ManPowerPostData.ToList();
                                        foreach (var item1 in manpanalysis)
                                        {


                                            GenericField100 OGeneticApprStatement = new GenericField100()
                                            {
                                                Fld1 = item.BatchName.ToString(),
                                                Fld2 = item.ProcessDate.Value.ToShortDateString(),
                                                Fld5 = item1.ManPowerBudget.GeoStruct == null ? "" : item1.ManPowerBudget.GeoStruct.Location.LocationObj.LocDesc.ToString(),
                                                Fld6 = item1.ManPowerBudget.PayStruct == null ? "" : item1.ManPowerBudget.PayStruct.Grade == null ? "" : item1.ManPowerBudget.PayStruct.Grade.Name.ToString(),
                                                Fld7 = item1.ManPowerBudget.FuncStruct == null ? "" : item1.ManPowerBudget.FuncStruct.JobPosition == null ? "" : item1.ManPowerBudget.FuncStruct.JobPosition.JobPositionDesc.ToString(),
                                                Fld3 = item1.CurrentCTC.ToString(),
                                                Fld4 = item1.BudgetCTC.ToString(),
                                                Fld8 = item1.ExcessCTC.ToString(),
                                                Fld9 = item1.TotalCTC.ToString(),
                                                Fld10 = item1.SanctionedPosts.ToString(),
                                                Fld11 = item1.FilledPosts.ToString(),
                                                Fld12 = item1.ExcessPosts.ToString(),
                                                Fld13 = item1.VacantPosts.ToString()
                                            };
                                            OGenericPayrollStatement.Add(OGeneticApprStatement);
                                        }
                                    }
                                }
                            }
                        }

                        return OGenericPayrollStatement;
                        break;
                    case "MANPOWERREQUISITION":
                        var manpowerreq = db.ManpowerRequestPost
                            .Include(e => e.PostSourceType)
                             .Include(e => e.FuncStruct)
                             .Include(e => e.FuncStruct.Job)
                            .Where(e => e.PostRequestDate >= pFromDate && e.PostRequestDate <= pToDate)
                            .ToList();
                        if (manpowerreq == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var item in manpowerreq)
                            {
                                GenericField100 OGeneticApprStatement = new GenericField100()
                                          {
                                              Fld1 = item.PostRequestDate.Value.ToShortDateString(),
                                              Fld2 = item.PostSourceType.LookupVal.ToString(),
                                              Fld3 = item.PostCode.ToString(),
                                              Fld4 = item.FuncStruct.Job == null ? "" : item.FuncStruct.Job.Name.ToString(),
                                              Fld5 = item.RequestVacancies.ToString(),
                                              Fld6 = item.ExpYearFrom.ToString(),
                                              Fld7 = item.ExpYearTo.ToString(),
                                              Fld8 = item.AgeFrom.ToString(),
                                              Fld9 = item.AgeTo.ToString()

                                          };
                                OGenericPayrollStatement.Add(OGeneticApprStatement);
                            }
                        }
                        return OGenericPayrollStatement;
                        break;
                    case "MANPOWERPOSTCREATION":
                        var manpowerpostcreation = db.PostDetails

                             .Include(e => e.FuncStruct)
                             .Include(e => e.FuncStruct.Job)
                            .Where(e => e.CreationDate >= pFromDate && e.CreationDate <= pToDate)
                            .ToList();
                        if (manpowerpostcreation == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var item in manpowerpostcreation)
                            {
                                GenericField100 OGeneticApprStatement = new GenericField100()
                                {
                                    Fld1 = item.CreationDate.Value.ToShortDateString(),
                                    Fld2 = item.PostCode.ToString(),
                                    Fld3 = item.FuncStruct.Job == null ? "" : item.FuncStruct.Job.Name.ToString(),
                                    Fld4 = item.RequestVacancies.ToString(),
                                    Fld5 = item.ExpYearFrom.ToString(),
                                    Fld6 = item.ExpYearTo.ToString(),
                                    Fld7 = item.AgeFrom.ToString(),
                                    Fld8 = item.AgeTo.ToString()

                                };
                                OGenericPayrollStatement.Add(OGeneticApprStatement);
                            }
                        }
                        return OGenericPayrollStatement;
                        break;


                    case "MANPOWEREMPLOYEEWISECTC":
                        if (salheadlist.Count > 0)
                        {
                            foreach (var batch in salheadlist)
                            {
                                var manpoweranalysis = db.ManPowerDetailsBatch
                                    .Include(e => e.ManPowerPostData)
                                    .Include(e => e.ManPowerPostData.Select(x => x.ManPowerBudget))
                                    .Include(e => e.ManPowerPostData.Select(x => x.ManPowerBudget.GeoStruct.Location.LocationObj))
                                    .Include(e => e.ManPowerPostData.Select(x => x.ManPowerBudget.GeoStruct.Department.DepartmentObj))
                                    .Include(e => e.ManPowerPostData.Select(x => x.ManPowerBudget.FuncStruct.Job))
                                    .Where(z => z.ProcessDate >= pFromDate && z.ProcessDate <= pToDate && z.BatchName.ToUpper() == batch.ToUpper().ToString())
                                    .ToList();
                                if (manpoweranalysis.Count() > 0)
                                {
                                    foreach (var item in manpoweranalysis)
                                    {
                                        foreach (var item1 in item.ManPowerPostData)
                                        {
                                            List<int> FilledPostemp = db.Employee
                            .Include(a => a.FuncStruct)
                            .Include(a => a.GeoStruct)
                            .Include(a => a.ServiceBookDates)
                            .Where(e => e.FuncStruct.Id == item1.ManPowerBudget.FuncStruct.Id && e.GeoStruct.Id == item1.ManPowerBudget.GeoStruct.Id && e.ServiceBookDates.ServiceLastDate == null).Select(q => q.Id).ToList();

                                            var employeepayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).Include(e => e.Employee.FuncStruct).Where(q => FilledPostemp.Contains(q.Employee.Id)).ToList();
                                            foreach (var emppayroll in employeepayroll)
                                            {
                                                var EmpSalStructobj = db.EmpSalStruct
                                      .Include(e => e.EmpSalStructDetails)
                                    .Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency))
                                    .Where(e => e.EndDate == null && e.EmployeePayroll_Id == emppayroll.Id).FirstOrDefault();

                                                if (EmpSalStructobj != null)
                                                {
                                                    double distribution = 0;
                                                    var dataempload = db.CTCDefinition.Include(r => r.SalaryHead).SelectMany(e => e.SalaryHead).ToList();
                                                    var datapass = dataempload.Select(r => r.Id);
                                                    var empsalstructdetails = EmpSalStructobj.EmpSalStructDetails.Where(e => datapass.Contains(e.SalaryHead.Id) && e.Amount != 0).OrderBy(e => e.SalaryHead.SeqNo).ToList();
                                                    foreach (var data3 in empsalstructdetails)
                                                    {
                                                        if (data3.SalaryHead.Frequency.LookupVal.ToString().ToUpper() == "MONTHLY")
                                                        {
                                                            distribution = data3.Amount * 12;
                                                        }
                                                        else if (data3.SalaryHead.Frequency.LookupVal.ToString().ToUpper() == "HALFYEARLY")
                                                        {
                                                            distribution = data3.Amount * 2;
                                                        }
                                                        else if (data3.SalaryHead.Frequency.LookupVal.ToString().ToUpper() == "QUARTERLY")
                                                        {
                                                            distribution = data3.Amount * 4;
                                                        }
                                                        else
                                                        {
                                                            distribution = data3.Amount;
                                                        }
                                                        GenericField100 OGeneticApprStatement = new GenericField100()
                                                        {
                                                            Fld7 = item.BatchName.ToString(),
                                                            Fld1 = emppayroll.Employee.EmpCode,
                                                            Fld2 = emppayroll.Employee.EmpName.FullNameFML,
                                                            Fld3 = item1.ManPowerBudget.GeoStruct.Location.LocationObj.LocDesc,
                                                            Fld4 = item1.ManPowerBudget.FuncStruct.Job.Code + " " + item1.ManPowerBudget.FuncStruct.Job.Name.ToString(),
                                                            Fld6 = data3.SalaryHead.Name.ToString(),
                                                            Fld5 = data3.Amount.ToString(),
                                                            Fld8 = item1.ManPowerBudget.GeoStruct.Department.DepartmentObj.DeptDesc == null ? "" : item1.ManPowerBudget.GeoStruct.Department.DepartmentObj.DeptDesc,
                                                            Fld9 = distribution.ToString(),
                                                            Fld20 = data3.SalaryHead.SeqNo.ToString(),
                                                        };
                                                        OGenericPayrollStatement.Add(OGeneticApprStatement);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        return OGenericPayrollStatement;
                        break;

                    case "MANPOWERANALYSISCTCSUMMARY":
                        if (salheadlist.Count > 0)
                        {

                            foreach (var batch in salheadlist)
                            {
                                string[] values = batch.Split(new string[] { ":" }, StringSplitOptions.None);
                                string locationName = values[0].Trim();
                                var manpoweranalysis = db.ManPowerDetailsBatch
                                    .Include(e => e.ManPowerPostData)
                                    .Include(e => e.ManPowerPostData.Select(x => x.ManPowerBudget))
                                    .Include(e => e.ManPowerPostData.Select(x => x.ManPowerBudget.GeoStruct.Location.LocationObj))
                                    .Include(e => e.ManPowerPostData.Select(x => x.ManPowerBudget.GeoStruct.Department.DepartmentObj))
                                    .Include(e => e.ManPowerPostData.Select(x => x.ManPowerBudget.FuncStruct.Job))
                                    .Where(z => z.ProcessDate >= pFromDate && z.ProcessDate <= pToDate)
                                    .Where(z => z.ManPowerPostData.Any(e => e.ManPowerBudget.GeoStruct.Location.LocationObj.LocDesc == locationName))
                                    .ToList();


                                if (manpoweranalysis.Count() > 0)
                                {
                                    foreach (var item in manpoweranalysis)
                                    {
                                        foreach (var item1 in item.ManPowerPostData)
                                        {
                                            if (salheadlistLevel1.Count() > 0)
                                            {
                                                foreach (var list1 in salheadlistLevel1)
                                                {
                                                    if (item1.ManPowerBudget.FuncStruct.Job.Name.ToUpper() == list1.ToUpper())
                                                    {
                                                        List<int> FilledPostemp = db.Employee.Include(a => a.FuncStruct).Include(a => a.GeoStruct).Include(a => a.ServiceBookDates)
                                        .Where(e => e.FuncStruct.Id == item1.ManPowerBudget.FuncStruct.Id && e.GeoStruct.Id == item1.ManPowerBudget.GeoStruct.Id && e.ServiceBookDates.ServiceLastDate == null).Select(q => q.Id).ToList();

                                                        var employeepayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.FuncStruct).Where(q => FilledPostemp.Contains(q.Employee.Id)).ToList();
                                                        foreach (var emppayroll in employeepayroll)
                                                        {
                                                            var EmpSalStructobj = db.EmpSalStruct
                                                  .Include(e => e.EmpSalStructDetails)
                                                .Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency))
                                                .Where(e => e.EndDate == null && e.EmployeePayroll_Id == emppayroll.Id).FirstOrDefault();
                                                            if (EmpSalStructobj != null)
                                                            {
                                                                double distribution = 0;
                                                                var dataempload = db.CTCDefinition.Include(r => r.SalaryHead).SelectMany(e => e.SalaryHead).ToList();
                                                                var datapass = dataempload.Select(r => r.Id);
                                                                var empsalstructdetails = EmpSalStructobj.EmpSalStructDetails.Where(e => datapass.Contains(e.SalaryHead.Id) && e.Amount != 0).OrderBy(e => e.SalaryHead.SeqNo).ToList();
                                                                foreach (var data3 in empsalstructdetails)
                                                                {
                                                                    if (data3.SalaryHead.Frequency.LookupVal.ToString().ToUpper() == "MONTHLY")
                                                                    {
                                                                        distribution = data3.Amount * 12;
                                                                    }
                                                                    else if (data3.SalaryHead.Frequency.LookupVal.ToString().ToUpper() == "HALFYEARLY")
                                                                    {
                                                                        distribution = data3.Amount * 2;
                                                                    }
                                                                    else if (data3.SalaryHead.Frequency.LookupVal.ToString().ToUpper() == "QUARTERLY")
                                                                    {
                                                                        distribution = data3.Amount * 4;
                                                                    }
                                                                    else
                                                                    {
                                                                        distribution = data3.Amount;
                                                                    }
                                                                    GenericField100 OGeneticApprStatement = new GenericField100()
                                                                    {
                                                                        //Fld7 = item.BatchName.ToString(),
                                                                        Fld3 = item1.ManPowerBudget.GeoStruct.Location.LocationObj.LocDesc,
                                                                        Fld4 = item1.ManPowerBudget.FuncStruct.Job.Code+" "+ item1.ManPowerBudget.FuncStruct.Job.Name.ToString(),
                                                                        Fld6 = data3.SalaryHead.Name.ToString(),
                                                                        Fld5 = data3.Amount.ToString(),
                                                                        Fld8 = item1.ManPowerBudget.GeoStruct.Department.DepartmentObj.DeptDesc == null ? "" : item1.ManPowerBudget.GeoStruct.Department.DepartmentObj.DeptDesc,
                                                                        Fld9 = distribution.ToString(),
                                                                        Fld20 = data3.SalaryHead.SeqNo.ToString(),
                                                                        Fld21 = emppayroll.Employee.EmpCode,
                                                                    };
                                                                    OGenericPayrollStatement.Add(OGeneticApprStatement);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                List<int> FilledPostemp = db.Employee.Include(a => a.FuncStruct).Include(a => a.GeoStruct).Include(a => a.ServiceBookDates)
                                                .Where(e => e.FuncStruct.Id == item1.ManPowerBudget.FuncStruct.Id && e.GeoStruct.Id == item1.ManPowerBudget.GeoStruct.Id && e.ServiceBookDates.ServiceLastDate == null).Select(q => q.Id).ToList();

                                                var employeepayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.FuncStruct).Where(q => FilledPostemp.Contains(q.Employee.Id)).ToList();
                                                foreach (var emppayroll in employeepayroll)
                                                {
                                                    var EmpSalStructobj = db.EmpSalStruct
                                          .Include(e => e.EmpSalStructDetails)
                                        .Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency))
                                        .Where(e => e.EndDate == null && e.EmployeePayroll_Id == emppayroll.Id).FirstOrDefault();
                                                    if (EmpSalStructobj != null)
                                                    {
                                                        double distribution = 0;
                                                        var dataempload = db.CTCDefinition.Include(r => r.SalaryHead).SelectMany(e => e.SalaryHead).ToList();
                                                        var datapass = dataempload.Select(r => r.Id);
                                                        var empsalstructdetails = EmpSalStructobj.EmpSalStructDetails.Where(e => datapass.Contains(e.SalaryHead.Id) && e.Amount != 0).OrderBy(e => e.SalaryHead.SeqNo).ToList();
                                                        foreach (var data3 in empsalstructdetails)
                                                        {
                                                            if (data3.SalaryHead.Frequency.LookupVal.ToString().ToUpper() == "MONTHLY")
                                                            {
                                                                distribution = data3.Amount * 12;
                                                            }
                                                            else if (data3.SalaryHead.Frequency.LookupVal.ToString().ToUpper() == "HALFYEARLY")
                                                            {
                                                                distribution = data3.Amount * 2;
                                                            }
                                                            else if (data3.SalaryHead.Frequency.LookupVal.ToString().ToUpper() == "QUARTERLY")
                                                            {
                                                                distribution = data3.Amount * 4;
                                                            }
                                                            else
                                                            {
                                                                distribution = data3.Amount;
                                                            }
                                                            GenericField100 OGeneticApprStatement = new GenericField100()
                                                            {
                                                                //Fld7 = item.BatchName.ToString(),
                                                                Fld3 = item1.ManPowerBudget.GeoStruct.Location.LocationObj.LocDesc,
                                                                Fld4 = item1.ManPowerBudget.FuncStruct.Job.Code + " " + item1.ManPowerBudget.FuncStruct.Job.Name.ToString(),
                                                                Fld6 = data3.SalaryHead.Name.ToString(),
                                                                Fld5 = data3.Amount.ToString(),
                                                                Fld8 = item1.ManPowerBudget.GeoStruct.Department.DepartmentObj.DeptDesc == null ? "" : item1.ManPowerBudget.GeoStruct.Department.DepartmentObj.DeptDesc,
                                                                Fld9 = distribution.ToString(),
                                                                Fld20 = data3.SalaryHead.SeqNo.ToString(),
                                                                Fld21 = emppayroll.Employee.EmpCode,
                                                            };
                                                            OGenericPayrollStatement.Add(OGeneticApprStatement);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            var manpoweranalysis = db.ManPowerDetailsBatch
                                .Include(e => e.ManPowerPostData)
                                .Include(e => e.ManPowerPostData.Select(x => x.ManPowerBudget))
                                .Include(e => e.ManPowerPostData.Select(x => x.ManPowerBudget.GeoStruct.Location.LocationObj))
                                .Include(e => e.ManPowerPostData.Select(x => x.ManPowerBudget.GeoStruct.Department.DepartmentObj))
                                .Include(e => e.ManPowerPostData.Select(x => x.ManPowerBudget.FuncStruct.Job))
                                .Where(z => z.ProcessDate >= pFromDate && z.ProcessDate <= pToDate)
                                .ToList();

                            if (salheadlistLevel1.Count() > 0)
                            {
                                manpoweranalysis = manpoweranalysis.Where(z => salheadlistLevel1.Contains(z.ManPowerPostData.SelectMany(S =>S.ManPowerBudget.FuncStruct.Job.Name))).ToList();
                            }
                            else
                            {
                                manpoweranalysis = manpoweranalysis.ToList();
                            }
                            if (manpoweranalysis.Count() > 0)
                            {
                                foreach (var item in manpoweranalysis)
                                {
                                    foreach (var item1 in item.ManPowerPostData)
                                    {
                                        List<int> FilledPostemp = db.Employee.Include(a => a.FuncStruct).Include(a => a.GeoStruct).Include(a => a.ServiceBookDates)
                        .Where(e => e.FuncStruct.Id == item1.ManPowerBudget.FuncStruct.Id && e.GeoStruct.Id == item1.ManPowerBudget.GeoStruct.Id && e.ServiceBookDates.ServiceLastDate == null).Select(q => q.Id).ToList();

                                        var employeepayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.FuncStruct).Where(q => FilledPostemp.Contains(q.Employee.Id)).ToList();
                                        foreach (var emppayroll in employeepayroll)
                                        {
                                            var EmpSalStructobj = db.EmpSalStruct
                                  .Include(e => e.EmpSalStructDetails)
                                .Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency))
                                .Where(e => e.EndDate == null && e.EmployeePayroll_Id == emppayroll.Id).FirstOrDefault();
                                            if (EmpSalStructobj != null)
                                            {
                                                double distribution = 0;
                                                var dataempload = db.CTCDefinition.Include(r => r.SalaryHead).SelectMany(e => e.SalaryHead).ToList();
                                                var datapass = dataempload.Select(r => r.Id);
                                                var empsalstructdetails = EmpSalStructobj.EmpSalStructDetails.Where(e => datapass.Contains(e.SalaryHead.Id) && e.Amount != 0).OrderBy(e => e.SalaryHead.SeqNo).ToList();
                                                foreach (var data3 in empsalstructdetails)
                                                {
                                                    if (data3.SalaryHead.Frequency.LookupVal.ToString().ToUpper() == "MONTHLY")
                                                    {
                                                        distribution = data3.Amount * 12;
                                                    }
                                                    else if (data3.SalaryHead.Frequency.LookupVal.ToString().ToUpper() == "HALFYEARLY")
                                                    {
                                                        distribution = data3.Amount * 2;
                                                    }
                                                    else if (data3.SalaryHead.Frequency.LookupVal.ToString().ToUpper() == "QUARTERLY")
                                                    {
                                                        distribution = data3.Amount * 4;
                                                    }
                                                    else
                                                    {
                                                        distribution = data3.Amount;
                                                    }
                                                    GenericField100 OGeneticApprStatement = new GenericField100()
                                                    {
                                                        Fld3 = item1.ManPowerBudget.GeoStruct.Location.LocationObj.LocDesc,
                                                        Fld4 = item1.ManPowerBudget.FuncStruct.Job.Code + " " + item1.ManPowerBudget.FuncStruct.Job.Name.ToString(),
                                                        Fld6 = data3.SalaryHead.Name.ToString(),
                                                        Fld5 = data3.Amount.ToString(),
                                                        Fld8 = item1.ManPowerBudget.GeoStruct.Department.DepartmentObj.DeptDesc == null ? "" : item1.ManPowerBudget.GeoStruct.Department.DepartmentObj.DeptDesc,
                                                        Fld9 = distribution.ToString(),
                                                        Fld20 = data3.SalaryHead.SeqNo.ToString(),
                                                        Fld21 = emppayroll.Employee.EmpCode,
                                                    };
                                                    OGenericPayrollStatement.Add(OGeneticApprStatement);
                                                }
                                            }
                                        }
                                    }
                                }
                            }



                        }
                        return OGenericPayrollStatement;
                        break;

                    case "RECRUITMENTBATCHINITIATOR":
                        if (salheadlist.Count > 0)
                        {
                            foreach (var jobref in salheadlist)
                            {
                                var Recruitbatchcreation = db.RecruitBatchInitiator
                                    .Include(e => e.PostDetails)

                                    .Include(e => e.PostDetails.FuncStruct)
                                    .Include(e => e.PostDetails.FuncStruct.Job)

                                    .Where(e => e.initiatedDate >= pFromDate && e.initiatedDate <= pToDate && e.JobReferenceNo == jobref)
                                    .ToList();
                                if (Recruitbatchcreation == null)
                                {
                                    return null;
                                }
                                else
                                {
                                    foreach (var item in Recruitbatchcreation)
                                    {

                                        GenericField100 OGeneticApprStatement = new GenericField100()
                                        {
                                            Fld1 = item.initiatedDate.Value.ToShortDateString(),
                                            Fld2 = item.PublishDate != null ? item.PublishDate.Value.ToShortDateString() : null,
                                            Fld3 = item.BatchCloseDate != null ? item.BatchCloseDate.Value.ToShortDateString() : null,
                                            Fld4 = item.PostDetails == null ? "" : item.PostDetails.CreationDate == null ? "" : item.PostDetails.CreationDate.Value.ToShortDateString(),
                                            Fld5 = item.PostDetails == null ? "" : item.PostDetails.PostCode.ToString(),
                                            Fld6 = item.PostDetails == null ? "" : item.PostDetails.FuncStruct == null ? "" : item.PostDetails.FuncStruct.Job == null ? "" : item.PostDetails.FuncStruct.Job.Name.ToString(),
                                            Fld7 = item.PostDetails == null ? "" : item.PostDetails.RequestVacancies.ToString(),
                                            Fld8 = item.PostDetails == null ? "" : item.PostDetails.ExpYearFrom.ToString(),
                                            Fld9 = item.PostDetails == null ? "" : item.PostDetails.ExpYearTo.ToString(),
                                            Fld10 = item.PostDetails == null ? "" : item.PostDetails.AgeFrom.ToString(),
                                            Fld11 = item.PostDetails == null ? "" : item.PostDetails.AgeTo.ToString(),
                                            Fld12 = item.BatchName == null ? "" : item.BatchName.ToString(),
                                            Fld13 = item.JobReferenceNo.ToString()

                                        };
                                        OGenericPayrollStatement.Add(OGeneticApprStatement);
                                    }
                                }
                            }
                        }
                        return OGenericPayrollStatement;
                        break;

                    case "RECRUITMENTINITIATOR":
                        if (salheadlist.Count > 0)
                        {
                            foreach (var jobref in salheadlist)
                            {
                                // var Recruitbatchcreation = db.RecruitBatchInitiator
                                var recruitiniator = db.RecruitInitiator
                                    .Include(e => e.RecruitBatchInitiator)
                                     .Include(e => e.RecruitBatchInitiator)
                                     .Include(e => e.RecruitBatchInitiator.Select(x => x.PostDetails))
                                     .Include(e => e.RecruitBatchInitiator.Select(x => x.PostDetails.FuncStruct))
                                     .Include(e => e.RecruitBatchInitiator.Select(x => x.PostDetails.FuncStruct.Job))
                                    //.Include(e => e.PostDetails)

                                    //.Include(e => e.PostDetails.FuncStruct)
                                    //.Include(e => e.PostDetails.FuncStruct.Job)

                                    .Where(e => e.initiatedDate >= pFromDate && e.initiatedDate <= pToDate && e.AdvertiseReferenceNo == jobref)
                                    .ToList();
                                if (recruitiniator == null)
                                {
                                    return null;
                                }
                                else
                                {
                                    foreach (var item in recruitiniator)
                                    {
                                        var batchin = item.RecruitBatchInitiator.ToList();
                                        foreach (var item1 in batchin)
                                        {


                                            GenericField100 OGeneticApprStatement = new GenericField100()
                                            {
                                                Fld1 = item1.initiatedDate.Value.ToShortDateString(),
                                                Fld2 = item1.PublishDate != null ? item1.PublishDate.Value.ToShortDateString() : null,
                                                Fld3 = item1.BatchCloseDate != null ? item1.BatchCloseDate.Value.ToShortDateString() : null,
                                                Fld4 = item1.PostDetails == null ? "" : item1.PostDetails.CreationDate == null ? "" : item1.PostDetails.CreationDate.Value.ToShortDateString(),
                                                Fld5 = item1.PostDetails == null ? "" : item1.PostDetails.PostCode.ToString(),
                                                Fld6 = item1.PostDetails == null ? "" : item1.PostDetails.FuncStruct == null ? "" : item1.PostDetails.FuncStruct.Job == null ? "" : item1.PostDetails.FuncStruct.Job.Name.ToString(),
                                                Fld7 = item1.PostDetails == null ? "" : item1.PostDetails.RequestVacancies.ToString(),
                                                Fld8 = item1.PostDetails == null ? "" : item1.PostDetails.ExpYearFrom.ToString(),
                                                Fld9 = item1.PostDetails == null ? "" : item1.PostDetails.ExpYearTo.ToString(),
                                                Fld10 = item1.PostDetails == null ? "" : item1.PostDetails.AgeFrom.ToString(),
                                                Fld11 = item1.PostDetails == null ? "" : item1.PostDetails.AgeTo.ToString(),
                                                Fld12 = item1.BatchName == null ? "" : item1.BatchName.ToString(),
                                                Fld13 = item1.JobReferenceNo.ToString(),

                                                Fld14 = item.AdvertiseReferenceNo.ToString(),
                                                Fld15 = item.initiatedDate.Value.ToShortDateString(),
                                                Fld16 = item.PublishDate != null ? item.PublishDate.Value.ToShortDateString() : null,
                                                Fld17 = item.CloseAdvDate != null ? item.CloseAdvDate.Value.ToShortDateString() : null,


                                            };
                                            OGenericPayrollStatement.Add(OGeneticApprStatement);
                                        }
                                    }
                                }
                            }
                        }
                        return OGenericPayrollStatement;
                        break;
                    case "MANPOWERRECOMENDATION":
                        if (salheadlist.Count > 0)
                        {
                            foreach (var batch in salheadlist)
                            {
                                var manpoweranalysis = db.ManPowerDetailsBatch
                                    .Include(e => e.ManPowerPostData)
                                    .Include(e => e.ManPowerPostData.Select(x => x.ManPowerBudget))
                                    .Include(e => e.ManPowerPostData.Select(x => x.ManPowerBudget.GeoStruct.Location.LocationObj))
                                    .Include(e => e.ManPowerPostData.Select(x => x.ManPowerBudget.PayStruct.Grade))
                                    .Include(e => e.ManPowerPostData.Select(x => x.ManPowerBudget.FuncStruct.JobPosition))
                                    .Where(z => z.ProcessDate >= pFromDate && z.ProcessDate <= pToDate && z.BatchName.ToUpper() == batch.ToUpper().ToString())
                                    .ToList();
                                if (manpoweranalysis == null)
                                {
                                    return null;
                                }
                                else
                                {
                                    foreach (var item in manpoweranalysis)
                                    {
                                        var manpanalysis = item.ManPowerPostData.ToList();
                                        foreach (var item1 in manpanalysis)
                                        {


                                            GenericField100 OGeneticApprStatement = new GenericField100()
                                            {
                                                Fld1 = item.BatchName.ToString(),
                                                Fld2 = item.ProcessDate.Value.ToShortDateString(),
                                                Fld5 = item1.ManPowerBudget.GeoStruct == null ? "" : item1.ManPowerBudget.GeoStruct.Location.LocationObj.LocDesc.ToString(),
                                                Fld6 = item1.ManPowerBudget.PayStruct == null ? "" : item1.ManPowerBudget.PayStruct.Grade == null ? "" : item1.ManPowerBudget.PayStruct.Grade.Name.ToString(),
                                                Fld7 = item1.ManPowerBudget.FuncStruct == null ? "" : item1.ManPowerBudget.FuncStruct.JobPosition == null ? "" : item1.ManPowerBudget.FuncStruct.JobPosition.JobPositionDesc.ToString(),
                                                Fld3 = item.ActionMovement.ToString(),
                                                Fld4 = item.ActionRecruitment.ToString(),
                                                Fld8 = item.ActionDate != null ? item.ActionDate.Value.ToShortDateString() : "",
                                                Fld9 = item.IsCloseBatch.ToString(),
                                                Fld10 = item1.SanctionedPosts.ToString(),
                                                Fld11 = item1.FilledPosts.ToString(),
                                                Fld12 = item1.ExcessPosts.ToString(),
                                                Fld13 = item1.VacantPosts.ToString()
                                            };
                                            OGenericPayrollStatement.Add(OGeneticApprStatement);
                                        }
                                    }
                                }
                            }
                        }

                        return OGenericPayrollStatement;
                        break;
                    case "CANDIDATECTCDEFINITION":
                        var ctcdef = db.CTCDefinition.Include(e => e.SalaryHead)
                            .Where(e => e.EffectiveDate >= pFromDate && e.EndDate <= pToDate).ToList();
                        if (ctcdef == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var item in ctcdef)
                            {
                                var salh = item.SalaryHead.ToList();
                                foreach (var item1 in salh)
                                {
                                    GenericField100 OGeneticApprStatement = new GenericField100()
                                               {
                                                   Fld1 = item.EffectiveDate.Value.ToShortDateString(),
                                                   Fld2 = item.EndDate.Value.ToShortDateString(),
                                                   Fld3 = item1.Code.ToString()

                                               };
                                    OGenericPayrollStatement.Add(OGeneticApprStatement);
                                }
                            }
                        }
                        return OGenericPayrollStatement;
                        break;
                    case "JOBSOURCE":
                        if (SpecialGroupslist.Count() > 0)
                        {
                            foreach (var Jobrec in SpecialGroupslist)
                            {
                                if (Jobrec.ToUpper().ToString() == "JOBAGENCY")
                                {
                                    var aa = db.CompanyRecruitment
                                        .Include(e => e.JobAgency)
                                        .Include(e => e.JobAgency.Select(x => x.ContactDetails))
                                        .Include(e => e.JobAgency.Select(x => x.ContactPerson))
                                        .Include(e => e.JobAgency.Select(x => x.AgencyAddress))
                                        .Include(e => e.JobAgency.Select(x => x.AgencyAddress.Area))
                                        .Include(e => e.JobAgency.Select(x => x.AgencyAddress.City))
                                        .Include(e => e.JobAgency.Select(x => x.AgencyAddress.Country))
                                        .Include(e => e.JobAgency.Select(x => x.AgencyAddress.District))
                                         .Include(e => e.JobAgency.Select(x => x.AgencyAddress.State))
                                          .Include(e => e.JobAgency.Select(x => x.AgencyAddress.StateRegion))
                                           .Include(e => e.JobAgency.Select(x => x.AgencyAddress.Taluka))
                                        .Include(e => e.JobAgency.Select(x => x.ContactDetails.ContactNumbers))
                                        .ToList();

                                    foreach (var item1 in aa)
                                    {
                                        var jobr = item1.JobAgency.ToList();
                                        foreach (var item2 in jobr)
                                        {


                                            GenericField100 OGeneticApprStatement = new GenericField100()
                                            {
                                                Fld1 = item2.Name.ToString(),
                                                Fld2 = item2.PANNo != null ? item2.PANNo.ToString() : "",
                                                Fld3 = item2.GSTNo != null ? item2.GSTNo.ToString() : "",
                                                Fld4 = item2.ContactPerson != null && item2.ContactPerson.FullNameFML != null ? item2.ContactPerson.FullNameFML.ToString() : "",
                                                Fld5 = item2.ContactDetails != null && item2.ContactDetails.FullContactDetails != null ? item2.ContactDetails.FullContactDetails.ToString() : "",
                                                Fld6 = item2.AgencyAddress != null ? item2.AgencyAddress.FullAddress.ToString() : "",
                                                Fld7 = Jobrec.ToUpper().ToString()


                                            };
                                            OGenericPayrollStatement.Add(OGeneticApprStatement);
                                        }
                                    }

                                }
                                else if (Jobrec.ToUpper().ToString() == "JOBINSIDEORG")
                                {
                                    var aa = db.CompanyRecruitment
                                                                            .Include(e => e.JobInsideOrg)
                                                                            .Include(e => e.JobInsideOrg.Select(x => x.ContactDetails))
                                                                            .Include(e => e.JobInsideOrg.Select(x => x.ContactPerson))
                                                                            .Include(e => e.JobInsideOrg.Select(x => x.PortalAddress))
                                                                            .Include(e => e.JobInsideOrg.Select(x => x.PortalAddress.Area))
                                                                            .Include(e => e.JobInsideOrg.Select(x => x.PortalAddress.City))
                                                                            .Include(e => e.JobInsideOrg.Select(x => x.PortalAddress.Country))
                                                                            .Include(e => e.JobInsideOrg.Select(x => x.PortalAddress.District))
                                                                             .Include(e => e.JobInsideOrg.Select(x => x.PortalAddress.State))
                                                                              .Include(e => e.JobInsideOrg.Select(x => x.PortalAddress.StateRegion))
                                                                               .Include(e => e.JobInsideOrg.Select(x => x.PortalAddress.Taluka))
                                                                            .Include(e => e.JobInsideOrg.Select(x => x.ContactDetails.ContactNumbers))
                                                                            .ToList();

                                    foreach (var item1 in aa)
                                    {
                                        var jobr = item1.JobInsideOrg.ToList();
                                        foreach (var item2 in jobr)
                                        {


                                            GenericField100 OGeneticApprStatement = new GenericField100()
                                            {
                                                Fld1 = item2.Name.ToString(),
                                                Fld2 = item2.PANNo != null ? item2.PANNo.ToString() : "",
                                                Fld3 = item2.GSTNo != null ? item2.GSTNo.ToString() : "",
                                                Fld4 = item2.ContactPerson != null && item2.ContactPerson.FullNameFML != null ? item2.ContactPerson.FullNameFML.ToString() : "",
                                                Fld5 = item2.ContactDetails != null && item2.ContactDetails.FullContactDetails != null ? item2.ContactDetails.FullContactDetails.ToString() : "",
                                                Fld6 = item2.PortalAddress != null ? item2.PortalAddress.FullAddress.ToString() : null,
                                                Fld7 = Jobrec.ToUpper().ToString()


                                            };
                                            OGenericPayrollStatement.Add(OGeneticApprStatement);
                                        }
                                    }

                                }
                                else if (Jobrec.ToUpper().ToString() == "JOBNEWSPAPER")
                                {
                                    var aa = db.CompanyRecruitment
                                                                            .Include(e => e.JobNewsPaper)
                                                                            .Include(e => e.JobNewsPaper.Select(x => x.ContactDetails))
                                                                            .Include(e => e.JobNewsPaper.Select(x => x.ContactPerson))
                                                                            .Include(e => e.JobNewsPaper.Select(x => x.NewPaperAddress))
                                                                            .Include(e => e.JobNewsPaper.Select(x => x.NewPaperAddress.Area))
                                                                            .Include(e => e.JobNewsPaper.Select(x => x.NewPaperAddress.City))
                                                                            .Include(e => e.JobNewsPaper.Select(x => x.NewPaperAddress.Country))
                                                                            .Include(e => e.JobNewsPaper.Select(x => x.NewPaperAddress.District))
                                                                             .Include(e => e.JobNewsPaper.Select(x => x.NewPaperAddress.State))
                                                                              .Include(e => e.JobNewsPaper.Select(x => x.NewPaperAddress.StateRegion))
                                                                               .Include(e => e.JobNewsPaper.Select(x => x.NewPaperAddress.Taluka))
                                                                            .Include(e => e.JobNewsPaper.Select(x => x.ContactDetails.ContactNumbers))
                                                                            .ToList();

                                    foreach (var item1 in aa)
                                    {
                                        var jobr = item1.JobNewsPaper.ToList();
                                        foreach (var item2 in jobr)
                                        {


                                            GenericField100 OGeneticApprStatement = new GenericField100()
                                            {
                                                Fld1 = item2.Name.ToString(),
                                                Fld2 = item2.PANNo != null ? item2.PANNo.ToString() : "",
                                                Fld3 = item2.GSTNo != null ? item2.GSTNo.ToString() : "",
                                                Fld4 = item2.ContactPerson != null && item2.ContactPerson.FullNameFML != null ? item2.ContactPerson.FullNameFML.ToString() : "",
                                                Fld5 = item2.ContactDetails != null && item2.ContactDetails.FullContactDetails != null ? item2.ContactDetails.FullContactDetails.ToString() : "",
                                                Fld6 = item2.NewPaperAddress != null ? item2.NewPaperAddress.FullAddress.ToString() : null,
                                                Fld7 = Jobrec.ToUpper().ToString()


                                            };
                                            OGenericPayrollStatement.Add(OGeneticApprStatement);
                                        }
                                    }

                                }
                                else if (Jobrec.ToUpper().ToString() == "JOBPORTAL")
                                {
                                    var aa = db.CompanyRecruitment
                                                                            .Include(e => e.JobPortal)
                                                                            .Include(e => e.JobPortal.Select(x => x.ContactDetails))
                                                                            .Include(e => e.JobPortal.Select(x => x.ContactPerson))
                                                                            .Include(e => e.JobPortal.Select(x => x.PortalAddress))
                                                                            .Include(e => e.JobPortal.Select(x => x.PortalAddress.Area))
                                                                            .Include(e => e.JobPortal.Select(x => x.PortalAddress.City))
                                                                            .Include(e => e.JobPortal.Select(x => x.PortalAddress.Country))
                                                                            .Include(e => e.JobPortal.Select(x => x.PortalAddress.District))
                                                                             .Include(e => e.JobPortal.Select(x => x.PortalAddress.State))
                                                                              .Include(e => e.JobPortal.Select(x => x.PortalAddress.StateRegion))
                                                                               .Include(e => e.JobPortal.Select(x => x.PortalAddress.Taluka))
                                                                            .Include(e => e.JobPortal.Select(x => x.ContactDetails.ContactNumbers))
                                                                            .ToList();

                                    foreach (var item1 in aa)
                                    {
                                        var jobr = item1.JobPortal.ToList();
                                        foreach (var item2 in jobr)
                                        {


                                            GenericField100 OGeneticApprStatement = new GenericField100()
                                            {
                                                Fld1 = item2.Name.ToString(),
                                                Fld2 = item2.PANNo != null ? item2.PANNo.ToString() : "",
                                                Fld3 = item2.GSTNo != null ? item2.GSTNo.ToString() : "",
                                                Fld4 = item2.ContactPerson != null && item2.ContactPerson.FullNameFML != null ? item2.ContactPerson.FullNameFML.ToString() : "",
                                                Fld5 = item2.ContactDetails != null && item2.ContactDetails.FullContactDetails != null ? item2.ContactDetails.FullContactDetails.ToString() : "",
                                                Fld6 = item2.PortalAddress != null ? item2.PortalAddress.FullAddress.ToString() : null,
                                                Fld7 = Jobrec.ToUpper().ToString()


                                            };
                                            OGenericPayrollStatement.Add(OGeneticApprStatement);
                                        }
                                    }

                                }


                            }
                        }
                        return OGenericPayrollStatement;
                        break;
                    case "RECRUITMENTVENUE":

                        var OrecVenueReportData = db.CompanyRecruitment
                            .Include(a => a.RecruitVenue)
                            .Include(a => a.RecruitVenue.Select(x => x.VenuType))
                            .Include(a => a.RecruitVenue.Select(y => y.Address))
                            .Include(a => a.RecruitVenue.Select(z => z.ContactDetails))
                            .Include(a => a.RecruitVenue.Select(z => z.ContactDetails.ContactNumbers))
                            .ToList();
                        if (OrecVenueReportData == null || OrecVenueReportData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OrecVenueReportData)
                            {
                                var rec = ca.RecruitVenue.ToList();

                                foreach (var item in rec)
                                {

                                    GenericField100 OGeneticApprStatement = new GenericField100()
                                    {
                                        Fld1 = item.Name.ToString(),
                                        Fld2 = item.ContactPerson.ToString(),
                                        Fld3 = item.Address == null ? "" : item.Address.FullAddress.ToString(),
                                        Fld4 = item.ContactDetails == null ? "" : item.ContactDetails.FullContactDetails.ToString(),
                                        Fld5 = item.Fees == null ? "" : item.Fees.ToString(),
                                        Fld6 = item.VenuType == null ? "" : item.VenuType.LookupVal.ToString()

                                        // Fld3 = item.FullDetails.ToString()


                                    };
                                    OGenericPayrollStatement.Add(OGeneticApprStatement);
                                }


                            }
                        }
                        return OGenericPayrollStatement;
                        break;

                    case "RECRUITMENTCALENDAR":

                        var ORECcalReportData = db.Calendar.Include(e => e.Name).Where(a => a.Name.LookupVal.ToUpper() == "RECRUITMENTCALENDAR").ToList();

                        if (ORECcalReportData == null || ORECcalReportData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in ORECcalReportData)
                            {
                                //if (ca.ProgramList != null)
                                //{
                                GenericField100 OGeneticApprStatement = new GenericField100()
                                {
                                    Fld1 = ca.FromDate != null ? ca.FromDate.Value.ToShortDateString() : "",
                                    Fld2 = ca.ToDate != null ? ca.ToDate.Value.ToShortDateString() : "",
                                    Fld3 = ca.Name != null ? ca.Name.LookupVal.ToString() : "",
                                    Fld4 = ca.Default != null ? ca.Default.ToString() : ""

                                    //Fld1 = ca.StartDate != null ? ca.StartDate.Value.ToShortDateString() : "",
                                    //Fld2 = ca.EndDate != null ? ca.EndDate.Value.ToShortDateString() : "",
                                    //Fld3 = ca.ProgramList.TrainingType == null ? "" : ca.ProgramList.TrainingType.LookupVal.ToString(),
                                    //Fld4 = ca.ProgramList.Subject.ToString(),
                                    //Fld5 = ca.ProgramList.SubjectDetails.ToString(),
                                };
                                OGenericPayrollStatement.Add(OGeneticApprStatement);
                                //}

                            }
                        }

                        return OGenericPayrollStatement;
                        break;
                    case "RESUMESHORTLISTING":
                        var Candidaters = new List<Candidate>();
                        var candidateselectedata = db.RecruitInitiator
                                                 .Include(e => e.RecruitBatchInitiator)
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate.CanName)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate.Gender)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate.CorContact)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate.CorContact.ContactNumbers)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate.CorAddr)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate.CorAddr.Area)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate.CorAddr.City)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate.CorAddr.Country)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate.CorAddr.State)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate.CorAddr.District)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate.CorAddr.StateRegion)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate.CorAddr.Taluka)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.ResumeSortlistingStatus)))
                                                 .Where(e => salheadlist.Contains(e.AdvertiseReferenceNo))
                                                 .ToList();




                        if (candidateselectedata == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var item in candidateselectedata)
                            {
                                foreach (var item1 in item.RecruitBatchInitiator)
                                {
                                    var canddata = item1.ResumeCollection.Where(e => EmpPayrollIdList.Contains
                                                  (e.Candidate.Id)).ToList();

                                    foreach (var item2 in canddata)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100
                                        {
                                            Fld1 = item2.Candidate == null ? "" : item2.Candidate.CanCode.ToString(),
                                            Fld2 = item2.Candidate == null ? "" : item2.Candidate.CanName == null ? "" : item2.Candidate.CanName.FullNameFML.ToString(),
                                            Fld3 = item2.Candidate == null ? "" : item2.Candidate.Gender == null ? "" : item2.Candidate.Gender.LookupVal.ToString(),
                                            Fld4 = item2.ResumeSortlistingStatus == null ? "" : item2.ResumeSortlistingStatus.LookupVal.ToString(),
                                            Fld5 = item.AdvertiseReferenceNo.ToString(),
                                            Fld6 = item2.Candidate == null ? "" : item2.Candidate.CorContact == null ? "" : item2.Candidate.CorContact.FullContactDetails == null ? "" : item2.Candidate.CorContact.FullContactDetails.ToString(),
                                            Fld7 = item2.Candidate == null ? "" : item2.Candidate.CorAddr == null ? "" : item2.Candidate.CorAddr.FullAddress == null ? "" : item2.Candidate.CorAddr.FullAddress.ToString()



                                        };
                                        OGenericPayrollStatement.Add(OGenericObjStatement);

                                    }
                                }
                            }

                        }
                        return OGenericPayrollStatement;
                        break;
                    case "CANDIDATEFINALIZATION":
                        var Candidatefinal = new List<Candidate>();
                        var candidateselectedataF = db.RecruitInitiator
                                                 .Include(e => e.RecruitBatchInitiator)

                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate.CanName)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate.Gender)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.ResumeSortlistingStatus)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.HREvaluationStatus)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.HRJoiningStatus)))
                                                 .Where(e => salheadlist.Contains(e.AdvertiseReferenceNo))
                                                 .ToList();




                        if (candidateselectedataF == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var item in candidateselectedataF)
                            {
                                foreach (var item1 in item.RecruitBatchInitiator)
                                {
                                    foreach (var candi in EmpPayrollIdList)
                                    {

                                        var item2 = item1.ResumeCollection
                                       .Where(e => e.Candidate.Id == candi).SingleOrDefault();

                                        if (item2 != null)
                                        {

                                            if (item2.HREvaluationStatus != null
                                                    && item2.HRJoiningStatus != null)
                                            {

                                                if (item2.HREvaluationStatus.LookupVal.ToUpper() == "SELECTED"
                                                         && item2.HRJoiningStatus.LookupVal.ToUpper() == "SELECTED")
                                                {


                                                    GenericField100 OGenericObjStatement = new GenericField100
                                                    {


                                                        Fld1 = item2.Candidate == null ? "" : item2.Candidate.CanCode.ToString(),
                                                        Fld2 = item2.Candidate == null ? "" : item2.Candidate.CanName == null ? "" : item2.Candidate.CanName.FullNameFML.ToString(),
                                                        Fld3 = item2.Candidate == null ? "" : item2.Candidate.Gender == null ? "" : item2.Candidate.Gender.LookupVal.ToString(),
                                                        Fld4 = item1.BatchName == null ? "" : item1.BatchName.ToString(),
                                                        Fld5 = item.AdvertiseReferenceNo.ToString(),
                                                        Fld6 = item2.IsHREvaluationConfirmation == null ? "" : item2.IsHREvaluationConfirmation.ToString(),
                                                        Fld7 = item2.IsHRJoiningConfirmation == null ? "" : item2.IsHRJoiningConfirmation.ToString(),
                                                        Fld8 = item2.IsInductionTraining == null ? "" : item2.IsInductionTraining.ToString(),
                                                        Fld9 = item2.IsJoined == null ? "" : item2.IsJoined.ToString(),
                                                        Fld10 = item2.IsNotificationToHeads == null ? "" : item2.IsNotificationToHeads.ToString(),
                                                        Fld11 = item2.IsServiceBookUpdate == null ? "" : item2.IsServiceBookUpdate.ToString(),
                                                        Fld12 = item2.ReasonToFailureEvaluat == null ? "" : item2.ReasonToFailureEvaluat.ToString(),
                                                        Fld13 = item2.ReasonToFailureJoining == null ? "" : item2.ReasonToFailureJoining.ToString()



                                                    };
                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                        }
                        return OGenericPayrollStatement;
                        break;
                    case "CANDIDATEEVALUATION":
                        var Candidateeval = new List<Candidate>();
                        var candidateselectedataeval = db.RecruitInitiator
                                                 .Include(e => e.RecruitBatchInitiator)
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(m => m.HREvaluationStatus)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.RecruitEvaluationProcessResult)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.RecruitEvaluationProcessResult.Select(a => a.RecruitEvaluationPara))))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.RecruitEvaluationProcessResult.Select(a => a.RecruitEvaluationPara.RecruitEvalPara))))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.RecruitEvaluationProcessResult.Select(a => a.ActivityResult))))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate.CanName)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate.Gender)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.ResumeSortlistingStatus)))
                                                 .Where(e => salheadlist.Contains(e.AdvertiseReferenceNo))
                                                 .ToList();




                        if (candidateselectedataeval == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var item in candidateselectedataeval)
                            {
                                foreach (var item1 in item.RecruitBatchInitiator)
                                {
                                    var canddata = item1.ResumeCollection.Where(e => EmpPayrollIdList.Contains
                                                  (e.Candidate.Id)).ToList();

                                    foreach (var item2 in canddata)
                                    {
                                        var receval = item2.RecruitEvaluationProcessResult.Where(e => e.ActivityDate >= pFromDate && e.ActivityDate <= pToDate).ToList();

                                        foreach (var item3 in receval)
                                        {

                                            GenericField100 OGenericObjStatement = new GenericField100
                                            {
                                                Fld1 = item2.Candidate != null ? item2.Candidate.CanCode.ToString() : "",
                                                Fld2 = item2.Candidate != null && item2.Candidate.CanName != null ? item2.Candidate.CanName.FullNameFML.ToString() : "",
                                                Fld3 = item3.RecruitEvaluationPara != null ? item3.RecruitEvaluationPara.FullDetails.ToString() : "",
                                                Fld4 = item1.BatchName != null ? item1.BatchName.ToString() : "",
                                                Fld5 = item.AdvertiseReferenceNo != null ? item.AdvertiseReferenceNo.ToString() : "",
                                                Fld6 = item3.ActivityDate != null ? item3.ActivityDate.Value.ToShortDateString() : "",
                                                Fld7 = item3.ActivityLetterIssue != null ? item3.ActivityLetterIssue.ToString() : "",
                                                Fld8 = SpecialGroupslist.Count() > 0 ? SpecialGroupslist.FirstOrDefault() : "",
                                                Fld9 = item3.ActivityResult != null ? item3.ActivityResult.LookupVal.ToString() : "",
                                                Fld10 = item3.ActivityScore != null ? item3.ActivityScore.ToString() : "",
                                                Fld11 = item3.AbsentRemark != null ? item3.AbsentRemark.ToString() : "",
                                                Fld12 = item2.HREvaluationStatus != null ? item2.HREvaluationStatus.LookupVal.ToString() : ""



                                            };
                                            OGenericPayrollStatement.Add(OGenericObjStatement);



                                        }


                                    }
                                }
                            }

                        }
                        return OGenericPayrollStatement;
                        break;
                    case "CANDIDATEJOINING":
                        var Candidatejoin = new List<Candidate>();
                        var candidateselectedatajoin = db.RecruitInitiator
                                                 .Include(e => e.RecruitBatchInitiator)
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(m => m.HRJoiningStatus)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.RecruitJoinParaProcessResult)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.RecruitJoinParaProcessResult.Select(a => a.RecruitJoiningPara))))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.RecruitJoinParaProcessResult.Select(a => a.RecruitJoiningPara.RecruitJoinPara))))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate.CanName)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.Candidate.Gender)))
                                                 .Include(e => e.RecruitBatchInitiator.Select(x => x.ResumeCollection.Select(t => t.ResumeSortlistingStatus)))
                                                 .Where(e => salheadlist.Contains(e.AdvertiseReferenceNo))
                                                 .ToList();




                        if (candidateselectedatajoin == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var item in candidateselectedatajoin)
                            {
                                foreach (var item1 in item.RecruitBatchInitiator)
                                {
                                    var canddata = item1.ResumeCollection.Where(e => EmpPayrollIdList.Contains
                                                  (e.Candidate.Id)).ToList();

                                    foreach (var item2 in canddata)
                                    {
                                        var receval = item2.RecruitJoinParaProcessResult.Where(e => e.ActivityDate >= pFromDate && e.ActivityDate <= pToDate).ToList();

                                        foreach (var item3 in receval)
                                        {

                                            GenericField100 OGenericObjStatement = new GenericField100
                                            {
                                                Fld1 = item2.Candidate == null ? "" : item2.Candidate.CanCode.ToString(),
                                                Fld2 = item2.Candidate == null ? "" : item2.Candidate.CanName == null ? "" : item2.Candidate.CanName.FullNameFML.ToString(),
                                                Fld3 = item3.RecruitJoiningPara == null ? "" : item3.RecruitJoiningPara.FullDetails.ToString(),
                                                Fld4 = item1.BatchName == null ? "" : item1.BatchName.ToString(),
                                                Fld5 = item.AdvertiseReferenceNo.ToString(),
                                                Fld6 = item3.ActivityDate == null ? "" : item3.ActivityDate.Value.ToShortDateString(),
                                                Fld7 = item3.ActivityLetterIssue.ToString(),
                                                Fld8 = SpecialGroupslist.Count() > 0 ? SpecialGroupslist.Select(e => e).FirstOrDefault() : "",
                                                Fld9 = item3.ActivityAccepted.ToString(),
                                                Fld10 = item3.ActivityAcceptedDate == null ? "" : item3.ActivityAcceptedDate.Value.ToShortDateString(),
                                                Fld11 = item2.HRJoiningStatus == null ? "" : item2.HRJoiningStatus.LookupVal.ToString()



                                            };
                                            OGenericPayrollStatement.Add(OGenericObjStatement);



                                        }


                                    }
                                }
                            }

                        }
                        return OGenericPayrollStatement;
                        break;
                    case "CANDIDATEQUALIFICATION":
                        var Candidateq = new List<Candidate>();
                        foreach (var cid in EmpPayrollIdList)
                        {
                            var OCandidateQuali = db.Candidate
                                  .Include(a => a.CanAcademicInfo)
                                .Include(e => e.CanName)
                                .Include(e => e.CanAcademicInfo.QualificationDetails)
                                .Include(e => e.CanAcademicInfo.QualificationDetails.Select(t => t.Qualification))
                               .Include(e => e.CanAcademicInfo.QualificationDetails.Select(t => t.Qualification.Select(y => y.QualificationType)))
                                .AsNoTracking()
                                .Where(e => e.CanAcademicInfo != null && e.CanAcademicInfo.QualificationDetails.Count > 0 && e.Id == cid).FirstOrDefault();
                            if (OCandidateQuali != null)
                            {
                                Candidateq.Add(OCandidateQuali);
                            }

                        }
                        if (Candidateq == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in Candidateq)
                            {
                                if (ca.CanAcademicInfo != null)
                                {
                                    var v2 = ca.CanAcademicInfo.QualificationDetails.ToList();
                                    foreach (var item in v2)
                                    {
                                        if (salheadlist.Count() > 0)
                                        {
                                            foreach (var quali in salheadlist)
                                            {
                                                var v3 = item.Qualification.Where(e => e.QualificationShortName.ToUpper() == quali.ToUpper().ToString()).ToList();
                                                foreach (var item2 in v3)
                                                {
                                                    GenericField100 OGenericObjStatement = new GenericField100
                                                    {
                                                        Fld2 = ca.CanCode,
                                                        Fld3 = ca.CanName != null ? ca.CanName.FullNameFML : "",
                                                        //  Fld4 = item.Qualification.ToString(),
                                                        Fld5 = item.SpecilisedBranch != null ? item.SpecilisedBranch.ToString() : "",
                                                        Fld6 = item.Institute != null ? item.Institute.ToString() : "",
                                                        Fld7 = item.University != null ? item.University.ToString() : "",
                                                        Fld8 = item.ResultPercentage != null ? item.ResultPercentage.ToString() : "",
                                                        Fld9 = item2.QualificationType != null ? item2.QualificationType.LookupVal.ToString() : "",
                                                        Fld10 = item.PasingYear != null ? item.PasingYear.Value.ToShortDateString() : "",
                                                        Fld11 = item.ResultSubmissionDate != null ? item.ResultSubmissionDate.Value.ToShortDateString() : "",
                                                        Fld12 = item.SpecialFeature != null ? item.SpecialFeature.ToString() : "",
                                                        Fld13 = item2.QualificationShortName != null ? item2.QualificationShortName.ToString() : ""

                                                    };
                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                                }
                                            }

                                        }
                                        else
                                        {
                                            var v3 = item.Qualification.ToList();
                                            foreach (var item2 in v3)
                                            {
                                                GenericField100 OGenericObjStatement = new GenericField100
                                                {
                                                    Fld2 = ca.CanCode,
                                                    Fld3 = ca.CanName != null ? ca.CanName.FullNameFML : "",
                                                    //  Fld4 = item.Qualification.ToString(),
                                                    Fld5 = item.SpecilisedBranch != null ? item.SpecilisedBranch.ToString() : "",
                                                    Fld6 = item.Institute != null ? item.Institute.ToString() : "",
                                                    Fld7 = item.University != null ? item.University.ToString() : "",
                                                    Fld8 = item.ResultPercentage != null ? item.ResultPercentage.ToString() : "",
                                                    Fld9 = item2.QualificationType != null ? item2.QualificationType.LookupVal.ToString() : "",
                                                    Fld10 = item.PasingYear != null ? item.PasingYear.Value.ToShortDateString() : "",
                                                    Fld11 = item.ResultSubmissionDate != null ? item.ResultSubmissionDate.Value.ToShortDateString() : "",
                                                    Fld12 = item.SpecialFeature != null ? item.SpecialFeature.ToString() : "",
                                                    Fld13 = item2.QualificationShortName != null ? item2.QualificationShortName.ToString() : ""


                                                };
                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                            }
                                        }

                                    }
                                }
                            }
                            return OGenericPayrollStatement;
                        }
                        break;


                    case "CANDIDATEMEDICAL":

                        var oCanMedicalInfo = db.Candidate

                               .Include(e => e.CanMedicalInfo)
                               .Include(e => e.CanMedicalInfo.Allergy)
                               .Include(e => e.CanMedicalInfo.BloodGroup)
                               .Include(e => e.CanMedicalInfo.Disease)
                               .Include(e => e.CanMedicalInfo.Doctor)
                            //.Include(e => e.CanMedicalInfo.EmergencyContact)
                            //.Include(e => e.CanMedicalInfo.HospitalAddress)
                            //.Include(e => e.CanMedicalInfo.HospitalContactDetails)
                               .Include(e => e.CanMedicalInfo.Doctor.Select(a => a.Name))
                            .Include(e => e.CanName).AsNoTracking()

                            .Where(a => a.CanMedicalInfo != null)
                          .ToList();


                        if (oCanMedicalInfo == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in oCanMedicalInfo)
                            {

                                GenericField100 OGeneticApprStatement = new GenericField100()
                                {
                                    Fld2 = ca.CanCode,
                                    Fld3 = ca.CanName != null ? ca.CanName.FullNameFML : null,

                                    Fld4 = ca.CanMedicalInfo.BloodGroup != null ? ca.CanMedicalInfo.BloodGroup.LookupVal : null,
                                    Fld5 = ca.CanMedicalInfo.Height != 0 ? ca.CanMedicalInfo.Height.ToString() : null,
                                    Fld6 = ca.CanMedicalInfo.Weight != 0 ? ca.CanMedicalInfo.Weight.ToString() : null,

                                    //Fld6 = ca.HospitalAddress != null ? ca.HospitalAddress.FullAddress : null,
                                    //Fld7 = ca.HospitalContactDetails != null ? ca.HospitalContactDetails.FullContactDetails : null,
                                    //Fld10 = ca.IDMark != null ? ca.IDMark : null,

                                    Fld7 = ca.CanMedicalInfo.Allergy != null ? string.Join(",", ca.CanMedicalInfo.Allergy.Select(a => a.Name).ToArray()) : null,
                                    Fld8 = ca.CanMedicalInfo.Disease != null ? string.Join(",", ca.CanMedicalInfo.Disease.Select(a => a.Name).ToArray()) : null,
                                    Fld9 = ca.CanMedicalInfo.Doctor != null ? string.Join(",", ca.CanMedicalInfo.Doctor.Select(a => a.Name != null ? a.Name.FullNameFML : null).ToArray()) : null,
                                    Fld10 = ca.CanMedicalInfo.PreferredHospital == null ? "" : ca.CanMedicalInfo.PreferredHospital.ToString()

                                };
                                OGenericPayrollStatement.Add(OGeneticApprStatement);
                            }
                        }
                        return OGenericPayrollStatement;
                        break;


                    case "CANDIDATESOCIALINFO":
                        var OCandidateSocial = db.Candidate
                            .Include(a => a.CanSocialInfo)
                            .Include(e => e.CanName)
                            .Include(e => e.CanSocialInfo.Religion)
                           .Include(e => e.CanSocialInfo.Caste)
                           .Include(e => e.CanSocialInfo.SubCaste)
                           .Include(e => e.CanSocialInfo.Category)
                            .Include(e => e.CanSocialInfo.SocialActivities).AsNoTracking()
                            .Where(e => e.CanSocialInfo != null && e.CanSocialInfo.SocialActivities.Count > 0).ToList();
                        if (OCandidateSocial == null || OCandidateSocial.Count == 0)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OCandidateSocial)
                            {
                                var v2 = ca.CanSocialInfo.SocialActivities.ToList();
                                foreach (var item in v2)
                                {

                                    GenericField100 OGenericObjStatement = new GenericField100
                                    {
                                        Fld2 = ca.CanCode,
                                        Fld3 = ca.CanName != null ? ca.CanName.FullNameFML : null,
                                        Fld4 = ca.CanSocialInfo != null && ca.CanSocialInfo.Religion != null ? ca.CanSocialInfo.Religion.LookupVal : null,
                                        Fld5 = ca.CanSocialInfo != null && ca.CanSocialInfo.Category != null ? ca.CanSocialInfo.Category.LookupVal : null,
                                        Fld6 = ca.CanSocialInfo != null && ca.CanSocialInfo.Caste != null ? ca.CanSocialInfo.Caste.LookupVal : null,
                                        Fld7 = ca.CanSocialInfo != null && ca.CanSocialInfo.SubCaste != null ? ca.CanSocialInfo.SubCaste.LookupVal : null,
                                        Fld8 = item.InstituteName,
                                        Fld9 = item.PostHeld,
                                        Fld10 = item.FromPeriod != null ? item.FromPeriod.Value.ToShortDateString() : null,
                                        Fld11 = item.ToPeriod != null ? item.ToPeriod.Value.ToShortDateString() : null,
                                    };
                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                }
                            }
                        }
                        return OGenericPayrollStatement;
                        break;


                    case "CANDIDATESCHEDULING":
                        //   var cand = db.RecruitBatchInitiator.Include(e => e.ResumeCollection.Select(q => q.Candidate.Id)).ToList();
                        var Oschedule = db.RecruitInitiator
                            .Include(e => e.RecruitBatchInitiator)
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.PostDetails))
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.PostDetails.FuncStruct))
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.PostDetails.FuncStruct.Job))
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.PostDetails.Qualification))
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.ResumeCollection))
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.ResumeCollection.Select(t => t.Candidate)))
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.ResumeCollection.Select(y => y.Candidate.CanName)))
                            //      .Where(e=>e.ResumeCollection.Select(q => q.Candidate.Id!=null))
                                     .ToList();
                        if (Oschedule == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca1 in Oschedule)
                            {
                                foreach (var ca in ca1.RecruitBatchInitiator)
                                {


                                    var qual = ca.PostDetails.Qualification.Select(r => r.QualificationDesc).FirstOrDefault();
                                    var exp = ca.ResumeCollection.Select(r => r.YrsofExperience).FirstOrDefault();
                                    var canname = ca.ResumeCollection.Select(e => e.Candidate.CanName.FullNameFML).FirstOrDefault();
                                    var intvdate = ca.ResumeCollection.Select(q => q.InterviewDate.Value.ToShortDateString()).FirstOrDefault();
                                    GenericField100 OGeneticApprStatement = new GenericField100()
                                    {
                                        Fld1 = ca.JobReferenceNo == null ? "" : ca.JobReferenceNo.ToString(),//ref no
                                        //Fld2 = ca. == null ? "" : ca.ReferenceNo.ToString(),//ref no
                                        Fld3 = ca.PostDetails.FuncStruct.Job == null ? "" : ca.PostDetails.FuncStruct.Job.Name,//post for
                                        Fld4 = qual == null ? "" : qual,//qual
                                        Fld5 = exp == null ? "" : exp.ToString(), //exp
                                        Fld6 = ca1.AdvertiseReferenceNo == null ? "" : ca1.AdvertiseReferenceNo.ToString(),    //appln no
                                        Fld7 = canname == null ? "" : canname,//candidate name
                                        Fld8 = intvdate == null ? "" : intvdate,

                                    };
                                    OGenericPayrollStatement.Add(OGeneticApprStatement);
                                }
                            }
                        }
                        return OGenericPayrollStatement;
                        break;


                    case "CANDIDATELANGUAGES":
                        var OCanLang = db.Candidate
                            .Include(e => e.CanName)
                            .Include(e => e.CanAcademicInfo)
                            .Include(e => e.CanAcademicInfo.LanguageSkill)
                            .Include(e => e.CanAcademicInfo.LanguageSkill.Select(a => a.Language))
                            .Include(e => e.CanAcademicInfo.LanguageSkill.Select(a => a.SkillType)).AsNoTracking().ToList();
                        if (OCanLang == null || OCanLang.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var item in OCanLang)
                            {
                                foreach (var item2 in item.CanAcademicInfo.LanguageSkill)
                                {
                                    foreach (var item3 in item2.Language)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        {
                                            Fld1 = item.CanName.FullNameFML.ToString(),  // can name
                                            Fld2 = item3.LanguageName.ToString(),      // Language
                                            Fld3 = item2.SkillType.LookupVal.ToString(), // read, write , speak
                                        };
                                        OGenericPayrollStatement.Add(OGenericObjStatement);

                                    }

                                }
                            }
                            return OGenericPayrollStatement;
                        }
                        break;

                    case "CANDIDATESHORTLIST":
                        //   var cand = db.RecruitBatchInitiator.Include(e => e.ResumeCollection.Select(q => q.Candidate.Id)).ToList();
                        var Oschedule12 = db.RecruitInitiator
                            .Include(e => e.RecruitBatchInitiator)
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.PostDetails))
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.PostDetails.FuncStruct))
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.PostDetails.FuncStruct.Job))
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.PostDetails.Qualification))
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.ResumeCollection))
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.ResumeCollection.Select(t => t.Candidate)))
                                    .Include(e => e.RecruitBatchInitiator.Select(q => q.ResumeCollection.Select(y => y.Candidate.CanName))).AsNoTracking()
                            //      .Where(e=>e.ResumeCollection.Select(q => q.Candidate.Id!=null))
                                     .ToList();
                        if (Oschedule12 == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca1 in Oschedule12)
                            {
                                foreach (var ca in ca1.RecruitBatchInitiator)
                                {
                                    var qual = ca.PostDetails.Qualification.Select(r => r.QualificationDesc).FirstOrDefault();
                                    var exp = ca.ResumeCollection.Select(r => r.YrsofExperience).FirstOrDefault();
                                    var canname = ca.ResumeCollection.Select(e => e.Candidate.CanName.FullNameFML).FirstOrDefault();

                                    GenericField100 OGeneticApprStatement = new GenericField100()
                                    {
                                        Fld1 = ca.JobReferenceNo == null ? "" : ca.JobReferenceNo,//ref no
                                        Fld2 = ca.JobReferenceNo, //reqi no
                                        Fld3 = ca.PostDetails.FuncStruct.Job == null ? "" : ca.PostDetails.FuncStruct.Job.Name,//post for
                                        Fld4 = qual == null ? "" : qual,//qual
                                        Fld5 = exp == null ? "" : exp.ToString(), //exp
                                        Fld6 = ca1.AdvertiseReferenceNo == null ? "" : ca1.AdvertiseReferenceNo,    //appln no
                                        Fld7 = canname == null ? "" : canname,//candidate name
                                    };
                                    OGenericPayrollStatement.Add(OGeneticApprStatement);
                                }
                            }
                        }
                        return OGenericPayrollStatement;
                        break;


                    case "CANDIDATEMPLOYEMENTHISTORYREPORT":

                        var OCan = db.Candidate
                           .Include(e => e.CanName)
                            .Include(e => e.CanPreCompExp)
                            .Include(e => e.CanPreCompExp.Select(a => a.CompAddress))
                              .Include(e => e.CanPreCompExp.Select(a => a.ContactDetails.ContactNumbers))
                            .Include(e => e.CanPreCompExp.Select(a => a.CompAddress.Area))
                            .Include(e => e.CanPreCompExp.Select(a => a.CompAddress.City))
                            .Include(e => e.CanPreCompExp.Select(a => a.CompAddress.Country))
                            .Include(e => e.CanPreCompExp.Select(a => a.CompAddress.District))
                            .Include(e => e.CanPreCompExp.Select(a => a.CompAddress.State))
                            .Include(e => e.CanPreCompExp.Select(a => a.CompAddress.StateRegion))
                            .Include(e => e.CanPreCompExp.Select(a => a.CompAddress.Taluka))
                            .Include(e => e.ServiceBookDates).AsNoTracking()
                            .ToList();

                        if (OCan == null || OCan.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var item in OCan)
                            {
                                foreach (var item2 in item.CanPreCompExp)
                                {

                                    var contact = item2.ContactDetails.ContactNumbers.ToList();
                                    foreach (var item3 in contact)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                         {
                                             Fld1 = item.CanName.FullNameFML == null ? "" : item.CanName.FullNameFML.ToString(), // can name
                                             Fld2 = item2.CompName == null ? "" : item2.CompName.ToString(), // CompName
                                             Fld3 = item2.FromDate == null ? "" : item2.FromDate.Value.ToShortDateString(), // exp from date
                                             Fld4 = item2.ToDate == null ? "" : item2.ToDate.Value.ToShortDateString(), // exp to date
                                             Fld5 = item2.YrOfService == null ? "" : item2.YrOfService.ToString(), // Years of exp
                                             Fld6 = item2.JoiningJobPosition == null ? "" : item2.JoiningJobPosition.ToString(), // Design at joing
                                             Fld7 = item2.LeaveingJobPosition == null ? "" : item2.LeaveingJobPosition.ToString(), // Design at Leaving
                                             Fld8 = item2.LastDrawnSalary == null ? "" : item2.LastDrawnSalary.ToString(), // Last Drawn Salary
                                             Fld9 = item2.Reason == null ? "" : item2.Reason.ToString(), // Reason
                                             Fld10 = item2.CompAddress.Area == null ? "" : item2.CompAddress.Area.Name.ToString(),
                                             Fld11 = item2.CompAddress.Area == null ? "" : item2.CompAddress.Area.PinCode.ToString(), // pin code
                                             Fld12 = item2.CompAddress.City == null ? "" : item2.CompAddress.City.Name.ToString(), //City
                                             Fld13 = item2.CompAddress.Country == null ? "" : item2.CompAddress.Country.Name.ToString(), //Country
                                             Fld14 = item2.CompAddress.District == null ? "" : item2.CompAddress.District.Name.ToString(),
                                             Fld15 = item2.CompAddress.Landmark == null ? "" : item2.CompAddress.Landmark.ToString(),
                                             Fld16 = item2.CompAddress.State == null ? "" : item2.CompAddress.State.Name.ToString(), //state
                                             Fld17 = item2.CompAddress.StateRegion == null ? "" : item2.CompAddress.StateRegion.Name.ToString(),
                                             Fld18 = item2.CompAddress.Taluka == null ? "" : item2.CompAddress.Taluka.Name.ToString(),
                                             Fld19 = item3.LandlineNo == null ? "" : item3.LandlineNo.ToString(), // phone no

                                         };
                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }
                            }
                            return OGenericPayrollStatement;
                        }
                        break;


                    default:
                        return null;
                        break;

                }

            }

        }

        public static List<Utility.ReportClass> GetViewData(int Flag)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Flag == 0)
                {
                    List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                    var query1 = "select * from VGeoStructD ";
                    if (query1 != "")
                    {
                        GeoData = db.Database.SqlQuery<Utility.ReportClass>(query1).ToList<Utility.ReportClass>();
                    }

                    return GeoData;
                }
                //}
                //if (chkpaystruct == 1)
                //{
                if (Flag == 1)
                {
                    List<Utility.ReportClass> PayData = new List<Utility.ReportClass>();

                    var query2 = "select * from VPayStructD ";
                    if (query2 != "")
                    {
                        PayData = db.Database.SqlQuery<Utility.ReportClass>(query2).ToList<Utility.ReportClass>();
                    }
                    return PayData;
                }

                // }

                //if (chkfunstruct == 1)
                //{
                if (Flag == 2)
                {
                    List<Utility.ReportClass> FuncData = new List<Utility.ReportClass>();
                    var query3 = "select * from VFuncStructD ";
                    if (query3 != "")
                    {
                        FuncData = db.Database.SqlQuery<Utility.ReportClass>(query3).ToList<Utility.ReportClass>();
                    }
                    return FuncData;
                }

                return null;

            }
        }

        public static Utility.ReportClass GetViewDataIndiv(int geoid, int payid, int funid, List<Utility.ReportClass> GrpInfoPass, int Flag)
        {

            Utility.ReportClass GrpInfo = new Utility.ReportClass();
            if (Flag == 0)
            {
                GrpInfo = GrpInfoPass.Where(e => e.Geo_Struct_Id == geoid).SingleOrDefault();
                return GrpInfo;
            }


            if (Flag == 1)
            {
                GrpInfo = GrpInfoPass.Where(e => e.PayStruct_Id == payid).SingleOrDefault();
                return GrpInfo;
            }


            if (Flag == 2)
            {
                GrpInfo = GrpInfoPass.Where(e => e.FuncStruct_Id == funid).SingleOrDefault();
                return GrpInfo;
            }


            return null;
        }

        public static GenericField100 GetFilterData(List<string> forithead, GenericField100 OGenericObjStatement, string paymonth, string employee, Utility.ReportClass GeodataInd, Utility.ReportClass PaydataInd, Utility.ReportClass FuncdataInd)
        {
            var month = false;
            var emp = false;
            var dept = false;
            var loca = false;
            var comp = false;
            var grp = false;
            var unit = false;
            var div = false;
            var regn = false;
            var grade = false;
            var lvl = false;
            var jobstat = false;
            var job = false;
            var jobpos = false;

            using (DataBaseContext db = new DataBaseContext())
            {

                var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();

                foreach (var item in vc)
                {
                    if (item.LookupVal.ToUpper() == "MONTH")
                    {
                        month = true;
                    }
                    if (item.LookupVal.ToUpper() == "LOCATION")
                    {

                        loca = true;
                    }
                    if (item.LookupVal.ToUpper() == "EMPLOYEE")
                    {
                        emp = true;
                    }
                    if (item.LookupVal.ToUpper() == "DEPARTMENT")
                    {
                        dept = true;
                    }
                    if (item.LookupVal.ToUpper() == "COMPANY")
                    {
                        comp = true;
                    }
                    if (item.LookupVal.ToUpper() == "GROUP")
                    {
                        grp = true;
                    }
                    if (item.LookupVal.ToUpper() == "UNIT")
                    {
                        unit = true;
                    }
                    if (item.LookupVal.ToUpper() == "DIVISION")
                    {
                        div = true;
                    }
                    if (item.LookupVal.ToUpper() == "REGION")
                    {
                        regn = true;
                    }
                    if (item.LookupVal.ToUpper() == "GRADE")
                    {
                        grade = true;
                    }
                    if (item.LookupVal.ToUpper() == "LEVEL")
                    {
                        lvl = true;
                    }
                    if (item.LookupVal.ToUpper() == "JOBSTATUS")
                    {
                        jobstat = true;
                    }

                    if (item.LookupVal.ToUpper() == "JOB")
                    {
                        job = true;
                    }
                    if (item.LookupVal.ToUpper() == "JOBPOSITION")
                    {
                        jobpos = true;
                    }
                }


                if (month)
                {
                    OGenericObjStatement.Fld100 = paymonth;
                }
                if (comp)
                {
                    OGenericObjStatement.Fld99 = GeodataInd.Company_Name;
                }
                if (div)
                {
                    OGenericObjStatement.Fld98 = GeodataInd.Division_Name;
                }
                if (loca)
                {
                    OGenericObjStatement.Fld97 = GeodataInd.LocDesc;
                }
                if (dept)
                {
                    OGenericObjStatement.Fld96 = GeodataInd.DeptDesc;
                }
                if (grp)
                {
                    OGenericObjStatement.Fld95 = GeodataInd.Group_Name;
                }
                if (unit)
                {
                    OGenericObjStatement.Fld94 = GeodataInd.Unit_Name;
                }
                if (grade)
                {
                    OGenericObjStatement.Fld93 = PaydataInd.Grade_Name;
                }
                if (lvl)
                {
                    OGenericObjStatement.Fld92 = PaydataInd.Level_Name;
                }
                if (jobstat)
                {
                    OGenericObjStatement.Fld91 = GeodataInd.JobStatus_Id.ToString();
                }
                if (job)
                {
                    OGenericObjStatement.Fld90 = FuncdataInd.Job_Name;
                }
                if (jobpos)
                {
                    OGenericObjStatement.Fld89 = FuncdataInd.JobPositionDesc;
                }
                if (emp)
                {
                    OGenericObjStatement.Fld88 = employee;
                }

                return OGenericObjStatement;
            }
        }
        public class returnClass
        {
            public Int32 Id { get; set; }
            public string EmpCode { get; set; }
            public string PFNo { get; set; }
            public string PTNo { get; set; }
            public string PANNo { get; set; }
            public string AdharNo { get; set; }
            public string UANNo { get; set; }
            public string PensionNo { get; set; }
            public string ESICNo { get; set; }
            public string LWFNo { get; set; }
            public string EDLINo { get; set; }
            public string VCNo { get; set; }

            public string BirthDate { get; set; }
            public string JoiningDate { get; set; }
            public string ProbationPeriod { get; set; }
            public string ProbationDate { get; set; }
            public string ConfirmPeriod { get; set; }
            public string ConfirmationDate { get; set; }
            public string LastIncrementDate { get; set; }
            public string LastPromotionDate { get; set; }
            public string LastTransferDate { get; set; }
            public string SeniorityDate { get; set; }
            public string SeniorityNo { get; set; }
            public string RetirementDate { get; set; }
            public string ServiceLastDate { get; set; }
            public string PFJoingDate { get; set; }
            public string PensionJoingDate { get; set; }
            public string PFExitDate { get; set; }
            public string PensionExitDate { get; set; }
            // public ServiceBookDates ServiceBookDates { get; set; }
        }


        public static EmployeePayroll _returnGetEmployeeData(Int32 item, string MonYr)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //////var OPayslipData_temp = db.EmployeePayroll
                //////               .Include(e => e.Employee.EmpOffInfo.PayProcessGroup)
                //////           .Include(e => e.SalaryT.Select(r => r.PayslipR))
                //////           .Include(e => e.SalaryT.Select(r => r.PayslipR.Select(a => a.GeoStruct.Location.LocationObj)))
                //////           .Include(e => e.SalaryT.Select(r => r.PayslipR.Select(t => t.PaySlipDetailEarnR)))
                //////           .Include(e => e.SalaryT.Select(r => r.PayslipR.Select(t => t.PaySlipDetailDedR)))
                //////           .Include(e => e.SalaryT.Select(r => r.PayslipR.Select(t => t.PaySlipDetailLeaveR)))
                //////           .Where(e => e.Employee.Id == item).AsParallel()
                //////           .FirstOrDefault();
                //////return OPayslipData_temp;

                var empPayroll = db.EmployeePayroll.Where(e => e.Employee.Id == item).OrderBy(e => e.Id).SingleOrDefault();

                db.Entry(empPayroll).Collection(x => x.SalaryT).Query().Where(t => t.PayMonth == MonYr).Load();
                if (empPayroll.SalaryT != null)
                {
                    db.Entry(empPayroll.SalaryT.First()).Collection(x => x.PayslipR).Load();
                    if (db.Entry(empPayroll.SalaryT.First()).Collection(x => x.PayslipR).Query().Count() != 0)
                    {
                        db.Entry(empPayroll.SalaryT.First().PayslipR.First()).Collection(x => x.PaySlipDetailEarnR).Load();
                        db.Entry(empPayroll.SalaryT.First().PayslipR.First()).Collection(x => x.PaySlipDetailDedR).Load();
                        db.Entry(empPayroll.SalaryT.First().PayslipR.First()).Collection(x => x.PaySlipDetailLeaveR).Load();
                        db.Entry(empPayroll.SalaryT.First().PayslipR.First()).Reference(x => x.GeoStruct).Load();
                        db.Entry(empPayroll.SalaryT.First().PayslipR.First().GeoStruct).Reference(x => x.Location).Load();
                        db.Entry(empPayroll.SalaryT.First().PayslipR.First().GeoStruct.Location).Reference(x => x.LocationObj).Load();
                    }
                }


                return empPayroll;
            }
        }

    }
}