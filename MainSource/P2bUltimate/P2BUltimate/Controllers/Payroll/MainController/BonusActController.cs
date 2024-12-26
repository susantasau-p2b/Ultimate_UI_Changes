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
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class BonusActController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/BonusAct/Index.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(BonusAct B, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int company_Id;
                    string BonusWageslist = form["BonusWageslist"] == "0" ? "" : form["BonusWageslist"];
                    string BonusCalendarlist = form["BonusCalendarlist"] == "0" ? "" : form["BonusCalendarlist"];
                    string WantToGiveExgratia = form["WantToGiveExgratia"] == "0" ? "" : form["WantToGiveExgratia"];
                    string ApplicationForBonus = form["ApplicationForBonus"] == "0" ? "" : form["ApplicationForBonus"];
                    B.WantToGiveExgratia = Convert.ToBoolean(WantToGiveExgratia);
                    B.ApplicationForBonus = Convert.ToBoolean(ApplicationForBonus);

                    company_Id = Convert.ToInt32(Session["CompId"]);
                    var companypayroll = new CompanyPayroll();
                    companypayroll = db.CompanyPayroll.Include(e => e.BonusAct).Where(e => e.Company.Id == company_Id).SingleOrDefault();
                    if (BonusWageslist != null && BonusWageslist != "")
                    {
                        var value = db.Wages.Find(int.Parse(BonusWageslist));
                        B.BonusWages = value;
                    }
                    else
                    {
                        Msg.Add(" kindly provide data for Bonus Wages");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (BonusCalendarlist != null && BonusCalendarlist != "")
                    {
                        var value = db.Calendar.Find(int.Parse(BonusCalendarlist));
                        B.BonusCalendar = value;
                    }
                    else
                    {
                        Msg.Add(" kindly provide data for Bonus Calendar");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (db.BonusAct.Any(e => (e.BonusCalendar.FromDate == B.BonusCalendar.FromDate && e.BonusCalendar.ToDate == B.BonusCalendar.ToDate)))
                    {
                        Msg.Add("  BonusAct For This Year Already Exists. ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new Object[] { "", "", "BonusAct For This Year Already Exists.", JsonRequestBehavior.AllowGet });
                    }

                    //string Values = form["PayScaleArealist"];

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            B.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            BonusAct BonusAct = new BonusAct()
                            {
                                WantToGiveExgratia = B.WantToGiveExgratia,
                                ApplicationForBonus = B.ApplicationForBonus,
                                BonusName = B.BonusName,
                                MinimumBonusAmount = B.MinimumBonusAmount,
                                MaximumBonus = B.MaximumBonus,
                                MinPercentage = B.MinPercentage,
                                MaxPercentage = B.MaxPercentage,
                                MinimumWorkingDays = B.MinimumWorkingDays,
                                QualiAmount = B.QualiAmount,
                                BonusWages = B.BonusWages,
                                DBTrack = B.DBTrack,
                                BonusCalendar = B.BonusCalendar
                            };
                            try
                            {

                                db.BonusAct.Add(BonusAct);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, B.DBTrack);
                                // DT_OBJ = (DT_PayScale)rtn_Obj;
                                //DT_OBJ.PayScaleType_Id = NOBJ.PayScaleType == null ? 0 : NOBJ.PayScaleType.Id;
                                //db.Create(DT_OBJ);
                                db.SaveChanges();
                                List<BonusAct> BonusAct_list = new List<BonusAct>();
                                BonusAct_list.AddRange(companypayroll.BonusAct);
                                BonusAct_list.Add(BonusAct);
                                if (companypayroll != null)
                                {
                                    companypayroll.BonusAct = BonusAct_list;
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Detached;
                                }
                                ts.Complete();
                                //   return this.Json(new Object[] { BonusAct.Id, BonusAct.BonusName, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = BonusAct.Id, Val = BonusAct.BonusName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = B.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

                        // Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

        public class P2BGridData
        {
            public int Id { get; set; }
            public string BonusName { get; set; }
            public double MaximumBonus { get; set; }
            public double MinimumBonusAmount { get; set; }
            public double MaxPercentage { get; set; }

        }


        [HttpPost]
        public ActionResult GetCalendarDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.Calendar.Where(e => (e.Name.LookupVal.ToUpper() == "BONUSYEAR") && e.Default == true).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Calendar.Where(e => !e.Id.ToString().Contains(a.ToString()) && e.Name.LookupVal.ToUpper() == "BONUSYEAR").ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> salheadList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;


                    //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                    //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);

                    var BindCompList = db.CompanyPayroll.Include(e => e.BonusAct).Where(e => e.Company.Id == company_Id).ToList();

                    foreach (var z in BindCompList)
                    {
                        if (z.BonusAct != null)
                        {

                            foreach (var B in z.BonusAct)
                            {
                                //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
                                view = new P2BGridData()
                                {
                                    Id = B.Id,
                                    BonusName = B.BonusName,
                                    MaximumBonus = B.MaximumBonus,
                                    MaxPercentage = B.MaxPercentage,
                                    MinimumBonusAmount = B.MinimumBonusAmount
                                };
                                model.Add(view);

                            }
                        }

                    }

                    salheadList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = salheadList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            //jsonData = IE.Select(a => new { a.Id, a.BonusName, a.MaximumBonus, a.MaxPercentage, a.MinimumBonusAmount }).Where(e => (e.Id.ToString().Contains(gp.searchString)) ||(e.BonusName.ToString().Contains(gp.searchString)) || (e.MaximumBonus.ToString().Contains(gp.searchString)) || (e.MaxPercentage.ToString().Contains(gp.searchString))||(e.MinimumBonusAmount.ToString().Contains(gp.searchString))  ).ToList();
                            jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                                || (e.BonusName.ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.MaximumBonus.ToString().Contains(gp.searchString))
                                // || (e.MaxPercentage.ToString().Contains(gp.searchString))
                                //  || (e.MinimumBonusAmount.ToString().Contains(gp.searchString))
                                ).Select(a => new { a.BonusName, a.MaximumBonus, a.Id, a.MaxPercentage, a.MinimumBonusAmount }).ToList();
                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.BonusName), Convert.ToString(a.MaximumBonus), a.Id, Convert.ToString(a.MaxPercentage), Convert.ToString(a.MinimumBonusAmount) }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = salheadList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Code" ? c.BonusName.ToString() :
                                             gp.sidx == "MaxBonus" ? c.MaximumBonus.ToString() : ""

                                            );
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] {  Convert.ToString(a.BonusName), Convert.ToString(a.MaximumBonus), a.Id ,Convert.ToString(a.MaxPercentage), Convert.ToString(a.MinimumBonusAmount) }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.BonusName), Convert.ToString(a.MaximumBonus), a.Id, Convert.ToString(a.MaxPercentage), Convert.ToString(a.MinimumBonusAmount) }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.BonusName), Convert.ToString(a.MaximumBonus), a.Id, Convert.ToString(a.MaxPercentage), Convert.ToString(a.MinimumBonusAmount) }).ToList();
                        }
                        totalRecords = salheadList.Count();
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

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.BonusAct
                    .Include(e => e.BonusWages)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        WantToGiveExgratia = e.WantToGiveExgratia,
                        ApplicationForBonus = e.ApplicationForBonus,
                        BonusName = e.BonusName,
                        MinimumBonusAmount = e.MinimumBonusAmount,
                        MaximumBonus = e.MaximumBonus,
                        MinPercentage = e.MinPercentage,
                        MaxPercentage = e.MaxPercentage,
                        MinimumWorkingDays = e.MinimumWorkingDays,
                        QualiAmount = e.QualiAmount,
                        BonusWages_Id = e.BonusWages == null ? 0 : e.BonusWages.Id,
                        BonusWages_FullDetails = e.BonusWages.FullDetails,
                        Action = e.DBTrack.Action,
                        BonusCalendar_Id = e.BonusCalendar == null ? 0 : e.BonusCalendar.Id,
                        BonusCalendar_FullDetails = e.BonusCalendar.FullDetails,
                    }).ToList();



                var Corp = db.BonusAct.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(BonusAct B, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string BonusCalendarlist = form["BonusCalendarlist"] == "0" ? "" : form["BonusCalendarlist"];
                    string BonusWageslist = form["BonusWageslist"] == "0" ? "" : form["BonusWageslist"];
                    string WantToGiveExgratia = form["WantToGiveExgratia"] == "0" ? "" : form["WantToGiveExgratia"];
                    string ApplicationForBonus = form["ApplicationForBonus"] == "0" ? "" : form["ApplicationForBonus"];
                    B.WantToGiveExgratia = Convert.ToBoolean(WantToGiveExgratia);
                    B.ApplicationForBonus = Convert.ToBoolean(ApplicationForBonus);

                    B.BonusWages_Id = BonusWageslist != null && BonusWageslist != "" ? int.Parse(BonusWageslist) : 0;
                    B.BonusCalendar_Id = BonusCalendarlist != null && BonusCalendarlist != "" ? int.Parse(BonusCalendarlist) : 0;
                     

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    if (BonusWageslist==null)
                    {

                        Msg.Add(" kindly provide data for Bonus Wages");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (BonusCalendarlist == null)
                    {
                        Msg.Add(" kindly provide data for Bonus Calendar");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    BonusAct blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                     
                                        blog = db.BonusAct.Where(e => e.Id == data).Include(e => e.BonusWages)
                                                                .SingleOrDefault();
                                        originalBlogValues = db.Entry(blog).OriginalValues;
                                    

                                    B.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                     

                                    var CurCorp = db.BonusAct.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion; 
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                         
                                            CurCorp.ApplicationForBonus = B.ApplicationForBonus;
                                            CurCorp.BonusName = B.BonusName;
                                            CurCorp.MaximumBonus = B.MaximumBonus;
                                            CurCorp.MaxPercentage = B.MaxPercentage;
                                            CurCorp.MinimumBonusAmount = B.MinimumBonusAmount;
                                            CurCorp.MinimumWorkingDays = B.MinimumWorkingDays;
                                            CurCorp.MinPercentage = B.MinPercentage;
                                            CurCorp.QualiAmount = B.QualiAmount;
                                            CurCorp.WantToGiveExgratia = B.WantToGiveExgratia;
                                            CurCorp.Id = data;
                                            CurCorp.DBTrack = B.DBTrack;
                                            CurCorp.BonusCalendar_Id = B.BonusCalendar_Id;
                                            CurCorp.BonusWages_Id = B.BonusWages_Id;
                                       
                                        db.Entry(CurCorp).State = System.Data.Entity.EntityState.Modified;
                                         db.SaveChanges();
                                    }
                                    
                                       
                                     
                                    ts.Complete();
                                    //  return Json(new Object[] { B.Id, B.BonusName, "Record Updated", JsonRequestBehavior.AllowGet });
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = B.Id, Val = B.BonusName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }

                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (BonusAct)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var databaseValues = (BonusAct)databaseEntry.ToObject();
                                    B.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //  return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            BonusAct blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            BonusAct Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.BonusAct.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            B.DBTrack = new DBTrack
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

                            BonusAct corp = new BonusAct()
                            {
                                ApplicationForBonus = B.ApplicationForBonus,
                                BonusName = B.BonusName,
                                MaximumBonus = B.MaximumBonus,
                                MaxPercentage = B.MaxPercentage,
                                MinimumBonusAmount = B.MinimumBonusAmount,
                                MinimumWorkingDays = B.MinimumWorkingDays,
                                MinPercentage = B.MinPercentage,
                                QualiAmount = B.QualiAmount,
                                WantToGiveExgratia = B.WantToGiveExgratia,
                                Id = data,
                                DBTrack = B.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, corp, "BonusAct", B.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.BonusAct.Where(e => e.Id == data).Include(e => e.BonusWages)
                                      .SingleOrDefault();
                                //DT_LvCreditPolicy DT_Corp = (DT_LvCreditPolicy)obj;
                                //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;

                                //db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = B.DBTrack;
                            db.BonusAct.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            //  return Json(new Object[] { blog.Id, B.BonusWages, "Record Updated", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = B.BonusWages.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }

                    }
                    return View();
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
        public ActionResult Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    BonusAct bonusact = db.BonusAct.Include(e => e.BonusWages).Include(e => e.BonusCalendar).Where(e => e.Id == data).SingleOrDefault();
                    if (bonusact.BonusCalendar.Id != null)
                    {
                        Msg.Add(" Child record exist.cannot remove it.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (bonusact.BonusWages.Id != null)
                    {
                        Msg.Add(" Child record exist.cannot remove it.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    var id = int.Parse(Session["CompId"].ToString());
                    var companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == id).SingleOrDefault();
                    companypayroll.BonusAct.Where(e => e.Id == bonusact.Id);
                    companypayroll.BonusAct = null;
                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    //  return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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