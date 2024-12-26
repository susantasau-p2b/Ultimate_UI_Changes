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
using P2b.Global;

namespace P2BUltimate.Controllers
{
    public class CompanyPayscaleAssignmentController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        private DataBaseContext db = new DataBaseContext();
        public ActionResult Fill_company()
        {
            var a = db.Company.ToList();
            SelectList s = new SelectList(db.Company, "Id", "Name", "");
            return Json(s, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Fill_Payscale()
        {
            List<SelectListItem> payscale_list = new List<SelectListItem>();
            var payscaleagreement_data = db.PayScaleAgreement.ToList();
            foreach (var ca in payscaleagreement_data)
            {
                payscale_list.Add(new SelectListItem
                {
                    Value = ca.Id.ToString(),
                    Text = ca.EffDate.Value.Date.ToShortDateString() + " TO " + ca.EndDate.Value.Date.ToShortDateString(),
                    Selected = false
                });
            }
            return Json(payscale_list, JsonRequestBehavior.AllowGet);
        }
        public ActionResult create(String BaseClassName, string BaseClassId, String DerivedClassName, String PropertyName, string table)
        {

            int Id = Convert.ToInt32(BaseClassId);
            var CompQuery = db.Company.OfType<CompanyPayroll>().Include(d => d.PFMaster).Include(e => e.ESICMaster)
                        .Include(e => e.LWFMaster).Include(e => e.PFMaster).Include(e => e.PTaxMaster).Include(e => e.PromoActivity).Where(d => d.Id == Id).SingleOrDefault();

            string Values = table;
            string caseSwitch = PropertyName;
            switch (caseSwitch)
            {
                case "IncrActivity":
                    List<IncrActivity> LWFLkVal = new List<IncrActivity>();
                    if (Values != null && Values != "")
                    {
                        var ids = one_ids(Values);
                        foreach (var ca in ids)
                        {
                            var LWF_val = db.IncrActivity.Find(ca);
                            LWFLkVal.Add(LWF_val);
                            CompQuery.IncrActivity = LWFLkVal;

                        }
                    }
                    else
                    {
                        CompQuery.IncrActivity = null;
                        return Json(new { success = false, responseText = "Select data" }, JsonRequestBehavior.AllowGet);
                    }
                    break;
                case "PromoActivity":
                    List<PromoActivity> PTaxLkVal = new List<PromoActivity>();
                    if (Values != null && Values != "")
                    {
                        var ids = one_ids(Values);
                        foreach (var ca in ids)
                        {
                            var PTax_val = db.PromoActivity.Find(ca);
                            PTaxLkVal.Add(PTax_val);
                            CompQuery.PromoActivity = PTaxLkVal;

                        }
                    }
                    else
                    {
                        CompQuery.PromoActivity = null;
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

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        public ActionResult Getlookupdetails(string data, List<int> SkipIds)
        {
            if (!string.IsNullOrEmpty(data))
            {
                string caseSwitch = data;
                switch (caseSwitch)
                {
                    case "IncrActivity":
                        var IncrData = db.IncrActivity.ToList();
                        if (SkipIds != null)
                        {
                            foreach (var a in SkipIds)
                            {
                                if (IncrData == null)
                                    IncrData = db.IncrActivity.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                                else
                                    IncrData = IncrData.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                            }
                        }
                        var r = (from ca in IncrData select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                        return Json(r, JsonRequestBehavior.AllowGet);
                    case "PromoActivity":
                        var PromoData = db.PromoActivity.ToList();
                        if (SkipIds != null)
                        {
                            foreach (var a in SkipIds)
                            {
                                if (PromoData == null)
                                    PromoData = db.PromoActivity.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                                else
                                    PromoData = PromoData.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                            }
                        }
                        var r1 = (from ca in PromoData select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                        return Json(r1, JsonRequestBehavior.AllowGet);
                    default:
                        Console.WriteLine("Default case");
                        break;
                }
            }

            return View();

        }

        public ActionResult GetProperty(string data)
        {
            //    string AssemblyName = "";
            //    string caseSwitch = data;
            //    switch (caseSwitch)
            //    {
            //        case "EPMS":
            //            AssemblyName = "Payroll";
            //            break;
            //        case "ELMS":
            //            AssemblyName = "Leave";
            //            break;
            //        case "ETRM":
            //            AssemblyName = "Attendance";
            //            break;
            //        case "EDMS":
            //            AssemblyName = "Document";
            //            break;

            //    }

            //    Assembly assembly = Assembly.Load(AssemblyName);

            //    Type t = assembly.GetType(AssemblyName + "." + "CompanyPayroll");
            //    var type = Activator.CreateInstance(t);

            //    IList<PropertyInfo> props = new List<PropertyInfo>(t.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly));
            //    List<SelectListItem> values = new List<SelectListItem>();

            //    foreach (PropertyInfo prop in props)
            //    {

            //        values.Add(new SelectListItem
            //        {
            //            Text = prop.Name,
            //            Value = prop.Name.ToString(),
            //        });

            //    }
            return Json("", JsonRequestBehavior.AllowGet);
        }


    }
}

