
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data;
using Payroll;
using System.Data.Entity.Infrastructure;
using System.Text;
using P2b.Global;
using System.Transactions;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using P2BUltimate.Process;


namespace P2BUltimate.Controllers
{
    public class SalHeadFormulaController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /SalHeadFormula/

        public ActionResult AdvFilter()
        {
            return View("~/Views/Shared/Payroll/_AdvanceFilter.cshtml");
        }

        private MultiSelectList GetWageRanges(List<int> selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<WagesRange> range = new List<WagesRange>();
                range = db.WagesRange.ToList();
                return new MultiSelectList(range, "Id", "RangeFrom", selectedValues);
            }
        }


        private MultiSelectList GetServiceRanges(List<int> selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<ServiceRange> range = new List<ServiceRange>();
                range = db.ServiceRange.ToList();
                return new MultiSelectList(range, "Id", "RangeFrom", selectedValues);
            }
        }

        public ActionResult GetWagesLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.Wages.Include(e => e.RateMaster).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Wages.Include(e => e.RateMaster).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }


                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }

        public ActionResult GetRateMasterLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.RateMaster.ToList().OrderBy(e => e.Id);

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.RateMaster.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList().OrderBy(e => e.Id);
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList().OrderBy(e => e.Id);
                    }
                }


                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.Code }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }

        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/SalHeadFormula/Index.cshtml");
        }

        public ActionResult Slabwise()
        {
            return View("~/Views/Shared/Payroll/_SlabDependRule.cshtml");
        }
        public ActionResult Amtwise()
        {
            return View("~/Views/Shared/Payroll/_AmountDependRule.cshtml");
        }
        public ActionResult Perwise()
        {
            return View("~/Views/Shared/Payroll/_PercentDependRule.cshtml");
        }
        public ActionResult Servicewise()
        {
            return View("~/Views/Shared/Payroll/_ServiceDependRule.cshtml");
        }
        public ActionResult vdarule()
        {
            return View("~/Views/Shared/Payroll/_VDADependRule.cshtml");
        }
        public ActionResult basicrule()
        {
            return View("~/Views/Shared/Payroll/_BASICDependRule.cshtml");
        }


        public ActionResult CreateWageRange_partial()
        {
            return View("~/Views/Shared/Payroll/_WagesRange.cshtml");
        }


        public ActionResult CreateServRange_partial()
        {
            return View("~/Views/Shared/Payroll/_ServiceRange.cshtml");
        }

        public ActionResult CreateWages_partial()
        {
            return View("~/Views/Shared/Payroll/_Wages.cshtml");
        }

        public ActionResult CreateVDARule_partial()
        {
            return View("~/Views/Shared/Payroll/_CPIRuleP.cshtml");
        }

        public ActionResult CreateBASICRule_partial()
        {
            return View("~/Views/Shared/Payroll/_BasicScale.cshtml");
        }



        public class SalHead_WagesRange
        {
            public Array WagesRange_id { get; set; }
            public Array WagesRange_val { get; set; }
            public string Wages_id { get; set; }
            public string Wages_val { get; set; }
            public Array ServRange_id { get; set; }
            public Array ServRange_val { get; set; }
            public string CPIRule_id { get; set; }
            public string CPIRule_val { get; set; }
            public string BASICRule_id { get; set; }
            public string BASICRule_val { get; set; }
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(SalHeadFormula SalHead, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string list_Id = form["list_Id"] == "0" ? "" : form["list_Id"];

                    string wages = form["WagesList"] == "0" ? "" : form["WagesList"];
                    string selectedValue = form["radio"];
                    string geo_id = form["geo_id"] == "0" ? "" : form["geo_id"];
                    string fun_id = form["fun_id"] == "0" ? "" : form["fun_id"];
                    string pay_id = form["pay_id"] == "0" ? "" : form["pay_id"];

                    if (geo_id == "" && fun_id == "" && pay_id == "")
                    {
                        Msg.Add("  Kindly Apply Advance Filter..!!! ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    string formulatype = form["Formulatype_dropview"] == "0" ? "" : form["Formulatype_dropview"];
                    if (list_Id != null)
                    {

                    }
                    if (formulatype == null || formulatype == "")
                    {
                        Msg.Add("  Kindly select formula type ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (selectedValue == null)
                    {
                        Msg.Add("  Kindly select depend rule  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    int CompId = 0;
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = int.Parse(Session["CompId"].ToString());
                    }


                    CompanyPayroll OCompanyPayroll = null;
                    List<SalHeadFormula> OFAT = new List<SalHeadFormula>();
                    OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == CompId).SingleOrDefault();
                    Wages val = null;
                    if (wages != null && wages != "")
                    {
                        val = db.Wages.Find(int.Parse(wages));
                    }
                    SalHead.SalWages = val;
                    if (formulatype != null && formulatype != "")
                    {
                        var val1 = db.LookupValue.Find(int.Parse(formulatype));
                        SalHead.FormulaType = val1;
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                          new System.TimeSpan(0, 30, 0)))
                        {
                            if (db.SalHeadFormula.Any(o => o.Name == SalHead.Name))
                            {
                                //return this.Json(new { msg = "Code already exists." });
                                //  return this.Json(new Object[] { null, null, "FormulaName already exists.", JsonRequestBehavior.AllowGet });
                                Msg.Add("FormulaName already exists.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            try
                            {
                                if (selectedValue == "1")
                                {
                                    string wageRange = form["WageRangeList"];
                                    List<WagesRange> WagesRangeval = new List<WagesRange>();

                                    SlabDependRule SlabDependRule = new SlabDependRule();

                                    SlabDependRule.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    if (wageRange != null)
                                    {
                                        var ids = Utility.StringIdsToListIds(wageRange);
                                        foreach (var ca in ids)
                                        {
                                            var Lookup_val = db.WagesRange.Find(ca);
                                            WagesRangeval.Add(Lookup_val);
                                            SlabDependRule.WageRange = WagesRangeval;
                                        }
                                    }
                                    else
                                    {
                                        SlabDependRule.WageRange = null;
                                    }
                                    SalHead.SlabDependRule = SlabDependRule;

                                    db.SlabDependRule.Add(SlabDependRule);

                                }
                                if (selectedValue == "2")
                                {

                                    AmountDependRule AmountDependRule = new AmountDependRule();
                                    AmountDependRule.SalAmount = Convert.ToInt32(form["SalAmount"]);
                                    SalHead.AmountDependRule = AmountDependRule;
                                    AmountDependRule.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    db.AmountDependRule.Add(AmountDependRule);
                                }
                                if (selectedValue == "3")
                                {
                                    PercentDependRule PercentDependRule = new PercentDependRule();
                                    PercentDependRule.SalPercent = Convert.ToDouble(form["SalPercent"]);

                                    SalHead.PercentDependRule = PercentDependRule;
                                    PercentDependRule.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    db.PercentDependRule.Add(PercentDependRule);
                                }
                                if (selectedValue == "4")
                                {
                                    string servRange = form["ServRangeList"];
                                    List<ServiceRange> ServiceRangeval = new List<ServiceRange>();

                                    ServiceDependRule ServiceDependRule = new ServiceDependRule();


                                    if (servRange != null)
                                    {
                                        var ids = Utility.StringIdsToListIds(servRange);
                                        foreach (var ca in ids)
                                        {
                                            var Lookup_val = db.ServiceRange.Find(ca);
                                            ServiceRangeval.Add(Lookup_val);
                                            ServiceDependRule.ServiceRange = ServiceRangeval;
                                        }
                                    }
                                    else
                                    {
                                        ServiceDependRule.ServiceRange = null;
                                    }
                                    SalHead.ServiceDependRule = ServiceDependRule;
                                    ServiceDependRule.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    db.ServiceDependRule.Add(ServiceDependRule);
                                }
                                if (selectedValue == "5")
                                {
                                    string CPIRule = form["CPIRuleList"];

                                    VDADependRule VDADependRule = new VDADependRule();

                                    if (CPIRule != null)
                                    {
                                        var CPIRuleval = db.CPIRule.Find(int.Parse(CPIRule));
                                        VDADependRule.CPIRule = CPIRuleval;
                                    }
                                    else
                                    {
                                        VDADependRule.CPIRule = null;
                                    }
                                    SalHead.VDADependRule = VDADependRule;
                                    VDADependRule.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    db.VDADependRule.Add(VDADependRule);
                                }
                                if (selectedValue == "6")
                                {
                                    string BASICScale = form["BASICRuleList"];

                                    BASICDependRule BASICDependRule = new BASICDependRule();

                                    if (BASICScale != null)
                                    {
                                        var BasicScaleval = db.BasicScale.Find(int.Parse(BASICScale));
                                        BASICDependRule.BasicScale = BasicScaleval;
                                    }
                                    else
                                    {
                                        BASICDependRule.BasicScale = null;
                                    }
                                    SalHead.BASICDependRule = BASICDependRule;
                                    BASICDependRule.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    db.BASICDependRule.Add(BASICDependRule);
                                }

                                SalHead.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                if ((geo_id != "") && (pay_id != "") && (fun_id != ""))
                                {
                                    var ids = Utility.StringIdsToListIds(geo_id);
                                    var ids1 = Utility.StringIdsToListIds(pay_id);
                                    var ids2 = Utility.StringIdsToListIds(fun_id);
                                    foreach (var G in ids)
                                    {
                                        foreach (var P in ids1)
                                        {
                                            foreach (var F in ids2)
                                            {
                                                GeoStruct geo = db.GeoStruct.Find(G);
                                                SalHead.GeoStruct = geo;

                                                PayStruct pay = db.PayStruct.Find(P);
                                                SalHead.PayStruct = pay;

                                                FuncStruct fun = db.FuncStruct.Find(F);
                                                SalHead.FuncStruct = fun;

                                                SalHeadFormula SalHeadForGeo = new SalHeadFormula()
                                                {
                                                    AmountDependRule = SalHead.AmountDependRule,
                                                    BASICDependRule = SalHead.BASICDependRule,
                                                    CeilingMax = SalHead.CeilingMax,
                                                    CeilingMin = SalHead.CeilingMin,
                                                    DBTrack = SalHead.DBTrack,
                                                    FuncStruct = SalHead.FuncStruct,
                                                    GeoStruct = SalHead.GeoStruct,
                                                    Name = SalHead.Name,
                                                    PayStruct = SalHead.PayStruct,
                                                    PercentDependRule = SalHead.PercentDependRule,
                                                    SalWages = SalHead.SalWages,
                                                    ServiceDependRule = SalHead.ServiceDependRule,
                                                    SlabDependRule = SalHead.SlabDependRule,
                                                    VDADependRule = SalHead.VDADependRule
                                                };

                                                db.SalHeadFormula.Add(SalHeadForGeo);
                                                db.SaveChanges();

                                                OFAT.Add(db.SalHeadFormula.Find(SalHeadForGeo.Id));
                                            }

                                        }

                                    }

                                }

                                if ((geo_id != "") && (pay_id != "") && (fun_id == ""))
                                {
                                    var ids = Utility.StringIdsToListIds(geo_id);
                                    var ids1 = Utility.StringIdsToListIds(pay_id);
                                    foreach (var G in ids)
                                    {
                                        foreach (var P in ids1)
                                        {
                                            GeoStruct geo = db.GeoStruct.Find(G);
                                            SalHead.GeoStruct = geo;

                                            PayStruct pay = db.PayStruct.Find(P);
                                            SalHead.PayStruct = pay;

                                            SalHead.FuncStruct = null;

                                            SalHeadFormula SalHeadForGeo = new SalHeadFormula()
                                            {
                                                AmountDependRule = SalHead.AmountDependRule,
                                                BASICDependRule = SalHead.BASICDependRule,
                                                CeilingMax = SalHead.CeilingMax,
                                                CeilingMin = SalHead.CeilingMin,
                                                DBTrack = SalHead.DBTrack,
                                                FuncStruct = SalHead.FuncStruct,
                                                GeoStruct = SalHead.GeoStruct,
                                                Name = SalHead.Name,
                                                FormulaType = SalHead.FormulaType,
                                                PayStruct = SalHead.PayStruct,
                                                PercentDependRule = SalHead.PercentDependRule,
                                                SalWages = SalHead.SalWages,
                                                ServiceDependRule = SalHead.ServiceDependRule,
                                                SlabDependRule = SalHead.SlabDependRule,
                                                VDADependRule = SalHead.VDADependRule
                                            };

                                            db.SalHeadFormula.Add(SalHeadForGeo);
                                            db.SaveChanges();

                                            OFAT.Add(db.SalHeadFormula.Find(SalHeadForGeo.Id));
                                        }

                                    }

                                }

                                if ((geo_id != "") && (fun_id != "") && (pay_id == ""))
                                {
                                    var ids = Utility.StringIdsToListIds(geo_id);
                                    var ids1 = Utility.StringIdsToListIds(fun_id);
                                    foreach (var G in ids)
                                    {
                                        foreach (var P in ids1)
                                        {
                                            GeoStruct geo = db.GeoStruct.Find(G);
                                            SalHead.GeoStruct = geo;

                                            FuncStruct func = db.FuncStruct.Find(P);
                                            SalHead.FuncStruct = func;

                                            SalHead.PayStruct = null;

                                            SalHeadFormula SalHeadForGeo = new SalHeadFormula()
                                            {
                                                AmountDependRule = SalHead.AmountDependRule,
                                                BASICDependRule = SalHead.BASICDependRule,
                                                CeilingMax = SalHead.CeilingMax,
                                                CeilingMin = SalHead.CeilingMin,
                                                DBTrack = SalHead.DBTrack,
                                                FuncStruct = SalHead.FuncStruct,
                                                GeoStruct = SalHead.GeoStruct,
                                                Name = SalHead.Name,
                                                FormulaType = SalHead.FormulaType,
                                                PayStruct = SalHead.PayStruct,
                                                PercentDependRule = SalHead.PercentDependRule,
                                                SalWages = SalHead.SalWages,
                                                ServiceDependRule = SalHead.ServiceDependRule,
                                                SlabDependRule = SalHead.SlabDependRule,
                                                VDADependRule = SalHead.VDADependRule
                                            };

                                            db.SalHeadFormula.Add(SalHeadForGeo);
                                            db.SaveChanges();

                                            OFAT.Add(db.SalHeadFormula.Find(SalHeadForGeo.Id));
                                        }

                                    }

                                }

                                if ((geo_id != "") && (fun_id == "") && (pay_id == ""))
                                {
                                    var ids = Utility.StringIdsToListIds(geo_id);

                                    foreach (var G in ids)
                                    {
                                        var geo = db.GeoStruct.Find(G);
                                        SalHead.GeoStruct = geo;

                                        SalHead.FuncStruct = null;
                                        SalHead.PayStruct = null;

                                        SalHeadFormula SalHeadForGeo = new SalHeadFormula()
                                        {
                                            AmountDependRule = SalHead.AmountDependRule,
                                            BASICDependRule = SalHead.BASICDependRule,
                                            CeilingMax = SalHead.CeilingMax,
                                            CeilingMin = SalHead.CeilingMin,
                                            DBTrack = SalHead.DBTrack,
                                            FuncStruct = SalHead.FuncStruct,
                                            GeoStruct = SalHead.GeoStruct,
                                            Name = SalHead.Name,
                                            FormulaType = SalHead.FormulaType,
                                            PayStruct = SalHead.PayStruct,
                                            PercentDependRule = SalHead.PercentDependRule,
                                            SalWages = SalHead.SalWages,
                                            ServiceDependRule = SalHead.ServiceDependRule,
                                            SlabDependRule = SalHead.SlabDependRule,
                                            VDADependRule = SalHead.VDADependRule
                                        };

                                        db.SalHeadFormula.Add(SalHeadForGeo);
                                        db.SaveChanges();

                                        OFAT.Add(db.SalHeadFormula.Find(SalHeadForGeo.Id));
                                    }

                                }

                                if ((fun_id != "") && (pay_id == "") && (geo_id == ""))
                                {
                                    //var geo = db.GeoStruct.Where(e => e.Company != null && e.Company.Id == Comp_Id).Distinct().FirstOrDefault();
                                    //SalHead.GeoStruct = geo;

                                    //var pay = db.PayStruct.Where(e => e.Company != null && e.Company.Id == Comp_Id).Distinct().FirstOrDefault();
                                    //SalHead.PayStruct = pay;

                                    var ids = Utility.StringIdsToListIds(fun_id);
                                    foreach (var F in ids)
                                    {
                                        var fun = db.FuncStruct.Find(F);
                                        SalHead.FuncStruct = fun;

                                        SalHead.GeoStruct = null;
                                        SalHead.PayStruct = null;

                                        SalHeadFormula SalHeadForFunct = new SalHeadFormula()
                                        {
                                            AmountDependRule = SalHead.AmountDependRule,
                                            BASICDependRule = SalHead.BASICDependRule,
                                            CeilingMax = SalHead.CeilingMax,
                                            CeilingMin = SalHead.CeilingMin,
                                            DBTrack = SalHead.DBTrack,
                                            FuncStruct = SalHead.FuncStruct,
                                            GeoStruct = SalHead.GeoStruct,
                                            Name = SalHead.Name,
                                            FormulaType = SalHead.FormulaType,
                                            PayStruct = SalHead.PayStruct,
                                            PercentDependRule = SalHead.PercentDependRule,
                                            SalWages = SalHead.SalWages,
                                            ServiceDependRule = SalHead.ServiceDependRule,
                                            SlabDependRule = SalHead.SlabDependRule,
                                            VDADependRule = SalHead.VDADependRule
                                        };

                                        db.SalHeadFormula.Add(SalHeadForFunct);
                                        db.SaveChanges();

                                        OFAT.Add(db.SalHeadFormula.Find(SalHeadForFunct.Id));
                                    }
                                }

                                if ((pay_id != "") && (geo_id == "") && (fun_id == ""))
                                {
                                    var ids = Utility.StringIdsToListIds(pay_id);

                                    //var geo = db.GeoStruct.Where(e => e.Company != null && e.Company.Id == Comp_Id).Distinct().FirstOrDefault();
                                    //SalHead.GeoStruct = geo;

                                    //var fun = db.FuncStruct.Where(e => e.Company != null && e.Company.Id == Comp_Id).Distinct().FirstOrDefault();
                                    //SalHead.FuncStruct = fun;

                                    foreach (var P in ids)
                                    {
                                        var pay = db.PayStruct.Find(P);
                                        SalHead.PayStruct = pay;

                                        SalHead.GeoStruct = null;
                                        SalHead.FuncStruct = null;

                                        SalHeadFormula SalHeadForPayt = new SalHeadFormula()
                                        {
                                            AmountDependRule = SalHead.AmountDependRule,
                                            BASICDependRule = SalHead.BASICDependRule,
                                            CeilingMax = SalHead.CeilingMax,
                                            CeilingMin = SalHead.CeilingMin,
                                            DBTrack = SalHead.DBTrack,
                                            FuncStruct = SalHead.FuncStruct,
                                            GeoStruct = SalHead.GeoStruct,
                                            Name = SalHead.Name,
                                            FormulaType = SalHead.FormulaType,
                                            PayStruct = SalHead.PayStruct,
                                            PercentDependRule = SalHead.PercentDependRule,
                                            SalWages = SalHead.SalWages,
                                            ServiceDependRule = SalHead.ServiceDependRule,
                                            SlabDependRule = SalHead.SlabDependRule,
                                            VDADependRule = SalHead.VDADependRule
                                        };

                                        db.SalHeadFormula.Add(SalHeadForPayt);
                                        db.SaveChanges();

                                        OFAT.Add(db.SalHeadFormula.Find(SalHeadForPayt.Id));
                                    }
                                }

if ((geo_id == "") && (fun_id != "") && (pay_id != ""))
                                {
                                    var ids = Utility.StringIdsToListIds(pay_id);
                                    var ids1 = Utility.StringIdsToListIds(fun_id);
                                    foreach (var P in ids)
                                    {
                                        foreach (var F in ids1)
                                        {
                                            PayStruct pay = db.PayStruct.Find(P);
                                            SalHead.PayStruct = pay;

                                            FuncStruct func = db.FuncStruct.Find(F);
                                            SalHead.FuncStruct = func;

                                            SalHead.GeoStruct = null;

                                            SalHeadFormula SalHeadForGeo = new SalHeadFormula()
                                            {
                                                AmountDependRule = SalHead.AmountDependRule,
                                                BASICDependRule = SalHead.BASICDependRule,
                                                CeilingMax = SalHead.CeilingMax,
                                                CeilingMin = SalHead.CeilingMin,
                                                DBTrack = SalHead.DBTrack,
                                                FuncStruct = SalHead.FuncStruct,
                                                GeoStruct = SalHead.GeoStruct,
                                                Name = SalHead.Name,
                                                FormulaType = SalHead.FormulaType,
                                                PayStruct = SalHead.PayStruct,
                                                PercentDependRule = SalHead.PercentDependRule,
                                                SalWages = SalHead.SalWages,
                                                ServiceDependRule = SalHead.ServiceDependRule,
                                                SlabDependRule = SalHead.SlabDependRule,
                                                VDADependRule = SalHead.VDADependRule
                                            };

                                            db.SalHeadFormula.Add(SalHeadForGeo);
                                            db.SaveChanges();

                                            OFAT.Add(db.SalHeadFormula.Find(SalHeadForGeo.Id));
                                        }

                                    }

                                }

                                if (geo_id == "" && pay_id == "" && fun_id == "")
                                {
                                    var geoList = db.GeoStruct.Where(e => e.Company != null && e.Company.Id == CompId).ToList();

                                    foreach (var geo in geoList)
                                    {
                                        SalHead.GeoStruct = geo;

                                        SalHead.PayStruct = null;
                                        SalHead.FuncStruct = null;

                                        SalHeadFormula SalHeadForFunct = new SalHeadFormula()
                                        {
                                            AmountDependRule = SalHead.AmountDependRule,
                                            BASICDependRule = SalHead.BASICDependRule,
                                            CeilingMax = SalHead.CeilingMax,
                                            CeilingMin = SalHead.CeilingMin,
                                            DBTrack = SalHead.DBTrack,
                                            FuncStruct = SalHead.FuncStruct,
                                            GeoStruct = SalHead.GeoStruct,
                                            Name = SalHead.Name,
                                            FormulaType = SalHead.FormulaType,
                                            PayStruct = SalHead.PayStruct,
                                            PercentDependRule = SalHead.PercentDependRule,
                                            SalWages = SalHead.SalWages,
                                            ServiceDependRule = SalHead.ServiceDependRule,
                                            SlabDependRule = SalHead.SlabDependRule,
                                            VDADependRule = SalHead.VDADependRule
                                        };

                                        db.SalHeadFormula.Add(SalHeadForFunct);
                                        db.SaveChanges();

                                        OFAT.Add(db.SalHeadFormula.Find(SalHeadForFunct.Id));
                                    }
                                }
                                //SalHeadFormula SalHeadFor = new SalHeadFormula()
                                //{
                                //    AmountDependRule = SalHead.AmountDependRule,
                                //    BASICDependRule = SalHead.BASICDependRule,
                                //    CeilingMax = SalHead.CeilingMax,
                                //    CeilingMin = SalHead.CeilingMin,
                                //    DBTrack = SalHead.DBTrack,
                                //    FuncStruct = SalHead.FuncStruct,
                                //    GeoStruct = SalHead.GeoStruct,
                                //    Name = SalHead.Name,
                                //    PayStruct = SalHead.PayStruct,
                                //    PercentDependRule = SalHead.PercentDependRule,
                                //    SalWages = SalHead.SalWages,
                                //    ServiceDependRule = SalHead.ServiceDependRule,
                                //    SlabDependRule = SalHead.SlabDependRule,
                                //    VDADependRule = SalHead.VDADependRule
                                //};


                                //db.SalHeadFormula.Add(SalHeadFor);
                                //db.SaveChanges();
                                if (OCompanyPayroll == null)
                                {
                                    CompanyPayroll OTEP = new CompanyPayroll()
                                    {
                                        Company = db.Company.Find(SessionManager.CompanyId),
                                        SalHeadFormula = OFAT,
                                        DBTrack = SalHead.DBTrack

                                    };


                                    db.CompanyPayroll.Add(OTEP);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    var aa = db.CompanyPayroll.Find(OCompanyPayroll.Id);
                                    //aa.SalAttendance = null;
                                    if (aa.SalHeadFormula != null)
                                        OFAT.AddRange(aa.SalHeadFormula);
                                    aa.SalHeadFormula = OFAT;
                                    //OEmployeePayroll.DBTrack = dbt;
                                    db.CompanyPayroll.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                }

                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DataException /* dex */)
                            {
                                //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                                //ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                                //return View(level);
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        //return this.Json(new { msg = errorMsg });
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
                //  return View();
            }
        }

        //select filter value pending//
        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int selected = 0;
                List<SalHead_WagesRange> return_data = new List<SalHead_WagesRange>();

                var Q = db.SalHeadFormula
                   .Where(e => e.Id == data).Select
                   (e => new
                   {
                       CeilingMax = e.CeilingMax,
                       CeilingMin = e.CeilingMin,
                       Name = e.Name,
                       FormulaType_Id = e.FormulaType.Id == null ? 0 : e.FormulaType.Id,
                       Action = e.DBTrack.Action,
                       Wages_id = e.SalWages.Id.ToString(),
                       Wages_val = e.SalWages.FullDetails
                   }).ToList();


                var SlabHeadForm = db.SalHeadFormula.Where(e => e.Id == data).Select(e => e.SlabDependRule).Include(e => e.WageRange).ToList();

                if (SlabHeadForm.Count > 0)
                {
                    if (SlabHeadForm[0] != null)
                    {
                        foreach (var ca in SlabHeadForm)
                        {
                            return_data.Add(
                        new SalHead_WagesRange
                        {
                            WagesRange_id = ca.WageRange.Select(e => e.Id.ToString()).ToArray(),
                            WagesRange_val = ca.WageRange.Select(e => e.FullDetails).ToArray()
                        });
                        }

                        selected = 1;

                        var SalHead = db.SalHeadFormula.Find(data);
                        TempData["RowVersion"] = SalHead.RowVersion;
                        var Auth = SalHead.DBTrack.IsModified;

                        return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);
                    }
                }


                var AmtDependRule = db.SalHeadFormula.Include(e => e.AmountDependRule).Where(e => e.Id == data).SingleOrDefault();

                if (AmtDependRule.AmountDependRule != null)
                {
                    var Amount = AmtDependRule.AmountDependRule.SalAmount;
                    selected = 2;
                    var SalHead = db.SalHeadFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;
                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth, Amount }, JsonRequestBehavior.AllowGet);
                }

                var PercentDependRule = db.SalHeadFormula.Include(e => e.PercentDependRule).Where(e => e.Id == data).SingleOrDefault();

                if (PercentDependRule.PercentDependRule != null)
                {
                    var Percent = PercentDependRule.PercentDependRule.SalPercent;
                    selected = 3;
                    var SalHead = db.SalHeadFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;
                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth, Percent }, JsonRequestBehavior.AllowGet);
                }



                var ServHeadForm = db.SalHeadFormula.Where(e => e.Id == data).Select(e => e.ServiceDependRule).Include(e => e.ServiceRange).ToList();


                if (ServHeadForm.Count > 0)
                {
                    if (ServHeadForm[0] != null)
                    {
                        foreach (var ca in ServHeadForm)
                        {
                            return_data.Add(
                        new SalHead_WagesRange
                        {
                            ServRange_id = ca.ServiceRange.Select(e => e.Id.ToString()).ToArray(),
                            ServRange_val = ca.ServiceRange.Select(e => e.FullDetails).ToArray()
                        });
                        }

                        selected = 4;

                        var SalHead = db.SalHeadFormula.Find(data);
                        TempData["RowVersion"] = SalHead.RowVersion;
                        var Auth = SalHead.DBTrack.IsModified;

                        return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);
                    }


                }



                var VDADependRule = db.SalHeadFormula.Where(e => e.Id == data).Select(e => e.VDADependRule).Include(e => e.CPIRule).ToList();
                if (VDADependRule.Count > 0)
                {
                    if (VDADependRule[0] != null)
                    {
                        foreach (var ca in VDADependRule)
                        {
                            return_data.Add(
                            new SalHead_WagesRange
                            {
                                CPIRule_id = ca.CPIRule.Id.ToString(),
                                CPIRule_val = ca.CPIRule.Name
                            });
                        }


                        selected = 5;

                        var SalHead = db.SalHeadFormula.Find(data);
                        TempData["RowVersion"] = SalHead.RowVersion;
                        var Auth = SalHead.DBTrack.IsModified;

                        return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);
                    }
                }


                var BASICDependRule = db.SalHeadFormula.Where(e => e.Id == data).Select(e => e.BASICDependRule).Include(e => e.BasicScale).ToList();
                if (BASICDependRule.Count > 0)
                {
                    if (BASICDependRule[0] != null)
                    {
                        foreach (var ca in BASICDependRule)
                        {
                            return_data.Add(
                            new SalHead_WagesRange
                            {
                                BASICRule_id = ca.BasicScale.Id.ToString(),
                                BASICRule_val = ca.BasicScale.ScaleName
                            });
                        }


                        selected = 6;

                        var SalHead = db.SalHeadFormula.Find(data);
                        TempData["RowVersion"] = SalHead.RowVersion;
                        var Auth = SalHead.DBTrack.IsModified;

                        return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);
                    }
                }
                return View();
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult EditSave(SalHeadFormula SalHead, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string WagesList = form["WagesList"] == "0" ? "" : form["WagesList"];
                    string selectedValue = form["radio"];


                    //Wages wages = null;
                    if (WagesList != null && WagesList != "")
                    {
                        SalHead.SalWages = db.Wages.Find(int.Parse(WagesList));
                    }



                    if (ModelState.IsValid)
                    {
                        //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 60, 0)))
                        //// using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                        //                  new System.TimeSpan(0, 120, 0)))
                        // {
                        SalHeadFormula blog = null; // to retrieve old data
                        using (var context = new DataBaseContext())
                        {
                            blog = context.SalHeadFormula.Where(e => e.Id == data).SingleOrDefault();
                        }

                        SalHead.DBTrack = new DBTrack
                        {
                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                            Action = "M",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now
                        };
                        try
                        {
                            if (selectedValue == "1")
                            {
                               var wageRange = form["WageRangeList"];

                                if (wageRange != null)
                                {
                                   
                                   
                                    //int wageRangeId = int.Parse(wageRange);
                                    var ids = Utility.StringIdsToListIds(wageRange);
                                    List<WagesRange> WagesRangeVal = new List<WagesRange>();
                                    SlabDependRule SlabDependRuleNew = db.SalHeadFormula.Include(e => e.SlabDependRule)
                                           .Where(e => e.Id == data).Select(e => e.SlabDependRule).FirstOrDefault();
                                   

                                    if (SlabDependRuleNew == null)
                                    {
                                        SlabDependRule SlabDependRule = new SlabDependRule();

                                        //var WageRangeval = db.WagesRange.Find(wageRangeId);
                                        var WageRangeval = db.WagesRange.Find(ids);
                                        WagesRangeVal.Add(WageRangeval);
                                        SlabDependRule.WageRange = WagesRangeVal;

                                        SalHead.SlabDependRule = SlabDependRule;
                                        SlabDependRule.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        db.SlabDependRule.Add(SlabDependRule);
                                    }
                                    else
                                    {
                                        SlabDependRule typedetails = db.SlabDependRule.Include(e => e.WageRange).Where(e => e.Id == SlabDependRuleNew.Id).FirstOrDefault();
                                        typedetails.DBTrack = new DBTrack
                                        {
                                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };

                                        if (wageRange != null && wageRange != "")
                                        {
                                            
                                            foreach (var ca in ids)
                                            {
                                                var WageRange_val = db.WagesRange.Find(ca);
                                                WagesRangeVal.Add(WageRange_val);
                                                typedetails.WageRange = WagesRangeVal;
                                            }
                                        }
                                        else
                                        {
                                            typedetails.WageRange = null;
                                        }


                                        db.SlabDependRule.Attach(typedetails);
                                        db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        SalHead.SlabDependRule = SlabDependRuleNew; 
                                    }

                                    //VDADependRule.CPIRule = CPIRuleval;

                                }
                                else
                                {
                                    SalHead.SlabDependRule = null;
                                }
                                //List<WagesRange> WagesRangeval = new List<WagesRange>();
                                //int SlabDependRule = db.SalHeadFormula.Include(e => e.SlabDependRule)
                                //            .Where(e => e.Id == data).Select(e => e.SlabDependRule).SingleOrDefault().Id;


                                //SlabDependRule typedetails = db.SlabDependRule.Include(e => e.WageRange).Where(e => e.Id == SlabDependRule).SingleOrDefault();
                                //typedetails.DBTrack = new DBTrack
                                //{
                                //    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                //    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                //    Action = "M",
                                //    ModifiedBy = SessionManager.UserName,
                                //    ModifiedOn = DateTime.Now
                                //};

                                //if (wageRange != null && wageRange != "")
                                //{
                                //    var ids = Utility.StringIdsToListIds(wageRange);
                                //    foreach (var ca in ids)
                                //    {
                                //        var PayScaleval_val = db.WagesRange.Find(ca);
                                //        WagesRangeval.Add(PayScaleval_val);
                                //        typedetails.WageRange = WagesRangeval;
                                //    }
                                //}
                                //else
                                //{
                                //    typedetails.WageRange = null;
                                //}


                                //db.SlabDependRule.Attach(typedetails);
                                //db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();
                            }
                            if (selectedValue == "2")
                            {
                                AmountDependRule AmtDependRule = db.SalHeadFormula.Include(e => e.AmountDependRule)
                                            .Where(e => e.Id == data).Select(e => e.AmountDependRule).SingleOrDefault();
                                AmtDependRule.SalAmount = Convert.ToInt32(form["SalAmount"]);
                                db.AmountDependRule.Attach(AmtDependRule);
                                db.Entry(AmtDependRule).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                            }
                            if (selectedValue == "3")
                            {
                                PercentDependRule PercDependRule = db.SalHeadFormula.Include(e => e.PercentDependRule)
                                            .Where(e => e.Id == data).Select(e => e.PercentDependRule).SingleOrDefault();
                                PercDependRule.SalPercent = Convert.ToDouble(form["SalPercent"]);
                                db.PercentDependRule.Attach(PercDependRule);
                                db.Entry(PercDependRule).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                            }
                            if (selectedValue == "4")
                            {
                                string servRange = form["ServRangeList"];

                                //ServiceDependRule ServiceDependRule = db.SalHeadFormula.Include(e => e.ServiceDependRule)
                                //           .Where(e => e.Id == data).Select(e => e.ServiceDependRule).SingleOrDefault();



                                if (servRange != null)
                                {
                                    //int servRangeId = int.Parse(servRange);
                                    var ids = Utility.StringIdsToListIds(servRange);
                                    ServiceDependRule ServiceDependRuleNew = db.SalHeadFormula.Include(e => e.ServiceDependRule)
                                           .Where(e => e.Id == data).Select(e => e.ServiceDependRule).FirstOrDefault();
                                    List<ServiceRange> ServiceRangeval = new List<ServiceRange>();

                                    if (ServiceDependRuleNew == null)
                                    {
                                        ServiceDependRule ServiceDependRule = new ServiceDependRule();

                                        //var ServRangeval = db.ServiceRange.Find(servRangeId);
                                        var ServRangeval = db.ServiceRange.Find(ids);
                                        ServiceRangeval.Add(ServRangeval);
                                        ServiceDependRule.ServiceRange = ServiceRangeval;

                                        SalHead.ServiceDependRule = ServiceDependRule;
                                        ServiceDependRule.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        db.ServiceDependRule.Add(ServiceDependRule);
                                    }
                                    else
                                    {
                                        //VDADependRule.CPIRule = CPIRuleval;
                                         //ServiceDependRule ServiceDependRule = db.SalHeadFormula.Include(e => e.ServiceDependRule)
                                         //          .Where(e => e.Id == data).Select(e => e.ServiceDependRule).SingleOrDefault();

                                         ServiceDependRule ServiceDependRule = db.ServiceDependRule.Include(e => e.ServiceRange).Where(e => e.Id == ServiceDependRuleNew.Id).FirstOrDefault();
                                         ServiceDependRule.DBTrack = new DBTrack
                                         {
                                             CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                             CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                             Action = "M",
                                             ModifiedBy = SessionManager.UserName,
                                             ModifiedOn = DateTime.Now
                                         };

                                        if (servRange != null)
                                        {
                                            
                                            foreach (var ca in ids)
                                            {
                                                var ServeRange_val = db.ServiceRange.Find(ca);
                                                ServiceRangeval.Add(ServeRange_val);
                                                ServiceDependRule.ServiceRange = ServiceRangeval;
                                            }
                                        }
                                        else
                                        {
                                            ServiceDependRule.ServiceRange = null;
                                        }
                                        db.ServiceDependRule.Attach(ServiceDependRule);
                                        db.Entry(ServiceDependRule).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        SalHead.ServiceDependRule = ServiceDependRuleNew; 
                                       
                                    }

                                }
                                else
                                {
                                    SalHead.ServiceDependRule = null;
                                }

                                //List<ServiceRange> ServiceRangeval = new List<ServiceRange>();

                                //ServiceDependRule ServiceDependRule = db.SalHeadFormula.Include(e => e.ServiceDependRule)
                                //           .Where(e => e.Id == data).Select(e => e.ServiceDependRule).SingleOrDefault();


                                //if (servRange != null)
                                //{
                                //    var ids = Utility.StringIdsToListIds(servRange);
                                //    foreach (var ca in ids)
                                //    {
                                //        var Lookup_val = db.ServiceRange.Find(ca);
                                //        ServiceRangeval.Add(Lookup_val);
                                //        ServiceDependRule.ServiceRange = ServiceRangeval;
                                //    }
                                //}
                                //else
                                //{
                                //    ServiceDependRule.ServiceRange = null;
                                //}
                                //db.ServiceDependRule.Attach(ServiceDependRule);
                                //db.Entry(ServiceDependRule).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();
                            }
                            if (selectedValue == "5")
                            {
                                string CPIRule = form["CPIRuleList"];
                                VDADependRule VDADependRule = db.SalHeadFormula.Include(e => e.VDADependRule)
                                           .Where(e => e.Id == data).Select(e => e.VDADependRule).SingleOrDefault();

                               

                                if (CPIRule != null)
                                {
                                    int CPIRuleId = int.Parse(CPIRule);
                                    VDADependRule VDADependRuleNew = db.VDADependRule.Include(e => e.CPIRule)
                                         .Where(e => e.CPIRule.Id == CPIRuleId).FirstOrDefault();
                                    //var CPIRuleval = db.CPIRule.Include(e => e.CPIRuleDetails).Where(e => e.Id == CPIRuleId).SingleOrDefault();

                                    if (VDADependRuleNew == null)
                                    {
                                        VDADependRule VDADependRule1 = new VDADependRule();

                                        var CPIRuleval = db.CPIRule.Find(CPIRuleId);
                                        VDADependRule1.CPIRule = CPIRuleval;

                                        SalHead.VDADependRule = VDADependRule1;
                                        VDADependRule1.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        db.VDADependRule.Add(VDADependRule1);
                                    }
                                    else
                                    { SalHead.VDADependRule = VDADependRuleNew; }

                                    //VDADependRule.CPIRule = CPIRuleval;
                                   
                                }
                                else
                                {
                                    SalHead.VDADependRule = null;
                                }
                                //db.SalHeadFormula.Attach(VDADependRule);
                                //db.Entry(VDADependRule).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();
                            }
                            if (selectedValue == "6")
                            {
                                string BASICScale = form["BASICRuleList"];

                                BASICDependRule BASICDependRule = db.SalHeadFormula.Include(e => e.BASICDependRule)
                                .Where(e => e.Id == data).Select(e => e.BASICDependRule).SingleOrDefault();



                                if (BASICScale != null)
                                {
                                    int BASICScaleId = int.Parse(BASICScale);
                                    BASICDependRule BASICDependRuleNew = db.BASICDependRule.Include(e => e.BasicScale)
                                         .Where(e => e.BasicScale.Id == BASICScaleId).FirstOrDefault();
                                    //var CPIRuleval = db.CPIRule.Include(e => e.CPIRuleDetails).Where(e => e.Id == CPIRuleId).SingleOrDefault();

                                    if (BASICDependRuleNew == null)
                                    {
                                        BASICDependRule BASICDependRule1 = new BASICDependRule();

                                        var CPIRuleval = db.BasicScale.Find(BASICScaleId);
                                        BASICDependRule1.BasicScale = CPIRuleval;

                                        SalHead.BASICDependRule = BASICDependRule1;
                                        BASICDependRule1.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        db.BASICDependRule.Add(BASICDependRule1);
                                    }
                                    else
                                    {  //VDADependRule.CPIRule = CPIRuleval;
                                        db.BASICDependRule.Attach(BASICDependRule);
                                        db.Entry(BASICDependRule).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }

                                    

                                }
                                else
                                {
                                    SalHead.BASICDependRule = null;
                                }
                                ////BASICDependRule BASICDependRule = db.SalHeadFormula.Include(e => e.BASICDependRule)
                                ////           .Where(e => e.Id == data).Select(e => e.BASICDependRule).SingleOrDefault();

                                ////if (BASICScale != null)
                                ////{
                                ////    int BASICScaleId = int.Parse(BASICScale);
                                ////    var BasicScaleval = db.BasicScale.Include(e => e.BasicScaleDetails).Where(e => e.Id == BASICScaleId).SingleOrDefault();
                                ////    BASICDependRule.BasicScale = BasicScaleval;
                                ////}
                                ////else
                                ////{
                                ////    BASICDependRule.BasicScale = null;
                                ////}
                                ////db.BASICDependRule.Attach(BASICDependRule);
                                ////db.Entry(BASICDependRule).State = System.Data.Entity.EntityState.Modified;
                                ////db.SaveChanges();
                            }

                            // var a = db.SalHeadFormula.Where(e => e.Name == SalHead.Name).Select(e => e.Id).ToList();

                            //foreach (int i in a)
                            //{
                            //    SalHeadFormula SalHeadFor = db.SalHeadFormula.Include(e => e.FuncStruct)
                            //                                .Include(e => e.PayStruct).Include(e => e.GeoStruct).Where(e => e.Id == i).SingleOrDefault();

                            //    //SalHeadFor.AmountDependRule = SalHead.AmountDependRule;
                            //    //SalHeadFor.BASICDependRule = SalHead.BASICDependRule;
                            //    SalHeadFor.CeilingMax = SalHead.CeilingMax;
                            //    SalHeadFor.CeilingMin = SalHead.CeilingMin;
                            //    //SalHeadFor.FuncStruct = SalHeadFor.FuncStruct;
                            //    //SalHeadFor.GeoStruct = SalHeadFor.GeoStruct;
                            //    //SalHeadFor.Name = SalHead.Name;
                            //    //SalHeadFor.PayStruct = SalHeadFor.PayStruct;
                            //    //SalHeadFor.PercentDependRule = SalHead.PercentDependRule;
                            //    SalHeadFor.SalWages = SalHead.SalWages;
                            //    //SalHeadFor.ServiceDependRule = SalHead.ServiceDependRule;
                            //    //SalHeadFor.SlabDependRule = SalHead.SlabDependRule;
                            //    //SalHeadFor.VDADependRule = SalHead.VDADependRule;
                            //    SalHeadFor.DBTrack = SalHead.DBTrack;



                            //    db.SalHeadFormula.Attach(SalHeadFor);
                            //    db.Entry(SalHeadFor).State = System.Data.Entity.EntityState.Modified;
                            //    //db.Entry(SalHeadFor).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //    db.SaveChanges();
                            //    //db.Entry(SalHeadFor).State = System.Data.Entity.EntityState.Detached;
                            //}

                            db.SalHeadFormula.Where(e => e.Name == SalHead.Name).ToList().ForEach(x =>
                            {
                                SalHeadFormula SalHeadFor = db.SalHeadFormula.Include(e => e.FuncStruct)
                                                            .Include(e => e.PayStruct).Include(e => e.GeoStruct).Where(e => e.Id == x.Id).SingleOrDefault();

                                SalHeadFor.CeilingMax = SalHead.CeilingMax;
                                SalHeadFor.CeilingMin = SalHead.CeilingMin;
                                SalHeadFor.SalWages = SalHead.SalWages;
                                SalHeadFor.DBTrack = SalHead.DBTrack;
                                SalHeadFor.VDADependRule = SalHead.VDADependRule;
                            });
                            db.SaveChanges();

                            var OEmpPayroll = db.EmployeePayroll.Include(e => e.EmpSalStruct)
                                 .Include(e => e.Employee)
                                .Include(e => e.Employee.ServiceBookDates)
                                .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                                //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                                //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalHeadFormula)))
                                //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                                //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                                              .AsNoTracking().ToList();
                            foreach (var Emp in OEmpPayroll)
                            {
                                //var b = Emp.EmpSalStruct.Where(e => e.EndDate == null).SingleOrDefault();
                                var EmpSalStruct = Emp.EmpSalStruct.Where(e => e.EndDate == null).SingleOrDefault().Id;
                                var EmpSalstuctDet = db.EmpSalStruct.Include(e => e.EmpSalStructDetails)
                                              .Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula))
                                              .Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead))
                                               .Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.SalHeadOperationType))
                                               .Where(e => e.Id == EmpSalStruct).SingleOrDefault();

                                if (EmpSalstuctDet != null)
                                {
                                    foreach (var Struct in EmpSalstuctDet.EmpSalStructDetails)
                                    {
                                        var SalForm = Struct.SalHeadFormula;
                                        if (SalForm != null)
                                        {
                                            if (SalForm.Name == SalHead.Name)
                                                SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(EmpSalstuctDet, Emp, DateTime.Now);
                                        }
                                    }
                                }
                            }
                            //SalaryHeadGenProcess.EmployeeSalaryStructUpdate(Emp, DateTime.Now, db);

                            //ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = SalHead.Id, Val = SalHead.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { SalHead.Id, SalHead.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                        }
                        catch (DataException /* dex */)
                        {
                            Msg.Add(" Unable to Edit.Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                            // ModelState.AddModelError(string.Empty, "Unable to Edit. Try again, and if the problem persists contact your system administrator.");
                            // return RedirectToAction("Edit");
                        }
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
                        Msg.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new { msg = errorMsg });
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
            //  return View();
        }


        [HttpPost]
        public ActionResult Delete(string data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var id = int.Parse(Session["CompId"].ToString());

                    using (DataBaseContext db2 = new DataBaseContext())
                    {

                        var PayScaleAssign = db2.PayScaleAssignment.Include(e => e.SalHeadFormula).ToList();
                        if (PayScaleAssign.Count > 0)
                        {
                            foreach (var a in PayScaleAssign)
                            {
                                foreach (var b in a.SalHeadFormula)
                                {
                                    if (b.Name == data)
                                    {
                                        Msg.Add("  Data cann't be removed as formula is used  ");
                                        return Json(new Utility.JsonReturnClass { data = data.ToString(), success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                        }
                    }

                    var companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == id).SingleOrDefault();
                    var SalHead = db.SalHeadFormula.Where(e => e.Name == data).ToList();


                    companypayroll.SalHeadFormula = SalHead;
                    companypayroll.SalHeadFormula = null;
                    db.CompanyPayroll.Attach(companypayroll);
                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;

                    //
                    var SalHead1 = db.SalHeadFormula.Where(e => e.Name == data).Select(e => e.Name).Distinct().FirstOrDefault();
                    var v1 = db.SalHeadFormula.Where(a => a.Name == SalHead1).ToList();
                    db.SalHeadFormula.RemoveRange(v1);



                    db.SaveChanges();
                    Msg.Add("  Data removed successfully  ");
                    return Json(new Utility.JsonReturnClass { data = data.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { data, "Data removed." }, JsonRequestBehavior.AllowGet);
                    // return this.Json(new { msg = "Data removed." });
                }
                catch (DbUpdateConcurrencyException)
                {
                    return RedirectToAction("Delete", new { concurrencyError = true, id = data });
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

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                var SalHeadFor = db.SalHeadFormula.Include(e => e.SalWages).ToList();
                IEnumerable<SalHeadFormula> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = SalHeadFor;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                      || (e.Name.ToString().Contains(gp.searchString))
                      || (e.CeilingMin.ToString().Contains(gp.searchString))
                      || (e.CeilingMax.ToString().Contains(gp.searchString))
                      ).Select(a => new Object[] { a.Id, Convert.ToString(a.Name), Convert.ToString(a.CeilingMin), Convert.ToString(a.CeilingMax) }).Where(a => a.Contains((gp.searchString))).ToList();

                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.CeilingMin, a.CeilingMax }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = SalHeadFor;

                    Func<SalHeadFormula, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Name" ? c.Name :
                                         gp.sidx == "CeilingMin" ? c.CeilingMin.ToString() :
                                         gp.sidx == "CeilingMax" ? c.CeilingMax.ToString() : "");
                    }

                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), Convert.ToString(a.CeilingMin), Convert.ToString(a.CeilingMax) }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), Convert.ToString(a.CeilingMin), Convert.ToString(a.CeilingMax) }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.CeilingMax, Convert.ToString(a.CeilingMax) }).ToList();
                    }
                    totalRecords = SalHeadFor.Count();
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


        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public double CeilingMin { get; set; }
            public double CeilingMax { get; set; }
            public string DependRule { get; set; }
        }


        public class FormChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string GeoStruct { get; set; }
            public string PayStruct { get; set; }
            public string FuncStruct { get; set; }
        }

        public ActionResult Formula_Grid(ParamModel param, string y)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var id = int.Parse(SessionManager.CompPayrollId);
                    //var a = db.PayScaleAssignment.Include(e => e.SalHeadFormula).Include(e => e.PayScaleAgreemnt).Where(e => e.Id == data).ToList();
                    //var Sal = db.CompanyPayroll.Where(e => e.Company.Id == id).Include(e => e.SalHeadFormula).Select(e => e.SalHeadFormula).SingleOrDefault();
                    List<SalHeadFormula> SalHeadFormula = db.SalHeadFormula.Where(e=>e.CompanyPayroll_Id== id).OrderBy(e => e.Id).AsNoTracking().AsParallel().ToList();

                    var all = SalHeadFormula.GroupBy(e => e.Name).Select(e => e.FirstOrDefault()).ToList();
                    // for searchs
                    IEnumerable<SalHeadFormula> fall;
                    string DependRule = "";
                    if (param.sSearch == null)
                    {
                        fall = all;

                    }
                    else
                    {
                        if (param.sSearch.All(char.IsDigit) == true)
                        {
                            int SalFormulaId = Convert.ToInt32(param.sSearch);
                            string SalHeadformulaname = db.SalHeadFormula.Find(SalFormulaId) != null ? db.SalHeadFormula.Find(SalFormulaId).Name : "";
                            if (SalHeadformulaname == "")
                            {
                                fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                                                                      || (e.Name.ToUpper().Contains(param.sSearch.ToUpper()))
                                                                                      || (e.CeilingMax.ToString().Contains(param.sSearch))
                                                                                      || (e.CeilingMin.ToString().Contains(param.sSearch))
                                                                                      ).ToList();
                            }
                            else
                            { fall = all.Where(e => (e.Name.ToUpper().Contains(SalHeadformulaname.ToUpper()))).ToList(); }
                            
                        }
                        else
                        {
                            fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                                       || (e.Name.ToUpper().Contains(param.sSearch.ToUpper()))
                                                       || (e.CeilingMax.ToString().Contains(param.sSearch))
                                                       || (e.CeilingMin.ToString().Contains(param.sSearch))
                                                       ).ToList();
                        }
                        

                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<SalHeadFormula, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.Name : "");
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
                        List<returndatagridclass> result = new List<returndatagridclass>();
                        foreach (var item in fall)
                        {
                            var temp = db.SalHeadFormula.Include(e => e.AmountDependRule).Include(e => e.BASICDependRule)
                                .Include(e => e.PercentDependRule).Include(e => e.ServiceDependRule).Include(e => e.SlabDependRule)
                                .Include(e => e.VDADependRule).Where(e => e.Id == item.Id).FirstOrDefault();
                            if (temp.AmountDependRule != null)
                            {
                                DependRule = "AmountDependRule";
                            }
                            else if (temp.PercentDependRule != null)
                            {
                                DependRule = "PercentDependRule";
                            }
                            else if (temp.BASICDependRule != null)
                            {
                                DependRule = "BASICDependRule";
                            }
                            else if (temp.ServiceDependRule != null)
                            {
                                DependRule = "ServiceDependRule";
                            }
                            else if (temp.SlabDependRule != null)
                            {
                                DependRule = "SlabDependRule";
                            }
                            else if (temp.VDADependRule != null)
                            {
                                DependRule = "VDADependRule";
                            }

                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                Name = item.Name != null ? item.Name : null,
                                CeilingMin = item.CeilingMin,
                                CeilingMax = item.CeilingMax,
                                DependRule = DependRule
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

                                     select new[] { null, Convert.ToString(c.Id), c.Name, };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }//for data reterivation
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

        public ActionResult Get_FormulaStructDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int id = Convert.ToInt32(Session["CompId"]);
                    var SalHdForm = db.SalHeadFormula.Where(e => e.Id == data).SingleOrDefault();

                    var db_data = db.SalHeadFormula
                      .Where(e => e.Name == SalHdForm.Name)
                        .Include(e => e.GeoStruct)
                        .Include(e => e.GeoStruct.Location)
                         .Include(e => e.GeoStruct.Location.LocationObj)
                        .Include(e => e.GeoStruct.Department)
                        .Include(e => e.GeoStruct.Department.DepartmentObj)
                        .Include(e => e.PayStruct)
                        .Include(e => e.PayStruct.Grade)
                        .Include(e => e.PayStruct.Level)
                        .Include(e => e.PayStruct.JobStatus)
                        .Include(e => e.PayStruct.JobStatus.EmpStatus)
                        .Include(e => e.PayStruct.JobStatus.EmpActingStatus)
                        .Include(e => e.FuncStruct)
                        .Include(e => e.FuncStruct.Job)
                        .Include(e => e.FuncStruct.JobPosition).ToList();


                    if (db_data != null)
                    {
                        List<FormChildDataClass> returndata = new List<FormChildDataClass>();

                        //foreach (var a in db_data)
                        //{
                        // var SalFor = a.SalHeadFormula.Where(e => e.Name == SalHdForm.Name);
                        foreach (var item in db_data)
                        {
                            returndata.Add(new FormChildDataClass
                            {
                                Id = item.Id,
                                GeoStruct = item.GeoStruct != null ? item.GeoStruct.FullDetailsLD : "",
                                FuncStruct = item.FuncStruct != null ? item.FuncStruct.FullDetails : "",
                                PayStruct = item.PayStruct != null ? item.PayStruct.FullDetails : "",

                            });
                        }
                        //}
                        return Json(returndata, JsonRequestBehavior.AllowGet);
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

        public ActionResult GridDelete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int id = Convert.ToInt32(Session["CompId"]);

                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                        new System.TimeSpan(0, 30, 0)))
                    {

                        var salheadformula = db.SalHeadFormula.Include(e => e.PayScaleAssignment).Where(e => e.Id == data).SingleOrDefault();
                        var companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == id).SingleOrDefault();
                        companypayroll.SalHeadFormula.Where(e => e.Id == salheadformula.Id).SingleOrDefault();
                        companypayroll.SalHeadFormula = null;
                        db.CompanyPayroll.Attach(companypayroll);
                        db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;

                        if (salheadformula.PayStruct != null)
                        {
                            var OEmpPayroll = db.EmployeePayroll.Include(e => e.Employee).ToList();
                            foreach (var Emp in OEmpPayroll)
                            {
                                if (Emp.Employee.PayStruct.Id == salheadformula.PayStruct.Id)
                                {
                                    SalaryHeadGenProcess.EmployeeSalaryStructUpdate(Emp, DateTime.Now);
                                }
                            }
                        }
                        if (salheadformula.GeoStruct != null)
                        {
                            var OEmpPayroll = db.EmployeePayroll.Include(e => e.Employee).ToList();
                            foreach (var Emp in OEmpPayroll)
                            {
                                if (Emp.Employee.GeoStruct.Id == salheadformula.GeoStruct.Id)
                                {
                                    SalaryHeadGenProcess.EmployeeSalaryStructUpdate(Emp, DateTime.Now);
                                }
                            }
                        }
                        if (salheadformula.FuncStruct != null)
                        {
                            var OEmpPayroll = db.EmployeePayroll.Include(e => e.Employee).ToList();
                            foreach (var Emp in OEmpPayroll)
                            {
                                if (Emp.Employee.FuncStruct.Id == salheadformula.FuncStruct.Id)
                                {
                                    SalaryHeadGenProcess.EmployeeSalaryStructUpdate(Emp, DateTime.Now);
                                }
                            }
                        }
                        db.SaveChanges();
                        ts.Complete();
                        Msg.Add("  Record Deleted");
                        return Json(new Utility.JsonReturnClass { data = "", success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new { status = true, data = "", responseText = "Record Deleted..!" }, JsonRequestBehavior.AllowGet);


                    }
                }
                catch (DataException /* dex */)
                {
                    //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                    //ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                    //return View(level);
                    Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
    }

}
