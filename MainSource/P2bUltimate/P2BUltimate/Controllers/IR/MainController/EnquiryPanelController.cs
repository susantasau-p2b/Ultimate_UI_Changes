using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IR;
using P2b.Global;
using System.Data.Entity.Infrastructure;
using System.Text;
using System.Transactions;
using System.Data;
using System.Data.Entity;
using P2BUltimate.Models;
using P2BUltimate.App_Start;
using System.Threading.Tasks;
using P2BUltimate.Security;
//using System.Web.Script.Serialization;
using Payroll;
using System.ComponentModel.DataAnnotations;
using Microsoft.Ajax.Utilities;
using DocumentFormat.OpenXml.Wordprocessing;
namespace P2BUltimate.Controllers.IR.MainController
{
    public class EnquiryPanelController : Controller
    {
        //
        // GET: /EnquiryPanel/
        public ActionResult Index()
        {
            return View("~/Views/IR/MainViews/EnquiryPanel/Index.cshtml");
        }
        public class returnDataClass
        {
            public int Id { get; set; }
            public string FullDetails { get; set; }

        }

        public ActionResult GetExitEmployee(List<int> SkipIds, string expression)
        {
            using (DataBaseContext db = new DataBaseContext())
            {


                switch (expression)
                {

                    case "External":

                        var EmpExternal = db.EnquiryPanelExternal.ToList();

                        if (SkipIds != null)
                        {
                            foreach (var a in SkipIds)
                            {
                                if (EmpExternal == null)
                                    EmpExternal = db.EnquiryPanelExternal.Where(e => e.Id != a).ToList();
                                else
                                    EmpExternal = EmpExternal.Where(e => e.Id != a).ToList();
                            }
                        }
                        var r3 = (from ca in EmpExternal select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                        return Json(r3, JsonRequestBehavior.AllowGet);

                        break;


                    case "Both":

                        var EmpExternalboth = db.EnquiryPanelExternal.ToList();

                        if (SkipIds != null)
                        {
                            foreach (var a in SkipIds)
                            {
                                if (EmpExternalboth == null)
                                    EmpExternalboth = db.EnquiryPanelExternal.Where(e => e.Id != a).ToList();
                                else
                                    EmpExternalboth = EmpExternalboth.Where(e => e.Id != a).ToList();
                            }
                        }
                        var r4 = (from ca in EmpExternalboth select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                        return Json(r4, JsonRequestBehavior.AllowGet);

                        break;
                }



            }
            return null;
        }


        public ActionResult GetEmployeeDetails(List<int> SkipIds, string expression)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                switch (expression)
                {

                    case "Internal":

                        var EmpInternal = db.Employee.Include(e => e.EmpName).ToList();
                        if (SkipIds != null)
                        {
                            foreach (var a in SkipIds)
                            {
                                if (EmpInternal == null)
                                    EmpInternal = db.Employee.Where(e => e.Id != a).ToList();
                                else
                                    EmpInternal = EmpInternal.Where(e => e.Id != a).ToList();
                            }
                        }
                        var r1 = (from ca in EmpInternal select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                        return Json(r1, JsonRequestBehavior.AllowGet);

                        break;

                    case "Both":

                        var EmpInternalboth = db.Employee.Include(e => e.EmpName).ToList();
                        if (SkipIds != null)
                        {
                            foreach (var a in SkipIds)
                            {
                                if (EmpInternalboth == null)
                                    EmpInternalboth = db.Employee.Where(e => e.Id != a).ToList();
                                else
                                    EmpInternalboth = EmpInternalboth.Where(e => e.Id != a).ToList();
                            }
                        }
                        var r2 = (from ca in EmpInternalboth select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                        return Json(r2, JsonRequestBehavior.AllowGet);

                        break;
                }



            }
            return null;
        }


        public List<string> ValidateObj(Object obj)
        {
            var errorList = new List<String>();
            var context = new ValidationContext(obj, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(obj, context, results, true);
            if (!isValid)
            {
                foreach (var validationResult in results)
                {
                    errorList.Add(validationResult.ErrorMessage);
                }
                return errorList;
            }
            else
            {
                return errorList;
            }
        }
        public ActionResult Create(EnquiryPanel c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int comp_Id = 0;
                    comp_Id = Convert.ToInt32(Session["CompId"]);
                    var Company = new Company();
                    Company = db.Company.Where(e => e.Id == comp_Id).SingleOrDefault();
                    string PanelName = form["PanelName"] == "0" ? "" : form["PanelName"];
                    string EnquiryRepresentativeTypelist = form["EnquiryRepresentativeTypelist"] == "0" ? "" : form["EnquiryRepresentativeTypelist"];
                    string EnquiryPanelType = form["EnquiryPanelTypelist"] == "" ? null : form["EnquiryPanelTypelist"];
                    string Employee = form["EmployeeList"] == "" ? null : form["EmployeeList"];
                    string ExitEmployee = form["ExtEmployeeList"] == "" ? null : form["ExtEmployeeList"];
                    var Emplist = new List<Witness>();
                    string enqtype = "";
                    if (EnquiryPanelType != null && EnquiryPanelType != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(EnquiryPanelType));
                        c.EnquiryPanelType = val;

                    }
                    if (EnquiryRepresentativeTypelist != null && EnquiryRepresentativeTypelist != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(EnquiryRepresentativeTypelist));
                        c.EnquiryRepresentativeType = val;
                        enqtype = val.LookupVal.ToUpper();
                    }

                    if (enqtype == "INTERNAL")
                    {
                        if (Employee != null)
                        {
                            var ids = Utility.StringIdsToListIds(Employee);
                            var WitnessEmplist = new List<Witness>();
                            foreach (var item in ids)
                            {

                                int WitnessEmpListid = Convert.ToInt32(item);
                                var val = db.EmployeeIR.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id == WitnessEmpListid).SingleOrDefault();
                                if (val != null)
                                {
                                    c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };
                                    Witness objwit = new Witness()
                                    {
                                        WitnessEmp = val,
                                        DBTrack = c.DBTrack

                                    };

                                    WitnessEmplist.Add(objwit);
                                }
                            }
                            c.Witness = WitnessEmplist;
                        }
                        else
                        {
                            Msg.Add(" Please select at least a record! ");

                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                   
                     if (enqtype == "EXTERNAL")
                    {
                        if (ExitEmployee != null)
                        {
                            var ids = Utility.StringIdsToListIds(ExitEmployee);
                            var ExtEmplist = new List<Witness>();
                            foreach (var item in ids)
                            {

                                int ExternalEmpListid = Convert.ToInt32(item);
                                var val = db.EnquiryPanelExternal.Where(e => e.Id == ExternalEmpListid).SingleOrDefault();
                                if (val != null)
                                {
                                    c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };
                                    Witness objext = new Witness()
                                    {
                                        WitnessExt = val,
                                        DBTrack = c.DBTrack

                                    };

                                    ExtEmplist.Add(objext);
                                }
                            }
                            c.Witness = ExtEmplist;
                        }
                        else
                        {
                            Msg.Add(" Please select at least a record! ");

                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }


                     if (enqtype == "BOTH")
                     {
                         if (Employee != null && ExitEmployee != null)
                         {
                             if (Employee != null)
                             {
                                 var ids = Utility.StringIdsToListIds(Employee);
                                 foreach (var item in ids)
                                 {
                                     int EmployeeListid = Convert.ToInt32(item);
                                     var val = db.EmployeeIR.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id == EmployeeListid).SingleOrDefault();

                                     if (val != null)
                                     {
                                         c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };
                                         Witness objwit = new Witness()
                                         {
                                             WitnessEmp = val,
                                             DBTrack = c.DBTrack

                                         };

                                         Emplist.Add(objwit);
                                     }
                                 }

                             }


                             if (ExitEmployee != null)
                             {
                                 var ids = Utility.StringIdsToListIds(ExitEmployee);

                                 foreach (var item in ids)
                                 {

                                     int ExternalEmpListid = Convert.ToInt32(item);
                                     var val = db.EnquiryPanelExternal.Where(e => e.Id == ExternalEmpListid).SingleOrDefault();
                                     if (val != null)
                                     {
                                         c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };
                                         Witness objext = new Witness()
                                         {
                                             WitnessExt = val,
                                             DBTrack = c.DBTrack

                                         };

                                         Emplist.Add(objext);
                                     }
                                 }
                             }
                             c.Witness = Emplist;
                         }
                         else
                         {
                             Msg.Add(" Please select at least a record! ");

                             return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                         }

                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            EnquiryPanel EnquiryPanel = new EnquiryPanel()
                            {
                                PanelName = c.PanelName,
                                Witness = c.Witness,

                                EnquiryRepresentativeType = c.EnquiryRepresentativeType,
                                EnquiryPanelType = c.EnquiryPanelType,
                                DBTrack = c.DBTrack
                            };


                            var EnquiryPanelValidation = ValidateObj(EnquiryPanel);
                            if (EnquiryPanelValidation.Count > 0)
                            {
                                foreach (var item in EnquiryPanelValidation)
                                {

                                    Msg.Add("EnquiryPanel" + item);
                                }
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            db.EnquiryPanel.Add(EnquiryPanel);

                            try
                            {

                                db.SaveChanges();

                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = EnquiryPanel.Id, Val = EnquiryPanel.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                var EnquiryPanel = new List<EnquiryPanel>();
                EnquiryPanel = db.EnquiryPanel
                    .Include(e => e.EnquiryPanelType)
                                   .ToList();
                IEnumerable<EnquiryPanel> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = EnquiryPanel;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE
                         .Where((e => (e.PanelName.ToString().ToUpper().Contains(gp.searchString))

                                || (e.EnquiryPanelType.LookupVal.ToString().ToUpper().Contains(gp.searchString.ToUpper()))

                                  || (e.Id.ToString().Contains(gp.searchString))
                                 )).ToList()
                        .Select(a => new Object[] { a.EnquiryPanelType.LookupVal.ToString(), a.PanelName, a.Id });
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EnquiryPanelType.LookupVal.ToString(), a.PanelName, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();


                }
                else
                {
                    IE = EnquiryPanel;
                    Func<EnquiryPanel, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "PanelName" ? c.PanelName.ToString() :
                            gp.sidx == "EnquiryPanelType" ? c.EnquiryPanelType.LookupVal.ToString() : "");


                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EnquiryPanelType.LookupVal.ToString(), a.PanelName, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EnquiryPanelType.LookupVal.ToString(), a.PanelName, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EnquiryPanelType.LookupVal.ToString(), a.PanelName, a.Id }).ToList();
                    }
                    totalRecords = EnquiryPanel.Count();
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
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    EnquiryPanel corporates = db.EnquiryPanel.Include(e => e.Witness)
                                                .Include(e => e.Witness.Select(t => t.WitnessEmp))
                                                .Include(e => e.Witness.Select(t => t.WitnessExt))
                                                .Include(e => e.Witness.Select(t => t.WitnessEmp.Employee))
                                                .Include(e => e.Witness.Select(t => t.WitnessEmp.Employee.EmpName))
                                                .Include(e=>e.EnquiryPanelType).Include(e=>e.EnquiryRepresentativeType)
                                                .Where(e => e.Id == data).SingleOrDefault();


                    if (corporates.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? true : false
                            };

                            await db.SaveChangesAsync();
                            ts.Complete();

                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {


                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? false : false//,

                            };



                            db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;


                            await db.SaveChangesAsync();



                            ts.Complete();

                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {

                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

        public class WitnessEditClass
        {
            public string WitnessEmp_Id { get; set; }
            public string WitnessEmp_Val { get; set; }
            public string WitnessExt_Id { get; set; }
            public string WitnessExt_Val { get; set; }
        }

        public ActionResult Edit(int data)
        {
            List<WitnessEditClass> oreturnEditClass = new List<WitnessEditClass>();


            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.EnquiryPanel
                    .Include(e => e.EnquiryPanelType)
                    .Include(e => e.EnquiryRepresentativeType)
                                 .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        Name = e.PanelName,
                        EnquiryPanelType_Id = e.EnquiryPanelType.Id == null ? 0 : e.EnquiryPanelType.Id,
                        EnquiryRepresentativeType_Id = e.EnquiryRepresentativeType.Id == null ? 0 : e.EnquiryRepresentativeType.Id
                    }).ToList();

               
                var return_data = db.EnquiryPanel.Include(e => e.Witness)
                    .Include(e => e.Witness.Select(t => t.WitnessEmp))
                    .Include(e => e.Witness.Select(t => t.WitnessExt))
                    .Include(e => e.Witness.Select(t => t.WitnessEmp.Employee))
                    .Include(e => e.Witness.Select(t => t.WitnessEmp.Employee.EmpName))
                    .Where(e => e.Id == data && e.Witness.Count > 0).SingleOrDefault();



                if (return_data != null && return_data.Witness != null)
                {
                    var WitnessList = return_data.Witness.ToList();
                    if (WitnessList.Count()>0)
                    {
                        foreach (var item in WitnessList)
                        {
                            if (item.WitnessEmp != null)
                            {
                                oreturnEditClass.Add(new WitnessEditClass
                                {
                                    WitnessEmp_Id = item.WitnessEmp.Id.ToString(),
                                    WitnessEmp_Val = item.WitnessEmp != null && item.WitnessEmp.Employee != null && item.WitnessEmp.Employee.EmpName != null ? item.WitnessEmp.Employee.EmpName.FullNameFML : ""
                                });
                            }

                            if (item.WitnessExt != null)
                            {
                                oreturnEditClass.Add(new WitnessEditClass
                                {
                                    WitnessExt_Id = item.WitnessExt.Id.ToString(),
                                    WitnessExt_Val = item.WitnessExt != null ? item.WitnessExt.Name : ""
                                });
                            }
                        }
                    }
                }
                
                
                return Json(new Object[] { returndata, oreturnEditClass, JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]

        public async Task<ActionResult> EditSave(EnquiryPanel c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    List<Witness> WitnessList = new List<Witness>();
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    string EnquiryPanelType = form["EnquiryPanelTypelist"] == "0" ? "" : form["EnquiryPanelTypelist"];
                    string EnquiryRepresentativeTypelist = form["EnquiryRepresentativeTypelist"] == "0" ? "" : form["EnquiryRepresentativeTypelist"];                   
                    string ddlEmployee = form["EmployeeList"] == "" ? null : form["EmployeeList"];
                    string ExitEmployee = form["ExtEmployeeList"] == "" ? null : form["ExtEmployeeList"];
                   
                    var enqtypesedit = "";

                    if (EnquiryPanelType != null)
                    {
                        if (EnquiryPanelType != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(EnquiryPanelType));
                            c.EnquiryPanelType = val;

                            var type = db.EnquiryPanel.Include(e => e.EnquiryPanelType).Where(e => e.Id == data).SingleOrDefault();
                            IList<EnquiryPanel> typedetails = null;
                            if (type.EnquiryPanelType != null)
                            {
                                typedetails = db.EnquiryPanel.Where(x => x.EnquiryPanelType.Id == type.EnquiryPanelType.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.EnquiryPanel.Where(x => x.Id == data).ToList();
                            }

                            foreach (var s in typedetails)
                            {
                                s.EnquiryPanelType = c.EnquiryPanelType;
                                db.EnquiryPanel.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        else
                        {
                            var EnquiryPanelTypeDetails = db.EnquiryPanel.Include(e => e.EnquiryPanelType).Where(x => x.Id == data).ToList();
                            foreach (var s in EnquiryPanelTypeDetails)
                            {
                                s.EnquiryPanelType = null;
                                db.EnquiryPanel.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }



                    if (EnquiryRepresentativeTypelist != null)
                    {
                        if (EnquiryRepresentativeTypelist != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(EnquiryRepresentativeTypelist));
                            c.EnquiryRepresentativeType = val;
                            enqtypesedit = val.LookupVal.ToUpper();

                            var type = db.EnquiryPanel.Include(e => e.EnquiryRepresentativeType).Where(e => e.Id == data).SingleOrDefault();
                            IList<EnquiryPanel> typedetails = null;
                            if (type.EnquiryRepresentativeType != null)
                            {
                                typedetails = db.EnquiryPanel.Where(x => x.EnquiryRepresentativeType.Id == type.EnquiryRepresentativeType.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.EnquiryPanel.Where(x => x.Id == data).ToList();
                            }

                            foreach (var s in typedetails)
                            {
                                s.EnquiryRepresentativeType = c.EnquiryRepresentativeType;
                                db.EnquiryPanel.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        else
                        {
                            var EnquiryPanelTypeDetails = db.EnquiryPanel.Include(e => e.EnquiryRepresentativeType).Where(x => x.Id == data).ToList();
                            foreach (var s in EnquiryPanelTypeDetails)
                            {
                                s.EnquiryRepresentativeType = null;
                                db.EnquiryPanel.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }


                    var db_data = db.EnquiryPanel.Include(e => e.Witness).Include(e => e.Witness.Select(t => t.WitnessEmp))
                                    .Include(e => e.Witness.Select(t => t.WitnessExt))
                                    .Include(e => e.Witness.Select(t => t.WitnessEmp.Employee))
                                    .Include(e => e.Witness.Select(t => t.WitnessEmp.Employee.EmpName)).Where(e => e.Id == data).SingleOrDefault();

                    if (enqtypesedit == "INTERNAL")
                    {
                        if (ddlEmployee != null)
                        {
                            var ids = Utility.StringIdsToListIds(ddlEmployee);
                            foreach (var ca in ids)
                            {
                                int empid = Convert.ToInt32(ca);
                                var empIR_val = db.EmployeeIR.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id == empid).SingleOrDefault();
                                if (empIR_val != null)
                                {
                                    c.DBTrack = new DBTrack { Action = "M", CreatedBy = SessionManager.UserName, IsModified = true, ModifiedOn = DateTime.Now };
                                    Witness Objwit = new Witness()
                                    {
                                        WitnessEmp = empIR_val,
                                        DBTrack = c.DBTrack
                                    };
                                    WitnessList.Add(Objwit);
                                }

                            }
                            db_data.Witness = WitnessList;
                        }
                        else
                        {
                            Msg.Add(" Please select at least a record! ");

                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                        
                    }

                    if (enqtypesedit == "EXTERNAL")
                    {                        
                        if (ExitEmployee != null)
                        {
                            var ids = Utility.StringIdsToListIds(ExitEmployee);
                            foreach (var ca in ids)
                            {
                                int empid = Convert.ToInt32(ca);
                                var empExt_val = db.EnquiryPanelExternal.Where(e => e.Id == empid).SingleOrDefault();
                                if (empExt_val != null)
                                {
                                    c.DBTrack = new DBTrack { Action = "M", CreatedBy = SessionManager.UserName, IsModified = true, ModifiedOn = DateTime.Now };
                                    Witness ObjwitExt = new Witness()
                                    {
                                        WitnessExt = empExt_val,
                                        DBTrack = c.DBTrack
                                    };
                                    WitnessList.Add(ObjwitExt);
                                }

                            }
                            db_data.Witness = WitnessList; 
                        }
                        else
                        {
                            Msg.Add(" Please select at least a record! ");

                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }

                        
                    }


                    if (enqtypesedit == "BOTH")
                    {
                        if (ddlEmployee != null && ExitEmployee != null)
                        {
                            var ids1 = Utility.StringIdsToListIds(ddlEmployee);
                            var ids2 = Utility.StringIdsToListIds(ExitEmployee);
                            foreach (var ca in ids1)
                            {
                                int empid = Convert.ToInt32(ca);
                                var empIR_val = db.EmployeeIR.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id == empid).SingleOrDefault();
                                if (empIR_val != null)
                                {
                                    c.DBTrack = new DBTrack { Action = "M", CreatedBy = SessionManager.UserName, IsModified = true, ModifiedOn = DateTime.Now };
                                    Witness Objwit = new Witness()
                                    {
                                        WitnessEmp = empIR_val,
                                        DBTrack = c.DBTrack
                                    };
                                    WitnessList.Add(Objwit);
                                }

                            }
                            foreach (var ca in ids2)
                            {
                                int empid = Convert.ToInt32(ca);
                                var empExt_val = db.EnquiryPanelExternal.Where(e => e.Id == empid).SingleOrDefault();
                                if (empExt_val != null)
                                {
                                    c.DBTrack = new DBTrack { Action = "M", CreatedBy = SessionManager.UserName, IsModified = true, ModifiedOn = DateTime.Now };
                                    Witness ObjwitExt = new Witness()
                                    {
                                        WitnessExt = empExt_val,
                                        DBTrack = c.DBTrack
                                    };
                                    WitnessList.Add(ObjwitExt);
                                }

                            }

                            db_data.Witness = WitnessList;

                        }
                        else
                        {
                            Msg.Add(" Please select at least a record! ");

                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }


                    }

                    db.EnquiryPanel.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    TempData["RowVersion"] = db_data.RowVersion;
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    //EnquiryPanel blog = null; // to retrieve old data
                                    //DbPropertyValues originalBlogValues = null;

                                    //using (var context = new DataBaseContext())
                                    //{
                                    //    blog = context.EnquiryPanel.Where(e => e.Id == data).SingleOrDefault();
                                    //    originalBlogValues = context.Entry(blog).OriginalValues;
                                    //}
                                    EnquiryPanel ObjEnqPanel = db.EnquiryPanel.Find(data);

                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = ObjEnqPanel.DBTrack.CreatedBy == null ? null : ObjEnqPanel.DBTrack.CreatedBy,
                                        CreatedOn = ObjEnqPanel.DBTrack.CreatedOn == null ? null : ObjEnqPanel.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    //var m1 = db.EnquiryPanel.Include(e => e.Witness).Where(e => e.Id == data).ToList();
                                    //foreach (var s in m1)
                                    //{

                                    //    db.EnquiryPanel.Attach(s);
                                    //    db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                                    //    db.SaveChanges();
                                    //    TempData["RowVersion"] = s.RowVersion;
                                    //    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    //}

                                    //var CurCorp = db.EnquiryPanel.Find(data);
                                    //TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    //db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;

                                    //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    //{

                                        //EnquiryPanel corp = new EnquiryPanel()
                                        //{                                          
                                        //    PanelName = c.PanelName,                                          
                                        //    Id = data,
                                        //    DBTrack = c.DBTrack
                                        //};

                                        ObjEnqPanel.Id = data;
                                        ObjEnqPanel.PanelName = c.PanelName;
                                        ObjEnqPanel.DBTrack = c.DBTrack;

                                        db.EnquiryPanel.Attach(ObjEnqPanel);
                                        db.Entry(ObjEnqPanel).State = System.Data.Entity.EntityState.Modified;
                                        //db.Entry(ObjEnqPanel).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        await db.SaveChangesAsync();
                                        ts.Complete();
                                        Msg.Add("  Record Updated  ");

                                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //}


                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (EnquiryPanel)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var databaseValues = (EnquiryPanel)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

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
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            EnquiryPanel blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.EnquiryPanel.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            EnquiryPanel corp = new EnquiryPanel()
                            {

                                PanelName = c.PanelName,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];


                            blog.DBTrack = c.DBTrack;
                            db.EnquiryPanel.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, c.LocationObj.LocDesc, "Record Updated", JsonRequestBehavior.AllowGet });
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

    }
}
