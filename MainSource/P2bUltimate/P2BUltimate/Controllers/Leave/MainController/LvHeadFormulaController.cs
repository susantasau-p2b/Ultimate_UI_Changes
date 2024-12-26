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
namespace P2BUltimate.Controllers.Leave.MainController
{
    [AuthoriseManger]
    public class LvHeadFormulaController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Leave/MainViews/LvHeadFormula/Index.cshtml");
        }


        public ActionResult GetLookupLvCreditPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LvCreditPolicy.ToList();
                IEnumerable<LvHead> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LvCreditPolicy.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }

        }

        public ActionResult GetLookupLvDebitpolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LvDebitPolicy.ToList();
                IEnumerable<LvDebitPolicy> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LvDebitPolicy.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }

        }

        public ActionResult GetLookupLvEncashpolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LvEncashPolicy.ToList();
                IEnumerable<LvEncashPolicy> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LvEncashPolicy.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }

        }

        public ActionResult GetLookupLvBankPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LvBankPolicy.ToList();
                IEnumerable<LvBankPolicy> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LvBankPolicy.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


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

                        var LvAssign = db2.LvAssignment.Include(e => e.LvHeadFormula).ToList();
                        if (LvAssign.Count > 0)
                        {
                            foreach (var a in LvAssign)
                            {
                                foreach (var b in a.LvHeadFormula)
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

                    var companyLeave = db.CompanyLeave.Where(e => e.Company.Id == id).SingleOrDefault();
                    var lvHead = db.LvHeadFormula.Where(e => e.Name == data).ToList();


                    companyLeave.LvHeadFormula = lvHead;
                    companyLeave.LvHeadFormula = null;
                    db.CompanyLeave.Attach(companyLeave);
                    db.Entry(companyLeave).State = System.Data.Entity.EntityState.Modified;

                    //
                    var LVHead1 = db.LvHeadFormula.Where(e => e.Name == data).Select(e => e.Name).Distinct().FirstOrDefault();
                    var v1 = db.LvHeadFormula.Where(a => a.Name == LVHead1).ToList();
                    db.LvHeadFormula.RemoveRange(v1);



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
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(LvHeadFormula L, FormCollection form)
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
                        Msg.Add("  Kindly select leave rule  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    int CompId = 0;
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = int.Parse(Session["CompId"].ToString());
                    }

                    CompanyLeave OCompanyLeave = null;
                    List<LvHeadFormula> OFAT = new List<LvHeadFormula>();
                    OCompanyLeave = db.CompanyLeave.Where(e => e.Company.Id == CompId).SingleOrDefault();


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                        new System.TimeSpan(0, 30, 0)))
                        {
                            try
                            {
                                if (selectedValue == "1")
                                {
                                    string LvCreditPolicyList = form["LvCreditPolicyList"];
                                    if (LvCreditPolicyList != null && LvCreditPolicyList != "")
                                    {
                                        var value = db.LvCreditPolicy.Find(int.Parse(LvCreditPolicyList));
                                        L.LvCreditPolicy = value;
                                    }

                                }
                                if (selectedValue == "2")
                                {

                                    string LvDebitPolicyList = form["LvDebitPolicyList"];
                                    if (LvDebitPolicyList != null && LvDebitPolicyList != "")
                                    {
                                        var value = db.LvDebitPolicy.Find(int.Parse(LvDebitPolicyList));
                                        L.LvDebitPolicy = value;
                                    }
                                }
                                if (selectedValue == "3")
                                {
                                    string LvEncashPolicyList = form["LvEncashPolicyList"];
                                    if (LvEncashPolicyList != null && LvEncashPolicyList != "")
                                    {
                                        var value = db.LvEncashPolicy.Find(int.Parse(LvEncashPolicyList));
                                        L.LvEncashPolicy = value;
                                    }
                                }
                                if (selectedValue == "4")
                                {
                                    string LvBankPolicyList = form["LvBankPolicyList"];
                                    if (LvBankPolicyList != null && LvBankPolicyList != "")
                                    {
                                        var value = db.LvBankPolicy.Find(int.Parse(LvBankPolicyList));
                                        L.LvBankPolicy = value;
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

                                                LvHeadFormula LvHeadForGeo = new LvHeadFormula()
                                                {
                                                    Name = L.Name,
                                                    LvBankPolicy = L.LvBankPolicy,
                                                    LvCreditPolicy = L.LvCreditPolicy,
                                                    LvDebitPolicy = L.LvDebitPolicy,
                                                    LvEncashPolicy = L.LvEncashPolicy,
                                                    FuncStruct = L.FuncStruct,
                                                    GeoStruct = L.GeoStruct,
                                                    PayStruct = L.PayStruct,
                                                    DBTrack = L.DBTrack,
                                                };


                                                db.LvHeadFormula.Add(LvHeadForGeo);
                                                db.SaveChanges();

                                                OFAT.Add(db.LvHeadFormula.Find(LvHeadForGeo.Id));
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

                                            LvHeadFormula LvHeadForGeo = new LvHeadFormula()
                                            {
                                                Name = L.Name,
                                                LvBankPolicy = L.LvBankPolicy,
                                                LvCreditPolicy = L.LvCreditPolicy,
                                                LvDebitPolicy = L.LvDebitPolicy,
                                                LvEncashPolicy = L.LvEncashPolicy,
                                                FuncStruct = L.FuncStruct,
                                                GeoStruct = L.GeoStruct,
                                                PayStruct = L.PayStruct,
                                                DBTrack = L.DBTrack,
                                            };


                                            db.LvHeadFormula.Add(LvHeadForGeo);
                                            db.SaveChanges();

                                            OFAT.Add(db.LvHeadFormula.Find(LvHeadForGeo.Id));
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

                                            LvHeadFormula LvHeadForGeo = new LvHeadFormula()
                                            {
                                                Name = L.Name,
                                                LvBankPolicy = L.LvBankPolicy,
                                                LvCreditPolicy = L.LvCreditPolicy,
                                                LvDebitPolicy = L.LvDebitPolicy,
                                                LvEncashPolicy = L.LvEncashPolicy,
                                                FuncStruct = L.FuncStruct,
                                                GeoStruct = L.GeoStruct,
                                                PayStruct = L.PayStruct,
                                                DBTrack = L.DBTrack,
                                            };


                                            db.LvHeadFormula.Add(LvHeadForGeo);
                                            db.SaveChanges();

                                            OFAT.Add(db.LvHeadFormula.Find(LvHeadForGeo.Id));
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

                                        LvHeadFormula LvHeadForGeo = new LvHeadFormula()
                                        {
                                            Name = L.Name,
                                            LvBankPolicy = L.LvBankPolicy,
                                            LvCreditPolicy = L.LvCreditPolicy,
                                            LvDebitPolicy = L.LvDebitPolicy,
                                            LvEncashPolicy = L.LvEncashPolicy,
                                            FuncStruct = L.FuncStruct,
                                            GeoStruct = L.GeoStruct,
                                            PayStruct = L.PayStruct,
                                            DBTrack = L.DBTrack,
                                        };


                                        db.LvHeadFormula.Add(LvHeadForGeo);
                                        db.SaveChanges();

                                        OFAT.Add(db.LvHeadFormula.Find(LvHeadForGeo.Id));
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

                                        LvHeadFormula LvHeadForGeo = new LvHeadFormula()
                                        {
                                            Name = L.Name,
                                            LvBankPolicy = L.LvBankPolicy,
                                            LvCreditPolicy = L.LvCreditPolicy,
                                            LvDebitPolicy = L.LvDebitPolicy,
                                            LvEncashPolicy = L.LvEncashPolicy,
                                            FuncStruct = L.FuncStruct,
                                            GeoStruct = L.GeoStruct,
                                            PayStruct = L.PayStruct,
                                            DBTrack = L.DBTrack,
                                        };


                                        db.LvHeadFormula.Add(LvHeadForGeo);
                                        db.SaveChanges();

                                        OFAT.Add(db.LvHeadFormula.Find(LvHeadForGeo.Id));
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

                                        LvHeadFormula LvHeadForGeo = new LvHeadFormula()
                                        {
                                            Name = L.Name,
                                            LvBankPolicy = L.LvBankPolicy,
                                            LvCreditPolicy = L.LvCreditPolicy,
                                            LvDebitPolicy = L.LvDebitPolicy,
                                            LvEncashPolicy = L.LvEncashPolicy,
                                            FuncStruct = L.FuncStruct,
                                            GeoStruct = L.GeoStruct,
                                            PayStruct = L.PayStruct,
                                            DBTrack = L.DBTrack,
                                        };


                                        db.LvHeadFormula.Add(LvHeadForGeo);
                                        db.SaveChanges();

                                        OFAT.Add(db.LvHeadFormula.Find(LvHeadForGeo.Id));
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

                                        LvHeadFormula LvHeadForGeo = new LvHeadFormula()
                                        {
                                            Name = L.Name,
                                            LvBankPolicy = L.LvBankPolicy,
                                            LvCreditPolicy = L.LvCreditPolicy,
                                            LvDebitPolicy = L.LvDebitPolicy,
                                            LvEncashPolicy = L.LvEncashPolicy,
                                            FuncStruct = L.FuncStruct,
                                            GeoStruct = L.GeoStruct,
                                            PayStruct = L.PayStruct,
                                            DBTrack = L.DBTrack,
                                        };


                                        db.LvHeadFormula.Add(LvHeadForGeo);
                                        db.SaveChanges();

                                        OFAT.Add(db.LvHeadFormula.Find(LvHeadForGeo.Id));
                                    }
                                }



                                if (OCompanyLeave == null)
                                {
                                    CompanyLeave OTEP = new CompanyLeave()
                                    {
                                        Company = db.Company.Find(SessionManager.CompanyId),
                                        LvHeadFormula = OFAT,
                                        DBTrack = L.DBTrack

                                    };


                                    db.CompanyLeave.Add(OTEP);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    var aa = db.CompanyLeave.Find(OCompanyLeave.Id);
                             
                                    if (aa.LvHeadFormula != null)
                                        OFAT.AddRange(aa.LvHeadFormula);
                                    aa.LvHeadFormula = OFAT; 
                                    db.CompanyLeave.Attach(aa);
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
                IEnumerable<LvHeadFormula> lencash = null;
                if (gp.IsAutho == true)
                {
                    lencash = db.LvHeadFormula.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    lencash = db.LvHeadFormula.AsNoTracking().ToList();
                }

                IEnumerable<LvHeadFormula> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = lencash;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.FullDetails, a.Id }).Where((e => (e.Id.ToString() == gp.searchString) || (e.FullDetails.ToString() == gp.searchString.ToLower()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.FullDetails, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = lencash;
                    Func<LvHeadFormula, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "FullDetails" ? c.FullDetails.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.FullDetails, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.FullDetails, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.FullDetails, a.Id }).ToList();
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

        public class LvHeadFormulaEditdetails
        {
            public string Lvcreditpolicy_Id { get; set; }
            public string Lvcreditpolicy_Fulldetails { get; set; }
            public string LvDebitpolicy_Id { get; set; }
            public string Lvdebitpolicy_Fulldetails { get; set; }
            public string LvEncashtpolicy_Id { get; set; }
            public string LvEncashpolicy_Fulldetails { get; set; }
            public string LvBankpolicy_Id { get; set; }
            public string LvBankpolicy_Fulldetails { get; set; }


        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int selected = 0;
                List<LvHeadFormulaEditdetails> return_data = new List<LvHeadFormulaEditdetails>();

                var Q = db.LvHeadFormula
                   .Where(e => e.Id == data).Select
                   (e => new
                   {
                       Name = e.Name,
                   }).ToList();


                var ObjLVCP = db.LvHeadFormula.Include(e => e.LvCreditPolicy).Where(e => e.Id == data).Select(e => e.LvCreditPolicy).SingleOrDefault();

                if (ObjLVCP != null)
                {


                    return_data.Add(
                       new LvHeadFormulaEditdetails
                       {
                           Lvcreditpolicy_Id = ObjLVCP.Id.ToString(),
                           Lvcreditpolicy_Fulldetails = ObjLVCP.FullDetails

                       });


                    selected = 1;

                    var SalHead = db.LvHeadFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;

                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);

                }


                var ObjLVDP = db.LvHeadFormula.Include(e => e.LvDebitPolicy).Where(e => e.Id == data).Select(e => e.LvDebitPolicy).SingleOrDefault();

                if (ObjLVDP != null)
                {


                    return_data.Add(
                       new LvHeadFormulaEditdetails
                       {
                           LvDebitpolicy_Id = ObjLVDP.Id.ToString(),
                           Lvdebitpolicy_Fulldetails = ObjLVDP.FullDetails

                       });


                    selected = 2;

                    var SalHead = db.LvHeadFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;

                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);

                }


                var ObjLVEP = db.LvHeadFormula.Include(e => e.LvEncashPolicy).Where(e => e.Id == data).Select(e => e.LvEncashPolicy).SingleOrDefault();

                if (ObjLVEP != null)
                {


                    return_data.Add(
                       new LvHeadFormulaEditdetails
                       {
                           LvEncashtpolicy_Id = ObjLVEP.Id.ToString(),
                           LvEncashpolicy_Fulldetails = ObjLVEP.FullDetails

                       });


                    selected = 3;

                    var SalHead = db.LvHeadFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;

                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);

                }

                var ObjLVBP = db.LvHeadFormula.Include(e => e.LvBankPolicy).Where(e => e.Id == data).Select(e => e.LvBankPolicy).SingleOrDefault();

                if (ObjLVBP != null)
                {


                    return_data.Add(
                       new LvHeadFormulaEditdetails
                       {
                           LvBankpolicy_Id = ObjLVBP.Id.ToString(),
                           LvBankpolicy_Fulldetails = ObjLVBP.FullDetails == null ? "" : ObjLVBP.FullDetails

                       });


                    selected = 4;

                    var SalHead = db.LvHeadFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;

                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);

                }

                return Json(new object[] { Q, return_data, "", selected.ToString(), null }, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public async Task<ActionResult> EditSave(LvHeadFormula L, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string LvCreditPolicyList = form["LvCreditPolicyList"];
                    string LvDebitPolicyList = form["LvDebitPolicyList"];
                    string LvEncashPolicyList = form["LvEncashPolicyList"];
                    string LvBankPolicyList = form["LvBankPolicyList"];
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
                                    LvHeadFormula blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.LvHeadFormula.Where(e => e.Id == data).Include(e => e.LvBankPolicy)
                                                                .Include(e => e.LvCreditPolicy)
                                                                .Include(e => e.LvDebitPolicy)
                                                                .Include(e => e.LvEncashPolicy)
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
                                        if (LvCreditPolicyList != null && LvCreditPolicyList != "")
                                        {

                                            var val = db.LvCreditPolicy.Find(int.Parse(LvCreditPolicyList));
                                            L.LvCreditPolicy = val;

                                            db.LvHeadFormula.Where(e => e.Name == L.Name).ToList().ForEach(x =>
                                            {
                                                LvHeadFormula LVHeadFor = db.LvHeadFormula.Include(e => e.FuncStruct)
                                                                            .Include(e => e.PayStruct).Include(e => e.GeoStruct).Where(e => e.Id == x.Id).SingleOrDefault();

                                                LVHeadFor.LvCreditPolicy = L.LvCreditPolicy;
                                                LVHeadFor.DBTrack = L.DBTrack;
                                            });
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            db.LvHeadFormula.Where(e => e.Name == L.Name).ToList().ForEach(x =>
                                            {
                                                LvHeadFormula LVHeadFor = db.LvHeadFormula.Include(e => e.FuncStruct)
                                                                            .Include(e => e.PayStruct).Include(e => e.GeoStruct).Where(e => e.Id == x.Id).SingleOrDefault();

                                                LVHeadFor.LvCreditPolicy = null;
                                                LVHeadFor.DBTrack = L.DBTrack;
                                            });
                                            db.SaveChanges();
                                        }
                                    }


                                    if (selectedValue == "2")
                                    {
                                        if (LvDebitPolicyList != null && LvDebitPolicyList != "")
                                        {
                                            
                                                var val = db.LvDebitPolicy.Find(int.Parse(LvDebitPolicyList));
                                                L.LvDebitPolicy = val;

                                                db.LvHeadFormula.Where(e => e.Name == L.Name).ToList().ForEach(x =>
                                                {
                                                    LvHeadFormula LVHeadFor = db.LvHeadFormula.Include(e => e.FuncStruct)
                                                                                .Include(e => e.PayStruct).Include(e => e.GeoStruct).Where(e => e.Id == x.Id).SingleOrDefault();

                                                    LVHeadFor.LvDebitPolicy = L.LvDebitPolicy; 
                                                    LVHeadFor.DBTrack = L.DBTrack;
                                                });
                                                db.SaveChanges(); 
                                        }
                                        else
                                        {
                                            db.LvHeadFormula.Where(e => e.Name == L.Name).ToList().ForEach(x =>
                                            {
                                                LvHeadFormula LVHeadFor = db.LvHeadFormula.Include(e => e.FuncStruct)
                                                                            .Include(e => e.PayStruct).Include(e => e.GeoStruct).Where(e => e.Id == x.Id).SingleOrDefault();

                                                LVHeadFor.LvDebitPolicy = null;
                                                LVHeadFor.DBTrack = L.DBTrack;
                                            });
                                            db.SaveChanges(); 
                                        }
                                    }

                                    if (selectedValue == "3")
                                    {
                                        if (LvEncashPolicyList != null && LvEncashPolicyList != "")
                                        { 
                                            var val = db.LvEncashPolicy.Find(int.Parse(LvEncashPolicyList));
                                            L.LvEncashPolicy = val;

                                            db.LvHeadFormula.Where(e => e.Name == L.Name).ToList().ForEach(x =>
                                            {
                                                LvHeadFormula LVHeadFor = db.LvHeadFormula.Include(e => e.FuncStruct)
                                                                            .Include(e => e.PayStruct).Include(e => e.GeoStruct).Where(e => e.Id == x.Id).SingleOrDefault();

                                                LVHeadFor.LvEncashPolicy = L.LvEncashPolicy;
                                                LVHeadFor.DBTrack = L.DBTrack;
                                            });
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            db.LvHeadFormula.Where(e => e.Name == L.Name).ToList().ForEach(x =>
                                            {
                                                LvHeadFormula LVHeadFor = db.LvHeadFormula.Include(e => e.FuncStruct)
                                                                            .Include(e => e.PayStruct).Include(e => e.GeoStruct).Where(e => e.Id == x.Id).SingleOrDefault();

                                                LVHeadFor.LvEncashPolicy = null;
                                                LVHeadFor.DBTrack = L.DBTrack;
                                            });
                                            db.SaveChanges();
                                        }
                                    }
                                    if (selectedValue == "4")
                                    {
                                        if (LvBankPolicyList != null && LvBankPolicyList != "")
                                        {
                                            var val = db.LvBankPolicy.Find(int.Parse(LvBankPolicyList));
                                            L.LvBankPolicy = val;

                                            db.LvHeadFormula.Where(e => e.Name == L.Name).ToList().ForEach(x =>
                                            {
                                                LvHeadFormula LVHeadFor = db.LvHeadFormula.Include(e => e.FuncStruct)
                                                                            .Include(e => e.PayStruct).Include(e => e.GeoStruct).Where(e => e.Id == x.Id).SingleOrDefault();

                                                LVHeadFor.LvBankPolicy = L.LvBankPolicy;
                                                LVHeadFor.DBTrack = L.DBTrack;
                                            });
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            db.LvHeadFormula.Where(e => e.Name == L.Name).ToList().ForEach(x =>
                                            {
                                                LvHeadFormula LVHeadFor = db.LvHeadFormula.Include(e => e.FuncStruct)
                                                                            .Include(e => e.PayStruct).Include(e => e.GeoStruct).Where(e => e.Id == x.Id).SingleOrDefault();

                                                LVHeadFor.LvBankPolicy = null;
                                                LVHeadFor.DBTrack = L.DBTrack;
                                            });
                                            db.SaveChanges();
                                        }
                                    }
                                    var CurCorp = db.LvHeadFormula.Find(data);
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
                                                LvHeadFormula LvHeadFormula = new LvHeadFormula()
                                                {
                                                    Name = L.Name,
                                                    LvBankPolicy = L.LvBankPolicy,
                                                    LvCreditPolicy = L.LvCreditPolicy,
                                                    LvDebitPolicy = L.LvDebitPolicy,
                                                    LvEncashPolicy = L.LvEncashPolicy,
                                                    GeoStruct = L.GeoStruct,
                                                    FuncStruct = L.FuncStruct,
                                                    PayStruct = L.PayStruct,
                                                    Id = data,
                                                    DBTrack = L.DBTrack
                                                };
                                                db.LvHeadFormula.Attach(LvHeadFormula);
                                                db.Entry(LvHeadFormula).State = System.Data.Entity.EntityState.Modified;
                                                db.Entry(LvHeadFormula).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                                db.Entry(LvHeadFormula).State = System.Data.Entity.EntityState.Detached;

                                            }
                                        }

                                        if (fun_id != null && fun_id != "")
                                        {
                                            var ids = Utility.StringIdsToListIds(fun_id);

                                            foreach (var F in ids)
                                            {
                                                var fun = db.FuncStruct.Find(F);
                                                L.FuncStruct = fun;
                                                LvHeadFormula LvHeadFormula = new LvHeadFormula()
                                                {

                                                    LvBankPolicy = L.LvBankPolicy,
                                                    LvCreditPolicy = L.LvCreditPolicy,
                                                    LvDebitPolicy = L.LvDebitPolicy,
                                                    LvEncashPolicy = L.LvEncashPolicy,
                                                    GeoStruct = L.GeoStruct,
                                                    FuncStruct = L.FuncStruct,
                                                    PayStruct = L.PayStruct,
                                                    Id = data,
                                                    DBTrack = L.DBTrack
                                                };
                                                db.LvHeadFormula.Add(LvHeadFormula);
                                                db.Entry(LvHeadFormula).State = System.Data.Entity.EntityState.Modified;
                                                db.Entry(LvHeadFormula).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                                db.Entry(LvHeadFormula).State = System.Data.Entity.EntityState.Detached;

                                            }

                                        }

                                        if (pay_id != null && pay_id != "")
                                        {
                                            var ids = Utility.StringIdsToListIds(pay_id);

                                            foreach (var P in ids)
                                            {
                                                var Pay = db.PayStruct.Find(P);
                                                L.PayStruct = Pay;
                                                LvHeadFormula LvHeadFormula = new LvHeadFormula()
                                                {
                                                    Name = L.Name,
                                                    LvBankPolicy = L.LvBankPolicy,
                                                    LvCreditPolicy = L.LvCreditPolicy,
                                                    LvDebitPolicy = L.LvDebitPolicy,
                                                    LvEncashPolicy = L.LvEncashPolicy,
                                                    GeoStruct = L.GeoStruct,
                                                    FuncStruct = L.FuncStruct,
                                                    PayStruct = L.PayStruct,
                                                    Id = data,
                                                    DBTrack = L.DBTrack
                                                };
                                                db.LvHeadFormula.Attach(LvHeadFormula);
                                                db.Entry(LvHeadFormula).State = System.Data.Entity.EntityState.Modified;
                                                db.Entry(LvHeadFormula).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                                db.Entry(LvHeadFormula).State = System.Data.Entity.EntityState.Detached;


                                            }
                                        }

                                    }
                                    // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Leave/Leave", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                        //DT_LvHeadFormula DT_Corp = (DT_LvHeadFormula)obj;
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
                                    return Json(new Utility.JsonReturnClass { Id = L.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                }

                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (LvCreditPolicy)entry.Entity;
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
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            LvHeadFormula blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            LvHeadFormula Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.LvHeadFormula.Where(e => e.Id == data).SingleOrDefault();
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

                            LvHeadFormula corp = new LvHeadFormula()
                            {
                                Name = L.Name,
                                LvBankPolicy = L.LvBankPolicy,
                                LvCreditPolicy = L.LvCreditPolicy,
                                LvDebitPolicy = L.LvDebitPolicy,
                                LvEncashPolicy = L.LvEncashPolicy,
                                Id = data,
                                DBTrack = L.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Leave/Leave", "M", blog, corp, "LvHeadFormula", L.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.LvHeadFormula.Where(e => e.Id == data).Include(e => e.LvEncashPolicy)
                                    .Include(e => e.LvDebitPolicy).Include(e => e.LvCreditPolicy).Include(e => e.LvBankPolicy).SingleOrDefault();
                                DT_LvHeadFormula DT_Corp = (DT_LvHeadFormula)obj;
                                DT_Corp.LvBankPolicy_Id = blog.LvBankPolicy == null ? 0 : blog.LvBankPolicy.Id;
                                DT_Corp.LvCreditPolicy_Id = blog.LvCreditPolicy == null ? 0 : blog.LvCreditPolicy.Id;
                                DT_Corp.LvDebitPolicy_Id = blog.LvDebitPolicy == null ? 0 : blog.LvDebitPolicy.Id;
                                DT_Corp.LvEncashPolicy_Id = blog.LvEncashPolicy == null ? 0 : blog.LvEncashPolicy.Id;

                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = L.DBTrack;
                            db.LvHeadFormula.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                    var SalHdForm = db.LvHeadFormula.Where(e => e.Id == data).SingleOrDefault();

                    var db_data = db.LvHeadFormula
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
            public double CeilingMin { get; set; }
            public double CeilingMax { get; set; }
            public string DependRule { get; set; }
        }

        public ActionResult Formula_Grid(ParamModel param, string y)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var id = int.Parse(Session["CompId"].ToString());
                    //var a = db.PayScaleAssignment.Include(e => e.SalHeadFormula).Include(e => e.PayScaleAgreemnt).Where(e => e.Id == data).ToList();
                    var Sal = db.CompanyLeave.Where(e => e.Company.Id == id).Include(e => e.LvHeadFormula).Select(e => e.LvHeadFormula).SingleOrDefault();

                    var all = Sal.GroupBy(e => e.Name).Select(e => e.FirstOrDefault()).ToList();
                    // for searchs
                    IEnumerable<LvHeadFormula> fall;
                    //string DependRule = "";
                    if (param.sSearch == null)
                    {
                        fall = all;

                    }
                    else
                    {
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                            || (e.Name.ToUpper().Contains(param.sSearch.ToUpper()))
                            //|| (e.CeilingMax.ToString().Contains(param.sSearch))
                            //|| (e.CeilingMin.ToString().Contains(param.sSearch))
                            ).ToList();

                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<LvHeadFormula, string> orderfunc = (c =>
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
                            var temp = db.LvHeadFormula.Include(e => e.LvCreditPolicy).Include(e => e.LvDebitPolicy)
                                .Include(e => e.LvEncashPolicy).Include(e => e.LvBankPolicy).Include(e => e.LvAssignment)
                                .Where(e => e.Id == item.Id).FirstOrDefault();

                            //if (temp.AmountDependRule != null)
                            //{
                            //    DependRule = "AmountDependRule";
                            //}
                            //else if (temp.PercentDependRule != null)
                            //{
                            //    DependRule = "PercentDependRule";
                            //}
                            //else if (temp.BASICDependRule != null)
                            //{
                            //    DependRule = "BASICDependRule";
                            //}
                            //else if (temp.ServiceDependRule != null)
                            //{
                            //    DependRule = "ServiceDependRule";
                            //}
                            //else if (temp.SlabDependRule != null)
                            //{
                            //    DependRule = "SlabDependRule";
                            //}
                            //else if (temp.VDADependRule != null)
                            //{
                            //    DependRule = "VDADependRule";
                            //}

                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                Name = item.Name != null ? item.Name : null,
                                //CeilingMin = item.CeilingMin,
                                //CeilingMax = item.CeilingMax,
                                //DependRule = DependRule
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
    }
}