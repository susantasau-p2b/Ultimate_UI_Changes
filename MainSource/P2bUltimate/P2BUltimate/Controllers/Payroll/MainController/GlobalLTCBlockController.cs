using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using System.IO;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class GlobalLTCBlockController : Controller
    {
        //
        // GET: /GlobalLTCBlock/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/GlobalLTCBlock/Index.cshtml");
        }

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.GlobalLTCBlock.Where(e => e.Id == data).AsEnumerable()
                    .Select(e => new
                    {
                      //  Name = e.Name != null ? e.Name.Id : 0,
                        BlockStart = e.BlockStart,
                        BlockEnd = e.BlockEnd,
                        NoOfTimes = e.NoOfTimes,
                        BlockYear = e.BlockYear,
                    }).ToList();
                return Json(new Object[] { returndata, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public ActionResult Create(GlobalLTCBlock c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //var Name = form["Name_drop"] == "0" ? 0 : Convert.ToInt32(form["Name_drop"]);
                    //if (Name != 0)
                    //{
                    //    c.Name = db.LookupValue.Find(Name);
                    //}
                    c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };
                    if (ModelState.IsValid)
                    {
                        GlobalLTCBlock GlobalLTCBlock = new GlobalLTCBlock()
                        {
                            BlockYear = c.BlockYear,
                            BlockStart = c.BlockStart,
                            BlockEnd = c.BlockEnd,
                            NoOfTimes = c.NoOfTimes,
                            DBTrack = c.DBTrack
                        };
                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {

                                //var alrDq = db.GlobalLTCBlock
                                // .Any(q => ((q.BlockStart <= c.BlockStart && q.BlockEnd <= c.BlockEnd) || (q.BlockStart <= c.BlockStart && q.BlockEnd >= c.BlockEnd)) && (q.BlockEnd >= c.BlockStart));
                                //if (alrDq == true)
                                //{
                                //    Msg.Add("Year With this Period already exist.  ");
                                //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //}
                                if (c.BlockStart > c.BlockEnd)
                                {
                                    Msg.Add(" To Date should be greater than From Date.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }


                                if (db.GlobalLTCBlock.Any(o => o.BlockStart == c.BlockStart && o.BlockEnd==c.BlockEnd))
                                {
                                    Msg.Add("  From Date and To Date already exists.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }

                                //if (db.GlobalLTCBlock.Any(o => o.BlockEnd == c.BlockEnd))
                                //{
                                //    Msg.Add("  To Date Already Exists.  ");
                                //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //}

                                db.GlobalLTCBlock.Add(GlobalLTCBlock);
                                db.SaveChanges();
                                var compid = Convert.ToInt32(SessionManager.CompanyId);
                                //var oCompany = db.Company.Find(compid);
                                var oCompany = db.CompanyPayroll.Include(e => e.Company).Where(e => e.Company.Id == compid).FirstOrDefault();
                                var GlobalLTCBlockList = new List<GlobalLTCBlock>();
                                GlobalLTCBlockList.Add(GlobalLTCBlock);
                                oCompany.GlobalLTCBlock = GlobalLTCBlockList;
                                db.Entry(oCompany).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();                               
                                ts.Complete();
                            }
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "", "Data saved successfully." }, JsonRequestBehavior.AllowGet);
                            // return this.Json(new { Error = calendar.Id });
                            //return RedirectToAction("Index");
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                            //ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                            //return View(c);
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
                return View(c);
            }
        }
        
      

        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var id = Convert.ToInt32(data);
                var qurey = db.GlobalLTCBlock
                    .Where(e => e.Id == id)
                    .Select(e =>
                        new
                        {
                            FromDate_Month = e.BlockStart.Value,
                            FromDate_Year = e.BlockStart.Value,
                            ToDate_Month = e.BlockEnd.Value,
                            ToDate_Year = e.BlockEnd.Value
                        }).ToList();
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }

        }
       
       

        [HttpPost]
        public ActionResult EditSave(GlobalLTCBlock data1, int data, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    //Calendar c = db.Calendar.Find(data);
                    //var Name = form["Name_drop"] == "0" ? 0 : Convert.ToInt32(form["Name_drop"]);

                    //if (Name != 0)
                    //{
                    //    data1.Name = db.LookupValue.Find(Name);
                    //}

                    var db_data = db.GlobalLTCBlock.Where(a => a.Id == data).SingleOrDefault();
                    
                    //var alrDq = db.GlobalLTCBlock
                    //             .Any(q => ((q.BlockStart <= data1.BlockStart && q.BlockEnd <= data1.BlockEnd) || (q.BlockStart <= data1.BlockStart && q.BlockEnd >= data1.BlockEnd)) && (q.BlockEnd >= data1.BlockStart));
                    //if (alrDq == true)
                    //{
                    //    Msg.Add("Year With this Period already exist.  ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    if (data1.BlockStart > data1.BlockEnd)
                    {
                        Msg.Add(" To Date should be greater than From Date.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }


                    //if (db.GlobalLTCBlock.Any(o => o.BlockStart == data1.BlockStart))
                    //{
                    //    Msg.Add("  From Date already exists.  ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    //if (db.GlobalLTCBlock.Any(o => o.BlockEnd == data1.BlockEnd))
                    //{
                    //    Msg.Add("  To Date Already Exists.  ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    db_data.BlockEnd = data1.BlockEnd;
                    db_data.BlockStart = data1.BlockStart;
                    db_data.NoOfTimes = data1.NoOfTimes;
                    db_data.BlockYear = data1.BlockYear;

                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                    }
                    Msg.Add("  Data Saved successfully  ");
                    return Json(new Utility.JsonReturnClass { Id = db_data.Id, Val = db_data.BlockStart.Value.ToShortDateString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
        //[ValidateAntiForgeryToken]
        public ActionResult Delete(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                GlobalLTCBlock GlobalLTCBlock = db.GlobalLTCBlock.Find(data);
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                       
                        var chk = db.EmpLTCBlock.Include(q => q.GlobalLTCBlock).Where(e => e.GlobalLTCBlock.Id == data).Count();
                        if (chk > 0)
                        {
                            Msg.Add("  Data Cannont Be removed Since its used in Employee LTC Block. ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        db.Entry(GlobalLTCBlock).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { "", "Data deleted.", JsonRequestBehavior.AllowGet });
                }

                catch (DataException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                    //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
                    //ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
                    //return RedirectToAction("Index");
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
        public ActionResult ValidateForm(Calendar c, FormCollection form)
        {
            // for success
            //return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            //for error
            List<string> Msg = new List<string>();
            Msg.Add("Ok");
            Msg.Add("Okk");
            return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                var ModuleName = System.Web.HttpContext.Current.Session["ModuleType"];
                var GlobalLTCBlock = db.GlobalLTCBlock.ToList();
                                   
                IEnumerable<GlobalLTCBlock> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = GlobalLTCBlock;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE
                            .Where((e => (e.Id.ToString().Contains(gp.searchString))
                                || (e.BlockStart.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.BlockEnd.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.BlockYear.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.NoOfTimes.ToString().ToUpper().Contains(gp.searchString.ToUpper())))).ToList()
                        .Select(a => new Object[] { a.BlockStart.Value.ToShortDateString(), a.BlockEnd.Value.ToShortDateString(), a.BlockYear.ToString(), a.NoOfTimes.ToString(), a.Id });
                        // jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name != null ? a.Name.LookupVal : null), Convert.ToString(a.FromDate), Convert.ToString(a.ToDate), Convert.ToString(a.Default) }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.BlockStart.Value.ToString("dd/MM/yyyy"), a.BlockEnd.Value.ToString("dd/MM/yyyy"), Convert.ToString(a.BlockYear != null ? a.BlockYear.ToString() : "a"), Convert.ToString(a.NoOfTimes), a.Id }).ToList();
                        //jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name != null ? a.Name.LookupVal : null, a.FromDate, a.ToDate, a.Default }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = GlobalLTCBlock;
                    Func<GlobalLTCBlock, string> orderfuc = (c =>
                                                               gp.sidx == "Id" ? c.Id.ToString() :
                                                               gp.sidx == "Block start Date" ? c.BlockStart.ToString() :
                                                               gp.sidx == "Block End Date" ? c.BlockEnd.ToString() :
                                                               gp.sidx == "Block Year" ? c.BlockYear != null ? c.BlockYear.ToString() : null :
                                                               gp.sidx == "No Of Times" ? c.NoOfTimes.ToString() : "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.BlockStart.Value.ToString("dd/MM/yyyy"), a.BlockEnd.Value.ToString("dd/MM/yyyy"), Convert.ToString(a.BlockYear != null ? a.BlockYear.ToString() : "a"), Convert.ToString(a.NoOfTimes), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.BlockStart.Value.ToString("dd/MM/yyyy"), a.BlockEnd.Value.ToString("dd/MM/yyyy"), Convert.ToString(a.BlockYear != null ? a.BlockYear.ToString() : "a"), Convert.ToString(a.NoOfTimes), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  a.BlockStart, a.BlockEnd, a.BlockYear != null ? a.BlockYear.ToString() : null, a.NoOfTimes, a.Id }).ToList();
                    }
                    totalRecords = GlobalLTCBlock.Count();
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

	}
}