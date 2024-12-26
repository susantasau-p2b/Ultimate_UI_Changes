using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Data.Entity;
using P2BUltimate.Models;
using Payroll;
using System.Diagnostics;
using P2BUltimate.Security;
using Leave;
using System.Web;
namespace P2BUltimate.Process
{
    public class LeaveStructureProcess
    {
        public static LeaveHeadFormulaT LeaveHeadFinderNew(EmployeeLvStruct OEmpSalstruct, List<LeaveHeadFormulaT> OPayScaleAssignment, int OSalaryHeadID)
        {
            var OEmployeeSalStruct = OEmpSalstruct;
            LeaveHeadFormulaT OLeaveHeadFormula = null;
            LeaveHeadFormulaT OLeaveHeadFormulaGeo = null;
            LeaveHeadFormulaT OLeaveHeadFormulaPay = null;
            LeaveHeadFormulaT OLeaveHeadFormulaFunc = null;
            LeaveHeadFormulaT OLeaveHeadFormulaGeopay = null;
            LeaveHeadFormulaT OLeaveHeadFormulaGeofun = null;
            LeaveHeadFormulaT OLeaveHeadFormulaGeopayfun = null;

            OLeaveHeadFormulaGeo = OPayScaleAssignment
              .Where(r => r.GeoStruct != null && OEmployeeSalStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeSalStruct.GeoStruct.Id && r.FuncStruct == null && r.PayStruct == null).FirstOrDefault();

            OLeaveHeadFormula = OLeaveHeadFormulaGeo;


            if (OLeaveHeadFormula == null)
            {
                OLeaveHeadFormulaPay = OPayScaleAssignment
              .Where(r => r.PayStruct != null && OEmployeeSalStruct.PayStruct != null && r.PayStruct.Id == OEmployeeSalStruct.PayStruct.Id && r.FuncStruct == null && r.GeoStruct == null).FirstOrDefault();

            }
            else
            {
                OLeaveHeadFormulaPay = OPayScaleAssignment
              .Where(r =>  r.PayStruct != null && OEmployeeSalStruct.PayStruct != null && r.PayStruct.Id == OEmployeeSalStruct.PayStruct.Id && r.FuncStruct == null && r.GeoStruct == null).FirstOrDefault();

            }

            if (OLeaveHeadFormulaPay != null)
            {
                OLeaveHeadFormula = OLeaveHeadFormulaPay;
            }
            if (OLeaveHeadFormula == null)
            {
                OLeaveHeadFormulaFunc = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeeSalStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeSalStruct.FuncStruct.Id && r.PayStruct == null && r.GeoStruct == null).FirstOrDefault();

            }
            else
            {
                OLeaveHeadFormulaFunc = OPayScaleAssignment
              .Where(r =>  r.FuncStruct != null && OEmployeeSalStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeSalStruct.FuncStruct.Id && r.PayStruct == null && r.GeoStruct == null).FirstOrDefault();

            }

            if (OLeaveHeadFormulaFunc != null)
            {
                OLeaveHeadFormula = OLeaveHeadFormulaFunc;
            }

            OLeaveHeadFormulaGeopay = OPayScaleAssignment
                .Where(r => r.PayStruct != null && OEmployeeSalStruct.PayStruct != null && r.PayStruct.Id == OEmployeeSalStruct.PayStruct.Id && r.GeoStruct != null && OEmployeeSalStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeSalStruct.GeoStruct.Id && r.FuncStruct == null).FirstOrDefault();

            if (OLeaveHeadFormulaGeopay != null)
            {
                OLeaveHeadFormula = OLeaveHeadFormulaGeopay;
            }

            OLeaveHeadFormulaGeofun = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeeSalStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeSalStruct.FuncStruct.Id && r.GeoStruct != null && OEmployeeSalStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeSalStruct.GeoStruct.Id && r.PayStruct == null).FirstOrDefault();

            if (OLeaveHeadFormulaGeofun != null)
            {
                OLeaveHeadFormula = OLeaveHeadFormulaGeofun;
            }

            OLeaveHeadFormulaGeopayfun = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeeSalStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeSalStruct.FuncStruct.Id && r.GeoStruct != null && OEmployeeSalStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeSalStruct.GeoStruct.Id && r.PayStruct != null && OEmployeeSalStruct.PayStruct != null && r.PayStruct.Id == OEmployeeSalStruct.PayStruct.Id).FirstOrDefault();

            if (OLeaveHeadFormulaGeopayfun != null)
            {
                OLeaveHeadFormula = OLeaveHeadFormulaGeopayfun;
            }


            return OLeaveHeadFormula;
        }//return salheadformula

        public static void EmployeeLeaveStructCreationTest(EmployeeLeave OEmployeePayroll, int OEmployee_Id,
           int OPayScaleAgreement_Id, DateTime mEffectiveDate, int ComPanyLeave_Id)
        {
            // db.RefreshAllEntites(RefreshMode.StoreWins);
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                //var Compayroll_id = Convert.ToInt32(SessionManager.CompPayrollId);
                //var CompanyPayroll_OPayScaleAssignment = db.CompanyPayroll
                //    .Include(a => a.PayScaleAssignment)
                //    .Include(a => a.PayScaleAssignment.Select(e => e.PayScaleAgreement))
                //     .Include(a => a.PayScaleAssignment.Select(e => e.SalaryHead))
                //    .Include(a => a.PayScaleAssignment.Select(e => e.SalHeadFormula))
                //    .Include(a => a.PayScaleAssignment.Select(e => e.SalHeadFormula.Select(t=>t.FormulaType)))
                //    .Include(a => a.PayScaleAssignment.Select(t => t.SalHeadFormula.Select(e => e.FuncStruct)))
                //    .Include(a => a.PayScaleAssignment.Select(t => t.SalHeadFormula.Select(e => e.GeoStruct)))
                //    .Include(a => a.PayScaleAssignment.Select(t => t.SalHeadFormula.Select(e => e.PayStruct)))
                //    .Where(e => e.Id == Compayroll_id ).SingleOrDefault();


                var OPayScaleAssignment = db.LvAssignment
                        .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                       .Where(p => p.p.PayScaleAgreement.Id == OPayScaleAgreement_Id && p.p.CompanyLeave_Id == ComPanyLeave_Id)
                       .Select(m => new
                       {
                           LvAssignment = m.p, // or m.ppc.pc.ProdId
                           LvHeadFormula = m.p.LvHeadFormula.Select(t => new LeaveHeadFormulaT
                           {
                               GeoStruct = t.GeoStruct,
                               PayStruct = t.PayStruct,
                               FuncStruct = t.FuncStruct,
                               Name = t.Name,
                               Id = t.Id,
                               LvBankPolicy = t.LvBankPolicy,
                               LvCreditPolicy = t.LvCreditPolicy,
                               LvDebitPolicy = t.LvDebitPolicy,
                               LvEncashPolicy = t.LvEncashPolicy
                           }),
                           LvHead = m.p.LvHead,
                           PolicyName = m.p.PolicyName
                           // other assignments
                       }).ToList();


                //Employee OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.EmpOffInfo)
                //                      .Where(r => r.Id == OEmployee_Id).SingleOrDefault();

                var OEmployee = db.Employee
                    .Where(r => r.Id == OEmployee_Id).Select(a => new
                {
                    Id = a.Id,
                    GeoStruct = a.GeoStruct,
                    FuncStruct = a.FuncStruct,
                    PayStruct = a.PayStruct
                }).FirstOrDefault();


                //EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == OEmployeePayroll_Id).SingleOrDefault();

                if (OPayScaleAssignment.Count() == 0)
                {
                    return;
                }
                //var OPayScaleAssignment = CompanyPayroll_OPayScaleAssignment.PayScaleAssignment.Where(e => e.PayScaleAgreement != null &&
                //    e.PayScaleAgreement.Id == OPayScaleAgreement_Id).ToList();

                EmployeeLvStruct EmployeeLvStruct = new EmployeeLvStruct();
                {
                    EmployeeLvStruct.EffectiveDate = mEffectiveDate;
                    if (OEmployee.GeoStruct != null)
                    {
                        EmployeeLvStruct.GeoStruct = OEmployee.GeoStruct; //db.GeoStruct.Where(e => e.Id == OEmployee.GeoStruct.Id).SingleOrDefault();
                    };
                    if (OEmployee.FuncStruct != null)
                    {
                        EmployeeLvStruct.FuncStruct = OEmployee.FuncStruct; // db.FuncStruct.Where(e => e.Id == OEmployee.FuncStruct.Id).SingleOrDefault();
                    };
                    if (OEmployee.PayStruct != null)
                    {
                        EmployeeLvStruct.PayStruct = OEmployee.PayStruct; // db.PayStruct.Where(e => e.Id == OEmployee.PayStruct.Id).SingleOrDefault();
                    };
                    EmployeeLvStruct.DBTrack = dbt;
                    //db.EmpSalStruct.Add(OEmpSalStruct);
                    //db.SaveChanges();
                    List<EmployeeLvStructDetails> OEmpLvStructDetails = new List<EmployeeLvStructDetails>();
                    EmployeeLvStructDetails OEmpLeaveStructDetailsObj = null;
                    foreach (var OPayScaleAssignmentData in OPayScaleAssignment)
                    {
                        if (OPayScaleAssignmentData.LvHeadFormula.Where(r => r.LvBankPolicy != null).Count() > 0)
                        {
                            OEmpLeaveStructDetailsObj = new EmployeeLvStructDetails();
                            {
                                //  OEmpSalStructDetailsObj.Amount = 0;
                                if (OPayScaleAssignmentData != null)
                                {
                                    OEmpLeaveStructDetailsObj.LvAssignment = OPayScaleAssignmentData.LvAssignment; //db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                }
                                if (OPayScaleAssignmentData.LvHead != null)
                                {
                                    OEmpLeaveStructDetailsObj.LvHead = OPayScaleAssignmentData.LvHead;  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                }
                                if (OPayScaleAssignmentData.PolicyName != null)
                                {
                                    OEmpLeaveStructDetailsObj.PolicyName = OPayScaleAssignmentData.PolicyName;  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                }

                                if (OPayScaleAssignmentData.PolicyName != null && OPayScaleAssignmentData.LvHead != null && OPayScaleAssignmentData.LvHeadFormula.Count() > 0)//newly added by prashant on 24042017
                                {

                                    LeaveHeadFormulaT Leaveformula = new LeaveHeadFormulaT();

                                    Leaveformula = LeaveHeadFinderNew(EmployeeLvStruct, OPayScaleAssignmentData.LvHeadFormula.Where(r => r.LvBankPolicy != null).ToList(), OPayScaleAssignmentData.LvHead.Id);
                                    OEmpLeaveStructDetailsObj.LvHeadFormula = Leaveformula == null ? null : db.LvHeadFormula.Where(e => e.Id == Leaveformula.Id).SingleOrDefault();
                                    //OEmpLeaveStructDetailsObj.LvNewReq == null;
                                }
                                else
                                {
                                    OEmpLeaveStructDetailsObj.LvHeadFormula = null;
                                }
                                OEmpLeaveStructDetailsObj.DBTrack = dbt;
                                //OEmpLvStructDetails.Add(OEmpLeaveStructDetailsObj);
                            };
                        }


                        if (OPayScaleAssignmentData.LvHeadFormula.Where(r => r.LvCreditPolicy != null).Count() > 0)
                        {
                            OEmpLeaveStructDetailsObj = new EmployeeLvStructDetails();
                            {
                                //  OEmpSalStructDetailsObj.Amount = 0;
                                if (OPayScaleAssignmentData != null)
                                {
                                    OEmpLeaveStructDetailsObj.LvAssignment = OPayScaleAssignmentData.LvAssignment; //db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                }
                                if (OPayScaleAssignmentData.LvHead != null)
                                {
                                    OEmpLeaveStructDetailsObj.LvHead = OPayScaleAssignmentData.LvHead;  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                }
                                if (OPayScaleAssignmentData.PolicyName != null)
                                {
                                    OEmpLeaveStructDetailsObj.PolicyName = OPayScaleAssignmentData.PolicyName;  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                }
                                if (OPayScaleAssignmentData.PolicyName != null && OPayScaleAssignmentData.LvHead != null && OPayScaleAssignmentData.LvHeadFormula.Count() > 0)//newly added by prashant on 24042017
                                {

                                    LeaveHeadFormulaT Leaveformula = new LeaveHeadFormulaT();

                                    Leaveformula = LeaveHeadFinderNew(EmployeeLvStruct, OPayScaleAssignmentData.LvHeadFormula.Where(r => r.LvCreditPolicy != null).ToList(), OPayScaleAssignmentData.LvHead.Id);
                                    OEmpLeaveStructDetailsObj.LvHeadFormula = Leaveformula == null ? null : db.LvHeadFormula.Where(e => e.Id == Leaveformula.Id).SingleOrDefault();
                                    //OEmpLeaveStructDetailsObj.LvNewReq == null;
                                }
                                else
                                {
                                    OEmpLeaveStructDetailsObj.LvHeadFormula = null;
                                }
                                OEmpLeaveStructDetailsObj.DBTrack = dbt;
                               // OEmpLvStructDetails.Add(OEmpLeaveStructDetailsObj);

                            };
                        }
                        if (OPayScaleAssignmentData.LvHeadFormula.Where(r => r.LvDebitPolicy != null).Count() > 0)
                        {

                            OEmpLeaveStructDetailsObj = new EmployeeLvStructDetails();
                            {
                                //  OEmpSalStructDetailsObj.Amount = 0;
                                if (OPayScaleAssignmentData != null)
                                {
                                    OEmpLeaveStructDetailsObj.LvAssignment = OPayScaleAssignmentData.LvAssignment; //db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                }
                                if (OPayScaleAssignmentData.LvHead != null)
                                {
                                    OEmpLeaveStructDetailsObj.LvHead = OPayScaleAssignmentData.LvHead;  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                }
                                if (OPayScaleAssignmentData.PolicyName != null)
                                {
                                    OEmpLeaveStructDetailsObj.PolicyName = OPayScaleAssignmentData.PolicyName;  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                }

                                if (OPayScaleAssignmentData.PolicyName != null && OPayScaleAssignmentData.LvHead != null && OPayScaleAssignmentData.LvHeadFormula.Count() > 0)//newly added by prashant on 24042017
                                {

                                    LeaveHeadFormulaT Leaveformula = new LeaveHeadFormulaT();

                                    Leaveformula = LeaveHeadFinderNew(EmployeeLvStruct, OPayScaleAssignmentData.LvHeadFormula.Where(r => r.LvDebitPolicy != null).ToList(), OPayScaleAssignmentData.LvHead.Id);
                                    OEmpLeaveStructDetailsObj.LvHeadFormula = Leaveformula == null ? null : db.LvHeadFormula.Where(e => e.Id == Leaveformula.Id).SingleOrDefault();
                                    //OEmpLeaveStructDetailsObj.LvNewReq == null;
                                }
                                else
                                {
                                    OEmpLeaveStructDetailsObj.LvHeadFormula = null;
                                }
                                OEmpLeaveStructDetailsObj.DBTrack = dbt;
                                //OEmpLvStructDetails.Add(OEmpLeaveStructDetailsObj);

                            };
                        }
                        if (OPayScaleAssignmentData.LvHeadFormula.Where(r => r.LvEncashPolicy != null).Count() > 0)
                        {
                            OEmpLeaveStructDetailsObj = new EmployeeLvStructDetails();
                            {
                                //  OEmpSalStructDetailsObj.Amount = 0;
                                if (OPayScaleAssignmentData != null)
                                {
                                    OEmpLeaveStructDetailsObj.LvAssignment = OPayScaleAssignmentData.LvAssignment; //db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                }
                                if (OPayScaleAssignmentData.LvHead != null)
                                {
                                    OEmpLeaveStructDetailsObj.LvHead = OPayScaleAssignmentData.LvHead;  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                }
                                if (OPayScaleAssignmentData.PolicyName != null)
                                {
                                    OEmpLeaveStructDetailsObj.PolicyName = OPayScaleAssignmentData.PolicyName;  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                }

                                if (OPayScaleAssignmentData.PolicyName != null && OPayScaleAssignmentData.LvHead != null && OPayScaleAssignmentData.LvHeadFormula.Count() > 0)//newly added by prashant on 24042017
                                {

                                    LeaveHeadFormulaT Leaveformula = new LeaveHeadFormulaT();

                                    Leaveformula = LeaveHeadFinderNew(EmployeeLvStruct, OPayScaleAssignmentData.LvHeadFormula.Where(r => r.LvEncashPolicy != null).ToList(), OPayScaleAssignmentData.LvHead.Id);
                                    OEmpLeaveStructDetailsObj.LvHeadFormula = Leaveformula == null ? null : db.LvHeadFormula.Where(e => e.Id == Leaveformula.Id).SingleOrDefault();
                                    //OEmpLeaveStructDetailsObj.LvNewReq == null;
                                }
                                else
                                {
                                    OEmpLeaveStructDetailsObj.LvHeadFormula = null;
                                }
                                OEmpLeaveStructDetailsObj.DBTrack = dbt;
                                //OEmpLvStructDetails.Add(OEmpLeaveStructDetailsObj);
                            };
                        }
                          OEmpLvStructDetails.Add(OEmpLeaveStructDetailsObj);

                    }
                    EmployeeLvStruct.EmployeeLvStructDetails = OEmpLvStructDetails;
                    db.EmployeeLvStruct.Add(EmployeeLvStruct);
                    db.SaveChanges();
                    //db.EmpSalStructDetails.AddRange(OEmpSalStructDetails);
                    //db.SaveChanges();
                    //List<EmpSalStructDetails> OFAT = new List<EmpSalStructDetails>();
                    //var aa = db.EmpSalStruct.Where(e => e.Id == OEmpSalStruct.Id).SingleOrDefault();
                    //OFAT.AddRange(OEmpSalStructDetails);
                    //aa.EmpSalStructDetails = OFAT;
                    //db.EmpSalStruct.Attach(aa);
                    //db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();
                }
                try
                {

                    List<EmployeeLvStruct> OTemp2 = new List<EmployeeLvStruct>();
                    OTemp2.Add(db.EmployeeLvStruct.Where(e => e.Id == EmployeeLvStruct.Id).SingleOrDefault());
                    if (OEmployeePayroll == null)
                    {
                        EmployeeLeave OTEP = new EmployeeLeave()
                        {
                            Employee = db.Employee.Where(e => e.Id == OEmployee.Id).SingleOrDefault(),
                            EmployeeLvStruct = OTemp2,
                            DBTrack = dbt
                        };
                        db.EmployeeLeave.Add(OTEP);
                        db.SaveChanges();
                    }
                    else
                    {
                        var aa = db.EmployeeLeave.Where(e => e.Id == OEmployeePayroll.Id).SingleOrDefault();
                        aa.EmployeeLvStruct = OTemp2;
                        db.EmployeeLeave.Attach(aa);
                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = "EmployeeLvStructCreation - LeaveStructureProcess",
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                }
            }
        }
        public class LeaveHeadFormulaT
        {
            public int Id { get; set; }
            public GeoStruct GeoStruct { get; set; }
            public PayStruct PayStruct { get; set; }
            public FuncStruct FuncStruct { get; set; }
            public LvBankPolicy LvBankPolicy { get; set; }
            public LvCreditPolicy LvCreditPolicy { get; set; }
            public LvDebitPolicy LvDebitPolicy { get; set; }
            public LvEncashPolicy LvEncashPolicy { get; set; }
            public string Name { get; set; }
        }

        public static void EmployeeLvStructureCreationWithUpdationTest(int OEmpSalStructId, int OEmployeeId,
        int OPayScaleAgreementId, DateTime mEffectiveDate, int OEmployeePayroll_Id, int ComPanyLeave_Id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                int CompId = Convert.ToInt32(SessionManager.CompanyId);


                //var OCompanyPayroll = db.CompanyPayroll.Include(e => e.PayScaleAssignment)
                //  .Include(e => e.PayScaleAssignment.Select(r => r.SalaryHead))
                //  .Include(e => e.PayScaleAssignment.Select(r => r.SalaryHead.SalHeadOperationType))
                //  .Include(e => e.PayScaleAssignment.Select(r => r.SalHeadFormula)).Where(e => e.Id == CompId).SingleOrDefault();
                //var OPayScaleAssignment = OCompanyPayroll.PayScaleAssignment.ToList();

                //var OPayScaleAssignment = db.PayScaleAssignment
                //        .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                //       .Where(p => p.p.PayScaleAgreement.Id == OPayScaleAgreementId && p.p.CompanyPayroll.Id == CompId)
                //       .Select(m => new
                //       {
                //           PayScaleAssignment = m.p,
                //           SalHeadOperationType = m.p.SalaryHead.SalHeadOperationType,
                //           Id = m.p.Id, // or m.ppc.pc.ProdId
                //           Salheadformula = m.p.SalHeadFormula.Select(t => new SalHeadFormulaT
                //           {
                //               GeoStruct = t.GeoStruct,
                //               PayStruct = t.PayStruct,
                //               FuncStruct = t.FuncStruct,
                //               FormulaType = t.FormulaType,
                //               Name = t.Name,
                //               Id = t.Id,
                //           }),
                //           SalaryHead = m.p.SalaryHead
                //           // other assignments
                //       }).ToList();

                var OPayScaleAssignment = db.LvAssignment
                        .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                       .Where(p => p.p.PayScaleAgreement.Id == OPayScaleAgreementId && p.p.CompanyLeave_Id == ComPanyLeave_Id)
                       .Select(m => new
                       {
                           LvAssignment = m.p, // or m.ppc.pc.ProdId
                           Id = m.p.Id,
                           LvHeadFormula = m.p.LvHeadFormula.Select(t => new LeaveHeadFormulaT
                           {
                               GeoStruct = t.GeoStruct,
                               PayStruct = t.PayStruct,
                               FuncStruct = t.FuncStruct,
                               Name = t.Name,
                               Id = t.Id,
                               LvBankPolicy = t.LvBankPolicy,
                               LvCreditPolicy = t.LvCreditPolicy,
                               LvDebitPolicy = t.LvDebitPolicy,
                               LvEncashPolicy = t.LvEncashPolicy
                           }),
                           LvHead = m.p.LvHead,
                           PolicyName = m.p.PolicyName
                           // other assignments
                       }).ToList();


                EmployeeLeave OEmployeePayroll = db.EmployeeLeave
                    //.Include(e => e.Employee.GeoStruct)
                    //.Include(e => e.Employee.PayStruct)
                    //.Include(e => e.Employee.FuncStruct)
                               .Include(e => e.EmployeeLvStruct)
                               .Include(e => e.Employee.EmpOffInfo)
                               .Include(e => e.Employee.EmpOffInfo.PayScale).AsNoTracking().OrderBy(e => e.Id)
                               .Where(e => e.Id == OEmployeePayroll_Id).SingleOrDefault();



                Employee OEmployee = db.Employee
                    //.Include(e => e.GeoStruct)
                    //.Include(e => e.FuncStruct)
                    //.Include(e => e.PayStruct)
                                                   .Include(e => e.EmpOffInfo)
                                                    .Where(e => e.Id == OEmployeePayroll.Employee.Id)
                                                    .SingleOrDefault();

                List<EmployeeLvStructDetails> OEmpLeaveStructDetails = new List<EmployeeLvStructDetails>();
                int Count = 0;
                EmployeeLvStruct OEmpLvStruct = db.EmployeeLvStruct.Include(e => e.EmployeeLvStructDetails)
                     .Include(e => e.EmployeeLvStructDetails.Select(r => r.PolicyName))
                    .Include(e => e.EmployeeLvStructDetails.Select(r => r.LvHead)).Include(e => e.EmployeeLvStructDetails.Select(r => r.LvHead))
                    .Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.PayStruct).Where(e => e.Id == OEmpSalStructId).FirstOrDefault();//OEmployeePayroll.EmpSalStruct.Where(e => e.EffectiveDate == mEffectiveDate).SingleOrDefault(); 


                //foreach (var a in OEmpSalStruct.EmpSalStructDetails.ToList())
                //{
                //    if (a.PayScaleAssignment == null)
                //    {
                //        OEmpSalStruct.EmpSalStructDetails.Remove(a);
                //    }
                //}

                OEmpLvStruct.EmployeeLvStructDetails.Where(x => x.LvAssignment == null).ToList().ForEach(x =>
                {
                    OEmpLvStruct.EmployeeLvStructDetails.Remove(x);
                });
                db.SaveChanges();
                EmployeeLvStructDetails OEmpLvStructDetailsObj = null;
                foreach (var a in OPayScaleAssignment)
                {

                    foreach (var b in OEmpLvStruct.EmployeeLvStructDetails)
                    {
                        if (a.LvHead.Id == b.LvHead.Id && a.PolicyName.Id == b.PolicyName.Id)
                        {
                            Count = 0;
                            break;
                        }
                        Count = 1;
                    }
                    if (Count == 1)
                    {
                        //if (a.LvHead.Code.ToUpper() == "VPF" && !OEmployee.EmpOffInfo.VPFAppl)
                        //{
                        //    continue;
                        //}
                        if (a.LvHeadFormula.Where(r => r.LvBankPolicy != null).Count() > 0)
                        {
                            OEmpLvStructDetailsObj = new EmployeeLvStructDetails();
                            {
                                if (a != null)
                                {
                                    OEmpLvStructDetailsObj.LvAssignment = db.LvAssignment
                                        .Include(e => e.LvHeadFormula)
                                        .Include(e => e.LvHeadFormula.Select(z => z.GeoStruct))
                                        .Include(e => e.LvHeadFormula.Select(z => z.PayStruct))
                                        .Include(e => e.LvHeadFormula.Select(z => z.FuncStruct))
                                        .Where(e => e.Id == a.Id).FirstOrDefault();
                                }
                                if (a.LvHead != null)
                                {
                                    OEmpLvStructDetailsObj.LvHead = db.LvHead.Where(e => e.Id == a.LvHead.Id).FirstOrDefault();
                                }
                                if (a.PolicyName != null)
                                {
                                    OEmpLvStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();
                                }
                                if (OEmpLvStructDetailsObj.PolicyName != null && OEmpLvStructDetailsObj.LvHead != null && OEmpLvStructDetailsObj.LvAssignment.LvHeadFormula != null)//newly added by prashant on 24042017
                                {
                                    LeaveHeadFormulaT Lvformula = new LeaveHeadFormulaT();

                                    Lvformula = LeaveHeadFinderNew(OEmpLvStruct, a.LvHeadFormula.Where(e => e.LvBankPolicy != null).ToList(), a.LvHead.Id);

                                    OEmpLvStructDetailsObj.LvHeadFormula = Lvformula == null ? null : db.LvHeadFormula.Where(e => e.Id == Lvformula.Id).FirstOrDefault(); ;
                                } //added by Rekha 13072017

                                OEmpLvStructDetailsObj.DBTrack = dbt;
                                OEmpLeaveStructDetails.Add(OEmpLvStructDetailsObj);
                            };
                        }
                        if (a.LvHeadFormula.Where(r => r.LvCreditPolicy != null).Count() > 0)
                        {
                            OEmpLvStructDetailsObj = new EmployeeLvStructDetails();
                            {
                                if (a != null)
                                {
                                    OEmpLvStructDetailsObj.LvAssignment = db.LvAssignment
                                        .Include(e => e.LvHeadFormula)
                                        .Include(e => e.LvHeadFormula.Select(z => z.GeoStruct))
                                        .Include(e => e.LvHeadFormula.Select(z => z.PayStruct))
                                        .Include(e => e.LvHeadFormula.Select(z => z.FuncStruct))
                                        .Where(e => e.Id == a.Id).FirstOrDefault();
                                }
                                if (a.LvHead != null)
                                {
                                    OEmpLvStructDetailsObj.LvHead = db.LvHead.Where(e => e.Id == a.LvHead.Id).FirstOrDefault();
                                }
                                if (a.PolicyName != null)
                                {
                                    OEmpLvStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();
                                }
                                if (OEmpLvStructDetailsObj.PolicyName != null && OEmpLvStructDetailsObj.LvHead != null && OEmpLvStructDetailsObj.LvAssignment.LvHeadFormula != null)//newly added by prashant on 24042017
                                {
                                    LeaveHeadFormulaT Lvformula = new LeaveHeadFormulaT();

                                    Lvformula = LeaveHeadFinderNew(OEmpLvStruct, a.LvHeadFormula.Where(e => e.LvCreditPolicy != null).ToList(), a.LvHead.Id);

                                    OEmpLvStructDetailsObj.LvHeadFormula = Lvformula == null ? null : db.LvHeadFormula.Where(e => e.Id == Lvformula.Id).FirstOrDefault(); ;
                                } //added by Rekha 13072017

                                OEmpLvStructDetailsObj.DBTrack = dbt;
                                OEmpLeaveStructDetails.Add(OEmpLvStructDetailsObj);
                            };
                        }
                        if (a.LvHeadFormula.Where(r => r.LvDebitPolicy != null).Count() > 0)
                        {
                            OEmpLvStructDetailsObj = new EmployeeLvStructDetails();
                            {
                                if (a != null)
                                {
                                    OEmpLvStructDetailsObj.LvAssignment = db.LvAssignment
                                        .Include(e => e.LvHeadFormula)
                                        .Include(e => e.LvHeadFormula.Select(z => z.GeoStruct))
                                        .Include(e => e.LvHeadFormula.Select(z => z.PayStruct))
                                        .Include(e => e.LvHeadFormula.Select(z => z.FuncStruct))
                                        .Where(e => e.Id == a.Id).FirstOrDefault();
                                }
                                if (a.LvHead != null)
                                {
                                    OEmpLvStructDetailsObj.LvHead = db.LvHead.Where(e => e.Id == a.LvHead.Id).FirstOrDefault();
                                }
                                if (a.PolicyName != null)
                                {
                                    OEmpLvStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();
                                }
                                if (OEmpLvStructDetailsObj.PolicyName != null && OEmpLvStructDetailsObj.LvHead != null && OEmpLvStructDetailsObj.LvAssignment.LvHeadFormula != null)//newly added by prashant on 24042017
                                {
                                    LeaveHeadFormulaT Lvformula = new LeaveHeadFormulaT();

                                    Lvformula = LeaveHeadFinderNew(OEmpLvStruct, a.LvHeadFormula.Where(e => e.LvDebitPolicy != null).ToList(), a.LvHead.Id);

                                    OEmpLvStructDetailsObj.LvHeadFormula = Lvformula == null ? null : db.LvHeadFormula.Where(e => e.Id == Lvformula.Id).FirstOrDefault(); ;
                                } //added by Rekha 13072017

                                OEmpLvStructDetailsObj.DBTrack = dbt;
                                OEmpLeaveStructDetails.Add(OEmpLvStructDetailsObj);
                            };
                        }
                        if (a.LvHeadFormula.Where(r => r.LvEncashPolicy != null).Count() > 0)
                        {
                            OEmpLvStructDetailsObj = new EmployeeLvStructDetails();
                            {
                                if (a != null)
                                {
                                    OEmpLvStructDetailsObj.LvAssignment = db.LvAssignment
                                        .Include(e => e.LvHeadFormula)
                                        .Include(e => e.LvHeadFormula.Select(z => z.GeoStruct))
                                        .Include(e => e.LvHeadFormula.Select(z => z.PayStruct))
                                        .Include(e => e.LvHeadFormula.Select(z => z.FuncStruct))
                                        .Where(e => e.Id == a.Id).FirstOrDefault();
                                }
                                if (a.LvHead != null)
                                {
                                    OEmpLvStructDetailsObj.LvHead = db.LvHead.Where(e => e.Id == a.LvHead.Id).FirstOrDefault();
                                }
                                if (a.PolicyName != null)
                                {
                                    OEmpLvStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();
                                }
                                if (OEmpLvStructDetailsObj.PolicyName != null && OEmpLvStructDetailsObj.LvHead != null && OEmpLvStructDetailsObj.LvAssignment.LvHeadFormula != null)//newly added by prashant on 24042017
                                {
                                    LeaveHeadFormulaT Lvformula = new LeaveHeadFormulaT();

                                    Lvformula = LeaveHeadFinderNew(OEmpLvStruct, a.LvHeadFormula.Where(e => e.LvEncashPolicy != null).ToList(), a.LvHead.Id);

                                    OEmpLvStructDetailsObj.LvHeadFormula = Lvformula == null ? null : db.LvHeadFormula.Where(e => e.Id == Lvformula.Id).FirstOrDefault(); ;
                                } //added by Rekha 13072017

                                OEmpLvStructDetailsObj.DBTrack = dbt;
                                OEmpLeaveStructDetails.Add(OEmpLvStructDetailsObj);
                            };
                        }

                        //OEmpLeaveStructDetails.Add(OEmpLvStructDetailsObj);
                    }
                    // }
                }
                db.EmployeeLvStructDetails.AddRange(OEmpLeaveStructDetails);
                OEmpLeaveStructDetails.AddRange(OEmpLvStruct.EmployeeLvStructDetails);
                OEmpLvStruct.EmployeeLvStructDetails = OEmpLeaveStructDetails;

                db.EmployeeLvStruct.Attach(OEmpLvStruct);
                db.Entry(OEmpLvStruct).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                //   EmployeeSalaryStructUpdateNew(OEmpLvStruct, OEmployeePayroll, mEffectiveDate);
                OEmployeePayroll.EmployeeLvStruct.Add(OEmpLvStruct);

                db.SaveChanges();

            }

        }

        public static void EmployeeLvStructureCreationWithUpdateFormulaTest(int OEmpLvStructId, int OEmployee_Id,
           int PayScaleAgreementId, DateTime mEffectiveDate, int OEmployeeLeave_Id, int ComPanyLeave_Id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                
                EmployeeLeave OEmployeeLeave = db.EmployeeLeave
                     
                                  .Include(e => e.EmployeeLvStruct)
                                  .Include(e => e.Employee.EmpOffInfo)
                                  .Include(e => e.Employee.EmpOffInfo.PayScale)
                                  .Where(e => e.Id == OEmployeeLeave_Id).SingleOrDefault();

             

                var OPayScaleAssignment = db.LvAssignment
                       .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                      .Where(p => p.p.PayScaleAgreement.Id == PayScaleAgreementId && p.p.CompanyLeave_Id == ComPanyLeave_Id)
                      .Select(m => new
                      {
                          LvAssignment = m.p, // or m.ppc.pc.ProdId
                          Id = m.p.Id,
                          LvHeadFormula = m.p.LvHeadFormula.Select(t => new LeaveHeadFormulaT
                          {
                              GeoStruct = t.GeoStruct,
                              PayStruct = t.PayStruct,
                              FuncStruct = t.FuncStruct,
                              Name = t.Name,
                              Id = t.Id,
                              LvBankPolicy = t.LvBankPolicy,
                              LvCreditPolicy = t.LvCreditPolicy,
                              LvDebitPolicy = t.LvDebitPolicy,
                              LvEncashPolicy = t.LvEncashPolicy
                          }),
                          LvHead = m.p.LvHead,
                          PolicyName = m.p.PolicyName
                          // other assignments
                      }).ToList();

                Employee OEmployee = db.Employee
                                                   .Include(e => e.EmpOffInfo)
                                                    .Where(e => e.Id == OEmployeeLeave.Employee.Id).AsNoTracking().OrderBy(e => e.Id)
                                                    .SingleOrDefault();

                List<EmployeeLvStructDetails> OEmpLvStructDetails = new List<EmployeeLvStructDetails>();

                EmployeeLvStruct OEmpLvStruct = db.EmployeeLvStruct.Include(e => e.EmployeeLvStructDetails)
                    .Include(e => e.EmployeeLvStructDetails.Select(r => r.LvHead))
                    .Include(e => e.EmployeeLvStructDetails.Select(r => r.PolicyName))
                    .Include(e => e.EmployeeLvStructDetails.Select(r => r.LvHeadFormula))
                     .Include(e => e.EmployeeLvStructDetails.Select(r => r.LvAssignment))
                      .Include(e => e.EmployeeLvStructDetails.Select(r => r.LvAssignment.PayScaleAgreement))
                    .Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.PayStruct).Where(e => e.Id == OEmpLvStructId).FirstOrDefault();//OEmployeePayroll.EmpSalStruct.Where(e => e.EffectiveDate == mEffectiveDate).SingleOrDefault(); 

                OEmpLvStructDetails = OEmpLvStruct.EmployeeLvStructDetails.ToList();

                foreach (var a in OPayScaleAssignment)
                {

                    //if (a.PolicyName.Id == b.PolicyName.Id)
                    //{
                    //Count = 0;
                    List<int> PolicyFormulaIds = new List<int>();
                    if (a.LvHeadFormula.Where(r => r.LvBankPolicy != null && a.PolicyName.LookupVal.ToUpper() == "BANK POLICY").Count() > 0)
                    {
                        var Policyformula = LeaveHeadFinderNew(OEmpLvStruct, a.LvHeadFormula.Where(r => r.LvBankPolicy != null).ToList(), a.PolicyName.Id);
                        if (Policyformula != null)
                        {
                            PolicyFormulaIds.Add(Policyformula.Id);

                            var PolicyFormulaNotInPolicyStructDetails = OEmpLvStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.LvHeadFormula != null ? e.LvHeadFormula.Id : 0) && e.LvHead.Id == a.LvHead.Id).FirstOrDefault();

                            if (PolicyFormulaNotInPolicyStructDetails != null)
                            {
                                PolicyFormulaNotInPolicyStructDetails.LvHeadFormula = Policyformula == null ? null : db.LvHeadFormula.Where(e => e.Id == Policyformula.Id).FirstOrDefault(); 
                                //db.EmployeeLvStructDetails.RemoveRange(PolicyFormulaNotInPolicyStructDetails);
                                //db.SaveChanges();
                                db.EmployeeLvStructDetails.Attach(PolicyFormulaNotInPolicyStructDetails);
                                db.Entry(PolicyFormulaNotInPolicyStructDetails).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            var PolicyFormulaNotInPolicyStructDetails = OEmpLvStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id &&  e.LvHead.Id == a.LvHead.Id).FirstOrDefault();

                            if (PolicyFormulaNotInPolicyStructDetails != null)
                            {
                                PolicyFormulaNotInPolicyStructDetails.LvHeadFormula = Policyformula == null ? null : db.LvHeadFormula.Where(e => e.Id == Policyformula.Id).FirstOrDefault();
                                //db.EmployeeLvStructDetails.RemoveRange(PolicyFormulaNotInPolicyStructDetails);
                                //db.SaveChanges();
                                db.EmployeeLvStructDetails.Attach(PolicyFormulaNotInPolicyStructDetails);
                                db.Entry(PolicyFormulaNotInPolicyStructDetails).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }

                    if (a.LvHeadFormula.Where(r => r.LvCreditPolicy != null && a.PolicyName.LookupVal.ToUpper() == "CREDIT POLICY").Count() > 0)
                    {
                        var Policyformula = LeaveHeadFinderNew(OEmpLvStruct, a.LvHeadFormula.Where(r => r.LvCreditPolicy != null).ToList(), a.PolicyName.Id);
                        if (Policyformula != null)
                        {
                            PolicyFormulaIds.Add(Policyformula.Id);

                            EmployeeLvStructDetails PolicyFormulaNotInPolicyStructDetails = OEmpLvStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.LvHeadFormula != null ? e.LvHeadFormula.Id : 0) && e.LvHead.Id == a.LvHead.Id).FirstOrDefault();

                            if (PolicyFormulaNotInPolicyStructDetails != null)
                            {
                                PolicyFormulaNotInPolicyStructDetails.LvHeadFormula = Policyformula == null ? null : db.LvHeadFormula.Where(e => e.Id == Policyformula.Id).FirstOrDefault(); 
                                //db.EmployeeLvStructDetails.RemoveRange(PolicyFormulaNotInPolicyStructDetails);
                                //db.SaveChanges();
                                db.EmployeeLvStructDetails.Attach(PolicyFormulaNotInPolicyStructDetails);
                                db.Entry(PolicyFormulaNotInPolicyStructDetails).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            //PolicyFormulaIds.Add(Policyformula.Id);

                            EmployeeLvStructDetails PolicyFormulaNotInPolicyStructDetails = OEmpLvStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id && e.LvHead.Id == a.LvHead.Id).FirstOrDefault();

                            if (PolicyFormulaNotInPolicyStructDetails != null)
                            {
                                PolicyFormulaNotInPolicyStructDetails.LvHeadFormula = Policyformula == null ? null : db.LvHeadFormula.Where(e => e.Id == Policyformula.Id).FirstOrDefault();
                                //db.EmployeeLvStructDetails.RemoveRange(PolicyFormulaNotInPolicyStructDetails);
                                //db.SaveChanges();
                                db.EmployeeLvStructDetails.Attach(PolicyFormulaNotInPolicyStructDetails);
                                db.Entry(PolicyFormulaNotInPolicyStructDetails).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }

                    if (a.LvHeadFormula.Where(r => r.LvDebitPolicy != null && a.PolicyName.LookupVal.ToUpper() == "DEBIT POLICY").Count() > 0)
                    {
                        var Policyformula = LeaveHeadFinderNew(OEmpLvStruct, a.LvHeadFormula.Where(r => r.LvDebitPolicy != null).ToList(), a.PolicyName.Id);
                        if (Policyformula != null)
                        {
                            PolicyFormulaIds.Add(Policyformula.Id);

                            EmployeeLvStructDetails PolicyFormulaNotInPolicyStructDetails = OEmpLvStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.LvHeadFormula != null ? e.LvHeadFormula.Id : 0) && e.LvHead.Id == a.LvHead.Id).FirstOrDefault();
                           

                            if (PolicyFormulaNotInPolicyStructDetails != null)
                            {
                                PolicyFormulaNotInPolicyStructDetails.LvHeadFormula = Policyformula == null ? null : db.LvHeadFormula.Where(e => e.Id == Policyformula.Id).FirstOrDefault(); 
                                //db.EmployeeLvStructDetails.RemoveRange(PolicyFormulaNotInPolicyStructDetails);
                                //db.SaveChanges();
                                db.EmployeeLvStructDetails.Attach(PolicyFormulaNotInPolicyStructDetails);
                                db.Entry(PolicyFormulaNotInPolicyStructDetails).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            EmployeeLvStructDetails PolicyFormulaNotInPolicyStructDetails = OEmpLvStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id && e.LvHead.Id == a.LvHead.Id).FirstOrDefault();


                            if (PolicyFormulaNotInPolicyStructDetails != null)
                            {
                                PolicyFormulaNotInPolicyStructDetails.LvHeadFormula = Policyformula == null ? null : db.LvHeadFormula.Where(e => e.Id == Policyformula.Id).FirstOrDefault();
                                //db.EmployeeLvStructDetails.RemoveRange(PolicyFormulaNotInPolicyStructDetails);
                                //db.SaveChanges();
                                db.EmployeeLvStructDetails.Attach(PolicyFormulaNotInPolicyStructDetails);
                                db.Entry(PolicyFormulaNotInPolicyStructDetails).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }

                        }
                    }

                    if (a.LvHeadFormula.Where(r => r.LvEncashPolicy != null && a.PolicyName.LookupVal.ToUpper() == "ENCASHMENT POLICY").Count() > 0)
                    {
                        var Policyformula = LeaveHeadFinderNew(OEmpLvStruct, a.LvHeadFormula.Where(r => r.LvEncashPolicy != null).ToList(), a.PolicyName.Id);
                        if (Policyformula != null)
                        {
                            PolicyFormulaIds.Add(Policyformula.Id);


                            EmployeeLvStructDetails PolicyFormulaNotInPolicyStructDetails = OEmpLvStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.LvHeadFormula != null ? e.LvHeadFormula.Id : 0) && e.LvHead.Id == a.LvHead.Id).FirstOrDefault();

                            if (PolicyFormulaNotInPolicyStructDetails != null)
                            {
                                PolicyFormulaNotInPolicyStructDetails.LvHeadFormula = Policyformula == null ? null : db.LvHeadFormula.Where(e => e.Id == Policyformula.Id).FirstOrDefault(); 
                                //db.EmployeeLvStructDetails.RemoveRange(PolicyFormulaNotInPolicyStructDetails);
                                //db.SaveChanges();
                                db.EmployeeLvStructDetails.Attach(PolicyFormulaNotInPolicyStructDetails);
                                db.Entry(PolicyFormulaNotInPolicyStructDetails).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            EmployeeLvStructDetails PolicyFormulaNotInPolicyStructDetails = OEmpLvStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id  && e.LvHead.Id == a.LvHead.Id).FirstOrDefault();

                            if (PolicyFormulaNotInPolicyStructDetails != null)
                            {
                                PolicyFormulaNotInPolicyStructDetails.LvHeadFormula = Policyformula == null ? null : db.LvHeadFormula.Where(e => e.Id == Policyformula.Id).FirstOrDefault();
                                //db.EmployeeLvStructDetails.RemoveRange(PolicyFormulaNotInPolicyStructDetails);
                                //db.SaveChanges();
                                db.EmployeeLvStructDetails.Attach(PolicyFormulaNotInPolicyStructDetails);
                                db.Entry(PolicyFormulaNotInPolicyStructDetails).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }


                }
            }

        }

    }
}