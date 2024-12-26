using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Payroll;
using Training;
using Attendance;
using Leave;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class LTCFormulaController : Controller
    {
        //
        // GET: /LTCFormula/
       //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/LTCFormula/Index.cshtml");
        }


        public ActionResult GetLookupTravelEligPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TravelEligibilityPolicy.Include(e => e.TravelModeEligibilityPolicy).ToList();
                IEnumerable<TravelEligibilityPolicy> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.TravelEligibilityPolicy.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }

        }

        public ActionResult GetLookupHotelEligPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.HotelEligibilityPolicy.ToList();
                IEnumerable<HotelEligibilityPolicy> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.HotelEligibilityPolicy.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }

        }
        public ActionResult GetLookupGlobalLTCBlockPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.GlobalLTCBlock.ToList();
                IEnumerable<GlobalLTCBlock> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.GlobalLTCBlock.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }

        }
        public ActionResult GetLookupDAEligibilityPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.DAEligibilityPolicy.Include(e => e.SlabDependRule).ToList();
                IEnumerable<TravelModeEligibilityPolicy> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.DAEligibilityPolicy.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }

        }
        public ActionResult GetLookupTravelModeEligPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TravelModeEligibilityPolicy.Include(e => e.TravelMode).ToList();
                IEnumerable<TravelModeEligibilityPolicy> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.TravelModeEligibilityPolicy.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }

        }

        public ActionResult GetLookupTravelModeRateCeilPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TravelModeRateCeilingPolicy.Include(e => e.TravelModeEligibilityPolicy).ToList();
                IEnumerable<TravelModeRateCeilingPolicy> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.TravelModeRateCeilingPolicy.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }

        }


        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(LTCFormula L, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string selectedValue = form["radio"];
                    string geo_id = form["geo_id"] == "0" ? "" : form["geo_id"];
                    string fun_id = form["fun_id"] == "0" ? "" : form["fun_id"];
                    string pay_id = form["pay_id"] == "0" ? "" : form["pay_id"];


                    if (geo_id == "" && fun_id == "" && pay_id == "")
                    {
                        Msg.Add("  Kindly Apply Advance Filter..!!! ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (selectedValue == null)
                    {
                        Msg.Add("  Kindly select Policy rule  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    int CompId = 0;
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = int.Parse(Session["CompId"].ToString());
                    }

                    CompanyPayroll OCompanyPayroll = null;
                    List<LTCFormula> OFAT = new List<LTCFormula>();
                    OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == CompId).SingleOrDefault();


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            try
                            {
                                List<HotelEligibilityPolicy> ObjHotelEligPolicy = new List<HotelEligibilityPolicy>();
                                if (selectedValue == "1")
                                {
                                    string HotelEligPolicyList = form["HotelEligibilityPolicyList"];
                                    if (HotelEligPolicyList != null && HotelEligPolicyList != "")
                                    {
                                        //var value = db.OthServiceBookPolicy.Find(int.Parse(OthServiceBookPolicyList));
                                        //L.OthServiceBookPolicy = value;
                                        var ids = Utility.StringIdsToListIds(HotelEligPolicyList);
                                        foreach (var ca in ids)
                                        {
                                            var value = db.HotelEligibilityPolicy.Find(ca);
                                            ObjHotelEligPolicy.Add(value);
                                            L.HotelEligibilityPolicy = ObjHotelEligPolicy;
                                        }
                                    }
                                }

                                List<TravelEligibilityPolicy> ObjTravelEligPolicy = new List<TravelEligibilityPolicy>();
                                if (selectedValue == "2")
                                {
                                    string TravelEligPolicyList = form["TravelEligibilityPolicyList"];
                                    if (TravelEligPolicyList != null && TravelEligPolicyList != "")
                                    {
                                        //var value = db.IncrPolicy.Find(int.Parse(IncrPolicyList));
                                        //L.IncrPolicy = value;
                                        var ids = Utility.StringIdsToListIds(TravelEligPolicyList);
                                        foreach (var ca in ids)
                                        {
                                            var value = db.TravelEligibilityPolicy.Find(ca);
                                            ObjTravelEligPolicy.Add(value);
                                            L.TravelEligibilityPolicy = ObjTravelEligPolicy;
                                        }
                                    }

                                }

                                List<TravelModeEligibilityPolicy> ObjTravelModeEligPolicy = new List<TravelModeEligibilityPolicy>();
                                if (selectedValue == "3")
                                {

                                    string TravelModeEligPolicyList = form["TravelModeEligibilityPolicyList"];
                                    if (TravelModeEligPolicyList != null && TravelModeEligPolicyList != "")
                                    {
                                        //var value = db.PromoPolicy.Find(int.Parse(PromoPolicyList));
                                        //L.PromoPolicy = value;
                                        var ids = Utility.StringIdsToListIds(TravelModeEligPolicyList);
                                        foreach (var ca in ids)
                                        {
                                            var value = db.TravelModeEligibilityPolicy.Find(ca);
                                            ObjTravelModeEligPolicy.Add(value);
                                            L.TravelModeEligibilityPolicy = ObjTravelModeEligPolicy;
                                        }
                                    }
                                }

                                List<TravelModeRateCeilingPolicy> ObjTravelModeRateCeilPolicy = new List<TravelModeRateCeilingPolicy>();
                                if (selectedValue == "4")
                                {
                                    string TravelModeRateCeilPolicyList = form["TravelModeRateCeilingPolicyList"];
                                    if (TravelModeRateCeilPolicyList != null && TravelModeRateCeilPolicyList != "")
                                    {
                                        //var value = db.TransPolicy.Find(int.Parse(TransPolicyList));
                                        //L.TransPolicy = value;
                                        var ids = Utility.StringIdsToListIds(TravelModeRateCeilPolicyList);
                                        foreach (var ca in ids)
                                        {
                                            var value = db.TravelModeRateCeilingPolicy.Find(ca);
                                            ObjTravelModeRateCeilPolicy.Add(value);
                                            L.TravelModeRateCeilingPolicy = ObjTravelModeRateCeilPolicy;
                                        }
                                    }
                                }

                               

                                List<GlobalLTCBlock> ObjGlobalLTCBlockPolicy = new List<GlobalLTCBlock>();
                                if (selectedValue == "5")
                                {
                                    string GlobalLTCBlockPolicyList = form["GlobalLTCBlockPolicyList"];
                                    if (GlobalLTCBlockPolicyList != null && GlobalLTCBlockPolicyList != "")
                                    {
                                        //var value = db.OthServiceBookPolicy.Find(int.Parse(OthServiceBookPolicyList));
                                        //L.OthServiceBookPolicy = value;
                                        var ids = Utility.StringIdsToListIds(GlobalLTCBlockPolicyList);
                                        foreach (var ca in ids)
                                        {
                                            var value = db.GlobalLTCBlock.Find(ca);
                                            ObjGlobalLTCBlockPolicy.Add(value);
                                            L.GlobalLTCBlock = ObjGlobalLTCBlockPolicy;
                                        }
                                    }
                                }

                                List<DAEligibilityPolicy> ObjDAEligibilityPolicy = new List<DAEligibilityPolicy>();
                                if (selectedValue == "6")
                                {
                                    string DAEligibilityPolicyList = form["DAEligibilityPolicyList"];
                                    if (DAEligibilityPolicyList != null && DAEligibilityPolicyList != "")
                                    {
                                        //var value = db.OthServiceBookPolicy.Find(int.Parse(OthServiceBookPolicyList));
                                        //L.OthServiceBookPolicy = value;
                                        var ids = Utility.StringIdsToListIds(DAEligibilityPolicyList);
                                        foreach (var ca in ids)
                                        {
                                            var value = db.DAEligibilityPolicy.Find(ca);
                                            ObjDAEligibilityPolicy.Add(value);
                                            L.DAEligibilityPolicy = ObjDAEligibilityPolicy;
                                        }
                                    }
                                }


                                L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

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
                                                L.GeoStruct = geo;

                                                PayStruct pay = db.PayStruct.Find(P);
                                                L.PayStruct = pay;

                                                FuncStruct func = db.FuncStruct.Find(F);
                                                L.FuncStruct = func;

                                                LTCFormula Policy = new LTCFormula()
                                                {
                                                    Name = L.Name,
                                                    HotelEligibilityPolicy = L.HotelEligibilityPolicy,
                                                    TravelEligibilityPolicy = L.TravelEligibilityPolicy,
                                                    TravelModeEligibilityPolicy = L.TravelModeEligibilityPolicy,
                                                    TravelModeRateCeilingPolicy = L.TravelModeRateCeilingPolicy,
                                                    GlobalLTCBlock=L.GlobalLTCBlock,
                                                    DAEligibilityPolicy=L.DAEligibilityPolicy,
                                                    FuncStruct = L.FuncStruct,
                                                    GeoStruct = L.GeoStruct,
                                                    PayStruct = L.PayStruct,
                                                    DBTrack = L.DBTrack,
                                                };


                                                db.LTCFormula.Add(Policy);
                                                db.SaveChanges();

                                                OFAT.Add(db.LTCFormula.Find(Policy.Id));
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
                                            L.GeoStruct = geo;

                                            PayStruct pay = db.PayStruct.Find(P);
                                            L.PayStruct = pay;

                                            L.FuncStruct = null;

                                            LTCFormula Policy = new LTCFormula()
                                            {
                                                Name = L.Name,
                                                HotelEligibilityPolicy = L.HotelEligibilityPolicy,
                                                TravelEligibilityPolicy = L.TravelEligibilityPolicy,
                                                TravelModeEligibilityPolicy = L.TravelModeEligibilityPolicy,
                                                TravelModeRateCeilingPolicy = L.TravelModeRateCeilingPolicy,
                                                GlobalLTCBlock = L.GlobalLTCBlock,
                                                DAEligibilityPolicy = L.DAEligibilityPolicy,
                                                FuncStruct = L.FuncStruct,
                                                GeoStruct = L.GeoStruct,
                                                PayStruct = L.PayStruct,
                                                DBTrack = L.DBTrack,
                                            };


                                            db.LTCFormula.Add(Policy);
                                            db.SaveChanges();

                                            OFAT.Add(db.LTCFormula.Find(Policy.Id));
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
                                            L.GeoStruct = geo;

                                            L.PayStruct = null;

                                            FuncStruct func = db.FuncStruct.Find(P);
                                            L.FuncStruct = func;

                                            LTCFormula Policy = new LTCFormula()
                                            {
                                                Name = L.Name,
                                                HotelEligibilityPolicy = L.HotelEligibilityPolicy,
                                                TravelEligibilityPolicy = L.TravelEligibilityPolicy,
                                                TravelModeEligibilityPolicy = L.TravelModeEligibilityPolicy,
                                                TravelModeRateCeilingPolicy = L.TravelModeRateCeilingPolicy,
                                                GlobalLTCBlock = L.GlobalLTCBlock,
                                                DAEligibilityPolicy = L.DAEligibilityPolicy,
                                                FuncStruct = L.FuncStruct,
                                                GeoStruct = L.GeoStruct,
                                                PayStruct = L.PayStruct,
                                                DBTrack = L.DBTrack,
                                            };


                                            db.LTCFormula.Add(Policy);
                                            db.SaveChanges();

                                            OFAT.Add(db.LTCFormula.Find(Policy.Id));
                                        }

                                    }

                                }

                                if ((geo_id != "") && (fun_id == "") && (pay_id == ""))
                                {
                                    var ids = Utility.StringIdsToListIds(geo_id);

                                    foreach (var G in ids)
                                    {
                                        GeoStruct geo = db.GeoStruct.Find(G);
                                        L.GeoStruct = geo;
                                        L.PayStruct = null;
                                        L.FuncStruct = null;

                                        LTCFormula Policy = new LTCFormula()
                                        {
                                            Name = L.Name,
                                            HotelEligibilityPolicy = L.HotelEligibilityPolicy,
                                            TravelEligibilityPolicy = L.TravelEligibilityPolicy,
                                            TravelModeEligibilityPolicy = L.TravelModeEligibilityPolicy,
                                            TravelModeRateCeilingPolicy = L.TravelModeRateCeilingPolicy,
                                            GlobalLTCBlock = L.GlobalLTCBlock,
                                            DAEligibilityPolicy = L.DAEligibilityPolicy,
                                            FuncStruct = L.FuncStruct,
                                            GeoStruct = L.GeoStruct,
                                            PayStruct = L.PayStruct,
                                            DBTrack = L.DBTrack,
                                        };


                                        db.LTCFormula.Add(Policy);
                                        db.SaveChanges();

                                        OFAT.Add(db.LTCFormula.Find(Policy.Id));
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

                                        L.GeoStruct = null;
                                        L.PayStruct = null;

                                        FuncStruct func = db.FuncStruct.Find(F);
                                        L.FuncStruct = func;

                                        LTCFormula Policy = new LTCFormula()
                                        {
                                            Name = L.Name,
                                            HotelEligibilityPolicy = L.HotelEligibilityPolicy,
                                            TravelEligibilityPolicy = L.TravelEligibilityPolicy,
                                            TravelModeEligibilityPolicy = L.TravelModeEligibilityPolicy,
                                            TravelModeRateCeilingPolicy = L.TravelModeRateCeilingPolicy,
                                            GlobalLTCBlock = L.GlobalLTCBlock,
                                            DAEligibilityPolicy = L.DAEligibilityPolicy,
                                            FuncStruct = L.FuncStruct,
                                            GeoStruct = L.GeoStruct,
                                            PayStruct = L.PayStruct,
                                            DBTrack = L.DBTrack,
                                        };


                                        db.LTCFormula.Add(Policy);
                                        db.SaveChanges();

                                        OFAT.Add(db.LTCFormula.Find(Policy.Id));
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

                                        L.GeoStruct = null;
                                        PayStruct pay = db.PayStruct.Find(P);
                                        L.PayStruct = pay;
                                        L.FuncStruct = null;

                                        LTCFormula Policy = new LTCFormula()
                                        {
                                            Name = L.Name,
                                            HotelEligibilityPolicy = L.HotelEligibilityPolicy,
                                            TravelEligibilityPolicy = L.TravelEligibilityPolicy,
                                            TravelModeEligibilityPolicy = L.TravelModeEligibilityPolicy,
                                            TravelModeRateCeilingPolicy = L.TravelModeRateCeilingPolicy,
                                            GlobalLTCBlock = L.GlobalLTCBlock,
                                            DAEligibilityPolicy = L.DAEligibilityPolicy,
                                            FuncStruct = L.FuncStruct,
                                            GeoStruct = L.GeoStruct,
                                            PayStruct = L.PayStruct,
                                            DBTrack = L.DBTrack,
                                        };


                                        db.LTCFormula.Add(Policy);
                                        db.SaveChanges();

                                        OFAT.Add(db.LTCFormula.Find(Policy.Id));
                                    }
                                }

                                if (geo_id == "" && pay_id == "" && fun_id == "")
                                {
                                    var geoList = db.GeoStruct.Where(e => e.Company != null && e.Company.Id == CompId).ToList();

                                    foreach (var geo in geoList)
                                    {

                                        L.GeoStruct = null;
                                        L.PayStruct = null;
                                        L.FuncStruct = null;

                                        LTCFormula Policy = new LTCFormula()
                                        {
                                            Name = L.Name,
                                            HotelEligibilityPolicy = L.HotelEligibilityPolicy,
                                            TravelEligibilityPolicy = L.TravelEligibilityPolicy,
                                            TravelModeEligibilityPolicy = L.TravelModeEligibilityPolicy,
                                            TravelModeRateCeilingPolicy = L.TravelModeRateCeilingPolicy,
                                            GlobalLTCBlock = L.GlobalLTCBlock,
                                            DAEligibilityPolicy = L.DAEligibilityPolicy,
                                            FuncStruct = L.FuncStruct,
                                            GeoStruct = L.GeoStruct,
                                            PayStruct = L.PayStruct,
                                            DBTrack = L.DBTrack,
                                        };


                                        db.LTCFormula.Add(Policy);
                                        db.SaveChanges();

                                        OFAT.Add(db.LTCFormula.Find(Policy.Id));
                                    }
                                }



                                if (OCompanyPayroll == null)
                                {
                                    CompanyPayroll OTEP = new CompanyPayroll()
                                    {
                                        Company = db.Company.Find(SessionManager.CompanyId),
                                        LTCFormula  = OFAT,
                                        DBTrack = L.DBTrack

                                    };


                                    db.CompanyPayroll.Add(OTEP);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    var aa = db.CompanyPayroll.Find(OCompanyPayroll.Id);

                                    if (aa.LTCFormula != null)
                                        OFAT.AddRange(aa.LTCFormula);
                                    aa.LTCFormula = OFAT;
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
                return View();
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
                IEnumerable<LTCFormula> lencash = null;
                if (gp.IsAutho == true)
                {
                    lencash = db.LTCFormula.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    lencash = db.LTCFormula.AsNoTracking().ToList();
                }

                IEnumerable<LTCFormula> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = lencash;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.Name }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Name.ToString() == gp.searchString.ToLower()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = lencash;
                    Func<LTCFormula, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Name" ? c.Name.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Name }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Name }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name }).ToList();
                    }
                    totalRecords = lencash.Count();
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

        public class LTCFormulaEditdetails
        {
            public Array TravelEligibilityPolicy_Id { get; set; }
            public string TravelEligibilityPolicys_Fulldetails { get; set; }

            public Array HotelEligibilityPolicy_Id { get; set; }
            public string HotelEligibilityPolicy_Fulldetails { get; set; }

            public Array TravelModeEligibilityPolicy_Id { get; set; }
            public string TravelModeEligibilityPolicy_Fulldetails { get; set; }

            public Array TravelModeRateCeilingPolicy_Id { get; set; }
            public string TravelModeRateCeilingPolicy_Fulldetails { get; set; }

            public Array GlobalLTCBlockPolicy_Id { get; set; }
            public string GlobalLTCBlockPolicy_Fulldetails { get; set; }

            public Array DAEligibilityPolicy_Id { get; set; }
            public string DAEligibilityPolicy_Fulldetails { get; set; }


        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int selected = 0;
                List<LTCFormulaEditdetails> return_data = new List<LTCFormulaEditdetails>();

                var Q = db.LTCFormula
                   .Where(e => e.Id == data).Select
                   (e => new
                   {
                       Name = e.Name,
                   }).ToList();


                var ObjLVCP = db.LTCFormula.Include(e => e.HotelEligibilityPolicy).Where(e => e.Id == data).Select(e => e.HotelEligibilityPolicy).SingleOrDefault();

                if (ObjLVCP != null && ObjLVCP.Count() > 0)
                {
                    foreach (var ca in ObjLVCP)
                    {

                        return_data.Add(
                           new LTCFormulaEditdetails
                           {
                               HotelEligibilityPolicy_Id = ca.Id.ToString().ToArray(),
                               HotelEligibilityPolicy_Fulldetails  = ca.FullDetails.ToString()

                           });
                    }

                    selected = 1;

                    var SalHead = db.LTCFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;

                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);

                }


                var ObjLVDP = db.LTCFormula.Include(e => e.TravelEligibilityPolicy).Where(e => e.Id == data).Select(e => e.TravelEligibilityPolicy).SingleOrDefault();

                if (ObjLVDP != null && ObjLVDP.Count() > 0)
                {
                    foreach (var ca in ObjLVDP)
                    {

                        return_data.Add(
                           new LTCFormulaEditdetails
                           {
                               TravelEligibilityPolicy_Id = ca.Id.ToString().ToArray(),
                               TravelEligibilityPolicys_Fulldetails = ca.FullDetails.ToString(),

                           });

                    }
                    selected = 2;

                    var SalHead = db.LTCFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;

                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);

                }


                var ObjLVEP = db.LTCFormula.Include(e => e.TravelModeEligibilityPolicy).Where(e => e.Id == data).Select(e => e.TravelModeEligibilityPolicy).SingleOrDefault();

                if (ObjLVEP != null && ObjLVEP.Count() > 0)
                {
                    foreach (var ca in ObjLVEP)
                    {

                        return_data.Add(
                           new LTCFormulaEditdetails
                           {
                               TravelModeEligibilityPolicy_Id  = ca.Id.ToString().ToArray(),
                               TravelModeEligibilityPolicy_Fulldetails = ca.FullDetails.ToString()

                           });

                    }

                    selected = 3;

                    var SalHead = db.LTCFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;

                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);

                }

                var ObjLVBP = db.LTCFormula.Include(e => e.TravelModeRateCeilingPolicy).Where(e => e.Id == data).Select(e => e.TravelModeRateCeilingPolicy).SingleOrDefault();

                if (ObjLVBP != null && ObjLVBP.Count() > 0)
                {
                    foreach (var ca in ObjLVBP)
                    {

                        return_data.Add(
                           new LTCFormulaEditdetails
                           {
                               TravelModeRateCeilingPolicy_Id = ca.Id.ToString().ToArray(),
                               TravelModeRateCeilingPolicy_Fulldetails = ca.FullDetails.ToString()

                           });

                    }
                    selected = 4;



                    var SalHead = db.LTCFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;

                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);

                }

                var ObjLVBPG = db.LTCFormula.Include(e => e.GlobalLTCBlock).Where(e => e.Id == data).Select(e => e.GlobalLTCBlock).SingleOrDefault();

                if (ObjLVBPG != null && ObjLVBPG.Count() > 0)
                {
                    foreach (var ca in ObjLVBPG)
                    {

                        return_data.Add(
                           new LTCFormulaEditdetails
                           {
                               GlobalLTCBlockPolicy_Id = ca.Id.ToString().ToArray(),
                               GlobalLTCBlockPolicy_Fulldetails = ca.FullDetails.ToString()

                           });

                    }
                    selected = 5;



                    var SalHead = db.LTCFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;

                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);

                }

                var ObjLVBPGD = db.LTCFormula.Include(e => e.DAEligibilityPolicy).Where(e => e.Id == data).Select(e => e.DAEligibilityPolicy).SingleOrDefault();

                if (ObjLVBPGD != null && ObjLVBPGD.Count() > 0)
                {
                    foreach (var ca in ObjLVBPGD)
                    {

                        return_data.Add(
                           new LTCFormulaEditdetails
                           {
                               DAEligibilityPolicy_Id = ca.Id.ToString().ToArray(),
                               DAEligibilityPolicy_Fulldetails = ca.FullDetails.ToString()

                           });

                    }
                    selected = 6;



                    var SalHead = db.LTCFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;

                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);

                }



                return Json(new object[] { Q, return_data, "", selected.ToString(), null }, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public async Task<ActionResult> EditSave(LTCFormula L, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string HotelEligibilityPolicyList = form["HotelEligibilityPolicyList"];
                    string TravelEligibilityPolicyList = form["TravelEligibilityPolicyList"];
                    string TravelModeEligibilityPolicyList = form["TravelModeEligibilityPolicyList"];
                    string TravelModeRateCeilingPolicyList = form["TravelModeRateCeilingPolicyList"];
                    string GlobalLTCBlockPolicyList = form["GlobalLTCBlockPolicyList"];
                    string DAEligibilityPolicyList = form["DAEligibilityPolicyList"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    string selectedValue = form["radio"];
                    string geo_id = form["geo_id"] == "0" ? "" : form["geo_id"];
                    string fun_id = form["fun_id"] == "0" ? "" : form["fun_id"];
                    string pay_id = form["pay_id"] == "0" ? "" : form["pay_id"];

                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    LTCFormula blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {

                                        blog = context.LTCFormula.Where(e => e.Id == data)
                                                                .Include(e => e.HotelEligibilityPolicy)
                                                                .Include(e => e.TravelEligibilityPolicy)
                                                                .Include(e => e.TravelModeEligibilityPolicy)
                                                                .Include(e => e.TravelModeRateCeilingPolicy)
                                                                .Include(e => e.GlobalLTCBlock)
                                                                 .Include(e => e.DAEligibilityPolicy)
                                                                .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    L.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    if (selectedValue == "1")
                                    {
                                        if (HotelEligibilityPolicyList != null && HotelEligibilityPolicyList != "")
                                        {
                                           
                                                List<HotelEligibilityPolicy> HotelEligibilityPolicyval = new List<HotelEligibilityPolicy>();
                                                LTCFormula typedetails = db.LTCFormula.Include(e => e.HotelEligibilityPolicy).Where(e => e.Id == data).SingleOrDefault();
                                                typedetails.DBTrack = new DBTrack
                                                {
                                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                                    Action = "M",
                                                    ModifiedBy = SessionManager.UserName,
                                                    ModifiedOn = DateTime.Now
                                                };

                                                if (HotelEligibilityPolicyList != null && HotelEligibilityPolicyList != "")
                                                {
                                                    var ids = Utility.StringIdsToListIds(HotelEligibilityPolicyList);
                                                    foreach (var ca in ids)
                                                    {
                                                        var PayScaleval_val = db.HotelEligibilityPolicy.Find(ca);
                                                        HotelEligibilityPolicyval.Add(PayScaleval_val);
                                                        typedetails.HotelEligibilityPolicy = HotelEligibilityPolicyval;
                                                    }
                                                }
                                                else
                                                {
                                                    typedetails.HotelEligibilityPolicy = null;
                                                }


                                                db.LTCFormula.Attach(typedetails);
                                                db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                         
                                            
                                        }
                                        
                                    }


                                    if (selectedValue == "2")
                                    {
                                        if (TravelEligibilityPolicyList != null && TravelEligibilityPolicyList != "")
                                        {

                                            List<TravelEligibilityPolicy> TravelEligibilityPolicyListval = new List<TravelEligibilityPolicy>();
                                            LTCFormula typedetails = db.LTCFormula.Include(e => e.TravelEligibilityPolicy).Where(e => e.Id == data).SingleOrDefault();
                                            typedetails.DBTrack = new DBTrack
                                            {
                                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                                Action = "M",
                                                ModifiedBy = SessionManager.UserName,
                                                ModifiedOn = DateTime.Now
                                            };

                                            if (TravelEligibilityPolicyList != null && TravelEligibilityPolicyList != "")
                                            {
                                                var ids = Utility.StringIdsToListIds(TravelEligibilityPolicyList);
                                                foreach (var ca in ids)
                                                {
                                                    var PayScaleval_val = db.TravelEligibilityPolicy.Find(ca);
                                                    TravelEligibilityPolicyListval.Add(PayScaleval_val);
                                                    typedetails.TravelEligibilityPolicy = TravelEligibilityPolicyListval;
                                                }
                                            }
                                            else
                                            {
                                                typedetails.TravelEligibilityPolicy = null;
                                            }


                                            db.LTCFormula.Attach(typedetails);
                                            db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();


                                        }
                                         
                                    }

                                    if (selectedValue == "3")
                                    {
                                        if (TravelModeEligibilityPolicyList != null && TravelModeEligibilityPolicyList != "")
                                        {

                                            List<TravelModeEligibilityPolicy> TravelModeEligibilityPolicyListval = new List<TravelModeEligibilityPolicy>();
                                            LTCFormula typedetails = db.LTCFormula.Include(e => e.TravelModeEligibilityPolicy).Where(e => e.Id == data).SingleOrDefault();
                                            typedetails.DBTrack = new DBTrack
                                            {
                                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                                Action = "M",
                                                ModifiedBy = SessionManager.UserName,
                                                ModifiedOn = DateTime.Now
                                            };

                                            if (TravelModeEligibilityPolicyList != null && TravelModeEligibilityPolicyList != "")
                                            {
                                                var ids = Utility.StringIdsToListIds(TravelModeEligibilityPolicyList);
                                                foreach (var ca in ids)
                                                {
                                                    var PayScaleval_val = db.TravelModeEligibilityPolicy.Find(ca);
                                                    TravelModeEligibilityPolicyListval.Add(PayScaleval_val);
                                                    typedetails.TravelModeEligibilityPolicy = TravelModeEligibilityPolicyListval;
                                                }
                                            }
                                            else
                                            {
                                                typedetails.TravelModeEligibilityPolicy = null;
                                            }


                                            db.LTCFormula.Attach(typedetails);
                                            db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();


                                        }
                                       
                                    }
                                    if (selectedValue == "4")
                                    {
                                        if (TravelModeRateCeilingPolicyList != null && TravelModeRateCeilingPolicyList != "")
                                        {

                                            List<TravelModeRateCeilingPolicy> TravelModeRateCeilingPolicyListval = new List<TravelModeRateCeilingPolicy>();
                                            LTCFormula typedetails = db.LTCFormula.Include(e => e.TravelModeRateCeilingPolicy).Where(e => e.Id == data).SingleOrDefault();
                                            typedetails.DBTrack = new DBTrack
                                            {
                                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                                Action = "M",
                                                ModifiedBy = SessionManager.UserName,
                                                ModifiedOn = DateTime.Now
                                            };

                                            if (TravelModeRateCeilingPolicyList != null && TravelModeRateCeilingPolicyList != "")
                                            {
                                                var ids = Utility.StringIdsToListIds(TravelModeRateCeilingPolicyList);
                                                foreach (var ca in ids)
                                                {
                                                    var PayScaleval_val = db.TravelModeRateCeilingPolicy.Find(ca);
                                                    TravelModeRateCeilingPolicyListval.Add(PayScaleval_val);
                                                    typedetails.TravelModeRateCeilingPolicy = TravelModeRateCeilingPolicyListval;
                                                }
                                            }
                                            else
                                            {
                                                typedetails.TravelModeRateCeilingPolicy = null;
                                            }


                                            db.LTCFormula.Attach(typedetails);
                                            db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();


                                        }
                                    }

                                    if (selectedValue == "5")
                                    {
                                        if (GlobalLTCBlockPolicyList != null && GlobalLTCBlockPolicyList != "")
                                        {

                                            List<GlobalLTCBlock> GlobalLTCBlockPolicyListval = new List<GlobalLTCBlock>();
                                            LTCFormula typedetails = db.LTCFormula.Include(e => e.GlobalLTCBlock).Where(e => e.Id == data).SingleOrDefault();
                                            typedetails.DBTrack = new DBTrack
                                            {
                                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                                Action = "M",
                                                ModifiedBy = SessionManager.UserName,
                                                ModifiedOn = DateTime.Now
                                            };

                                            if (GlobalLTCBlockPolicyList != null && GlobalLTCBlockPolicyList != "")
                                            {
                                                var ids = Utility.StringIdsToListIds(GlobalLTCBlockPolicyList);
                                                foreach (var ca in ids)
                                                {
                                                    var PayScaleval_val = db.GlobalLTCBlock.Find(ca);
                                                    GlobalLTCBlockPolicyListval.Add(PayScaleval_val);
                                                    typedetails.GlobalLTCBlock = GlobalLTCBlockPolicyListval;
                                                }
                                            }
                                            else
                                            {
                                                typedetails.GlobalLTCBlock = null;
                                            }


                                            db.LTCFormula.Attach(typedetails);
                                            db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();


                                        }
                                    }

                                    if (selectedValue == "6")
                                    {
                                        if (DAEligibilityPolicyList != null && DAEligibilityPolicyList != "")
                                        {

                                            List<DAEligibilityPolicy> DAEligibilityPolicyListval = new List<DAEligibilityPolicy>();
                                            LTCFormula typedetails = db.LTCFormula.Include(e => e.DAEligibilityPolicy).Where(e => e.Id == data).SingleOrDefault();
                                            typedetails.DBTrack = new DBTrack
                                            {
                                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                                Action = "M",
                                                ModifiedBy = SessionManager.UserName,
                                                ModifiedOn = DateTime.Now
                                            };

                                            if (DAEligibilityPolicyList != null && DAEligibilityPolicyList != "")
                                            {
                                                var ids = Utility.StringIdsToListIds(DAEligibilityPolicyList);
                                                foreach (var ca in ids)
                                                {
                                                    var PayScaleval_val = db.DAEligibilityPolicy.Find(ca);
                                                    DAEligibilityPolicyListval.Add(PayScaleval_val);
                                                    typedetails.DAEligibilityPolicy = DAEligibilityPolicyListval;
                                                }
                                            }
                                            else
                                            {
                                                typedetails.GlobalLTCBlock = null;
                                            }


                                            db.LTCFormula.Attach(typedetails);
                                            db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();


                                        }
                                    }


                                    var CurCorp = db.LTCFormula.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        if (geo_id != null && geo_id != "")
                                        {
                                            var ids = Utility.StringIdsToListIds(geo_id);

                                            foreach (var G in ids)
                                            {
                                                var geo = db.GeoStruct.Find(G);
                                                L.GeoStruct = geo;
                                                LTCFormula LTCFormula = new LTCFormula()
                                                {
                                                    Name = L.Name,
                                                    HotelEligibilityPolicy = L.HotelEligibilityPolicy,
                                                    TravelEligibilityPolicy = L.TravelEligibilityPolicy,
                                                    TravelModeEligibilityPolicy = L.TravelModeEligibilityPolicy,
                                                    TravelModeRateCeilingPolicy = L.TravelModeRateCeilingPolicy,
                                                    GlobalLTCBlock = L.GlobalLTCBlock,
                                                    DAEligibilityPolicy = L.DAEligibilityPolicy,
                                                    GeoStruct = L.GeoStruct,
                                                    FuncStruct = L.FuncStruct,
                                                    PayStruct = L.PayStruct,
                                                    Id = data,
                                                    DBTrack = L.DBTrack
                                                };
                                                db.LTCFormula.Attach(LTCFormula);
                                                db.Entry(LTCFormula).State = System.Data.Entity.EntityState.Modified;
                                                db.Entry(LTCFormula).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                                db.Entry(LTCFormula).State = System.Data.Entity.EntityState.Detached;

                                            }
                                        }

                                        if (fun_id != null && fun_id != "")
                                        {
                                            var ids = Utility.StringIdsToListIds(fun_id);

                                            foreach (var F in ids)
                                            {
                                                var fun = db.FuncStruct.Find(F);
                                                L.FuncStruct = fun;
                                                LTCFormula LTCFormula = new LTCFormula()
                                                {

                                                    HotelEligibilityPolicy = L.HotelEligibilityPolicy,
                                                    TravelEligibilityPolicy = L.TravelEligibilityPolicy,
                                                    TravelModeEligibilityPolicy = L.TravelModeEligibilityPolicy,
                                                    TravelModeRateCeilingPolicy = L.TravelModeRateCeilingPolicy,
                                                    GlobalLTCBlock = L.GlobalLTCBlock,
                                                    DAEligibilityPolicy = L.DAEligibilityPolicy,
                                                    GeoStruct = L.GeoStruct,
                                                    FuncStruct = L.FuncStruct,
                                                    PayStruct = L.PayStruct,
                                                    Id = data,
                                                    DBTrack = L.DBTrack
                                                };
                                                db.LTCFormula.Add(LTCFormula);
                                                db.Entry(LTCFormula).State = System.Data.Entity.EntityState.Modified;
                                                db.Entry(LTCFormula).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                                db.Entry(LTCFormula).State = System.Data.Entity.EntityState.Detached;

                                            }

                                        }

                                        if (pay_id != null && pay_id != "")
                                        {
                                            var ids = Utility.StringIdsToListIds(pay_id);

                                            foreach (var P in ids)
                                            {
                                                var Pay = db.PayStruct.Find(P);
                                                L.PayStruct = Pay;
                                                LTCFormula LTCFormula = new LTCFormula()
                                                {
                                                    Name = L.Name,
                                                    HotelEligibilityPolicy = L.HotelEligibilityPolicy,
                                                    TravelEligibilityPolicy = L.TravelEligibilityPolicy,
                                                    TravelModeEligibilityPolicy = L.TravelModeEligibilityPolicy,
                                                    TravelModeRateCeilingPolicy = L.TravelModeRateCeilingPolicy,
                                                    GlobalLTCBlock = L.GlobalLTCBlock,
                                                    DAEligibilityPolicy = L.DAEligibilityPolicy,
                                                    GeoStruct = L.GeoStruct,
                                                    FuncStruct = L.FuncStruct,
                                                    PayStruct = L.PayStruct,
                                                    Id = data,
                                                    DBTrack = L.DBTrack
                                                };
                                                db.LTCFormula.Attach(LTCFormula);
                                                db.Entry(LTCFormula).State = System.Data.Entity.EntityState.Modified;
                                                db.Entry(LTCFormula).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                                db.Entry(LTCFormula).State = System.Data.Entity.EntityState.Detached;


                                            }
                                        }

                                    }
                                    // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                    using (var context = new DataBaseContext())
                                    {
                                        //var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                        //DT_PolicyFormula DT_Corp = (DT_PolicyFormula)obj;
                                        //DT_Corp.LvBankPolicy_Id = blog.LvBankPolicy == null ? 0 : blog.LvBankPolicy.Id;
                                        //DT_Corp.LvCreditPolicy_Id = blog.LvCreditPolicy == null ? 0 : blog.LvCreditPolicy.Id;
                                        //DT_Corp.LvDebitPolicy_Id = blog.LvDebitPolicy == null ? 0 : blog.LvDebitPolicy.Id;
                                        //DT_Corp.LvEncashPolicy_Id = blog.LvEncashPolicy == null ? 0 : blog.LvEncashPolicy.Id;
                                        //db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = L.Id, Val = L.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                }

                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (HotelEligibilityPolicy)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (HotelEligibilityPolicy)databaseEntry.ToObject();
                                    L.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            LTCFormula blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            LTCFormula Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.LTCFormula.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            L.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            if (TempData["RowVersion"] == null)
                            {
                                TempData["RowVersion"] = blog.RowVersion;
                            }

                            LTCFormula corp = new LTCFormula()
                            {
                                Name = L.Name,
                                HotelEligibilityPolicy = L.HotelEligibilityPolicy,
                                TravelEligibilityPolicy = L.TravelEligibilityPolicy,
                                TravelModeEligibilityPolicy = L.TravelModeEligibilityPolicy,
                                TravelModeRateCeilingPolicy = L.TravelModeRateCeilingPolicy,
                                GlobalLTCBlock = L.GlobalLTCBlock,
                                DAEligibilityPolicy = L.DAEligibilityPolicy,
                                Id = data,
                                DBTrack = L.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                //var obj = DBTrackFile.ModifiedDataHistory("Leave/Leave", "M", blog, corp, "PolicyFormula", L.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                //Old_Corp = context.PolicyFormula.Where(e => e.Id == data).Include(e => e.LvEncashPolicy)
                                //    .Include(e => e.LvDebitPolicy).Include(e => e.LvCreditPolicy).Include(e => e.LvBankPolicy).SingleOrDefault();
                                //DT_PolicyFormula DT_Corp = (DT_PolicyFormula)obj;
                                //DT_Corp.LvBankPolicy_Id = blog.LvBankPolicy == null ? 0 : blog.LvBankPolicy.Id;
                                //DT_Corp.LvCreditPolicy_Id = blog.LvCreditPolicy == null ? 0 : blog.LvCreditPolicy.Id;
                                //DT_Corp.LvDebitPolicy_Id = blog.LvDebitPolicy == null ? 0 : blog.LvDebitPolicy.Id;
                                //DT_Corp.LvEncashPolicy_Id = blog.LvEncashPolicy == null ? 0 : blog.LvEncashPolicy.Id;

                                //db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = L.DBTrack;
                            db.LTCFormula.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = L.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                        }

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

                return View();

            }

        }


        public class FormChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string GeoStruct { get; set; }
            public string PayStruct { get; set; }
            public string FuncStruct { get; set; }
        }

        public ActionResult Get_FormulaStructDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int id = Convert.ToInt32(Session["CompId"]);
                    var SalHdForm = db.LTCFormula.Where(e => e.Id == data).SingleOrDefault();

                    var db_data = db.LTCFormula
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


        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public ActionResult Formula_Grid(ParamModel param, string y)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var id = int.Parse(Session["CompId"].ToString());

                    var Sal = db.CompanyPayroll.Where(e => e.Company.Id == id).Include(e => e.LTCFormula).Select(e => e.LTCFormula).SingleOrDefault();

                    var all = Sal.GroupBy(e => e.Name).Select(e => e.FirstOrDefault()).ToList();
                    // for searchs
                    IEnumerable<LTCFormula> fall;
                    //string DependRule = "";
                    if (param.sSearch == null)
                    {
                        fall = all;

                    }
                    else
                    {
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                            || (e.Name.ToUpper().Contains(param.sSearch.ToUpper()))
                            ).ToList();

                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<LTCFormula, string> orderfunc = (c =>
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
                            var temp = db.LTCFormula
                                .Include(e => e.HotelEligibilityPolicy)
                                .Include(e => e.TravelEligibilityPolicy)
                                .Include(e => e.TravelModeEligibilityPolicy)
                                .Include(e => e.TravelModeRateCeilingPolicy)
                                .Include(e => e.TravelModeRateCeilingPolicy)
                                .Include(e => e.DAEligibilityPolicy)
                                .Where(e => e.Id == item.Id).FirstOrDefault();



                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                Name = item.Name != null ? item.Name : null,

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

                        var PolicyAssign = db2.LTCPolicyAssignment.Include(e => e.LTCFormula).ToList();
                        if (PolicyAssign.Count > 0)
                        {
                            foreach (var a in PolicyAssign)
                            {
                                foreach (var b in a.LTCFormula)
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
                    var Policy = db.LTCFormula.Where(e => e.Name == data).ToList();


                    companypayroll.LTCFormula = Policy;
                    companypayroll.LTCFormula = null;
                    db.CompanyPayroll.Attach(companypayroll);
                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;

                    //
                    var SalHead1 = db.LTCFormula.Where(e => e.Name == data).Select(e => e.Name).Distinct().FirstOrDefault();
                    var v1 = db.LTCFormula.Where(a => a.Name == SalHead1).ToList();
                    db.LTCFormula.RemoveRange(v1);



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
    }
}
