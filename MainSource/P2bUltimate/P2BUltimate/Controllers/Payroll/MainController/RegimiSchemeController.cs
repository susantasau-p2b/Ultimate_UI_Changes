using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Xml;
using P2BUltimate.App_Start;
using System.Net;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Transactions;
using P2b.Global;
using System.Text;
using System.Threading.Tasks;
using P2BUltimate.Security;
using Payroll;
using Leave;


namespace P2BUltimate.Controllers.Core.MainController
{
    public class RegimiSchemeController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Partial()
        {
            return View("~/Views/Shared/Payroll/_RegimiScheme.cshtml");
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
                    string HolidayListList = form["HolidayListList"] == null ? null : form["HolidayListList"];
                    int comp_Id = 0;
                    comp_Id = Convert.ToInt32(Session["CompId"]);
                    var oCompany = new Company();
                    
                    oCompany = db.Company.Include(e => e.Location)
                        .Where(e => e.Id == comp_Id).FirstOrDefault();


                    if (Schemedrop == null)
                    {
                        if (Schemedrop == "")
                        {
                            Msg.Add(" Kindly select RegimiScheme  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        Msg.Add(" Kindly select RegimiScheme  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (FinancialYeardrop == null)
                    {
                        if (FinancialYeardrop == "")
                        {
                            Msg.Add(" Kindly select Calendar  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        Msg.Add(" Kindly select Calendar  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    Calendar cal = null;
                    LookupValue lk = null;
                    if (FinancialYeardrop != null)
                    {
                        int hdid = Convert.ToInt16(FinancialYeardrop);
                        cal = db.Calendar.Include(a => a.Name).Where(q => q.Id == hdid).SingleOrDefault();//.Find(int.Parse(HoliCalendarDDL));
                        frmRegimiScheme.FinancialYear = cal;
                    }

                    if (Schemedrop != null)
                    {
                        lk = db.LookupValue.Find(int.Parse(Schemedrop));
                        frmRegimiScheme.Scheme = lk;
                    }
                    int holiid = Convert.ToInt32(FinancialYeardrop);
                      int Schemeid = Convert.ToInt32(Schemedrop);

                      if (db.RegimiScheme.Include(q => q.FinancialYear).Include(x => x.Scheme).Any(e => e.FinancialYear.Id == holiid && e.Scheme.Id == Schemeid))
                    {
                        Msg.Add(" Data with this Calendar Year Already Exist. ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                                return Json(new Utility.JsonReturnClass { Id = oRegimeSch.Id, Val = oRegimeSch.FinancialYear.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { , , "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = "" });
                            }
                            catch (DataException /* dex */)
                            {
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
        //public ActionResult Edit(int data)
        //{
        //    var id = Convert.ToInt32(data);
        //    var data1 = db.Calendar.Where(e => e.Id == id).SingleOrDefault();
        //    var data2 = db.HolidayCalendar.Include(e => e.HolidayList)
        //        .Select(e => new
        //        {
        //            HolidayList_id = e.HolidayList.Select(a => a.Id.ToString()).ToArray(),
        //            HolidayList_val = e.HolidayList.Select(a => a.HolidayDate).ToArray()
        //        }).ToList();
        //    return Json(new Object[] { data1, data2 }, JsonRequestBehavior.AllowGet);
        //}


        public class holicalendarlistdetails
        {
            public string Financialyear_Id { get; set; }
            public string Scheme_Id { get; set; }
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

                    if (Schemedrop == null)
                    {
                        if (Schemedrop == "")
                        {
                            Msg.Add(" Kindly select RegimiScheme  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        Msg.Add(" Kindly select RegimiScheme  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (FinancialYeardrop == null)
                    {
                        if (FinancialYeardrop == "")
                        {
                            Msg.Add(" Kindly select Calendar  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        Msg.Add(" Kindly select Calendar  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
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

                    if (db.RegimiScheme.Include(q => q.FinancialYear).Include(x => x.Scheme).Any(e => e.FinancialYear.Id == L.FinancialYear_Id && e.Scheme.Id == L.Scheme.Id))
                    {
                        Msg.Add(" Data with this Calendar Year Already Exist. ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                                        //blog = context.HolidayCalendar.Where(e => e.Id == data).Include(e => e.HoliCalendar)
                                        //                        .Include(e => e.HolidayList).Include(e => e.HoliCalendar.Name)
                                        //                        .SingleOrDefault();
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
                                            typedetails = db.RegimiScheme.Include(e => e.FinancialYear).Include(e => e.Scheme).Where(x =>  x.Id == data).ToList();
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
                                   


                                   

                                    // *********** End Duplicate HolidayList Checking Added on : 04-01-2024 By Anandrao **************






                                 
                                    ////var CurCorp = db.HolidayCalendar.Find(data);
                                    ////TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    ////db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
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
                                    }
                                    // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                    using (var context = new DataBaseContext())
                                    {

                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    //ts.Complete();
                                    ////var qurey = db.HolidayCalendar.Include(e => e.HoliCalendar).Include(e => e.HolidayList).Where(e => e.Id == data).SingleOrDefault();
                                    //Msg.Add("  Record Updated");
                                    //return Json(new Utility.JsonReturnClass { Id = data, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                                    var query = db.RegimiScheme.Include(e => e.FinancialYear).Include(e => e.FinancialYear.Name).Include(e => e.Scheme).Where(e => e.Id == data).SingleOrDefault();
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = query.Id, Val = query.FinancialYear.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

        public class P2BGridClass
        {
            public int Id { get; set; }
            public string FinaCalendar { get; set; }
            public string Scheme { get; set; }
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
                IEnumerable<P2BGridClass> RegimiScheme = null;
                var data = db.RegimiScheme.Include(e => e.FinancialYear).Include(e => e.Scheme).ToList();
                List<P2BGridClass> holidaylist = new List<P2BGridClass>();
                foreach (var item in data)
                {
                    holidaylist.Add(new P2BGridClass
                    {
                        Id = item.Id,
                        FinaCalendar = item.FinancialYear != null ? item.FinancialYear.FullDetails : null,
                        Scheme = item.Scheme.LookupVal.ToUpper(),
                    });
                }
                RegimiScheme = holidaylist;
                IEnumerable<P2BGridClass> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = RegimiScheme;
                    if (gp.searchOper.Equals("eq"))
                    {
                        if (gp.searchField == "Id")
                            jsonData = IE.Select(a => new { a.Id, a.Scheme, a.FinaCalendar }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "HoliCalendar")
                            jsonData = IE.Select(a => new { a.Id, a.Scheme, a.FinaCalendar }).Where((e => (e.FinaCalendar.ToString().Contains(gp.searchString)))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Scheme, a.FinaCalendar }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = RegimiScheme;
                    Func<P2BGridClass, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.FinaCalendar
                                          : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Scheme, a.FinaCalendar }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Scheme, a.FinaCalendar }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Scheme, a.FinaCalendar }).ToList();
                    }
                    totalRecords = RegimiScheme.Count();
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
            public int Id { get; set; }
            public string HoliCalendar { get; set; }
            public string Name { get; set; }
            //it was public Calendar HoliCalendar { get; set; }
            // LoanAdvRequest 
        }

        public class ChildDataClass
        {
            public String FullDetails { get; set; }
            // public ICollection<HolidayList> HolidayList { get; set; }
        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    List<returndatagridclass> model = new List<returndatagridclass>();
                    returndatagridclass view = null;

                    var all = db.HolidayCalendar.Include(e => e.HoliCalendar).Include(e => e.HoliCalendar.Name).ToList();

                    IEnumerable<HolidayCalendar> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => e.HoliCalendar.Id.ToString().Contains(param.sSearch)
                            || (e.HoliCalendar.ToString().Contains(param.sSearch))
                            || (e.Name.ToString().Contains(param.sSearch))
                            ).ToList();
                    }

                    foreach (var z in fall)
                    {
                        view = new returndatagridclass()
                        {
                            Id = z.Id,
                            HoliCalendar = z.HoliCalendar != null ? z.HoliCalendar.FullDetails : null,
                            Name = z.Name
                        };
                        model.Add(view);
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<returndatagridclass, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.HoliCalendar : "");
                    var sortcolumn = Request["sSortDir_0"];
                    var dcompanies = model
                              .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompanies.Count == 0)
                    {
                        List<returndatagridclass> result = new List<returndatagridclass>();
                        result = model;
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = model.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = from c in dcompanies

                                     select new[] { null, Convert.ToString(c.Id), c.HoliCalendar, c.Name };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = model.Count(),
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


        public class returnDataClass
        {
            public Int32 Id { get; set; }
            public String HolidayList { get; set; }
            // public ICollection<HolidayList> HolidayList { get; set; }             
        }
        public ActionResult GridEditData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returnlist = new List<returnDataClass>();

                if (data != 0)
                {

                    var retrundata = db.HolidayCalendar.Include(e => e.HolidayList).Include(e => e.HolidayList.Select(a => a.Holiday)).Include(e => e.HolidayList.Select(a => a.Holiday.HolidayName)).Include(e => e.HolidayList.Select(a => a.Holiday.HolidayType)).Where(e => e.Id == data).SingleOrDefault();
                    foreach (var item in retrundata.HolidayList.OrderBy(e => e.HolidayDate))
                    {
                        if (item.Holiday != null)
                        {
                            returnlist.Add(new returnDataClass
                            {
                                Id = item.Id,
                                HolidayList = item.Holiday.FullDetails + ",HolidayDate :" + item.HolidayDate.Value.ToShortDateString()
                            });
                        }
                        else
                        {
                            return Json(new { status = false, responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    return Json(returnlist, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View();
                }
            }
        }

        public JsonResult EditGridDetails(int data, string filter)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var all = db.HolidayCalendar.Include(e => e.HolidayList).Where(e => e.Id == data).SingleOrDefault();

                if (all.HolidayList.Count > 0)
                {
                    List<ChildDataClass> returndata = new List<ChildDataClass>();
                    List<HolidayList> Data = new List<HolidayList>();

                    if (filter != "")
                    {
                        Data = all.HolidayList.Where(e => e.Holiday.ToString().Contains(filter)).ToList();
                    }
                    else
                    {
                        Data = all.HolidayList.ToList();
                    }

                    foreach (var item in Data)
                    {

                        returndata.Add(new ChildDataClass
                        {
                            FullDetails = item.FullDetails
                        });
                    }
                    return Json(returndata, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                }

            }

        }



        public ActionResult GridEditSave(HolidayCalendar ypay, FormCollection from, string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    var db_data = db.HolidayCalendar.Where(e => e.Id == id).SingleOrDefault();

                    var val = db.HolidayCalendar.Include(e => e.HoliCalendar).ToList();
                    foreach (var a in val)
                    {
                        if (a.Id == id)
                        {
                            var SalT = db.HolidayCalendar.Where(e => e.Id == a.Id).Select(e => e.HolidayList.Where(r => r.FullDetails == a.HolidayList.ToString()).FirstOrDefault()).SingleOrDefault();

                        }
                    }

                    db_data.HolidayList = ypay.HolidayList;
                    try
                    {
                        db.HolidayCalendar.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        return Json(new { status = true, data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception e)
                    {

                        return Json(new { status = false, responseText = e.InnerException.ToString() }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { status = false, responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
                }
            }
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

        public ActionResult PopulateDropDownListCalendarEditview(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int hid = Convert.ToInt32(data2);
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "HOLIDAYCALENDAR" && e.Id == hid).ToList();
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


        //[HttpPost]
        //public async Task<ActionResult> Delete(int data)
        //{

        //    var HolidayCalendars = db.HolidayCalendar
        //        .Include(e => e.HoliCalendar)
        //        .Include(e => e.HolidayList)
        //        .Where(e => e.Id == data).SingleOrDefault();
        //    if (HolidayCalendars.HoliCalendar != null && HolidayCalendars.HolidayList != null)
        //    {
        //        return Json(new Object[] { "", "", "Child Record Exits..!" }, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        try
        //        {
        //            using (TransactionScope ts = new TransactionScope())
        //            {


        //                db.Entry(HolidayCalendars).State = System.Data.Entity.EntityState.Deleted;
        //                db.SaveChanges();
        //                return Json(new Object[] { "", "", "Record Deleted..!" }, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //        catch (Exception e)
        //        {

        //            return Json(new Object[] { "", "", e.InnerException.ToString() }, JsonRequestBehavior.AllowGet);
        //        }

        //    }

        //}

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    HolidayCalendar holidaycalendar = db.HolidayCalendar.Include(e => e.HolidayList)
                                                       .Include(e => e.HoliCalendar)
                                                       .Where(e => e.Id == data).SingleOrDefault();


                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (holidaycalendar.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = holidaycalendar.DBTrack.CreatedBy != null ? holidaycalendar.DBTrack.CreatedBy : null,
                                CreatedOn = holidaycalendar.DBTrack.CreatedOn != null ? holidaycalendar.DBTrack.CreatedOn : null,
                                IsModified = holidaycalendar.DBTrack.IsModified == true ? true : false
                            };
                            holidaycalendar.DBTrack = dbT;
                            db.Entry(holidaycalendar).State = System.Data.Entity.EntityState.Modified;
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, holidaycalendar.DBTrack);
                            //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            //DT_Corp.Address_Id = corporates.Address == null ? 0 : corporates.Address.Id;
                            //DT_Corp.BusinessType_Id = corporates.BusinessType == null ? 0 : corporates.BusinessType.Id;
                            //DT_Corp.ContactDetails_Id = corporates.ContactDetails == null ? 0 : corporates.ContactDetails.Id;
                            //db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        var HolidayList = holidaycalendar.HolidayList;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            if (HolidayList != null)
                            {
                                var holidayList = new HashSet<int>(holidaycalendar.HolidayList.Select(e => e.Id));
                                if (holidayList.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                    // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                }
                            }

                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = holidaycalendar.DBTrack.CreatedBy != null ? holidaycalendar.DBTrack.CreatedBy : null,
                                    CreatedOn = holidaycalendar.DBTrack.CreatedOn != null ? holidaycalendar.DBTrack.CreatedOn : null,
                                    IsModified = holidaycalendar.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(holidaycalendar).State = System.Data.Entity.EntityState.Deleted;
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                                //DT_Corp.Address_Id = add == null ? 0 : add.Id;
                                //DT_Corp.BusinessType_Id = val == null ? 0 : val.Id;
                                //DT_Corp.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
                                //db.Create(DT_Corp);

                                await db.SaveChangesAsync();


                                //using (var context = new DataBaseContext())
                                //{
                                //    corporates.Address = add;
                                //    corporates.ContactDetails = conDet;
                                //    corporates.BusinessType = val;
                                //    DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", dbT);
                                //}
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
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
            }
        }

        public ActionResult GetLookup(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.HolidayCalendar.Include(e => e.HoliCalendar).Include(e => e.HolidayList).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.HolidayCalendar.Include(e => e.HoliCalendar).Include(e => e.HolidayList).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.HoliCalendar.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
            // return View();
        }

        public void RollBack()
        {

            //  var context = DataContextFactory.GetDataContext();
            using (DataBaseContext db = new DataBaseContext())
            {
                var changedEntries = db.ChangeTracker.Entries()
                    .Where(x => x.State != System.Data.Entity.EntityState.Unchanged).ToList();

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Modified))
                {
                    entry.CurrentValues.SetValues(entry.OriginalValues);
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Added))
                {
                    entry.State = System.Data.Entity.EntityState.Detached;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Deleted))
                {
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }
            }
        }

        public ActionResult ValidateForm(HolidayCalendar FormHolidayCalendar, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string HoliCalendarDDL = form["HoliCalendarDDL"] == "0" ? null : form["HoliCalendarDDL"];
                if (HoliCalendarDDL != null && HoliCalendarDDL != "-Select-")
                {

                    var value = db.Calendar.Find(int.Parse(HoliCalendarDDL));
                    FormHolidayCalendar.HoliCalendar = value;

                }
                else
                {
                    List<string> Msg = new List<string>();
                    Msg.Add("  Kindly select one value from Calendar.  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }



                if (db.HolidayCalendar.Any(e => e.HoliCalendar.Id == FormHolidayCalendar.HoliCalendar.Id))
                {
                    var Msg = new List<string>();
                    Msg.Add("Already Exist");
                    return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }

                return Json(new { success = true }, JsonRequestBehavior.AllowGet);

            }

        }

        public ActionResult GridDelete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var LvEP = db.HolidayList.Find(data);
                db.HolidayList.Remove(LvEP);
                db.SaveChanges();
                //return Json(new Object[] { "", "", " Record Deleted Successfully " }, JsonRequestBehavior.AllowGet);
                List<string> Msgr = new List<string>();
                Msgr.Add("Record Deleted Successfully  ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}