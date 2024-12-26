using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Collections;
using System.Reflection;
using P2BUltimate.Security;
using Attendance;

namespace P2BUltimate.Controllers.Attendance.MainController
{
    public class MachineInterfaceController : Controller
    {
        // GET: MachineInterface

        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/MachineInterface/Index.cshtml");
            //D:\P2b Ultimate source\With Svn\Latest\Bhavnagar\P2bUltimate\P2BUltimate\Views\Attendance\MainViews\MachineInterface\Index.cshtml
        }
        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {


                    // DataBaseContext db = new DataBaseContext();
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;
                    IEnumerable<MachineInterface> MachineInterface = null;
                    //if (gp.IsAutho == true)
                    //{
                    //    MachineInterface = db.MachineInterface.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                    //}
                    //else
                    //{
                    //    MachineInterface = db.MachineInterface.AsNoTracking().ToList();
                    //}
                    //['Id', 'CardCode', 'DatabaseName', 'DatabaseType', 'DateField', 'InterfaceName', 'InTimeField', 'OutTimeField', 'TableName', 'UnitNoField'];
                    MachineInterface = db.MachineInterface.Include(e => e.DatabaseType).Include(e => e.InterfaceName).AsNoTracking().ToList();
                    IEnumerable<MachineInterface> IE;
                    if (!string.IsNullOrEmpty(gp.searchField))
                    {
                        IE = MachineInterface;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.CardCode.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.DatabaseName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.DatabaseType.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.DateField.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.InterfaceName.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.InTimeField.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.OutTimeField.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.TableName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.UnitNoField.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.CardCode, a.DatabaseName, a.DatabaseType.LookupVal, a.DateField, a.InterfaceName.LookupVal, a.InTimeField, a.OutTimeField, a.TableName, a.UnitNoField, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CardCode, a.DatabaseName, a.DatabaseType.LookupVal.ToString(), a.DateField, 
                                    a.InterfaceName.LookupVal, a.InTimeField, a.OutTimeField, a.TableName, a.UnitNoField, a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = MachineInterface;
                        Func<MachineInterface, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "CardCode" ? c.CardCode :
                                gp.sidx == "DatabaseName" ? c.DatabaseName :
                                gp.sidx == "DatabaseType" ? c.DatabaseType.LookupVal :
                                gp.sidx == "DateField" ? c.DateField :
                                gp.sidx == "InterfaceName" ? c.InterfaceName.LookupVal :
                                gp.sidx == "InTimeField" ? c.InTimeField :
                                gp.sidx == "OutTimeField" ? c.OutTimeField :
                                gp.sidx == "TableName" ? c.TableName :
                                gp.sidx == "UnitNoField" ? c.UnitNoField : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.CardCode, a.DatabaseName, a.DatabaseType.LookupVal.ToString(), a.DateField, 
                                    a.InterfaceName.LookupVal, a.InTimeField, a.OutTimeField, a.TableName, a.UnitNoField, a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.CardCode, a.DatabaseName, a.DatabaseType.LookupVal.ToString(), a.DateField, 
                                    a.InterfaceName.LookupVal, a.InTimeField, a.OutTimeField, a.TableName, a.UnitNoField, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CardCode, a.DatabaseName, a.DatabaseType.LookupVal.ToString(), a.DateField, 
                                    a.InterfaceName.LookupVal, a.InTimeField, a.OutTimeField, a.TableName, a.UnitNoField, a.Id }).ToList();
                        }
                        totalRecords = MachineInterface.Count();
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
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public ActionResult GetTimingPolicyLKDetails(List<int> SkipIds)
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.TimingPolicy.Include(e => e.EarlyAction).ToList();
        //        if (SkipIds != null)
        //        {
        //            foreach (var a in SkipIds)
        //            {
        //                if (fall == null)
        //                    fall = db.TimingPolicy.Include(e => e.EarlyAction).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
        //                else
        //                    fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

        //            }
        //        }
        //        var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

        //        return Json(r, JsonRequestBehavior.AllowGet);


        //    }
        //    // return View();
        //}

        public ActionResult Create(MachineInterface COBJ, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string InterfaceName = form["InterfaceName-Drop"] == "0" ? "" : form["InterfaceName-Drop"];
                    string DatabaseType = form["DatabaseType-Drop"] == "0" ? "" : form["DatabaseType-Drop"];
                    string TimingPolicy = form["TimingPolicylist"] == "0" ? "" : form["TimingPolicylist"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    if (DatabaseType != null && DatabaseType != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "610").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(DatabaseType)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(DatabaseType));
                            COBJ.DatabaseType = val; 
                    }

                    if (InterfaceName != null && InterfaceName != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1002").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(InterfaceName)).FirstOrDefault(); // db.LookupValue.Find(int.Parse(InterfaceName));
                            COBJ.InterfaceName = val; 
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            MachineInterface ReportingStruct = new MachineInterface()
                            {
                                DatabaseType = COBJ.DatabaseType,
                                InterfaceName = COBJ.InterfaceName,
                                InTimeField = COBJ.InTimeField,
                                CardCode = COBJ.CardCode,
                                DatabaseName = COBJ.DatabaseName,
                                DateField = COBJ.DateField,
                                OutTimeField = COBJ.OutTimeField,
                                TableName = COBJ.TableName,
                                UnitNoField = COBJ.UnitNoField,
                                DBTrack = COBJ.DBTrack,

                                ServerName = COBJ.ServerName,
                                UserId = COBJ.UserId,
                                Password = COBJ.Password,

                            };
                            try
                            {
                                db.MachineInterface.Add(ReportingStruct);
                                db.SaveChanges();
                                var id = Convert.ToInt32(SessionManager.CompanyId);
                                var CompData = db.CompanyAttendance.Include(e => e.MachineInterface).Where(e => e.Company.Id == id).SingleOrDefault();
                                if (CompData.MachineInterface.Count == 0)
                                {
                                    List<MachineInterface> oMachineInterface = new List<MachineInterface>();
                                    oMachineInterface.Add(ReportingStruct);
                                    CompData.MachineInterface = oMachineInterface;
                                }
                                else if (CompData.MachineInterface.Count > 0)
                                {
                                    CompData.MachineInterface.Add(ReportingStruct);

                                }
                                db.Entry(CompData).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                                ts.Complete();
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = ReportingStruct.Id, Val = ReportingStruct.TableName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { ReportingStruct.Id, ReportingStruct.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = COBJ.Id });
                            }
                            catch (DataException /* dex */)
                            {
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
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        // return this.Json(new { msg = errorMsg });
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

        [HttpPost]
        public ActionResult Edit(int data)
        {
          
            using (DataBaseContext db = new DataBaseContext())
            {


                var Q = db.MachineInterface
                    .Include(e => e.InterfaceName)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        // JobPosition_Id = e.FuncStruct.JobPosition.Id==null? 0 : e.FuncStruct.JobPosition.Id ,
                        DatabaseType = e.DatabaseType != null ? e.DatabaseType.Id : 0,
                        InterfaceName = e.InterfaceName != null ? e.InterfaceName.Id : 0,
                        InTimeField = e.InTimeField,
                        CardCode = e.CardCode,
                        DatabaseName = e.DatabaseName,
                        DateField = e.DateField,
                        OutTimeField = e.OutTimeField,
                        TableName = e.TableName,
                        UnitNoField = e.UnitNoField,
                        DBTrack = e.DBTrack,
                        Action = e.DBTrack.Action,
                        ServerName = e.ServerName,
                        UserId = e.UserId,
                        Password = e.Password,

                    }).ToList();

                var Corp = db.MachineInterface.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        public async Task<ActionResult> EditSave(MachineInterface L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
                try
                {
                    string DatabaseType = form["DatabaseType-drop"] == "0" ? "" : form["DatabaseType-drop"];
                    string InterfaceName = form["InterfaceName-drop"] == "0" ? "" : form["InterfaceName-drop"];

                    var blog1 = db.MachineInterface
                        .Include(e => e.InterfaceName)
                        .Where(e => e.Id == data).SingleOrDefault();

                    blog1.InTimeField = L.InTimeField;
                    blog1.CardCode = L.CardCode;
                    blog1.DatabaseName = L.DatabaseName;
                    blog1.DateField = L.DateField;
                    blog1.OutTimeField = L.OutTimeField;
                    blog1.TableName = L.TableName;
                    blog1.UnitNoField = L.UnitNoField;

                    blog1.ServerName = L.ServerName;
                    blog1.UserId = L.UserId;
                    blog1.Password = L.Password;
                   
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {

                                    using (var context = new DataBaseContext())

                                        blog1.DBTrack = new DBTrack
                                        {
                                            CreatedBy = blog1.DBTrack.CreatedBy == null ? null : blog1.DBTrack.CreatedBy,
                                            CreatedOn = blog1.DBTrack.CreatedOn == null ? null : blog1.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };

                                    if (InterfaceName != null && InterfaceName != "")
                                    {
                                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1002").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(InterfaceName)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(InterfaceName));
                                        blog1.InterfaceName = val;

                                        var type = db.MachineInterface.Include(e => e.InterfaceName).Where(e => e.Id == data).SingleOrDefault();
                                        IList<MachineInterface> typedetails = null;
                                        if (type.InterfaceName != null)
                                        {
                                            typedetails = db.MachineInterface.Include(e => e.InterfaceName).Where(x => x.Id == data && x.InterfaceName.Id == type.InterfaceName.Id).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.MachineInterface.Include(e => e.InterfaceName).Where(x => x.Id == data).ToList();
                                        }
                                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                        foreach (var s in typedetails)
                                        {
                                            s.InterfaceName = blog1.InterfaceName;
                                            db.MachineInterface.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    else
                                    {
                                        var WFTypeDetails = db.MachineInterface.Include(e => e.InterfaceName).Where(x => x.Id == data).ToList();
                                        foreach (var s in WFTypeDetails)
                                        {
                                            s.InterfaceName = null;
                                            db.MachineInterface.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    if (DatabaseType != null && DatabaseType != "")
                                    {
                                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "610").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(DatabaseType)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(DatabaseType));
                                        blog1.DatabaseType = val;

                                        var type = db.MachineInterface.Include(e => e.DatabaseType).Where(e => e.Id == data).SingleOrDefault();
                                        IList<MachineInterface> typedetails = null;
                                        if (type.InterfaceName != null)
                                        {
                                            typedetails = db.MachineInterface.Include(e => e.DatabaseType).Where(x => x.Id == data && x.DatabaseType.Id == type.DatabaseType.Id).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.MachineInterface.Include(e => e.DatabaseType).Where(x => x.Id == data).ToList();
                                        }
                                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                        foreach (var s in typedetails)
                                        {
                                            s.DatabaseType = blog1.DatabaseType;
                                            db.MachineInterface.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    else
                                    {
                                        var WFTypeDetails = db.MachineInterface.Include(e => e.DatabaseType).Where(x => x.Id == data).ToList();
                                        foreach (var s in WFTypeDetails)
                                        {
                                            s.DatabaseType = null;
                                            db.MachineInterface.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    ts.Complete();
                                    Msg.Add("Record Updated");
                                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (MachineInterface)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var databaseValues = (MachineInterface)databaseEntry.ToObject();
                                blog1.RowVersion = databaseValues.RowVersion;
                            }
                        }
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new Object[] { "", "", "", JsonRequestBehavior.AllowGet });
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

        public ActionResult Delete(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    MachineInterface MachineInterface = db.MachineInterface.Where(e => e.Id == data).SingleOrDefault();
                    try
                    {
                        //var selectedValues = MachineInterface.SocialActivities;
                        //var lkValue = new HashSet<int>(RemarkConfig.SocialActivities.Select(e => e.Id));
                        //if (lkValue.Count > 0)
                        //{
                        //    return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                        //}

                        using (TransactionScope ts = new TransactionScope())
                        {
                            db.Entry(MachineInterface).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            ts.Complete();
                        }
                        // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        Msg.Add("  Data removed .  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    catch (DataException /* dex */)
                    {
                        // return this.Json(new Object[] { "", "Unable to delete. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
                        Msg.Add(" Child record exists.Cannot delete.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

    }
}