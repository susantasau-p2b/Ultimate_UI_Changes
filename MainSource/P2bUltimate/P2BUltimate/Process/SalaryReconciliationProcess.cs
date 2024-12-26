using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Payroll;
using ReportPayroll;
using P2b.Global;
using P2BUltimate.Security;
using System.Web.Mvc;
using System.Transactions;


namespace P2BUltimate.Process
{
    public static class SalaryReconciliationProcess
    {


        public static void SalReconciliation(string PayMonth, string PrePayMonth)
        {
           

                int compid = Convert.ToInt32(SessionManager.CompPayrollId);
                List<SalaryT> CurSalaryEmp = null;
                List<SalaryT> PrevSalaryEmp = null;
                using (DataBaseContext db = new DataBaseContext())
                {
                  CurSalaryEmp = db.SalaryT.Include(e => e.Geostruct).Where(e => e.PayMonth == PayMonth && e.Geostruct.Company_Id == compid).AsNoTracking().ToList();
                  //CurSalaryEmp = db.SalaryT.Include(e => e.Geostruct).Where(e => e.PayMonth == PayMonth && e.Geostruct.Company_Id == compid && e.EmployeePayroll_Id==122).AsNoTracking().ToList();
                }


                foreach (var item in CurSalaryEmp)
                {
                    using (DataBaseContext db = new DataBaseContext())
                    {
                        item.EmployeePayroll = db.EmployeePayroll.Find(item.EmployeePayroll_Id);
                        item.SalEarnDedT = db.SalaryT.Where(e => e.Id == item.Id).Select(e => e.SalEarnDedT).AsNoTracking().FirstOrDefault();
                        foreach (var item1 in item.SalEarnDedT)
                        {
                            item1.SalaryHead = db.SalaryHead.Find(item1.SalaryHead_Id);
                            item1.SalaryHead.Frequency = db.LookupValue.Find(item1.SalaryHead.Frequency_Id);
                            item1.SalaryHead.Type = db.LookupValue.Find(item1.SalaryHead.Type_Id);
                        }
                       // item.SalEarnDedT = item.SalEarnDedT.Where(e => e.SalaryHead.InPayslip == true).ToList();
                        item.EmployeePayroll.PromotionServiceBook = db.PromotionServiceBook.Include(e => e.PromotionActivity).Where(e => e.EmployeePayroll_Id == item.EmployeePayroll_Id).ToList();
                        //foreach (var item1 in item.EmployeePayroll.PromotionServiceBook)
                        //{
                        //    item1.PromotionActivity = db.PromoActivity.Find(item1.PromotionActivity_Id);
                        //}
                        item.EmployeePayroll.IncrementServiceBook = db.IncrementServiceBook.Include(e => e.IncrActivity).Where(e => e.EmployeePayroll_Id == item.EmployeePayroll_Id).ToList();
                        item.EmployeePayroll.SalAttendance = db.SalAttendanceT.Where(e => e.EmployeePayroll_Id == item.EmployeePayroll_Id && e.PayMonth == PayMonth).ToList();
                    }
                }

                using (DataBaseContext db = new DataBaseContext())
                {
                        PrevSalaryEmp = db.SalaryT.Include(e => e.Geostruct).Where(e => e.PayMonth == PrePayMonth && e.Geostruct.Company_Id == compid).AsNoTracking().ToList();
                    //PrevSalaryEmp = db.SalaryT.Include(e => e.Geostruct).Where(e => e.PayMonth == PrePayMonth && e.Geostruct.Company_Id == compid && e.EmployeePayroll_Id == 122).AsNoTracking().ToList();
                }
              
                foreach (var item in PrevSalaryEmp)
                {
                    using (DataBaseContext db = new DataBaseContext())
                    {
                        item.EmployeePayroll = db.EmployeePayroll.Find(item.EmployeePayroll_Id);
                        item.SalEarnDedT = db.SalaryT.Where(e => e.Id == item.Id).Select(e => e.SalEarnDedT).AsNoTracking().FirstOrDefault();
                        foreach (var item1 in item.SalEarnDedT)
                        {
                            item1.SalaryHead = db.SalaryHead.Find(item1.SalaryHead_Id);
                            item1.SalaryHead.Frequency = db.LookupValue.Find(item1.SalaryHead.Frequency_Id);
                            item1.SalaryHead.Type = db.LookupValue.Find(item1.SalaryHead.Type_Id);
                        }
                     //   item.SalEarnDedT = item.SalEarnDedT.Where(e => e.SalaryHead.InPayslip == true).ToList();
                        item.EmployeePayroll.PromotionServiceBook = db.PromotionServiceBook.Include(e => e.PromotionActivity).Where(e => e.EmployeePayroll_Id == item.EmployeePayroll_Id).ToList();
                        item.EmployeePayroll.IncrementServiceBook = db.IncrementServiceBook.Include(e => e.IncrActivity).Where(e => e.EmployeePayroll_Id == item.EmployeePayroll_Id).ToList();
                        item.EmployeePayroll.SalAttendance = db.SalAttendanceT.Where(e => e.EmployeePayroll_Id == item.EmployeePayroll_Id && e.PayMonth == PrePayMonth).ToList();
                    }
                }

                var SalaryTList = CurSalaryEmp.Union(PrevSalaryEmp);

                //var employeePayroll = db.SalaryT
                //    .Include(e => e.EmployeePayroll)
                //   .Include(e => e.EmployeePayroll.Employee)
                //    .Include(e => e.EmployeePayroll.Employee.FuncStruct)
                //     .Include(e => e.EmployeePayroll.Employee.FuncStruct.Company)
                //    .Include(e => e.EmployeePayroll.PromotionServiceBook)
                //    .Include(e => e.EmployeePayroll.PromotionServiceBook.Select(b => b.PromotionActivity))
                //    .Include(e => e.EmployeePayroll.IncrementServiceBook)
                //    .Include(e => e.EmployeePayroll.IncrementServiceBook.Select(b => b.IncrActivity))
                //   .Include(e => e.EmployeePayroll.SalAttendance)
                //   .Include(e => e.SalEarnDedT)
                //   .Include(e => e.SalEarnDedT.Select(c => c.SalaryHead))
                //   .Include(e => e.SalEarnDedT.Select(c => c.SalaryHead.Frequency))
                //   .Include(e => e.SalEarnDedT.Select(c => c.SalaryHead.Type))
                //   .AsNoTracking().Where(e => e.PayMonth == PayMonth && e.EmployeePayroll.Employee.FuncStruct.Company.Id == compid)
                //   .Union(db.SalaryT
                //    .Include(e => e.EmployeePayroll)
                //   .Include(e => e.EmployeePayroll.Employee)
                //    .Include(e => e.EmployeePayroll.Employee.FuncStruct)
                //     .Include(e => e.EmployeePayroll.Employee.FuncStruct.Company)
                //    .Include(e => e.EmployeePayroll.PromotionServiceBook)
                //    .Include(e => e.EmployeePayroll.PromotionServiceBook.Select(b => b.PromotionActivity))
                //     .Include(e => e.EmployeePayroll.IncrementServiceBook)
                //    .Include(e => e.EmployeePayroll.IncrementServiceBook.Select(b => b.IncrActivity))
                //   .Include(e => e.EmployeePayroll.SalAttendance)
                //   .Include(e => e.SalEarnDedT)
                //   .Include(e => e.SalEarnDedT.Select(c => c.SalaryHead))
                //   .Include(e => e.SalEarnDedT.Select(c => c.SalaryHead.Frequency))
                //   .Include(e => e.SalEarnDedT.Select(c => c.SalaryHead.Type))
                //   .AsNoTracking().Where(e => e.PayMonth == PrePayMonth && e.EmployeePayroll.Employee.FuncStruct.Company.Id == compid))
                //   .ToList();

                //(e.PayMonth == PayMonth || e.PayMonth == PrePayMonth) &&
                List<EmployeePayroll> totalemp = SalaryTList.Select(e => e.EmployeePayroll).GroupBy(e => e.Id).Select(q => q.First()).ToList();

                foreach (var item in totalemp)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                         new System.TimeSpan(0, 60, 0)))
                    {
                        var salaryT = CurSalaryEmp.Where(q => q.EmployeePayroll_Id == item.Id).FirstOrDefault();
                        var presalaryT = PrevSalaryEmp.Where(q => q.EmployeePayroll_Id == item.Id).FirstOrDefault();

                        List<string> sal = new List<string>();
                        if ((salaryT != null && presalaryT != null))
                        {
                            // sal.Add(int);
                            bool Cur = false;
                            bool Prv = false;
                            Cur = true;
                            //firstLOOP
                            List<string> salhcode = SalRec(item, salaryT, presalaryT, PayMonth, PrePayMonth, sal, Cur, Prv);
                            Cur = false;
                            Prv = true;
                            //secondLOOP
                            List<string> salhcode1 =
                            SalRec(item, presalaryT, salaryT, PayMonth, PrePayMonth, salhcode, Cur, Prv);
                        }
                        if ((salaryT == null || presalaryT == null))
                        {
                            List<string> salhcoderetjoin = SalRec_retjoin(item, salaryT, presalaryT, PayMonth, PrePayMonth, sal);
                        }
                        ts.Complete();
                    }

                    
                }
            }
        

        public static List<string> SalRec(EmployeePayroll employeePayroll, SalaryT salaryT1, SalaryT salaryT2, string PayMonth, string PrePayMonth, List<string> salheads, bool Cur, bool Prv)
        {
            List<string> salheadcode = new List<string>();

            if (salaryT1 == null || salaryT2 == null)
            {
                return null;
            }
            else
            {
                var SalHeadList = salaryT1.SalEarnDedT.Where(e => (e.SalaryHead.Type.LookupVal.ToUpper().ToString() == "EARNING") && (salheads != null && !salheads.Contains(e.SalaryHead.Code))).ToList();
                foreach (var salEDT in SalHeadList)
                {
                    double? Amt = null;
                    double? PAmt = null;
                    double? Diff = null;
                    string Reason = "";
                    string salhead = "";

                    if (salEDT.SalaryHead.Frequency.LookupVal.ToUpper().ToString() == "MONTHLY")
                    {
                        salheadcode.Add(salEDT.SalaryHead.Code);
                        if (Cur == true)
                        {
                            Amt = salEDT.Amount;

                            PAmt = salaryT2.SalEarnDedT.Where(e => e.SalaryHead.Code == salEDT.SalaryHead.Code).Select(e => e.Amount).FirstOrDefault();
                        }
                        else
                        {
                            PAmt = salEDT.Amount;

                            Amt = salaryT2.SalEarnDedT.Where(e => e.SalaryHead.Code == salEDT.SalaryHead.Code).Select(e => e.Amount).FirstOrDefault();
                        }

                        if (Amt != PAmt)
                        {
                            Diff = Amt - PAmt;

                            double NoOfDays = Convert.ToDouble(salaryT1.PaybleDays);
                            double PNoOfDAYS = Convert.ToDouble(salaryT2.PaybleDays);

                            double TotalDays = Convert.ToDouble(salaryT1.TotalDays);
                            double PTotalDays = Convert.ToDouble(salaryT2.TotalDays);

                            double CLwp = TotalDays - NoOfDays;
                            double PLwp = PTotalDays - PNoOfDAYS;

                            if (PLwp != CLwp)
                            {
                                Reason += "Current month Lwp days are " + CLwp.ToString() + " Previous Month Lwp days are " + PLwp.ToString();
                            }

                            if (employeePayroll.PromotionServiceBook.Where(e => e.ReleaseDate != null && e.ReleaseDate.Value.ToString("MM/yyyy") == PayMonth).Count() > 0)
                            {
                                string PromotionActivity = employeePayroll.PromotionServiceBook.Where(e=> e.ReleaseDate != null && e.ReleaseDate.Value.ToString("MM/yyyy") == PayMonth).Select(e => e.PromotionActivity.Name).FirstOrDefault();

                                if (PromotionActivity != null || PromotionActivity != "")
                                {
                                    Reason += "Employee has promotional activity " + PromotionActivity.ToString() + "in this month.";
                                }
                            }

                            if (employeePayroll.IncrementServiceBook.Where(e => e.ReleaseDate != null && e.ReleaseDate.Value.ToString("MM/yyyy") == PayMonth).Count() > 0)
                            {
                                string IncrActivity = employeePayroll.IncrementServiceBook.Where(e => e.ReleaseDate != null && e.ReleaseDate.Value.ToString("MM/yyyy") == PayMonth).Select(e => e.IncrActivity.Name).FirstOrDefault();

                                if (IncrActivity != null || IncrActivity != "")
                                {
                                    Reason += "Employee has Increment activity " + IncrActivity.ToString() + "in this month.";
                                }
                            }


                        }
                        else
                        {
                            Reason = "Amount is Same.";
                        }
                    }
                    else if (salEDT.SalaryHead.Frequency.LookupVal.ToUpper().ToString() == "DAILY" || salEDT.SalaryHead.Frequency.LookupVal.ToUpper().ToString() == "HOURLY")
                    { 
                        salheadcode.Add(salEDT.SalaryHead.Code);
                        // salhead = salEDT.SalaryHead.Code.ToString();

                        if (Cur == true)
                        {
                            Amt = salEDT.Amount;
                            PAmt = salaryT2.SalEarnDedT.Where(e => e.SalaryHead.Code == salEDT.SalaryHead.Code).Select(e => e.Amount).SingleOrDefault();
                        }
                        else
                        {
                            PAmt = salEDT.Amount;
                            Amt = salaryT2.SalEarnDedT.Where(e => e.SalaryHead.Code == salEDT.SalaryHead.Code).Select(e => e.Amount).SingleOrDefault();
                        }

                        if (Amt != PAmt)
                        {
                            Diff = Amt - PAmt;
                            double Hrs = employeePayroll.SalAttendance.Select(e => e.PaybleHours).FirstOrDefault();
                            double PHrs = employeePayroll.SalAttendance.Where(e => e.PayMonth == PrePayMonth).Select(e => e.PaybleHours).SingleOrDefault();

                            Reason += "Current month payable hours are " + Hrs.ToString() + " Previous month payable hours are " + PHrs.ToString();
                        }
                    }
                    DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                    using (DataBaseContext db = new DataBaseContext())
                    {
                        SalaryReconcilation genericField100 = new SalaryReconcilation()
                    {
                        EmployeePayroll = db.EmployeePayroll.Find(employeePayroll.Id),                                                        //EmpCode
                        CurrentAmt = Convert.ToDouble(Amt),                                                       //Current month Amt
                        PrevAmt = Convert.ToDouble(PAmt),                                                    //Pre Month Amt
                        DiffAmt = Convert.ToDouble(Diff),                                                    //Amt Diff
                        CurrentMonth = PayMonth.ToString(),                                                                     //PayMonth
                        PrevMonth = PrePayMonth.ToString(),                                                                   //PrePayMonth
                        Reason = Reason,
                        SalaryHead = db.SalaryHead.Where(q => q.Id == salEDT.SalaryHead_Id).FirstOrDefault(),                                                                                 //salhead
                        DBTrack = dbt
                    };
                        db.SalaryReconcilation.Add(genericField100);
                        db.SaveChanges();
                    }
                }
                //Deduction
                foreach (var salEDT in salaryT1.SalEarnDedT.Where(e =>(e.SalaryHead.Type.LookupVal.ToUpper().ToString() == "DEDUCTION") && (salheads != null && !salheads.Contains(e.SalaryHead.Code))).ToList())
                {
                    string salhead = salEDT.SalaryHead.Code;
                    salheadcode.Add(salhead);

                    double? AmtD = null;
                    double? PAmtD = null;
                    if (Cur == true)
                    {
                        AmtD = salEDT.Amount;
                        PAmtD = salaryT2.SalEarnDedT.Where(e => e.SalaryHead.Code == salEDT.SalaryHead.Code).Sum(e => e.Amount);
                    }
                    else
                    {

                        PAmtD = salEDT.Amount;
                        AmtD = salaryT2.SalEarnDedT.Where(e => e.SalaryHead.Code == salEDT.SalaryHead.Code).Sum(e => e.Amount);
                    }

                    double? DiffD = null;
                    string Reason = "";

                    if (AmtD != PAmtD)
                    {
                        DiffD = AmtD - PAmtD;
                        Reason = "";
                    }
                    DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                    using (DataBaseContext db = new DataBaseContext())
                    {
                        SalaryReconcilation genericField100 = new SalaryReconcilation()
                    {
                        EmployeePayroll = db.EmployeePayroll.Find(employeePayroll.Id),                                                        //EmpCode
                        CurrentAmt = Convert.ToDouble(AmtD),                                                       //Current month Amt
                        PrevAmt = Convert.ToDouble(PAmtD),                                                    //Pre Month Amt
                        DiffAmt = Convert.ToDouble(DiffD),                                                    //Amt Diff
                        CurrentMonth = PayMonth.ToString(),                                                                     //PayMonth
                        PrevMonth = PrePayMonth.ToString(),
                        Reason = Reason,
                        DBTrack = dbt,
                        SalaryHead = db.SalaryHead.Where(q => q.Id == salEDT.SalaryHead_Id).FirstOrDefault(),                                                                                 //salhead
                    };
                        db.SalaryReconcilation.Add(genericField100);
                        db.SaveChanges();
                    }
                }
                return salheadcode;
            }
        }

        // ret and new join
        public static List<string> SalRec_retjoin(EmployeePayroll employeePayroll, SalaryT salaryT1, SalaryT salaryT2, string PayMonth, string PrePayMonth, List<string> salheads)
        {
            List<string> salhcoderetjoin = new List<string>();


            if (salaryT1 == null && salaryT2 == null)
            {
                return null;
            }

            else
            {
                SalaryT retjoin = null;
                bool ret = false;
                bool join = false;
                if (salaryT1 == null && salaryT2 != null)
                {
                    retjoin = salaryT2;
                    ret = true;
                }
                else if (salaryT1 != null && salaryT2 == null)
                {
                    retjoin = salaryT1;
                    join = true;
                }

                foreach (var salEDT in retjoin.SalEarnDedT.Where(e => (e.SalaryHead.Type.LookupVal.ToUpper().ToString() == "EARNING") && (salheads != null && !salheads.Contains(e.SalaryHead.Code))).ToList())
                {
                    double? Amt = null;
                    double? PAmt = null;
                    double? Diff = null;
                    string Reason = "";
                    string salhead = "";

                    if (salEDT.SalaryHead.Frequency.LookupVal.ToUpper().ToString() == "MONTHLY")
                    {
                        salhead = salEDT.SalaryHead.Code;
                        salhcoderetjoin.Add(salhead);
                        if (ret == true)
                        {
                            PAmt = salEDT.Amount;

                            Amt = 0;//salaryT2.SalEarnDedT.Where(e => e.SalaryHead.Code == salEDT.SalaryHead.Code).Select(e => e.Amount).SingleOrDefault();

                            Reason = "Employee is exited from the organisation.";
                        }
                        else if (join == true)
                        {
                            Amt = salEDT.Amount;

                            PAmt = 0;//salaryT2.SalEarnDedT.Where(e => e.SalaryHead.Code == salEDT.SalaryHead.Code).Select(e => e.Amount).SingleOrDefault();
                            Reason = "Employee is new join.";
                        }

                        if (Amt != PAmt)
                        {
                            Diff = Amt - PAmt;
                           
                        }
                        
                    }
                    else if (salEDT.SalaryHead.Frequency.LookupVal.ToUpper().ToString() == "DAILY" || salEDT.SalaryHead.Frequency.LookupVal.ToUpper().ToString() == "HOURLY")
                    {
                        salhead = salEDT.SalaryHead.Code;
                        salhcoderetjoin.Add(salhead);
                        // salhead = salEDT.SalaryHead.Code.ToString();

                        if (ret == true)
                        {
                            PAmt = salEDT.Amount;

                            Amt = 0;//salaryT2.SalEarnDedT.Where(e => e.SalaryHead.Code == salEDT.SalaryHead.Code).Select(e => e.Amount).SingleOrDefault();

                            Reason = "Employee is exited from the organisation.";
                        }
                        else if (join == true)
                        {
                            Amt = salEDT.Amount;

                            PAmt = 0;//salaryT2.SalEarnDedT.Where(e => e.SalaryHead.Code == salEDT.SalaryHead.Code).Select(e => e.Amount).SingleOrDefault();
                            Reason = "Employee is new join.";
                        }

                        if (Amt != PAmt)
                        {
                            Diff = Amt - PAmt;
                           
                        }
                    }
                    DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                    using (DataBaseContext db = new DataBaseContext())
                    {
                        SalaryReconcilation genericField100 = new SalaryReconcilation()
                        {
                            EmployeePayroll = db.EmployeePayroll.Find(employeePayroll.Id),                                                        //EmpCode
                            CurrentAmt = Convert.ToDouble(Amt),                                                       //Current month Amt
                            PrevAmt = Convert.ToDouble(PAmt),                                                    //Pre Month Amt
                            DiffAmt = Convert.ToDouble(Diff),                                                    //Amt Diff
                            CurrentMonth = PayMonth.ToString(),                                                                     //PayMonth
                            PrevMonth = PrePayMonth.ToString(),                                                                   //PrePayMonth
                            Reason=Reason,
                            SalaryHead = db.SalaryHead.Where(q => q.Id == salEDT.SalaryHead_Id).FirstOrDefault(),                                                                                 //salhead
                            DBTrack = dbt
                        };
                        db.SalaryReconcilation.Add(genericField100);
                        db.SaveChanges();
                    }
                }
                //Deduction

                SalaryT retjoind = null;
                bool retd = false;
                bool joind= false;
                if (salaryT1 == null && salaryT2 != null)
                {
                    retjoin = salaryT2;
                    retd = true;
                }
                else if (salaryT1 != null && salaryT2 == null)
                {
                    retjoin = salaryT1;
                    joind = true;
                }

                foreach (var salEDT in retjoin.SalEarnDedT.Where(e => (e.SalaryHead.Type.LookupVal.ToUpper().ToString() == "DEDUCTION") && (salheads != null && !salheads.Contains(e.SalaryHead.Code))).ToList())
                {
                    string salhead = salEDT.SalaryHead.Code;
                    salhcoderetjoin.Add(salhead);

                    double? AmtD = null;
                    double? PAmtD = null;
                    if (retd == true)
                    {
                        PAmtD = salEDT.Amount;
                        AmtD = 0;//salaryT2.SalEarnDedT.Where(e => e.SalaryHead.Code == salEDT.SalaryHead.Code).Sum(e => e.Amount);
                    }
                    else if (joind == true)
                    {

                        AmtD = salEDT.Amount;
                        PAmtD = 0;//salaryT2.SalEarnDedT.Where(e => e.SalaryHead.Code == salEDT.SalaryHead.Code).Sum(e => e.Amount);
                    }

                    double? DiffD = null;
                    string Reason = "";

                    if (AmtD != PAmtD)
                    {
                        DiffD = AmtD - PAmtD;
                        Reason = "";
                    }
                    DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                    using (DataBaseContext db = new DataBaseContext())
                    {
                        SalaryReconcilation genericField100 = new SalaryReconcilation()
                        {
                            EmployeePayroll = db.EmployeePayroll.Find(employeePayroll.Id),                                                        //EmpCode
                            CurrentAmt = Convert.ToDouble(AmtD),                                                       //Current month Amt
                            PrevAmt = Convert.ToDouble(PAmtD),                                                    //Pre Month Amt
                            DiffAmt = Convert.ToDouble(DiffD),                                                    //Amt Diff
                            CurrentMonth = PayMonth.ToString(),                                                                     //PayMonth
                            PrevMonth = PrePayMonth.ToString(),
                            Reason = Reason,
                            DBTrack = dbt,
                            SalaryHead = db.SalaryHead.Where(q => q.Id == salEDT.SalaryHead_Id).FirstOrDefault(),                                                                                 //salhead
                        };
                        db.SalaryReconcilation.Add(genericField100);
                        db.SaveChanges();
                    }
                }
                return salhcoderetjoin;
            }
        }

        //ret and new join


       
    }
}