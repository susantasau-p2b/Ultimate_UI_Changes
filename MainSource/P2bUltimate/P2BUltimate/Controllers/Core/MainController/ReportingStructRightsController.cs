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
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Core.MainController
{
    public class ReportingStructRightsController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/_ReportingStructRights.cshtml");
        }
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult GetLookup(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ReportingStructRights
                    .Include(e => e.ReportingStruct)
                    .Include(e => e.FuncModules)
                    .Include(e => e.FuncSubModules)
                    .Include(e => e.AccessRights)
                    .Where(e => e.ReportingStruct != null && e.ReportingStruct.RSName != null).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ReportingStructRights.Include(e => e.ReportingStruct)
                    .Include(e => e.FuncModules)
                    .Include(e => e.FuncSubModules)
                    .Include(e => e.AccessRights).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLookupValue(int data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Mainqurey = db.LookupValue.Where(e => e.Id == data).Select(e => e.LookupVal).SingleOrDefault();
                SelectList s = (SelectList)null;
                var selected = "";
                if (data2 != "" && data2 != "0")
                {
                    selected = data2;
                }
                if (Mainqurey != null)
                {
                    IEnumerable<LookupValue> Lookup = new List<LookupValue>();
                    switch (Mainqurey)
                    {
                        case "ELMS":
                            //var qurey = db.LvHead.ToList();
                            //s = new SelectList(qurey, "Id", "LvCode", selected);
                            //return Json(s, JsonRequestBehavior.AllowGet);
                            Lookup = db.Lookup.Where(e => e.Code == "617").Select(e => e.LookupValues.Where(r => r.IsActive == true)).SingleOrDefault();
                            if (Lookup != null)
                            {
                                s = new SelectList(Lookup, "Id", "LookupVal", selected);
                            }
                            break;
                        case "ETRM":
                            Lookup = db.Lookup.Where(e => e.Code == "615").Select(e => e.LookupValues.Where(r => r.IsActive == true)).SingleOrDefault();
                            if (Lookup != null)
                            {
                                s = new SelectList(Lookup, "Id", "LookupVal", selected);
                            }
                            break;
                        case "EPMS":
                            Lookup = db.Lookup.Where(e => e.Code == "614").Select(e => e.LookupValues.Where(r => r.IsActive == true)).SingleOrDefault();
                            if (Lookup != null)
                            {
                                s = new SelectList(Lookup, "Id", "LookupVal", selected);
                            }
                            break;
                        case "EEIS":
                            Lookup = db.Lookup.Where(e => e.Code == "616").Select(e => e.LookupValues.Where(r => r.IsActive == true)).SingleOrDefault();
                            if (Lookup != null)
                            {
                                s = new SelectList(Lookup, "Id", "LookupVal", selected);
                            }
                            break;
                    }
                }
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult Create(ReportingStructRights COBJ, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string ReportingStruct = form["ReportingStructlist"] == "0" ? "" : form["ReportingStructlist"];
                    string FuncModules = form["FuncModuleslist"] == "0" ? "" : form["FuncModuleslist"];
                    string FuncSubModules = form["FuncSubModuleslist"] == "0" ? "" : form["FuncSubModuleslist"];
                    string AccessRights = form["AccessRightslist"] == "0" ? "" : form["AccessRightslist"];

                    int comp_Id = Convert.ToInt32(Session["CompID"]);
                    var Company = new Company();
                    Company = db.Company.Where(e => e.Id == comp_Id).SingleOrDefault();
                    if (ReportingStruct != null && ReportingStruct != "" && FuncModules != null && FuncModules != "")
                    {
                        if (ReportingStruct != "")
                        {
                            var id = int.Parse(ReportingStruct);
                            var val = db.ReportingStruct.Where(e => e.Id == id).SingleOrDefault();
                            COBJ.ReportingStruct = val;
                        }

                        if (FuncModules != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "601").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(FuncModules)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(FuncModules));
                            COBJ.FuncModules = val;
                        }
                        if (AccessRights != "")
                        {
                            var id = int.Parse(AccessRights);
                            var val = db.AccessRights.Include(e => e.ActionName).Where(e => e.Id == id).SingleOrDefault();
                            COBJ.AccessRights = val;
                        }
                        if (FuncSubModules != "")
                        {
                            switch (COBJ.FuncModules.LookupVal.ToUpper())
                            {
                                case "ELMS": 
                                     var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "617").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(FuncSubModules)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(FuncSubModules));
                                     COBJ.FuncSubModules = val;
                                    break;
                                case "ETRM": 
                                       val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "615").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(FuncSubModules)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(FuncSubModules));
                                       COBJ.FuncSubModules = val;
                                    break;
                                case "EPMS": 
                                       val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "614").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(FuncSubModules)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(FuncSubModules));
                                       COBJ.FuncSubModules = val;
                                    break;
                                case "EEIS":
                                     val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "616").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(FuncSubModules)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(FuncSubModules));
                                     COBJ.FuncSubModules = val;
                                    break;
                            }

                           
                        }


                        if (ModelState.IsValid)
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {

                                //   COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                                ReportingStructRights oReportingStructRights = new ReportingStructRights()
                                {
                                    ReportingStruct = COBJ.ReportingStruct,
                                    FuncModules = COBJ.FuncModules,
                                    AccessRights = COBJ.AccessRights,
                                    FuncSubModules = COBJ.FuncSubModules,
                                    FullDetails = COBJ.FullDetails,
                                };
                                try
                                {
                                    db.ReportingStructRights.Add(oReportingStructRights);
                                    //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, COBJ.DBTrack);
                                    //DT_JobStatus DT_OBJ = (DT_JobStatus)rtn_Obj;
                                    //DT_OBJ.EmpStatus_Id = COBJ.FuncModules == null ? 0 : COBJ.FuncModules.Id;
                                    //DT_OBJ.EmpActingStatus_Id = COBJ.ReportingStruct == null ? 0 : COBJ.ReportingStruct.Id;
                                    //db.Create(DT_OBJ);
                                    db.SaveChanges();


                                    if (Company != null)
                                    {
                                        var Objjobstatus = new List<ReportingStructRights>();
                                        Objjobstatus.Add(oReportingStructRights);
                                        Company.ReportingStructRights = Objjobstatus;
                                        db.Company.Attach(Company);
                                        db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(Company).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    ts.Complete();
                                    Msg.Add("  Data Created successfully  ");
                                    return Json(new Utility.JsonReturnClass { Id = oReportingStructRights.Id, Val = oReportingStructRights.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return this.Json(new Object[] { oReportingStructRights.Id, oReportingStructRights.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                                }

                                catch (DbUpdateConcurrencyException)
                                {
                                    return RedirectToAction("Create", new { concurrencyError = true, id = COBJ.Id });
                                }
                                catch (DataException e)
                                {
                                    return this.Json(new Object[] { null, null, e.InnerException.Message.ToString(), JsonRequestBehavior.AllowGet });
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
                        }
                    }
                    else
                    {
                        Msg.Add(" ReportingStruct , FuncModules Can't Be Empty.... ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //var errorMsg = "ReportingStruct , FuncModules Can't Be Empty....";
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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
                var ReportingStructRights = db.ReportingStructRights
                                    .Include(e => e.ReportingStruct)
                                    .Include(e=>e.ReportingStruct.GeoFuncList)
                                    .Include(e=>e.ReportingStruct.GeoGraphList)
                                    .Include(e => e.FuncModules)
                                    .Include(e => e.AccessRights)
                                    .Include(e => e.AccessRights.ActionName)
                                    .Include(e => e.FuncSubModules)
                                    .Where(e => e.Id == data).ToList();

                var dfs = db.Lookup.Include(q => q.LookupValues).Where(a => a.Code == "601").SingleOrDefault();
                var rep = ReportingStructRights.FirstOrDefault().FuncModules;
                var repdd = dfs.LookupValues.Where(q => q.Id == rep.Id).SingleOrDefault();
                var dbAccessRights = ReportingStructRights.Select(t => t.AccessRights).FirstOrDefault();
                string IsApproveRejectAppl = dbAccessRights.IsApproveRejectAppl == true ? "Yes" : "No";
                string IsClose = dbAccessRights.IsClose == true ? "Yes" : "No";
                var dbReportingStruct=ReportingStructRights.Select(z=>z.ReportingStruct).FirstOrDefault();
                string FunctionalAppl = dbReportingStruct.FunctionalAppl == true ? "Yes" : "No";
                string GeographicalAppl = dbReportingStruct.GeographicalAppl == true ? "Yes" : "No";
                string IndividualAppl = dbReportingStruct.IndividualAppl == true ? "Yes" : "No";
                string RoleBasedAppl = dbReportingStruct.RoleBasedAppl == true ? "Yes" : "No";
                
                var dsfsdf = "";
                if (repdd != null)
                {
                    switch (repdd.LookupVal)
                    {
                        case "ELMS":
                            dsfsdf = "617";
                            break;
                        case "ETRM":
                            dsfsdf = "615";
                            break;
                        case "EPMS":
                            dsfsdf = "614";
                            break;
                        case "EEIS":
                            dsfsdf = "616";
                            break;
                    }
                }
               
                var r = (from ca in ReportingStructRights
                         select new
                         {   
                             Id = ca.Id,
                             FuncModules_Id = ca.FuncModules != null ? ca.FuncModules.Id : 0,
                             FuncModules_Code = dsfsdf,
                             ReportingStruct_Id = ca.ReportingStruct != null ? ca.ReportingStruct.Id : 0,
                             ReportingStruct_Val = ca.ReportingStruct != null ?  ca.ReportingStruct.RSName + "  " + "GeographicalAppl:" + GeographicalAppl +" "+ "FunctionalAppl:" + FunctionalAppl +" "+ "RoleBasedAppl:" + RoleBasedAppl : "",
                             AccessRights_Id = ca.AccessRights != null ? ca.AccessRights.Id : 0,
                             AccessRights_Val = ca.AccessRights != null && ca.AccessRights.ActionName != null ? ca.AccessRights.ActionName.LookupVal + "  " + "IsApproveRejectAppl:" + IsApproveRejectAppl + "  " + "IsClose:" + IsClose + "  " + " LvNoOfDaysFrom:" + ca.AccessRights.LvNoOfDaysFrom + "  " + " LvNoOfDaysTo:" + ca.AccessRights.LvNoOfDaysTo : "",
                             FuncSubModules_Id = ca.FuncSubModules != null ? ca.FuncSubModules.Id : 0,
                             //Action = ca.DBTrack.Action
                         }).Distinct();


                return this.Json(new Object[] { r, "", JsonRequestBehavior.AllowGet });
            }
        }
        public async Task<ActionResult> EditSave(ReportingStructRights ESOBJ, FormCollection form, int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    string FuncModules = form["FuncModuleslist"] == "0" ? "" : form["FuncModuleslist"];
                    string FuncSubModules = form["FuncSubModuleslist"] == "0" ? "" : form["FuncSubModuleslist"];
                    string ReportingStruct = form["ReportingStructlist"] == "0" ? "" : form["ReportingStructlist"];
                    string AccessRights = form["AccessRightslist"] == "0" ? "" : form["AccessRightslist"];
                    if (ReportingStruct != null)
                    {
                        var val = db.ReportingStruct.Find(int.Parse(ReportingStruct));
                        ESOBJ.ReportingStruct = val;
                    }
                    if (FuncModules != null && FuncModules != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "601").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(FuncModules)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(FuncModules));
                        ESOBJ.FuncModules = val;
                    }

                    if (FuncSubModules != null && FuncSubModules != "")
                    {
                        //var val = db.LookupValue.Find(int.Parse(FuncSubModules));
                        //ESOBJ.FuncSubModules = val;

                        switch (ESOBJ.FuncModules.LookupVal.ToUpper())
                        {
                            case "ELMS":
                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "617").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(FuncSubModules)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(FuncSubModules));
                                ESOBJ.FuncSubModules = val;
                                break;
                            case "ETRM":
                                val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "615").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(FuncSubModules)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(FuncSubModules));
                                ESOBJ.FuncSubModules = val;
                                break;
                            case "EPMS":
                                val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "614").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(FuncSubModules)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(FuncSubModules));
                                ESOBJ.FuncSubModules = val;
                                break;
                            case "EEIS":
                                val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "616").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(FuncSubModules)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(FuncSubModules));
                                ESOBJ.FuncSubModules = val;
                                break;
                        }
                    }
                    if (AccessRights != null)
                    {
                        var val = db.AccessRights.Find(int.Parse(AccessRights));
                        ESOBJ.AccessRights = val;
                    }
                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    ReportingStructRights blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.ReportingStructRights.Where(e => e.Id == data)
                                                                .Include(e => e.ReportingStruct)
                                                                .Include(e => e.FuncModules)
                                                                .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }
                                    int a = EditS(FuncModules, FuncSubModules, ReportingStruct, AccessRights, data, ESOBJ);
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { ESOBJ.Id, ESOBJ.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                                }
                            }

                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (ReportingStructRights)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (ReportingStructRights)databaseEntry.ToObject();
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
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            ReportingStructRights blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            ReportingStructRights Old_Obj = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.ReportingStructRights.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            //ESOBJ.DBTrack = new DBTrack
                            //{
                            //    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                            //    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                            //    Action = "M",
                            //    IsModified = blog.DBTrack.IsModified == true ? true : false,
                            //    ModifiedBy = SessionManager.UserName,
                            //    ModifiedOn = DateTime.Now
                            //};
                            ReportingStructRights corp = new ReportingStructRights()
                            {
                                Id = data,
                                //DBTrack = ESOBJ.DBTrack,
                                //RowVersion = (Byte[])TempData["RowVersion"]
                            };
                            //using (var context = new DataBaseContext())
                            //{
                            //    var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "ReportingStructRights","");
                            //    // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                            //    Old_Obj = context.ReportingStructRights.Where(e => e.Id == data).Include(e => e.ReportingStruct)
                            //        .Include(e => e.FuncModules).SingleOrDefault();
                            //    DT_JobStatus DT_Corp = (DT_JobStatus)obj;
                            //    DT_Corp.EmpActingStatus_Id = DBTrackFile.ValCompare(Old_Obj.ReportingStruct, ESOBJ.ReportingStruct);//Old_Obj.Address == c.Address ? 0 : Old_Obj.Address == null && c.Address != null ? c.Address.Id : Old_Obj.Address.Id;
                            //    DT_Corp.EmpStatus_Id = DBTrackFile.ValCompare(Old_Obj.FuncModules, ESOBJ.FuncModules); //Old_Obj.BusinessType == c.BusinessType ? 0 : Old_Obj.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Obj.BusinessType.Id;                        
                            //    db.Create(DT_Corp);
                            //    //db.SaveChanges();
                            //}
                            //blog.DBTrack = ESOBJ.DBTrack;
                            //db.ReportingStructRights.Attach(blog);
                            //db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            //db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            //ts.Complete();
                            Msg.Add(" Record Updated ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "", "Record Updated", JsonRequestBehavior.AllowGet });
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
        public int EditS(string FuncModules, string FuncSubModules, string ReportingStruct, string AccessRights, int data, ReportingStructRights ESOBJ)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (FuncModules != null)
                {
                    if (FuncModules != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(FuncModules));
                        ESOBJ.FuncModules = val;

                        var type = db.ReportingStructRights.Include(e => e.FuncModules)
                            .Where(e => e.Id == data).SingleOrDefault();
                        IList<ReportingStructRights> typedetails = null;
                        if (type.FuncModules != null)
                        {
                            typedetails = db.ReportingStructRights.Where(x => x.FuncModules.Id == type.FuncModules.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.ReportingStructRights.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in typedetails)
                        {
                            s.FuncModules = ESOBJ.FuncModules;
                            db.ReportingStructRights.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.ReportingStructRights.Include(e => e.FuncModules).Where(x => x.Id == data).ToList();
                        foreach (var s in Dtls)
                        {
                            s.FuncModules = null;
                            db.ReportingStructRights.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var Dtls = db.ReportingStructRights.Include(e => e.FuncModules).Where(x => x.Id == data).ToList();
                    foreach (var s in Dtls)
                    {
                        s.FuncModules = null;
                        db.ReportingStructRights.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (FuncSubModules != null)
                {
                    if (FuncSubModules != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(FuncSubModules));
                        ESOBJ.FuncSubModules = val;

                        var type = db.ReportingStructRights.Include(e => e.FuncSubModules)
                            .Where(e => e.Id == data).SingleOrDefault();
                        IList<ReportingStructRights> typedetails = null;
                        if (type.FuncSubModules != null)
                        {
                            typedetails = db.ReportingStructRights.Where(x => x.FuncSubModules.Id == type.FuncSubModules.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.ReportingStructRights.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in typedetails)
                        {
                            s.FuncSubModules = ESOBJ.FuncSubModules;
                            db.ReportingStructRights.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.ReportingStructRights.Include(e => e.FuncSubModules).Where(x => x.Id == data).ToList();
                        foreach (var s in Dtls)
                        {
                            s.FuncSubModules = null;
                            db.ReportingStructRights.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var Dtls = db.ReportingStructRights.Include(e => e.FuncSubModules).Where(x => x.Id == data).ToList();
                    foreach (var s in Dtls)
                    {
                        s.FuncSubModules = null;
                        db.ReportingStructRights.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (ReportingStruct != null)
                {
                    if (ReportingStruct != "")
                    {
                        var val = db.ReportingStruct.Find(int.Parse(ReportingStruct));
                        ESOBJ.ReportingStruct = val;

                        var type = db.ReportingStructRights
                            .Include(e => e.ReportingStruct)
                            .Where(e => e.Id == data).SingleOrDefault();
                        IList<ReportingStructRights> typedetails = null;
                        if (type.ReportingStruct != null)
                        {
                            typedetails = db.ReportingStructRights.Where(x => x.ReportingStruct.Id == type.ReportingStruct.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.ReportingStructRights.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in typedetails)
                        {
                            s.ReportingStruct = ESOBJ.ReportingStruct;
                            db.ReportingStructRights.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.ReportingStructRights.Include(e => e.ReportingStruct).Where(x => x.Id == data).ToList();
                        foreach (var s in Dtls)
                        {
                            s.ReportingStruct = null;
                            db.ReportingStructRights.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var Dtls = db.ReportingStructRights.Include(e => e.ReportingStruct).Where(x => x.Id == data).ToList();
                    foreach (var s in Dtls)
                    {
                        s.ReportingStruct = null;
                        db.ReportingStructRights.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (AccessRights != null)
                {
                    if (AccessRights != "")
                    {
                        var val = db.AccessRights.Find(int.Parse(AccessRights));
                        ESOBJ.AccessRights = val;

                        var type = db.ReportingStructRights
                            .Include(e => e.AccessRights)
                            .Where(e => e.Id == data).SingleOrDefault();
                        IList<ReportingStructRights> typedetails = null;
                        if (type.AccessRights != null)
                        {
                            typedetails = db.ReportingStructRights.Include(e => e.AccessRights).Where(x => x.AccessRights.Id == type.AccessRights.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.ReportingStructRights.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in typedetails)
                        {
                            s.AccessRights = ESOBJ.AccessRights;
                            db.ReportingStructRights.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.ReportingStructRights.Include(e => e.AccessRights).Where(x => x.Id == data).ToList();
                        foreach (var s in Dtls)
                        {
                            s.AccessRights = null;
                            db.ReportingStructRights.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var Dtls = db.ReportingStructRights.Include(e => e.AccessRights).Where(x => x.Id == data).ToList();
                    foreach (var s in Dtls)
                    {
                        s.AccessRights = null;
                        db.ReportingStructRights.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                var CurOBJ = db.ReportingStructRights.Find(data);
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                //{
                //    ReportingStructRights ESIOBJ = new ReportingStructRights()
                //    {
                //        Id = data,
                //    };

                //    db.ReportingStructRights.Attach(ESIOBJ);
                //    db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                //    db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                //    return 1;
                //}
                return 1;
            }
        }
        public ActionResult Delete(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                ReportingStructRights ReportingStructRights = db.ReportingStructRights.Where(e => e.Id == data).SingleOrDefault();
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(ReportingStructRights).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                }

                catch (DataException /* dex */)
                {
                    // return this.Json(new Object[] { "", "Unable to delete. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
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
    }
}