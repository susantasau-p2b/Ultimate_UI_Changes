using EssPortal.App_Start;
using EssPortal.Models;
using P2b.Global;
using Payroll;
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
using EssPortal.Security;
using System.Threading.Tasks;

namespace EssPortal.Controllers
{
    public class RegimeSchemeController : Controller
    {
        //
        // GET: /RegimeScheme/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/_RegimiScheme.cshtml");
        }

        public ActionResult PopulateDropDownListCalendar(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).ToList();
                //  var qurey = db.Calendar.Include(e=>e.Name).ToList();
                var selected = "";
                if (!string.IsNullOrEmpty(data2))
                {
                    selected = data2;
                }
                var returndata = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateDropDownListCalendarRegime(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).ToList();
                //  var qurey = db.Calendar.Include(e=>e.Name).ToList();
                var selected = "";
                if (!string.IsNullOrEmpty(data2))
                {
                    selected = data2;
                }
                var returndata = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Create(RegimiScheme frmRegimiScheme, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string FinancialYeardrop = form["FinancialYeardrop1"] == "0" ? null : form["FinancialYeardrop1"];
                    string Schemedrop = form["Schemedrop2"] == "0" ? null : form["Schemedrop2"];
                    string HolidayListlist = form["HolidayListlist"] == null ? null : form["HolidayListlist"];
                    int comp_Id = 0;
                    comp_Id = Convert.ToInt32(Session["CompId"]);
                    var oCompany = new Company();

                    oCompany = db.Company.Include(e => e.Location)
                        .Where(e => e.Id == comp_Id).FirstOrDefault();


                    if (Schemedrop == null && Schemedrop == "")
                    { 
                        Msg.Add(" Kindly select RegimiScheme  "); 
                        return this.Json(new Object[] { 0, null, Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (FinancialYeardrop == null && FinancialYeardrop == "")
                    { 
                        Msg.Add(" Kindly select Calendar  "); 
                        return this.Json(new Object[] { 0, null, Msg }, JsonRequestBehavior.AllowGet);
                    }

                    Calendar cal = null;
                    LookupValue lk = null;
                    if (FinancialYeardrop != null)
                    {
                        int hdid = Convert.ToInt16(FinancialYeardrop);
                        cal = db.Calendar.Include(a => a.Name).Where(q => q.Id == hdid).SingleOrDefault();//.Find(int.Parse(HoliCalendardrop));
                        frmRegimiScheme.FinancialYear = cal;
                    }

                    if (Schemedrop != null)
                    {
                        lk = db.LookupValue.Find(int.Parse(Schemedrop));
                        frmRegimiScheme.Scheme = lk;
                    }
                    int holiid = Convert.ToInt32(FinancialYeardrop);
                    int Schemeid = Convert.ToInt32(Schemedrop);

                    if (db.RegimiScheme.Include(q => q.FinancialYear).Include(x => x.Scheme).Any(e => e.FinancialYear.Id == holiid && e.Scheme_Id == Schemeid))
                    {
                        Msg.Add(" Data with this Calendar Year Already Exist. ");
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        return this.Json(new Object[] { 0, null, Msg }, JsonRequestBehavior.AllowGet);
                    }



                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            frmRegimiScheme.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            RegimiScheme oRegimeSch = new RegimiScheme()
                            {
                                FinancialYear = frmRegimiScheme.FinancialYear,
                                Scheme = frmRegimiScheme.Scheme,
                                DBTrack = frmRegimiScheme.DBTrack,

                            };
                            try
                            {



                                db.RegimiScheme.Add(oRegimeSch);
                                db.SaveChanges();

                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                               // return Json(new Utility.JsonReturnClass { Id = oRegimeSch.Id, Val = oRegimeSch.FinancialYear.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                return this.Json(new Object[] { oRegimeSch.Id, oRegimeSch.FinancialYear.FullDetails + " " + oRegimeSch.Scheme.LookupVal, "Data Saved Successfully" }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { , , "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = "" });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                               // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                return this.Json(new Object[] { 0, null, Msg }, JsonRequestBehavior.AllowGet);
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
                        // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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
            }
        }

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.RegimiScheme.Include(e => e.FinancialYear).Include(e => e.Scheme).Where(e => e.Id == data).Select(e => new
                {

                    Financialyear_Id = e.FinancialYear != null ? e.FinancialYear.Id : 0,
                    Scheme_Id = e.Scheme != null ? e.Scheme.Id : 0,


                }).ToList();


                var Corp = db.RegimiScheme.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });

            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(RegimiScheme L, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string FinancialYeardrop = form["FinancialYeardrop1"] == "0" ? null : form["FinancialYeardrop1"];
                    string Schemedrop = form["Schemedrop2"] == "0" ? null : form["Schemedrop2"];

                    if (Schemedrop == null && Schemedrop == "")
                    {
                        Msg.Add(" Kindly select RegimiScheme  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    if (FinancialYeardrop == null && FinancialYeardrop == "")
                    {
                        Msg.Add(" Kindly select Calendar  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }


                    bool Auth = form["autho_allow"] == "true" ? true : false;
                    if (FinancialYeardrop != null && FinancialYeardrop != "" && FinancialYeardrop != "-Select-")
                    { 
                        L.FinancialYear_Id = int.Parse(FinancialYeardrop); 
                    }
                    else
                    {
                        L.FinancialYear_Id = null;
                    }
                    if (Schemedrop != null && Schemedrop != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Schemedrop));
                        L.Scheme = val;
                    }
 
                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    RegimiScheme blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    { 
                                        blog = context.RegimiScheme.Where(e => e.Id == data).Include(e => e.FinancialYear).Include(e => e.Scheme)

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

                                    if (FinancialYeardrop != null && FinancialYeardrop != "")
                                    {

                                        var val = db.Calendar.Find(int.Parse(FinancialYeardrop));
                                        L.FinancialYear = val;
                                        var type = db.RegimiScheme.Include(e => e.FinancialYear).Include(e => e.Scheme).Where(e => e.Id == data).SingleOrDefault();
                                        //  var type = db.HolidayCalendar.Include(e => e.HoliCalendar).Where(e => e.Id == data).SingleOrDefault();
                                        IList<RegimiScheme> typedetails = null;
                                        if (type.FinancialYear != null)
                                        {
                                            typedetails = db.RegimiScheme.Include(e => e.FinancialYear).Include(e => e.Scheme).Where(x => x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.RegimiScheme.Include(e => e.FinancialYear).Include(e => e.Scheme).Where(x => x.Id == data).ToList();
                                        }
                                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                        foreach (var s in typedetails)
                                        {
                                            s.FinancialYear_Id = L.FinancialYear_Id;
                                            s.Scheme = L.Scheme;
                                            db.RegimiScheme.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
 
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        RegimiScheme RegimiScheme = new RegimiScheme()
                                        {

                                            Id = data,
                                            DBTrack = L.DBTrack,
                                            Scheme = L.Scheme,
                                            FinancialYear_Id = L.FinancialYear_Id
                                        };
                                        db.RegimiScheme.Attach(RegimiScheme);
                                        db.Entry(RegimiScheme).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(RegimiScheme).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        db.SaveChanges();
                                    }
                              
                                        
                                   
                                
                                    var query = db.RegimiScheme.Include(e => e.FinancialYear).Include(e => e.FinancialYear.Name).Include(e => e.Scheme).Where(e => e.Id == data).FirstOrDefault();
                                    ts.Complete(); 
                                    Msg.Add("  Record Updated"); 
                                    return Json(new Utility.JsonReturnClass { Id = query.Id, Val = query.FinancialYear.FullDetails + " " + query.Scheme.LookupVal, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }

                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (HolidayCalendar)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (HolidayCalendar)databaseEntry.ToObject();
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

                            RegimiScheme blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            RegimiScheme Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.RegimiScheme.Where(e => e.Id == data).SingleOrDefault();
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

                            RegimiScheme corp = new RegimiScheme()
                            {
                                FinancialYear = L.FinancialYear,
                                Scheme = L.Scheme,
                                Id = data,
                                DBTrack = L.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };




                            using (var context = new DataBaseContext())
                            {

                            }
                            blog.DBTrack = L.DBTrack;
                            db.RegimiScheme.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = L.Id, Val = L.FinancialYear.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
	}
}