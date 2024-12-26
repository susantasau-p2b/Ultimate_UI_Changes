using P2BUltimate.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace P2BUltimate.Controllers.Report
{
    [AuthoriseManger]
    public class RptViewController : Controller
    {
        public ActionResult rptworksummaryeis(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptfinalamountsummaryeis(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptmonthlysummaryeis(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult rptoldandnewremarksatt(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult rptodnotfillatt(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptemployeenotfillappraisaleis(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptexpensebudgeteis(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptBeforeActionOnAttendanceLVatt(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptCompanyMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptCompanyOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptBonusActMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        
        public ActionResult RptBonusActOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptCalendarOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptCalendarMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTLTCGlobalblockMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPThoteleligibilitypolicyMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTtravelmodeeligibilitypolicyMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTtraveleligibilitypolicyMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        //public ActionResult RPTINCREMENTSER(string parm)
        //{
        //    Session["FilterNo"] = parm;
        //    return View("~/Views/Report/RptView.cshtml");
        //}

        public ActionResult RPTIRVICTIMEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTIRVICTIMCASEEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTtravelmoderateceilingpolicyMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptCPIRULEMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptCPIRULEOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSalaryHeadFormulasOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTSALHEADFORMULAOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptDepartmentMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptDepartmentOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptDivisionMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptDivisionOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptESICMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptESICOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptGradeMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptGradeOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLEVELMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLEVELOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptGratuityActMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptGratuityActOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptGroupRMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptGroupROBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptHolparmayCalendar(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
       
        public ActionResult RptITSection10BMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptITSection10BOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTOTHEREARNINGTRANSMONTH(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTOTHERDEDUCTIONTRANSMONTH(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTITCHALLANDETAILS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTITCHALLANDETAILSSUMMARY(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptITTDSMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptITTDSOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptJOBMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptJOBOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptJobPositionMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptJobPositionOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptJobStatusMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptJobStatusOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLocationMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLocationOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPackageOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPayBankMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPayBankOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPayProcessGroupMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPayProcessGroupOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPayScaleMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPayScaleOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTReportingStructR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTReportingStructRightsR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTUnitMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTUnitOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTWeeklyOffCalendarOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTWeeklyOffCalendarMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTFunctAttendancetTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTLWFMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTLWFmasterOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLoanAdvancedHeadOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLoanAdvancedHeadMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSalHeadFormulaSlab(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSalHeadFormulaService(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSalSalHeadFormulaBasicScale(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSalHeadFormulaVda(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSalHeadFormulaPercent(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSalHeadFormulaAmount(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSalaryHeadMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSalaryHeadOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptITSECTIONOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptITSECTIONMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
       
        public ActionResult RptITInvestmentMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptITInvestmentOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptJVPARAMETER(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLOANADVANCEHEADMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptNEGSALACTOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptNEGSALACTMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPAYSCALEASSIGNMENTMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPAYSCALEASSIGNMENTOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPFMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPFOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPTAXMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPTAXOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSALREGISTERSRG(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSUSPENSIONSALPOLICYMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSUSPENSIONSALPOLICYOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptEMPSALSTRUCTMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPaySlipPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptBankStatementBAN(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptITTRANSTTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptINSURANCEDETAILSTTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTInvestmentPaymentITR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTITSection10PaymentITR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTITSection24PaymentITR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTLoanAdvRepaymentTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTLoanAdvRequestTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTSalaryArrearTPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTSalaryArrearTSummaryPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTRequisitiontTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTSalAttendanceTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTITProjectionITR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTAnnualSalaryANN(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTBonusChkTPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTBONUSCHKTPROCESSREPORTPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTGratuityTPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTGratuityTdetailsPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTYEARLYPAYMENT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTLeaveEncashREQ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTOTHEARNTRANSMONTH(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTOTHDEDTRANSMONTH(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTPromotionServiceBookSER(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTIncrementServiceBookSER(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTTransferServiceBookSER(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTOtherServiceBookSER(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        /// <summary> reports

        public ActionResult RPTEarnSalaryStatementSUM(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTDedSalaryStatementSUM(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTPFECRRPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");

        }
        public ActionResult RPTINCRDUELISTPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTPFECRCHALLANPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        /// leave reports

        public ActionResult RptLvHeadLVR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLVDETAILSLVR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLvCreditPolicyLVR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLvDebitPolicyLVR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLvEncashPolicy(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLvEncashPolicyLVR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTLvBankPolicyLVR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTLvBankLVR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLvNewReqLVR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLvOpenBalLVR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLvOpenBal3LVR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLVCLOSINGLVR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLvCancelReq(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLeaveEncashReqLVR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLVENCASHMENT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLVLEDGERLVR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLVCancelLVR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptYEARLYPAYMENTPAYTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPerkTransMonthlyTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptBranchSummaryDetailsBRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSalDedSummarySDS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSalDedSummaryDetailsDSD(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSalEarnDedSummaryEDS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSalEarnDedSummaryDetailsEDD(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSalEarnSummarySES(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSalEarnSummaryDetailsESD(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptGrandSummaryDetailsGRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult GRANDSUMMARYDETAILSBYJOBPOSITION(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLoginMasterLOG(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPayrollUserLOG(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptESSUserLOG(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptEmployeemas(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptmenuauthorityrightsmas(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptEmpOffmas(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLookup(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPtaxStatement(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult Rptjvreportdetail(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTJVREPORTDETAILBRANCHWISEPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTJVSUMMARYPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTJVREPORTDETAILINDIVIDUALPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult Rptjvreportsummary(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPtaxStatementslabwisePRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPtaxStatementstatewisePRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptGratuityPremiumReportPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTSalaryArrearRequisition(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        //Attendance
        public ActionResult RptAttendanceATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptAttendancenotpunchATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptattendanceuploadatt(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTMusterCorrectionatt(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptattendanceuploadlwpatt(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTDAILYINATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTOUTDOORDUTYATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTDAILYOUTATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTDailySwipeATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }


        public ActionResult RPTMULTIPLESWIPEATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTOTHERDEPTPUNCHATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTMOBILESWIPEATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTREMARKWISESUMMARYATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTTimingMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTMonthlyMusterATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTEmployeeInformationEIN(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTAGE(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTBANKACCOUNT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTBLOODGROUP(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTFAMILYINFO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTEMAIL(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTEMPSTATUS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTEXEMPLOYEE(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTFAMILTINFO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTFOREIGNTRIP(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTGENDER(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTGUARANTORNFO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTHANDICAP(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTHOBBIES(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTLANGUAGE(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTMARITIALSTATUS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTMEDICALINFO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTNOMINEESINFO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTPASSPORTINFO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTPFAPPLICABLE(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTPRECOMPANYEXP(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTQUALIFICATION(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTRELIGION(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTSKILL(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTSOCIALINFO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTVISAINFO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTYRSOFSERVICE(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTCorporateMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTCorporateOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTRegionMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTRegionOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult Rptincometax(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptRetirementdayactMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptRetirementdayactOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptHolidayCalendarOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptHolidayCalendarMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        //recurement

        public ActionResult RPTMANPOWERBUDGETREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTCTCDEFINITIONREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTCANDIDATECTCDEFINITIONREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTMANPOWERYEARLYBUDGETREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTMANPOWERANALYSISREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTMANPOWEREMPLOYEEWISECTCREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTMANPOWERANALYSISCTCSUMMARYREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTMANPOWERRECOMENDATIONREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTMANPOWERREQUISITIONREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTMANPOWERPOSTCREATIONREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTCANDIDATEJOININGREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptCANDIDATEFINALIZATIONREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTAPPRAISALCALENDAREIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTRECRUITMENTBATCHINITIATORREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTRECRUITMENTINITIATORREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTCANDIDATEEVALUATIONREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTRESUMESHORTLISTINGREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTMANPOWERPROVISIONREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTMANPOWERPOSTDATAREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTCANDIDATESHORTLISTREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTINTERVIEWDETAILSREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }


        public ActionResult RPTCANDIDATESCHEDULINGREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptCandidateMasterREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptCANDIDATEMEDICALREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptCANDIDATESOCIALINFOREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptCANDIDATEQUALIFICATIONREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptCANDIDATEMPLOYEMENTHISTORYREPORTREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptRecruitementSourceAllocation(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptCandidateHobbies(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptCandidateLanguagesREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTRECRUITMENTCALENDARREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTJOBSOURCEREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTRECRUITMENTVENUEREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        //traning
        public ActionResult RPTYEARLYTRAININGBUDEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptTrainingCalendarEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptEMPLOYEEWISETRAININGEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptSESSIONWISEEMPLOYEETRAININGEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSESSIONWISETrainingFeedbackEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTSESSIONWISEtrainingevaluationEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPROGRAMWISEATTENDANCEEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSESSIONWISEPROGRAMATTENDANCEEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTNOTASSIGNTRAININGEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptTrainingFeedbackEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTBudgetParaEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptcategoryEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTAPPRAISALCATEGORYEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTAPPRAISALCATEGORYRATINGEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTAPPRAISALEVALUATIONEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTAPPRAISALSUBCATEGORYRATINGEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTAPPRAISALRATINGCONCLUSIONEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
		
		public ActionResult RPTAPPRAISALRATINGCONCLUSIONSUMMARYEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTAPPRAISALRATINGANALYSISEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTAPPRAISALRATINGANALYSISLOCATIONEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTAPPRAISALRATINGANALYSISCONFIDENTIALEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTAPPRAISALEVALUATIONMETHODEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTAPPRAISALSCHEDULEEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTAPPRAISALPUBLISHEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTAPPRAISALASSIGNMENTEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTFacultyEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTVenueEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptTrainingEmployeeCalendar(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptTrainingEmployeeEvaluation(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptTrainingAttendance(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptProgramWiseTrainingCalendarEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptFacultyWiseTrainingCalendarEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTAPPRKRAEVALUATION(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        //apprisal

        public ActionResult RptAPPRCATEGORYMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }


        public ActionResult RPTITSUMMARYANN(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTNEGATIVESALACT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTFORM16ANN(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTLVCREDITLVR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTFORM12BAANN(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTLEAVEINTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTSALARYRECONCELLATIONSAL(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptTRAININGMasterEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptTRAININGBUDGET(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }


        public ActionResult RPTApprDeptWise(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTApprGradeWise(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTEMPGLOBALEGL(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTSLABWISEDAPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult rptAPPRAISALCATEGORYMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult rptAPPRAISALDETAIL(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult rptGRADEWISEAPPRAISALMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptAPPRAISALSUBCATEGORYMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptAPPASSIGNMENT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult rptsalheadformula(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptITMonthlyPayment(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptEmployeeReporting(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptUserList(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLWFstatementPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptESICSTATEMENTPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptSalaryReconciliationSAL(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSalaryReconciliationEarnSAL(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSalaryReconciliationDedSAL(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSalaryReconciliationSummarySAL(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptTRAININGEVALUATIONEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTYearlyProgramListEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptTRAININGScheduleEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptEMPEISMIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptLOANADVREPAYMENTPROCESSPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptINSURANCEPROCESSPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptFUNCTIONALATTENDANCEPROCESSPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptEARNINGSTATEMENTPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptDEDUCTIONSTATEMENTPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptOTHERSERVICEBOOKACTIVITYMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPROMOTIONACTIVITYMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptTRANSFERACTIVITYMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptINCREMENTACTIVITYMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptNEGATIVESALARYPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptNEGATIVESALARYNEWPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSUSPENDEDEMPLOYEEPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptINCREMENTSERVICEBOOKDETAILSSER(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptPROMOTIONSERVICEBOOKDETAILSSER(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptHIERARCHYEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSERVICESECURITYMASTERMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptEMPLOYEESUMMARYMASTERMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTJvEmpAccountOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTARREARPFTPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTSalaryArrearHeadwisePRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSERVICESECURITYFDCLOSERMASTERMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTJVGRANDSUMMARYJGS(string parm)  
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTITChallanMonthlyITR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTJVParameterOBJ(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTHRAExemptionMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTHRAExemptionTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        ///attendance master
        public ActionResult RPTTIMINGGROUPATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTTIMINGWEEKLYSCHEDULEATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTREMARKCONFIGATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTROASTERGROUPATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTTIMINGPOLICYBATCHATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTTIMINGPOLICYBATCHASSIGNMENTATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTMACHINEINTERFACEATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTREPORTINGTIMINGSTRUCTUREATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTTIMEPOLICYBASEDMONTHLYROASTERATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTINTIMEBASEDMONTHLYROASTERATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTOVERTIMEPOLICYATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTTIMINGPOLICYATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTATTENDANCEREPORTBUILDERATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTPUNCHTIMEWISEATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptBranchwiseJVPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptEMPLOYEESENIORITYMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTLOANADVREPAYMENTPRINCIPLEANDINTERESTTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTLEAVECREDITTRAILBALANCELVR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTPENDINGLEAVECREDITLVR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTLEAVEENCASHPAYMENTLVR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTLEAVEENCASHPROJECTIONLVR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptTARGETENTRYFORMEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptTARGETENTRYDETAILSFORMEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptTARGETACHEIVEDEMPWISEEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptTARGETACHEIVEDEMPGRANDSUMMARYEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTLEAVENOTCREDITLVR(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptEmployeeStaffListmas(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptEarningSummarySUM(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTExceptionalATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTExceptionalRemarkATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTHOLDINCREMENTRELEASESER(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTLTCAdvanceClaimTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTLTCSETTLEMENTCLAIMTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTTADAAdvanceClaimTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTTADASETTLEMENTCLAIMTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTHOTELBOOKINGREQUESTTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTTICKETBOOKINGREQUESTTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTVEHICLEBOOKINGREQUESTTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTSLABWISEDACALPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTOTHEREARNINGTTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        //Manish
        public ActionResult RPTLVDEPENDONSALARYHEADDAYSTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTOTHERDEDUCTIONTTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTGEOFENCINGMASTERMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptExtnRednservicebookdetailsser(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptEXTNREDNACTIVITYMasterMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTLookupMAS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTResignationrequestLOG(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTNoticeperiodprocessLOG(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTFFSLeaveEncashmentLOG(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTMonthlyMusterTimewiseATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RPTMonthlyMusterRemarkwiseATT(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptSalRegisterSummarysrg(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        //PFTrust Report
        public ActionResult rptpassbookobj(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult rptpassbookmonthwiseobj(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptpassbookEmployeewiseobj(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptloanproposalobj(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptsettelmentproposalobj(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptIntereststatementobj(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptInterestproductobj(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptsettelmentregisterobj(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptloanregisterstatementobj(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptemployeepfloanhistoryobj(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptEmpbranchwisePRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTCMSEIS(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTCMSOBJREC(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptCompetencyModelRec(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptCompetencyModelAssignmentRec(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptCompetencyEmployeeDataTRec(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptSuccessionModelRec(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptSuccessionModelAssignmentRec(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptSuccessionEmployeeDataTRec(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult rptpftledgerstatementobj(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
        public ActionResult RptCPIENTRYTTRA(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptBASICDEPENDRULEOBJSUBobj(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptEMPQUALIFICATIONmis(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTOFFICIATINGPAYMENTPRO(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RPTOFFICIATINGPAYMENTDETAILSSER(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptRegimeSchemetra(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptTransferLetterser(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptPromotionLetterser(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }

        public ActionResult RptIncrementLetterser(string parm)
        {
            Session["FilterNo"] = parm;
            return View("~/Views/Report/RptView.cshtml");
        }
    }
}