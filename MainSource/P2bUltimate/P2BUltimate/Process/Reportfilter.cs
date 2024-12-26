using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Process;
using P2BUltimate.Security;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Data.Entity;
using System.Web.UI;

namespace P2BUltimate.Process
{
    public class ReportfilterData
    {
        public string ModuleName { get; set; }
        public string ReportName { get; set; }
        public string TypewiseName { get; set; }
        public string monthlyforrptname { get; set; }
        public List<string> forithead { get; set; }
        public List<string> HeadList { get; set; }
        public dynamic p2bdatad { get; set; }
    }
    public class Reportfilter
    {
        //  private static dynamic P2BData;
        public static dynamic Getreportdata(string val , string Paymnth)
        {
            dynamic Gen100 = null;
            dynamic P2BData = null;
            var NewObj = new NameValueCollection();
            var employeeids = (String)null;
            var Candidateids = (String)null;
            var Victimids = (String)null;
            var paymonth = (String)null;
            var monthly = (String)null;
            var monthlyforrptname = (String)null;
            var mFromDate = (String)null;
            var mToDate = (String)null;
            var pFromDate = (String)null;
            var pToDate = (String)null;
            var FromTime = (String)null;
            var ToTime = (String)null;
            var ReqDate = (String)null;

            if (val != null)
            {
                NewObj = HttpUtility.ParseQueryString(val);
                if (NewObj != null)
                {
                    employeeids = NewObj["Employee-Table"];
                    Candidateids = NewObj["Candidate_table"];
                    Victimids = NewObj["Victim_table"];

                    switch (NewObj["typeoffilter"].ToUpper())
                    {
                        case "MONTHLY":
                            if (NewObj["monthly"] != "0" || NewObj["monthly"] != null)
                            {
                                monthly = NewObj["monthly"];
                                monthlyforrptname = "MONTH " + monthly;
                            }

                            if (NewObj["fromtime1"] != "" && NewObj["totime1"] != "")
                            {

                                //var fromdate = DateTime.ParseExact(NewObj["fromdate"], "dd/MM/yyyy", null);
                                var fromtime = Convert.ToDateTime(NewObj["fromtime1"]);
                                var totime = Convert.ToDateTime(NewObj["totime1"]);

                                var ftime = NewObj["fromtime1"];
                                var ttime = NewObj["totime1"];

                                FromTime = ftime;
                                ToTime = ttime;

                                //var todate = DateTime.ParseExact(NewObj["todate"], "dd/MM/yyyy", null);
                                monthlyforrptname = "PERIOD " + FromTime + " TO " + ToTime;
                            }
                            break;
                        case "DATE":
                            if (NewObj["date"] != "0" || NewObj["date"] != null)
                            {
                                ReqDate = NewObj["date"];
                            }
                            break;
                        case "PERIODICALLY":
                            if (NewObj["fromdate1"] != null && NewObj["todate1"] != null)
                            {

                                //var fromdate = DateTime.ParseExact(NewObj["fromdate"], "dd/MM/yyyy", null);
                                var fromdate = Convert.ToDateTime(NewObj["fromdate1"]);
                                var todate = Convert.ToDateTime(NewObj["todate1"]);

                                var fdate = NewObj["fromdate1"];
                                var tdate = NewObj["todate1"];

                                pFromDate = fdate;
                                pToDate = tdate;

                                //var todate = DateTime.ParseExact(NewObj["todate"], "dd/MM/yyyy", null);
                                var spaymonth = "";
                                for (var mDate = fromdate.Date; mDate <= todate.Date; mDate = mDate.Date.AddMonths(1))
                                {
                                    spaymonth = spaymonth + "," + mDate.ToString("MM/yyyy") + "";
                                }
                                paymonth = spaymonth;
                                paymonth = paymonth.Remove(0, 1);
                                // monthlyforrptname = "PERIOD " + fromdate.Month + "/" + fromdate.Year + " TO " + todate.Month + "/" + todate.Year;
                                monthlyforrptname = "PERIOD " + pFromDate + " TO " + pToDate;
                            }
                            if (NewObj["fromtime1"] != "" && NewObj["totime1"] != "")
                            {

                                //var fromdate = DateTime.ParseExact(NewObj["fromdate"], "dd/MM/yyyy", null);
                                var fromtime = Convert.ToDateTime(NewObj["fromtime1"]);
                                var totime = Convert.ToDateTime(NewObj["totime1"]);

                                var ftime = NewObj["fromtime1"];
                                var ttime = NewObj["totime1"];

                                FromTime = ftime;
                                ToTime = ttime;

                                //var todate = DateTime.ParseExact(NewObj["todate"], "dd/MM/yyyy", null);
                                monthlyforrptname = "PERIOD " + FromTime + " TO " + ToTime;
                            }

                            break;
                        case "CALENDER":
                            if (NewObj["fromdatecal"] != null && NewObj["todatecal"] != null)
                            {
                                var Formdate = NewObj["fromdatecal"];
                                var todate = NewObj["todatecal"];
                                mFromDate = Formdate;
                                mToDate = todate;
                                // professional tax slabwiese report calendar wise report
                                var fromdatee = Convert.ToDateTime(NewObj["fromdatecal"]);
                                var todatee = Convert.ToDateTime(NewObj["todatecal"]);

                                var spaymonth = "";
                                for (var mDate = fromdatee.Date; mDate <= todatee.Date; mDate = mDate.Date.AddMonths(1))
                                {
                                    spaymonth = spaymonth + "," + mDate.ToString("MM/yyyy") + "";
                                }
                                paymonth = spaymonth;
                                paymonth = paymonth.Remove(0, 1);

                                monthlyforrptname = "PERIOD " + mFromDate + " TO " + mToDate;
                            }
                            break;
                        case "WEEKLY":
                        case "SUMMARY":
                        case "FINANCE":
                            if (NewObj["FromDate"] != null && NewObj["ToDate"] != null)
                            {

                                var Formdate = NewObj["FromDate"];
                                var todate = NewObj["ToDate"];
                                mFromDate = Formdate;
                                mToDate = todate;
                            }
                            break;

                        case "DAILY":
                            if (NewObj["todatecal"] != null && NewObj["fromdatecal"] != null)
                            {
                                var Formdate = NewObj["fromdatecal"];
                                var todate = NewObj["todatecal"];
                                mFromDate = Formdate;
                                mToDate = todate;
                            }
                            break;
                    }
                }
            }



            //var HeadList = new List<String>();
            //if (NewObj["Head1"] != null)
            //{
            //    var HeadListtemp = Utility.StringIdsToListString(NewObj["Head1"]);
            //    HeadList.AddRange(HeadListtemp);
            //}

            //if (NewObj["Head2"] != null)
            //{
            //    HeadList.Add(NewObj["Head2"]);
            //}
            List<string> HeadList = new List<string>();
            var FilterHead = HeadList;
            var ReportName = NewObj["ReportName"];
            var FieldCount = NewObj["FieldCount"];
            var Reportype = NewObj["Reportype"];
            var ReportTypeModulewiseName = "";
            var ModuleName = HttpContext.Current.Session["ModuleType"];
            var SalaryHeads = NewObj["SalaryHeads"];

            //added by bhagesh
            var parameter = HttpContext.Current.Session["FilterNo"];
            int param1 = Convert.ToInt32(parameter);
            var MoreThanT = NewObj["MoreThan1"];
            //Code added For Other Service Book
            var oth_id = (String)null;
            oth_id = NewObj["oth_id"];

            var oth_ids = new List<string>();

            if (oth_id != null)
            {
                oth_ids = Utility.StringIdsToListString(oth_id);
            }
            var oth_idlist = new List<string>();
            foreach (var item in oth_ids)
            {
                oth_idlist.Add(Convert.ToString(item));
            }
            var oth_idsgr = (String)null;
            oth_idsgr = NewObj["GroupbyOption"];

            var oth_idss = new List<string>();

            if (oth_idsgr != null)
            {
                oth_idss = Utility.StringIdsToListString(oth_idsgr);
            }
            var forithead = new List<string>();
            foreach (var item in oth_idss)
            {
                forithead.Add(Convert.ToString(item));
            }          
            //Code Added For Earn and Ded Sal Statements
            var salrhead = (String)null;
            salrhead = NewObj["YearlyPayData_id"];

            var salheadlist = new List<string>();
            if (salrhead != null)
            {
                var salrheads = new List<string>();
                salrheads = Utility.StringIdsToListString(salrhead);
                foreach (var item in salrheads)
                {
                    salheadlist.Add(Convert.ToString(item));
                }
            }

            var salrhead1 = (String)null;
            salrhead1 = NewObj["YearlyPayData_idLevel1"];

            var salheadlistLevel1 = new List<string>();
            if (salrhead1 != null)
            {
                var salrheads = new List<string>();
                salrheads = Utility.StringIdsToListString(salrhead1);
                foreach (var item in salrheads)
                {
                    salheadlistLevel1.Add(Convert.ToString(item));
                }
            }

            var salrhead2 = (String)null;
            salrhead2 = NewObj["YearlyPayData_idLevel2"];

            var salheadlistLevel2 = new List<string>();
            if (salrhead2 != null)
            {
                var salrheads = new List<string>();
                salrheads = Utility.StringIdsToListString(salrhead2);
                foreach (var item in salrheads)
                {
                    salheadlistLevel2.Add(Convert.ToString(item));
                }
            }

            var SpecialGroup = (String)null;
            SpecialGroup = NewObj["SpecialGroup_id"];
            var SpecialGroupslist = new List<string>();

            if (SpecialGroup != null)
            {
                var SpecialGroups = new List<string>();
                SpecialGroups = Utility.StringIdsToListString(SpecialGroup);
                foreach (var item in SpecialGroups)
                {
                    SpecialGroupslist.Add(Convert.ToString(item));
                }
            }

            var Sorting = (String)null;
            Sorting = NewObj["Sorting_id"];
            var Sortingslist = new List<string>();

            if (Sorting != null)
            {
                var Sortings = new List<string>();
                Sortings = Utility.StringIdsToListString(Sorting);
                foreach (var item in Sortings)
                {
                    Sortingslist.Add(Convert.ToString(item));
                }
            }

            ////////////////////Report Type

            var ProcessType = (String)null;
            ProcessType = NewObj["ProcessType_id"];

            var ReportType = (String)null;
            ReportType = NewObj["ReportType_id"];
            //var ReportTypeslist = new List<string>();

            //if (ReportType != null)
            //{
            //    var ReportTypes = new List<string>();
            //    ReportTypes = Utility.StringIdsToListString(ReportType);
            //    foreach (var item in ReportTypes)
            //    {
            //        ReportTypeslist.Add(Convert.ToString(item));
            //    }
            //}
            //code for loanm adv repayment
            var loanadvid = (String)null;
            loanadvid = NewObj["loanadv_id"];
            var loanadvids = new List<string>();
            if (loanadvid != null)
            {
                loanadvids = Utility.StringIdsToListString(loanadvid);
            }
            var loanadvidlist = new List<string>();
            foreach (var item in loanadvids)
            {
                loanadvidlist.Add(Convert.ToString(item));
            }
            //code for lvhead in lvcancel
            var lvhdid = (String)null;
            lvhdid = NewObj["lv_id"];
            var lvhdids = new List<string>();
            if (lvhdid != null)
            {
                lvhdids = Utility.StringIdsToListString(lvhdid);
            }
            var lvhdlist = new List<string>();
            foreach (var item in lvhdids)
            {
                lvhdlist.Add(Convert.ToString(item));
            }

            var lvname = (String)null;
            lvname = NewObj["GroupbyOption"];
            var lvnames = new List<string>();
            if (lvname != null)
            {
                lvnames = Utility.StringIdsToListString(lvname);
            }
            var lvnamelist = new List<string>();
            foreach (var item in lvnames)
            {
                lvnamelist.Add(Convert.ToString(item));
            }
            //
            var emp = new List<int>();
            var qurey = "";
            var months = new List<string>();
            if (employeeids != null)
            {
                emp = Utility.StringIdsToListIds(employeeids);
            }
            if (Candidateids != null)
            {
                emp = Utility.StringIdsToListIds(Candidateids);
            }
            if (Victimids != null)
            {
                emp = Utility.StringIdsToListIds(Victimids);
            }
            if (monthly != null)
            {
                months = Utility.StringIdsToListString(monthly);
            }
            if (paymonth != null)
            {
                months = Utility.StringIdsToListString(paymonth);
            }
            var z = new List<int>();
            foreach (var item in emp)
            {
                z.Add(Convert.ToInt32(item));
            }
            var monthllist = new List<string>();
            foreach (var item in months)
            {
                monthllist.Add(Convert.ToString(item));
            }
            var emplvidslist = new List<int>();

            var LvFromdate = DateTime.Now;
            var LvTodate = DateTime.Now;


            //var P2BData = new List<dynamic>();
            if (ModuleName != null)
            {
                if (ModuleName.ToString().Trim().ToUpper() == "EPMS")
                {
                    ModuleName = "Payroll";
                }
                if (ModuleName.ToString().Trim().ToUpper() == "ELMS")
                {
                    ModuleName = "Leave";
                }
                if (ModuleName.ToString().Trim().ToUpper() == "CORE")
                {
                    ModuleName = "Core";
                }
                if (ModuleName.ToString().Trim().ToUpper() == "EEIS")
                {
                    ModuleName = "EEIS";
                }
                if (ModuleName.ToString().Trim().ToUpper() == "ETRM")
                {
                    ModuleName = "Attendance";
                }
                if (ModuleName.ToString().Trim().ToUpper() == "EPFT")
                {
                    ModuleName = "EPFT";
                }                   
            }

            //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
            //              new System.TimeSpan(0, 60, 0)))
            //{
            if (ReportName != null)
            {
                ReportName = ReportName.Remove(0, 3);
                //string founderMinus1 = founder.Remove(founder.Length - 1, 1);  
                var ReportTypeModulewiseNametemp = (string)ReportName;
                ReportTypeModulewiseName = ReportTypeModulewiseNametemp.Substring(ReportTypeModulewiseNametemp.Length - 3);

                ReportName = ReportName.Remove(ReportName.Length - 3, 3);
            }
            else
            {
                return null;
            }

            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                if (ModuleName == "Payroll")
                {
                    if (ReportName.ToUpper() == "EMPLOYEEINFORMATION")
                    {
                        if (ReportTypeModulewiseName.ToUpper() == "EIN")
                        {

                            Gen100 = ReportRDLCObjectClass.GenerateEmployeeInformation(Convert.ToInt32(SessionManager.CompanyId), z, HeadList, NewObj["typeoffilter"]);
                            P2BData = Gen100;
                        }
                    }

                    else if (ReportTypeModulewiseName.ToUpper() == "LOG")
                    {
                        //if (ReportTypeModulewiseName.ToUpper() == "LOG")
                        //{
                            //var month = monthllist[0];
                            Gen100 = ReportRDLCObjectClass.GenerateLoginMaster(ReportName.ToUpper(),Convert.ToInt32(SessionManager.CompanyId), z, Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), forithead);
                            P2BData = Gen100;
                        //}

                    }
                    else if (ReportName.ToUpper() == "EMPGLOBAL")
                    {
                        if (ReportTypeModulewiseName.ToUpper() == "EGL")
                        {
                            var month = monthllist[0];
                            Gen100 = ReportRDLCObjectClass.GenerateGlobalEmployee(Convert.ToInt32(SessionManager.CompPayrollId), z, month);
                            P2BData = Gen100;
                        }

                    }
                   else if (ReportName.ToUpper() == "BANKSTATEMENT")
                    {
                        if (ReportTypeModulewiseName.ToUpper() == "BAN")
                        {
                            var month = monthllist[0];
                            Gen100 = ReportRDLCObjectClass.GenerateBankStatement(Convert.ToInt32(SessionManager.CompPayrollId), z, month, forithead, SpecialGroupslist, salheadlist);
                            P2BData = Gen100;
                        }
                    }
                    else if (ReportName.ToUpper() == "SALREGISTER")
                    {
                        if (ReportTypeModulewiseName.ToUpper() == "SRG")
                        {
                            Gen100 = ReportRDLCObjectClass.GenerateSalRegister(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, forithead, SpecialGroupslist, salheadlist, Sortingslist);
                            P2BData = Gen100;
                        }

                    }

                    else if (ReportName.ToUpper() == "SALREGISTERSUMMARY")
                    {
                        if (ReportTypeModulewiseName.ToUpper() == "SRG")
                        {
                            Gen100 = ReportRDLCObjectClass.GenerateSalRegisterSummary(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, forithead, SpecialGroupslist, salheadlist);
                            P2BData = Gen100;
                        }

                    }


                    //else if (ReportName.ToUpper() == "ANNUALSALARY" || ReportName.ToUpper() == "BONUSCHKT" || ReportName.ToUpper() == "ITPROJECTION" || ReportName.ToUpper() == "ITSUMMARY" || ReportName.ToUpper() == "FORM16" || ReportName.ToUpper() == "FORM12BA")
                    //{
                    //    Gen100 = ReportRDLCObjectClass.GenerateAnnualStatementReport(Convert.ToInt32(SessionManager.CompPayrollId), z, ReportName.ToUpper(), Convert.ToDateTime(mFromDate), Convert.ToDateTime(mToDate), forithead);
                    //    P2BData = Gen100;

                    //}
                    else if (ReportName.ToUpper() == "BRANCHSUMMARYDETAILS" || ReportName.ToUpper() == "GRANDSUMMARYDETAILS")
                    {
                        if (ReportTypeModulewiseName.ToUpper() == "BRA" || ReportTypeModulewiseName.ToUpper() == "GRA")
                        {

                            int GroupByOptions = Convert.ToInt32(NewObj["GroupbyOption"]);

                            var where = "";
                            if (monthly != null)
                            {
                                //where += "PayMonth IN ('" + monthly + "')";
                                where += "PayMonth ='" + monthly + "'";  //changed by vinayak 
                            }
                            string Grpby = db.LookupValue.Where(b => b.Id == GroupByOptions).Select(b => b.LookupVal.ToUpper().ToString()).SingleOrDefault();
                            if (Grpby == "JOBPOSITION")
                            {
                                if (ReportName.ToUpper() == "BRANCHSUMMARYDETAILS")
                                {
                                    ReportName = "BRANCHSUMMARYDETAILSBYJOBPOSITION";
                                }
                                else
                                {
                                    ReportName = "GRANDSUMMARYDETAILSBYJOBPOSITION";
                                }
                            }

                            //if (ReportName == "BRANCHSUMMARYDETAILSBYJOBPOSITION")
                            //{
                            //    qurey = "select * from QJobPosBranchSummary Where " + where;
                            //}
                            //else if (ReportName == "GRANDSUMMARYDETAILSBYJOBPOSITION")
                            //{
                            //    qurey = "select * from QJobPosGrandSummary Where " + where;
                            //}
                            //else
                            //{
                            //    qurey = "select * from QLocEarnDedSummary Where " + where;
                            //}

                            if (Gen100 == null)
                                Gen100 = SummaryReportGen.GenerateSummaryReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, forithead, SpecialGroupslist, Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), Sortingslist);
                            P2BData = Gen100;

                        }
                    }


                    else if (ReportName.ToUpper() == "SALDEDSUMMARY")
                    {
                        if (ReportTypeModulewiseName.ToUpper() == "SDS")
                        {
                            var where = "";
                            string DeductionSalheadname;
                            if (monthly != null)
                            {
                                //where += " And PayMonth IN ('" + monthly + "')";
                                where += "And PayMonth ='" + monthly + "'";  //changed by vinayak 
                            }
                            if (salheadlist.Count() > 0)
                            {
                                DeductionSalheadname = string.Join("', '", salheadlist.Select(t => t));
                                where += "And EarnHead IN ('" + DeductionSalheadname + "')";
                            }

                            if (SalaryHeads != null)
                            {
                                where += " And Id IN ('" + SalaryHeads + "')";
                            }
                            if (ReportName.ToUpper() == "SALDEDSUMMARY")
                            {
                                loanadvidlist = salheadlistLevel1;
                            }
                            //if (forithead.Count() > 0)
                            //{
                            //    var filter = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();

                            //    foreach (var item in filter)
                            //    {
                            //        if (item.LookupVal.ToUpper() == "DEPARTMENT")
                            //        {
                            //            qurey = "select LocCode,LocDesc,DeptCode,DeptDesc,EarnHead,EarnAmount,PayMonth from SalaryDetails where lookupval='Deduction'" + where + "ORDER BY SEQNO";
                            //        }
                            //        else if (item.LookupVal.ToUpper() == "LOCATION")
                            //        {
                            //            qurey = "select LocCode,LocDesc,EarnHead,EarnAmount,PayMonth from SalaryDetails where lookupval='Deduction'" + where + "ORDER BY SEQNO";

                            //        }
                            //        else if (item.LookupVal.ToUpper() == "GRADE")
                            //        {
                            //            qurey = "select Grade_Code,Grade_Name,EarnHead,EarnAmount,PayMonth from SalaryDetails where lookupval='Deduction'" + where + "ORDER BY SEQNO";
                            //        }
                            //        else if (item.LookupVal.ToUpper() == "JOBPOSITION")
                            //        {
                            //            qurey = "select JobPositionCode,JobPositionDesc,EarnHead,EarnAmount,PayMonth from SalaryDetails where lookupval='Deduction'" + where + "ORDER BY SEQNO";
                            //        }

                            //    }
                            //}
                            //else
                            //{
                            //    qurey = "select LocCode,LocDesc,EarnHead,EarnAmount,PayMonth from SalaryDetails where lookupval='Deduction'" + where + "ORDER BY SEQNO";
                            //}

                            if (Gen100 == null)
                                Gen100 = SummaryReportGen.GenerateSummaryReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, forithead, SpecialGroupslist, Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), Sortingslist);
                            P2BData = Gen100;
                            // qurey = "select * from SalaryDetails where lookupval='Deduction'" + " " + where + "  ORDER BY SEQNO";
                        }
                    }
                    else if (ReportName.ToUpper() == "SALDEDSUMMARYDETAILS")
                    {
                        if (ReportTypeModulewiseName.ToUpper() == "DSD")
                        {
                            var where = "";

                            if (employeeids != null)
                            {
                                int employeetotalno = db.Employee.Where(q => q.EMPRESIGNSTAT == false).Count();
                                if (employeetotalno != emp.Count())
                                {
                                    where += "And Employee_Id IN (" + employeeids + ")";
                                }
                            }

                            if (monthly != null)
                            {
                                //where += " And PayMonth IN ('" + monthly + "')";
                                where += " And PayMonth ='" + monthly + "'";  //changed by vinayak 
                            }

                            if (SalaryHeads != null)
                            {
                                where += " And Id IN ('" + SalaryHeads + "')";
                            }

                            //if (forithead.Count() > 0)
                            //{
                            //    var filter = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();

                            //    foreach (var item in filter)
                            //    {
                            //        if (item.LookupVal.ToUpper() == "DEPARTMENT")
                            //        {
                            //            qurey = "select LocCode,LocDesc,DeptCode,EmpCode,DeptDesc,EarnHead,ISNULL(EarnAmount,0) as EarnAmount,FullNameFML,PayMonth,job_name from SalaryDetails where lookupval='Deduction'" + where + "ORDER BY SEQNO";
                            //        }
                            //        else if (item.LookupVal.ToUpper() == "LOCATION")
                            //        {
                            //            qurey = "select LocCode,EmpCode,LocDesc,EarnHead,ISNULL(EarnAmount,0) as EarnAmount,FullNameFML,PayMonth,job_name from SalaryDetails where lookupval='Deduction'" + where + "ORDER BY SEQNO";

                            //        }
                            //        else if (item.LookupVal.ToUpper() == "GRADE")
                            //        {
                            //            qurey = "select Grade_Code,Grade_Name,EmpCode,EarnHead,ISNULL(EarnAmount,0) as EarnAmount,FullNameFML,PayMonth,job_name from SalaryDetails where lookupval='Deduction'" + where + "ORDER BY SEQNO";
                            //        }
                            //        else if (item.LookupVal.ToUpper() == "JOBPOSITION")
                            //        {
                            //            qurey = "select JobPositionCode,JobPositionDesc,EmpCode,EarnHead,ISNULL(EarnAmount,0) as EarnAmount,FullNameFML,PayMonth,job_name from SalaryDetails where lookupval='Deduction'" + where + "ORDER BY SEQNO";
                            //        }

                            //    }
                            //}
                            //else
                            //{
                            //    //qurey = "select LocCode,LocDesc,EarnHead, ISNULL(EarnAmount,0) as EarnAmount,PayMonth,FullNameFML,EmpCode,job_name from SalaryDetails where lookupval='Deduction'" + where + "ORDER BY SEQNO";
                            //    if (Gen100 == null)
                            //        Gen100 = SummaryReportGen.GenerateSummaryReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, forithead, SpecialGroupslist, Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate));
                            //    P2BData = Gen100;
                            //}
                            if (Gen100 == null)
                                Gen100 = SummaryReportGen.GenerateSummaryReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, forithead, SpecialGroupslist, Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), Sortingslist);
                            P2BData = Gen100;
                            //qurey = "select * from SalaryDetails where lookupval='Deduction'" + where + "  ORDER BY SEQNO";
                        }
                    }
                    else if (ReportName.ToUpper() == "SALEARNDEDSUMMARY")
                    {
                        if (ReportTypeModulewiseName.ToUpper() == "EDS")
                        {
                            var where = " Where ";
                            if (monthly != null)
                            {
                                //where += "PayMonth IN ('" + monthly + "')";
                                where += "PayMonth ='" + monthly + "'";  //changed by vinayak 
                            }
                            if (SalaryHeads != null)
                            {
                                where += " And Id IN ('" + SalaryHeads + "')";
                            }
                            //if (forithead.Count() > 0)
                            //{
                            //    var filter = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();

                            //    foreach (var item in filter)
                            //    {
                            //        if (item.LookupVal.ToUpper() == "DEPARTMENT")
                            //        {
                            //            qurey = "select LocCode,LocDesc, DeptCode,DeptDesc,EarnHead,EarnAmount,PayMonth,LookupVal,TotalEarning,TotalDeduction,TotalNet from SalaryDetails" + where + " ORDER BY Lookupval desc,SEQNO asc";
                            //        }
                            //        else if (item.LookupVal.ToUpper() == "LOCATION")
                            //        {
                            //            qurey = "select LocCode,LocDesc,EarnHead,EarnAmount,PayMonth,LookupVal,TotalEarning,TotalDeduction,TotalNet from SalaryDetails" + where + " ORDER BY Lookupval desc,SEQNO asc";

                            //        }
                            //        else if (item.LookupVal.ToUpper() == "GRADE")
                            //        {
                            //            qurey = "select Grade_Code,Grade_Name,EarnAmount,EarnHead,PayMonth,LookupVal,TotalEarning,TotalDeduction,TotalNet from SalaryDetails" + where + " ORDER BY Lookupval desc,SEQNO asc";
                            //        }
                            //        else if (item.LookupVal.ToUpper() == "JOBPOSITION")
                            //        {
                            //            qurey = "select JobPositionCode,JobPositionDesc,EarnAmount,PayMonth,EarnHead,LookupVal,TotalEarning,TotalDeduction,TotalNet from SalaryDetails" + where + " ORDER BY Lookupval desc,SEQNO asc";
                            //        }

                            //    }
                            //}
                            //else
                            //{
                            //    qurey = "select LocCode,LocDesc,EarnHead, EarnAmount,PayMonth,LookupVal,TotalEarning,TotalDeduction,TotalNet from SalaryDetails" + where + " ORDER BY Lookupval desc,SEQNO asc";
                                                               
                            //}

                            if (Gen100 == null)
                                Gen100 = SummaryReportGen.GenerateSummaryReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, forithead, SpecialGroupslist, Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), Sortingslist);
                            P2BData = Gen100;
                            //  qurey = "select * from SalaryDetails" + where + " ORDER BY Lookupval desc,SEQNO asc";
                        }
                    }
                    else if (ReportName.ToUpper() == "SALEARNDEDSUMMARYDETAILS")
                    {
                        if (ReportTypeModulewiseName.ToUpper() == "EDD")
                        {
                            var where = " Where ";
                            if (monthly != null)
                            {
                                //where += " And PayMonth IN ('" + monthly + "')";
                                where += " PayMonth ='" + monthly + "'";  //changed by vinayak 

                            }

                            if (employeeids != null)
                            {
                                int employeetotalno = db.Employee.Where(q => q.EMPRESIGNSTAT == false).Count();
                                if (employeetotalno != emp.Count())
                                {
                                    where += "And Employee_Id IN (" + employeeids + ") ";
                                }
                            }

                            if (SalaryHeads != null)
                            {
                                where += " And Id IN ('" + SalaryHeads + "')";
                            }

                            //if (forithead.Count() > 0)
                            //{
                            //    var filter = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();

                            //    foreach (var item in filter)
                            //    {
                            //        if (item.LookupVal.ToUpper() == "DEPARTMENT")
                            //        {
                            //            qurey = "select LocCode,LocDesc,DeptCode,EmpCode,DeptDesc,EarnHead,EarnAmount,FullNameFML,PayMonth,LookupVal,TotalEarning,TotalDeduction,EPS_Share,ER_Share,TotalNet,job_name from SalaryDetails " + where + "ORDER BY SEQNO";
                            //        }
                            //        else if (item.LookupVal.ToUpper() == "LOCATION")
                            //        {
                            //            qurey = "select LocCode,EmpCode,LocDesc,EarnHead,EarnAmount,FullNameFML,PayMonth,LookupVal,TotalEarning,TotalDeduction,EPS_Share,ER_Share,TotalNet,job_name from SalaryDetails " + where + "ORDER BY SEQNO";

                            //        }
                            //        else if (item.LookupVal.ToUpper() == "GRADE")
                            //        {
                            //            qurey = "select Grade_Code,Grade_Name,EmpCode,EarnHead,EarnAmount,FullNameFML,PayMonth,LookupVal,TotalEarning,TotalDeduction,EPS_Share,ER_Share,TotalNet,job_name from SalaryDetails" + where + "ORDER BY SEQNO";
                                       
                            //        }
                            //        else if (item.LookupVal.ToUpper() == "JOBPOSITION")
                            //        {
                            //            qurey = "select JobPositionCode,JobPositionDesc,EmpCode,EarnHead,EarnAmount,FullNameFML,PayMonth,LookupVal,TotalEarning,TotalDeduction,EPS_Share,ER_Share,TotalNet,job_name from SalaryDetails " + where + "ORDER BY SEQNO";
                            //        }

                            //    }
                            //}
                            //else
                            //{
                            //    qurey = "select LocCode,EmpCode,LocDesc,EarnHead,EarnAmount,FullNameFML,PayMonth,LookupVal,TotalEarning,TotalDeduction,EPS_Share,ER_Share,TotalNet,job_name from SalaryDetails " + where + "ORDER BY SEQNO";
                                
                            //}

                            if (Gen100 == null)
                                Gen100 = SummaryReportGen.GenerateSummaryReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, forithead, SpecialGroupslist, Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), Sortingslist);
                            P2BData = Gen100;

                            //added for oder by
                            //var forbhav = db.Company.Where(ee => ee.Code.ToUpper() == "BDCB").SingleOrDefault();
                            //if (forbhav != null)
                            //{

                            //    qurey = "select * from SalaryDetails" + where + " ORDER BY LookupVal DESC, (CASE EarnCode  WHEN 'BASIC' THEN 29  WHEN 'CHARGEALW' THEN 28  WHEN 'DA' THEN 27  WHEN 'HRA' THEN 26  WHEN 'OFFALW' THEN 25 WHEN 'SPLALW' THEN 24  WHEN 'CASHIER' THEN 23  WHEN 'TYPIST' THEN 22  WHEN 'DRIVERALW' THEN 21  WHEN 'ACCOUNTANT' THEN 20  WHEN 'OT' THEN 19  WHEN 'TELEALW' THEN 18  WHEN 'WASHINGALW' THEN 17  wHEN 'ECCSBANK'THEN 16  WHEN 'ECCSSOC' THEN 15  WHEN 'EWS' THEN 14  WHEN 'LIC' THEN 13  WHEN 'LWF' THEN 12  WHEN 'PF' THEN 11  WHEN 'PHONE' THEN 10  WHEN 'PTAX' THEN 9  WHEN 'ITAX' THEN 8  WHEN 'HL' THEN 7  WHEN 'HLN1' THEN 6  WHEN 'HLN2' THEN 5  WHEN 'HLN3' THEN 4  WHEN 'VEHLN' THEN 3  WHEN 'CONSLN' THEN 2 WHEN 'EDULN' THEN 1  END) DESC";
                            //}
                            //else
                            //{
                            //    qurey = "select LocCode, LocDesc, EmpCode,FullNameFML, Grade_Code,Job_Name, PayMonth, LookupVal,EarnHead,EarnAmount,TotalEarning,  TotalDeduction, EPS_Share,ER_Share,TotalNet,DeptCode,DeptDesc from SalaryDetails" + where + " ORDER BY SEQNO";

                            //}
                            //  qurey = "select * from SalaryDetails" + where + " ORDER BY ID,SEQNO";
                        }
                    }
                    else if (ReportName.ToUpper() == "SALEARNSUMMARY")
                    {
                        if (ReportTypeModulewiseName.ToUpper() == "SES")
                        {

                            var where = "";
                            string EarningSalheadname;
                            if (monthly != null)
                            {
                                //where += " And PayMonth IN ('" + monthly + "')";
                                where += "And PayMonth ='" + monthly + "'";  //changed by vinayak 
                            }
                            if (salheadlist.Count() > 0)
                            {
                                EarningSalheadname = string.Join("', '", salheadlist.Select(t => t));
                                where += "And EarnHead IN ('" + EarningSalheadname + "')";
                            }
                            if (SalaryHeads != null)
                            {
                                where += " And Id IN ('" + SalaryHeads + "')";
                            }
                            //if (forithead.Count() > 0)
                            //{
                            //    var filter = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();

                            //    foreach (var item in filter)
                            //    {
                            //        if (item.LookupVal.ToUpper() == "DEPARTMENT")
                            //        {
                            //            qurey = "select LocCode,LocDesc,DeptCode,DeptDesc,EarnHead,EarnAmount,PayMonth from SalaryDetails where lookupval='Earning'" + where + "ORDER BY SEQNO";
                            //        }
                            //        else if (item.LookupVal.ToUpper() == "LOCATION")
                            //        {
                            //            qurey = "select LocCode,LocDesc,EarnHead,EarnAmount,PayMonth from SalaryDetails where lookupval='Earning'" + where + "ORDER BY SEQNO";

                            //        }
                            //        else if (item.LookupVal.ToUpper() == "GRADE")
                            //        {
                            //            qurey = "select Grade_Code,Grade_Name,EarnHead,EarnAmount,PayMonth from SalaryDetails where lookupval='Earning'" + where + "ORDER BY SEQNO";
                            //        }
                            //        else if (item.LookupVal.ToUpper() == "JOBPOSITION")
                            //        {
                            //            qurey = "select JobPositionCode,JobPositionDesc,EarnHead,EarnAmount,PayMonth from SalaryDetails where lookupval='Earning'" + where + "ORDER BY SEQNO";
                            //        }
                            //        else if (item.LookupVal.ToUpper() == "JOB")
                            //        {
                            //            qurey = "select Job_Code,Job_Name,EarnHead,EarnAmount,PayMonth from SalaryDetails where lookupval='Earning'" + where + "ORDER BY SEQNO";
                            //        }

                            //    }
                            //}
                            //else
                            //{
                            //    qurey = "select LocCode,LocDesc,EarnHead,EarnAmount,PayMonth from SalaryDetails where lookupval='Earning'" + where + "ORDER BY SEQNO";
                            //}

                            if (ReportName.ToUpper() == "SALEARNSUMMARY")
                            {
                                loanadvidlist = salheadlistLevel1;
                            }

                            if (Gen100 == null)
                                Gen100 = SummaryReportGen.GenerateSummaryReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, forithead, SpecialGroupslist, Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), Sortingslist);
                            P2BData = Gen100;
                        }
                    }
                    else if (ReportName.ToUpper() == "SALEARNSUMMARYDETAILS")
                    {
                        if (ReportTypeModulewiseName.ToUpper() == "ESD")
                        {

                            var where = " ";

                            if (employeeids != null)
                            {
                                int employeetotalno = db.Employee.Where(q => q.EMPRESIGNSTAT == false).Count();
                                if (employeetotalno != emp.Count())
                                {
                                    where += "And Employee_Id IN (" + employeeids + ")";
                                }
                            }


                            if (monthly != null)
                            {
                                //where += " And PayMonth IN ('" + monthly + "')";
                                where += " And PayMonth ='" + monthly + "'";  //changed by vinayak 
                            }

                            if (SalaryHeads != null)
                            {
                                where += " And Id IN ('" + SalaryHeads + "')";
                            }
                            //if (forithead.Count() > 0)
                            //{
                            //    var filter = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();

                            //    foreach (var item in filter)
                            //    {
                            //        if (item.LookupVal.ToUpper() == "DEPARTMENT")
                            //        {
                            //            qurey = "select LocCode,LocDesc,DeptCode,EmpCode,DeptDesc,EarnHead,EarnAmount,FullNameFML,PayMonth,job_name from SalaryDetails where lookupval='Earning'" + where + "ORDER BY SEQNO";
                            //        }
                            //        else if (item.LookupVal.ToUpper() == "LOCATION")
                            //        {
                            //            qurey = "select LocCode,EmpCode,LocDesc,EarnHead,EarnAmount,FullNameFML,PayMonth,job_name from SalaryDetails where lookupval='Earning'" + where + "ORDER BY SEQNO";

                            //        }
                            //        else if (item.LookupVal.ToUpper() == "GRADE")
                            //        {
                            //            qurey = "select Grade_Code,Grade_Name,EmpCode,EarnHead,EarnAmount,FullNameFML,PayMonth,job_name from SalaryDetails where lookupval='Earning'" + where + "ORDER BY SEQNO";
                            //        }
                            //        else if (item.LookupVal.ToUpper() == "JOBPOSITION")
                            //        {
                            //            qurey = "select JobPositionCode,JobPositionDesc,EmpCode,EarnHead,EarnAmount,FullNameFML,PayMonth,job_name from SalaryDetails where lookupval='Earning'" + where + "ORDER BY SEQNO";
                            //        }

                            //    }
                            //}
                            //else
                            //{
                            //    qurey = "select LocCode,LocDesc,EarnHead,EarnAmount,PayMonth,FullNameFML,EmpCode,job_name from SalaryDetails where lookupval='Earning'" + where + "ORDER BY SEQNO";
                            //}
                            if (Gen100 == null)
                                Gen100 = SummaryReportGen.GenerateSummaryReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, forithead, SpecialGroupslist, Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), Sortingslist);
                            P2BData = Gen100;
                            //  qurey = "select * from SalaryDetails where lookupval='Earning'" + where + "  ORDER BY SEQNO";
                        }
                    }
                    else if (ReportName.ToUpper() == "JVGRANDSUMMARY")
                    {
                        if (salheadlist != null && salheadlist.Count() != 0)
                        {
                            foreach (var item in salheadlist)
                            {

                                if (ReportTypeModulewiseName.ToUpper() == "JGS")
                                {
                                    var where = "";
                                    if (monthly != null)
                                    {
                                        where += "ProcessMonth ='" + monthly + "'";  //changed by vinayak 
                                    }

                                    if (SalaryHeads != null)
                                    {
                                        where += " And Id IN ('" + SalaryHeads + "')";
                                    }
                                    if (item != null)
                                    {
                                        where += " And BatchName= '" + item + "'";
                                    }

                                    //qurey = "select * from SalaryDetails where lookupval='Deduction'" + " " + where + "  ORDER BY SEQNO";
                                    qurey = "select * from JVcompairtosalary where " + " " + where + " ";
                                }
                            }
                        }
                    }
                    else if (ReportName.ToUpper() == "SALARYRECONCILIATIONSUMMARY")
                    {

                        if (ReportTypeModulewiseName.ToUpper() == "SAL")
                        {
                            int GroupByOptions = Convert.ToInt32(NewObj["GroupbyOption"]);

                            string Grpby = db.LookupValue.Where(b => b.Id == GroupByOptions).Select(b => b.LookupVal.ToUpper().ToString()).SingleOrDefault();
                            if (Grpby == "DEPARTMENT")
                            {
                                ReportName = "SALARYRECONCILIATIONSUMMARYDEP";
                            }
                            else if (Grpby == "LOCATION")
                            {
                                ReportName = "SALARYRECONCILIATIONSUMMARYLOC";
                            }
                            else
                            {
                                ReportName = "SALARYRECONCILIATIONSUMMARYGRADE";
                            }
                            ReportName = ReportName.ToLower();
                            if (Gen100 == null)
                                Gen100 = PayrollSalaryReconcialiationReportGen.GenerateSalaryReconciliationReReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, forithead, SpecialGroupslist);
                            P2BData = Gen100;
                        }
                    }
                    else if (ReportTypeModulewiseName.ToUpper() == "TRA")
                    {
                        if (Gen100 == null)
                            Gen100 = PayrollTransactionEntryReportGen.GenerateTransactionEntryReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, forithead, SpecialGroupslist, Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate),Convert.ToDateTime(mFromDate), Convert.ToDateTime(mToDate), ProcessType);
                        P2BData = Gen100;
                    }
                    else if (ReportTypeModulewiseName.ToUpper() == "MAS")
                    {
                        if (Gen100 == null)
                            Gen100 = MasterReportGen.GenerateMasterReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, salheadlistLevel1, loanadvidlist, forithead, SpecialGroupslist, Convert.ToDateTime(mFromDate), Convert.ToDateTime(mToDate), Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), ReportType);
                        P2BData = Gen100;
                    }
                    else if (ReportTypeModulewiseName.ToUpper() == "OBJ")
                    {
                        if (Gen100 == null)
                            Gen100 = ObjectReportGen.GenerateObjectReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, forithead, SpecialGroupslist, Convert.ToDateTime(mFromDate), Convert.ToDateTime(mToDate));
                        P2BData = Gen100;
                    }
                    else if (ReportTypeModulewiseName.ToUpper() == "PRO")
                    {
                        if (Gen100 == null)
                            Gen100 = PayrollProcessReportGen.GenerateProcessReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, forithead, SpecialGroupslist, Convert.ToDateTime(mFromDate), Convert.ToDateTime(mToDate), Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), Sorting, salheadlistLevel1);
                        P2BData = Gen100;
                    }
                    else if (ReportTypeModulewiseName.ToUpper() == "SER")
                    {
                        if (Gen100 == null)
                            Gen100 = ServiceBookReportGen.GenerateServiceBookReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, forithead, SpecialGroupslist, Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate));
                        P2BData = Gen100;
                    }
                    else if (ReportTypeModulewiseName.ToUpper() == "ITR")
                    {
                        if (Gen100 == null)
                            Gen100 = ITRReportGen.GenerateITReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, forithead, SpecialGroupslist, Convert.ToDateTime(mFromDate), Convert.ToDateTime(mToDate));
                        P2BData = Gen100;
                    }
                    else if (ReportTypeModulewiseName.ToUpper() == "ANN")
                    {
                        Gen100 = AnnualStatementReportGen.GenerateAnnualStatementReport(Convert.ToInt32(SessionManager.CompPayrollId), z, ReportName.ToUpper(), Convert.ToDateTime(mFromDate), Convert.ToDateTime(mToDate), forithead);
                        P2BData = Gen100;

                    }
                    else if (ReportTypeModulewiseName.ToUpper() == "SUM")
                    {
                        Gen100 = SummaryReportGen.GenerateSummaryReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, forithead, SpecialGroupslist, Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), Sortingslist);
                        P2BData = Gen100;

                    }
                    else if (ReportTypeModulewiseName.ToUpper() == "SAL")
                    {
                        Gen100 = PayrollSalaryReconcialiationReportGen.GenerateSalaryReconciliationReReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, forithead, SpecialGroupslist);
                        P2BData = Gen100;

                    }

                    //else
                    //{
                    //    if (Gen100 == null)
                    //        Gen100 = ReportRDLCObjectClass.GeneratePayrollReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, forithead, SpecialGroupslist);
                    //    P2BData = Gen100;

                    //}

                }

                if (ModuleName == "Leave")
                {
                    if (ReportTypeModulewiseName.ToUpper() == "LVR")
                    {
                        Boolean settlementemp = false;
                        foreach (var item in z)
                        {
                            var id = Convert.ToInt32(item);
                            var data = db.EmployeeLeave.Include(a => a.Employee).Where(a => a.Employee.Id == id).Select(a => a.Id).SingleOrDefault();
                            emplvidslist.Add(data);
                        }
                        Gen100 = LeaveReportGen.GenerateLeaveReport(Convert.ToInt32(SessionManager.CompLvId), ReportName.ToUpper(), emplvidslist, Convert.ToDateTime(mFromDate), Convert.ToDateTime(mToDate), lvhdlist, forithead, lvnamelist, salheadlist, SpecialGroupslist, Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), Convert.ToDateTime(LvFromdate), Convert.ToDateTime(LvTodate), Convert.ToDateTime(ReqDate), settlementemp, monthllist.FirstOrDefault());
                        P2BData = Gen100;
                    }
                }

                if (ModuleName == "Core")
                {
                    Gen100 = ReportRDLCObjectClass.GenerateCoreReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId));
                    P2BData = Gen100;
                }

                if (ModuleName == "Attendance")
                {
                    if (ReportTypeModulewiseName.ToUpper() == "ATT")
                    {                    
                        if (ReportName.ToUpper() == "MONTHLYMUSTER")
                        {
                            Gen100 = AttendanceReportGen.GenerateAttendanceReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, forithead, salheadlist, Convert.ToDateTime("01/" + monthly), Convert.ToDateTime(mToDate), Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), FromTime, ToTime, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), MoreThanT, SpecialGroupslist);
                        }
                        else if (ReportName.ToUpper() == "BEFOREACTIONONATTENDANCELV")
                        {
                             monthllist.Add(Paymnth);
                             Gen100 = AttendanceReportGen.GenerateAttendanceReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, forithead, salheadlist, Convert.ToDateTime(mFromDate), Convert.ToDateTime(mToDate), Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), FromTime, ToTime, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), MoreThanT, SpecialGroupslist);
                        }
                        else
                        {                           
                            Gen100 = AttendanceReportGen.GenerateAttendanceReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, forithead, salheadlist, Convert.ToDateTime(mFromDate), Convert.ToDateTime(mToDate), Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), FromTime, ToTime, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), MoreThanT, SpecialGroupslist);
                        }
                        P2BData = Gen100;

                    }
                }

                if (ModuleName == "EEIS")
                {                 
                    if (ReportTypeModulewiseName.ToUpper() == "MIS")
                    {
                        Gen100 = EMPEISReportGen.GenerateEMPEIS(Convert.ToInt32(SessionManager.CompanyId), z, monthllist, HeadList, NewObj["typeoffilter"], param1.ToString(), forithead, salheadlist, monthllist, ReportType, Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), ReportName.ToUpper());
                        P2BData = Gen100;

                    }
                    else if (ReportTypeModulewiseName.ToUpper() == "EIS")
                    {
                        if (ReportName.ToUpper() == "EMPLOYEENOTFILLAPPRAISAL" || ReportName.ToUpper() == "APPRAISALEVALUATION" || ReportName.ToUpper() == "WORKSUMMARY" || ReportName.ToUpper() == "FINALAMOUNTSUMMARY" || ReportName.ToUpper() == "MONTHLYSUMMARY")
                        {
                            Gen100 = ReportRDLCObjectClass.GenerateEEISNWReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, SpecialGroupslist, z, forithead, Convert.ToDateTime(mFromDate), Convert.ToDateTime(mToDate), Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), salheadlistLevel1, salheadlistLevel2, ReportType);
                            P2BData = Gen100;
                        }
                        else
                        {
                            Gen100 = ReportRDLCObjectClass.GenerateEEISlReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, SpecialGroupslist, z, forithead, Convert.ToDateTime(mFromDate), Convert.ToDateTime(mToDate), Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), salheadlistLevel1, salheadlistLevel2, ReportType);
                            P2BData = Gen100;
                        }
                        
                    }

                    else if (ReportTypeModulewiseName.ToUpper() == "REC")
                    {
                        Gen100 = RecruitmentReportGen.GenerateRecruitmentReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, forithead, SpecialGroupslist, Convert.ToDateTime(mFromDate), Convert.ToDateTime(mToDate), Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), salheadlistLevel1, salheadlistLevel2,ReportType);
                        P2BData = Gen100;
                    }                  
                }

                if (ModuleName == "EPFT")
                {
                    if (ReportTypeModulewiseName.ToUpper() == "PRO")
                    {
                        Gen100 = EMPEISReportGen.GenerateEMPEIS(Convert.ToInt32(SessionManager.CompanyId), z, monthllist, HeadList, NewObj["typeoffilter"], param1.ToString(), forithead, salheadlist, monthllist, ReportType, Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), ReportName.ToUpper());
                        P2BData = Gen100;

                    }
                    else if (ReportTypeModulewiseName.ToUpper() == "TRN")
                    {

                        Gen100 = ReportRDLCObjectClass.GenerateEEISlReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, SpecialGroupslist, z, forithead, Convert.ToDateTime(mFromDate), Convert.ToDateTime(mToDate), Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate), salheadlistLevel1, salheadlistLevel2, ReportType);
                        P2BData = Gen100;
                    }

                    else if (ReportTypeModulewiseName.ToUpper() == "OBJ")
                    {
                        Gen100 = RecruitmentReportGen.GenerateRecruitmentReport(Convert.ToInt32(SessionManager.CompPayrollId), z, monthllist, ReportName.ToUpper(), Convert.ToInt32(SessionManager.CompanyId), oth_idlist, salheadlist, loanadvidlist, forithead, SpecialGroupslist, Convert.ToDateTime(mFromDate), Convert.ToDateTime(mToDate), Convert.ToDateTime(pFromDate), Convert.ToDateTime(pToDate),salheadlistLevel1, salheadlistLevel2, ReportType);
                        P2BData = Gen100;
                    }

                    else
                    {
                        if (Gen100 == null)                 
                            P2BData = Gen100;
                    }
                }


                ReportfilterData obj = new ReportfilterData();
                if (Gen100 == null)
                {
                    if (qurey != "")
                    {
                        //db.Database.CommandTimeout = 180;
                        db.Database.CommandTimeout = 3600;
                        var cmd = db.Database.SqlQuery<Utility.ReportClass>(qurey).ToList<Utility.ReportClass>();
                        if (cmd.Count() == 0)
                        {
                            obj = null;
                            return obj;
                        }
                        P2BData = cmd;
                    }
                }
                string vc = "";
                if (forithead != null && forithead.Count > 0)
                {
                    vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).Select(q => q.LookupVal).AsNoTracking().FirstOrDefault();

                }
                obj.ModuleName = (string)ModuleName;
                obj.ReportName = ReportName;
                obj.monthlyforrptname = monthlyforrptname;
                obj.forithead = forithead;
                obj.TypewiseName = vc;
                obj.HeadList = HeadList;
                obj.p2bdatad = P2BData;
                return obj;
            }
        }
    }
}