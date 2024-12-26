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
    public class FormulaUpdationStructController : Controller
    {
        //
        // GET: /FormulaUpdationStruct/
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult getlookup()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var LWF = db.Lookup.Select(e => new { srno = e.Id, lookupvalue = e.Name }).ToList();

                return Json(LWF, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Create(FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    SalHeadFormula SalHead = new SalHeadFormula();
                    string geo_id = form["geo_id"] == "0" ? "" : form["geo_id"];
                    string fun_id = form["fun_id"] == "0" ? "" : form["fun_id"];
                    string pay_id = form["pay_id"] == "0" ? "" : form["pay_id"];

                    if (geo_id == "" && fun_id == "" && pay_id == "")
                    {
                        Msg.Add("  Kindly Apply Advance Filter..!!! ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    int CompId = 0;
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = int.Parse(Session["CompId"].ToString());
                    }
                    List<SalHeadFormula> OFAT1 = new List<SalHeadFormula>();
                    if ((pay_id != "") && (geo_id == "") && (fun_id == ""))
                    {
                        var ids = Utility.StringIdsToListIds(pay_id);
                        
                        foreach (var P in ids)
                        {
                            var pay = db.PayStruct.Include(e => e.Grade).Include(e => e.Level).Where(e => e.Id == P).FirstOrDefault();
                             

                            List<JobStatus> OJobStatus = new List<JobStatus>();
                            OJobStatus = db.JobStatus.ToList();
                           
                            foreach (var J in OJobStatus)
                            {
                                PayStruct OPayStruct = db.PayStruct.Where(e => e.Grade.Id == pay.Grade.Id && e.Level == null && e.JobStatus.Id == J.Id).FirstOrDefault();

                                List<SalHeadFormula> OOldFormula = db.SalHeadFormula//.Include(e => e.AmountDependRule).Include(e => e.BASICDependRule).Include(e => e.FormulaType)
                                   // .Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.NonStandardSalWages).Include(e => e.PayStruct).Include(e => e.PercentDependRule)
                                  //  .Include(e => e.SalWages).Include(e => e.ServiceDependRule).Include(e => e.SlabDependRule).Include(e => e.VDADependRule)
                            .Where(e => e.PayStruct.Id == OPayStruct.Id).ToList();
                                if (OOldFormula.Count > 0)
                                {
                                    foreach (var item in OOldFormula)
                                    {
                                        List<SalHeadFormula> OFAT = new List<SalHeadFormula>();
                                        int PayScaleAssignId = db.SalHeadFormula.Include(e => e.PayScaleAssignment).Where(e => e.Id == item.Id).FirstOrDefault().PayScaleAssignment.FirstOrDefault().Id;
                                        PayStruct OPayStructNew = db.PayStruct.Where(e => e.Grade.Id == pay.Grade.Id && e.Level.Id == pay.Level.Id && e.JobStatus.Id == J.Id).FirstOrDefault();
                                        if (OPayStructNew != null)
                                        {
                                            if (!db.SalHeadFormula.Include(e => e.PayStruct).Any(e => e.Name == item.Name && e.PayStruct.Id == OPayStructNew.Id))
                                            {
                                                SalHeadFormula OSalFor = db.SalHeadFormula.Include(e => e.AmountDependRule).Include(e => e.BASICDependRule).Include(e => e.FormulaType)
                                                     .Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.NonStandardSalWages).Include(e => e.PayStruct).Include(e => e.PercentDependRule)
                                                      .Include(e => e.SalWages).Include(e => e.ServiceDependRule).Include(e => e.SlabDependRule).Include(e => e.VDADependRule)
                                                .Where(e => e.Id == item.Id).FirstOrDefault();
                                                SalHeadFormula SalHeadForPayt = new SalHeadFormula()
                                                {
                                                    AmountDependRule = OSalFor.AmountDependRule,
                                                    BASICDependRule = OSalFor.BASICDependRule,
                                                    CeilingMax = item.CeilingMax,
                                                    CeilingMin = item.CeilingMin,
                                                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                                                    FuncStruct = OSalFor.FuncStruct,
                                                    GeoStruct = OSalFor.GeoStruct,
                                                    Name = item.Name + " " + OPayStructNew.Level.Name,
                                                    FormulaType = OSalFor.FormulaType,
                                                    PayStruct = OPayStructNew,
                                                    PercentDependRule = OSalFor.PercentDependRule,
                                                    SalWages = OSalFor.SalWages,
                                                    ServiceDependRule = OSalFor.ServiceDependRule,
                                                    SlabDependRule = OSalFor.SlabDependRule,
                                                    VDADependRule = OSalFor.VDADependRule,
                                                    NonStandardSalWages = OSalFor.NonStandardSalWages 
                                                    
                                                };

                                                db.SalHeadFormula.Add(SalHeadForPayt);
                                                db.SaveChanges();

                                                OFAT1.Add(db.SalHeadFormula.Find(SalHeadForPayt.Id));
                                                OFAT.Add(db.SalHeadFormula.Find(SalHeadForPayt.Id));


                                                PayScaleAssignment OPayScaleAssignment = db.PayScaleAssignment.Find(PayScaleAssignId);

                                                if (OPayScaleAssignment.SalHeadFormula != null)
                                                    OFAT.AddRange(OPayScaleAssignment.SalHeadFormula);
                                                OPayScaleAssignment.SalHeadFormula = OFAT;
                                                db.PayScaleAssignment.Attach(OPayScaleAssignment);
                                                db.Entry(OPayScaleAssignment).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                db.Entry(OPayScaleAssignment).State = System.Data.Entity.EntityState.Detached;
                                            }
                                             
                                        }
                                    }

                                }
                            }

                        
                        }
                        CompanyPayroll OCompanyPayroll = null; 
                        OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == CompId).SingleOrDefault();
                        var aa = db.CompanyPayroll.Find(OCompanyPayroll.Id); 
                        if (aa.SalHeadFormula != null)
                            OFAT1.AddRange(aa.SalHeadFormula);
                        aa.SalHeadFormula = OFAT1; 
                        db.CompanyPayroll.Attach(aa);
                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                    }
                    return Json(new Utility.JsonReturnClass { success = true, responseText = "  Data Saved successfully  " }, JsonRequestBehavior.AllowGet);
                }
                catch (DataException  dex)
                {
                    //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                    //ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                    //return View(level);
                    Msg.Add(dex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                }
            }
            return View();
        }
    }
}