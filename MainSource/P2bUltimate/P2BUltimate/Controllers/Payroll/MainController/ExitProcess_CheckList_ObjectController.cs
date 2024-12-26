
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
using EMS;

namespace P2BUltimate.Controllers
{
    public class ExitProcess_CheckList_ObjectController : Controller
    { 
        //
        // GET: /ExitProcess_CheckList_Object/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ExitProcess_CheckList_Object/Index.cshtml");
        }


        public ActionResult Partial_ExitProcessCheckListObject()
        {
            return View("~/Views/Shared/Payroll/_ExitProcessCheckListObject.cshtml");
        }



        [HttpPost]
        public ActionResult Create(ExitProcess_CheckList_Object c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string CheckListItem = form["checklistitem_ID"] == "0" ? "" : form["checklistitem_ID"];
               

                 if (CheckListItem != null && CheckListItem != "")
                  {
                      var value = db.LookupValue.Find(int.Parse(CheckListItem));
                      c.CheckListItem = value;

                  }
                List<String> Msg = new List<String>();
                try
                {

                  
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            ExitProcess_CheckList_Object sep = new ExitProcess_CheckList_Object()
                            {
                                CheckListItemDesc = c.CheckListItemDesc,
                                CheckListItem = c.CheckListItem,
                                DBTrack = c.DBTrack
                            };

                            db.ExitProcess_CheckList_Object.Add(sep);
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
                        Msg.Add("Code Already Exists.");
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




        public ActionResult GetExitProcessCheckListObjectDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.ExitProcess_CheckList_Object.Include(e => e.CheckListItem).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ExitProcess_CheckList_Object.Include(e => e.CheckListItem.LookupVal).Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }


                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = "CheckList Item:" + ca.CheckListItem.LookupVal.ToString() }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
            // return View();
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
                IEnumerable<ExitProcess_CheckList_Object> Corporate = null;
                if (gp.IsAutho == true)
                {
                    Corporate = db.ExitProcess_CheckList_Object.Include(e=>e.CheckListItem).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Corporate = db.ExitProcess_CheckList_Object
                                .Include(e=>e.CheckListItem).AsNoTracking().ToList();
                }

                IEnumerable<ExitProcess_CheckList_Object> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Corporate;
                    if (gp.searchOper.Equals("eq"))
                    {

                        jsonData = IE.Where(e => (e.CheckListItem != null ? e.CheckListItem.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                              || (e.CheckListItemDesc != null ? e.CheckListItemDesc.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                              //|| (e.ProcessConfigName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                              || (e.Id.ToString().Contains(gp.searchString))
                              ).Select(a => new Object[] { a.CheckListItem != null ? a.CheckListItem.LookupVal : "", a.CheckListItemDesc != null ? a.CheckListItemDesc : "", a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CheckListItem.LookupVal, a.CheckListItemDesc, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Corporate;
                    Func<ExitProcess_CheckList_Object, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "CheckListItem" ? c.CheckListItem != null ? c.CheckListItem.LookupVal : "" :
                                         gp.sidx == "CheckListItemDesc" ? c.CheckListItemDesc.ToString() :
                                          "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CheckListItem != null ? a.CheckListItem.LookupVal : "", a.CheckListItemDesc, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc); 
                        jsonData = IE.Select(a => new Object[] { a.CheckListItem != null ? a.CheckListItem.LookupVal : "", a.CheckListItemDesc, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CheckListItem != null ? a.CheckListItemDesc: "", a.Id }).ToList();
                    }
                    totalRecords = Corporate.Count();
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

        [HttpPost]

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ExitProcess_CheckList_Object
                 .Include(e => e.CheckListItem)
                 .Where(e => e.Id == data).Select
                 (e => new
                 {
                     checklistitemdesc = e.CheckListItemDesc,
                     checklistitem_ID = e.CheckListItem.Id,
                 }).ToList();

                var Corp = db.ExitProcess_CheckList_Object.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, Auth, JsonRequestBehavior.AllowGet });

            }

        }

        [HttpPost]
        public async Task<ActionResult> EditSave(ExitProcess_CheckList_Object c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string CheckListItem = form["checklistitem_ID"] == "0" ? "" : form["checklistitem_ID"];
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;



                    if (CheckListItem != null)
                    {
                        if (CheckListItem != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(CheckListItem));
                            c.CheckListItem = val;

                            var type = db.ExitProcess_CheckList_Object.Include(e => e.CheckListItem).Where(e => e.Id == data).SingleOrDefault();
                            IList<ExitProcess_CheckList_Object> typedetails = null;
                            if (type.CheckListItem != null)
                            {
                                typedetails = db.ExitProcess_CheckList_Object.Where(x => x.CheckListItem.Id == type.CheckListItem.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.ExitProcess_CheckList_Object.Where(x => x.Id == data).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.CheckListItem = c.CheckListItem;
                                db.ExitProcess_CheckList_Object.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                   

                   
                    if (Auth == false)
                    {
                       
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    ExitProcess_CheckList_Object blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.ExitProcess_CheckList_Object.Where(e => e.Id == data).Include(e => e.CheckListItem)
                                                                .AsNoTracking().SingleOrDefault();
                                        //originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    var m1 = db.ExitProcess_CheckList_Object.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.ExitProcess_CheckList_Object.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    //int a = EditS(Category, PromoPolicy, data, c, c.DBTrack);
                                    var CurCorp = db.ExitProcess_CheckList_Object.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {


                                        ExitProcess_CheckList_Object corp = new ExitProcess_CheckList_Object()
                                        {
                                            CheckListItem = c.CheckListItem,
                                            CheckListItemDesc=c.CheckListItemDesc,
                                            Id = data,
                                            DBTrack = c.DBTrack
                                        };

                                        db.ExitProcess_CheckList_Object.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // return 1;
                                    }

                                    using (var context = new DataBaseContext())
                                    {
                                        //var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        //DT_ExitProcess_CheckList_Object DT_Corp = (DT_IncrActivity)obj;
                                        //DT_Corp.IncrList_Id = blog.IncrList == null ? 0 : blog.IncrList.Id;
                                        //DT_Corp.IncrPolicy_Id = blog.IncrPolicy == null ? 0 : blog.IncrPolicy.Id;

                                        //db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.CheckListItemDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { c.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (PromoActivity)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (PromoActivity)databaseEntry.ToObject();       
                                    c.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
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
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    ExitProcess_CheckList_Object exitprocesschecklistobj = db.ExitProcess_CheckList_Object.Include(e => e.CheckListItem)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    db.Entry(exitprocesschecklistobj).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                    //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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