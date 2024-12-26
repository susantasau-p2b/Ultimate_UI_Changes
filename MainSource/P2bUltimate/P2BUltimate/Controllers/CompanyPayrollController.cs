using P2BUltimate.App_Start;
using Payroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Data.Entity;
using System.Transactions;
using System.Text;
using P2BUltimate.Models;

namespace P2BUltimate.Controllers
{
    public class CompanyPayrollController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Fill_company()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.Company.ToList();
                SelectList s = new SelectList(db.Company, "Id", "Name", "");
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult create(String BaseClassName, string BaseClassId, String DerivedClassName, String PropertyName, string table)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = Convert.ToInt32(BaseClassId);
                var CompQuery = db.CompanyPayroll.Include(d => d.PFMaster).Include(e => e.ESICMaster)
                            .Include(e => e.LWFMaster).Include(e => e.PFMaster).Include(e => e.PTaxMaster).Where(d => d.Id == Id).SingleOrDefault();

                string Values = table;
                string caseSwitch = PropertyName;
                switch (caseSwitch)
                {
                    case "LWFMaster":
                        List<LWFMaster> LWFLkVal = new List<LWFMaster>();
                        if (Values != null && Values != "")
                        {
                            var ids = Utility.StringIdsToListIds(Values);
                            foreach (var ca in ids)
                            {
                                var LWF_val = db.LWFMaster.Find(ca);
                                LWFLkVal.Add(LWF_val);
                                CompQuery.LWFMaster = LWFLkVal;

                            }
                        }
                        else
                        {

                            CompQuery.LWFMaster = null;
                            return Json(new { success = false, responseText = "Select data" }, JsonRequestBehavior.AllowGet);
                        }
                        break;
                    case "PTaxMaster":
                        List<PTaxMaster> PTaxLkVal = new List<PTaxMaster>();
                        if (Values != null && Values != "")
                        {
                            var ids = Utility.StringIdsToListIds(Values);
                            foreach (var ca in ids)
                            {
                                var PTax_val = db.PTaxMaster.Find(ca);
                                PTaxLkVal.Add(PTax_val);
                                CompQuery.PTaxMaster = PTaxLkVal;

                            }
                        }
                        else
                        {
                            CompQuery.PTaxMaster = null;
                            return Json(new { success = false, responseText = "Select data" }, JsonRequestBehavior.AllowGet);
                        }
                        break;
                    case "PFMaster":
                        List<PFMaster> PFLkVal = new List<PFMaster>();
                        if (Values != null && Values != "")
                        {
                            var ids = Utility.StringIdsToListIds(Values);
                            foreach (var ca in ids)
                            {
                                var PF_val = db.PFMaster.Find(ca);
                                PFLkVal.Add(PF_val);
                                CompQuery.PFMaster = PFLkVal;

                            }
                        }
                        else
                        {
                            CompQuery.PFMaster = null;
                            return Json(new { success = false, responseText = "Select data" }, JsonRequestBehavior.AllowGet);
                        }
                        break;
                    case "ESICMaster":
                        List<ESICMaster> ESICLkVal = new List<ESICMaster>();
                        if (Values != null && Values != "")
                        {
                            var ids = Utility.StringIdsToListIds(Values);
                            foreach (var ca in ids)
                            {
                                var ESIC_val = db.ESICMaster.Find(ca);
                                ESICLkVal.Add(ESIC_val);
                                CompQuery.ESICMaster = ESICLkVal;

                            }
                        }
                        else
                        {
                            CompQuery.ESICMaster = null;
                            return Json(new { success = false, responseText = "Select data" }, JsonRequestBehavior.AllowGet);
                        }
                        break;
                    default:
                        Console.WriteLine("Default case");
                        break;
                }




                if (ModelState.IsValid)
                {

                    try
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            db.CompanyPayroll.Attach(CompQuery);
                            db.Entry(CompQuery).State = System.Data.Entity.EntityState.Modified;
                            TempData["RowVersion"] = CompQuery.RowVersion;
                            db.Entry(CompQuery).State = System.Data.Entity.EntityState.Detached;
                            db.SaveChanges();
                            ts.Complete();
                            return Json(new { success = true, responseText = "Record Created" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception e)
                    {

                        throw e;
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
                    return Json(new { success = true, responseText = errorMsg }, JsonRequestBehavior.AllowGet);
                    // return this.Json(new { msg = errorMsg });
                }
                return Json(new { success = true, responseText = "Data Saved successfully" }, JsonRequestBehavior.AllowGet);

            }
        }


        public ActionResult Getlookupdetails(string data, List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (!string.IsNullOrEmpty(data))
                {
                    string caseSwitch = data;
                    switch (caseSwitch)
                    {
                        case "LWFMaster":
                            var LWFData = db.LWFMaster.Include(e => e.LWFStates).Include(e => e.WagesMaster).Include(e => e.LWFStatutoryEffectiveMonths).ToList();
                            if (SkipIds != null)
                            {
                                foreach (var a in SkipIds)
                                {
                                    if (LWFData == null)
                                        LWFData = db.LWFMaster.Include(e => e.LWFStates).Include(e => e.WagesMaster).Include(e => e.LWFStatutoryEffectiveMonths).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                                    else
                                        LWFData = LWFData.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                                }
                            }
                            var r = (from ca in LWFData select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                            return Json(r, JsonRequestBehavior.AllowGet);

                            break;
                        case "PTaxMaster":
                            var PTaxData = db.PTaxMaster.Include(e => e.States).Include(e => e.PTWagesMaster).Include(e => e.PTStatutoryEffectiveMonths).ToList();
                            if (SkipIds != null)
                            {
                                foreach (var a in SkipIds)
                                {
                                    if (PTaxData == null)
                                        PTaxData = db.PTaxMaster.Include(e => e.States).Include(e => e.PTWagesMaster).Include(e => e.PTStatutoryEffectiveMonths).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                                    else
                                        PTaxData = PTaxData.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                                }
                            }
                            var r1 = (from ca in PTaxData select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                            return Json(r1, JsonRequestBehavior.AllowGet);



                            break;
                        case "PFMaster":
                            var PFData = db.PFMaster.ToList();
                            if (SkipIds != null)
                            {
                                foreach (var a in SkipIds)
                                {
                                    if (PFData == null)
                                        PFData = db.PFMaster.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                                    else
                                        PFData = PFData.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                                }
                            }
                            var r2 = (from ca in PFData select new { srno = ca.Id, lookupvalue = ca.CompPFNo }).Distinct();
                            return Json(r2, JsonRequestBehavior.AllowGet);
                            break;
                        case "ESICMaster":
                            var ESICData = db.ESICMaster.Include(e => e.WageMasterQualify).Include(e => e.WageMasterPay).Include(e => e.ESICStatutoryEffectiveMonths).ToList();
                            if (SkipIds != null)
                            {
                                foreach (var a in SkipIds)
                                {
                                    if (ESICData == null)
                                        ESICData = db.ESICMaster.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                                    else
                                        ESICData = ESICData.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                                }
                            }
                            var r3 = (from ca in ESICData select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                            return Json(r3, JsonRequestBehavior.AllowGet);
                            break;
                        default:
                            Console.WriteLine("Default case");
                            break;
                    }
                }

                return View();
            }
        }

        public ActionResult GetProperty(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                data = "EPMS";
                string AssemblyName = "";
                string caseSwitch = data;
                switch (caseSwitch)
                {
                    case "EPMS":
                        AssemblyName = "Payroll";
                        break;
                    case "ELMS":
                        AssemblyName = "Leave";
                        break;
                    case "ETRM":
                        AssemblyName = "Attendance";
                        break;
                    case "EDMS":
                        AssemblyName = "Document";
                        break;

                }

                Assembly assembly = Assembly.Load(AssemblyName);

                Type t = assembly.GetType(AssemblyName + "." + "CompanyPayroll");
                var type = Activator.CreateInstance(t);

                IList<PropertyInfo> props = new List<PropertyInfo>(t.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly));
                List<SelectListItem> values = new List<SelectListItem>();

                foreach (PropertyInfo prop in props)
                {
                    if (prop.Name != "Id" && prop.Name != "DBTrack" && prop.Name != "RowVersion" && prop.Name != "Company")
                    {
                        values.Add(new SelectListItem
                        {
                            Text = prop.Name,
                            Value = prop.Name.ToString(),
                        });
                    }
                }
                return Json(values, JsonRequestBehavior.AllowGet);
            }

        }
    }
}

