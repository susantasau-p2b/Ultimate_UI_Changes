///
/// Created by Tanushri
///

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

namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class WorkExpDetailsController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /WorkExpDetails/

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/WorkExpDetails/Index.cshtml");
        }
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(WorkExpDetails Emp, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //    string GeoStruct = form["GeoStructList"] == "0" ? "" : form["GeoStructList"];
                    //    if (GeoStruct != null)
                    //    {
                    //        if (GeoStruct != "")
                    //        {
                    //            GeoStruct OBJ = db.GeoStruct.Find(Convert.ToInt32(GeoStruct));
                    //            COBJ.GeoStruct = OBJ;
                    //        }
                    //    }

                    //    string FuncStruct = form["FuncStructList"] == "0" ? "" : form["FuncStructList"];
                    //    if (FuncStruct != null)
                    //    {
                    //        if (FuncStruct != "")
                    //        {
                    //            FuncStruct OBJ = db.FuncStruct.Find(Convert.ToInt32(FuncStruct));
                    //            COBJ.FuncStruct = OBJ;
                    //        }
                    //    }

                    string Corporate = form["Corporate_drop"] == "0" ? "" : form["Corporate_drop"];
                    string Region = form["Region_drop"] == "0" ? "" : form["Region_drop"];
                    string Company = form["Company_drop"] == "0" ? "" : form["Company_drop"];
                    string Division = form["Division_drop"] == "0" ? "" : form["Division_drop"];
                    string Location = form["Location_drop"] == "0" ? "" : form["Location_drop"];
                    string Department = form["Department_drop"] == "0" ? "" : form["Department_drop"];
                    string Group = form["Group_drop"] == "0" ? "" : form["Group_drop"];
                    string Unit = form["Unit_drop"] == "0" ? "" : form["Unit_drop"];
                    //   string Grade = form["Grade_drop"] == "0" ? "" : form["Grade_drop"];
                    //  string Level = form["Level_drop"] == "0" ? "" : form["Level_drop"];
                    //   string JobStatus = form["JobStatus_drop"] == "0" ? "" : form["JobStatus_drop"];
                    string EmpActingStatus = form["EmpActingStatus_drop"] == "0" ? "" : form["EmpActingStatus_drop"];
                    string Job = form["Job_drop"] == "0" ? "" : form["Job_drop"];
                    string Position = form["Position_drop"] == "0" ? "" : form["Position_drop"];

                    int Emp1 = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);

                    Employee empdata;
                    if (Emp1 != null && Emp1 != 0)
                    {
                        empdata = db.Employee.Find(Emp1);
                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    var Get_Geo_Id = db.GeoStruct.Include(e => e.Company)
                             .Include(e => e.Department)
                             .Include(e => e.Department.DepartmentObj)
                             .Include(e => e.Division)
                             .Include(e => e.Location)
                             .Include(e => e.Location.LocationObj)
                             .Include(e => e.Group)
                             .Include(e => e.Unit);

                    var Get_Fun_Id = db.FuncStruct.Include(e => e.Job)
                         .Include(e => e.JobPosition);

                    if (Job != "")
                    {
                        var id = Convert.ToInt32(Job);
                        Get_Fun_Id = Get_Fun_Id.Where(e => e.Job.Id == id);
                    }
                    if (Position != "")
                    {
                        var id = Convert.ToInt32(Position);
                        Get_Fun_Id = Get_Fun_Id.Where(e => e.JobPosition.Id == id);
                    }

                    // var fun_data = Get_Fun_Id.SingleOrDefault();

                    if (Corporate != "")
                    {
                        var id = Convert.ToInt32(Corporate);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Corporate.Id == id);
                    }
                    if (Region != "")
                    {
                        var id = Convert.ToInt32(Region);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Region.Id == id);
                    }
                    if (Company != "")
                    {
                        var id = Convert.ToInt32(Company);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Company.Id == id);
                    }
                    if (Division != "")
                    {
                        var id = Convert.ToInt32(Division);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Division.Id == id);
                    }
                    if (Location != "")
                    {
                        var id = Convert.ToInt32(Location);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Location.Id == id);
                    }
                    if (Department != "")
                    {
                        var id = Convert.ToInt32(Department);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Department.Id == id);
                    }
                    if (Group != "")
                    {
                        var id = Convert.ToInt32(Group);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Group.Id == id);
                    }
                    if (Unit != "")
                    {
                        var id = Convert.ToInt32(Unit);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Unit.Id == id);
                    }
                    if (Get_Geo_Id != null)
                    {
                        var geo_data = Get_Geo_Id.FirstOrDefault();
                        if (geo_data != null)
                        {
                            Emp.GeoStruct = db.GeoStruct.Find(geo_data.Id);
                        }
                    }

                    if (Get_Fun_Id != null)
                    {
                        var fun_data = Get_Fun_Id.FirstOrDefault();

                        if (fun_data != null)
                        {
                            Emp.FuncStruct = db.FuncStruct.Find(fun_data.Id);
                        }

                    }
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            var EmpSocialInfo1 = db.Employee.Include(e => e.WorkExp).Include(e => e.WorkExp.Select(q => q.GeoStruct))
                                                    .Include(e => e.WorkExp.Select(q => q.GeoStruct.Company))
                                                     .Include(e => e.WorkExp.Select(q => q.GeoStruct.Department))
                                                    .Include(e => e.WorkExp.Select(q => q.GeoStruct.Division))
                                                    .Include(e => e.WorkExp.Select(q => q.GeoStruct.Location))
                                                     .Include(e => e.WorkExp.Select(q => q.FuncStruct))
                                                      .Include(e => e.WorkExp.Select(q => q.FuncStruct.Job))
                                                      .Include(e => e.WorkExp.Select(q => q.FuncStruct.JobPosition))
                                                   .Where(e => e.Id != null).ToList();
                            foreach (var item in EmpSocialInfo1)
                            {
                                if (item.WorkExp.Count != 0 && empdata.WorkExp.Count != 0)
                                {
                                    int aid = item.WorkExp.Select(a => a.Id).SingleOrDefault();
                                    int bid = empdata.WorkExp.Select(a => a.Id).SingleOrDefault();

                                    if (aid == bid)
                                    {
                                        var v = empdata.EmpCode;
                                        Msg.Add("Record Already Exist For Employee Code=" + v);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }

                            }


                            if (Emp.FromDate > Emp.ToDate)
                            {
                                Msg.Add("  From Date should be less than To Date.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            DateTime current = DateTime.Now;
                            if (Emp.FromDate > current)
                            {
                                Msg.Add("  From Date should be less than current Date.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }


                            Emp.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            WorkExpDetails WorkExpDetails = new WorkExpDetails()
                            {
                                YrOfService = Emp.YrOfService,
                                FromDate = Emp.FromDate,
                                ToDate = Emp.ToDate,
                                WorkDetails = Emp.WorkDetails,
                                FuncStruct = Emp.FuncStruct,
                                GeoStruct = Emp.GeoStruct,
                                DBTrack = Emp.DBTrack
                            };
                            try
                            {
                                db.WorkExpDetails.Add(WorkExpDetails);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Emp.DBTrack);
                                DT_WorkExpDetails DT_OBJ = (DT_WorkExpDetails)rtn_Obj;
                                DT_OBJ.FuncStruct_Id = Emp.FuncStruct == null ? 0 : Emp.FuncStruct.Id;
                                DT_OBJ.GeoStruct_Id = Emp.GeoStruct == null ? 0 : Emp.GeoStruct.Id;
                                db.Create(DT_OBJ);
                                db.SaveChanges();


                                List<WorkExpDetails> empVisaDetails = new List<WorkExpDetails>();
                                empVisaDetails.Add(WorkExpDetails);
                                empdata.WorkExp = empVisaDetails;
                                db.Employee.Attach(empdata);
                                db.Entry(empdata).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();



                                ts.Complete();
                                Msg.Add("  Data Created successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { null, null, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = Emp.Id });
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
                        // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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

        public class returnDataClass
        {
            public Array id { get; set; }
            public Array val { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var EmpSocialInfo1 = db.Employee.Include(e => e.WorkExp).Include(e => e.WorkExp.Select(q => q.GeoStruct))
                                               .Include(e => e.WorkExp.Select(q => q.GeoStruct.Company))
                                                .Include(e => e.WorkExp.Select(q => q.GeoStruct.Department))
                                               .Include(e => e.WorkExp.Select(q => q.GeoStruct.Division))
                                               .Include(e => e.WorkExp.Select(q => q.GeoStruct.Location))
                                                .Include(e => e.WorkExp.Select(q => q.FuncStruct))
                                                 .Include(e => e.WorkExp.Select(q => q.FuncStruct.Job))
                                                 .Include(e => e.WorkExp.Select(q => q.FuncStruct.JobPosition))
                                              .Where(e => e.Id == data).SingleOrDefault();

                int AID = EmpSocialInfo1.WorkExp.Select(s => s.Id).SingleOrDefault();

                var WorkExpDetails = db.WorkExpDetails

                                    .Where(e => e.Id == AID).Select(e => new
                                    {
                                        YrOfService = e.YrOfService,
                                        FromDate = e.FromDate,
                                        ToDate = e.ToDate,
                                        WorkDetails = e.WorkDetails,
                                        GeoStruct_Corporate_Id = e.GeoStruct.Corporate == null ? 0 : e.GeoStruct.Corporate.Id,
                                        GeoStruct_Region_Id = e.GeoStruct.Region == null ? 0 : e.GeoStruct.Region.Id,
                                        GeoStruct_Company_Id = e.GeoStruct.Company == null ? 0 : e.GeoStruct.Company.Id,
                                        GeoStruct_Division_Id = e.GeoStruct.Division == null ? 0 : e.GeoStruct.Division.Id,
                                        GeoStruct_Location_Id = e.GeoStruct.Location == null ? 0 : e.GeoStruct.Location.Id,
                                        GeoStruct_Department_Id = e.GeoStruct.Department == null ? 0 : e.GeoStruct.Department.Id,
                                        GeoStruct_Group_Id = e.GeoStruct.Group == null ? 0 : e.GeoStruct.Group.Id,
                                        GeoStruct_Unit_Id = e.GeoStruct.Unit == null ? 0 : e.GeoStruct.Unit.Id,
                                        FuncStruct_Job_Id = e.FuncStruct.Job == null ? 0 : e.FuncStruct.Job.Id,
                                        FuncStruct_Position_Id = e.FuncStruct.JobPosition == null ? 0 : e.FuncStruct.JobPosition.Id,
                                    }).ToList();



                //    var a = "";

                //var W = db.DT_WorkExpDetails
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,
                //         YrOfService = e.YrOfService ,
                //         FromDate = e.FromDate ,
                //         ToDate = e.ToDate,                     
                //         WorkDetails = e.WorkDetails,
                //        // Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
                //         FuncStruct_Val = e.FuncStruct_Id == null ? "" : e.FuncStruct_Id.ToString(),
                //         GeoStruct_Val = e.GeoStruct_Id == null ? "" : e.GeoStruct_Id.ToString(),
                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var LKup = db.WorkExpDetails.Find(AID);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return Json(new Object[] { WorkExpDetails, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> EditSave1(WorkExpDetails Emp, FormCollection form, int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Corporate = form["Corporate_drop"] == "0" ? "" : form["Corporate_drop"];
                    string Region = form["Region_drop"] == "0" ? "" : form["Region_drop"];
                    string Company = form["Company_drop"] == "0" ? "" : form["Company_drop"];
                    string Division = form["Division_drop"] == "0" ? "" : form["Division_drop"];
                    string Location = form["Location_drop"] == "0" ? "" : form["Location_drop"];
                    string Department = form["Department_drop"] == "0" ? "" : form["Department_drop"];
                    string Group = form["Group_drop"] == "0" ? "" : form["Group_drop"];
                    string Unit = form["Unit_drop"] == "0" ? "" : form["Unit_drop"];
                    //   string Grade = form["Grade_drop"] == "0" ? "" : form["Grade_drop"];
                    //  string Level = form["Level_drop"] == "0" ? "" : form["Level_drop"];
                    //   string JobStatus = form["JobStatus_drop"] == "0" ? "" : form["JobStatus_drop"];
                    string EmpActingStatus = form["EmpActingStatus_drop"] == "0" ? "" : form["EmpActingStatus_drop"];
                    string Job = form["Job_drop"] == "0" ? "" : form["Job_drop"];
                    string Position = form["Position_drop"] == "0" ? "" : form["Position_drop"];


                    var EmpSocialInfo1 = db.Employee.Include(e => e.WorkExp).Include(e => e.WorkExp.Select(q => q.GeoStruct))
                                                          .Include(e => e.WorkExp.Select(q => q.GeoStruct.Company))
                                                           .Include(e => e.WorkExp.Select(q => q.GeoStruct.Department))
                                                          .Include(e => e.WorkExp.Select(q => q.GeoStruct.Division))
                                                          .Include(e => e.WorkExp.Select(q => q.GeoStruct.Location))
                                                           .Include(e => e.WorkExp.Select(q => q.FuncStruct))
                                                            .Include(e => e.WorkExp.Select(q => q.FuncStruct.Job))
                                                            .Include(e => e.WorkExp.Select(q => q.FuncStruct.JobPosition))
                                                         .Where(e => e.Id == data).SingleOrDefault();

                    int AID = EmpSocialInfo1.WorkExp.Select(s => s.Id).SingleOrDefault();




                    bool Auth = form["Autho_Allow"] == "true" ? true : false;



                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                if (Emp.FromDate > Emp.ToDate)
                                {
                                    Msg.Add("  From Date should be less than To Date.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    WorkExpDetails blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.WorkExpDetails.Where(e => e.Id == AID)
                                                                .Include(e => e.GeoStruct)
                                                                .Include(e => e.FuncStruct)
                                                                .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    Emp.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    int a = EditS(AID, Emp, Emp.DBTrack);



                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, Emp.DBTrack);

                                        DT_WorkExpDetails DT_OBJ = (DT_WorkExpDetails)obj;
                                        DT_OBJ.FuncStruct_Id = blog.FuncStruct == null ? 0 : blog.FuncStruct.Id;
                                        DT_OBJ.GeoStruct_Id = blog.GeoStruct == null ? 0 : blog.GeoStruct.Id;
                                        db.Create(DT_OBJ);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();

                                    Msg.Add(" Record Updated ");
                                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }

                            //catch (DbUpdateException e) { throw e; }
                            //catch (DataException e) { throw e; }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (WorkExpDetails)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (WorkExpDetails)databaseEntry.ToObject();
                                    Emp.RowVersion = databaseValues.RowVersion;

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

                            WorkExpDetails blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            WorkExpDetails Old_Obj = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.WorkExpDetails.Where(e => e.Id == AID).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            Emp.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            WorkExpDetails corp = new WorkExpDetails()
                            {
                                YrOfService = Emp.YrOfService,
                                FromDate = Emp.FromDate,
                                ToDate = Emp.ToDate,
                                Id = AID,
                                DBTrack = Emp.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "WorkExpDetails", Emp.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("CCore/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Obj = context.WorkExpDetails.Where(e => e.Id == AID).Include(e => e.GeoStruct)
                                    .Include(e => e.FuncStruct).SingleOrDefault();
                                DT_WorkExpDetails DT_OBJ = (DT_WorkExpDetails)obj;
                                DT_OBJ.FuncStruct_Id = DBTrackFile.ValCompare(Old_Obj.FuncStruct, Emp.FuncStruct);//Old_Obj.Address == c.Address ? 0 : Old_Obj.Address == null && c.Address != null ? c.Address.Id : Old_Obj.Address.Id;
                                DT_OBJ.GeoStruct_Id = DBTrackFile.ValCompare(Old_Obj.GeoStruct, Emp.GeoStruct); //Old_Obj.BusinessType == c.BusinessType ? 0 : Old_Obj.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Obj.BusinessType.Id;                        
                                DT_OBJ.FromDate = Emp.FromDate;
                                db.Create(DT_OBJ);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = Emp.DBTrack;
                            db.WorkExpDetails.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = Emp.FromDate.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { blog.Id, Emp.FromDate, "Record Updated", JsonRequestBehavior.AllowGet });
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
        public int EditS(int AID, WorkExpDetails Emp, DBTrack dbT)
        {
            //*************************** Code For One to One********************************************
            //WorkExpDetails typedetails = null;
            //if (Emp.GeoStruct != null)
            //{
            //    WorkExpDetails Employee = db.WorkExpDetails.Include(e => e.GeoStruct)
            //                                     .Where(e => e.Id == AID).SingleOrDefault();

            //    Employee.GeoStruct = new GeoStruct
            //    {
            //        Company = Emp.GeoStruct.Company,
            //        Corporate = Emp.GeoStruct.Corporate,
            //        Department = Emp.GeoStruct.Department,
            //        Division = Emp.GeoStruct.Division,
            //        Group = Emp.GeoStruct.Group,
            //        Location = Emp.GeoStruct.Location,
            //        Region = Emp.GeoStruct.Region,
            //        Unit = Emp.GeoStruct.Unit
            //    };
            //}

            //if (Emp.FuncStruct != null)
            //{
            //    WorkExpDetails Employee = db.WorkExpDetails.Include(e => e.FuncStruct)
            //                                     .Where(e => e.Id == AID).SingleOrDefault();

            //    Employee.FuncStruct = new FuncStruct
            //    {
            //        Job = Emp.FuncStruct.Job,
            //        JobPosition = Emp.FuncStruct.JobPosition
            //    };
            //}


            //db.WorkExpDetails.Attach(typedetails);
            //db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
            //db.SaveChanges();
            TempData["RowVersion"] = Emp.RowVersion;
            //db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;

            using (DataBaseContext db = new DataBaseContext())
            {
                var CurEmp = db.WorkExpDetails.Find(AID);
                TempData["CurrRowVersion"] = CurEmp.RowVersion;
                db.Entry(CurEmp).State = System.Data.Entity.EntityState.Detached;


                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    Emp.DBTrack = dbT;
                    WorkExpDetails ESIOBJ = new WorkExpDetails()
                    {
                        Id = AID,
                        FromDate = Emp.FromDate,
                        ToDate = Emp.ToDate,
                        YrOfService = Emp.YrOfService,
                        WorkDetails = Emp.WorkDetails,
                        FullDetails = Emp.FullDetails,
                        DBTrack = Emp.DBTrack
                    };

                    db.WorkExpDetails.Attach(ESIOBJ);
                    db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(WorkExpDetails c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string Corporate = form["Corporate_drop"] == "0" ? "" : form["Corporate_drop"];
                    string Region = form["Region_drop"] == "0" ? "" : form["Region_drop"];
                    string Company = form["Company_drop"] == "0" ? "" : form["Company_drop"];
                    string Division = form["Division_drop"] == "0" ? "" : form["Division_drop"];
                    string Location = form["Location_drop"] == "0" ? "" : form["Location_drop"];
                    string Department = form["Department_drop"] == "0" ? "" : form["Department_drop"];
                    string Group = form["Group_drop"] == "0" ? "" : form["Group_drop"];
                    string Unit = form["Unit_drop"] == "0" ? "" : form["Unit_drop"];
                    string EmpActingStatus = form["EmpActingStatus_drop"] == "0" ? "" : form["EmpActingStatus_drop"];
                    string Job = form["Job_drop"] == "0" ? "" : form["Job_drop"];
                    string Position = form["Position_drop"] == "0" ? "" : form["Position_drop"];


                    var EmpSocialInfo1 = db.Employee.Include(e => e.WorkExp).Include(e => e.WorkExp.Select(q => q.GeoStruct))
                                                          .Include(e => e.WorkExp.Select(q => q.GeoStruct.Company))
                                                           .Include(e => e.WorkExp.Select(q => q.GeoStruct.Department))
                                                          .Include(e => e.WorkExp.Select(q => q.GeoStruct.Division))
                                                          .Include(e => e.WorkExp.Select(q => q.GeoStruct.Location))
                                                           .Include(e => e.WorkExp.Select(q => q.FuncStruct))
                                                            .Include(e => e.WorkExp.Select(q => q.FuncStruct.Job))
                                                            .Include(e => e.WorkExp.Select(q => q.FuncStruct.JobPosition))
                                                         .Where(e => e.Id == data).SingleOrDefault();

                    int AID = EmpSocialInfo1.WorkExp.Select(s => s.Id).SingleOrDefault();


                    var Get_Geo_Id = db.GeoStruct.Include(e => e.Company)
                     .Include(e => e.Department)
                     .Include(e => e.Department.DepartmentObj)
                     .Include(e => e.Division)
                     .Include(e => e.Location)
                     .Include(e => e.Location.LocationObj)
                     .Include(e => e.Group)
                     .Include(e => e.Unit);

                    var Get_Fun_Id = db.FuncStruct.Include(e => e.Job)
                         .Include(e => e.JobPosition);

                    if (Job != "")
                    {
                        var id = Convert.ToInt32(Job);
                        Get_Fun_Id = Get_Fun_Id.Where(e => e.Job.Id == id);
                    }
                    if (Position != "")
                    {
                        var id = Convert.ToInt32(Position);
                        Get_Fun_Id = Get_Fun_Id.Where(e => e.JobPosition.Id == id);
                    }

                    // var fun_data = Get_Fun_Id.SingleOrDefault();

                    if (Corporate != "")
                    {
                        var id = Convert.ToInt32(Corporate);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Corporate.Id == id);
                    }
                    if (Region != "")
                    {
                        var id = Convert.ToInt32(Region);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Region.Id == id);
                    }
                    if (Company != "")
                    {
                        var id = Convert.ToInt32(Company);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Company.Id == id);
                    }
                    if (Division != "")
                    {
                        var id = Convert.ToInt32(Division);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Division.Id == id);
                    }
                    if (Location != "")
                    {
                        var id = Convert.ToInt32(Location);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Location.Id == id);
                    }
                    if (Department != "")
                    {
                        var id = Convert.ToInt32(Department);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Department.Id == id);
                    }
                    if (Group != "")
                    {
                        var id = Convert.ToInt32(Group);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Group.Id == id);
                    }
                    if (Unit != "")
                    {
                        var id = Convert.ToInt32(Unit);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Unit.Id == id);
                    }
                    if (Get_Geo_Id != null)
                    {
                        var geo_data = Get_Geo_Id.FirstOrDefault();
                        if (geo_data != null)
                        {
                            c.GeoStruct = db.GeoStruct.Find(geo_data.Id);
                        }
                    }

                    if (Get_Fun_Id != null)
                    {
                        var fun_data = Get_Fun_Id.FirstOrDefault();

                        if (fun_data != null)
                        {
                            c.FuncStruct = db.FuncStruct.Find(fun_data.Id);
                        }

                    }



                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    if (Auth == false)
                    {

                        if (ModelState.IsValid)
                        {
                            try
                            {
                                if (c.FromDate > c.ToDate)
                                {
                                    Msg.Add("  From Date should be less than To Date.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }

                                DateTime current = DateTime.Now;
                                if (c.FromDate > current)
                                {
                                    Msg.Add("  From Date should be less than current Date.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    WorkExpDetails blog = null; // to retrieve old data                           
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.WorkExpDetails.Where(e => e.Id == AID).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                        TempData["RowVersion"] = blog.RowVersion;
                                    }
                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    var CurEmp = db.WorkExpDetails.Find(AID);
                                    TempData["CurrRowVersion"] = CurEmp.RowVersion;
                                    db.Entry(CurEmp).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        WorkExpDetails ESIOBJ = new WorkExpDetails()
                                        {
                                            Id = AID,
                                            FromDate = c.FromDate,
                                            ToDate = c.ToDate,
                                            YrOfService = c.YrOfService,
                                            WorkDetails = c.WorkDetails,
                                            FullDetails = c.FullDetails,
                                            DBTrack = c.DBTrack
                                        };

                                        db.WorkExpDetails.Attach(ESIOBJ);
                                        db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    }
                                    await db.SaveChangesAsync();
                                    using (var context = new DataBaseContext())
                                    {

                                        //To save data in history table 
                                        var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, c, "WorkExpDetails", c.DBTrack);
                                        DT_WorkExpDetails DT_GRD = (DT_WorkExpDetails)Obj;
                                        db.DT_WorkExpDetails.Add(DT_GRD);
                                        db.SaveChanges();
                                    }
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (WorkExpDetails)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (WorkExpDetails)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;
                                }
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

                            WorkExpDetails blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            WorkExpDetails Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.WorkExpDetails.Where(e => e.Id == AID).SingleOrDefault();
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
                            WorkExpDetails corp = new WorkExpDetails()
                            {
                                FuncStruct = c.FuncStruct,
                                FromDate = c.FromDate,
                                GeoStruct = c.GeoStruct,
                                ToDate = c.ToDate,
                                YrOfService = c.YrOfService,
                                Id = AID,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "WorkExpDetails", c.DBTrack);

                                Old_Corp = context.WorkExpDetails.Where(e => e.Id == AID)
                                    .Include(e => e.GeoStruct).Include(e => e.FuncStruct).SingleOrDefault();
                                DT_WorkExpDetails DT_Corp = (DT_WorkExpDetails)obj;
                                DT_Corp.GeoStruct_Id = DBTrackFile.ValCompare(Old_Corp.GeoStruct, c.GeoStruct);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                DT_Corp.FuncStruct_Id = DBTrackFile.ValCompare(Old_Corp.FuncStruct, c.FuncStruct); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                db.Create(DT_Corp);
                            }
                            blog.DBTrack = c.DBTrack;
                            db.WorkExpDetails.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, c.CompName, "Record Updated", JsonRequestBehavior.AllowGet });
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
        //[ValidateAntiForgeryToken]
        public ActionResult Delete(int? data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();

                var EmpSocialInfo1 = db.Employee.Include(e => e.WorkExp).Include(e => e.WorkExp.Select(q => q.GeoStruct))
                                                   .Include(e => e.WorkExp.Select(q => q.GeoStruct.Company))
                                                    .Include(e => e.WorkExp.Select(q => q.GeoStruct.Department))
                                                   .Include(e => e.WorkExp.Select(q => q.GeoStruct.Division))
                                                   .Include(e => e.WorkExp.Select(q => q.GeoStruct.Location))
                                                    .Include(e => e.WorkExp.Select(q => q.FuncStruct))
                                                     .Include(e => e.WorkExp.Select(q => q.FuncStruct.Job))
                                                     .Include(e => e.WorkExp.Select(q => q.FuncStruct.JobPosition))
                                                  .Where(e => e.Id == data).SingleOrDefault();

                int AID = EmpSocialInfo1.WorkExp.Select(s => s.Id).SingleOrDefault();



                WorkExpDetails WorkExpDetails = db.WorkExpDetails.Where(e => e.Id == AID).SingleOrDefault();
                try
                {
                    //var selectedValues = WorkExpDetails.SocialActivities;
                    //var lkValue = new HashSet<int>(WorkExpDetails.SocialActivities.Select(e => e.Id));
                    //if (lkValue.Count > 0)
                    //{
                    //    return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                    //}

                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(WorkExpDetails).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                }

                catch (DataException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new Object[] { "", "Unable to delete. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });

                }
            }
        }
        public ActionResult P2BGrid1(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                int ParentId = 2;
                var jsonData = (Object)null;
                var LKVal = db.WorkExpDetails.ToList();

                if (gp.IsAutho == true)
                {
                    LKVal = db.WorkExpDetails.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    LKVal = db.WorkExpDetails.Include(e => e.GeoStruct).Include(e => e.FuncStruct).AsNoTracking().ToList();
                }


                IEnumerable<WorkExpDetails> IE;
                if (!string.IsNullOrEmpty(gp.searchString))
                {
                    IE = LKVal;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.FromDate, a.ToDate, a.YrOfService, a.WorkDetails }).Where((e => (e.Id.ToString() == gp.searchString) || (e.FromDate.ToString() == gp.searchString.ToLower()) || (e.ToDate.ToString() == gp.searchString.ToLower()) || (e.YrOfService.ToString() == gp.searchString.ToLower()) || (e.WorkDetails.ToString() == gp.searchString.ToLower())));
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.FromDate, a.ToDate, a.YrOfService, a.WorkDetails }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = LKVal;
                    Func<WorkExpDetails, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "YrOfService" ? c.YrOfService.ToString() :
                                        gp.sidx == "FromDate" ? c.FromDate.ToString() :
                                         gp.sidx == "WorkDetails" ? c.WorkDetails.ToString() :
                                         gp.sidx == "ToDate " ? c.ToDate.ToString() :
                                         "");
                    }

                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.FromDate.Value.ToShortDateString(), a.ToDate.Value.ToShortDateString(), a.YrOfService, a.WorkDetails }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.FromDate.Value.ToShortDateString(), a.ToDate.Value.ToShortDateString(), a.YrOfService, a.WorkDetails }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.FromDate.Value.ToShortDateString(), a.ToDate.Value.ToShortDateString(), a.YrOfService, a.WorkDetails }).ToList();
                    }
                    totalRecords = LKVal.Count();
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
                    total = totalPages,
                    p2bparam = ParentId
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
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
                IEnumerable<Employee> VisaDetails = null;
                if (gp.IsAutho == true)
                {
                    VisaDetails = db.Employee.Include(e => e.WorkExp).Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    VisaDetails = db.Employee.Include(q => q.WorkExp).Include(q => q.EmpName).Where(q => q.WorkExp.Count > 0).ToList();
                }

                IEnumerable<Employee> IE;

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = VisaDetails;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e =>  (e.EmpCode.ToString().Contains(gp.searchString.ToString())) 
                                || (e.EmpName.FullNameFML.ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.Id.ToString().Contains(gp.searchString.ToString()))
                                )
                            .Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFM, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = VisaDetails;
                    Func<Employee, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.EmpName.FullNameFML.ToString() :
                                          "");
                    }


                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    totalRecords = VisaDetails.Count();
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
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            
   			List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            WorkExpDetails ESI = db.WorkExpDetails
                                .Include(e => e.FuncStruct)
                                .Include(e => e.GeoStruct)
                                .FirstOrDefault(e => e.Id == auth_id);

                            ESI.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = ESI.DBTrack.ModifiedBy != null ? ESI.DBTrack.ModifiedBy : null,
                                CreatedBy = ESI.DBTrack.CreatedBy != null ? ESI.DBTrack.CreatedBy : null,
                                CreatedOn = ESI.DBTrack.CreatedOn != null ? ESI.DBTrack.CreatedOn : null,
                                IsModified = ESI.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.WorkExpDetails.Attach(ESI);
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(ESI).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, ESI.DBTrack);
                            DT_WorkExpDetails DT_OBJ = (DT_WorkExpDetails)rtn_Obj;
                            DT_OBJ.GeoStruct_Id = ESI.GeoStruct == null ? 0 : ESI.GeoStruct.Id;
                            DT_OBJ.FuncStruct_Id = ESI.FuncStruct == null ? 0 : ESI.FuncStruct.Id;
                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = ESI.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { ESI.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        WorkExpDetails Old_OBJ = db.WorkExpDetails
                                                .Include(e => e.FuncStruct)
                                .Include(e => e.GeoStruct)
                                                .Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_WorkExpDetails Curr_OBJ = db.DT_WorkExpDetails
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_OBJ != null)
                        {
                            WorkExpDetails WorkExpDetails = new WorkExpDetails();
                            // string FuncStruct = Curr_OBJ.FuncStruct_Id == null ? null : Curr_OBJ.FuncStruct_Id.ToString();
                            //   string GeoStruct = Curr_OBJ.GeoStruct_Id == null ? null : Curr_OBJ.GeoStruct_Id.ToString();
                            WorkExpDetails.YrOfService = Curr_OBJ.YrOfService == 0.0 ? Old_OBJ.YrOfService : Curr_OBJ.YrOfService;
                            WorkExpDetails.FromDate = Curr_OBJ.FromDate == null ? Old_OBJ.FromDate : Curr_OBJ.FromDate;
                            WorkExpDetails.ToDate = Curr_OBJ.ToDate == null ? Old_OBJ.ToDate : Curr_OBJ.ToDate;
                            WorkExpDetails.WorkDetails = Curr_OBJ.WorkDetails == null ? Old_OBJ.WorkDetails : Curr_OBJ.WorkDetails;

                            //      corp.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        WorkExpDetails.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
                                            CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_OBJ.DBTrack.ModifiedBy == null ? null : Old_OBJ.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_OBJ.DBTrack.ModifiedOn == null ? null : Old_OBJ.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        //  int a = EditS(auth_id, WorkExpDetails, WorkExpDetails.DBTrack);

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = WorkExpDetails.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { WorkExpDetails.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (WorkExpDetails)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (WorkExpDetails)databaseEntry.ToObject();
                                        WorkExpDetails.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                        {
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        //return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //WorkExpDetails corp = db.WorkExpDetails.Find(auth_id);
                            WorkExpDetails ESI = db.WorkExpDetails.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                            //Address add = corp.Address;
                            //ContactDetails conDet = corp.ContactDetails;
                            //SocialActivities val = corp.BusinessType;

                            ESI.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = ESI.DBTrack.ModifiedBy != null ? ESI.DBTrack.ModifiedBy : null,
                                CreatedBy = ESI.DBTrack.CreatedBy != null ? ESI.DBTrack.CreatedBy : null,
                                CreatedOn = ESI.DBTrack.CreatedOn != null ? ESI.DBTrack.CreatedOn : null,
                                IsModified = false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.WorkExpDetails.Attach(ESI);
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, ESI.DBTrack);
                            DT_WorkExpDetails DT_OBJ = (DT_WorkExpDetails)rtn_Obj;
                            //DT_OBJ.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            //DT_OBJ.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                            //DT_OBJ.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            DT_OBJ.GeoStruct_Id = ESI.GeoStruct == null ? 0 : ESI.GeoStruct.Id;
                            DT_OBJ.FuncStruct_Id = ESI.FuncStruct == null ? 0 : ESI.FuncStruct.Id;
                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();
                            db.Entry(ESI).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();

                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
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
        public Object ExitanceFirstlevelFun()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var data = (string)null;
                var a = Get_Job(data);
                if (a != null)
                {
                    var selectlistjson = new
                    {
                        SelectlistType = "Job_drop",
                        selectlist = new SelectList(a, "Id", "Name", "")
                    };
                    return selectlistjson;
                }
                else
                {
                    var b = Get_JobPosition(data);
                    if (b != null)
                    {
                        var selectlistjson = new
                        {
                            SelectlistType = "JobPosition_drop",
                            selectlist = new SelectList(a, "Id", "Name", "")
                        };
                        return selectlistjson;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
        private List<JobPosition> Get_JobPosition(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<JobPosition> Corporate_selectlist = new List<JobPosition>();
                if (data == null)
                {
                    var Corporate_data = db.FuncStruct.Include(e => e.JobPosition).Where(e => e.JobPosition != null).Select(e => e.JobPosition.Id).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.JobPosition.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (data != "" && data != "0")
                {
                    var Corp_id = Convert.ToInt32(data);
                    var region_data = db.FuncStruct.Include(e => e.JobPosition)
                        .Where(e => (e.Job.Id == Corp_id && e.JobPosition != null))
                        .Select(e => e.JobPosition.Id).Distinct().ToList();
                    if (region_data != null && region_data.Count != 0)
                    {
                        foreach (var ca in region_data)
                        {
                            var a = db.JobPosition.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
        private List<Job> Get_Job(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Job> Corporate_selectlist = new List<Job>();
                if (data == null)
                {
                    var Corporate_data = db.FuncStruct.Include(e => e.Job).Where(e => e.Job != null).Select(e => e.Job.Id).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.Job.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
        public List<Corporate> Get_Corporate(String data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Corporate> Corporate_selectlist = new List<Corporate>();
                if (data == null)
                {
                    var Corporate_data = db.GeoStruct.Include(e => e.Corporate).Where(e => e.Corporate != null).Select(e => e.Corporate.Id).Distinct().ToList();

                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.Corporate.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
        public List<Region> Get_Region(String data)
        {
            List<Region> Corporate_selectlist = new List<Region>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data == null)
                {
                    var Corporate_data = db.GeoStruct.Include(e => e.Region).Where(e => e.Region != null).Select(e => e.Region.Id).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.Region.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (data != "" && data != "0")
                {
                    var Corp_id = Convert.ToInt32(data);
                    var region_data = db.GeoStruct.Include(e => e.Region).Where(e => (e.Corporate.Id == Corp_id && e.Region != null)).Select(e => e.Region.Id).Distinct().ToList();
                    if (region_data != null && region_data.Count != 0)
                    {
                        foreach (var ca in region_data)
                        {
                            var a = db.Region.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
        public List<Company> Get_Company(String data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Company> Corporate_selectlist = new List<Company>();
                if (data == null)
                {
                    var Corporate_data = db.GeoStruct.Include(e => e.Company).Where(e => e.Company != null).Select(e => e.Company.Id).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.Company.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (data != "" && data != "0")
                {
                    var Corp_id = Convert.ToInt32(data);
                    var region_data = db.GeoStruct.Include(e => e.Company)
                        .Where(e => (e.Corporate.Id == Corp_id && e.Company != null) || (e.Region.Id == Corp_id && e.Company != null))
                        .Select(e => e.Company.Id).Distinct().ToList();
                    if (region_data != null && region_data.Count != 0)
                    {
                        foreach (var ca in region_data)
                        {
                            var a = db.Company.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
        public List<Division> Get_Division(String data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Division> Corporate_selectlist = new List<Division>();
                if (data == null)
                {
                    var Corporate_data = db.GeoStruct.Include(e => e.Division).Where(e => e.Division != null).Select(e => e.Division.Id).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.Division.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (data != "" && data != "0")
                {
                    var Corp_id = Convert.ToInt32(data);

                    var region_data = db.GeoStruct.Include(e => e.Division).Where(e =>
                        (e.Corporate.Id == Corp_id && e.Division != null) ||
                        (e.Region.Id == Corp_id && e.Division != null) ||
                        (e.Company.Id == Corp_id && e.Division != null))
                        .Select(e => e.Division.Id).Distinct().ToList();
                    if (region_data != null && region_data.Count != 0)
                    {
                        foreach (var ca in region_data)
                        {
                            var a = db.Division.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
        public List<Location> Get_Location(String data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Location> Corporate_selectlist = new List<Location>();
                if (data == null)
                {
                    var Corporate_data = db.GeoStruct.Include(e => e.Location).Where(e => e.Location != null).Select(e => e.Location.Id).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.Location.Include(e => e.LocationObj).Where(e => e.Id == ca).SingleOrDefault();
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (data != "" && data != "0")
                {
                    var Corp_id = Convert.ToInt32(data);
                    var region_data = db.GeoStruct.Include(e => e.Location).Where(e =>
                        (e.Corporate.Id == Corp_id && e.Location != null) ||
                        (e.Region.Id == Corp_id && e.Location != null) ||
                        (e.Company.Id == Corp_id && e.Location != null) ||
                        (e.Division.Id == Corp_id && e.Location != null))
                        .Select(e => e.Location.Id).Distinct().ToList();
                    if (region_data != null && region_data.Count != 0)
                    {
                        foreach (var ca in region_data)
                        {
                            var a = db.Location.Include(e => e.LocationObj).Where(e => e.Id == ca).SingleOrDefault();
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
        public List<Department> Get_Department(String data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Department> Corporate_selectlist = new List<Department>();
                if (data == null)
                {
                    var Corporate_data = db.GeoStruct.Include(e => e.Department).Where(e => e.Department != null).Select(e => e.Department.Id).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.Department.Include(e => e.DepartmentObj).Where(e => e.Id == ca).SingleOrDefault();
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (data != "" && data != "0")
                {
                    var Corp_id = Convert.ToInt32(data);
                    var region_data = db.GeoStruct.Include(e => e.Department).Where(e =>
                        (e.Corporate.Id == Corp_id && e.Department != null) ||
                        (e.Region.Id == Corp_id && e.Department != null) ||
                        (e.Company.Id == Corp_id && e.Department != null) ||
                        (e.Division.Id == Corp_id && e.Department != null) ||
                        (e.Location.Id == Corp_id && e.Department != null))
                        .Select(e => e.Department.Id).Distinct().ToList();
                    if (region_data != null && region_data.Count != 0)
                    {
                        foreach (var ca in region_data)
                        {
                            var a = db.Department.Include(e => e.DepartmentObj).Where(e => e.Id == ca).SingleOrDefault();
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
        public List<Group> Get_Group(String data)
        {
            List<Group> Corporate_selectlist = new List<Group>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data == null)
                {
                    var Corporate_data = db.GeoStruct.Include(e => e.Group).Where(e => e.Group != null).Select(e => e.Group.Id).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.Group.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (data != "" && data != "0")
                {
                    var Corp_id = Convert.ToInt32(data);
                    var region_data = db.GeoStruct.Include(e => e.Group).Where(e =>
                        (e.Corporate.Id == Corp_id && e.Group != null) ||
                        (e.Region.Id == Corp_id && e.Group != null) ||
                        (e.Company.Id == Corp_id && e.Group != null) ||
                        (e.Division.Id == Corp_id && e.Group != null) ||
                        (e.Location.Id == Corp_id && e.Group != null) ||
                        (e.Department.Id == Corp_id && e.Group != null))
                        .Select(e => e.Group.Id).Distinct().ToList();
                    if (region_data != null && region_data.Count != 0)
                    {
                        foreach (var ca in region_data)
                        {
                            var a = db.Group.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
        public List<Unit> Get_Unit(String data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Unit> Corporate_selectlist = new List<Unit>();
                if (data == null)
                {
                    var Corporate_data = db.GeoStruct.Include(e => e.Unit).Where(e => e.Unit != null).Select(e => e.Unit.Id).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.Unit.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (data != "" && data != "0")
                {
                    var Corp_id = Convert.ToInt32(data);
                    var region_data = db.GeoStruct.Include(e => e.Unit).Where(e =>
                        (e.Corporate.Id == Corp_id && e.Unit != null) ||
                        (e.Region.Id == Corp_id && e.Unit != null) ||
                        (e.Company.Id == Corp_id && e.Unit != null) ||
                        (e.Division.Id == Corp_id && e.Unit != null) ||
                        (e.Location.Id == Corp_id && e.Unit != null) ||
                        (e.Department.Id == Corp_id && e.Unit != null) ||
                        (e.Group.Id == Corp_id && e.Unit != null))
                        .Select(e => e.Unit.Id).Distinct().ToList();
                    if (region_data != null && region_data.Count != 0)
                    {
                        foreach (var ca in region_data)
                        {
                            var a = db.Unit.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
        public Object ExitanceFirstlevelGeo()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var data = (String)null;
                var a = Get_Corporate(data);
                if (a != null)
                {
                    var selectlistjson = new
                    {
                        SelectlistType = "Corporate_drop",
                        selectlist = new SelectList(a, "Id", "Name", "")
                    };
                    return selectlistjson;
                }
                else
                {
                    var b = Get_Region(data);
                    if (b != null)
                    {
                        var selectlistjson = new
                        {
                            SelectlistType = "Region_drop",
                            selectlist = new SelectList(b, "Id", "Name", "")
                        };
                        return selectlistjson;
                    }
                    else
                    {
                        var c = Get_Company(data);
                        if (c != null)
                        {
                            var selectlistjson = new
                            {
                                SelectlistType = "Company_drop",
                                selectlist = new SelectList(c, "Id", "Name", "")
                            };
                            return selectlistjson;
                        }
                        else
                        {
                            var d = Get_Division(data);
                            if (d != null)
                            {
                                var selectlistjson = new
                                {
                                    SelectlistType = "Division_drop",
                                    selectlist = new SelectList(d, "Id", "Name", "")
                                };
                                return selectlistjson;
                            }
                            else
                            {
                                var e = Get_Location(data);
                                if (e != null)
                                {
                                    var selectlistjson = new
                                    {
                                        SelectlistType = "Location_drop",
                                        selectlist = new SelectList(e, "Id", "Name", "")
                                    };
                                    return selectlistjson;
                                }
                                else
                                {
                                    var f = Get_Department(data);
                                    if (f != null)
                                    {
                                        var selectlistjson = new
                                        {
                                            SelectlistType = "Department_drop",
                                            selectlist = new SelectList(f, "Id", "Name", "")
                                        };
                                        return selectlistjson;
                                    }
                                    else
                                    {
                                        var g = Get_Group(data);
                                        if (g != null)
                                        {
                                            var selectlistjson = new
                                            {
                                                SelectlistType = "Group_drop",
                                                selectlist = new SelectList(g, "Id", "Name", "")
                                            };
                                            return selectlistjson;
                                        }
                                        else
                                        {
                                            var h = Get_Unit(data);
                                            if (f != null)
                                            {
                                                var selectlistjson = new
                                                {
                                                    SelectlistType = "Unit_drop",
                                                    selectlist = new SelectList(h, "Id", "Name", "")
                                                };
                                                return selectlistjson;
                                            }
                                            else
                                            {
                                                return null;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public ActionResult PopulateDropDownListGeo(string data, string data2)
        {
            var a = ExitanceFirstlevelGeo();
            return Json(a, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PopulateDropDownListFun(string data, string data2)
        {
            var a = ExitanceFirstlevelFun();
            return Json(a, JsonRequestBehavior.AllowGet);
        }
        public Object DropDownListCheckLevelGeo(string data, string data2)
        {
            //var a = ExitanceFirstlevel();
            using (DataBaseContext db = new DataBaseContext())
            {
                switch (data2)
                {
                    case "Corporate":

                        var b = Get_Region(data);
                        if (b != null)
                        {
                            var selectlistjson = new
                            {
                                SelectlistType = "Region_drop",
                                selectlist = new SelectList(b, "Id", "Name", "")
                            };
                            return selectlistjson;
                        }
                        else
                        {
                            var c = Get_Company(data);
                            if (c != null)
                            {
                                var selectlistjson = new
                                {
                                    SelectlistType = "Company_drop",
                                    selectlist = new SelectList(c, "Id", "Name", "")
                                };
                                return selectlistjson;
                            }
                            else
                            {
                                var d = Get_Division(data);
                                if (d != null)
                                {
                                    var selectlistjson = new
                                    {
                                        SelectlistType = "Division_drop",
                                        selectlist = new SelectList(d, "Id", "Name", "")
                                    };
                                    return selectlistjson;
                                }
                                else
                                {
                                    var e = Get_Location(data);
                                    if (e != null)
                                    {
                                        var selectlistjson = new
                                        {
                                            SelectlistType = "Location_drop",
                                            selectlist = new SelectList(e, "Id", "FullDetails", "")
                                        };
                                        return selectlistjson;
                                    }
                                    else
                                    {
                                        var f = Get_Department(data);
                                        if (f != null)
                                        {
                                            var selectlistjson = new
                                            {
                                                SelectlistType = "Department_drop",
                                                selectlist = new SelectList(f, "Id", "FullDetails", "")
                                            };
                                            return selectlistjson;
                                        }
                                        else
                                        {
                                            var g = Get_Group(data);
                                            if (g != null)
                                            {
                                                var selectlistjson = new
                                                {
                                                    SelectlistType = "Group_drop",
                                                    selectlist = new SelectList(g, "Id", "Name", "")
                                                };
                                                return selectlistjson;
                                            }
                                            else
                                            {
                                                var h = Get_Unit(data);
                                                if (h != null)
                                                {
                                                    var selectlistjson = new
                                                    {
                                                        SelectlistType = "Unit_drop",
                                                        selectlist = new SelectList(h, "Id", "Name", "")
                                                    };
                                                    return selectlistjson;
                                                }
                                                else
                                                {
                                                    return null;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case "Region":
                        var i = Get_Company(data);
                        if (i != null)
                        {
                            var selectlistjson = new
                            {
                                SelectlistType = "Company_drop",
                                selectlist = new SelectList(i, "Id", "Name", "")
                            };
                            return selectlistjson;
                        }
                        else
                        {
                            var j = Get_Division(data);
                            if (j != null)
                            {
                                var selectlistjson = new
                                {
                                    SelectlistType = "Division_drop",
                                    selectlist = new SelectList(j, "Id", "Name", "")
                                };
                                return selectlistjson;
                            }
                            else
                            {
                                var k = Get_Location(data);
                                if (k != null)
                                {
                                    var selectlistjson = new
                                    {
                                        SelectlistType = "Location_drop",
                                        selectlist = new SelectList(k, "Id", "FullDetails", "")
                                    };
                                    return selectlistjson;
                                }
                                else
                                {
                                    var l = Get_Department(data);
                                    if (l != null)
                                    {
                                        var selectlistjson = new
                                        {
                                            SelectlistType = "Department_drop",
                                            selectlist = new SelectList(l, "Id", "FullDetails", "")
                                        };
                                        return selectlistjson;
                                    }
                                    else
                                    {
                                        var m = Get_Group(data);
                                        if (m != null)
                                        {
                                            var selectlistjson = new
                                            {
                                                SelectlistType = "Group_drop",
                                                selectlist = new SelectList(m, "Id", "Name", "")
                                            };
                                            return selectlistjson;
                                        }
                                        else
                                        {
                                            var n = Get_Unit(data);
                                            if (n != null)
                                            {
                                                var selectlistjson = new
                                                {
                                                    SelectlistType = "Unit_drop",
                                                    selectlist = new SelectList(n, "Id", "Name", "")
                                                };
                                                return selectlistjson;
                                            }
                                            else
                                            {
                                                return null;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case "Company":
                        var o = Get_Division(data);
                        if (o != null)
                        {
                            var selectlistjson = new
                            {
                                SelectlistType = "Division_drop",
                                selectlist = new SelectList(o, "Id", "Name", "")
                            };
                            return selectlistjson;
                        }
                        else
                        {
                            var p = Get_Location(data);
                            if (p != null)
                            {
                                var selectlistjson = new
                                {
                                    SelectlistType = "Location_drop",
                                    selectlist = new SelectList(p, "Id", "FullDetails", "")
                                };
                                return selectlistjson;
                            }
                            else
                            {
                                var q = Get_Department(data);
                                if (q != null)
                                {
                                    var selectlistjson = new
                                    {
                                        SelectlistType = "Department_drop",
                                        selectlist = new SelectList(q, "Id", "FullDetails", "")
                                    };
                                    return selectlistjson;
                                }
                                else
                                {
                                    var r = Get_Group(data);
                                    if (r != null)
                                    {
                                        var selectlistjson = new
                                        {
                                            SelectlistType = "Group_drop",
                                            selectlist = new SelectList(r, "Id", "Name", "")
                                        };
                                        return selectlistjson;
                                    }
                                    else
                                    {
                                        var s = Get_Unit(data);
                                        if (s != null)
                                        {
                                            var selectlistjson = new
                                            {
                                                SelectlistType = "Unit_drop",
                                                selectlist = new SelectList(s, "Id", "Name", "")
                                            };
                                            return selectlistjson;
                                        }
                                        else
                                        {
                                            return null;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case "Division":
                        var t = Get_Location(data);
                        if (t != null)
                        {
                            var selectlistjson = new
                            {
                                SelectlistType = "Location_drop",
                                selectlist = new SelectList(t, "Id", "FullDetails", "")
                            };
                            return selectlistjson;
                        }
                        else
                        {
                            var x = Get_Department(data);
                            if (x != null)
                            {
                                var selectlistjson = new
                                {
                                    SelectlistType = "Department_drop",
                                    selectlist = new SelectList(x, "Id", "FullDetails", "")
                                };
                                return selectlistjson;
                            }
                            else
                            {
                                var y = Get_Group(data);
                                if (y != null)
                                {
                                    var selectlistjson = new
                                    {
                                        SelectlistType = "Group_drop",
                                        selectlist = new SelectList(y, "Id", "Name", "")
                                    };
                                    return selectlistjson;
                                }
                                else
                                {
                                    var z = Get_Unit(data);
                                    if (z != null)
                                    {
                                        var selectlistjson = new
                                        {
                                            SelectlistType = "Unit_drop",
                                            selectlist = new SelectList(z, "Id", "Name", "")
                                        };
                                        return selectlistjson;
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                            }
                        }
                        break;
                    case "Location":
                        var x1 = Get_Department(data);
                        if (x1 != null)
                        {
                            var selectlistjson = new
                            {
                                SelectlistType = "Department_drop",
                                selectlist = new SelectList(x1, "Id", "FullDetails", "")
                            };
                            return selectlistjson;
                        }
                        else
                        {
                            var y1 = Get_Group(data);
                            if (y1 != null)
                            {
                                var selectlistjson = new
                                {
                                    SelectlistType = "Group_drop",
                                    selectlist = new SelectList(y1, "Id", "Name", "")
                                };
                                return selectlistjson;
                            }
                            else
                            {
                                var z1 = Get_Unit(data);
                                if (z1 != null)
                                {
                                    var selectlistjson = new
                                    {
                                        SelectlistType = "Unit_drop",
                                        selectlist = new SelectList(z1, "Id", "Name", "")
                                    };
                                    return selectlistjson;
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        }
                        break;
                    case "Department":
                        var y11 = Get_Group(data);
                        if (y11 != null)
                        {
                            var selectlistjson = new
                            {
                                SelectlistType = "Group_drop",
                                selectlist = new SelectList(y11, "Id", "Name", "")
                            };
                            return selectlistjson;
                        }
                        else
                        {
                            var z11 = Get_Unit(data);
                            if (z11 != null)
                            {
                                var selectlistjson = new
                                {
                                    SelectlistType = "Unit_drop",
                                    selectlist = new SelectList(z11, "Id", "Name", "")
                                };
                                return selectlistjson;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        break;
                    case "Group":
                        var z111 = Get_Unit(data);
                        if (z111 != null)
                        {
                            var selectlistjson = new
                            {
                                SelectlistType = "Unit_drop",
                                selectlist = new SelectList(z111, "Id", "Name", "")
                            };
                            return selectlistjson;
                        }
                        else
                        {
                            return null;
                        }
                        break;
                    default:
                        return null;
                        break;
                }
            }
        }

        public ActionResult PopulateDropDownlistOnChangeGeo(string data, string data2)
        {
            var a = DropDownListCheckLevelGeo(data, data2);

            return Json(a, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PopulateDropDownlistOnChangeFun(string data, string data2)
        {
            var a = DropDownListCheckLevelFun(data, data2);
            return Json(a, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PopulateCorporateList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.Corporate.ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Corporate.Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateRegionList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.Region.ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Region.Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateCompanyList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.Company.ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Company.Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateDivisionList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.Division.ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Division.Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateLocationList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.Location.Include(e => e.LocationObj).ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Location.Include(e => e.LocationObj).Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateDepartmentList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Department.Include(e => e.DepartmentObj).ToList();
                var selected = (String)null;
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Department.Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateGroupList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.Group.ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Group.Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateUnitList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Unit.ToList();
                var selected = (String)null;
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Unit.Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateGradeList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.Grade.ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Grade.Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateLevelList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.Level.ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Level.Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateJobStatusList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.JobStatus.ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.JobStatus.Include(e => e.EmpActingStatus).Include(e => e.EmpStatus).Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PopulateJobNameList1(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.Job.ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Job.Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PopulateJobNameList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                String selected = null;
                var qurey = db.Job.ToList();
                if (data != "" && data != null && data != "0")
                {
                    var filter = Convert.ToInt32(data);

                    if (data2 != "" && data2 != null && data2 != "0")
                    {
                        selected = data2;
                    }

                    SelectList s = new SelectList(qurey, "Id", "Name", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (data2 != "")
                    {
                        selected = data2;
                    }
                    SelectList s = new SelectList(qurey, "Id", "Name", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }

            }
        }


        public ActionResult PopulatePositionList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.JobPosition.ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.JobPosition.Where(e => e.Id == id).ToList();
                }

                SelectList s = new SelectList(qurey, "Id", "JobPositionDesc", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }


        private object DropDownListCheckLevelFun(string data, string data2)
        {
            switch (data2)
            {
                case "Job":
                    var a = Get_Job(data);
                    if (a != null)
                    {
                        var selectlistjson = new
                        {
                            SelectlistType = "Job_drop",
                            selectlist = new SelectList(a, "Id", "Name", "")
                        };
                        return selectlistjson;
                    }
                    else
                    {
                        //var c = DropDownListCheckLevelFun(data, "JobPosition");
                        //if (c != null)
                        //{
                        //    return c;
                        //}
                        var b = Get_JobPosition(data);
                        if (b != null)
                        {
                            var selectlistjson = new
                            {
                                SelectlistType = "JobPosition_drop",
                                selectlist = new SelectList(b, "Id", "JobPositionDesc", "")
                            };
                            return selectlistjson;
                        }
                        else
                        {
                            return null;
                        }
                    }
                 //   break;
                //case "JobPosition":
                //    var b = Get_JobPosition(data);
                //    if (b != null)
                //    {
                //        var selectlistjson = new
                //        {
                //            SelectlistType = "JobPosition_drop",
                //            selectlist = new SelectList(b, "Id", "JobPositionDesc", "")
                //        };
                //        return selectlistjson;
                //    }
                //    else
                //    {
                //        return null;
                //    }
                //    break;
                default:
                    return null;
                    break;
            }
            return null;
        }
    }
}
