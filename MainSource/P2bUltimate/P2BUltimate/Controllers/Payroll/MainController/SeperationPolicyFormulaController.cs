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
using EMS;
using Training;
using Attendance;
using Leave;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers
{
    public class SeperationPolicyFormulaController : Controller
    {
        //
        // GET: /SeperationPolicyFormula/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/SeperationPolicyFormula/Index.cshtml");
        }

        public ActionResult Partial_NoticePeriodObject()
        {
            return View("~/Views/Shared/Payroll/_NoticePeriodObject.cshtml");
        }


        public ActionResult GetExitProcessProcessPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.ExitProcess_Process_Policy.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ExitProcess_Process_Policy.Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }


                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = "Process Name:" + ca.ProcessName }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }


        public ActionResult GetExitProcessCheckListPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.ExitProcess_CheckList_Policy.Include(e => e.ExitProcess_CheckList_Object).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ExitProcess_CheckList_Policy.Include(e => e.ExitProcess_CheckList_Object).Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }


                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = "Checklist Name:" + ca.ChecklistName }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }


        public ActionResult GetExitProcessConfigPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.ExitProcess_Config_Policy.Include(e => e.LeaveHead).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ExitProcess_Config_Policy.Include(e => e.LeaveHead).Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }


                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = "Process Config Name:" + ca.ProcessConfigName }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }


        public ActionResult Create(SeperationPolicyFormula L, FormCollection form)
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

                    CompanyExit OCompanyExit = null;
                    List<SeperationPolicyFormula> OFAT = new List<SeperationPolicyFormula>();
                    OCompanyExit = db.CompanyExit.Where(e => e.Company.Id == CompId).SingleOrDefault();


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            try
                            {
                                List<ExitProcess_Process_Policy> ObjExitProcess_ProcessPolicy = new List<ExitProcess_Process_Policy>();
                                if (selectedValue == "1")
                                {
                                    string ExitProcess_ProcessPolicyList = form["ExitProcess_ProcessPolicyList"];
                                    if (ExitProcess_ProcessPolicyList != null && ExitProcess_ProcessPolicyList != "")
                                    {
                                        //var value = db.IncrPolicy.Find(int.Parse(IncrPolicyList));
                                        //L.IncrPolicy = value;
                                        //var ids = Convert.ToInt32(ExitProcess_ProcessPolicyList);

                                        //ObjExitProcess_ProcessPolicy = db.ExitProcess_Process_Policy.Find(ids);
                                        //L.ExitProcess_Process_Policy = ObjExitProcess_ProcessPolicy;
                                        var ids = Utility.StringIdsToListIds(ExitProcess_ProcessPolicyList);
                                        foreach (var ca in ids)
                                        {
                                            var value = db.ExitProcess_Process_Policy.Find(ca);
                                            ObjExitProcess_ProcessPolicy.Add(value);
                                            L.ExitProcess_Process_Policy = ObjExitProcess_ProcessPolicy;
                                        }

                                    }
                                    else
                                    {
                                        Msg.Add("  Please Enter ExitProcess_ProcessPolicyList ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }

                                }

                                List<ExitProcess_CheckList_Policy> ObjExitProcessCheckListPolicy = new List<ExitProcess_CheckList_Policy>();
                                if (selectedValue == "2")
                                {

                                    string ExitProcessCheckListPolicyList = form["ExitProcessCheckListPolicyList"];
                                    if (ExitProcessCheckListPolicyList != null && ExitProcessCheckListPolicyList != "")
                                    {
                                        //var value = db.PromoPolicy.Find(int.Parse(PromoPolicyList));
                                        //L.PromoPolicy = value;
                                        //var ids = Convert.ToInt32(ExitProcessCheckListPolicyList);

                                        //ObjExitProcessCheckListPolicy = db.ExitProcess_CheckList_Policy.Find(ids);
                                        //L.ExitProcess_CheckList_Policy = ObjExitProcessCheckListPolicy;

                                        var ids = Utility.StringIdsToListIds(ExitProcessCheckListPolicyList);
                                        foreach (var ca in ids)
                                        {
                                            var value = db.ExitProcess_CheckList_Policy.Find(ca);
                                            ObjExitProcessCheckListPolicy.Add(value);
                                            L.ExitProcess_CheckList_Policy = ObjExitProcessCheckListPolicy;
                                        }


                                    }
                                    else
                                    {
                                        Msg.Add("  Please Enter ExitProcessCheckListPolicyList");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }

                                List<ExitProcess_Config_Policy> ObjExitProcessConfigPolicy = new List<ExitProcess_Config_Policy>();
                                if (selectedValue == "3")
                                {
                                    string ExitProcessConfigPolicyList = form["ExitProcessConfigPolicyList"];
                                    if (ExitProcessConfigPolicyList != null && ExitProcessConfigPolicyList != "")
                                    {
                                        //var value = db.TransPolicy.Find(int.Parse(TransPolicyList));
                                        //L.TransPolicy = value;
                                        //var ids = Convert.ToInt32(ExitProcessConfigPolicyList);

                                        //ObjExitProcessConfigPolicy = db.ExitProcess_Config_Policy.Find(ids);
                                        //L.ExitProcess_Config_Policy = ObjExitProcessConfigPolicy;

                                        var ids = Utility.StringIdsToListIds(ExitProcessConfigPolicyList);
                                        foreach (var ca in ids)
                                        {
                                            var value = db.ExitProcess_Config_Policy.Find(ca);
                                            ObjExitProcessConfigPolicy.Add(value);
                                            L.ExitProcess_Config_Policy = ObjExitProcessConfigPolicy;
                                        }

                                    }
                                    else
                                    {
                                        Msg.Add("  Please Enter ExitProcessConfigPolicyList");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }

                                }

                                List<NoticePeriod_Object> NoticePeriodObjectPolicy = new List<NoticePeriod_Object>();
                                if (selectedValue == "4")
                                {
                                    string NoticePeriodObjectList = form["NoticePeriodObjectList"];
                                    if (NoticePeriodObjectList != null && NoticePeriodObjectList != "")
                                    {
                                        //var value = db.TransPolicy.Find(int.Parse(TransPolicyList));
                                        //L.TransPolicy = value;
                                        //var ids = Convert.ToInt32(NoticePeriodObjectList);

                                        //NoticePeriodObjectPolicy = db.NoticePeriod_Object.Find(ids);
                                        //L.NoticePeriod_Object = NoticePeriodObjectPolicy;
                                        var ids = Utility.StringIdsToListIds(NoticePeriodObjectList);
                                        foreach (var ca in ids)
                                        {
                                            var value = db.NoticePeriod_Object.Find(ca);
                                            NoticePeriodObjectPolicy.Add(value);
                                            L.NoticePeriod_Object = NoticePeriodObjectPolicy;
                                        }

                                    }
                                    else
                                    {
                                        Msg.Add("  Please Enter NoticePeriodObjectList");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

                                                SeperationPolicyFormula Policy = new SeperationPolicyFormula()
                                                {
                                                    Name = L.Name,
                                                    ExitProcess_Process_Policy = L.ExitProcess_Process_Policy,
                                                    ExitProcess_CheckList_Policy = L.ExitProcess_CheckList_Policy,
                                                    ExitProcess_Config_Policy = L.ExitProcess_Config_Policy,
                                                    NoticePeriod_Object = L.NoticePeriod_Object,
                                                    FuncStruct = L.FuncStruct,
                                                    GeoStruct = L.GeoStruct,
                                                    PayStruct = L.PayStruct,
                                                    DBTrack = L.DBTrack,
                                                };


                                                db.SeperationPolicyFormula.Add(Policy);
                                                db.SaveChanges();

                                                OFAT.Add(db.SeperationPolicyFormula.Find(Policy.Id));
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

                                            SeperationPolicyFormula Policy = new SeperationPolicyFormula()
                                            {
                                                Name = L.Name,
                                                ExitProcess_Process_Policy = L.ExitProcess_Process_Policy,
                                                ExitProcess_CheckList_Policy = L.ExitProcess_CheckList_Policy,
                                                ExitProcess_Config_Policy = L.ExitProcess_Config_Policy,
                                                NoticePeriod_Object = L.NoticePeriod_Object,
                                                FuncStruct = L.FuncStruct,
                                                GeoStruct = L.GeoStruct,
                                                PayStruct = L.PayStruct,
                                                DBTrack = L.DBTrack,
                                            };


                                            db.SeperationPolicyFormula.Add(Policy);
                                            db.SaveChanges();

                                            OFAT.Add(db.SeperationPolicyFormula.Find(Policy.Id));
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

                                            SeperationPolicyFormula Policy = new SeperationPolicyFormula()
                                            {
                                                Name = L.Name,
                                                ExitProcess_Process_Policy = L.ExitProcess_Process_Policy,
                                                ExitProcess_CheckList_Policy = L.ExitProcess_CheckList_Policy,
                                                ExitProcess_Config_Policy = L.ExitProcess_Config_Policy,
                                                NoticePeriod_Object = L.NoticePeriod_Object,
                                                FuncStruct = L.FuncStruct,
                                                GeoStruct = L.GeoStruct,
                                                PayStruct = L.PayStruct,
                                                DBTrack = L.DBTrack,
                                            };


                                            db.SeperationPolicyFormula.Add(Policy);
                                            db.SaveChanges();

                                            OFAT.Add(db.SeperationPolicyFormula.Find(Policy.Id));
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

                                        SeperationPolicyFormula Policy = new SeperationPolicyFormula()
                                        {
                                            Name = L.Name,
                                            ExitProcess_Process_Policy = L.ExitProcess_Process_Policy,
                                            ExitProcess_CheckList_Policy = L.ExitProcess_CheckList_Policy,
                                            ExitProcess_Config_Policy = L.ExitProcess_Config_Policy,
                                            NoticePeriod_Object = L.NoticePeriod_Object,
                                            FuncStruct = L.FuncStruct,
                                            GeoStruct = L.GeoStruct,
                                            PayStruct = L.PayStruct,
                                            DBTrack = L.DBTrack,
                                        };


                                        db.SeperationPolicyFormula.Add(Policy);
                                        db.SaveChanges();

                                        OFAT.Add(db.SeperationPolicyFormula.Find(Policy.Id));
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

                                        SeperationPolicyFormula Policy = new SeperationPolicyFormula()
                                        {
                                            Name = L.Name,
                                            ExitProcess_Process_Policy = L.ExitProcess_Process_Policy,
                                            ExitProcess_CheckList_Policy = L.ExitProcess_CheckList_Policy,
                                            ExitProcess_Config_Policy = L.ExitProcess_Config_Policy,
                                            NoticePeriod_Object = L.NoticePeriod_Object,
                                            FuncStruct = L.FuncStruct,
                                            GeoStruct = L.GeoStruct,
                                            PayStruct = L.PayStruct,
                                            DBTrack = L.DBTrack,
                                        };


                                        db.SeperationPolicyFormula.Add(Policy);
                                        db.SaveChanges();

                                        OFAT.Add(db.SeperationPolicyFormula.Find(Policy.Id));
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

                                        SeperationPolicyFormula Policy = new SeperationPolicyFormula()
                                        {
                                            Name = L.Name,
                                            ExitProcess_Process_Policy = L.ExitProcess_Process_Policy,
                                            ExitProcess_CheckList_Policy = L.ExitProcess_CheckList_Policy,
                                            ExitProcess_Config_Policy = L.ExitProcess_Config_Policy,
                                            NoticePeriod_Object = L.NoticePeriod_Object,
                                            FuncStruct = L.FuncStruct,
                                            GeoStruct = L.GeoStruct,
                                            PayStruct = L.PayStruct,
                                            DBTrack = L.DBTrack,
                                        };


                                        db.SeperationPolicyFormula.Add(Policy);
                                        db.SaveChanges();

                                        OFAT.Add(db.SeperationPolicyFormula.Find(Policy.Id));
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

                                        SeperationPolicyFormula Policy = new SeperationPolicyFormula()
                                        {
                                            Name = L.Name,
                                            ExitProcess_Process_Policy = L.ExitProcess_Process_Policy,
                                            ExitProcess_CheckList_Policy = L.ExitProcess_CheckList_Policy,
                                            ExitProcess_Config_Policy = L.ExitProcess_Config_Policy,
                                            NoticePeriod_Object = L.NoticePeriod_Object,
                                            FuncStruct = L.FuncStruct,
                                            GeoStruct = L.GeoStruct,
                                            PayStruct = L.PayStruct,
                                            DBTrack = L.DBTrack,
                                        };


                                        db.SeperationPolicyFormula.Add(Policy);
                                        db.SaveChanges();

                                        OFAT.Add(db.SeperationPolicyFormula.Find(Policy.Id));
                                    }
                                }



                                if (OCompanyExit == null)
                                {
                                    CompanyExit OTEP = new CompanyExit()
                                    {
                                        Company = db.Company.Find(CompId),
                                        SeperationPolicyFormula = OFAT,
                                        DBTrack = L.DBTrack

                                    };


                                    db.CompanyExit.Add(OTEP);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    var aa = db.CompanyExit.Find(OCompanyExit.Id);

                                    if (aa.SeperationPolicyFormula != null)
                                        OFAT.AddRange(aa.SeperationPolicyFormula);
                                    aa.SeperationPolicyFormula = OFAT;
                                    db.CompanyExit.Attach(aa);
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
                IEnumerable<SeperationPolicyFormula> lencash = null;
                if (gp.IsAutho == true)
                {
                    lencash = db.SeperationPolicyFormula.Include(e => e.NoticePeriod_Object).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    lencash = db.SeperationPolicyFormula.Include(e => e.NoticePeriod_Object).AsNoTracking().ToList();
                }

                IEnumerable<SeperationPolicyFormula> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = lencash;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.Name }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Name.ToString() == gp.searchString.ToLower()))).ToList();
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
                    Func<SeperationPolicyFormula, dynamic> orderfuc;
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



        public class SeperationPolicyFormulaEditdetails
        {
            public string ExitProcess_ProcessPolicy_Id { get; set; }
            public string ExitProcess_ProcessPolicy_Fulldetails { get; set; }

            public string ExitProcessCheckListPolicy_Id { get; set; }
            public string ExitProcessCheckListPolicy_Fulldetails { get; set; }

            public string ExitProcessConfigPolicy_Id { get; set; }
            public string ExitProcessConfigPolicy_Fulldetails { get; set; }

            public string NoticePeriodObjectPolicy_Id { get; set; }
            public string NoticePeriodObjectPolicy_Fulldetails { get; set; }



        }



        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int selected = 0;
                List<SeperationPolicyFormulaEditdetails> return_data = new List<SeperationPolicyFormulaEditdetails>();

                var Q = db.SeperationPolicyFormula
                   .Where(e => e.Id == data).Select
                   (e => new
                   {
                       Name = e.Name,
                   }).ToList();


                var ObjLVCP = db.SeperationPolicyFormula.Include(e => e.ExitProcess_Process_Policy).Where(e => e.Id == data).Select(e => e.ExitProcess_Process_Policy).SingleOrDefault();

                if (ObjLVCP != null && ObjLVCP.Count() > 0)
                {
                    foreach (var ca in ObjLVCP)
                    {

                        return_data.Add(
                           new SeperationPolicyFormulaEditdetails
                           {
                               ExitProcess_ProcessPolicy_Id = ca.Id.ToString(),
                               ExitProcess_ProcessPolicy_Fulldetails = ca.ProcessName

                           });
                    }

                    selected = 1;

                    var SalHead = db.SeperationPolicyFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;

                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);

                }


                var ObjLVDP = db.SeperationPolicyFormula.Include(e => e.ExitProcess_CheckList_Policy).Where(e => e.Id == data).Select(e => e.ExitProcess_CheckList_Policy).SingleOrDefault();

                if (ObjLVDP != null && ObjLVDP.Count() > 0)
                {
                    foreach (var ca in ObjLVDP)
                    {

                        return_data.Add(
                           new SeperationPolicyFormulaEditdetails
                           {
                               ExitProcessCheckListPolicy_Id = ca.Id.ToString(),
                               ExitProcessCheckListPolicy_Fulldetails = ca.ChecklistName

                           });
                    }

                    selected = 2;

                    var SalHead = db.SeperationPolicyFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;

                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);

                }


                var ObjLVEP = db.SeperationPolicyFormula.Include(e => e.ExitProcess_Config_Policy).Where(e => e.Id == data).Select(e => e.ExitProcess_Config_Policy).SingleOrDefault();

                if (ObjLVEP != null && ObjLVEP.Count() > 0)
                {
                    foreach (var ca in ObjLVEP)
                    {

                        return_data.Add(
                           new SeperationPolicyFormulaEditdetails
                           {
                               ExitProcessConfigPolicy_Id = ca.Id.ToString(),
                               ExitProcessConfigPolicy_Fulldetails = ca.ProcessConfigName

                           });
                    }


                    selected = 3;

                    var SalHead = db.SeperationPolicyFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;

                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);

                }
                var ObjLVNP = db.SeperationPolicyFormula.Include(e => e.NoticePeriod_Object).Where(e => e.Id == data).Select(e => e.NoticePeriod_Object).SingleOrDefault();

                if (ObjLVNP != null && ObjLVNP.Count() > 0)
                {
                    foreach (var ca in ObjLVNP)
                    {

                        return_data.Add(
                           new SeperationPolicyFormulaEditdetails
                           {
                               NoticePeriodObjectPolicy_Id = ca.Id.ToString(),
                               NoticePeriodObjectPolicy_Fulldetails = ca.PolicyName

                           });
                    }


                    selected = 4;

                    var SalHead = db.SeperationPolicyFormula.Find(data);
                    TempData["RowVersion"] = SalHead.RowVersion;
                    var Auth = SalHead.DBTrack.IsModified;

                    return Json(new object[] { Q, return_data, "", selected.ToString(), Auth }, JsonRequestBehavior.AllowGet);

                }
                return Json(new object[] { Q, return_data, "", selected.ToString(), null }, JsonRequestBehavior.AllowGet);
            }
        }


        public class FormChildDataClass
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
                    var SalHdForm = db.SeperationPolicyFormula.Where(e => e.Id == data).SingleOrDefault();

                    var db_data = db.SeperationPolicyFormula
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


        public class returndatagridclass
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

                    var Sal = db.CompanyExit.Where(e => e.Company.Id == id).Include(e => e.SeperationPolicyFormula).Select(e => e.SeperationPolicyFormula).SingleOrDefault();
                    List<SeperationPolicyFormula> all = new List<SeperationPolicyFormula>();
                    if (Sal != null)
                    {
                        all = Sal.GroupBy(e => e.Name).Select(e => e.FirstOrDefault()).ToList();
                    }

                    // for searchs
                    IEnumerable<SeperationPolicyFormula> fall;
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

                    Func<SeperationPolicyFormula, string> orderfunc = (c =>
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
                            var temp = db.SeperationPolicyFormula
                                .Include(e => e.ExitProcess_Process_Policy)
                                .Include(e => e.ExitProcess_CheckList_Policy)
                                .Include(e => e.ExitProcess_Config_Policy)
                                .Include(e => e.SeperationPolicyAssignment)
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

                        var SeperationPolicyAssign = db2.SeperationPolicyAssignment.Include(e => e.SeperationFormula).ToList();
                        if (SeperationPolicyAssign.Count > 0)
                        {
                            foreach (var a in SeperationPolicyAssign)
                            {
                                foreach (var b in a.SeperationFormula)
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

                    var CompanyExit = db.CompanyExit.Where(e => e.Company.Id == id).SingleOrDefault();
                    var Policy = db.SeperationPolicyFormula.Where(e => e.Name == data).ToList();


                    CompanyExit.SeperationPolicyFormula = Policy;
                    CompanyExit.SeperationPolicyFormula = null;
                    db.CompanyExit.Attach(CompanyExit);
                    db.Entry(CompanyExit).State = System.Data.Entity.EntityState.Modified;

                    //
                    var SalHead1 = db.SeperationPolicyFormula.Where(e => e.Name == data).Select(e => e.Name).Distinct().FirstOrDefault();
                    var v1 = db.SeperationPolicyFormula.Where(a => a.Name == SalHead1).ToList();
                    db.SeperationPolicyFormula.RemoveRange(v1);



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




