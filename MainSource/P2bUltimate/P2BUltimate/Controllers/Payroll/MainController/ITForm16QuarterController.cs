using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity.Infrastructure;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Collections;
using System.Data.Entity.Core.Objects;
using System.Reflection;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class ITForm16QuarterController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /ITForm16Quarter/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ITForm16Quarter/Index.cshtml");
        }

        [HttpPost] 
        public ActionResult Create(ITForm16Quarter ITForm, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string QuarterName = form["QuarterName_drop"] == "0" ? "" : form["QuarterName_drop"];
                string ITChallan = form["ITChallanlist"] == "0" ? "" : form["ITChallanlist"];
                List<String> Msg = new List<String>();
                try
                {

                    if (ITForm.QuarterToDate < ITForm.QuarterFromDate)
                    {
                        Msg.Add(" To Date should be greater than From Date.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new object[]{null,null,"To Date should be greater than From Date.", JsonRequestBehavior.AllowGet });
                        //return View(c);
                    }

                    if (QuarterName != null && QuarterName != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(QuarterName));
                        ITForm.QuarterName = val;
                    }

                    ITForm.ITChallan = null;
                    List<ITChallan> OBJ = new List<ITChallan>();

                    if (ITChallan != null)
                    {
                        var ids = Utility.StringIdsToListIds(ITChallan);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.ITChallan.Find(ca);
                            OBJ.Add(OBJ_val);
                            ITForm.ITChallan = OBJ;
                        }
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.ITForm16Quarter.Any(o => o.QuarterName.Id == ITForm.QuarterName.Id && o.QuarterFromDate == ITForm.QuarterFromDate))
                            {
                                Msg.Add("Record Already Exists.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            ITForm.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            ITForm16Quarter ITForm16Quarter = new ITForm16Quarter()
                            {
                                ITChallan = ITForm.ITChallan,
                                QuarterAckNo = ITForm.QuarterAckNo,
                                QuarterFromDate = ITForm.QuarterFromDate,
                                QuarterName = ITForm.QuarterName,
                                QuarterToDate = ITForm.QuarterToDate,
                                TaxableIncome = ITForm.TaxableIncome,
                                DBTrack = ITForm.DBTrack
                            };

                            db.ITForm16Quarter.Add(ITForm16Quarter);

                            db.SaveChanges();

                            var compid = Convert.ToInt32(SessionManager.CompanyId);
                            var oCompany = db.CompanyPayroll.Include(e => e.Company).Where(e => e.Company.Id == compid).FirstOrDefault();
                            var ITForm16List = new List<ITForm16Quarter>();
                            ITForm16List.Add(ITForm16Quarter);
                            oCompany.ITForm16Quarter = ITForm16List;
                            db.Entry(oCompany).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                            ts.Complete();
                            Msg.Add("Data Saved Successfully.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        //catch (DbUpdateConcurrencyException)
                        //{
                        //    return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                        //}


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
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        //return this.Json(new { msg = errorMsg });
                    }
                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }

        }

        public class ITChallan_CD
        {
            public Array ITChallan_Id { get; set; }
            public Array ITChallan_FullDetails { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            List<ITChallan_CD> return_data = new List<ITChallan_CD>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ITForm16Quarter
                    .Include(e => e.QuarterName) 
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        QuarterAckNo = e.QuarterAckNo,
                        FromDate = e.QuarterFromDate,
                        ToDate = e.QuarterToDate,
                        TaxableIncome = e.TaxableIncome,
                        QuarterName_Id = e.QuarterName.Id == null ? 0 : e.QuarterName.Id, 
                        Action = e.DBTrack.Action
                    }).ToList();



                var a = db.ITForm16Quarter.Include(e => e.ITChallan).Where(e => e.Id == data).Select(e => e.ITChallan).ToList();

                foreach (var ca in a)
                {
                    return_data.Add(
                new ITChallan_CD
                {
                    ITChallan_Id = ca.Select(e => e.Id.ToString()).ToArray(),
                    ITChallan_FullDetails = ca.Select(e => e.ChallanNo).ToArray()
                });
                }
             

                var NewReq = db.ITForm16Quarter.Find(data);
                TempData["RowVersion"] = NewReq.RowVersion;
                var Auth = NewReq.DBTrack.IsModified;
                return Json(new Object[] { Q, return_data, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(ITForm16Quarter ITForm, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //string QuarterName = form["QuarterName_drop"] == "0" ? "" : form["QuarterName_drop"];
                    string ITChallan = form["ITChallanlist"] == "0" ? "" : form["ITChallanlist"];

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;



                    var db_Data = db.ITForm16Quarter.Include(e => e.QuarterName).Include(e => e.ITChallan).Include(e=>e.CompanyPayroll).Where(e => e.Id == data).SingleOrDefault();
                    //db_Data.QuarterName = null;
                    //db_Data.ITChallan = null;
                    int QuarterName = db_Data.QuarterName.Id;
                    db_Data.QuarterName_Id = QuarterName;
                    int compid = db_Data.CompanyPayroll.Id;
                    db_Data.CompanyPayroll_Id = compid;

                    List<ITChallan> ChallanVal = new List<ITChallan>();
                    if (ITChallan != null)
                    {
                        var ids = Utility.StringIdsToListIds(ITChallan);
                        foreach (var ca in ids)
                        {
                            var Challan_val = db.ITChallan.Find(ca);
                            ChallanVal.Add(Challan_val);
                            db_Data.ITChallan = ChallanVal;
                        }
                    }
                    else
                        db_Data.ITChallan = null;

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {

                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    db.ITForm16Quarter.Attach(db_Data);
                                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_Data.RowVersion;
                                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_OBJ = db.ITForm16Quarter.Find(data);
                                    TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                    db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        ITForm16Quarter blog = blog = null;
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.ITForm16Quarter.Include(e => e.QuarterName).Where(e => e.Id == data).SingleOrDefault();
                                            originalBlogValues = context.Entry(blog).OriginalValues;
                                        }

                                        ITForm.DBTrack = new DBTrack
                                        {
                                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };
                                        ITForm16Quarter lk = new ITForm16Quarter
                                        {
                                            Id = data,
                                            QuarterFromDate = ITForm.QuarterFromDate,
                                            QuarterToDate = ITForm.QuarterToDate,
                                            TaxableIncome = ITForm.TaxableIncome,
                                            DBTrack = ITForm.DBTrack,
                                            QuarterAckNo = ITForm.QuarterAckNo,
                                            QuarterName_Id = db_Data.QuarterName_Id,
                                            CompanyPayroll_Id=db_Data.CompanyPayroll_Id 
                                        };


                                        db.ITForm16Quarter.Attach(lk);
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                        // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                        //db.SaveChanges();
                                        db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                        ////var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        ////DT_EmpMedicalInfo DT_LK = (DT_EmpMedicalInfo)obj;
                                        //////  DT_LK.Allergy = lk.Allergy.Select(e => e.Id);
                                        ////db.Create(DT_LK);
                                        db.SaveChanges();
                                        LookupValue QName = db.LookupValue.Find(QuarterName);

                                        //var EmpId = db.ITForm16QuarterEmpDetails.Where(e => e.QuarterAckNo == ITForm.QuarterAckNo && e.QuarterName.Id == QName.Id && e.QuarterFromDate == ITForm.QuarterFromDate && e.QuarterToDate == ITForm.QuarterToDate).SingleOrDefault();
                                        //EmpId = new ITForm16QuarterEmpDetails
                                        //{
                                        //    QuarterFromDate = ITForm.QuarterFromDate,
                                        //    QuarterToDate = ITForm.QuarterToDate,
                                        //    TaxableIncome = ITForm.TaxableIncome,
                                        //    DBTrack = ITForm.DBTrack,
                                        //    QuarterName = QName,
                                        //    QuarterAckNo = ITForm.QuarterAckNo 
                                        //};
                                        //db.ITForm16QuarterEmpDetails.Attach(EmpId);
                                        //db.Entry(EmpId).State = System.Data.Entity.EntityState.Modified;
                                        //db.Entry(EmpId).State = System.Data.Entity.EntityState.Detached;


                                        // await db.SaveChangesAsync();
                                        //DisplayTrackedEntities(db.ChangeTracker);
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
                                        ts.Complete();
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        // return Json(new Object[] { lk.Id, lk.PreferredHospital, "Record Updated", JsonRequestBehavior.AllowGet });
                                    }
                                }
                            }

                            catch (DbUpdateException e) { throw e; }
                            catch (DataException e) { throw e; }
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

                            //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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
        public ActionResult GetITChallanLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.ITChallan.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ITChallan.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }


                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.ChallanNo }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Calculate(ITForm16Quarter ITForm, FormCollection form)
        {
            List<string> Msg = new List<string>();
            
                try
                {
                    using (DataBaseContext db = new DataBaseContext())
                    {
                        string ITChallanlist = form["ITChallanlist"] == "0" ? "" : form["ITChallanlist"];
                        double TaxAmount = 0;
                        List<int> ids = null;
                        if (ITChallanlist != null && ITChallanlist != "0" && ITChallanlist != "false")
                        {
                            ids = Utility.StringIdsToListIds(ITChallanlist);
                        }
                        else
                        {
                            return Json(new { TaxAmount });
                        }

                        if (ids != null)
                        {

                            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                       new System.TimeSpan(0, 30, 0)))
                            {
                                try
                                {

                                    foreach (var i in ids)
                                    {
                                        var OBJ_val = db.ITChallan.Find(i);
                                        TaxAmount = TaxAmount + OBJ_val.TaxAmount;
                                    }
                                    if (TaxAmount != 0)
                                    {
                                        return Json(new { TaxAmount });
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
                                    Msg.Add(ex.ToString());
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                    }
                    Msg.Add("  Date Has Been Process  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { "", "", "Date Has Been Process" }, JsonRequestBehavior.AllowGet);
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
                    Msg.Add(ex.ToString());
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                var ITForm16 = db.ITForm16Quarter.Include(e => e.QuarterName).ToList();
                IEnumerable<ITForm16Quarter> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = ITForm16;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData=IE.Where(e=>   (e.QuarterName.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.QuarterAckNo.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.QuarterFromDate.Value.ToShortDateString().ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.QuarterToDate.Value.ToShortDateString().ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.TaxableIncome.ToString().Contains(gp.searchString))
                            || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.QuarterName.LookupVal, a.QuarterAckNo, a.QuarterFromDate.Value.ToShortDateString(), a.QuarterToDate.Value.ToShortDateString(), a.TaxableIncome, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.QuarterName != null ? a.QuarterName.LookupVal : null), a.QuarterAckNo, a.QuarterFromDate.Value.ToString("dd/MM/yyyy"), a.QuarterToDate.Value.ToString("dd/MM/yyyy"), a.TaxableIncome }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.QuarterName != null ? a.QuarterName.LookupVal : null), a.QuarterAckNo, a.QuarterFromDate.Value.ToString("dd/MM/yyyy"), a.QuarterToDate.Value.ToString("dd/MM/yyyy"), a.TaxableIncome, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = ITForm16;
                    Func<ITForm16Quarter, string> orderfuc = (c =>
                                                               gp.sidx == "Id" ? c.Id.ToString() :
                                                               gp.sidx == "QuarterName" ?  c.QuarterName.LookupVal : 
                                                               gp.sidx == "Receipt No" ? c.QuarterAckNo.ToString() :
                                                               gp.sidx == "From Date" ? c.QuarterFromDate.ToString() :
                                                               gp.sidx == "To Date" ? c.QuarterToDate.ToString() :
                                                               gp.sidx == "Tax Deducted" ? c.TaxableIncome.ToString() : "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.QuarterName != null ? a.QuarterName.LookupVal : null), a.QuarterAckNo, a.QuarterFromDate.Value.ToString("dd/MM/yyyy"), a.QuarterToDate.Value.ToString("dd/MM/yyyy"), a.TaxableIncome, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.QuarterName != null ? a.QuarterName.LookupVal : null), a.QuarterAckNo, a.QuarterFromDate.Value.ToString("dd/MM/yyyy"), a.QuarterToDate.Value.ToString("dd/MM/yyyy"), a.TaxableIncome, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.QuarterName != null ? a.QuarterName.LookupVal : null), a.QuarterAckNo, a.QuarterFromDate.Value.ToString("dd/MM/yyyy"), a.QuarterToDate.Value.ToString("dd/MM/yyyy"), a.TaxableIncome, a.Id }).ToList();
                    }
                    totalRecords = ITForm16.Count();
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