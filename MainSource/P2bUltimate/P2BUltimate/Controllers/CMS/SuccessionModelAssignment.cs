using CMS_SPS;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Transactions;
using P2BUltimate.Models;
using System.Data.Entity.Infrastructure;
using System.Data;
using System.Text;
using P2b.Global;
using P2BUltimate.Security;
using System.Threading.Tasks;


namespace P2BUltimate.Controllers.CMS
{
    public class SuccessionModelAssignmentController : Controller
    {
        //
        // GET: /SuccessionModelAssignment/
        public ActionResult Index()
        {
            return View("~/Views/CMS/MainViews/SuccessionModelAssignment/Index.cshtml");
        }
        public ActionResult GetSuccessionModel(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var fall = db.SuccessionModel.ToList();
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }

        }
        public class returndatagridDataclass //Parentgrid
        {
            public string Id { get; set; }
            public string BatchName { get; set; }
            public string BatchDescription { get; set; }
            public string SuccessionModel { get; set; }
            public string CreationDate { get; set; }
        }

        public ActionResult SuccessionModelAssignment_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.SuccessionModelAssignment.Include(e => e.SuccessionModel)
                        .Include(e => e.SussessionModelAssignment_OrgStructure).Include(e => e.SussessionModelAssignment_OrgStructure.Select(r => r.GeoStruct))
                        .Include(e => e.SussessionModelAssignment_OrgStructure.Select(r => r.PayStruct))
                        .Include(e => e.SussessionModelAssignment_OrgStructure.Select(r => r.FuncStruct))
                        .ToList();

                    IEnumerable<CMS_SPS.SuccessionModelAssignment> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {

                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.BatchName.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                  || (e.BatchDescription.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<CMS_SPS.SuccessionModelAssignment, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.BatchName : "");
                    var sortcolumn = Request["sSortDir_0"];
                    if (sortcolumn == "asc")
                    {
                        fall = fall.OrderBy(orderfunc);
                    }
                    else
                    {
                        fall = fall.OrderByDescending(orderfunc);
                    }
                    // Paging 
                    var dcompanies = fall
                            .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompanies.Count == 0)
                    {
                        List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                        foreach (var item in fall)
                        {
                            result.Add(new returndatagridDataclass
                            {
                                Id = item.Id.ToString(),
                                BatchName = item.BatchName,
                                BatchDescription = item.BatchDescription,
                                SuccessionModel = item.SuccessionModel != null ? item.SuccessionModel.FullDetails : "",
                                CreationDate = item.CreationDate.Value.ToShortDateString(),

                            });
                        }
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = from c in dcompanies

                                     select new[] { null, Convert.ToString(c.Id), c.BatchName, };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }//for data reterivation
                }
                catch (Exception e)
                {
                    List<string> Msg = new List<string>();
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = e.Message,
                        ExceptionStackTrace = e.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public class returndatagridChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string GeoStruct { get; set; }
            public string PayStruct { get; set; }
            public string FuncStruct { get; set; }

        }
        public ActionResult A_SuccessionModelAssignment_Grid(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.SuccessionModelAssignment.Include(e => e.SuccessionModel)
                       .Include(e => e.SussessionModelAssignment_OrgStructure)
                       .Include(e => e.SussessionModelAssignment_OrgStructure.Select(r => r.GeoStruct.Location))
                       .Include(e => e.SussessionModelAssignment_OrgStructure.Select(r => r.GeoStruct.Department))
                       .Include(e => e.SussessionModelAssignment_OrgStructure.Select(r => r.GeoStruct.Department.DepartmentObj))
                       .Include(e => e.SussessionModelAssignment_OrgStructure.Select(r => r.GeoStruct.Location.LocationObj))
                       .Include(e => e.SussessionModelAssignment_OrgStructure.Select(r => r.PayStruct))
                       .Include(e => e.SussessionModelAssignment_OrgStructure.Select(r => r.PayStruct.JobStatus))
                       .Include(e => e.SussessionModelAssignment_OrgStructure.Select(r => r.PayStruct.Grade))
                       .Include(e => e.SussessionModelAssignment_OrgStructure.Select(r => r.PayStruct.JobStatus.EmpStatus))
                       .Include(e => e.SussessionModelAssignment_OrgStructure.Select(r => r.FuncStruct))
                       .Include(e => e.SussessionModelAssignment_OrgStructure.Select(r => r.FuncStruct.Job))
                       .Where(e => e.Id == data).SingleOrDefault();

                 

                    if (db_data.SussessionModelAssignment_OrgStructure != null)
                    {
                        List<returndatagridChildDataClass> returndata = new List<returndatagridChildDataClass>();

                        foreach (var item in db_data.SussessionModelAssignment_OrgStructure.OrderByDescending(e => e.Id))
                        {
                            returndata.Add(new returndatagridChildDataClass
                            {
                                Id = item.Id,
                                GeoStruct = item.GeoStruct != null && item.GeoStruct.Location != null && item.GeoStruct.Location.LocationObj != null ? item.GeoStruct.FullDetails : "",
                                PayStruct = item.PayStruct != null && item.PayStruct.JobStatus != null && item.PayStruct.JobStatus.EmpStatus != null ? item.PayStruct.FullDetails : "",
                                FuncStruct = item.FuncStruct != null && item.FuncStruct.Job != null ? item.FuncStruct.FullDetails : "",
                            });
                        }
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
                    }

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        [HttpPost]
        public ActionResult Create(CMS_SPS.SuccessionModelAssignment c, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();

                try
                {
                    string BatchName = form["BatchName"] == "0" ? "" : form["BatchName"];
                    string BatchDescription = form["BatchDescription"] == "0" ? "" : form["BatchDescription"];
                    string disc = form["SuccessionModelList"] == "0" ? "" : form["SuccessionModelList"];
                    if (disc != null && disc != "")
                    {
                        int SuccessionModelId = Convert.ToInt32(disc);
                        var val = db.SuccessionModel
                                            .Where(e => e.Id == SuccessionModelId).SingleOrDefault();
                        c.SuccessionModel = val;
                    }
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            try
                            {
                                CMS_SPS.SuccessionModelAssignment SuccessionModelAssignment = new CMS_SPS.SuccessionModelAssignment()
                                {
                                    BatchName = c.BatchName,
                                    BatchDescription = c.BatchDescription,
                                    SuccessionModel = c.SuccessionModel,
                                    CreationDate = DateTime.Now.Date,
                                    DBTrack = c.DBTrack
                                };
                                db.SuccessionModelAssignment.Add(SuccessionModelAssignment);
                                db.SaveChanges();

                                string geo_id = form["geo_id"] == "0" ? "" : form["geo_id"];
                                string fun_id = form["fun_id"] == "0" ? "" : form["fun_id"];
                                string pay_id = form["pay_id"] == "0" ? "" : form["pay_id"];



                                CMS_SPS.SussessionModelAssignment_OrgStructure cmaorg = null;

                                if (geo_id != "" && geo_id != null)
                                {
                                    var item1 = Utility.StringIdsToListIds(geo_id);
                                    foreach (var geoId in item1)
                                    {
                                        GeoStruct geostruct = db.GeoStruct.Find(geoId);
                                        cmaorg = new CMS_SPS.SussessionModelAssignment_OrgStructure()
                                        {
                                            GeoStruct = geostruct,
                                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                        };
                                    }

                                }


                                if (pay_id != "" && pay_id != null)
                                {
                                    var item2 = Utility.StringIdsToListIds(pay_id);
                                    foreach (var payId in item2)
                                    {
                                        PayStruct paystruct = db.PayStruct.Find(payId);
                                        cmaorg = new CMS_SPS.SussessionModelAssignment_OrgStructure()
                                        {
                                            PayStruct = paystruct,
                                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                        };
                                    }


                                }

                                if (fun_id != "" && fun_id != null)
                                {
                                    var item3 = Utility.StringIdsToListIds(fun_id);
                                    foreach (var funId in item3)
                                    {
                                        FuncStruct funcstruct = db.FuncStruct.Find(funId);
                                        cmaorg = new CMS_SPS.SussessionModelAssignment_OrgStructure()
                                        {
                                            FuncStruct = funcstruct,
                                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                        };
                                    }

                                }
                                List<CMS_SPS.SussessionModelAssignment_OrgStructure> SuccessionModelAssignmentOrgStructurelist = new List<CMS_SPS.SussessionModelAssignment_OrgStructure>();
                                db.SussessionModelAssignment_OrgStructure.Add(cmaorg);
                                SuccessionModelAssignmentOrgStructurelist.Add(cmaorg);
                                SuccessionModelAssignment.SussessionModelAssignment_OrgStructure = SuccessionModelAssignmentOrgStructurelist;
                                db.SuccessionModelAssignment.Attach(SuccessionModelAssignment);
                                db.Entry(SuccessionModelAssignment).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(SuccessionModelAssignment).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                                Msg.Add("Data Saved successfully");
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
                                //    List<string> Msg = new List<string>();
                                Msg.Add(ex.Message);
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
                        //var errorMsg = sb.ToString();
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        List<string> MsgB = new List<string>();
                        var errorMsg = sb.ToString();
                        MsgB.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);
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
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    SuccessionModelAssignment CompAssign = db.SuccessionModelAssignment
                                                     .Where(e => e.Id == data).FirstOrDefault();

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        DBTrack dbT = new DBTrack
                        {
                            Action = "M",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now,
                            CreatedBy = CompAssign.DBTrack.CreatedBy != null ? CompAssign.DBTrack.CreatedBy : null,
                            CreatedOn = CompAssign.DBTrack.CreatedOn != null ? CompAssign.DBTrack.CreatedOn : null,
                            IsModified = CompAssign.DBTrack.IsModified == true ? false : false

                        };
                        CompAssign.CloseDate = DateTime.Now;
                        CompAssign.DBTrack = dbT;
                        db.Entry(CompAssign).State = System.Data.Entity.EntityState.Modified;
                        await db.SaveChangesAsync();
                        ts.Complete();
                        Msg.Add("  Batch Closed.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    //}
                }
                catch (RetryLimitExceededException /* dex */)
                {

                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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

