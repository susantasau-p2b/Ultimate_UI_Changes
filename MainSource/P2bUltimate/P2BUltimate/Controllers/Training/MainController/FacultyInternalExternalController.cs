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
using System.Data.Entity.Core.Objects;
using System.Reflection;
using Training;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Training.MainController
{
    public class FacultyInternalExternalController : Controller
    {
        // GET: FacultyInternalExternal
        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/FacultyInternalExternal/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Training/_FacultyInternalExternal.cshtml");
        }
        List<string> Msg = new List<string>();

        //private DataBaseContext db = new DataBaseContext();

        [HttpPost]
        public ActionResult Create(FacultyInternalExternal c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    string Category = form["CategoryList_DDL"] == "0" ? "" : form["CategoryList_DDL"];
                    string Addrs = form["Address_List"] == "0" ? "" : form["Address_List"];
                    string ContactDetails = form["Contact_List"] == "0" ? "" : form["Contact_List"];
                    string employee = form["Employee-Table"] == "0" ? null : form["Employee-Table"];
                    string Specilisation = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
                    string insti = form["TrainingInstitutelist"] == "0" ? "" : form["TrainingInstitutelist"];
                    string TrainingType = form["ProgramListlist"] == "0" ? "" : form["ProgramListlist"];

                    if (TrainingType == null)
                    {
                        TrainingType = form["ProgramListlistS"] == "0" ? "" : form["ProgramListlistS"];
                    }

                    var id = int.Parse(Session["CompId"].ToString());
                    var companypayroll = db.CompanyTraining.Where(e => e.Company.Id == id).SingleOrDefault();


                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Category));
                            c.FacultyType = val;
                        }
                    }
                    var vala = db.Employee.Where(a => a.EmpCode == c.Code).SingleOrDefault();
                    var facultyinternal_Id = db.FacultyInternalExternal.Where(e => e.Code == c.Code).FirstOrDefault();
                    if (c.FacultyType.LookupVal.ToUpper() == "INTERNAL")
                    {
                        if (facultyinternal_Id != null)
                        {
                            Msg.Add("Employee with this Code Already Exists.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        c.EmpID = vala;
                        //empId = Convert.ToInt32(employee);
                        //var vals = db.Employee.Where(e => e.Id == empId).SingleOrDefault();
                    }
                    else
                    {
                        if (vala != null)
                        {
                            Msg.Add("  Employee with this Code Already Exists.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    //if (employee != null && employee != "")
                    //{
                    //    empId = Convert.ToInt32(employee);
                    //    var vals = db.Employee.Where(e => e.Id == empId).SingleOrDefault();
                    //    c.EmpID = vals;
                    //}
                    //else
                    //{
                    //    Msg.Add("  Kindly select employee  ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    //var empval = db.FacultyInternalExternal.Include(e => e.EmpID).Any(q => q.EmpID.Id == empId);
                    //if (empval == true)
                    //{
                    //    Msg.Add("  Record Already Exists For This Employee.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}





                    if (Specilisation != null)
                    {
                        c.FacultySpecialization = new List<FacultySpecialization>();
                        List<int> IDs = Specilisation.Split(',').Select(e => int.Parse(e)).ToList();
                        foreach (var k in IDs)
                        {
                            var value = db.FacultySpecialization.Find(k);
                            c.FacultySpecialization.Add(value);
                        }
                    }
                    if (Addrs != null && Addrs != "")
                    {
                        int AddId = Convert.ToInt32(Addrs);
                        var val = db.Address.Include(e => e.Area)
                                            .Include(e => e.City)
                                            .Include(e => e.Country)
                                            .Include(e => e.District)
                                            .Include(e => e.State)
                                            .Include(e => e.StateRegion)
                                            .Include(e => e.Taluka)
                                            .Where(e => e.Id == AddId).SingleOrDefault();
                        c.Address = val;
                    }

                    if (ContactDetails != null && ContactDetails != "")
                    {
                        int ContId = Convert.ToInt32(ContactDetails);
                        var val = db.ContactDetails.Include(e => e.ContactNumbers)
                                            .Where(e => e.Id == ContId).SingleOrDefault();
                        c.ContactDetails = val;
                    }
                    if (insti != null && insti != "")
                    {
                        int ContId = Convert.ToInt32(insti);
                        var val = db.TrainingInstitute.Include(e => e.Address).Include(e => e.ContactDetails).Include(e => e.InstituteType)
                                            .Where(e => e.Id == ContId).SingleOrDefault();
                        c.TrainingInstitue = val;
                    }

                    if (TrainingType != null)
                    {
                        if (TrainingType != "")
                        {
                            var ids = Utility.StringIdsToListIds(TrainingType);
                            List<ProgramList> SubCategoryList = new List<ProgramList>();
                            foreach (var item in ids)
                            {
                                var val = db.ProgramList.Find(item);
                                SubCategoryList.Add(val);
                            }
                            c.ProgramList = SubCategoryList;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            FacultyInternalExternal FacultyInternal = new FacultyInternalExternal()
                            {
                                Code = c.Code == null ? "" : c.Code.Trim(),
                                Name = c.Name == null ? "" : c.Name.Trim(),
                                FacultyType = c.FacultyType,
                                EmpID = c.EmpID,
                                Address = c.Address,
                                FacultySpecialization = c.FacultySpecialization,
                                ContactDetails = c.ContactDetails,
                                TrainingInstitue = c.TrainingInstitue,
                                ProgramList = c.ProgramList,
                                Narration = c.Narration,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.FacultyInternalExternal.Add(FacultyInternal);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, c.DBTrack);
                                //DT_FacultyInternalExternal DT_Corp = (DT_FacultyInternalExternal)rtn_Obj;
                                //DT_Corp.Address_Id = c.Address == null ? 0 : c.Address.Id;
                                //DT_Corp.FacultyType_Id = c.FacultyType == null ? 0 : c.FacultyType.Id;
                                //DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                                //DT_Corp.TrainingInstitue_Id = c.TrainingInstitue == null ? 0 : c.TrainingInstitue.Id;
                                //DT_Corp.EmpID_Id = c.EmpID == null ? 0 : c.EmpID.Id;
                                //db.Create(DT_Corp);
                                db.SaveChanges();

                                if (companypayroll != null)
                                {
                                    List<FacultyInternalExternal> pfmasterlist = new List<FacultyInternalExternal>();
                                    pfmasterlist.Add(FacultyInternal);
                                    companypayroll.FacultyInternalExternal = pfmasterlist;
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Detached;
                                }

                                ts.Complete();

                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = FacultyInternal.Id, Val = FacultyInternal.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            }
                            catch (DataException /* dex */)
                            {
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
                        return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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

        public class EmpSocialInfo_SA
        {
            public Array SA_id { get; set; }
            public Array SA_val { get; set; }
        }
        public class ProgramList1
        {
            public Array ProgramList_Id { get; set; }
            public Array ProgramList_val { get; set; }

        }
        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<EmpSocialInfo_SA> return_data = new List<EmpSocialInfo_SA>();
                var EmpSocialInfo = db.FacultyInternalExternal.Include(e => e.Address).Include(e => e.ContactDetails)
                                    .Include(e => e.EmpID)
                                      .Include(e => e.EmpID.EmpName)
                                    .Include(e => e.FacultySpecialization)
                                    .Include(e => e.TrainingInstitue)
                                      .Include(e => e.FacultyType)
                                    .Where(e => e.Id == data).ToList();


                var r = (from ca in EmpSocialInfo
                         select new
                         {
                             Id = ca.Id,
                             Code = ca.Code,
                             Name = ca.Name,
                             FacultyType_Id = ca.FacultyType == null ? 0 : ca.FacultyType.Id,
                             FacultyType_Name = ca.FacultyType == null ? "" : ca.FacultyType.LookupVal.ToString(),

                             Narration = ca.Narration,
                             Action = ca.DBTrack.Action
                         }).Distinct();

                var a = db.FacultyInternalExternal.Include(e => e.FacultySpecialization).Where(e => e.Id == data).Select(e => e.FacultySpecialization).ToList();


                var add_data = db.FacultyInternalExternal.Include(e => e.Address).Include(e => e.Address.Area)
                    .Include(e => e.Address.City)
                    .Include(e => e.Address.Country)
                    .Include(e => e.Address.District)
                    .Include(e => e.Address.State)
                    .Include(e => e.Address.StateRegion)
                    .Include(e => e.Address.Taluka).Include(e => e.ContactDetails)
                    .Include(e => e.EmpID)
                     .Include(e => e.EmpID.EmpName.FullNameFML)
                     .Include(e => e.FacultySpecialization)
                     .Include(e => e.TrainingInstitue)
                     .Include(e => e.FacultyType)
                      .Include(e => e.ProgramList)
                                    .Where(e => e.Id == data)

                  .Select(e => new
                  {
                      Address_FullAddress = e.Address.FullAddress == null ? "" : e.Address.FullAddress,
                      Add_Id = e.Address.Id == null ? "" : e.Address.Id.ToString(),
                      Cont_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
                      FullContactDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails,
                      Train_Id = e.TrainingInstitue == null ? "" : e.TrainingInstitue.Id.ToString(),
                      Train_Details = e.TrainingInstitue.FullDetails == null ? "" : e.TrainingInstitue.FullDetails,


                  }).ToList();

                List<ProgramList1> fext = new List<ProgramList1>();


                var add_data1 = db.FacultyInternalExternal
                               .Include(e => e.ProgramList)
                    //.Include(e => e.Budget)


                             .Where(e => e.Id == data).ToList();



                foreach (var ca in add_data1)
                {
                    fext.Add(new ProgramList1
                    {
                        ProgramList_Id = ca.ProgramList.Select(e => e.Id.ToString()).ToArray(),
                        ProgramList_val = ca.ProgramList.Select(e => e.FullDetails).ToArray(),
                        //Budget_Id = ca.Budget.Select(e => e.Id.ToString()).ToArray(),
                        //Budget_val = ca.Budget.Select(e => e.FullDetails.ToString()).ToArray()
                    });
                }

                foreach (var ca in a)
                {
                    return_data.Add(
                new EmpSocialInfo_SA
                {
                    SA_id = ca.Select(e => e.Id.ToString()).ToArray(),
                    SA_val = ca.Select(e => e.FullDetails).ToArray()
                });
                }




                var LKup = db.FacultyInternalExternal.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;
                return this.Json(new Object[] { r, add_data, "", return_data, fext, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        /* ---------------------------- Details Contact -----------------------*/
        [HttpPost]
        public async Task<ActionResult> EditSave(FacultyInternalExternal c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            int Category = 0;
            string Addrs = form["Address_List"] == "0" ? "" : form["Address_List"];
            string ContactDetails = form["Contact_List"] == "0" ? "" : form["Contact_List"];
            string employee = form["Employee-Table"] == "0" ? null : form["Employee-Table"];
            string Specilisation = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
            string insti = form["TrainingInstitutelist"] == "0" ? "" : form["TrainingInstitutelist"];
            string ProgramList = form["ProgramListlistS"] == "0" ? "" : form["ProgramListlistS"];

            c.TrainingInstitue_Id = insti != null && insti != "" ? int.Parse(insti) : 0;
            c.Address_Id = Addrs != null && Addrs != "" ? int.Parse(Addrs) : 0;
            c.ContactDetails_Id = ContactDetails != null && ContactDetails != "" ? int.Parse(ContactDetails) : 0;

            string Facultytype = form["CategoryList_DDL"] == "0" ? "" : form["CategoryList_DDL"];

            c.FacultyType_Id = Facultytype != null && Facultytype != "" ? int.Parse(Facultytype) : 0;



            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.FacultyInternalExternal.Include(e => e.ProgramList).Include(e => e.FacultySpecialization).Where(e => e.Id == data).FirstOrDefault();
                        List<ProgramList> ProgramListDet = new List<ProgramList>();
                        List<FacultySpecialization> FacultySpe = new List<FacultySpecialization>();

                        if (ProgramList != null)
                        {
                            var ids = Utility.StringIdsToListIds(ProgramList);
                            foreach (var ca in ids)
                            {
                                var ProgramList_val = db.ProgramList.Find(ca);
                                ProgramListDet.Add(ProgramList_val);
                                db_data.ProgramList = ProgramListDet;
                            }
                        }
                        else
                        {
                            db_data.ProgramList = null;
                        }

                        if (Specilisation != null)
                        {
                            var ids = Utility.StringIdsToListIds(Specilisation);
                            foreach (var ca in ids)
                            {
                                var FacultySpe_val = db.FacultySpecialization.Find(ca);
                                FacultySpe.Add(FacultySpe_val);
                                db_data.FacultySpecialization = FacultySpe;
                            }
                        }
                        else
                        {
                            db_data.FacultySpecialization = null;
                        }


                        db.FacultyInternalExternal.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;

                        FacultyInternalExternal FacultyIE = db.FacultyInternalExternal.Find(data);
                        TempData["CurrRowVersion"] = FacultyIE.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = FacultyIE.DBTrack.CreatedBy == null ? null : FacultyIE.DBTrack.CreatedBy,
                                CreatedOn = FacultyIE.DBTrack.CreatedOn == null ? null : FacultyIE.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            FacultyIE.Name = c.Name;
                            FacultyIE.Code = c.Code;
                            FacultyIE.Narration = c.Narration;
                            FacultyIE.Id = data;
                            FacultyIE.DBTrack = c.DBTrack;

                            if (c.ContactDetails_Id !=0)
                            {
                                FacultyIE.ContactDetails_Id = c.ContactDetails_Id;
                                
                            }
                            if (c.Address_Id != 0)
                            {
                                FacultyIE.Address_Id = c.Address_Id;     
                            }
                            if (c.TrainingInstitue_Id != 0)
                            {
                                FacultyIE.TrainingInstitue_Id = c.TrainingInstitue_Id;
                            }

                            if (c.FacultyType_Id != 0)
                            {
                                FacultyIE.FacultyType_Id = c.FacultyType_Id;
                            }
                           
                            
                            



                            db.Entry(FacultyIE).State = System.Data.Entity.EntityState.Modified;
                            // db.SaveChanges();

                            //using (var context = new DataBaseContext())
                            //{
                            FacultyInternalExternal blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;

                            blog = db.FacultyInternalExternal.Where(e => e.Id == data).Include(e => e.TrainingInstitue)
                                                    .Include(e => e.ContactDetails)
                                                    .Include(e => e.Address)
                                                    .Include(e => e.FacultyType).SingleOrDefault();
                            originalBlogValues = db.Entry(blog).OriginalValues;
                            db.ChangeTracker.DetectChanges();
                            var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
                            DT_FacultyInternalExternal DT_Corp = (DT_FacultyInternalExternal)obj;
                            DT_Corp.FacultyType_Id = blog.FacultyType == null ? 0 : blog.FacultyType.Id;
                            DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
                            DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
                            DT_Corp.TrainingInstitue_Id = blog.TrainingInstitue == null ? 0 : blog.TrainingInstitue.Id;

                            db.Create(DT_Corp);
                            db.SaveChanges();
                        }
                        //}
                        await db.SaveChangesAsync();
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
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
                IEnumerable<FacultyInternalExternal> Corporate = null;
                if (gp.IsAutho == true)
                {
                    Corporate = db.FacultyInternalExternal.Include(e => e.FacultyType).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Corporate = db.FacultyInternalExternal.Include(e => e.FacultyType).AsNoTracking().ToList();
                }

                IEnumerable<FacultyInternalExternal> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = Corporate;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                               || (e.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.FacultyType != null ? e.FacultyType.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.Code, a.Name, a.FacultyType != null ? a.FacultyType.LookupVal : "", a.Id }).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.FacultyType) != null ? Convert.ToString(a.FacultyType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.FacultyType != null ? Convert.ToString(a.FacultyType.LookupVal) : "", a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Corporate;
                    Func<FacultyInternalExternal, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.Code :
                                         gp.sidx == "Name" ? c.Name :
                                         gp.sidx == "FacultyType" ? c.FacultyType.LookupVal : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.FacultyType != null ? Convert.ToString(a.FacultyType.LookupVal) : "", a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.FacultyType != null ? Convert.ToString(a.FacultyType.LookupVal) : "", a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.FacultyType != null ? Convert.ToString(a.FacultyType.LookupVal) : "", a.Id }).ToList();
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

        public int EditS(int Category, string Addrs, string ContactDetails, string insti, string Specilisation, int data, FacultyInternalExternal c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Category != 0)
                {
                    var val = db.LookupValue.Find(Category);
                    c.FacultyType = val;

                    var type = db.FacultyInternalExternal.Include(e => e.FacultyType).Where(e => e.Id == data).SingleOrDefault();
                    IList<FacultyInternalExternal> typedetails = null;
                    if (type.FacultyType != null)
                    {
                        typedetails = db.FacultyInternalExternal.Where(x => x.FacultyType.Id == type.FacultyType.Id && x.Id == data).ToList();
                    }
                    else
                    {
                        typedetails = db.FacultyInternalExternal.Where(x => x.Id == data).ToList();
                    }
                    //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                    foreach (var s in typedetails)
                    {
                        s.FacultyType = c.FacultyType;
                        db.FacultyInternalExternal.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                else
                {
                    var InstituteTypeDetails = db.FacultyInternalExternal.Include(e => e.FacultyType).Where(x => x.Id == data).ToList();
                    foreach (var s in InstituteTypeDetails)
                    {
                        s.FacultyType = null;
                        db.FacultyInternalExternal.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }




                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        var val = db.Address.Find(int.Parse(Addrs));
                        c.Address = val;

                        var add = db.FacultyInternalExternal.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                        IList<FacultyInternalExternal> addressdetails = null;
                        if (add.Address != null)
                        {
                            addressdetails = db.FacultyInternalExternal.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.FacultyInternalExternal.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.Address = c.Address;
                                db.FacultyInternalExternal.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                // await db.SaveChangesAsync(false);
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                }
                else
                {
                    var addressdetails = db.FacultyInternalExternal.Include(e => e.Address).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.Address = null;
                        db.FacultyInternalExternal.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (ContactDetails != null)
                {
                    if (ContactDetails != "")
                    {
                        var val = db.ContactDetails.Find(int.Parse(ContactDetails));
                        c.ContactDetails = val;

                        var add = db.FacultyInternalExternal.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                        IList<FacultyInternalExternal> contactsdetails = null;
                        if (add.ContactDetails != null)
                        {
                            contactsdetails = db.FacultyInternalExternal.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            contactsdetails = db.FacultyInternalExternal.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.ContactDetails = c.ContactDetails;
                            db.FacultyInternalExternal.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var contactsdetails = db.FacultyInternalExternal.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.ContactDetails = null;
                        db.FacultyInternalExternal.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }



                //var db_data = db.FacultyInternalExternal.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                //List<ContactDetails> lookupval = new List<ContactDetails>();
                //string Values = OBJ;

                //if (Values != null)
                //{
                //    var ids = Utility.StringIdsToListIds(Values);
                //    foreach (var ca in ids)
                //    {
                //        var Lookup_val = db.ContactDetails.Find(ca);
                //        lookupval.Add(Lookup_val);
                //        db_data.ContactDetails = lookupval;
                //    }
                //}
                //else
                //{
                //    db_data.ContactDetails = null;
                //}

                if (insti != null)
                {
                    if (insti != "")
                    {
                        var val = db.TrainingInstitute.Find(int.Parse(insti));
                        c.TrainingInstitue = val;

                        var add = db.FacultyInternalExternal.Include(e => e.TrainingInstitue).Where(e => e.Id == data).SingleOrDefault();
                        IList<FacultyInternalExternal> addressdetails = null;
                        if (add.Address != null)
                        {
                            addressdetails = db.FacultyInternalExternal.Where(x => x.TrainingInstitue.Id == add.TrainingInstitue.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.FacultyInternalExternal.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.TrainingInstitue = c.TrainingInstitue;
                                db.FacultyInternalExternal.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                // await db.SaveChangesAsync(false);
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                }
                else
                {
                    var addressdetails = db.FacultyInternalExternal.Include(e => e.TrainingInstitue).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.TrainingInstitue = null;
                        db.FacultyInternalExternal.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                //db.FacultyInternalExternal.Attach(db_data);
                //db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
                //TempData["RowVersion"] = db_data.RowVersion;
                //db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                //List<FacultySpecialization> ObjITQuarter = new List<FacultySpecialization>();

                //VAR ITdetails = db.FacultyInternalExternal.Include(e => e.FacultySpecialization).Where(e => e.Id == data).SingleOrDefault();
                //if (Quarterlist != null && Quarterlist != "")
                //{
                //    var ids = Utility.StringIdsToListIds(Quarterlist);
                //    foreach (var ca in ids)
                //    {
                //        var ITQuarterlist = db.ITForm16Quarter.Find(ca);
                //        ObjITQuarter.Add(ITQuarterlist);
                //        ITdetails.ITForm16Quarter = ObjITQuarter;
                //    }
                //}
                //else
                //{
                //    ITdetails.ITForm16Quarter = null;
                //}

                if (Specilisation != null)
                {
                    var ids = Utility.StringIdsToListIds(Specilisation);
                    List<FacultySpecialization> HolidayList = new List<FacultySpecialization>();
                    foreach (var item in ids)
                    {

                        int HolidayListid = Convert.ToInt32(item);
                        var val = db.FacultySpecialization.Where(e => e.Id == HolidayListid).SingleOrDefault();
                        if (val != null)
                        {
                            HolidayList.Add(val);
                        }
                    }
                    c.FacultySpecialization = HolidayList;
                }

                else
                {
                    c.FacultySpecialization = null;
                }

                var CurOBJ = db.FacultyInternalExternal.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    FacultyInternalExternal BOBJ = new FacultyInternalExternal()
                    {
                        Name = c.Name,
                        Code = c.Code,
                        Narration = c.Narration,
                        Id = data,
                        DBTrack = c.DBTrack,
                        FacultySpecialization = c.FacultySpecialization,
                        ContactDetails = c.ContactDetails,
                        Address = c.Address,
                        TrainingInstitue = c.TrainingInstitue
                    };


                    db.FacultyInternalExternal.Attach(BOBJ);
                    db.Entry(BOBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(BOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    ////  DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
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
                    FacultyInternalExternal Postdetails = db.FacultyInternalExternal
                     .Include(e => e.Address)
                     .Include(e => e.ContactDetails)
                                 .Include(e => e.EmpID)
                                   .Include(e => e.EmpID.EmpName)
                                 .Include(e => e.FacultySpecialization)
                                 .Include(e => e.TrainingInstitue)
                                   .Include(e => e.FacultyType)
                                   .Where(e => e.Id == data).SingleOrDefault();

                    Employee EmpID = Postdetails.EmpID;
                    TrainingInstitute TrainingInstitue = Postdetails.TrainingInstitue;
                    LookupValue FacultyType = Postdetails.FacultyType;
                    if (Postdetails.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = Postdetails.DBTrack.CreatedBy != null ? Postdetails.DBTrack.CreatedBy : null,
                                CreatedOn = Postdetails.DBTrack.CreatedOn != null ? Postdetails.DBTrack.CreatedOn : null,
                                IsModified = Postdetails.DBTrack.IsModified == true ? true : false
                            };
                            Postdetails.DBTrack = dbT;
                            db.Entry(Postdetails).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, Postdetails.DBTrack);
                            DT_FacultyInternalExternal DT_Post = (DT_FacultyInternalExternal)rtn_Obj;
                            db.Create(DT_Post);

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
                            var lkValue = new HashSet<int>(Postdetails.FacultySpecialization.Select(e => e.Id));
                            if (lkValue.Count > 0)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                            }

                            var lkValue1 = Postdetails.TrainingInstitue;

                            if (lkValue1 != null)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            var lkValue2 = Postdetails.Address;
                            if (lkValue2 != null)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            //    var corpRegion = new HashSet<int>(Postdetails.FuncStruct.JobPosition.Id.CompareTo();
                            var lkValue3 = Postdetails.ContactDetails;

                            if (lkValue3 != null)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = Postdetails.DBTrack.CreatedBy != null ? Postdetails.DBTrack.CreatedBy : null,
                                CreatedOn = Postdetails.DBTrack.CreatedOn != null ? Postdetails.DBTrack.CreatedOn : null,
                                IsModified = Postdetails.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                            db.Entry(Postdetails).State = System.Data.Entity.EntityState.Deleted;
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, dbT);
                            // var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            // DT_FacultyInternalExternal DT_Post = (DT_FacultyInternalExternal)rtn_Obj;

                            //DT_Post.EmpID_Id = EmpID == null ? 0 : EmpID.Id;
                            //DT_Post.TrainingInstitue_Id = TrainingInstitue == null ? 0 : TrainingInstitue.Id;                                                             // the declaratn for lookup is remain 
                            //DT_Post.FacultyType_Id = FacultyType == null ? 0 : FacultyType.Id;

                            //db.Create(DT_Post);

                            await db.SaveChangesAsync();

                            ts.Complete();
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed.  ");                                                                                             // the original place 
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
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


        public ActionResult getemp(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int id = int.Parse(data);
                var empcode = db.Employee.Include(a => a.EmpName.FullNameFML).Where(a => a.Id == id).Select(a => new
                {
                    Id = a.EmpCode,
                    Name = a.EmpName.FullNameFML
                })
                    .SingleOrDefault();
                return Json(empcode, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetLookupDetailsContact(string data, string EmpCode)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<ContactDetails> AllContact = new List<ContactDetails>();

                //var fall = db.ContactDetails.Include(e => e.ContactNumbers).ToList();
                if (EmpCode != "")
                {
                    var fall1 = db.Employee.Include(e => e.PerContact)
                 .Include(e => e.PerContact.ContactNumbers).Where(e => e.EmpCode == EmpCode).FirstOrDefault().PerContact;

                    var fall2 = db.Employee.Include(e => e.CorContact)
                        .Include(e => e.CorContact.ContactNumbers).Where(e => e.EmpCode == EmpCode).FirstOrDefault().CorContact;

                    var fall3 = db.Employee.Include(e => e.ResContact)
                        .Include(e => e.ResContact.ContactNumbers).Where(e => e.EmpCode == EmpCode).FirstOrDefault().ResContact;


                    if (fall1 != null)
                    {
                        AllContact.Add(fall1);
                    }
                    if (fall2 != null)
                    {
                        AllContact.Add(fall2);
                    }
                    if (fall3 != null)
                    {
                        AllContact.Add(fall3);
                    }
                }
                IEnumerable<ContactDetails> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.ContactDetails.ToList().Where(d => d.FullContactDetails.Contains(data));

                }
                else
                {
                    var r = (from ca in AllContact select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.FacultyInternalCode, c.FacultyInternalName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in AllContact
                              select new { c.Id, c.FullContactDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            // return View();
        }

        /*------------------------------- ContactDetails Delete --------------------------------- */
        //[HttpPost]
        //public ActionResult ContactDetailsRemove(int? data, int? forwarddata)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        ContactDetails contDet = db.ContactDetails.Find(data);
        //       // FacultyInternal loc = db.FacultyInternal.Find(forwarddata);
        //        try
        //        {
        //            using (TransactionScope ts = new TransactionScope())
        //            {
        //                loc.ContactDetails = null;
        //                db.Entry(loc).State = System.Data.Entity.EntityState.Modified;
        //                db.SaveChanges();
        //                ts.Complete();
        //            }

        //            return Json(new Object[] { null, "Data Remove successfully." }, JsonRequestBehavior.AllowGet);
        //        }

        //        catch (DataException /* dex */)
        //        {

        //            return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });

        //        }
        //    }
        //}

        /*--------------------------------------------- Lookup Details Address --------------------------------------- */

        [HttpPost]
        public ActionResult GetLookupDetailsAddress(string data, string EmpCode)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Address> AllAddress = new List<Address>();

                if (EmpCode != "")
                {

                    var fall1 = db.Employee.Include(e => e.PerAddr)
                        .Include(e => e.PerAddr.Country).Include(e => e.PerAddr.State).Include(e => e.PerAddr.StateRegion)
                        .Include(e => e.PerAddr.District).Include(e => e.PerAddr.Taluka).Include(e => e.PerAddr.City).Include(e => e.PerAddr.Area).Where(e => e.EmpCode == EmpCode).FirstOrDefault().PerAddr;

                    var fall2 = db.Employee.Include(e => e.CorAddr)
                        .Include(e => e.CorAddr.Country).Include(e => e.CorAddr.State).Include(e => e.CorAddr.StateRegion)
                        .Include(e => e.CorAddr.District).Include(e => e.CorAddr.Taluka).Include(e => e.CorAddr.City).Include(e => e.CorAddr.Area).Where(e => e.EmpCode == EmpCode).FirstOrDefault().CorAddr;

                    var fall3 = db.Employee.Include(e => e.ResAddr)
                        .Include(e => e.ResAddr.Country).Include(e => e.ResAddr.State).Include(e => e.ResAddr.StateRegion)
                        .Include(e => e.ResAddr.District).Include(e => e.ResAddr.Taluka).Include(e => e.ResAddr.City).Include(e => e.ResAddr.Area).Where(e => e.EmpCode == EmpCode).FirstOrDefault().PerAddr;


                    if (fall1 != null)
                    {
                        AllAddress.Add(fall1);
                    }
                    if (fall2 != null)
                    {
                        AllAddress.Add(fall2);
                    }
                    if (fall3 != null)
                    {
                        AllAddress.Add(fall3);
                    }
                }
                IEnumerable<Address> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Address.ToList().Where(d => d.FullAddress.Contains(data));

                }
                else
                {
                    var r = (from ca in AllAddress select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in AllAddress
                              select new { c.Id, c.Address3 }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult GetLookupTrinstDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.FacultyInternalExternal.Include(e => e.Address).Include(e => e.ContactDetails).Include(e => e.FacultyType)
                 .ToList();
                IEnumerable<FacultyInternalExternal> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.FacultyInternalExternal.ToList().Where(d => d.FullDetails.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetLookupEmployeeDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Employee.ToList();
                IEnumerable<Employee> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Employee.ToList().Where(d => d.EmpCode.Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.EmpCode }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.EmpCode }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult Editcontactdetails_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var r = (from ca in db.ContactDetails
                         .Where(e => e.Id == data)
                         select new
                         {
                             Id = ca.Id,
                             EmailId = ca.EmailId,
                             FaxNo = ca.FaxNo,
                             Website = ca.Website
                         }).ToList();

                var a = db.ContactDetails.Include(e => e.ContactNumbers).Where(e => e.Id == data).SingleOrDefault();
                var b = a.ContactNumbers;

                var r1 = (from s in b
                          select new
                          {
                              Id = s.Id,
                              FullContactNumbers = s.FullContactNumbers
                          }).ToList();

                TempData["RowVersion"] = db.ContactDetails.Find(data).RowVersion;
                return Json(new object[] { r, r1 }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Createcontactdetails_partial()
        {
            return View("~/Views/Shared/Core/_Contactdetails.cshtml");
        }

        public ActionResult GetLookupDetailsProgramList(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var fall = db.SubCategory.Include(a => a.ProgramList).SelectMany(a => a.ProgramList).ToList();
                var fall = db.ProgramList.ToList();
                IEnumerable<ProgramList> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.ProgramList.ToList();

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }

    }
}