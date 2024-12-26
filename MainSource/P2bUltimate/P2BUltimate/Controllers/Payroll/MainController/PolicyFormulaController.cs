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
    [AuthoriseManger]
    public class PolicyFormulaController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/PolicyFormula/Index.cshtml");
        }


        public ActionResult GetLookupIncrPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.IncrActivity.Include(e => e.IncrList).ToList();
                IEnumerable<IncrActivity> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.IncrActivity.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }

        }

        public ActionResult GetLookupPromoPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.PromoActivity.Include(e => e.PromoList).ToList();
                IEnumerable<PromoActivity> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.PromoActivity.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }

        }

        public ActionResult GetLookupTransPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TransActivity.Include(e => e.TransList).ToList();
                IEnumerable<TransActivity> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.TransActivity.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }

        }

        public ActionResult GetLookupOthServiceBookPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.OthServiceBookActivity.Include(e => e.OtherSerBookActList).ToList();
                IEnumerable<OthServiceBookActivity> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.OthServiceBookActivity.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }

        }

        public ActionResult GetLookupExtnRednPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ExtnRednActivity.Include(e => e.ExtnRednList).ToList();
                IEnumerable<ExtnRednActivity> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ExtnRednActivity.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }

        }


        public ActionResult GetLookupOfficiatingPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.OfficiatingParameter.ToList();
                IEnumerable<OfficiatingParameter> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.OfficiatingParameter.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
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
        public ActionResult Create(PolicyFormula L, FormCollection form)
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
                    List<PolicyFormula> OFAT = new List<PolicyFormula>();
                    OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == CompId).SingleOrDefault();


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            try
                            {
                                List<IncrActivity> ObjIncrPolicy = new List<IncrActivity>();
                                if (selectedValue == "1")
                                {
                                    string IncrPolicyList = form["IncrPolicyList"];
                                    if (IncrPolicyList != null && IncrPolicyList != "")
                                    {
                                        //var value = db.IncrPolicy.Find(int.Parse(IncrPolicyList));
                                        //L.IncrPolicy = value;
                                        var ids = Utility.StringIdsToListIds(IncrPolicyList);
                                        foreach (var ca in ids)
                                        {
                                            var value = db.IncrActivity.Find(ca);
                                            ObjIncrPolicy.Add(value);
                                            L.IncrActivity = ObjIncrPolicy;
                                        }
                                    }

                                }

                                List<PromoActivity> ObjPromoPolicy = new List<PromoActivity>();
                                if (selectedValue == "2")
                                {

                                    string PromoPolicyList = form["PromoPolicyList"];
                                    if (PromoPolicyList != null && PromoPolicyList != "")
                                    {
                                        //var value = db.PromoPolicy.Find(int.Parse(PromoPolicyList));
                                        //L.PromoPolicy = value;
                                        var ids = Utility.StringIdsToListIds(PromoPolicyList);
                                        foreach (var ca in ids)
                                        {
                                            var value = db.PromoActivity.Find(ca);
                                            ObjPromoPolicy.Add(value);
                                            L.PromoActivity = ObjPromoPolicy;
                                        }
                                    }
                                }

                                List<TransActivity> ObjTransPolicy = new List<TransActivity>();
                                if (selectedValue == "3")
                                {
                                    string TransPolicyList = form["TransPolicyList"];
                                    if (TransPolicyList != null && TransPolicyList != "")
                                    {
                                        //var value = db.TransPolicy.Find(int.Parse(TransPolicyList));
                                        //L.TransPolicy = value;
                                        var ids = Utility.StringIdsToListIds(TransPolicyList);
                                        foreach (var ca in ids)
                                        {
                                            var value = db.TransActivity.Find(ca);
                                            ObjTransPolicy.Add(value);
                                            L.TransActivity = ObjTransPolicy;
                                        }
                                    }
                                }

                                List<OthServiceBookActivity> ObjOthServiceBookPolicy = new List<OthServiceBookActivity>();
                                if (selectedValue == "4")
                                {
                                    string OthServiceBookPolicyList = form["OthServiceBookPolicyList"];
                                    if (OthServiceBookPolicyList != null && OthServiceBookPolicyList != "")
                                    {
                                        //var value = db.OthServiceBookPolicy.Find(int.Parse(OthServiceBookPolicyList));
                                        //L.OthServiceBookPolicy = value;
                                        var ids = Utility.StringIdsToListIds(OthServiceBookPolicyList);
                                        foreach (var ca in ids)
                                        {
                                            var value = db.OthServiceBookActivity.Find(ca);
                                            ObjOthServiceBookPolicy.Add(value);
                                            L.OthServiceBookActivity = ObjOthServiceBookPolicy;
                                        }
                                    }
                                }

                                List<ExtnRednActivity> ObjExtnRednPolicy = new List<ExtnRednActivity>();
                                if (selectedValue == "5")
                                {
                                    string ExtnRednPolicyList = form["ExtnRednPolicyList"];
                                    if (ExtnRednPolicyList != null && ExtnRednPolicyList != "")
                                    { 
                                        var ids = Utility.StringIdsToListIds(ExtnRednPolicyList);
                                        foreach (var ca in ids)
                                        {
                                            var value = db.ExtnRednActivity.Find(ca);
                                            ObjExtnRednPolicy.Add(value);
                                            L.ExtnRednActivity = ObjExtnRednPolicy;
                                        }
                                    }
                                }

                                List<OfficiatingParameter> ObjOfficiatingPolicy = new List<OfficiatingParameter>();
                                if (selectedValue == "6")
                                {
                                    string OfficiatingPolicyList = form["OfficiatingPolicyList"];
                                    if (OfficiatingPolicyList != null && OfficiatingPolicyList != "")
                                    {
                                        var ids = Utility.StringIdsToListIds(OfficiatingPolicyList);
                                        foreach (var ca in ids)
                                        {
                                            var value = db.OfficiatingParameter.Find(ca);
                                            ObjOfficiatingPolicy.Add(value);
                                            L.OfficiatingParameter = ObjOfficiatingPolicy;
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

                                                PolicyFormula Policy = new PolicyFormula()
                                                {
                                                    Name = L.Name,
                                                    IncrActivity = L.IncrActivity,
                                                    PromoActivity = L.PromoActivity,
                                                    TransActivity = L.TransActivity,
                                                    OthServiceBookActivity = L.OthServiceBookActivity,
                                                    ExtnRednActivity = L.ExtnRednActivity,
                                                    OfficiatingParameter = L.OfficiatingParameter,
                                                    FuncStruct = L.FuncStruct,
                                                    GeoStruct = L.GeoStruct,
                                                    PayStruct = L.PayStruct,
                                                    DBTrack = L.DBTrack,
                                                };


                                                db.PolicyFormula.Add(Policy);
                                                db.SaveChanges();

                                                OFAT.Add(db.PolicyFormula.Find(Policy.Id));
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

                                            PolicyFormula Policy = new PolicyFormula()
                                            {
                                                Name = L.Name,
                                                IncrActivity = L.IncrActivity,
                                                PromoActivity = L.PromoActivity,
                                                TransActivity = L.TransActivity,
                                                OthServiceBookActivity = L.OthServiceBookActivity,
                                                ExtnRednActivity = L.ExtnRednActivity,
                                                OfficiatingParameter = L.OfficiatingParameter,
                                                FuncStruct = L.FuncStruct,
                                                GeoStruct = L.GeoStruct,
                                                PayStruct = L.PayStruct,
                                                DBTrack = L.DBTrack,
                                            };


                                            db.PolicyFormula.Add(Policy);
                                            db.SaveChanges();

                                            OFAT.Add(db.PolicyFormula.Find(Policy.Id));
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

                                            PolicyFormula Policy = new PolicyFormula()
                                            {
                                                Name = L.Name,
                                                IncrActivity = L.IncrActivity,
                                                PromoActivity = L.PromoActivity,
                                                TransActivity = L.TransActivity,
                                                OthServiceBookActivity = L.OthServiceBookActivity,
                                                ExtnRednActivity = L.ExtnRednActivity,
                                                OfficiatingParameter = L.OfficiatingParameter,
                                                FuncStruct = L.FuncStruct,
                                                GeoStruct = L.GeoStruct,
                                                PayStruct = L.PayStruct,
                                                DBTrack = L.DBTrack,
                                            };


                                            db.PolicyFormula.Add(Policy);
                                            db.SaveChanges();

                                            OFAT.Add(db.PolicyFormula.Find(Policy.Id));
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

                                        PolicyFormula Policy = new PolicyFormula()
                                        {
                                            Name = L.Name,
                                            IncrActivity = L.IncrActivity,
                                            PromoActivity = L.PromoActivity,
                                            TransActivity = L.TransActivity,
                                            OthServiceBookActivity = L.OthServiceBookActivity,
                                            ExtnRednActivity = L.ExtnRednActivity,
                                            OfficiatingParameter = L.OfficiatingParameter,
                                            FuncStruct = L.FuncStruct,
                                            GeoStruct = L.GeoStruct,
                                            PayStruct = L.PayStruct,
                                            DBTrack = L.DBTrack,
                                        };


                                        db.PolicyFormula.Add(Policy);
                                        db.SaveChanges();

                                        OFAT.Add(db.PolicyFormula.Find(Policy.Id));
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

                                        PolicyFormula Policy = new PolicyFormula()
                                        {
                                            Name = L.Name,
                                            IncrActivity = L.IncrActivity,
                                            PromoActivity = L.PromoActivity,
                                            TransActivity = L.TransActivity,
                                            OthServiceBookActivity = L.OthServiceBookActivity,
                                            ExtnRednActivity = L.ExtnRednActivity,
                                            OfficiatingParameter = L.OfficiatingParameter,
                                            FuncStruct = L.FuncStruct,
                                            GeoStruct = L.GeoStruct,
                                            PayStruct = L.PayStruct,
                                            DBTrack = L.DBTrack,
                                        };


                                        db.PolicyFormula.Add(Policy);
                                        db.SaveChanges();

                                        OFAT.Add(db.PolicyFormula.Find(Policy.Id));
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

                                        PolicyFormula Policy = new PolicyFormula()
                                        {
                                            Name = L.Name,
                                            IncrActivity = L.IncrActivity,
                                            PromoActivity = L.PromoActivity,
                                            TransActivity = L.TransActivity,
                                            OthServiceBookActivity = L.OthServiceBookActivity,
                                            ExtnRednActivity = L.ExtnRednActivity,
                                            OfficiatingParameter = L.OfficiatingParameter,
                                            FuncStruct = L.FuncStruct,
                                            GeoStruct = L.GeoStruct,
                                            PayStruct = L.PayStruct,
                                            DBTrack = L.DBTrack,
                                        };


                                        db.PolicyFormula.Add(Policy);
                                        db.SaveChanges();

                                        OFAT.Add(db.PolicyFormula.Find(Policy.Id));
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

                                        PolicyFormula Policy = new PolicyFormula()
                                        {
                                            Name = L.Name,
                                            IncrActivity = L.IncrActivity,
                                            PromoActivity = L.PromoActivity,
                                            TransActivity = L.TransActivity,
                                            OthServiceBookActivity = L.OthServiceBookActivity,
                                            ExtnRednActivity = L.ExtnRednActivity,
                                            OfficiatingParameter = L.OfficiatingParameter,
                                            FuncStruct = L.FuncStruct,
                                            GeoStruct = L.GeoStruct,
                                            PayStruct = L.PayStruct,
                                            DBTrack = L.DBTrack,
                                        };


                                        db.PolicyFormula.Add(Policy);
                                        db.SaveChanges();

                                        OFAT.Add(db.PolicyFormula.Find(Policy.Id));
                                    }
                                }



                                if (OCompanyPayroll == null)
                                {
                                    CompanyPayroll OTEP = new CompanyPayroll()
                                    {
                                        Company = db.Company.Find(SessionManager.CompanyId),
                                        PolicyFormula = OFAT,
                                        DBTrack = L.DBTrack

                                    };


                                    db.CompanyPayroll.Add(OTEP);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    var aa = db.CompanyPayroll.Find(OCompanyPayroll.Id);

                                    if (aa.PolicyFormula != null)
                                        OFAT.AddRange(aa.PolicyFormula);
                                    aa.PolicyFormula = OFAT;
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
                IEnumerable<PolicyFormula> lencash = null;
                if (gp.IsAutho == true)
                {
                    lencash = db.PolicyFormula.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    lencash = db.PolicyFormula.AsNoTracking().ToList();
                }

                IEnumerable<PolicyFormula> IE;
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
                    Func<PolicyFormula, dynamic> orderfuc;
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

        public class PolicyFormulaEditdetails
        {
            public String Incrementpolicy_Id { get; set; }
            public String Incrementpolicy_Fulldetails { get; set; }

            public String Promotionpolicy_Id { get; set; }
            public String Promotionpolicy_Fulldetails { get; set; }

            public String Transferpolicy_Id { get; set; }
            public String Transferpolicy_Fulldetails { get; set; }

            public String Otherpolicy_Id { get; set; }
            public String Otherpolicy_Fulldetails { get; set; }

            public String ExtnRednpolicy_Id { get; set; }
            public String ExtnRednpolicy_Fulldetails { get; set; }

            public String Officiatingpolicy_Id { get; set; }
            public String Officiatingpolicy_Fulldetails { get; set; }


        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int selected = 0;
                List<PolicyFormulaEditdetails> return_data = new List<PolicyFormulaEditdetails>();

                var Q = db.PolicyFormula
                   .Where(e => e.Id == data).Select
                   (e => new
                   {
                       Name = e.Name,
                   }).ToList();


                var ObjLVCP = db.PolicyFormula.Include(e => e.IncrActivity).Include(e => e.IncrActivity.Select(t => t.IncrList)).Where(e => e.Id == data).SingleOrDefault();

                if (ObjLVCP != null && ObjLVCP.IncrActivity.Count() > 0)
                {
                    foreach (var ca in ObjLVCP.IncrActivity)
                    {

                        return_data.Add(
                           new PolicyFormulaEditdetails
                           {
                               Incrementpolicy_Id = ca.Id.ToString(),
                               Incrementpolicy_Fulldetails = ca.FullDetails

                           });
                    }

                    selected = 1;

                    var SalHead = db.PolicyFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;

                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);

                }


                var ObjLVDP = db.PolicyFormula.Include(e => e.PromoActivity).Include(e => e.PromoActivity.Select(t => t.PromoList)).Where(e => e.Id == data).SingleOrDefault();
               

                if (ObjLVDP != null && ObjLVDP.PromoActivity.Count() > 0)
                {
                    foreach (var ca in ObjLVDP.PromoActivity)
                    {

                        return_data.Add(
                           new PolicyFormulaEditdetails
                           {
                               Promotionpolicy_Id = ca.Id.ToString(),
                               Promotionpolicy_Fulldetails = ca.FullDetails,

                           });

                    }
                    selected = 2;

                    var SalHead = db.PolicyFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;

                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);

                }


                var ObjLVEP = db.PolicyFormula.Include(e => e.TransActivity).Include(e => e.TransActivity.Select(t => t.TransList)).Where(e => e.Id == data).SingleOrDefault();

                if (ObjLVEP != null && ObjLVEP.TransActivity.Count() > 0)
                {
                    foreach (var ca in ObjLVEP.TransActivity)
                    {

                        return_data.Add(
                           new PolicyFormulaEditdetails
                           {
                               Transferpolicy_Id = ca.Id.ToString(),
                               Transferpolicy_Fulldetails = ca.FullDetails

                           });

                    }

                    selected = 3;

                    var SalHead = db.PolicyFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;

                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);

                }

                var ObjLVBP = db.PolicyFormula.Include(e => e.OthServiceBookActivity).Include(e => e.OthServiceBookActivity.Select(t => t.OtherSerBookActList)).Where(e => e.Id == data).SingleOrDefault();

                if (ObjLVBP != null && ObjLVBP.OthServiceBookActivity.Count() > 0)
                {
                    foreach (var ca in ObjLVBP.OthServiceBookActivity)
                    {

                        return_data.Add(
                           new PolicyFormulaEditdetails
                           {
                               Otherpolicy_Id = ca.Id.ToString(),
                               Otherpolicy_Fulldetails = ca.FullDetails

                           });

                    }
                    selected = 4;

                    var SalHead = db.PolicyFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;

                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);

                }

                var ObjLVERP = db.PolicyFormula.Include(e => e.ExtnRednActivity).Include(e => e.ExtnRednActivity.Select(t => t.ExtnRednList)).Where(e => e.Id == data).SingleOrDefault();

                if (ObjLVERP != null && ObjLVERP.ExtnRednActivity.Count() > 0)
                {
                    foreach (var ca in ObjLVERP.ExtnRednActivity)
                    {

                        return_data.Add(
                           new PolicyFormulaEditdetails
                           {
                               ExtnRednpolicy_Id = ca.Id.ToString(),
                               ExtnRednpolicy_Fulldetails = ca.FullDetails

                           });

                    }
                    selected = 5;

                    var SalHead = db.PolicyFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;

                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);

                }

                var ObjLVOP = db.PolicyFormula.Include(e => e.OfficiatingParameter).Include(e => e.OfficiatingParameter.Select(t => t.AllowanceList)).Where(e => e.Id == data).SingleOrDefault();

                if (ObjLVOP != null && ObjLVOP.OfficiatingParameter.Count() > 0)
                {
                    foreach (var ca in ObjLVERP.OfficiatingParameter)
                    {

                        return_data.Add(
                           new PolicyFormulaEditdetails
                           {
                               Officiatingpolicy_Id = ca.Id.ToString(),
                               Officiatingpolicy_Fulldetails = ca.FullDetails

                           });

                    }
                    selected = 6;

                    var SalHead = db.PolicyFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;

                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);

                }

                return Json(new object[] { Q, return_data, "", selected.ToString(), null }, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public async Task<ActionResult> EditSave(PolicyFormula L, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                   // string IncrPolicyList = form["IncrPolicyList"];
                    //string PromoPolicyList = form["PromoPolicyList"];
                    //string TransPolicyList = form["TransPolicyList"];
                    //string OthServiceBookPolicyList = form["OthServiceBookPolicyList"];
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
                                    PolicyFormula blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.PolicyFormula.Where(e => e.Id == data)
                                                                .Include(e => e.IncrActivity)
                                                                .Include(e => e.PromoActivity)
                                                                .Include(e => e.PromoActivity)
                                                                .Include(e => e.OthServiceBookActivity)
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
                                        string IncrPolicyList = form["IncrPolicyList"];
                                        List<IncrActivity> IncrActivityval = new List<IncrActivity>();

                                        PolicyFormula typedetails = db.PolicyFormula.Include(e => e.IncrActivity).Where(e => e.Id == data).FirstOrDefault();

                                        List<PolicyFormula> OFormulaList = db.PolicyFormula.Include(e => e.IncrActivity).Where(e => e.Name == typedetails.Name).ToList();
                                        foreach (var item in OFormulaList)
                                        {
                                            if (IncrPolicyList != null && IncrPolicyList != "")
                                            {
                                                var ids = Utility.StringIdsToListIds(IncrPolicyList);
                                                foreach (var ca in ids)
                                                {
                                                    var IncrActval_val = db.IncrActivity.Find(ca);
                                                    IncrActivityval.Add(IncrActval_val);
                                                    item.IncrActivity = IncrActivityval;
                                                }
                                            }
                                            else
                                            {
                                                item.IncrActivity = null;
                                            }


                                            db.PolicyFormula.Attach(item);
                                            db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                       
                                    }

                                    if (selectedValue == "2")
                                    {
                                        string PromoPolicyList = form["PromoPolicyList"];
                                        List<PromoActivity> PromoActivityval = new List<PromoActivity>();

                                        PolicyFormula typedetails = db.PolicyFormula.Include(e => e.PromoActivity).Where(e => e.Id == data).FirstOrDefault();
                                        List<PolicyFormula> OFormulaList = db.PolicyFormula.Include(e => e.PromoActivity).Where(e => e.Name == typedetails.Name).ToList();
                                          foreach (var item in OFormulaList)
                                          {
                                              if (PromoPolicyList != null && PromoPolicyList != "")
                                              {
                                                  var ids = Utility.StringIdsToListIds(PromoPolicyList);
                                                  foreach (var ca in ids)
                                                  {
                                                      var PromoActval_val = db.PromoActivity.Find(ca);
                                                      PromoActivityval.Add(PromoActval_val);
                                                      item.PromoActivity = PromoActivityval;
                                                  }
                                              }
                                              else
                                              {
                                                  item.PromoActivity = null;
                                              }


                                              db.PolicyFormula.Attach(item);
                                              db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                              db.SaveChanges();
                                          }
                                        
                                    }

                                    if (selectedValue == "3")
                                    {
                                        string TransPolicyList = form["TransPolicyList"];
                                        List<TransActivity> TransActivityval = new List<TransActivity>();

                                        PolicyFormula typedetails = db.PolicyFormula.Include(e => e.TransActivity).Where(e => e.Id == data).FirstOrDefault();
                                        List<PolicyFormula> OFormulaList = db.PolicyFormula.Include(e => e.TransActivity).Where(e => e.Name == typedetails.Name).ToList();
                                         foreach (var item in OFormulaList)
                                         {
                                             if (TransPolicyList != null && TransPolicyList != "")
                                             {
                                                 var ids = Utility.StringIdsToListIds(TransPolicyList);
                                                 foreach (var ca in ids)
                                                 {
                                                     var TransActval_val = db.TransActivity.Find(ca);
                                                     TransActivityval.Add(TransActval_val);
                                                     item.TransActivity = TransActivityval;
                                                 }
                                             }
                                             else
                                             {
                                                 item.TransActivity = null;
                                             }


                                             db.PolicyFormula.Attach(item);
                                             db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                             db.SaveChanges();
                                         }
                                     
                                    }

                                    if (selectedValue == "4")
                                    {
                                        string OthServiceBookPolicyList = form["OthServiceBookPolicyList"];
                                        List<OthServiceBookActivity> OthServActivityval = new List<OthServiceBookActivity>();

                                        PolicyFormula typedetails = db.PolicyFormula.Include(e => e.OthServiceBookActivity).Where(e => e.Id == data).SingleOrDefault();
                                        List<PolicyFormula> OFormulaList = db.PolicyFormula.Include(e => e.OthServiceBookActivity).Where(e => e.Name == typedetails.Name).ToList();
                                           foreach (var item in OFormulaList)
                                           {
                                               if (OthServiceBookPolicyList != null && OthServiceBookPolicyList != "")
                                               {
                                                   var ids = Utility.StringIdsToListIds(OthServiceBookPolicyList);
                                                   foreach (var ca in ids)
                                                   {
                                                       var OthServActval_val = db.OthServiceBookActivity.Find(ca);
                                                       OthServActivityval.Add(OthServActval_val);
                                                       item.OthServiceBookActivity = OthServActivityval;
                                                   }
                                               }
                                               else
                                               {
                                                   item.OthServiceBookActivity = null;
                                               }


                                               db.PolicyFormula.Attach(item);
                                               db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                               db.SaveChanges();
                                           }
                                        
                                    }

                                    if (selectedValue == "5")
                                    {
                                        string ExtnRednPolicyList = form["ExtnRednPolicyList"];
                                        List<ExtnRednActivity> ExtnRednActivityval = new List<ExtnRednActivity>();

                                        PolicyFormula typedetails = db.PolicyFormula.Include(e => e.ExtnRednActivity).Where(e => e.Id == data).FirstOrDefault();
                                        List<PolicyFormula> OFormulaList = db.PolicyFormula.Include(e => e.ExtnRednActivity).Where(e => e.Name == typedetails.Name).ToList();
                                         foreach (var item in OFormulaList)
                                         {
                                             if (ExtnRednPolicyList != null && ExtnRednPolicyList != "")
                                             {
                                                 var ids = Utility.StringIdsToListIds(ExtnRednPolicyList);
                                                 foreach (var ca in ids)
                                                 {
                                                     var ExtnRednActval_val = db.ExtnRednActivity.Find(ca);
                                                     ExtnRednActivityval.Add(ExtnRednActval_val);
                                                     item.ExtnRednActivity = ExtnRednActivityval;
                                                 }
                                             }
                                             else
                                             {
                                                 item.ExtnRednActivity = null;
                                             }


                                             db.PolicyFormula.Attach(item);
                                             db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                             db.SaveChanges();
                                         }
                                       
                                    }

                                    if (selectedValue == "6")
                                    {
                                        string OfficatingPolicyList = form["OfficiatingPolicyList"];
                                        List<OfficiatingParameter> OfficiatingActivityval = new List<OfficiatingParameter>();

                                        PolicyFormula typedetails = db.PolicyFormula.Include(e => e.OfficiatingParameter).Where(e => e.Id == data).FirstOrDefault();
                                          List<PolicyFormula> OFormulaList = db.PolicyFormula.Include(e => e.ExtnRednActivity).Where(e => e.Name == typedetails.Name).ToList();
                                          foreach (var item in OFormulaList)
                                          {
                                              if (OfficatingPolicyList != null && OfficatingPolicyList != "")
                                              {
                                                  var ids = Utility.StringIdsToListIds(OfficatingPolicyList);
                                                  foreach (var ca in ids)
                                                  {
                                                      var OfficiatingActval_val = db.OfficiatingParameter.Find(ca);
                                                      OfficiatingActivityval.Add(OfficiatingActval_val);
                                                      item.OfficiatingParameter = OfficiatingActivityval;
                                                  }
                                              }
                                              else
                                              {
                                                  item.OfficiatingParameter = null;
                                              }


                                              db.PolicyFormula.Attach(item);
                                              db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                              db.SaveChanges();
                                          }
                                       
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
                                var clientValues = (IncrPolicy)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (LvEncashReq)databaseEntry.ToObject();
                                    L.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
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
                    var SalHdForm = db.PolicyFormula.Where(e => e.Id == data).SingleOrDefault();

                    var db_data = db.PolicyFormula
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

                    var Sal = db.CompanyPayroll.Where(e => e.Company.Id == id).Include(e => e.PolicyFormula).Select(e => e.PolicyFormula).SingleOrDefault();

                    var all = Sal.GroupBy(e => e.Name).Select(e => e.FirstOrDefault()).ToList();
                    // for searchs
                    IEnumerable<PolicyFormula> fall;
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

                    Func<PolicyFormula, string> orderfunc = (c =>
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
                            var temp = db.PolicyFormula
                                .Include(e => e.IncrActivity)
                                .Include(e => e.PromoActivity)
                                .Include(e => e.TransActivity)
                                .Include(e => e.OthServiceBookActivity)
                                .Include(e => e.ExtnRednActivity)
                                .Include(e => e.OfficiatingParameter)
                                .Include(e => e.PolicyAssignment)
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

                        var PolicyAssign = db2.PolicyAssignment.Include(e => e.PolicyFormula).ToList();
                        if (PolicyAssign.Count > 0)
                        {
                            foreach (var a in PolicyAssign)
                            {
                                foreach (var b in a.PolicyFormula)
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
                    var Policy = db.PolicyFormula.Where(e => e.Name == data).ToList();


                    companypayroll.PolicyFormula = Policy;
                    companypayroll.PolicyFormula = null;
                    db.CompanyPayroll.Attach(companypayroll);
                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;

                    //
                    var SalHead1 = db.PolicyFormula.Where(e => e.Name == data).Select(e => e.Name).Distinct().FirstOrDefault();
                    var v1 = db.PolicyFormula.Where(a => a.Name == SalHead1).ToList();
                    db.PolicyFormula.RemoveRange(v1);



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