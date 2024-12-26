using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using P2b.Global;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Transactions;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using P2BUltimate.Security;
using System.Globalization;
using Payroll;
using ReportPayroll;
using System.Xml.Linq;
using System.Xml;
namespace P2BUltimate.Controllers
{

    public class ReportFilterController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult GetSalaryHead(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.SalaryHead.Select(b => b.Code).Distinct().ToList();
                SelectList drop = new SelectList(a);
                return Json(drop, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult Getloadadvhead(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.LoanAdvRequest.Select(b => b.LoanAdvanceHead.SalaryHead.Name).Distinct().ToList();
                SelectList drop = new SelectList(a);
                return Json(drop, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetSpecialGroupFilter(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data == "RPTSALARYARREARTSUMMARYPRO")
                {
                    var a = db.SalaryArrearT.Select(e => e.ArrearType.LookupVal.ToUpper()).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }

                else if (data == "RPTLEAVEENCASHPROJECTIONLVR")
                {
                    var a = db.LvHead.Select(b => b.LvCode).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTBRANCHSUMMARYDETAILSBRA") //changes by deepak
                {
                    //var a = db.LocationObj.Where(b => b.LocCode == "LocCode").Select(c => c.LocDesc == "LocName").ToList();
                    var loc = db.Location.Select(r => new
                    {
                      FullDetails=  r.LocationObj.LocCode + ":" + r.LocationObj.LocDesc
                    });
                    var a = loc.Select(r => r.FullDetails).ToList();

                   // var a = loc.Select(b => b).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }

                //else if (data == "RPTResignationRequestStatuslog") //changes by deepak
                //{
                //    var a = db.SeperationMaster.Select(b => b.TypeOfSeperation).Distinct().ToList();
                //    SelectList drop = new SelectList(a);
                //    return Json(drop, JsonRequestBehavior.AllowGet);

                //}
                else
                    return null;
            }
        }

        public ActionResult GetSpecialFilter(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data == "RPTTRANSFERLETTERSER")
                {
                    var a = db.TransPolicy.Select(b => b.Name).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                if (data == "RPTINSURANCEDETAILSTTRA")
                {
                    var a = db.InsuranceProduct.Select(b => b.InsuranceProductDesc).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
              
                else if (data == "RPTLOANPROPOSALOBJ")
                {
                    var a = db.LoanAdvanceHeadPFT.Select(e => e.Name).ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop != null ? drop : null, JsonRequestBehavior.AllowGet);
                }

                else if (data == "RPTOFFICIATINGPAYMENTDETAILSSER")
                {
                    var query = db.SalaryHead.Include(e => e.SalHeadOperationType).ToList();

                    List<string> salaryheadCodes = new List<string> { "OFFICIATING", "LTA", "MEDALLOW" };

                    List<SelectListItem> returndata = new List<SelectListItem>();

                    foreach (var item in query)
                    {
                        if (salaryheadCodes.Contains(item.SalHeadOperationType.LookupVal.ToUpper()))
                        {
                            returndata.Add(new SelectListItem
                            {
                                Value = item.Id.ToString(),
                                Text = item.Name
                            });
                        }
                    }

                    return Json(new SelectList(returndata, "Value", "Text"), JsonRequestBehavior.AllowGet);
                }


                else if (data == "RPTPASSBOOKMONTHWISEOBJ")
                {
                    var Lookupvaluesdetails = db.Lookup.Where(e => e.Code == "512").Select(e => e.LookupValues.Where(r => r.IsActive == true && (r.LookupVal == "LOAN DEBIT BALANCE" || r.LookupVal == "LOAN CREDIT BALANCE" || r.LookupVal == "INTEREST BALANCE" || r.LookupVal == "SETTLEMENT BALANCE" || r.LookupVal == "PF BALANCE"))).SingleOrDefault();
                    var a = Lookupvaluesdetails.Select(r => r.LookupVal).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop != null ? drop : null, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTPASSBOOKOBJ" || data == "RPTSETTELMENTPROPOSALOBJ" || data == "RPTINTERESTSTATEMENTOBJ" || data == "RPTINTERESTPRODUCTOBJ" || data == "RPTSETTELMENTREGISTEROBJ" || data == "RPTLOANREGISTERSTATEMENTOBJ" || data == "RPTEMPLOYEEPFLOANHISTORYOBJ")
                {
                    var Lookupvaluesdetails = db.Lookup.Where(e => e.Code == "512").Select(e => e.LookupValues.Where(r => r.IsActive == true)).SingleOrDefault();
                    var a = Lookupvaluesdetails.Select(r => r.LookupVal).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop != null ? drop : null, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTCMSOBJREC")
                {
                    var Lookupvaluesdetails = db.Lookup.Where(e => e.Code == "507").Select(e => e.LookupValues.Where(r => r.IsActive == true)).SingleOrDefault();
                    var a = Lookupvaluesdetails.Select(r => r.LookupVal).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop != null ? drop : null, JsonRequestBehavior.AllowGet);
                }

                else if (data == "RPTCOMPETENCYMODELREC")
                {
                    var a = db.CompetencyModel.Select(e => e.Code).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop != null ? drop : null, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTCOMPETENCYMODELASSIGNMENTREC")
                {
                    var a = db.CompetencyModelAssignment.Select(e => e.BatchName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop != null ? drop : null, JsonRequestBehavior.AllowGet);
                }

                else if (data == "RPTCOMPETENCYEMPLOYEEDATATREC")
                {
                    var a = db.CompetencyBatchProcessT.Select(e => e.ProcessBatch).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop != null ? drop : null, JsonRequestBehavior.AllowGet);
                }

                else if (data == "RPTSUCCESSIONMODELREC")
                {
                    var a = db.SuccessionModel.Select(e => e.Code).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop != null ? drop : null, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTSUCCESSIONMODELASSIGNMENTREC")
                {
                    var a = db.SuccessionModelAssignment.Select(e => e.BatchName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop != null ? drop : null, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTSUCCESSIONEMPLOYEEDATATREC")
                {
                    var a = db.SuccessionBatchProcessT.Select(e => e.ProcessBatch).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop != null ? drop : null, JsonRequestBehavior.AllowGet);
                }

                else if (data == "RPTPTAXSTATEMENTSLABWISEPRO")
                {
                    List<string> range = new List<string>();
                    var dataforrange = db.Range.ToList();
                    foreach (var item in dataforrange)
                    {
                        var from = item.RangeFrom;
                        var to = item.RangeTo;
                        var c = from + " TO " + to;
                        range.Add(c);
                    }
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }

                else if (data == "RPTTARGETENTRYFORMEIS")
                {
                    //List<string> range = new List<string>();
                    //var dateforrange = db.BA_EmpTarget.ToList();
                    //var S = dateforrange.Select(r => r.StartDate).Distinct().ToList();
                    //var E = dateforrange.Select(r => r.EndDate).Distinct().ToList();
                    //foreach(var i in S)
                    //{
                    //    foreach(var j in E)
                    //    {
                    //    var from = i.Value.ToShortDateString();
                    //    var to =j.Value.ToShortDateString();
                    //    var c = from + "-" + to;
                    //    range.Add(c);
                    //    }
                    //}
                    var a = db.BA_SubCategory.Select(b => b.Code).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);

                }

                else if (data == "RPTTARGETENTRYDETAILSFORMEIS")
                {
                    //List<string> range = new List<string>();
                    //var dateforrange = db.BA_EmpTarget.ToList();
                    //var S = dateforrange.Select(r => r.StartDate).Distinct().ToList();
                    //var E = dateforrange.Select(r => r.EndDate).Distinct().ToList();
                    //foreach (var i in S)
                    //{
                    //    foreach (var j in E)
                    //    {
                    //        var from = i.Value.ToShortDateString();
                    //        var to = j.Value.ToShortDateString();
                    //        var c = from + "-" + to;
                    //        range.Add(c);
                    //    }
                    //}
                    var a = db.BA_SubCategory.Select(b => b.Code).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);

                }

                else if (data == "RPTTARGETACHEIVEDEMPWISEEIS")
                {

                    var a = db.BA_SubCategory.Select(b => b.Code).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);

                }
                else if (data == "RPTTARGETACHEIVEDEMPGRANDSUMMARYEIS")
                {

                    var a = db.BA_SubCategory.Select(b => b.Code).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);

                }


                else if (data == "RPTINCREMENTSERVICEBOOKSER" || data == "RPTINCREMENTSERVICEBOOKDETAILSSER")
                {
                    var a = db.IncrActivity.Select(r => r.Name).ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);

                }

                else if (data == "RPTOTHERSERVICEBOOKSER")
                {
                    var a = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "312").FirstOrDefault().LookupValues.ToList().Select(l => l.LookupVal);
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);

                }

                else if (data == "RPTPROMOTIONSERVICEBOOKSER" || data == "RPTPROMOTIONSERVICEBOOKDETAILSSER" || data == "RPTPROMOTIONLETTERSER")
                {
                    var a = db.PromoActivity.Select(r => r.Name).ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);

                }

                else if (data == "RPTLOANADVREPAYMENTTRA" || data == "RPTLOANADVREQUESTTRA" || data == "RPTLOANADVREPAYMENTPRINCIPLEANDINTERESTTRA")
                {
                    var a = db.LoanAdvRequest.Select(b => b.LoanAdvanceHead.SalaryHead.Name).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTFUNCTATTENDANCETTRA" || data == "RPTFUNCTIONALATTENDANCEPROCESSPRO")
                {
                    var a = db.SalaryHead.Include(q => q.Frequency).Include(aa => aa.Type).Where(q => q.Frequency.LookupVal.ToUpper() == "DAILY").Select(b => b.Code).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTYEARLYPAYMENTPAYTRA")
                {
                    var a = db.SalaryHead.Include(q => q.Frequency).Include(aa => aa.Type).Where(q => q.Frequency.LookupVal.ToUpper() == "YEARLY" && q.SalHeadOperationType.LookupVal.ToUpper() != "PERK").Select(b => b.Code).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTINCREMENTLETTERSER")
                {
                    var a = db.IncrActivity.Select(r => r.Name.ToUpper()).ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }

                else if (data == "RPTIRVICTIMEIS")
                {
                    List<string> ProceedingStageList = new List<string>();
                    var a = db.EmpDisciplineProcedings.Select(r => r.ProceedingStage).Distinct().ToList();
                    var dates = new Dictionary<string, int>();
                    var listofstring = new List<string> { "MisconductComplaint", "PreliminaryEnquiry",
                        "PreliminaryEnquiryAction", "ChargeSheet", "ChargeSheetServing", "ChargeSheetReply", 
                        "ChargeSheetEnquiryNotice", "ChargeSheetEnquiryNoticeServing", "ChargeSheetEnquiryProceedings",
                        "ChargeSheetEnquiryReport","PostEnquiryPrerequisite","FinalShowCauseNotice","FinalShowCauseNoticeServing",
                        "FinalShowCauseNoticeReply","FinalShowCauseNoticeClarification","FinalShowCauseNoticeClarificationServing",
                        "PunishmentOrder","PunishmentOrderDelivery", "PunishmentOrderAppeal","PunishmentOrderAppealReply","PunishmentOrderimplementation"};
                    foreach (var item in listofstring)
                    {
                        foreach (var item1 in a)
                        {
                            if (item == "MisconductComplaint" && item1 == 0)
                            {
                                dates.Add(item, item1);
                                break;
                            }
                            if (item == "PreliminaryEnquiry" && item1 == 1)
                            {
                                dates.Add(item, item1);
                                break;
                            }
                            if (item == "PreliminaryEnquiryAction" && item1 == 2)
                            {
                                dates.Add(item, item1);
                                break;
                            }
                            if (item == "ChargeSheet" && item1 == 3)
                            {
                                dates.Add(item, item1);
                                break;
                            }
                            if (item == "ChargeSheetServing" && item1 == 4)
                            {
                                dates.Add(item, item1);
                                break;
                            }
                            if (item == "ChargeSheetReply" && item1 == 5)
                            {
                                dates.Add(item, item1);
                                break;
                            }
                            if (item == "ChargeSheetEnquiryNotice" && item1 == 6)
                            {
                                dates.Add(item, item1);
                                break;
                            }
                            if (item == "ChargeSheetEnquiryNoticeServing" && item1 == 7)
                            {
                                dates.Add(item, item1);
                                break;
                            }
                            if (item == "ChargeSheetEnquiryProceedings" && item1 == 8)
                            {
                                dates.Add(item, item1);
                                break;
                            }
                            if (item == "ChargeSheetEnquiryReport" && item1 == 9)
                            {
                                dates.Add(item, item1);
                                break;
                            }
                            if (item == "PostEnquiryPrerequisite" && item1 == 10)
                            {
                                dates.Add(item, item1);
                                break;
                            }
                            if (item == "FinalShowCauseNotice" && item1 == 11)
                            {
                                dates.Add(item, item1);
                                break;
                            }
                            if (item == "FinalShowCauseNoticeServing" && item1 == 12)
                            {
                                dates.Add(item, item1);
                                break;
                            }
                            if (item == "FinalShowCauseNoticeReply" && item1 == 13)
                            {
                                dates.Add(item, item1);
                                break;
                            }
                            if (item == "FinalShowCauseNoticeClarification" && item1 == 14)
                            {
                                dates.Add(item, item1);
                                break;
                            }
                            if (item == "FinalShowCauseNoticeClarificationServing" && item1 == 15)
                            {
                                dates.Add(item, item1);
                                break;
                            }
                            if (item == "PunishmentOrder" && item1 == 16)
                            {
                                dates.Add(item, item1);
                                break;
                            }
                            if (item == "PunishmentOrderDelivery" && item1 == 17)
                            {
                                dates.Add(item, item1);
                                break;
                            }
                            if (item == "PunishmentOrderAppeal" && item1 == 18)
                            {
                                dates.Add(item, item1);
                                break;
                            }
                            if (item == "PunishmentOrderAppealReply" && item1 == 19)
                            {
                                dates.Add(item, item1);
                                break;
                            }
                            if (item == "PunishmentOrderimplementation" && item1 == 20)
                            {
                                dates.Add(item, item1);
                                break;
                            }
                        }
                    }

                    StringBuilder SB = new StringBuilder();
                    foreach (var item in dates)
                    {
                        SB.Append(item.Key + "_" + item.Value);
                        ProceedingStageList.Add(SB.ToString());
                        SB.Clear();
                    }
                    SelectList drop = new SelectList(ProceedingStageList);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }

                else if (data == "RPTIRVICTIMCASEEIS")
                {
                    var a = db.EmpDisciplineProcedings.Select(r => r.CaseNo).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }

                else if (data == "RPTOTHEREARNINGTTRA")
                {
                    var a = db.SalaryHead.Include(aa => aa.Type).Include(e => e.SalHeadOperationType).Where(q => q.Type.LookupVal.ToUpper() == "EARNING" && q.SalHeadOperationType.LookupVal.ToUpper() == "NONREGULAR").Select(b => b.Code).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTOTHERDEDUCTIONTTRA")
                {
                    var a = db.SalaryHead.Include(aa => aa.Type).Include(e => e.SalHeadOperationType).Where(q => q.Type.LookupVal.ToUpper() == "DEDUCTION" && q.SalHeadOperationType.LookupVal.ToUpper() == "NONREGULAR").Select(b => b.Code).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTPERKTRANSMONTHLYTRA")
                {
                    var a = db.SalaryHead.Include(q => q.Frequency).Include(aa => aa.Type).Where(q => q.SalHeadOperationType.LookupVal.ToUpper() == "PERK").Select(b => b.Code).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTCANDIDATEQUALIFICATIONREC")
                {
                    var a = db.Qualification.Select(b => b.QualificationShortName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTCANDIDATEMASTERREC")
                {
                    var a = db.Qualification.Select(b => b.QualificationShortName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTMANPOWERANALYSISREC")
                {
                    var a = db.ManPowerDetailsBatch.Select(b => b.BatchName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTMANPOWEREMPLOYEEWISECTCREC")
                {
                    var a = db.ManPowerDetailsBatch.Select(b => b.BatchName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTMANPOWERANALYSISCTCSUMMARYREC")
                {
                    var a = db.ManPowerBudget.Select(b => b.GeoStruct.Location.LocationObj.LocDesc + " : " + b.GeoStruct.Department.DepartmentObj.DeptCode).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTRECRUITMENTBATCHINITIATORREC")
                {
                    var a = db.RecruitBatchInitiator.Select(b => b.JobReferenceNo).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTRECRUITMENTINITIATORREC")
                {
                    var a = db.RecruitInitiator.Select(b => b.AdvertiseReferenceNo).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTCANDIDATEEVALUATIONREC")
                {
                    var a = db.RecruitInitiator.Select(b => b.AdvertiseReferenceNo).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTCANDIDATEJOININGREC")
                {
                    var a = db.RecruitInitiator.Select(b => b.AdvertiseReferenceNo).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTCANDIDATEFINALIZATIONREC")
                {
                    var a = db.RecruitInitiator.Select(b => b.AdvertiseReferenceNo).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTRESUMESHORTLISTINGREC")
                {
                    var a = db.RecruitInitiator.Select(b => b.AdvertiseReferenceNo).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTMANPOWERRECOMENDATIONREC")
                {
                    var a = db.ManPowerDetailsBatch.Select(b => b.BatchName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTAPPRAISALSCHEDULEEIS")
                {
                    var a = db.AppraisalSchedule.Select(b => b.BatchName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTAPPRAISALPUBLISHEIS")
                {
                    var a = db.AppraisalSchedule.Select(b => b.BatchName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTAPPRAISALRATINGCONCLUSIONEIS" || data == "RPTAPPRAISALRATINGANALYSISEIS" || data == "RPTAPPRAISALRATINGCONCLUSIONSUMMARYEIS" || data == "RPTEMPLOYEENOTFILLAPPRAISALEIS")
                {
                    var a = db.AppraisalSchedule.Select(b => b.BatchCode).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }

                else if (data == "RPTAPPRAISALEVALUATIONEIS")
                {
                    var a = db.AppraisalSchedule.Select(b => b.BatchName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTSALARYARREARTPRO")
                {
                    var a = db.SalaryArrearT.Select(e => e.ArrearType.LookupVal.ToUpper()).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTSALARYARREARTSUMMARYPRO")
                {
                    var a = db.SalaryArrearPaymentT.Select(e => e.SalaryHead.Name).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTSALHEADFORMULAOBJ")
                {
                    var a = db.SalHeadFormula.Select(e => e.Name).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTSALARYRECONCILIATIONSAL")
                {
                    List<string> range = new List<string>();
                    var a = db.SalaryHead.Distinct().ToList();
                    foreach (var item in a)
                    {
                        var code = item.Code;
                        var name = item.Name;
                        var c = code + "-" + name;
                        range.Add(c);
                    }
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTSALARYRECONCILIATIONEARNSAL")
                {
                    List<string> range = new List<string>();
                    var a = db.SalaryHead.Include(q => q.Type).Where(q => q.Type.LookupVal.ToUpper() == "EARNING").Distinct().ToList();
                    foreach (var item in a)
                    {
                        var code = item.Code;
                        var name = item.Name;
                        var c = code + "-" + name;
                        range.Add(c);
                    }
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTSALARYRECONCILIATIONDEDSAL")
                {
                    List<string> range = new List<string>();
                    var a = db.SalaryHead.Include(q => q.Type).Where(q => q.Type.LookupVal.ToUpper() == "DEDUCTION").Distinct().ToList();
                    foreach (var item in a)
                    {
                        var code = item.Code;
                        var name = item.Name;
                        var c = code + "-" + name;
                        range.Add(c);
                    }
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTSALARYRECONCILIATIONSUMMARYLOCSAL")
                {
                    List<string> range = new List<string>();
                    var a = db.SalaryHead.Distinct().ToList();
                    foreach (var item in a)
                    {
                        var code = item.Code;
                        var name = item.Name;
                        var c = code + "-" + name;
                        range.Add(c);
                    }
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTSALARYRECONCILIATIONSUMMARYSAL")
                {
                    List<string> range = new List<string>();
                    var a = db.SalaryHead.Distinct().ToList();
                    foreach (var item in a)
                    {
                        var code = item.Code;
                        var name = item.Name;
                        var c = code + "-" + name;
                        range.Add(c);
                    }
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTSALARYRECONCILIATIONSUMMARYDEPSAL")
                {
                    List<string> range = new List<string>();
                    var a = db.SalaryHead.Distinct().ToList();
                    foreach (var item in a)
                    {
                        var code = item.Code;
                        var name = item.Name;
                        var c = code + "-" + name;
                        range.Add(c);
                    }
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTSALARYRECONCILIATIONSUMMARYGRADESAL")
                {
                    List<string> range = new List<string>();
                    var a = db.SalaryHead.Distinct().ToList();
                    foreach (var item in a)
                    {
                        var code = item.Code;
                        var name = item.Name;
                        var c = code + "-" + name;
                        range.Add(c);
                    }
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }

                else if (data == "RPTLOANADVREPAYMENTPROCESSPRO")
                {
                    var a = db.LoanAdvRequest.Select(b => b.LoanAdvanceHead.SalaryHead.Name).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTINSURANCEPROCESSPRO")
                {
                    var a = db.InsuranceProduct.Select(b => b.InsuranceProductDesc).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTLVHEADLVR" || data == "RPTLVNEWREQLVR" || data == "RPTLVLEDGERLVR" || data == "RPTLVCREDITPOLICYLVR" || data == "RPTLVBANKLVR" || data == "RPTLVBANKPOLICYLVR" || data == "RPTLVENCASHPOLICYLVR" || data == "RPTLVENCASHPOLICYLVR" || data == "RPTLVDEBITPOLICYLVR" || data == "RPTLVCREDITLVR" || data == "RPTLVCANCELLVR" || data == "RPTLVCANCELREQLVR" || data == "RPTLVOPENBALLVR" || data == "RPTLVOPENBAL3LVR" || data == "RPTLEAVEENCASHREQLVR" || data == "RPTLEAVEENCASHPAYMENTLVR")
                {
                    var a = db.LvHead.Select(r => r.LvName).ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTLEAVEENCASHPROJECTIONLVR")
                {
                    var a = db.LvHead.Where(e => e.EncashRegular == true).Select(r => r.LvName).ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTPAYSLIPPRO")
                {
                    List<string> range = new List<string>();
                    //var a = db.GeoStruct.Where(e => e.Location != null).Select(e => e.Location_Id).Distinct().ToList();

                    //foreach (var Loc in a)
                    //{
                    //    Location locbusin = db.Location.Where(e => e.Id == Loc).FirstOrDefault();

                    //    if (locbusin.BusinessCategory_Id != 0 && locbusin.BusinessCategory_Id != null)
                    //        {
                    //            string name = db.LookupValue.Find(locbusin.BusinessCategory_Id).LookupVal; 
                    //            range.Add(name);
                    //        }  



                    //}
                    var a = db.Bank.ToList();

                    foreach (var Loc in a)
                    {
                        if (Loc.Name != null)
                        {
                            string name = Loc.Name;
                            range.Add(name);
                        }

                    }

                    range = range.Distinct().ToList();
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }

                else if (data == "RPTBANKSTATEMENTBAN")
                {
                    List<string> range = new List<string>();

                    var a = db.Bank.ToList();

                    foreach (var Loc in a)
                    {
                        if (Loc.Name != null)
                        {
                            string name = Loc.Name;
                            range.Add(name);
                        }

                    }

                    range = range.Distinct().ToList();
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }

                else if (data == "RPTSALREGISTERSRG")
                {
                    List<string> range = new List<string>();
                    //var a = db.GeoStruct.Where(e => e.Location != null).Select(e => e.Location_Id).Distinct().ToList();

                    //foreach (var Loc in a)
                    //{
                    //    Location locbusin = db.Location.Where(e => e.Id == Loc).FirstOrDefault();

                    //    if (locbusin.BusinessCategory_Id != 0 && locbusin.BusinessCategory_Id != null)
                    //    {
                    //        string name = db.LookupValue.Find(locbusin.BusinessCategory_Id).LookupVal;
                    //        range.Add(name);
                    //    }

                    //}

                    var a = db.Bank.ToList();

                    foreach (var Loc in a)
                    {
                        if (Loc.Name != null)
                        {
                            string name = Loc.Name;
                            range.Add(name);
                        }

                    }
                    range = range.Distinct().ToList();
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }

                else if (data == "RPTSALREGISTERSUMMARYSRG")
                {
                    List<string> range = new List<string>();

                    var Paybanklist = db.Bank.ToList();

                    foreach (var Paybank in Paybanklist)
                    {
                        if (Paybank.Name != null)
                        {
                            string name = Paybank.Name;
                            range.Add(name);
                        }

                    }
                    range = range.Distinct().ToList();
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTSALEARNDEDSUMMARYEDS")
                {
                    List<string> range = new List<string>();

                    var a = db.Bank.ToList();

                    foreach (var Loc in a)
                    {
                        if (Loc.Name != null)
                        {
                            string name = Loc.Name;
                            range.Add(name);
                        }

                    }
                    range = range.Distinct().ToList();
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTSALEARNDEDSUMMARYDETAILSEDD")
                {
                    List<string> range = new List<string>();

                    var a = db.Bank.ToList();

                    foreach (var Loc in a)
                    {
                        if (Loc.Name != null)
                        {
                            string name = Loc.Name;
                            range.Add(name);
                        }

                    }
                    range = range.Distinct().ToList();
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }

                else if (data == "RPTEMPOFFMAS")
                {
                    List<string> range = new List<string>();
                    //var a = db.GeoStruct.Where(e => e.Location != null).Select(e => e.Location_Id).Distinct().ToList();

                    //foreach (var Loc in a)
                    //{
                    //    Location locbusin = db.Location.Where(e => e.Id == Loc).FirstOrDefault();

                    //    if (locbusin.BusinessCategory_Id != 0 && locbusin.BusinessCategory_Id != null)
                    //    {
                    //        string name = db.LookupValue.Find(locbusin.BusinessCategory_Id).LookupVal;
                    //        range.Add(name);
                    //    }

                    //}

                    var a = db.Bank.ToList();

                    foreach (var Loc in a)
                    {
                        if (Loc.Name != null)
                        {
                            string name = Loc.Name;
                            range.Add(name);
                        }

                    }
                    range = range.Distinct().ToList();
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }



                else if (data == "RPTEMPLOYEEMAS")
                {
                    List<string> range = new List<string>();
                    //var a = db.GeoStruct.Where(e => e.Location != null).Select(e => e.Location_Id).Distinct().ToList();

                    //foreach (var Loc in a)
                    //{
                    //    Location locbusin = db.Location.Where(e => e.Id == Loc).FirstOrDefault();

                    //    if (locbusin.BusinessCategory_Id != 0 && locbusin.BusinessCategory_Id != null)
                    //    {
                    //        string name = db.LookupValue.Find(locbusin.BusinessCategory_Id).LookupVal;
                    //        range.Add(name);
                    //    }

                    //}

                    var a = db.Bank.ToList();

                    foreach (var Loc in a)
                    {
                        if (Loc.Name != null)
                        {
                            string name = Loc.Name;
                            range.Add(name);
                        }

                    }
                    range = range.Distinct().ToList();
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }



                else if (data == "RPTBRANCHSUMMARYDETAILSBRA")
                {
                    List<string> range = new List<string>();
                    //var a = db.GeoStruct.Where(e => e.Location != null).Select(e => e.Location_Id).Distinct().ToList();

                    //foreach (var Loc in a)
                    //{
                    //    Location locbusin = db.Location.Where(e => e.Id == Loc).FirstOrDefault();

                    //    if (locbusin.BusinessCategory_Id != 0 && locbusin.BusinessCategory_Id != null)
                    //    {
                    //        string name = db.LookupValue.Find(locbusin.BusinessCategory_Id).LookupVal;
                    //        range.Add(name);
                    //    }

                    //}

                    var a = db.Bank.ToList();

                    foreach (var Loc in a)
                    {
                        if (Loc.Name != null)
                        {
                            string name = Loc.Name;
                            range.Add(name);
                        }

                    }
                    range = range.Distinct().ToList();
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);



                }



                else if (data == "RPTGRANDSUMMARYDETAILSGRA")
                {
                    List<string> range = new List<string>();
                    //var a = db.GeoStruct.Where(e => e.Location != null).Select(e => e.Location_Id).Distinct().ToList();

                    //foreach (var Loc in a)
                    //{
                    //    Location locbusin = db.Location.Where(e => e.Id == Loc).FirstOrDefault();

                    //    if (locbusin.BusinessCategory_Id != 0 && locbusin.BusinessCategory_Id != null)
                    //    {
                    //        string name = db.LookupValue.Find(locbusin.BusinessCategory_Id).LookupVal;
                    //        range.Add(name);
                    //    }

                    //}

                    var a = db.Bank.ToList();

                    foreach (var Loc in a)
                    {
                        if (Loc.Name != null)
                        {
                            string name = Loc.Name;
                            range.Add(name);
                        }

                    }
                    range = range.Distinct().ToList();
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }

                else if (data == "RPTEARNINGSTATEMENTPRO")
                {
                    List<string> range = new List<string>();
                    var a = db.SalaryHead.Include(q => q.Frequency).Include(aa => aa.Type).Where(q => q.Type.LookupVal.ToUpper() == "EARNING" && q.InPayslip == true).Distinct().ToList();
                    foreach (var item in a)
                    {
                        var name = item.Name;
                        var code = item.Code;
                        var c = name + "-" + code;
                        range.Add(c);
                    }
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTDEDUCTIONSTATEMENTPRO")
                {
                    List<string> range = new List<string>();
                    var salname = db.SalaryHead.Include(q => q.Frequency).Include(aa => aa.Type).Where(q => q.Type.LookupVal.ToUpper() == "DEDUCTION" && q.InPayslip == true).Distinct().ToList();
                    foreach (var item in salname)
                    {
                        var a = item.Name;
                        var b = item.Code;
                        var c = a + "-" + b;
                        range.Add(c);
                    }
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTINVESTMENTPAYMENTITR")
                {
                    //var a = db.ITSection.Include(e => e.ITSectionList).Include(e => e.ITSectionListType).Where(e => e.ITSectionListType.LookupVal.ToUpper() == "REBATE" || e.ITSectionListType.LookupVal.ToUpper() == "DEDUCT").Select(e => e.ITSectionList.LookupVal).ToList();
                    var a = db.ITInvestment.Select(e => e.ITInvestmentName).ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTITSECTION10PAYMENTITR")
                {
                    var a = db.ITSection.Include(e => e.ITSectionList).Include(e => e.ITSectionListType).Where(e => e.ITSectionListType.LookupVal.ToUpper() == "SECTION10B").Select(e => e.ITSectionList.LookupVal).ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTLVDETAILSLVR")
                {
                    var a = db.LvNewReq.Select(e => e.LeaveHead.LvName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTJVGRANDSUMMARYJGS")
                {
                    var a = db.JVProcessDataSummary.Select(e => e.BatchName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTJVSUMMARYPRO")
                {
                    var a = db.JVProcessDataSummary.Select(e => e.BatchName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTJVPARAMETEROBJ")
                {
                    var a = db.JVParameter.Select(e => e.JVGroup.LookupVal.ToUpper()).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTATTENDANCEREPORTBUILDERATT")
                {
                    var a = db.RemarkConfig.Select(e => e.MusterRemarks.LookupVal.ToUpper()).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTEXCEPTIONALREMARKATT")
                {
                    var a = db.RemarkConfig.Select(e => e.MusterRemarks.LookupVal.ToUpper()).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTJVREPORTDETAILBRANCHWISEPRO")
                {
                    var a = db.JVProcessDataSummary.Select(e => e.BatchName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTJVREPORTDETAILINDIVIDUALPRO")
                {
                    var a = db.JVProcessDataSummary.Select(e => e.BatchName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTLEAVECREDITTRAILBALANCELVR")
                {
                    var a = db.LvHead.Select(e => e.LvName).ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data2 == "112")
                {
                    SelectList s = (SelectList)null;
                    //var a = db.Lookup.Include(q=>q.LookupValues).Where(e => e.Name == "Caste").Select(e => e.LookupValues).ToList();

                    var a = db.Lookup.Where(e => e.Name == "Caste").Select(e => e.LookupValues).SingleOrDefault();
                    List<string> te = new List<string>();
                    foreach (var item in a)
                    {
                        te.Add(item.LookupVal);
                    }
                    s = new SelectList(te);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else if (data2 == "114")
                {
                    SelectList s = (SelectList)null;
                    //var a = db.Lookup.Include(q=>q.LookupValues).Where(e => e.Name == "Caste").Select(e => e.LookupValues).ToList();

                    var a = db.Lookup.Where(e => e.Code == "104").Select(e => e.LookupValues).SingleOrDefault();
                    List<string> te = new List<string>();
                    foreach (var item in a)
                    {
                        te.Add(item.LookupVal);
                    }
                    s = new SelectList(te);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                //else if (data2 == "108")
                //{
                //    SelectList s = (SelectList)null;
                //    //var a = db.Lookup.Include(q=>q.LookupValues).Where(e => e.Name == "Caste").Select(e => e.LookupValues).ToList();

                //    var a = db.Lookup.Where(e => e.Name == "Emp Status").Select(e => e.LookupValues).SingleOrDefault();
                //    List<string> te = new List<string>();
                //    foreach (var item in a)
                //    {
                //        te.Add(item.LookupVal);
                //    }
                //    s = new SelectList(te);
                //    return Json(s, JsonRequestBehavior.AllowGet);
                //}
                else if (data2 == "148")
                {
                    var a = db.Disease.Select(t => t.Name).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data2 == "143")
                {
                    var a = db.Taluka.Select(e => e.Name).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data2 == "144")
                {
                    var a = db.Taluka.Select(e => e.Name).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data2 == "145")
                {
                    var a = db.Taluka.Select(e => e.Name).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data2 == "161")
                {
                    var a = db.Employee.Select(e => e.TimingCode).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data2 == "169")
                {
                    var a = db.OthServiceBookActivity.Select(e => e.Name).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data2 == "146")
                {
                    var a = db.Allergy.Select(e => e.Name).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }

                else if (data2 == "113")
                {
                    SelectList s = (SelectList)null;
                    var a = db.Lookup.Where(e => e.Name == "Religion").Select(e => e.LookupValues).SingleOrDefault();
                    List<string> te = new List<string>();
                    foreach (var item in a)
                    {
                        te.Add(item.LookupVal);
                    }
                    s = new SelectList(te);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else if (data2 == "115")
                {
                    SelectList s = (SelectList)null;
                    var a = db.Lookup.Where(e => e.Name == "Subcaste").Select(e => e.LookupValues).SingleOrDefault();
                    List<string> te = new List<string>();
                    foreach (var item in a)
                    {
                        te.Add(item.LookupVal);
                    }
                    s = new SelectList(te);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }

                else if (data2 == "118")
                {
                    SelectList s = (SelectList)null;
                    var a = db.Lookup.Where(e => e.Name == "Blood").Select(e => e.LookupValues).SingleOrDefault();
                    List<string> te = new List<string>();
                    foreach (var item in a)
                    {
                        te.Add(item.LookupVal);
                    }
                    s = new SelectList(te);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else if (data2 == "123")
                {
                    SelectList s = (SelectList)null;
                    var a = db.Lookup.Where(e => e.Name == "GENDER").Select(e => e.LookupValues).SingleOrDefault();
                    List<string> te = new List<string>();
                    foreach (var item in a)
                    {
                        te.Add(item.LookupVal);
                    }
                    s = new SelectList(te);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }

                else if (data2 == "120")
                {
                    SelectList s = (SelectList)null;
                    var a = db.Lookup.Where(e => e.Name == "MARITAL").Select(e => e.LookupValues).SingleOrDefault();
                    List<string> te = new List<string>();
                    foreach (var item in a)
                    {
                        te.Add(item.LookupVal);
                    }
                    s = new SelectList(te);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else if (data2 == "122")
                {
                    var a = db.Language.Select(e => e.LanguageName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data.ToUpper() == "RPTEMPQUALIFICATIONMIS")
                {
                    var a = db.Qualification.Select(e => e.QualificationShortName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data2 == "121")
                {
                    var a = db.Skill.Select(e => e.Name).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data2 == "127")
                {
                    var a = db.Hobby.Select(e => e.HobbyName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data2 == "105")
                {
                    SelectList s = (SelectList)null;
                    var a = db.Lookup.Where(e => e.Name == "Visa").Select(e => e.LookupValues).SingleOrDefault();
                    List<string> te = new List<string>();
                    foreach (var item in a)
                    {
                        te.Add(item.LookupVal);
                    }
                    s = new SelectList(te);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else if (data2 == "147")
                {
                    var a = db.Awards.Select(e => e.Name).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data2 == "151")
                {
                    var a = db.Scolarship.Select(e => e.Name).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data2 == "148")
                {
                    var a = db.Disease.Select(e => e.Name).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTHIERARCHYEIS")
                {
                    var a = db.ReportingStructRights.Select(e => e.FuncModules.LookupVal).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTYEARLYPROGRAMLISTEIS")
                {
                    var a = db.ProgramList.Select(b => b.SubjectDetails).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTTRAININGSCHEDULEEIS")
                {
                    var a = db.TrainingSchedule.Select(b => b.TrainingBatchName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTEMPLOYEEWISETRAININGEIS")
                {
                    var a = db.TrainingSchedule.Select(b => b.TrainingBatchName).Distinct().ToList();
                    List<string> range = new List<string>();
                    foreach (var item in a)
                    {
                        if (item != null)
                        {
                            range.Add(item);
                        }

                    }
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTSESSIONWISEEMPLOYEETRAININGEIS")
                {
                    var a = db.TrainingSchedule.Select(b => b.TrainingBatchName).Distinct().ToList();
                     List<string> range = new List<string>();
                    foreach (var item in a)
                    {
                        if (item != null)
                        {
                            range.Add(item);
                        }

                    }
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTSESSIONWISETRAININGFEEDBACKEIS")
                {
                    var a = db.TrainingSchedule.Distinct().Select(b => b.TrainingBatchName).ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTSESSIONWISETRAININGEVALUATIONEIS")
                {
                    var a = db.TrainingSchedule.Select(b => b.TrainingBatchName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTSESSIONWISEPROGRAMATTENDANCEEIS")
                {
                    var a = db.TrainingSchedule.Select(b => b.TrainingBatchName).Distinct().ToList();
                    List<string> range = new List<string>();
                    foreach (var item in a)
                    {
                        if (item != null)
                        {
                            range.Add(item);
                        }

                    }
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTPROGRAMWISEATTENDANCEEIS")
                {
                    var a = db.TrainingSchedule.Select(b => b.TrainingBatchName).Distinct().ToList();
                    List<string> range = new List<string>();
                    foreach (var item in a)
                    {
                        if (item != null)
                        {
                            range.Add(item);
                        }

                    }
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTTRAININGFEEDBACKEIS")
                {
                    var a = db.TrainingSchedule.Select(b => b.TrainingBatchName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTTRAININGEVALUATIONEIS")
                {
                    var a = db.TrainingSchedule.Select(b => b.TrainingBatchName).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }

                else if (data == "RPTSALARYARREARHEADWISEPRO")
                {
                    var a = db.SalaryArrearPaymentT.Select(e => e.SalaryHead.Name).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTSALDEDSUMMARYSDS")
                {
                    var a = db.SalaryHead.Include(aa => aa.Type).Where(q => q.Type.LookupVal.ToUpper() == "DEDUCTION").Select(e => e.Name).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);

                }
                else if (data == "RPTSALEARNSUMMARYSES")
                {
                    var a = db.SalaryHead.Include(aa => aa.Type).Where(q => q.Type.LookupVal.ToUpper() == "EARNING").Select(e => e.Code).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);

                }
                else if (data == "RPTLEAVENOTCREDITLVR")
                {
                    var a = db.LvNewReq.Include(t => t.LeaveHead).Where(t => t.LeaveHead != null && t.LvCreditDate != null).Select(t => t.LeaveHead.LvCode).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);

                }
                else if (data == "RPTEARNINGSUMMARYSUM")
                {
                    var a = db.SalaryHead.Include(aa => aa.Type).Where(q => q.Type.LookupVal.ToUpper() == "EARNING").Select(e => e.Name).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop, JsonRequestBehavior.AllowGet);

                }

                else
                {
                    return null;

                }

            }
        }

        public ActionResult GetSpecialFilterLevel1(string data, string[] data2, string saa)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                if (data == "RPTSESSIONWISEEMPLOYEETRAININGEIS" || data == "RPTSESSIONWISETRAININGFEEDBACKEIS" || data == "RPTSESSIONWISETRAININGEVALUATIONEIS" || data == "RPTSESSIONWISEPROGRAMATTENDANCEEIS")
                {
                    if (data2[0] != null)
                    {
                        string a1 = data2[0];

                        List<string> range = new List<string>();
                        //var a = db.TrainingSchedule
                        //    .Include(e => e.TrainingSession)
                        //    .Include(e => e.TrainingSession
                        //    .Select(t => t.SessionType))
                        //    .Where(e => e.TrainingBatchName == a1).SingleOrDefault();

                      
                        var res = db.TrainingSchedule.Where(e => e.TrainingBatchName == a1).Select(r => new
                        {                         
                            TrainingSession = r.TrainingSession.Select(s => new
                            {
                                SessionTypeLookupVal = s.SessionType.LookupVal,
                                SessionTypeId = s.SessionType.Id
                            }).ToList()
                        }).FirstOrDefault();

                        if (res != null)
                        {
                            var a = res.TrainingSession.GroupBy(x => x.SessionTypeLookupVal).Select(y => y.First()).ToList();

                            if (a != null)
                            {
                                foreach (var item in a)
                                {
                                    var c = item.SessionTypeLookupVal;
                                    range.Add(c);
                                }

                            }
                            SelectList drop = new SelectList(range);
                            return Json(drop, JsonRequestBehavior.AllowGet);
                        }
                    }

                    else
                    {
                        var a = db.TrainingSession.Include(e => e.SessionType).Select(t => t.SessionType.LookupVal).Distinct().ToList();
                        SelectList drop = new SelectList(a);
                        return Json(drop, JsonRequestBehavior.AllowGet);
                    }

                    return null;

                }
                else if (data == "RPTMANPOWERANALYSISCTCSUMMARYREC")
                {
                    List<string> range = new List<string>();

                    string[] values = saa.Split(new string[] { " : " }, StringSplitOptions.None);
                    string locationName = values[0].Trim();
                    //string deptCode = values.Length > 1 ? values[1].Trim() : "";  && (string.IsNullOrEmpty(deptCode) || e.GeoStruct.Department.DepartmentObj.DeptCode == deptCode)
                    var manpowerdata = db.ManPowerBudget
                                          .Include(e => e.GeoStruct)
                                          .Include(e => e.GeoStruct.Location.LocationObj)
                                          .Include(e => e.FuncStruct)
                                          .Include(e => e.FuncStruct.Job)
                                          .Where(e => e.GeoStruct.Location.LocationObj.LocDesc == locationName)
                                          .ToList();

                    foreach (var dataEntry in manpowerdata)
                    {
                        if (dataEntry.GeoStruct != null && dataEntry.FuncStruct != null)
                        {
                            string funcStructName = dataEntry.FuncStruct.Job.Name;
                            range.Add(funcStructName);
                        }
                    }

                    range = range.Distinct().ToList();
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTAPPRAISALRATINGCONCLUSIONEIS")
                {
                    List<string> range = new List<string>();

                    var a = db.Lookup.Where(e => e.Code == "1068").Select(e => e.LookupValues.Where(r => r.IsActive == true)).SingleOrDefault();

                    foreach (var Loc in a)
                    {
                        if (Loc.LookupVal != null)
                        {
                            string name = Loc.LookupVal;
                            range.Add(name);
                        }

                    }
                    range = range.Distinct().ToList();
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                //else if (data == "RPTAPPRAISALEVALUATIONEIS") //HR Recommend For Increment
                //{
                //    List<string> range = new List<string>() { "Recommend  For Further Discussion", "Recommend For Increment", "Recommend For Promotion", "Recommend For Training" };

                //    //var a = db.Lookup.Where(e => e.Code == "--").Select(e => e.LookupValues.Where(r => r.IsActive == true)).SingleOrDefault();

                   
                //    range = range.Distinct().ToList();
                //    SelectList drop = new SelectList(range);
                //    return Json(drop, JsonRequestBehavior.AllowGet);
                //}

                else if (data == "RPTCMSOBJREC")
                {
                    List<string> range = new List<string>();
                    var Lookupvaluesdetails = db.Lookup.Where(e => e.Code == "507").Select(e => e.LookupValues.Where(r => r.IsActive == true)).SingleOrDefault();
                    var a = Lookupvaluesdetails.Select(r => r.LookupVal).Distinct().ToList();
                    string[] values = (saa.Split(new string[] { "," }, StringSplitOptions.None));
                    foreach (var collection in values)
                    {
                        foreach (var sa in a)
                        {

                            if (sa == collection)
                            {
                                if (sa == "AppraisalAttributeModel")
                                {
                                    var Attribute = db.AppraisalAttributeModel.ToList();

                                    foreach (var item1 in Attribute)
                                    {
                                        string name = item1.Code;
                                        range.Add(name);
                                    }
                                }


                                if (sa == "AppraisalBusinessAppraisalModel")
                                {
                                    var Business = db.AppraisalBusinessAppraisalModel.ToList();
                                    foreach (var item1 in Business)
                                    {
                                        string name = item1.Code;
                                        range.Add(name);
                                    }
                                }



                                if (sa == "AppraisalKRAModel")
                                {
                                    var KRA = db.AppraisalKRAModel.ToList();

                                    foreach (var item1 in KRA)
                                    {
                                        string name = item1.Code;
                                        range.Add(name);
                                    }
                                }




                                if (sa == "AppraisalPotentialModel")
                                {
                                    var Potential = db.AppraisalPotentialModel.ToList();

                                    foreach (var item1 in Potential)
                                    {
                                        string name = item1.Code;
                                        range.Add(name);
                                    }
                                }




                                if (sa == "PastExperienceModel")
                                {
                                    var Experience = db.PastExperienceModel.ToList();

                                    foreach (var item1 in Experience)
                                    {
                                        string name = item1.Code;
                                        range.Add(name);
                                    }
                                }




                                if (sa == "PersonnelModel")
                                {
                                    var Personnel = db.PersonnelModel.ToList();

                                    foreach (var item1 in Personnel)
                                    {
                                        string name = item1.Code;
                                        range.Add(name);
                                    }
                                }




                                if (sa == "QualificationModel")
                                {
                                    var Qualification = db.QualificationModel.ToList();

                                    foreach (var item1 in Qualification)
                                    {
                                        string name = item1.Code;
                                        range.Add(name);
                                    }
                                }



                                if (sa == "ServiceModel")
                                {
                                    var Service = db.ServiceModel.ToList();

                                    foreach (var item1 in Service)
                                    {
                                        string name = item1.Code;
                                        range.Add(name);
                                    }
                                }



                                if (sa == "SkillModel")
                                {
                                    var Skill = db.SkillModel.ToList();

                                    foreach (var item1 in Skill)
                                    {
                                        string name = item1.Code;
                                        range.Add(name);
                                    }
                                }





                                if (sa == "TrainingModel")
                                {
                                    var Training = db.TrainingModel.ToList();

                                    foreach (var item1 in Training)
                                    {
                                        string name = item1.Code;
                                        range.Add(name);
                                    }
                                }

                            }
                        }
                    }

                    range = range.Distinct().ToList();

                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);

                    //var expression = data2[0];
                    //switch (expression)
                    //{
                    //    case "AppraisalAttributeModel":
                    //        List<string> rangea = new List<string>();

                    //        var a = db.AppraisalAttributeModel.ToList();

                    //        foreach (var item in a)
                    //        {
                    //                string name = item.Code;
                    //                rangea.Add(name);
                    //        }
                    //        rangea = rangea.Distinct().ToList();
                    //        SelectList dropa = new SelectList(rangea);
                    //        return Json(dropa, JsonRequestBehavior.AllowGet);
                    //        break;



                    //    case "AppraisalBusinessAppraisalModel":
                    //        List<string> rangeb = new List<string>();

                    //        var b = db.AppraisalBusinessAppraisalModel.ToList();

                    //        foreach (var item in b)
                    //        {
                    //                string name = item.Code;
                    //                rangeb.Add(name);
                    //        }
                    //        rangeb = rangeb.Distinct().ToList();
                    //        SelectList dropb = new SelectList(rangeb);
                    //        return Json(dropb, JsonRequestBehavior.AllowGet);
                    //        break;

                    //    case "AppraisalKRAModel":
                    //         List<string> rangek = new List<string>();

                    //        var k = db.AppraisalBusinessAppraisalModel.ToList();

                    //        foreach (var item in k)
                    //        {
                    //                string name = item.Code;
                    //                rangek.Add(name);
                    //        }
                    //        rangek = rangek.Distinct().ToList();
                    //        SelectList dropk = new SelectList(rangek);
                    //        return Json(dropk, JsonRequestBehavior.AllowGet);
                    //        break;

                    //    case "AppraisalPotentialModel":
                    //        List<string> rangep = new List<string>();

                    //        var p = db.AppraisalPotentialModel.ToList();

                    //        foreach (var item in p)
                    //        {
                    //            string name = item.Code;
                    //            rangep.Add(name);
                    //        }
                    //        rangep = rangep.Distinct().ToList();
                    //        SelectList dropp = new SelectList(rangep);
                    //        return Json(dropp, JsonRequestBehavior.AllowGet);
                    //        break;

                    //    case "PastExperienceModel":
                    //        List<string> rangeps = new List<string>();

                    //        var ps = db.PastExperienceModel.ToList();

                    //        foreach (var item in ps)
                    //        {
                    //            string name = item.Code;
                    //            rangeps.Add(name);
                    //        }
                    //        rangeps = rangeps.Distinct().ToList();
                    //        SelectList dropps = new SelectList(rangeps);
                    //        return Json(dropps, JsonRequestBehavior.AllowGet);
                    //        break;

                    //    case "PersonnelModel":
                    //        List<string> rangepe = new List<string>();

                    //        var pe = db.PersonnelModel.ToList();

                    //        foreach (var item in pe)
                    //        {
                    //            string name = item.Code;
                    //            rangepe.Add(name);
                    //        }
                    //        rangepe = rangepe.Distinct().ToList();
                    //        SelectList droppe = new SelectList(rangepe);
                    //        return Json(droppe, JsonRequestBehavior.AllowGet);
                    //        break;

                    //    case "QualificationModel":
                    //        List<string> rangeq = new List<string>();

                    //        var q = db.QualificationModel.ToList();

                    //        foreach (var item in q)
                    //        {
                    //            string name = item.Code;
                    //            rangeq.Add(name);
                    //        }
                    //        rangeq = rangeq.Distinct().ToList();
                    //        SelectList dropq = new SelectList(rangeq);
                    //        return Json(dropq, JsonRequestBehavior.AllowGet);
                    //        break;

                    //    case "ServiceModel":

                    //        List<string> rangese = new List<string>();

                    //        var se = db.ServiceModel.ToList();

                    //        foreach (var item in se)
                    //        {
                    //            string name = item.Code;
                    //            rangese.Add(name);
                    //        }
                    //        rangese = rangese.Distinct().ToList();
                    //        SelectList dropse = new SelectList(rangese);
                    //        return Json(dropse, JsonRequestBehavior.AllowGet);
                    //        break;

                    //    case "SkillModel":

                    //        List<string> rangesk = new List<string>();

                    //        var sk = db.SkillModel.ToList();

                    //        foreach (var item in sk)
                    //        {
                    //            string name = item.Code;
                    //            rangesk.Add(name);
                    //        }
                    //        rangesk = rangesk.Distinct().ToList();
                    //        SelectList dropsk = new SelectList(rangesk);
                    //        return Json(dropsk, JsonRequestBehavior.AllowGet);
                    //        break;

                    //    case "TrainingModel":

                    //        List<string> rangetr = new List<string>();

                    //        var tr = db.TrainingModel.ToList();

                    //        foreach (var item in tr)
                    //        {
                    //            string name = item.Code;
                    //            rangetr.Add(name);
                    //        }
                    //        rangetr = rangetr.Distinct().ToList();
                    //        SelectList droptr = new SelectList(rangetr);
                    //        return Json(droptr, JsonRequestBehavior.AllowGet);
                    //        break;

                    //    default:

                    //        break;

                    //}     
                }
                else if (data == "RPTINSURANCEPROCESSPRO")
                {
                    List<string> range = new List<string>();

                    var a = db.Bank.ToList();

                    foreach (var Loc in a)
                    {
                        if (Loc.Name != null)
                        {
                            string name = Loc.Name;
                            range.Add(name);
                        }

                    }
                    range = range.Distinct().ToList();
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTSALDEDSUMMARYSDS")
                {
                    List<string> range = new List<string>();

                    var a = db.Bank.ToList();
                    foreach (var i in a)
                    {
                        if (i.Name != null)
                        {
                            string name = i.Name;
                            range.Add(name);
                        }

                    }
                    range = range.Distinct().ToList();
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }
                else if (data == "RPTSALEARNSUMMARYSES")
                {
                    List<string> range = new List<string>();

                    var a = db.Bank.ToList();
                    foreach (var i in a)
                    {
                        if (i.Name != null)
                        {
                            string name = i.Name;
                            range.Add(name);
                        }

                    }
                    range = range.Distinct().ToList();
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }

                else if (data == "RPTSALARYARREARTPRO")
                {
                    List<string> range = new List<string>();

                    var a = db.Bank.ToList();

                    foreach (var Loc in a)
                    {
                        if (Loc.Name != null)
                        {
                            string name = Loc.Name;
                            range.Add(name);
                        }

                    }
                    range = range.Distinct().ToList();
                    SelectList drop = new SelectList(range);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }

                else if (data == "RPTEMPBRANCHWISEPRO")
                {
                    var d = db.Job.OrderBy(e => e.Name).ToList().Select(r => r.Name);
                    SelectList drop = new SelectList(d);
                    return Json(drop, JsonRequestBehavior.AllowGet);
                }

                else
                {
                    return null;

                }

            }
        }


        public ActionResult GetSpecialFilterLevel2(string data, string[] data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                if (data == "RPTSESSIONWISEEMPLOYEETRAININGEIS" || data == "RPTSESSIONWISETRAININGFEEDBACKEIS" || data == "RPTSESSIONWISETRAININGEVALUATIONEIS" || data == "RPTSESSIONWISEPROGRAMATTENDANCEEIS")
                {
                    if (data2[0] != null)
                    {
                        var a1 = data2[0];


                        List<string> range = new List<string>();
                        //var a = db.TrainingSchedule
                        //    .Include(e => e.TrainingSession)
                        //    .Include(e => e.TrainingSession.Select(t => t.TrainingProgramCalendar))
                        //    .Include(e => e.TrainingSession.Select(t => t.TrainingProgramCalendar.ProgramList))
                        //    .Where(e => e.TrainingBatchCode == a1).SingleOrDefault();

                        var res = db.TrainingSchedule.Where(e => e.TrainingBatchName == a1).Select(r => new
                        {
                            TrainingSession = r.TrainingSession.Select(s => new
                            {
                                ProgramList = s.TrainingProgramCalendar.ProgramList.SubjectDetails,
                                ProgramListId = s.TrainingProgramCalendar.ProgramList.Id
                            }).ToList()
                        }).SingleOrDefault();

                        if (res != null)
                        {
                            var a = res.TrainingSession.GroupBy(x => x.ProgramList).Select(y => y.First()).ToList();

                            if (a != null)
                            {
                                foreach (var item in a)
                                {
                                    var c = item.ProgramList;
                                    range.Add(c);
                                }

                            }
                            SelectList drop = new SelectList(range);
                            return Json(drop, JsonRequestBehavior.AllowGet);
                        }                 
                    }
                    else
                    {
                        var a = db.ProgramList.Select(t => t.SubjectDetails).Distinct().ToList();
                        SelectList drop = new SelectList(a);
                        return Json(drop, JsonRequestBehavior.AllowGet);
                    }

                }
               
               return null;

            }
        }

        public ActionResult Getothservbookactivity(string data, string data2)
        {
            // var a = db.OtherServiceBook.Select(e => e.OthServiceBookActivity.OtherSerBookActList.LookupVal).ToList();
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.OthServiceBookActivity.Select(e => e.OtherSerBookActList.LookupVal).ToList();
                SelectList drop = new SelectList(a);
                return Json(drop, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Getlvhead(string data, string data2)
        {
            // var a = db.OtherServiceBook.Select(e => e.OthServiceBookActivity.OtherSerBookActList.LookupVal).ToList();
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.LvNewReq.Select(e => e.LeaveHead.LvName).Distinct().ToList();
                SelectList drop = new SelectList(a);
                return Json(drop, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetInsuranceheads(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.Insurance.Select(e => e.InsuranceDesc).Distinct().ToList();
                SelectList drop = new SelectList(a);
                return Json(drop, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetJobSatus(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.PayStruct
                    .Include(e => e.JobStatus.EmpActingStatus)
                    .Include(e => e.JobStatus.EmpStatus).Where(e => e.JobStatus != null && e.JobStatus.EmpActingStatus != null && e.JobStatus.EmpStatus != null)
                    .Select(e => new { Id = e.JobStatus.Id, fulldata = "EmpStatus :" + e.JobStatus.EmpStatus.LookupVal + "EmpActingStatus :" + e.JobStatus.EmpActingStatus.LookupVal }).Distinct().ToList();
                SelectList drop = new SelectList(a, "Id", "fulldata");
                return Json(drop, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetSpecialGroup(string data)
        {
            String path = Server.MapPath("~/App_Data/Menu_Json/SpecialGroup.xml");

            try
            {
                List<string> query = new List<string>();

                var ttt = XElement.Load(path).Elements("Rptname");
                foreach (XElement level1Element in ttt)
                {
                    var chk = level1Element.Attribute("name").Value;
                    if (chk == data)
                    {
                        var lv2 = level1Element.Elements("column");
                        foreach (XElement level2Element in lv2)
                        {
                            query.Add(level2Element.Attribute("name").Value);
                        }
                    }
                }
                if (query.Count != 0)
                {
                    var result = (from c in query
                                  select new { value = c, LookupVal = c });
                    var s = new SelectList(result, "value", "LookupVal", "");
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception)
            {

                throw;
            }
            return null;
        }

        public ActionResult GetReportname(string data)
        {
            String path = Server.MapPath("~/App_Data/Menu_Json/ReportName.xml");

            try
            {
                List<string> query = new List<string>();

                var ttt = XElement.Load(path).Elements("Rptname");
                foreach (XElement level1Element in ttt)
                {
                    var chk = level1Element.Attribute("name").Value;
                    if (chk == data)
                    {
                        var lv2 = level1Element.Elements("column");
                        foreach (XElement level2Element in lv2)
                        {
                            query.Add(level2Element.Attribute("name").Value);
                        }
                    }
                }
                if (query.Count != 0)
                {
                    var result = (from c in query
                                  select new { value = c, LookupVal = c });
                    var s = new SelectList(result, "value", "LookupVal", "");
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception)
            {

                throw;
            }
            return null;
        }
        public class returndataclass
        {
            public string data { get; set; }
            public string name { get; set; }
            public Int32 Id { get; set; }
        }
        //public class returndataclass
        //{
        //    public string data { get; set; }
        //    public string name { get; set; }
        //    public Int32 Id { get; set; }
        //}
        public ActionResult GetAttendanceRemark()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Lookup.Where(e => e.Code == "700").Select(e => new { LookupValues = e.LookupValues }).SingleOrDefault();
                var oreturndataclass = new List<returndataclass>();
                if (qurey != null)
                {
                    var listofdata = qurey.LookupValues.Where(e => e.IsActive == true).OrderBy(e => e.LookupVal);
                    foreach (var item in listofdata)
                    {
                        var name = Utility.GetRemarkName().Where(e => e.Key == item.LookupVal.Trim()).Select(e => e.Value).SingleOrDefault();
                        oreturndataclass.Add(new returndataclass
                        {
                            data = item.LookupVal,
                            Id = item.Id,
                            name = name + "(" + item.LookupVal + ")"
                        });
                    }
                }
                if (oreturndataclass.Count > 0)
                {
                    return Json(new { success = true, data = oreturndataclass }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        //sk

        public ActionResult GetGratuityAct()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var data = db.GratuityAct.ToList();
                foreach (var s in data)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        value = s.GratuityActName.ToString() + ": " + s.EffectiveDate.Value.ToShortDateString(),
                        code = s.Id.ToString()
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLWFeffdate()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var data = db.LWFMaster.ToList();
                foreach (var s in data)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        value = s.EffectiveDate.Value.ToShortDateString(),
                        code = s.Id.ToString()
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetAgreementEffDate()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var data = db.PayScaleAgreement.ToList();
                foreach (var s in data)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        value = s.EffDate.Value.ToShortDateString(),
                        code = s.Id.ToString()
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult GetReportname(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
        //        switch (data)
        //        {
        //            case "101":
        //                returndata.Add(new Utility.returndataclass
        //                {
        //                    value ="Age Report",
        //                    code = "Age Report"
        //                });
        //                return Json(returndata, JsonRequestBehavior.AllowGet);
        //           break;

        //            case "123":
        //           var s = "GendarReport";
        //           SelectList gen = new SelectList(s);
        //           return Json(gen, JsonRequestBehavior.AllowGet);
        //           break;

        //            default:
        //           return null;
        //            break;


        //        }
        //        return null;
        //    }
        // }

        public ActionResult GetSuseffdate()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var data = db.SuspensionSalPolicy.ToList();
                foreach (var s in data)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        value = s.EffectiveDate.Value.ToShortDateString(),
                        code = s.Id.ToString()
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetNegSaleffdate()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var data = db.NegSalAct.ToList();
                foreach (var s in data)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        value = s.EffectiveDate.Value.ToShortDateString(),
                        code = s.Id.ToString()
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetCalendar()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();

                var data = db.Calendar.Include(e => e.Name).Where(e => e.Default == true).ToList();

                foreach (var s in data)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        value = s.Name.LookupVal.ToString(),
                        code = s.Id.ToString()
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetReportOption(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var qurey = new Lookup();
                if (data == "RPTLOCATION")
                {
                    qurey = db.Lookup.Include(E => E.LookupValues).Where(e => e.Code == "2000").SingleOrDefault();
                }
                else
                {
                    qurey = db.Lookup.Include(E => E.LookupValues).Where(e => e.Code == "2000").SingleOrDefault();
                }
                //
                if (qurey.Id != 0 || qurey != null)
                {
                    var aa = qurey.LookupValues;
                    var s = new SelectList(aa, "Id", "LookupVal", "");
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }



            }

        }
        public ActionResult Getsorting(string data)
        {
            String path = Server.MapPath("~/App_Data/Menu_Json/Sorting.xml");

            try
            {
                List<string> query = new List<string>();

                var ttt = XElement.Load(path).Elements("Rptname");
                foreach (XElement level1Element in ttt)
                {
                    var chk = level1Element.Attribute("name").Value;
                    if (chk == data)
                    {
                        var lv2 = level1Element.Elements("column");
                        foreach (XElement level2Element in lv2)
                        {
                            query.Add(level2Element.Attribute("name").Value);
                        }
                    }
                }
                if (query.Count != 0)
                {
                    int aa = 1;
                    var result = (from c in query
                                  select new { value = c, LookupVal = c });
                    var s = new SelectList(result, "value", "LookupVal", "");
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        public ActionResult GetSort(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data == "RPTSALEARNDEDSUMMARYDETAILSEDD" || data == "RPTSALEARNDEDSUMMARYEDS" || data == "RPTEARNSALARYSTATEMENTSUM" || data == "RPTDEDSALARYSTATEMENTSUM" || data == "RPTSALREGISTERSRG" )
                {
                    var Lookupvaluesdetails = db.Lookup.Where(e => e.Code == "130").Select(e => e.LookupValues.Where(r => r.IsActive == true)).SingleOrDefault();
                    var a = Lookupvaluesdetails.Select(r => r.LookupVal).Distinct().ToList();
                    SelectList drop = new SelectList(a);
                    return Json(drop != null ? drop : null, JsonRequestBehavior.AllowGet);
                }

                else
                {
                    return null;
                }

            }
          
        }


        public ActionResult GetProcessType(string data)
        {
            String path = Server.MapPath("~/App_Data/Menu_Json/ProcessType.xml");

            try
            {
                List<string> query = new List<string>();

                var ttt = XElement.Load(path).Elements("Rptname");
                foreach (XElement level1Element in ttt)
                {
                    var chk = level1Element.Attribute("name").Value;
                    if (chk.ToUpper() == data)
                    {
                        var lv2 = level1Element.Elements("column");
                        foreach (XElement level2Element in lv2)
                        {
                            query.Add(level2Element.Attribute("name").Value);
                        }
                    }
                }
                if (query.Count != 0)
                {
                    int aa = 1;
                    var result = (from c in query
                                  select new { value = c, LookupVal = c });
                    var s = new SelectList(result, "value", "LookupVal", "");
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        public ActionResult GetReportSplGroupOption(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Lookup> querylist = new List<Lookup>();
                //hobby
                if (data == "101")
                {
                    List<Hobby> query = new List<Hobby>();
                    query = db.Hobby.ToList();
                    if (query.Count != 0)
                    {
                        var result = (from c in query
                                      select new { Id = c.Id, LookupVal = c.HobbyName }).Distinct();
                        var s = new SelectList(result, "Id", "LookupVal", "");
                        return Json(s, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }
                return null;
            }
        }


        public ActionResult Get_Calender()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.Calendar.Select(e => new { srno = e.Id, lookupvalue = e.Name.LookupVal }).ToList();
                return Json(a, JsonRequestBehavior.AllowGet);
            }
        }
        public string GenrateReport(ReportUtility.FormCollectionClass form, string url)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var where = ReportUtility.EarningStatementWhereClause(form);
                return url + where;
            }
        }


        public ActionResult GetBonusCalendar()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var data = db.Calendar.Where(e => e.Name.LookupVal.ToUpper().ToString() == "BONUSYEAR").ToList();

                foreach (var s in data)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = s.Id.ToString(),
                        value = s.FromDate.Value.ToShortDateString() + " to " + s.ToDate.Value.ToShortDateString()
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }



        public ActionResult GetEffdateforLWFmaster()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var data = db.LWFMaster.ToList();
                foreach (var s in data)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = s.Id.ToString(),
                        value = s.EffectiveDate.Value.ToShortDateString()
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetEffdateforEsicmaster()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var data = db.ESICMaster.ToList();
                foreach (var s in data)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = s.Id.ToString(),
                        value = s.EffectiveDate.Value.ToShortDateString()
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult GetEffdateforptaxmaster()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var data = db.PTaxMaster.ToList();
                foreach (var s in data)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = s.Id.ToString(),
                        value = s.EffectiveDate.Value.ToShortDateString()
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetEffdateforpfmaster()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var data = db.PFMaster.ToList();
                foreach (var s in data)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = s.Id.ToString(),
                        value = s.EffectiveDate.Value.ToShortDateString()
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetEffdate()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var data = db.PayScaleAgreement.ToList();
                foreach (var s in data)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = s.Id.ToString(),
                        value = s.EffDate.Value.ToShortDateString()
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetITSection24ByDefault()
        {
            List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITSection.Include(e => e.ITInvestments)
                    .Include(e => e.ITSectionList)
                    .Include(e => e.ITSectionListType)
                    .Where(e => e.ITSectionList.LookupVal.ToUpper() == "SECTION24").ToList();

                //var r = (from ca in fall.Select(q=>q.) select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                List<ITInvestment> data2 = new List<ITInvestment>();
                foreach (var item in fall)
                {
                    var ch = item.ITInvestments.ToList();
                    foreach (var item1 in ch)
                    {
                        data2.Add(item1);
                    }
                }
                foreach (var s in data2)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = s.Id.ToString(),
                        value = s.ITInvestmentName.ToString()
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetITSection10ByDefault()
        {
            List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITSection.Include(e => e.ITInvestments)
                    .Include(e => e.ITSectionList)
                    .Include(e => e.ITSectionListType)
                    .Where(e => e.ITSectionList.LookupVal.ToUpper() == "SECTION10").ToList();

                //var r = (from ca in fall.Select(q=>q.) select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                List<ITInvestment> data2 = new List<ITInvestment>();
                foreach (var item in fall)
                {
                    var ch = item.ITInvestments.ToList();
                    foreach (var item1 in ch)
                    {
                        data2.Add(item1);
                    }
                }
                foreach (var s in data2)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = s.Id.ToString(),
                        value = s.ITInvestmentName.ToString()
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult Getinvestmentname()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var data = db.ITInvestment.ToList();
                foreach (var s in data)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = s.Id.ToString(),
                        value = s.ITInvestmentName.ToString()
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PayProcessGroup()
        {
            List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                {
                    int CompId = int.Parse(Session["CompId"].ToString());
                    var query = db.Company.Include(e => e.PayProcessGroup).Where(e => e.Id == CompId).ToList();
                    //    var selected = (Object)null;
                    //   selected = query.PayProcessGroup.Select(e => e.Id).FirstOrDefault();
                    //   selected = query.PayProcessGroup.Where(e => e.Id != null).ToList();
                    foreach (var s in query)
                    {
                        returndata.Add(new Utility.returndataclass
                        {
                            code = s.Id.ToString(),
                            value = s.PayProcessGroup.Select(a => a.Name).SingleOrDefault().ToString()
                        });
                    }
                    //  SelectList s = new SelectList(query.PayProcessGroup, "Id", "FullDetails", selected);
                    return Json(returndata, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult Getsalhead(List<int> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data.Count != 0 && data[0] != 0)
                {

                    foreach (var s in data)
                    {
                        var v1 = db.PayScaleAssignment.Include(a => a.PayScaleAgreement).Include(a => a.SalaryHead).Where(a => a.PayScaleAgreement.Id == s).Select(a => a.SalaryHead).ToList();
                        // int id = Convert.ToInt32(s);
                        //var query = db.PayScaleAgreement.Where(e => e.Id == id).Select(e => e.EffDate).SingleOrDefault();

                        foreach (var ca in v1)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = ca.Id.ToString(),
                                value = ca.Name.ToString()
                            });
                        }
                    }
                }
                else
                {
                    var reference_data = db.Corporate.Include(e => e.Regions).SelectMany(e => e.Regions).ToList();
                    var db_data = db.SalaryHead.ToList();
                    // var expact_list = db_data.Except(reference_data);
                    foreach (var s in db_data)
                    {
                        returndata.Add(new Utility.returndataclass
                        {
                            code = s.Id.ToString(),
                            value = s.Name.ToString()
                        });
                    }
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }




        public ActionResult Getfinanceyearincome()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var data = db.Calendar.Include(a => a.Name).Where(a => a.Name.LookupVal.ToUpper() == "FINANCIALYEAR").ToList();
                foreach (var s in data)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = s.Id.ToString(),
                        value = s.FromDate.Value.ToShortDateString() + "To" + s.ToDate.Value.ToShortDateString()
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }

        //GetEmployee
        public ActionResult GetEmployee(string data, string monthlist, string filter)
        {

            var month = Utility.StringIdsToListString(monthlist);
            List<string> result = new List<string>();
            //monthly-true
            //periodically-false
            var paymonth = "";
            var Date_paymonth = new DateTime();

            var fromdate = "";
            var Date_fromdate = new DateTime();

            var todate = "";
            var Date_todate = new DateTime();

            var ItsMonthlyOrPerodicalicy = true;
            if (month.Count > 1)
            {
                ItsMonthlyOrPerodicalicy = false;
                fromdate = month[0];
                Date_fromdate = Convert.ToDateTime(month[0]);

                todate = month[1];
                Date_todate = Convert.ToDateTime(month[1]);


                DateTime startDate = DateTime.ParseExact(fromdate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(todate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime temp = startDate;
                DateTime dt3 = new DateTime();
                //List<string> result = new List<string>();
                endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
                //while (temp <= endDate)
                //{
                //    // Console.WriteLine((string.Format("{0}/{1}", temp.Month, temp.Year)));
                //    var mnt = (string.Format("{0}/{1}", temp.Month, temp.Year));
                //    temp = temp.AddMonths(1);
                //    result.Add(mnt);
                //}

                for (dt3 = startDate.AddMonths(1); dt3 <= endDate; dt3 = dt3.AddMonths(1))
                {
                    var mnth = dt3.ToString("MM/yyyy");
                    result.Add(mnth);
                }



            }
            else
            {
                paymonth = month[0];
                if (paymonth != "")
                {
                    Date_paymonth = Convert.ToDateTime("01/" + month[0]);

                }
            }
            //






            using (DataBaseContext db = new DataBaseContext())
            {
                var controlname = data;
                List<int> Empids = new List<int>();

                switch (controlname)
                {
                    case "ITMONTHLYPAYMENT":

                        if (ItsMonthlyOrPerodicalicy)
                        {
                            Empids = db.EmployeePayroll.Where(e => e.ITaxTransT.Count() > 0 &&
                                e.ITaxTransT.Where(a => a.PayMonth == paymonth).Count() > 0)
                                .Select(e => e.Employee.Id).ToList();
                        }
                        else
                        {
                            foreach (var d1 in result)
                            {
                                Empids = db.EmployeePayroll.Where(e => e.ITaxTransT.Where(a => a.PayMonth == d1).Count() > 0).Select(a => a.Employee.Id).ToList();
                            }
                        }
                        break;
                    case "ITTRANST":

                        if (ItsMonthlyOrPerodicalicy)
                        {
                            Empids = db.EmployeePayroll.Where(e => e.ITaxTransT.Count() > 0 &&
                                e.ITaxTransT.Where(a => a.PayMonth == paymonth).Count() > 0)
                                .Select(e => e.Employee.Id).ToList();
                        }
                        else
                        {
                            foreach (var d1 in result)
                            {
                                Empids = db.EmployeePayroll.Where(e => e.ITaxTransT.Where(a => a.PayMonth == d1).Count() > 0).Select(a => a.Employee.Id).ToList();
                            }
                        }
                        break;
                    case "PAYSLIP":

                        if (ItsMonthlyOrPerodicalicy)
                        {
                            Empids = db.EmployeePayroll.Where(e => e.SalaryT.Count() > 0 &&
                                e.SalaryT.Where(a => a.PayMonth == paymonth).Count() > 0)
                                .Select(e => e.Employee.Id).ToList();
                        }
                        else
                        {
                            foreach (var d1 in result)
                            {
                                Empids = db.EmployeePayroll.Where(e => e.SalaryT.Where(a => a.PayMonth == d1).Count() > 0).Select(a => a.Employee.Id).ToList();
                            }
                        }
                        break;

                    case "ANNUALSALARY":

                        if (ItsMonthlyOrPerodicalicy)
                        {
                            Empids = db.EmployeePayroll.Where(e => e.SalaryT.Count() > 0 &&
                                e.SalaryT.Where(a => a.PayMonth == paymonth).Count() > 0)
                                .Select(e => e.Employee.Id).ToList();
                        }
                        else
                        {
                            foreach (var d1 in result)
                            {
                                Empids = db.EmployeePayroll.Where(e => e.SalaryT.Where(a => a.PayMonth == d1).Count() > 0).Select(a => a.Employee.Id).ToList();
                            }
                        }
                        break;
                    case "BONUSCHKT":

                        if (ItsMonthlyOrPerodicalicy)
                        {
                            Empids = db.EmployeePayroll.Where(e => e.SalaryT.Count() > 0 &&
                                e.SalaryT.Where(a => a.PayMonth == paymonth).Count() > 0)
                                .Select(e => e.Employee.Id).ToList();
                        }
                        else
                        {
                            foreach (var d1 in result)
                            {
                                Empids = db.EmployeePayroll.Where(e => e.SalaryT.Where(a => a.PayMonth == d1).Count() > 0).Select(a => a.Employee.Id).ToList();
                            }
                        }
                        break;
                    case "GRATUITYT":
                        if (ItsMonthlyOrPerodicalicy)
                        {
                            Empids = db.EmployeePayroll.Where(e => e.GratuityT.Count() > 0 &&
                                e.GratuityT.Where(a => a.ProcessDate.Value.ToShortDateString() == paymonth).Count() > 0)
                                .Select(e => e.Employee.Id).ToList();
                        }
                        else
                        {
                            foreach (var d1 in result)
                            {
                                Empids = db.EmployeePayroll.Where(e => e.GratuityT.Where(a => a.ProcessDate.Value.ToShortDateString() == d1).Count() > 0).Select(a => a.Employee.Id).ToList();
                            }
                        }
                        break;
                    case "ITPROJECTION":
                        if (ItsMonthlyOrPerodicalicy)
                        {
                            Empids = db.EmployeePayroll.Where(e => e.GratuityT.Count() > 0 &&
                                e.GratuityT.Where(a => a.ProcessDate.Value.ToShortDateString() == paymonth).Count() > 0)
                                .Select(e => e.Employee.Id).ToList();
                        }
                        else
                        {
                            foreach (var d1 in result)
                            {
                                Empids = db.EmployeePayroll.Where(e => e.GratuityT.Where(a => a.ProcessDate.Value.ToShortDateString() == d1).Count() > 0).Select(a => a.Employee.Id).ToList();
                            }
                        }
                        break;
                    case "FORM16":

                        var cal = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.FromDate == Date_fromdate && e.ToDate == Date_todate).ToList();
                        foreach (var item in cal)
                        {
                            Empids = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.ITForm16Data.Where(a => a.FinancialYear.Id == item.Id))
                              .Select(e => e.Employee.Id).ToList();
                        }

                        break;
                    case "FORM12BA":
                        var ca2 = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.FromDate == Date_fromdate && e.ToDate == Date_todate).ToList();
                        foreach (var item in ca2)
                        {
                            Empids = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.ITForm16Data.Where(a => a.FinancialYear.Id == item.Id))
                              .Select(e => e.Employee.Id).ToList();
                        }

                        break;

                    case "GRATUITYPREMIUMREPORT":
                        if (ItsMonthlyOrPerodicalicy)
                        {
                            Empids = db.EmployeePayroll.Where(e => e.SalaryT.Count() > 0 &&
                                e.SalaryT.Where(a => a.PayMonth == paymonth).Count() > 0)
                                .Select(e => e.Employee.Id).ToList();
                        }
                        else
                        {
                            foreach (var d1 in result)
                            {
                                Empids = db.EmployeePayroll.Where(e => e.SalaryT.Where(a => a.PayMonth == d1).Count() > 0).Select(a => a.Employee.Id).ToList();
                            }
                        }
                        break;

                    case "OTHEREARNINGTRANSMONTH":
                        if (ItsMonthlyOrPerodicalicy)
                        {
                            Empids = db.EmployeePayroll.Where(e => e.SalaryT.Count() > 0 &&
                                e.SalaryT.Where(a => a.PayMonth == paymonth).Count() > 0)
                                .Select(e => e.Employee.Id).ToList();
                        }
                        else
                        {
                            foreach (var d1 in result)
                            {
                                Empids = db.EmployeePayroll.Where(e => e.SalaryT.Where(a => a.PayMonth == d1).Count() > 0).Select(a => a.Employee.Id).ToList();
                            }
                        }
                        break;

                    case "RPTOTHERDEDUCTIONTRANSMONTH":
                        if (ItsMonthlyOrPerodicalicy)
                        {
                            Empids = db.EmployeePayroll.Where(e => e.SalaryT.Count() > 0 &&
                                e.SalaryT.Where(a => a.PayMonth == paymonth).Count() > 0)
                                .Select(e => e.Employee.Id).ToList();
                        }
                        else
                        {
                            foreach (var d1 in result)
                            {
                                Empids = db.EmployeePayroll.Where(e => e.SalaryT.Where(a => a.PayMonth == d1).Count() > 0).Select(a => a.Employee.Id).ToList();
                            }
                        }
                        break;

                    case "EMPLOYEEREPORTING":


                        if (filter != null)
                        {
                            var oth_ids1 = new List<int>();
                            oth_ids1 = Utility.StringIdsToListIds(filter);
                            foreach (var ca in oth_ids1)
                            {
                                Empids = db.EmployeePayroll.Where(a => a.Employee.PayStruct.JobStatus.Id == ca).Select(e => e.Employee.Id).ToList();
                            }
                            if (Empids.Count() == 0)
                            {
                                Empids = db.EmployeePayroll.Select(a => a.Employee.Id).ToList();
                            }
                        }
                        else
                        {
                            foreach (var d1 in result)
                            {
                                Empids = db.EmployeePayroll.Where(e => e.ITaxTransT.Where(a => a.PayMonth == d1).Count() > 0).Select(a => a.Employee.Id).ToList();

                            }


                        }
                        break;

                    case "BANKSTATEMENT":

                        if (ItsMonthlyOrPerodicalicy)
                        {
                            Empids = db.EmployeePayroll.Include(q => q.SalaryT).Where(e => e.SalaryT.Count() > 0 &&
                                e.SalaryT.Where(a => a.PayMonth == paymonth).Count() > 0)
                                .Select(e => e.Employee.Id).ToList();
                        }
                        else
                        {

                            foreach (var d1 in result)
                            {
                                var Empids1 = db.EmployeePayroll.Where(e => e.SalaryT.Where(a => a.PayMonth == d1).Count() > 0).Select(a => a.Employee.Id).ToList();
                                foreach (var item in Empids1)
                                {
                                    Empids.Add(item);
                                }
                            }
                        }
                        break;

                    case "SALATTENDANCE":

                        if (ItsMonthlyOrPerodicalicy)
                        {
                            Empids = db.EmployeePayroll.Include(q => q.SalAttendance).Where(e => e.SalAttendance.Count() > 0 &&
                                e.SalAttendance.Where(a => a.PayMonth == paymonth).Count() > 0)
                                .Select(e => e.Employee.Id).ToList();
                        }
                        else
                        {

                            foreach (var d1 in result)
                            {
                                var Empids1 = db.EmployeePayroll.Where(e => e.SalAttendance.Where(a => a.PayMonth == d1).Count() > 0).Select(a => a.Employee.Id).ToList();
                                foreach (var item in Empids1)
                                {
                                    Empids.Add(item);
                                }
                            }
                        }
                        break;

                    case "INCRDUELIST":

                        if (ItsMonthlyOrPerodicalicy)
                        {
                            var datacyhk = db.EmployeePayroll.Include(q => q.IncrDataCalc).Include(a => a.Employee).Where(e => e.IncrDataCalc != null).ToList();

                            foreach (var item in datacyhk)
                            {
                                var mDate = item.IncrDataCalc.ProcessIncrDate;
                                var dateperiod = "";
                                var datediff = "";

                                dateperiod = dateperiod + "," + mDate.Value.ToShortDateString() + "";

                                datediff = dateperiod;
                                datediff = datediff.Remove(0, 4);

                                if (datediff == paymonth)
                                {
                                    var Empi = item.Employee.Id;
                                    Empids.Add(Empi);
                                }
                            }
                        }
                        else
                        {
                            foreach (var d1 in result)
                            {
                                var datacyhk = db.EmployeePayroll.Include(q => q.IncrDataCalc).Include(a => a.Employee).Where(e => e.IncrDataCalc != null).ToList();

                                foreach (var item in datacyhk)
                                {
                                    var mDate = item.IncrDataCalc.ProcessIncrDate;
                                    var dateperiod = "";
                                    var datediff = "";

                                    dateperiod = dateperiod + "," + mDate.Value.ToShortDateString() + "";

                                    datediff = dateperiod;
                                    datediff = datediff.Remove(0, 4);

                                    if (datediff == d1)
                                    {
                                        var Empi = item.Employee.Id;
                                        Empids.Add(Empi);
                                    }
                                }
                            }
                        }
                        break;

                    case "ITSUMMARY":

                        var OITSUMData = db.EmployeePayroll.Include(q => q.ITProjection).ToList();
                        //.Select(q=>q.ITProjection.Where(e => e.FinancialYear.FromDate <= Convert.ToDateTime(fromdate) && e.FinancialYear.ToDate.Value >= Convert.ToDateTime(todate))).ToList();
                        foreach (var item in OITSUMData)
                        {
                            foreach (var ca in item.ITProjection)
                            {
                                if (ca.FinancialYear.FromDate <= Convert.ToDateTime(fromdate) && ca.FinancialYear.ToDate.Value >= Convert.ToDateTime(todate))
                                {
                                    //   var chk = item.ITProjection.Where(q => q.Id == ca.Id).Select(q=>q.Id);
                                    var Empids1 = db.EmployeePayroll.Where(e => e.ITProjection.Where(a => a.Id == ca.Id).Count() > 0).Select(a => a.Employee.Id).ToList();
                                    foreach (var d1 in Empids1)
                                    {
                                        Empids.Add(d1);
                                    }
                                }
                            }
                        }
                        break;

                    case "PERKTRANSMONTHLY":

                        if (ItsMonthlyOrPerodicalicy)
                        {
                            Empids = db.EmployeePayroll.Include(q => q.PerkTransT).Where(e => e.PerkTransT.Count() > 0 &&
                                e.PerkTransT.Where(a => a.PayMonth == paymonth).Count() > 0)
                                .Select(e => e.Employee.Id).ToList();
                        }
                        else
                        {

                            foreach (var d1 in result)
                            {
                                var Empids1 = db.EmployeePayroll.Where(e => e.PerkTransT.Where(a => a.PayMonth == d1).Count() > 0).Select(a => a.Employee.Id).ToList();
                                foreach (var item in Empids1)
                                {
                                    Empids.Add(item);
                                }
                            }
                        }
                        break;



                    default:
                        break;
                }
                if (Empids.Count > 0)
                {

                    return Json(new { success = true, data = Empids }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);

                }

            }
        }
        public ActionResult getPFYear()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var aa = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "PFCALENDAR" && e.Default == true).SingleOrDefault();
                var ab = new
                {
                    ToDate = aa.ToDate.Value.ToShortDateString(),
                    FromDate = aa.FromDate.Value.ToShortDateString()
                };
                return Json(new { data = ab }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getFyYear()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var aa = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).SingleOrDefault();
                var ab = new
                {
                    ToDate = aa.ToDate.Value.ToShortDateString(),
                    FromDate = aa.FromDate.Value.ToShortDateString()
                };
                return Json(new { data = ab }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getApprYear()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var aa = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).SingleOrDefault();
                var ab = new
                {
                    ToDate = aa.ToDate.Value.ToShortDateString(),
                    FromDate = aa.FromDate.Value.ToShortDateString()
                };
                return Json(new { data = ab }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult getExpenseBudgetYear()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var aa = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "EXPENSECALENDAR" && e.Default == true).SingleOrDefault();
                var ab = new
                {
                    ToDate = aa.ToDate.Value.ToShortDateString(),
                    FromDate = aa.FromDate.Value.ToShortDateString()
                };
                return Json(new { data = ab }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getLvYear()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var aa = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                var ab = new
                {
                    ToDate = aa.ToDate.Value.ToShortDateString(),
                    FromDate = aa.FromDate.Value.ToShortDateString()
                };
                return Json(new { data = ab }, JsonRequestBehavior.AllowGet);
            }
        }
        //vr
        public ActionResult GetSalaryHeadEarn(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.SalaryHead.Include(aa => aa.Type).Where(q => q.Type.LookupVal.ToUpper() == "EARNING").Select(b => b.Code).Distinct().ToList();
                SelectList drop = new SelectList(a);
                return Json(drop, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetSalaryHeadDed(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.SalaryHead.Include(q => q.Type).Include(aa => aa.Type).Where(q => q.Type.LookupVal.ToUpper() == "DEDUCTION").Select(b => b.Code).Distinct().ToList();
                SelectList drop = new SelectList(a);
                return Json(drop, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLeaveHead(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.LvHead.Select(e => e.LvName).ToList();
                SelectList drop = new SelectList(a);
                return Json(drop, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetPerkTransData(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.SalaryHead.Include(q => q.SalHeadOperationType).Include(aa => aa.Type).Where(q => q.SalHeadOperationType.LookupVal.ToUpper() == "PERK").Select(b => b.Code).Distinct().ToList();
                SelectList drop = new SelectList(a);
                return Json(drop, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetYearlyPayData(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.SalaryHead.Include(q => q.Frequency).Include(aa => aa.Type).Where(q => q.Frequency.LookupVal.ToUpper() == "YEARLY").Select(b => b.Code).Distinct().ToList();
                SelectList drop = new SelectList(a);
                return Json(drop, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetFunAttendanceData(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.SalaryHead.Include(q => q.Frequency).Include(aa => aa.Type).Where(q => q.Frequency.LookupVal.ToUpper() == "DAILY").Select(b => b.Code).Distinct().ToList();
                SelectList drop = new SelectList(a);
                return Json(drop, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetincrdueData(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.JVProcessData.Select(q => q.BatchName).Distinct().ToList();
                SelectList drop = new SelectList(a);
                return Json(drop, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetPaymentbankData(string data, string data2)           //YearlyPayData_id
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.Bank.Select(q => q.Name).Distinct().ToList();
                SelectList drop = new SelectList(a);
                return Json(drop, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetPaymentmodeData(string data, string data2)           //PerkData_id
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.EmpOff.Include(q => q.PayMode).Select(q => q.PayMode.LookupVal).Distinct().ToList();
                SelectList drop = new SelectList(a);
                return Json(drop, JsonRequestBehavior.AllowGet);
            }
        }


    }


}