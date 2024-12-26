using P2b.Global;
using EssPortal.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Payroll;
using EssPortal.Security;
using System.IO;
using EssPortal.Models;
using System.Diagnostics;
using System.Transactions;


namespace EssPortal.Process
{
    public static class SalaryHeadGenProcess
    {
        public static SalHeadFormula SalFormulaFinder(EmpSalStruct OEmpSalstruct, int OSalaryHeadID)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string mFormulaName = "";

                var OEmployeeSalStruct = db.EmpSalStruct.Where(t => t.Id == OEmpSalstruct.Id).SingleOrDefault();
                GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OEmployeeSalStruct.GeoStruct_Id).SingleOrDefault();
                OEmployeeSalStruct.GeoStruct = GeoStruct;
                PayStruct PayStruct = db.PayStruct.Where(e => e.Id == OEmployeeSalStruct.PayStruct_Id).SingleOrDefault();
                OEmployeeSalStruct.PayStruct = PayStruct;
                FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == OEmployeeSalStruct.FuncStruct_Id).SingleOrDefault();
                OEmployeeSalStruct.FuncStruct = FuncStruct;
                List<EmpSalStructDetails> EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == OEmployeeSalStruct.Id).ToList();
                OEmployeeSalStruct.EmpSalStructDetails = EmpSalStructDetails;
                foreach (var EmpSalStructDetailsitem in EmpSalStructDetails)
                {
                    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsitem.SalaryHead_Id).SingleOrDefault();
                    EmpSalStructDetailsitem.SalaryHead = SalaryHead;
                    PayScaleAssignment PayScaleAssignment = db.PayScaleAssignment.Include(e => e.SalHeadFormula).Where(e => e.Id == EmpSalStructDetailsitem.PayScaleAssignment_Id).SingleOrDefault();
                    EmpSalStructDetailsitem.PayScaleAssignment = PayScaleAssignment;

                    EmpSalStructDetailsitem.PayScaleAssignment.SalHeadFormula = PayScaleAssignment.SalHeadFormula;
                    foreach (var SalHeadFormulaitem in PayScaleAssignment.SalHeadFormula)
                    {
                        GeoStruct GeoStruct1 = db.GeoStruct.Where(e => e.Id == SalHeadFormulaitem.GeoStruct_Id).SingleOrDefault();
                        SalHeadFormulaitem.GeoStruct = GeoStruct1;
                        PayStruct PayStruct1 = db.PayStruct.Where(e => e.Id == SalHeadFormulaitem.PayStruct_Id).SingleOrDefault();
                        SalHeadFormulaitem.PayStruct = PayStruct1;
                        FuncStruct FuncStruct1 = db.FuncStruct.Where(e => e.Id == SalHeadFormulaitem.FuncStruct_Id).SingleOrDefault();
                        SalHeadFormulaitem.FuncStruct = FuncStruct1;

                    }

                }



                var OSalHeadFormulaGeo = OEmployeeSalStruct.EmpSalStructDetails
              .Where(e => e.SalaryHead.Id == OSalaryHeadID)
              .Select(e => e.PayScaleAssignment.SalHeadFormula
                  .Where(r => r.GeoStruct != null)).SingleOrDefault();

                if (OSalHeadFormulaGeo != null)
                    OSalHeadFormulaGeo = OSalHeadFormulaGeo.Where(e => e.GeoStruct.Id == OEmployeeSalStruct.GeoStruct.Id);

                var OSalHeadFormulaPay = OSalHeadFormulaGeo;
                OSalHeadFormulaPay = null;

                var OSalHeadFormula = OSalHeadFormulaGeo;
                if (OSalHeadFormulaGeo.SingleOrDefault() != null && OSalHeadFormulaGeo.Count() > 0)
                {
                    mFormulaName = OSalHeadFormulaGeo.Select(e => e.Name).FirstOrDefault();
                    OSalHeadFormulaPay = OEmployeeSalStruct.EmpSalStructDetails
                                  .Where(e => e.SalaryHead.Id == OSalaryHeadID)
                                  .Select(e => e.PayScaleAssignment.SalHeadFormula
                                         .Where(r => r.PayStruct != null && OEmployeeSalStruct.PayStruct != null && r.Name == mFormulaName)).SingleOrDefault();
                    if (OSalHeadFormulaPay != null || OSalHeadFormulaPay.Count() > 0)
                    {

                        OSalHeadFormulaPay = OSalHeadFormulaPay.Where(e => e.PayStruct.Id == OEmployeeSalStruct.PayStruct.Id);
                    }
                }
                else
                {
                    OSalHeadFormulaPay = OEmployeeSalStruct.EmpSalStructDetails
                                  .Where(e => e.SalaryHead.Id == OSalaryHeadID)
                                  .Select(e => e.PayScaleAssignment.SalHeadFormula
                                         .Where(r => (r.PayStruct != null && OEmployeeSalStruct.PayStruct != null))).SingleOrDefault();

                    if (OSalHeadFormulaPay != null || OSalHeadFormulaPay.Count() > 0)
                    {
                        OSalHeadFormulaPay = OSalHeadFormulaPay.Where(e => e.PayStruct.Id == OEmployeeSalStruct.PayStruct.Id);
                    }
                }

                var OSalHeadFormulaFunc = OSalHeadFormulaGeo;
                OSalHeadFormulaFunc = null;

                if (OSalHeadFormula.SingleOrDefault() == null)
                {
                    OSalHeadFormulaFunc = OEmployeeSalStruct.EmpSalStructDetails
                                .Where(e => e.SalaryHead.Id == OSalaryHeadID)
                                   .Select(e => e.PayScaleAssignment.SalHeadFormula
                  .Where(r => (r.FuncStruct != null && OEmployeeSalStruct.FuncStruct != null))
                    ).SingleOrDefault();

                    if (OSalHeadFormulaFunc != null || OSalHeadFormulaFunc.Count() > 0)
                    {
                        OSalHeadFormulaFunc = OSalHeadFormulaFunc.Where(e => e.FuncStruct.Id == OEmployeeSalStruct.FuncStruct.Id);
                    }
                }
                else
                {
                    mFormulaName = OSalHeadFormula.Select(e => e.Name).FirstOrDefault();
                    OSalHeadFormulaFunc = OEmployeeSalStruct.EmpSalStructDetails
                                 .Where(e => e.SalaryHead.Id == OSalaryHeadID)
                                 .Select(e => e.PayScaleAssignment.SalHeadFormula
                                        .Where(r => r.FuncStruct != null && r.Name == mFormulaName)).SingleOrDefault();

                    if (OSalHeadFormulaFunc != null || OSalHeadFormulaFunc.Count() > 0)
                    {
                        OSalHeadFormulaFunc = OSalHeadFormulaFunc.Where(r => r.FuncStruct.Id == OEmployeeSalStruct.FuncStruct.Id);
                    }
                }

                if (OSalHeadFormulaPay.SingleOrDefault() != null && OSalHeadFormulaFunc.SingleOrDefault() != null)
                    if (OSalHeadFormulaPay.Count() > 0 && OSalHeadFormulaFunc.Count() > 0)
                        OSalHeadFormula = OSalHeadFormulaFunc;

                if (OSalHeadFormulaPay.SingleOrDefault() != null && OSalHeadFormulaFunc.SingleOrDefault() == null)
                    if (OSalHeadFormulaPay.Count() > 0 && OSalHeadFormulaFunc.Count() == 0)
                        OSalHeadFormula = OSalHeadFormulaPay;

                if (OSalHeadFormulaPay.SingleOrDefault() == null && OSalHeadFormulaFunc.SingleOrDefault() != null)
                    if (OSalHeadFormulaPay.Count() == 0 && OSalHeadFormulaFunc.Count() > 0)
                        OSalHeadFormula = OSalHeadFormulaFunc;
                return OSalHeadFormula.SingleOrDefault();
            }
        }

        public static SalHeadFormulaT SalFormulaFinderNewTest(EmpSalStruct OEmpSalstruct, List<SalHeadFormulaT> OPayScaleAssignment, int OSalaryHeadID)
        {
            var OEmployeeSalStruct = OEmpSalstruct;
            SalHeadFormulaT OSalHeadFormula = null;
            SalHeadFormulaT OSalHeadFormulaGeo = null;
            SalHeadFormulaT OSalHeadFormulaPay = null;
            SalHeadFormulaT OSalHeadFormulaFunc = null;
            SalHeadFormulaT OSalHeadFormulaGeopay = null;
            SalHeadFormulaT OSalHeadFormulaGeofun = null;
            SalHeadFormulaT OSalHeadFormulaGeopayfun = null;

            OSalHeadFormulaGeo = OPayScaleAssignment
              .Where(r => r.GeoStruct != null && OEmployeeSalStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeSalStruct.GeoStruct.Id && r.FuncStruct == null && r.PayStruct == null).FirstOrDefault();

            OSalHeadFormula = OSalHeadFormulaGeo;


            if (OSalHeadFormula == null)
            {
                OSalHeadFormulaPay = OPayScaleAssignment
              .Where(r => r.PayStruct != null && OEmployeeSalStruct.PayStruct != null && r.PayStruct.Id == OEmployeeSalStruct.PayStruct.Id && r.FuncStruct == null && r.GeoStruct == null).FirstOrDefault();

            }
            else
            {
                OSalHeadFormulaPay = OPayScaleAssignment
              .Where(r => r.Name == OSalHeadFormula.Name && r.PayStruct != null && OEmployeeSalStruct.PayStruct != null && r.PayStruct.Id == OEmployeeSalStruct.PayStruct.Id && r.FuncStruct == null && r.GeoStruct == null).FirstOrDefault();

            }

            if (OSalHeadFormulaPay != null)
            {
                OSalHeadFormula = OSalHeadFormulaPay;
            }
            if (OSalHeadFormula == null)
            {
                OSalHeadFormulaFunc = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeeSalStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeSalStruct.FuncStruct.Id && r.PayStruct == null && r.GeoStruct == null).FirstOrDefault();

            }
            else
            {
                OSalHeadFormulaFunc = OPayScaleAssignment
              .Where(r => r.Name == OSalHeadFormula.Name && r.FuncStruct != null && OEmployeeSalStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeSalStruct.FuncStruct.Id && r.PayStruct == null && r.GeoStruct == null).FirstOrDefault();

            }

            if (OSalHeadFormulaFunc != null)
            {
                OSalHeadFormula = OSalHeadFormulaFunc;
            }

            OSalHeadFormulaGeopay = OPayScaleAssignment
                .Where(r => r.PayStruct != null && OEmployeeSalStruct.PayStruct != null && r.PayStruct.Id == OEmployeeSalStruct.PayStruct.Id && r.GeoStruct != null && OEmployeeSalStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeSalStruct.GeoStruct.Id && r.FuncStruct == null).FirstOrDefault();

            if (OSalHeadFormulaGeopay != null)
            {
                OSalHeadFormula = OSalHeadFormulaGeopay;
            }

            OSalHeadFormulaGeofun = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeeSalStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeSalStruct.FuncStruct.Id && r.GeoStruct != null && OEmployeeSalStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeSalStruct.GeoStruct.Id && r.PayStruct == null).FirstOrDefault();

            if (OSalHeadFormulaGeofun != null)
            {
                OSalHeadFormula = OSalHeadFormulaGeofun;
            }

            OSalHeadFormulaGeopayfun = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeeSalStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeSalStruct.FuncStruct.Id && r.GeoStruct != null && OEmployeeSalStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeSalStruct.GeoStruct.Id && r.PayStruct != null && OEmployeeSalStruct.PayStruct != null && r.PayStruct.Id == OEmployeeSalStruct.PayStruct.Id && r.FormulaType.LookupVal.ToUpper() == "STANDARDFORMULA").FirstOrDefault();

            if (OSalHeadFormulaGeopayfun != null)
            {
                OSalHeadFormula = OSalHeadFormulaGeopayfun;
            }


            return OSalHeadFormula;
        }//return salheadformula
        public static SalHeadFormula SalFormulaFinderNew(EmpSalStruct OEmpSalstruct, PayScaleAssignment OPayScaleAssignment, int OSalaryHeadID)
        {
            var OEmployeeSalStruct = OEmpSalstruct;
            SalHeadFormula OSalHeadFormula = null;
            SalHeadFormula OSalHeadFormulaGeo = null;
            SalHeadFormula OSalHeadFormulaPay = null;
            SalHeadFormula OSalHeadFormulaFunc = null;
            SalHeadFormula OSalHeadFormulaGeopay = null;
            SalHeadFormula OSalHeadFormulaGeofun = null;
            SalHeadFormula OSalHeadFormulaGeopayfun = null;

            OSalHeadFormulaGeo = OPayScaleAssignment.SalHeadFormula
              .Where(r => r.GeoStruct != null && OEmployeeSalStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeSalStruct.GeoStruct.Id && r.FuncStruct == null && r.PayStruct == null).FirstOrDefault();

            OSalHeadFormula = OSalHeadFormulaGeo;


            if (OSalHeadFormula == null)
            {
                OSalHeadFormulaPay = OPayScaleAssignment.SalHeadFormula
              .Where(r => r.PayStruct != null && OEmployeeSalStruct.PayStruct != null && r.PayStruct.Id == OEmployeeSalStruct.PayStruct.Id && r.FuncStruct == null && r.GeoStruct == null).FirstOrDefault();

            }
            else
            {
                OSalHeadFormulaPay = OPayScaleAssignment.SalHeadFormula
              .Where(r => r.Name == OSalHeadFormula.Name && r.PayStruct != null && OEmployeeSalStruct.PayStruct != null && r.PayStruct.Id == OEmployeeSalStruct.PayStruct.Id && r.FuncStruct == null && r.GeoStruct == null).FirstOrDefault();

            }

            if (OSalHeadFormulaPay != null)
            {
                OSalHeadFormula = OSalHeadFormulaPay;
            }
            if (OSalHeadFormula == null)
            {
                OSalHeadFormulaFunc = OPayScaleAssignment.SalHeadFormula
              .Where(r => r.FuncStruct != null && OEmployeeSalStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeSalStruct.FuncStruct.Id && r.PayStruct == null && r.GeoStruct == null).FirstOrDefault();

            }
            else
            {
                OSalHeadFormulaFunc = OPayScaleAssignment.SalHeadFormula
              .Where(r => r.Name == OSalHeadFormula.Name && r.FuncStruct != null && OEmployeeSalStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeSalStruct.FuncStruct.Id && r.PayStruct == null && r.GeoStruct == null).FirstOrDefault();

            }

            if (OSalHeadFormulaFunc != null)
            {
                OSalHeadFormula = OSalHeadFormulaFunc;
            }

            OSalHeadFormulaGeopay = OPayScaleAssignment.SalHeadFormula
                .Where(r => r.PayStruct != null && OEmployeeSalStruct.PayStruct != null && r.PayStruct.Id == OEmployeeSalStruct.PayStruct.Id && r.GeoStruct != null && OEmployeeSalStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeSalStruct.GeoStruct.Id && r.FuncStruct == null).FirstOrDefault();

            if (OSalHeadFormulaGeopay != null)
            {
                OSalHeadFormula = OSalHeadFormulaGeopay;
            }

            OSalHeadFormulaGeofun = OPayScaleAssignment.SalHeadFormula
              .Where(r => r.FuncStruct != null && OEmployeeSalStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeSalStruct.FuncStruct.Id && r.GeoStruct != null && OEmployeeSalStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeSalStruct.GeoStruct.Id && r.PayStruct == null).FirstOrDefault();

            if (OSalHeadFormulaGeofun != null)
            {
                OSalHeadFormula = OSalHeadFormulaGeofun;
            }

            OSalHeadFormulaGeopayfun = OPayScaleAssignment.SalHeadFormula
              .Where(r => r.FuncStruct != null && OEmployeeSalStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeSalStruct.FuncStruct.Id && r.GeoStruct != null && OEmployeeSalStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeSalStruct.GeoStruct.Id && r.PayStruct != null && OEmployeeSalStruct.PayStruct != null && r.PayStruct.Id == OEmployeeSalStruct.PayStruct.Id).FirstOrDefault();

            if (OSalHeadFormulaGeopayfun != null)
            {
                OSalHeadFormula = OSalHeadFormulaGeopayfun;
            }


            return OSalHeadFormula;
        }//return salheadformula
        public static SalHeadFormula SalFormulaFinderNewNonStandard(EmpSalStruct OEmpSalstruct, PayScaleAssignment OPayScaleAssignment, int OSalaryHeadID)
        {
            var OEmployeeSalStruct = OEmpSalstruct;
            SalHeadFormula OSalHeadFormula = null;
            SalHeadFormula OSalHeadFormulaGeo = null;
            SalHeadFormula OSalHeadFormulaPay = null;
            SalHeadFormula OSalHeadFormulaFunc = null;
            SalHeadFormula OSalHeadFormulaGeopay = null;
            SalHeadFormula OSalHeadFormulaGeofun = null;
            SalHeadFormula OSalHeadFormulaGeopayfun = null;

            OSalHeadFormulaGeo = OPayScaleAssignment.SalHeadFormula
              .Where(r => r.GeoStruct != null && OEmployeeSalStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeSalStruct.GeoStruct.Id && r.FuncStruct == null && r.PayStruct == null && r.FormulaType.LookupVal.ToUpper() == "NONSTANDARDFORMULA").FirstOrDefault();

            OSalHeadFormula = OSalHeadFormulaGeo;


            if (OSalHeadFormula == null)
            {
                OSalHeadFormulaPay = OPayScaleAssignment.SalHeadFormula
              .Where(r => r.PayStruct != null && OEmployeeSalStruct.PayStruct != null && r.PayStruct.Id == OEmployeeSalStruct.PayStruct.Id && r.FuncStruct == null && r.GeoStruct == null && r.FormulaType.LookupVal.ToUpper() == "NONSTANDARDFORMULA").FirstOrDefault();

            }
            else
            {
                OSalHeadFormulaPay = OPayScaleAssignment.SalHeadFormula
              .Where(r => r.Name == OSalHeadFormula.Name && r.PayStruct != null && OEmployeeSalStruct.PayStruct != null && r.PayStruct.Id == OEmployeeSalStruct.PayStruct.Id && r.FuncStruct == null && r.GeoStruct == null && r.FormulaType.LookupVal.ToUpper() == "NONSTANDARDFORMULA").FirstOrDefault();

            }

            if (OSalHeadFormulaPay != null)
            {
                OSalHeadFormula = OSalHeadFormulaPay;
            }
            if (OSalHeadFormula == null)
            {
                OSalHeadFormulaFunc = OPayScaleAssignment.SalHeadFormula
              .Where(r => r.FuncStruct != null && OEmployeeSalStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeSalStruct.FuncStruct.Id && r.PayStruct == null && r.GeoStruct == null && r.FormulaType.LookupVal.ToUpper() == "NONSTANDARDFORMULA").FirstOrDefault();

            }
            else
            {
                OSalHeadFormulaFunc = OPayScaleAssignment.SalHeadFormula
              .Where(r => r.Name == OSalHeadFormula.Name && r.FuncStruct != null && OEmployeeSalStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeSalStruct.FuncStruct.Id && r.PayStruct == null && r.GeoStruct == null && r.FormulaType.LookupVal.ToUpper() == "NONSTANDARDFORMULA").FirstOrDefault();

            }

            if (OSalHeadFormulaFunc != null)
            {
                OSalHeadFormula = OSalHeadFormulaFunc;
            }

            OSalHeadFormulaGeopay = OPayScaleAssignment.SalHeadFormula
                .Where(r => r.PayStruct != null && OEmployeeSalStruct.PayStruct != null && r.PayStruct.Id == OEmployeeSalStruct.PayStruct.Id && r.GeoStruct != null && OEmployeeSalStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeSalStruct.GeoStruct.Id && r.FuncStruct == null && r.FormulaType.LookupVal.ToUpper() == "NONSTANDARDFORMULA").FirstOrDefault();

            if (OSalHeadFormulaGeopay != null)
            {
                OSalHeadFormula = OSalHeadFormulaGeopay;
            }

            OSalHeadFormulaGeofun = OPayScaleAssignment.SalHeadFormula
              .Where(r => r.FuncStruct != null && OEmployeeSalStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeSalStruct.FuncStruct.Id && r.GeoStruct != null && OEmployeeSalStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeSalStruct.GeoStruct.Id && r.PayStruct == null && r.FormulaType.LookupVal.ToUpper() == "NONSTANDARDFORMULA").FirstOrDefault();

            if (OSalHeadFormulaGeofun != null)
            {
                OSalHeadFormula = OSalHeadFormulaGeofun;
            }

            OSalHeadFormulaGeopayfun = OPayScaleAssignment.SalHeadFormula
              .Where(r => r.FuncStruct != null && OEmployeeSalStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeSalStruct.FuncStruct.Id && r.GeoStruct != null && OEmployeeSalStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeSalStruct.GeoStruct.Id && r.PayStruct != null && OEmployeeSalStruct.PayStruct != null && r.PayStruct.Id == OEmployeeSalStruct.PayStruct.Id && r.FormulaType.LookupVal.ToUpper() == "STANDARDFORMULA").FirstOrDefault();

            if (OSalHeadFormulaGeopayfun != null)
            {
                OSalHeadFormula = OSalHeadFormulaGeopayfun;
            }


            return OSalHeadFormula;
        }//return salheadformula

        public static double WagecalcDirect(Wages OWagesMaster, List<SalEarnDedT> OSalaryEarnDedT, List<EmpSalStructDetails> OEmpSalStructDetails)
        {
            double OWages = 0;
            if (OSalaryEarnDedT != null)
            {
                OWages = OWagesMaster.RateMaster
                    .Join(OSalaryEarnDedT, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                            (u, uir) => new { u, uir })
                    .Select(e => e.u.Percentage / 100 * e.uir.Amount).Sum();

                OWages = OWages + OWagesMaster.RateMaster
                    .Join(OSalaryEarnDedT, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                            (u, uir) => new { u, uir })
                    .Select(e => e.u.Amount).Sum();
            }
            else if (OEmpSalStructDetails != null)
            {
                OWages = OWagesMaster.RateMaster
                    .Join(OEmpSalStructDetails, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                            (u, uir) => new { u, uir })
                    .Select(e => e.u.Percentage / 100 * e.uir.Amount).Sum();

                OWages = OWages + OWagesMaster.RateMaster
                    .Join(OEmpSalStructDetails, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                            (u, uir) => new { u, uir })
                    .Select(e => e.u.Amount).Sum();
            }
            if (OWagesMaster.CeilingMin != null)
            {
                if (OWages < OWagesMaster.CeilingMin)
                {
                    OWages = OWagesMaster.CeilingMin;
                }
            }
            if (OWagesMaster.CeilingMax != null)
            {
                if (OWages > OWagesMaster.CeilingMax)
                {
                    OWages = OWagesMaster.CeilingMax;
                }
            }
            return OWages;
        }

        public static double Wagecalc(SalHeadFormula OSalHeadFormula, List<SalEarnDedT> OSalaryDetails, List<EmpSalStructDetails> OEmpSalStructDetails)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var OSalHeadFormualQuery = db.SalHeadFormula.Where(e => e.Id == OSalHeadFormula.Id)
                    .Include(e => e.SalWages)
                    .Include(e => e.SalWages.RateMaster).Include(e => e.SalWages.RateMaster.Select(r => r.SalHead)).FirstOrDefault();

                double OWages = 0;
                if (OSalHeadFormualQuery.SalWages != null)
                {
                    if (OSalaryDetails != null)
                    {
                        OWages = OSalHeadFormualQuery.SalWages.RateMaster
                            .Join(OSalaryDetails, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                    (u, uir) => new { u, uir })
                            .Select(e => (e.u.Percentage / 100) * e.uir.Amount).Sum();
                        OWages = OWages + OSalHeadFormualQuery.SalWages.RateMaster
                            .Join(OSalaryDetails, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                    (u, uir) => new { u, uir })
                            .Select(e => e.u.Amount).Sum();
                        OWages = (OWages * OSalHeadFormualQuery.SalWages.Percentage) / 100;

                        return OWages;
                    }
                    else if (OEmpSalStructDetails != null)
                    {
                        OWages = OSalHeadFormualQuery.SalWages.RateMaster
                            .Join(OEmpSalStructDetails, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                    (u, uir) => new { u, uir })
                            .Select(e => (e.u.Percentage / 100) * e.uir.Amount).Sum();
                        OWages = OWages + OSalHeadFormualQuery.SalWages.RateMaster
                            .Join(OEmpSalStructDetails, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                    (u, uir) => new { u, uir })
                            .Select(e => e.u.Amount).Sum();
                        OWages = (OWages * OSalHeadFormualQuery.SalWages.Percentage) / 100;
                        return OWages;
                    }
                    else
                        return 0;
                }
                else
                    return 0;
            }
        }
        public static double WagecalcNonstd(SalHeadFormula OSalHeadFormula, List<SalEarnDedT> OSalaryDetails, List<EmpSalStructDetails> OEmpSalStructDetails)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var OSalHeadFormualQuery = db.SalHeadFormula.Where(e => e.Id == OSalHeadFormula.Id)
                    .Include(e => e.NonStandardSalWages)
                    .Include(e => e.NonStandardSalWages.RateMaster).Include(e => e.NonStandardSalWages.RateMaster.Select(r => r.SalHead)).FirstOrDefault();

                double OWages = 0;
                if (OSalHeadFormualQuery.NonStandardSalWages != null)
                {
                    if (OSalaryDetails != null)
                    {
                        OWages = OSalHeadFormualQuery.NonStandardSalWages.RateMaster
                            .Join(OSalaryDetails, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                    (u, uir) => new { u, uir })
                            .Select(e => (e.u.Percentage / 100) * e.uir.Amount).Sum();
                        OWages = OWages + OSalHeadFormualQuery.NonStandardSalWages.RateMaster
                            .Join(OSalaryDetails, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                    (u, uir) => new { u, uir })
                            .Select(e => e.u.Amount).Sum();
                        OWages = (OWages * OSalHeadFormualQuery.NonStandardSalWages.Percentage) / 100;

                        return OWages;
                    }
                    else if (OEmpSalStructDetails != null)
                    {
                        OWages = OSalHeadFormualQuery.NonStandardSalWages.RateMaster
                            .Join(OEmpSalStructDetails, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                    (u, uir) => new { u, uir })
                            .Select(e => (e.u.Percentage / 100) * e.uir.Amount).Sum();
                        OWages = OWages + OSalHeadFormualQuery.NonStandardSalWages.RateMaster
                            .Join(OEmpSalStructDetails, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                    (u, uir) => new { u, uir })
                            .Select(e => e.u.Amount).Sum();
                        OWages = (OWages * OSalHeadFormualQuery.NonStandardSalWages.Percentage) / 100;
                        return OWages;
                    }
                    else
                        return 0;
                }
                else
                    return 0;
            }
        }
        //Celling check
        public static double CellingCkeck(SalHeadFormula OSalHeadFormula, double mSalHeadAmount)
        {

            if (OSalHeadFormula.CeilingMin != null)
            {
                if (mSalHeadAmount < OSalHeadFormula.CeilingMin)
                {
                    mSalHeadAmount = OSalHeadFormula.CeilingMin;
                }
            }
            if (OSalHeadFormula.CeilingMax != null)
            {
                if (mSalHeadAmount > OSalHeadFormula.CeilingMax)
                {
                    mSalHeadAmount = OSalHeadFormula.CeilingMax;
                }
            }
            return mSalHeadAmount;
        }
        public static SalHeadFormula _returnSalHeadFormula(Int32 OSalHeadFormula)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SalHeadFormula OSalHeadFormualQuery = db.SalHeadFormula
                      .Where(e => e.Id == OSalHeadFormula).SingleOrDefault();
                Wages SalWages = db.Wages.Include(e => e.RateMaster).Where(e => e.Id == OSalHeadFormualQuery.SalWages_Id).SingleOrDefault();
                OSalHeadFormualQuery.SalWages = SalWages;
                //SalWages.RateMaster = SalWages.RateMaster;
                Wages NonStandardSalWages = db.Wages.Include(e => e.RateMaster).Where(e => e.Id == OSalHeadFormualQuery.NonStandardSalWages_Id).SingleOrDefault();
                OSalHeadFormualQuery.NonStandardSalWages = NonStandardSalWages;
                //NonStandardSalWages.RateMaster = NonStandardSalWages.RateMaster;
                SlabDependRule SlabDependRule = db.SlabDependRule.Include(e => e.WageRange).Where(e => e.Id == OSalHeadFormualQuery.SlabDependRule_Id).SingleOrDefault();
                OSalHeadFormualQuery.SlabDependRule = SlabDependRule;
                //SlabDependRule.WageRange = SlabDependRule.WageRange;
                ServiceDependRule ServiceDependRule = db.ServiceDependRule.Include(e => e.ServiceRange).Where(e => e.Id == OSalHeadFormualQuery.ServiceDependRule_Id).SingleOrDefault();
                OSalHeadFormualQuery.ServiceDependRule = ServiceDependRule;
                //ServiceDependRule.ServiceRange = ServiceDependRule.ServiceRange;
                AmountDependRule AmountDependRule = db.AmountDependRule.Where(e => e.Id == OSalHeadFormualQuery.AmountDependRule_Id).SingleOrDefault();
                OSalHeadFormualQuery.AmountDependRule = AmountDependRule;
                PercentDependRule PercentDependRule = db.PercentDependRule.Where(e => e.Id == OSalHeadFormualQuery.PercentDependRule_Id).SingleOrDefault();
                OSalHeadFormualQuery.PercentDependRule = PercentDependRule;
                VDADependRule VDADependRule = db.VDADependRule.Where(e => e.Id == OSalHeadFormualQuery.VDADependRule_Id).SingleOrDefault();
                OSalHeadFormualQuery.VDADependRule = VDADependRule;
                if (VDADependRule != null)
                {
                    CPIRule CPIRule = db.CPIRule.Include(e => e.CPIRuleDetails).Include(e => e.CPIUnitCalc).Where(e => e.Id == VDADependRule.CPIRule_Id).SingleOrDefault();
                    VDADependRule.CPIRule = CPIRule;
                    //VDADependRule.CPIRule.CPIRuleDetails = CPIRule.CPIRuleDetails;
                    //VDADependRule.CPIRule.CPIUnitCalc = CPIRule.CPIUnitCalc;
                    foreach (var CPIRuleDetailsitem in CPIRule.CPIRuleDetails)
                    {
                        Wages CPIWages = db.Wages.Where(e => e.Id == CPIRuleDetailsitem.CPIWages_Id).SingleOrDefault();
                        CPIRuleDetailsitem.CPIWages = CPIWages;
                    }
                }

                return OSalHeadFormualQuery;
            }
        }
        //apply Salheadformula to calculate amount
        public class Vpfclass
        {
            public double? VPFAmt { get; set; }
            public double? VPFPerc { get; set; }
        }

        public static double SalHeadAmountCalc(Int32 OSalHeadFormula, List<SalEarnDedT> OSalaryDetails, List<EmpSalStructDetails> OEmpSalStructDetails,
                                               EmployeePayroll OEmployeePayroll, string mCPIMonth, bool VPFApplicable = false)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var OSalHeadFormualQuery = _returnSalHeadFormula(OSalHeadFormula);

                double mSalWages = 0;
                double mSalWagesnonstd = 0;// for MSC bank CCA
                double mSalHeadAmount = 0;
                var _Empoffdetils = new Vpfclass();
                //var VPFData = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.Employee).Include(e => e.Employee.EmpOffInfo)
                //        .Select(e => new Vpfclass
                //        {
                //            VPFAmt = e.Employee.EmpOffInfo.VPFAmt != 0 ?
                //            e.Employee.EmpOffInfo.VPFAmt : 0,
                //            VPFPerc = e.Employee.EmpOffInfo.VPFPerc != 0 ?
                //                e.Employee.EmpOffInfo.VPFPerc : 0
                //        }).FirstOrDefault();

                var VPFData = db.Employee.Where(e => e.Id == OEmployeePayroll.Employee_Id)
                        .Select(e => new Vpfclass
                        {
                            VPFAmt = e.EmpOffInfo.VPFAmt != 0 ?
                            e.EmpOffInfo.VPFAmt : 0,
                            VPFPerc = e.EmpOffInfo.VPFPerc != 0 ?
                                e.EmpOffInfo.VPFPerc : 0
                        }).FirstOrDefault();


                if (VPFApplicable)
                {
                    //var _Empoffdetils=

                    if (VPFData.VPFAmt == 0 && OSalHeadFormualQuery.AmountDependRule != null)
                    {
                        _Empoffdetils.VPFAmt = OSalHeadFormualQuery.AmountDependRule.SalAmount;
                    }
                    if (VPFData.VPFPerc == 0 && OSalHeadFormualQuery.PercentDependRule != null)
                    {
                        _Empoffdetils.VPFPerc = OSalHeadFormualQuery.PercentDependRule.SalPercent;
                    }
                }
                if (VPFApplicable)
                {
                    if (VPFData.VPFAmt != 0.0 && VPFData.VPFAmt != null)
                    {
                        _Empoffdetils.VPFAmt = VPFData.VPFAmt;
                        return Convert.ToDouble(VPFData.VPFAmt);
                    }
                    if (VPFData.VPFPerc != 0.0 && VPFData.VPFPerc != null)
                    {
                        _Empoffdetils.VPFPerc = VPFData.VPFPerc;

                    }
                }


                mSalWages = Wagecalc(OSalHeadFormualQuery, OSalaryDetails, OEmpSalStructDetails);
                if (OSalHeadFormualQuery.NonStandardSalWages != null)
                {
                    mSalWagesnonstd = WagecalcNonstd(OSalHeadFormualQuery, OSalaryDetails, OEmpSalStructDetails);
                }

                if (VPFApplicable && _Empoffdetils.VPFPerc != null)
                {

                    mSalHeadAmount = (mSalWages * Convert.ToDouble(_Empoffdetils.VPFPerc)) / 100;
                    mSalHeadAmount = CellingCkeck(OSalHeadFormualQuery, mSalHeadAmount);

                    return mSalHeadAmount;
                }

                if (OSalHeadFormualQuery.AmountDependRule != null)
                {

                    mSalHeadAmount = OSalHeadFormualQuery.AmountDependRule.SalAmount;
                    mSalHeadAmount = CellingCkeck(OSalHeadFormualQuery, mSalHeadAmount);
                    return mSalHeadAmount;
                }

                if (OSalHeadFormualQuery.PercentDependRule != null && mSalWages != 0)
                {
                    // MSC bank  DA calulation on Ibase multiply. Ibase change monthly. Prashant Sir Told Hard code bank and Salary head 25/01/2022
                    var CompId = Convert.ToInt32(SessionManager.CompanyId);
                    var CompCode = db.Company.Where(e => e.Id == CompId).AsNoTracking().AsParallel().Select(a => a.Code.ToUpper()).FirstOrDefault();  //asd
                    double mtempIBase = 1;
                    if (CompCode == "MSCB")
                    {


                        var MSCDA = OEmpSalStructDetails.Where(e => e.SalaryHead.Code.ToString().ToUpper() == "DA").FirstOrDefault();
                        int EmpSalStruct_Id = OEmpSalStructDetails.FirstOrDefault().EmpSalStruct.Id;
                        var CurSalHeadQ = db.EmpSalStructDetails.Include(e => e.SalaryHead)
                            .Include(e => e.SalHeadFormula).Where(e => e.SalHeadFormula.Id == OSalHeadFormula && e.EmpSalStruct.Id == EmpSalStruct_Id).FirstOrDefault();
                        string CurSalHead = "";
                        if (CurSalHeadQ != null)
                        {
                            CurSalHead = CurSalHeadQ.SalaryHead.Code;
                        }

                        if (MSCDA != null && CurSalHead == "DA")
                        {
                            if (OSalHeadFormualQuery.SalWages.CeilingMin != null)
                            {
                                if (mSalWages < OSalHeadFormualQuery.SalWages.CeilingMin)
                                {
                                    mSalWages = OSalHeadFormualQuery.SalWages.CeilingMin;
                                }
                            }
                            if (OSalHeadFormualQuery.SalWages.CeilingMax != null)
                            {
                                if (mSalWages > OSalHeadFormualQuery.SalWages.CeilingMax)
                                {
                                    mSalWages = OSalHeadFormualQuery.SalWages.CeilingMax;
                                }
                            }

                            int vdaf = 0;

                            if (OEmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToString() == "VDA").FirstOrDefault() != null && OEmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToString() == "VDA").FirstOrDefault().SalHeadFormula != null)
                            {
                                vdaf = OEmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToString() == "VDA").FirstOrDefault().SalHeadFormula.Id;
                            }
                            var OSalHeadFormualQuery1 = _returnSalHeadFormula(vdaf);

                            var mPayMonth = "";
                            mPayMonth = mCPIMonth;
                            if (OSalaryDetails != null)
                            {
                                var mSalayM = db.SalaryT.Where(e => e.SalEarnDedT == OSalaryDetails);
                                foreach (var ca in mSalayM)
                                {
                                    mPayMonth = ca.PayMonth;
                                }
                            }
                            CPIEntryT OCPIEntryObj = db.CPIEntryT.Where(e => e.PayMonth == mPayMonth && e.EmployeePayroll_Id == OEmployeePayroll.Id).FirstOrDefault();
                            double mIBase = 0;
                            mtempIBase = 0;
                            if (OSalHeadFormualQuery1 != null)
                            {

                                var OCPIRule = OSalHeadFormualQuery1.VDADependRule.CPIRule;
                                var OCPIRuleUnitCalc = OSalHeadFormualQuery1.VDADependRule.CPIRule.CPIUnitCalc;
                                var OCPIRuleDetail = OSalHeadFormualQuery1.VDADependRule.CPIRule.CPIRuleDetails;


                                foreach (var ca in OCPIRuleUnitCalc)
                                {
                                    if (OCPIEntryObj.CalIndexPoint <= ca.IndexMaxCeiling)
                                    {
                                        mIBase = mIBase + ((OCPIEntryObj.CalIndexPoint - ca.BaseIndex) / ca.Unit);
                                        break;
                                    }
                                    else
                                    {
                                        mIBase = mIBase + ((ca.IndexMaxCeiling - ca.BaseIndex) / ca.Unit);
                                    }
                                }
                                mtempIBase = Math.Round(mIBase, OCPIRule.IBaseDigit);
                            }

                        }


                    }
                    //DateTime effdate = Convert.ToDateTime("01/" + mCPIMonth).Date;
                    DateTime effdate = Convert.ToDateTime(mCPIMonth).Date;
                    if (OEmpSalStructDetails != null && OEmpSalStructDetails.Count() > 0)
                    {
                        var oper = OEmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "DA").FirstOrDefault();
                        if (oper != null && oper.SalHeadFormula != null)
                        {
                            var Rule = OEmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "DA" && e.SalHeadFormula.Id == OSalHeadFormula).FirstOrDefault();
                            var dapermonth = new BasicLinkedDA();
                            if (Rule != null)
                            {
                                dapermonth = db.BasicLinkedDA.Where(e => e.EffectiveFrom <= effdate && effdate <= e.EffectiveTo).OrderByDescending(x => x.Id).FirstOrDefault();
                                if (dapermonth != null)
                                {
                                    mSalHeadAmount = (mSalWages * dapermonth.DAPoint) / 100;
                                }
                                else
                                {
                                    mSalHeadAmount = (mSalWages * OSalHeadFormualQuery.PercentDependRule.SalPercent) / 100;

                                }
                            }
                            else
                            {
                                mSalHeadAmount = (mSalWages * OSalHeadFormualQuery.PercentDependRule.SalPercent) / 100;

                            }
                        }

                        else
                        {
                            mSalHeadAmount = (mSalWages * mtempIBase * OSalHeadFormualQuery.PercentDependRule.SalPercent) / 100;

                        }
                    }
                    else
                    {
                        mSalHeadAmount = (mSalWages * mtempIBase * OSalHeadFormualQuery.PercentDependRule.SalPercent) / 100;

                    }
                    mSalHeadAmount = CellingCkeck(OSalHeadFormualQuery, mSalHeadAmount);
                    return mSalHeadAmount;
                }


                if (OSalHeadFormualQuery.SlabDependRule != null && mSalWages != 0)
                {
                    if (mSalWagesnonstd == 0)
                    {

                        double mTempSalWages = 0;
                        foreach (var ca in OSalHeadFormualQuery.SlabDependRule.WageRange)
                        {

                            if (mSalWages >= ca.WageFrom)
                            {
                                if (mSalWages >= ca.WageTo)
                                {
                                    mTempSalWages = ca.WageTo - ca.WageFrom;
                                }
                                else
                                {
                                    mTempSalWages = mSalWages - ca.WageFrom;
                                }
                                mSalHeadAmount = mSalHeadAmount + (mTempSalWages * ca.Percentage / 100) + ca.Amount;
                            }

                        }
                    }
                    else
                    {
                        // msc bank cca as per sir suggest
                        double mTempSalWages = 0;
                        foreach (var ca in OSalHeadFormualQuery.SlabDependRule.WageRange)
                        {

                            if (mSalWagesnonstd > ca.WageFrom && mSalWagesnonstd <= ca.WageTo)
                            {
                                //if (mSalWagesnonstd >= ca.WageTo)
                                //{
                                //    mTempSalWages = ca.WageTo - ca.WageFrom;
                                //}
                                //else
                                //{
                                //    mTempSalWages = mSalWages - ca.WageFrom;
                                //}
                                //mSalHeadAmount = mSalHeadAmount + (mTempSalWages * ca.Percentage / 100) + ca.Amount;
                                mSalHeadAmount = mSalHeadAmount + (mSalWages * ca.Percentage / 100) + ca.Amount;
                            }

                        }

                    }
                    mSalHeadAmount = CellingCkeck(OSalHeadFormualQuery, mSalHeadAmount);
                    return mSalHeadAmount;
                }
                if (OSalHeadFormualQuery.ServiceDependRule != null && mSalWages != 0)
                {
                    // Servicedependrule Confirmation or Joining Date
                    string requiredPathLoanNet = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                    bool existscdNet = System.IO.Directory.Exists(requiredPathLoanNet);
                    string localPathLoanNet;
                    if (!existscdNet)
                    {
                        localPathLoanNet = new Uri(requiredPathLoanNet).LocalPath;
                        System.IO.Directory.CreateDirectory(localPathLoanNet);
                    }
                    string pathLoanNet = requiredPathLoanNet + @"\ServiceDependRule" + ".ini";
                    localPathLoanNet = new Uri(pathLoanNet).LocalPath;
                    if (!System.IO.File.Exists(localPathLoanNet))
                    {

                        using (var fs = new FileStream(localPathLoanNet, FileMode.OpenOrCreate))
                        {
                            StreamWriter str = new StreamWriter(fs);
                            str.BaseStream.Seek(0, SeekOrigin.Begin);

                            str.Flush();
                            str.Close();
                            fs.Close();
                        }


                    }
                    string ServiceDependRule = "";
                    string Compcodechk = "";

                    string requiredPathchkNet = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                    bool existschkNet = System.IO.Directory.Exists(requiredPathchkNet);
                    string localPathchkNet;
                    if (!existschkNet)
                    {
                        localPathchkNet = new Uri(requiredPathchkNet).LocalPath;
                        System.IO.Directory.CreateDirectory(localPathchkNet);
                    }
                    string pathchkNet = requiredPathchkNet + @"\ServiceDependRule" + ".ini";
                    localPathchkNet = new Uri(pathchkNet).LocalPath;
                    using (var streamReader = new StreamReader(localPathchkNet))
                    {
                        string line;

                        while ((line = streamReader.ReadLine()) != null)
                        {

                            var JoinConfermation = line.Split('_')[0];
                            ServiceDependRule = JoinConfermation;
                            var comp = line.Split('_')[1];
                            Compcodechk = comp;

                        }
                    }

                    double mService = 0;
                    string OJoiningService = "";
                    if (ServiceDependRule == "")
                    {
                        if (OEmployeePayroll.Employee.ServiceBookDates.JoiningDate != null)
                        {
                            OJoiningService = OEmployeePayroll.Employee.ServiceBookDates.JoiningDate.Value.ToString();
                        }
                    }
                    else if (ServiceDependRule.ToUpper() == "CONFIRMATIONDATE")
                    {
                        if (OEmployeePayroll.Employee.ServiceBookDates.ConfirmationDate != null)
                        {
                            OJoiningService = OEmployeePayroll.Employee.ServiceBookDates.ConfirmationDate.Value.ToString(); // Rajguru nagar
                        }

                    }
                    else if (ServiceDependRule.ToUpper() == "JOININGDATE")
                    {
                        if (OEmployeePayroll.Employee.ServiceBookDates.JoiningDate != null)
                        {
                            OJoiningService = OEmployeePayroll.Employee.ServiceBookDates.JoiningDate.Value.ToString();
                        }
                    }

                    // DateTime? OJoiningService = OEmployeePayroll.Employee.ServiceBookDates.JoiningDate;




                    if (OJoiningService != "")
                    {


                        var mPayMonth = "";
                        mPayMonth = mCPIMonth;
                        DateTime start =Convert.ToDateTime(OJoiningService);
                        DateTime end = Convert.ToDateTime("01/" + mPayMonth).AddMonths(1).AddDays(-1).Date;// DateTime.Now.Date;
                        int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                        double daysInEndMonth = (end.AddMonths(1) - end).Days;
                        double months;
                        if (end.Day >= start.Day)
                        {
                            months = compMonth + (end.Day - start.Day) / daysInEndMonth;
                        }
                        else
                        {
                            months = compMonth + (start.Day - end.Day) / daysInEndMonth;
                        }

                        mService = months / 12;
                        if (Compcodechk.ToUpper()=="PJSB")
                        {
                            mService = 0;
                            var gradecode = db.PayStruct.Include(e => e.Grade).Where(e => e.Id == OEmployeePayroll.Employee.PayStruct_Id).FirstOrDefault();
                            DateTime oct15 = Convert.ToDateTime("01/11/2015");
                            if (start.Date >= oct15.Date)
                            {
                                DateTime grade120 = Convert.ToDateTime("01/10/2023");
                                if (start.Date >= grade120.Date && gradecode.Grade.Code == "120")
                                {
                                    mService = 0;  
                                }
                                else
                                {
                                    if ((months / 12) > 4 && (gradecode.Grade.Code == "120" || gradecode.Grade.Code == "130" || gradecode.Grade.Code == "140"))
                                    {
                                        mService = 0;  
                                    }
                                    else
                                    {
                                        mService = 1;
                                    }
                                }
                            }
                            else
                            {
                                mService = 0;
                            }
                        }
                        //mService = (DateTime.Now.Date - OJoiningService.Value.Date).Days;
                        //mService = mService / 365;
                        double mTempSalWages = 0;
                        foreach (var ca in OSalHeadFormualQuery.ServiceDependRule.ServiceRange)
                        {

                            if (mService >= ca.ServiceFrom && mService <= ca.ServiceTo)
                            {
                                if (mSalWages >= ca.WagesFrom)
                                {
                                    if (mSalWages >= ca.WagesTo)
                                    {
                                        mTempSalWages = ca.WagesTo - ca.WagesFrom;
                                    }
                                    else
                                    {
                                        mTempSalWages = mSalWages - ca.WagesFrom;
                                    }
                                    mSalHeadAmount = mSalHeadAmount + (mTempSalWages * ca.Percentage / 100) + ca.Amount;
                                }

                                
                            }

                            //if (mService >= ca.ServiceFrom && mService <= ca.ServiceTo)
                            //{
                            //    if (mSalWages > ca.WagesFrom && mSalWages <= ca.WagesTo)
                            //    {
                            //        mSalHeadAmount = ((mSalWages - ca.WagesFrom) * ca.Percentage / 100) + ca.Amount;
                            //        mSalHeadAmount = CellingCkeck(OSalHeadFormualQuery, mSalHeadAmount);
                            //        return mSalHeadAmount;
                            //    }
                            //}
                        }
                        mSalHeadAmount = CellingCkeck(OSalHeadFormualQuery, mSalHeadAmount);
                        return mSalHeadAmount;
                    }
                }
                if (OSalHeadFormualQuery.VDADependRule != null && mSalWages != 0)
                {
                    double mVDA = 0;
                    double mVDAWages = 0;
                    var mPayMonth = "";
                    mPayMonth = mCPIMonth;
                    if (OSalaryDetails != null)
                    {
                        var mSalayM = db.SalaryT.Where(e => e.SalEarnDedT == OSalaryDetails);
                        foreach (var ca in mSalayM)
                        {
                            mPayMonth = ca.PayMonth;
                        }
                    }

                    // EmployeePayroll OEmpCpi = db.EmployeePayroll.Include(e => e.CPIEntryT).Where(e => e.Id == OEmployeePayroll.Id).SingleOrDefault(); //asd

                    CPIEntryT OCPIEntryObj = db.CPIEntryT.Where(e => e.PayMonth == mPayMonth && e.EmployeePayroll_Id == OEmployeePayroll.Id).FirstOrDefault();
                    /* if (OCPIEntryObj == null)
                     {
                         //modified by prashant on 13042017
                         OCPIEntryObj = OEmpCpi.CPIEntryT != null ? OEmpCpi.CPIEntryT.LastOrDefault() : null;
                         //return 0;//modified by prashant 13042017
                         if (OCPIEntryObj == null)
                         {
                             return 0;
						 
                         }
                     }*/
                    //.Select(m=>new{CalIndexPoint=m.CalIndexPoint });
                    var OCPIRule = OSalHeadFormualQuery.VDADependRule.CPIRule;
                    var OCPIRuleUnitCalc = OSalHeadFormualQuery.VDADependRule.CPIRule.CPIUnitCalc;
                    var OCPIRuleDetail = OSalHeadFormualQuery.VDADependRule.CPIRule.CPIRuleDetails;
                    double mIBase = 0;

                    foreach (var ca in OCPIRuleUnitCalc)
                    {
                        if (OCPIEntryObj.CalIndexPoint <= ca.IndexMaxCeiling)
                        {
                            mIBase = mIBase + ((OCPIEntryObj.CalIndexPoint - ca.BaseIndex) / ca.Unit);
                            break;
                        }
                        else
                        {
                            mIBase = mIBase + ((ca.IndexMaxCeiling - ca.BaseIndex) / ca.Unit);
                        }
                    }
                    mIBase = Math.Round(mIBase, OCPIRule.IBaseDigit);
                    var IbaseS = db.SalaryHead.Include(e => e.RoundingMethod).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "IBASE").SingleOrDefault();
                    if (IbaseS != null)
                    {
                        mIBase = RoundingFunction(IbaseS, mIBase);
                    }
                    double Mtempbasic = 0;
                    double tempVDA = 0;
                    var OCPIRuleDetailList = OCPIRuleDetail.OrderBy(e => e.SalFrom).ToList();
                    var CompId = Convert.ToInt32(SessionManager.CompanyId);
                    var CompCode = db.Company.Where(e => e.Id == CompId).AsNoTracking().AsParallel().Select(a => a.Code.ToUpper()).FirstOrDefault();  //asd

                    if (OCPIRule.VDAOnDirectBasic == true)
                    {
                        // Direct On Basic start For Mahanagar bank update
                        for (int i = 0; i < OCPIRuleDetailList.Count(); i++)
                        {
                            var ca = OCPIRuleDetailList[i];


                            if ((ca.ServiceFrom == null && ca.ServiceTo == null) || (ca.ServiceFrom == "0" && ca.ServiceTo == "0"))
                            {

                                if (mSalWages > ca.SalFrom && mSalWages <= ca.SalTo)
                                {

                                    mVDA = mVDA + ((((mSalWages - ca.SalFrom) * (ca.IncrPercent)) / 100) + ca.AdditionalIncrAmount) * mIBase;

                                    if (((((mSalWages - ca.SalFrom) * ca.IncrPercent) / 100) + ca.AdditionalIncrAmount) <= ca.MinAmountIBase)
                                    {
                                        mVDA = ca.MinAmountIBase * mIBase;
                                    }
                                    else if (((((mSalWages - ca.SalFrom) * ca.IncrPercent) / 100) + ca.AdditionalIncrAmount) >= ca.MaxAmountIBase)
                                    {
                                        mVDA = ca.MaxAmountIBase * mIBase;
                                    }



                                }
                            }
                            else // service range vda calculation
                            {
                                double mService = 0;
                                //var vservicedate = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).FirstOrDefault();
                                var vservicedate = db.ServiceBookDates.Where(e => e.Id == OEmployeePayroll.Employee.ServiceBookDates_Id).FirstOrDefault();
                                if (vservicedate.LastPromotionDate == null)
                                {
                                    DateTime? OJoiningService = vservicedate.JoiningDate;

                                    // mService = (Convert.ToDateTime("01/" + mPayMonth).AddMonths(1).AddDays(-1).Date - OJoiningService.Value.Date).Days;
                                    DateTime start = OJoiningService.Value;
                                    DateTime end = Convert.ToDateTime("01/" + mPayMonth).AddMonths(1).AddDays(-1).Date;// DateTime.Now.Date;
                                    int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                                    double daysInEndMonth = (end.AddMonths(1) - end).Days;
                                    double months = compMonth + (start.Day - end.Day) / daysInEndMonth;
                                    mService = months / 12;
                                }
                                else
                                {
                                    DateTime? OJoiningService = vservicedate.LastPromotionDate;

                                    // mService = (Convert.ToDateTime("01/" + mPayMonth).AddMonths(1).AddDays(-1).Date - OJoiningService.Value.Date).Days;

                                    DateTime start = OJoiningService.Value;
                                    DateTime end = Convert.ToDateTime("01/" + mPayMonth).AddMonths(1).AddDays(-1).Date;// DateTime.Now.Date;
                                    int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                                    double daysInEndMonth = (end.AddMonths(1) - end).Days;
                                    double months = compMonth + (start.Day - end.Day) / daysInEndMonth;
                                    mService = months / 12;

                                }

                                //  mService = mService / 365;
                                if (mService >= Convert.ToDouble(ca.ServiceFrom) && mService <= Convert.ToDouble(ca.ServiceTo))
                                {

                                    if (mSalWages > ca.SalFrom && mSalWages <= ca.SalTo)
                                    {

                                        // mVDA = mVDA + ((((ca.SalTo - ca.SalFrom) * (ca.IncrPercent * mIBase)) / 100));
                                        mVDA = mVDA + ((((mSalWages - ca.SalFrom) * (ca.IncrPercent)) / 100) + ca.AdditionalIncrAmount) * mIBase;

                                        if (((((mSalWages - ca.SalFrom) * ca.IncrPercent) / 100) + ca.AdditionalIncrAmount) <= ca.MinAmountIBase)
                                        {
                                            mVDA = ca.MinAmountIBase * mIBase;
                                        }
                                        else if (((((mSalWages - ca.SalFrom) * ca.IncrPercent) / 100) + ca.AdditionalIncrAmount) >= ca.MaxAmountIBase)
                                        {
                                            mVDA = ca.MaxAmountIBase * mIBase;
                                        }



                                    }
                                }
                            }




                        }
                        // Direct On Basic End
                    }
                    else //Not Direct On Basic
                    {


                        for (int i = 0; i < OCPIRuleDetailList.Count(); i++)
                        {
                            var ca = OCPIRuleDetailList[i];
                            if (CompCode == "KDCC" || CompCode == "SDCCB")// Kolhapur DCC and Satara DCC
                            {
                                if (i == 0)
                                {
                                    if (mSalWages > ca.SalTo)//110
                                    {

                                        Mtempbasic = ca.SalTo;//50

                                        mVDA = mVDA + (((Mtempbasic * (ca.IncrPercent * mIBase)) / 100));
                                        if (((Mtempbasic * ca.IncrPercent) / 100) <= ca.MinAmountIBase)
                                        {
                                            mVDA = ca.MinAmountIBase * mIBase;
                                        }
                                        else if (((Mtempbasic * ca.IncrPercent) / 100) >= ca.MaxAmountIBase)
                                        {
                                            mVDA = ca.MaxAmountIBase * mIBase;
                                        }
                                        mSalWages = mSalWages - ca.SalTo;
                                    }
                                    else if (mSalWages > ca.SalFrom && mSalWages <= ca.SalTo)
                                    {

                                        mVDA = mVDA + (((mSalWages * (ca.IncrPercent * mIBase)) / 100));
                                        if (((mSalWages * ca.IncrPercent) / 100) <= ca.MinAmountIBase)
                                        {
                                            mVDA = ca.MinAmountIBase * mIBase;
                                        }
                                        else if (((mSalWages * ca.IncrPercent) / 100) >= ca.MaxAmountIBase)
                                        {
                                            mVDA = ca.MaxAmountIBase * mIBase;
                                        }

                                        mSalWages = mSalWages - ca.SalTo;
                                        if (mSalWages < 0)
                                        {
                                            mSalWages = 0;

                                        }

                                    }
                                }
                                else
                                {
                                    mVDA = mVDA + (mSalWages * 0.25);
                                }
                            }
                            else if (CompCode == "HCBL")
                            {
                                var Offmin = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id)
                                .Include(e => e.Employee.EmpOffInfo)
                                .Include(e => e.Employee.EmpOffInfo.NationalityID)
                        .FirstOrDefault();
                                double minibase = Convert.ToDouble(Offmin.Employee.EmpOffInfo.NationalityID.No3);


                                if (mSalWages > ca.SalFrom && mSalWages <= ca.SalTo)
                                {

                                    mVDA = mVDA + (((mSalWages * (ca.IncrPercent * mIBase)) / 100));
                                    if (((mSalWages * ca.IncrPercent) / 100) <= minibase)
                                    {
                                        mVDA = minibase * mIBase;
                                    }
                                    else if (((mSalWages * ca.IncrPercent) / 100) >= ca.MaxAmountIBase)
                                    {
                                        mVDA = ca.MaxAmountIBase * mIBase;
                                    }

                                }
                            }
                            else
                            {
                                if ((ca.ServiceFrom == null && ca.ServiceTo == null) || (ca.ServiceFrom == "0" && ca.ServiceTo == "0"))
                                {

                                    if (mSalWages >= ca.SalFrom)
                                    {
                                        if (mSalWages >= ca.SalTo)
                                        {
                                            //  mVDA = mVDA + ((((ca.SalTo - ca.SalFrom) * (ca.IncrPercent * mIBase)) / 100));
                                            mVDA = mVDA + ((((ca.SalTo - ca.SalFrom) * (ca.IncrPercent)) / 100) + ca.AdditionalIncrAmount) * mIBase;

                                            if (((((ca.SalTo - ca.SalFrom) * ca.IncrPercent) / 100) + ca.AdditionalIncrAmount) <= ca.MinAmountIBase)
                                            {
                                                mVDA = ca.MinAmountIBase * mIBase;
                                            }
                                            else if (((((ca.SalTo - ca.SalFrom) * ca.IncrPercent) / 100) + ca.AdditionalIncrAmount) >= ca.MaxAmountIBase)
                                            {
                                                mVDA = ca.MaxAmountIBase * mIBase;
                                            }

                                        }
                                        else
                                        {
                                            // mVDA = mVDA + ((((mSalWages - ca.SalFrom) * (ca.IncrPercent * mIBase)) / 100));
                                            mVDA = mVDA + ((((mSalWages - ca.SalFrom) * (ca.IncrPercent)) / 100) + ca.AdditionalIncrAmount) * mIBase;
                                            if (((((mSalWages - ca.SalFrom) * ca.IncrPercent) / 100) + ca.AdditionalIncrAmount) <= ca.MinAmountIBase)
                                            {
                                                mVDA = ca.MinAmountIBase * mIBase;
                                            }
                                            else if (((((mSalWages - ca.SalFrom) * ca.IncrPercent) / 100) + ca.AdditionalIncrAmount) >= ca.MaxAmountIBase)
                                            {
                                                mVDA = ca.MaxAmountIBase * mIBase;
                                            }
                                            break;
                                        }
                                    }
                                }
                                else // service range vda calculation
                                {
                                    double mService = 0;
                                    //var vservicedate = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).FirstOrDefault();
                                    var vservicedate = db.ServiceBookDates.Where(e => e.Id == OEmployeePayroll.Employee.ServiceBookDates_Id).FirstOrDefault();
                                    if (vservicedate.LastPromotionDate == null)
                                    {
                                        DateTime? OJoiningService = vservicedate.JoiningDate;

                                        // mService = (Convert.ToDateTime("01/" + mPayMonth).AddMonths(1).AddDays(-1).Date - OJoiningService.Value.Date).Days;
                                        DateTime start = OJoiningService.Value;
                                        DateTime end = Convert.ToDateTime("01/" + mPayMonth).AddMonths(1).AddDays(-1).Date;// DateTime.Now.Date;
                                        int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                                        double daysInEndMonth = (end.AddMonths(1) - end).Days;
                                        if (start.Day > end.Day)
                                        {
                                            double months = compMonth + (start.Day - end.Day) / daysInEndMonth;
                                            mService = months / 12;
                                        }
                                        else
                                        {
                                            double months = compMonth + (end.Day - start.Day) / daysInEndMonth;
                                            mService = months / 12;
                                        }
                                    }
                                    else
                                    {
                                        DateTime? OJoiningService = vservicedate.LastPromotionDate;

                                        // mService = (Convert.ToDateTime("01/" + mPayMonth).AddMonths(1).AddDays(-1).Date - OJoiningService.Value.Date).Days;

                                        DateTime start = OJoiningService.Value;
                                        DateTime end = Convert.ToDateTime("01/" + mPayMonth).AddMonths(1).AddDays(-1).Date;// DateTime.Now.Date;
                                        int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                                        double daysInEndMonth = (end.AddMonths(1) - end).Days;
                                        if (start.Day > end.Day)
                                        {
                                            double months = compMonth + (start.Day - end.Day) / daysInEndMonth;
                                            mService = months / 12;
                                        }
                                        else
                                        {
                                            double months = compMonth + (end.Day - start.Day) / daysInEndMonth;
                                            mService = months / 12;
                                        }


                                    }

                                    //  mService = mService / 365;
                                    if (mService >= Convert.ToDouble(ca.ServiceFrom) && mService <= Convert.ToDouble(ca.ServiceTo))
                                    {

                                        if (mSalWages >= ca.SalFrom)
                                        {
                                            if (mSalWages >= ca.SalTo)
                                            {
                                                // mVDA = mVDA + ((((ca.SalTo - ca.SalFrom) * (ca.IncrPercent * mIBase)) / 100));
                                                mVDA = mVDA + ((((ca.SalTo - ca.SalFrom) * (ca.IncrPercent)) / 100) + ca.AdditionalIncrAmount) * mIBase;

                                                if (((((ca.SalTo - ca.SalFrom) * ca.IncrPercent) / 100) + ca.AdditionalIncrAmount) <= ca.MinAmountIBase)
                                                {
                                                    mVDA = ca.MinAmountIBase * mIBase;
                                                }
                                                else if (((((ca.SalTo - ca.SalFrom) * ca.IncrPercent) / 100) + ca.AdditionalIncrAmount) >= ca.MaxAmountIBase)
                                                {
                                                    mVDA = ca.MaxAmountIBase * mIBase;
                                                }

                                            }
                                            else
                                            {
                                                //mVDA = mVDA + ((((mSalWages - ca.SalFrom) * (ca.IncrPercent * mIBase)) / 100));
                                                mVDA = mVDA + ((((mSalWages - ca.SalFrom) * (ca.IncrPercent)) / 100) + ca.AdditionalIncrAmount) * mIBase;
                                                if (((((mSalWages - ca.SalFrom) * ca.IncrPercent) / 100) + ca.AdditionalIncrAmount) <= ca.MinAmountIBase)
                                                {
                                                    mVDA = ca.MinAmountIBase * mIBase;
                                                }
                                                else if (((((mSalWages - ca.SalFrom) * ca.IncrPercent) / 100) + ca.AdditionalIncrAmount) >= ca.MaxAmountIBase)
                                                {
                                                    mVDA = ca.MaxAmountIBase * mIBase;
                                                }
                                                break;
                                            }
                                        }
                                    }
                                }
                                //if (mSalWages > ca.SalTo)//110
                                //{

                                //    Mtempbasic = ca.SalTo;//50

                                //    mVDA = mVDA + (((Mtempbasic * (ca.IncrPercent * mIBase)) / 100));
                                //    if (((Mtempbasic * ca.IncrPercent) / 100) <= ca.MinAmountIBase)
                                //    {
                                //        mVDA = ca.MinAmountIBase * mIBase;
                                //    }
                                //    else if (((Mtempbasic * ca.IncrPercent) / 100) >= ca.MaxAmountIBase)
                                //    {
                                //        mVDA = ca.MaxAmountIBase * mIBase;
                                //    }
                                //    mSalWages = mSalWages - ca.SalTo;
                                //}
                                //else
                                //{
                                //    mVDA = mVDA + (((mSalWages * (ca.IncrPercent * mIBase)) / 100));
                                //    if (((mSalWages * ca.IncrPercent) / 100) <= ca.MinAmountIBase)
                                //    {
                                //        mVDA = ca.MinAmountIBase * mIBase;
                                //    }
                                //    else if (((mSalWages * ca.IncrPercent) / 100) >= ca.MaxAmountIBase)
                                //    {
                                //        mVDA = ca.MaxAmountIBase * mIBase;
                                //    }
                                //}

                                //if (mSalWages > ca.SalFrom && mSalWages <= ca.SalTo) this code comment as told sir and above code open
                                //{

                                //    mVDA = mVDA + (((mSalWages * (ca.IncrPercent * mIBase)) / 100));
                                //    if (((mSalWages * ca.IncrPercent) / 100) <= ca.MinAmountIBase)
                                //    {
                                //        mVDA = ca.MinAmountIBase * mIBase;
                                //    }
                                //    else if (((mSalWages * ca.IncrPercent) / 100) >= ca.MaxAmountIBase)
                                //    {
                                //        mVDA = ca.MaxAmountIBase * mIBase;
                                //    }

                                //}
                            }
                        }//for each end
                    }
                    //foreach (var ca in OCPIRuleDetail)
                    //{

                    //    if (mSalWages > ca.SalFrom && mSalWages <= ca.SalTo)
                    //    {
                    //        mVDA = mVDA + (((mSalWages * (ca.IncrPercent * mIBase)) / 100));
                    //        if (((mSalWages * ca.IncrPercent) / 100) <= OCPIRule.MinAmountIBase)
                    //        {
                    //            mVDA = OCPIRule.MinAmountIBase * mIBase;
                    //        }
                    //        else if (((mSalWages * ca.IncrPercent) / 100) >= OCPIRule.MaxAmountIBase)
                    //        {
                    //            mVDA = OCPIRule.MaxAmountIBase * mIBase;
                    //        }
                    //    }
                    //}
                    return mVDA;
                }

                return 0;
            }
        }

        public static double RoundingFunction(SalaryHead OSalaryHead, double mAmount)
        {
            if (OSalaryHead.RoundingMethod == null)
            {
                return Math.Round(mAmount, 0);
            }
            switch (OSalaryHead.RoundingMethod.LookupVal.ToUpper())
            {
                case "NORMAL":
                    if (OSalaryHead.RoundDigit == 0)
                    {
                        return Math.Round(mAmount + 0.01, OSalaryHead.RoundDigit, MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        return Math.Round(mAmount, OSalaryHead.RoundDigit, MidpointRounding.AwayFromZero);
                    }
                    break;
                case "GOVERNMENT":

                    return GovtRound(mAmount, OSalaryHead.RoundDigit);
                    break;
                case "NEARESTFIFTY":
                    var Actamt = mAmount.ToString();
                    string rs = Actamt.Split('.')[0];
                    string Ps = Actamt.Split('.')[1];
                    if (Ps == "5")
                    {
                        Ps = "50";// exact 50 paise e.g 1060.5 then will 1060.50
                    }
                    int pais = Convert.ToInt32(Ps);
                    if (pais >= 50)
                    {
                        mAmount = Convert.ToDouble(rs + "." + "50");
                    }
                    else
                    {
                        mAmount = Convert.ToDouble(rs + "." + "00");
                    }

                    return mAmount;
                    //  return GovtRound(mAmount, OSalaryHead.RoundDigit);
                    break;
                case "PARSE":
                    string mStr = "";
                    switch (OSalaryHead.RoundDigit)
                    {
                        case 0:
                            mStr = "0";
                            break;
                        case 1:
                            mStr = "0.0";
                            break;
                        case 2:
                            mStr = "0.00";
                            break;
                        case 3:
                            mStr = "0.000";
                            break;
                        case 4:
                            mStr = "0.0000";
                            break;
                        case 5:
                            mStr = "0.00000";
                            break;
                        case 6:
                            mStr = "0.000000";
                            break;
                        case 7:
                            mStr = "0.0000000";
                            break;
                        case 8:
                            mStr = "0.00000000";
                            break;
                        case 9:
                            mStr = "0.000000000";
                            break;

                    }
                    return Convert.ToDouble(mAmount.ToString(mStr));
                    break;
                case "ACTUAL":
                    return Math.Round(mAmount);
                    break;
                case "UPPERROUNDING":
                    string Amt = mAmount.ToString("0.00");
                    double amount1 = Convert.ToDouble(Amt);
                    int mstrnum = (int)amount1;
                    if (amount1 > mstrnum)
                    {
                        mAmount = mstrnum + 1;
                    }
                    return mAmount;

                    break;
                default:
                    return 0;
                    break;
            }
        }
        public static double GovtRound(double Amount, int Round_Digit)
        {
            double functionReturnValue = 0;
            double mstrnum = 0;
            double rounddigit = 0;
            double amount1 = 0;
            int i = 0;
            rounddigit = 1;
            for (i = 1; i <= Round_Digit; i++)
            {
                rounddigit = rounddigit * 10;
            }
            if (Round_Digit != 0)
            {
                amount1 = Convert.ToDouble(Amount.ToString("0.00")) * rounddigit; //Strings.Format(Amount, "0.00") * rounddigit;
                mstrnum = (amount1 % 10);
                amount1 = amount1 - mstrnum;
                if (mstrnum > 0 & mstrnum <= 2)
                {
                    mstrnum = 0;
                }
                else if (mstrnum > 2 & mstrnum <= 5)
                {
                    mstrnum = 5;
                }
                else if (mstrnum > 5 & mstrnum <= 7)
                {
                    mstrnum = 5;
                }
                else if (mstrnum > 7 & mstrnum <= 9)
                {
                    mstrnum = 10;
                }
                amount1 = amount1 + mstrnum;
                functionReturnValue = Convert.ToDouble((amount1 / rounddigit).ToString("0.00"));// *rounddigit; //Strings.Format((amount1 / rounddigit), "0.00");
            }
            else
            {
                return Math.Round(Amount, Round_Digit, MidpointRounding.AwayFromZero);
            }
            return functionReturnValue;
        }
        public static void EmployeeSalaryStructCreation(EmployeePayroll OEmployeePayroll, Employee OEmployee,
              PayScaleAgreement OPayScaleAgreement, DateTime mEffectiveDate)
        {
            // db.RefreshAllEntites(RefreshMode.StoreWins);
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                var Compayroll_id = Convert.ToInt32(SessionManager.CompPayrollId);

                var CompanyPayroll_OPayScaleAssignment = db.CompanyPayroll.Where(e => e.Id == Compayroll_id).SingleOrDefault();
                List<PayScaleAssignment> PayScaleAssignment = db.PayScaleAssignment.Where(e => e.CompanyPayroll_Id == CompanyPayroll_OPayScaleAssignment.Id).ToList();
                CompanyPayroll_OPayScaleAssignment.PayScaleAssignment = PayScaleAssignment;
                foreach (var PayScaleAssignmentitem in PayScaleAssignment)
                {
                    PayScaleAgreement PayScaleAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAssignmentitem.PayScaleAgreement_Id).SingleOrDefault();
                    PayScaleAssignmentitem.PayScaleAgreement = PayScaleAgreement;
                    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == PayScaleAssignmentitem.SalaryHead_Id).SingleOrDefault();
                    PayScaleAssignmentitem.SalaryHead = SalaryHead;
                    List<SalHeadFormula> SalHeadFormula = db.PayScaleAssignment.Where(e => e.Id == PayScaleAssignmentitem.Id).Select(r => r.SalHeadFormula.ToList()).SingleOrDefault();
                    //.Select(r => r.SalHeadFormula).ToList().SingleOrDefault()
                    PayScaleAssignmentitem.SalHeadFormula = SalHeadFormula;
                    foreach (var SalHeadFormulaitem in SalHeadFormula)
                    {
                        GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == SalHeadFormulaitem.GeoStruct_Id).SingleOrDefault();
                        SalHeadFormulaitem.GeoStruct = GeoStruct;
                        PayStruct PayStruct = db.PayStruct.Where(e => e.Id == SalHeadFormulaitem.PayStruct_Id).SingleOrDefault();
                        SalHeadFormulaitem.PayStruct = PayStruct;
                        FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == SalHeadFormulaitem.FuncStruct_Id).SingleOrDefault();
                        SalHeadFormulaitem.FuncStruct = FuncStruct;
                    }
                }




                if (CompanyPayroll_OPayScaleAssignment == null)
                {
                    return;
                }
                var OPayScaleAssignment = CompanyPayroll_OPayScaleAssignment.PayScaleAssignment.Where(e => e.PayScaleAgreement != null &&
                    e.PayScaleAgreement.Id == OPayScaleAgreement.Id).ToList();

                EmpSalStruct OEmpSalStruct = new EmpSalStruct();
                {
                    OEmpSalStruct.EffectiveDate = mEffectiveDate;
                    if (OEmployee.GeoStruct != null)
                    {
                        OEmpSalStruct.GeoStruct = db.GeoStruct.Where(e => e.Id == OEmployee.GeoStruct.Id).SingleOrDefault();
                    };
                    if (OEmployee.FuncStruct != null)
                    {
                        OEmpSalStruct.FuncStruct = db.FuncStruct.Where(e => e.Id == OEmployee.FuncStruct.Id).SingleOrDefault();
                    };
                    if (OEmployee.PayStruct != null)
                    {
                        OEmpSalStruct.PayStruct = db.PayStruct.Where(e => e.Id == OEmployee.PayStruct.Id).SingleOrDefault();
                    };
                    OEmpSalStruct.DBTrack = dbt;
                    db.EmpSalStruct.Add(OEmpSalStruct);
                    db.SaveChanges();
                    List<EmpSalStructDetails> OEmpSalStructDetails = new List<EmpSalStructDetails>();
                    foreach (var OPayScaleAssignmentData in OPayScaleAssignment)
                    {
                        if (OPayScaleAssignmentData.SalaryHead.Code.ToUpper() == "VPF" && !OEmployee.EmpOffInfo.VPFAppl)
                        {
                            continue;
                        }
                        EmpSalStructDetails OEmpSalStructDetailsObj = new EmpSalStructDetails();
                        {
                            OEmpSalStructDetailsObj.Amount = 0;
                            if (OPayScaleAssignmentData != null)
                            {
                                OEmpSalStructDetailsObj.PayScaleAssignment = db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                            }
                            if (OPayScaleAssignmentData.SalaryHead != null)
                            {
                                OEmpSalStructDetailsObj.SalaryHead = db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                            }

                            if (OPayScaleAssignmentData.SalaryHead != null && OEmpSalStructDetailsObj.PayScaleAssignment.SalHeadFormula != null)//newly added by prashant on 24042017
                            {
                                SalHeadFormula Salformula = new SalHeadFormula();
                                Salformula = SalFormulaFinderNew(OEmpSalStruct, OEmpSalStructDetailsObj.PayScaleAssignment, OPayScaleAssignmentData.SalaryHead.Id);
                                OEmpSalStructDetailsObj.SalHeadFormula = Salformula == null ? null : db.SalHeadFormula.Where(e => e.Id == Salformula.Id).SingleOrDefault();
                            }
                            OEmpSalStructDetailsObj.DBTrack = dbt;
                        };
                        OEmpSalStructDetails.Add(OEmpSalStructDetailsObj);

                    }
                    db.EmpSalStructDetails.AddRange(OEmpSalStructDetails);
                    db.SaveChanges();
                    List<EmpSalStructDetails> OFAT = new List<EmpSalStructDetails>();
                    var aa = db.EmpSalStruct.Where(e => e.Id == OEmpSalStruct.Id).SingleOrDefault();
                    OFAT.AddRange(OEmpSalStructDetails);
                    aa.EmpSalStructDetails = OFAT;
                    db.EmpSalStruct.Attach(aa);
                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                try
                {

                    List<EmpSalStruct> OTemp2 = new List<EmpSalStruct>();
                    OTemp2.Add(db.EmpSalStruct.Where(e => e.Id == OEmpSalStruct.Id).SingleOrDefault());
                    if (OEmployeePayroll == null)
                    {
                        EmployeePayroll OTEP = new EmployeePayroll()
                        {
                            Employee = db.Employee.Where(e => e.Id == OEmployee.Id).SingleOrDefault(),
                            EmpSalStruct = OTemp2,
                            DBTrack = dbt
                        };
                        db.EmployeePayroll.Add(OTEP);
                        db.SaveChanges();
                    }
                    else
                    {
                        var aa = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).SingleOrDefault();
                        aa.EmpSalStruct = OTemp2;
                        db.EmployeePayroll.Attach(aa);
                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = "EmployeeSalaryStructCreation - SalaryHeadGenProcess",
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                }
            }
        }
        public static Boolean Child(EmployeePayroll OEmployeePayroll, DateTime? mEffectiveDate)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                Boolean childappl = false;
                var childcheck = db.EmployeePayroll
                    .Include(e => e.Employee.FamilyDetails.Select(r => r.Relation))
                    .Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                if (childcheck != null)
                {
                    var chldrec = childcheck.Employee.FamilyDetails.Where(e => e.Relation.LookupVal.ToUpper() == "SON" || e.Relation.LookupVal.ToUpper() == "DAUGHTER").ToList();
                    foreach (var item in chldrec)
                    {
                        double mAge = 0;
                        var mDateofBirth = item.DateofBirth;
                        DateTime start = mDateofBirth.Value;
                        DateTime end = Convert.ToDateTime("01/" + mEffectiveDate.Value.ToString("MM/yyyy")).AddMonths(1).AddDays(-1).Date;
                        int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                        double daysInEndMonth = (end.AddMonths(1) - end).Days;
                        double months = compMonth + (start.Day - end.Day) / daysInEndMonth;
                        mAge = months / 12;
                        if (item.IsHandicap == false && (mAge >= 5 && mAge <= 21))
                        {
                            childappl = true;

                            break;
                        }
                        else if (item.IsHandicap == true && (mAge >= 5 && mAge <= 22))
                        {
                            childappl = true;
                            break;
                        }
                    }

                }
                return childappl;
            }
        }

        public static void EmployeeSalaryStructUpdate(EmployeePayroll OEmployeePayroll, DateTime? mEffectiveDate)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var OEmpSalStruct = OEmployeePayroll.EmpSalStruct
                      .Where(e => e.EffectiveDate.Value.Date.Date == mEffectiveDate.Value.Date).OrderBy(e => e.EmpSalStructDetails.Select(r => r.SalaryHead)).ToList();

                foreach (var OEmpSalStructData in OEmpSalStruct)
                {
                    var OEmpSalHead = OEmpSalStructData.EmpSalStructDetails.OrderBy(e => e.SalaryHead.SeqNo).ToList();
                    foreach (var OSalStructDetails in OEmpSalHead)
                    {
                        if (OSalStructDetails.SalHeadFormula != null && OSalStructDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "BASIC")
                        {
                            double SalAmount = SalHeadAmountCalc(OSalStructDetails.SalHeadFormula.Id, null, OEmpSalHead, OEmployeePayroll,
                                OEmpSalStructData.EffectiveDate.Value.ToString("MM/yyyy"), OSalStructDetails.SalaryHead.Code.ToUpper() == "VPF" ? true : false);
                            SalAmount = RoundingFunction(OSalStructDetails.SalaryHead, SalAmount);
                            //child alllowance start

                            if (OSalStructDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "CHILD")
                            {
                                Boolean Childap = Child(OEmployeePayroll, mEffectiveDate);
                                if (Childap == false)
                                {
                                    SalAmount = 0;
                                }
                            }
                            //child alllowance end
                            if (SalAmount != OSalStructDetails.Amount)
                            {
                                var OSalStructDetailSave = db.EmpSalStructDetails
                                    .Where(e => e.Id == OSalStructDetails.Id).SingleOrDefault();

                                OSalStructDetailSave.Amount = SalAmount;
                                OSalStructDetails.Amount = SalAmount;

                                try
                                {
                                    db.EmpSalStructDetails.Attach(OSalStructDetailSave);
                                    db.Entry(OSalStructDetailSave).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    LogFile Logfile = new LogFile();
                                    ErrorLog Err = new ErrorLog()
                                    {
                                        ControllerName = "EmployeeSalaryStructUpdate - SalaryHeadGenProcess",
                                        ExceptionMessage = ex.Message,
                                        ExceptionStackTrace = ex.StackTrace,
                                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                        LogTime = DateTime.Now
                                    };
                                    Logfile.CreateLogFile(Err);
                                }
                            }
                        }
                    }
                }
            }
        }
        public static void EmployeeSalaryStructUpdateAmount(EmployeePayroll OEmployeePayroll, DateTime? mEffectiveDate, List<EmpSalStruct> Oempsalstructdata)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var OEmpSalStruct = Oempsalstructdata
                      .Where(e => e.EffectiveDate.Value.Date.Date == mEffectiveDate.Value.Date).OrderBy(e => e.EmpSalStructDetails.Select(r => r.SalaryHead)).ToList();

                foreach (var OEmpSalStructData in OEmpSalStruct)
                {
                    var OEmpSalHead = OEmpSalStructData.EmpSalStructDetails.OrderBy(e => e.SalaryHead.SeqNo).ToList();
                    foreach (var OSalStructDetails in OEmpSalHead)
                    {
                        if (OSalStructDetails.SalHeadFormula != null && OSalStructDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "BASIC")
                        {
                            double SalAmount = SalHeadAmountCalc(OSalStructDetails.SalHeadFormula.Id, null, OEmpSalHead, OEmployeePayroll,
                                OEmpSalStructData.EffectiveDate.Value.ToString("MM/yyyy"), OSalStructDetails.SalaryHead.Code.ToUpper() == "VPF" ? true : false);
                            SalAmount = RoundingFunction(OSalStructDetails.SalaryHead, SalAmount);
                            //child alllowance start

                            if (OSalStructDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "CHILD")
                            {
                                Boolean Childap = Child(OEmployeePayroll, mEffectiveDate);
                                if (Childap == false)
                                {
                                    SalAmount = 0;
                                }
                            }
                            //child alllowance end
                            if (SalAmount != OSalStructDetails.Amount)
                            {
                                var OSalStructSave = db.EmpSalStructDetails
                                    .Where(e => e.Id == OSalStructDetails.Id).SingleOrDefault();
                                var OSalStruct = db.EmpSalStruct
                                    .Where(e => e.Id == OSalStructDetails.EmpSalStruct_Id).SingleOrDefault();
                                OSalStruct.EmpSalStructDetails.Add(OSalStructSave);


                                OSalStructSave.Amount = SalAmount;
                                OSalStructDetails.Amount = SalAmount;

                                try
                                {

                                    db.EmpSalStruct.Attach(OSalStruct);
                                    db.Entry(OSalStruct).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    LogFile Logfile = new LogFile();
                                    ErrorLog Err = new ErrorLog()
                                    {
                                        ControllerName = "EmployeeSalaryStructUpdate - SalaryHeadGenProcess",
                                        ExceptionMessage = ex.Message,
                                        ExceptionStackTrace = ex.StackTrace,
                                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                        LogTime = DateTime.Now
                                    };
                                    Logfile.CreateLogFile(Err);
                                }
                            }
                        }
                    }

                }
            }
        }

        public static void EmpStructBackup(EmpSalStruct OEmpSalStruct)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, OEmpSalStruct.DBTrack);
                        DT_EmpSalStruct DT_EmpSalStruct = (DT_EmpSalStruct)rtn_Obj;
                        DT_EmpSalStruct.FuncStruct_Id = OEmpSalStruct.FuncStruct == null ? 0 : OEmpSalStruct.FuncStruct.Id;
                        DT_EmpSalStruct.GeoStruct_Id = OEmpSalStruct.GeoStruct == null ? 0 : OEmpSalStruct.GeoStruct.Id;
                        DT_EmpSalStruct.PayStruct_Id = OEmpSalStruct.PayStruct == null ? 0 : OEmpSalStruct.PayStruct.Id;
                        db.Create(DT_EmpSalStruct);
                        db.SaveChanges();
                        foreach (var ca in OEmpSalStruct.EmpSalStructDetails)
                        {
                            rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, ca.DBTrack);
                            DT_EmpSalStructDetails DT_EmpSalStructDet = (DT_EmpSalStructDetails)rtn_Obj;
                            DT_EmpSalStructDet.SalaryHead_Id = ca.SalaryHead == null ? 0 : ca.SalaryHead.Id;
                            DT_EmpSalStructDet.PayScaleAssignment_Id = ca.PayScaleAssignment == null ? 0 : ca.PayScaleAssignment.Id;
                            db.Create(DT_EmpSalStructDet);
                            db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogFile Logfile = new LogFile();
                        ErrorLog Err = new ErrorLog()
                        {
                            ControllerName = "EmpStructBackup",
                            ExceptionMessage = ex.Message,
                            ExceptionStackTrace = ex.StackTrace,
                            LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                            LogTime = DateTime.Now
                        };
                        Logfile.CreateLogFile(Err);
                    }
                }

            }
        }
        public static void EmployeeSalaryStructUpdateNew(EmpSalStruct OEmpSalStructData, EmployeePayroll OEmployeePayroll, DateTime? mEffectiveDate)
        {
            //write salhead updating criteria e.g. ignore basic
            var OEmpSalHead = OEmpSalStructData.EmpSalStructDetails.OrderBy(e => e.SalaryHead.SeqNo).ToList();//added by prashant 13/4/17


            foreach (var OSalStructDetails in OEmpSalHead)
            {
                if (OSalStructDetails.SalHeadFormula != null && OSalStructDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "BASIC")
                {

                    double SalAmount = 0;

                    if (OSalStructDetails.SalaryHead.Code.ToUpper() == "VPF")
                    {
                        bool OVPFAppl;
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            // OVPFAppl = db.EmployeePayroll.Include(e => e.Employee.EmpOffInfo).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault().Employee.EmpOffInfo.VPFAppl;
                            OVPFAppl = db.Employee.Where(e => e.Id == OEmployeePayroll.Employee_Id).Select(r => r.EmpOffInfo.VPFAppl).SingleOrDefault();

                        }

                        SalAmount = SalHeadAmountCalc(OSalStructDetails.SalHeadFormula.Id, null, OEmpSalHead, OEmployeePayroll, OEmpSalStructData.EffectiveDate.Value.ToString("MM/yyyy"), OVPFAppl);
                    }
                    else
                    {

                        SalAmount = SalHeadAmountCalc(OSalStructDetails.SalHeadFormula.Id, null, OEmpSalHead, OEmployeePayroll, OEmpSalStructData.EffectiveDate.Value.ToString("MM/yyyy"), false);
                    }

                    //double SalAmount = SalHeadAmountCalc(OSalStructDetails.SalHeadFormula.Id, null, OEmpSalHead, OEmployeePayroll,
                    //    OEmpSalStructData.EffectiveDate.Value.ToString("MM/yyyy"));


                    SalAmount = RoundingFunction(OSalStructDetails.SalaryHead, SalAmount);

                    //child alllowance start

                    if (OSalStructDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "CHILD")
                    {
                        Boolean Childap = Child(OEmployeePayroll, mEffectiveDate);
                        if (Childap == false)
                        {
                            SalAmount = 0;
                        }
                    }
                    //child alllowance end

                    if (SalAmount != OSalStructDetails.Amount)
                    {
                        OSalStructDetails.Amount = SalAmount;
                    }
                }
                else if (OSalStructDetails.SalHeadFormula == null && OSalStructDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "BASIC")
                {
                    //OSalStructDetails.Amount = 0.0;
                    OSalStructDetails.Amount = OSalStructDetails.Amount;
                }
            }
        }
        public static EmployeePayroll _EmployeePayrollStruct(Int32 OEmployeePayroll_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Database.CommandTimeout = 300;
                EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll_id && e.EmpSalStruct.Any(a => a.EndDate == null))
                         .FirstOrDefault();
                EmpSalStruct EmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id).SingleOrDefault();
                OEmployeePayroll.EmpSalStruct.Add(EmpSalStruct);
                GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == EmpSalStruct.GeoStruct_Id).SingleOrDefault();
                EmpSalStruct.GeoStruct = GeoStruct;
                FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == EmpSalStruct.FuncStruct_Id).SingleOrDefault();
                EmpSalStruct.FuncStruct = FuncStruct;
                PayStruct PayStruct = db.PayStruct.Where(e => e.Id == EmpSalStruct.PayStruct_Id).SingleOrDefault();
                EmpSalStruct.PayStruct = PayStruct;
                List<EmpSalStructDetails> EmpSalStructDetailsList = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == EmpSalStruct.Id).ToList();
                EmpSalStruct.EmpSalStructDetails = EmpSalStructDetailsList;
                foreach (var EmpSalStructDetailsListitem in EmpSalStructDetailsList)
                {
                    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsListitem.SalaryHead_Id).SingleOrDefault();
                    EmpSalStructDetailsListitem.SalaryHead = SalaryHead;
                    PayScaleAssignment PayScaleAssignment = db.PayScaleAssignment.Include(r => r.SalHeadFormula).Where(e => e.Id == EmpSalStructDetailsListitem.PayScaleAssignment_Id).SingleOrDefault();
                    EmpSalStructDetailsListitem.PayScaleAssignment = PayScaleAssignment;
                    LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                    SalaryHead.SalHeadOperationType = SalHeadOperationType;
                }


                return OEmployeePayroll;
            }
        }
        public static void EmployeeSalaryStructCreationForCPI(EmployeePayroll OEmployeePayroll, DateTime? mEffectiveDate, int pagree)
        {
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    // Rajgurunagar bank vda point add 5 rs agreementdate_grade_year_april_amount
                    string requiredPathLoan = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                    bool exists = System.IO.Directory.Exists(requiredPathLoan);
                    string localPathLoan;
                    if (!exists)
                    {
                        localPathLoan = new Uri(requiredPathLoan).LocalPath;
                        System.IO.Directory.CreateDirectory(localPathLoan);
                    }
                    string pathLoan = requiredPathLoan + @"\VDAPOINT" + ".ini";
                    localPathLoan = new Uri(pathLoan).LocalPath;
                    if (!System.IO.File.Exists(localPathLoan))
                    {

                        using (var fs = new FileStream(localPathLoan, FileMode.OpenOrCreate))
                        {
                            StreamWriter str = new StreamWriter(fs);
                            str.BaseStream.Seek(0, SeekOrigin.Begin);

                            str.Flush();
                            str.Close();
                            fs.Close();
                        }


                    }

                    string requiredPathchk = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                    bool existschk = System.IO.Directory.Exists(requiredPathchk);
                    string localPathchk;
                    if (!existschk)
                    {
                        localPathchk = new Uri(requiredPathchk).LocalPath;
                        System.IO.Directory.CreateDirectory(localPathchk);
                    }
                    string pathchk = requiredPathchk + @"\VDAPOINT" + ".ini";
                    localPathchk = new Uri(pathchk).LocalPath;
                    var Gradecode = db.Employee.Include(e => e.PayStruct).Include(e => e.PayStruct.Grade).Where(e => e.Id == OEmployeePayroll.Employee.Id).SingleOrDefault();
                    // Rajgurunagar bank vda point add 5 rs agreementdate_grade_year_april_amount

                    var dbt = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                    //var OEmployeePayroll = _EmployeePayrollStruct(OEmployeePayroll_id);
                    //Utility.DumpProcessStatus("Employee Salary Struct Creation For CPI", 946);



                    ////EmployeePayroll OEmployeePayroll = db.EmployeePayroll
                    ////    .Include(e => e.EmpSalStruct)
                    ////    .Include(e => e.EmpSalStruct.Select(r => r.GeoStruct))
                    ////    .Include(e => e.EmpSalStruct.Select(r => r.FuncStruct))
                    ////    .Include(e => e.EmpSalStruct.Select(r => r.PayStruct))
                    ////    //.Include(e => e.Employee.GeoStruct)
                    ////    //.Include(e => e.Employee.GeoStruct.Location)
                    ////    //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                    ////    //.Include(e => e.Employee.PayStruct)
                    ////    //.Include(e => e.Employee.FuncStruct)
                    ////    .Include(e => e.EmpSalStruct.Select(a => a.EmpSalStructDetails))
                    ////    .Include(e => e.EmpSalStruct.Select(a => a.EmpSalStructDetails.Select(z => z.SalaryHead)))
                    ////    .Include(e => e.EmpSalStruct.Select(a => a.EmpSalStructDetails.Select(r => r.PayScaleAssignment)))
                    ////    .Include(e => e.EmpSalStruct.Select(a => a.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula)))//added by prashant 13042017
                    ////    .Include(e => e.EmpSalStruct.Select(a => a.EmpSalStructDetails.Select(r => r.SalaryHead.SalHeadOperationType)))
                    ////    .Where(e => e.Id == OEmployeePayroll_id && e.EmpSalStruct.Any(a => a.EndDate == null))
                    ////    .FirstOrDefault();

                    /*  EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Include(e => e.EmpSalStruct).Include(e => e.Employee.ServiceBookDates)
                         .Where(e => e.Id == OEmployeePayroll_id)
                        .FirstOrDefault();*/
                    /*
                      if (OEmployeePayroll == null)
                      {
                          return;
                      }*/

                    EmpSalStruct OEmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id && e.EndDate == null)
                     .FirstOrDefault();
                    if (OEmpSalStruct != null)
                    {

                        GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OEmpSalStruct.GeoStruct_Id).SingleOrDefault();
                        OEmpSalStruct.GeoStruct = GeoStruct;
                        FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == OEmpSalStruct.FuncStruct_Id).SingleOrDefault();
                        OEmpSalStruct.FuncStruct = FuncStruct;
                        PayStruct PayStruct = db.PayStruct.Where(e => e.Id == OEmpSalStruct.PayStruct_Id).SingleOrDefault();
                        OEmpSalStruct.PayStruct = PayStruct;
                        List<EmpSalStructDetails> EmpSalStructDetailsList = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == OEmpSalStruct.Id).ToList();
                        OEmpSalStruct.EmpSalStructDetails = EmpSalStructDetailsList;

                        foreach (var EmpSalStructDetailsitem in EmpSalStructDetailsList)
                        {
                            SalaryHead SalaryHead = db.SalaryHead.Include(e => e.RoundingMethod).Where(e => e.Id == EmpSalStructDetailsitem.SalaryHead_Id).SingleOrDefault();
                            EmpSalStructDetailsitem.SalaryHead = SalaryHead;
                            //LookupValue RoundingMethod = db.LookupValue.Include(e => e.Id).Where(e => e.Id == SalaryHead.RoundingMethod_Id).SingleOrDefault();
                            //SalaryHead.RoundingMethod = RoundingMethod;

                        }

                        //List<EmpSalStructDetails> EmpSalStructDetails = db.EmpSalStructDetails
                        //                                    .Include(e => e.SalaryHead)
                        //             .Include(e => e.SalaryHead.RoundingMethod)
                        //   .Include(e => e.PayScaleAssignment)
                        //   .Include(e => e.PayScaleAssignment.PayScaleAgreement)
                        //   .Include(e => e.PayScaleAssignment.SalHeadFormula)//added by prashant 13042017
                        //   .Include(e => e.SalaryHead.SalHeadOperationType)
                        //        .Where(e => e.EmpSalStruct.Id == OEmpSalStruct.Id).OrderBy(e => e.SalaryHead.SeqNo).ToList();
                        var EmpSalStructDetails = db.EmpSalStructDetails
                                                          .Join(db.PayScaleAssignment, p => p.PayScaleAssignment.Id, pc => pc.Id, (p, pc) => new { p, pc })
                                                          .Where(p => p.p.EmpSalStruct.Id == OEmpSalStruct.Id).OrderBy(p => p.p.SalaryHead.SeqNo)
                                                         .Select(m => new EmpSalStructDetailsData
                                                         {
                                                             Amount = m.p.Amount,
                                                             PayScaleAgreement = m.p.PayScaleAssignment.PayScaleAgreement,
                                                             SalaryHead = m.p.SalaryHead,
                                                             PayScaleAssignment = m.p.PayScaleAssignment,
                                                             Id = m.p.PayScaleAssignment.Id,
                                                             SalHeadFormula = m.p.SalHeadFormula,

                                                             SalHeadOperationType = m.p.SalaryHead.SalHeadOperationType
                                                         }).ToList();

                        if (OEmpSalStruct.EffectiveDate < mEffectiveDate)
                        {
                            //end previous empstruct and new one

                            var OListESD = new List<EmpSalStructDetails>();
                            var newagreementstruct = EmpSalStructDetails.Where(e => e.PayScaleAgreement.Id == pagree).Select(r => r.PayScaleAgreement.Id == pagree).ToList();
                            if (newagreementstruct.Count > 0)
                            {


                                foreach (var ca in EmpSalStructDetails)
                                {
                                    // Rajgurunagar bank vda point add 5 rs agreementdate_grade_year_april_amount
                                    double vdapoint = 0;
                                    if (ca.SalaryHead.Code == "VDA POINTS")
                                    {
                                        using (var streamReader = new StreamReader(localPathchk))
                                        {
                                            string line;

                                            while ((line = streamReader.ReadLine()) != null)
                                            {

                                                var agreementdate = line.Split('_')[0];
                                                DateTime agrdt = Convert.ToDateTime(agreementdate);
                                                var Grade = line.Split('_')[1];
                                                var CYear = line.Split('_')[2];
                                                var CMonth = line.Split('_')[3];
                                                var addamount = line.Split('_')[4];
                                                string monthName = (mEffectiveDate ?? DateTime.MinValue).ToString("MMMM");
                                                if (agreementdate != "" && monthName == CMonth)
                                                {
                                                    if (ca.Amount != 0 && OEmployeePayroll.Employee.ServiceBookDates.ConfirmationDate.Value.Date < agrdt.Date && Gradecode.PayStruct.Grade.Code == Grade && monthName == CMonth)
                                                    {
                                                        vdapoint = Convert.ToDouble(addamount);
                                                        break;
                                                    }
                                                    else if (ca.Amount != 0 && OEmployeePayroll.Employee.ServiceBookDates.ConfirmationDate.Value.Date >= agrdt.Date && Gradecode.PayStruct.Grade.Code == Grade && monthName == CMonth)
                                                    {
                                                        double mAge = 0;
                                                        var mDateofConf = OEmployeePayroll.Employee.ServiceBookDates.ConfirmationDate;
                                                        DateTime start = mDateofConf.Value;
                                                        DateTime end = agrdt;// DateTime.Now.Date;
                                                        int compMonth = (start.Month + start.Year * 12) - (end.Month + end.Year * 12);
                                                        //   double daysInEndMonth = (end - end.AddMonths(1)).Days;
                                                        double daysInEndMonth = (end.AddMonths(1) - end).Days;
                                                        double months = compMonth + (start.Day - end.Day) / daysInEndMonth;
                                                        mAge = months / 12;
                                                        int confyear = Convert.ToInt32(CYear);
                                                        if (mAge > confyear)
                                                        {
                                                            vdapoint = Convert.ToDouble(addamount);
                                                            break;
                                                        }


                                                    }
                                                }
                                            }
                                        }
                                    }
                                    // Rajgurunagar bank vda point add 5 rs agreementdate_grade_year_april_amount
                                    EmpSalStructDetails OEmpSalStructDetailsNEw = new EmpSalStructDetails()
                                    {
                                        Amount = ca.Amount + vdapoint,
                                        DBTrack = dbt,
                                        PayScaleAssignment = ca.PayScaleAssignment == null ? null : db.PayScaleAssignment.Where(e => e.Id == ca.PayScaleAssignment.Id).SingleOrDefault(),
                                        SalaryHead = db.SalaryHead.Include(e => e.SalHeadOperationType).Include(e => e.RoundingMethod).Where(e => e.Id == ca.SalaryHead.Id).SingleOrDefault(),// .Where(e => e.Id == ca.SalaryHead.Id).SingleOrDefault(),
                                        SalHeadFormula = ca.SalHeadFormula == null ? null : db.SalHeadFormula.Where(e => e.Id == ca.SalHeadFormula.Id).SingleOrDefault(),

                                    };
                                    OListESD.Add(OEmpSalStructDetailsNEw);
                                }

                                EmpSalStruct db_EmpSalStruct = OEmpSalStruct;
                                db_EmpSalStruct.EndDate = mEffectiveDate.Value.AddDays(-1);

                                db.EmpSalStruct.Attach(db_EmpSalStruct);
                                db.Entry(db_EmpSalStruct).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                                EmpSalStruct OEmpSalStructNew = new EmpSalStruct();
                                OEmpSalStructNew.DBTrack = dbt;
                                OEmpSalStructNew.FuncStruct = OEmpSalStruct.FuncStruct == null ? null : db.FuncStruct.Where(e => e.Id == OEmpSalStruct.FuncStruct.Id).SingleOrDefault();
                                OEmpSalStructNew.GeoStruct = OEmpSalStruct.GeoStruct == null ? null : db.GeoStruct.Where(e => e.Id == OEmpSalStruct.GeoStruct.Id).SingleOrDefault();
                                OEmpSalStructNew.PayStruct = OEmpSalStruct.PayStruct == null ? null : db.PayStruct.Where(e => e.Id == OEmpSalStruct.PayStruct.Id).SingleOrDefault();
                                OEmpSalStructNew.PayDays = OEmpSalStruct.PayDays;

                                OEmpSalStructNew.EffectiveDate = mEffectiveDate;
                                OEmpSalStructNew.EndDate = null;


                                OEmpSalStructNew.EmpSalStructDetails = OListESD;
                                db.EmpSalStruct.Add(OEmpSalStructNew);
                                db.SaveChanges();
                                EmployeePayroll OEmpPayrollSave = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).SingleOrDefault();
                                OEmpPayrollSave.EmpSalStruct.Add(OEmpSalStructNew);
                                db.Entry(OEmpPayrollSave).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                                EmployeeSalaryStructUpdateNew(OEmpSalStructNew, OEmployeePayroll, mEffectiveDate);
                                OEmployeePayroll.EmpSalStruct.Add(OEmpSalStructNew);

                                db.SaveChanges();

                            }
                            else
                            {
                                int Comp_Id = int.Parse((System.Web.HttpContext.Current.Session["CompId"].ToString()));
                                int empid = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Select(e => e.Employee.Id).SingleOrDefault();
                                EmployeeSalaryStructCreationTest(OEmployeePayroll, empid, pagree, Convert.ToDateTime(mEffectiveDate), Comp_Id);

                                EmpSalStruct db_EmpSalStruct = OEmpSalStruct;
                                db_EmpSalStruct.EndDate = mEffectiveDate.Value.AddDays(-1);

                                db.EmpSalStruct.Attach(db_EmpSalStruct);
                                db.Entry(db_EmpSalStruct).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                            }

                            // var db_EmpSalStruct = db.EmpSalStruct.Where(e => e.Id == OEmpSalStruct.Id).SingleOrDefault();







                            // Utility.DumpProcessStatus(LineNo: 1008);

                        }
                        else if (OEmpSalStruct.EffectiveDate == mEffectiveDate)
                        {
                            EmployeeSalaryStructUpdateNew(OEmpSalStruct, OEmployeePayroll, mEffectiveDate);
                            OEmployeePayroll.EmpSalStruct.Add(OEmpSalStruct);

                            db.SaveChanges();
                            // Utility.DumpProcessStatus(LineNo: 1016);

                        }
                        else if (OEmpSalStruct.EffectiveDate > mEffectiveDate)
                        {

                            List<EmpSalStruct> OSalStructList = db.EmpSalStruct.Where(e => e.EffectiveDate >= mEffectiveDate && e.EmployeePayroll.Id == OEmployeePayroll.Id).ToList();


                            //.Include(e => e.GeoStruct)
                            //    .Include(e => e.FuncStruct)
                            //.Include(e => e.PayStruct)
                            //.Include(e => e.EmpSalStructDetails)
                            //.Include(e => e.EmpSalStructDetails.Select(z => z.SalaryHead))
                            // .Include(e => e.EmpSalStructDetails.Select(z => z.SalaryHead.RoundingMethod))

                            foreach (var OSalStructListitem in OSalStructList)
                            {

                                //EmpSalStruct OEmpSalStruct1 = db.EmpSalStruct.Where(e => e.Id == OSalStructListitem.Id).FirstOrDefault();

                                GeoStruct GeoStruct1 = db.GeoStruct.Where(e => e.Id == OSalStructListitem.GeoStruct_Id).SingleOrDefault();
                                OSalStructListitem.GeoStruct = GeoStruct1;
                                FuncStruct FuncStruct1 = db.FuncStruct.Where(e => e.Id == OSalStructListitem.FuncStruct_Id).SingleOrDefault();
                                OSalStructListitem.FuncStruct = FuncStruct1;
                                PayStruct PayStruct1 = db.PayStruct.Where(e => e.Id == OSalStructListitem.PayStruct_Id).SingleOrDefault();
                                OSalStructListitem.PayStruct = PayStruct1;
                                List<EmpSalStructDetails> EmpSalStructDetailsList1 = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == OSalStructListitem.Id).ToList();
                                OEmpSalStruct.EmpSalStructDetails = EmpSalStructDetailsList1;
                                foreach (var EmpSalStructDetailsitem in EmpSalStructDetailsList1)
                                {
                                    SalaryHead SalaryHead1 = db.SalaryHead.Include(e => e.RoundingMethod).Include(e => e.SalHeadOperationType).Where(e => e.Id == EmpSalStructDetailsitem.SalaryHead_Id).SingleOrDefault();
                                    EmpSalStructDetailsitem.SalaryHead = SalaryHead1;
                                    LookupValue RoundingMethod = db.LookupValue.Where(e => e.Id == SalaryHead1.RoundingMethod_Id).SingleOrDefault();
                                    SalaryHead1.RoundingMethod = RoundingMethod;

                                }

                            }

                            foreach (var ca in OSalStructList)
                            {
                                EmployeeSalaryStructUpdateNew(ca, OEmployeePayroll, ca.EffectiveDate);
                                OEmployeePayroll.EmpSalStruct.Add(ca);

                                db.SaveChanges();
                                //   Utility.DumpProcessStatus(LineNo: 1029);

                            }

                        }
                    }
                    // Da percentage update Start
                    //bool OCPIAppl = db.EmployeePayroll.Include(e => e.Employee.EmpOffInfo).Include(e => e.Employee.EmpOffInfo.PayScale).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault().Employee.EmpOffInfo.PayScale.CPIAppl;
                    var mEmpOffId = db.Employee.Where(e => e.Id == OEmployeePayroll.Employee_Id).Select(r => r.EmpOffInfo_Id).SingleOrDefault();
                    bool OCPIAppl = db.EmpOff.Where(e => e.Id == mEmpOffId).Select(e => e.PayScale.CPIAppl).SingleOrDefault();

                    if (OCPIAppl == false)
                    {
                        var dapermonth = new BasicLinkedDA();
                        dapermonth = db.BasicLinkedDA.Where(e => e.EffectiveFrom <= mEffectiveDate && mEffectiveDate <= e.EffectiveTo).OrderByDescending(x => x.Id).FirstOrDefault();
                        if (dapermonth != null)
                        {
                            var DAList = OEmployeePayroll.EmpSalStruct.Where(e => e.EffectiveDate >= dapermonth.EffectiveFrom && e.EffectiveDate <= dapermonth.EffectiveTo).ToList();
                            if (DAList != null)
                            {
                                foreach (var Struct in DAList)
                                {
                                    //var DAListDet = db.EmpSalStruct.Where(e => e.Id == Struct.Id).SingleOrDefault();
                                    List<EmpSalStructDetails> EmpSalStructDetails2 = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == Struct.Id).ToList();
                                    Struct.EmpSalStructDetails = EmpSalStructDetails2;
                                    foreach (var EmpSalStructDetails2item in EmpSalStructDetails2)
                                    {
                                        SalaryHead SalaryHead2 = db.SalaryHead.Where(e => e.Id == EmpSalStructDetails2item.SalaryHead_Id).SingleOrDefault();
                                        EmpSalStructDetails2item.SalaryHead = SalaryHead2;
                                        SalHeadFormula SalHeadFormula2 = db.SalHeadFormula.Include(x => x.PercentDependRule).Where(e => e.Id == EmpSalStructDetails2item.SalHeadFormula_Id).SingleOrDefault();
                                        EmpSalStructDetails2item.SalHeadFormula = SalHeadFormula2;
                                        if (SalHeadFormula2 != null)
                                        {
                                            PercentDependRule PercentDependRule = db.PercentDependRule.Where(e => e.Id == SalHeadFormula2.PercentDependRule_Id).SingleOrDefault();
                                            SalHeadFormula2.PercentDependRule = PercentDependRule;

                                        }
                                        LookupValue SalHeadOperationType2 = db.LookupValue.Where(e => e.Id == SalaryHead2.SalHeadOperationType_Id).SingleOrDefault();
                                        SalaryHead2.SalHeadOperationType = SalHeadOperationType2;

                                    }


                                    var Rule = Struct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "DA" && e.SalHeadFormula != null).Select(r => r.SalHeadFormula.PercentDependRule).FirstOrDefault();
                                    if (Rule != null)
                                    {
                                        int PerId = Rule.Id;
                                        PercentDependRule PercDependRule = db.PercentDependRule
                                               .Where(e => e.Id == PerId).SingleOrDefault();
                                        PercDependRule.SalPercent = Convert.ToDouble(dapermonth.DAPoint);
                                        db.PercentDependRule.Attach(PercDependRule);
                                        db.Entry(PercDependRule).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }

                                }
                            }


                        }
                    }
                    // Da percentage update End
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            //return null;
        }

        public static void EmployeeSalaryStructCreationWithUpdation(int OEmpSalStructId, Employee OEmployee,
            PayScaleAgreement OPayScaleAgreement, DateTime mEffectiveDate, EmployeePayroll OEmployeePayroll)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };


                int CompId = Convert.ToInt32(SessionManager.CompanyId);
                var OCompanyPayroll = db.CompanyPayroll.Where(e => e.Id == CompId).SingleOrDefault();
                List<PayScaleAssignment> PayScaleAssignment = db.PayScaleAssignment.Where(e => e.CompanyPayroll_Id == OCompanyPayroll.Id).ToList();
                OCompanyPayroll.PayScaleAssignment = PayScaleAssignment;
                foreach (var PayScaleAssignmentitem in PayScaleAssignment)
                {
                    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == PayScaleAssignmentitem.SalaryHead_Id).SingleOrDefault();
                    PayScaleAssignmentitem.SalaryHead = SalaryHead;
                    LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                    SalaryHead.SalHeadOperationType = SalHeadOperationType;
                    List<SalHeadFormula> SalHeadFormula = db.PayScaleAssignment.Where(e => e.Id == PayScaleAssignmentitem.Id).Select(r => r.SalHeadFormula.ToList()).SingleOrDefault();
                    PayScaleAssignmentitem.SalHeadFormula = SalHeadFormula;

                }

                var OPayScaleAssignment = OCompanyPayroll.PayScaleAssignment.ToList();


                List<EmpSalStructDetails> OEmpSalStructDetails = new List<EmpSalStructDetails>();
                int Count = 0;
                //EmpSalStruct OEmpSalStruct = db.EmpSalStruct

                //    .Include(e => e.EmpSalStructDetails)
                //    .Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead)).Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.RoundingMethod))
                //    .Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.PayStruct).Where(e => e.Id == OEmpSalStructId).SingleOrDefault();//OEmployeePayroll.EmpSalStruct.Where(e => e.EffectiveDate == mEffectiveDate).SingleOrDefault(); 
                EmpSalStruct OEmpSalStruct = db.EmpSalStruct.Where(e => e.Id == OEmpSalStructId).SingleOrDefault();
                GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OEmpSalStruct.GeoStruct_Id).SingleOrDefault();
                OEmpSalStruct.GeoStruct = GeoStruct;
                FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == OEmpSalStruct.FuncStruct_Id).SingleOrDefault();
                OEmpSalStruct.FuncStruct = FuncStruct;
                PayStruct PayStruct = db.PayStruct.Where(e => e.Id == OEmpSalStruct.PayStruct_Id).SingleOrDefault();
                OEmpSalStruct.PayStruct = PayStruct;
                List<EmpSalStructDetails> EmpSalStructDetailsList = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == OEmpSalStruct.Id).ToList();
                OEmpSalStruct.EmpSalStructDetails = EmpSalStructDetailsList;
                foreach (var EmpSalStructDetailsitem in EmpSalStructDetailsList)
                {
                    SalaryHead SalaryHead = db.SalaryHead.Include(e => e.RoundingMethod).Where(e => e.Id == EmpSalStructDetailsitem.SalaryHead_Id).SingleOrDefault();
                    EmpSalStructDetailsitem.SalaryHead = SalaryHead;
                    //LookupValue RoundingMethod = db.LookupValue.Include(e => e.Id).Where(e => e.Id == SalaryHead.RoundingMethod_Id).SingleOrDefault();
                    //SalaryHead.RoundingMethod = RoundingMethod;

                }


                foreach (var a in OEmpSalStruct.EmpSalStructDetails.ToList())
                {
                    if (a.PayScaleAssignment == null)
                    {
                        OEmpSalStruct.EmpSalStructDetails.Remove(a);
                    }
                }

                foreach (var a in OPayScaleAssignment.ToList())
                {

                    foreach (var b in OEmpSalStruct.EmpSalStructDetails)
                    {
                        if (a.SalaryHead.Id == b.SalaryHead.Id)
                        {
                            Count = 0;
                            break;
                        }
                        Count = 1;
                    }
                    if (Count == 1)
                    {
                        if (a.SalaryHead.Code.ToUpper() == "VPF" && !OEmployee.EmpOffInfo.VPFAppl)
                        {
                            continue;
                        }
                        EmpSalStructDetails OEmpSalStructDetailsObj = new EmpSalStructDetails();
                        {
                            if (a != null)
                            {
                                PayScaleAssignment PayScaleAssignment3 = db.PayScaleAssignment.Where(e => e.Id == a.Id).FirstOrDefault();
                                OEmpSalStructDetailsObj.PayScaleAssignment = PayScaleAssignment3;
                                List<SalHeadFormula> SalHeadFormula = db.PayScaleAssignment.Where(e => e.Id == PayScaleAssignment3.Id).Select(e => e.SalHeadFormula.ToList()).SingleOrDefault();
                                PayScaleAssignment3.SalHeadFormula = SalHeadFormula;
                                foreach (var SalHeadFormulaitem in SalHeadFormula)
                                {
                                    GeoStruct GeoStruct3 = db.GeoStruct.Where(e => e.Id == SalHeadFormulaitem.GeoStruct_Id).SingleOrDefault();
                                    SalHeadFormulaitem.GeoStruct = GeoStruct3;
                                    PayStruct PayStruct3 = db.PayStruct.Where(e => e.Id == SalHeadFormulaitem.PayStruct_Id).SingleOrDefault();
                                    SalHeadFormulaitem.PayStruct = PayStruct3;
                                    FuncStruct FuncStruct3 = db.FuncStruct.Where(e => e.Id == SalHeadFormulaitem.FuncStruct_Id).SingleOrDefault();
                                    SalHeadFormulaitem.FuncStruct = FuncStruct3;

                                }


                            }
                            if (a.SalaryHead != null)
                            {
                                OEmpSalStructDetailsObj.SalaryHead = db.SalaryHead.Where(e => e.Id == a.SalaryHead.Id).SingleOrDefault();
                            }
                            if (OEmpSalStructDetailsObj.SalaryHead != null && OEmpSalStructDetailsObj.PayScaleAssignment.SalHeadFormula != null)//newly added by prashant on 24042017
                            {
                                SalHeadFormula Salformula = new SalHeadFormula();

                                Salformula = SalFormulaFinderNew(OEmpSalStruct, OEmpSalStructDetailsObj.PayScaleAssignment, a.SalaryHead.Id);
                                OEmpSalStructDetailsObj.SalHeadFormula = Salformula == null ? Salformula : db.SalHeadFormula.Where(e => e.Id == Salformula.Id).SingleOrDefault();
                            } //added by Rekha 13072017

                            OEmpSalStructDetailsObj.DBTrack = dbt;
                            //if (a.SalHeadFormula != null && a.SalHeadFormula.Count > 0)
                            if (OEmpSalStructDetailsObj.SalHeadFormula != null)//updated by Rohit
                            {

                                if (OEmpSalStructDetailsObj.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "BASIC")
                                {
                                    double SalAmount = SalHeadAmountCalc(OEmpSalStructDetailsObj.SalHeadFormula.Id, null, OEmpSalStructDetails, OEmployeePayroll, OEmpSalStruct.EffectiveDate.Value.ToString("MM/yyyy"));

                                    SalAmount = RoundingFunction(OEmpSalStructDetailsObj.SalaryHead, SalAmount);
                                    //child alllowance start

                                    if (OEmpSalStructDetailsObj.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "CHILD")
                                    {
                                        Boolean Childap = Child(OEmployeePayroll, mEffectiveDate);
                                        if (Childap == false)
                                        {
                                            SalAmount = 0;
                                        }
                                    }
                                    //child alllowance end

                                    OEmpSalStructDetailsObj.Amount = SalAmount;
                                }
                            } //added by Rekha 13072017
                            else
                            {
                                OEmpSalStructDetailsObj.Amount = 0;
                            }
                            ////OEmpSalStructDetailsObj.Amount = 0;
                        };

                        OEmpSalStructDetails.Add(OEmpSalStructDetailsObj);
                    }
                    // }
                }
                db.EmpSalStructDetails.AddRange(OEmpSalStructDetails);
                OEmpSalStructDetails.AddRange(OEmpSalStruct.EmpSalStructDetails);
                OEmpSalStruct.EmpSalStructDetails = OEmpSalStructDetails;

                db.EmpSalStruct.Attach(OEmpSalStruct);
                db.Entry(OEmpSalStruct).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                EmployeeSalaryStructUpdateNew(OEmpSalStruct, OEmployeePayroll, mEffectiveDate);
                OEmployeePayroll.EmpSalStruct.Add(OEmpSalStruct);

                db.SaveChanges();

            }

        }

        public static void EmployeeSalaryStructCreationWithUpdationTest(int OEmpSalStructId, int OEmployeeId,
           int OPayScaleAgreementId, DateTime mEffectiveDate, int OEmployeePayroll_Id)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                    int CompId = Convert.ToInt32(SessionManager.CompanyId);


                    //var OCompanyPayroll = db.CompanyPayroll.Include(e => e.PayScaleAssignment)
                    //  .Include(e => e.PayScaleAssignment.Select(r => r.SalaryHead))
                    //  .Include(e => e.PayScaleAssignment.Select(r => r.SalaryHead.SalHeadOperationType))
                    //  .Include(e => e.PayScaleAssignment.Select(r => r.SalHeadFormula)).Where(e => e.Id == CompId).SingleOrDefault();
                    //var OPayScaleAssignment = OCompanyPayroll.PayScaleAssignment.ToList();

                    var OPayScaleAssignment = db.PayScaleAssignment
                            .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                           .Where(p => p.p.PayScaleAgreement.Id == OPayScaleAgreementId && p.p.CompanyPayroll.Id == CompId)
                           .Select(m => new
                           {
                               PayScaleAssignment = m.p,
                               SalHeadOperationType = m.p.SalaryHead.SalHeadOperationType,
                               Id = m.p.Id, // or m.ppc.pc.ProdId
                               Salheadformula = m.p.SalHeadFormula.Select(t => new SalHeadFormulaT
                               {
                                   GeoStruct = t.GeoStruct,
                                   PayStruct = t.PayStruct,
                                   FuncStruct = t.FuncStruct,
                                   FormulaType = t.FormulaType,
                                   Name = t.Name,
                                   Id = t.Id,
                               }),
                               SalaryHead = m.p.SalaryHead
                               // other assignments
                           }).ToList();


                    EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll_Id).OrderBy(e => e.Id).SingleOrDefault();
                    Employee OEmployee = db.Employee.Where(e => e.Id == OEmployeePayroll.Employee_Id).SingleOrDefault();
                    OEmployeePayroll.Employee = OEmployee;
                    EmpOff EmpOffInfo = db.EmpOff.Where(e => e.Id == OEmployee.EmpOffInfo_Id).SingleOrDefault();
                    OEmployee.EmpOffInfo = EmpOffInfo;
                    PayScale PayScale = db.PayScale.Where(e => e.Id == EmpOffInfo.PayScale_Id).SingleOrDefault();
                    EmpOffInfo.PayScale = PayScale;
                    ServiceBookDates ServiceBookDates = db.ServiceBookDates.Where(e => e.Id == OEmployee.ServiceBookDates_Id).SingleOrDefault();
                    OEmployee.ServiceBookDates = ServiceBookDates;
                    List<EmpSalStruct> EmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id).ToList();
                    OEmployeePayroll.EmpSalStruct = EmpSalStruct;


                    List<EmpSalStructDetails> OEmpSalStructDetails = new List<EmpSalStructDetails>();
                    List<EmpSalStructDetails> OEmpSalStructDetailsNew = new List<EmpSalStructDetails>();
                    int Count = 0;
                    EmpSalStruct OEmpSalStruct = db.EmpSalStruct.Where(e => e.Id == OEmpSalStructId).FirstOrDefault();
                    FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == OEmpSalStruct.FuncStruct_Id).SingleOrDefault();
                    OEmpSalStruct.FuncStruct = FuncStruct;
                    GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OEmpSalStruct.GeoStruct_Id).SingleOrDefault();
                    OEmpSalStruct.GeoStruct = GeoStruct;
                    PayStruct PayStruct = db.PayStruct.Where(e => e.Id == OEmpSalStruct.PayStruct_Id).SingleOrDefault();
                    OEmpSalStruct.PayStruct = PayStruct;

                    OEmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == OEmpSalStruct.Id).ToList();
                    OEmpSalStruct.EmpSalStructDetails = OEmpSalStructDetails;
                    foreach (var OEmpSalStructDetailsitem in OEmpSalStructDetails)
                    {
                        SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == OEmpSalStructDetailsitem.SalaryHead_Id).SingleOrDefault();
                        OEmpSalStructDetailsitem.SalaryHead = SalaryHead;
                        SalHeadFormula SalHeadFormula = db.SalHeadFormula.Where(e => e.Id == OEmpSalStructDetailsitem.SalHeadFormula_Id).SingleOrDefault();
                        OEmpSalStructDetailsitem.SalHeadFormula = SalHeadFormula;
                        LookupValue RoundingMethod = db.LookupValue.Where(e => e.Id == SalaryHead.RoundingMethod_Id).SingleOrDefault();
                        SalaryHead.RoundingMethod = RoundingMethod;


                    }


                    //foreach (var a in OEmpSalStruct.EmpSalStructDetails.ToList())
                    //{
                    //    if (a.PayScaleAssignment == null)
                    //    {
                    //        OEmpSalStruct.EmpSalStructDetails.Remove(a);
                    //    }
                    //}

                    OEmpSalStruct.EmpSalStructDetails.Where(x => x.PayScaleAssignment == null).ToList().ForEach(x =>
                    {
                        OEmpSalStruct.EmpSalStructDetails.Remove(x);
                    });
                    db.SaveChanges();

                    foreach (var a in OPayScaleAssignment)
                    {

                        foreach (var b in OEmpSalStruct.EmpSalStructDetails)
                        {
                            if (a.SalaryHead.Id == b.SalaryHead.Id)
                            {
                                Count = 0;
                                break;
                            }
                            Count = 1;
                        }
                        if (Count == 1)
                        {
                            if (a.SalaryHead.Code.ToUpper() == "VPF" && !OEmployee.EmpOffInfo.VPFAppl)
                            {
                                continue;
                            }
                            EmpSalStructDetails OEmpSalStructDetailsObj = new EmpSalStructDetails();
                            {
                                if (a != null)
                                {
                                    //    OEmpSalStructDetailsObj.PayScaleAssignment = db.PayScaleAssignment
                                    //        .Include(e => e.SalHeadFormula)
                                    //        .Include(e => e.SalHeadFormula.Select(z => z.GeoStruct))
                                    //        .Include(e => e.SalHeadFormula.Select(z => z.PayStruct))
                                    //        .Include(e => e.SalHeadFormula.Select(z => z.FuncStruct))
                                    //        .Include(e => e.SalHeadFormula.Select(z => z.FormulaType))
                                    //        .Where(e => e.Id == a.Id).FirstOrDefault();

                                    OEmpSalStructDetailsObj.PayScaleAssignment = db.PayScaleAssignment.Include(e => e.SalHeadFormula).Where(e => e.Id == a.Id).FirstOrDefault();
                                    //var PayScaleAssignment4= db.PayScaleAssignment.Where(e => e.Id == a.Id).FirstOrDefault();
                                    List<SalHeadFormula> SalHeadFormula = OEmpSalStructDetailsObj.PayScaleAssignment.SalHeadFormula.ToList(); //db.PayScaleAssignment.Include(e=>e.SalHeadFormula).Where(e => e.Id == PayScaleAssignment4.Id).Select(e => e.SalHeadFormula.ToList()).SingleOrDefault();
                                    //PayScaleAssignment4.SalHeadFormula = SalHeadFormula;
                                    foreach (var SalHeadFormulaitem in SalHeadFormula)
                                    {
                                        LookupValue FormulaType = db.SalHeadFormula.Where(e => e.Id == SalHeadFormulaitem.Id).Select(e => e.FormulaType).SingleOrDefault();
                                        SalHeadFormulaitem.FormulaType = FormulaType;
                                        GeoStruct GeoStruct3 = db.GeoStruct.Where(e => e.Id == SalHeadFormulaitem.GeoStruct_Id).SingleOrDefault();
                                        SalHeadFormulaitem.GeoStruct = GeoStruct3;
                                        PayStruct PayStruct3 = db.PayStruct.Where(e => e.Id == SalHeadFormulaitem.PayStruct_Id).SingleOrDefault();
                                        SalHeadFormulaitem.PayStruct = PayStruct3;
                                        FuncStruct FuncStruct3 = db.FuncStruct.Where(e => e.Id == SalHeadFormulaitem.FuncStruct_Id).SingleOrDefault();
                                        SalHeadFormulaitem.FuncStruct = FuncStruct3;

                                    }
                                }
                                if (a.SalaryHead != null)
                                {
                                    OEmpSalStructDetailsObj.SalaryHead = db.SalaryHead.Include(x => x.RoundingMethod).Where(e => e.Id == a.SalaryHead.Id).FirstOrDefault();
                                }
                                if (OEmpSalStructDetailsObj.SalaryHead != null && OEmpSalStructDetailsObj.PayScaleAssignment.SalHeadFormula != null)//newly added by prashant on 24042017
                                {
                                    SalHeadFormulaT Salformula = new SalHeadFormulaT();

                                    Salformula = SalFormulaFinderNewTest(OEmpSalStruct, a.Salheadformula.ToList(), a.SalaryHead.Id);

                                    OEmpSalStructDetailsObj.SalHeadFormula = Salformula == null ? null : db.SalHeadFormula.Where(e => e.Id == Salformula.Id).FirstOrDefault(); ;
                                } //added by Rekha 13072017

                                OEmpSalStructDetailsObj.DBTrack = dbt;
                                //if (a.SalHeadFormula != null && a.SalHeadFormula.Count > 0)
                                if (OEmpSalStructDetailsObj.SalHeadFormula != null)//updated by Rohit
                                {

                                    if (OEmpSalStructDetailsObj.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "BASIC")
                                    {

                                        double SalAmount = 0;
                                        if (OEmployee.EmpOffInfo.VPFAppl == true && OEmpSalStructDetailsObj.SalaryHead.Code.ToUpper() == "VPF")
                                        {
                                            SalAmount = SalHeadAmountCalc(OEmpSalStructDetailsObj.SalHeadFormula.Id, null, OEmpSalStructDetails, OEmployeePayroll, OEmpSalStruct.EffectiveDate.Value.ToString("MM/yyyy"), true);
                                        }
                                        else
                                        {

                                            SalAmount = SalHeadAmountCalc(OEmpSalStructDetailsObj.SalHeadFormula.Id, null, OEmpSalStructDetails, OEmployeePayroll, OEmpSalStruct.EffectiveDate.Value.ToString("MM/yyyy"), false);
                                        }


                                        SalAmount = RoundingFunction(OEmpSalStructDetailsObj.SalaryHead, SalAmount);
                                        //child alllowance start

                                        if (OEmpSalStructDetailsObj.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "CHILD")
                                        {
                                            Boolean Childap = Child(OEmployeePayroll, mEffectiveDate);
                                            if (Childap == false)
                                            {
                                                SalAmount = 0;
                                            }
                                        }
                                        //child alllowance end
                                        OEmpSalStructDetailsObj.Amount = SalAmount;
                                    }
                                } //added by Rekha 13072017
                                else
                                {
                                    OEmpSalStructDetailsObj.Amount = 0;
                                }
                                ////OEmpSalStructDetailsObj.Amount = 0;
                            };

                            OEmpSalStructDetailsNew.Add(OEmpSalStructDetailsObj);
                        }
                        // }
                    }
                    db.EmpSalStructDetails.AddRange(OEmpSalStructDetailsNew);
                    OEmpSalStructDetails.AddRange(OEmpSalStructDetailsNew);
                    OEmpSalStruct.EmpSalStructDetails = OEmpSalStructDetails;

                    db.EmpSalStruct.Attach(OEmpSalStruct);
                    db.Entry(OEmpSalStruct).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    EmployeeSalaryStructUpdateNew(OEmpSalStruct, OEmployeePayroll, mEffectiveDate);
                    OEmployeePayroll.EmpSalStruct.Add(OEmpSalStruct);

                    db.SaveChanges();
                }
                catch (Exception Ex)
                {
                    throw Ex;

                }

            }

        }

        public static void EmployeeSalaryStructCreationWithUpdateFormulaTest(int OEmpSalStructId, int OEmployee_Id,
           int PayScaleAgreementId, DateTime mEffectiveDate, int OEmployeePayroll_Id)
        {


            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                    int Count = 0;

                    EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll_Id).FirstOrDefault();
                    List<EmpSalStruct> EmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id).ToList();
                    OEmployeePayroll.EmpSalStruct = EmpSalStruct;
                    Employee OEmployee = db.Employee.Where(e => e.Id == OEmployeePayroll.Employee_Id).SingleOrDefault();
                    OEmployeePayroll.Employee = OEmployee;
                    EmpOff EmpOffInfo = db.EmpOff.Where(e => e.Id == OEmployee.EmpOffInfo_Id).SingleOrDefault();
                    OEmployee.EmpOffInfo = EmpOffInfo;
                    ServiceBookDates ServiceBookDates = db.ServiceBookDates.Where(e => e.Id == OEmployee.ServiceBookDates_Id).SingleOrDefault();
                    OEmployee.ServiceBookDates = ServiceBookDates;
                    PayScale PayScale = db.PayScale.Where(e => e.Id == EmpOffInfo.PayScale_Id).SingleOrDefault();
                    EmpOffInfo.PayScale = PayScale;

                    //.Include(e => e.Employee.GeoStruct)
                    //.Include(e => e.Employee.PayStruct)
                    //.Include(e => e.Employee.FuncStruct)

                    //var OPayScaleAssignment = db.PayScaleAssignment
                    //    .Include(e => e.SalaryHead)
                    //    .Include(e => e.SalaryHead.SalHeadOperationType)
                    //    .Include(e => e.SalHeadFormula)//added by prashant 13042017
                    //    .Include(e => e.SalHeadFormula.Select(r => r.FuncStruct))
                    //    .Include(e => e.SalHeadFormula.Select(r => r.PayStruct))
                    //    .Include(e => e.SalHeadFormula.Select(r => r.GeoStruct))
                    //    .Include(e => e.SalHeadFormula.Select(r => r.FormulaType))
                    //    .Where(e => e.PayScaleAgreement.Id == PayScaleAgreementId).ToList();

                    int CompId = Convert.ToInt32(SessionManager.CompanyId);

                    var OPayScaleAssignment = db.PayScaleAssignment
                            .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                           .Where(p => p.p.PayScaleAgreement.Id == PayScaleAgreementId && p.p.CompanyPayroll.Id == CompId && p.p.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "LOAN")
                           .Select(m => new
                           {
                               PayScaleAssignment = m.p,
                               SalHeadOperationType = m.p.SalaryHead.SalHeadOperationType,
                               Id = m.p.Id, // or m.ppc.pc.ProdId
                               Salheadformula = m.p.SalHeadFormula.Select(t => new SalHeadFormulaT
                               {
                                   GeoStruct = t.GeoStruct,
                                   PayStruct = t.PayStruct,
                                   FuncStruct = t.FuncStruct,
                                   FormulaType = t.FormulaType,
                                   Name = t.Name,
                                   Id = t.Id,
                               }),
                               SalaryHead = m.p.SalaryHead
                               // other assignments
                           }).ToList();

                    //Employee OEmployee = db.Employee
                    //    //.Include(e => e.GeoStruct)
                    //    //.Include(e => e.FuncStruct)
                    //    //.Include(e => e.PayStruct)
                    //                                   .Include(e => e.EmpOffInfo)
                    //                                    .Where(e => e.Id == OEmployeePayroll.Employee.Id).AsNoTracking().OrderBy(e => e.Id)
                    //                                    .FirstOrDefault();

                    List<EmpSalStructDetails> OEmpSalStructDetails = new List<EmpSalStructDetails>();

                    EmpSalStruct OEmpSalStruct = db.EmpSalStruct.Where(e => e.Id == OEmpSalStructId).FirstOrDefault();
                    OEmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == OEmpSalStruct.Id).ToList();
                    OEmpSalStruct.EmpSalStructDetails = OEmpSalStructDetails;
                    FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == OEmpSalStruct.FuncStruct_Id).SingleOrDefault();
                    OEmpSalStruct.FuncStruct = FuncStruct;
                    GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OEmpSalStruct.GeoStruct_Id).SingleOrDefault();
                    OEmpSalStruct.GeoStruct = GeoStruct;
                    PayStruct PayStruct = db.PayStruct.Where(e => e.Id == OEmpSalStruct.PayStruct_Id).SingleOrDefault();
                    OEmpSalStruct.PayStruct = PayStruct;
                    foreach (var OEmpSalStructDetailsItem in OEmpSalStructDetails)
                    {
                        SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == OEmpSalStructDetailsItem.SalaryHead_Id).SingleOrDefault();
                        OEmpSalStructDetailsItem.SalaryHead = SalaryHead;
                        SalHeadFormula SalHeadFormula = db.SalHeadFormula.Include(e => e.FormulaType).Where(e => e.Id == OEmpSalStructDetailsItem.SalHeadFormula_Id).SingleOrDefault();
                        OEmpSalStructDetailsItem.SalHeadFormula = SalHeadFormula;
                        //SalHeadFormula.FormulaType = SalHeadFormula.FormulaType;
                        LookupValue RoundingMethod = db.LookupValue.Where(e => e.Id == SalaryHead.RoundingMethod_Id).SingleOrDefault();
                        SalaryHead.RoundingMethod = RoundingMethod;
                        LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                        SalaryHead.SalHeadOperationType = SalHeadOperationType;
                        PayScaleAssignment PayScaleAssignment = db.PayScaleAssignment.Where(e => e.Id == OEmpSalStructDetailsItem.PayScaleAssignment_Id).SingleOrDefault();
                        OEmpSalStructDetailsItem.PayScaleAssignment = PayScaleAssignment;
                        PayScaleAgreement PayScaleAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAssignment.PayScaleAgreement_Id).SingleOrDefault();
                        PayScaleAssignment.PayScaleAgreement = PayScaleAgreement;
                    }

                    //.Include(e => e.EmpSalStructDetails)
                    //.Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead))
                    //.Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula.FormulaType))
                    //.Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.RoundingMethod))
                    //.Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.SalHeadOperationType))
                    // .Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment))
                    //  .Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.PayScaleAgreement))
                    //.Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.PayStruct)//OEmployeePayroll.EmpSalStruct.Where(e => e.EffectiveDate == mEffectiveDate).SingleOrDefault(); 

                    OEmpSalStructDetails = OEmpSalStruct.EmpSalStructDetails.Where(z => z.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "LOAN").ToList();
                    foreach (var a in OPayScaleAssignment.OrderBy(e => e.SalaryHead.SeqNo))
                    {
                        foreach (var b in OEmpSalStruct.EmpSalStructDetails.Where(x => x.SalaryHead.Id == a.SalaryHead.Id).OrderBy(e => e.SalaryHead.SeqNo))
                        {
                            if (a.SalaryHead.Id == b.SalaryHead.Id)
                            {
                                Count = 0;
                                /* added by bhagesh */
                                if (a.SalaryHead.Code.ToUpper() == "VPF" && !OEmployee.EmpOffInfo.VPFAppl)
                                {
                                    EmpSalStructDetails OEmpSalStructDetailsObj = db.EmpSalStructDetails.Where(e => e.Id == b.Id).SingleOrDefault();
                                    OEmpSalStructDetailsObj.SalHeadFormula = null;
                                    OEmpSalStructDetailsObj.Amount = 0;
                                    db.EmpSalStructDetails.Attach(OEmpSalStructDetailsObj);
                                    db.Entry(OEmpSalStructDetailsObj).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    break;
                                }
                                //end / 
                                var Salformula = SalFormulaFinderNewTest(OEmpSalStruct, a.Salheadformula.ToList(), b.SalaryHead.Id);
                                if (Salformula != null && b.SalHeadFormula != null)
                                {
                                    if (b.SalHeadFormula.Id == Salformula.Id)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        EmpSalStructDetails OEmpSalStructDetailsObj = db.EmpSalStructDetails.Where(e => e.Id == b.Id).SingleOrDefault();
                                        OEmpSalStructDetailsObj.SalHeadFormula = Salformula == null ? null : db.SalHeadFormula.Where(e => e.Id == Salformula.Id).FirstOrDefault();
                                        if (OEmpSalStructDetailsObj.SalHeadFormula != null)//updated by Rohit
                                        {
                                            if (OEmpSalStructDetailsObj.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "BASIC")
                                            {
                                                double SalAmount = 0;
                                                if (OEmployee.EmpOffInfo.VPFAppl == true && OEmpSalStructDetailsObj.SalaryHead.Code.ToUpper() == "VPF")
                                                {
                                                    SalAmount = SalHeadAmountCalc(OEmpSalStructDetailsObj.SalHeadFormula.Id, null, OEmpSalStructDetails, OEmployeePayroll, OEmpSalStruct.EffectiveDate.Value.ToString("MM/yyyy"), true);
                                                }
                                                else
                                                {

                                                    SalAmount = SalHeadAmountCalc(OEmpSalStructDetailsObj.SalHeadFormula.Id, null, OEmpSalStructDetails, OEmployeePayroll, OEmpSalStruct.EffectiveDate.Value.ToString("MM/yyyy"), false);
                                                }

                                                SalAmount = RoundingFunction(OEmpSalStructDetailsObj.SalaryHead, SalAmount);
                                                //child alllowance start

                                                if (OEmpSalStructDetailsObj.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "CHILD")
                                                {
                                                    Boolean Childap = Child(OEmployeePayroll, mEffectiveDate);
                                                    if (Childap == false)
                                                    {
                                                        SalAmount = 0;
                                                    }
                                                }
                                                //child alllowance end
                                                OEmpSalStructDetailsObj.Amount = SalAmount;
                                            }
                                        } //added by Rekha 13072017
                                        else
                                        {
                                            OEmpSalStructDetailsObj.Amount = 0;
                                        }
                                        db.EmpSalStructDetails.Attach(OEmpSalStructDetailsObj);
                                        db.Entry(OEmpSalStructDetailsObj).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                }
                                else if (Salformula == null && b.SalHeadFormula != null)
                                {
                                    EmpSalStructDetails OEmpSalStructDetailsObj = db.EmpSalStructDetails.Where(e => e.Id == b.Id).SingleOrDefault();
                                    OEmpSalStructDetailsObj.SalHeadFormula = Salformula == null ? null : db.SalHeadFormula.Where(e => e.Id == Salformula.Id).FirstOrDefault();
                                    if (OEmpSalStructDetailsObj.SalHeadFormula != null)//updated by Rohit
                                    {
                                        if (OEmpSalStructDetailsObj.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "BASIC")
                                        {
                                            double SalAmount = 0;
                                            if (OEmployee.EmpOffInfo.VPFAppl == true && OEmpSalStructDetailsObj.SalaryHead.Code.ToUpper() == "VPF")
                                            {
                                                SalAmount = SalHeadAmountCalc(OEmpSalStructDetailsObj.SalHeadFormula.Id, null, OEmpSalStructDetails, OEmployeePayroll, OEmpSalStruct.EffectiveDate.Value.ToString("MM/yyyy"), true);
                                            }
                                            else
                                            {
                                                SalAmount = SalHeadAmountCalc(OEmpSalStructDetailsObj.SalHeadFormula.Id, null, OEmpSalStructDetails, OEmployeePayroll, OEmpSalStruct.EffectiveDate.Value.ToString("MM/yyyy"), false);
                                            }
                                            SalAmount = RoundingFunction(OEmpSalStructDetailsObj.SalaryHead, SalAmount);
                                            //child alllowance start

                                            if (OEmpSalStructDetailsObj.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "CHILD")
                                            {
                                                Boolean Childap = Child(OEmployeePayroll, mEffectiveDate);
                                                if (Childap == false)
                                                {
                                                    SalAmount = 0;
                                                }
                                            }
                                            //child alllowance end
                                            OEmpSalStructDetailsObj.Amount = SalAmount;
                                        }
                                    } //added by Rekha 13072017
                                    else
                                    {
                                        OEmpSalStructDetailsObj.Amount = 0;
                                    }
                                    db.EmpSalStructDetails.Attach(OEmpSalStructDetailsObj);
                                    db.Entry(OEmpSalStructDetailsObj).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else if (Salformula != null && b.SalHeadFormula == null)
                                {
                                    if (Salformula.FormulaType.LookupVal.ToUpper() == "NONSTANDARDFORMULA" && b.SalHeadFormula == null)
                                    {

                                    }
                                    else
                                    {
                                        EmpSalStructDetails OEmpSalStructDetailsObj = db.EmpSalStructDetails.Where(e => e.Id == b.Id).SingleOrDefault();
                                        OEmpSalStructDetailsObj.SalHeadFormula = Salformula == null ? null : db.SalHeadFormula.Where(e => e.Id == Salformula.Id).FirstOrDefault();
                                        if (OEmpSalStructDetailsObj.SalHeadFormula != null)//updated by Rohit
                                        {
                                            if (OEmpSalStructDetailsObj.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "BASIC")
                                            {
                                                double SalAmount = 0;
                                                if (OEmployee.EmpOffInfo.VPFAppl == true && OEmpSalStructDetailsObj.SalaryHead.Code.ToUpper() == "VPF")
                                                {
                                                    SalAmount = SalHeadAmountCalc(OEmpSalStructDetailsObj.SalHeadFormula.Id, null, OEmpSalStructDetails, OEmployeePayroll, OEmpSalStruct.EffectiveDate.Value.ToString("MM/yyyy"), true);
                                                }
                                                else
                                                {
                                                    SalAmount = SalHeadAmountCalc(OEmpSalStructDetailsObj.SalHeadFormula.Id, null, OEmpSalStructDetails, OEmployeePayroll, OEmpSalStruct.EffectiveDate.Value.ToString("MM/yyyy"), false);
                                                }
                                                SalAmount = RoundingFunction(OEmpSalStructDetailsObj.SalaryHead, SalAmount);
                                                //child alllowance start

                                                if (OEmpSalStructDetailsObj.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "CHILD")
                                                {
                                                    Boolean Childap = Child(OEmployeePayroll, mEffectiveDate);
                                                    if (Childap == false)
                                                    {
                                                        SalAmount = 0;
                                                    }
                                                }
                                                //child alllowance end
                                                OEmpSalStructDetailsObj.Amount = SalAmount;
                                            }
                                        } //added by Rekha 13072017
                                        else
                                        {
                                            OEmpSalStructDetailsObj.Amount = 0;
                                        }

                                        db.EmpSalStructDetails.Attach(OEmpSalStructDetailsObj);
                                        db.Entry(OEmpSalStructDetailsObj).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                }

                            }
                            Count = 1;
                        }

                    }

                }
                catch (Exception Ex)
                {

                    throw Ex;
                }

            }

        }

        public static void EmployeeSalaryStructCreationWithUpdateFormula(int OEmpSalStructId, Employee OEmployee,
            PayScaleAgreement OPayScaleAgreement, DateTime mEffectiveDate, EmployeePayroll OEmployeePayroll)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                var OPayScaleAssignment = db.PayScaleAssignment.Where(e => e.PayScaleAgreement.Id == OPayScaleAgreement.Id).ToList();
                foreach (var OPayScaleAssignmentitem in OPayScaleAssignment)
                {
                    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentitem.SalaryHead_Id).SingleOrDefault();
                    OPayScaleAssignmentitem.SalaryHead = SalaryHead;
                    LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                    SalaryHead.SalHeadOperationType = SalHeadOperationType;
                    List<SalHeadFormula> SalHeadFormula = db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentitem.Id).Select(e => e.SalHeadFormula.ToList()).SingleOrDefault();
                    OPayScaleAssignmentitem.SalHeadFormula = SalHeadFormula;

                }

                //.Include(e => e.SalaryHead)
                //.Include(e => e.SalaryHead.SalHeadOperationType)
                //.Include(e => e.SalHeadFormula)//added by prashant 13042017


                List<EmpSalStructDetails> OEmpSalStructDetails = new List<EmpSalStructDetails>();
                int Count = 0;
                //EmpSalStruct OEmpSalStruct = db.EmpSalStruct.Include(e => e.EmpSalStructDetails)
                //    .Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead)).Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.RoundingMethod))
                //    .Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.PayStruct).Where(e => e.Id == OEmpSalStructId).SingleOrDefault();//OEmployeePayroll.EmpSalStruct.Where(e => e.EffectiveDate == mEffectiveDate).SingleOrDefault(); 

                EmpSalStruct OEmpSalStruct = db.EmpSalStruct.Where(e => e.Id == OEmpSalStructId).FirstOrDefault();
                OEmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == OEmpSalStruct.Id).ToList();
                OEmpSalStruct.EmpSalStructDetails = OEmpSalStructDetails;
                FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == OEmpSalStruct.FuncStruct_Id).SingleOrDefault();
                OEmpSalStruct.FuncStruct = FuncStruct;
                GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OEmpSalStruct.GeoStruct_Id).SingleOrDefault();
                OEmpSalStruct.GeoStruct = GeoStruct;
                PayStruct PayStruct = db.PayStruct.Where(e => e.Id == OEmpSalStruct.PayStruct_Id).SingleOrDefault();
                OEmpSalStruct.PayStruct = PayStruct;
                foreach (var OEmpSalStructDetailsItem in OEmpSalStructDetails)
                {
                    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == OEmpSalStructDetailsItem.SalaryHead_Id).SingleOrDefault();
                    OEmpSalStructDetailsItem.SalaryHead = SalaryHead;
                    LookupValue RoundingMethod = db.LookupValue.Where(e => e.Id == SalaryHead.RoundingMethod_Id).SingleOrDefault();
                    SalaryHead.RoundingMethod = RoundingMethod;

                }

                OEmpSalStructDetails = OEmpSalStruct.EmpSalStructDetails.ToList();
                foreach (var a in OPayScaleAssignment)
                {
                    foreach (var b in OEmpSalStruct.EmpSalStructDetails.OrderBy(e => e.SalaryHead.SeqNo))
                    {
                        if (a.SalaryHead.Id == b.SalaryHead.Id)
                        {
                            Count = 0;
                            var Salformula = SalFormulaFinderNew(OEmpSalStruct, b.PayScaleAssignment, b.SalaryHead.Id);
                            if (Salformula != null && b.SalHeadFormula != null)
                            {
                                if (b.SalHeadFormula.Id == Salformula.Id)
                                {
                                    break;
                                }
                                else
                                {
                                    EmpSalStructDetails OEmpSalStructDetailsObj = db.EmpSalStructDetails.Where(e => e.Id == b.Id).SingleOrDefault();
                                    OEmpSalStructDetailsObj.SalHeadFormula = Salformula == null ? Salformula : db.SalHeadFormula.Where(e => e.Id == Salformula.Id).SingleOrDefault();
                                    if (OEmpSalStructDetailsObj.SalHeadFormula != null)//updated by Rohit
                                    {
                                        if (OEmpSalStructDetailsObj.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "BASIC")
                                        {
                                            double SalAmount = SalHeadAmountCalc(OEmpSalStructDetailsObj.SalHeadFormula.Id, null, OEmpSalStructDetails, OEmployeePayroll, OEmpSalStruct.EffectiveDate.Value.ToString("MM/yyyy"));

                                            SalAmount = RoundingFunction(OEmpSalStructDetailsObj.SalaryHead, SalAmount);
                                            //child alllowance start

                                            if (OEmpSalStructDetailsObj.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "CHILD")
                                            {
                                                Boolean Childap = Child(OEmployeePayroll, mEffectiveDate);
                                                if (Childap == false)
                                                {
                                                    SalAmount = 0;
                                                }
                                            }
                                            //child alllowance end

                                            OEmpSalStructDetailsObj.Amount = SalAmount;
                                        }
                                    } //added by Rekha 13072017
                                    else
                                    {
                                        OEmpSalStructDetailsObj.Amount = 0;
                                    }
                                    db.EmpSalStructDetails.Attach(OEmpSalStructDetailsObj);
                                    db.Entry(OEmpSalStructDetailsObj).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            else if (Salformula == null && b.SalHeadFormula != null)
                            {
                                EmpSalStructDetails OEmpSalStructDetailsObj = db.EmpSalStructDetails.Where(e => e.Id == b.Id).SingleOrDefault();
                                OEmpSalStructDetailsObj.SalHeadFormula = Salformula == null ? Salformula : db.SalHeadFormula.Where(e => e.Id == Salformula.Id).SingleOrDefault();
                                if (OEmpSalStructDetailsObj.SalHeadFormula != null)//updated by Rohit
                                {
                                    if (OEmpSalStructDetailsObj.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "BASIC")
                                    {
                                        double SalAmount = SalHeadAmountCalc(OEmpSalStructDetailsObj.SalHeadFormula.Id, null, OEmpSalStructDetails, OEmployeePayroll, OEmpSalStruct.EffectiveDate.Value.ToString("MM/yyyy"));

                                        SalAmount = RoundingFunction(OEmpSalStructDetailsObj.SalaryHead, SalAmount);
                                        //child alllowance start

                                        if (OEmpSalStructDetailsObj.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "CHILD")
                                        {
                                            Boolean Childap = Child(OEmployeePayroll, mEffectiveDate);
                                            if (Childap == false)
                                            {
                                                SalAmount = 0;
                                            }
                                        }
                                        //child alllowance end

                                        OEmpSalStructDetailsObj.Amount = SalAmount;
                                    }
                                } //added by Rekha 13072017
                                else
                                {
                                    OEmpSalStructDetailsObj.Amount = 0;
                                }
                                db.EmpSalStructDetails.Attach(OEmpSalStructDetailsObj);
                                db.Entry(OEmpSalStructDetailsObj).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            else if (Salformula != null && b.SalHeadFormula == null)
                            {
                                EmpSalStructDetails OEmpSalStructDetailsObj = db.EmpSalStructDetails.Where(e => e.Id == b.Id).SingleOrDefault();
                                OEmpSalStructDetailsObj.SalHeadFormula = Salformula == null ? Salformula : db.SalHeadFormula.Where(e => e.Id == Salformula.Id).SingleOrDefault();
                                if (OEmpSalStructDetailsObj.SalHeadFormula != null)//updated by Rohit
                                {
                                    if (OEmpSalStructDetailsObj.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "BASIC")
                                    {
                                        double SalAmount = SalHeadAmountCalc(OEmpSalStructDetailsObj.SalHeadFormula.Id, null, OEmpSalStructDetails, OEmployeePayroll, OEmpSalStruct.EffectiveDate.Value.ToString("MM/yyyy"));

                                        SalAmount = RoundingFunction(OEmpSalStructDetailsObj.SalaryHead, SalAmount);
                                        //child alllowance start

                                        if (OEmpSalStructDetailsObj.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "CHILD")
                                        {
                                            Boolean Childap = Child(OEmployeePayroll, mEffectiveDate);
                                            if (Childap == false)
                                            {
                                                SalAmount = 0;
                                            }
                                        }
                                        //child alllowance end

                                        OEmpSalStructDetailsObj.Amount = SalAmount;
                                    }
                                } //added by Rekha 13072017
                                else
                                {
                                    OEmpSalStructDetailsObj.Amount = 0;
                                }

                                db.EmpSalStructDetails.Attach(OEmpSalStructDetailsObj);
                                db.Entry(OEmpSalStructDetailsObj).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }

                        }
                        Count = 1;
                    }

                }

            }

        }
        public static void EmployeeSalaryStructUpdateForCPI(EmployeePayroll OEmployeePayroll, DateTime? mEffectiveDate)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var OEmployeePayroll = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId).Include(e => e.EmpSalStruct)
                //         .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                //         .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                //         .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.RoundingMethod)))
                //          .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalHeadFormula)))
                //          .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                //        .SingleOrDefault();

                //var OEmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id && e.EffectiveDate == mEffectiveDate)
                //                         .Include(e => e.EmpSalStructDetails)
                //                         .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                //                         .Include(e => e.EmpSalStructDetails.Select(q => q.SalHeadFormula))
                //                         .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod))
                //                         .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType)).ToList();

                DateTime enddate = Convert.ToDateTime(mEffectiveDate).AddMonths(1).Date.AddDays(-1).Date;
                List<EmpSalStruct> OEmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.EffectiveDate >= mEffectiveDate && e.EffectiveDate <= enddate).ToList();
                foreach (var OEmpSalStructitem in OEmpSalStruct)
                {
                    List<EmpSalStructDetails> EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == OEmpSalStructitem.Id).ToList();


                    foreach (var OEmpSalStructDetailsItem in EmpSalStructDetails)
                    {
                        SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == OEmpSalStructDetailsItem.SalaryHead_Id).SingleOrDefault();
                        OEmpSalStructDetailsItem.SalaryHead = SalaryHead;
                        SalHeadFormula SalHeadFormula = db.SalHeadFormula.Where(e => e.Id == OEmpSalStructDetailsItem.SalHeadFormula_Id).SingleOrDefault();
                        OEmpSalStructDetailsItem.SalHeadFormula = SalHeadFormula;

                        LookupValue RoundingMethod = db.LookupValue.Where(e => e.Id == SalaryHead.RoundingMethod_Id).SingleOrDefault();
                        SalaryHead.RoundingMethod = RoundingMethod;
                        LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                        SalaryHead.SalHeadOperationType = SalHeadOperationType;

                    }
                    OEmpSalStructitem.EmpSalStructDetails = EmpSalStructDetails;
                }


                // Da percentage update Start
                //bool OCPIAppl = db.EmployeePayroll.Include(e => e.Employee.EmpOffInfo).Include(e => e.Employee.EmpOffInfo.PayScale).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault().Employee.EmpOffInfo.PayScale.CPIAppl;
                var mEmpOffId = db.Employee.Where(e => e.Id == OEmployeePayroll.Employee_Id).Select(r => r.EmpOffInfo_Id).SingleOrDefault();
                bool OCPIAppl = db.EmpOff.Where(e => e.Id == mEmpOffId).Select(e => e.PayScale.CPIAppl).SingleOrDefault();

                if (OCPIAppl == false)
                {
                    var dapermonth = new BasicLinkedDA();
                    dapermonth = db.BasicLinkedDA.Where(e => e.EffectiveFrom <= mEffectiveDate && mEffectiveDate <= e.EffectiveTo).OrderByDescending(x => x.Id).FirstOrDefault();
                    if (dapermonth != null)
                    {
                        //  var DAList = OEmployeePayroll.EmpSalStruct.Where(e => e.EffectiveDate >= dapermonth.EffectiveFrom && e.EffectiveDate <= dapermonth.EffectiveTo).ToList();
                        var DAList = OEmpSalStruct.Where(e => e.EffectiveDate >= dapermonth.EffectiveFrom && e.EffectiveDate <= dapermonth.EffectiveTo).ToList();
                        if (DAList != null)
                        {
                            foreach (var Struct in DAList)
                            {
                                var DAListDet = db.EmpSalStruct.Where(e => e.Id == Struct.Id).SingleOrDefault();
                                List<EmpSalStructDetails> EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == DAListDet.Id).ToList();
                                DAListDet.EmpSalStructDetails = EmpSalStructDetails;
                                foreach (var EmpSalStructDetailsitem in EmpSalStructDetails)
                                {
                                    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsitem.SalaryHead_Id).SingleOrDefault();
                                    EmpSalStructDetailsitem.SalaryHead = SalaryHead;
                                    LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                                    SalaryHead.SalHeadOperationType = SalHeadOperationType;
                                    SalHeadFormula SalHeadFormula = db.SalHeadFormula.Include(x => x.PercentDependRule).Where(e => e.Id == EmpSalStructDetailsitem.SalHeadFormula_Id).SingleOrDefault();
                                    EmpSalStructDetailsitem.SalHeadFormula = SalHeadFormula;
                                    if (SalHeadFormula != null)
                                    {
                                        PercentDependRule PercentDependRule = db.PercentDependRule.Where(e => e.Id == SalHeadFormula.PercentDependRule_Id).SingleOrDefault();
                                        SalHeadFormula.PercentDependRule = PercentDependRule;

                                    }



                                }


                                var Rule = DAListDet.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "DA" && e.SalHeadFormula != null).Select(r => r.SalHeadFormula.PercentDependRule).FirstOrDefault();
                                if (Rule != null)
                                {
                                    int PerId = Rule.Id;
                                    PercentDependRule PercDependRule = db.PercentDependRule
                                           .Where(e => e.Id == PerId).SingleOrDefault();
                                    PercDependRule.SalPercent = Convert.ToDouble(dapermonth.DAPoint);
                                    db.PercentDependRule.Attach(PercDependRule);
                                    db.Entry(PercDependRule).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }

                            }
                        }


                    }
                }
                // Da percentage update End

                //var OEmpSalStruct = QEmpSalStruct1.Where(e => e.EmployeePayroll.Id == OEmployeePayrollId && EntityFunctions.TruncateTime(e.EffectiveDate.Value) == mEffectiveDate.Value.Date)
                //                            .OrderBy(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.SeqNo)).ToList();


                //var OEmpSalStruct = db.EmpSalStruct
                //     .Include(e => e.EmpSalStructDetails)
                //         .Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead))
                //         .Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead.RoundingMethod))
                //          .Include(e => e.EmpSalStructDetails.Select(t => t.SalHeadFormula))
                //          .Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType))
                //      .Where(e => DbFunctions.TruncateTime(e.EffectiveDate.Value) == mEffectiveDate.Value.Date)
                //      .OrderBy(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.SeqNo)).ToList();

                foreach (var OEmpSalStructData in OEmpSalStruct)
                {
                    var OEmpSalHead = OEmpSalStructData.EmpSalStructDetails
                                         .OrderBy(e => e.SalaryHead.SeqNo).ToList();
                    foreach (var OSalStructDetails in OEmpSalHead)
                    {
                        if (OSalStructDetails.SalHeadFormula != null && OSalStructDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "BASIC")
                        {
                            // EmployeePayroll OEmployeePayroll = OEmployeePayroll;

                            EmployeePayroll ooEmployeePayroll = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                            Employee Employee = db.Employee.Where(e => e.Id == ooEmployeePayroll.Employee_Id).SingleOrDefault();
                            ooEmployeePayroll.Employee = Employee;
                            EmpOff EmpOffInfo = db.EmpOff.Where(e => e.Id == Employee.EmpOffInfo_Id).SingleOrDefault();
                            Employee.EmpOffInfo = EmpOffInfo;
                            ServiceBookDates ServiceBookDates = db.ServiceBookDates.Where(e => e.Id == Employee.ServiceBookDates_Id).SingleOrDefault();
                            Employee.ServiceBookDates = ServiceBookDates;
                            PayScale PayScale = db.PayScale.Where(e => e.Id == EmpOffInfo.PayScale_Id).SingleOrDefault();
                            EmpOffInfo.PayScale = PayScale;



                            double SalAmount = SalHeadAmountCalc(OSalStructDetails.SalHeadFormula.Id, null, OEmpSalHead, ooEmployeePayroll,
                                OEmpSalStructData.EffectiveDate.Value.ToString("MM/yyyy"), OSalStructDetails.SalaryHead.Code.ToUpper() == "VPF" ? true : false);
                            SalAmount = RoundingFunction(OSalStructDetails.SalaryHead, SalAmount);

                            //child alllowance start

                            if (OSalStructDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "CHILD")
                            {
                                Boolean Childap = Child(OEmployeePayroll, mEffectiveDate);
                                if (Childap == false)
                                {
                                    SalAmount = 0;
                                }
                            }
                            //child alllowance end

                            if (SalAmount != OSalStructDetails.Amount)
                            {
                                EmpSalStructDetails OSalStructDetailSave = OSalStructDetails;
                                //db.EmpSalStructDetails
                                //.Where(e => e.Id == OSalStructDetails.Id).SingleOrDefault();

                                OSalStructDetailSave.Amount = SalAmount;
                                OSalStructDetails.Amount = SalAmount;

                                try
                                {
                                    db.EmpSalStructDetails.Attach(OSalStructDetailSave);
                                    db.Entry(OSalStructDetailSave).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    LogFile Logfile = new LogFile();
                                    ErrorLog Err = new ErrorLog()
                                    {
                                        ControllerName = "EmployeeSalaryStructUpdate - SalaryHeadGenProcess",
                                        ExceptionMessage = ex.Message,
                                        ExceptionStackTrace = ex.StackTrace,
                                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                        LogTime = DateTime.Now
                                    };
                                    Logfile.CreateLogFile(Err);
                                }
                            }
                        }
                    }
                }
            }
        }


        public static void EmployeeSalaryStructUpdateForCPIVdaArrear(EmployeePayroll OEmployeePayroll, DateTime? mEffectiveDate)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime enddate = Convert.ToDateTime(mEffectiveDate).AddMonths(1).Date.AddDays(-1).Date;

                // List<EmpSalStruct> OEmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.EffectiveDate >= mEffectiveDate && e.EffectiveDate.Value.Date <= (Convert.ToDateTime(mEffectiveDate).AddMonths(1).Date.AddDays(-1).Date)).ToList();
                List<EmpSalStruct> OEmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.EffectiveDate >= mEffectiveDate && e.EffectiveDate <= enddate).ToList();
                foreach (var OEmpSalStructitem in OEmpSalStruct)
                {
                    List<EmpSalStructDetails> EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == OEmpSalStructitem.Id).ToList();


                    foreach (var OEmpSalStructDetailsItem in EmpSalStructDetails)
                    {
                        SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == OEmpSalStructDetailsItem.SalaryHead_Id).SingleOrDefault();
                        OEmpSalStructDetailsItem.SalaryHead = SalaryHead;
                        SalHeadFormula SalHeadFormula = db.SalHeadFormula.Where(e => e.Id == OEmpSalStructDetailsItem.SalHeadFormula_Id).SingleOrDefault();
                        OEmpSalStructDetailsItem.SalHeadFormula = SalHeadFormula;

                        LookupValue RoundingMethod = db.LookupValue.Where(e => e.Id == SalaryHead.RoundingMethod_Id).SingleOrDefault();
                        SalaryHead.RoundingMethod = RoundingMethod;
                        LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                        SalaryHead.SalHeadOperationType = SalHeadOperationType;

                    }
                    OEmpSalStructitem.EmpSalStructDetails = EmpSalStructDetails;
                }


                // Da percentage update Start
                //bool OCPIAppl = db.EmployeePayroll.Include(e => e.Employee.EmpOffInfo).Include(e => e.Employee.EmpOffInfo.PayScale).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault().Employee.EmpOffInfo.PayScale.CPIAppl;
                var mEmpOffId = db.Employee.Where(e => e.Id == OEmployeePayroll.Employee_Id).Select(r => r.EmpOffInfo_Id).SingleOrDefault();
                bool OCPIAppl = db.EmpOff.Where(e => e.Id == mEmpOffId).Select(e => e.PayScale.CPIAppl).SingleOrDefault();

                if (OCPIAppl == false)
                {
                    var dapermonth = new BasicLinkedDA();
                    dapermonth = db.BasicLinkedDA.Where(e => e.EffectiveFrom <= mEffectiveDate && mEffectiveDate <= e.EffectiveTo).OrderByDescending(x => x.Id).FirstOrDefault();
                    if (dapermonth != null)
                    {
                        //  var DAList = OEmployeePayroll.EmpSalStruct.Where(e => e.EffectiveDate >= dapermonth.EffectiveFrom && e.EffectiveDate <= dapermonth.EffectiveTo).ToList();
                        var DAList = OEmpSalStruct.Where(e => e.EffectiveDate >= dapermonth.EffectiveFrom && e.EffectiveDate <= dapermonth.EffectiveTo).ToList();
                        if (DAList != null)
                        {
                            foreach (var Struct in DAList)
                            {
                                var DAListDet = db.EmpSalStruct.Where(e => e.Id == Struct.Id).SingleOrDefault();
                                List<EmpSalStructDetails> EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == DAListDet.Id).ToList();
                                DAListDet.EmpSalStructDetails = EmpSalStructDetails;
                                foreach (var EmpSalStructDetailsitem in EmpSalStructDetails)
                                {
                                    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsitem.SalaryHead_Id).SingleOrDefault();
                                    EmpSalStructDetailsitem.SalaryHead = SalaryHead;
                                    LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                                    SalaryHead.SalHeadOperationType = SalHeadOperationType;
                                    SalHeadFormula SalHeadFormula = db.SalHeadFormula.Include(x => x.PercentDependRule).Where(e => e.Id == EmpSalStructDetailsitem.SalHeadFormula_Id).SingleOrDefault();
                                    EmpSalStructDetailsitem.SalHeadFormula = SalHeadFormula;
                                    if (SalHeadFormula != null)
                                    {
                                        PercentDependRule PercentDependRule = db.PercentDependRule.Where(e => e.Id == SalHeadFormula.PercentDependRule_Id).SingleOrDefault();
                                        SalHeadFormula.PercentDependRule = PercentDependRule;

                                    }



                                }


                                var Rule = DAListDet.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "DA" && e.SalHeadFormula != null).Select(r => r.SalHeadFormula.PercentDependRule).FirstOrDefault();
                                if (Rule != null)
                                {
                                    int PerId = Rule.Id;
                                    PercentDependRule PercDependRule = db.PercentDependRule
                                           .Where(e => e.Id == PerId).SingleOrDefault();
                                    PercDependRule.SalPercent = Convert.ToDouble(dapermonth.DAPoint);
                                    db.PercentDependRule.Attach(PercDependRule);
                                    db.Entry(PercDependRule).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }

                            }
                        }


                    }
                }
                // Da percentage update End


                foreach (var OEmpSalStructData in OEmpSalStruct)
                {
                    var OEmpSalHead = OEmpSalStructData.EmpSalStructDetails
                                         .OrderBy(e => e.SalaryHead.SeqNo).ToList();
                    foreach (var OSalStructDetails in OEmpSalHead)
                    {
                        if (OSalStructDetails.SalHeadFormula != null && OSalStructDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "BASIC")
                        {
                            // EmployeePayroll OEmployeePayroll = OEmployeePayroll;

                            EmployeePayroll ooEmployeePayroll = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                            Employee Employee = db.Employee.Where(e => e.Id == ooEmployeePayroll.Employee_Id).SingleOrDefault();
                            ooEmployeePayroll.Employee = Employee;
                            EmpOff EmpOffInfo = db.EmpOff.Where(e => e.Id == Employee.EmpOffInfo_Id).SingleOrDefault();
                            Employee.EmpOffInfo = EmpOffInfo;
                            ServiceBookDates ServiceBookDates = db.ServiceBookDates.Where(e => e.Id == Employee.ServiceBookDates_Id).SingleOrDefault();
                            Employee.ServiceBookDates = ServiceBookDates;
                            PayScale PayScale = db.PayScale.Where(e => e.Id == EmpOffInfo.PayScale_Id).SingleOrDefault();
                            EmpOffInfo.PayScale = PayScale;



                            double SalAmount = SalHeadAmountCalc(OSalStructDetails.SalHeadFormula.Id, null, OEmpSalHead, ooEmployeePayroll,
                                OEmpSalStructData.EffectiveDate.Value.ToString("MM/yyyy"), OSalStructDetails.SalaryHead.Code.ToUpper() == "VPF" ? true : false);
                            SalAmount = RoundingFunction(OSalStructDetails.SalaryHead, SalAmount);

                            //child alllowance start

                            if (OSalStructDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "CHILD")
                            {
                                Boolean Childap = Child(OEmployeePayroll, mEffectiveDate);
                                if (Childap == false)
                                {
                                    SalAmount = 0;
                                }
                            }
                            //child alllowance end

                            if (SalAmount != OSalStructDetails.Amount)
                            {
                                EmpSalStructDetails OSalStructDetailSave = OSalStructDetails;
                                //db.EmpSalStructDetails
                                //.Where(e => e.Id == OSalStructDetails.Id).SingleOrDefault();

                                OSalStructDetailSave.Amount = SalAmount;
                                OSalStructDetails.Amount = SalAmount;

                                try
                                {
                                    db.EmpSalStructDetails.Attach(OSalStructDetailSave);
                                    db.Entry(OSalStructDetailSave).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    LogFile Logfile = new LogFile();
                                    ErrorLog Err = new ErrorLog()
                                    {
                                        ControllerName = "EmployeeSalaryStructUpdate - SalaryHeadGenProcess",
                                        ExceptionMessage = ex.Message,
                                        ExceptionStackTrace = ex.StackTrace,
                                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                        LogTime = DateTime.Now
                                    };
                                    Logfile.CreateLogFile(Err);
                                }
                            }
                        }
                    }
                }
            }
        }


        public class SalHeadFormulaT
        {
            public int Id { get; set; }
            public GeoStruct GeoStruct { get; set; }
            public PayStruct PayStruct { get; set; }
            public FuncStruct FuncStruct { get; set; }
            public LookupValue FormulaType { get; set; }
            public string Name { get; set; }
        }
        public class EmpSalStructDetailsData
        {
            public double Amount { get; set; }
            public int Id { get; set; }
            public PayScaleAssignment PayScaleAssignment { get; set; }
            public PayScaleAgreement PayScaleAgreement { get; set; }
            public SalaryHead SalaryHead { get; set; }
            public LookupValue SalHeadOperationType { get; set; }
            public SalHeadFormula SalHeadFormula { get; set; }
            public string Name { get; set; }
        }
        public static void EmployeeSalaryStructCreationTest(EmployeePayroll OEmployeePayroll, int OEmployee_Id,
             int OPayScaleAgreement_Id, DateTime mEffectiveDate, int ComPanyPayroll_Id)
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


                var OPayScaleAssignment = db.PayScaleAssignment
                        .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                       .Where(p => p.p.PayScaleAgreement.Id == OPayScaleAgreement_Id && p.p.CompanyPayroll.Id == ComPanyPayroll_Id)
                       .Select(m => new
                       {
                           PayScaleAssignment = m.p, // or m.ppc.pc.ProdId
                           Salheadformula = m.p.SalHeadFormula.Select(t => new SalHeadFormulaT
                           {
                               GeoStruct = t.GeoStruct,
                               PayStruct = t.PayStruct,
                               FuncStruct = t.FuncStruct,
                               FormulaType = t.FormulaType,
                               Name = t.Name,
                               Id = t.Id,
                           }),
                           SalaryHead = m.p.SalaryHead
                           // other assignments
                       }).ToList();


                Employee OEmployee = db.Employee.Where(r => r.Id == OEmployee_Id).SingleOrDefault();
                GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OEmployee.GeoStruct_Id).SingleOrDefault();
                OEmployee.GeoStruct = GeoStruct;
                FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == OEmployee.FuncStruct_Id).SingleOrDefault();
                OEmployee.FuncStruct = FuncStruct;
                PayStruct PayStruct = db.PayStruct.Where(e => e.Id == OEmployee.PayStruct_Id).SingleOrDefault();
                OEmployee.PayStruct = PayStruct;
                EmpOff EmpOffInfo = db.EmpOff.Where(e => e.Id == OEmployee.EmpOffInfo_Id).SingleOrDefault();
                OEmployee.EmpOffInfo = EmpOffInfo;





                //EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == OEmployeePayroll_Id).SingleOrDefault();

                if (OPayScaleAssignment.Count() == 0)
                {
                    return;
                }
                //var OPayScaleAssignment = CompanyPayroll_OPayScaleAssignment.PayScaleAssignment.Where(e => e.PayScaleAgreement != null &&
                //    e.PayScaleAgreement.Id == OPayScaleAgreement_Id).ToList();

                EmpSalStruct OEmpSalStruct = new EmpSalStruct();
                {
                    OEmpSalStruct.EffectiveDate = mEffectiveDate;
                    if (OEmployee.GeoStruct != null)
                    {
                        OEmpSalStruct.GeoStruct = OEmployee.GeoStruct; //db.GeoStruct.Where(e => e.Id == OEmployee.GeoStruct.Id).SingleOrDefault();
                    };
                    if (OEmployee.FuncStruct != null)
                    {
                        OEmpSalStruct.FuncStruct = OEmployee.FuncStruct; // db.FuncStruct.Where(e => e.Id == OEmployee.FuncStruct.Id).SingleOrDefault();
                    };
                    if (OEmployee.PayStruct != null)
                    {
                        OEmpSalStruct.PayStruct = OEmployee.PayStruct; // db.PayStruct.Where(e => e.Id == OEmployee.PayStruct.Id).SingleOrDefault();
                    };
                    OEmpSalStruct.DBTrack = dbt;
                    //db.EmpSalStruct.Add(OEmpSalStruct);
                    //db.SaveChanges();
                    List<EmpSalStructDetails> OEmpSalStructDetails = new List<EmpSalStructDetails>();
                    foreach (var OPayScaleAssignmentData in OPayScaleAssignment)
                    {
                        if (OPayScaleAssignmentData.SalaryHead.Code.ToUpper() == "VPF" && !OEmployee.EmpOffInfo.VPFAppl)
                        {
                            continue;
                        }
                        EmpSalStructDetails OEmpSalStructDetailsObj = new EmpSalStructDetails();
                        {
                            OEmpSalStructDetailsObj.Amount = 0;
                            if (OPayScaleAssignmentData != null)
                            {
                                OEmpSalStructDetailsObj.PayScaleAssignment = OPayScaleAssignmentData.PayScaleAssignment; //db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                            }
                            if (OPayScaleAssignmentData.SalaryHead != null)
                            {
                                OEmpSalStructDetailsObj.SalaryHead = OPayScaleAssignmentData.SalaryHead;  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                            }

                            if (OPayScaleAssignmentData.SalaryHead != null && OPayScaleAssignmentData.Salheadformula != null)//newly added by prashant on 24042017
                            {
                                SalHeadFormulaT Salformula = new SalHeadFormulaT();

                                Salformula = SalFormulaFinderNewTest(OEmpSalStruct, OPayScaleAssignmentData.Salheadformula.ToList(), OPayScaleAssignmentData.SalaryHead.Id);
                                OEmpSalStructDetailsObj.SalHeadFormula = Salformula == null ? null : db.SalHeadFormula.Where(e => e.Id == Salformula.Id).SingleOrDefault();
                            }
                            OEmpSalStructDetailsObj.DBTrack = dbt;
                        };
                        OEmpSalStructDetails.Add(OEmpSalStructDetailsObj);

                    }
                    OEmpSalStruct.EmpSalStructDetails = OEmpSalStructDetails;
                    db.EmpSalStruct.Add(OEmpSalStruct);
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

                    List<EmpSalStruct> OTemp2 = new List<EmpSalStruct>();
                    OTemp2.Add(db.EmpSalStruct.Where(e => e.Id == OEmpSalStruct.Id).SingleOrDefault());
                    if (OEmployeePayroll == null)
                    {
                        EmployeePayroll OTEP = new EmployeePayroll()
                        {
                            Employee = db.Employee.Where(e => e.Id == OEmployee.Id).SingleOrDefault(),
                            EmpSalStruct = OTemp2,
                            DBTrack = dbt
                        };
                        db.EmployeePayroll.Add(OTEP);
                        db.SaveChanges();
                    }
                    else
                    {
                        var aa = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).SingleOrDefault();
                        aa.EmpSalStruct = OTemp2;
                        db.EmployeePayroll.Attach(aa);
                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = "EmployeeSalaryStructCreation - SalaryHeadGenProcess",
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                }
            }
        }
    }
}