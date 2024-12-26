using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using System.Transactions;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Reflection;
using P2b.Global;
using Leave;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using P2BUltimate.Process;
using System.Diagnostics;
namespace P2BUltimate.Controllers.Leave.MainController
{
    [AuthoriseManger]
    public class EmployeeLvStructController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Leave/MainViews/EmployeeLvStruct/Index.cshtml");
        }

        public ActionResult GetLookupLvstructdetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.EmployeeLvStructDetails.Include(e => e.LvHead).ToList();
                IEnumerable<EmployeeLvStructDetails> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.EmployeeLvStructDetails.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }

        }

        public ActionResult CreateEmpLvStructDet_partial()
        {
            return View("~/Views/Shared/Leave/_EmployeeLvStructDetails.cshtml");
        }


        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        [HttpPost]
        //public ActionResult Create(EmployeeLvStruct E, FormCollection form, String forwarddata)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<string> Msg = new List<string>();
        //        try
        //        {
        //            string Emp = forwarddata == "0" ? "" : forwarddata;
        //            //#ConvertLeaveHeadBallist,#ConvertLeaveHeadlist
        //            var EmployeeLvStructDetailslist = form["EmployeeLvStructDetailslist"] == "0" ? "" : form["EmployeeLvStructDetailslist"];
        //            List<EmployeeLvStructDetails> ObjLvstructdetails = new List<EmployeeLvStructDetails>();
        //            if (EmployeeLvStructDetailslist != null && EmployeeLvStructDetailslist != "")
        //            {
        //                var value = db.EmployeeLvStructDetails.Find(int.Parse(EmployeeLvStructDetailslist));
        //                ObjLvstructdetails.Add(value);
        //                E.EmployeeLvStructDetails = ObjLvstructdetails;

        //            }
        //            List<int> ids = null;
        //            if (Emp != null && Emp != "0" && Emp != "false")
        //            {
        //                ids = one_ids(Emp);
        //            }
        //            else
        //            {
        //                return Json(new { success = false, responseText = "Kindly select employee" }, JsonRequestBehavior.AllowGet);
        //            }

        //            if (ModelState.IsValid)
        //            {
        //                using (TransactionScope ts = new TransactionScope())
        //                {


        //                    E.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

        //                    //look.DBTrack = new DBTrack { Action = "C", CreatedBy = "
        //                    // " };

        //                    EmployeeLvStruct OBJELVS = new EmployeeLvStruct()
        //                    {
        //                        EffectiveDate = E.EffectiveDate,
        //                        EmployeeLvStructDetails = E.EmployeeLvStructDetails,
        //                        EndDate = E.EndDate,
        //                        DBTrack = E.DBTrack
        //                    };
        //                    try
        //                    {
        //                        db.EmployeeLvStruct.Add(OBJELVS);
        //                        var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, E.DBTrack);
        //                        //DT_EmployeeLvStruct DT_OBJ = (DT_EmployeeLvStruct)rtn_Obj;
        //                        //db.Create(DT_OBJ);
        //                        db.SaveChanges();
        //                        ts.Complete();
        //                        Msg.Add("  Data Created successfully  ");
        //                        return Json(new Utility.JsonReturnClass { Id = OBJELVS.Id, Val = OBJELVS.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        // return this.Json(new Object[] { OBJELVS.Id, OBJELVS.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
        //                    }

        //                    catch (DbUpdateConcurrencyException)
        //                    {
        //                        return RedirectToAction("Create", new { concurrencyError = true, id = E.Id });
        //                    }
        //                    catch (DataException /* dex */)
        //                    {
        //                        Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                StringBuilder sb = new StringBuilder("");
        //                foreach (ModelState modelState in ModelState.Values)
        //                {
        //                    foreach (ModelError error in modelState.Errors)
        //                    {
        //                        sb.Append(error.ErrorMessage);
        //                        sb.Append("." + "\n");
        //                    }
        //                }
        //                var errorMsg = sb.ToString();
        //                Msg.Add(errorMsg);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
        //                // return this.Json(new { msg = errorMsg });
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LogFile Logfile = new LogFile();
        //            ErrorLog Err = new ErrorLog()
        //            {
        //                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                ExceptionMessage = ex.Message,
        //                ExceptionStackTrace = ex.StackTrace,
        //                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}
        public ActionResult Create(EmployeeLvStruct EmpSalStruct, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            try
            {
                string Emp = forwarddata == "0" ? "" : forwarddata;
                string PayScaleAgr = form["payscaleagreement_drop"] == "0" ? "" : form["payscaleagreement_drop"];
                string Effective_date = form["Effective_Date"] == "0" ? "" : form["Effective_Date"];
                using (DataBaseContext db = new DataBaseContext())
                {


                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = one_ids(Emp);
                    }
                    else
                    {
                        return Json(new { success = false, responseText = "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                    }

                    Employee OEmployee = null;
                    EmployeeLeave OEmployeeLeave = null;
                    PayScaleAgreement OPayScalAgreement = null;

                    if (PayScaleAgr != null && PayScaleAgr != "")
                    {
                        int PayScaleAgrId = int.Parse(PayScaleAgr);
                        OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgrId).SingleOrDefault();
                        if (OPayScalAgreement != null)
                        {
                            var PayScaleAssign = db.PayScaleAssignment.Where(e => e.PayScaleAgreement.Id == PayScaleAgrId).ToList();
                            if (PayScaleAssign.Count == 0)
                            {
                                return Json(new { success = false, responseText = "PayScalessignment not defined." }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    else
                    {
                        return Json(new { success = false, responseText = "Kindly select PayScaleAgreement." }, JsonRequestBehavior.AllowGet);
                    }
                    int Comp_Id = int.Parse(Session["CompId"].ToString());
                    var ComPanyLeave_Id = db.CompanyLeave.Where(e => e.Company.Id == Comp_Id).SingleOrDefault();
                    foreach (var i in ids)
                    {


                        OEmployeeLeave = db.EmployeeLeave.Where(e => e.Employee.Id == i).SingleOrDefault();


                        using (TransactionScope ts = new TransactionScope())
                        {
                            try
                            {
                                LeaveStructureProcess.EmployeeLeaveStructCreationTest(OEmployeeLeave, i, OPayScalAgreement.Id, Convert.ToDateTime(EmpSalStruct.EffectiveDate), ComPanyLeave_Id.Id);

                            }

                            catch (DataException ex)
                            {
                                LogFile Logfile = new LogFile();
                                ErrorLog Err = new ErrorLog()
                                {
                                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                    ExceptionMessage = ex.Message,
                                    ExceptionStackTrace = ex.StackTrace,
                                    LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                    LogTime = DateTime.Now
                                };
                                Logfile.CreateLogFile(Err);
                                return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                            }
                            ts.Complete();
                            //SalaryHeadGenProcess.EmployeeSalaryStructUpdate(OEmployeePayroll, DateTime.Now);
                        }

                    }
                    return Json(new { success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

                    //return View();
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

        public class EmployeeLvStructEditDetails
        {
            public Array EmpLVStructDetails_Id { get; set; }
            public Array EmpLVStructDetails_FullDetails { get; set; }
        }
        public class EditData
        {
            public int Id { get; set; }
            public String PolicyFormula { get; set; }
            public bool Editable { get; set; }
            public String LvName { get; set; }
            public String PolicyName { get; set; }

            //public double Amount { get; set; }
        }
        public JsonResult GetPayscaleagreement(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var a = db.PayScaleAgreement.Find(int.Parse(data));
                int Struct_Id = int.Parse(data);
                var a = db.EmployeeLvStruct.Include(e => e.EmployeeLvStructDetails).Include(e => e.EmployeeLvStructDetails.Select(r => r.LvAssignment))
                    .Include(e => e.EmployeeLvStructDetails.Select(r => r.LvAssignment.PayScaleAgreement))
                    .Where(e => e.Id == Struct_Id).AsNoTracking().SingleOrDefault()
                    .EmployeeLvStructDetails.FirstOrDefault().LvAssignment.PayScaleAgreement;
                return Json(a, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult P2BInlineGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<EditData> EmployeelvStruct = null;

                List<EditData> model = new List<EditData>();
                var view = new EditData();

                int EmpId = int.Parse(gp.id);
                bool EditAppl = true;


                var OEmployeeSalStruct = db.EmployeeLeave.Where(e => e.Employee.Id == EmpId).Include(e => e.Employee).Include(e => e.EmployeeLvStruct.Select(r =>r.EmployeeLvStructDetails))

                                        .Include(e => e.EmployeeLvStruct.Select(r=>r.EmployeeLvStructDetails.Select(t=>t.LvAssignment)))
                                        .Include(e => e.EmployeeLvStruct.Select(r => r.EmployeeLvStructDetails.Select(t => t.LvAssignment.LvHeadFormula)))
                                        .Include(e => e.EmployeeLvStruct.Select(r => r.EmployeeLvStructDetails.Select(t => t.LvAssignment.LvHead)))
                                        .Include(e => e.EmployeeLvStruct.Select(r => r.EmployeeLvStructDetails.Select(t => t.LvAssignment.PolicyName)))
                    // .Include(e => e.EmployeePolicyStruct.Select(r => r.EmployeePolicyStructDetails.Select(t => t.SalaryHead.Type)))

                                       .SingleOrDefault();
                var id = Convert.ToInt32(gp.filter);
                var OEmpSalStruct = OEmployeeSalStruct.EmployeeLvStruct.Where(e => e.Id == id);

                foreach (var x in OEmpSalStruct)
                {

                    var OEmpSalStructDet = x.EmployeeLvStructDetails;
                    foreach (var SalForAppl in OEmpSalStructDet)
                    {
                        var m = db.EmployeeLvStructDetails.Include(e => e.LvAssignment).Include(e => e.LvHeadFormula).Include(e => e.PolicyName).Where(e => e.Id == SalForAppl.Id).SingleOrDefault();


                        //var OSalHeadFormula = x.EmpSalStructDetails
                        //    .Where(e => e.SalaryHead.Id == m.SalaryHead.Id).Select(e => e.PayScaleAssignment.SalHeadFormula
                        //         .Where(r => (r.GeoStruct.Corporate == null || r.GeoStruct.Corporate == x.GeoStruct.Corporate)
                        //             && (r.GeoStruct.Region == null || r.GeoStruct.Region == x.GeoStruct.Region)
                        //             && (r.GeoStruct.Company == null || r.GeoStruct.Company == x.GeoStruct.Company)
                        //            && (r.GeoStruct.Division == null || r.GeoStruct.Division == x.GeoStruct.Division)
                        //            && (r.GeoStruct.Location == null || r.GeoStruct.Location == x.GeoStruct.Location)
                        //            && (r.GeoStruct.Department == null || r.GeoStruct.Department == x.GeoStruct.Department)
                        //            && (r.GeoStruct.Group == null || r.GeoStruct.Group == x.GeoStruct.Group)
                        //            && (r.GeoStruct.Unit == null || r.GeoStruct.Unit == x.GeoStruct.Unit)
                        //            && (r.FuncStruct.Job == null || r.FuncStruct.Job == x.FuncStruct.Job)
                        //            && (r.FuncStruct.JobPosition == null || r.FuncStruct.JobPosition == x.FuncStruct.JobPosition)
                        //            && (r.PayStruct.Grade == null || r.PayStruct.Grade == x.PayStruct.Grade)
                        //            && (r.PayStruct.Level == null || r.PayStruct.Level == x.PayStruct.Level)
                        //             )).ToList();


                        var SalHeadForm = m.LvHeadFormula; //SalaryHeadGenProcess.SalFormulaFinder(x, m.SalaryHead.Id);

                        EditAppl = false;
                        //if (SalHeadForm != null)
                        //{
                        //    if (SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "BASIC")
                        //        EditAppl = true;
                        //    else
                        //        EditAppl = false;
                        //}
                        //else
                        //{
                        //    if (SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "EPF" || SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "PT" ||
                        //        SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "ITAX" || SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "LWF" ||
                        //        SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "ESIC" || SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "CPF" ||
                        //        SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "PENSION")
                        //        EditAppl = false;
                        //    else
                        //        EditAppl = true;
                        //}
                        //var test = db.PayScaleAssignment.AsNoTracking().Where(e => e.SalaryHead.Id == SalForAppl.SalaryHead.Id && e.SalaryHead.SalHeadOperationType.LookupVal.ToString() != "BASIC").Include(e => e.SalHeadFormula.Select(r => r.FormulaType)).FirstOrDefault();
                        //if (test != null)
                        //{
                        //    if (test.SalHeadFormula.Count() > 0 && test.SalHeadFormula.Any(r => r.FormulaType.LookupVal.ToUpper() == "STANDARDFORMULA"))
                        //    {
                        //        EditAppl = false;
                        //    }
                        //}
                        if (m.LvHeadFormula != null)
                        {
                            view = new EditData()
                            {
                                Id = SalForAppl.Id,
                                PolicyFormula = m.LvHeadFormula.Name.ToString(),
                                LvName = SalForAppl.LvHead.LvName.ToUpper(),
                                PolicyName = SalForAppl.PolicyName.LookupVal.ToUpper(),
                                Editable = EditAppl
                            };

                            model.Add(view);
                        }
                    }
                }

                EmployeelvStruct = model;

                IEnumerable<EditData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmployeelvStruct;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.PolicyFormula.ToString().Contains(gp.searchString.ToString()))
                                  || (e.PolicyName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                 
                                   || (e.Editable.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                  || (e.Id.ToString().Contains(gp.searchString.ToString())))
                              .Select(a => new Object[] { a.PolicyFormula, a.LvName, a.PolicyName, a.Editable, a.Id }).ToList();

                        jsonData = IE.Select(a => new Object[] {  a.PolicyFormula,a.LvName, a.PolicyName, a.Editable, a.Id, }).Where((e => (e.Contains(gp.searchString)))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.PolicyFormula, a.LvName, a.PolicyName, a.Editable, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmployeelvStruct;
                    Func<EditData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id.ToString() : ""
                            //gp.sidx == "Id" ? c.Id.ToString() :""
                             );
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "PolicyFormula" ? c.PolicyFormula.ToString() :
                                        gp.sidx == "LvHead" ? c.LvName.ToString() :
                                         gp.sidx == "PolicyName" ? c.PolicyName.ToString() :
                                          gp.sidx == "Editable" ? c.Editable.ToString() :

                                         "");
                        
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.PolicyFormula, a.LvName, a.PolicyName, a.Editable, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.PolicyFormula, a.LvName, a.PolicyName, a.Editable, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.PolicyFormula, a.LvName, a.PolicyName, a.Editable, a.Id }).ToList();
                    }
                    totalRecords = EmployeelvStruct.Count();
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
                var Q = db.EmployeeLvStruct
                    .Include(e => e.EmployeeLvStructDetails)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        EffectiveDate = e.EffectiveDate,
                        EmployeeLvStructDetails = e.EmployeeLvStructDetails,
                        EndDate = e.EndDate,
                        Action = e.DBTrack.Action
                    }).ToList();

                List<EmployeeLvStructEditDetails> EmpLvStruct = new List<EmployeeLvStructEditDetails>();

                var EmpLvStruct_Details = db.EmployeeLvStruct.Include(e => e.EmployeeLvStructDetails).Where(e => e.Id == data).Select(e => e.EmployeeLvStructDetails).ToList();
                if (EmpLvStruct_Details != null)
                {
                    foreach (var ca in EmpLvStruct_Details)
                    {
                        EmpLvStruct.Add(new EmployeeLvStructEditDetails
                        {
                            EmpLVStructDetails_Id = ca.Select(e => e.Id).ToArray(),
                            EmpLVStructDetails_FullDetails = ca.Select(e => e.FullDetails).ToArray()

                        });


                    }

                }




                var Corp = db.EmployeeLvStruct.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, EmpLvStruct, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }



        [HttpPost]
        public async Task<ActionResult> EditSave(EmployeeLvStruct E, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var EmployeeLvStructDetailslist = form["EmployeeLvStructDetailslist"] == "0" ? "" : form["EmployeeLvStructDetailslist"];

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    if (Auth == false)
                    {

                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    EmployeeLvStruct blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.EmployeeLvStruct.Where(e => e.Id == data).Include(e => e.EmployeeLvStructDetails)
                                                                .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    E.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };


                                    List<EmployeeLvStructDetails> ObjEmployeeLvStructDetails = new List<EmployeeLvStructDetails>();
                                    EmployeeLvStruct lvheaddetails = null;
                                    lvheaddetails = db.EmployeeLvStruct.Include(e => e.EmployeeLvStructDetails).Where(e => e.Id == data).SingleOrDefault();
                                    if (EmployeeLvStructDetailslist != null && EmployeeLvStructDetailslist != "")
                                    {
                                        var ids = Utility.StringIdsToListIds(EmployeeLvStructDetailslist);
                                        foreach (var ca in ids)
                                        {
                                            var value = db.EmployeeLvStructDetails.Find(ca);
                                            ObjEmployeeLvStructDetails.Add(value);
                                            lvheaddetails.EmployeeLvStructDetails = ObjEmployeeLvStructDetails;
                                        }
                                    }
                                    else
                                    {
                                        lvheaddetails.EmployeeLvStructDetails = null;
                                    }

                                    var CurCorp = db.EmployeeLvStruct.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        EmployeeLvStruct ObjELVS = new EmployeeLvStruct()
                                        {
                                            EffectiveDate = E.EffectiveDate,
                                            EndDate = E.EndDate,

                                            Id = data,
                                            DBTrack = E.DBTrack
                                        };
                                        db.EmployeeLvStruct.Attach(ObjELVS);
                                        db.Entry(ObjELVS).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(ObjELVS).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    }
                                    // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Leave/Leave", originalBlogValues, db.ChangeTracker, E.DBTrack);
                                        DT_EmployeeLvStruct DT_Corp = (DT_EmployeeLvStruct)obj;

                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = E.Id, Val = E.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { E.Id, E.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                }

                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (LvCreditPolicy)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (LvEncashReq)databaseEntry.ToObject();
                                    E.RowVersion = databaseValues.RowVersion;

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

                            EmployeeLvStruct blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            EmployeeLvStruct Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.EmployeeLvStruct.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            E.DBTrack = new DBTrack
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

                            EmployeeLvStruct corp = new EmployeeLvStruct()
                            {
                                EffectiveDate = E.EffectiveDate,
                                EndDate = E.EndDate,
                                EmployeeLvStructDetails = E.EmployeeLvStructDetails,
                                Id = data,
                                DBTrack = E.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Leave/Leave", "M", blog, corp, "LvEncashReq", E.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.EmployeeLvStruct.Where(e => e.Id == data).Include(e => e.EmployeeLvStructDetails)
                                          .SingleOrDefault();
                                DT_EmployeeLvStruct DT_Corp = (DT_EmployeeLvStruct)obj;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = E.DBTrack;
                            db.EmployeeLvStruct.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = E.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { blog.Id, E.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    EmployeeLvStruct dellvencash = db.EmployeeLvStruct.Where(e => e.Id == data).SingleOrDefault();

                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (dellvencash.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = dellvencash.DBTrack.CreatedBy != null ? dellvencash.DBTrack.CreatedBy : null,
                                CreatedOn = dellvencash.DBTrack.CreatedOn != null ? dellvencash.DBTrack.CreatedOn : null,
                                IsModified = dellvencash.DBTrack.IsModified == true ? true : false
                            };
                            dellvencash.DBTrack = dbT;
                            db.Entry(dellvencash).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, dellvencash.DBTrack);
                            DT_EmployeeLvStruct DT_Corp = (DT_EmployeeLvStruct)rtn_Obj;

                            db.Create(DT_Corp);
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

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            var Empstructdetails = dellvencash.EmployeeLvStructDetails;
                            if (Empstructdetails != null)
                            {
                                var LvBankPolicy = new HashSet<int>(dellvencash.EmployeeLvStructDetails.Select(e => e.Id));
                                if (LvBankPolicy.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
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
                                    CreatedBy = dellvencash.DBTrack.CreatedBy != null ? dellvencash.DBTrack.CreatedBy : null,
                                    CreatedOn = dellvencash.DBTrack.CreatedOn != null ? dellvencash.DBTrack.CreatedOn : null,
                                    IsModified = dellvencash.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = "


                                //   ", ModifiedOn = DateTime.Now };

                                db.Entry(dellvencash).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, dbT);
                                DT_EmployeeLvStruct DT_Corp = (DT_EmployeeLvStruct)rtn_Obj;

                                db.Create(DT_Corp);

                                await db.SaveChangesAsync();


                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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

        public class P2BGridData
        {
            public int Id { get; set; }

            public string struct_Id { get; set; }
            //public Employee Employee { get; set; }
            public string EmpCode { get; set; }//FullNameFML
            public string FullNameFML { get; set; }//FullNameFML
            public string EffectiveDate { get; set; }
            public string EndDate { get; set; }
            //  public int PayScaleAgreement_Id { get; set; }
        }
        public class Employeelvstructdata
        {
            public int Id { get; set; }
            public string FullNameFML { get; set; }
            public string EmpCode { get; set; }
            public ICollection<EmployeeLvStruct> EmployeeLvStruct { get; set; }
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
                IEnumerable<P2BGridData> EmployeeLvStruct = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;
                var oemployeedata = db.EmployeeLvStruct.Where(e => e.EmployeeLeave.Employee.ServiceBookDates != null && e.EndDate == null
                  )
                                    .Include(e => e.EmployeeLeave.Employee)
                                    .Include(e => e.EmployeeLeave.Employee.EmpName).ToList();

                //var oemployeedata = db.EmployeeLeave
                //        .Join(db.Employee, p => p.Employee.Id, pc => pc.Id, (p, pc) => new { p, pc })
                //        .Where(p => p.p.Employee.ServiceBookDates != null && p.p.Employee.ServiceBookDates.ServiceLastDate == null)
                //       .Select(m => new Employeelvstructdata
                //       {
                //           Id = m.p.Employee.Id,
                //           EmpCode = m.p.Employee.EmpCode,
                //           FullNameFML = m.p.Employee.EmpName.FullNameFML,
                //           EmployeeLvStruct = m.p.EmployeeLvStruct
                //       }).ToList();
                foreach (var item in oemployeedata)
                {
                    view = new P2BGridData()
                    {
                        Id = item.EmployeeLeave.Employee.Id,
                        struct_Id = item.Id.ToString(),
                        EmpCode = item.EmployeeLeave.Employee.EmpCode,
                        FullNameFML = item.EmployeeLeave.Employee.EmpName.FullNameFML,
                        //Employee = z.Employee,
                        EffectiveDate = item.EffectiveDate.Value.ToString("dd/MM/yyyy"),
                        EndDate = null,
                        //     PayScaleAgreement_Id = PayScaleAgr.Id
                    };
                    model.Add(view);

                }
                //if (gp.IsAutho == true)
                //{
                //    lencash = db.EmployeeLvStruct.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                //}
                //else
                //{
                //    lencash = db.EmployeeLvStruct.AsNoTracking().ToList();
                //}
                EmployeeLvStruct = model;
                IEnumerable<P2BGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = EmployeeLvStruct;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString))
                             || (e.FullNameFML.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                             || (e.EffectiveDate.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                             || (e.EndDate != null? e.EndDate.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                             || (e.struct_Id.ToString().Contains(gp.searchString))
                             || (e.Id.ToString().Contains(gp.searchString))
                             ).Select(a => new Object[] { a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate !=null ? a.EndDate : "", a.struct_Id, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.struct_Id, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmployeeLvStruct;
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode :
                            gp.sidx == "EmpName" ? c.FullNameFML :
                            gp.sidx == "EffectiveDate" ? c.EffectiveDate :
                            gp.sidx == "EndDate" ? c.EndDate.ToString() :
                            gp.sidx == "struct_Id" ? c.struct_Id.ToString() : 
                            "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.struct_Id, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.struct_Id, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.struct_Id, a.Id }).ToList();
                    }
                    totalRecords = EmployeeLvStruct.Count();
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

        public ActionResult LoadEmp(P2BGrid_Parameters gp)
        {
            List<string> Msg = new List<string>();
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<Employee> Employee = null;
                if (gp.IsAutho == true)
                {
                    Employee = db.Employee.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    db.Database.CommandTimeout = 300;

                      List<EmployeeLeave> uids = db.EmployeeLeave
                        .Include(e => e.EmployeeLvStruct)
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.ServiceBookDates)
                        .AsNoTracking()
                        //.Select(e => e.Employee.Id)
                        .ToList();
                    var Id = Convert.ToInt32(SessionManager.CompanyId);
                    DateTime fromdt = Convert.ToDateTime("01/01/2021");
                      DateTime Todt =Convert.ToDateTime( "31/12/2021");
                    string _CompCode = db.Company.Where(e => e.Id == Id).SingleOrDefault().Code.ToUpper();
                 
                    if (_CompCode=="KDCC")
                    {
                        uids = uids.Where(e => e.Employee.ServiceBookDates != null && (e.Employee.ServiceBookDates.ServiceLastDate == null || (e.Employee.ServiceBookDates.ServiceLastDate.Value >= fromdt && e.Employee.ServiceBookDates.ServiceLastDate.Value<=Todt)) && e.EmployeeLvStruct.Count() == 0).ToList();
                    }
                    else
                    {

                        uids = uids.Where(e => e.Employee.ServiceBookDates != null && e.Employee.ServiceBookDates.ServiceLastDate == null && e.EmployeeLvStruct.Count() == 0).ToList();
                    }
                    if (uids != null && uids.Count() > 0)
                    {
                        var mUids = uids.Select(e => e.Employee.Id).ToList();
                        Employee = db.Employee.Include(e => e.EmpName).Where(t => mUids.Contains(t.Id));
                    }
                    else
                    {
                        Employee = null;
                        Msg.Add(" Employee Null  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        // return this.Json(new Object[] { "", "", " ", JsonRequestBehavior.AllowGet });
                    }
                    //Employee = db.Employee.Include(e => e.EmpName).ToList();
                }

                IEnumerable<Employee> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = Employee;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString.ToString()))
                                || (e.EmpName.FullNameFML.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                   || (e.Id.ToString().Contains(gp.searchString.ToString())))
                                .Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();

                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CardCode, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Employee;


                    Func<Employee, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>
                                         gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.EmpName.FullNameFML.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.EmpCode), a.EmpName != null ? a.EmpName.FullNameFML : "", a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  Convert.ToString(a.EmpCode), a.EmpName != null ? a.EmpName.FullNameFML : "", a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  Convert.ToString(a.EmpCode), a.EmpName != null ? a.EmpName.FullNameFML : "", a.Id }).ToList();
                    }
                    totalRecords = Employee.Count();
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
        public ActionResult Polulate_payscale_agreement(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.PayScaleAgreement.Where(e => e.EndDate == null).ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult Update_Struct(int PayScaleAgreementId) //Create submit
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    //var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgreementId).AsNoTracking().SingleOrDefault();

                    List<EmployeeLeave> EmpList = db.EmployeeLeave
                       .Include(e => e.Employee.EmpOffInfo)
                       .Include(e => e.Employee.EmpName)
                       .Include(e => e.Employee.ServiceBookDates).ToList();

                    //var EmpList = oemployee.Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                    //           .ToList().OrderBy(d => d.Id);
                    
                    var Id = Convert.ToInt32(SessionManager.CompanyId);
                    DateTime fromdt = Convert.ToDateTime("01/01/2021");
                    DateTime Todt = Convert.ToDateTime("31/12/2021");
                    string _CompCode = db.Company.Where(e => e.Id == Id).SingleOrDefault().Code.ToUpper();

                    if (_CompCode=="KDCC")
                    {
                        EmpList = EmpList.Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null || (e.Employee.ServiceBookDates.ServiceLastDate.Value >= fromdt && e.Employee.ServiceBookDates.ServiceLastDate.Value <= Todt))
                               .ToList();
                    }
                    else
                    {
                        EmpList = EmpList.Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                              .ToList();
                    }

                    //var EmpList = db.EmployeePayroll.AsNoTracking().OrderBy(e => e.Id)
                    //            .ToList();
                    int Comp_Id = int.Parse(Session["CompId"].ToString());
                    var ComPanyLeave_Id = db.CompanyLeave.Where(e => e.Company.Id == Comp_Id).SingleOrDefault();

                    foreach (var a in EmpList)
                    {
                        //Utility.DumpProcessStatus("Emp started structure update " + a.Id);

                        var EmpSalStruct = db.EmployeeLvStruct
                                        .Include(e => e.EmployeeLvStructDetails)
                                         .Include(e => e.EmployeeLvStructDetails.Select(t => t.LvHead))
                                        .Include(e => e.EmployeeLvStructDetails.Select(t => t.LvAssignment))
                                        .Include(e => e.EmployeeLvStructDetails.Select(t => t.LvAssignment.PayScaleAgreement))
                                        .Where(e => e.EmployeeLeave_Id == a.Id && e.EndDate == null).AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();

                        //  var SalStruct = EmpSalStruct.EmpSalStruct.Where(r => r.EndDate == null).SingleOrDefault();
                        if (EmpSalStruct != null)
                        {
                            var OEmpSalStructDet = EmpSalStruct.EmployeeLvStructDetails.Select(r => r.LvAssignment.PayScaleAgreement.Id == PayScaleAgreementId).ToList();
                            if (OEmpSalStructDet.Count > 0)
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                              new System.TimeSpan(0, 30, 0)))
                                {
                                    try
                                    {
                                        //List<int> q = new List<int>();
                                        ////if (!a.Employee.EmpOffInfo.VPFAppl)
                                        ////{
                                        ////    q = db.PayScaleAssignment.Where(e => e.PayScaleAgreement.Id == PayScaleAgreementId && e.SalaryHead.Code.ToUpper() != "VPF").Select(e => e.SalaryHead.Id).ToList();
                                        ////}
                                        ////else
                                        ////{
                                        //q = db.LvAssignment.Where(e => e.PayScaleAgreement.Id == PayScaleAgreementId).Select(e => e.LvHead.Id).ToList();
                                        ////  }
                                        //// var q = db.PayScaleAssignment.Where(e => e.PayScaleAgreement.Id == PayScaleAgreementId ).OrderBy(e => e.SalaryHead.Id).Select(e => e.SalaryHead.Id).ToList();
                                        //var t = EmpSalStruct.EmployeeLvStructDetails.OrderBy(e => e.LvHead.Id).Select(r => r.LvHead.Id).ToList();
                                        //var w = q.Except(t);

                                        //if (w.Count() > 0)
                                        //{
                                            LeaveStructureProcess.EmployeeLvStructureCreationWithUpdationTest(EmpSalStruct.Id, 0, PayScaleAgreementId, Convert.ToDateTime(EmpSalStruct.EffectiveDate), a.Id, ComPanyLeave_Id.Id);
                                        //}


                                        LeaveStructureProcess.EmployeeLvStructureCreationWithUpdateFormulaTest(EmpSalStruct.Id, 0, PayScaleAgreementId, Convert.ToDateTime(EmpSalStruct.EffectiveDate), a.Id, ComPanyLeave_Id.Id);

                                        //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 14042017
                                        ts.Complete();
                                        //Utility.DumpProcessStatus("Emp ended structure update " + a.Id);
                                    }
                                    catch (DataException ex)
                                    {
                                        LogFile Logfile = new LogFile();
                                        ErrorLog Err = new ErrorLog()
                                        {
                                            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                            ExceptionMessage = ex.InnerException.Message,
                                            ExceptionStackTrace = ex.StackTrace,
                                            LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                            LogTime = DateTime.Now
                                        };
                                        Logfile.CreateLogFile(Err);
                                        return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                                    }


                                }
                            }
                        }

                    }

                    // return Json(new { success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                    Msg.Add("  Data Saved successfully  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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